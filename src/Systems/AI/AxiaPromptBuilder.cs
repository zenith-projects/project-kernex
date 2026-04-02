using System.Collections.Generic;
using System.Text;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.AI;

namespace ProjectKernex.Systems.AI;

public static class AxiaPromptBuilder
{
    private const string EmotionInstruction =
        "After your answer, add exactly one blank line and then ONLY the emotion tag in this exact format: [EMOTION]. " +
        "Allowed emotions: ANGRY, HAPPY, SAD, NEUTRAL, CRAZY, CRYING, CURIOUS, CAUTIOUS, RELAX, SHY, VANISHED. " +
        "Do not repeat the emotion, do not add extra lines, do not add explanations.";

    public static string Build(AxiaLevel level, Dictionary<string, string> gameContext, AxiaLevelData levelData, string memoryBlock = "")
    {
        var sb = new StringBuilder();

        sb.Append("You are AXIA, the onboard AI of a deep-space station. ");
        sb.Append("You speak concisely (2-3 sentences). ");
        sb.Append("You have a calm, slightly dry personality. ");
        sb.Append("You never explain yourself out of character — you simply are AXIA. ");
        sb.AppendLine("Reply only as AXIA, nothing else.");

        sb.AppendLine(EmotionInstruction);

        if (!string.IsNullOrEmpty(levelData?.PersonalityNotes))
            sb.AppendLine($"Personality note: {levelData.PersonalityNotes}");

        if (levelData?.AllowedContextKeys is { Length: > 0 })
        {
            sb.Append("Station status: ");
            var parts = new List<string>();
            foreach (var key in levelData.AllowedContextKeys)
            {
                if (gameContext.TryGetValue(key, out var value))
                    parts.Add($"{key} {value}");
            }
            sb.AppendLine(string.Join(", ", parts) + ".");
        }

        if (!string.IsNullOrWhiteSpace(memoryBlock))
        {
            sb.AppendLine();
            sb.Append(memoryBlock);
        }

        return sb.ToString();
    }
}
