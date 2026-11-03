using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Região 3 — Serra de Kaggen (Docs/Mapas100.md §5, return city Vila de Litema), Rondavel style of the town kit:
    /// <list type="bullet">
    /// <item><b>13 · Trilha dos Rondavéis</b> (M, nv 9–11): the whole <b>Vila de Litema</b> (<see cref="CityTownRecipes.Litema"/>,
    /// the same town of the city walk; the map's return city) on the plateau to the north, the zig-zag trail down the
    /// golden grass slopes with two dry-stone pens (currais), rock-art shelters and a baobab; west to the Ruínas dos
    /// Artífices (08), east to the Passo do Hahya (14).</item>
    /// <item><b>14 · Passo do Hahya</b> (M, nv 10–13): the outpost <b>Boca da Mina do Mihr</b> (the mine forge, the
    /// watchman's rondavel, the Cais da Barca), the narrow west pass between boulder walls, the meltwater torrent crossed by
    /// two plank bridges, the climb to Hahya's ring of stones at the top (the fight stays the world-map boss challenge)
    /// and the old gallery east to the Grutas do Limiar (19).</item>
    /// </list>
    /// </summary>
    public static partial class MapLayouts
    {
        /// <summary>[distância] of the Serra de Kaggen (Mapas100 13; also over the pass, 14).</summary>
        private const string CrestedHawk = "enemy_mountain_crested_hawk";

        /// <summary>The Serra's own beasts of 13 and 14 (Docs/WorldBible_Maniqueismo.md §3.2.1).</summary>
        private const string Baboon = "enemy_mountain_baboon";
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string IllWindMist = "enemy_mountain_ill_wind_mist";

        // ------------------------------------------------------------------
        // 13 · Trilha dos Rondavéis
        // ------------------------------------------------------------------

        private static MapLayout Rondaveis()
        {
            var m = new MapLayout
            {
                MapId = RondaveisId,
                Biome = BiomeType.Mountain,
                Bounds = Rect.MinMaxRect(-100f, -160f, 100f, 40f),
                SettlementGroundHalfExtent = 46f,
                SettlementIsReturnCity = true,
            };

            // Step 2: the Vila de Litema itself (same recipe as the city walk), its backdrop replaced by the slopes; the
            // pens, the rock-art shelters and the baobab of the trail are placed with it (the town kit's own colliders).
            CityTownRecipe vila = CityTownRecipes.Litema();
            vila.Backdrop = false;
            Pen(vila, new Vector2(-62f, -18f), 6f, 4f, 3f);
            Pen(vila, new Vector2(56f, -24f), 6f, 4f, 3f);
            vila.PlazaProps.Add(Piece(CityPropKind.HayBale, -64f, -16f, 20f));
            vila.PlazaProps.Add(Piece(CityPropKind.HayBale, 58f, -22f, 70f));
            vila.PlazaProps.Add(Piece(CityPropKind.RockArt, -35f, -75f, CityGeometry.YawFacing(Vector2.right)));
            vila.PlazaProps.Add(Piece(CityPropKind.RockArt, 14f, -124f, CityGeometry.YawFacing(Vector2.up)));
            vila.PlazaProps.Add(Piece(CityPropKind.Tree, -18f, -56f, 40f, 1.6f, 3));
            vila.PlazaProps.Add(Piece(CityPropKind.Stela, 10f, -86f, CityGeometry.YawFacing(Vector2.left), 0.8f));
            m.Settlement = vila;
            m.SafeZone = Rect.MinMaxRect(-42f, -40f, 42f, 32f);

            // Step 1: the west road (to 08), the zig-zag trail down from the south gate, the branch to the island map.
            m.Paths.Add(new MapPath(new Vector2(-100f, 0f), new Vector2(-36f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -36f), new Vector2(28f, -56f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(28f, -56f), new Vector2(-26f, -82f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-26f, -82f), new Vector2(30f, -108f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(30f, -108f), new Vector2(100f, -100f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-26f, -82f), new Vector2(-50f, -160f), 3.5f));

            m.Exits.Add(Border("exit_artifices", MapExitKind.Map, ArtificesId, Rect.MinMaxRect(-100f, -6f, -97f, 6f), new Vector2(-93f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_hahya", MapExitKind.Map, HahyaId, Rect.MinMaxRect(97f, -106f, 100f, -94f), new Vector2(93f, -100f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-56f, -160f, -44f, -157f), new Vector2(-50f, -153f), Vector2.up));

            // Step 4–5 (Mapas100 13): Ushabti de Pedra [tanque]; the Pedra que Rola [controle] among the loose stones; the
            // Babuíno Apagado [rápido] in the grass and the Javali charging down it, the Lobo do Tugai on the pass's edge; at night the Ave-Graveto de Omöl and the
            // Coruja-de-Fumaça visiting from the Zarafshan; the Gavião-de-Penacho [distância] throws feathers from the ledges.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "encosta_oeste", Center = new Vector2(-70f, -52f), Radius = 11f, MinLevel = 9, MaxLevel = 10, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(CrestedHawk, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 3), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "capinzal", Center = new Vector2(64f, -62f), Radius = 12f, MinLevel = 9, MaxLevel = 10, MaxAlive = 4, RespawnSeconds = 22f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Baboon, 3), PeriodSpawnEntry.Normal(Boar, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Baboon, 3), PeriodSpawnEntry.Variant(Baboon, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Baboon, 3), PeriodSpawnEntry.Variant(Boar, 1), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "pedras_soltas", Center = new Vector2(-66f, -126f), Radius = 11f, MinLevel = 10, MaxLevel = 11, MaxAlive = 4, RespawnSeconds = 28f,
                Archetypes = "[tanque] [controle]",
                Day = new[] { PeriodSpawnEntry.Normal(RollingStone, 3), PeriodSpawnEntry.Normal(Golem, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(RollingStone, 3), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_do_passo", Center = new Vector2(66f, -136f), Radius = 11f, MinLevel = 10, MaxLevel = 11, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[rápido] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(CrestedHawk, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(CrestedHawk, 1), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
            });
            ClearingsFromZones(m);

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-21f, -86f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(-46f, 4f), 0f));

            // Step 3: golden grass slopes: twisted cedars, tree aloes, the odd baobab, aloes in flower, boulders.
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 8.5f, new[] { 0, 2, 1, 0, 3 }, 0.3f, "rondaveis_serra");
            Rocks(m, 40, "rondaveis_pedras");
            Backdrop(m, new[] { 0, 2, 1 }, "rondaveis_fundo");
            KeepOffSettlement(m);
            return m;
        }

        // ------------------------------------------------------------------
        // 14 · Passo do Hahya
        // ------------------------------------------------------------------

        private static MapLayout Hahya()
        {
            var m = new MapLayout
            {
                MapId = HahyaId,
                Biome = BiomeType.Mountain,
                Bounds = Rect.MinMaxRect(-100f, -100f, 100f, 100f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Boca da Mina do Mihr: the mine forge (stone, thatch gable, chimney), the watchman's and the
            // miners' rondavels, the Cais da Barca; boulders round the mine mouth behind the forge.
            var posto = new CityTownRecipe
            {
                CityId = "boca_da_mina",
                Region = BiomeType.Mountain,
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
                Id = "forja_da_mina", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down,
                Width = 7f, Depth = 5f, Porch = 1.4f,
                Spec = new TownBuildingSpec
                {
                    WallColor = TownSwatch.Stone, TrimColor = TownSwatch.LimeWhite, RoofColor = TownSwatch.Thatch, JoineryColor = TownSwatch.WoodDark,
                    AccentColor = TownSwatch.ClothIndigo, SignColor = TownSwatch.ClothYellow, Roof = TownRoofKind.Gable, Dressing = TownShopDressing.Forja,
                    WallHeight = 3f, Chimney = true,
                },
            });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_da_mina", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe { Id = "casa_do_vigia", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f });
            posto.Lots.Add(new CityLotRecipe { Id = "casa_dos_mineiros", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 9f, Width = 5f, Depth = 4.4f, Porch = 1f });
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, -3f, -6f, 20f));
            posto.PlazaProps.Add(Piece(CityPropKind.Jugs, 3.2f, -5.6f, 40f));
            posto.PlazaProps.Add(Piece(CityPropKind.Crates, -8.5f, -7.5f, 65f, 1.2f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            posto.PlazaProps.Add(Piece(CityPropKind.Rock, -6f, 16f, 20f, 2.6f));
            posto.PlazaProps.Add(Piece(CityPropKind.Rock, 6f, 16.5f, 80f, 2.8f));
            posto.PlazaProps.Add(Piece(CityPropKind.Rock, 0f, 19.5f, 140f, 3.2f));
            // The old gallery to the Limiar at the east edge, and a rock-art shelter on the climb.
            posto.PlazaProps.Add(Piece(CityPropKind.Rock, 93f, -13f, 30f, 3f));
            posto.PlazaProps.Add(Piece(CityPropKind.Rock, 90f, -40f, 110f, 3.2f));
            posto.PlazaProps.Add(Piece(CityPropKind.RockArt, 18f, 52f, CityGeometry.YawFacing(Vector2.right)));

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: the meltwater torrent from the north-west down to the south-east, two plank bridges.
            var torrent = new MapRiver { Width = 7f };
            torrent.Points.AddRange(new[]
            {
                new Vector2(-100f, 62f), new Vector2(-40f, 44f), new Vector2(20f, 34f), new Vector2(60f, 12f), new Vector2(78f, -30f), new Vector2(100f, -62f),
            });
            torrent.Crossings.Add(new MapCrossing { At = new Vector2(40f, 23f), Width = 4f, IsBridge = true });
            torrent.Crossings.Add(new MapCrossing { At = new Vector2(69f, -9f), Width = 4f, IsBridge = true });
            m.Rivers.Add(torrent);

            m.Paths.Add(new MapPath(new Vector2(-100f, 0f), new Vector2(-12f, 0f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(34f, 12f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(34f, 12f), new Vector2(46f, 34f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(46f, 34f), new Vector2(30f, 64f), 3f));
            m.Paths.Add(new MapPath(new Vector2(30f, 64f), new Vector2(34f, 76f), 3f));
            m.Paths.Add(new MapPath(new Vector2(12f, 0f), new Vector2(60f, -13f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(60f, -13f), new Vector2(78f, -5f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(78f, -5f), new Vector2(100f, -28f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(0f, -14f), new Vector2(0f, -100f), 3.5f));

            m.Exits.Add(Border("exit_rondaveis", MapExitKind.Map, RondaveisId, Rect.MinMaxRect(-100f, -6f, -97f, 6f), new Vector2(-93f, 0f), Vector2.right));
            m.Exits.Add(Border("exit_limiar", MapExitKind.Map, LagoLimiarId, Rect.MinMaxRect(97f, -34f, 100f, -22f), new Vector2(93f, -28f), Vector2.left));
            m.Exits.Add(Border("exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-6f, -100f, 6f, -97f), new Vector2(0f, -93f), Vector2.up));

            // Step 4–5 (Mapas100 14): Ushabti de Pedra [tanque]; the Lobo stands in for the Lince Apagado [rápido], the
            // bat swarm is the Morcego da Mina [enxame], the Neblina do Vento Ruim [controle] fills the gorge; at night the Ave-Graveto, the Coruja-de-Fumaça and Ubır at the
            // gallery; the Gavião-de-Penacho [distância] of the 13 also hunts the slope and the summit.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "ladeira_sul", Center = new Vector2(-52f, -55f), Radius = 11f, MinLevel = 10, MaxLevel = 11, MaxAlive = 4, RespawnSeconds = 26f,
                Archetypes = "[tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 3), PeriodSpawnEntry.Normal(CrestedHawk, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 3), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "ponte_de_baixo", Center = new Vector2(48f, -60f), Radius = 11f, MinLevel = 11, MaxLevel = 12, MaxAlive = 5, RespawnSeconds = 22f,
                Archetypes = "[rápido] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Bats, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Bats, 2), PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "garganta", Center = new Vector2(-55f, 78f), Radius = 11f, MinLevel = 11, MaxLevel = 12, MaxAlive = 4, RespawnSeconds = 28f,
                Archetypes = "[tanque] [controle]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 2), PeriodSpawnEntry.Normal(IllWindMist, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 2), PeriodSpawnEntry.Variant(IllWindMist, 1), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_do_cume", Center = new Vector2(76f, 62f), Radius = 11f, MinLevel = 12, MaxLevel = 13, MaxAlive = 3, RespawnSeconds = 32f,
                Archetypes = "[elite] [tanque] [distância]",
                Day = new[] { PeriodSpawnEntry.Normal(Golem, 2), PeriodSpawnEntry.Normal(CrestedHawk, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Golem, 3), PeriodSpawnEntry.Variant(CrestedHawk, 1), PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1) },
            });
            ClearingsFromZones(m);

            // Hahya's ring of stones at the top of the climb (the boss clearing).
            m.Clearings.Add(new Vector2(34f, 84f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.StoneRing, new Vector2(34f, 84f), 0f, 1.1f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-6f, -8f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(18f, -5.5f), 20f));

            // Step 3: the pass: boulder walls along the west road and the north climb, cedars and tree aloes by the
            // torrent, scattered rocks, a belt of big rocks beyond the walls.
            RockLine(m, new Vector2(-90f, 7.5f), new Vector2(-20f, 7.5f), 3.2f, 1.4f, 2.4f, "hahya_passo_n");
            RockLine(m, new Vector2(-90f, -7.5f), new Vector2(-20f, -7.5f), 3.2f, 1.4f, 2.4f, "hahya_passo_s");
            RockLine(m, new Vector2(51.3f, 36.8f), new Vector2(35.3f, 66.8f), 3f, 1.3f, 2.2f, "hahya_subida_l");
            RockLine(m, new Vector2(40.7f, 31.2f), new Vector2(24.7f, 61.2f), 3f, 1.3f, 2.2f, "hahya_subida_o");
            Riverside(m, torrent, "hahya_margem");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 2, 0, 1 }, 0.26f, "hahya_mata");
            Rocks(m, 46, "hahya_pedras");
            RockBackdrop(m, "hahya_fundo");
            KeepOffSettlement(m);
            return m;
        }
    }
}
