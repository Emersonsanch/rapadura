using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 10 — Picos de Farnkesh: the peaks' pieces (low-poly, the shared palette; each built round the origin and placed by
    /// the chunk builder): sandstone pinnacles and the white hermitages of the stone nests (the PinnacleNest form: walls leaning
    /// in, painted wooden windows, plank roofs weighed with stones), wind bells, rope bridges, paper streamers and rope baskets
    /// (54–56), the satyrs' vines, gourd huts and stone stages (53), the shelter stones and twisted trees of the gorge (57), Huma's
    /// golden nest (58); and the ground detail stamped by the hundred: cloud puffs and the Pactolo's golden pebbles.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Picos de Farnkesh piece (Região 10, 220–239).</summary>
        public static bool IsFarnkeshDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 220 && i < 240;
        }

        public static readonly TownStamp CloudStamp = TownMeshBuilder.Record(b => CloudPuff(b, 1f));
        public static readonly TownStamp GoldPebbleStamp = TownMeshBuilder.Record(b => GoldPebbles(b, 1f));

        /// <summary>A sandstone pinnacle: a banded round tower narrowing upwards, a domed cap with grass, a pine at its foot.</summary>
        public static void SandstonePinnacle(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch band = (variant & 1) == 0 ? TownSwatch.AdobeOchre : TownSwatch.Sandstone;
            b.Cylinder(Vector3.down * 0.5f * s, 2.6f * s, 2.2f * s, 3.5f * s, 7, TownSwatch.Sandstone, null, variant * 0.4f);
            b.Cylinder(new Vector3(0f, 3f, 0f) * s, 2.25f * s, 1.8f * s, 3f * s, 7, band, null, variant * 0.4f + 0.2f);
            b.Cylinder(new Vector3(0f, 6f, 0f) * s, 1.85f * s, 1.4f * s, 2.6f * s, 7, TownSwatch.Sandstone, null, variant * 0.4f);
            b.Blob(new Vector3(0f, 8.7f, 0f) * s, new Vector3(2.9f, 1.1f, 2.9f) * s, (variant & 2) == 0 ? TownSwatch.GrassDark : TownSwatch.Sandstone);
            b.Crown(new Vector3(2.6f, 1.8f, 0.6f) * s, new Vector3(1.1f, 2.4f, 1.1f) * s, TownSwatch.Pine, variant * 31f);
            b.Log(new Vector3(2.6f, -0.3f, 0.6f) * s, new Vector3(2.6f, 1f, 0.6f) * s, 0.12f * s, TownSwatch.Trunk, 4);
        }

        /// <summary>A hermitage: white walls leaning in, a red wooden window frame, a plank roof with stones on it, a small balcony.</summary>
        public static void Hermitage(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.2f, 0f) * s, new Vector3(4.2f, 0.6f, 3.6f) * s, TownSwatch.Stone, 0f, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 0.5f, 0f) * s, 2.5f * s, 2.15f * s, 2.8f * s, 4, TownSwatch.LimeWhite, null, 0.5f);
            b.Box(new Vector3(0f, 1.9f, -1.72f) * s, new Vector3(1.2f, 0.9f, 0.08f) * s, TownSwatch.ClothRed);
            b.Box(new Vector3(0f, 1.9f, -1.77f) * s, new Vector3(0.8f, 0.6f, 0.06f) * s, TownSwatch.WoodDark);
            b.Box(new Vector3(0f, 1f, 1.62f) * s, new Vector3(0.8f, 1.5f, 0.08f) * s, TownSwatch.WoodDark);
            b.Box(new Vector3(0f, 3.45f, 0f) * s, new Vector3(3.9f, 0.2f, 3.5f) * s, TownSwatch.WoodWeathered, 0f, TownSwatch.Slate);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(-1.1f + i * 1.1f, 3.65f, (i & 1) == 0 ? 0.4f : -0.5f) * s, new Vector3(0.45f, 0.3f, 0.4f) * s, TownSwatch.Stone);
            }

            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(-2.2f, 1.6f, 0f) * s, new Vector3(0.9f, 0.12f, 2f) * s, TownSwatch.WoodLight);
                b.Box(new Vector3(-2.6f, 1.95f, 0f) * s, new Vector3(0.06f, 0.6f, 2f) * s, TownSwatch.ClothRed);
            }

            if ((variant & 2) == 0)
            {
                b.Box(new Vector3(0f, 3.75f, 0f) * s, new Vector3(0.12f, 0.6f, 0.12f) * s, TownSwatch.Gold);
            }
        }

        /// <summary>A wind bell: two posts and a beam, a bronze bell with its clapper, a strip of wind paper under it.</summary>
        public static void WindBell(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-0.9f, -0.3f, 0f) * s, new Vector3(-0.9f, 2.8f, 0f) * s, 0.09f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(0.9f, -0.3f, 0f) * s, new Vector3(0.9f, 2.8f, 0f) * s, 0.09f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(0f, 2.85f, 0f) * s, new Vector3(2.3f, 0.18f, 0.24f) * s, TownSwatch.ClothRed);
            b.Cylinder(new Vector3(0f, 1.6f, 0f) * s, 0.48f * s, 0.2f * s, 0.95f * s, 7, (variant & 1) == 0 ? TownSwatch.Bronze : TownSwatch.Gold);
            b.Blob(new Vector3(0f, 1.5f, 0f) * s, new Vector3(0.14f, 0.18f, 0.14f) * s, TownSwatch.Iron);
            b.TwoSidedQuad(new Vector3(-0.08f, 1.45f, 0f) * s, new Vector3(0.08f, 1.45f, 0f) * s, new Vector3(0.12f, 0.6f, 0.05f) * s, new Vector3(-0.04f, 0.6f, 0.05f) * s,
                (variant & 2) == 0 ? TownSwatch.ClothWhite : TownSwatch.ClothYellow);
        }

        /// <summary>
        /// A run of a rope bridge from ground point <paramref name="a"/> to <paramref name="c"/>: planks across the way, two rope
        /// rails on posts at both ends (laid on the ground: the bridges' line over the hollows of the wind bridges).
        /// </summary>
        public static void RopeBridgeRun(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Vector3 d = c - a;
            float length = new Vector2(d.x, d.z).magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            Vector3 side = new Vector3(d.z, 0f, -d.x) / length;
            int planks = Mathf.Max(2, Mathf.RoundToInt(length / 0.7f));
            for (int i = 0; i < planks; i++)
            {
                Vector3 m = Vector3.Lerp(a, c, (i + 0.5f) / planks) + Vector3.up * 0.12f;
                b.Box(m, new Vector3(2.2f, 0.08f, 0.5f), ((i + variant) % 4) == 0 ? TownSwatch.WoodDark : TownSwatch.WoodWeathered, yaw);
            }

            foreach (float k in new[] { -1.15f, 1.15f })
            {
                Vector3 p0 = a + side * k;
                Vector3 p1 = c + side * k;
                b.Log(p0 + Vector3.down * 0.2f, p0 + Vector3.up * 1.1f, 0.07f, TownSwatch.WoodDark, 4);
                b.Log(p0 + Vector3.up * 0.95f, p1 + Vector3.up * 0.95f, 0.035f, TownSwatch.Thatch, 3);
            }
        }

        /// <summary>A line of wind paper streamers from ground point <paramref name="a"/> to <paramref name="c"/>: two poles, a cord, coloured strips.</summary>
        public static void PaperStreamers(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            b.Log(a + Vector3.down * 0.3f, a + Vector3.up * 3f, 0.06f, TownSwatch.WoodDark, 4);
            b.Log(c + Vector3.down * 0.3f, c + Vector3.up * 3f, 0.06f, TownSwatch.WoodDark, 4);
            Vector3 ta = a + Vector3.up * 2.9f;
            Vector3 tc = c + Vector3.up * 2.9f;
            b.Log(ta, tc, 0.02f, TownSwatch.Thatch, 3);
            TownSwatch[] colours = { TownSwatch.ClothWhite, TownSwatch.ClothYellow, TownSwatch.ClothRed, TownSwatch.SilkBlue, TownSwatch.ClothGreen };
            int strips = Mathf.Clamp(Mathf.RoundToInt(Vector3.Distance(a, c) / 0.6f), 2, 10);
            for (int i = 1; i < strips; i++)
            {
                float t = i / (float)strips;
                Vector3 top = Vector3.Lerp(ta, tc, t) + Vector3.down * 0.25f * Mathf.Sin(t * Mathf.PI);
                b.TwoSidedQuad(top + Vector3.left * 0.12f, top + Vector3.right * 0.12f, top + new Vector3(0.16f, -0.7f, 0.1f), top + new Vector3(-0.08f, -0.7f, 0.1f),
                    colours[(i + variant) % colours.Length]);
            }
        }

        /// <summary>A rope basket lift: a timber crane on a stone foot, its pulley, the rope and a wicker basket at the bottom.</summary>
        public static void RopeBasket(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.25f, 0f) * s, new Vector3(1.6f, 0.7f, 1.6f) * s, TownSwatch.Stone, 0f, TownSwatch.StoneDark);
            b.Log(new Vector3(0f, 0.5f, 0f) * s, new Vector3(0f, 4.6f, 0f) * s, 0.14f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(0f, 4.4f, 0f) * s, new Vector3(1.9f, 4.6f, 0f) * s, 0.1f * s, TownSwatch.WoodDark, 4);
            b.Cylinder(new Vector3(1.85f, 4.25f, -0.1f) * s, 0.24f * s, 0.24f * s, 0.2f * s, 6, TownSwatch.WoodLight);
            b.Log(new Vector3(1.85f, 4.25f, 0f) * s, new Vector3(1.85f, 1.2f, 0f) * s, 0.025f * s, TownSwatch.Thatch, 3);
            b.Cylinder(new Vector3(1.85f, 0.2f, 0f) * s, 0.55f * s, 0.7f * s, 1f * s, 7, (variant & 1) == 0 ? TownSwatch.Thatch : TownSwatch.Straw);
            b.Cylinder(new Vector3(-0.6f, 0.6f, 0.4f) * s, 0.35f * s, 0.35f * s, 0.5f * s, 6, TownSwatch.WoodWeathered);
        }

        /// <summary>A grape vine on its stake: a twisted stock, a leafy crown, bunches of dark or golden grapes.</summary>
        public static void GrapeVine(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.down * 0.3f * s, new Vector3(0f, 1.7f, 0f) * s, 0.04f * s, TownSwatch.WoodLight, 4);
            b.Log(Vector3.down * 0.2f * s, new Vector3(0.15f, 1.2f, 0.05f) * s, 0.07f * s, TownSwatch.Trunk, 4);
            b.Crown(new Vector3(0.05f, 1.3f, 0f) * s, new Vector3(0.9f, 0.7f, 0.8f) * s, (variant & 3) == 3 ? TownSwatch.LeafGold : TownSwatch.Vine, variant * 41f);
            TownSwatch grape = (variant & 1) == 0 ? TownSwatch.Violet : TownSwatch.Gold;
            b.Blob(new Vector3(0.35f, 0.95f, -0.2f) * s, new Vector3(0.2f, 0.28f, 0.2f) * s, grape);
            b.Blob(new Vector3(-0.3f, 1f, 0.2f) * s, new Vector3(0.18f, 0.26f, 0.18f) * s, grape);
        }

        /// <summary>A satyr's wicker hut shaped like a gourd: a round wicker belly, a narrow neck and a leafy stem, a low arched door.</summary>
        public static void GourdHut(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch wicker = (variant & 1) == 0 ? TownSwatch.Thatch : TownSwatch.Straw;
            b.Blob(new Vector3(0f, 1.3f, 0f) * s, new Vector3(3.2f, 2.8f, 3.2f) * s, wicker);
            b.Blob(new Vector3(0f, 3f, 0f) * s, new Vector3(1.5f, 1.6f, 1.5f) * s, wicker);
            b.Cylinder(new Vector3(0f, 3.7f, 0f) * s, 0.2f * s, 0.12f * s, 0.7f * s, 5, TownSwatch.WoodDark);
            b.Crown(new Vector3(0.3f, 4.3f, 0f) * s, new Vector3(0.7f, 0.35f, 0.5f) * s, TownSwatch.Vine, variant * 30f);
            b.Box(new Vector3(0f, 0.6f, -1.52f) * s, new Vector3(0.9f, 1.2f, 0.1f) * s, TownSwatch.WoodDark);
            b.Box(new Vector3(0.9f, 1.6f, -1.3f) * s, new Vector3(0.4f, 0.4f, 0.08f) * s, (variant & 2) == 0 ? TownSwatch.TaipaRose : TownSwatch.ClothYellow);
        }

        /// <summary>A round stone stage of the dances: a low drum of fitted stones, a ring of flat seats, a double flute on it.</summary>
        public static void StoneStage(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.3f * s, 2.6f * s, 2.5f * s, 0.75f * s, 10, TownSwatch.Stone, TownSwatch.Paving);
            b.Ring(new Vector3(0f, 0.46f, 0f) * s, 2.1f * s, 2.5f * s, 10, TownSwatch.StoneDark, TownSwatch.Stone);
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f + variant * 0.03f) * Mathf.PI * 2f;
                b.Box(new Vector3(Mathf.Cos(a) * 3.3f, 0.15f, Mathf.Sin(a) * 3.3f) * s, new Vector3(0.9f, 0.45f, 0.5f) * s, TownSwatch.Stone, -a * Mathf.Rad2Deg);
            }

            b.Log(new Vector3(-0.3f, 0.55f, 0f) * s, new Vector3(0.4f, 0.55f, 0.15f) * s, 0.04f * s, TownSwatch.Reed, 4);
            b.Log(new Vector3(-0.3f, 0.55f, 0.08f) * s, new Vector3(0.4f, 0.55f, 0.3f) * s, 0.04f * s, TownSwatch.Reed, 4);
        }

        /// <summary>A shelter stone: a curved low wall of stacked grey rock, its back to the wind.</summary>
        public static void ShelterStone(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 5; i++)
            {
                float a = (-0.5f + i * 0.25f) * Mathf.PI * 0.7f;
                var p = new Vector3(Mathf.Sin(a) * 2.4f, 0f, Mathf.Cos(a) * 2.4f);
                float h = 1.2f + 0.35f * ((i + variant) % 3);
                b.Box((p + Vector3.up * (h * 0.5f - 0.3f)) * s, new Vector3(1.4f, h, 0.9f) * s, (i & 1) == 0 ? TownSwatch.Granite : TownSwatch.Stone, a * Mathf.Rad2Deg);
            }

            b.Blob(new Vector3(0f, 1.6f, 2.4f) * s, new Vector3(1.2f, 0.5f, 0.8f) * s, TownSwatch.StoneDark);
        }

        /// <summary>A wind-twisted tree: a trunk bent away from the wind in two kinks, a flat crown swept to one side.</summary>
        public static void TwistedTree(TownMeshBuilder b, float s, int variant)
        {
            var k1 = new Vector3(0.6f, 1.6f, 0f) * s;
            var k2 = new Vector3(1.8f, 2.6f, 0.2f) * s;
            b.Log(Vector3.down * 0.3f * s, k1, 0.2f * s, TownSwatch.Trunk, 5);
            b.Log(k1, k2, 0.15f * s, TownSwatch.Trunk, 5);
            b.Log(k1, new Vector3(-0.5f, 2.4f, 0.3f) * s, 0.08f * s, TownSwatch.Trunk, 4);
            TownSwatch crown = (variant & 1) == 0 ? TownSwatch.Juniper : TownSwatch.Pine;
            b.Crown(k2 + new Vector3(0.5f, 0.2f, 0f) * s, new Vector3(2.6f, 0.9f, 1.6f) * s, crown, variant * 17f);
            b.Crown(new Vector3(-0.5f, 2.5f, 0.3f) * s, new Vector3(1.1f, 0.5f, 0.9f) * s, crown, variant * 29f);
        }

        /// <summary>Huma's nest: a wide ring of golden branches criss-crossed, white feathers on the rim, a hollow of soft gold inside.</summary>
        public static void GoldenNest(TownMeshBuilder b, float s, int variant)
        {
            const int sticks = 14;
            for (int i = 0; i < sticks; i++)
            {
                float a = (i / (float)sticks + variant * 0.02f) * Mathf.PI * 2f;
                float a2 = a + 0.7f;
                var p0 = new Vector3(Mathf.Cos(a) * 4.2f, 0.4f + 0.5f * (i & 1), Mathf.Sin(a) * 4.2f);
                var p1 = new Vector3(Mathf.Cos(a2) * 4.4f, 1f + 0.4f * ((i + 1) & 1), Mathf.Sin(a2) * 4.4f);
                b.Log(p0 * s, p1 * s, 0.22f * s, (i % 3) == 0 ? TownSwatch.LeafGold : TownSwatch.Gold, 4);
            }

            b.Cylinder(new Vector3(0f, -0.3f, 0f) * s, 3.6f * s, 3.9f * s, 0.9f * s, 10, TownSwatch.Gold, TownSwatch.Straw);
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f) * Mathf.PI * 2f + 0.3f;
                var p = new Vector3(Mathf.Cos(a) * 4.3f, 1.5f, Mathf.Sin(a) * 4.3f) * s;
                b.TwoSidedQuad(p, p + new Vector3(0.15f, 1.1f, 0f) * s, p + new Vector3(0.45f, 1.0f, 0.1f) * s, p + new Vector3(0.3f, 0f, 0.1f) * s, TownSwatch.ClothWhite);
            }
        }

        /// <summary>A low cloud puff on the ground: three soft white lumps (ground detail).</summary>
        public static void CloudPuff(TownMeshBuilder b, float s)
        {
            b.Blob(new Vector3(0f, 0.6f, 0f) * s, new Vector3(3.2f, 1.1f, 2.2f) * s, TownSwatch.ClothWhite);
            b.Blob(new Vector3(1.3f, 0.5f, 0.6f) * s, new Vector3(2f, 0.9f, 1.6f) * s, TownSwatch.Plaster);
            b.Blob(new Vector3(-1.2f, 0.45f, -0.4f) * s, new Vector3(1.8f, 0.8f, 1.5f) * s, TownSwatch.ClothWhite);
        }

        /// <summary>A scatter of the Pactolo's golden pebbles (ground detail).</summary>
        public static void GoldPebbles(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.9f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.35f, 0.04f, Mathf.Sin(a) * 0.35f) * s, new Vector3(0.22f, 0.12f, 0.18f) * s, (i & 1) == 0 ? TownSwatch.Gold : TownSwatch.LeafGold);
            }
        }
    }
}
