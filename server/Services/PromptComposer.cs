using Server.DTOs;
using Server.Models;
using SharpCompress.Common;
using System.Text;

namespace Server.Services
{
    public interface IPromptComposer
    {
       (string systemMessage, string data) Compose(QueryIntentContext context, List<EvidenceDto> evidence);
    }

    public class PromptComposer : IPromptComposer
    {
        public (string systemMessage, string data) Compose(QueryIntentContext context, List<EvidenceDto> evidence)
        {
            var sbSystem = new StringBuilder();
            var sbUser = new StringBuilder();

            sbSystem.AppendLine("You are an expert assistant that analyzes media data.");
            sbSystem.AppendLine();
            sbSystem.AppendLine("--- USER QUERY ---");
            sbSystem.AppendLine(context.OriginalQuery);
            sbSystem.AppendLine();
            sbSystem.AppendLine(
                "Answer ONLY using the provided transcripts. " +
                "If the answer is not present, say you cannot find it.");

            if (evidence.Any())
            {
                sbUser.AppendLine("--- TRANSCRIPTS ---");
                foreach (var e in evidence)
                {
                    sbUser.AppendLine(
                        $"[{e.ChannelId} @ {e.ClipStartTime:yyyy-MM-dd HH:mm:ss}] {e.Text}");
                }
            }

            return (sbSystem.ToString(), sbUser.ToString());
        }

    }
}

//private string GenerateTaskDescription(List<string> intents, List<Entity> entities)
//{
//    if (intents.Contains("Summarize", StringComparer.OrdinalIgnoreCase))
//    {
//        return "Please summarize the key topics and insights based on the above content.";
//    }

//    if (intents.Contains("DetectKeywords", StringComparer.OrdinalIgnoreCase))
//    {
//        return "Please identify important or recurring keywords related to the entities.";
//    }

//    if (intents.Contains("EmotionAnalysis", StringComparer.OrdinalIgnoreCase))
//    {
//        return "Analyze the emotional tone or sentiment in the above transcripts.";
//    }

//    if (intents.Contains("CheckAlerts", StringComparer.OrdinalIgnoreCase))
//    {
//        return "Analyze the transcripts and alerts for potential issues or warnings.";
//    }

//    // Default fallback
//    return "Provide an analysis based on the above context and transcripts.";
//}