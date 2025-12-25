using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;
using Server.Settings;

namespace Server.Services
{
    public class DataSeederService
    {
        private readonly IMongoCollection<Clip> _clipsCollection;
        private readonly IVideoUtilityService _videoUtility;
        private readonly ILogger<DataSeederService> _logger;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] SampleTitles =
        {
            "Breaking News - Market Update",
            "Sports Highlights - Finals",
            "Tech Interview with CEO",
            "Political Debate Analysis",
            "Entertainment Weekly Roundup",
            "Financial Markets Overview",
            "Weather Forecast Update",
            "Health & Wellness Tips",
            "Travel Destination Guide",
            "Cooking Show Special"
        };

        public DataSeederService(
            IOptions<MongoDbSettings> mongoDbSettings,
            IWebHostEnvironment environment,
            IVideoUtilityService videoUtility,
            ILogger<DataSeederService> logger)
        {
            _environment = environment;
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _clipsCollection = mongoDatabase.GetCollection<Clip>(
                mongoDbSettings.Value.ClipsCollectionName
            );
            _videoUtility = videoUtility;
            _logger = logger;
        }

        private static List<string> GenerateTags(string channelName)
        {
            return channelName switch
            {
                "BBC" or "BBC2" or "CNN" or "EuroNews" or "France24"
                    => new List<string> { "News", "Politics", "World" },

                "TRTWorld" or "WION" or "WION2"
                    => new List<string> { "International", "Breaking" },

                "ILTV"
                    => new List<string> { "Israel", "Regional" },

                _ => new List<string> { "General" }
            };
        }

        public async Task SeedClipsAsync()
        {
            var existingCount = await _clipsCollection.CountDocumentsAsync(_ => true);
            if (existingCount > 0)
            {
                _logger.LogInformation("Clips already exist ({Count}), skipping seed", existingCount);
                return;
            }

            _logger.LogInformation("Seeding clips from existing media files...");

            var videoFiles = Directory
                .GetFiles(Path.Combine(_environment.WebRootPath, "videos"), "*.mp4")
                .Select(Path.GetFileName)
                .ToList();

            if (!videoFiles.Any())
            {
                _logger.LogWarning("No video files found for seeding");
                return;
            }

            var clips = new List<Clip>();
            var random = new Random();

            for (int i = 0; i < videoFiles.Count; i++)
            {
                var videoFile = videoFiles[i];
                var baseName = Path.GetFileNameWithoutExtension(videoFile);

                clips.Add(new Clip
                {
                    Title = SampleTitles[i % SampleTitles.Length],
                    ChannelName = baseName,
                    ChannelId = baseName.ToLowerInvariant(),
                    VideoUrl = _videoUtility.GetVideoUrl(videoFile),
                    ThumbnailUrl = _videoUtility.GetThumbnailUrl(videoFile),
                    Duration = _videoUtility.GetVideoDuration(videoFile),
                    FileSize = _videoUtility.GetFileSize(videoFile),
                    Tags = GenerateTags(baseName),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(10, 5000)),
                    IsProcessed = random.NextDouble() > 0.3,
                    Transcription = "Sample transcription content...",
                    Sentiment = new SentimentData
                    {
                        Positive = random.NextDouble(),
                        Neutral = random.NextDouble() * 0.5,
                        Negative = random.NextDouble() * 0.3,
                        OverallScore = random.NextDouble()
                    }
                });
            }

            await _clipsCollection.InsertManyAsync(clips);

            _logger.LogInformation("Seeded {Count} clips successfully", clips.Count);
        }

        //public async Task SeedClipsAsync()
        //{
        //    // Check if clips already exist
        //    var existingCount = await _clipsCollection.CountDocumentsAsync(_ => true);
        //    if (existingCount > 0)
        //    {
        //        _logger.LogInformation("Database already seeded with {Count} clips", existingCount);
        //        return;
        //    }

        //    _logger.LogInformation("Seeding database with sample clips...");

        //    var sampleClips = new List<Clip>
        //    {
        //        new Clip
        //        {
        //            Title = "Breaking News - Market Update",
        //            ChannelName = "News Network",
        //            ChannelId = "channel-1",
        //            VideoUrl = _videoUtility.GetVideoUrl("clip-001.mp4"),
        //            ThumbnailUrl = _videoUtility.GetThumbnailUrl("clip-001.mp4"),
        //            Duration = _videoUtility.GetVideoDuration("clip-001.mp4"),
        //            FileSize = _videoUtility.GetFileSize("clip-001.mp4"),
        //            Tags = new List<string> { "News", "Finance", "Breaking" },
        //            CreatedAt = DateTime.UtcNow.AddHours(-2),
        //            IsProcessed = true,
        //            Transcription = "Good evening, today's market update shows significant gains...",
        //            Sentiment = new SentimentData
        //            {
        //                Positive = 0.72,
        //                Neutral = 0.20,
        //                Negative = 0.08,
        //                OverallScore = 0.64
        //            }
        //        },
        //        new Clip
        //        {
        //            Title = "Sports Highlights - Finals",
        //            ChannelName = "ESPN",
        //            ChannelId = "channel-2",
        //            VideoUrl = _videoUtility.GetVideoUrl("clip-002.mp4"),
        //            ThumbnailUrl = _videoUtility.GetThumbnailUrl("clip-002.mp4"),
        //            Duration = _videoUtility.GetVideoDuration("clip-002.mp4"),
        //            FileSize = _videoUtility.GetFileSize("clip-002.mp4"),
        //            Tags = new List<string> { "Sports", "Basketball", "Highlights" },
        //            CreatedAt = DateTime.UtcNow.AddHours(-5),
        //            IsProcessed = false
        //        },
        //        new Clip
        //        {
        //            Title = "Tech Interview with CEO",
        //            ChannelName = "Tech Today",
        //            ChannelId = "channel-3",
        //            VideoUrl = _videoUtility.GetVideoUrl("clip-003.mp4"),
        //            ThumbnailUrl = _videoUtility.GetThumbnailUrl("clip-003.mp4"),
        //            Duration = _videoUtility.GetVideoDuration("clip-003.mp4"),
        //            FileSize = _videoUtility.GetFileSize("clip-003.mp4"),
        //            Tags = new List<string> { "Technology", "Interview", "Business" },
        //            CreatedAt = DateTime.UtcNow.AddDays(-1),
        //            IsProcessed = true
        //        }
        //    };

        //    await _clipsCollection.InsertManyAsync(sampleClips);
        //    _logger.LogInformation("Successfully seeded {Count} clips", sampleClips.Count);
        //}
    }
}