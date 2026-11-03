using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 3 — Serra de Kaggen: the mountain pieces (low-poly, the shared palette; each built round the origin and placed
    /// by the chunk builder): herders' rondavels with a litema band, round dry-stone kraals, terrace rows of sorghum,
    /// sandstone shelters with an eland panel (only the animal shapes: no rite), the rock walls of the passes, conical
    /// towers, bronze rune pillars of Napirasu, snow drifts and mist, the Mazans' granite blocks and the high stela.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>Wall segments of a kraal (segment 0 is the gap) and its radius (m, unit scale).</summary>
        public const int KraalSegments = 9;
        public const float KraalRadius = 4f;

        public static void Rondavel(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch wall = (variant & 1) == 0 ? TownSwatch.RedClay : TownSwatch.LimeWhite;
            TownSwatch band = (variant & 2) == 0 ? TownSwatch.ClothIndigo : TownSwatch.ClothYellow;
            b.Cylinder(new Vector3(0f, -1.2f * s, 0f), 2.35f * s, 2.3f * s, 1.25f * s, 9, TownSwatch.Stone);
            b.Cylinder(Vector3.zero, 2.2f * s, 2.15f * s, 2f * s, 9, wall);
            b.Cylinder(new Vector3(0f, 1.15f * s, 0f), 2.19f * s, 2.17f * s, 0.32f * s, 9, band);
            b.Cylinder(new Vector3(0f, 1.95f * s, 0f), 2.75f * s, 0f, 2.2f * s, 9, TownSwatch.Thatch);
            b.Box(new Vector3(0f, 0.8f * s, -2.12f * s), new Vector3(0.85f, 1.6f, 0.16f) * s, TownSwatch.WoodDark);
        }

        public static void Kraal(TownMeshBuilder b, float s, int variant)
        {
            float r = KraalRadius * s;
            for (int i = 1; i < KraalSegments; i++)
            {
                float a0 = i * Mathf.PI * 2f / KraalSegments;
                float a1 = (i + 1) * Mathf.PI * 2f / KraalSegments;
                var p0 = new Vector3(Mathf.Cos(a0) * r, 0f, Mathf.Sin(a0) * r);
                var p1 = new Vector3(Mathf.Cos(a1) * r, 0f, Mathf.Sin(a1) * r);
                Vector3 mid = (p0 + p1) * 0.5f;
                float yaw = Mathf.Atan2(p1.x - p0.x, p1.z - p0.z) * Mathf.Rad2Deg + 90f;
                float h = (0.85f + 0.12f * ((i + variant) % 3)) * s;
                b.Box(new Vector3(mid.x, h * 0.5f - 0.3f * s, mid.z), new Vector3(Vector3.Distance(p0, p1) + 0.2f * s, h + 0.3f * s, 0.6f * s),
                    ((i + variant) & 1) == 0 ? TownSwatch.Stone : TownSwatch.StoneDark, yaw, TownSwatch.Cobble);
            }

            // A trough and a little dung-floor inside.
            b.Box(new Vector3(0.6f * s, 0.2f * s, 0.8f * s), new Vector3(1.4f, 0.4f, 0.5f) * s, TownSwatch.WoodWeathered, 30f, TownSwatch.Water);
        }

        /// <summary>A terrace step from ground point <paramref name="a"/> to <paramref name="c"/>: retaining wall with a sorghum row on top.</summary>
        public static void TerraceRow(TownMeshBuilder b, Vector3 a, Vector3 c, int seed)
        {
            float h01 = TownStyle.Hash01("terraco", seed);
            Run(b, a - Vector3.up * 0.3f, c - Vector3.up * 0.3f, 0.55f, 0f, 0.75f, (seed & 1) == 0 ? TownSwatch.Stone : TownSwatch.StoneDark, TownSwatch.Cobble);
            Vector3 d = c - a;
            d.y = 0f;
            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * 0.95f;
            Vector3 up = Vector3.up * 0.1f;
            Ridge(b, a + n + up, c + n + up, 0.6f, 0.95f + 0.25f * h01, TownSwatch.Vine, TownSwatch.LeafGold);
            b.Blob(Vector3.Lerp(a, c, 0.3f + 0.3f * h01) + n + Vector3.up * (1.15f + 0.2f * h01), new Vector3(0.3f, 0.36f, 0.3f), TownSwatch.RedClay);
            b.Blob(Vector3.Lerp(a, c, 0.8f) + n + Vector3.up * 1.1f, new Vector3(0.28f, 0.34f, 0.28f), (seed & 2) == 0 ? TownSwatch.RedClay : TownSwatch.LeafGold);
        }

        /// <summary>A sandstone overhang (opening towards −z) with an ochre panel of elands on its back wall.</summary>
        public static void PaintedShelter(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 1.6f, 0.9f) * s, new Vector3(6.2f, 4.4f, 1.6f) * s, TownSwatch.Sandstone, 0f, TownSwatch.Thatch);
            b.Box(new Vector3(0f, 3.75f, -0.4f) * s, new Vector3(6.8f, 1f, 3.2f) * s, TownSwatch.Sandstone, 0f, TownSwatch.SandDark);
            b.Crown(new Vector3(-3.4f, 1.3f, -0.1f) * s, new Vector3(2.2f, 3.2f, 2.6f) * s, TownSwatch.RedClay, 20f);
            b.Crown(new Vector3(3.3f, 1.1f, 0f) * s, new Vector3(2f, 2.8f, 2.4f) * s, TownSwatch.Sandstone, 70f);

            // The panel: three elands in ochre and white (body, neck and head, legs), just in front of the wall.
            float z = 0.07f * s;
            for (int k = 0; k < 3; k++)
            {
                float x = (-1.8f + k * 1.8f + ((variant >> k) & 1) * 0.3f) * s;
                float y = (1.6f + (k & 1) * 0.5f) * s;
                TownSwatch paint = k == 1 ? TownSwatch.LimeWhite : TownSwatch.RedClay;
                b.Box(new Vector3(x, y, z), new Vector3(0.9f, 0.34f, 0.04f) * s, paint);
                b.Box(new Vector3(x - 0.5f * s, y + 0.28f * s, z), new Vector3(0.22f, 0.4f, 0.04f) * s, paint);
                b.Box(new Vector3(x - 0.3f * s, y - 0.32f * s, z), new Vector3(0.08f, 0.36f, 0.04f) * s, paint);
                b.Box(new Vector3(x + 0.32f * s, y - 0.32f * s, z), new Vector3(0.08f, 0.36f, 0.04f) * s, paint);
            }
        }

        /// <summary>A tall rock slab of a pass wall (snow on its top in the cold variants).</summary>
        public static void RockSpire(TownMeshBuilder b, float s, int variant, TownSwatch rock)
        {
            if (rock == TownSwatch.Black)
            {
                rock = TownSwatch.Stone;
            }

            b.Crown(new Vector3(0f, 3.2f, 0f) * s, new Vector3(2.8f, 7.6f, 2.1f) * s, rock, variant * 23f);
            b.Crown(new Vector3(0.9f, 1.2f, 0.5f) * s, new Vector3(2.3f, 2.8f, 2f) * s, rock == TownSwatch.Sandstone ? TownSwatch.RedClay : TownSwatch.StoneDark, 40f);
            if ((variant & 3) == 0)
            {
                b.Crown(new Vector3(0.1f, 6.8f, 0f) * s, new Vector3(1.3f, 0.6f, 1.1f) * s, TownSwatch.Snow, 10f);
            }
        }

        public static void ConicalTower(TownMeshBuilder b, float s)
        {
            b.Cylinder(new Vector3(0f, -0.6f * s, 0f), 1.95f * s, 1.25f * s, 6.2f * s, 10, TownSwatch.Stone, TownSwatch.Cobble);
            b.Cylinder(new Vector3(0f, 4.3f * s, 0f), 1.43f * s, 1.37f * s, 0.45f * s, 10, TownSwatch.StoneDark, null, 0.5f);
        }

        public static void BronzePillar(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1.3f, 0.7f, 1.3f) * s, TownSwatch.StoneDark, 0f, TownSwatch.Basalt);
            b.Cylinder(new Vector3(0f, 0.5f * s, 0f), 0.48f * s, 0.42f * s, 3.2f * s, 6, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 1.7f * s, 0f), 0.5f * s, 0.5f * s, 0.3f * s, 6, TownSwatch.Copper);
            b.Cylinder(new Vector3(0f, 3.7f * s, 0f), 0.58f * s, 0f, 0.75f * s, 6, (variant & 1) == 0 ? TownSwatch.Copper : TownSwatch.Gold);
            // The rune that lights up at night.
            b.Box(new Vector3(0f, 2.5f, -0.44f) * s, new Vector3(0.22f, 0.5f, 0.05f) * s, TownSwatch.LampGlow);
        }

        public static void SnowDrift(TownMeshBuilder b, float s)
        {
            b.Crown(new Vector3(0f, 0.02f, 0f) * s, new Vector3(2.8f, 0.36f, 1.9f) * s, TownSwatch.Snow);
            b.Crown(new Vector3(0.9f, 0.04f, 0.5f) * s, new Vector3(1.4f, 0.26f, 1.1f) * s, TownSwatch.LimeWhite, 40f);
        }

        /// <summary>Three flat, pale cushions of mist hanging a little above the ground.</summary>
        public static void MistBank(TownMeshBuilder b, float s)
        {
            b.Crown(new Vector3(0f, 1f, 0f) * s, new Vector3(5.5f, 0.7f, 3.2f) * s, TownSwatch.Snow);
            b.Crown(new Vector3(1.8f, 1.5f, 0.8f) * s, new Vector3(3.4f, 0.55f, 2.4f) * s, TownSwatch.Pearl, 30f);
            b.Crown(new Vector3(-1.9f, 0.7f, -0.5f) * s, new Vector3(3f, 0.5f, 2f) * s, TownSwatch.LimeWhite, 70f);
        }

        public static void GraniteBlock(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.9f, 0f) * s, new Vector3(3.2f, 2.6f, 2.6f) * s, TownSwatch.Stone, 0f, (variant & 1) == 0 ? TownSwatch.Snow : TownSwatch.StoneDark);
            b.Box(new Vector3(1.5f, 0.4f, 1.2f) * s, new Vector3(1.6f, 1.4f, 1.4f) * s, TownSwatch.StoneDark, 25f, (variant & 2) == 0 ? TownSwatch.Snow : TownSwatch.Stone);
        }

        /// <summary>The high stela: a tall carved granite slab with false windows, on a stepped base.</summary>
        public static void Stela(TownMeshBuilder b, float s)
        {
            b.Box(new Vector3(0f, 0.1f, 0f) * s, new Vector3(1.9f, 0.8f, 1.4f) * s, TownSwatch.Stone, 0f, TownSwatch.Cobble);
            b.Box(new Vector3(0f, 3.5f, 0f) * s, new Vector3(1.05f, 6f, 0.55f) * s, TownSwatch.Stone, 0f, TownSwatch.Snow);
            for (int k = 0; k < 3; k++)
            {
                b.Box(new Vector3(0f, (1.6f + k * 1.5f) * s, -0.28f * s), new Vector3(0.5f, 0.6f, 0.04f) * s, TownSwatch.StoneDark);
            }

            b.Crown(new Vector3(0f, 6.6f, 0f) * s, new Vector3(1.05f, 0.7f, 0.55f) * s, TownSwatch.Stone);
        }
    }
}
