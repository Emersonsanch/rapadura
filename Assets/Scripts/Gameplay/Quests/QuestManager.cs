using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
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

        /// <summary>Completed quests whose rewards are still owed (completed while no Player existed, e.g. on the world map).</summary>
        public List<string> PendingRewardQuestIds = new List<string>();
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
    public class QuestManager : IManager, ISaveable, IResettable
    {
        private const string LogCategory = "Quests";

        private readonly Dictionary<string, QuestDefinition> _definitionsById = new Dictionary<string, QuestDefinition>();
        private readonly Dictionary<string, QuestProgress> _activeQuests = new Dictionary<string, QuestProgress>();
        private readonly HashSet<string> _completedQuestIds = new HashSet<string>();
        private readonly HashSet<string> _customFlags = new HashSet<string>();
        private readonly List<string> _pendingRewardQuestIds = new List<string>();

        private InventoryManager _rewardInventory;
        private PlayerStats _rewardStats;

        public string SaveKey => "Quests";

        public IReadOnlyCollection<string> CompletedQuestIds => _completedQuestIds;

        /// <summary>Completed quests whose rewards wait for a live Player (see <see cref="DeliverPendingRewards"/>).</summary>
        public IReadOnlyList<string> PendingRewardQuestIds => _pendingRewardQuestIds;

        /// <summary>
        /// True when some reward target is set and alive (Unity's == null also catches the destroyed Player
        /// of a scene that was unloaded). False on the world map, where no Player exists.
        /// </summary>
        public bool HasRewardTargets => _rewardInventory != null || _rewardStats != null;

        /// <summary>
        /// Whether a unique enemy (a region boss, keyed by its <c>EnemyDefinition</c> asset name) was already
        /// defeated. Null = the <see cref="DefeatedEnemyRegistry"/> registered in the ServiceLocator. Used to
        /// credit a KillEnemyType objective on a boss that died before the quest was taken: a boss is fought
        /// once (<c>RegionBossProgress</c>), so without this the Guardião da Ruína (Ícaro) and the Habitante
        /// Ancestral (convergence) could never be counted once their region was cleared.
        /// </summary>
        public Func<string, bool> DefeatedUniqueEnemyLookup { get; set; }

        public void Initialize()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Subscribe<RecipeCraftedEvent>(OnRecipeCrafted);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);

            GameLogger.Info(LogCategory, "QuestManager initialized.");
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Unsubscribe<RecipeCraftedEvent>(OnRecipeCrafted);
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);

            _definitionsById.Clear();
            _activeQuests.Clear();
            _completedQuestIds.Clear();
            _customFlags.Clear();
            _pendingRewardQuestIds.Clear();
        }

        /// <summary>
        /// New game / load: no active or completed quests, no custom flags. Registered definitions
        /// (content) and reward targets (the current scene's player) are kept.
        /// </summary>
        public void ResetForNewSession()
        {
            _activeQuests.Clear();
            _completedQuestIds.Clear();
            _customFlags.Clear();
            _pendingRewardQuestIds.Clear();
        }

        /// <summary>Registers a quest's definition so it can be looked up by id later (StartQuest, prerequisite checks, save/load restore).</summary>
        public void RegisterDefinition(QuestDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.QuestId))
            {
                return;
            }

            _definitionsById[definition.QuestId] = definition;

            if (_activeQuests.TryGetValue(definition.QuestId, out QuestProgress progress))
            {
                SyncProgressWithDefinition(progress, definition);
            }
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

            // Same idea for a boss that already fell (it never respawns): count it now.
            if (IsActive(definition.QuestId))
            {
                CreditDefeatedUniqueEnemies(progress);
            }

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

        /// <summary>
        /// Forgets a <see cref="QuestType.Daily"/> quest entirely — drops it from the active and the
        /// completed sets, no events, no rewards — so <see cref="StartQuest"/> accepts it again.
        /// Used by <c>DailyQuestService</c> on day rollover; every other quest stays one-shot
        /// (StartQuest keeps refusing completed quests). Refuses anything whose registered
        /// definition isn't Daily, so a story/side quest can never be reset by mistake.
        /// </summary>
        public bool ResetDailyQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)
                || !_definitionsById.TryGetValue(questId, out QuestDefinition definition)
                || definition == null
                || definition.QuestType != QuestType.Daily)
            {
                return false;
            }

            bool wasActive = _activeQuests.Remove(questId);
            bool wasCompleted = _completedQuestIds.Remove(questId);
            return wasActive || wasCompleted;
        }

        /// <summary>
        /// Forgets the given quests (active and completed, no events, no rewards) so they can be taken
        /// again — "Nova Jornada+" replays the region side quests (<c>NewGamePlusProgress</c>). Rewards
        /// still waiting for a live Player (<see cref="PendingRewardQuestIds"/>) stay owed. Returns how many were forgotten.
        /// </summary>
        public int ForgetQuests(IEnumerable<string> questIds)
        {
            int forgotten = 0;
            if (questIds == null)
            {
                return forgotten;
            }

            foreach (string questId in questIds)
            {
                if (string.IsNullOrEmpty(questId))
                {
                    continue;
                }

                bool wasActive = _activeQuests.Remove(questId);
                bool wasCompleted = _completedQuestIds.Remove(questId);
                if (wasActive || wasCompleted)
                {
                    forgotten++;
                }
            }

            return forgotten;
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

        /// <summary>
        /// How many more units of <paramref name="itemId"/> the active quests still need while the hero holds
        /// <paramref name="currentlyHeld"/>: the largest RequiredAmount among unfinished CollectItem objectives on
        /// that item, minus what is held (CollectItem counts what the bag holds, so one stack serves every quest at
        /// once). 0 when no active quest wants it. <c>EnemyLootGranter</c> lets this much past the weight limit, so
        /// a full bag never eats a quest item (Docs/Balance/README.md §9).
        /// </summary>
        public int GetOutstandingCollectAmount(string itemId, int currentlyHeld)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0;
            }

            int required = 0;
            foreach (QuestProgress progress in _activeQuests.Values)
            {
                foreach (QuestObjective objective in progress.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.CollectItem && !objective.IsComplete && objective.TargetId == itemId)
                    {
                        required = Math.Max(required, Math.Max(1, objective.RequiredAmount));
                    }
                }
            }

            return Math.Max(0, required - Math.Max(0, currentlyHeld));
        }

        /// <summary>
        /// Credits every unfinished KillEnemyType objective of <paramref name="progress"/> whose target is a unique
        /// enemy already defeated (<see cref="DefeatedUniqueEnemyLookup"/>) and still at 0. Idempotent; a boss counts
        /// once, which is why boss objectives must ask for 1 kill (<c>QuestReachabilityTests</c>).
        /// </summary>
        private void CreditDefeatedUniqueEnemies(QuestProgress progress)
        {
            if (progress == null)
            {
                return;
            }

            bool credited = false;
            foreach (QuestObjective objective in progress.Objectives)
            {
                if (objective.Type != QuestObjectiveType.KillEnemyType || objective.IsComplete || objective.CurrentAmount > 0
                    || !IsUniqueEnemyDefeated(objective.TargetId))
                {
                    continue;
                }

                objective.CurrentAmount = 1;
                credited = true;
                EventBus.Publish(new QuestObjectiveUpdatedEvent(
                    progress.QuestId, objective.ObjectiveId, objective.CurrentAmount, objective.RequiredAmount, objective.IsComplete));
            }

            if (credited && progress.AllObjectivesComplete)
            {
                CompleteQuest(progress.QuestId);
            }
        }

        private bool IsUniqueEnemyDefeated(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return false;
            }

            if (DefeatedUniqueEnemyLookup != null)
            {
                return DefeatedUniqueEnemyLookup(enemyId);
            }

            return ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(enemyId);
        }

        /// <summary>
        /// A save keeps each active quest's objectives as they were when it was taken. When the definition changed
        /// since (quest_joaquim_echoes_of_home once asked for 6 kills of a boss that is fought once), the saved copy
        /// would stay unfinishable. Rebuilds the objectives from <paramref name="definition"/>, keeping the progress
        /// of every objective whose id, type and target are unchanged (clamped to the new amount). No-op when they match.
        /// </summary>
        private static void SyncProgressWithDefinition(QuestProgress progress, QuestDefinition definition)
        {
            if (progress == null || definition == null || definition.Objectives == null || definition.Objectives.Count == 0)
            {
                return;
            }

            bool same = progress.Objectives != null && progress.Objectives.Count == definition.Objectives.Count;
            for (int i = 0; same && i < definition.Objectives.Count; i++)
            {
                QuestObjective saved = progress.Objectives[i];
                QuestObjective authored = definition.Objectives[i];
                same = saved != null && authored != null
                       && saved.ObjectiveId == authored.ObjectiveId
                       && saved.Type == authored.Type
                       && saved.TargetId == authored.TargetId
                       && saved.RequiredAmount == Math.Max(1, authored.RequiredAmount)
                       && saved.DescriptionKey == authored.DescriptionKey;
            }

            if (same)
            {
                return;
            }

            var savedById = new Dictionary<string, QuestObjective>();
            foreach (QuestObjective saved in progress.Objectives ?? new List<QuestObjective>())
            {
                if (saved != null && !string.IsNullOrEmpty(saved.ObjectiveId) && !savedById.ContainsKey(saved.ObjectiveId))
                {
                    savedById[saved.ObjectiveId] = saved;
                }
            }

            var rebuilt = new List<QuestObjective>();
            foreach (QuestObjective authored in definition.Objectives)
            {
                if (authored == null)
                {
                    continue;
                }

                QuestObjective copy = authored.Clone();
                if (!string.IsNullOrEmpty(copy.ObjectiveId) && savedById.TryGetValue(copy.ObjectiveId, out QuestObjective saved)
                    && saved.Type == copy.Type && saved.TargetId == copy.TargetId)
                {
                    copy.CurrentAmount = Math.Min(copy.RequiredAmount, Math.Max(0, saved.CurrentAmount));
                }

                rebuilt.Add(copy);
            }

            progress.Objectives = rebuilt;
            GameLogger.Info(LogCategory, $"Quest '{progress.QuestId}': saved objectives updated to the current definition.");
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            foreach (QuestProgress progress in _activeQuests.Values.ToList())
            {
                if (_definitionsById.TryGetValue(progress.QuestId, out QuestDefinition definition))
                {
                    SyncProgressWithDefinition(progress, definition);
                }

                if (IsActive(progress.QuestId))
                {
                    CreditDefeatedUniqueEnemies(progress);
                }
            }
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

        /// <summary>
        /// Grants the rewards of quests completed while no live Player existed (the world map has none:
        /// the hero's InventoryManager/PlayerStats live in Main.unity), then re-checks CollectItem
        /// objectives against the now-live inventory (items already held never raise an
        /// InventoryChangedEvent). No-op without live reward targets. Call once the scene's Player has
        /// its carried state restored — GameManager does, right after SaveManager.RestoreCarriedState,
        /// so the restore doesn't overwrite what was just granted.
        /// </summary>
        public void DeliverPendingRewards()
        {
            if (!HasRewardTargets)
            {
                return;
            }

            if (_pendingRewardQuestIds.Count > 0)
            {
                List<string> pending = new List<string>(_pendingRewardQuestIds);
                _pendingRewardQuestIds.Clear();

                foreach (string questId in pending)
                {
                    GrantRewards(questId);
                }
            }

            foreach (QuestProgress progress in _activeQuests.Values.ToList())
            {
                EvaluateCollectItemObjectives(progress);
            }
        }

        private void GrantRewards(string questId)
        {
            if (!_definitionsById.TryGetValue(questId, out QuestDefinition definition) || definition == null)
            {
                return;
            }

            bool hasRewards = definition.RewardExperience > 0 || definition.RewardItems.Any(r => r?.Item != null && r.Quantity > 0);
            if (hasRewards && !HasRewardTargets)
            {
                // No live Player right now (world map): owe the rewards instead of dropping them.
                if (!_pendingRewardQuestIds.Contains(questId))
                {
                    _pendingRewardQuestIds.Add(questId);
                }

                GameLogger.Info(LogCategory, $"Quest '{questId}' completed without a live Player; rewards queued until one exists.");
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
                        int leftover = _rewardInventory.AddItem(itemReward.Item, itemReward.Quantity);
                        if (leftover > 0)
                        {
                            // No room on the Mochila's grid: the reward waits in the Baú da Vó Edna instead of vanishing.
                            VillageChestStorage.ResolveShared().EnqueueOverflow(new InventorySlotData
                            {
                                itemId = itemReward.Item.ItemId,
                                quantity = leftover,
                                currentDurability = itemReward.Item.HasDurability ? itemReward.Item.MaxDurability : 0f,
                            });
                        }
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

        /// <summary>
        /// Resolves the id used to match a killed enemy against a KillEnemyType objective's
        /// <see cref="QuestObjective.TargetId"/>. Every quest asset in the project sets TargetId to
        /// the EnemyDefinition's asset name (e.g. "enemy_desert_ruin_guardian" — confirmed against
        /// all 6 quest assets), not its human-readable <see cref="EnemyDefinition.EnemyName"/>
        /// (e.g. "Guardião da Ruína"). Using EnemyName here — the bug this replaces — meant
        /// KillEnemyType objectives could never progress from a real kill; the only way to advance
        /// them was forcing QuestObjective.CurrentAmount directly. <c>Definition.name</c> is
        /// UnityEngine.Object's built-in asset name, which is exactly the id format quests use
        /// (confirmed: a loaded EnemyDefinition's .name equals its Resources path filename).
        /// </summary>
        private static string ResolveEnemyTargetId(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            var controller = target.GetComponent<EnemyController>();
            string definitionAssetName = controller != null && controller.Definition != null ? controller.Definition.name : null;

            return !string.IsNullOrEmpty(definitionAssetName) ? definitionAssetName : target.name;
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

        /// <summary>
        /// Same class of id-mismatch bug as <see cref="ResolveEnemyTargetId"/>: every seeded
        /// TalkToNpc objective's TargetId uses an "npc_" prefix (e.g. "npc_village_survivor"), but
        /// <see cref="DialogueEndedEvent.DialogueId"/> is always the dialogue asset's own id, which
        /// uses a "dialogue_" prefix ("dialogue_village_survivor") — confirmed across all seeded
        /// dialogue/quest pairs in the project. Matching on the raw DialogueId alone (the bug this
        /// replaces) meant no TalkToNpc objective could ever progress from a real conversation.
        /// Progress is published under both the raw id and the "npc_"-normalized id so this keeps
        /// working regardless of which convention a given quest asset happens to use.
        /// </summary>
        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            if (string.IsNullOrEmpty(evt.DialogueId))
            {
                return;
            }

            UpdateObjectiveProgress(QuestObjectiveType.TalkToNpc, evt.DialogueId, 1);

            const string DialoguePrefix = "dialogue_";
            if (evt.DialogueId.StartsWith(DialoguePrefix))
            {
                string npcId = "npc_" + evt.DialogueId.Substring(DialoguePrefix.Length);
                UpdateObjectiveProgress(QuestObjectiveType.TalkToNpc, npcId, 1);
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
                CompletedQuestIds = _completedQuestIds.ToList(),
                PendingRewardQuestIds = _pendingRewardQuestIds.ToList()
            };
        }

        public void RestoreState(object state)
        {
            _activeQuests.Clear();
            _completedQuestIds.Clear();
            _pendingRewardQuestIds.Clear();

            if (state is not QuestSaveState saveState)
            {
                return;
            }

            if (saveState.PendingRewardQuestIds != null)
            {
                _pendingRewardQuestIds.AddRange(saveState.PendingRewardQuestIds.Where(id => !string.IsNullOrEmpty(id)).Distinct());
            }

            foreach (QuestProgress progress in saveState.ActiveQuests)
            {
                if (progress == null || string.IsNullOrEmpty(progress.QuestId))
                {
                    continue;
                }

                QuestProgress restored = CloneProgress(progress);
                if (_definitionsById.TryGetValue(restored.QuestId, out QuestDefinition definition))
                {
                    SyncProgressWithDefinition(restored, definition);
                }

                _activeQuests[restored.QuestId] = restored;
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
