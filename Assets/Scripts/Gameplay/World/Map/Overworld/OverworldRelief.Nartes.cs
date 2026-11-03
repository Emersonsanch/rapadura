using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 9 — Estepe dos Nartes: the steppe's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and
    /// continuous like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Kurgans"/>: round burial mounds (one per cell of a jittered grid, most cells filled) on a
    /// flat steppe floor; the trails and the clearings keep to the floor (the smooth part), the mounds rise round them;</item>
    /// <item><see cref="ReliefShape.Massif"/>: the Caucasus under Ohya's summit: tilted rock strata in long hogback ridges
    /// (a gentle back, a steep scarp) under sharp crests of ridged noise, on a broad high swell.</item>
    /// </list>
    /// The steppe, the camp, the rivers and the prairies reuse the rolling default; the birches, the dream vale, the taiga and
    /// the svan valleys <see cref="ReliefShape.Hills"/>; the giants' hills <see cref="ReliefShape.Terraced"/> (the rings of
    /// their ramparts); the chain's crags <see cref="ReliefShape.Peaks"/>. Towns, posts, gates and trails flatten every one of
    /// them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The Estepe dos Nartes shapes; false for the other ones (the caller then draws them).</summary>
        private static bool NartesShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Kurgans:
                {
                    // A flat floor with a faint swell; one round mound per 0.62-unit cell (~105 m), most cells filled.
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.6f + 1.9f, z * 0.6f + 7.4f, 2);
                    float mound = KurganMound(x, z);
                    smooth = a * 0.05f * floor;
                    shaped = a * (0.05f * floor + 0.95f * mound);
                    return true;
                }

                case ReliefShape.Massif:
                {
                    // Strata dipping south-west: a gentle back rising to a steep scarp, repeated; crests of ridged noise on top.
                    float swell = Mathf.Clamp01(0.5f + 0.8f * OverworldNoise.Fbm(x * 0.55f + 4.2f, z * 0.55f + 1.6f, 2));
                    float u = (x * 0.8f + z * 0.6f) * 1.7f + 0.5f * OverworldNoise.Signed(x * 0.7f + 2.8f, z * 0.7f + 9.3f);
                    float f = u - Mathf.Floor(u);
                    float hogback = SmoothStep(0f, 0.82f, f) * (1f - SmoothStep(0.86f, 1f, f));
                    float ridge = OverworldNoise.Ridged(x * 1.2f + 6.1f, z * 1.2f + 3.4f);
                    smooth = a * (0.2f + 0.45f * swell);
                    shaped = a * (0.15f + 0.35f * swell + 0.22f * hogback + 0.28f * ridge * ridge * (0.4f + 0.6f * swell));
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }

        /// <summary>
        /// The kurgans' mounds at (<paramref name="x"/>, <paramref name="z"/>) in relief units: 0 on the floor, 1 on the top of the
        /// tallest; continuous (the max of round domes, each one zero at its rim).
        /// </summary>
        private static float KurganMound(float x, float z)
        {
            const float cell = 0.62f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float best = 0f;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = MoundHash(i, j);
                    if ((h & 0xFF) < 70)
                    {
                        continue; // an empty cell: open steppe
                    }

                    float jx = ((h >> 8) & 0xFF) / 255f;
                    float jz = ((h >> 16) & 0xFF) / 255f;
                    float size = 0.55f + 0.45f * ((h >> 24) & 0xFF) / 255f;
                    float dx = x - (i + 0.2f + 0.6f * jx) * cell;
                    float dz = z - (j + 0.2f + 0.6f * jz) * cell;
                    float r = 0.19f * size;
                    float d2 = (dx * dx + dz * dz) / (r * r);
                    if (d2 < 1f)
                    {
                        float dome = 1f - d2;
                        best = Mathf.Max(best, size * dome * dome * (3f - 2f * dome));
                    }
                }
            }

            return best;
        }

        private static uint MoundHash(int i, int j)
        {
            unchecked
            {
                uint h = (uint)i * 0x8DA6B343u ^ (uint)j * 0xD8163841u ^ 0x2C1B3C6Du;
                h ^= h >> 15;
                h *= 0x2C1B3C6Du;
                h ^= h >> 12;
                h *= 0x297A2D39u;
                h ^= h >> 15;
                return h;
            }
        }
    }
}
