using Server.Models;
using System.Diagnostics.Eventing.Reader;

namespace Server.InsightProviders
{
    public abstract class ProviderBase
    {
        public abstract string Name { get; }

        /// <summary>
        /// What insight types this provider can handle
        /// </summary>
        public abstract IReadOnlyCollection<InsightTypes> SupportedInsightTypes { get; }

        protected abstract Task<Insight> StartProcessingAsync(InsightInputData insightInputData, InsightRequest insightRequest);

        protected abstract void EnsureInputValidity(InsightInputData insightInputData);
        public virtual bool CanHandleRequest(InsightRequest request)//InsightInputData insightInputData, InsightRequest insight)
        {
            return SupportedInsightTypes.Contains(request.InsightType);
        }
        public async Task<Insight> ProcessAsync(InsightInputData insightInputData, InsightRequest insightRequest)//, AIClipDm? aiclip = null)
        {
            try
            {
                EnsureInputValidity(insightInputData);

                var insightResult = await StartProcessingAsync(insightInputData, insightRequest);//, providerLogEntry);

                return insightResult;
            }
            catch (Exception ex)
            {
                // Log the error and rethrow
                //TODO : update here the DB log
                //ProviderServices.Logger.LogWarning($"{aiclip} - Failed to process insight {insightRequest.InsightType}. Error: {ex.Message}");
                //await LogActivity(providerLogEntry, LogStatus.Failed);

                throw;
            }
        }

    }
}
