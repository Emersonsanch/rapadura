using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the boat of the Lago do Limiar (19) and the lamps of the Salão de Tages (20). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class LimiarLakeTexts
    {
        public const string BoatReady = "ui.limiar_lake.boat_ready";
        public const string BoatResting = "ui.limiar_lake.boat_resting";
        public const string BoatSailing = "ui.limiar_lake.boat_sailing";
        public const string BoatLanded = "ui.limiar_lake.boat_landed";
        public const string LampOut = "ui.limiar_lake.lamp_out";
        public const string LampBurning = "ui.limiar_lake.lamp_burning";
        public const string LampLit = "ui.limiar_lake.lamp_lit";
        public const string HallSighs = "ui.limiar_lake.hall_sighs";
        public const string BeastsBold = "ui.limiar_lake.beasts_bold";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(BoatReady, "[E] Take the boat across the dark lake", "[E] Pegar o barco e atravessar o lago escuro"),
            new LocalizationEntry(BoatResting, "The boatman is tying up the boat…", "O barqueiro está amarrando o barco…"),
            new LocalizationEntry(BoatSailing, "The boat glides over the dark water", "O barco desliza sobre a água escura"),
            new LocalizationEntry(BoatLanded, "The boat touches the shore", "O barco encosta na margem"),
            new LocalizationEntry(LampOut, "[E] Light the lamp: in the dark the beasts grow bold", "[E] Acender a lâmpada: no escuro os bichos ficam mais fortes"),
            new LocalizationEntry(LampBurning, "The lamp burns ({0} min left): the beasts nearby are calmer", "A lâmpada está acesa (falta {0} min): os bichos de perto ficam mais calmos"),
            new LocalizationEntry(LampLit, "The lamp is lit: the beasts round it calm down ({0}/{1} lamps burning)", "A lâmpada acendeu: os bichos em volta se acalmam ({0}/{1} lâmpadas acesas)"),
            new LocalizationEntry(HallSighs, "The hall sighs, and a lamp goes out", "O salão suspira, e uma lâmpada se apaga"),
            new LocalizationEntry(BeastsBold, "Beasts by an unlit lamp hit harder and run faster", "Bichos perto de lâmpada apagada batem mais forte e correm mais"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BoatReady))
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
