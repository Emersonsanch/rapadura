using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve most-seen roles of the Picos de Farnkesh (Região 10, Docs/Mapas100.md 57–58), the Mar Gelado
    /// (Região 11, 60–64) and the Arquipélago de Hinokura (Região 12, 68–70) that the overworld still played with another beast (bible §3.2.1,
    /// twenty-first pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="HighlandOwnBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class NorthOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_flying_stone", "enemy_mountain_gorge_ays", "enemy_desert_trapped_lightning", "enemy_mountain_ice_fish",
            "enemy_desert_lantern_eel", "enemy_desert_tundra_mosquito", "enemy_mountain_dimmed_narwhal", "enemy_mountain_storm_gull",
            "enemy_cave_witch_fish", "enemy_cave_obsidian_octopus", "enemy_desert_raging_spark", "enemy_cave_living_ash",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Garganta dos Ays, Gruta Abrigo, Escarpa dos Ventos (57) --
            Page(entries, "enemy_mountain_flying_stone",
                "In the gorge of the Ays the destroying wind blows so hard that it lifts the loose stones off the scree and throws them about like hail. Most fall back down and stay. The ones the shadow got into stay up, spinning in the wind with a grey dust round them, and dive at whoever walks the gorge. They are heavy and slow to turn. Shelter behind the rocks the wind cannot reach, and hit them when they land to catch their breath.",
                "Na garganta dos Ays o vento destruidor sopra tão forte que levanta as pedras soltas da encosta e joga pra todo lado que nem granizo. A maioria cai de volta e fica. As que a sombra pegou ficam lá em cima, girando no vento com uma poeira cinza em volta, e mergulham em quem anda pela garganta. São pesadas e demoram pra virar. Se abriguem atrás das pedras onde o vento não chega, e batam quando elas pousam pra tomar fôlego.",
                ("Wait for it to land. Then hit it hard.", "Espera ela pousar. Aí bate com força."),
                ("It wants to go back to the ground. I'll help it, gently.", "Ela quer voltar pro chão. Eu ajudo, com jeito."),
                ("I caught one! It was a bad idea. My hands still hurt.", "Eu peguei uma! Foi má ideia. Minha mão ainda dói."),
                ("A stone was made to stay put. The shadow loves to make things forget what they are.", "Pedra foi feita pra ficar parada. A sombra adora fazer as coisas esquecerem o que são."),
                ("A stone that flies! Does it have wings inside?", "Uma pedra que voa! Ela tem asa por dentro?"));

            Page(entries, "enemy_mountain_gorge_ays",
                "The people of the peaks say the Ays are the spirits of the ill wind, and that you only see them by the dust they carry. In the gorge they run with the gusts, quick as a slap, and you feel them before you see them. Since the shadow came the dust they carry is grey and full of ash, and they bite. Walk with your back to a rock when the wind turns; the dust always shows where they are.",
                "O povo dos picos diz que os Ays são os espíritos do vento ruim, e que só dá pra ver eles pela poeira que carregam. Na garganta eles correm com as rajadas, rápidos que nem um tapa, e a gente sente antes de ver. Desde que a sombra chegou a poeira deles é cinza e cheia de cinza de fogueira, e eles mordem. Andem de costas pra pedra quando o vento vira; a poeira sempre mostra onde eles estão.",
                ("Look at the dust, not at the wind. It shows you where to hit.", "Olha a poeira, não o vento. Ela mostra onde bater."),
                ("They used to only push the clouds about. They were never mean.", "Eles só empurravam as nuvens. Nunca foram maus."),
                ("One stole my hat. I'm going to get my hat back.", "Um roubou meu chapéu. Eu vou pegar meu chapéu de volta."),
                ("Maithe says she can hear them argue with the Vento Vivo. The Vento Vivo is winning.", "A Maithe diz que escuta eles brigando com o Vento Vivo. O Vento Vivo tá ganhando."),
                ("It's a whirlwind with a face! It's tickling me!", "É um redemoinho de cara! Tá me fazendo cócega!"));

            // -- Ilhas de Nuvem, Ninho da Huma (58) --
            Page(entries, "enemy_desert_trapped_lightning",
                "Up above the clouds, round Huma's nest, the storms leave little bolts of lightning behind, and the huma gathers them into her nest to keep the eggs warm. The shadow caught some of them in cages of cloud before they reached her, and now they hang over the isles crackling and throw themselves at whoever comes near the nest. They shoot from far off. Run in under them while they gather the next spark.",
                "Lá em cima das nuvens, em volta do ninho da Huma, as tempestades deixam raiozinhos pra trás, e a huma junta eles no ninho pra esquentar os ovos. A sombra prendeu alguns em gaiolas de nuvem antes de chegarem até ela, e agora eles ficam pendurados em cima das ilhas estalando e se jogam em quem chega perto do ninho. Atiram de longe. Corram por baixo enquanto eles juntam a próxima faísca.",
                ("It shoots from far. Close in while it charges.", "Atira de longe. Chega perto enquanto ele carrega."),
                ("It only wanted to keep the eggs warm. Let's open its cage.", "Ele só queria esquentar os ovos. Vamos abrir a gaiola dele."),
                ("My hair is standing up. All of it. Even the eyebrows.", "Meu cabelo tá em pé. Todo. Até a sobrancelha."),
                ("A light kept in a cage still pulls at the bars. That's how you know it is light.", "Luz presa numa gaiola ainda puxa as grades. É assim que a gente sabe que é luz."),
                ("It's a lightning in a little cloud jar! Can I keep it?", "É um raio num potinho de nuvem! Posso ficar com ele?"));

            // -- Canais de Gelo, Canais do Oeste (60) --
            Page(entries, "enemy_mountain_ice_fish",
                "The ice fish of the canals are thin as a knife and white as the ice, and the boatmen say they swim right through the floes without breaking them. They used to follow the boats for the scraps of the nets. The shadow grew a frost on their backs, and now they come up through the thin ice in shoals and bite at the ankles of whoever walks the canals. Keep moving, and fight them where the ice is thick.",
                "Os peixes-gelo dos canais são finos que nem faca e brancos que nem o gelo, e os barqueiros dizem que eles nadam por dentro das placas sem quebrar. Eles seguiam os barcos atrás das sobras das redes. A sombra fez crescer uma geada nas costas deles, e agora eles sobem pelo gelo fino em cardume e mordem o tornozelo de quem anda pelos canais. Não fiquem parados, e lutem onde o gelo é grosso.",
                ("They come in shoals. Fight on the thick ice.", "Eles vêm em cardume. Luta no gelo grosso."),
                ("Yirkap still leaves them a little fish at the end of the day.", "O Yirkap ainda deixa um peixinho pra eles no fim do dia."),
                ("One of them swallowed Grandpa's tin, and I have the bite marks to prove I got it back.", "Um deles engoliu a lata do Vô, e eu tenho a marca da mordida pra provar que peguei de volta."),
                ("They swim through the ice without breaking it. The shadow can't break that in them.", "Eles nadam pelo gelo sem quebrar. Isso a sombra não consegue quebrar neles."),
                ("It's see-through! I can see its little bones!", "Ele é transparente! Dá pra ver os ossinhos dele!"));

            Page(entries, "enemy_desert_lantern_eel",
                "In the black water under the canals live the lantern eels, long and slick, with a row of little lights down their sides that the boatmen used to steer by on the polar nights. The shadow dimmed the lights to a sickly glow, and the eels come up at the cracks to snap at whoever passes, and slide back down before you can strike. They are quick and slippery. Hit them the moment they come up, not after.",
                "Na água preta debaixo dos canais moram as enguias-lanterna, compridas e escorregadias, com uma fileira de luzinhas do lado que os barqueiros usavam pra se guiar nas noites polares. A sombra apagou as luzes até um brilho doente, e as enguias sobem nas rachaduras pra abocanhar quem passa, e escorregam de volta antes da gente bater. São rápidas e lisas. Batam na hora que elas sobem, não depois.",
                ("Hit it the moment it comes up. It doesn't wait.", "Bate na hora que ela sobe. Ela não espera."),
                ("The boatmen miss their lights. On the long nights they get lost.", "Os barqueiros sentem falta das luzes. Nas noites compridas eles se perdem."),
                ("I tried to grab one. It's like holding soap that bites.", "Eu tentei agarrar uma. É que nem segurar sabonete que morde."),
                ("A light that shows the way, made to hide in the dark. The shadow turned it inside out.", "Uma luz que mostra o caminho, feita pra esconder no escuro. A sombra virou ela do avesso."),
                ("She has a necklace of lights! I want one!", "Ela tem colar de luz! Eu quero um!"));

            // -- Tundra do Sul, Lagos de Omöl (61) --
            Page(entries, "enemy_desert_tundra_mosquito",
                "In the short summer of the tundra the lakes of Omöl breed mosquitoes by the cloud, and the reindeer herders say it is Omöl's way of reminding everyone he is still there. They were only a nuisance. The shadow got into a few swarms and they grew big and grey, with a bead of stolen light in each belly, and they go for the herds and the herders alike. They come in a cloud. Fight with your back to the wind, and keep swinging.",
                "No verão curto da tundra os lagos de Omöl criam mosquito às nuvens, e os pastores de rena dizem que é o jeito do Omöl lembrar todo mundo que ele ainda tá lá. Eram só um incômodo. A sombra entrou em alguns enxames e eles ficaram grandes e cinzentos, com uma gotinha de luz roubada em cada barriga, e atacam as manadas e os pastores do mesmo jeito. Vêm em nuvem. Lutem de costas pro vento, e não parem de bater.",
                ("They come in a cloud. Keep swinging.", "Eles vêm em nuvem. Não para de bater."),
                ("Even a mosquito has a little light in it. I'll let it out.", "Até mosquito tem uma luzinha dentro. Eu deixo ela sair."),
                ("I have nine bites on one ear. I counted. Nine.", "Eu tenho nove picadas numa orelha. Contei. Nove."),
                ("Omöl made them clumsy on purpose. The shadow made them hungry. That is worse.", "O Omöl fez eles desajeitados de propósito. A sombra fez eles com fome. Isso é pior."),
                ("They buzz like a song! A very annoying song.", "Eles zumbem que nem música! Uma música muito chata."));

            // -- Banquisas, Mar das Auroras, Ilha do Farol (63) --
            Page(entries, "enemy_mountain_dimmed_narwhal",
                "The narwhals of the Aurora Sea carry a long twisted tusk that the people of the Lua Branca say catches the light of the auroras and brings it under the ice for the fish. The shadow dimmed the tusks, and the narwhals come up between the floes now and charge whoever stands on the edge. They are fast in the water and slow on the ice. Stay away from the edge, and make them come up to you.",
                "Os narvais do Mar das Auroras têm um dente comprido e torcido que o povo da Lua Branca diz que pega a luz das auroras e leva pra debaixo do gelo pros peixes. A sombra apagou os dentes, e agora os narvais sobem entre as placas e investem em quem fica na beirada. São rápidos na água e lentos no gelo. Fiquem longe da beirada, e façam eles subirem até vocês.",
                ("Stay off the edge. Make it come to you.", "Sai da beirada. Faz ele vir até você."),
                ("The fish under the ice are in the dark without them. Let's bring the light back.", "Os peixes debaixo do gelo tão no escuro sem eles. Vamos trazer a luz de volta."),
                ("That tusk is longer than me. I measured. With my own body. Lying down.", "Esse dente é maior que eu. Eu medi. Com meu corpo. Deitado."),
                ("A tusk that carries light down into the dark. That is what the auroras are for.", "Um dente que leva luz pra dentro do escuro. É pra isso que servem as auroras."),
                ("It's a sea unicorn! A grumpy sea unicorn!", "É um unicórnio do mar! Um unicórnio do mar emburrado!"));

            Page(entries, "enemy_mountain_storm_gull",
                "The storm gulls ride the gales of the Aurora Sea with their wings stiff as boards, and the fishermen of the Lua Branca read the weather by how high they fly. The shadow put a grey in their feathers and they no longer warn anyone of the storm: they bring it, screaming down from the lighthouse and throwing their hard feathers from far off. Wait for the throw, then strike while they turn in the wind.",
                "As gaivotas-tempestade montam as ventanias do Mar das Auroras com as asas duras que nem tábua, e os pescadores da Lua Branca leem o tempo pela altura que elas voam. A sombra pôs um cinza nas penas delas e elas não avisam mais ninguém da tempestade: elas trazem ela, descendo do farol gritando e jogando as penas duras de longe. Esperem o arremesso, e ataquem enquanto elas viram no vento.",
                ("Wait for the feathers, then hit it on the turn.", "Espera as penas, depois bate na virada."),
                ("They used to warn the boats. They can warn them again.", "Elas avisavam os barcos. Podem avisar de novo."),
                ("One of them took my fish. My whole fish. Right out of my hand.", "Uma pegou meu peixe. O peixe inteiro. Da minha mão."),
                ("A bird that read the storm, made to be the storm. The shadow never makes, it only turns.", "Um pássaro que lia a tempestade, feito pra ser a tempestade. A sombra nunca faz, só vira."),
                ("Their wings don't move! They're surfing on the wind!", "As asas delas não mexem! Elas tão surfando no vento!"));

            // -- Buracos de Pesca, Lago do Kul, Gelo Negro (64) --
            Page(entries, "enemy_cave_witch_fish",
                "In the bottom of Old Kul's lake live the witch fish, eyeless and slow, that the old fishermen say tidy the lake by eating what falls to the bottom. They have no bones to speak of and they knot themselves to get loose. Since the Kul went grumpy the shadow got into them, and they come up through the fishing holes in a cloud of slime that slows whoever it touches. Don't fight in their slime; draw them out of it.",
                "No fundo do lago do Velho Kul moram os peixes-bruxa, sem olho e lentos, que os pescadores velhos dizem que arrumam o lago comendo o que cai no fundo. Eles quase não têm osso e se dão um nó pra se soltar. Desde que o Kul ficou rabugento a sombra entrou neles, e eles sobem pelos buracos de pesca numa nuvem de gosma que deixa lento quem encosta. Não lutem na gosma; puxem eles pra fora dela.",
                ("Don't stand in the slime. Pull it out first.", "Não fica na gosma. Puxa ele pra fora primeiro."),
                ("They keep the lake clean. The lake needs them back.", "Eles deixam o lago limpo. O lago precisa deles de volta."),
                ("It tied itself in a knot. Then it untied itself on my boot.", "Ele deu um nó nele mesmo. Depois desatou na minha bota."),
                ("They have no eyes and still find the way. The shadow can't blind what never looked.", "Eles não têm olho e ainda acham o caminho. A sombra não cega quem nunca olhou."),
                ("It's so slimy! Can I touch it? I'm touching it.", "Ele é tão gosmento! Posso pegar? Já peguei."));

            // -- Canais Quentes, Recife de Obsidiana (68) --
            Page(entries, "enemy_cave_obsidian_octopus",
                "In the warm channels of the obsidian reef the octopuses hide in the glass and change colour with it, black and red and gold, and the smiths of Hinokura say they taught them the patterns of the lacquer. The shadow turned them the colour of cold glass, and now they lie in wait in the reef and wrap whoever comes near in arms that hold fast. Keep clear of its arms and strike at its head when it lets go.",
                "Nos canais quentes do recife de obsidiana os polvos se escondem no vidro e mudam de cor junto com ele, preto e vermelho e dourado, e os ferreiros de Hinokura dizem que foram eles que ensinaram os desenhos da laca. A sombra deixou eles da cor de vidro frio, e agora eles ficam de tocaia no recife e enrolam quem chega perto com braços que seguram firme. Fiquem longe dos braços e batam na cabeça quando ele solta.",
                ("Keep off its arms. Hit the head when it lets go.", "Sai dos braços. Bate na cabeça quando ele soltar."),
                ("The smiths want their teachers back. Let's be careful with it.", "Os ferreiros querem os professores de volta. Vamos com cuidado."),
                ("It had me by the leg. Eight arms is not fair.", "Ele me pegou pela perna. Oito braços não é justo."),
                ("A creature that teaches beauty by changing. The shadow made it one colour. That is its whole trick.", "Um bicho que ensina beleza mudando. A sombra deixou ele de uma cor só. O truque dela é esse."),
                ("It's made of glass! A glass octopus! Don't drop it!", "Ele é de vidro! Um polvo de vidro! Não deixa cair!"));

            // -- Castelo, Pátio das Fornalhas, Escória Velha (69) --
            Page(entries, "enemy_desert_raging_spark",
                "The nine furnaces of the castle throw sparks all night, and the smiths say every spark is a little bit of the Fogo Vivo that wants to go home to the sky. Most of them go out on the way. The shadow catches some before they rise, and they come back down angry, hot and buzzing in swarms round the courtyard, burning whoever stands close. They are many and small. Keep swinging, and stay away from the furnace mouths.",
                "As nove fornalhas do castelo soltam faísca a noite toda, e os ferreiros dizem que cada faísca é um pedacinho do Fogo Vivo que quer voltar pro céu. A maioria apaga no caminho. A sombra pega algumas antes de subirem, e elas descem de volta raivosas, quentes e zumbindo em enxame pelo pátio, queimando quem fica perto. São muitas e pequenas. Não parem de bater, e fiquem longe da boca das fornalhas.",
                ("Many and small. Keep swinging, keep moving.", "Muitas e pequenas. Bate e anda."),
                ("They only wanted to go up. Let's show them the way.", "Elas só queriam subir. Vamos mostrar o caminho."),
                ("One of them landed on my nose. It was very rude about it.", "Uma pousou no meu nariz. Foi muito mal-educada."),
                ("A spark of light that wanted the sky, held down until it is angry. That is all the anger of the shadow is.", "Uma faísca de luz que queria o céu, segurada até ficar com raiva. A raiva da sombra é só isso."),
                ("They're like angry fireflies! Very angry fireflies!", "São que nem vaga-lumes bravos! Vaga-lumes muito bravos!"));

            // -- Encosta da Caldeira, Cinzas Vivas, Borda Fria (70) --
            Page(entries, "enemy_cave_living_ash",
                "In the caldera of the Devouring Fire the ash lies so deep that it moves when you step on it. Some of it moves when no one steps on it at all: the shadow breathed into the warm ash of what the fire ate, and it rises in heaps with two coals for eyes and drags itself after whoever crosses the caldera. It is slow and heavy and smothers what it covers. Don't let it surround you, and scatter it before it gathers again.",
                "Na caldeira do Fogo Devorador a cinza é tão funda que mexe quando a gente pisa. Uma parte mexe sem ninguém pisar: a sombra soprou na cinza quente do que o fogo comeu, e ela levanta em montes com duas brasas no lugar dos olhos e se arrasta atrás de quem cruza a caldeira. É lenta e pesada e abafa o que cobre. Não deixem ela cercar vocês, e espalhem antes que ela se junte de novo.",
                ("Don't let it surround you. Scatter it.", "Não deixa ela cercar. Espalha."),
                ("It was something once, before the fire ate it. It deserves to rest.", "Ela era alguma coisa antes do fogo comer. Merece descansar."),
                ("I sneezed and it got angrier. I didn't mean to.", "Eu espirrei e ela ficou mais brava. Foi sem querer."),
                ("What the Devouring Fire leaves behind still wants to be something. Even ash remembers the light.", "O que o Fogo Devorador deixa pra trás ainda quer ser alguma coisa. Até a cinza lembra da luz."),
                ("It's a sandcastle that walks! A grey sandcastle!", "É um castelo de areia que anda! Um castelo de areia cinza!"));
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
