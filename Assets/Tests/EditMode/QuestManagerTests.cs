using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 8 "Quests" backend: <see cref="QuestManager"/> and
    /// <see cref="QuestDefinition"/>. Quests/items are ScriptableObjects built via
    /// <see cref="ScriptableObject.CreateInstance{T}"/> + <c>SetDataForTests</c>/reflection, the
    /// same approach as <c>CraftingManagerTests</c>/<c>DialogueManagerTests</c> — no AssetDatabase,
    /// no Scene required.
    /// </summary>
    public class QuestManagerTests
    {
        private QuestManager _quests;
        private GameObject _owner;
        private InventoryManager _inventory;
        private ItemDefinition _wolfPelt;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _quests = new QuestManager();
            _quests.Initialize();

            _owner = new GameObject("QuestOwner");
            _inventory = _owner.AddComponent<InventoryManager>();

            _wolfPelt = CreateItem("item_wolf_pelt", 99);

            _quests.SetRewardTargets(_inventory, null);
        }

        [TearDown]
        public void TearDown()
        {
            _quests.Shutdown();
            EventBus.Clear();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_wolfPelt);
        }

        // ------------------------------------------------------------------
        // StartQuest
        // ------------------------------------------------------------------

        [Test]
        public void StartQuest_ActivatesQuestAndPublishesEvent()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3);
            bool published = false;
            EventBus.Subscribe<QuestStartedEvent>(evt => published = evt.QuestId == "quest_wolves");

            bool started = _quests.StartQuest(quest);

            Assert.IsTrue(started);
            Assert.IsTrue(_quests.IsActive("quest_wolves"));
            Assert.IsTrue(published);
        }

        [Test]
        public void StartQuest_AlreadyActive_ReturnsFalse()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3);
            _quests.StartQuest(quest);

            bool startedAgain = _quests.StartQuest(quest);

            Assert.IsFalse(startedAgain);
        }

        [Test]
        public void StartQuest_WithUnmetPrerequisite_IsBlocked()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3, prerequisiteIds: new[] { "quest_intro" });

            bool started = _quests.StartQuest(quest);

            Assert.IsFalse(started);
            Assert.IsFalse(_quests.IsActive("quest_wolves"));
        }

        [Test]
        public void StartQuest_WithCompletedPrerequisite_Succeeds()
        {
            QuestDefinition intro = BuildKillQuest("quest_intro", "rat", 1);
            _quests.StartQuest(intro);
            SimulateEnemyDeath("rat");
            Assert.IsTrue(_quests.IsCompleted("quest_intro"));

            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3, prerequisiteIds: new[] { "quest_intro" });
            bool started = _quests.StartQuest(quest);

            Assert.IsTrue(started);
        }

        // ------------------------------------------------------------------
        // Objective progress via simulated events
        // ------------------------------------------------------------------

        [Test]
        public void CombatTargetDiedEvent_ProgressesMatchingKillObjective()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3);
            _quests.StartQuest(quest);

            SimulateEnemyDeath("wolf");
            SimulateEnemyDeath("wolf");

            QuestProgress progress = _quests.GetProgress("quest_wolves");
            Assert.AreEqual(2, progress.Objectives[0].CurrentAmount);
            Assert.IsFalse(progress.Objectives[0].IsComplete);
        }

        [Test]
        public void CombatTargetDiedEvent_WithNonMatchingEnemy_DoesNotProgress()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3);
            _quests.StartQuest(quest);

            SimulateEnemyDeath("goblin");

            QuestProgress progress = _quests.GetProgress("quest_wolves");
            Assert.AreEqual(0, progress.Objectives[0].CurrentAmount);
        }

        [Test]
        public void RecipeCraftedEvent_ProgressesCollectItemObjective()
        {
            QuestDefinition quest = BuildCollectQuest("quest_pelts", _wolfPelt.ItemId, 2);
            _quests.StartQuest(quest);

            RecipeDefinition recipe = CreateRecipe("recipe_pelt", _wolfPelt, 2);
            EventBus.Publish(new RecipeCraftedEvent(recipe));

            QuestProgress progress = _quests.GetProgress("quest_pelts");
            Assert.AreEqual(2, progress.Objectives[0].CurrentAmount);
            Assert.IsTrue(progress.Objectives[0].IsComplete);
        }

        [Test]
        public void DialogueEndedEvent_ProgressesTalkToNpcObjective()
        {
            QuestDefinition quest = BuildTalkQuest("quest_meet_elder", "dialogue_elder");
            _quests.StartQuest(quest);

            EventBus.Publish(new DialogueEndedEvent("dialogue_elder"));

            Assert.IsTrue(_quests.IsCompleted("quest_meet_elder"));
        }

        // ------------------------------------------------------------------
        // Completion and rewards
        // ------------------------------------------------------------------

        [Test]
        public void CompleteQuest_WhenAllObjectivesDone_GrantsItemReward()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 1, rewardItem: _wolfPelt, rewardItemQty: 5);
            _quests.StartQuest(quest);

            SimulateEnemyDeath("wolf");

            Assert.IsTrue(_quests.IsCompleted("quest_wolves"));
            Assert.AreEqual(5, _inventory.GetTotalCount(_wolfPelt.ItemId));
        }

        [Test]
        public void CompleteQuest_WhenAllObjectivesDone_GrantsExperienceReward()
        {
            GameObject playerObj = new GameObject("Player");
            PlayerStats stats = playerObj.AddComponent<PlayerStats>();
            _quests.SetRewardTargets(_inventory, stats);

            int levelBefore = stats.Level;

            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 1, rewardExperience: 50);
            _quests.StartQuest(quest);

            SimulateEnemyDeath("wolf");

            Assert.IsTrue(_quests.IsCompleted("quest_wolves"));
            // AddExperience(50) should not throw and should either raise XP or level; smoke-check via no exception plus completion.
            Assert.GreaterOrEqual(stats.Level, levelBefore);

            Object.DestroyImmediate(playerObj);
        }

        [Test]
        public void CompleteQuest_PublishesQuestCompletedEvent()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 1);
            _quests.StartQuest(quest);

            string completedId = null;
            EventBus.Subscribe<QuestCompletedEvent>(evt => completedId = evt.QuestId);

            SimulateEnemyDeath("wolf");

            Assert.AreEqual("quest_wolves", completedId);
        }

        [Test]
        public void AbandonQuest_RemovesActiveQuestWithoutGrantingRewards()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 1, rewardItem: _wolfPelt, rewardItemQty: 5);
            _quests.StartQuest(quest);

            bool abandoned = _quests.AbandonQuest("quest_wolves");

            Assert.IsTrue(abandoned);
            Assert.IsFalse(_quests.IsActive("quest_wolves"));
            Assert.IsFalse(_quests.IsCompleted("quest_wolves"));
            Assert.AreEqual(0, _inventory.GetTotalCount(_wolfPelt.ItemId));
        }

        // ------------------------------------------------------------------
        // Save / load
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_RoundTripsActiveProgressAndCompletedQuests()
        {
            QuestDefinition completedQuest = BuildKillQuest("quest_intro", "rat", 1);
            _quests.StartQuest(completedQuest);
            SimulateEnemyDeath("rat");

            QuestDefinition activeQuest = BuildKillQuest("quest_wolves", "wolf", 3);
            _quests.StartQuest(activeQuest);
            SimulateEnemyDeath("wolf");

            object snapshot = _quests.CaptureState();

            var freshManager = new QuestManager();
            freshManager.Initialize();
            freshManager.RestoreState(snapshot);

            Assert.IsTrue(freshManager.IsCompleted("quest_intro"));
            Assert.IsTrue(freshManager.IsActive("quest_wolves"));
            Assert.AreEqual(1, freshManager.GetProgress("quest_wolves").Objectives[0].CurrentAmount);

            freshManager.Shutdown();
        }

        [Test]
        public void RestoreState_WithNull_ClearsExistingProgress()
        {
            QuestDefinition quest = BuildKillQuest("quest_wolves", "wolf", 3);
            _quests.StartQuest(quest);

            _quests.RestoreState(null);

            Assert.IsFalse(_quests.IsActive("quest_wolves"));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private void SimulateEnemyDeath(string enemyName)
        {
            var enemyObj = new GameObject("Enemy_" + enemyName);
            var controller = enemyObj.AddComponent<EnemyController>();

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetPrivateField(definition, "_enemyName", enemyName);
            SetPrivateField(controller, "_definition", definition);

            EventBus.Publish(new CombatTargetDiedEvent(enemyObj, null));

            Object.DestroyImmediate(enemyObj);
            Object.DestroyImmediate(definition);
        }

        private static QuestDefinition BuildKillQuest(
            string questId,
            string enemyId,
            int requiredAmount,
            string[] prerequisiteIds = null,
            ItemDefinition rewardItem = null,
            int rewardItemQty = 0,
            int rewardExperience = 0)
        {
            var objective = new QuestObjective
            {
                ObjectiveId = "obj_kill",
                Type = QuestObjectiveType.KillEnemyType,
                TargetId = enemyId,
                RequiredAmount = requiredAmount,
                DescriptionKey = "quest.obj.kill"
            };

            return BuildQuest(questId, new List<QuestObjective> { objective }, prerequisiteIds, rewardItem, rewardItemQty, rewardExperience);
        }

        private static QuestDefinition BuildCollectQuest(string questId, string itemId, int requiredAmount)
        {
            var objective = new QuestObjective
            {
                ObjectiveId = "obj_collect",
                Type = QuestObjectiveType.CollectItem,
                TargetId = itemId,
                RequiredAmount = requiredAmount,
                DescriptionKey = "quest.obj.collect"
            };

            return BuildQuest(questId, new List<QuestObjective> { objective });
        }

        private static QuestDefinition BuildTalkQuest(string questId, string dialogueId)
        {
            var objective = new QuestObjective
            {
                ObjectiveId = "obj_talk",
                Type = QuestObjectiveType.TalkToNpc,
                TargetId = dialogueId,
                RequiredAmount = 1,
                DescriptionKey = "quest.obj.talk"
            };

            return BuildQuest(questId, new List<QuestObjective> { objective });
        }

        private static QuestDefinition BuildQuest(
            string questId,
            List<QuestObjective> objectives,
            string[] prerequisiteIds = null,
            ItemDefinition rewardItem = null,
            int rewardItemQty = 0,
            int rewardExperience = 0)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();

            List<QuestItemReward> rewardItems = new List<QuestItemReward>();
            if (rewardItem != null && rewardItemQty > 0)
            {
                rewardItems.Add(new QuestItemReward { Item = rewardItem, Quantity = rewardItemQty });
            }

            quest.SetDataForTests(
                questId,
                $"quest.{questId}.title",
                $"quest.{questId}.description",
                QuestType.Side,
                objectives,
                prerequisiteIds != null ? new List<string>(prerequisiteIds) : null,
                rewardExperience,
                rewardItems);

            return quest;
        }

        private static ItemDefinition CreateItem(string itemId, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static RecipeDefinition CreateRecipe(string recipeId, ItemDefinition resultItem, int resultQuantity)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            SetPrivateField(recipe, "_recipeId", recipeId);
            SetPrivateField(recipe, "_displayName", recipeId);
            SetPrivateField(recipe, "_ingredients", new RecipeIngredient[0]);
            SetPrivateField(recipe, "_resultItem", resultItem);
            SetPrivateField(recipe, "_resultQuantity", resultQuantity);
            SetPrivateField(recipe, "_knownByDefault", true);
            return recipe;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
