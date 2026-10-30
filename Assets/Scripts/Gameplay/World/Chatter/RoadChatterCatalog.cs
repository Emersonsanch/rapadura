using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Chatter
{
    /// <summary>
    /// How far the campaign has gone, as the road conversations see it. Driven by the story beats already
    /// seen (<see cref="RoadChatterRules.StageFrom"/>), so a cousin never mentions a boss or a reveal before its
    /// interlude played — and a "Nova Jornada+" (which clears the seen beats) starts again at <see cref="Forest"/>.
    /// </summary>
    public enum RoadChatterStage
    {
        /// <summary>Prologue seen, Lobo Alfa not beaten yet.</summary>
        Forest = 0,
        /// <summary>"A Floresta respira de novo" seen: on the way through the Desert.</summary>
        Desert = 1,
        /// <summary>"Segredos na areia" seen: on the way up the Mountain.</summary>
        Mountain = 2,
        /// <summary>"No alto da Montanha" seen: going down to the Caves.</summary>
        Caves = 3,
        /// <summary>The epilogue seen: post-game, back home.</summary>
        Home = 4,
    }

    /// <summary>
    /// <see cref="Radio"/>: the other cousin answers on one of the battery radios Ícaro built from Vó Zefa's old
    /// one (prologue). <see cref="Memory"/>: the active cousin remembers something the other one said.
    /// </summary>
    public enum RoadChatterKind
    {
        Radio = 0,
        Memory = 1,
    }

    /// <summary>A pair of texts (en + pt-BR) for one line of an exchange.</summary>
    public readonly struct RoadChatterLine
    {
        public readonly string En;
        public readonly string Pt;

        public RoadChatterLine(string en, string pt)
        {
            En = en;
            Pt = pt;
        }
    }

    /// <summary>
    /// One "Conversa entre primos na estrada": two lines, <see cref="First"/> then <see cref="Second"/>. A radio
    /// exchange plays when either cousin is the active one (the other is "no rádio"); a memory exchange only when
    /// <see cref="First"/> is (they remember <see cref="Second"/>'s words). Gated by <see cref="MinStage"/>..
    /// <see cref="MaxStage"/> and <see cref="MinCycle"/> (1 = Nova Jornada+ only).
    /// </summary>
    public sealed class RoadChatterSpec
    {
        public readonly string Id;
        public readonly CharacterId First;
        public readonly CharacterId Second;
        public readonly RoadChatterKind Kind;
        public readonly RoadChatterStage MinStage;
        public readonly RoadChatterStage MaxStage;
        public readonly int MinCycle;
        public readonly RoadChatterLine FirstLine;
        public readonly RoadChatterLine SecondLine;

        public RoadChatterSpec(string id, CharacterId first, CharacterId second, RoadChatterKind kind,
            RoadChatterStage minStage, RoadChatterStage maxStage, int minCycle, RoadChatterLine firstLine, RoadChatterLine secondLine)
        {
            Id = id;
            First = first;
            Second = second;
            Kind = kind;
            MinStage = minStage;
            MaxStage = maxStage;
            MinCycle = minCycle;
            FirstLine = firstLine;
            SecondLine = secondLine;
        }

        /// <summary><c>road_chatter.&lt;id&gt;.line_1</c> / <c>.line_2</c>.</summary>
        public string LineKey(int index) => RoadChatterCatalog.KeyPrefix + Id + ".line_" + (index <= 0 ? 1 : 2);

        public CharacterId Speaker(int index) => index <= 0 ? First : Second;

        public bool Involves(CharacterId hero) => First == hero || Second == hero;

        /// <summary>Whether <paramref name="active"/> can be the cousin on the road for this exchange.</summary>
        public bool PlayableBy(CharacterId active) => Kind == RoadChatterKind.Memory ? First == active : Involves(active);
    }

    /// <summary>
    /// "Conversas entre primos na estrada": every exchange, as data (texts inline, merged by
    /// <see cref="RoadChatterTexts"/>). Canon (Docs/Narrative.md): Maithe and Ícaro are siblings, Maria and
    /// Joaquim are siblings, Lavine is cousin to all; Maithe (14) &gt; Maria (13) &gt; Joaquim (12) &gt; Ícaro (11) &gt;
    /// Lavine (9). Siblings tease ("pestinha", "mana", "maninho" — only ever said down to a younger sibling);
    /// cousins say "prima/primo". More exchanges for the two sibling pairs.
    /// </summary>
    public static class RoadChatterCatalog
    {
        public const string KeyPrefix = "road_chatter.";

        private const CharacterId Maithe = CharacterId.Maithe;
        private const CharacterId Maria = CharacterId.Maria;
        private const CharacterId Joaquim = CharacterId.Joaquim;
        private const CharacterId Icaro = CharacterId.Icaro;
        private const CharacterId Lavine = CharacterId.Lavine;

        private const RoadChatterKind Radio = RoadChatterKind.Radio;
        private const RoadChatterKind Memory = RoadChatterKind.Memory;

        private const RoadChatterStage Forest = RoadChatterStage.Forest;
        private const RoadChatterStage Desert = RoadChatterStage.Desert;
        private const RoadChatterStage Mountain = RoadChatterStage.Mountain;
        private const RoadChatterStage Caves = RoadChatterStage.Caves;
        private const RoadChatterStage Home = RoadChatterStage.Home;

        public static readonly IReadOnlyList<RoadChatterSpec> All = new List<RoadChatterSpec>
        {
            // ================= Maithe & Ícaro (siblings) =================
            C("maithe_icaro_radio_test", Icaro, Maithe, Radio, Forest, Forest, 0,
                "Testing, testing... Maithe, can you hear me? Over!", "Testando, testando... Maithe, tá me ouvindo? Câmbio!",
                "Loud and clear. Too loud. Turn it down, pest. Over.", "Alto e claro. Alto demais. Abaixa esse volume, pestinha. Câmbio."),
            C("maithe_icaro_light_meter", Maithe, Icaro, Radio, Forest, Desert, 0,
                "Is your light meter still pointing at the Caves?", "Teu medidor de luz ainda aponta pras Cavernas?",
                "Always. Either science is right, or I taped the needle on backwards.", "Sempre. Ou a ciência tá certa, ou eu colei a agulha do lado errado."),
            C("maithe_icaro_caja_machine", Maithe, Icaro, Memory, Forest, Home, 0,
                "That clatter of gears reminds me of Ícaro's cajá-peeling machine...", "Esse barulho de engrenagem me lembra a máquina de descascar cajá do Ícaro...",
                "\"Maithe, step back! ...Okay, it only exploded a little.\"", "\"Maithe, sai de perto! ...Tá, ela só explodiu um pouquinho.\""),
            C("maithe_icaro_impressed", Icaro, Maithe, Radio, Desert, Caves, 0,
                "My drone got three scorpions in a row. Go on, say you're impressed.", "Meu drone acertou três escorpiões seguidos. Pode falar que impressionou.",
                "Impressed. Now stop narrating and watch your back.", "Impressionada. Agora para de narrar e olha pra trás."),
            C("maithe_icaro_panel", Icaro, Maithe, Radio, Mountain, Mountain, 0,
                "Sis, I've almost cracked the Guardian's panel. The ancients had awful handwriting!", "Mana, eu tô quase decifrando o painel do Guardião. Os antigos escreviam torto!",
                "Decode it while you walk. A cracking seal won't wait for your notebook.", "Decifra andando. Selo rachando não espera ninguém terminar o caderno."),
            C("maithe_icaro_mountain_wind", Maithe, Icaro, Radio, Mountain, Caves, 0,
                "The mountain wind keeps lying to the Eye of the Wind. Don't trust anything that flies.", "O vento da serra vive mentindo pro Olho do Vento. Não confia em nada que voa.",
                "Not even my drone? ...I withdraw the question.", "Nem no meu drone? ...Retiro a pergunta."),
            C("maithe_icaro_last_page", Icaro, Maithe, Radio, Home, Home, 0,
                "Maithe, did you read the last page of my notebook?", "Maithe, você leu a última página do meu caderno?",
                "What page? ...I think so too, pest. Best invention ever.", "Que página? ...Eu também acho, pestinha. Melhor invenção de todas."),
            C("maithe_icaro_experiment_two", Maithe, Icaro, Radio, Forest, Desert, 1,
                "Experiment number two, huh? So what's the hypothesis?", "Experimento número dois, é? E qual é a hipótese?",
                "That this time you'll count the cousins four times. I'm already writing it down.", "Que desta vez você vai contar os primos quatro vezes. Já tô anotando."),

            // ================= Maria & Joaquim (siblings) =================
            C("maria_joaquim_out_front", Maria, Joaquim, Radio, Forest, Desert, 0,
                "Joaquim, you're out in front again, aren't you? I can feel it from here.", "Joaquim, você tá na frente de novo, né? Eu sinto daqui.",
                "Someone has to see the road first, sis!", "Alguém tem que ver o caminho primeiro, mana!"),
            C("maria_joaquim_dream", Joaquim, Maria, Radio, Forest, Forest, 0,
                "Sis, did your dream say whether we beat the Alpha Wolf?", "Mana, o teu sonho dizia se a gente ganha do Lobo Alfa?",
                "It said we'd go together. The rest is up to you not running off.", "Dizia que a gente ia junto. O resto depende de você não sair correndo."),
            C("maria_joaquim_spinning_top", Joaquim, Maria, Memory, Forest, Home, 0,
                "This packed-dirt road would be perfect for spinning a top...", "Esse chão de terra batida ia ser perfeito pra rodar pião...",
                "\"I won again, little brother. You can cry quietly.\"", "\"Ganhei de novo, maninho. Pode chorar baixinho.\""),
            C("maria_joaquim_wolf_light", Maria, Joaquim, Radio, Desert, Mountain, 0,
                "I keep thinking about the Alpha Wolf. He wasn't bad, Joaquim.", "Eu não paro de pensar no Lobo Alfa. Ele não era mau, Joaquim.",
                "I know. That's why I only hit him with the shield.", "Eu sei. Por isso eu só bati com o escudo."),
            C("maria_joaquim_baiao", Joaquim, Maria, Radio, Desert, Caves, 0,
                "Sis, when we get home, will Grandma make baião de dois?", "Mana, quando a gente voltar, a Vó faz baião de dois?",
                "She will. And you'll have seconds three times, like always.", "Faz. E você vai repetir três vezes, como sempre."),
            C("maria_joaquim_taller", Joaquim, Maria, Radio, Mountain, Home, 0,
                "Sis, have you noticed I'm taller than you now?", "Mana, já reparou que eu tô mais alto que você?",
                "I have. You're still my little brother.", "Reparei. Continua sendo meu irmão mais novo."),
            C("maria_joaquim_step_aside", Joaquim, Maria, Radio, Caves, Caves, 0,
                "Up on the mountain I learned that protecting also means stepping aside. That's so hard, sis.", "Lá na serra eu aprendi que proteger também é saber sair da frente. Que coisa difícil, mana.",
                "I know, little brother. That's why I pray for you every single day.", "Eu sei, maninho. Por isso eu rezo por você todo santo dia."),
            C("maria_joaquim_hammock", Maria, Joaquim, Radio, Home, Home, 0,
                "You admitted you were tired, Joaquim. I heard you.", "Você admitiu que tava cansado, Joaquim. Eu ouvi.",
                "That was the wind. Nobody can prove anything.", "Foi o vento. Ninguém prova nada."),
            C("maria_joaquim_shield_grew", Joaquim, Maria, Radio, Forest, Caves, 1,
                "Sis, the creatures are fiercer this time. Good thing my shield grew too.", "Mana, dessa vez os bichos tão mais bravos. Ainda bem que o meu escudo cresceu também.",
                "The shield didn't grow, little brother. You did.", "O escudo, não. Você é que cresceu, maninho."),

            // ================= Maithe & Maria (cousins, three months apart) =================
            C("maithe_maria_three_months", Maria, Maithe, Radio, Forest, Home, 0,
                "Maithe, you decide everything just because you're three months older.", "Maithe, você decide tudo só porque é três meses mais velha.",
                "Three months and eleven days. And yes.", "Três meses e onze dias. E sim."),
            C("maithe_maria_dream_and_eye", Maithe, Maria, Radio, Forest, Desert, 0,
                "Your dream and my Eye of the Wind point at the same place, Maria.", "O teu sonho e o meu Olho do Vento apontam pro mesmo lugar, Maria.",
                "Then for once we agree without arguing. Someone write that down!", "Então pela primeira vez a gente concorda sem brigar. Alguém anota isso!"),
            C("maithe_maria_light_first", Maria, Maithe, Radio, Caves, Caves, 0,
                "In the Caves, let me go first with the light. Just this once.", "Nas Cavernas, deixa eu ir na frente com a luz. Só dessa vez.",
                "Deal, cousin. But I'll be right behind you, arrow ready.", "Combinado, prima. Mas eu vou logo atrás, com a flecha pronta."),
            C("maithe_maria_sliver", Maithe, Maria, Radio, Forest, Mountain, 1,
                "Did your crystal sliver really go cold?", "A tua lasquinha de cristal esfriou mesmo?",
                "It did. My heart didn't. Let's light it up again.", "Esfriou. Meu coração, não. Bora acender de novo."),

            // ================= Maithe & Joaquim (cousins) =================
            C("maithe_joaquim_front", Joaquim, Maithe, Radio, Forest, Mountain, 0,
                "Maithe, let me go first this time!", "Maithe, deixa eu ir na frente dessa vez!",
                "Go on. I'll hit whatever bites you before it bites.", "Pode ir. Eu acerto o que for te morder antes de morder."),
            C("maithe_joaquim_stubborn", Maithe, Joaquim, Radio, Desert, Mountain, 0,
                "Old Bento says you're as stubborn as Grandpa Chico.", "O Seu Bento diz que você é teimoso igual ao Vô Chico.",
                "That's a compliment, right? I'll pretend it's a compliment.", "Isso é elogio, né? Vou fingir que é elogio."),
            C("maithe_joaquim_counting", Joaquim, Maithe, Radio, Caves, Home, 0,
                "Why do you keep counting us all the time?", "Por que você fica contando a gente toda hora?",
                "It's my way of saying everyone's okay.", "É o meu jeito de dizer que tá todo mundo bem."),
            C("maithe_joaquim_day_off", Maithe, Joaquim, Radio, Home, Home, 0,
                "You leaned the shield on the wall and never picked it up. Are you sick?", "Você encostou o escudo na parede e não pegou de volta. Tá doente?",
                "Day off. Even guardians take holidays.", "Tô de folga. Até guardião tira férias."),

            // ================= Maithe & Lavine (cousins: eldest and youngest) =================
            C("maithe_lavine_little", Maithe, Lavine, Radio, Forest, Desert, 0,
                "Lavine, stay close, little one.", "Lavine, fica por perto, pequena.",
                "I'm not little. I'm the youngest. That's different.", "Eu não sou pequena. Eu sou a mais nova. É diferente."),
            C("maithe_lavine_shadow", Lavine, Maithe, Radio, Desert, Caves, 0,
                "Maithe, can your Eye of the Wind see inside my shadow?", "Maithe, o teu Olho do Vento consegue ver a minha sombra por dentro?",
                "It can. There's only courage in there. And a bit of mess.", "Consegue. Lá dentro só tem coragem. E um pouco de bagunça."),
            C("maithe_lavine_shoulder", Lavine, Maithe, Radio, Caves, Caves, 0,
                "Sorry I fell asleep on your shoulder by the campfire.", "Desculpa ter dormido no teu ombro na fogueira.",
                "Nothing to be sorry for. That's what an eldest cousin's shoulder is for.", "Desculpa nada. Ombro de prima mais velha serve pra isso."),
            C("maithe_lavine_promise", Maithe, Lavine, Radio, Home, Home, 0,
                "Nobody's calling you little anymore. I promise.", "Ninguém mais vai te chamar de pequena. Prometo.",
                "You can, once in a while. Only you.", "Você pode, de vez em quando. Só você."),

            // ================= Maria & Ícaro (cousins) =================
            C("maria_icaro_crackle", Maria, Icaro, Radio, Forest, Home, 0,
                "Ícaro, the radio is crackling. Is it the batteries or is it you?", "Ícaro, o radinho tá chiando. É a pilha ou é você?",
                "The batteries. I crackle with more style.", "É a pilha. Eu chio com mais estilo."),
            C("maria_icaro_watts", Icaro, Maria, Radio, Desert, Caves, 0,
                "Maria, how many watts is your light?", "Maria, a tua luz tem quantos watts?",
                "Just enough to heal you when your drone lands on your head.", "O suficiente pra te curar quando o teu drone cai na tua cabeça."),
            C("maria_icaro_lanterns", Maria, Icaro, Radio, Caves, Caves, 0,
                "They'll need your pocket lanterns down in the Caves.", "Lá nas Cavernas vão precisar das tuas lanternas de bolso.",
                "I already left a few with Master Tonho. Scientific forecast, cousin.", "Já deixei umas com o Mestre Tonho. Previsão científica, prima."),

            // ================= Maria & Lavine (cousins) =================
            C("maria_lavine_cold", Lavine, Maria, Radio, Forest, Desert, 0,
                "Maria, the shadow is cold today.", "Maria, a sombra tá fria hoje.",
                "Then think of my light. It reaches you, even from here.", "Então pensa na minha luz. Ela chega até você, mesmo daqui."),
            C("maria_lavine_put_out", Lavine, Maria, Radio, Desert, Mountain, 0,
                "You said someone put out the Wolf's light. Could they put out mine?", "Você disse que alguém apagou a luz do Lobo. Podem apagar a minha também?",
                "Nobody puts out what belongs to the family. And you're family.", "Ninguém apaga o que é da família. E você é da família."),
            C("maria_lavine_soft", Maria, Lavine, Radio, Caves, Caves, 0,
                "Down there, if I light my light slowly, will you help me?", "Lá embaixo, se eu acender a minha luz devagarinho, você me ajuda?",
                "I will. I'll show it a shadow can be soft too.", "Ajudo. Eu mostro que a sombra também pode ser macia."),
            C("maria_lavine_listens", Lavine, Maria, Radio, Forest, Caves, 1,
                "The first time, I was afraid of my own shadow.", "Da primeira vez, eu tinha medo da minha própria sombra.",
                "And now it listens to you. Come on, cousin.", "E agora ela te obedece. Bora, prima."),

            // ================= Joaquim & Ícaro (cousins: the two boys) =================
            C("joaquim_icaro_turret", Joaquim, Icaro, Radio, Forest, Mountain, 0,
                "Ícaro, your turret shot my shield again!", "Ícaro, a tua torreta atirou no meu escudo de novo!",
                "It was testing its strength. Result: approved!", "Ela tava testando a resistência. Resultado: aprovado!"),
            C("joaquim_icaro_contract", Icaro, Joaquim, Radio, Desert, Caves, 0,
                "Joaquim, I don't need you protecting me all the time.", "Joaquim, eu não preciso que você me proteja o tempo todo.",
                "I know. But I'm the oldest of the boys. It's in the contract.", "Eu sei. Mas eu sou o mais velho dos meninos. Tá no contrato."),
            C("joaquim_icaro_roots", Joaquim, Icaro, Radio, Mountain, Caves, 0,
                "Stone golem against the Shield of Roots. Want to bet?", "Golem de pedra contra o Escudo das Raízes. Aposta?",
                "I'm betting on the roots. Statistically, roots crack stone.", "Aposto na raiz. Estatisticamente, raiz racha pedra."),

            // ================= Joaquim & Lavine (cousins) =================
            C("joaquim_lavine_behind", Joaquim, Lavine, Radio, Forest, Desert, 0,
                "Lavine, stay behind me and I'll protect you.", "Lavine, fica atrás de mim, que eu te protejo.",
                "You stay behind me. My mist will hide you.", "Fica você atrás de mim. A minha névoa te esconde."),
            C("joaquim_lavine_mist_trick", Joaquim, Lavine, Memory, Desert, Home, 0,
                "Mist like this reminds me of the time Lavine fooled me with her Mist of Illusion...", "Neblina assim me lembra a vez que a Lavine me enganou com a Névoa da Ilusão...",
                "\"Cousin, I'm over here! No, here! No — here!\"", "\"Primo, eu tô aqui! Não, aqui! Não — aqui!\""),
            C("joaquim_lavine_afraid", Lavine, Joaquim, Radio, Caves, Home, 0,
                "Joaquim, are you afraid of anything?", "Joaquim, você tem medo de alguma coisa?",
                "Of being far from you lot. ...And of geese. But only a little.", "De ficar longe de vocês. ...E de ganso. Mas só um pouquinho."),

            // ================= Ícaro & Lavine (cousins: the two youngest) =================
            C("icaro_lavine_measure", Icaro, Lavine, Radio, Forest, Mountain, 0,
                "Lavine, can I measure your shadow? For science.", "Lavine, posso medir a tua sombra? Pra ciência.",
                "You can. But if it bites you, blame science.", "Pode. Mas se ela te morder, a culpa é da ciência."),
            C("icaro_lavine_basic_maths", Lavine, Icaro, Radio, Desert, Caves, 0,
                "Ícaro, why are you the only cousin who doesn't treat me like a baby?", "Ícaro, por que você é o único primo que não me trata como bebê?",
                "Because you're two years younger, not ten. Basic maths.", "Porque você é dois anos mais nova, não dez. Matemática básica."),
            C("icaro_lavine_first_customer", Lavine, Icaro, Radio, Home, Home, 0,
                "The Dweller said he liked the lantern you sent him.", "O Habitante disse que gostou da lanterna que você mandou.",
                "My first customer from the bottom of the caves. I'm opening a branch.", "Primeiro freguês do fundo da caverna. Vou abrir filial."),
            C("icaro_lavine_partner", Icaro, Lavine, Radio, Forest, Home, 1,
                "Lavine, want to be my assistant for experiment number two?", "Lavine, quer ser minha assistente no experimento número dois?",
                "Not assistant. Partner.", "Assistente, não. Sócia."),
        };

        private static Dictionary<string, RoadChatterSpec> _byId;

        public static RoadChatterSpec Find(string id)
        {
            if (_byId == null)
            {
                _byId = All.ToDictionary(spec => spec.Id);
            }

            return id != null && _byId.TryGetValue(id, out RoadChatterSpec spec) ? spec : null;
        }

        private static RoadChatterSpec C(string id, CharacterId first, CharacterId second, RoadChatterKind kind,
            RoadChatterStage minStage, RoadChatterStage maxStage, int minCycle,
            string firstEn, string firstPt, string secondEn, string secondPt)
        {
            return new RoadChatterSpec(id, first, second, kind, minStage, maxStage, minCycle,
                new RoadChatterLine(firstEn, firstPt), new RoadChatterLine(secondEn, secondPt));
        }
    }
}
