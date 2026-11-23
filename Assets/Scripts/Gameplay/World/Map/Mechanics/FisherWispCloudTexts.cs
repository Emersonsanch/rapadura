using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the cloud of marsh lights the freed Pescador de Luzes lets go (33, <see cref="FisherWispCloudRules"/>).</summary>
    public static class FisherWispCloudTexts
    {
        public const string Freed = "ui.fisher_wisps.freed";
        public const string InCloud = "ui.fisher_wisps.in_cloud";
        public const string NightClear = "ui.fisher_wisps.night_clear";
        public const string LineJoaquim = "ui.fisher_wisps.line.joaquim";
        public const string LineMaria = "ui.fisher_wisps.line.maria";
        public const string LineMaithe = "ui.fisher_wisps.line.maithe";
        public const string LineIcaro = "ui.fisher_wisps.line.icaro";
        public const string LineLavine = "ui.fisher_wisps.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Freed, "The Light Fisher opens his mouth: a cloud of marsh lights rises over the ring for {0} s, and they light the swamp for {1} min",
                "O Pescador de Luzes abre a boca: uma nuvem de fogos-fátuos sobe sobre o anel por {0} s, e eles clareiam o pântano por {1} min"),
            new LocalizationEntry(InCloud, "Inside the cloud of lights your mana comes back", "Dentro da nuvem de luzes a sua mana volta"),
            new LocalizationEntry(NightClear, "The freed marsh lights fly over the stilts: the lilac mist hides nothing", "Os fogos-fátuos soltos voam sobre as palafitas: a névoa lilás não esconde nada"),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"Light kept for oneself goes out, Grandpa Jorge says. Look how it shines now that it's shared.\"",
                "Joaquim: \"Luz guardada só para si apaga, diz o Vô Jorge. Olha como brilha agora que é de todo mundo.\""),
            new LocalizationEntry(LineMaria, "Maria: \"Each one of those is a spark going home. Go on, little ones!\"",
                "Maria: \"Cada uma dessas é uma faísca voltando para casa. Vão, pequenas!\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Ícaro, stand still: they're landing on your hair.\"",
                "Maithe: \"Ícaro, fica parado: elas estão pousando no seu cabelo.\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"He swallowed all that? No wonder he was grumpy.\"",
                "Ícaro: \"Ele engoliu tudo isso? Por isso era tão rabugento.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"Grandma Edna would wrap one in a cloth for the house. I'll just watch.\"",
                "Lavine: \"A Vó Edna enrolaria uma num pano para levar para casa. Eu só vou olhar.\""),
        };

        /// <summary>The cousin's line when the Pescador lets the lights go.</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Lavine: return LineLavine;
                default: return LineJoaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Freed))
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

        public static string Format(LocalizationManager manager, string key, params object[] arguments) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), arguments);
    }
}
