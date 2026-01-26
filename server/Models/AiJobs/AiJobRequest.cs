using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Models.AiJobs.Server.Models.AiJobs;
using System.Data;

namespace Server.Models.AiJobs
{
    public static class JobStatus
    {
        public const string Pending = "Pending";
        public const string InProgress = "In Progress";
        public const string Paused = "Paused";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Stopped = "Stopped";
    }

    [BsonIgnoreExtraElements]
    public class AiJobRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public required string Name { get; set; }
        public List<int> ChannelIds { get; set; } = new List<int>();
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime BroadcastStartTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime BroadcastEndTime { get; set; }
        public Server.Models.AiJobs.Rule? RequestRule { get; set; }
        public List<string>? Keywords { get; set; } = new List<string>();
        public List<string>? KeywordsLangauges { get; set; } = new List<string>();
        public required List<string> Operations { get; set; } = new List<string>();
        public string? ExpectedAudioLanguage { get; set; }
        /// <summary>
        /// make sure that this list contains the English name of the languages. This is what we use internally to identify a language.
        /// </summary>
        public List<string>? TranslationLanguages { get; set; }
        public List<string> NotificationIds { get; set; } = new List<string>();
        public string? Status { get; set; } //pending, in progress, paused, completed, failed
        public string? Error { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? NextScheduledTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<RunHistoryEntry> RunHistory { get; set; } = new List<RunHistoryEntry>();
        public bool CanBePausedResumed()
        {
            if (IsContinuousJob())
                return true;

            if (IsRecurring())
                return true;

            return false;
        }
        public bool IsContinuousJob()
        {
            if (RequestRule != null && RequestRule.Recurrence == RuleRecurrenceEnum.Continuous)
                return true;

            return false;
        }
        public bool IsRecurring()
        {
            if (RequestRule != null && RequestRule.Recurrence == RuleRecurrenceEnum.Recurring)
                return true;

            return false;
        }
    }
}
