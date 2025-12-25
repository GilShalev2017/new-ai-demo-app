namespace Server.Models
{
    public class Transcript
    {
        public string Text { get; set; } = null!;
        public int StartInSeconds { get; set; }
        public int EndInSeconds { get; set; }
    }
}
