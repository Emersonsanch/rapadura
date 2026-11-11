using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of Regions 1–4 that the overworld still played with stand-ins (bible §3.2.1, twelfth pass):
    /// lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="LateStandInBestiaryTexts"/>; <see cref="BestiaryTexts"/>
    /// appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3); the ubır only as the Tatar tales give it, a hungry spirit.
    /// </summary>
    public static class EarlyStandInBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_dimmed_stork", "enemy_forest_bubble_toad", "enemy_forest_smoke_mosquito", "enemy_forest_silk_moth",
            "enemy_forest_laurel_pigeon", "enemy_forest_laurel_lizard", "enemy_forest_smoke_peacock", "enemy_desert_thorn_lizard",
            "enemy_mountain_dimmed_lynx", "enemy_mountain_smudge", "enemy_mountain_stone_jackdaw", "enemy_cave_young_ubir",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Tugai do Rio Dourado e Prado das Cegonhas (02) --
            Page(entries, "enemy_forest_dimmed_stork",
                "Every spring the storks came back to the poles on the meadow and I counted them from the porch; your grandfather said it was the only sum I ever enjoyed. This one came back grey at the edges, as if somebody had blown out the white in it. It still builds a nest, but the twigs are for throwing. Keep moving when it lifts a beak, and don't stand under the poles.",
                "Toda primavera as cegonhas voltavam pros postes do prado e eu contava da varanda; o avô de vocês dizia que era a única conta que eu fazia com gosto. Esta voltou cinzenta nas bordas, como se alguém tivesse soprado o branco dela. Ainda faz ninho, mas os gravetos são pra jogar. Continuem andando quando ela erguer o bico, e não fiquem debaixo dos postes.",
                ("It throws from far. Walk in under it and it runs out of sticks.", "Ela joga de longe. Chega embaixo dela que acabam os gravetos."),
                ("She still remembers how to build a nest. That's something.", "Ela ainda lembra de fazer ninho. Já é alguma coisa."),
                ("A stick hit me in the head. A whole stick!", "Um graveto me acertou na cabeça. Um graveto inteiro!"),
                ("It brings the spring back and keeps the light. A bad trade for the meadow.", "Traz a primavera e fica com a luz. Mau negócio pro prado."),
                ("She's so tall! Her legs are like my whole self!", "Ela é tão alta! As pernas dela são do meu tamanho inteiro!"));

            // -- Brejo do Lodo Escuro (03) --
            Page(entries, "enemy_forest_bubble_toad",
                "The dark mire drank a little of the Darkness when the Sun Crystal went out, and the toads drank the mire. This one swelled up with it. It keeps a bubble of mud in its throat and blows it at you from the bank, and the mud sticks to your feet. Stay on the boardwalk when you can; if it hits you, Joaquim's water washes it off. And don't poke the bubble.",
                "O brejo escuro bebeu um pouco da Treva quando o Cristal do Sol apagou, e os sapos beberam o brejo. Este inchou com isso. Guarda uma bolha de lama na garganta e sopra em vocês da margem, e a lama gruda nos pés. Fiquem na passarela quando der; se acertar, a água do Joaquim lava. E não cutuquem a bolha.",
                ("It spits mud from the bank. Close in fast.", "Cospe lama da margem. Chega perto rápido."),
                ("Its throat glows. Something it swallowed is still lit in there.", "A garganta dele brilha. Tem coisa acesa lá dentro ainda."),
                ("I poked the bubble. Don't poke the bubble.", "Eu cutuquei a bolha. Não cutuquem a bolha."),
                ("A toad that drank a swamp. Proportion, it seems, was not consulted.", "Um sapo que bebeu um brejo. A proporção, pelo visto, não foi consultada."),
                ("He's so round! Can he come home if he stops spitting?", "Ele é tão redondo! Pode ir pra casa se parar de cuspir?"));

            Page(entries, "enemy_forest_smoke_mosquito",
                "Smoke is the first of the five elements of the Darkness, the old books say, and it does not keep still. Over the mire it gathered into mosquitoes, which is about what you would expect of smoke. They come in clouds and they bite, and what they drink glows in their bellies. Swing wide and keep swinging; a cloud of smoke is still only smoke.",
                "A Fumaça é o primeiro dos cinco elementos da Treva, dizem os livros antigos, e não para quieta. Em cima do brejo ela se juntou em mosquitos, que é mais ou menos o que se espera de fumaça. Vêm em nuvem e picam, e o que bebem brilha na barriga. Batam largo e continuem batendo; nuvem de fumaça continua sendo só fumaça.",
                ("They come in a cloud. Hit wide, not hard.", "Vêm em nuvem. Bate largo, não forte."),
                ("The little ember in each one is a bit of somebody's light.", "A brasinha em cada um é um pouco da luz de alguém."),
                ("I got bitten by smoke. That's not even fair.", "Fui picado por fumaça. Isso nem é justo."),
                ("Smoke learning to bite. The Darkness is nothing if not persistent.", "Fumaça aprendendo a picar. A Treva pode ser tudo, menos preguiçosa."),
                ("They buzz like the kettle! Shoo! Shoo!", "Eles zumbem igual à chaleira! Xô! Xô!"));

            // -- Bosque das Amoreiras de Seda (04) --
            Page(entries, "enemy_forest_silk_moth",
                "Silk comes from a moth that eats mulberry leaves and spins itself to sleep. This one ate the leaves of the loom gardens and never spun; it kept the silk inside and grew and grew. When it beats those big pale wings the dust comes off them, and the dust makes you drowsy. Don't stand in the cloud. Hit it before it settles, and keep your eyes open, my loves.",
                "A seda vem de uma mariposa que come folha de amoreira e se fia até dormir. Esta comeu as folhas dos quintais de tear e nunca fiou; guardou a seda dentro e cresceu, cresceu. Quando bate aquelas asas grandes e claras, o pó sai delas, e o pó dá sono. Não fiquem na nuvem. Batam antes de ela pousar, e mantenham os olhos abertos, meus amores.",
                ("It throws the dust from range. Don't breathe it in.", "Joga o pó de longe. Não respira."),
                ("All that silk and no cocoon. It never got to rest.", "Tanta seda e nenhum casulo. Nunca conseguiu descansar."),
                ("The dust made me yawn in the middle of a fight. Embarrassing.", "O pó me fez bocejar no meio da luta. Que vergonha."),
                ("It carries a loom's worth of thread and weaves nothing. A waste with wings.", "Carrega um tear inteiro de linha e não tece nada. Um desperdício com asas."),
                ("Its wings have eyes! Light eyes! Look, Joaquim, LOOK!", "As asas dela têm olhos! Olhos de luz! Olha, Joaquim, OLHA!"));

            // -- Copas de Garajonay (05) --
            Page(entries, "enemy_forest_laurel_pigeon",
                "The pigeons of the laurel woods eat only laurel berries, and the elves up there have always loved them for it. The mist took their colour and left them grey, all but the green band on the neck. Now they sit in the canopy and spit the berry stones down at anyone below. Look up when you walk under the platforms. They're not wicked; they just forgot what the berries were for.",
                "Os pombos das matas de louro só comem baga de louro, e os elfos lá de cima sempre gostaram deles por isso. A névoa tirou a cor deles e deixou tudo cinza, menos a faixa verde no pescoço. Agora ficam nas copas cuspindo caroço em quem passa embaixo. Olhem pra cima quando passarem sob as plataformas. Não são maus; só esqueceram pra que servia a baga.",
                ("It shoots from the branches. Draw it down, then strike.", "Atira dos galhos. Puxa pra baixo, depois bate."),
                ("That green band is the only colour it kept. It's lovely.", "Essa faixa verde é a única cor que ficou. É linda."),
                ("A berry stone in the ear. From a pigeon. I'm telling Grandma.", "Um caroço na orelha. De um pombo. Vou contar pra Vó."),
                ("The mist keeps the berries and lets the bird keep the habit.", "A névoa fica com as bagas e deixa a ave com a mania."),
                ("Coo coo! It didn't coo back. It spat. Rude.", "Ruu ruu! Ele não fez ruu de volta. Cuspiu. Mal-educado."));

            Page(entries, "enemy_forest_laurel_lizard",
                "The giant lizards of the laurel islands are real, and very old, and slow in the cold and terribly quick in the sun. When the mist rose and the springs dried this one stopped going down to drink and took to the canopy instead, and now it thinks every platform is its own warm stone. It runs at you along the branches. Step aside and let it go past, then turn.",
                "Os lagartos-gigantes das ilhas do louro existem de verdade, são muito velhos, lentos no frio e rapidíssimos no sol. Quando a névoa subiu e as fontes secaram, este parou de descer pra beber e foi morar nas copas, e agora acha que toda plataforma é a pedra quentinha dele. Vem correndo pelos galhos. Saiam do caminho, deixem passar, e virem.",
                ("Fast in a straight line. Sidestep and hit the flank.", "Rápido em linha reta. Desvia e bate no flanco."),
                ("It's thirsty. The springs dried and nobody told it why.", "Está com sede. As fontes secaram e ninguém contou por quê."),
                ("It chased me round a whole tree. Twice.", "Ele me perseguiu em volta de uma árvore inteira. Duas vezes."),
                ("Older than the elves' houses, and still convinced the sun owes it something.", "Mais velho que as casas dos elfos, e ainda convencido de que o sol lhe deve alguma coisa."),
                ("He has a crest! Like a little roof! Hello, sir!", "Ele tem crista! Feito um telhadinho! Olá, senhor!"));

            // -- Jardim Esquecido de Mihrshah (06) --
            Page(entries, "enemy_forest_smoke_peacock",
                "Mihrshah's garden kept peacocks once, when the canals ran and the roses were cut every morning. When the light was taken the peacocks went away, all but one, and that one is mostly smoke now. Only the eyes in its tail are still lit, and when it is cross it shakes them loose and throws them. Keep to the cypresses. And when the canals run again, I think it will remember.",
                "O jardim de Mihrshah tinha pavões, no tempo em que os canais corriam e as rosas eram cortadas toda manhã. Quando levaram a luz os pavões foram embora, menos um, e esse agora é quase todo fumaça. Só os olhos da cauda ainda estão acesos, e quando ele se zanga sacode e joga. Fiquem perto dos ciprestes. E quando os canais correrem de novo, acho que ele vai lembrar.",
                ("The tail eyes fly. Break line of sight behind a cypress.", "Os olhos da cauda voam. Se esconde atrás de um cipreste."),
                ("It stayed when the others left. It's still waiting for the garden.", "Ficou quando os outros foram. Ainda está esperando o jardim."),
                ("It threw its own eye at me. Who does that?", "Jogou o próprio olho em mim. Quem faz isso?"),
                ("Vanity outlives the bird. Instructive.", "A vaidade sobrevive à ave. Instrutivo."),
                ("Its tail is a fan of fireflies! Do it again! No wait, don't!", "A cauda é um leque de vaga-lumes! Faz de novo! Não, espera, não faz!"));

            // -- Dunas que Cantam (10) --
            Page(entries, "enemy_desert_thorn_lizard",
                "There is a lizard in the sands that drinks the dew off its own back, through the grooves between its thorns; your grandfather showed me one on the caravan road and I didn't believe him. The Devouring Fire dried the dunes, and this kind learned to drink the sparks instead. The thorns glow at the tips now. It is quick and low, so watch your ankles, and don't grab it, whatever Joaquim says.",
                "Tem um lagarto nas areias que bebe o orvalho das próprias costas, pelos sulcos entre os espinhos; o avô de vocês me mostrou um na estrada das caravanas e eu não acreditei. O Fogo Devorador secou as dunas, e esse tipo aprendeu a beber faísca no lugar. Os espinhos agora brilham na ponta. É rápido e baixinho, então cuidado com os tornozelos, e não peguem com a mão, diga o Joaquim o que disser.",
                ("Low and fast. Strike down, not across.", "Baixo e rápido. Bate pra baixo, não de lado."),
                ("It learned to drink sparks just to stay alive. Clever thing.", "Aprendeu a beber faísca só pra sobreviver. Espertinho."),
                ("I grabbed it. Don't grab it.", "Eu peguei. Não peguem."),
                ("It turned its armour into a cup. Grandpa would have liked it.", "Fez da armadura um copo. O vô teria gostado."),
                ("It's a pincushion with legs! A shiny one!", "É uma almofada de alfinete com pernas! Brilhante ainda!"));

            // -- Passo do Hahya (14) --
            Page(entries, "enemy_mountain_dimmed_lynx",
                "The lynx of the passes walks on snow feet and you never hear it. This one has a dark patch on its flank where its light went out, and the patch takes in the snow and gives nothing back; that's how you see it coming, a hole in the white. It comes fast down the narrow ways and goes for the legs. Keep your back to the rock and let it come to you.",
                "O lince dos passos anda com pé de neve e nunca se ouve. Este tem uma mancha escura no flanco onde a luz dele apagou, e a mancha engole a neve e não devolve nada; é assim que se vê ele chegando, um buraco no branco. Desce rápido pelos caminhos estreitos e vai nas pernas. Fiquem de costas pra pedra e deixem ele vir.",
                ("Look for the dark patch in the snow. That's it.", "Procura a mancha escura na neve. É ele."),
                ("The patch is where it hurts. I'm sure of it.", "A mancha é onde dói. Tenho certeza."),
                ("It's so quiet! It was behind me the whole time!", "Ele é tão quieto! Estava atrás de mim o tempo todo!"),
                ("A lynx that swallows the light it walks on. The Darkness wastes nothing.", "Um lince que engole a luz em que pisa. A Treva não desperdiça nada."),
                ("Its ears have little paintbrushes on top!", "As orelhas dele têm pincelzinhos em cima!"));

            // -- Abrigos Pintados (15) --
            Page(entries, "enemy_mountain_smudge",
                "In the painted shelters the old people drew the eland running and the hunters after it, red and white on the rock. Something came out of the dark and ate the paintings one colour at a time, and that something is the smudge. You can see what it ate showing through it in streaks. It is quick and greedy and it wants your colours too. Strike it hard and the colours come loose; that is how the walls get theirs back.",
                "Nos abrigos pintados o povo antigo desenhou o eland correndo e os caçadores atrás, vermelho e branco na pedra. Uma coisa saiu do escuro e comeu as pinturas uma cor de cada vez, e essa coisa é o borrão. Dá pra ver o que ele comeu aparecendo em listras. É rápido e guloso e quer as cores de vocês também. Batam com força que as cores soltam; é assim que as paredes recebem as delas de volta.",
                ("It's fast. Hit it while it's still eating.", "É rápido. Bate enquanto ele ainda está comendo."),
                ("Every streak on it is a painting someone made with love.", "Cada listra nele é uma pintura que alguém fez com carinho."),
                ("It tried to eat the red off my shirt!", "Ele tentou comer o vermelho da minha camisa!"),
                ("A thief of colours. The walls are the poorer, and so is the thief.", "Um ladrão de cores. As paredes ficaram mais pobres, e o ladrão também."),
                ("Give back the eland! Give it BACK!", "Devolve o eland! DEVOLVE!"));

            // -- Muralhas de Pedra Seca (16) --
            Page(entries, "enemy_mountain_stone_jackdaw",
                "Jackdaws nest in the drystone walls and take the loose chips to line the nest. That's what they've always done and nobody minded. These have started taking the walls, one stone at a time, and throwing them at whoever comes up the path. They have pale eyes and very good aim. Keep moving along the wall and get close; up close a jackdaw has nothing left to throw.",
                "As gralhas fazem ninho nos muros de pedra seca e pegam as lasquinhas soltas pra forrar o ninho. Sempre fizeram isso e ninguém se importava. Estas começaram a levar os muros, uma pedra de cada vez, e a jogar em quem sobe a trilha. Têm olho claro e mira muito boa. Continuem andando junto do muro e cheguem perto; de perto a gralha não tem mais o que jogar.",
                ("Good aim. Get in close and it's harmless.", "Mira boa. Chega perto e ela não faz nada."),
                ("It's only building a nest. It just doesn't know when to stop.", "Só está fazendo ninho. Só não sabe a hora de parar."),
                ("It threw a rock bigger than its head!", "Jogou uma pedra maior que a cabeça dela!"),
                ("Every wall is a jackdaw's nest waiting to happen.", "Todo muro é um ninho de gralha esperando acontecer."),
                ("Its eyes are like little moons! Grey little moons!", "Os olhos dela são feito luazinhas! Luazinhas cinzas!"));

            // -- Salão de Tages e Terraços de Tufo (20) --
            Page(entries, "enemy_cave_young_ubir",
                "The Tatar women on the caravans used to tell of the ubır, a hungry spirit that hides in a person and goes out at night as a ball of fire. A young one has not learned to hide yet. It goes about the halls as a ball of black hair with an ember for a mouth, very quick and very sure of itself. Don't let it get behind you, and don't let it near your lamp; it is the light it is hungry for.",
                "As tártaras das caravanas contavam do ubır, um espírito faminto que se esconde numa pessoa e sai de noite feito bola de fogo. O jovem ainda não aprendeu a se esconder. Anda pelos salões como uma bola de cabelo preto com uma brasa no lugar da boca, muito rápido e muito cheio de si. Não deixem ele chegar por trás, e não deixem chegar perto da lamparina; é de luz que ele tem fome.",
                ("It's fast. Keep it in front of you.", "É rápido. Deixa ele sempre na frente."),
                ("It's young. It hasn't chosen anyone yet. Let's keep it that way.", "É novinho. Ainda não escolheu ninguém. Vamos manter assim."),
                ("It's a hairball that bites. I'm not touching it again.", "É uma bola de pelo que morde. Não encosto mais."),
                ("Hunger with no one to wear yet. Give it a few years and it will learn manners, or worse.", "Fome ainda sem ninguém pra vestir. Daqui uns anos aprende modos, ou coisa pior."),
                ("It's fluffy! Fluffy and angry! Fluffy and on FIRE!", "Ele é fofinho! Fofinho e bravo! Fofinho e pegando FOGO!"));
        }

        private static void Page(
            List<LocalizationEntry> entries, string enemyId, string loreEn, string lorePt,
            (string en, string pt) maithe, (string en, string pt) maria, (string en, string pt) joaquim,
            (string en, string pt) icaro, (string en, string pt) lavine)
        {
            entries.Add(new LocalizationEntry(BestiaryCatalog.LoreKey(enemyId), loreEn, lorePt));
            (string en, string pt)[] comments = { maithe, maria, joaquim, icaro, lavine };
            for (int i = 0; i < BestiaryTexts.Cousins.Length; i++)
            {
                entries.Add(new LocalizationEntry(BestiaryCatalog.CommentKey(enemyId, BestiaryTexts.Cousins[i]), comments[i].en, comments[i].pt));
            }
        }
    }
}
