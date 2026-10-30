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
    /// fascination with the Artificers' engines (ushabtis), Joaquim's Escudo das Raízes and Lavine's Noite Rubra. Region bosses in order
    /// (Docs/WorldBible_Maniqueismo.md §3.3): Kök-Böri (Bosque do Zarafshan), o Grande Ushabti (Areias Flamejantes),
    /// Hahya (Serra de Kaggen), Tages (Grutas do Limiar). Grandparents: Vó Edna and Vô Jorge; the dog is Ari.
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
                "Every holiday, the whole family gathers at Grandma Edna's house, in the orchard at the edge of the Zarafshan Woods. The five cousins grew up there: climbing apricot trees, listening to Grandpa Jorge's stories on the porch and fighting over the last Golden Brick in the jar.",
                "Toda férias, a família inteira se junta na casa da Vó Edna, no pomar à beira do Bosque do Zarafshan. Os cinco primos cresceram ali: subindo em pé de damasco, ouvindo as histórias do Vô Jorge na varanda e brigando pelo último Torrão Dourado do pote."),
            new LocalizationEntry("story.prologue.2",
                "But this year the village's Sun Crystal woke up dark. The animals of the Woods dimmed and turned wild, the roads turned dangerous, and a cold shadow began to seep out of the Threshold Caves, on the far side of the island.",
                "Mas, neste ano, o Cristal do Sol da vila amanheceu apagado. Os bichos do Bosque se apagaram e ficaram bravos, as estradas ficaram perigosas e uma sombra fria começou a escorrer das Grutas do Limiar, lá do outro lado da ilha."),
            new LocalizationEntry("story.prologue.hero.maithe",
                "Through the Eye of the Wind, Maithe, the eldest, saw a thread of stolen light running towards the Caves. She was not going to let her cousins go alone — and they were not going to let Maithe go alone either.",
                "Pelo Olho do Vento, Maithe, a mais velha, viu um fio de luz roubada correndo rumo às Grutas. Ela não ia deixar os primos irem sozinhos — e eles não iam deixar a Maithe ir sozinha."),
            new LocalizationEntry("story.prologue.hero.maria",
                "Maria dreamed of the crystals losing their glow, one by one, all the way to the Caves, and of little sparks of light calling from the dark. She woke up determined: if the light was taken, someone has to bring it back. And she knew exactly who to call.",
                "Maria sonhou com os cristais perdendo o brilho, um por um, até chegar às Grutas, e com faisquinhas de luz chamando lá do escuro. Acordou decidida: se levaram a luz, alguém precisa ir buscar. E ela sabia direitinho quem chamar."),
            new LocalizationEntry("story.prologue.hero.joaquim",
                "Joaquim had his backpack on before anyone had decided anything. \"If there's danger, I go first,\" he said, holding the Shield of Roots. His sister Maria rolled her eyes — and followed him, as always.",
                "Joaquim já estava de mochila nas costas antes de alguém decidir qualquer coisa. \"Se tem perigo, eu vou na frente\", disse, segurando o Escudo das Raízes. A irmã, Maria, revirou os olhos — e foi atrás dele, como sempre."),
            new LocalizationEntry("story.prologue.hero.icaro",
                "Overnight, Ícaro took Grandma's radio apart and built a light meter. The needle always pointed the same way: the Caves. \"See? Science,\" he told his big sister Maithe, very proud of himself.",
                "Ícaro desmontou o rádio da Vó e, numa noite só, montou um medidor de luz. A agulha apontava sempre para o mesmo lado: as Grutas. \"Viu? Ciência\", disse ele, todo orgulhoso, para a irmã Maithe."),
            new LocalizationEntry("story.prologue.hero.lavine",
                "Lavine, the youngest, felt the shadow before anyone else — the same chill as the Noite Rubra she was born under. This time she was not staying home to wait. \"I'm family too,\" she warned them.",
                "Lavine, a caçula, sentiu a sombra antes de todo mundo — o mesmo frio da Noite Rubra em que nasceu. Desta vez, ela não ia ficar em casa esperando. \"Eu também sou da família\", avisou."),
            new LocalizationEntry("story.prologue.3",
                "Grandma Edna wrapped tandir bread and Golden Brick for the road, checked that each of them carried one of the battery radios Ícaro built from the leftovers of hers and kissed them on the forehead: \"Tell me about every creature you meet — I'll write it all in my notebook. And stay together: together, you are stronger than any darkness.\" And so the five cousins set off, into the Zarafshan Woods.",
                "A Vó Edna embrulhou nan do tandir e Torrão Dourado para a viagem, conferiu se cada um levava o radinho de pilha que o Ícaro montou com as sobras do rádio dela e deu um beijo na testa de cada um: \"Me contem cada bicho que encontrarem, que eu anoto tudo no meu caderno. E fiquem juntos: juntos, vocês são mais fortes que qualquer escuridão.\" E assim os cinco primos partiram, Bosque do Zarafshan adentro."),

            // ---- Interlude: Forest cleared (Kök-Böri, the Sky Wolf) ----
            new LocalizationEntry("story.interlude_forest.title", "The Woods Breathe Again", "O Bosque respira de novo"),
            new LocalizationEntry("story.interlude_forest.1",
                "When Kök-Böri, the Sky Wolf, fell, a small dark stone rolled off his collar and melted into smoke. A spark of light rose from his fur towards the sky. The wolf blinked, confused, and slipped back among the poplars in silence, like someone waking from a nightmare.",
                "Quando Kök-Böri, o Lobo Celeste, caiu, uma pedrinha escura rolou da coleira dele e se desfez em fumaça. Uma faísca de luz saiu do pelo dele e subiu para o céu. O lobo piscou, confuso, e voltou para o meio dos choupos em silêncio, como quem acorda de um pesadelo."),
            new LocalizationEntry("story.interlude_forest.2",
                "\"He wasn't bad,\" said Maria, kneeling in the grass. \"Someone put out his light. Grandpa Jorge said this wolf used to guide travellers through the woods.\" Lavine squeezed her cousin's hand: she knew that cold very well.",
                "\"Ele não era mau\", disse Maria, ajoelhada na grama. \"Alguém apagou a luz dele. O Vô Jorge contava que esse lobo guiava os viajantes pelo bosque.\" Lavine apertou a mão da prima: ela conhecia bem aquele frio."),
            new LocalizationEntry("story.interlude_forest.3",
                "On the ground, a trail of golden sand pointed east. Ari sniffed it and sneezed. Ícaro scratched his head: \"Sand? Here in the Woods?\" Maithe was already tightening her backpack. \"Next stop: the Flaming Sands.\"",
                "No chão, um rastro de areia dourada apontava para o leste. O Ari farejou e espirrou. Ícaro coçou a cabeça: \"Areia? Aqui no Bosque?\" Maithe já estava apertando a mochila. \"Próxima parada: as Areias Flamejantes.\""),

            // ---- Interlude: Desert cleared (the Great Ushabti) ----
            new LocalizationEntry("story.interlude_desert.title", "Secrets in the Sand", "Segredos na areia"),
            new LocalizationEntry("story.interlude_desert.1",
                "Among the Great Ushabti's broken stones, Ícaro brushed the sand off an old panel full of gears and drawings. \"It's a message! And 'ushabti' means 'the one who answers'!\" he shouted, his eyes shining brighter than any crystal.",
                "Entre as pedras do Grande Ushabti, Ícaro limpou a areia de um painel antigo, cheio de engrenagens e desenhos. \"É uma mensagem! E 'ushabti' quer dizer 'o que responde'!\", gritou, com os olhos brilhando mais que qualquer cristal."),
            new LocalizationEntry("story.interlude_desert.2",
                "Long ago, the Artificers closed the door at the bottom of the Threshold Caves with a seal: the threshold between the island and the Land of Darkness. Something hungry for light waits behind it. They built the ushabtis to watch over the seal, but the seal is cracking, the shadow reached the Great Ushabti too, and he forgot how to answer.",
                "Há muito tempo, os Artífices fecharam com um selo a porta do fundo das Grutas do Limiar: o limiar entre a ilha e a Terra da Treva. Atrás dele espera algo faminto de luz. Eles fizeram os ushabtis para vigiar o selo, mas o selo está rachando, a sombra chegou no Grande Ushabti também, e ele esqueceu como responder."),
            new LocalizationEntry("story.interlude_desert.3",
                "\"So the seal is breaking,\" said Joaquim, as serious as his and Maria's dad gets. On the radio, Grandma Edna's voice crackled: \"It's all in my notebook now. Go on — together.\" Maithe looked at the Kaggen Range on the horizon. \"Then we have to walk faster.\"",
                "\"Então o selo está se rompendo\", disse Joaquim, sério do jeito que o pai dele e da Maria fica. No radinho, a voz da Vó Edna chiou: \"Já anotei tudo no caderno. Agora vão — e juntos.\" Maithe olhou para a Serra de Kaggen no horizonte. \"Então a gente tem que andar mais rápido.\""),

            // ---- Interlude: Mountain cleared (Hahya, the Giant Warrior) ----
            new LocalizationEntry("story.interlude_mountain.title", "At the Top of the Range", "No alto da Serra"),
            new LocalizationEntry("story.interlude_mountain.1",
                "Hahya, the Giant Warrior, lowered his sword and, for the first time, spoke with a gentle voice: \"Thank you, children. Long ago I dreamed of ruin and chose to guard this range instead, until the shadow made me forget who I was.\"",
                "Hahya, o Gigante Guerreiro, baixou a espada e, pela primeira vez, falou com voz mansa: \"Obrigado, crianças. Há muito tempo eu sonhei com destruição e escolhi guardar esta serra, até a sombra me fazer esquecer quem eu era.\""),
            new LocalizationEntry("story.interlude_mountain.2",
                "He pointed at the dark mouth of the Threshold Caves far below. \"What lives down there does not hate you. It is afraid of the dark, so it hoards every light it finds. The cold shadow does not come from it: it comes from behind the seal.\"",
                "Ele apontou para a boca escura das Grutas do Limiar, lá embaixo. \"O que mora ali não odeia vocês. Ele tem medo do escuro, e por isso agarra toda luz que encontra. A sombra fria não vem dele: vem de trás do selo.\""),
            new LocalizationEntry("story.interlude_mountain.3",
                "That night, around the campfire, nobody could sleep. Lavine leaned on Maithe's shoulder and pointed at the moon: \"Kaggen's shoe.\" Joaquim split the last Golden Brick into five equal pieces — plus a tiny one, hidden, for Ari. Tomorrow, the Caves.",
                "Naquela noite, em volta da fogueira, ninguém conseguiu dormir. Lavine encostou no ombro da Maithe e apontou para a lua: \"O sapato do Kaggen.\" Joaquim dividiu o último Torrão Dourado em cinco pedaços iguais — e mais um pedacinho escondido pro Ari. Amanhã, as Grutas."),

            // ---- Ending: Caves cleared (Tages, the Ancient Dweller) ----
            new LocalizationEntry("story.ending.title", "Epilogue: The Light Comes Home", "Epílogo: a luz volta para casa"),
            new LocalizationEntry("story.ending.1",
                "Deep in the Threshold Caves, Tages, the Ancient Dweller, curled up on the floor, hugging a pile of stolen light. He had a child's face and an old man's eyes, and he was no monster: just someone who had sprouted from the earth long ago and been alone in the dark for far too long.",
                "No fundo das Grutas do Limiar, Tages, o Habitante Ancestral, se encolheu no chão, abraçado a um monte de luz roubada. Tinha rosto de criança e olhos de velho, e não era um monstro: era só alguém que tinha brotado da terra muito tempo atrás e ficado sozinho no escuro por tempo demais."),
            new LocalizationEntry("story.ending.2",
                "Maria lit her light slowly, gently, and Lavine showed that a shadow can be soft too. \"You don't have to steal it,\" said the youngest. \"We can share.\"",
                "Maria acendeu a sua luz devagarinho, sem pressa, e Lavine mostrou que a sombra também pode ser macia. \"Você não precisa roubar\", disse a caçula. \"Dá pra dividir.\""),
            new LocalizationEntry("story.ending.3",
                "Together, the five of them remade the seal. Behind the crack, a huge faceless shadow drew back slowly — pushed back, not defeated. And the light ran back across the island like water in a creek: through the Kaggen Range, the Flaming Sands and the Zarafshan Woods, until the village's Sun Crystal glowed again, warm as a late afternoon.",
                "Juntos, os cinco refizeram o selo. Atrás da fresta, uma sombra enorme e sem rosto se afastou devagar — empurrada de volta, não vencida. E a luz voltou a correr pela ilha como água de riacho: pela Serra de Kaggen, pelas Areias Flamejantes, pelo Bosque do Zarafshan, até o Cristal do Sol da vila acender de novo, quentinho como fim de tarde."),
            new LocalizationEntry("story.ending.hero.maithe",
                "On the way back, Maithe counted her cousins once, twice, three times. Everyone was there, and the Eye of the Wind showed no threat at all. She had found out who made the monsters: nobody — they were guardians the shadow made forget. For the first time on the whole trip, the eldest let someone else carry her backpack.",
                "No caminho de volta, Maithe contou os primos uma, duas, três vezes. Estavam todos ali, e o Olho do Vento não mostrava ameaça nenhuma. Ela tinha descoberto quem fez os monstros: ninguém — eram guardiões que a sombra fez esquecer. Pela primeira vez na viagem inteira, a mais velha deixou alguém carregar a mochila dela."),
            new LocalizationEntry("story.ending.hero.maria",
                "Her dream had five cousins in it, and five were coming home. Maria kept a sliver of the crystal in her pocket, still warm. When the others asked what for, she smiled: \"To remember that we can do anything, as long as we go together.\"",
                "No sonho dela tinha cinco primos, e cinco estavam voltando. Maria guardou uma lasquinha do cristal no bolso, ainda morna. Quando os outros perguntaram para quê, ela sorriu: \"Para lembrar que a gente consegue tudo, desde que vá junto.\""),
            new LocalizationEntry("story.ending.hero.joaquim",
                "Joaquim pretended he wasn't tired all the way to the orchard gate. There he leaned the Shield of Roots against the wall — for once in no hurry to pick it up — dropped onto the cushions on the porch and admitted, quietly: \"Good thing you were all behind me. Or in front of me.\"",
                "Joaquim fingiu que não estava cansado até chegar no portão do pomar. Lá, encostou o Escudo das Raízes na parede — pela primeira vez sem pressa de pegar de volta —, desabou nas almofadas da varanda e admitiu, baixinho: \"Ainda bem que vocês estavam atrás de mim. Ou na frente.\""),
            new LocalizationEntry("story.ending.hero.icaro",
                "Ícaro wrote everything down in his notebook: the gears, the ushabtis, the maps, the way the light came back. On the last page he wrote: \"Best invention ever: my cousins.\" And hid it before Maithe could see.",
                "Ícaro anotou tudo no caderno: as engrenagens, os ushabtis, os mapas, o jeito que a luz voltou. Na última página, escreveu: \"Melhor invenção de todas: os primos.\" E escondeu antes que a Maithe visse."),
            new LocalizationEntry("story.ending.hero.lavine",
                "Lavine came home with her head held high. Nobody said she was too little anymore. And whenever the Noite Rubra returned, she already knew: her power was for protecting the people she loves.",
                "Lavine voltou para casa de cabeça erguida. Ninguém mais disse que ela era pequena demais. E, quando a Noite Rubra voltasse, ela já sabia: o poder dela era para proteger quem ela ama."),
            new LocalizationEntry("story.ending.4",
                "On the porch, Grandma Edna was waiting with apricot cake, a full jar of Golden Brick and her notebook open on a fresh page. Ari ran ahead, wagging his tail. \"I knew it,\" she said, hugging all five at once. Grandpa Jorge's rocking chair creaked softly, as if he were there too. And the family, whole again, stayed there until the stars came out.",
                "Na varanda, a Vó Edna esperava com bolo de damasco, o pote de Torrão Dourado cheio e o caderno aberto numa página nova. O Ari correu na frente, abanando o rabo. \"Eu sabia\", disse ela, abraçando os cinco de uma vez. A cadeira de balanço do Vô Jorge rangeu de leve, como se ele também estivesse ali. E a família, inteira de novo, ficou ali até as estrelas aparecerem."),
            new LocalizationEntry("story.ending.5",
                "The end... for now. There are still corners of the island to explore, and these cousins never say no to an adventure.",
                "Fim... por enquanto. Ainda tem canto da ilha para explorar, e esses primos nunca dizem não para uma aventura."),

            // ---- Nova Jornada+: replaces the prologue from the second campaign on (StoryScript.NewJourneyId) ----
            new LocalizationEntry("story.new_journey.title", "New Journey: The Crystal Flickers Again", "Nova Jornada: o Cristal pisca de novo"),
            new LocalizationEntry("story.new_journey.1",
                "One late afternoon, a few days after the party, Grandma Edna called the five of them to the porch. The Sun Crystal was flickering, weak as a candle in the wind. \"Some old shadows never learned to share,\" she said, stirring her tea.",
                "Num fim de tarde, poucos dias depois da festa, a Vó Edna chamou os cinco na varanda. O Cristal do Sol piscava fraquinho, como vela no vento. \"Tem sombra velha que nunca aprendeu a dividir\", disse ela, mexendo o chá."),
            new LocalizationEntry("story.new_journey.2",
                "From the Threshold Caves, Tages sent a message with a firefly: older, stronger shadows had woken up to the noise of the light coming back. The beasts, the ruins and the giants were fiercer than the first time.",
                "Lá das Grutas do Limiar, o Tages mandou um recado por um vaga-lume: sombras mais antigas e mais fortes acordaram com o barulho da luz voltando. Os bichos, as ruínas e os gigantes estavam mais bravos que da primeira vez."),
            new LocalizationEntry("story.new_journey.hero.maithe",
                "Maithe opened the Eye of the Wind and saw the thread of light unravelling towards the Caves again. \"Last time I counted my cousins three times,\" she said. \"This time I'll count four.\"",
                "Maithe abriu o Olho do Vento e viu o fio de luz se desfiando outra vez rumo às Grutas. \"Da primeira vez eu contei os primos três vezes\", disse ela. \"Desta vez eu conto quatro.\""),
            new LocalizationEntry("story.new_journey.hero.maria",
                "The sliver of crystal in Maria's pocket had gone cold. \"It's not the end of the light,\" she said, squeezing her brother Joaquim's hand. \"It's just a reminder to go together again.\"",
                "A lasquinha de cristal no bolso da Maria tinha esfriado. \"Não é o fim da luz\", disse ela, apertando a mão do irmão, Joaquim. \"É só um aviso para a gente ir junto de novo.\""),
            new LocalizationEntry("story.new_journey.hero.joaquim",
                "Once again, Joaquim had his backpack on before anyone else. \"Now I know the way,\" he said, knocking on the Shield of Roots. Maria laughed: \"That's what you said the first time.\"",
                "De novo, Joaquim estava de mochila nas costas antes de todo mundo. \"Agora eu conheço o caminho\", disse, batendo no Escudo das Raízes. Maria riu: \"Foi o que você disse da primeira vez.\""),
            new LocalizationEntry("story.new_journey.hero.icaro",
                "Ícaro took the light meter out of the drawer and the needle trembled, pointing at the Caves. He opened his notebook on a fresh page and wrote: \"Experiment number two.\" His big sister Maithe just shook her head.",
                "Ícaro tirou o medidor de luz da gaveta e a agulha tremeu, apontando para as Grutas. Abriu o caderno numa página nova e escreveu: \"Experimento número dois.\" A irmã, Maithe, só balançou a cabeça."),
            new LocalizationEntry("story.new_journey.hero.lavine",
                "Lavine felt the chill come back, deeper than the first time. But the youngest wasn't afraid anymore: \"If the shadow grew, so did we,\" she said, and was the first one through the orchard gate.",
                "Lavine sentiu o frio voltar, mais fundo que da primeira vez. Mas a caçula não tinha mais medo: \"Se a sombra cresceu, a gente também cresceu\", disse, e foi a primeira a passar pelo portão do pomar."),
            new LocalizationEntry("story.new_journey.3",
                "Grandma filled the Golden Brick jar again and gave each of them the same kiss on the forehead. \"You already know the way. Now light it up again — and come back in one piece.\" And the five cousins set off once more, into the Zarafshan Woods.",
                "A Vó encheu o pote de Torrão Dourado de novo e deu o mesmo beijo na testa de cada um. \"Vocês já sabem o caminho. Agora é acender de novo — e voltar inteiros.\" E os cinco primos partiram outra vez, Bosque do Zarafshan adentro."),
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
