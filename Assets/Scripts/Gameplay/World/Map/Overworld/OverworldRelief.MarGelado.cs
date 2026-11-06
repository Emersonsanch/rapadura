using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 11 — Mar Gelado: the frozen sea's own relief shapes (Docs/Mapas100.md §0.10, "Relevo"), pure and continuous like
    /// the rest of <see cref="OverworldRelief"/>:
    /// <list type="bullet">
    /// <item><see cref="ReliefShape.Floes"/>: plates of sea ice on a jittered cell grid, each at its own height, sinking to the
    /// floor at the leads between them (the height goes to zero on the line where two plates meet, so the field stays
    /// continuous); the bay, the canals and the pack ice;</item>
    /// <item><see cref="ReliefShape.Icebergs"/>: angular bergs with flat tops (a rotated square per filled cell) standing out of a
    /// flat frozen sea; the trails and clearings keep to the sea (the smooth part);</item>
    /// <item><see cref="ReliefShape.PressureRidges"/>: long sharp ridges of crushed ice where the ridged noise peaks, over a
    /// floor with a faint swell;</item>
    /// <item><see cref="ReliefShape.FjordWalls"/>: a wide channel winding along the lines where a signed noise crosses zero,
    /// between high walls cut in three steps;</item>
    /// <item><see cref="ReliefShape.IceVaults"/>: round domes of ice and frozen earth, most cells filled, the lanes between them
    /// (the galleries) on the floor.</item>
    /// </list>
    /// The shore, the harbour, the tundra and the lake reuse the rolling default and <see cref="ReliefShape.Hills"/>. Towns,
    /// posts, gates and trails flatten every one of them like anywhere else.
    /// </summary>
    public sealed partial class OverworldRelief
    {
        /// <summary>The Mar Gelado shapes; false for the other ones (the caller then draws them).</summary>
        private static bool MarGeladoShape(OverworldTerrainStyle style, float x, float z, float a, out float smooth, out float shaped)
        {
            switch (style.Shape)
            {
                case ReliefShape.Floes:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.8f + 2.1f, z * 0.8f + 6.4f, 2);
                    float plate = FloePlate(x, z);
                    smooth = a * 0.15f * floor;
                    shaped = a * (0.15f * floor + 0.85f * plate);
                    return true;
                }

                case ReliefShape.Icebergs:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.7f + 7.3f, z * 0.7f + 1.9f, 2);
                    float berg = IcebergAt(x, z);
                    smooth = a * 0.04f * floor;
                    shaped = a * (0.04f * floor + 0.96f * berg);
                    return true;
                }

                case ReliefShape.PressureRidges:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.6f + 4.4f, z * 0.6f + 8.8f, 2);
                    float r = OverworldNoise.Ridged(x * 1.4f + 3.7f, z * 1.4f + 0.9f);
                    float ridge = SmoothStep(0.72f, 0.97f, r);
                    float rough = 0.85f + 0.15f * OverworldNoise.Signed(x * 6f + 1.1f, z * 6f + 2.3f);
                    smooth = a * 0.1f * floor;
                    shaped = a * (0.1f * floor + 0.9f * ridge * rough);
                    return true;
                }

                case ReliefShape.FjordWalls:
                {
                    // A slow warp makes the channel wind; it is wide (the |signed| band up to 0.2) and the walls step up in three.
                    float wx = x + 0.35f * OverworldNoise.Signed(z * 0.5f + 5.5f, x * 0.5f + 2.5f);
                    float v = Mathf.Abs(OverworldNoise.Signed(wx * 0.55f + 9.1f, z * 0.55f + 3.3f));
                    float wall = SmoothStep(0.2f, 0.42f, v);
                    float stepped = Terrace(wall, 3, 0.4f);
                    float rim = 0.8f + 0.2f * OverworldNoise.Perlin(x * 2.4f + 6.2f, z * 2.4f + 4.1f);
                    smooth = a * 0.04f;
                    shaped = a * (0.04f + 0.96f * stepped * rim);
                    return true;
                }

                case ReliefShape.IceVaults:
                {
                    float floor = 0.5f + 0.5f * OverworldNoise.Fbm(x * 0.9f + 5.6f, z * 0.9f + 3.2f, 2);
                    float dome = VaultDome(x, z);
                    smooth = a * 0.05f * floor;
                    shaped = a * (0.05f * floor + 0.95f * dome);
                    return true;
                }

                default:
                    smooth = shaped = 0f;
                    return false;
            }
        }

        /// <summary>
        /// The floe at (<paramref name="x"/>, <paramref name="z"/>) in relief units: its own height (0.3–1) on the plate, falling to
        /// zero on the lead where it meets the next one (F2 − F1 of a jittered grid: zero exactly on the bisector, so the field is
        /// continuous whichever plate is the nearest).
        /// </summary>
        private static float FloePlate(float x, float z)
        {
            const float cell = 0.42f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float f1 = float.MaxValue;
            float f2 = float.MaxValue;
            uint nearest = 0;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = IceHash(i, j, 0x2C1B3C6Du);
                    float px = (i + 0.15f + 0.7f * (((h >> 8) & 0xFF) / 255f)) * cell;
                    float pz = (j + 0.15f + 0.7f * (((h >> 16) & 0xFF) / 255f)) * cell;
                    float dx = x - px;
                    float dz = z - pz;
                    float d = Mathf.Sqrt(dx * dx + dz * dz);
                    if (d < f1)
                    {
                        f2 = f1;
                        f1 = d;
                        nearest = h;
                    }
                    else if (d < f2)
                    {
                        f2 = d;
                    }
                }
            }

            float plate = 0.3f + 0.7f * ((nearest >> 24) & 0xFF) / 255f;
            float lead = SmoothStep(0.012f, 0.05f, f2 - f1);
            return plate * lead;
        }

        /// <summary>
        /// The icebergs at (<paramref name="x"/>, <paramref name="z"/>) in relief units: 0 on the sea, up to 1 on the flat top of the
        /// tallest; a rotated square per filled cell (a steep flank, a flat top), the max of them: continuous.
        /// </summary>
        private static float IcebergAt(float x, float z)
        {
            const float cell = 0.5f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float best = 0f;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = IceHash(i, j, 0x68E31DA4u);
                    if ((h & 0xFF) < 120)
                    {
                        continue; // open sea
                    }

                    float size = 0.55f + 0.45f * ((h >> 24) & 0xFF) / 255f;
                    float angle = ((h >> 4) & 0xF) / 16f * Mathf.PI * 0.5f;
                    float dx = x - (i + 0.3f + 0.4f * (((h >> 8) & 0xFF) / 255f)) * cell;
                    float dz = z - (j + 0.3f + 0.4f * (((h >> 16) & 0xFF) / 255f)) * cell;
                    float c = Mathf.Cos(angle);
                    float s = Mathf.Sin(angle);
                    float u = Mathf.Abs(dx * c - dz * s);
                    float w = Mathf.Abs(dx * s + dz * c) * 1.3f;
                    float r = 0.12f * (0.7f + 0.3f * size);
                    float d = Mathf.Max(u, w) / r;
                    if (d < 1f)
                    {
                        best = Mathf.Max(best, size * SmoothStep(1f, 0.72f, d));
                    }
                }
            }

            return best;
        }

        /// <summary>The ice vaults at (<paramref name="x"/>, <paramref name="z"/>): round domes (0 at the foot, 1 on top), most cells filled.</summary>
        private static float VaultDome(float x, float z)
        {
            const float cell = 0.55f;
            int cx = Mathf.FloorToInt(x / cell);
            int cz = Mathf.FloorToInt(z / cell);
            float best = 0f;
            for (int i = cx - 1; i <= cx + 1; i++)
            {
                for (int j = cz - 1; j <= cz + 1; j++)
                {
                    uint h = IceHash(i, j, 0x1B873593u);
                    if ((h & 0xFF) < 50)
                    {
                        continue;
                    }

                    float size = 0.5f + 0.5f * ((h >> 24) & 0xFF) / 255f;
                    float dx = x - (i + 0.3f + 0.4f * (((h >> 8) & 0xFF) / 255f)) * cell;
                    float dz = z - (j + 0.3f + 0.4f * (((h >> 16) & 0xFF) / 255f)) * cell;
                    float r = 0.2f * (0.75f + 0.25f * size);
                    float d = Mathf.Sqrt(dx * dx + dz * dz) / r;
                    if (d < 1f)
                    {
                        // A steep foot, a rounded cap.
                        float dome = SmoothStep(1f, 0.55f, d) * (0.8f + 0.2f * (1f - d * d));
                        best = Mathf.Max(best, size * dome);
                    }
                }
            }

            return best;
        }

        private static uint IceHash(int i, int j, uint seed)
        {
            unchecked
            {
                uint h = (uint)i * 0x9E3779B1u ^ (uint)j * 0x85EBCA77u ^ seed;
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
