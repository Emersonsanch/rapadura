using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the current of the Rio de Luz (92). Same self-contained pattern as <see cref="RioRaftTexts"/>.</summary>
    public static class CeuRiverTexts
    {
        public const string BuoyDark = "ui.ceu_river.buoy_dark";
        public const string BuoyLit = "ui.ceu_river.buoy_lit";
        public const string RideDown = "ui.ceu_river.ride_down";
        public const string RideUp = "ui.ceu_river.ride_up";
        public const string NoBuoy = "ui.ceu_river.no_buoy";
        public const string Blocked = "ui.ceu_river.blocked";
        public const string Resting = "ui.ceu_river.resting";
        public const string CarriedDown = "ui.ceu_river.carried_down";
        public const string CarriedUp = "ui.ceu_river.carried_up";
        public const string Arrived = "ui.ceu_river.arrived";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(BuoyDark, "[E] Light the buoy: the current will bring you back here", "[E] Acender a boia: a correnteza vai trazer você até aqui"),
            new LocalizationEntry(BuoyLit, "The buoy shines: the current knows this bank now", "A boia brilha: a correnteza já conhece esta margem"),
            new LocalizationEntry(RideDown, "[E] Step onto the light: the current carries you east to the next buoy (fast, {0} s)",
                "[E] Pisar na luz: a correnteza leva você para leste até a próxima boia (rápido, {0} s)"),
            new LocalizationEntry(RideUp, "[E] Step onto the light and go west against the current to the next buoy (slow, {0} s)",
                "[E] Pisar na luz e subir para oeste contra a correnteza até a próxima boia (devagar, {0} s)"),
            new LocalizationEntry(NoBuoy, "No lit buoy that way: turn round, or light the buoys along the bank", "Nenhuma boia acesa para lá: vire-se, ou acenda as boias pela margem"),
            new LocalizationEntry(Blocked, "The river runs past banks still closed to you", "O rio passa por margens ainda fechadas para você"),
            new LocalizationEntry(Resting, "The light is still settling round the buoy", "A luz ainda está assentando em volta da boia"),
            new LocalizationEntry(CarriedDown, "You walk on the light and the current runs with you", "Você anda sobre a luz e a correnteza corre junto"),
            new LocalizationEntry(CarriedUp, "Against the current, step by step: the beasts of the banks cannot follow", "Contra a correnteza, passo a passo: os bichos da margem não acompanham"),
            new LocalizationEntry(Arrived, "The light sets you on the bank by the buoy", "A luz deixa você na margem, junto da boia"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BuoyDark))
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
