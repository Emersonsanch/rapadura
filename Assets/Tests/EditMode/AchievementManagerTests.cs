using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Achievements;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Achievements system: event-driven counter and id-list unlocks, the
    /// player-only filter on skill casts, distinct region counting, unlock-once semantics, the
    /// per-profile file (Saves/profile.json under <see cref="TestSaveRoot"/>) and the one-time merge of
    /// achievements found in legacy save slots (slot 90041), retroactive unlocks for definitions added after a save, hidden-entry masking in the presenter,
    /// and sanity checks over the seeded <c>Resources/Achievements</c> catalog and its localization.
    /// </summary>
    public class AchievementManagerTests
    {
        private const int TestSlot = 90041;

        private readonly List<Object> _created = new List<Object>();
        private readonly List<AchievementManager> _managers = new List<AchievementManager>();
        private readonly List<string> _unlockedEvents = new List<string>();

        private static string SlotPath(int slot) => TestSaveRoot.SlotPath(slot);

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            DeleteSlotFiles();
            _unlockedEvents.Clear();
            EventBus.Subscribe<AchievementUnlockedEvent>(OnUnlocked);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (AchievementManager manager in _managers)
            {
                manager.Shutdown();
            }

            _managers.Clear();
            EventBus.Clear();
            DeleteSlotFiles();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            TestSaveRoot.Exit();
        }

        private void OnUnlocked(AchievementUnlockedEvent evt) => _unlockedEvents.Add(evt.Achievement.AchievementId);

        private static void DeleteSlotFiles()
        {
            string main = SlotPath(TestSlot);

            foreach (string path in new[] { main, main + ".bak", main + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private AchievementDefinition Def(string id, AchievementConditionType type, int target, string[] ids = null, bool hidden = false)
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            definition.ConfigureForTests(id, type, target, ids, hidden);
            _created.Add(definition);
            return definition;
        }

        private AchievementManager CreateManager(params AchievementDefinition[] definitions)
        {
            var manager = new AchievementManager(definitions);
            manager.Initialize();
            _managers.Add(manager);
            return manager;
        }

        /// <summary>Like <see cref="CreateManager"/> but reading/writing the profile file (inside the TestSaveRoot temp folder).</summary>
        private AchievementManager CreatePersistentManager(params AchievementDefinition[] definitions)
        {
            var manager = new AchievementManager(definitions) { PersistsProfile = true };
            manager.Initialize();
            _managers.Add(manager);
            return manager;
        }

        /// <summary>Writes a save slot the way pre-profile builds did: with an "Achievements" entry.</summary>
        private static void WriteLegacySlot(int slot, AchievementSaveState state)
        {
            var data = new SaveData
            {
                entries = new List<SaveEntry>
                {
                    new SaveEntry
                    {
                        key = AchievementManager.Key,
                        typeName = typeof(AchievementSaveState).AssemblyQualifiedName,
                        json = JsonUtility.ToJson(state)
                    }
                }
            };

            Directory.CreateDirectory(TestSaveRoot.Current);
            File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, prettyPrint: true));
        }

        private static AchievementSaveState LegacyState(long kills, params string[] unlockedIds)
        {
            return new AchievementSaveState
            {
                counterTypes = new[] { (int)AchievementConditionType.EnemiesKilled },
                counterValues = new[] { kills },
                unlockedIds = unlockedIds,
                unlockedAtIso8601 = unlockedIds.Select(_ => "2026-01-01T00:00:00.0000000Z").ToArray()
            };
        }

        private GameObject CreateObject(string name, string tag = null)
        {
            var go = new GameObject(name);

            if (tag != null)
            {
                go.tag = tag;
            }

            _created.Add(go);
            return go;
        }

        // ---------------------------------------------------------------
        // Unlocking
        // ---------------------------------------------------------------

        [Test]
        public void EnemiesKilled_UnlocksAtTarget_AndPublishesOnce()
        {
            AchievementManager manager = CreateManager(Def("kill3", AchievementConditionType.EnemiesKilled, 3));

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new EnemyDiedEvent(null, false));
            Assert.IsFalse(manager.IsUnlocked("kill3"));
            Assert.AreEqual(2, manager.GetProgress(manager.GetDefinition("kill3")));

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new EnemyDiedEvent(null, false));

            Assert.IsTrue(manager.IsUnlocked("kill3"));
            Assert.AreEqual(4, manager.GetStat(AchievementConditionType.EnemiesKilled));
            Assert.AreEqual(3, manager.GetProgress(manager.GetDefinition("kill3")), "Progress is clamped to the target.");
            CollectionAssert.AreEqual(new[] { "kill3" }, _unlockedEvents);
            Assert.IsNotNull(manager.GetUnlockedAt("kill3"));
        }

        [Test]
        public void BossDeath_CountsAsKillAndBoss()
        {
            AchievementManager manager = CreateManager(
                Def("boss", AchievementConditionType.BossesDefeated, 1),
                Def("kill", AchievementConditionType.EnemiesKilled, 1));

            EventBus.Publish(new EnemyDiedEvent(null, true));

            Assert.IsTrue(manager.IsUnlocked("boss"));
            Assert.IsTrue(manager.IsUnlocked("kill"));
        }

        [Test]
        public void QuestIdList_RequiresEveryListedQuest()
        {
            AchievementManager manager = CreateManager(
                Def("siblings", AchievementConditionType.QuestsCompleted, 1, new[] { "quest_maithe_windtrail", "quest_icaro_ancient_gears" }));
            AchievementDefinition siblings = manager.GetDefinition("siblings");

            Assert.AreEqual(2, siblings.TargetValue);

            EventBus.Publish(new QuestCompletedEvent("quest_maithe_windtrail"));
            EventBus.Publish(new QuestCompletedEvent("quest_unrelated"));
            Assert.IsFalse(manager.IsUnlocked("siblings"));
            Assert.AreEqual(1, manager.GetProgress(siblings));

            EventBus.Publish(new QuestCompletedEvent("quest_icaro_ancient_gears"));
            Assert.IsTrue(manager.IsUnlocked("siblings"));
        }

        [Test]
        public void LevelReached_TracksHighestLevel()
        {
            AchievementManager manager = CreateManager(Def("lvl5", AchievementConditionType.LevelReached, 5));

            EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 4));
            Assert.IsFalse(manager.IsUnlocked("lvl5"));

            EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 5));
            EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 2));

            Assert.IsTrue(manager.IsUnlocked("lvl5"));
            Assert.AreEqual(5, manager.GetStat(AchievementConditionType.LevelReached));
        }

        [Test]
        public void SkillCast_OnlyCountsPlayerCaster()
        {
            AchievementManager manager = CreateManager(Def("cast2", AchievementConditionType.SkillsCast, 2));
            GameObject player = CreateObject("Player", AchievementManager.PlayerTag);
            GameObject enemy = CreateObject("Enemy");

            EventBus.Publish(new SkillCastEvent(enemy, null, Vector3.zero));
            EventBus.Publish(new SkillCastEvent(null, null, Vector3.zero));
            EventBus.Publish(new SkillCastEvent(player, null, Vector3.zero));
            Assert.AreEqual(1, manager.GetStat(AchievementConditionType.SkillsCast));

            EventBus.Publish(new SkillCastEvent(player, null, Vector3.zero));
            Assert.IsTrue(manager.IsUnlocked("cast2"));
        }

        [Test]
        public void RegionsUnlocked_CountsEachRegionOnce()
        {
            AchievementManager manager = CreateManager(Def("explore2", AchievementConditionType.RegionsUnlocked, 2));

            EventBus.Publish(new WorldMapRegionUnlockedEvent(BiomeType.Desert));
            EventBus.Publish(new WorldMapRegionUnlockedEvent(BiomeType.Desert));
            Assert.AreEqual(1, manager.GetStat(AchievementConditionType.RegionsUnlocked));
            Assert.IsFalse(manager.IsUnlocked("explore2"));

            EventBus.Publish(new WorldMapRegionUnlockedEvent(BiomeType.Caves));
            Assert.IsTrue(manager.IsUnlocked("explore2"));
        }

        [Test]
        public void GoldEarned_SumsSalePayouts()
        {
            AchievementManager manager = CreateManager(Def("gold100", AchievementConditionType.GoldEarned, 100));

            EventBus.Publish(new ItemSoldEvent("shop", "item_a", 2, 60));
            Assert.IsFalse(manager.IsUnlocked("gold100"));

            EventBus.Publish(new ItemSoldEvent("shop", "item_b", 1, 40));
            Assert.IsTrue(manager.IsUnlocked("gold100"));
            Assert.AreEqual(100, manager.GetStat(AchievementConditionType.GoldEarned));
        }

        [Test]
        public void ManualUnlock_ReturnsTrueOnlyOnce_AndUnknownIdFails()
        {
            AchievementManager manager = CreateManager(Def("story", AchievementConditionType.QuestsCompleted, 99));

            Assert.IsTrue(manager.Unlock("story"));
            Assert.IsFalse(manager.Unlock("story"));
            Assert.IsFalse(manager.Unlock("does_not_exist"));
            Assert.AreEqual(1, _unlockedEvents.Count);
            Assert.AreEqual(1, manager.UnlockedCount);
        }

        [Test]
        public void Shutdown_StopsListening()
        {
            AchievementManager manager = CreateManager(Def("kill1", AchievementConditionType.EnemiesKilled, 1));
            manager.Shutdown();

            EventBus.Publish(new EnemyDiedEvent(null, false));

            Assert.IsFalse(manager.IsUnlocked("kill1"));
        }

        // ---------------------------------------------------------------
        // Save / load
        // ---------------------------------------------------------------

        [Test]
        public void ProfileFile_RoundTripsStatsObservedIdsAndUnlocks()
        {
            AchievementDefinition kill = Def("kill1", AchievementConditionType.EnemiesKilled, 1);
            AchievementDefinition pair = Def("pair", AchievementConditionType.QuestsCompleted, 1, new[] { "q_a", "q_b" });
            AchievementManager source = CreatePersistentManager(kill, pair);

            StringAssert.StartsWith(TestSaveRoot.Current, AchievementManager.ProfileFilePath, "The profile must follow SaveRootOverride.");

            EventBus.Publish(new EnemyDiedEvent(null, false));
            Assert.IsTrue(File.Exists(AchievementManager.ProfileFilePath), "An unlock writes the profile right away.");

            EventBus.Publish(new QuestCompletedEvent("q_a"));
            EventBus.Publish(new ItemSoldEvent("shop", "item", 1, 25));

            source.Shutdown(); // flushes stats gathered since the last unlock
            _unlockedEvents.Clear();

            AchievementManager restored = CreatePersistentManager(kill, pair);

            Assert.IsTrue(restored.IsUnlocked("kill1"));
            Assert.IsFalse(restored.IsUnlocked("pair"));
            Assert.IsTrue(restored.HasObserved(AchievementConditionType.QuestsCompleted, "q_a"));
            Assert.AreEqual(25, restored.GetStat(AchievementConditionType.GoldEarned));
            Assert.IsEmpty(_unlockedEvents, "Already-unlocked achievements must not re-announce on load.");

            EventBus.Publish(new QuestCompletedEvent("q_b"));
            Assert.IsTrue(restored.IsUnlocked("pair"));
        }

        [Test]
        public void RestoreState_UnlocksDefinitionsAddedAfterTheSave()
        {
            AchievementManager source = CreateManager(Def("kill1", AchievementConditionType.EnemiesKilled, 1));

            for (int i = 0; i < 10; i++)
            {
                EventBus.Publish(new EnemyDiedEvent(null, false));
            }

            object state = source.CaptureProfileState();
            source.Shutdown();
            _unlockedEvents.Clear();

            AchievementManager restored = CreateManager(
                Def("kill1", AchievementConditionType.EnemiesKilled, 1),
                Def("kill10", AchievementConditionType.EnemiesKilled, 10));
            restored.RestoreState(state);

            Assert.IsTrue(restored.IsUnlocked("kill10"));
            CollectionAssert.AreEqual(new[] { "kill10" }, _unlockedEvents);
        }

        [Test]
        public void ProfileFile_IsWrittenOnGameSaving_AndSlotsNoLongerCarryAchievements()
        {
            AchievementManager manager = CreatePersistentManager(Def("kill5", AchievementConditionType.EnemiesKilled, 5));
            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new EnemyDiedEvent(null, false));

            var saveManager = new SaveManager();
            saveManager.Initialize();
            saveManager.Register(manager);
            saveManager.Save(TestSlot); // publishes GameSavingEvent

            SaveData slot = saveManager.PeekSlot(TestSlot);
            Assert.IsNotNull(slot);
            Assert.IsFalse(SaveManager.TryGetEntryState(slot, AchievementManager.Key, out AchievementSaveState _),
                "Achievements live in the profile now, not in each save slot.");

            var profile = JsonUtility.FromJson<AchievementProfileData>(File.ReadAllText(AchievementManager.ProfileFilePath));
            Assert.IsTrue(profile.legacySlotsImported);
            CollectionAssert.AreEqual(new[] { (long)2 }, profile.achievements.counterValues);
        }

        [Test]
        public void LegacySlots_AreMergedIntoANewProfile_OnlyOnce()
        {
            AchievementDefinition kill1 = Def("kill1", AchievementConditionType.EnemiesKilled, 1);
            AchievementDefinition kill5 = Def("kill5", AchievementConditionType.EnemiesKilled, 5);
            WriteLegacySlot(TestSlot, LegacyState(3, "kill1"));
            WriteLegacySlot(TestSlot + 1, LegacyState(1));
            Assume.That(File.Exists(AchievementManager.ProfileFilePath), Is.False);

            AchievementManager first = CreatePersistentManager(kill1, kill5);

            Assert.IsTrue(first.IsUnlocked("kill1"));
            Assert.AreEqual(new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc), first.GetUnlockedAt("kill1")?.ToUniversalTime());
            Assert.AreEqual(3, first.GetStat(AchievementConditionType.EnemiesKilled), "Counters keep the highest value across slots.");
            Assert.IsTrue(File.Exists(AchievementManager.ProfileFilePath), "The import is persisted immediately.");
            Assert.IsEmpty(_unlockedEvents, "Imported unlocks must not re-announce.");
            first.Shutdown();

            // A slot changed after the import is not re-imported on the next start.
            WriteLegacySlot(TestSlot, LegacyState(50, "kill1", "kill5"));
            AchievementManager second = CreatePersistentManager(kill1, kill5);

            Assert.AreEqual(3, second.GetStat(AchievementConditionType.EnemiesKilled));
            Assert.IsFalse(second.IsUnlocked("kill5"));
        }

        [Test]
        public void LoadingALegacySlot_MergesIntoTheProfile_WithoutTakingProgressAway()
        {
            AchievementDefinition kill1 = Def("kill1", AchievementConditionType.EnemiesKilled, 1);
            AchievementDefinition boss = Def("boss1", AchievementConditionType.BossesDefeated, 1);
            AchievementManager manager = CreatePersistentManager(kill1, boss);

            for (int i = 0; i < 5; i++)
            {
                EventBus.Publish(new EnemyDiedEvent(null, false));
            }

            // Written after the profile already exists, e.g. a save copied from another machine.
            WriteLegacySlot(TestSlot, LegacyState(2, "boss1"));

            var saveManager = new SaveManager();
            saveManager.Initialize();
            saveManager.Register(manager);
            Assert.IsTrue(saveManager.Load(TestSlot));

            Assert.AreEqual(5, manager.GetStat(AchievementConditionType.EnemiesKilled), "Merge keeps the higher counter.");
            Assert.IsTrue(manager.IsUnlocked("kill1"));
            Assert.IsTrue(manager.IsUnlocked("boss1"));

            var profile = JsonUtility.FromJson<AchievementProfileData>(File.ReadAllText(AchievementManager.ProfileFilePath));
            CollectionAssert.Contains(profile.achievements.unlockedIds, "boss1", "What a legacy slot adds is written to the profile right away.");
        }

        [Test]
        public void ProfileFile_CorruptedMain_FallsBackToBackup()
        {
            AchievementDefinition kill1 = Def("kill1", AchievementConditionType.EnemiesKilled, 1);
            AchievementDefinition kill2 = Def("kill2", AchievementConditionType.EnemiesKilled, 2);
            AchievementManager source = CreatePersistentManager(kill1, kill2);
            EventBus.Publish(new EnemyDiedEvent(null, false)); // writes profile (kill1)
            EventBus.Publish(new EnemyDiedEvent(null, false)); // writes profile again, previous one becomes .bak
            source.Shutdown();

            File.WriteAllText(AchievementManager.ProfileFilePath, "{ not json");
            AchievementManager restored = CreatePersistentManager(kill1, kill2);

            Assert.IsTrue(restored.IsUnlocked("kill1"));
        }

        [Test]
        public void CustomCatalogConstructor_NeverTouchesTheProfile()
        {
            AchievementManager manager = CreateManager(Def("kill1", AchievementConditionType.EnemiesKilled, 1));
            EventBus.Publish(new EnemyDiedEvent(null, false));
            manager.Shutdown();

            Assert.IsFalse(manager.PersistsProfile);
            Assert.IsFalse(File.Exists(AchievementManager.ProfileFilePath));
        }

        // ---------------------------------------------------------------
        // Presentation
        // ---------------------------------------------------------------

        [Test]
        public void Presenter_MasksHiddenUntilUnlocked()
        {
            AchievementManager manager = CreateManager(Def("secret", AchievementConditionType.EnemiesKilled, 2, hidden: true));
            AchievementDefinition secret = manager.GetDefinition("secret");

            AchievementEntryViewModel locked = AchievementListPresenter.Build(secret, manager, key => key);
            Assert.IsTrue(locked.IsMasked);
            Assert.AreEqual(AchievementTexts.HiddenTitleKey, locked.Title);
            Assert.AreEqual("0/2", locked.ProgressText);

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new EnemyDiedEvent(null, false));

            AchievementEntryViewModel unlocked = AchievementListPresenter.Build(secret, manager, key => key);
            Assert.IsFalse(unlocked.IsMasked);
            Assert.AreEqual(secret.TitleKey, unlocked.Title);
            Assert.AreEqual(1f, unlocked.Fraction);
            Assert.AreEqual("1/1 unlocked", AchievementListPresenter.FormatSummary(manager, key => key == AchievementTexts.SummaryKey ? "{0}/{1} unlocked" : key));
        }

        [Test]
        public void Texts_EnsureLoaded_DoesNotOverrideExistingKeys()
        {
            var localization = new LocalizationManager();
            localization.LoadEntries(new[] { new LocalizationEntry(AchievementTexts.TitleKey, "Custom", "Customizado") });

            AchievementTexts.EnsureLoaded(localization);
            localization.SetLanguage(LanguageCode.en);

            Assert.AreEqual("Custom", localization.Get(AchievementTexts.TitleKey));
            Assert.AreEqual("Achievement unlocked!", localization.Get(AchievementTexts.ToastHeaderKey));
        }

        // ---------------------------------------------------------------
        // Seeded catalog
        // ---------------------------------------------------------------

        [Test]
        public void SeededCatalog_HasUniqueIds_LocalizedTexts_AndValidStoryQuests()
        {
            AchievementDefinition[] seeded = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);

            Assert.GreaterOrEqual(seeded.Length, 15);
            Assert.AreEqual(seeded.Length, seeded.Select(d => d.AchievementId).Distinct().Count(), "Achievement ids must be unique.");

            var knownQuestIds = new HashSet<string>(Resources.LoadAll<QuestDefinition>("Quests").Select(q => q.QuestId));

            foreach (AchievementDefinition definition in seeded)
            {
                foreach (string key in new[] { definition.TitleKey, definition.DescriptionKey })
                {
                    LocalizationEntry entry = AchievementTexts.All.FirstOrDefault(e => e.Key == key);
                    Assert.IsNotNull(entry, $"Missing localization for '{key}'.");
                    Assert.IsFalse(string.IsNullOrEmpty(entry.En), $"Missing en text for '{key}'.");
                    Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), $"Missing pt-BR text for '{key}'.");
                }

                if (definition.ConditionType == AchievementConditionType.QuestsCompleted)
                {
                    foreach (string questId in definition.TargetIds)
                    {
                        Assert.IsTrue(knownQuestIds.Contains(questId), $"{definition.AchievementId} references unknown quest '{questId}'.");
                    }
                }
            }

            foreach (string hero in new[] { "maithe", "maria", "joaquim", "icaro", "lavine" })
            {
                Assert.IsTrue(seeded.Any(d => d.AchievementId == $"ach_hero_{hero}"), $"Missing personal-quest achievement for {hero}.");
            }
        }

        [Test]
        public void DefaultConstructor_LoadsSeededCatalogFromResources()
        {
            var manager = new AchievementManager();
            manager.Initialize();
            _managers.Add(manager);

            Assert.GreaterOrEqual(manager.TotalCount, 15);

            EventBus.Publish(new EnemyDiedEvent(null, false));
            Assert.IsTrue(manager.IsUnlocked("ach_first_blood"));
        }
    }
}
