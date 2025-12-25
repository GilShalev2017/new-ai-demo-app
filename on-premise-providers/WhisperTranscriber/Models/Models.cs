using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperTranscriber.Models
{
    public class Transcript
    {
        public string Text { get; set; } = null!;
        public int StartInSeconds { get; set; }
        public int EndInSeconds { get; set; }
    }

    public class InternalTranscriptionResponse
    {
        public List<Transcript>? Transcripts { get; set; }

        public string? DetectedLanguage { get; set; }

        public string? Language { get; set; }

        public string? Text { get; set; }
    }
}
