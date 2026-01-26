using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.AiJobs
{
    public class ChannelResultStatistics
    {
        private readonly object _errorsLock = new();
        private readonly object _distinctAudioLanguagesLock = new();
        private readonly object _distinctTranslatedLanguagesLock = new();
        private readonly object _keywordsDetectionsFoundLock = new();

        public List<string> DistinctAudioLanguages { get; set; } = new List<string>();
        public List<string> DistinctTranslatedLanguages { get; set; } = new List<string>();

        private int _mp4FilesProcessed;

        [BsonElement("Mp4FilesProcessed")]
        public int Mp4FilesProcessed
        {
            get => _mp4FilesProcessed;
            set => _mp4FilesProcessed = value;
        }
        public void IncrementMp4FilesProcessed()
        {
            Interlocked.Increment(ref _mp4FilesProcessed);
        }

        private int _mp3FilesCreated;

        [BsonElement("Mp3FilesCreated")]
        public int Mp3FilesCreated
        {
            get => _mp3FilesCreated;
            set => _mp3FilesCreated = value;
        }
        public void IncrementMp3FilesCreated()
        {
            Interlocked.Increment(ref _mp3FilesCreated);
        }
       
        private int _keywordDetectedAlertsSent;

        [BsonElement("KeywordDetectedAlertsSent")]
        public int KeywordDetectedAlertsSent
        {
            get => _keywordDetectedAlertsSent;
            set => _keywordDetectedAlertsSent = value;
        }
        public void IncrementKeywordDetectedAlertsSent()
        {
            Interlocked.Increment(ref _keywordDetectedAlertsSent);
        }
        public List<string> KeywordsDetectionsFound { get; set; } = new List<string>();
        public int? FaceDetectedAlertSent { get; set; } = 0;
        public List<string>? FaceDetectionsFound { get; set; } = new List<string>();
        public int? LogoDetectedAlertSent { get; set; } = 0;
        public List<string>? LogoDetectionsFound { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(string error)
        {
            lock (_errorsLock)
            {
                if (!Errors.Contains(error))
                    Errors.Add(error);
            }
        }

        public void AddDistinctAudioLanguages(string language)
        {
            lock (_distinctAudioLanguagesLock)
            {
                if (DistinctAudioLanguages.Contains(language))
                    return;

                DistinctAudioLanguages.Add(language);
            }
        }

        public void AddDistinctTranslatedLanguages(string language)
        {
            lock (_distinctTranslatedLanguagesLock)
            {
                if (DistinctTranslatedLanguages.Contains(language))
                    return;

                DistinctTranslatedLanguages.Add(language);
            }
        }

        public void AddDistinctDetectedKW(string kw)
        {
            lock (_keywordsDetectionsFoundLock)
            {
                if (KeywordsDetectionsFound.Contains(kw))
                    return;

                KeywordsDetectionsFound.Add(kw);
            }
        }
    }
    public class ResultStatistics
    {
        public double? ProcessDurationInMinutes { get; set; }
        public Dictionary<string, ChannelResultStatistics> ChannelStatistics { get; set; } = new Dictionary<string, ChannelResultStatistics>();
    }
    public class RunHistoryEntry
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime ActualRunStartTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime ActualRunEndTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime BroadcastStartTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public required DateTime BroadcastEndTime { get; set; }
        public required ResultStatistics Statistics { get; set; } = new ResultStatistics();
    }
}
