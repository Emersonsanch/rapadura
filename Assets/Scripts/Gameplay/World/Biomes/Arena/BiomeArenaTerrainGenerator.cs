using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Builds the two meshes of a biome arena (pure, deterministic per biome + seed; no scene access):
    /// <list type="bullet">
    /// <item><b>Ground</b> — a vertex-coloured height grid (<see cref="Cells"/>² quads over
    /// ±<see cref="HalfExtent"/> m). Flat (y = 0) inside <see cref="ZeroHeightRadius"/>, so the combat
    /// circle (<see cref="FlatRadius"/>) is walkable; the same mesh is the MeshCollider.</item>
    /// <item><b>Backdrop</b> — flat-shaded, vertex-coloured silhouette past the flee radius (tree line,
    /// mesas, cliffs, cave walls) plus flush ground decals (roots, cracks). No collider.</item>
    /// </list>
    /// Local space: arena centre at the origin.
    /// </summary>
    public static class BiomeArenaTerrainGenerator
    {
        /// <summary>Guaranteed-flat combat radius. ≥ EncounterArenaController's spawn clamp (18 m) + an enemy capsule.</summary>
        public const float FlatRadius = 20f;

        /// <summary>Walkable radius = EncounterArenaController's default flee radius: nothing blocks the walk out to here.</summary>
        public const float WalkableRadius = 30f;

        /// <summary>Backdrop geometry inside <see cref="WalkableRadius"/> is either flush (≤ this) or overhead (≥ <see cref="OverheadClearance"/>).</summary>
        public const float MaxDecalHeight = 0.3f;

        public const float OverheadClearance = 8f;

        public const float HalfExtent = 50f;
        public const int Cells = 64;

        /// <summary>Encounter seeds fold into this many terrain variants per biome (bounds the mesh cache).</summary>
        public const int VariantsPerBiome = 3;

        public static float CellSize => HalfExtent * 2f / Cells;

        /// <summary>
        /// Height is exactly 0 up to here: <see cref="FlatRadius"/> plus 1.5 cells, so every grid
        /// triangle touching the combat circle has all three vertices at y = 0.
        /// </summary>
        public static float ZeroHeightRadius => FlatRadius + CellSize * 1.5f;

        public static int VariantFor(int encounterSeed)
        {
            int v = encounterSeed % VariantsPerBiome;
            return v < 0 ? v + VariantsPerBiome : v;
        }

        // ---------------------------------------------------------------
        // Ground
        // ---------------------------------------------------------------

        public static Mesh BuildGround(BiomeArenaTerrainField field)
        {
            int side = Cells + 1;
            var vertices = new Vector3[side * side];
            var colors = new Color32[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            float step = CellSize;

            for (int row = 0; row < side; row++)
            {
                float z = -HalfExtent + row * step;
                for (int col = 0; col < side; col++)
                {
                    float x = -HalfExtent + col * step;
                    int index = row * side + col;
                    vertices[index] = new Vector3(x, field.Height(x, z), z);
                    colors[index] = field.GroundColor(x, z);
                    uvs[index] = new Vector2(x * 0.1f, z * 0.1f);
                }
            }

            var triangles = new int[Cells * Cells * 6];
            int t = 0;
            for (int row = 0; row < Cells; row++)
            {
                for (int col = 0; col < Cells; col++)
                {
                    int i0 = row * side + col;
                    int i1 = i0 + 1;
                    int i2 = i0 + side;
                    int i3 = i2 + 1;
                    // Alternate the diagonal so the grid doesn't show one directional grain.
                    if (((row + col) & 1) == 0)
                    {
                        triangles[t++] = i0; triangles[t++] = i2; triangles[t++] = i1;
                        triangles[t++] = i1; triangles[t++] = i2; triangles[t++] = i3;
                    }
                    else
                    {
                        triangles[t++] = i0; triangles[t++] = i2; triangles[t++] = i3;
                        triangles[t++] = i0; triangles[t++] = i3; triangles[t++] = i1;
                    }
                }
            }

            var mesh = new Mesh { name = $"ArenaGround_{field.Biome}_{field.Seed}" };
            mesh.indexFormat = vertices.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.colors32 = colors;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ---------------------------------------------------------------
        // Backdrop
        // ---------------------------------------------------------------

        public static Mesh BuildBackdrop(BiomeArenaTerrainField field)
        {
            var b = new ArenaMeshBuilder();
            var random = new System.Random(unchecked(field.Seed * 31337 + (int)field.Biome * 977 + 5));
            switch (field.Biome)
            {
                case BiomeType.Desert:
                    BuildDesert(b, field, random);
                    break;
                case BiomeType.Mountain:
                    BuildMountain(b, field, random);
                    break;
                case BiomeType.Caves:
                    BuildCaves(b, field, random);
                    break;
                default:
                    BuildForest(b, field, random);
                    break;
            }

            Mesh mesh = b.ToMesh($"ArenaBackdrop_{field.Biome}_{field.Seed}");
            mesh.RecalculateBounds();
            return mesh;
        }

        private static float Range(System.Random random, float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());

        private static Color Vary(Color color, System.Random random, float amount)
        {
            float k = 1f + ((float)random.NextDouble() * 2f - 1f) * amount;
            return new Color(Mathf.Clamp01(color.r * k), Mathf.Clamp01(color.g * k), Mathf.Clamp01(color.b * k), 1f);
        }

        private static Vector3 OnGround(BiomeArenaTerrainField field, float x, float z, float sink = 0f)
        {
            return new Vector3(x, field.Height(x, z) - sink, z);
        }

        /// <summary>Stratified angle around the ring (props spread evenly instead of clumping).</summary>
        private static Vector2 RingPoint(System.Random random, int index, int count, float minRadius, float maxRadius)
        {
            float angle = (index + Range(random, 0.1f, 0.9f)) * Mathf.PI * 2f / count;
            float radius = Range(random, minRadius, maxRadius);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        // ----- Forest: tree line + surface roots -------------------------------------------

        private static void BuildForest(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random)
        {
            var trunk = new Color(0.33f, 0.23f, 0.15f);
            var pine = new Color(0.15f, 0.31f, 0.17f);
            var leaves = new Color(0.27f, 0.46f, 0.20f);
            var leavesShade = new Color(0.17f, 0.31f, 0.13f);
            var root = new Color(0.30f, 0.21f, 0.13f);

            var rootSources = new List<Vector3>();
            const int inner = 56;
            for (int i = 0; i < inner; i++)
            {
                Vector2 p = RingPoint(random, i, inner, 34.5f, 43f);
                Vector3 at = OnGround(field, p.x, p.y, 0.2f);
                float yaw = Range(random, 0f, 360f);
                if (random.NextDouble() < 0.68)
                {
                    Pine(b, at, Range(random, 0.85f, 1.25f), yaw, trunk, Vary(pine, random, 0.15f), null, random);
                }
                else
                {
                    Broadleaf(b, at, Range(random, 0.85f, 1.2f), yaw, trunk, Vary(leaves, random, 0.15f), leavesShade);
                }

                if (p.magnitude < 37f && rootSources.Count < 16)
                {
                    rootSources.Add(at);
                }
            }

            // Back row: bigger, trunkless cones that close the gaps in the silhouette.
            const int outer = 40;
            for (int i = 0; i < outer; i++)
            {
                Vector2 p = RingPoint(random, i, outer, 44f, 48f);
                Vector3 at = OnGround(field, p.x, p.y, 0.5f);
                float s = Range(random, 1.2f, 1.6f);
                Color c = Vary(pine * 0.85f, random, 0.12f);
                b.Cone(at + Vector3.up * 0.8f * s, 3.2f * s, 5.5f * s, 6, Range(random, 0f, 60f), c);
                b.Cone(at + Vector3.up * 4.2f * s, 2.3f * s, 4.6f * s, 6, Range(random, 0f, 60f), c);
            }

            // Surface roots crawling from the inner tree line towards the clearing.
            foreach (Vector3 source in rootSources)
            {
                int roots = random.Next(2, 4);
                for (int k = 0; k < roots; k++)
                {
                    Vector2 inward = -new Vector2(source.x, source.z).normalized;
                    float angle = Mathf.Atan2(inward.y, inward.x) + Range(random, -0.9f, 0.9f);
                    Root(b, field, new Vector2(source.x, source.z), angle, Range(random, 3f, 6f), Vary(root, random, 0.12f), random);
                }
            }

            // A few root knots inside the clearing (flush, walk-over) so the fight floor isn't bare.
            for (int i = 0; i < 5; i++)
            {
                Vector2 knot = RingPoint(random, i, 5, 12f, 18f);
                int roots = random.Next(3, 6);
                for (int k = 0; k < roots; k++)
                {
                    Root(b, field, knot, Range(random, 0f, Mathf.PI * 2f), Range(random, 1.5f, 3f), Vary(root, random, 0.12f), random, 0.12f);
                }
            }
        }

        private static void Pine(ArenaMeshBuilder b, Vector3 at, float scale, float yaw, Color trunk, Color needles, Color? snow, System.Random random)
        {
            b.Frustum(at, 0.35f * scale, 0.25f * scale, 1.9f * scale, 5, yaw, trunk, false);
            float[] baseY = { 1.4f, 3.3f, 5.1f };
            float[] radius = { 2.3f, 1.8f, 1.25f };
            float[] height = { 3.2f, 2.8f, 2.4f };
            for (int i = 0; i < baseY.Length; i++)
            {
                Color tier = snow.HasValue ? Color.Lerp(needles, snow.Value, 0.35f + 0.15f * i) : needles;
                b.Cone(at + Vector3.up * baseY[i] * scale, radius[i] * scale, height[i] * scale, 6, yaw + i * 20f, tier, snow);
            }
        }

        private static void Broadleaf(ArenaMeshBuilder b, Vector3 at, float scale, float yaw, Color trunk, Color crown, Color shade)
        {
            b.Frustum(at, 0.42f * scale, 0.3f * scale, 2.6f * scale, 5, yaw, trunk, false);
            b.Blob(at + Vector3.up * 3.6f * scale, new Vector3(2.4f, 1.9f, 2.2f) * scale, yaw, crown, shade);
            b.Blob(at + new Vector3(0.8f, 4.9f, 0.4f) * scale, new Vector3(1.6f, 1.3f, 1.5f) * scale, yaw + 30f, crown, shade);
        }

        /// <summary>Low ridge ("tent" profile) following the terrain from <paramref name="start"/> along <paramref name="angle"/>.</summary>
        private static void Root(ArenaMeshBuilder b, BiomeArenaTerrainField field, Vector2 start, float angle, float length, Color color,
            System.Random random, float rise = 0.2f)
        {
            const int segments = 3;
            float width = 0.22f;
            Vector2 p0 = start;
            for (int s = 0; s < segments; s++)
            {
                angle += Range(random, -0.35f, 0.35f);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 p1 = p0 + dir * (length / segments);
                float t0 = 1f - (float)s / segments;
                float t1 = 1f - (float)(s + 1) / segments;
                Vector2 side = new Vector2(-dir.y, dir.x);

                Vector3 ridge0 = OnGround(field, p0.x, p0.y) + Vector3.up * (rise * t0 + 0.02f);
                Vector3 ridge1 = OnGround(field, p1.x, p1.y) + Vector3.up * (rise * t1 + 0.02f);
                Vector2 l0 = p0 + side * width * (0.5f + t0), r0 = p0 - side * width * (0.5f + t0);
                Vector2 l1 = p1 + side * width * (0.5f + t1), r1 = p1 - side * width * (0.5f + t1);
                Vector3 left0 = OnGround(field, l0.x, l0.y, 0.03f), right0 = OnGround(field, r0.x, r0.y, 0.03f);
                Vector3 left1 = OnGround(field, l1.x, l1.y, 0.03f), right1 = OnGround(field, r1.x, r1.y, 0.03f);
                var sideUp = new Vector3(side.x, 1.5f, side.y);
                b.Quad(left0, left1, ridge1, ridge0, color, sideUp);
                b.Quad(right0, right1, ridge1, ridge0, color * 0.85f, new Vector3(-side.x, 1.5f, -side.y));
                p0 = p1;
            }
        }

        // ----- Desert: mesas, buttes, cracked-earth patch ------------------------------------

        private static void BuildDesert(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random)
        {
            Color[] strata =
            {
                new Color(0.78f, 0.50f, 0.33f),
                new Color(0.88f, 0.64f, 0.42f),
                new Color(0.72f, 0.45f, 0.30f),
                new Color(0.84f, 0.58f, 0.38f),
            };
            var mesaTop = new Color(0.86f, 0.68f, 0.46f);

            const int mesas = 8;
            for (int i = 0; i < mesas; i++)
            {
                Vector2 p = RingPoint(random, i, mesas, 43f, 47f);
                Vector3 at = OnGround(field, p.x, p.y, 1.5f);
                float radius = Range(random, 4.5f, 7.5f);
                float height = Range(random, 8f, 15f);
                int layers = 3;
                float y = 0f;
                float yaw = Range(random, 0f, 360f);
                for (int layer = 0; layer < layers; layer++)
                {
                    float layerHeight = height / layers;
                    float r0 = radius * (1f - 0.08f * layer);
                    float r1 = radius * (1f - 0.08f * (layer + 1));
                    bool top = layer == layers - 1;
                    var layerRandom = new System.Random(random.Next());
                    b.Frustum(at + Vector3.up * y, r0, r1, layerHeight, 8, yaw, Vary(strata[(i + layer) % strata.Length], random, 0.05f),
                        top, layerRandom, 0.12f, mesaTop);
                    y += layerHeight;
                }
            }

            // Closer, slimmer buttes/spires.
            const int spires = 7;
            for (int i = 0; i < spires; i++)
            {
                Vector2 p = RingPoint(random, i, spires, 34.5f, 39f);
                Vector3 at = OnGround(field, p.x, p.y, 0.6f);
                float r = Range(random, 1.1f, 2f);
                float h = Range(random, 3.5f, 7.5f);
                b.Frustum(at, r, r * 0.55f, h, 6, Range(random, 0f, 360f), Vary(strata[i % strata.Length], random, 0.06f), true,
                    new System.Random(random.Next()), 0.15f, mesaTop);
            }

            // Cracked-earth patch (flush decals) on the clay spot GroundColor paints at PatchCenter.
            Cracks(b, field, random, field.PatchCenter, field.PatchRadius, 26, new Color(0.46f, 0.31f, 0.20f), 0.08f);
        }

        /// <summary>Random-walk crack network of thin flush quads inside the patch circle.</summary>
        private static void Cracks(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random, Vector2 center, float radius,
            int count, Color color, float width)
        {
            for (int i = 0; i < count; i++)
            {
                float a = Range(random, 0f, Mathf.PI * 2f);
                Vector2 p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius * Mathf.Sqrt((float)random.NextDouble()) * 0.85f;
                float heading = Range(random, 0f, Mathf.PI * 2f);
                int segments = random.Next(3, 7);
                float w = width;
                for (int s = 0; s < segments; s++)
                {
                    heading += Range(random, -0.6f, 0.6f);
                    Vector2 dir = new Vector2(Mathf.Cos(heading), Mathf.Sin(heading));
                    Vector2 q = p + dir * Range(random, 0.45f, 1f);
                    if (Vector2.Distance(q, center) > radius)
                    {
                        break;
                    }

                    Vector2 side = new Vector2(-dir.y, dir.x) * (w * 0.5f);
                    float wq = w * 0.8f;
                    Vector2 sideQ = new Vector2(-dir.y, dir.x) * (wq * 0.5f);
                    b.Quad(Flush(field, p + side), Flush(field, q + sideQ), Flush(field, q - sideQ), Flush(field, p - side), color, Vector3.up);
                    p = q;
                    w = wq;
                }
            }
        }

        private static Vector3 Flush(BiomeArenaTerrainField field, Vector2 p) => new Vector3(p.x, field.Height(p.x, p.y) + 0.02f, p.y);

        // ----- Mountain: cliff ring with snow caps, crags, snowy pines --------------------

        private static void BuildMountain(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random)
        {
            var rockLow = new Color(0.42f, 0.43f, 0.47f);
            var rockHigh = new Color(0.56f, 0.57f, 0.61f);
            var snow = new Color(0.93f, 0.95f, 1f);

            float[] rowHeights = { -1.5f, Range(random, 6f, 8f), Range(random, 12f, 15f), Range(random, 18f, 22f) };
            float[] rowInset = { 0f, 1.5f, 1f, 3f };
            Ring(b, field, random, 46, 40f, 2.5f, rowHeights, rowInset, true, 0.9f, (row, t) =>
                row == rowHeights.Length - 2 ? Color.Lerp(rockHigh, snow, 0.55f) : Color.Lerp(rockLow, rockHigh, t), snow);

            const int crags = 14;
            for (int i = 0; i < crags; i++)
            {
                Vector2 p = RingPoint(random, i, crags, 33.5f, 38f);
                Vector3 at = OnGround(field, p.x, p.y, 0.4f);
                float r = Range(random, 1.4f, 2.6f);
                float h = Range(random, 3f, 6.5f);
                float yaw = Range(random, 0f, 360f);
                b.Frustum(at, r, r * 0.4f, h * 0.65f, 5, yaw, Vary(rockLow, random, 0.1f), false, new System.Random(random.Next()), 0.2f);
                b.Cone(at + Vector3.up * h * 0.65f, r * 0.4f, h * 0.35f, 5, yaw, snow);
            }

            const int pines = 10;
            var trunk = new Color(0.30f, 0.22f, 0.16f);
            var needles = new Color(0.16f, 0.30f, 0.22f);
            for (int i = 0; i < pines; i++)
            {
                Vector2 p = RingPoint(random, i, pines, 33.5f, 38.5f);
                Pine(b, OnGround(field, p.x, p.y, 0.2f), Range(random, 0.8f, 1.1f), Range(random, 0f, 360f), trunk, Vary(needles, random, 0.1f), snow, random);
            }
        }

        /// <summary>
        /// Jagged wall ring facing the centre: <paramref name="segments"/> columns around
        /// <paramref name="radius"/> (±<paramref name="radiusNoise"/>), vertex rows at
        /// <paramref name="rowHeights"/> (row 0 relative to the terrain, sunk), each row pulled in by
        /// <paramref name="rowInset"/> (positive = leaning towards the centre). Optionally caps the top
        /// with a ledge sloping away from the arena.
        /// </summary>
        private static void Ring(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random, int segments, float radius, float radiusNoise,
            float[] rowHeights, float[] rowInset, bool capTop, float jitter, System.Func<int, float, Color> rowColor, Color capColor)
        {
            int rows = rowHeights.Length;
            var grid = new Vector3[segments, rows];
            for (int s = 0; s < segments; s++)
            {
                float angle = (s + Range(random, -0.2f, 0.2f)) * Mathf.PI * 2f / segments;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float baseRadius = radius + Range(random, -radiusNoise, radiusNoise);
                float inset = 0f;
                for (int row = 0; row < rows; row++)
                {
                    inset += rowInset[row];
                    float r = baseRadius - inset + Range(random, -jitter, jitter);
                    Vector2 p = dir * r;
                    float y = row == 0
                        ? field.Height(p.x, p.y) + rowHeights[0]
                        : rowHeights[row] + Range(random, -1f, 1f) + field.Height(dir.x * baseRadius, dir.y * baseRadius) * 0.5f;
                    grid[s, row] = new Vector3(p.x, y, p.y);
                }
            }

            for (int s = 0; s < segments; s++)
            {
                int n = (s + 1) % segments;
                for (int row = 0; row < rows - 1; row++)
                {
                    Vector3 mid = (grid[s, row] + grid[n, row] + grid[s, row + 1] + grid[n, row + 1]) * 0.25f;
                    Vector3 inward = new Vector3(-mid.x, 0f, -mid.z);
                    Color c = Vary(rowColor(row, (float)row / Mathf.Max(1, rows - 2)), random, 0.07f);
                    b.Quad(grid[s, row], grid[n, row], grid[n, row + 1], grid[s, row + 1], c, inward);
                }

                if (capTop)
                {
                    Vector3 a = grid[s, rows - 1];
                    Vector3 c2 = grid[n, rows - 1];
                    Vector3 outA = a + new Vector3(a.x, 0f, a.z).normalized * 7f + Vector3.down * 2.5f;
                    Vector3 outC = c2 + new Vector3(c2.x, 0f, c2.z).normalized * 7f + Vector3.down * 2.5f;
                    b.Quad(a, c2, outC, outA, Vary(capColor, random, 0.03f), Vector3.up);
                }
            }
        }

        // ----- Caves: enclosing wall ring, overhang, stalagmites/stalactites, fissures ----

        private static void BuildCaves(ArenaMeshBuilder b, BiomeArenaTerrainField field, System.Random random)
        {
            var wallLow = new Color(0.24f, 0.22f, 0.23f);
            var wallHigh = new Color(0.07f, 0.065f, 0.075f);
            var ceiling = new Color(0.03f, 0.03f, 0.035f);
            var spike = new Color(0.30f, 0.28f, 0.30f);
            var spikeTip = new Color(0.44f, 0.42f, 0.45f);

            float[] rowHeights = { -1f, 5f, 11f, 15.5f };
            float[] rowInset = { 0f, 1.2f, 1.8f, 2.6f };
            Ring(b, field, random, 52, 34.5f, 1.2f, rowHeights, rowInset, false, 0.6f,
                (row, t) => Color.Lerp(wallLow, wallHigh, t), ceiling);

            // Overhang: a dark band hanging inwards from the wall top (faces down), the cave's ceiling edge.
            const int segments = 40;
            for (int s = 0; s < segments; s++)
            {
                float a0 = s * Mathf.PI * 2f / segments;
                float a1 = (s + 1) * Mathf.PI * 2f / segments;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                float outer = 32f;
                float innerR = 25f + Range(random, -1f, 1f);
                b.Quad(d0 * outer + Vector3.up * 15f, d1 * outer + Vector3.up * 15f, d1 * innerR + Vector3.up * 16.5f, d0 * innerR + Vector3.up * 16.5f,
                    ceiling, Vector3.down);
                // Topside, so from above the walls read as a solid rock mass instead of open sky.
                b.Quad(d0 * outer + Vector3.up * 15f, d1 * outer + Vector3.up * 15f, d1 * 60f + Vector3.up * 15f, d0 * 60f + Vector3.up * 15f,
                    ceiling, Vector3.up);
            }

            const int stalagmites = 34;
            for (int i = 0; i < stalagmites; i++)
            {
                Vector2 p = RingPoint(random, i, stalagmites, 31.6f, 33.5f);
                Vector3 at = OnGround(field, p.x, p.y, 0.2f);
                float r = Range(random, 0.5f, 1.3f);
                b.Cone(at, r, Range(random, 1.6f, 5f), 5, Range(random, 0f, 360f), Vary(spike, random, 0.1f), spikeTip);
            }

            const int stalactites = 26;
            for (int i = 0; i < stalactites; i++)
            {
                Vector2 p = RingPoint(random, i, stalactites, 30.5f, 32f);
                float r = Range(random, 0.4f, 0.9f);
                b.Cone(new Vector3(p.x, 15.5f, p.y), r, -Range(random, 2f, 5f), 5, Range(random, 0f, 360f), Vary(spike * 0.7f, random, 0.1f));
            }

            // Fissures on the floor (flush decals).
            Cracks(b, field, random, field.PatchCenter, field.PatchRadius, 18, new Color(0.08f, 0.075f, 0.085f), 0.1f);
            Vector2 second = -field.PatchCenter.normalized * 14f;
            Cracks(b, field, random, second, 3f, 12, new Color(0.08f, 0.075f, 0.085f), 0.09f);
        }
    }
}
