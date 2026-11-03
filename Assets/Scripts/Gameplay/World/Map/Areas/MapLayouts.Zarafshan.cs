using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Região 1 — Bosque do Zarafshan, the maps after the Pomar and the Tugai (Docs/Mapas100.md §5, Sogdian style of the
    /// town kit; return city Vila da Vó Edna for 03–04, Copas de Garajonay for 05–06):
    /// <list type="bullet">
    /// <item><b>03 · Brejo do Lodo Escuro</b> (M, nv 16–22): the outpost <b>Ponte do Aryk</b> (the lamp, the boatman's house,
    /// the Cais da Barca) on the north island, two black water channels crossed only on log walkways, the Poço Turvo in the
    /// west fen (Mãe-Lodo's ring; the fight stays the world-map boss challenge); north to the Tugai (02), east to 04.</item>
    /// <item><b>04 · Bosque das Amoreiras de Seda</b> (M, nv 18–24): the outpost <b>Casa do Tear</b> (the weaver's workshop)
    /// in the middle of the abandoned mulberry rows (a soft maze of four blocks), four ruined weavers' yards with their
    /// looms and drying lines; west to the Brejo (03), east to the Copas (05).</item>
    /// <item><b>05 · Copas de Garajonay</b> (G, nv 20–26): the race city of the elfos-da-mata
    /// (<see cref="CityTownRecipes.Garajonay"/>; the map's return city) with its canopy of giant laurels, ring platforms and
    /// rope bridges; the laurel forest floor with three trails and two cascading streams, the Velho da Névoa's ring to the
    /// north; west to 04, east to the Jardim (06).</item>
    /// <item><b>06 · Jardim Esquecido de Mihrshah</b> (M, nv 26–32): the outpost <b>Portão do Jardim</b> at the west gate,
    /// the walled chahar bagh (four quadrants, dry canals in a cross, cypress rows, roses, dry fountains) and the pavilion of
    /// columns at the centre (Saklas's arena; the fight stays the world-map boss challenge); west back to the Copas.</item>
    /// </list>
    /// Monsters reuse today's Forest definitions (Lobo do Tugai, Aranha-Tecelã, Javali) and the Ushabti de Pedra for the
    /// Jardineiro de Barro; the night tables use the playable night species of <see cref="NightVariantRules"/>. Each layout
    /// is self-contained in its own <see cref="MapLayout.Bounds"/> (the settlement is generated at the layout's origin).
    /// </summary>
    public static partial class MapLayouts
    {
        public const string BrejoId = "map_003_brejo";
        public const string AmoreirasId = "map_004_amoreiras";
        public const string GarajonayId = "map_005_garajonay";
        public const string MihrshahId = "map_006_mihrshah";

        /// <summary>The Zarafshan maps this build can lay out after 01–02, in campaign order.</summary>
        public static readonly IReadOnlyList<string> ZarafshanMapIds = new[] { BrejoId, AmoreirasId, GarajonayId, MihrshahId };

        private static bool HasZarafshan(string mapId)
        {
            foreach (string id in ZarafshanMapIds)
            {
                if (id == mapId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>A small Zarafshan outpost: lamp house to the north, W/E arms to the map, Cais da Barca, a workshop and a house.</summary>
        private static CityTownRecipe ZarafshanPost(string cityId, string workshopId, string houseId, TownSwatch lampTrim)
        {
            var posto = new CityTownRecipe
            {
                CityId = cityId,
                Region = BiomeType.Forest,
                PlazaRadius = 4.5f,
                StreetWidth = 3.5f,
                SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f,
                EntryArm = CityArm.West,
                AutoYards = false,
                Backdrop = false,
                NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "lampada_" + cityId, Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down,
                Width = 4.5f, Depth = 6f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.Plaster, TrimColor = lampTrim, RoofColor = lampTrim, JoineryColor = TownSwatch.Walnut, AccentColor = TownSwatch.Gold, WallHeight = 3.4f },
            });
            posto.Lots.Add(new CityLotRecipe { Id = workshopId, Kind = CityLotKind.Workshop, Arm = CityArm.East, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 4.6f, Porch = 1.4f });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_" + cityId, Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe { Id = houseId, Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f });
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            return posto;
        }

        // ------------------------------------------------------------------
        // 03 · Brejo do Lodo Escuro
        // ------------------------------------------------------------------

        private static MapLayout Brejo()
        {
            var m = new MapLayout
            {
                MapId = BrejoId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-100f, -170f, 100f, 30f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Ponte do Aryk outpost on the north island: the lamp, the boatman's house, the reed-cutter's
            // workshop, the Cais da Barca; two more lamps on the stone bridge over the first channel.
            CityTownRecipe posto = ZarafshanPost("ponte_do_aryk", "oficina_do_junco", "casa_do_barqueiro", TownSwatch.Turquoise);
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, 3.2f, -5.6f, 40f));
            posto.PlazaProps.Add(Piece(CityPropKind.HayBale, -9f, -8.5f, 30f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 13.5f, -9.5f, 15f));
            posto.PlazaProps.Add(Piece(CityPropKind.LampPost, 23.5f, -34f, 270f));
            posto.PlazaProps.Add(Piece(CityPropKind.LampPost, 16.5f, -44f, 90f));
            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: two channels of black water across the fen, crossed only on log walkways (bridges).
            var north = new MapRiver { Width = 7f };
            north.Points.AddRange(new[] { new Vector2(-100f, -30f), new Vector2(-50f, -40f), new Vector2(0f, -36f), new Vector2(50f, -44f), new Vector2(100f, -38f) });
            north.Crossings.Add(new MapCrossing { At = new Vector2(20f, -39.2f), Width = 4f, IsBridge = true });
            north.Crossings.Add(new MapCrossing { At = new Vector2(-60f, -38f), Width = 4f, IsBridge = true });
            m.Rivers.Add(north);

            var south = new MapRiver { Width = 7f };
            south.Points.AddRange(new[] { new Vector2(-100f, -115f), new Vector2(-40f, -125f), new Vector2(20f, -118f), new Vector2(70f, -128f), new Vector2(100f, -122f) });
            south.Crossings.Add(new MapCrossing { At = new Vector2(0f, -120.33333f), Width = 4f, IsBridge = true });
            m.Rivers.Add(south);

            m.Paths.Add(new MapPath(new Vector2(-12f, 0f), new Vector2(-40f, 0f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-40f, 0f), new Vector2(-40f, 30f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(20f, -10f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(20f, -10f), new Vector2(20f, -80f), 3f));
            m.Paths.Add(new MapPath(new Vector2(20f, -80f), new Vector2(70f, -80f), 3f));
            m.Paths.Add(new MapPath(new Vector2(70f, -80f), new Vector2(70f, -100f), 3f));
            m.Paths.Add(new MapPath(new Vector2(70f, -100f), new Vector2(100f, -100f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(20f, -80f), new Vector2(0f, -95f), 3f));
            m.Paths.Add(new MapPath(new Vector2(0f, -95f), new Vector2(0f, -170f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-40f, 0f), new Vector2(-60f, -15f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-60f, -15f), new Vector2(-60f, -90f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-60f, -90f), new Vector2(-30f, -90f), 3f));

            m.Exits.Add(Border("exit_tugai", MapExitKind.Map, MapCatalog.TugaiId, Rect.MinMaxRect(-46f, 27f, -34f, 30f), new Vector2(-40f, 23f), Vector2.down));
            m.Exits.Add(Border("exit_amoreiras", MapExitKind.Map, AmoreirasId, Rect.MinMaxRect(97f, -106f, 100f, -94f), new Vector2(93f, -100f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -170f, 6f, -167f), new Vector2(0f, -163f), Vector2.up));

            // Step 4–5 (Mapas100 03): Javali do Lodo [tanque]; the Lobo do Tugai stands in for the Enguia de Lama [rápido],
            // the Aranha-Tecelã and the bat swarm for the Mosquitos de Fumaça [enxame]; at night the herd of the Javali do
            // Lodo, the trapped sparks (Fogo-Fátuo Preso) and, rarely, Mardagayl.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "juncal_norte", Center = new Vector2(55f, -15f), Radius = 11f, MinLevel = 16, MaxLevel = 17, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 3), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Boar, 2), PeriodSpawnEntry.Species(NightVariantRules.Queixada, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "lodo_oeste", Center = new Vector2(-82f, -65f), Radius = 11f, MinLevel = 17, MaxLevel = 19, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[rápido] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Normal(Spider, 2) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Spider, 2), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "ilha_do_meio", Center = new Vector2(45f, -62f), Radius = 9f, MinLevel = 18, MaxLevel = 20, MaxAlive = 4, RespawnSeconds = 24f,
                Archetypes = "[rápido] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Boar, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Boar, 1), PeriodSpawnEntry.Species(NightVariantRules.Queixada, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "poca_leste", Center = new Vector2(60f, -150f), Radius = 11f, MinLevel = 19, MaxLevel = 21, MaxAlive = 6, RespawnSeconds = 20f,
                Archetypes = "[enxame] [controle]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Normal(Bats, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 2), PeriodSpawnEntry.Variant(Bats, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_do_poco", Center = new Vector2(-60f, -145f), Radius = 11f, MinLevel = 20, MaxLevel = 22, MaxAlive = 4, RespawnSeconds = 30f,
                Archetypes = "[tanque] [elite]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 3), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Boar, 2), PeriodSpawnEntry.Species(NightVariantRules.Queixada, 2), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            ClearingsFromZones(m);

            // The Poço Turvo, where the Mãe-Lodo swallowed the spring (the boss clearing).
            m.Clearings.Add(new Vector2(-30f, -90f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.StoneRing, new Vector2(-30f, -96f), 0f, 0.9f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-4.5f, -8f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-36f, 4f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(24f, -84f), 30f));

            // Step 3: the drowned tugai: poplars and tamarisks along the channels, reeds and tamarisk thickets on the
            // islands, rotten trunks and stones, a dark belt of trees beyond the walls.
            Riverside(m, north, "brejo_canal_n");
            Riverside(m, south, "brejo_canal_s");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 8.5f, new[] { 2, 0, 2, 1 }, 0.42f, "brejo_mata");
            Rocks(m, 30, "brejo_troncos");
            Backdrop(m, new[] { 2, 0, 1 }, "brejo_fundo");
            KeepOffSettlement(m);
            return m;
        }

        // ------------------------------------------------------------------
        // 04 · Bosque das Amoreiras de Seda
        // ------------------------------------------------------------------

        private static MapLayout Amoreiras()
        {
            var m = new MapLayout
            {
                MapId = AmoreirasId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-100f, -100f, 100f, 100f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Casa do Tear (the weaver who mends clothes) and, through the rows, the four ruined weavers' yards:
            // broken walls, a loom (trellis), crates of cocoons.
            CityTownRecipe posto = ZarafshanPost("casa_do_tear", "oficina_do_tear", "casa_da_tecela", TownSwatch.Violet);
            posto.PlazaProps.Add(Piece(CityPropKind.Trellis, -3f, -6f, 0f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 3.4f, -5.8f, 25f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stall, 13.5f, -9.5f, 180f, 1f, 1));
            foreach (Vector2 yard in new[] { new Vector2(-80f, 35f), new Vector2(-28f, -30f), new Vector2(25f, -75f), new Vector2(20f, 40f) })
            {
                Pen(posto, yard, 5f, 4f, 3f);
                posto.PlazaProps.Add(Piece(CityPropKind.Trellis, yard.x - 1.5f, yard.y + 1.2f, 0f));
                posto.PlazaProps.Add(Piece(CityPropKind.Crates, yard.x + 2.5f, yard.y + 1.5f, 35f));
            }

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: the road west–east, the lanes between the mulberry blocks.
            m.Paths.Add(new MapPath(new Vector2(-100f, 0f), new Vector2(-12f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(40f, 0f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(40f, 0f), new Vector2(60f, 40f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(60f, 40f), new Vector2(100f, 40f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -14f), new Vector2(0f, -100f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-60f, 0f), new Vector2(-60f, 70f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-60f, 70f), new Vector2(20f, 70f), 3f));
            m.Paths.Add(new MapPath(new Vector2(40f, 0f), new Vector2(55f, -60f), 3f));

            m.Exits.Add(Border("exit_brejo", MapExitKind.Map, BrejoId, Rect.MinMaxRect(-100f, -6f, -97f, 6f), new Vector2(-93f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_garajonay", MapExitKind.Map, GarajonayId, Rect.MinMaxRect(97f, 34f, 100f, 46f), new Vector2(93f, 40f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -100f, 6f, -97f), new Vector2(0f, -93f), Vector2.up));

            // Step 4–5 (Mapas100 04): Aranha-Tecelã Apagada [enxame]; the Javali stands in for the Lagarta-Casulo [tanque]
            // and the Lobo for the Gralha Ladra de Fio [rápido]; at night the Coruja-de-Fumaça hunts and the moths light up
            // (the trapped sparks).
            m.Zones.Add(new MapSpawnZone
            {
                Id = "fileiras_oeste", Center = new Vector2(-55f, -55f), Radius = 11f, MinLevel = 18, MaxLevel = 19, MaxAlive = 5, RespawnSeconds = 20f,
                Archetypes = "[enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 4) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Variant(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "casulos", Center = new Vector2(-30f, 45f), Radius = 11f, MinLevel = 19, MaxLevel = 21, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 2), PeriodSpawnEntry.Normal(Spider, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Boar, 2), PeriodSpawnEntry.Variant(Spider, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "varais", Center = new Vector2(75f, -50f), Radius = 11f, MinLevel = 20, MaxLevel = 22, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Normal(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "tear_grande", Center = new Vector2(60f, 80f), Radius = 11f, MinLevel = 22, MaxLevel = 24, MaxAlive = 4, RespawnSeconds = 30f,
                Archetypes = "[elite] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Normal(Boar, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            ClearingsFromZones(m);

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-4.5f, -8f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-20f, 4f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(44f, -5f), 30f));

            // Step 3: the abandoned mulberry rows (a soft maze of four blocks), poplars on the edges.
            OrchardRows(m, Rect.MinMaxRect(-95f, -95f, -14f, -18f), 8.5f, 10f, "amoreiras_so");
            OrchardRows(m, Rect.MinMaxRect(14f, -95f, 95f, -18f), 8.5f, 10f, "amoreiras_se");
            OrchardRows(m, Rect.MinMaxRect(-95f, 18f, -14f, 95f), 8.5f, 10f, "amoreiras_no");
            OrchardRows(m, Rect.MinMaxRect(14f, 18f, 95f, 95f), 8.5f, 10f, "amoreiras_ne");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 1, 2 }, 0.18f, "amoreiras_mata");
            Rocks(m, 14, "amoreiras_pedras");
            Backdrop(m, new[] { 3, 0, 1 }, "amoreiras_fundo");
            KeepOffSettlement(m);
            return m;
        }

        // ------------------------------------------------------------------
        // 05 · Copas de Garajonay
        // ------------------------------------------------------------------

        private static MapLayout Garajonay()
        {
            var m = new MapLayout
            {
                MapId = GarajonayId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-150f, -150f, 150f, 150f),
                SettlementGroundHalfExtent = 46f,
                SettlementIsReturnCity = true,
            };

            // Step 2: the city of the elfos-da-mata itself, its backdrop replaced by the laurel forest.
            CityTownRecipe copas = CityTownRecipes.Garajonay();
            copas.Backdrop = false;
            m.Settlement = copas;
            m.SafeZone = Rect.MinMaxRect(-44f, -36f, 42f, 34f);

            // Step 1: two streams falling from the north edge (the cascades), the three trails of the forest floor.
            var west = new MapRiver { Width = 6f };
            west.Points.AddRange(new[] { new Vector2(-90f, 150f), new Vector2(-80f, 80f), new Vector2(-95f, 20f), new Vector2(-85f, -60f), new Vector2(-100f, -150f) });
            west.Crossings.Add(new MapCrossing { At = new Vector2(-92.5f, 0f), Width = 4f, IsBridge = true });
            m.Rivers.Add(west);

            var east = new MapRiver { Width = 6f };
            east.Points.AddRange(new[] { new Vector2(95f, 150f), new Vector2(80f, 60f), new Vector2(100f, -10f), new Vector2(85f, -80f), new Vector2(110f, -150f) });
            east.Crossings.Add(new MapCrossing { At = new Vector2(97.142857f, 0f), Width = 4f, IsBridge = true });
            m.Rivers.Add(east);

            m.Paths.Add(new MapPath(new Vector2(-150f, 0f), new Vector2(-36f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(32f, 0f), new Vector2(150f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -32f), new Vector2(0f, -150f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-62f, 0f), new Vector2(-50f, 90f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-50f, 90f), new Vector2(-20f, 118f), 3f));

            m.Exits.Add(Border("exit_amoreiras", MapExitKind.Map, AmoreirasId, Rect.MinMaxRect(-150f, -6f, -147f, 6f), new Vector2(-143f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_mihrshah", MapExitKind.Map, MihrshahId, Rect.MinMaxRect(147f, -6f, 150f, 6f), new Vector2(143f, 0f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -150f, 6f, -147f), new Vector2(0f, -143f), Vector2.up));

            // Step 4–5 (Mapas100 05): the Lobo stands in for the Lagarto-Gigante do Louro [rápido], the Javali is the Javali
            // de Musgo [tanque], the Aranha-Tecelã the Névoa Pegajosa [controle]; at night Mardagayl in the low forest and the
            // fireflies over the trails.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "samambaial", Center = new Vector2(-60f, -70f), Radius = 12f, MinLevel = 20, MaxLevel = 21, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Normal(Boar, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "musgo", Center = new Vector2(55f, -80f), Radius = 12f, MinLevel = 21, MaxLevel = 23, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 3), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Boar, 2), PeriodSpawnEntry.Species(NightVariantRules.Queixada, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "nevoa_baixa", Center = new Vector2(-120f, 85f), Radius = 12f, MinLevel = 22, MaxLevel = 24, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[controle] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 4) },
                Dawn = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Variant(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "louros_altos", Center = new Vector2(50f, 90f), Radius = 12f, MinLevel = 23, MaxLevel = 25, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[rápido] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Boar, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Boar, 1), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "cascata_leste", Center = new Vector2(125f, -100f), Radius = 11f, MinLevel = 24, MaxLevel = 26, MaxAlive = 3, RespawnSeconds = 32f,
                Archetypes = "[elite] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 2), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Boar, 2), PeriodSpawnEntry.Variant(Wolf, 1), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_da_cascata", Center = new Vector2(-125f, -115f), Radius = 11f, MinLevel = 25, MaxLevel = 26, MaxAlive = 5, RespawnSeconds = 24f,
                Archetypes = "[enxame] [controle]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            ClearingsFromZones(m);

            // The Velho da Névoa's ring at the end of the north trail (the boss clearing).
            m.Clearings.Add(new Vector2(-16f, 124f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.StoneRing, new Vector2(-16f, 124f), 0f, 1.1f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-66f, 4f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(4f, -40f), 0f));

            // Step 3: the laurisilva: dense dark laurels (poplar shapes in dark green), heather, the cascades' boulders, the
            // stream banks, a belt of tall trees beyond the walls.
            RockLine(m, new Vector2(-96f, 146f), new Vector2(-84f, 110f), 3f, 1.4f, 2.6f, "garajonay_cascata_o");
            RockLine(m, new Vector2(102f, 146f), new Vector2(90f, 104f), 3f, 1.4f, 2.6f, "garajonay_cascata_l");
            Riverside(m, west, "garajonay_margem_o");
            Riverside(m, east, "garajonay_margem_l");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 10f, new[] { 0, 0, 1, 2 }, 0.42f, "garajonay_louros");
            Rocks(m, 30, "garajonay_pedras");
            Backdrop(m, new[] { 0, 0, 1 }, "garajonay_fundo");
            KeepOffSettlement(m, 2.4f);
            return m;
        }

        // ------------------------------------------------------------------
        // 06 · Jardim Esquecido de Mihrshah
        // ------------------------------------------------------------------

        private static MapLayout Mihrshah()
        {
            var m = new MapLayout
            {
                MapId = MihrshahId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-22f, -100f, 178f, 100f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Portão do Jardim (the lamp and the gardener's house) and the walled chahar bagh east of it: the
            // wall with its four gates, the dry canals in a cross, cypress rows along the axes, roses, four dry fountains
            // and the pavilion of columns at the centre.
            CityTownRecipe posto = ZarafshanPost("portao_do_jardim", "casa_do_jardineiro", "abrigo_do_portao", TownSwatch.Turquoise);
            posto.PlazaProps.Add(Piece(CityPropKind.FlowerBed, -3f, -6f, 0f, 1f, 1));
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, 3.4f, -5.8f, 25f));
            var garden = new Vector2(80f, 0f);
            const float half = 58f;
            const float gate = 4f;
            float x0 = garden.x - half, x1 = garden.x + half, z0 = -half, z1 = half;
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z1, garden.x - gate, z1, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, garden.x + gate, z1, x1, z1, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z0, garden.x - gate, z0, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, garden.x + gate, z0, x1, z0, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, z0, x0, -gate, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x0, gate, x0, z1, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x1, z0, x1, -gate, 2.4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, x1, gate, x1, z1, 2.4f));

            posto.PlazaProps.Add(Run(CityPropKind.KarezChannel, x0 + 2f, 0f, garden.x - 9f, 0f));
            posto.PlazaProps.Add(Run(CityPropKind.KarezChannel, garden.x + 9f, 0f, x1 - 2f, 0f));
            posto.PlazaProps.Add(Run(CityPropKind.KarezChannel, garden.x, z0 + 2f, garden.x, -9f));
            posto.PlazaProps.Add(Run(CityPropKind.KarezChannel, garden.x, 9f, garden.x, z1 - 2f));

            for (float t = 14f; t <= half - 4f; t += 8f)
            {
                foreach (float side in new[] { -6f, 6f })
                {
                    posto.PlazaProps.Add(Piece(CityPropKind.Tree, garden.x - t, side, 0f, 1.1f, 0));
                    posto.PlazaProps.Add(Piece(CityPropKind.Tree, garden.x + t, side, 0f, 1.1f, 0));
                    posto.PlazaProps.Add(Piece(CityPropKind.Tree, garden.x + side, -t, 0f, 1.1f, 0));
                    posto.PlazaProps.Add(Piece(CityPropKind.Tree, garden.x + side, t, 0f, 1.1f, 0));
                }
            }

            for (int k = 0; k < 8; k++)
            {
                Vector2 column = garden + CityTownRecipes.Polar(22.5f + k * 45f, 8f);
                posto.PlazaProps.Add(Piece(CityPropKind.Ruin, column.x, column.y, k * 45f, 1.2f, 1));
            }

            foreach (Vector2 q in new[] { new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f) })
            {
                Vector2 fountain = garden + q * 16f;
                posto.PlazaProps.Add(Piece(CityPropKind.Well, fountain.x, fountain.y, 0f, 1.2f));
                posto.PlazaProps.Add(Piece(CityPropKind.Bush, garden.x + q.x * 44f, q.y * 14f, 0f, 1.2f));
                posto.PlazaProps.Add(Piece(CityPropKind.Bush, garden.x + q.x * 14f, q.y * 44f, 0f, 1.2f));
                posto.PlazaProps.Add(Piece(CityPropKind.FlowerBed, garden.x + q.x * 24f, q.y * 12f, 0f, 1f, 1));
            }

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: the axes of the garden and the road out to the south.
            m.Paths.Add(new MapPath(new Vector2(-22f, 0f), new Vector2(-12f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(x1, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(garden.x, z0), new Vector2(garden.x, z1), 4f));
            m.Paths.Add(new MapPath(new Vector2(garden.x, z0), new Vector2(garden.x, -100f), 4f));

            m.Exits.Add(Border("exit_garajonay", MapExitKind.Map, GarajonayId, Rect.MinMaxRect(-22f, -7f, -19f, 7f), new Vector2(-16.5f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(garden.x - 6f, -100f, garden.x + 6f, -97f), new Vector2(garden.x, -93f), Vector2.up));

            // Step 4–5 (Mapas100 06): one quadrant per spark: the Aranha-Tecelã stands in for the Roseira-Espinho [controle],
            // the Lobo for the Lobo de Cinzas [rápido], the Ushabti de Pedra is the Jardineiro de Barro [tanque]; at night
            // the roses glow (trapped sparks), the Coruja-de-Fumaça and Mardagayl, Saklas's wolf.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "quadrante_noroeste", Center = new Vector2(50f, 30f), Radius = 10f, MinLevel = 26, MaxLevel = 27, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[controle] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 4) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "quadrante_nordeste", Center = new Vector2(110f, 30f), Radius = 10f, MinLevel = 27, MaxLevel = 29, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Normal(Spider, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Variant(Wolf, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "quadrante_sudoeste", Center = new Vector2(50f, -30f), Radius = 10f, MinLevel = 28, MaxLevel = 30, MaxAlive = 4, RespawnSeconds = 28f,
                Archetypes = "[tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "quadrante_sudeste", Center = new Vector2(110f, -30f), Radius = 10f, MinLevel = 30, MaxLevel = 32, MaxAlive = 4, RespawnSeconds = 32f,
                Archetypes = "[elite] [rápido] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Golem, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Golem, 1), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            ClearingsFromZones(m);

            // The pavilion: Saklas's arena (the fight stays the world-map boss challenge).
            m.Clearings.Add(garden);
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-4.5f, -8f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(16f, 5f), 0f));

            // Step 3: the garden gone wild (thin), the forest outside the walls, stones.
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 2, 1 }, 0.3f, "mihrshah_mato");
            Rocks(m, 16, "mihrshah_pedras");
            Backdrop(m, new[] { 0, 0, 2 }, "mihrshah_fundo");
            KeepOffSettlement(m);
            return m;
        }
    }
}
