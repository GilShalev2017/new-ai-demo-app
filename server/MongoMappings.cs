using MongoDB.Bson.Serialization;
using Server.Models;

namespace Server
{

    public static class MongoMappings
    {
        private static bool _registered = false;

        public static void Register()
        {
            if (_registered)
                return;

            _registered = true;

            BsonClassMap.RegisterClassMap<Insight>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
                cm.SetDiscriminator("insightType");
            });

            BsonClassMap.RegisterClassMap<TranscriptionInsight>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator(nameof(TranscriptionInsight));
            });

            BsonClassMap.RegisterClassMap<SummaryInsight>(cm =>
            {
                cm.AutoMap();
                cm.SetDiscriminator(nameof(SummaryInsight));
            });
        }
    }
}
