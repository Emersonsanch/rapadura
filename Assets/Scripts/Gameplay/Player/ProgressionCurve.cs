using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// One row of the player progression table (one per level). Mirrors one line of
    /// <c>Docs/Balance/progression.csv</c> — see <see cref="ProgressionCsv"/>.
    /// </summary>
    [Serializable]
    public class ProgressionLevelRow
    {
        [Tooltip("Player level this row describes (1-based).")]
        public int level = 1;

        [Tooltip("Experience needed to go from this level to the next one.")]
        public int experienceToNext = 100;

        [Tooltip("Max health at this level. Only the difference to level 1 is applied on top of PlayerStats' own base value.")]
        public float maxHealth = 100f;

        [Tooltip("Max mana at this level. Only the difference to level 1 is applied on top of PlayerStats' own base value.")]
        public float maxMana = 100f;

        [Tooltip("Max stamina at this level. Only the difference to level 1 is applied on top of PlayerStats' own base value.")]
        public float maxStamina = 100f;

        [Tooltip("Multiplier applied to outgoing skill damage at this level (1 = no change).")]
        public float damageMultiplier = 1f;

        [Tooltip("Attribute points granted when REACHING this level (level 1 always grants 0).")]
        public int attributePoints;

        [Tooltip("Skill points granted when REACHING this level (level 1 always grants 0).")]
        public int skillPoints;

        public ProgressionLevelRow Clone()
        {
            return (ProgressionLevelRow)MemberwiseClone();
        }
    }

    /// <summary>
    /// Single source of truth for the player's level curve: XP per level, max health/mana/stamina
    /// growth, outgoing damage growth and attribute/skill points granted per level. Replaces the
    /// formula that used to be hardcoded in <see cref="PlayerStats"/>
    /// (<c>base * multiplier^(level-1)</c>), <see cref="AttributeSet"/> (2 points/level) and
    /// <c>SkillManager</c> (1 point/level).
    ///
    /// Two layers:
    /// <list type="bullet">
    /// <item>Formula parameters (the old hardcoded numbers) — used to (re)generate the table and as the
    /// fallback for any level the table doesn't cover (e.g. past level 50).</item>
    /// <item>An explicit per-level table (<see cref="Rows"/>) that designers edit through the spreadsheet
    /// <c>Docs/Balance/progression.csv</c> and re-import via <c>Rapadura &gt; Balance &gt; Import Progression CSV</c>.</item>
    /// </list>
    /// With the default values (and growth = 0) every number is identical to the old hardcoded behaviour.
    /// The shipped asset lives at <c>Assets/Resources/Balance/ProgressionCurve.asset</c> and is picked up
    /// automatically through <see cref="LoadDefault"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionCurve", menuName = "Rapadura/Balance/Progression Curve")]
    public class ProgressionCurve : ScriptableObject
    {
        /// <summary>Path (relative to a Resources folder) of the shipped curve.</summary>
        public const string ResourcesPath = "Balance/ProgressionCurve";

        /// <summary>Number of levels the exported table covers.</summary>
        public const int DefaultMaxLevel = 50;

        [Header("Formula (fallback + table generator)")]
        [SerializeField] private int _baseExperienceToLevel = 100;
        [SerializeField] private float _experienceCurveMultiplier = 1.25f;
        [SerializeField] private float _baseMaxHealth = 100f;
        [SerializeField] private float _maxHealthPerLevel = 0f;
        [SerializeField] private float _baseMaxMana = 100f;
        [SerializeField] private float _maxManaPerLevel = 0f;
        [SerializeField] private float _baseMaxStamina = 100f;
        [SerializeField] private float _maxStaminaPerLevel = 0f;
        [SerializeField] private float _damageMultiplierPerLevel = 0f;
        [SerializeField] private int _attributePointsPerLevel = 2;
        [SerializeField] private int _skillPointsPerLevel = 1;

        [Header("Per-level table (imported from Docs/Balance/progression.csv)")]
        [SerializeField] private List<ProgressionLevelRow> _levels = new List<ProgressionLevelRow>();

        public int BaseExperienceToLevel => _baseExperienceToLevel;
        public float ExperienceCurveMultiplier => _experienceCurveMultiplier;
        public int AttributePointsPerLevel => _attributePointsPerLevel;
        public int SkillPointsPerLevel => _skillPointsPerLevel;

        /// <summary>The explicit table, ordered by level. May be empty (then the formula is used for every level).</summary>
        public IReadOnlyList<ProgressionLevelRow> Rows => _levels;

        /// <summary>Highest level covered by the explicit table (0 if the table is empty).</summary>
        public int TableMaxLevel => _levels.Count;

        /// <summary>Loads the shipped curve from Resources, or null if it doesn't exist.</summary>
        public static ProgressionCurve LoadDefault()
        {
            return Resources.Load<ProgressionCurve>(ResourcesPath);
        }

        /// <summary>
        /// Builds the row the formula parameters produce for a level. The XP expression is kept
        /// bit-for-bit identical to the old <c>PlayerStats.GetExperienceToNextLevel</c>.
        /// </summary>
        public ProgressionLevelRow BuildFormulaRow(int level)
        {
            level = Mathf.Max(1, level);
            int steps = level - 1;
            return new ProgressionLevelRow
            {
                level = level,
                experienceToNext = Mathf.RoundToInt(_baseExperienceToLevel * Mathf.Pow(_experienceCurveMultiplier, level - 1)),
                maxHealth = _baseMaxHealth + _maxHealthPerLevel * steps,
                maxMana = _baseMaxMana + _maxManaPerLevel * steps,
                maxStamina = _baseMaxStamina + _maxStaminaPerLevel * steps,
                damageMultiplier = 1f + _damageMultiplierPerLevel * steps,
                attributePoints = level <= 1 ? 0 : _attributePointsPerLevel,
                skillPoints = level <= 1 ? 0 : _skillPointsPerLevel
            };
        }

        /// <summary>Overwrites the explicit table with the formula's output for levels 1..maxLevel.</summary>
        public void RegenerateFromFormula(int maxLevel = DefaultMaxLevel)
        {
            _levels.Clear();
            for (int level = 1; level <= maxLevel; level++)
            {
                _levels.Add(BuildFormulaRow(level));
            }
        }

        /// <summary>
        /// Replaces the explicit table (e.g. from a CSV import). Rows are sorted by level and must form
        /// a contiguous 1..N sequence; throws <see cref="ArgumentException"/> otherwise so a broken
        /// spreadsheet never half-applies.
        /// </summary>
        public void SetRows(IEnumerable<ProgressionLevelRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var sorted = new List<ProgressionLevelRow>();
            foreach (ProgressionLevelRow row in rows)
            {
                if (row != null)
                {
                    sorted.Add(row.Clone());
                }
            }

            sorted.Sort((a, b) => a.level.CompareTo(b.level));

            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].level != i + 1)
                {
                    throw new ArgumentException($"Progression table must cover levels 1..N contiguously; expected level {i + 1} but found {sorted[i].level}.");
                }

                if (sorted[i].experienceToNext <= 0)
                {
                    throw new ArgumentException($"Level {sorted[i].level}: experienceToNext must be > 0.");
                }
            }

            _levels = sorted;
        }

        /// <summary>The table row for a level, or the formula row if the table doesn't cover it.</summary>
        public ProgressionLevelRow GetRow(int level)
        {
            level = Mathf.Max(1, level);
            return level <= _levels.Count ? _levels[level - 1] : BuildFormulaRow(level);
        }

        public int GetExperienceToNextLevel(int level) => GetRow(level).experienceToNext;

        public float GetMaxHealth(int level) => GetRow(level).maxHealth;

        public float GetMaxMana(int level) => GetRow(level).maxMana;

        public float GetMaxStamina(int level) => GetRow(level).maxStamina;

        public float GetDamageMultiplier(int level) => GetRow(level).damageMultiplier;

        /// <summary>Max health gained since level 1 (what <see cref="PlayerStats"/> adds to its own base).</summary>
        public float GetMaxHealthBonus(int level) => GetMaxHealth(level) - GetMaxHealth(1);

        /// <summary>Max mana gained since level 1.</summary>
        public float GetMaxManaBonus(int level) => GetMaxMana(level) - GetMaxMana(1);

        /// <summary>Max stamina gained since level 1.</summary>
        public float GetMaxStaminaBonus(int level) => GetMaxStamina(level) - GetMaxStamina(1);

        /// <summary>Total XP needed to reach <paramref name="level"/> from level 1 with 0 XP.</summary>
        public long GetTotalExperienceToReach(int level)
        {
            long total = 0;
            for (int l = 1; l < level; l++)
            {
                total += GetExperienceToNextLevel(l);
            }

            return total;
        }

        /// <summary>Attribute points granted when leveling from <paramref name="fromLevel"/> to <paramref name="toLevel"/>.</summary>
        public int GetAttributePointsGained(int fromLevel, int toLevel)
        {
            int total = 0;
            for (int l = Mathf.Max(1, fromLevel) + 1; l <= toLevel; l++)
            {
                total += GetRow(l).attributePoints;
            }

            return total;
        }

        /// <summary>Skill points granted when leveling from <paramref name="fromLevel"/> to <paramref name="toLevel"/>.</summary>
        public int GetSkillPointsGained(int fromLevel, int toLevel)
        {
            int total = 0;
            for (int l = Mathf.Max(1, fromLevel) + 1; l <= toLevel; l++)
            {
                total += GetRow(l).skillPoints;
            }

            return total;
        }
    }
}
