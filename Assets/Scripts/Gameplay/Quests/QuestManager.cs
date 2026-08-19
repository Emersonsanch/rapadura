using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>Runtime state of one quest instance: which definition, which objectives (as mutable runtime clones) and whether it's complete.</summary>
    [Serializable]
    public class QuestProgress
    {
        public string QuestId;
        public List<QuestObjective> Objectives = new List<QuestObjective>();
        public bool IsCompleted;

        public bool AllObjectivesComplete => Objectives.Count > 0 && Objectives.All(o => o.IsComplete);
    }

    /// <summary>Plain-C# JSON-serializable snapshot persisted by <see cref="ISaveable"/>.</summary>
    [Serializable]
    public class QuestSaveState
    {
        public List<QuestProgress> ActiveQuests = new List<QuestProgress>();
        public List<string> CompletedQuestIds = new List<string>();
    }

    /// <summary>
    /// Top-level quest system: plain C# class implementing <see cref="IManager"/> and
    /// <see cref="ISaveable"/>, same shape as every other manager in this project
    /// (SaveManager, CraftingManager, DialogueManager...). Tracks active quest instances (each a
    /// runtime clone of a <see cref="QuestDefinition"/>'s objectives so progress doesn't mutate the
    /// shared asset), listens to existing EventBus events for automatic objective progression
    /// (<see cref="CombatTargetDiedEvent"/> → KillEnemyType, <see cref="RecipeCraftedEvent"/> and
    /// <see cref="InventoryChangedEvent"/> → CollectItem, <see cref="DialogueEndedEvent"/> →
    /// TalkToNpc), and publishes <see cref="QuestStartedEvent"/>/<see cref="QuestObjectiveUpdatedEvent"/>/
    /// <see cref="QuestCompletedEvent"/>/<see cref="QuestAbandonedEvent"/>.
    ///
    /// Like <c>CraftingManager</c>, this manager does not hold a hard reference to a particular
    /// player's <see cref="InventoryManager"/>/<see cref="PlayerStats"/> — reward granting and the
    /// CollectItem check take those as explicit parameters (<see cref="SetRewardTargets"/>), and the
    /// automatic-progression event handlers use whatever targets were last configured. Callers
    /// (Scene bootstrap) are expected to call <see cref="SetRewardTargets"/> once the player's
    /// components exist.
    ///
    /// NOTE: this class is intentionally NOT wired into GameManager.cs by this change (per task
    /// instructions, GameManager.cs is not to be edited here) — a follow-up edit needs to add,
    /// inside <c>GameManager.BuildManagers()</c>:
    ///   QuestManager = new QuestManager();
    ///   RegisterManager(QuestManager);
    ///   SaveManager.Register(QuestManager);
    /// plus a public <c>QuestManager</c> property, mirroring DialogueManager/CraftingManager.
    /// </summary>
    public class QuestManager : IManager, ISaveable
    {
        private const string LogCategory = "Quests";

        private readonly Dictionary<string, QuestDefinition> _definitionsById = new Dictionary<string, QuestDefinition>();
        private readonly Dictionary<string, QuestProgress> _activeQuests = new Dictionary<string, QuestProgress>();
        private readonly HashSet<string> _completedQuestIds = new HashSet<string>();
        private readonly HashSet<string> _customFlags = new HashSet<string>();

        private InventoryManager _rewardInventory;
        private PlayerStats _rewardStats;

        public string SaveKey => "Quests";

        public IReadOnlyCollection<string> CompletedQuestIds => _completedQuestIds;

        public void Initialize()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Subscribe<RecipeCraftedEvent>(OnRecipeCrafted);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);

            GameLogger.Info(LogCategory, "QuestManager initialized.");
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Unsubscribe<RecipeCraftedEvent>(OnRecipeCrafted);
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);

            _definitionsById.Clear();
            _activeQuests.Clear();
            _completedQuestIds.Clear();
            _customFlags.Clear();
        }

        /// <summary>Registers a quest's definition so it can be looked up by id later (StartQuest, prerequisite checks, save/load restore).</summary>
        public void RegisterDefinition(QuestDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.QuestId))
            {
                return;
            }

            _definitionsById[definition.QuestId] = definition;
        }

        /// <summary>Sets which inventory/stats receive rewards and satisfy CollectItem checks. Call once the player's components exist (Scene bootstrap).</summary>
        public void SetRewardTargets(InventoryManager inventory, PlayerStats stats)
        {
            _rewardInventory = inventory;
            _rewardStats = stats;
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        public bool IsActive(string questId) => _activeQuests.ContainsKey(questId);

        public bool IsCompleted(string questId) => _completedQuestIds.Contains(questId);

        public QuestProgress GetProgress(string questId)
        {
            return _activeQuests.TryGetValue(questId, out QuestProgress progress) ? progress : null;
        }

        /// <summary>Returns true if every prerequisite quest id is already completed (or there are none).</summary>
        public bool ArePrerequisitesMet(QuestDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            foreach (string prerequisiteId in definition.PrerequisiteQuestIds)
            {
                if (!string.IsNullOrEmpty(prerequisiteId) && !_completedQuestIds.Contains(prerequisiteId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Starts a quest: fails if already active/completed, if the definition is invalid, or if
        /// prerequisite quests aren't complete yet. Publishes <see cref="QuestStartedEvent"/> on success.
        /// </summary>
        public bool StartQuest(QuestDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.QuestId))
            {
                GameLogger.Warning(LogCategory, "StartQuest called with an invalid QuestDefinition.");
                return false;
            }

            if (IsActive(definition.QuestId) || IsCompleted(definition.QuestId))
            {
                return false;
            }

            if (!ArePrerequisitesMet(definition))
            {
                GameLogger.Warning(LogCategory, $"StartQuest('{definition.QuestId}') blocked: prerequisites not met.");
                return false;
            }

            RegisterDefinition(definition);

            var progress = new QuestProgress
            {
                QuestId = definition.QuestId,
                Objectives = definition.Objectives.Select(o => o.Clone()).ToList(),
                IsCompleted = false
            };

            _activeQuests[definition.QuestId] = progress;

            EventBus.Publish(new QuestStartedEvent(definition.QuestId));

            // An objective might already be satisfied by state that predates the quest (e.g. the
            // player already carries the required item) — evaluate CollectItem immediately so the
            // quest doesn't demand the player drop and re-pick-up the item.
            EvaluateCollectItemObjectives(progress);

            return true;
        }

        /// <summary>Removes an active quest without completing it or granting rewards. Publishes <see cref="QuestAbandonedEvent"/>.</summary>
        public bool AbandonQuest(string questId)
        {
            if (!_activeQuests.Remove(questId))
            {
                return false;
            }

            EventBus.Publish(new QuestAbandonedEvent(questId));
            return true;
        }

        // ------------------------------------------------------------------
        // Objective progression
        // ------------------------------------------------------------------

        /// <summary>
        /// Adds <paramref name="amount"/> progress to every matching objective of every active
        /// quest (type + target id match). Publishes <see cref="QuestObjectiveUpdatedEvent"/> per
        /// updated objective, and auto-completes the quest once every objective is done.
        /// </summary>
        public void UpdateObjectiveProgress(QuestObjectiveType type, string targetId, int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            // Snapshot keys: CompleteQuest below mutates _activeQuests while we may still be iterating quests.
            foreach (string questId in _activeQuests.Keys.ToList())
            {
                QuestProgress progress = _activeQuests[questId];
                bool questNowComplete = false;

                foreach (QuestObjective objective in progress.Objectives)
                {
                    if (objective.Type != type || objective.IsComplete)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(objective.TargetId) && objective.TargetId != targetId)
                    {
                        continue;
                    }

                    objective.CurrentAmount = Math.Min(objective.RequiredAmount, objective.CurrentAmount + amount);

                    EventBus.Publish(new QuestObjectiveUpdatedEvent(
                        questId, objective.ObjectiveId, objective.CurrentAmount, objective.RequiredAmount, objective.IsComplete));
                }

                if (progress.AllObjectivesComplete)
                {
                    questNowComplete = true;
                }

                if (questNowComplete)
                {
                    CompleteQuest(questId);
                }
            }
        }

        /// <summary>Explicit setter for a single objective's progress (e.g. UI-driven or scripted event), rather than an increment.</summary>
        public void SetObjectiveProgress(string questId, string objectiveId, int amount)
        {
            if (!_activeQuests.TryGetValue(questId, out QuestProgress progress))
            {
                return;
            }

            QuestObjective objective = progress.Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);

            if (objective == null)
            {
                return;
            }

            objective.CurrentAmount = Math.Max(0, Math.Min(objective.RequiredAmount, amount));

            EventBus.Publish(new QuestObjectiveUpdatedEvent(
                questId, objective.ObjectiveId, objective.CurrentAmount, objective.RequiredAmount, objective.IsComplete));

            if (progress.AllObjectivesComplete)
            {
                CompleteQuest(questId);
            }
        }

        /// <summary>Sets a named custom flag and immediately progresses any active CustomFlag objective that targets it.</summary>
        public void SetCustomFlag(string flagName)
        {
            if (string.IsNullOrEmpty(flagName) || !_customFlags.Add(flagName))
            {
                return;
            }

            UpdateObjectiveProgress(QuestObjectiveType.CustomFlag, flagName, 1);
        }

        /// <summary>Reports a ReachLocation objective's location id as reached (called by a world trigger once one exists — see <see cref="QuestObjectiveType.ReachLocation"/>).</summary>
        public void ReportLocationReached(string locationId)
        {
            UpdateObjectiveProgress(QuestObjectiveType.ReachLocation, locationId, 1);
        }

        private void EvaluateCollectItemObjectives(QuestProgress progress)
        {
            if (_rewardInventory == null)
            {
                return;
            }

            foreach (QuestObjective objective in progress.Objectives)
            {
                if (objective.Type != QuestObjectiveType.CollectItem || objective.IsComplete || string.IsNullOrEmpty(objective.TargetId))
                {
                    continue;
                }

                int owned = _rewardInventory.GetTotalCount(objective.TargetId);

                if (owned <= 0)
                {
                    continue;
                }

                objective.CurrentAmount = Math.Min(objective.RequiredAmount, owned);

                EventBus.Publish(new QuestObjectiveUpdatedEvent(
                    progress.QuestId, objective.ObjectiveId, objective.CurrentAmount, objective.RequiredAmount, objective.IsComplete));
            }

            if (progress.AllObjectivesComplete)
            {
                CompleteQuest(progress.QuestId);
            }
        }

        // ------------------------------------------------------------------
        // Completion
        // ------------------------------------------------------------------

        /// <summary>Marks a quest complete (if all objectives are done), grants rewards against the configured reward targets, and publishes <see cref="QuestCompletedEvent"/>.</summary>
        public bool CompleteQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out QuestProgress progress))
            {
                return false;
            }

            if (!progress.AllObjectivesComplete)
            {
                GameLogger.Warning(LogCategory, $"CompleteQuest('{questId}') called before all objectives were done; ignored.");
                return false;
            }

            _activeQuests.Remove(questId);
            _completedQuestIds.Add(questId);
            progress.IsCompleted = true;

            GrantRewards(questId);

            EventBus.Publish(new QuestCompletedEvent(questId));
            return true;
        }

        private void GrantRewards(string questId)
        {
            if (!_definitionsById.TryGetValue(questId, out QuestDefinition definition) || definition == null)
            {
                return;
            }

            if (_rewardStats != null && definition.RewardExperience > 0)
            {
                _rewardStats.AddExperience(definition.RewardExperience);
            }

            if (_rewardInventory != null)
            {
                foreach (QuestItemReward itemReward in definition.RewardItems)
                {
                    if (itemReward?.Item != null && itemReward.Quantity > 0)
                    {
                        _rewardInventory.AddItem(itemReward.Item, itemReward.Quantity);
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // EventBus handlers — automatic objective progression.
        // ------------------------------------------------------------------

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            string enemyId = ResolveEnemyTargetId(evt.Target);

            if (!string.IsNullOrEmpty(enemyId))
            {
                UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, enemyId, 1);
            }
        }

        private static string ResolveEnemyTargetId(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            var controller = target.GetComponent<EnemyController>();
            string definitionName = controller != null ? controller.Definition?.EnemyName : null;

            return !string.IsNullOrEmpty(definitionName) ? definitionName : target.name;
        }

        private void OnRecipeCrafted(RecipeCraftedEvent evt)
        {
            if (evt.Recipe?.ResultItem != null)
            {
                UpdateObjectiveProgress(QuestObjectiveType.CollectItem, evt.Recipe.ResultItem.ItemId, evt.Recipe.ResultQuantity);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (_rewardInventory == null || _activeQuests.Count == 0)
            {
                return;
            }

            foreach (QuestProgress progress in _activeQuests.Values.ToList())
            {
                EvaluateCollectItemObjectives(progress);
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.DialogueId))
            {
                UpdateObjectiveProgress(QuestObjectiveType.TalkToNpc, evt.DialogueId, 1);
            }
        }

        // ------------------------------------------------------------------
        // Persistence
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            return new QuestSaveState
            {
                ActiveQuests = _activeQuests.Values.Select(CloneProgress).ToList(),
                CompletedQuestIds = _completedQuestIds.ToList()
            };
        }

        public void RestoreState(object state)
        {
            _activeQuests.Clear();
            _completedQuestIds.Clear();

            if (state is not QuestSaveState saveState)
            {
                return;
            }

            foreach (QuestProgress progress in saveState.ActiveQuests)
            {
                if (progress == null || string.IsNullOrEmpty(progress.QuestId))
                {
                    continue;
                }

                _activeQuests[progress.QuestId] = CloneProgress(progress);
            }

            foreach (string questId in saveState.CompletedQuestIds)
            {
                if (!string.IsNullOrEmpty(questId))
                {
                    _completedQuestIds.Add(questId);
                }
            }
        }

        private static QuestProgress CloneProgress(QuestProgress source)
        {
            return new QuestProgress
            {
                QuestId = source.QuestId,
                IsCompleted = source.IsCompleted,
                Objectives = source.Objectives.Select(o => new QuestObjective
                {
                    ObjectiveId = o.ObjectiveId,
                    Type = o.Type,
                    TargetId = o.TargetId,
                    RequiredAmount = o.RequiredAmount,
                    DescriptionKey = o.DescriptionKey,
                    CurrentAmount = o.CurrentAmount
                }).ToList()
            };
        }
    }
}
