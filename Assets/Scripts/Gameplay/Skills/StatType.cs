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
        ResourceCostReduction,
        // Appended values only (never renumber): saves, assets and DamageChannel.ForDamageOverTime
        // store StatType as its int value.
        /// <summary>
        /// Fraction (0.1 = +10%) added to basic-attack speed: the swing cooldown is divided by
        /// 1 + total (see <see cref="Rapadura.Gameplay.Player.PlayerBasicAttack.ResolveCooldown"/>).
        /// </summary>
        AttackSpeed,
        /// <summary>
        /// Fraction (0.1 = 10%) of the melee damage the owner takes that is reflected back to the
        /// attacker (see <see cref="Rapadura.Gameplay.Combat.ThornsRules"/>). Reflected damage never reflects again.
        /// </summary>
        Thorns
    }
}
