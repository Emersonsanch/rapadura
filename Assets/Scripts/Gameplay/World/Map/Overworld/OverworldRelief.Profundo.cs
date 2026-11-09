using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 15 — Limiar Profundo: the relief shapes of the five pits and the bridge (Docs/Mapas100.md §0.10, maps 83–90), pure and continuous:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.SmokeCorridors"/>: long parallel walls of soot (the corridors of the Pit of Smoke);</item>
    /// <item><see cref="ReliefShape.EmberCells"/>: a floor of square slabs with raised seams (the Pit of Devouring Fire, the floor that lights);</item>
    /// <item><see cref="ReliefShape.ShaftTerraces"/>: rings of steps rising outwards round <see cref="OverworldLayouts.WindShaftCenter"/> (the shaft of the wind);</item>
    /// <item><see cref="ReliefShape.DarkLabyrinth"/>: a tangle of winding walls drawn on the zero lines of a noise (the Pit of Darkness);</item>
    /// <item><see cref="ReliefShape.NoveloCauseway"/>: one long raised causeway along <see cref="OverworldLayouts.NoveloBridgeX"/> over low black ground
    /// (the Bridge of the Bōlos, the flanks gentle enough to walk).</item>
    /// </list>
    /// Towns, posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Distance (m) between the raised seams of the floor of the Pit of Devouring Fire.</summary>
        public const float EmberCellPeriod = 44f;

        /// <summary>Radius (m) over which the shaft of the wind climbs from its bottom to its rim.</summary>
        public const float WindShaftReach = 560f;

        /// <summary>Half the width (m) of the flat crest of the Bridge of the Bōlos, and where its flank meets the black ground.</summary>
        public const float CausewayCrest = 9f;
        public const float CausewayFoot = 46f;

        /// <summary>The Profundo shapes; false for the other ones (the caller then draws them).</summary>
        private static bool ProfundoShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.SmokeCorridors:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float wave = Mathf.Abs(Mathf.Sin(p.x * 0.04f + 0.018f * p.y + 0.9f * OverworldNoise.Perlin(x * 0.7f + 2.4f, z * 0.7f + 6.1f)));
                    float wall = SmoothStep(0.72f, 0.97f, wave);
                    smooth = a * 0.1f;
                    shaped = a * (0.1f + 0.9f * wall);
                    return true;
                }

                case ReliefShape.EmberCells:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float u = Mathf.Abs(Mathf.Repeat(p.x / EmberCellPeriod, 1f) - 0.5f) * 2f;
                    float v = Mathf.Abs(Mathf.Repeat(p.y / EmberCellPeriod, 1f) - 0.5f) * 2f;
                    float seam = SmoothStep(0.78f, 0.98f, Mathf.Max(u, v));
                    smooth = a * 0.12f;
                    shaped = a * (0.12f + 0.88f * seam);
                    return true;
                }

                case ReliefShape.ShaftTerraces:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.WindShaftCenter).magnitude;
                    float u = Mathf.Clamp01(r / WindShaftReach);
                    smooth = a * u;
                    shaped = a * Terrace(u, 5, 0.5f);
                    return true;
                }

                case ReliefShape.DarkLabyrinth:
                {
                    float n = Mathf.Abs(OverworldNoise.Perlin(x * 2.6f + 1.1f, z * 2.6f + 3.3f));
                    float wall = SmoothStep(0.13f, 0.03f, n);
                    smooth = a * 0.08f;
                    shaped = a * (0.08f + 0.92f * wall);
                    return true;
                }

                case ReliefShape.NoveloCauseway:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float crest = SmoothStep(CausewayFoot, CausewayCrest, Mathf.Abs(p.x - OverworldLayouts.NoveloBridgeX));
                    crest *= SmoothStep(22f, 80f, (p - OverworldLayouts.PilarDoMeioOffset).magnitude); // down to the flat ground of the post on the middle pillar
                    smooth = a * (0.04f + 0.96f * crest); // Trail centre lines follow the smooth part: the road must ride the crest.
                    shaped = a * (0.04f + 0.96f * crest);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
