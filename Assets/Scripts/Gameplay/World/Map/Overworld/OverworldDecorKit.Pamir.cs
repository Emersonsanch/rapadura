using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 7 — Teto do Mundo: the high mountain's pieces (low-poly, the shared palette; each built round the origin and
    /// placed by the chunk builder): apricot trees in flower and old twisted junipers, the herders' flat-roofed stone huts,
    /// the polished stone chutes of the Calhas de Pedra (Docs/Mapas100.md 37) on their pillars and their timber stations, blue
    /// seracs, the Forte dos Ventos' wooden wind wheels and watch towers with wind bells (39), the caravans' ribbon lines (40,
    /// coloured cloth of the caravans, no rite) and Aydahar's great shed scales; and the ground detail stamped by the hundred:
    /// crevasse lines on the ice, marmot burrows and ice shards.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Teto do Mundo piece (Região 7, 160–179).</summary>
        public static bool IsPamirDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 160 && i < 180;
        }

        public static readonly TownStamp CrevasseStamp = TownMeshBuilder.Record(b => Crevasse(b, 1f));
        public static readonly TownStamp MarmotStamp = TownMeshBuilder.Record(b => MarmotMound(b, 1f));
        public static readonly TownStamp IceShardStamp = TownMeshBuilder.Record(b => IceShards(b, 1f));

        /// <summary>An apricot tree in flower: a crooked trunk, a wide pale-pink crown with white tufts.</summary>
        public static void ApricotBlossom(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.down * 0.2f * s, new Vector3(0.25f, 1.2f, 0.1f) * s, 0.16f * s, TownSwatch.Trunk, 5);
            b.Log(new Vector3(0.25f, 1.2f, 0.1f) * s, new Vector3(-0.35f, 1.9f, -0.15f) * s, 0.1f * s, TownSwatch.Trunk, 4);
            b.Log(new Vector3(0.25f, 1.2f, 0.1f) * s, new Vector3(0.8f, 1.8f, 0.3f) * s, 0.09f * s, TownSwatch.Trunk, 4);
            TownSwatch blossom = (variant & 1) == 0 ? TownSwatch.Tamarisk : TownSwatch.TaipaRose;
            b.Crown(new Vector3(0.1f, 2.3f, 0f) * s, new Vector3(2.8f, 1.6f, 2.6f) * s, blossom, variant * 37f);
            b.Crown(new Vector3(0.5f, 2.7f, 0.2f) * s, new Vector3(1.5f, 1f, 1.4f) * s, TownSwatch.ClothWhite, variant * 53f + 20f);
            b.Crown(new Vector3(-0.6f, 2.1f, -0.3f) * s, new Vector3(1.2f, 0.9f, 1.2f) * s, (variant & 2) == 0 ? TownSwatch.Plaster : TownSwatch.Tamarisk, variant * 19f);
        }

        /// <summary>An old twisted juniper: a thick bent trunk, a dead branch, dark scale-leaf clumps in layers.</summary>
        public static void Juniper(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.5f : -0.4f;
            Vector3 mid = new Vector3(lean, 1.4f, 0.2f) * s;
            Vector3 top = new Vector3(lean * 0.4f, 2.8f, -0.1f) * s;
            b.Log(Vector3.down * 0.3f * s, mid, 0.32f * s, TownSwatch.WoodWeathered, 5);
            b.Log(mid, top, 0.22f * s, TownSwatch.Trunk, 5);
            b.Log(mid, mid + new Vector3(-lean * 2f, 0.7f, 0.5f) * s, 0.08f * s, TownSwatch.WoodWeathered, 3);
            b.Crown(top + Vector3.up * 0.3f * s, new Vector3(1.8f, 1.1f, 1.7f) * s, TownSwatch.Pine, variant * 41f);
            b.Crown(mid + new Vector3(lean * 0.8f, 0.3f, 0f) * s, new Vector3(1.6f, 0.8f, 1.4f) * s, TownSwatch.LeafDark, variant * 23f + 10f);
            b.Crown(top + new Vector3(-0.6f, -0.5f, 0.4f) * s, new Vector3(1.2f, 0.7f, 1.1f) * s, TownSwatch.Pine, variant * 61f);
        }

        /// <summary>A herder's stone hut: low dry-stone walls, a flat roof of earth on beams, a smoke hole, a felt door.</summary>
        public static void StoneHut(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.8f, 0f) * s, new Vector3(3.6f, 2f, 3f) * s, TownSwatch.Stone, 0f, TownSwatch.Stone);
            b.Box(new Vector3(0f, 1.9f, 0f) * s, new Vector3(4f, 0.25f, 3.4f) * s, TownSwatch.Dirt, 0f, (variant & 1) == 0 ? TownSwatch.GrassDark : TownSwatch.Thatch);
            for (int i = -1; i <= 1; i++)
            {
                b.Log(new Vector3(i * 1.2f, 1.75f, -1.8f) * s, new Vector3(i * 1.2f, 1.75f, -1.6f) * s, 0.08f * s, TownSwatch.WoodDark, 4);
            }

            b.Box(new Vector3(0.4f, 0.7f, -1.52f) * s, new Vector3(0.8f, 1.3f, 0.06f) * s, (variant & 2) == 0 ? TownSwatch.ClothRed : TownSwatch.ClothIndigo);
            b.Cylinder(new Vector3(-0.9f, 2f, 0.4f) * s, 0.28f * s, 0.24f * s, 0.35f * s, 5, TownSwatch.StoneDark);
            b.Blob(new Vector3(1.6f, 0.25f, 1.2f) * s, new Vector3(0.9f, 0.5f, 0.8f) * s, TownSwatch.StoneDark);
        }

        /// <summary>A run of polished stone chute on low pillars from ground point <paramref name="a"/> to <paramref name="c"/>, ice in its groove.</summary>
        public static void StoneChute(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Vector3 lift = Vector3.up * 0.7f;
            Ridge(b, a + lift + Vector3.down * 0.25f, c + lift + Vector3.down * 0.25f, 1.4f, 0.35f, TownSwatch.Stone, TownSwatch.Slate);
            Ridge(b, a + lift + Vector3.up * 0.12f, c + lift + Vector3.up * 0.12f, 0.6f, 0.04f, TownSwatch.Glass, TownSwatch.LimeWhite);
            if ((variant & 1) == 0)
            {
                Vector3 m = (a + c) * 0.5f;
                b.Box(m + Vector3.up * 0.25f, new Vector3(0.5f, 0.9f, 0.5f), TownSwatch.StoneDark);
            }
        }

        /// <summary>A timber chute station: a roofed platform on posts, a cart on its rails, a lantern.</summary>
        public static void ChuteStation(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.6f, 0f) * s, new Vector3(4f, 0.25f, 3f) * s, TownSwatch.WoodLight, 0f, TownSwatch.WoodLight);
            foreach (Vector2 c in new[] { new Vector2(-1.8f, -1.3f), new Vector2(1.8f, -1.3f), new Vector2(-1.8f, 1.3f), new Vector2(1.8f, 1.3f) })
            {
                b.Log(new Vector3(c.x, -0.3f, c.y) * s, new Vector3(c.x, 2.9f, c.y) * s, 0.1f * s, TownSwatch.WoodDark, 4);
            }

            Vector3 l = new Vector3(-2.2f, 2.9f, 1.7f) * s, r = new Vector3(2.2f, 2.9f, 1.7f) * s;
            Vector3 lf = new Vector3(-2.2f, 2.9f, -1.7f) * s, rf = new Vector3(2.2f, 2.9f, -1.7f) * s;
            Vector3 ridgeBack = new Vector3(-2.2f, 3.6f, 0f) * s, ridgeFront = new Vector3(2.2f, 3.6f, 0f) * s;
            b.TwoSidedQuad(l, r, ridgeFront, ridgeBack, TownSwatch.Slate);
            b.TwoSidedQuad(ridgeBack, ridgeFront, rf, lf, TownSwatch.Slate);
            b.Box(new Vector3(0f, 1.15f, 0f) * s, new Vector3(1.6f, 0.8f, 1f) * s, TownSwatch.Walnut, 0f, TownSwatch.WoodDark);
            b.Log(new Vector3(-0.5f, 0.85f, -0.55f) * s, new Vector3(-0.5f, 0.85f, 0.55f) * s, 0.18f * s, TownSwatch.Iron, 6);
            b.Log(new Vector3(0.5f, 0.85f, -0.55f) * s, new Vector3(0.5f, 0.85f, 0.55f) * s, 0.18f * s, TownSwatch.Iron, 6);
            b.Cylinder(new Vector3(1.6f, 2.2f, -1.3f) * s, 0.14f * s, 0.18f * s, 0.32f * s, 5, TownSwatch.LampGlow, TownSwatch.Iron);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(-1.2f, 0.95f, 0.9f) * s, new Vector3(0.7f, 0.5f, 0.6f) * s, TownSwatch.WoodWeathered);
            }
        }

        /// <summary>A serac: a leaning tower of blue ice, a cap of snow, a fallen block at its foot.</summary>
        public static void Serac(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.35f : -0.3f;
            b.Cylinder(Vector3.down * 0.4f * s, 1.5f * s, 0.9f * s, 3.4f * s, 5, TownSwatch.Glass, TownSwatch.Snow, variant * 0.13f);
            b.Log(new Vector3(0f, 2.8f, 0f) * s, new Vector3(lean, 5.2f, 0.2f) * s, 0.7f * s, TownSwatch.Glass, 4, TownSwatch.Snow);
            b.Blob(new Vector3(lean, 5.4f, 0.2f) * s, new Vector3(1.5f, 0.6f, 1.4f) * s, TownSwatch.Snow);
            b.Blob(new Vector3(1.6f, 0.3f, 0.8f) * s, new Vector3(1.2f, 0.9f, 1f) * s, (variant & 2) == 0 ? TownSwatch.Pearl : TownSwatch.Glass);
        }

        /// <summary>A wooden wind wheel on a round stone tower: four cloth sails on a hub, a tail vane.</summary>
        public static void WindWheel(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.3f * s, 1.5f * s, 1.15f * s, 4.4f * s, 7, TownSwatch.Stone, TownSwatch.WoodDark);
            b.Cylinder(new Vector3(0f, 4.1f, 0f) * s, 1.3f * s, 0f, 1.4f * s, 7, TownSwatch.Slate);
            Vector3 hub = new Vector3(0f, 4.4f, -1.5f) * s;
            b.Log(new Vector3(0f, 4.4f, 0f) * s, hub, 0.12f * s, TownSwatch.WoodDark, 4);
            float phase = variant * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                float a = phase + i * Mathf.PI * 0.5f;
                var dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                var side = new Vector3(-dir.y, dir.x, 0f) * 0.45f * s;
                Vector3 tip = hub + dir * 3.2f * s;
                b.Log(hub, tip, 0.06f * s, TownSwatch.WoodWeathered, 4);
                b.TwoSidedQuad(hub + dir * 0.8f * s, tip, tip + side, hub + dir * 0.8f * s + side, (i & 1) == 0 ? TownSwatch.Sail : TownSwatch.ClothWhite);
            }

            b.Log(new Vector3(0f, 4.4f, 0f) * s, new Vector3(0f, 4.6f, 1.8f) * s, 0.06f * s, TownSwatch.WoodWeathered, 4);
            b.TwoSidedQuad(new Vector3(0f, 4.3f, 1.3f) * s, new Vector3(0f, 4.3f, 2f) * s, new Vector3(0f, 5.1f, 2f) * s, new Vector3(0f, 5f, 1.3f) * s, TownSwatch.ClothRed);
        }

        /// <summary>A line of coloured pennants between two poles from ground point <paramref name="a"/> to <paramref name="c"/>.</summary>
        public static void RibbonLine(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Vector3 ta = a + Vector3.up * 2.4f;
            Vector3 tc = c + Vector3.up * 2.1f;
            b.Log(a + Vector3.down * 0.3f, ta, 0.05f, TownSwatch.WoodWeathered, 4);
            b.Log(c + Vector3.down * 0.3f, tc, 0.05f, TownSwatch.WoodWeathered, 4);
            TownSwatch[] colors = { TownSwatch.ClothRed, TownSwatch.ClothYellow, TownSwatch.ClothGreen, TownSwatch.ClothIndigo, TownSwatch.ClothWhite };
            const int flags = 6;
            Vector3 along = (tc - ta) / flags;
            for (int i = 0; i < flags; i++)
            {
                float sag = Mathf.Sin((i + 0.5f) / flags * Mathf.PI) * 0.35f;
                Vector3 p0 = ta + along * i + Vector3.down * sag;
                Vector3 p1 = ta + along * (i + 0.8f) + Vector3.down * sag;
                Vector3 drop = Vector3.down * 0.45f;
                b.TwoSidedQuad(p0, p1, p1 + drop, p0 + drop, colors[(i + variant) % colors.Length]);
            }
        }

        /// <summary>A square watch tower of stone with a timber gallery on top, a slate roof and a ring of wind bells.</summary>
        public static void WatchTower(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 2.1f, 0f) * s, new Vector3(2.8f, 5f, 2.8f) * s, TownSwatch.Stone, 0f, TownSwatch.WoodDark);
            b.Box(new Vector3(0f, 4.9f, 0f) * s, new Vector3(3.6f, 0.2f, 3.6f) * s, TownSwatch.WoodLight, 0f, TownSwatch.WoodLight);
            foreach (Vector2 c in new[] { new Vector2(-1.7f, -1.7f), new Vector2(1.7f, -1.7f), new Vector2(-1.7f, 1.7f), new Vector2(1.7f, 1.7f) })
            {
                b.Log(new Vector3(c.x, 4.9f, c.y) * s, new Vector3(c.x, 6.3f, c.y) * s, 0.09f * s, TownSwatch.WoodDark, 4);
                b.Cylinder(new Vector3(c.x * 1.05f, 5.8f, c.y * 1.05f) * s, 0.1f * s, 0.05f * s, 0.25f * s, 5, TownSwatch.Bronze);
            }

            b.Box(new Vector3(0f, 5.3f, 0f) * s, new Vector3(3.5f, 0.6f, 3.5f) * s, TownSwatch.WoodWeathered);
            b.Cylinder(new Vector3(0f, 6.3f, 0f) * s, 2.7f * s, 0f, 1.4f * s, 4, TownSwatch.Slate, null, 0.5f);
            b.Box(new Vector3(0f, 2.6f, -1.42f) * s, new Vector3(0.5f, 0.8f, 0.06f) * s, TownSwatch.Black);
            if ((variant & 1) == 0)
            {
                b.Log(new Vector3(0f, 7.7f, 0f) * s, new Vector3(0f, 8.6f, 0f) * s, 0.04f * s, TownSwatch.WoodDark, 3);
                b.TwoSidedQuad(new Vector3(0f, 8.55f, 0f) * s, new Vector3(0.8f, 8.45f, 0f) * s, new Vector3(0.8f, 8.15f, 0f) * s, new Vector3(0f, 8.2f, 0f) * s,
                    TownSwatch.ClothRed);
            }
        }

        /// <summary>A great shed scale of Aydahar stuck slanting in the ground: dark, a green-gold sheen on its edge.</summary>
        public static void DragonScale(TownMeshBuilder b, float s, int variant)
        {
            float tilt = (variant & 1) == 0 ? 0.6f : -0.5f;
            Vector3 foot = Vector3.down * 0.5f * s;
            Vector3 top = new Vector3(tilt, 3.4f, 0.3f) * s;
            Vector3 l = new Vector3(-1.3f, 0.6f, 0f) * s, r = new Vector3(1.3f, 0.6f, 0.1f) * s;
            Vector3 thick = new Vector3(0f, 0f, 0.25f) * s;
            b.TwoSidedQuad(foot, l, top, r, TownSwatch.Basalt);
            b.TwoSidedQuad(foot + thick, r + thick, top + thick, l + thick, TownSwatch.Verdigris);
            b.Log(l, top, 0.07f * s, TownSwatch.Gold, 3);
            b.Log(top, r, 0.07f * s, TownSwatch.Gold, 3);
            b.Blob(new Vector3(0.4f, 0.1f, 0.6f) * s, new Vector3(1f, 0.5f, 0.8f) * s, TownSwatch.Snow);
        }

        /// <summary>A blue crevasse line on the ice: a dark-blue slit with pale lips (ground detail).</summary>
        public static void Crevasse(TownMeshBuilder b, float s)
        {
            b.QuadFacing(new Vector3(-2.2f, 0.04f, -0.12f) * s, new Vector3(-2.2f, 0.04f, 0.12f) * s, new Vector3(2.2f, 0.04f, 0.2f) * s, new Vector3(2.2f, 0.04f, -0.05f) * s, Vector3.up, TownSwatch.Indigo);
            b.QuadFacing(new Vector3(-2.4f, 0.03f, 0.12f) * s, new Vector3(-2.4f, 0.03f, 0.42f) * s, new Vector3(2.4f, 0.03f, 0.5f) * s, new Vector3(2.4f, 0.03f, 0.2f) * s, Vector3.up, TownSwatch.Glass);
            b.QuadFacing(new Vector3(-2.4f, 0.03f, -0.4f) * s, new Vector3(-2.4f, 0.03f, -0.12f) * s, new Vector3(2.4f, 0.03f, -0.05f) * s, new Vector3(2.4f, 0.03f, -0.32f) * s, Vector3.up, TownSwatch.Glass);
        }

        /// <summary>A marmot's burrow: a mound of bare earth with its dark hole (ground detail).</summary>
        public static void MarmotMound(TownMeshBuilder b, float s)
        {
            b.Blob(new Vector3(0f, 0.05f, 0f), new Vector3(1.3f, 0.4f, 1.1f) * s, TownSwatch.Dirt);
            b.Disc(new Vector3(0.15f * s, 0.22f * s, -0.1f * s), 0.22f * s, 5, TownSwatch.Black);
            b.Blob(new Vector3(-0.5f * s, 0.04f, 0.45f * s), new Vector3(0.4f, 0.16f, 0.35f) * s, TownSwatch.DirtDark);
        }

        /// <summary>Ice shards and frost on the ground (ground detail).</summary>
        public static void IceShards(TownMeshBuilder b, float s)
        {
            Spike(b, Vector3.zero, new Vector3(0.1f, 0.6f, 0.05f) * s, 0.12f * s, TownSwatch.Glass);
            Spike(b, new Vector3(0.3f, 0f, 0.2f) * s, new Vector3(0.45f, 0.4f, 0.3f) * s, 0.09f * s, TownSwatch.LimeWhite);
            Spike(b, new Vector3(-0.25f, 0f, 0.15f) * s, new Vector3(-0.4f, 0.35f, 0.2f) * s, 0.08f * s, TownSwatch.Glass);
            b.Disc(new Vector3(0f, 0.02f, 0.1f * s), 0.55f * s, 5, TownSwatch.Snow);
        }
    }
}
