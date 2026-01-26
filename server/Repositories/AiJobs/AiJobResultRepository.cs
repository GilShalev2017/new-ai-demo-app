using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Models;
using Server.Models.AiJobs;
using Server.Settings;

namespace Server.Repositories.AiJobs
{
    public interface IAiJobResultRepository
    {
        Task UpdateJobResultAsync(JobResult jobResult);
        Task<JobResult> GetJobResultByIdAsync(string jobResultId);
        Task<string> SaveJobResultAsync(JobResult jobResult);
        Task<List<AIDetection>> GetFilteredAIDetectionsAsync(JobResultFilter filter);
        Task<List<string>?> GetDistinctAIDetectedKeywordsAsync(JobResultFilter filter);
        Task<bool> DeleteJobRelatedResultsAsync(string jobId);
        Task<long> DeleteResultsOlderThanDateAsync(DateTime localDateTime, string? aiJobRequestId = null);
        public Task<List<BoundingBoxObjectsResult>> GetFilteredBoundingBoxObjects(BoundingBoxObjectFilter boundingBoxObjectFilter);
    }
    public class AiJobResultRepository : IAiJobResultRepository
    {
        const string CollectionName = "intelligence_aijob_results";
        private readonly ILogger<AiJobResultRepository> _logger;
        private readonly IMongoCollection<JobResult> _aiJobResultCollection;
        //private readonly IMongoCollection<BsonDocument> _bsonCollection;
        //private readonly int _mongoItemsLimit = 500;
        public AiJobResultRepository(IOptions<MongoDbSettings> settings, ILogger<AiJobResultRepository> logger)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _aiJobResultCollection = database.GetCollection<JobResult>(CollectionName);
            //_bsonCollection = database.GetCollection<BsonDocument>(CollectionName);

            _logger = logger;

            _aiJobResultCollection.Indexes.CreateOne(new CreateIndexModel<JobResult>(
                Builders<JobResult>.IndexKeys
                    .Ascending(jr => jr.Start)
                    .Ascending(jr => jr.End)
                    .Ascending(jr => jr.ChannelId)
                    .Ascending(jr => jr.Operation)
            ));

