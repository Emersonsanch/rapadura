using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve most-seen roles of Regions 5–16 that the overworld still played with stand-ins after the passes by
    /// region (bible §3.2.1, fifteenth pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="FarStandInBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class WideStandInBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_steppe_vulture", "enemy_mountain_steppe_wolf", "enemy_mountain_grass_storm", "enemy_forest_marsh_wisp",
            "enemy_forest_bull_frog", "enemy_forest_mudskipper", "enemy_desert_broken_mirror", "enemy_mountain_paper_crane",
            "enemy_cave_smoke_locust", "enemy_forest_coral_ushabti", "enemy_desert_confused_spark", "enemy_cave_dark_drop",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Pradaria Ondulada e Mil Cavalos (51) --
            Page(entries, "enemy_desert_steppe_vulture",
                "The vultures of the steppe followed the herds and the horsemen and tidied up after both, and nobody minded them. The shadow came in on the wind over the poppies, and now they follow whoever is still walking. They circle high and throw their hard feathers from up there. Don't stand about in the open counting them. Walk on, and hit the one that comes down to look.",
                "Os abutres da estepe seguiam as manadas e os cavaleiros e limpavam a sujeira dos dois, e ninguém se importava com eles. A sombra chegou no vento por cima das papoulas, e agora eles seguem quem ainda está andando. Rodam lá no alto e jogam as penas duras de lá de cima. Não fiquem parados no descampado contando. Continuem andando, e batam no que descer pra olhar.",
                ("They shoot from high up. Hit the one that dives.", "Atiram lá do alto. Bate no que mergulhar."),
                ("They're only following us because they're used to following someone.", "Só seguem a gente porque estão acostumados a seguir alguém."),
                ("I'm not dead! Go follow someone else!", "Eu não tô morto! Vai seguir outro!"),
                ("A bird that waits for the end of things. The shadow made it impatient.", "Um pássaro que espera o fim das coisas. A sombra deixou ele sem paciência."),
                ("It's got a fluffy white collar! Like Grandpa's old scarf!", "Ele tem uma golinha branca fofa! Igual ao cachecol velho do Vô!"));

            // -- Vau Largo, Estepe de Soslan e Pistas de Corrida (47) --
            Page(entries, "enemy_mountain_steppe_wolf",
                "The steppe wolves used to run beside Soslan's horses for the joy of running, and they never took a foal; Grandpa Jorge swore he saw them race. The shadow thinned them to the bone and taught them to run at the horses instead. They hunt in threes along the fords and the racing tracks. Keep your back to the water, and don't let them get behind you.",
                "Os lobos da estepe corriam do lado dos cavalos do Soslan pela alegria de correr, e nunca pegaram um potro; o Vô Jorge jurava que viu eles apostando corrida. A sombra afinou eles até o osso e ensinou a correr contra os cavalos. Caçam de três em três pelos vaus e pelas pistas de corrida. Fiquem de costas pra água, e não deixem eles irem pra trás de vocês.",
                ("Fast and in threes. Back to the water.", "Rápidos, de três em três. Costas pra água."),
                ("They used to run for fun. I hope they remember how.", "Eles corriam por diversão. Tomara que lembrem como."),
                ("Race me, then! Come on! ...Not like that!", "Aposta corrida comigo, então! Vem! ...Assim não!"),
                ("A wolf that runs for joy is a rare thing. The shadow takes the rare things first.", "Lobo que corre por alegria é coisa rara. A sombra pega as coisas raras primeiro."),
                ("They have the same grey as the grass! I almost didn't see them!", "Eles têm o mesmo cinza do capim! Quase não vi!"));

            // -- Campos de Papoula e Pradaria dos Mil Cavalos (51) --
            Page(entries, "enemy_mountain_grass_storm",
                "Over the prairie of the thousand horses the wind lifts the dry grass every evening and puts it down again in the morning. Since the shadow, it does not put it down. The grass goes round and round with a spark of the last lightning in the middle, and it wraps round anyone who walks into it. Don't run through it. Step out sideways and wait for it to slow down.",
                "Na pradaria dos mil cavalos o vento levanta o capim seco toda tarde e põe de volta de manhã. Desde a sombra, não põe mais. O capim fica rodando, rodando, com uma faísca do último raio no meio, e enrola quem entra nele. Não atravessem correndo. Saiam de lado e esperem ele diminuir.",
                ("It holds you in place. Step out sideways.", "Prende a gente no lugar. Sai de lado."),
                ("All that grass wants is to lie down. Me too, honestly.", "Todo esse capim só quer deitar. Eu também, sinceramente."),
                ("I'm full of grass. In my ears. EVERYWHERE.", "Tô cheio de capim. No ouvido. EM TODO LUGAR."),
                ("A wind that forgot to stop. Most of the trouble in the world is like that.", "Um vento que esqueceu de parar. A maior parte dos problemas do mundo é assim."),
                ("It's a grass tornado! It's spinning! Wheee!", "É um tornado de capim! Tá girando! Iiiih!"));

            // -- Ilhas de Junco, Pântano Aberto e Palafitas de Tarchon (33) --
            Page(entries, "enemy_forest_marsh_wisp",
                "The fishermen of Tarchon used to catch the marsh lights in little cages of reed, to see by when they went out at night. The Dark got into the cages, and now the lights run about the swamp with their cages still on, and burst against anyone who comes close. They come in a crowd. Don't wade after them in the deep water; let them come to the boards, and put them out one by one.",
                "Os pescadores de Tarchon pegavam os fogos-fátuos do pântano em gaiolinhas de junco, pra enxergar quando saíam de noite. A Treva entrou nas gaiolas, e agora os fogos correm pelo pântano com a gaiola ainda em volta, e estouram em quem chega perto. Vêm em bando. Não entrem na água funda atrás deles; deixem que venham até as tábuas, e apaguem um por um.",
                ("They swarm. Fight them from the boards, not the water.", "Vêm em enxame. Briga nas tábuas, não na água."),
                ("They're still in the cages. Somebody should open them.", "Ainda estão nas gaiolas. Alguém devia abrir."),
                ("It popped in my face. My eyebrows are fine. Probably.", "Estourou na minha cara. Minhas sobrancelhas estão bem. Acho."),
                ("A light kept in a cage learns to hate cages. And then everything else.", "Luz guardada em gaiola aprende a odiar gaiola. Depois todo o resto."),
                ("It's a little green lantern! It's running away!", "É uma lanterninha verde! Tá fugindo!"));

            // -- Brejo das Palmeiras, Charco do Ninki Nanka e Toca da Dragoa (34) --
            Page(entries, "enemy_forest_bull_frog",
                "The bullfrogs of the palm swamp sang the rain in every evening; you could set the supper by them. The mire went into their throats and the song changed. Now it pulls the travellers off the paths and into the mud, and the frog throws all its weight on whoever comes. Watch the throat. When it swells and shines, it is about to jump.",
                "Os sapos-boi do brejo das palmeiras cantavam a chuva toda tarde; dava pra marcar a janta por eles. O lodo entrou na garganta deles e a canção mudou. Agora ela puxa os viajantes pra fora do caminho e pra dentro da lama, e o sapo joga o peso todo em quem chega. Olhem a garganta. Quando ela inchar e brilhar, ele vai pular.",
                ("Heavy. When the throat swells, step aside.", "Pesado. Quando a garganta inchar, sai do lado."),
                ("Their song used to bring the rain. Now it brings people. That's sad.", "A canção trazia a chuva. Agora traz gente. Que triste."),
                ("It jumped on me. A FROG. Sat on me.", "Pulou em mim. Um SAPO. Sentou em mim."),
                ("A frog the size of a stove. The swamp has been eating well.", "Um sapo do tamanho de um fogão. O pântano anda comendo bem."),
                ("Its throat lights up like a balloon! Do it again!", "A garganta dele acende igual balão! Faz de novo!"));

            // -- Lago de Jhilpur e os bancos (31) --
            Page(entries, "enemy_forest_mudskipper",
                "The mudskippers walk on the mud of the lake at low tide, propped on their fins, with their eyes on top of their heads like two little lamps, and they skip from bank to bank. The mire taught them to skip at people. One alone is nothing. They come by the dozen, so stand where the ground is firm and let them come to you.",
                "Os peixes-saltadores andam na lama do lago na maré baixa, apoiados nas barbatanas, com os olhos em cima da cabeça igual duas lamparininhas, e pulam de um banco pro outro. O lodo ensinou eles a pular nas pessoas. Um sozinho não é nada. Vêm às dúzias, então fiquem onde o chão é firme e deixem que venham até vocês.",
                ("Small, but many. Firm ground, and keep swinging.", "Pequenos, mas muitos. Chão firme e continua batendo."),
                ("A fish that learned to walk. Imagine how proud it was.", "Um peixe que aprendeu a andar. Imagina o orgulho dele."),
                ("It slapped me with its tail. A fish slapped me.", "Me deu um tapa com o rabo. Um peixe me deu um tapa."),
                ("Fins as legs, eyes on top. Somebody designed this in a hurry.", "Barbatana de perna, olho em cima. Alguém desenhou isso com pressa."),
                ("Its eyes are on its head like a hat! Ha!", "Os olhos dele ficam em cima da cabeça igual chapéu! Rá!"));

            // -- Jardins em Terraço, Torres Brancas e Colinas dos Espelhos (42) --
            Page(entries, "enemy_desert_broken_mirror",
                "In the terraced gardens they hung bronze mirrors to send the moonlight into the houses at night, so nobody would sleep in the dark. The shadow cracked them, and the pieces kept a bit of the moon and send it at whoever walks the terraces. They are slow, and they shoot from far. Come at them from the side, where the glass can't see you.",
                "Nos jardins em terraço penduravam espelhos de bronze pra mandar o luar pra dentro das casas de noite, pra ninguém dormir no escuro. A sombra rachou eles, e os pedaços guardaram um pouco da lua e mandam ela em quem anda pelos terraços. São lentos e atiram de longe. Cheguem de lado, onde o vidro não vê vocês.",
                ("It shoots straight. Come in from the side.", "Atira reto. Chega pelo lado."),
                ("It was made to keep people out of the dark. It still thinks it is.", "Foi feito pra tirar as pessoas do escuro. Ainda acha que tá fazendo isso."),
                ("I saw myself in it and it shot me. Rude.", "Me vi nele e ele atirou em mim. Que grosseria."),
                ("Bronze that holds moonlight. I'd love to know the alloy.", "Bronze que segura luar. Queria muito saber a liga."),
                ("I made a face in the mirror and it made a lightning back!", "Fiz careta no espelho e ele fez um raio de volta!"));

            // -- Anéis de Mingcheng, Vilas do Anel e Colinas de Chá (41) --
            Page(entries, "enemy_mountain_paper_crane",
                "The children of the rings fold a thousand paper cranes for a wish, and hang them in the windows until the wish comes. One of those wishes was eaten by the shadow before it came, and the cranes flew off without it. They are only paper, but the folds are sharp, and they throw them. Close in fast; paper doesn't like being close.",
                "As crianças dos anéis dobram mil grous de papel pra fazer um pedido, e penduram nas janelas até o pedido chegar. Um desses pedidos a sombra comeu antes de chegar, e os grous foram embora voando sem ele. São só papel, mas as dobras são afiadas, e eles atiram. Cheguem perto rápido; papel não gosta de perto.",
                ("Shoots folds of paper. Close in fast.", "Atira dobras de papel. Fecha rápido."),
                ("Somebody wished something on these. I hope it was a good wish.", "Alguém fez um pedido com eles. Espero que fosse um pedido bom."),
                ("A paper cut. From a BIRD. That's a new one.", "Um corte de papel. De um PASSARINHO. Essa é nova."),
                ("A thousand folds and not one of them came out wrong. Admirable work.", "Mil dobras e nenhuma saiu torta. Trabalho admirável."),
                ("Can I keep one? I'll unfold it gently!", "Posso ficar com um? Eu desdobro com cuidado!"));

            // -- Campos de Painço, Arrozais Espelhados e Colinas de Chá (44) --
            Page(entries, "enemy_cave_smoke_locust",
                "The Smoke is the first of the Dark's five, and it settles where people are hungry. Over the millet and the mirrored paddies it took the shape of locusts, which are hunger with legs. They come in clouds and jump very far. Don't run in front of them across the open field; stand by the dikes and swing when they land.",
                "A Fumaça é a primeira das cinco da Treva, e ela pousa onde as pessoas passam fome. Por cima do painço e dos arrozais espelhados ela tomou a forma de gafanhoto, que é fome com perna. Vêm em nuvem e pulam muito longe. Não corram na frente deles no campo aberto; fiquem perto dos diques e batam quando eles pousarem.",
                ("They swarm and jump. Hit them as they land.", "Vêm em enxame e pulam. Bate quando pousarem."),
                ("They're eating the millet. People need that millet.", "Estão comendo o painço. As pessoas precisam desse painço."),
                ("One jumped in my mouth. I'm not talking about it.", "Um pulou na minha boca. Não vou falar disso."),
                ("Hunger with legs. Grandma, you put that very precisely.", "Fome com perna. Vó, a senhora falou isso com muita precisão."),
                ("They jump SO far! Further than Joaquim!", "Eles pulam TÃO longe! Mais que o Joaquim!"));

            // -- Ruínas de Gedi, Kilwa Afundada e Palácio de Husuni (29) --
            Page(entries, "enemy_forest_coral_ushabti",
                "The Artificers left answering figures in the drowned town, to keep it until the sea gave it back. The coral grew over them while they waited, and they forgot whom they were waiting for. Now they stand in the streets under the water with their arms crossed, and they throw their whole weight at anyone who walks in. Hit them where the seal on the chest shines.",
                "Os Artífices deixaram figuras que respondem na cidade afogada, pra guardar ela até o mar devolver. O coral cresceu em cima delas enquanto esperavam, e elas esqueceram por quem estavam esperando. Agora ficam nas ruas debaixo d'água de braços cruzados, e jogam o peso todo em quem entra. Batam onde o selo do peito brilha.",
                ("Slow and heavy. Strike the shining seal.", "Lenta e pesada. Bate no selo que brilha."),
                ("They're still waiting. Someone should tell them they can rest.", "Ainda estão esperando. Alguém devia dizer que podem descansar."),
                ("I hit it and a fish came out of its ear.", "Bati e saiu um peixe da orelha dela."),
                ("An order given a thousand years ago, still being carried out. Badly, but still.", "Uma ordem dada há mil anos, ainda sendo cumprida. Mal, mas ainda."),
                ("It's pink! It's a pink statue with a hat of coral!", "É rosa! É uma estátua rosa com chapéu de coral!"));

            // -- Beira Oeste, Cais da Coluna e Píer do Sol (91) --
            Page(entries, "enemy_desert_confused_spark",
                "The sparks gathered up from the world wait on the quay for their turn on the Barcas de Luz. Some of them waited so long they forgot what they were waiting for, and now they fly about the edges of the quay bumping into everyone. They are not wicked, only lost, and there are a great many. Don't chase them. Stand still and let them bump into your weapons.",
                "As faíscas recolhidas do mundo esperam no cais a vez delas nas Barcas de Luz. Algumas esperaram tanto que esqueceram o que estavam esperando, e agora voam pelas beiradas do cais trombando em todo mundo. Não são más, só perdidas, e são muitas. Não corram atrás delas. Fiquem parados e deixem que trombem nas armas de vocês.",
                ("Many and erratic. Stand still and let them come.", "Muitas e desorientadas. Fica parado e deixa virem."),
                ("They're just lost. The Barca will come back for them.", "Só estão perdidas. A Barca vai voltar pra buscar."),
                ("It bumped into me five times. On purpose, I think.", "Trombou em mim cinco vezes. De propósito, acho."),
                ("A light that forgot where it was going. I know a few people like that.", "Uma luz que esqueceu pra onde ia. Conheço umas pessoas assim."),
                ("It's dizzy! I'll show it the way to the boat!", "Ela tá tonta! Eu mostro o caminho do barco!"));

            // -- Roda Celeste, Jarras e Engrenagens (94) --
            Page(entries, "enemy_cave_dark_drop",
                "The Wheel lifts the light up to the Moon in its jars, round and round, and now and then a drop of the Dark comes up with it, clinging to the bottom of a jar. It drips off the gears and runs back down looking for more light to fall on. It is quick for a drop. When the light it swallowed shines through, step back, because it is about to splash.",
                "A Roda leva a luz até a Lua nas jarras dela, rodando e rodando, e de vez em quando sobe junto uma gota de Treva, grudada no fundo de uma jarra. Ela pinga das engrenagens e desce correndo procurando mais luz pra cair em cima. É rápida pra uma gota. Quando a luz que ela engoliu aparecer brilhando, deem um passo atrás, que ela vai espirrar.",
                ("Quick. When the light shows through, step back.", "Rápida. Quando a luz aparecer, recua."),
                ("It swallowed a little light. Maybe we can get it back out.", "Engoliu uma luzinha. Quem sabe a gente consegue tirar."),
                ("It splashed my boots. They're BLACK now. They were black before, but still.", "Espirrou na minha bota. Tá PRETA agora. Já era preta, mas mesmo assim."),
                ("A drop that runs uphill toward the light. Most physics disagrees.", "Uma gota que corre morro acima atrás da luz. A física quase toda discorda."),
                ("It's a raindrop of night! Can I catch it in a cup?", "É uma gota de chuva de noite! Posso pegar num copo?"));
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
