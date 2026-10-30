using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>
    /// Localization (en + pt-BR) of "Golpe em dupla": the 10 combo names, their two shout lines (spoken by the elder
    /// cousin, then the younger) and the HUD / pause menu captions. Same self-contained pattern as <c>BossTexts</c>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback
    /// otherwise.
    /// </summary>
    public static class DuoTexts
    {
        /// <summary>HUD button while charging; {0} = percent.</summary>
        public const string HudCharging = "ui.duo.hud.charging";

        /// <summary>HUD button when full.</summary>
        public const string HudReady = "ui.duo.hud.ready";

        /// <summary>HUD button once used in this encounter.</summary>
        public const string HudSpent = "ui.duo.hud.spent";

        /// <summary>HUD button during the arm delay / between uses.</summary>
        public const string HudWait = "ui.duo.hud.wait";

        /// <summary>Small caption under the button; {0} = partner name.</summary>
        public const string HudPartner = "ui.duo.hud.partner";

        /// <summary>Banner tag of the sibling combos.</summary>
        public const string SiblingBond = "ui.duo.banner.sibling";

        /// <summary>Banner tag of the cousin combos.</summary>
        public const string CousinBond = "ui.duo.banner.cousins";

        /// <summary>"{0}: {1}" — speaker and line.</summary>
        public const string BannerLine = "ui.duo.banner.line";

        /// <summary>Pause menu button; {0} = partner name.</summary>
        public const string PausePartner = "ui.duo.pause.partner";

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(HudCharging, "Duo {0}%", "Dupla {0}%"),
            new LocalizationEntry(HudReady, "Duo!", "Dupla!"),
            new LocalizationEntry(HudSpent, "Used", "Usado"),
            new LocalizationEntry(HudWait, "Duo…", "Dupla…"),
            new LocalizationEntry(HudPartner, "with {0}", "com {0}"),
            new LocalizationEntry(SiblingBond, "Sibling bond!", "Laço de irmãos!"),
            new LocalizationEntry(CousinBond, "Cousins together!", "Primos juntos!"),
            new LocalizationEntry(BannerLine, "{0}: {1}", "{0}: {1}"),
            new LocalizationEntry(PausePartner, "Duo partner: {0}", "Parceiro da dupla: {0}"),
        };

        // id → (en name, pt name, elder line en, elder line pt, younger line en, younger line pt)
        private static readonly (string Id, string NameEn, string NamePt, string L1En, string L1Pt, string L2En, string L2Pt)[] ComboTexts =
        {
            ("maithe_icaro", "Thunder Arrow", "Flecha-Trovão",
                "Icaro, charge my arrow!", "Ícaro, carrega a minha flecha!",
                "Charged, sis! Let it fly!", "Carregada, mana! Agora vai!"),
            ("maria_joaquim", "Holy Bastion", "Bastião Sagrado",
                "Joaquim, hold the line, I'll bless it!", "Joaquim, segura que eu abençoo!",
                "Nobody gets past us, sis!", "Ninguém passa pela gente, mana!"),
            ("maithe_maria", "Dawn Arrows", "Flechas da Aurora",
                "Maria, light the way!", "Maria, ilumina o caminho!",
                "Light on your arrows, cousin!", "Luz nas suas flechas, prima!"),
            ("maithe_joaquim", "Wall Leap", "Salto da Muralha",
                "Joaquim, give me a boost!", "Joaquim, me dá um impulso!",
                "Up the rock, aim from the top!", "Sobe na pedra e mira lá de cima!"),
            ("maithe_lavine", "Hunter's Moon", "Lua Caçadora",
                "Lavine, hide my arrow in the shadows!", "Lavine, esconde a minha flecha na sombra!",
                "They won't even see where it came from!", "Eles nem vão ver de onde veio!"),
            ("maria_icaro", "Blessed Drone", "Drone Abençoado",
                "Icaro, carry my light on your drone!", "Ícaro, leva a minha luz no drone!",
                "Healing mode on, cousin!", "Modo cura ligado, prima!"),
            ("maria_lavine", "Serene Twilight", "Crepúsculo Sereno",
                "Breathe, Lavine. Together, no fear.", "Respira, Lavine. Juntas, sem medo.",
                "My shadow and your light... now!", "Minha sombra e a sua luz... agora!"),
            ("joaquim_icaro", "Shock Quake", "Terremoto Elétrico",
                "Icaro, I'll crack the ground!", "Ícaro, eu racho o chão!",
                "And I'll electrify the crack!", "E eu eletrifico a rachadura!"),
            ("joaquim_lavine", "Stone Chains", "Correntes de Pedra",
                "Lavine, bind them, I'll hold them!", "Lavine, prende eles que eu seguro!",
                "Chains on my cousin's stones!", "Correntes nas pedras do primo!"),
            ("icaro_lavine", "Arcane Short Circuit", "Curto-Circuito Arcano",
                "Lavine, throw magic into my circuit!", "Lavine, joga magia no meu circuito!",
                "It's gonna short out... the good way!", "Vai dar curto... do jeito bom!"),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            foreach (var combo in ComboTexts)
            {
                entries.Add(new LocalizationEntry("duo.combo." + combo.Id + ".name", combo.NameEn, combo.NamePt));
                entries.Add(new LocalizationEntry("duo.combo." + combo.Id + ".line1", combo.L1En, combo.L1Pt));
                entries.Add(new LocalizationEntry("duo.combo." + combo.Id + ".line2", combo.L2En, combo.L2Pt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(HudReady)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

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
                var map = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    map[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = map;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>Cousin name as shown in the game (proper names are not translated).</summary>
        public static string CousinName(CharacterId id) => CharacterRegistry.Get(id).DisplayName;

        /// <summary>"Maithe: Ícaro, carrega a minha flecha!".</summary>
        public static string ShoutLine(LocalizationManager manager, DuoComboSpec combo, int index)
        {
            if (combo == null)
            {
                return string.Empty;
            }

            return Format(manager, BannerLine, CousinName(combo.Speaker(index)), Get(manager, combo.LineKey(index)));
        }
    }
}
