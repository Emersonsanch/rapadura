using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the last capital events (<see cref="FinalCityEventsMechanicsDriver"/>): A Barca Atrasou of the Cais de Mesektet
    /// (71), A Cheia of Dur-Jarras and its ziggurat's steps (77), the Chegada da Caravana of the Cais da Coluna and its low gravity (91) and the
    /// pay of the Maré de Treva (83). Same self-contained pattern as <see cref="LateCityEventsTexts"/>.
    /// </summary>
    public static class FinalCityEventsTexts
    {
        // ---- A Barca Atrasou ----
        public const string BarqueWarning = "ui.city_event.barque.warning";
        public const string BarqueStart = "ui.city_event.barque.start";
        public const string BarqueMoreScarabs = "ui.city_event.barque.more_scarabs";
        public const string BarqueGuarded = "ui.city_event.barque.guarded";
        public const string BarqueCut = "ui.city_event.barque.cut";
        public const string BarqueRopeCut = "ui.city_event.barque.rope_cut";
        public const string BarqueSails = "ui.city_event.barque.sails";
        public const string BarqueDragged = "ui.city_event.barque.dragged";
        public const string BarqueAbandoned = "ui.city_event.barque.abandoned";
        public const string BarquePaid = "ui.city_event.barque.paid";
        public const string BarqueNone = "ui.city_event.barque.none";

        // ---- A Cheia and the ziggurat's steps ----
        public const string FloodWarning = "ui.city_event.flood.warning";
        public const string FloodStart = "ui.city_event.flood.start";
        public const string FloodStep = "ui.city_event.flood.step";
        public const string FloodCurrent = "ui.city_event.flood.current";
        public const string FloodTablet = "ui.city_event.flood.tablet";
        public const string FloodPick = "ui.city_event.flood.pick";
        public const string FloodPicked = "ui.city_event.flood.picked";
        public const string FloodSaved = "ui.city_event.flood.saved";
        public const string FloodLost = "ui.city_event.flood.lost";
        public const string FloodRecedes = "ui.city_event.flood.recedes";
        public const string FloodPaid = "ui.city_event.flood.paid";
        public const string FloodNone = "ui.city_event.flood.none";
        public const string StepOpened = "ui.city_event.flood.step_opened";
        public const string StepTower = "ui.city_event.flood.step_tower";
        public const string StepNext = "ui.city_event.flood.step_next";

        // ---- Chegada da Caravana and the low gravity ----
        public const string CaravanWarning = "ui.city_event.caravan.warning";
        public const string CaravanStart = "ui.city_event.caravan.start";
        public const string CaravanGift = "ui.city_event.caravan.gift";
        public const string CaravanTake = "ui.city_event.caravan.take";
        public const string CaravanTaken = "ui.city_event.caravan.taken";
        public const string CaravanDelivered = "ui.city_event.caravan.delivered";
        public const string CaravanGone = "ui.city_event.caravan.gone";
        public const string CaravanPaid = "ui.city_event.caravan.paid";
        public const string CaravanNone = "ui.city_event.caravan.none";
        public const string GravityEnter = "ui.city_event.caravan.low_gravity";

        // ---- Maré de Treva ----
        public const string TidePaid = "ui.city_event.dark_tide.paid";

        // ---- Sides (the posts, the roads and the piers share them: W, E, S) ----
        public const string SideWest = "ui.city_event.final.side.west";
        public const string SideEast = "ui.city_event.final.side.east";
        public const string SideSouth = "ui.city_event.final.side.south";

        public const string BagFull = "ui.city_event.final.bag_full";

        private static readonly string[] Cousins = { "joaquim", "maria", "maithe", "icaro", "lavine" };

        /// <summary>The kinds of event with a cousin's line at the end (<see cref="LineFor"/>).</summary>
        public static readonly string[] LineKinds = { "barque", "flood", "caravan", "dark_tide" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                // ---- A Barca Atrasou (71) ----
                new LocalizationEntry(BarqueWarning, "Sinuhe looks at the water: the Shadow is coming for the Mesektet's ropes, in {0}",
                    "O Sinuhe olha para a água: a Sombra vem segurar as cordas da Mesektet, em {0}"),
                new LocalizationEntry(BarqueStart, "The Barque is Late! The Shadow ties the Mesektet to three posts on the roads: cut the ropes so she can sail",
                    "A Barca Atrasou! A Sombra amarrou a Mesektet em três postes nas estradas: corta as cordas para ela poder partir"),
                new LocalizationEntry(BarqueMoreScarabs, "More Shadow Scarabs crawl out of the river to guard the ropes",
                    "Mais Escaravelhos de Sombra saem do rio para guardar as cordas"),
                new LocalizationEntry(BarqueGuarded, "A scarab guards this post: drive it off before cutting the rope",
                    "Um escaravelho guarda este poste: espanta ele antes de cortar a corda"),
                new LocalizationEntry(BarqueCut, "[E] Cut the rope of shadow", "[E] Cortar a corda de sombra"),
                new LocalizationEntry(BarqueRopeCut, "The {0} rope is cut ({1}/{2})", "A corda {0} foi cortada ({1}/{2})"),
                new LocalizationEntry(BarqueSails, "Every rope cut: the Mesektet sails down the dark river on time",
                    "Todas as cordas cortadas: a Mesektet desce o rio escuro na hora certa"),
                new LocalizationEntry(BarqueDragged, "The hour is over: the Mesektet tears loose and goes down the river late",
                    "A hora acabou: a Mesektet se solta no tranco e desce o rio atrasada"),
                new LocalizationEntry(BarqueAbandoned, "Far from the quay, the scarabs slip back into the river",
                    "Longe do cais, os escaravelhos voltam para dentro do rio"),
                new LocalizationEntry(BarquePaid, "Setne thanks you for {0} rope(s) cut: {1} gold and {2} Spark Stone(s)",
                    "O Setne agradece por {0} corda(s) cortada(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(BarqueNone, "No rope was cut this time. The barque will come by again next hour",
                    "Desta vez nenhuma corda foi cortada. A barca passa de novo na próxima hora"),

                // ---- A Cheia (77) ----
                new LocalizationEntry(FloodWarning, "Utnapishtim taps the canal wall: the river is rising over Dur-Jarras, in {0}",
                    "O Utnapishtim bate na parede do canal: o rio vai subir sobre Dur-Jarras, em {0}"),
                new LocalizationEntry(FloodStart, "The Flood! The river climbs the ziggurat step by step: bring the library's tablets up before the water takes them",
                    "A Cheia! O rio sobe o zigurate degrau por degrau: leva as tábuas da biblioteca para cima antes que a água leve"),
                new LocalizationEntry(FloodStep, "The water reached the step of the {0} ({1}/{2})", "A água chegou ao degrau {0} ({1}/{2})"),
                new LocalizationEntry(FloodCurrent, "The current pulls at you out here: get back nearer the ziggurat",
                    "A correnteza puxa você aqui fora: volta para perto do zigurate"),
                new LocalizationEntry(FloodTablet, "A clay tablet washed out on the {0} road: {1} s before the water takes it ({2}/{3})",
                    "Uma tábua de argila caiu na estrada {0}: {1} s antes que a água leve ({2}/{3})"),
                new LocalizationEntry(FloodPick, "[E] Pick up the clay tablet", "[E] Pegar a tábua de argila"),
                new LocalizationEntry(FloodPicked, "Tablet in your arms: take it to the foot of the ziggurat", "Tábua nos braços: leva para o pé do zigurate"),
                new LocalizationEntry(FloodSaved, "Sin-leqi-unninni's scribes carry it up to the library ({0} saved)",
                    "Os escribas do Sin-leqi-unninni levam ela para a biblioteca ({0} salvas)"),
                new LocalizationEntry(FloodLost, "The water took the tablet on the {0} road", "A água levou a tábua da estrada {0}"),
                new LocalizationEntry(FloodRecedes, "The river goes back into its canals: the Flood is over", "O rio volta para os canais: a Cheia acabou"),
                new LocalizationEntry(FloodPaid, "Sin-leqi-unninni thanks you for {0} tablet(s) saved: {1} gold and {2} Spark Stone(s)",
                    "O Sin-leqi-unninni agradece por {0} tábua(s) salva(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(FloodNone, "No tablet reached the library this time. The river will rise again",
                    "Desta vez nenhuma tábua chegou à biblioteca. O rio vai subir de novo"),
                new LocalizationEntry(StepOpened, "The ziggurat opens the step of the {0} to you: {1} gold from its quarter",
                    "O zigurate abre para você o degrau {0}: {1} de ouro do bairro"),
                new LocalizationEntry(StepTower, "From the top of the Tower the Roda Celeste can be seen turning in the sky",
                    "Do alto da Torre dá para ver a Roda Celeste girando no céu"),
                new LocalizationEntry(StepNext, "{0} more tablet(s) and the ziggurat opens the step of the {1}",
                    "Mais {0} tábua(s) e o zigurate abre o degrau {1}"),
                new LocalizationEntry(StepKey(1), "market", "da feira"),
                new LocalizationEntry(StepKey(2), "crafts", "dos ofícios"),
                new LocalizationEntry(StepKey(3), "library", "da biblioteca"),
                new LocalizationEntry(StepKey(4), "clock", "do relógio"),
                new LocalizationEntry(StepKey(5), "Tower", "da Torre"),

                // ---- Chegada da Caravana (91) ----
                new LocalizationEntry(CaravanWarning, "Adda points at the clouds: the lights of Ammo's last caravan, mooring in {0}",
                    "A Adda aponta para as nuvens: as luzes da última caravana do Ammo, atracando em {0}"),
                new LocalizationEntry(CaravanStart, "The Caravan Arrives! Ammo brings gifts for the cities down on the island: carry each to its Barque of Light",
                    "Chegada da Caravana! O Ammo traz presentes para as cidades lá embaixo na ilha: leva cada um para a sua Barca de Luz"),
                new LocalizationEntry(CaravanGift, "A gift for the {0} pier: its barque waits {1} s ({2}/{3})",
                    "Um presente para o píer {0}: a barca espera {1} s ({2}/{3})"),
                new LocalizationEntry(CaravanTake, "[E] Take the gift from the caravan", "[E] Pegar o presente da caravana"),
                new LocalizationEntry(CaravanTaken, "Gift in hand: run to the {0} pier", "Presente na mão: corre para o píer {0}"),
                new LocalizationEntry(CaravanDelivered, "The rowers take the gift aboard and go down to the cities ({0}/{1})",
                    "Os remadores levam o presente e descem para as cidades ({0}/{1})"),
                new LocalizationEntry(CaravanGone, "The barque of the {0} pier went down without its gift", "A barca do píer {0} desceu sem o presente"),
                new LocalizationEntry(CaravanPaid, "Ammo thanks you for {0} gift(s) delivered: {1} gold and {2} Spark Stone(s)",
                    "O Ammo agradece por {0} presente(s) entregue(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(CaravanNone, "No gift went down this time. The caravan will come again",
                    "Desta vez nenhum presente desceu. A caravana vem de novo"),
                new LocalizationEntry(GravityEnter, "Up here the light weighs less: you jump higher and fall slowly",
                    "Aqui em cima a luz pesa menos: você pula mais alto e cai devagar"),

                // ---- Maré de Treva (83) ----
                new LocalizationEntry(TidePaid, "Pamour thanks you for {0} ring(s) of lamps held through the Tide: {1} gold and {2} Spark Stone(s)",
                    "O Pamour agradece por {0} anel(éis) de lâmpadas de pé na Maré: {1} de ouro e {2} Pedra(s)-Faísca"),

                new LocalizationEntry(SideWest, "west", "oeste"),
                new LocalizationEntry(SideEast, "east", "leste"),
                new LocalizationEntry(SideSouth, "south", "sul"),

                new LocalizationEntry(BagFull, "The bag is full: the reward waits until there is room", "A mochila está cheia: o prêmio espera ter espaço"),
            };

            // The cousins' lines at the end of each event (Joaquim and Maria are siblings, Maithe and Ícaro too, Lavine is cousin to all).
            string[][] barque =
            {
                new[] { "Joaquim: \"Three ropes, three cuts. The Shadow ties knots like somebody in a hurry.\"", "Joaquim: \"Três cordas, três cortes. A Sombra dá nó de quem está com pressa.\"" },
                new[] { "Maria: \"The Light cut them before I even touched them. The rope just let go.\"", "Maria: \"A Luz cortou antes de eu encostar. A corda simplesmente soltou.\"" },
                new[] { "Maithe: \"The wind went down the river with the barque. It was waiting too.\"", "Maithe: \"O vento desceu o rio junto com a barca. Ele também estava esperando.\"" },
                new[] { "Ícaro: \"Scarabs again. Next time I'm bringing a torch for each one.\"", "Ícaro: \"Escaravelho de novo. Da próxima vez eu trago uma tocha para cada um.\"" },
                new[] { "Lavine: \"A barque that sails on time is a happy barque. I could hear it creak.\"", "Lavine: \"Barca que sai na hora é barca feliz. Deu para ouvir ela estalar.\"" },
            };
            string[][] flood =
            {
                new[] { "Joaquim: \"The river and I get along. It just wanted to read the tablets too.\"", "Joaquim: \"Eu e o rio a gente se entende. Ele só queria ler as tábuas também.\"" },
                new[] { "Maria: \"Five tablets, five stories nobody will forget now.\"", "Maria: \"Cinco tábuas, cinco histórias que agora ninguém esquece.\"" },
                new[] { "Maithe: \"Grandma Edna would have read all of them before bringing them up.\"", "Maithe: \"A Vó Edna ia ler todas antes de levar para cima.\"" },
                new[] { "Ícaro: \"Clay and water. If I'd baked them first, they'd float.\"", "Ícaro: \"Barro e água. Se eu tivesse assado antes, elas boiavam.\"" },
                new[] { "Lavine: \"My feet are soaked, but the library is dry. Good trade.\"", "Lavine: \"Meus pés estão encharcados, mas a biblioteca está seca. Boa troca.\"" },
            };
            string[][] caravan =
            {
                new[] { "Joaquim: \"Six gifts, six barques. I jumped half the way, it's faster up here.\"", "Joaquim: \"Seis presentes, seis barcas. Fui pulando metade do caminho, aqui em cima é mais rápido.\"" },
                new[] { "Maria: \"Somewhere down there a city is opening a gift right now.\"", "Maria: \"Lá embaixo alguma cidade está abrindo um presente agora mesmo.\"" },
                new[] { "Maithe: \"Ícaro tried to fly with a gift in his arms. He almost did it.\"", "Maithe: \"O Ícaro tentou voar com um presente no colo. Quase conseguiu.\"" },
                new[] { "Ícaro: \"Almost is a lot, Maithe. Up here, almost is a lot.\"", "Ícaro: \"Quase é muito, Maithe. Aqui em cima, quase é muito.\"" },
                new[] { "Lavine: \"The rowers knew my name. Grandpa Jorge must have told them.\"", "Lavine: \"Os remadores sabiam meu nome. O Vô Jorge deve ter contado para eles.\"" },
            };
            string[][] tide =
            {
                new[] { "Joaquim: \"The Shadow came up to the walls and stopped. It knew better.\"", "Joaquim: \"A Sombra subiu até a muralha e parou. Ela sabia que não ia dar.\"" },
                new[] { "Maria: \"Every lamp we kept is one more window for the Light to look out of.\"", "Maria: \"Cada lâmpada acesa é mais uma janela para a Luz olhar para fora.\"" },
                new[] { "Maithe: \"The smoke wanted to blow them out. I asked the wind to say no.\"", "Maithe: \"A fumaça queria apagar. Eu pedi para o vento dizer que não.\"" },
                new[] { "Ícaro: \"Three rings of fire around the fortress. Best night of my life.\"", "Ícaro: \"Três anéis de fogo em volta da fortaleza. Melhor noite da minha vida.\"" },
                new[] { "Lavine: \"From here the lamps look like a harbour. A harbour for the Light.\"", "Lavine: \"Daqui as lâmpadas parecem um porto. Um porto para a Luz.\"" },
            };
            AddLines(list, "barque", barque);
            AddLines(list, "flood", flood);
            AddLines(list, "caravan", caravan);
            AddLines(list, "dark_tide", tide);
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

        /// <summary>The cousin's line at the end of an event (<paramref name="kind"/>: one of <see cref="LineKinds"/>).</summary>
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

        /// <summary>The name of the ziggurat's step 1 (the market) … 5 (the Tower).</summary>
        public static string StepKey(int step) => "ui.city_event.flood.step_name." + (step < 1 ? 1 : step > 5 ? 5 : step).ToString(CultureInfo.InvariantCulture);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BarqueWarning))
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
