using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Server.Models.AiJobs
{
    [BsonIgnoreExtraElements]
    public class SettingsDm
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        //[JsonIgnore]
        //public ACL? Acl { get; set; }
        [JsonIgnore]
        public string? LastUpdatedBy { get; set; }
        [JsonIgnore]
        public string? LastUpdateDate { get; set; }

        public double Credit { get; set; } = 0;//credit units / cents for the entire Intelligence service

        public int AiJobLiveSegmentDurationSec { get; set; } = -1; // -1 means not set

        public int MultiViewerLiveOffsetSec { get; set; } = 30; // in seconds, default is 30, meaning 30 seconds behind live

        //public LicensePlateSettings LicensePlate { get; set; } = new LicensePlateSettings();
        //public CorsightFaceDetectionSettings CorsightFaceDetection { get; set; } = new CorsightFaceDetectionSettings();

        public int MaxFilesPerChannelToProcessForContinuousAiJob { get; set; } = 500000; //default to max 500000 mp4 files for a single run of a continuous job - PER CHANNEL
        public int CleanupOlderResultsForContinuousAiJobDays { get; set; } = 90;
    }
}
