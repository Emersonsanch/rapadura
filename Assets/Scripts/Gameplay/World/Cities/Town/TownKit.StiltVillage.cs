using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Palafitas de Tarchon, the trols' city in the open swamp of the Delta de Jhilpur (Docs/Mapas100.md §3.3
    /// and map 33, style <c>StiltVillage</c>): the long houses on piles (<see cref="StiltLonghouseHouse"/>: tall timber piles
    /// standing in the bog water, a plank floor high over it, the gable end to the street with the door, a steep leaf roof that
    /// comes down to the floor on both sides, fishing nets drying on the eaves, a plank stair up from the deck and a
    /// will-o'-wisp lantern under the gable), the edge of the open swamp, plank boardwalks over the bog channels, the Mastro dos
    /// Fogos-Fátuos on the plaza, giant baskets, nets drying on racks, floating reed islands and the pile gate at the streets'
    /// ends; Larthia's baskets. Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/> (row 12 holds moss
    /// blue, wet wood, leaf, lilac mist, the will-o'-wisp, bog water, rush and net twine), low-poly, deterministic. The
    /// will-o'-wisps are sparks caught in the Ar Sereno (bíblia §3.5), lanterns, not a rite.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of a <see cref="CityPropKind.SwampEdge"/>'s rail: its collider is the run inflated by this.</summary>
        public const float SwampEdgeHalfThickness = 0.5f;

        /// <summary>Radius of the <see cref="CityPropKind.WispMast"/>'s round deck (× its scale): its collider.</summary>
        public const float WispMastRadius = 1.4f;

        /// <summary>Radius of a <see cref="CityPropKind.GiantBasket"/> (× its scale): its collider.</summary>
        public const float GiantBasketRadius = 0.95f;

        /// <summary>Footprint of a <see cref="CityPropKind.NetRack"/> (local x along the net, × its scale): its collider.</summary>
        public static readonly Vector2 NetRackSize = new Vector2(3.2f, 0.6f);

        /// <summary>Radius of a <see cref="CityPropKind.ReedIsland"/>'s patch of bog water (× its scale): its collider.</summary>
        public const float ReedIslandRadius = 1.8f;

        /// <summary>Half the width of the bog channel of a <see cref="CityPropKind.Boardwalk"/>.</summary>
        public const float BogChannelHalfWidth = 0.8f;

        /// <summary>How high the plank floor of a trol long house stands over the bog (trols are three metres tall).</summary>
        public const float LonghouseLift = 1.25f;

        // ------------------------------------------------------------------
        // The long house on piles
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.StiltLonghouse"/> (local frame, front −z): bog water under it, eight timber piles with
        /// cross braces, the plank floor <see cref="LonghouseLift"/> over the footing, low rush walls, the gable end to the
        /// street (woven gable with two moss-blue bands, the tall door, a lit window high up), the steep leaf roof whose eaves
        /// come down to the floor on both sides (a rush course at the bottom, leaf above, a ridge pole with crossed ends), nets
        /// drying on one eave, a will-o'-wisp lantern hanging under the gable; on the deck a plank stair up to the door and a
        /// leaf awning on piles (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list).
        /// </summary>
        public static void StiltLonghouseHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float floor = TownBuildingBuilder.FloorY + LonghouseLift;
            float rise = Mathf.Max(2.6f, top - TownBuildingBuilder.FloorY + 0.4f);
            float ridgeY = floor + rise;
            TownSwatch wood = TownSwatch.WetWood;

            // The bog water standing under the house.
            b.Flat(-hw, zFacade, hw, zBack, 0.03f, TownSwatch.BogWater);

            // Piles: three rows of three (the middle one left out under the door), cross braces on the long sides.
            float inX = hw - 0.3f;
            float zMid = (zFacade + zBack) * 0.5f;
            foreach (float px in new[] { -inX, 0f, inX })
            {
                foreach (float pz in new[] { zFacade + 0.3f, zMid, zBack - 0.3f })
                {
                    if (px == 0f && pz == zMid)
                    {
                        continue;
                    }

                    b.Log(new Vector3(px, 0f, pz), new Vector3(px, floor - 0.1f, pz), 0.14f, wood, 5);
                }
            }

            foreach (float px in new[] { -inX, inX })
            {
                b.Log(new Vector3(px, 0.2f, zFacade + 0.3f), new Vector3(px, floor - 0.25f, zMid), 0.05f, TownSwatch.WoodDark, 4);
                b.Log(new Vector3(px, 0.2f, zBack - 0.3f), new Vector3(px, floor - 0.25f, zMid), 0.05f, TownSwatch.WoodDark, 4);
            }

            // The plank floor, a little wider than the walls.
            b.BoxMinMax(new Vector3(-hw - 0.1f, floor - 0.15f, zFacade - 0.1f), new Vector3(hw + 0.1f, floor, zBack + 0.1f), wood, TownSwatch.WoodWeathered);

            // Low rush walls under the roof (mostly hidden by the eaves).
            float wx = hw - 0.35f;
            float wzF = zFacade + 0.15f;
            float wzB = zBack - 0.15f;
            float ox = hw + 0.2f;
            float eaveY = floor + 0.2f;
            float RoofY(float x) => eaveY + (ridgeY - eaveY) * (1f - Mathf.Abs(x) / ox);
            float knee = RoofY(wx);
            b.BoxMinMax(new Vector3(-wx, floor, wzF), new Vector3(wx, knee, wzB), spec.WallColor);

            // The gable ends: woven rush, front and back.
            foreach (float gz in new[] { wzF, wzB })
            {
                var facing = new Vector3(0f, 0f, gz < zMid ? -1f : 1f);
                b.QuadFacing(new Vector3(-wx, floor, gz), new Vector3(wx, floor, gz), new Vector3(wx, knee, gz), new Vector3(-wx, knee, gz), facing, spec.WallColor);
                b.TriangleFacing(new Vector3(-wx, knee, gz), new Vector3(wx, knee, gz), new Vector3(0f, ridgeY - 0.05f, gz), facing, spec.WallColor);
            }

            // Corner posts and two moss-blue plaited bands across the front gable.
            foreach (float px in new[] { -wx, wx })
            {
                b.Log(new Vector3(px, floor, wzF - 0.02f), new Vector3(px, RoofY(px) + 0.05f, wzF - 0.02f), 0.09f, spec.JoineryColor, 5);
            }

            foreach (float band in new[] { floor + 0.35f, Mathf.Min(knee - 0.15f, floor + 2.25f) })
            {
                float half = Mathf.Min(wx, ox * (1f - (band - eaveY) / (ridgeY - eaveY))) - 0.05f;
                if (half > 0.3f)
                {
                    b.BoxMinMax(new Vector3(-half, band - 0.06f, wzF - 0.05f), new Vector3(half, band + 0.06f, wzF), spec.TrimColor);
                }
            }

            // The tall door (trols are tall) and a lit window high in the gable.
            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.5f : 1.1f;
            float doorTop = floor + 2.0f;
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, floor, wzF - 0.03f), new Vector3(doorX + doorW * 0.5f, doorTop, wzF - 0.01f), TownSwatch.Black);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.12f, floor, wzF - 0.08f), new Vector3(doorX - doorW * 0.5f, doorTop + 0.1f, wzF), spec.JoineryColor);
            b.BoxMinMax(new Vector3(doorX + doorW * 0.5f, floor, wzF - 0.08f), new Vector3(doorX + doorW * 0.5f + 0.12f, doorTop + 0.1f, wzF), spec.JoineryColor);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.18f, doorTop, wzF - 0.09f), new Vector3(doorX + doorW * 0.5f + 0.18f, doorTop + 0.16f, wzF), spec.JoineryColor);
            float winY = Mathf.Min(ridgeY - 0.9f, doorTop + 0.7f);
            b.BoxMinMax(new Vector3(-0.3f, winY, wzF - 0.03f), new Vector3(0.3f, winY + 0.45f, wzF - 0.01f), TownSwatch.WindowLit);
            b.BoxMinMax(new Vector3(-0.36f, winY - 0.06f, wzF - 0.06f), new Vector3(0.36f, winY, wzF), spec.JoineryColor);

            // The steep roof: a rush course along the eaves, leaf above, from the facade (a short overhang) to past the back.
            float zr0 = wzF - 0.45f;
            float zr1 = zBack + 0.45f;
            float midY = Mathf.Lerp(eaveY, ridgeY, 0.35f);
            float midX = ox * 0.65f;
            foreach (float side in new[] { -1f, 1f })
            {
                var e0 = new Vector3(side * ox, eaveY, zr1);
                var e1 = new Vector3(side * ox, eaveY, zr0);
                var m0 = new Vector3(side * midX, midY, zr1);
                var m1 = new Vector3(side * midX, midY, zr0);
                var r0 = new Vector3(0f, ridgeY, zr1);
                var r1 = new Vector3(0f, ridgeY, zr0);
                if (side < 0f)
                {
                    b.Slab(e0, m0, m1, e1, 0.16f, TownSwatch.Rush, TownSwatch.WoodDark);
                    b.Slab(m0, r0, r1, m1, 0.16f, spec.RoofColor, TownSwatch.WoodDark);
                }
                else
                {
                    b.Slab(m0, e0, e1, m1, 0.16f, TownSwatch.Rush, TownSwatch.WoodDark);
                    b.Slab(r0, m0, m1, r1, 0.16f, spec.RoofColor, TownSwatch.WoodDark);
                }
            }

            // Ridge pole and the crossed poles over the front gable.
            b.Log(new Vector3(0f, ridgeY + 0.08f, zr0 - 0.1f), new Vector3(0f, ridgeY + 0.08f, zr1 + 0.1f), 0.08f, wood, 5);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Log(new Vector3(side * 0.55f, ridgeY - 0.35f, zr0 - 0.05f), new Vector3(-side * 0.45f, ridgeY + 0.6f, zr0 - 0.1f), 0.06f, wood, 4);
            }

            // A will-o'-wisp lantern under the front gable: a lilac cage round the pale light.
            var hang = new Vector3(0f, ridgeY - 0.35f, zr0 + 0.05f);
            b.Log(hang, hang + Vector3.down * 0.45f, 0.012f, TownSwatch.Black, 3);
            b.Blob(hang + Vector3.down * 0.62f, new Vector3(0.26f, 0.24f, 0.26f), TownSwatch.WillWisp);
            b.Ring(hang + Vector3.down * 0.5f, 0.12f, 0.17f, 6, TownSwatch.LilacMist);

            // Fishing nets drying on one eave (by the seed), a few floats along the bottom.
            float netSide = TownStyle.Hash01(spec.Seed ?? string.Empty, 3) < 0.5f ? -1f : 1f;
            float nx = netSide * (ox + 0.04f);
            float nz0 = zMid - 1.1f;
            float nz1 = zMid + 1.1f;
            b.TwoSidedQuad(new Vector3(nx, eaveY - 0.02f, nz0), new Vector3(nx, eaveY - 0.02f, nz1), new Vector3(nx, eaveY - 0.95f, nz1 - 0.2f), new Vector3(nx, eaveY - 0.85f, nz0 + 0.2f), TownSwatch.NetTwine);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(nx, eaveY - 0.92f, Mathf.Lerp(nz0 + 0.3f, nz1 - 0.3f, i * 0.5f)), new Vector3(0.12f, 0.12f, 0.12f), TownSwatch.ClothWhite);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.WoodWeathered, wood);

                // The plank stair from the deck up to the door.
                const int steps = 5;
                float stairDepth = Mathf.Min(1.35f, (zFacade - zFront) * 0.8f);
                for (int i = 0; i < steps; i++)
                {
                    float y = Mathf.Lerp(deck, floor, (i + 1f) / steps);
                    float z0 = zFacade - stairDepth * (steps - i) / steps;
                    b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.05f, deck, z0), new Vector3(doorX + doorW * 0.5f + 0.05f, y, zFacade), TownSwatch.WoodWeathered, wood);
                }

                // Piles and the leaf awning over the deck.
                float z = zFront + TownBuildingBuilder.PostInset;
                float low = Mathf.Max(top + 0.1f, doorTop - 0.2f);
                float high = Mathf.Max(low + 0.35f, doorTop + 0.35f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, low + 0.05f, z), 0.09f, wood, 5);
                }

                b.Log(new Vector3(-hw - 0.1f, low, z), new Vector3(hw + 0.1f, low, z), 0.07f, wood, 4);
                b.Slab(new Vector3(-hw - 0.15f, low, z - 0.25f), new Vector3(-hw - 0.15f, high, wzF), new Vector3(hw + 0.15f, high, wzF), new Vector3(hw + 0.15f, low, z - 0.25f),
                    0.08f, spec.RoofColor, TownSwatch.WoodDark);
            }
        }

        /// <summary>The pile gate of Tarchon: two tall piles bound with rush at the foot, a crossbar under a little leaf roof, will-o'-wisp lanterns.</summary>
        public static void StiltGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            const float height = 3.6f;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Log(p, p + Vector3.up * height, 0.2f, TownSwatch.WetWood, 6);
                b.Crown(p + Vector3.up * 0.4f, new Vector3(0.75f, 0.8f, 0.75f), TownSwatch.Rush);
                b.Cylinder(p + Vector3.up * 1.2f, 0.23f, 0.23f, 0.12f, 6, TownSwatch.TrollMoss);
            }

            Vector3 along = (c - a).normalized;
            var across = new Vector3(-along.z, 0f, along.x);
            Vector3 ta = a + Vector3.up * (height - 0.3f) - along * 0.4f;
            Vector3 tc = c + Vector3.up * (height - 0.3f) + along * 0.4f;
            b.Log(ta, tc, 0.12f, TownSwatch.WetWood, 5);
            Vector3 ridgeA = ta + Vector3.up * 0.7f;
            Vector3 ridgeC = tc + Vector3.up * 0.7f;
            b.Slab(ta - across * 0.7f, ridgeA, ridgeC, tc - across * 0.7f, 0.1f, TownSwatch.SwampLeaf, TownSwatch.WoodDark);
            b.Slab(ridgeA, ta + across * 0.7f, tc + across * 0.7f, ridgeC, 0.1f, TownSwatch.SwampLeaf, TownSwatch.WoodDark);

            foreach (float t in new[] { 0.3f, 0.7f })
            {
                Vector3 h = Vector3.Lerp(ta, tc, t) + Vector3.down * 0.1f;
                b.Log(h, h + Vector3.down * 0.5f, 0.012f, TownSwatch.Black, 3);
                b.Blob(h + Vector3.down * 0.66f, new Vector3(0.24f, 0.22f, 0.24f), TownSwatch.WillWisp);
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The edge of the open swamp from the origin to <paramref name="d"/>: a rail of wet piles (its collider is the run
        /// inflated by <see cref="SwampEdgeHalfThickness"/>), rush along it and, on the back side, the bog water with clumps of
        /// rush and a few will-o'-wisps drifting low.
        /// </summary>
        private static void SwampEdge(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                int piles = Mathf.Max(2, Mathf.RoundToInt(length / 1.4f) + 1);
                for (int i = 0; i < piles; i++)
                {
                    float x = length * i / (piles - 1);
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x, 0.95f * s, 0f), 0.1f, TownSwatch.WetWood, 5);
                }

                b.Log(new Vector3(0f, 0.75f * s, 0f), new Vector3(length, 0.75f * s, 0f), 0.05f, TownSwatch.WetWood, 4);
                b.Log(new Vector3(0f, 0.4f * s, 0f), new Vector3(length, 0.4f * s, 0f), 0.04f, TownSwatch.WoodDark, 4);
                b.Flat(-0.4f, SwampEdgeHalfThickness, length + 0.4f, SwampEdgeHalfThickness + 6f, 0.03f, TownSwatch.BogWater);

                int clumps = Mathf.Max(2, Mathf.RoundToInt(length / 2.5f));
                string key = "swamp" + Mathf.RoundToInt(length * 10f);
                for (int i = 0; i < clumps; i++)
                {
                    float x = (i + 0.5f) * length / clumps;
                    float z = SwampEdgeHalfThickness + 0.3f + 3.5f * TownStyle.Hash01(key, i);
                    b.Crown(new Vector3(x, 0.35f, 0.2f), new Vector3(0.7f, 0.7f, 0.45f), (i % 2) == 0 ? TownSwatch.Rush : TownSwatch.SwampLeaf);
                    b.Crown(new Vector3(x + 0.6f, 0.3f, z), new Vector3(0.6f, 0.6f, 0.6f), TownSwatch.Rush);
                    if ((i % 2) == 1)
                    {
                        b.Blob(new Vector3(x - 0.4f, 0.9f + 0.3f * TownStyle.Hash01(key, i + 7), z + 0.8f), new Vector3(0.2f, 0.2f, 0.2f), TownSwatch.WillWisp);
                    }
                }
            });
        }

        /// <summary>
        /// A bog channel from the origin to <paramref name="d"/> (a strip <see cref="BogChannelHalfWidth"/> each side) and, at its
        /// middle, a plank boardwalk across it on short piles, with a low rope rail on posts at both sides. Walkable: the planks
        /// stay under the walker's step.
        /// </summary>
        private static void Boardwalk(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = BogChannelHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.04f, TownSwatch.BogWater);
                b.Crown(new Vector3(0.2f, 0.3f, -w + 0.2f), new Vector3(0.5f, 0.6f, 0.4f), TownSwatch.Rush);
                b.Crown(new Vector3(length - 0.2f, 0.3f, w - 0.2f), new Vector3(0.5f, 0.6f, 0.4f), TownSwatch.Rush);

                float xm = length * 0.5f;
                float half = Mathf.Min(1.3f, length * 0.5f - 0.1f);
                float bw = w + 0.4f;
                const int planks = 7;
                for (int i = 0; i < planks; i++)
                {
                    float z0 = -bw + 2f * bw * i / planks + 0.02f;
                    float z1 = -bw + 2f * bw * (i + 1) / planks - 0.02f;
                    b.BoxMinMax(new Vector3(xm - half, 0.08f, z0), new Vector3(xm + half, 0.15f, z1), (i % 2) == 0 ? TownSwatch.WoodWeathered : TownSwatch.WetWood);
                }

                foreach (float x in new[] { xm - half, xm + half })
                {
                    foreach (float z in new[] { -bw, bw })
                    {
                        b.Log(new Vector3(x, 0f, z), new Vector3(x, 0.75f, z), 0.07f, TownSwatch.WetWood, 5);
                    }

                    b.Log(new Vector3(x, 0.68f, -bw), new Vector3(x, 0.6f, 0f), 0.02f, TownSwatch.NetTwine, 3);
                    b.Log(new Vector3(x, 0.6f, 0f), new Vector3(x, 0.68f, bw), 0.02f, TownSwatch.NetTwine, 3);
                }
            });
        }

        /// <summary>
        /// The Mastro dos Fogos-Fátuos: a round deck of wet planks, a tall mast with two crossarms, will-o'-wisp lanterns in lilac
        /// cages hanging from the arm ends, a big one at the top, guy ropes and baskets at its foot. Every wisp set free on map
        /// 33 is brought here to light the city (the Cesto de Lanternas, Docs/Mapas100.md §3.3).
        /// </summary>
        private static void WispMast(TownMeshBuilder b, float s)
        {
            float r = WispMastRadius * s;
            b.Cylinder(Vector3.zero, r, r, 0.25f, 12, TownSwatch.WetWood, TownSwatch.WoodWeathered);
            b.Ring(new Vector3(0f, 0.26f, 0f), r - 0.15f, r, 12, TownSwatch.TrollMoss);
            float mast = 6.2f * s;
            b.Log(Vector3.up * 0.25f, Vector3.up * mast, 0.17f * s, TownSwatch.WetWood, 6);
            b.Crown(Vector3.up * 0.7f, new Vector3(0.7f, 0.8f, 0.7f) * s, TownSwatch.Rush);

            foreach (float armY in new[] { 4.3f * s, 5.4f * s })
            {
                float span = armY < 5f * s ? 1.6f * s : 1.1f * s;
                float yaw = armY < 5f * s ? 0f : 90f;
                Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
                b.Log(Vector3.up * armY - dir * span, Vector3.up * armY + dir * span, 0.07f * s, TownSwatch.WetWood, 4);
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 end = Vector3.up * armY + dir * span * side;
                    b.Log(end, end + Vector3.down * 0.55f * s, 0.012f, TownSwatch.Black, 3);
                    b.Blob(end + Vector3.down * 0.75f * s, new Vector3(0.28f, 0.26f, 0.28f) * s, TownSwatch.WillWisp);
                    b.Ring(end + Vector3.down * 0.62f * s, 0.13f * s, 0.19f * s, 6, TownSwatch.LilacMist);
                }
            }

            b.Blob(Vector3.up * (mast + 0.25f * s), new Vector3(0.45f, 0.42f, 0.45f) * s, TownSwatch.WillWisp);
            b.Ring(Vector3.up * (mast + 0.05f * s), 0.2f * s, 0.3f * s, 8, TownSwatch.LilacMist);
            for (int i = 0; i < 3; i++)
            {
                float a = (i * 120f + 30f) * Mathf.Deg2Rad;
                var foot = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r - 0.2f) + Vector3.up * 0.25f;
                b.Log(Vector3.up * (mast - 0.4f * s), foot, 0.015f, TownSwatch.NetTwine, 3);
            }

            Basket(b, new Vector3(0.75f * s, 0.25f, -0.5f * s), TownSwatch.WillWisp);
            Basket(b, new Vector3(-0.7f * s, 0.25f, -0.6f * s), TownSwatch.TrollMoss);
        }

        /// <summary>A giant woven basket, taller than a cousin: rush with wet-wood bands (variant 0: the day's fish, 1: empty, 2: a lid).</summary>
        private static void GiantBasket(TownMeshBuilder b, float s, int variant)
        {
            float r = GiantBasketRadius * s;
            float h = 1.5f * s;
            b.Cylinder(Vector3.zero, r * 0.72f, r, h, 10, TownSwatch.Rush, TownSwatch.NetTwine);
            foreach (float y in new[] { 0.35f * h, 0.75f * h })
            {
                float rr = Mathf.Lerp(r * 0.72f, r, y / h) + 0.02f;
                b.Cylinder(Vector3.up * y, rr, rr, 0.08f * s, 10, TownSwatch.WetWood);
            }

            b.Cylinder(Vector3.up * h, r + 0.03f, r + 0.03f, 0.07f * s, 10, TownSwatch.WetWood, TownSwatch.Black);
            switch (variant % 3)
            {
                case 0:
                    for (int i = 0; i < 4; i++)
                    {
                        float a = (i * 90f + 20f) * Mathf.Deg2Rad;
                        b.Blob(new Vector3(Mathf.Cos(a) * r * 0.45f, h + 0.06f, Mathf.Sin(a) * r * 0.45f), new Vector3(0.42f, 0.14f, 0.16f) * s, TownSwatch.Stone);
                    }

                    break;
                case 2:
                    b.Crown(Vector3.up * (h + 0.05f), new Vector3(r * 2f, 0.3f * s, r * 2f), TownSwatch.Rush);
                    break;
            }
        }

        /// <summary>A net rack: two poles, a crossbar, the fishing net sagging between them with floats and two fish drying.</summary>
        private static void NetRack(TownMeshBuilder b, float s)
        {
            float hx = NetRackSize.x * 0.5f * s - 0.15f;
            float h = 2.0f * s;
            foreach (float x in new[] { -hx, hx })
            {
                b.Log(new Vector3(x, 0f, 0f), new Vector3(x, h, 0f), 0.08f, TownSwatch.WetWood, 5);
            }

            b.Log(new Vector3(-hx - 0.1f, h - 0.1f, 0f), new Vector3(hx + 0.1f, h - 0.1f, 0f), 0.05f, TownSwatch.WetWood, 4);
            b.TwoSidedQuad(new Vector3(-hx, h - 0.12f, 0f), new Vector3(0f, h - 0.12f, 0f), new Vector3(0f, 0.5f * s, 0.05f), new Vector3(-hx, 0.75f * s, 0f), TownSwatch.NetTwine);
            b.TwoSidedQuad(new Vector3(0f, h - 0.12f, 0f), new Vector3(hx, h - 0.12f, 0f), new Vector3(hx, 0.75f * s, 0f), new Vector3(0f, 0.5f * s, 0.05f), TownSwatch.NetTwine);
            for (int i = 0; i < 4; i++)
            {
                float x = Mathf.Lerp(-hx + 0.3f, hx - 0.3f, i / 3f);
                float y = 0.75f * s - 0.25f * s * (1f - Mathf.Abs(x) / hx);
                b.Blob(new Vector3(x, y, 0.02f), new Vector3(0.13f, 0.13f, 0.13f), (i % 2) == 0 ? TownSwatch.ClothWhite : TownSwatch.LilacMist);
            }

            foreach (float x in new[] { -hx * 0.45f, hx * 0.5f })
            {
                b.Log(new Vector3(x, h - 0.12f, -0.05f), new Vector3(x, h - 0.5f, -0.05f), 0.01f, TownSwatch.Black, 3);
                b.Blob(new Vector3(x, h - 0.75f, -0.05f), new Vector3(0.12f, 0.45f, 0.08f), TownSwatch.Stone);
            }
        }

        /// <summary>A floating reed island: a round patch of bog water, a raft of bound rush, rush tufts and a will-o'-wisp over it (variant 1: a little reed hut on it).</summary>
        private static void ReedIsland(TownMeshBuilder b, float s, int variant)
        {
            float r = ReedIslandRadius * s;
            b.Disc(new Vector3(0f, 0.03f, 0f), r, 12, TownSwatch.BogWater);
            float raft = r * 0.62f;
            b.Cylinder(new Vector3(0f, 0.02f, 0f), raft, raft * 0.95f, 0.22f, 9, TownSwatch.Rush, TownSwatch.Reed);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 15f + variant * 20f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * raft * 0.8f;
                b.Crown(at + Vector3.up * 0.45f, new Vector3(0.5f, 0.75f, 0.5f) * s, (i % 2) == 0 ? TownSwatch.Rush : TownSwatch.SwampLeaf);
            }

            if ((variant % 2) == 1)
            {
                b.Cylinder(new Vector3(0f, 0.24f, 0f), raft * 0.55f, 0.05f, 1.3f * s, 7, TownSwatch.Rush, TownSwatch.Rush);
            }

            b.Blob(new Vector3(r * 0.3f, 1.3f * s, -r * 0.2f), new Vector3(0.24f, 0.24f, 0.24f) * s, TownSwatch.WillWisp);
            b.Disc(new Vector3(-r * 0.75f, 0.05f, r * 0.3f), 0.25f, 6, TownSwatch.SwampLeaf);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Larthia's front: a stack of rush baskets, one as tall as a cousin, bundles of rush leaning on the stair, a lantern basket of will-o'-wisps.</summary>
        private static void StiltVillageDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            // Stacked baskets, smaller on top.
            for (int i = 0; i < 3; i++)
            {
                float r = 0.36f - i * 0.07f;
                b.Cylinder(new Vector3(x - 0.45f, y + i * 0.34f, z - 0.4f), r * 0.85f, r, 0.32f, 8, (i % 2) == 0 ? TownSwatch.Rush : TownSwatch.Reed, TownSwatch.NetTwine);
            }

            // The tall basket and the rush bundles.
            b.Cylinder(new Vector3(x + 0.45f, y, z - 0.45f), 0.3f, 0.4f, 1.15f, 8, TownSwatch.Rush, TownSwatch.Black);
            b.Cylinder(new Vector3(x + 0.45f, y + 0.55f, z - 0.45f), 0.38f, 0.38f, 0.06f, 8, TownSwatch.WetWood);
            for (int i = 0; i < 3; i++)
            {
                float px = x - 0.1f + i * 0.2f;
                b.Log(new Vector3(px, y, z + 0.05f), new Vector3(px + 0.12f, y + 1.6f, z + 0.2f), 0.06f, (i % 2) == 0 ? TownSwatch.Rush : TownSwatch.SwampLeaf, 4);
            }

            // The lantern basket: a low basket full of caught wisps (the Cesto de Lanternas).
            Basket(b, new Vector3(x, y, z - 0.85f), TownSwatch.WillWisp);
        }
    }
}
