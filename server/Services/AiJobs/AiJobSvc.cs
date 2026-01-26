using Server.Models.AiJobs;
using Server.Repositories.AiJobs;

namespace Server.Services.AiJobs
{
    public interface IAiJobSvc
    {
        Task<AiJobRequest?> GetJobByIdAsync(string jobId);
        Task<AiJobResponse> ScheduleJobAsync(AiJobRequest jobRequest);
        Task<List<AiJobRequest>> GetAllJobRequestsAsync();
        Task<bool> DeleteJobAsync(string jobId, bool onlyTheJob);
        Task<bool> PauseJobAsync(AiJobRequest jobRequest);
        Task<bool> ResumeJobAsync(AiJobRequest jobRequest);
        Task<bool> StopJobAsync(AiJobRequest jobRequest);
        Task<List<AIDetection>> GetFilteredAIDetections(JobResultFilter filter);
        Task<List<AiJobRequest>> GetFilteredJobRequests(JobRequestFilter filter);
        Task<List<string>?> GetDistinctAIDetectionsAsync(JobResultFilter filter);
    }

    public class AiJobSvc : IAiJobSvc
    {
        private readonly AiJobSchedulerSvc _aiJobSchedulerSvc;
        protected readonly IAiJobResultRepository _aiJobResultRepository;
        private readonly IAiJobRequestRepository _aiJobRequestRepository;
        protected ILogger<IAiJobSvc> _logger;

        public AiJobSvc(ILogger<IAiJobSvc> logger, AiJobSchedulerSvc aiJobSchedulerSvc, IAiJobResultRepository aiJobResultRepository, IAiJobRequestRepository aiJobRequestRepository)
        {
            _logger = logger;
            _aiJobSchedulerSvc = aiJobSchedulerSvc;
            _aiJobResultRepository = aiJobResultRepository;
            _aiJobRequestRepository = aiJobRequestRepository;
        }
        public async Task<AiJobResponse> ScheduleJobAsync(AiJobRequest jobRequest)
        {
            try
            {
                await _aiJobSchedulerSvc.ScheduleJobAsync(jobRequest);
            }
            catch (Exception ex)
            {
                return new AiJobResponse { JobRequest = jobRequest, Status = "Error", Errors = new List<string> { ex.Message } };
            }

            return new AiJobResponse { JobRequest = jobRequest, Status = "Success" };
        }
        public async Task<List<AiJobRequest>> GetAllJobRequestsAsync()
        {
            return await _aiJobSchedulerSvc.GetAllJobRequestsAsync();
        }
        public virtual async Task<bool> DeleteJobAsync(string jobId, bool onlyTheJob)
        {
            await _aiJobSchedulerSvc.DeleteJobAsync(jobId);

            if (!onlyTheJob)
            {
                await _aiJobResultRepository.DeleteJobRelatedResultsAsync(jobId);
            }

            return true;
        }
        public async Task<bool> PauseJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                throw new ArgumentNullException(nameof(jobRequest), "Job request cannot be null.");
            }

            return await _aiJobSchedulerSvc.PauseJobAsync(jobRequest);
        }
        public async Task<bool> ResumeJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                throw new ArgumentNullException(nameof(jobRequest), "Job request cannot be null.");
            }

            return await _aiJobSchedulerSvc.ResumeJobAsync(jobRequest);
        }
        public async Task<bool> StopJobAsync(AiJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                throw new ArgumentNullException(nameof(jobRequest), "Job request cannot be null.");
            }

            return await _aiJobSchedulerSvc.StopJobAsync(jobRequest);
        }
        public async Task<List<AIDetection>> GetFilteredAIDetections(JobResultFilter filter)
        {
            return await _aiJobResultRepository.GetFilteredAIDetectionsAsync(filter);
        }
        public async Task<List<AiJobRequest>> GetFilteredJobRequests(JobRequestFilter filter)
        {
            return await _aiJobRequestRepository.GetFilteredJobRequestsAsync(filter);
        }
        public async Task<List<string>?> GetDistinctAIDetectionsAsync(JobResultFilter filter)
        {
            return await _aiJobResultRepository.GetDistinctAIDetectedKeywordsAsync(filter);
        }
        public async Task<AiJobRequest?> GetJobByIdAsync(string jobId)
        {
            return await _aiJobRequestRepository.GetJobByIdAsync(jobId);
        }
    }
}
