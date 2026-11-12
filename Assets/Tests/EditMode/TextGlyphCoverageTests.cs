using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Localization;

namespace Rapadura.Tests
{
    /// <summary>
    /// Every en/pt text in the code tables must be drawable by the UI font. UI Toolkit uses the default runtime
    /// theme, whose font is Inter (Regular/SemiBold/Italic); Android has no system fallback fonts, so a glyph
    /// Inter lacks draws as an empty box (it happened with ⇋, ⟲/⟳ and ▶). The allowed set below was checked
    /// against the cmap of Inter-Regular/SemiBold/Italic.ttf shipped with Unity 6000.3: printable ASCII,
    /// Latin-1, Latin Extended-A (minus ŉ) and a short list of symbols in use. To add a symbol, first check
    /// that the three Inter files have it, then add it to <see cref="ExtraAllowed"/>.
    /// Pure: collects the tables by reflection (static entry lists, the <c>EnsureRegistered</c>/<c>EnsureLoaded</c>
    /// loaders into a fresh <see cref="LocalizationManager"/>, and parameterless entry getters).
    /// </summary>
    public class TextGlyphCoverageTests
    {
        /// <summary>Symbols outside the base ranges that Inter has (all verified in its cmap).</summary>
        private const string ExtraAllowed =
            "‘’‚“”„–—…•·‹›′″" +  // punctuation (· and × are Latin-1)
            "←↑→↓↔↕↺" +            // arrows
            "−±≤≥≈≠∞√" +           // maths
            "✓✗◀★☆" +             // marks (▶ is NOT in Inter)
            "ʾʿḫḥṣṭṇṃṛḍẓ" +        // transliteration letters used in names
            " ";

        private static bool IsAllowed(char c)
        {
            if (c == '\n' || c == '\r' || c == '\t') return true;
            if (c >= 0x20 && c <= 0x7E) return true;
            if (c >= 0xA0 && c <= 0xFF) return true;
            if (c >= 0x100 && c <= 0x17F && c != 0x149) return true;
            return ExtraAllowed.IndexOf(c) >= 0;
        }

        [Test]
        public void AllTextTables_UseOnlyGlyphsTheUiFontHas()
        {
            List<LocalizationEntry> entries = CollectEntries();
            Assert.Greater(entries.Count, 2000, "the reflection sweep should find the text tables");

            var problems = new SortedSet<string>(StringComparer.Ordinal);
            foreach (LocalizationEntry entry in entries)
            {
                Check(entry.Key, "en", entry.En, problems);
                Check(entry.Key, "pt", entry.PtBR, problems);
            }

            Assert.IsEmpty(problems, "Glyphs missing from the UI font (Inter):\n" + string.Join("\n", problems.Take(60)));
        }

        private static void Check(string key, string language, string text, ISet<string> problems)
        {
            if (string.IsNullOrEmpty(text)) return;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsSurrogate(c) || !IsAllowed(c))
                {
                    problems.Add($"U+{(int)c:X4} in {key} ({language}): {text.Substring(0, Math.Min(text.Length, 80))}");
                }
            }
        }

        private static List<LocalizationEntry> CollectEntries()
        {
            var result = new List<LocalizationEntry>();
            var manager = new LocalizationManager();
            const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            Type[] types;
            try { types = typeof(LocalizationEntry).Assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            foreach (Type type in types)
            {
                if (type.ContainsGenericParameters) continue;

                foreach (FieldInfo field in type.GetFields(Flags))
                {
                    if (typeof(IEnumerable<LocalizationEntry>).IsAssignableFrom(field.FieldType))
                    {
                        AddAll(result, () => field.GetValue(null));
                    }
                }

                foreach (PropertyInfo property in type.GetProperties(Flags))
                {
                    if (property.GetIndexParameters().Length == 0 && property.GetMethod != null
                        && typeof(IEnumerable<LocalizationEntry>).IsAssignableFrom(property.PropertyType))
                    {
                        AddAll(result, () => property.GetValue(null));
                    }
                }

                foreach (MethodInfo method in type.GetMethods(Flags))
                {
                    if (method.ContainsGenericParameters) continue;
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 0 && typeof(IEnumerable<LocalizationEntry>).IsAssignableFrom(method.ReturnType))
                    {
                        AddAll(result, () => method.Invoke(null, null));
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(LocalizationManager)
                             && method.ReturnType == typeof(void)
                             && (method.Name == "EnsureRegistered" || method.Name == "EnsureLoaded"))
                    {
                        try { method.Invoke(null, new object[] { manager }); }
                        catch (Exception) { /* a loader that needs the running game is covered by its static table */ }
                    }
                }
            }

            FieldInfo store = typeof(LocalizationManager).GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic);
            if (store?.GetValue(manager) is IDictionary dictionary)
            {
                result.AddRange(dictionary.Values.OfType<LocalizationEntry>());
            }

            return result;
        }

        private static void AddAll(List<LocalizationEntry> result, Func<object> read)
        {
            try
            {
                if (read() is IEnumerable<LocalizationEntry> items)
                {
                    result.AddRange(items.Where(e => e != null));
                }
            }
            catch (Exception)
            {
                // A table whose type initializer needs the running game: the loaders above still reach it.
            }
        }
    }
}
