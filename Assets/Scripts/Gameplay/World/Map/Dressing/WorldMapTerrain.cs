using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>One region node as the terrain sees it: biome, ground position (x/z) and click radius.</summary>
    public readonly struct WorldMapSite
    {
        public readonly BiomeType Region;
        public readonly Vector2 Position;
        public readonly float ClickRadius;

        public WorldMapSite(BiomeType region, Vector2 position, float clickRadius = WorldMapTerrain.DefaultClickRadius)
        {
            Region = region;
            Position = position;
            ClickRadius = Mathf.Max(0f, clickRadius);
        }
    }

    /// <summary>A road between two connected sites (indices into the site list). The token walks it in a straight line.</summary>
    public readonly struct WorldMapPath
    {
        public readonly int From;
        public readonly int To;
        public readonly Vector2 A;
        public readonly Vector2 B;

        public WorldMapPath(int from, int to, Vector2 a, Vector2 b)
        {
            From = from;
            To = to;
            A = a;
            B = b;
        }
    }

    /// <summary>
    /// Pure, deterministic description of the world-map island built around the region nodes:
    /// per-region blend weights (soft Voronoi around the nodes), land/sea mask, a river, the
    /// straight roads between connected nodes (the same segments <see cref="WorldMapController"/>
    /// moves the token along), ground height and ground colour. No Unity objects are touched, so
    /// <see cref="WorldMapDressingLayout"/> and the EditMode tests use it directly; the runtime
    /// <see cref="WorldMapDressing"/> samples it into a mesh and a colour texture once per load.
    /// </summary>
    public sealed class WorldMapTerrain
    {
        public const float DefaultClickRadius = 1f;
        public const int RegionCount = 4;

        // Shape (world units).
        public const float PathHalfWidth = 0.8f;
        public const float NodePlazaRadius = 1.5f;
        public const float IslandRadius = 9.5f;
        public const float RiverHalfWidth = 0.75f;
        public const float BlendWidth = 1.6f;
        /// <summary>Extra coast radius north of the nodes (away from the camera), which fills the top of the screen with land.</summary>
        public const float NorthReach = 5f;

        /// <summary>Height of walkable ground (nodes, roads). Slightly below 0 so the token's feet and the "current" disc sit on top.</summary>
        public const float GroundLevel = -0.02f;
        public const float SeaLevel = -0.35f;

        private readonly WorldMapSite[] _sites;
        private readonly WorldMapPath[] _paths;
        private readonly Vector2 _noiseOffset;
        private readonly Vector2 _noiseOffsetB;
        private readonly bool _hasRiver;
        private readonly Vector2 _riverOrigin;
        private readonly Vector2 _riverAlong;  // river flows along this axis
        private readonly Vector2 _riverAcross; // the crossed road's direction
        private readonly float _riverPhase;
        private readonly Rect _landBounds;
        private readonly float _sitesMaxY;

        public WorldMapTerrain(IReadOnlyList<WorldMapSite> sites, IReadOnlyList<WorldMapPath> paths, int seed, bool river = true)
        {
            _sites = new WorldMapSite[sites?.Count ?? 0];
            for (int i = 0; i < _sites.Length; i++)
            {
                _sites[i] = sites[i];
            }

            _paths = new WorldMapPath[paths?.Count ?? 0];
            for (int i = 0; i < _paths.Length; i++)
            {
                _paths[i] = paths[i];
            }

            Seed = seed;
            var random = new System.Random(seed);
            _noiseOffset = new Vector2((float)random.NextDouble() * 500f + 50f, (float)random.NextDouble() * 500f + 50f);
            _noiseOffsetB = new Vector2((float)random.NextDouble() * 500f + 600f, (float)random.NextDouble() * 500f + 600f);
            _riverPhase = (float)random.NextDouble() * Mathf.PI * 2f;

            // The river crosses the first road at its midpoint (Floresta -> Deserto on the default
            // map): a natural border between the first two regions, crossed by a bridge.
            if (river && _paths.Length > 0 && (_paths[0].B - _paths[0].A).sqrMagnitude > 16f)
            {
                _hasRiver = true;
                _riverOrigin = (_paths[0].A + _paths[0].B) * 0.5f;
                _riverAcross = (_paths[0].B - _paths[0].A).normalized;
                _riverAlong = new Vector2(-_riverAcross.y, _riverAcross.x);
            }

            _sitesMaxY = float.NegativeInfinity;
            for (int i = 0; i < _sites.Length; i++)
            {
                _sitesMaxY = Mathf.Max(_sitesMaxY, _sites[i].Position.y);
            }

            _landBounds = ComputeLandBounds();
        }

        public int Seed { get; }
        public IReadOnlyList<WorldMapSite> Sites => _sites;
        public IReadOnlyList<WorldMapPath> Paths => _paths;
        public bool HasRiver => _hasRiver;

        /// <summary>Where the river crosses the first road (bridge centre) and the road direction there.</summary>
        public Vector2 BridgeCenter => _riverOrigin;
        public Vector2 BridgeDirection => _riverAcross;

        /// <summary>Axis-aligned bounds of everything that can be land (sites + roads + island radius).</summary>
        public Rect LandBounds => _landBounds;

        // ---------------------------------------------------------------
        // Distances
        // ---------------------------------------------------------------

        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            float t = lengthSq > 1e-6f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        /// <summary>Distance to the nearest road centre line (infinity without roads).</summary>
        public float DistanceToPath(Vector2 p)
        {
            float best = float.PositiveInfinity;
            for (int i = 0; i < _paths.Length; i++)
            {
                best = Mathf.Min(best, DistanceToSegment(p, _paths[i].A, _paths[i].B));
            }

            return best;
        }

        /// <summary>Distance to the nearest site; <paramref name="index"/> is -1 without sites.</summary>
        public float DistanceToNearestSite(Vector2 p, out int index)
        {
            index = -1;
            float best = float.PositiveInfinity;
            for (int i = 0; i < _sites.Length; i++)
            {
                float d = Vector2.Distance(p, _sites[i].Position);
                if (d < best)
                {
                    best = d;
                    index = i;
                }
            }

            return best;
        }

        /// <summary>Approximate distance to the river centre line (infinity without a river).</summary>
        public float DistanceToRiver(Vector2 p)
        {
            if (!_hasRiver)
            {
                return float.PositiveInfinity;
            }

            Vector2 local = p - _riverOrigin;
            float s = Vector2.Dot(local, _riverAlong);
            float u = Vector2.Dot(local, _riverAcross);
            const float amplitude = 1.3f;
            const float frequency = 0.33f;
            // Meander that is 0 (and straight) where it meets the road, so the bridge is square to it.
            float meander = amplitude * Mathf.Sin(s * frequency) * Mathf.Clamp01(Mathf.Abs(s) / 3f);
            meander += 0.6f * Mathf.Sin(s * 0.17f + _riverPhase) * Mathf.Clamp01(Mathf.Abs(s) / 5f);
            float slope = amplitude * frequency;
            return Mathf.Abs(u - meander) / Mathf.Sqrt(1f + slope * slope * 0.5f);
        }

        // ---------------------------------------------------------------
        // Regions
        // ---------------------------------------------------------------

        /// <summary>Soft Voronoi weights per <see cref="BiomeType"/> (index = enum value), summing to 1.</summary>
        public void RegionWeights(Vector2 p, float[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 0f;
            }

            if (_sites.Length == 0)
            {
                weights[0] = 1f;
                return;
            }

            // Wobble the borders a little so regions don't meet on ruler-straight lines.
            Vector2 q = p + new Vector2(Noise(p * 0.18f, _noiseOffsetB) - 0.5f, Noise(p * 0.18f, _noiseOffset) - 0.5f) * 3f;
            float nearest = DistanceToNearestSite(q, out _);
            float total = 0f;
            for (int i = 0; i < _sites.Length; i++)
            {
                float d = Vector2.Distance(q, _sites[i].Position);
                float w = Mathf.Exp(-(d - nearest) / BlendWidth);
                int region = (int)_sites[i].Region;
                if (region >= 0 && region < weights.Length)
                {
                    weights[region] += w;
                    total += w;
                }
            }

            if (total <= 0f)
            {
                weights[0] = 1f;
                return;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] /= total;
            }
        }

        private readonly float[] _scratchWeights = new float[RegionCount];

        /// <summary>The region whose ground covers <paramref name="p"/>.</summary>
        public BiomeType DominantRegion(Vector2 p)
        {
            RegionWeights(p, _scratchWeights);
            int best = 0;
            for (int i = 1; i < _scratchWeights.Length; i++)
            {
                if (_scratchWeights[i] > _scratchWeights[best])
                {
                    best = i;
                }
            }

            return (BiomeType)best;
        }

        // ---------------------------------------------------------------
        // Land / height
        // ---------------------------------------------------------------

        /// <summary>1 well inland, 0 at sea; the coastline wobbles with noise around the roads/nodes.</summary>
        public float LandMask(Vector2 p)
        {
            float d = _paths.Length > 0 ? DistanceToPath(p) : DistanceToNearestSite(p, out _);
            if (float.IsInfinity(d))
            {
                return 0f;
            }

            float radius = IslandRadius + (Noise(p * 0.09f, _noiseOffset) - 0.5f) * 6f + (Noise(p * 0.3f, _noiseOffsetB) - 0.5f) * 1.2f;
            if (_sites.Length > 0)
            {
                radius += Mathf.Clamp(p.y - _sitesMaxY, 0f, NorthReach * 2f) * 0.5f;
            }

            return 1f - Smooth(radius - 2f, radius, d);
        }

        /// <summary>
        /// 1 on roads, node plazas and the bridge (walkable, forced flat at <see cref="GroundLevel"/>),
        /// fading to 0 a little way off them.
        /// </summary>
        public float FlatMask(Vector2 p)
        {
            float path = 1f - Smooth(PathHalfWidth, PathHalfWidth + 2.2f, DistanceToPath(p));
            float node = 1f - Smooth(NodePlazaRadius, NodePlazaRadius + 2.4f, DistanceToNearestSite(p, out _));
            return Mathf.Max(path, node);
        }

        /// <summary>Ground height (world y). Water (sea, river) is flat at <see cref="SeaLevel"/>.</summary>
        public float Height(Vector2 p) => Mathf.Max(RawHeight(p, out _), SeaLevel);

        /// <summary>How far below <see cref="SeaLevel"/> the bed lies (0 = dry land).</summary>
        public float WaterDepth(Vector2 p)
        {
            float raw = RawHeight(p, out _);
            return Mathf.Max(0f, SeaLevel - raw);
        }

        /// <summary>True where a prop may stand: dry, inland and off the river.</summary>
        public bool IsDryLand(Vector2 p, float margin = 0f)
        {
            if (LandMask(p) < 0.95f)
            {
                return false;
            }

            return DistanceToRiver(p) > RiverHalfWidth + 0.6f + margin;
        }

        /// <summary>Unclamped height; also leaves <paramref name="p"/>'s region weights in the scratch buffer when on land.</summary>
        private float RawHeight(Vector2 p, out float relief)
        {
            relief = 0f;
            float landMask = LandMask(p);
            if (landMask <= 0f)
            {
                return SeaLevel - 1.2f; // open sea: skip the (costly) relief
            }

            RegionWeights(p, _scratchWeights);
            relief += _scratchWeights[(int)BiomeType.Forest] * ForestRelief(p);
            relief += _scratchWeights[(int)BiomeType.Desert] * DesertRelief(p);
            relief += _scratchWeights[(int)BiomeType.Mountain] * MountainRelief(p);
            relief += _scratchWeights[(int)BiomeType.Caves] * CavesRelief(p);

            float flat = FlatMask(p);
            float land = GroundLevel + relief * (1f - flat);

            if (_hasRiver)
            {
                // The river is carved under the road too: the token walks at a fixed height, and
                // the bridge (WorldMapDressing) spans the gap there.
                float river = 1f - Smooth(RiverHalfWidth, RiverHalfWidth + 0.7f, DistanceToRiver(p));
                land = Mathf.Lerp(land, SeaLevel - 0.25f, river);
            }

            return Mathf.Lerp(SeaLevel - 1.2f, land, landMask);
        }

        private float ForestRelief(Vector2 p) => 0.25f * Fbm(p * 0.3f, _noiseOffset);

        private float DesertRelief(Vector2 p)
        {
            float bend = Noise(p * 0.12f, _noiseOffsetB) * 3f;
            float wave = 0.5f + 0.5f * Mathf.Sin(p.x * 0.55f + p.y * 0.95f + bend);
            return 0.05f + 0.5f * wave * wave;
        }

        private float MountainRelief(Vector2 p)
        {
            float ridge = Fbm(p * 0.2f, _noiseOffsetB);
            float rise = Smooth(1.5f, 7f, DistanceToNearestSite(p, out _));
            return (0.3f + 3.6f * ridge * ridge) * (0.35f + 0.65f * rise);
        }

        private float CavesRelief(Vector2 p) => 0.15f + 0.45f * Fbm(p * 0.45f, _noiseOffset);

        // ---------------------------------------------------------------
        // Colour
        // ---------------------------------------------------------------

        public static readonly Color ForestGround = new Color(0.40f, 0.66f, 0.27f);
        public static readonly Color ForestGroundDark = new Color(0.27f, 0.52f, 0.22f);
        public static readonly Color DesertGround = new Color(0.94f, 0.81f, 0.53f);
        public static readonly Color DesertGroundDark = new Color(0.86f, 0.69f, 0.42f);
        public static readonly Color MountainGround = new Color(0.55f, 0.6f, 0.47f);
        public static readonly Color MountainRock = new Color(0.56f, 0.54f, 0.53f);
        public static readonly Color Snow = new Color(0.95f, 0.96f, 0.99f);
        public static readonly Color CavesGround = new Color(0.33f, 0.29f, 0.36f);
        public static readonly Color CavesGroundDark = new Color(0.22f, 0.2f, 0.26f);
        public static readonly Color Road = new Color(0.72f, 0.57f, 0.37f);
        public static readonly Color RoadEdge = new Color(0.56f, 0.43f, 0.28f);
        public static readonly Color Beach = new Color(0.96f, 0.88f, 0.64f);
        public static readonly Color Foam = new Color(0.86f, 0.95f, 0.97f);
        public static readonly Color ShallowWater = new Color(0.33f, 0.76f, 0.82f);
        public static readonly Color DeepWater = new Color(0.16f, 0.42f, 0.66f);

        /// <summary>Ground albedo at <paramref name="p"/> (biomes blended, roads, beach, water).</summary>
        public Color GroundColor(Vector2 p)
        {
            float raw = RawHeight(p, out float relief);
            float depth = SeaLevel - raw;
            if (depth > 0f)
            {
                Color water = Color.Lerp(ShallowWater, DeepWater, Smooth(0.05f, 0.75f, depth));
                return Color.Lerp(Foam, water, Smooth(0f, 0.06f, depth));
            }

            // RawHeight left this point's region weights in _scratchWeights.
            float n = Noise(p * 0.7f, _noiseOffsetB);
            float n2 = Noise(p * 2.3f, _noiseOffset);

            Color forest = Color.Lerp(ForestGround, ForestGroundDark, Smooth(0.35f, 0.75f, n) * 0.8f + (n2 - 0.5f) * 0.2f);

            float bend = Noise(p * 0.12f, _noiseOffsetB) * 3f;
            float dune = 0.5f + 0.5f * Mathf.Sin(p.x * 0.55f + p.y * 0.95f + bend);
            Color desert = Color.Lerp(DesertGround, DesertGroundDark, Smooth(0.25f, 0.05f, dune) * 0.8f + (n2 - 0.5f) * 0.15f);

            float height = raw - GroundLevel;
            Color mountain = Color.Lerp(MountainGround, MountainRock, Smooth(0.35f, 0.9f, height) + (n2 - 0.5f) * 0.2f);
            mountain = Color.Lerp(mountain, Snow, Smooth(1.55f, 1.85f, height + (n - 0.5f) * 0.4f));

            Color caves = Color.Lerp(CavesGround, CavesGroundDark, Smooth(0.3f, 0.7f, n) + (n2 - 0.5f) * 0.25f);

            Color ground =
                forest * _scratchWeights[(int)BiomeType.Forest] +
                desert * _scratchWeights[(int)BiomeType.Desert] +
                mountain * _scratchWeights[(int)BiomeType.Mountain] +
                caves * _scratchWeights[(int)BiomeType.Caves];

            // Beach along the coast (but not on high ground).
            float coast = 1f - Smooth(0.15f, 0.55f, raw - SeaLevel);
            ground = Color.Lerp(ground, Beach, coast);

            // Riverbank: a thin sandy rim.
            if (_hasRiver)
            {
                float bank = 1f - Smooth(RiverHalfWidth + 0.5f, RiverHalfWidth + 0.9f, DistanceToRiver(p));
                ground = Color.Lerp(ground, Beach, bank * 0.6f);
            }

            // Roads and node plazas: dirt with a darker rim.
            float roadDistance = Mathf.Min(DistanceToPath(p), DistanceToNearestSite(p, out _) - (NodePlazaRadius - PathHalfWidth));
            float edge = 1f - Smooth(PathHalfWidth - 0.05f, PathHalfWidth + 0.2f, roadDistance);
            float core = 1f - Smooth(PathHalfWidth - 0.35f, PathHalfWidth - 0.15f, roadDistance);
            ground = Color.Lerp(ground, RoadEdge, edge);
            ground = Color.Lerp(ground, Color.Lerp(Road, RoadEdge, (n2 - 0.5f) * 0.4f), core);

            ground.a = 1f;
            return ground;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private Rect ComputeLandBounds()
        {
            if (_sites.Length == 0)
            {
                return new Rect(-IslandRadius, -IslandRadius, IslandRadius * 2f, IslandRadius * 2f);
            }

            Vector2 min = _sites[0].Position;
            Vector2 max = min;
            for (int i = 1; i < _sites.Length; i++)
            {
                min = Vector2.Min(min, _sites[i].Position);
                max = Vector2.Max(max, _sites[i].Position);
            }

            // Island radius plus the coastline noise amplitude.
            float pad = IslandRadius + 4f;
            return Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad + NorthReach);
        }

        /// <summary>Smoothstep from <paramref name="edge0"/> to <paramref name="edge1"/> (either order).</summary>
        public static float Smooth(float edge0, float edge1, float x)
        {
            if (Mathf.Approximately(edge0, edge1))
            {
                return x < edge0 ? 0f : 1f;
            }

            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static float Noise(Vector2 p, Vector2 offset) => Mathf.Clamp01(Mathf.PerlinNoise(p.x + offset.x, p.y + offset.y));

        private static float Fbm(Vector2 p, Vector2 offset)
        {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            for (int octave = 0; octave < 3; octave++)
            {
                sum += Noise(p, offset) * amplitude;
                total += amplitude;
                p *= 2.03f;
                amplitude *= 0.5f;
            }

            return sum / total;
        }
    }
}
