using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 4 — Grutas do Limiar as ONE continuous underground map of 2400 × 2400 m (Docs/Mapas100.md §0.10), no scene
    /// change between the Vila das Lamparinas and the halls around it. The deeper (south) and the farther from the Vila, the
    /// harder; five rows of halls, each row a few hundred metres deeper:
    /// <list type="bullet">
    /// <item>row 1 (north, under the surface): the <b>Lago do Limiar</b> (19, nv 13–15: its hand-made map embedded, the whole
    /// <b>Vila das Lamparinas</b> on the north wall, the dark lake, the two tunnels), the <b>Salão de Tages</b> east of it (20,
    /// 14–18: the Nicho da Ponte, the stream and the hall of columns embedded), the <b>Boca do Hahya</b> (14–16) and, farther
    /// out, the <b>Fendas Ocidentais</b> and the <b>Terraços de Tufo</b> (19–24); under the north wall two long galleries
    /// (14–16) run from the Vila's hall to the gates: the <b>Galeria do Hahya</b> to the west edge (up to the Passo do Hahya,
    /// 14) and the <b>Galeria da Ravina</b> to the east edge (up to the Ravina das Bocas de Luz, whose gate on Zarafshan's west
    /// edge leads down here);</item>
    /// <item>row 2: <b>Colunata do Lago</b> (15–20, the barred portal of the <b>Ossuário dos Artífices</b>), <b>Campo de
    /// Cristais</b> and <b>Lago dos Peixes-Lanterna</b> (17–22), <b>Bosque de Cogumelos</b> and <b>Abismo Oriental</b>
    /// (20–26);</item>
    /// <item>row 3: <b>Jardim de Fungos</b> (22–27), <b>Prados de Musgo</b> and <b>Galerias Rosadas</b> (23–28), <b>Ravina de
    /// Cristal</b> and <b>Fenda do Bronze</b> (27–32);</item>
    /// <item>row 4, the four galleries in a cross: <b>Campo das Pedras-Dragão</b> (21, 32–38, runic lamp: twelve vishapakar, the
    /// outpost Vishapakar Grande) with the <b>Fontes do Vishap</b>, the <b>Mercado dos Túneis</b> of the kobolds (22, 34–40,
    /// race city and return city of 22–23) in the middle, the <b>Cânion da Roda</b> (23, 38–46, eternal-fire oil: the wheel
    /// of twelve jars, the Torre das Jarras) and its aqueducts;</item>
    /// <item>row 5, the bottom: the <b>Ninho de Mahaway</b> (24, 42–50, wings: the giant shaft, nests on the ledges, the
    /// Última Ponte de Corda) between the <b>Escarpas da Revoada</b>, the <b>Aquedutos Profundos</b> and the <b>Abismo sem
    /// Fundo</b>.</item>
    /// </list>
    /// Chasms are carved "rivers" drawn as a black void with rope bridges (<see cref="OverworldChunkBuilder"/>, cave part); tall
    /// tuff walls along the borders of the halls (gaps at every trail) suggest the vault without a real ceiling. Monsters reuse
    /// the cave definitions (cave bats, lantern fish, cave ooze, dark crow, spider, stone golem, broken lantern, potoo, crested hawk, peccary, fire
    /// sparks and the night species of <see cref="NightVariantRules"/>: Ubır is the vampire bat).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string LimiarId = "overworld_limiar";

        public static readonly Vector2 TagesOffset = new Vector2(500f, -60f);

        /// <summary>The Mercado dos Túneis (22), the kobolds' race city of the caves.</summary>
        public const string MercadoTuneisMapId = "map_022_mercado_tuneis";

        /// <summary>The Campo das Pedras-Dragão (21), the vishapakar meadows of the caves.</summary>
        public const string PedrasDragaoMapId = "map_021_pedras_dragao";

        /// <summary>O Engarrafador Guloso's arena (22, map boss): the stone ring in the gallery east of the market, off the rails, by its trail.</summary>
        public static readonly Vector2 EngarrafadorGulosoArena = new Vector2(160f, -1464f);

        /// <summary>O Vishap Adormecido's arena (21, map boss): the stone ring on the Prados de Musgo, by the trail, the one area of the Dragonstone Field that asks for no lamp.</summary>
        public static readonly Vector2 VishapAdormecidoArena = new Vector2(-476f, -1010f);
        public static readonly Vector2 PedrasDragaoPost = new Vector2(-500f, -1475f);
        public static readonly Vector2 MercadoTuneisPost = new Vector2(0f, -1475f);
        public static readonly Vector2 TorreJarrasPost = new Vector2(500f, -1475f);
        public static readonly Vector2 UltimaPontePost = new Vector2(0f, -1900f);
        public static readonly Vector2 OssuaryPortal = new Vector2(-180f, -500f);

        /// <summary>The beasts of the caves (the Lodo de Gruta and the Corvo-da-Treva their own; the other stand-ins follow Mapas100 19–24).</summary>
        private static class LimiarBeasts
        {
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Fish = "enemy_cave_lantern_fish";
            public const string Spider = PeriodSpawnTable.ForestSpider;
            public const string Golem = PeriodSpawnTable.MountainGolem;
            public const string Lantern = "enemy_desert_broken_lantern";
            public const string Potoo = NightVariantRules.PotooEnemyId;
            public const string Hawk = "enemy_mountain_crested_hawk";
            public const string Boar = NightVariantRules.PeccaryEnemyId;
            public const string Scorpion = PeriodSpawnTable.DesertScorpion;
            public const string Sparks = NightVariantRules.FireflySwarmEnemyId;
            public const string Ooze = "enemy_cave_ooze";
            public const string Crow = "enemy_cave_dark_crow";
        }

        private static OverworldLayout Limiar()
        {
            OverworldLayout w = NewRegion(LimiarId, MapRegion.GrutasDoLimiar, Rect.MinMaxRect(-1200f, -2100f, 1200f, 300f));
            w.World.Biome = BiomeType.Caves;
            const string lago = MapLayouts.LagoLimiarId;
            const string tages = MapLayouts.TagesId;
            const string pedras = "map_021_pedras_dragao";
            const string mercado = "map_022_mercado_tuneis";
            const string canion = "map_023_canion_roda";
            const string ninho = "map_024_mahaway";
            const string under = MapRegionTypes.Underground;
            const string torch = MapRequirementTags.TorchRequired;

            // ---------------- Zones: 5 columns × 5 rows, tiling the region ----------------
            // Columns x: −1200 | −700 | −300 | 300 | 700 | 1200. Rows z: 300 | −350 | −800 | −1250 | −1700 | −2100.
            OverworldZone fendas = Zone(w, "fendas_ocidentais", null, lago, -1200f, -350f, -700f, 150f, OverworldTerrain.Chasm, 19, 24, 17,
                "Western Fissures", "Fendas Ocidentais", MapRegionTypes.Mountain, torch, 95f);
            OverworldZone boca = Zone(w, "boca_do_hahya", null, lago, -700f, -350f, -300f, 150f, OverworldTerrain.CaveMouth, 14, 16, 12,
                "Hahya Mouth", "Boca do Hahya", under, torch, 85f);
            OverworldZone lagoZone = Zone(w, "lago_do_limiar", lago, null, -300f, -350f, 300f, 300f, OverworldTerrain.LakeShore, 13, 15, 12,
                "Threshold Lake", "Lago do Limiar", under, torch, 80f);
            OverworldZone tagesZone = Zone(w, "salao_de_tages", tages, null, 300f, -350f, 700f, 150f, OverworldTerrain.StalactiteHall, 14, 18, 13,
                "Tages' Hall", "Salão de Tages", under, torch, 85f);
            OverworldZone terracos = Zone(w, "terracos_de_tufo", null, tages, 700f, -350f, 1200f, 150f, OverworldTerrain.TuffTerraces, 19, 24, 17,
                "Tuff Terraces", "Terraços de Tufo", under, torch, 95f);
            // The two long galleries under the north wall, out to the gates: west up to the Passo do Hahya (14), east up to the
            // Ravina das Bocas de Luz (Zarafshan, whose west gate leads down here).
            OverworldZone galeriaHahya = Zone(w, "galeria_do_hahya", null, lago, -1200f, 150f, -300f, 300f, OverworldTerrain.CaveMouth, 14, 16, 12,
                "Hahya Gallery", "Galeria do Hahya", under, torch, 70f);
            OverworldZone galeriaRavina = Zone(w, "galeria_da_ravina", null, lago, 300f, 150f, 1200f, 300f, OverworldTerrain.CaveMouth, 14, 16, 12,
                "Ravine Gallery", "Galeria da Ravina", under, torch, 70f);

            OverworldZone cogumelos = Zone(w, "bosque_de_cogumelos", null, lago, -1200f, -800f, -700f, -350f, OverworldTerrain.MushroomGrove, 20, 26, 18,
                "Mushroom Grove", "Bosque de Cogumelos", under, torch, 90f);
            OverworldZone cristais = Zone(w, "campo_de_cristais", null, lago, -700f, -800f, -300f, -350f, OverworldTerrain.CrystalField, 17, 22, 15,
                "Crystal Field", "Campo de Cristais", under, torch, 90f);
            OverworldZone colunata = Zone(w, "colunata_do_lago", null, lago, -300f, -800f, 300f, -350f, OverworldTerrain.ColumnForest, 15, 20, 13,
                "Lake Colonnade", "Colunata do Lago", under, torch, 85f);
            OverworldZone peixes = Zone(w, "lago_dos_peixes", null, tages, 300f, -800f, 700f, -350f, OverworldTerrain.LakeShore, 17, 22, 15,
                "Lantern Fish Lake", "Lago dos Peixes-Lanterna", under, torch, 90f);
            OverworldZone abismoLeste = Zone(w, "abismo_oriental", null, tages, 700f, -800f, 1200f, -350f, OverworldTerrain.Chasm, 20, 26, 18,
                "Eastern Abyss", "Abismo Oriental", MapRegionTypes.Mountain, torch, 95f);

            OverworldZone ravinaCristal = Zone(w, "ravina_de_cristal", null, pedras, -1200f, -1250f, -700f, -800f, OverworldTerrain.CrystalField, 27, 32, 25,
                "Crystal Ravine", "Ravina de Cristal", under, spacing: 95f);
            OverworldZone prados = Zone(w, "prados_de_musgo", null, pedras, -700f, -1250f, -300f, -800f, OverworldTerrain.MossMeadow, 23, 28, 21,
                "Moss Meadows", "Prados de Musgo", under, spacing: 90f);
            OverworldZone fungos = Zone(w, "jardim_de_fungos", null, mercado, -300f, -1250f, 300f, -800f, OverworldTerrain.MushroomGrove, 22, 27, 20,
                "Fungus Garden", "Jardim de Fungos", under, spacing: 90f);
            OverworldZone rosadas = Zone(w, "galerias_rosadas", null, canion, 300f, -1250f, 700f, -800f, OverworldTerrain.TuffTerraces, 23, 28, 21,
                "Pink Galleries", "Galerias Rosadas", under, spacing: 90f);
            OverworldZone fendaBronze = Zone(w, "fenda_do_bronze", null, canion, 700f, -1250f, 1200f, -800f, OverworldTerrain.Chasm, 27, 32, 25,
                "Bronze Rift", "Fenda do Bronze", MapRegionTypes.Mountain, spacing: 95f);

            OverworldZone fontes = Zone(w, "fontes_do_vishap", pedras, null, -1200f, -1700f, -700f, -1250f, OverworldTerrain.LakeShore, 34, 38, 32,
                "Vishap Springs", "Fontes do Vishap", under, MapRequirementTags.RunicLamp, 95f);
            OverworldZone pedrasZone = Zone(w, "campo_pedras_dragao", pedras, null, -700f, -1700f, -300f, -1250f, OverworldTerrain.MossMeadow, 32, 36, 30,
                "Dragonstone Field", "Campo das Pedras-Dragão", under, MapRequirementTags.RunicLamp, 90f);
            OverworldZone mercadoZone = Zone(w, "mercado_dos_tuneis", mercado, null, -300f, -1700f, 300f, -1250f, OverworldTerrain.TunnelMarket, 34, 40, 32,
                "Tunnel Market", "Mercado dos Túneis", under, spacing: 90f);
            OverworldZone canionZone = Zone(w, "canion_da_roda", canion, null, 300f, -1700f, 700f, -1250f, OverworldTerrain.WheelCanyon, 38, 44, 36,
                "Wheel Canyon", "Cânion da Roda", MapRegionTypes.Ruins, MapRequirementTags.EternalFireOil, 90f);
            OverworldZone aquedutos = Zone(w, "aquedutos_da_roda", canion, null, 700f, -1700f, 1200f, -1250f, OverworldTerrain.WheelCanyon, 40, 46, 38,
                "Wheel Aqueducts", "Aquedutos da Roda", MapRegionTypes.Ruins, MapRequirementTags.EternalFireOil, 95f);

            OverworldZone escarpas = Zone(w, "escarpas_da_revoada", null, ninho, -1200f, -2100f, -700f, -1700f, OverworldTerrain.Chasm, 44, 50, 42,
                "Flock Scarps", "Escarpas da Revoada", MapRegionTypes.Mountain, spacing: 95f);
            OverworldZone ninhosOeste = Zone(w, "ninhos_ocidentais", ninho, null, -700f, -2100f, -300f, -1700f, OverworldTerrain.Roost, 42, 48, 40,
                "Western Nests", "Ninhos Ocidentais", MapRegionTypes.Mountain, MapRequirementTags.WingsOrDoubleJump, 90f);
            OverworldZone ninhoZone = Zone(w, "ninho_de_mahaway", ninho, null, -300f, -2100f, 300f, -1700f, OverworldTerrain.Roost, 42, 50, 40,
                "Mahaway's Roost", "Ninho de Mahaway", MapRegionTypes.Mountain, MapRequirementTags.WingsOrDoubleJump, 90f);
            OverworldZone aquedutosFundos = Zone(w, "aquedutos_profundos", null, canion, 300f, -2100f, 700f, -1700f, OverworldTerrain.WheelCanyon, 42, 48, 40,
                "Deep Aqueducts", "Aquedutos Profundos", MapRegionTypes.Ruins, spacing: 90f);
            OverworldZone semFundo = Zone(w, "abismo_sem_fundo", null, ninho, 700f, -2100f, 1200f, -1700f, OverworldTerrain.Chasm, 44, 50, 42,
                "Bottomless Abyss", "Abismo sem Fundo", MapRegionTypes.Mountain, spacing: 95f);

            AddLimiarGalleryFauna(galeriaHahya, galeriaRavina);
            AddLimiarFauna(fendas, boca, lagoZone, tagesZone, terracos, cogumelos, cristais, colunata, peixes, abismoLeste, ravinaCristal, prados, fungos,
                rosadas, fendaBronze, fontes, pedrasZone, mercadoZone, canionZone, aquedutos, escarpas, ninhosOeste, ninhoZone, aquedutosFundos, semFundo);

            // ---------------- The hand-made maps, embedded (their dressing comes from the cave scatter) ----------------
            MapLayout lagoSrc = MapLayouts.For(lago);
            w.Vila = Embed(w, lagoSrc, Vector2.zero, withProps: false, authored: false);
            w.Vila.IsReturnCity = true;
            MapLayout tagesSrc = MapLayouts.For(tages);
            Embed(w, tagesSrc, TagesOffset, withProps: false, authored: false);

            // ---------------- The outposts of 21–24 and the kobolds' market ----------------
            var postSafe = Rect.MinMaxRect(-15f, -14f, 16f, 15f);
            Place(w, pedras, LimiarPost("vishapakar_grande", TownShopDressing.Storyteller, TownShopDressing.Wicks), PedrasDragaoPost, postSafe, 18f, false);
            OverworldSettlement market = Place(w, mercado, CityTownRecipes.TunnelMarket(), MercadoTuneisPost, Rect.MinMaxRect(-21f, -20f, 22f, 21f), 24f, true);
            market.IsReturnCity = true;
            Place(w, canion, LimiarPost("torre_das_jarras", TownShopDressing.Pottery, TownShopDressing.Cistern), TorreJarrasPost, postSafe, 18f, false);
            Place(w, ninho, LimiarPost("ultima_ponte", TownShopDressing.OldMiner, TownShopDressing.Wicks), UltimaPontePost, postSafe, 18f, false);

            // ---------------- Water and chasms ----------------
            // The Tages stream runs on north and south of the embedded hall, into the lake of the lantern fish.
            River(w, 7f, new Vector2(524f, 40f), new Vector2(516f, 130f));
            River(w, 7f, new Vector2(524f, -160f), new Vector2(545f, -300f), new Vector2(570f, -470f), new Vector2(586f, -530f));
            River(w, 44f, new Vector2(580f, -545f), new Vector2(636f, -575f));
            // A black lake in the colonnade and pools of the moss meadows.
            River(w, 26f, new Vector2(120f, -660f), new Vector2(160f, -680f));
            River(w, 14f, new Vector2(-600f, -1340f), new Vector2(-580f, -1352f));
            River(w, 12f, new Vector2(-400f, -1580f), new Vector2(-385f, -1590f));
            River(w, 18f, new Vector2(-1000f, -1400f), new Vector2(-960f, -1420f));
            River(w, 16f, new Vector2(-860f, -1580f), new Vector2(-830f, -1560f));
            River(w, 20f, new Vector2(-620f, -925f), new Vector2(-590f, -940f));
            // The wheel's pool and its aqueduct east to the region's wall (a plank bridge where the east trail crosses it).
            River(w, 24f, new Vector2(340f, -1640f), new Vector2(400f, -1650f));
            MapRiver aqueduct = River(w, 5f, new Vector2(400f, -1650f), new Vector2(560f, -1620f), new Vector2(680f, -1560f), new Vector2(820f, -1600f),
                new Vector2(950f, -1570f), new Vector2(1080f, -1600f), new Vector2(1200f, -1590f));
            Crossing(aqueduct, new Vector2(950f, -1570f), 4f, true);

            // Chasms: carved beds drawn as a black void, crossed only on rope bridges.
            MapRiver fissure = River(w, 12f, new Vector2(-1200f, -200f), new Vector2(-1060f, -170f), new Vector2(-950f, -220f), new Vector2(-820f, -180f),
                new Vector2(-720f, -230f));
            Crossing(fissure, new Vector2(-950f, -220f), 4f, true);
            MapRiver eastAbyss = River(w, 14f, new Vector2(720f, -470f), new Vector2(860f, -540f), new Vector2(1040f, -580f), new Vector2(1200f, -560f));
            Crossing(eastAbyss, new Vector2(950f, -560f), 4f, true);
            MapRiver bronzeRift = River(w, 12f, new Vector2(760f, -1000f), new Vector2(880f, -1040f), new Vector2(950f, -1000f), new Vector2(1060f, -960f),
                new Vector2(1200f, -1010f));
            Crossing(bronzeRift, new Vector2(950f, -1000f), 4f, true);
            MapRiver crystalRift = River(w, 10f, new Vector2(-1200f, -1060f), new Vector2(-1080f, -1020f), new Vector2(-950f, -1060f), new Vector2(-840f, -1030f),
                new Vector2(-760f, -1080f));
            Crossing(crystalRift, new Vector2(-950f, -1060f), 4f, true);
            MapRiver scarps = River(w, 12f, new Vector2(-1200f, -1850f), new Vector2(-1080f, -1820f), new Vector2(-950f, -1880f), new Vector2(-820f, -1840f),
                new Vector2(-700f, -1870f));
            Crossing(scarps, new Vector2(-950f, -1880f), 4f, true);
            MapRiver bottomless = River(w, 14f, new Vector2(700f, -1850f), new Vector2(820f, -1880f), new Vector2(950f, -1840f), new Vector2(1080f, -1880f),
                new Vector2(1200f, -1850f));
            Crossing(bottomless, new Vector2(950f, -1840f), 4f, true);
            // Mahaway's shaft: the giant well under the last rope bridge.
            MapRiver shaft = River(w, 40f, new Vector2(-40f, -2010f), new Vector2(40f, -2016f));
            foreach (MapRiver chasm in new[] { fissure, eastAbyss, bronzeRift, crystalRift, scarps, bottomless, shaft })
            {
                OverworldChunkBuilder.MarkAbyss(w, chasm);
            }

            // ---------------- Gates out of the region ----------------
            // Up to the Passo do Hahya (14) and up to the Ravina das Bocas de Luz (Zarafshan, its gate leads to the 19).
            Gate(w, "exit_hahya", MapExitKind.Map, MapLayouts.HahyaId, Rect.MinMaxRect(-1200f, 182f, -1197f, 198f), new Vector2(-1191f, 190f), Vector2.right);
            Gate(w, "exit_zarafshan", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(1197f, 182f, 1200f, 198f), new Vector2(1191f, 190f), Vector2.left);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(60f, -32f, 64f, -28f), new Vector2(62f, -38f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, 62f, -30f, 0f, 0.45f);
            // The barred portal of the Ossuário dos Artífices: walking into it starts the dungeon run (MapModeController.OssuaryExitId).
            Gate(w, "exit_ossuario", MapExitKind.WorldMap, null, Rect.MinMaxRect(OssuaryPortal.x - 3f, OssuaryPortal.y - 4f, OssuaryPortal.x + 3f, OssuaryPortal.y - 1f),
                OssuaryPortal + new Vector2(0f, -9f), Vector2.up);
            OverworldChunkBuilder.AddCavePiece(w, OverworldDecor.OssuaryPortal, OssuaryPortal, 0f, 1f, 0, true);

            // ---------------- Trails ----------------
            // The Vila's tunnels: west to the Boca do Hahya and its gate, east to the Nicho da Ponte (20).
            Path(w, 3.5f, -100f, -60f, -220f, -80f, -420f, -60f, -500f, 60f, -520f, 190f, -1191f, 190f);
            Path(w, 3.5f, 100f, -60f, 250f, -62f, 400f, -60f);
            // Up to the Ravina das Bocas de Luz, round the north cliff of the Vila; the island portal beside the town.
            Path(w, 3.5f, 100f, -60f, 180f, -40f, 222f, 100f, 320f, 190f, 1191f, 190f);
            Path(w, 3f, 62f, -58f, 62f, -42f);
            // East of the Nicho: round Tages' hall to the terraces.
            Path(w, 3.5f, 560f, -60f, 640f, -40f, 950f, -60f, 950f, -350f);
            // The main road south: round the lake, the colonnade, the fungus garden, into the market from the north.
            Path(w, 4f, 0f, -170f, 0f, -350f, -20f, -560f, 0f, -800f, 0f, -1000f, 0f, -1250f, 0f, -1453f);
            // The Nicho's south road: the lake of the lantern fish, the pink galleries, the Torre das Jarras.
            Path(w, 3.5f, 500f, -160f, 480f, -350f, 470f, -600f, 500f, -800f, 480f, -1000f, 500f, -1250f, 500f, -1457f);
            // The west road: the crystal field, the moss meadows, the Vishapakar Grande.
            Path(w, 3.5f, -420f, -60f, -500f, -350f, -480f, -560f, -520f, -800f, -500f, -1000f, -500f, -1250f, -500f, -1457f);
            // The outer roads (fissures, mushrooms, crystal ravine, springs, scarps; terraces, abysses, bronze rift, aqueducts).
            Path(w, 3.5f, -420f, -60f, -900f, -80f, -950f, -220f, -950f, -350f, -950f, -800f, -950f, -1060f, -950f, -1250f, -950f, -1475f,
                -950f, -1600f, -950f, -1880f, -950f, -1960f);
            Path(w, 3.5f, 950f, -350f, 950f, -560f, 950f, -800f, 950f, -1000f, 950f, -1250f, 950f, -1475f, 950f, -1570f, 950f, -1840f, 950f, -1960f);
            // Cross galleries of every row.
            Path(w, 3f, -950f, -560f, -480f, -560f, -20f, -560f, 250f, -600f, 470f, -600f, 950f, -640f);
            Path(w, 3f, -950f, -1000f, -500f, -1000f, 0f, -1000f, 480f, -1000f, 800f, -1090f, 950f, -1100f);
            Path(w, 3.5f, -950f, -1475f, -518f, -1475f);
            Path(w, 3.5f, -482f, -1475f, -21f, -1475f);
            Path(w, 3.5f, 22f, -1475f, 482f, -1475f);
            Path(w, 3.5f, 518f, -1475f, 950f, -1475f);
            // Down to the bottom: from the market's south gate to the Última Ponte; the bottom gallery east and west.
            Path(w, 3.5f, 0f, -1496f, 0f, -1700f, 0f, -1882f);
            Path(w, 3f, -18f, -1900f, -500f, -1760f, -720f, -1930f, -950f, -1960f);
            Path(w, 3f, 18f, -1900f, 500f, -1930f, 950f, -1960f);
            // The Ossuário's spur and the spurs to the wheel and the springs.
            Path(w, 3f, OssuaryPortal.x, -560f, OssuaryPortal.x, OssuaryPortal.y - 9f);
            Path(w, 3f, 500f, -1493f, 470f, -1600f);

            // ---------------- Landmarks ----------------
            Landmark(w, MapLandmarkKind.Signpost, -414f, -54f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -350f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, -14f, -566f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -1000f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 944f, -354f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, -944f, -1006f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -1150f, 210f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 1150f, 210f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -540f, -620f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.Campfire, 30f, -1020f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.Campfire, 920f, -1500f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -920f, -1500f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -760f, -1120f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, 1040f, -150f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.StoneRing, EngarrafadorGulosoArena.x, EngarrafadorGulosoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, VishapAdormecidoArena.x, VishapAdormecidoArena.y, 0f, 1.1f);

            // ---------------- The areas' own monuments ----------------
            // 21: the twelve vishapakar of the dragonstone field (the biggest one is the outpost's lamp stone).
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f + 0.2f;
                float r = 120f + 30f * TownStyle.Hash01("vishapakar", i);
                Vector2 at = PedrasDragaoPost + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.9f) * r;
                OverworldChunkBuilder.AddCavePiece(w, OverworldDecor.Vishapakar, at, i * 47f, 1.1f + 0.5f * TownStyle.Hash01("vishapakar", i + 20), i);
            }

            OverworldChunkBuilder.AddCavePiece(w, OverworldDecor.Vishapakar, PedrasDragaoPost + new Vector2(-30f, 35f), 0f, 2.2f, 1);
            // 23: the wheel of twelve jars on its pool.
            OverworldChunkBuilder.AddCavePiece(w, OverworldDecor.JarWheel, new Vector2(370f, -1612f), 90f, 1f, 0, true);
            // 24: the nests on the ledges round the shaft.
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Vector2 at = new Vector2(0f, -2013f) + new Vector2(Mathf.Cos(a) * 62f, Mathf.Sin(a) * 40f);
                OverworldChunkBuilder.AddCavePiece(w, OverworldDecor.Nest, at, i * 61f, 1.3f);
            }

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            // Like the Serra: no generated clearing whose plateau would tilt a trail or that sits on sloping ground.
            TrimSteepClearings(w);
            return w;
        }

        // ------------------------------------------------------------------
        // The settlements of the deep caves
        // ------------------------------------------------------------------

        /// <summary>A small outpost cut in the tuff (the Nicho da Ponte's model): the lamp niche, two tuff houses, the Cais da Barca.</summary>
        private static CityTownRecipe LimiarPost(string id, TownShopDressing west, TownShopDressing east)
        {
            var r = new CityTownRecipe
            {
                CityId = id, Region = BiomeType.Caves, PlazaRadius = 4.5f, StreetWidth = 3.5f, SidewalkWidth = 0.8f, WalkHalfExtent = 16f,
                EntryArm = CityArm.North, AutoYards = false, Backdrop = false, NightLights = 4, AmbientNight = 0.65f, LampStrings = true, LampSpacing = 5.5f,
            };
            r.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.North, 12f, exitToMap: true));
            r.Lots.Add(new CityLotRecipe
            {
                Id = id + "_nicho", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, -9.5f), Front = Vector2.up, Width = 4.5f, Depth = 5f, Porch = 0f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.PinkTuff, TrimColor = TownSwatch.Basalt, RoofColor = TownSwatch.Copper, JoineryColor = TownSwatch.WoodDark,
                    AccentColor = TownSwatch.Copper, WallHeight = 3f,
                },
            });
            r.Lots.Add(new CityLotRecipe { Id = id + "_cais", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            r.Lots.Add(new CityLotRecipe
            {
                Id = id + "_casa", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 8.5f, Width = 5f, Depth = 4.4f, Porch = 1f,
                Spec = LimiarTuffHouse(east, 2.8f),
            });
            r.Lots.Add(new CityLotRecipe
            {
                Id = id + "_oficina", Kind = CityLotKind.House, Arm = CityArm.West, Side = CityArm.South, Along = 8.5f, Width = 5f, Depth = 4.4f, Porch = 1f,
                Spec = LimiarTuffHouse(west, 2.6f),
            });
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Jugs, Position = new Vector2(3f, 5.8f), Yaw = 20f, Scale = 1f });
            return r;
        }

        private static TownBuildingSpec LimiarTuffHouse(TownShopDressing dressing, float wallHeight)
        {
            return new TownBuildingSpec
            {
                WallColor = TownSwatch.PinkTuff, TrimColor = TownSwatch.Basalt, RoofColor = TownSwatch.PinkTuff, JoineryColor = TownSwatch.WoodDark,
                AccentColor = TownSwatch.Copper, Roof = TownRoofKind.Rock, Dressing = dressing, WallHeight = wallHeight, FrontWindows = 1,
            };
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only)
        // ------------------------------------------------------------------

        private static void AddLimiarGalleryFauna(OverworldZone hahya, OverworldZone ravina)
        {
            NightSpecies ubir = NightVariantRules.MorcegoVampiro;
            hahya.Fauna.Add(Fauna("galeria_morcegos", "[enxame]", T(N(LimiarBeasts.Bats, 4)), T(V(LimiarBeasts.Bats, 3), S(ubir, 1)), 5, null, 22f));
            hahya.Fauna.Add(Fauna("galeria_aranhas", "[rápido]", T(N(LimiarBeasts.Spider, 3), N(LimiarBeasts.Bats, 1)), T(V(LimiarBeasts.Spider, 2), S(ubir, 1)), 4));
            ravina.Fauna.Add(Fauna("galeria_urutaus", "[distância] [enxame]", T(N(LimiarBeasts.Potoo, 2), N(LimiarBeasts.Bats, 2)), T(V(LimiarBeasts.Potoo, 2), S(ubir, 1)), 4));
            ravina.Fauna.Add(Fauna("galeria_pedras", "[tanque]", T(N(LimiarBeasts.Golem, 2), N(LimiarBeasts.Spider, 1)), T(V(LimiarBeasts.Golem, 2), S(ubir, 1)), 3, null, 28f));
        }

        private static void AddLimiarFauna(OverworldZone fendas, OverworldZone boca, OverworldZone lago, OverworldZone tages, OverworldZone terracos,
            OverworldZone cogumelos, OverworldZone cristais, OverworldZone colunata, OverworldZone peixes, OverworldZone abismoLeste,
            OverworldZone ravinaCristal, OverworldZone prados, OverworldZone fungos, OverworldZone rosadas, OverworldZone fendaBronze,
            OverworldZone fontes, OverworldZone pedras, OverworldZone mercado, OverworldZone canion, OverworldZone aquedutos,
            OverworldZone escarpas, OverworldZone ninhosOeste, OverworldZone ninho, OverworldZone aquedutosFundos, OverworldZone semFundo)
        {
            const string bats = LimiarBeasts.Bats;
            const string fish = LimiarBeasts.Fish;
            const string spider = LimiarBeasts.Spider;
            const string golem = LimiarBeasts.Golem;
            const string lantern = LimiarBeasts.Lantern;
            const string potoo = LimiarBeasts.Potoo;
            const string hawk = LimiarBeasts.Hawk;
            const string boar = LimiarBeasts.Boar;
            const string scorpion = LimiarBeasts.Scorpion;
            const string sparks = LimiarBeasts.Sparks;
            const string ooze = LimiarBeasts.Ooze;
            const string crow = LimiarBeasts.Crow;
            NightSpecies ubir = NightVariantRules.MorcegoVampiro;

            // Row 1 — the Lago do Limiar (19), the Boca do Hahya (the Lodo de Gruta [tanque]), the Salão de Tages (20), the outer fissures and terraces.
            lago.Fauna.Add(Fauna("morcegos_do_limiar", "[enxame]", T(N(bats, 4)), T(V(bats, 3), S(ubir, 1)), 5, null, 22f));
            lago.Fauna.Add(Fauna("peixes_da_margem", "[distância] [enxame]", T(N(fish, 2), N(bats, 2)), T(V(fish, 2), S(ubir, 1)), 4, null, 26f));
            lago.Fauna.Add(Fauna("aranhas_de_gruta", "[rápido]", T(N(spider, 3), N(bats, 1)), T(V(spider, 2), S(ubir, 1)), 4));
            boca.Fauna.Add(Fauna("boca_aranhas", "[rápido]", T(N(spider, 3), N(bats, 1)), T(V(spider, 3), S(ubir, 1)), 4));
            boca.Fauna.Add(Fauna("boca_pedras", "[tanque]", T(N(ooze, 2), N(bats, 2)), T(V(ooze, 2), S(ubir, 1)), 3, null, 28f));
            tages.Fauna.Add(Fauna("estalactites", "[enxame] [controle]", T(N(bats, 3), N(golem, 1)), T(V(bats, 3), S(ubir, 1)), 5, null, 22f));
            tages.Fauna.Add(Fauna("lanternas_quebradas", "[distância] [tanque]", T(N(lantern, 2), N(golem, 2)), T(V(lantern, 2), S(ubir, 1)), 4, null, 28f));
            fendas.Fauna.Add(Fauna("beirais", "[tanque] [enxame]", T(N(golem, 2), N(bats, 2)), T(V(golem, 2), S(ubir, 2)), 4, null, 28f));
            fendas.Fauna.Add(Fauna("urutaus_da_fenda", "[distância]", T(N(potoo, 3), N(bats, 1)), T(V(potoo, 2), S(ubir, 1)), 3));
            terracos.Fauna.Add(Fauna("degraus_rosados", "[rápido] [enxame]", T(N(EarlyBeasts.YoungUbir, 2), N(bats, 2)), T(V(EarlyBeasts.YoungUbir, 2), S(ubir, 1)), 4));
            terracos.Fauna.Add(Fauna("guardas_de_tufo", "[tanque] [distância]", T(N(golem, 2), N(lantern, 1)), T(V(golem, 2), V(lantern, 1)), 3, null, 30f));

            // Row 2 — mushrooms, crystals, the colonnade by the Ossuário, the lantern fish's lake, the eastern abyss.
            cogumelos.Fauna.Add(Fauna("esporos", "[enxame] [controle]", T(N(sparks, 2), N(spider, 2)), T(V(sparks, 2), S(NightVariantRules.VagaLume, 1)), 5, null, 22f));
            cogumelos.Fauna.Add(Fauna("fucadores", "[tanque]", T(N(boar, 3), N(spider, 1)), T(V(boar, 2), S(ubir, 1)), 3, null, 30f));
            cristais.Fauna.Add(Fauna("lascas", "[tanque] [controle]", T(N(golem, 3), N(bats, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 30f));
            cristais.Fauna.Add(Fauna("brilho_roubado", "[distância]", T(N(lantern, 2), N(bats, 2)), T(V(lantern, 2), S(ubir, 1)), 4));
            colunata.Fauna.Add(Fauna("entre_colunas", "[enxame]", T(N(bats, 4)), T(V(bats, 3), S(ubir, 1)), 5, null, 22f));
            colunata.Fauna.Add(Fauna("vigias_do_ossuario", "[tanque] [rápido]", T(N(golem, 2), N(spider, 2)), T(V(golem, 2), V(spider, 1), S(ubir, 1)), 4, null, 28f));
            peixes.Fauna.Add(Fauna("peixes_lanterna", "[distância]", T(N(fish, 3), N(bats, 1)), T(V(fish, 3), S(ubir, 1)), 4, null, 26f));
            peixes.Fauna.Add(Fauna("margem_escura", "[rápido] [enxame]", T(N(spider, 2), N(bats, 2)), T(V(spider, 2), S(ubir, 2)), 4));
            abismoLeste.Fauna.Add(Fauna("borda_do_abismo", "[distância] [enxame]", T(N(potoo, 2), N(bats, 2)), T(V(potoo, 2), S(ubir, 2)), 4));
            abismoLeste.Fauna.Add(Fauna("pedras_da_borda", "[tanque]", T(N(golem, 3)), T(V(golem, 2), S(ubir, 1)), 3, null, 30f));

            // Row 3 — towards the four galleries.
            ravinaCristal.Fauna.Add(Fauna("cristal_vivo", "[tanque] [controle]", T(N(golem, 3), N(lantern, 1)), T(V(golem, 2), V(lantern, 1)), 4, null, 30f));
            ravinaCristal.Fauna.Add(Fauna("ecos", "[enxame]", T(N(bats, 3), N(potoo, 1)), T(V(bats, 3), S(ubir, 1)), 5, null, 22f));
            prados.Fauna.Add(Fauna("enguias_de_fonte", "[rápido] [distância]", T(N(fish, 2), N(spider, 2)), T(V(fish, 2), S(ubir, 1)), 4));
            prados.Fauna.Add(Fauna("sapos_de_pedra", "[tanque]", T(N(golem, 2), N(boar, 2)), T(V(golem, 2), V(boar, 1)), 3, null, 30f));
            fungos.Fauna.Add(Fauna("fungos", "[enxame] [controle]", T(N(sparks, 3), N(spider, 1)), T(V(sparks, 2), S(NightVariantRules.VagaLume, 1)), 5, null, 22f));
            fungos.Fauna.Add(Fauna("toupeiras", "[tanque]", T(N(boar, 3), N(scorpion, 1)), T(V(boar, 2), S(ubir, 1)), 3, null, 30f));
            rosadas.Fauna.Add(Fauna("lagartas_de_pedra", "[enxame] [rápido]", T(N(scorpion, 3), N(bats, 1)), T(V(scorpion, 3), S(ubir, 1)), 5, null, 22f));
            rosadas.Fauna.Add(Fauna("ushabtis", "[tanque]", T(N(golem, 3), N(lantern, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 30f));
            fendaBronze.Fauna.Add(Fauna("bronze_quebrado", "[distância] [tanque]", T(N(lantern, 2), N(golem, 2)), T(V(lantern, 2), V(golem, 1)), 4, null, 28f));
            fendaBronze.Fauna.Add(Fauna("revoada_da_fenda", "[enxame]", T(N(bats, 3), N(hawk, 1)), T(V(bats, 3), S(ubir, 1)), 5, null, 22f));

            // Row 4 — 21 (vishaps), 22 (the market's tunnels: only the Āz-touched, never the people), 23 (the wheel's canyon).
            fontes.Fauna.Add(Fauna("nhang", "[controle] [distância]", T(N(fish, 3), N(spider, 1)), T(V(fish, 2), S(ubir, 1)), 4, null, 26f));
            fontes.Fauna.Add(Fauna("vishaps_jovens", "[elite] [tanque]", T(N(golem, 2), N(boar, 2)), T(V(golem, 2), V(boar, 1)), 3, null, 34f));
            pedras.Fauna.Add(Fauna("pedras_que_acordam", "[elite] [tanque]", T(N(golem, 3), N(fish, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 34f));
            pedras.Fauna.Add(Fauna("enguias", "[rápido] [distância]", T(N(fish, 2), N(spider, 2)), T(V(fish, 2), V(spider, 1)), 4));
            mercado.Fauna.Add(Fauna("tocados_por_az", "[rápido]", T(N(spider, 2), N(scorpion, 2)), T(V(spider, 2), V(scorpion, 2), S(ubir, 1)), 4));
            mercado.Fauna.Add(Fauna("toupeiras_gigantes", "[tanque]", T(N(boar, 3)), T(V(boar, 2), S(ubir, 1)), 3, null, 30f));
            mercado.Fauna.Add(Fauna("fumaca_de_mina", "[controle] [enxame]", T(N(sparks, 2), N(bats, 2)), T(V(sparks, 2), S(NightVariantRules.VagaLume, 1)), 5, null, 22f));
            canion.Fauna.Add(Fauna("filhos_de_ullikummi", "[elite] [tanque]", T(N(golem, 3), N(lantern, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 34f));
            canion.Fauna.Add(Fauna("jarras_rachadas", "[distância]", T(N(lantern, 3), N(fish, 1)), T(V(lantern, 2), V(fish, 1)), 4));
            aquedutos.Fauna.Add(Fauna("gotas_de_treva", "[rápido] [enxame]", T(N(bats, 2), N(sparks, 2)), T(V(bats, 2), S(ubir, 2)), 5, null, 22f));
            aquedutos.Fauna.Add(Fauna("ushabtis_de_bronze", "[tanque]", T(N(golem, 3), N(lantern, 1)), T(V(golem, 3), S(ubir, 1)), 3, null, 32f));

            // Row 5 — Mahaway's flock round the shaft: the Corvos-da-Treva [distância] throw their feather darts from the roosts.
            ninho.Fauna.Add(Fauna("revoada_de_mahaway", "[enxame]", T(N(bats, 3), N(hawk, 2)), T(V(bats, 3), S(ubir, 2)), 5, null, 22f));
            ninho.Fauna.Add(Fauna("morcegoes_de_pedra", "[tanque] [distância]", T(N(golem, 2), N(crow, 2)), T(V(golem, 2), V(crow, 1), S(ubir, 1)), 4, null, 30f));
            ninhosOeste.Fauna.Add(Fauna("corvos_da_treva", "[distância]", T(N(crow, 3), N(hawk, 1)), T(V(crow, 2), S(ubir, 1)), 4));
            ninhosOeste.Fauna.Add(Fauna("ninhada", "[enxame] [rápido]", T(N(bats, 3), N(spider, 1)), T(V(bats, 3), S(ubir, 2)), 5, null, 22f));
            escarpas.Fauna.Add(Fauna("gavioes_da_escarpa", "[rápido] [distância]", T(N(hawk, 3), N(bats, 1)), T(V(hawk, 2), S(ubir, 2)), 4));
            escarpas.Fauna.Add(Fauna("rochas_vivas", "[tanque]", T(N(golem, 3), N(potoo, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 32f));
            aquedutosFundos.Fauna.Add(Fauna("canais_escuros", "[distância] [controle]", T(N(fish, 2), N(lantern, 2)), T(V(fish, 2), V(lantern, 1), S(ubir, 1)), 4));
            aquedutosFundos.Fauna.Add(Fauna("guardas_do_aqueduto", "[tanque]", T(N(golem, 3), N(scorpion, 1)), T(V(golem, 2), S(ubir, 1)), 3, null, 32f));
            semFundo.Fauna.Add(Fauna("sem_fundo", "[enxame] [distância]", T(N(bats, 2), N(hawk, 2)), T(V(bats, 2), S(ubir, 2)), 5, null, 22f));
            semFundo.Fauna.Add(Fauna("pedras_que_caem", "[tanque]", T(N(golem, 3), N(lantern, 1)), T(V(golem, 2), V(lantern, 1)), 3, null, 32f));
        }
    }
}
