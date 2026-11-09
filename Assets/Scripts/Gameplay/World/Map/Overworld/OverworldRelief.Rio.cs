using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 13 — Rio das Doze Horas: the river's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous like the rest
    /// of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.ReedBedGrid"/>: raised rectangular beds with furrows between them (the canteiros of the Campos de Juncos);</item>
    /// <item><see cref="ReliefShape.ZigzagDunes"/>: diagonal zigzag ridges of dark sand with lanes between them (the 4ª hora's zigzag path);</item>
    /// <item><see cref="ReliefShape.CoilRings"/>: concentric rings of scales round <see cref="OverworldLayouts.MehenCenter"/>, a gap in each ring
    /// that turns from ring to ring (Mehen's labyrinth), low in the middle for the Anel Quieto;</item>
    /// <item><see cref="ReliefShape.ApepBody"/>: Apep's body as one long sinuous ridge across the north of the region, segmented like scales,
    /// over a low sandbank.</item>
    /// </list>
    /// The shapes with a fixed centre read the region's own coordinates back from the relief units (<see cref="LocalOf"/>).
    /// Towns, posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Distance (m) between Mehen's rings.</summary>
        public const float MehenRingPeriod = 96f;

        /// <summary>The Rio shapes; false for the other ones (the caller then draws them).</summary>
        private static bool RioShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.ReedBedGrid:
                {
                    const float cells = 5f;
                    float fx = Mathf.Repeat(x * cells, 1f);
                    float fz = Mathf.Repeat(z * cells, 1f);
                    float edge = Mathf.Min(Mathf.Min(fx, 1f - fx), Mathf.Min(fz, 1f - fz));
                    float bed = SmoothStep(0.03f, 0.14f, edge);
                    float tuft = 0.9f + 0.1f * OverworldNoise.Perlin(x * 6f + 2.2f, z * 6f + 7.1f);
                    smooth = a * 0.4f;
                    shaped = a * (0.4f + 0.6f * bed * tuft);
                    return true;
                }

                case ReliefShape.ZigzagDunes:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 4.4f, z * 0.8f + 1.9f, 2);
                    float zig = Mathf.Abs(Mathf.Repeat(x * 1.1f, 2f) - 1f); // 0…1 triangular wave along x
                    float wave = Mathf.Repeat(z * 1.6f + zig * 0.9f, 1f);
                    float crest = 1f - Mathf.Abs(wave - 0.5f) * 2f; // 0 in the lanes, 1 on the ridge
                    float ridge = SmoothStep(0.65f, 0.95f, crest);
                    smooth = a * 0.1f * floor;
                    shaped = a * (0.1f * floor + 0.9f * ridge);
                    return true;
                }

                case ReliefShape.CoilRings:
                {
                    Vector2 p = LocalOf(style, x, z);
                    Vector2 d = p - OverworldLayouts.MehenCenter;
                    float r = d.magnitude;
                    float ring = r / MehenRingPeriod;
                    int index = Mathf.FloorToInt(ring);
                    float v = ring - index;
                    float wall = SmoothStep(0.26f, 0.1f, Mathf.Abs(v - 0.5f));
                    float angle = Mathf.Atan2(d.y, d.x) + index * 1.15f;
                    float gap = SmoothStep(0.72f, 0.9f, Mathf.Cos(angle * 2f));
                    float inner = SmoothStep(55f, 95f, r);
                    float rim = 0.85f + 0.15f * OverworldNoise.Perlin(x * 4.4f + 1.1f, z * 4.4f + 6.3f);
                    smooth = a * 0.03f;
                    shaped = a * (0.03f + 0.97f * wall * (1f - gap) * inner * rim);
                    return true;
                }

                case ReliefShape.ApepBody:
                {
                    Vector2 p = LocalOf(style, x, z);
                    // Apep's spine: one long sine across the region; a second loop of it further north.
                    float spine = 1560f + 120f * Mathf.Sin(p.x * 0.0095f + 0.4f);
                    float d = Mathf.Min(Mathf.Abs(p.y - spine), Mathf.Abs(p.y - spine - 170f));
                    float ridge = SmoothStep(30f, 8f, d);
                    float scales = 0.82f + 0.18f * Mathf.Abs(Mathf.Sin(p.x * 0.16f));
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 2.8f, z * 0.8f + 5.5f, 2);
                    smooth = a * 0.05f * floor;
                    shaped = a * (0.05f * floor + 0.95f * ridge * scales);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }
    }
}
