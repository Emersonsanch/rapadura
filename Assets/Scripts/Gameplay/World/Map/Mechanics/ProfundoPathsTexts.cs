using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the ways of the pits: the rising currents (86), the sluices (87) and the flights of the Bōlos (89). Same
    /// self-contained pattern as <see cref="ProfundoGateTexts"/>.
    /// </summary>
    public static class ProfundoPathsTexts
    {
        public const string UpdraftReady = "ui.profundo_paths.updraft_ready";
        public const string UpdraftBlocked = "ui.profundo_paths.updraft_blocked";
        public const string UpdraftResting = "ui.profundo_paths.updraft_resting";
        public const string UpdraftTide = "ui.profundo_paths.updraft_tide";
        public const string UpdraftLift = "ui.profundo_paths.updraft_lift";
        public const string UpdraftPecked = "ui.profundo_paths.updraft_pecked";
        public const string UpdraftGust = "ui.profundo_paths.updraft_gust";
        public const string UpdraftLanded = "ui.profundo_paths.updraft_landed";

        public const string WaterDeep = "ui.profundo_paths.water_deep";
        public const string WaterBites = "ui.profundo_paths.water_bites";
        public const string LeverReady = "ui.profundo_paths.lever_ready";
        public const string LeverDone = "ui.profundo_paths.lever_done";
        public const string Drained = "ui.profundo_paths.drained";
        public const string Rush = "ui.profundo_paths.rush";

        public const string CageReady = "ui.profundo_paths.cage_ready";
        public const string CageFree = "ui.profundo_paths.cage_free";
        public const string CageDark = "ui.profundo_paths.cage_dark";
        public const string CageFreeing = "ui.profundo_paths.cage_freeing";
        public const string CageLeft = "ui.profundo_paths.cage_left";
        public const string CageBreath = "ui.profundo_paths.cage_breath";
        public const string FlightLit = "ui.profundo_paths.flight_lit";
        public const string BridgeLit = "ui.profundo_paths.bridge_lit";
        public const string SparksBurst = "ui.profundo_paths.sparks_burst";
        public const string DarkPush = "ui.profundo_paths.dark_push";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(UpdraftReady, "[E] Ride the rising current up the shaft", "[E] Subir pela corrente de ar do poço"),
            new LocalizationEntry(UpdraftBlocked, "The current would carry you somewhere still closed to you", "A corrente levaria você a um lugar ainda fechado para você"),
            new LocalizationEntry(UpdraftResting, "The current is gathering again", "A corrente está se juntando de novo"),
            new LocalizationEntry(UpdraftTide, "The Tide of Darkness turns the wind down: no current rises now", "A Maré de Treva vira o vento para baixo: nenhuma corrente sobe agora"),
            new LocalizationEntry(UpdraftLift, "The current lifts you up the shaft!", "A corrente levanta você pelo poço!"),
            new LocalizationEntry(UpdraftPecked, "The wind harpies peck at you in the air", "As harpias do vento bicam você no ar"),
            new LocalizationEntry(UpdraftGust, "The current sets you down and knocks the beasts on the ring!", "A corrente deixa você no anel e derruba os bichos!"),
            new LocalizationEntry(UpdraftLanded, "The current sets you down on the outer ring", "A corrente deixa você no anel de fora"),

            new LocalizationEntry(WaterDeep, "Deep black water: you wade slowly on your Air Bubble. A sluice of the Artificers can lower it",
                "Água preta funda: você atravessa devagar com a Bolha de Ar. Uma comporta dos Artífices pode baixar a água"),
            new LocalizationEntry(WaterBites, "The bubble ran out: the black water bites", "A bolha acabou: a água preta morde"),
            new LocalizationEntry(LeverReady, "[E] Open the sluice of the Artificers and lower the water", "[E] Abrir a comporta dos Artífices e baixar a água"),
            new LocalizationEntry(LeverDone, "This sluice is open: the water stays low until the end of the day", "Esta comporta está aberta: a água fica baixa até o fim do dia"),
            new LocalizationEntry(Drained, "The sluice groans open and the black water drains away", "A comporta range, abre, e a água preta escoa"),
            new LocalizationEntry(Rush, "The rush of the water knocks the beasts in the hall!", "A correnteza derruba os bichos do salão!"),

            new LocalizationEntry(CageReady, "[E] Free the sparks of this flight (stay beside the cage for {0} s)", "[E] Libertar as faíscas deste lance (fique ao lado da gaiola por {0} s)"),
            new LocalizationEntry(CageFree, "These sparks are free: the next flight is lit", "Estas faíscas já estão livres: o próximo lance está aceso"),
            new LocalizationEntry(CageDark, "This cage is on a dark flight: free the sparks of the flight before it first", "Esta gaiola está num lance escuro: liberte antes as faíscas do lance anterior"),
            new LocalizationEntry(CageFreeing, "The cage opens slowly... the Novelo breathes at you", "A gaiola abre devagar... o Novelo sopra em você"),
            new LocalizationEntry(CageLeft, "You left the cage: the sparks are still caught", "Você saiu de perto da gaiola: as faíscas continuam presas"),
            new LocalizationEntry(CageBreath, "The breath of the Novelo chills you", "O sopro do Novelo gela você"),
            new LocalizationEntry(FlightLit, "The sparks fly free and flight {0} of the bridge lights up!", "As faíscas voam livres e o lance {0} da ponte acende!"),
            new LocalizationEntry(BridgeLit, "The whole Bridge of the Bōlos is lit, up to the Selo dos Artífices!", "A Ponte do Novelo inteira está acesa, até o Selo dos Artífices!"),
            new LocalizationEntry(SparksBurst, "The freed sparks burn the beasts round the cage!", "As faíscas libertadas queimam os bichos em volta da gaiola!"),
            new LocalizationEntry(DarkPush, "The dark of the Novelo pushes you back: free the sparks of the lit flight to light the next one",
                "A treva do Novelo empurra você de volta: liberte as faíscas do lance aceso para acender o próximo"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(UpdraftReady))
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
