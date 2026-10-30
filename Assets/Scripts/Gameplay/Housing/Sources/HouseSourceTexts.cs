using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// Localization (en + pt-BR) of the H3 sources and the house workbench: the golden "+ Encomenda" line, the toast, the
    /// reason lines and the crafting panel. Same self-contained pattern as <see cref="HouseItemTexts"/>: merged into the
    /// live manager on first use, pt-BR fallback without one.
    /// </summary>
    public static class HouseSourceTexts
    {
        /// <summary>{0} = item name.</summary>
        public const string LootLine = "ui.house_sources.loot_line";
        public const string ToastTitle = "ui.house_sources.toast_title";
        /// <summary>{0} = item name.</summary>
        public const string ToastBody = "ui.house_sources.toast_body";
        public const string ReasonEnemy = "ui.house_sources.reason_enemy";
        public const string ReasonTrophy = "ui.house_sources.reason_trophy";
        public const string ReasonBoss = "ui.house_sources.reason_boss";
        public const string ReasonQuest = "ui.house_sources.reason_quest";
        public const string ReasonDailyBox = "ui.house_sources.reason_daily_box";
        public const string ReasonDailyFirst = "ui.house_sources.reason_daily_first";
        public const string ReasonAchievement = "ui.house_sources.reason_achievement";
        public const string ReasonCraft = "ui.house_sources.reason_craft";

        public const string CraftTitle = "ui.house_sources.craft_title";
        public const string CraftIntro = "ui.house_sources.craft_intro";
        /// <summary>{0} = result name.</summary>
        public const string CraftRow = "ui.house_sources.craft_row";
        /// <summary>{0} = material name, {1} = have, {2} = need.</summary>
        public const string CraftCost = "ui.house_sources.craft_cost";
        public const string CraftDo = "ui.house_sources.craft_do";
        public const string CraftNoStation = "ui.house_sources.craft_no_station";
        public const string CraftBossNeeded = "ui.house_sources.craft_boss_needed";
        public const string CraftMissing = "ui.house_sources.craft_missing";
        public const string CraftNoHouse = "ui.house_sources.craft_no_house";
        /// <summary>{0} = result name.</summary>
        public const string CraftDone = "ui.house_sources.craft_done";
        public const string CraftFailed = "ui.house_sources.craft_failed";
        public const string CraftWardrobeNote = "ui.house_sources.craft_wardrobe_note";
        public const string StationBench = "ui.house_sources.station_bench";
        public const string StationForge = "ui.house_sources.station_forge";
        public const string StationStove = "ui.house_sources.station_stove";
        public const string StationMaster = "ui.house_sources.station_master";
        public const string Close = "ui.house_sources.close";

        private static readonly LocalizationEntry[] EntryArray =
        {
            E(LootLine, "+ Parcel: {0}", "+ Encomenda: {0}"),
            E(ToastTitle, "A parcel for the house!", "Encomenda para a casa!"),
            E(ToastBody, "{0} is waiting in the house storeroom.", "{0} está esperando no Depósito da casa."),
            E(ReasonEnemy, "Found after the fight.", "Achado depois da luta."),
            E(ReasonTrophy, "A keepsake of the first victory: the boss is free of the shadow.", "Lembrança da primeira vitória: o chefe se livrou da sombra."),
            E(ReasonBoss, "The boss left something behind.", "O chefe deixou uma lembrança."),
            E(ReasonQuest, "A thank-you for the quest.", "Um agradecimento pela missão."),
            E(ReasonDailyBox, "Grandma's little box, for today's chore.", "Caixinha da Vó, pela tarefa do dia."),
            E(ReasonDailyFirst, "Grandma kept this one for the first time.", "A Vó guardou esta para a primeira vez."),
            E(ReasonAchievement, "A reward for the achievement (bound to the account).", "Prêmio da conquista (preso à conta)."),
            E(ReasonCraft, "Made by hand at the house.", "Feito à mão na casa."),

            E(CraftTitle, "Workbench of the house", "Bancada da casa"),
            E(CraftIntro, "Furniture made with the regions' materials (bag + house chest). It goes to the storeroom.", "Móveis feitos com os materiais das regiões (mochila + baú da casa). Vão para o Depósito."),
            E(CraftRow, "{0}", "{0}"),
            E(CraftCost, "{0}: {1}/{2}", "{0}: {1}/{2}"),
            E(CraftDo, "Make", "Fazer"),
            E(CraftNoStation, "Needs the station placed in the house.", "Precisa da bancada montada na casa."),
            E(CraftBossNeeded, "Needs the keepsake of Hahya the Giant Warrior.", "Precisa da lembrança do Hahya, o Gigante Guerreiro."),
            E(CraftMissing, "Missing materials.", "Faltam materiais."),
            E(CraftNoHouse, "First get the house from Grandma Edna.", "Primeiro ganhe a casa da Vó Edna."),
            E(CraftDone, "Made: {0}.", "Feito: {0}."),
            E(CraftFailed, "Could not make it.", "Não deu para fazer."),
            E(CraftWardrobeNote, "(goes to the wardrobe)", "(vai para o guarda-roupa)"),
            E(StationBench, "Workbench", "Bancada"),
            E(StationForge, "Forge", "Forja"),
            E(StationStove, "Tandir (clay oven)", "Tandir (forno de barro)"),
            E(StationMaster, "Master craftsman's bench", "Bancada de mestre"),
            E(Close, "Close", "Fechar"),
            E(MaterialKey("item_wood"), "Wood", "Madeira"),
            E(MaterialKey("item_fiber"), "Fiber", "Fibra"),
            E(MaterialKey("item_stone"), "Stone", "Pedra"),
            E(MaterialKey("item_iron"), "Iron ingot", "Ferro (lingote)"),
            E(MaterialKey("item_iron_ore"), "Iron ore", "Minério de ferro"),
        };

        public static string MaterialKey(string itemId) => "ui.house_sources.item." + itemId;

        public static string MaterialName(LocalizationManager manager, string itemId)
        {
            string key = MaterialKey(itemId);
            return GetFallback(key) != null ? Get(manager, key) : itemId ?? string.Empty;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static IReadOnlyList<LocalizationEntry> Entries => EntryArray;

        private static LocalizationEntry E(string key, string en, string ptBR) => new LocalizationEntry(key, en, ptBR);

        public static string ReasonKey(HouseGrantReason reason)
        {
            switch (reason)
            {
                case HouseGrantReason.BossTrophy: return ReasonTrophy;
                case HouseGrantReason.BossDrop: return ReasonBoss;
                case HouseGrantReason.Quest: return ReasonQuest;
                case HouseGrantReason.DailyBox: return ReasonDailyBox;
                case HouseGrantReason.DailyFirst: return ReasonDailyFirst;
                case HouseGrantReason.Achievement: return ReasonAchievement;
                case HouseGrantReason.Craft: return ReasonCraft;
                default: return ReasonEnemy;
            }
        }

        public static string StationKey(HouseCraftRecipe recipe)
        {
            if (recipe == null) return StationBench;
            if (!string.IsNullOrEmpty(recipe.StationItemId)) return StationMaster;
            switch (recipe.Station)
            {
                case HouseItemFunction.Forge: return StationForge;
                case HouseItemFunction.Stove: return StationStove;
                default: return StationBench;
            }
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(EntryArray[0].Key)))
            {
                return;
            }

            manager.LoadEntries(EntryArray.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
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
            if (key == null) return null;
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in EntryArray) table[entry.Key] = entry.PtBR;
                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
