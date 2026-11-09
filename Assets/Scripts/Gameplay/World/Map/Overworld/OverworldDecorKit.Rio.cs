using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 13 — Rio das Doze Horas: the river's pieces (low-poly, the shared palette; each built round the origin and placed by the
    /// chunk builder): papyrus clumps, wheat sheaves, stone doorways of the hidden sands, scarab idols, the arches of Mehen's scales, date
    /// palms, night obelisks, Apep's scale slabs, stranded reed hulls; and the ground detail stamped by the hundred: lotus pads, sand
    /// ripples, fire puddles and purple scale chips.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Rio piece (Região 13, 280–299).</summary>
        public static bool IsRioDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 280 && i < 300;
        }

        public static readonly TownStamp LotusPadStamp = TownMeshBuilder.Record(b => LotusPad(b, 1f));
        public static readonly TownStamp SandRippleStamp = TownMeshBuilder.Record(b => SandRipple(b, 1f));
        public static readonly TownStamp FirePuddleStamp = TownMeshBuilder.Record(b => FirePuddle(b, 1f));
        public static readonly TownStamp ScaleChipStamp = TownMeshBuilder.Record(b => ScaleChip(b, 1f));

        /// <summary>A clump of papyrus: five stems, each with an open umbel.</summary>
        public static void PapyrusClump(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.1f, 0f) * s, new Vector3(1.4f, 0.4f, 1.2f) * s, TownSwatch.Mud);
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f + variant * 0.09f) * Mathf.PI * 2f;
                var foot = new Vector3(Mathf.Cos(a) * 0.35f, 0f, Mathf.Sin(a) * 0.35f) * s;
                var tip = foot + new Vector3(Mathf.Cos(a) * 0.3f, 2.6f + 0.35f * (i % 3), Mathf.Sin(a) * 0.3f) * s;
                b.Log(foot, tip, 0.05f * s, TownSwatch.PapyrusGreen, 4);
                b.Blob(tip, new Vector3(0.7f, 0.3f, 0.7f) * s, (i & 1) == 0 ? TownSwatch.Reed : TownSwatch.PapyrusGreen);
            }
        }

        /// <summary>A sheaf of wheat of light: tied stalks with a faintly glowing tip.</summary>
        public static void WheatSheaf(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.5f * s, 0.32f * s, 1.05f * s, 7, TownSwatch.GoldGrass, TownSwatch.GoldGrass, variant * 0.4f);
            b.Cylinder(new Vector3(0f, 0.55f, 0f) * s, 0.36f * s, 0.34f * s, 0.12f * s, 7, TownSwatch.Reed);
            b.Cylinder(new Vector3(0f, 1.05f, 0f) * s, 0.55f * s, 0.2f * s, 0.55f * s, 7, TownSwatch.KemetGold, TownSwatch.StarGlow);
        }

        /// <summary>A doorway of sandstone with a gold lintel stripe (the closed doors of the hidden sands).</summary>
        public static void StoneDoorway(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(4f, 0.3f, 1.3f) * s, TownSwatch.Slate);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * 1.45f, 1.55f, 0f) * s, new Vector3(0.6f, 2.9f, 0.8f) * s, TownSwatch.QuaySandstone);
            }

            b.Box(new Vector3(0f, 3.1f, 0f) * s, new Vector3(4.1f, 0.5f, 1f) * s, TownSwatch.QuaySandstone);
            b.Box(new Vector3(0f, 3.1f, -0.52f) * s, new Vector3(3.8f, 0.18f, 0.04f) * s, TownSwatch.KemetGold);
            if ((variant & 1) == 0)
            {
                b.Box(new Vector3(0f, 1.45f, 0f) * s, new Vector3(2.3f, 2.75f, 0.2f) * s, TownSwatch.Slate);
            }
        }

        /// <summary>A gold scarab on a low plinth of dark stone.</summary>
        public static void ScarabIdol(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.5f, 0.6f, 1.2f) * s, TownSwatch.Slate, variant * 12f);
            b.Blob(new Vector3(0f, 0.95f, 0f) * s, new Vector3(1f, 0.55f, 0.75f) * s, TownSwatch.KemetGold);
            b.Blob(new Vector3(0.45f, 0.9f, 0f) * s, new Vector3(0.4f, 0.28f, 0.4f) * s, TownSwatch.Gold);
        }

        /// <summary>An arch of Mehen's coil: three bent lengths of green scales, gold at the crest.</summary>
        public static void MehenArch(TownMeshBuilder b, float s, int variant)
        {
            const float span = 2.4f;
            Vector3 left = new Vector3(-span, -0.2f, 0f) * s;
            Vector3 apex = new Vector3(0f, 3.2f, 0f) * s;
            Vector3 right = new Vector3(span, -0.2f, 0f) * s;
            Vector3 l2 = new Vector3(-span * 0.6f, 2.5f, 0f) * s;
            Vector3 r2 = new Vector3(span * 0.6f, 2.5f, 0f) * s;
            TownSwatch scales = (variant & 1) == 0 ? TownSwatch.RootGreen : TownSwatch.PapyrusGreen;
            b.Log(left, l2, 0.5f * s, scales, 6);
            b.Log(l2, apex, 0.45f * s, TownSwatch.KemetGold, 6);
            b.Log(apex, r2, 0.45f * s, TownSwatch.KemetGold, 6);
            b.Log(r2, right, 0.5f * s, scales, 6);
            b.Blob(new Vector3(0f, 3.5f, 0f) * s, new Vector3(0.5f, 0.32f, 0.5f) * s, TownSwatch.StarGlow);
        }

        /// <summary>A date palm of the islets: a leaning ringed trunk, a crown of fronds.</summary>
        public static void DatePalm(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 1) == 0 ? 0.6f : -0.5f;
            var p0 = new Vector3(0f, -0.2f, 0f) * s;
            var p1 = new Vector3(lean * 0.6f, 2.8f, 0.1f) * s;
            var p2 = new Vector3(lean, 5f, 0.2f) * s;
            b.Log(p0, p1, 0.22f * s, TownSwatch.Trunk, 5);
            b.Log(p1, p2, 0.18f * s, TownSwatch.Trunk, 5);
            for (int i = 0; i < 6; i++)
            {
                float a = (i / 6f) * Mathf.PI * 2f + variant * 0.5f;
                var d = new Vector3(Mathf.Cos(a) * 1.6f, -0.4f, Mathf.Sin(a) * 1.6f) * s;
                b.Log(p2, p2 + d, 0.09f * s, (i & 1) == 0 ? TownSwatch.Pine : TownSwatch.PapyrusGreen, 4);
            }

            b.Blob(p2, new Vector3(0.5f, 0.3f, 0.5f) * s, TownSwatch.Ember);
        }

        /// <summary>A night obelisk of dark sandstone, a gold cap and a band of painted stars.</summary>
        public static void NightObelisk(TownMeshBuilder b, float s, int variant)
        {
            float h = 4.2f + (variant & 3) * 0.3f;
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(1.8f, 0.4f, 1.8f) * s, TownSwatch.Slate);
            b.Cylinder(new Vector3(0f, 0.3f, 0f) * s, 0.55f * s, 0.32f * s, h * s, 4, TownSwatch.QuaySandstone, null, 0.785f);
            b.Cylinder(new Vector3(0f, 0.3f + h, 0f) * s, 0.32f * s, 0.02f * s, 0.55f * s, 4, TownSwatch.KemetGold, null, 0.785f);
            b.Box(new Vector3(0f, 2.2f, -0.42f) * s, new Vector3(0.4f, 1.2f, 0.03f) * s, TownSwatch.StarGlow);
        }

        /// <summary>A slab of Apep's skin: a tall curved purple scale half-sunk in the sand.</summary>
        public static void ApepScaleSlab(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1.9f, 0.4f, 1.5f) * s, TownSwatch.SandDark);
            b.Crown(new Vector3(0f, 0.8f, 0f) * s, new Vector3(1.4f, 2.2f, 0.7f) * s, (variant & 1) == 0 ? TownSwatch.Violet : TownSwatch.Indigo, variant * 23f);
            b.Blob(new Vector3(0.2f, 1.4f, -0.3f) * s, new Vector3(0.4f, 0.6f, 0.15f) * s, TownSwatch.Lotus);
        }

        /// <summary>A reed hull hauled up on the bank: bound papyrus curling up at both ends, a paddle.</summary>
        public static void StrandedHull(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.32f, 0f) * s, new Vector3(3.8f, 0.7f, 1.3f) * s, TownSwatch.Reed);
            b.Log(new Vector3(-1.8f, 0.3f, 0f) * s, new Vector3(-2.4f, 1.3f, 0f) * s, 0.18f * s, TownSwatch.Reed, 5);
            b.Log(new Vector3(1.8f, 0.3f, 0f) * s, new Vector3(2.4f, 1.3f, 0f) * s, 0.18f * s, (variant & 1) == 0 ? TownSwatch.KemetGold : TownSwatch.Reed, 5);
            b.Log(new Vector3(-1f, 0.7f, 0.5f) * s, new Vector3(1.1f, 0.8f, -0.4f) * s, 0.05f * s, TownSwatch.WoodDark, 4);
        }

        /// <summary>A lotus pad with a flower (ground detail).</summary>
        public static void LotusPad(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.7f * s, 7, TownSwatch.PapyrusGreen);
            b.Blob(new Vector3(0.1f, 0.12f, 0f) * s, new Vector3(0.28f, 0.14f, 0.28f) * s, TownSwatch.Lotus);
        }

        /// <summary>Ripples of dark sand: three pale curved lines (ground detail).</summary>
        public static void SandRipple(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 3; i++)
            {
                float z = -0.5f + i * 0.5f;
                b.Quad(new Vector3(-1.6f, 0.03f, z - 0.05f) * s, new Vector3(-1.6f, 0.03f, z + 0.05f) * s, new Vector3(1.6f, 0.03f, z + 0.05f) * s, new Vector3(1.6f, 0.03f, z - 0.05f) * s, TownSwatch.QuaySandstone);
            }
        }

        /// <summary>A puddle of fire with a ring of ash (ground detail).</summary>
        public static void FirePuddle(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.8f * s, 7, TownSwatch.Ember);
            b.Ring(new Vector3(0f, 0.04f, 0f) * s, 0.8f * s, 1.15f * s, 7, TownSwatch.ColdEmber, TownSwatch.ColdEmber);
        }

        /// <summary>A chip of Apep's purple scale lying on the sand (ground detail).</summary>
        public static void ScaleChip(TownMeshBuilder b, float s)
        {
            b.Quad(new Vector3(-0.6f, 0.03f, -0.2f) * s, new Vector3(-0.6f, 0.03f, 0.2f) * s, new Vector3(0.6f, 0.03f, 0.35f) * s, new Vector3(0.6f, 0.03f, -0.35f) * s, TownSwatch.Violet);
        }
    }
}
