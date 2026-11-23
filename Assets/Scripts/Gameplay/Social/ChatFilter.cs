using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Gameplay.Guild;

namespace Rapadura.Gameplay.Social
{
    /// <summary>What <see cref="ChatFilter.Apply"/> did to one text.</summary>
    public readonly struct ChatFilterResult
    {
        /// <summary>The text to show: same length as the input, each letter (or digit) of a caught word swapped for '*'.</summary>
        public readonly string Text;

        /// <summary>How many words were masked (0 = the text went through untouched).</summary>
        public readonly int MaskedWords;

        public ChatFilterResult(string text, int maskedWords)
        {
            Text = text;
            MaskedWords = maskedWords;
        }

        public bool Masked => MaskedWords > 0;
    }

    /// <summary>
    /// The word filter of the chat (TODO.md Fase 12.3, "Chat ... com filtro de palavras"). Pure C#, the server runs it on
    /// every message before routing it, and the client may run it too (a client-side filter is only a courtesy).
    /// <list type="bullet">
    /// <item><b>One list for the whole game.</b> The words come from <see cref="GuildNameRules.ContainsWords"/> and
    /// <see cref="GuildNameRules.WholeWords"/> (the guild names, motto, board and message of the day use the same ones, and a
    /// word the server adds there with <see cref="GuildNameRules.AddContainsWord"/> reaches the chat too), plus a few
    /// chat-only insults in <see cref="ChatWholeWords"/> / <see cref="ChatContainsWords"/>, extensible with
    /// <see cref="AddWholeWord"/> / <see cref="AddContainsWord"/>.</item>
    /// <item><b>Same normalization as the guild:</b> lower case, accents removed, leetspeak undone (0→o, 1→i, 3→e, 4→a, 5→s,
    /// 7→t, 8→b, @→a, $→s, !→i), repeated letters collapsed ("merdaaa"), runs of one-letter words joined ("p u t a",
    /// "f.u.c.k"); invisible characters (zero-width space, control characters) are dropped first by <see cref="Clean"/>,
    /// so "pu​ta" is one word.</item>
    /// <item><b>Masking, not refusing:</b> the message still goes, with "*" over every character of the caught word, and
    /// the length stays the same (the rest of the sentence keeps its positions).</item>
    /// <item><b>Difference from the guild filter, on purpose:</b> a "contains" word is looked for inside one word (or one run of
    /// spaced-out letters), never across two words. A guild name is two or three words, a chat line is a whole sentence:
    /// "o pé do filho" would read "pedofilho" across the words. The price: "fu ck" passes; the report button is for that.</item>
    /// </list>
    /// </summary>
    public static class ChatFilter
    {
        public const char MaskChar = '*';

        /// <summary>Chat-only, caught as a whole word (short insults that hide inside harmless words). PT and EN.</summary>
        private static readonly List<string> ChatWholeWordList = new List<string>
        {
            // Never "piranha" (a fish of the bestiary) nor "retardado" ("efeito retardado" = delayed effect).
            "bosta", "otario", "otaria", "babaca", "vagabunda", "vadia",
            "slut", "bastard", "retard", "retarded", "dumbass", "jackass",
        };

        /// <summary>Chat-only, caught inside a word (long and distinctive, written as they collapse). PT and EN.</summary>
        private static readonly List<string> ChatContainsWordList = new List<string>
        {
            "filhodaputa", "ashole", "bulshit",
        };

        public static IReadOnlyList<string> ChatWholeWords => ChatWholeWordList;
        public static IReadOnlyList<string> ChatContainsWords => ChatContainsWordList;

        private static HashSet<string> _whole;
        private static List<string> _contains;
        private static int _guildWholeCount = -1;
        private static int _guildContainsCount = -1;

