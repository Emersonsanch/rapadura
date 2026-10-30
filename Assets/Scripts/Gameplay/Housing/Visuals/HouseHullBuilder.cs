using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>The hull meshes of one house level, split so the interior view can cut away the walls facing the camera.</summary>
    public sealed class HouseHullMeshes
    {
        /// <summary>Ground, yard, the adobe plinth (sufa), floor, iwan deck and steps, the cousin's apricot tree and the yard extras.</summary>
        public Mesh Base;
        /// <summary>Indexed by <see cref="HouseWallSide"/>.</summary>
        public Mesh[] Walls = new Mesh[4];
        /// <summary>Flat earth roof with its parapet, the iwan's columns and roof, windcatchers and rooftop pieces.</summary>
        public Mesh Roof;

        public int TriangleCount
        {
            get
            {
                int total = Count(Base) + Count(Roof);
                foreach (Mesh wall in Walls) total += Count(wall);
                return total;
            }
        }

        private static int Count(Mesh mesh) => mesh != null ? (int)(mesh.GetIndexCount(0) / 3) : 0;
    }

    /// <summary>
    /// Builds the player's house in the style of the Vila da Vó Edna (world bible §2.1: Sogdian adobe, flat roofs with a
    /// parapet, the iwan as the varanda, carved walnut columns, lattice windows, turquoise only as detail) from modules,
    /// one per <see cref="HouseHullSocket"/> and level (<c>HouseHullBuilder.Kits.cs</c>): N1 is a little raw-adobe house on
    /// an earth plinth with a pole-and-reed ayvan, N5 a plastered courtyard house with an L-shaped iwan, windcatchers and a
    /// rooftop pavilion. Local space: interior from (0, <see cref="FloorY"/>, 0) to (<c>width</c>, ·, <c>depth</c>) metres,
    /// walls built outwards from those faces, entrance on the south (−z); the yard lies in front of the porch
    /// (<see cref="YardOrigin"/>). Deterministic; every level stays under its triangle budget (§3.5, checked by a test).
    /// </summary>
    public static partial class HouseHullBuilder
    {
        public const float FloorY = 0.35f;
        public const float WallHeight = HouseLevelSpec.WallRows * HouseLevelSpec.InteriorCellSize;
        /// <summary>The N1 ayvan's depth (the yard origin of the first house depends on it).</summary>
        public const float PorchDepth = 1.5f;
        /// <summary>The N1 adobe wall thickness (outwards from the interior faces).</summary>
        public const float WallThickness = 0.18f;

        public static float InteriorWidth(HouseLevelSpec spec) => spec.InteriorWidth * HouseLevelSpec.InteriorCellSize;
        public static float InteriorDepth(HouseLevelSpec spec) => spec.InteriorDepth * HouseLevelSpec.InteriorCellSize;

        /// <summary>World-local min corner of the yard grid (x, z), centred on the house, in front of the porch.</summary>
        public static Vector2 YardOrigin(HouseLevelSpec spec)
        {
            float yardWidth = spec.YardWidth * HouseLevelSpec.YardCellSize;
            float yardDepth = spec.YardDepth * HouseLevelSpec.YardCellSize;
            return new Vector2(InteriorWidth(spec) * 0.5f - yardWidth * 0.5f, -PorchDepthOf(spec) - yardDepth);
        }

        // ------------------------------------------------------------------
        // Wall frame helpers
        // ------------------------------------------------------------------

        /// <summary>Wall origin, direction of its columns and outward normal (columns as in <see cref="HousePlacementRules"/>).</summary>
        public static void WallFrame(HouseLevelSpec spec, HouseWallSide side, out Vector3 origin, out Vector3 along, out Vector3 outward)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            switch (side)
            {
                case HouseWallSide.North:
                    origin = new Vector3(0f, FloorY, d); along = Vector3.right; outward = Vector3.forward; break;
                case HouseWallSide.West:
                    origin = new Vector3(0f, FloorY, 0f); along = Vector3.forward; outward = Vector3.left; break;
                case HouseWallSide.East:
                    origin = new Vector3(w, FloorY, 0f); along = Vector3.forward; outward = Vector3.right; break;
                default:
                    origin = new Vector3(0f, FloorY, 0f); along = Vector3.right; outward = Vector3.back; break;
            }
        }

        /// <summary>A point on a wall: <paramref name="u"/> metres along it, <paramref name="v"/> up from the floor, <paramref name="depth"/> outwards.</summary>
        public static Vector3 WallPoint(HouseLevelSpec spec, HouseWallSide side, float u, float v, float depth)
        {
            WallFrame(spec, side, out Vector3 origin, out Vector3 along, out Vector3 outward);
            return origin + along * u + Vector3.up * v + outward * depth;
        }

        /// <param name="caps">False for thin strips whose ends are always hidden: only the four sides are built.</param>
        private static void WallBox(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, float u0, float u1, float v0, float v1,
            float d0, float d1, HouseSwatch swatch, bool caps = true)
        {
            Vector3 p0 = WallPoint(spec, side, u0, v0, d0);
            Vector3 p1 = WallPoint(spec, side, u1, v1, d1);
            if (caps) b.BoxMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), swatch);
            else b.BoxSidesMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), swatch);
        }

        /// <summary>A flat triangle on a wall face at <paramref name="depth"/>, visible from both sides (arch heads).</summary>
        private static void WallTriangle(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, Vector2 a, Vector2 c, Vector2 apex, float depth, HouseSwatch swatch)
        {
            Vector3 pa = WallPoint(spec, side, a.x, a.y, depth);
            Vector3 pc = WallPoint(spec, side, c.x, c.y, depth);
            Vector3 pt = WallPoint(spec, side, apex.x, apex.y, depth);
            b.Triangle(pa, pt, pc, swatch);
            b.Triangle(pa, pc, pt, swatch);
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        /// <summary>Builds the hull of <paramref name="spec"/>'s level (N1 when null), one module per socket.</summary>
        public static HouseHullMeshes Build(HouseLevelSpec spec)
        {
            return BuildKit(spec ?? HouseLevels.Get(HouseLevels.MinLevel));
        }

        /// <summary>
        /// The cousin's tree at N1 (§2.1; the bible's orchard turns the old cajá into an apricot, §3.20): a seedling with a
        /// stake, a ring of stones and a little name tag; fixed, not an item.
        /// </summary>
        private static void BuildSeedling(HouseMeshBuilder b, HouseLevelSpec spec, Vector2 yard)
        {
            CellRect tree = spec.FixedTree;
            var center = new Vector3(yard.x + (tree.X + tree.Width * 0.5f) * HouseLevelSpec.YardCellSize, 0f,
                yard.y + (tree.Y + tree.Height * 0.5f) * HouseLevelSpec.YardCellSize);
            b.Cylinder(center, 0.5f, 0.45f, 0.04f, 8, HouseSwatch.Dirt);
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2f / 7f;
                b.Box(center + new Vector3(Mathf.Cos(a) * 0.55f, 0.05f, Mathf.Sin(a) * 0.55f), new Vector3(0.16f, 0.1f, 0.13f), HouseSwatch.Stone, i * 23f);
            }

            b.Cylinder(center, 0.05f, 0.035f, 0.75f, 5, HouseSwatch.WoodDark);
            b.Blob(center + new Vector3(0f, 0.85f, 0f), new Vector3(0.55f, 0.4f, 0.55f), HouseSwatch.Leaf);
            b.Blob(center + new Vector3(0.18f, 0.65f, 0.05f), new Vector3(0.32f, 0.25f, 0.3f), HouseSwatch.Leaf);
            b.Blob(center + new Vector3(-0.15f, 0.7f, -0.1f), new Vector3(0.3f, 0.22f, 0.3f), HouseSwatch.GrassDark);
            b.Box(center + new Vector3(0.12f, 0.45f, 0.12f), new Vector3(0.03f, 0.9f, 0.03f), HouseSwatch.WoodRaw);
            b.Box(center + new Vector3(0.35f, 0.25f, -0.35f), new Vector3(0.28f, 0.16f, 0.03f), HouseSwatch.Cream, 35f);
        }
    }
}
