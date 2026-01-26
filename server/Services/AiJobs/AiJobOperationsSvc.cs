using Server.InsightProviders;
using Server.Models;
using Server.Models.AiJobs;
using Server.Models.AiJobs.Server.Models.AiJobs;
using Server.Repositories.AiJobs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Server.Services.AiJobs
{
    public enum InvocationType
    {
        Monitoring,
        Batch,
        Both,
    }
    public interface IAiJobOperationsSvc
    {
        Task ExecuteJobAsync(AiJobRequest job, CancellationToken token);
        //Task DetectLogosAsync(MediaPart mediaPart);
        Task<InsightResult> TranscribeFileAsync(string audioPath);
        Task<Dictionary<string, InsightResult>> TranslateTranscriptionAsync(AiJobRequest jobRequest, Channel channel, InsightResult sttInsightResult, string sttAudioLanguage, /*MediaPart mediaPart,*/ string transcriptionJobResultId);
        Task<List<TranscriptEx>> DetectKeywordsAsync(RunHistoryEntry currentRun, AiJobRequest jobRequest, InsightResult insightResult, int channelId, string sttJsonFile, InsightResult? translatedTranscript = null);
        Task CleanOldAIJobResultsSafeAsync();
    }

    public class AiJobOperationsSvc : IAiJobOperationsSvc
    {
        public Task CleanOldAIJobResultsSafeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<TranscriptEx>> DetectKeywordsAsync(RunHistoryEntry currentRun, AiJobRequest jobRequest, InsightResult insightResult, int channelId, string sttJsonFile, InsightResult? translatedTranscript = null)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteJobAsync(AiJobRequest job, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<InsightResult> TranscribeFileAsync(string audioPath)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, InsightResult>> TranslateTranscriptionAsync(AiJobRequest jobRequest, Channel channel, InsightResult sttInsightResult, string sttAudioLanguage, /*MediaPart mediaPart*/ string transcriptionJobResultId)
        {
            throw new NotImplementedException();
        }
    }
}

//        private readonly ILogger<AiJobOperationsSvc> _logger;
//        private readonly IAiJobRequestRepository _aiJobRequestRepository;
//        private readonly IAiJobResultRepository _aiJobResultRepository;
//        private readonly IAIProviderSvc _aiProviderSvc;
//        private List<string> _distinctKeywordsFound = new List<string>();
//        private readonly IAccountManagerConnector _accountManagerConnector;
//        private readonly IActDotnet4ResourcesSvc _actDotnet4ResourcesSvc;
//        private readonly ConcurrentDictionary<string, byte> _processedFiles = new();
//        private readonly IMediaContentSvc _mediaContentSvc;
//        private readonly ServiceManagerSvc _serviceManagerSvc;
//        private const int DEFAULT_LIVE_SEGMENT_DURATION = 10;
//        private readonly string _uiServer;
//        public AiJobOperationsSvc(ILogger<AiJobOperationsSvc> logger,
//                                  IAiJobRequestRepository aiJobRequestRepository,
//                                  IAiJobResultRepository aiJobResultRepository,
//                                  IAIProviderSvc aiProviderSvc,
//                                  IAccountManagerConnector accountManagerConnector, IActDotnet4ResourcesSvc actDotnet4ResourcesSvc, IConfigSettings config, IMediaContentSvc mediaContentSvc,
//                                  ServiceManagerSvc serviceManagerSvc)
//        {
//            _logger = logger;

//            _aiJobRequestRepository = aiJobRequestRepository;

//            _aiJobResultRepository = aiJobResultRepository;

//            _aiProviderSvc = aiProviderSvc;

//            _accountManagerConnector = accountManagerConnector;

//            _actDotnet4ResourcesSvc = actDotnet4ResourcesSvc;

//            _mediaContentSvc = mediaContentSvc;
//            _serviceManagerSvc = serviceManagerSvc;

//            _uiServer = Dns.GetHostName();
//        }

//        public async Task<InsightResult?> DetectGenericObjectsAsync(Channel channel, MediaPart mediaPart, string jobRequestId, string insightType, RunHistoryEntry currentRun)
//        {
//            string providerDisplayName = "-";
//            string tempDir = string.Empty;

//            try
//            {
//                if (!File.Exists(mediaPart.VideoFileLocation))
//                    return null;

//                double durationSec = (mediaPart.BroadcastEnd - mediaPart.BroadcastStart).TotalSeconds;
//                InsightInputData insightInputData = new()
//                {
//                    VideoInput = new VideoDTO() { FilePath = mediaPart.VideoFileLocation, DurationSec = durationSec }
//                };

//                InsightRequest insightRequest;

//                string systemInsightType = SystemInsightTypes.ObjectDetection;

//                if (insightType.ToLower() == SystemInsightTypes.FaceDetection.ToLower())
//                {
//                    tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
//                    Directory.CreateDirectory(tempDir);

//                    systemInsightType = SystemInsightTypes.FaceDetection;
//                    SettingsDm settings = await _serviceManagerSvc.GetSettingsAsync();
//                    insightInputData.FramesInput = await _mediaContentSvc.ExtractFramesFromVideoAsync(mediaPart.VideoFileLocation, tempDir, settings.CorsightFaceDetection.DetectionFps, settings.CorsightFaceDetection.FramesWidth);
//                }

//                if (insightType.ToLower() == SystemInsightTypes.LicensePlateDetection.ToLower())
//                    systemInsightType = SystemInsightTypes.LicensePlateDetection;

//                insightRequest = GetInsightRequest(systemInsightType);

//                var provider = GetProvider(insightInputData, insightRequest);

//                if (provider == null)
//                {
//                    return null;
//                }

//                providerDisplayName = provider.ProviderMetadata.DisplayName!;

//                var results = await provider.ProcessAsync(insightInputData, insightRequest);

//                if (results is null || results.Count == 0)
//                    return null;


//                _logger.LogDebug($"Detected {results[0].GenericObjectDetections?.Count} objects in video file: {insightInputData.VideoInput.FilePath}");

//                await SaveJobResultInDB(GetProviderType(provider.ProviderMetadata.DisplayName!), null, mediaPart, channel, Operation.DetectObjects, jobRequestId, mediaPart.VideoFileLocation, null, null, null, results[0]);

//                return results?[0];
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning($"[DetectGenericObjectsAsync] failed: {ex}");
//                string errorMessage = $"{providerDisplayName} failed to DetectGenericObjectsAsync. Error:{ex.Message}";
//                var stats = currentRun.Statistics.ChannelStatistics[mediaPart.Channel.Id.ToString()];

//                stats.AddError(errorMessage);
//            }
//            finally
//            {
//                // Clean up temp files
//                if (Directory.Exists(tempDir))
//                    await FileHelper.SafeDeleteFolderRecursiveAsync(tempDir);
//            }

//            return new InsightResult();
//        }

//        public async Task<List<TranscriptEx>> DetectKeywordsAsync(RunHistoryEntry currentRun, AiJobRequest job, InsightResult insightResult, int channelId, string sttJsonFile, InsightResult? translatedTranscript = null)
//        {
//            if (job.Keywords != null && job.Keywords.Count > 0)
//            {
//                List<TranscriptEx> keywordMatches = FindKeywordsAsync(currentRun, job, insightResult, job.Keywords, channelId, sttJsonFile, translatedTranscript);

//                //DBG File
//                //if (keywordMatches.Count > 0)
//                //{
//                //    string keywordFile = sttJsonFile.Replace(".json", "_keywords.json");

//                //    await SaveKeywordMatchesAsync(channelId, job, keywordFile, keywordMatches);
//                //}

//                return await Task.FromResult(keywordMatches);
//            }

//            return new List<TranscriptEx>();
//        }
//        private List<TranscriptEx> FindKeywordsAsync(RunHistoryEntry currentRun, AiJobRequest job, InsightResult insightResult, List<string> keywords, int channelId, string fileName, InsightResult? translatedTranscript = null)
//        {
//            var keywordMatches = new List<TranscriptEx>();

//            string channelIdStr = channelId.ToString();

//            var channelStats = currentRun.Statistics.ChannelStatistics;

//            var lowerCaseKeywords = keywords.Select(keyword => keyword.Trim().ToLower()).ToList();//! Trimmed and Lower cased

//            try
//            {
//                //Search in original transcripts (original audio language transcript)
//                foreach (var transcript in insightResult.TimeCodedContent!)
//                {
//                    foreach (var keyword in lowerCaseKeywords)
//                    {
//                        if (transcript.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
//                        {
//                            if (!_distinctKeywordsFound.Contains(keyword))
//                            {
//                                _distinctKeywordsFound.Add(keyword);
//                            }

//                            if (channelStats != null && channelStats.TryGetValue(channelIdStr, out var stats))
//                            {
//                                // Increment for every keyword occurrence
//                                stats.IncrementKeywordDetectedAlertsSent();
//                                stats.AddDistinctDetectedKW(keyword);
//                            }

//                            keywordMatches.Add(new TranscriptEx
//                            {
//                                Text = transcript.Text,
//                                StartInSeconds = transcript.StartInSeconds,
//                                EndInSeconds = transcript.EndInSeconds,
//                                Keyword = keyword
//                            });
//                        }
//                    }
//                }

//                //Search in translated transcripts 
//                if (translatedTranscript != null)
//                {
//                    foreach (var transcript in translatedTranscript.TimeCodedContent!)
//                    {
//                        foreach (var keyword in lowerCaseKeywords)
//                        {
//                            if (transcript.Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
//                            {
//                                if (!_distinctKeywordsFound.Contains(keyword))
//                                {
//                                    _distinctKeywordsFound.Add(keyword);
//                                }

//                                if (channelStats != null && channelStats.TryGetValue(channelIdStr, out var stats))
//                                {
//                                    // Increment for every keyword occurrence
//                                    stats.IncrementKeywordDetectedAlertsSent();
//                                    stats.AddDistinctDetectedKW(keyword);
//                                }

//                                //Add keyword only if it doesn't exit in original transcript!
//                                bool alreadyExists = keywordMatches.Any(match =>
//                                   match.StartInSeconds == transcript.StartInSeconds &&
//                                   match.EndInSeconds == transcript.EndInSeconds &&
//                                   match.Keyword == keyword);

//                                if (!alreadyExists)
//                                {
//                                    keywordMatches.Add(new TranscriptEx
//                                    {
//                                        Text = transcript.Text,
//                                        StartInSeconds = transcript.StartInSeconds,
//                                        EndInSeconds = transcript.EndInSeconds,
//                                        Keyword = keyword
//                                    });
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning($"FindKeywordsAsync failed: {ex.Message} stack: {ex.StackTrace}");
//            }

//            return keywordMatches;
//        }
//        public Task DetectLogosAsync(MediaPart mediaPart)
//        {
//            throw new NotImplementedException();
//        }
//        public async Task ExecuteJobAsync(AiJobRequest jobRequest, CancellationToken cancellationToken)
//        {
//            var currentRun = CreateNewRunHistory(jobRequest);

//            Stopwatch stopwatch = Stopwatch.StartNew();

//            try
//            {
//                List<Task> jobForEachChannel = new List<Task>();

//                foreach (int channelId in jobRequest.ChannelIds)
//                {
//                    Channel? channel = await _actDotnet4ResourcesSvc.GetChannelByIdSafeAsync(channelId);
//                    if (channel is null)
//                    {
//                        _logger.LogWarning($"Unable to get channel with ID {channelId} from mysql.");
//                        continue;
//                    }

//                    Task jobForChannel = RunJobForChannel(jobRequest, currentRun, cancellationToken, channel);

//                    jobForEachChannel.Add(jobForChannel);

//                }

//                await Task.WhenAll(jobForEachChannel);

//            }
//            catch (OperationCanceledException)
//            {
//                _logger.LogInformation($"ExecuteJobAsync: Job {jobRequest.Id} was canceled.");
//            }
//            catch (Exception ex)
//            {
//                string error = $"ExecuteJobAsync failed: {ex.Message} stack: {ex.StackTrace}";

//                _logger.LogError(error);

//                await UpdateJobStatus(jobRequest, JobStatus.Failed, error);
//            }

//            stopwatch.Stop();

//            UpdateRunResults(jobRequest, stopwatch, currentRun);
//        }

//        public InvocationType GetInvocationType(AiJobRequest jobRequest, RunHistoryEntry currentRun)
//        {
//            //Consider checking the actual existence of files rather the expected files according to the broadcast times
//            DateTime startTime = currentRun.BroadcastStartTime;
//            DateTime endTime = currentRun.BroadcastEndTime;
//            DateTime currentTime = DateTime.Now;

//            // Calculate the time when the first file is expected to be available
//            DateTime fileAvailableTime = startTime.AddMinutes(5);
//            // Calculate the time when the last file is expected to be available
//            DateTime lastFileAvailableTime = endTime.AddMinutes(5);

//            if (lastFileAvailableTime < currentTime)
//            {
//                // All files have been created and the creation window has passed
//                return InvocationType.Batch;
//            }
//            else if (fileAvailableTime > currentTime)
//            {
//                // The broadcast has started, but no files are available yet
//                return InvocationType.Monitoring;
//            }
//            else
//            {
//                // Some files are available, and more may be created in the future
//                return InvocationType.Both;
//            }
//        }
//        public bool IsExtractionSucceeded(AiJobRequest jobRequest, RunHistoryEntry currentRun, int channelId, MediaPart mediaPart)
//        {
//            if (!File.Exists(mediaPart.AudioFileLocation)) // Fail is considered if resulted .mp3 is less than 1MB/5MB
//            {
//                string errorMessage = $"Failed to Convert {mediaPart.VideoFileLocation}";

//                var stats = currentRun.Statistics.ChannelStatistics[channelId.ToString()];

//                stats.AddError(errorMessage);

//                _logger.LogWarning(errorMessage);

//                return false;
//            }
//            else
//            {
//                currentRun.Statistics.ChannelStatistics[channelId.ToString()].IncrementMp3FilesCreated();

//                return true;
//            }
//        }

//        public async Task<InsightResult> TranscribeFileAsync(string audioFileLocation)
//        {
//            try
//            {
//                var insightResult = await GetTranscriptionResult(SystemInsightTypes.Transcription, audioFileLocation);

//                return insightResult!;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning($"TranscribeFileAsync failed for {audioFileLocation}. {ex}");
//            }

//            return new InsightResult();
//        }
//        private async Task<InsightResult?> GetTranscriptionResult(string insightType, string audioFilePath)
//        {
//            var insightInputData = GetInsightInputData(audioFilePath);

//            InsightRequest insightRequest;

//            insightRequest = GetInsightRequest(insightType);

//            var provider = GetProvider(insightInputData, insightRequest);

//            if (provider == null)
//            {
//                return null;
//            }

//            var results = await provider.ProcessAsync(insightInputData, insightRequest);

//            return results?[0];
//        }
//        private static InsightInputData GetInsightInputData(string filePath)
//        {
//            InsightInputData insightInputData = new()
//            {
//                AudioInput = new AudioDTO { FilePath = filePath }
//            };

//            return insightInputData;
//        }
//        private static InsightRequest GetInsightRequest(string insightType, string? sourceLanguageEnglishName = null, string? targetLanguageEnglishName = null)
//        {
//            var insightRequest = new InsightRequest
//            {
//                InsightType = insightType,
//            };
//            if (sourceLanguageEnglishName != null && targetLanguageEnglishName != null)
//            {
//                insightRequest.AIParameters = new List<KeyValuePair<string, string>>();
//                insightRequest.AIParameters.Add(new KeyValuePair<string, string>(AIParametersKeys.SourceLanguageKey, sourceLanguageEnglishName));
//                insightRequest.AIParameters.Add(new KeyValuePair<string, string>(AIParametersKeys.TargetLanguageKey, targetLanguageEnglishName));
//            }
//            return insightRequest;
//        }
//        private ProviderBase? GetProvider(InsightInputData insightInputData, InsightRequest insightRequest)
//        {
//            List<ProviderBase>? aiProvidersForInsight = _aiProviderSvc!.GetAllAIProvidersForInsightProcessing(insightInputData, insightRequest);

//            if (aiProvidersForInsight is null || aiProvidersForInsight.Count == 0)
//                return null;

//            //Ignore actusCCTranscriber!
//            if (aiProvidersForInsight[0].ProviderMetadata.ProviderInternalId == "actusCCTranscriber")
//                return aiProvidersForInsight[1];
//            return aiProvidersForInsight[0];
//        }
//        private RunHistoryEntry CreateNewRunHistory(AiJobRequest job)
//        {
//            _distinctKeywordsFound = new List<string>();

//            var broadcastStartTime = new DateTime(job.NextScheduledTime!.Value.Year, job.NextScheduledTime!.Value.Month, job.NextScheduledTime!.Value.Day, job.BroadcastStartTime.Hour, job.BroadcastStartTime.Minute, job.BroadcastStartTime.Second);

//            var broadcastendTime = new DateTime(job.NextScheduledTime!.Value.Year, job.NextScheduledTime!.Value.Month, job.NextScheduledTime!.Value.Day, job.BroadcastEndTime.Hour, job.BroadcastEndTime.Minute, job.BroadcastEndTime.Second);

//            var newRun = new RunHistoryEntry
//            {
//                ActualRunStartTime = DateTime.Now,
//                ActualRunEndTime = DateTime.MinValue, // Will be updated after processing
//                BroadcastStartTime = broadcastStartTime,
//                BroadcastEndTime = broadcastendTime,
//                Statistics = new ResultStatistics
//                {
//                    ChannelStatistics = job.ChannelIds.ToDictionary(channelId => channelId.ToString(), channelId => new ChannelResultStatistics())
//                }
//            };

//            job.RunHistory.Insert(0, newRun);

//            return newRun;
//        }
//        /// <summary>
//        /// put here both strored content and live processing.
//        /// </summary>
//        /// <param name="jobRequest"></param>
//        /// <param name="currentRun"></param>
//        /// <param name="cancellationToken"></param>
//        /// <param name="channel"></param>
//        /// <returns></returns>
//        private async Task RunJobForChannel(AiJobRequest jobRequest, RunHistoryEntry currentRun, CancellationToken cancellationToken, Channel channel)
//        {
//            cancellationToken.ThrowIfCancellationRequested(); // Check if cancellation was requested

//            if (jobRequest.NextScheduledTime is null)
//                return;

//            if (jobRequest.IsContinuousJob())
//            {
//                DateTime now = DateTime.Now;
//                currentRun.BroadcastStartTime = now.AddMilliseconds(-1 * now.Millisecond);
//                currentRun.BroadcastEndTime = currentRun.BroadcastStartTime.AddDays(360);  // ???
//            }

//            var broadcastStartTime = currentRun.BroadcastStartTime;
//            var broadcastEndTime = currentRun.BroadcastEndTime;

//            DateTime mediaPartsNextBroadcastStartTime = broadcastStartTime;

//            //here we could have both stored content (5m) and also live content (10s mp4)

//            List<MediaPart> existingMp4s = _mediaContentSvc.GetExistingChuncksSafe(channel, broadcastStartTime, broadcastEndTime, jobRequest.Id!);

//            foreach (MediaPart mediaPart in existingMp4s)
//            {
//                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation between each file

//                await ProcessFileSafeAsync(currentRun, mediaPart, jobRequest, channel);
//            }

//            if (existingMp4s.Count > 0)
//                mediaPartsNextBroadcastStartTime = existingMp4s.Select(p => p.BroadcastEnd).Max();

//            DateTime lastFileAvailableTime = broadcastEndTime.AddMinutes(6); // The last file is created at least 5 minutes after the BroadcastEndTime and add an extra 1m to be sure...

//            //if we already have the last file, then the job is done for this channel.

//            if (mediaPartsNextBroadcastStartTime >= broadcastEndTime)
//            {
//                _logger.LogDebug($"All mp4 were in stored content. Job {jobRequest.Id}-{jobRequest.Name} is done for channel {channel.Id}");
//                return;
//            }

//            var processedFiles = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // Thread-safe collection to track already processed files

//            int livePartDurationSecond = await GetAiJobLiveSegmentDurationSecAsync();

//            TimeSpan pollingInterval = TimeSpan.FromSeconds(1); // Polling interval

//            //todo. for continuous job, increase the mediaPartsNextBroadcastStartTime in case there is no content for X tries ... advance with start time ... 

//            SettingsDm settings = await _serviceManagerSvc.GetSettingsAsync();
//            int filesPerChannelProcessedForContinuousJob = 0;

//            while (DateTime.Now <= lastFileAvailableTime)
//            {
//                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation before each polling iteration

//                List<MediaPart> mediaParts = await _mediaContentSvc.GetLiveChuncksSafeAsync(channel, mediaPartsNextBroadcastStartTime, broadcastEndTime, jobRequest.Id!, livePartDurationSecond);

//                if (mediaParts.Count > 0)
//                    mediaPartsNextBroadcastStartTime = mediaParts.Select(p => p.BroadcastEnd).Max();

//                foreach (MediaPart mediaPart in mediaParts)
//                {
//                    string key = $"{mediaPart.Channel.Id}_{mediaPart.Channel.InternalName}_{mediaPart.BroadcastStart}_{mediaPart.BroadcastEnd}";
//                    if (processedFiles.ContainsKey(key)) // Skip already processed files
//                        continue;

//                    processedFiles[key] = true;
//                    // _ = ProcessFileSafeAsync(currentRun, mediaPart, jobRequest, channel);


//                    _ = Task.Run(async () => await ProcessFileSafeAsync(currentRun, mediaPart, jobRequest, channel));

//                }

//                //check if we got the final file. if yes, we can stop polling.
//                if (mediaPartsNextBroadcastStartTime >= broadcastEndTime)
//                {
//                    _logger.LogDebug($"Processing of live files completed for Job {jobRequest.Id}-{jobRequest.Name}; channel {channel.Id}");
//                    return;
//                }

//                pollingInterval = TimeSpan.FromSeconds(1);

//                if (!jobRequest.IsContinuousJob())
//                {
//                    await Task.Delay(pollingInterval, cancellationToken); // Pass the token to Task.Delay for responsive cancellation
//                    continue;
//                }

//                // we have a continuous jobs 
//                // For continuous jobs, we want to limit the number of files we process...
//                filesPerChannelProcessedForContinuousJob += mediaParts.Count;

//                if (filesPerChannelProcessedForContinuousJob >= settings.MaxFilesPerChannelToProcessForContinuousAiJob)
//                {
//                    _logger.LogWarning($"Processed {filesPerChannelProcessedForContinuousJob} files for channel {channel.Id} for continuous job {jobRequest.Id}. Stopping further processing to avoid overload.");
//                    break;
//                }

//                await Task.Delay(pollingInterval, cancellationToken); // Pass the token to Task.Delay for responsive cancellation
//            }
//        }

//        private async Task<int> GetAiJobLiveSegmentDurationSecAsync()
//        {
//            int? liveSegmentDuration = await _serviceManagerSvc.GetAiJobLiveSegmentDurationSecAsync();

//            if (liveSegmentDuration is null || liveSegmentDuration < 0)
//                liveSegmentDuration = DEFAULT_LIVE_SEGMENT_DURATION;

//            return (int)liveSegmentDuration;
//        }
//        private async Task UpdateJobStatus(AiJobRequest job, string status, string error = "")
//        {
//            await _aiJobRequestRepository.UpdateJobStatusAsync(job, status, error);
//        }

//        private async void UpdateRunResults(AiJobRequest jobRequest, Stopwatch stopwatch, RunHistoryEntry currentRun)
//        {
//            currentRun.ActualRunEndTime = DateTime.Now;

//            currentRun.Statistics.ProcessDurationInMinutes = stopwatch.Elapsed.TotalMinutes;

//            _logger.LogDebug($"Finished Executing JOB ID: {jobRequest.Id}, Elapsed Time: {stopwatch.Elapsed.TotalMinutes} minutes.");

//            await _aiJobRequestRepository.UpdateRunHistoryAsync(jobRequest.Id!, currentRun);
//        }
//        private async Task HandleDetectFacesAsync(Channel channel, string jobRequestId, MediaPart mediaPart, RunHistoryEntry currentRun)
//        {
//            InsightResult? faceInsightResult = await DetectGenericObjectsAsync(channel, mediaPart, jobRequestId, SystemInsightTypes.FaceDetection, currentRun);
//            if (faceInsightResult is null)
//                return;
//        }

//        private async Task HandleDetectLicensePlatesAsync(Channel channel, string jobRequestId, MediaPart mediaPart, RunHistoryEntry currentRun)
//        {
//            _logger.LogDebug($"Starting HandleDetectLicensePlatesAsync Detection for file: {mediaPart.BroadcastStart.ToString("HH:mm:ss.fff")} - {mediaPart.BroadcastEnd.ToString("HH:mm:ss.fff")}");

//            Stopwatch sw = Stopwatch.StartNew();
//            InsightResult? faceInsightResult = await DetectGenericObjectsAsync(channel, mediaPart, jobRequestId, SystemInsightTypes.LicensePlateDetection, currentRun);
//            sw.Stop();

//            _logger.LogDebug($"DONE HandleDetectLicensePlatesAsync for file: {mediaPart.BroadcastStart.ToString("HH:mm:ss.fff")} - {mediaPart.BroadcastEnd.ToString("HH:mm:ss.fff")} took {sw.Elapsed.TotalSeconds} secs. Offset from NOW: {(DateTime.Now - mediaPart.BroadcastEnd).TotalSeconds} sec");

//            if (faceInsightResult is null)
//                return;
//        }

//        private async Task HandleDetectObjectsAsync(Channel channel, string jobRequestId, MediaPart mediaPart, RunHistoryEntry currentRun)
//        {
//            _logger.LogDebug($"Starting Object Detection for file: {mediaPart.BroadcastStart.ToString("HH:mm:ss.fff")} - {mediaPart.BroadcastEnd.ToString("HH:mm:ss.fff")}");

//            Stopwatch sw = Stopwatch.StartNew();
//            InsightResult? objectInsightResult = await DetectGenericObjectsAsync(channel, mediaPart, jobRequestId, SystemInsightTypes.ObjectDetection, currentRun);

//            sw.Stop();
//            _logger.LogDebug($"Starting Object Detection for file: {mediaPart.BroadcastStart.ToString("HH:mm:ss.fff")} - {mediaPart.BroadcastEnd.ToString("HH:mm:ss.fff")} took {sw.Elapsed.TotalSeconds} secs");

//            if (objectInsightResult is null)
//                return;
//        }

//        private async Task<(bool isSuccess, InsightResult? insightResult, string? transcriptionJobResultId)> HandleTranscriptionAsync(AiJobRequest jobRequest, MediaPart mediaPart, Channel channel, RunHistoryEntry currentRun)
//        {
//            // Attempt transcription
//            var insightResult = await TranscribeFileAsync(mediaPart.AudioFileLocation);

//            // Check if transcription was successful
//            if (insightResult?.TimeCodedContent == null || !insightResult.TimeCodedContent.Any())
//            {
//                // Retry transcription once
//                insightResult = await TranscribeFileAsync(mediaPart.AudioFileLocation);

//                // If retry fails, log error and update statistics
//                if (insightResult?.TimeCodedContent == null || !insightResult.TimeCodedContent.Any())
//                {
//                    string errorMessage = $"Failed to Transcribe {mediaPart.AudioFileLocation}, After Retry!";

//                    var channelStats = currentRun.Statistics.ChannelStatistics[channel.Id.ToString()];
//                    channelStats.AddError(errorMessage);

//                    _logger.LogWarning(errorMessage);

//                    return (false, null, null);
//                }
//            }

//            // Log detected audio language
//            currentRun.Statistics.ChannelStatistics[channel.Id.ToString()].AddDistinctAudioLanguages(insightResult.Language);

//            // Convert transcripts and save job result in the database
//            var timeCodedContentAsTranscriptEx = ConvertTranscriptsToTranscriptsEx(insightResult.TimeCodedContent!);

//            var transcriptionJobResultId = await SaveJobResultInDB(ProviderType.None, timeCodedContentAsTranscriptEx, mediaPart, channel, Operation.Transcription, jobRequest.Id!, mediaPart.VideoFileLocation, insightResult.Language);

//            return (true, insightResult, transcriptionJobResultId);
//        }
//        private async Task<Dictionary<string, InsightResult>?> HandleTranslateTranscriptionAsync(AiJobRequest jobRequest, Channel channel, InsightResult insightResult, MediaPart mediaPart, string transcriptionJobResultId, Dictionary<string, InsightResult>? translatedTranscripts, RunHistoryEntry currentRun)
//        {
//            if (jobRequest.Operations.Contains(Operation.TranslateTranscription) && jobRequest.TranslationLanguages != null && jobRequest.TranslationLanguages.Count > 0)
//            {
//                translatedTranscripts = await TranslateTranscriptionAsync(jobRequest, channel, insightResult, insightResult.Language, mediaPart, transcriptionJobResultId);

//                if (translatedTranscripts.Count < jobRequest.TranslationLanguages.Count - 1)
//                {
//                    //-1 because it maybe that one of the translation languages is the audio lng which we skip
//                    string errorMessage = $"Failed to Translate one or more languages!";

//                    currentRun.Statistics.ChannelStatistics[channel.Id.ToString()].AddError(errorMessage);

//                    _logger.LogWarning(errorMessage);
//                }

//                LogTranslatedLanguages(translatedTranscripts.Keys, currentRun, channel.Id);

//                return translatedTranscripts;
//            }
//            return null;
//        }
//        private async Task HandleDetectKeywordsAsync(AiJobRequest jobRequest, InsightResult insightResult, MediaPart mediaPart, Channel channel, Dictionary<string, InsightResult>? translatedTranscripts, RunHistoryEntry currentRun)
//        {
//            //In case of a MIX - process file only once!
//            if (_processedFiles.ContainsKey(mediaPart.MetadataFileLocation))
//            {
//                _logger.LogInformation($"Skipping already processed file: {mediaPart.MetadataFileLocation}");
//                return;
//            }

//            _processedFiles.TryAdd(mediaPart.MetadataFileLocation, 0); // Mark file as processed

//            if (!jobRequest.Operations.Contains(Operation.DetectKeywords))
//                return;

//            InsightResult? translatedTranscript = null;

//            if (jobRequest.KeywordsLangauges != null && jobRequest.KeywordsLangauges.Count > 0 && translatedTranscripts != null)
//            {
//                var keywordsLanguage = jobRequest.KeywordsLangauges[0];

//                // Add the translated transcripts only if the translater language and the keywordsLanguage are different!
//                if (!string.Equals(keywordsLanguage, insightResult.Language, StringComparison.OrdinalIgnoreCase))
//                {
//                    // Try to get the translated transcript for the specified language
//                    if (!translatedTranscripts.TryGetValue(keywordsLanguage, out translatedTranscript))
//                    {
//                        _logger.LogWarning($"No translated transcript found for language: {keywordsLanguage}. Using the original transcript.");
//                    }
//                }
//            }

//            List<TranscriptEx> keywordMatches = await DetectKeywordsAsync(currentRun, jobRequest, insightResult, channel.Id, mediaPart.MetadataFileLocation, translatedTranscript);

//            if (keywordMatches != null && keywordMatches.Count > 0)
//            {
//                await SendKeywordsNotificationEmail(jobRequest, keywordMatches, channel, mediaPart);

//                await SaveJobResultInDB(ProviderType.None, keywordMatches, mediaPart, channel, Operation.DetectKeywords, jobRequest.Id!, mediaPart.VideoFileLocation, insightResult.Language);
//            }
//        }
//        private async Task SendKeywordsNotificationEmail(AiJobRequest jobRequest, List<TranscriptEx> keywordMatches, Channel channel, MediaPart mediaPart)
//        {
//            if (jobRequest.NotificationIds.Count == 0 || keywordMatches == null || !keywordMatches.Any())
//                return;

//            string subject = $"Actus Keyword Detection: Keyword(s) detected in job {jobRequest.Name}";

//            DateTime start = mediaPart.BroadcastStart;
//            DateTime end = start.AddMinutes(5); // End time is start time plus 5 minutes

//            // Compose email body
//            string body = $"<b>Job Name</b>: {jobRequest.Name} <br/>" +
//                          $"<b>Created at</b>: {jobRequest.CreatedAt:MM/dd/yyyy HH:mm:ss} <br/>" +
//                          $"<b>Channel</b>: {channel.DisplayName} <br/><br/>" +
//                          "The following keyword(s) were detected:<br/><br/>" +
//                          "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>" +
//                          //"<tr><th>Keyword</th><th>Text</th><th>Start Time</th><th>End Time</th></tr>";
//                          "<tr><th>Keyword</th><th>Text</th><th>Start Time</th></tr>";//no End Time

//            foreach (var keywordMatch in keywordMatches)
//            {
//                if (keywordMatch.Keyword is null)
//                    continue;

//                DateTime keywordStartTime = start.AddSeconds(keywordMatch.StartInSeconds);
//                // DateTime keywordEndTime = start.AddSeconds(keywordMatch.EndInSeconds);

//                string highlightedKeyword = $"<span style='color: blue; font-weight: bold;'>{keywordMatch.Keyword}</span>";

//                // Use Regex to replace keyword (case-insensitive)
//                string highlightedText = System.Text.RegularExpressions.Regex.Replace(
//                    keywordMatch.Text,
//                    System.Text.RegularExpressions.Regex.Escape(keywordMatch.Keyword),
//                    $"<span style='color: blue; font-weight: bold;'>$0</span>",
//                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
//                );

//                body += $"<tr>" +
//                        $"<td>{highlightedKeyword}</td>" +
//                        $"<td>{highlightedText}</td>" +
//                        $"<td>{keywordStartTime:HH:mm:ss}</td>" +
//                        //$"<td>{keywordEndTime:HH:mm:ss}</td>" +
//                        $"</tr>";
//            }

//            body += "</table><br/><br/>" +
//                    "You can view the detected keywords directly in the video by clicking the link below:<br/>" +
//                    $"<a href='http://{_uiServer}/actus5/{channel.Id}?from={start:yyyy_MM_dd_HH_mm_ss}&to={end:yyyy_MM_dd_HH_mm_ss}&startoffset=120&keywordsFound=true&jobRequestId={jobRequest.Id}'>View Keywords in Video</a>";

//            // Prepare notification DTO
//            GlobalNotificationSendDTO globalNotificationSendDTO = new();
//            globalNotificationSendDTO.NotificationDestinationsIds = new();
//            globalNotificationSendDTO.NotificationDestinationsIds.AddRange(jobRequest.NotificationIds);

//            globalNotificationSendDTO.NotificationParams = new Dictionary<string, string>()
//            {
//                { NotificationParamKeys.Subject.ToString(), subject },
//                { NotificationParamKeys.Body.ToString(), body }
//            };

//            // Send the notification
//            await _accountManagerConnector.SendNotificationDestinationAsync(globalNotificationSendDTO);
//        }

//        private void LogTranslatedLanguages(object keys, RunHistoryEntry currentRun, int channelId)
//        {
//            if (keys is IEnumerable<string> languageKeys)
//            {
//                foreach (var language in languageKeys)
//                {
//                    currentRun.Statistics.ChannelStatistics[channelId.ToString()].AddDistinctTranslatedLanguages(language);
//                }
//            }
//        }
//        private List<TranscriptEx> ConvertTranscriptsToTranscriptsEx(List<Transcript> transcripts)
//        {
//            List<TranscriptEx> transcriptsExs = new List<TranscriptEx>();
//            foreach (var transcript in transcripts)
//            {
//                var transcriptEx = new TranscriptEx
//                {
//                    Text = transcript.Text,
//                    StartInSeconds = transcript.StartInSeconds,
//                    EndInSeconds = transcript.EndInSeconds,
//                };
//                transcriptsExs.Add(transcriptEx);
//            }
//            return transcriptsExs;
//        }

//        /// <summary>
//        /// This method will process all Operations for a given file - sequential
//        /// possible improvement - process in parallel the Operations that are not dependent on each other
//        /// </summary>
//        /// <param name="currentRun"></param>
//        /// <param name="mediaPart"></param>
//        /// <param name="jobRequest"></param>
//        /// <param name="channel"></param>
//        /// <returns></returns>
//        private async Task ProcessFileSafeAsync(RunHistoryEntry currentRun, MediaPart mediaPart, AiJobRequest jobRequest, Channel channel)
//        {
//            try
//            {
//                var stopwatch = Stopwatch.StartNew();

//                List<Task> videoTasks = new List<Task>();

//                if (jobRequest.Operations.Contains(Operation.DetectFaces))
//                    videoTasks.Add(HandleDetectFacesAsync(channel, jobRequest.Id!, mediaPart, currentRun));

//                if (jobRequest.Operations.Contains(Operation.DetectObjects))
//                    videoTasks.Add(HandleDetectObjectsAsync(channel, jobRequest.Id!, mediaPart, currentRun));

//                if (jobRequest.Operations.Contains(Operation.DetectLicensePlates))
//                    videoTasks.Add(HandleDetectLicensePlatesAsync(channel, jobRequest.Id!, mediaPart, currentRun));

//                if (jobRequest.Operations.Contains(Operation.DetectLogo))
//                    videoTasks.Add(DetectLogosAsync(mediaPart));

//                Task.WaitAll(videoTasks.ToArray());

//                if (!(jobRequest.Operations.Contains(Operation.CreateClosedCaptions) ||
//                    jobRequest.Operations.Contains(Operation.DetectKeywords) ||
//                    jobRequest.Operations.Contains(Operation.TranslateTranscription) ||
//                    jobRequest.Operations.Contains(Operation.VerifyAudioLanguage)))
//                {
//                    //take care of srtatistics, deleting media, logs and return.
//                    currentRun.Statistics.ChannelStatistics[channel.Id.ToString()].IncrementMp4FilesProcessed();
//                    _mediaContentSvc.DeleteMediaSafe(mediaPart);
//                    stopwatch.Stop();
//                    _logger.LogDebug($"Total aijob processing time for {(mediaPart.BroadcastEnd - mediaPart.BroadcastStart).TotalSeconds}s: {mediaPart.VideoFileLocation}: {stopwatch.Elapsed.TotalSeconds} seconds");

//                    return; //no other known operations remained to process
//                }

//                currentRun.Statistics.ChannelStatistics[channel.Id.ToString()].IncrementMp4FilesProcessed();

//                if (!File.Exists(mediaPart.AudioFileLocation))
//                {
//                    var stopwatchExtractAudio = Stopwatch.StartNew();

//                    await _mediaContentSvc.ExtractAudioSafeAsync(mediaPart);

//                    stopwatchExtractAudio.Stop();
//                    _logger.LogDebug($"Audio extraction took: {stopwatchExtractAudio.Elapsed.TotalSeconds} seconds");
//                }

//                var isExtractionSucceeded = IsExtractionSucceeded(jobRequest, currentRun, channel.Id, mediaPart);

//                if (!isExtractionSucceeded)
//                {
//                    _mediaContentSvc.DeleteMediaSafe(mediaPart);
//                    return;
//                }

//                var (isSuccess, insightResult, transcriptionJobResultId) = await HandleTranscriptionAsync(jobRequest, mediaPart, channel, currentRun);

//                if (!isSuccess || insightResult == null || transcriptionJobResultId == null)
//                {
//                    _mediaContentSvc.DeleteMediaSafe(mediaPart);
//                    return;
//                }

//                _logger.LogDebug($"Transcription took: {stopwatch.Elapsed.TotalSeconds} seconds");

//                var translatedTranscripts = await HandleTranslateTranscriptionAsync(jobRequest, channel, insightResult, mediaPart, transcriptionJobResultId, null/*translatedTranscripts*/, currentRun);

//                await HandleDetectKeywordsAsync(jobRequest, insightResult, mediaPart, channel, translatedTranscripts, currentRun);

//                string status = await _mediaContentSvc.HandleCreateClosedCaptionsAsync(jobRequest, mediaPart, insightResult, channel, translatedTranscripts);
//                if (!string.IsNullOrEmpty(status))
//                {
//                    jobRequest.RunHistory[0].Statistics.ChannelStatistics[channel.Id.ToString()]!.AddError(status);
//                }

//                _mediaContentSvc.DeleteMediaSafe(mediaPart);

//                stopwatch.Stop();

//                _logger.LogDebug($"Total processing time for {(mediaPart.BroadcastEnd - mediaPart.BroadcastStart).TotalSeconds}s: {mediaPart.VideoFileLocation}: {stopwatch.Elapsed.TotalSeconds} seconds");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning($"[ProcessFileAsync] Error processing {mediaPart.VideoFileLocation}: {ex.Message}, Stack Trace: {ex.StackTrace}");
//            }
//        }

//        private List<TranscriptEx>? ConvertToTranscriptExList(List<TranscriptEx>? transcripts, DateTime baseStartTime)
//        {
//            if (transcripts is null)
//                return null;

//            return transcripts.Select(transcript => new TranscriptEx
//            {
//                Text = transcript.Text,
//                StartInSeconds = transcript.StartInSeconds,
//                EndInSeconds = transcript.EndInSeconds,
//                StartTime = baseStartTime.AddSeconds(transcript.StartInSeconds),
//                EndTime = baseStartTime.AddSeconds(transcript.EndInSeconds),
//                Keyword = transcript.Keyword,
//            }).ToList();
//        }

//        private async Task<string> SaveJobResultInDB(ProviderType providerType, List<TranscriptEx>? content, MediaPart mediaPart, Channel channel,
//                string operation, string jobRequestId, string? filePath = null, string? audioLanguage = null, string? translationLanguage = null, string? transcriptionJobResultId = null, InsightResult? insightResult = null)
//        {
//            DateTime start = mediaPart.BroadcastStart;
//            DateTime end = mediaPart.BroadcastEnd;

//            var cnt = ConvertToTranscriptExList(content, start);
//            List<BoundingBoxObject>? detectedObjects = await CreateBoundingBoxObjectsFromDetectionsAsync(mediaPart, insightResult?.GenericObjectDetections);

//            var jobResult = new JobResult()
//            {
//                ChannelId = channel.Id,
//                ChannelDisplayName = channel.DisplayName,
//                AiJobRequestId = jobRequestId,
//                Status = JobStatus.Completed,
//                Content = cnt,
//                DetectedObjects = detectedObjects,
//                Operation = operation,
//                Start = start,
//                End = end,
//                FilePath = filePath,
//                AudioLanguage = audioLanguage,
//                TranslationLanguage = translationLanguage,
//                TranscriptionJobResultId = transcriptionJobResultId,
//                ProviderType = providerType,
//                AIEngine = insightResult?.AIProviderId,
//                AIEngineResultRaw = insightResult?.AIEngineResultRaw,
//            };
//            await this._aiJobResultRepository.SaveJobResultAsync(jobResult);

//            //after we have the detected objects, save them to the db for listing operation.
//            //do not wait for this to complete.
//            // _ = SaveDetectedObjectsAsync(detectedObjects);

//            return jobResult.Id!;
//        }

//        private OverlayStyle GetOverlayStyle(SettingsDm settings, BoundingBoxObjectType boundingBoxObjectType, string style)
//        {
//            if (boundingBoxObjectType == BoundingBoxObjectType.Face)
//            {
//                if (style == "known")
//                    return settings.CorsightFaceDetection.DetectionStyleKnown;
//                if (style == "lowconfidence")
//                    return settings.CorsightFaceDetection.DetectionStyleLowConfidence;

//                return settings.CorsightFaceDetection.DetectionStyle;
//            }

//            if (boundingBoxObjectType == BoundingBoxObjectType.LicensePlate)
//            {
//                if (style == "known")
//                    return settings.LicensePlate.DetectionStyleKnown;
//                if (style == "lowconfidence")
//                    return settings.LicensePlate.DetectionStyleLowConfidence;

//                return settings.LicensePlate.DetectionStyle;
//            }

//            return settings.LicensePlate.DetectionStyle;
//        }

//        private async Task<List<BoundingBoxObject>?> CreateBoundingBoxObjectsFromDetectionsAsync(MediaPart mediaPart, List<BoundingBoxDetection>? boundingBoxDetections)
//        {
//            if (boundingBoxDetections is null)
//                return null;

//            List<BoundingBoxObject> faceObjects = new List<BoundingBoxObject>();

//            SettingsDm settings = await _serviceManagerSvc.GetSettingsAsync();

//            foreach (var boundingboxDetection in boundingBoxDetections)
//            {
//                string description = boundingboxDetection.Description;

//                float left = (float)boundingboxDetection.xCoordinatePercentage / 100f;
//                float top = (float)boundingboxDetection.yCoordinatePercentage / 100f;
//                float right = (float)(boundingboxDetection.xCoordinatePercentage + boundingboxDetection.WidthPercentage) / 100f;
//                float bottom = (float)(boundingboxDetection.yCoordinatePercentage + boundingboxDetection.HeightPercentage) / 100f;

//                bool known = false;
//                foreach (string knwonplate in settings.LicensePlate.DetectionKnownPlates)
//                {
//                    if (boundingboxDetection.Description.StartsWith(knwonplate))
//                    {
//                        known = true;
//                        break;
//                    }
//                }

//                //decide overlay style.
//                OverlayStyle overlayStyle = GetOverlayStyle(settings, boundingboxDetection.ObjectType, "default");

//                if (boundingboxDetection.ObjectType == BoundingBoxObjectType.LicensePlate && boundingboxDetection.Confidence < settings.LicensePlate.DetectionConfidenceTreshold)
//                {
//                    description = "";//do not show description if low confidence. we will not show these in the list - only as overlay over the video
//                    overlayStyle = settings.LicensePlate.DetectionStyleLowConfidence;
//                }

//                if (boundingboxDetection.ObjectType == BoundingBoxObjectType.Face && boundingboxDetection.Confidence < settings.CorsightFaceDetection.DetectionConfidenceTreshold)
//                {
//                    description = "";//do not show description if low confidence. we will not show these in the list - only as overlay over the video
//                    overlayStyle = settings.CorsightFaceDetection.DetectionStyleLowConfidence;
//                }

//                if (known)
//                    overlayStyle = GetOverlayStyle(settings, boundingboxDetection.ObjectType, "known");


//                DateTime TimestampStartForFace = mediaPart.BroadcastStart.AddMilliseconds(boundingboxDetection.VideoTimeOffsetMillis);
//                if (boundingboxDetection.DurationMillis == 0)
//                {
//                    boundingboxDetection.DurationMillis = 40;
//                }
//                DateTime TimestampEndForFace = TimestampStartForFace.AddMilliseconds(boundingboxDetection.DurationMillis);

//                //string debugData = $"{boundingboxDetection.Description}({boundingboxDetection.Confidence.ToString("N02")})|{TimestampStartForFace.ToString("mm:ss.fff")}-{TimestampEndForFace.ToString("mm:ss.fff")}";
//                string debugData = $"{boundingboxDetection.Description}({boundingboxDetection.Confidence.ToString("N02")})";

//                var faceObject = new BoundingBoxObject
//                {
//                    ChannelDisplayName = mediaPart.Channel.DisplayName,
//                    ChannelId = mediaPart.Channel.Id,
//                    Confidence = boundingboxDetection.Confidence,
//                    ObjectType = boundingboxDetection.ObjectType,
//                    OverlayStyle = overlayStyle,
//                    ImageDataBase64 = boundingboxDetection.ImageDataBase64,
//                    TimestampStart = TimestampStartForFace,
//                    TimestampEnd = TimestampEndForFace,
//                    NormalizedBoundingBox = new BoundingBox()
//                    {
//                        Top = top,
//                        Left = left,
//                        Right = right,
//                        Bottom = bottom
//                    },
//                    Description = description,
//                    Debug = debugData,
//                    AIEngineResult = new AIEngineResult
//                    {
//                        ObjectId = boundingboxDetection.SubjectID.ToString(),
//                    }
//                };
//                faceObjects.Add(faceObject);
//            }

//            return faceObjects;
//        }

//        public async Task<Dictionary<string, InsightResult>> TranslateTranscriptionAsync(AiJobRequest jobRequest, Channel channel, InsightResult sttInsightResult,
//                                                                                        string sttAudioLanguage, MediaPart mediaPart, string transcriptionJobResultId)
//        {
//            if (jobRequest.TranslationLanguages == null || !jobRequest.TranslationLanguages.Any())
//            {
//                _logger.LogDebug("Request is missing TranslationLanguages");
//                throw new Exception("Translation Request is missing translation languages.");
//            }

//            var translationResults = new Dictionary<string, InsightResult>();

//            var filteredLanaguages = jobRequest.TranslationLanguages.Where(trLng => trLng != sttAudioLanguage);

//            var translationTasks = filteredLanaguages.Select(async trLanguage =>
//            {
//                try
//                {
//                    // Create translation request and provider
//                    var trRequest = GetInsightRequest(SystemInsightTypes.Translation, sttAudioLanguage, trLanguage);

//                    var trInsightInputData = new InsightInputData { SourceInsightInput = sttInsightResult };

//                    ProviderBase? translatorProvider = GetProvider(trInsightInputData, trRequest);

//                    if (translatorProvider is null)
//                    {
//                        _logger.LogWarning($"[TranslateTranscriptionAsync] there is no provider that can handle this Translation request.");
//                        return;
//                    }

//                    // Process translation
//                    var trInsightResult = await translatorProvider.ProcessAsync(trInsightInputData, trRequest);

//                    // DBG File - Save translation to file
//                    //await SaveInsightToFileAsync(trInsightResult![0], sttJsonFile.Replace(".json", $"_{trLanguage}.json"));

//                    if (trInsightResult == null || !trInsightResult.Any() || trInsightResult[0].TimeCodedContent == null)
//                    {
//                        _logger.LogWarning($"[TranslateTranscriptionAsync] Translation result is null or empty for language {trLanguage}.");
//                        return;
//                    }

//                    var translationContent = trInsightResult[0].TimeCodedContent!;

//                    List<TranscriptEx> translationsAsTranscriptEx = ConvertTranscriptsToTranscriptsEx(translationContent);

//                    // Save result in DB
//                    await SaveJobResultInDB(GetProviderType(translatorProvider.ProviderMetadata.DisplayName!), translationsAsTranscriptEx, mediaPart, channel, Operation.TranslateTranscription, jobRequest.Id!, mediaPart.VideoFileLocation, sttAudioLanguage, trLanguage, transcriptionJobResultId);

//                    //_logger.LogDebug($"[TranslateTranscriptionAsync] Translation saved: {sttJsonFile.Replace(".json", $"_{trLanguage}.json")}");

//                    // Add to the result dictionary
//                    lock (translationResults) // Ensure thread-safety
//                    {
//                        translationResults[trLanguage] = trInsightResult[0];
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogDebug($"[TranslateTranscriptionAsync] Translation failed: {ex}");
//                }
//            });

//            await Task.WhenAll(translationTasks);

//            return translationResults;
//        }

//        /// <summary>
//        /// for continuousjob - clean results older that XX number of days
//        /// </summary>
//        /// <returns></returns>
//        public async Task CleanOldAIJobResultsSafeAsync()
//        {
//            try
//            {
//                JobRequestFilter jobRequestFilter = new JobRequestFilter() { RuleRecurrence = RuleRecurrenceEnum.Continuous };

//                List<AiJobRequest> jobs = await _aiJobRequestRepository.GetFilteredJobRequestsAsync(jobRequestFilter);

//                if (jobs is null || jobs.Count == 0)
//                    return;

//                SettingsDm settings = await _serviceManagerSvc.GetSettingsAsync();

//                foreach (AiJobRequest job in jobs)
//                {
//                    long deletedDocuments = await _aiJobResultRepository.DeleteResultsOlderThanDateAsync(DateTime.Now.AddDays(-1 * settings.CleanupOlderResultsForContinuousAiJobDays), job.Id);

//                    _logger.LogInformation($"CleanOldAIJobResultsSafeAsync: Deleted {deletedDocuments} results for continuous job {job.Id}({job.Name}) older than {settings.CleanupOlderResultsForContinuousAiJobDays} days.");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning($"Faield to CleanOldAIJobResultsSafeAsync. Error: {ex}");
//            }
//        }

//        private ProviderType GetProviderType(string providerDisplayName)
//        {
//            if (providerDisplayName.ToLower().Contains("open ai"))
//                return ProviderType.OpenAI;
//            else if (providerDisplayName.ToLower().Contains("whisper"))
//                return ProviderType.Whisper;
//            else if (providerDisplayName.ToLower().Contains("speechmatix"))
//                return ProviderType.Speechmatix;
//            else if (providerDisplayName.ToLower().Contains("azure"))
//                return ProviderType.Azure;
//            else if (providerDisplayName.ToLower().Contains("neurotech"))
//                return ProviderType.Neurotech;
//            else if (providerDisplayName.ToLower().Contains("googlevideo"))
//                return ProviderType.Neurotech;

//            return ProviderType.None;
//        }

//    }
//}
