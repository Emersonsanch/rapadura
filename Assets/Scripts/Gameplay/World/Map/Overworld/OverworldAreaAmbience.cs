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

        /// <summary>The monsoon's rain (the Costa das Monções' mangroves and drowned streets).</summary>
        private const WeatherType Monsoon = WeatherType.Rain;

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

            // Região 5 — Costa das Monções: clear sky on the beaches, the reef and the savanna; the monsoon rain over the
            // mangroves and the drowned city; sea mist on the cliffs and the kelp; Meroë's pyramids in the dust.
            { OverworldTerrain.WhiteBeach, new AreaClimate(Clear, false) },
            { OverworldTerrain.Reef, new AreaClimate(Clear, false) },
            { OverworldTerrain.Mangrove, new AreaClimate(Monsoon, true) },
            { OverworldTerrain.CoastalDunes, new AreaClimate(null, false) },
            { OverworldTerrain.CoralRuins, new AreaClimate(null, true) },
            { OverworldTerrain.CloveRows, new AreaClimate(null, false) },
            { OverworldTerrain.CoconutGrove, new AreaClimate(null, false) },
            { OverworldTerrain.SeaCliffs, new AreaClimate(Mist, true) },
            { OverworldTerrain.Shoals, new AreaClimate(Clear, false) },
            { OverworldTerrain.Savanna, new AreaClimate(Clear, false) },
            { OverworldTerrain.KushPyramids, new AreaClimate(Dust, true) },
            { OverworldTerrain.Atoll, new AreaClimate(Clear, false) },
            { OverworldTerrain.SunkenCity, new AreaClimate(Monsoon, true) },
            { OverworldTerrain.CoastScrub, new AreaClimate(null, false) },
            { OverworldTerrain.TideLagoons, new AreaClimate(Mist, false) },
            { OverworldTerrain.BaobabWoods, new AreaClimate(null, false) },
            { OverworldTerrain.KelpFlats, new AreaClimate(Mist, true) },

            // Região 6 — Delta de Jhilpur: the monsoon over the gorges, the rain forest and the tidal forest; the lilac mist
            // over the reed swamp, the bog, the dead wood and the black mire; the lake, the paddies and the mud banks let the
            // biome roll its weather.
            { OverworldTerrain.RootLake, new AreaClimate(null, false) },
            { OverworldTerrain.BanyanGrove, new AreaClimate(null, true) },
            { OverworldTerrain.LotusMarsh, new AreaClimate(Mist, false) },
            { OverworldTerrain.RootGorge, new AreaClimate(Monsoon, true) },
            { OverworldTerrain.RainForest, new AreaClimate(Monsoon, true) },
            { OverworldTerrain.PaddyLevees, new AreaClimate(null, false) },
            { OverworldTerrain.MudFlats, new AreaClimate(null, false) },
            { OverworldTerrain.ReedIslands, new AreaClimate(Mist, true) },
            { OverworldTerrain.RaffiaBog, new AreaClimate(Mist, true) },
            { OverworldTerrain.BlackMire, new AreaClimate(Mist, true) },
            { OverworldTerrain.DeadWood, new AreaClimate(Mist, true) },
            { OverworldTerrain.TidalForest, new AreaClimate(Monsoon, true) },

            // Região 7 — Teto do Mundo: snow over the high terraces, the chutes, the glacier, the seracs, the ridges, the pass and
            // the peaks; mist on the meltwater, the lake and the dragon's crags; clear cold sky on the high steppe and in the
            // Wakhan valley; Sangkent's terraces, the apricots, the poplars, the junipers and the pastures let the biome roll it.
            { OverworldTerrain.StairTerraces, new AreaClimate(null, false) },
            { OverworldTerrain.ApricotValley, new AreaClimate(null, false) },
            { OverworldTerrain.PoplarCanals, new AreaClimate(null, false) },
            { OverworldTerrain.HighSteppe, new AreaClimate(Clear, false) },
            { OverworldTerrain.MeltRiver, new AreaClimate(Mist, false) },
            { OverworldTerrain.JuniperSlope, new AreaClimate(null, true) },
            { OverworldTerrain.YakPasture, new AreaClimate(null, false) },
            { OverworldTerrain.TurquoiseLake, new AreaClimate(Mist, false) },
            { OverworldTerrain.StoneChutes, new AreaClimate(Snow, false) },
            { OverworldTerrain.SnowTerraces, new AreaClimate(Snow, false) },
            { OverworldTerrain.Moraine, new AreaClimate(null, true) },
            { OverworldTerrain.Glacier, new AreaClimate(Snow, true) },
            { OverworldTerrain.Seracs, new AreaClimate(Snow, true) },
            { OverworldTerrain.WindRidge, new AreaClimate(Snow, true) },
            { OverworldTerrain.FortRock, new AreaClimate(null, true) },
            { OverworldTerrain.WakhanValley, new AreaClimate(Clear, false) },
            { OverworldTerrain.WakhanPass, new AreaClimate(Snow, true) },
            { OverworldTerrain.DragonCrags, new AreaClimate(Mist, true) },
            { OverworldTerrain.IcePeaks, new AreaClimate(Snow, true) },

            // Região 8 — Planície de Mingcheng: mist on the canal, the tea hills, the bamboo, the karst, the pine crests and under
            // the wall; dust over the loess and the terracotta pits; clear sky on the millet, the white hills and the Torres' gardens;
            // the rings, the spirit way, the willow river and the paddies let the biome roll it.
            { OverworldTerrain.NineRings, new AreaClimate(null, false) },
            { OverworldTerrain.MilletFields, new AreaClimate(Clear, false) },
            { OverworldTerrain.WillowRiver, new AreaClimate(null, false) },
            { OverworldTerrain.LanternCanal, new AreaClimate(Mist, false) },
            { OverworldTerrain.MirrorPaddies, new AreaClimate(null, false) },
            { OverworldTerrain.TeaTerraces, new AreaClimate(Mist, false) },
            { OverworldTerrain.BambooGrove, new AreaClimate(Mist, true) },
            { OverworldTerrain.WhiteHills, new AreaClimate(Clear, false) },
            { OverworldTerrain.SpireGardens, new AreaClimate(Clear, false) },
            { OverworldTerrain.KarstPeaks, new AreaClimate(Mist, true) },
            { OverworldTerrain.PineRidge, new AreaClimate(Mist, true) },
            { OverworldTerrain.GreatWall, new AreaClimate(Mist, true) },
            { OverworldTerrain.TerracottaPits, new AreaClimate(Dust, true) },
            { OverworldTerrain.SpiritWay, new AreaClimate(null, false) },
            { OverworldTerrain.LoessGullies, new AreaClimate(Dust, true) },

            // Região 9 — Estepe dos Nartes: clear sky over the open steppe, the camp, the kurgans, the prairies and the plains of the
            // herds (céu #A7C7E7); dust over the wormwood and its salt; mist in the taiga, the dream vale and the svan valleys; snow on
            // the chain's crags and Ohya's crest; the rivers, the birches and the giants' hills let the biome roll it.
            { OverworldTerrain.FeatherSteppe, new AreaClimate(Clear, false) },
            { OverworldTerrain.SteppeRiver, new AreaClimate(null, false) },
            { OverworldTerrain.HorseCamp, new AreaClimate(Clear, false) },
            { OverworldTerrain.KurganField, new AreaClimate(Clear, true) },
            { OverworldTerrain.WormwoodSteppe, new AreaClimate(Dust, true) },
            { OverworldTerrain.BirchSteppe, new AreaClimate(null, false) },
            { OverworldTerrain.GiantHills, new AreaClimate(null, false) },
            { OverworldTerrain.DreamVale, new AreaClimate(Mist, true) },
            { OverworldTerrain.Taiga, new AreaClimate(Mist, true) },
            { OverworldTerrain.PoppyPrairie, new AreaClimate(Clear, false) },
            { OverworldTerrain.HorsePlains, new AreaClimate(Clear, false) },
            { OverworldTerrain.SvanValley, new AreaClimate(Mist, true) },
            { OverworldTerrain.ChainPeak, new AreaClimate(Snow, true) },
            { OverworldTerrain.SnowCrest, new AreaClimate(Snow, true) },

            // Região 10 — Picos de Farnkesh: clear sky over the satyrs' vines and glades, the high meadows, Farnkesh on its pinnacles
            // (visão ótima, céu #8FB8E0) and the cloud isles round Huma's nest; mist in the sea of clouds, the pinnacle wood and on the
            // wind bridges; dust in the Gorge of the Ays and on its scree; the Pactolo, the foot woods and the hermitages' cliff let
            // the biome roll it.
            { OverworldTerrain.VineVale, new AreaClimate(Clear, false) },
            { OverworldTerrain.GoldenRiver, new AreaClimate(null, false) },
            { OverworldTerrain.SatyrGlades, new AreaClimate(Clear, false) },
            { OverworldTerrain.FootWoods, new AreaClimate(null, false) },
            { OverworldTerrain.AlpineMeadow, new AreaClimate(Clear, false) },
            { OverworldTerrain.PinnacleWood, new AreaClimate(Mist, true) },
            { OverworldTerrain.StoneNests, new AreaClimate(Clear, false) },
            { OverworldTerrain.CloudSea, new AreaClimate(Mist, true) },
            { OverworldTerrain.WindBridges, new AreaClimate(Mist, true) },
            { OverworldTerrain.BellCliffs, new AreaClimate(null, true) },
            { OverworldTerrain.AysGorge, new AreaClimate(Dust, true) },
            { OverworldTerrain.WindScree, new AreaClimate(Dust, true) },
            { OverworldTerrain.CloudIsles, new AreaClimate(Clear, true) },
            { OverworldTerrain.HumaNest, new AreaClimate(Clear, true) },

            // Região 11 — Mar Gelado: the clear polar sky over the harbour, the bay and the lighthouse isle (the aurora at night);
            // sea smoke on the canals and over Omöl's lakes; snow on the shore, the tundra, the pastures, the cracked ice and the
            // blizzard on the pack ice and the aurora sea (map 63); the fjords and the galleries let the biome roll it; Old Kul's
            // lake and its black ice under a still, clear cold.
            { OverworldTerrain.SnowShore, new AreaClimate(Snow, false) },
            { OverworldTerrain.MoonHarbor, new AreaClimate(Clear, false) },
            { OverworldTerrain.FrozenBay, new AreaClimate(Clear, false) },
            { OverworldTerrain.IceCanals, new AreaClimate(Mist, false) },
            { OverworldTerrain.CrackedIce, new AreaClimate(Snow, true) },
            { OverworldTerrain.Tundra, new AreaClimate(Snow, false) },
            { OverworldTerrain.TundraLakes, new AreaClimate(Mist, false) },
            { OverworldTerrain.ReindeerPasture, new AreaClimate(Snow, false) },
            { OverworldTerrain.Fjord, new AreaClimate(null, true) },
            { OverworldTerrain.MammothGalleries, new AreaClimate(null, true) },
            { OverworldTerrain.FrozenEarth, new AreaClimate(Snow, true) },
            { OverworldTerrain.PackIce, new AreaClimate(Snow, true) },
            { OverworldTerrain.AuroraSea, new AreaClimate(Snow, true) },
            { OverworldTerrain.LighthouseIsle, new AreaClimate(Clear, true) },
            { OverworldTerrain.FrozenLake, new AreaClimate(Clear, true) },
            { OverworldTerrain.BlackIce, new AreaClimate(Clear, true) },

            // Região 12 — Arquipélago de Hinokura: a clear sky over the forge isle, the gardens, the red bridges and the broom
            // fields (the lava channels light the night); steam (the fog) over the thaw strait, the hot springs, the fumaroles,
            // the hot channels and the reef; ash falling (the dust) on the volcano's cone, the malpaís, the slag, the ash plains and
            // the caldera; the castle's courts let the biome roll it.
            { OverworldTerrain.ForgeIsle, new AreaClimate(Clear, false) },
            { OverworldTerrain.BlackSandBeach, new AreaClimate(Clear, false) },
            { OverworldTerrain.ThawStrait, new AreaClimate(Mist, false) },
            { OverworldTerrain.LavaChannels, new AreaClimate(Clear, false) },
            { OverworldTerrain.HotSprings, new AreaClimate(Mist, false) },
            { OverworldTerrain.FumaroleCoast, new AreaClimate(Mist, true) },
            { OverworldTerrain.LavaGardens, new AreaClimate(Clear, false) },
            { OverworldTerrain.CloudPines, new AreaClimate(Mist, false) },
            { OverworldTerrain.RedBridges, new AreaClimate(Clear, false) },
            { OverworldTerrain.BroomFields, new AreaClimate(Clear, false) },
            { OverworldTerrain.EcheydeCone, new AreaClimate(Dust, true) },
            { OverworldTerrain.Malpais, new AreaClimate(Dust, true) },
            { OverworldTerrain.HotChannels, new AreaClimate(Mist, true) },
            { OverworldTerrain.ObsidianReef, new AreaClimate(Mist, true) },
            { OverworldTerrain.GlassIslets, new AreaClimate(Mist, true) },
            { OverworldTerrain.CurvedWalls, new AreaClimate(null, true) },
            { OverworldTerrain.NineFurnaces, new AreaClimate(null, true) },
            { OverworldTerrain.SlagFields, new AreaClimate(Dust, true) },
            { OverworldTerrain.AshPlain, new AreaClimate(Dust, true) },
            { OverworldTerrain.Caldera, new AreaClimate(Dust, true) },

            // Região 13 — Rio das Doze Horas: always night; a clear starry sky over the quay, the piers and the fields; mist over the river's
            // reaches, the reed beds and the canals; dust blowing on the hidden sands, the zigzag and Apep's bed; the coils and the lake
            // are wild.
            { OverworldTerrain.NightQuay, new AreaClimate(Clear, false) },
            { OverworldTerrain.PapyrusBank, new AreaClimate(Mist, false) },
            { OverworldTerrain.AshDelta, new AreaClimate(Mist, true) },
            { OverworldTerrain.RowersBeach, new AreaClimate(Clear, false) },
            { OverworldTerrain.HourPiers, new AreaClimate(Clear, false) },
            { OverworldTerrain.BarqueCanal, new AreaClimate(Mist, false) },
            { OverworldTerrain.ReedBeds, new AreaClimate(Mist, false) },
            { OverworldTerrain.ReedFields, new AreaClimate(Clear, false) },
            { OverworldTerrain.LightWheat, new AreaClimate(Clear, false) },
            { OverworldTerrain.SekhetCanals, new AreaClimate(Mist, false) },
            { OverworldTerrain.HiddenSands, new AreaClimate(Dust, true) },
            { OverworldTerrain.ClosedGates, new AreaClimate(Dust, true) },
            { OverworldTerrain.ZigzagSands, new AreaClimate(Dust, true) },
            { OverworldTerrain.MehenCoils, new AreaClimate(null, true) },
            { OverworldTerrain.CoilKnot, new AreaClimate(Mist, true) },
            { OverworldTerrain.OuterCoil, new AreaClimate(null, true) },
            { OverworldTerrain.FireLakeShore, new AreaClimate(Dust, true) },
            { OverworldTerrain.FireLake, new AreaClimate(Clear, true) },
            { OverworldTerrain.ApepSandbank, new AreaClimate(Dust, true) },
            { OverworldTerrain.ApepScales, new AreaClimate(Dust, true) },

            // Região 14 — Margens da Mistura: clear skies over the city and its fields, dust on the Desert of the Clock and the brick
            // gate, mist on the pitch and the cedars; the shoulder of the giant and the field of the tablets are wild.
            { OverworldTerrain.ZigguratTerrace, new AreaClimate(Clear, false) },
            { OverworldTerrain.BrickGate, new AreaClimate(Dust, false) },
            { OverworldTerrain.BrickKilns, new AreaClimate(Dust, false) },
            { OverworldTerrain.TellMounds, new AreaClimate(Clear, false) },
            { OverworldTerrain.PalmCanal, new AreaClimate(Clear, false) },
            { OverworldTerrain.BarleyTerraces, new AreaClimate(Clear, false) },
            { OverworldTerrain.ClockDesert, new AreaClimate(Dust, true) },
            { OverworldTerrain.ShadowSands, new AreaClimate(Dust, true) },
            { OverworldTerrain.HourSands, new AreaClimate(Clear, true) },
            { OverworldTerrain.TarPools, new AreaClimate(Mist, true) },
            { OverworldTerrain.BitumenWells, new AreaClimate(Mist, true) },
            { OverworldTerrain.TarReeds, new AreaClimate(Mist, true) },
            { OverworldTerrain.CedarEdge, new AreaClimate(null, true) },
            { OverworldTerrain.CedarRings, new AreaClimate(Mist, true) },
            { OverworldTerrain.ElderCedars, new AreaClimate(Mist, true) },
            { OverworldTerrain.ShoulderStone, new AreaClimate(null, true) },
            { OverworldTerrain.SkinFolds, new AreaClimate(Dust, true) },
            { OverworldTerrain.DioriteRise, new AreaClimate(Clear, true) },
            { OverworldTerrain.DreamTablets, new AreaClimate(Mist, true) },
            { OverworldTerrain.CutGarden, new AreaClimate(Mist, true) },
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
