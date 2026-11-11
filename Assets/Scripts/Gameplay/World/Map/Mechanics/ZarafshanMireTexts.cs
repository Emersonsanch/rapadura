using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the mud of the Brejo (03) and the webs of the Amoreiras (04). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class ZarafshanMireTexts
    {
        public const string MudSlows = "ui.zarafshan_mire.mud_slows";
        public const string MudJoaquim = "ui.zarafshan_mire.mud_joaquim";
        public const string MudCleaned = "ui.zarafshan_mire.mud_cleaned";
        public const string WebCaught = "ui.zarafshan_mire.web_caught";
        public const string WebCut = "ui.zarafshan_mire.web_cut";
        public const string WebAhead = "ui.zarafshan_mire.web_ahead";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(MudSlows, "Mud: off the log walkways you wade slowly", "Lama: fora das passarelas de tronco você anda devagar"),
            new LocalizationEntry(MudJoaquim, "Mud: your skills' Living Water clears it for a while", "Lama: a Água Viva das suas skills limpa a lama por um tempo"),
            new LocalizationEntry(MudCleaned, "Living Water clears the mud round here", "A Água Viva limpa a lama aqui em volta"),
            new LocalizationEntry(WebCaught, "Caught in a silk web!", "Preso numa teia de seda!"),
            new LocalizationEntry(WebCut, "The web is cut", "A teia foi cortada"),
            new LocalizationEntry(WebAhead, "A glowing web between the trees: cut it with a skill or go round", "Uma teia brilhante entre as árvores: corte com uma skill ou dê a volta"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(MudSlows))
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
