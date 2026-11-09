using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 16 — Reinos do Céu (the Barcas, the Wheel and the Column) as ONE continuous map of 2400 × 3600 m (Docs/Mapas100.md §0.10 and §5, maps 91–100), no scene
    /// change between the Cais da Coluna and the Porch of the Time After. The quay (91, the capital, level 420) stands at the plaza (0, 0), 360 m from the south edge, where two passes come up from the
    /// Limiar Profundo (flanking the Gate of the Land of Darkness, level ~432–440): in the stitched world the region grows a sixth row, north of the Limiar Profundo (x 1400…3800, z 8600…12200), the two
    /// cells west of it walled holes (the end of the road: nothing stands over the Rio and the Margens). The bands rise with the distance from the quay, a 5 × 7 grid of areas:
    /// <list type="bullet">
    /// <item>the south row (nv 420–430, map 91): the <b>Cais</b> and the approaches to the piers of the Moon and the Sun, the <b>Beira Oeste</b> and the <b>Fresta das Luzinhas</b> (the way in);</item>
    /// <item>the river row (nv 428–440, map 92): the <b>Rio de Luz</b> (a real river of light, three bridges) with the Boia de Luz, the islets of cloud and the banks of the current;</item>
    /// <item>the moon row (nv 436–450, map 93): the deck of the <b>Barca da Lua</b> in three levels, the mast, the hold, prow and stern, with the Cabine da Proa;</item>
    /// <item>the wheel row (nv 446–460, map 94, a Climbing Rope: the jars are platforms): the <b>Roda Celeste</b>, its rim, its hub with the Eixo da Roda and the drifts of gears, twelve jars;</item>
    /// <item>the spheres row (nv 456–470, map 95, Canvas Wings: from sphere to sphere): the seven <b>Esferas dos Arcontes</b> on their orbit ridges, the Ilhota Parada;</item>
    /// <item>the garden row: the <b>Jardim de Luz</b> (nv 466–478, map 96) with the Pérgola do Jardineiro, west and centre; the deck of the <b>Barca do Sol</b> (nv 474–486, map 97) with the Cabine da Popa and the sail, east;</item>
    /// <item>the north row: the <b>Pés da Coluna da Glória</b> in rings of steps (nv 482–492, map 98) with the Degrau Um and the pillar of light; the <b>Mirante do que Segura o Céu</b> with the five cracks and the Mão Aberta
    /// (nv 488–498, map 99, a Storm Cloak); the <b>Varanda do Tempo de Depois</b>, the empty rocking chair (nv 495–500, map 100).</item>
    /// </list>
    /// Monsters: existing beasts that fit the sheets (see <see cref="Ceu"/>) and the night species, a night table in every clearing; no boss (O Remador Perdido, O Clandestino Grande, O Arconte da Roda, Saklas and Namrael, Āz,
    /// A Sombra da Vela, O Último Arconte do Degrau and A Rachadura Grande are not placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string CeuRegionId = "overworld_reinos_do_ceu";

        public const string CaisDaColunaMapId = "map_091_cais_da_coluna";
        public const string RioDeLuzMapId = "map_092_rio_de_luz";
        public const string BarcaDaLuaMapId = "map_093_barca_da_lua";
        public const string RodaCelesteMapId = "map_094_roda_celeste";
        public const string EsferasArcontesMapId = "map_095_esferas_arcontes";
        public const string JardimDeLuzMapId = "map_096_jardim_de_luz";
        public const string BarcaDoSolMapId = "map_097_barca_do_sol";
        public const string PesDaColunaMapId = "map_098_pes_da_coluna";
        public const string MiranteCeuMapId = "map_099_mirante_do_ceu";
        public const string VarandaMapId = "map_100_varanda";

        public static readonly Vector2 CaisDaColunaOffset = Vector2.zero;
        public static readonly Vector2 BoiaDeLuzOffset = new Vector2(0f, 620f);
        public static readonly Vector2 CabineDaProaOffset = new Vector2(0f, 1000f);
        public static readonly Vector2 EixoDaRodaOffset = new Vector2(0f, 1490f);
        public static readonly Vector2 IlhotaParadaOffset = new Vector2(0f, 1990f);
        public static readonly Vector2 PergolaOffset = new Vector2(0f, 2490f);
        public static readonly Vector2 CabineDaPopaOffset = new Vector2(480f, 2490f);
        public static readonly Vector2 DegrauUmOffset = new Vector2(0f, 2900f);
        public static readonly Vector2 MaoAbertaOffset = new Vector2(480f, 2900f);
        public static readonly Vector2 VarandaOffset = new Vector2(960f, 2900f);

        /// <summary>Half the side (m) of the flat ground reserved for the quay's recipe (±30 m at y = 0).</summary>
        public const float CaisColunaGround = 30f;

        /// <summary>The keel lines of the two decks (region z).</summary>
        public const float MoonDeckKeelZ = 990f;
        public const float SunDeckKeelZ = 2490f;

        /// <summary>The centre of the Celestial Wheel (the Eixo da Roda stands on it, region coordinates).</summary>
        public static readonly Vector2 WheelCenter = EixoDaRodaOffset;

        public const int CelestialJarCount = 12;
        public const float CelestialJarRadius = 150f;

        /// <summary>Where jar <paramref name="i"/> of the twelve stands (off the roads, which run along the axes; region coordinates).</summary>
        public static Vector2 CelestialJarPosition(int i)
        {
            float a = (15f + 30f * i) * Mathf.Deg2Rad;
            return WheelCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * CelestialJarRadius;
        }

        public const int ArchonSphereCount = 7;
        public const float ArchonSphereRadius = 190f;

        /// <summary>Where sphere <paramref name="i"/> of the seven stands on its orbit round the Ilhota Parada (region coordinates).</summary>
        public static Vector2 ArchonSpherePosition(int i)
        {
            float a = (8f + 360f / ArchonSphereCount * i) * Mathf.Deg2Rad;
            return IlhotaParadaOffset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ArchonSphereRadius;
        }

        public const int SkyCrackCount = 5;
        public const float SkyCrackRadius = 110f;

        /// <summary>Where crack <paramref name="i"/> of the five round the Mão Aberta stands (region coordinates).</summary>
        public static Vector2 SkyCrackPosition(int i)
        {
            float a = (18f + 72f * i) * Mathf.Deg2Rad;
            return MaoAbertaOffset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * SkyCrackRadius;
        }

        /// <summary>The great mast of the Barca da Lua (west of the deck) and the sail of the Barca do Sol (east of its deck).</summary>
        public static readonly Vector2 MoonMastCenter = new Vector2(-620f, 1075f);
        public static readonly Vector2 SunMastCenter = new Vector2(960f, 2560f);

        /// <summary>The pillar of light of the Coluna da Glória, north of the Degrau Um (region coordinates).</summary>
        public static readonly Vector2 GloryColumnCenter = new Vector2(0f, 3100f);

        /// <summary>Vô Jorge's rocking chair on the Porch, north of the Varanda post (region coordinates).</summary>
        public static readonly Vector2 RockingChairPosition = new Vector2(960f, 2950f);

        /// <summary>The Rio de Luz, west to east across the row of map 92 (the road bridges it three times).</summary>
        public static readonly Vector2[] RioDeLuz =
        {
            new Vector2(-1180f, 480f), new Vector2(-720f, 510f), new Vector2(-240f, 475f), new Vector2(240f, 510f), new Vector2(720f, 475f), new Vector2(1180f, 505f),
        };

        /// <summary>The existing beasts that stand in for the sheets' monsters (Mapas100 91–100).</summary>
        private static class Ceu
        {
            public const string LesserArchon = "enemy_desert_crowned_orc";
            public const string ConfusedSpark = "enemy_desert_firefly_swarm";
            public const string CloudBird = "enemy_forest_dimmed_ibis";
            public const string StowawayShadow = "enemy_forest_garden_shadow";
            public const string LightFish = "enemy_cave_lantern_fish";
            public const string SwimmingShadow = "enemy_forest_late_shadow";
            public const string WrongCurrent = "enemy_mountain_ill_wind_mist";
            public const string RunawayLantern = "enemy_desert_broken_lantern";
            public const string DarkStowaway = "enemy_forest_shadow_biter";
            public const string StubbornSpark = "enemy_forest_glow_mantle";
            public const string HoldShadow = "enemy_mountain_shadow_seal";
            public const string LivingRope = "enemy_forest_reed_snare";
            public const string ToyArchon = "enemy_forest_nian_cub";
            public const string CrackedJar = "enemy_forest_broken_tablet";
            public const string DarkDrop = "enemy_cave_ooze";
            public const string ChainedArchon = "enemy_mountain_stone_golem";
            public const string SphereSmoke = "enemy_forest_smoke_goat";
            public const string OrbitWind = "enemy_mountain_storm_eagle";
            public const string BalloonGiant = "enemy_mountain_sleepwalker_giant";
            public const string FlowerShadow = "enemy_forest_garden_shadow";
            public const string AzThief = "enemy_forest_thief_monkey";
            public const string SmokeThorn = "enemy_forest_angry_cedar";
            public const string EmberStowaway = "enemy_mountain_bronze_wolf";
            public const string NoonShadow = "enemy_forest_late_shadow";
            public const string DimmedGull = "enemy_mountain_ember_gull";
            public const string LastShadow = "enemy_forest_creeping_dark";
            public const string HighWind = "enemy_mountain_smoke_harpy";
            public const string SkyCrackBeast = "enemy_mountain_ill_wind_mist";
            public const string FledArchon = "enemy_desert_crowned_orc";
            public const string SkyStone = "enemy_mountain_rolling_stone";
        }

        private static OverworldLayout CeuRegion()
        {
            OverworldLayout w = NewRegion(CeuRegionId, MapRegion.ReinosDoCeu, Rect.MinMaxRect(-1200f, -360f, 1200f, 3240f));
            w.World.Biome = BiomeType.Forest;
            const string cais = CaisDaColunaMapId;
            const string rio = RioDeLuzMapId;
            const string lua = BarcaDaLuaMapId;
            const string roda = RodaCelesteMapId;
            const string esferas = EsferasArcontesMapId;
            const string jardim = JardimDeLuzMapId;
            const string sol = BarcaDoSolMapId;
            const string coluna = PesDaColunaMapId;
            const string mirante = MiranteCeuMapId;
            const string varanda = VarandaMapId;
            const string plain = MapRegionTypes.Plain;
            const string ruins = MapRegionTypes.Ruins;
            const string coast = MapRegionTypes.Coast;
            const string forest = MapRegionTypes.Forest;
            const string mountain = MapRegionTypes.Mountain;
            const string rope = MapRequirementTags.ElevatedPlatforms;
            const string wings = MapRequirementTags.WingsOrDoubleJump;
            const string storm = MapRequirementTags.StormProtection;

            // ---------------- Zones (a 5 × 7 grid; columns x −1200/−720/−240/240/720/1200, rows z −360/240/740/1240/1740/2240/2740/3240) ----------------
            // South row: the Cais (91) and its west rim, the approaches to the piers, the gap in the cloud (the way in).
            OverworldZone zBeira = Zone(w, "beira_oeste", cais, null, -1200f, -360f, -720f, 240f, OverworldTerrain.QuayRimWest, 428, 430, 424,
                "West Rim of the Quay", "Beira Oeste do Cais", plain, spacing: 95f);
            OverworldZone zPierLua = Zone(w, "pier_da_lua", cais, null, -720f, -360f, -240f, 240f, OverworldTerrain.MoonPierApproach, 422, 428, 418,
                "Moon Pier Approach", "Píer da Lua", plain, spacing: 90f);
            OverworldZone zCais = Zone(w, "cais_da_coluna", cais, null, -240f, -360f, 240f, 240f, OverworldTerrain.ColumnQuayGround, 420, 426, 416,
                "Column Quay", "Cais da Coluna", plain, spacing: 90f);
            OverworldZone zPierSol = Zone(w, "pier_do_sol", cais, null, 240f, -360f, 720f, 240f, OverworldTerrain.SunPierApproach, 422, 428, 418,
                "Sun Pier Approach", "Píer do Sol", plain, spacing: 90f);
            OverworldZone zFresta = Zone(w, "fresta_das_luzinhas", cais, null, 720f, -360f, 1200f, 240f, OverworldTerrain.CityLightsGap, 428, 430, 424,
                "Gap over the City Lights", "Fresta das Luzinhas", plain, spacing: 95f);
            // River row (92): the river of light from west to east.
            OverworldZone zFozOeste = Zone(w, "foz_oeste", rio, null, -1200f, 240f, -720f, 740f, OverworldTerrain.RiverMouthWest, 432, 438, 428,
                "West Reach of the River", "Foz Oeste", coast, spacing: 95f);
            OverworldZone zIlhas = Zone(w, "ilhotas_de_nuvem", rio, null, -720f, 240f, -240f, 740f, OverworldTerrain.CloudIslets, 428, 434, 424,
                "Cloud Islets", "Ilhas de Nuvem", coast, spacing: 90f);
            OverworldZone zRio = Zone(w, "rio_de_luz", rio, null, -240f, 240f, 240f, 740f, OverworldTerrain.LightRiver, 428, 434, 424,
                "River of Light", "Rio de Luz", coast, spacing: 90f);
            OverworldZone zMargens = Zone(w, "margens_da_corrente", rio, null, 240f, 240f, 720f, 740f, OverworldTerrain.CurrentBanks, 428, 434, 424,
                "Banks of the Current", "Margens da Corrente", coast, spacing: 90f);
            OverworldZone zFozLeste = Zone(w, "foz_leste", rio, null, 720f, 240f, 1200f, 740f, OverworldTerrain.RiverMouthEast, 432, 440, 428,
                "East Reach of the River", "Foz Leste", coast, spacing: 95f);
            // Moon row (93): the deck of the Barca da Lua, stern to prow.
            OverworldZone zProa = Zone(w, "proa_da_lua", lua, null, -1200f, 740f, -720f, 1240f, OverworldTerrain.MoonProw, 442, 448, 438,
                "Prow of the Moon Ship", "Proa da Barca da Lua", ruins, spacing: 95f);
            OverworldZone zMastro = Zone(w, "mastro_da_lua", lua, null, -720f, 740f, -240f, 1240f, OverworldTerrain.MoonMastDeck, 438, 444, 434,
                "Mast Deck of the Moon Ship", "Mastro da Barca da Lua", ruins, spacing: 90f);
            OverworldZone zConves = Zone(w, "conves_da_lua", lua, null, -240f, 740f, 240f, 1240f, OverworldTerrain.MoonDeck, 436, 442, 432,
                "Moon Ship Deck", "Convés da Barca da Lua", ruins, spacing: 90f);
            OverworldZone zPorao = Zone(w, "porao_da_lua", lua, null, 240f, 740f, 720f, 1240f, OverworldTerrain.MoonHold, 438, 444, 434,
                "Hold of the Moon Ship", "Porão da Barca da Lua", ruins, spacing: 90f);
            OverworldZone zPopa = Zone(w, "popa_da_lua", lua, null, 720f, 740f, 1200f, 1240f, OverworldTerrain.MoonStern, 442, 450, 438,
                "Stern of the Moon Ship", "Popa da Barca da Lua", ruins, spacing: 95f);
            // Wheel row (94): the rim of jars round the hub, the drifts of gears at the ends.
            OverworldZone zGearsW = Zone(w, "engrenagens_oeste", roda, null, -1200f, 1240f, -720f, 1740f, OverworldTerrain.GearDrift, 452, 458, 448,
                "West Gear Drifts", "Engrenagens do Oeste", ruins, rope, 95f);
            OverworldZone zJarrasW = Zone(w, "jarras_oeste", roda, null, -720f, 1240f, -240f, 1740f, OverworldTerrain.JarRimWest, 448, 454, 444,
                "West Rim of Jars", "Jarras do Oeste", ruins, rope, 90f);
            OverworldZone zEixo = Zone(w, "roda_celeste", roda, null, -240f, 1240f, 240f, 1740f, OverworldTerrain.WheelHub, 446, 452, 442,
                "Celestial Twelve-Jar Wheel", "Roda Celeste das Doze Jarras", ruins, rope, 90f);
            OverworldZone zJarrasE = Zone(w, "jarras_leste", roda, null, 240f, 1240f, 720f, 1740f, OverworldTerrain.JarRimEast, 448, 454, 444,
                "East Rim of Jars", "Jarras do Leste", ruins, rope, 90f);
            OverworldZone zGearsE = Zone(w, "engrenagens_leste", roda, null, 720f, 1240f, 1200f, 1740f, OverworldTerrain.GearDriftEast, 452, 460, 448,
                "East Gear Drifts", "Engrenagens do Leste", ruins, rope, 95f);
            // Spheres row (95): the seven spheres on their orbit ridges, the empty orbit at the ends.
            OverworldZone zVazioW = Zone(w, "orbita_vazia_oeste", esferas, null, -1200f, 1740f, -720f, 2240f, OverworldTerrain.OrbitVoidWest, 462, 468, 458,
                "West Empty Orbit", "Órbita Vazia do Oeste", ruins, wings, 95f);
            OverworldZone zRidgeW = Zone(w, "orbita_oeste", esferas, null, -720f, 1740f, -240f, 2240f, OverworldTerrain.OrbitRidgeWest, 458, 464, 454,
                "West Orbit Ridges", "Órbita do Oeste", ruins, wings, 90f);
            OverworldZone zIlhota = Zone(w, "esferas_dos_arcontes", esferas, null, -240f, 1740f, 240f, 2240f, OverworldTerrain.StillIslet, 456, 462, 452,
                "Spheres of the Archons", "Esferas dos Arcontes", ruins, wings, 90f);
            OverworldZone zRidgeE = Zone(w, "orbita_leste", esferas, null, 240f, 1740f, 720f, 2240f, OverworldTerrain.OrbitRidgeEast, 458, 464, 454,
                "East Orbit Ridges", "Órbita do Leste", ruins, wings, 90f);
            OverworldZone zVazioE = Zone(w, "orbita_vazia_leste", esferas, null, 720f, 1740f, 1200f, 2240f, OverworldTerrain.OrbitVoidEast, 462, 470, 458,
                "East Empty Orbit", "Órbita Vazia do Leste", ruins, wings, 95f);
            // Garden row: the Garden of Light (96) west and centre, the deck of the Barca do Sol (97) east.
            OverworldZone zFlores = Zone(w, "jardim_das_flores", jardim, null, -1200f, 2240f, -720f, 2740f, OverworldTerrain.FlowerGarden, 470, 476, 466,
                "Flower Garden", "Jardim das Flores", forest, spacing: 100f);
            OverworldZone zPomar = Zone(w, "pomar_de_luz", jardim, null, -720f, 2240f, -240f, 2740f, OverworldTerrain.LightOrchard, 468, 474, 464,
                "Orchard of Light", "Pomar de Luz", forest, spacing: 100f);
            OverworldZone zPergola = Zone(w, "pergola_do_jardineiro", jardim, null, -240f, 2240f, 240f, 2740f, OverworldTerrain.GardenPavilion, 466, 472, 462,
                "Gardener's Pergola", "Pérgola do Jardineiro", forest, spacing: 100f);
            OverworldZone zConvesSol = Zone(w, "conves_do_sol", sol, null, 240f, 2240f, 720f, 2740f, OverworldTerrain.SunDeck, 474, 482, 470,
                "Sun Ship Deck", "Convés da Barca do Sol", ruins, spacing: 95f);
            OverworldZone zVela = Zone(w, "vela_do_sol", sol, null, 720f, 2240f, 1200f, 2740f, OverworldTerrain.SunSailDeck, 476, 486, 472,
                "Under the Sun's Sail", "Sob a Vela do Sol", ruins, spacing: 95f);
            // North row: the Foot of the Column (98), the Lookout and its cracks (99), the Porch (100).
            OverworldZone zRachaduras = Zone(w, "rachaduras_do_oeste", mirante, null, -1200f, 2740f, -720f, 3240f, OverworldTerrain.SkyCracksWest, 490, 496, 486,
                "West Cracks in the Sky", "Rachaduras do Oeste", mountain, storm, 100f);
            OverworldZone zDegraus = Zone(w, "degraus_da_coluna", coluna, null, -720f, 2740f, -240f, 3240f, OverworldTerrain.ColumnStepsWest, 484, 490, 480,
                "Steps of the Column", "Degraus da Coluna", mountain, spacing: 100f);
            OverworldZone zPes = Zone(w, "pes_da_coluna", coluna, null, -240f, 2740f, 240f, 3240f, OverworldTerrain.ColumnFoot, 482, 492, 478,
                "Foot of the Column of Glory", "Pés da Coluna da Glória", mountain, spacing: 100f);
            OverworldZone zMirante = Zone(w, "mirante_do_ceu", mirante, null, 240f, 2740f, 720f, 3240f, OverworldTerrain.SkyHolderLookout, 488, 498, 484,
                "Lookout of the Sky-Holder", "Mirante do que Segura o Céu", mountain, storm, 100f);
            OverworldZone zVaranda = Zone(w, "varanda_do_tempo_de_depois", varanda, null, 720f, 2740f, 1200f, 3240f, OverworldTerrain.TimeAfterPorch, 495, 500, 491,
                "Porch of the Time After", "Varanda do Tempo de Depois", plain, spacing: 100f);

            AddCeuFauna(zBeira, zPierLua, zCais, zPierSol, zFresta, zFozOeste, zIlhas, zRio, zMargens, zFozLeste, zProa, zMastro, zConves, zPorao, zPopa, zGearsW, zJarrasW, zEixo, zJarrasE, zGearsE,
                zVazioW, zRidgeW, zIlhota, zRidgeE, zVazioE, zFlores, zPomar, zPergola, zConvesSol, zVela, zRachaduras, zDegraus, zPes, zMirante, zVaranda);

            // ---------------- Settlements ----------------
            // The quay: the region's return city and the barca's stop for 91–100 (flat ground at y = 0, ±30 m). Then the posts of 92–100.
            w.Vila = Place(w, cais, CaisDaColunaRecipe(), CaisDaColunaOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), CaisColunaGround, true);
            Place(w, rio, Post("boia_de_luz", TownSwatch.QuayCloud, TownSwatch.QuayGold, TownRoofKind.Flat, TownShopDressing.BoatTrader, false), BoiaDeLuzOffset, PostSafe, 18f, false);
            Place(w, lua, Post("cabine_da_proa", TownSwatch.MoonSilver, TownSwatch.QuayLilac, TownRoofKind.Flat, TownShopDressing.BoatTrader, false), CabineDaProaOffset, PostSafe, 18f, false);
            Place(w, roda, Post("eixo_da_roda", TownSwatch.Bronze, TownSwatch.QuayGold, TownRoofKind.Flat, TownShopDressing.Storyteller, false), EixoDaRodaOffset, PostSafe, 18f, false);
            Place(w, esferas, Post("ilhota_parada", TownSwatch.QuayGlassPane, TownSwatch.QuayGold, TownRoofKind.Flat, TownShopDressing.Storyteller, false), IlhotaParadaOffset, PostSafe, 18f, false);
            Place(w, jardim, Post("pergola_do_jardineiro", TownSwatch.QuayCloud, TownSwatch.GardenGreen, TownRoofKind.Gable, TownShopDressing.Herder, false), PergolaOffset, PostSafe, 18f, false);
            Place(w, sol, Post("cabine_da_popa", TownSwatch.QuayGold, TownSwatch.ElementFire, TownRoofKind.Flat, TownShopDressing.BoatTrader, false), CabineDaPopaOffset, PostSafe, 18f, false);
            Place(w, coluna, Post("degrau_um", TownSwatch.QuayCloud, TownSwatch.ColumnLight, TownRoofKind.Flat, TownShopDressing.Storyteller, false), DegrauUmOffset, PostSafe, 18f, false);
            Place(w, mirante, Post("mao_aberta", TownSwatch.QuayCloud, TownSwatch.QuayGold, TownRoofKind.Flat, TownShopDressing.HuntingRack, false), MaoAbertaOffset, PostSafe, 18f, false);
            Place(w, varanda, Post("a_varanda", TownSwatch.AdobeOchre, TownSwatch.QuayLilac, TownRoofKind.Gable, TownShopDressing.Storyteller, false), VarandaOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Rio de Luz (10 m, carved), three bridges where the roads north cross it; the hero walks only on the bridges in this build (andar no rio stays for later).
            Vector2 bridgeWest = On(RioDeLuz[1], RioDeLuz[2], 0.5f);
            Vector2 bridgeN = On(RioDeLuz[2], RioDeLuz[3], 0.5f);
            Vector2 bridgeEast = On(RioDeLuz[3], RioDeLuz[4], 0.5f);
            MapRiver rioLuz = River(w, 10f, RioDeLuz);
            Crossing(rioLuz, bridgeWest, 4.5f, true);
            Crossing(rioLuz, bridgeN, 4.5f, true);
            Crossing(rioLuz, bridgeEast, 4.5f, true);

            // ---------------- Gates out of the region ----------------
            // South: two passes down to the Limiar Profundo (the Gate of the Land of Darkness; tagged with maps 90 and 89: one target per gate). Off the line of the gate, on either side of it, so the led trails go round it.
            Gate(w, "exit_portao_oeste", MapExitKind.Map, PortaoTrevaMapId, Rect.MinMaxRect(772f, -360f, 788f, -357f), new Vector2(780f, -351f), Vector2.up);
            Gate(w, "exit_portao_leste", MapExitKind.Map, PonteNoveloMapId, Rect.MinMaxRect(1132f, -360f, 1148f, -357f), new Vector2(1140f, -351f), Vector2.up);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = CaisColunaGround;
            Vector2 eastFork = new Vector2(300f, 10f);
            Vector2 westFork = new Vector2(-300f, 10f);
            Vector2 gateFork = new Vector2(760f, -120f);
            // East and west roads off the quay; the south road and the east road meet at the fork above the two passes.
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(150f, 8f), eastFork, new Vector2(520f, -30f), gateFork);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-150f, 8f), westFork, new Vector2(-620f, 30f), new Vector2(-960f, 40f));
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(60f, -140f), new Vector2(420f, -200f), gateFork);
            PathVia(w, 4f, gateFork, new Vector2(780f, -351f), new Vector2(780f, -358f)); // On to the edge: the pass is on the trail.
            PathVia(w, 4f, gateFork, new Vector2(1000f, -180f), new Vector2(1140f, -351f), new Vector2(1140f, -358f));
            PathVia(w, 3.5f, gateFork, new Vector2(960f, -40f), new Vector2(960f, 120f));

            // North over the river to the Boia de Luz (the quay has no north street: the road leaves from the east road, like the fortress's).
            PathVia(w, 4f, eastFork, new Vector2(220f, 200f), new Vector2(30f, 400f), bridgeN, new Vector2(0f, 560f), BoiaDeLuzOffset + new Vector2(0f, -18f));
            // The two side roads over the bridges, up the deck row.
            PathVia(w, 3.5f, westFork, new Vector2(-460f, 200f), new Vector2(-480f, 420f), bridgeWest, new Vector2(-480f, 620f), new Vector2(-480f, 860f), new Vector2(-480f, 1000f));
            PathVia(w, 3.5f, eastFork, new Vector2(470f, 200f), new Vector2(480f, 420f), bridgeEast, new Vector2(480f, 620f), new Vector2(480f, 860f), new Vector2(480f, 1000f));

            // The trunk north, post to post (the raised rim, the orbit ridges and the deck tiers are ridden: they live in the smooth part of the relief).
            PathVia(w, 4f, BoiaDeLuzOffset + new Vector2(0f, 18f), new Vector2(0f, 820f), CabineDaProaOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, CabineDaProaOffset + new Vector2(0f, 18f), new Vector2(0f, 1240f), EixoDaRodaOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, EixoDaRodaOffset + new Vector2(0f, 18f), new Vector2(0f, 1740f), IlhotaParadaOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, IlhotaParadaOffset + new Vector2(0f, 18f), new Vector2(0f, 2240f), PergolaOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, PergolaOffset + new Vector2(0f, 18f), new Vector2(0f, 2740f), DegrauUmOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, DegrauUmOffset + new Vector2(0f, 18f), new Vector2(0f, 3040f));

            // East–west lanes of each row, the trunk's post in the middle.
            Lane(w, CabineDaProaOffset);
            Lane(w, EixoDaRodaOffset);
            Lane(w, IlhotaParadaOffset);
            PathVia(w, 3.5f, PergolaOffset + new Vector2(-18f, 0f), new Vector2(-480f, 2490f), new Vector2(-960f, 2490f));
            PathVia(w, 3.5f, PergolaOffset + new Vector2(18f, 0f), CabineDaPopaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, CabineDaPopaOffset + new Vector2(18f, 0f), new Vector2(720f, 2490f), new Vector2(960f, 2490f));
            PathVia(w, 3.5f, DegrauUmOffset + new Vector2(-18f, 0f), new Vector2(-480f, 2900f), new Vector2(-960f, 2900f));
            PathVia(w, 3.5f, DegrauUmOffset + new Vector2(18f, 0f), MaoAbertaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, MaoAbertaOffset + new Vector2(18f, 0f), VarandaOffset + new Vector2(-18f, 0f));

            // Side connectors between the rows along x = ±480.
            foreach (float sx in new[] { -480f, 480f })
            {
                PathVia(w, 3.5f, new Vector2(sx, 1000f), new Vector2(sx, 1240f), new Vector2(sx, 1490f), new Vector2(sx, 1740f), new Vector2(sx, 1990f));
            }

            PathVia(w, 3.5f, new Vector2(-480f, 1990f), new Vector2(-480f, 2240f), new Vector2(-480f, 2490f), new Vector2(-480f, 2700f), new Vector2(-480f, 2900f));
            PathVia(w, 3.5f, new Vector2(480f, 1990f), new Vector2(480f, 2240f), CabineDaPopaOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, CabineDaPopaOffset + new Vector2(0f, 18f), new Vector2(480f, 2700f), MaoAbertaOffset + new Vector2(0f, -18f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-620f, 30f), 4f, 0f);
            Sign(w, eastFork, new Vector2(220f, 200f), 4f, 0f);
            Sign(w, gateFork, new Vector2(780f, -351f), 4f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -470f, 150f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 470f, 130f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -470f, 660f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 470f, 680f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -300f, 1120f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 600f, 1380f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -600f, 2100f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 300f, 2620f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -700f, 2820f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 1040f, 3000f, 0f, 1f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            for (int i = 0; i < CelestialJarCount; i++)
            {
                KeepClearingsOff(w, CelestialJarPosition(i), 4f);
            }

            for (int i = 0; i < ArchonSphereCount; i++)
            {
                KeepClearingsOff(w, ArchonSpherePosition(i), 8f);
            }

            for (int i = 0; i < SkyCrackCount; i++)
            {
                KeepClearingsOff(w, SkyCrackPosition(i), 3f);
            }

            KeepClearingsOff(w, MoonMastCenter, 3.5f);
            KeepClearingsOff(w, SunMastCenter, 4f);
            KeepClearingsOff(w, GloryColumnCenter, 9f);
            KeepClearingsOff(w, RockingChairPosition, 3f);
            TrimSteepClearings(w);
            return w;
        }

        /// <summary>An east–west lane through a post of the trunk, to both ends of its row.</summary>
        private static void Lane(OverworldLayout w, Vector2 post)
        {
            PathVia(w, 3.5f, post + new Vector2(-18f, 0f), new Vector2(-480f, post.y), new Vector2(-960f, post.y));
            PathVia(w, 3.5f, post + new Vector2(18f, 0f), new Vector2(480f, post.y), new Vector2(960f, post.y));
        }

        // ------------------------------------------------------------------
        // The city (its own recipe, Cities/Town: the SkyQuay style on its reserved flat ground of ±30 m at y = 0; no backdrop: open sky round it). Its streets leave W, E and S
        // (no north street: the foot of the Coluna closes the plaza to the north), so the road north leaves from the east road.
        // ------------------------------------------------------------------

        private static CityTownRecipe CaisDaColunaRecipe()
        {
            // The hand recipe has no landing (it lives in Cities/); the barca stops at the end of the south street, west side, past the second pavilion and the gap in the cloud.
            CityTownRecipe recipe = NoBackdrop(CityTownRecipes.CaisDaColuna());
            recipe.Lots.Add(new CityLotRecipe { Id = "cais_da_barca", Kind = CityLotKind.ShipDock, Arm = CityArm.South, Side = CityArm.West, Along = 17.5f, Width = 3.6f, Depth = 3f, Porch = 1f });
            return recipe;
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 91–100 archetypes)
        // ------------------------------------------------------------------

        private static void CeuGroup(OverworldZone zone, string id, string archetype, string lead, string mate, NightSpecies night, int maxAlive = 4, float respawn = 25f)
        {
            zone.Fauna.Add(Fauna(id, archetype, T(N(lead, 3), N(mate, 1)), T(V(lead, 3), S(night, 1)), maxAlive, null, respawn));
        }

        private static void AddCeuFauna(OverworldZone beira, OverworldZone pierLua, OverworldZone cais, OverworldZone pierSol, OverworldZone fresta, OverworldZone fozOeste, OverworldZone ilhas, OverworldZone rio,
            OverworldZone margens, OverworldZone fozLeste, OverworldZone proa, OverworldZone mastro, OverworldZone conves, OverworldZone porao, OverworldZone popa, OverworldZone gearsW, OverworldZone jarrasW,
            OverworldZone eixo, OverworldZone jarrasE, OverworldZone gearsE, OverworldZone vazioW, OverworldZone ridgeW, OverworldZone ilhota, OverworldZone ridgeE, OverworldZone vazioE, OverworldZone flores,
            OverworldZone pomar, OverworldZone pergola, OverworldZone convesSol, OverworldZone vela, OverworldZone rachaduras, OverworldZone degraus, OverworldZone pes, OverworldZone mirante, OverworldZone varanda)
        {
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;
            NightSpecies werewolf = NightVariantRules.Lobisomem;

            // The edges of the Cais (91): the Arconte Fujão Menor [elite] (the crowned orc), the Faísca Confusa [enxame] (the firefly swarm), the Ave de Nuvem Apagada [rápido] (the dimmed ibis),
            // the Sombra Clandestina [controle] (the garden shadow).
            CeuGroup(beira, "arcontes_da_beira", "[elite]", Ceu.LesserArchon, Ceu.CloudBird, moon, 3, 32f);
            CeuGroup(beira, "faiscas_da_beira", "[enxame]", Ceu.ConfusedSpark, Ceu.StowawayShadow, sparks, 5);
            CeuGroup(pierLua, "aves_da_lua", "[rápido]", Ceu.CloudBird, Ceu.ConfusedSpark, moon);
            CeuGroup(pierLua, "sombras_clandestinas", "[controle]", Ceu.StowawayShadow, Ceu.CloudBird, owl);
            CeuGroup(cais, "faiscas_do_cais", "[enxame]", Ceu.ConfusedSpark, Ceu.CloudBird, sparks, 5);
            CeuGroup(cais, "aves_do_cais", "[rápido]", Ceu.CloudBird, Ceu.StowawayShadow, moon);
            CeuGroup(pierSol, "faiscas_do_sol", "[enxame]", Ceu.ConfusedSpark, Ceu.LesserArchon, sparks, 5);
            CeuGroup(pierSol, "arcontes_do_sol", "[elite]", Ceu.LesserArchon, Ceu.ConfusedSpark, werewolf, 3, 32f);
            CeuGroup(fresta, "aves_da_fresta", "[rápido]", Ceu.CloudBird, Ceu.LesserArchon, owl);
            CeuGroup(fresta, "sombras_da_fresta", "[controle]", Ceu.StowawayShadow, Ceu.ConfusedSpark, bats);

            // The Rio de Luz (92): the Peixe de Luz Apagado [rápido] (the lantern fish), the Sombra Nadadora [controle] (the late shadow), the Corrente Errada [controle] (the ill-wind mist),
            // the Lanterna Fugitiva [enxame] (the broken lantern).
            CeuGroup(fozOeste, "peixes_apagados", "[rápido]", Ceu.LightFish, Ceu.SwimmingShadow, moon);
            CeuGroup(fozOeste, "sombras_nadadoras", "[controle]", Ceu.SwimmingShadow, Ceu.LightFish, owl);
            CeuGroup(ilhas, "lanternas_fugitivas", "[enxame]", Ceu.RunawayLantern, Ceu.SwimmingShadow, sparks, 5);
            CeuGroup(ilhas, "peixes_das_ilhas", "[rápido]", Ceu.LightFish, Ceu.RunawayLantern, bats);
            CeuGroup(rio, "correntes_erradas", "[controle]", Ceu.WrongCurrent, Ceu.LightFish, moon);
            CeuGroup(rio, "peixes_do_rio", "[rápido]", Ceu.LightFish, Ceu.WrongCurrent, sparks);
            CeuGroup(margens, "lanternas_das_margens", "[enxame]", Ceu.RunawayLantern, Ceu.WrongCurrent, sparks, 5);
            CeuGroup(margens, "sombras_das_margens", "[controle]", Ceu.SwimmingShadow, Ceu.RunawayLantern, owl);
            CeuGroup(fozLeste, "correntes_da_foz", "[controle]", Ceu.WrongCurrent, Ceu.RunawayLantern, moon);
            CeuGroup(fozLeste, "lanternas_da_foz", "[enxame]", Ceu.RunawayLantern, Ceu.LightFish, bats, 5);

            // The Barca da Lua (93): the Clandestino de Treva [rápido] (the shadow biter), the Faísca Teimosa [especial] (the glow mantle), the Sombra do Porão [tanque] (the shadow seal),
            // the Corda Viva [controle] (the reed snare).
            CeuGroup(proa, "clandestinos_da_proa", "[rápido]", Ceu.DarkStowaway, Ceu.LivingRope, bats);
            CeuGroup(proa, "cordas_da_proa", "[controle]", Ceu.LivingRope, Ceu.DarkStowaway, moon);
            CeuGroup(mastro, "faiscas_teimosas", "[especial]", Ceu.StubbornSpark, Ceu.DarkStowaway, sparks);
            CeuGroup(mastro, "clandestinos_do_mastro", "[rápido]", Ceu.DarkStowaway, Ceu.StubbornSpark, owl);
            CeuGroup(conves, "clandestinos_de_treva", "[rápido]", Ceu.DarkStowaway, Ceu.HoldShadow, moon);
            CeuGroup(conves, "faiscas_do_conves", "[especial]", Ceu.StubbornSpark, Ceu.LivingRope, sparks);
            CeuGroup(porao, "sombras_do_porao", "[tanque]", Ceu.HoldShadow, Ceu.LivingRope, moon, 3, 30f);
            CeuGroup(porao, "cordas_vivas", "[controle]", Ceu.LivingRope, Ceu.HoldShadow, bats);
            CeuGroup(popa, "sombras_da_popa", "[tanque]", Ceu.HoldShadow, Ceu.StubbornSpark, moon, 3, 30f);
            CeuGroup(popa, "cordas_da_popa", "[controle]", Ceu.LivingRope, Ceu.DarkStowaway, owl);

            // The Celestial Wheel (94): Os Doze da Roda [elite] (the nian cub), the Jarra Rachada [distância] (the broken tablet), the Gota de Treva [rápido] (the ooze).
            CeuGroup(gearsW, "gotas_do_oeste", "[rápido]", Ceu.DarkDrop, Ceu.ToyArchon, owl);
            CeuGroup(gearsW, "jarras_rachadas_oeste", "[distância]", Ceu.CrackedJar, Ceu.DarkDrop, moon);
            CeuGroup(jarrasW, "doze_oeste", "[elite]", Ceu.ToyArchon, Ceu.CrackedJar, moon, 3, 32f);
            CeuGroup(jarrasW, "gotas_das_jarras", "[rápido]", Ceu.DarkDrop, Ceu.ToyArchon, bats);
            CeuGroup(eixo, "doze_da_roda", "[elite]", Ceu.ToyArchon, Ceu.DarkDrop, werewolf, 3, 32f);
            CeuGroup(eixo, "jarras_rachadas", "[distância]", Ceu.CrackedJar, Ceu.ToyArchon, sparks);
            CeuGroup(jarrasE, "doze_leste", "[elite]", Ceu.ToyArchon, Ceu.CrackedJar, moon, 3, 32f);
            CeuGroup(jarrasE, "jarras_do_leste", "[distância]", Ceu.CrackedJar, Ceu.DarkDrop, owl);
            CeuGroup(gearsE, "gotas_do_leste", "[rápido]", Ceu.DarkDrop, Ceu.CrackedJar, bats);
            CeuGroup(gearsE, "jarras_rachadas_leste", "[distância]", Ceu.CrackedJar, Ceu.ToyArchon, moon);

            // The Spheres (95): the Arconte Acorrentado Menor [tanque] (the stone golem), the Fumaça de Esfera [controle] (the smoke goat), the Vento de Órbita [rápido] (the storm eagle),
            // the Gigante-Balão [elite] (the sleepwalker giant).
            CeuGroup(vazioW, "ventos_de_orbita_oeste", "[rápido]", Ceu.OrbitWind, Ceu.SphereSmoke, owl);
            CeuGroup(vazioW, "gigantes_balao_oeste", "[elite]", Ceu.BalloonGiant, Ceu.OrbitWind, werewolf, 3, 32f);
            CeuGroup(ridgeW, "fumacas_de_esfera", "[controle]", Ceu.SphereSmoke, Ceu.ChainedArchon, moon);
            CeuGroup(ridgeW, "arcontes_acorrentados", "[tanque]", Ceu.ChainedArchon, Ceu.SphereSmoke, moon, 3, 30f);
            CeuGroup(ilhota, "arcontes_da_ilhota", "[tanque]", Ceu.ChainedArchon, Ceu.OrbitWind, moon, 3, 30f);
            CeuGroup(ilhota, "ventos_da_ilhota", "[rápido]", Ceu.OrbitWind, Ceu.ChainedArchon, owl);
            CeuGroup(ridgeE, "fumacas_do_leste", "[controle]", Ceu.SphereSmoke, Ceu.BalloonGiant, bats);
            CeuGroup(ridgeE, "gigantes_balao", "[elite]", Ceu.BalloonGiant, Ceu.SphereSmoke, werewolf, 3, 32f);
            CeuGroup(vazioE, "ventos_de_orbita", "[rápido]", Ceu.OrbitWind, Ceu.BalloonGiant, owl);
            CeuGroup(vazioE, "arcontes_do_leste", "[tanque]", Ceu.ChainedArchon, Ceu.OrbitWind, moon, 3, 30f);

            // The Garden of Light (96): the Sombra que Pisa as Flores [rápido] (the garden shadow), the Ladrões de Āz [elite] (the thief monkey), the Espinho de Fumaça [controle] (the angry cedar).
            CeuGroup(flores, "sombras_das_flores", "[rápido]", Ceu.FlowerShadow, Ceu.SmokeThorn, sparks, 3);
            CeuGroup(flores, "espinhos_das_flores", "[controle]", Ceu.SmokeThorn, Ceu.AzThief, moon, 3);
            CeuGroup(pomar, "ladroes_do_pomar", "[elite]", Ceu.AzThief, Ceu.FlowerShadow, owl, 3, 32f);
            CeuGroup(pomar, "espinhos_do_pomar", "[controle]", Ceu.SmokeThorn, Ceu.FlowerShadow, sparks, 3);
            CeuGroup(pergola, "sombras_da_pergola", "[rápido]", Ceu.FlowerShadow, Ceu.AzThief, moon, 3);
            CeuGroup(pergola, "ladroes_da_pergola", "[elite]", Ceu.AzThief, Ceu.SmokeThorn, owl, 3, 32f);

            // The Barca do Sol (97): the Clandestino de Brasa [rápido] (the bronze wolf), the Sombra do Meio-Dia [controle] (the late shadow), the Gaivota de Luz Apagada [distância] (the ember gull),
            // the Corda de Fumaça [controle] (the reed snare).
            CeuGroup(convesSol, "clandestinos_de_brasa", "[rápido]", Ceu.EmberStowaway, Ceu.DimmedGull, sparks);
            CeuGroup(convesSol, "gaivotas_de_luz", "[distância]", Ceu.DimmedGull, Ceu.EmberStowaway, moon);
            CeuGroup(vela, "sombras_do_meio_dia", "[controle]", Ceu.NoonShadow, Ceu.LivingRope, moon);
            CeuGroup(vela, "cordas_de_fumaca", "[controle]", Ceu.LivingRope, Ceu.DimmedGull, bats);

            // The Foot of the Column (98): the Última Sombra [rápido] (the creeping dark), the Arconte Menor [elite] (the crowned orc), the Vento do Alto [controle] (the smoke harpy).
            CeuGroup(degraus, "ultimas_sombras", "[rápido]", Ceu.LastShadow, Ceu.HighWind, moon);
            CeuGroup(degraus, "ventos_do_alto", "[controle]", Ceu.HighWind, Ceu.LastShadow, owl);
            CeuGroup(pes, "arcontes_do_degrau", "[elite]", Ceu.LesserArchon, Ceu.LastShadow, werewolf, 3, 32f);
            CeuGroup(pes, "sombras_dos_pes", "[rápido]", Ceu.LastShadow, Ceu.HighWind, bats);

            // The Lookout (99): the Rachadura do Céu [controle] (the ill-wind mist), the Arconte Fugido [elite] (the crowned orc), the Vento do Alto [rápido] (the storm eagle), the Pedra do Céu [tanque] (the rolling stone).
            CeuGroup(rachaduras, "rachaduras_do_ceu", "[controle]", Ceu.SkyCrackBeast, Ceu.SkyStone, moon);
            CeuGroup(rachaduras, "pedras_do_ceu", "[tanque]", Ceu.SkyStone, Ceu.SkyCrackBeast, owl, 3, 30f);
            CeuGroup(mirante, "arcontes_fugidos", "[elite]", Ceu.FledArchon, Ceu.OrbitWind, werewolf, 3, 32f);
            CeuGroup(mirante, "ventos_do_mirante", "[rápido]", Ceu.OrbitWind, Ceu.SkyStone, moon);

            // The Porch (100): the sheet's Sombras de Saudade are not fought; outside the orchard, the beasts of 99.
            CeuGroup(varanda, "pedras_do_pomar", "[tanque]", Ceu.SkyStone, Ceu.SkyCrackBeast, moon, 2, 40f);
            CeuGroup(varanda, "ventos_do_pomar", "[rápido]", Ceu.OrbitWind, Ceu.SkyStone, owl, 2, 40f);
        }
    }
}
