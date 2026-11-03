using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 2 — Areias Flamejantes: the desert's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous
    /// like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Dunes"/>: crested dunes across the wind (from the west): a long gentle windward back, a
    /// short steep slip face, sinuous crests broken into barchans, bigger and smaller dune fields;</item>
    /// <item><see cref="ReliefShape.Mesas"/>: a sand floor with flat-topped sandstone mesas and smaller buttes on top, sheer
    /// sides;</item>
    /// <item><see cref="ReliefShape.Canyons"/>: a high plateau of layered sandstone cut by narrow winding canyons, the walls
    /// stepped in strata (the badlands of the Tambor Rubro).</item>
    /// </list>
    /// Trails follow the smooth part (the mean dune height, the floor between the mesas, the plateau's top).
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Share of a dune's wavelength taken by its windward back (the rest is the slip face).</summary>
        public const float DuneBack = 0.72f;

        /// <summary>The desert shapes; false for the other ones (the caller then draws them).</summary>
        private static bool DesertShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Dunes:
                {
                    float swell = 0.22f * (0.5f + 0.5f * OverworldNoise.Fbm(x * 0.3f + 2.7f, z * 0.3f + 9.1f, 2));
                    float amp = DuneAmplitude(x, z);
                    smooth = a * (swell + 0.75f * 0.45f * amp);
                    shaped = a * (swell + 0.75f * amp * DuneProfile(x, z));
                    return true;
                }

                case ReliefShape.Mesas:
                {
                    float m = Mathf.Clamp01(0.5f + 0.95f * OverworldNoise.Fbm(x, z, 3));
                    float top = SmoothStep(0.6f, 0.67f, m);
                    float butte = SmoothStep(0.8f, 0.86f, m);
                    float ripple = 0.04f * (0.5f + 0.5f * OverworldNoise.Signed(x * 3.1f + 4.2f, z * 3.1f + 1.3f));
                    smooth = a * (0.06f + 0.45f * SmoothStep(0.4f, 0.85f, m));
                    shaped = a * (0.04f + ripple + 0.7f * top + 0.24f * butte);
                    return true;
                }

                case ReliefShape.Canyons:
                {
                    float m = OverworldNoise.Fbm(x * 0.5f + 3f, z * 0.5f + 1f, 2);
                    float n = OverworldNoise.Signed(x, z) + 0.4f * OverworldNoise.Signed(x * 2.3f + 5f, z * 2.3f + 3f);
                    float g = n * n / (n * n + 0.03f);
                    smooth = a * (0.74f + 0.1f * m);
                    shaped = a * (0.08f + 0.8f * Terrace(g, Mathf.Max(2, style.TerraceSteps), 0.35f) + 0.1f * (0.5f + 0.5f * m));
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }

        /// <summary>0..1 height of the dune field at (<paramref name="x"/>, <paramref name="z"/>) in relief units (one wavelength = 1).</summary>
        public static float DuneProfile(float x, float z)
        {
            // Crests run about north–south (the wind blows from the west), bent by two octaves of noise.
            float u = x + 0.38f * OverworldNoise.Signed(z * 0.55f + 3.1f, x * 0.3f + 1.7f) + 0.12f * OverworldNoise.Signed(x * 1.7f + 8.3f, z * 1.7f + 2.2f);
            float f = u - Mathf.Floor(u);
            return f < DuneBack ? SmoothStep(0f, DuneBack, f) : 1f - SmoothStep(DuneBack, 1f, f);
        }

        /// <summary>How tall the dunes are here (0.25..1): fields of big dunes, gaps between barchans.</summary>
        private static float DuneAmplitude(float x, float z)
        {
            float field = 0.45f + 0.55f * SmoothStep(0.25f, 0.75f, OverworldNoise.Perlin(x * 0.35f + 7.1f, z * 0.35f + 2.3f));
            float along = 0.55f + 0.45f * OverworldNoise.Perlin(x * 0.8f + 11.3f, z * 1.6f + 5.9f);
            return Mathf.Max(0.25f, field * along);
        }
    }
}
