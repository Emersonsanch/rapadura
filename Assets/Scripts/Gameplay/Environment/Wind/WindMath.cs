using UnityEngine;

namespace Rapadura.Gameplay.Environment.Wind
{
    /// <summary>Which built-in sway tuning a <see cref="WindSway"/> uses.</summary>
    public enum WindSwayKind
    {
        None = 0,
        /// <summary>Tall, stiff, slow: whole-trunk lean of a few degrees with a soft canopy flutter.</summary>
        Tree = 1,
        /// <summary>Short, flexible, fast: blades bend far and flutter visibly.</summary>
        Grass = 2,
        /// <summary>Cacti, shrubs: barely moves, just enough to not read as a statue.</summary>
        Shrub = 3,
    }

    /// <summary>Tuning for one kind of swaying object. All angles in degrees, times in seconds.</summary>
    [System.Serializable]
    public struct WindSwayProfile
    {
        [Tooltip("Lean (degrees from rest) at wind strength 1. Gusts can push ~50% beyond this.")]
        public float MaxTilt;
        [Tooltip("Seconds the object takes to follow a change in wind (inertia). Trees are slow, grass is quick.")]
        public float ResponseSeconds;
        [Tooltip("Amplitude (degrees) of the high-frequency flutter added on top of the lean, at wind strength 1.")]
        public float FlutterAmplitude;
        [Tooltip("Flutter cycles per second.")]
        public float FlutterFrequency;

        public static readonly WindSwayProfile Tree = new WindSwayProfile { MaxTilt = 3.5f, ResponseSeconds = 1.2f, FlutterAmplitude = 0.7f, FlutterFrequency = 1.1f };
        public static readonly WindSwayProfile Grass = new WindSwayProfile { MaxTilt = 22f, ResponseSeconds = 0.25f, FlutterAmplitude = 6f, FlutterFrequency = 2.3f };
        public static readonly WindSwayProfile Shrub = new WindSwayProfile { MaxTilt = 1.5f, ResponseSeconds = 0.8f, FlutterAmplitude = 0.4f, FlutterFrequency = 1.6f };

        public static WindSwayProfile For(WindSwayKind kind)
        {
            switch (kind)
            {
                case WindSwayKind.Tree: return Tree;
                case WindSwayKind.Grass: return Grass;
                case WindSwayKind.Shrub: return Shrub;
                default: return default;
            }
        }
    }

    /// <summary>
    /// Pure wind math (EditMode-tested, no scene access). The wind is a planar (XZ) vector field:
    /// a prevailing direction and strength from the weather, with gusts that travel downwind as a
    /// Perlin band (so neighbouring trees bend together, and the gust visibly passes through the
    /// field) plus a little cross-wind turbulence so nothing moves in perfect unison.
    /// </summary>
    public static class WindMath
    {
        /// <summary>Strength floor/ceiling exposed to swayers; strength 1 = sandstorm.</summary>
        public const float MaxStrength = 1.5f;

        /// <summary>Gusts multiply the prevailing strength by [1 - Gustiness, 1 + Gustiness].</summary>
        public const float Gustiness = 0.45f;

        /// <summary>How fast gust bands travel downwind, in world units per second.</summary>
        public const float GustTravelSpeed = 4f;

        /// <summary>Size of a gust band along the wind, in world units (bigger = broader, slower swells).</summary>
        public const float GustLength = 14f;

        /// <summary>Prevailing wind strength for a weather type (0..1).</summary>
        public static float StrengthFor(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Sandstorm: return 1f;
                case WeatherType.Rain: return 0.7f;
                case WeatherType.Snow: return 0.55f;
                case WeatherType.Fog: return 0.18f;
                default: return 0.3f; // Clear: a light breeze so foliage never freezes solid
            }
        }

        /// <summary>Strength while <see cref="WeatherSystem"/> blends from the previous weather into the current one.</summary>
        public static float BlendedStrength(WeatherType previous, WeatherType current, float transitionProgress)
        {
            return Mathf.Lerp(StrengthFor(previous), StrengthFor(current), Mathf.Clamp01(transitionProgress));
        }

