using System;
using System.Collections.Generic;
using System.Text;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;

namespace Rapadura.Gameplay.Items.Procedural
{
    /// <summary>
    /// Display name of a procedurally generated item, composed at display time from the base item
    /// + the rolled affix ids saved in <see cref="InventorySlotData.generatedAffixIds"/>, in the
    /// current language:
    /// <list type="bullet">
    /// <item>pt-BR: base name, then prefix affixes (adjectives follow the noun), then suffixes —
    /// "Espada de Ferro Afiada do Vento".</item>
    /// <item>en: prefix affixes, base name, suffixes — "Sharp Iron Sword of the Wind".</item>
    /// </list>
    /// Base name via <see cref="ContentText"/> (<c>item.&lt;id&gt;.name</c>), affix text via its
    /// <see cref="ItemAffixDefinition.NameLocalizationKey"/> (<c>affix.*</c>, falling back to the
    /// asset's pt-BR <see cref="ItemAffixDefinition.FallbackDisplayText"/>). A null manager means
    /// pt-BR asset text, like the other <see cref="ContentText"/> overloads.
    ///
    /// Backward compatibility: <see cref="InventorySlotData.generatedName"/> (pt-BR, written by
    /// <see cref="ProceduralItemGenerator"/>) stays in saves. It is only shown when the name can't
    /// be composed — a slot whose affix ids no longer resolve (affix asset removed/renamed) or whose
    /// base item is unknown. UI only; sorting and logs keep using the stored name.
    /// </summary>
    public static class GeneratedItemName
    {
        /// <summary>Name for <paramref name="slot"/>, resolving its base item through <see cref="ItemDatabase"/>.</summary>
        public static string Compose(InventorySlotData slot, LocalizationManager manager)
        {
            if (slot == null || slot.IsEmpty)
            {
                return string.Empty;
            }

            return Compose(ItemDatabase.GetById(slot.itemId), slot, manager);
        }

        /// <summary>
        /// Name for <paramref name="slot"/> holding <paramref name="item"/>. Plain slots (no affix ids)
        /// get the localized base name; generated ones get the composed name, or the stored
        /// <see cref="InventorySlotData.generatedName"/> when any affix id fails to resolve.
        /// <paramref name="resolveAffix"/> defaults to <see cref="AffixDatabase.GetById"/>.
        /// </summary>
        public static string Compose(
            ItemDefinition item,
            InventorySlotData slot,
            LocalizationManager manager,
            Func<string, ItemAffixDefinition> resolveAffix = null)
        {
            // Refined pieces carry their level in the name everywhere: "Facão do Vaqueiro +7" (RefineRules).
            Rapadura.Gameplay.Refining.RefineTexts.EnsureRegistered(manager);
            string name = ComposeUnrefined(item, slot, manager, resolveAffix);
            return slot != null && slot.refineLevel > 0 && !string.IsNullOrEmpty(name)
                ? name + Rapadura.Gameplay.Refining.RefineRules.NameSuffix(slot.refineLevel)
                : name;
        }

        /// <summary><see cref="Compose(ItemDefinition, InventorySlotData, LocalizationManager, Func{string, ItemAffixDefinition})"/> without the " +N" refine suffix.</summary>
        public static string ComposeUnrefined(
            ItemDefinition item,
            InventorySlotData slot,
            LocalizationManager manager,
            Func<string, ItemAffixDefinition> resolveAffix = null)
        {
            if (item == null)
            {
                return slot?.generatedName ?? string.Empty;
            }

            string baseName = item.LocalizedName(manager);
            if (slot == null || !slot.HasGeneratedAffixes)
            {
                return baseName;
            }

            resolveAffix = resolveAffix ?? AffixDatabase.GetById;
            var prefixes = new List<string>();
            var suffixes = new List<string>();

            foreach (string affixId in slot.generatedAffixIds)
            {
                ItemAffixDefinition affix = string.IsNullOrEmpty(affixId) ? null : resolveAffix(affixId);
                if (affix == null)
                {
                    // Can't rebuild faithfully: keep showing what the save recorded.
                    return string.IsNullOrEmpty(slot.generatedName) ? baseName : slot.generatedName;
                }

                string text = AffixText(affix, manager);
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                (affix.SlotKind == AffixSlotKind.Prefix ? prefixes : suffixes).Add(text);
            }

            return ComposeParts(baseName, prefixes, suffixes, LanguageOf(manager));
        }

        /// <summary>Affix text in the manager's language, or the asset's pt-BR fallback.</summary>
        public static string AffixText(ItemAffixDefinition affix, LocalizationManager manager)
        {
            if (affix == null)
            {
                return string.Empty;
            }

            return ContentText.Resolve(manager, affix.NameLocalizationKey, affix.FallbackDisplayText);
        }

        /// <summary>The ordering language: the manager's current one, pt-BR without a manager.</summary>
        public static LanguageCode LanguageOf(LocalizationManager manager) =>
            manager != null ? manager.CurrentLanguage : LanguageCode.ptBR;

        /// <summary>
        /// Joins already-localized parts in <paramref name="language"/>'s order (pt-BR: base +
        /// prefixes + suffixes; en: prefixes + base + suffixes).
        /// </summary>
        public static string ComposeParts(string baseName, IEnumerable<string> prefixes, IEnumerable<string> suffixes, LanguageCode language)
        {
            var parts = new List<string>();

            if (language == LanguageCode.en)
            {
                AddAll(parts, prefixes);
                Add(parts, baseName);
            }
            else
            {
                Add(parts, baseName);
                AddAll(parts, prefixes);
            }

            AddAll(parts, suffixes);

            var builder = new StringBuilder();
            foreach (string part in parts)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(part);
            }

            return builder.ToString();
        }

        private static void AddAll(List<string> parts, IEnumerable<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (string value in values)
            {
                Add(parts, value);
            }
        }

        private static void Add(List<string> parts, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add(value);
            }
        }
    }
}
