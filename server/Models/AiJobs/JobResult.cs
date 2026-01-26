using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    [BsonIgnoreExtraElements]
    public class JobResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string AiJobRequestId { get; set; } = null!;
        public int ChannelId { get; set; } = 0;
        public string ChannelDisplayName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Operation { get; set; } = "";
        /// <summary>
        /// Name of the AI Engine that detected this object (e.g., Google, OpenALPR, Corsight)
        /// </summary>
        public string? AIEngine { get; set; }
        /// <summary>
        ///  Json encoded result ;the object will be specific to each AI Engine.
        /// </summary>
        public string? AIEngineResultRaw { get; set; } // json encoded result from the AI engine
        public List<TranscriptEx>? Content { get; set; }
        public List<BoundingBoxObject>? DetectedObjects { get; set; } = new();
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime End { get; set; }
        public string? FilePath { get; set; } //.mp4 or .mp3 or none
        public string? AudioLanguage { get; set; }
        public string? TranslationLanguage { get; set; }
        public ProviderType? ProviderType { get; set; }
        public string? TranscriptionJobResultId { get; set; }
    }
}
