using Server.Models;
using Sprache;
using System.Text;
using System.Text.Json;
using SharpToken;
using System.Net.Http.Headers;

namespace Server.InsightProviders
{
    public class OpenAIChatProvider : ProviderBase
    {
        public override string Name => "OpenAIChatProvider";

        public override IReadOnlyCollection<InsightTypes> SupportedInsightTypes =>
            new[] { InsightTypes.Summary, InsightTypes.ChatGPTPrompt, InsightTypes.SemanticSearch };

        private readonly HttpClient _httpClient;
        private readonly string _openAiKey;

        public OpenAIChatProvider()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            _openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            if (string.IsNullOrWhiteSpace(_openAiKey))
                throw new InvalidOperationException("OPENAI_API_KEY is not set");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);
        }

        protected override void EnsureInputValidity(InsightInputData input)
        {
            if (input.Transcripts == null || input.Transcripts.Count == 0)
                throw new InvalidOperationException("Transcripts are required for OpenAI insights.");
        }

        protected override async Task<Insight> StartProcessingAsync(InsightInputData insightInputData, InsightRequest insightRequest)
        {
            EnsureInputValidity(insightInputData);

            string transcriptText = ConvertTranscriptsToStringWithTimeStamp(
                insightInputData.Transcripts!);

            if (insightRequest.InsightType == InsightTypes.Summary)
            {
                string systemPrompt =
                    "You are an AI assistant. Summarize the following transcription concisely while preserving the main points.";

                string summary = await CallOpenAIAsync(systemPrompt, transcriptText);

                if (string.IsNullOrWhiteSpace(summary))
                    throw new InvalidOperationException("OpenAI returned an empty summary.");

                return new SummaryInsight
                {
                    ProviderName = Name,
                    InsightType = InsightTypes.Summary,
                    Summary = summary
                };
            }

            // ChatGPTPrompt (user-defined)
            if (insightRequest.InsightType == InsightTypes.ChatGPTPrompt)
            {
                if (string.IsNullOrWhiteSpace(insightRequest.PromptText))
                    throw new InvalidOperationException("PromptText is required for ChatGPTPrompt.");

                string response = await CallOpenAIAsync(
                    insightRequest.PromptText,
                    transcriptText);

                return new ChatGPTPromptInsight
                {
                    ProviderName = Name,
                    InsightType = InsightTypes.ChatGPTPrompt,
                    PromptName = insightRequest.PromptName ?? "Custom Prompt",
                    PromptText = insightRequest.PromptText!,
                    Result = response
                };
            }

            throw new NotSupportedException($"Insight type {insightRequest.InsightType} is not supported by {Name}");
        }

        private async Task<string> CallOpenAIAsync(string systemPrompt, string userContent)
        {
            string prompt = $"{systemPrompt}\n\n{userContent}";

            var requestBody = new
            {
                model = "gpt-5.2",
                input = prompt
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/responses",
                content);

            string jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"OpenAI API error: {response.StatusCode} - {jsonString}");

            using var doc = JsonDocument.Parse(jsonString);

            if (doc.RootElement.TryGetProperty("output", out var outputArray))
            {
                foreach (var item in outputArray.EnumerateArray())
                {
                    if (item.GetProperty("type").GetString() == "message" &&
                        item.TryGetProperty("content", out var contentArray))
                    {
                        foreach (var contentElem in contentArray.EnumerateArray())
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
        public async Task<string> GetChatCompletionAsync(string prompt, string data)
        {
            var encoding = GptEncoding.GetEncodingForModel("gpt-5.2");
            int totalTokens = encoding.Encode(prompt).Count + encoding.Encode(data).Count;

            if (totalTokens > 128000)//find out the gpt-5.2 limitations
            {
                //throw new Exception($"Your query contains too much data ({totalTokens} tokens). " +
                //    $"Please reduce the time range, number of channels, or amount of input data.");
                return $"__TOO_MANY_TOKENS__:{totalTokens}";
            }

            var body = new
            {
                model = "gpt-5.2",//Could be others!!!
                messages = new[]
                {
                     new { role = "system", content = prompt },  // Instructions and context
                     new { role = "user", content = data }    // The actual data to analyze
                },
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception("OpenAI API error: " + error);
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        private static string ConvertTranscriptsToStringWithTimeStamp(List<TranscriptEx> transcripts)
        {
            return string.Join("\n\n",
                transcripts.Select(t =>
                    $"{TimeSpan.FromSeconds(t.StartInSeconds):hh\\:mm\\:ss} - " +
                    $"{TimeSpan.FromSeconds(t.EndInSeconds):hh\\:mm\\:ss}\n{t.Text}"));
        }

        
    }
}

