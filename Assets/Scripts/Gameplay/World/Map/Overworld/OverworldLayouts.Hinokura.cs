using System.Runtime.CompilerServices;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 12 — Arquipélago de Hinokura as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 65–70), no
    /// scene change between the forge isle and the caldera. Hinokura (65, the capital of the Fogo Vivo, the first map of the
    /// region, level 220) stands at the plaza (0, 0) on its isle, 600 m from the south edge where two passes come up from the Teto do
    /// Mundo's northern heights and 720 m from the east edge where two passes come over the thaw strait from the Mar Gelado's tundra
    /// and lakes (in the stitched world the archipelago fills the last walled hole, north of the Teto and west of the frozen sea:
    /// the frozen sea thaws westwards over the volcanoes' fire and the Teto's glaciers run down to its black beaches); the islands
    /// rise westwards and northwards:
    /// <list type="bullet">
    /// <item>round the town (nv 220–227, map 65): <b>Hinokura</b>, the <b>Praia de Areia Negra</b> (the way in from the Teto), the
    /// <b>Costa das Fumarolas</b> (the second way in from the Teto), the <b>Estreito do Degelo</b> (the way in from the Mar Gelado),
    /// the <b>Canais de Lava</b> and the <b>Fontes Termais</b>;</item>
    /// <item>north of the town (nv 226–235, map 66): the <b>Jardins de Lava Laqueada</b> with the Casa de Chá de Pedra (post), the
    /// <b>Pinhais de Nuvem</b> and the <b>Pontes Vermelhas</b> (the second way in from the Mar Gelado);</item>
    /// <item>west, Mount Echeyde (nv 234–246, map 67: Climbing Crampons): the <b>Campos de Retama</b> with the Abrigo dos Pastores
    /// (post), the cone of <b>Monte Echeyde</b> and the <b>Malpaís de Echeyde</b>;</item>
    /// <item>the reef (nv 244–254, map 68: a boat or the quay): the <b>Canais Quentes</b>, the <b>Recife de Obsidiana</b> with the
    /// Barco-Farol (post) and the <b>Ilhotas de Vidro</b>;</item>
    /// <item>far west, the castle (nv 252–264, map 69): the <b>Muralhas Curvas</b>, the <b>Castelo das Nove Fornalhas</b> with the
    /// Portão Vermelho (post), the <b>Pátio das Fornalhas</b> and the <b>Escória Velha</b>;</item>
    /// <item>the north row, the Devouring Fire (nv 264–280, map 70: Fresh Water against the heat): the <b>Campos de Cinza</b>, the
    /// <b>Cinzas Vivas</b>, the <b>Mar de Cinza</b>, the <b>Borda Fria</b> (post), the <b>Encosta da Caldeira</b> and the
    /// <b>Caldeira do Fogo Devorador</b>.</item>
    /// </list>
    /// Lava (drawn glowing, <see cref="IsLava"/>): two tamed lava channels past the town (bridged on the west road), the lava ponds
    /// of the malpaís and the caldera's lake. Water: the hot springs, the thaw strait's open leads, the garden and red-bridge ponds,
    /// the two hot channels and the reef's pools. Monsters: the region's own beasts (the volcano crab, the lava lizard, the ember
    /// gull, the dimmed shisa, the thieving tanuki, the tibicena, the giant lava lizard, the blade fish, the ember coral, the lacquer
    /// ushabti, the furnace ogre, the slag giant), existing beasts that fit the other sheets (the sand snake for the devouring
    /// salamanders, the ember vulture for the volcanic bombs, the rolling stone for the flying pumice, the lantern fish for the ember
    /// carp, the ill-wind mist for the smoke bamboo, the fumaroles and the living bellows, the smoke goat for the shepherds' wild
    /// goats, the ooze for the obsidian octopus and the living ash, the stone turtle for the glass urchins, the firefly swarm for the
    /// angry sparks and the hungry flames, the nian cub in the castle) and the night species; no boss (Guayota, the Tartaruga-Vulcão, the Mestre das Nove Fornalhas and the Glutão de Brasa are not
    /// placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string HinokuraRegionId = "overworld_hinokura";

        public const string HinokuraMapId = "map_065_hinokura";
        public const string JardinsLavaMapId = "map_066_jardins_lava";
        public const string MonteEcheydeMapId = "map_067_monte_echeyde";
        public const string RecifeObsidianaMapId = "map_068_recife_obsidiana";
        public const string NoveFornalhasMapId = "map_069_nove_fornalhas";
        public const string CaldeiraMapId = "map_070_caldeira_fogo_devorador";

        public static readonly Vector2 HinokuraOffset = Vector2.zero;
        /// <summary>The Glutão de Brasa's arena (70): the cold stone north of the caldera's rim (flat ground, open from the Borda Fria).</summary>
        public static readonly Vector2 GlutaoArena = new Vector2(-1440f, 1690f);

        /// <summary>O Rei Shisa do Telhado's arena (66, map boss): the stone ring on the tea house's east road over the red bridges.</summary>
        public static readonly Vector2 ReiShisaArena = new Vector2(350f, 600f);

        /// <summary>O Mestre das Nove Fornalhas's arena (69, map boss): the stone ring on the road through the furnaces' court.</summary>
        public static readonly Vector2 MestreDasFornalhasArena = new Vector2(-1366f, 540f);
        public static readonly Vector2 CasaDeChaOffset = new Vector2(0f, 600f);
        public static readonly Vector2 AbrigoPastoresOffset = new Vector2(-960f, -380f);
        public static readonly Vector2 BarcoFarolOffset = new Vector2(0f, 1080f);
        public static readonly Vector2 PortaoVermelhoOffset = new Vector2(-1330f, 120f);
        public static readonly Vector2 BordaFriaOffset = new Vector2(-960f, 1560f);

        /// <summary>The centre of Mount Echeyde's cone (its crater), of the castle's curved walls and of the caldera (region coordinates).</summary>
        public static readonly Vector2 EcheydeCenter = new Vector2(-960f, 120f);

        public static readonly Vector2 CastleCenter = new Vector2(-1440f, 120f);

        public static readonly Vector2 CalderaCenter = new Vector2(-1440f, 1320f);

        /// <summary>Half the side (m) of the flat ground reserved for Hinokura's recipe (±30 m at y = 0).</summary>
        public const float HinokuraCityGround = 30f;

        /// <summary>The east lava channel: from the cone's foot south past the town's west road to the forges.</summary>
        public static readonly Vector2[] LavaCanalLeste =
        {
            new Vector2(-400f, 340f), new Vector2(-430f, 200f), new Vector2(-410f, 60f), new Vector2(-440f, -60f), new Vector2(-420f, -100f),
        };

        /// <summary>The west lava channel, beside the first one.</summary>
        public static readonly Vector2[] LavaCanalOeste =
        {
            new Vector2(-610f, 330f), new Vector2(-590f, 180f), new Vector2(-630f, 40f), new Vector2(-600f, -90f),
        };

        /// <summary>The two hot channels of the reef (steaming water).</summary>
        public static readonly Vector2[] CanalQuenteSul =
        {
            new Vector2(-700f, 920f), new Vector2(-560f, 960f), new Vector2(-420f, 930f), new Vector2(-260f, 980f),
        };

        public static readonly Vector2[] CanalQuenteNorte =
        {
            new Vector2(-700f, 1240f), new Vector2(-540f, 1200f), new Vector2(-400f, 1250f), new Vector2(-250f, 1210f),
        };

        /// <summary>Water pools (centre segment, width): the hot springs, the thaw strait's leads, the garden and red-bridge ponds, the reef's pools.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] HinokuraPools =
        {
            (new Vector2(-560f, -320f), new Vector2(-545f, -310f), 24f),
            (new Vector2(-380f, -430f), new Vector2(-370f, -420f), 18f),
            (new Vector2(-650f, -520f), new Vector2(-640f, -515f), 20f),
            (new Vector2(-280f, -470f), new Vector2(-272f, -465f), 16f),
            (new Vector2(520f, -40f), new Vector2(540f, -30f), 36f),
            (new Vector2(640f, 170f), new Vector2(650f, 180f), 28f),
            (new Vector2(-120f, 720f), new Vector2(-110f, 726f), 20f),
            (new Vector2(420f, 700f), new Vector2(440f, 710f), 30f),
            (new Vector2(600f, 760f), new Vector2(608f, 766f), 22f),
            (new Vector2(300f, 1250f), new Vector2(320f, 1260f), 40f),
            (new Vector2(560f, 1000f), new Vector2(570f, 1006f), 36f),
            (new Vector2(650f, 1220f), new Vector2(656f, 1226f), 30f),
        };

        /// <summary>Lava pools (centre segment, width): the malpaís' ponds and the caldera's lake (the last one).</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] LavaPools =
        {
            (new Vector2(-900f, 780f), new Vector2(-890f, 786f), 28f),
            (new Vector2(-1100f, 700f), new Vector2(-1094f, 704f), 22f),
            (new Vector2(-1470f, 1320f), new Vector2(-1410f, 1330f), 210f),
        };

        // ---------------- Places of the region's mechanics (Mechanics/Hinokura*Rules; region coordinates) ----------------

        /// <summary>The zone whose origin maps this region onto the stitched world.</summary>
        public const string HinokuraAnchorZoneId = "hinokura";

        public static readonly Rect HinokuraBounds = Rect.MinMaxRect(-1680f, -600f, 720f, 1800f);

        /// <summary>The three sluices: beside the bridges of the two lava channels (65) and on the road through the furnaces' court (69).</summary>
        public static readonly Vector2[] ForgeSluices =
        {
            new Vector2(-403f, 40f), new Vector2(-616f, 44f), new Vector2(-1344f, 672f),
        };

        /// <summary>The caldera's lake alone (the last of <see cref="LavaPools"/>), which is what the heat meter measures.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] CaldeiraLake = { LavaPools[LavaPools.Length - 1] };

        /// <summary>The cold stones on the ring between the lake and the rim, where the road comes in and round the far side.</summary>
        public static readonly Vector2[] CaldeiraColdStones =
        {
            new Vector2(-1316f, 1396f), new Vector2(-1290f, 1290f), new Vector2(-1396f, 1160f), new Vector2(-1590f, 1330f),
        };

        /// <summary>The three flanks the lava of Echeyde runs down (A at the crater's lip, B at the foot): west along the crater road, south, north.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] EruptionRoutes =
        {
            (new Vector2(-1006f, 118f), new Vector2(-1170f, 100f), 14f),
            (new Vector2(-960f, 74f), new Vector2(-960f, -105f), 14f),
            (new Vector2(-960f, 166f), new Vector2(-1000f, 345f), 14f),
        };

        private static readonly ConditionalWeakTable<MapRiver, object> Lava =new ConditionalWeakTable<MapRiver, object>();

        /// <summary>True for a river or pool of lava (Região 12: drawn glowing; for the rest of the rules it is water nobody wades).</summary>
        public static bool IsLava(MapRiver river) => river != null && Lava.TryGetValue(river, out _);

        private static MapRiver LavaRiver(OverworldLayout w, float width, params Vector2[] points)
        {
            MapRiver river = River(w, width, points);
            Lava.Add(river, null);
            return river;
        }

        private static class Fogo
        {
            public const string Crab = "enemy_mountain_volcano_crab";
            public const string Lizard = "enemy_mountain_lava_lizard";
            public const string Salamander = "enemy_mountain_devouring_salamander";
            public const string EmberGull = "enemy_mountain_ember_gull";
            public const string Bomb = "enemy_mountain_volcanic_bomb";
            public const string Pumice = "enemy_mountain_rolling_stone";
            public const string Shisa = "enemy_mountain_dimmed_shisa";
            public const string Carp = "enemy_cave_lantern_fish";
            public const string Smoke = "enemy_mountain_ill_wind_mist";
            public const string Tanuki = "enemy_mountain_tanuki";
            public const string Tibicena = "enemy_mountain_tibicena";
            public const string GiantLizard = "enemy_mountain_giant_lava_lizard";
            public const string Goat = "enemy_forest_smoke_goat";
            public const string Octopus = "enemy_cave_ooze";
            public const string Blade = "enemy_mountain_blade_fish";
            public const string Coral = "enemy_mountain_ember_coral";
            public const string Urchin = "enemy_forest_stone_turtle";
            public const string Ushabti = "enemy_mountain_lacquer_ushabti";
            public const string Ogre = "enemy_mountain_furnace_ogre";
            public const string Spark = NightVariantRules.FireflySwarmEnemyId;
            public const string HungryFlame = FarBeasts.HungryFlame;
            public const string Giant = "enemy_mountain_slag_giant";
            public const string Nian = "enemy_forest_nian_cub";
            public const string Crow = "enemy_cave_dark_crow";
        }

        private static OverworldLayout HinokuraRegion()
        {
            OverworldLayout w = NewRegion(HinokuraRegionId, MapRegion.ArquipelagoDeHinokura, Rect.MinMaxRect(-1680f, -600f, 720f, 1800f));
            w.World.Biome = BiomeType.Mountain;
            const string hino = HinokuraMapId;
            const string jardins = JardinsLavaMapId;
            const string echeyde = MonteEcheydeMapId;
            const string recife = RecifeObsidianaMapId;
            const string castelo = NoveFornalhasMapId;
            const string caldeira = CaldeiraMapId;
            const string coast = MapRegionTypes.Coast;
            const string plain = MapRegionTypes.Plain;
            const string mountain = MapRegionTypes.Mountain;
            const string ruins = MapRegionTypes.Ruins;
            const string crampons = MapRequirementTags.ClimbingCrampons;
            const string boat = MapRequirementTags.BoatOrDock;
            const string water = MapRequirementTags.FreshWater;

            // ---------------- Zones (a 5 × 5 grid; columns x −1680/−1200/−720/−240/240/720, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards the Teto do Mundo): the castle's curved walls, the broom fields and the shepherds' shelter (67), the
            // hot springs, the black beach (the way in from the Teto), the fumarole coast (the second way in).
            OverworldZone zMuralhas = Zone(w, "muralhas_curvas", castelo, null, -1680f, -600f, -1200f, -120f, OverworldTerrain.CurvedWalls, 254, 260, 250,
                "Curved Walls", "Muralhas Curvas", ruins, spacing: 100f);
            OverworldZone zRetama = Zone(w, "campos_de_retama", echeyde, null, -1200f, -600f, -720f, -120f, OverworldTerrain.BroomFields, 234, 240, 230,
                "Broom Fields", "Campos de Retama", mountain, crampons, 95f);
            OverworldZone zFontes = Zone(w, "fontes_termais", hino, null, -720f, -600f, -240f, -120f, OverworldTerrain.HotSprings, 223, 227, 218,
                "Hot Springs", "Fontes Termais", coast, spacing: 90f);
            OverworldZone zPraia = Zone(w, "praia_de_areia_negra", hino, null, -240f, -600f, 240f, -120f, OverworldTerrain.BlackSandBeach, 220, 224, 216,
                "Black Sand Beach", "Praia de Areia Negra", coast, spacing: 90f);
            OverworldZone zFumarolas = Zone(w, "costa_das_fumarolas", hino, null, 240f, -600f, 720f, -120f, OverworldTerrain.FumaroleCoast, 222, 227, 218,
                "Fumarole Coast", "Costa das Fumarolas", coast, spacing: 90f);
            // Second row: the castle (69) and its red gate, Mount Echeyde's cone (67), the lava channels, Hinokura (65), the thaw strait.
            OverworldZone zCastelo = Zone(w, "castelo_das_nove_fornalhas", castelo, null, -1680f, -120f, -1200f, 360f, OverworldTerrain.NineFurnaces, 252, 260, 248,
                "Castle of Nine Furnaces", "Castelo das Nove Fornalhas", ruins, spacing: 100f);
            OverworldZone zEcheyde = Zone(w, "monte_echeyde", echeyde, null, -1200f, -120f, -720f, 360f, OverworldTerrain.EcheydeCone, 234, 242, 230,
                "Mount Echeyde", "Monte Echeyde", mountain, crampons, 95f);
            OverworldZone zCanais = Zone(w, "canais_de_lava", hino, null, -720f, -120f, -240f, 360f, OverworldTerrain.LavaChannels, 222, 226, 218,
                "Lava Channels", "Canais de Lava", coast, spacing: 90f);
            OverworldZone zHinokura = Zone(w, "hinokura", hino, null, -240f, -120f, 240f, 360f, OverworldTerrain.ForgeIsle, 220, 223, 216,
                "Hinokura, the Forge Isle", "Hinokura, a Ilha-Forja", coast, spacing: 85f);
            OverworldZone zEstreito = Zone(w, "estreito_do_degelo", hino, null, 240f, -120f, 720f, 360f, OverworldTerrain.ThawStrait, 221, 225, 216,
                "Thaw Strait", "Estreito do Degelo", coast, spacing: 90f);
            // Middle row: the furnaces' court (69), the malpaís (67), the cloud pines, the lava gardens and the stone tea house, the red bridges (66).
            OverworldZone zPatio = Zone(w, "patio_das_fornalhas", castelo, null, -1680f, 360f, -1200f, 840f, OverworldTerrain.SlagFields, 256, 264, 252,
                "Furnaces' Court", "Pátio das Fornalhas", ruins, spacing: 100f);
            OverworldZone zMalpais = Zone(w, "malpais_de_echeyde", echeyde, null, -1200f, 360f, -720f, 840f, OverworldTerrain.Malpais, 236, 246, 232,
                "Malpaís of Echeyde", "Malpaís de Echeyde", mountain, crampons, 95f);
            OverworldZone zPinhais = Zone(w, "pinhais_de_nuvem", jardins, null, -720f, 360f, -240f, 840f, OverworldTerrain.CloudPines, 228, 234, 224,
                "Cloud Pines", "Pinhais de Nuvem", plain, spacing: 90f);
            OverworldZone zJardins = Zone(w, "jardins_de_lava_laqueada", jardins, null, -240f, 360f, 240f, 840f, OverworldTerrain.LavaGardens, 226, 232, 222,
                "Lacquered Lava Gardens", "Jardins de Lava Laqueada", plain, spacing: 90f);
            OverworldZone zPontes = Zone(w, "pontes_vermelhas", jardins, null, 240f, 360f, 720f, 840f, OverworldTerrain.RedBridges, 227, 235, 222,
                "Red Bridges", "Pontes Vermelhas", plain, spacing: 90f);
            // Fourth row: the caldera's slope (70), the old slag (69), the reef (68: a boat or the quay) with the lighthouse boat.
            OverworldZone zEncosta = Zone(w, "encosta_da_caldeira", caldeira, null, -1680f, 840f, -1200f, 1320f, OverworldTerrain.Caldera, 266, 274, 262,
                "Caldera Slope", "Encosta da Caldeira", mountain, water, 100f);
            OverworldZone zEscoria = Zone(w, "escoria_velha", castelo, null, -1200f, 840f, -720f, 1320f, OverworldTerrain.SlagFields, 252, 258, 248,
                "Old Slag", "Escória Velha", ruins, spacing: 100f);
            OverworldZone zCanaisQuentes = Zone(w, "canais_quentes", recife, null, -720f, 840f, -240f, 1320f, OverworldTerrain.HotChannels, 244, 252, 240,
                "Hot Channels", "Canais Quentes", coast, boat, 95f);
            OverworldZone zRecife = Zone(w, "recife_de_obsidiana", recife, null, -240f, 840f, 240f, 1320f, OverworldTerrain.ObsidianReef, 244, 250, 240,
                "Obsidian Reef", "Recife de Obsidiana", coast, boat, 95f);
            OverworldZone zIlhotas = Zone(w, "ilhotas_de_vidro", recife, null, 240f, 840f, 720f, 1320f, OverworldTerrain.GlassIslets, 245, 254, 240,
                "Glass Islets", "Ilhotas de Vidro", coast, boat, 95f);
            // North row, the Devouring Fire (70: Fresh Water): the caldera, the cold rim, the living ash, the ash fields, the ash sea.
            OverworldZone zCaldeira = Zone(w, "caldeira_do_fogo_devorador", caldeira, null, -1680f, 1320f, -1200f, 1800f, OverworldTerrain.Caldera, 272, 280, 266,
                "Devouring Fire Caldera", "Caldeira do Fogo Devorador", mountain, water, 100f);
            OverworldZone zBorda = Zone(w, "borda_fria", caldeira, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.AshPlain, 268, 276, 262,
                "Cold Rim", "Borda Fria", mountain, water, 100f);
            OverworldZone zCinzasVivas = Zone(w, "cinzas_vivas", caldeira, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.Malpais, 265, 272, 260,
                "Living Ash", "Cinzas Vivas", mountain, water, 100f);
            OverworldZone zCampos = Zone(w, "campos_de_cinza", caldeira, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.AshPlain, 264, 270, 260,
                "Ash Fields", "Campos de Cinza", mountain, water, 100f);
            OverworldZone zMar = Zone(w, "mar_de_cinza", caldeira, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.AshPlain, 264, 272, 260,
                "Sea of Ash", "Mar de Cinza", mountain, water, 100f);

            AddHinokuraFauna(zMuralhas, zRetama, zFontes, zPraia, zFumarolas, zCastelo, zEcheyde, zCanais, zHinokura, zEstreito, zPatio, zMalpais, zPinhais,
                zJardins, zPontes, zEncosta, zEscoria, zCanaisQuentes, zRecife, zIlhotas, zCaldeira, zBorda, zCinzasVivas, zCampos, zMar);

            // ---------------- Settlements ----------------
            // Hinokura: the region's return city and the barca's stop for 65–70 (flat ground at y = 0, ±30 m). Then the posts of 66–70.
            w.Vila = Place(w, hino, HinokuraRecipe(), HinokuraOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), HinokuraCityGround, true);
            Place(w, jardins, Post("casa_de_cha_de_pedra", TownSwatch.Lacquer, TownSwatch.ClayTile, TownRoofKind.Gable, TownShopDressing.Pottery, false),
                CasaDeChaOffset, PostSafe, 18f, false);
            Place(w, echeyde, Post("abrigo_dos_pastores", TownSwatch.Stone, TownSwatch.Thatch, TownRoofKind.Gable, TownShopDressing.Herder, false),
                AbrigoPastoresOffset, PostSafe, 18f, false);
            Place(w, recife, Post("barco_farol", TownSwatch.WoodDark, TownSwatch.Lacquer, TownRoofKind.Gable, TownShopDressing.BoatTrader, false),
                BarcoFarolOffset, PostSafe, 18f, false);
            Place(w, castelo, Post("portao_vermelho", TownSwatch.Lacquer, TownSwatch.ClayTile, TownRoofKind.Gable, TownShopDressing.Ferraria, false),
                PortaoVermelhoOffset, PostSafe, 18f, false);
            Place(w, caldeira, Post("borda_fria", TownSwatch.Basalt, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Cistern, false),
                BordaFriaOffset, PostSafe, 18f, false);

            // ---------------- Lava and water ----------------
            // The two tamed lava channels (10 m, carved, glowing), bridged where the west road crosses them; the malpaís' lava ponds
            // and the caldera's lake. The hot channels of the reef, the springs, the leads, the ponds and the reef's pools.
            MapRiver lavaLeste = LavaRiver(w, 10f, LavaCanalLeste);
            Vector2 bridgeLeste = On(LavaCanalLeste[2], LavaCanalLeste[3], 0.233f);
            Crossing(lavaLeste, bridgeLeste, 4.5f, true);
            MapRiver lavaOeste = LavaRiver(w, 10f, LavaCanalOeste);
            Vector2 bridgeOeste = On(LavaCanalOeste[2], LavaCanalOeste[3], 0.03f);
            Crossing(lavaOeste, bridgeOeste, 4.5f, true);
            foreach ((Vector2 a, Vector2 b, float width) in LavaPools)
            {
                LavaRiver(w, width, a, b);
            }

            River(w, 10f, CanalQuenteSul);
            River(w, 10f, CanalQuenteNorte);
            foreach ((Vector2 a, Vector2 b, float width) in HinokuraPools)
            {
                River(w, width, a, b);
            }

            // ---------------- Gates out of the region ----------------
            // South: two passes down to the Teto do Mundo's northern heights (Wakhan's pass, map 40, and the crests of the ill wind,
            // map 39's area); east: two passes over the thaw strait to the Mar Gelado's tundra (map 61's area) and Omöl's lakes (tagged with map 60: one target per gate).
            Gate(w, "exit_wakhan", MapExitKind.Map, WakhanId, Rect.MinMaxRect(72f, -600f, 88f, -597f), new Vector2(80f, -591f), Vector2.up);
            Gate(w, "exit_cristas", MapExitKind.Map, ForteVentosId, Rect.MinMaxRect(512f, -600f, 528f, -597f), new Vector2(520f, -591f), Vector2.up);
            Gate(w, "exit_tundra", MapExitKind.Map, TundraOmolMapId, Rect.MinMaxRect(717f, 292f, 720f, 308f), new Vector2(711f, 300f), Vector2.left);
            Gate(w, "exit_lagos", MapExitKind.Map, CanaisGeloMapId, Rect.MinMaxRect(717f, 592f, 720f, 608f), new Vector2(711f, 600f), Vector2.left);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = HinokuraCityGround;
            // The south road from the town to Wakhan's pass; from its fork south-east to the crests' pass and west along the springs
            // to the shepherds' shelter.
            Vector2 southFork = new Vector2(40f, -220f);
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(15f, -120f), southFork, new Vector2(70f, -400f), new Vector2(80f, -591f));
            PathVia(w, 3.5f, southFork, new Vector2(250f, -300f), new Vector2(450f, -420f), new Vector2(520f, -591f));
            PathVia(w, 3.5f, southFork, new Vector2(-200f, -260f), new Vector2(-460f, -380f), new Vector2(-700f, -420f), AbrigoPastoresOffset + new Vector2(18f, 0f));
            // The east road over the thaw strait to the tundra's pass; from its fork north-east to the lakes' pass.
            Vector2 eastFork = new Vector2(330f, 120f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(200f, 40f), eastFork, new Vector2(520f, 240f), new Vector2(711f, 300f));
            PathVia(w, 3.5f, eastFork, new Vector2(450f, 380f), new Vector2(600f, 520f), new Vector2(711f, 600f));
            // The west road over the two lava channels (bridges) to the cone's foot and the shepherds' shelter.
            Vector2 westFork = new Vector2(-300f, 30f);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-150f, 10f), westFork, bridgeLeste, new Vector2(-520f, 36f), bridgeOeste, new Vector2(-760f, 20f),
                new Vector2(-880f, -150f), AbrigoPastoresOffset + new Vector2(0f, 18f));
            // Round the cone's west foot (and up to the crater rim), on to the castle's red gate, north to the malpaís.
            Vector2 coneFork = new Vector2(-1170f, 100f);
            PathVia(w, 3.5f, AbrigoPastoresOffset + new Vector2(-18f, 0f), new Vector2(-1120f, -220f), new Vector2(-1170f, 0f), coneFork, new Vector2(-1170f, 200f),
                new Vector2(-1100f, 380f), new Vector2(-1000f, 560f));
            PathVia(w, 3f, coneFork, new Vector2(-1060f, 110f), new Vector2(-1012f, 116f));
            PathVia(w, 3.5f, coneFork, new Vector2(-1230f, 110f), PortaoVermelhoOffset + new Vector2(18f, 0f));
            // The garden road (from the east road: the town has no north street) to the stone tea house, on to the lighthouse boat.
            PathVia(w, 4f, On(new Vector2(g, 0f), new Vector2(200f, 40f), 0.35f), new Vector2(80f, 150f), new Vector2(20f, 400f), CasaDeChaOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, CasaDeChaOffset + new Vector2(0f, 18f), new Vector2(20f, 800f), new Vector2(0f, 950f), BarcoFarolOffset + new Vector2(0f, -18f));
            // From the tea house east to the red bridges, west through the cloud pines to the malpaís.
            PathVia(w, 3.5f, CasaDeChaOffset + new Vector2(18f, 0f), new Vector2(250f, 640f), new Vector2(450f, 560f), new Vector2(600f, 520f));
            PathVia(w, 3.5f, CasaDeChaOffset + new Vector2(-18f, 0f), new Vector2(-300f, 620f), new Vector2(-560f, 600f), new Vector2(-800f, 640f), new Vector2(-1000f, 560f));
            // The castle: south through the curved walls, north past the furnaces' court to the old slag and the caldera's slope.
            Vector2 courtFork = new Vector2(-1350f, 700f);
            PathVia(w, 3.5f, PortaoVermelhoOffset + new Vector2(0f, -18f), new Vector2(-1400f, -120f), new Vector2(-1440f, -380f));
            PathVia(w, 3.5f, PortaoVermelhoOffset + new Vector2(0f, 18f), new Vector2(-1380f, 400f), courtFork, new Vector2(-1150f, 900f), new Vector2(-960f, 1000f));
            PathVia(w, 3.5f, courtFork, new Vector2(-1420f, 950f), new Vector2(-1400f, 1150f));
            // The reef: west through the hot channels to the old slag, east to the glass islets and the sea of ash.
            Vector2 slagFork = new Vector2(-960f, 1000f);
            PathVia(w, 3.5f, BarcoFarolOffset + new Vector2(-18f, 0f), new Vector2(-300f, 1090f), new Vector2(-520f, 1080f), new Vector2(-800f, 1060f), slagFork);
            PathVia(w, 3.5f, BarcoFarolOffset + new Vector2(18f, 0f), new Vector2(300f, 1100f), new Vector2(500f, 1150f), new Vector2(620f, 1330f), new Vector2(560f, 1620f));
            // The north road over the ash to the cold rim and down into the caldera; up from the old slag to the cold rim.
            Vector2 ashFork = new Vector2(0f, 1500f);
            PathVia(w, 4f, BarcoFarolOffset + new Vector2(0f, 18f), new Vector2(20f, 1300f), ashFork, new Vector2(-300f, 1560f), new Vector2(-600f, 1560f), BordaFriaOffset + new Vector2(18f, 0f));
            PathVia(w, 3.5f, ashFork, new Vector2(300f, 1550f), new Vector2(560f, 1620f));
            PathVia(w, 3.5f, slagFork, new Vector2(-900f, 1300f), BordaFriaOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, BordaFriaOffset + new Vector2(-18f, 0f), new Vector2(-1150f, 1500f), new Vector2(-1260f, 1440f), new Vector2(-1320f, 1390f));

            // ---------------- Landmarks ----------------
            Sign(w, southFork, new Vector2(250f, -300f), 4f, 0f);
            Sign(w, eastFork, new Vector2(450f, 380f), 4f, 0f);
            Sign(w, westFork, new Vector2(-150f, 10f), 4f, 0f);
            Sign(w, coneFork, new Vector2(-1060f, 110f), 3.5f, 0f);
            Sign(w, courtFork, new Vector2(-1420f, 950f), 3.5f, 0f);
            Sign(w, slagFork, new Vector2(-900f, 1300f), 3.5f, 0f);
            Sign(w, ashFork, new Vector2(300f, 1550f), 4f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -220f, -330f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 420f, 60f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -820f, -300f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -420f, 660f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 160f, 1140f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -1260f, 560f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -400f, 1500f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 160f, 520f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, GlutaoArena.x, GlutaoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, ReiShisaArena.x, ReiShisaArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, MestreDasFornalhasArena.x, MestreDasFornalhasArena.y, 0f, 1.1f);

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
        // The forge town (its own recipe, Cities/Town: the LacquerPalace style, inside a radius of 22.5 m on its reserved flat ground of
        // ±30 m at y = 0; no backdrop: the isle is all round it). Its streets leave W, E and S (no north street: the palace's yards and
        // the volcano), so the garden road leaves from the east road.
        // ------------------------------------------------------------------

        private static CityTownRecipe HinokuraRecipe() => NoBackdrop(CityTownRecipes.Hinokura());

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 65–70 archetypes)
        // ------------------------------------------------------------------

        private static void AddHinokuraFauna(OverworldZone muralhas, OverworldZone retama, OverworldZone fontes, OverworldZone praia, OverworldZone fumarolas,
            OverworldZone castelo, OverworldZone echeyde, OverworldZone canais, OverworldZone hinokura, OverworldZone estreito, OverworldZone patio,
            OverworldZone malpais, OverworldZone pinhais, OverworldZone jardins, OverworldZone pontes, OverworldZone encosta, OverworldZone escoria,
            OverworldZone canaisQuentes, OverworldZone recife, OverworldZone ilhotas, OverworldZone caldeira, OverworldZone borda, OverworldZone cinzasVivas,
            OverworldZone campos, OverworldZone mar)
        {
            // At night: the lava lights the isle (the sparks: fireflies), the owl over the beaches and the gardens, the tanuki come out
            // (the moon-mother with them), the tibicenas double on Echeyde (the werewolf runs with them), the bats in the castle, the
            // reef glows (the sparks) and the vapour hides beasts.
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;

            // Hinokura, the beach, the fumaroles, the strait, the lava channels, the springs (65): the Caranguejo-Vulcão [tanque], the
            // Lagarto de Lava [rápido], the Gaivota-Brasa [distância], the Pedra-Pomes Voadora [enxame] (the rolling stone).
            hinokura.Fauna.Add(Fauna("gaivotas_brasa", "[distância]", T(N(Fogo.EmberGull, 3), N(Fogo.Lizard, 1)), T(V(Fogo.EmberGull, 2), S(sparks, 1)), 4));
            hinokura.Fauna.Add(Fauna("lagartos_de_lava", "[rápido]", T(N(Fogo.Lizard, 3), N(Fogo.Crab, 1)), T(V(Fogo.Lizard, 2), S(owl, 1)), 4));
            praia.Fauna.Add(Fauna("caranguejos_vulcao", "[tanque]", T(N(Fogo.Crab, 3), N(Fogo.EmberGull, 1)), T(V(Fogo.Crab, 2), S(owl, 1)), 3, null, 30f));
            praia.Fauna.Add(Fauna("gaivotas_da_praia", "[distância]", T(N(Fogo.EmberGull, 3), N(Fogo.Crab, 1)), T(V(Fogo.EmberGull, 2), S(sparks, 1)), 3));
            fumarolas.Fauna.Add(Fauna("pedras_pomes", "[enxame]", T(N(Fogo.Pumice, 3), N(Fogo.Smoke, 1)), T(V(Fogo.Pumice, 2), S(sparks, 1)), 5));
            fumarolas.Fauna.Add(Fauna("caranguejos_das_fumarolas", "[tanque]", T(N(Fogo.Crab, 3), N(Fogo.Pumice, 1)), T(V(Fogo.Crab, 2), S(owl, 1)), 3, null, 30f));
            estreito.Fauna.Add(Fauna("caranguejos_do_degelo", "[tanque]", T(N(Fogo.Crab, 3), N(Fogo.Carp, 1)), T(V(Fogo.Crab, 2), S(moon, 1)), 3, null, 30f));
            estreito.Fauna.Add(Fauna("gaivotas_do_estreito", "[distância]", T(N(Fogo.EmberGull, 3), N(Fogo.Carp, 1)), T(V(Fogo.EmberGull, 2), S(owl, 1)), 3));
            canais.Fauna.Add(Fauna("lagartos_dos_canais", "[rápido]", T(N(Fogo.Lizard, 3), N(Fogo.Pumice, 1)), T(V(Fogo.Lizard, 3), S(sparks, 1)), 4));
            canais.Fauna.Add(Fauna("pedras_pomes_dos_canais", "[enxame]", T(N(Fogo.Pumice, 3), N(Fogo.Lizard, 1)), T(V(Fogo.Pumice, 2), S(sparks, 1)), 5));
            fontes.Fauna.Add(Fauna("lagartos_das_fontes", "[rápido]", T(N(Fogo.Lizard, 3), N(Fogo.Smoke, 1)), T(V(Fogo.Lizard, 2), S(moon, 1)), 4));
            fontes.Fauna.Add(Fauna("vapores_das_fontes", "[controle]", T(N(Fogo.Smoke, 3), N(Fogo.Crab, 1)), T(V(Fogo.Smoke, 2), S(sparks, 1)), 4));

            // Jardins, Pinhais de Nuvem, Pontes Vermelhas (66): the Shisa Apagada [elite], the Carpa de Brasa [rápido]
            // (the lantern fish), the Bambu de Fumaça [controle] (the ill-wind mist), the Tanuki Ladrão [rápido]; the
            // stone lanterns light up and the tanuki come out at night.
            jardins.Fauna.Add(Fauna("shisas_apagadas", "[elite]", T(N(Fogo.Shisa, 3), N(Fogo.Tanuki, 1)), T(V(Fogo.Shisa, 2), S(owl, 1)), 3, null, 32f));
            jardins.Fauna.Add(Fauna("tanukis_ladroes", "[rápido]", T(N(Fogo.Tanuki, 3), N(Fogo.Carp, 1)), T(V(Fogo.Tanuki, 3), S(moon, 1)), 4));
            pinhais.Fauna.Add(Fauna("bambus_de_fumaca", "[controle]", T(N(Fogo.Smoke, 3), N(Fogo.Tanuki, 1)), T(V(Fogo.Smoke, 2), S(owl, 1)), 4));
            pinhais.Fauna.Add(Fauna("tanukis_dos_pinhais", "[rápido]", T(N(Fogo.Tanuki, 3), N(Fogo.Smoke, 1)), T(V(Fogo.Tanuki, 3), S(moon, 1)), 4));
            pontes.Fauna.Add(Fauna("carpas_de_brasa", "[rápido]", T(N(Fogo.Carp, 3), N(Fogo.Tanuki, 1)), T(V(Fogo.Carp, 2), S(sparks, 1)), 4));
            pontes.Fauna.Add(Fauna("shisas_das_pontes", "[elite]", T(N(Fogo.Shisa, 3), N(Fogo.Carp, 1)), T(V(Fogo.Shisa, 2), S(moon, 1)), 3, null, 32f));

            // Campos de Retama, Monte Echeyde, Malpaís (67): the Tibicena [rápido] (Guayota's black dogs, twice as many at
            // night), the Lagarto-Gigante de Lava [tanque], the Bomba Vulcânica [distância] (the ember vulture),
            // the Fumarola [controle] (the ill-wind mist); the shepherds' wild goats (the smoke goat) on the broom.
            retama.Fauna.Add(Fauna("cabras_dos_pastores", "[rápido]", T(N(Fogo.Goat, 3), N(Fogo.Tibicena, 1)), T(V(Fogo.Tibicena, 2), S(werewolf, 1)), 4));
            retama.Fauna.Add(Fauna("tibicenas_da_retama", "[rápido]", T(N(Fogo.Tibicena, 3), N(Fogo.Goat, 1)), T(V(Fogo.Tibicena, 4), S(werewolf, 1)), 4));
            echeyde.Fauna.Add(Fauna("tibicenas", "[rápido]", T(N(Fogo.Tibicena, 3), N(Fogo.Bomb, 1)), T(V(Fogo.Tibicena, 4), S(werewolf, 1)), 4));
            echeyde.Fauna.Add(Fauna("bombas_vulcanicas", "[distância]", T(N(Fogo.Bomb, 3), N(Fogo.Smoke, 1)), T(V(Fogo.Bomb, 2), S(sparks, 1)), 3));
            malpais.Fauna.Add(Fauna("lagartos_gigantes", "[tanque]", T(N(Fogo.GiantLizard, 3), N(Fogo.Tibicena, 1)), T(V(Fogo.GiantLizard, 2), S(werewolf, 1)), 3, null, 30f));
            malpais.Fauna.Add(Fauna("fumarolas", "[controle]", T(N(Fogo.Smoke, 3), N(Fogo.GiantLizard, 1)), T(V(Fogo.Smoke, 2), S(sparks, 1)), 4));

            // Canais Quentes, Recife de Obsidiana, Ilhotas de Vidro (68): the Polvo de Obsidiana [controle] (the ooze), the Peixe-Lâmina
            // [rápido], the Coral de Brasa [distância], the Ouriço de Vidro [tanque] (the stone
            // turtle); the reef glows at night.
            canaisQuentes.Fauna.Add(Fauna("peixes_lamina", "[rápido]", T(N(Fogo.Blade, 3), N(Fogo.Octopus, 1)), T(V(Fogo.Blade, 2), S(sparks, 1)), 4));
            canaisQuentes.Fauna.Add(Fauna("polvos_dos_canais", "[controle]", T(N(Fogo.Octopus, 3), N(Fogo.Blade, 1)), T(V(Fogo.Octopus, 2), S(moon, 1)), 4));
            recife.Fauna.Add(Fauna("polvos_de_obsidiana", "[controle]", T(N(Fogo.Octopus, 3), N(Fogo.Coral, 1)), T(V(Fogo.Octopus, 2), S(sparks, 1)), 4));
            recife.Fauna.Add(Fauna("corais_de_brasa", "[distância]", T(N(Fogo.Coral, 3), N(Fogo.Urchin, 1)), T(V(Fogo.Coral, 3), S(sparks, 1)), 3));
            ilhotas.Fauna.Add(Fauna("ouricos_de_vidro", "[tanque]", T(N(Fogo.Urchin, 3), N(Fogo.Blade, 1)), T(V(Fogo.Urchin, 2), S(moon, 1)), 3, null, 30f));
            ilhotas.Fauna.Add(Fauna("peixes_das_ilhotas", "[rápido]", T(N(Fogo.Blade, 3), N(Fogo.Coral, 1)), T(V(Fogo.Blade, 2), S(sparks, 1)), 4));

            // Muralhas Curvas, Castelo, Pátio, Escória Velha (69): the Ushabti de Laca [tanque], the
            // Ogro-Fornalha [elite], the Fole Vivo [controle] (the ill-wind mist), the Faísca Raivosa [enxame] (the
            // firefly swarm); the nian cub by the furnaces, the crows on the walls; the bats at night (the furnaces burn hotter).
            muralhas.Fauna.Add(Fauna("ushabtis_de_laca", "[tanque]", T(N(Fogo.Ushabti, 3), N(Fogo.Crow, 1)), T(V(Fogo.Ushabti, 2), S(bats, 1)), 3, null, 30f));
            muralhas.Fauna.Add(Fauna("corvos_das_muralhas", "[distância]", T(N(Fogo.Crow, 3), N(Fogo.Ushabti, 1)), T(V(Fogo.Crow, 2), S(bats, 1)), 3));
            castelo.Fauna.Add(Fauna("ogros_fornalha", "[elite]", T(N(Fogo.Ogre, 3), N(Fogo.Ushabti, 1)), T(V(Fogo.Ogre, 2), S(bats, 1)), 3, null, 32f));
            castelo.Fauna.Add(Fauna("foles_vivos", "[controle]", T(N(Fogo.Smoke, 3), N(Fogo.Spark, 1)), T(V(Fogo.Smoke, 2), S(sparks, 1)), 4));
            patio.Fauna.Add(Fauna("faiscas_raivosas", "[enxame]", T(N(Fogo.Spark, 3), N(Fogo.Nian, 1)), T(V(Fogo.Spark, 3), S(sparks, 1)), 5));
            patio.Fauna.Add(Fauna("nians_das_fornalhas", "[rápido]", T(N(Fogo.Nian, 3), N(Fogo.Ogre, 1)), T(V(Fogo.Nian, 2), S(bats, 1)), 4));
            escoria.Fauna.Add(Fauna("ushabtis_da_escoria", "[tanque]", T(N(Fogo.Ushabti, 3), N(Fogo.Spark, 1)), T(V(Fogo.Ushabti, 2), S(bats, 1)), 3, null, 30f));
            escoria.Fauna.Add(Fauna("faiscas_da_escoria", "[enxame]", T(N(Fogo.Spark, 3), N(Fogo.Ushabti, 1)), T(V(Fogo.Spark, 3), S(sparks, 1)), 5));

            // The Devouring Fire (70): the Chama Faminta [enxame] (its own beast now, FarBeasts), the Salamandra Devoradora [rápido] (the sand
            // snake), the Gigante de Escória [tanque], the Cinza Viva [controle] (the ooze); always night in the
            // caldera (its night groups are the same beasts, stronger).
            caldeira.Fauna.Add(Fauna("gigantes_de_escoria", "[tanque]", T(N(Fogo.Giant, 3), N(Fogo.HungryFlame, 1)), T(V(Fogo.Giant, 3), S(bats, 1)), 3, null, 30f));
            caldeira.Fauna.Add(Fauna("chamas_famintas", "[enxame]", T(N(Fogo.HungryFlame, 3), N(Fogo.Salamander, 1)), T(V(Fogo.HungryFlame, 3), S(sparks, 1)), 5));
            encosta.Fauna.Add(Fauna("salamandras_devoradoras", "[rápido]", T(N(Fogo.Salamander, 3), N(Fogo.Giant, 1)), T(V(Fogo.Salamander, 3), S(sparks, 1)), 4));
            encosta.Fauna.Add(Fauna("cinzas_vivas_da_encosta", "[controle]", T(N(Fogo.Octopus, 3), N(Fogo.Salamander, 1)), T(V(Fogo.Octopus, 2), S(bats, 1)), 4));
            borda.Fauna.Add(Fauna("gigantes_da_borda", "[tanque]", T(N(Fogo.Giant, 3), N(Fogo.Octopus, 1)), T(V(Fogo.Giant, 2), S(werewolf, 1)), 3, null, 30f));
            borda.Fauna.Add(Fauna("chamas_da_borda", "[enxame]", T(N(Fogo.HungryFlame, 3), N(Fogo.Giant, 1)), T(V(Fogo.HungryFlame, 3), S(sparks, 1)), 5));
            cinzasVivas.Fauna.Add(Fauna("cinzas_vivas", "[controle]", T(N(Fogo.Octopus, 3), N(Fogo.Spark, 1)), T(V(Fogo.Octopus, 2), S(sparks, 1)), 4));
            cinzasVivas.Fauna.Add(Fauna("salamandras_das_cinzas", "[rápido]", T(N(Fogo.Salamander, 3), N(Fogo.Octopus, 1)), T(V(Fogo.Salamander, 3), S(bats, 1)), 4));
            campos.Fauna.Add(Fauna("chamas_dos_campos", "[enxame]", T(N(Fogo.HungryFlame, 3), N(Fogo.Crow, 1)), T(V(Fogo.HungryFlame, 3), S(sparks, 1)), 5));
            campos.Fauna.Add(Fauna("corvos_da_cinza", "[distância]", T(N(Fogo.Crow, 3), N(Fogo.Giant, 1)), T(V(Fogo.Crow, 2), S(owl, 1)), 3));
            mar.Fauna.Add(Fauna("gigantes_do_mar_de_cinza", "[tanque]", T(N(Fogo.Giant, 3), N(Fogo.Salamander, 1)), T(V(Fogo.Giant, 2), S(bats, 1)), 3, null, 30f));
            mar.Fauna.Add(Fauna("salamandras_do_mar", "[rápido]", T(N(Fogo.Salamander, 3), N(Fogo.Spark, 1)), T(V(Fogo.Salamander, 3), S(sparks, 1)), 4));
        }
    }
}
