using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WhisperService.Configuration;
using WhisperService.Models;
using WhisperTranscriber.Models;

namespace WhisperService.Services
{
    public class WhisperSvc
    {
        private readonly ILogger<WhisperSvc> _logger;
        private readonly WhisperSettings _settings;
        public WhisperSvc(ILogger<WhisperSvc> logger, IOptions<WhisperSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<(string?, string?)> SaveAudioFileAsync(string audioFileName, HttpRequest Request)
        {
            string tempDirectory = _settings.AudioFilesDirectory; //@"c:\Temp\Whisper\AudioFiles";

            string uniqueFolderName = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_{audioFileName.Replace(".", "_")}";

            string audioFileDirectory = Path.Combine(tempDirectory, uniqueFolderName);

            string fullPath = Path.Combine(audioFileDirectory, audioFileName);

            _logger.LogDebug($"Saving {audioFileName} to {fullPath}");

            try
            {
                Directory.CreateDirectory(audioFileDirectory);

                var file = Request.Form.Files[0];

                using var stream = new FileStream(fullPath, FileMode.Create);

                await file.CopyToAsync(stream);

                _logger.LogDebug($"File {audioFileName} successfully saved to {fullPath}");

                return (audioFileDirectory, fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"File {audioFileName} failed to save to {fullPath}, " + ex.Message);

                return (null, null);
            }
        }
        public void DeleteAudioFileAsync(string? audioFileDirectory)
        {
            if (!string.IsNullOrEmpty(audioFileDirectory) && Directory.Exists(audioFileDirectory))
            {
                _logger.LogDebug($"Deleting Audio File Directory: {audioFileDirectory}");

                try
                {
                    Directory.Delete(audioFileDirectory, true); // Recursive delete

                    _logger.LogDebug($"Directory {audioFileDirectory} successfully deleted.");
                }
                catch (IOException deleteEx)
                {
                    _logger.LogError($"Error deleting directory {audioFileDirectory}: {deleteEx.Message}");
                    // Consider adding retry logic or logging for cleanup
                }
            }
            else if (string.IsNullOrEmpty(audioFileDirectory))
            {
                _logger.LogWarning("Attempted to delete a null or empty audio file directory path.");
            }
            else
            {
                _logger.LogWarning($"Audio file directory does not exist: {audioFileDirectory}");
            }
        }
        public async Task<InternalTranscriptionResponse?> WhisperNugetTranscribeAsync(string audioFilePath, string modelType = "Base", bool useTranslate = false)
        {
            var result = new InternalTranscriptionResponse();
            try
            {
                // Define the path to the WhisperTranscriber app
                //string transcriberAppPath = @"C:\ACTUS_LIVEU\new-ai-demo-app\on-premise-providers\WhisperTranscriber\bin\Debug\net9.0\WhisperTranscriber.exe";
                //string whisperModelsPath = @"C:\temp\Whisper\Models";
                //string segmentDurationSec = "300";

                string transcriberAppPath = _settings.TranscriberAppPath;
                string whisperModelsPath = _settings.WhisperModelsPath;
                string segmentDurationSec = _settings.SegmentDurationSec.ToString();

                // Build the arguments to pass to the transcriber app
                string arguments = $"\"{audioFilePath}\" {whisperModelsPath} {modelType} {segmentDurationSec} {useTranslate} ";

                //Console.WriteLine($"arguments = {arguments}");

                // Set up the process
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = transcriberAppPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    // Subscribe to the output and error streams
                    process.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
                    process.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

                    // Start the process
                    process.Start();

                    // Begin reading output and error streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        // Parse the JSON response from the external app
                        JsonSerializerOptions jsonSerializerOptions = new()
                        {
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };
                        JsonSerializerOptions options = jsonSerializerOptions;
                        string output = outputBuilder.ToString();
                        result = JsonSerializer.Deserialize<InternalTranscriptionResponse>(output, options);
                    }
                    else
                    {
                        string error = errorBuilder.ToString();
                        throw new Exception($"WhisperTranscriber exited with code {process.ExitCode}: {error}");
                    }
                }
            }
            catch (Exception)
            {
                // Log the error and rethrow or handle as needed
                //Console.WriteLine($"Error invoking WhisperTranscriber: {ex.Message}");
                throw;
            }

            return result;
        }
        //public async Task<InternalTranscriptionResponse?> CommandLineTranscribeAsync(string audioFilePath, string modelType = "base", bool useTranslate = false)
        //{
        //    // Ensure the model type is valid
        //    modelType = string.IsNullOrWhiteSpace(modelType) ? "base" : modelType.ToLower();

        //    string outputDirectory = _serviceConfiguration.TranscriptsOutputDirectory!;

