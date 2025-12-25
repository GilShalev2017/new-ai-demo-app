using Microsoft.AspNetCore.Mvc;
using WhisperService.Models;
using WhisperService.Services;
using WhisperTranscriber.Models;

namespace WhisperService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhisperController : ControllerBase
    {
        private readonly ILogger<WhisperController> _logger;
        private readonly WhisperSvc _whisperSvc;
        private readonly TranscriptionStateService _stateService;

        public WhisperController(ILogger<WhisperController> logger, WhisperSvc whisperSvc, TranscriptionStateService stateService)
        {
            _logger = logger;
            _whisperSvc = whisperSvc;
            _stateService = stateService;
        }

        [HttpPost("transcribe")]
        [DisableRequestSizeLimit]
        public async Task<InternalTranscriptionResponse> Transcribe([FromForm] CaptionsRequest captionsRequest)
        {
            _logger.LogDebug($"Received new request with audio file: {captionsRequest.AudioFileName}, model type={captionsRequest.ModelType}, " +
                             $"translation enabled={(captionsRequest.UseTranslate ? "Yes" : "No")}");


            if (captionsRequest == null || captionsRequest.AudioFileName == null || Request.Form.Files[0] == null || Request.Form.Files[0].Length == 0)
            {
                throw new Exception("Invalid request data. Please check the following:\n" +
                                          "- 'captionsRequest' is null or missing.\n" +
                                          "- 'AudioFileName' is null or missing.\n" +
                                          "- No files were included in the request.");
            }

            string? audioFileDirectory = null;
            string? audioFilePath = null;

            try
            {
                (audioFileDirectory, audioFilePath) = await _whisperSvc.SaveAudioFileAsync(captionsRequest.AudioFileName, Request);

                if (string.IsNullOrEmpty(audioFilePath))
                {
                    _logger.LogError($"Failed to save audio file {captionsRequest.AudioFileName}");

                    throw new Exception($"Failed to save audio file {captionsRequest.AudioFileName}");
                }

                var internalTranscriptionResponse = await _whisperSvc.WhisperNugetTranscribeAsync(audioFilePath, captionsRequest.ModelType!, captionsRequest.UseTranslate);

                if (internalTranscriptionResponse == null)
                {
                    _logger.LogError($"Added {captionsRequest.AudioFileName} as a failed file!");

                    _stateService.FailedTranscriptions.Add(captionsRequest.AudioFileName);
                }
                //var internalTranscriptionResponse = await _whisperSvc.CommandLineTranscribeAsync(audioFilePath, captionsRequest.ModelType!, captionsRequest.UseTranslate);

                //FOR DEBUGGING - PRINTING
                //if (internalTranscriptionResponse != null)
                //{
                //    Console.WriteLine($"Received Detected Language: {internalTranscriptionResponse.DetectedLanguage ?? "N/A"}");

                //    if (internalTranscriptionResponse.Transcripts != null && internalTranscriptionResponse.Transcripts.Any())
                //    {
                //        Console.WriteLine("--- Transcripts ---");
                //        foreach (var transcript in internalTranscriptionResponse.Transcripts)
                //        {
                //            Console.WriteLine($"Text: {transcript.Text}");
                //            Console.WriteLine($"  Start: {transcript.StartInSeconds}s, End: {transcript.EndInSeconds}s");
                //            Console.WriteLine("--------------------"); // Separator for readability
                //        }
                //    }
                //    else
                //    {
                //        Console.WriteLine("No transcripts received.");
                //    }
                //}

                return internalTranscriptionResponse!;
            }
            catch (Exception ex)
            {
                string captionsType = captionsRequest.UseTranslate ? "translations" : "transcriptions";

                _logger.LogError($"Failed To retrieve {captionsType}, for file {captionsRequest.AudioFileName}");

                throw new Exception($"Failed to retrieve {captionsType}, for file {captionsRequest.AudioFileName}, " + ex.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(audioFileDirectory))
                {
                    _logger.LogDebug($"Request completed, attempting to delete directory: {audioFileDirectory}");

                    _whisperSvc.DeleteAudioFileAsync(audioFileDirectory); // Removed 'await'
                }
            }
        }
    }
}
