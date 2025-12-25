
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Models;
using SharpCompress.Common;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

namespace Server.InsightProviders
{
    public class WhisperTranscriberProvider : ProviderBase
    {
        public override string Name => "WhisperTranscriberProvider";
        public override IReadOnlyCollection<InsightTypes> SupportedInsightTypes => new[] { InsightTypes.Transcription };

        private readonly string WhisperServerUrl = "https://localhost:7094/Whisper/transcribe";

        public WhisperTranscriberProvider()
        {
        }

        protected override void EnsureInputValidity(InsightInputData insightInputData)
        {
            if (insightInputData.AudioInput == null)
                throw new InvalidOperationException("Audio input is required for transcription.");
        }

        protected override async Task<Insight> StartProcessingAsync(InsightInputData insightInputData, InsightRequest insightRequest)
        {
            var internalTranscriptionResponse = await TranscribeAsync(insightInputData.AudioInput!.FilePath!, insightInputData.AudioInput.AudioLanguage!);

            if (internalTranscriptionResponse == null || internalTranscriptionResponse.Transcripts == null || internalTranscriptionResponse.Transcripts.Count == 0)
            {
                throw new InvalidOperationException("No transcriptions found in the response.");
            }

            Insight transcriptionInsight = CreateTranscriptionResponse(internalTranscriptionResponse);

            return transcriptionInsight;
        }

        private async Task<InternalTranscriptionResponse?> TranscribeAsync(string audioFilePath, string? sourceLanguageCode = null)
        {
            var httpClient = new HttpClient
            {
                //Timeout = TimeSpan.FromMinutes(TimeoutInMinutes)
                Timeout = Timeout.InfiniteTimeSpan
            };

            var audioFileName = Path.GetFileName(audioFilePath);

            using var formData = new MultipartFormDataContent
            {
                { new StringContent(audioFileName), "AudioFileName" },
                { new StreamContent(File.OpenRead(audioFilePath)), "File", audioFileName },
                //{ new StringContent(ModelType.ToString()), "ModelType" },
                { new StringContent("Base"), "ModelType" }
            };

            if (!string.IsNullOrEmpty(sourceLanguageCode))
            {
                formData.Add(new StringContent(sourceLanguageCode), "Language");
            }

            var response = await httpClient.PostAsync(WhisperServerUrl, formData);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new InvalidOperationException("No response data received.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var internalTranscriptionResponse = JsonSerializer.Deserialize<InternalTranscriptionResponse>(jsonString, options);

            if (internalTranscriptionResponse == null || string.IsNullOrWhiteSpace(internalTranscriptionResponse.DetectedLanguage))
            {
                throw new InvalidOperationException("Deserialization failed or language not detected.");
            }

            return internalTranscriptionResponse;
        }

        private Insight CreateTranscriptionResponse(InternalTranscriptionResponse internalTranscriptionResponse)
        {
            return new TranscriptionInsight
            {
                Transcripts = internalTranscriptionResponse.Transcripts!,
                ProviderName = "WhisperTranscriberProvider",
                AudioLanguage = internalTranscriptionResponse.Language!,
                InsightType = InsightTypes.Transcription
            };
        }

    }
}
