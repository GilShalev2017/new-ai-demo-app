namespace Server.Models
{
    public class ClipSearchRequest
    {
        public string? SearchTerm { get; set; }
        public bool SearchOperandAnd { get; set; } = false;
        public string[]? Tags { get; set; }
        public string[]? ChannelIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SortOption { get; set; } // 0=Newest, 1=Oldest, 2=A-Z, etc.
        public int Limit { get; set; } = 100;
        public int Skip { get; set; } = 0;
    }

    public class ClipSearchResponse
    {
        public List<Clip> Clips { get; set; } = new();
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
    }
}
