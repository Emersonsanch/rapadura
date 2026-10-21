using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.UI.Quests;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Quest Log/Tracker view-model layer — <see cref="QuestLogPresenter"/>,
    /// <see cref="QuestLogTexts"/> and <see cref="QuestTracking"/> — exercised without a
    /// <c>UIDocument</c>, same split as <c>SaveSlotPresenterTests</c>/<c>HudBarMathTests</c>.
    /// Quests are built via <see cref="ScriptableObject.CreateInstance{T}"/> + <c>SetDataForTests</c>,
    /// like <c>QuestManagerTests</c>.
    /// </summary>
    public class QuestLogPresenterTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            QuestTracking.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            QuestTracking.Reset();
            EventBus.Clear();
        }

        // ------------------------------------------------------------------
        // Formatting
        // ------------------------------------------------------------------

        [Test]
        public void FormatProgress_ShowsCurrentOverRequired()
        {
            Assert.AreEqual("3/5", QuestLogPresenter.FormatProgress(3, 5));
        }

        [Test]
        public void FormatProgress_ClampsOutOfRangeValues()
        {
            Assert.AreEqual("5/5", QuestLogPresenter.FormatProgress(9, 5));
            Assert.AreEqual("0/5", QuestLogPresenter.FormatProgress(-2, 5));
            Assert.AreEqual("0/1", QuestLogPresenter.FormatProgress(0, 0), "RequiredAmount floors at 1, matching QuestObjective.IsComplete.");
        }

        [Test]
        public void GetFraction_IsClampedZeroToOne()
        {
            Assert.AreEqual(0.6f, QuestLogPresenter.GetFraction(3, 5), 0.0001f);
            Assert.AreEqual(1f, QuestLogPresenter.GetFraction(7, 5), 0.0001f);
            Assert.AreEqual(0f, QuestLogPresenter.GetFraction(-1, 5), 0.0001f);
        }

        [Test]
        public void StripBakedProgress_RemovesTrailingStaticCounter()
        {
            Assert.AreEqual("Fell the marked stone golems", QuestLogPresenter.StripBakedProgress("Fell the marked stone golems (0/4)"));
            Assert.AreEqual("Derrote os golens", QuestLogPresenter.StripBakedProgress("Derrote os golens ( 0 / 4 )"));
        }

        [Test]
        public void StripBakedProgress_LeavesOtherTextAlone()
        {
            Assert.AreEqual("Speak with the survivor (near the ruins)", QuestLogPresenter.StripBakedProgress("Speak with the survivor (near the ruins)"));
            Assert.AreEqual(string.Empty, QuestLogPresenter.StripBakedProgress(null));
        }

        [Test]
        public void GetJournalKey_DerivesFromTitleKey()
        {
            Assert.AreEqual("quest.maithe_windtrail.journal", QuestLogPresenter.GetJournalKey("quest.maithe_windtrail.title"));
            Assert.IsNull(QuestLogPresenter.GetJournalKey("quest.maithe_windtrail.name"));
            Assert.IsNull(QuestLogPresenter.GetJournalKey(null));
        }

        [Test]
        public void GetTypeLabelKey_MapsEveryQuestType()
        {
            Assert.AreEqual(QuestLogPresenter.TypeMainKey, QuestLogPresenter.GetTypeLabelKey(QuestType.MainStory));
            Assert.AreEqual(QuestLogPresenter.TypeSideKey, QuestLogPresenter.GetTypeLabelKey(QuestType.Side));
            Assert.AreEqual(QuestLogPresenter.TypeDailyKey, QuestLogPresenter.GetTypeLabelKey(QuestType.Daily));
        }

        [Test]
        public void FormatRewards_JoinsXpAndItems_SkippingInvalidEntries()
        {
            var items = new List<(string, int)> { ("Arco de Caça", 1), ("Poção", 3), ("", 2), ("Nada", 0) };

            string text = QuestLogPresenter.FormatRewards(450, items, key => "{0} XP");

            Assert.AreEqual("450 XP · Arco de Caça · Poção x3", text);
        }

        [Test]
        public void FormatRewards_MissingXpKey_FallsBackInsteadOfShowingRawKey()
        {
            string text = QuestLogPresenter.FormatRewards(100, null, key => $"[{key}]");

            Assert.AreEqual("100 XP", text);
        }

        [Test]
        public void FormatRewards_NothingToGrant_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, QuestLogPresenter.FormatRewards(0, new List<(string, int)>(), key => key));
        }

        // ------------------------------------------------------------------
        // BuildEntry
        // ------------------------------------------------------------------

        [Test]
        public void BuildEntry_ActiveQuest_UsesLiveProgressAndStripsBakedCounter()
        {
            QuestDefinition quest = CreateQuest("quest_wolves", QuestType.Side,
                Objective("kill", 5, "obj.kill"),
                Objective("talk", 1, "obj.talk"));

            var progress = new QuestProgress
            {
                QuestId = "quest_wolves",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { ObjectiveId = "kill", RequiredAmount = 5, CurrentAmount = 3 },
                    new QuestObjective { ObjectiveId = "talk", RequiredAmount = 1, CurrentAmount = 1 }
                }
            };

            var texts = new Dictionary<string, string>
            {
                { "quest.wolves.title", "Wolves" },
                { "obj.kill", "Hunt the wolves (0/5)" },
                { "obj.talk", "Talk to the hunter" }
            };

            QuestEntryViewModel entry = QuestLogPresenter.BuildEntry(quest, progress, false, key => texts.TryGetValue(key, out string t) ? t : key);

            Assert.AreEqual("Wolves", entry.Title);
            Assert.IsFalse(entry.IsCompleted);
            Assert.AreEqual(2, entry.Objectives.Count);
            Assert.AreEqual("Hunt the wolves", entry.Objectives[0].Text);
            Assert.AreEqual("3/5", entry.Objectives[0].ProgressText);
            Assert.IsFalse(entry.Objectives[0].IsComplete);
            Assert.IsTrue(entry.Objectives[1].IsComplete);
            Assert.AreEqual(1, entry.CompletedObjectiveCount);
            Assert.AreEqual("kill", entry.NextObjective.ObjectiveId);
        }

        [Test]
        public void BuildEntry_CompletedQuest_ShowsEveryObjectiveDone()
        {
            QuestDefinition quest = CreateQuest("quest_done", QuestType.MainStory, Objective("a", 4, "obj.a"), Objective("b", 2, "obj.b"));

            QuestEntryViewModel entry = QuestLogPresenter.BuildEntry(quest, null, true, key => key);

            Assert.IsTrue(entry.IsCompleted);
            Assert.IsTrue(entry.Objectives.All(o => o.IsComplete));
            Assert.AreEqual("4/4", entry.Objectives[0].ProgressText);
            Assert.IsNull(entry.NextObjective);
        }

        [Test]
        public void BuildEntry_NotStartedQuest_ShowsZeroProgress()
        {
            QuestDefinition quest = CreateQuest("quest_new", QuestType.Side, Objective("a", 3, "obj.a"));

            QuestEntryViewModel entry = QuestLogPresenter.BuildEntry(quest, null, false, key => key);

            Assert.AreEqual("0/3", entry.Objectives[0].ProgressText);
            Assert.AreEqual(0f, entry.Objectives[0].Fraction);
        }

        [Test]
        public void BuildEntry_JournalOnlyWhenKeyExists()
        {
            QuestDefinition quest = CreateQuest("quest_wolves", QuestType.Side, Objective("a", 1, "obj.a"));

            QuestEntryViewModel withoutJournal = QuestLogPresenter.BuildEntry(quest, null, false, key => key, key => false);
            QuestEntryViewModel withJournal = QuestLogPresenter.BuildEntry(quest, null, false, key => key == "quest.wolves.journal" ? "Dear diary" : key, key => true);

            Assert.IsNull(withoutJournal.Journal);
            Assert.AreEqual("Dear diary", withJournal.Journal);
        }

        [Test]
        public void BuildEntry_IncludesItemRewards()
        {
            ItemDefinition bow = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(bow);
            SetPrivateField(bow, "_displayName", "Arco de Caça");

            QuestDefinition quest = CreateQuest("quest_reward", QuestType.Side, Objective("a", 1, "obj.a"));
            quest.SetDataForTests("quest_reward", "quest.reward.title", "quest.reward.description", QuestType.Side,
                new List<QuestObjective> { Objective("a", 1, "obj.a") }, null, 450,
                new List<QuestItemReward> { new QuestItemReward { Item = bow, Quantity = 1 } });

            QuestEntryViewModel entry = QuestLogPresenter.BuildEntry(quest, null, false, key => key == QuestLogPresenter.RewardXpKey ? "{0} XP" : key);

            Assert.AreEqual("450 XP · Arco de Caça", entry.RewardText);
        }

        // ------------------------------------------------------------------
        // Ordering / tracking
        // ------------------------------------------------------------------

        [Test]
        public void Sort_MainStoryFirst_ThenByTitle()
        {
            var entries = new List<QuestEntryViewModel>
            {
                new QuestEntryViewModel { QuestId = "side_b", QuestType = QuestType.Side, Title = "B" },
                new QuestEntryViewModel { QuestId = "main_z", QuestType = QuestType.MainStory, Title = "Z" },
                new QuestEntryViewModel { QuestId = "side_a", QuestType = QuestType.Side, Title = "A" },
                new QuestEntryViewModel { QuestId = "daily", QuestType = QuestType.Daily, Title = "0" }
            };

            List<string> order = QuestLogPresenter.Sort(entries).Select(e => e.QuestId).ToList();

            CollectionAssert.AreEqual(new[] { "main_z", "side_a", "side_b", "daily" }, order);
        }

        [Test]
        public void ResolveTrackedQuestId_KeepsPreferredWhileActive_ElseFallsBackToFirst()
        {
            var active = new List<QuestEntryViewModel>
            {
                new QuestEntryViewModel { QuestId = "first" },
                new QuestEntryViewModel { QuestId = "second" }
            };

            Assert.AreEqual("second", QuestLogPresenter.ResolveTrackedQuestId("second", active));
            Assert.AreEqual("first", QuestLogPresenter.ResolveTrackedQuestId("already_completed", active));
            Assert.AreEqual("first", QuestLogPresenter.ResolveTrackedQuestId(null, active));
            Assert.IsNull(QuestLogPresenter.ResolveTrackedQuestId("second", new List<QuestEntryViewModel>()));
        }

        [Test]
        public void QuestTracking_SetPreferred_PublishesOnlyOnChange()
        {
            int published = 0;
            string lastId = null;
            EventBus.Subscribe<QuestTrackedChangedEvent>(evt => { published++; lastId = evt.QuestId; });

            QuestTracking.SetPreferred("quest_a");
            QuestTracking.SetPreferred("quest_a");
            QuestTracking.SetPreferred("quest_b");

            Assert.AreEqual(2, published);
            Assert.AreEqual("quest_b", lastId);
            Assert.AreEqual("quest_b", QuestTracking.PreferredQuestId);
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void QuestLogTexts_EnsureLoaded_AddsMissingKeys_WithoutOverridingExisting()
        {
            var localization = new LocalizationManager();
            localization.LoadEntries(new[] { new LocalizationEntry(QuestLogTexts.TitleKey, "Custom Quests", "Missões Custom") });

            QuestLogTexts.EnsureLoaded(localization);

            Assert.AreEqual("Custom Quests", localization.Get(QuestLogTexts.TitleKey));
            Assert.IsTrue(localization.HasKey(QuestLogTexts.TrackKey));
            Assert.IsTrue(localization.HasKey(QuestLogPresenter.RewardXpKey));
        }

        [Test]
        public void QuestLogTexts_EveryEntryHasEnglishAndPortuguese()
        {
            foreach (LocalizationEntry entry in QuestLogTexts.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{entry.Key} missing en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{entry.Key} missing ptBR");
            }
        }

        [Test]
        public void EverySeededQuest_HasAJournalEntry()
        {
            QuestDefinition[] quests = Resources.LoadAll<QuestDefinition>(QuestLogDataSource.ResourcesFolder);
            Assume.That(quests.Length, Is.GreaterThan(0), "No quest assets under Resources/Quests in this checkout.");

            foreach (QuestDefinition quest in quests)
            {
                string journalKey = QuestLogPresenter.GetJournalKey(quest.TitleKey);
                Assert.IsNotNull(journalKey, $"{quest.QuestId} title key doesn't end with .title");
                // Daily quests ship their journal notes with their other texts in DailyQuestTexts.
                Assert.IsTrue(QuestLogTexts.All.Concat(DailyQuestTexts.All).Any(e => e.Key == journalKey), $"{quest.QuestId} has no journal entry '{journalKey}'.");
            }
        }

        [Test]
        public void GetFallback_ReturnsPortugueseOrBracketedKey()
        {
            Assert.AreEqual("Missões", QuestLogTexts.GetFallback(QuestLogTexts.TitleKey));
            Assert.AreEqual("[ui.quest.nope]", QuestLogTexts.GetFallback("ui.quest.nope"));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private QuestDefinition CreateQuest(string questId, QuestType type, params QuestObjective[] objectives)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            _created.Add(quest);

            string stem = questId.Replace("quest_", "quest.");
            quest.SetDataForTests(questId, $"{stem}.title", $"{stem}.description", type, objectives.ToList());
            return quest;
        }

        private static QuestObjective Objective(string id, int required, string descriptionKey)
        {
            return new QuestObjective
            {
                ObjectiveId = id,
                Type = QuestObjectiveType.CustomFlag,
                TargetId = id,
                RequiredAmount = required,
                DescriptionKey = descriptionKey
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
