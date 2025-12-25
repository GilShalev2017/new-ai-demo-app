namespace WhisperService.Services
{
    public class TranscriptionStateService
    {
        public List<string> ProcessedFiles { get; } = new List<string>();
        public List<string> FailedTranscriptions { get; } = new List<string>();
    }
}
