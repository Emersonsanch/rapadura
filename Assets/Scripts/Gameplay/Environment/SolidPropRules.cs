using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>What a prop is, for collision purposes (see <see cref="SolidPropRules.Classify"/>).</summary>
    public enum SolidPropKind
    {
        /// <summary>Walk-through decor (grass, tufts, pebbles, twigs, flowers) or something that is not a prop at all (NPCs, enemies, the player, UI).</summary>
        None = 0,

        /// <summary>LOD'd tree: only the trunk blocks, never the canopy.</summary>
        Tree = 1,

        /// <summary>Rock / boulder: a hull fitted to the visual mesh.</summary>
        Rock = 2,

        /// <summary>Bench, workbench, stall, cart, house, fence, crate, barrel, table...: an axis-aligned box.</summary>
        Box = 3,

        /// <summary>Cactus, big bush, post, pillar: an upright capsule.</summary>
        Capsule = 4,
    }

    /// <summary>The collider the binder builds for a prop.</summary>
    public enum SolidColliderShape
    {
        None = 0,
        TrunkCapsule = 1,
        ConvexMesh = 2,
        Box = 3,
        Capsule = 4,
    }

    /// <summary>Outcome of the "can the hero squeeze between these two trunks?" check.</summary>
    public enum SolidGapResolution
    {
        /// <summary>The trunks already touch/overlap: nothing to do.</summary>
        AlreadyClosed = 0,

        /// <summary>The gap is wide enough for the hero's capsule (plus skin): nothing to do.</summary>
        Passable = 1,

        /// <summary>Slightly too narrow: both trunks were shrunk a little so the hero fits.</summary>
        Shrunk = 2,

        /// <summary>Clearly too narrow: a thin wall should seal the gap so the hero can't wedge into it.</summary>
        Seal = 3,
    }

    /// <summary>Upright trunk capsule in the prop's local frame (Y up).</summary>
    public struct TrunkEstimate
    {
        public Vector3 Center;
        public float Radius;
        public float Height;
        public float TreeHeight;
    }

    /// <summary>
    /// Pure collision rules for scene and runtime-spawned props — which ones block the hero, with
    /// what collider shape and size, and how close neighbours/roads are handled. No scene access:
    /// <see cref="SolidPropBinder"/> feeds it names, mesh points and sizes, and applies the answer.
    /// EditMode-tested (SolidPropRulesTests). See Docs/Collision.md.
    /// </summary>
    public static class SolidPropRules
    {
        /// <summary>Hero CharacterController radius (Main.unity Player); used for gap checks.</summary>
        public const float HeroRadius = 0.35f;

        /// <summary>Hero CharacterController step offset: anything lower is stepped over anyway.</summary>
        public const float HeroStepOffset = 0.3f;

        /// <summary>Extra room (m) a passage needs beyond the hero's diameter to be walked through without scraping/jitter.</summary>
        public const float GapSkin = 0.15f;

        /// <summary>Thinnest trunk collider (m, world).</summary>
        public const float MinTrunkRadius = 0.12f;

        /// <summary>Thickest trunk collider (m, world) — a wider "trunk" estimate means canopy leaked into the band.</summary>
        public const float MaxTrunkRadius = 0.75f;

        /// <summary>The most a trunk collider may be shrunk (m) to open a tight gap — beyond this the hero would visibly clip the bark.</summary>
        public const float MaxTrunkShrink = 0.06f;

        /// <summary>Padding (m) added to the measured trunk so the hero's body doesn't sink into the bark.</summary>
        public const float TrunkPadding = 0.04f;

        /// <summary>Trunk collider height as a fraction of the tree height (clamped by <see cref="MinTrunkColliderHeight"/>/<see cref="MaxTrunkColliderHeight"/>).</summary>
        public const float TrunkHeightFraction = 0.55f;

        public const float MinTrunkColliderHeight = 2.2f;
        public const float MaxTrunkColliderHeight = 3.5f;

        /// <summary>Vertical band (fractions of the tree height above its lowest point) sampled to measure the trunk — above root flares, below any canopy.</summary>
        public const float TrunkBandLow = 0.03f;
        public const float TrunkBandHigh = 0.16f;

        /// <summary>Percentile of band-point distances taken as the trunk radius (robust to a stray root/branch vertex).</summary>
        public const float TrunkRadiusPercentile = 0.85f;

        /// <summary>Rocks lower than this (m) with a footprint under <see cref="MinBlockingFootprint"/> are walked over (pebble-sized).</summary>
        public const float MinBlockingHeight = 0.25f;

        public const float MinBlockingFootprint = 0.6f;

        /// <summary>An authored rock collider smaller than this fraction of its visual (XZ) is stale (e.g. the old "Sphere" mesh collider at half size) and is replaced.</summary>
        public const float UndersizedColliderRatio = 0.65f;

        /// <summary>Above this many source vertices a rock gets a fitted Box instead of a convex hull (cooking cost on mobile).</summary>
        public const int MaxConvexSourceVertices = 4000;

        /// <summary>Capsule radius as a fraction of the prop's narrower half-footprint (cactus arms/bush fringe stay walk-through).</summary>
        public const float CapsuleFootprintFraction = 0.8f;

        private static readonly string[] NonSolidPrefixes =
        {
            "Tuft_", "GrassTuft", "Grass_", "Pebble_", "Twig_", "Flower_", "Mushroom_", "Leaf_", "Leaves_",
            "Npc_", "Enemy_", "Player", "ArenaSecret_", "SolidCollider", "SolidGapSeal",
        };

        private static readonly string[] BoxPrefixes =
        {
            "Workbench", "Bench", "Banco", "Bancada", "Stall", "Barraca", "Cart", "Carroca", "Carroça", "Fence", "Cerca",
            "House", "Casa", "Crate", "Caixote", "Barrel", "Barril", "Table", "Mesa", "Well", "Poco", "Poço",
            "Anvil", "Bigorna", "Forge", "Forja", "Chest", "Bau", "Baú", "Hut", "Cabana", "Shed", "Wagon",
        };

        private static readonly string[] CapsulePrefixes =
        {
            "ArenaProp_Cactus", "ArenaProp_Stalagmite", "Cactus", "Cacto", "Mandacaru", "Bush_Large", "BigBush", "Arbusto",
            "Post_", "Poste", "Pillar", "Pilar", "Column", "Coluna", "Totem", "Stalagmite",
        };

        private static readonly string[] RockPrefixes =
        {
            "Rock", "ProceduralRock_", "Boulder", "Pedra", "ArenaProp_SnowyRock", "ArenaProp_Boulder",
        };

        /// <summary>
        /// Prop kind from how it was authored (scatter tools and runtime factories name everything by
        /// convention, same as <see cref="EnvironmentPropRules"/>). Grass/tufts/pebbles/twigs and
        /// characters are always <see cref="SolidPropKind.None"/>; the "Rocks" group is not a rock.
        /// </summary>
        public static SolidPropKind Classify(string objectName, string meshName, bool hasLodGroup)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return SolidPropKind.None;
            }

            if (meshName == EnvironmentPropRules.GrassTuftMeshName || StartsWithAny(objectName, NonSolidPrefixes))
            {
                return SolidPropKind.None;
            }

            if (objectName.StartsWith("Tree_", StringComparison.Ordinal) || objectName.StartsWith("Arvore", StringComparison.Ordinal)
                || objectName.StartsWith("Árvore", StringComparison.Ordinal))
            {
                return SolidPropKind.Tree;
            }

            if (StartsWithAny(objectName, CapsulePrefixes))
            {
                return SolidPropKind.Capsule;
            }

            if (objectName != "Rocks" && StartsWithAny(objectName, RockPrefixes))
            {
                return SolidPropKind.Rock;
            }

            if (StartsWithAny(objectName, BoxPrefixes))
            {
                return SolidPropKind.Box;
            }

            return SolidPropKind.None;
        }

        /// <summary>
        /// Collider shape for a kind. Rocks get a convex hull of their (low-poly) visual mesh when it
        /// is readable and small enough, else a fitted box; everything else is a primitive (cheap on mobile).
        /// </summary>
        public static SolidColliderShape ShapeFor(SolidPropKind kind, bool meshReadable, int vertexCount)
        {
            switch (kind)
            {
                case SolidPropKind.Tree:
                    return SolidColliderShape.TrunkCapsule;
                case SolidPropKind.Rock:
                    return meshReadable && vertexCount > 0 && vertexCount <= MaxConvexSourceVertices
                        ? SolidColliderShape.ConvexMesh
                        : SolidColliderShape.Box;
                case SolidPropKind.Box:
                    return SolidColliderShape.Box;
                case SolidPropKind.Capsule:
                    return SolidColliderShape.Capsule;
                default:
                    return SolidColliderShape.None;
            }
        }

        /// <summary>
        /// Whether a prop of this kind and world size blocks at all: pebble-sized rocks (lower than
        /// <see cref="MinBlockingHeight"/>, narrower than <see cref="MinBlockingFootprint"/>) are
        /// walked over; everything else of a solid kind blocks.
        /// </summary>
        public static bool Blocks(SolidPropKind kind, float worldHeight, float worldFootprint)
        {
            if (kind == SolidPropKind.None)
            {
                return false;
            }

            if (kind == SolidPropKind.Rock && worldHeight < MinBlockingHeight && worldFootprint < MinBlockingFootprint)
            {
                return false;
            }

            return worldHeight > 0.01f || worldFootprint > 0.01f;
        }

        /// <summary>True when an authored collider covers clearly less of the visual than it should (XZ, either axis).</summary>
        public static bool IsUndersized(Vector2 colliderSizeXZ, Vector2 visualSizeXZ)
        {
            if (visualSizeXZ.x <= 0.01f || visualSizeXZ.y <= 0.01f)
            {
                return false;
            }

            return colliderSizeXZ.x < visualSizeXZ.x * UndersizedColliderRatio
                || colliderSizeXZ.y < visualSizeXZ.y * UndersizedColliderRatio;
        }

        /// <summary>
        /// Measures the trunk of a tree from its mesh points (in the tree's local frame, Y up, any
        /// unit): takes the points in the low band [<see cref="TrunkBandLow"/>, <see cref="TrunkBandHigh"/>]
        /// of the height, centres on them and uses the <see cref="TrunkRadiusPercentile"/> distance as the
        /// radius — so the canopy never inflates it. <paramref name="unitsPerMeter"/> converts the world
        /// limits (min/max radius, collider height) into the points' unit. Returns false with no points.
        /// </summary>
        public static bool TryEstimateTrunk(IReadOnlyList<Vector3> points, float unitsPerMeter, out TrunkEstimate trunk)
        {
            trunk = default;
            if (points == null || points.Count == 0 || unitsPerMeter <= 0f)
            {
                return false;
            }

            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < points.Count; i++)
            {
                minY = Mathf.Min(minY, points[i].y);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            float height = maxY - minY;
            if (height <= 1e-4f)
            {
                return false;
            }

            float low = minY + height * TrunkBandLow;
            float high = minY + height * TrunkBandHigh;
            var band = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p = points[i];
                if (p.y >= low && p.y <= high)
                {
                    band.Add(new Vector2(p.x, p.z));
                }
            }

            if (band.Count == 0)
            {
                // Sparse mesh: fall back to the lowest points.
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].y <= high)
                    {
                        band.Add(new Vector2(points[i].x, points[i].z));
                    }
                }
            }

            Vector2 centre = Vector2.zero;
            for (int i = 0; i < band.Count; i++)
            {
                centre += band[i];
            }

            centre /= band.Count;

            var distances = new float[band.Count];
            for (int i = 0; i < band.Count; i++)
            {
                distances[i] = (band[i] - centre).magnitude;
            }

            Array.Sort(distances);
            int index = Mathf.Clamp(Mathf.CeilToInt(distances.Length * TrunkRadiusPercentile) - 1, 0, distances.Length - 1);
            float radius = distances[index] + TrunkPadding * unitsPerMeter;
            radius = Mathf.Clamp(radius, MinTrunkRadius * unitsPerMeter, MaxTrunkRadius * unitsPerMeter);

            float colliderHeight = Mathf.Clamp(height * TrunkHeightFraction, MinTrunkColliderHeight * unitsPerMeter, MaxTrunkColliderHeight * unitsPerMeter);
            colliderHeight = Mathf.Min(colliderHeight, height);
            colliderHeight = Mathf.Max(colliderHeight, radius * 2f);

            trunk = new TrunkEstimate
            {
                Center = new Vector3(centre.x, minY + colliderHeight * 0.5f, centre.y),
                Radius = radius,
                Height = colliderHeight,
                TreeHeight = height,
            };
            return true;
        }

        /// <summary>Fallback trunk from bounds only (mesh not readable): a thin trunk at the bounds centre.</summary>
        public static TrunkEstimate TrunkFromBounds(Bounds bounds, float unitsPerMeter)
        {
            float upm = Mathf.Max(unitsPerMeter, 1e-4f);
            float radius = Mathf.Clamp(Mathf.Min(bounds.size.x, bounds.size.z) * 0.08f, MinTrunkRadius * upm, 0.4f * upm);
            float height = Mathf.Clamp(bounds.size.y * TrunkHeightFraction, MinTrunkColliderHeight * upm, MaxTrunkColliderHeight * upm);
            height = Mathf.Max(Mathf.Min(height, bounds.size.y), radius * 2f);
            return new TrunkEstimate
            {
                Center = new Vector3(bounds.center.x, bounds.min.y + height * 0.5f, bounds.center.z),
                Radius = radius,
                Height = height,
                TreeHeight = bounds.size.y,
            };
        }

        /// <summary>Upright capsule (cactus/bush/post) fitted to local bounds: radius from the narrower footprint, full height.</summary>
        public static void CapsuleFromBounds(Bounds bounds, out Vector3 center, out float radius, out float height)
        {
            float halfNarrow = Mathf.Min(bounds.size.x, bounds.size.z) * 0.5f;
            radius = Mathf.Max(0.01f, halfNarrow * CapsuleFootprintFraction);
            height = Mathf.Max(bounds.size.y, radius * 2f);
            center = new Vector3(bounds.center.x, bounds.min.y + height * 0.5f, bounds.center.z);
        }

        /// <summary>Narrowest passage (m) the hero walks through cleanly: its diameter plus <see cref="GapSkin"/>.</summary>
        public static float PassableGap(float heroRadius = HeroRadius)
        {
            return heroRadius * 2f + GapSkin;
        }

        /// <summary>
        /// Two trunks <paramref name="distance"/> apart (centre to centre, XZ, world): leaves a gap
        /// the hero walks through, shrinks both a little (≤ <see cref="MaxTrunkShrink"/> each, never
        /// under <see cref="MinTrunkRadius"/>) to open one that is just too tight, or asks for a
        /// seal when the gap is clearly narrower than the hero — so click-to-move never wedges the
        /// hero into a V between two trunks. <paramref name="radiusA"/>/<paramref name="radiusB"/> are updated on Shrunk.
        /// </summary>
        public static SolidGapResolution ResolveGap(float distance, ref float radiusA, ref float radiusB, float heroRadius = HeroRadius)
        {
            return ResolveGap(distance, ref radiusA, ref radiusB, radiusA - MaxTrunkShrink, radiusB - MaxTrunkShrink, heroRadius);
        }

        /// <summary>
        /// Same as <see cref="ResolveGap(float, ref float, ref float, float)"/>, with explicit floors
        /// (a trunk already shrunk for another neighbour keeps its original floor, so the total shrink
        /// stays within <see cref="MaxTrunkShrink"/>). Floors are clamped to <see cref="MinTrunkRadius"/>.
        /// </summary>
        public static SolidGapResolution ResolveGap(float distance, ref float radiusA, ref float radiusB, float floorA, float floorB, float heroRadius)
        {
            float gap = distance - radiusA - radiusB;
            if (gap <= 0f)
            {
                return SolidGapResolution.AlreadyClosed;
            }

            float need = PassableGap(heroRadius);
            if (gap >= need)
            {
                return SolidGapResolution.Passable;
            }

            float missing = need - gap;
            float shrinkA = Mathf.Min(MaxTrunkShrink, Mathf.Max(0f, radiusA - Mathf.Max(MinTrunkRadius, floorA)));
            float shrinkB = Mathf.Min(MaxTrunkShrink, Mathf.Max(0f, radiusB - Mathf.Max(MinTrunkRadius, floorB)));
            if (shrinkA + shrinkB > 0f && shrinkA + shrinkB >= missing)
            {
                float share = missing / (shrinkA + shrinkB);
                radiusA -= shrinkA * share;
                radiusB -= shrinkB * share;
                return SolidGapResolution.Shrunk;
            }

            return SolidGapResolution.Seal;
        }

        /// <summary>XZ distance from <paramref name="point"/> to the edge of a corridor (segment a-b, half width); negative = inside.</summary>
        public static float CorridorClearance(Vector2 point, Vector2 a, Vector2 b, float halfWidth)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            float t = lengthSq > 1e-8f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq) : 0f;
            return (point - (a + ab * t)).magnitude - halfWidth;
        }

        /// <summary>
        /// Largest radius a collider centred <paramref name="clearance"/> from a road's edge may have
        /// without intruding: <paramref name="desired"/> when it fits, shrunk down to
        /// <paramref name="minRadius"/> otherwise (a trunk standing on the road still blocks — it is
        /// visibly there — but never wider than its minimum).
        /// </summary>
        public static float RadiusAllowedByCorridor(float desired, float clearance, float minRadius)
        {
            if (clearance >= desired)
            {
                return desired;
            }

            return Mathf.Max(minRadius, Mathf.Min(desired, clearance));
        }

        /// <summary>True when a segment p-q (a gap seal) crosses or touches the corridor a-b (half width): seals never close a road.</summary>
        public static bool SegmentTouchesCorridor(Vector2 p, Vector2 q, Vector2 a, Vector2 b, float halfWidth)
        {
            const int Samples = 8;
            for (int i = 0; i <= Samples; i++)
            {
                if (CorridorClearance(Vector2.Lerp(p, q, i / (float)Samples), a, b, halfWidth) < 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool StartsWithAny(string value, string[] prefixes)
        {
            for (int i = 0; i < prefixes.Length; i++)
            {
                if (value.StartsWith(prefixes[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
