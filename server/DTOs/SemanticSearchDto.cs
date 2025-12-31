namespace Server.DTOs
{
    public class SemanticSearchDto
    {
        /// <summary>
        /// Natural language query from the user
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Optional conversation/session id (future follow-ups)
        /// </summary>
        public string? ConversationId { get; set; }

        /// <summary>
        /// Optional clip scope (e.g. user is inside a clip)
        /// </summary>
        public string? ClipId { get; set; }

        /// <summary>
        /// Optional max evidence items (UI hint)
        /// </summary>
        public int MaxEvidence { get; set; } = 10;
    }

}
