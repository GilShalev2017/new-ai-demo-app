using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Models.AiJobs;
using Server.Settings;

namespace Server.Repositories.AiJobs
{
    public interface IAiJobRequestRepository
    {
        Task<AiJobRequest?> GetJobByIdAsync(string jobId);
        Task<string?> CreateJobAsync(AiJobRequest jobRequest);
        Task DeleteJobAsync(string jobId);
        Task<List<AiJobRequest>> GetAllJobRequestsAsync();
        Task<List<AiJobRequest>> GetFilteredJobRequestsAsync(JobRequestFilter filter);
        Task<List<AiJobRequest>> GetUnfinishedJobsAsync();
        Task<List<AiJobRequest>> GetJobsByStatusAsync(string status);
        Task UpdateJobStatusAsync(AiJobRequest job, string status, string error = "");
        Task ScheduleNextOccurrenceAsync(AiJobRequest job);
        Task SaveOperationResultForChannel(string jobId, string channelName, string operationName, object segmentResult);
        Task UpdateRunHistoryAsync(string jobId, RunHistoryEntry runHistoryEntry);
    }
    public class AiJobRequestRepository : IAiJobRequestRepository
    {
        const string CollectionName = "intelligence_aijob_requests";
        private readonly ILogger<AiJobRequestRepository> _logger;
        private readonly IMongoCollection<AiJobRequest> _aiJobRequestCollection;
        private readonly IMongoCollection<BsonDocument> _bsonCollection;
        //private readonly int _mongoItemsLimit = 5000;

        public AiJobRequestRepository(IOptions<MongoDbSettings> settings, ILogger<AiJobRequestRepository> logger)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _aiJobRequestCollection = database.GetCollection<AiJobRequest>(CollectionName);
            _bsonCollection = database.GetCollection<BsonDocument>(CollectionName);
            _logger = logger;
        }
        public async Task<string?> CreateJobAsync(AiJobRequest jobRequest)
        {
            jobRequest.Status = JobStatus.Pending;
            jobRequest.CreatedBy = "Administrator";
            jobRequest.CreatedAt = DateTime.Now;
            jobRequest.BroadcastStartTime = TruncateMilliseconds(jobRequest.BroadcastStartTime);
            jobRequest.BroadcastEndTime = TruncateMilliseconds(jobRequest.BroadcastEndTime);
            jobRequest.NextScheduledTime = jobRequest.BroadcastStartTime;
            await _aiJobRequestCollection.InsertOneAsync(jobRequest);
            return jobRequest.Id;
        }
        public async Task<List<AiJobRequest>> GetAllJobRequestsAsync()
        {
            return await _aiJobRequestCollection.Find(Builders<AiJobRequest>.Filter.Empty).ToListAsync();
        }
        public async Task<List<AiJobRequest>> GetFilteredJobRequestsAsync(JobRequestFilter filter)
        {
            // Step 1: Convert filter.Start and filter.End to UTC to ensure consistency
            var filterStart = filter.Start?.ToUniversalTime();
            var filterEnd = filter.End?.ToUniversalTime();

            // Step 2: Build the dynamic filter for JobResults
            var jobFilters = new List<FilterDefinition<AiJobRequest>>();

            // Add time range filters
            if (filterStart.HasValue)
                jobFilters.Add(Builders<AiJobRequest>.Filter.Gte(jr => jr.BroadcastEndTime, filterStart.Value));

            if (filterEnd.HasValue)
                jobFilters.Add(Builders<AiJobRequest>.Filter.Lte(jr => jr.BroadcastStartTime, filterEnd.Value));

            // Add ChannelIds filter
            if (filter.ChannelIds != null && filter.ChannelIds.Length > 0)
            {
                jobFilters.Add(Builders<AiJobRequest>.Filter.AnyIn(jr => jr.ChannelIds, filter.ChannelIds));
            }


            if (filter.Operations is not null && filter.Operations.Count > 0)
                jobFilters.Add(Builders<AiJobRequest>.Filter.AnyIn(jr => jr.Operations, filter.Operations));

            if (filter.RuleRecurrence is not null)
                //TODO since RuleRecurrenceEnum was simplified - make sure it still works!
                jobFilters.Add(Builders<AiJobRequest>.Filter.Eq(jr => jr.RequestRule!.Recurrence, filter.RuleRecurrence.Value));

            // Combine all filters using AND
            var combinedFilter = jobFilters.Any()
                ? Builders<AiJobRequest>.Filter.And(jobFilters)
                : FilterDefinition<AiJobRequest>.Empty;

            // Step 3: Fetch the filtered JobRequests
            var jobRequests = await _aiJobRequestCollection.Find(combinedFilter).ToListAsync();

            // Step 5: Sort based on SortDirection
            if (filter.SortDirection == 1)
            {
                // Sort by oldest to newest
                jobRequests = jobRequests.OrderBy(e => e.CreatedAt).ToList();
            }
            else if (filter.SortDirection == 0)
            {
                // Sort by newest to oldest
                jobRequests = jobRequests.OrderByDescending(e => e.CreatedAt).ToList();
            }

            return jobRequests;
        }
        public async Task<AiJobRequest?> GetJobByIdAsync(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("Job ID cannot be null or empty.", nameof(jobId));
            }

