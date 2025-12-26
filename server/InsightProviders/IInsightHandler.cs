using Server.Models;

namespace Server.InsightProviders
{
    public interface IInsightHandler
    {
        InsightTypes InsightType { get; }

        Task<InsightInputData?> PrepareInputAsync(Clip clip, InsightRequest request);
    }

}
