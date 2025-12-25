using Server.Models;
using Server.Services;

namespace Server.InsightProviders
{
    public sealed class TranscriptionInsightHandler : IInsightHandler
    {
        public InsightTypes InsightType => InsightTypes.Transcription;

        private readonly IVideoUtilityService _videoUtilityService;

        public TranscriptionInsightHandler(IVideoUtilityService videoUtilityService)
        {
            _videoUtilityService = videoUtilityService;
        }

        public async Task<InsightInputData> PrepareInputAsync(Clip clip, InsightRequest request)
        {
            var videoFilePath = _videoUtilityService.GetFilePathFromUrl(clip.VideoUrl);
            var mp3Path = await _videoUtilityService.ConvertMp4ToMp3Async(videoFilePath);

            return new InsightInputData
            {
                AudioInput = new AudioDTO
                {
                    FilePath = mp3Path,
                    AudioLanguage = request.SourceLanguage
                }
            };
        }
    }

}
