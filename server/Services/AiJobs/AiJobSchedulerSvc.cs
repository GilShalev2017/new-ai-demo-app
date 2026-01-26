using Server.Models.AiJobs;
using Server.Models.AiJobs.Server.Models.AiJobs;
using Server.Repositories.AiJobs;
using System.Collections.Concurrent;

namespace Server.Services.AiJobs
{
    public class AiJobSchedulerSvc : BackgroundService
    {
        private readonly ILogger<AiJobSchedulerSvc> _logger;
        private readonly IAiJobRequestRepository _aiJobRequestRepository;
        private readonly IAiJobOperationsSvc _aiJobOperationsService;
        private readonly Timer _timer;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _scheduledExecutionRuns;

        public AiJobSchedulerSvc(ILogger<AiJobSchedulerSvc> logger, IAiJobRequestRepository aiJobRequestRepository, IAiJobResultRepository aiJobResultRepository, IAiJobOperationsSvc aiJobOperationsSvc)
        {
            _logger = logger;

            _aiJobRequestRepository = aiJobRequestRepository;

            _aiJobOperationsService = aiJobOperationsSvc;

            _scheduledExecutionRuns = new ConcurrentDictionary<string, CancellationTokenSource>();

            _timer = new Timer(CheckAndRunJobsAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Job Scheduler starting...");

            // Recover unfinished jobs asynchronously without blocking startup
            _ = RecoverInProgressJobs();

            _ = CleanOldAIJobResultsTaskSafeAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        /// <summary>
        /// this task is started after the servie starts and runs once a day.
        /// inside while will run every day to clean the results (older that X days) for all continuous jobs 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task CleanOldAIJobResultsTaskSafeAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _aiJobOperationsService.CleanOldAIJobResultsSafeAsync();

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CleanOldAIJobResultsTaskSafeAsync: {ex.Message}");
            }
        }

        public async Task<List<AiJobRequest>> GetAllJobRequestsAsync()
        {
            return await _aiJobRequestRepository.GetAllJobRequestsAsync();
        }

        public async Task ScheduleJobAsync(AiJobRequest jobRequest)
        {
            if (!jobRequest.IsContinuousJob())
            {
                await _aiJobRequestRepository.CreateJobAsync(jobRequest);
                CheckAndRunJobsAsync(null); // Immediately check for jobs to schedule

                return;
            }

            JobRequestFilter filter = new JobRequestFilter
            {
                Operations = jobRequest.Operations,
                RuleRecurrence = RuleRecurrenceEnum.Continuous
            };

            //for continuous job, allow a SINGLE continuous job for the a channel using a operations
            foreach (int channelId in jobRequest.ChannelIds)
            {
                filter.ChannelIds = new int[] { channelId };
                List<AiJobRequest> aiJobRequestAlreadyInDB = await _aiJobRequestRepository.GetFilteredJobRequestsAsync(filter);
                if (aiJobRequestAlreadyInDB.Count > 0)
                {
                    string err = $"We already have a ContinuousJob for channel id {channelId} for some/all of these operations: {string.Join(", ", jobRequest.Operations)}";
                    _logger.LogWarning($"AiJobSchedulerSvc.ScheduleJobAsync : {err}");
                    throw new Exception(err);
                }
            }

            //we can add this continuous job.
            await _aiJobRequestRepository.CreateJobAsync(jobRequest);
            CheckAndRunJobsAsync(null); // Immediately check for jobs to schedule
        }

