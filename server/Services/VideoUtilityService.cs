using FFMpegCore;
using FFMpegCore.Enums;
using System.Diagnostics;

namespace Server.Services
{
    public interface IVideoUtilityService
    {
        int GetVideoDuration(string videoPath);
        long GetFileSize(string videoPath);
        Task<string> GenerateThumbnailAsync(string videoPath, string outputPath);
        string GetVideoUrl(string fileName);
        string GetThumbnailUrl(string fileName);
        Task<string> ConvertMp4ToMp3Async(string mp4Path);
        string GetFilePathFromUrl(string url);
    }

    public class VideoUtilityService : IVideoUtilityService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VideoUtilityService> _logger;
        private readonly string _videosFolder;
        private readonly string _thumbnailsFolder;
        private readonly string _audioOutputFolder;

        private readonly string _ffmpegPath;

        public VideoUtilityService(
            IWebHostEnvironment environment,
            IConfiguration config,
            ILogger<VideoUtilityService> logger)
        {
            _environment = environment;
            _logger = logger;

            // Folders
            _videosFolder = Path.Combine(_environment.WebRootPath, "videos");
            _thumbnailsFolder = Path.Combine(_environment.WebRootPath, "thumbnails");
            Directory.CreateDirectory(_videosFolder);
            Directory.CreateDirectory(_thumbnailsFolder);

            // Audio output folder
            _audioOutputFolder = config["AudioOutputFolder"]
                                 ?? Path.Combine(_environment.WebRootPath, "audio");
            Directory.CreateDirectory(_audioOutputFolder);

            // FFmpeg path
            _ffmpegPath = config["FFmpegPath"];
            if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
                throw new FileNotFoundException("FFmpeg executable not found", _ffmpegPath);

            // Configure globally for FFMpegCore
            GlobalFFOptions.Configure(options =>
            {
                options.BinaryFolder = Path.GetDirectoryName(_ffmpegPath)!;
            });
        }

        public string GetVideoUrl(string fileName)
        {
            return $"/videos/{fileName}";
        }

        public string GetThumbnailUrl(string fileName)
        {
            var thumbnailName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            return $"/thumbnails/{thumbnailName}";
        }

        public int GetVideoDuration(string videoPath)
        {
            try
            {
                var fullPath = Path.Combine(_videosFolder, videoPath);
                if (!File.Exists(fullPath))
                    return 0;

                // Using FFmpeg to get duration
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,//"ffmpeg",
                    Arguments = $"-i \"{fullPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                var output = process?.StandardError.ReadToEnd() ?? "";
                process?.WaitForExit();

                // Parse duration from FFmpeg output
                var match = System.Text.RegularExpressions.Regex.Match(
                    output,
                    @"Duration: (\d{2}):(\d{2}):(\d{2})"
                );

                if (match.Success)
                {
                    var hours = int.Parse(match.Groups[1].Value);
                    var minutes = int.Parse(match.Groups[2].Value);
                    var seconds = int.Parse(match.Groups[3].Value);
                    return hours * 3600 + minutes * 60 + seconds;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video duration");
                return 0;
            }
        }

        public long GetFileSize(string videoPath)
        {
            try
            {
                var fullPath = Path.Combine(_videosFolder, videoPath);
                if (File.Exists(fullPath))
                {
                    return new FileInfo(fullPath).Length;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<string> GenerateThumbnailAsync(string videoFileName, string outputFileName)
        {
            try
            {
                var videoPath = Path.Combine(_videosFolder, videoFileName);
                var thumbnailPath = Path.Combine(_thumbnailsFolder, outputFileName);

                if (!File.Exists(videoPath))
                {
                    _logger.LogWarning($"Video file not found: {videoPath}");
                    return string.Empty;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{thumbnailPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                await process!.WaitForExitAsync();

                return File.Exists(thumbnailPath)
                    ? GetThumbnailUrl(outputFileName)
                    : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating thumbnail");
                return string.Empty;
            }
        }

        public async Task<string> ConvertMp4ToMp3Async(string mp4Path)
        {
            if (!File.Exists(mp4Path))
                throw new FileNotFoundException("Video file not found", mp4Path);

            var outputFile = Path.Combine(_audioOutputFolder,
                $"{Path.GetFileNameWithoutExtension(mp4Path)}.mp3");

            // Process asynchronously – no need to pass FFOptions here
            await FFMpegArguments
                .FromFileInput(mp4Path)
                .OutputToFile(outputFile, overwrite: true, options => options
                    .WithAudioCodec("libmp3lame")
                    .WithAudioBitrate(128)
                    .DisableChannel(Channel.Video))
                .ProcessAsynchronously(); // ← fixed

            return outputFile;
        }

        public string GetFilePathFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            var relativePath = url.TrimStart('/');
            return Path.Combine(_environment.WebRootPath, relativePath);
        }

    }
}