namespace Server.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ClipsCollectionName { get; set; } = "Clips";
        public string ClipRequestsCollectionName { get; set; } = "ClipRequests";
        public string AiJobRequestsCollectionName { get; set; } = "AiJobRequests";
        public string AiJobResultsCollectionName { get; set; } = "AiJobResults";
        public string AgentsCollectionName { get; set; } = "Agents";
    }
}
