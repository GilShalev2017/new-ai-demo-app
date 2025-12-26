using Server.Models;

namespace Server.InsightProviders
{
    public sealed class TranscriptionDependentInputBuilder : IInsightInputBuilder
    {
        public bool CanBuild(InsightRequest request)
            => request.InsightType == InsightTypes.Summary
            || request.InsightType == InsightTypes.ChatGPTPrompt;

        public Task<InsightInputData?> BuildAsync(Clip clip, InsightRequest request)
        {
            var transcription = clip.GetInsight<TranscriptionInsight>();
            if (transcription == null)
                return Task.FromResult<InsightInputData?>(null);

            return Task.FromResult<InsightInputData?>(new InsightInputData
            {
                Transcripts = transcription.Transcripts
            });
        }
    }

}
