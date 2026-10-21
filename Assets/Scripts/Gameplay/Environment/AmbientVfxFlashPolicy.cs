using Rapadura.Gameplay.Skills.Vfx;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Pure policy applying <c>AccessibilitySettings.ReduceFlashEffects</c> to ambient layers, built
    /// on <see cref="SkillVfxFlashPolicy"/> so skill and ambient VFX dim the same way:
    ///   - blinking layers (fireflies, sparkles) switch to a steady glow (no alpha pulses);
    ///   - flash-like layers get SkillVfxFlashPolicy's dimmed colours and lose additive blending;
    ///   - everything else is untouched.
    /// Static and allocation-free (the alpha-key arrays are shared, read-only templates).
    /// </summary>
    public static class AmbientVfxFlashPolicy
    {
        // Three soft pulses over the particle's lifetime (≈0.5 Hz for 6 s fireflies — well under 3 Hz).
        private static readonly GradientAlphaKey[] BlinkAlphaKeys =
        {
            new GradientAlphaKey(0f, 0f),
            new GradientAlphaKey(1f, 0.12f),
            new GradientAlphaKey(0.15f, 0.27f),
            new GradientAlphaKey(1f, 0.42f),
            new GradientAlphaKey(0.2f, 0.58f),
            new GradientAlphaKey(1f, 0.74f),
            new GradientAlphaKey(0f, 1f),
        };

        private static readonly GradientAlphaKey[] SteadyAlphaKeys =
        {
            new GradientAlphaKey(0f, 0f),
            new GradientAlphaKey(1f, 0.2f),
            new GradientAlphaKey(1f, 0.8f),
            new GradientAlphaKey(0f, 1f),
        };

        private static readonly GradientColorKey[] WhiteColorKeys =
        {
            new GradientColorKey(Color.white, 0f),
            new GradientColorKey(Color.white, 1f),
        };

        public static bool ShouldBlink(AmbientVfxPreset preset, bool reduceFlashes)
        {
            return preset != null && preset.blink && !reduceFlashes;
        }

        public static bool UseAdditive(AmbientVfxPreset preset, bool reduceFlashes)
        {
            return preset != null && SkillVfxFlashPolicy.UseAdditive(preset.additive, preset.flashLike, reduceFlashes);
        }

        public static Color ResolveColor(Color color, bool flashLike, bool reduceFlashes)
        {
            return SkillVfxFlashPolicy.Apply(new SkillVfxBurst(1, color, 1f, false, flashLike), reduceFlashes).Color;
        }

        /// <summary>Alpha-over-lifetime keys: pulsing when blinking, a plain fade in/hold/fade out otherwise.</summary>
        public static GradientAlphaKey[] GetAlphaKeys(bool blink) => blink ? BlinkAlphaKeys : SteadyAlphaKeys;

        /// <summary>Writes the colour-over-lifetime gradient for a layer into <paramref name="gradient"/> (reused, no new Gradient).</summary>
        public static void ApplyLifetimeGradient(Gradient gradient, AmbientVfxPreset preset, bool reduceFlashes)
        {
            gradient.SetKeys(WhiteColorKeys, GetAlphaKeys(ShouldBlink(preset, reduceFlashes)));
        }

        /// <summary>True when any alpha key after the first fade-in drops back down (i.e. the curve pulses).</summary>
        public static bool IsPulsing(GradientAlphaKey[] keys)
        {
            if (keys == null)
            {
                return false;
            }

            bool risen = false;
            for (int i = 1; i < keys.Length - 1; i++)
            {
                if (keys[i].alpha >= 0.99f)
                {
                    risen = true;
                }
                else if (risen && keys[i].alpha < 0.5f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
