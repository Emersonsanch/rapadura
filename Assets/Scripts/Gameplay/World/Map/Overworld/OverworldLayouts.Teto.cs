using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 7 — Teto do Mundo (the Pamir) as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps
    /// 36–40), no scene change between Sangkent and the mountains round it. Sangkent (36, the stair city, the region's return
    /// city) stands at the plaza (0, 0) in its terraced valley, 600 m from the south edge where the two passes from the Serra
    /// de Kaggen come in (the Teto do Mundo lies north of the Serra in the stitched world, its high ground going on):
    /// <list type="bullet">
    /// <item>round the town (nv 70–75): the <b>Terraços de Sangkent</b> (36), the <b>Vale dos Damasqueiros</b>, the
    /// <b>Choupal dos Canais</b>, the <b>Estepe Alta</b>, the <b>Rio de Degelo</b>; above the town the <b>Terraços Altos</b> and
    /// the <b>Campos de Neve de Sangkent</b> (36: Thermal Clothes);</item>
    /// <item>the middle ring (nv 74–84): the <b>Calhas de Pedra</b>, the <b>Estação da Calha Alta</b> (post) and the <b>Calhas
    /// Geladas</b> (37), the <b>Encosta dos Zimbros</b>, the <b>Pastagens dos Iaques</b>, the <b>Lago Turquesa</b>, the
    /// <b>Morenas da Geleira</b>, the <b>Torres de Cata-Vento</b> and the <b>Vale do Wakhan</b>;</item>
    /// <item>the far north (nv 80–98): the <b>Geleira do Teto do Mundo</b> with the Cabana do Iaque (post) and the <b>Campo de
    /// Seracs</b> (38: Climbing Crampons), the <b>Desfiladeiro</b> and the <b>Forte dos Ventos</b> (39, the fort is the post;
    /// wings), the <b>Alto do Wakhan</b>, the <b>Passo de Wakhan</b> with the Caravançará de Pedra (post) and the <b>Ninho de
    /// Aydahar</b> (40: Storm Cloak), the <b>Picos Gelados</b> and the <b>Cristas do Vento Ruim</b>.</item>
    /// </list>
    /// Water: the Rio do Wakhan comes down from the pass along the caravan road, past the chutes east of the town, to a
    /// meltwater pond in the south (carved, crossed at stone bridges); the Lago Turquesa and tarns. Monsters: existing beasts
    /// that fit the sheets (the werewolf's body for the brown bear, crows for the choughs, the jackal for the giant marmots, the
    /// rolling stone, the stone golem for the ushabti of the chutes and the fort, the ember vulture for the lammergeier, the
    /// ill-wind mist, the peccary for the wild yak and the argali, the baboon for the almas, the wolf for the ice wolf and the
    /// irbis, the ooze for the living crevasse, broken lanterns for the smoke kites, the hawk, bats for the wind bats, the
    /// crowned orc for Āz's bandits) and the night species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string TetoId = "overworld_teto";

        public const string SangkentMapId = "map_036_sangkent";
        public const string CalhasId = "map_037_calhas";
        public const string GeleiraId = "map_038_geleira";
        public const string ForteVentosId = "map_039_forte_ventos";
        public const string WakhanId = "map_040_wakhan";

        public static readonly Vector2 SangkentOffset = Vector2.zero;
        public static readonly Vector2 CalhaAltaOffset = new Vector2(960f, 100f);
        public static readonly Vector2 CabanaIaqueOffset = new Vector2(-480f, 1080f);
        public static readonly Vector2 ForteVentosOffset = new Vector2(960f, 1080f);
        public static readonly Vector2 CaravancaraPedraOffset = new Vector2(0f, 1560f);

        /// <summary>Aydahar's arena (40): the stone ring at the top of the trail from the caravanserai to the dragon's nest.</summary>
        public static readonly Vector2 AydaharArena = new Vector2(560f, 1640f);

        /// <summary>O Carrinho Desgovernado's arena (37, map boss): the stone ring on the shelf of the Encosta dos Zimbros, by its trail, far from the cart course.</summary>
        public static readonly Vector2 CarrinhoDesgovernadoArena = new Vector2(997f, -185f);

        /// <summary>O Grande Almas's arena (38, map boss): the stone ring on the shore trail of the Lago Turquesa, below the glacier.</summary>
        public static readonly Vector2 GrandeAlmasArena = new Vector2(-797f, 205f);

        /// <summary>Half the side (m) of Sangkent's flat ground (reserved for the stair city's recipe, ±28 m and more).</summary>
        public const float SangkentGround = 30f;

        /// <summary>The Rio do Wakhan: from the pass down the caravan valley, past the chutes east of Sangkent, to its pond.</summary>
        public static readonly Vector2[] WakhanRiver =
        {
            new Vector2(120f, 1790f), new Vector2(110f, 1500f), new Vector2(190f, 1200f), new Vector2(160f, 950f), new Vector2(260f, 760f),
            new Vector2(380f, 560f), new Vector2(440f, 330f), new Vector2(500f, 100f), new Vector2(440f, -150f), new Vector2(520f, -400f),
            new Vector2(560f, -470f),
        };

        private static class Teto
        {
            public const string Bear = "enemy_mountain_brown_bear";
            public const string Chough = "enemy_cave_dark_crow";
            public const string Marmot = "enemy_mountain_giant_marmot";
            public const string RollingStone = "enemy_mountain_rolling_stone";
            public const string Ushabti = PeriodSpawnTable.MountainGolem;
            public const string Vulture = "enemy_desert_ember_vulture";
            public const string Mist = "enemy_mountain_ill_wind_mist";
            public const string Yak = "enemy_mountain_wild_yak";
            public const string Almas = "enemy_mountain_almas";
            public const string Wolf = PeriodSpawnTable.ForestWolf;
            public const string Irbis = "enemy_mountain_irbis";
            public const string Crevasse = "enemy_cave_ooze";
            public const string Kite = "enemy_desert_broken_lantern";
            public const string Hawk = "enemy_mountain_crested_hawk";
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Bandit = "enemy_desert_crowned_orc";
            public const string Potoo = NightVariantRules.PotooEnemyId;
        }

        private static OverworldLayout TetoRegion()
        {
            OverworldLayout w = NewRegion(TetoId, MapRegion.TetoDoMundo, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Mountain;
            const string sang = SangkentMapId;
            const string rock = MapRegionTypes.Mountain;
            const string forest = MapRegionTypes.Forest;
            const string plain = MapRegionTypes.Plain;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards the Serra de Kaggen): yak pastures, poplar channels, the apricot valley, the meltwater, junipers.
            OverworldZone zIaques = Zone(w, "pastagens_dos_iaques", null, sang, -1200f, -600f, -720f, -120f, OverworldTerrain.YakPasture, 75, 78, 73,
                "Yak Pastures", "Pastagens dos Iaques", rock, spacing: 90f);
            OverworldZone zChoupal = Zone(w, "choupal_dos_canais", null, sang, -720f, -600f, -240f, -120f, OverworldTerrain.PoplarCanals, 71, 74, 69,
                "Poplar Channels", "Choupal dos Canais", forest, spacing: 85f);
            OverworldZone zDamascos = Zone(w, "vale_dos_damasqueiros", null, sang, -240f, -600f, 240f, -120f, OverworldTerrain.ApricotValley, 70, 73, 68,
                "Apricot Valley", "Vale dos Damasqueiros", forest, spacing: 85f);
            OverworldZone zDegelo = Zone(w, "rio_de_degelo", null, CalhasId, 240f, -600f, 720f, -120f, OverworldTerrain.MeltRiver, 71, 74, 69,
                "Meltwater River", "Rio de Degelo", plain, spacing: 85f);
            OverworldZone zZimbros = Zone(w, "encosta_dos_zimbros", null, CalhasId, 720f, -600f, 1200f, -120f, OverworldTerrain.JuniperSlope, 75, 78, 73,
                "Juniper Slope", "Encosta dos Zimbros", rock, spacing: 90f);
            // Second row: the turquoise lake, the high steppe, Sangkent's terraces (36), the stone chutes and their station (37).
            OverworldZone zLago = Zone(w, "lago_turquesa", null, GeleiraId, -1200f, -120f, -720f, 360f, OverworldTerrain.TurquoiseLake, 75, 79, 73,
                "Turquoise Lake", "Lago Turquesa", plain, spacing: 90f);
            OverworldZone zEstepe = Zone(w, "estepe_alta", null, sang, -720f, -120f, -240f, 360f, OverworldTerrain.HighSteppe, 71, 74, 69,
                "High Steppe", "Estepe Alta", plain, spacing: 90f);
            OverworldZone zTerracos = Zone(w, "terracos_de_sangkent", sang, null, -240f, -120f, 240f, 360f, OverworldTerrain.StairTerraces, 70, 73, 68,
                "Terraces of Sangkent", "Terraços de Sangkent", rock, spacing: 80f);
            OverworldZone zCalhas = Zone(w, "calhas_de_pedra", CalhasId, null, 240f, -120f, 720f, 360f, OverworldTerrain.StoneChutes, 74, 78, 70,
                "Stone Chutes", "Calhas de Pedra", rock, spacing: 90f);
            OverworldZone zEstacao = Zone(w, "estacao_da_calha_alta", CalhasId, null, 720f, -120f, 1200f, 360f, OverworldTerrain.StoneChutes, 76, 80, 74,
                "High Chute Station", "Estação da Calha Alta", rock, spacing: 90f);
            // Middle row: the moraines, the snowfields and the high terraces above the town (36), the frozen chutes (37), the wind wheels.
            OverworldZone zMorenas = Zone(w, "morenas_da_geleira", null, GeleiraId, -1200f, 360f, -720f, 840f, OverworldTerrain.Moraine, 78, 82, 76,
                "Glacier Moraines", "Morenas da Geleira", rock, spacing: 90f);
            OverworldZone zNeves = Zone(w, "campos_de_neve_de_sangkent", sang, null, -720f, 360f, -240f, 840f, OverworldTerrain.SnowTerraces, 73, 76, 71,
                "Snowfields of Sangkent", "Campos de Neve de Sangkent", rock, MapRequirementTags.ThermalClothes, 90f);
            OverworldZone zAltos = Zone(w, "terracos_altos", sang, null, -240f, 360f, 240f, 840f, OverworldTerrain.SnowTerraces, 72, 76, 70,
                "High Terraces", "Terraços Altos", rock, MapRequirementTags.ThermalClothes, 90f);
            OverworldZone zGeladas = Zone(w, "calhas_geladas", CalhasId, null, 240f, 360f, 720f, 840f, OverworldTerrain.StoneChutes, 75, 80, 73,
                "Frozen Chutes", "Calhas Geladas", rock, spacing: 90f);
            OverworldZone zCataVento = Zone(w, "torres_de_cata_vento", null, ForteVentosId, 720f, 360f, 1200f, 840f, OverworldTerrain.WindRidge, 79, 83, 77,
                "Wind-Wheel Towers", "Torres de Cata-Vento", rock, spacing: 95f);
            // Fourth row: the seracs and the glacier (38), the Wakhan valley, the gorge and the Fort of the Winds (39).
            OverworldZone zSeracs = Zone(w, "campo_de_seracs", GeleiraId, null, -1200f, 840f, -720f, 1320f, OverworldTerrain.Seracs, 82, 86, 80,
                "Serac Field", "Campo de Seracs", rock, MapRequirementTags.ClimbingCrampons, 95f);
            OverworldZone zGeleira = Zone(w, "geleira_do_teto_do_mundo", GeleiraId, null, -720f, 840f, -240f, 1320f, OverworldTerrain.Glacier, 80, 84, 76,
                "Roof-of-the-World Glacier", "Geleira do Teto do Mundo", rock, MapRequirementTags.ClimbingCrampons, 95f);
            OverworldZone zVale = Zone(w, "vale_do_wakhan", null, WakhanId, -240f, 840f, 240f, 1320f, OverworldTerrain.WakhanValley, 80, 84, 78,
                "Wakhan Valley", "Vale do Wakhan", plain, spacing: 90f);
            OverworldZone zDesfiladeiro = Zone(w, "desfiladeiro_dos_ventos", ForteVentosId, null, 240f, 840f, 720f, 1320f, OverworldTerrain.FortRock, 84, 88, 80,
                "Gorge of the Winds", "Desfiladeiro dos Ventos", rock, MapRequirementTags.WingsRequired, 95f);
            OverworldZone zForte = Zone(w, "forte_dos_ventos", ForteVentosId, null, 720f, 840f, 1200f, 1320f, OverworldTerrain.FortRock, 86, 92, 84,
                "Fort of the Winds", "Forte dos Ventos", rock, MapRequirementTags.WingsRequired, 95f);
            // North row: the ice peaks, the high Wakhan and its pass (40), the dragon's nest, the crests of the ill wind.
            OverworldZone zPicos = Zone(w, "picos_gelados", null, GeleiraId, -1200f, 1320f, -720f, 1800f, OverworldTerrain.IcePeaks, 88, 94, 86,
                "Ice Peaks", "Picos Gelados", rock, spacing: 100f);
            OverworldZone zAltoWakhan = Zone(w, "alto_do_wakhan", WakhanId, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.WakhanPass, 90, 95, 86,
                "High Wakhan", "Alto do Wakhan", rock, MapRequirementTags.StormProtection, 95f);
            OverworldZone zPasso = Zone(w, "passo_de_wakhan", WakhanId, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.WakhanPass, 90, 96, 86,
                "Wakhan Pass", "Passo de Wakhan", rock, MapRequirementTags.StormProtection, 95f);
            OverworldZone zNinho = Zone(w, "ninho_de_aydahar", WakhanId, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.DragonCrags, 92, 98, 90,
                "Aydahar's Nest", "Ninho de Aydahar", rock, MapRequirementTags.StormProtection, 100f);
            OverworldZone zCristas = Zone(w, "cristas_do_vento_ruim", null, ForteVentosId, 720f, 1320f, 1200f, 1800f, OverworldTerrain.WindRidge, 90, 94, 88,
                "Crests of the Ill Wind", "Cristas do Vento Ruim", rock, spacing: 100f);

            AddTetoFauna(zIaques, zChoupal, zDamascos, zDegelo, zZimbros, zLago, zEstepe, zTerracos, zCalhas, zEstacao, zMorenas, zNeves, zAltos,
                zGeladas, zCataVento, zSeracs, zGeleira, zVale, zDesfiladeiro, zForte, zPicos, zAltoWakhan, zPasso, zNinho, zCristas);

            // ---------------- Settlements ----------------
            // Sangkent first: the region's return city and the barca's stop for 36 (its own recipe, on flat ground at y = 0, ±30 m;
            // its streets leave W, E and S, the terraces and chutes fill its north side).
            CityTownRecipe sangkent = CityTownRecipes.Sangkent();
            sangkent.Backdrop = false;
            w.Vila = Place(w, sang, sangkent, SangkentOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), SangkentGround, true);

            // The posts of 37–40 (the Fort of the Winds is the post of 39: a stone yard with slate roofs).
            Place(w, CalhasId, Post("estacao_calha_alta", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Marcenaria, false),
                CalhaAltaOffset, PostSafe, 18f, false);
            Place(w, GeleiraId, Post("cabana_do_iaque", TownSwatch.Stone, TownSwatch.Thatch, TownRoofKind.Gable, TownShopDressing.Herder, false),
                CabanaIaqueOffset, PostSafe, 18f, false);
            Place(w, ForteVentosId, Post("forte_dos_ventos", TownSwatch.Granite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Ferraria, false),
                ForteVentosOffset, PostSafe, 18f, false);
            Place(w, WakhanId, Post("caravancara_de_pedra", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Mercearia, false),
                CaravancaraPedraOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Rio do Wakhan: stone bridges on the caravanserai's east road, the gorge road, the town's east road.
            MapRiver wakhan = River(w, 10f, WakhanRiver);
            Vector2 passBridge = On(WakhanRiver[0], WakhanRiver[1], 0.7f);
            Crossing(wakhan, passBridge, 4.5f, true);
            Vector2 gorgeBridge = On(WakhanRiver[2], WakhanRiver[3], 0.5f);
            Crossing(wakhan, gorgeBridge, 4.5f, true);
            Vector2 eastBridge = On(WakhanRiver[6], WakhanRiver[7], 0.9f);
            Crossing(wakhan, eastBridge, 4.5f, true);
            Vector2 chuteBridge = On(WakhanRiver[4], WakhanRiver[5], 0.5f);
            Crossing(wakhan, chuteBridge, 4f, true);
            // Its meltwater pond in the south, the Lago Turquesa, the tarns of the Wakhan valley and of the moraines.
            River(w, 36f, new Vector2(540f, -470f), new Vector2(610f, -500f));
            River(w, 60f, new Vector2(-1030f, 140f), new Vector2(-900f, 180f));
            River(w, 44f, new Vector2(-1080f, 60f), new Vector2(-980f, 70f));
            River(w, 22f, new Vector2(-160f, 1100f), new Vector2(-120f, 1120f));
            River(w, 20f, new Vector2(-940f, 640f), new Vector2(-900f, 650f));
            River(w, 16f, new Vector2(-640f, -420f), new Vector2(-610f, -430f));

            // ---------------- Gates out of the region ----------------
            // South: the two passes down to the Serra de Kaggen (the drystone walls' road and the painted shelters' trail).
            Gate(w, "exit_pedra_seca", MapExitKind.Map, PedraSecaId, Rect.MinMaxRect(-158f, -600f, -142f, -597f), new Vector2(-150f, -591f), Vector2.up);
            Gate(w, "exit_abrigos", MapExitKind.Map, AbrigosId, Rect.MinMaxRect(1112f, -600f, 1128f, -597f), new Vector2(1120f, -591f), Vector2.up);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // The south road from the west pass up the apricot valley to the town.
            Vector2 southFork = new Vector2(-90f, -330f);
            PathVia(w, 4f, new Vector2(-150f, -591f), new Vector2(-140f, -460f), southFork, new Vector2(-30f, -180f), new Vector2(0f, -SangkentGround));
            // The east road over the Rio do Wakhan to the Estação da Calha Alta; from there down the junipers to the east pass.
            Vector2 chuteFork = new Vector2(650f, 120f);
            PathVia(w, 4f, new Vector2(SangkentGround, 0f), new Vector2(200f, 30f), new Vector2(350f, 90f), eastBridge, chuteFork, CalhaAltaOffset + new Vector2(-18f, 0f));
            PathVia(w, 4f, new Vector2(1120f, -591f), new Vector2(1090f, -420f), new Vector2(1000f, -200f), CalhaAltaOffset + new Vector2(0f, -18f));
            // The meltwater path from the apricot valley to the pond and the chutes.
            PathVia(w, 3.5f, southFork, new Vector2(150f, -300f), new Vector2(330f, -260f), new Vector2(350f, 90f));
            // The west road over the steppe to the turquoise lake, the yak pastures and the moraines.
            Vector2 westFork = new Vector2(-260f, -20f);
            Vector2 lakeFork = new Vector2(-760f, 20f);
            PathVia(w, 4f, new Vector2(-SangkentGround, 0f), westFork, new Vector2(-500f, 30f), lakeFork);
            PathVia(w, 3.5f, westFork, new Vector2(-420f, -260f), new Vector2(-560f, -480f));
            PathVia(w, 3.5f, lakeFork, new Vector2(-860f, -200f), new Vector2(-960f, -420f));
            PathVia(w, 3.5f, lakeFork, new Vector2(-820f, 330f), new Vector2(-800f, 620f), new Vector2(-640f, 900f), CabanaIaqueOffset + new Vector2(-18f, 0f));
            // The north road (from the east road: the town has no north street): the high terraces, the Wakhan valley, the caravanserai.
            Vector2 northFork = new Vector2(-40f, 760f);
            Vector2 valleyFork = new Vector2(30f, 1100f);
            PathVia(w, 4f, On(new Vector2(SangkentGround, 0f), new Vector2(200f, 30f), 0.7f), new Vector2(60f, 250f), new Vector2(20f, 500f), northFork, new Vector2(0f, 960f), valleyFork,
                new Vector2(40f, 1300f), CaravancaraPedraOffset + new Vector2(0f, -18f));
            // To the Cabana do Iaque on the glacier, on to the seracs and the ice peaks; north to the high Wakhan and the pass.
            PathVia(w, 3.5f, northFork, new Vector2(-250f, 820f), new Vector2(-400f, 960f), CabanaIaqueOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, CabanaIaqueOffset + new Vector2(0f, 18f), new Vector2(-520f, 1300f), new Vector2(-420f, 1520f), CaravancaraPedraOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, CabanaIaqueOffset + new Vector2(-18f, 0f), new Vector2(-760f, 1100f), new Vector2(-950f, 1150f), new Vector2(-980f, 1500f));
            // The caravanserai east over the river's head to the dragon's nest.
            PathVia(w, 3.5f, CaravancaraPedraOffset + new Vector2(18f, 0f), passBridge, new Vector2(320f, 1600f), new Vector2(560f, 1640f));
            // The chutes: from the east road up the frozen chutes to the wind wheels; the gorge road over the river to the valley.
            Vector2 wheelFork = new Vector2(940f, 640f);
            PathVia(w, 3.5f, chuteFork, new Vector2(630f, 420f), new Vector2(580f, 680f), new Vector2(760f, 640f), wheelFork);
            PathVia(w, 3.5f, CalhaAltaOffset + new Vector2(0f, 18f), new Vector2(920f, 380f), wheelFork, new Vector2(980f, 880f), ForteVentosOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, ForteVentosOffset + new Vector2(-18f, 0f), new Vector2(700f, 1080f), new Vector2(420f, 1060f), gorgeBridge, valleyFork);
            PathVia(w, 3.5f, ForteVentosOffset + new Vector2(0f, 18f), new Vector2(980f, 1300f), new Vector2(1000f, 1550f));
            // The chute bridge path: from the frozen chutes over the river to the high terraces.
            PathVia(w, 3f, new Vector2(580f, 680f), chuteBridge, new Vector2(200f, 620f), new Vector2(20f, 500f));

            // ---------------- Landmarks ----------------
            Sign(w, southFork, new Vector2(150f, -300f), 4f, 0f);
            Sign(w, westFork, new Vector2(-420f, -260f), 4f, 0f);
            Sign(w, lakeFork, new Vector2(-860f, -200f), 4f, 0f);
            Sign(w, northFork, new Vector2(-250f, 820f), 4f, 0f);
            Sign(w, valleyFork, new Vector2(420f, 1060f), 4f, 0f);
            Sign(w, chuteFork, new Vector2(630f, 420f), 4f, 0f);
            Sign(w, wheelFork, new Vector2(980f, 880f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -200f, -420f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -720f, 60f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 60f, 780f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 1060f, -300f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 70f, 1220f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, 1240f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -860f, 470f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, -150f, 1460f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, AydaharArena.x, AydaharArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, CarrinhoDesgovernadoArena.x, CarrinhoDesgovernadoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, GrandeAlmasArena.x, GrandeAlmasArena.y, 0f, 1.1f);

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
        // Monster groups per area (existing definitions only, no boss; Mapas100 36–40 archetypes)
        // ------------------------------------------------------------------

        private static void AddTetoFauna(OverworldZone iaques, OverworldZone choupal, OverworldZone damascos, OverworldZone degelo, OverworldZone zimbros,
            OverworldZone lago, OverworldZone estepe, OverworldZone terracos, OverworldZone calhas, OverworldZone estacao, OverworldZone morenas,
            OverworldZone neves, OverworldZone altos, OverworldZone geladas, OverworldZone cataVento, OverworldZone seracs, OverworldZone geleira,
            OverworldZone vale, OverworldZone desfiladeiro, OverworldZone forte, OverworldZone picos, OverworldZone altoWakhan, OverworldZone passo,
            OverworldZone ninho, OverworldZone cristas)
        {
            // The wolves turn werewolves at night (the ice wolves' night form), the shroud owl over the snow, the vampire bats
            // in the fort's towers, the potoo's moon-mother in the poplars, the yaks' night herd (the queixada).
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies herd = NightVariantRules.Queixada;

            // Terraços de Sangkent (36): the Urso-Pardo do Pamir [tanque] (the werewolf's body), the Gralha-de-Bico-Vermelho
            // [rápido] (crows), the Marmota Gigante [enxame] (jackals), the Pedra-Rolante [controle].
            terracos.Fauna.Add(Fauna("gralhas_de_bico_vermelho", "[rápido]", T(N(MidBeasts.RedBilledChough, 3), N(Teto.Marmot, 1)), T(V(MidBeasts.RedBilledChough, 2), S(owl, 1)), 4));
            terracos.Fauna.Add(Fauna("marmotas_gigantes", "[enxame]", T(N(Teto.Marmot, 3), N(Teto.Chough, 1)), T(V(Teto.Marmot, 2), S(moon, 1)), 5));
            terracos.Fauna.Add(Fauna("pedras_rolantes", "[controle]", T(N(Teto.RollingStone, 3), N(Teto.Marmot, 1)), T(V(Teto.RollingStone, 2), V(Teto.Marmot, 1)), 3, null, 28f));

            // Vale dos Damasqueiros and Choupal dos Canais: marmots under the apricots, choughs, the brown bear in the poplars.
            damascos.Fauna.Add(Fauna("marmotas_dos_pomares", "[enxame]", T(N(Teto.Marmot, 3), N(Teto.Chough, 1)), T(V(Teto.Marmot, 2), S(moon, 1)), 5));
            damascos.Fauna.Add(Fauna("gralhas_dos_pomares", "[rápido]", T(N(MidBeasts.RedBilledChough, 3), N(Teto.Hawk, 1)), T(V(MidBeasts.RedBilledChough, 2), S(owl, 1)), 4));
            choupal.Fauna.Add(Fauna("ursos_do_choupal", "[tanque]", T(N(Teto.Bear, 3), N(Teto.Marmot, 1)), T(V(Teto.Bear, 2), S(werewolf, 1)), 3, null, 30f));
            choupal.Fauna.Add(Fauna("gralhas_dos_canais", "[rápido]", T(N(MidBeasts.RedBilledChough, 3), N(Teto.Potoo, 1)), T(V(MidBeasts.RedBilledChough, 2), S(moon, 1)), 4));

            // Estepe Alta: marmot colonies, hawks, the yaks' cousins (the argali's body, the peccary).
            estepe.Fauna.Add(Fauna("colonias_de_marmotas", "[enxame]", T(N(Teto.Marmot, 3), N(Teto.Hawk, 1)), T(V(Teto.Marmot, 2), S(owl, 1)), 5));
            estepe.Fauna.Add(Fauna("gavioes_da_estepe", "[distância]", T(N(Teto.Hawk, 3), N(Teto.Chough, 1)), T(V(Teto.Hawk, 2), S(owl, 1)), 3));
            estepe.Fauna.Add(Fauna("argalis_da_estepe", "[tanque]", T(N(Teto.Yak, 3), N(Teto.Marmot, 1)), T(V(Teto.Yak, 2), S(herd, 1), V(Teto.Marmot, 1)), 3, null, 28f));

            // Rio de Degelo: the river's mists, the vultures over the gravel, rolling stones off the banks.
            degelo.Fauna.Add(Fauna("neblinas_do_degelo", "[controle]", T(N(Teto.Mist, 3), N(Teto.Chough, 1)), T(V(Teto.Mist, 2), S(owl, 1)), 4));
            degelo.Fauna.Add(Fauna("pedras_do_degelo", "[controle]", T(N(Teto.RollingStone, 3), N(Teto.Marmot, 1)), T(V(Teto.RollingStone, 2), V(Teto.Marmot, 1)), 3, null, 28f));

            // Encosta dos Zimbros and Pastagens dos Iaques: bears in the junipers, wild yaks [tanque], wolves.
            zimbros.Fauna.Add(Fauna("ursos_dos_zimbros", "[tanque]", T(N(Teto.Bear, 3), N(Teto.Chough, 1)), T(V(Teto.Bear, 2), S(werewolf, 1)), 3, null, 30f));
            zimbros.Fauna.Add(Fauna("lobos_dos_zimbros", "[rápido]", T(N(Teto.Wolf, 3), N(Teto.Marmot, 1)), T(V(Teto.Wolf, 2), S(werewolf, 1)), 4));
            iaques.Fauna.Add(Fauna("iaques_selvagens", "[tanque]", T(N(Teto.Yak, 3), N(Teto.Wolf, 1)), T(V(Teto.Yak, 2), S(herd, 1), V(Teto.Wolf, 1)), 3, null, 28f));
            iaques.Fauna.Add(Fauna("lobos_das_pastagens", "[rápido]", T(N(Teto.Wolf, 3), N(Teto.Marmot, 1)), T(V(Teto.Wolf, 2), S(werewolf, 1)), 4));

            // Lago Turquesa: mists on the water, vultures, marmots on the shore.
            lago.Fauna.Add(Fauna("neblinas_do_lago", "[controle]", T(N(Teto.Mist, 3), N(Teto.Hawk, 1)), T(V(Teto.Mist, 2), S(owl, 1)), 4));
            lago.Fauna.Add(Fauna("abutres_do_lago", "[distância]", T(N(MidBeasts.BeardedVulture, 3), N(Teto.Marmot, 1)), T(V(MidBeasts.BeardedVulture, 2), V(Teto.Marmot, 1)), 3));

            // Calhas de Pedra, Estação da Calha Alta and Calhas Geladas (37): the Ushabti de Calha [tanque] (the golem), the
            // Abutre-Barbudo Apagado [distância, drops stones] (the ember vulture), the Marmota Gigante, the Neblina do Vento Ruim.
            calhas.Fauna.Add(Fauna("ushabtis_de_calha", "[tanque]", T(N(Teto.Ushabti, 3), N(Teto.RollingStone, 1)), T(V(Teto.Ushabti, 2), V(Teto.RollingStone, 1)), 3, null, 30f));
            calhas.Fauna.Add(Fauna("abutres_barbudos", "[distância]", T(N(MidBeasts.BeardedVulture, 3), N(Teto.Chough, 1)), T(V(MidBeasts.BeardedVulture, 2), S(owl, 1)), 3));
            calhas.Fauna.Add(Fauna("marmotas_das_calhas", "[enxame]", T(N(Teto.Marmot, 3), N(Teto.RollingStone, 1)), T(V(Teto.Marmot, 2), S(moon, 1)), 5));
            estacao.Fauna.Add(Fauna("ushabtis_da_estacao", "[tanque]", T(N(Teto.Ushabti, 3), N(Teto.Mist, 1)), T(V(Teto.Ushabti, 2), V(Teto.Mist, 1)), 3, null, 30f));
            estacao.Fauna.Add(Fauna("neblinas_do_vento_ruim", "[controle]", T(N(Teto.Mist, 3), N(Teto.Vulture, 1)), T(V(Teto.Mist, 2), S(owl, 1)), 4));
            geladas.Fauna.Add(Fauna("abutres_das_calhas_geladas", "[distância]", T(N(MidBeasts.BeardedVulture, 3), N(Teto.Ushabti, 1)), T(V(MidBeasts.BeardedVulture, 2), V(Teto.Ushabti, 1)), 3));
            geladas.Fauna.Add(Fauna("pedras_das_calhas", "[controle]", T(N(Teto.RollingStone, 3), N(Teto.Mist, 1)), T(V(Teto.RollingStone, 2), S(owl, 1)), 3, null, 28f));

            // Campos de Neve and Terraços Altos (36, the cold): ice wolves, the bear, choughs in the snow.
            neves.Fauna.Add(Fauna("lobos_das_neves", "[rápido]", T(N(Teto.Wolf, 3), N(Teto.Chough, 1)), T(V(Teto.Wolf, 2), S(werewolf, 1)), 4));
            neves.Fauna.Add(Fauna("ursos_das_neves", "[tanque]", T(N(Teto.Bear, 3), N(Teto.Wolf, 1)), T(V(Teto.Bear, 2), S(werewolf, 1)), 3, null, 30f));
            altos.Fauna.Add(Fauna("gralhas_dos_terracos_altos", "[rápido]", T(N(MidBeasts.RedBilledChough, 3), N(Teto.RollingStone, 1)), T(V(MidBeasts.RedBilledChough, 2), S(owl, 1)), 4));
            altos.Fauna.Add(Fauna("pedras_dos_terracos", "[controle]", T(N(Teto.RollingStone, 3), N(Teto.Wolf, 1)), T(V(Teto.RollingStone, 2), V(Teto.Wolf, 1)), 3, null, 28f));

            // Morenas da Geleira: rolling stones on the ridges, wild yaks, wolves.
            morenas.Fauna.Add(Fauna("pedras_das_morenas", "[controle]", T(N(Teto.RollingStone, 3), N(Teto.Yak, 1)), T(V(Teto.RollingStone, 2), S(herd, 1)), 3, null, 28f));
            morenas.Fauna.Add(Fauna("lobos_das_morenas", "[rápido]", T(N(MidBeasts.IceWolf, 3), N(Teto.Chough, 1)), T(V(MidBeasts.IceWolf, 2), S(werewolf, 1)), 4));

            // Geleira do Teto do Mundo and Campo de Seracs (38): the Iaque Selvagem Apagado [tanque], the Almas [elite, shy,
            // snowballs] (the baboon; it comes out at night), the Lobo de Gelo [rápido], the Fenda Viva [controle] (the ooze).
            geleira.Fauna.Add(Fauna("iaques_da_geleira", "[tanque]", T(N(Teto.Yak, 3), N(Teto.Wolf, 1)), T(V(Teto.Yak, 2), S(herd, 1), V(Teto.Wolf, 1)), 3, null, 28f));
            geleira.Fauna.Add(Fauna("almas", "[elite]", T(N(Teto.Almas, 3), N(Teto.Yak, 1)), T(V(Teto.Almas, 3), S(owl, 1)), 3, null, 32f));
            geleira.Fauna.Add(Fauna("fendas_vivas", "[controle]", T(N(Teto.Crevasse, 3), N(Teto.Mist, 1)), T(V(Teto.Crevasse, 2), V(Teto.Mist, 1)), 4));
            seracs.Fauna.Add(Fauna("lobos_de_gelo", "[rápido]", T(N(MidBeasts.IceWolf, 3), N(Teto.Crevasse, 1)), T(V(MidBeasts.IceWolf, 2), S(werewolf, 1)), 4));
            seracs.Fauna.Add(Fauna("fendas_dos_seracs", "[controle]", T(N(Teto.Crevasse, 3), N(Teto.Almas, 1)), T(V(Teto.Crevasse, 2), V(Teto.Almas, 1)), 4));
            seracs.Fauna.Add(Fauna("almas_dos_seracs", "[elite]", T(N(Teto.Almas, 3), N(Teto.Wolf, 1)), T(V(Teto.Almas, 3), S(werewolf, 1)), 3, null, 32f));

            // Vale do Wakhan: argali on the gravel flats, vultures, choughs on the caravan road.
            vale.Fauna.Add(Fauna("argalis_do_vale", "[tanque]", T(N(Teto.Yak, 3), N(Teto.Chough, 1)), T(V(Teto.Yak, 2), S(herd, 1), V(Teto.Chough, 1)), 3, null, 28f));
            vale.Fauna.Add(Fauna("abutres_do_vale", "[distância]", T(N(MidBeasts.BeardedVulture, 3), N(Teto.Marmot, 1)), T(V(MidBeasts.BeardedVulture, 2), S(owl, 1)), 3));
            vale.Fauna.Add(Fauna("marmotas_do_vale", "[enxame]", T(N(Teto.Marmot, 3), N(Teto.Chough, 1)), T(V(Teto.Marmot, 2), V(Teto.Chough, 1)), 5));

            // Desfiladeiro and Forte dos Ventos (39): the Pipa de Fumaça [distância] (broken lanterns), the Gavião-Real Apagado
            // [rápido] (the hawk), the Guarda de Pedra Ushabti [tanque] (the golem), the Morcego do Vento [enxame] (bats).
            forte.Fauna.Add(Fauna("pipas_de_fumaca", "[distância]", T(N(Teto.Kite, 3), N(Teto.Hawk, 1)), T(V(Teto.Kite, 2), S(bats, 1)), 4));
            forte.Fauna.Add(Fauna("guardas_de_pedra", "[tanque]", T(N(Teto.Ushabti, 3), N(Teto.Kite, 1)), T(V(Teto.Ushabti, 2), V(Teto.Kite, 1)), 3, null, 30f));
            forte.Fauna.Add(Fauna("morcegos_do_vento", "[enxame]", T(N(Teto.Bats, 3), N(Teto.Hawk, 1)), T(V(Teto.Bats, 2), S(bats, 2)), 5));
            desfiladeiro.Fauna.Add(Fauna("gavioes_reais", "[rápido]", T(N(Teto.Hawk, 3), N(Teto.Bats, 1)), T(V(Teto.Hawk, 2), S(bats, 1)), 4));
            desfiladeiro.Fauna.Add(Fauna("pipas_do_desfiladeiro", "[distância]", T(N(Teto.Kite, 3), N(Teto.Ushabti, 1)), T(V(Teto.Kite, 2), V(Teto.Ushabti, 1)), 4));
            cataVento.Fauna.Add(Fauna("gavioes_dos_cata_ventos", "[rápido]", T(N(Teto.Hawk, 3), N(Teto.Kite, 1)), T(V(Teto.Hawk, 2), S(bats, 1)), 4));
            cataVento.Fauna.Add(Fauna("morcegos_das_torres", "[enxame]", T(N(Teto.Bats, 3), N(Teto.Mist, 1)), T(V(Teto.Bats, 2), S(bats, 2)), 5));
            cristas.Fauna.Add(Fauna("neblinas_das_cristas", "[controle]", T(N(Teto.Mist, 3), N(Teto.Hawk, 1)), T(V(Teto.Mist, 2), S(owl, 1)), 4));
            cristas.Fauna.Add(Fauna("guardas_das_cristas", "[tanque]", T(N(Teto.Ushabti, 3), N(Teto.Bats, 1)), T(V(Teto.Ushabti, 2), S(bats, 1)), 3, null, 30f));

            // Alto do Wakhan, Passo de Wakhan and Ninho de Aydahar (40): the Argali Apagado [tanque] (the peccary), the Irbis
            // Apagado [rápido] (the wolf's body), the Bandoleiro de Āz [elite] (the crowned orc), the Gralha das Neves [distância].
            passo.Fauna.Add(Fauna("argalis_apagados", "[tanque]", T(N(Teto.Yak, 3), N(Teto.Chough, 1)), T(V(Teto.Yak, 2), S(herd, 1), V(Teto.Chough, 1)), 3, null, 28f));
            passo.Fauna.Add(Fauna("bandoleiros_de_az", "[elite]", T(N(Teto.Bandit, 3), N(Teto.Wolf, 1)), T(V(Teto.Bandit, 2), S(werewolf, 1)), 3, null, 32f));
            passo.Fauna.Add(Fauna("gralhas_das_neves", "[distância]", T(N(Teto.Chough, 3), N(Teto.Vulture, 1)), T(V(Teto.Chough, 2), S(owl, 1)), 4));
            altoWakhan.Fauna.Add(Fauna("irbis_apagados", "[rápido]", T(N(Teto.Irbis, 3), N(Teto.Chough, 1)), T(V(Teto.Irbis, 2), S(werewolf, 1)), 4));
            altoWakhan.Fauna.Add(Fauna("bandoleiros_do_alto", "[elite]", T(N(Teto.Bandit, 3), N(Teto.Yak, 1)), T(V(Teto.Bandit, 2), S(herd, 1)), 3, null, 32f));
            ninho.Fauna.Add(Fauna("irbis_do_ninho", "[rápido]", T(N(Teto.Irbis, 3), N(Teto.Vulture, 1)), T(V(Teto.Irbis, 2), S(werewolf, 1)), 4));
            ninho.Fauna.Add(Fauna("abutres_do_ninho", "[distância]", T(N(MidBeasts.BeardedVulture, 3), N(Teto.Bandit, 1)), T(V(MidBeasts.BeardedVulture, 2), V(Teto.Bandit, 1)), 3));
            ninho.Fauna.Add(Fauna("guardas_do_ouro_de_az", "[elite] [tanque]", T(N(Teto.Bandit, 3), N(Teto.Ushabti, 1)), T(V(Teto.Bandit, 2), V(Teto.Ushabti, 1)), 3, null, 32f));
            picos.Fauna.Add(Fauna("almas_dos_picos", "[elite]", T(N(Teto.Almas, 3), N(Teto.Crevasse, 1)), T(V(Teto.Almas, 3), S(owl, 1)), 3, null, 32f));
            picos.Fauna.Add(Fauna("lobos_dos_picos", "[rápido]", T(N(MidBeasts.IceWolf, 3), N(Teto.Mist, 1)), T(V(MidBeasts.IceWolf, 2), S(werewolf, 1)), 4));
        }
    }
}
