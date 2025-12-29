namespace Server.DTOs
{
    public class FolderIngestionRequestDto
    {
        /// <summary>
        /// Root folder containing subfolders per channel
        /// </summary>
        public string RootFolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Maximum number of clips per channel to ingest
        /// </summary>
        public int MaxClipsPerChannel { get; set; } = 6;

        /// <summary>
        /// Optional: Only ingest clips newer than this date
        /// </summary>
        public DateTime? FromDate { get; set; }
    }

}
