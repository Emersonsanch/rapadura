using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 7 — Teto do Mundo: the high mountain's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and
    /// continuous like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Moraines"/>: long bent ridges of gravel side by side, boulder humps riding them, the
    /// trails running on the mean so they never climb every ridge;</item>
    /// <item><see cref="ReliefShape.IceTongue"/>: the glacier, a broad smooth swell cut across by shallow crevasse grooves;</item>
    /// <item><see cref="ReliefShape.SeracField"/>: a high floor of ice broken into sharp-edged blocks (the seracs);</item>
    /// <item><see cref="ReliefShape.Peaks"/>: sharp crests of ridged noise, the highest ground of the world.</item>
    /// </list>
    /// Sangkent's terraces, the apricot valley and the snowy terraces reuse <see cref="ReliefShape.Terraced"/>; the pastures
    /// and the steppe <see cref="ReliefShape.Hills"/> and the rolling default; the Wakhan valley and the fort's crag
    /// <see cref="ReliefShape.Gorges"/>. Towns, posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The Teto do Mundo shapes; false for the other ones (the caller then draws them).</summary>
        private static bool PamirShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Moraines:
                {
                    // Ridges about east–west (the glacier flowed south), bent by noise; boulder humps on the crests.
                    float u = z * 1.3f + 0.45f * OverworldNoise.Signed(x * 0.5f + 4.1f, z * 0.4f + 0.7f);
                    float f = Mathf.Abs(u - Mathf.Floor(u) - 0.5f);
                    float ridge = 1f - SmoothStep(0.05f, 0.24f, f);
                    float boulder = SmoothStep(0.62f, 0.8f, OverworldNoise.Perlin(x * 4.3f + 2.9f, z * 4.3f + 8.3f));
                    float swell = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.6f + 1.7f, z * 0.6f + 5.5f, 2);
                    smooth = a * (0.3f + 0.2f * swell);
                    shaped = a * (0.15f + 0.2f * swell + 0.5f * ridge + 0.15f * boulder * ridge);
                    return true;
                }

                case ReliefShape.IceTongue:
                {
                    // A broad smooth swell; the crevasses are narrow grooves across the flow (east–west, bent).
                    float swell = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 3.3f, z * 0.7f + 9.1f, 2);
                    float u = z * 3.2f + 0.6f * OverworldNoise.Signed(x * 0.9f + 6.2f, z * 0.3f + 2.4f);
                    float f = Mathf.Abs(u - Mathf.Floor(u) - 0.5f);
                    float groove = 1f - SmoothStep(0.015f, 0.06f, f);
                    float patch = SmoothStep(0.45f, 0.6f, OverworldNoise.Perlin(x * 1.4f + 7.7f, z * 1.4f + 1.3f));
                    smooth = a * (0.35f + 0.45f * swell);
                    shaped = a * (0.35f + 0.45f * swell - 0.14f * groove * patch);
                    return true;
                }

                case ReliefShape.SeracField:
                {
                    // A high, slightly tilted floor; blocks of ice stand on it with steep sides.
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 0.9f, z * 0.8f + 3.7f, 2);
                    float n = OverworldNoise.Perlin(x * 4.6f + 5.1f, z * 4.6f + 2.2f);
                    float block = SmoothStep(0.5f, 0.68f, n);
                    float top = 0.6f + 0.4f * OverworldNoise.Perlin(x * 9f + 3.3f, z * 9f + 6.6f);
                    smooth = a * (0.3f + 0.4f * floor);
                    shaped = a * (0.25f + 0.35f * floor + 0.4f * block * top);
                    return true;
                }

                case ReliefShape.Peaks:
                {
                    // Sharp crests where the gradient noise crosses zero, higher where the broad swell is high.
                    float swell = Mathf.Clamp01(0.5f + 0.8f * OverworldNoise.Fbm(x * 0.6f + 2.5f, z * 0.6f + 7.3f, 2));
                    float ridge = OverworldNoise.Ridged(x * 1.1f + 1.2f, z * 1.1f + 4.8f);
                    smooth = a * (0.15f + 0.55f * swell);
                    shaped = a * (0.1f + 0.4f * swell + 0.5f * ridge * ridge * (0.4f + 0.6f * swell));
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
