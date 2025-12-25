using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class CreateClipsPayloadDto
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public List<string> ChannelIds { get; set; } = new();

        [Required]
        public DateTime FromTime { get; set; }

        [Required]
        public DateTime ToTime { get; set; }

        public List<string>? Tags { get; set; }

        public bool ProcessAI { get; set; } = true;

        public string VideoFileName { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string ClipRequestId { get; set; } = string.Empty;
    }

    public class UpdateClipDto
    {
        public string? Title { get; set; }
        public List<string>? Tags { get; set; }
        public string? Transcription { get; set; }
    }
}
