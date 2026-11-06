using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58) that the overworld layout
    /// used to play with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="MingchengNartesBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class FarnkeshBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_smoke_goat", "enemy_forest_golden_bee", "enemy_forest_flute_bird", "enemy_forest_vine_fox",
            "enemy_mountain_alpine_chough", "enemy_mountain_takin", "enemy_mountain_blue_sheep", "enemy_mountain_candle_bat",
            "enemy_mountain_rope_ushabti", "enemy_mountain_pallas_cat", "enemy_mountain_storm_eagle", "enemy_mountain_smoke_harpy",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Vale dos Sátiros (listed with the Living Water) --
            Page(entries, "enemy_forest_smoke_goat",
                "The satyrs' goats used to graze in the vines and dance along with the Vale's ball, and Gordias says every one of them had a name. When the shadow came down the Pactolo it got into their fleece, and now their wool is smoke, grey and warm, with embers still glowing in it. They don't remember their names, so they butt anyone who calls them. Watch the head: when it drops, the horns are coming. At night they sleep in a pile and nobody should wake them.",
                "As cabras dos sátiros pastavam nas vinhas e dançavam junto com o baile do Vale, e o Gordias diz que cada uma tinha nome. Quando a sombra desceu pelo Pactolo, ela entrou na lã delas, e agora a lã é fumaça, cinza e quentinha, com brasa ainda acesa dentro. Elas não lembram o próprio nome, então dão cabeçada em quem chama. Olhem a cabeça: quando ela abaixa, os chifres estão vindo. De noite elas dormem amontoadas e ninguém deve acordar.",
                ("The head goes down, I step aside. Every time.", "A cabeça abaixa, eu dou um passo pro lado. Sempre."),
                ("She just wants her name back. Gordias will know it. We'll ask him.", "Ela só quer o nome dela de volta. O Gordias vai saber. A gente pergunta pra ele."),
                ("She butted my shield and my shield smells like a barbecue now.", "Ela deu cabeçada no meu escudo e agora meu escudo tá com cheiro de churrasco."),
                ("Wool that burns but doesn't burn up. That's a heat source that never runs out.", "Lã que queima mas não acaba. Isso é uma fonte de calor que nunca termina."),
                ("Can I hug her when she's asleep? She looks warm.", "Posso abraçar ela quando ela tiver dormindo? Ela parece quentinha."));

            Page(entries, "enemy_forest_golden_bee",
                "You know the tale of King Midas, who wished that everything he touched turned to gold, and then he couldn't even eat? Midas the satyr laughs at it, but the bees of the Vale aren't laughing. Āz the greedy touched them the same way, and now they are made of gold, heavy and shiny, and they fight over every flower because gold never feels it has enough. When one stings, the gold sticks to you and you walk slow. Wash it off in the Pactolo: Living Water takes gold off as easily as mud.",
                "Vocês conhecem a história do rei Midas, que pediu pra tudo que ele tocasse virar ouro, e depois nem conseguia comer? O Midas sátiro ri dessa história, mas as abelhas do Vale não estão rindo. A Āz, aquela gulosa, tocou nelas do mesmo jeito, e agora elas são de ouro, pesadas e brilhantes, e brigam por cada flor porque ouro nunca acha que tem o bastante. Quando uma pica, o ouro gruda e a gente anda devagar. Lavem no Pactolo: Água Viva tira ouro tão fácil quanto tira barro.",
                ("They're slow to turn. Shoot the one in front and the rest bump into it.", "Elas demoram pra virar. Atira na da frente que as outras trombam nela."),
                ("Poor things, gold is so heavy. They must be so tired of flying.", "Coitadas, ouro é tão pesado. Elas devem estar tão cansadas de voar."),
                ("I got stung and now my arm is gold. Is that good? It feels bad.", "Levei uma picada e agora meu braço tá de ouro. Isso é bom? Tá ruim."),
                ("Solid-gold wings and they still fly. Someone explain the lift to me.", "Asa de ouro maciço e ainda voa. Alguém me explica essa sustentação."),
                ("Do they make golden honey? Vó, is golden honey sweet?", "Elas fazem mel de ouro? Vó, mel de ouro é doce?"));

            Page(entries, "enemy_forest_flute_bird",
                "Marsyas, the satyr who plays the double flute, learned his first tunes from these birds: their beaks are hollow like reeds, and when they sing, the whole Vale dances. The shadow blew into their flutes and put out the song. Now they only play one sharp note, and they throw their stiff tail feathers like darts from far away. Get close fast and they lose their aim. If you hum one of Marsyas's tunes, Lavine's way, sometimes one stops and listens.",
                "O Marsyas, o sátiro que toca a flauta dupla, aprendeu as primeiras músicas com esses pássaros: o bico deles é oco feito caniço, e quando eles cantam o Vale inteiro dança. A sombra soprou dentro das flautas deles e apagou a música. Agora eles só tocam uma nota fina, e jogam as penas duras do rabo feito dardo, lá de longe. Cheguem perto rápido que eles perdem a mira. Se vocês cantarolarem uma música do Marsyas, do jeito da Lavine, às vezes um para pra escutar.",
                ("He shoots from far away. So do I. Let's see who blinks first.", "Ele atira de longe. Eu também. Vamos ver quem pisca primeiro."),
                ("He forgot his song. Marsyas can teach him again, I'm sure.", "Ele esqueceu a música dele. O Marsyas ensina de novo, tenho certeza."),
                ("I can't catch him, he's always far. Somebody bring him to me.", "Não consigo pegar ele, ele tá sempre longe. Alguém traz ele pra mim."),
                ("A beak with finger holes. It's a flute that flies. I want to tune it.", "Um bico com furinho de dedo. É uma flauta que voa. Quero afinar ela."),
                ("La-la-laaa... He stopped! Vó, he stopped to listen!", "Lá-lá-lááá... Ele parou! Vó, ele parou pra escutar!"));

            Page(entries, "enemy_forest_vine_fox",
                "The vine foxes are real foxes, the clever red kind, and they've always stolen a few grapes from the satyrs' vines; Midas used to leave the low bunches for them on purpose. The shadow made them want every bunch, every vine, every barrel, and now they snap at anyone who walks between the rows. They are quick and they run off when they are hurt. You can tell where they've been by the purple on their snouts, and by the empty vines.",
                "As raposas da vinha são raposas de verdade, daquelas vermelhas espertas, e sempre roubaram umas uvas das vinhas dos sátiros; o Midas até deixava os cachos de baixo pra elas, de propósito. A sombra fez elas quererem todo cacho, toda vinha, todo barril, e agora elas mordem quem passa entre as fileiras. São rápidas e fogem quando se machucam. Dá pra saber por onde elas passaram pelo roxo no focinho, e pelas vinhas vazias.",
                ("Purple snout, empty vine. She went that way.", "Focinho roxo, vinha vazia. Ela foi por ali."),
                ("Midas left grapes for her. She only forgot that sharing is nicer.", "O Midas deixava uva pra ela. Ela só esqueceu que dividir é mais gostoso."),
                ("She stole my snack. From my pocket. While I was fighting.", "Ela roubou meu lanche. Do meu bolso. Enquanto eu lutava."),
                ("If she runs away hurt, she'll come back. I'm setting a grape trap.", "Se ela foge machucada, ela volta. Vou montar uma armadilha de uva."),
                ("Her mouth is all purple! She looks like me after Vó's jabuticaba.", "A boca dela tá toda roxa! Parece eu depois da jabuticaba da Vó."));

            // -- Picos de Farnkesh (listed with the Mountain) --
            Page(entries, "enemy_mountain_alpine_chough",
                "Alpine choughs are real birds of the high peaks, black with a yellow beak and red legs, and they ride the wind better than anything that has wings. Shad-Ohrmizd says they used to ring the wind bells of Farnkesh with their beaks just for fun. The shadow made them greedy for shiny things: now they dive at bells, buckles and buttons, and at whoever wears them. Fast and noisy. Hold still when they come in a flock: they go for whatever moves.",
                "A gralha-alpina é um pássaro de verdade dos picos altos, preta, de bico amarelo e perna vermelha, e ela anda no vento melhor que qualquer coisa que tem asa. O Shad-Ohrmizd conta que elas tocavam os sinos de vento de Farnkesh com o bico, só de brincadeira. A sombra deixou elas gulosas por coisa brilhante: agora elas mergulham em sino, fivela e botão, e em quem estiver usando. Rápidas e barulhentas. Fiquem paradas quando vier o bando: elas vão no que se mexe.",
                ("The wind here is alive. I can feel where she'll dive.", "O vento aqui é vivo. Eu sinto onde ela vai mergulhar."),
                ("She just wanted to ring the bells again. Let's let her, after.", "Ela só queria tocar os sinos de novo. Depois a gente deixa."),
                ("She pulled a button off my coat. It was my favourite button.", "Ela arrancou um botão do meu casaco. Era meu botão preferido."),
                ("Red legs, yellow beak, perfect aerodynamics. Nature showing off.", "Perna vermelha, bico amarelo, aerodinâmica perfeita. A natureza se exibindo."),
                ("She took my shiny hair clip! Give it back, gralha!", "Ela pegou minha presilha brilhante! Devolve, gralha!"));

            Page(entries, "enemy_mountain_takin",
                "The takin is a real animal of the high mountains, a big golden cousin of the goats with a heavy curved nose, and the people of Farnkesh drink its milk and weave its wool into the ropes of Sisin's bridges. The shadow greyed its gold and made it think every bridge is a trap. Now the takins stand on the stairways and charge anyone climbing up. They are strong, slow to turn and very hard to stop. Let it charge past and strike its side: on the stairs, it can't turn quickly.",
                "O takin é um bicho de verdade das montanhas altas, um primo grande e dourado das cabras, de nariz curvo e pesado, e o povo de Farnkesh bebe o leite e tece a lã dele nas cordas das pontes do Sisin. A sombra acinzentou o dourado e fez ele achar que toda ponte é armadilha. Agora os takins ficam nas escadarias e dão carga em quem sobe. São fortes, demoram pra virar e são muito difíceis de parar. Deixem ele passar na carga e batam do lado: na escada ele não consegue virar rápido.",
                ("Big, slow to turn. I'll shoot from the side and keep moving.", "Grande, vira devagar. Eu atiro de lado e continuo andando."),
                ("Sisin's ropes are made from his wool. He helps us cross without knowing.", "As cordas do Sisin são feitas da lã dele. Ele ajuda a gente a atravessar sem saber."),
                ("I stood in front of him. Don't stand in front of him.", "Eu fiquei na frente dele. Não fiquem na frente dele."),
                ("A nose that size is for warming the air up here. Clever animal.", "Um nariz desse tamanho é pra esquentar o ar aqui em cima. Bicho esperto."),
                ("He looks like a giant golden sofa. A grumpy sofa.", "Ele parece um sofá dourado gigante. Um sofá emburrado."));

            Page(entries, "enemy_mountain_blue_sheep",
                "The bharal, the blue sheep, is a real animal: grey-blue like slate, with a black stripe down its flank so it disappears on the cliffs, and it runs on rock faces where nothing else can stand. Tadi says they used to show the people of Farnkesh the safe ways up the pinnacles. The shadow made them afraid of everyone, so now they butt first and look afterwards. Quick and nimble. If you lose one on the rocks, look for the stripe, not the sheep.",
                "O bharal, a cabra-azul, é um bicho de verdade: cinza-azulado feito ardósia, com uma listra preta no flanco pra sumir no paredão, e corre em parede de pedra onde nada mais fica em pé. A Tadi conta que eles mostravam pro povo de Farnkesh os caminhos seguros pra subir os pináculos. A sombra fez eles terem medo de todo mundo, então agora eles dão cabeçada primeiro e olham depois. Rápidos e ágeis. Se perderem um nas pedras, procurem a listra, não a cabra.",
                ("Stripe, there. She's not hiding from me.", "A listra, ali. Ela não se esconde de mim."),
                ("She used to show the safe paths. When she calms down, she'll show us.", "Ela mostrava os caminhos seguros. Quando ela se acalmar, ela mostra pra gente."),
                ("She ran up a wall. A WALL. I can't even climb stairs fast.", "Ela subiu uma parede. Uma PAREDE. Eu nem subo escada rápido."),
                ("Rubber hooves with a hard rim. Better grip than my boots.", "Casco de borracha com borda dura. Agarra melhor que minha bota."),
                ("She's not blue. Vó, why is she called blue? She's grey!", "Ela não é azul. Vó, por que chama azul? Ela é cinza!"));

            Page(entries, "enemy_mountain_candle_bat",
                "Every night the people of Farnkesh light candles in the lanterns of the wind bridges so no one gets lost between the pinnacles. The bats of the bridges stole one candle, then another, and Āz liked that: now they each wear a stolen candle stuck to their heads with its own wax, and they fight anyone who comes near their light. At night there are twice as many. Watch the flame: it flares right before the bite, and that is your warning.",
                "Toda noite o povo de Farnkesh acende vela nas lanternas das pontes de vento, pra ninguém se perder entre os pináculos. Os morcegos das pontes roubaram uma vela, depois outra, e a Āz gostou: agora cada um usa uma vela roubada grudada na cabeça com a própria cera, e briga com quem chega perto da luz dele. De noite tem o dobro. Olhem a chama: ela cresce bem antes da mordida, e esse é o aviso de vocês.",
                ("He carries his own light. Easiest target in the sky.", "Ele carrega a própria luz. Alvo mais fácil do céu."),
                ("The candles belong to the bridges. We'll light them again together.", "As velas são das pontes. A gente acende de novo, junto."),
                ("The wax dripped on my hair. Hot wax. On my hair.", "A cera pingou no meu cabelo. Cera quente. No meu cabelo."),
                ("A flying candle holder. If I put a reflector on it, it lights the whole bridge.", "Um castiçal voador. Se eu puser um refletor, ilumina a ponte inteira."),
                ("Make a wish and blow! ...He didn't like that.", "Faz um pedido e assopra! ...Ele não gostou."));

            Page(entries, "enemy_mountain_rope_ushabti",
                "Long ago in Egypt people made little figures called ushabti, servants to do the field work for them in the other world. Sisin learned the trick and knotted his own out of rope and clay, to haul the baskets and mend the bridges while he rests. They were helpers, nothing more. The shadow untied their orders: now they still work, but they work against everyone, cutting ropes and knocking travellers off the bridges with their hoes. Heavy and stubborn. When one breaks, the knot inside lets go and it lies down to rest at last.",
                "Faz muito tempo, no Egito, o povo fazia umas figurinhas chamadas ushabti, servos pra fazer o trabalho do campo por eles no outro mundo. O Sisin aprendeu o jeito e amarrou os dele de corda e barro, pra puxar os cestos e consertar as pontes enquanto ele descansa. Eram ajudantes, nada mais. A sombra desamarrou as ordens deles: agora eles ainda trabalham, mas trabalham contra todo mundo, cortando corda e derrubando viajante das pontes com a enxada. Pesados e teimosos. Quando um quebra, o nó lá de dentro se solta e ele deita pra descansar, finalmente.",
                ("Both arms up means the hoe is coming down. Move.", "Os dois braços pra cima quer dizer que a enxada vai descer. Saiam."),
                ("He works so hard. He deserves a rest. We'll give it to him.", "Ele trabalha tanto. Ele merece descansar. A gente dá esse descanso."),
                ("He hit my shield with a hoe. A HOE. Like I'm a potato patch.", "Ele bateu no meu escudo com uma enxada. Uma ENXADA. Como se eu fosse uma roça de batata."),
                ("Rope muscles and clay bones. Sisin built a robot before me. Unfair.", "Músculo de corda e osso de barro. O Sisin fez um robô antes de mim. Injusto."),
                ("He's like a doll Vó made with string. A big angry doll.", "Ele parece uma boneca que a Vó fez de barbante. Uma boneca grande e brava."));

            Page(entries, "enemy_mountain_pallas_cat",
                "The Pallas's cat, the manul, is a real wild cat of the high cold places, as small as a house cat but with so much fur it looks like a cushion with an angry face. The hermits of the hanging hermitages love them because they are quiet, and quiet is what the hermitages are for. The shadow left them jumpy: the smallest noise and they pounce. They are fast and they bite. Walk slowly near the hermitages and they may not even notice you.",
                "O gato-de-pallas, o manul, é um gato selvagem de verdade dos lugares altos e frios, do tamanho de um gato de casa mas com tanto pelo que parece uma almofada de cara brava. Os eremitas das ermidas suspensas gostam muito deles porque eles são quietos, e quieto é o que as ermidas querem ser. A sombra deixou eles sobressaltados: o menor barulho e eles pulam. São rápidos e mordem. Andem devagar perto das ermidas que talvez eles nem percebam vocês.",
                ("Quiet steps. He hears more than he sees.", "Passo leve. Ele escuta mais do que enxerga."),
                ("He only wants silence. So do the hermits. We can give them both.", "Ele só quer silêncio. Os eremitas também. A gente consegue dar pros dois."),
                ("He's small. He's angry. I respect that.", "Ele é pequeno. Ele é bravo. Eu respeito isso."),
                ("Round pupils, not slits. He hunts by day. Interesting.", "Pupila redonda, não fininha. Ele caça de dia. Interessante."),
                ("He's so fluffy! Why is he so grumpy if he's so fluffy?", "Ele é tão fofinho! Por que ele é tão emburrado se ele é tão fofinho?"));

            Page(entries, "enemy_mountain_storm_eagle",
                "The eagles of the Gorge of the Ays fly in the worst wind in the world, and the Armenian tales say the ays, the bad wind spirits, envied them for it. When the shadow gave the ays their strength, the eagles flew into the storm to fight them and came back with lightning stuck in their wing tips. Now they guard the gorge from far above and throw feathers as hard as stone at anything that moves. Hide behind the shelter stones and wait for the glide: when the wings go still, it is about to throw.",
                "As águias da Garganta dos Ays voam no pior vento do mundo, e os contos da Armênia dizem que os ays, os espíritos do vento ruim, tinham inveja delas por isso. Quando a sombra deu força pros ays, as águias entraram na tempestade pra brigar com eles e voltaram com raio preso na ponta das asas. Agora elas guardam a garganta lá de cima e jogam penas duras feito pedra em tudo que se mexe. Escondam atrás das pedras-abrigo e esperem o planeio: quando as asas param, ela vai jogar.",
                ("She waits for the glide to throw. So I wait for the glide to shoot.", "Ela espera o planeio pra jogar. Então eu espero o planeio pra atirar."),
                ("She fought the bad wind to protect the gorge. She's brave. She's just confused now.", "Ela brigou com o vento ruim pra proteger a garganta. Ela é corajosa. Só tá confusa agora."),
                ("I got hit by a feather and it hurt like a rock. Feathers shouldn't do that.", "Levei uma pena e doeu feito pedra. Pena não devia fazer isso."),
                ("Lightning in the wing tips. Maithe, if I catch one feather, I'll charge my drone forever.", "Raio na ponta da asa. Maithe, se eu pegar uma pena, carrego meu drone pra sempre."),
                ("Her wings go zap! Like Vô Jorge's radio when it rains.", "As asas dela fazem zap! Igual o rádio do Vô Jorge quando chove."));

            Page(entries, "enemy_mountain_smoke_harpy",
                "In the old Greek tales the harpies were the snatchers, winged things of the storm that grabbed and carried off. Up on the cloud islands around the Huma's nest they are only beasts, big birds of prey made half of smoke, with a pale face like a mask. The Destroying Wind has them in its grip now: they snatch whatever they can from the nest and from travellers, and they are very fast. Never fight one at the edge of an island. Step to the middle and make it come to you.",
                "Nos contos antigos da Grécia as harpias eram as que agarram, bichos de asa da tempestade que pegavam e levavam embora. Lá nas ilhas de nuvem em volta do ninho da Huma elas são só bichos, aves de rapina grandes feitas meio de fumaça, com uma cara pálida feito máscara. O Vento Destruidor está com elas na mão agora: elas agarram o que podem do ninho e dos viajantes, e são muito rápidas. Nunca briguem com uma na beira da ilha. Vão pro meio e façam ela vir até vocês.",
                ("Middle of the island. Let her come. Then she's mine.", "Meio da ilha. Deixa ela vir. Aí ela é minha."),
                ("The wind is holding her. When it lets go, she'll just be a bird.", "O vento tá segurando ela. Quando ele soltar, ela vai ser só um pássaro."),
                ("She tried to carry me off. I'm too heavy. Ha. Take that.", "Ela tentou me levar embora. Eu sou pesado demais. Rá. Toma."),
                ("Feathers made of smoke still push air. I need to study that.", "Pena de fumaça ainda empurra o ar. Preciso estudar isso."),
                ("Her face is like a white mask. Is she pretending to be someone?", "A cara dela parece uma máscara branca. Ela tá fingindo ser alguém?"));
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
