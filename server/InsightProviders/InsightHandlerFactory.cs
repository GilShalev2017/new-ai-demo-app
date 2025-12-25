using Server.Models;

namespace Server.InsightProviders
{
    public interface IInsightHandlerFactory
    {
        IInsightHandler? GetHandler(InsightTypes insightType);
    }

    public sealed class InsightHandlerFactory : IInsightHandlerFactory
    {
        private readonly IReadOnlyDictionary<InsightTypes, IInsightHandler> _handlers;

        public InsightHandlerFactory(IEnumerable<IInsightHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.InsightType);
        }

        public IInsightHandler? GetHandler(InsightTypes insightType)
        {
            _handlers.TryGetValue(insightType, out var handler);
            return handler;
        }
    }

}
