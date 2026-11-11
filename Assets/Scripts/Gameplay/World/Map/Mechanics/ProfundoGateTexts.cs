using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the five gates of the pits (map 83). Same self-contained pattern as <see cref="ProfundoMechanicsTexts"/>.</summary>
    public static class ProfundoGateTexts
    {
        public const string Sealed = "ui.profundo_gate.sealed";
        public const string Pass = "ui.profundo_gate.pass";
        public const string Back = "ui.profundo_gate.back";
        public const string TideShut = "ui.profundo_gate.tide_shut";
        public const string Blocked = "ui.profundo_gate.blocked";
        public const string Resting = "ui.profundo_gate.resting";
        public const string Unsealed = "ui.profundo_gate.unsealed";
        public const string Breath = "ui.profundo_gate.breath";
        public const string LampBurns = "ui.profundo_gate.lamp_burns";

        /// <summary>The pits behind the gates, in gate order (Fumaça, Fogo Devorador, Vento Destruidor, Lodo, Treva).</summary>
        public static readonly string[] Pits =
        {
            "ui.profundo_gate.pit.fumaca", "ui.profundo_gate.pit.fogo", "ui.profundo_gate.pit.vento", "ui.profundo_gate.pit.lodo", "ui.profundo_gate.pit.treva",
        };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Sealed, "The gate of {0} is sealed from the far side: reach its lamp on foot to break the seal",
                "O portão d{0} está selado do outro lado: chegue à lâmpada dela a pé para romper o selo"),
            new LocalizationEntry(Pass, "[E] Go through the gate of {0}", "[E] Passar pelo portão d{0}"),
            new LocalizationEntry(Back, "[E] Back through the gate to the Fortress", "[E] Voltar pelo portão para a Fortaleza"),
            new LocalizationEntry(TideShut, "The Tide of Darkness is rising: the guardians have barred every gate", "A Maré de Treva está subindo: os guardiões trancaram todos os portões"),
            new LocalizationEntry(Blocked, "The way through that gate is still closed to you", "O caminho por esse portão ainda está fechado para você"),
            new LocalizationEntry(Resting, "The gate is still swinging shut", "O portão ainda está fechando"),
            new LocalizationEntry(Unsealed, "The seal of the gate of {0} broke: the Fortress opens the way here", "O selo do portão d{0} se rompeu: a Fortaleza abre o caminho até aqui"),
            new LocalizationEntry(Breath, "The breath of the pit passes over you in the tunnel", "O bafo da cova passa por você no túnel"),
            new LocalizationEntry(LampBurns, "The gate's lamp burns the beasts at the door!", "A lâmpada do portão queima os bichos da porta!"),
            new LocalizationEntry(Pits[0], "the Pit of Smoke", "a Cova da Fumaça"),
            new LocalizationEntry(Pits[1], "the Pit of Devouring Fire", "a Cova do Fogo Devorador"),
            new LocalizationEntry(Pits[2], "the Pit of Ruinous Wind", "a Cova do Vento Destruidor"),
            new LocalizationEntry(Pits[3], "the Pit of Mire", "a Cova do Lodo"),
            new LocalizationEntry(Pits[4], "the Pit of Darkness", "a Cova da Treva"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Sealed))
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
