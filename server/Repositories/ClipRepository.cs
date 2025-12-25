using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Models;
using Server.Settings;

namespace Server.Repositories
{
    public interface IClipRepository
    {
        Task<ClipSearchResponse> SearchClipsAsync(ClipSearchRequest request);
        Task<Clip?> GetByIdAsync(string id);
        Task<Clip> CreateAsync(Clip clip);
        Task<Clip?> UpdateAsync(string id, Clip clip);
        Task<bool> DeleteAsync(string id);
        Task<int> DeleteMultipleAsync(List<string> ids);
        Task<List<Clip>> GetByRequestIdAsync(string requestId);
        Task<List<Clip>> GetByChannelIdAsync(string channelId);
    }

    public class ClipRepository : IClipRepository
    {
        private readonly IMongoCollection<Clip> _clips;
        private readonly ILogger<ClipRepository> _logger;

        public ClipRepository(
            IOptions<MongoDbSettings> settings,
            ILogger<ClipRepository> logger)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _clips = database.GetCollection<Clip>(settings.Value.ClipsCollectionName);
            _logger = logger;

            //CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexName = "CreatedAt_ChannelId_ClipRequestId_Title_text";

                // List existing indexes
                var existingIndexes = _clips.Indexes.List().ToList();

                bool indexExists = existingIndexes.Any(ix =>
                    ix.GetValue("name", "").AsString == indexName
                );

                if (!indexExists)
                {
                    var indexKeys = Builders<Clip>.IndexKeys
                        .Ascending(c => c.CreatedAt)
                        .Ascending(c => c.ChannelId)
                        .Ascending(c => c.ClipRequestId)
                        .Text(c => c.Title);

                    var model = new CreateIndexModel<Clip>(indexKeys, new CreateIndexOptions { Name = indexName });
                    _clips.Indexes.CreateOne(model);

                    _logger.LogInformation("Created index {IndexName}", indexName);
                }
                else
                {
                    _logger.LogInformation("Index {IndexName} already exists. Skipping.", indexName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create indexes for Clips");
            }
        }


        public async Task<ClipSearchResponse> SearchClipsAsync(ClipSearchRequest request)
        {
            var filterBuilder = Builders<Clip>.Filter;
            var filter = filterBuilder.Empty;

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var terms = request.SearchTerm.ToLower()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var searchFilters = new List<FilterDefinition<Clip>>();

                foreach (var term in terms)
                {
                    var termFilter = filterBuilder.Or(
                        filterBuilder.Regex(c => c.Title,
                            new BsonRegularExpression(term, "i")),
                        filterBuilder.Regex(c => c.Transcription,
                            new BsonRegularExpression(term, "i")),
                        filterBuilder.AnyEq(c => c.Tags, term)
                    );
                    searchFilters.Add(termFilter);
                }

                filter &= request.SearchOperandAnd
                    ? filterBuilder.And(searchFilters)
                    : filterBuilder.Or(searchFilters);
            }

            // Tag filter
            if (request.Tags?.Length > 0)
            {
                filter &= filterBuilder.AnyIn(c => c.Tags, request.Tags);
            }

            // Channel filter
            if (request.ChannelIds?.Length > 0)
            {
                filter &= filterBuilder.In(c => c.ChannelId, request.ChannelIds);
            }

            // Date filter
            if (request.FromDate.HasValue)
            {
                filter &= filterBuilder.Gte(c => c.CreatedAt, request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                filter &= filterBuilder.Lte(c => c.CreatedAt, request.ToDate.Value);
            }

            var totalCount = await _clips.CountDocumentsAsync(filter);

            // Sorting
            var sortBuilder = Builders<Clip>.Sort;
            var sort = request.SortOption switch
            {
                0 => sortBuilder.Descending(c => c.CreatedAt),
                1 => sortBuilder.Ascending(c => c.CreatedAt),
                2 => sortBuilder.Ascending(c => c.Title),
                3 => sortBuilder.Descending(c => c.Title),
                4 => sortBuilder.Descending(c => c.Duration),
                5 => sortBuilder.Ascending(c => c.Duration),
                _ => sortBuilder.Descending(c => c.CreatedAt)
            };

            var clips = await _clips
                .Find(filter)
                .Sort(sort)
                .Skip(request.Skip)
                .Limit(request.Limit)
                .ToListAsync();

            return new ClipSearchResponse
            {
                Clips = clips,
                TotalCount = (int)totalCount,
                HasMore = totalCount > (request.Skip + request.Limit)
            };
        }

        public async Task<Clip?> GetByIdAsync(string id)
        {
            return await _clips.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Clip> CreateAsync(Clip clip)
        {
            clip.CreatedAt = DateTime.UtcNow;
            await _clips.InsertOneAsync(clip);
            return clip;
        }

        public async Task<Clip?> UpdateAsync(string id, Clip clip)
        {
            clip.UpdatedAt = DateTime.UtcNow;
            var result = await _clips.ReplaceOneAsync(c => c.Id == id, clip);
            return result.ModifiedCount > 0 ? await GetByIdAsync(id) : null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _clips.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<int> DeleteMultipleAsync(List<string> ids)
        {
            var filter = Builders<Clip>.Filter.In(c => c.Id, ids);
            var result = await _clips.DeleteManyAsync(filter);
            return (int)result.DeletedCount;
        }

        public async Task<List<Clip>> GetByRequestIdAsync(string requestId)
        {
            return await _clips
                .Find(c => c.ClipRequestId == requestId)
                .ToListAsync();
        }

        public async Task<List<Clip>> GetByChannelIdAsync(string channelId)
        {
            return await _clips
                .Find(c => c.ChannelId == channelId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }

}
