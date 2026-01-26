using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    public class TimeRange
    {
        public int StartInSeconds { get; set; }
        public int EndInSeconds { get; set; }
    }
    public class TimeCodedItem
    {
        public string? ImageUrl { get; set; }
        public string Description { get; set; } = "";
        public List<TimeRange> TimeRanges { get; set; } = new List<TimeRange>();
        public string Name { get; set; } = "";
        public int AppearancePercentage { get; set; }
        public int TotalSeconds { get; set; } // TODO : remove this
    }

    [BsonIgnoreExtraElements]
    public class InsightResult
    {
        public List<Transcript>? TimeCodedContent { get; set; }
        public List<BoundingBoxDetection>? GenericObjectDetections { get; set; }
        public List<TimeCodedItem>? TimeCodedItems { get; set; }
        public List<Transcript>? TimeCodedContentWithSearchData { get; set; }//to be renamed! ! ! 
        public string SourceInsightType { get; set; } = null!;
        public string? AIProviderId { get; set; }
        public string AIEngineResultRaw { get; set; } = null!; // json encoded result from the AI engine
        public string Language { get; set; } = null!;
        /// <summary>
        /// LanguageSufix set by each provider based on it's name and Insight type. This will be used to create the correct language description in mysql
        /// </summary>
        [BsonIgnore]
        public string LanguageSufix { get; set; } = "Auto-Generated";
        public string? Prompt { get; set; }
    }
}
