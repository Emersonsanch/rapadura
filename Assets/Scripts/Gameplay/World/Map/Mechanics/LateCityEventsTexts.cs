using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of three more capital events (<see cref="LateCityEventsMechanicsDriver"/>): the Vendaval of Farnkesh (54), the
    /// Noite Polar of the Porto da Lua Branca (59) and the Grande Tremor of Hinokura (65). Same self-contained pattern as <see cref="CityEventsTexts"/>.
    /// </summary>
    public static class LateCityEventsTexts
    {
        // ---- Vendaval ----
        public const string GaleWarning = "ui.city_event.gale.warning";
        public const string GaleStart = "ui.city_event.gale.start";
        public const string GaleGust = "ui.city_event.gale.gust";
        public const string GaleGrab = "ui.city_event.gale.grab";
        public const string GaleGrabbed = "ui.city_event.gale.grabbed";
        public const string GaleHeld = "ui.city_event.gale.held";
        public const string GaleSwung = "ui.city_event.gale.swung";
        public const string GaleThrown = "ui.city_event.gale.thrown";
        public const string GalePaid = "ui.city_event.gale.paid";
        public const string GaleNone = "ui.city_event.gale.none";

        // ---- Noite Polar ----
        public const string PolarWarning = "ui.city_event.polar.warning";
        public const string PolarStart = "ui.city_event.polar.start";
        public const string PolarSoon = "ui.city_event.polar.soon";
        public const string PolarToday = "ui.city_event.polar.today";
        public const string PolarWave = "ui.city_event.polar.wave";
        public const string PolarDrinking = "ui.city_event.polar.drinking";
        public const string PolarOut = "ui.city_event.polar.out";
        public const string PolarRelight = "ui.city_event.polar.relight";
        public const string PolarRelit = "ui.city_event.polar.relit";
        public const string PolarDarkNear = "ui.city_event.polar.dark_near";
        public const string PolarNoOil = "ui.city_event.polar.no_oil";
        public const string PolarAbandoned = "ui.city_event.polar.abandoned";
        public const string PolarDawn = "ui.city_event.polar.dawn";
        public const string PolarPaid = "ui.city_event.polar.paid";
        public const string PolarNone = "ui.city_event.polar.none";

        // ---- Grande Tremor ----
        public const string TremorWarning = "ui.city_event.tremor.warning";
        public const string TremorCall = "ui.city_event.tremor.call";
        public const string TremorTurn = "ui.city_event.tremor.turn";
        public const string TremorTurned = "ui.city_event.tremor.turned";
        public const string TremorFed = "ui.city_event.tremor.fed";
        public const string TremorSpilled = "ui.city_event.tremor.spilled";
        public const string TremorBurn = "ui.city_event.tremor.burn";
        public const string TremorPaid = "ui.city_event.tremor.paid";
        public const string TremorNone = "ui.city_event.tremor.none";

        // ---- Sides (the moorings, the lanterns and the gates share them: W, E, S) ----
        public const string SideWest = "ui.city_event.side.west";
        public const string SideEast = "ui.city_event.side.east";
        public const string SideSouth = "ui.city_event.side.south";

        public const string BagFull = "ui.city_event.late.bag_full";

        private static readonly string[] Cousins = { "joaquim", "maria", "maithe", "icaro", "lavine" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                // ---- Vendaval (54) ----
                new LocalizationEntry(GaleWarning, "Shad-Ohrmizd's wind bells all ring at once: the Gale comes over Farnkesh in {0}",
                    "Os sinos de vento do Shad-Ohrmizd tocam todos juntos: o Vendaval chega em Farnkesh em {0}"),
                new LocalizationEntry(GaleStart, "Gale! The wind turns from gust to gust: hold the ropes of the bridge it falls on",
                    "Vendaval! O vento vira a cada rajada: segura as cordas da ponte onde ela cair"),
                new LocalizationEntry(GaleGust, "The bells of the {0} side ring: a gust on that bridge in {1} s ({2}/5)",
                    "Os sinos do lado {0} tocam: rajada naquela ponte em {1} s ({2}/5)"),
                new LocalizationEntry(GaleGrab, "[E] Grab the bridge ropes", "[E] Segurar as cordas da ponte"),
                new LocalizationEntry(GaleGrabbed, "Ropes in hand: stay by the mooring when the gust comes", "Cordas na mão: fica junto da amarra quando a rajada vier"),
                new LocalizationEntry(GaleHeld, "The ropes held: the bridge stayed still ({0} held)", "As cordas aguentaram: a ponte ficou parada ({0} seguradas)"),
                new LocalizationEntry(GaleSwung, "Nobody held the ropes: the {0} bridge swings in the wind", "Ninguém segurou as cordas: a ponte {0} balança no vento"),
                new LocalizationEntry(GaleThrown, "The swinging bridge throws you off your feet!", "A ponte balançando derruba você!"),
                new LocalizationEntry(GalePaid, "Sisin thanks you for {0} gust(s) held: {1} gold and {2} Spark Stone(s)",
                    "O Sisin agradece por {0} rajada(s) segurada(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(GaleNone, "The wind took every bridge this time. The bells will ring again", "Desta vez o vento levou todas as pontes. Os sinos vão tocar de novo"),

                // ---- Noite Polar (59) ----
                new LocalizationEntry(PolarWarning, "Makarios looks east: the sun will not come up today. The Polar Night falls at 10:00",
                    "O Makarios olha para o leste: hoje o sol não nasce. A Noite Polar cai às 10h"),
                new LocalizationEntry(PolarStart, "Polar Night! The Creeping Dark comes for the moon lanterns at the gates: keep them lit until 15:00",
                    "Noite Polar! A Treva Rastejante vem atrás das lanternas de lua das portas: mantém elas acesas até as 15h"),
                new LocalizationEntry(PolarSoon, "The next Polar Night comes with the dark of the moon: in {0} day(s)", "A próxima Noite Polar vem com a lua escura: em {0} dia(s)"),
                new LocalizationEntry(PolarToday, "Today the sun does not rise: the Polar Night falls from 10:00 to 15:00", "Hoje o sol não nasce: a Noite Polar cai das 10h às 15h"),
                new LocalizationEntry(PolarWave, "More Creeping Dark comes out of the ice ({0}/{1})", "Mais Treva Rastejante sai do gelo ({0}/{1})"),
                new LocalizationEntry(PolarDrinking, "The Dark is drinking the {0} lantern: {1}% of its light left", "A Treva está bebendo a lanterna {0}: resta {1}% da luz"),
                new LocalizationEntry(PolarOut, "The {0} lantern went out!", "A lanterna {0} apagou!"),
                new LocalizationEntry(PolarRelight, "[E] Relight the lantern with Pamour's oil", "[E] Reacender a lanterna com o óleo do Pamour"),
                new LocalizationEntry(PolarRelit, "The lantern burns again: the moon's light comes back to the gate", "A lanterna acende de novo: a luz da lua volta para a porta"),
                new LocalizationEntry(PolarDarkNear, "The Dark is too close: drive it off before relighting", "A Treva está perto demais: espanta ela antes de reacender"),
                new LocalizationEntry(PolarNoOil, "This lantern's oil is spent for today", "O óleo desta lanterna já acabou por hoje"),
                new LocalizationEntry(PolarAbandoned, "Far from the gates, the Creeping Dark slips back into the ice", "Longe das portas, a Treva Rastejante volta para dentro do gelo"),
                new LocalizationEntry(PolarDawn, "The Barque of the Moon sends its light back: the Polar Night is over",
                    "A Barca da Lua devolve a luz: a Noite Polar acabou"),
                new LocalizationEntry(PolarPaid, "Pamour thanks you for {0} lantern(s) lit: {1} gold and {2} Spark Stone(s)",
                    "O Pamour agradece por {0} lanterna(s) acesa(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(PolarNone, "Every lantern went out this time. The harbour will light them again", "Desta vez todas as lanternas apagaram. O porto vai acender de novo"),

                // ---- Grande Tremor (65) ----
                new LocalizationEntry(TremorWarning, "Kurogane drops his hammer: the volcano is about to shake Hinokura, in {0}",
                    "O Kurogane larga o martelo: o vulcão vai sacudir Hinokura, em {0}"),
                new LocalizationEntry(TremorCall, "Great Tremor! The channels changed: lava comes down the {0} gate in {1} s, turn its stone to the forge ({2}/4)",
                    "Grande Tremor! Os canais mudaram: a lava desce pela porta {0} em {1} s, vira a pedra para a forja ({2}/4)"),
                new LocalizationEntry(TremorTurn, "[E] Turn the stone to the forge", "[E] Virar a pedra para a forja"),
                new LocalizationEntry(TremorTurned, "The stone is turned: the channel runs to the forge", "A pedra está virada: o canal corre para a forja"),
                new LocalizationEntry(TremorFed, "The forge roars: the lava reached it ({0} fed)", "A forja ruge: a lava chegou nela ({0} alimentadas)"),
                new LocalizationEntry(TremorSpilled, "The stone was still turned to the street: the lava spills over the {0} road", "A pedra ainda apontava para a rua: a lava transborda na estrada {0}"),
                new LocalizationEntry(TremorBurn, "The spilled lava burns!", "A lava derramada queima!"),
                new LocalizationEntry(TremorPaid, "Kurogane thanks you for {0} forge(s) fed: {1} gold and {2} Spark Stone(s)",
                    "O Kurogane agradece por {0} forja(s) alimentada(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(TremorNone, "No forge got its lava this time. The volcano will shake again", "Desta vez nenhuma forja recebeu lava. O vulcão vai tremer de novo"),

                new LocalizationEntry(SideWest, "west", "oeste"),
                new LocalizationEntry(SideEast, "east", "leste"),
                new LocalizationEntry(SideSouth, "south", "sul"),

                new LocalizationEntry(BagFull, "The bag is full: the reward waits until there is room", "A mochila está cheia: o prêmio espera ter espaço"),
            };

            // The cousins' lines at the end of each event (Joaquim and Maria are siblings, Maithe and Ícaro too, Lavine is cousin to all).
            string[][] gale =
            {
                new[] { "Joaquim: \"Five gusts and the bridges didn't move. Ropes are easy when your arms are stubborn.\"", "Joaquim: \"Cinco rajadas e as pontes nem mexeram. Corda é fácil quando o braço é teimoso.\"" },
                new[] { "Maria: \"The wind wasn't angry. It just wanted somebody to hold on.\"", "Maria: \"O vento não estava bravo. Só queria alguém que segurasse firme.\"" },
                new[] { "Maithe: \"I heard every gust before it came. The Living Wind always says where it's going.\"", "Maithe: \"Eu ouvi cada rajada antes de chegar. O Vento Vivo sempre conta para onde vai.\"" },
                new[] { "Ícaro: \"Maithe, admit it: I ran faster than the wind on the last one.\"", "Ícaro: \"Maithe, admite: na última eu corri mais rápido que o vento.\"" },
                new[] { "Lavine: \"Grandpa Jorge tied knots like these on the boat. None of them ever let go.\"", "Lavine: \"O Vô Jorge dava nós assim no barco. Nenhum soltava nunca.\"" },
            };
            string[][] polar =
            {
                new[] { "Joaquim: \"Three lanterns, one whole night at noon. Maria, I didn't blink once.\"", "Joaquim: \"Três lanternas, uma noite inteira ao meio-dia. Maria, eu não pisquei nenhuma vez.\"" },
                new[] { "Maria: \"Every little flame we kept is light the Dark didn't get to swallow.\"", "Maria: \"Cada chaminha que a gente segurou é luz que a Treva não conseguiu engolir.\"" },
                new[] { "Maithe: \"The wind stopped all day. Even it was waiting for the sun.\"", "Maithe: \"O vento parou o dia inteiro. Até ele estava esperando o sol.\"" },
                new[] { "Ícaro: \"Next dark day I bring a bigger fire. Much bigger.\"", "Ícaro: \"No próximo dia escuro eu trago um fogo maior. Muito maior.\"" },
                new[] { "Lavine: \"The Barque of the Moon passed right over us. It saw the lanterns, I know it did.\"", "Lavine: \"A Barca da Lua passou bem em cima da gente. Ela viu as lanternas, eu sei que viu.\"" },
            };
            string[][] tremor =
            {
                new[] { "Joaquim: \"Four surges, four forges. Kurogane should put our names on an anvil.\"", "Joaquim: \"Quatro ondas, quatro forjas. O Kurogane devia gravar nossos nomes numa bigorna.\"" },
                new[] { "Maria: \"Grandma Edna says even fire works better when it has somewhere to go.\"", "Maria: \"A Vó Edna diz que até o fogo trabalha melhor quando tem para onde ir.\"" },
                new[] { "Maithe: \"Ícaro, you're glowing. Not the lava. You.\"", "Maithe: \"Ícaro, você está brilhando. Não a lava. Você.\"" },
                new[] { "Ícaro: \"The Living Fire listened to me. It went right where I pointed.\"", "Ícaro: \"O Fogo Vivo me escutou. Foi direitinho para onde eu apontei.\"" },
                new[] { "Lavine: \"The ground stopped shaking. I think the volcano just wanted to help with the forges.\"", "Lavine: \"O chão parou de tremer. Acho que o vulcão só queria ajudar nas forjas.\"" },
            };
            AddLines(list, "gale", gale);
            AddLines(list, "polar", polar);
            AddLines(list, "tremor", tremor);
            return list;
        }

        private static void AddLines(List<LocalizationEntry> list, string kind, string[][] lines)
        {
            for (int i = 0; i < Cousins.Length; i++)
            {
                list.Add(new LocalizationEntry(LineKey(kind, Cousins[i]), lines[i][0], lines[i][1]));
            }
        }

        private static string LineKey(string kind, string cousin) => $"ui.city_event.{kind}.line.{cousin}";

        /// <summary>The cousin's line at the end of an event (<paramref name="kind"/>: "gale", "polar" or "tremor").</summary>
        public static string LineFor(string kind, CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maria: return LineKey(kind, "maria");
                case CharacterId.Maithe: return LineKey(kind, "maithe");
                case CharacterId.Icaro: return LineKey(kind, "icaro");
                case CharacterId.Lavine: return LineKey(kind, "lavine");
                default: return LineKey(kind, "joaquim");
            }
        }

        /// <summary>The name of side 0 (W), 1 (E) or 2 (S).</summary>
        public static string SideKey(int side) => side <= 0 ? SideWest : side == 1 ? SideEast : SideSouth;

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(GaleWarning))
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
