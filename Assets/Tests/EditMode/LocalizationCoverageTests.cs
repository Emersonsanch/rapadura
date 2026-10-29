using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.Tutorial;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Achievements;
using Rapadura.UI.CharacterSheet;
using Rapadura.UI.Common;
using Rapadura.UI.Crafting;
using Rapadura.UI.Encounter;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using Rapadura.UI.Tutorial;
using Rapadura.UI.WorldMap;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// Coverage audit for every localized text source (see Docs/Localization.md): the core
    /// <see cref="LocalizationManager.DefaultEntries"/> table plus the runtime-merged *Texts helpers
    /// (QuestLogTexts, QuestGiverTexts, DailyQuestTexts, AchievementTexts, TooltipLocalization,
    /// SettingsMenuTexts, UiTexts, ReturnToTitleTexts, ContentTexts, TutorialTexts, HapticsTexts, TouchControlsTexts, CharacterSheetTexts). Checks that every key referenced by ScriptableObject assets or built in
    /// code exists with both en and pt-BR text, that no key is defined twice with different text,
    /// that helpers never shadow a core key, and that {0}-style placeholders match per language.
    /// Also covers every <c>class="loc"</c> + <c>view-data-key</c> marker in Assets/UI/**/*.uxml
    /// (see <see cref="UiDocumentLocalizer"/>) and the persisted-language startup rule.
    /// A new helper must be added to <see cref="Sources"/> or its keys will be reported missing.
    /// </summary>
    public class LocalizationCoverageTests
    {
        private static readonly Regex PlaceholderPattern = new Regex(@"\{\d+(?:[,:][^}]*)?\}");

        private const string CoreSourceName = "LocalizationManager.DefaultEntries";

        /// <summary>
        /// One localized text source: its entries plus the runtime merge it performs into a live
        /// <see cref="LocalizationManager"/> (null for the core table, which the manager owns).
        /// Registering the merge here means <see cref="RuntimeMerge_HelpersDoNotOverrideExistingKeys"/>
        /// automatically exercises every helper listed in <see cref="Sources"/>.
        /// </summary>
        private sealed class TextSource
        {
            public readonly string Name;
            public readonly IReadOnlyList<LocalizationEntry> Entries;
            public readonly Action<LocalizationManager> Merge;

            public TextSource(string name, IReadOnlyList<LocalizationEntry> entries, Action<LocalizationManager> merge)
            {
                Name = name;
                Entries = entries;
                Merge = merge;
            }
        }

        private static IEnumerable<TextSource> Sources()
        {
            yield return Source(CoreSourceName, LocalizationManager.DefaultEntries.All, null);
            yield return Source(nameof(QuestLogTexts), QuestLogTexts.All, QuestLogTexts.EnsureLoaded);
            yield return Source(nameof(QuestGiverTexts), QuestGiverTexts.All, QuestGiverTexts.EnsureLoaded);
            yield return Source(nameof(RegionQuestTexts), RegionQuestTexts.All, RegionQuestTexts.EnsureLoaded);
            yield return Source(nameof(DailyQuestTexts), DailyQuestTexts.All, DailyQuestTexts.EnsureLoaded);
            yield return Source(nameof(AchievementTexts), AchievementTexts.All, AchievementTexts.EnsureLoaded);
            yield return Source(nameof(TooltipLocalization), TooltipLocalization.Entries, TooltipLocalization.EnsureRegistered);
            yield return Source(nameof(SettingsMenuTexts), SettingsMenuTexts.Entries, SettingsMenuTexts.EnsureRegistered);
            yield return Source(nameof(UiTexts), UiTexts.Entries, UiTexts.EnsureRegistered);
            yield return Source(nameof(ReturnToTitleTexts), ReturnToTitleTexts.Entries, ReturnToTitleTexts.EnsureRegistered);
            yield return Source(nameof(WorldMapTexts), WorldMapTexts.Entries, WorldMapTexts.EnsureRegistered);
            yield return Source(nameof(ContentTexts), ContentTexts.Entries, ContentTexts.EnsureRegistered);
            yield return Source(nameof(TutorialTexts), TutorialTexts.Entries, TutorialTexts.EnsureRegistered);
            yield return Source(nameof(HapticsTexts), HapticsTexts.Entries, HapticsTexts.EnsureRegistered);
            yield return Source(nameof(TouchControlsTexts), TouchControlsTexts.Entries, TouchControlsTexts.EnsureRegistered);
            yield return Source(nameof(CharacterSheetTexts), CharacterSheetTexts.Entries, CharacterSheetTexts.EnsureRegistered);
            yield return Source(nameof(BossTexts), BossTexts.Entries, BossTexts.EnsureRegistered);
            yield return Source(nameof(StoryTexts), StoryTexts.Entries, StoryTexts.EnsureRegistered);
        }

        private static TextSource Source(string name, IReadOnlyList<LocalizationEntry> entries, Action<LocalizationManager> merge)
        {
            return new TextSource(name, entries, merge);
        }

        private static IEnumerable<(string Source, LocalizationEntry Entry)> AllEntries()
        {
            foreach (TextSource source in Sources())
            {
                foreach (LocalizationEntry entry in source.Entries)
                {
                    yield return (source.Name, entry);
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
            foreach (TextSource source in Sources().Where(s => s.Name != CoreSourceName))
            {
                Assert.IsNotNull(source.Merge, $"{source.Name} has no runtime merge registered in Sources().");
                LocalizationEntry first = source.Entries.First();
                external[first.Key] = "EXTERNAL";
            }

            manager.LoadEntries(external.Select(pair => new LocalizationEntry(pair.Key, pair.Value, pair.Value)));

            // Driven by the same registry as the coverage checks, so a helper added to Sources()
            // is merged here too (no hand-maintained list to forget).
            foreach (TextSource source in Sources().Where(s => s.Merge != null))
            {
                source.Merge(manager);
            }

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
        // ScriptableObject content (ContentText / ContentTexts)
        // ------------------------------------------------------------------

        private const string ResourcesRoot = "Assets/Resources/";

        /// <summary>Every (key, asset pt-BR text, origin) the UI resolves through <see cref="ContentText"/>.</summary>
        private static IEnumerable<(string Key, string PtBR, string Origin)> ContentReferences()
        {
            foreach (ItemDefinition item in ResourceAssets<ItemDefinition>("Items"))
            {
                yield return (ContentText.ItemNameKey(item.ItemId), item.DisplayName, Origin(item));
                if (!string.IsNullOrEmpty(item.Description)) yield return (ContentText.ItemDescriptionKey(item.ItemId), item.Description, Origin(item));
            }

            foreach (SkillDefinition skill in ResourceAssets<SkillDefinition>("Skills"))
            {
                yield return (ContentText.SkillNameKey(skill.SkillId), skill.DisplayName, Origin(skill));
                if (!string.IsNullOrEmpty(skill.Description)) yield return (ContentText.SkillDescriptionKey(skill.SkillId), skill.Description, Origin(skill));
            }

            foreach (EnemyDefinition enemy in ResourceAssets<EnemyDefinition>("Enemies"))
            {
                yield return (ContentText.EnemyNameKey(enemy.name), enemy.EnemyName, Origin(enemy));
            }

            foreach (RecipeDefinition recipe in ResourceAssets<RecipeDefinition>("Recipes"))
            {
                yield return (ContentText.RecipeNameKey(recipe.RecipeId), recipe.DisplayName, Origin(recipe));
                if (!string.IsNullOrEmpty(recipe.Description)) yield return (ContentText.RecipeDescriptionKey(recipe.RecipeId), recipe.Description, Origin(recipe));
            }

            foreach (ComboDefinition combo in ResourceAssets<ComboDefinition>("Combos"))
            {
                yield return (ContentText.ComboNameKey(combo.ComboId), combo.DisplayName, Origin(combo));
                if (!string.IsNullOrEmpty(combo.Description)) yield return (ContentText.ComboDescriptionKey(combo.ComboId), combo.Description, Origin(combo));
            }

            foreach (SkillTreeDefinition tree in ResourceAssets<SkillTreeDefinition>("SkillTrees"))
            {
                yield return (ContentText.TreeNameKey(tree.TreeId), tree.DisplayName, Origin(tree));
                if (!string.IsNullOrEmpty(tree.Description)) yield return (ContentText.TreeDescriptionKey(tree.TreeId), tree.Description, Origin(tree));
            }

            foreach (PlayableCharacter character in CharacterRegistry.GetAll())
            {
                string origin = $"Gameplay/Characters/{character.Id}.cs";
                yield return (ContentText.CharacterNameKey(character.Id), character.DisplayName, origin);
                yield return (ContentText.CharacterClassKey(character.Id), character.ClassName, origin);
                yield return (ContentText.CharacterRoleKey(character.Id), character.Role, origin);
                yield return (ContentText.CharacterLoreKey(character.Id), character.Lore, origin);
            }
        }

        private static IEnumerable<T> ResourceAssets<T>(string folder) where T : UnityEngine.Object
        {
            return LoadAssets<T>().Where(a => Origin(a).StartsWith(ResourcesRoot + folder + "/", StringComparison.Ordinal));
        }

        [Test]
        public void ContentAssets_AllHaveEnAndPtBR()
        {
            List<(string Key, string PtBR, string Origin)> references = ContentReferences().ToList();
            Assert.Greater(references.Count, 100, "Content assets were not found under Assets/Resources.");
            AssertKeysResolve(references.Select(r => (r.Key, r.Origin)));
        }

        [Test]
        public void ContentTexts_PtBRMatchesAssetText()
        {
            Dictionary<string, LocalizationEntry> table = ContentTexts.Entries.ToDictionary(e => e.Key);
            var problems = new List<string>();
            foreach ((string key, string ptBR, string origin) in ContentReferences())
            {
                if (table.TryGetValue(key, out LocalizationEntry entry) && entry.PtBR != ptBR)
                {
                    problems.Add($"{key} ({origin}):\n  asset:        {ptBR}\n  ContentTexts: {entry.PtBR}");
                }
            }

            CollectionAssert.IsEmpty(problems, "ContentTexts pt-BR drifted from the asset text (update both together):\n" + string.Join("\n", problems));
        }

        [Test]
        public void ContentTexts_HasNoOrphanKeys()
        {
            var used = new HashSet<string>(ContentReferences().Select(r => r.Key));
            List<string> orphans = ContentTexts.Entries.Select(e => e.Key).Where(k => !used.Contains(k)).ToList();
            CollectionAssert.IsEmpty(orphans, "ContentTexts keys with no matching asset (renamed or deleted?):\n" + string.Join("\n", orphans));
        }

        [Test]
        public void ContentText_ResolvesByLanguageWithAssetFallback()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            ItemDefinition sword = ResourceAssets<ItemDefinition>("Items").Single(i => i.ItemId == "item_iron_sword");
            PlayableCharacter icaro = CharacterRegistry.Get(CharacterId.Icaro);

            manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Iron Sword", sword.LocalizedName(manager));
            Assert.AreEqual("Huntress", CharacterRegistry.Get(CharacterId.Maithe).LocalizedClassName(manager));
            StringAssert.StartsWith("At 11, Ícaro", icaro.LocalizedLore(manager));

            manager.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual(sword.DisplayName, sword.LocalizedName(manager));
            Assert.AreEqual(icaro.Lore, icaro.LocalizedLore(manager));

            Assert.AreEqual(sword.DisplayName, sword.LocalizedName(null), "no manager -> asset pt-BR text");

            // An asset without an entry keeps showing its own text in any language.
            var unknown = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                manager.SetLanguage(LanguageCode.en);
                Assert.AreEqual(unknown.DisplayName, unknown.LocalizedName(manager));
                Assert.IsFalse(manager.HasKey(ContentText.ItemNameKey(unknown.ItemId)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(unknown);
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
            foreach (Type type in new[] { typeof(QuestLogTexts), typeof(QuestLogPresenter), typeof(AchievementTexts), typeof(SettingsMenuTexts), typeof(UiTexts), typeof(WorldMapTexts), typeof(TutorialTexts), typeof(HapticsTexts), typeof(CharacterSheetTexts), typeof(BossTexts) })
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

            foreach (LanguageCode language in Enum.GetValues(typeof(LanguageCode)))
            {
                references.Add((SettingsMenuTexts.LanguageNameKey(language), "SettingsMenuTexts.LanguageNameKey"));
            }

            foreach (TutorialStepDefinition step in TutorialSteps.All)
            {
                references.Add((step.TitleKey, $"TutorialSteps[{step.Id}].TitleKey"));
                references.Add((step.BodyKey, $"TutorialSteps[{step.Id}].BodyKey"));
            }

            foreach (Rapadura.Gameplay.Player.AttributeType attribute in Enum.GetValues(typeof(Rapadura.Gameplay.Player.AttributeType)))
            {
                references.Add((CharacterSheetTexts.AttributeNameKey(attribute), "CharacterSheetTexts.AttributeNameKey"));
                references.Add((CharacterSheetTexts.PreviewKey(attribute), "CharacterSheetTexts.PreviewKey"));
            }

            foreach (CraftingStationType station in Enum.GetValues(typeof(CraftingStationType)))
            {
                references.Add((CraftingRecipePresenter.StationKey(station), "CraftingRecipePresenter.StationKey"));
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
        // UXML markers (UiDocumentLocalizer convention)
        // ------------------------------------------------------------------

        private static readonly Regex UxmlElementPattern = new Regex(@"<ui:(\w+)\b([^>]*)>");
        private static readonly Regex ClassAttributePattern = new Regex(@"\bclass=""([^""]*)""");
        private static readonly Regex ViewDataKeyAttributePattern = new Regex(@"\bview-data-key=""([^""]*)""");

        private static IEnumerable<(string Path, string Tag, string Classes, string Key)> UxmlElements()
        {
            foreach (string path in Directory.GetFiles("Assets/UI", "*.uxml", SearchOption.AllDirectories))
            {
                foreach (Match match in UxmlElementPattern.Matches(File.ReadAllText(path)))
                {
                    string attributes = match.Groups[2].Value;
                    Match classes = ClassAttributePattern.Match(attributes);
                    Match key = ViewDataKeyAttributePattern.Match(attributes);
                    yield return (path.Replace('\\', '/'), match.Groups[1].Value,
                        classes.Success ? classes.Groups[1].Value : string.Empty,
                        key.Success ? key.Groups[1].Value : string.Empty);
                }
            }
        }

        private static bool HasLocClass(string classes)
        {
            return classes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(UiDocumentLocalizer.LocClass);
        }

        [Test]
        public void UxmlLocMarkers_AllKeysLocalized()
        {
            var references = new List<(string, string)>();
            var problems = new List<string>();
            foreach (var element in UxmlElements().Where(e => HasLocClass(e.Classes)))
            {
                if (string.IsNullOrEmpty(element.Key))
                {
                    problems.Add($"{element.Path}: <ui:{element.Tag}> has class \"loc\" but no view-data-key");
                    continue;
                }

                references.Add((element.Key, $"{element.Path} <ui:{element.Tag}>"));
            }

            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
            Assert.IsNotEmpty(references, "No class=\"loc\" markers found under Assets/UI.");
            AssertKeysResolve(references);
        }

        [Test]
        public void UxmlLocMarkers_CoverTheConvertedScreens()
        {
            string[] converted =
            {
                "Assets/UI/HUD/HudView.uxml",
                "Assets/UI/Shop/ShopView.uxml",
                "Assets/UI/Menus/PauseMenuView.uxml",
                "Assets/UI/Menus/MainMenuView.uxml",
                "Assets/UI/Menus/SettingsMenuView.uxml",
                "Assets/UI/Menus/SaveLoadMenuView.uxml",
                "Assets/UI/Menus/CreditsView.uxml",
                "Assets/UI/Crafting/CraftingView.uxml",
                "Assets/UI/SkillTree/SkillTreeView.uxml",
                "Assets/UI/Death/DeathScreenView.uxml",
                "Assets/UI/CharacterSelect/CharacterSelectView.uxml",
                "Assets/UI/WorldMap/WorldMapHudView.uxml",
            };

            var marked = new HashSet<string>(UxmlElements().Where(e => HasLocClass(e.Classes)).Select(e => e.Path));
            foreach (string path in converted)
            {
                Assert.IsTrue(marked.Contains(path), $"{path} has no class=\"loc\" markers.");
            }

            // The Shop used to ship English literals in a pt-BR game.
            string shop = File.ReadAllText("Assets/UI/Shop/ShopView.uxml");
            foreach (string literal in new[] { "\"Gold:\"", "\"Buy\"", "\"Sell\"" })
            {
                StringAssert.DoesNotContain(literal, shop);
            }
        }

        [Test]
        public void UxmlLocMarkers_UseTheExpectedKeys()
        {
            var keysByPath = UxmlElements()
                .Where(e => HasLocClass(e.Classes))
                .GroupBy(e => e.Path)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(e => e.Key)));

            // The title screen has both "Novo jogo" and "Continuar": the first button must not reuse the
            // core ui.menu.start ("Iniciar Jogo"), which stays as it is.
            CollectionAssert.IsSubsetOf(new[] { UiTexts.MenuNewGame, UiTexts.MenuContinue }, keysByPath["Assets/UI/Menus/MainMenuView.uxml"]);
            CollectionAssert.DoesNotContain(keysByPath["Assets/UI/Menus/MainMenuView.uxml"], UiTexts.MenuStart);
            StringAssert.Contains("text=\"Novo jogo\"", File.ReadAllText("Assets/UI/Menus/MainMenuView.uxml"));
            Assert.AreEqual("Iniciar Jogo", LocalizationManager.DefaultEntries.All.Single(e => e.Key == UiTexts.MenuStart).PtBR);

            CollectionAssert.IsSubsetOf(
                new[] { UiTexts.DeathTitle, UiTexts.DeathMessage, UiTexts.DeathContinue, UiTexts.DeathLoad },
                keysByPath["Assets/UI/Death/DeathScreenView.uxml"]);
            CollectionAssert.IsSubsetOf(
                new[] { UiTexts.CharacterSelectTitle, UiTexts.CharacterSelectConfirm },
                keysByPath["Assets/UI/CharacterSelect/CharacterSelectView.uxml"]);
        }

        [Test]
        public void ConvertedControllers_HaveNoHardcodedScreenText()
        {
            var forbidden = new Dictionary<string, string[]>
            {
                { "Assets/Scripts/UI/Death/DeathScreenController.cs", new[] { "$\"Continuando em" } },
                { "Assets/Scripts/UI/CharacterSelect/CharacterSelectController.cs", new[] { "$\"Continuar com" } },
            };

            // Interpolated literals only: doc comments may still quote the pt-BR text.
            foreach (KeyValuePair<string, string[]> file in forbidden)
            {
                string source = File.ReadAllText(file.Key);
                foreach (string literal in file.Value)
                {
                    StringAssert.DoesNotContain(literal, source, $"{file.Key} should build this text from UiTexts.");
                }
            }
        }

        [Test]
        public void CreditsSectionTitles_AreKeyedWithPtBRFallback()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);

            foreach (CreditsSection section in CreditsContent.Sections)
            {
                Assert.IsFalse(string.IsNullOrEmpty(section.TitleKey), $"Credits section \"{section.Title}\" has no TitleKey.");
                Assert.AreEqual(section.Title, UiTexts.GetFallback(section.TitleKey), $"{section.TitleKey}: pt-BR text must match the authored heading.");
                Assert.AreEqual(section.Title, section.GetTitle(null), "no manager -> pt-BR heading");
                Assert.AreNotEqual(section.Title, section.GetTitle(manager), $"{section.TitleKey} is not translated to English.");
            }

            Assert.AreEqual("Team", CreditsContent.TeamSection.GetTitle(manager));
        }

        [Test]
        public void UiDocumentLocalizer_AppliesKeysToTextAndFieldLabels()
        {
            var root = new VisualElement();
            var label = new Label("Comprar") { viewDataKey = UiTexts.ShopBuy };
            label.AddToClassList(UiDocumentLocalizer.LocClass);
            var toggle = new Toggle("Tela Cheia") { viewDataKey = SettingsMenuTexts.GraphicsFullscreen };
            toggle.AddToClassList(UiDocumentLocalizer.LocClass);
            var unmarked = new Label("Fixo") { viewDataKey = UiTexts.ShopSell };
            var unknown = new Button { text = "Texto", viewDataKey = "ui.test.unknown_key" };
            unknown.AddToClassList(UiDocumentLocalizer.LocClass);
            root.Add(label);
            root.Add(toggle);
            root.Add(unmarked);
            root.Add(unknown);

            // No manager: the UXML pt-BR text stays.
            Assert.AreEqual(0, UiDocumentLocalizer.Apply(root, null));
            Assert.AreEqual("Comprar", label.text);

            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            UiDocumentLocalizer.Apply(root, manager, SettingsMenuTexts.Get);
            UiDocumentLocalizer.Apply(root, manager);

            Assert.AreEqual("Buy", label.text);
            Assert.AreEqual("Fullscreen", toggle.label);
            Assert.AreEqual("Fixo", unmarked.text, "elements without class \"loc\" are left alone");
            Assert.AreEqual("Texto", unknown.text, "an unknown key keeps the UXML text instead of showing [key]");

            manager.SetLanguage(LanguageCode.ptBR);
            UiDocumentLocalizer.Apply(root, manager);
            Assert.AreEqual("Comprar", label.text);
        }

        // ------------------------------------------------------------------
        // No-manager fallbacks (pt-BR everywhere)
        // ------------------------------------------------------------------

        [Test]
        public void NoManagerFallbacks_ArePtBR()
        {
            Assert.AreEqual("Raro", TooltipLocalization.GetFallback("ui.tooltip.rarity.rare"));
            Assert.AreEqual("Comprar", UiTexts.Get(null, UiTexts.ShopBuy));
            Assert.AreEqual("Pausado", UiTexts.Get(null, UiTexts.PauseTitle), "core ui.* keys also fall back to pt-BR");
            Assert.AreEqual("Nv. 3", UiTexts.Format(null, UiTexts.HudLevel, 3));
            Assert.AreEqual("Novo jogo", UiTexts.Get(null, UiTexts.MenuNewGame));
            Assert.AreEqual("Continuando em 5s", UiTexts.Format(null, UiTexts.DeathCountdown, 5));
            Assert.AreEqual("Voltar ao mapa", UiTexts.Get(null, UiTexts.DeathReturnToMap));
            Assert.AreEqual("Continuar com Maithe", UiTexts.Format(null, UiTexts.CharacterSelectContinueWith, "Maithe"));
            Assert.AreEqual("Português (Brasil)", SettingsMenuTexts.LanguageName(null, LanguageCode.ptBR));
            Assert.AreEqual("English", SettingsMenuTexts.LanguageName(null, LanguageCode.en));

            // Presenter constants are the pt-BR text of their keys (EditMode tests compare against them).
            Assert.AreEqual(CraftingRecipePresenter.UnknownRecipeReason, UiTexts.GetFallback(UiTexts.CraftingUnknownRecipe));
            Assert.AreEqual(CraftingRecipePresenter.MissingMaterialsReason, UiTexts.GetFallback(UiTexts.CraftingMissingMaterials));
            Assert.AreEqual(CraftingRecipePresenter.NoInventoryReason, UiTexts.GetFallback(UiTexts.CraftingNoInventory));
        }

        [Test]
        public void Presenters_UseManagerLanguageWhenGiven()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);

            Assert.AreEqual("Slot 2", SaveSlotPresenter.GetSlotTitle(1, manager));
            Assert.AreEqual("Overwrite", SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode: true, slotExists: true, localization: manager));
            Assert.AreEqual("Requires: Forge", CraftingRecipePresenter.FormatStationRequirement(CraftingStationType.Forge, manager));
            Assert.AreEqual("Requer: Forja", CraftingRecipePresenter.FormatStationRequirement(CraftingStationType.Forge));
            Assert.AreEqual("New game", UiTexts.Get(manager, UiTexts.MenuNewGame));
            Assert.AreEqual("Continuing in 5s", UiTexts.Format(manager, UiTexts.DeathCountdown, 5));
            Assert.AreEqual("Continue as Maithe", UiTexts.Format(manager, UiTexts.CharacterSelectContinueWith, "Maithe"));
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
            Assert.AreEqual(expected, LocalizationManager.ResolveStartupLanguage(system, null), "no saved choice -> system rule");
            Assert.AreEqual(expected, LocalizationManager.ResolveStartupLanguage(system, "xx"), "garbage saved value -> system rule");
        }

        [TestCase(SystemLanguage.English, "ptBR", LanguageCode.ptBR)]
        [TestCase(SystemLanguage.Portuguese, "en", LanguageCode.en)]
        [TestCase(SystemLanguage.Spanish, "en", LanguageCode.en)]
        [TestCase(SystemLanguage.English, "en", LanguageCode.en)]
        public void StartupLanguage_PersistedChoiceWinsOverSystemLanguage(SystemLanguage system, string saved, LanguageCode expected)
        {
            Assert.AreEqual(expected, LocalizationManager.ResolveStartupLanguage(system, saved));
        }

        [Test]
        public void SaveLanguagePreference_RoundTripsThroughPlayerPrefs()
        {
            bool hadValue = PlayerPrefs.HasKey(LocalizationManager.LanguagePrefsKey);
            string previous = PlayerPrefs.GetString(LocalizationManager.LanguagePrefsKey);
            try
            {
                LocalizationManager.SaveLanguagePreference(LanguageCode.en);
                Assert.AreEqual(LanguageCode.en,
                    LocalizationManager.ResolveStartupLanguage(SystemLanguage.Portuguese, LocalizationManager.LoadSavedLanguage()));

                LocalizationManager.SaveLanguagePreference(LanguageCode.ptBR);
                Assert.AreEqual(LanguageCode.ptBR,
                    LocalizationManager.ResolveStartupLanguage(SystemLanguage.English, LocalizationManager.LoadSavedLanguage()));
            }
            finally
            {
                if (hadValue)
                {
                    PlayerPrefs.SetString(LocalizationManager.LanguagePrefsKey, previous);
                }
                else
                {
                    PlayerPrefs.DeleteKey(LocalizationManager.LanguagePrefsKey);
                }

                PlayerPrefs.Save();
            }
        }
    }
}
