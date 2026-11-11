using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 8 — Planície de Mingcheng: the plain's pieces (low-poly, the shared palette; each built round the origin and
    /// placed by the chunk builder): weeping willows, red paper lanterns on posts, little pavilions with curved roofs, stone
    /// guardian lions and stele turtles (the Leão de Pedra and the Tartaruga de Pedra of Docs/Mapas100.md 41 and 43), pits of
    /// terracotta soldiers, the great wall's runs and its beacon towers (45), bamboo clumps, karst towers, flat-crowned
    /// mountain pines, the Torres Brancas' bronze mirrors and white spires (42), the water wheel and the scarecrows of the
    /// paddies (44); and the ground detail stamped by the hundred: millet heads and bamboo shoots.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Planície de Mingcheng piece (Região 8, 180–199).</summary>
        public static bool IsMingchengDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 180 && i < 200;
        }

        public static readonly TownStamp MilletStamp = TownMeshBuilder.Record(b => MilletTuft(b, 1f));
        public static readonly TownStamp BambooShootStamp = TownMeshBuilder.Record(b => BambooShoots(b, 1f));

        /// <summary>A weeping willow: a leaning trunk, a rounded crown and long curtains of hanging green.</summary>
        public static void Willow(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.4f : -0.35f;
            Vector3 top = new Vector3(lean, 2.6f, 0.1f) * s;
            b.Log(Vector3.down * 0.3f * s, top, 0.22f * s, TownSwatch.Trunk, 5);
            b.Crown(top + Vector3.up * 0.5f * s, new Vector3(2.6f, 1.2f, 2.4f) * s, TownSwatch.Vine, variant * 31f);
            for (int i = 0; i < 6; i++)
            {
                float a = (i / 6f + variant * 0.07f) * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 hang = top + dir * 1.15f * s + Vector3.up * 0.4f * s;
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.55f * s;
                Vector3 foot = hang + dir * 0.25f * s + Vector3.down * 2.3f * s;
                b.TwoSidedQuad(hang - side, hang + side, foot + side * 0.7f, foot - side * 0.7f, (i & 1) == 0 ? TownSwatch.Grass : TownSwatch.Vine);
            }
        }

        /// <summary>A post with a red paper lantern, a little roof over it.</summary>
        public static void RedLantern(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.down * 0.3f * s, new Vector3(0f, 2.6f, 0f) * s, 0.07f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(0f, 2.5f, 0f) * s, new Vector3(0.55f, 2.5f, 0f) * s, 0.04f * s, TownSwatch.WoodDark, 3);
            b.Cylinder(new Vector3(0.55f, 1.75f, 0f) * s, 0.22f * s, 0.26f * s, 0.6f * s, 6, (variant & 1) == 0 ? TownSwatch.ClothRed : TownSwatch.LampGlow, TownSwatch.Gold);
            b.Cylinder(new Vector3(0.55f, 2.35f, 0f) * s, 0.12f * s, 0.06f * s, 0.12f * s, 4, TownSwatch.Gold);
            b.Cylinder(new Vector3(0f, 2.6f, 0f) * s, 0.3f * s, 0f, 0.25f * s, 4, TownSwatch.Slate, null, 0.5f);
        }

        /// <summary>A small pavilion: four red columns on a stone base, a curved grey-tile roof with upturned eaves, a gold knob.</summary>
        public static void Pavilion(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(3.4f, 0.5f, 3.4f) * s, TownSwatch.Stone, 0f, TownSwatch.Paving);
            foreach (Vector2 c in new[] { new Vector2(-1.2f, -1.2f), new Vector2(1.2f, -1.2f), new Vector2(-1.2f, 1.2f), new Vector2(1.2f, 1.2f) })
            {
                b.Log(new Vector3(c.x, 0.3f, c.y) * s, new Vector3(c.x, 2.5f, c.y) * s, 0.12f * s, TownSwatch.ClothRed, 5);
            }

            b.Box(new Vector3(0f, 2.55f, 0f) * s, new Vector3(3f, 0.2f, 3f) * s, TownSwatch.ClothRed, 0f, TownSwatch.ClothGreen);
            // Curved roof: a wide flat skirt with lifted corners, then a steep cap.
            b.Cylinder(new Vector3(0f, 2.65f, 0f) * s, 2.7f * s, 1.5f * s, 0.45f * s, 4, TownSwatch.Slate, null, 0.5f);
            b.Cylinder(new Vector3(0f, 3.1f, 0f) * s, 1.55f * s, 0f, 1.1f * s, 4, TownSwatch.Slate, null, 0.5f);
            foreach (Vector2 c in new[] { new Vector2(-1.9f, -1.9f), new Vector2(1.9f, -1.9f), new Vector2(-1.9f, 1.9f), new Vector2(1.9f, 1.9f) })
            {
                b.Log(new Vector3(c.x * 0.8f, 2.8f, c.y * 0.8f) * s, new Vector3(c.x, 3.15f, c.y) * s, 0.06f * s, TownSwatch.Slate, 3);
            }

            b.Cylinder(new Vector3(0f, 4.1f, 0f) * s, 0.12f * s, 0f, 0.35f * s, 4, TownSwatch.Gold);
            if ((variant & 1) == 0)
            {
                b.Cylinder(new Vector3(0.9f, 1.7f, -1.25f) * s, 0.16f * s, 0.2f * s, 0.45f * s, 6, TownSwatch.ClothRed, TownSwatch.Gold);
            }
        }

        /// <summary>A stone guardian lion on its plinth: a crouched body, a curled mane, one paw on a ball.</summary>
        public static void StoneLion(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch stone = (variant & 2) == 0 ? TownSwatch.Stone : TownSwatch.Granite;
            b.Box(new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.4f, 1.1f, 2f) * s, TownSwatch.StoneDark, 0f, TownSwatch.Stone);
            b.Blob(new Vector3(0f, 1.25f, 0.15f) * s, new Vector3(0.9f, 0.9f, 1.4f) * s, stone);
            b.Blob(new Vector3(0f, 1.9f, -0.5f) * s, new Vector3(1.1f, 1.1f, 0.9f) * s, stone);
            b.Blob(new Vector3(0f, 2f, -0.95f) * s, new Vector3(0.6f, 0.55f, 0.4f) * s, TownSwatch.StoneDark);
            b.Blob(new Vector3((variant & 1) == 0 ? 0.35f : -0.35f, 1f, -0.85f) * s, new Vector3(0.4f, 0.4f, 0.4f) * s, TownSwatch.StoneDark);
        }

        /// <summary>A stone turtle carrying a stele on its back.</summary>
        public static void StoneTurtle(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.35f, 0f) * s, new Vector3(1.8f, 0.8f, 2.4f) * s, TownSwatch.StoneDark);
            b.Blob(new Vector3(0f, 0.45f, -1.35f) * s, new Vector3(0.6f, 0.5f, 0.7f) * s, TownSwatch.Stone);
            b.Box(new Vector3(0f, 1.9f, 0.1f) * s, new Vector3(1.2f, 2.4f, 0.32f) * s, TownSwatch.Stone, 0f, TownSwatch.StoneDark);
            b.Box(new Vector3(0f, 3.2f, 0.1f) * s, new Vector3(1.35f, 0.35f, 0.4f) * s, TownSwatch.StoneDark);
            b.Box(new Vector3(0f, 2f, -0.08f) * s, new Vector3(0.7f, 1.4f, 0.04f) * s, (variant & 1) == 0 ? TownSwatch.Black : TownSwatch.Gold);
        }

        /// <summary>A pit of terracotta soldiers: an earthen rim, three rows of clay figures, one horse.</summary>
        public static void TerracottaPit(TownMeshBuilder b, float s, int variant)
        {
            b.Ring(new Vector3(0f, 0.05f, 0f), 3.4f * s, 4.2f * s, 8, TownSwatch.Loam);
            for (int row = -1; row <= 1; row++)
            {
                for (int i = -2; i <= 2; i++)
                {
                    if (((i + row + variant) & 3) == 3)
                    {
                        continue; // a gap: a soldier still in the earth
                    }

                    var at = new Vector3(i * 1.2f, 0f, row * 1.5f) * s;
                    b.Cylinder(at + Vector3.down * 0.2f * s, 0.3f * s, 0.26f * s, 1.1f * s, 5, TownSwatch.RedClay);
                    b.Blob(at + Vector3.up * 1.05f * s, new Vector3(0.36f, 0.4f, 0.36f) * s, TownSwatch.AdobeOchre);
                }
            }

            b.Box(new Vector3(2.9f, 0.6f, 0f) * s, new Vector3(0.6f, 0.7f, 1.6f) * s, TownSwatch.RedClay);
            b.Box(new Vector3(2.9f, 1.05f, -0.85f) * s, new Vector3(0.4f, 0.6f, 0.5f) * s, TownSwatch.AdobeOchre);
        }

        /// <summary>
        /// A run of the great wall from ground point <paramref name="a"/> to <paramref name="c"/>: grey stone walls sunk into the
        /// slope, the paved walkway on top and crenellations along both edges.
        /// </summary>
        public static void GreatWallRun(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Run(b, a, c, 3.6f, -1.6f, 3.4f, TownSwatch.Stone, TownSwatch.Paving);
            Vector3 d = c - a;
            d.y = 0f;
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            Vector3 along = d / length;
            Vector3 n = new Vector3(-along.z, 0f, along.x);
            float yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            int merlons = Mathf.Max(1, Mathf.RoundToInt(length / 2f));
            for (int i = 0; i < merlons; i++)
            {
                Vector3 m = Vector3.Lerp(a, c, (i + 0.5f) / merlons) + Vector3.up * 3.75f;
                b.Box(m + n * 1.6f, new Vector3(0.4f, 0.7f, 0.9f), TownSwatch.StoneDark, yaw);
                if (((i + variant) & 1) == 0)
                {
                    b.Box(m - n * 1.6f, new Vector3(0.4f, 0.5f, 0.9f), TownSwatch.StoneDark, yaw);
                }
            }
        }

        /// <summary>A beacon tower of the wall: square grey stone, a crenellated top with its beacon basin, a red banner.</summary>
        public static void BeaconTower(TownMeshBuilder b, float s, int variant)
        {
            // The solid foot up to the wall's deck, then two side blocks and a roof: the wall's walk (local z) passes through.
            b.Box(new Vector3(0f, 1.3f, 0f) * s, new Vector3(5f, 4.2f, 5f) * s, TownSwatch.Stone, 0f, TownSwatch.Paving);
            foreach (float side in new[] { -1.85f, 1.85f })
            {
                b.Box(new Vector3(side, 4.75f, 0f) * s, new Vector3(1.3f, 2.7f, 5f) * s, TownSwatch.Stone);
            }

            b.Box(new Vector3(0f, 5.75f, 0f) * s, new Vector3(5f, 0.7f, 5f) * s, TownSwatch.Stone, 0f, TownSwatch.Paving);
            foreach (Vector2 c in new[] { new Vector2(-2.2f, -2.2f), new Vector2(0f, -2.2f), new Vector2(2.2f, -2.2f), new Vector2(-2.2f, 2.2f), new Vector2(0f, 2.2f),
                         new Vector2(2.2f, 2.2f), new Vector2(-2.2f, 0f), new Vector2(2.2f, 0f) })
            {
                b.Box(new Vector3(c.x, 6.4f, c.y) * s, new Vector3(0.7f, 0.7f, 0.7f) * s, TownSwatch.StoneDark);
            }

            b.Cylinder(new Vector3(0f, 6.1f, 0f) * s, 0.9f * s, 1.1f * s, 0.6f * s, 6, TownSwatch.Iron, (variant & 1) == 0 ? TownSwatch.Ember : TownSwatch.StoneDark);
            b.Log(new Vector3(2f, 6.1f, -2f) * s, new Vector3(2f, 8.1f, -2f) * s, 0.05f * s, TownSwatch.WoodDark, 3);
            b.TwoSidedQuad(new Vector3(2f, 8f, -2f) * s, new Vector3(2.9f, 7.9f, -2f) * s, new Vector3(2.9f, 7.3f, -2f) * s, new Vector3(2f, 7.4f, -2f) * s, TownSwatch.ClothRed);
        }

        /// <summary>The wall's walk carried over a gap in its body (a gate over a trail): a stone slab 3 m over the ground, nothing under it.</summary>
        public static void GreatWallDeckSpan(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Run(b, a, c, 3.6f, 3f, 3.4f, TownSwatch.StoneDark, TownSwatch.Paving);
        }

        /// <summary>A clump of bamboo: thin jointed culms, tufts of leaves up high, leaning out.</summary>
        public static void Bamboo(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f + variant * 0.11f) * Mathf.PI * 2f;
                var foot = new Vector3(Mathf.Cos(a) * 0.35f, -0.3f, Mathf.Sin(a) * 0.35f) * s;
                var tip = new Vector3(Mathf.Cos(a) * 1.1f, 5.2f + 0.6f * ((i + variant) % 3), Mathf.Sin(a) * 1.1f) * s;
                b.Log(foot, tip, 0.07f * s, (i & 1) == 0 ? TownSwatch.Vine : TownSwatch.GrassDark, 4);
                b.Crown(tip + Vector3.down * 0.4f * s, new Vector3(1.2f, 0.7f, 1.1f) * s, (i % 3) == 0 ? TownSwatch.Grass : TownSwatch.Vine, i * 47f);
            }

            b.Crown(new Vector3(0f, 3.4f, 0f) * s, new Vector3(1.4f, 0.8f, 1.3f) * s, TownSwatch.GrassDark, variant * 29f);
        }

        /// <summary>A karst tower of grey limestone: stacked tapering blocks, green ledges, a pine on top.</summary>
        public static void KarstPillar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.5f * s, 2.6f * s, 2f * s, 4f * s, 6, TownSwatch.Stone, TownSwatch.Moss, variant * 0.3f);
            b.Cylinder(new Vector3(0.3f, 3.4f, 0.1f) * s, 1.9f * s, 1.3f * s, 4f * s, 6, TownSwatch.LimeWhite, TownSwatch.Moss, variant * 0.5f);
            b.Cylinder(new Vector3(0.1f, 7.2f, -0.1f) * s, 1.25f * s, 0.6f * s, 2.6f * s, 5, TownSwatch.Stone, TownSwatch.Pine);
            b.Crown(new Vector3(0.1f, 10.1f, -0.1f) * s, new Vector3(1.4f, 0.6f, 1.3f) * s, TownSwatch.Pine, variant * 17f);
            b.Crown(new Vector3(1.6f, 4f, 0.6f) * s, new Vector3(0.9f, 0.5f, 0.8f) * s, TownSwatch.GrassDark, variant * 41f);
        }

        /// <summary>An old mountain pine: a bent trunk, two or three flat layered crowns.</summary>
        public static void MountainPine(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.7f : -0.6f;
            Vector3 mid = new Vector3(lean * 0.5f, 1.8f, 0f) * s;
            Vector3 top = new Vector3(lean, 3.4f, 0.2f) * s;
            b.Log(Vector3.down * 0.3f * s, mid, 0.24f * s, TownSwatch.Trunk, 5);
            b.Log(mid, top, 0.17f * s, TownSwatch.Trunk, 5);
            b.Log(mid, mid + new Vector3(-lean * 2.2f, 0.6f, 0.3f) * s, 0.09f * s, TownSwatch.Trunk, 3);
            b.Crown(top + Vector3.up * 0.2f * s, new Vector3(2.6f, 0.5f, 2.2f) * s, TownSwatch.Pine, variant * 37f);
            b.Crown(mid + new Vector3(-lean * 2.2f, 0.8f, 0.3f) * s, new Vector3(1.8f, 0.4f, 1.6f) * s, TownSwatch.LeafDark, variant * 13f);
            if ((variant & 2) == 0)
            {
                b.Crown(top + new Vector3(0.4f, 0.9f, -0.2f) * s, new Vector3(1.5f, 0.35f, 1.3f) * s, TownSwatch.Pine, variant * 53f);
            }
        }

        /// <summary>A bronze mirror on a white post, tilted to pass the light down; a gold ring round it.</summary>
        public static void MirrorPost(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.9f, 0f) * s, new Vector3(0.4f, 2.2f, 0.4f) * s, TownSwatch.LimeWhite, 0f, TownSwatch.Gold);
            b.Log(new Vector3(0f, 2f, 0f) * s, new Vector3(0f, 2.4f, 0.15f) * s, 0.06f * s, TownSwatch.Gold, 3);
            var c = new Vector3(0f, 2.65f, 0.2f) * s;
            b.Cylinder(c + new Vector3(0f, 0f, -0.05f) * s, 0.55f * s, 0.55f * s, 0.08f * s, 8, TownSwatch.Bronze, (variant & 1) == 0 ? TownSwatch.Glass : TownSwatch.Gold);
        }

        /// <summary>A small white spire on a stepped Meroë base, a glass-and-gold cap.</summary>
        public static void WhiteSpire(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.3f * s, 1.9f * s, 1.3f * s, 1.4f * s, 4, TownSwatch.Sandstone, TownSwatch.LimeWhite, 0.5f);
            b.Cylinder(new Vector3(0f, 1.1f, 0f) * s, 0.8f * s, 0.55f * s, 4.6f * s, 6, TownSwatch.LimeWhite, TownSwatch.Gold);
            b.Ring(new Vector3(0f, 3.4f, 0f) * s, 0.55f * s, 0.75f * s, 6, TownSwatch.Gold);
            b.Cylinder(new Vector3(0f, 5.7f, 0f) * s, 0.6f * s, 0f, 1.4f * s, 6, (variant & 1) == 0 ? TownSwatch.Glass : TownSwatch.Gold);
        }

        /// <summary>A wooden water wheel in its stone race, with the little mill house beside it.</summary>
        public static void WaterWheel(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(1.6f, 1f, 0f) * s, new Vector3(2.4f, 2.4f, 2.6f) * s, TownSwatch.Plaster, 0f, TownSwatch.Thatch);
            b.Cylinder(new Vector3(1.6f, 2.2f, 0f) * s, 1.8f * s, 0f, 1f * s, 4, TownSwatch.Thatch, null, 0.5f);
            b.Box(new Vector3(-0.6f, 0.15f, 0f) * s, new Vector3(1.4f, 0.4f, 3.2f) * s, TownSwatch.Stone, 0f, TownSwatch.Glass);
            Vector3 hub = new Vector3(-0.6f, 1.5f, 0f) * s;
            b.Log(hub + Vector3.left * 0.4f * s, hub + Vector3.right * 1.2f * s, 0.12f * s, TownSwatch.WoodDark, 5);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.25f + variant * 0.2f;
                var dir = new Vector3(0f, Mathf.Sin(a), Mathf.Cos(a)) * 1.4f * s;
                b.Box(hub + new Vector3(-0.2f * s, 0f, 0f), new Vector3(0.12f * s, Mathf.Abs(dir.y) * 2f + 0.15f * s, Mathf.Abs(dir.z) * 2f + 0.15f * s), TownSwatch.WoodWeathered);
            }

            b.Ring(hub + new Vector3(-0.2f * s, 0f, 0f), 1.3f * s, 1.5f * s, 8, TownSwatch.WoodLight);
        }

        /// <summary>A straw scarecrow on its pole: crossed arms, a ragged coat, a wide conical hat.</summary>
        public static void Scarecrow(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.down * 0.3f * s, new Vector3(0f, 2.1f, 0f) * s, 0.05f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(-0.8f, 1.5f, 0f) * s, new Vector3(0.8f, 1.5f, 0f) * s, 0.04f * s, TownSwatch.WoodWeathered, 3);
            b.Box(new Vector3(0f, 1.25f, 0f) * s, new Vector3(0.6f, 0.8f, 0.3f) * s, (variant & 1) == 0 ? TownSwatch.ClothIndigo : TownSwatch.Straw);
            b.Blob(new Vector3(0f, 1.95f, 0f) * s, new Vector3(0.3f, 0.32f, 0.3f) * s, TownSwatch.Straw);
            b.Cylinder(new Vector3(0f, 2.05f, 0f) * s, 0.55f * s, 0f, 0.35f * s, 6, TownSwatch.Thatch);
            b.Blob(new Vector3(0.85f, 1.45f, 0f) * s, new Vector3(0.25f, 0.2f, 0.2f) * s, TownSwatch.Straw);
            b.Blob(new Vector3(-0.85f, 1.45f, 0f) * s, new Vector3(0.25f, 0.2f, 0.2f) * s, TownSwatch.Straw);
        }

        /// <summary>Golden millet heads bending over (ground detail).</summary>
        public static void MilletTuft(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.1f;
                var foot = new Vector3(Mathf.Cos(a) * 0.15f, 0f, Mathf.Sin(a) * 0.15f) * s;
                var bend = foot + new Vector3(Mathf.Cos(a) * 0.25f, 0.8f, Mathf.Sin(a) * 0.25f) * s;
                Spike(b, foot, bend, 0.03f * s, TownSwatch.GrassDark);
                Spike(b, bend, bend + new Vector3(Mathf.Cos(a) * 0.25f, -0.15f, Mathf.Sin(a) * 0.25f) * s, 0.07f * s, TownSwatch.GoldGrass);
            }
        }

        /// <summary>Fallen bamboo leaves and two young shoots (ground detail).</summary>
        public static void BambooShoots(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.02f, 0f), 0.6f * s, 5, TownSwatch.Straw);
            Spike(b, new Vector3(0.2f, 0f, 0.1f) * s, new Vector3(0.22f, 0.45f, 0.1f) * s, 0.08f * s, TownSwatch.Vine);
            Spike(b, new Vector3(-0.25f, 0f, -0.1f) * s, new Vector3(-0.26f, 0.3f, -0.12f) * s, 0.07f * s, TownSwatch.LeafGold);
        }
    }
}
