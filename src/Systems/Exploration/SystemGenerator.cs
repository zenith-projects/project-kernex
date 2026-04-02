using System;
using Godot;
using ProjectKernex.Core.Enums;
using ProjectKernex.Resources.World;

namespace ProjectKernex.Systems.Exploration;

public static class SystemGenerator
{
    // Weighted distribution for celestial body types (more common = higher weight)
    private static readonly (CelestialBodyType type, int weight)[] BodyWeights =
    [
        (CelestialBodyType.RedDwarf, 30),
        (CelestialBodyType.OrangeDwarf, 20),
        (CelestialBodyType.YellowDwarf, 15),
        (CelestialBodyType.BrownDwarf, 10),
        (CelestialBodyType.RedGiant, 7),
        (CelestialBodyType.WhiteDwarf, 5),
        (CelestialBodyType.BlueGiant, 4),
        (CelestialBodyType.NeutronStar, 3),
        (CelestialBodyType.ProtostellarNebula, 3),
        (CelestialBodyType.Pulsar, 1),
        (CelestialBodyType.BlackHole, 1),
        (CelestialBodyType.Quasar, 1),
    ];

    public static SolarSystemData Generate(long seed)
    {
        var rng = new Random((int)(seed & 0x7FFFFFFF));
        var systemName = CelestialNameGenerator.GenerateSystemName(seed);

        var bodyType = PickWeighted(rng, BodyWeights);
        var centralBody = GenerateCentralBody(rng, bodyType, seed);
        centralBody.Name = systemName;

        var planets = GeneratePlanets(rng, centralBody, systemName);
        var spacing = 0.8f + (float)rng.NextDouble() * 0.6f;

        return new SolarSystemData
        {
            Seed = seed,
            SystemName = systemName,
            CentralBody = centralBody,
            Planets = new Godot.Collections.Array<PlanetData>(planets),
            SystemSpacing = spacing,
        };
    }

