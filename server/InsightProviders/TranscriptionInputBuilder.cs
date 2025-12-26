using Server.Models;
using Server.Services;

namespace Server.InsightProviders
{
    public sealed class TranscriptionInputBuilder : IInsightInputBuilder
    {
        private readonly IVideoUtilityService _videoUtilityService;

        public TranscriptionInputBuilder(IVideoUtilityService videoUtilityService)
        {
            _videoUtilityService = videoUtilityService;
        }

        public bool CanBuild(InsightRequest request)
            => request.InsightType == InsightTypes.Transcription;

        public async Task<InsightInputData?> BuildAsync(Clip clip, InsightRequest request)
        {
            var videoPath = _videoUtilityService.GetFilePathFromUrl(clip.VideoUrl);
            if (string.IsNullOrEmpty(videoPath))
                return null;

            var mp3Path = await _videoUtilityService.ConvertMp4ToMp3Async(videoPath);

            return new InsightInputData
            {
                AudioInput = new AudioDTO
                {
                    FilePath = mp3Path,
                    AudioLanguage = request.SourceLanguage,
                    DurationSec = 0 // optional – fill later if you calculate it
                }
            };
        }

    }

}
