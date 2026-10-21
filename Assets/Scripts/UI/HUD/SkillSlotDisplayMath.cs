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

        /// <summary>Characters per line that fit a 64px slot at the USS font size (9px bold) with font scale 1.</summary>
        public const int SlotNameBaseCharsPerLine = 8;

        /// <summary>Never shorten a word below this, even at the largest accessibility font scale.</summary>
        public const int SlotNameMinCharsPerLine = 3;

        /// <summary>Slot name fallback is at most this many lines (one significant word per line).</summary>
        public const int SlotNameMaxLines = 2;

        private static readonly string[] ConnectorWords = { "de", "da", "do", "das", "dos", "e", "a", "o", "of", "the", "and" };

        /// <summary>
        /// Characters per line for the slot name at an accessibility font scale (UiAccessibilityApplier
        /// scales the label's font, not the slot, so bigger text means fewer characters fit).
        /// </summary>
        public static int SlotNameCharsPerLine(float fontScale)
        {
            float scale = fontScale > 1f ? fontScale : 1f;
            return Mathf.Max(SlotNameMinCharsPerLine, Mathf.FloorToInt(SlotNameBaseCharsPerLine / scale));
        }

        /// <summary>
        /// Short, never-broken-mid-word text for a skill slot without an icon: one significant word
        /// per line (connectors like "de"/"of" dropped, at most <see cref="SlotNameMaxLines"/>),
        /// each word cut to <paramref name="maxCharsPerLine"/> with a trailing "." when shortened —
        /// "Disparo Explosivo" -> "Disparo\nExplosi.", "Chuva de Flechas" -> "Chuva\nFlechas".
        /// The label uses white-space: nowrap, so the only line breaks are the ones made here.
        /// </summary>
        public static string FormatSlotName(string displayName, int maxCharsPerLine)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return string.Empty;
            }

            int maxChars = Mathf.Max(SlotNameMinCharsPerLine, maxCharsPerLine);
            string[] words = displayName.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

            var lines = new System.Collections.Generic.List<string>(SlotNameMaxLines);

            foreach (string word in words)
            {
                if (lines.Count >= SlotNameMaxLines)
                {
                    break;
                }

                // Connectors are skipped unless they're the very first word (a single-word name stays intact).
                if (lines.Count > 0 && IsConnector(word))
                {
                    continue;
                }

                lines.Add(word.Length > maxChars ? word.Substring(0, maxChars - 1) + "." : word);
            }

            return string.Join("\n", lines);
        }

        private static bool IsConnector(string word)
        {
            foreach (string connector in ConnectorWords)
            {
                if (string.Equals(word, connector, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
