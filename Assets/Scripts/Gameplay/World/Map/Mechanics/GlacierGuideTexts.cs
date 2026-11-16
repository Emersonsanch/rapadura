using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the freed Great Almas guiding the cousins over the glacier (38, <see cref="GlacierColdRules.GuideFactor"/>), and
    /// one line per cousin the first time. Same self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class GlacierGuideTexts
    {
        /// <summary>The save mark of the first walk with the guide (a time mark, set once).</summary>
        public const string FirstGuideKey = "teto_almas_guided";

        public const string Guides = "ui.glacier_guide.guides";
        public const string LineMaithe = "ui.glacier_guide.maithe";
        public const string LineIcaro = "ui.glacier_guide.icaro";
        public const string LineJoaquim = "ui.glacier_guide.joaquim";
        public const string LineMaria = "ui.glacier_guide.maria";
        public const string LineLavine = "ui.glacier_guide.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Guides, "The Great Almas walks ahead by the sheltered ways: the cold comes slower",
                "O Grande Almas vai na frente pelos caminhos abrigados: o frio chega mais devagar"),
            new LocalizationEntry(LineMaithe, "Maithe: He walks where the wind can't hear him. Clever.", "Maithe: Ele anda onde o vento não escuta. Esperto."),
            new LocalizationEntry(LineIcaro, "Ícaro: A furry map that walks. Best gear on the whole glacier.", "Ícaro: Um mapa peludo que anda. Melhor equipamento da geleira inteira."),
            new LocalizationEntry(LineJoaquim, "Joaquim: He was only hiding the sparks because he was scared. Now he shows us the way.",
                "Joaquim: Ele só escondia as faíscas porque tinha medo. Agora mostra o caminho."),
            new LocalizationEntry(LineMaria, "Maria: Wait for me, big fluffy! Your steps are too long!", "Maria: Me espera, peludão! Seus passos são grandes demais!"),
            new LocalizationEntry(LineLavine, "Lavine: Vô Jorge says a good guide is worth two blankets.", "Lavine: O Vô Jorge diz que um bom guia vale dois cobertores."),
        };

        /// <summary>The cousin's line the first time the Almas guides them (Joaquim's when no cousin is known).</summary>
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
                if (!manager.HasKey(Guides))
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
