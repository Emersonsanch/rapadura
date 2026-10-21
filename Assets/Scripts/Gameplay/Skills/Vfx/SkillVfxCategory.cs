namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>
    /// Visual archetype of a skill, derived from its <see cref="SkillDefinition"/> data by
    /// <see cref="SkillVfxClassifier"/>. Drives the spawn pattern (travelling projectile, ground
    /// burst, self aura...) while the resolved <see cref="SkillVfxPreset"/> drives the look
    /// (colors, particle size/speed). Kept separate from <see cref="SkillCategory"/>, which is a
    /// UI grouping and doesn't say anything about how a skill should look.
    /// </summary>
    public enum SkillVfxCategory
    {
        Projectile,
        Area,
        Heal,
        Buff,
        Debuff,
        Summon,
        Ultimate
    }
}