    private static CelestialBodyData GenerateCentralBody(Random rng, CelestialBodyType type, long seed)
    {
        var (radius, mass, temp, luminosity, coreColor, coronaColor, pulse, accretion, maxPlanets, hzStart, hzEnd) = type switch
        {
            // Realistic radii in R☉: Sun=1.0, Jupiter≈0.1, Earth≈0.009
            // maxPlanets doubled from original

            CelestialBodyType.RedDwarf => (
                0.12f + Rf(rng, 0.4f), 0.1f + Rf(rng, 0.5f), 2500f + Rf(rng, 1200f), 0.01f + Rf(rng, 0.08f),
                new Color(1f, 0.3f, 0.1f), new Color(1f, 0.5f, 0.2f), 0f, false, 8, 0.05f, 0.25f),

            CelestialBodyType.OrangeDwarf => (
                0.7f + Rf(rng, 0.2f), 0.5f + Rf(rng, 0.4f), 3900f + Rf(rng, 1300f), 0.15f + Rf(rng, 0.35f),
                new Color(1f, 0.6f, 0.2f), new Color(1f, 0.7f, 0.3f), 0f, false, 12, 0.4f, 1.0f),

            CelestialBodyType.YellowDwarf => (
                0.9f + Rf(rng, 0.3f), 0.8f + Rf(rng, 0.4f), 5200f + Rf(rng, 1200f), 0.6f + Rf(rng, 0.8f),
                new Color(1f, 0.95f, 0.7f), new Color(1f, 1f, 0.8f), 0f, false, 16, 0.8f, 1.5f),

            CelestialBodyType.BlueGiant => (
                5f + Rf(rng, 10f), 5f + Rf(rng, 50f), 15000f + Rf(rng, 30000f), 1000f + Rf(rng, 50000f),
                new Color(0.6f, 0.7f, 1f), new Color(0.7f, 0.8f, 1f), 0f, false, 8, 5f, 20f),

            CelestialBodyType.RedGiant => (
                20f + Rf(rng, 180f), 0.5f + Rf(rng, 5f), 3000f + Rf(rng, 1500f), 100f + Rf(rng, 2000f),
                new Color(1f, 0.25f, 0.05f), new Color(1f, 0.4f, 0.1f), 0.03f + Rf(rng, 0.05f), false, 10, 10f, 50f),

            CelestialBodyType.WhiteDwarf => (
                0.008f + Rf(rng, 0.012f), 0.5f + Rf(rng, 0.8f), 8000f + Rf(rng, 32000f), 0.001f + Rf(rng, 0.03f),
                new Color(0.9f, 0.9f, 1f), new Color(0.8f, 0.85f, 1f), 0f, false, 6, 0.005f, 0.02f),

            CelestialBodyType.NeutronStar => (
                0.000015f + Rf(rng, 0.000005f), 1.4f + Rf(rng, 0.8f), 600000f + Rf(rng, 600000f), 0.0001f + Rf(rng, 0.001f),
                new Color(0.8f, 0.85f, 1f), new Color(0.5f, 0.6f, 1f), 0.3f + Rf(rng, 0.5f), false, 4, 0f, 0f),

            CelestialBodyType.Pulsar => (
                0.000015f + Rf(rng, 0.000005f), 1.4f + Rf(rng, 0.8f), 600000f + Rf(rng, 600000f), 0.0001f + Rf(rng, 0.001f),
                new Color(0.7f, 0.8f, 1f), new Color(0.4f, 0.5f, 1f), 1f + Rf(rng, 4f), false, 4, 0f, 0f),

            CelestialBodyType.BlackHole => (
                0.00004f + Rf(rng, 0.0001f), 5f + Rf(rng, 50f), 0f, 0f,
                new Color(0f, 0f, 0f), new Color(1f, 0.5f, 0.1f), 0f, true, 6, 0f, 0f),

            CelestialBodyType.Quasar => (
                0.0001f + Rf(rng, 0.0005f), 100f + Rf(rng, 900f), 0f, 10000f + Rf(rng, 90000f),
                new Color(0.2f, 0.1f, 0.5f), new Color(0.8f, 0.5f, 1f), 0.1f + Rf(rng, 0.3f), true, 2, 0f, 0f),

            CelestialBodyType.BrownDwarf => (
                0.08f + Rf(rng, 0.04f), 0.01f + Rf(rng, 0.07f), 500f + Rf(rng, 2000f), 0.0001f + Rf(rng, 0.001f),
                new Color(0.4f, 0.15f, 0.05f), new Color(0.5f, 0.2f, 0.1f), 0f, false, 6, 0.01f, 0.05f),

            CelestialBodyType.ProtostellarNebula => (
                5f + Rf(rng, 15f), 0.3f + Rf(rng, 2f), 1500f + Rf(rng, 2500f), 0.5f + Rf(rng, 5f),
                new Color(0.3f, 0.2f, 0.5f), new Color(0.5f, 0.3f, 0.6f), 0.02f + Rf(rng, 0.05f), false, 12, 0.3f, 2f),

            _ => (1f, 1f, 5778f, 1f, Colors.White, Colors.Yellow, 0f, false, 10, 0.8f, 1.5f),
        };

        return new CelestialBodyData
        {
            Type = type,
            Seed = seed,
            Radius = radius,
            Mass = mass,
            Temperature = temp,
            Luminosity = luminosity,
            CoreColor = coreColor,
            CoronaColor = coronaColor,
            PulseSpeed = pulse,
            HasAccretionDisk = accretion,
            MaxPlanets = maxPlanets,
            HabitableZoneStart = hzStart,
            HabitableZoneEnd = hzEnd,
        };
    }

    private static PlanetData[] GeneratePlanets(Random rng, CelestialBodyData body, string systemName)
    {
        var count = rng.Next(0, body.MaxPlanets + 1);
        if (count == 0) return [];

        var planets = new PlanetData[count];
        var slotsRemaining = count;

        // Titius-Bode law: d = a + b * 2^n (AU)
        // Real solar system: 0.4, 0.7, 1.0, 1.6, 2.8, 5.2, 10.0, 19.6
        // We scale by star mass and add random variation
        var baseDistance = 0.2f + Rf(rng, 0.2f);  // inner edge (AU)
        var scaleFactor = 0.3f + body.Mass * 0.4f + Rf(rng, 0.2f); // spacing multiplier

        for (int i = 0; i < count && slotsRemaining > 0; i++)
        {
            // Titius-Bode: distance = base + scale * 2^n with perturbation
            var titiusBode = baseDistance + scaleFactor * Mathf.Pow(2f, i) * (0.8f + Rf(rng, 0.4f));

            var planetSeed = body.Seed * 31 + i * 7919;
            var planetRng = new Random((int)(planetSeed & 0x7FFFFFFF));

            var type = PickPlanetType(planetRng, body, titiusBode);
            var planet = GeneratePlanet(planetRng, type, planetSeed, body, titiusBode, i);
            planet.Name = CelestialNameGenerator.GeneratePlanetName(systemName, i);

            // Orbital speed from Kepler's 3rd law: v ∝ 1/√d
            planet.OrbitalSpeed = 0.5f / Mathf.Sqrt(Mathf.Max(titiusBode, 0.1f));

            planets[i] = planet;

            if (type == PlanetType.GasGiant && planet.Radius > 5f)
                slotsRemaining -= 2;
            else
                slotsRemaining--;
        }

        // Trim if gas giant consumed extra slots
        var actual = 0;
        for (int i = 0; i < planets.Length; i++)
        {
            if (planets[i] != null) actual++;
            else break;
        }
        if (actual < planets.Length)
            Array.Resize(ref planets, actual);

        return planets;
    }

