using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// The playable maps of this build, code-built through the 6-step pipeline (TODO.md "Pipeline dos 100 Mapas"):
    /// <list type="number">
    /// <item><b>01 · Pomar da Vó Edna</b> (P, nv 1–3): the whole Vila da Vó Edna (<see cref="CityTownRecipes.VilaEdna"/>,
    /// the very same town of the city walk) at the centre-north, the orchard around it with three training clearings; exits
    /// east to 02, west back to the island map, south to the Estrada das Caravanas (07, not open yet).</item>
    /// <item><b>02 · Tugai do Rio Dourado</b> (M, nv 2–5): the outpost Moinho do Pomar by the west entrance, the S-shaped
    /// golden river with two fords and a log bridge, four hunting clearings and the boss clearing to the north (Kök-Böri's
    /// ring of stones; the fight itself stays the world-map boss challenge); a trail south to the Brejo do Lodo Escuro (03).</item>
    /// </list>
    /// Deterministic (props by <see cref="TownStyle.Hash01"/>, never a runtime RNG) and cached for the session.
    /// </summary>
    public static partial class MapLayouts
    {
        private static readonly Dictionary<string, MapLayout> Cache = new Dictionary<string, MapLayout>();

        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Boar = NightVariantRules.PeccaryEnemyId;

        /// <summary>The layout of <paramref name="mapId"/>, or null when the map has no built layout.</summary>
        public static MapLayout For(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                return null;
            }

            if (Cache.TryGetValue(mapId, out MapLayout cached))
            {
                return cached;
            }

            MapLayout layout;
            switch (mapId)
            {
                case MapCatalog.PomarId: layout = Pomar(); break;
                case MapCatalog.TugaiId: layout = Tugai(); break;
                default:
                    layout = BuildRegional(mapId);
                    if (layout == null)
                    {
                        return null;
                    }

                    break;
            }

            Finish(layout);
            Cache[mapId] = layout;
            return layout;
        }

        public static bool Has(string mapId) => mapId == MapCatalog.PomarId || mapId == MapCatalog.TugaiId || HasRegional(mapId);

        public static void ClearCache() => Cache.Clear();

        // ------------------------------------------------------------------
        // 01 · Pomar da Vó Edna
        // ------------------------------------------------------------------

        private static MapLayout Pomar()
        {
            var m = new MapLayout
            {
                MapId = MapCatalog.PomarId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-58f, -92f, 62f, 28f),
                SettlementGroundHalfExtent = 46f,
                SettlementIsReturnCity = true,
            };

            // Step 2: the Vila itself (same recipe as the city walk); its own backdrop forest is replaced by the orchard.
            CityTownRecipe vila = CityTownRecipes.VilaEdna();
            vila.Backdrop = false;
            m.Settlement = vila;
            m.SafeZone = Rect.MinMaxRect(-40f, -40f, 42f, 28f);

            // Trails: the south road to the Caravan Road, the orchard lane to the Tugai, the west road to the island map.
            m.Paths.Add(new MapPath(new Vector2(0f, -40f), new Vector2(0f, -92f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -58f), new Vector2(34f, -40f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(34f, -40f), new Vector2(62f, -20f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(-40f, 0f), new Vector2(-58f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(0f, -66f), new Vector2(-22f, -64f), 3f));
            m.Paths.Add(new MapPath(new Vector2(0f, -76f), new Vector2(20f, -74f), 3f));

            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_tugai", Kind = MapExitKind.Map, TargetMapId = MapCatalog.TugaiId,
                Trigger = Rect.MinMaxRect(59f, -27f, 62f, -13f), Arrival = new Vector2(55f, -21.5f), ArrivalFacing = Vector2.left,
            });
            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_island", Kind = MapExitKind.WorldMap,
                Trigger = Rect.MinMaxRect(-58f, -6f, -55f, 6f), Arrival = new Vector2(-51f, 0f), ArrivalFacing = Vector2.right,
            });
            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_caravanas", Kind = MapExitKind.Map, TargetMapId = "map_007_caravanas",
                Trigger = Rect.MinMaxRect(-6f, -92f, 6f, -89f), Arrival = new Vector2(0f, -85f), ArrivalFacing = Vector2.up,
            });

            // Step 4–5: three training clearings (Mapas100 01: Lobo do Tugai filhote [rápido], Aranha-Tecelã pequena
            // [enxame]); at night the Coruja-de-Fumaça comes to the orchard and only watches below level 3.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "clareira_lobos", Center = new Vector2(-28f, -64f), Radius = 9f, MinLevel = 1, MaxLevel = 2, MaxAlive = 3, RespawnSeconds = 22f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Variant(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Variant(Wolf, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
                PassiveSpeciesId = NightVariantRules.SpeciesRasgaMortalha, PassiveBelowHeroLevel = 3,
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "clareira_aranhas", Center = new Vector2(24f, -76f), Radius = 9f, MinLevel = 1, MaxLevel = 3, MaxAlive = 4, RespawnSeconds = 20f,
                Archetypes = "[enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Variant(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Normal(Spider, 1), PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
                PassiveSpeciesId = NightVariantRules.SpeciesRasgaMortalha, PassiveBelowHeroLevel = 3,
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "clareira_leste", Center = new Vector2(46f, -62f), Radius = 8f, MinLevel = 2, MaxLevel = 3, MaxAlive = 3, RespawnSeconds = 25f,
                Archetypes = "[rápido] [enxame]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 2), PeriodSpawnEntry.Normal(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 2), PeriodSpawnEntry.Variant(Spider, 2), PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1) },
                PassiveSpeciesId = NightVariantRules.SpeciesRasgaMortalha, PassiveBelowHeroLevel = 3,
            });
            foreach (MapSpawnZone zone in m.Zones)
            {
                m.Clearings.Add(zone.Center);
            }

            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(3f, -55f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.MelonBed, new Vector2(-14f, -50f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.MelonBed, new Vector2(14f, -52f), 90f));

            // Step 3: the orchard (apricot / pomegranate rows south of the Vila), poplars and tamarisks on the edges.
            OrchardRows(m, Rect.MinMaxRect(-54f, -88f, 58f, -44f), 6.5f, 5.5f, "pomar");
            Edge(m, Rect.MinMaxRect(-56f, -44f, -42f, 26f), 5.5f, new[] { 0, 1, 2 }, 0.75f, "pomar_oeste");
            Edge(m, Rect.MinMaxRect(43f, -44f, 60f, 26f), 5.5f, new[] { 0, 1, 2, 3 }, 0.7f, "pomar_leste");
            Rocks(m, 10, "pomar_pedras");
            Backdrop(m, new[] { 0, 1, 2 }, "pomar_fundo");
            return m;
        }

        // ------------------------------------------------------------------
        // 02 · Tugai do Rio Dourado
        // ------------------------------------------------------------------

        private static MapLayout Tugai()
        {
            var m = new MapLayout
            {
                MapId = MapCatalog.TugaiId,
                Biome = BiomeType.Forest,
                Bounds = Rect.MinMaxRect(-22f, -100f, 178f, 100f),
                SettlementGroundHalfExtent = 18f,
            };

            // Step 2: the Moinho do Pomar outpost: the lamp of Ramrat, the mill house, the potion stall and the Cais da Barca.
            var posto = new CityTownRecipe
            {
                CityId = "moinho_do_pomar",
                Region = BiomeType.Forest,
                PlazaRadius = 4.5f,
                StreetWidth = 3.5f,
                SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f,
                EntryArm = CityArm.West,
                AutoYards = false,
                Backdrop = false,
                NightLights = 3,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = "lampada_do_posto", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down,
                Width = 4.5f, Depth = 6f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.Plaster, TrimColor = TownSwatch.Turquoise, RoofColor = TownSwatch.Turquoise, JoineryColor = TownSwatch.Walnut, AccentColor = TownSwatch.Gold, WallHeight = 3.4f },
            });
            posto.Lots.Add(new CityLotRecipe { Id = "casa_do_moinho", Kind = CityLotKind.Workshop, Arm = CityArm.East, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 4.6f, Porch = 1.4f });
            posto.Lots.Add(new CityLotRecipe { Id = "cais_do_moinho", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe { Id = "casa_da_pocao", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f });
            posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.Stall, Position = new Vector2(-2.4f, -3.2f), Yaw = 180f, Variant = 1 });
            posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.Well, Position = new Vector2(2.6f, 2.4f), Scale = 0.8f });
            posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.HayBale, Position = new Vector2(14f, -9.5f), Yaw = 15f });
            posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.Crates, Position = new Vector2(-9.5f, -8.5f), Yaw = 30f });
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            m.Settlement = posto;
            m.SafeZone = Rect.MinMaxRect(-15f, -14f, 16f, 15f);

            // Step 1: the golden river, an S from the north-west down to the east, two fords and a log bridge.
            var river = new MapRiver { Width = 9f };
            river.Points.AddRange(new[]
            {
                new Vector2(-22f, 44f), new Vector2(10f, 52f), new Vector2(40f, 48f), new Vector2(66f, 30f), new Vector2(84f, 8f),
                new Vector2(104f, -14f), new Vector2(128f, -30f), new Vector2(152f, -36f), new Vector2(178f, -30f),
            });
            river.Crossings.Add(new MapCrossing { At = new Vector2(40f, 48f), Width = 6f });
            river.Crossings.Add(new MapCrossing { At = new Vector2(94f, -3f), Width = 4.5f, IsBridge = true });
            river.Crossings.Add(new MapCrossing { At = new Vector2(152f, -36f), Width = 6f });
            m.Rivers.Add(river);

            m.Paths.Add(new MapPath(new Vector2(-22f, 0f), new Vector2(-12f, 0f), 4f));
            m.Paths.Add(new MapPath(new Vector2(13f, 0f), new Vector2(48f, -36f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(48f, -36f), new Vector2(94f, -3f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(94f, -3f), new Vector2(128f, 40f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(128f, 40f), new Vector2(118f, 76f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(48f, -36f), new Vector2(108f, -70f), 3f));
            m.Paths.Add(new MapPath(new Vector2(108f, -70f), new Vector2(152f, -36f), 3f));
            m.Paths.Add(new MapPath(new Vector2(152f, -36f), new Vector2(178f, -44f), 3.5f));
            m.Paths.Add(new MapPath(new Vector2(13f, 6f), new Vector2(40f, 48f), 3f));
            m.Paths.Add(new MapPath(new Vector2(40f, 48f), new Vector2(60f, 80f), 3f));
            m.Paths.Add(new MapPath(new Vector2(78f, -53f), new Vector2(78f, -100f), 3f));

            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_brejo", Kind = MapExitKind.Map, TargetMapId = BrejoId,
                Trigger = Rect.MinMaxRect(72f, -100f, 84f, -97f), Arrival = new Vector2(78f, -93f), ArrivalFacing = Vector2.up,
            });
            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_pomar", Kind = MapExitKind.Map, TargetMapId = MapCatalog.PomarId,
                Trigger = Rect.MinMaxRect(-22f, -7f, -19f, 7f), Arrival = new Vector2(-16.5f, 0f), ArrivalFacing = Vector2.right,
            });
            m.Exits.Add(new MapExitSpec
            {
                Id = "exit_island", Kind = MapExitKind.WorldMap,
                Trigger = Rect.MinMaxRect(175f, -52f, 178f, -36f), Arrival = new Vector2(171f, -44f), ArrivalFacing = Vector2.left,
            });

            // Step 4–5 (Mapas100 02): Lobo do Tugai [rápido], Aranha-Tecelã Apagada [enxame], Javali [tanque]; at night the
            // Mardagayl and the Javali do Lodo herd.
            m.Zones.Add(new MapSpawnZone
            {
                Id = "campina_lobos", Center = new Vector2(50f, -48f), Radius = 11f, MinLevel = 2, MaxLevel = 3, MaxAlive = 4, RespawnSeconds = 25f,
                Archetypes = "[rápido]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 4), PeriodSpawnEntry.Normal(Spider, 1) },
                Dusk = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Variant(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 4), PeriodSpawnEntry.Normal(Wolf, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "tamargueiral", Center = new Vector2(112f, -78f), Radius = 11f, MinLevel = 3, MaxLevel = 4, MaxAlive = 5, RespawnSeconds = 20f,
                Archetypes = "[enxame] [controle]",
                Day = new[] { PeriodSpawnEntry.Normal(Spider, 1) },
                Dawn = new[] { PeriodSpawnEntry.Normal(Spider, 3), PeriodSpawnEntry.Variant(Spider, 1) },
                Night = new[] { PeriodSpawnEntry.Variant(Spider, 3), PeriodSpawnEntry.Normal(Spider, 1), PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "mata_norte", Center = new Vector2(62f, 84f), Radius = 12f, MinLevel = 3, MaxLevel = 5, MaxAlive = 3, RespawnSeconds = 30f,
                Archetypes = "[tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Boar, 3), PeriodSpawnEntry.Normal(Wolf, 1) },
                Night = new[] { PeriodSpawnEntry.Species(NightVariantRules.Queixada, 2), PeriodSpawnEntry.Variant(Wolf, 2) },
            });
            m.Zones.Add(new MapSpawnZone
            {
                Id = "beira_do_circulo", Center = new Vector2(140f, 34f), Radius = 11f, MinLevel = 4, MaxLevel = 5, MaxAlive = 4, RespawnSeconds = 28f,
                Archetypes = "[rápido] [tanque]",
                Day = new[] { PeriodSpawnEntry.Normal(Wolf, 3), PeriodSpawnEntry.Normal(Boar, 2) },
                Night = new[] { PeriodSpawnEntry.Variant(Wolf, 3), PeriodSpawnEntry.Variant(Boar, 1), PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1) },
            });
            foreach (MapSpawnZone zone in m.Zones)
            {
                m.Clearings.Add(zone.Center);
            }

            m.Clearings.Add(new Vector2(118f, 82f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.StoneRing, new Vector2(118f, 82f), 0f, 1.2f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.WaterMill, new Vector2(-6f, 12.5f), 0f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Campfire, new Vector2(-4.5f, -8f), 0f, 0.8f));
            m.Landmarks.Add(new MapLandmark(MapLandmarkKind.Signpost, new Vector2(52f, -31f), 30f));

            // Step 3: the tugai: gold and green poplars along the river, tamarisks, fallen trunks (rocks), the edges.
            Riverside(m, river, "tugai_margem");
            Edge(m, CityGeometry.Inflate(m.Bounds, -2f), 9f, new[] { 0, 1, 2, 0, 1 }, 0.55f, "tugai_mata");
            Rocks(m, 26, "tugai_pedras");
            Backdrop(m, new[] { 0, 1, 1, 2 }, "tugai_fundo");
            return m;
        }

        // ------------------------------------------------------------------
        // Finishing: the town fitted to the map
        // ------------------------------------------------------------------

        /// <summary>Generates the settlement and fits it to the map: walls on the map edges, its own safe zone and ground, the dock arrival.</summary>
        private static void Finish(MapLayout m)
        {
            CityLayout town = CityLayoutGenerator.Generate(m.Settlement);
            town.WalkBounds = m.Bounds;
            town.SafeZone = m.SafeZone;
            town.GroundHalfExtent = m.SettlementGroundHalfExtent;
            town.Invalidate();
            m.Town = town;

            CityExit dock = town.Exits.Find(e => e.Kind == CityExitKind.ShipDock);
            if (dock != null)
            {
                // Out on the street in front of the pier (lot.Front faces the street), turned towards it; the dock trigger
                // itself only arms once the hero has stepped out of it.
                CityLot lot = town.Lots.Find(l => l.Kind == CityLotKind.ShipDock);
                Vector2 front = lot != null && lot.Front.sqrMagnitude > 0.01f ? lot.Front.normalized : Vector2.down;
                m.DockArrival = dock.Target + front * 2.6f;
                m.DockArrivalFacing = -front;
                m.Exits.Add(new MapExitSpec
                {
                    Id = "cais_da_barca", Kind = MapExitKind.Dock, Trigger = dock.Trigger, Arrival = m.DockArrival, ArrivalFacing = m.DockArrivalFacing,
                });
            }
            else
            {
                m.DockArrival = town.Respawn;
            }
        }

        // ------------------------------------------------------------------
        // Deterministic scatter
        // ------------------------------------------------------------------

        private static void OrchardRows(MapLayout m, Rect area, float dx, float dz, string salt)
        {
            int i = 0;
            for (float z = area.yMin; z <= area.yMax; z += dz)
            {
                for (float x = area.xMin; x <= area.xMax; x += dx, i++)
                {
                    var p = new Vector2(x + (TownStyle.Hash01(salt, i) - 0.5f) * 0.9f, z + (TownStyle.Hash01(salt, i + 7000) - 0.5f) * 0.9f);
                    if (TownStyle.Hash01(salt, i + 3000) < 0.12f || !MapLayoutRules.IsFreeForProp(m, p, 0.9f))
                    {
                        continue;
                    }

                    m.WildProps.Add(new CityProp
                    {
                        Kind = TownStyle.Hash01(salt, i + 5000) < 0.08f ? CityPropKind.Bush : CityPropKind.Tree,
                        Position = p,
                        Yaw = TownStyle.Hash01(salt, i + 9000) * 360f,
                        Scale = 0.85f + TownStyle.Hash01(salt, i + 11000) * 0.35f,
                        Variant = 3,
                    });
                }
            }
        }

        private static void Edge(MapLayout m, Rect area, float spacing, int[] variants, float density, string salt)
        {
            int i = 0;
            for (float z = area.yMin; z <= area.yMax; z += spacing)
            {
                for (float x = area.xMin; x <= area.xMax; x += spacing, i++)
                {
                    if (TownStyle.Hash01(salt, i) > density)
                    {
                        continue;
                    }

                    var p = new Vector2(x + (TownStyle.Hash01(salt, i + 100) - 0.5f) * spacing * 0.8f, z + (TownStyle.Hash01(salt, i + 200) - 0.5f) * spacing * 0.8f);
                    if (!MapLayoutRules.IsFreeForProp(m, p, 1.2f))
                    {
                        continue;
                    }

                    bool bush = TownStyle.Hash01(salt, i + 300) < 0.22f;
                    m.WildProps.Add(new CityProp
                    {
                        Kind = bush ? CityPropKind.Bush : CityPropKind.Tree,
                        Position = p,
                        Yaw = TownStyle.Hash01(salt, i + 400) * 360f,
                        Scale = bush ? 1f + TownStyle.Hash01(salt, i + 500) * 0.5f : 1f + TownStyle.Hash01(salt, i + 500) * 0.6f,
                        Variant = variants[(int)(TownStyle.Hash(salt, i + 600) % (uint)variants.Length)],
                    });
                }
            }
        }

        private static void Riverside(MapLayout m, MapRiver river, string salt)
        {
            int i = 0;
            for (int s = 0; s + 1 < river.Points.Count; s++)
            {
                Vector2 a = river.Points[s];
                Vector2 b = river.Points[s + 1];
                float length = Vector2.Distance(a, b);
                Vector2 d = (b - a) / Mathf.Max(0.01f, length);
                var n = new Vector2(-d.y, d.x);
                for (float t = 2f; t < length; t += 6f, i++)
                {
                    float side = (i & 1) == 0 ? 1f : -1f;
                    float off = river.Width * 0.5f + 2.2f + TownStyle.Hash01(salt, i) * 3f;
                    Vector2 p = a + d * t + n * side * off;
                    if (!MapLayoutRules.IsFreeForProp(m, p, 1f))
                    {
                        continue;
                    }

                    m.WildProps.Add(new CityProp
                    {
                        Kind = CityPropKind.Tree,
                        Position = p,
                        Yaw = TownStyle.Hash01(salt, i + 100) * 360f,
                        Scale = 1.1f + TownStyle.Hash01(salt, i + 200) * 0.5f,
                        Variant = TownStyle.Hash01(salt, i + 300) < 0.6f ? 1 : 2,
                    });
                }
            }
        }

        private static void Rocks(MapLayout m, int count, string salt)
        {
            Rect r = CityGeometry.Inflate(m.Bounds, -4f);
            for (int i = 0, placed = 0; i < count * 6 && placed < count; i++)
            {
                var p = new Vector2(Mathf.Lerp(r.xMin, r.xMax, TownStyle.Hash01(salt, i)), Mathf.Lerp(r.yMin, r.yMax, TownStyle.Hash01(salt, i + 500)));
                if (!MapLayoutRules.IsFreeForProp(m, p, 1.4f))
                {
                    continue;
                }

                placed++;
                m.WildProps.Add(new CityProp
                {
                    Kind = CityPropKind.Rock,
                    Position = p,
                    Yaw = TownStyle.Hash01(salt, i + 900) * 360f,
                    Scale = 0.8f + TownStyle.Hash01(salt, i + 1200) * 1.1f,
                });
            }
        }

        /// <summary>A belt of trees just outside the walls (seen from the isometric camera), with gaps at the exits.</summary>
        private static void Backdrop(MapLayout m, int[] variants, string salt)
        {
            Rect b = m.Bounds;
            int i = 0;
            for (float ring = 2.5f; ring <= 9f; ring += 5f)
            {
                Rect r = CityGeometry.Inflate(b, ring);
                float perimeter = 2f * (r.width + r.height);
                for (float s = 0f; s < perimeter; s += 5.5f, i++)
                {
                    Vector2 p = PerimeterPoint(r, s + TownStyle.Hash01(salt, i) * 2f);
                    if (!MapLayoutRules.IsFreeForProp(m, p, 0.5f, backdrop: true))
                    {
                        continue;
                    }

                    m.WildProps.Add(new CityProp
                    {
                        Kind = CityPropKind.Tree,
                        Position = p,
                        Yaw = TownStyle.Hash01(salt, i + 100) * 360f,
                        Scale = 1.2f + TownStyle.Hash01(salt, i + 200) * 0.6f,
                        Variant = variants[(int)(TownStyle.Hash(salt, i + 300) % (uint)variants.Length)],
                    });
                }
            }
        }

        private static Vector2 PerimeterPoint(Rect r, float s)
        {
            float w = r.width;
            float h = r.height;
            s %= 2f * (w + h);
            if (s < w) return new Vector2(r.xMin + s, r.yMin);
            s -= w;
            if (s < h) return new Vector2(r.xMax, r.yMin + s);
            s -= h;
            if (s < w) return new Vector2(r.xMax - s, r.yMax);
            s -= w;
            return new Vector2(r.xMin, r.yMax - s);
        }
    }
}
