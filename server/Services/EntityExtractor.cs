using Server.DTOs;
using Server.InsightProviders;
using Server.Models;
using System.Text.Json;

namespace Server.Services
{
    public interface IEntityExtractor
    {
        Task<QueryIntentContext> ExtractAsync(string userQuery);
    }
    /// <summary>
    /// Extract QueryIntentContext from user query
    /// </summary>
    public class EntityExtractor : IEntityExtractor
    {
        private readonly OpenAIChatProvider? _openAIChatProvider = null;

        public EntityExtractor(IAIProviderService aiProviderService)
        {
            var request = new InsightRequest
            {
                InsightType = InsightTypes.SemanticSearch,
                PromptName = "OpenAIChatProvider"
            };

            _openAIChatProvider = aiProviderService.GetProviderForRequest(request) as OpenAIChatProvider;

            if (_openAIChatProvider == null)
                throw new InvalidOperationException("No OpenAIChatProvider provider available");
        }

        public async Task<QueryIntentContext> ExtractAsync(string userQuery)
        {
            var systemPrompt = @$"
You are a smart assistant. Today's date is {DateTime.UtcNow:yyyy-MM-dd}. 
Analyze the following user query and extract:

- intents: list of strings.
- entities: list of objects with 'entity' and 'type'.
- dates: list of objects with this exact schema:
  - If the date is a single point in time:
    {{
      ""date"": ""yyyy-MM-dd"",
      ""type"": ""single"",
      ""startTime"": ""HH:mm:ss"" (optional),
      ""endTime"": ""HH:mm:ss"" (optional)
    }}
  - If the date is a range:
    {{
      ""type"": ""date_range"",
      ""startDate"": ""yyyy-MM-dd"",
      ""endDate"": ""yyyy-MM-dd"",
      ""startTime"": ""HH:mm:ss"" (optional),
      ""endTime"": ""HH:mm:ss"" (optional)
    }}

- sources: list of objects with 'source' and 'type'.

Rules:
- Use today's date ({DateTime.UtcNow:yyyy-MM-dd}) as the reference point for interpreting relative dates such as 'yesterday', 'last day', 'last week', etc.
- If the year is missing, use the current year ({DateTime.UtcNow.Year}).
- Always follow the above structure exactly.
- Always include startDate and endDate if type is ""date_range"".
- Return only valid JSON. No comments, no extra text.

User query: ""{userQuery}""
";


            var jsonResponse = await _openAIChatProvider!.GetChatCompletionAsync("You are a helpful media assistant.", systemPrompt);

            try
            {
                var llmDto = JsonSerializer.Deserialize<LlmQueryIntentDto>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (llmDto == null)
                    throw new InvalidOperationException("LLM returned invalid JSON");

                var context = Normalize(llmDto, userQuery);
               
                context.RawJsonResponse = jsonResponse;

                return context;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse model response as JSON. Raw content:\n" + jsonResponse, ex);
            }
        }

        private static QueryIntentContext Normalize(LlmQueryIntentDto llm, string originalQuery)
        {
            var context = new QueryIntentContext
            {
                OriginalQuery = originalQuery
            };

            var range = llm.Dates.FirstOrDefault(d => d.Type == "date_range");

            if (range != null)
            {
                if (DateTime.TryParse(range.StartDate, out var start))
                    context.StartDate = start;

                if (DateTime.TryParse(range.EndDate, out var end))
                    context.EndDate = end.AddDays(1).AddTicks(-1); // inclusive end
            }

            // Channels (if entity extractor finds them later)
            context.Channels = llm.Entities
                .Where(e => e.Type == "channel" || e.Type == "source")
                .Select(e => e.Entity)
                .Distinct()
                .ToList();

            return context;
        }

    }
}