        public static void AddWholeWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                ChatWholeWordList.Add(word);
                _whole = null;
            }
        }

        public static void AddContainsWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                ChatContainsWordList.Add(word);
                _contains = null;
            }
        }

        /// <summary>
        /// Trims, collapses any run of whitespace to one space and drops control and invisible format characters (zero-width
        /// space, joiners, direction marks). Null → "". What the chat counts for the length limit.
        /// </summary>
        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var visible = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    visible.Append(' ');
                    continue;
                }

                if (char.IsControl(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format)
                {
                    continue;
                }

                visible.Append(c);
            }

            return GuildNameRules.Clean(visible.ToString());
        }

        /// <summary>True when <see cref="Apply"/> would mask something.</summary>
        public static bool IsOffensive(string text) => Apply(text).Masked;

        /// <summary>Masks every caught word of <paramref name="text"/> (see the class summary). The text is not <see cref="Clean"/>ed here.</summary>
        public static ChatFilterResult Apply(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ChatFilterResult(string.Empty, 0);
            }

            EnsureLists();
            List<Word> words = Words(text);
            char[] output = null;
            int masked = 0;
            foreach (Word word in words)
            {
                bool caught = _whole.Contains(word.Collapsed);
                if (!caught)
                {
                    foreach (string bad in _contains)
                    {
                        if (word.Collapsed.Contains(bad))
                        {
                            caught = true;
                            break;
                        }
                    }
                }

                if (!caught)
                {
                    continue;
                }

                output = output ?? text.ToCharArray();
                foreach (int index in word.Positions)
                {
                    output[index] = MaskChar;
                }

                masked++;
            }

            return new ChatFilterResult(output == null ? text : new string(output), masked);
        }

        /// <summary>
        /// The comparison key of a message for the repetition rule (<see cref="ChatSpamState"/>): the normalized words joined
        /// by spaces, so "OI!!", "oi" and "0i" are the same message.
        /// </summary>
        public static string RepeatKey(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(text.Length);
            foreach (Word word in Words(text))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(word.Collapsed);
            }

            return builder.ToString();
        }

        /// <summary>One normalized word and the indices, in the original text, of the characters it was made of.</summary>
        private sealed class Word
        {
            public string Collapsed;
            public readonly List<int> Positions = new List<int>();
        }

        private static List<Word> Words(string text)
        {
            var raw = new List<Word>();
            var letters = new StringBuilder();
            Word current = null;
            for (int i = 0; i < text.Length; i++)
            {
                char c = Normalize(text[i]);

                // "!" is an "i" only inside a word ("sh!t"); at the end it is just "!" ("que legal!").
                if (text[i] == '!' && (current == null || i + 1 >= text.Length || text[i + 1] == '!'
                    || !IsLetter(Normalize(text[i + 1]))))
                {
                    c = ' ';
                }

                if (IsLetter(c))
                {
                    current = current ?? new Word();
                    letters.Append(c);
                    current.Positions.Add(i);
                    continue;
                }

                // A combining accent written apart from its letter ("á") and an invisible character ("pu​ta") keep the word going.
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(text[i]);
                if (current != null && (category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.Format))
                {
                    current.Positions.Add(i);
                    continue;
                }

                Flush(ref current, letters, raw);
            }

            Flush(ref current, letters, raw);

            // "p u t a" / "f.u.c.k": runs of one-letter words are one word.
            var words = new List<Word>(raw.Count);
            Word run = null;
            var runLetters = new StringBuilder();
            foreach (Word word in raw)
            {
                if (word.Collapsed.Length == 1)
                {
                    run = run ?? new Word();
                    runLetters.Append(word.Collapsed);
                    run.Positions.AddRange(word.Positions);
                    continue;
                }

                Flush(ref run, runLetters, words);
                words.Add(word);
            }

            Flush(ref run, runLetters, words);
            return words;
        }

        private static void Flush(ref Word word, StringBuilder letters, List<Word> into)
        {
            if (word == null)
            {
                return;
            }

            word.Collapsed = Collapse(letters.ToString());
            into.Add(word);
            word = null;
            letters.Clear();
        }

        /// <summary>One character to a–z (lower case, accent removed, leetspeak undone), or something else for a separator.</summary>
        private static char Normalize(char original)
        {
            char c = char.ToLowerInvariant(original);
            if (c > 127)
            {
                string plain = GuildNameRules.RemoveDiacritics(c.ToString());
                c = plain.Length == 1 ? plain[0] : ' ';
            }

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

        private static bool IsLetter(char c) => c >= 'a' && c <= 'z';

        /// <summary>"merdaaa" → "merda": every run of one letter becomes one letter.</summary>
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

        private static void EnsureLists()
        {
            // The guild lists only grow (AddContainsWord / AddWholeWord): a new count means a new word.
            if (_whole == null || _guildWholeCount != GuildNameRules.WholeWords.Count)
            {
                var whole = new HashSet<string>();
                AddNormalized(GuildNameRules.WholeWords, whole);
                AddNormalized(ChatWholeWordList, whole);
                _whole = whole;
                _guildWholeCount = GuildNameRules.WholeWords.Count;
            }

            if (_contains == null || _guildContainsCount != GuildNameRules.ContainsWords.Count)
            {
                var contains = new HashSet<string>();
                AddNormalized(GuildNameRules.ContainsWords, contains);
                AddNormalized(ChatContainsWordList, contains);
                _contains = new List<string>(contains);
                _guildContainsCount = GuildNameRules.ContainsWords.Count;
            }
        }

        private static void AddNormalized(IReadOnlyList<string> source, HashSet<string> into)
        {
            foreach (string entry in source)
            {
                foreach (Word word in Words(entry))
                {
                    into.Add(word.Collapsed);
                }
            }
        }
    }
}
