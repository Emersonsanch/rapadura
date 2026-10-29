using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.UI.CharacterSheet
{
    /// <summary>Derived numbers shown in the sheet's "Status" panel (all fractions are 0..1, not percents).</summary>
    public readonly struct CharacterDerivedStats
    {
        public readonly float MaxHealth;
        public readonly float MaxMana;
        public readonly float MaxStamina;
        public readonly float CriticalChance;
        public readonly float CriticalMultiplier;
        public readonly float CooldownReduction;
        /// <summary>Percent bonus on outgoing skill/attack damage from AttackDamage modifiers (0.05 = +5%).</summary>
        public readonly float SkillDamageBonus;
        /// <summary>Basic-attack speed bonus (Σ AttackSpeed, clamped like PlayerBasicAttack.ResolveCooldown; 0.1 = +10%).</summary>
        public readonly float AttackSpeedBonus;
        /// <summary>Share of melee damage taken reflected to the attacker (ThornsRules.GetFraction; 0.1 = 10%).</summary>
        public readonly float Thorns;

        public CharacterDerivedStats(float maxHealth, float maxMana, float maxStamina, float criticalChance,
            float criticalMultiplier, float cooldownReduction, float skillDamageBonus)
            : this(maxHealth, maxMana, maxStamina, criticalChance, criticalMultiplier, cooldownReduction, skillDamageBonus, 0f, 0f)
        {
        }

        public CharacterDerivedStats(float maxHealth, float maxMana, float maxStamina, float criticalChance,
            float criticalMultiplier, float cooldownReduction, float skillDamageBonus, float attackSpeedBonus, float thorns)
        {
            AttackSpeedBonus = attackSpeedBonus;
            Thorns = thorns;
            MaxHealth = maxHealth;
            MaxMana = maxMana;
            MaxStamina = maxStamina;
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
            CooldownReduction = cooldownReduction;
            SkillDamageBonus = skillDamageBonus;
        }
    }

    /// <summary>
    /// Pure (MonoBehaviour-free) math/formatting for the Ficha do personagem, so EditMode tests can
    /// check it without a UIDocument. Every number is read through the SAME code the game uses for
    /// the real effect, never re-derived: per-point values are <see cref="AttributeSet"/>'s constants,
    /// max vitals come from <see cref="PlayerStats"/>, crit chance/multiplier from
    /// <see cref="OutgoingDamage"/>, cooldown reduction exactly like <c>SkillManager.GetCooldownReductionPercent</c>
    /// and the skill damage bonus is what <see cref="OutgoingDamage.Resolve"/> multiplies a hit by
    /// (AttackDamage modifiers: Poder Arcano, equipment affixes, buffs).
    /// </summary>
    public static class CharacterSheetPresenter
    {
        /// <summary>Display order of the attribute rows.</summary>
        public static readonly IReadOnlyList<AttributeType> AttributeOrder = new[]
        {
            AttributeType.Vitality,
            AttributeType.Spirit,
            AttributeType.Dexterity,
            AttributeType.Intelligence,
            AttributeType.ArcanePower,
        };

        /// <summary>Most the HUD badge spells out; more shows as "9+" so the 24px bubble never overflows.</summary>
        public const int BadgeMaxShown = 9;

        /// <summary>
        /// What one more point in <paramref name="type"/> adds, in display units: flat HP/mana for
        /// Vitalidade/Espírito, percentage points for the fraction stats (0.005 → 0.5).
        /// </summary>
        public static float GetPreviewDelta(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Vitality: return AttributeSet.VitalityHealthPerPoint;
                case AttributeType.Spirit: return AttributeSet.SpiritManaPerPoint;
                case AttributeType.Dexterity: return AttributeSet.DexterityCriticalChancePerPoint * 100f;
                case AttributeType.Intelligence: return AttributeSet.IntelligenceCooldownReductionPerPoint * 100f;
                case AttributeType.ArcanePower: return AttributeSet.ArcanePowerAttackDamagePerPoint * 100f;
                default: return 0f;
            }
        }

        /// <summary>"+10 Vida máx." / "+0,5% Chance de crítico" (en: "+10 Max HP" / "+0.5% Crit chance").</summary>
        public static string FormatPreview(AttributeType type, LocalizationManager manager)
        {
            return CharacterSheetTexts.Format(manager, CharacterSheetTexts.PreviewKey(type), FormatNumber(GetPreviewDelta(type), manager));
        }

        public static string GetAttributeName(AttributeType type, LocalizationManager manager)
        {
            return CharacterSheetTexts.Get(manager, CharacterSheetTexts.AttributeNameKey(type));
        }

        /// <summary>Reads every derived stat from live components. Any of them may be null (missing component → base values).</summary>
        public static CharacterDerivedStats BuildDerivedStats(PlayerStats stats, BuffController buffs, PlayerCombatBalance balance)
        {
            float cooldownReduction = buffs != null ? Mathf.Clamp01(buffs.GetModifiedValue(StatType.CooldownReduction, 0f)) : 0f;
            // GetModifiedValue = (base + flat) × (1 + percent), so f(1) − f(0) = 1 + percent: the percent
            // part alone, unaffected by any flat AttackDamage from gear (which isn't a "%").
            float skillDamageBonus = buffs != null
                ? buffs.GetModifiedValue(StatType.AttackDamage, 1f) - buffs.GetModifiedValue(StatType.AttackDamage, 0f) - 1f
                : 0f;

            return new CharacterDerivedStats(
                stats != null ? stats.MaxHealth : 0f,
                stats != null ? stats.MaxMana : 0f,
                stats != null ? stats.MaxStamina : 0f,
                OutgoingDamage.GetCriticalChance(buffs, balance),
                OutgoingDamage.GetCriticalMultiplier(buffs, balance),
                cooldownReduction,
                skillDamageBonus,
                buffs != null
                    ? Mathf.Clamp(buffs.GetAdditiveFractionTotal(StatType.AttackSpeed), PlayerBasicAttack.MinAttackSpeedBonus, PlayerBasicAttack.MaxAttackSpeedBonus)
                    : 0f,
                ThornsRules.GetFraction(buffs));
        }

        public static bool CanSpend(int availablePoints) => availablePoints > 0;

        /// <summary>
        /// Spends one point via <see cref="AttributeSet.TrySpendPoint"/> (the only spending path — the
        /// sheet never edits attributes itself). False with no set or no points left.
        /// </summary>
        public static bool TrySpend(AttributeSet attributes, AttributeType type)
        {
            return attributes != null && attributes.TrySpendPoint(type, 1);
        }

        /// <summary>The HUD "Ficha" badge is shown only while there are unspent attribute points.</summary>
        public static bool ShouldShowBadge(int availablePoints) => availablePoints > 0;

        public static string FormatBadge(int availablePoints)
        {
            if (availablePoints <= 0)
            {
                return string.Empty;
            }

            return availablePoints > BadgeMaxShown
                ? BadgeMaxShown.ToString(CultureInfo.InvariantCulture) + "+"
                : availablePoints.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Attribute total shown on a row: allocated + talent, e.g. "12".</summary>
        public static string FormatAttributeValue(int total) => total.ToString(CultureInfo.InvariantCulture);

        public static string FormatFlat(float value) => Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);

        /// <summary>0.125 → "12,5%" (pt-BR) / "12.5%" (en).</summary>
        public static string FormatPercent(float fraction, LocalizationManager manager)
        {
            return FormatNumber(fraction * 100f, manager) + "%";
        }

        /// <summary>1.5 → "x1,5" (pt-BR) / "x1.5" (en).</summary>
        public static string FormatMultiplier(float multiplier, LocalizationManager manager)
        {
            return "x" + FormatNumber(multiplier, manager);
        }

        /// <summary>
        /// Up to one decimal, trailing ",0" dropped. Decimal comma unless the language is English (no
        /// manager = pt-BR, the game's default). Built by hand instead of CultureInfo("pt-BR") so it
        /// doesn't depend on the culture data a stripped/invariant mobile build may not ship.
        /// </summary>
        public static string FormatNumber(float value, LocalizationManager manager)
        {
            float rounded = Mathf.Round(value * 10f) / 10f;
            string text = rounded.ToString("0.#", CultureInfo.InvariantCulture);
            bool english = manager != null && manager.CurrentLanguage == LanguageCode.en;
            return english ? text : text.Replace('.', ',');
        }

        /// <summary>XP bar fill, 0..1 (0 when the next level needs no XP / data is missing).</summary>
        public static float ExperienceFraction(int current, int toNextLevel)
        {
            return toNextLevel > 0 ? Mathf.Clamp01((float)current / toNextLevel) : 0f;
        }
    }
}
