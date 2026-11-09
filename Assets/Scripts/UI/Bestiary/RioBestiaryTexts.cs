using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Rio das Doze Horas (Docs/Mapas100.md 71–74) that the overworld layout used to play
    /// with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="HinokuraBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Only the beasts of the Kemet tales, never its gods:
    /// the scarab is a beetle and the serpents are snakes, as the sheets say.
    /// </summary>
    public static class RioBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_dark_river_crocodile", "enemy_forest_dimmed_ibis", "enemy_forest_shadow_scarab", "enemy_forest_reed_snare",
            "enemy_forest_shadow_locust", "enemy_forest_mud_hippo", "enemy_forest_reed_snake", "enemy_forest_three_headed_serpent",
            "enemy_forest_black_scorpion", "enemy_forest_sand_ushabti", "enemy_forest_shadow_biter", "enemy_forest_creeping_dark",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Cais de Mesektet and its margins (71) --
            Page(entries, "enemy_forest_dark_river_crocodile",
                "The river of the night has crocodiles like every river, and for a long time they only watched the barque go by, each one lying in its own hour. Sinuhe says that when the shadow reached the water they stopped watching and started waiting, sure that whatever floats belongs to whoever is under it. They bask with the mouth a little open. When it opens a lot, you are standing too close and too far in front. Go round by the side: a crocodile does everything forwards.",
                "O rio da noite tem crocodilo como todo rio, e por muito tempo eles só olhavam a barca passar, cada um deitado na sua hora. O Sinuhe conta que quando a sombra chegou na água eles pararam de olhar e começaram a esperar, certos de que tudo que boia é de quem está embaixo. Eles ficam com a boca um pouquinho aberta. Quando abrir muito, vocês estão perto demais e na frente demais. Deem a volta pelo lado: crocodilo faz tudo pra frente.",
                ("It only bites forward. Stay at the shoulder.", "Só morde pra frente. Fiquem no ombro."),
                ("He's been waiting so long he forgot what for.", "Ele espera faz tanto tempo que esqueceu o quê."),
                ("I thought it was a log. I sat on it. Don't tell Maithe.", "Achei que era um tronco. Sentei. Não conta pra Maithe."),
                ("Eyes on top of the skull: he sees the barque without lifting his head.", "Olho em cima do crânio: ele vê a barca sem levantar a cabeça."),
                ("His smile has so many teeth! Does he brush them?", "O sorriso dele tem tanto dente! Ele escova?"));

            Page(entries, "enemy_forest_dimmed_ibis",
                "The ibis walked the banks counting: fish, frogs, the hours of the night. Djedi swears they were never wrong. The shadow took the white from their feathers and left the ash, and now they don't count anymore, they just take. They come in fast, one peck and gone, and they go for whatever shines. Keep your lamp low and your eyes up, and don't bother chasing one. It will come back by itself. That is the whole trouble with them.",
                "A íbis andava pela margem contando: peixe, sapo, as horas da noite. O Djedi jura que elas nunca erraram. A sombra tirou o branco das penas e deixou a cinza, e agora elas não contam mais, só pegam. Vêm rápido, uma bicada e foi, e vão no que brilha. Deixem a lamparina baixa e os olhos no alto, e nem adianta correr atrás. Ela volta sozinha. O problema dela é justamente esse.",
                ("Fast in, fast out. Hit it as it comes.", "Vem rápido, sai rápido. Acerta na vinda."),
                ("One feather stayed gold. I think she keeps it on purpose.", "Uma pena ficou dourada. Acho que ela guarda de propósito."),
                ("She took my spoon. Why would a bird need a spoon?", "Ela levou minha colher. Pra que passarinho quer colher?"),
                ("That curved bill probes mud by touch. She doesn't even need to look.", "Esse bico curvo procura na lama pelo tato. Ela nem precisa olhar."),
                ("Her nose is so long it goes to the next room!", "O nariz dela é tão comprido que chega no outro quarto!"));

            Page(entries, "enemy_forest_shadow_scarab",
                "Dung beetles have always rolled their little balls across the sand and steered by the stars, and I find that the most admirable thing a beetle ever did. The ones on the river got the shadow in them and began to want a bigger ball, then a bigger one, and now they walk along with a dim gold sun on the forehead that they found somewhere and will not put down. They come in numbers. Step on the ones at the edge first, and don't let them go round you.",
                "Besouro de esterco sempre rolou a bolinha dele pela areia e se guiou pelas estrelas, e eu acho isso a coisa mais admirável que um besouro já fez. Os do rio pegaram a sombra e começaram a querer uma bola maior, e depois outra maior, e agora andam com um solzinho dourado apagado na testa, que acharam em algum lugar e não largam. Vêm em quantidade. Pisem primeiro nos da beirada, e não deixem que eles deem a volta em vocês.",
                ("Thin the edges first. Don't get ringed.", "Tira os da beirada primeiro. Não deixa fechar a roda."),
                ("All that carrying, and nobody ever told him he could stop.", "Tanto carregar, e ninguém nunca disse que ele podia parar."),
                ("I tried to help one roll. He bit me. It was a nice ball.", "Tentei ajudar um a rolar. Ele me mordeu. Era uma bola bonita."),
                ("Real ones navigate by the Milky Way. On a river that is always night, these have it easy.", "Os de verdade se guiam pela Via Láctea. Num rio que é sempre noite, esses têm vida fácil."),
                ("He's got a tiny sun on his head! Is it hot? Can I hold it?", "Ele tem um solzinho na cabeça! É quente? Posso segurar?"));

            Page(entries, "enemy_forest_reed_snare",
                "They used to cut the papyrus and tie it in bundles here, thousands of bundles, for the boats and the mats. Some of the bundles were left on the bank and never fetched, and the shadow found them. Now they drift over the shallows on the root they still have, with an arm of plaited reed on each side and a loop at the end, and what they want is to tie something. Anything. When the loop goes up, step away: being tied is exactly as bad as it sounds.",
                "Aqui cortavam o papiro e amarravam em feixes, milhares de feixes, pros barcos e as esteiras. Alguns feixes ficaram na margem e ninguém veio buscar, e a sombra achou. Agora eles boiam em cima do raso na raiz que sobrou, com um braço de junco trançado de cada lado e um laço na ponta, e o que eles querem é amarrar alguma coisa. Qualquer coisa. Quando o laço subir, saiam de perto: ficar amarrado é tão ruim quanto parece.",
                ("Watch the loop. Step out when it rises.", "Olha o laço. Sai quando ele subir."),
                ("Nobody came back for him. So he's trying to keep someone.", "Ninguém voltou pra buscar ele. Então ele tenta segurar alguém."),
                ("I got tied to my own backpack. Took me an hour.", "Fui amarrado na minha própria mochila. Levei uma hora."),
                ("Cellulose fibres under tension. Technically it's a very angry rope.", "Fibra de celulose sob tensão. Tecnicamente é uma corda muito brava."),
                ("It's a broom that wants a hug!", "É uma vassoura que quer um abraço!"));

            // -- Campos de Juncos (72) --
            Page(entries, "enemy_forest_shadow_locust",
                "One locust eats a mouthful and goes home. That's a locust. When they crowd together and the shadow gets among them, each one forgets it had a home, and then the fields of light go down in an afternoon, the wheat that shines and everything planted next to it. Khunanup says that hunger is the only thing that ever talked him into silence. Don't fight the whole cloud: stand where the wheat is thin and let them come to you a few at a time.",
                "Um gafanhoto come um bocado e vai pra casa. Isso é um gafanhoto. Quando eles se juntam e a sombra entra no meio, cada um esquece que tinha casa, e aí os campos de luz vão embora numa tarde, o trigo que brilha e tudo que estiver plantado do lado. O Khunanup diz que a fome foi a única coisa que já convenceu ele a calar a boca. Não briguem com a nuvem inteira: fiquem onde o trigo é ralo e deixem eles virem de poucos.",
                ("Fight on thin ground. Few at a time.", "Briga onde é ralo. De poucos em poucos."),
                ("Each one is just hungry. There are simply too many hungers.", "Cada um só tá com fome. É que tem fome demais."),
                ("One went in my ear. It came out still eating.", "Um entrou no meu ouvido. Saiu ainda comendo."),
                ("Crowding really does change them. Same insect, different behaviour: density is the switch.", "Aglomeração muda eles de verdade. Mesmo inseto, outro comportamento: a densidade é o interruptor."),
                ("He jumped on my head and said nothing! Rude!", "Ele pulou na minha cabeça e não disse nada! Sem educação!"));

            Page(entries, "enemy_forest_mud_hippo",
                "People laugh at a hippo until they have stood next to one. They were the great mowers of the canals, up at night to graze and back in the water by morning, and the farmers of the reed fields put up with them because the channels stayed open. The shadow told them the canals were theirs and that the farmers were in the way. Now they stand in the middle of the path covered in mud and reeds and they do not move for anyone. If the mouth opens, you have about as long as it takes to say so.",
                "As pessoas riem de hipopótamo até ficarem do lado de um. Eles eram os grandes cortadores dos canais, de noite saíam pra pastar e de manhã voltavam pra água, e os lavradores dos campos de juncos aguentavam porque os canais ficavam abertos. A sombra disse que os canais eram deles e que os lavradores estavam atrapalhando. Agora ficam parados no meio do caminho cobertos de barro e junco e não saem pra ninguém. Se a boca abrir, vocês têm o tempo de dizer isso.",
                ("He doesn't dodge. Neither should your plan.", "Ele não desvia. Seu plano também não deve."),
                ("So much mud on him. Like he's trying to hide from himself.", "Tanto barro nele. Parece que tá tentando se esconder dele mesmo."),
                ("He yawned at me. I gave him everything in my pockets.", "Ele bocejou pra mim. Dei tudo que tinha no bolso."),
                ("That yawn is a threat display. It is also about a metre and a half wide.", "Esse bocejo é ameaça. Também tem um metro e meio de largura."),
                ("He has a garden on his back! Who waters it?", "Ele tem uma horta nas costas! Quem rega?"));

            Page(entries, "enemy_forest_reed_snake",
                "Reed snakes never hurt anybody who looked where they were going, and they kept the fields free of mice for a thousand years. They are the colour of the stalks and have the patience of the stalks. The shadow took the patience and left the colour. So now you have a thing that looks like the plant next to you and bites first and thinks never. In the reed beds, walk slowly and watch for the reed that isn't swaying with the others.",
                "A cobra do junco nunca fez mal a quem olhava por onde andava, e manteve os campos sem rato por mil anos. Ela tem a cor do talo e a paciência do talo. A sombra tirou a paciência e deixou a cor. Então agora vocês têm uma coisa que parece a planta do lado e que morde primeiro e não pensa nunca. Nos canteiros, andem devagar e procurem o junco que não está balançando junto com os outros.",
                ("One reed isn't moving with the wind. That one.", "Um junco não balança com o vento. Aquele."),
                ("She kept the mice away for so long. She must be so tired of being stepped on.", "Ela espantou rato por tanto tempo. Deve estar tão cansada de ser pisada."),
                ("I grabbed the wrong reed. Twice.", "Peguei o junco errado. Duas vezes."),
                ("Countershading plus stillness. Remove the stillness and it's just a snake that's very sorry for you.", "Camuflagem e imobilidade. Tira a imobilidade e sobra uma cobra com muita pena de você."),
                ("She looks like a shoelace! A mean shoelace!", "Ela parece um cadarço! Um cadarço malvado!"));

            // -- Areal Escondido (73) --
            Page(entries, "enemy_forest_three_headed_serpent",
                "In the fourth hour the sand is so bad that the barque has to crawl, and the tales say serpents with more heads than sense pulled it along on a rope. I don't know about the rope. I do know that three heads do not agree on anything, and when the shadow told each one that it alone was pulling, they stopped pulling altogether and took to arguing with whoever walks past. Go at the one looking away. There is always one looking away.",
                "Na quarta hora a areia é tão ruim que a barca tem que se arrastar, e os contos dizem que serpentes com mais cabeça do que juízo puxavam ela numa corda. Da corda eu não sei. Sei que três cabeças não concordam em nada, e quando a sombra disse pra cada uma que só ela estava puxando, elas pararam de puxar e passaram a discutir com quem passa. Vão na que está olhando pro outro lado. Sempre tem uma olhando pro outro lado.",
                ("Three heads, one body. Hit the body.", "Três cabeças, um corpo. Bate no corpo."),
                ("Three heads and they still can't talk it out.", "Três cabeças e mesmo assim não conseguem conversar."),
                ("I said hello to the middle one. The other two got jealous.", "Dei oi pra do meio. As outras duas ficaram com ciúme."),
                ("Bilateral nerve cords in three necks, shared gut. They must really fight about dinner.", "Três pescoços, um intestino só. A briga no jantar deve ser feia."),
                ("Which one is the boss? I bet it's the left one.", "Qual delas manda? Aposto que é a da esquerda."));

            Page(entries, "enemy_forest_black_scorpion",
                "A scorpion is a very honest animal. It tells you exactly what it is going to do and then it does it. The ones in the hidden sands are black as the inside of a pot, with a band of gold on each joint of the tail, and they can run. That is what the shadow gave them: the old scorpion's patience turned into speed. Watch the tail when it comes up over the back, and if you are near a door of stone, put the door between you.",
                "Escorpião é um bicho muito honesto. Ele diz exatamente o que vai fazer e depois faz. Os do Areal Escondido são pretos feito fundo de panela, com uma faixa de ouro em cada junta do rabo, e correm. Foi o que a sombra deu pra eles: a paciência do escorpião antigo virada em pressa. Olhem o rabo quando ele subir por cima das costas, e se tiver uma porta de pedra por perto, ponham a porta entre vocês.",
                ("Tail up means now. Move.", "Rabo em cima é agora. Sai."),
                ("He tells you first. That's more than most do.", "Ele avisa antes. É mais do que a maioria faz."),
                ("It went in my boot and I had boots on.", "Entrou na minha bota e eu tava de bota."),
                ("Scorpions fluoresce under moonlight. On this river that's every moment of their lives.", "Escorpião brilha sob a lua. Nesse rio é o tempo todo."),
                ("He's got gold bracelets on his tail! Fancy!", "Ele tem pulseira de ouro no rabo! Chique!"));

            Page(entries, "enemy_forest_sand_ushabti",
                "Ushabti are the little servants that were put in the tomb to do the work of the fields in the other world, so that nobody would have to. A figure of sand is the cheapest of all, and I have always felt sorry for the cheap ones. The shadow found these still waiting at the closed doors for somebody to give the order, and it told them there was no field and no one coming, only the door. They hit very slowly and very hard. They crumble at the knees, if you go on hitting them there.",
                "Ushabti são aqueles servinhos que punham na tumba pra fazer o trabalho do campo no outro mundo, pra ninguém ter que fazer. A figura de areia é a mais barata de todas, e eu sempre tive pena das baratas. A sombra achou essas ainda esperando nas portas fechadas alguém dar a ordem, e disse pra elas que não tinha campo nenhum e não vinha ninguém, só a porta. Batem bem devagar e bem forte. Desmancham nos joelhos, se vocês insistirem ali.",
                ("Slow arms. Knees give first.", "Braço lento. O joelho cede primeiro."),
                ("Still at the door. Still waiting to be told.", "Ainda na porta. Ainda esperando mandarem."),
                ("I told him he could go home. He hit me with the hoe.", "Falei que ele podia ir pra casa. Ele me bateu com a enxada."),
                ("Packed sand with almost no binder. It shouldn't hold a shape, and it knows.", "Areia batida quase sem liga. Não devia parar em pé, e ele sabe."),
                ("He's dropping sand! Is he a sandbox? Can I play in him?", "Ele tá derrubando areia! Ele é caixa de areia? Posso brincar nele?"));

            // -- Anéis de Mehen (74) --
            Page(entries, "enemy_forest_shadow_biter",
                "Mehen lies coiled in a great ring around the light, and each night the dark tries to get its teeth in. Those teeth have to go somewhere when the night is over, and some of them stay. Pieces of the dark, bitten off and left to run about in the rings: no coat, no bones, only the mouth and the shape of a dog that a mouth would need. You can see the seam down the back where it tore off. They are very quick, and they are afraid of nothing, because nobody ever told them there was anything else.",
                "A Mehen fica enrolada num anel enorme em volta da luz, e toda noite a treva tenta enfiar os dentes. Esses dentes têm que ir pra algum lugar quando a noite acaba, e alguns ficam. Pedaços de treva, arrancados e largados correndo pelos anéis: sem pelo, sem osso, só a boca e o jeito de cachorro que uma boca precisa ter. Dá pra ver a costura nas costas onde rasgou. São muito rápidos, e não têm medo de nada, porque ninguém nunca contou que existia outra coisa.",
                ("Don't chase it. It comes to you.", "Não corre atrás. Ele vem até você."),
                ("It doesn't know it's only a piece of something.", "Ele não sabe que é só um pedaço de alguma coisa."),
                ("It bit my shadow. It hurt. Why did it hurt?", "Mordeu minha sombra. Doeu. Por que doeu?"),
                ("No bones, no tissue, and it still has a bite radius. I'm going to need a new notebook.", "Sem osso, sem tecido, e ainda tem raio de mordida. Vou precisar de outro caderno."),
                ("It's a very bad dog! The worst dog!", "É um cachorro muito mau! O pior cachorro!"));

            Page(entries, "enemy_forest_creeping_dark",
                "It is what's left on the coils in the morning, when the night goes back and doesn't take everything with it. It's very low and very wide and it moves by feeling along the scales with long arms, and wherever it stops, it has bitten off a little of the gold, which you can see sinking slowly inside it. Don't let it reach your feet, it's quicker there than anywhere else. It's the greediest thing on the river, and it is greedy for the smallest things. That's how you know it's the Āz.",
                "É o que sobra em cima dos anéis de manhã, quando a noite volta e não leva tudo. É bem baixinha e bem larga, e anda tateando as escamas com uns braços compridos, e onde ela para, ela já arrancou um pouquinho do ouro, que dá pra ver afundando devagar dentro dela. Não deixem chegar nos pés, que ali ela é mais rápida. É a coisa mais gulosa do rio, e é gulosa das coisas menores. É assim que se reconhece a Āz.",
                ("Keep your feet moving. Burn it from the edge.", "Pés em movimento. Vai pela beirada."),
                ("It only wants a little. It always only wants a little.", "Ela só quer um pouquinho. Sempre só quer um pouquinho."),
                ("I thought it was a shadow. It was a shadow. Worse.", "Achei que era uma sombra. Era uma sombra. Pior."),
                ("Each gold flake inside is dimmer than the last. It's digesting light. Slowly.", "Cada lasca de ouro lá dentro tá mais fraca que a anterior. Ela tá digerindo luz. Devagar."),
                ("It's a puddle with arms! Eew! Hi, puddle!", "É uma poça com braço! Eca! Oi, poça!"));
        }

        /// <summary>Lore + one comment per cousin (Maithe, Maria, Joaquim, Ícaro, Lavine, in that order).</summary>
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
