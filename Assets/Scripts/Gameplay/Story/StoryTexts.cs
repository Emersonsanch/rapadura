using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the campaign story: the story panel's chrome
    /// (<c>story.ui.*</c>) and every panel of <see cref="StoryScript"/> (<c>story.&lt;beatId&gt;.*</c>).
    /// Same self-contained pattern as <c>BossTexts</c>/<c>TutorialTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback when no
    /// manager is registered.
    ///
    /// Canon (Docs/GDD.md, "Árvore genealógica"): the five heroes are cousins who have known each
    /// other all their lives — Maithe and Ícaro are siblings, Joaquim and Maria are siblings, Lavine is
    /// cousin to all four; oldest to youngest Maithe, Maria, Joaquim, Ícaro, Lavine. The story ties into
    /// their lore: Maria's vision of the crystals losing their light, Maithe's Olho do Vento, Ícaro's
    /// ancient machines, Joaquim's Escudo das Raízes and Lavine's Noite Rubra. Region bosses in order:
    /// Lobo Alfa (Floresta), Guardião da Ruína (Deserto), Senhor da Guerra (Montanha), Habitante Ancestral (Cavernas).
    /// </summary>
    public static class StoryTexts
    {
        public const string Next = "story.ui.next";
        public const string Skip = "story.ui.skip";
        public const string Finish = "story.ui.finish";
        public const string ToCredits = "story.ui.credits";
        public const string TapHint = "story.ui.tap_hint";
        public const string Progress = "story.ui.progress";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Next, "Continue", "Continuar"),
            new LocalizationEntry(Skip, "Skip", "Pular"),
            new LocalizationEntry(Finish, "Onward!", "Seguir viagem!"),
            new LocalizationEntry(ToCredits, "Roll credits", "Ver os créditos"),
            new LocalizationEntry(TapHint, "Tap the screen to continue", "Toque na tela para continuar"),
            new LocalizationEntry(Progress, "{0}/{1}", "{0}/{1}"),

            // ---- Prologue: why the five cousins set out ----
            new LocalizationEntry("story.prologue.title", "Prologue: The Dimmed Crystal", "Prólogo: o Cristal apagado"),
            new LocalizationEntry("story.prologue.1",
                "Every holiday, the whole family gathers at Grandma Zefa's farm, at the edge of the Forest. The five cousins grew up there: climbing cajá trees, hearing stories on the porch and fighting over the last piece of rapadura in the jar.",
                "Toda férias, a família inteira se junta no sítio da Vó Zefa, na beira da Floresta. Os cinco primos cresceram ali: subindo em pé de cajá, ouvindo história na varanda e brigando pelo último pedaço de rapadura do pote."),
            new LocalizationEntry("story.prologue.2",
                "But this year the village's Sun Crystal woke up dark. The Forest animals turned wild, the roads turned dangerous, and a cold shadow began to seep out of the Caves, on the far side of the map.",
                "Mas, neste ano, o Cristal do Sol da vila amanheceu apagado. Os bichos da Floresta ficaram bravos, as estradas ficaram perigosas e uma sombra fria começou a escorrer das Cavernas, lá do outro lado do mapa."),
            new LocalizationEntry("story.prologue.hero.maithe",
                "Through the Eye of the Wind, Maithe, the eldest, saw a thread of stolen light running towards the Caves. She was not going to let her cousins go alone — and they were not going to let Maithe go alone either.",
                "Pelo Olho do Vento, Maithe, a mais velha, viu um fio de luz roubada correndo rumo às Cavernas. Ela não ia deixar os primos irem sozinhos — e eles não iam deixar a Maithe ir sozinha."),
            new LocalizationEntry("story.prologue.hero.maria",
                "Maria dreamed of the crystals losing their glow, one by one, all the way to the Caves. She woke up determined: if the light was taken, someone has to bring it back. And she knew exactly who to call.",
                "Maria sonhou com os cristais perdendo o brilho, um por um, até chegar às Cavernas. Acordou decidida: se levaram a luz, alguém precisa ir buscar. E ela sabia direitinho quem chamar."),
            new LocalizationEntry("story.prologue.hero.joaquim",
                "Joaquim had his backpack on before anyone had decided anything. \"If there's danger, I go first,\" he said, holding the Shield of Roots. His sister Maria rolled her eyes — and followed him, as always.",
                "Joaquim já estava de mochila nas costas antes de alguém decidir qualquer coisa. \"Se tem perigo, eu vou na frente\", disse, segurando o Escudo das Raízes. A irmã, Maria, revirou os olhos — e foi atrás dele, como sempre."),
            new LocalizationEntry("story.prologue.hero.icaro",
                "Overnight, Ícaro took Grandma's radio apart and built a light meter. The needle always pointed the same way: the Caves. \"See? Science,\" he told his big sister Maithe, very proud of himself.",
                "Ícaro desmontou o rádio da Vó e, numa noite só, montou um medidor de luz. A agulha apontava sempre para o mesmo lado: as Cavernas. \"Viu? Ciência\", disse ele, todo orgulhoso, para a irmã Maithe."),
            new LocalizationEntry("story.prologue.hero.lavine",
                "Lavine, the youngest, felt the shadow before anyone else — the same chill as the Noite Rubra she was born under. This time she was not staying home to wait. \"I'm family too,\" she warned them.",
                "Lavine, a caçula, sentiu a sombra antes de todo mundo — o mesmo frio da Noite Rubra em que nasceu. Desta vez, ela não ia ficar em casa esperando. \"Eu também sou da família\", avisou."),
            new LocalizationEntry("story.prologue.3",
                "Grandma Zefa wrapped rapadura for the road, kissed each of them on the forehead and said: \"Stay together. Together, you are stronger than any darkness.\" And so the five cousins set off, into the Forest.",
                "A Vó Zefa embrulhou rapadura para a viagem, deu um beijo na testa de cada um e disse: \"Fiquem juntos. Juntos, vocês são mais fortes que qualquer escuridão.\" E assim os cinco primos partiram, Floresta adentro."),

            // ---- Interlude: Forest cleared (Lobo Alfa) ----
            new LocalizationEntry("story.interlude_forest.title", "The Forest Breathes Again", "A Floresta respira de novo"),
            new LocalizationEntry("story.interlude_forest.1",
                "When the Alpha Wolf fell, a small dark stone rolled off its collar and melted into smoke. The wolf blinked, confused, and slipped back into the woods in silence, like someone waking from a nightmare.",
                "Quando o Lobo Alfa caiu, uma pedrinha escura rolou da coleira dele e se desfez em fumaça. O lobo piscou, confuso, e voltou para a mata em silêncio, como quem acorda de um pesadelo."),
            new LocalizationEntry("story.interlude_forest.2",
                "\"He wasn't bad,\" said Maria, kneeling in the grass. \"Someone put out his light.\" Lavine squeezed her cousin's hand: she knew that cold very well.",
                "\"Ele não era mau\", disse Maria, ajoelhada na grama. \"Alguém apagou a luz dele.\" Lavine apertou a mão da prima: ela conhecia bem aquele frio."),
            new LocalizationEntry("story.interlude_forest.3",
                "On the ground, a trail of golden sand pointed east. Ícaro scratched his head: \"Sand? Here in the Forest?\" Maithe was already tightening her backpack. \"Next stop: the Desert.\"",
                "No chão, um rastro de areia dourada apontava para o leste. Ícaro coçou a cabeça: \"Areia? Aqui na Floresta?\" Maithe já estava apertando a mochila. \"Próxima parada: o Deserto.\""),

            // ---- Interlude: Desert cleared (Guardião da Ruína) ----
            new LocalizationEntry("story.interlude_desert.title", "Secrets in the Sand", "Segredos na areia"),
            new LocalizationEntry("story.interlude_desert.1",
                "Among the Ruin Guardian's broken stones, Ícaro brushed the sand off an old panel full of gears and drawings. \"It's a message!\" he shouted, his eyes shining brighter than any crystal.",
                "Entre as pedras do Guardião da Ruína, Ícaro limpou a areia de um painel antigo, cheio de engrenagens e desenhos. \"É uma mensagem!\", gritou, com os olhos brilhando mais que qualquer cristal."),
            new LocalizationEntry("story.interlude_desert.2",
                "Long ago, the ancients sealed a creature hungry for light deep in the Caves. The Guardian watched over the way there but, over the years, the shadow reached him too.",
                "Há muito tempo, os antigos prenderam uma criatura faminta de luz no fundo das Cavernas. O Guardião vigiava o caminho, mas, com os anos, a sombra acabou chegando nele também."),
            new LocalizationEntry("story.interlude_desert.3",
                "\"So the seal is breaking,\" said Joaquim, as serious as their dad gets. Maithe looked at the mountains on the horizon. \"Then we have to walk faster.\"",
                "\"Então o selo está se rompendo\", disse Joaquim, sério do jeito que o pai deles fica. Maithe olhou para as montanhas no horizonte. \"Então a gente tem que andar mais rápido.\""),

            // ---- Interlude: Mountain cleared (Senhor da Guerra) ----
            new LocalizationEntry("story.interlude_mountain.title", "At the Top of the Mountain", "No alto da Montanha"),
            new LocalizationEntry("story.interlude_mountain.1",
                "The Warlord lowered his sword and, for the first time, spoke with a human voice: \"Thank you, children. I guarded this mountain, until the shadow made me forget who I was.\"",
                "O Senhor da Guerra baixou a espada e, pela primeira vez, falou com voz de gente: \"Obrigado, crianças. Eu guardava esta montanha, até a sombra me fazer esquecer quem eu era.\""),
            new LocalizationEntry("story.interlude_mountain.2",
                "He pointed at the dark mouth of the Caves far below. \"What lives down there does not hate you. It is afraid of the dark, so it steals every light it finds.\"",
                "Ele apontou para a boca escura das Cavernas, lá embaixo. \"O que mora ali não odeia vocês. Ele tem medo do escuro, e por isso rouba toda luz que encontra.\""),
            new LocalizationEntry("story.interlude_mountain.3",
                "That night, around the campfire, nobody could sleep. Lavine leaned on Maithe's shoulder; Joaquim split the last rapadura into five equal pieces. Tomorrow, the Caves.",
                "Naquela noite, em volta da fogueira, ninguém conseguiu dormir. Lavine encostou no ombro da Maithe; Joaquim dividiu a última rapadura em cinco pedaços iguais. Amanhã, as Cavernas."),

            // ---- Ending: Caves cleared (Habitante Ancestral) ----
            new LocalizationEntry("story.ending.title", "Epilogue: The Light Comes Home", "Epílogo: a luz volta para casa"),
            new LocalizationEntry("story.ending.1",
                "Deep in the Caves, the Ancient Dweller curled up on the floor, hugging a pile of stolen light. It was no monster: just someone who had been alone in the dark for far too long.",
                "No fundo das Cavernas, o Habitante Ancestral se encolheu no chão, abraçado a um monte de luz roubada. Não era um monstro: era só alguém que tinha ficado sozinho no escuro por tempo demais."),
            new LocalizationEntry("story.ending.2",
                "Maria lit her light slowly, gently, and Lavine showed that a shadow can be soft too. \"You don't have to steal it,\" said the youngest. \"We can share.\"",
                "Maria acendeu a sua luz devagarinho, sem pressa, e Lavine mostrou que a sombra também pode ser macia. \"Você não precisa roubar\", disse a caçula. \"Dá pra dividir.\""),
            new LocalizationEntry("story.ending.3",
                "The light ran back through the world like water in a creek: across the Mountains, the Desert and the Forest, until the village's Sun Crystal glowed again, warm as a late afternoon.",
                "A luz voltou a correr pelo mundo como água de riacho: pela Montanha, pelo Deserto, pela Floresta, até o Cristal do Sol da vila acender de novo, quentinho como fim de tarde."),
            new LocalizationEntry("story.ending.hero.maithe",
                "On the way back, Maithe counted her cousins once, twice, three times. Everyone was there. For the first time on the whole trip, the eldest let someone else carry her backpack.",
                "No caminho de volta, Maithe contou os primos uma, duas, três vezes. Estavam todos ali. Pela primeira vez na viagem inteira, a mais velha deixou alguém carregar a mochila dela."),
            new LocalizationEntry("story.ending.hero.maria",
                "Maria kept a sliver of the crystal in her pocket, still warm. When the others asked what for, she smiled: \"To remember that we can do anything, as long as we go together.\"",
                "Maria guardou uma lasquinha do cristal no bolso, ainda morna. Quando os outros perguntaram para quê, ela sorriu: \"Para lembrar que a gente consegue tudo, desde que vá junto.\""),
            new LocalizationEntry("story.ending.hero.joaquim",
                "Joaquim pretended he wasn't tired all the way to the farm gate. There he dropped into the hammock and admitted, quietly: \"Good thing you were all behind me. Or in front of me.\"",
                "Joaquim fingiu que não estava cansado até chegar na porteira do sítio. Lá, desabou na rede e admitiu, baixinho: \"Ainda bem que vocês estavam atrás de mim. Ou na frente.\""),
            new LocalizationEntry("story.ending.hero.icaro",
                "Ícaro wrote everything down in his notebook: the gears, the maps, the way the light came back. On the last page he wrote: \"Best invention ever: my cousins.\" And hid it before Maithe could see.",
                "Ícaro anotou tudo no caderno: as engrenagens, os mapas, o jeito que a luz voltou. Na última página, escreveu: \"Melhor invenção de todas: os primos.\" E escondeu antes que a Maithe visse."),
            new LocalizationEntry("story.ending.hero.lavine",
                "Lavine came home with her head held high. Nobody said she was too little anymore. And whenever the Noite Rubra returned, she already knew: her power was for protecting the people she loves.",
                "Lavine voltou para casa de cabeça erguida. Ninguém mais disse que ela era pequena demais. E, quando a Noite Rubra voltasse, ela já sabia: o poder dela era para proteger quem ela ama."),
            new LocalizationEntry("story.ending.4",
                "On the porch, Grandma Zefa was waiting with corn cake and a full jar of rapadura. \"I knew it,\" she said, hugging all five at once. And the family, whole again, stayed there until the stars came out.",
                "Na varanda, a Vó Zefa esperava com bolo de milho e o pote de rapadura cheio. \"Eu sabia\", disse ela, abraçando os cinco de uma vez. E a família, inteira de novo, ficou ali até as estrelas aparecerem."),
            new LocalizationEntry("story.ending.5",
                "The end... for now. There are still corners of the map to explore, and these cousins never say no to an adventure.",
                "Fim... por enquanto. Ainda tem canto do mapa para explorar, e esses primos nunca dizem não para uma aventura."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Next)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
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
