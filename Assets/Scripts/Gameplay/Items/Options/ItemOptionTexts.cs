using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Items.Options
{
    /// <summary>
    /// en + pt-BR text of the MU-style item options (FASE 16): the green "Excelente" lines, "com Sorte", the five ancient
    /// sets (one per cousin), socket engravings and the ferreiro's engraving buttons. Same self-contained pattern as the
    /// other *Texts helpers (merged into the live <see cref="LocalizationManager"/>, pt-BR fallback without one).
    /// </summary>
    public static class ItemOptionTexts
    {
        public const string ExcellentTag = "ui.item_option.excellent_tag";
        public const string LuckLine = "ui.item_option.luck";
        /// <summary>{0} set name.</summary>
        public const string SetLine = "ui.item_option.set_line";
        /// <summary>{0} socket number.</summary>
        public const string SocketEmpty = "ui.item_option.socket_empty";
        /// <summary>{0} socket number, {1} engraving name.</summary>
        public const string SocketFilled = "ui.item_option.socket_filled";
        /// <summary>{0} engraving name, {1} material quantity, {2} material name, {3} gold.</summary>
        public const string EngraveButton = "ui.item_option.engrave_button";
        /// <summary>{0} item, {1} engraving name.</summary>
        public const string Engraved = "ui.item_option.engraved";
        public const string EngraveNoSocket = "ui.item_option.engrave_no_socket";
        public const string EngraveMissing = "ui.item_option.engrave_missing";
        public const string EngraveTitle = "ui.item_option.engrave_title";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(ExcellentTag, "Excellent", "Excelente"),
            new LocalizationEntry("ui.item_option.excellent.weapondamage", "Excellent: +1.5% damage", "Excelente: +1,5% de dano"),
            new LocalizationEntry("ui.item_option.excellent.weaponspeed", "Excellent: +1.5% attack speed", "Excelente: +1,5% de velocidade de ataque"),
            new LocalizationEntry("ui.item_option.excellent.weaponcritical", "Excellent: +1.5% critical chance", "Excelente: +1,5% de chance de crítico"),
            new LocalizationEntry("ui.item_option.excellent.armourhealth", "Excellent: +2% max health", "Excelente: +2% de vida máxima"),
            new LocalizationEntry("ui.item_option.excellent.armourdefense", "Excellent: 1.5% less damage taken", "Excelente: 1,5% menos dano recebido"),
            new LocalizationEntry("ui.item_option.excellent.armourthorns", "Excellent: reflects 1% of melee damage", "Excelente: devolve 1% do dano corpo a corpo"),
            new LocalizationEntry(LuckLine, "Lucky: +5% refine chance, +0.5% critical chance", "Com Sorte: +5% de chance no refino, +0,5% de crítico"),
            new LocalizationEntry(SetLine, "Ancient set: {0} (bonus with 2, 3 and 4 pieces worn)", "Conjunto antigo: {0} (bônus com 2, 3 e 4 peças vestidas)"),
            new LocalizationEntry("ui.item_option.set.ancient_carcara.name", "Old Caracara's Feathers", "Penas do Carcará Antigo"),
            new LocalizationEntry("ui.item_option.set.ancient_vaqueiro.name", "Old Cowboy's Leather", "Couro do Vaqueiro Antigo"),
            new LocalizationEntry("ui.item_option.set.ancient_benzedeira.name", "Old Healer's Lace", "Renda da Benzedeira Antiga"),
            new LocalizationEntry("ui.item_option.set.ancient_inventor.name", "Old Inventor's Gears", "Engrenagens do Inventor Antigo"),
            new LocalizationEntry("ui.item_option.set.ancient_vagalume.name", "Old Firefly's Glow", "Luz do Vaga-lume Antigo"),
            new LocalizationEntry(SocketEmpty, "Socket {0}: empty", "Engaste {0}: vazio"),
            new LocalizationEntry(SocketFilled, "Socket {0}: {1}", "Engaste {0}: {1}"),
            new LocalizationEntry("ui.item_option.engraving.engrave_ferro.name", "Iron engraving (weapon, +1% damage)", "Engaste de ferro (arma, +1% de dano)"),
            new LocalizationEntry("ui.item_option.engraving.engrave_pedra.name", "Stone engraving (armour, +1 defense)", "Engaste de pedra (armadura, +1 de defesa)"),
            new LocalizationEntry("ui.item_option.engraving.engrave_erva.name", "Herb engraving (armour, +1.5% max health)", "Engaste de erva (armadura, +1,5% de vida máxima)"),
            new LocalizationEntry("ui.item_option.engraving.engrave_caos.name", "Chaos engraving (weapon, +1.5% critical chance)", "Engaste do caos (arma, +1,5% de crítico)"),
            new LocalizationEntry(EngraveTitle, "Engrave a socket:", "Engastar:"),
            new LocalizationEntry(EngraveButton, "{0}: {1} {2} + {3} gold", "{0}: {1} {2} + {3} de ouro"),
            new LocalizationEntry(Engraved, "{0} got {1}!", "{0} ganhou {1}!"),
            new LocalizationEntry(EngraveNoSocket, "No empty socket on this piece.", "Essa peça não tem engaste vazio."),
            new LocalizationEntry(EngraveMissing, "Missing material or gold for this engraving.", "Falta material ou ouro para esse engaste."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>
        /// Every option line of a piece for the tooltip, in order: excellent lines, luck, ancient set, sockets.
        /// Empty for a plain piece.
        /// </summary>
        public static List<string> TooltipLines(LocalizationManager manager, ItemDefinition item, Inventory.InventorySlotData data)
        {
            var lines = new List<string>();
            if (data == null || !ItemOptionRules.CanHaveOptions(item))
            {
                return lines;
            }

            foreach (ExcellentLine line in ItemOptionRules.ExcellentLinesOf(data.excellentMask, ItemOptionRules.IsWeapon(item)))
            {
                lines.Add(Get(manager, line.TextKey));
            }

            if (data.luck)
            {
                lines.Add(Get(manager, LuckLine));
            }

            AncientSet set = ItemOptionCatalog.FindSet(data.ancientSetId);
            if (set != null)
            {
                lines.Add(Format(manager, SetLine, Get(manager, set.NameKey)));
            }

            if (data.sockets != null)
            {
                for (int i = 0; i < data.sockets.Length; i++)
                {
                    Engraving engraving = ItemOptionCatalog.FindEngraving(data.sockets[i]);
                    lines.Add(engraving == null
                        ? Format(manager, SocketEmpty, i + 1)
                        : Format(manager, SocketFilled, i + 1, Get(manager, engraving.NameKey)));
                }
            }

            return lines;
        }
    }
}
