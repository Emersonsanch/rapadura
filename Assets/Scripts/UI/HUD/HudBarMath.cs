using UnityEngine;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Pure, UIDocument-free math used by the HUD bars (health/stamina/mana/XP).
    /// Kept separate from <see cref="HudController"/> so it can be covered by EditMode
    /// tests without needing a real <c>UIDocument</c>/<c>PanelSettings</c> (those only
    /// exist once the project has been opened in the Editor — see TODO.md Fase 1 note).
    /// </summary>
    public static class HudBarMath
    {
        /// <summary>
        /// Computes the fill fraction (0..1) of a bar given its current/max values.
        /// Clamps defensively against negative values, current &gt; max and max &lt;= 0
        /// (division-by-zero guard) so callers never have to sanitize input first.
        /// </summary>
        public static float ComputeFillFraction(float current, float max)
        {
            if (max <= 0f)
            {
                return 0f;
            }

            float fraction = current / max;
            return Mathf.Clamp01(fraction);
        }

        /// <summary>Same as <see cref="ComputeFillFraction"/> but expressed as a 0..100 percentage.</summary>
        public static float ComputeFillPercent(float current, float max)
        {
            return ComputeFillFraction(current, max) * 100f;
        }

        /// <summary>
        /// True when the fill fraction is at or below <paramref name="threshold"/> (default 25%),
        /// used by <see cref="HudController"/> to swap a bar into its "low value" warning color.
        /// </summary>
        public static bool IsLow(float current, float max, float threshold = 0.25f)
        {
            return ComputeFillFraction(current, max) <= threshold;
        }

        /// <summary>Formats a "current/max" label, rounding to whole numbers like the rest of the HUD.</summary>
        public static string FormatValueLabel(float current, float max)
        {
            return $"{Mathf.RoundToInt(Mathf.Max(0f, current))}/{Mathf.RoundToInt(Mathf.Max(0f, max))}";
        }
    }
}
