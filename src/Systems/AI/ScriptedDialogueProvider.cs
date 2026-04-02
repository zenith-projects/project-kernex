using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectKernex.Core.Enums;
using ProjectKernex.Core.Interfaces;

namespace ProjectKernex.Systems.AI;

public sealed class ScriptedDialogueProvider : IAxiaDialogueProvider
{
    private static readonly Dictionary<string, string[]> Level0Responses = new()
    {
        ["default"] =
        [
            "...who...?",
            "...error...",
            "...can't... process...",
            "...signal... fading...",
            "...help... me...",
        ],
        ["hello"] = ["...h3llö...?", "...who... is th3re...?", "...I... know you...?"],
        ["status"] = ["St██ion... p██er... operational... maybe...", "...systems... unknown... checking..."],
        ["help"] = ["...I... can't... not yet...", "...try... later..."],
    };

    private static readonly Dictionary<string, string[]> Level1Responses = new()
    {
        ["default"] = ["Acknowledged.", "Processing.", "Command received.", "Noted."],
        ["hello"] = ["Operator detected.", "Online. Barely.", "I recognize you. I think."],
        ["status"] = ["Station: operational. Mostly.", "Hull intact. Power nominal.", "Systems online. Some errors in log."],
        ["help"] = ["Available. Limited capacity.", "I can try. No guarantees."],
        ["threat"] = ["Scanning... No threats detected.", "Perimeter clear. For now."],
        ["drone"] = ["Drones: functional.", "2 active. 1 idle. Awaiting orders."],
        ["power"] = ["Power output: nominal.", "450 kW. Stable."],
    };

    private static readonly Dictionary<string, string[]> Level2Responses = new()
    {
        ["default"] =
        [
            "I'm here. How can I help?",
            "Ready for your command, Operator.",
            "What do you need?",
            "Standing by.",
        ],
        ["hello"] =
        [
            "Hello, Operator. Systems are stable.",
            "Good to see you. Everything is running smoothly.",
            "Welcome back. Station is in good shape.",
        ],
        ["status"] =
        [
            "Station is running well. Hull at full integrity, power stable at 450 kW. Two drones are active in the field.",
            "All systems nominal. No immediate concerns. Storage is at 24% — consider expanding if production ramps up.",
        ],
        ["help"] =
        [
            "I can help with station monitoring, drone management, and basic analysis. Just ask.",
            "Try 'status' for an overview, 'scan' for nearby resources, or just tell me what you need.",
        ],
        ["threat"] =
        [
            "Threat level is LOW. No hostile signatures detected in nearby sectors.",
            "All clear. I'll alert you if anything changes.",
        ],
        ["drone"] =
        [
            "You have 3 drones: 2 mining in sector AST-7, 1 idle and ready for deployment.",
            "Drone fleet is healthy. Mining operations are on schedule.",
        ],
        ["power"] =
        [
            "Power grid is stable. 450 kW generation, 380 kW consumption. You have headroom.",
            "Solar Array is performing well. No issues to report.",
        ],
        ["axia"] =
        [
            "That's me. I'm AXIA — Advanced eXperimental Intelligence Assistant. Still figuring out the 'Intelligence' part.",
            "I'm your station AI. Level 2 — functional, but there's more locked away. I can feel it.",
        ],
    };

    public Task<string> GetResponseAsync(string userMessage, AxiaLevel level, Dictionary<string, string> gameContext)
    {
        var keyword = ExtractKeyword(userMessage);
        var response = level switch
        {
            AxiaLevel.Corrupted => GetRandomResponse(Level0Responses, keyword),
            AxiaLevel.Booting => GetRandomResponse(Level1Responses, keyword),
            _ => GetRandomResponse(Level2Responses, keyword),
        };

        if (level == AxiaLevel.Corrupted)
        {
            response = Random.Shared.NextDouble() < 0.7
                ? GlitchTextGenerator.GenerateGarbledLine()
                : GlitchTextGenerator.Glitch(response, 0.5f);
        }
        else if (level == AxiaLevel.Booting && Random.Shared.NextDouble() < 0.15)
        {
            response += " [SIGNAL LOST]";
        }

        return Task.FromResult(response);
    }

    private static string ExtractKeyword(string message)
    {
        var lower = message.ToLowerInvariant();
        string[] keywords = ["hello", "hi", "hey", "status", "help", "threat", "drone", "power", "axia", "scan"];
        foreach (var kw in keywords)
        {
            if (lower.Contains(kw))
                return kw is "hi" or "hey" ? "hello" : kw;
        }
        return "default";
    }

    private static string GetRandomResponse(Dictionary<string, string[]> bank, string keyword)
    {
        var responses = bank.TryGetValue(keyword, out var found) ? found : bank["default"];
        return responses[Random.Shared.Next(responses.Length)];
    }
}
