using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>What a cousin is talking about at the Bema (<see cref="FestaJuninaTexts.LineKey"/>).</summary>
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
    /// Localization keys (en + pt-BR) of the Bema (the class keeps its old Festa Junina name): the Bema window (<c>ui.festa.*</c>) and the cousins' lines
    /// at the Bema (<c>festa.line.&lt;cousin&gt;.&lt;kind&gt;</c>, "any" without a cousin). Same self-contained pattern as
    /// <c>FishingTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use, pt-BR fallback without one.
    /// </summary>
    public static class FestaJuninaTexts
    {
        public const string Button = "ui.festa.button";
        public const string Title = "ui.festa.title";
        public const string Subtitle = "ui.festa.subtitle";
        /// <summary>{0} = offerings (the prendas field).</summary>
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
        /// <summary>{0} = score, {1} = max, {2} = perfects, {3} = misses, {4} = offerings.</summary>
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
            // -- Window (the Bema: the festival of the empty seat, Docs/WorldBible_Maniqueismo.md §3.15) --
            new LocalizationEntry(Button, "Bema", "Bema"),
            new LocalizationEntry(Title, "Grandma Edna's Bema", "O Bema da Vó Edna"),
            new LocalizationEntry(Subtitle, "It's June in the village: the Bema, the festival of the empty seat. Lanterns, hymns and the offerings tent.", "É junho na vila: o Bema, a festa da cadeira vazia. Lanternas, hinos e a Tenda das Oferendas."),
            new LocalizationEntry(Prendas, "Offerings: {0}", "Oferendas: {0}"),
            new LocalizationEntry(TabDance, "Bema Hymns", "Hinos do Bema"),
            new LocalizationEntry(TabStall, "Offerings tent", "Tenda das Oferendas"),
            new LocalizationEntry(DanceIntro, "Master Xroshtag calls, you answer on the beat: ← Light, ↑ Wind, ↓ Water, → Fire. Every perfect answer shines lilac, like the Pure Air.", "O Mestre Xroshtag chama, você responde no compasso: ← Luz, ↑ Vento, ↓ Água, → Fogo. Cada resposta perfeita brilha lilás, como o Ar Sereno."),
            new LocalizationEntry(DanceStart, "Start the hymn", "Começar o hino"),
            new LocalizationEntry(DanceAgain, "Sing again", "Cantar de novo"),
            new LocalizationEntry(DanceRelaxed, "Easy hymn (slower, wider timing)", "Hino tranquilo (mais lento, tempo mais folgado)"),
            new LocalizationEntry(DanceGetReady, "Gather round the Bema... the talking drum is starting!", "Todo mundo em volta do Bema... o tambor falante vai começar!"),
            new LocalizationEntry(DanceScore, "Points {0}/{1} · Combo {2}", "Pontos {0}/{1} · Sequência {2}"),
            new LocalizationEntry(DanceResult, "{0}/{1} points · {2} perfect · {3} missed · +{4} offerings", "{0}/{1} pontos · {2} perfeitos · {3} erros · +{4} oferendas"),
            new LocalizationEntry(DanceFirstToday, "First hymn of the day: +{0} offerings included", "Primeiro hino do dia: +{0} oferendas incluídas"),
            new LocalizationEntry(DanceFullCombo, "Not a single call left unanswered!", "Nenhum chamado ficou sem resposta!"),
            new LocalizationEntry(JudgePerfect, "Perfect!", "Perfeito!"),
            new LocalizationEntry(JudgeGood, "Good!", "Bom!"),
            new LocalizationEntry(JudgeMiss, "Missed!", "Errou!"),
            new LocalizationEntry(MoveAnarrie, "← Light!", "← Luz!"),
            new LocalizationEntry(MoveBalance, "↑ Wind!", "↑ Vento!"),
            new LocalizationEntry(MoveCobra, "↓ Water!", "↓ Água!"),
            new LocalizationEntry(MoveCaminho, "→ Fire!", "→ Fogo!"),
            new LocalizationEntry(StallIntro, "Trade offerings for Bema decorations. They go to your house's Storage.", "Troque oferendas por enfeites do Bema. Eles vão para o Depósito da casa."),
            new LocalizationEntry(StallBuy, "Trade ({0} offerings)", "Trocar ({0} oferendas)"),
            new LocalizationEntry(StallBought, "{0} went to the house's Storage!", "{0} foi para o Depósito da casa!"),
            new LocalizationEntry(StallOwned, "already have it", "já tem"),
            new LocalizationEntry(StallNotEnough, "Not enough offerings ({0})", "Faltam oferendas ({0})"),
            new LocalizationEntry(StallClosed, "The tent only opens during the Bema", "A tenda só abre durante o Bema"),
            new LocalizationEntry(TasksTitle, "Bema tasks", "Tarefas do Bema"),
            new LocalizationEntry(TasksProgress, "Hymns {0}/{1} · A hymn with no miss: {2} · Different decorations {3}/{4}", "Hinos {0}/{1} · Um hino sem errar: {2} · Enfeites diferentes {3}/{4}"),
            new LocalizationEntry(Yes, "yes", "sim"),
            new LocalizationEntry(No, "not yet", "ainda não"),
            new LocalizationEntry(LegendaryGranted, "Every Bema task done! Grandma handed you this: {0}. It's in the house's Storage.", "Todas as tarefas do Bema! A Vó te entregou: {0}. Está no Depósito da casa."),
            new LocalizationEntry(Close, "Close", "Fechar"),

            // -- The cousins at the Bema (birth order Maithe > Maria > Joaquim > Ícaro > Lavine) --
            Line(null, FestaLineKind.Greet, "Smell that? Samsa from Grandma's oven, apricots and lamp oil.", "Sente o cheiro? Samsa do forno da Vó, damasco e óleo de lamparina."),
            Line(null, FestaLineKind.GreetAgain, "The talking drum hasn't stopped since sunset.", "O tambor falante não parou desde o pôr do sol."),
            Line(null, FestaLineKind.DanceGreat, "What a hymn! Grandma clapped the whole time.", "Que hino! A Vó bateu palma o tempo todo."),
            Line(null, FestaLineKind.DanceOk, "Not bad! One more round and it's perfect.", "Nada mal! Mais uma rodada e fica perfeito."),
            Line(null, FestaLineKind.DanceBad, "Lost the call halfway... happens to everyone.", "Perdeu o chamado no meio... acontece com todo mundo."),
            Line(null, FestaLineKind.Stall, "A Bema decoration for the house! Grandma's going to want to see it.", "Um enfeite do Bema pra casa! A Vó vai querer ver."),

            Line(CharacterId.Maithe, FestaLineKind.Greet, "Every June Grandpa Jorge lit the first lamp of the Bema and Grandma started the first hymn. Some things never change.", "Todo junho o Vô Jorge acendia a primeira lâmpada do Bema e a Vó puxava o primeiro hino. Tem coisa que não muda."),
            Line(CharacterId.Maithe, FestaLineKind.GreetAgain, "I'm the eldest: I keep an eye on the lamps. Ícaro, away from the oil.", "Sou a mais velha: eu fico de olho nas lâmpadas. Ícaro, longe do óleo."),
            Line(CharacterId.Maithe, FestaLineKind.DanceGreat, "Steady voice, ears on the call. Singing is like aiming: trust the wind.", "Voz firme, ouvido no chamado. Cantar é igual mirar: confia no vento."),
            Line(CharacterId.Maithe, FestaLineKind.DanceOk, "Good rhythm. Next time, wait for the drum before answering.", "Ritmo bom. Na próxima, espera o tambor antes de responder."),
            Line(CharacterId.Maithe, FestaLineKind.DanceBad, "I answered Fire when he called Water. Twice. Don't write that down.", "Respondi Fogo quando ele chamou Água. Duas vezes. Não anota isso."),
            Line(CharacterId.Maithe, FestaLineKind.Stall, "This one goes on the porch, next to Grandpa's rocking chair.", "Essa vai pra varanda, do lado da cadeira de balanço do Vô."),

            Line(CharacterId.Maria, FestaLineKind.Greet, "They brought Grandpa's rocking chair to the square, right by the Bema. Nobody sits in it. We just sing close by.", "Trouxeram a cadeira de balanço do Vô pra praça, do lado do Bema. Ninguém senta. A gente só canta pertinho."),
            Line(CharacterId.Maria, FestaLineKind.GreetAgain, "Joaquim, leave some plov for the others!", "Joaquim, deixa plov pros outros!"),
            Line(CharacterId.Maria, FestaLineKind.DanceGreat, "We sang like the grown-ups! Grandma, did you see?", "A gente cantou igual gente grande! Vó, viu?"),
            Line(CharacterId.Maria, FestaLineKind.DanceOk, "So much fun! Again, again!", "Que divertido! De novo, de novo!"),
            Line(CharacterId.Maria, FestaLineKind.DanceBad, "I laughed so hard I forgot the answers. Worth it!", "Ri tanto que esqueci as respostas. Valeu a pena!"),
            Line(CharacterId.Maria, FestaLineKind.Stall, "It's so pretty! Let's put it where everyone who visits can see.", "Que lindo! Vamos pôr onde todo mundo que visitar veja."),

            Line(CharacterId.Joaquim, FestaLineKind.Greet, "Plov, samsa, honey melon... I'm here to defend the food table.", "Plov, samsa, melão com mel... vim defender a mesa de comidas."),
            Line(CharacterId.Joaquim, FestaLineKind.GreetAgain, "Bet I can shout \"Water!\" louder than Master Xroshtag.", "Aposto que grito \"Água!\" mais alto que o Mestre Xroshtag."),
            Line(CharacterId.Joaquim, FestaLineKind.DanceGreat, "Champion of the hymns! Maria, you're my witness!", "Campeão dos hinos! Maria, você é testemunha!"),
            Line(CharacterId.Joaquim, FestaLineKind.DanceOk, "Not bad for someone who sings holding a shield.", "Nada mal pra quem canta segurando escudo."),
            Line(CharacterId.Joaquim, FestaLineKind.DanceBad, "The drum sped up. I swear it did.", "O tambor acelerou. Juro que acelerou."),
            Line(CharacterId.Joaquim, FestaLineKind.Stall, "Does it come with plov? No? Fine, I'll take it anyway.", "Vem com plov? Não? Tá, levo assim mesmo."),

            Line(CharacterId.Icaro, FestaLineKind.Greet, "I calculated the weight of the lanterns on the strings. They hold. Mostly.", "Calculei o peso das lanternas nos cordões. Aguenta. Quase tudo."),
            Line(CharacterId.Icaro, FestaLineKind.GreetAgain, "If I tie the lanterns to a pulley, they light themselves. Maithe said no.", "Se eu amarrar as lanternas numa roldana, elas acendem sozinhas. A Maithe disse não."),
            Line(CharacterId.Icaro, FestaLineKind.DanceGreat, "A call every 0.9 seconds, zero errors. Call and answer is pure maths! See? Science.", "Um chamado a cada 0,9 segundo, zero erro. Chamado e Resposta é matemática pura! Viu? Ciência."),
            Line(CharacterId.Icaro, FestaLineKind.DanceOk, "The data says I need to practise Wind.", "Os dados dizem que preciso treinar o Vento."),
            Line(CharacterId.Icaro, FestaLineKind.DanceBad, "My voice didn't follow the algorithm.", "Minha voz não seguiu o algoritmo."),
            Line(CharacterId.Icaro, FestaLineKind.Stall, "I'll improve it. Just a few gears. Grandma won't notice.", "Vou melhorar ele. Só umas engrenagens. A Vó nem vai notar."),

            Line(CharacterId.Lavine, FestaLineKind.Greet, "Youngest carries the first lamp this year. Grandma said so. Probably.", "A caçula leva a primeira lâmpada esse ano. A Vó disse. Acho."),
            Line(CharacterId.Lavine, FestaLineKind.GreetAgain, "I don't need anyone answering with me. I answer by myself just fine.", "Não preciso de ninguém respondendo junto. Respondo sozinha muito bem."),
            Line(CharacterId.Lavine, FestaLineKind.DanceGreat, "See? Nobody had to help. Go tell Maithe.", "Viu? Ninguém precisou ajudar. Conta pra Maithe."),
            Line(CharacterId.Lavine, FestaLineKind.DanceOk, "...It was fine. I was just warming up.", "...Foi bom. Eu tava só esquentando."),
            Line(CharacterId.Lavine, FestaLineKind.DanceBad, "The drum was too fast. Not my fault.", "O tambor tava rápido demais. Não foi culpa minha."),
            Line(CharacterId.Lavine, FestaLineKind.Stall, "That one's mine. I traded the offerings, I choose where it goes.", "Esse é meu. Eu troquei as oferendas, eu escolho onde vai."),
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
