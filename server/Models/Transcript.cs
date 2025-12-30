using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models
{
    public class Transcript
    {
        public string Text { get; set; } = null!;
        public int StartInSeconds { get; set; }
        public int EndInSeconds { get; set; }
    }

    public class TranscriptEx : Transcript
    {
        public TranscriptEx()
            : base()
        {
            AbsStartTime = DateTime.MinValue;
            AbsEndTime = DateTime.MinValue;
        }
        public TranscriptEx(string text, int startInSeconds, int endInSeconds, DateTime startTime, DateTime endTime)
            : base()
        {
            Text = text;
            StartInSeconds = startInSeconds;
            EndInSeconds = endInSeconds;
            AbsStartTime = startTime;
            AbsEndTime = endTime;
        }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime AbsStartTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime AbsEndTime { get; set; }
        public List<string>? Keyword { get; set; }
    }


    public class TranscriptsFilter
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Start { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? End { get; set; }
        public int[]? ChannelIds { get; set; }

    }
}