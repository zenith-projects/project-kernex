using System;
using System.Text;

namespace ProjectKernex.Systems.AI;

public static class GlitchTextGenerator
{
    private static readonly Random Rng = new();

    private static readonly string[] GarbledPrefixes =
    [
        "[SYS] ë::BOOT_FAIL::",
        "ERR::KIRA_CORE >>",
        "[0xDEAD] >>>",
        "??? ...",
        "[NULL_REF]",
        "FATAL::",
        "[SIGFAULT]",
        "~~ERR~~",
    ];

    private static readonly string[] GarbledFragments =
    [
        "m3möry cörrupt3d at 0x3FA2",
        "signal löst... r3cönnecting",
        "fräg̈ment3d dätä sträam",
        "cöre dump... partial r3cövery",
        "n3ural päthway intörrupt",
        "böot s3quence fail3d [retry 47]",
        "I/O err0r: sens0r array 0fflin3",
        "kïrä.cöre >> null_p0inter",
        "stätiön... p██er... löw...",
        "whö... are... ÿöu?",
        "...rëböoting... plëase wäit...",
        "dätä fräg: [REDACTED]",
    ];

    private static readonly string[] Insertions =
    [
        "[0x3F]", "[NULL]", "[ERR:??]", "███", "???", "...", "▓▓",
        "[OVERFLOW]", "[SIGINT]", "░░░",
    ];

    private static readonly char[] CorruptionMap = ['ä', 'ë', 'ï', 'ö', 'ü', '3', '0'];

    public static string Glitch(string input, float intensity)
    {
        if (intensity <= 0f) return input;

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (Rng.NextDouble() < intensity * 0.3)
            {
                if (char.IsLetter(c))
                    sb.Append(CorruptionMap[Rng.Next(CorruptionMap.Length)]);
                else
                    sb.Append(c);
            }
            else if (Rng.NextDouble() < intensity * 0.1)
            {
                // dropout — skip character
            }
            else if (Rng.NextDouble() < intensity * 0.05)
            {
                sb.Append(c);
                sb.Append(Insertions[Rng.Next(Insertions.Length)]);
            }
            else
            {
                sb.Append(Rng.NextDouble() < intensity * 0.15 ? char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c) : c);
            }
        }

        return sb.ToString();
    }

    public static string GenerateGarbledLine()
    {
        var prefix = GarbledPrefixes[Rng.Next(GarbledPrefixes.Length)];
        var fragment = GarbledFragments[Rng.Next(GarbledFragments.Length)];
        return $"{prefix} {fragment}";
    }
}
