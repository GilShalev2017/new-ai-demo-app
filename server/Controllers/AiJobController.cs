using Microsoft.AspNetCore.Mvc;
using Server.Models.AiJobs;
using Server.Services.AiJobs;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiJobController : ControllerBase
    {
        private readonly ILogger<AiJobController> _logger;
        private readonly IAiJobSvc _aiJobService;

        public AiJobController(ILogger<AiJobController> logger, IAiJobSvc aiIJobService)
        {
            _logger = logger;
            _aiJobService = aiIJobService;
        }

        [HttpGet("hello")]
        public string Get()
        {
            return "Hello from AiJobController Controller";
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleJob([FromBody] AiJobRequest jobRequest)
        {
            //TODO replace IActionResult with AiJobDm
            //TODO validate request
            if (jobRequest == null)
            {
                _logger.LogError("JobRequest is null.");

                return BadRequest("JobRequest cannot be null.");
            }

            PrintJobRequest(jobRequest);

            var jobResponse = await _aiJobService.ScheduleJobAsync(jobRequest);

            if (jobResponse.Status == "Success")
            {
                return Ok(new { Message = "Added JobRequest Successfully", JobRequest = jobRequest });
            }

            return BadRequest(jobResponse.Errors);
        }

        [HttpGet]
        public async Task<List<AiJobRequest>> GetAllJobRequestsAsync()
        {
            var jobs = await _aiJobService.GetAllJobRequestsAsync();

            return jobs;
        }

        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(string jobId, [FromQuery] bool onlyTheJob)
        {
            //var actuser = ActUser.DecodeFromHeader(Request)!;
            //await _aiClipManagerService.CheckACLOrThrowAsync(id, actuser, ACL.Rights.FullAccess);
            //AIClipDm aiclip = await GetByIdAsync(id);
            await _aiJobService.DeleteJobAsync(jobId, onlyTheJob);
            //_ = _activityLogConnector.WriteAsync(actuser, "AI Clip Deleted", $"Name: {aiclip.Name}");
            return Ok();
        }

        [HttpPut("pauseJob/{jobId}")]
        public async Task<IActionResult> PauseJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                _logger.LogError("PauseJob: jobId is null or empty.");

                return BadRequest("Job ID cannot be null or empty.");
            }

            var job = await _aiJobService.GetJobByIdAsync(jobId);

            if (job == null)
            {
                _logger.LogError($"PauseJob: Job with ID {jobId} not found.");

                return NotFound($"Job with ID {jobId} not found.");
            }

            if (!job.CanBePausedResumed())
            {
                _logger.LogWarning($"PauseJob: Attempted to pause a non-recurring job (ID: {jobId}).");

                return BadRequest("Only recurring jobs can be paused.");
            }

            bool isPaused = await _aiJobService.PauseJobAsync(job);

            if (!isPaused)
            {
                _logger.LogWarning($"PauseJob: Job {jobId} could not be paused (possibly already paused or completed).");

                return BadRequest($"Job {jobId} could not be paused.");
            }

            _logger.LogInformation($"Job {jobId} has been successfully paused.");

            return Ok(new { Message = $"Job {jobId} has been paused." });
        }


        [HttpPut("resumeJob/{jobId}")]
        public async Task<IActionResult> ResumeJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                _logger.LogError("ResumeJob: jobId is null or empty.");

                return BadRequest("Job ID cannot be null or empty.");
            }

            var job = await _aiJobService.GetJobByIdAsync(jobId);

            if (job == null)
            {
                _logger.LogError($"ResumeJob: Job with ID {jobId} not found.");

                return NotFound($"Job with ID {jobId} not found.");
            }

            if (!job.IsContinuousJob() && !job.IsRecurring())
            {
                _logger.LogWarning($"ResumeJob: Attempted to resume a non-ContinuousJob or a non-recurring job (ID: {jobId}).");

                return BadRequest("Only Recurring or Continuous jobs can be resumed.");
            }

            await _aiJobService.ResumeJobAsync(job);

            _logger.LogInformation($"Job {jobId} has been resumed.");

            return Ok(new { Message = $"Job {jobId} has been resumed." });
        }


        [HttpPut("stopJob/{jobId}")]
        public async Task<IActionResult> StopJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                _logger.LogError("StopJob: jobId is null or empty.");

                return BadRequest("Job ID cannot be null or empty.");
            }

            var job = await _aiJobService.GetJobByIdAsync(jobId);

            if (job == null)
            {
                _logger.LogError($"StopJob: Job with ID {jobId} not found.");

                return NotFound($"Job with ID {jobId} not found.");
            }

            if (job.Status == JobStatus.Stopped)
            {
                _logger.LogWarning($"StopJob: Job {jobId} is already stopped.");

                return BadRequest($"Job {jobId} is already stopped.");
            }

            if (job.Status == JobStatus.Completed)
            {
                _logger.LogWarning($"StopJob: Cannot stop a completed job (ID: {jobId}).");

                return BadRequest("Cannot stop a completed job.");
            }

            bool isStopped = await _aiJobService.StopJobAsync(job);

            if (!isStopped)
            {
                _logger.LogWarning($"StopJob: Job {jobId} could not be stopped (possibly already stopped or in an invalid state).");

                return BadRequest($"Job {jobId} could not be stopped.");
            }

            _logger.LogInformation($"Job {jobId} has been successfully stopped.");

            return Ok(new { Message = $"Job {jobId} has been stopped." });
        }


        [HttpPost("ai-job-requests")]
        public async Task<List<AiJobRequest>> GetFilteredJobRequests([FromBody] JobRequestFilter filter)
        {
            return await _aiJobService.GetFilteredJobRequests(filter);
        }

        [HttpPost("alerts")]
        public async Task<List<AIDetection>> GetAIDetectionsResults([FromBody] JobResultFilter filter)
        {
            _logger.LogDebug("Received request to get AIDetections with payload: {@filter}", filter);

            return await _aiJobService.GetFilteredAIDetections(filter);
        }

        [HttpPost("distinctAlerts")]
        public async Task<List<string>?> GetDistinctAIDetectionsAsync([FromBody] JobResultFilter filter)
        {
            var aiDetections = await _aiJobService.GetDistinctAIDetectionsAsync(filter);

            return aiDetections;
        }

        private void PrintJobRequest(AiJobRequest jobRequest)
        {
            string logMessage = $"Received JobRequest:\n" +
                                $"- Name: {jobRequest.Name}\n" +
                                $"- BraodcastStartTime: {jobRequest.BroadcastStartTime}\n" +
                                $"- BroadcastEndTime: {jobRequest.BroadcastEndTime}\n" +
                                $"- Keywords: {string.Join(", ", jobRequest.Keywords ?? new List<string>())}\n" +
                                $"- Keywords Languages: {string.Join(", ", jobRequest.KeywordsLangauges ?? new List<string>())}\n" +
                                $"- Operations: {string.Join(", ", jobRequest.Operations ?? new List<string>())}\n" +
                                $"- ExpectedAudioLanguage: {jobRequest.ExpectedAudioLanguage}\n" +
                                $"- TranslationLanguages: {string.Join(", ", jobRequest.TranslationLanguages ?? new List<string>())}";

            _logger.LogDebug(logMessage);
        }
    }

}
