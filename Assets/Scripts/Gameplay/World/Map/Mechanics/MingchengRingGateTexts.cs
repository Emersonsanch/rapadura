using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the ring gates of Mingcheng (41, <see cref="MingchengRingGateMechanicsDriver"/>), with the cousins' line when the
    /// bell shuts the rings. Same self-contained pattern as <see cref="LateRouteMechanicsTexts"/>.
    /// </summary>
    public static class MingchengRingGateTexts
    {
        public const string GateWest = "ui.ring_gates.gate_west";
        public const string GateEast = "ui.ring_gates.gate_east";
        public const string GateSouth = "ui.ring_gates.gate_south";

        public const string DayOpen = "ui.ring_gates.day_open";
        public const string FestaOpen = "ui.ring_gates.festa_open";
        public const string WatchPrompt = "ui.ring_gates.watch_prompt";
        public const string PassPrompt = "ui.ring_gates.pass_prompt";
        public const string TollPrompt = "ui.ring_gates.toll_prompt";
        public const string WaitHint = "ui.ring_gates.wait_hint";
        public const string Paid = "ui.ring_gates.paid";
        public const string WatchIn = "ui.ring_gates.watch_in";
        public const string PassIn = "ui.ring_gates.pass_in";
        public const string Arrived = "ui.ring_gates.arrived";

        public const string BellOutside = "ui.ring_gates.bell_outside";
        public const string BellInside = "ui.ring_gates.bell_inside";
        public const string Drum = "ui.ring_gates.drum";
        public const string WallShut = "ui.ring_gates.wall_shut";
        public const string LeftAtNight = "ui.ring_gates.left_at_night";
        public const string ZoneBlocked = "ui.ring_gates.zone_blocked";

        private const string LinePrefix = "ui.ring_gates.bell.line.";
        private static readonly string[] Cousins = { "joaquim", "maria", "maithe", "icaro", "lavine" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(GateWest, "West Gate", "Portão Oeste"),
                new LocalizationEntry(GateEast, "East Gate", "Portão Leste"),
                new LocalizationEntry(GateSouth, "South Gate", "Portão Sul"),

                new LocalizationEntry(DayOpen, "{0} of the 4th ring: open with the drum until the bell of 20 h",
                    "{0} do 4º anel: aberto com o tambor até o sino das 20 h"),
                new LocalizationEntry(FestaOpen, "Festa das Lanternas: the gates of the rings stay open until 1 h",
                    "Festa das Lanternas: os portões dos anéis ficam abertos até 1 h"),
                new LocalizationEntry(WatchPrompt, "[E] The watch of {0} h opened the postern: go in",
                    "[E] A vigia das {0} h abriu a portinhola: entrar"),
                new LocalizationEntry(PassPrompt, "[E] Go in (Bögü's safe-conduct holds until the drum)",
                    "[E] Entrar (o salvo-conduto do Bögü vale até o tambor)"),
                new LocalizationEntry(TollPrompt, "[E] Pay {0} gold to Bögü's guard and go in (good for the whole night) · next watch at {1} h",
                    "[E] Pagar {0} de ouro à guarda do Bögü e entrar (vale a noite toda) · próxima vigia às {1} h"),
                new LocalizationEntry(WaitHint, "The bell shut the gate. The postern opens at the watch of {0} h (or now for {1} gold)",
                    "O sino fechou o portão. A portinhola abre na vigia das {0} h (ou agora por {1} de ouro)"),
                new LocalizationEntry(Paid, "{0} gold to Bögü's guard: the postern opens, and the safe-conduct holds at every gate until the drum",
                    "{0} de ouro para a guarda do Bögü: a portinhola abre, e o salvo-conduto vale em todo portão até o tambor"),
                new LocalizationEntry(WatchIn, "The watch drum: the postern opens and you go in", "O tambor da vigia: a portinhola abre e você entra"),
                new LocalizationEntry(PassIn, "The guard sees the safe-conduct and opens the postern", "A guarda vê o salvo-conduto e abre a portinhola"),
                new LocalizationEntry(Arrived, "Inside the 4th ring, among the craft districts", "Dentro do 4º anel, entre os bairros de ofício"),

                new LocalizationEntry(BellOutside, "The bell of 20 h: the gates of the 4th ring are shut. In only by the postern (watches at 22, 0, 2 and 4 h) or for {0} gold",
                    "O sino das 20 h: os portões do 4º anel fecharam. Entrar, só pela portinhola (vigias às 22, 0, 2 e 4 h) ou por {0} de ouro"),
                new LocalizationEntry(BellInside, "The bell of 20 h: the gates of the 4th ring are shut. Going out is free; coming back waits for the watch",
                    "O sino das 20 h: os portões do 4º anel fecharam. Sair é livre; voltar espera a vigia"),
                new LocalizationEntry(Drum, "The drum of 6 h: the gates of the rings open", "O tambor das 6 h: os portões dos anéis abrem"),
                new LocalizationEntry(WallShut, "The lantern fence of the 4th ring: at night you only get in through the gates",
                    "A cerca de lanternas do 4º anel: de noite só se entra pelos portões"),
                new LocalizationEntry(LeftAtNight, "You went out after the bell: back in by the postern at the watch, or for {0} gold",
                    "Você saiu depois do sino: volta pela portinhola na vigia, ou por {0} de ouro"),
                new LocalizationEntry(ZoneBlocked, "The other side is still closed to you", "O outro lado ainda está fechado para você"),
            };

            // The cousins' line when the bell shuts the rings (Joaquim and Maria are siblings, Maithe and Ícaro too, Lavine is cousin to all).
            string[][] bell =
            {
                new[] { "Joaquim: \"Bögü shuts it on the dot. Grandpa Jorge would have liked him.\"", "Joaquim: \"O Bögü fecha na hora certa. O Vô Jorge ia gostar dele.\"" },
                new[] { "Maria: \"Inside the rings the lanterns keep watch. Outside, it's us.\"", "Maria: \"Dentro dos anéis as lanternas vigiam. Do lado de fora, somos nós.\"" },
                new[] { "Maithe: \"Ícaro, it's the bell. No, we can't climb the fence.\"", "Maithe: \"Ícaro, é o sino. Não, não dá para pular a cerca.\"" },
                new[] { "Ícaro: \"Two hours to the watch? I'll count the lanterns. One...\"", "Ícaro: \"Duas horas até a vigia? Vou contar as lanternas. Uma...\"" },
                new[] { "Lavine: \"The city closes like an eye. The drum opens it again.\"", "Lavine: \"A cidade fecha como um olho. O tambor abre de novo.\"" },
            };

            for (int i = 0; i < Cousins.Length; i++)
            {
                list.Add(new LocalizationEntry(LinePrefix + Cousins[i], bell[i][0], bell[i][1]));
            }

            return list;
        }

        /// <summary>The name key of gate <paramref name="index"/> of <c>OverworldLayouts.MingchengRingGates</c> (west, east, south).</summary>
        public static string GateName(int index) => index <= 0 ? GateWest : index == 1 ? GateEast : GateSouth;

        /// <summary>The cousin's line when the bell shuts the rings.</summary>
        public static string BellLineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maria: return LinePrefix + "maria";
                case CharacterId.Maithe: return LinePrefix + "maithe";
                case CharacterId.Icaro: return LinePrefix + "icaro";
                case CharacterId.Lavine: return LinePrefix + "lavine";
                default: return LinePrefix + "joaquim";
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(GateWest))
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
