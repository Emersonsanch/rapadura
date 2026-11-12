using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve most-seen roles of Regions 10–16 that the overworld still played with stand-ins (bible §3.2.1,
    /// fourteenth pass): lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="MidStandInBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3); the kul only as the Komi tales
    /// give them, Ullikummi and Upelluri only as the old song gives them, and no god appears.
    /// </summary>
    public static class FarStandInBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_peak_vulture", "enemy_mountain_shadow_kite", "enemy_mountain_drifting_floe", "enemy_mountain_little_kul",
            "enemy_mountain_hungry_flame", "enemy_forest_diorite_stone", "enemy_forest_shoulder_bird", "enemy_forest_dream_bird",
            "enemy_forest_little_shadow", "enemy_forest_dark_bat", "enemy_forest_orbit_wind", "enemy_forest_runaway_lantern",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Farnkesh e a Escadaria dos Cestos (54) --
            Page(entries, "enemy_mountain_peak_vulture",
                "The vultures of the pinnacles used to keep the peaks clean, and the people of Farnkesh left them the leftovers on the high stones as thanks. Then the wind of ruin settled in their ruffs, and now they keep the basket stairs from anyone who climbs. They sit on the towers and throw their hard feathers from far off. Don't stop on the stairs to look at the view; keep climbing, and when one comes down low, that is the moment.",
                "Os abutres dos pináculos mantinham os picos limpos, e o povo de Farnkesh deixava as sobras nas pedras altas como agradecimento. Aí o vento ruim se aninhou nas golas deles, e agora eles guardam as escadas dos cestos de quem sobe. Ficam nas torres e jogam as penas duras de longe. Não parem na escada pra olhar a paisagem; continuem subindo, e quando um descer baixo, é a hora.",
                ("It shoots from the towers. Wait till it comes down, then hit it.", "Atira das torres. Espera ele descer e aí bate."),
                ("Somebody used to leave them food. I wonder if anyone still does.", "Alguém deixava comida pra eles. Será que ainda deixam?"),
                ("Hey! Come down here and say that!", "Ei! Desce aqui e fala isso na minha cara!"),
                ("It guards the stairs it once cleaned. That is almost a promotion.", "Guarda a escada que antes limpava. Quase uma promoção."),
                ("It has a fluffy collar! Like a cloud scarf!", "Ele tem uma gola fofinha! Tipo um cachecol de nuvem!"));

            // -- Pontes de Vento e Ermida Baixa (55) --
            Page(entries, "enemy_mountain_shadow_kite",
                "The children of Farnkesh flew paper kites from the wind bridges, with faces painted on them so the wind would have somebody to talk to. When the ropes of the bridges were cut, the kites went on flying by themselves, and the shadow got into the paper. They still have the faces, gone dark, and they spit little bolts of the lamp-light they stole. Watch the ribbon; where the ribbon goes, the kite follows. And never let one get between you and the bridge.",
                "As crianças de Farnkesh soltavam pipas de papel das pontes de vento, com rosto pintado pra o vento ter com quem conversar. Quando cortaram as cordas das pontes, as pipas continuaram voando sozinhas, e a sombra entrou no papel. Ainda têm os rostos, escurecidos, e cospem raiozinhos da luz das lamparinas que roubaram. Olhem a rabiola; pra onde a rabiola vai, a pipa vai atrás. E nunca deixem uma ficar entre vocês e a ponte.",
                ("It spits from range. Follow the ribbon and close in.", "Cospe de longe. Segue a rabiola e fecha nela."),
                ("A child painted that face. Look, it's still smiling a little.", "Uma criança pintou esse rosto. Olha, ainda sorri um pouquinho."),
                ("I'm grabbing the string. Somebody hold my legs.", "Vou pegar a linha. Alguém segura minhas pernas."),
                ("A kite with no one holding the string is just a very small storm.", "Pipa sem ninguém segurando a linha é só uma tempestade bem pequena."),
                ("Can we fix it and fly it properly? Please?", "A gente pode consertar e soltar direitinho? Por favor?"));

            // -- Banquisas e o Mar das Auroras (63) --
            Page(entries, "enemy_mountain_drifting_floe",
                "On the night of the great aurora a slab of the pack ice broke off with a ribbon of the light frozen inside it. The ice did not know what to do with something so bright, and the shadow told it to keep it. Now it drifts after the boats and leans its whole weight on whatever is warm. It is slow, and it is heavy. When the light inside flares, get off the ice it is about to come down on.",
                "Na noite da grande aurora, um pedaço da banquisa se soltou com uma fita da luz congelada dentro. O gelo não sabia o que fazer com uma coisa tão brilhante, e a sombra disse pra ele guardar. Agora ele vai atrás dos barcos e encosta todo o peso no que estiver quente. É lento, e é pesado. Quando a luz de dentro brilhar, saiam do gelo onde ele vai cair.",
                ("Slow, heavy. When it glows, step back, then hit.", "Lento e pesado. Quando brilhar, recua e depois bate."),
                ("It's holding the aurora like something it found and can't give back.", "Segura a aurora feito coisa que achou e não consegue devolver."),
                ("I punched it. My hand is cold now. Forever, probably.", "Dei um soco. Minha mão ficou gelada. Pra sempre, acho."),
                ("Ice with a light inside. Somebody will build a lamp out of that one day.", "Gelo com luz dentro. Um dia alguém faz uma lâmpada disso."),
                ("It's a floating window! There's a rainbow in it!", "É uma janela que boia! Tem um arco-íris dentro!"));

            // -- Fundo do Velho Kul e Lago do Kul (64) --
            Page(entries, "enemy_mountain_little_kul",
                "In the Komi tales the kul are the water's own people, and they keep the fish; you leave them a little of the catch and they leave you the rest. When the Old Kul soured, the little ones came up through the fishing holes after the lamps of the fishermen, and one of them is still holding a lamp's light in its chest. They come in a crowd, wet hair dragging on the ice. Don't lean over a hole to look in. And don't take that light from it; let it go back down, and it will.",
                "Nos contos komi os kul são o povo da própria água, e guardam os peixes; vocês deixam um pouco da pesca e eles deixam o resto. Quando o Velho Kul azedou, os pequenos subiram pelos buracos de pesca atrás das lamparinas dos pescadores, e um deles ainda segura a luz de uma lamparina no peito. Vêm em bando, com o cabelo molhado arrastando no gelo. Não se debrucem num buraco pra espiar. E não tirem essa luz dele; deixem que ele volte lá pra baixo, que ele volta.",
                ("They come in numbers. Keep your back to solid ice.", "Vêm em bando. Fica de costas pro gelo firme."),
                ("They only wanted the lamps. It's very dark under the ice.", "Só queriam as lamparinas. É muito escuro debaixo do gelo."),
                ("Its hair is all over my boots. Ugh, it's WET hair.", "O cabelo dele tá todo na minha bota. Eca, é cabelo MOLHADO."),
                ("The water keeps its own accounts. We are just overdue on the fish.", "A água tem as contas dela. A gente só está devendo os peixes."),
                ("It's like a little person made of puddle! Hi!", "Parece uma pessoinha feita de poça! Oi!"));

            // -- Caldeira do Fogo Devorador, Lago de Fogo e Cova do Fogo (70, 75, 85) --
            Page(entries, "enemy_mountain_hungry_flame",
                "There is the fire that warms the pot and there is the Devouring Fire, the fire of the Dark, which eats and is never warm. Little tongues of it come loose from the caldera, from the lake of fire and from the burning pit, and go looking for more to eat. You will see the light it ate still burning in the middle. Don't feed it, not a torch, not a twig. Put it out quickly and step back; it is hungriest just before it bites.",
                "Tem o fogo que esquenta a panela e tem o Fogo Devorador, o fogo da Treva, que come e nunca esquenta. Linguinhas dele se soltam da caldeira, do lago de fogo e da cova que arde, e saem procurando mais pra comer. Vocês vão ver a luz que ele comeu ainda queimando no meio. Não deem de comer, nem tocha, nem graveto. Apaguem depressa e deem um passo atrás; ele fica com mais fome logo antes de morder.",
                ("They swarm. Put them out fast, one by one.", "Vêm em enxame. Apaga rápido, uma de cada vez."),
                ("There's light inside it. It didn't ask to be eaten.", "Tem luz dentro dela. Ela não pediu pra ser comida."),
                ("It ATE my torch. Right out of my hand.", "Ela COMEU minha tocha. Da minha mão."),
                ("A fire that eats and is never warm is just an appetite with a colour.", "Fogo que come e não esquenta é só uma fome com cor."),
                ("It has a mouth! Fire shouldn't have a mouth!", "Ela tem boca! Fogo não devia ter boca!"));

            // -- Ombro de Upelluri e Diorito Crescente (81) --
            Page(entries, "enemy_forest_diorite_stone",
                "The old song says a child of diorite was set on Upelluri's shoulder, the giant who carries the sky and the earth and never noticed. The stone child grew and grew, and chips broke off him as he grew. These are the chips. They crawl about the shoulder on little stubs looking for the rest of themselves, and they throw their whole weight at anything that moves. Hit them where the crack shines, and don't stand in front of the eye.",
                "A canção antiga diz que puseram uma criança de diorito no ombro de Upelluri, o gigante que carrega o céu e a terra e nem reparou. A criança de pedra cresceu, cresceu, e foi soltando lascas enquanto crescia. Estas são as lascas. Andam pelo ombro em uns tocos, procurando o resto de si, e jogam o peso todo em qualquer coisa que se mexe. Batam onde a rachadura brilha, e não fiquem na frente do olho.",
                ("Hard as it looks. Hit the shining crack.", "É dura como parece. Bate na rachadura que brilha."),
                ("It's looking for the rest of itself. I know that feeling a little.", "Tá procurando o resto de si. Eu conheço um pouco esse sentimento."),
                ("I hit a rock. The rock hit back. Not fair.", "Bati numa pedra. A pedra bateu de volta. Não é justo."),
                ("Diorite is very hard. Whoever carved that eye had excellent tools.", "Diorito é duríssimo. Quem esculpiu esse olho tinha ferramentas ótimas."),
                ("It's a crawling rock with one eye! Hello, little rock!", "É uma pedra que anda com um olho só! Oi, pedrinha!"));

            // -- Ombro de Upelluri, Dobras de Pedra e Beiral do Céu (81) --
            Page(entries, "enemy_forest_shoulder_bird",
                "Birds have nested on Upelluri's shoulder since before anyone counted, and he never felt them. When the stone child grew in their nests, the birds took the colour of the stone and a bit of its temper too. They have lichen on their backs now, like the giant has, and they dive at anyone walking where they nest. They are quick but light. Keep your guard up and let them come to you.",
                "Desde antes de alguém contar, os pássaros fazem ninho no ombro de Upelluri, e ele nunca sentiu. Quando a criança de pedra cresceu nos ninhos, os pássaros pegaram a cor da pedra e um pouco do gênio dela também. Agora têm líquen nas costas, igual ao gigante, e mergulham em quem anda perto dos ninhos. São rápidos, mas leves. Fiquem com a guarda alta e deixem que venham até vocês.",
                ("Fast divers. Hold your ground and counter.", "Mergulham rápido. Firma o pé e contra-ataca."),
                ("They're only guarding their nests. I'd do the same.", "Só estão guardando os ninhos. Eu faria igual."),
                ("It pecked my head! I have a STONE bird bump!", "Bicou minha cabeça! Tô com um galo de pássaro de PEDRA!"),
                ("They live on a giant and think it is a mountain. Most of us do something similar.", "Moram num gigante e acham que é montanha. A maioria de nós faz algo parecido."),
                ("It has moss on its back! It's a garden bird!", "Tem musgo nas costas! É um passarinho-jardim!"));

            // -- Campo das Tábuas do Sonho (82) --
            Page(entries, "enemy_forest_dream_bird",
                "The old kings wrote down their dreams on clay so they would not forget them, and kept the tablets in a field. A bird flew out of one of those dreams and kept the writing on its feathers. Then the shadow broke the tablets, and the bird lost the way back into its dream. It flits about the broken field, very fast and a little lost, and pecks at anyone who might have taken its page. Don't swat at it. Wait, it always comes back the same way.",
                "Os reis antigos escreviam os sonhos no barro pra não esquecer, e guardavam as tábuas num campo. Um pássaro saiu voando de um desses sonhos e ficou com a escrita nas penas. Aí a sombra quebrou as tábuas, e o pássaro perdeu o caminho de volta pro sonho. Fica voejando pelo campo quebrado, rapidíssimo e meio perdido, e bica quem possa ter levado a página dele. Não fiquem abanando a mão. Esperem, ele sempre volta pelo mesmo caminho.",
                ("It always comes back the same way. Time it.", "Sempre volta pelo mesmo caminho. Pega o tempo."),
                ("It's looking for its dream. I hope it finds the right tablet.", "Tá procurando o sonho dele. Espero que ache a tábua certa."),
                ("I didn't take your page! I can't even read that!", "Eu não peguei sua página! Eu nem sei ler isso!"),
                ("Somebody's dream, still flying, without the somebody. Remarkable.", "O sonho de alguém, ainda voando, sem o alguém. Notável."),
                ("Its feathers have writing! What does it say, Ícaro?", "As penas dele têm escrita! O que tá escrito, Ícaro?"));

            // -- Fortaleza da Última Lâmpada (83) --
            Page(entries, "enemy_forest_little_shadow",
                "The little shadows are the ones the fortress lamps throw on the walls, the ones that got up and walked off while the lamps were looking the other way. They are about the size of Lavine, they run on all fours, and they steal wicks, because a wick is the only thing they are afraid of. Don't chase them into the dark corners. Stay by the lamps and let them come; they always come for the wick.",
                "As sombras pequenas são as que as lâmpadas da fortaleza jogam nas paredes, as que levantaram e foram embora enquanto as lâmpadas olhavam pro outro lado. Têm mais ou menos o tamanho da Lavine, correm de quatro, e roubam pavio, porque pavio é a única coisa de que elas têm medo. Não corram atrás delas nos cantos escuros. Fiquem perto das lâmpadas e deixem que venham; elas sempre vêm atrás do pavio.",
                ("Quick and low. Stay in the light and let it come.", "Rápida e baixa. Fica na luz e deixa ela vir."),
                ("It's afraid of the wick, so it steals it. That's so sad.", "Tem medo do pavio, então rouba. Que coisa triste."),
                ("It took my candle! I wasn't even using it! Okay, I was.", "Levou minha vela! Eu nem tava usando! Tá, tava."),
                ("A shadow without anyone to belong to. It must be cold.", "Uma sombra sem ninguém pra pertencer. Deve ser frio."),
                ("It's my size! Grandma, it's exactly my size!", "É do meu tamanho! Vó, é igualzinha do meu tamanho!"));

            // -- Fortaleza da Última Lâmpada e Nichos de Basalto (83) --
            Page(entries, "enemy_forest_dark_bat",
                "The bats of the Dark hang in the arrow slits of the last fortress and drink the lamps one by one, so slowly that the guards only notice when the corridor is black. Each one alone is small. They come in a cloud, and the cloud is the trouble. Keep your back to a wall that has a lamp on it, and strike when they bunch up.",
                "Os morcegos da Treva ficam pendurados nas seteiras da última fortaleza e bebem as lâmpadas uma por uma, tão devagar que os guardas só percebem quando o corredor já está preto. Cada um sozinho é pequeno. Eles vêm em nuvem, e o problema é a nuvem. Fiquem de costas pra uma parede que tenha lâmpada, e batam quando eles se juntarem.",
                ("They swarm. Back to a lit wall, strike when they bunch.", "Vêm em enxame. Costas pra parede acesa, bate quando juntarem."),
                ("Poor things, they're drinking the light because they never had any.", "Coitados, bebem a luz porque nunca tiveram nenhuma."),
                ("There are SO many. Who counts bats? Not me.", "São MUITOS. Quem conta morcego? Eu não."),
                ("They drink a lamp so slowly it looks like nothing happened. That is how the Dark likes to work.", "Bebem uma lâmpada tão devagar que parece que nada aconteceu. É assim que a Treva gosta de trabalhar."),
                ("Their ears are bigger than their heads! Like little horns!", "As orelhas são maiores que a cabeça! Tipo chifrinhos!"));

            // -- Esferas dos Arcontes, Mirante e Varanda (95, 99, 100) --
            Page(entries, "enemy_forest_orbit_wind",
                "Up by the Column of Glory there are winds whose only work is to carry the lights round the spheres, each on its own path, round and round, until the light is ready to go up. The archons knocked this one off its orbit. It still turns, because turning is all it knows, but now it turns round whatever comes near, faster and faster, and then it gusts. Stand still when it circles you, and hit the little light in the middle.",
                "Lá perto da Coluna da Glória tem uns ventos que só servem pra levar as luzes ao redor das esferas, cada uma no seu caminho, rodando e rodando, até a luz estar pronta pra subir. Os arcontes tiraram este da órbita. Ele ainda gira, porque girar é tudo o que sabe, mas agora gira em volta do que chega perto, cada vez mais rápido, e aí dá a rajada. Fiquem parados quando ele rodear vocês, e batam na luzinha do meio.",
                ("It circles fast. Stand still and hit the core.", "Rodeia rápido. Fica parado e bate no miolo."),
                ("It's still trying to do its job. Somebody took its path away.", "Ainda tenta fazer o trabalho dele. Alguém tirou o caminho dele."),
                ("I'm dizzy. I'm SO dizzy. Is it still going?", "Tô tonto. Tô MUITO tonto. Ele ainda tá rodando?"),
                ("An orbit with nothing in the middle. A very lonely kind of physics.", "Uma órbita sem nada no meio. Um tipo de física bem solitária."),
                ("It's a merry-go-round made of wind! Can I ride it?", "É um carrossel de vento! Posso andar nele?"));

            // -- Rio de Luz e Ilhotas de Nuvem (92) --
            Page(entries, "enemy_forest_runaway_lantern",
                "The Barcas de Luz carry the light that has been gathered up the river and up the Column, in paper lanterns on the decks. One of those lanterns did not want to go up. It ran off the deck, hid in the clouds over the river, and burnt two holes in itself so it could see where it was running. Now there are a lot of them, and they knock into anyone who comes looking. Don't be rough with them. Put the flame out gently and the Barca will come back for the light.",
                "As Barcas de Luz levam a luz recolhida rio acima e Coluna acima, em lanternas de papel nos conveses. Uma dessas lanternas não quis subir. Fugiu do convés, se escondeu nas nuvens em cima do rio e queimou dois furinhos em si mesma pra ver por onde corria. Agora tem um monte delas, e elas trombam em quem vem procurar. Não sejam brutos com elas. Apaguem a chama com jeito, que a Barca volta pra buscar a luz.",
                ("They swarm and ram. Sidestep and tap them out.", "Vêm em bando e trombam. Desvia e apaga."),
                ("It was scared of going up. That's allowed, you know.", "Ela teve medo de subir. Isso pode, sabia?"),
                ("A lantern headbutted me. A PAPER lantern.", "Uma lanterna me deu cabeçada. Uma lanterna de PAPEL."),
                ("A light that chose not to be gathered. The Barcas did not plan for that.", "Uma luz que escolheu não ser recolhida. As Barcas não contavam com isso."),
                ("It made itself eyes! It wanted to see the sky!", "Ela fez olhinhos em si mesma! Queria ver o céu!"));
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
