using Server.Models;

namespace Server.Repositories
{
    public interface IInsightDefinitionRepository
    {
        Task<InsightDefinition?> GetByNameAsync(string name);
    }

    public class InsightDefinitionRepository : IInsightDefinitionRepository
    {
        public Task<InsightDefinition?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
