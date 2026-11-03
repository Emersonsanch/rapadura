using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Managed 2D gradient noise for the overworld (relief, ground colours, borders): the same numbers on every platform
    /// and on a server without the engine (no <see cref="Mathf.PerlinNoise"/> call, which is native). Pure, allocation-free.
    /// </summary>
    public static class OverworldNoise
    {
        private static readonly int[] Perm = BuildPermutation();

        /// <summary>Gradient noise in 0..1 (about 0.5 on average, like <see cref="Mathf.PerlinNoise"/>).</summary>
        public static float Perlin(float x, float y) => Signed(x, y) * 0.5f + 0.5f;

        /// <summary>Gradient noise in about −1..1.</summary>
        public static float Signed(float x, float y)
        {
            int xi = FastFloor(x);
            int yi = FastFloor(y);
            float xf = x - xi;
            float yf = y - yi;
            int x0 = xi & 255;
            int y0 = yi & 255;
            int x1 = (x0 + 1) & 255;
            int y1 = (y0 + 1) & 255;
            float u = Fade(xf);
            float v = Fade(yf);
            float n00 = Grad(Perm[Perm[x0] + y0], xf, yf);
            float n10 = Grad(Perm[Perm[x1] + y0], xf - 1f, yf);
            float n01 = Grad(Perm[Perm[x0] + y1], xf, yf - 1f);
            float n11 = Grad(Perm[Perm[x1] + y1], xf - 1f, yf - 1f);
            float a = n00 + (n10 - n00) * u;
            float b = n01 + (n11 - n01) * u;
            float n = (a + (b - a) * v) * 1.4142f;
            return n < -1f ? -1f : n > 1f ? 1f : n;
        }

        /// <summary>Fractal sum of <paramref name="octaves"/> octaves (lacunarity 2, gain 0.5), about −1..1.</summary>
        public static float Fbm(float x, float y, int octaves)
        {
            float sum = 0f;
            float amp = 1f;
            float norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Signed(x, y) * amp;
                norm += amp;
                amp *= 0.5f;
                x = x * 2.03f + 17.1f;
                y = y * 2.03f + 9.7f;
            }

            return sum / norm;
        }

        /// <summary>Ridged noise in 0..1: sharp crests where the gradient noise crosses zero.</summary>
        public static float Ridged(float x, float y)
        {
            float n = 1f - Mathf.Abs(Signed(x, y));
            return n * n;
        }

        private static int FastFloor(float v)
        {
            int i = (int)v;
            return v < i ? i - 1 : i;
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float Grad(int hash, float x, float y)
        {
            switch (hash & 7)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x;
                case 5: return -x;
                case 6: return y;
                default: return -y;
            }
        }

        private static int[] BuildPermutation()
        {
            var p = new int[256];
            for (int i = 0; i < 256; i++)
            {
                p[i] = i;
            }

            // Fixed shuffle (xorshift with a constant seed): the same table everywhere.
            uint s = 0x9E3779B9u;
            for (int i = 255; i > 0; i--)
            {
                s ^= s << 13;
                s ^= s >> 17;
                s ^= s << 5;
                int j = (int)(s % (uint)(i + 1));
                (p[i], p[j]) = (p[j], p[i]);
            }

            var perm = new int[512];
            for (int i = 0; i < 512; i++)
            {
                perm[i] = p[i & 255];
            }

            return perm;
        }
    }
}
