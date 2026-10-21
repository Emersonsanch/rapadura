using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Achievements;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Coverage audit for every localized text source (see Docs/Localization.md): the core
    /// <see cref="LocalizationManager.DefaultEntries"/> table plus the runtime-merged *Texts helpers
    /// (QuestLogTexts, QuestGiverTexts, DailyQuestTexts, AchievementTexts, TooltipLocalization,
    /// SettingsMenuTexts). Checks that every key referenced by ScriptableObject assets or built in
    /// code exists with both en and pt-BR text, that no key is defined twice with different text,
    /// that helpers never shadow a core key, and that {0}-style placeholders match per language.
    /// A new helper must be added to <see cref="Sources"/> or its keys will be reported missing.
    /// </summary>
    public class LocalizationCoverageTests
    {
        private static readonly Regex PlaceholderPattern = new Regex(@"\{\d+(?:[,:][^}]*)?\}");

        private const string CoreSourceName = "LocalizationManager.DefaultEntries";

        private static IEnumerable<KeyValuePair<string, IReadOnlyList<LocalizationEntry>>> Sources()
        {
            yield return Source(CoreSourceName, LocalizationManager.DefaultEntries.All);
            yield return Source(nameof(QuestLogTexts), QuestLogTexts.All);
            yield return Source(nameof(QuestGiverTexts), QuestGiverTexts.All);
            yield return Source(nameof(DailyQuestTexts), DailyQuestTexts.All);
            yield return Source(nameof(AchievementTexts), AchievementTexts.All);
            yield return Source(nameof(TooltipLocalization), TooltipLocalization.Entries);
            yield return Source(nameof(SettingsMenuTexts), SettingsMenuTexts.Entries);
        }

        private static KeyValuePair<string, IReadOnlyList<LocalizationEntry>> Source(string name, IReadOnlyList<LocalizationEntry> entries)
        {
            return new KeyValuePair<string, IReadOnlyList<LocalizationEntry>>(name, entries);
        }

        private static IEnumerable<(string Source, LocalizationEntry Entry)> AllEntries()
        {
            foreach (KeyValuePair<string, IReadOnlyList<LocalizationEntry>> source in Sources())
            {
                foreach (LocalizationEntry entry in source.Value)
                {
                    yield return (source.Key, entry);
                }
            }
        }

        /// <summary>Same view the game ends up with once every helper has merged its entries.</summary>
        private static Dictionary<string, LocalizationEntry> MergedTable()
        {
            var table = new Dictionary<string, LocalizationEntry>();
            foreach (var (_, entry) in AllEntries())
            {
                if (entry != null && !string.IsNullOrEmpty(entry.Key) && !table.ContainsKey(entry.Key))
                {
                    table.Add(entry.Key, entry);
                }
            }

            return table;
        }

        private static void AssertKeysResolve(IEnumerable<(string Key, string Origin)> references)
        {
            Dictionary<string, LocalizationEntry> table = MergedTable();
            var problems = new List<string>();

            foreach ((string key, string origin) in references)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!table.TryGetValue(key, out LocalizationEntry entry))
                {
                    problems.Add($"{key} (missing, used by {origin})");
                }
                else if (string.IsNullOrEmpty(entry.En) || string.IsNullOrEmpty(entry.PtBR))
                {
                    problems.Add($"{key} (no {(string.IsNullOrEmpty(entry.En) ? "en" : "pt-BR")} text, used by {origin})");
                }
            }

            CollectionAssert.IsEmpty(problems.Distinct().ToList(), "Unresolved localization keys:\n" + string.Join("\n", problems.Distinct()));
        }

        private static IEnumerable<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets" }))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        private static string Origin(UnityEngine.Object asset) => AssetDatabase.GetAssetPath(asset);

        // ------------------------------------------------------------------
        // Table integrity
        // ------------------------------------------------------------------

        [Test]
        public void EveryEntry_HasKeyAndBothLanguages()
        {
            var problems = new List<string>();
            foreach ((string source, LocalizationEntry entry) in AllEntries())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    problems.Add($"{source}: entry with empty key");
                    continue;
                }

                if (string.IsNullOrEmpty(entry.En))
                {
                    problems.Add($"{source}: {entry.Key} has no en text");
                }

                if (string.IsNullOrEmpty(entry.PtBR))
                {
                    problems.Add($"{source}: {entry.Key} has no pt-BR text");
                }
            }

            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void NoKey_IsDefinedTwiceWithConflictingText()
        {
            var problems = new List<string>();
            foreach (IGrouping<string, (string Source, LocalizationEntry Entry)> group in AllEntries()
                         .Where(e => e.Entry != null && !string.IsNullOrEmpty(e.Entry.Key))
                         .GroupBy(e => e.Entry.Key))
            {
                var definitions = group.ToList();
                if (definitions.Count < 2)
                {
                    continue;
                }

                bool conflicting = definitions.Any(d => d.Entry.En != definitions[0].Entry.En || d.Entry.PtBR != definitions[0].Entry.PtBR);
                if (conflicting)
                {
                    problems.Add($"{group.Key}: {string.Join(", ", definitions.Select(d => d.Source))}");
                }
            }

            CollectionAssert.IsEmpty(problems, "Keys defined more than once with different text:\n" + string.Join("\n", problems));
        }

        [Test]
        public void Helpers_NeverRedefineCoreKeys()
        {
            var coreKeys = new HashSet<string>(LocalizationManager.DefaultEntries.All.Select(e => e.Key));
            var problems = AllEntries()
                .Where(e => e.Source != CoreSourceName && e.Entry != null && coreKeys.Contains(e.Entry.Key))
                .Select(e => $"{e.Source}: {e.Entry.Key}")
                .ToList();

            CollectionAssert.IsEmpty(problems, "Helper entries shadowing LocalizationManager.DefaultEntries:\n" + string.Join("\n", problems));
        }

        [Test]
        public void Placeholders_MatchBetweenLanguages()
        {
            var problems = new List<string>();
            foreach ((string source, LocalizationEntry entry) in AllEntries())
            {
                if (entry == null || string.IsNullOrEmpty(entry.En) || string.IsNullOrEmpty(entry.PtBR))
                {
                    continue;
                }

                string en = string.Join(" ", PlaceholderPattern.Matches(entry.En).Cast<Match>().Select(m => m.Value).Distinct().OrderBy(v => v, StringComparer.Ordinal));
                string pt = string.Join(" ", PlaceholderPattern.Matches(entry.PtBR).Cast<Match>().Select(m => m.Value).Distinct().OrderBy(v => v, StringComparer.Ordinal));
                if (en != pt)
                {
                    problems.Add($"{source}: {entry.Key} en=[{en}] pt-BR=[{pt}]");
                }
            }

            CollectionAssert.IsEmpty(problems, "Placeholder mismatch:\n" + string.Join("\n", problems));
        }

        [Test]
        public void RuntimeMerge_HelpersDoNotOverrideExistingKeys()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.ptBR);

            // Simulate an external CSV/LocalizationTable that already defines one key of every helper.
            var external = new Dictionary<string, string>();
            foreach (KeyValuePair<string, IReadOnlyList<LocalizationEntry>> source in Sources().Where(s => s.Key != CoreSourceName))
            {
                LocalizationEntry first = source.Value.First();
                external[first.Key] = "EXTERNAL";
            }

            manager.LoadEntries(external.Select(pair => new LocalizationEntry(pair.Key, pair.Value, pair.Value)));

            QuestLogTexts.EnsureLoaded(manager);
            QuestGiverTexts.EnsureLoaded(manager);
            DailyQuestTexts.EnsureLoaded(manager);
            AchievementTexts.EnsureLoaded(manager);
            TooltipLocalization.EnsureRegistered(manager);
            SettingsMenuTexts.EnsureRegistered(manager);

            foreach (KeyValuePair<string, string> pair in external)
            {
                Assert.AreEqual(pair.Value, manager.Get(pair.Key), $"A helper overwrote externally-defined key '{pair.Key}'.");
            }

            foreach (LocalizationEntry entry in LocalizationManager.DefaultEntries.All)
            {
                Assert.AreEqual(entry.PtBR, manager.Get(entry.Key), $"A helper overwrote core key '{entry.Key}'.");
            }

            // Every helper key is still registered after the merge, even though one per helper pre-existed.
            foreach ((string source, LocalizationEntry entry) in AllEntries())
            {
                Assert.IsTrue(manager.HasKey(entry.Key), $"{source}: '{entry.Key}' was not merged.");
            }
        }

        // ------------------------------------------------------------------
        // Keys referenced by assets
        // ------------------------------------------------------------------

        [Test]
        public void QuestAssets_AllKeysLocalized()
        {
            var references = new List<(string, string)>();
            foreach (QuestDefinition quest in LoadAssets<QuestDefinition>())
            {
                references.Add((quest.TitleKey, Origin(quest)));
                references.Add((quest.DescriptionKey, Origin(quest)));
                foreach (QuestObjective objective in quest.Objectives)
                {
                    if (objective != null)
                    {
                        references.Add((objective.DescriptionKey, Origin(quest)));
                    }
                }
            }

            Assert.IsNotEmpty(references, "No QuestDefinition assets found.");
            AssertKeysResolve(references);
        }

        [Test]
        public void DialogueAssets_AllKeysLocalized()
        {
            var references = new List<(string, string)>();
            foreach (DialogueDefinition dialogue in LoadAssets<DialogueDefinition>())
            {
                foreach (DialogueNode node in dialogue.Nodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    references.Add((node.TextKey, $"{Origin(dialogue)}#{node.Id}"));
                    references.Add((node.SpeakerNameKey, $"{Origin(dialogue)}#{node.Id}"));
                    foreach (DialogueChoice choice in node.Choices)
                    {
                        if (choice != null)
                        {
                            references.Add((choice.TextKey, $"{Origin(dialogue)}#{node.Id}"));
                        }
                    }
                }
            }

            Assert.IsNotEmpty(references, "No DialogueDefinition assets found.");
            AssertKeysResolve(references);
        }

        [Test]
        public void AchievementAssets_AllKeysLocalized()
        {
            var references = new List<(string, string)>();
            foreach (AchievementDefinition achievement in LoadAssets<AchievementDefinition>())
            {
                references.Add((achievement.TitleKey, Origin(achievement)));
                references.Add((achievement.DescriptionKey, Origin(achievement)));
            }

            Assert.IsNotEmpty(references, "No AchievementDefinition assets found.");
            AssertKeysResolve(references);
        }

        [Test]
        public void ShopBiomeAndAffixAssets_AllKeysLocalized()
        {
            var references = new List<(string, string)>();
            references.AddRange(LoadAssets<ShopDefinition>().Select(a => (a.DisplayNameKey, Origin(a))));
            references.AddRange(LoadAssets<BiomeDefinition>().Select(a => (a.NameLocalizationKey, Origin(a))));
            references.AddRange(LoadAssets<ItemAffixDefinition>().Select(a => (a.NameLocalizationKey, Origin(a))));

            AssertKeysResolve(references);
        }

        // ------------------------------------------------------------------
        // Keys built in code
        // ------------------------------------------------------------------

        [Test]
        public void HelperKeyConstants_AllLocalized()
        {
            var references = new List<(string, string)>();
            foreach (Type type in new[] { typeof(QuestLogTexts), typeof(QuestLogPresenter), typeof(AchievementTexts), typeof(SettingsMenuTexts) })
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!field.IsLiteral || field.FieldType != typeof(string))
                    {
                        continue;
                    }

                    string value = (string)field.GetRawConstantValue();
                    if (value.StartsWith("ui.", StringComparison.Ordinal) && !value.EndsWith(".", StringComparison.Ordinal))
                    {
                        references.Add((value, $"{type.Name}.{field.Name}"));
                    }
                }
            }

            references.AddRange(SettingsMenuTexts.Rows.Select(row => (row.LabelKey, $"SettingsMenuTexts.Rows[{row.ActionName}]")));
            foreach (ColorblindMode mode in Enum.GetValues(typeof(ColorblindMode)))
            {
                references.Add((SettingsMenuTexts.ColorblindKey(mode), "SettingsMenuTexts.ColorblindKey"));
            }

            AssertKeysResolve(references);
        }

        [Test]
        public void TooltipEnumKeys_AllLocalized()
        {
            var references = new List<(string, string)>();
            AddEnumKeys<ItemRarity>(references, "ui.tooltip.rarity.");
            AddEnumKeys<ItemType>(references, "ui.tooltip.type.");
            AddEnumKeys<EquipmentSlot>(references, "ui.tooltip.slot.", "None");
            AddEnumKeys<StatType>(references, "ui.tooltip.stat.");
            AddEnumKeys<ElementType>(references, "ui.tooltip.element.", "None");

            AssertKeysResolve(references);
        }

        private static void AddEnumKeys<TEnum>(List<(string, string)> references, string prefix, params string[] skipped) where TEnum : Enum
        {
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                string name = value.ToString();
                if (!skipped.Contains(name))
                {
                    references.Add((prefix + TooltipLocalization.ToSnakeCase(name), $"{typeof(TEnum).Name}.{name}"));
                }
            }
        }

        // ------------------------------------------------------------------
        // Startup language
        // ------------------------------------------------------------------

        [TestCase(SystemLanguage.Portuguese, LanguageCode.ptBR)]
        [TestCase(SystemLanguage.Spanish, LanguageCode.ptBR)]
        [TestCase(SystemLanguage.Unknown, LanguageCode.ptBR)]
        [TestCase(SystemLanguage.English, LanguageCode.en)]
        public void StartupLanguage_IsPtBRUnlessDeviceIsEnglish(SystemLanguage system, LanguageCode expected)
        {
            Assert.AreEqual(expected, LocalizationManager.ResolveStartupLanguage(system));
        }
    }
}
