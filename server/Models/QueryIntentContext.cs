namespace Server.Models
{
    public class QueryIntentContext
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsTimeCodeNeeded { get; set; }
        public List<string> Channels { get; set; } = new();
        public string OriginalQuery { get; set; } = string.Empty;
        public string RawJsonResponse { get; set; } = string.Empty;
    }

}
