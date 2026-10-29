using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// Default <see cref="IDialogueContext"/>: bridges dialogue conditions/actions onto the existing
    /// <see cref="QuestManager"/> API (StartQuest/CompleteQuest/SetCustomFlag/IsActive/IsCompleted/
    /// GetProgress/ArePrerequisitesMet) and the selected hero (<see cref="CharacterSelectionState"/>).
    ///
    /// Both are resolved lazily on every call through the <see cref="ServiceLocator"/> by default —
    /// the same "never a hard reference" rule as NpcInteractable/DialogueUIController, and needed
    /// because GameManager may (re)build managers after this object exists. Tests pass their own
    /// providers instead.
    ///
    /// Dialogue data refers to quests by id, but <see cref="QuestManager.StartQuest"/> needs the
    /// <see cref="QuestDefinition"/>; definitions are looked up with <see cref="Resources.Load{T}(string)"/>
    /// at <c>Resources/Quests/&lt;questId&gt;</c> (every quest asset is named after its id) and cached.
    /// </summary>
    public class QuestDialogueContext : IDialogueContext
    {
        private const string LogCategory = "Dialogue";
        private const string QuestResourcesFolder = "Quests/";

        private readonly Func<QuestManager> _questManagerProvider;
        private readonly Func<string, QuestDefinition> _definitionResolver;
        private readonly Func<string> _activeCharacterProvider;
        private readonly Dictionary<string, QuestDefinition> _definitionCache = new Dictionary<string, QuestDefinition>();

        /// <summary>Runtime default: everything from the ServiceLocator / Resources.</summary>
        public QuestDialogueContext()
            : this(null, null, null)
        {
        }

        /// <param name="questManagerProvider">Null = <c>ServiceLocator.TryGet&lt;QuestManager&gt;</c>.</param>
        /// <param name="definitionResolver">Null = <c>Resources.Load&lt;QuestDefinition&gt;("Quests/" + id)</c>.</param>
        /// <param name="activeCharacterProvider">Null = <c>ServiceLocator.TryGet&lt;CharacterSelectionState&gt;</c>'s selection name.</param>
        public QuestDialogueContext(
            Func<QuestManager> questManagerProvider,
            Func<string, QuestDefinition> definitionResolver,
            Func<string> activeCharacterProvider)
        {
            _questManagerProvider = questManagerProvider ?? ResolveQuestManagerFromServices;
            _definitionResolver = definitionResolver ?? LoadDefinitionFromResources;
            _activeCharacterProvider = activeCharacterProvider ?? ResolveActiveCharacterFromServices;
        }

        public string ActiveCharacterId => _activeCharacterProvider();

        public DialogueQuestState GetQuestState(string questId)
        {
            QuestManager quests = _questManagerProvider();

            if (quests == null || string.IsNullOrEmpty(questId))
            {
                return DialogueQuestState.NotStarted;
            }

            if (quests.IsCompleted(questId))
            {
                return DialogueQuestState.Completed;
            }

            if (quests.IsActive(questId))
            {
                QuestProgress progress = quests.GetProgress(questId);
                return progress != null && progress.AllObjectivesComplete ? DialogueQuestState.ObjectivesComplete : DialogueQuestState.Active;
            }

            QuestDefinition definition = GetDefinition(questId);
            return definition != null && quests.ArePrerequisitesMet(definition) ? DialogueQuestState.Available : DialogueQuestState.NotStarted;
        }

        public bool IsQuestObjectiveComplete(string questId, string objectiveId)
        {
            QuestManager quests = _questManagerProvider();

            if (quests == null || string.IsNullOrEmpty(questId))
            {
                return false;
            }

            if (quests.IsCompleted(questId))
            {
                return true;
            }

            QuestProgress progress = quests.GetProgress(questId);
            QuestObjective objective = progress?.Objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            return objective != null && objective.IsComplete;
        }

        public bool StartQuest(string questId)
        {
            QuestManager quests = _questManagerProvider();
            QuestDefinition definition = GetDefinition(questId);

            if (quests == null || definition == null)
            {
                GameLogger.Warning(LogCategory, $"Dialogue action StartQuest('{questId}') skipped: " + (quests == null ? "no QuestManager registered." : "quest definition not found."));
                return false;
            }

            return quests.StartQuest(definition);
        }

        public bool CompleteQuest(string questId)
        {
            QuestManager quests = _questManagerProvider();
            return quests != null && !string.IsNullOrEmpty(questId) && quests.CompleteQuest(questId);
        }

        public void SetQuestFlag(string flagName)
        {
            _questManagerProvider()?.SetCustomFlag(flagName);
        }

        private QuestDefinition GetDefinition(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return null;
            }

            if (_definitionCache.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = _definitionResolver(questId);

            if (definition != null)
            {
                _definitionCache[questId] = definition;
            }

            return definition;
        }

        private static QuestManager ResolveQuestManagerFromServices()
        {
            return ServiceLocator.TryGet(out QuestManager quests) ? quests : null;
        }

        private static QuestDefinition LoadDefinitionFromResources(string questId)
        {
            QuestDefinition asset = Resources.Load<QuestDefinition>(QuestResourcesFolder + questId);

            // Region side quests are defined in code (RegionQuestCatalog), not as Resources assets.
            return asset != null ? asset : RegionQuestCatalog.GetQuest(questId);
        }

        private static string ResolveActiveCharacterFromServices()
        {
            return ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null && selection.Selected.HasValue
                ? selection.Selected.Value.ToString()
                : null;
        }
    }
}
