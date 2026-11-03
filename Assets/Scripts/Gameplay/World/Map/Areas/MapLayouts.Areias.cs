using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Região 2 — Areias Flamejantes (Docs/Mapas100.md §5, return city Oásis de Qocho), Oasis style of the town kit:
    /// <list type="bullet">
    /// <item><b>07 · Estrada das Caravanas</b> (M, nv 5–7): the wide caravan road west → east (Pomar to the west, Qocho
    /// beyond the east edge), the walled square of the <b>Caravançará do Ammo</b> on it, a north detour by the line of karez
    /// shafts and a south detour to the Ruínas dos Artífices; stone markers along the road and a broken caravan.</item>
    /// <item><b>08 · Ruínas dos Artífices</b> (M, nv 6–9): the <b>Acampamento das Pirâmides</b> by the north entrance, the
    /// avenue of ushabtis down to the ring of lotus columns (the Grande Ushabti's arena; the fight itself stays the
    /// world-map boss challenge), the three inscription stelae of "o painel do aviso", steep Meroë pyramids; east to the
    /// Serra de Kaggen (13).</item>
    /// </list>
    /// </summary>
    public static partial class MapLayouts
    {
        /// <summary>[distância] of the Areias (Mapas100 07 and 08); the lantern also haunts the Salão de Tages (20).</summary>
        private const string EmberVulture = "enemy_desert_ember_vulture";
        private const string BrokenLantern = "enemy_desert_broken_lantern";

        /// <summary>The Areias' own beasts of 07 and 08 (Docs/WorldBible_Maniqueismo.md §3.2.1).</summary>
        private const string Jackal = "enemy_desert_jackal";
        private const string SandSnake = "enemy_desert_sand_snake";

        // ------------------------------------------------------------------
        // 07 · Estrada das Caravanas
        // ------------------------------------------------------------------

        private static MapLayout Caravanas()
        {
            var m = new MapLayout
            {
                MapId = CaravanasId,
                Biome = BiomeType.Desert,
                Bounds = Rect.MinMaxRect(-100f, -90f, 100f, 90f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Caravançará do Ammo, a walled square on the road: the arcaded caravanserai at its head, the
            // cistern house, the bales' house, the Cais da Barca by the west gate; camels, stalls, jars and bales of silk.
            var posto = new CityTownRecipe
            {
                CityId = "caravancara_do_ammo",
                Region = BiomeType.Desert,
                PlazaRadius = 5f,
                StreetWidth = 4.5f,
                SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f,
                EntryArm = CityArm.West,
                AutoYards = false,
                Backdrop = false,
                NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 14f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 14f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "caravancara_do_ammo", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 10.5f), Front = Vector2.down,
                Width = 10f, Depth = 5f, Porch = 2f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.TaipaRose, TrimColor = TownSwatch.Sandstone, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Walnut,
                    AccentColor = TownSwatch.Gold, SignColor = TownSwatch.ClothIndigo, Dressing = TownShopDressing.Selaria, WallHeight = 3.4f,
                    Arcade = true, FullVaranda = true, WideDoor = true,
                },
            });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_da_caravancara", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 9.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "casa_da_cisterna", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 9.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.Adobe, TrimColor = TownSwatch.Sandstone, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Walnut,
                    AccentColor = TownSwatch.Turquoise, Roof = TownRoofKind.Dome, Dressing = TownShopDressing.Cistern,
                },
            });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "casa_dos_fardos", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 10f, Width = 5f, Depth = 4.2f, Porch = 1f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.TaipaRose, TrimColor = TownSwatch.Plaster, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Violet,
                    AccentColor = TownSwatch.Gold, DryingLoft = true, FrontWindows = 1,
                },
            });
            posto.PlazaProps.Add(Piece(CityPropKind.Camel, -3.5f, -7.6f, 0f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stall, 3f, -6.6f, 180f, 1f, 1));
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, -5.4f, -5.2f, 30f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, -11.5f, -8.5f, 20f));
            posto.PlazaProps.Add(Piece(CityPropKind.HayBale, -13.5f, -5.6f, 75f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 5.4f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            // The walls of the square (adobe with coping), open at the two road gates.
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, -16f, -12.5f, 16f, -12.5f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, -16f, 15f, 16f, 15f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, -16f, -12.5f, -16f, -4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, -16f, 4f, -16f, 15f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, 16f, -12.5f, 16f, -4f));
            posto.PlazaProps.Add(Run(CityPropKind.FenceRun, 16f, 4f, 16f, 15f));

            // Across the map: the line of karez shafts to the north, stone markers along the road, the broken caravan to
            // the north-east, broken lotus columns where the south detour leaves for the Ruínas dos Artífices.
            for (int i = 0; i < 8; i++)
            {
                posto.PlazaProps.Add(Piece(CityPropKind.KarezShaft, -90f + i * 9.5f, 82f, i * 41f));
            }

            posto.PlazaProps.Add(Piece(CityPropKind.Stela, -70f, 5.6f, 180f, 0.55f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stela, -45f, -5.6f, 0f, 0.55f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stela, 58f, 5.6f, 180f, 0.55f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stela, 82f, -5.6f, 0f, 0.55f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 46f, 60f, 20f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 49.5f, 63f, 70f, 1.2f));
            posto.PlazaProps.Add(Piece(CityPropKind.HayBale, 44f, 64.5f, 30f));
            posto.PlazaProps.Add(Piece(CityPropKind.HayBale, 51.5f, 58.5f, 80f));
            posto.PlazaProps.Add(Piece(CityPropKind.Stall, 47f, 67.5f, 200f, 1f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 46.5f, -68f, 0f, 1f, 1));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 34f, -76f, 0f, 0.9f, 1));

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-17f, -14f, 17f, 17f);

            // Step 1: the caravan road (5 m) west → east, the north detour by the karez, the south detour to 08.
            m.Paths.Add(new MapPath(new Vector2(-100f, 0f), new Vector2(-16f, 0f), 5f));
            m.Paths.Add(new MapPath(new Vector2(16f, 0f), new Vector2(100f, 0f), 5f));
            m.Paths.Add(new MapPath(new Vector2(-40f, 0f), new Vector2(-62f, 38f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-62f, 38f), new Vector2(-40f, 74f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(34f, 0f), new Vector2(40f, -40f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(40f, -40f), new Vector2(40f, -90f), 3.5f));

            m.Exits.Add(Border("exit_pomar", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(-100f, -7f, -97f, 7f), new Vector2(-93f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_qocho", MapExitKind.WorldMap, null, Rect.MinMaxRect(97f, -7f, 100f, 7f), new Vector2(93f, 0f), Vector2.left));
            m.Exits.Add(Border("exit_artifices", MapExitKind.Map, ArtificesId, Rect.MinMaxRect(34f, -90f, 46f, -87f), new Vector2(40f, -83f), Vector2.up));

            // Step 4–5 (Mapas100 07): Escorpião das Areias Flamejantes [tanque]; the Chacal Apagado [rápido] follows the
            // caravans; at dusk and night the Enxame de Faíscas Presas, and the Mardagayl
            // comes down the karez on some nights ([elite]); the Abutre de Brasa [distância] circles the broken caravan and Qocho.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "dunas_oeste", Center = new Vector2(-62f, -42f), Radius = 11f, MinLevel = 5, MaxLevel = 6, MaxAlive = 4, RespawnSeconds = 24f,
                Archetypes = "[tanque] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Scorpion, 4) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Variant(Scorpion, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
                Night = new[] { PeriodSpawnEntry.Normal(Scorpion, 1), PeriodSpawnEntry.Variant(Scorpion, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "karez_seco", Center = new Vector2(-74f, 58f), Radius = 10f, MinLevel = 5, MaxLevel = 6, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido] [elite]",
                Day = new[] { PeriodSpawnEntry.Normal(Jackal, 3), PeriodSpawnEntry.Normal(Scorpion, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Jackal, 3), PeriodSpawnEntry.Variant(Jackal, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Jackal, 3), PeriodSpawnEntry.Normal(Jackal, 1), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "caravana_quebrada", Center = new Vector2(66f, 42f), Radius = 11f, MinLevel = 6, MaxLevel = 7, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Normal(EmberVulture, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Scorpion, 2), PeriodSpawnEntry.Variant(EmberVulture, 2), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_de_qocho", Center = new Vector2(74f, -48f), Radius = 10f, MinLevel = 6, MaxLevel = 7, MaxAlive = 5, RespawnSeconds = 24f,
                Archetypes = "[tanque] [enxame] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Normal(EmberVulture, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Scorpion, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 3) },
            });

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-37f, 4.5f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(37.5f, -4.5f), 30f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(7f, -10.4f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-48f, 78f), 0f, 0.7f));

            // Step 3: dry steppe turning to sand: a few tamarisks, desert poplars and camel thorn, rocks, palms beyond the walls.
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 10f, new[] { 1, 2, 2, 0 }, 0.16f, "caravanas_ermo");
            Rocks(m, 28, "caravanas_pedras");
            Backdrop(m, new[] { 0, 1, 2, 0 }, "caravanas_fundo");
            KeepOffSettlement(m);
            return m;
        }

        // ------------------------------------------------------------------
        // 08 · Ruínas dos Artífices
        // ------------------------------------------------------------------

        private static MapLayout Artifices()
        {
            var m = new MapLayout
            {
                MapId = ArtificesId,
                Biome = BiomeType.Desert,
                Bounds = Rect.MinMaxRect(-100f, -170f, 100f, 30f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Acampamento das Pirâmides: the guardian lamp, the diggers' tent-house, the guard's domed house, the
            // Cais da Barca on the south street; canvas stalls, jars, crates and the pack camel.
            var posto = new CityTownRecipe
            {
                CityId = "acampamento_das_piramides",
                Region = BiomeType.Desert,
                PlazaRadius = 4.5f,
                StreetWidth = 4f,
                SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f,
                EntryArm = CityArm.North,
                AutoYards = false,
                Backdrop = false,
                NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.North, 13f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.South, 13f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "lampada_do_acampamento", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(9.5f, 0f), Front = Vector2.left,
                Width = 4.5f, Depth = 6f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.Sandstone, TrimColor = TownSwatch.Turquoise, RoofColor = TownSwatch.Turquoise, JoineryColor = TownSwatch.Walnut, AccentColor = TownSwatch.Gold, WallHeight = 3.4f },
            });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "tenda_dos_escavadores", Kind = CityLotKind.House, Arm = CityArm.North, Side = CityArm.West, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.Sandstone, TrimColor = TownSwatch.Plaster, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Walnut,
                    AccentColor = TownSwatch.Gold, Dressing = TownShopDressing.Pottery,
                },
            });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_do_acampamento", Kind = CityLotKind.ShipDock, Arm = CityArm.South, Side = CityArm.West, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "casa_do_guarda", Kind = CityLotKind.House, Arm = CityArm.South, Side = CityArm.East, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.TaipaRose, TrimColor = TownSwatch.Sandstone, RoofColor = TownSwatch.Mud, JoineryColor = TownSwatch.Violet,
                    AccentColor = TownSwatch.Gold, Roof = TownRoofKind.Dome,
                },
            });
            posto.PlazaProps.Add(Piece(CityPropKind.Stall, -8f, 2.4f, 90f, 1f, 1));
            posto.PlazaProps.Add(Piece(CityPropKind.Stall, -8f, -2.4f, 90f, 1f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Camel, 11.5f, -8.5f, 0f));
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, 5.6f, 4.6f, 20f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 12f, 6f, 15f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, -11.5f, 8f, 60f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            // The avenue of ushabtis (rows facing the road on both sides), the three inscription stelae, steep pyramids.
            for (float z = -24f; z >= -120f; z -= 12f)
            {
                if (Mathf.Abs(z + 40f) < 5f || Mathf.Abs(z + 61f) < 5f)
                {
                    continue; // the branches west (z −40) and east (z −61) leave the avenue here
                }

                posto.PlazaProps.Add(Piece(CityPropKind.Ruin, -6f, z, 90f, 1f, 2));
                posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 6f, z, 90f, 1f, 2));
            }

            posto.PlazaProps.Add(Piece(CityPropKind.Stela, -10.5f, -50f, CityGeometry.YawFacing(Vector2.right)));
            posto.PlazaProps.Add(Piece(CityPropKind.Stela, 10.5f, -80f, CityGeometry.YawFacing(Vector2.left)));
            posto.PlazaProps.Add(Piece(CityPropKind.Stela, -10.5f, -110f, CityGeometry.YawFacing(Vector2.right)));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, -30f, -20f, 15f, 2.4f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 32f, -30f, -10f, 2.8f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, -34f, -96f, 25f, 2.6f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 28f, -122f, -20f, 2.2f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 70f, -30f, 5f, 3f, 0));

            // The arena: a ring of broken lotus columns open to the avenue, two pyramids flanking it.
            var arena = new Vector2(0f, -145f);
            foreach (float angle in new[] { 0f, 40f, 140f, 180f, 220f, 260f, 300f, 320f })
            {
                Vector2 p = arena + CityTownRecipes.Polar(angle, 12f);
                posto.PlazaProps.Add(Piece(CityPropKind.Ruin, p.x, p.y, angle, 1.2f, 1));
            }

            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, -24f, -158f, 10f, 2.2f, 0));
            posto.PlazaProps.Add(Piece(CityPropKind.Ruin, 24f, -158f, -10f, 2.2f, 0));

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -15f, 16f, 15f);

            // Step 1: the north road from the Caravan Road, the avenue down to the arena, the branches east and west.
            m.Paths.Add(new MapPath(new Vector2(0f, 13f), new Vector2(0f, 30f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -13f), new Vector2(0f, -132f), 5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -62f), new Vector2(100f, -60f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -40f), new Vector2(-100f, -40f), 3.5f));

            m.Exits.Add(Border("exit_caravanas", MapExitKind.Map, CaravanasId, Rect.MinMaxRect(-6f, 27f, 6f, 30f), new Vector2(0f, 23f), Vector2.down));
            m.Exits.Add(Border("exit_rondaveis", MapExitKind.Map, RondaveisId, Rect.MinMaxRect(97f, -66f, 100f, -54f), new Vector2(93f, -60f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-100f, -46f, -97f, -34f), new Vector2(-93f, -40f), Vector2.right));

            // Step 4–5 (Mapas100 08): the Ushabti de Pedra [tanque] stands in for the Ushabti de Areia, the Cobra-de-Areia
            // [rápido] slides between their feet, the Escorpião [tanque]; the Lanterna Quebrada dos Artífices [distância] floats among the pyramids and the lotus columns; at
            // night the ushabtis "wake" (night variants, glowing eyes) and the sparks swarm over the stones.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "avenida_oeste", Center = new Vector2(-48f, -66f), Radius = 11f, MinLevel = 6, MaxLevel = 7, MaxAlive = 4, RespawnSeconds = 24f,
                Archetypes = "[tanque] [rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(SandSnake, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 3), PeriodSpawnEntry.Variant(SandSnake, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "piramides_leste", Center = new Vector2(52f, -95f), Radius = 12f, MinLevel = 7, MaxLevel = 8, MaxAlive = 5, RespawnSeconds = 24f,
                Archetypes = "[tanque] [enxame] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Normal(BrokenLantern, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Scorpion, 3), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Scorpion, 2), PeriodSpawnEntry.Variant(BrokenLantern, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "colunas_oeste", Center = new Vector2(-58f, -128f), Radius = 12f, MinLevel = 7, MaxLevel = 8, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Scorpion, 2), PeriodSpawnEntry.Normal(BrokenLantern, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Scorpion, 2), PeriodSpawnEntry.Variant(BrokenLantern, 2), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "antessala_do_ushabti", Center = new Vector2(52f, -146f), Radius = 10f, MinLevel = 8, MaxLevel = 9, MaxAlive = 3, RespawnSeconds = 32f,
                Archetypes = "[elite] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(Scorpion, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 4), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(4.5f, -57f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-4.5f, -35f), 180f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-12.5f, 0.5f), 0f, 0.8f));

            // Step 3: stony desert: rocks, a few tamarisks and poplars, camel thorn, palms beyond the walls.
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 11f, new[] { 1, 2, 0 }, 0.12f, "artifices_ermo");
            Rocks(m, 34, "artifices_pedras");
            Backdrop(m, new[] { 0, 2, 1 }, "artifices_fundo");
            KeepOffSettlement(m);
            return m;
        }
    }
}
