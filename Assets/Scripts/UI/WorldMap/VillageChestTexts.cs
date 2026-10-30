using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Baú da Vó Zefa" (<see cref="VillageChestUIController"/>), the village hub's
    /// storage chest. Same self-contained pattern as <see cref="VillageHubTexts"/>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without a manager.
    /// </summary>
    public static class VillageChestTexts
    {
        public const string HubButton = "ui.village_chest.hub_button";
        public const string Title = "ui.village_chest.title";
        public const string Subtitle = "ui.village_chest.subtitle";
        public const string BagPane = "ui.village_chest.bag_pane";
        public const string ChestPane = "ui.village_chest.chest_pane";
        /// <summary>Unused since the grid bag (kept so old translations resolve). {0} = carried weight, {1} = weight limit.</summary>
        public const string BagWeight = "ui.village_chest.bag_weight";
        /// <summary>{0} = used cells, {1} = cells of the Mochila grid.</summary>
        public const string BagSpace = "ui.village_chest.bag_space";
        /// <summary>{0} = items still waiting in the chest's inbox.</summary>
        public const string InboxWaiting = "ui.village_chest.inbox_waiting";
        /// <summary>{0} = used slots, {1} = slot count.</summary>
        public const string ChestSlots = "ui.village_chest.chest_slots";
        public const string Hint = "ui.village_chest.hint";
        public const string StoreMaterials = "ui.village_chest.store_materials";
        public const string TakeForQuests = "ui.village_chest.take_for_quests";
        public const string Close = "ui.village_chest.close";
        /// <summary>{0} = item name.</summary>
        public const string AmountToChest = "ui.village_chest.amount_to_chest";
        /// <summary>{0} = item name.</summary>
        public const string AmountToBag = "ui.village_chest.amount_to_bag";
        public const string AmountAll = "ui.village_chest.amount_all";
        public const string AmountMove = "ui.village_chest.amount_move";
        public const string AmountCancel = "ui.village_chest.amount_cancel";
        /// <summary>{0} = units moved.</summary>
        public const string StatusStored = "ui.village_chest.status_stored";
        /// <summary>{0} = units moved.</summary>
        public const string StatusTaken = "ui.village_chest.status_taken";
        public const string StatusChestFull = "ui.village_chest.status_chest_full";
        public const string StatusBagFull = "ui.village_chest.status_bag_full";
        public const string StatusNothingToStore = "ui.village_chest.status_nothing_to_store";
        public const string StatusNothingForQuests = "ui.village_chest.status_nothing_for_quests";
        /// <summary>{0} = units moved, {1} = units that stayed in the chest.</summary>
        public const string StatusTakenPartial = "ui.village_chest.status_taken_partial";
        /// <summary>{0} = units moved, {1} = units that stayed in the bag.</summary>
        public const string StatusStoredPartial = "ui.village_chest.status_stored_partial";
        public const string EmptyChest = "ui.village_chest.empty_chest";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(HubButton, "Chest", "Baú"),
            new LocalizationEntry(Title, "Grandma Zefa's chest", "Baú da Vó Zefa"),
            new LocalizationEntry(Subtitle, "Grandma's chest keeps everything — except secrets.", "O baú da Vó guarda tudo — menos segredo."),
            new LocalizationEntry(BagPane, "Backpack", "Mochila"),
            new LocalizationEntry(ChestPane, "Chest", "Baú"),
            new LocalizationEntry(BagWeight, "Weight {0}/{1}", "Peso {0}/{1}"),
            new LocalizationEntry(BagSpace, "Space {0}/{1}", "Espaço {0}/{1}"),
            new LocalizationEntry(InboxWaiting, "{0} item(s) waiting for room in the chest.", "{0} item(ns) esperando lugar no baú."),
            new LocalizationEntry(ChestSlots, "Slots {0}/{1}", "Espaços {0}/{1}"),
            new LocalizationEntry(Hint, "Tap an item to move it to the other side; drag to carry the whole stack.", "Toque num item para levá-lo ao outro lado; arraste para levar a pilha inteira."),
            new LocalizationEntry(StoreMaterials, "Store materials", "Guardar materiais"),
            new LocalizationEntry(TakeForQuests, "Take what quests need", "Pegar o que as missões pedem"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(AmountToChest, "How many {0} go in the chest?", "Quantas unidades de {0} vão pro baú?"),
            new LocalizationEntry(AmountToBag, "How many {0} go in the backpack?", "Quantas unidades de {0} vão pra mochila?"),
            new LocalizationEntry(AmountAll, "All", "Tudo"),
            new LocalizationEntry(AmountMove, "Move", "Levar"),
            new LocalizationEntry(AmountCancel, "Cancel", "Cancelar"),
            new LocalizationEntry(StatusStored, "Stored in the chest: {0}. Grandma keeps an eye on it.", "Guardado no baú: {0}. A Vó fica de olho."),
            new LocalizationEntry(StatusTaken, "Back in the backpack: {0}.", "De volta na mochila: {0}."),
            new LocalizationEntry(StatusChestFull, "The chest is full — not even a needle fits.", "O baú tá cheio — não cabe nem agulha."),
            new LocalizationEntry(StatusBagFull, "No room in the backpack — make space first.", "Não tem espaço na mochila — abra lugar antes."),
            new LocalizationEntry(StatusNothingToStore, "No spare materials: what's in the bag, the quests still need.", "Nenhum material sobrando: o que tá na mochila, as missões ainda pedem."),
            new LocalizationEntry(StatusNothingForQuests, "The chest has nothing the quests are asking for.", "O baú não tem nada que as missões estejam pedindo."),
            new LocalizationEntry(StatusTakenPartial, "In the backpack: {0}. Left in the chest: {1} (no more room in the backpack).", "Na mochila: {0}. Ficou no baú: {1} (não coube mais na mochila)."),
            new LocalizationEntry(StatusStoredPartial, "Stored: {0}. Left in the backpack: {1} (the chest is full).", "Guardado: {0}. Ficou na mochila: {1} (o baú encheu)."),
            new LocalizationEntry(EmptyChest, "The chest is empty. Grandma says that's a waste of a good chest.", "O baú tá vazio. A Vó diz que é desperdício de baú bom."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
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

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
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
    }
}
