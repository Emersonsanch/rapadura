using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts the map sheets asked for (Docs/Mapas100.md 07–25) and that the continuous
    /// worlds and Taa Pwani's quests used to play with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR.
    /// Kept apart like <see cref="RangedBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class OverworldBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_jackal", "enemy_desert_sand_snake", "enemy_desert_crowned_orc",
            "enemy_mountain_rolling_stone", "enemy_mountain_ill_wind_mist", "enemy_mountain_baboon",
            "enemy_cave_ooze", "enemy_cave_dark_crow",
            "enemy_forest_thief_gull", "enemy_forest_coconut_crab",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Areias Flamejantes (Deserto) --
            Page(entries, "enemy_desert_jackal",
                "Jackals walk behind the caravans picking up whatever falls: a date, a crust, a lost sandal. Grandpa Jorge used to say they were the desert's tidy cousins. These ones were dimmed when the Devouring Fire passed over the Caravan Road, and now they run in packs, fast and hungry, snapping at the heels of anyone walking alone. They lose heart when they're hurt and run off. Keep together, and don't chase them into the dunes.",
                "Os chacais andam atrás das caravanas catando o que cai: uma tâmara, uma casca de pão, uma sandália perdida. O Vô Jorge dizia que eles eram os primos arrumadinhos do deserto. Esses aí se apagaram quando o Fogo Devorador passou pela Estrada das Caravanas, e agora correm em bando, ligeiros e famintos, mordiscando o calcanhar de quem anda sozinho. Quando se machucam perdem a coragem e fogem. Fiquem juntos, e não corram atrás deles duna adentro.",
                ("Fast, but they run in a straight line. My arrows like straight lines.", "Rápidos, mas correm em linha reta. Minhas flechas gostam de linha reta."),
                ("They're just hungry. Let's free the light and give them a date afterwards.", "Eles só estão com fome. Vamos soltar a luz e depois dar uma tâmara pra eles."),
                ("Get behind me, Lavine. They go for whoever is alone.", "Fica atrás de mim, Lavine. Eles vão em quem tá sozinho."),
                ("Ears that big must hear everything. I want to build a pair.", "Orelha desse tamanho deve ouvir tudo. Quero construir um par."),
                ("One stole my sandal! Joaquim, he's running away with my sandal!", "Um roubou a minha sandália! Joaquim, ele tá fugindo com a minha sandália!"));

            Page(entries, "enemy_desert_sand_snake",
                "Among the pyramids of the Artificers lives the Sand Snake, the one with two little horns over its eyes. It sleeps buried, with only the eyes outside, and the shadow found it there and drank the warmth from its scales. Now it bites anything that passes, quick as a whip, and leaves its sideways track all over the ruins. Watch its head: when it pulls back, it is about to strike. Step aside and hit the body, never the face.",
                "No meio das pirâmides dos Artífices mora a Cobra-de-Areia, aquela de dois chifrinhos em cima dos olhos. Ela dorme enterrada, só com os olhos de fora, e foi ali que a sombra a encontrou e bebeu o calor das escamas dela. Agora ela morde tudo o que passa, ligeira feito chicote, e deixa o rastro de lado por todas as ruínas. Olhem a cabeça: quando ela recua, vai dar o bote. Saiam de lado e batam no corpo, nunca na cara.",
                ("It pulls its head back before biting. That's my cue.", "Ela puxa a cabeça pra trás antes de morder. Esse é o meu sinal."),
                ("If it bites, come to me right away. Don't be brave about it.", "Se morder, venham na hora. Nada de bancar o valente."),
                ("It bit my shield. My shield says it's fine.", "Ela mordeu o meu escudo. O meu escudo diz que tá tudo bem."),
                ("Sidewinding! It moves sideways to touch less hot sand. Brilliant.", "Ela anda de lado pra encostar menos na areia quente. Genial."),
                ("It has little horns. I'm not scared. I'm just standing behind Maria.", "Ela tem chifrinho. Não tô com medo. Só tô atrás da Maria."));

            Page(entries, "enemy_desert_crowned_orc",
                "The orcs of the Red Drum are good people: loud, proud and kind to children. But Āz, the greedy one, went through the badlands handing out little tin crowns, saying \"everything that shines is yours\". The orcs who put them on forgot their drums and their families and started guarding piles of junk for her. They hit hard and don't run. But when you beat one, the crown falls off, he blinks, scratches his head and walks back home. Don't be cruel: they're not bad, just crowned.",
                "Os orcs do Tambor Rubro são gente boa: barulhentos, orgulhosos e carinhosos com as crianças. Mas a Āz, aquela gulosa, passou pelos ermos distribuindo coroinhas de lata e dizendo \"tudo o que brilha é seu\". Os orcs que puseram a coroa esqueceram os tambores e a família e foram guardar montes de tralha pra ela. Batem forte e não fogem. Mas quando a gente vence um, a coroa cai, ele pisca, coça a cabeça e volta pra casa. Nada de maldade: eles não são maus, só estão coroados.",
                ("Aim for the crown. One good shot and it's over.", "Mirem na coroa. Um tiro bom e acabou."),
                ("He's someone's dad. Gently, please. Just the crown.", "Ele é pai de alguém. Com jeito, por favor. Só a coroa."),
                ("He's big. I'm stubborn. Let's see who gets tired first.", "Ele é grande. Eu sou teimoso. Vamos ver quem cansa primeiro."),
                ("Tin, painted gold. Āz didn't even buy real gold. Cheap.", "Lata pintada de dourado. A Āz nem comprou ouro de verdade. Pão-dura."),
                ("He said sorry after! He said I have a strong punch. I do.", "Ele pediu desculpa depois! Disse que eu tenho soco forte. Eu tenho."));

            // -- Serra de Kaggen (Montanha) --
            Page(entries, "enemy_mountain_rolling_stone",
                "Not every ushabti stands up. The smallest ones the Artificers made could curl into a ball and roll down the slopes carrying messages between the villages. When the shadow came, it trapped a spark inside one of them, and now it rolls up and down the Rondavel Trail knocking over anyone it meets, as if it still had a message and forgot which. It is heavy and slow to stop: let it pass, then hit it from behind while it turns around.",
                "Nem todo ushabti fica em pé. Os menorzinhos que os Artífices fizeram sabiam se enrolar feito bola e descer as encostas levando recado entre as vilas. Quando a sombra chegou, prendeu uma faísca dentro de um deles, e agora ele rola trilha acima e trilha abaixo derrubando quem encontra, como se ainda tivesse um recado e tivesse esquecido qual. É pesado e demora pra parar: deixem ele passar e batam por trás enquanto ele faz a volta.",
                ("Don't stand in front of it. Ever. Trust me.", "Não fiquem na frente dele. Nunca. Confiem em mim."),
                ("Bumps and bruises, I've got you. Just don't make a habit of it.", "Galo e roxo eu curo. Só não acostumem."),
                ("I tried to stop it with the shield. I'm on the ground now.", "Tentei parar ele com o escudo. Agora eu tô no chão."),
                ("A courier stone! If I find out what the message was, I'll deliver it.", "Uma pedra carteira! Se eu descobrir qual era o recado, eu entrego."),
                ("It rolled over my foot and said nothing. Rude stone.", "Rolou no meu pé e nem pediu desculpa. Pedra mal-educada."));

            Page(entries, "enemy_mountain_ill_wind_mist",
                "Up in the Hahya Pass, where the Living Wind should blow clean, a bad wind got stuck between the rocks. It is the Ruinous Wind, the one that only breaks things, and it gathered fog around itself until it had eyes. It floats in the cold, slows down whoever breathes it and pushes travellers toward the cliffs. Maithe feels it from far away. It swells up before it blows: that is when you hit it, hard and all together.",
                "Lá no Passo do Hahya, onde o Vento Vivo devia soprar limpinho, um vento ruim ficou preso entre as pedras. É o Vento Destruidor, aquele que só quebra as coisas, e ele juntou neblina em volta de si até ganhar olhos. Flutua no frio, deixa lento quem respira ele e empurra os viajantes pra beira do barranco. A Maithe sente de longe. Ele incha antes de soprar: é a hora de bater, forte e todo mundo junto.",
                ("It's my wind gone sour. I'll sort it out myself.", "É o meu vento azedado. Deixa que eu resolvo."),
                ("Breathe through your scarf. Cold air makes you cough.", "Respirem pelo cachecol. Ar gelado dá tosse."),
                ("I can't hit fog! ...Oh. I hit the eyes. That works.", "Não dá pra bater em neblina! ...Ah. Bati nos olhos. Funciona."),
                ("A pressure knot. If I had a big enough fan... no, Ícaro, focus.", "Um nó de pressão. Se eu tivesse um ventilador grande... não, Ícaro, foco."),
                ("It looked at me. Fog isn't supposed to look at people.", "Ela olhou pra mim. Neblina não devia olhar pras pessoas."));

            Page(entries, "enemy_mountain_baboon",
                "Baboons live in big families on the rocks of the Kaggen Range, and they are clever, noisy and very nosy. When the shadow passed by, some of them were dimmed and began to steal: food from the shepherds, wool from the pens, shiny things from the travellers' bags. They come running on all fours, bite and run away again, and they always come back for more. Keep your bags closed and your eyes open.",
                "Os babuínos vivem em famílias grandes nas pedras da Serra de Kaggen, e são espertos, barulhentos e muito enxeridos. Quando a sombra passou, alguns se apagaram e deram de roubar: comida dos pastores, lã dos currais, coisa brilhante da sacola dos viajantes. Vêm correndo de quatro, mordem e fogem de novo, e sempre voltam pra buscar mais. Sacola fechada e olho aberto.",
                ("He took my arrow and ran. I have more. He doesn't know that.", "Ele pegou uma flecha minha e correu. Eu tenho mais. Ele não sabe disso."),
                ("Look at his face, he knows he's being naughty. Gently with him.", "Olha a cara dele, ele sabe que tá fazendo arte. Com jeito."),
                ("He tried to take my shield. He couldn't lift it. Ha.", "Ele tentou levar o meu escudo. Não aguentou o peso. Rá."),
                ("He opened my bag's buckle in one second. I need a better buckle.", "Ele abriu a fivela da minha sacola em um segundo. Preciso de uma fivela melhor."),
                ("He made a face at me. I made one back. I won.", "Ele fez careta pra mim. Eu fiz de volta. Eu ganhei."));

            // -- Grutas do Limiar (Cavernas) --
            Page(entries, "enemy_cave_ooze",
                "At the edge of the Threshold Lake the water used to be clear. Then the Mire, the dark side of the Living Water, settled in the mud, and the mud got up and walked. The Cave Ooze swallows every bit of light it finds: crystals, lamp flames, even the shine of lost coins, and you can see them glowing inside it. It is slow and very sturdy. Be patient, don't let it corner you, and every swallowed light comes back out when it melts.",
                "Na beira do Lago do Limiar a água era limpinha. Aí o Lodo, o lado escuro da Água Viva, assentou na lama, e a lama levantou e saiu andando. O Lodo de Gruta engole toda luz que encontra: cristal, chama de lamparina, até o brilho de moeda perdida, e dá pra ver tudo brilhando lá dentro dele. É lento e muito resistente. Paciência, não deixem ele encurralar vocês, e toda luz engolida volta pra fora quando ele derrete.",
                ("Slow target. I just have to not fall asleep.", "Alvo lento. Só preciso não dormir."),
                ("There's light inside it, look! We'll get it all back.", "Tem luz dentro dele, olha! A gente devolve tudo."),
                ("It's the opposite of my water. My water says it's not afraid.", "É o contrário da minha água. A minha água diz que não tem medo."),
                ("Viscosity, high. Speed, low. Smell... let's not talk about the smell.", "Viscosidade alta. Velocidade baixa. Cheiro... melhor não falar do cheiro."),
                ("I stepped in it. My boot is still in there.", "Eu pisei nele. A minha bota ainda tá lá dentro."));

            Page(entries, "enemy_cave_dark_crow",
                "Down in Mahaway's Roost, where the giant used to rest his wings, there were crows that carried news from one lamp to the next. When the Darkness filled the pit, it got into their feathers, and now they glow violet at the tips and the crows throw them like darts from the rope bridges, then laugh that croaking laugh. They see well in the dark, but their feathers fly straight. Keep moving sideways and don't stop on the bridges.",
                "Lá no Ninho de Mahaway, onde o gigante descansava as asas, havia corvos que levavam notícia de uma lâmpada pra outra. Quando a Treva encheu o poço, entrou nas penas deles, e agora as pontas brilham roxo e os corvos atiram essas penas feito dardo lá das pontes de corda, e ainda dão aquela risada rouca. Enxergam bem no escuro, mas a pena deles voa reta. Andem de lado e não parem em cima das pontes.",
                ("He throws feathers, I shoot arrows. Arrows have feathers too. Fair fight.", "Ele atira pena, eu atiro flecha. Flecha também tem pena. Briga justa."),
                ("The violet goes away when the light comes back. I've seen it.", "O roxo some quando a luz volta. Eu já vi."),
                ("He laughed at me. Nobody laughs at me on a bridge.", "Ele riu de mim. Ninguém ri de mim em cima de ponte."),
                ("Corvids are the smartest birds there are. This one's using it for evil.", "Corvo é o pássaro mais esperto que existe. Esse tá usando pro mal."),
                ("I said \"caw\" back. He didn't like it.", "Eu disse \"cróc\" de volta. Ele não gostou."));

            // -- Costa das Monções (Taa Pwani, the late region; listed with the Living Water) --
            Page(entries, "enemy_forest_thief_gull",
                "Gulls follow the dhows of Taa Pwani all the way into the harbour, shouting for fish. These were dimmed by the greed of Āz: now they don't want fish, they want anything that shines, and they tear the sails apart looking for coins sewn into the seams. They dive fast, peck and fly off with whatever they grabbed. Captain Liyongo says they are not wicked, just greedy. Swing when they dive, not before.",
                "As gaivotas seguem os dhows de Taa Pwani até dentro do porto, gritando por peixe. Essas se apagaram com a gula da Āz: agora não querem peixe, querem tudo o que brilha, e rasgam as velas procurando moeda costurada nas bainhas. Mergulham ligeiro, bicam e saem voando com o que pegaram. O Capitão Liyongo diz que elas não são más, só gulosas. Batam quando elas mergulharem, não antes.",
                ("Thief versus archer. The archer always wins.", "Ladra contra arqueira. A arqueira sempre ganha."),
                ("She took a coin from my pocket! Well... she can keep that one.", "Ela tirou uma moeda do meu bolso! Bom... essa ela pode ficar."),
                ("She stole my sandwich. This is personal now.", "Ela roubou o meu sanduíche. Agora é pessoal."),
                ("If she likes shiny things, I'll make a decoy. A shiny decoy.", "Se ela gosta de coisa brilhante, eu faço uma isca. Uma isca brilhante."),
                ("She pooped on Joaquim. Best day of the trip.", "Ela fez cocô no Joaquim. Melhor dia da viagem."));

            Page(entries, "enemy_forest_coconut_crab",
                "The coconut crab is the biggest crab that walks on land, so strong it opens coconuts with its pincers. On the coast of Taa Pwani they lived quietly among the palms and the mangrove roots. The Mire got into the roots, and the crabs got angry and hard as stone: now they cut the roots and chase anyone who comes near. Their shell is thick, so be patient, and keep away from the big pincer, the left one.",
                "O caranguejo-dos-coqueiros é o maior caranguejo que anda em terra, tão forte que abre coco com a pinça. Na costa de Taa Pwani eles viviam quietos entre os coqueiros e as raízes do mangue. O Lodo entrou nas raízes, e os caranguejos ficaram bravos e duros feito pedra: agora cortam as raízes e correm atrás de quem chega perto. A casca é grossa, então paciência, e fiquem longe da pinça grande, a da esquerda.",
                ("Between the shell plates. Small gap, but I see it.", "Entre as placas da casca. Fresta pequena, mas eu vejo."),
                ("Mind your fingers! That pincer opens coconuts.", "Cuidado com os dedos! Essa pinça abre coco."),
                ("Shell against shield. Mine is thicker.", "Casca contra escudo. O meu é mais grosso."),
                ("Asymmetric pincers: one to crush, one to cut. Engineering!", "Pinça assimétrica: uma pra esmagar, outra pra cortar. Engenharia!"),
                ("It walks sideways. So do I now, look.", "Ele anda de lado. Agora eu também, olha."));
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
