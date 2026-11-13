using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the ten roles of the Teto do Mundo (Região 7, Docs/Mapas100.md 36–40) and the two of the Planície de
    /// Mingcheng (Região 8, 42 and 45) that the overworld still played with another region's beast (bible §3.2.1, twentieth pass): lore in
    /// Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="LentRoleBestiaryTexts"/>; <see cref="BestiaryTexts"/>
    /// appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class HighlandOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_moraine_roller", "enemy_mountain_chute_ushabti", "enemy_mountain_crest_mist", "enemy_desert_smoke_kite",
            "enemy_mountain_dimmed_royal_hawk", "enemy_cave_wind_bat", "enemy_mountain_stone_guard_ushabti", "enemy_desert_az_bandit",
            "enemy_mountain_dimmed_argali", "enemy_cave_snow_chough", "enemy_desert_terracotta_archer", "enemy_mountain_dimmed_light_ibis",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Terraços de Sangkent, Rio de Degelo, Calhas, Terraços Altos, Morenas (36–38) --
            Page(entries, "enemy_mountain_moraine_roller",
                "The glacier carries its boulders down a finger's width a year and leaves them on the moraines, where the yaks come to scratch their backs. The shadow got into the ice in their cracks, and now the boulders roll down on purpose, onto the terraces and the chutes. They are slow to start and very hard to stop. Step out of the line they roll in, and hit them while they turn round to find you.",
                "A geleira carrega as pedras dela um dedo por ano e deixa nas morenas, onde os iaques vêm coçar as costas. A sombra entrou no gelo das rachaduras delas, e agora as pedras rolam de propósito, pra cima dos terraços e das calhas. Demoram pra começar e é dificílimo parar. Saiam da linha em que ela rola, e batam enquanto ela vira pra procurar vocês.",
                ("Get out of its line, then hit it on the turn.", "Sai da linha dela, depois bate na virada."),
                ("The yaks miss it. It was the best back scratcher on the mountain.", "Os iaques sentem falta dela. Era o melhor coçador de costas da montanha."),
                ("I tried to push it back up. It pushed back harder.", "Eu tentei empurrar ela de volta pra cima. Ela empurrou mais forte."),
                ("The ice inside it is older than Sangkent. The shadow went for the oldest thing it could find.", "O gelo dentro dela é mais velho que Sangkent. A sombra foi atrás da coisa mais antiga que achou."),
                ("It has a little hat of snow! A rock with a hat!", "Ela tem um chapeuzinho de neve! Uma pedra de chapéu!"));

            // -- Calhas de Pedra, Estação da Calha Alta (37) --
            Page(entries, "enemy_mountain_chute_ushabti",
                "The Artificers cut the chutes to bring the ore of the Teto down to Sangkent, and set answering figures of stone at the top to fill the carts, the way the old figures answer when they are called to work the fields. The shadow called them too, and they answered it. Now they push the carts onto anyone below and swing their picks at whoever climbs. They are slow and heavy; keep moving round them.",
                "Os Artífices cavaram as calhas pra trazer o minério do Teto até Sangkent, e puseram figuras de pedra que respondem lá em cima pra encher os carrinhos, do jeito que as figuras antigas respondem quando chamam pra trabalhar na roça. A sombra chamou também, e elas responderam. Agora empurram os carrinhos em cima de quem tá embaixo e batem a picareta em quem sobe. São lentas e pesadas; fiquem rodeando.",
                ("Slow and heavy. Circle it and hit its back.", "Lenta e pesada. Rodeia e bate nas costas."),
                ("It only does what it's told. Someone should tell it something kind.", "Ela só faz o que mandam. Alguém devia mandar uma coisa boa."),
                ("Its basket is full of copper. Finders keepers?", "A cesta dela tá cheia de cobre. Achado não é roubado?"),
                ("It answers whoever calls. That's the danger of answering without asking who it is.", "Ela responde pra quem chamar. O perigo é responder sem perguntar quem é."),
                ("It has a lamp on its head like a miner! Can I have one?", "Ela tem lanterna na cabeça que nem mineiro! Eu posso ter uma?"));

            // -- Estação da Calha Alta, Rio de Degelo, Lago Turquesa, Cristas do Vento Ruim (37–39) --
            Page(entries, "enemy_mountain_crest_mist",
                "The ill wind of the crests comes over the ridge every evening and goes down the other side, and the herders count the days by it. The shadow made part of it stay behind, a cold cloud that sits on the chutes and the lake and will not move on. It took a glint of frost off a climber's lamp and keeps it inside. It chills whoever walks into it. Don't stay in it; fight it from its edge.",
                "O vento ruim das cristas passa por cima do cume toda tarde e desce do outro lado, e os pastores contam os dias por ele. A sombra fez um pedaço dele ficar pra trás, uma nuvem fria que senta nas calhas e no lago e não vai embora. Ela pegou um brilho de geada da lanterna de um escalador e guarda lá dentro. Gela quem entra nela. Não fiquem dentro; lutem da beirada.",
                ("Fight it from the edge. Inside, it freezes you.", "Luta da beirada. Dentro, ela congela."),
                ("It got left behind. I'd be grumpy too.", "Ela ficou pra trás. Eu também ia ficar de mau humor."),
                ("My nose went numb. I can't feel my nose. Is it still there?", "Meu nariz ficou dormente. Não sinto meu nariz. Ele ainda tá aí?"),
                ("A wind that stops isn't a wind any more. The shadow takes away what things are for.", "Vento que para não é mais vento. A sombra tira o pra que as coisas servem."),
                ("It's a cloud with eyes! Can I sit on it?", "É uma nuvem com olho! Posso sentar nela?"));

            // -- Forte dos Ventos, Desfiladeiro dos Ventos (39) --
            Page(entries, "enemy_desert_smoke_kite",
                "The caravan children fly paper kites off the fort with a ribbon of each of the five lights on the tail, and the falconers say the wind is good when the kites stand still. One night the kites came down without their children, grey with smoke, the ribbons gone dull. They still have the little lamp tied at the cross, and they throw its fire from far off. Get under them and close the distance.",
                "As crianças das caravanas soltam pipa de papel do forte com uma fita de cada uma das cinco luzes no rabo, e os falcoeiros dizem que o vento tá bom quando as pipas ficam paradas. Uma noite as pipas desceram sem as crianças, cinzentas de fumaça, as fitas desbotadas. Elas ainda têm a lamparininha amarrada no meio, e jogam o fogo dela de longe. Cheguem por baixo e encurtem a distância.",
                ("It shoots from far. Run in under it.", "Ela atira de longe. Corre por baixo dela."),
                ("Some child is still looking up for it.", "Alguma criança ainda tá olhando pro céu procurando ela."),
                ("I used to be great at kites. This one is cheating.", "Eu era ótimo de pipa. Essa tá roubando."),
                ("Five ribbons, five lights. They took the colour first.", "Cinco fitas, cinco luzes. Tiraram a cor primeiro."),
                ("It has a face drawn on it! It's smiling... kind of.", "Tem uma carinha desenhada nela! Tá sorrindo... mais ou menos."));

            // -- Desfiladeiro dos Ventos, Torres de Cata-Vento, Estepe Alta (39) --
            Page(entries, "enemy_mountain_dimmed_royal_hawk",
                "The falconers of the Forte dos Ventos fly the big crested falcons off the towers to keep the vultures away from the caravans' mules, and the fort calls them royal for the crest. The shadow came up the gorge on the ill wind, and the falcons stopped coming back to the glove. Now they stoop on the caravans themselves and throw their hard feathers from the sky. Keep moving, and strike when they drop.",
                "Os falcoeiros do Forte dos Ventos soltam os falcões grandes de penacho das torres pra espantar os abutres das mulas das caravanas, e o forte chama eles de reais por causa do penacho. A sombra subiu o desfiladeiro no vento ruim, e os falcões pararam de voltar pra luva. Agora eles mergulham nas caravanas e jogam as penas duras lá do céu. Fiquem andando, e ataquem quando ele descer.",
                ("Keep moving. Strike when it drops.", "Fica andando. Ataca quando ele descer."),
                ("It still has the falconer's strap on its leg. He's waiting for it.", "Ele ainda tem a tira do falcoeiro na perna. Ele tá esperando."),
                ("Royal? It's got the manners of a goat.", "Real? Tem modos de bode."),
                ("It was trained to protect the caravans. The shadow didn't change the bird, only who it protects them from.", "Ele foi treinado pra proteger as caravanas. A sombra não mudou o pássaro, só de quem ele protege."),
                ("It has a moustache! A bird with a moustache!", "Ele tem bigode! Um passarinho de bigode!"));

            // -- Forte dos Ventos, Torres de Cata-Vento (39) --
            Page(entries, "enemy_cave_wind_bat",
                "The pale bats roost inside the wind bells of the fort's towers and keep the moths off the bell ropes; the watch says a bell rings by itself when a bat comes home. Since the shadow the bells ring all night, and the bats come out of them in a cloud at whoever climbs. Each one is small and quick. Swing wide, and don't let them get behind you.",
                "Os morcegos claros dormem dentro dos sinos de vento das torres do forte e espantam as mariposas das cordas dos sinos; a guarda diz que o sino toca sozinho quando um morcego volta pra casa. Desde a sombra os sinos tocam a noite inteira, e os morcegos saem deles numa nuvem em cima de quem sobe. Cada um é pequeno e rápido. Batam largo, e não deixem eles irem pras costas de vocês.",
                ("Swing wide. Don't let them circle behind.", "Bate largo. Não deixa rodear pelas costas."),
                ("They just want to go back to their bells and sleep.", "Eles só querem voltar pros sinos e dormir."),
                ("It rang at me. It RANG at me. How is that fair?", "Ele tocou pra mim. Ele TOCOU pra mim. Isso é justo?"),
                ("The bells warn of danger. Now the danger rings them.", "Os sinos avisam do perigo. Agora é o perigo que toca eles."),
                ("It wears a tiny bell like a kitten!", "Ele usa um sininho que nem gatinho!"));

            // -- Forte dos Ventos, Cristas do Vento Ruim, Desfiladeiro, Ninho de Aydahar (39–40) --
            Page(entries, "enemy_mountain_stone_guard_ushabti",
                "The Artificers left the fort a watch of answering figures, whitewashed with the same lime as the walls, to stand on the battlements while the falconers slept. The shadow woke the watch at the wrong hour, and now it guards the gorge against everyone, the falconers too. It hides behind its juniper shield and thrusts with its spear. Go round the shield; it turns slowly.",
                "Os Artífices deixaram pro forte uma guarda de figuras que respondem, caiadas com a mesma cal das muralhas, pra ficar nas ameias enquanto os falcoeiros dormiam. A sombra acordou a guarda na hora errada, e agora ela guarda o desfiladeiro contra todo mundo, os falcoeiros também. Ela se esconde atrás do escudo de zimbro e estoca com a lança. Deem a volta no escudo; ela vira devagar.",
                ("Go round the shield. It turns slowly.", "Dá a volta no escudo. Ela vira devagar."),
                ("It's still keeping watch. It just forgot for whom.", "Ela ainda tá de guarda. Só esqueceu pra quem."),
                ("It's wearing a hat taller than me. Show-off.", "Ela tá de chapéu mais alto que eu. Exibida."),
                ("A guard that can't tell friend from stranger is the worst kind of wall.", "Guarda que não sabe quem é amigo e quem é estranho é o pior tipo de muro."),
                ("Its shield has a little windmill on it!", "O escudo dela tem um cata-ventinho desenhado!"));

            // -- Passo de Wakhan, Alto do Wakhan, Ninho de Aydahar (40) --
            Page(entries, "enemy_desert_az_bandit",
                "Āz is the greed that wants every load on the road for itself. Her bandits wait in the snow of the pass where the caravans have to go slowly, take whatever the mules carry, and tie the caravan's ribbons to their belts to keep count. They have more ribbons than they can count now, and still want more. They hit hard and come at you head on. Don't get greedy yourselves: hit once and step away.",
                "A Āz é a ganância que quer toda carga da estrada pra ela. Os bandoleiros dela esperam na neve do passo onde as caravanas têm que ir devagar, levam tudo que as mulas carregam, e amarram as fitas da caravana no cinto pra contar. Agora eles têm mais fita do que conseguem contar, e ainda querem mais. Batem forte e vêm de frente. Não fiquem gananciosos vocês também: batam uma vez e saiam.",
                ("Hit once and step away. Don't trade blows.", "Bate uma vez e sai. Não troca pancada."),
                ("I think someone told them they'd be happy with one more ribbon.", "Acho que alguém disse pra eles que iam ser felizes com mais uma fita."),
                ("He took my snack. My SNACK. This is personal now.", "Ele pegou meu lanche. Meu LANCHE. Agora é pessoal."),
                ("Greed never says 'enough'. That's how you know it's Āz talking.", "A ganância nunca diz 'chega'. É assim que dá pra saber que é a Āz falando."),
                ("He has so many ribbons! Can we give them back to the caravans?", "Ele tem tanta fita! A gente pode devolver pras caravanas?"));

            // -- Estepe Alta, Vale do Wakhan, Passo de Wakhan (36, 40) --
            Page(entries, "enemy_mountain_dimmed_argali",
                "The argali walk the pass in single file above the caravans and knock little stones down to warn them of the snow, so the caravan men call them the pass's own sentries. The shadow turned the sentries round. Now they knock the stones onto the road, and come down after them horns first. When its head goes down, step aside and let it go past.",
                "Os argalis andam pelo passo em fila acima das caravanas e derrubam pedrinhas pra avisar da neve, por isso os homens da caravana chamam eles de sentinelas do passo. A sombra virou as sentinelas do avesso. Agora eles derrubam as pedras na estrada, e descem atrás delas de chifre na frente. Quando a cabeça abaixar, saiam de lado e deixem passar.",
                ("Head down means charge. Step aside.", "Cabeça baixa é investida. Sai de lado."),
                ("They used to warn people. They still remember how, they just forgot why.", "Eles avisavam as pessoas. Ainda lembram como, só esqueceram por quê."),
                ("Those horns are bigger than its whole head. How does it even stand up?", "Esses chifres são maiores que a cabeça inteira. Como ele fica de pé?"),
                ("A sentry turned round is still a sentry. Just facing the wrong way.", "Sentinela virada ainda é sentinela. Só olhando pro lado errado."),
                ("Its horns are curly like snail shells!", "Os chifres dele são enroladinhos que nem casca de caracol!"));

            // -- Passo de Wakhan and every corner of the Teto (40) --
            Page(entries, "enemy_cave_snow_chough",
                "The choughs of the high pass follow the caravans for the crumbs and fly ahead of them into the snow, so the caravan men know where the road is when everything is white. The shadow got into the flock in a storm. Now they fly ahead to lead the caravans off the road, and throw frozen feathers at anyone who will not follow. Don't follow; bring them down when they land to throw.",
                "As gralhas do passo alto seguem as caravanas pelas migalhas e voam na frente delas pra dentro da neve, assim os homens da caravana sabem onde tá a estrada quando tudo fica branco. A sombra entrou no bando numa tempestade. Agora elas voam na frente pra tirar as caravanas da estrada, e jogam penas congeladas em quem não segue. Não sigam; derrubem quando elas pousarem pra jogar.",
                ("Don't follow them. Hit them when they land.", "Não segue. Bate quando elas pousarem."),
                ("They're still hungry for crumbs. Maybe we can share.", "Elas ainda tão com fome de migalha. Quem sabe a gente divide."),
                ("A bird threw ice at me. In the snow. As if I needed more ice.", "Um passarinho jogou gelo em mim. Na neve. Como se eu precisasse de mais gelo."),
                ("A guide that leads you off the road is worse than no guide.", "Guia que te tira da estrada é pior que guia nenhum."),
                ("Its beak is yellow like a banana!", "O bico dela é amarelo que nem banana!"));

            // -- Muralha das Mil Torres, Vale dos Guerreiros, Portão do Nian (45) --
            Page(entries, "enemy_desert_terracotta_archer",
                "The clay army stands in the pits under the Wall to keep it while the soldiers sleep, and the archers kneel at the front of every rank. Their knees cracked off in the cold winters. The shadow filled the break with its own smoke, and now they float up out of the pits and shoot from the watch-towers with arrows lit from the Wall's lanterns. Run at them; up close a bow is no use.",
                "O exército de barro fica nos fossos debaixo da Muralha pra guardar ela enquanto os soldados dormem, e os arqueiros ficam ajoelhados na frente de cada fileira. Os joelhos deles racharam nos invernos frios. A sombra encheu a rachadura com a fumaça dela, e agora eles flutuam pra fora dos fossos e atiram das torres de vigia com flechas acesas nas lanternas da Muralha. Corram pra cima deles; de perto, arco não adianta.",
                ("Close in fast. Up close the bow is useless.", "Chega perto rápido. De perto o arco não serve."),
                ("He was made to keep watch, not to shoot people. Let's let him kneel again.", "Ele foi feito pra vigiar, não pra atirar nos outros. Vamos deixar ele ajoelhar de novo."),
                ("No legs and still faster than me up the stairs. Rude.", "Sem perna e ainda mais rápido que eu na escada. Mal-educado."),
                ("Every one of them has a different face. Someone made each of them by hand.", "Cada um deles tem uma cara diferente. Alguém fez cada um com a mão."),
                ("He's floating! Like a ghost with a hairdo!", "Ele tá flutuando! Que nem fantasma de penteado!"));

            // -- Jardins em Terraço, Colinas dos Espelhos (42) --
            Page(entries, "enemy_mountain_dimmed_light_ibis",
                "The white ibises nest on the spires of the high elves and carry the morning light from mirror to mirror, a glint at a time, so all the gardens wake together. The shadow dimmed the glint on their brows, and they carry it back the wrong way now and throw it at whoever walks the terraces. They are quick in the air. Wait for the throw, then strike while they hang still.",
                "As íbis brancas fazem ninho nas torres dos altos-elfos e levam a luz da manhã de espelho em espelho, um brilho de cada vez, pra todos os jardins acordarem juntos. A sombra apagou o brilho da testa delas, e agora elas levam ele pro lado errado e jogam em quem anda pelos terraços. São rápidas no ar. Esperem o arremesso, e ataquem enquanto ela fica parada.",
                ("Wait for the throw, then strike while it hangs still.", "Espera o arremesso, depois ataca enquanto ela fica parada."),
                ("Amanirenas will want them carrying the light again. Gently.", "A Amanirenas vai querer elas levando a luz de novo. Com jeito."),
                ("It has a mirror on its forehead. It's prettier than me and it knows it.", "Ela tem um espelho na testa. É mais bonita que eu e sabe disso."),
                ("A messenger of light carrying it backwards. That's the shadow's favourite trick.", "Mensageira da luz levando ela ao contrário. É o truque preferido da sombra."),
                ("Its beak is like a bendy straw!", "O bico dela parece canudinho dobrável!"));
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
