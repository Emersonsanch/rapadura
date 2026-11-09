using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Margens da Mistura (Docs/Mapas100.md 77–82) that the overworld layout used to play
    /// with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="RioBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Zurvan stays a clock here, never a person
    /// (bible §1.1), and the giants of the Hurrian song are only stone.
    /// </summary>
    public static class MisturaBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_brick_lion", "enemy_forest_young_mushussu", "enemy_forest_bitumen_locust", "enemy_forest_living_hourglass",
            "enemy_forest_late_shadow", "enemy_forest_living_tar", "enemy_forest_tar_heron", "enemy_forest_angry_cedar",
            "enemy_forest_glow_mantle", "enemy_forest_ullikummi_son", "enemy_forest_broken_tablet", "enemy_forest_garden_shadow",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Dur-Jarras and its outskirts (77) --
            Page(entries, "enemy_forest_brick_lion",
                "The lions of Dur-Jarras were never alive: somebody glazed them brick by brick along the street of the processions, blue and gold, so the whole town would walk between them on feast days. Sin-leqi-unninni has the old list of who laid which course. When the shadow came through the walls it put the glaze out from the inside, and the lions climbed down to go and look for whoever had stopped looking at them. They are slow, and brick does not get tired. Hit the seams, where the mortar is, and don't stand in front of the mouth.",
                "Os leões de Dur-Jarras nunca foram vivos: alguém vitrificou eles tijolo por tijolo ao longo da rua das procissões, azul e dourado, pra cidade inteira passar no meio deles em dia de festa. O Sin-leqi-unninni tem a lista antiga de quem assentou cada fiada. Quando a sombra atravessou os muros, ela apagou o vidrado por dentro, e os leões desceram pra procurar quem parou de olhar pra eles. São lentos, e tijolo não cansa. Batam nas juntas, onde fica a argamassa, e não fiquem na frente da boca.",
                ("Aim for the mortar, it's the weak line.", "Mira na argamassa, é a linha fraca."),
                ("All that blue, and it went out one brick at a time.", "Tanto azul, e foi apagando um tijolo de cada vez."),
                ("I knocked. Nobody home. Very rude lion.", "Bati. Ninguém em casa. Leão muito mal-educado."),
                ("Glazed brick was fired twice. He has been through worse than us.", "Tijolo vidrado ia ao forno duas vezes. Ele já passou por coisa pior que a gente."),
                ("He has little squares all over! Can I keep one?", "Ele é todo de quadradinho! Posso ficar com um?"));

            Page(entries, "enemy_forest_young_mushussu",
                "The dragon-snake on the old gates has a snake's head, a lion's front feet, an eagle's back feet and a sting for a tail, and I always thought whoever drew it could not make up his mind. Atrahasis says the young ones really are like that. They grow into the parts one by one, so they don't know yet which half is supposed to go first. Fast, though, and they lose interest in nothing. Keep something between you and the tail. A young animal uses all of itself, and all of itself is pointy.",
                "O dragão-serpente dos portões antigos tem cabeça de cobra, pata da frente de leão, pata de trás de águia e ferrão no rabo, e eu sempre achei que quem desenhou não conseguiu se decidir. O Atrahasis diz que os novinhos são assim mesmo. Eles vão crescendo pedaço por pedaço, e ainda não sabem qual metade deve ir na frente. Mas são rápidos, e não perdem o interesse por nada. Deixem sempre alguma coisa entre vocês e o rabo. Bicho novo usa o corpo inteiro, e o corpo inteiro dele pica.",
                ("Watch the sting, it comes after the bite.", "Olho no ferrão, ele vem depois da mordida."),
                ("He doesn't know what he is yet. I feel for him.", "Ele ainda não sabe o que é. Tenho pena."),
                ("He has four kinds of feet. Imagine buying shoes.", "Ele tem quatro tipos de pé. Imagina comprar sapato."),
                ("Babylonian gates: glazed in relief, 575 BC or so. His grandparents posed.", "Portões da Babilônia: relevo vidrado, uns 575 a.C. Os avós dele posaram."),
                ("He's all mixed up like my drawings!", "Ele é todo misturado igual meus desenhos!"));

            Page(entries, "enemy_forest_bitumen_locust",
                "Everybody here knows the locust, and everybody knows what it does to barley. These ones started falling in the wells of bitumen and coming out shiny, and since nothing wanted to eat them anymore, there got to be a great many of them. Each one is the size of a cat and has the appetite of a small town. The pitch on them burns, which is good to remember if you are carrying a torch and bad to remember if you are standing in the barley. Kill them before they land on anything you want to keep.",
                "Todo mundo aqui conhece gafanhoto, e todo mundo sabe o que ele faz com a cevada. Esses começaram a cair nos poços de betume e a sair brilhando, e como nada mais queria comer eles, foram ficando muitos. Cada um é do tamanho de um gato e tem o apetite de uma vila pequena. O piche neles pega fogo, o que é bom lembrar se vocês estão com uma tocha e ruim lembrar se estão no meio da cevada. Matem antes que eles pousem em qualquer coisa que vocês queiram guardar.",
                ("Thin them while they're in the air.", "Derruba no ar, antes de pousar."),
                ("He only wanted a bit of barley. So did everybody.", "Ele só queria um pouco de cevada. Todo mundo quer."),
                ("Sticky AND hungry. That's my cousin Ícaro at breakfast.", "Grudento E com fome. É o Ícaro no café da manhã."),
                ("Bitumen waterproofs the shell. That is actually a clever adaptation.", "O betume impermeabiliza a carapaça. Até que é uma adaptação esperta."),
                ("He leaves black footprints on everything!", "Ele deixa pegadinha preta em tudo!"));

            // -- Deserto do Relógio (78) --
            Page(entries, "enemy_forest_living_hourglass",
                "Utnapishtim looks after the Clock of Zurvan in the city and says a clock is a very simple animal: it eats sand and it counts. In the desert to the east there are old hourglasses from the room where they tried to measure the longest hour, and when the shadow got into the glass they got up and went looking for the hour that was stolen. Don't try to wait one out. It has more time than you do, that is the whole of what it is. Break the bars and then the glass, and step away from the sand.",
                "O Utnapishtim cuida do Relógio de Zurvan na cidade e diz que relógio é um bicho muito simples: come areia e conta. No deserto a leste tem ampulhetas antigas, da sala onde tentaram medir a hora mais comprida, e quando a sombra entrou no vidro elas levantaram e foram procurar a hora que foi roubada. Não tentem cansar uma delas. Ela tem mais tempo que vocês, é só isso que ela é. Quebrem as barras, depois o vidro, e saiam de perto da areia.",
                ("Bars first, then the glass.", "Primeiro as barras, depois o vidro."),
                ("It's looking for an hour that nobody is going to give back.", "Ela procura uma hora que ninguém vai devolver."),
                ("I turned it upside down. It did not appreciate the joke.", "Virei ela de cabeça pra baixo. Ela não achou graça."),
                ("Sand flow doesn't depend on how full the bulb is. That's why they worked.", "O fluxo da areia não depende de quanto tem no bulbo. Por isso funcionava."),
                ("Is it time for lunch? I'm asking it!", "Já é hora do almoço? Vou perguntar pra ela!"));

            Page(entries, "enemy_forest_late_shadow",
                "In that desert a shadow can fall behind whoever is casting it. Usually it hurries and catches up. Sometimes the owner walks away, or dies, or was only a bird passing over, and then the shadow stays out there on the sand with nothing to be attached to, and it gets a temper. They come at you slow, a moment after you think they will, and the moment is the dangerous part. Don't swing where it is, swing where it is about to arrive. Yes, that sounds foolish. Do it anyway.",
                "Naquele deserto uma sombra pode ficar pra trás de quem faz ela. Normalmente ela se apressa e alcança. Às vezes o dono vai embora, ou morre, ou era só um passarinho passando, e aí a sombra fica ali na areia sem ter a quem se prender, e pega um gênio. Elas vêm devagar, um instante depois do que vocês acham, e é o instante que é perigoso. Não batam onde ela está, batam onde ela vai chegar. É, parece bobagem. Façam assim mesmo.",
                ("Hit where it's going to be, not where it is.", "Bate onde ela vai estar, não onde está."),
                ("It's only looking for whoever it belonged to.", "Ela só procura de quem ela era."),
                ("Mine is on time. I've been training it.", "A minha é pontual. Tenho treinado ela."),
                ("A shadow with no source shouldn't exist. I'd love to measure one.", "Sombra sem fonte não devia existir. Eu adoraria medir uma."),
                ("It waved at me a little bit late!", "Ela me deu tchau bem atrasadinha!"));

            // -- Poços de Betume (79) --
            Page(entries, "enemy_forest_living_tar",
                "Atrahasis caulked his boat with this and his boat went over the whole flood without taking in a cup of water, and I think that is the best thing anyone has ever said about a sticky substance. Then the shadow got into the wells, and the bitumen stopped being a thing you use and started being a thing that uses you. It grabs a boot and doesn't let go. It catches fire if you are careless, and you should be careless with it on purpose, from a good distance, with nobody standing nearby that you are fond of.",
                "O Atrahasis calafetou o barco dele com isso e o barco passou por cima do dilúvio inteiro sem entrar um copo de água, e eu acho que é a melhor coisa que alguém já disse sobre uma coisa grudenta. Aí a sombra entrou nos poços, e o betume parou de ser coisa que se usa e começou a ser coisa que usa a gente. Ele agarra a bota e não larga. Pega fogo se vocês se descuidarem, e vocês devem se descuidar dele de propósito, de bem longe, sem ninguém de quem vocês gostem por perto.",
                ("It sticks. Don't stand still next to it.", "Gruda. Não fica parado do lado."),
                ("It holds on so tightly. I wonder if anyone ever held it back.", "Ele segura tão forte. Será que alguém já segurou ele?"),
                ("I lost a shoe. Then I lost the other shoe, going back for it.", "Perdi um sapato. Depois perdi o outro, voltando buscar."),
                ("Fire works, but it spreads along the surface. Mind the wind.", "Fogo funciona, mas corre pela superfície. Cuidado com o vento."),
                ("It goes bloop! And then bloop again!", "Ele faz blup! E depois blup de novo!"));

            Page(entries, "enemy_forest_tar_heron",
                "A heron earns its living by standing still, and I always liked that about them. On the Wells the pools started moving under them, and the ones that stayed got it up to the knees and then in the throat. Now they stand far back on those long legs and spit it, a slow black gob that sticks where it lands. Don't try to outrun one in the open: get close behind the reeds, come in from the side, and remember a bird's neck is mostly neck.",
                "A garça ganha a vida ficando parada, e eu sempre gostei disso nelas. Nos Poços as poças começaram a se mexer debaixo delas, e as que ficaram pegaram piche até o joelho e depois na garganta. Agora ficam lá atrás naquelas pernas compridas e cospem, uma gosma preta lenta que gruda onde cai. Não tentem correr dela no aberto: cheguem perto pelos juncos, venham de lado, e lembrem que pescoço de garça é quase todo pescoço.",
                ("Get in from the side, behind the reeds.", "Entra pelo lado, atrás dos juncos."),
                ("She's standing so still, and it hurts her to swallow.", "Ela fica tão parada, e dói engolir."),
                ("She spat on my hat. I'm taking that personally.", "Ela cuspiu no meu chapéu. Levei pro lado pessoal."),
                ("Her range matches the pool radius. She doesn't need to walk at all.", "O alcance dela bate com o raio da poça. Ela nem precisa andar."),
                ("Her legs are thin like my pencil!", "As pernas dela são finas igual meu lápis!"));

            // -- Cedral de Hobabish (80) --
            Page(entries, "enemy_forest_angry_cedar",
                "Cedars are the only trees I ever met that seem to know what they are worth. The one that carried Hobabish's seven cloaks has been guarded for longer than anyone can count, and the shadow told the guardian that everybody who comes up the hill has an axe. It told the cedars the same thing, and some of them believed it enough to walk. Don't cut one. I mean it: if you promised not to, then you promised. Knock it down at the roots, and let the resin run, it is the only part that still remembers being kind.",
                "O cedro é a única árvore que eu já vi que parece saber quanto vale. O que carregava os sete mantos de Hobabish é guardado faz mais tempo do que dá pra contar, e a sombra disse pro guardião que todo mundo que sobe o morro traz machado. Disse a mesma coisa pros cedros, e alguns acreditaram o suficiente pra andar. Não cortem nenhum. É sério: se prometeram, prometeram. Derrubem pelas raízes e deixem a resina escorrer, é a única parte dele que ainda lembra de ser boa.",
                ("Go for the roots. Stay out of his reach.", "Vai nas raízes. Fica fora do alcance dele."),
                ("Someone told him that everyone has an axe. Poor tree.", "Disseram pra ele que todo mundo tem machado. Coitado."),
                ("I said sorry to him. He hit me anyway.", "Pedi desculpa pra ele. Ele me bateu assim mesmo."),
                ("Cedar resin resists rot. That's why the old boats and temples lasted.", "Resina de cedro resiste à podridão. Por isso os barcos e templos antigos duraram."),
                ("He smells so good and he's so grumpy!", "Ele tem um cheiro tão bom e é tão bravo!"));

            Page(entries, "enemy_forest_glow_mantle",
                "Hobabish wore seven cloaks of radiance. That is in the old book of the giants, and people have been arguing about it for a very long time. When the guardian went dim the cloaks did not go dim with him, they just came off, one at a time, and now they drift between the cedars looking for shoulders. There is nobody in there. It wraps around you and slows you down and you feel, for a moment, very important. That is the moment to get out. The lanterns of resin take them away, one lantern, one cloak.",
                "O Hobabish usava sete mantos de brilho. Isso está no livro antigo dos gigantes, e já faz muito tempo que discutem sobre isso. Quando o guardião se apagou os mantos não se apagaram com ele, só caíram, um de cada vez, e agora ficam vagando entre os cedros procurando ombros. Não tem ninguém ali dentro. Ele enrola em vocês, deixa vocês lentos e vocês se sentem, por um momento, muito importantes. Esse é o momento de sair. As lanternas de resina levam embora, uma lanterna, um manto.",
                ("It slows you. Break off and hit it again.", "Ele deixa lento. Sai e volta a bater."),
                ("It's been looking for someone to keep warm for so long.", "Ele procura faz tanto tempo alguém pra aquecer."),
                ("I tried it on. I looked amazing. Then it tried to eat me.", "Experimentei. Fiquei lindo. Aí ele tentou me comer."),
                ("Empty inside and still mobile? Convection doesn't explain that.", "Vazio por dentro e ainda se movendo? Convecção não explica."),
                ("He's so shiny! Can I wear him to the feast?", "Ele é tão brilhante! Posso usar na festa?"));

            // -- Ombro de Upelluri (81) --
            Page(entries, "enemy_forest_ullikummi_son",
                "There is a giant who holds the sky and the earth on his shoulder and has never noticed. Somebody planted a stone on him, a diorite one, and it grew until it nearly blocked the sky. They cut it down, and the chips stayed there on the shoulder, and the chips grew legs. They are not bright. They have inherited one idea, which is to be bigger, and they will apply it to you. Heavy blows do nothing to the middle of them, so go for the joints where the crystal is still growing, while it is still soft.",
                "Tem um gigante que carrega o céu e a terra no ombro e nunca percebeu. Alguém plantou uma pedra nele, de diorito, e ela cresceu até quase tapar o céu. Cortaram, as lascas ficaram ali no ombro, e as lascas criaram pernas. Não são espertas. Herdaram uma ideia só, que é ficar maior, e vão aplicar ela em vocês. Golpe pesado no meio não faz nada, então vão nas juntas onde o cristal ainda está crescendo, enquanto ainda está mole.",
                ("Joints, where the crystal is still growing.", "Nas juntas, onde o cristal ainda cresce."),
                ("He only knows one thing, and nobody taught him another.", "Ele só sabe uma coisa, e ninguém ensinou outra."),
                ("He's bigger than yesterday. I measured with my stick.", "Ele está maior que ontem. Medi com meu graveto."),
                ("Diorite doesn't grow. Except here, apparently, and I want a sample.", "Diorito não cresce. Só aqui, pelo visto, e quero uma amostra."),
                ("Is he growing because he eats his vegetables?", "Ele cresce porque come verdura?"));

            // -- Campo das Tábuas do Sonho (82) --
            Page(entries, "enemy_forest_broken_tablet",
                "Ohya and Hahya dreamed of a tablet washed in the water, and in the morning they went looking for someone to tell them what it meant. Out on that field the old stones are half the size they used to be. The shadow can't read, so it shook them until the words came loose, and they throw them. A broken sentence hurts more than you'd expect. Get close: it can't hit what stands behind it. And if one of the lines falls near you, don't read it. I am serious. Some sentences should be left unfinished.",
                "O Ohya e o Hahya sonharam com uma tábua lavada na água, e de manhã foram procurar quem explicasse o que era. Naquele campo as pedras velhas têm metade do tamanho que tinham. A sombra não sabe ler, então sacudiu elas até as palavras soltarem, e elas jogam as palavras. Uma frase quebrada machuca mais do que parece. Cheguem perto: ela não acerta quem fica atrás dela. E se uma linha cair do lado de vocês, não leiam. É sério. Tem frase que deve ficar sem terminar.",
                ("Get behind it, it only throws forward.", "Vai pra trás dela, ela só joga pra frente."),
                ("Half a word, thrown as hard as it can throw it.", "Meia palavra, jogada com toda a força."),
                ("I read the bit that hit me. It said 'and then'. And then what?!", "Li o pedaço que me acertou. Dizia 'e então'. E então o quê?!"),
                ("Cuneiform impressed on clay, then fired. Whoever wrote it meant it to last.", "Cuneiforme impresso em argila e depois queimado. Quem escreveu queria que durasse."),
                ("It writes on me! I'm a letter now!", "Ela escreve em mim! Virei uma carta!"));

            Page(entries, "enemy_forest_garden_shadow",
                "In the dream there is a garden where all the trees were cut. That is the whole of the dream and it frightened Ohya and Hahya so much they could not keep it to themselves. Whoever cut them is not in the story. The trees are. They came walking out of the dream on their stumps, sawn flat at the top, the rings still shining where the blade went through, and they are not angry the way the cedars are angry. It is worse. They just don't know what they are for. Go slowly, hit hard, and don't look down into the rings.",
                "No sonho tem um jardim com todas as árvores cortadas. É só isso o sonho, e assustou tanto o Ohya e o Hahya que eles não conseguiram guardar pra si. Quem cortou não está na história. As árvores estão. Saíram andando do sonho nos tocos, cortadas retinho no alto, com os anéis ainda brilhando onde a lâmina passou, e não têm raiva como os cedros têm. É pior. Eles só não sabem pra que servem. Vão devagar, batam forte e não olhem pra dentro dos anéis.",
                ("Slow swings. Stay out of the arc.", "Golpes lentos. Fica fora do arco."),
                ("It's not angry. It just doesn't know why it's still here.", "Não tem raiva. Só não sabe por que ainda está aqui."),
                ("I counted his rings. He's older than Vô Jorge. Barely.", "Contei os anéis dele. Ele é mais velho que o Vô Jorge. Por pouco."),
                ("Growth rings record every year. His stop all at the same one.", "Os anéis registram cada ano. Os dele param todos no mesmo."),
                ("He has a flat head. I could put my cup on it!", "A cabeça dele é reta. Dá pra pôr meu copo em cima!"));
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
