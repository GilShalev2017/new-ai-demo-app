using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    public class AIDetection
    {
        public string? JobRequestId { get; set; }
        public int ChannelId { get; set; }
        public string ChannelDisplayName { get; set; } = "";
        public string Operation { get; set; } = "";
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime End { get; set; }
        public string Text { get; set; } = "";
        public string? Keyword { get; set; } = "";
    }
}
