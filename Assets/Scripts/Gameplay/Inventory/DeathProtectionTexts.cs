using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the death penalty and its protections: the "−N XP" line of the death screen,
    /// the Bênção da Romaria at each city's oratório (city hub → Serviços), the HUD badge and the Patuá da Vó Zefa item.
    /// Same self-contained pattern as the other *Texts helpers: merged into the live <see cref="LocalizationManager"/> on
    /// first use (keys it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class DeathProtectionTexts
    {
        // ---- Death screen ----
        /// <summary>{0} = XP lost.</summary>
        public const string DeathXpLost = "ui.death.xp_lost";
        public const string DeathNoXpLost = "ui.death.no_xp_lost";
        public const string DeathProtectedPatua = "ui.death.protected_patua";
        public const string DeathProtectedBlessing = "ui.death.protected_blessing";

        // ---- Oratório (city hub) ----
        /// <summary>{0} = oratory name, {1} = gold.</summary>
        public const string HubButton = "ui.romaria.hub_button";
        /// <summary>{0} = oratory name.</summary>
        public const string HubButtonBlessed = "ui.romaria.hub_button_blessed";
        /// <summary>{0} = keeper name.</summary>
        public const string Blessed = "ui.romaria.blessed";
        /// <summary>{0} = keeper name.</summary>
        public const string AlreadyBlessed = "ui.romaria.already";
        /// <summary>{0} = keeper name, {1} = gold.</summary>
        public const string NoGold = "ui.romaria.no_gold";

        // ---- HUD ----
        public const string HudBlessed = "ui.romaria.hud_blessed";
        /// <summary>{0} = patuás in the bag.</summary>
        public const string HudPatua = "ui.romaria.hud_patua";

        public const string DonaFeName = "romaria.keeper.fe.name";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(DeathXpLost, "−{0} XP", "−{0} XP"),
            new LocalizationEntry(DeathNoXpLost, "No XP lost.", "Nenhum XP perdido."),
            new LocalizationEntry(DeathProtectedPatua, "Grandma Zefa's Patuá wore out keeping you safe: nothing was lost.",
                "O Patuá da Vó Zefa se gastou protegendo vocês: nada se perdeu."),
            new LocalizationEntry(DeathProtectedBlessing, "The Pilgrimage Blessing kept your backpack safe (and most of your XP). It ends here.",
                "A Bênção da Romaria guardou a mochila (e quase todo o XP). Ela acaba aqui."),

            new LocalizationEntry(HubButton, "{0}: blessing ({1} gold)", "{0}: bênção ({1} ouro)"),
            new LocalizationEntry(HubButtonBlessed, "{0} (already blessed)", "{0} (já abençoados)"),
            new LocalizationEntry(Blessed, "{0}: “Go with God, children. The pilgrimage blessing walks with you until your next fall.”",
                "{0}: “Vão com Deus, meninos. A bênção da romaria segue com vocês até a próxima queda.”"),
            new LocalizationEntry(AlreadyBlessed, "{0}: “You are already blessed. Keep your gold.”",
                "{0}: “Vocês já estão abençoados. Guardem o ouro.”"),
            new LocalizationEntry(NoGold, "{0}: “The blessing asks {1} gold for the candles. Come back when you have it.”",
                "{0}: “A bênção pede {1} ouro para as velas. Voltem quando tiverem.”"),

            new LocalizationEntry(HudBlessed, "Blessing", "Bênção"),
            new LocalizationEntry(HudPatua, "Patuá ×{0}", "Patuá ×{0}"),

            new LocalizationEntry(DonaFeName, "Dona Fé, the Pilgrim", "Dona Fé, a Romeira"),
            new LocalizationEntry("romaria.oratory.vila_zefa.name", "Dona Fé's little chapel", "Capelinha da Dona Fé"),
            new LocalizationEntry("romaria.oratory.areia_branca.name", "Cistern shrine", "Oratório da Cisterna"),
            new LocalizationEntry("romaria.oratory.serra_alta.name", "Stone chapel", "Capela de Pedra"),
            new LocalizationEntry("romaria.oratory.lamparinas.name", "Lamp shrine", "Oratório das Lamparinas"),

            // ---- Item (Resources/Items/item_patua_vo_zefa.asset; pt-BR = the asset text) ----
            new LocalizationEntry(ContentText.ItemNameKey(PatuaRules.ItemId), "Grandma Zefa's Patuá", "Patuá da Vó Zefa"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(PatuaRules.ItemId),
                "A little cloth pouch sewn by Grandma Zefa. Wears out when you fall: you lose no XP and drop nothing.",
                "Saquinho de pano costurado pela Vó Zefa. Gasta-se ao cair: você não perde XP nem deixa cair nada."),
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
    }
}
