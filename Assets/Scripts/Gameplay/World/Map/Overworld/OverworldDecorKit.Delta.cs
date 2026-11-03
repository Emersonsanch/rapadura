using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 6 — Delta de Jhilpur: the delta pieces (low-poly, the shared palette; each built round the origin and placed by
    /// the chunk builder): banyans with curtains of aerial roots, arches of living root in the gorges (the root bridges'
    /// shape, Docs/Mapas100.md 32, as form only), the fishers' reed huts on stilts of Tarchon's swamp, lotus lanterns on
    /// bamboo poles (the Ilhota das Lanternas), springs of Living Water in a ring of roots (the Coração do Lodo's fountains),
    /// twisted rotten roots, earthen paddy dikes; and the ground detail stamped by the hundred: lotus pads, rice clumps and
    /// the Lodo's violet bubbles.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A delta piece (Região 6, 140–159).</summary>
        public static bool IsDeltaDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 140 && i < 160;
        }

        public static readonly TownStamp LotusStamp = TownMeshBuilder.Record(b => LotusPads(b, 1f));
        public static readonly TownStamp RiceStamp = TownMeshBuilder.Record(b => RiceTuft(b, 1f));
        public static readonly TownStamp BubbleStamp = TownMeshBuilder.Record(b => MireBubbles(b, 1f));

        /// <summary>A banyan: three trunks fused, buttress roots, curtains of aerial roots from the limbs, a wide flat crown.</summary>
        public static void Banyan(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.75f * s, 0.5f * s, 3.4f * s, 7, TownSwatch.MangroveRoot);
            b.Log(new Vector3(0.6f, 0f, 0.3f) * s, new Vector3(0.35f, 3.2f, 0.1f) * s, 0.3f * s, TownSwatch.Trunk, 5);
            b.Log(new Vector3(-0.5f, 0f, -0.4f) * s, new Vector3(-0.2f, 3f, -0.2f) * s, 0.28f * s, TownSwatch.Trunk, 5);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 25f + variant * 17f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Spike(b, dir * 0.5f * s + Vector3.up * 0.8f * s, dir * 1.6f * s, 0.18f * s, TownSwatch.MangroveRoot);

                // A limb out, and the aerial roots dropping from it to the ground.
                Vector3 limb = dir * 3f * s + Vector3.up * 3.6f * s;
                b.Log(Vector3.up * 3f * s, limb, 0.16f * s, TownSwatch.Trunk, 4);
                b.Log(limb, new Vector3(limb.x, -0.2f * s, limb.z), 0.07f * s, TownSwatch.WoodWeathered, 3);
                Vector3 mid = Vector3.Lerp(Vector3.up * 3f * s, limb, 0.55f);
                b.Log(mid, new Vector3(mid.x + 0.2f * s, -0.2f * s, mid.z), 0.05f * s, TownSwatch.WoodWeathered, 3);
            }

            TownSwatch leaf = (variant & 1) == 0 ? TownSwatch.LeafDark : TownSwatch.Pine;
            b.Crown(new Vector3(0f, 4.6f, 0f) * s, new Vector3(7.6f, 1.8f, 7f) * s, leaf, variant * 31f);
            b.Crown(new Vector3(0.6f, 5.5f, -0.3f) * s, new Vector3(4.6f, 1.3f, 4.2f) * s, (variant & 2) == 0 ? TownSwatch.Mangrove : TownSwatch.GrassDark, variant * 47f + 20f);
        }

        /// <summary>An arch of living roots: two root feet twisted together, the span overhead, moss and a fern on top.</summary>
        public static void RootArch(TownMeshBuilder b, float s, int variant)
        {
            const int n = 6;
            float span = 2.4f * s;
            float rise = 2.9f * s;
            for (int strand = 0; strand < 2; strand++)
            {
                float z = (strand - 0.5f) * 0.35f * s;
                Vector3 previous = new Vector3(-span, -0.3f * s, z);
                for (int i = 1; i <= n; i++)
                {
                    float t = i / (float)n;
                    float x = Mathf.Lerp(-span, span, t);
                    float y = Mathf.Sin(t * Mathf.PI) * rise - 0.3f * s * (1f - Mathf.Sin(t * Mathf.PI));
                    var p = new Vector3(x, y, z + Mathf.Sin(t * Mathf.PI * 2f + strand) * 0.12f * s);
                    b.Log(previous, p, (0.2f - 0.05f * strand) * s, strand == 0 ? TownSwatch.Trunk : TownSwatch.MangroveRoot, 4);
                    previous = p;
                }
            }

            b.Blob(new Vector3(0f, rise + 0.1f * s, 0f), new Vector3(1.2f, 0.3f, 0.6f) * s, TownSwatch.Moss);
            b.Blob(new Vector3(-span, 0.2f * s, 0f), new Vector3(0.7f, 0.5f, 0.7f) * s, TownSwatch.Moss);
            b.Blob(new Vector3(span, 0.2f * s, 0f), new Vector3(0.7f, 0.5f, 0.7f) * s, (variant & 1) == 0 ? TownSwatch.Moss : TownSwatch.Vine);
        }

        /// <summary>A fisher's reed hut on four stilts: a plank floor, reed walls, a curved thatch roof, a ladder and a net.</summary>
        public static void StiltHut(TownMeshBuilder b, float s, int variant)
        {
            const float floor = 1.4f;
            foreach (Vector2 c in new[] { new Vector2(-1f, -0.9f), new Vector2(1f, -0.9f), new Vector2(-1f, 0.9f), new Vector2(1f, 0.9f) })
            {
                b.Log(new Vector3(c.x, -0.3f, c.y) * s, new Vector3(c.x, floor, c.y) * s, 0.08f * s, TownSwatch.WoodWeathered, 4);
            }

            b.Box(new Vector3(0f, floor + 0.05f, 0f) * s, new Vector3(2.6f, 0.1f, 2.4f) * s, TownSwatch.WoodLight);
            b.Box(new Vector3(0f, floor + 0.75f, 0.1f) * s, new Vector3(2f, 1.3f, 1.8f) * s, TownSwatch.Reed, 0f, TownSwatch.ReedMat);
            b.Box(new Vector3(0f, floor + 0.55f, -0.81f) * s, new Vector3(0.6f, 0.9f, 0.04f) * s, TownSwatch.WoodDark);
            Vector3 l = new Vector3(-1.2f, floor + 1.3f, 1.2f) * s, r = new Vector3(1.2f, floor + 1.3f, 1.2f) * s;
            Vector3 lf = new Vector3(-1.2f, floor + 1.3f, -1.2f) * s, rf = new Vector3(1.2f, floor + 1.3f, -1.2f) * s;
            Vector3 ridgeBack = new Vector3(0f, floor + 2.2f, 1.3f) * s, ridgeFront = new Vector3(0f, floor + 2.2f, -1.3f) * s;
            b.TwoSidedQuad(l, ridgeBack, ridgeFront, lf, TownSwatch.Thatch);
            b.TwoSidedQuad(ridgeBack, r, rf, ridgeFront, TownSwatch.Thatch);
            b.Log(new Vector3(-0.3f, -0.2f, -1.9f) * s, new Vector3(-0.3f, floor, -1.2f) * s, 0.05f * s, TownSwatch.WoodWeathered, 3);
            b.Log(new Vector3(0.3f, -0.2f, -1.9f) * s, new Vector3(0.3f, floor, -1.2f) * s, 0.05f * s, TownSwatch.WoodWeathered, 3);
            b.TwoSidedQuad(new Vector3(1.3f, floor + 1.1f, -0.6f) * s, new Vector3(1.3f, floor + 1.1f, 0.6f) * s, new Vector3(1.6f, 0.2f, 0.7f) * s,
                new Vector3(1.6f, 0.2f, -0.5f) * s, (variant & 1) == 0 ? TownSwatch.ReedMat : TownSwatch.Sail);
        }

        /// <summary>A lotus lantern hung from a leaning bamboo pole (the lantern glows at night).</summary>
        public static void LanternPole(TownMeshBuilder b, float s, int variant)
        {
            Vector3 top = new Vector3(0.5f, 2.9f, 0f) * s;
            b.Log(Vector3.down * 0.3f * s, top, 0.06f * s, TownSwatch.Reed, 4);
            Vector3 hook = top + new Vector3(0.5f, -0.1f, 0f) * s;
            b.Log(top, hook, 0.04f * s, TownSwatch.Reed, 3);
            b.Cylinder(hook + Vector3.down * 0.7f * s, 0.18f * s, 0.24f * s, 0.4f * s, 6, TownSwatch.LampGlow, TownSwatch.Tamarisk);
            b.Crown(hook + Vector3.down * 0.25f * s, new Vector3(0.5f, 0.2f, 0.5f) * s, (variant & 1) == 0 ? TownSwatch.Tamarisk : TownSwatch.ClothWhite, 20f);
        }

        /// <summary>A spring of Living Water: a ring of roots round a pool of glowing teal, a sprout in the middle.</summary>
        public static void LivingSpring(TownMeshBuilder b, float s, int variant)
        {
            b.Disc(Vector3.up * 0.08f * s, 1.5f * s, 9, TownSwatch.ReefTeal);
            for (int i = 0; i < 7; i++)
            {
                float a0 = i * Mathf.PI * 2f / 7f;
                float a1 = (i + 1) * Mathf.PI * 2f / 7f;
                var p0 = new Vector3(Mathf.Cos(a0), 0.15f, Mathf.Sin(a0)) * 1.75f * s;
                var p1 = new Vector3(Mathf.Cos(a1), 0.15f, Mathf.Sin(a1)) * 1.75f * s;
                b.Log(p0, p1, 0.22f * s, (i & 1) == 0 ? TownSwatch.MangroveRoot : TownSwatch.Trunk, 4);
            }

            b.Log(Vector3.zero, new Vector3(0.05f, 0.9f, 0f) * s, 0.05f * s, TownSwatch.Vine, 3);
            b.Crown(new Vector3(0.05f, 1f, 0f) * s, new Vector3(0.45f, 0.25f, 0.45f) * s, TownSwatch.Grass, variant * 40f);
        }

        /// <summary>A twisted mass of rotten root: black coils over the mud, a broken grey stub, bracket fungi.</summary>
        public static void RottenRoot(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-1.4f, 0.1f, -0.3f) * s, new Vector3(0.2f, 0.55f, 0.1f) * s, 0.28f * s, TownSwatch.Black, 5);
            b.Log(new Vector3(0.2f, 0.55f, 0.1f) * s, new Vector3(1.3f, 0.05f, 0.5f) * s, 0.24f * s, TownSwatch.MangroveRoot, 5);
            b.Log(new Vector3(0.1f, 0.4f, -0.1f) * s, new Vector3(0.3f, 1.7f, 0.2f) * s, 0.22f * s, TownSwatch.WoodWeathered, 5);
            b.Log(new Vector3(-0.3f, 0.3f, 0.2f) * s, new Vector3(-0.8f, 0.05f, 1.1f) * s, 0.15f * s, TownSwatch.Black, 4);
            b.Blob(new Vector3(0.45f, 1.1f, 0.15f) * s, new Vector3(0.4f, 0.08f, 0.3f) * s, (variant & 1) == 0 ? TownSwatch.Violet : TownSwatch.Sand);
            b.Blob(new Vector3(-0.6f, 0.45f, -0.15f) * s, new Vector3(0.3f, 0.07f, 0.25f) * s, TownSwatch.Sand);
        }

        /// <summary>A run of earthen paddy dike between ground points <paramref name="a"/> and <paramref name="c"/>, grass on top.</summary>
        public static void PaddyBund(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Ridge(b, a + Vector3.down * 0.15f, c + Vector3.down * 0.15f, 1.3f, 0.6f, TownSwatch.Mud, TownSwatch.DirtDark);
            Ridge(b, a + Vector3.up * 0.3f, c + Vector3.up * 0.3f, 0.5f, 0.12f, TownSwatch.Grass, TownSwatch.Vine);
        }

        /// <summary>Two lotus pads and a pink flower on the water's edge (ground detail).</summary>
        public static void LotusPads(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.05f, 0f), 0.45f * s, 6, TownSwatch.Kelp);
            b.Disc(new Vector3(0.7f * s, 0.05f, 0.3f * s), 0.35f * s, 6, TownSwatch.Vine, 0.4f);
            b.Crown(new Vector3(0.05f * s, 0.18f * s, 0.05f * s), new Vector3(0.28f, 0.22f, 0.28f) * s, TownSwatch.Tamarisk, 30f);
        }

        /// <summary>A clump of rice standing in the flooded paddy (ground detail).</summary>
        public static void RiceTuft(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.3f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.06f * s;
                Vector3 tip = dir * 0.25f * s + Vector3.up * 0.7f * s;
                b.TriangleFacing(Vector3.up * 0.02f + side, tip, Vector3.up * 0.02f - side, dir, (i & 1) == 0 ? TownSwatch.Vine : TownSwatch.Grass);
            }
        }

        /// <summary>Violet bubbles of the Lodo on the black mud (ground detail).</summary>
        public static void MireBubbles(TownMeshBuilder b, float s)
        {
            b.Blob(new Vector3(0f, 0.05f, 0f), new Vector3(0.36f, 0.22f, 0.36f) * s, TownSwatch.Violet);
            b.Blob(new Vector3(0.45f * s, 0.04f, 0.2f * s), new Vector3(0.22f, 0.14f, 0.22f) * s, TownSwatch.Violet);
            b.Blob(new Vector3(-0.3f * s, 0.03f, 0.35f * s), new Vector3(0.16f, 0.1f, 0.16f) * s, TownSwatch.Black);
        }
    }
}
