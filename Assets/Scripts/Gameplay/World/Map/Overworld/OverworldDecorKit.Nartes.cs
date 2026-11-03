using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 9 — Estepe dos Nartes: the steppe's pieces (low-poly, the shared palette; each built round the origin and placed
    /// by the chunk builder): the centaurs' round felt tents on carts and their horse standards (47), stone babas and golden
    /// stags in the Scythian animal style (48), birches of the forest-steppe, giant larches of the taiga (50), the hill giants'
    /// dream tablets, giant stone pots and earthen ramparts (49), svan towers and Amirani's giant chain (52), the stone well of
    /// the prairie (51); and the ground detail stamped by the hundred: feather-grass plumes and poppies.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>An Estepe dos Nartes piece (Região 9, 200–219).</summary>
        public static bool IsNartesDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 200 && i < 220;
        }

        public static readonly TownStamp FeatherStamp = TownMeshBuilder.Record(b => FeatherTuft(b, 1f));
        public static readonly TownStamp PoppyStamp = TownMeshBuilder.Record(b => PoppyTuft(b, 1f));

        /// <summary>A round felt tent on a cart: lattice walls of felt, a domed roof with its smoke ring, two big wheels, a painted door.</summary>
        public static void FeltYurt(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch felt = (variant & 1) == 0 ? TownSwatch.Plaster : TownSwatch.Adobe;
            TownSwatch band = (variant & 2) == 0 ? TownSwatch.ClothRed : TownSwatch.ClothIndigo;
            b.Box(new Vector3(0f, 0.55f, 0f) * s, new Vector3(3.6f, 0.25f, 3.2f) * s, TownSwatch.WoodDark);
            foreach (float side in new[] { -1.65f, 1.65f })
            {
                b.Cylinder(new Vector3(side, 0.55f, -0.6f) * s, 0.6f * s, 0.6f * s, 0.12f * s, 8, TownSwatch.WoodWeathered, TownSwatch.WoodDark);
                b.Cylinder(new Vector3(side, 0.55f, 0.8f) * s, 0.6f * s, 0.6f * s, 0.12f * s, 8, TownSwatch.WoodWeathered, TownSwatch.WoodDark);
            }

            b.Cylinder(new Vector3(0f, 0.68f, 0f) * s, 1.5f * s, 1.5f * s, 1.3f * s, 8, felt);
            b.Cylinder(new Vector3(0f, 1.5f, 0f) * s, 1.53f * s, 1.53f * s, 0.18f * s, 8, band);
            b.Cylinder(new Vector3(0f, 1.98f, 0f) * s, 1.62f * s, 0.35f * s, 0.95f * s, 8, felt, TownSwatch.WoodDark);
            b.Box(new Vector3(0f, 1.15f, -1.5f) * s, new Vector3(0.7f, 1f, 0.06f) * s, (variant & 4) == 0 ? TownSwatch.ClothRed : TownSwatch.ClothYellow);
            b.Log(new Vector3(0f, 0.55f, -1.6f) * s, new Vector3(0f, 0.35f, -2.8f) * s, 0.06f * s, TownSwatch.WoodDark, 4);
        }

        /// <summary>A horse standard (tug): a tall pole, a gold finial, a ring of horsetail tufts hanging under it, a ribbon.</summary>
        public static void HorseStandard(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.down * 0.3f * s, new Vector3(0f, 4.2f, 0f) * s, 0.07f * s, TownSwatch.WoodDark, 4);
            b.Cylinder(new Vector3(0f, 4.2f, 0f) * s, 0.14f * s, 0f, 0.5f * s, 4, TownSwatch.Gold);
            b.Ring(new Vector3(0f, 3.9f, 0f) * s, 0.06f * s, 0.2f * s, 6, TownSwatch.Gold);
            TownSwatch hair = (variant & 1) == 0 ? TownSwatch.Hair : TownSwatch.ClothWhite;
            for (int i = 0; i < 4; i++)
            {
                float a = (i / 4f + variant * 0.05f) * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 top = new Vector3(0f, 3.85f, 0f) * s + dir * 0.12f * s;
                Spike(b, top, top + dir * 0.35f * s + Vector3.down * 1.3f * s, 0.12f * s, hair);
            }

            b.TwoSidedQuad(new Vector3(0.05f, 3.4f, 0f) * s, new Vector3(0.9f, 3.25f, 0f) * s, new Vector3(0.85f, 3.05f, 0f) * s, new Vector3(0.05f, 3.2f, 0f) * s,
                (variant & 2) == 0 ? TownSwatch.ClothRed : TownSwatch.SilkBlue);
        }

        /// <summary>A stone baba (balbal): a squat weathered figure, a round head, hands with a cup at the belly, on a low plinth.</summary>
        public static void StoneBaba(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch stone = (variant & 1) == 0 ? TownSwatch.TerraceStone : TownSwatch.Stone;
            b.Box(new Vector3(0f, 0.05f, 0f) * s, new Vector3(1f, 0.3f, 0.8f) * s, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 0.3f, 0f) * s, 0.42f * s, 0.36f * s, 1.5f * s, 6, stone);
            b.Blob(new Vector3(0f, 2.05f, 0f) * s, new Vector3(0.62f, 0.66f, 0.58f) * s, stone);
            b.Cylinder(new Vector3(0f, 1.05f, -0.36f) * s, 0.14f * s, 0.18f * s, 0.2f * s, 5, (variant & 2) == 0 ? TownSwatch.StoneDark : TownSwatch.Gold);
            b.Box(new Vector3(0f, 1.15f, -0.32f) * s, new Vector3(0.6f, 0.12f, 0.12f) * s, TownSwatch.StoneDark);
        }

        /// <summary>A golden stag in the Scythian animal style: legs folded under, antlers swept back in curls, on a stone base.</summary>
        public static void GoldStag(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.2f, 0f) * s, new Vector3(1.4f, 0.7f, 2.2f) * s, TownSwatch.StoneDark, 0f, TownSwatch.Stone);
            b.Blob(new Vector3(0f, 0.95f, 0.1f) * s, new Vector3(0.6f, 0.55f, 1.4f) * s, TownSwatch.Gold);
            b.Log(new Vector3(0f, 1f, -0.5f) * s, new Vector3(0f, 1.55f, -0.75f) * s, 0.14f * s, TownSwatch.Gold, 5);
            b.Blob(new Vector3(0f, 1.6f, -0.92f) * s, new Vector3(0.28f, 0.3f, 0.45f) * s, TownSwatch.Gold);
            for (int i = 0; i < 4; i++)
            {
                float z = -0.75f + i * 0.35f;
                b.Log(new Vector3(0f, 1.7f, z) * s, new Vector3(0f, 2.05f + 0.1f * ((i + variant) & 1), z + 0.35f) * s, 0.05f * s, TownSwatch.Gold, 3);
            }

            b.Blob(new Vector3(0f, 0.68f, 0.6f) * s, new Vector3(0.7f, 0.25f, 0.5f) * s, TownSwatch.Bronze);
        }

        /// <summary>A birch: two or three thin white trunks with black marks, a light crown of small leaves (gold in some).</summary>
        public static void Birch(TownMeshBuilder b, float s, int variant)
        {
            int trunks = 2 + (variant & 1);
            TownSwatch leaf = (variant & 6) == 6 ? TownSwatch.LeafGold : TownSwatch.Vine;
            for (int i = 0; i < trunks; i++)
            {
                float a = (i / (float)trunks + variant * 0.13f) * Mathf.PI * 2f;
                var foot = new Vector3(Mathf.Cos(a) * 0.25f, -0.3f, Mathf.Sin(a) * 0.25f) * s;
                var top = new Vector3(Mathf.Cos(a) * 0.7f, 4.4f + 0.5f * i, Mathf.Sin(a) * 0.7f) * s;
                b.Log(foot, top, 0.1f * s, TownSwatch.LimeWhite, 5);
                b.Box(Vector3.Lerp(foot, top, 0.35f), new Vector3(0.22f, 0.08f, 0.22f) * s, TownSwatch.Black, a * Mathf.Rad2Deg);
                b.Crown(top + Vector3.down * 0.3f * s, new Vector3(1.3f, 1.6f, 1.2f) * s, i == 0 ? leaf : TownSwatch.Grass, variant * 23f + i * 40f);
            }
        }

        /// <summary>A giant larch: a tall straight trunk, three soft tiers of green-gold needles narrowing to the top.</summary>
        public static void Larch(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch needles = (variant & 3) == 3 ? TownSwatch.LeafGold : (variant & 1) == 0 ? TownSwatch.Vine : TownSwatch.Pine;
            b.Log(Vector3.down * 0.4f * s, new Vector3(0f, 8.5f, 0f) * s, 0.32f * s, TownSwatch.Trunk, 5);
            b.Cylinder(new Vector3(0f, 2.4f, 0f) * s, 2.4f * s, 1.3f * s, 2.2f * s, 6, needles, null, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 4.3f, 0f) * s, 1.9f * s, 0.9f * s, 2f * s, 6, needles, null, variant * 0.5f + 0.4f);
            b.Cylinder(new Vector3(0f, 6.1f, 0f) * s, 1.3f * s, 0f, 2.8f * s, 6, needles, null, variant * 0.2f);
        }

        /// <summary>A dream tablet: a tall rough slab of dark stone, carved lines glowing faintly gold, moss at the foot.</summary>
        public static void DreamTablet(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 4f : -6f;
            b.Box(new Vector3(0f, 1.4f, 0f) * s, new Vector3(1.6f, 3.4f, 0.45f) * s, TownSwatch.StoneDark, lean, TownSwatch.Stone);
            b.Box(new Vector3(0f, 2f, -0.24f) * s, new Vector3(1f, 0.08f, 0.04f) * s, TownSwatch.RuneLight, lean);
            b.Box(new Vector3(0f, 1.6f, -0.24f) * s, new Vector3(0.7f, 0.08f, 0.04f) * s, TownSwatch.RuneLight, lean);
            b.Box(new Vector3(0f, 1.2f, -0.24f) * s, new Vector3(0.9f, 0.08f, 0.04f) * s, (variant & 2) == 0 ? TownSwatch.RuneLight : TownSwatch.Gold, lean);
            b.Blob(new Vector3(0.3f, 0.05f, 0.2f) * s, new Vector3(1.1f, 0.35f, 0.8f) * s, TownSwatch.Moss);
        }

        /// <summary>A giant stone pot of the hill giants on its hearth stones, a wooden spoon as tall as a cousin, good smoke.</summary>
        public static void GiantCauldron(TownMeshBuilder b, float s, int variant)
        {
            foreach (float a in new[] { 0f, 2.1f, 4.2f })
            {
                b.Box(new Vector3(Mathf.Cos(a) * 1.9f, 0.3f, Mathf.Sin(a) * 1.9f) * s, new Vector3(0.9f, 0.9f, 0.9f) * s, TownSwatch.StoneDark, a * 57f);
            }

            b.Cylinder(new Vector3(0f, 0.6f, 0f) * s, 1.7f * s, 2.3f * s, 2f * s, 8, TownSwatch.Stone, (variant & 1) == 0 ? TownSwatch.Ember : TownSwatch.Water);
            b.Ring(new Vector3(0f, 2.62f, 0f) * s, 2.1f * s, 2.45f * s, 8, TownSwatch.StoneDark);
            b.Log(new Vector3(0.6f, 2.2f, 0.2f) * s, new Vector3(1.9f, 4.4f, 0.9f) * s, 0.14f * s, TownSwatch.WoodLight, 4);
            b.Blob(new Vector3(-0.2f, 3.6f, 0f) * s, new Vector3(1.4f, 0.8f, 1.2f) * s, TownSwatch.Plaster);
        }

        /// <summary>A svan tower: a tall square tower of grey stone narrowing upwards, small dark windows, a slate pyramid roof.</summary>
        public static void SvanTower(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch stone = (variant & 1) == 0 ? TownSwatch.Stone : TownSwatch.Granite;
            b.Cylinder(Vector3.down * 0.5f * s, 2.4f * s, 1.95f * s, 9.5f * s, 4, stone, null, 0.5f);
            b.Cylinder(new Vector3(0f, 9f, 0f) * s, 2.2f * s, 2.1f * s, 1.4f * s, 4, TownSwatch.StoneDark, null, 0.5f);
            b.Cylinder(new Vector3(0f, 10.4f, 0f) * s, 2.3f * s, 0f, 1.5f * s, 4, TownSwatch.Slate, null, 0.5f);
            b.Box(new Vector3(0f, 9.6f, -1.5f) * s, new Vector3(0.4f, 0.6f, 0.06f) * s, TownSwatch.Black);
            b.Box(new Vector3(0f, 5.5f, -1.62f) * s, new Vector3(0.3f, 0.5f, 0.06f) * s, TownSwatch.Black);
            b.Box(new Vector3(0f, 1f, -1.78f) * s, new Vector3(0.8f, 1.5f, 0.06f) * s, TownSwatch.WoodDark);
            if ((variant & 2) == 0)
            {
                b.Box(new Vector3(2.6f, 0.6f, 0.4f) * s, new Vector3(2.4f, 1.8f, 2.6f) * s, stone, 0f, TownSwatch.Slate);
            }
        }

        /// <summary>
        /// A run of Amirani's chain from ground point <paramref name="a"/> to <paramref name="c"/>: old iron links lying along the
        /// slope, alternately flat and on edge, each about 1.6 m long.
        /// </summary>
        public static void ChainRun(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Vector3 d = c - a;
            float length = new Vector2(d.x, d.z).magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            int links = Mathf.Max(1, Mathf.RoundToInt(length / 1.6f));
            for (int i = 0; i < links; i++)
            {
                Vector3 m = Vector3.Lerp(a, c, (i + 0.5f) / links) + Vector3.up * 0.35f;
                bool edge = ((i + variant) & 1) == 1;
                TownSwatch iron = ((i + variant) % 5) == 0 ? TownSwatch.Copper : TownSwatch.Iron;
                if (edge)
                {
                    b.Box(m + Vector3.up * 0.25f, new Vector3(0.18f, 0.9f, 1.5f), iron, yaw);
                }
                else
                {
                    b.Box(m, new Vector3(0.9f, 0.18f, 1.5f), iron, yaw);
                }
            }
        }

        /// <summary>A stone well: a round wall of fitted stones, a wooden frame with its pulley and a leather bucket.</summary>
        public static void StoneWell(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.2f * s, 1.3f * s, 1.2f * s, 1.1f * s, 8, TownSwatch.Stone, TownSwatch.Water);
            b.Ring(new Vector3(0f, 0.92f, 0f) * s, 1f * s, 1.32f * s, 8, TownSwatch.StoneDark);
            b.Log(new Vector3(-1.1f, 0.8f, 0f) * s, new Vector3(-1.1f, 2.6f, 0f) * s, 0.08f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(1.1f, 0.8f, 0f) * s, new Vector3(1.1f, 2.6f, 0f) * s, 0.08f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(-1.25f, 2.55f, 0f) * s, new Vector3(1.25f, 2.55f, 0f) * s, 0.07f * s, TownSwatch.WoodDark, 4);
            b.Cylinder(new Vector3(0.3f, 1.6f, 0f) * s, 0.22f * s, 0.26f * s, 0.4f * s, 5, (variant & 1) == 0 ? TownSwatch.WoodWeathered : TownSwatch.Gold);
        }

        /// <summary>A run of a giants' earthen rampart from ground point <paramref name="a"/> to <paramref name="c"/>: a grassy bank, a few stones on top.</summary>
        public static void EarthRampart(TownMeshBuilder b, Vector3 a, Vector3 c, int variant)
        {
            Ridge(b, a + Vector3.down * 0.3f, c + Vector3.down * 0.3f, 2.6f, 1.6f, TownSwatch.Grass, TownSwatch.GrassDark);
            if ((variant & 3) == 0)
            {
                b.Blob(Vector3.Lerp(a, c, 0.5f) + Vector3.up * 1.2f, new Vector3(0.7f, 0.5f, 0.6f), TownSwatch.Stone);
            }
        }

        /// <summary>Feather-grass plumes: thin silver-gold awns bending in the wind (ground detail).</summary>
        public static void FeatherTuft(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * 1.7f;
                var foot = new Vector3(Mathf.Cos(a) * 0.12f, 0f, Mathf.Sin(a) * 0.12f) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.35f + 0.25f, 0.85f, Mathf.Sin(a) * 0.35f) * s;
                Spike(b, foot, tip, 0.035f * s, (i & 1) == 0 ? TownSwatch.Straw : TownSwatch.ClothWhite);
            }
        }

        /// <summary>A clump of red poppies with dark hearts (ground detail).</summary>
        public static void PoppyTuft(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.3f;
                var foot = new Vector3(Mathf.Cos(a) * 0.18f, 0f, Mathf.Sin(a) * 0.18f) * s;
                var head = foot + new Vector3(0f, 0.5f + 0.12f * i, 0f) * s;
                Spike(b, foot, head, 0.025f * s, TownSwatch.GrassDark);
                b.Cylinder(head, 0.06f * s, 0.16f * s, 0.1f * s, 5, TownSwatch.ClothRed, TownSwatch.Black);
            }
        }
    }
}
