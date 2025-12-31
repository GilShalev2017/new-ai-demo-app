namespace Server.DTOs
{
    public class LlmQueryIntentDto
    {
        public List<string> Intents { get; set; } = new();

        public List<LlmEntityDto> Entities { get; set; } = new();

        public List<LlmDateDto> Dates { get; set; } = new();

        public List<LlmSourceDto> Sources { get; set; } = new();
    }

    public class LlmEntityDto
    {
        public string Entity { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class LlmDateDto
    {
        public string Type { get; set; } = ""; // "single" | "date_range"

        public string? Date { get; set; }

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }

    public class LlmSourceDto
    {
        public string Source { get; set; } = "";
        public string Type { get; set; } = "";
    }

}