        //    if (!Directory.Exists(outputDirectory))
        //    {
        //        Directory.CreateDirectory(outputDirectory);
        //    }

        //    string fileName = Path.GetFileNameWithoutExtension(audioFilePath);
        //    string outputFilePath = Path.Combine(outputDirectory, $"{fileName}.vtt");
        //    string jsonFilePath = Path.Combine(outputDirectory, $"{fileName}.json");

        //    string translateOption = useTranslate ? "--task translate" : "";

        //    string whisperLocation = _serviceConfiguration.PythonVirtualEnvWhisperExePath!;

        //    //Pay attention to the '--device cuda' forwarding !
        //    string commandArgs = $"\"{audioFilePath}\" --model base --output_dir \"{outputDirectory}\" --device cuda";

        //    try
        //    {
        //        // Run the Whisper CLI command
        //        var processStartInfo = new ProcessStartInfo
        //        {
        //            FileName = whisperLocation,
        //            Arguments = commandArgs,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        using (var process = Process.Start(processStartInfo))
        //        {
        //            if (process == null)
        //                throw new Exception("Failed to start Whisper process.");

        //            // Capture and log output and errors
        //            string output = await process.StandardOutput.ReadToEndAsync();
        //            string error = await process.StandardError.ReadToEndAsync();
        //            await process.WaitForExitAsync();

        //            _logger.LogDebug($"Whisper output: {output}");
        //            if (!string.IsNullOrEmpty(error))
        //            {
        //                _logger.LogError($"Whisper error: {error}");
        //            }

        //            if (process.ExitCode != 0)
        //            {
        //                throw new Exception($"Whisper command failed with exit code {process.ExitCode}");
        //            }
        //        }

        //        // Parse the VTT file to create transcripts
        //        var transcriptions = ReadTranscriptFile(outputFilePath);

        //        // Get detected language from the output or set a placeholder
        //        var detectedLanguage = await GetAutoDetectedLanguage(jsonFilePath);

        //        var internalTranscriptionResponse = new InternalTranscriptionResponse
        //        {
        //            Transcripts = transcriptions,
        //            DetectedLanguage = detectedLanguage
        //        };

        //        //RemoveTranscriptionFiles(outputDirectory);

        //        return internalTranscriptionResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error processing audio file {audioFilePath}: {ex.Message}");
        //        return null;
        //    }
        //}
        ////Not in use at the moment
        //public async Task<InternalTranscriptionResponse?> WhisperTranscribePythonAsync(string audioFilePath, string modelType = "base", bool useTranslate = false)
        //{
        //    var result = new InternalTranscriptionResponse();
        //    try
        //    {
        //        string transcriberAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "transcribe_with_faster_whisper.py");

        //        // Choose language based on useTranslate flag
        //        string language = useTranslate ? "en" : "None";

        //        // Prepare the arguments
        //        string arguments = $"\"{transcriberAppPath}\" \"{audioFilePath}\" \"{language}\" \"{modelType}\"";

        //        var processStartInfo = new ProcessStartInfo
        //        {
        //            FileName = "python", // Make sure 'python' is in your PATH or use full path to python.exe
        //            Arguments = arguments,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        var outputBuilder = new StringBuilder();
        //        var errorBuilder = new StringBuilder();

        //        using var process = new Process { StartInfo = processStartInfo };

        //        process.OutputDataReceived += (sender, args) =>
        //        {
        //            if (args.Data != null)
        //                outputBuilder.AppendLine(args.Data);
        //        };

        //        process.ErrorDataReceived += (sender, args) =>
        //        {
        //            if (args.Data != null)
        //                errorBuilder.AppendLine(args.Data);
        //        };

        //        process.Start();
        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();

        //        await process.WaitForExitAsync();

        //        if (process.ExitCode == 0)
        //        {
        //            string output = outputBuilder.ToString();

        //            var options = new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true,
        //                WriteIndented = true
        //            };

        //            result = JsonSerializer.Deserialize<InternalTranscriptionResponse>(output, options);
        //        }
        //        else
        //        {
        //            string error = errorBuilder.ToString();
        //            throw new Exception($"WhisperTranscriber exited with code {process.ExitCode}: {error}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error invoking WhisperTranscriber: {ex.Message}");
        //        throw;
        //    }

