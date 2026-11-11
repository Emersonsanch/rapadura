using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Rio das Doze Horas mechanics. Same self-contained pattern as <see cref="MarGeladoMechanicsTexts"/>.</summary>
    public static class RioMechanicsTexts
    {
        public const string HourChanged = "ui.rio.hour_changed";
        public const string Board = "ui.rio.board";
        public const string PierClosed = "ui.rio.pier_closed";
        public const string BarqueBlocked = "ui.rio.barque_blocked";
        public const string ReturnBerth = "ui.rio.return_berth";
        public const string Boarding = "ui.rio.boarding";
        public const string FireHeals = "ui.rio.fire_heals";
        public const string FireWaits = "ui.rio.fire_waits";
        public const string FireBurns = "ui.rio.fire_burns";
        public const string GateOpen = "ui.rio.gate_open";
        public const string GateShut = "ui.rio.gate_shut";
        public const string GateBlocked = "ui.rio.gate_blocked";

        /// <summary>The barque's destinations, indexed by <see cref="RioHourRules.DestinationOf"/>.</summary>
        public static readonly string[] Destinations =
        {
            "ui.rio.dest.casa_de_junco", "ui.rio.dest.treno_de_areia", "ui.rio.dest.anel_quieto", "ui.rio.dest.ilha_das_palmeiras", "ui.rio.dest.proa_encalhada",
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(HourChanged, "Hour {0}: the barque lies at pier {0}, bound for {1}", "Hora {0}: a barca está no píer {0}, rumo a {1}"),
            new LocalizationEntry(Board, "[E] Board the barque: {0}", "[E] Embarcar na barca: {0}"),
            new LocalizationEntry(PierClosed, "Pier {0} of the hours: the barque comes in {1}", "Píer {0} das horas: a barca chega em {1}"),
            new LocalizationEntry(BarqueBlocked, "This barque goes to {0}, still closed to you", "Esta barca vai para {0}, ainda fechado para você"),
            new LocalizationEntry(ReturnBerth, "[E] Barque back to the Quay", "[E] Barca de volta ao Cais"),
            new LocalizationEntry(Boarding, "The barque slips off into the dark...", "A barca desliza no escuro..."),
            new LocalizationEntry(FireHeals, "The fire knows your light and mends you", "O fogo reconhece sua luz e cura você"),
            new LocalizationEntry(FireWaits, "The fire only mends those at peace", "O fogo só cura quem está em paz"),
            new LocalizationEntry(FireBurns, "The fire bites the dimmed beasts on the shore!", "O fogo morde os bichos apagados da beira!"),
            new LocalizationEntry(GateOpen, "[E] Cross through the ring's opening", "[E] Passar pela abertura do anel"),
            new LocalizationEntry(GateShut, "The opening lines up here in {0} s", "A abertura alinha aqui em {0} s"),
            new LocalizationEntry(GateBlocked, "The other side of the ring is closed to you", "O outro lado do anel ainda está fechado para você"),
            new LocalizationEntry(Destinations[0], "the Reed House", "a Casa de Junco"),
            new LocalizationEntry(Destinations[1], "the Sand Sled", "o Trenó de Areia"),
            new LocalizationEntry(Destinations[2], "the Quiet Ring", "o Anel Quieto"),
            new LocalizationEntry(Destinations[3], "the Palm Isle", "a Ilha das Palmeiras"),
            new LocalizationEntry(Destinations[4], "the Stranded Prow", "a Proa Encalhada"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(HourChanged))
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
