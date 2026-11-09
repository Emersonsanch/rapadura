using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 15 — Limiar Profundo: the pieces of the pits (low-poly, the shared palette; each built round the origin and placed by the chunk builder): walls
    /// of lamp niches, racks of wicks, oil cisterns, the Artificers' vents and columns of smoke, glowing slabs and piles of cinder, spires and loose stones of the
    /// shaft of the wind, sluices and drowned columns, blind stalagmites, the pylons of the Bridge of the Bōlos and its dark globe, the five lamp pedestals, slabs
    /// of the broken seal and the Gate of the Land of Darkness; and the ground detail stamped by the hundred.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Limiar Profundo piece (Região 15, 320–359).</summary>
        public static bool IsProfundoDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 320 && i < 360;
        }

        public static readonly TownStamp LampEmberStamp = TownMeshBuilder.Record(b => LampEmber(b, 1f));
        public static readonly TownStamp SootPatchStamp = TownMeshBuilder.Record(b => SootPatch(b, 1f));
        public static readonly TownStamp EmberCrackStamp = TownMeshBuilder.Record(b => EmberCrack(b, 1f));
        public static readonly TownStamp WindStreakStamp = TownMeshBuilder.Record(b => WindStreak(b, 1f));
        public static readonly TownStamp SludgeRingStamp = TownMeshBuilder.Record(b => SludgeRing(b, 1f));
        public static readonly TownStamp ShadowPoolStamp = TownMeshBuilder.Record(b => ShadowPool(b, 1f));
        public static readonly TownStamp LightThreadStamp = TownMeshBuilder.Record(b => LightThread(b, 1f));
        public static readonly TownStamp SealRuneStamp = TownMeshBuilder.Record(b => SealRune(b, 1f));
        public static readonly TownStamp AshSpeckStamp = TownMeshBuilder.Record(b => AshSpeck(b, 1f));
        public static readonly TownStamp CopperFilingStamp = TownMeshBuilder.Record(b => CopperFiling(b, 1f));
        public static readonly TownStamp WickFiberStamp = TownMeshBuilder.Record(b => WickFiber(b, 1f));

        /// <summary>A low wall of basalt with a row of three lit niches.</summary>
        public static void LampNicheWall(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 1f, 0f) * s, new Vector3(3.6f, 2f, 0.8f) * s, TownSwatch.LampBasalt, variant * 13f, TownSwatch.BasaltDust);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(-1.1f + i * 1.1f, 1.15f, -0.42f) * s, new Vector3(0.42f, 0.6f, 0.05f) * s, TownSwatch.ThousandLamps);
            }
        }

        /// <summary>A rack of wicks drying on two posts.</summary>
        public static void WickRack(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-1.4f, 0f, 0f) * s, new Vector3(-1.4f, 2f, 0f) * s, 0.08f * s, TownSwatch.LampCopper, 4);
            b.Log(new Vector3(1.4f, 0f, 0f) * s, new Vector3(1.4f, 2f, 0f) * s, 0.08f * s, TownSwatch.LampCopper, 4);
            b.Log(new Vector3(-1.5f, 2f, 0f) * s, new Vector3(1.5f, 2f, 0f) * s, 0.06f * s, TownSwatch.LampCopper, 4);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.1f + i * 0.55f;
                b.Box(new Vector3(x, 1.5f, 0f) * s, new Vector3(0.16f, 0.95f - (variant + i & 1) * 0.2f, 0.03f) * s, TownSwatch.WickLinen);
            }
        }

        /// <summary>A round cistern of oil, copper bands round it.</summary>
        public static void OilCistern(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.25f * s, 1.15f * s, 1.3f * s, 8, TownSwatch.LampBasalt, TownSwatch.OilDark, variant * 0.4f);
            b.Ring(new Vector3(0f, 0.7f, 0f) * s, 1.2f * s, 1.3f * s, 8, TownSwatch.LampCopper, TownSwatch.LampCopper);
            b.Disc(new Vector3(0f, 1.31f, 0f) * s, 1.05f * s, 8, TownSwatch.ThousandLamps);
        }

        /// <summary>One of the Artificers' vents: a copper drum with a fan of blades.</summary>
        public static void ArtificerVent(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.3f * s, 1.1f * s, 1.5f * s, 8, TownSwatch.LampCopper, TownSwatch.Basalt, variant * 0.5f);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + variant * 17f) * Mathf.Deg2Rad;
                Vector3 c = new Vector3(Mathf.Cos(a) * 0.7f, 1.55f, Mathf.Sin(a) * 0.7f) * s;
                b.Box(c, new Vector3(1.1f, 0.06f, 0.26f) * s, TownSwatch.Bronze, a * Mathf.Rad2Deg);
            }
        }

        /// <summary>A leaning column of grey smoke.</summary>
        public static void SmokeColumn(TownMeshBuilder b, float s, int variant)
        {
            float lean = (variant & 3) * 0.12f;
            b.Blob(new Vector3(0f, 1f, 0f) * s, new Vector3(1.5f, 2f, 1.4f) * s, TownSwatch.Granite);
            b.Blob(new Vector3(lean, 3.1f, 0.1f) * s, new Vector3(1.1f, 1.9f, 1.1f) * s, TownSwatch.Stone);
            b.Blob(new Vector3(lean * 2f, 4.9f, 0.2f) * s, new Vector3(0.75f, 1.3f, 0.75f) * s, TownSwatch.StoneDark);
        }

        /// <summary>A flat slab of the floor that lights, cracks glowing.</summary>
        public static void EmberStone(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.12f, 0f) * s, new Vector3(2.4f, 0.24f, 2.4f) * s, TownSwatch.Basalt, variant * 19f);
            b.Box(new Vector3(0f, 0.25f, 0f) * s, new Vector3(1.8f, 0.02f, 0.2f) * s, TownSwatch.Ember, variant * 37f);
            b.Box(new Vector3(0f, 0.25f, 0f) * s, new Vector3(0.2f, 0.02f, 1.8f) * s, TownSwatch.Ember, variant * 37f);
        }

        /// <summary>A pile of cinders, an ember in its heart.</summary>
        public static void CinderPile(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.4f, 0f) * s, new Vector3(1.8f, 0.8f, 1.6f) * s, TownSwatch.Basalt);
            b.Blob(new Vector3(0.1f, 0.65f, 0f) * s, new Vector3(0.6f, 0.4f, 0.55f) * s, TownSwatch.Ember);
        }

        /// <summary>A thin spire of rock carved by the wind, a torn streamer at its top.</summary>
        public static void WindSpire(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1f * s, 0.2f * s, (4.2f + (variant & 3) * 0.4f) * s, 5, TownSwatch.MoonSilver, TownSwatch.Stone, variant * 0.6f);
            b.Box(new Vector3(0.6f, 4f, 0f) * s, new Vector3(1.2f, 0.18f, 0.03f) * s, TownSwatch.ColdWater, variant * 23f);
        }

        /// <summary>A loose stone held up on a thin neck of rock, as if floating.</summary>
        public static void FloatStone(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.5f * s, 0.18f * s, 2.2f * s, 5, TownSwatch.Granite, null, variant * 0.3f);
            b.Blob(new Vector3(0f, 2.7f, 0f) * s, new Vector3(1.8f, 0.9f, 1.5f) * s, TownSwatch.MoonSilver);
            b.Blob(new Vector3(0f, 3.3f, 0f) * s, new Vector3(0.9f, 0.3f, 0.8f) * s, TownSwatch.Stone);
        }

        /// <summary>A sluice gate of the Artificers: two posts, a wheel and a plank.</summary>
        public static void SluiceGate(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(-1.4f, 1.1f, 0f) * s, new Vector3(0.4f, 2.2f, 0.4f) * s, TownSwatch.LampBasalt, variant * 11f);
            b.Box(new Vector3(1.4f, 1.1f, 0f) * s, new Vector3(0.4f, 2.2f, 0.4f) * s, TownSwatch.LampBasalt, variant * 11f);
            b.Box(new Vector3(0f, 1f, 0f) * s, new Vector3(2.5f, 1.2f, 0.15f) * s, TownSwatch.Bronze);
            b.Ring(new Vector3(0f, 2.4f, 0f) * s, 0.28f * s, 0.42f * s, 8, TownSwatch.LampCopper, TownSwatch.LampCopper);
        }

        /// <summary>A sunken column standing in the black water, its crown broken.</summary>
        public static void DrownedColumn(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.75f * s, 0.65f * s, (2.3f + (variant & 3) * 0.3f) * s, 7, TownSwatch.MurkWater, TownSwatch.Moss, variant * 0.6f);
            b.Cylinder(new Vector3(0f, 2.3f, 0f) * s, 0.75f * s, 0.25f * s, 0.5f * s, 7, TownSwatch.OilDark, null, variant * 0.3f);
        }

        /// <summary>A pale stalagmite with no eyes to see it.</summary>
        public static void BlindStalagmite(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1f * s, 0.12f * s, (3.2f + (variant & 3) * 0.5f) * s, 6, TownSwatch.Black, TownSwatch.ElamGold, variant * 0.5f);
            b.Cylinder(new Vector3(0.9f, 0f, 0.4f) * s, 0.5f * s, 0.08f * s, 1.5f * s, 5, TownSwatch.AbyssIndigo, null, variant * 0.7f);
        }

        /// <summary>A pylon of light of one of the ten flights of the Bridge of the Bōlos.</summary>
        public static void BridgePylon(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.5f * s, 0.32f * s, 3.2f * s, 7, TownSwatch.AbyssBlack, TownSwatch.KemetGold, variant * 0.3f);
            b.Blob(new Vector3(0f, 3.5f, 0f) * s, new Vector3(0.55f, 0.55f, 0.55f) * s, TownSwatch.ThousandLamps);
        }

        /// <summary>The dark globe of the Novelo, a thin gold thread round it (it rests half sunk on the black ground).</summary>
        public static void NoveloGlobe(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 5.5f, 0f) * s, new Vector3(13f, 13f, 13f) * s, TownSwatch.AbyssBlack);
            b.Ring(new Vector3(0f, 5.5f, 0f) * s, 6.2f * s, 6.6f * s, 14, TownSwatch.KemetGold, TownSwatch.KemetGold);
            b.Ring(new Vector3(0f, 1.2f, 0f) * s, 2.6f * s, 3f * s, 12, TownSwatch.MoonLilac, TownSwatch.MoonLilac);
        }

        /// <summary>One of the five lamp pedestals of the Selo dos Artífices.</summary>
        public static void LampPedestal(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.8f * s, 0.55f * s, 1.3f * s, 7, TownSwatch.LampBasalt, TownSwatch.KemetGold, variant * 0.4f);
            b.Cylinder(new Vector3(0f, 1.3f, 0f) * s, 0.5f * s, 0.34f * s, 0.34f * s, 7, TownSwatch.LampCopper);
            b.Blob(new Vector3(0f, 1.75f, 0f) * s, new Vector3(0.34f, 0.5f, 0.34f) * s, TownSwatch.ThousandLamps);
        }

        /// <summary>A cracked slab of the Artificers' seal lying on the ground.</summary>
        public static void SealSlab(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.3f, 0f) * s, new Vector3(2.8f, 0.6f, 1.8f) * s, TownSwatch.LampBasalt, variant * 19f, TownSwatch.BasaltDust);
            b.Box(new Vector3(0.2f, 0.61f, 0f) * s, new Vector3(1.6f, 0.02f, 0.1f) * s, TownSwatch.KemetGold, variant * 41f);
        }

        /// <summary>The Gate of the Land of Darkness: two great uprights and a lintel of cracked stone, only black behind it.</summary>
        public static void DarkGate(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(-14f, 7f, 0f) * s, new Vector3(4f, 14f, 3f) * s, TownSwatch.LampBasalt, variant * 7f, TownSwatch.BasaltDust);
            b.Box(new Vector3(14f, 7f, 0f) * s, new Vector3(4f, 14f, 3f) * s, TownSwatch.LampBasalt, variant * 7f, TownSwatch.BasaltDust);
            b.Box(new Vector3(0f, 15f, 0f) * s, new Vector3(34f, 3f, 3f) * s, TownSwatch.AbyssBlack, variant * 7f, TownSwatch.BasaltDust);
            b.Box(new Vector3(0f, 7f, 0.8f) * s, new Vector3(24f, 13f, 0.4f) * s, TownSwatch.Black);
            b.Box(new Vector3(0f, 7f, 1.05f) * s, new Vector3(8f, 0.24f, 0.05f) * s, TownSwatch.KemetGold);
        }

        private static void GroundMark(TownMeshBuilder b, float s, float x0, float z0, float x1, float z1, TownSwatch swatch)
        {
            b.Quad(new Vector3(x0, 0.03f, z0) * s, new Vector3(x0, 0.03f, z1) * s, new Vector3(x1, 0.03f, z1) * s, new Vector3(x1, 0.03f, z0) * s, swatch);
        }

        /// <summary>A spilt ember of lamp oil, faintly lit (ground detail).</summary>
        public static void LampEmber(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.42f * s, 6, TownSwatch.ThousandLamps);

        /// <summary>A patch of soot (ground detail).</summary>
        public static void SootPatch(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 1.1f * s, 7, TownSwatch.Black);

        /// <summary>A crack lit from below (ground detail).</summary>
        public static void EmberCrack(TownMeshBuilder b, float s)
        {
            GroundMark(b, s, -1.1f, -0.06f, 1.1f, 0.06f, TownSwatch.Ember);
            GroundMark(b, s, -0.3f, 0.06f, -0.2f, 0.7f, TownSwatch.Ember);
        }

        /// <summary>A streak of wind scoured on the stone (ground detail).</summary>
        public static void WindStreak(TownMeshBuilder b, float s) => GroundMark(b, s, -1.6f, -0.05f, 1.6f, 0.05f, TownSwatch.ColdWater);

        /// <summary>A ring of slime round a hole of black water (ground detail).</summary>
        public static void SludgeRing(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.7f * s, 7, TownSwatch.Moss);
            b.Disc(new Vector3(0f, 0.04f, 0f) * s, 0.4f * s, 7, TownSwatch.NightWater);
        }

        /// <summary>A pool of shadow, a gold fleck in it (ground detail).</summary>
        public static void ShadowPool(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.9f * s, 7, TownSwatch.AbyssBlack);
            b.Disc(new Vector3(0.3f, 0.04f, 0f) * s, 0.12f * s, 5, TownSwatch.ElamGold);
        }

        /// <summary>A thread of light on the bridge (ground detail).</summary>
        public static void LightThread(TownMeshBuilder b, float s) => GroundMark(b, s, -0.05f, -1.4f, 0.05f, 1.4f, TownSwatch.KemetGold);

        /// <summary>A rune of the seal cut in the ground (ground detail).</summary>
        public static void SealRune(TownMeshBuilder b, float s)
        {
            GroundMark(b, s, -0.6f, -0.05f, 0.6f, 0.05f, TownSwatch.KemetGold);
            GroundMark(b, s, -0.05f, -0.6f, 0.05f, 0.6f, TownSwatch.KemetGold);
        }

        /// <summary>A fleck of grey ash (ground detail).</summary>
        public static void AshSpeck(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.26f * s, 5, TownSwatch.BasaltDust);

        /// <summary>A filing of copper (ground detail).</summary>
        public static void CopperFiling(TownMeshBuilder b, float s) => GroundMark(b, s, -0.5f, -0.04f, 0.5f, 0.04f, TownSwatch.LampCopper);

        /// <summary>A loose fiber of wick (ground detail).</summary>
        public static void WickFiber(TownMeshBuilder b, float s)
        {
            GroundMark(b, s, -0.5f, -0.03f, 0.4f, 0.03f, TownSwatch.WickLinen);
            GroundMark(b, s, 0.3f, 0.03f, 0.36f, 0.3f, TownSwatch.WickLinen);
        }
    }
}
