using System;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>How a modifier's value combines with the base stat.</summary>
    public enum ModifierApplication
    {
        Flat,
        PercentAdditive
    }

    /// <summary>
    /// Data-only description of a single buff or debuff effect a skill can apply.
    /// A skill's positive effects (buffs) and negative effects (debuffs) are both expressed
    /// with this same struct — the sign of <see cref="value"/> combined with <see cref="isDebuff"/>
    /// determines whether it helps or hurts the target. Kept as a plain serializable class so it
    /// shows up nicely in the Inspector and the custom Skill editor window.
    /// </summary>
    [Serializable]
    public class StatModifierDefinition
    {
        public string displayName = "New Effect";
        public StatType affectedStat = StatType.MoveSpeed;
        public ModifierApplication application = ModifierApplication.Flat;
        public float value = 0f;
        public float durationSeconds = 5f;
        public bool isDebuff = false;
        public bool ticksOverTime = false;
        public float tickIntervalSeconds = 1f;
        public float tickDamageOrHeal = 0f;
    }
}