    private static PlanetType PickPlanetType(Random rng, CelestialBodyData body, float distance)
    {
        var inHZ = distance >= body.HabitableZoneStart && distance <= body.HabitableZoneEnd;
        var close = distance < body.HabitableZoneStart;

        // Build weighted list based on body type and orbital distance
        (PlanetType type, int weight)[] weights = body.Type switch
        {
            CelestialBodyType.RedDwarf => close
                ? [(PlanetType.Rocky, 40), (PlanetType.Toxic, 20), (PlanetType.Lava, 15), (PlanetType.IceWorld, 15), (PlanetType.Desert, 10)]
                : [(PlanetType.IceWorld, 40), (PlanetType.Rocky, 25), (PlanetType.Toxic, 15), (PlanetType.Desert, 10), (PlanetType.Crystalline, 10)],

            CelestialBodyType.OrangeDwarf => inHZ
                ? [(PlanetType.Ocean, 30), (PlanetType.Rocky, 30), (PlanetType.Desert, 15), (PlanetType.Toxic, 10), (PlanetType.GasGiant, 10), (PlanetType.Crystalline, 5)]
                : [(PlanetType.Rocky, 25), (PlanetType.GasGiant, 20), (PlanetType.IceWorld, 20), (PlanetType.Desert, 15), (PlanetType.Toxic, 10), (PlanetType.Crystalline, 10)],

            CelestialBodyType.YellowDwarf => inHZ
                ? [(PlanetType.Ocean, 25), (PlanetType.Rocky, 25), (PlanetType.Desert, 15), (PlanetType.GasGiant, 10), (PlanetType.Toxic, 10), (PlanetType.Crystalline, 5), (PlanetType.IceWorld, 5), (PlanetType.Lava, 5)]
                : close
                    ? [(PlanetType.Rocky, 30), (PlanetType.Lava, 25), (PlanetType.Desert, 20), (PlanetType.Toxic, 15), (PlanetType.Crystalline, 10)]
                    : [(PlanetType.GasGiant, 30), (PlanetType.IceWorld, 30), (PlanetType.Rocky, 15), (PlanetType.Crystalline, 15), (PlanetType.Toxic, 10)],

            CelestialBodyType.BlueGiant =>
                [(PlanetType.GasGiant, 30), (PlanetType.Lava, 25), (PlanetType.Crystalline, 20), (PlanetType.Rocky, 15), (PlanetType.Desert, 10)],

            CelestialBodyType.RedGiant =>
                [(PlanetType.GasGiant, 30), (PlanetType.IceWorld, 25), (PlanetType.Rocky, 20), (PlanetType.Desert, 15), (PlanetType.Crystalline, 10)],

            CelestialBodyType.WhiteDwarf =>
                [(PlanetType.Rocky, 40), (PlanetType.IceWorld, 30), (PlanetType.Crystalline, 15), (PlanetType.Desert, 15)],

            CelestialBodyType.NeutronStar or CelestialBodyType.Pulsar =>
                [(PlanetType.Rocky, 40), (PlanetType.Crystalline, 30), (PlanetType.Lava, 20), (PlanetType.IceWorld, 10)],

            CelestialBodyType.BlackHole =>
                [(PlanetType.GasGiant, 30), (PlanetType.Lava, 30), (PlanetType.Crystalline, 25), (PlanetType.Rocky, 15)],

            CelestialBodyType.Quasar =>
                [(PlanetType.Lava, 40), (PlanetType.Crystalline, 40), (PlanetType.GasGiant, 20)],

            CelestialBodyType.BrownDwarf =>
                [(PlanetType.Rocky, 40), (PlanetType.IceWorld, 30), (PlanetType.Desert, 20), (PlanetType.Toxic, 10)],

            CelestialBodyType.ProtostellarNebula =>
                [(PlanetType.Rocky, 35), (PlanetType.GasGiant, 25), (PlanetType.Lava, 15), (PlanetType.IceWorld, 10), (PlanetType.Desert, 10), (PlanetType.Toxic, 5)],

            _ => [(PlanetType.Rocky, 30), (PlanetType.GasGiant, 20), (PlanetType.IceWorld, 15), (PlanetType.Desert, 10), (PlanetType.Lava, 10), (PlanetType.Toxic, 5), (PlanetType.Ocean, 5), (PlanetType.Crystalline, 5)],
        };

        return PickWeighted(rng, weights);
    }

