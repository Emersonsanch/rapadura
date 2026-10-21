using UnityEngine;

namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>One particle burst the VFX system is about to emit, before/after accessibility adjustments.</summary>
    public struct SkillVfxBurst
    {
        public int Count;
        public Color Color;
        public float Size;

        /// <summary>A pure "flash" (single bright, very short-lived glow sprite). Skipped entirely under reduce-flash.</summary>
        public bool IsFlash;

        /// <summary>Burst comes from a <see cref="SkillVfxPreset.flashLike"/> preset (bright/strobe-ish). Dimmed under reduce-flash.</summary>
        public bool IsFlashLike;

        public SkillVfxBurst(int count, Color color, float size, bool isFlash, bool isFlashLike)
        {
            Count = count;
            Color = color;
            Size = size;
            IsFlash = isFlash;
            IsFlashLike = isFlashLike;
        }

        public bool IsSkipped => Count <= 0;
    }

    /// <summary>
    /// Pure policy applying <c>AccessibilitySettings.ReduceFlashEffects</c> to skill VFX:
    ///   - flash sprites (<see cref="SkillVfxBurst.IsFlash"/>) are skipped entirely;
    ///   - bursts from flash-like presets are thinned out and dimmed (lower alpha + brightness, smaller);
    ///   - everything else is left untouched (non-flashy smoke/debris/auras aren't a photosensitivity concern).
    /// Static and allocation-free so it can run on every burst and be tested in EditMode.
    /// </summary>
    public static class SkillVfxFlashPolicy
    {
        public const float ReducedCountMultiplier = 0.5f;
        public const float ReducedAlphaMultiplier = 0.45f;
        public const float ReducedBrightnessMultiplier = 0.7f;
        public const float ReducedSizeMultiplier = 0.8f;

        public static SkillVfxBurst Apply(SkillVfxBurst burst, bool reduceFlashes)
        {
            if (!reduceFlashes)
            {
                return burst;
            }

            if (burst.IsFlash)
            {
                burst.Count = 0;
                return burst;
            }

            if (!burst.IsFlashLike)
            {
                return burst;
            }

            if (burst.Count > 0)
            {
                burst.Count = Mathf.Max(1, Mathf.RoundToInt(burst.Count * ReducedCountMultiplier));
            }

            Color c = burst.Color;
            burst.Color = new Color(
                c.r * ReducedBrightnessMultiplier,
                c.g * ReducedBrightnessMultiplier,
                c.b * ReducedBrightnessMultiplier,
                c.a * ReducedAlphaMultiplier);
            burst.Size *= ReducedSizeMultiplier;
            return burst;
        }

        /// <summary>Whether a flash-like preset should keep its additive (glow) blending. Reduce-flash forces plain alpha blending.</summary>
        public static bool UseAdditive(bool presetAdditive, bool presetFlashLike, bool reduceFlashes)
        {
            return presetAdditive && !(reduceFlashes && presetFlashLike);
        }
    }
}
