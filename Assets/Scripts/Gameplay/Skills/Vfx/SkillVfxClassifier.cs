namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>
    /// Derives a <see cref="SkillVfxCategory"/> purely from existing <see cref="SkillDefinition"/>
    /// data, so none of the 45 seeded skill assets need a new "visual type" field. Pure/static so it
    /// is trivially EditMode-testable.
    /// </summary>
    public static class SkillVfxClassifier
    {
        /// <summary>Skills with at least this range and an area radius no bigger than <see cref="ProjectileMaxSplashRadius"/> read as a thrown/fired projectile.</summary>
        public const float ProjectileMinRange = 8f;

        /// <summary>Largest splash radius that still reads as "a projectile that explodes" rather than "a ground-targeted area".</summary>
        public const float ProjectileMaxSplashRadius = 3f;

        public static SkillVfxCategory Classify(SkillDefinition skill)
        {
            if (skill == null)
            {
                return SkillVfxCategory.Projectile;
            }

            return Classify(
                skill.SkillType,
                skill.Category,
                skill.BaseDamage,
                skill.BaseHealing,
                skill.Range,
                skill.AreaRadius,
                skill.Buffs != null ? skill.Buffs.Length : 0,
                skill.Debuffs != null ? skill.Debuffs.Length : 0);
        }

        public static SkillVfxCategory Classify(SkillType type, SkillCategory category, float damage, float healing, float range, float areaRadius, int buffCount, int debuffCount)
        {
            if (type == SkillType.Ultimate)
            {
                return SkillVfxCategory.Ultimate;
            }

            if (category == SkillCategory.Summon)
            {
                return SkillVfxCategory.Summon;
            }

            if (damage <= 0f)
            {
                if (healing > 0f)
                {
                    return SkillVfxCategory.Heal;
                }

                if (buffCount <= 0 && debuffCount > 0)
                {
                    return SkillVfxCategory.Debuff;
                }

                // Pure self-buffs, shields, mobility (dash/teleport/jump) all read as an aura on the caster.
                return SkillVfxCategory.Buff;
            }

            if (areaRadius <= 0f || (range >= ProjectileMinRange && areaRadius <= ProjectileMaxSplashRadius))
            {
                return SkillVfxCategory.Projectile;
            }

            return SkillVfxCategory.Area;
        }

        /// <summary>True when the category's spawn pattern travels from the caster to the target point.</summary>
        public static bool Travels(SkillVfxCategory category)
        {
            return category == SkillVfxCategory.Projectile;
        }
    }
}
