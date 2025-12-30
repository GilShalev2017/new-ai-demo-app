using Server.Models;

namespace Server.InsightProviders
{
    public sealed class InsightInputData
    {
        public List<TranscriptEx>? Transcripts { get; init; }

        // Used by transcription / language verification providers
        public AudioDTO? AudioInput { get; init; }
    }
}
