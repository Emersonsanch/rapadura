using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 1 — Bosque do Zarafshan as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10), no scene change
    /// between the Vila and the areas around it:
    /// <list type="bullet">
    /// <item>the <b>Vila da Vó Edna</b> at the plaza (0, 0), centre-south-west; around it the <b>Pomar</b> (01, nv 1–3) and,
    /// south of the aryk canal, the <b>Campos do Aryk</b> (01, nv 2–3): 600 × 700 m of start area;</item>
    /// <item>east, the <b>Tugai do Rio Dourado</b> (02, nv 2–5: its hand-made map embedded around the Moinho do Pomar) with the
    /// golden river running the whole region north to south, and the <b>Círculo de Kök-Böri</b> north of it (nv 4–5);</item>
    /// <item>the in-between areas, harder the farther from the Vila: <b>Campinas do Vento Vivo</b> (nv 3–6), <b>Prado das
    /// Cegonhas</b> with its lake (4–7), <b>Estepe de Ohrmazd</b> towards the Areias (4–6, gate to 07), <b>Várzea Dourada</b>
    /// (5–8), <b>Colinas de Mihryazd</b> (7–11), <b>Altos do Mensageiro</b> (8–12, gate to 13), <b>Ruínas do Aryk Velho</b>
    /// (8–12), <b>Mata das Cinco Árvores</b> (10–15), <b>Ravina das Bocas de Luz</b> (11–15, gate to 19), <b>Mata Alta de
    /// Zurvan</b> (12–16);</item>
    /// <item>the far east, each its hand-made map embedded: <b>Brejo do Lodo Escuro</b> (03, nv 16–22, boots), <b>Bosque das
    /// Amoreiras</b> (04, 18–24), <b>Copas de Garajonay</b> with the city of the elfos-da-mata (05, 20–26) and the <b>Jardim
    /// de Mihrshah</b> (06, 26–32, garden key).</item>
    /// </list>
    /// Monsters reuse today's definitions (wolf, spider, boar, scorpion, stone golem, potoo, bat swarm and the night
    /// species of <see cref="NightVariantRules"/>).
    /// </summary>
    public static partial class OverworldLayouts
    {
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Boar = NightVariantRules.PeccaryEnemyId;
        private const string Scorpion = PeriodSpawnTable.DesertScorpion;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Potoo = NightVariantRules.PotooEnemyId;

        /// <summary>
        /// The twelfth pass of beasts (bible §3.2.1, <c>EarlyStandInBeastRecipes</c>): the roles of Regions 1–4 that the continuous world played with
        /// stand-ins (Docs/Mapas100.md 02–06, 10, 14–16, 20), placed in the groups of their sheets here and in the Areias, Kaggen and Limiar layouts.
        /// </summary>
        public static class EarlyBeasts
        {
            public const string Stork = "enemy_forest_dimmed_stork";
            public const string BubbleToad = "enemy_forest_bubble_toad";
            public const string SmokeMosquito = "enemy_forest_smoke_mosquito";
            public const string SilkMoth = "enemy_forest_silk_moth";
            public const string LaurelPigeon = "enemy_forest_laurel_pigeon";
            public const string LaurelLizard = "enemy_forest_laurel_lizard";
            public const string SmokePeacock = "enemy_forest_smoke_peacock";
            public const string ThornLizard = "enemy_desert_thorn_lizard";
            public const string DimmedLynx = "enemy_mountain_dimmed_lynx";
            public const string Smudge = "enemy_mountain_smudge";
            public const string StoneJackdaw = "enemy_mountain_stone_jackdaw";
            public const string YoungUbir = "enemy_cave_young_ubir";
        }

        public static readonly Vector2 MoinhoOffset = new Vector2(420f, 40f);
        public static readonly Vector2 BrejoOffset = new Vector2(880f, -560f);
        public static readonly Vector2 AmoreirasOffset = new Vector2(1225f, -560f);

        /// <summary>A Fiandeira Esquecida's arena (04, map boss): the stone ring by the trail west of the mulberry grove, outside its rows and webs.</summary>
        public static readonly Vector2 FiandeiraEsquecidaArena = new Vector2(1099f, -579f);
        public static readonly Vector2 GarajonayOffset = new Vector2(1225f, 300f);
        public static readonly Vector2 MihrshahOffset = new Vector2(1150f, 1050f);

        private static OverworldLayout Zarafshan()
        {
            OverworldLayout w = NewRegion(ZarafshanId, MapRegion.Zarafshan, Rect.MinMaxRect(-1000f, -1000f, 1400f, 1400f));
            const string pomar = MapCatalog.PomarId;
            const string tugai = MapCatalog.TugaiId;

            // ---------------- Zones (tile the whole region, no overlap) ----------------
            OverworldZone zPomar = Zone(w, "pomar", pomar, null, -300f, -100f, 300f, 350f, OverworldTerrain.Orchard, 1, 3, 1,
                "Grandma Edna's Orchard", "Pomar da Vó Edna", spacing: 72f);
            OverworldZone zCampos = Zone(w, "campos_do_aryk", pomar, null, -300f, -350f, 300f, -100f, OverworldTerrain.Fields, 2, 3, 1,
                "Aryk Fields", "Campos do Aryk", spacing: 70f);
            OverworldZone zEstepe = Zone(w, "estepe_de_ohrmazd", null, pomar, -300f, -1000f, 300f, -350f, OverworldTerrain.Steppe, 4, 6, 3,
                "Ohrmazd Steppe", "Estepe de Ohrmazd", MapRegionTypes.Plain, spacing: 95f);
            OverworldZone zTugai = Zone(w, "tugai_margem", tugai, null, 300f, -350f, 700f, 350f, OverworldTerrain.Riverbank, 2, 5, 2,
                "Golden River Tugai", "Tugai do Rio Dourado", spacing: 85f);
            OverworldZone zKokBori = Zone(w, "circulo_kok_bori", tugai, null, 300f, 350f, 700f, 800f, OverworldTerrain.Riverbank, 4, 5, 2,
                "Kök-Böri's Circle", "Círculo de Kök-Böri", spacing: 90f);
            OverworldZone zCampinas = Zone(w, "campinas_vento_vivo", null, pomar, -700f, -1000f, -300f, 350f, OverworldTerrain.Meadow, 3, 6, 2,
                "Living Wind Meadows", "Campinas do Vento Vivo", MapRegionTypes.Plain, spacing: 95f);
            OverworldZone zPrado = Zone(w, "prado_das_cegonhas", null, pomar, -300f, 350f, 300f, 800f, OverworldTerrain.Meadow, 4, 7, 3,
                "Stork Meadow", "Prado das Cegonhas", MapRegionTypes.Plain, spacing: 90f);
            OverworldZone zColinas = Zone(w, "colinas_mihryazd", null, pomar, -700f, 350f, -300f, 800f, OverworldTerrain.Hills, 7, 11, 5,
                "Mihryazd Hills", "Colinas de Mihryazd", MapRegionTypes.Mountain, spacing: 95f);
            OverworldZone zAltos = Zone(w, "altos_do_mensageiro", null, pomar, -1000f, -1000f, -700f, 350f, OverworldTerrain.HighHills, 8, 12, 6,
                "Messenger's Heights", "Altos do Mensageiro", MapRegionTypes.Mountain, spacing: 100f);
            OverworldZone zRavina = Zone(w, "ravina_bocas_de_luz", null, pomar, -1000f, 350f, -700f, 800f, OverworldTerrain.Ravine, 11, 15, 9,
                "Ravine of the Light Mouths", "Ravina das Bocas de Luz", MapRegionTypes.Mountain, spacing: 100f);
            OverworldZone zVarzea = Zone(w, "varzea_dourada", null, tugai, 300f, -1000f, 700f, -350f, OverworldTerrain.Floodplain, 5, 8, 4,
                "Golden Floodplain", "Várzea Dourada", spacing: 95f);
            OverworldZone zRuinas = Zone(w, "ruinas_aryk_velho", null, pomar, -1000f, 800f, 300f, 1400f, OverworldTerrain.Ruins, 8, 12, 6,
                "Old Aryk Ruins", "Ruínas do Aryk Velho", MapRegionTypes.Ruins, spacing: 110f);
            OverworldZone zMata = Zone(w, "mata_cinco_arvores", null, tugai, 700f, -150f, 1050f, 800f, OverworldTerrain.DenseWood, 10, 15, 8,
                "Wood of the Five Trees", "Mata das Cinco Árvores", spacing: 95f);
            OverworldZone zMataAlta = Zone(w, "mata_alta_zurvan", null, tugai, 300f, 800f, 1050f, 1400f, OverworldTerrain.DenseWood, 12, 16, 10,
                "Zurvan's High Wood", "Mata Alta de Zurvan", spacing: 100f);
            OverworldZone zBrejo = Zone(w, "brejo", MapLayouts.BrejoId, null, 700f, -1000f, 1050f, -150f, OverworldTerrain.Marsh, 16, 22, 14,
                "Dark Mire Marsh", "Brejo do Lodo Escuro", MapRegionTypes.Swamp, MapRequirementTags.LeatherBoots, 95f);
            OverworldZone zAmoreiras = Zone(w, "amoreiras", MapLayouts.AmoreirasId, null, 1050f, -1000f, 1400f, -150f, OverworldTerrain.Mulberry, 18, 24, 16,
                "Silk Mulberry Grove", "Bosque das Amoreiras de Seda", spacing: 100f);
            OverworldZone zGarajonay = Zone(w, "garajonay", MapLayouts.GarajonayId, null, 1050f, -150f, 1400f, 700f, OverworldTerrain.Canopy, 20, 26, 18,
                "Garajonay Canopy", "Copas de Garajonay", MapRegionTypes.Forest, MapRequirementTags.ElevatedPlatforms, 100f);
            OverworldZone zJardim = Zone(w, "jardim_mihrshah", MapLayouts.MihrshahId, null, 1050f, 700f, 1400f, 1400f, OverworldTerrain.Garden, 26, 32, 24,
                "Mihrshah's Forgotten Garden", "Jardim Esquecido de Mihrshah", MapRegionTypes.Ruins, MapRequirementTags.GardenKey, 100f);

            AddFauna(zPomar, zCampos, zEstepe, zTugai, zKokBori, zCampinas, zPrado, zColinas, zAltos, zRavina, zVarzea, zRuinas, zMata, zMataAlta,
                zBrejo, zAmoreiras, zGarajonay, zJardim);

            // ---------------- The hand-made maps, embedded ----------------
            MapLayout pomarSrc = MapLayouts.For(pomar);
            w.Vila = Embed(w, pomarSrc, Vector2.zero, withProps: false, authored: false);
            w.Vila.IsReturnCity = true;

            MapLayout tugaiSrc = MapLayouts.For(tugai);
            Embed(w, tugaiSrc, MoinhoOffset, withProps: true, withRivers: false);
            MapLayout brejoSrc = MapLayouts.For(MapLayouts.BrejoId);
            Embed(w, brejoSrc, BrejoOffset, withProps: true);
            MapLayout amoreirasSrc = MapLayouts.For(MapLayouts.AmoreirasId);
            Embed(w, amoreirasSrc, AmoreirasOffset, withProps: true);
            MapLayout garajonaySrc = MapLayouts.For(MapLayouts.GarajonayId);
            Embed(w, garajonaySrc, GarajonayOffset, withProps: true);
            MapLayout mihrshahSrc = MapLayouts.For(MapLayouts.MihrshahId);
            Embed(w, mihrshahSrc, MihrshahOffset, withProps: true);

            // ---------------- Water ----------------
            // The Rio Dourado, north to south through the whole region; its middle is the Tugai's own S around the Moinho.
            var dourado = new MapRiver { Width = 10f };
            dourado.Points.AddRange(new[]
            {
                new Vector2(520f, 1400f), new Vector2(560f, 1150f), new Vector2(500f, 950f), new Vector2(470f, 800f), new Vector2(440f, 500f),
                new Vector2(360f, 300f), new Vector2(330f, 150f), new Vector2(370f, 98f),
            });
            if (tugaiSrc != null && tugaiSrc.Rivers.Count > 0)
            {
                foreach (Vector2 p in tugaiSrc.Rivers[0].Points)
                {
                    dourado.Points.Add(p + MoinhoOffset);
                }

                foreach (MapCrossing crossing in tugaiSrc.Rivers[0].Crossings)
                {
                    dourado.Crossings.Add(new MapCrossing { At = crossing.At + MoinhoOffset, Width = crossing.Width, IsBridge = crossing.IsBridge });
                }
            }

            dourado.Points.AddRange(new[]
            {
                new Vector2(640f, -60f), new Vector2(680f, -200f), new Vector2(640f, -380f), new Vector2(580f, -560f), new Vector2(620f, -760f),
                new Vector2(560f, -1000f),
            });
            Crossing(dourado, new Vector2(500f, 950f), 6f, false);
            Crossing(dourado, new Vector2(440f, 500f), 4.5f, true);
            Crossing(dourado, new Vector2(330f, 150f), 6f, false);
            // Where the river bends south past the Tugai's east exit, its trail (on to the Mata and the Costa) wades a ford.
            Crossing(dourado, new Vector2(604.5f, -0.9f), 6f, false);
            Vector2 brejoBridge = On(new Vector2(680f, -200f), new Vector2(640f, -380f), 1f / 3f);
            Crossing(dourado, brejoBridge, 4.5f, true);
            Crossing(dourado, new Vector2(580f, -560f), 6f, false);
            Crossing(dourado, new Vector2(620f, -760f), 4.5f, true);
            w.World.Rivers.Add(dourado);

            // The aryk of the fields (the Vila's irrigation canal), three plank bridges.
            MapRiver aryk = River(w, 3.5f, new Vector2(-290f, -195f), new Vector2(-150f, -172f), new Vector2(0f, -185f), new Vector2(150f, -172f), new Vector2(290f, -195f));
            Crossing(aryk, new Vector2(-150f, -172f), 4f, true);
            Crossing(aryk, new Vector2(0f, -185f), 4.5f, true);
            Crossing(aryk, new Vector2(150f, -172f), 4f, true);
            Crossing(aryk, new Vector2(-240f, -187f), 4f, false);

            // The Mihryazd brook from the western heights into the stork lake.
            MapRiver brook = River(w, 6f, new Vector2(-1000f, 470f), new Vector2(-860f, 420f), new Vector2(-700f, 520f), new Vector2(-560f, 600f),
                new Vector2(-420f, 640f), new Vector2(-300f, 660f), new Vector2(-180f, 640f), new Vector2(-122f, 624f));
            Crossing(brook, new Vector2(-860f, 420f), 5f, false);
            Crossing(brook, new Vector2(-560f, 600f), 4.5f, true);
            Crossing(brook, new Vector2(-300f, 660f), 4.5f, true);
            River(w, 40f, new Vector2(-100f, 620f), new Vector2(-30f, 628f));

            // Ponds: the mire pools outside the Brejo map, the floodplain backwater.
            River(w, 22f, new Vector2(760f, -320f), new Vector2(820f, -290f));
            River(w, 18f, new Vector2(1000f, -900f), new Vector2(940f, -950f));
            River(w, 16f, new Vector2(420f, -880f), new Vector2(470f, -900f));

            // ---------------- Gates and the island portal ----------------
            Gate(w, "exit_caravanas", MapExitKind.Map, MapLayouts.CaravanasId, Rect.MinMaxRect(-8f, -1000f, 8f, -997f), new Vector2(0f, -991f), Vector2.up);
            Gate(w, "exit_rondaveis", MapExitKind.Map, MapLayouts.RondaveisId, Rect.MinMaxRect(-1000f, -158f, -997f, -142f), new Vector2(-991f, -150f), Vector2.right);
            Gate(w, "exit_lago_limiar", MapExitKind.Map, MapLayouts.LagoLimiarId, Rect.MinMaxRect(-1000f, 642f, -997f, 658f), new Vector2(-991f, 650f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-77f, 58f, -73f, 62f), new Vector2(-75f, 50f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -75f, 60f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // The Vila's roads out (they start where the old Pomar trails ended).
            Path(w, 4f, 0f, -92f, 0f, -185f, 0f, -350f, 0f, -600f, 0f, -1000f);
            Path(w, 4f, 62f, -20f, 200f, 10f, 398f, 40f);
            Path(w, 4f, -58f, 0f, -300f, -20f, -520f, -60f, -760f, -120f, -1000f, -150f);
            Path(w, 3f, -58f, 0f, -75f, 46f);
            Path(w, 4f, 0f, 46f, 0f, 350f, 15f, 560f, 20f, 800f, 0f, 1000f);
            // Orchard and field lanes.
            Path(w, 3f, -280f, 140f, 280f, 140f);
            Path(w, 3f, -150f, 46f, -150f, 330f);
            Path(w, 3f, 150f, 46f, 150f, 330f);
            Path(w, 3f, -280f, -130f, 280f, -130f);
            Path(w, 3f, -150f, -100f, -150f, -340f);
            Path(w, 3f, 150f, -100f, 150f, -340f);
            Path(w, 3f, -280f, -270f, 280f, -270f);
            // The Tugai: north along the east bank to Kök-Böri's circle, east into the closed wood.
            Vector2 tugaiNorth = new Vector2(480f, 120f);
            Path(w, 3.5f, tugaiNorth.x, tugaiNorth.y, 500f, 330f, 560f, 560f, 600f, 690f);
            Path(w, 3.5f, 15f, 560f, 200f, 540f, 440f, 500f, 560f, 560f);
            Vector2 tugaiEast = Port(tugaiSrc, MoinhoOffset, null, MapExitKind.WorldMap, new Vector2(598f, -4f));
            PathVia(w, 3.5f, tugaiEast, new Vector2(760f, 60f), new Vector2(900f, 200f));
            // The Brejo: from the Tugai's south exit over the golden bridge; from the floodplain to its south trail.
            Vector2 tugaiSouth = Port(tugaiSrc, MoinhoOffset, MapLayouts.BrejoId, MapExitKind.Map, new Vector2(498f, -60f));
            Vector2 brejoNorth = Port(brejoSrc, BrejoOffset, tugai, MapExitKind.Map, new Vector2(840f, -530f));
            PathVia(w, 3.5f, tugaiSouth, new Vector2(580f, -180f), brejoBridge, new Vector2(760f, -360f), brejoNorth);
            Vector2 brejoSouth = Port(brejoSrc, BrejoOffset, null, MapExitKind.WorldMap, new Vector2(880f, -730f));
            Path(w, 3.5f, 0f, -270f, 300f, -420f, 620f, -760f);
            PathVia(w, 3.5f, new Vector2(620f, -760f), new Vector2(760f, -800f), brejoSouth);
            // Brejo → Amoreiras → Copas → Jardim, along the far east.
            Vector2 brejoEast = Port(brejoSrc, BrejoOffset, MapLayouts.AmoreirasId, MapExitKind.Map, new Vector2(980f, -660f));
            Vector2 amoreirasWest = Port(amoreirasSrc, AmoreirasOffset, MapLayouts.BrejoId, MapExitKind.Map, new Vector2(1125f, -560f));
            PathVia(w, 3.5f, brejoEast, new Vector2(1050f, -620f), amoreirasWest);
            Vector2 amoreirasEast = Port(amoreirasSrc, AmoreirasOffset, MapLayouts.GarajonayId, MapExitKind.Map, new Vector2(1325f, -520f));
            Vector2 garajonaySouth = Port(garajonaySrc, GarajonayOffset, null, MapExitKind.WorldMap, new Vector2(1225f, 150f));
            PathVia(w, 3.5f, amoreirasEast, new Vector2(1380f, -420f), new Vector2(1380f, -120f), new Vector2(1225f, 110f), garajonaySouth);
            Vector2 garajonayWest = Port(garajonaySrc, GarajonayOffset, MapLayouts.AmoreirasId, MapExitKind.Map, new Vector2(1075f, 300f));
            PathVia(w, 3.5f, new Vector2(900f, 200f), new Vector2(1000f, 290f), garajonayWest);
            Vector2 garajonayEast = Port(garajonaySrc, GarajonayOffset, MapLayouts.MihrshahId, MapExitKind.Map, new Vector2(1375f, 300f));
            Vector2 mihrshahWest = Port(mihrshahSrc, MihrshahOffset, MapLayouts.GarajonayId, MapExitKind.Map, new Vector2(1128f, 1050f));
            PathVia(w, 3.5f, garajonayEast, new Vector2(1392f, 520f), new Vector2(1300f, 760f), new Vector2(1100f, 880f), new Vector2(1100f, 1030f), mihrshahWest);
            // The west: meadows, heights, the brook, the ravine and its gate.
            Path(w, 3.5f, -520f, -60f, -560f, -450f, -420f, -800f);
            Path(w, 3.5f, -760f, -120f, -850f, -500f, -720f, -860f);
            Path(w, 3.5f, -300f, -20f, -420f, 250f, -560f, 600f, -760f, 680f, -1000f, 650f);
            Path(w, 3f, -560f, 600f, -450f, 820f, -300f, 1000f);
            Path(w, 3f, -760f, 680f, -850f, 780f);
            Path(w, 3f, -420f, 250f, -620f, 200f);
            // The north: ruins, the high wood, the closed wood.
            Path(w, 3.5f, 0f, 1000f, -200f, 1150f, -500f, 1250f, -800f, 1150f);
            Path(w, 3f, -300f, 1000f, -200f, 1150f);
            Path(w, 3.5f, 20f, 800f, 300f, 900f, 500f, 950f, 800f, 1100f, 1000f, 1300f);
            Path(w, 3f, 900f, 200f, 950f, 600f, 880f, 760f);
            Path(w, 3f, 760f, 60f, 1000f, -100f);
            Path(w, 3f, 0f, -600f, -200f, -700f);
            Path(w, 3f, 0f, -600f, 200f, -760f);

            // ---------------- Landmarks ----------------
            Landmark(w, MapLandmarkKind.WaterMill, 352f, 112f, 90f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -96f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, -296f, -12f, 20f);
            Landmark(w, MapLandmarkKind.Signpost, 8f, 352f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 6f, -604f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, -552f, 594f, 40f);
            Landmark(w, MapLandmarkKind.Signpost, 22f, 994f, 0f);
            Landmark(w, MapLandmarkKind.Signpost, 906f, 192f, -30f);
            foreach (Vector2 bed in new[] { new Vector2(-220f, -232f), new Vector2(-80f, -236f), new Vector2(80f, -300f), new Vector2(222f, -232f), new Vector2(-222f, -310f), new Vector2(60f, -230f) })
            {
                Landmark(w, MapLandmarkKind.MelonBed, bed.x, bed.y, TownYaw(bed));
            }

            Landmark(w, MapLandmarkKind.Campfire, 30f, -640f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 614f, 712f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -940f, -170f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -992f, 676f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 40f, 600f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.Campfire, 930f, 222f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -520f, -40f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, -420f, 1250f, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, -760f, 1000f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, 120f, 1250f, 0f, 0.7f);
            Landmark(w, MapLandmarkKind.StoneRing, -640f, 600f, 0f, 0.6f);
            Landmark(w, MapLandmarkKind.StoneRing, 980f, 600f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.StoneRing, FiandeiraEsquecidaArena.x, FiandeiraEsquecidaArena.y, 0f, 1.1f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            return w;
        }

        private static float TownYaw(Vector2 p) => (Mathf.Abs(p.x) + Mathf.Abs(p.y)) % 2f < 1f ? 0f : 90f;

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only)
        // ------------------------------------------------------------------

        private static void AddFauna(OverworldZone pomar, OverworldZone campos, OverworldZone estepe, OverworldZone tugai, OverworldZone kokBori,
            OverworldZone campinas, OverworldZone prado, OverworldZone colinas, OverworldZone altos, OverworldZone ravina, OverworldZone varzea,
            OverworldZone ruinas, OverworldZone mata, OverworldZone mataAlta, OverworldZone brejo, OverworldZone amoreiras, OverworldZone garajonay,
            OverworldZone jardim)
        {
            string owl = NightVariantRules.SpeciesRasgaMortalha;

            // Pomar (01): wolf cubs [rápido], small weaver spiders [enxame]; the smoke owl only watches below level 3.
            pomar.Fauna.Add(Fauna("filhotes", "[rápido]", T(N(Wolf, 1)), T(N(Wolf, 2), V(Wolf, 1), S(NightVariantRules.RasgaMortalha, 1)), 3,
                T(N(Wolf, 3), V(Wolf, 1)), 22f, owl, 3));
            pomar.Fauna.Add(Fauna("tecelas", "[enxame]", T(N(Spider, 1)), T(N(Spider, 1), V(Spider, 2), S(NightVariantRules.RasgaMortalha, 1)), 4,
                null, 20f, owl, 3));

            // Campos do Aryk (01): spiders in the melon beds, wolves at the field edges, sparks over the crops at night.
            campos.Fauna.Add(Fauna("melancial", "[enxame]", T(N(Spider, 3), N(Wolf, 1)), T(V(Spider, 3), S(NightVariantRules.VagaLume, 1)), 4,
                null, 20f, owl, 3));
            campos.Fauna.Add(Fauna("lobos_da_lavoura", "[rápido]", T(N(Wolf, 3)), T(V(Wolf, 3), S(NightVariantRules.RasgaMortalha, 1)), 3,
                null, 24f, owl, 3));

            // Estepe de Ohrmazd: the sand scorpions creeping up from the Areias, steppe wolves.
            estepe.Fauna.Add(Fauna("escorpioes", "[rápido] [controle]", T(N(Scorpion, 3), N(Wolf, 1)), T(V(Scorpion, 3), S(NightVariantRules.VagaLume, 1)), 4));
            estepe.Fauna.Add(Fauna("lobos_da_estepe", "[rápido]", T(N(Wolf, 3), N(Scorpion, 1)), T(V(Wolf, 3), N(Scorpion, 1)), 4));

            // Tugai (02): wolves, spiders and boars along the river; Kök-Böri's circle: the wolf packs and the Mardagayl.
            tugai.Fauna.Add(Fauna("matilha_do_tugai", "[rápido]", T(N(Wolf, 4), N(Spider, 1)), T(V(Wolf, 4), N(Wolf, 1)), 4));
            tugai.Fauna.Add(Fauna("tamargueiral", "[enxame] [controle]", T(N(Spider, 2)), T(V(Spider, 3), S(NightVariantRules.VagaLume, 1)), 5, null, 20f));
            tugai.Fauna.Add(Fauna("javalis", "[tanque]", T(N(Boar, 3), N(Wolf, 1)), T(S(NightVariantRules.Queixada, 2), V(Wolf, 1)), 3, null, 30f));
            kokBori.Fauna.Add(Fauna("matilha_celeste", "[rápido]", T(N(Wolf, 4), N(Boar, 1)), T(V(Wolf, 3), S(NightVariantRules.Lobisomem, 1)), 5, null, 28f));
            kokBori.Fauna.Add(Fauna("javalis_do_circulo", "[tanque]", T(N(Boar, 3)), T(S(NightVariantRules.Queixada, 2), V(Boar, 1)), 3, null, 30f));

            // Campinas do Vento Vivo: boar herds [tanque] in the open grass, wolves.
            campinas.Fauna.Add(Fauna("manada", "[tanque]", T(N(Boar, 3), N(Wolf, 1)), T(S(NightVariantRules.Queixada, 2), V(Boar, 1)), 4, null, 28f));
            campinas.Fauna.Add(Fauna("lobos_do_vento", "[rápido]", T(N(Wolf, 3)), T(V(Wolf, 3), S(NightVariantRules.RasgaMortalha, 1)), 4));
            campinas.Fauna.Add(Fauna("teias_do_capim", "[enxame]", T(N(Spider, 2), N(Boar, 1)), T(V(Spider, 2), S(NightVariantRules.VagaLume, 1)), 4));

            // Prado das Cegonhas: the potoo stands in for the Cegonha Apagada [distância], spiders in the reeds.
            prado.Fauna.Add(Fauna("cegonhas", "[distância]", T(N(EarlyBeasts.Stork, 3), N(Spider, 1)), T(V(EarlyBeasts.Stork, 2), S(NightVariantRules.RasgaMortalha, 1)), 3));
            prado.Fauna.Add(Fauna("juncal", "[enxame] [tanque]", T(N(Spider, 2), N(Boar, 2)), T(V(Spider, 2), S(NightVariantRules.Queixada, 1)), 4));

            // Várzea Dourada: spiders and boars in the mud, birds over the backwater.
            varzea.Fauna.Add(Fauna("lodo_raso", "[tanque] [enxame]", T(N(Boar, 2), N(Spider, 2)), T(S(NightVariantRules.Queixada, 2), V(Spider, 2)), 4));
            varzea.Fauna.Add(Fauna("aves_da_varzea", "[distância]", T(N(EarlyBeasts.Stork, 3)), T(V(EarlyBeasts.Stork, 2), S(NightVariantRules.VagaLume, 1)), 3));

            // Colinas de Mihryazd and the Altos: stone golems [tanque], potoos [distância], wolves; the Mardagayl on the heights.
            colinas.Fauna.Add(Fauna("pedras_vivas", "[tanque]", T(N(Golem, 2), N(Wolf, 1)), T(V(Golem, 2), S(NightVariantRules.RasgaMortalha, 1)), 3, null, 30f));
            colinas.Fauna.Add(Fauna("urutaus", "[distância]", T(N(Potoo, 3), N(Wolf, 1)), T(V(Potoo, 3)), 3));
            altos.Fauna.Add(Fauna("guardas_de_pedra", "[tanque]", T(N(Golem, 3)), T(V(Golem, 2), S(NightVariantRules.Lobisomem, 1)), 3, null, 32f));
            altos.Fauna.Add(Fauna("lobos_dos_altos", "[rápido]", T(N(Wolf, 3), N(Potoo, 1)), T(V(Wolf, 3), V(Potoo, 1)), 4));

            // Ruínas do Aryk Velho: the Jardineiros de Barro (golems), scorpions in the rubble, bats at night.
            ruinas.Fauna.Add(Fauna("jardineiros_de_barro", "[tanque]", T(N(Golem, 3), N(Scorpion, 1)), T(V(Golem, 2), N(Bats, 2)), 3, null, 30f));
            ruinas.Fauna.Add(Fauna("entulho", "[rápido] [enxame]", T(N(Scorpion, 2), N(Bats, 1)), T(N(Bats, 2), S(NightVariantRules.MorcegoVampiro, 1)), 4));

            // Mata das Cinco Árvores and the Mata Alta: the closed wood, every forest beast, the Mardagayl at night.
            mata.Fauna.Add(Fauna("mata_fechada", "[rápido] [enxame]", T(N(Wolf, 2), N(Spider, 2)), T(V(Wolf, 2), V(Spider, 1), S(NightVariantRules.Lobisomem, 1)), 5));
            mata.Fauna.Add(Fauna("javalis_da_mata", "[tanque]", T(N(Boar, 3), N(Spider, 1)), T(S(NightVariantRules.Queixada, 2), S(NightVariantRules.RasgaMortalha, 1)), 4, null, 28f));
            mataAlta.Fauna.Add(Fauna("copas_escuras", "[enxame] [distância]", T(N(Spider, 2), N(Potoo, 2)), T(V(Spider, 2), S(NightVariantRules.RasgaMortalha, 1)), 5));
            mataAlta.Fauna.Add(Fauna("matilha_alta", "[rápido] [tanque]", T(N(Wolf, 3), N(Boar, 2)), T(V(Wolf, 2), S(NightVariantRules.Lobisomem, 1)), 4, null, 28f));

            // Ravina das Bocas de Luz: bat swarms from the cave mouths, golems on the ledges.
            ravina.Fauna.Add(Fauna("bocas", "[enxame]", T(N(Bats, 3)), T(N(Bats, 2), S(NightVariantRules.MorcegoVampiro, 1)), 5, null, 22f));
            ravina.Fauna.Add(Fauna("beiral", "[tanque] [enxame]", T(N(Golem, 2), N(Bats, 1)), T(V(Golem, 2), S(NightVariantRules.MorcegoVampiro, 1)), 3, null, 30f));

            // The far east around the embedded maps (their own clearings come with them).
            brejo.Fauna.Add(Fauna("lodo_escuro", "[tanque] [distância]", T(N(Boar, 3), N(EarlyBeasts.BubbleToad, 2)), T(S(NightVariantRules.Queixada, 2), S(NightVariantRules.VagaLume, 1)), 5));
            brejo.Fauna.Add(Fauna("fumaca", "[enxame]", T(N(EarlyBeasts.SmokeMosquito, 3), N(Spider, 1)), T(V(EarlyBeasts.SmokeMosquito, 3), S(NightVariantRules.VagaLume, 1)), 5, null, 22f));
            amoreiras.Fauna.Add(Fauna("fiandeiras", "[enxame]", T(N(Spider, 4)), T(V(Spider, 3), S(NightVariantRules.VagaLume, 1)), 5, null, 22f));
            amoreiras.Fauna.Add(Fauna("gralhas", "[rápido] [distância]", T(N(EarlyBeasts.SilkMoth, 3), N(Spider, 1)), T(V(EarlyBeasts.SilkMoth, 2), S(NightVariantRules.RasgaMortalha, 1)), 4));
            garajonay.Fauna.Add(Fauna("louros", "[rápido] [tanque]", T(N(EarlyBeasts.LaurelLizard, 2), N(Boar, 2)), T(V(EarlyBeasts.LaurelLizard, 2), S(NightVariantRules.Lobisomem, 1)), 4, null, 28f));
            garajonay.Fauna.Add(Fauna("nevoa", "[controle] [distância]", T(N(Spider, 2), N(EarlyBeasts.LaurelPigeon, 2)), T(V(Spider, 1), V(EarlyBeasts.LaurelPigeon, 2), S(NightVariantRules.VagaLume, 1)), 4));
            jardim.Fauna.Add(Fauna("jardineiros", "[tanque]", T(N(Golem, 3), N(Wolf, 1)), T(V(Golem, 2), S(NightVariantRules.Lobisomem, 1)), 4, null, 30f));
            jardim.Fauna.Add(Fauna("lobos_de_cinzas", "[rápido] [distância]", T(N(Wolf, 3), N(EarlyBeasts.SmokePeacock, 1)), T(V(Wolf, 3), S(NightVariantRules.VagaLume, 1)), 4));
        }
    }
}
