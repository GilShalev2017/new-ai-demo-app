namespace Server.DTOs
{
    public class EvidenceDto
    {
        public string ClipId { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;

        public DateTime? Timestamp { get; set; }

        public string Text { get; set; } = string.Empty;

        public float Score { get; set; }
    }

    public class SemanticSearchResponseDto
    {
        public string Answer { get; set; } = string.Empty;

        public List<EvidenceDto> Evidence { get; set; } = new();

        //public QueryDebugInfoDto? Debug { get; set; } // optional (dev mode)
    }

}
