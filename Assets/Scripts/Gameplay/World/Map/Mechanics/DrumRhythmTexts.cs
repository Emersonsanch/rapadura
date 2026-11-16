using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the Ritmo do Tambor of the Tambor Rubro (11, <see cref="DrumRhythmRules"/>): the warning, the drums, the
    /// bonus of a hunting task and one line per cousin (and Ashmunikal, the matriarch of the drums). Same self-contained pattern as
    /// <c>AreiasSouthTexts</c>.
    /// </summary>
    public static class DrumRhythmTexts
    {
        public const string Warning = "ui.tambor.warning";
        public const string Playing = "ui.tambor.playing";
        public const string PlayingFar = "ui.tambor.playing_far";
        public const string Over = "ui.tambor.over";
        public const string TaskBonus = "ui.tambor.task_bonus";
        public const string LineMaithe = "ui.tambor.line_maithe";
        public const string LineIcaro = "ui.tambor.line_icaro";
        public const string LineJoaquim = "ui.tambor.line_joaquim";
        public const string LineMaria = "ui.tambor.line_maria";
        public const string LineLavine = "ui.tambor.line_lavine";
        public const string LineAshmunikal = "ui.tambor.line_ashmunikal";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Warning, "The drummers climb the tower: the drums play soon", "Os tocadores sobem na torre: os tambores vão tocar"),
            new LocalizationEntry(Playing, "Ritmo do Tambor: +10% stamina recovery in the city, +1 Hunt Point per task finished in the Badlands", "Ritmo do Tambor: +10% de recuperação de fôlego na cidade, +1 Ponto de Caça por tarefa feita nos Ermos"),
            new LocalizationEntry(PlayingFar, "The drums of the Tambor Rubro echo in the canyons: a hunting task finished now gives +1 Hunt Point", "Os tambores do Tambor Rubro ecoam nos cânions: tarefa de caça feita agora dá +1 Ponto de Caça"),
            new LocalizationEntry(Over, "The drums go quiet", "Os tambores silenciam"),
            new LocalizationEntry(TaskBonus, "The drums counted it: +{0} Hunt Point", "Os tambores contaram: +{0} Ponto de Caça"),
            new LocalizationEntry(LineMaithe, "Maithe: I can feel the drum in my chest. My breath keeps time with it!", "Maithe: Dá para sentir o tambor no peito. Meu fôlego vai no ritmo!"),
            new LocalizationEntry(LineIcaro, "Ícaro: Every ten minutes, on the dot. Someone up there has a better clock than mine.", "Ícaro: De dez em dez minutos, certinho. Alguém lá em cima tem relógio melhor que o meu."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vô Jorge used to beat the table like that when he told stories.", "Joaquim: O Vô Jorge batia na mesa assim quando contava história."),
            new LocalizationEntry(LineMaria, "Maria: Come on, Joaquim, nobody is looking. Dance!", "Maria: Vai, Joaquim, ninguém está olhando. Dança!"),
            new LocalizationEntry(LineLavine, "Lavine: The orcs stop everything to listen. I think I'm starting to like this place.", "Lavine: Os orcs param tudo para ouvir. Acho que estou gostando daqui."),
            new LocalizationEntry(LineAshmunikal, "Ashmunikal: The drum keeps the Lamp awake. Rest while it plays, little ones.", "Ashmunikal: O tambor mantém a Lâmpada acordada. Descansem enquanto ele toca, pequenos."),
        };

        /// <summary>The line said in the city at a beat: the hero's own, or Ashmunikal's every other beat (and when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero, long beat)
        {
            if (hero == null || beat % 2L == 0L)
            {
                return LineAshmunikal;
            }

            switch (hero.Value)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Joaquim: return LineJoaquim;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return LineAshmunikal;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Warning))
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