    private static PlanetData GeneratePlanet(Random rng, PlanetType type, long seed, CelestialBodyData body, float distance, int index)
    {
        // Realistic radii in R⊕: Earth=1.0, Mars=0.53, Jupiter=11.2, Neptune=3.88, Mercury=0.38
        var (radiusMin, radiusMax, primary, secondary, atmosphere, atmosIntensity, clouds, ringsChance) = type switch
        {
            PlanetType.Rocky => (0.4f, 1.8f,
                new Color(0.5f + Rf(rng, 0.2f), 0.4f + Rf(rng, 0.15f), 0.3f + Rf(rng, 0.15f)),
                new Color(0.35f + Rf(rng, 0.15f), 0.3f + Rf(rng, 0.1f), 0.25f + Rf(rng, 0.1f)),
                new Color(0.5f, 0.6f, 0.9f, 0.15f), 0.1f + Rf(rng, 0.2f), Rf(rng, 0.15f), 0.02f),

            PlanetType.GasGiant => (3.5f, 11.5f,
                new Color(0.8f + Rf(rng, 0.15f), 0.6f + Rf(rng, 0.2f), 0.3f + Rf(rng, 0.2f)),
                new Color(0.6f + Rf(rng, 0.2f), 0.4f + Rf(rng, 0.15f), 0.2f + Rf(rng, 0.15f)),
                new Color(0.8f, 0.7f, 0.5f, 0.3f), 0.6f + Rf(rng, 0.3f), 0.7f + Rf(rng, 0.3f), 0.3f),

            PlanetType.IceWorld => (0.3f, 4.0f,
                new Color(0.7f + Rf(rng, 0.2f), 0.8f + Rf(rng, 0.15f), 0.95f + Rf(rng, 0.05f)),
                new Color(0.5f + Rf(rng, 0.15f), 0.6f + Rf(rng, 0.15f), 0.8f + Rf(rng, 0.15f)),
                new Color(0.7f, 0.8f, 1f, 0.2f), 0.15f + Rf(rng, 0.15f), Rf(rng, 0.1f), 0.05f),

            PlanetType.Lava => (0.3f, 1.5f,
                new Color(0.3f + Rf(rng, 0.1f), 0.05f + Rf(rng, 0.05f), 0.02f),
                new Color(1f, 0.4f + Rf(rng, 0.3f), 0f),
                new Color(0.8f, 0.2f, 0.05f, 0.3f), 0.3f + Rf(rng, 0.2f), 0.1f + Rf(rng, 0.15f), 0.01f),

            PlanetType.Ocean => (0.8f, 2.0f,
                new Color(0.1f + Rf(rng, 0.1f), 0.2f + Rf(rng, 0.15f), 0.6f + Rf(rng, 0.2f)),
                new Color(0.05f + Rf(rng, 0.1f), 0.15f + Rf(rng, 0.1f), 0.5f + Rf(rng, 0.2f)),
                new Color(0.6f, 0.7f, 1f, 0.3f), 0.5f + Rf(rng, 0.3f), 0.4f + Rf(rng, 0.4f), 0.03f),

            PlanetType.Desert => (0.4f, 1.6f,
                new Color(0.85f + Rf(rng, 0.1f), 0.7f + Rf(rng, 0.15f), 0.4f + Rf(rng, 0.15f)),
                new Color(0.7f + Rf(rng, 0.15f), 0.55f + Rf(rng, 0.1f), 0.3f + Rf(rng, 0.1f)),
                new Color(0.9f, 0.8f, 0.6f, 0.1f), 0.05f + Rf(rng, 0.1f), Rf(rng, 0.05f), 0.01f),

            PlanetType.Toxic => (0.8f, 1.2f,
                new Color(0.4f + Rf(rng, 0.2f), 0.6f + Rf(rng, 0.2f), 0.1f + Rf(rng, 0.15f)),
                new Color(0.3f + Rf(rng, 0.15f), 0.5f + Rf(rng, 0.15f), 0.05f + Rf(rng, 0.1f)),
                new Color(0.5f, 0.7f, 0.2f, 0.5f), 0.7f + Rf(rng, 0.3f), 0.5f + Rf(rng, 0.4f), 0.01f),

            PlanetType.Crystalline => (0.2f, 1.0f,
                new Color(0.5f + Rf(rng, 0.3f), 0.3f + Rf(rng, 0.3f), 0.7f + Rf(rng, 0.25f)),
                new Color(0.3f + Rf(rng, 0.3f), 0.6f + Rf(rng, 0.3f), 0.8f + Rf(rng, 0.15f)),
                new Color(0.6f, 0.4f, 0.9f, 0.25f), 0.2f + Rf(rng, 0.2f), Rf(rng, 0.1f), 0.08f),

            _ => (0.5f, 1f, Colors.Gray, Colors.DarkGray, new Color(0.5f, 0.5f, 0.5f, 0.2f), 0.1f, 0.1f, 0.05f),
        };

        var radius = radiusMin + (float)rng.NextDouble() * (radiusMax - radiusMin);
        var mass = radius * radius * (type == PlanetType.GasGiant ? 0.3f : 1.5f);
        var temp = body.Luminosity > 0 ? body.Temperature * 0.7f / (1 + distance * distance) : 50f + Rf(rng, 100f);
        var habitable = temp > 200f && temp < 400f && atmosIntensity > 0.2f;
        var moonCount = rng.Next(0, (int)(radius * 3) + 1);

        // Ring system: chance and size depend on planet type and radius
        // Larger rings are rarer. Gas giants are most likely to have rings.
        var (ringBaseChance, ringMinScale, ringMaxScale) = type switch
        {
            PlanetType.GasGiant    => (0.35f, 0.3f, 1.0f),   // Saturn/Jupiter style, common
            PlanetType.IceWorld    => (0.08f, 0.2f, 0.6f),    // icy debris rings
            PlanetType.Crystalline => (0.12f, 0.2f, 0.7f),    // crystal shard rings
            PlanetType.Rocky       => (0.02f, 0.1f, 0.3f),    // very rare, thin
            PlanetType.Desert      => (0.01f, 0.1f, 0.25f),   // extremely rare
            PlanetType.Ocean       => (0.03f, 0.15f, 0.4f),   // rare, icy
            PlanetType.Toxic       => (0.01f, 0.1f, 0.2f),    // almost never
            PlanetType.Lava        => (0.005f, 0.1f, 0.15f),  // nearly impossible (too hot)
            _ => (0.02f, 0.1f, 0.3f),
        };

        // Bigger planets → higher chance but bigger rings are rarer
        var sizeBonus = Mathf.Clamp(radius / 5f, 0f, 0.3f); // up to +30% for huge planets
        var finalChance = ringBaseChance + sizeBonus;
        var hasRings = (float)rng.NextDouble() < finalChance;

        var ringScale = 0f;
        if (hasRings)
        {
            // Roll ring size — bigger rings exponentially rarer
            var rawScale = ringMinScale + (float)rng.NextDouble() * (ringMaxScale - ringMinScale);
            // Square the roll so large values are rare (0.5 → 0.25, 0.8 → 0.64, 1.0 → 1.0)
            ringScale = ringMinScale + (rawScale - ringMinScale) * (rawScale - ringMinScale)
                        / ((ringMaxScale - ringMinScale) > 0.001f ? (ringMaxScale - ringMinScale) : 1f);
            ringScale = Mathf.Clamp(ringScale, ringMinScale, ringMaxScale);
        }

        return new PlanetData
        {
            Type = type,
            Seed = seed,
            Radius = radius,
            Mass = mass,
            OrbitalDistance = distance,
            OrbitalSpeed = 0.5f / (0.5f + distance),
            RotationSpeed = 0.05f + Rf(rng, 0.15f),
            Temperature = temp,
            PrimaryColor = primary,
            SecondaryColor = secondary,
            AtmosphereColor = atmosphere,
            AtmosphereIntensity = atmosIntensity,
            CloudCoverage = clouds,
            HasRings = hasRings,
            RingScale = ringScale,
            HasMoons = moonCount > 0,
            MoonCount = moonCount,
            ResourceRichness = 0.1f + Rf(rng, 0.8f),
            ThreatLevel = Rf(rng, 0.5f) + (body.Type == CelestialBodyType.Pulsar || body.Type == CelestialBodyType.Quasar ? 0.3f : 0f),
            IsHabitable = habitable,
        };
    }

    private static T PickWeighted<T>(Random rng, (T item, int weight)[] options)
    {
        var total = 0;
        foreach (var (_, w) in options) total += w;
        var roll = rng.Next(total);
        var cumulative = 0;
        foreach (var (item, w) in options)
        {
            cumulative += w;
            if (roll < cumulative) return item;
        }
        return options[^1].item;
    }

    private static float Rf(Random rng, float range) => (float)rng.NextDouble() * range;
}
