using FFMpegCore;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Concurrent;
using System.Text.Json;
using Whisper.net;
using Whisper.net.Ggml;
using WhisperTranscriber.Models;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            GlobalFFOptions.Configure(new FFOptions
            {
                BinaryFolder = Path.Combine(AppContext.BaseDirectory, "C:\\Users\\USER\\Downloads\\ffmpeg-8.0.1-full_build\\ffmpeg-8.0.1-full_build\\bin")
            });

            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: WhisperTranscriber <audioFilePath> <whisperModelsPath> <modelType> [segmentDurationSec] [useTranslate] [isoCodeLanguage]");
                Environment.Exit(1);
            }

            string audioFilePath = args[0];
            string whisperModelsPath = args[1];
            string modelType = args[2];

            int segmentDurationSec = (args.Length > 3 && int.TryParse(args[3], out var parsedDuration)) ? parsedDuration : 300;
            bool useTranslate = args.Length > 4 && bool.TryParse(args[4], out var translate) && translate;
            string isoCodeLanguage = args.Length > 5 ? args[5] : string.Empty;

            if (!File.Exists(audioFilePath))
            {
                Console.Error.WriteLine("Invalid file path. Please ensure the file exists.");
                Environment.Exit(1);
            }

            if (!Directory.Exists(whisperModelsPath))
            {
                Console.Error.WriteLine("Invalid models path. Please ensure the directory exists.");
                Environment.Exit(1);
            }

            var whisperApp = new WhisperApp(whisperModelsPath);

            InternalTranscriptionResponse? result;
            try
            {
                result = await whisperApp.TranscribeWithSplittingAsync(audioFilePath, modelType, segmentDurationSec, useTranslate, isoCodeLanguage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during transcription: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
                return; // Just for compiler
            }

            if (result != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(result));
            }
            else
            {
                Console.Error.WriteLine("Transcription failed.");
                Environment.Exit(1);
            }

            try
            {
                string segmentsDir = Path.Combine(Path.GetDirectoryName(audioFilePath)!, "segments");
                if (Directory.Exists(segmentsDir))
                    Directory.Delete(segmentsDir, recursive: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Failed to delete segment files: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}

class WhisperApp
{
    private readonly string _modelsPath;

    public WhisperApp(string modelsPath)
    {
        _modelsPath = modelsPath;
    }

    public async Task<InternalTranscriptionResponse?> TranscribeWithSplittingAsync(string audioFilePath, string modelType, int segmentDurationSec, bool useTranslate, string isoCodeLanguage)
    {
        var allTranscripts = new ConcurrentBag<Transcript>();
        string? detectedLanguage = null;
        List<(string path, int offsetSec)> segmentOffsets = new();

        try
        {
            var segments = await SplitAudioAsync(audioFilePath, segmentDurationSec);

            string modelFileName = GetModelFileName(GetGgmlModel(ToPascalCase(modelType)));
            string modelPath = Path.Combine(_modelsPath, modelFileName);

            await DownloadModelFileIfMissing(GetGgmlModel(ToPascalCase(modelType)));

            int currentOffset = 0;

            foreach (var segment in segments)
            {
                try
                {
                    using var reader = new AudioFileReader(segment);
                    int duration = (int)reader.TotalTime.TotalSeconds;
                    segmentOffsets.Add((segment, currentOffset));
                    currentOffset += duration;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to read segment audio file '{segment}': {ex.Message}");
                }
            }

            await Parallel.ForEachAsync(segmentOffsets, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (item, ct) =>
            {
                string segmentPath = item.path;
                int offset = item.offsetSec;

                try
                {
                    using var whisperFactory = WhisperFactory.FromPath(modelPath);

                    var builder = whisperFactory.CreateBuilder()
                        .WithLanguage(string.IsNullOrEmpty(isoCodeLanguage) ? "auto" : isoCodeLanguage);

                    if (useTranslate)
                        builder.WithTranslate();

                    using var processor = builder.Build();

                    using var wavStream = await ConvertAudioToWavFileAsync(segmentPath);

                    await foreach (var result in processor.ProcessAsync(wavStream, ct))
                    {
                        detectedLanguage ??= result.Language;

                        allTranscripts.Add(new Transcript
                        {
                            Text = result.Text,
                            StartInSeconds = (int)result.Start.TotalSeconds + offset,
                            EndInSeconds = (int)result.End.TotalSeconds + offset
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing segment '{segmentPath}': {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error in transcription process: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return null;
        }

        return new InternalTranscriptionResponse
        {
            Transcripts = allTranscripts.OrderBy(t => t.StartInSeconds).ToList(),
            DetectedLanguage = detectedLanguage ?? "unknown"
        };
    }

    private static async Task<MemoryStream> ConvertAudioToWavFileAsync(string audioFilePath)
    {
        using var reader = new AudioFileReader(audioFilePath);

        var mono = reader.WaveFormat.Channels > 1
            ? new StereoToMonoSampleProvider(reader.ToSampleProvider())
            : reader.ToSampleProvider();

        var resampler = new WdlResamplingSampleProvider(mono, 16000);

        var wavStream = new MemoryStream();

        await Task.Run(() => WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16()));

        wavStream.Seek(0, SeekOrigin.Begin);

        return wavStream;
    }

    public async Task<List<string>> SplitAudioAsync(string inputFilePath, int segmentDurationSec)
    {
        var outputDir = Path.Combine(Path.GetDirectoryName(inputFilePath)!, "segments");

        Directory.CreateDirectory(outputDir);

        var baseName = Path.GetFileNameWithoutExtension(inputFilePath);

        var ext = Path.GetExtension(inputFilePath);

        var outputTemplate = Path.Combine(outputDir, $"{baseName}_%03d{ext}");

        var args = FFMpegArguments
            .FromFileInput(inputFilePath)
            .OutputToFile(outputTemplate, overwrite: true, options => options
                .ForceFormat("segment")
                .WithCustomArgument($"-segment_time {segmentDurationSec} -c copy -reset_timestamps 1"));

        await args.ProcessAsynchronously();

        return Directory.GetFiles(outputDir, $"{baseName}_*.{ext.Trim('.')}")
                        .OrderBy(f => f)
                        .ToList();
    }

    private async Task DownloadModelFileIfMissing(GgmlType ggmlType)
    {
        var modelName = GetModelFileName(ggmlType);

        var modelFilePath = Path.Combine(_modelsPath, modelName);

        if (!File.Exists(modelFilePath))
        {
            var downloader = new WhisperGgmlDownloader(new HttpClient());

            using var modelStream = await downloader.GetGgmlModelAsync(ggmlType);

            using var fileWriter = File.OpenWrite(modelFilePath);

            await modelStream.CopyToAsync(fileWriter);
        }
    }

    private string ToPascalCase(string str)
        => string.IsNullOrEmpty(str) ? str : char.ToUpper(str[0]) + str[1..];

    private GgmlType GetGgmlModel(string modelType)
        => Enum.TryParse<GgmlType>(modelType, out var parsed) ? parsed : GgmlType.Base;

    private string GetModelFileName(GgmlType ggmlType) => ggmlType switch
    {
        GgmlType.Tiny => "ggml-tiny.bin",
        GgmlType.TinyEn => "ggml-tiny.en.bin",
        GgmlType.Base => "ggml-base.bin",
        GgmlType.BaseEn => "ggml-base.en.bin",
        GgmlType.Small => "ggml-small.bin",
        GgmlType.SmallEn => "ggml-small.en.bin",
        GgmlType.Medium => "ggml-medium.bin",
        GgmlType.MediumEn => "ggml-medium.en.bin",
        GgmlType.LargeV1 => "ggml-large-v1.bin",
        GgmlType.LargeV2 => "ggml-large-v2.bin",
        GgmlType.LargeV3 => "ggml-large-v3.bin",
        GgmlType.LargeV3Turbo => "ggml-large-v3-turbo.bin",
        _ => "ggml-base.bin"
    };
}