using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Vale dos Sátiros, race city of the satyrs (Docs/Mapas100.md §3.3 and map 53, style <c>Glade</c>):
    /// the gourd-shaped wicker houses (<see cref="GourdHutHouse"/>: a ring of stones, a swelling wicker body with woven bands,
    /// a narrower neck, a leafy cap with a stalk, a round door with a flower garland, round windows and a vine pergola in
    /// front), the river of golden pebbles with its ford of flat stones (the Pactolus, where Midas washes his hands), rows of
    /// grape vines on posts, the round stone stages of the dance, broad oaks (one with a double flute hung on a branch) and the
    /// leaf-tunnel gate at the streets' ends; Marsyas' flute stand. Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (row 20 holds glade green, wicker, stage stone, the gold of the river, flower, grape, vine
    /// leaf and the fireflies), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of a <see cref="CityPropKind.VineRow"/>: its collider is the run inflated by this.</summary>
        public const float VineRowHalfThickness = 0.35f;

        /// <summary>Radius of a <see cref="CityPropKind.StoneStage"/> (× its scale): its collider.</summary>
        public const float StoneStageRadius = 1.8f;

        /// <summary>Radius of a <see cref="CityPropKind.GroveOak"/>'s trunk (× its scale): its collider.</summary>
        public const float GroveOakRadius = 0.6f;

        /// <summary>Half the width of the water of a <see cref="CityPropKind.GoldenRiver"/>.</summary>
        public const float GoldenRiverHalfWidth = 0.75f;

        // ------------------------------------------------------------------
        // The gourd hut
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.GourdHut"/> (local frame, front −z): a ring of stage stones at the foot, the wicker
        /// body (<see cref="TownBuildingSpec.WallColor"/>) swelling out and narrowing into a neck, two woven bands of straw, a
        /// leafy cap (<see cref="TownBuildingSpec.RoofColor"/>) with a curled stalk, a round door with a garland of flowers
        /// (<see cref="TownBuildingSpec.TrimColor"/>), round lit windows; in front a vine pergola on two posts (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) with leaves and grapes, the sign hanging
        /// from its beam.
        /// </summary>
        public static void GourdHutHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float r = Mathf.Max(1.3f, Mathf.Min(hw - 0.2f, (zBack - zFacade) * 0.5f - 0.05f));
            float h = Mathf.Max(3.0f, spec.WallHeight * 1.1f);
            TownSwatch wicker = spec.WallColor;

            // The ring of stones and the gourd body: swelling, widest a third up, then the neck.
            b.Ring(new Vector3(0f, 0.08f, cz), r * 0.85f, r * 1.02f, 10, TownSwatch.GladeStone, TownSwatch.Stone);
            float y0 = 0f, y1 = h * 0.18f, y2 = h * 0.45f, y3 = h * 0.72f, y4 = h * 0.86f;
            b.Cylinder(new Vector3(0f, y0, cz), r * 0.82f, r, y1 - y0, 10, wicker);
            b.Cylinder(new Vector3(0f, y1, cz), r, r * 0.92f, y2 - y1, 10, wicker);
            b.Cylinder(new Vector3(0f, y2, cz), r * 0.92f, r * 0.5f, y3 - y2, 10, wicker);
            b.Cylinder(new Vector3(0f, y3, cz), r * 0.5f, r * 0.42f, y4 - y3, 8, wicker);
            b.Cylinder(new Vector3(0f, y1 - 0.06f, cz), r * 1.01f, r * 1.01f, 0.12f, 10, TownSwatch.Straw);
            b.Cylinder(new Vector3(0f, y2 - 0.06f, cz), r * 0.93f, r * 0.93f, 0.12f, 10, TownSwatch.Straw);

            // The leafy cap and the curled stalk.
            b.Crown(new Vector3(0f, y4 + 0.1f, cz), new Vector3(r * 1.3f, 0.6f, r * 1.3f), spec.RoofColor);
            b.Log(new Vector3(0f, y4 + 0.3f, cz), new Vector3(0.12f, y4 + 0.95f, cz + 0.05f), 0.08f, TownSwatch.VineLeaf, 5);
            b.Log(new Vector3(0.12f, y4 + 0.95f, cz + 0.05f), new Vector3(0.38f, y4 + 1.05f, cz), 0.05f, TownSwatch.VineLeaf, 4);

            // The round door with its garland of flowers.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -r * 0.4f, r * 0.4f);
            float doorW = spec.WideDoor ? 1.2f : 0.95f;
            float doorZ = cz - Mathf.Sqrt(Mathf.Max(0.01f, r * r - doorX * doorX)) - 0.04f;
            float doorTop = 1.6f;
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, 0f, doorZ - 0.06f), new Vector3(doorX + doorW * 0.5f, doorTop, doorZ + 0.2f), TownSwatch.WoodDark);
            ArchFill(b, doorX, doorTop, doorW, doorW * 0.45f, doorZ - 0.06f, TownSwatch.WoodDark);
            for (int i = 0; i < 7; i++)
            {
                float a = Mathf.Lerp(0f, 180f, i / 6f) * Mathf.Deg2Rad;
                var at = new Vector3(doorX + Mathf.Cos(a) * (doorW * 0.55f + 0.08f), doorTop + Mathf.Sin(a) * (doorW * 0.5f) - 0.05f, doorZ - 0.1f);
                b.Blob(at, new Vector3(0.16f, 0.16f, 0.1f), (i & 1) == 0 ? spec.TrimColor : TownSwatch.VineLeaf);
            }

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float a = (270f + side * 48f) * Mathf.Deg2Rad;
                float y = h * 0.36f;
                var at = new Vector3(Mathf.Cos(a) * r * 0.97f, y, cz + Mathf.Sin(a) * r * 0.97f);
                b.Blob(at, new Vector3(0.5f, 0.5f, 0.3f), TownSwatch.Straw);
                b.Blob(at + new Vector3(0f, 0f, -0.06f), new Vector3(0.34f, 0.34f, 0.2f), TownSwatch.WindowLit);
            }

            if (spec.PorchDepth > 0.01f)
            {
                // The vine pergola: two posts, a beam, leaves and grapes, and the sign hanging from it.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.GladeStone, TownSwatch.GladeStone);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beam = 2.4f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, beam, z), 0.08f, spec.JoineryColor, 5);
                    b.Log(new Vector3(x, beam, z), new Vector3(x * 0.8f, beam + 0.1f, zFacade + 0.1f), 0.06f, spec.JoineryColor, 4);
                    b.Log(new Vector3(x, deck, z - 0.02f), new Vector3(x + 0.05f, beam, z - 0.04f), 0.035f, TownSwatch.VineLeaf, 4);
                }

                b.Log(new Vector3(-hw + 0.1f, beam, z), new Vector3(hw - 0.1f, beam, z), 0.07f, spec.JoineryColor, 5);
                b.Crown(new Vector3(-hw * 0.45f, beam + 0.25f, (z + zFacade) * 0.5f), new Vector3(hw * 0.95f, 0.45f, zFacade - z + 0.6f), TownSwatch.VineLeaf);
                b.Crown(new Vector3(hw * 0.45f, beam + 0.22f, (z + zFacade) * 0.5f), new Vector3(hw * 0.95f, 0.4f, zFacade - z + 0.5f), spec.RoofColor);
                foreach (float x in new[] { -hw * 0.6f, -hw * 0.15f, hw * 0.35f, hw * 0.7f })
                {
                    b.Blob(new Vector3(x, beam - 0.2f, z + 0.1f), new Vector3(0.18f, 0.3f, 0.18f), TownSwatch.Grape);
                }

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beam - 0.45f, z - 0.05f);
                    b.Log(at + new Vector3(-0.42f, 0.14f, 0f), at + new Vector3(-0.42f, 0.45f, 0f), 0.012f, TownSwatch.Straw, 3);
                    b.Log(at + new Vector3(0.42f, 0.14f, 0f), at + new Vector3(0.42f, 0.45f, 0f), 0.012f, TownSwatch.Straw, 3);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.06f), TownSwatch.Wicker, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.04f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>
        /// The gate of the vale at a street's end: a tunnel of foliage, two vine-wrapped posts with three arches of leaves over
        /// the way, flowers in the leaves and a firefly lantern hanging under the middle arch.
        /// </summary>
        public static void LeafTunnelGate(TownMeshBuilder b, Vector3 a, Vector3 c)
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
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x, 3.0f, 0f), 0.14f, TownSwatch.WoodWeathered, 5);
                    b.Blob(new Vector3(x, 1.2f, 0f), new Vector3(0.45f, 1.2f, 0.45f), TownSwatch.VineLeaf);
                    b.Blob(new Vector3(x, 2.6f, 0f), new Vector3(0.5f, 0.8f, 0.5f), TownSwatch.GladeGreen);
                }

                foreach (float z in new[] { -0.6f, 0f, 0.6f })
                {
                    b.Crown(new Vector3(mid, 3.3f, z), new Vector3(w + 0.8f, 0.9f, 0.6f), z == 0f ? TownSwatch.VineLeaf : TownSwatch.GladeGreen);
                }

                for (int i = 0; i < 5; i++)
                {
                    b.Blob(new Vector3(w * (i + 0.5f) / 5f, 3.05f, -0.35f), new Vector3(0.18f, 0.16f, 0.12f), (i & 1) == 0 ? TownSwatch.VineFlower : TownSwatch.Grape);
                }

                b.Log(new Vector3(mid, 3.0f, 0f), new Vector3(mid, 2.55f, 0f), 0.015f, TownSwatch.Straw, 3);
                b.Blob(new Vector3(mid, 2.4f, 0f), new Vector3(0.28f, 0.34f, 0.28f), TownSwatch.Firefly);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The river of golden pebbles from the origin to <paramref name="d"/> (the Living Water <see cref="GoldenRiverHalfWidth"/>
        /// each side, gold pebbles along both banks) crossed at its middle by a ford of flat stage stones. Walkable: the ford
        /// stays under the walker's step.
        /// </summary>
        private static void GoldenRiver(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = GoldenRiverHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.03f, TownSwatch.LivingWater);
                b.Flat(0f, -w * 0.4f, length, w * 0.4f, 0.035f, TownSwatch.PactolusGold);
                int pebbles = Mathf.Max(2, Mathf.RoundToInt(length / 0.9f));
                for (int i = 0; i < pebbles; i++)
                {
                    float x = length * (i + 0.5f) / pebbles;
                    if (Mathf.Abs(x - length * 0.5f) < 1.0f)
                    {
                        continue;
                    }

                    b.Blob(new Vector3(x, 0.05f, -w - 0.1f), new Vector3(0.3f, 0.12f, 0.24f), TownSwatch.PactolusGold);
                    b.Blob(new Vector3(x + 0.25f, 0.05f, w + 0.1f), new Vector3(0.28f, 0.12f, 0.22f), (i & 1) == 0 ? TownSwatch.GladeStone : TownSwatch.PactolusGold);
                }

                float xm = length * 0.5f;
                foreach (float dz in new[] { -w * 0.7f, 0f, w * 0.7f })
                {
                    b.Cylinder(new Vector3(xm - 0.3f, 0f, dz), 0.42f, 0.4f, 0.1f, 7, TownSwatch.GladeStone);
                    b.Cylinder(new Vector3(xm + 0.35f, 0f, dz + 0.12f), 0.38f, 0.36f, 0.1f, 7, TownSwatch.GladeStone);
                }
            });
        }

        /// <summary>
        /// A row of grape vines from the origin to <paramref name="d"/>: a post every metre and a half, two wires, the leafy vines
        /// along them and bunches of grapes hanging.
        /// </summary>
        private static void VineRow(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                int posts = Mathf.Max(2, Mathf.RoundToInt(length / 1.5f) + 1);
                for (int i = 0; i < posts; i++)
                {
                    float x = length * i / (posts - 1);
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x, 1.5f * s, 0f), 0.06f, TownSwatch.WoodWeathered, 4);
                }

                foreach (float y in new[] { 0.8f, 1.35f })
                {
                    b.Log(new Vector3(0f, y * s, 0f), new Vector3(length, y * s, 0f), 0.012f, TownSwatch.Iron, 3);
                }

                int leaves = Mathf.Max(2, Mathf.RoundToInt(length / 0.9f));
                for (int i = 0; i < leaves; i++)
                {
                    float x = length * (i + 0.5f) / leaves;
                    b.Crown(new Vector3(x, 1.1f * s, 0f), new Vector3(1.0f, 0.7f * s, 0.55f), (i & 1) == 0 ? TownSwatch.VineLeaf : TownSwatch.GladeGreen);
                    b.Blob(new Vector3(x + 0.2f, 0.75f * s, -0.2f), new Vector3(0.16f, 0.26f, 0.16f), TownSwatch.Grape);
                }
            });
        }

        /// <summary>
        /// The round stone stage of the dance: two tiers of stage stone with a gold rim, a flute stand at the back and a ring of
        /// flowers (Variant 1: a small one, a single tier).
        /// </summary>
        private static void StoneStage(TownMeshBuilder b, float s, int variant)
        {
            float r = StoneStageRadius * s;
            bool small = (variant % 2) == 1;
            b.Cylinder(Vector3.zero, r, r * 0.97f, 0.22f, 12, TownSwatch.GladeStone, TownSwatch.GladeStone);
            b.Ring(new Vector3(0f, 0.225f, 0f), r * 0.9f, r * 0.97f, 12, TownSwatch.PactolusGold, TownSwatch.GladeStone);
            float topY = 0.22f;
            if (!small)
            {
                b.Cylinder(new Vector3(0f, 0.22f, 0f), r * 0.72f, r * 0.7f, 0.2f, 12, TownSwatch.GladeStone, TownSwatch.Stone);
                topY = 0.42f;
            }

            b.Log(new Vector3(0f, topY, r * 0.5f), new Vector3(0f, topY + 1.0f, r * 0.5f), 0.04f, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(-0.25f, topY + 0.9f, r * 0.5f), new Vector3(0.25f, topY + 0.9f, r * 0.5f), 0.03f, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(-0.1f, topY + 0.9f, r * 0.5f - 0.03f), new Vector3(-0.18f, topY + 0.45f, r * 0.5f - 0.05f), 0.025f, TownSwatch.Wicker, 4);
            b.Log(new Vector3(0.1f, topY + 0.9f, r * 0.5f - 0.03f), new Vector3(0.18f, topY + 0.45f, r * 0.5f - 0.05f), 0.025f, TownSwatch.Wicker, 4);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.98f) + Vector3.up * 0.25f, new Vector3(0.2f, 0.16f, 0.2f), (i & 1) == 0 ? TownSwatch.VineFlower : TownSwatch.Firefly);
            }
        }

        /// <summary>
        /// A broad oak of the grove: a stout trunk, two low branches and a wide crown in three greens (Variant 1: a double flute
        /// and a ribbon hung on the low branch).
        /// </summary>
        private static void GroveOak(TownMeshBuilder b, float s, int variant)
        {
            float r = GroveOakRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.65f, 2.6f * s, 7, TownSwatch.Trunk);
            b.Log(new Vector3(0f, 2.0f * s, 0f), new Vector3(1.4f * s, 2.9f * s, 0.2f * s), 0.16f * s, TownSwatch.Trunk, 5);
            b.Log(new Vector3(0f, 2.3f * s, 0f), new Vector3(-1.2f * s, 3.2f * s, -0.3f * s), 0.15f * s, TownSwatch.Trunk, 5);
            b.Crown(new Vector3(0f, 3.4f * s, 0f), new Vector3(3.6f, 1.8f, 3.2f) * s, TownSwatch.LeafDark);
            b.Crown(new Vector3(1.0f * s, 3.9f * s, 0.2f * s), new Vector3(2.4f, 1.4f, 2.2f) * s, TownSwatch.VineLeaf);
            b.Crown(new Vector3(-0.9f * s, 4.1f * s, -0.2f * s), new Vector3(2.2f, 1.4f, 2.0f) * s, TownSwatch.GladeGreen);
            if ((variant % 2) == 1)
            {
                var at = new Vector3(0.9f * s, 2.55f * s, 0.12f * s);
                b.Log(at, at + new Vector3(0f, -0.3f, 0f), 0.01f, TownSwatch.Straw, 3);
                b.Log(at + new Vector3(-0.05f, -0.3f, 0f), at + new Vector3(-0.12f, -0.75f, -0.02f), 0.02f, TownSwatch.Wicker, 4);
                b.Log(at + new Vector3(0.05f, -0.3f, 0f), at + new Vector3(0.12f, -0.75f, -0.02f), 0.02f, TownSwatch.Wicker, 4);
                b.Box(at + new Vector3(0.25f, -0.4f, 0f), new Vector3(0.06f, 0.5f, 0.02f), TownSwatch.VineFlower, 10f);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Marsyas' front: double flutes on a stand, a pan-pipe, a small drum and a basket of grapes.</summary>
        private static void GladeDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Log(new Vector3(x - 0.9f, y, z - 0.4f), new Vector3(x - 0.9f, y + 1.1f, z - 0.4f), 0.03f, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(x - 1.2f, y + 1.0f, z - 0.4f), new Vector3(x - 0.6f, y + 1.0f, z - 0.4f), 0.025f, TownSwatch.WoodWeathered, 4);
            foreach (float dx in new[] { -0.2f, 0.2f })
            {
                b.Log(new Vector3(x - 0.9f + dx - 0.04f, y + 1.0f, z - 0.43f), new Vector3(x - 0.9f + dx - 0.1f, y + 0.5f, z - 0.45f), 0.018f, TownSwatch.Wicker, 4);
                b.Log(new Vector3(x - 0.9f + dx + 0.04f, y + 1.0f, z - 0.43f), new Vector3(x - 0.9f + dx + 0.1f, y + 0.5f, z - 0.45f), 0.018f, TownSwatch.Wicker, 4);
            }

            for (int i = 0; i < 5; i++)
            {
                b.Cylinder(new Vector3(x + 0.2f + i * 0.06f, y + 0.5f, z - 0.4f), 0.025f, 0.025f, 0.35f - i * 0.05f, 5, TownSwatch.Wicker);
            }

            b.Box(new Vector3(x + 0.32f, y + 0.25f, z - 0.4f), new Vector3(0.4f, 0.5f, 0.3f), TownSwatch.WoodWeathered, 0f);
            b.Cylinder(new Vector3(x + 0.95f, y, z - 0.45f), 0.22f, 0.22f, 0.32f, 8, TownSwatch.Wicker, TownSwatch.Ribbon);
            b.Cylinder(new Vector3(x + 0.95f, y + 0.55f, z - 0.45f), 0.2f, 0.22f, 0.2f, 8, TownSwatch.Wicker, TownSwatch.Wicker);
            b.Blob(new Vector3(x + 0.95f, y + 0.8f, z - 0.45f), new Vector3(0.3f, 0.22f, 0.3f), TownSwatch.Grape);
        }
    }
}
