using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>Raised the instant a skill successfully begins casting — animation/VFX/audio listeners hook here.</summary>
    public readonly struct SkillCastEvent : IGameEvent
    {
        public readonly GameObject Caster;
        public readonly SkillDefinition Skill;
        public readonly Vector3 TargetPoint;

        public SkillCastEvent(GameObject caster, SkillDefinition skill, Vector3 targetPoint)
        {
            Caster = caster;
            Skill = skill;
            TargetPoint = targetPoint;
        }
    }

    /// <summary>Raised whenever a skill is newly learned.</summary>
    public readonly struct SkillLearnedEvent : IGameEvent
    {
        public readonly SkillDefinition Skill;

        public SkillLearnedEvent(SkillDefinition skill)
        {
            Skill = skill;
        }
    }

    /// <summary>Raised whenever a learned skill gains a level.</summary>
    public readonly struct SkillLeveledUpEvent : IGameEvent
    {
        public readonly SkillDefinition Skill;
        public readonly int NewLevel;

        public SkillLeveledUpEvent(SkillDefinition skill, int newLevel)
        {
            Skill = skill;
            NewLevel = newLevel;
        }
    }

    /// <summary>Raised whenever a skill is equipped into or removed from a hotbar slot.</summary>
    public readonly struct SkillEquippedChangedEvent : IGameEvent
    {
        public readonly int SlotIndex;
        public readonly SkillDefinition Skill;

        public SkillEquippedChangedEvent(int slotIndex, SkillDefinition skill)
        {
            SlotIndex = slotIndex;
            Skill = skill;
        }
    }

    /// <summary>Raised whenever a learned skill is respec'd (unlearned via <see cref="SkillManager.RespecSkill"/>/<see cref="SkillManager.ResetSkillPoints"/>), refunding its spent skill points.</summary>
    public readonly struct SkillRespecEvent : IGameEvent
    {
        public readonly SkillDefinition Skill;
        public readonly int PointsRefunded;

        public SkillRespecEvent(SkillDefinition skill, int pointsRefunded)
        {
            Skill = skill;
            PointsRefunded = pointsRefunded;
        }
    }
}
