using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Server.Models;
using Server.Settings;

namespace Server.Repositories
{
    public interface IClipRequestRepository
    {
        Task<List<ClipRequest>> GetAllAsync();
        Task<ClipRequest?> GetByIdAsync(string id);
        Task<ClipRequest> CreateAsync(ClipRequest clipRequest);
        Task<ClipRequest?> UpdateAsync(string id, ClipRequest clipRequest);
        Task<bool> DeleteAsync(string id);
        Task<List<ClipRequest>> GetByStatusAsync(RequestStatus status);
        Task<bool> UpdateStatusAsync(string id, RequestStatus status, string? errorMessage = null);
        Task<bool> AddClipIdAsync(string requestId, string clipId);
    }

    public class ClipRequestRepository : IClipRequestRepository
    {
        private readonly IMongoCollection<ClipRequest> _clipRequests;
        private readonly ILogger<ClipRequestRepository> _logger;

        public ClipRequestRepository(
            IOptions<MongoDbSettings> settings,
            ILogger<ClipRequestRepository> logger)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _clipRequests = database.GetCollection<ClipRequest>(
                settings.Value.ClipRequestsCollectionName
            );
            _logger = logger;

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexKeys = Builders<ClipRequest>.IndexKeys
                    .Ascending(r => r.Status)
                    .Ascending(r => r.CreatedAt);

                _clipRequests.Indexes.CreateOne(
                    new CreateIndexModel<ClipRequest>(indexKeys)
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create indexes for ClipRequests");
            }
        }

        public async Task<List<ClipRequest>> GetAllAsync()
        {
            return await _clipRequests
                .Find(_ => true)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ClipRequest?> GetByIdAsync(string id)
        {
            return await _clipRequests
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ClipRequest> CreateAsync(ClipRequest clipRequest)
        {
            clipRequest.CreatedAt = DateTime.UtcNow;
            clipRequest.Status = RequestStatus.Pending;
            await _clipRequests.InsertOneAsync(clipRequest);
            return clipRequest;
        }

        public async Task<ClipRequest?> UpdateAsync(string id, ClipRequest clipRequest)
        {
            clipRequest.UpdatedAt = DateTime.UtcNow;
            var result = await _clipRequests.ReplaceOneAsync(
                r => r.Id == id,
                clipRequest
            );

            return result.ModifiedCount > 0
                ? await GetByIdAsync(id)
                : null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _clipRequests.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<ClipRequest>> GetByStatusAsync(RequestStatus status)
        {
            return await _clipRequests
                .Find(r => r.Status == status)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(
            string id,
            RequestStatus status,
            string? errorMessage = null)
        {
            var update = Builders<ClipRequest>.Update
                .Set(r => r.Status, status)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                update = update.Set(r => r.ErrorMessage, errorMessage);
            }

            var result = await _clipRequests.UpdateOneAsync(
                r => r.Id == id,
                update
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> AddClipIdAsync(string requestId, string clipId)
        {
            var update = Builders<ClipRequest>.Update
                .Push(r => r.ClipIds, clipId)
                .Set(r => r.UpdatedAt, DateTime.UtcNow);

            var result = await _clipRequests.UpdateOneAsync(
                r => r.Id == requestId,
                update
            );

            return result.ModifiedCount > 0;
        }
    }
}
