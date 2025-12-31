using Server.DTOs;
using Server.Models;
using System.Net.Http;
using System.Text.Json;

namespace Server.Services
{
    public interface IVectorDBRepository
    {
        Task StoreTranscriptsAsync(List<TranscriptEx> transcripts, string channelId, DateTime clipBroadcastStartTime, DateTime clipBroadcastEndTime, string? mongoId = null);
        Task<List<EvidenceDto>> SearchAsync(string query, TranscriptsFilter filter, int topK = 5);
    }

    public class VectorDBRepository : IVectorDBRepository
    {
        private readonly IEmbeddingProvider _embeddingProvider;
        private readonly HttpClient _httpClient;
        private readonly string _collectionName;

        public VectorDBRepository(IEmbeddingProvider embeddingProvider, HttpClient http)
        {
            _embeddingProvider = embeddingProvider;
            _httpClient = http;
            _collectionName = "transcripts";
            _httpClient.BaseAddress = new Uri("http://localhost:6333");
        }

        public async Task EnsureCollectionExistsAsync(int vectorSize = 1536)
        {
            try
            {
                var collectionUrl = $"collections/{_collectionName}";

                var response = await _httpClient.GetAsync(collectionUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var createResponse = await _httpClient.PutAsJsonAsync(collectionUrl, new
                    {
                        vectors = new
                        {
                            size = vectorSize,
                            distance = "Cosine"
                        }
                    });

                    createResponse.EnsureSuccessStatusCode();

                    Console.WriteLine("Qdrant collection '{0}' created via REST.", _collectionName);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Failed to connect to Qdrant: {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Collection creation failed or already exists: {0}", ex.Message);
            }
        }

        public async Task StoreTranscriptsAsync(List<TranscriptEx> transcripts, string channelId, DateTime clipBroadcastStartTime, DateTime clipBroadcastEndTime, string? mongoId)
        {
            if (transcripts.Count == 0)
                return;

            await EnsureCollectionExistsAsync();

            var points = new List<object>();

            foreach (var t in transcripts)
            {
                var vector = await _embeddingProvider.EmbedAsync(t.Text);

                points.Add(new
                {
                    id = Guid.NewGuid().ToString(),
                    vector,
                    payload = new
                    {
                        mongo_id = mongoId,
                        channelId,
                        absStartTime = t.AbsStartTime,   // DateTime (UTC)
                        absEndTime = t.AbsEndTime,     // DateTime (UTC)
                        clipStartTime = clipBroadcastStartTime,
                        clipEndTime = clipBroadcastEndTime,
                        text = t.Text
                    }
                });
            }

            var response = await _httpClient.PutAsJsonAsync($"collections/{_collectionName}/points?wait=true", new { points });

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<EvidenceDto>> SearchAsync(string query, TranscriptsFilter filter, int topK = 5)
        {
            var vector = await _embeddingProvider.EmbedAsync(query);

            var must = new List<object>();

            if (filter.Start.HasValue)
                must.Add(new { key = "absStartTime", range = new { gte = filter.Start } });

            if (filter.End.HasValue)
                must.Add(new { key = "absEndTime", range = new { lte = filter.End } });

            if (filter.Channels?.Any() == true)
                must.Add(new { key = "channelId", match = new { any = filter.Channels } });

            var response = await _httpClient.PostAsJsonAsync(
                $"collections/{_collectionName}/points/search",
                new
                {
                    vector,
                    limit = topK,
                    filter = must.Any() ? new { must } : null,
                    with_payload = true
                });

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();

            return json!.RootElement
                .GetProperty("result")
                .EnumerateArray()
                .Select(r => new EvidenceDto
                {
                    ClipId = r.GetProperty("payload").GetProperty("mongo_id").GetString()!,
                    ChannelId = r.GetProperty("payload").GetProperty("channelId").GetString()!,
                    ClipStartTime = r.GetProperty("payload").GetProperty("clipStartTime").GetDateTime(),
                    Text = r.GetProperty("payload").GetProperty("text").GetString()!,
                    Score = r.GetProperty("score").GetSingle()
                })
                .ToList();
        }

    }
}
