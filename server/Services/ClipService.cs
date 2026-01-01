using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Server.DTOs;
using Server.InsightProviders;
using Server.Models;
using Server.Repositories;
using Server.Services;
using Server.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        Task<Clip> AddInsightsToClipAsync(Clip clip, List<InsightRequest> requests);
        Task<Clip> RemoveInsightsAsync(Clip clip, List<string> insightIdsToDelete);
        Task<FolderIngestionResultDto> IngestFolderAsync(FolderIngestionRequestDto request);
        Task<SemanticSearchResponseDto> SemanticSearchAsync(SemanticSearchDto request);
    }

    public class ClipService : IClipService
    {
        private readonly IClipRepository _clipRepository;
        private readonly IClipRequestRepository _clipRequestRepository;
        private readonly IVideoUtilityService _videoUtilityService;
        private readonly ILogger<ClipService> _logger;
        private readonly IAIProviderService _aiProviderService;
        private readonly IInsightInputBuilder _inputBuilder;
        private readonly IInsightDefinitionRepository _definitionRepo;
        private readonly IEnumerable<IInsightInputBuilder> _inputBuilders;
        private readonly IVectorDBRepository _vectorDbRepository;
        private readonly IEmbeddingProvider _embeddingProvider;
        private readonly IEntityExtractor _entityExtractor;
        private readonly IPromptComposer _promptComposer;
        private readonly string _videoOutputPath = "";
        private readonly string _thumbnailOutputPath = "";

        public ClipService(
                IEnumerable<IInsightInputBuilder> inputBuilders,
                IAIProviderService aiProviderService,
                IInsightDefinitionRepository definitionRepo,
                IClipRepository clipRepository,
                IConfiguration configuration,
                IVideoUtilityService videoUtilityService,
                IEmbeddingProvider embeddingProvider,
                IVectorDBRepository vectorDbRepository,
                IEntityExtractor entityExtractor,
                IPromptComposer promptComposer)
        {
            _inputBuilders = inputBuilders;
            _aiProviderService = aiProviderService;
            _definitionRepo = definitionRepo;
            _clipRepository = clipRepository;
            _videoUtilityService = videoUtilityService;
            _vectorDbRepository = vectorDbRepository;
            _embeddingProvider = embeddingProvider;
            _entityExtractor = entityExtractor;
            _videoOutputPath = configuration["Paths:Videos"] ?? @"C:\ACTUS_LIVEU\new-ai-demo-app\server\wwwroot\videos";
            _thumbnailOutputPath = configuration["Paths:Thumbnails"] ?? @"C:\ACTUS_LIVEU\new-ai-demo-app\server\wwwroot\thumbnails";
            _promptComposer = promptComposer;
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
                VideoUrl = _videoUtilityService.GetVideoUrl(dto.ChannelName!, videoFileName),
                ThumbnailUrl = _videoUtilityService.GetThumbnailUrl(dto.ChannelName!, videoFileName),
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

        public async Task<Clip> AddInsightsToClipAsync(Clip clip, List<InsightRequest> requests)
        {
            foreach (var request in requests)
            {
                var provider = _aiProviderService.GetProviderForRequest(request);
                if (provider == null)
                    continue;

                // 🔹 Pick the right input builder
                var builder = _inputBuilders.FirstOrDefault(b => b.CanBuild(request));
                if (builder == null)
                    continue;

                var input = await builder.BuildAsync(clip, request);
                if (input == null)
                    continue;

                // 🔹 Enrich ChatGPT prompt from definition (optional)
                if (request.InsightType == InsightTypes.ChatGPTPrompt && request.PromptText == null)
                {
                    var def = await _definitionRepo.GetByNameAsync(request.PromptName!);
                    if (def != null)
                        request.PromptText = def.PromptTemplate;
                }

                var insight = await provider.ProcessAsync(input, request);

                if (request.InsightType == InsightTypes.Transcription && insight is TranscriptionInsight transcriptionInsight)
                {
                    // --- Convert Transcript → TranscriptEx with absolute times
                    if (clip.BroadcastStartTime != null)
                    {
                        var enrichedTranscripts = transcriptionInsight.Transcripts
                            .Select(t => new TranscriptEx(
                                t.Text,
                                t.StartInSeconds,
                                t.EndInSeconds,
                                clip.BroadcastStartTime.Value.AddSeconds(t.StartInSeconds),
                                clip.BroadcastStartTime.Value.AddSeconds(t.EndInSeconds)
                            ))
                            .ToList();

                        transcriptionInsight.Transcripts = enrichedTranscripts;

                        await EmbedTranscriptionsAsync(enrichedTranscripts, clip);
                    }
                }

                clip.AddOrReplaceInsight(insight);
            }

            await _clipRepository.UpdateAsync(clip.Id, clip);

            return clip;
        }

        private async Task EmbedTranscriptionsAsync(List<TranscriptEx> transcripts, Clip clip)
        {
            if (transcripts == null || transcripts.Count == 0)
                return;

            if (clip.BroadcastStartTime == null || clip.BroadcastEndTime == null)
                return;

            const int chunkSize = 6; // group ~6 transcripts per embedding

            var windowedTranscripts = new List<TranscriptEx>();

            for (int i = 0; i < transcripts.Count; i += chunkSize)
            {
                var chunk = transcripts.Skip(i).Take(chunkSize).ToList();

                var combinedText = string.Join(" ", chunk.Select(t => t.Text));

                var absStart = chunk.First().AbsStartTime;
                var absEnd = chunk.Last().AbsEndTime;

                windowedTranscripts.Add(new TranscriptEx
                {
                    Text = combinedText,
                    StartInSeconds = chunk.First().StartInSeconds,
                    EndInSeconds = chunk.Last().EndInSeconds,
                    AbsStartTime = absStart,
                    AbsEndTime = absEnd
                });
            }

            // --- Store each chunk in VectorDB
            await _vectorDbRepository.StoreTranscriptsAsync(
                transcripts: windowedTranscripts,
                channelId: clip.ChannelId,
                clipBroadcastStartTime: clip.BroadcastStartTime.Value,
                clipBroadcastEndTime: clip.BroadcastEndTime.Value,
                mongoId: clip.Id // yes, the clip's Id
            );
        }

        //private async Task EmbedTranscriptionsAsync(List<TranscriptEx> transcripts, Clip clip)
        //{
        //    if (transcripts.Count == 0)
        //        return;

        //    if (clip.BroadcastStartTime == null || clip.BroadcastEndTime == null)
        //        return;

        //    await _vectorDbRepository.StoreTranscriptsAsync(
        //        transcripts: transcripts,
        //        channelId: clip.ChannelId,
        //        clipBroadcastStartTime: clip.BroadcastStartTime.Value,
        //        clipBroadcastEndTime: clip.BroadcastEndTime.Value,
        //        mongoId: clip.Id // ✅ Clip's Id
        //    );
        //}

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

        string ShortenFileName(string originalName, string channel)
        {
            var words = Path.GetFileNameWithoutExtension(originalName)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(6)
                .Select(w => Regex.Replace(w, @"[^\w\d]", "_")); // remove invalid chars

            return $"{string.Join("_", words)}_{channel}.mp4";

            //TODO SHOULD BE
            //var words = Path.GetFileNameWithoutExtension(originalName)
            //   .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            //   .Take(6)
            //   .Select(w => Regex.Replace(w, @"[^\w\d]", " ")); // remove invalid chars

            //return $"{string.Join("_", words)}.mp4";
        }
        private (DateTime startUtc, DateTime endUtc)? TryReadBroadcastTimes(string videoPath)
        {
            var infoPath = Path.ChangeExtension(videoPath, ".info.json");
            if (!File.Exists(infoPath))
                return null;

            using var doc = JsonDocument.Parse(File.ReadAllText(infoPath));
            var root = doc.RootElement;

            if (!root.TryGetProperty("timestamp", out var tsProp))
                return null;

            var uploadUtc = DateTimeOffset
                .FromUnixTimeSeconds(tsProp.GetInt64())
                .UtcDateTime;

            var duration = root.TryGetProperty("duration", out var durProp)
                ? durProp.GetInt32()
                : 0;

            return (uploadUtc, uploadUtc.AddSeconds(duration));
        }

        public async Task<FolderIngestionResultDto> IngestFolderAsync(FolderIngestionRequestDto request)
        {
            var result = new FolderIngestionResultDto();

            try
            {
                if (!Directory.Exists(request.RootFolderPath))
                {
                    result.Errors.Add($"Root folder not found: {request.RootFolderPath}");
                    return result;
                }

                var channelDirs = Directory.GetDirectories(request.RootFolderPath);

                foreach (var channelDir in channelDirs)
                {
                    var channelName = Path.GetFileName(channelDir);
                    var videoFiles = Directory.GetFiles(channelDir, "*.mp4");

                    // Optional: filter by FromDate
                    if (request.FromDate.HasValue)
                    {
                        videoFiles = videoFiles
                            .Where(f => File.GetCreationTime(f) >= request.FromDate.Value)
                            .ToArray();
                    }

                    int clipsAdded = 0;
                    foreach (var filePath in videoFiles)
                    {
                        if (request.MaxClipsPerChannel > 0 && clipsAdded >= request.MaxClipsPerChannel)
                            break;

                        try
                        {
                            var originalFileName = Path.GetFileName(filePath);
                            var fileDate = File.GetCreationTime(filePath);

                            var shortenedFileName = ShortenFileName(originalFileName, channelName);

                            // --- Copy video file to wwwroot/videos/channelName ---
                            var videoDestFolder = Path.Combine(_videoUtilityService.GetFilePathFromUrl("/videos"), channelName);
                            Directory.CreateDirectory(videoDestFolder);
                            var videoDestPath = Path.Combine(videoDestFolder, shortenedFileName);
                            File.Copy(filePath, videoDestPath, true);

                            // --- Generate thumbnail ---
                            var thumbFileName = Path.GetFileNameWithoutExtension(shortenedFileName) + ".jpg";
                            var thumbDestFolder = Path.Combine(_videoUtilityService.GetFilePathFromUrl("/thumbnails"), channelName);
                            Directory.CreateDirectory(thumbDestFolder);
                            var thumbUrl = await _videoUtilityService.GenerateThumbnailAsync(channelName, shortenedFileName, thumbFileName);


                            var infoTimes = TryReadBroadcastTimes(filePath);
                            DateTime? broadcastStart = infoTimes?.startUtc;
                            DateTime? broadcastEnd = infoTimes?.endUtc;

                            var clip = new Clip
                            {
                                Title = Path.GetFileNameWithoutExtension(shortenedFileName),
                                ChannelName = channelName,
                                ChannelId = channelName.ToLowerInvariant(),
                                CreatedAt = fileDate,
                                VideoUrl = _videoUtilityService.GetVideoUrl(channelName, shortenedFileName),
                                ThumbnailUrl = _videoUtilityService.GetThumbnailUrl(channelName, shortenedFileName),
                                Duration = _videoUtilityService.GetVideoDuration(Path.Combine(channelName, shortenedFileName)),
                                FileSize = _videoUtilityService.GetFileSize(Path.Combine(channelName, shortenedFileName)),

                                BroadcastStartTime = broadcastStart,
                                BroadcastEndTime = broadcastEnd,
                            };

                            await _clipRepository.CreateAsync(clip);

                            clipsAdded++;
                            result.TotalFilesProcessed++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing file {filePath}");
                            result.Errors.Add($"File: {filePath} -> {ex.Message}");
                            result.FilesSkipped++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting folder");
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<SemanticSearchResponseDto> SemanticSearchAsync(SemanticSearchDto request)
        {
            // 1️⃣ Extract intent
            QueryIntentContext context = await _entityExtractor.ExtractAsync(request.Query);
         
            context.OriginalQuery = request.Query;

            // 2️⃣ Build filter
            var filter = new TranscriptsFilter { Start = context.StartDate, End = context.EndDate, Channels = context.Channels };

            // 3️⃣ Vector search (REAL transcripts)
            var evidence = await _vectorDbRepository.SearchAsync(request.Query,filter,request.MaxEvidence);

            if (!evidence.Any())
            {
                return new SemanticSearchResponseDto
                {
                    Answer = "I couldn’t find any relevant transcript data for your question."
                };
            }

            // 4️⃣ Compose grounded prompt
            var (systemPrompt, evidenceText) = _promptComposer.Compose(context, evidence);

            // 5️⃣ Resolve provider
            var insightRequest = new InsightRequest
            {
                InsightType = InsightTypes.SemanticSearch
            };

            var provider = _aiProviderService.GetProviderForRequest(insightRequest) as OpenAIChatProvider ?? throw new InvalidOperationException("OpenAIChatProvider not available");

            // 6️⃣ Call LLM
            var answer = await provider.GetChatCompletionAsync(systemPrompt,evidenceText);

            // 7️⃣ Return structured response
            return new SemanticSearchResponseDto
            {
                Answer = answer,
                Evidence = evidence
            };
        }
    }
}