using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 5 — Costa das Monções: the coast pieces (low-poly, the shared palette; each built round the origin and placed
    /// by the chunk builder): a dhow beached in the sand with its mast down, a <i>wema</i> fish trap of stakes, nets drying on
    /// poles, coral heads of the reef, runs of ruined coral-stone wall, the arches and pillar tombs of Gedi and Kilwa,
    /// flat-topped acacias and termite mounds of the savanna, cloves drying on mats, wrack lines on the sand and the striped
    /// posts that mark a ford (baixio). The mangroves are the town kit's.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A coast piece (Região 5, 120–139).</summary>
        public static bool IsCoastDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 120 && i < 140;
        }

        /// <summary>A dhow keeled into the sand: low hull, the lateen yard and mast lying across it, the sail bundled.</summary>
        public static void BeachedDhow(TownMeshBuilder b, float s, int variant)
        {
            const int segments = 6;
            float l = 6f * s;
            float beam = 1.9f * s;
            float seg = l / segments;
            for (int i = 0; i < segments; i++)
            {
                float t = (i + 0.5f) / segments * 2f - 1f;
                float width = beam * Mathf.Sqrt(Mathf.Max(0.1f, 1f - t * t * 0.9f));
                float z = -l * 0.5f + (i + 0.5f) * seg;
                float h = (0.55f + t * t * 0.35f) * s;
                b.Box(new Vector3(0f, h * 0.5f - 0.25f * s, z), new Vector3(width, h, seg + 0.02f), (i & 1) == 0 ? TownSwatch.Teak : TownSwatch.WoodDark, 0f,
                    TownSwatch.WoodWeathered);
            }

            // The stem post and a band of paint along the gunwale.
            b.Log(new Vector3(0f, 0.3f * s, l * 0.5f), new Vector3(0f, 1.3f * s, l * 0.62f), 0.1f * s, TownSwatch.Teak, 4);
            b.Box(new Vector3(0f, 0.62f * s, 0f), new Vector3(beam * 1.02f, 0.08f * s, l * 0.7f), (variant & 1) == 0 ? TownSwatch.Sea : TownSwatch.Laterite);

            // The mast down across the hull and onto the sand, the sail rolled on its yard.
            float side = (variant & 2) == 0 ? 1f : -1f;
            b.Log(new Vector3(0f, 0.7f * s, -0.6f * s), new Vector3(side * 3.4f * s, 0.12f * s, 1.4f * s), 0.11f * s, TownSwatch.WoodLight, 5);
            b.Log(new Vector3(-side * 0.6f * s, 0.75f * s, 1.6f * s), new Vector3(side * 2.8f * s, 0.2f * s, -2.4f * s), 0.2f * s, TownSwatch.Sail, 5);
        }

        /// <summary>A wema fish trap: a heart of stakes with a palm-rib fence between them, open towards the land.</summary>
        public static void FishTrap(TownMeshBuilder b, float s, int variant)
        {
            const int stakes = 9;
            Vector3 previous = Vector3.zero;
            for (int i = 0; i <= stakes; i++)
            {
                float a = Mathf.Lerp(-150f, 150f, i / (float)stakes) * Mathf.Deg2Rad;
                float r = (2.2f - 0.6f * Mathf.Abs(Mathf.Sin(a * 0.5f))) * s;
                var p = new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
                b.Log(p + Vector3.down * 0.2f * s, p + Vector3.up * (0.9f + 0.15f * ((i + variant) % 3)) * s, 0.06f * s, TownSwatch.WoodWeathered, 4);
                if (i > 0)
                {
                    Run(b, previous + Vector3.up * 0.1f * s, p + Vector3.up * 0.1f * s, 0.05f * s, 0f, 0.55f * s, TownSwatch.Reed, TownSwatch.ReedMat);
                }

                previous = p;
            }
        }

        /// <summary>Nets drying between two poles, a float of cork and a basket at the foot.</summary>
        public static void NetRack(TownMeshBuilder b, float s, int variant)
        {
            var left = new Vector3(-1.6f * s, 0f, 0f);
            var right = new Vector3(1.6f * s, 0f, 0f);
            b.Log(left + Vector3.down * 0.3f * s, left + Vector3.up * 2.1f * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            b.Log(right + Vector3.down * 0.3f * s, right + Vector3.up * 2.1f * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            TownSwatch net = (variant & 1) == 0 ? TownSwatch.ReedMat : TownSwatch.Sail;
            b.TwoSidedQuad(left + Vector3.up * 2f * s, right + Vector3.up * 2f * s, right + Vector3.up * 0.7f * s + Vector3.forward * 0.15f * s,
                left + Vector3.up * 0.5f * s + Vector3.forward * 0.15f * s, net);
            b.Blob(new Vector3(0.4f * s, 0.25f * s, 0.5f * s), new Vector3(0.45f, 0.45f, 0.45f) * s, TownSwatch.Reed);
            b.Blob(new Vector3(-0.6f * s, 1.3f * s, 0.12f * s), new Vector3(0.14f, 0.14f, 0.14f) * s, TownSwatch.ClothRed);
        }

        /// <summary>A coral head: two or three rounded boulders of live (or dimmed) coral, a fan on top.</summary>
        public static void CoralHead(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch main = (variant & 1) == 0 ? TownSwatch.CoralLive : TownSwatch.CoralDim;
            b.Crown(new Vector3(0f, 0.35f * s, 0f), new Vector3(1.3f, 0.75f, 1.1f) * s, main, variant * 37f);
            b.Crown(new Vector3(0.7f * s, 0.25f * s, 0.4f * s), new Vector3(0.7f, 0.5f, 0.6f) * s, (variant & 2) == 0 ? TownSwatch.CoralLime : TownSwatch.ReefTeal, 20f);
            b.TwoSidedQuad(new Vector3(-0.3f * s, 0.6f * s, 0f), new Vector3(0.3f * s, 0.6f * s, 0f), new Vector3(0.45f * s, 1.3f * s, 0.05f * s),
                new Vector3(-0.4f * s, 1.25f * s, 0.05f * s), TownSwatch.Pearl);
        }

        /// <summary>A run of ruined coral-stone wall between ground points <paramref name="a"/> and <paramref name="c"/>, broken top.</summary>
        public static void CoralWall(TownMeshBuilder b, Vector3 a, Vector3 c, float height, int seed)
        {
            float h01 = TownStyle.Hash01("coralwall", seed);
            float top = height * (0.55f + 0.45f * h01);
            Run(b, a - Vector3.up * 0.3f, c - Vector3.up * 0.3f, 0.6f, 0f, top + 0.3f, TownSwatch.CoralLime, TownSwatch.CoralDim);
            Vector3 mid = Vector3.Lerp(a, c, 0.35f + 0.3f * h01);
            b.Blob(mid + Vector3.up * (top + 0.05f), new Vector3(0.45f, 0.25f, 0.4f), TownSwatch.Moss);
        }

        /// <summary>A ruined arch: two coral piers, the pointed lintel, a fallen block.</summary>
        public static void CoralArch(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(-1.3f * s, 1.2f * s, 0f), new Vector3(0.7f, 2.9f, 0.8f) * s, TownSwatch.CoralLime, 0f, TownSwatch.CoralDim);
            b.Box(new Vector3(1.3f * s, 1.2f * s, 0f), new Vector3(0.7f, 2.9f, 0.8f) * s, TownSwatch.CoralLime, 0f, TownSwatch.CoralDim);
            b.Box(new Vector3(-0.65f * s, 2.85f * s, 0f), new Vector3(1.3f, 0.45f, 0.8f) * s, TownSwatch.CoralLime, 0f, TownSwatch.CoralDim);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(0.65f * s, 2.85f * s, 0f), new Vector3(1.3f, 0.45f, 0.8f) * s, TownSwatch.CoralLime, 0f, TownSwatch.CoralDim);
                b.Box(new Vector3(0f, 3.15f * s, 0f), new Vector3(0.5f, 0.3f, 0.8f) * s, TownSwatch.Teak);
            }
            else
            {
                b.Box(new Vector3(1.4f * s, 0.2f * s, 1.2f * s), new Vector3(1.2f, 0.4f, 0.7f) * s, TownSwatch.CoralDim, 35f);
            }

            b.Blob(new Vector3(-1.3f * s, 2.75f * s, 0.2f * s), new Vector3(0.4f, 0.2f, 0.4f) * s, TownSwatch.Moss);
        }

        /// <summary>A Swahili pillar tomb: a low coral enclosure with a tall fluted pillar, a blue bowl set in it.</summary>
        public static void PillarTomb(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.25f * s, 0f), new Vector3(2.4f, 0.8f, 1.6f) * s, TownSwatch.CoralLime, 0f, TownSwatch.CoralDim);
            float h = (3.2f + 0.6f * (variant & 1)) * s;
            b.Cylinder(new Vector3(0f, 0.6f * s, 0.9f * s), 0.42f * s, 0.32f * s, h, 8, TownSwatch.CoralLime);
            b.Disc(new Vector3(0f, 0.6f * s + h, 0.9f * s), 0.32f * s, 8, TownSwatch.CoralDim);
            b.Blob(new Vector3(0f, 0.6f * s + h * 0.62f, 0.55f * s), new Vector3(0.18f, 0.18f, 0.08f) * s, TownSwatch.Turquoise);
        }

        /// <summary>A flat-topped acacia: a thin forked trunk and a wide, flat, layered crown.</summary>
        public static void Acacia(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.35f : -0.3f;
            var fork = new Vector3(lean * s, 2.2f * s, 0.1f * s);
            b.Log(Vector3.down * 0.2f * s, fork, 0.17f * s, TownSwatch.Trunk, 5);
            b.Log(fork, fork + new Vector3(1.1f, 1.1f, 0.3f) * s, 0.1f * s, TownSwatch.Trunk, 4);
            b.Log(fork, fork + new Vector3(-1f, 1.2f, -0.4f) * s, 0.1f * s, TownSwatch.Trunk, 4);
            b.Crown(fork + new Vector3(0.1f, 1.45f, 0f) * s, new Vector3(4.2f, 0.55f, 3.6f) * s, TownSwatch.Kelp, variant * 29f);
            b.Crown(fork + new Vector3(0.4f, 1.8f, 0.3f) * s, new Vector3(2.6f, 0.4f, 2.2f) * s, TownSwatch.Mangrove, variant * 41f + 15f);
        }

        /// <summary>A termite mound of red earth: a lumpy spire with a smaller one beside it.</summary>
        public static void TermiteMound(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(new Vector3(0f, -0.3f * s, 0f), 0.95f * s, 0.25f * s, 2.9f * s, 7, TownSwatch.Laterite);
            b.Blob(new Vector3(0.2f * s, 0.5f * s, 0.1f * s), new Vector3(1.4f, 1f, 1.2f) * s, TownSwatch.RedClay);
            b.Cylinder(new Vector3(0.9f * s, -0.2f * s, -0.5f * s), 0.45f * s, 0.12f * s, (1.4f + 0.4f * (variant & 1)) * s, 6, TownSwatch.Laterite);
        }

        /// <summary>A line of wrack on the sand: dark seaweed strands and a few pale shells (ground detail).</summary>
        public static void Wrack(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 0.9f * s;
                b.QuadFacing(new Vector3(x - 0.45f * s, 0.03f, -0.12f * s), new Vector3(x - 0.45f * s, 0.03f, 0.12f * s), new Vector3(x + 0.45f * s, 0.03f, 0.18f * s),
                    new Vector3(x + 0.45f * s, 0.03f, -0.06f * s), Vector3.up, (i + variant) % 2 == 0 ? TownSwatch.Kelp : TownSwatch.MangroveRoot);
            }

            b.Blob(new Vector3(0.3f * s, 0.05f, 0.35f * s), new Vector3(0.14f, 0.07f, 0.12f) * s, TownSwatch.Pearl);
            b.Blob(new Vector3(-0.8f * s, 0.05f, -0.3f * s), new Vector3(0.12f, 0.06f, 0.1f) * s, TownSwatch.CoralLive);
        }

        /// <summary>Cloves drying on a woven mat (ground detail).</summary>
        public static void CloveMat(TownMeshBuilder b, float s, int variant)
        {
            b.QuadFacing(new Vector3(-1f * s, 0.03f, -0.7f * s), new Vector3(-1f * s, 0.03f, 0.7f * s), new Vector3(1f * s, 0.03f, 0.7f * s), new Vector3(1f * s, 0.03f, -0.7f * s),
                Vector3.up, TownSwatch.ReedMat);
            b.QuadFacing(new Vector3(-0.8f * s, 0.05f, -0.5f * s), new Vector3(-0.8f * s, 0.05f, 0.5f * s), new Vector3(0.8f * s, 0.05f, 0.5f * s), new Vector3(0.8f * s, 0.05f, -0.5f * s),
                Vector3.up, (variant & 1) == 0 ? TownSwatch.Laterite : TownSwatch.WoodDark);
        }

        /// <summary>A post striped red and white marking the edge of a shallow ford.</summary>
        public static void FordPost(TownMeshBuilder b, float s)
        {
            b.Log(Vector3.down * 0.4f * s, Vector3.up * 0.7f * s, 0.09f * s, TownSwatch.ClothWhite, 4);
            b.Log(Vector3.up * 0.7f * s, Vector3.up * 1.15f * s, 0.095f * s, TownSwatch.ClothRed, 4);
            b.Log(Vector3.up * 1.15f * s, Vector3.up * 1.5f * s, 0.09f * s, TownSwatch.ClothWhite, 4);
            b.Blob(Vector3.up * 1.6f * s, new Vector3(0.18f, 0.12f, 0.18f) * s, TownSwatch.ClothRed);
        }
    }
}
