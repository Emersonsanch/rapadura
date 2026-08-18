using System;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Runtime state for a skill a character has learned: its current level, accumulated
    /// experience and remaining cooldown. Wraps a <see cref="SkillDefinition"/> asset (the
    /// static data) without mutating it, so the same definition can be shared by every
    /// character that learns that skill.
    /// </summary>
    [Serializable]
    public class SkillInstance
    {
        public SkillDefinition Definition { get; }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public float CooldownRemaining { get; private set; }

        public bool IsOnCooldown => CooldownRemaining > 0f;
        public bool IsMaxLevel => Level >= Definition.MaxLevel;

        public SkillInstance(SkillDefinition definition)
        {
            Definition = definition;
        }

        public void TickCooldown(float deltaTime)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = UnityEngine.Mathf.Max(0f, CooldownRemaining - deltaTime);
            }
        }

        public void StartCooldown(float cooldownReductionPercent = 0f)
        {
            float multiplier = UnityEngine.Mathf.Clamp01(1f - cooldownReductionPercent);
            CooldownRemaining = Definition.CooldownSeconds * multiplier;
        }

        public void AddExperience(int amount)
        {
            if (IsMaxLevel || amount <= 0)
            {
                return;
            }

            Experience += amount;

            while (!IsMaxLevel && Experience >= Definition.ExperienceToUnlock && Definition.ExperienceToUnlock > 0)
            {
                Experience -= Definition.ExperienceToUnlock;
                Level++;
            }
        }

        public void SetLevel(int level, int experience)
        {
            Level = UnityEngine.Mathf.Clamp(level, 1, Definition.MaxLevel);
            Experience = experience;
        }

        /// <summary>Increments the level by one directly, used when a skill point is spent in the skill tree.</summary>
        public bool LevelUp()
        {
            if (IsMaxLevel)
            {
                return false;
            }

            Level++;
            return true;
        }
    }
}
