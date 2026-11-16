using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the red lanterns and the drum of Nian's ring (45, <see cref="NianScareRules"/>), and one line per cousin the
    /// first time a lantern or the drum frightens it. Same self-contained pattern as <see cref="QochoOasisTexts"/>.
    /// </summary>
    public static class NianScareTexts
    {
        public const string LanternPrompt = "ui.nian_scare.lantern_prompt";
        public const string LanternLit = "ui.nian_scare.lantern_lit";
        public const string LanternBurning = "ui.nian_scare.lantern_burning";
        public const string LanternCooling = "ui.nian_scare.lantern_cooling";
        public const string LanternSwatted = "ui.nian_scare.lantern_swatted";
        public const string DrumPrompt = "ui.nian_scare.drum_prompt";
        public const string DrumBeat = "ui.nian_scare.drum_beat";
        public const string DrumResting = "ui.nian_scare.drum_resting";
        public const string FrightDrum = "ui.nian_scare.fright_drum";
        public const string FrightLantern = "ui.nian_scare.fright_lantern";
        public const string LineMaithe = "ui.nian_scare.maithe";
        public const string LineIcaro = "ui.nian_scare.icaro";
        public const string LineJoaquim = "ui.nian_scare.joaquim";
        public const string LineMaria = "ui.nian_scare.maria";
        public const string LineLavine = "ui.nian_scare.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(LanternPrompt, "[E] Light the red lantern (Nian runs from red)", "[E] Acender a lanterna vermelha (o Nian foge do vermelho)"),
            new LocalizationEntry(LanternLit, "The red lantern burns: fight beside it", "A lanterna vermelha acesa: lute do lado dela"),
            new LocalizationEntry(LanternBurning, "This lantern is already burning", "Essa lanterna já está acesa"),
            new LocalizationEntry(LanternCooling, "The wick is still smoking: {0} s", "O pavio ainda está fumegando: {0} s"),
            new LocalizationEntry(LanternSwatted, "Nian swats the red lantern out as it flees", "O Nian apaga a lanterna vermelha com a pata ao fugir"),
            new LocalizationEntry(DrumPrompt, "[E] Beat the drum when Nian crouches", "[E] Bater o tambor quando o Nian se agachar"),
            new LocalizationEntry(DrumBeat, "BOOM!", "BUM!"),
            new LocalizationEntry(DrumResting, "The drumskin is still shaking", "O couro do tambor ainda está tremendo"),
            new LocalizationEntry(FrightDrum, "The drum! Nian leaps back", "O tambor! O Nian pula para trás"),
            new LocalizationEntry(FrightLantern, "The red light! Nian leaps back", "A luz vermelha! O Nian pula para trás"),
            new LocalizationEntry(LineMaithe, "Maithe: It hates the sound more than the light. I can hear it flinch.", "Maithe: Ele odeia mais o som que a luz. Dá para ouvir ele tremer."),
            new LocalizationEntry(LineIcaro, "Ícaro: Red and loud. My kind of plan.", "Ícaro: Vermelho e barulhento. Meu tipo de plano."),
            new LocalizationEntry(LineJoaquim, "Joaquim: We don't have to hurt it. Just send it back to sleep.", "Joaquim: Não precisa machucar. É só mandar ele de volta para dormir."),
            new LocalizationEntry(LineMaria, "Maria: Again! Do the drum again!", "Maria: De novo! Bate o tambor de novo!"),
            new LocalizationEntry(LineLavine, "Lavine: Vô Jorge says every beast has something it's afraid of.", "Lavine: O Vô Jorge diz que todo bicho tem medo de alguma coisa."),
        };

        /// <summary>The cousin's line the first time a lantern or the drum frightens Nian (Joaquim's when no cousin is known).</summary>
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
                if (!manager.HasKey(LanternPrompt))
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
