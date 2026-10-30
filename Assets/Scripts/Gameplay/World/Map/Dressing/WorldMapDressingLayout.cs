using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>Scattered world-map prop kinds. Every region owns its own set (<see cref="WorldMapDressingLayout.KindsFor"/>).</summary>
    public enum WorldMapPropKind
    {
        // Floresta
        Tree,
        GrassClump,
        Bush,

        // Deserto
        /// <summary>The desert shrub: saxaul, every third one a tamarisk (id kept from the old cactus).</summary>
        Cactus,
        DesertRock,

        // Montanha
        Pine,
        Rock,
        SnowRock,
        Peak,

        // Cavernas
        Stalagmite,
        Crystal,
        CaveRock,
    }

    /// <summary>One scattered prop: kind, owning region, world position (y = ground height), yaw (degrees) and scale.</summary>
    public readonly struct WorldMapPropPlacement
    {
        public readonly WorldMapPropKind Kind;
        public readonly BiomeType Region;
        public readonly Vector3 Position;
        public readonly float Yaw;
        public readonly float Scale;

        public WorldMapPropPlacement(WorldMapPropKind kind, BiomeType region, Vector3 position, float yaw, float scale)
        {
            Kind = kind;
            Region = region;
            Position = position;
            Yaw = yaw;
            Scale = scale;
        }

        public Vector2 Ground => new Vector2(Position.x, Position.z);
        public float Radius => WorldMapDressingLayout.RadiusOf(Kind) * Scale;
    }

    /// <summary>A cloud bank floating over the sea around the island (border dressing).</summary>
    public readonly struct WorldMapCloudPlacement
    {
        public readonly Vector3 Position;
        public readonly float Scale;
        public readonly int Seed;

        public WorldMapCloudPlacement(Vector3 position, float scale, int seed)
        {
            Position = position;
            Scale = scale;
            Seed = seed;
        }
    }

    /// <summary>
    /// Deterministic (seeded) placement of the world-map dressing on a <see cref="WorldMapTerrain"/>:
    /// per-region props, the landmark spot next to each node and the cloud banks around the island.
    /// Rules: props only on dry land of their own region, never on a road (or its shoulder), never
    /// inside a node's click radius plus a margin (more for tall props, which also keep out of the
    /// strip between the camera and each node so the hero and labels stay readable), never on a
    /// landmark, and never overlapping each other. Pure math; EditMode-tested.
    /// </summary>
    public static class WorldMapDressingLayout
    {
        /// <summary>Clearance between a prop's footprint and the road edge.</summary>
        public const float PathClearance = 0.35f;
        /// <summary>Minimum distance from a node to any prop's footprint (beyond its click radius).</summary>
        public const float NodeClearance = 1.2f;
        /// <summary>Minimum distance from a node to a tall prop's footprint.</summary>
        public const float TallNodeClearRadius = 3.4f;
        /// <summary>Tall props keep out of this half-width strip in front of (camera side, -z) each node.</summary>
        public const float FrontStripHalfWidth = 2.6f;
        public const float FrontStripDepth = 6f;

        public const float LandmarkDistance = 2.6f;
        public const float LandmarkRadius = 1.1f;

        private const int AttemptsPerProp = 40;

        private static readonly WorldMapPropKind[] ForestKinds = { WorldMapPropKind.Tree, WorldMapPropKind.GrassClump, WorldMapPropKind.Bush };
        private static readonly WorldMapPropKind[] DesertKinds = { WorldMapPropKind.Cactus, WorldMapPropKind.DesertRock };
        // Peaks first: they need the most room.
        private static readonly WorldMapPropKind[] MountainKinds = { WorldMapPropKind.Peak, WorldMapPropKind.Pine, WorldMapPropKind.Rock, WorldMapPropKind.SnowRock };
        private static readonly WorldMapPropKind[] CavesKinds = { WorldMapPropKind.Stalagmite, WorldMapPropKind.Crystal, WorldMapPropKind.CaveRock };

        private static readonly int[] ForestCounts = { 26, 18, 10 };
        private static readonly int[] DesertCounts = { 12, 10 };
        private static readonly int[] MountainCounts = { 5, 9, 9, 8 };
        private static readonly int[] CavesCounts = { 16, 8, 8 };

        // Landmark direction candidates (degrees from +x, +90 = away from the camera). Diagonals
        // first: straight behind the node the hero model (~3.6 tall) would hide the landmark.
        private static readonly float[] LandmarkAngles = { 55f, 125f, 65f, 115f, 90f, 20f, 160f, -25f, 205f };

        /// <summary>The prop kinds a region is dressed with (disjoint between regions).</summary>
        public static IReadOnlyList<WorldMapPropKind> KindsFor(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return DesertKinds;
                case BiomeType.Mountain: return MountainKinds;
                case BiomeType.Caves: return CavesKinds;
                default: return ForestKinds;
            }
        }

        private static int[] CountsFor(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return DesertCounts;
                case BiomeType.Mountain: return MountainCounts;
                case BiomeType.Caves: return CavesCounts;
                default: return ForestCounts;
            }
        }

        /// <summary>Footprint radius at scale 1.</summary>
        public static float RadiusOf(WorldMapPropKind kind)
        {
            switch (kind)
            {
                case WorldMapPropKind.Tree: return 0.85f;
                case WorldMapPropKind.Pine: return 0.65f;
                case WorldMapPropKind.GrassClump: return 0.4f;
                case WorldMapPropKind.Bush: return 0.5f;
                case WorldMapPropKind.Cactus: return 0.5f;
                case WorldMapPropKind.DesertRock: return 0.6f;
                case WorldMapPropKind.Rock: return 0.6f;
                case WorldMapPropKind.SnowRock: return 0.8f;
                case WorldMapPropKind.Peak: return 2.3f;
                case WorldMapPropKind.Stalagmite: return 0.5f;
                case WorldMapPropKind.Crystal: return 0.4f;
                case WorldMapPropKind.CaveRock: return 0.7f;
                default: return 0.5f;
            }
        }

        /// <summary>Props tall enough to hide a node, the hero or a label when close to/in front of a node.</summary>
        public static bool IsTall(WorldMapPropKind kind)
        {
            return kind == WorldMapPropKind.Tree || kind == WorldMapPropKind.Pine || kind == WorldMapPropKind.Peak ||
                   kind == WorldMapPropKind.Cactus || kind == WorldMapPropKind.Stalagmite;
        }

        private static Vector2 ScaleRange(WorldMapPropKind kind)
        {
            switch (kind)
            {
                case WorldMapPropKind.Peak: return new Vector2(0.85f, 1.25f);
                case WorldMapPropKind.Tree:
                case WorldMapPropKind.Pine: return new Vector2(0.8f, 1.2f);
                default: return new Vector2(0.75f, 1.25f);
            }
        }

        /// <summary>Distance a prop footprint must keep from site <paramref name="site"/>'s centre.</summary>
        public static float NodeClearRadius(WorldMapSite site, bool tall)
        {
            float clear = site.ClickRadius + NodeClearance;
            return tall ? Mathf.Max(clear, TallNodeClearRadius) : clear;
        }

        /// <summary>Hashes the layout seed with a region and entry index (stable across runs and platforms).</summary>
        public static int SeedFor(int seed, BiomeType region, int entry)
        {
            unchecked
            {
                int h = seed * 486187739;
                h = (h ^ ((int)region + 1) * 7919) * 16777619;
                h = (h ^ (entry + 1) * 104729) * 16777619;
                return h;
            }
        }

        // ---------------------------------------------------------------
        // Landmarks
        // ---------------------------------------------------------------

        /// <summary>
        /// Ground spot (x/z) next to site <paramref name="siteIndex"/> for its landmark (hut, ruin,
        /// peak, cave mouth): <see cref="LandmarkDistance"/> away, preferring the back diagonals,
        /// clear of every road and of the other nodes.
        /// </summary>
        public static Vector2 LandmarkPosition(WorldMapTerrain terrain, int siteIndex)
        {
            Vector2 origin = terrain.Sites[siteIndex].Position;
            Vector2 best = origin + Direction(LandmarkAngles[0]) * LandmarkDistance;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < LandmarkAngles.Length; i++)
            {
                Vector2 candidate = origin + Direction(LandmarkAngles[i]) * LandmarkDistance;
                float clearance = terrain.DistanceToPath(candidate) - WorldMapTerrain.PathHalfWidth - LandmarkRadius;
                clearance = Mathf.Min(clearance, terrain.DistanceToRiver(candidate) - WorldMapTerrain.RiverHalfWidth - LandmarkRadius);
                for (int s = 0; s < terrain.Sites.Count; s++)
                {
                    if (s != siteIndex)
                    {
                        clearance = Mathf.Min(clearance, Vector2.Distance(candidate, terrain.Sites[s].Position) - NodeClearRadius(terrain.Sites[s], true) - LandmarkRadius);
                    }
                }

                if (clearance >= 0.15f)
                {
                    return candidate;
                }

                if (clearance > bestScore)
                {
                    bestScore = clearance;
                    best = candidate;
                }
            }

            return best;
        }

        public static void LandmarkPositions(WorldMapTerrain terrain, List<Vector2> results)
        {
            results.Clear();
            for (int i = 0; i < terrain.Sites.Count; i++)
            {
                results.Add(LandmarkPosition(terrain, i));
            }
        }

        private static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        // ---------------------------------------------------------------
        // Props
        // ---------------------------------------------------------------

        /// <summary>Scatters every region's props. Same terrain + seed = same list, in the same order.</summary>
        public static List<WorldMapPropPlacement> BuildProps(WorldMapTerrain terrain, int seed, float density = 1f)
        {
            var results = new List<WorldMapPropPlacement>(160);
            if (terrain == null || terrain.Sites.Count == 0)
            {
                return results;
            }

            var landmarks = new List<Vector2>(terrain.Sites.Count);
            LandmarkPositions(terrain, landmarks);
            Rect bounds = terrain.LandBounds;

            var regionsDone = new HashSet<BiomeType>();
            for (int s = 0; s < terrain.Sites.Count; s++)
            {
                BiomeType region = terrain.Sites[s].Region;
                if (!regionsDone.Add(region))
                {
                    continue;
                }

                IReadOnlyList<WorldMapPropKind> kinds = KindsFor(region);
                int[] counts = CountsFor(region);
                for (int entry = 0; entry < kinds.Count; entry++)
                {
                    WorldMapPropKind kind = kinds[entry];
                    int target = Mathf.RoundToInt(counts[entry] * Mathf.Max(0f, density));
                    var random = new System.Random(SeedFor(seed, region, entry));
                    Vector2 scaleRange = ScaleRange(kind);
                    int placed = 0;

                    for (int attempt = 0; attempt < target * AttemptsPerProp && placed < target; attempt++)
                    {
                        var p = new Vector2(
                            Mathf.Lerp(bounds.xMin, bounds.xMax, (float)random.NextDouble()),
                            Mathf.Lerp(bounds.yMin, bounds.yMax, (float)random.NextDouble()));
                        float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)random.NextDouble());
                        float yaw = (float)random.NextDouble() * 360f;

                        if (!CanPlace(terrain, landmarks, results, region, kind, p, RadiusOf(kind) * scale))
                        {
                            continue;
                        }

                        results.Add(new WorldMapPropPlacement(kind, region, new Vector3(p.x, terrain.Height(p), p.y), yaw, scale));
                        placed++;
                    }
                }
            }

            return results;
        }

        /// <summary>Every placement rule in one place (also what the tests assert).</summary>
        public static bool CanPlace(WorldMapTerrain terrain, IReadOnlyList<Vector2> landmarks, IReadOnlyList<WorldMapPropPlacement> placed,
            BiomeType region, WorldMapPropKind kind, Vector2 p, float radius)
        {
            if (!terrain.IsDryLand(p, radius * 0.5f) || terrain.DominantRegion(p) != region)
            {
                return false;
            }

            if (terrain.DistanceToPath(p) < WorldMapTerrain.PathHalfWidth + PathClearance + radius)
            {
                return false;
            }

            bool tall = IsTall(kind);
            int nearest = kind == WorldMapPropKind.Peak ? NearestSite(terrain, p) : -1;
            for (int i = 0; i < terrain.Sites.Count; i++)
            {
                WorldMapSite site = terrain.Sites[i];
                Vector2 delta = p - site.Position;
                if (delta.magnitude < NodeClearRadius(site, tall) + radius)
                {
                    return false;
                }

                // In front of the node (between it and the camera, which looks toward +z).
                if (tall && delta.y < 0f && -delta.y < FrontStripDepth && Mathf.Abs(delta.x) < FrontStripHalfWidth + radius)
                {
                    return false;
                }

                // Peaks only rise behind their node.
                if (i == nearest && delta.y < 4f)
                {
                    return false;
                }
            }

            for (int i = 0; i < landmarks.Count; i++)
            {
                if (Vector2.Distance(p, landmarks[i]) < LandmarkRadius + radius + 0.2f)
                {
                    return false;
                }
            }

            if (terrain.HasRiver && Vector2.Distance(p, terrain.BridgeCenter) < 2.5f + radius)
            {
                return false;
            }

            for (int i = 0; i < placed.Count; i++)
            {
                WorldMapPropPlacement other = placed[i];
                float spacing = (radius + other.Radius) * 0.9f;
                Vector2 delta = p - other.Ground;
                if (delta.sqrMagnitude < spacing * spacing)
                {
                    return false;
                }
            }

            return true;
        }

        private static int NearestSite(WorldMapTerrain terrain, Vector2 p)
        {
            terrain.DistanceToNearestSite(p, out int index);
            return index;
        }

        // ---------------------------------------------------------------
        // Border clouds
        // ---------------------------------------------------------------

        /// <summary>
        /// Cloud banks on an ellipse just past the coast, thicker on the far side (top of the screen)
        /// and the flanks, and left out of the near arc so the bottom of the screen (HUD) stays clear.
        /// </summary>
        public static List<WorldMapCloudPlacement> BuildClouds(WorldMapTerrain terrain, int seed)
        {
            var results = new List<WorldMapCloudPlacement>(32);
            if (terrain == null || terrain.Sites.Count == 0)
            {
                return results;
            }

            Rect bounds = terrain.LandBounds;
            Vector2 center = bounds.center;
            var random = new System.Random(unchecked(seed * 31 + 977));

            AddRing(results, random, center, bounds.width * 0.5f + 3f, bounds.height * 0.5f + 3f, 22, 1f, 2.6f);
            AddRing(results, random, center, bounds.width * 0.5f + 9f, bounds.height * 0.5f + 9f, 14, 1.5f, 3.2f);
            return results;
        }

        private static void AddRing(List<WorldMapCloudPlacement> results, System.Random random, Vector2 center, float radiusX, float radiusZ,
            int count, float scale, float height)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (i + (float)random.NextDouble() * 0.6f) / count * 360f;
                float jitter = (float)random.NextDouble();
                float size = scale * Mathf.Lerp(0.8f, 1.35f, (float)random.NextDouble());
                int cloudSeed = random.Next();

                // Skip the near arc (camera side): it would sit right under the HUD and cover the shore.
                if (angle > 235f && angle < 305f)
                {
                    continue;
                }

                Vector2 p = center +new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) * radiusX, Mathf.Sin(angle * Mathf.Deg2Rad) * radiusZ) * Mathf.Lerp(0.98f, 1.08f, jitter);
                results.Add(new WorldMapCloudPlacement(new Vector3(p.x, height + jitter * 0.8f, p.y), size, cloudSeed));
            }
        }
    }
}
