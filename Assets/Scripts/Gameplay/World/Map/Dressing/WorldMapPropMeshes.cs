using System.Collections.Generic;
using Rapadura.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>
    /// Low-poly geometry recipes for the world map, written into a <see cref="WorldMapMeshBuilder"/>
    /// in prop-local space (pivot at the base, front facing -z = the camera): static props, region
    /// landmarks (village, ruin, peak camp, cave mouth), locked fog, cleared banner, bridge, road
    /// stones, sea and clouds. Rocks reuse <see cref="ProceduralRockGenerator"/> meshes.
    /// </summary>
    public sealed class WorldMapPropMeshes
    {
        private const int RockVariants = 4;

        private readonly List<Mesh> _rocks = new List<Mesh>(RockVariants);

        public WorldMapPropMeshes(int seed)
        {
            for (int i = 0; i < RockVariants; i++)
            {
                Mesh rock = ProceduralRockGenerator.BuildRockMesh(unchecked(seed * 17 + 4001 + i * 131), 1, 0.32f, 0.72f);
                rock.name = "WorldMapRock_" + i;
                _rocks.Add(rock);
            }
        }

        /// <summary>Generated rock meshes (owned: destroy with the dressing).</summary>
        public IReadOnlyList<Mesh> OwnedMeshes => _rocks;

        private Mesh Rock(int variant) => _rocks[Mathf.Abs(variant) % _rocks.Count];

        // ---------------------------------------------------------------
        // Static props
        // ---------------------------------------------------------------

        /// <summary>Appends a static (non-swaying) prop. Returns false for kinds built as their own objects.</summary>
        public bool AppendStatic(WorldMapMeshBuilder b, WorldMapPropKind kind, int variant)
        {
            switch (kind)
            {
                case WorldMapPropKind.Bush:
                    b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.65f, 0.8f), WorldMapSwatch.Bush);
                    b.Blob(new Vector3(0.35f, 0.22f, 0.15f), new Vector3(0.55f, 0.45f, 0.5f), WorldMapSwatch.Leaf);
                    b.Blob(new Vector3(-0.15f, 0.55f, -0.25f), new Vector3(0.14f, 0.14f, 0.14f), WorldMapSwatch.Flower);
                    b.Blob(new Vector3(0.3f, 0.45f, -0.2f), new Vector3(0.12f, 0.12f, 0.12f), WorldMapSwatch.Flower);
                    return true;

                case WorldMapPropKind.DesertRock:
                    b.AddMesh(Rock(variant), Matrix4x4.TRS(new Vector3(0f, 0.15f, 0f), Quaternion.identity, new Vector3(0.6f, 0.55f, 0.5f)), WorldMapSwatch.SandRock);
                    b.AddMesh(Rock(variant + 1), Matrix4x4.TRS(new Vector3(0.5f, 0.05f, 0.2f), Quaternion.identity, new Vector3(0.25f, 0.25f, 0.25f)), WorldMapSwatch.SandRock);
                    return true;

                case WorldMapPropKind.Rock:
                    b.AddMesh(Rock(variant), Matrix4x4.TRS(new Vector3(0f, 0.15f, 0f), Quaternion.identity, new Vector3(0.6f, 0.6f, 0.55f)), WorldMapSwatch.Rock);
                    b.AddMesh(Rock(variant + 2), Matrix4x4.TRS(new Vector3(-0.45f, 0.05f, 0.25f), Quaternion.identity, new Vector3(0.28f, 0.28f, 0.28f)), WorldMapSwatch.RockDark);
                    return true;

                case WorldMapPropKind.SnowRock:
                    b.AddMesh(Rock(variant), Matrix4x4.TRS(new Vector3(0f, 0.2f, 0f), Quaternion.identity, new Vector3(0.8f, 0.75f, 0.7f)), WorldMapSwatch.Rock, WorldMapSwatch.Snow);
                    return true;

                case WorldMapPropKind.Peak:
                    AppendPeak(b, Vector3.zero, 2.3f, 4.4f, variant);
                    AppendPeak(b, new Vector3(1.6f, 0f, 0.8f), 1.4f, 2.6f, variant + 1);
                    return true;

                case WorldMapPropKind.Stalagmite:
                    b.Cylinder(Vector3.zero, 0.42f, 0.28f, 0.55f, 6, WorldMapSwatch.CaveStone, yaw: variant * 13f);
                    b.Cylinder(new Vector3(0f, 0.55f, 0f), 0.28f, 0.14f, 0.6f, 6, WorldMapSwatch.CaveStone, yaw: variant * 29f);
                    b.Cylinder(new Vector3(0f, 1.15f, 0f), 0.14f, 0f, 0.5f, 6, WorldMapSwatch.Rock);
                    b.Cylinder(new Vector3(0.45f, 0f, 0.1f), 0.2f, 0f, 0.6f, 5, WorldMapSwatch.CaveStone);
                    return true;

                case WorldMapPropKind.Crystal:
                    AppendCrystals(b, Vector3.zero, 1f, variant);
                    return true;

                case WorldMapPropKind.CaveRock:
                    b.AddMesh(Rock(variant), Matrix4x4.TRS(new Vector3(0f, 0.2f, 0f), Quaternion.identity, new Vector3(0.7f, 0.65f, 0.65f)), WorldMapSwatch.CaveStone, WorldMapSwatch.CaveDark);
                    return true;

                default:
                    return false;
            }
        }

        private static void AppendPeak(WorldMapMeshBuilder b, Vector3 at, float radius, float height, int variant)
        {
            float yaw = variant * 37f;
            float snowLine = 0.6f;
            b.Cylinder(at, radius, radius * (1f - snowLine), height * snowLine, 6, WorldMapSwatch.Rock, WorldMapSwatch.Snow, yaw);
            b.Cylinder(at + Vector3.up * height * snowLine, radius * (1f - snowLine), 0f, height * (1f - snowLine), 6, WorldMapSwatch.Snow, yaw: yaw);
            b.Cylinder(at + new Vector3(0f, -0.05f, 0f), radius * 1.08f, radius * 0.9f, 0.35f, 6, WorldMapSwatch.RockDark, yaw: yaw + 30f);
        }

        private static void AppendCrystals(WorldMapMeshBuilder b, Vector3 at, float size, int variant)
        {
            Matrix4x4 saved = b.Matrix;
            float[] tilts = { 0f, 22f, -18f };
            float[] heights = { 0.95f, 0.65f, 0.55f };
            for (int i = 0; i < tilts.Length; i++)
            {
                float yaw = variant * 47f + i * 120f;
                b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, tilts[i]), Vector3.one * size);
                float h = heights[i];
                b.Cylinder(Vector3.zero, 0.13f, 0.13f, h * 0.7f, 5, WorldMapSwatch.Crystal);
                b.Cylinder(new Vector3(0f, h * 0.7f, 0f), 0.13f, 0f, h * 0.3f, 5, WorldMapSwatch.Crystal);
            }

            b.Matrix = saved;
            b.Blob(at + new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.2f, 0.5f) * size, WorldMapSwatch.CaveDark);
        }

        /// <summary>Simple conifer / tugai poplar for when the tree prefabs are not available.</summary>
        public static void AppendFallbackTree(WorldMapMeshBuilder b, bool pine, float height)
        {
            if (pine)
            {
                float trunk = height * 0.28f;
                b.Cylinder(Vector3.zero, 0.14f, 0.1f, trunk, 5, WorldMapSwatch.Trunk);
                b.Cylinder(new Vector3(0f, trunk * 0.8f, 0f), 0.75f, 0f, height * 0.5f, 7, WorldMapSwatch.PineLeaf);
                b.Cylinder(new Vector3(0f, trunk * 0.8f + height * 0.3f, 0f), 0.55f, 0f, height * 0.45f, 7, WorldMapSwatch.PineLeaf);
            }
            else
            {
                AppendPoplar(b, Vector3.zero, height, false);
            }
        }

        // ---------------------------------------------------------------
        // Ilha da Aurora vegetation (world bible §2): tugai poplar, apricot, saxaul, tamarisk, baobab, aloe.
        // Pivot at the base; small helpers shared by the town clusters and the secret hints.
        // ---------------------------------------------------------------

        /// <summary>A tugai poplar: pale trunk and a tall narrow crown, gold-flecked (or all gold in autumn).</summary>
        public static void AppendPoplar(WorldMapMeshBuilder b, Vector3 at, float height, bool gold)
        {
            float trunk = height * 0.3f;
            float k = height / 2.6f;
            b.Cylinder(at, 0.12f * k, 0.08f * k, trunk, 5, WorldMapSwatch.Trunk);
            b.Blob(at + new Vector3(0f, trunk + height * 0.32f, 0f), new Vector3(0.75f, 1.75f, 0.7f) * k, gold ? WorldMapSwatch.PoplarGold : WorldMapSwatch.Poplar);
            b.Blob(at + new Vector3(0.12f * k, trunk + height * 0.5f, -0.1f * k), new Vector3(0.45f, 0.9f, 0.45f) * k, gold ? WorldMapSwatch.Gold : WorldMapSwatch.PoplarGold, 30f, coarse: true);
        }

        /// <summary>An orchard apricot tree: short crooked trunk, round crown dotted with fruit (the Vila da Vó Edna orchard).</summary>
        public static void AppendApricot(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            b.Cylinder(at, 0.08f * size, 0.06f * size, 0.45f * size, 5, WorldMapSwatch.Trunk, yaw: 15f);
            b.Blob(at + new Vector3(0f, 0.72f, 0f) * size, new Vector3(0.95f, 0.65f, 0.9f) * size, WorldMapSwatch.Leaf);
            b.Blob(at + new Vector3(0.18f, 0.85f, -0.1f) * size, new Vector3(0.5f, 0.4f, 0.5f) * size, WorldMapSwatch.LeafDark, 40f, coarse: true);
            Vector3[] fruit = { new Vector3(-0.3f, 0.62f, -0.32f), new Vector3(0.25f, 0.55f, -0.36f), new Vector3(0.05f, 0.9f, -0.38f), new Vector3(0.38f, 0.78f, 0.1f) };
            foreach (Vector3 f in fruit)
            {
                b.Blob(at + f * size, Vector3.one * 0.09f * size, WorldMapSwatch.Apricot, 0f, true);
            }
        }

        /// <summary>A saxaul of the Flaming Sands: a gnarled pale trunk forking into crooked branches with flat grey-green crowns.</summary>
        public static void AppendSaxaul(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.identity, Vector3.one * size);
            b.Blob(new Vector3(0.02f, 0.06f, 0f), new Vector3(0.36f, 0.14f, 0.32f), WorldMapSwatch.RuinDark, 0f, true);
            b.Cylinder(Vector3.zero, 0.11f, 0.08f, 0.55f, 5, WorldMapSwatch.RuinDark, yaw: 20f);
            Vector3 a = Branch(b, new Vector3(0.02f, 0.5f, 0f), -28f, 8f, 0.62f, 0.06f);
            Vector3 c = Branch(b, new Vector3(0f, 0.48f, 0f), 26f, -14f, 0.55f, 0.055f);
            Vector3 d = Branch(b, new Vector3(0f, 0.52f, 0f), 6f, 30f, 0.6f, 0.05f);
            b.Blob(a, new Vector3(0.5f, 0.24f, 0.44f), WorldMapSwatch.Saxaul, 10f);
            b.Blob(c + new Vector3(0f, 0.04f, 0f), new Vector3(0.46f, 0.22f, 0.42f), WorldMapSwatch.Saxaul, 50f);
            b.Blob(d, new Vector3(0.42f, 0.2f, 0.4f), WorldMapSwatch.LeafDark, 80f, coarse: true);
            b.Blob(new Vector3(0.05f, 1.12f, -0.02f), new Vector3(0.42f, 0.2f, 0.4f), WorldMapSwatch.Saxaul, 25f, coarse: true);
            b.Matrix = saved;
        }

        /// <summary>A crooked branch from <paramref name="from"/> (in the current frame), leaning by the two angles; returns its tip.</summary>
        private static Vector3 Branch(WorldMapMeshBuilder b, Vector3 from, float tiltZ, float tiltX, float length, float radius)
        {
            Quaternion lean = Quaternion.Euler(tiltX, 0f, tiltZ);
            Matrix4x4 local = b.Matrix;
            b.Matrix = local * Matrix4x4.TRS(from, lean, Vector3.one);
            b.Cylinder(Vector3.zero, radius, radius * 0.6f, length, 4, WorldMapSwatch.RuinDark);
            b.Matrix = local;
            return from + lean * Vector3.up * length;
        }

        /// <summary>A tamarisk: a fan of thin stems with feathery pink plumes over a grey-green skirt.</summary>
        public static void AppendTamarisk(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.identity, Vector3.one * size);
            b.Blob(new Vector3(0f, 0.25f, 0f), new Vector3(0.75f, 0.45f, 0.7f), WorldMapSwatch.Saxaul, 0f, true);
            float[] angles = { 0f, 72f, 144f, 216f, 288f };
            for (int i = 0; i < angles.Length; i++)
            {
                float rad = angles[i] * Mathf.Deg2Rad;
                Vector3 tip = Branch(b, new Vector3(0f, 0.05f, 0f), Mathf.Cos(rad) * 22f, Mathf.Sin(rad) * 22f, 0.85f + (i % 2) * 0.25f, 0.035f);
                b.Blob(tip, new Vector3(0.3f, 0.55f, 0.3f), i % 3 == 0 ? WorldMapSwatch.Flower : WorldMapSwatch.Tamarisk, angles[i]);
            }

            b.Matrix = saved;
        }

        /// <summary>A baobab (African highlands, Litema): a fat bottle trunk with a few stubby branches and small crowns.</summary>
        public static void AppendBaobab(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            b.Cylinder(at, 0.34f * size, 0.26f * size, 0.75f * size, 7, WorldMapSwatch.SandRock, WorldMapSwatch.RuinDark);
            b.Cylinder(at + Vector3.up * 0.75f * size, 0.26f * size, 0.12f * size, 0.2f * size, 7, WorldMapSwatch.SandRock);
            Vector3 top = at + Vector3.up * 0.92f * size;
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 20f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 tip = top + (dir * 0.32f + Vector3.up * 0.18f) * size;
                b.Box((top + tip) * 0.5f, new Vector3(0.08f + Mathf.Abs(dir.x) * 0.3f, 0.14f, 0.08f + Mathf.Abs(dir.z) * 0.3f) * size, WorldMapSwatch.SandRock);
                b.Blob(tip + Vector3.up * 0.05f * size, new Vector3(0.36f, 0.2f, 0.36f) * size, i % 2 == 0 ? WorldMapSwatch.Leaf : WorldMapSwatch.LeafDark, i * 30f, coarse: true);
            }
        }

        /// <summary>An aloe in flower (Kaggen Range): a rosette of spiky leaves and a red flower spike.</summary>
        public static void AppendAloe(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                b.Cylinder(at + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.08f * size, 0.06f * size, 0f, 0.3f * size, 4, i % 2 == 0 ? WorldMapSwatch.Cactus : WorldMapSwatch.CactusDark, yaw: i * 40f);
            }

            b.Cylinder(at, 0.015f * size, 0.012f * size, 0.5f * size, 4, WorldMapSwatch.CactusDark);
            b.Cylinder(at + Vector3.up * 0.42f * size, 0.05f * size, 0f, 0.22f * size, 5, WorldMapSwatch.Banner);
        }

        // ---------------------------------------------------------------
        // Swaying props (their own objects, one shared mesh each)
        // ---------------------------------------------------------------

        /// <summary>
        /// The desert's main swaying shrub, pivot at the base (built once, shared): a saxaul. The kind keeps its old
        /// "Cactus" id and object prefix so saved layouts and the shrub sway still find it.
        /// </summary>
        public static void AppendCactus(WorldMapMeshBuilder b)
        {
            AppendSaxaul(b, Vector3.zero, 1.15f);
        }

        /// <summary>The desert's second swaying shrub (every third "Cactus" placement): a tamarisk.</summary>
        public static void AppendDesertTamarisk(WorldMapMeshBuilder b)
        {
            AppendTamarisk(b, Vector3.zero, 1f);
        }

        /// <summary>A clump of crossed-quad grass tufts (the mesh must be named "GrassTuft" so wind finds it).</summary>
        public static void AppendGrassClump(WorldMapMeshBuilder b)
        {
            Vector3[] tufts = { new Vector3(0f, 0f, 0f), new Vector3(0.22f, 0f, 0.12f), new Vector3(-0.18f, 0f, 0.16f), new Vector3(0.05f, 0f, -0.2f) };
            float[] heights = { 0.5f, 0.38f, 0.42f, 0.32f };
            for (int t = 0; t < tufts.Length; t++)
            {
                for (int blade = 0; blade < 3; blade++)
                {
                    Quaternion rotation = Quaternion.Euler(0f, blade * 60f + t * 25f, 0f);
                    Vector3 right = rotation * Vector3.right * 0.16f;
                    Vector3 up = Vector3.up * heights[t];
                    Vector3 lean = rotation * Vector3.forward * 0.04f;
                    Vector3 normal = (Vector3.up + rotation * Vector3.forward * 0.3f).normalized;
                    b.Card(tufts[t] - right, tufts[t] - right * 0.3f + up + lean, tufts[t] + right * 0.3f + up + lean, tufts[t] + right,
                        t % 2 == 0 ? WorldMapSwatch.Grass : WorldMapSwatch.Leaf, normal);
                }
            }
        }

        // ---------------------------------------------------------------
        // Landmarks and node states
        // ---------------------------------------------------------------

        /// <summary>The region's landmark, pivot at the base centre, front (door/opening) facing -z.</summary>
        public void AppendLandmark(WorldMapMeshBuilder b, BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert:
                    AppendRuin(b);
                    break;
                case BiomeType.Mountain:
                    AppendPeakCamp(b);
                    break;
                case BiomeType.Caves:
                    AppendCaveMouth(b);
                    break;
                default:
                    AppendVillage(b);
                    break;
            }
        }

        /// <summary>
        /// A Sogdian adobe house (world bible §2.1), pivot at the base centre, front facing -z: adobe walls on an ochre
        /// plinth, flat earth roof with a parapet and beam ends, and either an iwan (deep arched porch on two carved posts)
        /// or a plain door and lit window. Shared by the map landmark and the town clusters.
        /// </summary>
        public static void AppendAdobeHouse(WorldMapMeshBuilder b, Vector3 at, float yaw, float size, WorldMapSwatch wall, WorldMapSwatch roof, bool iwan)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * size);
            b.Box(new Vector3(0f, 0.04f, 0f), new Vector3(1.1f, 0.08f, 0.95f), WorldMapSwatch.AdobeDark);
            b.Box(new Vector3(0f, 0.38f, 0f), new Vector3(1f, 0.62f, 0.85f), wall);
            // Flat roof slab with its parapet (four strips) and the beam ends under it.
            b.Box(new Vector3(0f, 0.72f, 0f), new Vector3(1.06f, 0.06f, 0.91f), wall, top: roof);
            b.Box(new Vector3(0f, 0.8f, -0.43f), new Vector3(1.06f, 0.1f, 0.05f), wall);
            b.Box(new Vector3(0f, 0.8f, 0.43f), new Vector3(1.06f, 0.1f, 0.05f), wall);
            b.Box(new Vector3(-0.505f, 0.8f, 0f), new Vector3(0.05f, 0.1f, 0.81f), wall);
            b.Box(new Vector3(0.505f, 0.8f, 0f), new Vector3(0.05f, 0.1f, 0.81f), wall);
            for (int i = 0; i < 5; i++)
            {
                b.Box(new Vector3(-0.4f + i * 0.2f, 0.64f, -0.46f), new Vector3(0.05f, 0.05f, 0.08f), WorldMapSwatch.WoodDark);
            }

            if (iwan)
            {
                // The iwan: a dark bay with a pointed head, two thin carved posts, a step, a turquoise line (detail only).
                b.Box(new Vector3(-0.12f, 0.33f, -0.425f), new Vector3(0.5f, 0.42f, 0.02f), WorldMapSwatch.Black);
                b.Triangle(new Vector3(-0.37f, 0.54f, -0.436f), new Vector3(-0.12f, 0.66f, -0.436f), new Vector3(0.13f, 0.54f, -0.436f), WorldMapSwatch.Black);
                b.Box(new Vector3(-0.34f, 0.33f, -0.46f), new Vector3(0.04f, 0.42f, 0.04f), WorldMapSwatch.Wood);
                b.Box(new Vector3(0.1f, 0.33f, -0.46f), new Vector3(0.04f, 0.42f, 0.04f), WorldMapSwatch.Wood);
                b.Box(new Vector3(-0.12f, 0.12f, -0.47f), new Vector3(0.6f, 0.06f, 0.08f), WorldMapSwatch.Wall);
                b.Box(new Vector3(0.34f, 0.44f, -0.43f), new Vector3(0.16f, 0.16f, 0.04f), WorldMapSwatch.Lantern);
                b.Box(new Vector3(0f, 0.6f, -0.44f), new Vector3(1.0f, 0.03f, 0.02f), WorldMapSwatch.Turquoise);
            }
            else
            {
                b.Box(new Vector3(-0.15f, 0.27f, -0.43f), new Vector3(0.24f, 0.4f, 0.04f), WorldMapSwatch.WoodDark);
                b.Box(new Vector3(0.24f, 0.44f, -0.43f), new Vector3(0.18f, 0.16f, 0.04f), WorldMapSwatch.Lantern);
            }

            b.Matrix = saved;
        }

        /// <summary>The Vila da Vó Edna landmark: two adobe houses (one with its iwan), poplars behind, the apricot, a low adobe wall and the tandir.</summary>
        private static void AppendVillage(WorldMapMeshBuilder b)
        {
            AppendAdobeHouse(b, new Vector3(0.1f, 0f, 0.15f), -8f, 1f, WorldMapSwatch.Adobe, WorldMapSwatch.SandRock, iwan: true);
            AppendAdobeHouse(b, new Vector3(-0.85f, 0f, 0.75f), 24f, 0.75f, WorldMapSwatch.Adobe, WorldMapSwatch.SandRock, iwan: false);
            AppendPoplar(b, new Vector3(0.85f, 0f, 0.75f), 1.9f, false);
            AppendPoplar(b, new Vector3(-0.2f, 0f, 1.05f), 1.6f, true);
            AppendApricot(b, new Vector3(0.95f, 0f, -0.2f), 0.6f);
            // Low adobe garden wall (duval) in front, open at the path.
            b.Box(new Vector3(-0.75f, 0.1f, -0.75f), new Vector3(0.7f, 0.2f, 0.1f), WorldMapSwatch.Adobe, top: WorldMapSwatch.AdobeDark);
            b.Box(new Vector3(0.45f, 0.1f, -0.75f), new Vector3(0.35f, 0.2f, 0.1f), WorldMapSwatch.Adobe, top: WorldMapSwatch.AdobeDark);
            // Tandir: the round clay oven.
            b.Cylinder(new Vector3(0.85f, 0f, -0.6f), 0.16f, 0.1f, 0.22f, 7, WorldMapSwatch.AdobeDark, WorldMapSwatch.Black);
        }

        private static void AppendRuin(WorldMapMeshBuilder b)
        {
            b.Box(new Vector3(0f, 0.1f, 0f), new Vector3(2.1f, 0.2f, 1.7f), WorldMapSwatch.RuinDark);
            b.Box(new Vector3(0f, 0.26f, 0.1f), new Vector3(1.7f, 0.12f, 1.3f), WorldMapSwatch.Ruin);

            // Columns (one broken), an arch lintel over the back two.
            Vector3[] columns = { new Vector3(-0.65f, 0.32f, 0.45f), new Vector3(0.65f, 0.32f, 0.45f), new Vector3(-0.65f, 0.32f, -0.35f), new Vector3(0.65f, 0.32f, -0.35f) };
            float[] heights = { 1.35f, 1.35f, 0.75f, 0.4f };
            for (int i = 0; i < columns.Length; i++)
            {
                b.Cylinder(columns[i], 0.17f, 0.15f, heights[i], 6, WorldMapSwatch.Ruin, WorldMapSwatch.RuinDark, i * 11f);
                b.Box(columns[i] + new Vector3(0f, 0.04f, 0f), new Vector3(0.42f, 0.08f, 0.42f), WorldMapSwatch.RuinDark);
            }

            b.Box(new Vector3(0f, 1.75f, 0.45f), new Vector3(1.75f, 0.22f, 0.4f), WorldMapSwatch.Ruin, top: WorldMapSwatch.Sand);
            // Fallen column drum and rubble.
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(new Vector3(0.2f, 0.45f, -0.55f), Quaternion.Euler(0f, 20f, 90f), Vector3.one);
            b.Cylinder(new Vector3(0f, -0.35f, 0f), 0.15f, 0.15f, 0.7f, 6, WorldMapSwatch.Ruin);
            b.Matrix = saved;
            b.Box(new Vector3(-1.05f, 0.12f, -0.6f), new Vector3(0.3f, 0.24f, 0.26f), WorldMapSwatch.RuinDark, 30f);
            b.Box(new Vector3(1.1f, 0.1f, 0.2f), new Vector3(0.24f, 0.2f, 0.3f), WorldMapSwatch.Ruin, -15f);

            // A small steep pyramid of the Artificers (Meroe style) behind, with its little chapel in front.
            b.Cylinder(new Vector3(1.05f, 0.18f, 0.75f), 0.45f, 0f, 1.35f, 4, WorldMapSwatch.RuinDark, yaw: 45f);
            b.Box(new Vector3(1.05f, 0.32f, 0.3f), new Vector3(0.34f, 0.3f, 0.22f), WorldMapSwatch.Ruin, top: WorldMapSwatch.Sand);
            b.Box(new Vector3(1.05f, 0.28f, 0.185f), new Vector3(0.12f, 0.2f, 0.02f), WorldMapSwatch.Black);
        }

        private void AppendPeakCamp(WorldMapMeshBuilder b)
        {
            AppendPeak(b, new Vector3(0.25f, 0f, 0.45f), 1.05f, 2.1f, 3);
            AppendPeak(b, new Vector3(-0.75f, 0f, 0.7f), 0.7f, 1.35f, 5);
            // Warlord camp: a tent, a banner and a campfire in front of the peak.
            b.Cylinder(new Vector3(-0.55f, 0f, -0.35f), 0.5f, 0f, 0.85f, 6, WorldMapSwatch.Banner, yaw: 15f);
            b.Box(new Vector3(-0.55f, 0.18f, -0.78f), new Vector3(0.18f, 0.34f, 0.04f), WorldMapSwatch.Black);
            b.Cylinder(new Vector3(0.75f, 0f, -0.45f), 0.035f, 0.035f, 1.3f, 4, WorldMapSwatch.WoodDark);
            b.Card(new Vector3(0.77f, 0.85f, -0.45f), new Vector3(0.77f, 1.25f, -0.45f), new Vector3(1.2f, 1.2f, -0.45f), new Vector3(1.2f, 0.9f, -0.45f),
                WorldMapSwatch.Banner, new Vector3(0f, 0.5f, -1f));
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                b.Box(new Vector3(0.25f + Mathf.Cos(a) * 0.2f, 0.04f, -0.75f + Mathf.Sin(a) * 0.2f), new Vector3(0.12f, 0.08f, 0.1f), WorldMapSwatch.RockDark, i * 30f);
            }

            b.Cylinder(new Vector3(0.25f, 0f, -0.75f), 0.12f, 0f, 0.3f, 5, WorldMapSwatch.Lantern);
        }

        private void AppendCaveMouth(WorldMapMeshBuilder b)
        {
            b.AddMesh(Rock(1), Matrix4x4.TRS(new Vector3(0f, 0.25f, 0.25f), Quaternion.identity, new Vector3(1.35f, 1.55f, 1.1f)), WorldMapSwatch.CaveStone, WorldMapSwatch.CaveDark);
            b.AddMesh(Rock(3), Matrix4x4.TRS(new Vector3(-0.95f, 0.1f, 0.05f), Quaternion.identity, new Vector3(0.6f, 0.75f, 0.6f)), WorldMapSwatch.CaveStone);
            b.AddMesh(Rock(2), Matrix4x4.TRS(new Vector3(0.95f, 0.1f, 0.1f), Quaternion.identity, new Vector3(0.55f, 0.65f, 0.55f)), WorldMapSwatch.CaveStone);

            // Dark opening facing the camera, with a stone lintel.
            b.Box(new Vector3(0f, 0.42f, -1.0f), new Vector3(0.95f, 0.84f, 0.3f), WorldMapSwatch.Black);
            b.Box(new Vector3(0f, 1.0f, -1.12f), new Vector3(1.25f, 0.2f, 0.25f), WorldMapSwatch.CaveStone, top: WorldMapSwatch.CaveDark);
            b.Box(new Vector3(-0.58f, 0.45f, -1.12f), new Vector3(0.22f, 0.9f, 0.22f), WorldMapSwatch.CaveStone);
            b.Box(new Vector3(0.58f, 0.45f, -1.12f), new Vector3(0.22f, 0.9f, 0.22f), WorldMapSwatch.CaveStone);

            // Stalagmites and crystals flanking the entrance.
            b.Cylinder(new Vector3(-1.25f, 0f, -0.55f), 0.22f, 0f, 0.9f, 6, WorldMapSwatch.CaveStone);
            b.Cylinder(new Vector3(1.3f, 0f, -0.45f), 0.18f, 0f, 0.7f, 6, WorldMapSwatch.CaveStone);
            AppendCrystals(b, new Vector3(1.0f, 0f, -1.2f), 0.7f, 2);
            AppendCrystals(b, new Vector3(-0.95f, 0f, -1.25f), 0.55f, 5);
        }

        /// <summary>Grey fog puffs veiling a locked region's landmark (and the node's plaza edge).</summary>
        public static void AppendLockedFog(WorldMapMeshBuilder b, int seed)
        {
            var random = new System.Random(seed);
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f + (float)random.NextDouble() * 0.5f;
                float r = 0.5f + (float)random.NextDouble() * 0.9f;
                var at = new Vector3(Mathf.Cos(a) * r, 0.35f + (float)random.NextDouble() * 0.9f, Mathf.Sin(a) * r);
                float s = 0.9f + (float)random.NextDouble() * 0.7f;
                b.Blob(at, new Vector3(s * 1.3f, s * 0.8f, s * 1.1f), i % 3 == 0 ? WorldMapSwatch.CloudShade : WorldMapSwatch.Fog, i * 40f, coarse: true);
            }

            b.Blob(new Vector3(0f, 1.35f, 0.1f), new Vector3(1.8f, 1.1f, 1.5f), WorldMapSwatch.CloudShade, coarse: true);
        }

        /// <summary>Gold victory banner planted at a cleared region's landmark.</summary>
        public static void AppendClearedBanner(WorldMapMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.05f, 0.04f, 1.9f, 5, WorldMapSwatch.WoodDark);
            b.Blob(new Vector3(0f, 1.93f, 0f), new Vector3(0.12f, 0.12f, 0.12f), WorldMapSwatch.Gold);
            b.Card(new Vector3(0.04f, 1.2f, 0f), new Vector3(0.04f, 1.82f, 0f), new Vector3(0.7f, 1.72f, 0f), new Vector3(0.62f, 1.3f, 0f),
                WorldMapSwatch.Gold, new Vector3(0f, 0.5f, -1f));
            b.Card(new Vector3(0.04f, 1.2f, -0.005f), new Vector3(0.04f, 1.32f, -0.005f), new Vector3(0.63f, 1.4f, -0.005f), new Vector3(0.62f, 1.3f, -0.005f),
                WorldMapSwatch.Banner, new Vector3(0f, 0.5f, -1f));
        }

        // ---------------------------------------------------------------
        // Roads, bridge, sea, clouds
        // ---------------------------------------------------------------

        /// <summary>Wooden bridge, deck along local +x, top at <paramref name="deckTop"/>.</summary>
        public static void AppendBridge(WorldMapMeshBuilder b, float length, float width, float deckTop)
        {
            float half = length * 0.5f;
            int planks = Mathf.Max(3, Mathf.RoundToInt(length / 0.32f));
            float plank = length / planks;
            for (int i = 0; i < planks; i++)
            {
                float x = -half + plank * (i + 0.5f);
                b.Box(new Vector3(x, deckTop - 0.06f, 0f), new Vector3(plank * 0.9f, 0.12f, width), i % 2 == 0 ? WorldMapSwatch.Wood : WorldMapSwatch.WoodDark);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * (width * 0.5f + 0.02f);
                b.Box(new Vector3(0f, deckTop - 0.12f, z), new Vector3(length + 0.2f, 0.14f, 0.1f), WorldMapSwatch.WoodDark);
                for (int p = 0; p < 4; p++)
                {
                    float x = Mathf.Lerp(-half + 0.1f, half - 0.1f, p / 3f);
                    b.Box(new Vector3(x, deckTop + 0.2f, z), new Vector3(0.09f, 0.45f, 0.09f), WorldMapSwatch.WoodDark);
                }

                b.Box(new Vector3(0f, deckTop + 0.4f, z), new Vector3(length, 0.06f, 0.07f), WorldMapSwatch.Wood);
            }
        }

        /// <summary>Little flat stones lining a road, alternating sides; skips node plazas and the bridge.</summary>
        public static void AppendRoadStones(WorldMapMeshBuilder b, WorldMapTerrain terrain, int seed)
        {
            var random = new System.Random(unchecked(seed * 7 + 31));
            for (int i = 0; i < terrain.Paths.Count; i++)
            {
                WorldMapPath path = terrain.Paths[i];
                Vector2 dir = path.B - path.A;
                float length = dir.magnitude;
                if (length < 0.01f)
                {
                    continue;
                }

                dir /= length;
                var side = new Vector2(-dir.y, dir.x);
                int index = 0;
                for (float d = WorldMapTerrain.NodePlazaRadius + 0.8f; d < length - WorldMapTerrain.NodePlazaRadius - 0.8f; d += 1.1f, index++)
                {
                    Vector2 p = path.A + dir * d + side * ((index % 2 == 0 ? 1f : -1f) * (WorldMapTerrain.PathHalfWidth + 0.12f));
                    if (terrain.HasRiver && Vector2.Distance(p, terrain.BridgeCenter) < 2.2f || !terrain.IsDryLand(p))
                    {
                        continue;
                    }

                    float s = 0.8f + (float)random.NextDouble() * 0.5f;
                    float y = terrain.Height(p);
                    b.Blob(new Vector3(p.x, y + 0.02f, p.y), new Vector3(0.28f, 0.1f, 0.22f) * s, WorldMapSwatch.Pebble, (float)random.NextDouble() * 180f, coarse: true);
                }
            }
        }

        /// <summary>Open sea: a frame of flat quads around <paramref name="inner"/> (the ground mesh) out to <paramref name="outer"/>.</summary>
        public static void AppendSeaFrame(WorldMapMeshBuilder b, Rect inner, Rect outer, float y)
        {
            void AddRect(float x0, float z0, float x1, float z1)
            {
                if (x1 - x0 <= 0f || z1 - z0 <= 0f)
                {
                    return;
                }

                b.Quad(new Vector3(x0, y, z0), new Vector3(x0, y, z1), new Vector3(x1, y, z1), new Vector3(x1, y, z0), WorldMapSwatch.DeepWater);
            }

            AddRect(outer.xMin, outer.yMin, outer.xMax, inner.yMin);   // south
            AddRect(outer.xMin, inner.yMax, outer.xMax, outer.yMax);   // north
            AddRect(outer.xMin, inner.yMin, inner.xMin, inner.yMax);   // west
            AddRect(inner.xMax, inner.yMin, outer.xMax, inner.yMax);   // east
        }

        /// <summary>A puffy low-poly cloud bank (white tops, bluish undersides), pivot at its centre.</summary>
        public static void AppendCloud(WorldMapMeshBuilder b, int seed)
        {
            var random = new System.Random(seed);
            int puffs = 4 + random.Next(3);
            for (int i = 0; i < puffs; i++)
            {
                float x = (i - (puffs - 1) * 0.5f) * 0.9f + (float)(random.NextDouble() - 0.5) * 0.4f;
                float z = (float)(random.NextDouble() - 0.5) * 0.9f;
                float s = 1.1f + (float)random.NextDouble() * 0.9f - Mathf.Abs(i - (puffs - 1) * 0.5f) * 0.25f;
                b.Blob(new Vector3(x, s * 0.25f, z), new Vector3(s * 1.4f, s, s * 1.2f), WorldMapSwatch.Cloud, i * 50f, coarse: true);
                b.Blob(new Vector3(x, -0.05f, z), new Vector3(s * 1.3f, s * 0.35f, s * 1.1f), WorldMapSwatch.CloudShade, i * 50f, coarse: true);
            }
        }

        public void Dispose()
        {
            foreach (Mesh mesh in _rocks)
            {
                if (mesh == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(mesh);
                }
                else
                {
                    Object.DestroyImmediate(mesh);
                }
            }

            _rocks.Clear();
        }
    }
}
