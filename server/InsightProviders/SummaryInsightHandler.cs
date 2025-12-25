using Server.Models;

namespace Server.InsightProviders
{
    public sealed class SummaryInsightHandler : IInsightHandler
    {
        public InsightTypes InsightType => InsightTypes.Summary;

        public Task<InsightInputData> PrepareInputAsync(Clip clip, InsightRequest request)
        {
            var transcription = clip.GetInsight<TranscriptionInsight>();

            if (transcription?.Transcripts == null)
                return Task.FromResult<InsightInputData>(null!);

            return Task.FromResult(new InsightInputData
            {
                Transcripts = transcription.Transcripts
            });
        }
    }

}
