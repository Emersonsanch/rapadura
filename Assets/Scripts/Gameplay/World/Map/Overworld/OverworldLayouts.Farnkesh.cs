using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 10 — Picos de Farnkesh as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 53–58), no scene
    /// change between the satyrs' vale and the pinnacles above the clouds. The Vale dos Sátiros (53, the first map of the region,
    /// level 140) stands at the plaza (0, 0) on the Pactolo, 600 m from the south edge where two passes come up from the Estepe
    /// dos Nartes (in the stitched world the peaks stand north of the steppe, over the meltwater lake and Ohya's crest; the world
    /// grows a row: west of the peaks, north of Mingcheng and of the Teto do Mundo, nothing — two walled holes, a fall into the
    /// clouds); Farnkesh, the Stone Nests (54, the capital), stands on its pinnacles 1080 m north of the vale:
    /// <list type="bullet">
    /// <item>round the vale (nv 140–145): the <b>Vale dos Sátiros</b> and the <b>Clareiras do Baile</b>, the <b>Vau do
    /// Pactolo</b> and the <b>Rio Pactolo</b> (golden pebbles), the <b>Vinhas de Gordias</b> and the <b>Vinhas do Sol</b>;</item>
    /// <item>the first slopes (nv 145–150): the <b>Mata das Castanheiras</b> and the <b>Bosque do Pé dos Picos</b>, the
    /// <b>Prados do Vento</b> and the <b>Prados dos Sinos</b>, the <b>Subida de Farnkesh</b>, the <b>Escadaria dos Cestos</b>
    /// and the <b>Mar de Nuvens</b> (54), the <b>Bosque de Pináculos</b>;</item>
    /// <item>the heights (nv 145–168): <b>Farnkesh</b> (54) on its stone nests, the <b>Pináculos do Leste</b>, the <b>Pontes de
    /// Vento</b> and the <b>Ermida Baixa</b> (55, post), the <b>Pináculo do Sino</b> (56, post) and the <b>Ermidas
    /// Suspensas</b>;</item>
    /// <item>above the clouds (nv 160–200): the <b>Escarpa dos Ventos</b>, the <b>Gruta Abrigo</b> (57, post) and the
    /// <b>Garganta dos Ays</b> (57: Storm Protection), the <b>Ilhas de Nuvem</b> with the Último Pináculo (58, post) and the
    /// <b>Ninho da Huma</b> (58: wings or a glide).</item>
    /// </list>
    /// Water: the Pactolo rises in the cloud lake and runs south past the vale to the south edge, where the Rio dos Nartes
    /// begins on the other side; a wide ford of golden pebbles by the vale and a stone bridge under the cloud sea. Monsters:
    /// the region's own beasts (the smoke goat, Midas's golden bee, the flute bird and the vine fox in the vale; the alpine
    /// chough, the takin, the blue sheep, the candle bat, the rope ushabti, the Pallas cat, the storm eagle and the smoke harpy
    /// on the peaks), the Nartes' young griffin for the wild griffins, existing beasts that fit the other sheets (the ember
    /// vulture for the peak vultures, the broken lantern for the shadow kites, the cracked bells and the trapped lightning, the
    /// ill-wind mist for the ill-wind fogs, the echoes, the Ays and the gales, the rolling stone for the flying stones, the brown
    /// bear and the spider in the foot woods, the giant marmot in the meadows, the thief monkey in the vines) and the night
    /// species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string FarnkeshRegionId = "overworld_farnkesh";

        public const string ValeSatirosMapId = "map_053_vale_satiros";
        public const string FarnkeshMapId = "map_054_farnkesh";
        public const string PontesVentoMapId = "map_055_pontes_vento";
        public const string ErmidasMapId = "map_056_ermidas";
        public const string GargantaAysMapId = "map_057_garganta_ays";
        public const string NinhoHumaMapId = "map_058_ninho_huma";

        /// <summary>The Rei-Vendaval's arena (58): the stone ring of the nest.</summary>
        public static readonly Vector2 ReiVendavalArena = new Vector2(880f, 1500f);
        public static readonly Vector2 ValeSatirosOffset = Vector2.zero;
        public static readonly Vector2 FarnkeshOffset = new Vector2(0f, 1080f);
        public static readonly Vector2 ErmidaBaixaOffset = new Vector2(960f, 1080f);
        public static readonly Vector2 PinaculoDoSinoOffset = new Vector2(-480f, 1080f);
        public static readonly Vector2 GrutaAbrigoOffset = new Vector2(-480f, 1560f);
        public static readonly Vector2 UltimoPinaculoOffset = new Vector2(480f, 1560f);

        /// <summary>Half the side (m) of the flat ground of the two towns of the peaks (reserved for their recipes, ±28 m and more).</summary>
        public const float FarnkeshCityGround = 30f;

        /// <summary>The Pactolo: from the cloud lake south past the vale to the south edge, where the Rio dos Nartes begins.</summary>
        public static readonly Vector2[] PactoloRiver =
        {
            new Vector2(300f, 720f), new Vector2(280f, 560f), new Vector2(230f, 420f), new Vector2(170f, 280f), new Vector2(130f, 140f),
            new Vector2(140f, -20f), new Vector2(180f, -160f), new Vector2(240f, -320f), new Vector2(270f, -470f), new Vector2(280f, -600f),
        };

        /// <summary>
        /// The rope bridges of the Pontes de Vento (55) and of Farnkesh (54) in the region's own coordinates: lines from pinnacle to
        /// pinnacle over the hollows (the chunk builder lays their planks along them where the ground is the bridges' or the nests').
        /// </summary>
        public static readonly Vector2[][] WindBridgeLines =
        {
            new[] { new Vector2(300f, 900f), new Vector2(420f, 960f), new Vector2(560f, 940f), new Vector2(680f, 1050f), new Vector2(760f, 1180f), new Vector2(900f, 1240f), new Vector2(1100f, 1220f) },
            new[] { new Vector2(330f, 1260f), new Vector2(460f, 1180f), new Vector2(600f, 1230f), new Vector2(700f, 1300f) },
            new[] { new Vector2(780f, 880f), new Vector2(860f, 980f), new Vector2(1060f, 960f), new Vector2(1140f, 1100f) },
            new[] { new Vector2(-200f, 900f), new Vector2(-120f, 1000f), new Vector2(-150f, 1200f), new Vector2(-60f, 1290f), new Vector2(120f, 1250f), new Vector2(200f, 1150f) },
        };

        /// <summary>The region's own rectangle (where the Farnkesh mechanics apply, in the coordinates of the routes below).</summary>
        public static readonly Rect FarnkeshBounds = Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string FarnkeshAnchorZoneId = "farnkesh";

        /// <summary>The open ground the gusts reach besides the bridges (the Gorge of the Ays, region coordinates).</summary>
        public static readonly Rect[] GustAreas = { Rect.MinMaxRect(-1200f, 1320f, -720f, 1800f) };

        /// <summary>The axis of the gusts in the gorge.</summary>
        public static readonly Vector2 GorgeWindAxis = new Vector2(0.8f, 0.6f);

        /// <summary>
        /// The rope baskets (54–57, A = the low end) and the wind currents of the cloud isles (58, A = the end the current leaves
        /// from), landings on the trails and on the isle tops, in the region's own coordinates; each prop stands beside its landing.
        /// </summary>
        public static readonly SkyRoute[] SkyRoutes =
        {
            Basket("cesto_escadaria", new Vector2(-30f, 640f), new Vector2(3.8f, 0f), new Vector2(10f, 860f), new Vector2(3.5f, 1.5f)),
            Basket("cesto_sino", new Vector2(-700f, 860f), new Vector2(-3.8f, 0f), new Vector2(-560f, 988.5f), new Vector2(1.5f, -3.5f)),
            Basket("cesto_ermida", new Vector2(700f, 1000f), new Vector2(0f, 3.8f), new Vector2(860f, 1060f), new Vector2(2.7f, -2.7f)),
            Basket("cesto_gruta", new Vector2(-474f, 1224f), new Vector2(3.5f, 1.5f), new Vector2(-480f, 1530f), new Vector2(3.5f, 1.5f)),
            Current("corrente_ultimo", new Vector2(540f, 1568f), new Vector2(-2.7f, 2.7f), new Vector2(692f, 1556f), new Vector2(3.5f, -1.5f), 0),
            Current("corrente_ilhas", new Vector2(692f, 1556f), new Vector2(3.5f, -1.5f), new Vector2(900f, 1492f), new Vector2(3.5f, -1.5f), 1),
            Current("corrente_huma", new Vector2(900f, 1492f), new Vector2(3.5f, -1.5f), new Vector2(1052f, 1500f), new Vector2(3.5f, -1.5f), 2),
            Current("corrente_ninho", new Vector2(841f, 1621f), new Vector2(0f, -3.8f), new Vector2(1052f, 1500f), new Vector2(3.5f, -1.5f), 0),
        };

        private static SkyRoute Basket(string id, Vector2 low, Vector2 lowProp, Vector2 high, Vector2 highProp) => new SkyRoute
        {
            Id = id, Kind = SkyRouteKind.RopeBasket, A = low, B = high, PropA = low + lowProp, PropB = high + highProp,
        };

        private static SkyRoute Current(string id, Vector2 from, Vector2 fromProp, Vector2 to, Vector2 toProp, int group) => new SkyRoute
        {
            Id = id, Kind = SkyRouteKind.Glide, A = from, B = to, PropA = from + fromProp, PropB = to + toProp, CurrentGroup = group,
        };

        private static class Fark
        {
            public const string SmokeGoat = "enemy_forest_smoke_goat";
            public const string Bee = "enemy_forest_golden_bee";
            public const string FluteBird = "enemy_forest_flute_bird";
            public const string VineFox = "enemy_forest_vine_fox";
            public const string Chough = "enemy_mountain_alpine_chough";
            public const string Takin = "enemy_mountain_takin";
            public const string BlueSheep = "enemy_mountain_blue_sheep";
            public const string Vulture = "enemy_desert_ember_vulture";
            public const string Griffin = "enemy_mountain_young_griffin";
            public const string CandleBat = "enemy_mountain_candle_bat";
            public const string Kite = "enemy_desert_broken_lantern";
            public const string Mist = "enemy_mountain_ill_wind_mist";
            public const string Ushabti = "enemy_mountain_rope_ushabti";
            public const string Cat = "enemy_mountain_pallas_cat";
            public const string StormEagle = "enemy_mountain_storm_eagle";
            public const string Harpy = "enemy_mountain_smoke_harpy";
            public const string Stone = "enemy_mountain_rolling_stone";
            public const string Bear = "enemy_mountain_brown_bear";
            public const string Spider = "enemy_forest_spider";
            public const string Marmot = "enemy_mountain_giant_marmot";
            public const string Monkey = "enemy_forest_thief_monkey";
        }

        private static OverworldLayout FarnkeshRegion()
        {
            OverworldLayout w = NewRegion(FarnkeshRegionId, MapRegion.PicosDeFarnkesh, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Mountain;
            const string vale = ValeSatirosMapId;
            const string farn = FarnkeshMapId;
            const string pontes = PontesVentoMapId;
            const string ermidas = ErmidasMapId;
            const string garganta = GargantaAysMapId;
            const string ninho = NinhoHumaMapId;
            const string forest = MapRegionTypes.Forest;
            const string rock = MapRegionTypes.Mountain;
            const string storm = MapRequirementTags.StormProtection;
            const string wings = MapRequirementTags.WingsOrDoubleJump;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards the Estepe dos Nartes): the foot woods, Gordias's vines (the way in from the meltwater lake), the
            // Pactolo's ford, the vines of the sun, the windy meadows (the way in from Ohya's crest).
            OverworldZone zPe = Zone(w, "bosque_do_pe_dos_picos", null, vale, -1200f, -600f, -720f, -120f, OverworldTerrain.FootWoods, 146, 150, 144,
                "Wood at the Foot of the Peaks", "Bosque do Pé dos Picos", forest, spacing: 95f);
            OverworldZone zGordias = Zone(w, "vinhas_de_gordias", vale, null, -720f, -600f, -240f, -120f, OverworldTerrain.VineVale, 140, 144, 138,
                "Gordias's Vines", "Vinhas de Gordias", forest, spacing: 90f);
            OverworldZone zVau = Zone(w, "vau_do_pactolo", vale, null, -240f, -600f, 240f, -120f, OverworldTerrain.GoldenRiver, 140, 143, 138,
                "Ford of the Pactolus", "Vau do Pactolo", forest, spacing: 90f);
            OverworldZone zSol = Zone(w, "vinhas_do_sol", null, vale, 240f, -600f, 720f, -120f, OverworldTerrain.VineVale, 141, 145, 139,
                "Vines of the Sun", "Vinhas do Sol", forest, spacing: 90f);
            OverworldZone zPradosVento = Zone(w, "prados_do_vento", null, farn, 720f, -600f, 1200f, -120f, OverworldTerrain.AlpineMeadow, 146, 150, 144,
                "Windy Meadows", "Prados do Vento", rock, spacing: 95f);
            // Second row: the chestnut wood, the dance glades, the Vale dos Sátiros (53), the Pactolo, the meadows of the bells.
            OverworldZone zCastanheiras = Zone(w, "mata_das_castanheiras", vale, null, -1200f, -120f, -720f, 360f, OverworldTerrain.FootWoods, 145, 148, 143,
                "Chestnut Wood", "Mata das Castanheiras", forest, spacing: 95f);
            OverworldZone zBaile = Zone(w, "clareiras_do_baile", vale, null, -720f, -120f, -240f, 360f, OverworldTerrain.SatyrGlades, 140, 144, 138,
                "Glades of the Dance", "Clareiras do Baile", forest, spacing: 90f);
            OverworldZone zVale = Zone(w, "vale_dos_satiros", vale, null, -240f, -120f, 240f, 360f, OverworldTerrain.SatyrGlades, 140, 143, 138,
                "Satyr Vale", "Vale dos Sátiros", forest, spacing: 85f);
            OverworldZone zPactolo = Zone(w, "rio_pactolo", vale, null, 240f, -120f, 720f, 360f, OverworldTerrain.GoldenRiver, 141, 145, 139,
                "River Pactolus", "Rio Pactolo", forest, spacing: 90f);
            OverworldZone zSinos = Zone(w, "prados_dos_sinos", null, farn, 720f, -120f, 1200f, 360f, OverworldTerrain.AlpineMeadow, 146, 150, 144,
                "Meadows of the Bells", "Prados dos Sinos", rock, spacing: 95f);
            // Middle row: the pinnacle wood, the climb to Farnkesh and the stair of baskets (54), the sea of clouds (54), the eastern
            // pinnacles (55).
            OverworldZone zBosquePin = Zone(w, "bosque_de_pinaculos", null, ermidas, -1200f, 360f, -720f, 840f, OverworldTerrain.PinnacleWood, 148, 152, 146,
                "Pinnacle Wood", "Bosque de Pináculos", rock, spacing: 100f);
            OverworldZone zSubida = Zone(w, "subida_de_farnkesh", farn, null, -720f, 360f, -240f, 840f, OverworldTerrain.AlpineMeadow, 145, 148, 141,
                "Climb to Farnkesh", "Subida de Farnkesh", rock, spacing: 95f);
            OverworldZone zEscadaria = Zone(w, "escadaria_dos_cestos", farn, null, -240f, 360f, 240f, 840f, OverworldTerrain.PinnacleWood, 145, 149, 141,
                "Stair of the Baskets", "Escadaria dos Cestos", rock, spacing: 100f);
            OverworldZone zNuvens = Zone(w, "mar_de_nuvens", farn, null, 240f, 360f, 720f, 840f, OverworldTerrain.CloudSea, 145, 149, 141,
                "Sea of Clouds", "Mar de Nuvens", rock, spacing: 95f);
            OverworldZone zLeste = Zone(w, "pinaculos_do_leste", pontes, null, 720f, 360f, 1200f, 840f, OverworldTerrain.PinnacleWood, 150, 154, 146,
                "Eastern Pinnacles", "Pináculos do Leste", rock, spacing: 100f);
            // Fourth row: the hanging hermitages and the Pináculo do Sino (56), Farnkesh (54), the wind bridges and the Ermida Baixa (55).
            OverworldZone zErmidas = Zone(w, "ermidas_suspensas", ermidas, null, -1200f, 840f, -720f, 1320f, OverworldTerrain.BellCliffs, 158, 164, 154,
                "Hanging Hermitages", "Ermidas Suspensas", rock, spacing: 100f);
            OverworldZone zSino = Zone(w, "pinaculo_do_sino", ermidas, null, -720f, 840f, -240f, 1320f, OverworldTerrain.BellCliffs, 158, 162, 154,
                "Bell Pinnacle", "Pináculo do Sino", rock, spacing: 100f);
            OverworldZone zFarnkesh = Zone(w, "farnkesh", farn, null, -240f, 840f, 240f, 1320f, OverworldTerrain.StoneNests, 145, 152, 141,
                "Farnkesh, the Stone Nests", "Farnkesh, os Ninhos de Pedra", rock, spacing: 95f);
            OverworldZone zPontes = Zone(w, "pontes_de_vento", pontes, null, 240f, 840f, 720f, 1320f, OverworldTerrain.WindBridges, 150, 156, 146,
                "Wind Bridges", "Pontes de Vento", rock, spacing: 100f);
            OverworldZone zErmidaBaixa = Zone(w, "ermida_baixa", pontes, null, 720f, 840f, 1200f, 1320f, OverworldTerrain.WindBridges, 152, 158, 148,
                "Low Hermitage", "Ermida Baixa", rock, spacing: 100f);
            // North row, above the clouds: the Gorge of the Ays and the Gruta Abrigo (57: Storm Protection), the windy scree, the cloud
            // isles and Huma's nest (58: wings or a glide).
            OverworldZone zGarganta = Zone(w, "garganta_dos_ays", garganta, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.AysGorge, 170, 180, 166,
                "Gorge of the Ays", "Garganta dos Ays", rock, storm, 100f);
            OverworldZone zGruta = Zone(w, "gruta_abrigo", garganta, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.AysGorge, 166, 172, 162,
                "Shelter Cave", "Gruta Abrigo", rock, storm, 100f);
            OverworldZone zEscarpa = Zone(w, "escarpa_dos_ventos", null, garganta, -240f, 1320f, 240f, 1800f, OverworldTerrain.WindScree, 160, 166, 158,
                "Scarp of the Winds", "Escarpa dos Ventos", rock, spacing: 100f);
            OverworldZone zIlhas = Zone(w, "ilhas_de_nuvem", ninho, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.CloudIsles, 180, 188, 176,
                "Cloud Isles", "Ilhas de Nuvem", rock, wings, 100f);
            OverworldZone zNinho = Zone(w, "ninho_da_huma", ninho, null, 720f, 1320f, 1200f, 1800f, OverworldTerrain.HumaNest, 186, 200, 182,
                "Huma's Nest", "Ninho da Huma", rock, wings, 100f);

            AddFarnkeshFauna(zPe, zGordias, zVau, zSol, zPradosVento, zCastanheiras, zBaile, zVale, zPactolo, zSinos, zBosquePin, zSubida, zEscadaria, zNuvens,
                zLeste, zErmidas, zSino, zFarnkesh, zPontes, zErmidaBaixa, zGarganta, zGruta, zEscarpa, zIlhas, zNinho);

            // ---------------- Settlements ----------------
            // The Vale dos Sátiros first: the region's return city and the barca's stop for 53 (flat ground at y = 0, ±30 m). Then
            // Farnkesh (54), return city of 54–58, and the posts of 55, 56, 57 and 58.
            w.Vila = Place(w, vale, ValeDosSatirosRecipe(), ValeSatirosOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), FarnkeshCityGround, true);
            Place(w, farn, FarnkeshRecipe(), FarnkeshOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), FarnkeshCityGround, true);
            Place(w, pontes, Post("ermida_baixa", TownSwatch.LimeWhite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Marcenaria, false),
                ErmidaBaixaOffset, PostSafe, 18f, false);
            Place(w, ermidas, Post("pinaculo_do_sino", TownSwatch.LimeWhite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Lamparinas, false),
                PinaculoDoSinoOffset, PostSafe, 18f, false);
            Place(w, garganta, Post("gruta_abrigo", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Mercearia, false),
                GrutaAbrigoOffset, PostSafe, 18f, false);
            Place(w, ninho, Post("ultimo_pinaculo", TownSwatch.LimeWhite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Storyteller, false),
                UltimoPinaculoOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Pactolo (12 m, carved but shallow over its golden pebbles): a wide ford on the vale's east road, a stone bridge under
            // the sea of clouds; its cloud lake.
            MapRiver river = River(w, 12f, PactoloRiver);
            Vector2 valeFord = On(PactoloRiver[4], PactoloRiver[5], 0.875f);
            Crossing(river, valeFord, 9f, false);
            Vector2 cloudBridge = On(PactoloRiver[1], PactoloRiver[2], 0.5f);
            Crossing(river, cloudBridge, 4.5f, true);
            River(w, 50f, new Vector2(300f, 720f), new Vector2(330f, 760f));

            // ---------------- Gates out of the region ----------------
            // South: two passes down to the Estepe dos Nartes (by the meltwater lake, map 50, and by Ohya's crest, map 52).
            Gate(w, "exit_degelo", MapExitKind.Map, HaripushpaMapId, Rect.MinMaxRect(-308f, -600f, -292f, -597f), new Vector2(-300f, -591f), Vector2.up);
            Gate(w, "exit_ohya", MapExitKind.Map, AmiraniMapId, Rect.MinMaxRect(952f, -600f, 968f, -597f), new Vector2(960f, -591f), Vector2.up);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = FarnkeshCityGround;
            // The south-west road from the vale through Gordias's vines to the meltwater pass.
            Vector2 valeWest = new Vector2(-90f, -20f);
            Vector2 westFork = new Vector2(-200f, -160f);
            PathVia(w, 4f, new Vector2(-g, 0f), valeWest, westFork, new Vector2(-260f, -380f), new Vector2(-300f, -591f));
            // The west road through the dance glades and the chestnut wood up the pinnacle wood to the Pináculo do Sino; a loop into the foot woods.
            Vector2 woodsFork = new Vector2(-600f, 120f);
            PathVia(w, 3.5f, valeWest, new Vector2(-300f, 60f), woodsFork, new Vector2(-880f, 260f), new Vector2(-900f, 560f), new Vector2(-700f, 860f),
                PinaculoDoSinoOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, woodsFork, new Vector2(-760f, -200f), new Vector2(-900f, -420f));
            PathVia(w, 3.5f, westFork, new Vector2(-420f, -260f), new Vector2(-560f, -440f));
            // The east road over the Pactolo's ford through the vines of the sun to Ohya's pass.
            Vector2 eastFork = new Vector2(420f, -120f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(90f, 0f), valeFord, new Vector2(220f, -10f), eastFork, new Vector2(640f, -300f), new Vector2(860f, -480f),
                new Vector2(960f, -591f));
            // From the east road north along the Pactolo past the sea of clouds to the wind bridges and the Ermida Baixa; a branch to the bells.
            Vector2 riverFork = new Vector2(500f, 140f);
            Vector2 bridgesFork = new Vector2(700f, 1000f);
            PathVia(w, 3.5f, eastFork, riverFork, new Vector2(540f, 480f), new Vector2(600f, 800f), bridgesFork, new Vector2(860f, 1060f),
                ErmidaBaixaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, riverFork, new Vector2(780f, 200f), new Vector2(960f, 330f), new Vector2(1000f, 600f));
            // The north road up the stair of the baskets to Farnkesh; a branch over the stone bridge to the sea of clouds.
            Vector2 northFork = new Vector2(-10f, 420f);
            PathVia(w, 4f, new Vector2(0f, g), new Vector2(-10f, 200f), northFork, new Vector2(-30f, 640f), new Vector2(10f, 860f), FarnkeshOffset + new Vector2(0f, -g));
            PathVia(w, 3.5f, northFork, new Vector2(120f, 470f), cloudBridge, new Vector2(400f, 470f), new Vector2(540f, 480f));
            // From the climb (the north road) west over the meadow to the west road.
            PathVia(w, 3.5f, new Vector2(-30f, 640f), new Vector2(-300f, 600f), new Vector2(-560f, 520f), new Vector2(-900f, 560f));
            // Farnkesh east to the wind bridges, west to the Pináculo do Sino.
            PathVia(w, 3.5f, FarnkeshOffset + new Vector2(g, 0f), new Vector2(260f, 1100f), new Vector2(500f, 1060f), bridgesFork);
            PathVia(w, 3.5f, FarnkeshOffset + new Vector2(-g, 0f), new Vector2(-260f, 1080f), PinaculoDoSinoOffset + new Vector2(18f, 0f));
            // From Farnkesh's east road (the city has no north street) up the scarp to the Último Pináculo and the Gruta Abrigo; the
            // Sino's road up to the Gruta.
            Vector2 summitFork = new Vector2(40f, 1420f);
            PathVia(w, 3.5f, new Vector2(260f, 1100f), new Vector2(150f, 1300f), summitFork, new Vector2(300f, 1520f), UltimoPinaculoOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, summitFork, new Vector2(-200f, 1500f), GrutaAbrigoOffset + new Vector2(18f, 0f));
            PathVia(w, 3.5f, PinaculoDoSinoOffset + new Vector2(0f, 18f), new Vector2(-470f, 1300f), GrutaAbrigoOffset + new Vector2(0f, -18f));
            // Into the Gorge of the Ays; out to Huma's nest; from the Ermida Baixa up towards the nest.
            PathVia(w, 3.5f, GrutaAbrigoOffset + new Vector2(-18f, 0f), new Vector2(-760f, 1600f), new Vector2(-1000f, 1640f));
            PathVia(w, 3.5f, UltimoPinaculoOffset + new Vector2(18f, 0f), new Vector2(720f, 1600f), new Vector2(950f, 1640f));
            PathVia(w, 3.5f, ErmidaBaixaOffset + new Vector2(0f, 18f), new Vector2(980f, 1320f), new Vector2(960f, 1480f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-420f, -260f), 4f, 0f);
            Sign(w, valeWest, new Vector2(-300f, 60f), 4f, 0f);
            Sign(w, woodsFork, new Vector2(-760f, -200f), 3.5f, 0f);
            Sign(w, eastFork, riverFork, 4f, 0f);
            Sign(w, riverFork, new Vector2(780f, 200f), 3.5f, 0f);
            Sign(w, northFork, new Vector2(120f, 470f), 4f, 0f);
            Sign(w, bridgesFork, new Vector2(860f, 1060f), 3.5f, 0f);
            Sign(w, summitFork, new Vector2(-200f, 1500f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -460f, -40f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 640f, -200f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, 600f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 700f, 640f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -620f, 1460f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 160f, 1380f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -420f, 200f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 880f, 1500f, 0f, 0.9f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            TrimSteepClearings(w);
            return w;
        }

        // ------------------------------------------------------------------
        // The two towns of the peaks (their own recipes, Cities/Town: the Glade style of the Vale dos Sátiros and the PinnacleNest
        // style of Farnkesh, each inside a radius of 22.5 m on its reserved flat ground of ±30 m at y = 0; no backdrop: the peaks
        // are all round them). The vale's streets leave W, N and E; Farnkesh's W, E and S.
        // ------------------------------------------------------------------

        private static CityTownRecipe ValeDosSatirosRecipe() => NoBackdrop(CityTownRecipes.ValeDosSatiros());

        private static CityTownRecipe FarnkeshRecipe() => NoBackdrop(CityTownRecipes.Farnkesh());

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 53–58 archetypes)
        // ------------------------------------------------------------------

        private static void AddFarnkeshFauna(OverworldZone pe, OverworldZone gordias, OverworldZone vau, OverworldZone sol, OverworldZone pradosVento,
            OverworldZone castanheiras, OverworldZone baile, OverworldZone vale, OverworldZone pactolo, OverworldZone sinos, OverworldZone bosquePin,
            OverworldZone subida, OverworldZone escadaria, OverworldZone nuvens, OverworldZone leste, OverworldZone ermidas, OverworldZone sino,
            OverworldZone farnkesh, OverworldZone pontes, OverworldZone ermidaBaixa, OverworldZone garganta, OverworldZone gruta, OverworldZone escarpa,
            OverworldZone ilhas, OverworldZone ninho)
        {
            // At night: the smoke goats sleep (the vale's night is the satyrs' ball: fireflies over the vines, the moon-mother by the
            // Pactolo), the shroud owl over the meadows, the vampire bats round the hermitages and the bridges, the werewolf in the
            // woods and the gorge.
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies fireflies = NightVariantRules.VagaLume;

            // Vale dos Sátiros, Clareiras do Baile, Vinhas, Vau and Rio Pactolo (53): the Bode de Fumaça [tanque], the
            // Abelha Dourada [enxame], the Pássaro-Flauta Apagado [distância], the Raposa da Vinha [rápido];
            // the thief monkey in the vines.
            vale.Fauna.Add(Fauna("abelhas_douradas", "[enxame]", T(N(Fark.Bee, 3), N(Fark.VineFox, 1)), T(V(Fark.Bee, 2), S(fireflies, 1)), 5));
            vale.Fauna.Add(Fauna("raposas_da_vinha", "[rápido]", T(N(Fark.VineFox, 3), N(Fark.FluteBird, 1)), T(V(Fark.VineFox, 2), S(fireflies, 1)), 4));
            baile.Fauna.Add(Fauna("passaros_flauta", "[distância]", T(N(Fark.FluteBird, 3), N(Fark.Bee, 1)), T(V(Fark.FluteBird, 2), S(fireflies, 1)), 3));
            baile.Fauna.Add(Fauna("bodes_de_fumaca", "[tanque]", T(N(Fark.SmokeGoat, 3), N(Fark.VineFox, 1)), T(V(Fark.VineFox, 2), S(fireflies, 1)), 3, null, 28f));
            gordias.Fauna.Add(Fauna("raposas_de_gordias", "[rápido]", T(N(Fark.VineFox, 3), N(Fark.Monkey, 1)), T(V(Fark.VineFox, 2), S(moon, 1)), 4));
            gordias.Fauna.Add(Fauna("macacos_das_vinhas", "[rápido]", T(N(Fark.Monkey, 3), N(Fark.Bee, 1)), T(V(Fark.Monkey, 2), S(fireflies, 1)), 4));
            sol.Fauna.Add(Fauna("abelhas_do_sol", "[enxame]", T(N(Fark.Bee, 3), N(Fark.Monkey, 1)), T(V(Fark.Bee, 2), S(fireflies, 1)), 5));
            sol.Fauna.Add(Fauna("bodes_das_vinhas", "[tanque]", T(N(Fark.SmokeGoat, 3), N(Fark.Bee, 1)), T(V(Fark.VineFox, 2), S(owl, 1)), 3, null, 28f));
            vau.Fauna.Add(Fauna("raposas_do_vau", "[rápido]", T(N(Fark.VineFox, 3), N(Fark.FluteBird, 1)), T(V(Fark.VineFox, 2), S(moon, 1)), 4));
            vau.Fauna.Add(Fauna("passaros_do_vau", "[distância]", T(N(Fark.FluteBird, 3), N(Fark.VineFox, 1)), T(V(Fark.FluteBird, 2), S(moon, 1)), 3));
            pactolo.Fauna.Add(Fauna("abelhas_do_pactolo", "[enxame]", T(N(Fark.Bee, 3), N(Fark.FluteBird, 1)), T(V(Fark.Bee, 2), S(moon, 1)), 5));
            pactolo.Fauna.Add(Fauna("bodes_do_pactolo", "[tanque]", T(N(Fark.SmokeGoat, 3), N(Fark.VineFox, 1)), T(V(Fark.VineFox, 2), S(moon, 1)), 3, null, 28f));

            // The foot woods: the brown bear, spiders, the thief monkey; the werewolf at night.
            pe.Fauna.Add(Fauna("ursos_do_pe_dos_picos", "[tanque]", T(N(Fark.Bear, 3), N(Fark.Spider, 1)), T(V(Fark.Bear, 2), S(werewolf, 1)), 3, null, 30f));
            pe.Fauna.Add(Fauna("aranhas_do_pe_dos_picos", "[controle]", T(N(Fark.Spider, 3), N(Fark.Monkey, 1)), T(V(Fark.Spider, 2), S(moon, 1)), 4));
            castanheiras.Fauna.Add(Fauna("macacos_das_castanheiras", "[rápido]", T(N(Fark.Monkey, 3), N(Fark.Spider, 1)), T(V(Fark.Monkey, 2), S(owl, 1)), 4));
            castanheiras.Fauna.Add(Fauna("ursos_das_castanheiras", "[tanque]", T(N(Fark.Bear, 3), N(Fark.VineFox, 1)), T(V(Fark.Bear, 2), S(werewolf, 1)), 3, null, 30f));

            // Prados do Vento, Prados dos Sinos, Subida, Escadaria, Mar de Nuvens (54, outside the city): the Gralha-Alpina [rápido],
            // the Takin Apagado [tanque], the Abutre do Pico [distância] (the ember vulture), the
            // Cabra-Azul [rápido]; the giant marmot in the meadows.
            pradosVento.Fauna.Add(Fauna("marmotas_do_vento", "[tanque]", T(N(Fark.Marmot, 3), N(Fark.Chough, 1)), T(V(Fark.Marmot, 2), S(owl, 1)), 3, null, 28f));
            pradosVento.Fauna.Add(Fauna("gralhas_do_vento", "[rápido]", T(N(Fark.Chough, 3), N(Fark.Marmot, 1)), T(V(Fark.Chough, 2), S(owl, 1)), 4));
            sinos.Fauna.Add(Fauna("cabras_azuis", "[rápido]", T(N(Fark.BlueSheep, 3), N(Fark.Chough, 1)), T(V(Fark.BlueSheep, 2), S(owl, 1)), 4));
            sinos.Fauna.Add(Fauna("takins_dos_sinos", "[tanque]", T(N(Fark.Takin, 3), N(Fark.Marmot, 1)), T(V(Fark.Takin, 2), S(owl, 1)), 3, null, 28f));
            subida.Fauna.Add(Fauna("gralhas_alpinas", "[rápido]", T(N(Fark.Chough, 3), N(Fark.BlueSheep, 1)), T(V(Fark.Chough, 2), S(owl, 1)), 4));
            subida.Fauna.Add(Fauna("marmotas_da_subida", "[tanque]", T(N(Fark.Marmot, 3), N(Fark.BlueSheep, 1)), T(V(Fark.Marmot, 2), S(owl, 1)), 3, null, 28f));
            escadaria.Fauna.Add(Fauna("takins_apagados", "[tanque]", T(N(Fark.Takin, 3), N(Fark.Chough, 1)), T(V(Fark.Takin, 2), S(bats, 1)), 3, null, 28f));
            escadaria.Fauna.Add(Fauna("abutres_do_pico", "[distância]", T(N(Fark.Vulture, 3), N(Fark.Takin, 1)), T(V(Fark.Vulture, 2), S(bats, 1)), 3));
            nuvens.Fauna.Add(Fauna("abutres_das_nuvens", "[distância]", T(N(Fark.Vulture, 3), N(Fark.Mist, 1)), T(V(Fark.Vulture, 2), S(moon, 1)), 3));
            nuvens.Fauna.Add(Fauna("neblinas_das_nuvens", "[controle]", T(N(Fark.Mist, 3), N(Fark.Chough, 1)), T(V(Fark.Mist, 2), S(moon, 1)), 4));
            farnkesh.Fauna.Add(Fauna("gralhas_de_farnkesh", "[rápido]", T(N(Fark.Chough, 3), N(Fark.Vulture, 1)), T(V(Fark.Chough, 2), S(bats, 1)), 4));
            farnkesh.Fauna.Add(Fauna("cabras_dos_ninhos", "[rápido]", T(N(Fark.BlueSheep, 3), N(Fark.Takin, 1)), T(V(Fark.BlueSheep, 2), S(owl, 1)), 4));

            // Bosque de Pináculos and Pináculos do Leste: vultures and choughs in the towers, the Pallas cat.
            bosquePin.Fauna.Add(Fauna("gatos_dos_pinaculos", "[rápido]", T(N(Fark.Cat, 3), N(Fark.Chough, 1)), T(V(Fark.Cat, 2), S(werewolf, 1)), 4));
            bosquePin.Fauna.Add(Fauna("abutres_dos_pinaculos", "[distância]", T(N(Fark.Vulture, 3), N(Fark.Cat, 1)), T(V(Fark.Vulture, 2), S(bats, 1)), 3));
            leste.Fauna.Add(Fauna("morcegos_vela_do_leste", "[rápido]", T(N(Fark.CandleBat, 3), N(Fark.Vulture, 1)), T(V(Fark.CandleBat, 3), S(bats, 1)), 4));
            leste.Fauna.Add(Fauna("pipas_sombra_do_leste", "[distância]", T(N(Fark.Kite, 3), N(Fark.CandleBat, 1)), T(V(Fark.Kite, 2), S(bats, 1)), 3));

            // Pontes de Vento and Ermida Baixa (55): the Morcego-Vela [rápido], the Pipa-Sombra [distância] (the
            // broken lantern), the Neblina do Vento Ruim [controle] (the ill-wind mist), the Ushabti de Corda [tanque]; the
            // candle bats double at night.
            pontes.Fauna.Add(Fauna("morcegos_vela", "[rápido]", T(N(Fark.CandleBat, 3), N(Fark.Kite, 1)), T(V(Fark.CandleBat, 3), S(bats, 2)), 4));
            pontes.Fauna.Add(Fauna("neblinas_do_vento_ruim", "[controle]", T(N(Fark.Mist, 3), N(Fark.Kite, 1)), T(V(Fark.Mist, 2), S(bats, 1)), 4));
            pontes.Fauna.Add(Fauna("ushabtis_de_corda", "[tanque]", T(N(Fark.Ushabti, 3), N(Fark.Mist, 1)), T(V(Fark.Ushabti, 2), S(bats, 1)), 3, null, 30f));
            ermidaBaixa.Fauna.Add(Fauna("pipas_sombra", "[distância]", T(N(Fark.Kite, 3), N(Fark.Ushabti, 1)), T(V(Fark.Kite, 2), S(bats, 2)), 3));
            ermidaBaixa.Fauna.Add(Fauna("ushabtis_da_ermida", "[tanque]", T(N(Fark.Ushabti, 3), N(Fark.CandleBat, 1)), T(V(Fark.Ushabti, 2), S(bats, 1)), 3, null, 30f));

            // Ermidas Suspensas and Pináculo do Sino (56): the Eco Apagado [controle] (the ill-wind mist), the Gato-de-Pallas Apagado
            // [rápido], the Sino Rachado [distância] (the broken lantern), the Gralha [enxame] (a flock of alpine choughs).
            ermidas.Fauna.Add(Fauna("ecos_apagados", "[controle]", T(N(Fark.Mist, 3), N(Fark.Cat, 1)), T(V(Fark.Mist, 3), S(owl, 1)), 4));
            ermidas.Fauna.Add(Fauna("sinos_rachados", "[distância]", T(N(Fark.Kite, 3), N(Fark.Mist, 1)), T(V(Fark.Kite, 2), S(bats, 1)), 3));
            sino.Fauna.Add(Fauna("gatos_de_pallas", "[rápido]", T(N(Fark.Cat, 3), N(Fark.Kite, 1)), T(V(Fark.Cat, 2), S(werewolf, 1)), 4));
            sino.Fauna.Add(Fauna("gralhas_das_ermidas", "[enxame]", T(N(Fark.Chough, 3), N(Fark.Cat, 1)), T(V(Fark.Chough, 2), S(bats, 2)), 5));

            // Garganta dos Ays, Gruta Abrigo, Escarpa dos Ventos (57): the Ays [rápido] (the ill-wind mist, seen in the wind's dust),
            // the Redemoinho de Pó [controle] (the mist), the Águia da Tempestade [distância], the Pedra Voadora [tanque]
            // (the rolling stone).
            garganta.Fauna.Add(Fauna("ays_do_vento", "[rápido]", T(N(Fark.Mist, 3), N(Fark.StormEagle, 1)), T(V(Fark.Mist, 3), S(werewolf, 1)), 4));
            garganta.Fauna.Add(Fauna("pedras_voadoras", "[tanque]", T(N(Fark.Stone, 3), N(Fark.Mist, 1)), T(V(Fark.Stone, 2), S(bats, 1)), 3, null, 28f));
            gruta.Fauna.Add(Fauna("aguias_da_tempestade", "[distância]", T(N(Fark.StormEagle, 3), N(Fark.Stone, 1)), T(V(Fark.StormEagle, 2), S(bats, 1)), 3));
            gruta.Fauna.Add(Fauna("redemoinhos_de_po", "[controle]", T(N(Fark.Mist, 3), N(Fark.Stone, 1)), T(V(Fark.Mist, 2), S(werewolf, 1)), 4));
            escarpa.Fauna.Add(Fauna("pedras_da_escarpa", "[tanque]", T(N(Fark.Stone, 3), N(Fark.StormEagle, 1)), T(V(Fark.Stone, 2), S(owl, 1)), 3, null, 28f));
            escarpa.Fauna.Add(Fauna("gatos_da_escarpa", "[rápido]", T(N(Fark.Cat, 3), N(Fark.Stone, 1)), T(V(Fark.Cat, 2), S(werewolf, 1)), 4));

            // Ilhas de Nuvem and Ninho da Huma (58): the Ventania [controle] (the mist), the Harpia de Fumaça [rápido],
            // the Grifo Selvagem [elite] (a young griffin), the Raio Preso [distância] (the broken lantern).
            ilhas.Fauna.Add(Fauna("ventanias", "[controle]", T(N(Fark.Mist, 3), N(Fark.Harpy, 1)), T(V(Fark.Mist, 3), S(moon, 1)), 4));
            ilhas.Fauna.Add(Fauna("harpias_de_fumaca", "[rápido]", T(N(Fark.Harpy, 3), N(Fark.Vulture, 1)), T(V(Fark.Harpy, 3), S(bats, 1)), 4));
            ninho.Fauna.Add(Fauna("grifos_selvagens", "[elite]", T(N(Fark.Griffin, 3), N(Fark.Kite, 1)), T(V(Fark.Griffin, 2), S(owl, 1)), 3, null, 32f));
            ninho.Fauna.Add(Fauna("raios_presos", "[distância]", T(N(Fark.Kite, 3), N(Fark.Mist, 1)), T(V(Fark.Kite, 2), S(moon, 1)), 3));
        }
    }
}
