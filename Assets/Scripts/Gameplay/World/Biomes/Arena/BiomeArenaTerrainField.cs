using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Deterministic height + colour field of one biome arena terrain (local space, arena centre at
    /// the origin, y = 0 is the combat floor). Pure maths, no scene access, so EditMode tests can
    /// sample it. Inside <see cref="BiomeArenaTerrainGenerator.ZeroHeightRadius"/> the height is
    /// exactly 0 (the fight happens there); beyond it a low "walkable band" of bumps/dunes runs to
    /// the flee radius, then a rim rises into the biome's backdrop.
    /// </summary>
    public sealed class BiomeArenaTerrainField
    {
        /// <summary>Width over which the walkable band's bumps fade in past the flat circle.</summary>
        public const float BandFadeWidth = 6f;

        /// <summary>The rim (raised edge) starts here: just past the flee radius, so it never blocks a walk-out.</summary>
        public const float RimStart = 31f;

        public const float RimEnd = 44f;

        private readonly float _bandAmplitude;
        private readonly float _bandFrequency;
        private readonly float _rimHeight;
        private readonly float _rimNoise;
        private readonly float _rimFrequency;
        private readonly bool _ridgedRim;
        private readonly float _duneAmplitude;
        private readonly float _duneAngle;

        private readonly Vector2 _o1;
        private readonly Vector2 _o2;
        private readonly Vector2 _o3;
        private readonly Vector2 _o4;

        public BiomeType Biome { get; }
        public int Seed { get; }

        /// <summary>Cracked-earth / fissure patch centre (XZ, local) — inside the flat combat circle.</summary>
        public Vector2 PatchCenter { get; }

        /// <summary>Radius of <see cref="PatchCenter"/>'s patch (0 = none for this biome).</summary>
        public float PatchRadius { get; }

        public BiomeArenaTerrainField(BiomeType biome, int seed)
        {
            Biome = biome;
            Seed = seed;
            var random = new System.Random(unchecked(seed * 7919 + ((int)biome + 1) * 104729));
            _o1 = NextOffset(random);
            _o2 = NextOffset(random);
            _o3 = NextOffset(random);
            _o4 = NextOffset(random);
            _duneAngle = (float)random.NextDouble() * Mathf.PI;

            float patchAngle = (float)random.NextDouble() * Mathf.PI * 2f;
            float patchDistance = Mathf.Lerp(7f, 12f, (float)random.NextDouble());
            PatchCenter = new Vector2(Mathf.Cos(patchAngle), Mathf.Sin(patchAngle)) * patchDistance;

            switch (biome)
            {
                case BiomeType.Desert:
                    _bandAmplitude = 0.35f;
                    _bandFrequency = 0.06f;
                    _duneAmplitude = 1.2f;
                    _rimHeight = 1.5f;
                    _rimNoise = 2f;
                    _rimFrequency = 0.05f;
                    PatchRadius = 4.5f;
                    break;
                case BiomeType.Mountain:
                    _bandAmplitude = 0.7f;
                    _bandFrequency = 0.11f;
                    _rimHeight = 5f;
                    _rimNoise = 4f;
                    _rimFrequency = 0.09f;
                    _ridgedRim = true;
                    break;
                case BiomeType.Caves:
                    _bandAmplitude = 0.4f;
                    _bandFrequency = 0.14f;
                    _rimHeight = 1.5f;
                    _rimNoise = 1f;
                    _rimFrequency = 0.12f;
                    _ridgedRim = true;
                    PatchRadius = 3.5f;
                    break;
                default:
                    _bandAmplitude = 0.5f;
                    _bandFrequency = 0.08f;
                    _rimHeight = 2f;
                    _rimNoise = 1.2f;
                    _rimFrequency = 0.07f;
                    break;
            }
        }

        private static Vector2 NextOffset(System.Random random)
        {
            return new Vector2((float)random.NextDouble() * 1000f + 100f, (float)random.NextDouble() * 1000f + 100f);
        }

        /// <summary>Terrain height (local y) at local XZ. Exactly 0 inside the flat circle.</summary>
        public float Height(float x, float z)
        {
            float r = Mathf.Sqrt(x * x + z * z);
            float zero = BiomeArenaTerrainGenerator.ZeroHeightRadius;
            if (r <= zero)
            {
                return 0f;
            }

            float band = Smooth01((r - zero) / BandFadeWidth);
            float rim = Smooth01((r - RimStart) / (RimEnd - RimStart));

            float h = band * _bandAmplitude * Fbm(x * _bandFrequency + _o1.x, z * _bandFrequency + _o1.y);
            if (_duneAmplitude > 0f)
            {
                h += (band * 0.5f + rim * 2.5f) * _duneAmplitude * Dune(x, z);
            }

            if (rim > 0f)
            {
                float noise = _ridgedRim
                    ? Ridged(x * _rimFrequency + _o2.x, z * _rimFrequency + _o2.y)
                    : Fbm(x * _rimFrequency + _o2.x, z * _rimFrequency + _o2.y);
                h += rim * (_rimHeight + _rimNoise * noise);
            }

            return h;
        }

        /// <summary>Up component of the surface normal (1 = flat), by central differences.</summary>
        public float NormalY(float x, float z)
        {
            const float e = 0.75f;
            float dx = (Height(x + e, z) - Height(x - e, z)) / (2f * e);
            float dz = (Height(x, z + e) - Height(x, z - e)) / (2f * e);
            return 1f / Mathf.Sqrt(1f + dx * dx + dz * dz);
        }

        /// <summary>Raised-edge weight (0 in the walkable area, 1 at <see cref="RimEnd"/>).</summary>
        public static float RimWeight(float r) => Smooth01((r - RimStart) / (RimEnd - RimStart));

        /// <summary>Ground vertex colour at local XZ (palette harmonised with the biome's BiomeArenaProfile ground tint / fog).</summary>
        public Color GroundColor(float x, float z)
        {
            float r = Mathf.Sqrt(x * x + z * z);
            float rim = RimWeight(r);
            float ny = NormalY(x, z);
            float variation = Mathf.PerlinNoise(x * 0.09f + _o3.x, z * 0.09f + _o3.y);
            float patches = Mathf.PerlinNoise(x * 0.055f + _o4.x, z * 0.055f + _o4.y);
            float jitter = (Hash(x, z) - 0.5f) * 0.06f;
            float steep = Smooth01((0.93f - ny) / 0.2f);
            Color c;

            switch (Biome)
            {
                case BiomeType.Desert:
                {
                    var sand = new Color(0.93f, 0.80f, 0.58f);
                    var sandShade = new Color(0.85f, 0.69f, 0.47f);
                    var clay = new Color(0.76f, 0.56f, 0.38f);
                    var rimSand = new Color(0.90f, 0.71f, 0.49f);
                    c = Color.Lerp(sand, sandShade, variation * 0.6f);
                    if (_duneAmplitude > 0f)
                    {
                        c = Color.Lerp(c, sandShade, (1f - Dune(x, z)) * Mathf.Max(rim, 0.25f * Smooth01((r - 22f) / 6f)));
                    }

                    c = Color.Lerp(c, rimSand, rim * 0.6f);
                    c = Color.Lerp(c, clay, PatchWeight(x, z, 1.4f));
                    c = Color.Lerp(c, new Color(0.80f, 0.60f, 0.42f), steep * 0.5f);
                    break;
                }

                case BiomeType.Mountain:
                {
                    var snow = new Color(0.90f, 0.93f, 0.98f);
                    var snowShade = new Color(0.78f, 0.84f, 0.92f);
                    var gravel = new Color(0.60f, 0.61f, 0.65f);
                    var rock = new Color(0.46f, 0.47f, 0.52f);
                    c = Color.Lerp(snow, snowShade, variation * 0.7f);
                    c = Color.Lerp(c, gravel, Smooth01((patches - 0.62f) / 0.1f) * 0.85f);
                    c = Color.Lerp(c, rock, Mathf.Max(steep, rim * 0.35f));
                    break;
                }

                case BiomeType.Caves:
                {
                    var stone = new Color(0.27f, 0.25f, 0.25f);
                    var stoneDark = new Color(0.19f, 0.18f, 0.19f);
                    var wet = new Color(0.33f, 0.32f, 0.35f);
                    var depth = new Color(0.09f, 0.08f, 0.09f);
                    c = Color.Lerp(stone, stoneDark, variation);
                    c = Color.Lerp(c, wet, Smooth01((patches - 0.64f) / 0.08f) * 0.8f);
                    c = Color.Lerp(c, stoneDark, PatchWeight(x, z, 1f) * 0.6f);
                    // Darkness gathers towards the walls (the "ceiling" side of the cave).
                    c = Color.Lerp(c, depth, Mathf.Max(rim, Smooth01((r - 18f) / 14f) * 0.55f));
                    break;
                }

                default:
                {
                    var grass = new Color(0.33f, 0.52f, 0.22f);
                    var grassLight = new Color(0.43f, 0.60f, 0.27f);
                    var dirt = new Color(0.47f, 0.36f, 0.23f);
                    var shade = new Color(0.22f, 0.34f, 0.16f);
                    c = Color.Lerp(grass, grassLight, variation);
                    c = Color.Lerp(c, dirt, Smooth01((patches - 0.6f) / 0.1f) * 0.9f);
                    c = Color.Lerp(c, shade, rim * 0.8f);
                    c = Color.Lerp(c, dirt, steep * 0.6f);
                    break;
                }
            }

            c.r = Mathf.Clamp01(c.r + jitter);
            c.g = Mathf.Clamp01(c.g + jitter);
            c.b = Mathf.Clamp01(c.b + jitter);
            c.a = 1f;
            return c;
        }

        /// <summary>0..1 weight of the biome's patch (cracked earth / fissures) at XZ, soft over <paramref name="edge"/> metres.</summary>
        public float PatchWeight(float x, float z, float edge)
        {
            if (PatchRadius <= 0f)
            {
                return 0f;
            }

            float distance = Vector2.Distance(new Vector2(x, z), PatchCenter);
            return Smooth01((PatchRadius - distance) / Mathf.Max(0.01f, edge));
        }

        private float Dune(float x, float z)
        {
            float u = x * Mathf.Cos(_duneAngle) + z * Mathf.Sin(_duneAngle);
            float warp = Mathf.PerlinNoise(x * 0.03f + _o3.x, z * 0.03f + _o3.y) * 4f;
            float s = Mathf.Sin(u * 0.3f + warp) * 0.5f + 0.5f;
            return Mathf.Pow(s, 1.6f);
        }

        private static float Fbm(float x, float z)
        {
            float sum = Mathf.PerlinNoise(x, z) * 0.6f
                + Mathf.PerlinNoise(x * 2.1f + 17.3f, z * 2.1f + 5.1f) * 0.3f
                + Mathf.PerlinNoise(x * 4.3f + 41.7f, z * 4.3f + 29.9f) * 0.1f;
            return Mathf.Clamp01(sum);
        }

        private static float Ridged(float x, float z)
        {
            float a = 1f - Mathf.Abs(Mathf.PerlinNoise(x, z) * 2f - 1f);
            float b = 1f - Mathf.Abs(Mathf.PerlinNoise(x * 2.3f + 11.1f, z * 2.3f + 3.7f) * 2f - 1f);
            return Mathf.Clamp01(a * a * 0.7f + b * b * 0.3f);
        }

        private static float Hash(float x, float z)
        {
            unchecked
            {
                int h = Mathf.RoundToInt(x * 64f) * 73856093 ^ Mathf.RoundToInt(z * 64f) * 19349663;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
            }
        }

        public static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
