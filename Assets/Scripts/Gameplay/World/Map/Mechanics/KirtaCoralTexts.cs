using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of Reacender o coral of Kirta (28, <see cref="KirtaCoralRules"/>): the prompt at a dimmed coral, the spark going
    /// free, the lit coral mending, one line per cousin the first time a coral lights, and Kirta's line for the five. Same self-contained pattern
    /// as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class KirtaCoralTexts
    {
        public const string Prompt = "ui.kirta_coral.prompt";
        public const string Relit = "ui.kirta_coral.relit";
        public const string AlreadyLit = "ui.kirta_coral.already_lit";
        public const string Mends = "ui.kirta_coral.mends";
        public const string Waits = "ui.kirta_coral.waits";
        public const string AllLit = "ui.kirta_coral.all_lit";
        public const string LineMaithe = "ui.kirta_coral.maithe";
        public const string LineIcaro = "ui.kirta_coral.icaro";
        public const string LineJoaquim = "ui.kirta_coral.joaquim";
        public const string LineMaria = "ui.kirta_coral.maria";
        public const string LineLavine = "ui.kirta_coral.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Prompt, "[E] A spark is caught in the grey coral: set it free", "[E] Uma faísca está presa no coral cinza: liberte-a"),
            new LocalizationEntry(Relit, "The spark goes free and the coral lights again ({0}/{1})", "A faísca se solta e o coral acende de novo ({0}/{1})"),
            new LocalizationEntry(AlreadyLit, "This coral is already lit", "Este coral já está aceso"),
            new LocalizationEntry(Mends, "The lit coral is Living Water: rest beside it to heal", "O coral aceso é Água Viva: descanse ao lado dele para se curar"),
            new LocalizationEntry(Waits, "Catch your breath first: the coral mends whoever is not fighting", "Recupere o fôlego primeiro: o coral cura quem não está lutando"),
            new LocalizationEntry(AllLit, "Kirta: Five lights back on the reef. The mother coral is whole again: I hear my children's songs in the water.", "Kirta: Cinco luzes de volta no recife. O coral-mãe está inteiro de novo: ouço as canções dos meus filhos na água."),
            new LocalizationEntry(LineMaithe, "Maithe: It's glowing! Like the lamps on Vó Edna's porch.", "Maithe: Está brilhando! Igual às lamparinas da varanda da Vó Edna."),
            new LocalizationEntry(LineIcaro, "Ícaro: The spark was holding the colour hostage. Now it's back where it belongs.", "Ícaro: A faísca segurava a cor presa. Agora voltou para onde é dela."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Living Water in the coral... it answers my hands like the orchard's well.", "Joaquim: Água Viva no coral... responde às minhas mãos como o poço do pomar."),
            new LocalizationEntry(LineMaria, "Maria: One coral down, four to go! Race you to the next one.", "Maria: Um coral aceso, faltam quatro! Aposto corrida até o próximo."),
            new LocalizationEntry(LineLavine, "Lavine: Vô Jorge says light that's set free never forgets the way home.", "Lavine: O Vô Jorge diz que luz libertada nunca esquece o caminho de casa."),
        };

        /// <summary>The cousin's line the first time a coral lights (Joaquim's when no cousin is known).</summary>
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
                if (!manager.HasKey(Prompt))
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
