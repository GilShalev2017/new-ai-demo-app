using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Server.Services
{
    public interface IEmbeddingProvider
    {
        Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
    }

    public sealed class OpenAiEmbeddingProvider : IEmbeddingProvider
    {
        private readonly HttpClient _http;
        private const string Model = "text-embedding-3-small";

        public OpenAiEmbeddingProvider(HttpClient http, IConfiguration config)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://api.openai.com/v1/");
            //_http.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", config["OpenAI:ApiKey"]);

            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            if (string.IsNullOrWhiteSpace(openAiKey))
                throw new InvalidOperationException("OPENAI_API_KEY is not set");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", openAiKey);
        }

        public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("embeddings", new
            {
                model = Model,
                input = text
            }, ct);

            response.EnsureSuccessStatusCode();

            var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
            return doc!.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();
        }
    }

}
