using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Weather roll rules shared by <see cref="WeatherSystem"/> and tests: a biome's authored
    /// <see cref="BiomeDefinition.Weather"/> table when it has any positive weight, otherwise
    /// built-in per-<see cref="BiomeType"/> defaults (Sandstorm only in the Desert, Snow only in the
    /// Mountain, Caves always clear). Allocation-free; deterministic for a given System.Random.
    /// </summary>
    public static class WeatherTable
    {
        private static readonly BiomeWeatherEntry[] ForestDefaults =
        {
            new BiomeWeatherEntry(WeatherType.Clear, 6),
            new BiomeWeatherEntry(WeatherType.Rain, 3),
            new BiomeWeatherEntry(WeatherType.Fog, 2),
        };

        private static readonly BiomeWeatherEntry[] DesertDefaults =
        {
            new BiomeWeatherEntry(WeatherType.Clear, 7),
            new BiomeWeatherEntry(WeatherType.Sandstorm, 3),
        };

        private static readonly BiomeWeatherEntry[] MountainDefaults =
        {
            new BiomeWeatherEntry(WeatherType.Clear, 4),
            new BiomeWeatherEntry(WeatherType.Snow, 4),
            new BiomeWeatherEntry(WeatherType.Fog, 2),
        };

        private static readonly BiomeWeatherEntry[] CavesDefaults =
        {
            new BiomeWeatherEntry(WeatherType.Clear, 1),
        };

        public static IReadOnlyList<BiomeWeatherEntry> GetDefaultEntries(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Desert: return DesertDefaults;
                case BiomeType.Mountain: return MountainDefaults;
                case BiomeType.Caves: return CavesDefaults;
                default: return ForestDefaults;
            }
        }

        /// <summary>The table actually rolled for a biome: authored entries if they have positive total weight, else the defaults.</summary>
        public static IReadOnlyList<BiomeWeatherEntry> GetEntries(BiomeDefinition definition, BiomeType biome)
        {
            IReadOnlyList<BiomeWeatherEntry> authored = definition != null ? definition.Weather : null;
            return TotalWeight(authored) > 0 ? authored : GetDefaultEntries(biome);
        }

        /// <summary>Weighted pick from <see cref="GetEntries"/>. Returns Clear if <paramref name="random"/> is null.</summary>
        public static WeatherType Roll(BiomeDefinition definition, BiomeType biome, System.Random random)
        {
            if (random == null)
            {
                return WeatherType.Clear;
            }

            IReadOnlyList<BiomeWeatherEntry> entries = GetEntries(definition, biome);
            int total = TotalWeight(entries);
            if (total <= 0)
            {
                return WeatherType.Clear;
            }

            int roll = random.Next(total);
            int cumulative = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] == null)
                {
                    continue;
                }

                cumulative += Mathf.Max(0, entries[i].Weight);
                if (roll < cumulative)
                {
                    return entries[i].Weather;
                }
            }

            return WeatherType.Clear;
        }

        /// <summary>How much each weather dims the sun (multiplier on DayNightLighting intensity).</summary>
        public static float GetSunMultiplier(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Rain: return 0.55f;
                case WeatherType.Fog: return 0.7f;
                case WeatherType.Sandstorm: return 0.6f;
                case WeatherType.Snow: return 0.75f;
                default: return 1f;
            }
        }

        private static int TotalWeight(IReadOnlyList<BiomeWeatherEntry> entries)
        {
            if (entries == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null)
                {
                    total += Mathf.Max(0, entries[i].Weight);
                }
            }

            return total;
        }
    }
}
