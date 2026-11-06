using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 10 — Picos de Farnkesh: the peaks' own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous like
    /// the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Pinnacles"/>: sheer round towers of sandstone with domed tops (one per cell of a jittered grid,
    /// most cells filled; Meteora's form) standing out of a low floor; the trails and the clearings keep to the floor (the smooth
    /// part), the towers rise round them;</item>
    /// <item><see cref="ReliefShape.CloudIsles"/>: flat-topped stone isles with sheer sides over a low cloud floor (where the
    /// noise field rises over a threshold, a plateau with a slightly domed top);</item>
    /// <item><see cref="ReliefShape.NarrowGorge"/>: high walls over a level floor that winds along the lines where a signed noise
    /// field crosses zero (a network of narrow gorges, the wind's way).</item>
    /// </list>
    /// The vale, the river, the glades, the meadows and the cloud sea reuse the rolling default; the vines and the foot woods
    /// <see cref="ReliefShape.Hills"/>; the hermitages' cliff <see cref="ReliefShape.Terraced"/>; the scree
    /// <see cref="ReliefShape.Peaks"/>. Towns, posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The Picos de Farnkesh shapes; false for the other ones (the caller then draws them).</summary>
        private static bool FarnkeshShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Pinnacles:
                {
                    // A low floor with a faint swell; one tower per 0.48-unit cell (~95 m), most cells filled.
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 3.3f, z * 0.7f + 5.2f, 2);
                    float tower = PinnacleTower(x, z);
                    smooth = a * 0.05f * floor;
                    shaped = a * (0.05f * floor + 0.95f * tower);
                    return true;
                }

                case ReliefShape.CloudIsles:
                {
                    // Isles where the field passes 0.56: sheer sides (a narrow ramp of the field), a gently domed top.
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.6f + 8.1f, z * 0.6f + 2.7f, 2);
                    float n = OverworldNoise.Perlin(x * 1.9f + 6.6f, z * 1.9f + 3.1f);
                    float isle = SmoothStep(0.54f, 0.6f, n);
                    float top = 0.8f + 0.2f * SmoothStep(0.6f, 0.85f, n) + 0.08f * OverworldNoise.Signed(x * 5f + 2.2f, z * 5f + 7.7f);
                    smooth = a * 0.04f * floor;
                    shaped = a * (0.04f * floor + 0.96f * isle * top);
                    return true;
                }

                case ReliefShape.NarrowGorge:
                {
                    // |signed noise| is small along winding lines: the gorge floor there, walls rising fast either side, ragged rims.
                    float v = Mathf.Abs(OverworldNoise.Signed(x * 0.9f + 1.4f, z * 0.9f + 9.6f));
                    float wall = SmoothStep(0.15f, 0.3f, v);
                    float rim = 0.75f + 0.25f * OverworldNoise.Ridged(x * 2.2f + 4.9f, z * 2.2f + 0.8f);
                    smooth = a * 0.05f;
                    shaped = a * (0.05f + 0.95f * wall * rim);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }

        /// <summary>
        /// The sandstone towers at (<paramref name="x"/>, <paramref name="z"/>) in relief units: 0 on the floor, 1 on the top of the
        /// tallest; continuous (the max of round towers, each one zero at its foot: a steep flank under a domed cap).
        /// </summary>
        private static float PinnacleTower(float x, float z)
        {
            const float cell = 0.48f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float best = 0f;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = TowerHash(i, j);
                    if ((h & 0xFF) < 60)
                    {
                        continue; // an empty cell: open floor
                    }

                    float jx = ((h >> 8) & 0xFF) / 255f;
                    float jz = ((h >> 16) & 0xFF) / 255f;
                    float size = 0.5f + 0.5f * ((h >> 24) & 0xFF) / 255f;
                    float dx = x - (i + 0.25f + 0.5f * jx) * cell;
                    float dz = z - (j + 0.25f + 0.5f * jz) * cell;
                    float r = 0.13f * (0.7f + 0.3f * size);
                    float d = Mathf.Sqrt(dx * dx + dz * dz) / r;
                    if (d < 1f)
                    {
                        // Flank: rises from 0 at the foot (d = 1) to 0.85 at d = 0.6; the cap domes the last 15 %.
                        float flank = SmoothStep(1f, 0.6f, d);
                        float cap = 1f - d * d;
                        best = Mathf.Max(best, size * (0.85f * flank + 0.15f * Mathf.Max(0f, cap) * flank));
                    }
                }
            }

            return best;
        }

        private static uint TowerHash(int i, int j)
        {
            unchecked
            {
                uint h = (uint)i * 0x9E3779B1u ^ (uint)j * 0x85EBCA77u ^ 0x5BD1E995u;
                h ^= h >> 16;
                h *= 0x7FEB352Du;
                h ^= h >> 15;
                h *= 0x846CA68Bu;
                h ^= h >> 16;
                return h;
            }
        }
    }
}
