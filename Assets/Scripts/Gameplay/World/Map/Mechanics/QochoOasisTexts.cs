using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the Oásis de Qocho getting its water back (09, <see cref="QochoOasisRules"/>): Yardena of the cistern on the
    /// low water, the lake mending, and one line per cousin the first time it does. Same self-contained pattern as <c>AreiasSouthTexts</c>.
    /// </summary>
    public static class QochoOasisTexts
    {
        public const string Low = "ui.qocho_oasis.low";
        public const string Mends = "ui.qocho_oasis.mends";
        public const string Waits = "ui.qocho_oasis.waits";
        public const string LineMaithe = "ui.qocho_oasis.maithe";
        public const string LineIcaro = "ui.qocho_oasis.icaro";
        public const string LineJoaquim = "ui.qocho_oasis.joaquim";
        public const string LineMaria = "ui.qocho_oasis.maria";
        public const string LineLavine = "ui.qocho_oasis.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Low, "Yardena: The oasis is low. Something down in the karez is drinking our water.", "Yardena: O oásis está baixo. Alguma coisa lá no karez está bebendo a nossa água."),
            new LocalizationEntry(Mends, "The oasis is Living Water again: rest on the shore to heal", "O oásis voltou a ser Água Viva: descanse na margem para se curar"),
            new LocalizationEntry(Waits, "Catch your breath first: the water mends whoever is not fighting", "Recupere o fôlego primeiro: a água cura quem não está lutando"),
            new LocalizationEntry(LineMaithe, "Maithe: Listen to it! The karez is singing again.", "Maithe: Escuta! O karez voltou a cantar."),
            new LocalizationEntry(LineIcaro, "Ícaro: No root, no clog. Simple plumbing, really.", "Ícaro: Sem raiz, sem entupimento. Encanamento simples, no fundo."),
            new LocalizationEntry(LineJoaquim, "Joaquim: This water feels like my own hands. It is Living Water.", "Joaquim: Essa água parece a das minhas mãos. É Água Viva."),
            new LocalizationEntry(LineMaria, "Maria: Last one in is a dried date!", "Maria: Quem entrar por último é tâmara seca!"),
            new LocalizationEntry(LineLavine, "Lavine: Vó Edna would have a jar ready by now.", "Lavine: A Vó Edna já estaria com uma jarra cheia."),
        };

        /// <summary>The cousin's line the first time the oasis mends them (Joaquim's when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return LineJoaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Low))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }
    }
}
