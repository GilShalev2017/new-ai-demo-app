using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    public class JobResultFilter
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? End { get; set; }
        public int[]? ChannelIds { get; set; }
        public string? Operation { get; set; }
        public string[]? Keywords { get; set; }
        public string? AiJobRequestId { get; set; }
        public int? SortDirection { get; set; }
    }
}
