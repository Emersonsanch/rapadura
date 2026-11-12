using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 6 — Delta de Jhilpur as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 31–35), no
    /// scene change between Jhilpur and the delta round it. Jhilpur (31, the capital in the roots, the region's return city)
    /// stands at the plaza (0, 0) on the north shore of its lake, 600 m from the north edge where the roads from the Costa das
    /// Monções come in, and 1200 m from the west edge where the pass from the Areias opens:
    /// <list type="bullet">
    /// <item>round the town (nv 60–64): the <b>Lago de Jhilpur</b> (31), the <b>Bancos de Jhilpur</b>, the <b>Diques do
    /// Norte</b> and the <b>Várzea dos Búfalos</b> (paddies), the <b>Lagoas</b> and <b>Charcos de Lótus</b>, the
    /// <b>Figueiral das Raízes</b> (banyans);</item>
    /// <item>the middle ring (nv 63–70): the <b>Pontes de Raiz Viva</b> and the <b>Gargantas Esquecidas</b> (32: gorges crossed
    /// by root bridges, Plataforma das Garças; Balance Pole), the <b>Cascatas da Chuva</b> and the <b>Floresta de Chuva</b>, the
    /// <b>Floresta de Maré</b> (towards the Costa's mangroves), the <b>Ilhas de Junco</b>, the <b>Pântano Aberto</b> and the
    /// <b>Brejo das Palmeiras</b>;</item>
    /// <item>the far south (nv 70–88): the <b>Palafitas de Tarchon</b> and the <b>Ilhas que Andam</b> (33, the trolls' stilts
    /// — ground reserved flat with a post until their town exists; Reed Boat), the <b>Bancos de Lama</b>, the <b>Raízes
    /// Afogadas</b>, the <b>Mata Morta</b>, the <b>Charco do Ninki Nanka</b>, the <b>Ilhota das Lanternas</b> and the <b>Toca da
    /// Dragoa</b> (34; Antivenom Amulet), the <b>Coração do Lodo</b> and the <b>Olho do Lodo</b> (35, the Raiz-Mãe post;
    /// Purifying Fruit).</item>
    /// </list>
    /// Water: two great distributaries (the Canal de Jhilpur through the lake, the Canal do Leste through the reed swamp), the
    /// gorge rivers of 32, lotus ponds, bog and mire pools and the tidal creeks are carved water that blocks, crossed at fords
    /// (marked by striped posts, like the coast's baixios) and at living-root bridges. Monsters: existing beasts that fit the
    /// sheets (lantern fish for the mudskippers, the sand snake for the gharials, pythons and water snakes, the jackal for the
    /// otters, the gull for the herons, baboons, the lodo for leeches and the Filhos do Lodo, the spider for the root snares,
    /// the stone golem for the forgotten trolls, broken lanterns for the will-o'-wisps, the ill-wind mist for the smoke bubbles,
    /// crows for the rotten roots, bats for the smoke dragonflies) and the night species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string DeltaId = "overworld_delta";

        public const string JhilpurMapId = "map_031_jhilpur";
        public const string RaizVivaId = "map_032_raiz_viva";
        public const string TarchonMapId = "map_033_tarchon";
        public const string NinkiNankaId = "map_034_ninki_nanka";
        public const string CoracaoLodoId = "map_035_coracao_lodo";

        public static readonly Vector2 JhilpurOffset = Vector2.zero;
        public static readonly Vector2 PlataformaGarcasOffset = new Vector2(-950f, -80f);
        public static readonly Vector2 TarchonOffset = new Vector2(480f, -1080f);
        public static readonly Vector2 IlhotaLanternasOffset = new Vector2(480f, -1560f);
        public static readonly Vector2 RaizMaeOffset = new Vector2(-480f, -1560f);

        /// <summary>The arena of the Lodo-que-Engole (35): the stone ring of the Olho do Lodo, west of the Raiz-Mãe.</summary>
        public static readonly Vector2 LodoArena = new Vector2(-900f, -1640f);

        /// <summary>A Ponte que Esqueceu's arena (32, map boss): the stone ring on the trail through the Cascatas da Chuva, above the root gorges.</summary>
        public static readonly Vector2 PonteQueEsqueceuArena = new Vector2(-767f, 367f);

        /// <summary>Ninki Nanka's arena (34, map boss): the stone ring on the boardwalk trail of the Brejo das Palmeiras, the one area of the bog that asks for no amulet.</summary>
        public static readonly Vector2 NinkiNankaArena = new Vector2(1009f, -695f);

        /// <summary>O Pescador de Luzes's arena (33, map boss): the stone ring on the Bancos de Lama west of Tarchon, by the trail, the one area of the stilts that asks for no saddle.</summary>
        public static readonly Vector2 PescadorDeLuzesArena = new Vector2(-168f, -932f);

        /// <summary>Half the side (m) of Jhilpur's flat ground (its solids reach 22.5 m, its roads leave at 18.6 m).</summary>
        public const float JhilpurGround = 30f;

        /// <summary>The Canal de Jhilpur: from the north (the Costa's side) past the town's east side, through its lake, south to the mire.</summary>
        public static readonly Vector2[] JhilpurCanal =
        {
            new Vector2(300f, 560f), new Vector2(260f, 300f), new Vector2(160f, 150f), new Vector2(130f, -60f), new Vector2(120f, -110f), new Vector2(150f, -450f),
            new Vector2(200f, -800f), new Vector2(100f, -1150f), new Vector2(0f, -1450f), new Vector2(-100f, -1790f),
        };

        /// <summary>The Canal do Leste: from the tidal forest through the reed swamp to the east wall.</summary>
        public static readonly Vector2[] EastCanal =
        {
            new Vector2(720f, 560f), new Vector2(800f, 200f), new Vector2(650f, -200f), new Vector2(700f, -600f), new Vector2(900f, -1000f), new Vector2(1190f, -1200f),
        };

        private static class Delta
        {
            public const string Ooze = "enemy_cave_ooze";
            public const string LanternFish = "enemy_cave_lantern_fish";
            public const string Crow = "enemy_cave_dark_crow";
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Crocodile = "enemy_forest_mangrove_crocodile";
            public const string Python = "enemy_forest_dimmed_python";
            public const string Otter = "enemy_forest_thief_otter";
            public const string Lantern = "enemy_desert_broken_lantern";
            public const string Gull = "enemy_forest_thief_gull";
            public const string Crab = "enemy_forest_coconut_crab";
            public const string Spider = PeriodSpawnTable.ForestSpider;
            public const string Boar = NightVariantRules.PeccaryEnemyId;
            public const string Potoo = NightVariantRules.PotooEnemyId;
            public const string Monkey = "enemy_forest_thief_monkey";
            public const string Hawk = "enemy_mountain_crested_hawk";
            public const string Golem = PeriodSpawnTable.MountainGolem;
            public const string Troll = "enemy_forest_dimmed_troll";
            public const string Mist = "enemy_mountain_ill_wind_mist";
        }

        private static readonly Rect DeltaPostSafe = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

        private static OverworldLayout DeltaRegion()
        {
            OverworldLayout w = NewRegion(DeltaId, MapRegion.DeltaDeJhilpur, Rect.MinMaxRect(-1200f, -1800f, 1200f, 600f));
            w.World.Biome = BiomeType.Forest;
            const string jhil = JhilpurMapId;
            const string swamp = MapRegionTypes.Swamp;
            const string forest = MapRegionTypes.Forest;
            const string plain = MapRegionTypes.Plain;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −1800/−1320/−840/−360/120/600) ----------------
            // North row (towards the Costa): cascades, dikes, mudbanks, lotus lagoons, the tidal forest.
            OverworldZone zCascatas = Zone(w, "cascatas_da_chuva", null, RaizVivaId, -1200f, 120f, -720f, 600f, OverworldTerrain.RainForest, 64, 68, 62,
                "Rain Cascades", "Cascatas da Chuva", forest, spacing: 90f);
            OverworldZone zDiques = Zone(w, "diques_do_norte", null, jhil, -720f, 120f, -240f, 600f, OverworldTerrain.PaddyLevees, 61, 64, 59,
                "Northern Dikes", "Diques do Norte", plain, spacing: 85f);
            OverworldZone zBancosJhilpur = Zone(w, "bancos_de_jhilpur", null, jhil, -240f, 120f, 240f, 600f, OverworldTerrain.MudFlats, 60, 63, 58,
                "Jhilpur Mudbanks", "Bancos de Jhilpur", swamp, spacing: 85f);
            OverworldZone zLagoasLotus = Zone(w, "lagoas_de_lotus", null, jhil, 240f, 120f, 720f, 600f, OverworldTerrain.LotusMarsh, 61, 64, 59,
                "Lotus Lagoons", "Lagoas de Lótus", swamp, spacing: 85f);
            OverworldZone zMare = Zone(w, "floresta_de_mare", null, jhil, 720f, 120f, 1200f, 600f, OverworldTerrain.TidalForest, 64, 68, 62,
                "Tidal Forest", "Floresta de Maré", swamp, spacing: 90f);
            // Second row: the root bridges (32), the banyans, Jhilpur's lake (31), the buffalo floodplain, the reed islands.
            OverworldZone zPontes = Zone(w, "pontes_de_raiz_viva", RaizVivaId, null, -1200f, -360f, -720f, 120f, OverworldTerrain.RootGorge, 64, 68, 62,
                "Living Root Bridges", "Pontes de Raiz Viva", forest, MapRequirementTags.RootBridgeBalance, 95f);
            OverworldZone zFigueiral = Zone(w, "figueiral_das_raizes", null, jhil, -720f, -360f, -240f, 120f, OverworldTerrain.BanyanGrove, 61, 64, 59,
                "Banyan Grove", "Figueiral das Raízes", forest, spacing: 85f);
            OverworldZone zLago = Zone(w, "lago_de_jhilpur", jhil, null, -240f, -360f, 240f, 120f, OverworldTerrain.RootLake, 60, 66, 58,
                "Lake of Jhilpur", "Lago de Jhilpur", swamp, spacing: 80f);
            OverworldZone zVarzea = Zone(w, "varzea_dos_bufalos", null, jhil, 240f, -360f, 720f, 120f, OverworldTerrain.PaddyLevees, 61, 64, 59,
                "Buffalo Floodplain", "Várzea dos Búfalos", plain, spacing: 85f);
            OverworldZone zIlhasJunco = Zone(w, "ilhas_de_junco", null, TarchonMapId, 720f, -360f, 1200f, 120f, OverworldTerrain.ReedIslands, 64, 68, 62,
                "Reed Islands", "Ilhas de Junco", swamp, spacing: 85f);
            // Middle row: the forgotten gorges (32), the rain forest, the lotus pools, the open swamp, the palm bog.
            OverworldZone zGargantas = Zone(w, "gargantas_esquecidas", RaizVivaId, null, -1200f, -840f, -720f, -360f, OverworldTerrain.RootGorge, 66, 70, 64,
                "Forgotten Gorges", "Gargantas Esquecidas", forest, MapRequirementTags.RootBridgeBalance, 95f);
            OverworldZone zChuva = Zone(w, "floresta_de_chuva", null, RaizVivaId, -720f, -840f, -240f, -360f, OverworldTerrain.RainForest, 63, 66, 61,
                "Rain Forest", "Floresta de Chuva", forest, spacing: 90f);
            OverworldZone zCharcosLotus = Zone(w, "charcos_de_lotus", null, jhil, -240f, -840f, 240f, -360f, OverworldTerrain.LotusMarsh, 62, 65, 60,
                "Lotus Pools", "Charcos de Lótus", swamp, spacing: 85f);
            OverworldZone zPantano = Zone(w, "pantano_aberto", null, TarchonMapId, 240f, -840f, 720f, -360f, OverworldTerrain.ReedIslands, 63, 66, 61,
                "Open Swamp", "Pântano Aberto", swamp, spacing: 85f);
            OverworldZone zPalmeiras = Zone(w, "brejo_das_palmeiras", null, NinkiNankaId, 720f, -840f, 1200f, -360f, OverworldTerrain.RaffiaBog, 66, 70, 64,
                "Palm Bog", "Brejo das Palmeiras", swamp, spacing: 85f);
            // Fourth row: the dead wood, the drowned roots, the mud banks, Tarchon's stilts (33), the wandering islands (33).
            OverworldZone zMataMorta = Zone(w, "mata_morta", null, CoracaoLodoId, -1200f, -1320f, -720f, -840f, OverworldTerrain.DeadWood, 74, 78, 72,
                "Dead Wood", "Mata Morta", swamp, spacing: 90f);
            OverworldZone zAfogadas = Zone(w, "raizes_afogadas", null, CoracaoLodoId, -720f, -1320f, -240f, -840f, OverworldTerrain.TidalForest, 70, 74, 68,
                "Drowned Roots", "Raízes Afogadas", swamp, spacing: 90f);
            OverworldZone zBancosLama = Zone(w, "bancos_de_lama", null, TarchonMapId, -240f, -1320f, 240f, -840f, OverworldTerrain.MudFlats, 70, 74, 68,
                "Mud Banks", "Bancos de Lama", swamp, spacing: 85f);
            OverworldZone zTarchon = Zone(w, "palafitas_de_tarchon", TarchonMapId, null, 240f, -1320f, 720f, -840f, OverworldTerrain.ReedIslands, 70, 74, 66,
                "Tarchon Stilts", "Palafitas de Tarchon", swamp, MapRequirementTags.SaddleRequired, 85f);
            OverworldZone zIlhasAndam = Zone(w, "ilhas_que_andam", TarchonMapId, null, 720f, -1320f, 1200f, -840f, OverworldTerrain.ReedIslands, 74, 78, 72,
                "Wandering Islands", "Ilhas que Andam", swamp, MapRequirementTags.SaddleRequired, 85f);
            // South row: the eye and the heart of the mire (35), the Ninki Nanka's bog, the lantern islet and the dragon's den (34).
            OverworldZone zOlho = Zone(w, "olho_do_lodo", CoracaoLodoId, null, -1200f, -1800f, -720f, -1320f, OverworldTerrain.BlackMire, 84, 88, 82,
                "Eye of the Mire", "Olho do Lodo", swamp, MapRequirementTags.PurifyingFruit, 90f);
            OverworldZone zCoracao = Zone(w, "coracao_do_lodo", CoracaoLodoId, null, -720f, -1800f, -240f, -1320f, OverworldTerrain.BlackMire, 82, 86, 78,
                "Heart of the Mire", "Coração do Lodo", swamp, MapRequirementTags.PurifyingFruit, 90f);
            OverworldZone zNinki = Zone(w, "charco_do_ninki_nanka", NinkiNankaId, null, -240f, -1800f, 240f, -1320f, OverworldTerrain.RaffiaBog, 76, 82, 72,
                "Ninki Nanka Bog", "Charco do Ninki Nanka", swamp, MapRequirementTags.AntiVenomAmulet, 90f);
            OverworldZone zIlhota = Zone(w, "ilhota_das_lanternas", NinkiNankaId, null, 240f, -1800f, 720f, -1320f, OverworldTerrain.RaffiaBog, 78, 82, 72,
                "Lantern Islet", "Ilhota das Lanternas", swamp, MapRequirementTags.AntiVenomAmulet, 90f);
            OverworldZone zToca = Zone(w, "toca_da_dragoa", NinkiNankaId, null, 720f, -1800f, 1200f, -1320f, OverworldTerrain.RaffiaBog, 80, 82, 78,
                "Dragon's Den", "Toca da Dragoa", swamp, MapRequirementTags.AntiVenomAmulet, 90f);

            AddDeltaFauna(zCascatas, zDiques, zBancosJhilpur, zLagoasLotus, zMare, zPontes, zFigueiral, zLago, zVarzea, zIlhasJunco, zGargantas, zChuva,
                zCharcosLotus, zPantano, zPalmeiras, zMataMorta, zAfogadas, zBancosLama, zTarchon, zIlhasAndam, zOlho, zCoracao, zNinki, zIlhota, zToca);

            // ---------------- Settlements ----------------
            // Jhilpur first: the region's return city and the barca's stop for 31 (its own recipe; the delta is all round it).
            CityTownRecipe jhilpur = CityTownRecipes.Jhilpur();
            jhilpur.Backdrop = false;
            w.Vila = Place(w, jhil, jhilpur, JhilpurOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), JhilpurGround, true);

            // The Palafitas de Tarchon (33, the trolls' race city): its own recipe, the return city of 33–34.
            CityTownRecipe tarchon = CityTownRecipes.Tarchon();
            tarchon.Backdrop = false;
            Place(w, TarchonMapId, tarchon, TarchonOffset, Rect.MinMaxRect(-24f, -24f, 24f, 24f), RaceCityGround, true);

            // The posts of 32, 34 and 35.
            Place(w, RaizVivaId, DeltaPost("plataforma_das_garcas", TownSwatch.Moss, TownShopDressing.Storyteller), PlataformaGarcasOffset, DeltaPostSafe, 18f, false);
            Place(w, NinkiNankaId, DeltaPost("ilhota_das_lanternas", TownSwatch.Tamarisk, TownShopDressing.Wicks), IlhotaLanternasOffset, DeltaPostSafe, 18f, false);
            Place(w, CoracaoLodoId, DeltaPost("raiz_mae", TownSwatch.ReefTeal, TownShopDressing.Herder), RaizMaeOffset, DeltaPostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Canal de Jhilpur: the root bridge on the east road, a ford on the road to Tarchon, a ford to the lantern islet.
            MapRiver canal = River(w, 26f, JhilpurCanal);
            Vector2 eastBridge = On(JhilpurCanal[2], JhilpurCanal[3], 150f / 210f);
            Crossing(canal, eastBridge, 4.5f, true);
            Vector2 tarchonFord = On(JhilpurCanal[6], JhilpurCanal[7], 0.6f);
            Crossing(canal, tarchonFord, 12f, false);
            Vector2 islandFord = On(JhilpurCanal[8], JhilpurCanal[9], 150f / 340f);
            Crossing(canal, islandFord, 12f, false);
            Crossing(canal, On(JhilpurCanal[1], JhilpurCanal[2], 0.5f), 10f, false);

            // Jhilpur's lake south of the town (the ghats go down to it): two rows of water side by side.
            River(w, 40f, new Vector2(-150f, -85f), new Vector2(110f, -85f));
            River(w, 40f, new Vector2(-110f, -120f), new Vector2(90f, -120f));

            // The Canal do Leste: a ford on the east road, one on the road from Tarchon to the wandering islands.
            MapRiver east = River(w, 22f, EastCanal);
            Vector2 eastFord = On(EastCanal[1], EastCanal[2], 0.75f);
            Crossing(east, eastFord, 12f, false);
            Vector2 islandsFord = On(EastCanal[4], EastCanal[5], 0.3f);
            Crossing(east, islandsFord, 12f, false);
            Crossing(east, On(EastCanal[2], EastCanal[3], 0.5f), 10f, false);

            // The gorges of 32: three rivers east–west, each crossed by a living-root bridge on the gorge road.
            Vector2 g10 = new Vector2(-1190f, -230f), g11 = new Vector2(-950f, -250f), g12 = new Vector2(-730f, -220f);
            MapRiver gorge1 = River(w, 12f, g10, g11, g12);
            Vector2 gorge1Bridge = On(g10, g11, 140f / 240f);
            Crossing(gorge1, gorge1Bridge, 4f, true);
            Crossing(gorge1, On(g11, g12, 0.6f), 4f, true);
            Vector2 g20 = new Vector2(-1190f, -560f), g21 = new Vector2(-950f, -540f), g22 = new Vector2(-730f, -570f);
            MapRiver gorge2 = River(w, 12f, g20, g21, g22);
            Vector2 gorge2Bridge = On(g20, g21, 140f / 240f);
            Crossing(gorge2, gorge2Bridge, 4f, true);
            Vector2 g30 = new Vector2(-1190f, -780f), g31 = new Vector2(-760f, -800f);
            MapRiver gorge3 = River(w, 10f, g30, g31);
            Vector2 gorge3Bridge = On(g30, g31, 160f / 430f);
            Crossing(gorge3, gorge3Bridge, 4f, true);

            // Lotus ponds, bog and mire pools, tidal creeks, the cascades' pool.
            River(w, 18f, new Vector2(360f, 360f), new Vector2(390f, 380f));
            River(w, 16f, new Vector2(600f, 180f), new Vector2(640f, 200f));
            River(w, 16f, new Vector2(620f, 420f), new Vector2(660f, 440f));
            River(w, 18f, new Vector2(-100f, -500f), new Vector2(-60f, -520f));
            River(w, 16f, new Vector2(-150f, -700f), new Vector2(-110f, -690f));
            River(w, 14f, new Vector2(20f, -600f), new Vector2(50f, -620f));
            River(w, 14f, new Vector2(-150f, -1700f), new Vector2(-110f, -1690f));
            River(w, 14f, new Vector2(150f, -1700f), new Vector2(190f, -1720f));
            River(w, 14f, new Vector2(900f, -1450f), new Vector2(940f, -1470f));
            River(w, 14f, new Vector2(1050f, -1700f), new Vector2(1090f, -1690f));
            River(w, 20f, new Vector2(-1000f, -1450f), new Vector2(-950f, -1470f));
            River(w, 18f, new Vector2(-650f, -1720f), new Vector2(-600f, -1740f));
            River(w, 16f, new Vector2(-350f, -1450f), new Vector2(-320f, -1470f));
            River(w, 10f, new Vector2(1100f, 580f), new Vector2(1080f, 350f), new Vector2(1190f, 250f));
            River(w, 10f, new Vector2(-700f, -880f), new Vector2(-600f, -960f), new Vector2(-540f, -1020f));
            River(w, 14f, new Vector2(-1000f, 520f), new Vector2(-960f, 540f));

            // ---------------- Gates out of the region ----------------
            // North: towards the Costa das Monções (the red cliffs' road and the mangroves of Liyongo); west: the pass from the Areias.
            Gate(w, "exit_costa", MapExitKind.Map, OverworldLayouts.TaaPwaniMapId, Rect.MinMaxRect(-668f, 597f, -652f, 600f), new Vector2(-660f, 591f), Vector2.down);
            Gate(w, "exit_liyongo", MapExitKind.Map, LiyongoId, Rect.MinMaxRect(932f, 597f, 948f, 600f), new Vector2(940f, 591f), Vector2.down);
            Gate(w, "exit_areias", MapExitKind.Map, MapLayouts.ArtificesId, Rect.MinMaxRect(-1200f, 342f, -1197f, 358f), new Vector2(-1191f, 350f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // The north road (Jhilpur's North street) to the Costa gate; the Areias road joins it.
            Vector2 northFork = new Vector2(-20f, 300f);
            Vector2 areiasJoin = new Vector2(-560f, 450f);
            PathVia(w, 4f, new Vector2(0f, JhilpurGround), new Vector2(0f, 150f), northFork, new Vector2(-300f, 380f), areiasJoin, new Vector2(-660f, 591f));
            PathVia(w, 4f, new Vector2(-1191f, 350f), new Vector2(-950f, 330f), new Vector2(-700f, 380f), areiasJoin);
            PathVia(w, 3.5f, northFork, new Vector2(60f, 450f), new Vector2(100f, 560f));
            // The east road: over the root bridge, through the floodplain, over the Canal do Leste's ford to the reed islands;
            // the lotus lagoons' branch; the road up to the Liyongo gate.
            Vector2 eastFork = new Vector2(560f, -60f);
            Vector2 lotusFork = new Vector2(300f, 0f);
            Vector2 reedFork = new Vector2(1000f, -100f);
            PathVia(w, 4f, new Vector2(JhilpurGround, 0f), eastBridge, lotusFork, eastFork, eastFord, new Vector2(850f, -120f), reedFork);
            PathVia(w, 3.5f, lotusFork, new Vector2(420f, 250f), new Vector2(520f, 480f));
            PathVia(w, 3.5f, reedFork, new Vector2(960f, 300f), new Vector2(940f, 591f));
            // South from the floodplain to Tarchon's east road (the trolls' town opens west, east and south).
            Vector2 tarchonEast = new Vector2(700f, -1070f);
            PathVia(w, 3.5f, eastFork, new Vector2(540f, -400f), new Vector2(600f, -800f), tarchonEast);
            // The west road to the Plataforma das Garças, the gorge road over the three root bridges down to the dead wood.
            Vector2 westFork = new Vector2(-400f, -60f);
            PathVia(w, 4f, new Vector2(-JhilpurGround, 0f), new Vector2(-200f, -20f), westFork, new Vector2(-700f, -90f), PlataformaGarcasOffset + new Vector2(18f, 0f));
            Vector2 swJoin = new Vector2(-480f, -1100f);
            PathVia(w, 3.5f, PlataformaGarcasOffset + new Vector2(-18f, 0f), new Vector2(-1050f, -140f), gorge1Bridge, new Vector2(-1050f, -400f), gorge2Bridge,
                new Vector2(-1040f, -680f), gorge3Bridge, new Vector2(-1000f, -950f), new Vector2(-950f, -1100f), new Vector2(-700f, -1150f), swJoin);
            // The south-west road through the banyans and the rain forest to the Raiz-Mãe, on to the eye of the mire.
            Vector2 swFork = new Vector2(-450f, -800f);
            Vector2 raizWest = new Vector2(-560f, -1560f);
            PathVia(w, 4f, westFork, new Vector2(-420f, -400f), swFork, swJoin, new Vector2(-560f, -1350f), raizWest, RaizMaeOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, raizWest, new Vector2(-800f, -1580f), new Vector2(-980f, -1600f));
            // From the Raiz-Mãe east over the canal's ford to the lantern islet, on to the dragon's den.
            PathVia(w, 3.5f, RaizMaeOffset + new Vector2(18f, 0f), new Vector2(-200f, -1580f), islandFord, new Vector2(240f, -1570f), IlhotaLanternasOffset + new Vector2(-18f, 0f));
            Vector2 denFork = new Vector2(800f, -1560f);
            PathVia(w, 3.5f, IlhotaLanternasOffset + new Vector2(18f, 0f), denFork, new Vector2(1000f, -1620f));
            // Tarchon west over the canal's ford to the south-west road; east over the Canal do Leste to the wandering islands and
            // the palm bog, back up to the reed islands; south to the lantern islet and the dragon's den.
            PathVia(w, 3.5f, TarchonOffset + new Vector2(-RaceCityGround, 0f), new Vector2(300f, -1060f), tarchonFord, new Vector2(-100f, -950f), swFork);
            PathVia(w, 3.5f, TarchonOffset + new Vector2(RaceCityGround, 0f), tarchonEast, islandsFord, new Vector2(1050f, -900f), new Vector2(1000f, -600f), reedFork);
            PathVia(w, 3.5f, tarchonEast, new Vector2(760f, -1300f), denFork);
            PathVia(w, 3.5f, TarchonOffset + new Vector2(0f, -RaceCityGround), new Vector2(470f, -1300f), new Vector2(400f, -1450f), new Vector2(240f, -1570f));

            // ---------------- Landmarks ----------------
            Sign(w, northFork, new Vector2(60f, 450f), 4f, 0f);
            Sign(w, eastFork, new Vector2(540f, -400f), 4f, 0f);
            Sign(w, westFork, new Vector2(-420f, -400f), 4f, 0f);
            Sign(w, swFork, new Vector2(-100f, -950f), 4f, 0f);
            Sign(w, lotusFork, new Vector2(420f, 250f), 4f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -620f, 470f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 880f, -160f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -460f, -820f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 740f, -1300f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -1010f, -930f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -760f, 300f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, LodoArena.x, LodoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, PonteQueEsqueceuArena.x, PonteQueEsqueceuArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, NinkiNankaArena.x, NinkiNankaArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, PescadorDeLuzesArena.x, PescadorDeLuzesArena.y, 0f, 1.1f);

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
        // The posts of 32–35 (plank and reed, thatch, the coast's flora)
        // ------------------------------------------------------------------

        /// <summary>A delta post: a plank-and-thatch workshop at the back of the plaza, two reed houses, the Cais da Barca, lamps.</summary>
        private static CityTownRecipe DeltaPost(string id, TownSwatch trim, TownShopDressing dressing)
        {
            var posto = new CityTownRecipe
            {
                CityId = id, Region = BiomeType.Forest, Style = TownStyles.Coast, PlazaRadius = 4.5f, StreetWidth = 3.5f, SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f, EntryArm = CityArm.West, AutoYards = false, Backdrop = false, NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_oficina", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down, Width = 7f, Depth = 5f,
                Porch = 1.4f, Spec = DeltaSpec(trim, dressing),
            });
            posto.Lots.Add(new CityLotRecipe { Id = id + "_cais", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_casa1", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f,
                Spec = DeltaSpec(TownSwatch.Reed, TownShopDressing.None),
            });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_casa2", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 9f, Width = 5f, Depth = 4.4f, Porch = 1f,
                Spec = DeltaSpec(TownSwatch.Kelp, TownShopDressing.None),
            });
            posto.PlazaProps.Add(Prop(CityPropKind.Crates, -3f, -6f, 20f));
            posto.PlazaProps.Add(Prop(CityPropKind.Jugs, 3.2f, -5.6f, 40f));
            posto.PlazaProps.Add(Prop(CityPropKind.Tree, -8.5f, -7.5f, 0f, 1.1f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            return posto;
        }

        /// <summary>A plank-walled house with a thatched gable roof, dark joinery and a band of colour.</summary>
        private static TownBuildingSpec DeltaSpec(TownSwatch trim, TownShopDressing dressing)
        {
            return new TownBuildingSpec
            {
                WallColor = TownSwatch.WoodWeathered, TrimColor = trim, RoofColor = TownSwatch.Thatch, JoineryColor = TownSwatch.WoodDark, AccentColor = TownSwatch.ReefTeal,
                PlinthColor = TownSwatch.Brick, SignColor = TownSwatch.Kelp, Roof = TownRoofKind.Gable, Dressing = dressing, WallHeight = 2.8f, WoodenWalls = true,
            };
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 31–35 archetypes)
        // ------------------------------------------------------------------

        private static void AddDeltaFauna(OverworldZone cascatas, OverworldZone diques, OverworldZone bancosJhilpur, OverworldZone lagoasLotus, OverworldZone mare,
            OverworldZone pontes, OverworldZone figueiral, OverworldZone lago, OverworldZone varzea, OverworldZone ilhasJunco, OverworldZone gargantas,
            OverworldZone chuva, OverworldZone charcosLotus, OverworldZone pantano, OverworldZone palmeiras, OverworldZone mataMorta, OverworldZone afogadas,
            OverworldZone bancosLama, OverworldZone tarchon, OverworldZone ilhasAndam, OverworldZone olho, OverworldZone coracao, OverworldZone ninki,
            OverworldZone ilhota, OverworldZone toca)
        {
            // Fireflies in the roots and will-o'-wisps over the swamp at night (the firefly swarm); the ghost heron (the shroud owl).
            NightSpecies fireflies = NightVariantRules.VagaLume;
            NightSpecies ghostHeron = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;

            // Lago de Jhilpur (31): the Peixe-Saltador de Lodo [enxame] (lantern fish), the Gavial do Lodo [tanque] (the sand
            // snake), the Garça-Lança Apagada [distância] (the gull's body).
            lago.Fauna.Add(Fauna("peixes_saltadores", "[enxame]", T(N(Delta.LanternFish, 3), N(Delta.Ooze, 1)), T(V(Delta.LanternFish, 2), S(fireflies, 1)), 5));
            lago.Fauna.Add(Fauna("gaviais_do_lodo", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Crab, 1)), T(V(Delta.Crocodile, 2), V(Delta.Crab, 1)), 3, null, 28f));
            lago.Fauna.Add(Fauna("garcas_lanca", "[distância]", T(N(MidBeasts.SpearHeron, 3), N(Delta.Hawk, 1)), T(V(MidBeasts.SpearHeron, 2), S(ghostHeron, 1)), 4, null, 22f));

            // Bancos de Jhilpur: the Lontra Ladra [rápido] (the jackal's body), mudskippers.
            bancosJhilpur.Fauna.Add(Fauna("lontras_ladras", "[rápido]", T(N(Delta.Otter, 3), N(Delta.Gull, 1)), T(V(Delta.Otter, 2), S(fireflies, 1)), 4));
            bancosJhilpur.Fauna.Add(Fauna("saltadores_dos_bancos", "[enxame]", T(N(Delta.LanternFish, 3), N(Delta.Crab, 1)), T(V(Delta.LanternFish, 2), V(Delta.Crab, 1)), 5));

            // Diques do Norte and Várzea dos Búfalos: herons over the paddies, otters and the swamp pigs in the dikes.
            diques.Fauna.Add(Fauna("garcas_dos_diques", "[distância]", T(N(MidBeasts.SpearHeron, 3), N(Delta.Hawk, 1)), T(V(MidBeasts.SpearHeron, 2), S(ghostHeron, 1)), 4, null, 22f));
            diques.Fauna.Add(Fauna("lontras_dos_arrozais", "[rápido]", T(N(Delta.Otter, 3), N(Delta.Boar, 1)), T(V(Delta.Otter, 2), S(NightVariantRules.Queixada, 1)), 4));
            varzea.Fauna.Add(Fauna("porcos_do_brejo", "[tanque]", T(N(Delta.Boar, 3), N(Delta.Ooze, 1)), T(S(NightVariantRules.Queixada, 2), V(Delta.Ooze, 1)), 3, null, 28f));
            varzea.Fauna.Add(Fauna("garcas_da_varzea", "[distância] [rápido]", T(N(MidBeasts.SpearHeron, 3), N(Delta.Otter, 1)), T(V(MidBeasts.SpearHeron, 2), V(Delta.Otter, 1)), 4, null, 22f));

            // Lagoas and Charcos de Lótus: the leeches [enxame] (the lodo), herons and mudskippers.
            lagoasLotus.Fauna.Add(Fauna("sanguessugas_do_lotus", "[enxame]", T(N(MidBeasts.MireLeech, 3), N(Delta.LanternFish, 1)), T(V(MidBeasts.MireLeech, 2), S(fireflies, 1)), 5));
            lagoasLotus.Fauna.Add(Fauna("garcas_do_lotus", "[distância]", T(N(MidBeasts.SpearHeron, 3), N(Delta.Potoo, 1)), T(V(MidBeasts.SpearHeron, 2), S(NightVariantRules.MaeDaLua, 1)), 4, null, 22f));
            charcosLotus.Fauna.Add(Fauna("peixes_do_charco", "[enxame]", T(N(Delta.LanternFish, 3), N(Delta.Ooze, 1)), T(V(Delta.LanternFish, 2), S(fireflies, 1)), 5));
            charcosLotus.Fauna.Add(Fauna("gaviais_do_charco", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Gull, 1)), T(V(Delta.Crocodile, 2), S(ghostHeron, 1)), 3, null, 28f));

            // Figueiral das Raízes: the Macaco Ladrão [rápido] (baboons) in the banyans, the Raiz-Laço [controle] (the spider).
            figueiral.Fauna.Add(Fauna("macacos_do_figueiral", "[rápido]", T(N(Delta.Monkey, 3), N(Delta.Potoo, 1)), T(V(Delta.Monkey, 2), S(NightVariantRules.MaeDaLua, 1)), 4));
            figueiral.Fauna.Add(Fauna("raizes_laco", "[controle]", T(N(Delta.Spider, 3), N(Delta.Monkey, 1)), T(V(Delta.Spider, 2), S(fireflies, 1)), 4));

            // Floresta de Maré (towards the Costa's mangroves): gharials and crabs on the creeks, monkeys in the canopy.
            mare.Fauna.Add(Fauna("gaviais_da_mare", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Crab, 1)), T(V(Delta.Crocodile, 2), V(Delta.Crab, 1)), 3, null, 28f));
            mare.Fauna.Add(Fauna("macacos_da_mare", "[rápido]", T(N(Delta.Monkey, 3), N(Delta.Crow, 1)), T(V(Delta.Monkey, 2), S(bats, 1)), 4));

            // Pontes de Raiz Viva and Gargantas Esquecidas (32): the Macaco Ladrão, the Píton Apagada [controle] (the sand
            // snake), the Sanguessuga do Lodo [enxame], the Raiz-Laço; hawks over the gorges.
            pontes.Fauna.Add(Fauna("macacos_das_pontes", "[rápido]", T(N(Delta.Monkey, 3), N(Delta.Hawk, 1)), T(V(Delta.Monkey, 2), S(NightVariantRules.MaeDaLua, 1)), 4));
            pontes.Fauna.Add(Fauna("pitons_apagadas", "[controle]", T(N(Delta.Python, 3), N(Delta.Spider, 1)), T(V(Delta.Python, 2), V(Delta.Spider, 1)), 3, null, 28f));
            pontes.Fauna.Add(Fauna("raizes_laco_das_pontes", "[controle]", T(N(Delta.Spider, 3), N(Delta.Ooze, 1)), T(V(Delta.Spider, 2), S(fireflies, 1)), 4));
            gargantas.Fauna.Add(Fauna("sanguessugas_da_garganta", "[enxame]", T(N(MidBeasts.MireLeech, 3), N(Delta.Spider, 1)), T(V(MidBeasts.MireLeech, 2), S(fireflies, 1)), 5));
            gargantas.Fauna.Add(Fauna("pitons_da_garganta", "[controle]", T(N(Delta.Python, 3), N(Delta.Monkey, 1)), T(V(Delta.Python, 2), V(Delta.Monkey, 1)), 3, null, 28f));
            gargantas.Fauna.Add(Fauna("gavioes_da_garganta", "[distância]", T(N(Delta.Hawk, 3), N(Delta.Crow, 1)), T(V(Delta.Hawk, 2), S(bats, 1)), 3));

            // Cascatas da Chuva and Floresta de Chuva: monkeys, pythons, leeches.
            cascatas.Fauna.Add(Fauna("macacos_das_cascatas", "[rápido]", T(N(Delta.Monkey, 3), N(Delta.Hawk, 1)), T(V(Delta.Monkey, 2), V(Delta.Hawk, 1)), 4));
            cascatas.Fauna.Add(Fauna("pitons_das_cascatas", "[controle]", T(N(Delta.Python, 3), N(Delta.Spider, 1)), T(V(Delta.Python, 2), S(fireflies, 1)), 3, null, 28f));
            chuva.Fauna.Add(Fauna("macacos_da_chuva", "[rápido]", T(N(Delta.Monkey, 3), N(Delta.Potoo, 1)), T(V(Delta.Monkey, 2), S(NightVariantRules.MaeDaLua, 1)), 4));
            chuva.Fauna.Add(Fauna("sanguessugas_da_chuva", "[enxame] [controle]", T(N(MidBeasts.MireLeech, 3), N(Delta.Spider, 1)), T(V(MidBeasts.MireLeech, 2), V(Delta.Spider, 1)), 5));

            // Ilhas de Junco and Pântano Aberto (towards 33): the Fogo-Fátuo Preso [enxame] (broken lanterns, the fireflies at
            // night), the Garça-Fantasma [distância] (the gull by day, the shroud owl at night), the Crocodilo de Musgo [tanque].
            ilhasJunco.Fauna.Add(Fauna("fogos_fatuos", "[enxame]", T(N(Delta.Lantern, 3), N(Delta.Mist, 1)), T(V(Delta.Lantern, 2), S(fireflies, 2)), 5));
            ilhasJunco.Fauna.Add(Fauna("garcas_fantasma", "[distância]", T(N(Delta.Gull, 3), N(Delta.Crow, 1)), T(V(Delta.Gull, 1), S(ghostHeron, 2)), 4, null, 22f));
            pantano.Fauna.Add(Fauna("fogos_fatuos_do_pantano", "[enxame]", T(N(Delta.Lantern, 3), N(Delta.Bats, 1)), T(V(Delta.Lantern, 2), S(fireflies, 2)), 5));
            pantano.Fauna.Add(Fauna("crocodilos_de_musgo", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Ooze, 1)), T(V(Delta.Crocodile, 2), V(Delta.Ooze, 1)), 3, null, 28f));

            // Brejo das Palmeiras (towards 34): the Sapo-Boi [tanque] (the peccary's body), the Cobra-d'Água [rápido].
            palmeiras.Fauna.Add(Fauna("sapos_boi", "[tanque]", T(N(Delta.Boar, 3), N(Delta.Ooze, 1)), T(S(NightVariantRules.Queixada, 2), V(Delta.Ooze, 1)), 3, null, 28f));
            palmeiras.Fauna.Add(Fauna("cobras_dagua", "[rápido]", T(N(Delta.Python, 3), N(Delta.Gull, 1)), T(V(Delta.Python, 2), S(fireflies, 1)), 4));

            // Mata Morta and Raízes Afogadas (towards 35): the Raiz Podre [distância] (crows), forgotten trolls (the golem),
            // the Gavial-Sombra [tanque], the Bolha de Fumaça [controle] (the ill-wind mist).
            mataMorta.Fauna.Add(Fauna("raizes_podres", "[distância]", T(N(Delta.Crow, 3), N(Delta.Lantern, 1)), T(V(Delta.Crow, 2), S(bats, 1)), 4));
            mataMorta.Fauna.Add(Fauna("trols_esquecidos_da_mata", "[tanque]", T(N(Delta.Troll, 3), N(Delta.Mist, 1)), T(V(Delta.Troll, 2), V(Delta.Mist, 1)), 3, null, 30f));
            afogadas.Fauna.Add(Fauna("gaviais_sombra", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Ooze, 1)), T(V(Delta.Crocodile, 2), V(Delta.Ooze, 1)), 3, null, 28f));
            afogadas.Fauna.Add(Fauna("bolhas_de_fumaca", "[controle]", T(N(Delta.Mist, 3), N(Delta.Ooze, 1)), T(V(Delta.Mist, 2), S(fireflies, 1)), 4));

            // Bancos de Lama: the Filhos do Lodo begin [enxame], herons.
            bancosLama.Fauna.Add(Fauna("filhos_do_lodo_dos_bancos", "[enxame]", T(N(Delta.Ooze, 3), N(Delta.LanternFish, 1)), T(V(Delta.Ooze, 3), S(fireflies, 1)), 5));
            bancosLama.Fauna.Add(Fauna("garcas_da_lama", "[distância]", T(N(MidBeasts.SpearHeron, 3), N(Delta.Crow, 1)), T(V(MidBeasts.SpearHeron, 2), S(ghostHeron, 1)), 4, null, 22f));

            // Palafitas de Tarchon and Ilhas que Andam (33): the Trol Apagado [tanque] (the golem), the Fogo-Fátuo Preso
            // [enxame], the Garça-Fantasma [distância], the Crocodilo de Musgo [tanque].
            tarchon.Fauna.Add(Fauna("trols_apagados", "[tanque]", T(N(Delta.Troll, 3), N(Delta.Lantern, 1)), T(V(Delta.Troll, 2), V(Delta.Lantern, 1)), 3, null, 30f));
            tarchon.Fauna.Add(Fauna("fogos_fatuos_presos", "[enxame]", T(N(Delta.Lantern, 3), N(Delta.Bats, 1)), T(V(Delta.Lantern, 2), S(fireflies, 2)), 5));
            tarchon.Fauna.Add(Fauna("garcas_fantasma_de_tarchon", "[distância]", T(N(Delta.Gull, 3), N(Delta.Crow, 1)), T(V(Delta.Gull, 1), S(ghostHeron, 2)), 4, null, 22f));
            ilhasAndam.Fauna.Add(Fauna("crocodilos_das_ilhas", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Golem, 1)), T(V(Delta.Crocodile, 2), V(Delta.Golem, 1)), 3, null, 30f));
            ilhasAndam.Fauna.Add(Fauna("fogos_das_ilhas", "[enxame] [controle]", T(N(Delta.Lantern, 3), N(Delta.Mist, 1)), T(V(Delta.Lantern, 2), S(fireflies, 2)), 5));

            // Charco do Ninki Nanka, Ilhota das Lanternas and Toca da Dragoa (34): the Ninki Nanka Filhote [elite] (the sand
            // snake's body), the Sapo-Boi [tanque], the Libélula de Fumaça [enxame] (bats), the Cobra-d'Água [rápido].
            ninki.Fauna.Add(Fauna("filhotes_de_ninki_nanka", "[elite]", T(N(Delta.Crocodile, 3), N(Delta.Mist, 1)), T(V(Delta.Crocodile, 2), V(Delta.Mist, 1)), 3, null, 32f));
            ninki.Fauna.Add(Fauna("libelulas_de_fumaca", "[enxame]", T(N(Delta.Bats, 3), N(Delta.Lantern, 1)), T(V(Delta.Bats, 2), S(fireflies, 2)), 5));
            ninki.Fauna.Add(Fauna("sapos_boi_do_charco", "[tanque]", T(N(Delta.Boar, 3), N(Delta.Ooze, 1)), T(S(NightVariantRules.Queixada, 2), V(Delta.Ooze, 1)), 3, null, 28f));
            ilhota.Fauna.Add(Fauna("libelulas_das_lanternas", "[enxame]", T(N(Delta.Bats, 3), N(Delta.Lantern, 1)), T(V(Delta.Bats, 2), S(fireflies, 2)), 5));
            ilhota.Fauna.Add(Fauna("cobras_dagua_da_ilhota", "[rápido]", T(N(Delta.Python, 3), N(Delta.Gull, 1)), T(V(Delta.Python, 2), S(ghostHeron, 1)), 4));
            toca.Fauna.Add(Fauna("filhotes_da_toca", "[elite]", T(N(Delta.Crocodile, 3), N(Delta.Golem, 1)), T(V(Delta.Crocodile, 2), V(Delta.Golem, 1)), 3, null, 32f));
            toca.Fauna.Add(Fauna("sapos_da_toca", "[tanque] [enxame]", T(N(Delta.Boar, 3), N(Delta.Bats, 1)), T(S(NightVariantRules.Queixada, 2), V(Delta.Bats, 1)), 4));

            // Coração do Lodo and Olho do Lodo (35): the Filhos do Lodo [enxame] (the lodo, doubled at night), the Bolha de
            // Fumaça [controle], the Gavial-Sombra [tanque], the Raiz Podre [distância].
            coracao.Fauna.Add(Fauna("filhos_do_lodo", "[enxame]", T(N(Delta.Ooze, 3), N(Delta.Mist, 1)), T(V(Delta.Ooze, 4), S(fireflies, 1)), 6));
            coracao.Fauna.Add(Fauna("gaviais_sombra_do_lodo", "[tanque]", T(N(Delta.Crocodile, 3), N(Delta.Crow, 1)), T(V(Delta.Crocodile, 2), V(Delta.Crow, 1)), 3, null, 30f));
            coracao.Fauna.Add(Fauna("raizes_podres_do_lodo", "[distância]", T(N(Delta.Crow, 3), N(Delta.Lantern, 1)), T(V(Delta.Crow, 2), S(bats, 1)), 4));
            olho.Fauna.Add(Fauna("filhos_do_olho", "[enxame]", T(N(Delta.Ooze, 3), N(Delta.LanternFish, 1)), T(V(Delta.Ooze, 4), S(fireflies, 1)), 6));
            olho.Fauna.Add(Fauna("bolhas_do_olho", "[controle]", T(N(Delta.Mist, 3), N(Delta.Ooze, 1)), T(V(Delta.Mist, 2), V(Delta.Ooze, 1)), 4));
            olho.Fauna.Add(Fauna("gaviais_do_olho", "[tanque] [elite]", T(N(Delta.Crocodile, 3), N(Delta.Golem, 1)), T(V(Delta.Crocodile, 2), V(Delta.Golem, 1)), 3, null, 32f));
        }
    }
}
