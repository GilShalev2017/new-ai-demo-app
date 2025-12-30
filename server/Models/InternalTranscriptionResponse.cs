namespace Server.Models
{
    public class InternalTranscriptionResponse
    {
        public List<TranscriptEx>? Transcripts { get; set; }

        public string? DetectedLanguage { get; set; }

        public string? Language { get; set; }

        public string? Text { get; set; }
    }
}
