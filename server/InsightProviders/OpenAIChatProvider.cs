using Server.Models;
using System.Text;
using System.Text.Json;

namespace Server.InsightProviders
{
    public class OpenAIChatProvider : ProviderBase
    {
        public override string Name => "OpenAIChatProvider";
        public override IReadOnlyCollection<InsightTypes> SupportedInsightTypes => new[] { InsightTypes.Summary };

        private readonly HttpClient _httpClient;
        
        private string openAiKey = "";

        public OpenAIChatProvider()
        {
            // Ideally, inject IHttpClientFactory in production
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(openAiKey))
                throw new Exception("OPENAI_API_KEY is not set");
        }

        protected override void EnsureInputValidity(InsightInputData input)
        {
            if (input.Transcripts == null || input.Transcripts.Count == 0)
                throw new InvalidOperationException("Transcripts are required for summary.");
        }

        protected override async Task<Insight> StartProcessingAsync(InsightInputData insightInputData, InsightRequest insightRequest)
        {
            // Merge all transcripts into one string (with timestamps)
            string mergedTranscripts = ConvertTranscriptsToStringWithTimeStamp(insightInputData.Transcripts!);

            string systemMessage = "You are an AI assistant. Summarize the following transcription concisely while preserving the main points.";

            string summary = await SummarizeAsync(systemMessage, mergedTranscripts);

            if (string.IsNullOrWhiteSpace(summary))
                throw new InvalidOperationException("OpenAI returned an empty summary.");

            return CreateSummaryResponse(summary);
        }

        private async Task<string> SummarizeAsync(string systemMessage, string transcriptions)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiKey);

            // Build a single prompt combining system instruction and text to summarize
            string prompt = $"{systemMessage}\n\n{transcriptions}";

            var requestBody = new
            {
                model = "gpt-5.2",        // Use a recent reasoning-capable model
                input = prompt           // Pass prompt via "input"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody),
                Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/responses", content);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"OpenAI API error: {response.StatusCode} - {jsonString}");

            using var doc = JsonDocument.Parse(jsonString);

            // Responses API returns output items — find the assistant text
            if (doc.RootElement.TryGetProperty("output", out var outputArray))
            {
                foreach (var item in outputArray.EnumerateArray())
                {
                    if (item.GetProperty("type").GetString() == "message"
                        && item.TryGetProperty("content", out var contentElems))
                    {
                        foreach (var contentElem in contentElems.EnumerateArray())
                        {
                            if (contentElem.GetProperty("type").GetString() == "output_text")
                            {
                                return contentElem.GetProperty("text").GetString() ?? "";
                            }
                        }
                    }
                }
            }

            return "";
        }

        private static string ConvertTranscriptsToString(List<Transcript> transcripts)
        {
            return string.Join(" ", transcripts.Select(t => t.Text));
        }

        private static string ConvertTranscriptsToStringWithTimeStamp(List<Transcript> transcripts)
        {
            return string.Join("\n\n", transcripts.Select(t =>
                $"{TimeSpan.FromSeconds(t.StartInSeconds):hh\\:mm\\:ss} - {TimeSpan.FromSeconds(t.EndInSeconds):hh\\:mm\\:ss}\n{t.Text}"));
        }

        private Insight CreateSummaryResponse(string summary)
        {
            return new SummaryInsight
            {
                ProviderName = Name,
                InsightType = InsightTypes.Summary,
                Summary = summary
            };
        }
    }
}
