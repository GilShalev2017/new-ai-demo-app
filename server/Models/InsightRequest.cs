using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public enum InsightTypes
    {
        Transcription,
        Translation,
        Summary,
        FaceDetection
    }

    public class InsightRequest
    {
        public InsightTypes InsightType { get; set; }
        public string? SourceLanguage { get; set; }        // "he", "en", null = auto
        public string? TargetLanguage { get; set; }        // used later for translation


        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? Id { get; set; }

        //public InsightTypes InsightType { get; set; } 

        //[JsonIgnore]
        //[BsonIgnore]
        //public string? AiClipId { get; set; }

        //public string? SourceLanguage { get; set; }
        //public List<KeyValuePair<string, string>> AIParameters { get; set; } = new();

/*
        /// <summary>
        /// // some insights depend on another insights as their sources. 
        /// </summary>
        public string? DependsOnInsightId { get; set; } // some insights depend on another insight as their sources. 

        public string? DependsOnInsightType { get; set; }

        public ProgressEnum Progress { get; set; } = ProgressEnum.New;

        public string? Status { get; set; } // some enum ? 

        //public InsightResult? Result { get; set; }
        public List<InsightResult> Results { get; set; } = new List<InsightResult>();

        public string? StatusDetails { get; set; }

        /// <summary>
        /// list with all the languages that this insight should be translated into.
        /// </summary>
        public List<TranslateRequest>? TranslateRequests { get; set; }
        [JsonIgnore]
        public List<InsightRequest>? TranslateInsightRequests { get; set; }

        public void AddOrUpdateAIParameter(string key, string val)
        {
            var kvParam = AIParameters.Find(kvp => kvp.Key == key);
            if (!string.IsNullOrEmpty(kvParam.Key))
                AIParameters.Remove(kvParam);

            AIParameters.Add(new KeyValuePair<string, string>(key, val));
        }

        public string? GetValueOfAIParam(string key)
        {
            var kvParam = AIParameters.Find(kvp => kvp.Key == key);
            return kvParam.Value;
        }
*/
    }
}
