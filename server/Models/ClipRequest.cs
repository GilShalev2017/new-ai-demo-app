using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ClipRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [BsonElement("channelIds")]
        public List<string> ChannelIds { get; set; } = new();

        [BsonElement("fromTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FromTime { get; set; }

        [BsonElement("toTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ToTime { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("processAI")]
        public bool ProcessAI { get; set; } = true;

        [BsonElement("status")]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = "System";

        [BsonElement("errorMessage")]
        public string? ErrorMessage { get; set; }

        // Navigation - List of clip IDs created from this request
        [BsonElement("clipIds")]
        public List<string> ClipIds { get; set; } = new();
    }

    public enum RequestStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        PartiallyCompleted = 4
    }
}
