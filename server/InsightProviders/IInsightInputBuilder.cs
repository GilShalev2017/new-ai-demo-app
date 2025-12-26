using Server.Models;

namespace Server.InsightProviders
{
    public interface IInsightInputBuilder
    {
        bool CanBuild(InsightRequest request);
        Task<InsightInputData?> BuildAsync(Clip clip, InsightRequest request);
    }
}
