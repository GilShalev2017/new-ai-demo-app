using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Server.Models
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(TranscriptionInsight), typeof(SummaryInsight))]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "_type")]
    [JsonDerivedType(typeof(TranscriptionInsight), "transcription")]
    [JsonDerivedType(typeof(SummaryInsight), "summary")]
    public abstract class Insight
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public required InsightTypes InsightType { get; init; }

        public required string ProviderName { get; init; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

    public sealed class TranscriptionInsight : Insight
    {
        public required List<Transcript> Transcripts { get; init; }

        public required string AudioLanguage { get; init; }
    }

    public sealed class SummaryInsight : Insight
    {
        public required string Summary { get; init; }
    }

}
