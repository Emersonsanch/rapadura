using UnityEngine;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Pure, UIDocument-free math behind the HUD skill hotbar's cooldown overlay/label — kept out
    /// of <see cref="HudController"/> for the same reason as <see cref="HudBarMath"/>: so it can be
    /// covered by EditMode tests without a real UI Toolkit panel.
    /// </summary>
    public static class SkillSlotDisplayMath
    {
        /// <summary>
        /// Fraction (0..1) of the cooldown still remaining — 1 right after casting, 0 when ready.
        /// Drives the overlay height. Guards against duration &lt;= 0 (no cooldown / never cast) and
        /// remaining &gt; duration (e.g. duration unknown and the definition's base value was passed).
        /// </summary>
        public static float ComputeCooldownFraction(float remainingSeconds, float durationSeconds)
        {
            if (durationSeconds <= 0f || remainingSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(remainingSeconds / durationSeconds);
        }

        /// <summary>Same as <see cref="ComputeCooldownFraction"/> as a 0..100 percentage (UI Toolkit Length).</summary>
        public static float ComputeCooldownPercent(float remainingSeconds, float durationSeconds)
        {
            return ComputeCooldownFraction(remainingSeconds, durationSeconds) * 100f;
        }

        /// <summary>
        /// Countdown text shown over a slot on cooldown: whole seconds rounded up at/above 1s
        /// ("3", "2", "1"), one decimal below it ("0.4"), empty when ready.
        /// </summary>
        public static string FormatCooldownLabel(float remainingSeconds)
        {
            if (remainingSeconds <= 0f)
            {
                return string.Empty;
            }

            int tenths = Mathf.CeilToInt(remainingSeconds * 10f);

            if (remainingSeconds >= 1f || tenths >= 10)
            {
                return Mathf.CeilToInt(remainingSeconds).ToString();
            }

            return (tenths / 10f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Integer key used to skip re-assigning the label string when it wouldn't change
        /// (avoids allocating a new string every HUD tick while a cooldown counts down).
        /// </summary>
        public static int CooldownLabelKey(float remainingSeconds)
        {
            if (remainingSeconds <= 0f)
            {
                return 0;
            }

            int tenths = Mathf.CeilToInt(remainingSeconds * 10f);
            return remainingSeconds >= 1f || tenths >= 10 ? Mathf.CeilToInt(remainingSeconds) * 100 : tenths;
        }
    }
}
