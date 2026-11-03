using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Jhilpur, the city in the roots of the Delta de Jhilpur (Docs/Mapas100.md §2.1 and map 31): the houses
    /// on root-columns (<see cref="RootStiltHouse"/>: a brick footing, twisted aerial-root columns, a plank floor raised over
    /// the murky water, woven-straw walls between root-bark posts and the curved "boat-back" thatch roof of the Bengal delta,
    /// with a plank stair down to the deck and lotus lanterns at the eaves), the Raiz-Mãe on its round brick platform with
    /// the Living Water at its foot, banyans with curtains of aerial roots, lotus ponds, the canals crossed by living-root
    /// bridges, the brick ghats stepping down into the water in the south and the root gate at the streets' ends; Nuhzadag's
    /// trade goods. Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/> (row 11 holds root green,
    /// ghat brick, straw, murky water, the Living Water, lotus lanterns, lotus pink and root bark), low-poly, deterministic.
    /// Architecture only as form (bíblia §0.5): no shrine, no rite, no deity of the region.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.MotherRoot"/>'s brick platform (× its scale): its collider.</summary>
        public const float MotherRootRadius = 2.4f;

        /// <summary>Radius of a <see cref="CityPropKind.Banyan"/>'s ring of aerial roots (× its scale): its collider.</summary>
        public const float BanyanRadius = 1.1f;

        /// <summary>Radius of a <see cref="CityPropKind.LotusPond"/>'s brick rim (× its scale): its collider.</summary>
        public const float LotusPondRadius = 1.5f;

        /// <summary>Half the thickness of a <see cref="CityPropKind.Ghat"/>'s parapet: its collider is the run inflated by this.</summary>
        public const float GhatHalfThickness = 0.5f;

        /// <summary>Half the width of the canal of a <see cref="CityPropKind.RootBridge"/>.</summary>
        public const float RootCanalHalfWidth = 0.8f;

        /// <summary>How high the plank floor of a root-column house stands over its footing.</summary>
        public const float StiltLift = 0.85f;

        // ------------------------------------------------------------------
        // The house on root-columns
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.RootStilt"/> house (local frame, front −z): a brick footing, twisted root columns with
        /// splayed buttress roots, the plank floor <see cref="StiltLift"/> above it, woven-straw walls between root-bark posts
        /// with two plaited bands, the door and lit shutters, the curved boat-back thatch roof whose ridge and eaves bow down
        /// at both ends, a plank stair from the deck to the door, a straw awning on root posts over the deck (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) and lotus lanterns at the eaves.
        /// </summary>
        public static void RootStiltHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            float floor = y0 + StiltLift;
            float wallTop = top + StiltLift * 0.6f;
            TownSwatch bark = TownSwatch.RootBark;

            // Brick footing and the murky water standing round the columns.
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade), new Vector3(hw, y0, zBack), spec.PlinthColor, TownSwatch.MurkWater);

            // Root columns: a twisted pair at each corner and one in the middle of each long side, buttress roots splayed out.
            float inX = hw - 0.25f;
            var columns = new[]
            {
                new Vector2(-inX, zFacade + 0.25f), new Vector2(inX, zFacade + 0.25f), new Vector2(-inX, zBack - 0.25f), new Vector2(inX, zBack - 0.25f),
                new Vector2(0f, zBack - 0.25f), new Vector2(-inX, (zFacade + zBack) * 0.5f), new Vector2(inX, (zFacade + zBack) * 0.5f),
            };
            for (int i = 0; i < columns.Length; i++)
            {
                Vector2 c = columns[i];
                float lean = (i % 2 == 0 ? 0.08f : -0.08f);
                b.Log(new Vector3(c.x - lean, y0, c.y), new Vector3(c.x + lean, floor, c.y), 0.13f, bark, 5);
                b.Log(new Vector3(c.x + lean + 0.1f, y0, c.y + 0.08f), new Vector3(c.x - lean + 0.05f, floor, c.y - 0.04f), 0.07f, TownSwatch.WoodDark, 4);
                if (i < 4)
                {
                    float sx = Mathf.Sign(c.x);
                    float sz = c.y < (zFacade + zBack) * 0.5f ? -1f : 1f;
                    b.Log(new Vector3(c.x, y0 + 0.35f, c.y), new Vector3(c.x + sx * 0.35f, 0.02f, c.y + sz * 0.3f), 0.06f, bark, 4);
                }
            }

            // The plank floor, a little wider than the walls.
            b.BoxMinMax(new Vector3(-hw - 0.08f, floor - 0.12f, zFacade - 0.05f), new Vector3(hw + 0.08f, floor, zBack + 0.08f), TownSwatch.WoodWeathered, TownSwatch.WoodLight);

            // Woven-straw walls between root-bark posts, two plaited bands.
            float wx = hw - 0.12f;
            float wzF = zFacade + 0.12f;
            float wzB = zBack - 0.1f;
            b.BoxMinMax(new Vector3(-wx, floor, wzF), new Vector3(wx, wallTop, wzB), spec.WallColor);
            foreach (float px in new[] { -wx, wx })
            {
                foreach (float pz in new[] { wzF, wzB })
                {
                    b.Log(new Vector3(px, floor, pz), new Vector3(px, wallTop + 0.1f, pz), 0.09f, spec.JoineryColor, 5);
                }
            }

            foreach (float band in new[] { floor + 0.45f, wallTop - 0.35f })
            {
                b.BoxMinMax(new Vector3(-wx - 0.02f, band - 0.06f, wzF - 0.03f), new Vector3(wx + 0.02f, band + 0.06f, wzB + 0.03f), spec.TrimColor);
            }

            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.3f : 0.9f;
            float doorTop = Mathf.Min(wallTop - 0.25f, floor + 1.75f);
            b.With(new Vector3(0f, 0f, wzF - 0.02f), 0f, () =>
            {
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, floor, -0.01f), new Vector3(doorX + doorW * 0.5f, doorTop, 0f), TownSwatch.Black);
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.12f, floor, -0.06f), new Vector3(doorX - doorW * 0.5f, doorTop + 0.1f, 0f), spec.JoineryColor);
                b.BoxMinMax(new Vector3(doorX + doorW * 0.5f, floor, -0.06f), new Vector3(doorX + doorW * 0.5f + 0.12f, doorTop + 0.1f, 0f), spec.JoineryColor);
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.15f, doorTop, -0.07f), new Vector3(doorX + doorW * 0.5f + 0.15f, doorTop + 0.14f, 0f), spec.JoineryColor);

                // A lit shutter each side of the door, where there is room.
                foreach (float side in new[] { -1f, 1f })
                {
                    float sx = doorX + side * (doorW * 0.5f + 0.85f);
                    if (Mathf.Abs(sx) < wx - 0.45f)
                    {
                        b.BoxMinMax(new Vector3(sx - 0.3f, floor + 0.85f, -0.02f), new Vector3(sx + 0.3f, floor + 1.4f, 0f), TownSwatch.WindowLit);
                        b.BoxMinMax(new Vector3(sx - 0.36f, floor + 0.8f, -0.05f), new Vector3(sx + 0.36f, floor + 0.86f, 0f), spec.JoineryColor);
                    }
                }
            });

            // The boat-back roof: ridge along x, both the ridge and the eaves bowed down towards the ends (do-chala).
            float ox = hw + 0.5f;
            float eaveF = zFacade - 0.45f;
            float eaveB = zBack + 0.45f;
            float ridgeZ = (zFacade + zBack) * 0.5f;
            float rise = Mathf.Min(1.9f, (zBack - zFacade) * 0.42f);
            const int segs = 6;
            for (int i = 0; i < segs; i++)
            {
                float x0 = -ox + 2f * ox * i / segs;
                float x1 = -ox + 2f * ox * (i + 1) / segs;
                float c0 = Bow(x0 / ox);
                float c1 = Bow(x1 / ox);
                var r0 = new Vector3(x0, wallTop + rise + c0, ridgeZ);
                var r1 = new Vector3(x1, wallTop + rise + c1, ridgeZ);
                var f0 = new Vector3(x0, wallTop - 0.1f + c0 * 0.8f, eaveF);
                var f1 = new Vector3(x1, wallTop - 0.1f + c1 * 0.8f, eaveF);
                var k0 = new Vector3(x0, wallTop - 0.1f + c0 * 0.8f, eaveB);
                var k1 = new Vector3(x1, wallTop - 0.1f + c1 * 0.8f, eaveB);
                TownSwatch thatch = (i % 2) == 0 ? spec.RoofColor : TownSwatch.Straw;
                b.Slab(f0, r0, r1, f1, 0.14f, thatch, TownSwatch.WoodDark);
                b.Slab(r0, k0, k1, r1, 0.14f, thatch, TownSwatch.WoodDark);
            }

            b.Log(new Vector3(-ox, wallTop + rise + Bow(-1f) + 0.06f, ridgeZ), new Vector3(0f, wallTop + rise + Bow(0f) + 0.1f, ridgeZ), 0.09f, bark, 5);
            b.Log(new Vector3(0f, wallTop + rise + Bow(0f) + 0.1f, ridgeZ), new Vector3(ox, wallTop + rise + Bow(1f) + 0.06f, ridgeZ), 0.09f, bark, 5);

            // Gable ends: a woven triangle under the roof at each side.
            foreach (float gx in new[] { -wx, wx })
            {
                b.TwoSidedQuad(new Vector3(gx, wallTop, wzF), new Vector3(gx, wallTop, wzB), new Vector3(gx, wallTop + rise * 0.8f, ridgeZ + 0.01f),
                    new Vector3(gx, wallTop + rise * 0.8f, ridgeZ - 0.01f), spec.WallColor);
            }

            // Lotus lanterns hanging from the front eave corners.
            foreach (float lx in new[] { -hw + 0.1f, hw - 0.1f })
            {
                float ey = wallTop - 0.1f + Bow(lx / ox) * 0.8f;
                b.Log(new Vector3(lx, ey, eaveF + 0.05f), new Vector3(lx, ey - 0.45f, eaveF + 0.05f), 0.012f, TownSwatch.Black, 3);
                b.Blob(new Vector3(lx, ey - 0.58f, eaveF + 0.05f), new Vector3(0.24f, 0.2f, 0.24f), TownSwatch.LotusLantern);
                b.Blob(new Vector3(lx, ey - 0.5f, eaveF + 0.05f), new Vector3(0.3f, 0.06f, 0.3f), TownSwatch.Lotus);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.WoodWeathered, TownSwatch.WoodLight);

                // The plank stair from the deck up to the door.
                const int steps = 3;
                float stairDepth = Mathf.Min(0.9f, (zFacade - zFront) * 0.6f);
                for (int i = 0; i < steps; i++)
                {
                    float y = Mathf.Lerp(deck, floor, (i + 1f) / steps);
                    float z0 = zFacade - stairDepth * (steps - i) / steps;
                    b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.05f, deck, z0), new Vector3(doorX + doorW * 0.5f + 0.05f, y, zFacade), TownSwatch.WoodWeathered, TownSwatch.WoodLight);
                }

                // Root posts and the straw awning over the deck.
                float z = zFront + TownBuildingBuilder.PostInset;
                float low = floor + 1.25f;
                float high = Mathf.Min(wallTop - 0.2f, floor + 1.9f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x + 0.04f, low + 0.05f, z), 0.08f, bark, 5);
                    b.Log(new Vector3(x + 0.12f, deck, z + 0.05f), new Vector3(x + 0.02f, low * 0.6f, z), 0.05f, TownSwatch.WoodDark, 4);
                }

                b.Slab(new Vector3(-hw - 0.15f, low, z - 0.25f), new Vector3(-hw - 0.15f, high, wzF), new Vector3(hw + 0.15f, high, wzF), new Vector3(hw + 0.15f, low, z - 0.25f),
                    0.08f, TownSwatch.Straw, TownSwatch.WoodDark);
            }
        }

        /// <summary>How much the boat-back roof bows at <paramref name="t"/> (−1 … 1 along the ridge): 0 at the middle, down at the ends.</summary>
        private static float Bow(float t) => -0.42f * t * t;

        /// <summary>The root gate of Jhilpur: two pillars of twisted aerial roots and an arch of living root over the road, leaves and lotus lanterns.</summary>
        public static void RootGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            Vector3 mid = (a + c) * 0.5f;
            float r = Vector3.Distance(a, c) * 0.5f;
            const float pier = 2.3f;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Cylinder(p, 0.5f, 0.42f, 0.3f, 8, TownSwatch.GhatBrick, TownSwatch.GhatBrick);
                for (int k = 0; k < 3; k++)
                {
                    float ang = k * 120f * Mathf.Deg2Rad;
                    var off = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * 0.18f;
                    b.Log(p + off + Vector3.up * 0.3f, p - off * 0.5f + Vector3.up * pier, 0.11f, k == 1 ? TownSwatch.WoodDark : TownSwatch.RootBark, 5);
                }
            }

            const int knots = 8;
            Vector3 prev = a + Vector3.up * pier;
            for (int i = 1; i <= knots; i++)
            {
                float t = Mathf.PI * i / knots;
                Vector3 p = mid - along * Mathf.Cos(t) * r + Vector3.up * (pier + Mathf.Sin(t) * r * 0.6f);
                b.Log(prev, p, 0.13f, TownSwatch.RootBark, 5);
                b.Log(prev + Vector3.up * 0.12f, p + Vector3.up * 0.08f, 0.06f, TownSwatch.WoodDark, 4);
                if ((i % 2) == 1)
                {
                    b.Crown(p + Vector3.up * 0.25f, new Vector3(0.7f, 0.45f, 0.7f), (i % 4) == 1 ? TownSwatch.RootGreen : TownSwatch.LeafDark);
                }

                prev = p;
            }

            foreach (float t in new[] { 0.3f, 0.7f })
            {
                Vector3 hang = Vector3.Lerp(a, c, t) + Vector3.up * (pier + Mathf.Sin(Mathf.PI * t) * r * 0.6f - 0.1f);
                b.Log(hang, hang + Vector3.down * 0.5f, 0.012f, TownSwatch.Black, 3);
                b.Blob(hang + Vector3.down * 0.62f, new Vector3(0.24f, 0.2f, 0.24f), TownSwatch.LotusLantern);
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The Raiz-Mãe: a round brick platform with a ring of Living Water at its foot, a trunk of many fused roots, great
        /// branches, a wide canopy and curtains of aerial roots dropping back to the platform. The tree is the city's heart
        /// (map 31; the Maré de Lodo event defends it); a tree, not a shrine.
        /// </summary>
        private static void MotherRoot(TownMeshBuilder b, float s)
        {
            float r = MotherRootRadius * s;
            b.Cylinder(Vector3.zero, r, r, 0.45f, 14, TownSwatch.GhatBrick, TownSwatch.Mud);
            b.Ring(new Vector3(0f, 0.46f, 0f), r - 0.55f, r - 0.2f, 14, TownSwatch.LivingWater);
            float trunkTop = 3.2f * s;
            for (int k = 0; k < 5; k++)
            {
                float a = (k * 72f + 10f) * Mathf.Deg2Rad;
                var off = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(off * 0.75f * s + Vector3.up * 0.45f, off * 0.25f * s + Vector3.up * trunkTop, 0.32f * s, (k % 2) == 0 ? TownSwatch.RootBark : TownSwatch.WoodDark, 6);
            }

            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 25f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 elbow = Vector3.up * trunkTop + dir * 1.6f * s + Vector3.up * (0.7f + (i % 2) * 0.4f) * s;
                b.Log(Vector3.up * (trunkTop - 0.3f), elbow, 0.2f * s, TownSwatch.RootBark, 5);
                b.Crown(elbow + Vector3.up * 0.7f * s + dir * 0.6f * s, new Vector3(2.4f, 1.4f, 2.4f) * s, (i % 2) == 0 ? TownSwatch.RootGreen : TownSwatch.LeafDark, i * 23f);

                // An aerial root from the branch back down to the platform (inside its rim, so the collider covers it).
                Vector3 foot = dir * (r - 0.35f) + Vector3.up * 0.45f;
                b.Log(elbow + Vector3.down * 0.1f, foot, 0.06f * s, TownSwatch.RootBark, 4);
            }

            b.Crown(Vector3.up * (trunkTop + 2.1f * s), new Vector3(3.2f, 1.8f, 3.2f) * s, TownSwatch.RootGreen);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * (r - 0.38f), 0.5f, Mathf.Sin(a) * (r - 0.38f)), new Vector3(0.2f, 0.12f, 0.2f), TownSwatch.LotusLantern);
            }
        }

        /// <summary>A banyan fig: a short thick trunk, aerial roots dropping in a ring round it (its collider), a wide low crown.</summary>
        private static void Banyan(TownMeshBuilder b, float s, int variant)
        {
            float r = BanyanRadius * s;
            float top = 3.0f * s;
            b.Log(Vector3.zero, Vector3.up * top, 0.38f * s, TownSwatch.RootBark, 6);
            for (int k = 0; k < 6; k++)
            {
                float a = (k * 60f + variant * 17f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * r * 0.85f, dir * r * 1.1f + Vector3.up * (top + 0.4f), 0.07f * s, (k % 2) == 0 ? TownSwatch.RootBark : TownSwatch.WoodDark, 4);
            }

            TownSwatch leaf = (variant % 2) == 0 ? TownSwatch.RootGreen : TownSwatch.LeafDark;
            b.Crown(Vector3.up * (top + 0.9f * s), new Vector3(3.6f, 1.5f, 3.4f) * s, leaf, variant * 31f);
            b.Crown(Vector3.up * (top + 1.6f * s) + new Vector3(0.6f, 0f, -0.4f) * s, new Vector3(2.2f, 1.2f, 2.2f) * s, (variant % 2) == 0 ? TownSwatch.LeafDark : TownSwatch.RootGreen);
        }

        /// <summary>A lotus pond: a brick rim, the murky water, lotus pads, pink flowers and one floating lotus lantern.</summary>
        private static void LotusPond(TownMeshBuilder b, float s)
        {
            float r = LotusPondRadius * s;
            b.Ring(new Vector3(0f, 0.25f, 0f), r - 0.25f, r, 12, TownSwatch.GhatBrick, TownSwatch.Brick);
            b.Cylinder(Vector3.zero, r, r, 0.25f, 12, TownSwatch.GhatBrick);
            b.Disc(new Vector3(0f, 0.18f, 0f), r - 0.24f, 12, TownSwatch.MurkWater);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 61f + 12f) * Mathf.Deg2Rad;
                float d = (r - 0.5f) * (0.35f + 0.6f * ((i * 37) % 10) / 10f);
                var at = new Vector3(Mathf.Cos(a) * d, 0.2f, Mathf.Sin(a) * d);
                b.Disc(at, 0.28f, 6, TownSwatch.RootGreen, i * 20f);
                if ((i % 2) == 0)
                {
                    b.Blob(at + new Vector3(0.08f, 0.12f, 0.05f), new Vector3(0.18f, 0.16f, 0.18f), TownSwatch.Lotus);
                }
            }

            b.Blob(new Vector3(0.2f, 0.28f, -0.25f), new Vector3(0.22f, 0.16f, 0.22f), TownSwatch.LotusLantern);
        }

        /// <summary>
        /// A canal from the origin to <paramref name="d"/> (a strip <see cref="RootCanalHalfWidth"/> each side of murky water,
        /// brick lips) and, at its middle, a living-root bridge across it: a deck of woven roots rising in a hump, two
        /// handrails of twisted root and leafy root knots at the four corners. Walkable.
        /// </summary>
        private static void RootBridge(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = RootCanalHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.06f, TownSwatch.MurkWater);
                foreach (float z in new[] { -w, w })
                {
                    b.BoxMinMax(new Vector3(0f, 0f, z - 0.12f), new Vector3(length, 0.12f, z + 0.12f), TownSwatch.GhatBrick, TownSwatch.Brick);
                }

                // The bridge across the canal at the middle (along local z): a humped deck of woven roots and two handrails.
                float xm = length * 0.5f;
                const int planks = 6;
                float bw = w + 0.5f;
                for (int i = 0; i < planks; i++)
                {
                    float z0 = -bw + 2f * bw * i / planks;
                    float z1 = z0 + 2f * bw / planks;
                    float h0 = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * i / planks);
                    float h1 = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * (i + 1) / planks);
                    b.Slab(new Vector3(xm - 0.75f, h0, z0), new Vector3(xm - 0.75f, h1, z1), new Vector3(xm + 0.75f, h1, z1), new Vector3(xm + 0.75f, h0, z0), 0.08f,
                        (i % 2) == 0 ? TownSwatch.RootBark : TownSwatch.WoodWeathered, TownSwatch.WoodDark);
                }

                foreach (float x in new[] { xm - 0.8f, xm + 0.8f })
                {
                    var prev = new Vector3(x, 0.65f, -bw);
                    for (int i = 1; i <= 4; i++)
                    {
                        float z = -bw + 2f * bw * i / 4f;
                        float h = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * i / 4f) + 0.57f;
                        var p = new Vector3(x, h, z);
                        b.Log(prev, p, 0.05f, TownSwatch.RootBark, 4);
                        prev = p;
                    }

                    foreach (float z in new[] { -bw, bw })
                    {
                        b.Log(new Vector3(x, 0f, z), new Vector3(x, 0.7f, z), 0.08f, TownSwatch.RootBark, 5);
                        b.Crown(new Vector3(x, 0.85f, z), new Vector3(0.45f, 0.3f, 0.45f), TownSwatch.RootGreen);
                    }
                }
            });
        }

        /// <summary>
        /// A brick ghat from the origin to <paramref name="d"/>: a low parapet with a coping (its collider is the run inflated
        /// by <see cref="GhatHalfThickness"/>) and, on the back side, broad brick steps going down into the murky water, lotus
        /// lanterns floating beyond.
        /// </summary>
        private static void Ghat(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float t = GhatHalfThickness;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.BoxMinMax(new Vector3(-0.05f, 0f, -t), new Vector3(length + 0.05f, 0.75f * s, t), TownSwatch.GhatBrick, TownSwatch.Brick);
                b.BoxMinMax(new Vector3(-0.1f, 0.75f * s, -t - 0.06f), new Vector3(length + 0.1f, 0.85f * s, t + 0.06f), TownSwatch.Paving);
                const int steps = 4;
                for (int i = 0; i < steps; i++)
                {
                    float y = 0.05f - i * 0.12f;
                    float z0 = t + i * 0.55f;
                    b.BoxMinMax(new Vector3(0f, y - 0.12f, z0), new Vector3(length, y, z0 + 0.55f), (i % 2) == 0 ? TownSwatch.GhatBrick : TownSwatch.Brick);
                }

                b.Flat(-0.5f, t + steps * 0.55f, length + 0.5f, t + steps * 0.55f + 6f, -0.3f, TownSwatch.MurkWater);
                b.Flat(-0.5f, t + 1.1f, length + 0.5f, t + steps * 0.55f, -0.28f, TownSwatch.MurkWater);
                int lanterns = Mathf.Max(1, Mathf.RoundToInt(length / 4f));
                for (int i = 0; i < lanterns; i++)
                {
                    float x = (i + 0.5f) * length / lanterns;
                    float z = t + steps * 0.55f + 0.8f + 1.4f * TownStyle.Hash01("ghat" + Mathf.RoundToInt(length), i);
                    b.Blob(new Vector3(x, -0.24f, z), new Vector3(0.3f, 0.06f, 0.3f), TownSwatch.Lotus);
                    b.Blob(new Vector3(x, -0.16f, z), new Vector3(0.18f, 0.14f, 0.18f), TownSwatch.LotusLantern);
                }
            });
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Nuhzadag's front: an upturned reed boat, bamboo balance poles in a rack, antivenom amulets on a cord, a basket of purifying fruit.</summary>
        private static void RootStiltDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            // The upturned reed boat on two trestles.
            b.Box(new Vector3(x, y + 0.2f, z - 0.75f), new Vector3(1.3f, 0.1f, 0.12f), TownSwatch.WoodDark);
            b.Blob(new Vector3(x, y + 0.36f, z - 0.75f), new Vector3(1.5f, 0.26f, 0.5f), TownSwatch.Straw);
            b.Log(new Vector3(x - 0.75f, y + 0.36f, z - 0.75f), new Vector3(x - 0.95f, y + 0.6f, z - 0.75f), 0.07f, TownSwatch.Reed, 4);

            // The rack of balance poles leaning on the wall.
            for (int i = 0; i < 3; i++)
            {
                float px = x - 0.4f + i * 0.35f;
                b.Log(new Vector3(px, y, z - 0.05f), new Vector3(px + 0.15f, y + 2.1f, z + 0.15f), 0.03f, (i % 2) == 0 ? TownSwatch.Reed : TownSwatch.Straw, 4);
            }

            // Amulets on a cord between two pegs, and the basket of purifying fruit.
            b.Log(new Vector3(x - 0.6f, y + 1.45f, z - 0.25f), new Vector3(x + 0.6f, y + 1.45f, z - 0.25f), 0.01f, TownSwatch.Black, 3);
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(x - 0.45f + i * 0.3f, y + 1.3f, z - 0.25f), new Vector3(0.09f, 0.12f, 0.05f), (i % 2) == 0 ? TownSwatch.LivingWater : TownSwatch.Turquoise);
            }

            Basket(b, new Vector3(x + 0.85f, y, z - 0.45f), TownSwatch.Lotus);
        }
    }
}
