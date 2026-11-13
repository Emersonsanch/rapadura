using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of the Costa das Monções (Região 5, Docs/Mapas100.md 27–30) and the Delta de Jhilpur
    /// (Região 6, 32–35) that the overworld still played with another region's beast (bible §3.2.1, nineteenth pass): lore in Grandma Edna's
    /// voice and one line per cousin, en + pt-BR. Kept apart like <see cref="LentRoleBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to
    /// its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class CoastOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_kushite_ushabti", "enemy_desert_young_scorpion_king", "enemy_desert_black_sand_jackal", "enemy_cave_mud_jellyfish",
            "enemy_cave_dimmed_lionfish", "enemy_cave_kongamato", "enemy_cave_thief_octopus", "enemy_forest_snare_root",
            "enemy_forest_ghost_heron", "enemy_cave_smoke_dragonfly", "enemy_mountain_smoke_bubble", "enemy_cave_mud_child",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Pirâmides Altas de Meroé (27) --
            Page(entries, "enemy_mountain_kushite_ushabti",
                "The kings of Meroë were buried under their steep little pyramids with hundreds of bronze figures, each with a hoe and a basket, to work the fields of the other world for them. The shadow came into the chapels and woke them up without telling them which field. So they work on whoever passes. They are slow and heavy. Watch the hoe go up, and step out from under it.",
                "Os reis de Meroé foram enterrados debaixo das pirâmides pontudas deles com centenas de figuras de bronze, cada uma com enxada e cesto, pra trabalhar os campos do outro mundo por eles. A sombra entrou nas capelas e acordou as figuras sem dizer qual campo. Aí elas trabalham em quem passa. São lentas e pesadas. Olhem a enxada subir e saiam de baixo.",
                ("Slow swing, big hit. Wait for the hoe, then step in.", "Golpe lento, pancada grande. Espera a enxada, depois entra."),
                ("They only want to be told which field. That's sad and a bit sweet.", "Eles só querem que alguém diga qual campo. Isso é triste e meio fofo."),
                ("Hundreds of them. HUNDREDS. Who buries hundreds of gardeners?", "Centenas deles. CENTENAS. Quem enterra centenas de jardineiros?"),
                ("The ram on its chest still has lit eyes. The spell is still in there, waiting.", "O carneiro no peito ainda tem os olhos acesos. O feitiço ainda tá lá dentro, esperando."),
                ("It has a little basket! For seeds!", "Ele tem um cestinho! Pra semente!"));

            Page(entries, "enemy_desert_young_scorpion_king",
                "In the stories of the pyramid fields, the first king of the river had a scorpion for a name. The young scorpions of Meroë have always walked about as if they had inherited it, little gold crown and all, and nobody minded. Since the shadow they act on it: they rule the sand between the chapels and chase off anyone who did not bow. They are quick. Mind the sting when the tail draws back.",
                "Nas histórias dos campos de pirâmides, o primeiro rei do rio tinha nome de escorpião. Os escorpiões jovens de Meroé sempre andaram como se tivessem herdado o nome, com coroinha de ouro e tudo, e ninguém ligava. Desde a sombra eles levam a sério: mandam na areia entre as capelas e correm atrás de quem não fez reverência. São rápidos. Cuidado com o ferrão quando o rabo puxar pra trás.",
                ("The tail pulls back before the sting. That's your moment to dodge.", "O rabo puxa pra trás antes do ferrão. É a hora de desviar."),
                ("He's just a little one playing king. Let's not be too hard on him.", "Ele é pequenininho brincando de rei. Não vamos ser duros demais com ele."),
                ("I'm not bowing to a scorpion. I have standards.", "Eu não vou fazer reverência pra escorpião. Eu tenho padrão."),
                ("A crown makes anything think it owns the place. The shadow likes crowns.", "Coroa faz qualquer um achar que é dono do lugar. A sombra gosta de coroa."),
                ("His crown is so tiny! Does it fall off?", "A coroa dele é tão pequenininha! Ela não cai?"));

            Page(entries, "enemy_desert_black_sand_jackal",
                "The black jackals of Meroë are the ones painted on the chapel doors. They used to show the way between the pyramids, so nobody got lost in the sand, and they wore the beaded collars the priests gave them. Since the shadow they still lead, in packs, but the way they show goes nowhere, and they bite whoever follows. Never follow a black jackal. Make them come to you.",
                "Os chacais pretos de Meroé são os que estão pintados nas portas das capelas. Eles mostravam o caminho entre as pirâmides, pra ninguém se perder na areia, e usavam as coleiras de contas que os sacerdotes davam. Desde a sombra ainda guiam, em bando, mas o caminho que mostram não dá em lugar nenhum, e mordem quem vai atrás. Nunca sigam um chacal preto. Façam ele vir até vocês.",
                ("They come in packs. Back to a wall and take them one at a time.", "Vêm em bando. Costas pra parede e um de cada vez."),
                ("They still wear their collars. Somebody loved them once.", "Eles ainda usam a coleira. Alguém gostou deles um dia."),
                ("A jackal giving directions. And I thought the map was bad.", "Chacal dando direção. E eu achando que o mapa era ruim."),
                ("A guide that leads nowhere. That's the shadow's whole trick, isn't it.", "Um guia que leva pra lugar nenhum. É o truque inteiro da sombra, né."),
                ("His ears are taller than his head!", "As orelhas dele são mais altas que a cabeça!"));

            // -- Atol de Kirta e Jardins de Alga (28) --
            Page(entries, "enemy_cave_mud_jellyfish",
                "The jellyfish of Kirta's reef are clear as the water, and they carry the coral's light on their backs from one head of coral to another, so the whole reef shines. The Lodo got into some of them, and they went brown and heavy. They don't float any more; they walk the reef on their tentacles and sting with the mud. Hit the bell, not the tentacles.",
                "As águas-vivas do recife de Kirta são claras que nem a água, e carregam a luz do coral nas costas de uma cabeça de coral pra outra, pra o recife inteiro brilhar. O Lodo entrou em algumas, e elas ficaram marrons e pesadas. Não boiam mais; andam pelo recife nos tentáculos e queimam com a lama. Batam no sino, não nos tentáculos.",
                ("Go for the bell. When it flattens, it's about to sting.", "Mira no sino. Quando ele achata, vai queimar."),
                ("There's still a bit of light inside. Let's give it back to the reef.", "Ainda tem um pouquinho de luz lá dentro. Vamos devolver pro recife."),
                ("A jellyfish that walks. I'm going back to the beach.", "Água-viva que anda. Eu vou voltar pra praia."),
                ("It was a carrier of light. The Lodo made it a carrier of mud. Same job, wrong cargo.", "Ela carregava luz. O Lodo fez ela carregar lama. Mesmo serviço, carga errada."),
                ("It looks like a mushroom with a skirt!", "Parece um cogumelo de saia!"));

            Page(entries, "enemy_cave_dimmed_lionfish",
                "The lionfish of the atoll were the coral's guards, red and white so everyone could see them from far off and keep away. That was the whole point: they never had to fight. The shadow greyed the white of their stripes, and now they come looking for trouble, and flick the light gathered at the tips of their spines from far away. Close in fast and don't touch the spines.",
                "Os peixes-leão do atol eram os guardas do coral, vermelhos e brancos pra todo mundo ver de longe e manter distância. A graça era essa: eles nunca precisavam brigar. A sombra deixou o branco das listras cinza, e agora eles vêm procurar briga, e jogam de longe a luz que junta na ponta dos espinhos. Cheguem perto rápido e não encostem nos espinhos.",
                ("It shoots from far. Close the gap, and stay off its spines.", "Ele atira de longe. Encurta a distância e fica longe dos espinhos."),
                ("It was the kind of guard that never had to fight. Poor thing doesn't know how to stop.", "Ele era o tipo de guarda que nunca precisou brigar. Coitado não sabe parar."),
                ("Why is everything in this reef spiky? Even the fish is spiky.", "Por que tudo nesse recife tem espinho? Até o peixe tem espinho."),
                ("Bright colours meant 'stay away'. Grey ones mean nothing at all. That's worse.", "Cor viva queria dizer 'fica longe'. Cinza não quer dizer nada. Isso é pior."),
                ("He has a lion mane made of fins!", "Ele tem juba de leão feita de barbatana!"));

            // -- Kilwa Afundada (29) --
            Page(entries, "enemy_cave_thief_octopus",
                "The octopuses of Kilwa lived in the jars of the drowned houses and kept them tidy, every bead in its place. Since the shadow they keep things that are not theirs: lamps, beads, the brass pins off the carved doors. Every arm holds something, and it won't let go. It is slow, but it grabs and pulls. Stay out of reach of the arms and hit it when it squashes down.",
                "Os polvos de Kilwa moravam nos jarros das casas afundadas e deixavam tudo arrumado, cada conta no lugar. Desde a sombra eles guardam o que não é deles: lamparinas, contas, os pinos de latão das portas entalhadas. Cada braço segura alguma coisa, e não solta. Ele é lento, mas agarra e puxa. Fiquem longe dos braços e batam quando ele achatar.",
                ("When it squashes down, it's about to grab. Step back, then hit.", "Quando ele achata, vai agarrar. Dá um passo atrás e depois bate."),
                ("Maybe it's just lonely in the drowned houses and wants something to hold.", "Vai ver ele só tá sozinho nas casas afundadas e quer alguma coisa pra segurar."),
                ("Eight arms and every single one stealing. That's a professional.", "Oito braços e cada um roubando. Isso é profissional."),
                ("It took the lamp, and the lamp is still lit. Things don't stop being what they are just because the wrong one holds them.", "Ele pegou a lamparina, e ela continua acesa. As coisas não deixam de ser o que são só porque o errado segura."),
                ("He has a necklace and a box and a lamp! He's dressed for a party!", "Ele tem colar, caixinha e lamparina! Tá arrumado pra festa!"));

            // -- Canais e Mangues de Liyongo (30) --
            Page(entries, "enemy_cave_kongamato",
                "The boatmen of Liyongo's channels say the kongamato only ever upset canoes, and only the canoes of those who poled too fast past its roost in the mangroves. It was a rude bird, but a fair one. The shadow made it upset every boat, and it throws its hard skin-feathers at whoever is in the channel. Keep moving under it, and hit it when it comes down to perch.",
                "Os barqueiros dos canais de Liyongo dizem que o kongamato só virava canoa, e só a canoa de quem passava remando rápido demais perto do poleiro dele no mangue. Era um bicho mal-educado, mas justo. A sombra fez ele virar todo barco, e ele joga as penas de couro duras em quem estiver no canal. Fiquem andando embaixo dele e batam quando ele descer pra pousar.",
                ("It throws from the air. Move, then hit it when it lands.", "Ele joga lá do alto. Anda, e bate quando ele pousar."),
                ("He used to be fair. I think he still wants to be.", "Ele era justo. Acho que ainda quer ser."),
                ("Its beak has TEETH. Birds aren't supposed to have teeth.", "O bico dele tem DENTE. Pássaro não devia ter dente."),
                ("A rule that was fair, turned into a rule for everyone. That's how the shadow breaks things.", "Uma regra que era justa virou regra pra todo mundo. É assim que a sombra quebra as coisas."),
                ("His wings look like umbrellas!", "As asas dele parecem guarda-chuva!"));

            // -- Pontes de Raiz Viva, Figueiral e Gargantas (32) --
            Page(entries, "enemy_forest_snare_root",
                "The banyans of the gorges let their aerial roots down to the water and tie the bridges of Jhilpur with them, knot by knot, over many years. The shadow taught a few of those roots to tie knots for themselves. They come down from the branches and walk the bridges on their rootlets, looking for an ankle to tie. Watch the noose: when it opens, jump back.",
                "As figueiras das gargantas descem as raízes do ar até a água e amarram as pontes de Jhilpur com elas, nó por nó, por muitos anos. A sombra ensinou umas dessas raízes a dar nó sozinhas. Elas descem dos galhos e andam pelas pontes nas raizinhas, procurando um tornozelo pra amarrar. Olhem o laço: quando ele abrir, pulem pra trás.",
                ("When the loop opens, jump back. Then cut the knot.", "Quando o laço abrir, pula pra trás. Depois corta o nó."),
                ("It's the same root that holds the bridges up. Let's be gentle with it.", "É a mesma raiz que segura as pontes. Vamos ter jeito com ela."),
                ("A plant that walks AND ties knots. I can't even tie my shoes.", "Uma planta que anda E dá nó. Eu não sei nem amarrar o tênis."),
                ("Knots that hold a bridge, or knots that hold a foot. The root doesn't know the difference any more.", "Nó que segura ponte ou nó que segura pé. A raiz não sabe mais a diferença."),
                ("It walks on its little roots like a crab!", "Ela anda nas raizinhas que nem caranguejo!"));

            // -- Palafitas de Tarchon e Ilhas de Junco (33) --
            Page(entries, "enemy_forest_ghost_heron",
                "The herons of Tarchon fish beside the net-menders and take what falls through the nets, and nobody ever minded; there was enough for everyone. The shadow came over the marsh in the lilac mist, and the herons that stood in it went pale and thin as the mist itself. Now they take from the nets while the menders watch, and peck at whoever comes close. They are quick on the beak. Keep your eyes on the head.",
                "As garças de Tarchon pescam do lado dos remendadores de rede e pegam o que cai pelas malhas, e ninguém nunca ligou; tinha pra todo mundo. A sombra chegou no pântano na névoa lilás, e as garças que ficaram paradas nela ficaram pálidas e finas que nem a própria névoa. Agora tiram das redes com os remendadores olhando, e bicam quem chega perto. São rápidas no bico. Fiquem de olho na cabeça.",
                ("The neck pulls back before the peck. Step aside then.", "O pescoço puxa pra trás antes da bicada. Sai de lado nessa hora."),
                ("There was enough for everyone. I wish she remembered that.", "Tinha pra todo mundo. Queria que ela lembrasse disso."),
                ("A ghost. A bird ghost. That steals fish. This place is weird.", "Um fantasma. Um fantasma de pássaro. Que rouba peixe. Esse lugar é estranho."),
                ("You can see the shadow through her back, like water under ice.", "Dá pra ver a sombra pelas costas dela, que nem água debaixo do gelo."),
                ("She's see-through! Like a jellyfish with legs!", "Ela é transparente! Que nem água-viva de perna!"));

            // -- Charco do Ninki Nanka e Ilhota das Lanternas (34) --
            Page(entries, "enemy_cave_smoke_dragonfly",
                "Over the Ninki Nanka's marsh the dragonflies hunt the midges at dusk, green and quick, and they keep the boatmen from being eaten alive. The dragon's sleep went sour with the shadow, and her breath comes out as smoke; the dragonflies that flew through it became smoke too. They hunt in swarms over the reeds now, and they hunt us. Swing wide; they come from all sides.",
                "No charco da Ninki Nanka as libélulas caçam os mosquitinhos no fim da tarde, verdes e ligeiras, e não deixam os barqueiros serem comidos vivos. O sono da dragoa azedou com a sombra, e o bafo dela sai em fumaça; as libélulas que voaram nela viraram fumaça também. Agora caçam em bando por cima dos juncos, e caçam a gente. Batam de lado a lado; elas vêm de todo canto.",
                ("They come in a swarm. Wide swings, and keep moving.", "Vêm em enxame. Golpe largo e continua andando."),
                ("They used to protect the boatmen. Maybe they will again.", "Elas protegiam os barqueiros. Quem sabe voltam a proteger."),
                ("I used to like dragonflies. Used to.", "Eu gostava de libélula. Gostava."),
                ("Bad sleep makes bad breath, and bad breath makes bad dragonflies. The shadow works in chains.", "Sono ruim faz bafo ruim, e bafo ruim faz libélula ruim. A sombra trabalha em corrente."),
                ("Her eyes are as big as her head!", "Os olhos dela são do tamanho da cabeça!"));

            // -- Raízes Afogadas e Olho do Lodo (35) --
            Page(entries, "enemy_mountain_smoke_bubble",
                "In the heart of the Lodo the black mud breathes, slow as a sleeper, and every breath comes up as a bubble of smoke. Most of them burst on the surface and are gone. The ones the shadow holds together float off over the drowned roots, and they burst on whoever they find, and the smoke makes your legs heavy. Hit them from as far as you can, and step out of the smoke.",
                "No coração do Lodo a lama preta respira, devagar que nem quem dorme, e cada respiração sobe como uma bolha de fumaça. A maioria estoura na superfície e some. As que a sombra segura inteiras saem boiando por cima das raízes afogadas, e estouram em quem encontram, e a fumaça deixa as pernas pesadas. Batam de o mais longe que der, e saiam da fumaça.",
                ("When it swells up, it's about to burst. Get out of the smoke.", "Quando ela incha, vai estourar. Sai da fumaça."),
                ("It's only a breath. It doesn't even know it's alive.", "É só uma respiração. Ela nem sabe que tá viva."),
                ("I'm fighting a burp. A swamp burp. Write that in the book, Grandma.", "Eu tô lutando com um arroto. Um arroto de pântano. Escreve isso no caderno, vó."),
                ("If the mud breathes, it's sleeping. If it's sleeping, it can wake. Let's be quick.", "Se a lama respira, ela dorme. Se dorme, pode acordar. Vamos ser rápidos."),
                ("It's like the bubbles in the bath, but grumpy!", "É que nem bolha do banho, mas emburrada!"));

            Page(entries, "enemy_cave_mud_child",
                "The Lodo-que-Engole has no children of its own. The Filhos do Lodo are bits of it that learnt to swim, little lumps of mud with arms and a mouth. They climb the ghats of Jhilpur in the Maré de Lodo and swim the dark water of the heart, and wherever they go they try to swallow a little clean water to make it black. Some of them have swallowed a drop of the Água Viva, and it shines through their bellies. Free that drop.",
                "O Lodo-que-Engole não tem filho nenhum. Os Filhos do Lodo são pedaços dele que aprenderam a nadar, bolinhos de lama com braço e boca. Sobem os ghats de Jhilpur na Maré de Lodo e nadam na água escura do coração, e por onde passam tentam engolir um pouco de água limpa pra deixar preta. Alguns já engoliram uma gota da Água Viva, e ela brilha pela barriga deles. Soltem essa gota.",
                ("They come in waves. Don't let them surround you.", "Vêm em onda. Não deixa eles te cercarem."),
                ("There's a drop of the Água Viva in its belly. It's not lost, just swallowed.", "Tem uma gota da Água Viva na barriga dele. Não tá perdida, só engolida."),
                ("A mud baby. With arms. Hugging me. I need a bath.", "Um bebê de lama. Com braço. Me abraçando. Eu preciso de um banho."),
                ("Bits of the Lodo that learnt to swim. Then bits of the Lodo can learn to stop.", "Pedaços do Lodo que aprenderam a nadar. Então pedaços do Lodo podem aprender a parar."),
                ("It's like a little mud pie that walks!", "Parece um bolinho de lama que anda!"));
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
