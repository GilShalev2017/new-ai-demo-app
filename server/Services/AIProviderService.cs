using Server.InsightProviders;
using Server.Models;

namespace Server.Services
{
    public interface IAIProviderService
    {
        ProviderBase? GetProviderForRequest(InsightRequest request);
    }
    public class AIProviderService : IAIProviderService
    {
        private readonly IEnumerable<ProviderBase> _providers;

        public AIProviderService(IEnumerable<ProviderBase> providers)
        {
            _providers = providers;
        }

        public ProviderBase? GetProviderForRequest(InsightRequest request)
        {
            return _providers.FirstOrDefault(p => p.CanHandleRequest(request));
        }
    }
}
