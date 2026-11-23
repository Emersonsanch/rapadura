using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>
    /// Guild names and the word filter shared by the name, the motto, the board and the message of the day. Pure C#.
    /// <list type="bullet">
    /// <item>Name: <see cref="MinNameLength"/>–<see cref="MaxNameLength"/> characters after <see cref="Clean"/> (trim,
    /// single spaces); letters with or without accents, digits (at most <see cref="MaxNameDigits"/>) and spaces; starts
    /// with a letter.</item>
    /// <item>Unique by <see cref="NameKey"/>: case, accents and spacing do not make a name different ("Coluna da Glória" =
    /// "coluna  da gloria").</item>
    /// <item>Filter (<see cref="IsOffensive"/>): lower case, accents removed, "leetspeak" undone (0→o, 1→i, 3→e, 4→a,
    /// 5→s, 7→t, 8→b, @→a, $→s, !→i), repeated letters collapsed ("merdaaa" = "merda"), spaced-out letters joined
    /// ("p u t a"). <see cref="ContainsWords"/> are caught anywhere (they are long and distinctive);
    /// <see cref="WholeWords"/> only as a whole word, so "Escudo", "Espora" and "Disputa" pass. Both lists are small on
    /// purpose and extensible: the server adds its own with <see cref="AddContainsWord"/> / <see cref="AddWholeWord"/>.</item>
    /// </list>
    /// </summary>
    public static class GuildNameRules
    {
        public const int MinNameLength = 3;
        public const int MaxNameLength = 16;
        public const int MaxNameDigits = 3;

        /// <summary>Caught anywhere in the text (after normalization, so written as they collapse: no word whose collapsed form is a
        /// harmless word, e.g. "faggot" → "fagot" = "fagote", "nigger" → "niger"; those go in <see cref="WholeWordList"/>). PT and EN.</summary>
        private static readonly List<string> ContainsWordList = new List<string>
        {
            "caralho", "buceta", "fodase", "foder", "arrombad", "estupr", "pedofil", "hitler", "nazis", "nazism",
            "fuck", "bitch", "whore", "motherf",
        };

        /// <summary>Caught only as a whole word (short ones that hide inside harmless words). PT and EN. Never "ass": collapsed it is
        /// "as", the Portuguese article.</summary>
        private static readonly List<string> WholeWordList = new List<string>
        {
            "cu", "puta", "puto", "porra", "merda", "foda", "viado", "cuzao", "fdp", "pqp", "vsf", "corno", "nazi",
            "shit", "cunt", "dick", "cock", "sex", "sexo", "porn", "rape", "nigga", "nigger", "fag", "faggot",
        };

        /// <summary>Not offensive, but never a guild name (it would pose as the staff).</summary>
        private static readonly List<string> ReservedNameWords = new List<string>
        {
            "gm", "adm", "admin", "moderador", "moderator", "mod", "staff", "suporte", "support", "sistema", "system", "oficial",
        };

        public static IReadOnlyList<string> ContainsWords => ContainsWordList;
        public static IReadOnlyList<string> WholeWords => WholeWordList;
        public static IReadOnlyList<string> ReservedWords => ReservedNameWords;

        private static HashSet<string> _containsNormalized;
        private static HashSet<string> _wholeNormalized;
        private static HashSet<string> _reservedNormalized;

        public static void AddContainsWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                ContainsWordList.Add(word);
                _containsNormalized = null;
            }
        }

        public static void AddWholeWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                WholeWordList.Add(word);
                _wholeNormalized = null;
            }
        }

        /// <summary>Trims and collapses any run of whitespace to one space. Null → "".</summary>
        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(text.Length);
            bool pendingSpace = false;
            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>The comparison key of a name: <see cref="Clean"/>, lower case, no accents.</summary>
        public static string NameKey(string name) => RemoveDiacritics(Clean(name).ToLowerInvariant());

        /// <summary>
        /// Shape of a name (already <see cref="Clean"/>ed) and the filter. Uniqueness is the caller's
        /// (<see cref="GuildRules.Create"/> asks <c>isNameKeyTaken</c>).
        /// </summary>
        public static GuildRefusal ValidateName(string name)
        {
            string cleaned = Clean(name);
            if (cleaned.Length < MinNameLength)
            {
                return GuildRefusal.NameTooShort;
            }

            if (cleaned.Length > MaxNameLength)
            {
                return GuildRefusal.NameTooLong;
            }

            if (!IsBasicLetter(cleaned[0]))
            {
                return GuildRefusal.NameInvalidCharacters;
            }

            int digits = 0;
            foreach (char c in cleaned)
            {
                if (c >= '0' && c <= '9')
                {
                    digits++;
                }
                else if (c != ' ' && !IsBasicLetter(c))
                {
                    return GuildRefusal.NameInvalidCharacters;
                }
            }

            if (digits > MaxNameDigits)
            {
                return GuildRefusal.NameInvalidCharacters;
            }

            if (IsOffensive(cleaned) || IsReserved(cleaned))
            {
                return GuildRefusal.NameOffensive;
            }

            return GuildRefusal.None;
        }

        /// <summary>
        /// A free text (motto, notice, message of the day): at most <paramref name="maxLength"/> characters after
        /// <see cref="Clean"/>, not empty unless <paramref name="allowEmpty"/>, and through the filter.
        /// </summary>
        public static GuildRefusal ValidateText(string text, int maxLength, bool allowEmpty)
        {
            string cleaned = Clean(text);
            if (cleaned.Length == 0)
            {
                return allowEmpty ? GuildRefusal.None : GuildRefusal.TextTooLong;
            }

            if (cleaned.Length > maxLength)
            {
                return GuildRefusal.TextTooLong;
            }

            return IsOffensive(cleaned) ? GuildRefusal.TextOffensive : GuildRefusal.None;
        }

        /// <summary>The word filter (see the class summary).</summary>
        public static bool IsOffensive(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            EnsureLists();
            List<string> tokens = Tokens(text);
            var compact = new StringBuilder();
            foreach (string token in tokens)
            {
                if (_wholeNormalized.Contains(token))
                {
                    return true;
                }

                compact.Append(token);
            }

            string all = Collapse(compact.ToString());
            foreach (string word in _containsNormalized)
            {
                if (all.Contains(word))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsReserved(string name)
        {
            EnsureLists();
            foreach (string token in Tokens(name))
            {
                if (_reservedNormalized.Contains(token))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Lower case, no accents, leetspeak undone, everything but a–z as a separator, repeats collapsed, single letters joined.</summary>
        internal static List<string> Tokens(string text)
        {
            string plain = RemoveDiacritics(text.ToLowerInvariant());
            var raw = new List<string>();
            var current = new StringBuilder();
            foreach (char original in plain)
            {
                char c = UnLeet(original);
                if (c >= 'a' && c <= 'z')
                {
                    current.Append(c);
                }
                else if (current.Length > 0)
                {
                    raw.Add(Collapse(current.ToString()));
                    current.Clear();
                }
            }

            if (current.Length > 0)
            {
                raw.Add(Collapse(current.ToString()));
            }

            // "p u t a" / "f.u.c.k": runs of one-letter tokens are one word.
            var tokens = new List<string>(raw.Count);
            var run = new StringBuilder();
            foreach (string token in raw)
            {
                if (token.Length == 1)
                {
                    run.Append(token);
                    continue;
                }

                FlushRun(run, tokens);
                tokens.Add(token);
            }

            FlushRun(run, tokens);
            return tokens;
        }

        private static void FlushRun(StringBuilder run, List<string> tokens)
        {
            if (run.Length > 0)
            {
                tokens.Add(Collapse(run.ToString()));
                run.Clear();
            }
        }

        private static char UnLeet(char c)
        {
            switch (c)
            {
                case '0': return 'o';
                case '1': return 'i';
                case '3': return 'e';
                case '4': return 'a';
                case '5': return 's';
                case '7': return 't';
                case '8': return 'b';
                case '@': return 'a';
                case '$': return 's';
                case '!': return 'i';
                default: return c;
            }
        }

        /// <summary>"merdaaa" → "merda", "porra" → "pora": every run of one letter becomes one letter.</summary>
        private static string Collapse(string word)
        {
            var builder = new StringBuilder(word.Length);
            char last = '\0';
            foreach (char c in word)
            {
                if (c != last)
                {
                    builder.Append(c);
                    last = c;
                }
            }

            return builder.ToString();
        }

        internal static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string decomposed = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>A–Z / a–z with or without accents (á, ç, ñ…); no other alphabets, so lookalike letters cannot dodge the filter.</summary>
        private static bool IsBasicLetter(char c)
        {
            string plain = RemoveDiacritics(c.ToString());
            if (plain.Length != 1)
            {
                return false;
            }

            char p = plain[0];
            return (p >= 'a' && p <= 'z') || (p >= 'A' && p <= 'Z');
        }

        private static void EnsureLists()
        {
            if (_containsNormalized == null)
            {
                _containsNormalized = NormalizeList(ContainsWordList);
            }

            if (_wholeNormalized == null)
            {
                _wholeNormalized = NormalizeList(WholeWordList);
            }

            if (_reservedNormalized == null)
            {
                _reservedNormalized = NormalizeList(ReservedNameWords);
            }
        }

        private static HashSet<string> NormalizeList(List<string> words)
        {
            var set = new HashSet<string>();
            foreach (string word in words)
            {
                foreach (string token in Tokens(word))
                {
                    set.Add(token);
                }
            }

            return set;
        }
    }
}
