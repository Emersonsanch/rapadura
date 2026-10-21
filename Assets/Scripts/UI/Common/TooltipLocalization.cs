using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Localization keys (en + pt-BR) used by <see cref="TooltipContentBuilder"/>. Kept next to the
    /// tooltip code instead of inside <c>LocalizationManager.DefaultEntries</c> so the tooltip
    /// feature stays self-contained: <see cref="TooltipController"/> merges these into the live
    /// <see cref="LocalizationManager"/> on first use (<see cref="LocalizationManager.LoadEntries"/>
    /// only adds/overwrites matching keys, it never clears others), and
    /// <see cref="TooltipContentBuilder"/> falls back to the pt-BR text below when no localizer is
    /// available (EditMode tests, editor tools), same as every other *Texts helper — a single source
    /// of truth for every tooltip string.
    ///
    /// Format strings use <see cref="string.Format(string, object)"/> placeholders ({0}, {1}).
    /// </summary>
    public static class TooltipLocalization
    {
        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Rarity --
            new LocalizationEntry("ui.tooltip.rarity.common", "Common", "Comum"),
            new LocalizationEntry("ui.tooltip.rarity.uncommon", "Uncommon", "Incomum"),
            new LocalizationEntry("ui.tooltip.rarity.rare", "Rare", "Raro"),
            new LocalizationEntry("ui.tooltip.rarity.epic", "Epic", "Épico"),
            new LocalizationEntry("ui.tooltip.rarity.legendary", "Legendary", "Lendário"),

            // -- Item type --
            new LocalizationEntry("ui.tooltip.type.material", "Material", "Material"),
            new LocalizationEntry("ui.tooltip.type.consumable", "Consumable", "Consumível"),
            new LocalizationEntry("ui.tooltip.type.weapon", "Weapon", "Arma"),
            new LocalizationEntry("ui.tooltip.type.armor", "Armor", "Armadura"),
            new LocalizationEntry("ui.tooltip.type.tool", "Tool", "Ferramenta"),
            new LocalizationEntry("ui.tooltip.type.quest_item", "Quest item", "Item de missão"),
            new LocalizationEntry("ui.tooltip.type.currency", "Currency", "Moeda"),
            new LocalizationEntry("ui.tooltip.type.blueprint", "Blueprint", "Projeto"),

            // -- Equipment slot --
            new LocalizationEntry("ui.tooltip.slot.head", "Head", "Cabeça"),
            new LocalizationEntry("ui.tooltip.slot.chest", "Chest", "Peito"),
            new LocalizationEntry("ui.tooltip.slot.legs", "Legs", "Pernas"),
            new LocalizationEntry("ui.tooltip.slot.boots", "Boots", "Botas"),
            new LocalizationEntry("ui.tooltip.slot.main_hand", "Main hand", "Mão principal"),
            new LocalizationEntry("ui.tooltip.slot.off_hand", "Off hand", "Mão secundária"),
            new LocalizationEntry("ui.tooltip.slot.tool", "Tool", "Ferramenta"),
            new LocalizationEntry("ui.tooltip.slot.accessory", "Accessory", "Acessório"),

            // -- Stats (StatType) --
            new LocalizationEntry("ui.tooltip.stat.max_health", "Max health", "Vida máxima"),
            new LocalizationEntry("ui.tooltip.stat.health_regen", "Health regen", "Regeneração de vida"),
            new LocalizationEntry("ui.tooltip.stat.max_stamina", "Max stamina", "Stamina máxima"),
            new LocalizationEntry("ui.tooltip.stat.stamina_regen", "Stamina regen", "Regeneração de stamina"),
            new LocalizationEntry("ui.tooltip.stat.max_mana", "Max mana", "Mana máxima"),
            new LocalizationEntry("ui.tooltip.stat.mana_regen", "Mana regen", "Regeneração de mana"),
            new LocalizationEntry("ui.tooltip.stat.move_speed", "Move speed", "Velocidade de movimento"),
            new LocalizationEntry("ui.tooltip.stat.attack_damage", "Attack damage", "Dano de ataque"),
            new LocalizationEntry("ui.tooltip.stat.defense", "Defense", "Defesa"),
            new LocalizationEntry("ui.tooltip.stat.critical_chance", "Critical chance", "Chance de crítico"),
            new LocalizationEntry("ui.tooltip.stat.critical_damage", "Critical damage", "Dano crítico"),
            new LocalizationEntry("ui.tooltip.stat.cooldown_reduction", "Cooldown reduction", "Redução de recarga"),
            new LocalizationEntry("ui.tooltip.stat.resource_cost_reduction", "Resource cost reduction", "Redução de custo"),

            // -- Element (skills) --
            new LocalizationEntry("ui.tooltip.element.physical", "Physical", "Físico"),
            new LocalizationEntry("ui.tooltip.element.fire", "Fire", "Fogo"),
            new LocalizationEntry("ui.tooltip.element.ice", "Ice", "Gelo"),
            new LocalizationEntry("ui.tooltip.element.lightning", "Lightning", "Raio"),
            new LocalizationEntry("ui.tooltip.element.poison", "Poison", "Veneno"),
            new LocalizationEntry("ui.tooltip.element.earth", "Earth", "Terra"),
            new LocalizationEntry("ui.tooltip.element.wind", "Wind", "Vento"),
            new LocalizationEntry("ui.tooltip.element.holy", "Holy", "Sagrado"),
            new LocalizationEntry("ui.tooltip.element.dark", "Dark", "Sombrio"),
            new LocalizationEntry("ui.tooltip.element.arcane", "Arcane", "Arcano"),

            // -- Lines --
            new LocalizationEntry("ui.tooltip.weapon_damage", "Damage: {0}", "Dano: {0}"),
            new LocalizationEntry("ui.tooltip.attack_speed", "Attack speed: {0}", "Velocidade de ataque: {0}"),
            new LocalizationEntry("ui.tooltip.armor_defense", "Defense: {0}", "Defesa: {0}"),
            new LocalizationEntry("ui.tooltip.durability", "Durability: {0}/{1}", "Durabilidade: {0}/{1}"),
            new LocalizationEntry("ui.tooltip.restores_health", "Restores {0} health", "Restaura {0} de vida"),
            new LocalizationEntry("ui.tooltip.restores_stamina", "Restores {0} stamina", "Restaura {0} de stamina"),
            new LocalizationEntry("ui.tooltip.restores_mana", "Restores {0} mana", "Restaura {0} de mana"),
            new LocalizationEntry("ui.tooltip.price", "Price: {0}", "Preço: {0}"),
            new LocalizationEntry("ui.tooltip.sell_price", "Sells for: {0}", "Vende por: {0}"),
            new LocalizationEntry("ui.tooltip.quantity", "Quantity: {0}", "Quantidade: {0}"),
            new LocalizationEntry("ui.tooltip.duration", "{0} for {1}s", "{0} por {1}s"),
            new LocalizationEntry("ui.tooltip.skill_level", "Level {0}/{1}", "Nível {0}/{1}"),
            new LocalizationEntry("ui.tooltip.skill_not_learned", "Not learned", "Não aprendida"),
            new LocalizationEntry("ui.tooltip.skill_damage", "Damage: {0}", "Dano: {0}"),
            new LocalizationEntry("ui.tooltip.skill_healing", "Healing: {0}", "Cura: {0}"),
            new LocalizationEntry("ui.tooltip.skill_mana_cost", "Mana cost: {0}", "Custo de mana: {0}"),
            new LocalizationEntry("ui.tooltip.skill_energy_cost", "Energy cost: {0}", "Custo de energia: {0}"),
            new LocalizationEntry("ui.tooltip.skill_cooldown", "Cooldown: {0}s", "Recarga: {0}s"),
            new LocalizationEntry("ui.tooltip.equipped", "Equipped", "Equipado"),
            new LocalizationEntry("ui.tooltip.empty_slot", "Empty", "Vazio"),

            // -- Inventory/equipment screen (InventoryUIController) --
            new LocalizationEntry("ui.inventory.title", "Backpack", "Mochila"),
            new LocalizationEntry("ui.inventory.equipment", "Equipment", "Equipamento"),
            new LocalizationEntry("ui.inventory.items", "Items", "Itens"),
            new LocalizationEntry("ui.inventory.weight", "Weight: {0}/{1}", "Peso: {0}/{1}"),
            new LocalizationEntry("ui.inventory.hint", "Tap to use/equip. Hold for details.", "Toque para usar/equipar. Segure para ver detalhes."),
        };

        private static Dictionary<string, string> _englishByKey;
        private static Dictionary<string, string> _fallbackByKey;

        /// <summary>
        /// No-manager fallback: pt-BR text for <paramref name="key"/> from <see cref="Entries"/>, or null
        /// if the key isn't one of ours. pt-BR like the other helpers (QuestLogTexts, SettingsMenuTexts,
        /// UiTexts...), so a screen never mixes languages when no LocalizationManager is around.
        /// </summary>
        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>English text for <paramref name="key"/> from <see cref="Entries"/>, or null if the key isn't one of ours.</summary>
        public static string GetEnglishFallback(string key)
        {
            if (_englishByKey == null)
            {
                _englishByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _englishByKey[entry.Key] = entry.En;
                }
            }

            return key != null && _englishByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        private static LocalizationManager _mergedInto;

        /// <summary>"AttackDamage" -> "attack_damage", "MainHand" -> "main_hand". Used to build enum-based keys.</summary>
        public static string ToSnakeCase(string pascal)
        {
            if (string.IsNullOrEmpty(pascal))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(pascal.Length + 4);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
