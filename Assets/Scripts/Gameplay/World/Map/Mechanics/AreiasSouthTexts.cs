using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the far south of the Areias Flamejantes: the sunbeams of the karez (09), the singing dunes (10) and the
    /// blinding salt of Namrael (12). Same self-contained pattern as <c>OverworldTexts</c>.
    /// </summary>
    public static class AreiasSouthTexts
    {
        public const string KarezDark = "ui.areias.karez_dark";
        public const string KarezBeam = "ui.areias.karez_beam";
        public const string KarezNight = "ui.areias.karez_night";
        public const string DuneSings = "ui.areias.dune_sings";
        public const string DuneHint = "ui.areias.dune_hint";
        public const string GlareRising = "ui.areias.glare_rising";
        public const string GlareOn = "ui.areias.glare_on";
        public const string GlareShade = "ui.areias.glare_shade";
        public const string GlareOver = "ui.areias.glare_over";
        public const string StormBlind = "ui.areias.storm_blind";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(KarezDark, "The galleries are dark: mana barely comes back. Look for a sunbeam", "As galerias estão escuras: a mana quase não volta. Procure um facho de sol"),
            new LocalizationEntry(KarezBeam, "Sunbeam: mana flows again and the beasts shy from the light", "Facho de sol: a mana volta a correr e os bichos fogem da luz"),
            new LocalizationEntry(KarezNight, "Night in the karez: no sunbeams, only the lamps", "Noite no karez: nada de fachos, só as lâmpadas"),
            new LocalizationEntry(DuneSings, "The dune sings! The beasts freeze with fright", "A duna canta! Os bichos congelam de susto"),
            new LocalizationEntry(DuneHint, "Run down a dune face and the sand sings", "Desça correndo a face de uma duna e a areia canta"),
            new LocalizationEntry(GlareRising, "The salt starts to shine: get to a tower's shade before noon", "O sal começa a brilhar: chegue à sombra de uma torre antes do meio-dia"),
            new LocalizationEntry(GlareOn, "Noon on the salt: you can't see past 10 m!", "Meio-dia no sal: você não enxerga além de 10 m!"),
            new LocalizationEntry(GlareShade, "In the tower's shade: you can see again", "Na sombra da torre: dá para enxergar de novo"),
            new LocalizationEntry(GlareOver, "The salt stops blinding", "O sal parou de ofuscar"),
            new LocalizationEntry(StormBlind, "In the sand you can't see past 6 m: listen for the beasts", "Na areia você não enxerga além de 6 m: escute os bichos"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(KarezDark))
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
