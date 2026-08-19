using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Data-driven quest: id, localized title/description keys, an ordered list of objectives,
    /// rewards (XP/items), a broad <see cref="QuestType"/> (Principal/Secundária/Diária) and
    /// prerequisite quest ids. Authored as a ScriptableObject asset, same "data as asset" pattern
    /// as <c>DialogueDefinition</c>/<c>RecipeDefinition</c>/<c>SkillDefinition</c> — designers
    /// create/edit quests in the Inspector, and <see cref="QuestManager"/> contains no per-quest
    /// special-casing.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Rapadura/Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [SerializeField] private string _questId;
        [SerializeField] private string _titleKey;
        [SerializeField] private string _descriptionKey;
        [SerializeField] private QuestType _questType = QuestType.Side;
        [SerializeField] private List<QuestObjective> _objectives = new List<QuestObjective>();
        [SerializeField] private List<string> _prerequisiteQuestIds = new List<string>();

        [Header("Rewards")]
        [SerializeField] private int _rewardExperience;
        [SerializeField] private List<QuestItemReward> _rewardItems = new List<QuestItemReward>();

        public string QuestId => _questId;
        public string TitleKey => _titleKey;
        public string DescriptionKey => _descriptionKey;
        public QuestType QuestType => _questType;
        public IReadOnlyList<QuestObjective> Objectives => _objectives;
        public IReadOnlyList<string> PrerequisiteQuestIds => _prerequisiteQuestIds;
        public int RewardExperience => _rewardExperience;
        public IReadOnlyList<QuestItemReward> RewardItems => _rewardItems;

#if UNITY_EDITOR
        /// <summary>Editor/seeder-only property names, mirrors DialogueDefinition's pattern. Not used at runtime.</summary>
        public const string QuestIdPropertyName = nameof(_questId);
        public const string TitleKeyPropertyName = nameof(_titleKey);
        public const string DescriptionKeyPropertyName = nameof(_descriptionKey);
        public const string QuestTypePropertyName = nameof(_questType);
        public const string ObjectivesPropertyName = nameof(_objectives);
        public const string PrerequisiteQuestIdsPropertyName = nameof(_prerequisiteQuestIds);
        public const string RewardExperiencePropertyName = nameof(_rewardExperience);
        public const string RewardItemsPropertyName = nameof(_rewardItems);
#endif

        /// <summary>Plain-C# construction path for EditMode tests, which can't rely on AssetDatabase/CreateAsset.</summary>
        public void SetDataForTests(
            string questId,
            string titleKey,
            string descriptionKey,
            QuestType questType,
            List<QuestObjective> objectives,
            List<string> prerequisiteQuestIds = null,
            int rewardExperience = 0,
            List<QuestItemReward> rewardItems = null)
        {
            _questId = questId;
            _titleKey = titleKey;
            _descriptionKey = descriptionKey;
            _questType = questType;
            _objectives = objectives ?? new List<QuestObjective>();
            _prerequisiteQuestIds = prerequisiteQuestIds ?? new List<string>();
            _rewardExperience = rewardExperience;
            _rewardItems = rewardItems ?? new List<QuestItemReward>();
        }
    }
}
