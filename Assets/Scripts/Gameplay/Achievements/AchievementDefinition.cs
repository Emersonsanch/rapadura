using UnityEngine;

namespace Rapadura.Gameplay.Achievements
{
    /// <summary>
    /// Data-driven description of one achievement ("Conquista"). Lives in <c>Resources/Achievements</c>
    /// and is loaded by <see cref="AchievementManager"/> with <c>Resources.LoadAll</c>, same approach as
    /// quests/recipes, so adding an achievement never needs a code change.
    ///
    /// Two progress modes, both evaluated from the manager's lifetime stats (so achievements added
    /// later unlock retroactively from already-recorded progress):
    /// <list type="bullet">
    /// <item><b>Counter</b> (<see cref="TargetIds"/> empty): unlocks once the stat for
    /// <see cref="ConditionType"/> reaches <see cref="TargetValue"/> (e.g. 100 enemies killed, level 10).</item>
    /// <item><b>Specific ids</b> (<see cref="TargetIds"/> set): unlocks once every listed id has been
    /// observed for that condition (e.g. both siblings' personal quests completed).</item>
    /// </list>
    /// Title/description are localization keys (see <c>AchievementTexts</c>); empty keys fall back to
    /// <c>achievement.&lt;id&gt;.title</c>/<c>.description</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "Rapadura/Achievements/Achievement Definition", order = 0)]
    public class AchievementDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _achievementId = "achievement_new";
        [Tooltip("Localization key for the title. Empty = achievement.<id>.title.")]
        [SerializeField] private string _titleKey = string.Empty;
        [Tooltip("Localization key for the description. Empty = achievement.<id>.description.")]
        [SerializeField] private string _descriptionKey = string.Empty;
        [Tooltip("Optional icon shown in the toast and the achievements list.")]
        [SerializeField] private Sprite _icon;

        [Header("Condition")]
        [SerializeField] private AchievementConditionType _conditionType = AchievementConditionType.EnemiesKilled;
        [Tooltip("Counter mode: stat value required. Ignored when Target Ids is set.")]
        [Min(1)]
        [SerializeField] private int _targetValue = 1;
        [Tooltip("Optional: specific ids (quest ids, recipe ids, BiomeType names...) that must ALL be observed.")]
        [SerializeField] private string[] _targetIds = System.Array.Empty<string>();

        [Header("Presentation")]
        [Tooltip("Hidden achievements show as '???' in the list until unlocked.")]
        [SerializeField] private bool _hidden;
        [Tooltip("Lower values are listed first.")]
        [SerializeField] private int _sortOrder;

        public string AchievementId => _achievementId;
        public string TitleKey => string.IsNullOrEmpty(_titleKey) ? $"achievement.{_achievementId}.title" : _titleKey;
        public string DescriptionKey => string.IsNullOrEmpty(_descriptionKey) ? $"achievement.{_achievementId}.description" : _descriptionKey;
        public Sprite Icon => _icon;
        public AchievementConditionType ConditionType => _conditionType;
        public string[] TargetIds => _targetIds ?? System.Array.Empty<string>();
        public bool UsesTargetIds => _targetIds != null && _targetIds.Length > 0;
        public bool Hidden => _hidden;
        public int SortOrder => _sortOrder;

        /// <summary>Progress needed to unlock: the id count in id mode, otherwise <see cref="_targetValue"/>.</summary>
        public int TargetValue => UsesTargetIds ? _targetIds.Length : Mathf.Max(1, _targetValue);

#if UNITY_EDITOR
        /// <summary>Editor/test-only setter so tests don't need SerializedObject plumbing.</summary>
        public void ConfigureForTests(string id, AchievementConditionType type, int targetValue, string[] targetIds = null, bool hidden = false)
        {
            _achievementId = id;
            _conditionType = type;
            _targetValue = targetValue;
            _targetIds = targetIds ?? System.Array.Empty<string>();
            _hidden = hidden;
        }
#endif
    }
}
