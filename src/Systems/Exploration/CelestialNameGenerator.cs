using System;

namespace ProjectKernex.Systems.Exploration;

public static class CelestialNameGenerator
{
    private static readonly string[] Prefixes =
    [
        "KEPH", "VOSS", "THAL", "NARA", "ZEPH", "CRON", "XAEL", "MIRA",
        "PHOS", "AEON", "DRAK", "LYRA", "ORIX", "SAEL", "VAEL", "TYRA",
        "NEPH", "GALT", "RUNE", "APEX", "FLUX", "NOVA", "HELM", "CORE",
        "BOLT", "CRUX", "FADE", "GRIM", "HAZE", "JINX", "KELP", "LOOM",
    ];

    public static string GenerateSystemName(long seed)
    {
        var rng = new Random((int)(seed & 0x7FFFFFFF));
        var prefix = Prefixes[rng.Next(Prefixes.Length)];
        var number = rng.Next(1, 100);
        return $"{prefix}-{number}";
    }

    public static string GeneratePlanetName(string systemName, int index)
    {
        var suffix = (char)('b' + index);
        return $"{systemName}{suffix}";
    }
}
