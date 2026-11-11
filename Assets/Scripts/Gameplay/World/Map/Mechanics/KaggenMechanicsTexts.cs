using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Serra de Kaggen winds. Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class KaggenMechanicsTexts
    {
        public const string WindWarning = "ui.kaggen.wind_warning";
        public const string WindHit = "ui.kaggen.wind_hit";
        public const string BridgeHint = "ui.kaggen.bridge_hint";
        public const string MaitheCalm = "ui.kaggen.maithe_calm";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(WindWarning, "The mountain wind is coming: stand still and it passes", "O vento da serra vem aí: pare e ele passa"),
            new LocalizationEntry(WindHit, "Gust!", "Rajada!"),
            new LocalizationEntry(BridgeHint, "The rope bridge sways: running here tires you twice as fast", "A ponte de corda balança: correr aqui cansa o dobro"),
            new LocalizationEntry(MaitheCalm, "The wind knows Maithe: it goes round her", "O vento conhece a Maithe: passa em volta dela"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(WindWarning))
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
