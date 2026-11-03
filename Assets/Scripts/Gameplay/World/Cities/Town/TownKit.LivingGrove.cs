using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Arvoredo de Haripushpa, the trunk folk's grove (Docs/Mapas100.md §3.3 and map 50, style
    /// <c>LivingGrove</c>): the houses inside hollow giant trunks (<see cref="HollowTrunkHouse"/>: root flares, a tapering
    /// bark trunk with moss, an arched door into the dark of the hollow, nest windows, a crown of larch and new leaf, a deck of
    /// roots with two root posts under a leafy branch), the mossy standing stones of the circle, tall larches (one old one
    /// asleep, with a face in the bark), meltwater streams of Living Water with split-log bridges and the root-arch gate at the
    /// streets' ends; Suvarnapushpa's nursery. Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/>
    /// (row 18 holds bark, moss, new leaf, meltwater, flower, larch, lichen and the dark of a hollow), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of a <see cref="CityPropKind.MossStone"/>'s foot (× its scale): its collider.</summary>
        public const float MossStoneRadius = 0.45f;

        /// <summary>Radius of a <see cref="CityPropKind.Larch"/>'s trunk (× its scale): its collider.</summary>
        public const float LarchRadius = 0.5f;

        /// <summary>Half the width of the water of a <see cref="CityPropKind.MeltStream"/>.</summary>
        public const float MeltStreamHalfWidth = 0.6f;

        // ------------------------------------------------------------------
        // The hollow trunk house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.HollowTrunk"/> (local frame, front −z): five root flares (none in front of the door),
        /// the tapering bark trunk (<see cref="TownBuildingSpec.WallColor"/>) in two stages with moss patches
        /// (<see cref="TownBuildingSpec.TrimColor"/>), an arched door into the dark of the hollow with a bark frame, nest
        /// windows, a crown of larch and new leaf (<see cref="TownBuildingSpec.RoofColor"/>); in front a deck of roots with two
        /// root posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) curving into the trunk
        /// under a leafy branch, the sign hanging from it on two cords.
        /// </summary>
        public static void HollowTrunkHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float r = Mathf.Max(1.3f, Mathf.Min(hw - 0.3f, (zBack - zFacade) * 0.5f - 0.1f));
            float trunkH = Mathf.Max(3.4f, spec.WallHeight * 1.35f);
            TownSwatch bark = spec.WallColor;

            // Root flares round the foot (the front sector stays free for the door).
            for (int i = 0; i < 5; i++)
            {
                float a = (300f + i * 60f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(new Vector3(0f, 0.9f, cz) + dir * r * 0.8f, new Vector3(0f, 0.05f, cz) + dir * (r + 0.35f), 0.28f, bark, 5);
            }

            b.Cylinder(new Vector3(0f, 0f, cz), r, r * 0.86f, trunkH * 0.6f, 10, bark);
            b.Cylinder(new Vector3(0f, trunkH * 0.6f, cz), r * 0.86f, r * 0.62f, trunkH * 0.4f, 10, bark, TownSwatch.HollowDark);
            string key = spec.Seed + "trunk";
            for (int i = 0; i < 4; i++)
            {
                float a = (200f + 140f * TownStyle.Hash01(key, i)) * Mathf.Deg2Rad;
                float y = trunkH * (0.2f + 0.6f * TownStyle.Hash01(key, i + 9));
                float rr = Mathf.Lerp(r, r * 0.62f, y / trunkH) + 0.04f;
                b.Blob(new Vector3(Mathf.Cos(a) * rr, y, cz + Mathf.Sin(a) * rr), new Vector3(0.6f, 0.45f, 0.6f), spec.TrimColor);
            }

            // The crown: larch and new leaf, kept inside the lot.
            float crownY = trunkH + 0.6f;
            float cr = Mathf.Min(r * 1.25f, hw + 0.9f);
            b.Crown(new Vector3(0f, crownY, cz), new Vector3(cr * 2f, 2.0f, cr * 1.8f), spec.RoofColor);
            b.Crown(new Vector3(-cr * 0.45f, crownY + 0.9f, cz + 0.2f), new Vector3(cr * 1.1f, 1.4f, cr), TownSwatch.NewLeaf);
            b.Crown(new Vector3(cr * 0.4f, crownY + 1.2f, cz - 0.1f), new Vector3(cr, 1.5f, cr * 0.9f), spec.RoofColor);
            b.Cylinder(new Vector3(0f, crownY + 1.5f, cz), cr * 0.45f, 0f, 1.8f, 6, TownSwatch.Larch);

            // The arched door into the hollow, its bark frame, and the nest windows.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -r * 0.4f, r * 0.4f);
            float doorW = spec.WideDoor ? 1.3f : 1.0f;
            float doorZ = cz - Mathf.Sqrt(Mathf.Max(0.01f, r * r - doorX * doorX)) - 0.04f;
            float doorTop = 1.9f;
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.14f, 0f, doorZ - 0.05f), new Vector3(doorX + doorW * 0.5f + 0.14f, doorTop, doorZ + 0.25f), bark);
            ArchFill(b, doorX, doorTop, doorW + 0.28f, 0.5f, doorZ - 0.05f, bark);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, 0f, doorZ - 0.08f), new Vector3(doorX + doorW * 0.5f, doorTop, doorZ - 0.05f), TownSwatch.HollowDark);
            ArchFill(b, doorX, doorTop, doorW, 0.42f, doorZ - 0.08f, TownSwatch.HollowDark);
            b.Box(new Vector3(doorX, doorTop + 0.6f, doorZ - 0.1f), new Vector3(0.5f, 0.16f, 0.06f), spec.AccentColor, 0f);

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float a = (270f + side * 50f) * Mathf.Deg2Rad;
                float y = 2.5f + i * 0.4f;
                float rr = Mathf.Lerp(r, r * 0.86f, y / (trunkH * 0.6f)) + 0.02f;
                var at = new Vector3(Mathf.Cos(a) * rr, y, cz + Mathf.Sin(a) * rr);
                b.Blob(at, new Vector3(0.7f, 0.4f, 0.4f), TownSwatch.Straw);
                b.Box(at + new Vector3(0f, 0.05f, -0.12f), new Vector3(0.36f, 0.26f, 0.06f), TownSwatch.WindowLit, -side * 50f);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.GroveBark, TownSwatch.GroveMoss);
                float z = zFront + TownBuildingBuilder.PostInset;
                float branch = 2.5f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x * 0.9f, branch, z + 0.15f), 0.12f, bark, 5);
                    b.Log(new Vector3(x * 0.9f, branch, z + 0.15f), new Vector3(x * 0.5f, branch + 0.5f, zFacade + 0.2f), 0.1f, bark, 5);
                }

                b.Log(new Vector3(-hw + 0.1f, branch + 0.1f, z + 0.1f), new Vector3(hw - 0.1f, branch + 0.1f, z + 0.1f), 0.09f, bark, 5);
                b.Crown(new Vector3(-hw * 0.45f, branch + 0.45f, z + 0.3f), new Vector3(hw * 0.9f, 0.6f, 1.0f), TownSwatch.NewLeaf);
                b.Crown(new Vector3(hw * 0.45f, branch + 0.4f, z + 0.25f), new Vector3(hw * 0.9f, 0.55f, 0.9f), spec.RoofColor);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, branch - 0.45f, z - 0.05f);
                    b.Log(at + new Vector3(-0.42f, 0.14f, 0f), at + new Vector3(-0.42f, 0.55f, 0f), 0.012f, TownSwatch.Straw, 3);
                    b.Log(at + new Vector3(0.42f, 0.14f, 0f), at + new Vector3(0.42f, 0.55f, 0f), 0.012f, TownSwatch.Straw, 3);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.06f), TownSwatch.GroveBark, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.04f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>
        /// The gate of the grove at a street's end: two twisted root pillars curving up to meet overhead, a tuft of new leaf
        /// where they meet and a flower lantern hanging under it.
        /// </summary>
        public static void RootArchGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                float mid = w * 0.5f;
                foreach (float x in new[] { 0f, w })
                {
                    float inward = x > 0f ? -1f : 1f;
                    b.Log(new Vector3(x - inward * 0.35f, 0f, 0.2f), new Vector3(x, 1.0f, 0f), 0.28f, TownSwatch.GroveBark, 5);
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x + inward * 0.2f, 2.8f, 0f), 0.24f, TownSwatch.GroveBark, 5);
                    b.Log(new Vector3(x + inward * 0.2f, 2.8f, 0f), new Vector3(mid, 4.0f, 0f), 0.18f, TownSwatch.GroveBark, 5);
                    b.Blob(new Vector3(x + inward * 0.1f, 1.6f, -0.2f), new Vector3(0.5f, 0.6f, 0.3f), TownSwatch.GroveMoss);
                }

                b.Crown(new Vector3(mid, 4.3f, 0f), new Vector3(2.2f, 1.0f, 1.3f), TownSwatch.NewLeaf);
                b.Log(new Vector3(mid, 3.9f, 0f), new Vector3(mid, 3.3f, 0f), 0.015f, TownSwatch.Straw, 3);
                b.Blob(new Vector3(mid, 3.1f, 0f), new Vector3(0.32f, 0.38f, 0.32f), TownSwatch.GroveFlower);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>A mossy standing stone of the circle, lichen spots and a cap of moss (Variant 1: leaning, ringed with flowers).</summary>
        private static void MossStone(TownMeshBuilder b, float s, int variant)
        {
            float r = MossStoneRadius * s;
            Vector3 top = (variant % 2) == 1 ? new Vector3(0.35f * s, 1.7f * s, 0.1f * s) : new Vector3(0f, 2.0f * s, 0f);
            b.Log(Vector3.zero, top, r * 0.85f, TownSwatch.Stone, 5, TownSwatch.GroveMoss);
            b.Blob(top + new Vector3(0f, 0.05f, 0f), new Vector3(r * 1.9f, 0.3f * s, r * 1.9f), TownSwatch.GroveMoss);
            b.Blob(new Vector3(0.05f, 0.8f * s, -r * 0.8f), new Vector3(0.3f, 0.25f, 0.08f) * s, TownSwatch.Lichen);
            b.Blob(new Vector3(0f, 0.12f * s, 0f), new Vector3(r * 2.4f, 0.25f * s, r * 2.4f), TownSwatch.GroveMoss);
            if ((variant % 2) == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 1.4f) + Vector3.up * 0.12f, new Vector3(0.18f, 0.14f, 0.18f) * s, TownSwatch.GroveFlower);
                }
            }
        }

        /// <summary>
        /// A tall larch of the taiga: a tapering trunk and four tiers of soft needles, larch green and new leaf in turn
        /// (Variant 1: an old one of the trunk folk asleep, with closed eyes and a mouth in the bark and a beard of moss).
        /// </summary>
        private static void Larch(TownMeshBuilder b, float s, int variant)
        {
            float r = LarchRadius * s;
            b.Cylinder(Vector3.zero, r * 0.8f, r * 0.25f, 6.5f * s, 6, TownSwatch.GroveBark);
            for (int i = 0; i < 4; i++)
            {
                float y = (2.0f + i * 1.15f) * s;
                float rr = (1.9f - i * 0.38f) * s;
                b.Cylinder(new Vector3(0f, y, 0f), rr, 0f, 1.7f * s, 7, (i & 1) == 0 ? TownSwatch.Larch : TownSwatch.NewLeaf, null, i * 0.3f);
            }

            if ((variant % 2) == 1)
            {
                float z = -r * 0.72f;
                b.Box(new Vector3(-0.12f * s, 1.45f * s, z), new Vector3(0.14f, 0.03f, 0.04f) * s, TownSwatch.HollowDark, 0f);
                b.Box(new Vector3(0.12f * s, 1.45f * s, z), new Vector3(0.14f, 0.03f, 0.04f) * s, TownSwatch.HollowDark, 0f);
                b.Box(new Vector3(0f, 1.15f * s, z), new Vector3(0.2f, 0.05f, 0.04f) * s, TownSwatch.HollowDark, 0f);
                b.Blob(new Vector3(0f, 0.85f * s, z - 0.02f), new Vector3(0.35f, 0.45f, 0.12f) * s, TownSwatch.GroveMoss);
            }
        }

        /// <summary>
        /// A meltwater stream from the origin to <paramref name="d"/> (water <see cref="MeltStreamHalfWidth"/> each side between
        /// mossy stones) crossed at its middle by a bridge of two split logs. Walkable: the logs stay under the walker's step.
        /// </summary>
        private static void MeltStream(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = MeltStreamHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.04f, TownSwatch.MeltWater);
                int stones = Mathf.Max(2, Mathf.RoundToInt(length / 1.2f));
                for (int i = 0; i < stones; i++)
                {
                    float x = length * (i + 0.5f) / stones;
                    if (Mathf.Abs(x - length * 0.5f) < 1.0f)
                    {
                        continue;
                    }

                    b.Blob(new Vector3(x, 0.06f, -w - 0.12f), new Vector3(0.4f, 0.16f, 0.3f), TownSwatch.Stone);
                    b.Blob(new Vector3(x + 0.3f, 0.06f, w + 0.12f), new Vector3(0.35f, 0.14f, 0.3f), TownSwatch.GroveMoss);
                }

                float xm = length * 0.5f;
                foreach (float dx in new[] { -0.22f, 0.22f })
                {
                    b.Log(new Vector3(xm + dx, 0.1f, -w - 0.45f), new Vector3(xm + dx, 0.1f, w + 0.45f), 0.11f, TownSwatch.GroveBark, 5, TownSwatch.Lichen);
                }
            });
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Suvarnapushpa's front: two rows of seedlings in bark pots, a watering gourd and a young larch in a tub.</summary>
        private static void LivingGroveDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            for (int row = 0; row < 2; row++)
            {
                for (int i = 0; i < 3; i++)
                {
                    var at = new Vector3(x - 0.4f + i * 0.4f, y, z - 0.45f - row * 0.4f);
                    b.Cylinder(at, 0.12f, 0.15f, 0.2f, 6, TownSwatch.GroveBark, TownSwatch.DirtDark);
                    b.Blob(at + new Vector3(0f, 0.32f, 0f), new Vector3(0.22f, 0.22f, 0.22f), (i + row) % 2 == 0 ? TownSwatch.NewLeaf : TownSwatch.GroveFlower);
                }
            }

            b.Blob(new Vector3(x + 0.85f, y + 0.18f, z - 0.4f), new Vector3(0.3f, 0.36f, 0.3f), TownSwatch.Straw);
            b.Log(new Vector3(x + 0.85f, y + 0.34f, z - 0.4f), new Vector3(x + 0.7f, y + 0.5f, z - 0.55f), 0.03f, TownSwatch.Straw, 4);
            b.Cylinder(new Vector3(x - 1.0f, y, z - 0.5f), 0.25f, 0.3f, 0.3f, 8, TownSwatch.GroveBark, TownSwatch.DirtDark);
            b.Cylinder(new Vector3(x - 1.0f, y + 0.3f, z - 0.5f), 0.04f, 0.03f, 0.5f, 4, TownSwatch.GroveBark);
            b.Cylinder(new Vector3(x - 1.0f, y + 0.55f, z - 0.5f), 0.35f, 0f, 0.8f, 6, TownSwatch.Larch);
        }
    }
}