        /// <summary>
        /// Wind vector (XZ, |v| in [0, MaxStrength]) at a world position and time, for a prevailing
        /// direction (normalised XZ) and strength. Deterministic: same inputs, same result.
        /// </summary>
        public static Vector2 Sample(Vector2 worldXZ, float time, Vector2 direction, float strength)
        {
            if (strength <= 0f || direction.sqrMagnitude < 1e-6f)
            {
                return Vector2.zero;
            }

            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float along = Vector2.Dot(worldXZ, direction);
            float across = Vector2.Dot(worldXZ, perpendicular);

            // Gust band drifting downwind: sample Perlin in a frame that moves with the gusts.
            float gustU = (along - time * GustTravelSpeed) / GustLength;
            float gustV = across / (GustLength * 2.5f) + time * 0.03f;
            float gust = Mathf.PerlinNoise(gustU + 37.1f, gustV + 11.7f); // 0..1
            float gustFactor = Mathf.Lerp(1f - Gustiness, 1f + Gustiness, gust);

            // Cross-wind turbulence: smaller, faster and off-phase from the gust band.
            float turbulence = (Mathf.PerlinNoise(gustU * 2.7f + 91.3f, gustV * 2.7f + 53.9f) - 0.5f) * 0.5f;

            Vector2 wind = direction * (strength * gustFactor) + perpendicular * (strength * turbulence);
            float magnitude = wind.magnitude;
            if (magnitude > MaxStrength)
            {
                wind *= MaxStrength / magnitude;
            }

            return wind;
        }

        /// <summary>Exponential smoothing of the felt wind toward the sampled wind (inertia), stable for any dt.</summary>
        public static Vector2 Smooth(Vector2 felt, Vector2 sampled, float responseSeconds, float deltaTime)
        {
            if (responseSeconds <= 0f || deltaTime <= 0f)
            {
                return sampled;
            }

            float k = 1f - Mathf.Exp(-deltaTime / responseSeconds);
            return felt + (sampled - felt) * k;
        }

        /// <summary>
        /// Lean angle (degrees) from a felt wind vector: proportional to the wind, capped at 1.5x
        /// the profile's MaxTilt, plus a flutter whose amplitude grows with the square root of the
        /// wind (so a breeze still flickers the grass a little, and a storm does not shake it apart).
        /// </summary>
        public static float TiltAngle(Vector2 feltWind, in WindSwayProfile profile, float time, float phase)
        {
            float magnitude = feltWind.magnitude;
            float lean = Mathf.Min(magnitude * profile.MaxTilt, profile.MaxTilt * 1.5f);
            float flutter = profile.FlutterAmplitude * Mathf.Sqrt(Mathf.Clamp01(magnitude)) *
                            Mathf.Sin((time * profile.FlutterFrequency + phase) * Mathf.PI * 2f);
            return lean + flutter;
        }

        /// <summary>
        /// World-space axis to rotate about so that "up" leans toward the wind: up × wind. Zero vector
        /// when there is no wind (callers then leave the rest pose alone).
        /// </summary>
        public static Vector3 TiltAxis(Vector2 feltWind)
        {
            if (feltWind.sqrMagnitude < 1e-8f)
            {
                return Vector3.zero;
            }

            Vector3 windWorld = new Vector3(feltWind.x, 0f, feltWind.y).normalized;
            return Vector3.Cross(Vector3.up, windWorld);
        }

        /// <summary>Stable per-instance phase (0..1) from a world position, so instances never move in lockstep and the phase survives reloads.</summary>
        public static float PhaseFor(Vector3 worldPosition)
        {
            int seed = Mathf.RoundToInt(worldPosition.x * 97f) * 73856093 ^ Mathf.RoundToInt(worldPosition.z * 97f) * 19349663 ^ Mathf.RoundToInt(worldPosition.y * 97f) * 83492791;
            uint hash = (uint)seed;
            hash ^= hash >> 13;
            hash *= 0x5bd1e995;
            hash ^= hash >> 15;
            return (hash & 0xFFFF) / 65535f;
        }

        /// <summary>Slowly veering prevailing direction (normalised XZ) so the wind does not come from one fixed bearing all day.</summary>
        public static Vector2 PrevailingDirection(float baseBearingDegrees, float time)
        {
            float veer = (Mathf.PerlinNoise(time * 0.01f + 5.3f, 0.5f) - 0.5f) * 90f; // ±45° over minutes
            float radians = (baseBearingDegrees + veer) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }
    }
}