        //    return result;
        //}
        public void RemoveTranscriptionFiles(string outputDirectory)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(outputDirectory))
                {
                    // Get all files in the directory
                    var files = Directory.GetFiles(outputDirectory);

                    // Iterate through and delete each file
                    foreach (var file in files)
                    {
                        File.Delete(file);
                        //Console.WriteLine($"Deleted file: {file}");
                    }

                    //Console.WriteLine("All transcription files have been removed.");
                }
                else
                {
                    //Console.WriteLine("Output directory does not exist.");
                }
            }
            catch
            {
                //Console.WriteLine($"An error occurred while deleting files: {ex.Message}");
            }
        }
        private async Task<string?> GetAutoDetectedLanguage(string jsonFilePath)
        {
            try
            {
                // Read the JSON file
                string jsonString = await File.ReadAllTextAsync(jsonFilePath);

                // Deserialize the JSON string to the model
                JSONTranscriptionModel? jsonModel = JsonSerializer.Deserialize<JSONTranscriptionModel>(jsonString);

                // Access the language property
                if (jsonModel != null)
                {
                    //Console.WriteLine($"Language: {jsonModel.language}");
                    return jsonModel.language;
                }
                else
                {
                    //Console.WriteLine("Failed to deserialize JSON.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error GetAutoDetectedLanguage: {ex.Message}");
            }
            return null;
        }
        // Method to parse the VTT file and create transcript list
        private List<Transcript> ReadTranscriptFile(string filePath)
        {
            var transcripts = new List<Transcript>();
            // Adjusted regex to match the VTT time format
            var timeRegex = new Regex(@"^(\d{2}:\d{2}\.\d{3}) --> (\d{2}:\d{2}\.\d{3})$");

            string[] lines = File.ReadAllLines(filePath);
            Transcript? currentTranscript = null;
            string currentText = string.Empty;

            foreach (string line in lines)
            {
                // Ignore the first line with "WEBVTT" and any blank lines
                if (line.Trim().Equals("WEBVTT", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var match = timeRegex.Match(line);
                if (match.Success)
                {
                    if (currentTranscript != null)
                    {
                        currentTranscript.Text = currentText.Trim();
                        transcripts.Add(currentTranscript);
                        currentText = string.Empty;
                    }

                    currentTranscript = new Transcript
                    {
                        StartInSeconds = (int)ParseTimeInSeconds(match.Groups[1].Value),
                        EndInSeconds = (int)ParseTimeInSeconds(match.Groups[2].Value)
                    };
                }
                else if (!string.IsNullOrWhiteSpace(line) && currentTranscript != null)
                {
                    currentText += line + " ";
                }
            }

            // Add the last transcript if it exists
            if (currentTranscript != null)
            {
                currentTranscript.Text = currentText.Trim();
                transcripts.Add(currentTranscript);
            }

            return transcripts;
        }
        // Adjusted ParseTimeInSeconds method to handle the VTT format
        private double ParseTimeInSeconds(string time)
        {
            var timeParts = time.Split(':');
            var minutes = double.Parse(timeParts[0]);
            var secondsParts = timeParts[1].Split('.');
            var seconds = double.Parse(secondsParts[0]);
            var milliseconds = double.Parse(secondsParts[1]) / 1000.0;
            return minutes * 60 + seconds + milliseconds;
        }
        // Method to parse time in seconds
        private int ParseTimeInSeconds(string hours, string minutes, string seconds, string milliseconds)
        {
            return int.Parse(hours) * 3600 + int.Parse(minutes) * 60 + int.Parse(seconds);
        }
        public async Task TranscribeTestAsync(string audioFilePath, string modelType = "Base", bool useTranslate = false)
        {
            // Hardcoded file paths and arguments
            //audioFilePath = @"C:\Actus_Temp\AudioFiles\6729d65f3646d8cf1090ed23.mp3";
            //string outputDirectory = @"C:\IntelligenceApps\WhisperOutput"; //from the python virtual environment
            //string whisperLocation = @"C:\Actus_Temp\AudioFiles\whisper-env\Scripts\whisper.exe";

            audioFilePath = _settings.TempTestAudioFilePath;
            string outputDirectory = _settings.TranscriptsOutputDirectory;
            string whisperLocation = _settings.WhisperExePath;

            // Construct the Whisper command arguments with the confirmed options
            string commandArgs = $"\"{audioFilePath}\" --model base --output_dir \"{outputDirectory}\" --device cuda";

            try
            {
                // Set up the process start information
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = whisperLocation,  // Hardcoded path to whisper.exe
                    Arguments = commandArgs,      // Command arguments with hardcoded options
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                        throw new Exception("Failed to start Whisper process.");

                    // Capture and log output and errors
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogDebug($"Whisper output: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogError($"Whisper error: {error}");
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Whisper command failed with exit code {process.ExitCode}");
                    }
                }

                _logger.LogInformation("Whisper command executed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing Whisper command: {ex.Message}");
            }
        }
    }
}
