using System;
using System.Globalization;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Short labels for the big numbers of the 500-level curve (XP bars, master bar: up to 21 million per level, 1.85
    /// billion in total). Below <see cref="CompactFrom"/> the number is written in full, as before; above it is cut to
    /// thousands / millions / billions with one decimal at most ("288k", "21.3M", "1.8B"; pt-BR "21,3M"). Always
    /// truncated, never rounded up, so a bar one XP short of full never reads "21.3M/21.3M" by rounding. Pure.
    /// </summary>
    public static class LargeNumberFormat
    {
        /// <summary>Numbers below this are shown in full (5 digits fit every HUD label).</summary>
        public const long CompactFrom = 100_000;

        public static string Compact(long value, LocalizationManager localization) =>
            Compact(value, localization != null ? localization.CurrentLanguage : LanguageCode.en);

        public static string Compact(long value, LanguageCode language)
        {
            value = Math.Max(0L, value);
            if (value < CompactFrom)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            if (value < 1_000_000L)
            {
                return (value / 1_000L).ToString(CultureInfo.InvariantCulture) + "k";
            }

            if (value < 1_000_000_000L)
            {
                return OneDecimal(value, 1_000_000L, language) + "M";
            }

            return OneDecimal(value, 1_000_000_000L, language) + "B";
        }

        /// <summary>"current/max" with both sides compacted (the XP bar label).</summary>
        public static string Fraction(long current, long max, LanguageCode language) =>
            Compact(current, language) + "/" + Compact(max, language);

        private static string OneDecimal(long value, long unit, LanguageCode language)
        {
            long whole = value / unit;
            long tenth = value % unit * 10 / unit;
            if (whole >= 100 || tenth == 0)
            {
                return whole.ToString(CultureInfo.InvariantCulture);
            }

            string separator = language == LanguageCode.ptBR ? "," : ".";
            return whole.ToString(CultureInfo.InvariantCulture) + separator + tenth.ToString(CultureInfo.InvariantCulture);
        }
    }
}
