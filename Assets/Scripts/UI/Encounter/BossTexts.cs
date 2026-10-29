using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Encounter
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the arena's boss intro banner (<see cref="BossIntroBannerController"/>) and
    /// bag-full loot toast (<see cref="BagFullToastController"/>);
    /// the map's "Enfrentar o chefe" caption lives in <c>WorldMapTexts.BossChallenge</c>. Same self-contained pattern as <c>HapticsTexts</c>/<c>TutorialTexts</c>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win),
    /// pt-BR fallback otherwise. Boss names themselves come from <c>ContentText</c> (<c>enemy.&lt;asset&gt;.name</c>).
    /// </summary>
    public static class BossTexts
    {
        /// <summary>Small line above the boss name on the arena banner.</summary>
        public const string BannerTitle = "ui.boss.banner.title";

        /// <summary>Encounter intro banner title while enemies hold their aggro (EncounterIntroEvent).</summary>
        public const string PrepareTitle = "ui.encounter.prepare.title";

        /// <summary>Shown when the intro countdown reaches zero.</summary>
        public const string PrepareGo = "ui.encounter.prepare.go";

        /// <summary>Arena toast title when loot did not fit (<see cref="BagFullToastController"/>).</summary>
        public const string BagFullTitle = "ui.loot.bag_full.title";

        /// <summary>Toast line listing what was lost; {0} = "2 Minério de Ferro, Espada de Ferro".</summary>
        public const string BagFullLost = "ui.loot.bag_full.lost";

        /// <summary>Toast line when quest items went in over the weight limit.</summary>
        public const string BagFullKeptQuest = "ui.loot.bag_full.kept_quest";

        /// <summary>Toast hint on how to make room.</summary>
        public const string BagFullHint = "ui.loot.bag_full.hint";

        /// <summary>Warning text floating over the corpse; {0} = "2 Minério de Ferro".</summary>
        public const string LootNoRoom = "ui.loot.no_room";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(BannerTitle, "Region boss", "Chefe da região"),
            new LocalizationEntry(PrepareTitle, "Get ready!", "Prepare-se!"),
            new LocalizationEntry(PrepareGo, "Go!", "Já!"),
            new LocalizationEntry(BagFullTitle, "Bag full", "Mochila cheia"),
            new LocalizationEntry(BagFullLost, "Left behind: {0}", "Ficou para trás: {0}"),
            new LocalizationEntry(BagFullKeptQuest, "Quest items kept over the weight limit", "Itens de missão guardados acima do peso"),
            new LocalizationEntry(BagFullHint, "Sell, use or craft items in the village to make room", "Venda, use ou fabrique itens na vila para abrir espaço"),
            new LocalizationEntry(LootNoRoom, "No room: {0}", "Sem espaço: {0}"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(BannerTitle)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
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

        /// <summary><see cref="Get"/> + <c>string.Format</c>; falls back to the pt-BR pattern if a translation's placeholders are broken.</summary>
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
    }
}
