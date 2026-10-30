using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>What a cousin is talking about at the arraiá (<see cref="FestaJuninaTexts.LineKey"/>).</summary>
    public enum FestaLineKind
    {
        Greet,
        GreetAgain,
        DanceGreat,
        DanceOk,
        DanceBad,
        Stall,
    }

    /// <summary>
    /// Localization keys (en + pt-BR) of the Festa Junina: the arraiá window (<c>ui.festa.*</c>) and the cousins' lines
    /// at the festa (<c>festa.line.&lt;cousin&gt;.&lt;kind&gt;</c>, "any" without a cousin). Same self-contained pattern as
    /// <c>FishingTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use, pt-BR fallback without one.
    /// </summary>
    public static class FestaJuninaTexts
    {
        public const string Button = "ui.festa.button";
        public const string Title = "ui.festa.title";
        public const string Subtitle = "ui.festa.subtitle";
        /// <summary>{0} = prendas.</summary>
        public const string Prendas = "ui.festa.prendas";
        public const string TabDance = "ui.festa.tab_dance";
        public const string TabStall = "ui.festa.tab_stall";
        public const string DanceIntro = "ui.festa.dance_intro";
        public const string DanceStart = "ui.festa.dance_start";
        public const string DanceAgain = "ui.festa.dance_again";
        public const string DanceRelaxed = "ui.festa.dance_relaxed";
        public const string DanceGetReady = "ui.festa.dance_get_ready";
        /// <summary>{0} = score, {1} = max, {2} = combo.</summary>
        public const string DanceScore = "ui.festa.dance_score";
        /// <summary>{0} = score, {1} = max, {2} = perfects, {3} = misses, {4} = prendas.</summary>
        public const string DanceResult = "ui.festa.dance_result";
        /// <summary>{0} = bonus.</summary>
        public const string DanceFirstToday = "ui.festa.dance_first_today";
        public const string DanceFullCombo = "ui.festa.dance_full_combo";
        public const string JudgePerfect = "ui.festa.judge_perfect";
        public const string JudgeGood = "ui.festa.judge_good";
        public const string JudgeMiss = "ui.festa.judge_miss";
        public const string MoveAnarrie = "ui.festa.move_anarrie";
        public const string MoveBalance = "ui.festa.move_balance";
        public const string MoveCobra = "ui.festa.move_cobra";
        public const string MoveCaminho = "ui.festa.move_caminho";
        /// <summary>{0} = price.</summary>
        public const string StallBuy = "ui.festa.stall_buy";
        public const string StallIntro = "ui.festa.stall_intro";
        /// <summary>{0} = item.</summary>
        public const string StallBought = "ui.festa.stall_bought";
        public const string StallOwned = "ui.festa.stall_owned";
        /// <summary>{0} = price.</summary>
        public const string StallNotEnough = "ui.festa.stall_not_enough";
        public const string StallClosed = "ui.festa.stall_closed";
        public const string TasksTitle = "ui.festa.tasks_title";
        /// <summary>{0} = dances, {1} = needed, {2} = full combo yes/no, {3} = prizes, {4} = needed.</summary>
        public const string TasksProgress = "ui.festa.tasks_progress";
        public const string Yes = "ui.festa.yes";
        public const string No = "ui.festa.no";
        /// <summary>{0} = item.</summary>
        public const string LegendaryGranted = "ui.festa.legendary_granted";
        public const string Close = "ui.festa.close";

        public static string MoveKey(QuadrilhaMove move)
        {
            switch (move)
            {
                case QuadrilhaMove.Anarrie: return MoveAnarrie;
                case QuadrilhaMove.Balance: return MoveBalance;
                case QuadrilhaMove.Cobra: return MoveCobra;
                default: return MoveCaminho;
            }
        }

        public static string JudgeKey(QuadrilhaJudgement judgement)
        {
            switch (judgement)
            {
                case QuadrilhaJudgement.Perfect: return JudgePerfect;
                case QuadrilhaJudgement.Good: return JudgeGood;
                case QuadrilhaJudgement.Miss: return JudgeMiss;
                default: return null;
            }
        }

        /// <summary>"festa.line.maria.dance_great"; "any" when no cousin is selected.</summary>
        public static string LineKey(CharacterId? hero, FestaLineKind kind) => "festa.line." + HeroSegment(hero) + "." + KindSegment(kind);

        private static string HeroSegment(CharacterId? hero) => hero.HasValue ? hero.Value.ToString().ToLowerInvariant() : "any";

        private static string KindSegment(FestaLineKind kind)
        {
            switch (kind)
            {
                case FestaLineKind.Greet: return "greet";
                case FestaLineKind.GreetAgain: return "greet_again";
                case FestaLineKind.DanceGreat: return "dance_great";
                case FestaLineKind.DanceOk: return "dance_ok";
                case FestaLineKind.DanceBad: return "dance_bad";
                default: return "stall";
            }
        }

        /// <summary>The dance line for a result: great (full combo or 80%+), ok (50%+), bad.</summary>
        public static FestaLineKind DanceKind(int score, int maxScore, bool fullCombo)
        {
            if (fullCombo || (maxScore > 0 && score * 5 >= maxScore * 4))
            {
                return FestaLineKind.DanceGreat;
            }

            return maxScore > 0 && score * 2 >= maxScore ? FestaLineKind.DanceOk : FestaLineKind.DanceBad;
        }

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Window --
            new LocalizationEntry(Button, "Arraiá", "Arraiá"),
            new LocalizationEntry(Title, "Grandma Edna's Arraiá", "Arraiá da Vó Edna"),
            new LocalizationEntry(Subtitle, "It's June in the village: bunting, bonfire, quadrilha and the prize stall.", "É junho na vila: bandeirinhas, fogueira, quadrilha e barraca de prendas."),
            new LocalizationEntry(Prendas, "Prendas: {0}", "Prendas: {0}"),
            new LocalizationEntry(TabDance, "Quadrilha", "Quadrilha"),
            new LocalizationEntry(TabStall, "Prize stall", "Barraca de Prendas"),
            new LocalizationEntry(DanceIntro, "Grandma calls the steps. Answer each call on the beat: ← Anarriê, ↑ Balancê, ↓ Watch the snake, → Path to the farm.", "A Vó puxa a quadrilha. Responda cada chamada no compasso: ← Anarriê, ↑ Balancê, ↓ Olha a cobra, → Caminho da roça."),
            new LocalizationEntry(DanceStart, "Start the quadrilha", "Começar a quadrilha"),
            new LocalizationEntry(DanceAgain, "Dance again", "Dançar de novo"),
            new LocalizationEntry(DanceRelaxed, "Easy dance (slower, wider timing)", "Dança tranquila (mais lenta, tempo mais folgado)"),
            new LocalizationEntry(DanceGetReady, "Get in line... the accordion is starting!", "Em fila... a sanfona vai começar!"),
            new LocalizationEntry(DanceScore, "Points {0}/{1} · Combo {2}", "Pontos {0}/{1} · Sequência {2}"),
            new LocalizationEntry(DanceResult, "{0}/{1} points · {2} perfect · {3} missed · +{4} prendas", "{0}/{1} pontos · {2} perfeitos · {3} erros · +{4} prendas"),
            new LocalizationEntry(DanceFirstToday, "First dance of the day: +{0} prendas included", "Primeira dança do dia: +{0} prendas incluídas"),
            new LocalizationEntry(DanceFullCombo, "Not a single misstep!", "Nenhum passo errado!"),
            new LocalizationEntry(JudgePerfect, "Perfect!", "Perfeito!"),
            new LocalizationEntry(JudgeGood, "Good!", "Bom!"),
            new LocalizationEntry(JudgeMiss, "Missed!", "Errou!"),
            new LocalizationEntry(MoveAnarrie, "← Anarriê!", "← Anarriê!"),
            new LocalizationEntry(MoveBalance, "↑ Balancê!", "↑ Balancê!"),
            new LocalizationEntry(MoveCobra, "↓ Watch the snake!", "↓ Olha a cobra!"),
            new LocalizationEntry(MoveCaminho, "→ Path to the farm!", "→ Caminho da roça!"),
            new LocalizationEntry(StallIntro, "Trade prendas for festa decorations. They go to your house's Storage.", "Troque prendas por enfeites da festa. Eles vão para o Depósito da casa."),
            new LocalizationEntry(StallBuy, "Trade ({0} prendas)", "Trocar ({0} prendas)"),
            new LocalizationEntry(StallBought, "{0} went to the house's Storage!", "{0} foi para o Depósito da casa!"),
            new LocalizationEntry(StallOwned, "already have it", "já tem"),
            new LocalizationEntry(StallNotEnough, "Not enough prendas ({0})", "Faltam prendas ({0})"),
            new LocalizationEntry(StallClosed, "The stall only opens during the festa", "A barraca só abre durante a festa"),
            new LocalizationEntry(TasksTitle, "Arraiá tasks", "Tarefas do arraiá"),
            new LocalizationEntry(TasksProgress, "Dances {0}/{1} · A dance with no misstep: {2} · Different prizes {3}/{4}", "Danças {0}/{1} · Uma dança sem errar: {2} · Prendas diferentes {3}/{4}"),
            new LocalizationEntry(Yes, "yes", "sim"),
            new LocalizationEntry(No, "not yet", "ainda não"),
            new LocalizationEntry(LegendaryGranted, "Every arraiá task done! Grandma gave you the {0}. It's in the house's Storage.", "Todas as tarefas do arraiá! A Vó deu a {0}. Está no Depósito da casa."),
            new LocalizationEntry(Close, "Close", "Fechar"),

            // -- The cousins at the festa (birth order Maithe > Maria > Joaquim > Ícaro > Lavine) --
            Line(null, FestaLineKind.Greet, "Smell that? Corn cake, bonfire and Grandma's pamonha.", "Sente o cheiro? Bolo de milho, fogueira e a pamonha da Vó."),
            Line(null, FestaLineKind.GreetAgain, "The accordion hasn't stopped since sunset.", "A sanfona não parou desde o pôr do sol."),
            Line(null, FestaLineKind.DanceGreat, "What a quadrilha! Grandma clapped the whole time.", "Que quadrilha! A Vó bateu palma o tempo todo."),
            Line(null, FestaLineKind.DanceOk, "Not bad! One more round and it's perfect.", "Nada mal! Mais uma rodada e fica perfeito."),
            Line(null, FestaLineKind.DanceBad, "Tripped over the snake... happens to everyone.", "Tropeçou na cobra... acontece com todo mundo."),
            Line(null, FestaLineKind.Stall, "A prize for the house! Grandma's going to want to see it.", "Uma prenda pra casa! A Vó vai querer ver."),

            Line(CharacterId.Maithe, FestaLineKind.Greet, "Every June Grandpa Jorge lit the bonfire and Grandma called the quadrilha. Some things never change.", "Todo junho o Vô Jorge acendia a fogueira e a Vó puxava a quadrilha. Tem coisa que não muda."),
            Line(CharacterId.Maithe, FestaLineKind.GreetAgain, "I'm the eldest: I keep an eye on the bonfire. Ícaro, away from the fireworks.", "Sou a mais velha: eu fico de olho na fogueira. Ícaro, longe dos fogos."),
            Line(CharacterId.Maithe, FestaLineKind.DanceGreat, "Steady feet, eyes on the call. Dancing is like aiming.", "Pé firme, olho na chamada. Dançar é igual mirar."),
            Line(CharacterId.Maithe, FestaLineKind.DanceOk, "Good rhythm. Next time, wait for the accordion before stepping.", "Ritmo bom. Na próxima, espera a sanfona antes do passo."),
            Line(CharacterId.Maithe, FestaLineKind.DanceBad, "I stepped on Joaquim's foot. Twice. Don't write that down.", "Pisei no pé do Joaquim. Duas vezes. Não anota isso."),
            Line(CharacterId.Maithe, FestaLineKind.Stall, "This one goes on the porch, where Grandma can see it from the hammock.", "Essa vai pra varanda, onde a Vó vê da rede."),

            Line(CharacterId.Maria, FestaLineKind.Greet, "I helped Grandma cut the bunting! The yellow ones are mine.", "Ajudei a Vó a cortar as bandeirinhas! As amarelas são minhas."),
            Line(CharacterId.Maria, FestaLineKind.GreetAgain, "Joaquim, leave some canjica for the others!", "Joaquim, deixa canjica pros outros!"),
            Line(CharacterId.Maria, FestaLineKind.DanceGreat, "We danced like the grown-ups! Grandma, did you see?", "A gente dançou igual gente grande! Vó, viu?"),
            Line(CharacterId.Maria, FestaLineKind.DanceOk, "So much fun! Again, again!", "Que divertido! De novo, de novo!"),
            Line(CharacterId.Maria, FestaLineKind.DanceBad, "I laughed so hard I forgot the steps. Worth it!", "Ri tanto que esqueci os passos. Valeu a pena!"),
            Line(CharacterId.Maria, FestaLineKind.Stall, "It's so pretty! Let's put it where everyone who visits can see.", "Que lindo! Vamos pôr onde todo mundo que visitar veja."),

            Line(CharacterId.Joaquim, FestaLineKind.Greet, "Pamonha, canjica, pé-de-moleque... I'm here to defend the food table.", "Pamonha, canjica, pé-de-moleque... vim defender a mesa de comidas."),
            Line(CharacterId.Joaquim, FestaLineKind.GreetAgain, "Bet I can jump the snake higher than anyone.", "Aposto que pulo a cobra mais alto que todo mundo."),
            Line(CharacterId.Joaquim, FestaLineKind.DanceGreat, "Champion of the quadrilha! Maria, you're my witness!", "Campeão da quadrilha! Maria, tu é testemunha!"),
            Line(CharacterId.Joaquim, FestaLineKind.DanceOk, "Not bad for someone who dances holding a shield.", "Nada mal pra quem dança segurando escudo."),
            Line(CharacterId.Joaquim, FestaLineKind.DanceBad, "The snake was real. I swear I saw it move.", "A cobra era de verdade. Juro que vi mexer."),
            Line(CharacterId.Joaquim, FestaLineKind.Stall, "Does it come with pamonha? No? Fine, I'll take it anyway.", "Vem com pamonha? Não? Tá, levo assim mesmo."),

            Line(CharacterId.Icaro, FestaLineKind.Greet, "I calculated the bonfire's height. It's safe. Mostly.", "Calculei a altura da fogueira. É segura. Quase toda."),
            Line(CharacterId.Icaro, FestaLineKind.GreetAgain, "If I tie the bunting to a pulley, it goes up by itself. Maithe said no.", "Se eu amarrar as bandeirinhas numa roldana, elas sobem sozinhas. A Maithe disse não."),
            Line(CharacterId.Icaro, FestaLineKind.DanceGreat, "Ninety beats a minute, zero errors. The quadrilha is pure maths!", "Noventa batidas por minuto, zero erro. Quadrilha é matemática pura!"),
            Line(CharacterId.Icaro, FestaLineKind.DanceOk, "The data says I need to practise the balancê.", "Os dados dizem que preciso treinar o balancê."),
            Line(CharacterId.Icaro, FestaLineKind.DanceBad, "My legs didn't follow the algorithm.", "Minhas pernas não seguiram o algoritmo."),
            Line(CharacterId.Icaro, FestaLineKind.Stall, "I'll improve it. Just a few gears. Grandma won't notice.", "Vou melhorar ele. Só umas engrenagens. A Vó nem vai notar."),

            Line(CharacterId.Lavine, FestaLineKind.Greet, "Youngest leads the line this year. Grandma said so. Probably.", "A caçula puxa a fila esse ano. A Vó disse. Acho."),
            Line(CharacterId.Lavine, FestaLineKind.GreetAgain, "I don't need a partner. I dance by myself just fine.", "Não preciso de par. Danço sozinha muito bem."),
            Line(CharacterId.Lavine, FestaLineKind.DanceGreat, "See? Nobody had to help. Go tell Maithe.", "Viu? Ninguém precisou ajudar. Conta pra Maithe."),
            Line(CharacterId.Lavine, FestaLineKind.DanceOk, "...It was fine. I was just warming up.", "...Foi bom. Eu tava só esquentando."),
            Line(CharacterId.Lavine, FestaLineKind.DanceBad, "The accordion was too fast. Not my fault.", "A sanfona tava rápida demais. Não foi culpa minha."),
            Line(CharacterId.Lavine, FestaLineKind.Stall, "That one's mine. I traded the prendas, I choose where it goes.", "Essa é minha. Eu troquei as prendas, eu escolho onde vai."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>The cousin's line, falling back to the "any" line.</summary>
        public static string CousinLine(LocalizationManager manager, CharacterId? hero, FestaLineKind kind)
        {
            string key = LineKey(hero, kind);
            if (GetFallback(key) == null)
            {
                key = LineKey(null, kind);
            }

            return Get(manager, key);
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        private static LocalizationEntry Line(CharacterId? hero, FestaLineKind kind, string en, string ptBR) => new LocalizationEntry(LineKey(hero, kind), en, ptBR);
    }
}
