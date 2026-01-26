namespace Server.Models.AiJobs
{
    public class AiJobResponse
    {
        public string? JobId { get; set; }

        public required AiJobRequest JobRequest { get; set; }

        public string? Status { get; set; }

        public List<string>? Errors { get; set; }
    }
}
