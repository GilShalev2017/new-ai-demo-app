namespace Server.DTOs
{
    public class FolderIngestionResultDto
    {
        public int TotalFilesProcessed { get; set; }
        public int FilesSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }

}
