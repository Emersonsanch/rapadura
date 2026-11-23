using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>PT/EN texts of Apep spitting the river (<see cref="ApepSpitRiverRules"/>, map 76), loaded on first use like the other mechanics' texts.</summary>
    public static class ApepSpitRiverTexts
    {
        public const string Spat = "ui.apep_spit.spat";
        public const string SpatMidnight = "ui.apep_spit.spat_midnight";
        public const string BagFull = "ui.apep_spit.bag_full";
        public const string Paid = "ui.apep_spit.paid";

        private const string LineMaithe = "ui.apep_spit.line_maithe";
        private const string LineIcaro = "ui.apep_spit.line_icaro";
        private const string LineJoaquim = "ui.apep_spit.line_joaquim";
        private const string LineMaria = "ui.apep_spit.line_maria";
        private const string LineLavine = "ui.apep_spit.line_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Spat, "Apep spits the river back and sinks into the sand until another night: {0} Mixture Stone came out with the water",
                "Apep cospe o rio de volta e afunda na areia até outra noite: {0} Pedra da Mistura saiu junto com a água"),
            new LocalizationEntry(SpatMidnight, "Beaten at midnight, Apep spits the whole river: {0} Mixture Stones came out with the water",
                "Vencida na meia-noite, Apep cospe o rio inteiro: {0} Pedras da Mistura saíram junto com a água"),
            new LocalizationEntry(BagFull, "The Mixture Stones wait on the sand: make room in the bag ({0})",
                "As Pedras da Mistura esperam na areia: abra espaço na mochila ({0})"),
            new LocalizationEntry(Paid, "You pick up the Mixture Stones Apep spat: +{0}", "Você junta as Pedras da Mistura que Apep cuspiu: +{0}"),
            new LocalizationEntry(LineMaithe, "Maithe: She swallowed light and dark together. That's why the stone comes out mixed.",
                "Maithe: Ela engoliu luz e treva juntas. Por isso a pedra sai misturada."),
            new LocalizationEntry(LineIcaro, "Ícaro: She'll be back tomorrow night. The barque too. That's how this river goes.",
                "Ícaro: Amanhã à noite ela volta. A barca também. É assim que esse rio anda."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vô Jorge would say: whoever drinks too much gives it all back.",
                "Joaquim: O Vô Jorge diria: quem bebe demais devolve tudo."),
            new LocalizationEntry(LineMaria, "Maria: Ha! Spat out like a seed. Next night, again!", "Maria: Rá! Cuspiu feito caroço. Noite que vem, de novo!"),
            new LocalizationEntry(LineLavine, "Lavine: The water is back in the river. Listen, the barque is moving.",
                "Lavine: A água voltou para o rio. Escuta, a barca está andando."),
        };

        /// <summary>The cousin's line when Apep spits the river (null when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Joaquim: return LineJoaquim;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return null;
            }
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Spat))
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
