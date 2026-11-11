using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 8 — Planície de Mingcheng as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 41–46, all in
    /// the catalog: 43 is the canal, the Eclusa its open approach; 44 the paddies, the Moinho their open approach; 45 the four wall areas; 46 the
    /// Cidadela, north of the nian's peaks),
    /// no scene change between Mingcheng and the country round it. Mingcheng (41, the city of nine rings, the region's return
    /// city) stands at the plaza (0, 0) on the river plain, 600 m from the south edge where the spirit way comes up from the
    /// Bosque do Zarafshan (in the stitched world the plain fills the walled corner north of the Zarafshan, the Teto do Mundo
    /// to its west):
    /// <list type="bullet">
    /// <item>round the city (nv 90–96): the <b>Anéis de Mingcheng</b> and the <b>Vilas do Anel</b> (41), the <b>Caminho dos
    /// Leões</b> (the spirit way, 41), the <b>Campos de Painço</b>, the <b>Rio dos Salgueiros</b> (the upper Rio Dourado), the
    /// <b>Eclusa da Lua</b> on the Canal das Lanternas, the <b>Fossos de Terracota</b> (41);</item>
    /// <item>the middle ring (nv 95–102): the <b>Colinas de Chá</b>, the <b>Canal das Lanternas</b> (43), the <b>Arrozais
    /// Espelhados</b> and the <b>Moinho d'Água</b> (44), the <b>Bambuzal do Oeste</b>, the <b>Ravinas de Loess</b>, and east of
    /// the city the <b>Jardins em Terraço</b> and the <b>Torres Brancas</b> (42, the high elves' city: Sun Seal);</item>
    /// <item>the far north (nv 104–126): the <b>Picos de Calcário</b>, the <b>Cristas dos Pinheiros</b>, the <b>Vale dos
    /// Guerreiros</b> (terracotta), the <b>Colinas dos Espelhos</b> and the <b>Vale da Névoa</b> (the mirrors' shortcut, 42), and
    /// the <b>Muralha das Mil Torres</b> (45) along the north: <b>Muralha Oeste</b>, <b>Torre Quinhentos</b>, the wall itself,
    /// the <b>Portão do Nian</b> and the <b>Picos do Nian</b>.</item>
    /// </list>
    /// Water: the Rio Dourado rises here (its upper course comes down to the Zarafshan's north edge at the same point), the
    /// Canal das Lanternas runs north from it past the city to the carp's lake under the white hills, crossed only at stone
    /// bridges; a lotus pond in the outer rings. Monsters: the region's own beasts (the stone lion, also as the lime guardians,
    /// the lantern-tailed fox, the terracotta soldier, the stone turtle, the mud buffalo by day, the nian cub, the thief
    /// macaque) and existing beasts that fit the other sheets (the hawk for the paper crane and the ibis, broken lanterns for
    /// the paper kites, mirrors and scarecrows, the sand snake for the dragon carp, the lantern fish, the ill-wind mist for the
    /// smoke boatman and the crooked ray, the thief gull for the mandarin duck, bats for the smoke locusts, the wolf for the
    /// terracotta horses, the spider in the bamboo, the scorpion and the vulture in the loess, the rolling stone in the karst)
    /// and the night species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string MingchengId = "overworld_mingcheng";

        public const string MingchengMapId = "map_041_mingcheng";
        public const string TorresBrancasMapId = "map_042_torres_brancas";
        public const string CanalLanternasMapId = "map_043_canal_das_lanternas";
        public const string ArrozaisMapId = "map_044_arrozais_espelhados";
        public const string MuralhaMapId = "map_045_muralha_mil_torres";
        public const string CidadelaMapId = "map_046_cidadela_gulosos";

        public static readonly Vector2 MingchengOffset = Vector2.zero;
        public static readonly Vector2 TorresBrancasOffset = new Vector2(960f, 600f);
        public static readonly Vector2 EclusaDaLuaOffset = new Vector2(600f, 560f);
        public static readonly Vector2 MoinhoDaguaOffset = new Vector2(-960f, 240f);
        public static readonly Vector2 TorreQuinhentosOffset = new Vector2(-400f, 1380f);
        public static readonly Vector2 PortaoNonoAnelOffset = new Vector2(960f, 1660f);

        /// <summary>The arena of the Quatro Gulosos (46): the Salão Central's stone ring, east of the Portão do Nono Anel at the end of its spur.</summary>
        public static readonly Vector2 GulososArena = new Vector2(1100f, 1700f);

        /// <summary>A Carpa que Não Pulou's arena (43, map boss): the stone ring on the west bank trail of the Canal das Lanternas.</summary>
        public static readonly Vector2 CarpaQueNaoPulouArena = new Vector2(373f, 613f);

        /// <summary>Nian's arena (45, map boss): the stone ring on the trail of the Portão do Nian, south of the wall, under the peaks it comes down from.</summary>
        public static readonly Vector2 NianArena = new Vector2(505f, 1417f);

        /// <summary>Half the side (m) of Mingcheng's flat ground (reserved for the ring city's recipe, ±28 m and more).</summary>
        public const float MingchengGround = 30f;

        /// <summary>
        /// The upper Rio Dourado: it comes down from the canal's head to the Zarafshan's north edge, where its lower course goes
        /// on (world (520, 1400) = region (320, −600)).
        /// </summary>
        public static readonly Vector2[] DouradoHead =
        {
            new Vector2(400f, -40f), new Vector2(360f, -160f), new Vector2(300f, -380f), new Vector2(320f, -600f),
        };

        /// <summary>The Canal das Lanternas (43), from the river's head north past the city to the carp's lake under the white hills.</summary>
        public static readonly Vector2[] LanternCanalLine =
        {
            new Vector2(400f, -40f), new Vector2(430f, 200f), new Vector2(440f, 450f), new Vector2(480f, 700f), new Vector2(470f, 950f),
            new Vector2(520f, 1150f), new Vector2(540f, 1260f),
        };

        /// <summary>Half width (m) of the crest under the great wall (<see cref="ReliefShape.WallRidge"/>).</summary>
        public const float GreatWallRidgeHalfWidth = 110f;

        /// <summary>
        /// The great wall's line (45) in the region's own coordinates: z (north) of the crest at <paramref name="x"/>, bending
        /// gently between z ≈ 1510 and 1670 across the whole north row (the trails below keep ≥ 60 m south of it).
        /// </summary>
        public static float GreatWallZ(float x) => 1590f + 55f * Mathf.Sin(x / 190f) + 25f * Mathf.Sin(x / 71f + 1.3f);

        private static class Ming
        {
            public const string Lion = "enemy_forest_stone_lion";
            public const string Fox = "enemy_forest_lantern_fox";
            public const string Crane = "enemy_mountain_crested_hawk";
            public const string Terracotta = "enemy_forest_terracotta_soldier";
            public const string Kite = "enemy_desert_broken_lantern";
            public const string Carp = "enemy_desert_sand_snake";
            public const string LanternFish = "enemy_cave_lantern_fish";
            public const string Smoke = "enemy_mountain_ill_wind_mist";
            public const string Turtle = "enemy_forest_stone_turtle";
            public const string Buffalo = "enemy_forest_mud_buffalo";
            public const string Duck = "enemy_forest_thief_gull";
            public const string Locusts = PeriodSpawnTable.CaveBats;
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Spider = "enemy_forest_spider";
            public const string Monkey = "enemy_forest_thief_monkey";
            public const string Nian = "enemy_forest_nian_cub";
            public const string Horse = PeriodSpawnTable.ForestWolf;
            public const string Crow = "enemy_cave_dark_crow";
            public const string Scorpion = "enemy_desert_scorpion";
            public const string Vulture = "enemy_desert_ember_vulture";
            public const string Stone = "enemy_mountain_rolling_stone";
            public const string Potoo = NightVariantRules.PotooEnemyId;
        }

        private static OverworldLayout MingchengRegion()
        {
            OverworldLayout w = NewRegion(MingchengId, MapRegion.PlanicieDeMingcheng, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Forest;
            const string ming = MingchengMapId;
            const string torres = TorresBrancasMapId;
            const string canalMap = CanalLanternasMapId;
            const string arrozaisMap = ArrozaisMapId;
            const string muralhaMap = MuralhaMapId;
            const string cidadelaMap = CidadelaMapId;
            const string torch = MapRequirementTags.TorchRequired;
            const string plain = MapRegionTypes.Plain;
            const string forest = MapRegionTypes.Forest;
            const string rock = MapRegionTypes.Mountain;
            const string seal = MapRequirementTags.SunSeal;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards the Zarafshan): bamboo, millet, the spirit way (41), the willow river, the loess gullies.
            OverworldZone zBambu = Zone(w, "bambuzal_do_oeste", null, ming, -1200f, -600f, -720f, -120f, OverworldTerrain.BambooGrove, 96, 100, 94,
                "Western Bamboo Grove", "Bambuzal do Oeste", forest, spacing: 85f);
            OverworldZone zPainco = Zone(w, "campos_de_painco", null, ming, -720f, -600f, -240f, -120f, OverworldTerrain.MilletFields, 92, 95, 90,
                "Millet Fields", "Campos de Painço", plain, spacing: 85f);
            OverworldZone zLeoes = Zone(w, "caminho_dos_leoes", ming, null, -240f, -600f, 240f, -120f, OverworldTerrain.SpiritWay, 91, 94, 89,
                "Way of the Lions", "Caminho dos Leões", plain, spacing: 85f);
            OverworldZone zSalgueiros = Zone(w, "rio_dos_salgueiros", null, ming, 240f, -600f, 720f, -120f, OverworldTerrain.WillowRiver, 92, 95, 90,
                "Willow River", "Rio dos Salgueiros", plain, spacing: 85f);
            OverworldZone zLoess = Zone(w, "ravinas_de_loess", null, ming, 720f, -600f, 1200f, -120f, OverworldTerrain.LoessGullies, 96, 100, 94,
                "Loess Gullies", "Ravinas de Loess", rock, spacing: 90f);
            // Second row: the mirror paddies (44), the ring villages and the rings of Mingcheng (41), the moon lock, the garden terraces (42).
            OverworldZone zArrozais = Zone(w, "arrozais_espelhados", arrozaisMap, null, -1200f, -120f, -720f, 360f, OverworldTerrain.MirrorPaddies, 108, 116, 104,
                "Mirror Rice Terraces", "Arrozais Espelhados", plain, spacing: 90f);
            OverworldZone zVilas = Zone(w, "vilas_do_anel", ming, null, -720f, -120f, -240f, 360f, OverworldTerrain.NineRings, 92, 95, 90,
                "Ring Villages", "Vilas do Anel", plain, spacing: 85f);
            OverworldZone zAneis = Zone(w, "aneis_de_mingcheng", ming, null, -240f, -120f, 240f, 360f, OverworldTerrain.NineRings, 90, 93, 88,
                "Rings of Mingcheng", "Anéis de Mingcheng", plain, spacing: 80f);
            OverworldZone zEclusa = Zone(w, "eclusa_da_lua", null, canalMap, 240f, -120f, 720f, 360f, OverworldTerrain.LanternCanal, 92, 96, 90,
                "Moon Lock", "Eclusa da Lua", plain, spacing: 85f);
            OverworldZone zJardins = Zone(w, "jardins_em_terraco", torres, null, 720f, -120f, 1200f, 360f, OverworldTerrain.SpireGardens, 96, 100, 92,
                "Terrace Gardens", "Jardins em Terraço", plain, seal, 90f);
            // Middle row: the water mill (44), the tea hills, the terracotta pits (41), the lantern canal (43), the Torres Brancas (42).
            OverworldZone zMoinho = Zone(w, "moinho_dagua", null, arrozaisMap, -1200f, 360f, -720f, 840f, OverworldTerrain.MirrorPaddies, 97, 101, 95,
                "Water Mill", "Moinho d'Água", plain, spacing: 90f);
            OverworldZone zCha = Zone(w, "colinas_de_cha", null, ming, -720f, 360f, -240f, 840f, OverworldTerrain.TeaTerraces, 95, 98, 93,
                "Tea Hills", "Colinas de Chá", forest, spacing: 90f);
            OverworldZone zFossos = Zone(w, "fossos_de_terracota", ming, null, -240f, 360f, 240f, 840f, OverworldTerrain.TerracottaPits, 94, 96, 92,
                "Terracotta Pits", "Fossos de Terracota", plain, spacing: 85f);
            OverworldZone zCanal = Zone(w, "canal_das_lanternas", canalMap, null, 240f, 360f, 720f, 840f, OverworldTerrain.LanternCanal, 102, 110, 98,
                "Lantern Canal", "Canal das Lanternas", plain, spacing: 90f);
            OverworldZone zTorres = Zone(w, "torres_brancas", torres, null, 720f, 360f, 1200f, 840f, OverworldTerrain.WhiteHills, 97, 102, 95,
                "White Spires", "Torres Brancas", plain, seal, 90f);
            // Fourth row: the karst, the pine crests, the warriors' valley, the mirror hills (42's shortcut), the valley of mist.
            OverworldZone zCalcario = Zone(w, "picos_de_calcario", null, ming, -1200f, 840f, -720f, 1320f, OverworldTerrain.KarstPeaks, 110, 115, 108,
                "Limestone Peaks", "Picos de Calcário", rock, spacing: 95f);
            OverworldZone zPinheiros = Zone(w, "cristas_dos_pinheiros", null, ming, -720f, 840f, -240f, 1320f, OverworldTerrain.PineRidge, 104, 109, 102,
                "Pine Crests", "Cristas dos Pinheiros", rock, spacing: 95f);
            OverworldZone zGuerreiros = Zone(w, "vale_dos_guerreiros", null, ming, -240f, 840f, 240f, 1320f, OverworldTerrain.TerracottaPits, 104, 108, 102,
                "Valley of the Warriors", "Vale dos Guerreiros", plain, spacing: 90f);
            OverworldZone zEspelhos = Zone(w, "colinas_dos_espelhos", null, torres, 240f, 840f, 720f, 1320f, OverworldTerrain.WhiteHills, 104, 109, 102,
                "Mirror Hills", "Colinas dos Espelhos", plain, spacing: 95f);
            OverworldZone zNevoa = Zone(w, "vale_da_nevoa", null, torres, 720f, 840f, 1200f, 1320f, OverworldTerrain.PineRidge, 110, 115, 108,
                "Valley of Mist", "Vale da Névoa", rock, spacing: 95f);
            // North row: the Muralha das Mil Torres (45, return to the Torres Brancas: an oil lamp for the dark of its towers), the nian's
            // peaks and, carved out of their north half, the Cidadela dos Quatro Gulosos (46): the end of the plain, level 125.
            OverworldZone zMuralhaOeste = Zone(w, "muralha_oeste", muralhaMap, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.GreatWall, 118, 124, 116,
                "Western Wall", "Muralha Oeste", rock, torch, 100f);
            OverworldZone zQuinhentos = Zone(w, "torre_quinhentos", muralhaMap, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.GreatWall, 115, 122, 110,
                "Tower Five Hundred", "Torre Quinhentos", rock, torch, 100f);
            OverworldZone zMuralha = Zone(w, "muralha_das_mil_torres", muralhaMap, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.GreatWall, 115, 121, 110,
                "Wall of a Thousand Towers", "Muralha das Mil Torres", rock, torch, 100f);
            OverworldZone zPortao = Zone(w, "portao_do_nian", muralhaMap, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.GreatWall, 116, 123, 114,
                "Gate of the Nian", "Portão do Nian", rock, torch, 100f);
            OverworldZone zNian = Zone(w, "picos_do_nian", null, torres, 720f, 1320f, 1200f, 1560f, OverworldTerrain.KarstPeaks, 118, 126, 116,
                "Peaks of the Nian", "Picos do Nian", rock, spacing: 100f);
            OverworldZone zCidadela = Zone(w, "cidadela_dos_quatro_gulosos", cidadelaMap, null, 720f, 1560f, 1200f, 1800f, OverworldTerrain.NineRings, 125, 140, 121,
                "Citadel of the Four Gluttons", "Cidadela dos Quatro Gulosos", plain, spacing: 90f);

            AddMingchengFauna(zBambu, zPainco, zLeoes, zSalgueiros, zLoess, zArrozais, zVilas, zAneis, zEclusa, zJardins, zMoinho, zCha, zFossos, zCanal,
                zTorres, zCalcario, zPinheiros, zGuerreiros, zEspelhos, zNevoa, zMuralhaOeste, zQuinhentos, zMuralha, zPortao, zNian, zCidadela);

            // ---------------- Settlements ----------------
            // Mingcheng first: the region's return city and the barca's stop for 41 (its own recipe on flat ground at y = 0, ±30 m;
            // its streets leave W, E and S, the ring wall closes the north).
            w.Vila = Place(w, ming, MingchengRecipe(), MingchengOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), MingchengGround, true);
            // The Torres Brancas (42, the high elves' city): the return city of 42.
            Place(w, torres, TorresBrancasRecipe(), TorresBrancasOffset, Rect.MinMaxRect(-24f, -24f, 24f, 24f), RaceCityGround, true);
            // The posts of 43–46, each with its Cais da Barca, inside the map's own area (OverworldRules.Validate): the Eclusa da Lua's post on the
            // canal's east bank, the Moinho d'Água's on the paddies (the lock and the mill themselves stay in the open approach areas below them).
            Place(w, canalMap, Post("eclusa_da_lua", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.BoatTrader, false),
                EclusaDaLuaOffset, PostSafe, 18f, false);
            Place(w, arrozaisMap, Post("moinho_dagua", TownSwatch.Stone, TownSwatch.Thatch, TownRoofKind.Gable, TownShopDressing.Herder, false),
                MoinhoDaguaOffset, PostSafe, 18f, false);
            Place(w, muralhaMap, Post("torre_quinhentos", TownSwatch.Granite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Ferraria, false),
                TorreQuinhentosOffset, PostSafe, 18f, false);
            Place(w, cidadelaMap, Post("portao_do_nono_anel", TownSwatch.Granite, TownSwatch.Gold, TownRoofKind.Gable, TownShopDressing.Mercearia, false),
                PortaoNonoAnelOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The upper Rio Dourado (10 m, like its lower course in the Zarafshan), a stone bridge on the south-east road.
            MapRiver dourado = River(w, 10f, DouradoHead);
            Vector2 douradoBridge = On(DouradoHead[1], DouradoHead[2], 0.5f);
            Crossing(dourado, douradoBridge, 4.5f, true);
            // The Canal das Lanternas: stone bridges on the east road (the Eclusa da Lua) and on the north road over to the Torres.
            MapRiver canal = River(w, 14f, LanternCanalLine);
            Vector2 eastBridge = On(LanternCanalLine[0], LanternCanalLine[1], 0.55f);
            Crossing(canal, eastBridge, 4.5f, true);
            Vector2 northBridge = On(LanternCanalLine[2], LanternCanalLine[3], 0.68f);
            Crossing(canal, northBridge, 4.5f, true);
            // The carp's lake at the canal's head, the lotus pond of the outer rings, a pond by the water mill.
            River(w, 46f, new Vector2(540f, 1262f), new Vector2(600f, 1290f));
            River(w, 24f, new Vector2(150f, 250f), new Vector2(185f, 262f));
            River(w, 26f, new Vector2(-1040f, 520f), new Vector2(-1000f, 540f));

            // ---------------- Gates out of the region ----------------
            // South: the spirit way down to the Zarafshan (its ruins' road) and the loess track to its high wood; west: two passes
            // to the Teto do Mundo (by the chute station and below the wind wheels).
            Gate(w, "exit_pomar", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(-208f, -600f, -192f, -597f), new Vector2(-200f, -591f), Vector2.up);
            Gate(w, "exit_tugai", MapExitKind.Map, MapCatalog.TugaiId, Rect.MinMaxRect(792f, -600f, 808f, -597f), new Vector2(800f, -591f), Vector2.up);
            Gate(w, "exit_calhas", MapExitKind.Map, CalhasId, Rect.MinMaxRect(-1200f, 92f, -1197f, 108f), new Vector2(-1191f, 100f), Vector2.right);
            Gate(w, "exit_forte_ventos", MapExitKind.Map, ForteVentosId, Rect.MinMaxRect(-1200f, 612f, -1197f, 628f), new Vector2(-1191f, 620f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = MingchengGround;
            Vector2 torresWest = TorresBrancasOffset + new Vector2(-RaceCityGround, 0f);
            Vector2 torresEast = TorresBrancasOffset + new Vector2(RaceCityGround, 0f);
            Vector2 torresSouth = TorresBrancasOffset + new Vector2(0f, -RaceCityGround);
            // The spirit way: from the south gate up to the city's south street; the millet fork; the south-east road over the river.
            Vector2 lionFork = new Vector2(-10f, -150f);
            Vector2 milletFork = new Vector2(-60f, -300f);
            PathVia(w, 4f, new Vector2(0f, -g), lionFork, milletFork, new Vector2(-140f, -460f), new Vector2(-200f, -591f));
            PathVia(w, 3.5f, milletFork, new Vector2(-300f, -330f), new Vector2(-480f, -400f), new Vector2(-600f, -520f));
            Vector2 loessFork = new Vector2(680f, -450f);
            PathVia(w, 3.5f, lionFork, new Vector2(150f, -200f), douradoBridge, new Vector2(520f, -330f), loessFork, new Vector2(800f, -591f));
            PathVia(w, 3.5f, loessFork, new Vector2(900f, -320f), new Vector2(1060f, -150f));
            // The east road over the Eclusa da Lua to the Torres Brancas; the garden terraces down to the loess.
            Vector2 eastFork = new Vector2(760f, 330f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(200f, 40f), eastBridge, new Vector2(600f, 170f), eastFork, new Vector2(800f, 560f), torresWest);
            PathVia(w, 3.5f, torresSouth, new Vector2(980f, 300f), new Vector2(1000f, 80f), new Vector2(1060f, -150f));
            // The north road (from the east road: the city has no north street): the terracotta pits, the warriors' valley, through
            // the wall at Torre Quinhentos.
            Vector2 pitFork = new Vector2(0f, 560f);
            Vector2 wallFork = new Vector2(0f, 1430f);
            PathVia(w, 4f, On(new Vector2(g, 0f), new Vector2(200f, 40f), 0.6f), new Vector2(20f, 250f), pitFork, new Vector2(-40f, 900f), new Vector2(-20f, 1200f), wallFork,
                new Vector2(20f, 1600f), new Vector2(40f, 1760f));
            // Over the canal's north bridge, round the Torres' wall to their east street; on up the mirror hills and to the valley of mist.
            Vector2 mirrorFork = new Vector2(960f, 780f);
            PathVia(w, 3.5f, pitFork, new Vector2(240f, 600f), northBridge, new Vector2(700f, 700f), mirrorFork, new Vector2(1060f, 690f), torresEast);
            Vector2 hillFork = new Vector2(900f, 1000f);
            PathVia(w, 3.5f, mirrorFork, hillFork, new Vector2(720f, 1150f), new Vector2(650f, 1420f));
            PathVia(w, 3.5f, hillFork, new Vector2(1080f, 1150f), new Vector2(1100f, 1420f));
            // The west road: the ring villages, the bamboo, the mirror paddies and the first pass to the Teto.
            Vector2 westFork = new Vector2(-260f, 20f);
            Vector2 bambooFork = new Vector2(-520f, 40f);
            Vector2 paddyFork = new Vector2(-760f, 80f);
            PathVia(w, 4f, new Vector2(-g, 0f), westFork, bambooFork, paddyFork, new Vector2(-960f, 100f), new Vector2(-1191f, 100f));
            PathVia(w, 3.5f, bambooFork, new Vector2(-760f, -200f), new Vector2(-960f, -420f));
            // The paddies up to the water mill and the second pass; the karst above.
            Vector2 millFork = new Vector2(-960f, 620f);
            PathVia(w, 3.5f, paddyFork, new Vector2(-860f, 350f), millFork, new Vector2(-1191f, 620f));
            PathVia(w, 3.5f, millFork, new Vector2(-900f, 900f), new Vector2(-820f, 1150f), new Vector2(-800f, 1410f));
            // The tea road: from the west road up the tea hills and the pine crests to the wall.
            Vector2 teaFork = new Vector2(-480f, 600f);
            PathVia(w, 3.5f, westFork, new Vector2(-380f, 300f), teaFork, new Vector2(-520f, 900f), new Vector2(-500f, 1200f), new Vector2(-480f, 1440f));
            PathVia(w, 3f, teaFork, new Vector2(-240f, 640f), pitFork);
            // The wall walk below the crest (≥ 60 m south of the wall), from the karst to the nian's peaks.
            PathVia(w, 3.5f, new Vector2(-1100f, 1430f), new Vector2(-800f, 1410f), new Vector2(-480f, 1440f), new Vector2(-200f, 1410f), wallFork,
                new Vector2(300f, 1400f), new Vector2(650f, 1420f), new Vector2(1100f, 1420f));

            // The spurs to the posts of 43–46.
            PathVia(w, 3.5f, new Vector2(600f, 170f), new Vector2(590f, 400f), EclusaDaLuaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, new Vector2(700f, 700f), EclusaDaLuaOffset + new Vector2(18f, 0f));
            PathVia(w, 3.5f, new Vector2(-960f, 100f), MoinhoDaguaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, new Vector2(-480f, 1440f), TorreQuinhentosOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, new Vector2(960f, 1420f), new Vector2(950f, 1540f), PortaoNonoAnelOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, PortaoNonoAnelOffset + new Vector2(18f, 0f), new Vector2(1040f, 1680f), GulososArena);

            // ---------------- Landmarks ----------------
            Sign(w, lionFork, new Vector2(150f, -200f), 4f, 0f);
            Sign(w, milletFork, new Vector2(-300f, -330f), 4f, 0f);
            Sign(w, loessFork, new Vector2(900f, -320f), 3.5f, 0f);
            Sign(w, eastFork, new Vector2(800f, 560f), 4f, 0f);
            Sign(w, pitFork, new Vector2(240f, 600f), 4f, 0f);
            Sign(w, westFork, new Vector2(-380f, 300f), 4f, 0f);
            Sign(w, bambooFork, new Vector2(-760f, -200f), 4f, 0f);
            Sign(w, paddyFork, new Vector2(-860f, 350f), 4f, 0f);
            Sign(w, millFork, new Vector2(-900f, 900f), 3.5f, 0f);
            Sign(w, wallFork, new Vector2(300f, 1400f), 3.5f, 0f);
            // The rest stops of the ☆ sheets: the Eclusa da Lua (43), the Moinho d'Água (44), the Torre Quinhentos (45).
            Landmark(w, MapLandmarkKind.Campfire, 370f, 200f, 0f, 1f);
            Landmark(w, MapLandmarkKind.WaterMill, -900f, 560f, 90f);
            Landmark(w, MapLandmarkKind.Campfire, 60f, 1475f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, -280f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 300f, -470f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -700f, 300f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -320f, 1300f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 860f, 1240f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 120f, 700f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.StoneRing, -1000f, 1000f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, GulososArena.x, GulososArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, CarpaQueNaoPulouArena.x, CarpaQueNaoPulouArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, NianArena.x, NianArena.y, 0f, 1.1f);

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
        // The two towns (their own recipes, Cities/Town; no backdrop: the plain is all round them)
        // ------------------------------------------------------------------

        private static CityTownRecipe MingchengRecipe()
        {
            CityTownRecipe recipe = CityTownRecipes.Mingcheng();
            recipe.Backdrop = false;
            return recipe;
        }

        private static CityTownRecipe TorresBrancasRecipe()
        {
            CityTownRecipe recipe = CityTownRecipes.TorresBrancas();
            recipe.Backdrop = false;
            return recipe;
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 41–46 archetypes)
        // ------------------------------------------------------------------

        private static void AddMingchengFauna(OverworldZone bambu, OverworldZone painco, OverworldZone leoes, OverworldZone salgueiros, OverworldZone loess,
            OverworldZone arrozais, OverworldZone vilas, OverworldZone aneis, OverworldZone eclusa, OverworldZone jardins, OverworldZone moinho,
            OverworldZone cha, OverworldZone fossos, OverworldZone canal, OverworldZone torres, OverworldZone calcario, OverworldZone pinheiros,
            OverworldZone guerreiros, OverworldZone espelhos, OverworldZone nevoa, OverworldZone muralhaOeste, OverworldZone quinhentos,
            OverworldZone muralha, OverworldZone portao, OverworldZone nian, OverworldZone cidadela)
        {
            // At night: the werewolf in the pines and on the wall, the shroud owl over the fields, the vampire bats in the karst and
            // the towers, the moon-mother by the water, the mud buffaloes' night herd (the queixada), fireflies over the paddies.
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies herd = NightVariantRules.Queixada;
            NightSpecies fireflies = NightVariantRules.VagaLume;

            // Anéis de Mingcheng and Vilas do Anel (41): the Leão de Pedra Apagado [tanque], the Raposa de Cauda
            // Lanterna [rápido], the Grou de Papel [distância] (the hawk), the Soldado de Terracota Apagado [tanque].
            aneis.Fauna.Add(Fauna("raposas_de_cauda_lanterna", "[rápido]", T(N(Ming.Fox, 3), N(Ming.Crane, 1)), T(V(Ming.Fox, 2), S(moon, 1)), 4));
            aneis.Fauna.Add(Fauna("grous_de_papel", "[distância]", T(N(Ming.Crane, 3), N(Ming.Fox, 1)), T(V(Ming.Crane, 2), S(owl, 1)), 3));
            vilas.Fauna.Add(Fauna("leoes_de_pedra", "[tanque]", T(N(Ming.Lion, 3), N(Ming.Fox, 1)), T(V(Ming.Lion, 2), V(Ming.Fox, 1)), 3, null, 30f));
            vilas.Fauna.Add(Fauna("raposas_das_vilas", "[rápido]", T(N(Ming.Fox, 3), N(Ming.Duck, 1)), T(V(Ming.Fox, 2), S(owl, 1)), 4));
            vilas.Fauna.Add(Fauna("grous_das_vilas", "[distância]", T(N(Ming.Crane, 3), N(Ming.Kite, 1)), T(V(Ming.Crane, 2), S(moon, 1)), 3));

            // Caminho dos Leões (41): the lions of the spirit way, terracotta soldiers on watch, foxes.
            leoes.Fauna.Add(Fauna("leoes_do_caminho", "[tanque]", T(N(Ming.Lion, 3), N(Ming.Crane, 1)), T(V(Ming.Lion, 2), S(owl, 1)), 3, null, 30f));
            leoes.Fauna.Add(Fauna("soldados_do_caminho", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Lion, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            leoes.Fauna.Add(Fauna("raposas_do_caminho", "[rápido]", T(N(Ming.Fox, 3), N(Ming.Crane, 1)), T(V(Ming.Fox, 2), S(moon, 1)), 4));

            // Campos de Painço: smoke locusts over the millet (bats), thieving ducks, foxes.
            painco.Fauna.Add(Fauna("gafanhotos_do_painco", "[enxame]", T(N(Ming.Locusts, 3), N(Ming.Duck, 1)), T(V(Ming.Locusts, 2), S(fireflies, 1)), 5));
            painco.Fauna.Add(Fauna("raposas_do_painco", "[rápido]", T(N(Ming.Fox, 3), N(Ming.Locusts, 1)), T(V(Ming.Fox, 2), S(owl, 1)), 4));

            // Rio dos Salgueiros: dragon carps in the shallows (the sand snake), mandarin ducks, mud buffaloes on the banks.
            salgueiros.Fauna.Add(Fauna("carpas_do_rio", "[rápido]", T(N(MidBeasts.DragonCarp, 3), N(Ming.Duck, 1)), T(V(MidBeasts.DragonCarp, 2), S(moon, 1)), 4));
            salgueiros.Fauna.Add(Fauna("patos_mandarins", "[rápido]", T(N(Ming.Duck, 3), N(Ming.Carp, 1)), T(V(Ming.Duck, 2), S(owl, 1)), 4));
            salgueiros.Fauna.Add(Fauna("bufalos_da_margem", "[tanque]", T(N(Ming.Buffalo, 3), N(Ming.Duck, 1)), T(S(herd, 2), V(Ming.Duck, 1)), 3, null, 28f));

            // Eclusa da Lua and Canal das Lanternas (43): the Carpa-Dragão Apagada [rápido], the Barqueiro de Fumaça [elite] (the
            // ill-wind mist), the Lanterna-Peixe [enxame] (the lantern fish), the Tartaruga de Pedra [tanque] (the coconut crab).
            eclusa.Fauna.Add(Fauna("carpas_da_eclusa", "[rápido]", T(N(MidBeasts.DragonCarp, 3), N(Ming.LanternFish, 1)), T(V(MidBeasts.DragonCarp, 2), S(moon, 1)), 4));
            eclusa.Fauna.Add(Fauna("tartarugas_de_pedra", "[tanque]", T(N(Ming.Turtle, 3), N(Ming.Carp, 1)), T(V(Ming.Turtle, 2), V(Ming.Carp, 1)), 3, null, 30f));
            canal.Fauna.Add(Fauna("lanternas_peixe", "[enxame]", T(N(Ming.LanternFish, 3), N(Ming.Carp, 1)), T(V(Ming.LanternFish, 3), S(moon, 1)), 5));
            canal.Fauna.Add(Fauna("barqueiros_de_fumaca", "[elite]", T(N(Ming.Smoke, 3), N(Ming.LanternFish, 1)), T(V(Ming.Smoke, 3), S(owl, 1)), 3, null, 32f));
            canal.Fauna.Add(Fauna("tartarugas_do_canal", "[tanque]", T(N(Ming.Turtle, 3), N(Ming.Smoke, 1)), T(V(Ming.Turtle, 2), V(Ming.Smoke, 1)), 3, null, 30f));

            // Arrozais Espelhados and Moinho d'Água (44): the Búfalo de Lama [tanque] (the peccary), the Pato-Mandarim Ladrão
            // [rápido] (the thief gull), the Gafanhoto de Fumaça [enxame] (bats), the Espantalho Apagado [controle] (a broken lantern).
            arrozais.Fauna.Add(Fauna("bufalos_de_lama", "[tanque]", T(N(Ming.Buffalo, 3), N(Ming.Duck, 1)), T(S(herd, 2), V(Ming.Duck, 1)), 3, null, 28f));
            arrozais.Fauna.Add(Fauna("patos_ladroes", "[rápido]", T(N(Ming.Duck, 3), N(Ming.Locusts, 1)), T(V(Ming.Duck, 2), S(fireflies, 1)), 4));
            arrozais.Fauna.Add(Fauna("gafanhotos_de_fumaca", "[enxame]", T(N(Ming.Locusts, 3), N(Ming.Duck, 1)), T(V(Ming.Locusts, 2), S(fireflies, 2)), 5));
            moinho.Fauna.Add(Fauna("espantalhos_apagados", "[controle]", T(N(Ming.Kite, 3), N(Ming.Buffalo, 1)), T(V(Ming.Kite, 2), S(owl, 1)), 4));
            moinho.Fauna.Add(Fauna("bufalos_do_moinho", "[tanque]", T(N(Ming.Buffalo, 3), N(Ming.Locusts, 1)), T(S(herd, 2), S(fireflies, 1)), 3, null, 28f));

            // Bambuzal do Oeste: spiders in the culms, thief macaques, the potoo.
            bambu.Fauna.Add(Fauna("aranhas_do_bambu", "[controle]", T(N(Ming.Spider, 3), N(Ming.Potoo, 1)), T(V(Ming.Spider, 2), S(moon, 1)), 4));
            bambu.Fauna.Add(Fauna("macacos_do_bambu", "[rápido]", T(N(Ming.Monkey, 3), N(Ming.Spider, 1)), T(V(Ming.Monkey, 2), S(werewolf, 1)), 3, null, 28f));

            // Colinas de Chá: paper cranes over the rows, foxes, smoke locusts.
            cha.Fauna.Add(Fauna("grous_do_cha", "[distância]", T(N(Ming.Crane, 3), N(Ming.Fox, 1)), T(V(Ming.Crane, 2), S(owl, 1)), 3));
            cha.Fauna.Add(Fauna("gafanhotos_do_cha", "[enxame]", T(N(Ming.Locusts, 3), N(Ming.Crane, 1)), T(V(Ming.Locusts, 2), S(fireflies, 1)), 5));

            // Fossos de Terracota (41) and Vale dos Guerreiros (45): the Soldado de Terracota [tanque], the
            // Cavalo de Terracota [rápido] (the wolf's body), lions and paper kites.
            fossos.Fauna.Add(Fauna("soldados_de_terracota", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Lion, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            fossos.Fauna.Add(Fauna("cavalos_de_terracota", "[rápido]", T(N(MidBeasts.TerracottaHorse, 3), N(Ming.Terracotta, 1)), T(V(MidBeasts.TerracottaHorse, 2), S(werewolf, 1)), 4));
            guerreiros.Fauna.Add(Fauna("guerreiros_de_terracota", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Horse, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            guerreiros.Fauna.Add(Fauna("arqueiros_de_papel", "[distância]", T(N(Ming.Kite, 3), N(Ming.Terracotta, 1)), T(V(Ming.Kite, 2), S(owl, 1)), 4));
            guerreiros.Fauna.Add(Fauna("cavalos_do_vale", "[rápido]", T(N(MidBeasts.TerracottaHorse, 3), N(Ming.Lion, 1)), T(V(MidBeasts.TerracottaHorse, 2), S(werewolf, 1)), 4));

            // Jardins em Terraço and Torres Brancas (42): the Espelho Quebrado [distância] (a broken lantern), the Íbis de Luz
            // Apagada [rápido] (the hawk), the Guardião de Cal [tanque] (the golem), the Raio Torto [controle] (the ill-wind mist).
            jardins.Fauna.Add(Fauna("ibis_de_luz", "[rápido]", T(N(Ming.Crane, 3), N(Ming.Kite, 1)), T(V(Ming.Crane, 2), S(moon, 1)), 4));
            jardins.Fauna.Add(Fauna("espelhos_quebrados", "[distância]", T(N(Ming.Kite, 3), N(Ming.Crane, 1)), T(V(Ming.Kite, 2), S(owl, 1)), 4));
            torres.Fauna.Add(Fauna("guardioes_de_cal", "[tanque]", T(N(Ming.Lion, 3), N(Ming.Smoke, 1)), T(V(Ming.Lion, 2), V(Ming.Smoke, 1)), 3, null, 30f));
            torres.Fauna.Add(Fauna("raios_tortos", "[controle]", T(N(Ming.Smoke, 3), N(Ming.Kite, 1)), T(V(Ming.Smoke, 2), S(owl, 1)), 4));
            torres.Fauna.Add(Fauna("espelhos_das_torres", "[distância]", T(N(Ming.Kite, 3), N(Ming.Lion, 1)), T(V(Ming.Kite, 2), S(bats, 1)), 4));
            espelhos.Fauna.Add(Fauna("espelhos_do_atalho", "[distância]", T(N(Ming.Kite, 3), N(Ming.Smoke, 1)), T(V(Ming.Kite, 2), S(owl, 1)), 4));
            espelhos.Fauna.Add(Fauna("guardioes_das_colinas", "[tanque]", T(N(Ming.Lion, 3), N(Ming.Crane, 1)), T(V(Ming.Lion, 2), S(bats, 1)), 3, null, 30f));

            // Ravinas de Loess: scorpions in the yellow earth, vultures, sand snakes.
            loess.Fauna.Add(Fauna("escorpioes_do_loess", "[controle]", T(N(Ming.Scorpion, 3), N(Ming.Carp, 1)), T(V(Ming.Scorpion, 2), S(owl, 1)), 4));
            loess.Fauna.Add(Fauna("abutres_do_loess", "[distância]", T(N(Ming.Vulture, 3), N(Ming.Scorpion, 1)), T(V(Ming.Vulture, 2), V(Ming.Scorpion, 1)), 3));

            // Picos de Calcário and Picos do Nian: thief macaques and the nian's cubs, rolling stones, bats in the caves.
            calcario.Fauna.Add(Fauna("macacos_do_calcario", "[rápido]", T(N(Ming.Monkey, 3), N(Ming.Bats, 1)), T(V(Ming.Monkey, 2), S(bats, 1)), 3, null, 28f));
            calcario.Fauna.Add(Fauna("pedras_do_calcario", "[controle]", T(N(Ming.Stone, 3), N(Ming.Crow, 1)), T(V(Ming.Stone, 2), S(bats, 1)), 3, null, 28f));
            nian.Fauna.Add(Fauna("filhotes_de_nian", "[elite]", T(N(Ming.Nian, 3), N(Ming.Stone, 1)), T(V(Ming.Nian, 3), S(werewolf, 1)), 3, null, 32f));
            nian.Fauna.Add(Fauna("morcegos_do_nian", "[enxame]", T(N(Ming.Bats, 3), N(Ming.Crow, 1)), T(V(Ming.Bats, 2), S(bats, 2)), 5));

            // Cristas dos Pinheiros and Vale da Névoa: wolves in the pines, crows, the mist.
            pinheiros.Fauna.Add(Fauna("lobos_dos_pinheiros", "[rápido]", T(N(Ming.Horse, 3), N(Ming.Crow, 1)), T(V(Ming.Horse, 2), S(werewolf, 1)), 4));
            pinheiros.Fauna.Add(Fauna("corvos_dos_pinheiros", "[distância]", T(N(Ming.Crow, 3), N(Ming.Monkey, 1)), T(V(Ming.Crow, 2), S(owl, 1)), 4));
            nevoa.Fauna.Add(Fauna("neblinas_do_vale", "[controle]", T(N(Ming.Smoke, 3), N(Ming.Crow, 1)), T(V(Ming.Smoke, 2), S(owl, 1)), 4));
            nevoa.Fauna.Add(Fauna("macacos_da_nevoa", "[rápido]", T(N(Ming.Monkey, 3), N(Ming.Horse, 1)), T(V(Ming.Monkey, 2), S(werewolf, 1)), 3, null, 28f));

            // Muralha das Mil Torres (45): the Soldado de Terracota [tanque], the Arqueiro de Terracota [distância] (paper kites),
            // the Cavalo de Terracota [rápido] (the wolf), the Filhote de Nian [elite] (the baboon); bats in the towers.
            muralha.Fauna.Add(Fauna("soldados_da_muralha", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Kite, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            muralha.Fauna.Add(Fauna("arqueiros_de_terracota", "[distância]", T(N(Ming.Kite, 3), N(Ming.Terracotta, 1)), T(V(Ming.Kite, 2), S(bats, 1)), 4));
            quinhentos.Fauna.Add(Fauna("cavalos_da_muralha", "[rápido]", T(N(MidBeasts.TerracottaHorse, 3), N(Ming.Terracotta, 1)), T(V(MidBeasts.TerracottaHorse, 2), S(werewolf, 1)), 4));
            quinhentos.Fauna.Add(Fauna("morcegos_das_torres_de_vigia", "[enxame]", T(N(Ming.Bats, 3), N(Ming.Kite, 1)), T(V(Ming.Bats, 2), S(bats, 2)), 5));
            muralhaOeste.Fauna.Add(Fauna("soldados_do_oeste", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Horse, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            muralhaOeste.Fauna.Add(Fauna("pedras_da_muralha", "[controle]", T(N(Ming.Stone, 3), N(Ming.Crow, 1)), T(V(Ming.Stone, 2), S(bats, 1)), 3, null, 28f));
            portao.Fauna.Add(Fauna("filhotes_do_portao", "[elite]", T(N(Ming.Nian, 3), N(Ming.Terracotta, 1)), T(V(Ming.Nian, 3), S(werewolf, 1)), 3, null, 32f));
            portao.Fauna.Add(Fauna("arqueiros_do_portao", "[distância]", T(N(Ming.Kite, 3), N(Ming.Horse, 1)), T(V(Ming.Kite, 2), S(bats, 1)), 4));

            // Cidadela dos Quatro Gulosos (46): the Servos de Taotie [tanque] (the terracotta soldier), the Bolhas de Hundun [controle]
            // (the ill-wind mist), the Filhotes de Qiongqi [rápido] (the wolf), the Guarda de Taowu [elite] (the stone lion); no boss yet.
            cidadela.Fauna.Add(Fauna("servos_de_taotie", "[tanque]", T(N(Ming.Terracotta, 3), N(Ming.Lion, 1)), T(V(Ming.Terracotta, 2), S(werewolf, 1)), 3, null, 30f));
            cidadela.Fauna.Add(Fauna("bolhas_de_hundun", "[controle]", T(N(Ming.Smoke, 3), N(Ming.Kite, 1)), T(V(Ming.Smoke, 3), S(owl, 1)), 4));
            cidadela.Fauna.Add(Fauna("filhotes_de_qiongqi", "[rápido]", T(N(Ming.Horse, 3), N(Ming.Smoke, 1)), T(V(Ming.Horse, 2), S(werewolf, 1)), 4));
            cidadela.Fauna.Add(Fauna("guarda_de_taowu", "[elite]", T(N(Ming.Lion, 3), N(Ming.Terracotta, 1)), T(V(Ming.Lion, 3), S(bats, 1)), 3, null, 32f));
        }
    }
}
