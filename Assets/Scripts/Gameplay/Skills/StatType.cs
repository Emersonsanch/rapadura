namespace Rapadura.Gameplay.Skills
{
    /// <summary>Player/NPC stat that a buff or debuff can modify.</summary>
    public enum StatType
    {
        MaxHealth,
        HealthRegen,
        MaxStamina,
        StaminaRegen,
        MaxMana,
        ManaRegen,
        MoveSpeed,
        AttackDamage,
        Defense,
        CriticalChance,
        CriticalDamage,
        CooldownReduction,
        /// <summary>Percent reduction applied to mana/energy costs when casting skills (see <see cref="SkillManager"/>).</summary>
        ResourceCostReduction
    }
}
