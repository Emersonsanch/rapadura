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
            return ComputeDamage(info, defense, config, resistance: null);
        }

        /// <summary>
        /// Computes the final damage a hit should deal, given the target's flat defense stat and its
        /// (optional) elemental resistance table. Resistance is applied as a percentage multiplier on
        /// top of the existing armor-mitigation result — a target with 100%+ resistance to
        /// <see cref="DamageInfo.Element"/> is immune and takes zero damage, bypassing even
        /// <see cref="DamageBalanceConfig.MinimumDamage"/> (immunity means immunity).
        /// </summary>
        public static float ComputeDamage(DamageInfo info, float defense, DamageBalanceConfig config, ElementResistance resistance)
        {
            float resistanceFraction = resistance != null ? resistance.GetResistance(info.Element) : 0f;

            if (resistanceFraction >= 1f)
            {
                return 0f;
            }

            float armorConstant = config != null ? config.ArmorConstant : 100f;
            float minimumDamage = config != null ? config.MinimumDamage : 1f;
            float criticalMultiplier = config != null ? config.CriticalMultiplier : 1.5f;

            defense = Mathf.Max(0f, defense);
            float mitigation = defense / (defense + Mathf.Max(0.0001f, armorConstant));
            float mitigated = info.Amount * (1f - mitigation);
            mitigated *= (1f - resistanceFraction);

            if (info.IsCritical)
            {
                mitigated *= criticalMultiplier;
            }

            return Mathf.Max(minimumDamage, Mathf.Round(mitigated));
        }
    }
}
