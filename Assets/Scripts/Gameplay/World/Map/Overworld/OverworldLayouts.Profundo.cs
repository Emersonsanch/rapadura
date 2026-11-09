using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 15 — Limiar Profundo (the Five Pits of Darkness) as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 83–90),
    /// no scene change between the Fortaleza da Última Lâmpada and the Gate of the Land of Darkness. The fortress (83, the capital, level 340)
    /// stands at the plaza (0, 0), 600 m from the west edge, where two passes come over from the Margens da Mistura (the Field of the Dream
    /// Tablets, level ~344–360): in the stitched world the region fills the last walled cell, north of the Picos de Farnkesh. The bands rise with
    /// the distance from the fortress, each pit in its own part of the 5 × 5 grid:
    /// <list type="bullet">
    /// <item>round the fortress (nv 340–350, map 83): the <b>Fortaleza</b>, the <b>Muralha em Arco</b> (the way in), the <b>Nichos de Basalto</b>, the
    /// <b>Cisternas de Óleo</b>, the <b>Varais de Pavios</b> and the <b>Beira do Abismo</b>;</item>
    /// <item>east of it (nv 348–362, map 84: a Theriac): the <b>Cova da Fumaça</b> with the Lâmpada 1 (post), the <b>Ventiladores dos Artífices</b> and the
    /// <b>Corredores de Cinza</b>;</item>
    /// <item>the south row (nv 360–374, map 85: Eternal Fire Oil): the <b>Cova do Fogo Devorador</b> with the Lâmpada 2 (post), the <b>Chão que
    /// Acende</b> and the <b>Salões de Cinza Quente</b>;</item>
    /// <item>the north row (nv 372–388, map 86: Canvas Wings): the <b>Cova do Vento Destruidor</b> with the Lâmpada 3 (post), a shaft of stepped rings,
    /// the <b>Plataformas Soltas</b> and the <b>Correntes Ascendentes</b>;</item>
    /// <item>the fourth column (nv 386–402, map 87: an Air Bubble): the <b>Cova do Lodo</b> with the Lâmpada 4 (post), the <b>Salões Alagados</b> and the
    /// <b>Comportas dos Artífices</b>, pools of black water;</item>
    /// <item>the corners of that column (nv 400–416, map 88: a Runic Lamp): the <b>Cova da Treva</b> with the Lâmpada 5 (post) and the <b>Túneis Cegos</b>,
    /// a labyrinth of walls;</item>
    /// <item>the east column south to north (nv 412–428, map 89: a Balance Pole): the <b>Ponte do Novelo</b>, one long causeway of ten flights, with the
    /// <b>Cabeceira da Ponte</b>, the Pilar do Meio (post) and the <b>Lances do Novelo</b>, the dark globe beside it;</item>
    /// <item>the north end of that column (nv 424–440, map 90: a lit lamp): the <b>Praça dos Pedestais</b> with the Selo dos Artífices (post) and five
    /// lamp pedestals, and the <b>Portão da Terra da Treva</b>.</item>
    /// </list>
    /// Monsters: existing beasts that fit the sheets (see <see cref="Profundo"/>) and the night species, a night table in every clearing; no boss
    /// (O Rei da Fumaça, A Leoa, A Águia, O Peixe, O Dragão do Fundo, O Guardião and A Grande Treva are not placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string ProfundoRegionId = "overworld_limiar_profundo";

        public const string FortalezaUltimaLampadaMapId = "map_083_fortaleza_ultima_lampada";
        public const string CovaFumacaMapId = "map_084_cova_fumaca";
        public const string CovaFogoMapId = "map_085_cova_fogo_devorador";
        public const string CovaVentoMapId = "map_086_cova_vento_destruidor";
        public const string CovaLodoMapId = "map_087_cova_lodo";
        public const string CovaTrevaMapId = "map_088_cova_treva";
        public const string PonteNoveloMapId = "map_089_ponte_novelo";
        public const string PortaoTrevaMapId = "map_090_portao_terra_treva";

        public static readonly Vector2 FortalezaUltimaLampadaOffset = Vector2.zero;
        public static readonly Vector2 LampadaFumacaOffset = new Vector2(600f, 0f);
        public static readonly Vector2 LampadaFogoOffset = new Vector2(120f, -960f);
        public static readonly Vector2 LampadaVentoOffset = new Vector2(120f, 960f);
        public static readonly Vector2 LampadaLodoOffset = new Vector2(1080f, 0f);
        public static readonly Vector2 LampadaTrevaOffset = new Vector2(1080f, -960f);
        public static readonly Vector2 PilarDoMeioOffset = new Vector2(1560f, -480f);
        public static readonly Vector2 SeloArtificesOffset = new Vector2(1560f, 480f);

        /// <summary>Half the side (m) of the flat ground reserved for the fortress' recipe (±30 m at y = 0).</summary>
        public const float FortalezaProfundoGround = 30f;

        /// <summary>The centre of the shaft of the Cova do Vento (the Lâmpada 3 stands on it, region coordinates).</summary>
        public static readonly Vector2 WindShaftCenter = LampadaVentoOffset;

        /// <summary>The line of the Ponte do Novelo (region x of the causeway, running south to north).</summary>
        public const float NoveloBridgeX = 1560f;

        public const int NoveloFlightCount = 10;
        public const float NoveloFlightStart = -1090f;
        public const float NoveloFlightStep = 145f;
        public const float NoveloPylonOffset = 7f;

        public const int LampPedestalCount = 5;
        public const float LampPedestalRingRadius = 110f;

        /// <summary>Where the dark globe of the Novelo rests, east of the bridge (region coordinates).</summary>
        public static readonly Vector2 NoveloGlobeCenter = new Vector2(1700f, 130f);

        /// <summary>Where the Gate of the Land of Darkness stands, at the end of the last road (region coordinates).</summary>
        public static readonly Vector2 DarkGateCenter = new Vector2(1560f, 1130f);

        /// <summary>Where pylon <paramref name="i"/> of the ten flights stands (alternating sides of the causeway, region coordinates).</summary>
        public static Vector2 NoveloPylonPosition(int i)
        {
            return new Vector2(NoveloBridgeX + ((i & 1) == 0 ? -NoveloPylonOffset : NoveloPylonOffset), NoveloFlightStart + NoveloFlightStep * i);
        }

        /// <summary>Where lamp pedestal <paramref name="i"/> of the five stands round the Selo dos Artífices (region coordinates).</summary>
        public static Vector2 LampPedestalPosition(int i)
        {
            float a = 72f * i * Mathf.Deg2Rad; // no spoke on the north-south road through the plaza
            return SeloArtificesOffset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * LampPedestalRingRadius;
        }

        /// <summary>The pools of black water of the Cova do Lodo (centre segment, width).</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] LodoPools =
        {
            (new Vector2(900f, -100f), new Vector2(910f, -92f), 24f),
            (new Vector2(1210f, 110f), new Vector2(1220f, 118f), 22f),
            (new Vector2(1240f, -120f), new Vector2(1248f, -112f), 20f),
            (new Vector2(920f, -380f), new Vector2(930f, -372f), 26f),
            (new Vector2(950f, -610f), new Vector2(958f, -604f), 22f),
            (new Vector2(1230f, -430f), new Vector2(1240f, -424f), 26f),
            (new Vector2(1210f, -640f), new Vector2(1218f, -634f), 22f),
            (new Vector2(920f, 400f), new Vector2(930f, 408f), 24f),
            (new Vector2(1230f, 360f), new Vector2(1238f, 368f), 22f),
            (new Vector2(1210f, 620f), new Vector2(1218f, 628f), 22f),
            (new Vector2(950f, 640f), new Vector2(958f, 646f), 20f),
        };

        /// <summary>The existing beasts that stand in for the sheets' monsters (Mapas100 83–90).</summary>
        private static class Profundo
        {
            public const string SmallShadow = "enemy_forest_shadow_biter";
            public const string LampSmoke = "enemy_mountain_ill_wind_mist";
            public const string DarkBat = "enemy_cave_vampire_bat";
            public const string DimUshabti = "enemy_mountain_lacquer_ushabti";
            public const string CopyShadow = "enemy_mountain_almas";
            public const string SmokeGiant = "enemy_mountain_sleepwalker_giant";
            public const string ChokeSmoke = "enemy_forest_smoke_goat";
            public const string SmokeArcher = "enemy_forest_terracotta_soldier";
            public const string EmberWolf = "enemy_mountain_bronze_wolf";
            public const string AshBull = "enemy_mountain_wild_yak";
            public const string HotLion = "enemy_forest_stone_lion";
            public const string HungryFlame = "enemy_desert_firefly_swarm";
            public const string BlackEagle = "enemy_mountain_storm_eagle";
            public const string BladeBat = "enemy_mountain_candle_bat";
            public const string WindHarpy = "enemy_mountain_smoke_harpy";
            public const string Whirlwind = "enemy_mountain_ill_wind_mist";
            public const string DarkFish = "enemy_mountain_blade_fish";
            public const string GreatEel = "enemy_forest_reed_snake";
            public const string MudOctopus = "enemy_cave_ooze";
            public const string DarkShoal = "enemy_cave_lantern_fish";
            public const string BlindSerpent = "enemy_forest_dimmed_python";
            public const string ShadowLizard = "enemy_mountain_giant_lava_lizard";
            public const string Creeper = "enemy_forest_creeping_dark";
            public const string LesserDragon = "enemy_forest_three_headed_serpent";
            public const string LesserArchon = "enemy_desert_crowned_orc";
            public const string GiantUshabti = "enemy_mountain_stone_golem";
        }

        private static OverworldLayout ProfundoRegion()
        {
            OverworldLayout w = NewRegion(ProfundoRegionId, MapRegion.LimiarProfundo, Rect.MinMaxRect(-600f, -1200f, 1800f, 1200f));
            w.World.Biome = BiomeType.Forest;
            const string fortaleza = FortalezaUltimaLampadaMapId;
            const string fumaca = CovaFumacaMapId;
            const string fogo = CovaFogoMapId;
            const string vento = CovaVentoMapId;
            const string lodo = CovaLodoMapId;
            const string treva = CovaTrevaMapId;
            const string ponte = PonteNoveloMapId;
            const string portao = PortaoTrevaMapId;
            const string ruins = MapRegionTypes.Ruins;
            const string under = MapRegionTypes.Underground;
            const string theriac = MapRequirementTags.PoisonProtection;
            const string oil = MapRequirementTags.EternalFireOil;
            const string wings = MapRequirementTags.WingsOrDoubleJump;
            const string bubble = MapRequirementTags.AirBubble;
            const string runic = MapRequirementTags.RunicLamp;
            const string pole = MapRequirementTags.RootBridgeBalance;
            const string lamp = MapRequirementTags.TorchRequired;

            // ---------------- Zones (a 5 × 5 grid; columns x −600/−120/360/840/1320/1800, rows z −1200/−720/−240/240/720/1200) ----------------
            // West column: the glowing floor (85), the niches (83), the way in from the Margins (83), the racks of wicks (83), the loose platforms (86).
            OverworldZone zChao = Zone(w, "chao_que_acende", fogo, null, -600f, -1200f, -120f, -720f, OverworldTerrain.GlowPatternFloor, 363, 370, 359,
                "Floor That Lights", "Chão que Acende", under, oil, 95f);
            OverworldZone zNichos = Zone(w, "nichos_de_basalto", fortaleza, null, -600f, -720f, -120f, -240f, OverworldTerrain.BasaltNiches, 344, 348, 340,
                "Basalt Niches", "Nichos de Basalto", ruins, spacing: 90f);
            OverworldZone zMuralha = Zone(w, "muralha_em_arco", fortaleza, null, -600f, -240f, -120f, 240f, OverworldTerrain.RampartArc, 342, 346, 338,
                "Arc of the Rampart", "Muralha em Arco", ruins, spacing: 90f);
            OverworldZone zVarais = Zone(w, "varais_de_pavios", fortaleza, null, -600f, 240f, -120f, 720f, OverworldTerrain.WickYards, 344, 350, 340,
                "Wick Yards", "Varais de Pavios", ruins, spacing: 90f);
            OverworldZone zPlataformas = Zone(w, "plataformas_soltas", vento, null, -600f, 720f, -120f, 1200f, OverworldTerrain.LoosePlatforms, 376, 382, 372,
                "Loose Platforms", "Plataformas Soltas", under, wings, 100f);
            // Second column: the Devouring Fire (85), the oil cisterns (83), the fortress, the brink (83), the shaft of the wind (86).
            OverworldZone zFogo = Zone(w, "cova_do_fogo_devorador", fogo, null, -120f, -1200f, 360f, -720f, OverworldTerrain.DevouringFirePit, 360, 366, 356,
                "Pit of Devouring Fire", "Cova do Fogo Devorador", under, oil, 95f);
            OverworldZone zCisternas = Zone(w, "cisternas_de_oleo", fortaleza, null, -120f, -720f, 360f, -240f, OverworldTerrain.OilCisterns, 342, 346, 338,
                "Oil Cisterns", "Cisternas de Óleo", ruins, spacing: 90f);
            OverworldZone zFortaleza = Zone(w, "fortaleza_ultima_lampada", fortaleza, null, -120f, -240f, 360f, 240f, OverworldTerrain.LampFortress, 340, 344, 336,
                "Fortress of the Last Lamp", "Fortaleza da Última Lâmpada", ruins, spacing: 85f);
            OverworldZone zBeira = Zone(w, "beira_do_abismo", fortaleza, null, -120f, 240f, 360f, 720f, OverworldTerrain.AbyssBrink, 342, 346, 338,
                "Brink of the Abyss", "Beira do Abismo", ruins, spacing: 90f);
            OverworldZone zVento = Zone(w, "cova_do_vento_destruidor", vento, null, -120f, 720f, 360f, 1200f, OverworldTerrain.WindShaft, 372, 378, 368,
                "Pit of Ruinous Wind", "Cova do Vento Destruidor", under, wings, 100f);
            // Third column: the hot ash halls (85), the vents and the corridors of the Pit of Smoke (84), the updrafts (86).
            OverworldZone zCinzaQuente = Zone(w, "saloes_de_cinza_quente", fogo, null, 360f, -1200f, 840f, -720f, OverworldTerrain.HotAshHalls, 366, 374, 362,
                "Halls of Hot Ash", "Salões de Cinza Quente", under, oil, 95f);
            OverworldZone zVentiladores = Zone(w, "ventiladores_artifices", fumaca, null, 360f, -720f, 840f, -240f, OverworldTerrain.ArtificerVents, 352, 358, 348,
                "Vents of the Artificers", "Ventiladores dos Artífices", under, theriac, 95f);
            OverworldZone zFumaca = Zone(w, "cova_da_fumaca", fumaca, null, 360f, -240f, 840f, 240f, OverworldTerrain.SmokePit, 348, 354, 344,
                "Pit of Smoke", "Cova da Fumaça", under, theriac, 95f);
            OverworldZone zCorredores = Zone(w, "corredores_de_cinza", fumaca, null, 360f, 240f, 840f, 720f, OverworldTerrain.CinderCorridors, 354, 362, 350,
                "Corridors of Cinder", "Corredores de Cinza", under, theriac, 95f);
            OverworldZone zCorrentes = Zone(w, "correntes_ascendentes", vento, null, 360f, 720f, 840f, 1200f, OverworldTerrain.Updrafts, 380, 388, 376,
                "Rising Currents", "Correntes Ascendentes", under, wings, 100f);
            // Fourth column: the Dark (88), the drowned halls, the Mire and the sluices (87), the blind tunnels (88).
            OverworldZone zTreva = Zone(w, "cova_da_treva", treva, null, 840f, -1200f, 1320f, -720f, OverworldTerrain.DarknessPit, 400, 408, 396,
                "Pit of Darkness", "Cova da Treva", under, runic, 100f);
            OverworldZone zAlagados = Zone(w, "saloes_alagados", lodo, null, 840f, -720f, 1320f, -240f, OverworldTerrain.FloodedHalls, 390, 396, 386,
                "Flooded Halls", "Salões Alagados", under, bubble, 95f);
            OverworldZone zLodo = Zone(w, "cova_do_lodo", lodo, null, 840f, -240f, 1320f, 240f, OverworldTerrain.MirePit, 386, 392, 382,
                "Pit of Mire", "Cova do Lodo", under, bubble, 95f);
            OverworldZone zComportas = Zone(w, "comportas_artifices", lodo, null, 840f, 240f, 1320f, 720f, OverworldTerrain.Sluices, 394, 402, 390,
                "Sluices of the Artificers", "Comportas dos Artífices", under, bubble, 95f);
            OverworldZone zTuneis = Zone(w, "tuneis_cegos", treva, null, 840f, 720f, 1320f, 1200f, OverworldTerrain.BlindTunnels, 406, 416, 402,
                "Blind Tunnels", "Túneis Cegos", under, runic, 100f);
            // East column, south to north: the head of the bridge, the bridge with the Pilar do Meio, its last flights (89); the plaza and the gate (90).
            OverworldZone zCabeceira = Zone(w, "cabeceira_da_ponte", ponte, null, 1320f, -1200f, 1800f, -720f, OverworldTerrain.BridgeHead, 412, 418, 408,
                "Head of the Bridge", "Cabeceira da Ponte", ruins, pole, 100f);
            OverworldZone zPonte = Zone(w, "ponte_do_novelo", ponte, null, 1320f, -720f, 1800f, -240f, OverworldTerrain.NoveloBridge, 416, 424, 412,
                "Bridge of the Bōlos", "Ponte do Novelo", ruins, pole, 100f);
            OverworldZone zLances = Zone(w, "lances_do_novelo", ponte, null, 1320f, -240f, 1800f, 240f, OverworldTerrain.NoveloGlobeFlights, 420, 428, 416,
                "Flights of the Bōlos", "Lances do Novelo", ruins, pole, 100f);
            OverworldZone zPraca = Zone(w, "praca_dos_pedestais", portao, null, 1320f, 240f, 1800f, 720f, OverworldTerrain.PedestalPlaza, 424, 432, 420,
                "Plaza of the Pedestals", "Praça dos Pedestais", ruins, lamp, 100f);
            OverworldZone zPortao = Zone(w, "portao_da_terra_da_treva", portao, null, 1320f, 720f, 1800f, 1200f, OverworldTerrain.DarkGateGround, 432, 440, 428,
                "Gate of the Land of Darkness", "Portão da Terra da Treva", ruins, lamp, 100f);

            AddProfundoFauna(zChao, zNichos, zMuralha, zVarais, zPlataformas, zFogo, zCisternas, zFortaleza, zBeira, zVento, zCinzaQuente, zVentiladores, zFumaca, zCorredores, zCorrentes,
                zTreva, zAlagados, zLodo, zComportas, zTuneis, zCabeceira, zPonte, zLances, zPraca, zPortao);

            // ---------------- Settlements ----------------
            // The fortress: the region's return city and the barca's stop for 83–90 (flat ground at y = 0, ±30 m). Then the posts of 84–90.
            w.Vila = Place(w, fortaleza, FortalezaUltimaLampadaRecipe(), FortalezaUltimaLampadaOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), FortalezaProfundoGround, true);
            Place(w, fumaca, Post("lampada_1", TownSwatch.LampBasalt, TownSwatch.BasaltDust, TownRoofKind.Flat, TownShopDressing.Herder, false), LampadaFumacaOffset, PostSafe, 18f, false);
            Place(w, fogo, Post("lampada_2", TownSwatch.LampBasalt, TownSwatch.LampCopper, TownRoofKind.Flat, TownShopDressing.HuntingRack, false), LampadaFogoOffset, PostSafe, 18f, false);
            Place(w, vento, Post("lampada_3", TownSwatch.LampBasalt, TownSwatch.AbyssIndigo, TownRoofKind.Gable, TownShopDressing.Herder, false), LampadaVentoOffset, PostSafe, 18f, false);
            Place(w, lodo, Post("lampada_4", TownSwatch.LampBasalt, TownSwatch.OilDark, TownRoofKind.Flat, TownShopDressing.BoatTrader, false), LampadaLodoOffset, PostSafe, 18f, false);
            Place(w, treva, Post("lampada_5", TownSwatch.LampBasalt, TownSwatch.AbyssBlack, TownRoofKind.Flat, TownShopDressing.Storyteller, false), LampadaTrevaOffset, PostSafe, 18f, false);
            Place(w, ponte, Post("pilar_do_meio", TownSwatch.WickLinen, TownSwatch.LampCopper, TownRoofKind.Flat, TownShopDressing.Storyteller, false), PilarDoMeioOffset, PostSafe, 18f, false);
            Place(w, portao, Post("selo_dos_artifices", TownSwatch.LampBasalt, TownSwatch.ThousandLamps, TownRoofKind.Flat, TownShopDressing.Storyteller, false), SeloArtificesOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            foreach ((Vector2 a, Vector2 b, float width) in LodoPools)
            {
                River(w, width, a, b);
            }

            // ---------------- Gates out of the region ----------------
            // West: two passes to the Margens da Mistura (the Field of the Dream Tablets; tagged with maps 82 and 81: one target per gate). No gate south: Farnkesh is level ~200.
            Gate(w, "exit_tabuas_sul", MapExitKind.Map, TabuasSonhoMapId, Rect.MinMaxRect(-600f, -108f, -597f, -92f), new Vector2(-591f, -100f), Vector2.right);
            Gate(w, "exit_tabuas_norte", MapExitKind.Map, OmbroUpelluriMapId, Rect.MinMaxRect(-600f, 152f, -597f, 168f), new Vector2(-591f, 160f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = FortalezaProfundoGround;
            // West road from the fortress through the arc of the rampart to the two passes to the Margins.
            Vector2 westFork = new Vector2(-300f, 30f);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-150f, 10f), westFork);
            PathVia(w, 4f, westFork, new Vector2(-450f, -40f), new Vector2(-591f, -100f));
            PathVia(w, 4f, westFork, new Vector2(-450f, 100f), new Vector2(-591f, 160f));
            // From the fork, north past the wick yards to the loose platforms, south past the niches to the glowing floor.
            PathVia(w, 3.5f, westFork, new Vector2(-320f, 320f), new Vector2(-360f, 640f), new Vector2(-360f, 960f));
            PathVia(w, 3.5f, westFork, new Vector2(-340f, -280f), new Vector2(-380f, -560f), new Vector2(-400f, -760f), new Vector2(-360f, -960f));

            // South road: past the cisterns to the Devouring Fire and its post (the entrance street of the city).
            Vector2 southFork = new Vector2(20f, -240f);
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(10f, -130f), southFork, new Vector2(40f, -500f), new Vector2(90f, -800f), LampadaFogoOffset + new Vector2(0f, 18f));
            // West and east along the south row: the glowing floor and on to the hot ash and the Dark.
            PathVia(w, 3.5f, LampadaFogoOffset + new Vector2(-18f, 0f), new Vector2(-220f, -960f), new Vector2(-360f, -960f));
            PathVia(w, 3.5f, LampadaFogoOffset + new Vector2(18f, 0f), new Vector2(360f, -960f), new Vector2(600f, -960f), LampadaTrevaOffset + new Vector2(-18f, 0f));

            // East road from the fortress to the Pit of Smoke and the Pit of Mire.
            Vector2 eastFork = new Vector2(300f, 20f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(150f, 10f), eastFork, new Vector2(420f, 8f), LampadaFumacaOffset + new Vector2(-18f, 0f));
            PathVia(w, 4f, LampadaFumacaOffset + new Vector2(18f, 0f), new Vector2(790f, 6f), LampadaLodoOffset + new Vector2(-18f, 0f));

            // North of the east fork: the brink of the abyss and the Pit of Ruinous Wind.
            PathVia(w, 4f, eastFork, new Vector2(260f, 300f), new Vector2(220f, 640f), LampadaVentoOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, LampadaVentoOffset + new Vector2(-18f, 0f), new Vector2(-220f, 960f), new Vector2(-360f, 960f));
            PathVia(w, 3.5f, LampadaVentoOffset + new Vector2(18f, 0f), new Vector2(360f, 960f), new Vector2(600f, 960f), new Vector2(860f, 960f), new Vector2(1080f, 960f));

            // Vents and corridors off the Pit of Smoke, joining the south and north rows.
            PathVia(w, 3.5f, LampadaFumacaOffset + new Vector2(0f, -18f), new Vector2(600f, -240f), new Vector2(600f, -480f), new Vector2(600f, -700f), new Vector2(600f, -960f));
            PathVia(w, 3.5f, LampadaFumacaOffset + new Vector2(0f, 18f), new Vector2(600f, 240f), new Vector2(600f, 480f), new Vector2(620f, 720f), new Vector2(600f, 960f));

            // The Mire: south by the drowned halls to the Dark, north by the sluices to the blind tunnels.
            PathVia(w, 3.5f, LampadaLodoOffset + new Vector2(0f, -18f), new Vector2(1080f, -240f), new Vector2(1080f, -480f), new Vector2(1060f, -720f), LampadaTrevaOffset + new Vector2(0f, 18f));
            PathVia(w, 3.5f, LampadaLodoOffset + new Vector2(0f, 18f), new Vector2(1080f, 240f), new Vector2(1080f, 480f), new Vector2(1060f, 720f), new Vector2(1080f, 960f));

            // The Ponte do Novelo, south to north along one straight line, from the Dark over the Pilar do Meio to the Selo and the Gate.
            float bx = NoveloBridgeX;
            PathVia(w, 4f, LampadaTrevaOffset + new Vector2(18f, 0f), new Vector2(1320f, -960f), new Vector2(bx, -960f));
            PathVia(w, 4f, new Vector2(bx, -960f), new Vector2(bx, -720f), PilarDoMeioOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, PilarDoMeioOffset + new Vector2(0f, 18f), new Vector2(bx, -240f), new Vector2(bx, 0f), new Vector2(bx, 240f), SeloArtificesOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, SeloArtificesOffset + new Vector2(0f, 18f), new Vector2(bx, 720f), new Vector2(bx, 960f), new Vector2(bx, 1090f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-320f, 320f), 4f, 0f);
            Sign(w, southFork, new Vector2(40f, -500f), 4f, 0f);
            Sign(w, eastFork, new Vector2(260f, 300f), 4f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -470f, 280f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -220f, -560f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 470f, 420f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 480f, -600f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -170f, 1050f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 930f, 180f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 1410f, -860f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 1400f, 1000f, 0f, 1f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            KeepClearingsOff(w, NoveloGlobeCenter, 9.5f);
            KeepClearingsOff(w, DarkGateCenter, 17.5f);
            TrimSteepClearings(w);
            return w;
        }

        /// <summary>Drops the clearings that would stand on one of the two great fixed pieces (the chunk builder only places them on free ground).</summary>
        private static void KeepClearingsOff(OverworldLayout w, Vector2 center, float radius)
        {
            foreach (MapSpawnZone spawn in w.World.Zones.FindAll(s => Vector2.Distance(s.Center, center) < s.Radius + radius + 12f))
            {
                w.ZoneOfSpawn.Remove(spawn.Id);
                w.World.Zones.Remove(spawn);
            }
        }

        // ------------------------------------------------------------------
        // The city (its own recipe, Cities/Town: the BasaltFort style on its reserved flat ground of ±30 m at y = 0; no backdrop: open land all
        // round it). Its streets leave W, E and S (no north street: the sealed gates close the plaza to the north), so the north road leaves from
        // the east road.
        // ------------------------------------------------------------------

        private static CityTownRecipe FortalezaUltimaLampadaRecipe()
        {
            // The hand recipe has no landing (it lives in Cities/); the barca stops at the end of the south street, east side, clear of the keep, the steles and the rampart.
            CityTownRecipe recipe = NoBackdrop(CityTownRecipes.FortalezaUltimaLampada());
            recipe.Lots.Add(new CityLotRecipe { Id = "cais_da_barca", Kind = CityLotKind.ShipDock, Arm = CityArm.South, Side = CityArm.East, Along = 18f, Width = 4f, Depth = 3f, Porch = 1f });
            return recipe;
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 83–90 archetypes)
        // ------------------------------------------------------------------

        private static void ProfundoGroup(OverworldZone zone, string id, string archetype, string lead, string mate, NightSpecies night, int maxAlive = 4, float respawn = 25f)
        {
            zone.Fauna.Add(Fauna(id, archetype, T(N(lead, 3), N(mate, 1)), T(V(lead, 3), S(night, 1)), maxAlive, null, respawn));
        }

        private static void AddProfundoFauna(OverworldZone chao, OverworldZone nichos, OverworldZone muralha, OverworldZone varais, OverworldZone plataformas, OverworldZone fogo,
            OverworldZone cisternas, OverworldZone fortaleza, OverworldZone beira, OverworldZone vento, OverworldZone cinzaQuente, OverworldZone ventiladores, OverworldZone fumaca,
            OverworldZone corredores, OverworldZone correntes, OverworldZone treva, OverworldZone alagados, OverworldZone lodo, OverworldZone comportas, OverworldZone tuneis,
            OverworldZone cabeceira, OverworldZone ponte, OverworldZone lances, OverworldZone praca, OverworldZone portao)
        {
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;
            NightSpecies werewolf = NightVariantRules.Lobisomem;

            // Outside the gates of the fortress (83): the Sombra Pequena [rápido] (the shadow biter), the Fumaça [controle] (the ill-wind mist), the
            // Morcego da Treva [enxame] (the vampire bat), the Ushabti Apagado [tanque] (the lacquer ushabti).
            ProfundoGroup(fortaleza, "sombras_da_muralha", "[rápido]", Profundo.SmallShadow, Profundo.DarkBat, moon);
            ProfundoGroup(fortaleza, "morcegos_da_muralha", "[enxame]", Profundo.DarkBat, Profundo.SmallShadow, bats, 5);
            ProfundoGroup(muralha, "ushabtis_da_muralha", "[tanque]", Profundo.DimUshabti, Profundo.SmallShadow, moon, 3, 30f);
            ProfundoGroup(muralha, "sombras_do_arco", "[rápido]", Profundo.SmallShadow, Profundo.DimUshabti, owl);
            ProfundoGroup(nichos, "fumacas_dos_nichos", "[controle]", Profundo.LampSmoke, Profundo.DarkBat, moon);
            ProfundoGroup(nichos, "morcegos_dos_nichos", "[enxame]", Profundo.DarkBat, Profundo.LampSmoke, bats, 5);
            ProfundoGroup(cisternas, "ushabtis_das_cisternas", "[tanque]", Profundo.DimUshabti, Profundo.LampSmoke, moon, 3, 30f);
            ProfundoGroup(cisternas, "sombras_das_cisternas", "[rápido]", Profundo.SmallShadow, Profundo.LampSmoke, owl);
            ProfundoGroup(varais, "fumacas_dos_varais", "[controle]", Profundo.LampSmoke, Profundo.SmallShadow, sparks);
            ProfundoGroup(varais, "ushabtis_dos_varais", "[tanque]", Profundo.DimUshabti, Profundo.DarkBat, moon, 3, 30f);
            ProfundoGroup(beira, "morcegos_do_abismo", "[enxame]", Profundo.DarkBat, Profundo.DimUshabti, bats, 5);
            ProfundoGroup(beira, "sombras_do_abismo", "[rápido]", Profundo.SmallShadow, Profundo.DimUshabti, werewolf);

            // The Pit of Smoke (84): the Sombra-Cópia [rápido] (the almas), the Gigante de Fumaça [tanque] (the sleepwalker giant), the Fumaça Sufocante
            // [controle] (the smoke goat), the Arqueiro de Fumaça [distância] (the terracotta soldier).
            ProfundoGroup(fumaca, "sombras_copia", "[rápido]", Profundo.CopyShadow, Profundo.ChokeSmoke, owl);
            ProfundoGroup(fumaca, "gigantes_de_fumaca", "[tanque]", Profundo.SmokeGiant, Profundo.CopyShadow, moon, 3, 30f);
            ProfundoGroup(ventiladores, "fumacas_sufocantes", "[controle]", Profundo.ChokeSmoke, Profundo.SmokeArcher, bats);
            ProfundoGroup(ventiladores, "arqueiros_dos_ventiladores", "[distância]", Profundo.SmokeArcher, Profundo.ChokeSmoke, moon);
            ProfundoGroup(corredores, "arqueiros_de_fumaca", "[distância]", Profundo.SmokeArcher, Profundo.CopyShadow, owl);
            ProfundoGroup(corredores, "gigantes_dos_corredores", "[tanque]", Profundo.SmokeGiant, Profundo.SmokeArcher, werewolf, 3, 30f);

            // The Pit of Devouring Fire (85): the Lobo de Brasa [rápido] (the bronze wolf), the Touro de Cinza [tanque] (the wild yak), the Leão de Fumaça
            // Quente [elite] (the stone lion), the Chama Faminta [enxame] (the firefly swarm).
            ProfundoGroup(fogo, "lobos_de_brasa", "[rápido]", Profundo.EmberWolf, Profundo.HungryFlame, moon);
            ProfundoGroup(fogo, "chamas_famintas", "[enxame]", Profundo.HungryFlame, Profundo.EmberWolf, sparks, 5);
            ProfundoGroup(chao, "touros_de_cinza", "[tanque]", Profundo.AshBull, Profundo.HungryFlame, bats, 3, 30f);
            ProfundoGroup(chao, "chamas_do_chao", "[enxame]", Profundo.HungryFlame, Profundo.AshBull, sparks, 5);
            ProfundoGroup(cinzaQuente, "leoes_de_fumaca_quente", "[elite]", Profundo.HotLion, Profundo.EmberWolf, werewolf, 3, 32f);
            ProfundoGroup(cinzaQuente, "lobos_da_cinza", "[rápido]", Profundo.EmberWolf, Profundo.AshBull, owl);

            // The Pit of Ruinous Wind (86): the Águia Negra [rápido] (the storm eagle), the Morcego-Lâmina [enxame] (the candle bat), the Harpia de Vento
            // [distância] (the smoke harpy), the Redemoinho [controle] (the ill-wind mist).
            ProfundoGroup(vento, "aguias_negras", "[rápido]", Profundo.BlackEagle, Profundo.BladeBat, owl);
            ProfundoGroup(vento, "morcegos_lamina", "[enxame]", Profundo.BladeBat, Profundo.BlackEagle, bats, 5);
            ProfundoGroup(plataformas, "harpias_de_vento", "[distância]", Profundo.WindHarpy, Profundo.Whirlwind, moon);
            ProfundoGroup(plataformas, "redemoinhos", "[controle]", Profundo.Whirlwind, Profundo.WindHarpy, owl);
            ProfundoGroup(correntes, "aguias_das_correntes", "[rápido]", Profundo.BlackEagle, Profundo.WindHarpy, moon);
            ProfundoGroup(correntes, "redemoinhos_das_correntes", "[controle]", Profundo.Whirlwind, Profundo.BladeBat, sparks);

            // The Pit of Mire (87): the Peixe da Treva [rápido] (the blade fish), the Enguia Grande [controle] (the reed snake), the Polvo de Lodo [tanque]
            // (the ooze), the Cardume Escuro [enxame] (the lantern fish).
            ProfundoGroup(lodo, "peixes_da_treva", "[rápido]", Profundo.DarkFish, Profundo.DarkShoal, moon);
            ProfundoGroup(lodo, "polvos_de_lodo", "[tanque]", Profundo.MudOctopus, Profundo.DarkFish, bats, 3, 30f);
            ProfundoGroup(alagados, "enguias_grandes", "[controle]", Profundo.GreatEel, Profundo.DarkShoal, moon);
            ProfundoGroup(alagados, "cardumes_escuros", "[enxame]", Profundo.DarkShoal, Profundo.GreatEel, sparks, 5);
            ProfundoGroup(comportas, "polvos_das_comportas", "[tanque]", Profundo.MudOctopus, Profundo.GreatEel, moon, 3, 30f);
            ProfundoGroup(comportas, "peixes_das_comportas", "[rápido]", Profundo.DarkFish, Profundo.MudOctopus, owl);

            // The Pit of Darkness (88): the Serpente Cega [rápido] (the dimmed python), the Lagarto-Sombra [tanque] (the giant lava lizard), the Rastejante
            // [enxame] (the creeping dark), the Dragão-Sombra Menor [elite] (the three-headed serpent).
            ProfundoGroup(treva, "serpentes_cegas", "[rápido]", Profundo.BlindSerpent, Profundo.Creeper, bats);
            ProfundoGroup(treva, "lagartos_sombra", "[tanque]", Profundo.ShadowLizard, Profundo.BlindSerpent, moon, 3, 30f);
            ProfundoGroup(tuneis, "rastejantes", "[enxame]", Profundo.Creeper, Profundo.ShadowLizard, sparks, 5);
            ProfundoGroup(tuneis, "dragoes_sombra_menores", "[elite]", Profundo.LesserDragon, Profundo.Creeper, werewolf, 3, 32f);

            // The Bridge of the Bōlos (89): the five kinds of the pits mixed.
            ProfundoGroup(cabeceira, "filhos_da_fumaca", "[rápido]", Profundo.CopyShadow, Profundo.EmberWolf, owl);
            ProfundoGroup(cabeceira, "filhos_do_fogo", "[elite]", Profundo.HotLion, Profundo.BlackEagle, moon, 3, 32f);
            ProfundoGroup(ponte, "filhos_do_vento", "[rápido]", Profundo.BlackEagle, Profundo.DarkFish, bats);
            ProfundoGroup(ponte, "filhos_do_lodo", "[tanque]", Profundo.MudOctopus, Profundo.Creeper, moon, 3, 30f);
            ProfundoGroup(lances, "filhos_da_treva", "[enxame]", Profundo.Creeper, Profundo.SmokeArcher, sparks, 5);
            ProfundoGroup(lances, "arqueiros_do_novelo", "[distância]", Profundo.SmokeArcher, Profundo.AshBull, owl);

            // The Gate of the Land of Darkness (90): the Filhos da Treva, the Arconte Menor [elite] (the crowned orc), the Ushabti Apagado Gigante [tanque]
            // (the stone golem).
            ProfundoGroup(praca, "arcontes_menores", "[elite]", Profundo.LesserArchon, Profundo.Creeper, werewolf, 3, 32f);
            ProfundoGroup(praca, "ushabtis_gigantes", "[tanque]", Profundo.GiantUshabti, Profundo.LesserArchon, moon, 3, 30f);
            ProfundoGroup(portao, "arcontes_do_portao", "[elite]", Profundo.LesserArchon, Profundo.LesserDragon, werewolf, 3, 32f);
            ProfundoGroup(portao, "ushabtis_do_portao", "[tanque]", Profundo.GiantUshabti, Profundo.BladeBat, bats, 3, 30f);
        }
    }
}
