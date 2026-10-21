using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Save;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="DailyQuestService"/> (rotation, reset, clock rollback, save/load),
    /// <see cref="QuestManager.ResetDailyQuest"/> and the seeded daily assets in Resources/Quests/Daily —
    /// including an end-to-end pass that feeds each seeded objective the same events real gameplay
    /// publishes (kill/collect/talk), since objective-id mismatches have broken quests twice before.
    /// Same construction approach as QuestManagerTests: CreateInstance + SetDataForTests, no Scene.
    /// </summary>
    public class DailyQuestServiceTests
    {
        private const int TestSlot = 90061;

        private sealed class FakeClock : IDailyClock
        {
            public DateTime Now { get; set; }
        }

        private readonly List<Object> _created = new List<Object>();
        private QuestManager _quests;
        private DailyQuestService _daily;
        private FakeClock _clock;
        private GameObject _owner;
        private InventoryManager _inventory;
        private ItemDefinition _token;
        private List<QuestDefinition> _pool;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("DailyOwner");
            _inventory = _owner.AddComponent<InventoryManager>();
            _token = CreateItem("item_daily_token", 999);

            _quests = new QuestManager();
            _quests.Initialize();
            _quests.SetRewardTargets(_inventory, null);

            _pool = Enumerable.Range(1, 6).Select(i => CreateDaily($"quest_daily_test_{i}", $"flag_{i}")).ToList();
            _clock = new FakeClock { Now = new DateTime(2026, 10, 21, 9, 0, 0) };
            _daily = CreateService(_quests, _clock, _pool);
        }

        [TearDown]
        public void TearDown()
        {
            _daily.Shutdown();
            _quests.Shutdown();
            EventBus.Clear();
            Object.DestroyImmediate(_owner);

            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();

            var saves = new SaveManager();
            saves.DeleteSlot(TestSlot);
        }

        // ------------------------------------------------------------------
        // Picking
        // ------------------------------------------------------------------

        [Test]
        public void PickForDay_IsDeterministicDistinctAndIndependentOfInputOrder()
        {
            var ids = Enumerable.Range(1, 8).Select(i => $"q{i}").ToList();
            var reversed = Enumerable.Reverse(ids).ToList();

            List<string> a = DailyQuestService.PickForDay("2026-10-21", ids, 3);
            List<string> b = DailyQuestService.PickForDay("2026-10-21", reversed, 3);

            CollectionAssert.AreEqual(a, b);
            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(3, a.Distinct().Count());
        }

        [Test]
        public void PickForDay_VariesAcrossDays_AndClampsToPoolSize()
        {
            var ids = Enumerable.Range(1, 8).Select(i => $"q{i}").ToList();
            var distinctSets = new HashSet<string>();
            var day = new DateTime(2026, 1, 1);

            for (int i = 0; i < 30; i++)
            {
                distinctSets.Add(string.Join(",", DailyQuestService.PickForDay(DailyQuestService.ToDayKey(day.AddDays(i)), ids, 3)));
            }

            Assert.Greater(distinctSets.Count, 10, "30 consecutive days should not keep repeating the same few sets.");
            Assert.AreEqual(2, DailyQuestService.PickForDay("2026-10-21", new[] { "a", "b" }, 3).Count);
            Assert.AreEqual(0, DailyQuestService.PickForDay("2026-10-21", new string[0], 3).Count);
        }

        [Test]
        public void ToDayKey_UsesLocalCalendarDate()
        {
            Assert.AreEqual("2026-10-21", DailyQuestService.ToDayKey(new DateTime(2026, 10, 21, 23, 59, 59)));
            Assert.AreEqual("2026-10-22", DailyQuestService.ToDayKey(new DateTime(2026, 10, 22, 0, 0, 1)));
            Assert.AreEqual(20261021, DailyQuestService.DaySeed("2026-10-21"));
            Assert.AreEqual(0, DailyQuestService.DaySeed("garbage"));
        }

        // ------------------------------------------------------------------
        // Rotation
        // ------------------------------------------------------------------

        [Test]
        public void Initialize_StartsThreeDailiesForToday()
        {
            Assert.AreEqual("2026-10-21", _daily.CurrentDayKey);
            Assert.AreEqual(DailyQuestService.DefaultDailyCount, _daily.TodayQuestIds.Count);
            CollectionAssert.AreEqual(DailyQuestService.PickForDay("2026-10-21", _pool.Select(q => q.QuestId).ToList(), 3), _daily.TodayQuestIds);

            foreach (string id in _daily.TodayQuestIds)
            {
                Assert.IsTrue(_quests.IsActive(id), id);
            }
        }

        [Test]
        public void Pool_IgnoresNonDailyQuests()
        {
            QuestDefinition side = CreateQuest("quest_side_x", QuestType.Side, "flag_side");
            DailyQuestService service = CreateService(new QuestManager(), _clock, _pool.Concat(new[] { side }));

            Assert.IsFalse(service.IsDailyQuest("quest_side_x"));
            Assert.AreEqual(_pool.Count, service.Pool.Count);
            service.Shutdown();
        }

        [Test]
        public void Refresh_SameDay_DoesNothing()
        {
            List<string> before = _daily.TodayQuestIds.ToList();
            _clock.Now = _clock.Now.AddHours(10);

            Assert.IsFalse(_daily.Refresh());
            CollectionAssert.AreEqual(before, _daily.TodayQuestIds);
        }

        [Test]
        public void Refresh_NextDay_AbandonsUnfinishedResetsCompletedAndStartsNewSet()
        {
            List<string> yesterday = _daily.TodayQuestIds.ToList();
            Complete(yesterday[0]);
            var abandoned = new List<string>();
            EventBus.Subscribe<QuestAbandonedEvent>(e => abandoned.Add(e.QuestId));
            DailyQuestsRefreshedEvent? refreshed = null;
            EventBus.Subscribe<DailyQuestsRefreshedEvent>(e => refreshed = e);

            _clock.Now = new DateTime(2026, 10, 22, 0, 0, 5);
            Assert.IsTrue(_daily.Refresh());

            CollectionAssert.AreEquivalent(yesterday.Skip(1), abandoned, "Unfinished dailies are abandoned on rollover.");
            Assert.AreEqual("2026-10-22", _daily.CurrentDayKey);
            Assert.IsTrue(refreshed.HasValue);
            Assert.AreEqual("2026-10-22", refreshed.Value.DayKey);

            foreach (QuestDefinition quest in _pool)
            {
                bool picked = _daily.TodayQuestIds.Contains(quest.QuestId);
                Assert.AreEqual(picked, _quests.IsActive(quest.QuestId), quest.QuestId);
                Assert.IsFalse(_quests.IsCompleted(quest.QuestId), $"{quest.QuestId} should have been reset.");
            }
        }

        [Test]
        public void SameDaily_CanBeCompletedAgainOnALaterDay_AndPaysEachTime()
        {
            string id = _daily.TodayQuestIds[0];
            Complete(id);
            Assert.AreEqual(5, _inventory.GetTotalCount("item_daily_token"));

            // Walk forward until the same quest is picked again (pool of 6, 3 a day: quickly happens).
            // Always advance at least one day: today's rotation already contains the id.
            DateTime day = _clock.Now;
            for (int i = 1; i <= 60; i++)
            {
                _clock.Now = day.AddDays(i);
                _daily.Refresh();
                if (_daily.TodayQuestIds.Contains(id))
                {
                    break;
                }
            }

            Assume.That(_daily.TodayQuestIds.Contains(id), "quest never re-picked in 60 days");
            Assert.IsTrue(_quests.IsActive(id));
            Complete(id);
            Assert.AreEqual(10, _inventory.GetTotalCount("item_daily_token"));
        }

        [Test]
        public void ClockRollback_KeepsNewerRotation_NoResetNoDoubleReward()
        {
            _clock.Now = new DateTime(2026, 10, 22, 8, 0, 0);
            _daily.Refresh();
            string id = _daily.TodayQuestIds[0];
            Complete(id);
            int paid = _inventory.GetTotalCount("item_daily_token");

            _clock.Now = new DateTime(2026, 10, 21, 8, 0, 0);
            Assert.IsFalse(_daily.Refresh());
            Assert.AreEqual("2026-10-22", _daily.CurrentDayKey);
            Assert.IsTrue(_quests.IsCompleted(id));
            Assert.IsFalse(_quests.StartQuest(_pool.First(q => q.QuestId == id)), "completed daily must not restart");

            // Moving forward again to the rotation's own day still isn't a new day.
            _clock.Now = new DateTime(2026, 10, 22, 23, 0, 0);
            Assert.IsFalse(_daily.Refresh());
            Assert.IsTrue(_quests.IsCompleted(id));
            Assert.AreEqual(paid, _inventory.GetTotalCount("item_daily_token"));

            _clock.Now = new DateTime(2026, 10, 23, 0, 1, 0);
            Assert.IsTrue(_daily.Refresh());
            Assert.IsFalse(_quests.IsCompleted(id));
        }

        [Test]
        public void ResetDailyQuest_RefusesNonDailyQuests()
        {
            QuestDefinition side = CreateQuest("quest_side_y", QuestType.Side, "flag_side_y");
            _quests.StartQuest(side);
            _quests.SetCustomFlag("flag_side_y");
            Assume.That(_quests.IsCompleted("quest_side_y"));

            Assert.IsFalse(_quests.ResetDailyQuest("quest_side_y"));
            Assert.IsTrue(_quests.IsCompleted("quest_side_y"));
            Assert.IsFalse(_quests.ResetDailyQuest("quest_unknown"));
        }

        [Test]
        public void ResetDailyQuest_AllowsStartingACompletedDailyAgain()
        {
            string id = _daily.TodayQuestIds[0];
            Complete(id);
            QuestDefinition definition = _pool.First(q => q.QuestId == id);
            Assume.That(_quests.StartQuest(definition), Is.False);

            Assert.IsTrue(_quests.ResetDailyQuest(id));
            Assert.IsTrue(_quests.StartQuest(definition));
        }

        // ------------------------------------------------------------------
        // Save / load
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_SameDay_KeepsPicksAndCompletion()
        {
            string done = _daily.TodayQuestIds[0];
            Complete(done);
            List<string> picks = _daily.TodayQuestIds.ToList();
            SaveAndReload(new DateTime(2026, 10, 21, 22, 0, 0), out QuestManager quests, out DailyQuestService daily, out SaveManager saves);

            CollectionAssert.AreEqual(picks, daily.TodayQuestIds);
            Assert.IsTrue(quests.IsCompleted(done));
            Assert.IsFalse(quests.IsActive(done));
            Assert.IsTrue(picks.Skip(1).All(quests.IsActive));

            daily.Shutdown();
            quests.Shutdown();
            saves.Shutdown();
        }

        [Test]
        public void SaveLoad_OnALaterDay_RollsOver()
        {
            Complete(_daily.TodayQuestIds[0]);
            SaveAndReload(new DateTime(2026, 10, 25, 9, 0, 0), out QuestManager quests, out DailyQuestService daily, out SaveManager saves);

            Assert.AreEqual("2026-10-25", daily.CurrentDayKey);
            CollectionAssert.AreEqual(DailyQuestService.PickForDay("2026-10-25", _pool.Select(q => q.QuestId).ToList(), 3), daily.TodayQuestIds);
            Assert.IsTrue(_pool.All(q => !quests.IsCompleted(q.QuestId)));
            Assert.IsTrue(daily.TodayQuestIds.All(quests.IsActive));

            daily.Shutdown();
            quests.Shutdown();
            saves.Shutdown();
        }

        [Test]
        public void SaveLoad_WithClockBehindTheSave_DoesNotReset()
        {
            _clock.Now = new DateTime(2026, 10, 24, 9, 0, 0);
            _daily.Refresh();
            string done = _daily.TodayQuestIds[0];
            Complete(done);

            SaveAndReload(new DateTime(2026, 10, 20, 9, 0, 0), out QuestManager quests, out DailyQuestService daily, out SaveManager saves);

            Assert.AreEqual("2026-10-24", daily.CurrentDayKey);
            Assert.IsTrue(quests.IsCompleted(done));

            daily.Shutdown();
            quests.Shutdown();
            saves.Shutdown();
        }

        // ------------------------------------------------------------------
        // Seeded content (Resources/Quests/Daily)
        // ------------------------------------------------------------------

        private static QuestDefinition[] LoadSeededDailies() => Resources.LoadAll<QuestDefinition>(DailyQuestService.ResourcesFolder);

        [Test]
        public void SeededDailies_AreDailyTypeWithLocalizedTextsAndModestRewards()
        {
            QuestDefinition[] dailies = LoadSeededDailies();
            Assert.GreaterOrEqual(dailies.Length, 8);
            Assert.AreEqual(dailies.Length, dailies.Select(d => d.QuestId).Distinct().Count());

            var localization = new LocalizationManager();
            localization.Initialize();
            DailyQuestTexts.EnsureLoaded(localization);

            foreach (QuestDefinition quest in dailies)
            {
                Assert.AreEqual(QuestType.Daily, quest.QuestType, quest.QuestId);
                Assert.AreEqual(quest.name, quest.QuestId, "asset name must equal its quest id");
                Assert.IsEmpty(quest.PrerequisiteQuestIds, quest.QuestId);
                Assert.IsTrue(localization.HasKey(quest.TitleKey), quest.TitleKey);
                Assert.IsTrue(localization.HasKey(quest.DescriptionKey), quest.DescriptionKey);
                Assert.IsTrue(localization.HasKey(quest.TitleKey.Replace(".title", ".journal")), quest.QuestId + " journal");
                Assert.That(quest.RewardExperience, Is.InRange(1, 150), quest.QuestId);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null && r.Quantity > 0 && r.Quantity <= 30), quest.QuestId);
                Assert.IsNotEmpty(quest.Objectives, quest.QuestId);

                foreach (QuestObjective objective in quest.Objectives)
                {
                    Assert.IsTrue(localization.HasKey(objective.DescriptionKey), $"{quest.QuestId}/{objective.ObjectiveId}");
                }
            }

            localization.Shutdown();
        }

        [Test]
        public void DailyQuestTexts_EveryEntryHasEnglishAndPortuguese()
        {
            foreach (LocalizationEntry entry in DailyQuestTexts.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{entry.Key} missing en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{entry.Key} missing ptBR");
            }

            Assert.AreEqual(DailyQuestTexts.All.Count, DailyQuestTexts.All.Select(e => e.Key).Distinct().Count(), "duplicate keys");
        }

        [Test]
        public void SeededDailies_ObjectiveTargetsResolveToRealContent()
        {
            foreach (QuestDefinition quest in LoadSeededDailies())
            {
                foreach (QuestObjective objective in quest.Objectives)
                {
                    string where = $"{quest.QuestId}/{objective.ObjectiveId}";

                    switch (objective.Type)
                    {
                        case QuestObjectiveType.KillEnemyType:
                            var enemy = Resources.Load<EnemyDefinition>("Enemies/" + objective.TargetId);
                            Assert.IsNotNull(enemy, where);
                            Assert.AreEqual(objective.TargetId, enemy.name, where);
                            break;
                        case QuestObjectiveType.CollectItem:
                            var item = Resources.Load<ItemDefinition>("Items/" + objective.TargetId);
                            Assert.IsNotNull(item, where);
                            Assert.AreEqual(objective.TargetId, item.ItemId, where);
                            break;
                        case QuestObjectiveType.TalkToNpc:
                            StringAssert.StartsWith("npc_", objective.TargetId, where);
                            Assert.IsNotNull(Resources.Load<DialogueDefinition>("Dialogue/dialogue_" + objective.TargetId.Substring(4)), where);
                            break;
                        default:
                            Assert.Fail($"{where}: dailies must use objectives real gameplay can progress (kill/collect/talk), got {objective.Type}.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Feeds every seeded daily the exact events gameplay publishes — CombatTargetDiedEvent on an
        /// EnemyController carrying the real enemy asset, real items added to the inventory, and
        /// DialogueEndedEvent with the real dialogue id — and requires each quest to complete.
        /// </summary>
        [Test]
        public void SeededDailies_CompleteFromRealGameplayEvents()
        {
            foreach (QuestDefinition quest in LoadSeededDailies())
            {
                EventBus.Clear();
                var owner = new GameObject("Owner_" + quest.QuestId);
                var inventory = owner.AddComponent<InventoryManager>();
                var quests = new QuestManager();
                quests.Initialize();
                quests.SetRewardTargets(inventory, null);

                Assert.IsTrue(quests.StartQuest(quest), quest.QuestId);

                foreach (QuestObjective objective in quest.Objectives)
                {
                    for (int i = 0; i < objective.RequiredAmount && objective.Type == QuestObjectiveType.KillEnemyType; i++)
                    {
                        SimulateEnemyDeath(Resources.Load<EnemyDefinition>("Enemies/" + objective.TargetId));
                    }

                    if (objective.Type == QuestObjectiveType.CollectItem)
                    {
                        inventory.AddItem(Resources.Load<ItemDefinition>("Items/" + objective.TargetId), objective.RequiredAmount);
                    }

                    if (objective.Type == QuestObjectiveType.TalkToNpc)
                    {
                        EventBus.Publish(new DialogueEndedEvent("dialogue_" + objective.TargetId.Substring(4)));
                    }
                }

                Assert.IsTrue(quests.IsCompleted(quest.QuestId), $"{quest.QuestId} did not complete from real events.");

                quests.Shutdown();
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void ServiceWithSeededPool_StartsThreeRealDailies()
        {
            var quests = new QuestManager();
            quests.Initialize();
            var service = new DailyQuestService(quests, _clock);
            service.Initialize();

            Assert.AreEqual(3, service.TodayQuestIds.Count);
            Assert.IsTrue(service.TodayQuestIds.All(quests.IsActive));

            service.Shutdown();
            quests.Shutdown();
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private DailyQuestService CreateService(QuestManager quests, IDailyClock clock, IEnumerable<QuestDefinition> pool)
        {
            List<QuestDefinition> poolList = pool.ToList();
            var service = new DailyQuestService(quests, clock, () => poolList);
            service.Initialize();
            return service;
        }

        /// <summary>Saves the fixture's quest+daily state, then loads it into fresh managers whose clock reads <paramref name="loadTime"/>.</summary>
        private void SaveAndReload(DateTime loadTime, out QuestManager quests, out DailyQuestService daily, out SaveManager saves)
        {
            var writer = new SaveManager();
            writer.Initialize();
            writer.Register(_quests);
            writer.Register(_daily);
            writer.Save(TestSlot);
            writer.Shutdown();

            quests = new QuestManager();
            quests.Initialize();
            daily = CreateService(quests, new FakeClock { Now = loadTime }, _pool);

            saves = new SaveManager();
            saves.Initialize();
            saves.Register(quests);
            saves.Register(daily);
            Assert.IsTrue(saves.Load(TestSlot));
        }

        private void Complete(string questId)
        {
            // SetObjectiveProgress, not SetCustomFlag: QuestManager remembers raised flags forever, so
            // a flag-driven objective could only ever be satisfied once (why seeded dailies avoid CustomFlag).
            _quests.SetObjectiveProgress(questId, "do_it", 1);
            Assume.That(_quests.IsCompleted(questId), $"{questId} should have completed");
        }

        private QuestDefinition CreateDaily(string questId, string flag)
        {
            QuestDefinition quest = CreateQuest(questId, QuestType.Daily, flag);
            return quest;
        }

        private QuestDefinition CreateQuest(string questId, QuestType type, string flag)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            _created.Add(quest);

            var objectives = new List<QuestObjective>
            {
                new QuestObjective { ObjectiveId = "do_it", Type = QuestObjectiveType.CustomFlag, TargetId = flag, RequiredAmount = 1 }
            };

            quest.SetDataForTests(questId, questId + ".title", questId + ".description", type, objectives,
                rewardExperience: 10,
                rewardItems: new List<QuestItemReward> { new QuestItemReward { Item = _token, Quantity = 5 } });
            return quest;
        }

        private ItemDefinition CreateItem(string itemId, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static void SimulateEnemyDeath(EnemyDefinition definition)
        {
            var enemyObj = new GameObject("Enemy_" + definition.name);
            var controller = enemyObj.AddComponent<EnemyController>();
            FieldInfo field = typeof(EnemyController).GetField("_definition", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field);
            field.SetValue(controller, definition);

            EventBus.Publish(new CombatTargetDiedEvent(enemyObj, null));
            Object.DestroyImmediate(enemyObj);
        }
    }
}
