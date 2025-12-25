namespace WhisperService.Models
{
    public class CaptionsRequest
    {
        public string? AudioFileName { get; set; }
        public bool UseTranslate { get; set; }
        public string? ModelType { get; set; }
    }
}
