using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>The hull meshes of one house level, split so the interior view can cut away the walls facing the camera.</summary>
    public sealed class HouseHullMeshes
    {
        /// <summary>Ground, yard, stilts, floor, porch deck and steps, the cajá seedling, varal and toco.</summary>
        public Mesh Base;
        /// <summary>Indexed by <see cref="HouseWallSide"/>.</summary>
        public Mesh[] Walls = new Mesh[4];
        /// <summary>Main gable, gable ends and the porch lean-to.</summary>
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
    /// Builds the N1 "Casa de madeira" (Housing.md §3.2, §3.3) from modules: log stilts, raw-board floor, vertical
    /// boards with dark battens (mata-junta), board door and a shuttered window with a flower box, patched zinc roof
    /// with buriti straw, a 1-log porch (alpendre) with steps, the yard with the cousin's cajá seedling, a clothes line
    /// and a stump. Local space: interior from (0, <see cref="FloorY"/>, 0) to (<c>width</c>, ·, <c>depth</c>) metres,
    /// entrance on the south (−z); the yard lies in front of the porch (<see cref="YardOrigin"/>). Deterministic; the
    /// whole hull stays well under the N1 budget of 2,500 triangles (§3.5, checked by a test). Later levels (H4) swap
    /// these modules per socket instead of the whole model (<see cref="HouseHullSocket"/>, <c>HouseHullBuilder.Kits.cs</c>).
    /// </summary>
    public static partial class HouseHullBuilder
    {
        public const float FloorY = 0.35f;
        public const float WallHeight = HouseLevelSpec.WallRows * HouseLevelSpec.InteriorCellSize;
        public const float PorchDepth = 1.5f;
        public const float BoardWidth = 0.25f;
        public const float WallThickness = 0.06f;
        public const float RidgeHeight = 1.1f;

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

        /// <param name="caps">False for boards and battens: their ends are always hidden, so only the four sides are built.</param>
        private static void WallBox(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, float u0, float u1, float v0, float v1,
            float d0, float d1, HouseSwatch swatch, bool caps = true)
        {
            Vector3 p0 = WallPoint(spec, side, u0, v0, d0);
            Vector3 p1 = WallPoint(spec, side, u1, v1, d1);
            if (caps) b.BoxMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), swatch);
            else b.BoxSidesMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), swatch);
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        public static HouseHullMeshes Build(HouseLevelSpec spec)
        {
            // N2–N5 swap the modules socket by socket (HouseHullBuilder.Kits.cs); N1 is the original kit below.
            if (spec != null && spec.Level > HouseLevels.MinLevel)
            {
                return BuildKit(spec);
            }

            var meshes = new HouseHullMeshes();
            var b = new HouseMeshBuilder();

            BuildBase(b, spec);
            meshes.Base = b.ToMesh("HouseN1_Base");

            foreach (HouseWallSide side in new[] { HouseWallSide.South, HouseWallSide.East, HouseWallSide.North, HouseWallSide.West })
            {
                b.Clear();
                BuildWall(b, spec, side);
                meshes.Walls[(int)side] = b.ToMesh("HouseN1_Wall_" + side);
            }

            b.Clear();
            BuildRoof(b, spec);
            meshes.Roof = b.ToMesh("HouseN1_Roof");
            return meshes;
        }

        private static void BuildBase(HouseMeshBuilder b, HouseLevelSpec spec)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            Vector2 yard = YardOrigin(spec);
            float yardW = spec.YardWidth * HouseLevelSpec.YardCellSize;
            float yardD = spec.YardDepth * HouseLevelSpec.YardCellSize;

            // Ground around everything, a slightly darker yard plot on top.
            b.BoxMinMax(new Vector3(yard.x - 3f, -0.2f, yard.y - 3f), new Vector3(yard.x + yardW + 3f, -0.02f, d + 3f), HouseSwatch.GrassDark, HouseSwatch.GrassDark);
            b.BoxMinMax(new Vector3(yard.x, -0.05f, yard.y), new Vector3(yard.x + yardW, 0f, yard.y + yardD), HouseSwatch.Grass);

            // Log stilts (estacas de tora).
            for (int ix = 0; ix <= 2; ix++)
            {
                for (int iz = 0; iz <= 1; iz++)
                {
                    var foot = new Vector3(0.1f + ix * (w - 0.2f) / 2f, -0.02f, 0.1f + iz * (d - 0.2f));
                    b.Cylinder(foot, 0.1f, 0.09f, FloorY - 0.06f, 6, HouseSwatch.WoodDark, HouseSwatch.WoodJoint);
                }
            }

            // Floor: raw boards along x, alternating tone (tábua crua).
            int planks = Mathf.RoundToInt(d / BoardWidth);
            for (int i = 0; i < planks; i++)
            {
                float z0 = i * BoardWidth;
                b.BoxMinMax(new Vector3(-WallThickness, FloorY - 0.08f, z0), new Vector3(w + WallThickness, FloorY, z0 + BoardWidth - 0.01f),
                    i % 2 == 0 ? HouseSwatch.FloorBoard : HouseSwatch.WoodRaw);
            }

            // Porch deck (alpendre) and its stilts.
            int porchPlanks = Mathf.RoundToInt(PorchDepth / BoardWidth);
            for (int i = 0; i < porchPlanks; i++)
            {
                float z1 = -i * BoardWidth;
                b.BoxMinMax(new Vector3(-0.05f, FloorY - 0.1f, z1 - BoardWidth + 0.02f), new Vector3(w + 0.05f, FloorY - 0.03f, z1), i % 2 == 0 ? HouseSwatch.WoodRaw : HouseSwatch.FloorBoard);
            }

            b.Cylinder(new Vector3(0.15f, -0.02f, -PorchDepth + 0.15f), 0.09f, 0.08f, FloorY - 0.08f, 6, HouseSwatch.WoodDark);
            b.Cylinder(new Vector3(w - 0.15f, -0.02f, -PorchDepth + 0.15f), 0.09f, 0.08f, FloorY - 0.08f, 6, HouseSwatch.WoodDark);

            // Steps in front of the door.
            HouseOpening door = spec.EntranceDoor;
            float doorU0 = door != null ? door.Start * HouseLevelSpec.InteriorCellSize : 1f;
            float doorU1 = door != null ? (door.Start + door.Width) * HouseLevelSpec.InteriorCellSize : 2f;
            b.BoxMinMax(new Vector3(doorU0 - 0.1f, 0.12f, -PorchDepth - 0.3f), new Vector3(doorU1 + 0.1f, 0.24f, -PorchDepth), HouseSwatch.WoodJoint, HouseSwatch.WoodRaw);
            b.BoxMinMax(new Vector3(doorU0 - 0.1f, -0.02f, -PorchDepth - 0.6f), new Vector3(doorU1 + 0.1f, 0.12f, -PorchDepth - 0.3f), HouseSwatch.WoodJoint, HouseSwatch.WoodRaw);

            // Beaten-earth path from the steps to the yard entrance.
            float pathX = yard.x + (spec.YardEntranceX + 0.5f) * HouseLevelSpec.YardCellSize;
            b.BoxMinMax(new Vector3(Mathf.Min(pathX, (doorU0 + doorU1) * 0.5f) - 0.45f, -0.01f, yard.y + yardD - 1f), new Vector3(Mathf.Max(pathX, (doorU0 + doorU1) * 0.5f) + 0.45f, 0.005f, -PorchDepth - 0.6f), HouseSwatch.Dirt);

            BuildCajaSeedling(b, spec, yard);

            // Extras N1: clothes line on the east side, a stump by the porch (outside the yard grid).
            b.Cylinder(new Vector3(w + 0.9f, -0.02f, 0.4f), 0.05f, 0.04f, 1.7f, 5, HouseSwatch.WoodDark);
            b.Cylinder(new Vector3(w + 0.9f, -0.02f, d - 0.4f), 0.05f, 0.04f, 1.7f, 5, HouseSwatch.WoodDark);
            b.BoxMinMax(new Vector3(w + 0.885f, 1.6f, 0.4f), new Vector3(w + 0.915f, 1.62f, d - 0.4f), HouseSwatch.White);
            b.BoxMinMax(new Vector3(w + 0.86f, 1.2f, 1.1f), new Vector3(w + 0.94f, 1.6f, 1.6f), HouseSwatch.ClothBlue);
            b.BoxMinMax(new Vector3(w + 0.86f, 1.3f, 2.0f), new Vector3(w + 0.94f, 1.6f, 2.4f), HouseSwatch.ClothYellow);
            b.Cylinder(new Vector3(-0.8f, -0.02f, -PorchDepth - 0.4f), 0.28f, 0.24f, 0.4f, 7, HouseSwatch.WoodDark, HouseSwatch.FloorBoard);
        }

        /// <summary>The cousin's cajá (§2.1): a seedling at N1 with a stake and a little ring of stones; fixed, not an item.</summary>
        private static void BuildCajaSeedling(HouseMeshBuilder b, HouseLevelSpec spec, Vector2 yard)
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

        private static void BuildWall(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side)
        {
            float length = spec.WallLength(side) * HouseLevelSpec.InteriorCellSize;
            float cell = HouseLevelSpec.InteriorCellSize;
            int boards = Mathf.RoundToInt(length / BoardWidth);

            for (int i = 0; i < boards; i++)
            {
                float u0 = i * BoardWidth;
                float u1 = u0 + BoardWidth;
                float vStart = 0f;
                // Vertical runs of this board between the openings it crosses.
                foreach (HouseOpening opening in spec.Openings)
                {
                    if (opening.Side != side) continue;
                    float ou0 = opening.Start * cell, ou1 = (opening.Start + opening.Width) * cell;
                    if (u1 <= ou0 + 1e-4f || u0 >= ou1 - 1e-4f) continue;
                    float ov0 = opening.RowStart * cell, ov1 = (opening.RowStart + opening.Rows) * cell;
                    if (ov0 > vStart) WallBox(b, spec, side, u0, u1, vStart, ov0, 0f, WallThickness, BoardSwatch(i), caps: false);
                    vStart = Mathf.Max(vStart, ov1);
                }

                if (vStart < WallHeight) WallBox(b, spec, side, u0, u1, vStart, WallHeight, 0f, WallThickness, BoardSwatch(i), caps: false);

                // Mata-junta on every other joint, outside, same gaps.
                if (i % 2 == 1 && i < boards - 1)
                {
                    float vb = 0f;
                    foreach (HouseOpening opening in spec.Openings)
                    {
                        if (opening.Side != side) continue;
                        float ou0 = opening.Start * cell, ou1 = (opening.Start + opening.Width) * cell;
                        if (u1 < ou0 - 1e-4f || u1 > ou1 + 1e-4f) continue;
                        float ov0 = opening.RowStart * cell, ov1 = (opening.RowStart + opening.Rows) * cell;
                        if (ov0 > vb) WallBox(b, spec, side, u1 - 0.03f, u1 + 0.03f, vb, ov0, WallThickness, WallThickness + 0.025f, HouseSwatch.WoodJoint, caps: false);
                        vb = Mathf.Max(vb, ov1);
                    }

                    if (vb < WallHeight) WallBox(b, spec, side, u1 - 0.03f, u1 + 0.03f, vb, WallHeight, WallThickness, WallThickness + 0.025f, HouseSwatch.WoodJoint, caps: false);
                }
            }

            // Sill and top plate logs (corner posts on the east/west walls).
            WallBox(b, spec, side, -WallThickness, length + WallThickness, -0.08f, 0.04f, 0f, WallThickness + 0.04f, HouseSwatch.WoodDark);
            WallBox(b, spec, side, -WallThickness, length + WallThickness, WallHeight - 0.1f, WallHeight, 0f, WallThickness + 0.04f, HouseSwatch.WoodDark);
            if (side == HouseWallSide.East || side == HouseWallSide.West)
            {
                WallBox(b, spec, side, -0.1f, 0.02f, -0.08f, WallHeight, -0.02f, WallThickness + 0.06f, HouseSwatch.WoodDark);
                WallBox(b, spec, side, length - 0.02f, length + 0.1f, -0.08f, WallHeight, -0.02f, WallThickness + 0.06f, HouseSwatch.WoodDark);
            }

            foreach (HouseOpening opening in spec.Openings)
            {
                if (opening.Side != side) continue;
                float u0 = opening.Start * cell, u1 = (opening.Start + opening.Width) * cell;
                float v0 = opening.RowStart * cell, v1 = (opening.RowStart + opening.Rows) * cell;

                // Frame.
                WallBox(b, spec, side, u0 - 0.06f, u0, v0, v1 + 0.06f, -0.01f, WallThickness + 0.04f, HouseSwatch.WoodDark);
                WallBox(b, spec, side, u1, u1 + 0.06f, v0, v1 + 0.06f, -0.01f, WallThickness + 0.04f, HouseSwatch.WoodDark);
                WallBox(b, spec, side, u0 - 0.06f, u1 + 0.06f, v1, v1 + 0.08f, -0.01f, WallThickness + 0.04f, HouseSwatch.WoodDark);

                if (opening.IsDoor)
                {
                    // Board door swung open inwards against the wall.
                    WallBox(b, spec, side, u1 + 0.02f, u1 + 0.07f, 0.02f, v1 - 0.04f, -(u1 - u0) - 0.02f, -0.02f, HouseSwatch.WoodRaw);
                    WallBox(b, spec, side, u1 + 0.07f, u1 + 0.1f, v1 * 0.45f, v1 * 0.5f, -(u1 - u0) * 0.8f, -(u1 - u0) * 0.6f, HouseSwatch.Iron);
                }
                else
                {
                    WallBox(b, spec, side, u0 - 0.06f, u1 + 0.06f, v0 - 0.06f, v0, -0.03f, WallThickness + 0.08f, HouseSwatch.WoodDark);
                    // Open shutters (janela de tábua com tramela) and the flower box (§3.3: "tem flor na janela").
                    WallBox(b, spec, side, u0 - 0.06f - (u1 - u0) * 0.5f, u0 - 0.06f, v0, v1, WallThickness + 0.02f, WallThickness + 0.05f, HouseSwatch.WoodRaw);
                    WallBox(b, spec, side, u1 + 0.06f, u1 + 0.06f + (u1 - u0) * 0.5f, v0, v1, WallThickness + 0.02f, WallThickness + 0.05f, HouseSwatch.WoodRaw);
                    WallBox(b, spec, side, u0, u1, v0 - 0.2f, v0 - 0.04f, WallThickness + 0.02f, WallThickness + 0.2f, HouseSwatch.Clay);
                    for (int f = 0; f < 3; f++)
                    {
                        Vector3 flower = WallPoint(spec, side, u0 + (u1 - u0) * (0.2f + 0.3f * f), v0 + 0.02f, WallThickness + 0.11f);
                        b.Blob(flower, new Vector3(0.14f, 0.12f, 0.14f), f == 1 ? HouseSwatch.ClothYellow : HouseSwatch.Flower);
                        b.Blob(flower + Vector3.down * 0.06f, new Vector3(0.18f, 0.08f, 0.18f), HouseSwatch.Leaf);
                    }
                }
            }
        }

        private static HouseSwatch BoardSwatch(int i) => i % 5 == 3 ? HouseSwatch.FloorBoard : HouseSwatch.WoodRaw;

        private static void BuildRoof(HouseMeshBuilder b, HouseLevelSpec spec)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float top = FloorY + WallHeight;
            const float overhang = 0.35f;

            // Patched zinc (zinco remendado) on the main gable...
            b.GableRoof(-overhang, w + overhang, -overhang, d + overhang, top, RidgeHeight, 0.05f, HouseSwatch.Zinc, HouseSwatch.WoodJoint);
            b.Log(new Vector3(-overhang - 0.05f, top + RidgeHeight + 0.02f, d * 0.5f), new Vector3(w + overhang + 0.05f, top + RidgeHeight + 0.02f, d * 0.5f), 0.07f, HouseSwatch.ZincDark);

            // ...with buriti straw patches and a darker replaced sheet on the south slope.
            float slope = Mathf.Atan2(RidgeHeight, d * 0.5f + overhang) * Mathf.Rad2Deg;
            PatchOnSlope(b, w * 0.22f, 0.6f, 0.9f, 0.7f, top, d, overhang, slope, HouseSwatch.Straw);
            PatchOnSlope(b, w * 0.7f, 1.2f, 0.7f, 0.5f, top, d, overhang, slope, HouseSwatch.Straw);
            PatchOnSlope(b, w * 0.45f, 0.3f, 0.8f, 0.6f, top, d, overhang, slope, HouseSwatch.ZincDark);

            // Gable ends (boards), both faces.
            foreach (float x in new[] { 0f, w })
            {
                var a = new Vector3(x, top, -0.02f);
                var c = new Vector3(x, top, d + 0.02f);
                var r = new Vector3(x, top + RidgeHeight - 0.05f, d * 0.5f);
                b.Triangle(a, r, c, HouseSwatch.WoodRaw);
                b.Triangle(a, c, r, HouseSwatch.WoodRaw);
            }

            // Porch: one log beam on two posts (alpendre de 1 tora) under a zinc lean-to.
            float beamY = FloorY + 2.0f;
            b.Cylinder(new Vector3(0.15f, FloorY - 0.05f, -PorchDepth + 0.15f), 0.07f, 0.06f, beamY - FloorY + 0.05f, 6, HouseSwatch.WoodDark);
            b.Cylinder(new Vector3(w - 0.15f, FloorY - 0.05f, -PorchDepth + 0.15f), 0.07f, 0.06f, beamY - FloorY + 0.05f, 6, HouseSwatch.WoodDark);
            b.Log(new Vector3(-0.1f, beamY + 0.06f, -PorchDepth + 0.15f), new Vector3(w + 0.1f, beamY + 0.06f, -PorchDepth + 0.15f), 0.08f, HouseSwatch.WoodDark, HouseSwatch.FloorBoard);
            var l0 = new Vector3(-0.15f, top - 0.08f, -overhang + 0.05f);
            var l1 = new Vector3(w + 0.15f, top - 0.08f, -overhang + 0.05f);
            var l2 = new Vector3(w + 0.15f, beamY + 0.12f, -PorchDepth - 0.15f);
            var l3 = new Vector3(-0.15f, beamY + 0.12f, -PorchDepth - 0.15f);
            b.Quad(l0, l1, l2, l3, HouseSwatch.Zinc);
            b.Quad(l0, l3, l2, l1, HouseSwatch.WoodJoint);
        }

        private static void PatchOnSlope(HouseMeshBuilder b, float x, float fromEave, float width, float depth, float top, float d, float overhang, float slopeDeg, HouseSwatch swatch)
        {
            float run = d * 0.5f + overhang;
            float t = Mathf.Clamp01((fromEave + depth * 0.5f) / Mathf.Sqrt(run * run + RidgeHeight * RidgeHeight));
            var center = new Vector3(x, top + RidgeHeight * t + 0.03f, -overhang + run * t);
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(center, Quaternion.Euler(-slopeDeg, 0f, 0f), Vector3.one);
            b.Box(Vector3.zero, new Vector3(width, 0.04f, depth), swatch);
            b.Matrix = saved;
        }
    }
}
