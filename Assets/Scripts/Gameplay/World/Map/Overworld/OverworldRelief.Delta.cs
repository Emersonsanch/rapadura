using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 6 — Delta de Jhilpur: the delta's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous
    /// like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Hummocks"/>: round islands of tussock and root, close together, over a flat mud floor a
    /// little above the water (the reed swamp, the bog, the black mire);</item>
    /// <item><see cref="ReliefShape.Levees"/>: a flat floor crossed by long low dikes that bend gently (the paddies), the
    /// trails running on the mean so they never climb every dike.</item>
    /// </list>
    /// The gorges of the Pontes de Raiz Viva reuse <see cref="ReliefShape.Gorges"/>; the carved channels flatten their banks
    /// to y = 0 like every river (<see cref="ValleyFloor"/>, <see cref="ValleySide"/>).
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The delta shapes; false for the other ones (the caller then draws them).</summary>
        private static bool DeltaShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Hummocks:
                {
                    float hump = SmoothStep(0.46f, 0.72f, OverworldNoise.Perlin(x * 3.1f + 5.3f, z * 3.1f + 1.9f));
                    float swell = 0.5f + 0.5f * OverworldNoise.Signed(x * 0.5f + 2.2f, z * 0.5f + 7.7f);
                    smooth = a * (0.12f + 0.18f * swell);
                    shaped = a * (0.08f + 0.17f * swell + 0.75f * hump);
                    return true;
                }

                case ReliefShape.Levees:
                {
                    // Dikes about north–south, bent by noise; each a narrow rounded ridge, the paddies flat between them.
                    float u = x * 1.6f + 0.35f * OverworldNoise.Signed(z * 0.45f + 1.3f, x * 0.4f + 2.1f);
                    float f = Mathf.Abs(u - Mathf.Floor(u) - 0.5f);
                    float ridge = 1f - SmoothStep(0.02f, 0.11f, f);
                    float field = 0.5f + 0.5f * OverworldNoise.Signed(x * 0.6f + 9.1f, z * 0.6f + 3.4f);
                    smooth = a * (0.25f + 0.1f * field);
                    shaped = a * (0.2f + 0.1f * field + 0.7f * ridge);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
