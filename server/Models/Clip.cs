using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Clip
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [BsonElement("duration")]
        public int Duration { get; set; }

        [BsonElement("videoUrl")]
        public string VideoUrl { get; set; } = string.Empty;

        [BsonElement("thumbnailUrl")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        // Single channel per clip
        [BsonElement("channelName")]
        public string ChannelName { get; set; } = string.Empty;

        [BsonElement("channelId")]
        public string ChannelId { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("transcription")]
        public string? Transcription { get; set; }

        [BsonElement("sentiment")]
        public SentimentData? Sentiment { get; set; }

        [BsonElement("isProcessed")]
        public bool IsProcessed { get; set; } = false;

        [BsonElement("fileSize")]
        public long FileSize { get; set; }

        [BsonElement("resolution")]
        public string Resolution { get; set; } = "1920x1080";

        [BsonElement("fps")]
        public int Fps { get; set; } = 30;

        // Reference back to the request that created this clip
        [BsonElement("clipRequestId")]
        public string? ClipRequestId { get; set; }

        // Single video file path (no duplicates)
        [BsonElement("videoFileName")]
        public string VideoFileName { get; set; } = string.Empty;

        public List<Insight> Insights { get; set; } = new List<Insight>();

        public TInsight? GetInsight<TInsight>()  where TInsight : Insight =>  Insights.OfType<TInsight>().FirstOrDefault();

        public void AddOrReplaceInsight(Insight insight)
        {
            var existing = Insights.FirstOrDefault(i => i.InsightType == insight.InsightType);
            if (existing != null)
            {
                Insights.Remove(existing);
            }

            Insights.Add(insight);
        }
    }

    public class SentimentData
    {
        [BsonElement("positive")]
        public double Positive { get; set; }

        [BsonElement("neutral")]
        public double Neutral { get; set; }

        [BsonElement("negative")]
        public double Negative { get; set; }

        [BsonElement("overallScore")]
        public double OverallScore { get; set; }
    }

}
