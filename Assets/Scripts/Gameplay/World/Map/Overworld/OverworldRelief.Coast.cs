using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 5 — Costa das Monções: the coast's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous
    /// like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Cliffs"/>: a laterite headland in two broad steps, each ending in a sheer bluff, with
    /// coves cut back into it (the bluffs stand where the noise crosses its thresholds: always the same, any platform);</item>
    /// <item><see cref="ReliefShape.Islets"/>: sandbanks just over the water line with low coral humps, flat between them
    /// (the channels and lagoons that cut them are the region's carved water, the valleys keep their shores gentle).</item>
    /// </list>
    /// The beaches need no shape of their own: a carved channel or lagoon flattens its shore to y = 0 for
    /// <see cref="ValleyFloor"/> m and lets the relief rise over <see cref="ValleySide"/> m (praias suaves).
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The coast shapes; false for the other ones (the caller then draws them).</summary>
        private static bool CoastShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Cliffs:
                {
                    float m = Mathf.Clamp01(0.5f + 0.9f * OverworldNoise.Fbm(x, z, 3));
                    float lower = SmoothStep(0.36f, 0.42f, m);
                    float upper = SmoothStep(0.6f, 0.65f, m);
                    float ripple = 0.03f * (0.5f + 0.5f * OverworldNoise.Signed(x * 2.7f + 1.9f, z * 2.7f + 6.1f));
                    smooth = a * (0.08f + 0.55f * SmoothStep(0.3f, 0.75f, m));
                    shaped = a * (0.06f + ripple + 0.5f * lower + 0.4f * upper);
                    return true;
                }

                case ReliefShape.Islets:
                {
                    float hump = SmoothStep(0.5f, 0.78f, OverworldNoise.Perlin(x * 2.2f + 3.7f, z * 2.2f + 8.9f));
                    float swell = 0.5f + 0.5f * OverworldNoise.Signed(x * 0.6f + 1.1f, z * 0.6f + 4.4f);
                    smooth = a * (0.2f + 0.15f * swell);
                    shaped = a * (0.15f + 0.2f * swell + 0.65f * hump);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
