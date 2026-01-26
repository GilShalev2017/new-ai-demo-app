using MongoDB.Bson.Serialization.Attributes;
using Server.Models.AiJobs.Server.Models.AiJobs;

namespace Server.Models.AiJobs
{
    public class JobRequestFilter
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? End { get; set; }
        public int[]? ChannelIds { get; set; }
        public int? SortDirection { get; set; }
        public RuleRecurrenceEnum? RuleRecurrence { get; set; }
        public List<string>? Operations { get; set; }
        //public string? Operation { get; set; }
        //public string[]? Keywords { get; set; }
        //public string? AiJobRequestId { get; set; } 
        //public string? SearchTerm { get; set; }
    }
}
