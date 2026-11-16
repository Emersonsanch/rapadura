using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the headwind of the Garganta dos Ays (57, <see cref="GorgeHeadwindRules"/>) and of the Huma's shadow in the
    /// fight with the Rei-Vendaval (58, <see cref="HumaShadowRules"/>), with one line per cousin for each. Same self-contained pattern as
    /// <c>DrumRhythmTexts</c>.
    /// </summary>
    public static class FarnkeshHeightsTexts
    {
        public const string GorgeEnter = "ui.farnkesh_heights.gorge_enter";
        public const string Headwind = "ui.farnkesh_heights.headwind";
        public const string Sheltered = "ui.farnkesh_heights.sheltered";
        public const string HumaArrives = "ui.farnkesh_heights.huma_arrives";
        public const string HumaShadow = "ui.farnkesh_heights.huma_shadow";
        public const string HumaBlessed = "ui.farnkesh_heights.huma_blessed";
        public const string HumaMissed = "ui.farnkesh_heights.huma_missed";

        private const string GorgeMaithe = "ui.farnkesh_heights.gorge_maithe";
        private const string GorgeIcaro = "ui.farnkesh_heights.gorge_icaro";
        private const string GorgeJoaquim = "ui.farnkesh_heights.gorge_joaquim";
        private const string GorgeMaria = "ui.farnkesh_heights.gorge_maria";
        private const string GorgeLavine = "ui.farnkesh_heights.gorge_lavine";
        private const string HumaMaithe = "ui.farnkesh_heights.huma_maithe";
        private const string HumaIcaro = "ui.farnkesh_heights.huma_icaro";
        private const string HumaJoaquim = "ui.farnkesh_heights.huma_joaquim";
        private const string HumaMaria = "ui.farnkesh_heights.huma_maria";
        private const string HumaLavine = "ui.farnkesh_heights.huma_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(GorgeEnter, "The wind pours out of the gorge: walking into it is slow (−30%, −15% at night). Rest behind the shelter stones",
                "O vento sai da garganta: andar contra ele é lento (−30%, −15% de noite). Descanse atrás das pedras-abrigo"),
            new LocalizationEntry(Headwind, "Headwind! Every step is heavy", "Vento contra! Cada passo pesa"),
            new LocalizationEntry(Sheltered, "Behind the shelter stone the wind does not reach", "Atrás da pedra-abrigo o vento não chega"),
            new LocalizationEntry(HumaArrives, "A golden shadow circles high above the nest: the Huma has come to help", "Uma sombra dourada voa em círculos sobre o ninho: a Huma veio ajudar"),
            new LocalizationEntry(HumaShadow, "The Huma's shadow falls nearby: step into the golden ring!", "A sombra da Huma cai aqui perto: entre no anel dourado!"),
            new LocalizationEntry(HumaBlessed, "The Huma's shadow: +{0} health and +15% damage for 10 s", "Sombra da Huma: +{0} de vida e +15% de dano por 10 s"),
            new LocalizationEntry(HumaMissed, "The shadow passed. The Huma will come round again", "A sombra passou. A Huma vai dar outra volta"),
            new LocalizationEntry(GorgeMaithe, "Maithe: My wings are useless in here. The wind would throw me at the rocks.", "Maithe: Minhas asas não servem aqui. O vento me jogava nas pedras."),
            new LocalizationEntry(GorgeIcaro, "Ícaro: Lean forward, short steps, and stop at every stone. Physics, not courage.", "Ícaro: Corpo para frente, passo curto e parada em cada pedra. É física, não coragem."),
            new LocalizationEntry(GorgeJoaquim, "Joaquim: Vó Edna would say this wind is the kind that steals hats.", "Joaquim: A Vó Edna diria que esse é o vento que rouba chapéu."),
            new LocalizationEntry(GorgeMaria, "Maria: Ha! Going back will be a breeze. Literally.", "Maria: Rá! A volta vai ser moleza. Literalmente."),
            new LocalizationEntry(GorgeLavine, "Lavine: The wind sounds angry, but it's only lost. Like the Ays.", "Lavine: O vento parece bravo, mas só está perdido. Como os Ays."),
            new LocalizationEntry(HumaMaithe, "Maithe: The Huma! My wings are named after her! Hi, big sister!", "Maithe: A Huma! Minhas asas têm o nome dela! Oi, irmã grande!"),
            new LocalizationEntry(HumaIcaro, "Ícaro: She never lands. She just passes over and leaves the luck behind.", "Ícaro: Ela nunca pousa. Só passa por cima e deixa a sorte."),
            new LocalizationEntry(HumaJoaquim, "Joaquim: Vô Jorge said a bird's shadow on you is a good sign. He was right.", "Joaquim: O Vô Jorge dizia que sombra de pássaro em cima é bom sinal. Ele tinha razão."),
            new LocalizationEntry(HumaMaria, "Maria: Stand in the shadow, hit harder. I understood that rule straight away.", "Maria: Fica na sombra, bate mais forte. Essa regra eu entendi de primeira."),
            new LocalizationEntry(HumaLavine, "Lavine: The Vendaval wants her nest. She only wants to keep it. Let's help her.", "Lavine: O Vendaval quer o ninho dela. Ela só quer guardar. Vamos ajudar."),
        };

        public static string GorgeLineFor(CharacterId? hero) => Pick(hero, GorgeMaithe, GorgeIcaro, GorgeJoaquim, GorgeMaria, GorgeLavine);

        public static string HumaLineFor(CharacterId? hero) => Pick(hero, HumaMaithe, HumaIcaro, HumaJoaquim, HumaMaria, HumaLavine);

        private static string Pick(CharacterId? hero, string maithe, string icaro, string joaquim, string maria, string lavine)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return maithe;
                case CharacterId.Icaro: return icaro;
                case CharacterId.Joaquim: return joaquim;
                case CharacterId.Maria: return maria;
                case CharacterId.Lavine: return lavine;
                default: return null;
            }
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(GorgeEnter))
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
