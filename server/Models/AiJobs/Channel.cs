namespace Server.Models.AiJobs
{
    public class Channel
    {
        public int Id { get; set; } = 0;
        public string InternalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ServerId { get; set; } = 0;
        public string RecordingRoot { get; set; } = string.Empty;
    }
}
