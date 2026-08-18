using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Pure, side-effect-free damage formula shared by every hitbox/skill/hazard in the game.
    /// Keeping the math in one static class (instead of scattering "damage - defense" everywhere)
    /// means the formula only needs to be tuned/tested in one place.
    ///
    /// <para><b>Formula</b> (mitigation curve, same shape as League/Dota-style armor):</para>
    /// <code>
    /// mitigation   = defense / (defense + armorConstant)      // in [0, 1)
    /// mitigated    = amount * (1 - mitigation)
    /// critApplied  = isCritical ? mitigated * criticalMultiplier : mitigated
    /// finalDamage  = max(minimumDamage, round(critApplied))
    /// </code>
    /// A defense of 0 deals full damage; defense equal to <see cref="DamageBalanceConfig.ArmorConstant"/>
    /// halves incoming damage; defense trends towards (but never reaches) 100% mitigation, and
    /// <see cref="DamageBalanceConfig.MinimumDamage"/> guarantees a hit is never a total no-op.
    /// Element vs. resistance/weakness multipliers are intentionally out of scope here — that
    /// belongs to the future FASE 2 "Efeitos" resistance system, which can layer on top of this
    /// result without changing this formula.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>Computes the final damage a hit should deal, given the target's flat defense stat.</summary>
        public static float ComputeDamage(DamageInfo info, float defense, DamageBalanceConfig config)
        {
            float armorConstant = config != null ? config.ArmorConstant : 100f;
            float minimumDamage = config != null ? config.MinimumDamage : 1f;
            float criticalMultiplier = config != null ? config.CriticalMultiplier : 1.5f;

            defense = Mathf.Max(0f, defense);
            float mitigation = defense / (defense + Mathf.Max(0.0001f, armorConstant));
            float mitigated = info.Amount * (1f - mitigation);

            if (info.IsCritical)
            {
                mitigated *= criticalMultiplier;
            }

            return Mathf.Max(minimumDamage, Mathf.Round(mitigated));
        }
    }
}
