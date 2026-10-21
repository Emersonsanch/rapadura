using System;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>Motion/shape family of an ambient particle layer. The preset sets the look; the kind sets how particles move.</summary>
    public enum AmbientVfxKind
    {
        /// <summary>Slow wandering glow dots (noise-driven), optionally blinking.</summary>
        Fireflies = 0,

        /// <summary>Tumbling flakes falling from above with sideways drift.</summary>
        FallingLeaves = 1,

        /// <summary>Tiny almost-still specks floating in the air.</summary>
        DustMotes = 2,

        /// <summary>Faint, slowly rising soft blobs close to the ground.</summary>
        HeatShimmer = 3,

        /// <summary>Light snowfall with gentle drift (heavier snow is <see cref="WeatherVisuals"/>' job).</summary>
        SnowFlurry = 4,

        /// <summary>Stretched streaks blown sideways.</summary>
        WindWisps = 5,

        /// <summary>Sparse drops falling from the ceiling, optionally twinkling.</summary>
        Drips = 6,
    }

    /// <summary>Bit mask over <see cref="TimeOfDayPhase"/> (bit = 1 &lt;&lt; phase).</summary>
    [Flags]
    public enum AmbientPhaseMask
    {
        None = 0,
        Dawn = 1 << (int)TimeOfDayPhase.Dawn,
        Day = 1 << (int)TimeOfDayPhase.Day,
        Dusk = 1 << (int)TimeOfDayPhase.Dusk,
        Night = 1 << (int)TimeOfDayPhase.Night,
        All = Dawn | Day | Dusk | Night,
    }

    /// <summary>Bit mask over <see cref="WeatherType"/> (bit = 1 &lt;&lt; weather).</summary>
    [Flags]
    public enum AmbientWeatherMask
    {
        None = 0,
        Clear = 1 << (int)WeatherType.Clear,
        Rain = 1 << (int)WeatherType.Rain,
        Fog = 1 << (int)WeatherType.Fog,
        Sandstorm = 1 << (int)WeatherType.Sandstorm,
        Snow = 1 << (int)WeatherType.Snow,
        All = Clear | Rain | Fog | Sandstorm | Snow,
    }

    /// <summary>
    /// One ambient particle layer for a biome (e.g. Forest fireflies), active only in the listed
    /// phases/weathers. Plain serializable data (same style as <c>SkillVfxPreset</c>); turned into a
    /// procedural ParticleSystem by <see cref="AmbientVfx"/>.
    /// </summary>
    [Serializable]
    public class AmbientVfxPreset
    {
        public string presetId = "ambient";
        public BiomeType biome = BiomeType.Forest;
        public AmbientVfxKind kind = AmbientVfxKind.DustMotes;
        public AmbientPhaseMask phases = AmbientPhaseMask.All;
        public AmbientWeatherMask weathers = AmbientWeatherMask.All;

        public Color primaryColor = new Color(1f, 1f, 1f, 0.6f);
        public Color secondaryColor = new Color(1f, 1f, 1f, 0.4f);

        [Tooltip("Particles per second at full strength (before scene/mobile cap scaling).")]
        [Min(0f)] public float rate = 6f;
        [Tooltip("Particle cap for this layer. Always clamped to AmbientVfxLibrary.MaxParticlesCap (80).")]
        [Range(1, AmbientVfxLibrary.MaxParticlesCap)] public int maxParticles = 40;
        public float particleSize = 0.08f;
        public float particleLifetime = 5f;
        public float particleSpeed = 0.2f;
        [Tooltip("Negative rises, positive falls.")]
        public float gravity = 0f;
        [Tooltip("Emitter box size (x/z footprint, y height) centred on the camera follow point.")]
        public Vector3 area = new Vector3(18f, 4f, 18f);
        [Tooltip("Vertical offset of the emitter box from the follow point.")]
        public float heightOffset = 1.5f;

        [Tooltip("Particles pulse on/off (fireflies, sparkles). Disabled under 'reduce flashes' (steady glow instead).")]
        public bool blink = false;
        [Tooltip("Bright/sparkly look. Dimmed under 'reduce flashes' via SkillVfxFlashPolicy.")]
        public bool flashLike = false;
        [Tooltip("Additive (glowy) blending instead of plain alpha blending.")]
        public bool additive = false;

        public AmbientVfxPreset()
        {
        }

        public AmbientVfxPreset(string id, BiomeType biomeType, AmbientVfxKind vfxKind, AmbientPhaseMask phaseMask, AmbientWeatherMask weatherMask,
            Color primary, Color secondary, float ratePerSecond, int cap, float size, float lifetime, float speed, float gravityModifier,
            Vector3 emitterArea, float height, bool isBlinking = false, bool isFlashLike = false, bool isAdditive = false)
        {
            presetId = id;
            biome = biomeType;
            kind = vfxKind;
            phases = phaseMask;
            weathers = weatherMask;
            primaryColor = primary;
            secondaryColor = secondary;
            rate = ratePerSecond;
            maxParticles = cap;
            particleSize = size;
            particleLifetime = lifetime;
            particleSpeed = speed;
            gravity = gravityModifier;
            area = emitterArea;
            heightOffset = height;
            blink = isBlinking;
            flashLike = isFlashLike;
            additive = isAdditive;
        }

        public bool AllowsPhase(TimeOfDayPhase phase) => (phases & (AmbientPhaseMask)(1 << (int)phase)) != 0;

        public bool AllowsWeather(WeatherType weather) => (weathers & (AmbientWeatherMask)(1 << (int)weather)) != 0;
    }
}