            _aiJobResultCollection.Indexes.CreateOne(new CreateIndexModel<JobResult>(
                Builders<JobResult>.IndexKeys.Ascending("AiJobRequestId")
            ));
        }
        public async Task<string> SaveJobResultAsync(JobResult jobResult)
        {
            await _aiJobResultCollection.InsertOneAsync(jobResult);
            return jobResult!.Id!; // Assuming Id is generated during insertion
        }
        public async Task<JobResult> GetJobResultByIdAsync(string jobResultId)
        {
            // Assuming you're using an ORM like Entity Framework or MongoDB
            // For example, with MongoDB:
            var jobResult = await _aiJobResultCollection
                                                  .Find(result => result.Id == jobResultId)
                                                  .FirstOrDefaultAsync();

            return jobResult;
        }
        public async Task UpdateJobResultAsync(JobResult jobResult)
        {
            if (jobResult == null)
            {
                throw new ArgumentNullException(nameof(jobResult), "JobResult cannot be null");
            }

            // Assuming you're using MongoDB:
            var updateDefinition = Builders<JobResult>.Update.Set(result => result.AiJobRequestId, jobResult.AiJobRequestId);
            await _aiJobResultCollection.UpdateOneAsync(result => result.Id == jobResult.Id, updateDefinition);
        }
        public async Task<List<AIDetection>> GetFilteredAIDetectionsAsync(JobResultFilter filter)
        {
            var filterStart = filter.Start;
            var filterEnd = filter.End;

            // Step 2: Build the dynamic filter for JobResults
            var jobFilters = new List<FilterDefinition<JobResult>>();

            // Add time range filters
            if (filterStart.HasValue)
                jobFilters.Add(Builders<JobResult>.Filter.Gte(jr => jr.End, filterStart.Value));

            if (filterEnd.HasValue)
                jobFilters.Add(Builders<JobResult>.Filter.Lte(jr => jr.Start, filterEnd.Value));

            // Add ChannelIds filter
            if (filter.ChannelIds != null && filter.ChannelIds.Length > 0)
                jobFilters.Add(Builders<JobResult>.Filter.In(jr => jr.ChannelId, filter.ChannelIds));

            // Add Operation filter
            if (!string.IsNullOrEmpty(filter.Operation))
                jobFilters.Add(Builders<JobResult>.Filter.Eq(jr => jr.Operation, filter.Operation));

            if (!string.IsNullOrEmpty(filter.AiJobRequestId))
                jobFilters.Add(Builders<JobResult>.Filter.Eq("AiJobRequestId", filter.AiJobRequestId));

            // Combine all filters using AND
            var combinedFilter = jobFilters.Any()
                ? Builders<JobResult>.Filter.And(jobFilters)
                : FilterDefinition<JobResult>.Empty;

            // Step 3: Fetch the filtered JobResults
            var jobResults = await _aiJobResultCollection.Find(combinedFilter).ToListAsync();

            // Step 4: Extract and filter events from the Transcript list
            var events = jobResults
                .SelectMany(jr => jr.Content ?? new List<TranscriptEx>(), (jr, transcript) => new AIDetection
                {
                    ChannelId = jr.ChannelId,
                    ChannelDisplayName = jr.ChannelDisplayName,
                    Operation = jr.Operation,
                    Start = transcript.AbsStartTime,
                    End = transcript.AbsEndTime,
                    Text = transcript.Text,
                    //Keyword = transcript.Keyword,
                    JobRequestId = jr.AiJobRequestId,
                })
                .Where(e =>
                    (!filterStart.HasValue || e.Start >= filterStart.Value) &&
                    (!filterEnd.HasValue || e.End <= filterEnd.Value) &&
                    (filter.Keywords == null || filter.Keywords.Length == 0 || // No keyword filtering if Keywords is empty
                     (!string.IsNullOrEmpty(e.Text) &&
                      filter.Keywords.Any(kw => e.Text.Contains(kw, StringComparison.OrdinalIgnoreCase))))) // Match any keyword
                .ToList();

            // Step 5: Sort based on SortDirection
            if (filter.SortDirection == 1)
            {
                // Sort by oldest to newest
                events = events.OrderBy(e => e.Start).ToList();
            }
            else if (filter.SortDirection == 0)
            {
                // Sort by newest to oldest
                events = events.OrderByDescending(e => e.Start).ToList();
            }

            return events;
        }
        public async Task<List<string>?> GetDistinctAIDetectedKeywordsAsync(JobResultFilter filter)
        {
            // Step 1: Get all filtered alerts using the existing method
            List<AIDetection> filteredAIDetections = await GetFilteredAIDetectionsAsync(filter);

            // Step 2: Extract distinct keywords
            List<string>? distinctKeywords = filteredAIDetections
                .Where(alert => !string.IsNullOrEmpty(alert.Keyword)) // Ensure keyword is not null or empty
                .Select(alert => alert.Keyword!) // Use null-forgiving operator
                .Distinct() // Get unique keywords
                .ToList();

            return distinctKeywords;
        }
        public async Task<bool> DeleteJobRelatedResultsAsync(string aiJobRequestId)
        {
            if (string.IsNullOrEmpty(aiJobRequestId))
            {
                throw new ArgumentException("AiJobRequestId cannot be null or empty.", nameof(aiJobRequestId));
            }

            var filter = Builders<JobResult>.Filter.Eq(jr => jr.AiJobRequestId, aiJobRequestId);

            var result = await _aiJobResultCollection.DeleteManyAsync(filter);

            return result.DeletedCount > 0; // Return true if any documents were deleted, otherwise false
        }
        public async Task<long> DeleteResultsOlderThanDateAsync(DateTime localDateTime, string? aiJobRequestId = null)
        {
            var filter = Builders<JobResult>.Filter.Lte(jr => jr.End, localDateTime);

            if (!string.IsNullOrEmpty(aiJobRequestId))
            {
                filter = Builders<JobResult>.Filter.And(
                             Builders<JobResult>.Filter.Lte(jr => jr.End, localDateTime),
                            Builders<JobResult>.Filter.Eq(jr => jr.AiJobRequestId, aiJobRequestId)
                        );
            }

            var result = await _aiJobResultCollection.DeleteManyAsync(filter);

            return result.DeletedCount;
        }
        public async Task<List<BoundingBoxObjectsResult>> GetFilteredBoundingBoxObjects(BoundingBoxObjectFilter boundingBoxObjectFilter)
        {
            // Step 1: Build the filter for JobResults
            var jobFilters = new List<FilterDefinition<JobResult>>();

            // Filter by ChannelIds
            if (boundingBoxObjectFilter.ChannelIds.Count > 0)
            {
                jobFilters.Add(Builders<JobResult>.Filter.In(jr => jr.ChannelId, boundingBoxObjectFilter.ChannelIds));
            }

            // Filter by TimestampStart and TimestampEnd
            jobFilters.Add(Builders<JobResult>.Filter.Lte(jr => jr.Start, boundingBoxObjectFilter.TimestampEnd));
            jobFilters.Add(Builders<JobResult>.Filter.Gte(jr => jr.End, boundingBoxObjectFilter.TimestampStart));

            // Combine all filters using AND
            var combinedFilter = jobFilters.Any()
                ? Builders<JobResult>.Filter.And(jobFilters)
                : FilterDefinition<JobResult>.Empty;

            List<JobResult> jobResults = new List<JobResult>();

            if (boundingBoxObjectFilter.MaxResults == -1)
            {

                // Step 2: Query the database for matching JobResults
                jobResults = await _aiJobResultCollection
                    .Find(combinedFilter)
                    .SortBy(jr => jr.Start) // Sort chronologically by Start
                                            //.Limit(_mongoItemsLimit)
                    .ToListAsync();
            }
            else
            {
                // Step 2: Query the database for matching JobResults
                jobResults = await _aiJobResultCollection
                    .Find(combinedFilter)
                    .SortBy(jr => jr.Start) // Sort chronologically by Start
                    .Limit(boundingBoxObjectFilter.MaxResults)
                    .ToListAsync();
            }

            // Step 3: Group faces by channel ID
            var facesByChannel = jobResults
                .GroupBy(jr => jr.ChannelId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(jr => jr.DetectedObjects ?? new List<BoundingBoxObject>())
                        .Where(bb => (boundingBoxObjectFilter.ObjectType is null)
                            ?
                            bb.TimestampStart >= boundingBoxObjectFilter.TimestampStart &&
                            bb.TimestampEnd <= boundingBoxObjectFilter.TimestampEnd
                            :
                            bb.ObjectType == boundingBoxObjectFilter.ObjectType &&
                            bb.TimestampStart >= boundingBoxObjectFilter.TimestampStart &&
                            bb.TimestampEnd <= boundingBoxObjectFilter.TimestampEnd)
                        .OrderBy(face => face.TimestampStart)
                        .ToList()
                );

            // Step 4: Create FaceDetectionResult for each channel ID in the filter
            var bbResults = boundingBoxObjectFilter.ChannelIds
                .Select(channelId => new BoundingBoxObjectsResult
                {
                    ChannelId = channelId,
                    TimestampStart = boundingBoxObjectFilter.TimestampStart,
                    TimestampEnd = boundingBoxObjectFilter.TimestampEnd,
                    Detections = facesByChannel.ContainsKey(channelId) ? facesByChannel[channelId] : new List<BoundingBoxObject>()
                })
                .ToList();

            return bbResults;
        }
    }
}
