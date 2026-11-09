using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 12 — Arquipélago de Hinokura: the volcanic islands' own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and
    /// continuous like the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.VolcanoCone"/>: Mount Echeyde's cone round <see cref="OverworldLayouts.EcheydeCenter"/>, rising in
    /// three rings to a crater rim with a dip inside (the smooth part is the cone itself: the trails climb it);</item>
    /// <item><see cref="ReliefShape.LavaFlows"/>: rough lobes and ropy ridges of black lava (the malpaís) over a low floor;</item>
    /// <item><see cref="ReliefShape.GlassReef"/>: long sharp shards of obsidian (a rotated, elongated diamond per filled cell, the max
    /// of them: continuous) standing out of a low floor;</item>
    /// <item><see cref="ReliefShape.CurvedRamparts"/>: curved walls in rings round <see cref="OverworldLayouts.CastleCenter"/>, a gap
    /// in each ring where an angular wave dips (the castle's curved walls, Shuri's form);</item>
    /// <item><see cref="ReliefShape.CalderaRim"/>: the caldera's high ring of cold stone round <see cref="OverworldLayouts.CalderaCenter"/>,
    /// a low floor inside (the lava lake) and a long outer slope.</item>
    /// </list>
    /// The shapes with a fixed centre read the region's own coordinates back from the relief units (the stitched world moves the
    /// region by a whole offset, the zones keep their origin), so the volcano, the castle and the caldera stay where the layout puts
    /// them. Towns, posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>Radius (m) of Mount Echeyde's cone and of its crater.</summary>
        public const float EcheydeRadius = 250f;

        public const float EcheydeCraterRadius = 46f;

        /// <summary>Radius (m) of the caldera's rim (its crest).</summary>
        public const float CalderaRimRadius = 215f;

        /// <summary>The Hinokura shapes; false for the other ones (the caller then draws them).</summary>
        private static bool HinokuraShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.VolcanoCone:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = Vector2.Distance(p, OverworldLayouts.EcheydeCenter);
                    float u = Mathf.Clamp01(1f - r / EcheydeRadius);
                    float cone = u * u * (3f - 2f * u);
                    // Three rings (terraces) on the flanks, the rim a little higher, the crater a bowl inside it.
                    float ringed = Terrace(cone, 3, 0.45f);
                    float crater = SmoothStep(EcheydeCraterRadius, EcheydeCraterRadius * 0.35f, r);
                    float rough = 0.94f + 0.06f * OverworldNoise.Perlin(x * 3.1f + 1.7f, z * 3.1f + 4.9f);
                    smooth = a * cone * (1f - 0.35f * crater);
                    shaped = a * (ringed * rough * (1f - 0.45f * crater));
                    return true;
                }

                case ReliefShape.LavaFlows:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 3.3f, z * 0.7f + 9.1f, 2);
                    float wx = x + 0.3f * OverworldNoise.Signed(z * 0.6f + 2.2f, x * 0.6f + 7.7f);
                    float lobe = SmoothStep(0.6f, 0.93f, OverworldNoise.Ridged(wx * 1.2f + 5.1f, z * 1.2f + 0.4f));
                    float ropes = 0.8f + 0.2f * Mathf.Abs(OverworldNoise.Signed(x * 7f + 3.9f, z * 7f + 1.3f));
                    smooth = a * 0.08f * floor;
                    shaped = a * (0.08f * floor + 0.92f * lobe * ropes);
                    return true;
                }

                case ReliefShape.GlassReef:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 6.6f, z * 0.8f + 2.7f, 2);
                    float shard = GlassShardAt(x, z);
                    smooth = a * 0.04f * floor;
                    shaped = a * (0.04f * floor + 0.96f * shard);
                    return true;
                }

                case ReliefShape.CurvedRamparts:
                {
                    Vector2 p = LocalOf(style, x, z);
                    Vector2 d = p - OverworldLayouts.CastleCenter;
                    float r = d.magnitude;
                    const float period = 80f;
                    float ring = r / period;
                    int index = Mathf.FloorToInt(ring);
                    float v = ring - index;
                    float wall = SmoothStep(0.2f, 0.06f, Mathf.Abs(v - 0.5f));
                    // A gap in every ring, turned a little from ring to ring (the walls are walked round, Shuri's form).
                    float angle = Mathf.Atan2(d.y, d.x) + index * 1.3f;
                    float gap = SmoothStep(0.8f, 0.93f, Mathf.Cos(angle * 2f));
                    float inner = SmoothStep(70f, 110f, r);
                    float rim = 0.85f + 0.15f * OverworldNoise.Perlin(x * 4f + 2.6f, z * 4f + 8.2f);
                    smooth = a * 0.03f;
                    shaped = a * (0.03f + 0.97f * wall * (1f - gap) * inner * rim);
                    return true;
                }

                case ReliefShape.CalderaRim:
                {
                    Vector2 p = LocalOf(style, x, z);
                    float r = Vector2.Distance(p, OverworldLayouts.CalderaCenter);
                    float inside = SmoothStep(CalderaRimRadius - 70f, CalderaRimRadius, r);
                    float outside = SmoothStep(CalderaRimRadius + 150f, CalderaRimRadius, r);
                    float ring = Mathf.Min(inside, outside);
                    float rough = 0.8f + 0.2f * OverworldNoise.Perlin(x * 3.6f + 5.2f, z * 3.6f + 0.6f);
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.9f + 1.4f, z * 0.9f + 3.8f, 2);
                    smooth = a * 0.04f * floor;
                    shaped = a * (0.04f * floor + 0.96f * Terrace(ring, 2, 0.5f) * rough);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }

        /// <summary>The region-local point (m) of relief units (<paramref name="x"/>, <paramref name="z"/>) of <paramref name="style"/> (the inverse of <see cref="Shape"/>'s scaling).</summary>
        private static Vector2 LocalOf(OverworldTerrainStyle style, float x, float z)
        {
            float ox = 13.7f * ((int)style.Terrain + 1);
            float oz = 5.3f * ((int)style.Terrain + 1);
            return new Vector2((x - ox) * style.ReliefScale, (z - oz) * style.ReliefScale);
        }

        /// <summary>
        /// The obsidian shards at (<paramref name="x"/>, <paramref name="z"/>) in relief units: 0 on the floor, up to 1 on the crest of
        /// the tallest; a rotated elongated diamond per filled cell (a sharp crest along it), the max of them: continuous.
        /// </summary>
        private static float GlassShardAt(float x, float z)
        {
            const float cell = 0.45f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float best = 0f;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = IceHash(i, j, 0x5BD1E995u);
                    if ((h & 0xFF) < 100)
                    {
                        continue; // open floor
                    }

                    float size = 0.5f + 0.5f * ((h >> 24) & 0xFF) / 255f;
                    float angle = ((h >> 4) & 0xF) / 16f * Mathf.PI;
                    float dx = x - (i + 0.25f + 0.5f * (((h >> 8) & 0xFF) / 255f)) * cell;
                    float dz = z - (j + 0.25f + 0.5f * (((h >> 16) & 0xFF) / 255f)) * cell;
                    float c = Mathf.Cos(angle);
                    float s = Mathf.Sin(angle);
                    float along = Mathf.Abs(dx * c - dz * s) / (0.2f * (0.7f + 0.3f * size));
                    float across = Mathf.Abs(dx * s + dz * c) / 0.07f;
                    float d = along + across;
                    if (d < 1f)
                    {
                        best = Mathf.Max(best, size * SmoothStep(1f, 0.25f, d));
                    }
                }
            }

            return best;
        }
    }
}
