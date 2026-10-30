using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Story;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Nova Jornada+" (<see cref="NewGamePlusHud"/>: the offer button, Vó Zefa's
    /// confirmation panel and the small cycle badge on the world-map HUD and the title's "Continuar"). The story panel
    /// of the new journey lives in <see cref="StoryTexts"/> (<c>story.new_journey.*</c>). Same self-contained pattern as
    /// <see cref="VillageChestTexts"/>: merged into the live <see cref="LocalizationManager"/> on first use (keys it
    /// already has win), pt-BR fallback without a manager.
    /// </summary>
    public static class NewGamePlusTexts
    {
        public const string Button = "ui.ngplus.button";
        /// <summary>{0} = cycle. Small badge next to the hero's level (map HUD) and on "Continuar".</summary>
        public const string Badge = "ui.ngplus.badge";
        /// <summary>{0} = the cycle about to start.</summary>
        public const string Title = "ui.ngplus.title";
        public const string Keeps = "ui.ngplus.keeps";
        /// <summary>{0} = enemy HP multiplier, {1} = enemy damage multiplier, {2} = XP multiplier (already formatted).</summary>
        public const string Resets = "ui.ngplus.resets";
        public const string MaxCycle = "ui.ngplus.max_cycle";
        public const string Start = "ui.ngplus.start";
        public const string Cancel = "ui.ngplus.cancel";
        /// <summary>Vó Zefa's line when no hero is known; per hero: <see cref="ZefaPrefix"/> + <see cref="StoryScript.HeroId"/>.</summary>
        public const string ZefaDefault = "ui.ngplus.zefa.default";
        public const string ZefaPrefix = "ui.ngplus.zefa.";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Button, "New Journey+", "Nova Jornada+"),
            new LocalizationEntry(Badge, "NG+{0}", "Jornada+{0}"),
            new LocalizationEntry(Title, "New Journey+ {0}", "Nova Jornada+ {0}"),
            new LocalizationEntry(Keeps, "You keep: level, attributes, skills, equipment, backpack, gold and Grandma Zefa's chest.", "Você mantém: nível, atributos, skills, equipamentos, mochila, ouro e o Baú da Vó Zefa."),
            new LocalizationEntry(Resets, "Starting over: the story, the region quests and the bosses. Enemies get tougher (HP ×{0}, damage ×{1}) and give more XP (×{2}) and loot.", "Recomeçam: a história, as missões das regiões e os chefes. Os inimigos ficam mais fortes (vida ×{0}, dano ×{1}) e dão mais XP (×{2}) e loot."),
            new LocalizationEntry(MaxCycle, "This is already the hardest journey: enemies won't get any tougher.", "Esta já é a jornada mais difícil: os inimigos não ficam mais fortes que isso."),
            new LocalizationEntry(Start, "Set off again", "Partir de novo"),
            new LocalizationEntry(Cancel, "Not now", "Agora não"),
            new LocalizationEntry(ZefaDefault,
                "\"The Sun Crystal is flickering again, my dears. Will you light it up one more time?\" — Grandma Zefa",
                "\"O Cristal do Sol tá piscando de novo, meus netos. Vocês topam acender ele mais uma vez?\" — Vó Zefa"),
            new LocalizationEntry(ZefaPrefix + "maithe",
                "\"Maithe, my eldest, the Sun Crystal is flickering again. Will you gather your cousins and light it once more?\" — Grandma Zefa",
                "\"Maithe, minha mais velha, o Cristal do Sol tá piscando de novo. Junta os primos e acende ele mais uma vez?\" — Vó Zefa"),
            new LocalizationEntry(ZefaPrefix + "maria",
                "\"Maria, that dream of yours came back, didn't it? The Sun Crystal is dimming again. Will you fetch the light one more time?\" — Grandma Zefa",
                "\"Maria, aquele seu sonho voltou, né? O Cristal do Sol tá apagando de novo. Vai buscar a luz mais uma vez, minha neta?\" — Vó Zefa"),
            new LocalizationEntry(ZefaPrefix + "joaquim",
                "\"Joaquim, keep that shield close: the Sun Crystal is flickering again. Will you lead the way once more?\" — Grandma Zefa",
                "\"Joaquim, deixa esse escudo por perto: o Cristal do Sol tá piscando de novo. Vai na frente mais uma vez, meu neto?\" — Vó Zefa"),
            new LocalizationEntry(ZefaPrefix + "icaro",
                "\"Ícaro, your meter isn't broken: the Sun Crystal really is dimming again. Up for one more experiment?\" — Grandma Zefa",
                "\"Ícaro, teu medidor não tá quebrado, não: o Cristal do Sol tá apagando de novo. Topa mais um experimento?\" — Vó Zefa"),
            new LocalizationEntry(ZefaPrefix + "lavine",
                "\"Lavine, my youngest, the chill is back and the Sun Crystal is flickering again. This time nobody leaves you at home.\" — Grandma Zefa",
                "\"Lavine, minha caçula, o frio voltou e o Cristal do Sol tá piscando de novo. Desta vez ninguém te deixa em casa.\" — Vó Zefa"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Vó Zefa's offer line for <paramref name="hero"/> (the generic one without a hero).</summary>
        public static string ZefaKey(CharacterId? hero) => hero.HasValue ? ZefaPrefix + StoryScript.HeroId(hero.Value) : ZefaDefault;

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
