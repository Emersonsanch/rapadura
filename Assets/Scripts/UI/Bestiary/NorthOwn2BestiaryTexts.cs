using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the nine roles of the sheets 57–100 (the Ninho da Huma 58, the Canais de Gelo 60, the Galerias dos Mamutes
    /// 62, the Jardins de Lava Laqueada 66, Echeyde 67 and the Nine Furnaces 69) and of the Guarda de Taowu (46) that the overworld still played
    /// with another region's beast after the twenty-eighth pass (bible §3.2.1, twenty-ninth pass): lore in Grandma Edna's voice and one line
    /// per cousin, en + pt-BR. Kept apart like <see cref="PeaksOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class NorthOwn2BestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_gale", "enemy_cave_ice_crack", "enemy_desert_living_crystal", "enemy_cave_ice_root", "enemy_cave_ember_carp",
            "enemy_mountain_smoke_bamboo", "enemy_mountain_fumarole", "enemy_mountain_living_bellows", "enemy_forest_taowu_guard",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Ninho da Huma (58) --
            Page(entries, "enemy_mountain_gale",
                "The old teachers counted five dark lands under the Darkness, and one of them is the Destroying Wind. Its king wants the huma's golden nest for himself, and the little gales run ahead of him round the cloud isles, tearing twigs out of the nest and throwing them at whoever climbs. A Ventania has no face, only two pale eyes in the spin, and a scrap of the huma's lucky shadow caught in the middle of it. When it squats down low and the scrap shines, it is about to push: plant your feet, or step behind a stone.",
                "Os mestres antigos contavam cinco terras escuras debaixo da Treva, e uma delas é o Vento Destruidor. O rei dele quer o ninho dourado da huma só pra ele, e as ventaniazinhas correm na frente em volta das ilhas de nuvem, arrancando galhinhos do ninho e jogando em quem sobe. Ventania não tem cara, só dois olhos claros no meio do redemoinho, e um pedacinho da sombra de sorte da huma preso lá dentro. Quando ela se agacha e o pedacinho brilha, vai empurrar: firma o pé, ou se esconde atrás de uma pedra.",
                ("It crouches before it pushes. Brace, or get behind a rock.", "Ela se agacha antes de empurrar. Firma o pé ou vai pra trás de uma pedra."),
                ("It's carrying the huma's twigs. It doesn't even want them. It just wants to take.", "Ela carrega os galhos da huma. Nem quer eles. Só quer tirar."),
                ("My hair is never going back to normal. Never.", "Meu cabelo nunca mais volta ao normal. Nunca."),
                ("A wind that only destroys is still a wind. Turn it, and it could carry a seed instead.", "Vento que só destrói ainda é vento. Vira ele, e ele pode levar uma semente em vez disso."),
                ("It took a feather! Give it back! That's the huma's!", "Ela pegou uma pena! Devolve! É da huma!"));

            // -- Canais de Gelo (60) --
            Page(entries, "enemy_cave_ice_crack",
                "Where the boatmen's canals freeze too thin, the ice splits, and something down in the black water pushes the two edges up like a mouth. The boatmen call it the Rachadura. It walks the canals on its own broken slab, slow and heavy, and where it goes the ice cracks behind it. There is a cold light down in the crack, like an eye: when it swells, the two edges are about to snap shut, so step back off the slab. Heavy mounts, keep away from it; it is waiting for one to fall in.",
                "Onde os canais dos barqueiros congelam fino demais, o gelo racha, e alguma coisa lá na água preta empurra as duas beiradas pra cima que nem uma boca. Os barqueiros chamam de Rachadura. Ela anda pelos canais em cima do próprio pedaço quebrado, devagar e pesada, e por onde passa o gelo vai rachando atrás. Tem uma luz fria lá no fundo da fenda, que nem um olho: quando ela incha, as duas beiradas vão fechar, então sai de cima da placa. Montaria pesada, longe dela; ela está esperando uma cair.",
                ("The light in the crack swells first. Off the slab, then hit it.", "A luz da fenda incha primeiro. Sai da placa e depois bate."),
                ("It's only ice that broke. It's lonely in that cold water, I think.", "É só gelo que quebrou. Acho que é sozinho, naquela água fria."),
                ("It's a floor that bites. I knew floors were plotting something.", "É um chão que morde. Eu sabia que os chãos estavam tramando alguma coisa."),
                ("A crack is where the dark gets in, but it's also where the light gets in. Look which one you're feeding.", "Rachadura é por onde a treva entra, mas também é por onde a luz entra. Repara qual das duas você está alimentando."),
                ("Don't step on the cracks! It's a rule! A real rule!", "Não pisa na rachadura! É regra! Regra de verdade!"));

            // -- Galerias dos Mamutes (62) --
            Page(entries, "enemy_desert_living_crystal",
                "The old people of the north say the mammoth lives under the earth and digs the valleys, and where its tusks scrape the cold stone, crystals grow. Down in the dark they went hungry for light. A Cristal Vivo catches the light of your lamp, keeps it in its heart, and throws it back hard from across the gallery. You can see it coming: the whole cluster leans back and the heart goes bright. Step out of the line, then go close, because up close it has nothing to throw.",
                "O povo antigo do norte diz que o mamute mora debaixo da terra e cava os vales, e onde as presas dele raspam a pedra fria nascem cristais. Lá no escuro eles ficaram com fome de luz. O Cristal Vivo pega a luz da lamparina de vocês, guarda no coração e joga de volta com força, lá do outro lado da galeria. Dá pra ver vindo: o cacho inteiro tomba pra trás e o coração acende. Sai da linha e chega perto, que de perto ele não tem o que jogar.",
                ("It leans back before the shot. Sidestep, then close in.", "Ele tomba pra trás antes do tiro. Passo pro lado e chega perto."),
                ("It only wanted a little light. It's so dark down there.", "Ele só queria um pouquinho de luz. É tão escuro lá embaixo."),
                ("It stole my lamp light and threw it at me. That's double rude.", "Ele roubou a luz da minha lamparina e jogou em mim. Isso é falta de educação dobrada."),
                ("Light that's kept for yourself turns hard. Light that's given away stays soft.", "Luz guardada só pra si fica dura. Luz que se dá fica macia."),
                ("It's so shiny! Can I keep one little piece? Just one?", "Ele é tão brilhante! Posso ficar com um pedacinho? Só um?"));

            Page(entries, "enemy_cave_ice_root",
                "The old trees of the north keep their roots deep in the frozen earth. When the mammoth cuts a new gallery it tears some of them out, and the cold knots them together into a Raiz-Gelo. It walks on its roots like legs, slow, reaching for anything warm, and when it catches you it squeezes until you are as cold as it is. It is slow, so do not let it catch you: when it hunches down and the pale light in the knot flares, it is about to grab. Step away and come back after.",
                "As árvores velhas do norte guardam as raízes lá no fundo da terra congelada. Quando o mamute abre uma galeria nova, ele arranca algumas, e o frio amarra tudo num nó: a Raiz-Gelo. Ela anda nas raízes que nem perna, devagar, procurando qualquer coisa quente, e quando pega aperta até vocês ficarem frios igual a ela. Ela é lenta, então não deixa ela pegar: quando ela se encolhe e a luz clarinha do nó acende, vai agarrar. Sai de perto e volta depois.",
                ("It hunches before it grabs. Out of reach, then back in.", "Ela se encolhe antes de agarrar. Sai do alcance e volta."),
                ("It's looking for its tree. It doesn't know the tree is still up there.", "Ela está procurando a árvore dela. Não sabe que a árvore ainda está lá em cima."),
                ("A hug I did not ask for. A cold one.", "Um abraço que eu não pedi. E gelado."),
                ("Roots hold on, that's what they're for. This one just forgot what it was holding.", "Raiz segura, é pra isso que ela serve. Essa só esqueceu o que estava segurando."),
                ("It has so many legs! I counted six! Or seven!", "Ela tem um montão de pernas! Eu contei seis! Ou sete!"));

            // -- Jardins de Lava Laqueada (66) --
            Page(entries, "enemy_cave_ember_carp",
                "The garden ponds of the lacquered isle are lava now, and the carp that lived in them learned to swim in the hot air above. Their scales went to embers. Each one carries an ember in its mouth like a pearl, and spits it at whoever leans over the pond. They are quick and they keep their distance, flicking their long tails. Watch the mouth: it opens before the ember flies. Go round it, not straight at it, and it has to turn to aim.",
                "Os laguinhos dos jardins da ilha laqueada viraram lava, e as carpas que moravam neles aprenderam a nadar no ar quente lá em cima. As escamas viraram brasa. Cada uma carrega uma brasinha na boca que nem pérola, e cospe em quem se debruça no lago. Elas são rápidas e ficam longe, balançando aquele rabo comprido. Olha a boca: ela abre antes da brasa voar. Vai rodeando, não reto pra cima, que ela tem que virar pra mirar.",
                ("The mouth opens before it spits. Circle it, don't charge.", "A boca abre antes de cuspir. Vai rodeando, não corre reto."),
                ("They were somebody's pond fish. Somebody fed them every morning.", "Eram peixinhos do lago de alguém. Alguém dava comida pra elas toda manhã."),
                ("A fish that flies and spits fire. The sea gave up being normal.", "Peixe que voa e cospe fogo. O mar desistiu de ser normal."),
                ("The carp swims up the waterfall because it won't stop. Even in the lava it still won't stop.", "A carpa sobe a cachoeira porque não desiste. Até na lava ela continua não desistindo."),
                ("It's orange and gold! Like a goldfish! A really angry goldfish!", "Ela é laranja e dourada! Que nem peixinho dourado! Um peixinho dourado muito bravo!"));

            Page(entries, "enemy_mountain_smoke_bamboo",
                "Under the cloud pines of the gardens, the bamboo grew up in the smoke of the lava and went grey with it. The shadow got into the hollow canes. Now a clump will pull up its roots and walk down the raked gravel paths, and whip its canes at whoever comes near. There is a light caught in a node of the middle cane, the way a lantern hangs in the pines. When the whole clump leans back and the light shines, the canes are coming down: step to the side. Do not stand where you raked; it does not care for your pattern.",
                "Debaixo dos pinheiros de nuvem dos jardins, o bambu cresceu na fumaça da lava e ficou cinza que nem ela. A sombra entrou nos gomos ocos. Agora uma touceira arranca as raízes, desce pelos caminhos de cascalho rastelado e chicoteia os bambus em quem chega perto. Tem uma luz presa num nó do bambu do meio, que nem lanterna pendurada nos pinheiros. Quando a touceira inteira tomba pra trás e a luz acende, os bambus vão descer: passo pro lado. Não fica em cima do desenho que vocês rastelaram, que ela não liga pra ele.",
                ("It leans back before the whip. Sidestep, then cut.", "Ela tomba pra trás antes de chicotear. Passo pro lado e corta."),
                ("Bamboo bends so it doesn't break. This one broke anyway.", "Bambu enverga pra não quebrar. Esse quebrou mesmo assim."),
                ("It messed up my gravel. I raked that for ten minutes.", "Ele bagunçou meu cascalho. Eu rastelei aquilo por dez minutos."),
                ("A hollow cane can be a flute or a stick. It depends what you fill it with.", "Bambu oco pode ser flauta ou pode ser vara. Depende do que se põe dentro."),
                ("It walks! The plants walk here! Grandma, the plants walk!", "Ele anda! As plantas andam aqui! Vó, as plantas andam!"));

            // -- Monte Echeyde (67) --
            Page(entries, "enemy_mountain_fumarole",
                "The old people of the island told that Guayota, down in the deep of the volcano, shut the sun inside Echeyde, and that the mountain still breathes it out of a thousand little vents. Some of the vents got up and walked off across the malpaís. Where a Fumarola breathes, the air goes yellow and stings, and it is hard to see your way. There is a spark of that shut-in sun glowing in its throat: when the cone squats and the spark swells, the big breath is coming. Hold your breath and step out of the cloud.",
                "O povo antigo da ilha contava que Guayota, lá no fundo do vulcão, prendeu o sol dentro do Echeyde, e que a montanha ainda solta o bafo dele por mil buraquinhos. Alguns buraquinhos levantaram e saíram andando pelo malpaís. Onde a Fumarola respira, o ar fica amarelo e arde, e fica difícil de ver o caminho. Tem uma faísca daquele sol preso brilhando na garganta dela: quando o cone se agacha e a faísca incha, o bafo grande vem. Prende a respiração e sai da nuvem.",
                ("It squats before the big breath. Out of the cloud, then strike.", "Ela se agacha antes do bafo grande. Sai da nuvem e depois bate."),
                ("It has a piece of the sun inside. It must be so warm and so sad.", "Ela tem um pedaço do sol dentro. Deve ser tão quentinha e tão triste."),
                ("It smells like eggs. Rotten eggs. A mountain of rotten eggs.", "Ela tem cheiro de ovo. Ovo podre. Uma montanha de ovo podre."),
                ("You can shut the sun in a mountain, but it keeps finding a crack to come out of. Light always does.", "Pode prender o sol numa montanha, que ele acha uma fresta pra sair. Luz sempre acha."),
                ("Pee-yew! Hold your nose! Everybody hold your nose!", "Eca! Tampa o nariz! Todo mundo tampa o nariz!"));

            // -- Castelo das Nove Fornalhas (69) --
            Page(entries, "enemy_mountain_living_bellows",
                "Every furnace of the castle had its bellows, and the smiths worked them all day long so the fire would never sleep. When the Fole Apagado got up in the great hall, the little bellows got up after it, and now they blow on their own, on the furnaces and at you. A Fole Vivo puffs up its leather before it blows, and the coal at the end of its nozzle goes bright. Do not stand in front of the nozzle: go round to the handles at the back, where it cannot aim.",
                "Toda fornalha do castelo tinha o seu fole, e os ferreiros trabalhavam neles o dia inteiro pro fogo nunca dormir. Quando o Fole Apagado levantou no salão grande, os folezinhos levantaram atrás, e agora sopram sozinhos, nas fornalhas e em vocês. O Fole Vivo enche o couro antes de soprar, e a brasa na ponta do bico acende. Não fica na frente do bico: dá a volta até os cabos de trás, que dali ele não mira.",
                ("It fills up before it blows. Get behind it, by the handles.", "Ele enche antes de soprar. Vai pra trás dele, pelos cabos."),
                ("It's still doing its job. Nobody told it the smiths went home.", "Ele ainda está fazendo o trabalho dele. Ninguém avisou que os ferreiros foram embora."),
                ("Fwoomp. Fwoomp. It's like a very hot accordion.", "Fuuum. Fuuum. Parece uma sanfona muito quente."),
                ("A bellows gives breath to the fire. Breath can make a fire bright, or blow a lamp out. Same breath.", "O fole dá fôlego pro fogo. Fôlego pode acender o fogo ou apagar a lamparina. O mesmo fôlego."),
                ("It has feet! Little wooden feet! Tap tap tap!", "Ele tem pé! Pezinho de madeira! Toc toc toc!"));

            // -- Cidadela dos Quatro Gulosos (46) --
            Page(entries, "enemy_forest_taowu_guard",
                "The old books of the Middle Kingdom name four fiends, and the fourth is Taowu, the stubborn one who will not listen. They draw it with a tiger's body and a man's face, a boar's tusks and hair two feet long, and a tail far longer than the beast itself, and they say it fought on and never once stepped back. In the citadel its guards are made like it, in the bronze collars of the old rites. A Guarda de Taowu does not chase and does not give way: it waits at its door, and when its jaw drops open it charges. Step aside, let it go past, and strike it from the flank.",
                "Os livros antigos do Reino do Meio falam de quatro feras, e a quarta é Taowu, o teimoso que não escuta ninguém. Desenham ele com corpo de tigre e cara de gente, presa de javali e pelo de dois palmos, e um rabo muito mais comprido que o bicho, e dizem que ele brigava sem nunca dar um passo pra trás. Na cidadela os guardas dele são feitos iguais, com as coleiras de bronze dos ritos antigos. O Guarda de Taowu não persegue e não cede: espera na porta, e quando a boca abre, ele avança. Passo pro lado, deixa ele passar e bate pelo flanco.",
                ("It opens its jaw before the charge. Let it pass, then hit the flank.", "Ele abre a boca antes de avançar. Deixa passar e bate do lado."),
                ("It never listens. I wonder if anyone ever talked to it gently.", "Ele nunca escuta. Será que alguém já falou com ele com jeitinho?"),
                ("A tiger with a grumpy uncle's face. I've met that uncle.", "Um tigre com cara de tio rabugento. Eu conheço esse tio."),
                ("Not stepping back can be courage or it can be deafness. The difference is whether you still listen.", "Não dar passo pra trás pode ser coragem ou pode ser surdez. A diferença é se você ainda escuta."),
                ("His tail is sooo long! It's longer than Joaquim!", "O rabo dele é muuuito comprido! É maior que o Joaquim!"));
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
