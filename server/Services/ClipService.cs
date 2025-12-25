using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.DTOs;
using Server.InsightProviders;
using Server.Models;
using Server.Repositories;
using Server.Settings;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Services
{
    public interface IClipService
    {
        Task<ClipSearchResponse> SearchClipsAsync(ClipSearchRequest request);
        Task<Clip?> GetByIdAsync(string id);
        Task<Clip> CreateAsync(CreateClipsPayloadDto dto);
        Task<Clip?> UpdateAsync(string id, UpdateClipDto dto);
        Task<bool> DeleteAsync(string id);
        Task<int> DeleteMultipleAsync(List<string> ids);
        Task<Clip> AddInsightsToClipAsync(Clip clip, List <InsightRequest> requests);
        Task<Clip> RemoveInsightsAsync(Clip clip, List<string> insightIdsToDelete);
    }

    public class ClipService : IClipService
    {
        private readonly IClipRepository _clipRepository;
        private readonly IClipRequestRepository _clipRequestRepository;
        private readonly IVideoUtilityService _videoUtilityService;
        private readonly ILogger<ClipService> _logger;
        private readonly IAIProviderService _aiProviderService;
        private readonly IInsightHandlerFactory _insightHandlerFactory;

        public ClipService(
            IClipRepository clipRepository,
            IClipRequestRepository clipRequestRepository,
            IInsightHandlerFactory insightHandlerFactory,
            IVideoUtilityService videoUtility,
            IAIProviderService aiProviderService,
            ILogger<ClipService> logger)
        {
            _clipRepository = clipRepository;
            _clipRequestRepository = clipRequestRepository;
            _videoUtilityService = videoUtility;
            _logger = logger;
            _aiProviderService = aiProviderService;
            _insightHandlerFactory = insightHandlerFactory;
        }

        public async Task<ClipSearchResponse> SearchClipsAsync(ClipSearchRequest request)
        {
            return await _clipRepository.SearchClipsAsync(request);
        }

        public async Task<Clip?> GetByIdAsync(string id)
        {
            return await _clipRepository.GetByIdAsync(id);
        }

        public async Task<Clip> CreateAsync(CreateClipsPayloadDto dto)
        {
            // Validation
            if (dto.ChannelIds == null || dto.ChannelIds.Count == 0)
                throw new ArgumentException("At least one channelId is required");

            var videoFileName = dto.VideoFileName ?? $"clip-{Guid.NewGuid()}.mp4";

            var clip = new Clip
            {
                Title = dto.Title,
                ChannelId = dto.ChannelIds.First(),
                ChannelName = dto.ChannelName ?? "Default Channel",
                VideoUrl = _videoUtilityService.GetVideoUrl(videoFileName),
                ThumbnailUrl = _videoUtilityService.GetThumbnailUrl(videoFileName),
                Duration = _videoUtilityService.GetVideoDuration(videoFileName),
                FileSize = _videoUtilityService.GetFileSize(videoFileName),
                Tags = dto.Tags ?? new List<string>(),
                ClipRequestId = dto.ClipRequestId
            };

            var createdClip = await _clipRepository.CreateAsync(clip);

            // Optional: link clip to request
            if (!string.IsNullOrEmpty(dto.ClipRequestId))
            {
                await _clipRequestRepository.AddClipIdAsync(
                    dto.ClipRequestId,
                    createdClip.Id
                );
            }

            return createdClip;
        }

        public async Task<Clip?> UpdateAsync(string id, UpdateClipDto dto)
        {
            var existing = await _clipRepository.GetByIdAsync(id);
            if (existing == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Title))
                existing.Title = dto.Title;

            if (dto.Tags != null)
                existing.Tags = dto.Tags;

            if (!string.IsNullOrEmpty(dto.Transcription))
                existing.Transcription = dto.Transcription;

            return await _clipRepository.UpdateAsync(id, existing);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _clipRepository.DeleteAsync(id);
        }

        public async Task<int> DeleteMultipleAsync(List<string> ids)
        {
            return await _clipRepository.DeleteMultipleAsync(ids);
        }

        public async Task<Clip> AddInsightsToClipAsync(Clip clip,List<InsightRequest> requests)
        {
            foreach (var request in requests)
            {
                var provider = _aiProviderService.GetProviderForRequest(request);
                if (provider == null)
                    continue;

                var handler = _insightHandlerFactory.GetHandler(request.InsightType);
                if (handler == null)
                    continue;

                // 1. Prepare input (polymorphic)
                var inputData = await handler.PrepareInputAsync(clip, request);
                if (inputData == null)
                    continue;

                // 2. Execute insight
                var insight = await provider.ProcessAsync(inputData, request);

                insight.Id ??= ObjectId.GenerateNewId().ToString();

                // 3. Attach to aggregate (encapsulated)
                clip.AddOrReplaceInsight(insight);
            }

            // 4. Persist once
            await _clipRepository.UpdateAsync(clip.Id, clip);

            return clip;
        }

        public async Task<Clip> RemoveInsightsAsync(Clip clip, List<string> insightIdsToDelete)
        {
            if (clip.Insights == null || !clip.Insights.Any())
                return clip;

            // Remove insights with matching IDs
            clip.Insights = clip.Insights
                                 .Where(insight => !insightIdsToDelete.Contains(insight.Id!))
                                 .ToList();

            // Persist changes
            await _clipRepository.UpdateAsync(clip.Id, clip);

            return clip;
        }
    }
}