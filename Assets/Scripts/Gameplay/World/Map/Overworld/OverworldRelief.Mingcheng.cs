using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 8 — Planície de Mingcheng: the plain's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and
    /// continuous like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.KarstTowers"/>: sheer limestone towers standing on a flat green floor; the trails and the
    /// clearings follow the floor (the smooth part), the towers rise round them;</item>
    /// <item><see cref="ReliefShape.WallRidge"/>: one long crest under the great wall (<see cref="OverworldLayouts.GreatWallZ"/>,
    /// in the region's own coordinates, so it runs on from area to area and survives the move into the stitched world),
    /// rounded on top where the wall stands, foothills below it.</item>
    /// </list>
    /// The plain round Mingcheng, the canal and the river reuse the rolling default; the paddies, the tea and the white hills
    /// <see cref="ReliefShape.Terraced"/>; the pine ridges <see cref="ReliefShape.Peaks"/>; the loess
    /// <see cref="ReliefShape.Gorges"/>; the terracotta pits <see cref="ReliefShape.Mounds"/> (the burial mounds).
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The Planície de Mingcheng shapes; false for the other ones (the caller then draws them).</summary>
        private static bool MingchengShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.KarstTowers:
                {
                    // A flat floor with a gentle swell; towers where the cell noise peaks, steep-sided, ragged tops.
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 2.3f, z * 0.7f + 6.1f, 2);
                    float n = OverworldNoise.Perlin(x * 2.4f + 4.4f, z * 2.4f + 1.9f);
                    float tower = SmoothStep(0.46f, 0.84f, n);
                    float top = 0.55f + 0.45f * OverworldNoise.Perlin(x * 7f + 1.1f, z * 7f + 8.8f);
                    smooth = a * 0.06f * floor;
                    shaped = a * (0.06f * floor + 0.94f * tower * top);
                    return true;
                }

                case ReliefShape.WallRidge:
                {
                    // Back to the region's metres: the crest follows the wall's line, whatever the area's own noise offsets.
                    float scale = style.ReliefScale;
                    float lx = (x - 13.7f * ((int)style.Terrain + 1)) * scale;
                    float lz = (z - 5.3f * ((int)style.Terrain + 1)) * scale;
                    float d = Mathf.Abs(lz - OverworldLayouts.GreatWallZ(lx));
                    float crest = 1f - SmoothStep(0f, OverworldLayouts.GreatWallRidgeHalfWidth, d);
                    float swell = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 3.7f, z * 0.8f + 0.4f, 2);
                    smooth = a * (0.12f + 0.12f * swell + 0.66f * crest);
                    shaped = a * (0.1f + 0.2f * swell + 0.7f * crest);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
