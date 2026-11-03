using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Região 4 — Grutas do Limiar (Docs/Mapas100.md §5, return city Vila das Lamparinas), CaveTuff style of the town kit
    /// (basalt floor, glowing mushrooms and ferns, pink tuff cliffs):
    /// <list type="bullet">
    /// <item><b>19 · Lago do Limiar</b> (M, nv 13–15): the whole <b>Vila das Lamparinas</b> (<see cref="CityTownRecipes.Lamparinas"/>,
    /// the same town of the city walk; the map's return city) against the north wall of the cave, the dark lake to the south
    /// with the shore trail round it, two tunnels between tuff walls: west back to the Passo do Hahya (14), east to the Salão
    /// de Tages (20).</item>
    /// <item><b>20 · Salão de Tages</b> (M, nv 14–18): the outpost <b>Nicho da Ponte</b> (the lamp niche cut in the rock, the
    /// bridge guard's and the wick-maker's tuff houses, the Cais da Barca, lamps strung over the street) by the underground
    /// stream with its plank bridge and a ford, the corridor of columns up to Tages' hall and its ring of stones (the fight
    /// stays the world-map boss challenge).</item>
    /// </list>
    /// </summary>
    public static partial class MapLayouts
    {
        /// <summary>[distância] of the Grutas do Limiar (Mapas100 19; also up the dark ford of the 20).</summary>
        private const string LanternFish = "enemy_cave_lantern_fish";

        /// <summary>The Lodo de Gruta [tanque] of the Lago do Limiar (19, Docs/WorldBible_Maniqueismo.md §3.2.1).</summary>
        private const string CaveOoze = "enemy_cave_ooze";

        // ------------------------------------------------------------------
        // 19 · Lago do Limiar
        // ------------------------------------------------------------------

        private static MapLayout LagoLimiar()
        {
            var m = new MapLayout
            {
                MapId = LagoLimiarId,
                Biome = BiomeType.Caves,
                Bounds = Rect.MinMaxRect(-100f, -170f, 100f, 30f),
                SettlementGroundHalfExtent = 46f,
                SettlementIsReturnCity = true,
            };

            // Step 2: the Lamparinas itself (same recipe as the city walk); the cave's north wall continues its cliff to the
            // map edges and the two tunnels are cut between tuff walls (low on the camera side).
            CityTownRecipe vila = CityTownRecipes.Lamparinas();
            vila.Backdrop = false;
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, -100f, 24f, -46f, 24f));
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, 48f, 24f, 100f, 24f));
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, -100f, -52f, -70f, -52f));
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, -70f, -66f, -100f, -66f, 0.6f, 1));
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, 70f, -52f, 100f, -52f));
            vila.PlazaProps.Add(Run(CityPropKind.Cliff, 99.5f, -66f, 70f, -66f, 0.6f, 1));
            vila.PlazaProps.Add(Piece(CityPropKind.Vishapakar, 6f, -50f, CityGeometry.YawFacing(Vector2.left), 0.9f));
            vila.PlazaProps.Add(Piece(CityPropKind.LampPost, -66f, -56f, 90f));
            vila.PlazaProps.Add(Piece(CityPropKind.LampPost, 66f, -56f, 270f));
            m.Settlement = vila;
            m.SafeZone = Rect.MinMaxRect(-44f, -36f, 46f, 26f);

            // Step 1: the lake (a wide, still water body: no crossing, walked round), the trails.
            var lake = new MapRiver { Width = 28f };
            lake.Points.AddRange(new[] { new Vector2(-30f, -96f), new Vector2(5f, -102f), new Vector2(40f, -94f) });
            m.Rivers.Add(lake);

            m.Paths.Add(new MapPath(new Vector2(0f, -32f), new Vector2(0f, -55f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -55f), new Vector2(-100f, -60f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -55f), new Vector2(100f, -60f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -55f), new Vector2(-62f, -92f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-62f, -92f), new Vector2(-34f, -136f), 3f));
            m.Paths.Add(new MapPath(new Vector2(-34f, -136f), new Vector2(0f, -170f), 3f));

            m.Exits.Add(Border("exit_hahya", MapExitKind.Map, HahyaId, Rect.MinMaxRect(-100f, -65f, -97f, -53f), new Vector2(-93f, -60f), Vector2.right));
            m.Exits.Add(Border("exit_tages", MapExitKind.Map, TagesId, Rect.MinMaxRect(97f, -65f, 100f, -53f), new Vector2(93f, -60f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -170f, 6f, -167f), new Vector2(0f, -163f), Vector2.up));

            // Step 4–5 (Mapas100 19): Enxame de Morcegos do Limiar [enxame]; the Aranha-Tecelã stands in for the Aranha de
            // Gruta [rápido]; the Lodo de Gruta [tanque] oozes out of the west gallery and the south island; the Peixe-Lanterna Apagado [distância]
            // swims over the beach and the south island; Ubır comes out of the lake at night.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "margem_oeste", Center = new Vector2(-70f, -122f), Radius = 11f, MinLevel = 13, MaxLevel = 14, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Bats, 4) },
                Night = new[] { PeriodSpawnEntry.Variant(Bats, 3), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "margem_leste", Center = new Vector2(68f, -120f), Radius = 12f, MinLevel = 13, MaxLevel = 14, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Normal(Bats, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 2), PeriodSpawnEntry.Variant(Bats, 1), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "galeria_oeste", Center = new Vector2(-72f, -18f), Radius = 10f, MinLevel = 13, MaxLevel = 14, MaxAlive = 3, RespawnSeconds = 28f,
                Archetypes = "[tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(CaveOoze, 3) },
                Night = new[] { PeriodSpawnEntry.Variant(CaveOoze, 2), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "praia_do_lago", Center = new Vector2(10f, -136f), Radius = 9f, MinLevel = 14, MaxLevel = 15, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[enxame] [distância] [elite]",
                Day = new[] { PeriodSpawnEntry.Normal(Bats, 3), PeriodSpawnEntry.Normal(LanternFish, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Bats, 2), PeriodSpawnEntry.Variant(LanternFish, 1), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "ilha_sul", Center = new Vector2(56f, -154f), Radius = 10f, MinLevel = 14, MaxLevel = 15, MaxAlive = 4, RespawnSeconds = 30f,
                Archetypes = "[tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(CaveOoze, 2), PeriodSpawnEntry.Normal(LanternFish, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(CaveOoze, 2), PeriodSpawnEntry.Variant(LanternFish, 1), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 2) },
            });

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(4.5f, -60f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-40f, -140f), 0f, 0.7f));

            // Step 3: glowing mushrooms and ferns on the moss, by the shore and across the cave floor; stalagmite rocks;
            // a belt of big rocks beyond the walls.
            Riverside(m, lake, "limiar_margem");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 1, 2 }, 0.22f, "limiar_cogumelos");
            Rocks(m, 40, "limiar_pedras");
            RockBackdrop(m, "limiar_fundo");
            KeepOffSettlement(m);
            return m;
        }

        // ------------------------------------------------------------------
        // 20 · Salão de Tages
        // ------------------------------------------------------------------

        private static MapLayout Tages()
        {
            var m = new MapLayout
            {
                MapId = TagesId,
                Biome = BiomeType.Caves,
                Bounds = Rect.MinMaxRect(-100f, -100f, 100f, 100f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Nicho da Ponte: the lamp niche against a tuff wall, the bridge guard's and the wick-maker's houses
            // cut in the tuff, the Cais da Barca; lamps strung over the street (the cave keeps a permanent dusk).
            var posto = new CityTownRecipe
            {
                CityId = "nicho_da_ponte",
                Region = BiomeType.Caves,
                PlazaRadius = 4.5f,
                StreetWidth = 3.5f,
                SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f,
                EntryArm = CityArm.West,
                AutoYards = false,
                Backdrop = false,
                NightLights = 5,
                AmbientNight = 0.65f,
                LampStrings = true,
                LampSpacing = 5.5f,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "nicho_da_lampada", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down,
                Width = 4.5f, Depth = 6f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.PinkTuff, TrimColor = TownSwatch.Basalt, RoofColor = TownSwatch.Copper, JoineryColor = TownSwatch.WoodDark, AccentColor = TownSwatch.Copper, WallHeight = 3.2f },
            });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_do_nicho", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "casa_do_guarda_da_ponte", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1f,
                Spec = TuffHouse(TownShopDressing.OldMiner, 2.6f),
            });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "casa_do_pavio", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 9f, Width = 5f, Depth = 4.4f, Porch = 1f,
                Spec = TuffHouse(TownShopDressing.Wicks, 3f),
            });
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, -3f, -5.8f, 20f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, 3.4f, -6f, 40f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            posto.PlazaProps.Add(Run(CityPropKind.Cliff, -14f, 15f, 14f, 15f));

            // The cave: its north wall behind Tages' hall, a tuff wall closing the hall on the west.
            posto.PlazaProps.Add(Run(CityPropKind.Cliff, -100f, 94f, 100f, 94f));
            posto.PlazaProps.Add(Run(CityPropKind.Cliff, -40f, 60f, -40f, 93f));

            // The corridor of columns from the stream up to the hall, lamp posts (the niches of the lamps) along it.
            Vector2 a = new Vector2(62f, 40f);
            Vector2 b = new Vector2(10f, 72f);
            Vector2 d = (b - a).normalized;
            var n = new Vector2(-d.y, d.x);
            int k = 0;
            for (float t = 6f; t < Vector2.Distance(a, b) - 2f; t += 8f, k++)
            {
                Vector2 c = a + d * t;
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector2 p = c + n * side * 5.5f;
                    posto.PlazaProps.Add(Piece(CityPropKind.Ruin, p.x, p.y, k * 37f, 1.2f, 1));
                }

                if ((k & 1) == 1)
                {
                    Vector2 lamp = c + n * 3.6f;
                    posto.PlazaProps.Add(Piece(CityPropKind.LampPost, lamp.x, lamp.y, CityGeometry.YawFacing(-n)));
                }
            }

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: the underground stream north–south east of the niche, a plank bridge and a ford; the trails.
            var stream = new MapRiver { Width = 7f };
            stream.Points.AddRange(new[] { new Vector2(24f, -100f), new Vector2(30f, -40f), new Vector2(30f, 40f), new Vector2(24f, 100f) });
            stream.Crossings.Add(new MapCrossing { At = new Vector2(30f, 0f), Width = 4f, IsBridge = true });
            stream.Crossings.Add(new MapCrossing { At = new Vector2(27f, -70f), Width = 5.5f });
            m.Rivers.Add(stream);

            m.Paths.Add(new MapPath(new Vector2(-100f, 0f), new Vector2(-12f, 0f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(60f, 0f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(60f, 0f), new Vector2(62f, 40f), 3.5f));
            m.Paths.Add(new MapPath(a, b, 4f));
            m.Paths.Add(new MapPath(b, new Vector2(-8f, 80f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -14f), new Vector2(0f, -100f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -64f), new Vector2(27f, -70f), 3f));
            m.Paths.Add(new MapPath(new Vector2(27f, -70f), new Vector2(60f, -62f), 3f));

            m.Exits.Add(Border("exit_limiar", MapExitKind.Map, LagoLimiarId, Rect.MinMaxRect(-100f, -6f, -97f, 6f), new Vector2(-93f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -100f, 6f, -97f), new Vector2(0f, -93f), Vector2.up));

            // Step 4–5 (Mapas100 20): Morcegos [enxame]; the Aranha-Tecelã stands in for the Sombra Pequena [rápido] and the
            // Ushabti de Pedra for the Pedra-Lâmina [controle]; [distância]: the Peixe-Lanterna up the dark ford and a
            // Lanterna Quebrada of the Artífices among the columns; Ubır (the light-drinker) is the night's terror.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "galeria_sul", Center = new Vector2(-50f, -55f), Radius = 11f, MinLevel = 14, MaxLevel = 15, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Bats, 4) },
                Night = new[] { PeriodSpawnEntry.Variant(Bats, 3), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "vau_escuro", Center = new Vector2(68f, -66f), Radius = 11f, MinLevel = 15, MaxLevel = 16, MaxAlive = 4, RespawnSeconds = 24f,
                Archetypes = "[rápido] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Normal(LanternFish, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 2), PeriodSpawnEntry.Variant(LanternFish, 1), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "colunata", Center = new Vector2(84f, 62f), Radius = 10f, MinLevel = 16, MaxLevel = 17, MaxAlive = 4, RespawnSeconds = 28f,
                Archetypes = "[tanque] [controle] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(BrokenLantern, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 2), PeriodSpawnEntry.Variant(BrokenLantern, 1), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "antessala_de_tages", Center = new Vector2(-62f, 44f), Radius = 11f, MinLevel = 17, MaxLevel = 18, MaxAlive = 3, RespawnSeconds = 34f,
                Archetypes = "[elite] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 2), PeriodSpawnEntry.Normal(Bats, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 2), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 2) },
            });

            // Tages' hall: the ring of stones where the boss waits.
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.StoneRing, new Vector2(-12f, 82f), 0f, 1.2f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-6f, -8f), 0f, 0.7f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(56f, 4.5f), 0f));

            // Step 3: glowing mushrooms by the stream and on the moss, stalagmite rocks, big rocks beyond the walls.
            Riverside(m, stream, "tages_margem");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 1, 2 }, 0.2f, "tages_cogumelos");
            Rocks(m, 44, "tages_pedras");
            RockBackdrop(m, "tages_fundo");
            KeepOffSettlement(m);
            return m;
        }

        /// <summary>A house cut into the pink tuff (the Lamparinas' look: basalt frames, copper lamps, no roof).</summary>
        private static TownBuildingSpec TuffHouse(TownShopDressing dressing, float wallHeight)
        {
            return new TownBuildingSpec
            {
                WallColor = TownSwatch.PinkTuff, TrimColor = TownSwatch.Basalt, RoofColor = TownSwatch.PinkTuff, JoineryColor = TownSwatch.WoodDark,
                AccentColor = TownSwatch.Copper, Roof = TownRoofKind.Rock, Dressing = dressing, WallHeight = wallHeight, FrontWindows = 1,
            };
        }
    }
}
