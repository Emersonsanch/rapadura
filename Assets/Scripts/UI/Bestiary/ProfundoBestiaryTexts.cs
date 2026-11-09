using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Limiar Profundo (Docs/Mapas100.md 84–90) that the overworld layout used to play
    /// with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="MisturaBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Cold, sticky and noisy, never gore (bible §4.3);
    /// the Prince of Darkness only ever as a shape behind the seal (bible §1.4), and the archons are story giants who swallowed too much light.
    /// </summary>
    public static class ProfundoBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_copy_shadow", "enemy_forest_smoke_giant", "enemy_forest_smoke_archer", "enemy_forest_ember_wolf",
            "enemy_forest_hot_smoke_lion", "enemy_forest_black_eagle", "enemy_forest_wind_harpy", "enemy_forest_mud_octopus",
            "enemy_forest_dark_fish", "enemy_forest_shadow_lizard", "enemy_forest_lesser_shadow_dragon", "enemy_forest_lesser_archon",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Cova da Fumaça (84): the ones on two legs --
            Page(entries, "enemy_forest_copy_shadow",
                "Under the Fortress there are five pits, and the old lists say each one keeps its own kind: the first is Smoke, and Smoke keeps the ones that walk on two legs. Pamour says the copies have no shape of their own, so they wait by the vents and borrow yours: how you stand, which shoulder you favour, the little step you do before you swing. It is unpleasant to be imitated by something with no face. So don't do what you always do. Change hands, change feet, and it will stand there a moment not knowing who it is meant to be.",
                "Debaixo da Fortaleza há cinco covas, e as listas antigas dizem que cada uma guarda a sua gente: a primeira é a Fumaça, e a Fumaça guarda os que andam em duas pernas. O Pamour diz que as cópias não têm forma nenhuma, então ficam perto dos ventiladores e pegam a de vocês: o jeito de ficar de pé, o ombro preferido, aquele passinho antes do golpe. É desagradável ser imitado por uma coisa sem rosto. Então não façam o de sempre. Troquem de mão, troquem de pé, e ela vai ficar um instante parada sem saber quem deve ser.",
                ("It copies your opening. Open with something else.", "Ela copia sua abertura. Abre com outra coisa."),
                ("It stands like me. I don't like how tired that looks.", "Ela fica de pé igual a mim. Não gosto de como isso parece cansado."),
                ("I made a funny face. Nothing. Worst mirror ever.", "Fiz careta. Nada. Pior espelho da vida."),
                ("A copy needs an original to compare. It never gets to.", "Cópia precisa do original pra comparar. Ela nunca consegue."),
                ("It has my hair! It doesn't have my hair. It tried.", "Ela tem meu cabelo! Não tem. Mas tentou."));

            Page(entries, "enemy_forest_smoke_giant",
                "Smoke does not usually stay in one place, which is the whole point of smoke. These forgot. They stacked up in the corridors ring on ring until the top could reach the bottom of the next floor, and then they went for a walk. A blade goes in and comes out again and the giant barely notices. Tehat tells the guards to start the old vents of the Artificers first and fight after: wind is the only thing that gets through to it. If you see the amber low in its chest, that's the lamp it walked over. It would like another.",
                "Fumaça costuma não ficar num lugar só, é a graça da fumaça. Estes esqueceram. Foram se empilhando nos corredores, anel em cima de anel, até a ponta alcançar o teto, e aí saíram pra dar uma volta. A lâmina entra, sai de novo, e o gigante mal percebe. A Tehat manda os guardas ligarem primeiro os ventiladores antigos dos Artífices e só depois lutar: vento é a única coisa que chega nele. O âmbar lá embaixo no peito é a lâmpada em que ele pisou. Ele queria outra.",
                ("Vents first. Then hit whatever is left.", "Ventilador primeiro. Depois bate no que sobrar."),
                ("He doesn't know he's in the way. That's the sad part.", "Ele não sabe que está atrapalhando. É a parte triste."),
                ("I punched him and now my hand smells like a barbecue.", "Dei um soco nele e agora minha mão cheira a churrasco."),
                ("Rings of smoke hold their shape on spin alone. Stop the spin.", "Anel de fumaça só segura a forma girando. Parem o giro."),
                ("He's so tall he has clouds on his head. Inside clouds.", "Ele é tão alto que tem nuvem na cabeça. Nuvem de dentro."));

            Page(entries, "enemy_forest_smoke_archer",
                "The Artificers put soldiers in the first pit once, a long time ago, and when the vents were opened the soldiers were blown clean out of their clothes. What stayed behind remembered the bow. It floats in the far corner of the hall, where the smoke is thickest, and it does not need to breathe to aim. The arrow is only hot smoke with somewhere to go, but it bends a little on the way, so stepping aside once isn't enough. Get close: it has no idea what to do with anyone close.",
                "Os Artífices puseram soldados na primeira cova, faz muito tempo, e quando abriram os ventiladores os soldados saíram voando de dentro da roupa. O que ficou se lembrava do arco. Ele flutua no canto mais longe do salão, onde a fumaça é mais grossa, e não precisa respirar pra mirar. A flecha é só fumaça quente com destino, mas entorta um pouco no caminho, então desviar uma vez só não basta. Cheguem perto: ele não faz ideia do que fazer com gente perto.",
                ("The arrow curves. Sidestep late.", "A flecha faz curva. Desvia tarde."),
                ("He's still on duty. Nobody told him it ended.", "Ele ainda está de plantão. Ninguém avisou que acabou."),
                ("Bow, arrow, no arms. I respect the commitment.", "Arco, flecha, nenhum braço. Respeito o compromisso."),
                ("Copper bow, the Artificers' pattern. It's older than the Fortress.", "Arco de cobre, padrão dos Artífices. É mais velho que a Fortaleza."),
                ("He shot at me and it smelled like Vó's stove!", "Ele atirou em mim e cheirou a fogão da Vó!"));

            // -- Cova do Fogo Devorador (85): the ones on four legs --
            Page(entries, "enemy_forest_ember_wolf",
                "The second pit is the Devouring Fire, and that one keeps whatever goes on four legs. I know wolves. Wolves are sensible people who have a family and a territory and would much rather not meet you. These ones have nothing but the warm. They go where the embers go, and when the floor lights up in its patterns they run to it and lie down in it. Makarios says you can tell how long they have been down there by how far the fire has climbed up the spine. Stand on the dark stones and let them come to you. They are quick, but they are not clever.",
                "A segunda cova é o Fogo Devorador, e essa guarda quem anda em quatro patas. Eu conheço lobo. Lobo é gente sensata, tem família, tem território e prefere não encontrar vocês. Estes só têm o quentinho. Vão pra onde a brasa vai, e quando o chão acende nos desenhos eles correm e deitam em cima. O Makarios diz que dá pra saber há quanto tempo estão lá embaixo por quanto o fogo já subiu pela espinha. Fiquem nas pedras escuras e deixem eles virem. São rápidos, mas não são espertos.",
                ("Dark stones. Make them cross the bright ones.", "Pedra escura. Faz eles atravessarem a clara."),
                ("He looks cold. All that fire and he looks cold.", "Ele parece com frio. Tanto fogo e parece com frio."),
                ("Good boy. Bad boy. Hot boy. Ow.", "Bom menino. Mau menino. Menino quente. Ai."),
                ("Fire on the spine only: it's feeding from the top down.", "Fogo só na espinha: ele queima de cima pra baixo."),
                ("Can we warm our hands on him? Just a little bit?", "A gente pode esquentar a mão nele? Só um pouquinho?"));

            Page(entries, "enemy_forest_hot_smoke_lion",
                "My father used to say there is a lion in every story because somebody has to be in charge of being frightening. This one is basalt on the outside and furnace on the inside, and when it breathes out the whole hall goes a shade darker for a while. The lamps do not like it. Pamour keeps a count of the wicks it has put out by breathing near them, and he reads me the number in his letters as if it were the weather. It has a heavy way of walking, so you hear it first. When you do, go somewhere with a wall behind you and aim for the seams in its side, where the fire shows.",
                "Meu pai dizia que em toda história tem um leão porque alguém precisa ficar encarregado de dar medo. Este é basalto por fora e fornalha por dentro, e quando ele solta o ar o salão inteiro fica um tom mais escuro por um tempo. As lâmpadas não gostam. O Pamour conta os pavios que ele apagou só de respirar perto, e me escreve o número nas cartas como quem fala do tempo. Ele anda pesado, então vocês escutam primeiro. Quando escutarem, procurem uma parede pra ficar de costas e mirem nas frestas do flanco, onde o fogo aparece.",
                ("Hit the seams. The fire is under them.", "Bate nas frestas. O fogo está embaixo."),
                ("He put out a lamp and didn't even look at it.", "Ele apagou uma lâmpada e nem olhou pra ela."),
                ("His breath is awful. Someone should tell him. Not me.", "O bafo dele é horrível. Alguém devia avisar. Eu não."),
                ("Basalt holds heat for days. He's a walking oven.", "Basalto segura calor por dias. Ele é um forno que anda."),
                ("His hair is on fire and he's not even running!", "O cabelo dele está pegando fogo e ele nem corre!"));

            // -- Cova do Vento Destruidor (86): the ones that fly --
            Page(entries, "enemy_forest_black_eagle",
                "The third pit is the Ruinous Wind, and there the flying ones live, all up and down a shaft you could lose a whole village in. The old story gives the Prince of Darkness five shapes, and one of them is the wings of an eagle. This is not that. This is an ordinary eagle that flew too near and came away with the color, and I feel for it, because it still hunts the way an eagle should: from high up, and only once. If you hear nothing at all, look up. Move sideways the moment it folds, and hit it when it comes up off the stone, when it is cross with itself.",
                "A terceira cova é o Vento Destruidor, e lá moram os que voam, de cima a baixo num poço onde caberia uma vila inteira. A história antiga dá cinco formas ao Príncipe das Trevas, e uma delas é asa de águia. Esta não é aquela. Esta é uma águia comum que voou perto demais e saiu com a cor, e eu tenho pena, porque ela ainda caça como águia deve caçar: lá do alto, e uma vez só. Se não ouvirem nada, olhem pra cima. Saiam de lado quando ela fechar as asas, e batam quando ela levantar da pedra, zangada consigo mesma.",
                ("Dodge on the fold, not the dive.", "Desvia quando fecha a asa, não quando mergulha."),
                ("She's just a bird. She doesn't know what she's wearing.", "Ela é só uma ave. Não sabe o que está vestindo."),
                ("Nice wings. Did they come in any other color?", "Bonita asa. Tinha em outra cor?"),
                ("Eagles stoop at over a hundred an hour. Do the sums, then move.", "Águia mergulha a mais de cem por hora. Faz a conta e sai."),
                ("Her eyes are white! She can't see me if I'm quiet!", "O olho dela é branco! Ela não me vê se eu ficar quieta!"));

            Page(entries, "enemy_forest_wind_harpy",
                "When a strong wind takes what it wants from a bird, what's left is cross about it. That is more or less how Tehat explains the harpies, and she has been darning their tears in the canvas for forty years. They hang in the rising air on the far side of the gap and pull out their own feathers, sharp end first, and throw them at you with a good deal of commentary. It is a very rude sound. They run out of patience faster than feathers, so stay behind the columns of the platforms and walk up when they stop to shriek.",
                "Quando um vento forte leva o que quer de uma ave, o que sobra fica aborrecido. É mais ou menos assim que a Tehat explica as harpias, e ela remenda os rasgos delas na lona há quarenta anos. Ficam penduradas no ar que sobe, do outro lado do vão, arrancam as próprias penas, ponta primeiro, e jogam em vocês com muito comentário. É um som muito mal-educado. Elas perdem a paciência antes de perder as penas, então fiquem atrás das colunas das plataformas e cheguem perto quando pararem pra gritar.",
                ("Cover first. Walk up when she screams.", "Cobertura primeiro. Avança quando ela grita."),
                ("She's pulling them out herself. I wish she wouldn't.", "Ela mesma arranca. Queria que não fizesse isso."),
                ("I'm keeping this feather. It's sharper than my knife.", "Vou ficar com essa pena. É mais afiada que minha faca."),
                ("Wind from below doubles her throw. Fight her from above.", "Vento de baixo dobra o lançamento. Enfrenta ela por cima."),
                ("She called me something! I don't know what! It was long!", "Ela me chamou de alguma coisa! Não sei o quê! Foi comprido!"));

            // -- Cova do Lodo (87): the ones that swim --
            Page(entries, "enemy_forest_mud_octopus",
                "The fourth pit is the Mire, black water and worse, and the swimming ones keep it. This creature gave up on swimming some time ago and just lies there being the floor. You will take it for a mound until a mound takes hold of your ankle. There are eight arms and it uses them all for holding, and a little amber glows in the top of it from the lamp oil it has drunk out of the sluices. Do not get in the water to reach it. Stand back on the stone, make it stretch, and strike where the arms join.",
                "A quarta cova é o Lodo, água preta e coisa pior, e quem guarda são os que nadam. Este bicho desistiu de nadar faz tempo e fica só deitado sendo o chão. Vocês vão achar que é um monte de barro até um monte de barro segurar o tornozelo de vocês. São oito braços e ele usa todos pra segurar, e um pouco de âmbar brilha lá em cima, do óleo de lâmpada que ele bebeu das comportas. Não entrem na água pra alcançar. Fiquem na pedra, façam ele esticar, e batam onde os braços se juntam.",
                ("Stay on the stone. Make him reach.", "Fica na pedra. Faz ele esticar."),
                ("He only wants to hold on to something.", "Ele só quer se segurar em alguma coisa."),
                ("Eight arms and not one to help with the dishes.", "Oito braços e nenhum pra lavar a louça."),
                ("Weak point is the join: all eight pull from there.", "O ponto fraco é a junção: os oito puxam dali."),
                ("He's squishy on top! I touched him! He's cold!", "Ele é molinho em cima! Encostei! É gelado!"));

            Page(entries, "enemy_forest_dark_fish",
                "They say the water in this pit rises and falls at the whim of the old sluices, and sometimes it falls and the fish simply carry on, a yard off the ground, as though nothing has happened. I have never in my life seen anything so stubborn. There is a thin line of amber along the side where it swallowed a spark, and it will come at you all in one straight flat rush, and turn in its own length. Don't try to run from it; the air is its water. Wait with your feet planted, then hit it hard as it turns.",
                "Dizem que a água desta cova sobe e desce conforme as comportas antigas, e às vezes desce e os peixes simplesmente continuam, a um palmo do chão, como se nada tivesse acontecido. Nunca vi coisa tão teimosa na vida. Tem uma linha fina de âmbar no flanco, de uma faísca que engoliu, e ele vem de uma vez, reto e chato, e vira no próprio comprimento. Não tentem correr; o ar é a água dele. Esperem com os pés firmes e batam com força quando ele virar.",
                ("Wait for the turn, then strike.", "Espera a virada e bate."),
                ("The water left and he stayed. I understand him.", "A água foi embora e ele ficou. Eu entendo."),
                ("A fish that doesn't need water. So what's it for?", "Um peixe que não precisa de água. Pra que serve?"),
                ("Laterally flattened: he turns well and can't hide head-on.", "Achatado dos lados: vira bem e de frente não se esconde."),
                ("Fish! In the air! Nobody told him!", "Peixe! No ar! Ninguém avisou ele!"));

            // -- Cova da Treva (88): the ones that crawl --
            Page(entries, "enemy_forest_shadow_lizard",
                "The fifth pit has no light of its own at all, and the ones that crawl go on crawling there, because nothing ever told them to stop. This lizard has lived in the dark so long that its eyes have shrunk to pinpricks and the rest of it has spread out to make up for it. It feels you through the floor. Take a lamp. I mean it. In the dark, your hand won't bite half as deep as you think it will, and in the light it will flinch and show you the soft line under its throat. Keep Maria close. Her light does what mine never could.",
                "A quinta cova não tem luz nenhuma, e os que rastejam continuam rastejando lá, porque ninguém mandou parar. Este lagarto mora no escuro há tanto tempo que os olhos viraram dois pontinhos e o resto foi se espalhando pra compensar. Ele sente vocês pelo chão. Levem lamparina. Estou falando sério. No escuro a mão de vocês não entra nem metade do que parece, e na luz ele se encolhe e mostra a linha mole embaixo do pescoço. Mantenham a Maria perto. A luz dela faz o que a minha nunca fez.",
                ("Light on him before you swing.", "Luz nele antes de bater."),
                ("Tiny eyes. I'll be careful with my light near him.", "Olhinho minúsculo. Vou ter cuidado com a luz perto dele."),
                ("He's the size of the kitchen. He doesn't fit in the story.", "Ele é do tamanho da cozinha. Não cabe na história."),
                ("Vibration through the floor. Stand still and he loses you.", "Sente pelo chão. Fica parado e ele te perde."),
                ("He's so wide! He'd never fit in a hug!", "Ele é tão largo! Não cabe num abraço!"));

            Page(entries, "enemy_forest_lesser_shadow_dragon",
                "The last of the five shapes in that old story is the dragon's body, and it is shut up behind the seal with the rest of him, and I am glad to say none of you are ever going to look at it. This is not that either. This is a scrap of the same dark, big as a good cart, that came off a long time ago and remembers being enormous. It behaves accordingly. Its wings never finished growing and it does not know, so now and then it spreads them and looks pleased. Be polite about it for as long as it takes to reach its neck.",
                "A última das cinco formas dessa história antiga é o corpo de dragão, e ele está trancado atrás do selo com o resto dele, e fico feliz de dizer que nenhum de vocês vai olhar pra aquilo. Este também não é aquele. Este é um retalho da mesma treva, grande como uma boa carroça, que se soltou faz muito tempo e se lembra de ser enorme. Ele se comporta de acordo. As asas nunca acabaram de crescer e ele não sabe, então de vez em quando abre elas e fica satisfeito. Sejam educados com ele pelo tempo que levar pra alcançar o pescoço.",
                ("Neck. Everything else is just armor.", "Pescoço. O resto é só armadura."),
                ("He thinks he's huge. I won't be the one to tell him.", "Ele acha que é enorme. Não vou ser eu a contar."),
                ("Those wings. Buddy. We need to talk.", "Essas asas. Amigo. A gente precisa conversar."),
                ("Same scale pattern as the seal drawings. Worrying.", "Mesmo padrão de escama dos desenhos do selo. Preocupante."),
                ("His wings are little! Just like mine would be!", "As asinhas dele são pequenas! Igual às minhas seriam!"));

            // -- Portão da Terra da Treva (90) --
            Page(entries, "enemy_forest_lesser_archon",
                "You have met archons in my stories already: giants who swallowed too much light and would not give it back. The big ones are off in their own tales. The small ones at the Gate arrived late, and they swallowed whatever the big ones dropped, so they stand in the plaza of the pedestals with their crowns of dead lamps and a glow in the belly that clearly does not agree with them. They are slow, and proud, and terribly tired of being hungry. It is all still light in there. Aim for the belly until they give it back, and let it go home.",
                "Vocês já encontraram arcontes nas minhas histórias: gigantes que engoliram luz demais e não quiseram devolver. Os grandes estão nas histórias deles. Os pequenos, aqui do Portão, chegaram tarde e engoliram o que os grandes deixaram cair, por isso ficam na praça dos pedestais com a coroa de lâmpadas mortas e um brilho na barriga que claramente não caiu bem. São lentos, orgulhosos e cansadíssimos de ter fome. Lá dentro ainda é luz. Batam na barriga até eles devolverem, e deixem ela voltar pra casa.",
                ("Belly. That's where the light is.", "Barriga. É lá que a luz está."),
                ("So much light in him and he's still hungry.", "Tanta luz dentro e ele ainda com fome."),
                ("Nice crown. Did the lamps come with wicks?", "Bonita coroa. As lâmpadas vieram com pavio?"),
                ("Six dead lamps on the crown: he's been collecting.", "Seis lâmpadas mortas na coroa: ele anda colecionando."),
                ("His tummy is shining! He ate a star!", "A barriga dele está brilhando! Ele comeu uma estrela!"));
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
