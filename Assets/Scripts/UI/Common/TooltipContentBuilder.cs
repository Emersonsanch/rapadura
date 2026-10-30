using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Refining;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Skills;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Structured tooltip text: a title (item/skill name), a subtitle (rarity/type or skill
    /// level/element), stat/affix lines and a free-text description. <see cref="TooltipController"/>
    /// renders each part into its own label; <see cref="ToPlainText"/> flattens it for the single-label
    /// fallback and for tests.
    /// </summary>
    public sealed class TooltipContent
    {
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public readonly List<string> StatLines = new List<string>();
        public string Description = string.Empty;

        /// <summary>Set for items (drives the title color); null for skills.</summary>
        public ItemRarity? Rarity;

        /// <summary>Refine glow tier 0..5 of the item (<see cref="Rapadura.Gameplay.Refining.RefineRules.GlowTier"/>): the title glows from +7.</summary>
        public int RefineGlowTier;

        /// <summary>The MU-style option lines (also in <see cref="StatLines"/>): the tooltip paints them green ("Excelente").</summary>
        public readonly List<string> OptionLines = new List<string>();

        public string ToPlainText()
        {
            var builder = new StringBuilder();
            AppendLine(builder, Title);
            AppendLine(builder, Subtitle);
            foreach (string line in StatLines)
            {
                AppendLine(builder, line);
            }

            AppendLine(builder, Description);
            return builder.ToString();
        }

        private static void AppendLine(StringBuilder builder, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder.Append(text);
        }
    }

    /// <summary>
    /// Pure, UIDocument-free formatting of tooltip content for items (inventory, equipment, shop)
    /// and skills (skill tree). Same rationale as <see cref="TooltipPositioning"/>: all the text
    /// rules live here so EditMode tests can cover them without a panel.
    ///
    /// Every string goes through the optional <c>localize</c> resolver (normally
    /// <c>LocalizationManager.Get</c>); when it is null, or returns the "[key]" missing-key marker,
    /// the pt-BR text from <see cref="TooltipLocalization.Entries"/> is used instead. Numbers are
    /// formatted with the invariant culture so output is deterministic across locales.
    /// </summary>
    public static class TooltipContentBuilder
    {
        /// <summary>
        /// Builds the tooltip for an item. <paramref name="slot"/> is optional: when given, its
        /// generated name (composed per language by <see cref="GeneratedItemName"/>)/affixes/rarity
        /// (ProceduralItemGenerator rolls), durability and quantity
        /// are included. <paramref name="resolveAffix"/> defaults to <see cref="AffixDatabase.GetById"/>.
        /// <paramref name="buyPrice"/>/<paramref name="sellPrice"/> add a price line (shop screen).
        /// </summary>
        public static TooltipContent ForItem(
            ItemDefinition item,
            InventorySlotData slot = null,
            Func<string, string> localize = null,
            Func<string, ItemAffixDefinition> resolveAffix = null,
            int? buyPrice = null,
            int? sellPrice = null)
        {
            var content = new TooltipContent();
            if (item == null)
            {
                return content;
            }

            ItemRarity rarity = ResolveRarity(item, slot);
            content.Rarity = rarity;

            // Generated items compose "base + affixes" in the current language from the saved affix
            // ids; the stored pt-BR generatedName is only the fallback (unresolvable ids, old saves).
            content.Title = GeneratedItemName.Compose(item, slot, ContentText.CurrentManager, resolveAffix);
            content.Subtitle = BuildItemSubtitle(item, rarity, localize);
            content.Description = item.LocalizedDescription();

            List<string> lines = content.StatLines;

            // A refined piece shows its refined base stat (RefineRules.RefinedStat), then what the refine adds.
            int refineLevel = slot != null ? RefineRules.ClampLevel(slot.refineLevel) : 0;
            int refineOption = slot != null ? RefineRules.ClampOption(slot.refineOptionLevel) : 0;
            content.RefineGlowTier = RefineRules.GlowTier(refineLevel);

            if (item.WeaponDamage > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.weapon_damage", Number(RefineRules.RefinedStat(item.WeaponDamage, refineLevel))));
                lines.Add(Format(localize, "ui.tooltip.attack_speed", Number(item.WeaponAttackSpeed)));
            }

            if (item.ArmorDefense > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.armor_defense", Number(RefineRules.RefinedStat(item.ArmorDefense, refineLevel))));
            }

            AppendRefineLines(item, refineLevel, refineOption, lines);
            AppendWingLines(item, lines, localize);

            if (item.EquipStatModifiers != null)
            {
                foreach (StatModifierDefinition modifier in item.EquipStatModifiers)
                {
                    if (modifier != null)
                    {
                        lines.Add(FormatModifier(modifier.affectedStat, modifier.application, modifier.value, localize));
                    }
                }
            }

            AppendAffixLines(slot, lines, localize, resolveAffix ?? AffixDatabase.GetById);
            // MU-style options (FASE 16): green "Excelente" lines, luck, ancient set, sockets.
            content.OptionLines.AddRange(Rapadura.Gameplay.Items.Options.ItemOptionTexts.TooltipLines(ContentText.CurrentManager, item, slot));
            lines.AddRange(content.OptionLines);

            if (item.HealthRestore > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.restores_health", Number(item.HealthRestore)));
            }

            if (item.StaminaRestore > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.restores_stamina", Number(item.StaminaRestore)));
            }

            if (item.ManaRestore > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.restores_mana", Number(item.ManaRestore)));
            }

            if (item.ConsumeBuffs != null)
            {
                foreach (StatModifierDefinition buff in item.ConsumeBuffs)
                {
                    if (buff != null)
                    {
                        lines.Add(FormatTimedModifier(buff, localize));
                    }
                }
            }

            if (slot != null && item.HasDurability)
            {
                lines.Add(Format(localize, "ui.tooltip.durability", Number(slot.currentDurability), Number(item.MaxDurability)));
                if (Rapadura.Gameplay.Items.Durability.DurabilityRules.IsBroken(item, slot))
                {
                    lines.Add(Rapadura.Gameplay.Items.Durability.DurabilityTexts.Get(ContentText.CurrentManager, Rapadura.Gameplay.Items.Durability.DurabilityTexts.TooltipBroken));
                }
                else if (Rapadura.Gameplay.Items.Durability.DurabilityRules.IsWorn(item.MaxDurability, slot.currentDurability))
                {
                    lines.Add(Rapadura.Gameplay.Items.Durability.DurabilityTexts.Get(ContentText.CurrentManager, Rapadura.Gameplay.Items.Durability.DurabilityTexts.TooltipWorn));
                }
            }

            if (slot != null && item.IsStackable && slot.quantity > 1)
            {
                lines.Add(Format(localize, "ui.tooltip.quantity", slot.quantity.ToString(CultureInfo.InvariantCulture)));
            }

            lines.Add(WeightLine(item, slot != null ? slot.quantity : 1));

            if (buyPrice.HasValue)
            {
                lines.Add(Format(localize, "ui.tooltip.price", buyPrice.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (sellPrice.HasValue)
            {
                lines.Add(Format(localize, "ui.tooltip.sell_price", sellPrice.Value.ToString(CultureInfo.InvariantCulture)));
            }

            return content;
        }

        /// <summary>
        /// Builds the tooltip for a skill-tree node. <paramref name="currentLevel"/> is 0 when the
        /// skill isn't learned yet — damage/healing are then shown for level 1 (what learning it gives).
        /// </summary>
        public static TooltipContent ForSkill(SkillDefinition skill, int currentLevel, Func<string, string> localize = null)
        {
            var content = new TooltipContent();
            if (skill == null)
            {
                return content;
            }

            content.Title = skill.LocalizedName();
            content.Description = skill.LocalizedDescription();

            string levelText = currentLevel > 0
                ? Format(localize, "ui.tooltip.skill_level", currentLevel.ToString(CultureInfo.InvariantCulture), skill.MaxLevel.ToString(CultureInfo.InvariantCulture))
                : Localize(localize, "ui.tooltip.skill_not_learned");

            content.Subtitle = skill.Element != ElementType.None
                ? $"{levelText} · {Localize(localize, "ui.tooltip.element." + TooltipLocalization.ToSnakeCase(skill.Element.ToString()))}"
                : levelText;

            int effectiveLevel = Math.Max(1, currentLevel);
            List<string> lines = content.StatLines;

            float damage = skill.GetDamageForLevel(effectiveLevel);
            if (damage > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_damage", Number(damage)));
            }

            float healing = skill.GetHealingForLevel(effectiveLevel);
            if (healing > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_healing", Number(healing)));
            }

            if (skill.ManaCost > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_mana_cost", Number(skill.ManaCost)));
            }

            if (skill.EnergyCost > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_energy_cost", Number(skill.EnergyCost)));
            }

            if (skill.CooldownSeconds > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_cooldown", Number(skill.CooldownSeconds)));
            }

            AppendTimedModifiers(skill.Buffs, lines, localize);
            AppendTimedModifiers(skill.Debuffs, lines, localize);

            if (skill.StunDurationSeconds > 0f)
            {
                lines.Add(Format(localize, "ui.tooltip.skill_stun", Number(skill.StunDurationSeconds)));
            }

            return content;
        }

        /// <summary>
        /// A procedurally generated item shows the rarity it was rolled at; anything else (or a roll
        /// lower than the base item's own tier) shows <see cref="ItemDefinition.Rarity"/>.
        /// </summary>
        public static ItemRarity ResolveRarity(ItemDefinition item, InventorySlotData slot)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            if (slot == null || !slot.HasGeneratedAffixes)
            {
                return baseRarity;
            }

            return slot.generatedRarity > baseRarity ? slot.generatedRarity : baseRarity;
        }

        /// <summary>
        /// "+5 Attack damage", "+10% Move speed", "-2 Defense". Stats that are fractions by nature
        /// (crit chance/damage, cooldown/cost reduction — see AttributeSet) render as percentages
        /// even when applied Flat, matching how the affix seeder authors them (0.03 = 3%).
        /// </summary>
        public static string FormatModifier(StatType stat, ModifierApplication application, float value, Func<string, string> localize = null)
        {
            bool asPercent = application == ModifierApplication.PercentAdditive || IsFractionStat(stat);
            float shown = asPercent ? value * 100f : value;
            string sign = shown < 0f ? "-" : "+";
            string number = Number(Math.Abs(shown)) + (asPercent ? "%" : string.Empty);
            string statName = Localize(localize, "ui.tooltip.stat." + TooltipLocalization.ToSnakeCase(stat.ToString()));
            return $"{sign}{number} {statName}";
        }

        public static bool IsFractionStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.CriticalChance:
                case StatType.CriticalDamage:
                case StatType.CooldownReduction:
                case StatType.ResourceCostReduction:
                case StatType.AttackSpeed:
                case StatType.Thorns:
                    return true;
                default:
                    return false;
            }
        }

        private static string BuildItemSubtitle(ItemDefinition item, ItemRarity rarity, Func<string, string> localize)
        {
            string rarityText = Localize(localize, "ui.tooltip.rarity." + TooltipLocalization.ToSnakeCase(rarity.ToString()));
            string kindText = item.IsEquippable
                ? Localize(localize, "ui.tooltip.slot." + TooltipLocalization.ToSnakeCase(item.EquipSlot.ToString()))
                : Localize(localize, "ui.tooltip.type." + TooltipLocalization.ToSnakeCase(item.Type.ToString()));
            return $"{rarityText} · {kindText}";
        }

        /// <summary>"Refino +7: +30% no atributo base" and the Pedra da Vida's option line (RefineTexts).</summary>
        private static void AppendRefineLines(ItemDefinition item, int level, int option, List<string> lines)
        {
            if (!RefineRules.IsRefinable(item))
            {
                return;
            }

            LocalizationManager manager = ContentText.CurrentManager;
            if (level > 0)
            {
                lines.Add(RefineTexts.Format(manager, RefineTexts.TooltipLevel, level, Number(RefineRules.StatBonusFraction(level) * 100f)));
            }

            if (option > 0)
            {
                bool weapon = item.WeaponDamage > 0f;
                float percent = weapon ? option * RefineRules.WeaponOptionAttackPercent : RefineRules.ArmourHealthPercent(option);
                lines.Add(RefineTexts.Format(manager, weapon ? RefineTexts.TooltipOptionWeapon : RefineTexts.TooltipOptionArmour,
                    option, RefineRules.MaxOptionLevel, Number(percent * 100f)));
            }
        }

        /// <summary>A wing's bonuses (Tacho Encantado, <see cref="Rapadura.Gameplay.Tacho.WingCatalog"/>): damage, defense, speed, absorb.</summary>
        private static void AppendWingLines(ItemDefinition item, List<string> lines, Func<string, string> localize)
        {
            foreach (Rapadura.Gameplay.Tacho.WingModifier modifier in Rapadura.Gameplay.Tacho.WingCatalog.ModifiersFor(item.ItemId))
            {
                lines.Add(modifier.IsAbsorb
                    ? Rapadura.Gameplay.Tacho.TachoTexts.Format(ContentText.CurrentManager, Rapadura.Gameplay.Tacho.TachoTexts.TooltipAbsorb, Number(modifier.Value * 100f))
                    : FormatModifier(modifier.Stat, modifier.Application, modifier.Value, localize));
            }
        }

        private static void AppendAffixLines(InventorySlotData slot, List<string> lines, Func<string, string> localize, Func<string, ItemAffixDefinition> resolveAffix)
        {
            if (slot == null || !slot.HasGeneratedAffixes)
            {
                return;
            }

            for (int i = 0; i < slot.generatedAffixIds.Length; i++)
            {
                ItemAffixDefinition affix = resolveAffix(slot.generatedAffixIds[i]);
                if (affix == null)
                {
                    continue;
                }

                float value = slot.generatedAffixValues != null && i < slot.generatedAffixValues.Length ? slot.generatedAffixValues[i] : 0f;
                lines.Add(FormatModifier(affix.AffectedStat, affix.Application, value, localize));
            }
        }

        private static void AppendTimedModifiers(StatModifierDefinition[] modifiers, List<string> lines, Func<string, string> localize)
        {
            if (modifiers == null)
            {
                return;
            }

            foreach (StatModifierDefinition modifier in modifiers)
            {
                if (modifier != null)
                {
                    lines.Add(FormatTimedModifier(modifier, localize));
                }
            }
        }

        private static string FormatTimedModifier(StatModifierDefinition modifier, Func<string, string> localize)
        {
            string effect = FormatModifier(modifier.affectedStat, modifier.application, modifier.value, localize);
            return modifier.durationSeconds > 0f
                ? Format(localize, "ui.tooltip.duration", effect, Number(modifier.durationSeconds))
                : effect;
        }

        private static string Format(Func<string, string> localize, string key, params object[] args)
        {
            string pattern = Localize(localize, key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, pattern, args);
            }
            catch (FormatException)
            {
                // A translator broke the placeholders — fall back to the pt-BR pattern rather than crash the UI.
                string fallback = TooltipLocalization.GetFallback(key) ?? pattern;
                return string.Format(CultureInfo.InvariantCulture, fallback, args);
            }
        }

        private static string Localize(Func<string, string> localize, string key)
        {
            if (localize != null)
            {
                string resolved = localize(key);
                bool isMissingMarker = !string.IsNullOrEmpty(resolved) && resolved.StartsWith("[", StringComparison.Ordinal) && resolved.EndsWith("]", StringComparison.Ordinal);
                if (!string.IsNullOrEmpty(resolved) && !isMissingMarker)
                {
                    return resolved;
                }
            }

            return TooltipLocalization.GetFallback(key) ?? key;
        }

        /// <summary>Up to one decimal, invariant culture: 12 -> "12", 1.5 -> "1.5", 0.05 -> "0.1".</summary>
        private static string Number(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        /// <summary>"Peso: 3", "Peso: 0.5 cada (20 = 10)" for a stack, "Não pesa nada" for coins (<see cref="ItemWeightRules"/>).</summary>
        public static string WeightLine(ItemDefinition item, int quantity)
        {
            LocalizationManager manager = ContentText.CurrentManager;
            float unit = ItemWeightRules.UnitWeight(item);
            if (unit <= 0f)
            {
                return EncumbranceTexts.Get(manager, EncumbranceTexts.TooltipWeightless);
            }

            if (quantity > 1)
            {
                return EncumbranceTexts.Format(manager, EncumbranceTexts.TooltipStackWeight,
                    EncumbranceTexts.Number(unit), quantity.ToString(CultureInfo.InvariantCulture), EncumbranceTexts.Number(unit * quantity));
            }

            return EncumbranceTexts.Format(manager, EncumbranceTexts.TooltipWeight, EncumbranceTexts.Number(unit));
        }
    }
}
