using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the three capital events (<see cref="CityEventsMechanicsDriver"/>): the Maré de Lodo of Jhilpur (31), the
    /// Avalanche of Sangkent (36) and the Festa das Lanternas of Mingcheng (41). Same self-contained pattern as <see cref="ZarafshanGardenTexts"/>.
    /// </summary>
    public static class CityEventsTexts
    {
        // ---- Maré de Lodo ----
        public const string TideWarning = "ui.city_event.tide.warning";
        public const string TideStart = "ui.city_event.tide.start";
        public const string TideWave = "ui.city_event.tide.wave";
        public const string TideSilt = "ui.city_event.tide.silt";
        public const string TideSilted = "ui.city_event.tide.silted";
        public const string TideAbandoned = "ui.city_event.tide.abandoned";
        public const string TideWon = "ui.city_event.tide.won";
        public const string TidePaid = "ui.city_event.tide.paid";
        public const string TideDone = "ui.city_event.tide.done";

        // ---- Avalanche ----
        public const string SnowWarning = "ui.city_event.snow.warning";
        public const string SnowLane = "ui.city_event.snow.lane";
        public const string SnowPlant = "ui.city_event.snow.plant";
        public const string SnowPlanted = "ui.city_event.snow.planted";
        public const string SnowHeld = "ui.city_event.snow.held";
        public const string SnowBuried = "ui.city_event.snow.buried";
        public const string SnowHit = "ui.city_event.snow.hit";
        public const string SnowPaid = "ui.city_event.snow.paid";
        public const string SnowNone = "ui.city_event.snow.none";
        public const string LaneWest = "ui.city_event.snow.lane.west";
        public const string LaneMiddle = "ui.city_event.snow.lane.middle";
        public const string LaneEast = "ui.city_event.snow.lane.east";

        // ---- Festa das Lanternas ----
        public const string FestaWarning = "ui.city_event.festa.warning";
        public const string FestaStart = "ui.city_event.festa.start";
        public const string FestaSoon = "ui.city_event.festa.soon";
        public const string FestaTonight = "ui.city_event.festa.tonight";
        public const string FestaRead = "ui.city_event.festa.read";
        public const string FestaTake = "ui.city_event.festa.take";
        public const string FestaDark = "ui.city_event.festa.dark";
        public const string FestaSolvedLantern = "ui.city_event.festa.solved_lantern";
        public const string FestaAnswerPrompt = "ui.city_event.festa.answer_prompt";
        public const string FestaRight = "ui.city_event.festa.right";
        public const string FestaWrong = "ui.city_event.festa.wrong";
        public const string FestaAll = "ui.city_event.festa.all";
        public const string FestaOver = "ui.city_event.festa.over";

        public const string AnswerSun = "ui.city_event.festa.answer.sun";
        public const string AnswerMoon = "ui.city_event.festa.answer.moon";
        public const string AnswerCarp = "ui.city_event.festa.answer.carp";
        public const string AnswerBell = "ui.city_event.festa.answer.bell";

        public const string RiddlePrefix = "ui.city_event.festa.riddle.";

        public const string BagFull = "ui.city_event.bag_full";

        private static readonly string[] Cousins = { "joaquim", "maria", "maithe", "icaro", "lavine" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                // ---- Maré de Lodo (31) ----
                new LocalizationEntry(TideWarning, "The lake is boiling under the ghats: the Mud Tide rises in {0}", "O lago ferve debaixo dos ghats: a Maré de Lodo sobe em {0}"),
                new LocalizationEntry(TideStart, "Mud Tide! The Children of the Mud climb the ghats: keep them off the Mother Root",
                    "Maré de Lodo! Os Filhos do Lodo sobem os ghats: segura eles longe da Raiz-Mãe"),
                new LocalizationEntry(TideWave, "A wave of Children of the Mud rises from the lake ({0}/{1})", "Uma onda de Filhos do Lodo sobe do lago ({0}/{1})"),
                new LocalizationEntry(TideSilt, "Mud on the Mother Root: {0}%", "Lodo na Raiz-Mãe: {0}%"),
                new LocalizationEntry(TideSilted, "The mud reached the Mother Root. Salmaios scrapes it off: the tide comes again while the lake still boils",
                    "O lodo chegou na Raiz-Mãe. O Salmaios raspa: a maré volta enquanto o lago ainda ferve"),
                new LocalizationEntry(TideAbandoned, "Far from the ghats, the Children of the Mud sink back into the lake", "Longe dos ghats, os Filhos do Lodo afundam de volta no lago"),
                new LocalizationEntry(TideWon, "The tide is beaten: the Mother Root shines water-green again", "A maré foi vencida: a Raiz-Mãe brilha verde-água de novo"),
                new LocalizationEntry(TidePaid, "Salmaios gives you {0} gold and {1} Spark Stone(s)", "O Salmaios dá {0} de ouro e {1} Pedra(s)-Faísca"),
                new LocalizationEntry(TideDone, "The ghats are clean for this tide. The next one rises in {0}", "Os ghats estão limpos nesta maré. A próxima sobe em {0}"),

                // ---- Avalanche (36) ----
                new LocalizationEntry(SnowWarning, "Kustai rings the bell: the snow comes down over the terraces in {0}", "O Kustai toca o sino: a neve desce sobre os terraços em {0}"),
                new LocalizationEntry(SnowLane, "Avalanche on the {0} lane in {1} s: plant the stakes and stay by them!", "Avalanche na faixa {0} em {1} s: finca as estacas e fica do lado!"),
                new LocalizationEntry(SnowPlant, "[E] Plant the juniper stakes", "[E] Fincar as estacas de zimbro"),
                new LocalizationEntry(SnowPlanted, "Stakes planted: hold them when the snow comes", "Estacas fincadas: segura quando a neve vier"),
                new LocalizationEntry(SnowHeld, "The stakes held: the terrace below is saved ({0}/3)", "As estacas aguentaram: o terraço de baixo está salvo ({0}/3)"),
                new LocalizationEntry(SnowBuried, "The snow ran down and buried a terrace", "A neve desceu e enterrou um terraço"),
                new LocalizationEntry(SnowHit, "The snow front knocks you down!", "A frente de neve derruba você!"),
                new LocalizationEntry(SnowPaid, "Abā thanks you for {0} terrace(s): {1} gold and {2} Spark Stone(s)", "A Abā agradece por {0} terraço(s): {1} de ouro e {2} Pedra(s)-Faísca"),
                new LocalizationEntry(LaneWest, "west", "oeste"),
                new LocalizationEntry(LaneMiddle, "middle", "do meio"),
                new LocalizationEntry(LaneEast, "east", "leste"),
                new LocalizationEntry(SnowNone, "The snow buried every terrace this time. Kustai will ring again", "Desta vez a neve enterrou todos os terraços. O Kustai vai tocar de novo"),

                // ---- Festa das Lanternas (41) ----
                new LocalizationEntry(FestaWarning, "Master Mōzak lights the first lanterns: the Lantern Festival begins at 19:00",
                    "O Mestre Mōzak acende as primeiras lanternas: a Festa das Lanternas começa às 19h"),
                new LocalizationEntry(FestaStart, "Lantern Festival! Five riddles hang in the lanterns of Mingcheng: answer them at the plaza",
                    "Festa das Lanternas! Cinco charadas estão nas lanternas de Mingcheng: responde na praça"),
                new LocalizationEntry(FestaSoon, "The Lantern Festival comes with the bright moon: in {0} day(s)", "A Festa das Lanternas vem com a lua clara: em {0} dia(s)"),
                new LocalizationEntry(FestaTonight, "Tonight is the Lantern Festival: from 19:00", "Hoje à noite tem Festa das Lanternas: a partir das 19h"),
                new LocalizationEntry(FestaRead, "[E] Read the riddle in the lantern", "[E] Ler a charada da lanterna"),
                new LocalizationEntry(FestaTake, "Riddle: {0} (answer at one of the four lanterns of the plaza)", "Charada: {0} (responde numa das quatro lanternas da praça)"),
                new LocalizationEntry(FestaDark, "This lantern went dark: wait a moment", "Esta lanterna apagou: espera um pouco"),
                new LocalizationEntry(FestaSolvedLantern, "This riddle is already solved", "Esta charada já foi resolvida"),
                new LocalizationEntry(FestaAnswerPrompt, "[E] Answer: {0}", "[E] Responder: {0}"),
                new LocalizationEntry(FestaRight, "Right! The lantern turns gold: {0} gold ({1}/5)", "Acertou! A lanterna fica dourada: {0} de ouro ({1}/5)"),
                new LocalizationEntry(FestaWrong, "Wrong answer: the riddle lantern goes dark for a moment", "Resposta errada: a lanterna da charada apaga um pouco"),
                new LocalizationEntry(FestaAll, "All five riddles! Mōzak gives you {0} Spark Stone(s)", "As cinco charadas! O Mōzak dá {0} Pedra(s)-Faísca"),
                new LocalizationEntry(FestaOver, "The festival's lanterns go out: the riddles wait for the next bright moon", "As lanternas da festa se apagam: as charadas esperam a próxima lua clara"),

                new LocalizationEntry(AnswerSun, "the Sun", "o Sol"),
                new LocalizationEntry(AnswerMoon, "the Moon", "a Lua"),
                new LocalizationEntry(AnswerCarp, "the Carp", "a Carpa"),
                new LocalizationEntry(AnswerBell, "the Bell", "o Sino"),

                // The riddles (answers in MingchengLanternFestaRules.RiddleAnswers: Sun, Moon, Carp, Bell, Sun, Moon, Carp, Bell).
                new LocalizationEntry(RiddlePrefix + "0", "It climbs with no ladder and sets without falling; its Barque crosses the sky every day",
                    "Sobe sem escada e desce sem cair; a Barca dele cruza o céu todo dia"),
                new LocalizationEntry(RiddlePrefix + "1", "It grows without eating and thins without fasting; its Barque carries the light home",
                    "Cresce sem comer e emagrece sem jejuar; a Barca dela leva a luz de volta"),
                new LocalizationEntry(RiddlePrefix + "2", "It swims up the river against the current, and if it leaps the falls it becomes a dragon",
                    "Sobe o rio contra a correnteza e, se pular a cachoeira, vira dragão"),
                new LocalizationEntry(RiddlePrefix + "3", "It has a mouth and doesn't eat, a tongue and doesn't speak; when it's struck, it sings",
                    "Tem boca e não come, tem língua e não fala; quando apanha, canta"),
                new LocalizationEntry(RiddlePrefix + "4", "No one looks it in the face, and everyone lives on its light",
                    "Ninguém olha para ele de frente, e todo mundo vive da luz dele"),
                new LocalizationEntry(RiddlePrefix + "5", "By day it hides; by night it paints the canal silver",
                    "De dia se esconde; de noite pinta o canal de prata"),
                new LocalizationEntry(RiddlePrefix + "6", "It wears golden scales and never shuts its eyes, not even to sleep",
                    "Usa escamas de ouro e nunca fecha os olhos, nem para dormir"),
                new LocalizationEntry(RiddlePrefix + "7", "It closes the rings of the city at night without lifting a finger",
                    "Fecha os anéis da cidade à noite sem mexer um dedo"),

                new LocalizationEntry(BagFull, "The bag is full: the reward waits until there is room", "A mochila está cheia: o prêmio espera ter espaço"),
            };

            // The cousins' lines at the end of each event (Joaquim and Maria are siblings, Maithe and Ícaro too, Lavine is cousin to all).
            string[][] tide =
            {
                new[] { "Joaquim: \"Grandpa Jorge always said mud only wins when nobody stays to scrape.\"", "Joaquim: \"O Vô Jorge sempre dizia que lodo só ganha quando ninguém fica para raspar.\"" },
                new[] { "Maria: \"The Living Water is running in the root again. I can feel it from here.\"", "Maria: \"A Água Viva voltou a correr na raiz. Dá para sentir daqui.\"" },
                new[] { "Maithe: \"Ícaro, stop stomping in the puddles. ...Fine, one more.\"", "Maithe: \"Ícaro, para de pisar nas poças. ...Tá, só mais uma.\"" },
                new[] { "Ícaro: \"Eighteen Children of the Mud and not one got to the root. Write that down, Maithe.\"", "Ícaro: \"Dezoito Filhos do Lodo e nenhum chegou na raiz. Anota isso, Maithe.\"" },
                new[] { "Lavine: \"The lake took them back gently. They were only mud that forgot it was water.\"", "Lavine: \"O lago levou eles de volta com calma. Era só lodo que esqueceu que era água.\"" },
            };
            string[][] snow =
            {
                new[] { "Joaquim: \"Stakes, a strong arm and a cousin who doesn't run. That's how you hold a mountain.\"", "Joaquim: \"Estaca, braço firme e primo que não corre. É assim que se segura uma montanha.\"" },
                new[] { "Maria: \"Grandma Edna would have made apricot tea for the whole terrace by now.\"", "Maria: \"A Vó Edna já teria feito chá de damasco para o terraço inteiro.\"" },
                new[] { "Maithe: \"Did you hear it? The snow sounds like the wind when it gives up.\"", "Maithe: \"Ouviu? A neve faz o barulho do vento quando desiste.\"" },
                new[] { "Ícaro: \"I wasn't scared. My knees were just cold.\"", "Ícaro: \"Eu não fiquei com medo. Só os joelhos que ficaram com frio.\"" },
                new[] { "Lavine: \"The terraces are still warm. Kustai can put the bell down.\"", "Lavine: \"Os terraços continuam quentinhos. O Kustai já pode largar o sino.\"" },
            };
            string[][] festa =
            {
                new[] { "Joaquim: \"Five riddles, five lanterns. Maria, admit it: I got the bell one first.\"", "Joaquim: \"Cinco charadas, cinco lanternas. Maria, admite: a do sino eu acertei primeiro.\"" },
                new[] { "Maria: \"Every lantern is a little light set free. That's what the festival is for.\"", "Maria: \"Cada lanterna é uma luzinha solta. É para isso que serve a festa.\"" },
                new[] { "Maithe: \"The carp one was easy. Fish never close their eyes, Ícaro, everybody knows that.\"", "Maithe: \"A da carpa foi fácil. Peixe nunca fecha o olho, Ícaro, todo mundo sabe.\"" },
                new[] { "Ícaro: \"Next year I'm writing the riddles. Nobody will get any.\"", "Ícaro: \"Ano que vem quem escreve as charadas sou eu. Ninguém vai acertar nenhuma.\"" },
                new[] { "Lavine: \"Grandpa Jorge would have answered them all before the lanterns were lit.\"", "Lavine: \"O Vô Jorge teria respondido todas antes de acenderem as lanternas.\"" },
            };
            AddLines(list, "tide", tide);
            AddLines(list, "snow", snow);
            AddLines(list, "festa", festa);
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

        /// <summary>The cousin's line at the end of an event (<paramref name="kind"/>: "tide", "snow" or "festa").</summary>
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

        public static string LaneKey(int lane) => lane <= 0 ? LaneWest : lane == 1 ? LaneMiddle : LaneEast;

        public static string RiddleKey(int riddle) => RiddlePrefix + riddle.ToString(CultureInfo.InvariantCulture);

        public static string AnswerKey(LanternAnswer answer)
        {
            switch (answer)
            {
                case LanternAnswer.Moon: return AnswerMoon;
                case LanternAnswer.Carp: return AnswerCarp;
                case LanternAnswer.Bell: return AnswerBell;
                default: return AnswerSun;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(TideWarning))
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
