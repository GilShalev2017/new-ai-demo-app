namespace Server.Models
{
    public enum InsightTypes
    {
        Transcription,
        Translation,
        Summary,
        FaceDetection,
        ChatGPTPrompt,
        LogoDetection
    }

    public enum InsightKind
    {
        System,   // Built-in (Summary, Translation, etc.)
        User      // User-defined prompt
    }

    public enum InsightCategory
    {
        Transcription,
        NLP,
        Vision
    }

    public sealed class InsightDefinition
    {
        public string Name { get; set; } = null!;
        public InsightTypes InsightType { get; set; }
        public string PromptTemplate { get; set; } = null!;
    }

    //public sealed class InsightDefinition
    //{
    //    public string Id { get; set; } = null!;
    //    public string Name { get; set; } = null!;
    //    public InsightKind Kind { get; set; }
    //    public InsightCategory Category { get; set; }

    //    // execution
    //    public string ProviderName { get; set; } = null!;
    //    public string PromptTemplate { get; set; } = null!;

    //    // input requirements
    //    public bool RequiresTranscription { get; set; }

    //    // UI
    //    public bool IsBuiltIn { get; set; }
    //}
}