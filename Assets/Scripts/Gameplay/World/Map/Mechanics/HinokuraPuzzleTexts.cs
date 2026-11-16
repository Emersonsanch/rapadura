using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the 2026-11-16 Hinokura mechanics: the raked gravel and the hidden tea house (66,
    /// <see cref="HinokuraGravelRules"/>), the nine furnaces (69, <see cref="NineFurnacesPuzzle"/>) and the volcano goat's hooves on the eruption
    /// (67). Mestra Akane and Kurogane are Hinokura's own NPCs; the cousins say one line each. Same self-contained pattern as <c>DrumRhythmTexts</c>.
    /// </summary>
    public static class HinokuraPuzzleTexts
    {
        public const string GravelRule = "ui.hinokura_gravel.rule";
        public const string GravelPrompt = "ui.hinokura_gravel.prompt";
        public const string GravelRaked = "ui.hinokura_gravel.raked";
        public const string GravelCalm = "ui.hinokura_gravel.calm";
        public const string GravelAlreadyCalm = "ui.hinokura_gravel.already_calm";
        public const string TeaPrompt = "ui.hinokura_gravel.tea_prompt";
        public const string TeaFirst = "ui.hinokura_gravel.tea_first";
        public const string TeaDaily = "ui.hinokura_gravel.tea_daily";
        public const string TeaServed = "ui.hinokura_gravel.tea_served";
        public const string BagFull = "ui.hinokura_gravel.bag_full";

        public const string FurnaceRule = "ui.hinokura_furnaces.rule";
        public const string FurnaceGauge = "ui.hinokura_furnaces.gauge";
        public const string FurnacePumped = "ui.hinokura_furnaces.pumped";
        public const string FurnaceTooHot = "ui.hinokura_furnaces.too_hot";
        public const string FurnaceNeighbourCold = "ui.hinokura_furnaces.neighbour_cold";
        public const string FurnaceBalanced = "ui.hinokura_furnaces.balanced";
        public const string FurnaceReward = "ui.hinokura_furnaces.reward";
        public const string FurnaceRewardFirst = "ui.hinokura_furnaces.reward_first";
        public const string FurnaceSettled = "ui.hinokura_furnaces.settled";

        public const string GoatHooves = "ui.hinokura_goat.hooves";

        public static readonly string[] PatternNames = { "ui.hinokura_gravel.waves", "ui.hinokura_gravel.rings", "ui.hinokura_gravel.lines", "ui.hinokura_gravel.spiral" };
        public static readonly string[] NeighbourNames = { "ui.hinokura_gravel.by_water", "ui.hinokura_gravel.by_stone", "ui.hinokura_gravel.by_pine" };

        private const string CalmMaithe = "ui.hinokura_gravel.calm_maithe";
        private const string CalmIcaro = "ui.hinokura_gravel.calm_icaro";
        private const string CalmJoaquim = "ui.hinokura_gravel.calm_joaquim";
        private const string CalmMaria = "ui.hinokura_gravel.calm_maria";
        private const string CalmLavine = "ui.hinokura_gravel.calm_lavine";
        private const string FurnaceMaithe = "ui.hinokura_furnaces.line_maithe";
        private const string FurnaceIcaro = "ui.hinokura_furnaces.line_icaro";
        private const string FurnaceJoaquim = "ui.hinokura_furnaces.line_joaquim";
        private const string FurnaceMaria = "ui.hinokura_furnaces.line_maria";
        private const string FurnaceLavine = "ui.hinokura_furnaces.line_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(GravelRule, "Mestra Akane: \"Beside water, waves. Round a stone, rings. Under a pine, straight lines like its needles. The spiral is the wind's, and no garden here is the wind's.\"",
                "Mestra Akane: \"Junto da água, ondas. Em volta da pedra, anéis. Sob o pinheiro, linhas retas como as agulhas. A espiral é do vento, e nenhum jardim aqui é do vento.\""),
            new LocalizationEntry(GravelPrompt, "[E] Rake the gravel ({0}) — now: {1}", "[E] Rastelar o cascalho ({0}) — agora: {1}"),
            new LocalizationEntry(GravelRaked, "You rake {0}. Gardens in order: {1} of 3", "Você rastela {0}. Jardins certos: {1} de 3"),
            new LocalizationEntry(GravelCalm, "The three gardens are calm. Somewhere on the moss terrace a door slides open", "Os três jardins se acalmam. No terraço de musgo, uma porta corre e se abre"),
            new LocalizationEntry(GravelAlreadyCalm, "The gravel is calm today: leave it be", "O cascalho está calmo hoje: deixe como está"),
            new LocalizationEntry(TeaPrompt, "[E] Enter the hidden tea house", "[E] Entrar na casa de chá secreta"),
            new LocalizationEntry(TeaFirst, "Mestra Akane pours the tea: \"Few rake the way the garden asks.\" +{0} gold, +{1} Spark Stone", "A Mestra Akane serve o chá: \"Poucos rastelam como o jardim pede.\" +{0} de ouro, +{1} Pedra-Faísca"),
            new LocalizationEntry(TeaDaily, "Tea in the hidden house: +{0} gold", "Chá na casa secreta: +{0} de ouro"),
            new LocalizationEntry(TeaServed, "The tea was already served today. The gardens will be messed again tomorrow", "O chá de hoje já foi servido. Amanhã os jardins amanhecem bagunçados de novo"),
            new LocalizationEntry(BagFull, "Your bag is full: make room and come back", "A mochila está cheia: abra espaço e volte"),
            new LocalizationEntry(PatternNames[0], "waves", "ondas"),
            new LocalizationEntry(PatternNames[1], "rings", "anéis"),
            new LocalizationEntry(PatternNames[2], "straight lines", "linhas retas"),
            new LocalizationEntry(PatternNames[3], "a spiral", "espiral"),
            new LocalizationEntry(NeighbourNames[0], "beside the pond", "junto do laguinho"),
            new LocalizationEntry(NeighbourNames[1], "by the stone ring", "junto do anel de pedra"),
            new LocalizationEntry(NeighbourNames[2], "under the cloud pines", "sob os pinheiros de nuvem"),
            new LocalizationEntry(CalmMaithe, "Maithe: It's so quiet now I can hear the lava breathe.", "Maithe: Ficou tão quieto que dá para ouvir a lava respirar."),
            new LocalizationEntry(CalmIcaro, "Ícaro: Three gardens, four patterns, one right answer each. I like a puzzle that plays fair.", "Ícaro: Três jardins, quatro desenhos, uma resposta certa cada. Gosto de enigma honesto."),
            new LocalizationEntry(CalmJoaquim, "Joaquim: Vó Edna raked the orchard paths like this. She said a tidy path is a calm head.", "Joaquim: A Vó Edna rastelava os caminhos do pomar assim. Dizia que caminho arrumado é cabeça calma."),
            new LocalizationEntry(CalmMaria, "Maria: Tanukis, if you mess it up again tomorrow, I'm raking YOU.", "Maria: Tanukis, se bagunçarem de novo amanhã, eu rastelo VOCÊS."),
            new LocalizationEntry(CalmLavine, "Lavine: Even the stones look like they're resting. Let's sit a moment.", "Lavine: Até as pedras parecem descansar. Vamos sentar um pouco."),

            new LocalizationEntry(FurnaceRule, "Kurogane: \"A bellows heats its own furnace twice and steals one from each neighbour. Bring all nine to 5 and the court's forge is yours today.\"",
                "Kurogane: \"O fole esquenta a própria fornalha duas vezes e rouba uma de cada vizinha. Ponha as nove em 5 e a forja do pátio é sua hoje.\""),
            new LocalizationEntry(FurnaceGauge, "[E] Bellows of furnace {0}: heat {1} of 9 (balanced: 5) · neighbours {2} and {3} · balanced {4} of 9", "[E] Fole da fornalha {0}: calor {1} de 9 (certo: 5) · vizinhas {2} e {3} · certas {4} de 9"),
            new LocalizationEntry(FurnacePumped, "The bellows roar: furnace {0} at {1} · balanced {2} of 9", "O fole ruge: fornalha {0} em {1} · certas {2} de 9"),
            new LocalizationEntry(FurnaceTooHot, "Furnace {0} is white-hot already: the bellows won't take it higher", "A fornalha {0} já está branca de quente: o fole não sobe mais"),
            new LocalizationEntry(FurnaceNeighbourCold, "A neighbour of furnace {0} is nearly out: pumping here would kill its fire", "Uma vizinha da fornalha {0} está quase apagada: soprar aqui apagaria o fogo dela"),
            new LocalizationEntry(FurnaceBalanced, "The nine furnaces burn as one: the court's forge is lit", "As nove fornalhas queimam juntas: a forja do pátio acendeu"),
            new LocalizationEntry(FurnaceReward, "The court's forge: +{0} gold, +{1} Spark Stone", "A forja do pátio: +{0} de ouro, +{1} Pedra-Faísca"),
            new LocalizationEntry(FurnaceRewardFirst, "Kurogane: \"First time anyone has balanced them since the Master went mad.\" +{0} gold, +{1} Spark Stone, +{2} Mixture Stone",
                "Kurogane: \"Primeira vez que alguém equilibra as nove desde que o Mestre enlouqueceu.\" +{0} de ouro, +{1} Pedra-Faísca, +{2} Pedra da Mistura"),
            new LocalizationEntry(FurnaceSettled, "The furnaces settle into a new fire: start again", "As fornalhas assentam num fogo novo: recomece"),
            new LocalizationEntry(FurnaceMaithe, "Maithe: It's like a seesaw with nine seats. Lift one, two go down.", "Maithe: É uma gangorra de nove lugares. Sobe uma, descem duas."),
            new LocalizationEntry(FurnaceIcaro, "Ícaro: The total never changes: forty-five. You only move the fire around. That's the trick.", "Ícaro: O total nunca muda: quarenta e cinco. A gente só muda o fogo de lugar. O truque é esse."),
            new LocalizationEntry(FurnaceJoaquim, "Joaquim: Vô Jorge's wood stove had one bellows and he still argued with it every morning.", "Joaquim: O fogão a lenha do Vô Jorge tinha um fole só e mesmo assim ele brigava com ele toda manhã."),
            new LocalizationEntry(FurnaceMaria, "Maria: Pump the cold ones first. No, wait — the ones that LOOK fine can be the trap!", "Maria: Sopra as frias primeiro. Não, espera — as que PARECEM certas podem ser a armadilha!"),
            new LocalizationEntry(FurnaceLavine, "Lavine: Nine fires that wanted to be one. The Master only kept them fighting.", "Lavine: Nove fogos que queriam ser um só. O Mestre só os deixava brigando."),

            new LocalizationEntry(GoatHooves, "Pumice Hooves: the goat trots over the lava and nothing burns", "Cascos de Pedra-Pomes: a cabra trota sobre a lava e nada queima"),
        };

        /// <summary>The cousin's line when the gardens calm (null when no cousin is known).</summary>
        public static string CalmLineFor(CharacterId? hero) => Pick(hero, CalmMaithe, CalmIcaro, CalmJoaquim, CalmMaria, CalmLavine);

        /// <summary>The cousin's line when the nine furnaces balance (null when no cousin is known).</summary>
        public static string FurnaceLineFor(CharacterId? hero) => Pick(hero, FurnaceMaithe, FurnaceIcaro, FurnaceJoaquim, FurnaceMaria, FurnaceLavine);

        private static string Pick(CharacterId? hero, string maithe, string icaro, string joaquim, string maria, string lavine)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return maithe;
                case CharacterId.Icaro: return icaro;
                case CharacterId.Joaquim: return joaquim;
                case CharacterId.Maria: return maria;
                case CharacterId.Lavine: return lavine;
                default: return null;
            }
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(GravelRule))
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
