using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 14 — Margens da Mistura: the relief shapes of its own (Docs/Mapas100.md §0.10, maps 77–82), pure and continuous like the
    /// rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.GnomonDial"/>: a flat dial of light sand with twelve low radial ridges round
    /// <see cref="OverworldLayouts.GnomonDialCenter"/> (the Desert of the Clock), the gnomons lying between them;</item>
    /// <item><see cref="ReliefShape.CedarRings"/>: concentric rings of root ridges round <see cref="OverworldLayouts.CedarRingCenter"/>
    /// (Hobabish's rings of cedars), low in the middle for the woodcutter's clearing;</item>
    /// <item><see cref="ReliefShape.GiantShoulder"/>: one broad hump along the column of the giant's shoulder (a long spine with a slow
    /// waver, moss-grown bumps on it);</item>
    /// <item><see cref="ReliefShape.SkinFolds"/>: parallel sharp folds of the giant's stone skin;</item>
    /// <item><see cref="ReliefShape.DioriteRise"/>: a rough cone of diorite round <see cref="OverworldLayouts.DioriteCenter"/>
    /// (the Ullikummi that keeps growing).</item>
    /// </list>
    /// The shapes with a fixed centre read the region's own coordinates back from the relief units (<see cref="LocalOf"/>). Towns, posts,
    /// gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Distance (m) between Hobabish's rings of roots.</summary>
        public const float CedarRingPeriod = 72f;

        /// <summary>The Mistura shapes; false for the other ones (the caller then draws them).</summary>
        private static bool MisturaShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.GnomonDial:
                {
                    Vector2 p = LocalOf(style, x, z);
                    Vector2 d = p - OverworldLayouts.GnomonDialCenter;
                    float r = d.magnitude;
                    float spoke = Mathf.Abs(Mathf.Repeat(Mathf.Atan2(d.y, d.x) / (Mathf.PI * 2f) * 12f, 1f) - 0.5f) * 2f; // 1 on the twelve spokes
                    float band = SmoothStep(90f, 130f, r) * SmoothStep(340f, 300f, r);
                    float ridge = SmoothStep(0.88f, 0.985f, spoke) * band;
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 2.1f, z * 0.7f + 8.3f, 2);
                    smooth = a * 0.08f * floor;
                    shaped = a * (0.08f * floor + 0.92f * ridge);
                    return true;
                }

                case ReliefShape.CedarRings:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.CedarRingCenter).magnitude;
                    float ring = r / CedarRingPeriod;
                    int index = Mathf.FloorToInt(ring);
                    float v = ring - index;
                    float wall = SmoothStep(0.34f, 0.12f, Mathf.Abs(v - 0.5f));
                    float inner = SmoothStep(46f, 90f, r);
                    float root = 0.8f + 0.2f * OverworldNoise.Perlin(x * 5.1f + 3.3f, z * 5.1f + 1.7f);
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.9f + 6.2f, z * 0.9f + 4.1f, 2);
                    smooth = a * 0.12f * floor;
                    shaped = a * (0.12f * floor + 0.88f * wall * inner * root);
                    return true;
                }

                case ReliefShape.GiantShoulder:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float spine = 1080f + 70f * Mathf.Sin(p.y * 0.0042f + 0.6f);
                    float hump = SmoothStep(360f, 70f, Mathf.Abs(p.x - spine));
                    float moss = 0.82f + 0.18f * OverworldNoise.Perlin(x * 3.7f + 4.9f, z * 3.7f + 2.2f);
                    smooth = a * 0.1f;
                    shaped = a * (0.1f + 0.9f * hump * moss);
                    return true;
                }

                case ReliefShape.SkinFolds:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float wave = Mathf.Abs(Mathf.Sin(p.x * 0.055f + p.y * 0.012f + 0.8f * OverworldNoise.Perlin(x * 0.6f + 1.4f, z * 0.6f + 7.7f)));
                    float crest = wave * wave * wave;
                    smooth = a * 0.15f;
                    shaped = a * (0.15f + 0.85f * crest);
                    return true;
                }

                case ReliefShape.DioriteRise:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = (p - OverworldLayouts.DioriteCenter).magnitude;
                    float cone = 1f - Mathf.Clamp01(r / 420f);
                    float rough = 0.72f + 0.28f * OverworldNoise.Perlin(x * 2.3f + 5.5f, z * 2.3f + 9.1f);
                    smooth = a * 0.08f;
                    shaped = a * (0.08f + 0.92f * cone * Mathf.Sqrt(cone) * rough);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
