using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the death penalty and its protections: the "−N XP" line of the death screen,
    /// the Lâmpada Guardiã (ex-Bênção da Romaria) at each city's lamp niche (city hub → Serviços), the HUD badge and the Tumar da Vó Edna item.
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
            new LocalizationEntry(DeathProtectedPatua, "Grandma Edna's Tumar wore out keeping you safe: nothing was lost.",
                "O Tumar da Vó Edna se gastou protegendo vocês: nada se perdeu."),
            new LocalizationEntry(DeathProtectedBlessing, "The Guardian Lamp kept your backpack safe (and most of your XP). Its flame goes out here.",
                "A Lâmpada Guardiã guardou a mochila (e quase todo o XP). A chama dela se apaga aqui."),

            new LocalizationEntry(HubButton, "{0}: lamp ({1} gold)", "{0}: lâmpada ({1} ouro)"),
            new LocalizationEntry(HubButtonBlessed, "{0} (lamp already lit)", "{0} (lâmpada já acesa)"),
            new LocalizationEntry(Blessed, "{0}: “Come back with light, children. We lit a lamp for you: it stays lit until your next fall.”",
                "{0}: “Voltem com luz, meninos. Acendemos uma lâmpada por vocês: ela fica acesa até a próxima queda.”"),
            new LocalizationEntry(AlreadyBlessed, "{0}: “Your lamp is already lit. Keep your gold.”",
                "{0}: “A lâmpada de vocês já está acesa. Guardem o ouro.”"),
            new LocalizationEntry(NoGold, "{0}: “The lamp needs {1} gold for oil. Come back when you have it.”",
                "{0}: “A lâmpada pede {1} ouro para o óleo. Voltem quando tiverem.”"),

            new LocalizationEntry(HudBlessed, "Lamp", "Lâmpada"),
            new LocalizationEntry(HudPatua, "Tumar ×{0}", "Tumar ×{0}"),

            new LocalizationEntry(DonaFeName, "Ramrat the Lampkeeper", "Ramrat, a Guardiã da Lâmpada"),
            new LocalizationEntry("romaria.oratory.vila_zefa.name", "Ramrat's lamp niche", "Nicho da Lâmpada da Ramrat"),
            new LocalizationEntry("romaria.oratory.areia_branca.name", "Cistern lamp niche", "Nicho da Lâmpada da Cisterna"),
            new LocalizationEntry("romaria.oratory.serra_alta.name", "Stone lamp niche", "Nicho de Pedra da Lâmpada"),
            new LocalizationEntry("romaria.oratory.lamparinas.name", "The Mother Lamp", "A Lâmpada-Mãe"),

            // ---- Item (Resources/Items/item_patua_vo_zefa.asset; pt-BR = the asset text) ----
            new LocalizationEntry(ContentText.ItemNameKey(PatuaRules.ItemId), "Grandma Edna's Tumar", "Tumar da Vó Edna"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(PatuaRules.ItemId),
                "A little triangle of embroidered cloth sewn by Grandma Edna. Wears out when you fall: you lose no XP and drop nothing.",
                "Triângulo de pano bordado costurado pela Vó Edna. Gasta-se ao cair: você não perde XP nem deixa cair nada."),
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
