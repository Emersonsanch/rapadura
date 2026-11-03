using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Mingcheng, the city of nine rings (Planície de Mingcheng, Docs/Mapas100.md §2.1 and map 41, style
    /// <c>RingWall</c>): the courtyard halls of the walled cities (<see cref="RingWallHallHouse"/>: grey brick on a stone base,
    /// red-lacquered columns and lattice windows, a curved grey-tile roof with upturned eaves, paper lanterns under the eaves),
    /// the ring wall of the 6th ring with the roofs of the inner rings behind it and the closed three-arch gate of the 7th
    /// (the citadel, map 46), the Torre do Tambor e do Sino on the plaza (the city's clock: the drum by day, the bell by night),
    /// the humpback stone bridges over the canals, the willows, the lantern frames of the Festa das Lanternas and the
    /// three-arch gate at the streets' ends; Mōzak's lanterns. Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (row 14 holds grey brick, lacquer, golden tile, jade, lantern paper, roof tile, willow and
    /// canal water), low-poly, deterministic. The lanterns are the Festa das Lanternas (Yuanxiao) of the ficha: riddles and
    /// light, no rite.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of a <see cref="CityPropKind.RingWall"/>'s foot: its collider is the run inflated by this.</summary>
        public const float RingWallHalfThickness = 0.6f;

        /// <summary>Height of the ring wall (× the prop's scale).</summary>
        public const float RingWallHeight = 4.0f;

        /// <summary>Radius of the <see cref="CityPropKind.BellDrumTower"/>'s brick base (× its scale): its collider.</summary>
        public const float BellDrumTowerRadius = 1.45f;

        /// <summary>Radius of a <see cref="CityPropKind.Willow"/>'s brick ring (× its scale): its collider.</summary>
        public const float WillowRadius = 0.5f;

        /// <summary>Half the width of the water of a <see cref="CityPropKind.CanalBridge"/>.</summary>
        public const float CanalHalfWidth = 0.55f;

        /// <summary>Footprint of a <see cref="CityPropKind.LanternFrame"/> (local x along the bar, z across; × its scale): its collider.</summary>
        public static readonly Vector2 LanternFrameSize = new Vector2(1.8f, 0.6f);

        // ------------------------------------------------------------------
        // The courtyard hall
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.RingWallHall"/> (local frame, front −z): a stone base, grey-brick walls with a dark
        /// footing course, a lacquered frieze with a painted band under the eaves, a double door of red lacquer with gold studs,
        /// lattice windows lit from inside, a curved roof (<see cref="CurvedRoof"/>) of <see cref="TownBuildingSpec.RoofColor"/>
        /// over the hall and its porch with upturned corners and ridge ornaments; a flagstone deck in front with red columns (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) and gilded bracket sets, two paper
        /// lanterns hanging between them, the sign as a lacquered plaque over the door.
        /// </summary>
        public static void RingWallHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float floorY = TownBuildingBuilder.FloorY;
            float storey = Mathf.Max(2.4f, spec.WallHeight);
            float wallTop = floorY + storey;
            TownSwatch lacquer = spec.JoineryColor;

            // Stone base, the brick walls and their footing course.
            b.BoxMinMax(new Vector3(-hw - 0.1f, 0f, zFacade - 0.1f), new Vector3(hw + 0.1f, floorY, zBack + 0.1f), spec.PlinthColor, TownSwatch.Paving);
            b.BoxMinMax(new Vector3(-hw, floorY, zFacade), new Vector3(hw, wallTop, zBack), spec.WallColor);
            b.BoxMinMax(new Vector3(-hw - 0.03f, floorY, zFacade - 0.03f), new Vector3(hw + 0.03f, floorY + 0.45f, zBack + 0.03f), TownSwatch.StoneDark);

            // The lacquered frieze and the painted band (jade or gold) under the eaves.
            b.BoxMinMax(new Vector3(-hw - 0.04f, wallTop - 0.42f, zFacade - 0.05f), new Vector3(hw + 0.04f, wallTop, zBack + 0.04f), lacquer);
            b.BoxMinMax(new Vector3(-hw - 0.05f, wallTop - 0.3f, zFacade - 0.06f), new Vector3(hw + 0.05f, wallTop - 0.2f, zFacade - 0.04f), spec.AccentColor);

            // The double door of red lacquer, its frame and two rows of gold studs.
            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.6f : 1.2f;
            float doorTop = floorY + Mathf.Min(2.15f, storey - 0.5f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.12f, floorY, zFacade - 0.07f), new Vector3(doorX + doorW * 0.5f + 0.12f, doorTop + 0.14f, zFacade), TownSwatch.WoodDark);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, floorY, zFacade - 0.09f), new Vector3(doorX + doorW * 0.5f, doorTop, zFacade - 0.07f), TownSwatch.Lacquer);
            b.BoxMinMax(new Vector3(doorX - 0.02f, floorY, zFacade - 0.1f), new Vector3(doorX + 0.02f, doorTop, zFacade - 0.09f), TownSwatch.Black);
            foreach (float y in new[] { floorY + 0.9f, floorY + 1.5f })
            {
                foreach (float t in new[] { -0.3f, 0.3f })
                {
                    b.Box(new Vector3(doorX + t * doorW, y, zFacade - 0.11f), new Vector3(0.07f, 0.07f, 0.03f), TownSwatch.GoldTile, 0f);
                }
            }

            // Lattice windows on both sides of the door, lit from inside.
            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? (doorX > 0.01f ? -1f : 1f) : (doorX > 0.01f ? 1f : -1f);
                float x = side * hw * 0.62f;
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.75f)
                {
                    continue;
                }

                float y0 = floorY + 1.0f;
                float y1 = floorY + 1.85f;
                b.BoxMinMax(new Vector3(x - 0.45f, y0, zFacade - 0.03f), new Vector3(x + 0.45f, y1, zFacade - 0.01f), TownSwatch.WindowLit);
                b.BoxMinMax(new Vector3(x - 0.53f, y0 - 0.08f, zFacade - 0.07f), new Vector3(x + 0.53f, y0, zFacade), lacquer);
                b.BoxMinMax(new Vector3(x - 0.53f, y1, zFacade - 0.07f), new Vector3(x + 0.53f, y1 + 0.08f, zFacade), lacquer);
                foreach (float lx in new[] { -0.15f, 0.15f })
                {
                    b.BoxMinMax(new Vector3(x + lx - 0.025f, y0, zFacade - 0.05f), new Vector3(x + lx + 0.025f, y1, zFacade - 0.03f), lacquer);
                }

                b.BoxMinMax(new Vector3(x - 0.45f, (y0 + y1) * 0.5f - 0.025f, zFacade - 0.05f), new Vector3(x + 0.45f, (y0 + y1) * 0.5f + 0.025f, zFacade - 0.03f), lacquer);
            }

            // The curved roof over the hall and the porch.
            float zEave = spec.PorchDepth > 0.01f ? zFront - 0.3f : zFacade - 0.5f;
            float rise = Mathf.Clamp((zBack - zEave) * 0.3f, 1.1f, 1.9f);
            CurvedRoof(b, -hw - 0.45f, hw + 0.45f, zEave, zBack + 0.5f, wallTop + 0.05f, rise, spec.RoofColor, lacquer, spec.WallColor, -hw, hw, zFacade, zBack);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.StoneDark, TownSwatch.Paving);

                float z = zFront + TownBuildingBuilder.PostInset;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + 0.1f, z), new Vector3(0.34f, 0.2f, 0.34f), TownSwatch.StoneDark, 0f);
                    b.Log(new Vector3(x, deck + 0.2f, z), new Vector3(x, wallTop - 0.2f, z), 0.11f, TownSwatch.Lacquer, 6);
                    b.Box(new Vector3(x, wallTop - 0.12f, z), new Vector3(0.42f, 0.16f, 0.42f), TownSwatch.GoldTile, 0f);
                }

                b.BoxMinMax(new Vector3(-hw - 0.05f, wallTop - 0.42f, z - 0.12f), new Vector3(hw + 0.05f, wallTop - 0.04f, z + 0.12f), lacquer);
                b.BoxMinMax(new Vector3(-hw - 0.06f, wallTop - 0.3f, z - 0.14f), new Vector3(hw + 0.06f, wallTop - 0.2f, z - 0.12f), spec.AccentColor);

                // Two paper lanterns between the columns.
                foreach (float t in new[] { -0.55f, 0.55f })
                {
                    float lx = t * hw;
                    if (Mathf.Abs(lx - doorX) < 0.5f)
                    {
                        lx += t > 0f ? 0.6f : -0.6f;
                    }

                    PaperLantern(b, new Vector3(lx, wallTop - 0.42f, z), 0.35f, t > 0f);
                }
            }

            if (spec.Sign)
            {
                var at = new Vector3(doorX, doorTop + 0.42f, zFacade - 0.08f);
                b.Box(at, new Vector3(1.35f, 0.4f, 0.06f), TownSwatch.Lacquer, 0f);
                b.Box(at + new Vector3(0f, 0f, -0.035f), new Vector3(1.15f, 0.28f, 0.02f), spec.SignColor, 0f);
                b.Box(at + new Vector3(0f, 0f, -0.05f), new Vector3(0.3f, 0.14f, 0.02f), TownSwatch.Black, 0f);
            }
        }

        /// <summary>
        /// A curved tile roof (local frame): from the eave at <paramref name="zFront"/> up to the ridge halfway and down to
        /// <paramref name="zBack"/>, each slope in two slabs (the lower one flatter, so the eave flares out), the gables filled
        /// with <paramref name="gable"/> between <paramref name="gx0"/>/<paramref name="gx1"/> over the wall from
        /// <paramref name="gzFront"/> to <paramref name="gzBack"/>, a round ridge with its end ornaments and the four corners
        /// turned up.
        /// </summary>
        private static void CurvedRoof(TownMeshBuilder b, float x0, float x1, float zFront, float zBack, float yEave, float rise, TownSwatch roof, TownSwatch under,
            TownSwatch gable, float gx0, float gx1, float gzFront, float gzBack)
        {
            float zMid = (zFront + zBack) * 0.5f;
            float ridgeY = yEave + rise;
            float zf = zFront + (zMid - zFront) * 0.38f;
            float zb = zBack - (zBack - zMid) * 0.38f;
            float yk = yEave + rise * 0.22f;
            const float thick = 0.12f;

            b.Slab(new Vector3(x0, yEave, zFront), new Vector3(x0, yk, zf), new Vector3(x1, yk, zf), new Vector3(x1, yEave, zFront), thick, roof, under);
            b.Slab(new Vector3(x0, yk, zf), new Vector3(x0, ridgeY, zMid), new Vector3(x1, ridgeY, zMid), new Vector3(x1, yk, zf), thick, roof, under);
            b.Slab(new Vector3(x0, ridgeY, zMid), new Vector3(x0, yk, zb), new Vector3(x1, yk, zb), new Vector3(x1, ridgeY, zMid), thick, roof, under);
            b.Slab(new Vector3(x0, yk, zb), new Vector3(x0, yEave, zBack), new Vector3(x1, yEave, zBack), new Vector3(x1, yk, zb), thick, roof, under);

            if (gx1 > gx0)
            {
                float gy = yEave - 0.05f;
                float gTop = ridgeY - 0.2f;
                b.TriangleFacing(new Vector3(gx0, gy, gzFront), new Vector3(gx0, gTop, zMid), new Vector3(gx0, gy, gzBack), Vector3.left, gable);
                b.TriangleFacing(new Vector3(gx1, gy, gzFront), new Vector3(gx1, gTop, zMid), new Vector3(gx1, gy, gzBack), Vector3.right, gable);
            }

            b.Log(new Vector3(x0 + 0.15f, ridgeY + 0.04f, zMid), new Vector3(x1 - 0.15f, ridgeY + 0.04f, zMid), 0.11f, roof, 4, under);
            foreach (float x in new[] { x0 + 0.2f, x1 - 0.2f })
            {
                float o = x < (x0 + x1) * 0.5f ? -1f : 1f;
                b.Log(new Vector3(x, ridgeY + 0.08f, zMid), new Vector3(x + o * 0.12f, ridgeY + 0.5f, zMid), 0.09f, roof, 4, TownSwatch.Jade);
            }

            foreach (Vector2 c in new[] { new Vector2(x0, zFront), new Vector2(x1, zFront), new Vector2(x0, zBack), new Vector2(x1, zBack) })
            {
                float sx = c.x < (x0 + x1) * 0.5f ? -1f : 1f;
                float sz = c.y < zMid ? -1f : 1f;
                var p = new Vector3(c.x, yEave - 0.02f, c.y);
                b.Log(p, p + new Vector3(sx * 0.28f, 0.32f, sz * 0.28f), 0.06f, roof, 4);
            }
        }

        /// <summary>A paper lantern hanging on a cord from <paramref name="hook"/>: a glowing paper body with a red (or gold) band and caps.</summary>
        private static void PaperLantern(TownMeshBuilder b, Vector3 hook, float size, bool red)
        {
            Vector3 c = hook + Vector3.down * (0.25f + size * 0.6f);
            b.Log(hook, c + Vector3.up * size * 0.55f, 0.012f, TownSwatch.Black, 3);
            b.Blob(c, new Vector3(size, size * 1.2f, size), TownSwatch.LanternPaper);
            b.Box(c, new Vector3(size * 0.82f, size * 0.22f, size * 0.82f), red ? TownSwatch.Lacquer : TownSwatch.GoldTile, 45f);
            b.Box(c + Vector3.up * size * 0.62f, new Vector3(size * 0.4f, size * 0.12f, size * 0.4f), TownSwatch.Lacquer, 0f);
            b.Box(c + Vector3.down * size * 0.62f, new Vector3(size * 0.34f, size * 0.1f, size * 0.34f), TownSwatch.Lacquer, 0f);
        }

        /// <summary>
        /// The three-arch gate of Mingcheng at a street's end: four red-lacquered pillars on stone feet (the middle bay higher),
        /// painted lintels, a curved tile roof over each bay, the gilded name plaque in the middle and two paper lanterns.
        /// </summary>
        public static void TripleArchGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                float[] xs = { 0f, w / 3f, w * 2f / 3f, w };
                for (int i = 0; i < xs.Length; i++)
                {
                    bool inner = i == 1 || i == 2;
                    float h = inner ? 4.0f : 3.3f;
                    b.Box(new Vector3(xs[i], 0.25f, 0f), new Vector3(0.6f, 0.5f, 0.6f), TownSwatch.StoneDark, 0f, TownSwatch.Paving);
                    b.Log(new Vector3(xs[i], 0.5f, 0f), new Vector3(xs[i], h, 0f), 0.17f, TownSwatch.Lacquer, 6);
                }

                // Lintels: the outer bays lower, the middle bay higher, a jade band under each.
                for (int bay = 0; bay < 3; bay++)
                {
                    bool middle = bay == 1;
                    float y = middle ? 3.7f : 3.0f;
                    float xa = xs[bay] - (bay == 0 ? 0.25f : 0f);
                    float xb = xs[bay + 1] + (bay == 2 ? 0.25f : 0f);
                    b.BoxMinMax(new Vector3(xa, y, -0.16f), new Vector3(xb, y + 0.3f, 0.16f), TownSwatch.Lacquer);
                    b.BoxMinMax(new Vector3(xa + 0.1f, y - 0.14f, -0.13f), new Vector3(xb - 0.1f, y, 0.13f), TownSwatch.Jade);
                    CurvedRoof(b, xa - 0.3f, xb + 0.3f, -0.75f, 0.75f, y + 0.36f, middle ? 0.75f : 0.6f, TownSwatch.RoofTile, TownSwatch.Lacquer, TownSwatch.Lacquer, 0f, 0f, 0f, 0f);
                }

                // The name plaque (gold on lacquer) in the middle bay and two lanterns in the outer ones.
                float mid = w * 0.5f;
                b.Box(new Vector3(mid, 3.35f, -0.2f), new Vector3(1.0f, 0.5f, 0.06f), TownSwatch.Lacquer, 0f);
                b.Box(new Vector3(mid, 3.35f, -0.24f), new Vector3(0.82f, 0.36f, 0.02f), TownSwatch.GoldTile, 0f);
                PaperLantern(b, new Vector3(w / 6f, 3.0f, 0f), 0.36f, true);
                PaperLantern(b, new Vector3(w * 5f / 6f, 3.0f, 0f), 0.36f, true);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// A stretch of the ring wall from the origin to <paramref name="d"/> (the inner rings on the left, local +z): rammed
        /// earth faced with grey brick (its collider is the run inflated by <see cref="RingWallHalfThickness"/>) on a stone
        /// footing, brick courses, a crenellated walk on top; behind it two rows of hall roofs of the inner rings, grey tile in
        /// the first row and golden tile in the second (the golden tile is only for the rings inside). Variant 1 opens the closed
        /// three-arch gate of the 7th ring in the middle (red-lacquered leaves with gold studs, the middle one higher) under a
        /// two-storey gate tower with golden roofs and lanterns: the citadel stays shut until the Four Gluttons (map 46).
        /// </summary>
        private static void RingWall(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            string key = "ringwall" + Mathf.RoundToInt(length * 10f) + "_" + variant;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                float h = RingWallHeight * s;
                float zf = -RingWallHalfThickness;
                b.BoxMinMax(new Vector3(-0.4f, 0f, zf), new Vector3(length + 0.4f, h, 1.6f), TownSwatch.GreyBrick, TownSwatch.Paving);
                b.BoxMinMax(new Vector3(-0.45f, 0f, zf - 0.08f), new Vector3(length + 0.45f, 0.6f, 1.68f), TownSwatch.StoneDark);
                foreach (float y in new[] { h * 0.4f, h * 0.72f })
                {
                    b.BoxMinMax(new Vector3(-0.4f, y, zf - 0.03f), new Vector3(length + 0.4f, y + 0.07f, zf), TownSwatch.RoofTile);
                }

                // Crenellations along the front edge of the wall walk.
                int merlons = Mathf.Max(2, Mathf.RoundToInt((length + 0.8f) / 0.95f));
                for (int i = 0; i < merlons; i++)
                {
                    float x = Mathf.Lerp(-0.2f, length + 0.2f, i / (float)(merlons - 1));
                    b.Box(new Vector3(x, h + 0.25f, zf + 0.15f), new Vector3(0.48f, 0.5f, 0.3f), TownSwatch.GreyBrick, 0f, TownSwatch.Paving);
                }

                float mid = length * 0.5f;

                // The inner rings: two rows of hall roofs, grey then golden.
                for (int row = 0; row < 2; row++)
                {
                    float zc = 3.4f + row * 3.0f;
                    float baseH = h - 1.0f + row * 1.3f;
                    TownSwatch tile = row == 0 ? TownSwatch.RoofTile : TownSwatch.GoldTile;
                    int count = Mathf.Max(1, Mathf.RoundToInt(length / 4.2f));
                    for (int k = 0; k < count; k++)
                    {
                        float t = (k + 0.35f + 0.3f * TownStyle.Hash01(key, row * 17 + k)) / count;
                        float x = Mathf.Lerp(0.8f, length - 0.8f, t);
                        if (variant == 1 && Mathf.Abs(x - mid) < 3.4f)
                        {
                            continue;
                        }

                        float hw = 1.2f + 0.4f * TownStyle.Hash01(key, row * 29 + k + 3);
                        float hh = baseH + 0.6f * TownStyle.Hash01(key, row * 31 + k + 7);
                        b.BoxMinMax(new Vector3(x - hw, 0f, zc - 1.0f), new Vector3(x + hw, hh, zc + 1.0f), (k % 2) == 0 ? TownSwatch.Plaster : TownSwatch.GreyBrick);
                        b.BoxMinMax(new Vector3(x - hw - 0.02f, hh - 0.3f, zc - 1.02f), new Vector3(x + hw + 0.02f, hh, zc + 1.02f), TownSwatch.Lacquer);
                        CurvedRoof(b, x - hw - 0.35f, x + hw + 0.35f, zc - 1.4f, zc + 1.4f, hh + 0.05f, 0.9f, tile, TownSwatch.Lacquer, TownSwatch.Plaster, x - hw, x + hw, zc - 1.0f, zc + 1.0f);
                    }
                }

                if (variant == 1)
                {
                    // The closed three-arch gate of the 7th ring and its gate tower.
                    foreach (float off in new[] { -2.1f, 0f, 2.1f })
                    {
                        bool centre = Mathf.Abs(off) < 0.01f;
                        float gw = centre ? 1.7f : 1.15f;
                        float gh = centre ? 2.9f : 2.2f;
                        float x = mid + off;
                        b.BoxMinMax(new Vector3(x - gw * 0.5f - 0.18f, 0f, zf - 0.06f), new Vector3(x + gw * 0.5f + 0.18f, gh + 0.2f, zf), TownSwatch.StoneDark);
                        ArchFill(b, x, gh + 0.2f, gw + 0.36f, 0.45f, zf - 0.06f, TownSwatch.StoneDark);
                        b.BoxMinMax(new Vector3(x - gw * 0.5f, 0f, zf - 0.1f), new Vector3(x + gw * 0.5f, gh, zf - 0.06f), TownSwatch.Lacquer);
                        b.BoxMinMax(new Vector3(x - 0.02f, 0f, zf - 0.11f), new Vector3(x + 0.02f, gh, zf - 0.1f), TownSwatch.Black);
                        for (int row = 0; row < 3; row++)
                        {
                            b.Box(new Vector3(x - gw * 0.25f, 0.6f + row * 0.6f, zf - 0.12f), new Vector3(0.08f, 0.08f, 0.03f), TownSwatch.GoldTile, 0f);
                            b.Box(new Vector3(x + gw * 0.25f, 0.6f + row * 0.6f, zf - 0.12f), new Vector3(0.08f, 0.08f, 0.03f), TownSwatch.GoldTile, 0f);
                        }
                    }

                    // The gate tower: lacquered columns, a lit lattice, two storeys of golden roofs.
                    float ty = h;
                    b.BoxMinMax(new Vector3(mid - 2.6f, ty, zf + 0.3f), new Vector3(mid + 2.6f, ty + 1.9f, 1.4f), TownSwatch.Lacquer);
                    b.BoxMinMax(new Vector3(mid - 2.0f, ty + 0.5f, zf + 0.27f), new Vector3(mid + 2.0f, ty + 1.4f, zf + 0.3f), TownSwatch.WindowLit);
                    foreach (float lx in new[] { -2.7f, -1.0f, 1.0f, 2.7f })
                    {
                        b.Log(new Vector3(mid + lx, ty, zf + 0.15f), new Vector3(mid + lx, ty + 1.9f, zf + 0.15f), 0.12f, TownSwatch.Lacquer, 6);
                    }

                    CurvedRoof(b, mid - 3.3f, mid + 3.3f, zf - 0.4f, 2.2f, ty + 1.95f, 1.0f, TownSwatch.GoldTile, TownSwatch.Lacquer, TownSwatch.Lacquer, mid - 2.6f, mid + 2.6f, zf + 0.3f, 1.4f);
                    b.BoxMinMax(new Vector3(mid - 1.6f, ty + 2.6f, zf + 0.6f), new Vector3(mid + 1.6f, ty + 3.4f, 1.1f), TownSwatch.Lacquer);
                    CurvedRoof(b, mid - 2.2f, mid + 2.2f, zf + 0.1f, 1.7f, ty + 3.45f, 0.8f, TownSwatch.GoldTile, TownSwatch.Lacquer, TownSwatch.Lacquer, mid - 1.6f, mid + 1.6f, zf + 0.6f, 1.1f);
                    PaperLantern(b, new Vector3(mid - 1.8f, ty + 1.9f, zf - 0.2f), 0.4f, true);
                    PaperLantern(b, new Vector3(mid + 1.8f, ty + 1.9f, zf - 0.2f), 0.4f, true);
                }
            });
        }

        /// <summary>
        /// The Torre do Tambor e do Sino on the plaza (the city's clock: the gates open with the drum by day and close with the
        /// bell at night): a square grey-brick base with an arched passage, a lacquered pavilion on top holding the drum and the
        /// bronze bell, a hipped roof of grey tile with turned-up corners and a smaller second roof with a golden finial, two
        /// paper lanterns.
        /// </summary>
        private static void BellDrumTower(TownMeshBuilder b, float s)
        {
            float half = 1.0f * s;
            float baseH = 2.6f * s;
            b.BoxMinMax(new Vector3(-half, 0f, -half), new Vector3(half, baseH, half), TownSwatch.GreyBrick, TownSwatch.Paving);
            b.BoxMinMax(new Vector3(-half - 0.05f, 0f, -half - 0.05f), new Vector3(half + 0.05f, 0.5f, half + 0.05f), TownSwatch.StoneDark);
            b.BoxMinMax(new Vector3(-half - 0.04f, baseH - 0.25f, -half - 0.04f), new Vector3(half + 0.04f, baseH, half + 0.04f), TownSwatch.Lacquer);
            b.BoxMinMax(new Vector3(-0.42f, 0f, -half - 0.02f), new Vector3(0.42f, 1.5f, -half - 0.01f), TownSwatch.Black);
            ArchFill(b, 0f, 1.5f, 0.84f, 0.35f, -half - 0.015f, TownSwatch.Black);

            // The pavilion: four lacquered columns, the drum and the bell.
            float py = baseH;
            float ph = 1.35f * s;
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                b.Log(new Vector3(c.x * 0.72f * s, py, c.y * 0.72f * s), new Vector3(c.x * 0.72f * s, py + ph, c.y * 0.72f * s), 0.08f, TownSwatch.Lacquer, 6);
            }

            b.Log(new Vector3(-0.62f * s, py + 0.7f, 0f), new Vector3(-0.12f * s, py + 0.7f, 0f), 0.3f * s, TownSwatch.Lacquer, 10, TownSwatch.ClothWhite);
            b.Log(new Vector3(0.4f * s, py + ph - 0.05f, 0f), new Vector3(0.4f * s, py + ph - 0.3f, 0f), 0.02f, TownSwatch.Iron, 4);
            b.Cylinder(new Vector3(0.4f * s, py + 0.45f, 0f), 0.27f * s, 0.13f * s, 0.55f * s, 8, TownSwatch.Bronze, TownSwatch.Bronze);

            // The roofs: a hipped grey roof with turned-up corners, a smaller second roof and the golden finial.
            float r1 = (half + 0.55f) * 1.414f;
            float y1 = py + ph;
            b.Cylinder(new Vector3(0f, y1, 0f), r1, 0.65f * s, 0.6f * s, 4, TownSwatch.RoofTile, TownSwatch.RoofTile, 0.5f);
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                var p = new Vector3(c.x * (half + 0.5f), y1 + 0.02f, c.y * (half + 0.5f));
                b.Log(p, p + new Vector3(c.x * 0.25f, 0.3f, c.y * 0.25f), 0.06f, TownSwatch.RoofTile, 4);
            }

            float y2 = y1 + 0.6f * s;
            b.BoxMinMax(new Vector3(-0.45f * s, y2, -0.45f * s), new Vector3(0.45f * s, y2 + 0.5f * s, 0.45f * s), TownSwatch.Lacquer);
            b.Cylinder(new Vector3(0f, y2 + 0.5f * s, 0f), 1.0f * s, 0f, 0.75f * s, 4, TownSwatch.RoofTile, null, 0.5f);
            b.Blob(new Vector3(0f, y2 + 1.35f * s, 0f), new Vector3(0.22f, 0.3f, 0.22f) * s, TownSwatch.GoldTile);

            PaperLantern(b, new Vector3(-half - 0.3f, y1, -half - 0.3f), 0.34f, true);
            PaperLantern(b, new Vector3(half + 0.3f, y1, -half - 0.3f), 0.34f, true);
        }

        /// <summary>A weeping willow in a ring of grey brick: a leaning trunk, a soft crown and strands hanging down (variant 1: a paper lantern hangs from a branch).</summary>
        private static void Willow(TownMeshBuilder b, float s, int variant)
        {
            float r = WillowRadius * s;
            b.Ring(new Vector3(0f, 0.02f, 0f), r * 0.72f, r, 8, TownSwatch.GreyBrick, TownSwatch.StoneDark);
            b.Disc(new Vector3(0f, 0.03f, 0f), r * 0.72f, 8, TownSwatch.Dirt);
            var fork = new Vector3(0.15f * s, 1.7f * s, 0.05f);
            b.Log(Vector3.zero, fork, 0.12f * s, TownSwatch.Trunk, 5);
            b.Log(fork, new Vector3(-0.3f * s, 2.3f * s, -0.1f), 0.07f * s, TownSwatch.Trunk, 4);
            b.Crown(new Vector3(0.05f, 2.45f * s, 0f), new Vector3(1.9f, 0.95f, 1.8f) * s, TownSwatch.Willow);
            for (int i = 0; i < 8; i++)
            {
                float a = (i * 45f + 20f) * Mathf.Deg2Rad;
                float rr = 0.8f * s;
                var p = new Vector3(Mathf.Cos(a) * rr, 1.55f * s, Mathf.Sin(a) * rr);
                b.Box(p, new Vector3(0.22f, 1.25f * s, 0.06f), (i % 2) == 0 ? TownSwatch.Willow : TownSwatch.LeafDark, -i * 45f - 20f);
            }

            if ((variant % 2) == 1)
            {
                PaperLantern(b, new Vector3(0.55f * s, 2.0f * s, -0.55f * s), 0.32f, true);
            }
        }

        /// <summary>
        /// A canal from the origin to <paramref name="d"/> (water <see cref="CanalHalfWidth"/> each side between grey-brick
        /// kerbs) crossed at its middle by a humpback stone bridge with low brick parapets. Walkable: the hump stays under the
        /// walker's step.
        /// </summary>
        private static void CanalBridge(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = CanalHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.04f, TownSwatch.CanalWater);
                foreach (float z in new[] { -w - 0.14f, w })
                {
                    b.BoxMinMax(new Vector3(0f, 0f, z), new Vector3(length, 0.12f, z + 0.14f), TownSwatch.GreyBrick, TownSwatch.Paving);
                }

                float xm = length * 0.5f;
                float half = Mathf.Min(1.5f, length * 0.5f - 0.1f);
                float zEnd = w + 0.45f;
                const float hump = 0.3f;
                b.Slab(new Vector3(xm - half, 0.06f, -zEnd), new Vector3(xm - half, hump, 0f), new Vector3(xm + half, hump, 0f), new Vector3(xm + half, 0.06f, -zEnd), 0.1f,
                    TownSwatch.Paving, TownSwatch.StoneDark);
                b.Slab(new Vector3(xm - half, hump, 0f), new Vector3(xm - half, 0.06f, zEnd), new Vector3(xm + half, 0.06f, zEnd), new Vector3(xm + half, hump, 0f), 0.1f,
                    TownSwatch.Paving, TownSwatch.StoneDark);
                foreach (float x in new[] { xm - half + 0.08f, xm + half - 0.08f })
                {
                    b.Log(new Vector3(x, 0.12f, -zEnd), new Vector3(x, hump + 0.3f, 0f), 0.08f, TownSwatch.GreyBrick, 4);
                    b.Log(new Vector3(x, hump + 0.3f, 0f), new Vector3(x, 0.12f, zEnd), 0.08f, TownSwatch.GreyBrick, 4);
                }
            });
        }

        /// <summary>
        /// A lantern frame of the Festa das Lanternas: two red-lacquered posts on stone feet, a bar with a little tile roof,
        /// three paper lanterns with the riddle slips hanging under them (variant 1: the middle one is the golden fish lantern).
        /// </summary>
        private static void LanternFrame(TownMeshBuilder b, float s, int variant)
        {
            float hx = LanternFrameSize.x * 0.5f * s - 0.1f;
            float top = 2.3f * s;
            foreach (float x in new[] { -hx, hx })
            {
                b.Box(new Vector3(x, 0.12f, 0f), new Vector3(0.32f, 0.24f, 0.32f), TownSwatch.StoneDark, 0f);
                b.Log(new Vector3(x, 0.24f, 0f), new Vector3(x, top, 0f), 0.06f, TownSwatch.Lacquer, 6);
            }

            b.Log(new Vector3(-hx - 0.12f, top - 0.12f, 0f), new Vector3(hx + 0.12f, top - 0.12f, 0f), 0.06f, TownSwatch.Lacquer, 4);
            b.Slab(new Vector3(-hx - 0.3f, top, -0.3f), new Vector3(-hx - 0.3f, top + 0.18f, 0f), new Vector3(hx + 0.3f, top + 0.18f, 0f), new Vector3(hx + 0.3f, top, -0.3f), 0.06f,
                TownSwatch.RoofTile, TownSwatch.Lacquer);
            b.Slab(new Vector3(-hx - 0.3f, top + 0.18f, 0f), new Vector3(-hx - 0.3f, top, 0.3f), new Vector3(hx + 0.3f, top, 0.3f), new Vector3(hx + 0.3f, top + 0.18f, 0f), 0.06f,
                TownSwatch.RoofTile, TownSwatch.Lacquer);

            TownSwatch[] slips = { TownSwatch.ClothYellow, TownSwatch.ClothRed, TownSwatch.ClothWhite };
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * hx * 0.62f;
                var hook = new Vector3(x, top - 0.18f, 0f);
                if (i == 1 && (variant % 2) == 1)
                {
                    // The golden fish lantern (a carp): body, tail and fin.
                    Vector3 c = hook + Vector3.down * 0.55f;
                    b.Log(hook, c + Vector3.up * 0.14f, 0.012f, TownSwatch.Black, 3);
                    b.Blob(c, new Vector3(0.6f, 0.3f, 0.26f), TownSwatch.LanternPaper);
                    b.Blob(c + new Vector3(0.36f, 0.04f, 0f), new Vector3(0.2f, 0.3f, 0.06f), TownSwatch.GoldTile);
                    b.Box(c + new Vector3(-0.05f, 0.16f, 0f), new Vector3(0.2f, 0.08f, 0.03f), TownSwatch.Lacquer, 0f);
                }
                else
                {
                    PaperLantern(b, hook, 0.3f, i != 1);
                }

                b.Box(new Vector3(x, top - 1.35f, 0f), new Vector3(0.09f, 0.3f, 0.01f), slips[i], 0f);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Mōzak's front: a rack of paper lanterns (round red, tall white, the fish), riddle slips, a stack of paper and the pot of paste.</summary>
        private static void RingWallDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            foreach (float lx in new[] { -0.7f, 0.7f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.45f), new Vector3(x + lx, y + 1.7f, z - 0.45f), 0.045f, TownSwatch.Lacquer, 4);
            }

            b.Log(new Vector3(x - 0.75f, y + 1.65f, z - 0.45f), new Vector3(x + 0.75f, y + 1.65f, z - 0.45f), 0.035f, TownSwatch.Lacquer, 4);
            PaperLantern(b, new Vector3(x - 0.4f, y + 1.62f, z - 0.45f), 0.3f, true);
            PaperLantern(b, new Vector3(x + 0.05f, y + 1.62f, z - 0.45f), 0.26f, false);
            PaperLantern(b, new Vector3(x + 0.45f, y + 1.62f, z - 0.45f), 0.3f, true);

            // A stack of lantern paper, the paste pot and a fish lantern waiting on the deck.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(x - 0.25f, y + 0.06f + i * 0.08f, z - 0.95f), new Vector3(0.55f, 0.07f, 0.4f), (i % 2) == 0 ? TownSwatch.LanternPaper : TownSwatch.ClothRed, 0f);
            }

            b.Cylinder(new Vector3(x + 0.45f, y, z - 0.95f), 0.16f, 0.13f, 0.22f, 8, TownSwatch.Mud, TownSwatch.ClothWhite);
            b.Blob(new Vector3(x + 0.9f, y + 0.2f, z - 0.75f), new Vector3(0.5f, 0.26f, 0.22f), TownSwatch.LanternPaper);
            b.Blob(new Vector3(x + 1.18f, y + 0.22f, z - 0.75f), new Vector3(0.16f, 0.24f, 0.05f), TownSwatch.GoldTile);
        }
    }
}
