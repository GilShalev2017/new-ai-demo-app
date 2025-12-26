using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Server.Models
{
    //[BsonDiscriminator(RootClass = true)]
    //[BsonKnownTypes(typeof(TranscriptionInsight), typeof(SummaryInsight))]
    //[JsonPolymorphic(TypeDiscriminatorPropertyName = "_type")]
    //[JsonDerivedType(typeof(TranscriptionInsight), "transcription")]
    //[JsonDerivedType(typeof(SummaryInsight), "summary")]

    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(TranscriptionInsight),typeof(SummaryInsight),typeof(ChatGPTPromptInsight))]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "_type")]
    [JsonDerivedType(typeof(TranscriptionInsight), "transcription")]
    [JsonDerivedType(typeof(SummaryInsight), "summary")]
    [JsonDerivedType(typeof(ChatGPTPromptInsight), "chatgptPrompt")]
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

    public sealed class ChatGPTPromptInsight : Insight
    {
        /// <summary>
        /// User-friendly name, e.g. "Find Locations", "Detect Celebrities"
        /// </summary>
        public required string PromptName { get; init; }

        /// <summary>
        /// The actual prompt text sent to ChatGPT
        /// </summary>
        public required string PromptText { get; init; }

        /// <summary>
        /// AI response
        /// </summary>
        public required string Result { get; init; }
    }

}
