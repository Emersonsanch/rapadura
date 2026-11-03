using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The continuous regions of this build (Docs/Mapas100.md §0.10) and the helpers that build them: catalog maps placed
    /// as zones of one big world (their hand-made layouts embedded at an offset: settlement, trails, rivers, landmarks,
    /// spawn clearings, props), the in-between areas filled with generated clearings, the trails split for the chunk
    /// streaming, the settlements generated and fitted (walls on the region's edge, safe zones, docks). Cached per session.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string ZarafshanId = "overworld_zarafshan";

        /// <summary>Longest trail piece (m) after <see cref="SplitPaths"/>.</summary>
        public const float PathPiece = 24f;

        private static readonly Dictionary<string, OverworldLayout> Cache = new Dictionary<string, OverworldLayout>();

        /// <summary>The continuous region of each catalog region (one line per region; its builder in <see cref="Build"/>).</summary>
        private static readonly Dictionary<MapRegion, string> RegionIds = new Dictionary<MapRegion, string>
        {
            { MapRegion.Zarafshan, ZarafshanId },
            { MapRegion.AreiasFlamejantes, AreiasId },
            { MapRegion.SerraDeKaggen, KaggenId },
            { MapRegion.GrutasDoLimiar, LimiarId },
            { MapRegion.CostaDasMoncoes, CostaId },
            { MapRegion.DeltaDeJhilpur, DeltaId },
            { MapRegion.TetoDoMundo, TetoId },
        };

        /// <summary>Every continuous region of this build (texts, tests).</summary>
        public static IEnumerable<string> RegionLayoutIds => RegionIds.Values;

        /// <summary>The continuous region holding catalog map <paramref name="mapId"/>, or null when the map is its own scene.</summary>
        public static OverworldLayout ForMap(string mapId)
        {
            MapDefinition map = MapCatalog.Get(mapId);
            if (map == null || !RegionIds.TryGetValue(map.Region, out string id))
            {
                return null;
            }

            OverworldLayout layout = Get(id);
            return layout != null && layout.ContainsMap(mapId) ? layout : null;
        }

        public static bool IsContinuous(string mapId) => ForMap(mapId) != null;

        private static OverworldLayout Build(string id)
        {
            switch (id)
            {
                case ZarafshanId: return Zarafshan();
                case AreiasId: return Areias();
                case KaggenId: return Kaggen();
                case LimiarId: return Limiar();
                case CostaId: return Costa();
                case DeltaId: return DeltaRegion();
                case TetoId: return TetoRegion();
                case WorldId: return BuildWorld();
                default: return null;
            }
        }

        public static OverworldLayout Get(string id)
        {
            if (Cache.TryGetValue(id, out OverworldLayout cached))
            {
                return cached;
            }

            OverworldLayout layout = Build(id);
            if (layout != null)
            {
                Cache[id] = layout;
            }

            return layout;
        }

        public static void ClearCache() => Cache.Clear();

        // ------------------------------------------------------------------
        // Building blocks
        // ------------------------------------------------------------------

        private static OverworldLayout NewRegion(string id, MapRegion region, Rect bounds)
        {
            var layout = new OverworldLayout { Id = id, Region = region, Bounds = bounds };
            layout.World = new MapLayout { MapId = id, Biome = BiomeType.Forest, Bounds = bounds };
            return layout;
        }

        private static OverworldZone Zone(OverworldLayout w, string id, string mapId, string owner, float xMin, float zMin, float xMax, float zMax,
            OverworldTerrain terrain, int min, int max, int walk, string en, string pt, string regionType = MapRegionTypes.Forest, string requirement = "",
            float spacing = 100f)
        {
            var zone = new OverworldZone
            {
                Id = id, MapId = mapId, OwnerMapId = owner ?? mapId, Bounds = Rect.MinMaxRect(xMin, zMin, xMax, zMax), Terrain = terrain,
                MinLevel = min, MaxLevel = max, WalkLevel = walk, NameEn = en, NamePt = pt, RegionType = regionType,
                RequirementTag = requirement ?? string.Empty, ClearingSpacing = spacing,
            };
            w.Zones.Add(zone);
            return zone;
        }

        private static OverworldFauna Fauna(string id, string archetypes, PeriodSpawnEntry[] day, PeriodSpawnEntry[] night, int maxAlive = 3,
            PeriodSpawnEntry[] dusk = null, float respawn = 25f, string passive = null, int passiveBelow = 0)
        {
            return new OverworldFauna
            {
                Id = id, Archetypes = archetypes, Day = day, Night = night, Dusk = dusk, MaxAlive = maxAlive, RespawnSeconds = respawn,
                PassiveSpeciesId = passive, PassiveBelowHeroLevel = passiveBelow,
            };
        }

        private static PeriodSpawnEntry N(string enemy, int weight) => PeriodSpawnEntry.Normal(enemy, weight);

        private static PeriodSpawnEntry V(string enemy, int weight) => PeriodSpawnEntry.Variant(enemy, weight);

        private static PeriodSpawnEntry S(NightSpecies species, int weight) => PeriodSpawnEntry.Species(species, weight);

        private static PeriodSpawnEntry[] T(params PeriodSpawnEntry[] entries) => entries;

        private static void Path(OverworldLayout w, float width, params float[] xz)
        {
            for (int i = 0; i + 3 < xz.Length; i += 2)
            {
                w.World.Paths.Add(new MapPath(new Vector2(xz[i], xz[i + 1]), new Vector2(xz[i + 2], xz[i + 3]), width));
            }
        }

        private static void PathVia(OverworldLayout w, float width, params Vector2[] points)
        {
            for (int i = 0; i + 1 < points.Length; i++)
            {
                w.World.Paths.Add(new MapPath(points[i], points[i + 1], width));
            }
        }

        private static MapRiver River(OverworldLayout w, float width, params Vector2[] points)
        {
            var river = new MapRiver { Width = width };
            river.Points.AddRange(points);
            w.World.Rivers.Add(river);
            return river;
        }

        private static void Crossing(MapRiver river, Vector2 at, float width, bool bridge)
        {
            river.Crossings.Add(new MapCrossing { At = at, Width = width, IsBridge = bridge });
        }

        /// <summary>A point on segment <paramref name="a"/>–<paramref name="b"/> at <paramref name="t"/> (crossings mid-segment).</summary>
        private static Vector2 On(Vector2 a, Vector2 b, float t) => Vector2.Lerp(a, b, t);

        private static void Landmark(OverworldLayout w, MapLandmarkKind kind, float x, float z, float yaw = 0f, float scale = 1f)
        {
            w.World.Landmarks.Add(new MapLandmark(kind, new Vector2(x, z), yaw, scale));
        }

        private static void Gate(OverworldLayout w, string id, MapExitKind kind, string target, Rect trigger, Vector2 arrival, Vector2 facing)
        {
            w.World.Exits.Add(new MapExitSpec { Id = id, Kind = kind, TargetMapId = target, Trigger = trigger, Arrival = arrival, ArrivalFacing = facing });
        }

        /// <summary>
        /// Embeds a hand-made map layout (plaza at its origin) at <paramref name="offset"/>: its settlement, trails, rivers,
        /// landmarks, spawn clearings (ids prefixed) and, with <paramref name="withProps"/>, its wild props inside its own
        /// bounds (the backdrop belt outside them is dropped). Its border exits are dropped (the region is open); the world
        /// gates are the region's own. Returns the settlement.
        /// </summary>
        private static OverworldSettlement Embed(OverworldLayout w, MapLayout src, Vector2 offset, bool withProps, bool withRivers = true, bool authored = true)
        {
            if (src == null)
            {
                return null;
            }

            var settlement = new OverworldSettlement
            {
                Id = src.Settlement.CityId,
                MapId = src.MapId,
                Offset = offset,
                Recipe = src.Settlement,
                SafeZone = Shift(src.SafeZone, offset),
                GroundHalfExtent = src.SettlementGroundHalfExtent,
                IsReturnCity = src.SettlementIsReturnCity,
            };
            w.Settlements.Add(settlement);
            if (authored)
            {
                w.AuthoredAreas.Add(Shift(src.Bounds, offset));
            }

            foreach (MapPath path in src.Paths)
            {
                w.World.Paths.Add(new MapPath(path.A + offset, path.B + offset, path.Width));
            }

            if (withRivers)
            {
                foreach (MapRiver river in src.Rivers)
                {
                    var copy = new MapRiver { Width = river.Width };
                    foreach (Vector2 p in river.Points)
                    {
                        copy.Points.Add(p + offset);
                    }

                    foreach (MapCrossing crossing in river.Crossings)
                    {
                        copy.Crossings.Add(new MapCrossing { At = crossing.At + offset, Width = crossing.Width, IsBridge = crossing.IsBridge });
                    }

                    w.World.Rivers.Add(copy);
                }
            }

            foreach (MapLandmark landmark in src.Landmarks)
            {
                w.World.Landmarks.Add(new MapLandmark(landmark.Kind, landmark.Position + offset, landmark.Yaw, landmark.Scale));
            }

            string prefix = src.MapId.Length > 8 ? src.MapId.Substring(8) : src.MapId;
            foreach (MapSpawnZone zone in src.Zones)
            {
                Vector2 center = zone.Center + offset;
                OverworldZone owner = OverworldRules.ZoneAt(w, center);
                if (owner == null || owner.MapId != src.MapId)
                {
                    continue;
                }

                var copy = new MapSpawnZone
                {
                    Id = prefix + "_" + zone.Id, Center = center, Radius = zone.Radius, MinLevel = zone.MinLevel, MaxLevel = zone.MaxLevel,
                    MaxAlive = zone.MaxAlive, RespawnSeconds = zone.RespawnSeconds, Archetypes = zone.Archetypes, Day = zone.Day, Dawn = zone.Dawn,
                    Dusk = zone.Dusk, Night = zone.Night, PassiveSpeciesId = zone.PassiveSpeciesId, PassiveBelowHeroLevel = zone.PassiveBelowHeroLevel,
                };
                w.World.Zones.Add(copy);
                w.ZoneOfSpawn[copy.Id] = owner;
            }

            if (withProps)
            {
                foreach (CityProp prop in src.WildProps)
                {
                    if (src.Bounds.Contains(prop.Position))
                    {
                        w.FixedProps.Add(new CityProp
                        {
                            Kind = prop.Kind, Position = prop.Position + offset, End = prop.End + offset, Yaw = prop.Yaw, Scale = prop.Scale, Variant = prop.Variant,
                        });
                    }
                }
            }

            return settlement;
        }

        /// <summary>Where the trail of <paramref name="src"/>'s border exit towards <paramref name="target"/> leaves its bounds (world), else <paramref name="fallback"/>.</summary>
        private static Vector2 Port(MapLayout src, Vector2 offset, string target, MapExitKind kind, Vector2 fallback)
        {
            if (src == null)
            {
                return fallback;
            }

            MapExitSpec exit = src.Exits.Find(e => e.Kind == kind && (kind != MapExitKind.Map || e.TargetMapId == target));
            return exit != null ? exit.Trigger.center + offset : fallback;
        }

        private static Rect Shift(Rect r, Vector2 offset) => new Rect(r.x + offset.x, r.y + offset.y, r.width, r.height);

        /// <summary>A settlement of the region that has no hand-made layout (the Vila is built this way too).</summary>
        private static OverworldSettlement Place(OverworldLayout w, string mapId, CityTownRecipe recipe, Vector2 offset, Rect localSafe, float groundHalf, bool returnCity)
        {
            var settlement = new OverworldSettlement
            {
                Id = recipe.CityId, MapId = mapId, Offset = offset, Recipe = recipe, SafeZone = Shift(localSafe, offset), GroundHalfExtent = groundHalf,
                IsReturnCity = returnCity,
            };
            w.Settlements.Add(settlement);
            return settlement;
        }

        // ------------------------------------------------------------------
        // Generated spawn clearings of a zone
        // ------------------------------------------------------------------

        /// <summary>
        /// Fills <paramref name="zone"/> with spawn clearings on a jittered grid (<see cref="OverworldZone.ClearingSpacing"/>), each
        /// hosting one of the zone's monster groups at a level that rises with the distance from the Vila, kept off trails,
        /// rivers, safe zones, landmarks, gates and the embedded maps (which bring their own clearings).
        /// </summary>
        private static void GenerateClearings(OverworldLayout w, OverworldZone zone)
        {
            if (zone.Fauna.Count == 0)
            {
                return;
            }

            float spacing = zone.ClearingSpacing;
            Rect inner = CityGeometry.Inflate(zone.Bounds, -22f);
            string salt = "owclear_" + zone.Id;
            Vector2 vila = w.Vila != null ? w.Vila.Offset : Vector2.zero;
            float near = CityGeometry.DistanceToRect(vila, zone.Bounds);
            float far = Mathf.Max(
                Mathf.Max(Vector2.Distance(vila, new Vector2(zone.Bounds.xMin, zone.Bounds.yMin)), Vector2.Distance(vila, new Vector2(zone.Bounds.xMax, zone.Bounds.yMin))),
                Mathf.Max(Vector2.Distance(vila, new Vector2(zone.Bounds.xMin, zone.Bounds.yMax)), Vector2.Distance(vila, new Vector2(zone.Bounds.xMax, zone.Bounds.yMax))));
            int index = 0;
            int n = 0;
            for (float x = inner.xMin + spacing * 0.5f; x < inner.xMax; x += spacing)
            {
                for (float z = inner.yMin + spacing * 0.5f; z < inner.yMax; z += spacing, index++)
                {
                    var c = new Vector2(
                        x + (TownStyle.Hash01(salt, index) - 0.5f) * spacing * 0.6f,
                        z + (TownStyle.Hash01(salt, index + 1000) - 0.5f) * spacing * 0.6f);
                    float radius = 10f + TownStyle.Hash01(salt, index + 2000) * 3f;
                    if (!inner.Contains(c) || !ClearingFits(w, c, radius))
                    {
                        continue;
                    }

                    OverworldFauna fauna = zone.Fauna[(int)(TownStyle.Hash(salt, index + 3000) % (uint)zone.Fauna.Count)];
                    float t = Mathf.Clamp01((Vector2.Distance(vila, c) - near) / Mathf.Max(1f, far - near));
                    int span = zone.MaxLevel - zone.MinLevel;
                    int lo = zone.MinLevel + Mathf.FloorToInt(t * span * 0.85f);
                    int hi = Mathf.Min(zone.MaxLevel, lo + Mathf.Max(span > 0 ? 1 : 0, span / 3));
                    var spawn = new MapSpawnZone
                    {
                        Id = zone.Id + "_" + (++n).ToString("00") + "_" + fauna.Id, Center = c, Radius = radius, MinLevel = lo, MaxLevel = hi,
                        MaxAlive = fauna.MaxAlive, RespawnSeconds = fauna.RespawnSeconds, Archetypes = fauna.Archetypes, Day = fauna.Day, Dusk = fauna.Dusk,
                        Night = fauna.Night, PassiveSpeciesId = fauna.PassiveSpeciesId, PassiveBelowHeroLevel = fauna.PassiveBelowHeroLevel,
                    };
                    w.World.Zones.Add(spawn);
                    w.ZoneOfSpawn[spawn.Id] = zone;
                }
            }
        }

        private static bool ClearingFits(OverworldLayout w, Vector2 c, float radius)
        {
            if (OverworldRules.DistanceToSafety(w, c) < radius + OverworldRules.SpawnSafeMargin + 8f)
            {
                return false;
            }

            foreach (Rect authored in w.AuthoredAreas)
            {
                if (CityGeometry.Inflate(authored, radius + 6f).Contains(c))
                {
                    return false;
                }
            }

            foreach (OverworldSettlement settlement in w.Settlements)
            {
                if (CityGeometry.Inflate(settlement.GroundRect, radius + 10f).Contains(c))
                {
                    return false;
                }
            }

            foreach (MapPath path in w.World.Paths)
            {
                if (MapLayoutRules.DistanceToSegment(c, path.A, path.B) < path.Width * 0.5f + radius + 2f)
                {
                    return false;
                }
            }

            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(c, river.Points) < river.Width * 0.5f + radius + 2f)
                {
                    return false;
                }
            }

            foreach (MapLandmark landmark in w.World.Landmarks)
            {
                if (Vector2.Distance(c, landmark.Position) < radius + 12f)
                {
                    return false;
                }
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                if (Vector2.Distance(c, exit.Arrival) < radius + 25f)
                {
                    return false;
                }
            }

            foreach (MapSpawnZone other in w.World.Zones)
            {
                if (Vector2.Distance(c, other.Center) < radius + other.Radius + 30f)
                {
                    return false;
                }
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Finishing
        // ------------------------------------------------------------------

        /// <summary>Splits every trail into pieces of at most <see cref="PathPiece"/> m (chunked drawing and keep-clear tests).</summary>
        private static void SplitPaths(MapLayout world)
        {
            var split = new List<MapPath>();
            foreach (MapPath path in world.Paths)
            {
                float length = Vector2.Distance(path.A, path.B);
                int pieces = Mathf.Max(1, Mathf.CeilToInt(length / PathPiece));
                for (int i = 0; i < pieces; i++)
                {
                    split.Add(new MapPath(Vector2.Lerp(path.A, path.B, i / (float)pieces), Vector2.Lerp(path.A, path.B, (i + 1) / (float)pieces), path.Width));
                }
            }

            world.Paths.Clear();
            world.Paths.AddRange(split);
        }

        /// <summary>Generates every settlement's town fitted to the region (walls on the region's edge), its docks, and the Vila as the world's return city.</summary>
        private static void FinishSettlements(OverworldLayout w)
        {
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                CityLayout town = CityLayoutGenerator.Generate(settlement.Recipe);
                town.WalkBounds = Shift(w.Bounds, -settlement.Offset);
                town.SafeZone = Shift(settlement.SafeZone, -settlement.Offset);
                town.GroundHalfExtent = settlement.GroundHalfExtent;
                town.Invalidate();
                settlement.Town = town;

                CityExit dock = town.Exits.Find(e => e.Kind == CityExitKind.ShipDock);
                if (dock == null)
                {
                    continue;
                }

                CityLot lot = town.Lots.Find(l => l.Kind == CityLotKind.ShipDock);
                Vector2 front = lot != null && lot.Front.sqrMagnitude > 0.01f ? lot.Front.normalized : Vector2.down;
                settlement.HasDock = true;
                settlement.DockTrigger = Shift(dock.Trigger, settlement.Offset);
                settlement.DockArrival = dock.Target + front * 2.6f + settlement.Offset;
                settlement.DockArrivalFacing = -front;
                w.World.Exits.Add(new MapExitSpec
                {
                    Id = "cais_" + settlement.Id, Kind = MapExitKind.Dock, Trigger = settlement.DockTrigger, Arrival = settlement.DockArrival,
                    ArrivalFacing = settlement.DockArrivalFacing,
                });
            }

            OverworldSettlement vila = w.Vila;
            w.World.Settlement = vila.Recipe;
            w.World.Town = vila.Town;
            w.World.SafeZone = vila.SafeZone;
            w.World.SettlementGroundHalfExtent = vila.GroundHalfExtent;
            w.World.SettlementIsReturnCity = true;
            w.World.DockArrival = vila.HasDock ? vila.DockArrival : vila.Town.Respawn + vila.Offset;
            w.World.DockArrivalFacing = vila.DockArrivalFacing;
        }

        /// <summary>Drops the embedded props that now stand on a region trail or river (the region's trails run on past the old maps).</summary>
        private static void CleanFixedProps(OverworldLayout w)
        {
            w.FixedProps.RemoveAll(prop => MapLayoutRules.OnPath(w.World, prop.Position, 0.6f) || MapLayoutRules.InRiver(w.World, prop.Position, 0.8f)
                || OverworldRules.DistanceToSafety(w, prop.Position) < 1.5f);
        }
    }
}
