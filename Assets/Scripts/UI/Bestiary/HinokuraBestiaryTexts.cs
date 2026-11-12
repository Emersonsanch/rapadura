using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Arquipélago de Hinokura (Docs/Mapas100.md 65–70) that the overworld layout
    /// used to play with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="MarGeladoBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// Okinawa is alive and the shisa are still on its roofs: here they are only guardians the shadow dimmed, never mocked; the
    /// tibicenas only as the Guanche tale gives them; the tanuki only as the mischievous beast of the Japanese tales.
    /// </summary>
    public static class HinokuraBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_volcano_crab", "enemy_mountain_lava_lizard", "enemy_mountain_ember_gull", "enemy_mountain_dimmed_shisa",
            "enemy_mountain_tanuki", "enemy_mountain_tibicena", "enemy_mountain_giant_lava_lizard", "enemy_mountain_blade_fish",
            "enemy_mountain_ember_coral", "enemy_mountain_lacquer_ushabti", "enemy_mountain_furnace_ogre", "enemy_mountain_slag_giant",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Hinokura and its beaches (65; listed with the Mountain) --
            Page(entries, "enemy_mountain_volcano_crab",
                "Crabs have always climbed the black beaches of Hinokura to lay their eggs in the warm sand. Sango says that when the shadow came they stopped going back to the sea and started carrying the volcano with them: the shell grew a little cone that smokes, and now each one thinks it owns a mountain. It is the old lie of Āz, wanting more and more on your back until you cannot walk. Hit from the side, where the pincers don't reach, and when the claws open wide, get out from in front.",
                "Os caranguejos sempre subiram as praias negras de Hinokura pra botar os ovos na areia quente. A Sango conta que quando a sombra veio eles pararam de voltar pro mar e começaram a carregar o vulcão junto: a casca criou um conezinho que fumega, e agora cada um acha que é dono de uma montanha. É a velha mentira da Āz, querer mais e mais nas costas até não conseguir andar. Batam de lado, onde a pinça não alcança, e quando as garras abrirem grande, saiam da frente.",
                ("Slow. Heavy shell. The eyes on stalks are the soft part.", "Devagar. Casca pesada. Os olhos no talo são a parte mole."),
                ("He's carrying a whole volcano and he's so tired. Poor thing.", "Ele tá carregando um vulcão inteiro e tá tão cansado. Coitado."),
                ("I stepped on his cone by accident. It was hot. It was very hot.", "Eu pisei no cone dele sem querer. Tava quente. Tava bem quente."),
                ("The cone really vents. A crab with its own geothermal system.", "O cone solta vapor de verdade. Um caranguejo com sistema geotérmico próprio."),
                ("He has a tiny volcano! Can I keep the tiny volcano?", "Ele tem um vulcãozinho! Posso ficar com o vulcãozinho?"));

            Page(entries, "enemy_mountain_lava_lizard",
                "Little lizards have always lived in the walls of the lava channels, where the stones stay warm all night, and the smiths of Hinokura liked them, because they ate the flies. The shadow lit a stripe down their backs and made them forget that stones were enough. They run along the channels faster than the eye, and they don't want the warmth anymore, they want whatever is warm: your hands, your lunch, you. Watch for the line of light moving along the wall.",
                "Lagartinho sempre viveu nas paredes dos canais de lava, onde as pedras ficam quentes a noite inteira, e os ferreiros de Hinokura gostavam deles, porque comiam as moscas. A sombra acendeu uma listra nas costas deles e fez eles esquecerem que as pedras já bastavam. Eles correm pelos canais mais rápido que o olho, e não querem mais o calor, querem tudo que é quente: as mãos, o almoço, vocês. Fiquem de olho na linha de luz correndo pela parede.",
                ("Follow the light on his back. It tells you where he's going.", "Sigam a luz nas costas dele. Ela diz pra onde ele vai."),
                ("He used to eat the flies for the smiths. He remembers being useful.", "Ele comia as moscas dos ferreiros. Ele lembra de ser útil."),
                ("He ran up my leg. And down the other one. I didn't even swing.", "Ele subiu pela minha perna. E desceu pela outra. Eu nem balancei a espada."),
                ("That stripe is about nine hundred degrees. On a lizard. I need to sit down.", "Essa listra tem uns novecentos graus. Num lagarto. Preciso me sentar."),
                ("He looks like a fuse! Don't light him!", "Ele parece um pavio! Não acendam ele!"));

            Page(entries, "enemy_mountain_ember_gull",
                "Gulls follow the boats everywhere in the world, and the gulls of Hinokura followed the boats that carry ore to the forges. Too many of them fed on the sparks that fly out of the chimneys, the sparks stayed under their feathers, and the shadow did the rest. Now they circle over the laundry lines and the lighthouse boat and spit embers from far away, because they are too greedy to come close and share. Kurogane's people hang wet cloths over the forges for them. Shoot from cover, or close in under the wing.",
                "Gaivota segue barco no mundo inteiro, e as gaivotas de Hinokura seguiam os barcos que trazem minério pras forjas. Muitas delas comeram as faíscas que voam das chaminés, as faíscas ficaram debaixo das penas, e a sombra fez o resto. Agora elas rodam em cima dos varais e do Barco-Farol e cospem brasa de longe, porque são gulosas demais pra chegar perto e dividir. O pessoal do Kurogane pendura pano molhado nas forjas por causa delas. Atirem de trás de alguma coisa, ou cheguem por baixo da asa.",
                ("She spits, I shoot. She's the one with worse aim.", "Ela cospe, eu atiro. Ela é a que mira pior."),
                ("She keeps swallowing sparks. Her throat must hurt so much.", "Ela vive engolindo faísca. A garganta dela deve doer tanto."),
                ("An ember landed on my helmet. I smelled my own hair for an hour.", "Caiu uma brasa no meu capacete. Fiquei sentindo cheiro do meu cabelo uma hora."),
                ("Her feathers don't burn. There must be some oil in them. I want a feather.", "As penas dela não queimam. Deve ter algum óleo nelas. Eu quero uma pena."),
                ("She stole my dumpling and then she burned it!", "Ela roubou meu bolinho e ainda queimou ele!"));

            // -- Jardins de Lava Laqueada (66) --
            Page(entries, "enemy_mountain_dimmed_shisa",
                "On the roofs of Okinawa, far away and still today, the people set a pair of shisa, lions that are half dog, one with its mouth open and one with its mouth shut, to keep bad things out. The ones of the Lacquered Gardens were made by the same kind of hand, for the palaces of Hinokura. When the shadow passed they went dim and came down from the roofs, and now they wander in the gardens guarding nothing, biting anyone who comes near because they cannot remember who the house was for. Don't hate them. They are still doing their job, only wrong. Their mouths are wide and their bite is heavy: stay out of the front.",
                "Nos telhados de Okinawa, bem longe e até hoje, as pessoas põem um par de shisa, leões meio cachorro, um de boca aberta e outro de boca fechada, pra coisa ruim não entrar. Os dos Jardins Laqueados foram feitos pelo mesmo tipo de mão, pros palácios de Hinokura. Quando a sombra passou eles apagaram e desceram do telhado, e agora andam pelos jardins guardando nada, mordendo quem chega perto porque não lembram mais pra quem era a casa. Não tenham raiva deles. Eles ainda estão fazendo o trabalho, só que errado. A boca é larga e a mordida é pesada: fiquem fora da frente.",
                ("He's guarding nothing, very well. I'll go around.", "Ele tá guardando o nada, muito bem. Eu vou dar a volta."),
                ("He came down from the roof and forgot the way back. We can show him.", "Ele desceu do telhado e esqueceu o caminho de volta. A gente pode mostrar."),
                ("He has a roof tile stuck to his back. I know how that feels. Not the tile. The weight.", "Ele tem uma telha grudada nas costas. Eu sei como é. Não a telha. O peso."),
                ("Ceramic, fired once, glazed once. Whatever walks him around isn't the clay.", "Cerâmica, queimada uma vez, vidrada uma vez. O que faz ele andar não é o barro."),
                ("One had his mouth open and one had his mouth shut! Which one do I pet?", "Um tava de boca aberta e o outro de boca fechada! Qual eu faço carinho?"));

            Page(entries, "enemy_mountain_tanuki",
                "The tanuki is a real animal, a round raccoon-dog with a black mask, and the old tales of Japan call it a trickster who wears a leaf on its head to change its shape and fools the innkeepers. These in the pines were only ever cheeky. The shadow made them greedy instead of playful, and a trick that only takes is a theft. At night they come down to the gardens and the tea house with their little pouches of stolen coins. When they get hurt they run, so finish them quickly, or they'll be back tomorrow with your purse.",
                "O tanuki é um bicho de verdade, um cachorro-guaxinim redondo de máscara preta, e os contos antigos do Japão dizem que ele é um trapaceiro que põe uma folha na cabeça pra mudar de forma e engana os donos de estalagem. Os dos pinhais só sempre foram atrevidos. A sombra fez eles gulosos em vez de brincalhões, e brincadeira que só tira é roubo. De noite eles descem pros jardins e pra casa de chá com a bolsinha de moeda roubada. Quando se machucam eles fogem, então acabem logo, senão amanhã eles voltam com a sua bolsa.",
                ("He ran when I had the shot. Next time I aim at the escape route first.", "Ele correu quando eu tinha o tiro. Da próxima eu miro primeiro na saída."),
                ("He used to steal for fun and give it back. I think he'd like to go back to that.", "Ele roubava de brincadeira e devolvia. Acho que ele queria voltar pra isso."),
                ("He has my coin. I know it's mine. It has a dent where I bit it.", "Ele tá com a minha moeda. Eu sei que é minha. Tem a marca da minha mordida."),
                ("A leaf for a hat. Statistically it has not helped him once.", "Uma folha de chapéu. Estatisticamente não ajudou nenhuma vez."),
                ("He's so round! He has a leaf! I'm giving him all my coins!", "Ele é tão redondo! Ele tem uma folha! Vou dar todas as minhas moedas pra ele!"));

            // -- Monte Echeyde (67) --
            Page(entries, "enemy_mountain_tibicena",
                "The old people of Tenerife told that a dark thing lived inside the mountain they called Echeyde, and that black dogs, the tibicenas, came out of the earth at night and took the flocks. Those people are gone now and the tale is all that's left of them, so we tell it carefully. On our Echeyde the shepherds of the Abrigo blow their horns when the dogs come down. They are thin, all ribs and hunger, with embers where their eyes should be, and at night there are twice as many. Keep your back to a rock and never run for the fire alone.",
                "O povo antigo de Tenerife contava que uma coisa escura morava dentro da montanha que eles chamavam de Echeyde, e que uns cachorros pretos, os tibicenas, saíam da terra de noite e levavam os rebanhos. Esse povo já se foi e o conto é o que sobrou deles, então a gente conta com cuidado. No nosso Echeyde os pastores do Abrigo tocam a buzina quando os cachorros descem. São magros, só costela e fome, com brasa no lugar dos olhos, e de noite são o dobro. Fiquem com as costas numa pedra e nunca corram pra fogueira sozinhos.",
                ("I count them at dusk. Then I count them again and it's more.", "Eu conto eles no fim da tarde. Depois conto de novo e tem mais."),
                ("All ribs. Nobody's ever fed him enough. I don't think anybody can.", "Só costela. Nunca ninguém deu comida que chegasse. Acho que ninguém consegue."),
                ("I don't like these dogs. I like dogs. These I don't like.", "Eu não gosto desses cachorros. Eu gosto de cachorro. Desses eu não gosto."),
                ("Their ribs glow. Whatever is burning inside them isn't using any fuel.", "A costela deles brilha. O que queima lá dentro não tá gastando combustível nenhum."),
                ("That one is NOT a good boy. I checked.", "Esse NÃO é um bom menino. Eu confirmei."));

            Page(entries, "enemy_mountain_giant_lava_lizard",
                "Up on the malpaís, where the lava came down and froze in black waves, some lizards of the island grew as big as carts in the warm dark. The shepherds used to step around them and they would not even open an eye. The shadow got in where the plates of their backs don't meet, and now the lava runs in those seams and they cannot lie still anymore. They are slow until they are not. When the jaw drops open, you can see the fire in there: that is your warning, and it is the only one.",
                "Lá em cima do malpaís, onde a lava desceu e congelou em onda preta, alguns lagartos da ilha cresceram do tamanho de carroça no escuro quente. Os pastores passavam em volta e eles nem abriam o olho. A sombra entrou por onde as placas das costas não se encostam, e agora a lava corre nessas frestas e eles não conseguem mais ficar quietos. São devagar até não serem mais. Quando a boca abre, dá pra ver o fogo lá dentro: esse é o aviso, e é o único.",
                ("He turns like a boat. I walk around the stern.", "Ele vira feito barco. Eu dou a volta pela popa."),
                ("He slept here a hundred years and never hurt a soul. Let's help him sleep.", "Ele dormiu cem anos aqui e nunca fez mal a ninguém. Vamos ajudar ele a dormir."),
                ("I hit the plate. The plate won. I hit between the plates. Now he's mad and the plate won.", "Bati na placa. A placa ganhou. Bati entre as placas. Agora ele tá bravo e a placa ganhou."),
                ("Basalt plates, lava joints. He's a walking piece of the mountain.", "Placa de basalto, junta de lava. Ele é um pedaço de montanha andando."),
                ("I thought he was a rock! I sat on him! Sorry, rock!", "Eu achei que era pedra! Sentei nele! Desculpa, pedra!"));

            // -- Recife de Obsidiana (68) --
            Page(entries, "enemy_mountain_blade_fish",
                "Where the lava met the sea it turned to black glass, and the little fish that lived in the warm channels began to scrape themselves on it. Sango says that some of them got sharp. The shadow liked that, and now they swim in the steam above the channels as easily as in the water, thin as knives and so flat that from the front you can hardly see them. Don't try to see them from the front. Watch the steam, it moves before they do, and keep your legs off the ground if you can.",
                "Onde a lava encontrou o mar ela virou vidro preto, e os peixinhos que moravam nos canais quentes começaram a se esfregar nele. A Sango conta que alguns ficaram afiados. A sombra gostou disso, e agora eles nadam no vapor em cima dos canais tão fácil quanto na água, finos feito faca e tão chatos que de frente quase não dá pra ver. Não tentem ver de frente. Olhem o vapor, ele se mexe antes deles, e tirem as pernas do chão se der.",
                ("Thin from the front. So I don't shoot from the front.", "Fino de frente. Então eu não atiro de frente."),
                ("He scraped himself sharp because it was the only thing he knew to do.", "Ele se raspou até ficar afiado porque era a única coisa que sabia fazer."),
                ("He went through my cloak. Then through the other side of my cloak.", "Ele passou pela minha capa. Depois pelo outro lado da minha capa."),
                ("Obsidian edges are sharper than steel. I'd like that one for the lab. From a distance.", "Fio de obsidiana é mais afiado que aço. Eu queria esse pro laboratório. De longe."),
                ("He's a butter knife that swims! A MEAN butter knife!", "É uma faca de manteiga que nada! Uma faca de manteiga MALVADA!"));

            Page(entries, "enemy_mountain_ember_coral",
                "Coral is alive, little ones, even though it looks like stone: a whole village of tiny animals building one house. On the Obsidian Reef it grew around the hot vents, orange and glowing, and the lighthouse boat steered by it at night. The shadow told some of them that the village was too small and that the sea owed them more, so they let go of the rock and now they drift over the pools dragging their broken root and spitting embers at anything that passes. Get close: it can't turn quickly and it has no way to run.",
                "Coral é vivo, meus filhos, mesmo parecendo pedra: uma aldeia inteira de bichinhos construindo uma casa só. No Recife de Obsidiana ele cresceu em volta das fontes quentes, laranja e brilhando, e o Barco-Farol se guiava por ele de noite. A sombra disse a alguns deles que a aldeia era pequena demais e que o mar devia mais, então eles soltaram da pedra e agora vagam por cima das poças arrastando a raiz quebrada e cuspindo brasa em tudo que passa. Cheguem perto: ele não vira rápido e não tem como fugir.",
                ("It's slow to aim. I step left every time it glows.", "Ele demora pra mirar. Eu dou um passo pro lado toda vez que brilha."),
                ("A whole village, and it let go of home. I want to take it back to the rock.", "Uma aldeia inteira, e ela largou a casa. Eu quero levar de volta pra pedra."),
                ("It has no face and it hit me anyway. That's just rude.", "Não tem cara e me bateu assim mesmo. Isso é falta de educação."),
                ("Thousands of polyps coordinating a projectile. That's a very bad committee.", "Milhares de pólipos coordenando um projétil. É um comitê muito ruim."),
                ("It's a floating lantern tree that hates me!", "É uma árvore de lanternas voadora que me odeia!"));

            // -- Castelo das Nove Fornalhas (69) --
            Page(entries, "enemy_mountain_lacquer_ushabti",
                "An ushabti is a little figure the people of Kemet buried with the dead, to answer and do the work in the next world. The smiths of the Nine Furnaces copied the idea for their castle, tall figures of wood lacquered red, with gold bands and tongs in their hands, made to hand the hot iron along and never complain. They never asked to be anything else. The shadow took the Master of the Furnaces and they went on working without knowing why, and anyone who comes in now is just one more thing out of its place. They hit hard and don't tire. Their joints are lacquer over wood: fire doesn't scare them, but blows do.",
                "Um ushabti é uma figurinha que o povo de Kemet enterrava com os mortos, pra responder e fazer o trabalho no outro mundo. Os ferreiros das Nove Fornalhas copiaram a ideia pro castelo deles, figuras altas de madeira laqueada de vermelho, com faixas de ouro e tenaz na mão, feitas pra passar o ferro quente adiante e nunca reclamar. Elas nunca pediram pra ser outra coisa. A sombra levou o Mestre das Fornalhas e elas continuaram trabalhando sem saber por quê, e quem entra agora é só mais uma coisa fora do lugar. Batem forte e não cansam. As juntas são laca sobre madeira: fogo não assusta, pancada sim.",
                ("It stops to reset between swings. One, two, shoot.", "Ele para pra se arrumar entre os golpes. Um, dois, atiro."),
                ("It's been working for years and nobody has told it that it can stop. I'll tell it.", "Faz anos que ele trabalha e ninguém disse que pode parar. Eu vou dizer."),
                ("He tried to hand me to the next one with the tongs. Like a piece of iron. Me.", "Ele tentou me passar pro outro com a tenaz. Feito pedaço de ferro. Eu."),
                ("Red lacquer, thirty coats at least. That's a very well-made servant.", "Laca vermelha, trinta demãos no mínimo. É um criado muito bem feito."),
                ("His hat is taller than his head! Who gave him that hat?", "O chapéu dele é maior que a cabeça! Quem deu esse chapéu pra ele?"));

            Page(entries, "enemy_mountain_furnace_ogre",
                "No book tells of these, little ones, only the people who carried coal in the Castle. They say the ogres were the big stokers of the Nine Furnaces, the ones who could stand the heat closest to the door, and that they sang while they worked. The shadow told them that the fire should be kept inside instead of fed to the iron, and the furnace went into their bellies. Now they walk the rings of the castle with the door of the fire in their chests, swinging their hammers at whatever might put it out. They are slow to swing and terrible when they do. Mind the shoulders, they go up before the hammer comes down.",
                "Nenhum livro fala deles, meus filhos, só o povo que carregava carvão no Castelo. Dizem que os ogros eram os grandes foguistas das Nove Fornalhas, os que aguentavam o calor mais perto da porta, e que cantavam enquanto trabalhavam. A sombra disse a eles que o fogo devia ficar guardado em vez de ser dado pro ferro, e a fornalha foi parar na barriga deles. Agora eles andam pelos anéis do castelo com a porta do fogo no peito, batendo o martelo em tudo que possa apagar. Demoram pra bater e são terríveis quando batem. Olhem os ombros, eles sobem antes do martelo descer.",
                ("Shoulders up, I step left. Every time. He hasn't learned.", "Ombro subiu, eu dou um passo pro lado. Toda vez. Ele não aprendeu."),
                ("They used to sing while they worked. I'd like to hear that again.", "Eles cantavam enquanto trabalhavam. Eu queria ouvir isso de novo."),
                ("I blocked one. I'd like to stop blocking now, please.", "Eu defendi um. Agora eu queria parar de defender, por favor."),
                ("A furnace in the abdomen needs air intake. That's what the shoulder chimneys are for.", "Uma fornalha no abdome precisa entrada de ar. É pra isso as chaminés do ombro."),
                ("He has a fire in his tummy and it still doesn't warm him up.", "Ele tem um fogo na barriga e mesmo assim não esquenta."));

            // -- A Caldeira do Fogo Devorador (70) --
            Page(entries, "enemy_mountain_slag_giant",
                "Slag is what is left over when the fire has taken what it wanted from the ore: the part nobody wants, thrown on the heap. For as long as there have been forges at Hinokura the heap has grown, out past the castle and up against the caldera. The Devouring Fire looked at all that was thrown away and made it walk. The Slag Giants don't hate you: they only want to be something again, and they will take it from whoever is standing nearest. Remember, little ones, that there is no such thing as rubbish in the Light. Hit the seams where the lava shows and be quick, because a heap does not get tired.",
                "Escória é o que sobra quando o fogo pegou o que queria do minério: a parte que ninguém quer, jogada no monte. Desde que tem forja em Hinokura o monte cresce, passando do castelo e encostando na caldeira. O Fogo Devorador olhou tudo aquilo que foi jogado fora e fez andar. Os Gigantes de Escória não têm raiva de vocês: eles só querem voltar a ser alguma coisa, e pegam isso de quem estiver mais perto. Lembrem, meus filhos, que na Luz não existe lixo. Batam nas frestas onde a lava aparece e sejam rápidos, porque monte não cansa.",
                ("Huge target. I'm going for the bright bits.", "Alvo enorme. Vou nos pedaços que brilham."),
                ("Everything anyone ever threw away, walking. He only wants someone to look at him.", "Tudo que alguém já jogou fora, andando. Ele só quer que alguém olhe pra ele."),
                ("He picked me up and looked at me. Then he put me down. I don't know which was worse.", "Ele me pegou e olhou pra mim. Depois me pôs no chão. Não sei o que foi pior."),
                ("Calcium silicates, iron oxide and no reason at all to be walking.", "Silicato de cálcio, óxido de ferro e nenhum motivo pra estar andando."),
                ("He's a big pile of grumpy! Why does he smell like Grandpa Jorge's frying pan?", "Ele é um montão de mau humor! Por que ele tem cheiro da frigideira do Vô Jorge?"));
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
