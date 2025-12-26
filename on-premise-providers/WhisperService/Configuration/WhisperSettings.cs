namespace WhisperService.Configuration
{
    public class WhisperSettings
    {
        public string AudioFilesDirectory { get; set; } = @"c:\Temp\Whisper\AudioFiles";
        public string TranscriberAppPath { get; set; } = @"C:\ACTUS_LIVEU\new-ai-demo-app\on-premise-providers\WhisperTranscriber\bin\Debug\net9.0\WhisperTranscriber.exe";
        public string WhisperModelsPath { get; set; } = @"C:\temp\Whisper\Models";
        public string TranscriptsOutputDirectory { get; set; } = @"C:\IntelligenceApps\WhisperOutput";
        public string WhisperExePath { get; set; } = @"C:\Actus_Temp\AudioFiles\whisper-env\Scripts\whisper.exe";
        public string TempTestAudioFilePath { get; set; } = @"C:\Actus_Temp\AudioFiles\6729d65f3646d8cf1090ed23.mp3";
        public int SegmentDurationSec { get; set; } = 300;
    }
}
