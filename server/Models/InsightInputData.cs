namespace Server.Models
{
    public class VideoDTO
    {
        public string FilePath { get; set; } = null!;
        public double DurationSec { get; set; }
        public long SizeB { get; set; }
    }

    public class AudioDTO
    {
        public string FilePath { get; set; } = null!;
        public double DurationSec { get; set; }
        public string? AudioLanguage { get; set; } = null!;
    }

    public class InsightInputData
    {
        public VideoDTO? VideoInput { get; set; }
        public AudioDTO? AudioInput { get; set; }
        public List<Transcript> Transcripts { get; set; } = new List<Transcript>();
        //public List<FramesDTO>? FramesInput { get; set; }
        //public InsightResult? SourceInsightInput { get; set; }
    }
}
