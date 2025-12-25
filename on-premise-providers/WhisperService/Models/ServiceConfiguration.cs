using System.Xml.Serialization;

namespace WhisperService.Models
{
    [XmlRoot("configuration")]
    public class ServiceConfiguration
    {
        [XmlElement("audioFilesDirectory")]
        public string? AudioFilesDirectory { get; set; }
        [XmlElement("transcriberAppPath")]
        public string? TranscriberAppPath { get; set; }
        [XmlElement("transcriptsOutputDirectory")]
        public string? TranscriptsOutputDirectory { get; set; }
        [XmlElement("pythonVirtualEnvWhisperExePath")]
        public string? PythonVirtualEnvWhisperExePath { get; set; }
        [XmlElement("whisperModelsPath")]
        public string? WhisperModelsPath { get; set; }
        [XmlElement("segmentDurationSec")]
        public string? SegmentDurationSec { get; set; }
    }
}