            return await _aiJobRequestCollection
                .Find(entry => entry.Id == jobId)
                .FirstOrDefaultAsync();
        }
        public async Task<List<AiJobRequest>> GetJobsByStatusAsync(string status)
        {
            var filter = Builders<AiJobRequest>.Filter.Eq(j => j.Status, status);
            return await _aiJobRequestCollection.Find(filter).ToListAsync();

            //var now = DateTime.Now;
            //var filter = Builders<JobRequestEntity>.Filter.And(
            //    Builders<JobRequestEntity>.Filter.Eq(j => j.Status, status),
            //    Builders<JobRequestEntity>.Filter.Lte(j => j.NextScheduledTime, now)
            //);
            //return await _jobsCollection.Find(filter).ToListAsync();
        }
        public Task<List<AiJobRequest>> GetUnfinishedJobsAsync()
        {
            throw new NotImplementedException();
        }
        public async Task UpdateJobStatusAsync(AiJobRequest job, string status, string error = "")
        {
            if (string.IsNullOrEmpty(job.Id))
                throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

            // Define the filter to locate the job by ID
            var filter = Builders<AiJobRequest>.Filter.Eq(j => j.Id, job.Id);

            // Define the update to set the Status and optionally the Error
            var update = Builders<AiJobRequest>.Update
                .Set(j => j.Status, status)
                .Set(j => j.Error, string.IsNullOrEmpty(error) ? null : error); // Set error only if provided

            // Perform the update
            var result = await _aiJobRequestCollection.UpdateOneAsync(filter, update);

            // Optionally handle scenarios where no document was updated
            //if (result.ModifiedCount == 0)
            //    throw new Exception($"No document found with ID: {job.Id}");
        }
        //public async Task UpdateJobStatusAsync(AiJobRequest job, string status, string error = "")
        //{
        //    if (string.IsNullOrEmpty(job.Id))
        //        throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        //    // Update the job object with the new status and error
        //    job.Status = status;
        //    job.Error = string.IsNullOrEmpty(error) ? null : error;

        //    // Define the filter to locate the job by ID
        //    var filter = Builders<AiJobRequest>.Filter.Eq(j => j.Id, job.Id);

        //    // Perform the replace operation
        //    var result = await _aiJobRequestCollection.ReplaceOneAsync(filter, job);

        //    // Optionally handle scenarios where no document was updated
        //    //if (result.ModifiedCount == 0)
        //    //    throw new Exception($"No document found or updated with ID: {job.Id}");
        //}
        public async Task ScheduleNextOccurrenceAsync(AiJobRequest job)
        {
            var filter = Builders<AiJobRequest>.Filter.Eq(j => j.Id, job.Id);

            _logger.LogError($"ScheduleNextOccurrenceAsync job.RunHistory.count = {job.RunHistory.Count}");

            await _aiJobRequestCollection.ReplaceOneAsync(filter, job);
        }
        public async Task SaveOperationResultForChannel(string jobId, string channelName, string operationName, object segmentResult)
        {
            // Filter to find the job by its Id
            var filter = Builders<AiJobRequest>.Filter.Eq(j => j.Id, jobId);

            // Define the path to the specific channel and operation
            var updatePath = $"ChannelOperationResults.{channelName}.{operationName}";

            // Update definition to add the segment result to the specified channel and operation
            var update = Builders<AiJobRequest>.Update.Push(updatePath, segmentResult);

            // Use FindOneAndUpdateAsync to apply the update
            await _aiJobRequestCollection.FindOneAndUpdateAsync(filter, update);
        }
        public async Task DeleteJobAsync(string jobId)
        {
            await _aiJobRequestCollection!.DeleteOneAsync(d => d.Id == jobId);
        }
        /// <summary>
        /// called concurrently from multiple threads for the same AiJobRequest
        /// method needs to be atomic and thread-safe
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task UpdateRunHistoryAsync(string jobId, RunHistoryEntry runHistoryEntry)
        {
            var filter = Builders<AiJobRequest>.Filter.Eq(jr => jr.Id, jobId);

            var update = Builders<AiJobRequest>.Update
                .AddToSet(jr => jr.RunHistory, runHistoryEntry);

            _aiJobRequestCollection.UpdateOne(filter, update);

            // Then fetch the document and sort in memory
            var document = _aiJobRequestCollection.Find(filter).First();
            var sortedArray = document.RunHistory
                .OrderByDescending(x => x.ActualRunStartTime) // or your sort criteria
                .ToList();

            // Update with sorted array
            var sortUpdate = Builders<AiJobRequest>.Update
                .Set(jr => jr.RunHistory, sortedArray);

            _aiJobRequestCollection.UpdateOne(filter, sortUpdate);
        }
        private DateTime TruncateMilliseconds(DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }
    }
}
