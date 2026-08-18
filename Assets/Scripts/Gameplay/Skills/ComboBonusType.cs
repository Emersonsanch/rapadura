namespace Rapadura.Gameplay.Skills
{
    /// <summary>What kind of reward a completed <see cref="ComboDefinition"/> grants.</summary>
    public enum ComboBonusType
    {
        /// <summary>Grants a temporary percent bonus to outgoing skill damage (applied as an <see cref="StatType.AttackDamage"/> buff).</summary>
        DamageMultiplier,
        /// <summary>Grants a temporary percent reduction to mana/energy costs (applied as a <see cref="StatType.ResourceCostReduction"/> buff).</summary>
        CostReduction,
        /// <summary>Instantly unlocks (learns) <see cref="ComboDefinition.UnlockSkill"/> for free, bypassing skill point cost and level requirements.</summary>
        UnlockSkill
    }
}
