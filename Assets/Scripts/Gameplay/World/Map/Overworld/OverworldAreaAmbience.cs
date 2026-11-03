using System.Collections.Generic;
using Rapadura.Gameplay.Environment;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>How an area feels (picks the music, <c>Rapadura.Audio.AreaMusicRules</c>).</summary>
    public enum AreaMood
    {
        /// <summary>Inside a town's or post's safe zone.</summary>
        Town = 0,
        /// <summary>Open, calm country (orchards, fields, meadows, oases).</summary>
        Field = 1,
        /// <summary>Wild, gloomy ground (closed woods, ravines, marshes, ergs, chasms, summits).</summary>
        Wild = 2,
        /// <summary>The area's monsters are far above the hero (<see cref="MapAccessRules.GetMonsterLevelDeltaPenalty"/>).</summary>
        Danger = 3,
    }

    /// <summary>The weather and feel of one kind of ground.</summary>
    public readonly struct AreaClimate
    {
        /// <summary>The weather the area holds (fog, dust, snow, the caves' damp mist, clear), or null: the region's biome rolls it.</summary>
        public readonly WeatherType? Weather;

        /// <summary>Wild ground (<see cref="AreaMood.Wild"/>).</summary>
        public readonly bool Wild;

        public AreaClimate(WeatherType? weather, bool wild)
        {
            Weather = weather;
            Wild = wild;
        }
    }

    /// <summary>
    /// Climate and mood of each kind of ground of the continuous regions (pure, EditMode-tested: every
    /// <see cref="OverworldTerrain"/> has a line). The weather reuses the biome weather (<see cref="WeatherType"/>: the
    /// existing fog / sandstorm / snow visuals and ambient particles); the mood picks the music among the existing tracks.
    /// </summary>
    public static class OverworldAreaAmbience
    {
        private const WeatherType Mist = WeatherType.Fog;
        private const WeatherType Dust = WeatherType.Sandstorm;
        private const WeatherType Snow = WeatherType.Snow;
        private const WeatherType Clear = WeatherType.Clear;

        /// <summary>The caves' damp air: the fog visuals, under the vault.</summary>
        private const WeatherType Damp = WeatherType.Fog;

        private static readonly Dictionary<OverworldTerrain, AreaClimate> Table = new Dictionary<OverworldTerrain, AreaClimate>
        {
            // Região 1 — Zarafshan
            { OverworldTerrain.Orchard, new AreaClimate(null, false) },
            { OverworldTerrain.Fields, new AreaClimate(null, false) },
            { OverworldTerrain.Steppe, new AreaClimate(null, false) },
            { OverworldTerrain.Meadow, new AreaClimate(null, false) },
            { OverworldTerrain.Riverbank, new AreaClimate(null, false) },
            { OverworldTerrain.Floodplain, new AreaClimate(Mist, false) },
            { OverworldTerrain.Hills, new AreaClimate(null, false) },
            { OverworldTerrain.HighHills, new AreaClimate(null, true) },
            { OverworldTerrain.Ruins, new AreaClimate(null, true) },
            { OverworldTerrain.DenseWood, new AreaClimate(Mist, true) },
            { OverworldTerrain.Ravine, new AreaClimate(Mist, true) },
            { OverworldTerrain.Marsh, new AreaClimate(Mist, true) },
            { OverworldTerrain.Mulberry, new AreaClimate(null, false) },
            { OverworldTerrain.Canopy, new AreaClimate(Mist, false) },
            { OverworldTerrain.Garden, new AreaClimate(Clear, true) },

            // Região 2 — Areias Flamejantes
            { OverworldTerrain.CaravanSteppe, new AreaClimate(null, false) },
            { OverworldTerrain.Oasis, new AreaClimate(Clear, false) },
            { OverworldTerrain.PalmGrove, new AreaClimate(Clear, false) },
            { OverworldTerrain.StoneDesert, new AreaClimate(null, false) },
            { OverworldTerrain.LowDunes, new AreaClimate(null, false) },
            { OverworldTerrain.Erg, new AreaClimate(Dust, true) },
            { OverworldTerrain.RedCanyon, new AreaClimate(null, true) },
            { OverworldTerrain.SaltFlat, new AreaClimate(Clear, false) },
            { OverworldTerrain.MeroeRuins, new AreaClimate(null, true) },
            { OverworldTerrain.Mesas, new AreaClimate(null, false) },
            { OverworldTerrain.KarezField, new AreaClimate(null, false) },
            { OverworldTerrain.TamariskOasis, new AreaClimate(Clear, false) },
            { OverworldTerrain.BoneValley, new AreaClimate(Dust, true) },
            { OverworldTerrain.KarezGallery, new AreaClimate(null, true) },
            { OverworldTerrain.SingingDunes, new AreaClimate(Dust, true) },
            { OverworldTerrain.Badlands, new AreaClimate(Dust, true) },
            { OverworldTerrain.SaltThrone, new AreaClimate(Clear, true) },

            // Região 4 — Grutas do Limiar
            { OverworldTerrain.CaveMouth, new AreaClimate(null, false) },
            { OverworldTerrain.LakeShore, new AreaClimate(Damp, false) },
            { OverworldTerrain.StalactiteHall, new AreaClimate(Damp, true) },
            { OverworldTerrain.MushroomGrove, new AreaClimate(null, false) },
            { OverworldTerrain.CrystalField, new AreaClimate(Clear, false) },
            { OverworldTerrain.Chasm, new AreaClimate(Damp, true) },
            { OverworldTerrain.TuffTerraces, new AreaClimate(null, false) },
            { OverworldTerrain.ColumnForest, new AreaClimate(null, true) },
            { OverworldTerrain.MossMeadow, new AreaClimate(Damp, false) },
            { OverworldTerrain.TunnelMarket, new AreaClimate(Clear, false) },
            { OverworldTerrain.WheelCanyon, new AreaClimate(null, true) },
            { OverworldTerrain.Roost, new AreaClimate(Damp, true) },

            // Região 3 — Serra de Kaggen
            { OverworldTerrain.GoldenSlopes, new AreaClimate(null, false) },
            { OverworldTerrain.FarmTerraces, new AreaClimate(null, false) },
            { OverworldTerrain.AloeSlopes, new AreaClimate(null, false) },
            { OverworldTerrain.BaobabGrass, new AreaClimate(Clear, false) },
            { OverworldTerrain.HighPass, new AreaClimate(Snow, true) },
            { OverworldTerrain.Torrents, new AreaClimate(Mist, false) },
            { OverworldTerrain.Kraals, new AreaClimate(null, false) },
            { OverworldTerrain.NarrowPass, new AreaClimate(null, true) },
            { OverworldTerrain.SandstoneScarp, new AreaClimate(null, false) },
            { OverworldTerrain.PaintedShelters, new AreaClimate(null, false) },
            { OverworldTerrain.DryStone, new AreaClimate(null, false) },
            { OverworldTerrain.BronzeHalls, new AreaClimate(null, true) },
            { OverworldTerrain.Snowfield, new AreaClimate(Snow, true) },
            { OverworldTerrain.Summit, new AreaClimate(Snow, true) },
            { OverworldTerrain.StelaPlateau, new AreaClimate(Snow, false) },
        };

        /// <summary>True when <paramref name="terrain"/> has its own line (tests: every terrain must).</summary>
        public static bool Covers(OverworldTerrain terrain) => Table.ContainsKey(terrain);

        /// <summary>The climate of <paramref name="terrain"/> (an unknown terrain: the region's weather, calm).</summary>
        public static AreaClimate ClimateOf(OverworldTerrain terrain) => Table.TryGetValue(terrain, out AreaClimate climate) ? climate : default;

        /// <summary>The climate of <paramref name="zone"/>; inside a town the region's own weather, calm.</summary>
        public static AreaClimate ClimateOf(OverworldZone zone, OverworldSettlement settlement)
        {
            if (zone == null)
            {
                return default;
            }

            AreaClimate climate = ClimateOf(zone.Terrain);
            return settlement != null ? new AreaClimate(climate.Weather == Dust || climate.Weather == Snow ? null : climate.Weather, false) : climate;
        }

        /// <summary>
        /// The mood of the hero's area: a town's safe zone, else danger when the area's monsters are far above
        /// <paramref name="heroLevel"/> (the same rule as the "Cuidado" warning), else wild or calm by the ground.
        /// </summary>
        public static AreaMood MoodOf(OverworldZone zone, OverworldSettlement settlement, int heroLevel)
        {
            if (settlement != null)
            {
                return AreaMood.Town;
            }

            if (zone == null)
            {
                return AreaMood.Field;
            }

            if (IsDangerous(zone, heroLevel))
            {
                return AreaMood.Danger;
            }

            return ClimateOf(zone.Terrain).Wild ? AreaMood.Wild : AreaMood.Field;
        }

        /// <summary>The area's monsters are far above <paramref name="heroLevel"/> (they crit more: the "Cuidado" warning).</summary>
        public static bool IsDangerous(OverworldZone zone, int heroLevel) =>
            zone != null && MapAccessRules.GetMonsterLevelDeltaPenalty(heroLevel, zone.MaxLevel).MonsterCritChanceBonus > 0f;
    }
}
