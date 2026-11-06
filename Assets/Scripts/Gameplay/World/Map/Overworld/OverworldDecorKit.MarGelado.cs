using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 11 — Mar Gelado: the frozen sea's pieces (low-poly, the shared palette; each built round the origin and placed by
    /// the chunk builder): the snow domes and long plank houses of the harbour's shore (form only: snow blocks and boards with a
    /// painted moon, no crest poles), crescent boats, whalebone arches set in stone, ice blocks and small icebergs, the reindeer
    /// herders' tents of Omöl's tundra and its lichen boulders, the mammoth tusks and frost crystals of the galleries, ice
    /// lanterns, fishers' racks and the ice beacons of the lighthouse isle; and the ground detail stamped by the hundred: fishing
    /// holes, cracks in the ice and tufts of reindeer lichen.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Mar Gelado piece (Região 11, 240–259).</summary>
        public static bool IsMarGeladoDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 240 && i < 260;
        }

        public static readonly TownStamp FishingHoleStamp = TownMeshBuilder.Record(b => FishingHole(b, 1f));
        public static readonly TownStamp IceCrackStamp = TownMeshBuilder.Record(b => IceCrack(b, 1f));
        public static readonly TownStamp LichenStamp = TownMeshBuilder.Record(b => LichenTuft(b, 1f));

        /// <summary>A snow dome: a round dome of snow, a ring of blocks at its foot, a low entrance tunnel, a vent on top.</summary>
        public static void SnowDome(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.2f, 0f) * s, new Vector3(4.4f, 3.6f, 4.4f) * s, TownSwatch.Snow, variant * 23f);
            b.Ring(new Vector3(0f, 0.05f, 0f) * s, 1.9f * s, 2.4f * s, 8, TownSwatch.Firn, TownSwatch.Pearl);
            b.Box(new Vector3(0f, 0.45f, -2.3f) * s, new Vector3(1.3f, 1.2f, 1.6f) * s, TownSwatch.Snow, 0f, TownSwatch.Firn);
            b.Box(new Vector3(0f, 0.4f, -3.12f) * s, new Vector3(0.7f, 0.8f, 0.06f) * s, TownSwatch.HollowDark);
            if ((variant & 1) == 0)
            {
                b.Cylinder(new Vector3(0f, 1.85f, 0f) * s, 0.22f * s, 0.18f * s, 0.3f * s, 5, TownSwatch.Pearl);
            }

            if ((variant & 2) == 0)
            {
                b.Box(new Vector3(1.6f, 0.9f, -1.1f) * s, new Vector3(0.5f, 0.4f, 0.06f) * s, TownSwatch.WindowLit, -40f);
            }
        }

        /// <summary>A long plank house: a gable of dark boards on a stone sill, a white moon painted on the front, a plank door.</summary>
        public static void PlankLonghouse(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(5.2f, 0.4f, 8.2f) * s, TownSwatch.Stone, 0f, TownSwatch.StoneDark);
            b.Box(new Vector3(0f, 1.4f, 0f) * s, new Vector3(4.8f, 2.4f, 7.8f) * s, TownSwatch.WoodDark, 0f, TownSwatch.WoodDark);
            // The gable roof: two sloped slabs meeting on the ridge.
            b.Box(new Vector3(-1.3f, 3.15f, 0f) * s, new Vector3(3f, 0.2f, 8.4f) * s, TownSwatch.WoodWeathered, 0f, TownSwatch.Snow);
            b.Box(new Vector3(1.3f, 3.15f, 0f) * s, new Vector3(3f, 0.2f, 8.4f) * s, TownSwatch.WoodWeathered, 0f, TownSwatch.Snow);
            b.TwoSidedQuad(new Vector3(-2.4f, 2.6f, -3.92f) * s, new Vector3(0f, 3.9f, -3.92f) * s, new Vector3(2.4f, 2.6f, -3.92f) * s, new Vector3(0f, 2.6f, -3.92f) * s, TownSwatch.WoodDark);
            b.Disc(new Vector3(0f, 2.15f, -3.93f) * s, 0.75f * s, 8, (variant & 1) == 0 ? TownSwatch.ClothWhite : TownSwatch.LilacMist);
            b.Box(new Vector3(0f, 0.95f, -3.93f) * s, new Vector3(0.9f, 1.6f, 0.06f) * s, TownSwatch.DoorPlank);
            b.Log(new Vector3(-2.3f, 0f, -4.2f) * s, new Vector3(-2.3f, 2f, -4.2f) * s, 0.08f * s, TownSwatch.WoodWeathered, 4);
        }

        /// <summary>A crescent boat: a hull curving up at both ends like a half-moon, upturned on the ice (odd variants) or on its keel.</summary>
        public static void CrescentBoat(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch hull = (variant & 2) == 0 ? TownSwatch.WoodDark : TownSwatch.WoodWeathered;
            bool upturned = (variant & 1) == 1;
            float y = upturned ? 0.55f : 0.35f;
            b.Box(new Vector3(0f, y, 0f) * s, new Vector3(1.3f, 0.5f, 3.4f) * s, hull, 0f, upturned ? hull : TownSwatch.WoodLight);
            b.Log(new Vector3(0f, y, 1.6f) * s, new Vector3(0f, y + 1.1f, 2.4f) * s, 0.22f * s, hull, 4);
            b.Log(new Vector3(0f, y, -1.6f) * s, new Vector3(0f, y + 1.1f, -2.4f) * s, 0.22f * s, hull, 4);
            b.Box(new Vector3(0f, y + 0.27f, 0f) * s, new Vector3(1.32f, 0.06f, 0.3f) * s, TownSwatch.Pearl);
            if (!upturned)
            {
                b.Log(new Vector3(0.9f, 0.05f, 0.4f) * s, new Vector3(1.6f, 0.05f, -1.2f) * s, 0.05f * s, TownSwatch.WoodLight, 4);
            }
        }

        /// <summary>A whalebone arch: two curved ribs of bone rising from stone feet and meeting overhead.</summary>
        public static void WhaleboneArch(TownMeshBuilder b, float s, int variant)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * 1.8f, 0.2f, 0f) * s, new Vector3(0.9f, 0.6f, 0.9f) * s, TownSwatch.Stone, 0f, TownSwatch.StoneDark);
                b.Log(new Vector3(side * 1.8f, 0.3f, 0f) * s, new Vector3(side * 1.6f, 2.2f, 0f) * s, 0.2f * s, TownSwatch.Pearl, 5);
                b.Log(new Vector3(side * 1.6f, 2.2f, 0f) * s, new Vector3(side * 0.9f, 3.6f, 0f) * s, 0.17f * s, TownSwatch.Pearl, 5);
                b.Log(new Vector3(side * 0.9f, 3.6f, 0f) * s, new Vector3(side * 0.05f, 4f, 0f) * s, 0.14f * s, TownSwatch.LimeWhite, 5);
            }

            if ((variant & 1) == 0)
            {
                b.Log(new Vector3(-0.05f, 3.95f, 0f) * s, new Vector3(-0.05f, 3.2f, 0f) * s, 0.02f * s, TownSwatch.Thatch, 3);
                b.Disc(new Vector3(0f, 3.15f, 0.05f) * s, 0.22f * s, 6, TownSwatch.LilacMist);
            }
        }

        /// <summary>A block of blue ice thrown up on the surface, tilted, a smaller one leaning on it.</summary>
        public static void IceBlock(TownMeshBuilder b, float s, int variant)
        {
            float tilt = 12f + 9f * (variant & 3);
            b.Box(new Vector3(0f, 0.7f, 0f) * s, new Vector3(2.2f, 1.8f, 1.6f) * s, (variant & 1) == 0 ? TownSwatch.Glass : TownSwatch.SkyBlue, tilt, TownSwatch.Snow);
            b.Box(new Vector3(1.3f, 0.35f, 0.8f) * s, new Vector3(1f, 0.9f, 0.9f) * s, TownSwatch.Pearl, -tilt * 1.7f, TownSwatch.Snow);
        }

        /// <summary>A small iceberg: an angular block of white and blue ice with a flat snowy top and a blue fissure.</summary>
        public static void Iceberg(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.5f * s, 3.4f * s, 2.6f * s, 3.6f * s, 5, TownSwatch.Glass, TownSwatch.Snow, variant * 0.3f);
            b.Cylinder(new Vector3(0.6f, 3.1f, 0.3f) * s, 2.2f * s, 1.5f * s, 2.4f * s, 4, TownSwatch.Pearl, TownSwatch.Snow, variant * 0.5f);
            b.Box(new Vector3(0f, 1.6f, -2.75f) * s, new Vector3(0.5f, 3f, 0.3f) * s, TownSwatch.SkyBlue, 10f);
        }

        /// <summary>A herders' tent: a cone of hides on crossing poles, a door flap, a sled beside it.</summary>
        public static void HerderTent(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch hide = (variant & 1) == 0 ? TownSwatch.Felt : TownSwatch.SteppeLeather;
            b.Cylinder(Vector3.zero, 2.4f * s, 0.25f * s, 4.2f * s, 8, hide, null, variant * 0.2f);
            for (int i = 0; i < 4; i++)
            {
                float a = (i / 4f + 0.12f) * Mathf.PI * 2f;
                b.Log(new Vector3(Mathf.Cos(a) * 0.3f, 3.9f, Mathf.Sin(a) * 0.3f) * s, new Vector3(-Mathf.Cos(a) * 0.4f, 5.2f, -Mathf.Sin(a) * 0.4f) * s, 0.05f * s, TownSwatch.WoodDark, 3);
            }

            b.Box(new Vector3(0f, 0.75f, -2.05f) * s, new Vector3(0.9f, 1.4f, 0.08f) * s, TownSwatch.HorseHair, 0f);
            if ((variant & 2) == 0)
            {
                b.Box(new Vector3(2.6f, 0.25f, 0.6f) * s, new Vector3(0.8f, 0.12f, 2.4f) * s, TownSwatch.WoodLight);
                b.Log(new Vector3(2.25f, 0.05f, -0.7f) * s, new Vector3(2.25f, 0.05f, 1.9f) * s, 0.05f * s, TownSwatch.WoodDark, 3);
                b.Log(new Vector3(2.95f, 0.05f, -0.7f) * s, new Vector3(2.95f, 0.05f, 1.9f) * s, 0.05f * s, TownSwatch.WoodDark, 3);
            }
        }

        /// <summary>A round tundra boulder with yellow-grey lichen patches and a cap of snow.</summary>
        public static void LichenBoulder(TownMeshBuilder b, float s, int variant)
        {
            b.Crown(new Vector3(0f, 0.6f, 0f) * s, new Vector3(2.2f, 1.7f, 1.9f) * s, (variant & 1) == 0 ? TownSwatch.Stone : TownSwatch.Granite, variant * 37f);
            b.Blob(new Vector3(0.3f, 1.35f, 0.1f) * s, new Vector3(1.3f, 0.45f, 1.1f) * s, TownSwatch.Snow);
            b.Blob(new Vector3(-0.85f, 0.7f, 0.2f) * s, new Vector3(0.45f, 0.7f, 0.8f) * s, TownSwatch.Lichen);
            b.Blob(new Vector3(0.6f, 0.5f, -0.8f) * s, new Vector3(0.7f, 0.5f, 0.3f) * s, TownSwatch.SteppeGrass);
        }

        /// <summary>A giant mammoth tusk: a curved ivory horn coming out of the ground in three bent pieces, earth heaped at its root.</summary>
        public static void MammothTusk(TownMeshBuilder b, float s, int variant)
        {
            var p0 = new Vector3(0f, -0.4f, 0f) * s;
            var p1 = new Vector3(0.3f, 1.8f, 0.6f) * s;
            var p2 = new Vector3(1.2f, 3.5f, 1.1f) * s;
            var p3 = new Vector3(2.6f, 4.3f, 0.9f) * s;
            b.Log(p0, p1, 0.55f * s, TownSwatch.Pearl, 6);
            b.Log(p1, p2, 0.42f * s, TownSwatch.LimeWhite, 6);
            b.Log(p2, p3, 0.26f * s, TownSwatch.LimeWhite, 5);
            b.Blob(new Vector3(0f, 0.1f, 0f) * s, new Vector3(2.4f, 0.8f, 2.2f) * s, (variant & 1) == 0 ? TownSwatch.FortEarth : TownSwatch.Snow);
            if ((variant & 2) == 0)
            {
                b.Blob(new Vector3(-0.9f, 0.3f, -0.6f) * s, new Vector3(0.9f, 0.6f, 0.7f) * s, TownSwatch.Glass);
            }
        }

        /// <summary>An ice lantern: a post with a block of clear ice on it and a lilac crescent light inside.</summary>
        public static void IceLantern(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(0f, -0.3f, 0f) * s, new Vector3(0f, 1.5f, 0f) * s, 0.07f * s, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(0f, 1.85f, 0f) * s, new Vector3(0.6f, 0.7f, 0.6f) * s, TownSwatch.Glass, 0f, TownSwatch.Snow);
            b.Blob(new Vector3(0f, 1.85f, 0f) * s, new Vector3(0.3f, 0.42f, 0.12f) * s, (variant & 1) == 0 ? TownSwatch.SereneAir : TownSwatch.WillWisp);
        }

        /// <summary>A fishers' rack: two forked poles and a crossbar hung with dried silver fish.</summary>
        public static void FishRack(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-1.4f, -0.3f, 0f) * s, new Vector3(-1.4f, 2.1f, 0f) * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(1.4f, -0.3f, 0f) * s, new Vector3(1.4f, 2.1f, 0f) * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(-1.6f, 2f, 0f) * s, new Vector3(1.6f, 2f, 0f) * s, 0.05f * s, TownSwatch.WoodDark, 4);
            int fish = 4 + (variant & 1);
            for (int i = 0; i < fish; i++)
            {
                float x = -1.1f + i * 2.2f / (fish - 1);
                b.Box(new Vector3(x, 1.55f, 0f) * s, new Vector3(0.16f, 0.75f, 0.05f) * s, (i & 1) == 0 ? TownSwatch.Pearl : TownSwatch.Iron);
            }
        }

        /// <summary>An ice beacon: a tapering tower of stacked ice blocks on a stone foot, a lamp of Ar Sereno on top.</summary>
        public static void IceBeacon(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.down * 0.3f * s, 2f * s, 1.9f * s, 1.2f * s, 6, TownSwatch.Granite, TownSwatch.Stone);
            b.Cylinder(new Vector3(0f, 0.9f, 0f) * s, 1.6f * s, 1.25f * s, 2.6f * s, 6, TownSwatch.Glass, null, 0.5f);
            b.Cylinder(new Vector3(0f, 3.5f, 0f) * s, 1.25f * s, 0.9f * s, 2.4f * s, 6, TownSwatch.Pearl);
            b.Cylinder(new Vector3(0f, 5.9f, 0f) * s, 1.1f * s, 1.1f * s, 0.25f * s, 6, TownSwatch.StoneDark, TownSwatch.Stone);
            b.Crown(new Vector3(0f, 6.6f, 0f) * s, new Vector3(1f, 1.1f, 1f) * s, (variant & 1) == 0 ? TownSwatch.SereneAir : TownSwatch.WillWisp, variant * 13f);
        }

        /// <summary>A cluster of frost crystals: long hexagonal prisms of ice leaning out of a bed of snow.</summary>
        public static void FrostCrystal(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.05f, 0f) * s, new Vector3(1.8f, 0.4f, 1.6f) * s, TownSwatch.Snow);
            for (int i = 0; i < 4; i++)
            {
                float a = (i / 4f + variant * 0.07f) * Mathf.PI * 2f;
                float h = 1.4f + 0.5f * ((i + variant) % 3);
                var foot = new Vector3(Mathf.Cos(a) * 0.3f, 0f, Mathf.Sin(a) * 0.3f) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.5f, h, Mathf.Sin(a) * 0.5f) * s;
                b.Log(foot, tip, 0.16f * s, (i & 1) == 0 ? TownSwatch.Glass : TownSwatch.SereneAir, 6);
            }
        }

        /// <summary>A fishing hole: a dark disc of open water with a rim of broken ice (ground detail).</summary>
        public static void FishingHole(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.7f * s, 7, TownSwatch.BogWater);
            b.Ring(new Vector3(0f, 0.05f, 0f) * s, 0.7f * s, 1.05f * s, 7, TownSwatch.Snow, TownSwatch.Glass);
        }

        /// <summary>A dark crack across the ice, kinked in two (ground detail).</summary>
        public static void IceCrack(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-1.6f, 0.03f, -0.05f) * s, new Vector3(-1.6f, 0.03f, 0.05f) * s, new Vector3(0f, 0.03f, 0.35f) * s, new Vector3(0f, 0.03f, 0.18f) * s, TownSwatch.Indigo);
            b.Quad(new Vector3(0f, 0.03f, 0.18f) * s, new Vector3(0f, 0.03f, 0.35f) * s, new Vector3(1.7f, 0.03f, -0.1f) * s, new Vector3(1.7f, 0.03f, -0.18f) * s, TownSwatch.Indigo);
        }

        /// <summary>A tuft of reindeer lichen: three pale lumps on the moss (ground detail).</summary>
        public static void LichenTuft(TownMeshBuilder b, float s)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f) * s, new Vector3(0.5f, 0.22f, 0.45f) * s, TownSwatch.Lichen);
            b.Blob(new Vector3(0.35f, 0.06f, 0.2f) * s, new Vector3(0.35f, 0.18f, 0.3f) * s, TownSwatch.SteppeGrass);
            b.Blob(new Vector3(-0.3f, 0.06f, 0.25f) * s, new Vector3(0.3f, 0.16f, 0.3f) * s, TownSwatch.Lichen);
        }
    }
}
