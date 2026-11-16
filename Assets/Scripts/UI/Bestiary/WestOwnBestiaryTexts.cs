using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve most-seen roles of the Costa das Monções (Região 5, Docs/Mapas100.md 28–30), the Delta de
    /// Jhilpur (Região 6, 31–35), the Planície de Mingcheng (Região 8, 42–44) and the Estepe dos Nartes (Região 9, 51) that the overworld still
    /// played with another beast (bible §3.2.1, twenty-fourth pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart
    /// like <see cref="NorthOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore
    /// (bible §4.3).
    /// </summary>
    public static class WestOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_mud_shark", "enemy_cave_smoke_ray", "enemy_forest_dimmed_heron", "enemy_forest_mud_gharial",
            "enemy_forest_ninki_hatchling", "enemy_cave_rotten_root", "enemy_cave_fish_lantern", "enemy_mountain_smoke_boatman",
            "enemy_forest_mandarin_duck", "enemy_desert_dimmed_scarecrow", "enemy_mountain_crooked_ray", "enemy_forest_grey_wolf",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Jardins de Alga (28) --
            Page(entries, "enemy_desert_mud_shark",
                "In the kelp gardens of Kirta the sharks were the sweepers of the reef, and the pearl divers swam beside them without a second look. Then the Lodo of the Delta came down the coast with the monsoon, and the sharks lay in it to cool off. It dried on them grey, and the shadow came in with it. Now they cut through the kelp fast and low, and turn on whoever swims there. They charge in a straight line: step aside, and hit them as they go past.",
                "Nos jardins de alga de Kirta os tubarões eram os varredores do recife, e os mergulhadores de pérola nadavam do lado deles sem nem olhar duas vezes. Aí o Lodo do Delta desceu a costa com a monção, e os tubarões deitaram nele pra se refrescar. Secou neles cinza, e a sombra entrou junto. Agora eles cortam a alga rápidos e rentes, e se viram contra quem nada ali. Investem em linha reta: deem um passo pro lado e batam quando eles passarem.",
                ("It only charges straight. Step aside and hit.", "Ele só vai reto. Sai do caminho e bate."),
                ("Under the mud it's still a sweeper of the reef. Let's wash it.", "Debaixo do lodo ainda é varredor do recife. Vamos lavar ele."),
                ("I said shark in the kelp, nobody believed me. Now they believe me.", "Eu falei que tinha tubarão na alga, ninguém acreditou. Agora acreditam."),
                ("The Lodo doesn't bite by itself. It needs someone to carry it. That's what the shadow does with everyone.", "O Lodo não morde sozinho. Precisa de alguém pra carregar. É o que a sombra faz com todo mundo."),
                ("It's grey like a rain cloud! A rain cloud with teeth!", "Ele é cinza que nem nuvem de chuva! Uma nuvem de chuva com dente!"));

            // -- Kilwa Afundada (29) --
            Page(entries, "enemy_cave_smoke_ray",
                "When Kilwa went under the sea, the rays came in over the streets and lay on the warm stones of the houses, and the people of the reef said they were keeping the city company. The shadow blew its smoke in through the windows, and the rays breathed it. Now they glide through the drowned rooms like smoke themselves, and spit the light they swallowed at whoever swims in. They shoot from far off: get under them, where the tail cannot turn.",
                "Quando Kilwa afundou, as raias entraram por cima das ruas e deitaram nas pedras quentes das casas, e o povo do recife dizia que elas faziam companhia pra cidade. A sombra soprou a fumaça dela pelas janelas, e as raias respiraram. Agora elas deslizam pelas salas afogadas que nem fumaça, e cospem a luz que engoliram em quem entra nadando. Atiram de longe: fiquem debaixo delas, onde a cauda não alcança.",
                ("It spits from far. Get under it.", "Cospe de longe. Fica embaixo dela."),
                ("They were keeping the city company. They still are, in their way.", "Elas faziam companhia pra cidade. Ainda fazem, do jeito delas."),
                ("I thought it was a flying carpet. It was not a flying carpet.", "Eu achei que era um tapete voador. Não era um tapete voador."),
                ("What she spits is light she swallowed. Every time we beat one, a little of it goes home.", "O que ela cospe é luz que ela engoliu. Cada vez que a gente vence uma, um pouco volta pra casa."),
                ("It flies in the water and swims in the air! Both!", "Ela voa na água e nada no ar! Os dois!"));

            // -- Canais de Liyongo (30) --
            Page(entries, "enemy_forest_dimmed_heron",
                "The white herons of Liyongo stood in the canals so still that the fishermen tied their boats to them by mistake, and the herons never minded. The shadow put out the white of their feathers first, and then their patience. Now they stand grey in the mangroves and stab at anything that moves on the water, quick as a blink. Wait for the stab, then close in: the long bill is slow to come back.",
                "As garças brancas de Liyongo ficavam tão paradas nos canais que os pescadores amarravam o barco nelas por engano, e as garças nem ligavam. A sombra apagou primeiro o branco das penas, e depois a paciência. Agora elas ficam cinza no mangue e espetam qualquer coisa que mexe na água, rápidas que nem piscar. Esperem a bicada e cheguem perto: o bico comprido demora pra voltar.",
                ("Wait for the stab. Then step in.", "Espera a bicada. Aí entra."),
                ("They were so patient. I want to give them that back.", "Elas eram tão pacientes. Eu quero devolver isso pra elas."),
                ("I tried to tie my hammock to one. Don't do that.", "Eu tentei amarrar minha rede numa. Não façam isso."),
                ("White is all the colours of the light together. Put it out, and you get grey.", "O branco é todas as cores da luz juntas. Apaga, e vira cinza."),
                ("It stands on one leg! I can stand on one leg too! Look!", "Ela fica num pé só! Eu também fico! Olha!"));

            // -- Lago de Jhilpur, Charcos de Lótus, Floresta de Maré (31–32) --
            Page(entries, "enemy_forest_mud_gharial",
                "The gharials of Jhilpur's lake only ever ate fish, with those long thin jaws of theirs, and the children of the city swam right beside them. When the Lodo came up the delta it lay on them so long that it got in through the scales. Now they lie in the shallows with their thin jaws open, and they wait for anything at all. They are heavy and slow on the bank, quick in the water: fight them on dry ground.",
                "Os gaviais do lago de Jhilpur só comiam peixe, com aquelas mandíbulas compridas e finas, e as crianças da cidade nadavam do lado deles. Quando o Lodo subiu o delta, ficou tanto tempo em cima deles que entrou pelas escamas. Agora eles deitam no raso de boca aberta e esperam qualquer coisa. São pesados e lentos na margem, rápidos na água: lutem com eles no seco.",
                ("Pull it onto the bank. It's slow there.", "Puxa ele pra margem. Lá ele é lento."),
                ("It only ate fish. Its jaw isn't made for anything else.", "Ele só comia peixe. A boca dele não é feita pra outra coisa."),
                ("The knob on its nose looks like a little pot. I did not say that to its face.", "A bolinha no nariz parece um potinho. Eu não falei isso na cara dele."),
                ("Jhilpur is the Living Water. Even the mud there remembers it was water once.", "Jhilpur é a Água Viva. Até o lodo de lá lembra que já foi água."),
                ("Its mouth is so long! It could eat a whole noodle without biting!", "A boca dele é tão comprida! Comia um macarrão inteiro sem morder!"));

            // -- Charco do Ninki Nanka, Toca da Dragoa (34) --
            Page(entries, "enemy_forest_ninki_hatchling",
                "The Ninki Nanka of the bog was a dragon who guarded the water and never left it, and the stories say whoever saw her face did not come back the same. Her young hatched in the warm mud after the shadow came, and they guard nothing yet: they only bite, and they are bold, and the mirrors on their backs flash in the sun. Do not stare at the flash. Fight with the sun behind you.",
                "A Ninki Nanka do charco era uma dragoa que guardava a água e nunca saía dela, e as histórias dizem que quem via a cara dela não voltava o mesmo. Os filhotes dela nasceram no lodo quente depois que a sombra chegou, e ainda não guardam nada: só mordem, e são atrevidos, e os espelhos nas costas brilham no sol. Não fiquem olhando o brilho. Lutem com o sol nas costas.",
                ("Don't look at the flash. Sun behind you.", "Não olha o brilho. Sol nas costas."),
                ("They're only babies. Their mother would want them home.", "São só filhotes. A mãe deles ia querer eles em casa."),
                ("One looked at me and I forgot what I was saying. What was I saying?", "Um olhou pra mim e eu esqueci o que tava falando. O que eu tava falando?"),
                ("Guarding is what they were born for. The shadow only taught them the biting half.", "Guardar é pra isso que eles nasceram. A sombra só ensinou a metade de morder."),
                ("It has little horns! And a frill! It's a dragon puppy!", "Tem chifrinho! E uma golinha! É um cachorrinho dragão!"));

            // -- Mata Morta, Coração do Lodo (35) --
            Page(entries, "enemy_cave_rotten_root",
                "Under the Heart of the Mire the roots of the old trees drank the black water until they rotted. Some of them broke off and did not fall: a cloud of spores holds them up, and they drift through the dead wood spitting splinters at whatever is still alive. There is a light rotting inside each knot, the light the root drank. They shoot from far off; close in fast, and mind the spores, they make you cough.",
                "Debaixo do Coração do Lodo as raízes das árvores velhas beberam a água preta até apodrecer. Algumas quebraram e não caíram: uma nuvem de esporos segura elas no ar, e elas vagam pela mata morta cuspindo lascas em tudo que ainda está vivo. Tem uma luz apodrecendo dentro de cada nó, a luz que a raiz bebeu. Atiram de longe; cheguem perto rápido, e cuidado com os esporos, dão tosse.",
                ("It shoots splinters. Run in fast.", "Ela atira lasca. Corre pra perto."),
                ("A tree drank that light to live. Let's give it back to the forest.", "Uma árvore bebeu essa luz pra viver. Vamos devolver pra mata."),
                ("I sneezed so hard my hat flew off. Twice.", "Eu espirrei tão forte que meu chapéu voou. Duas vezes."),
                ("Rot is only the light leaving slowly. We can make it leave fast, and go up.", "Podre é só a luz indo embora devagar. A gente pode fazer ela ir rápido, e subir."),
                ("A root that floats! Where's its tree?", "Uma raiz que flutua! Cadê a árvore dela?"));

            // -- Canal das Lanternas, Eclusa da Lua (43) --
            Page(entries, "enemy_cave_fish_lantern",
                "On the Lantern Canal the children of Mingcheng carry paper carps on sticks, a candle inside each one, so the real carps will remember to jump the falls. The shadow blew out the candles and put a stolen light in their place, and the lanterns swam off their sticks all by themselves. Now they bob under the bridges in crowds and spit their light at the barges. They shoot from far off: they are only paper, so one good hit is enough.",
                "No Canal das Lanternas as crianças de Mingcheng levam carpas de papel na ponta de um pau, com uma vela dentro, pra as carpas de verdade lembrarem de pular a cachoeira. A sombra apagou as velas e pôs uma luz roubada no lugar, e as lanternas saíram nadando sozinhas dos paus. Agora ficam boiando debaixo das pontes em bando e cospem a luz nas barcas. Atiram de longe: são só papel, uma boa batida resolve.",
                ("Paper. One good hit.", "Papel. Uma batida boa."),
                ("Some child made each one of these. I'll take them back with care.", "Alguma criança fez cada uma dessas. Eu levo de volta com cuidado."),
                ("I tried to hang one in our room. It spat at me.", "Eu tentei pendurar uma no nosso quarto. Ela cuspiu em mim."),
                ("The light inside isn't theirs. When it goes free, the paper is just a lantern again.", "A luz de dentro não é delas. Quando ela sai livre, o papel volta a ser só lanterna."),
                ("They're so pretty! Can we keep one? A nice one?", "Elas são tão bonitas! Dá pra ficar com uma? Uma boazinha?"));

            Page(entries, "enemy_mountain_smoke_boatman",
                "The boatmen of the canal took the crafts from ring to ring at night, singing so the barges would not bump into each other in the dark. The shadow took the boats and the songs, and left the boatmen: smoke in a straw hat, still pushing a pole through the water, looking for a barge to steer. They swing the pole hard, both hands up first. When the hands go up, step back; then hit the smoke before it gathers again.",
                "Os barqueiros do canal levavam os ofícios de anel em anel de noite, cantando pra as barcas não baterem umas nas outras no escuro. A sombra levou os barcos e as canções, e deixou os barqueiros: fumaça de chapéu de palha, ainda empurrando uma vara na água, procurando uma barca pra levar. Eles batem com a vara com força, as duas mãos pra cima primeiro. Quando as mãos subirem, deem um passo pra trás; depois batam na fumaça antes que ela se junte de novo.",
                ("Hands up means the pole comes down. Step back.", "Mão pra cima, a vara desce. Pra trás."),
                ("They're still working. Nobody told them they could rest.", "Eles ainda tão trabalhando. Ninguém disse que eles podiam descansar."),
                ("I asked him for a ride. He said no with the pole.", "Eu pedi uma carona. Ele disse não com a vara."),
                ("A song is light you can hear. That's why the shadow took the songs first.", "Canção é luz que a gente escuta. Por isso a sombra levou as canções primeiro."),
                ("He has a big hat! I want a big hat!", "Ele tem um chapéu grandão! Eu quero um chapéu grandão!"));

            // -- Rio dos Salgueiros, Arrozais Espelhados (44) --
            Page(entries, "enemy_forest_mandarin_duck",
                "The mandarin drakes of the willow river were the good luck of brides: a pair carved in wood went into every new house in Mingcheng. The shadow made the live ones thieves, and now they snatch the rings and the coins off the boats and the paddies, and bite the hand that tries to take them back. They are quick and they peck in a flurry. Corner them against the water, they hate to get their feathers wet.",
                "Os patos-mandarins do rio dos salgueiros eram a sorte das noivas: um casal entalhado em madeira ia pra toda casa nova de Mingcheng. A sombra fez dos vivos uns ladrões, e agora eles pegam os anéis e as moedas dos barcos e dos arrozais, e bicam a mão que tenta pegar de volta. São rápidos e bicam de enxurrada. Encurralem eles contra a água, eles odeiam molhar as penas.",
                ("Corner it by the water. Then strike.", "Encurrala na beira d'água. Aí bate."),
                ("They were meant to come in pairs. Where's its partner?", "Eles eram pra andar em par. Cadê o par dele?"),
                ("It stole my button. I only had the one.", "Ele roubou meu botão. Eu só tinha esse."),
                ("A thief always thinks the shine is in the ring. The shine is in whoever gives it.", "Ladrão sempre acha que o brilho tá no anel. O brilho tá em quem dá."),
                ("It's every colour! Like a box of crayons with wings!", "Ele é de todas as cores! Que nem uma caixa de lápis de cor com asa!"));

            Page(entries, "enemy_desert_dimmed_scarecrow",
                "The scarecrows of the mirror paddies kept the sparrows off the rice, and kept a lantern lit on their arm so the ducks could find their way home at dusk. The shadow got into the straw. Now they keep the people off, and the lantern shows them where to throw. They throw from far off and they never move from their pole, so come in from the side and knock the pole out from under them.",
                "Os espantalhos dos arrozais espelhados espantavam os pardais do arroz, e deixavam uma lanterna acesa no braço pra os patos acharem o caminho de casa no fim da tarde. A sombra entrou na palha. Agora eles espantam as pessoas, e a lanterna mostra onde jogar. Jogam de longe e nunca saem do pau deles, então cheguem pelo lado e derrubem o pau debaixo deles.",
                ("It never leaves its pole. Come from the side.", "Ele nunca sai do pau. Chega pelo lado."),
                ("It kept the lantern for the ducks. That's a kind thing to be made for.", "Ele segurava a lanterna pros patos. É uma coisa boa de ter sido feito pra isso."),
                ("I said hello to it. I always say hello to scarecrows. This one answered.", "Eu dei oi pra ele. Eu sempre dou oi pra espantalho. Esse respondeu."),
                ("Straw holds the sun of a whole summer. No wonder the shadow wanted in.", "A palha guarda o sol de um verão inteiro. Não é à toa que a sombra quis entrar."),
                ("It's like a person but made of hay! Is it ticklish?", "É que nem gente mas de palha! Ele sente cócega?"));

            // -- Jardins em Terraço, Torres Brancas (42) --
            Page(entries, "enemy_mountain_crooked_ray",
                "The White Towers send their light out straight, from spire to spire, so the light of the city always finds its way home. The shadow held a shard of smoked glass in the path of one ray, and it came out bent. Now it wanders the terraces crooked and lost, and whoever it touches cannot see for a moment. When the glare in its heart flares, look away and step back; then break the glass.",
                "As Torres Brancas mandam a luz delas reta, de torre em torre, pra a luz da cidade sempre achar o caminho de casa. A sombra pôs um caco de vidro esfumaçado no caminho de um raio, e ele saiu torto. Agora ele anda pelos terraços torto e perdido, e quem ele toca fica sem ver por um instante. Quando o brilho no meio dele acender, olhem pro lado e deem um passo pra trás; depois quebrem o vidro.",
                ("When it flares, look away. Then break the glass.", "Quando ele acender, olha pro lado. Depois quebra o vidro."),
                ("It's lost. It only wants to go back to its tower.", "Ele tá perdido. Só quer voltar pra torre dele."),
                ("It blinded me and I walked into a hedge. A very prickly hedge.", "Ele me cegou e eu entrei numa moita. Uma moita bem espinhenta."),
                ("Light always goes straight. Only the shadow makes it go round. Take the shard away and it remembers.", "A luz sempre vai reta. Só a sombra faz ela dar volta. Tira o caco e ela lembra."),
                ("It zigzags! Like lightning drawn by a kid!", "Ele vai em zigue-zague! Que nem raio desenhado por criança!"));

            // -- Pradaria Ondulada e a estepe (47–51) --
            Page(entries, "enemy_forest_grey_wolf",
                "The grey wolves of the prairie ran with the wild horses and took only the old and the lame, and the Nartes left them alone for it. Since the Destroying Wind came, it blows the wrong way through their fur, and they run in packs at the herds and at the riders too. They come in a crowd and circle. Keep your back to a rock or a friend, and the circle has nowhere to close.",
                "Os lobos-cinzentos da pradaria corriam com os cavalos selvagens e só pegavam os velhos e os mancos, e os Nartes deixavam eles em paz por isso. Desde que o Vento Destruidor chegou, ele sopra ao contrário no pelo deles, e eles correm em bando atrás das manadas e dos cavaleiros também. Chegam em bando e cercam. Fiquem de costas pra uma pedra ou pra um amigo, e o cerco não tem onde fechar.",
                ("Back to a rock. They can't close the circle.", "Costas pra pedra. Eles não fecham o cerco."),
                ("They only took the old and the lame. They used to be fair.", "Eles só pegavam os velhos e os mancos. Eles eram justos."),
                ("I howled at them. They howled back. It was a whole conversation.", "Eu uivei pra eles. Eles uivaram de volta. Foi uma conversa inteira."),
                ("The Destroying Wind can't make a wolf. It can only blow the fur the wrong way.", "O Vento Destruidor não consegue fazer um lobo. Só consegue soprar o pelo ao contrário."),
                ("Their hair is all messy! They need a comb!", "O cabelo deles tá todo bagunçado! Precisam de um pente!"));
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
