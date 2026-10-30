using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the cutscenes (<see cref="CutsceneScript"/>): player chrome
    /// (<c>cutscene.ui.*</c>), speaker names, the intro / outro subtitle lines and the boss stingers.
    /// Same self-contained pattern as <see cref="StoryTexts"/>: merged into the live <see cref="LocalizationManager"/>
    /// on first use (keys it already has win), pt-BR fallback when no manager is registered.
    ///
    /// Canon (Docs/Narrative.md): the five cousins in birth order Maithe, Maria, Joaquim, Ícaro, Lavine; siblings
    /// Maithe &amp; Ícaro, Joaquim &amp; Maria; Lavine cousin to all. Grandparents Vó Edna (keeps the orchard house by the Vila) and
    /// Vô Jorge (his rocking chair on the porch). Lines only cite what the "Todos sabem" column has unlocked at that
    /// point: the intro's seal hook is narrated (the cousins don't know about the seal yet).
    /// Mythic frame (Manichaean, kid-friendly, told as Vô Jorge's tale — not doctrine): Light vs Darkness, sparks of
    /// light trapped in matter to be set free, the Call (Maria's dream) and the Answer (the cousins), the Prince of
    /// Darkness's shadow, and the Column of Glory the freed light climbs in the outro.
    /// </summary>
    public static class CutsceneTexts
    {
        public const string Skip = "cutscene.ui.skip";
        public const string TapHint = "cutscene.ui.tap_hint";
        public const string StingerFormat = "cutscene.stinger.format";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Skip, "Skip", "Pular"),
            new LocalizationEntry(TapHint, "Tap to continue", "Toque para continuar"),
            new LocalizationEntry(StingerFormat, "{0}: “{1}”", "{0}: “{1}”"),

            // ---- Speakers ----
            new LocalizationEntry("cutscene.speaker.vo_edna", "Grandma Edna", "Vó Edna"),
            new LocalizationEntry("cutscene.speaker.maithe", "Maithe", "Maithe"),
            new LocalizationEntry("cutscene.speaker.maria", "Maria", "Maria"),
            new LocalizationEntry("cutscene.speaker.joaquim", "Joaquim", "Joaquim"),
            new LocalizationEntry("cutscene.speaker.icaro", "Ícaro", "Ícaro"),
            new LocalizationEntry("cutscene.speaker.lavine", "Lavine", "Lavine"),

            // ---- Intro: arriving at Vó Edna's house (before the prologue panels) ----
            new LocalizationEntry("cutscene.intro.title",
                "The Call",
                "O Chamado"),
            new LocalizationEntry("cutscene.intro.1",
                "Grandpa Jorge used to say that, at the beginning of everything, the Light and the Darkness lived apart. Until the Darkness got jealous... and swallowed a little piece of the Light.",
                "O Vô Jorge contava que, no começo de tudo, a Luz e a Treva moravam separadas. Até que a Treva ficou com inveja... e engoliu um pedacinho da Luz."),
            new LocalizationEntry("cutscene.intro.2",
                "I got here first! Nobody saw it, but I got here first.",
                "Cheguei primeiro! Ninguém viu, mas cheguei primeiro."),
            new LocalizationEntry("cutscene.intro.3",
                "First nothing! I was in front the whole way!",
                "Primeiro nada! Eu vim na frente a estrada inteira!"),
            new LocalizationEntry("cutscene.intro.4",
                "One, two, three, four, five. Everyone's here. Let's go.",
                "Um, dois, três, quatro, cinco. Todo mundo aqui. Bora."),
            new LocalizationEntry("cutscene.intro.5",
                "My grandchildren! Come in, come in — the tea is hot and the apricot cake is coming out of the tandir.",
                "Meus netos! Entra, entra, que o chá tá quentinho e o bolo de damasco tá saindo do tandir."),
            new LocalizationEntry("cutscene.intro.6",
                "Sis... isn't the Sun Crystal supposed to be glowing? It's completely dark.",
                "Mana... o Cristal do Sol não devia estar brilhando? Tá apagadinho."),
            new LocalizationEntry("cutscene.intro.7",
                "Deep in the Threshold Caves, a very old sealed door cracked. The Prince of Darkness's shadow seeped across Dawn Isle and scattered the Crystal's light into a thousand sparks — trapped in animals, in stones, in guardians.",
                "No fundo das Grutas do Limiar, uma porta selada muito velha rachou. A sombra do Príncipe das Trevas escorreu pela Ilha da Aurora e espalhou a luz do Cristal em mil faíscas — presas nos bichos, nas pedras, nos guardiões."),
            new LocalizationEntry("cutscene.intro.8",
                "I dreamed about this. Little sparks of light stuck in the dark, calling us... all the way over there.",
                "Eu sonhei com isso. Faisquinhas de luz presas no escuro, chamando a gente... até lá."),
            new LocalizationEntry("cutscene.intro.9",
                "Grandpa Jorge always said: when the light calls, we answer. Together, my grandchildren. Always together.",
                "O Vô Jorge sempre dizia: quando a luz chama, a gente responde. Juntos, meus netos. Sempre juntos."),

            // ---- Outro: the light comes home (after the ending panels, before the credits) ----
            new LocalizationEntry("cutscene.outro.title",
                "The Column of Glory",
                "A Coluna da Glória"),
            new LocalizationEntry("cutscene.outro.1",
                "Spark by spark, the trapped light was set free, and the cold shadow drew back behind the remade seal — pushed back, not defeated. The light rose to the sky like a shining column — the Column of Glory, the one Grandpa Jorge pointed at on starry nights.",
                "Faísca por faísca, a luz presa foi solta, e a sombra fria recuou para trás do selo refeito — empurrada de volta, não vencida. A luz subiu pro céu feito uma coluna brilhante — a Coluna da Glória, que o Vô Jorge apontava nas noites de estrela."),
            new LocalizationEntry("cutscene.outro.2",
                "Bye, Tages! We'll come back to visit, neighbour. With a lamp!",
                "Tchau, Tages! A gente volta pra te visitar, vizinho. Com lamparina!"),
            new LocalizationEntry("cutscene.outro.3",
                "Meter maxed out in every region. Scientific conclusion: it worked.",
                "Medidor no máximo em todas as regiões. Conclusão científica: deu certo."),
            new LocalizationEntry("cutscene.outro.4",
                "Does anyone smell apricot cake? I do. I always do.",
                "Alguém sentiu cheiro de bolo de damasco? Eu senti. Eu sempre sinto."),
            new LocalizationEntry("cutscene.outro.5",
                "One, two, three, four, five. And Ari: six. Everyone's home. ...Does his shadow on the wall have little wings?",
                "Um, dois, três, quatro, cinco. E o Ari: seis. Todo mundo em casa. ...A sombra dele na parede tem asinha?"),
            new LocalizationEntry("cutscene.outro.6",
                "Grandma... we heard the call. And we answered. All five of us, just like in the dream.",
                "Vó... a gente ouviu o chamado. E respondeu. Os cinco, que nem no sonho."),
            new LocalizationEntry("cutscene.outro.7",
                "I knew it. Now sit down — I want to hear this story from the very beginning.",
                "Eu sabia. Agora senta, que essa história eu quero ouvir do começo."),
            new LocalizationEntry("cutscene.outro.8",
                "On Grandma Edna's porch, the deep arched iwan, Grandpa Jorge's rocking chair creaked softly. And up above, the Column of Glory shone, one spark at a time.",
                "Na varanda da Vó Edna, o iwan de arco fundo, a cadeira de balanço do Vô Jorge rangeu de leve. E lá em cima, a Coluna da Glória brilhava, uma faísca de cada vez."),

            // ---- Boss stingers (under the boss banner): the active cousin's quip, or the narrator without a hero ----
            new LocalizationEntry("cutscene.stinger.forest", "Eyes glow in the dark of the tugai.", "Olhos brilham no escuro do tugai."),
            new LocalizationEntry("cutscene.stinger.forest.maithe", "Fresh tracks. It's him. Nobody runs.", "Rastro fresco. É ele. Ninguém corre."),
            new LocalizationEntry("cutscene.stinger.forest.maria",
                "Easy... there's a spark of light trapped in him. Let's set it free.",
                "Calma... tem uma faísca de luz presa nele. A gente solta."),
            new LocalizationEntry("cutscene.stinger.forest.joaquim", "Bring it, big wolf! Shield up front!", "Pode vir, lobão! Escudo na frente!"),
            new LocalizationEntry("cutscene.stinger.forest.icaro", "Three times a normal wolf. And blue. Noting it down: runs a LOT.", "Três vezes um lobo normal. E azul. Anotando: corre MUITO."),
            new LocalizationEntry("cutscene.stinger.forest.lavine", "He's big. And ugly. I'm saying it. I'm not scared.", "Ele é grande. E feio. Eu falo. Não tenho medo."),

            new LocalizationEntry("cutscene.stinger.desert", "The red sand shifts. Something old and carved wakes up.", "A areia vermelha se mexe. Algo antigo e entalhado acorda."),
            new LocalizationEntry("cutscene.stinger.desert.maithe", "That's no animal. It's walking stone. Aim for the glow.", "Isso não é bicho. É pedra que anda. Mira no brilho."),
            new LocalizationEntry("cutscene.stinger.desert.maria", "Breathe, everyone. Breathe and go.", "Respira, gente. Respira e vai."),
            new LocalizationEntry("cutscene.stinger.desert.joaquim", "Big guy, huh? I'm bigger on the inside!", "Grandão, é? Eu sou maior por dentro!"),
            new LocalizationEntry("cutscene.stinger.desert.icaro", "Gears! Sis, someone BUILT this thing!", "Engrenagens! Mana, alguém CONSTRUIU isso!"),
            new LocalizationEntry("cutscene.stinger.desert.lavine", "The sand is cold. Sand isn't supposed to be cold.", "A areia tá fria. Areia não devia ser fria."),

            new LocalizationEntry("cutscene.stinger.mountain", "The mine bell rings once. Then silence.", "O sino da mina toca uma vez. Depois, silêncio."),
            new LocalizationEntry("cutscene.stinger.mountain.maithe", "The wind is lying to the Eye. I'm trusting you all.", "O vento tá mentindo pro Olho. Vou confiar em vocês."),
            new LocalizationEntry("cutscene.stinger.mountain.maria", "He's fighting the shadow too. You can see it.", "Ele tá lutando contra a sombra também. Dá pra ver."),
            new LocalizationEntry("cutscene.stinger.mountain.joaquim", "This time... I know when to step aside.", "Dessa vez... eu sei a hora de sair da frente."),
            new LocalizationEntry("cutscene.stinger.mountain.icaro", "Giant-sized stone armour, attack angle... statistically, we win.", "Armadura de pedra tamanho gigante, ângulo de ataque... estatisticamente, a gente ganha."),
            new LocalizationEntry("cutscene.stinger.mountain.lavine", "He shouts loud. I shout louder.", "Ele grita alto. Eu grito mais."),

            new LocalizationEntry("cutscene.stinger.caves",
                "Every lamp goes out at once. The Darkness is listening.",
                "Todas as lamparinas apagam de uma vez. A Treva está ouvindo."),
            new LocalizationEntry("cutscene.stinger.caves.maithe", "Maria goes first with the light. I've got your back.", "A Maria vai na frente com a luz. Eu cubro."),
            new LocalizationEntry("cutscene.stinger.caves.maria", "Hey... it's all right. We brought light to share.", "Ei... tá tudo bem. A gente trouxe luz pra dividir."),
            new LocalizationEntry("cutscene.stinger.caves.joaquim", "Sis, stay behind me. Or beside me. Just stay close!", "Mana, fica atrás de mim. Ou do lado. Só fica perto!"),
            new LocalizationEntry("cutscene.stinger.caves.icaro", "Meter at zero. It ate all the light down here.", "Medidor zerado. Ele comeu toda a luz daqui."),
            new LocalizationEntry("cutscene.stinger.caves.lavine", "The dark doesn't scare me. But you're scared of it, aren't you?", "O escuro não me assusta. Mas você tem medo dele, né?"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Skip)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (key == null)
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

        /// <summary><see cref="Get"/> + <c>string.Format</c>; falls back to the pt-BR pattern if a translation's placeholders are broken.</summary>
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

        /// <summary>The pt-BR text of <paramref name="key"/>, or null if this helper doesn't define it.</summary>
        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
