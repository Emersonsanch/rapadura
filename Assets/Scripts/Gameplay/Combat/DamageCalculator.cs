using Rapadura.Gameplay.Skills;
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
    /// mitigation   = defense / (defense + armorConstant)      // in [0, 1) — flat armor points
    /// mitigated    = amount * (1 - mitigation)
    /// mitigated   *= 1 - clamp(percentDefense, -1, 0.8)       // skill buffs/debuffs (+0.25 = 25% less damage)
    /// critApplied  = isCritical ? mitigated * criticalMultiplier : mitigated
    /// finalDamage  = max(minimumDamage, round(critApplied))
    /// </code>
    /// A defense of 0 deals full damage; defense equal to <see cref="DamageBalanceConfig.ArmorConstant"/>
    /// halves incoming damage; defense trends towards (but never reaches) 100% mitigation, and
    /// <see cref="DamageBalanceConfig.MinimumDamage"/> guarantees a hit is never a total no-op.
    ///
    /// <para><b>Two kinds of Defense</b> (see <c>Docs/Balance/README.md</c> §4): the item affixes add
    /// <i>flat</i> armor points (<see cref="ModifierApplication.Flat"/>, +2..+10) that go through the
    /// armor curve above; every skill expresses Defense as <see cref="ModifierApplication.PercentAdditive"/>
    /// (Campo Magnético +0.25, Muralha +0.6, Drone -0.2, Berserk -0.3...), which is read as a direct
    /// percent change of damage taken. Percent sources add up and are clamped to
    /// [<see cref="MinPercentDefense"/>, <see cref="MaxPercentDefense"/>]: buffs stacking can never go
    /// past 80% reduction, debuffs stacking at most double the damage taken.</para>
    /// Both the player (<c>PlayerStats</c>) and every <see cref="Health"/> actor resolve their
    /// defense through <see cref="GetDefense"/> + <see cref="GetDefenseMultiplier"/>.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>Cap of the summed percent Defense buffs: 0.8 = at most 80% less damage (Última Resistência alone).</summary>
        public const float MaxPercentDefense = 0.8f;

        /// <summary>Floor of the summed percent Defense debuffs: -1 = at most double damage taken.</summary>
        public const float MinPercentDefense = -1f;

        /// <summary>Armor constant used when no <see cref="DamageBalanceConfig"/> is assigned.</summary>
        public const float DefaultArmorConstant = 100f;

        /// <summary>Computes the final damage a hit should deal, given the target's flat defense stat.</summary>
        public static float ComputeDamage(DamageInfo info, float defense, DamageBalanceConfig config)
        {
            return ComputeDamage(info, defense, 0f, config, resistance: null);
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
            return ComputeDamage(info, defense, 0f, config, resistance);
        }

        /// <summary>
        /// Full formula: flat armor <paramref name="defense"/> plus the summed percent Defense modifiers
        /// <paramref name="percentDefense"/> (positive = less damage, negative = more; see class docs).
        /// </summary>
        public static float ComputeDamage(DamageInfo info, float defense, float percentDefense, DamageBalanceConfig config, ElementResistance resistance)
        {
            float resistanceFraction = resistance != null ? resistance.GetResistance(info.Element) : 0f;

            if (resistanceFraction >= 1f)
            {
                return 0f;
            }

            float minimumDamage = config != null ? config.MinimumDamage : 1f;
            float criticalMultiplier = config != null ? config.CriticalMultiplier : 1.5f;

            float mitigated = info.Amount * GetDefenseMultiplier(defense, percentDefense, config);
            mitigated *= (1f - resistanceFraction);

            if (info.IsCritical)
            {
                mitigated *= criticalMultiplier;
            }

            return Mathf.Max(minimumDamage, Mathf.Round(mitigated));
        }

        /// <summary>
        /// Damage-taken multiplier from defense alone: armor curve on the flat points times the clamped
        /// percent change. 1 = no change; 0.75 = Campo Magnético's +25%; 1.2 = Drone's -20% debuff.
        /// Never negative, and at least 1 - <see cref="MaxPercentDefense"/> times the armor factor.
        /// </summary>
        public static float GetDefenseMultiplier(float flatDefense, float percentDefense, DamageBalanceConfig config)
        {
            float armorConstant = config != null ? config.ArmorConstant : DefaultArmorConstant;
            flatDefense = Mathf.Max(0f, flatDefense);
            float armorFactor = 1f - flatDefense / (flatDefense + Mathf.Max(0.0001f, armorConstant));
            float percentFactor = 1f - Mathf.Clamp(percentDefense, MinPercentDefense, MaxPercentDefense);
            return armorFactor * percentFactor;
        }

        /// <summary>
        /// Splits a target's Defense into the two inputs of the formula: <paramref name="baseDefense"/>
        /// (inspector/definition armor) + every Flat Defense modifier, and the sum of every PercentAdditive
        /// Defense modifier. A null <paramref name="buffs"/> means no modifiers.
        /// </summary>
        public static void GetDefense(BuffController buffs, float baseDefense, out float flatDefense, out float percentDefense)
        {
            flatDefense = baseDefense;
            percentDefense = 0f;

            if (buffs != null)
            {
                flatDefense += buffs.GetFlatTotal(StatType.Defense);
                percentDefense = buffs.GetPercentTotal(StatType.Defense);
            }
        }
    }
}