        private async void CheckAndRunJobsAsync(object? state)
        {
            var jobs = await _aiJobRequestRepository.GetJobsByStatusAsync(JobStatus.Pending);

            foreach (var job in jobs)
            {
                if (_scheduledExecutionRuns.ContainsKey(job.Id!))
                {
                    continue;// Skip if the job is already scheduled or running
                }

                TimeSpan? delay = job.NextScheduledTime - DateTime.Now; // Calculate delay as a TimeSpan

                var cts = new CancellationTokenSource();

                _scheduledExecutionRuns.TryAdd(job.Id!, cts); // Mark the job as "Pending" and prepare for execution

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay((TimeSpan)delay, cts.Token);// Wait until the scheduled time, respect cancellation
                        }

                        if (!cts.Token.IsCancellationRequested)
                        {
                            await ExecuteJobAsync(job, cts.Token); // Execute job at precise time
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation($"Job {job.Id} was canceled before execution.");
                    }
                    finally
                    {
                        _scheduledExecutionRuns.TryRemove(job.Id!, out _);  // Remove after completion or cancellation
                    }
                });
            }
        }

        private async Task ExecuteJobAsync(AiJobRequest job, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateJobStatus(job, JobStatus.InProgress);

                await _aiJobOperationsService.ExecuteJobAsync(job, token);

                token.ThrowIfCancellationRequested();

                await UpdateJobStatus(job, JobStatus.Completed);

                if (job.IsRecurring())
                {
                    ScheduleNextOccurrence(job);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"ExecuteJobAsync: Job {job.Id} was canceled.");
            }
            catch (Exception ex)
            {
                string error = $"ExecuteJobAsync failed: {ex.Message} stack: {ex.StackTrace}";

                _logger.LogError(error);

                await UpdateJobStatus(job, JobStatus.Failed, error);
            }
        }

        private async Task UpdateJobStatus(AiJobRequest job, string status, string error = "")
        {
            await _aiJobRequestRepository.UpdateJobStatusAsync(job, status, error);
        }

        private async void ScheduleNextOccurrence(AiJobRequest job)
        {
            DateTime nextOccurrence = CalculateNextScheduledTime(job); // Calculate the next occurrence based on the recurrence rule.

            if (nextOccurrence != DateTime.MinValue)
            {
                job.NextScheduledTime = nextOccurrence;

                job.Status = JobStatus.Pending;

                await _aiJobRequestRepository.ScheduleNextOccurrenceAsync(job); // Update the job in the repository.

                _logger.LogDebug($"Next occurrence for job {job.Id} scheduled at {nextOccurrence}");
            }
            else
            {
                _logger.LogWarning($"Failed to calculate the next occurrence for job {job.Id}");
            }
        }

        private DateTime CalculateNextScheduledTime(AiJobRequest job)
        {
            // Start with the current NextScheduledTime or the broadcast start time
            DateTime baseTime = job.NextScheduledTime ?? job.BroadcastStartTime;

            // Handle recurrence based on the rule
            for (int i = 1; i < 7; i++) // Loop through the next 7 days
            {
                DateTime potentialDate = baseTime.AddDays(i);

                if (potentialDate == job.NextScheduledTime) // Skip if the potential date is the same as the current nextScheduledTime
                {
                    continue;
                }

                // Check if the potential date matches the rule
                if (job.RequestRule!.IsActiveOnDay(potentialDate))
                {
                    // Use the time component from `baseTime` to maintain the intended schedule time
                    DateTime nextScheduledDate = new DateTime(
                        potentialDate.Year,
                        potentialDate.Month,
                        potentialDate.Day,
                        baseTime.Hour,
                        baseTime.Minute,
                        baseTime.Second);

                    return nextScheduledDate;
                }
            }

            // If no valid date is found (which shouldn't happen), throw an exception or handle the edge case
            throw new InvalidOperationException("Could not calculate the next scheduled time within the given range.");
        }

        private async Task RecoverInProgressJobs()
        {
            var inProgressJobs = await _aiJobRequestRepository.GetJobsByStatusAsync(JobStatus.InProgress);

            foreach (var job in inProgressJobs)
            {
                var cts = new CancellationTokenSource();

                if (_scheduledExecutionRuns.TryAdd(job.Id!, cts))
                {
                    try
                    {
                        await ExecuteJobAsync(job, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation($"RecoverInProgressJobs: Job {job.Id} was canceled during recovery.");
                    }
                    finally
                    {
                        _scheduledExecutionRuns.TryRemove(job.Id!, out _);

                        cts.Dispose(); // Clean up resources
                    }
                }
            }
        }

        public async Task DeleteJobAsync(string jobId)
        {
            await _aiJobRequestRepository.DeleteJobAsync(jobId); // Delete the job and its associated results
        }

        public async Task<bool> PauseJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                _logger.LogError("PauseJobAsync: Job request is null.");

                return false;
            }

            if (jobRequest.Status == JobStatus.Paused)
            {
                _logger.LogWarning($"PauseJobAsync: Job {jobRequest.Id} is already paused.");

                return false;
            }

            if (jobRequest.Status != JobStatus.Pending && jobRequest.Status != JobStatus.InProgress)
            {
                _logger.LogWarning($"PauseJobAsync: Cannot pause job {jobRequest.Id} as it is in '{jobRequest.Status}' state.");

                return false;
            }

            if (_scheduledExecutionRuns.TryRemove(jobRequest.Id!, out var cts))
            {
                cts.Cancel(); // Cancel the scheduled task

                cts.Dispose(); // Clean up resources
            }

            jobRequest.Status = JobStatus.Paused;

            await _aiJobRequestRepository.UpdateJobStatusAsync(jobRequest, JobStatus.Paused);

            _logger.LogInformation($"PauseJobAsync: Job {jobRequest.Id} successfully paused.");

            return true;
        }

        public async Task<bool> ResumeJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                _logger.LogError("ResumeJobAsync: Job request is null.");

                return false;
            }


            if (!jobRequest.IsContinuousJob() && jobRequest.Status != JobStatus.Paused)
            {
                _logger.LogWarning($"ResumeJobAsync: Job {jobRequest.Id} is not in a paused state. Current status: {jobRequest.Status}");

                return false;
            }

            jobRequest.Status = JobStatus.Pending;

            await _aiJobRequestRepository.UpdateJobStatusAsync(jobRequest, JobStatus.Pending);

            _logger.LogInformation($"ResumeJobAsync: Job {jobRequest.Id} has been resumed and set to pending.");

            CheckAndRunJobsAsync(null); // Immediately check for jobs to schedule

            return true;
        }

        public async Task<bool> StopJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                _logger.LogError("StopJobAsync: Job request is null.");

                return false;
            }

            if (jobRequest.Status == JobStatus.Stopped)
            {
                _logger.LogWarning($"StopJobAsync: Job {jobRequest.Id} is already stopped.");

                return false;
            }

            if (jobRequest.Status == JobStatus.Completed)
            {
                _logger.LogWarning($"StopJobAsync: Cannot stop job {jobRequest.Id} as it is already completed.");

                return false;
            }

            if (jobRequest.Status != JobStatus.Pending && jobRequest.Status != JobStatus.InProgress && jobRequest.Status != JobStatus.Paused)
            {
                _logger.LogWarning($"StopJobAsync: Cannot stop job {jobRequest.Id} as it is in '{jobRequest.Status}' state.");

                return false;
            }

            if (_scheduledExecutionRuns.TryRemove(jobRequest.Id!, out var cts))
            {
                cts.Cancel(); // Cancel the scheduled task

                cts.Dispose(); // Clean up resources
            }

            jobRequest.Status = JobStatus.Stopped;

            await _aiJobRequestRepository.UpdateJobStatusAsync(jobRequest, JobStatus.Stopped);

            _logger.LogInformation($"StopJobAsync: Job {jobRequest.Id} successfully stopped.");

            return true;
        }
    }
}
