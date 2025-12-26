using Server.Models;

namespace Server.InsightProviders
{
    public sealed class InsightInputData
    {
        public List<Transcript>? Transcripts { get; init; }

        // Used by transcription / language verification providers
        public AudioDTO? AudioInput { get; init; }
    }
}
