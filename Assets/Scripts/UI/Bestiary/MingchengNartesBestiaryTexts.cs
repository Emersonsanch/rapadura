using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Planície de Mingcheng and the Estepe dos Nartes (Docs/Mapas100.md
    /// 41–52) that the overworld layouts used to play with stand-ins: lore in Vó Edna's voice and one line per cousin,
    /// en + pt-BR. Kept apart like <see cref="LateRegionBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class MingchengNartesBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_stone_lion", "enemy_forest_lantern_fox", "enemy_forest_terracotta_soldier", "enemy_forest_stone_turtle",
            "enemy_forest_mud_buffalo", "enemy_forest_nian_cub",
            "enemy_mountain_young_griffin", "enemy_mountain_saiga", "enemy_mountain_bronze_wolf", "enemy_mountain_sleepwalker_giant",
            "enemy_mountain_bark_beetle", "enemy_mountain_ochokochi",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Planície de Mingcheng (listed with the Living Water) --
            Page(entries, "enemy_forest_stone_lion",
                "Every gate of Mingcheng has a pair of stone lions, a male with a ball under his paw and a female with a cub, and Bögü says they were carved to keep bad dreams out of the rings. The shadow crept in through a crack, and the cold of it woke them. Now they walk off their pedestals and guard the roads against everyone, even the people they were made for. They are slow and very hard. Strike where the stone is cracked: that is where the light inside wants out.",
                "Todo portão de Mingcheng tem um par de leões de pedra, o macho com uma bola debaixo da pata e a fêmea com um filhote, e o Bögü diz que foram esculpidos pra não deixar sonho ruim entrar nos anéis. A sombra entrou por uma rachadura, e o frio dela acordou os dois. Agora eles descem do pedestal e guardam a estrada contra todo mundo, até contra o povo pra quem foram feitos. São lentos e duros demais. Batam onde a pedra está rachada: é por ali que a luz de dentro quer sair.",
                ("Aim for the glowing crack. Stone doesn't flinch, but light does.", "Mirem na rachadura que brilha. Pedra não pisca, mas luz pisca."),
                ("He was made to protect people. He still thinks he is. Gently.", "Ele foi feito pra proteger gente. Ele ainda acha que tá protegendo. Com jeito."),
                ("Stone against shield. Finally a fair fight.", "Pedra contra escudo. Finalmente uma briga justa."),
                ("Granite, hand-carved, no joints. How does it even walk? I need to know.", "Granito, esculpido à mão, sem articulação. Como é que isso anda? Eu preciso saber."),
                ("Where's his ball? If I find his ball, will he be nice?", "Cadê a bola dele? Se eu achar a bola dele, ele fica bonzinho?"));

            Page(entries, "enemy_forest_lantern_fox",
                "In the old Chinese tales the fox is the cleverest animal there is. The foxes of the millet fields learned to steal a little light from the paper lanterns of the rings and carry it on the tips of their tails, to see in the dark furrows. Āz the greedy found that clever and whispered that more light is always better, so now they steal every lantern they can reach and bite whoever wants it back. Watch the tail, not the fox: the lantern bobs before it jumps.",
                "Nos contos antigos da China a raposa é o bicho mais esperto que existe. As raposas dos campos de painço aprenderam a roubar um pouquinho de luz das lanternas de papel dos anéis e levar na ponta do rabo, pra enxergar nos sulcos escuros. A Āz, aquela gulosa, achou isso esperto e sussurrou que mais luz é sempre melhor, então agora elas roubam toda lanterna que alcançam e mordem quem quer de volta. Olhem o rabo, não a raposa: a lanterna balança antes do pulo.",
                ("She carries her own target. That's polite of her.", "Ela carrega o próprio alvo. Que educada."),
                ("She only wanted to see in the dark. Let's give the lantern back to Mestre Mōzak and leave her a little candle.", "Ela só queria enxergar no escuro. Vamos devolver a lanterna pro Mestre Mōzak e deixar uma velinha pra ela."),
                ("She ran circles round me. I got dizzy. She got away.", "Ela deu volta em mim. Eu fiquei tonto. Ela fugiu."),
                ("A tail with a light on it. I'm putting one on my backpack.", "Um rabo com luz na ponta. Vou pôr um na minha mochila."),
                ("Her tail is a lamp! Mine is just a tail. I don't have a tail.", "O rabo dela é uma lâmpada! O meu é só um rabo. Eu não tenho rabo."));

            Page(entries, "enemy_forest_terracotta_soldier",
                "Long ago an emperor had thousands of clay soldiers made to keep him company after he died, each one with its own face. Mingcheng keeps a few in the pits outside the rings, to remember that nobody should be guarded that much. The shadow got into the clay and made them stand up and march, all in step, along the great wall. They are heavy and they don't stop. When one cracks open, there is a little spark inside: it is the potter's care, and it flies home.",
                "Faz muito tempo um imperador mandou fazer milhares de soldados de barro pra fazer companhia pra ele depois que morresse, cada um com um rosto. Mingcheng guarda alguns nos fossos fora dos anéis, pra lembrar que ninguém precisa ser guardado tanto assim. A sombra entrou no barro e fez eles levantarem e marcharem, todos no mesmo passo, pela muralha grande. São pesados e não param. Quando um racha, tem uma faisquinha lá dentro: é o carinho do oleiro, e ela volta voando pra casa.",
                ("They all march in step. That means I know where the next one will be.", "Eles marcham no mesmo passo. Então eu sei onde o próximo vai estar."),
                ("Each one has a different face. Someone loved making them.", "Cada um tem um rosto diferente. Alguém gostou muito de fazer eles."),
                ("He's got a halberd. I've got a shield. And a better hairstyle.", "Ele tem uma alabarda. Eu tenho um escudo. E um penteado melhor."),
                ("Fired clay, bronze blade, about two thousand years. They don't make them like this anymore.", "Barro cozido, lâmina de bronze, uns dois mil anos. Não se faz mais assim."),
                ("He looks like Grandpa Jorge when he's sleeping in the chair.", "Ele parece o Vô Jorge quando dorme na cadeira."));

            Page(entries, "enemy_forest_stone_turtle",
                "The stone turtles of the canal carry the steles of Mingcheng on their backs, the big stones with the names of everyone who dug the canal, and Lin Deng says they swore never to put them down. The shadow didn't make them drop the stones; it made them afraid someone would take them. Now they snap at anyone who comes close to the locks. They are the slowest thing on the plain and the hardest to hurt. Go round behind: a turtle can't turn its head with a stone that big on its back.",
                "As tartarugas de pedra do canal carregam nas costas as estelas de Mingcheng, aquelas pedras grandes com o nome de todo mundo que cavou o canal, e o Lin Deng diz que elas juraram nunca pôr as pedras no chão. A sombra não fez elas largarem as pedras; fez elas terem medo de alguém pegar. Agora elas mordem quem chega perto das eclusas. São a coisa mais lenta da planície e a mais difícil de machucar. Deem a volta por trás: tartaruga não vira a cabeça com uma pedra desse tamanho nas costas.",
                ("Slow target, hard shell. I'll aim at the neck when it comes out.", "Alvo lento, casco duro. Vou mirar no pescoço quando ele sair."),
                ("She's guarding names. That's a beautiful job. We'll help her remember it's a good one.", "Ela tá guardando nomes. Que trabalho bonito. A gente ajuda ela a lembrar que é um trabalho bom."),
                ("I hit the shell. My arm hit back. Ow.", "Eu bati no casco. Meu braço que apanhou. Ai."),
                ("A stone carrying a stone. The weight distribution is actually perfect.", "Uma pedra carregando uma pedra. A distribuição de peso é perfeita, na verdade."),
                ("Can I read the names? Is my name there? Why not?", "Posso ler os nomes? O meu nome tá lá? Por que não?"));

            Page(entries, "enemy_forest_mud_buffalo",
                "The water buffaloes of the mirror paddies pull the ploughs through the mud, and they are so patient that children ride on their backs to the fields. The cold of the shadow came up through the flooded terraces into their legs, and now they can't stand anyone near the water. They lower their horns and charge, and a buffalo in the mud is faster than you would think. When the head drops, step to the side, and let the mud slow it down.",
                "Os búfalos-d'água dos arrozais espelhados puxam o arado pela lama, e são tão pacientes que as crianças vão pro campo montadas neles. O frio da sombra subiu pelos terraços alagados até as pernas deles, e agora eles não aguentam ninguém perto da água. Abaixam os chifres e correm, e búfalo na lama é mais ligeiro do que a gente pensa. Quando a cabeça abaixar, saiam de lado e deixem a lama segurar ele.",
                ("Head down, run. I know the drill now.", "Cabeça baixa, corre. Já aprendi a regra."),
                ("Children used to ride him. He'll be gentle again, I promise.", "As crianças montavam nele. Ele vai ficar manso de novo, eu prometo."),
                ("I stopped his charge with my shield. Then I sat down in the mud for a while.", "Eu segurei a corrida dele com o escudo. Depois fiquei sentado na lama um tempinho."),
                ("Wide hooves for mud, horns swept back so they don't catch. Clever animal.", "Casco largo pra lama, chifre virado pra trás pra não enroscar. Bicho esperto."),
                ("He's all muddy! I want to be all muddy.", "Ele tá todo enlameado! Eu quero ficar todo enlameado."));

            Page(entries, "enemy_forest_nian_cub",
                "The nian is the beast of the New Year in the Chinese tales: once a year it comes down from the mountains, and people chase it away with red lanterns, drums and firecrackers. These are its cubs, born on the peaks beyond the great wall. The shadow made them bold, and they come down any day of the year now, biting at the towers. They are quick and strong for their size. Remember what scares them: red, light, and a lot of noise. Ícaro, that means you.",
                "O nian é a fera do Ano-Novo dos contos chineses: uma vez por ano ele desce das montanhas, e o povo espanta ele com lanterna vermelha, tambor e bombinha. Esses são os filhotes dele, nascidos nos picos depois da muralha grande. A sombra deixou eles atrevidos, e agora eles descem em qualquer dia do ano, mordendo as torres. São ligeiros e fortes pro tamanho. Lembrem do que eles têm medo: vermelho, luz e muito barulho. Ícaro, isso é com você.",
                ("Red arrows. I'm painting all my arrows red.", "Flecha vermelha. Vou pintar todas as minhas flechas de vermelho."),
                ("He's just a baby. Scare him home, don't hurt him.", "Ele é só um bebê. Espanta ele pra casa, não machuca."),
                ("I yelled at him. He yelled at me. Then Ícaro's grenade yelled at both of us.", "Eu gritei com ele. Ele gritou comigo. Aí a granada do Ícaro gritou com os dois."),
                ("Loud noises, finally a monster that wants what I make. Fire in the hole!", "Barulho alto: finalmente um monstro que gosta do que eu faço. Lá vai!"),
                ("He's got one horn. Like a unicorn but angry. A grumpycorn.", "Ele tem um chifre só. Igual unicórnio, mas bravo. Um bravicórnio."));

            // -- Estepe dos Nartes (listed with the mountains) --
            Page(entries, "enemy_mountain_young_griffin",
                "The griffins of the steppe have the body of a lion and the head and wings of an eagle, and the old Scythians said they guard the gold of the north. Soslan's centaurs say the young ones used to race them along the tracks, for fun. The shadow put the old griffins' greed for gold into the young ones' heads, and now they dive at anything that shines, buckles and spears and the cousins' gear. When the wings open wide, it's about to pounce. Hold your ground together.",
                "Os grifos da estepe têm corpo de leão e cabeça e asas de águia, e os citas antigos diziam que eles guardam o ouro do norte. Os centauros do Soslan contam que os novinhos apostavam corrida com eles nas pistas, de brincadeira. A sombra pôs na cabeça dos novos a gula de ouro dos grifos velhos, e agora eles mergulham em tudo o que brilha, fivela, lança e as coisas dos primos. Quando as asas abrem bem, ele vai dar o bote. Fiquem firmes juntos.",
                ("Wings open means he's coming. That's when I let go.", "Asa aberta quer dizer que ele vem. É aí que eu solto a flecha."),
                ("He's young. He just wants to race. We'll race him after.", "Ele é novinho. Ele só quer apostar corrida. Depois a gente aposta com ele."),
                ("He tried to steal my shield because it's shiny. Polishing it was a mistake.", "Ele tentou roubar meu escudo porque brilha. Polir foi um erro."),
                ("A lion that flies. Back legs for running, front talons for grabbing. Brilliant design.", "Um leão que voa. Perna de trás pra correr, garra da frente pra agarrar. Projeto brilhante."),
                ("Can he carry me? I'm small. I'm very small.", "Ele me carrega? Eu sou pequena. Eu sou bem pequena."));

            Page(entries, "enemy_mountain_saiga",
                "The saiga is a little antelope of the steppe with a funny soft nose that hangs down over its mouth, to warm the cold air and keep out the dust. Saigas run in huge herds and are the most nervous animals there are: a shadow crossing the grass is enough. Now the real shadow has crossed it, and the herds have gone wild with fear, charging into anyone with their heads down. They are not mean, they are terrified. Hit a few and the rest will run.",
                "A saiga é um antílope pequeno da estepe com um nariz mole engraçado que cai por cima da boca, pra esquentar o ar frio e segurar a poeira. Saiga anda em manada enorme e é o bicho mais nervoso que existe: basta uma sombra passar no capim. Agora a sombra de verdade passou, e as manadas ficaram doidas de medo, correndo de cabeça baixa em cima de quem estiver na frente. Elas não são más, estão apavoradas. Acertem umas e o resto foge.",
                ("They come in herds. I only need to stop the first one.", "Elas vêm em manada. Eu só preciso parar a primeira."),
                ("They're so scared. Shh. It's alright. It's only us.", "Elas estão com tanto medo. Shh. Tá tudo bem. Somos só nós."),
                ("Twenty saigas hit my shield. I counted. Then I lost count.", "Vinte saigas bateram no meu escudo. Eu contei. Depois perdi a conta."),
                ("A nose with a built-in air filter. Nature did it first again.", "Um nariz com filtro de ar embutido. A natureza fez primeiro de novo."),
                ("Her nose wiggles! Do it again! No, don't run at me, just the nose!", "O nariz dela mexe! Faz de novo! Não, não corre pra mim, só o nariz!"));

            Page(entries, "enemy_mountain_bronze_wolf",
                "In the kurgans, the great burial hills of the steppe, the old chiefs were buried with bronze animals curled up in circles, wolves and deer and griffins. Satana of the camp says they were never meant to move. The shadow went down into an open kurgan and woke the bronze wolves, and now they guard the hills as if the gold were theirs. They are heavy, they don't feel cold, and they bite hard. Their seam glows where they were cast: that is where they break.",
                "Nos kurgans, os morros grandes onde a estepe enterrava os chefes antigos, os chefes eram enterrados com bichos de bronze enrolados em roda, lobos, cervos e grifos. A Satana do acampamento diz que eles nunca deviam se mexer. A sombra desceu num kurgan aberto e acordou os lobos de bronze, e agora eles guardam os morros como se o ouro fosse deles. São pesados, não sentem frio e mordem forte. A emenda deles brilha onde foram fundidos: é por ali que eles quebram.",
                ("Aim for the seam down the back. The light's showing me where.", "Mirem na emenda das costas. A luz tá mostrando onde."),
                ("He was put there to keep a sad chief company. Let's put him back to sleep.", "Puseram ele lá pra fazer companhia pra um chefe triste. Vamos fazer ele dormir de novo."),
                ("Metal wolf. My teeth hurt just looking at him.", "Lobo de metal. Meus dentes doem só de olhar pra ele."),
                ("Lost-wax casting, then filed by hand. Those spirals took weeks. Whoever made this was a master.", "Fundição em cera perdida, depois lixado à mão. Essas espirais levaram semanas. Quem fez isso era um mestre."),
                ("His tail is a circle! Can I wear it as a crown?", "O rabo dele é um círculo! Posso usar de coroa?"));

            Page(entries, "enemy_mountain_sleepwalker_giant",
                "The hill giants of the Fortim are the gentlest people you will ever meet; Virogdad reads their dreams on the old tablets every morning. But a bad dream ran away from one of the tablets, and some of the giants who sleep in the pastures can't wake up from it. They walk in their sleep with their arms out, and they swat at whatever is in the way as if it were the nightmare. Don't hate them for it. Beat the dream out of them, and they wake up, blinking, and walk home.",
                "Os gigantes-da-colina do Fortim são o povo mais manso que vocês vão conhecer; o Virogdad lê os sonhos deles nas tábuas antigas toda manhã. Mas um sonho ruim fugiu de uma das tábuas, e alguns gigantes que dormem nos pastos não conseguem acordar dele. Andam dormindo de braço esticado, e dão tapa no que estiver no caminho como se fosse o pesadelo. Não fiquem com raiva deles. Tirem o sonho deles na luta, e eles acordam piscando e vão pra casa.",
                ("He's asleep. He'll never see the arrow coming. That feels unfair. I'll shout first.", "Ele tá dormindo. Nunca vai ver a flecha chegando. Parece injusto. Vou gritar antes."),
                ("Poor thing, he's having a nightmare. When he wakes up, I'll sing for him.", "Coitado, ele tá tendo um pesadelo. Quando acordar, eu canto pra ele."),
                ("He slapped me in his sleep. Imagine awake.", "Ele me deu um tapa dormindo. Imagina acordado."),
                ("Two and a half metres and he sleepwalks. The floors in the Fortim must be very thick.", "Dois metros e meio e ainda é sonâmbulo. O chão do Fortim deve ser bem grosso."),
                ("Shh! Don't wake him! Wait. We have to wake him. I'm confused.", "Shh! Não acorda ele! Espera. A gente tem que acordar ele. Fiquei confusa."));

            Page(entries, "enemy_mountain_bark_beetle",
                "In the larch taiga around the Arvoredo there are beetles as big as dogs, with backs like bark, and Haripushpa says they used to eat only the dead wood and leave the living trees alone. Āz gave them a hunger that never ends, and now they chew into the trunk folk's homes, and they pinch hard with those mandibles. They are slow and their backs are very tough. Watch the head: the pincers open wide before they snap.",
                "Na taiga de lariços em volta do Arvoredo tem besouro do tamanho de um cachorro, com as costas que parecem casca de árvore, e a Haripushpa conta que eles só comiam a madeira morta e deixavam as árvores vivas em paz. A Āz deu pra eles uma fome que nunca acaba, e agora eles roem as casas do povo-tronco e beliscam forte com aquelas pinças. São lentos e as costas são duras demais. Olhem a cabeça: as pinças abrem bem antes de fechar.",
                ("The back's too hard. I aim under, where the legs are.", "As costas são duras demais. Eu miro embaixo, onde ficam as pernas."),
                ("They're just hungry. Once the hunger goes, they'll go back to the dead wood.", "Eles só estão com fome. Quando a fome passar, eles voltam pra madeira morta."),
                ("He pinched my boot. My good boot.", "Ele beliscou minha bota. Minha bota boa."),
                ("Six legs, armoured wing cases, and it chews wood. It's a tiny lumber mill.", "Seis pernas, asa blindada, e ainda mastiga madeira. É uma serraria pequenininha."),
                ("I like beetles. Not this one. This one bites.", "Eu gosto de besouro. Não desse. Esse morde."));

            Page(entries, "enemy_mountain_ochokochi",
                "The ochokochi is the wild man of the Georgian forests, covered in long hair, with a sharp blade of bone growing out of his chest like an axe. The old tales say he tries to hug travellers, and the hug is what cuts. Batradz says they live in the valley of the svan towers and never used to come near the villages. The shadow made them lonely and angry, and now they come down to the towers looking for someone to hug. Never let him close his arms around you. Keep him at a distance and keep moving.",
                "O ochokochi é o homem selvagem das florestas da Geórgia, coberto de pelo comprido, com uma lâmina de osso afiada saindo do peito feito um machado. Os contos antigos dizem que ele tenta abraçar quem passa, e é o abraço que corta. O Batradz conta que eles moram no vale das torres svan e nunca chegavam perto das vilas. A sombra deixou eles sozinhos e bravos, e agora eles descem até as torres procurando alguém pra abraçar. Nunca deixem ele fechar os braços em volta de vocês. Fiquem longe e não parem de andar.",
                ("When his arms go up, I'm already gone.", "Quando os braços dele sobem, eu já saí."),
                ("He just wants a hug. It's the blade that's the problem. Poor thing.", "Ele só quer um abraço. O problema é a lâmina. Coitadinho."),
                ("He hugged my shield. My shield has a scratch now. We're not friends.", "Ele abraçou meu escudo. Agora meu escudo tem um risco. A gente não é amigo."),
                ("A bone axe growing out of his chest. Biology is weirder than engineering.", "Um machado de osso crescendo no peito. A biologia é mais esquisita que a engenharia."),
                ("I'll hug him from behind. Where there's no blade. Vó, can I?", "Eu abraço ele por trás. Onde não tem lâmina. Vó, posso?"));
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
