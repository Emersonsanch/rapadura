using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 16 — Reinos do Céu: the pieces of the sky (low-poly, the shared palette; each built round the origin and placed by the chunk builder): banks of cloud,
    /// pier bollards and buoys of light, the rails, masts and crates of the two Barcas, the bronze gears and jars of the Celestial Wheel, the crystal spheres of the Archons,
    /// the trees, singing flowers and fountains of the Garden of Light, cloud steps and the Column of Glory, the cracks in the sky and the pillars that hold them, the bench and
    /// Vô Jorge's rocking chair; and the ground detail stamped by the hundred.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>A Reinos do Céu piece (Região 16, 360–419).</summary>
        public static bool IsCeuDecor(OverworldDecor decor)
        {
            int i = (int)decor;
            return i >= 360 && i < 420;
        }

        public static readonly TownStamp LightSpeckStamp = TownMeshBuilder.Record(b => LightSpeck(b, 1f));
        public static readonly TownStamp GoldRippleStamp = TownMeshBuilder.Record(b => GoldRipple(b, 1f));
        public static readonly TownStamp LilacPlankStamp = TownMeshBuilder.Record(b => LilacPlank(b, 1f));
        public static readonly TownStamp BronzeFilingStamp = TownMeshBuilder.Record(b => BronzeFiling(b, 1f));
        public static readonly TownStamp CrystalChipStamp = TownMeshBuilder.Record(b => CrystalChip(b, 1f));
        public static readonly TownStamp LightPetalStamp = TownMeshBuilder.Record(b => LightPetal(b, 1f));
        public static readonly TownStamp SunGrainStamp = TownMeshBuilder.Record(b => SunGrain(b, 1f));
        public static readonly TownStamp StepMarkStamp = TownMeshBuilder.Record(b => StepMark(b, 1f));
        public static readonly TownStamp SkyCrackLineStamp = TownMeshBuilder.Record(b => SkyCrackLine(b, 1f));
        public static readonly TownStamp ApricotDustStamp = TownMeshBuilder.Record(b => ApricotDust(b, 1f));
        public static readonly TownStamp CloudWispStamp = TownMeshBuilder.Record(b => CloudWisp(b, 1f));
        public static readonly TownStamp GoldMoteStamp = TownMeshBuilder.Record(b => GoldMote(b, 1f));
        public static readonly TownStamp LilacMoteStamp = TownMeshBuilder.Record(b => LilacMote(b, 1f));

        /// <summary>A bank of firm cloud.</summary>
        public static void CloudBank(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.7f, 0f) * s, new Vector3(3.4f, 1.5f, 2.8f) * s, TownSwatch.QuayCloud);
            b.Blob(new Vector3(1.1f, 1.1f, 0.4f + (variant & 1) * 0.3f) * s, new Vector3(1.8f, 1.1f, 1.6f) * s, TownSwatch.QuayCloudShade);
        }

        /// <summary>A short bollard of solid light on a pier.</summary>
        public static void LightBollard(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.42f * s, 0.32f * s, 1.3f * s, 6, TownSwatch.QuayGold, TownSwatch.QuayCloud, variant * 0.4f);
            b.Blob(new Vector3(0f, 1.5f, 0f) * s, new Vector3(0.42f, 0.42f, 0.42f) * s, TownSwatch.SolidLight);
        }

        /// <summary>A buoy of light on the Rio de Luz.</summary>
        public static void LightBuoy(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.8f * s, 0.55f * s, 0.7f * s, 7, TownSwatch.QuayGold, TownSwatch.QuaySkyBlue, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 0.7f, 0f) * s, 0.14f * s, 0.1f * s, 1.5f * s, 5, TownSwatch.QuayGold);
            b.Blob(new Vector3(0f, 2.4f, 0f) * s, new Vector3(0.5f, 0.5f, 0.5f) * s, TownSwatch.SolidLight);
        }

        /// <summary>A low islet of cloud with a gold fleck.</summary>
        public static void CloudIslet(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 0.5f, 0f) * s, new Vector3(4f, 1f, 3.2f) * s, TownSwatch.QuayCloudShade);
            b.Blob(new Vector3(0.4f, 0.95f, 0f) * s, new Vector3(1.2f, 0.4f, 1f) * s, TownSwatch.QuayGold);
        }

        /// <summary>A length of the Mehen serpent, the rail of the Barca da Lua.</summary>
        public static void MehenRail(TownMeshBuilder b, float s, int variant)
        {
            b.Log(new Vector3(-1.8f, 0.7f, 0f) * s, new Vector3(1.8f, 0.7f, 0f) * s, 0.22f * s, TownSwatch.MoonSilver, 5);
            b.Box(new Vector3(-1.5f, 0.35f, 0f) * s, new Vector3(0.18f, 0.7f, 0.18f) * s, TownSwatch.QuayLilac, variant * 11f);
            b.Box(new Vector3(1.5f, 0.35f, 0f) * s, new Vector3(0.18f, 0.7f, 0.18f) * s, TownSwatch.QuayLilac, variant * 11f);
            b.Blob(new Vector3(1.9f, 0.85f, 0f) * s, new Vector3(0.45f, 0.35f, 0.35f) * s, TownSwatch.MoonSilver);
        }

        /// <summary>A crate of silver bound with lilac cord on the moon deck.</summary>
        public static void DeckCrate(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.5f, 0f) * s, new Vector3(1.3f, 1f, 1.1f) * s, TownSwatch.MoonSilver, variant * 17f);
            b.Box(new Vector3(0f, 1.02f, 0f) * s, new Vector3(1.32f, 0.04f, 0.12f) * s, TownSwatch.QuayLilac, variant * 17f);
        }

        /// <summary>The great mast of the Barca da Lua.</summary>
        public static void MoonMast(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.7f * s, 0.3f * s, 13f * s, 7, TownSwatch.MoonSilver, TownSwatch.QuayLilac, variant * 0.4f);
            b.Box(new Vector3(0f, 10f, 0f) * s, new Vector3(7f, 0.22f, 0.22f) * s, TownSwatch.MoonSilver);
            b.Blob(new Vector3(0f, 13.4f, 0f) * s, new Vector3(0.9f, 0.9f, 0.9f) * s, TownSwatch.SolidLight);
        }

        /// <summary>One of the twelve jars of the Celestial Wheel, big as a house, bronze and light of water.</summary>
        public static void CelestialJar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 2.1f * s, 2.8f * s, 2.4f * s, 8, TownSwatch.Bronze, TownSwatch.ElementWater, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 2.4f, 0f) * s, 2.8f * s, 1.5f * s, 1.8f * s, 8, TownSwatch.Bronze);
            b.Disc(new Vector3(0f, 4.2f, 0f) * s, 1.4f * s, 8, TownSwatch.SolidLight);
        }

        /// <summary>A bronze gear lying half in the cloud.</summary>
        public static void BronzeGear(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(new Vector3(0f, 0f, 0f) * s, 1.5f * s, 1.5f * s, 0.5f * s, 10, TownSwatch.Bronze, TownSwatch.QuayGold, variant * 0.5f);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + variant * 7f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 1.65f, 0.25f, Mathf.Sin(a) * 1.65f) * s, new Vector3(0.5f, 0.5f, 0.4f) * s, TownSwatch.Bronze, -a * Mathf.Rad2Deg);
            }
        }

        /// <summary>A crystal sphere of the Archons, a sulking giant dim inside it.</summary>
        public static void ArchonSphere(TownMeshBuilder b, float s, int variant)
        {
            b.Blob(new Vector3(0f, 7f, 0f) * s, new Vector3(13f, 13f, 13f) * s, TownSwatch.QuayGlassPane);
            b.Blob(new Vector3(0f, 5.8f, 0f) * s, new Vector3(4f, 7f, 4f) * s, TownSwatch.BasaltDust);
            b.Ring(new Vector3(0f, 0.8f, 0f) * s, 6.3f * s, 6.9f * s, 14, TownSwatch.QuayGold, TownSwatch.QuayGold);
        }

        /// <summary>A pebble of crystal off an orbit, on a thin stalk of cloud.</summary>
        public static void OrbitShard(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.3f * s, 0.15f * s, 1.6f * s, 5, TownSwatch.QuayCloudShade, null, variant * 0.3f);
            b.Blob(new Vector3(0f, 2.2f, 0f) * s, new Vector3(1.6f, 1.6f, 1.6f) * s, TownSwatch.QuayGlassPane);
        }

        /// <summary>A tree of light in the Garden.</summary>
        public static void LightTree(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.36f * s, 0.2f * s, 2.4f * s, 6, TownSwatch.QuayGold, null, variant * 0.3f);
            b.Blob(new Vector3(0f, 3.1f, 0f) * s, new Vector3(3f, 2.4f, 3f) * s, TownSwatch.SolidLight);
            b.Blob(new Vector3(0.7f, 2.5f, 0.3f) * s, new Vector3(1.6f, 1.2f, 1.6f) * s, TownSwatch.ColumnLight);
        }

        /// <summary>A flower that sings, on a stem of gold.</summary>
        public static void SongFlower(TownMeshBuilder b, float s, int variant)
        {
            b.Log(Vector3.zero, new Vector3(0.1f, 0.9f, 0f) * s, 0.05f * s, TownSwatch.QuayGold, 4);
            b.Blob(new Vector3(0.1f, 1f, 0f) * s, new Vector3(0.6f, 0.3f, 0.6f) * s, (variant & 1) == 0 ? TownSwatch.ElementAir : TownSwatch.ElementFire);
        }

        /// <summary>A small garden fountain of gold.</summary>
        public static void GardenFount(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 1.2f * s, 1f * s, 0.5f * s, 8, TownSwatch.QuayCloud, TownSwatch.QuayGold, variant * 0.5f);
            b.Disc(new Vector3(0f, 0.51f, 0f) * s, 0.9f * s, 8, TownSwatch.ElementWater);
            b.Blob(new Vector3(0f, 1.1f, 0f) * s, new Vector3(0.4f, 0.8f, 0.4f) * s, TownSwatch.ElementWater);
        }

        /// <summary>The triangular dhow sail of the Barca do Sol on its mast.</summary>
        public static void SunSail(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.7f * s, 0.32f * s, 14f * s, 7, TownSwatch.QuayGold, TownSwatch.ElementFire, variant * 0.4f);
            b.Triangle(new Vector3(0f, 2f, 0.3f) * s, new Vector3(0f, 14f, 0.3f) * s, new Vector3(9f, 3f, 0.3f) * s, TownSwatch.QuayCloud);
            b.Triangle(new Vector3(9f, 3f, 0.34f) * s, new Vector3(0f, 14f, 0.34f) * s, new Vector3(0f, 2f, 0.34f) * s, TownSwatch.QuayCloud);
        }

        /// <summary>A coil of golden rope on the sun deck.</summary>
        public static void SunCoil(TownMeshBuilder b, float s, int variant)
        {
            b.Ring(new Vector3(0f, 0.2f, 0f) * s, 0.45f * s, 0.9f * s, 8, TownSwatch.QuayGold, TownSwatch.ElementFire);
            b.Ring(new Vector3(0f, 0.48f, 0f) * s, 0.42f * s, 0.8f * s, 8, TownSwatch.ElementFire, TownSwatch.QuayGold);
        }

        /// <summary>A heap of firm-cloud steps climbing to the Coluna.</summary>
        public static void CloudSteps(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.3f, 0f) * s, new Vector3(3.2f, 0.6f, 2.4f) * s, TownSwatch.QuayCloud, variant * 11f);
            b.Box(new Vector3(0.2f, 0.85f, 0f) * s, new Vector3(2.2f, 0.5f, 1.6f) * s, TownSwatch.QuayCloudShade, variant * 11f);
        }

        /// <summary>The Column of Glory, a pillar of light that goes on up out of sight.</summary>
        public static void GloryColumn(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 7f * s, 5.5f * s, 2f * s, 10, TownSwatch.QuayCloud, TownSwatch.QuayGold, variant * 0.3f);
            b.Cylinder(new Vector3(0f, 2f, 0f) * s, 4.5f * s, 3.8f * s, 38f * s, 12, TownSwatch.ColumnLight, TownSwatch.SolidLight, variant * 0.1f);
            b.Ring(new Vector3(0f, 2.2f, 0f) * s, 4.6f * s, 5.4f * s, 14, TownSwatch.ElementLight, TownSwatch.ElementFire);
        }

        /// <summary>A crack in the sky: a tall black shard with a bright rim, shadow dripping from it.</summary>
        public static void SkyCrack(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 3f, 0f) * s, new Vector3(1.4f, 6f, 0.25f) * s, TownSwatch.AbyssBlack, variant * 23f);
            b.Box(new Vector3(0f, 3f, 0f) * s, new Vector3(1.8f, 6.3f, 0.12f) * s, TownSwatch.SolidLight, variant * 23f);
            b.Blob(new Vector3(0f, 0.4f, 0f) * s, new Vector3(1.8f, 0.8f, 1.4f) * s, TownSwatch.AbyssIndigo);
        }

        /// <summary>A pillar of light carried to a crack to hold the sky.</summary>
        public static void HoldPillar(TownMeshBuilder b, float s, int variant)
        {
            b.Cylinder(Vector3.zero, 0.6f * s, 0.4f * s, 4.2f * s, 8, TownSwatch.SolidLight, TownSwatch.QuayGold, variant * 0.3f);
            b.Blob(new Vector3(0f, 4.5f, 0f) * s, new Vector3(0.7f, 0.7f, 0.7f) * s, TownSwatch.ColumnLight);
        }

        /// <summary>A wooden bench on the porch of the Time After, the color of the iwan.</summary>
        public static void PorchBench(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.45f, 0f) * s, new Vector3(2f, 0.14f, 0.55f) * s, TownSwatch.AdobeOchre, variant * 7f);
            b.Box(new Vector3(-0.8f, 0.22f, 0f) * s, new Vector3(0.14f, 0.44f, 0.5f) * s, TownSwatch.WoodWeathered, variant * 7f);
            b.Box(new Vector3(0.8f, 0.22f, 0f) * s, new Vector3(0.14f, 0.44f, 0.5f) * s, TownSwatch.WoodWeathered, variant * 7f);
        }

        /// <summary>Vô Jorge's rocking chair, empty, in light.</summary>
        public static void RockingChair(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.55f, 0f) * s, new Vector3(0.7f, 0.1f, 0.65f) * s, TownSwatch.AdobeOchre, variant * 5f);
            b.Box(new Vector3(0f, 1f, -0.3f) * s, new Vector3(0.7f, 0.8f, 0.08f) * s, TownSwatch.AdobeOchre, variant * 5f);
            b.Log(new Vector3(-0.35f, 0.04f, 0.4f) * s, new Vector3(-0.35f, 0.04f, -0.42f) * s, 0.05f * s, TownSwatch.QuayGold, 4);
            b.Log(new Vector3(0.35f, 0.04f, 0.4f) * s, new Vector3(0.35f, 0.04f, -0.42f) * s, 0.05f * s, TownSwatch.QuayGold, 4);
        }

        private static void CeuMark(TownMeshBuilder b, float s, float x0, float z0, float x1, float z1, TownSwatch swatch)
        {
            b.Quad(new Vector3(x0, 0.03f, z0) * s, new Vector3(x0, 0.03f, z1) * s, new Vector3(x1, 0.03f, z1) * s, new Vector3(x1, 0.03f, z0) * s, swatch);
        }

        /// <summary>A speck of solid light (ground detail).</summary>
        public static void LightSpeck(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.3f * s, 5, TownSwatch.SolidLight);

        /// <summary>A ripple of gold on the current (ground detail).</summary>
        public static void GoldRipple(TownMeshBuilder b, float s) => CeuMark(b, s, -1.2f, -0.05f, 1.2f, 0.05f, TownSwatch.QuayGold);

        /// <summary>A plank of the moon deck (ground detail).</summary>
        public static void LilacPlank(TownMeshBuilder b, float s) => CeuMark(b, s, -0.9f, -0.12f, 0.9f, 0.12f, TownSwatch.QuayLilac);

        /// <summary>A filing of bronze (ground detail).</summary>
        public static void BronzeFiling(TownMeshBuilder b, float s) => CeuMark(b, s, -0.6f, -0.05f, 0.6f, 0.05f, TownSwatch.Bronze);

        /// <summary>A chip of orbit crystal (ground detail).</summary>
        public static void CrystalChip(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.35f * s, 5, TownSwatch.QuayGlassPane);

        /// <summary>A petal of light (ground detail).</summary>
        public static void LightPetal(TownMeshBuilder b, float s)
        {
            b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.28f * s, 6, TownSwatch.ColumnLight);
            b.Disc(new Vector3(0.25f, 0.04f, 0.1f) * s, 0.14f * s, 5, TownSwatch.ElementAir);
        }

        /// <summary>A grain of sun gold (ground detail).</summary>
        public static void SunGrain(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.22f * s, 5, TownSwatch.ElementFire);

        /// <summary>The edge of a cloud step (ground detail).</summary>
        public static void StepMark(TownMeshBuilder b, float s) => CeuMark(b, s, -1.6f, -0.06f, 1.6f, 0.06f, TownSwatch.ElementLight);

        /// <summary>A thin black crack, bright at its edge (ground detail).</summary>
        public static void SkyCrackLine(TownMeshBuilder b, float s)
        {
            CeuMark(b, s, -1.1f, -0.03f, 1.1f, 0.03f, TownSwatch.AbyssBlack);
            CeuMark(b, s, -0.7f, 0.03f, 0.8f, 0.06f, TownSwatch.SolidLight);
        }

        /// <summary>A little apricot dust from the adobe of the porch (ground detail).</summary>
        public static void ApricotDust(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.42f * s, 6, TownSwatch.AdobeOchre);

        /// <summary>A wisp of cloud (ground detail).</summary>
        public static void CloudWisp(TownMeshBuilder b, float s) => CeuMark(b, s, -1f, -0.18f, 1f, 0.18f, TownSwatch.QuayCloudShade);

        /// <summary>A mote of gold (ground detail).</summary>
        public static void GoldMote(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.14f * s, 5, TownSwatch.QuayGold);

        /// <summary>A mote of lilac (ground detail).</summary>
        public static void LilacMote(TownMeshBuilder b, float s) => b.Disc(new Vector3(0f, 0.03f, 0f) * s, 0.16f * s, 5, TownSwatch.QuayLilac);
    }
}
