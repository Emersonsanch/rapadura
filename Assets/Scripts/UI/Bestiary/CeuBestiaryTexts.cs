using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Reinos do Céu (Docs/Mapas100.md 91–99) that the overworld layout plays with stand-ins:
    /// lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="ProfundoBestiaryTexts"/>; <see cref="BestiaryTexts"/>
    /// appends these to its <see cref="BestiaryTexts.Entries"/>. Quiet, pale and a little silly, never gore (bible §4.3); the archons are story
    /// giants who swallowed too much light (bible §1.4), and the Father of Greatness never appears (bible §0.2).
    /// </summary>
    public static class CeuBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_dimmed_cloud_bird", "enemy_forest_dimmed_light_fish", "enemy_forest_dark_stowaway", "enemy_forest_hold_shadow",
            "enemy_forest_living_rope", "enemy_forest_toy_archon", "enemy_forest_cracked_jar", "enemy_forest_chained_archon",
            "enemy_forest_balloon_giant", "enemy_forest_az_thief", "enemy_forest_dimmed_light_gull", "enemy_forest_sky_stone",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Cais da Coluna (91) --
            Page(entries, "enemy_forest_dimmed_cloud_bird",
                "The birds up here are made of cloud, and their whole job is to carry a little light from the piers to the boats. Most of them do it without thinking about it. This one set its light down somewhere, went back for it, forgot what it was going back for, and has been flying about the quay in a bad mood ever since. Ammo says they steal the ribbons off his caravan. Don't be cross with them. Hit them quick, they are fast and they bruise easily, and when one comes apart it goes back to being weather.",
                "Os pássaros daqui de cima são feitos de nuvem, e o trabalho deles é levar um pouquinho de luz dos píeres até as barcas. A maioria faz isso sem pensar. Este deixou a luz em algum lugar, voltou pra buscar, esqueceu o que tinha ido buscar, e anda voando pelo cais de mau humor desde então. O Ammo diz que eles roubam as fitas da caravana dele. Não fiquem bravos. Batam rápido, que eles são ligeiros e se machucam fácil, e quando um se desfaz volta a ser tempo.",
                ("Fast and fragile. Don't chase it, let it come.", "Rápida e frágil. Não corre atrás, deixa vir."),
                ("It forgot what it was carrying. I know that feeling.", "Ela esqueceu o que carregava. Conheço essa sensação."),
                ("A bird made of cloud pooped on me. That's just rain, right? Right?", "Um pássaro de nuvem fez cocô em mim. É só chuva, né? Né?"),
                ("Cloud holds light the way wool holds warmth: until you stop paying attention.", "Nuvem segura luz como lã segura calor: até a gente se distrair."),
                ("It's so grumpy! It's the softest grumpy ever!", "Ela é tão emburrada! É o emburrado mais fofinho do mundo!"));

            // -- Rio de Luz (92) --
            Page(entries, "enemy_forest_dimmed_light_fish",
                "This river isn't water, it is a road, and the fish are supposed to carry the small lights up it, a drop each. Some of them got tired and stopped. They kept swimming, because a fish doesn't know how to do anything else, but they swim in the air over the banks now, dull as an old coin, with one last drop hanging from a thread under the belly. They bite fast. If you can, knock the drop off and let it fall back in the river; it knows the way even if the fish doesn't.",
                "Este rio não é de água, é uma estrada, e os peixes deviam levar as luzinhas por ele, uma gota cada. Alguns cansaram e pararam. Continuaram nadando, porque peixe não sabe fazer outra coisa, mas agora nadam no ar por cima das margens, apagados como moeda velha, com uma última gota pendurada num fio embaixo da barriga. Mordem rápido. Se der, derrubem a gota e deixem cair no rio; ela sabe o caminho, mesmo que o peixe não saiba.",
                ("Aim for the thread. The fish slows down without it.", "Mira no fio. O peixe fica lento sem a gota."),
                ("It's still holding on to its last drop. That's the part that gets me.", "Ainda está segurando a última gota. É isso que me pega."),
                ("Fish in the air. Sure. Why not. It's that kind of day.", "Peixe no ar. Claro. Por que não. É esse tipo de dia."),
                ("Gold tarnishes from the outside in. So, apparently, do fish.", "Ouro escurece de fora pra dentro. Pelo visto, peixe também."),
                ("He has a little lamp on a string! I want a little lamp on a string!", "Ele tem uma lampadinha na cordinha! Eu quero uma lampadinha na cordinha!"));

            // -- Barca da Lua (93) --
            Page(entries, "enemy_forest_dark_stowaway",
                "Everything down below heard that the boats go up, and some of the dark thought: well, why not me? So it hid under a bench, or in a coil of rope, and came along. It runs low and fast across the deck and it has someone's boarding tag round its neck, which it thinks makes everything all right. It isn't wicked. It only wanted to see. But the Moon's boat is for the light, and it eats the light it sits next to, so it has to go back. Be firm. Then go and look under the benches again.",
                "Tudo lá embaixo ouviu dizer que as barcas sobem, e um pedaço da treva pensou: e por que não eu? Então se escondeu embaixo de um banco, ou num rolo de corda, e veio junto. Corre baixo e rápido pelo convés e leva no pescoço a etiqueta de embarque de alguém, achando que com isso está tudo certo. Não é malvado. Só queria ver. Mas a barca da Lua é da luz, e ele come a luz em que encosta, então tem que voltar. Sejam firmes. Depois olhem embaixo dos bancos de novo.",
                ("Check under the benches. There's always a second one.", "Olha embaixo dos bancos. Sempre tem um segundo."),
                ("It just wanted to see the top. I wish it could.", "Ele só queria ver o alto. Queria que pudesse."),
                ("Whose ticket is that? Hey. HEY. That's my ticket.", "De quem é essa etiqueta? Ei. EI. Essa etiqueta é minha."),
                ("Stowaways always believe the paperwork makes them passengers.", "Clandestino sempre acha que o papel faz dele passageiro."),
                ("He has a TICKET! He's a very polite shadow!", "Ele tem PASSAGEM! É uma sombra muito educada!"));

            Page(entries, "enemy_forest_hold_shadow",
                "The Moon's boat has a hold, and in the hold there is a shadow sitting on the luggage. It came aboard at the bottom of the world and it hasn't moved since. It is big, it is slow, it has crates stuck in it, and somewhere in the middle there is a ship's lantern it sat on by accident and hasn't noticed. The lantern is what you want. Hit where the light shows and the rest loses interest. Adda says it complains in its sleep about the service.",
                "A barca da Lua tem um porão, e no porão tem uma sombra sentada em cima da bagagem. Ela embarcou lá no fundo do mundo e nunca mais se mexeu. É grande, é lenta, tem caixote grudado nela, e lá no meio tem um lampião de navio em que sentou sem querer e ainda não notou. É o lampião que vocês querem. Batam onde a luz aparece, que o resto perde o interesse. O Adda diz que ela reclama do serviço dormindo.",
                ("Go for the lantern. Everything else is luggage.", "Vai no lampião. O resto é bagagem."),
                ("Someone is missing all those crates. Someone packed them carefully.", "Alguém está sentindo falta desses caixotes. Alguém arrumou com carinho."),
                ("It's sitting on the snacks. This is now personal.", "Ela está sentada em cima dos lanches. Agora é pessoal."),
                ("A shadow pressed on a lamp doesn't put it out. It only learns how to wear it.", "Sombra em cima de lamparina não apaga. Só aprende a vestir."),
                ("She's got eyes in the back of her... in the front of her... she has eyes!", "Ela tem olho nas costas... na frente... ela tem olho!"));

            Page(entries, "enemy_forest_living_rope",
                "Rope is honest work. It holds what you ask it to hold. But this rope spent a crossing or two tied to a shadow and it came away with the habit of grabbing things, and now it hangs about the rigging of both boats with a noose for a head. It won't hurt you much. It will hold you in place while something else does. So don't stand still near a coil, and if it gets you, don't pull: rope likes that. Cut the loose ends first, Vô Jorge always said, and the knot comes undone by itself.",
                "Corda é trabalho honesto. Segura o que a gente manda segurar. Mas esta passou uma ou duas travessias amarrada numa sombra e saiu com a mania de agarrar as coisas, e agora fica pelo cordame das duas barcas com um laço no lugar da cabeça. Não machuca muito. Segura vocês parados enquanto outra coisa machuca. Então não fiquem quietos perto de um rolo, e se ela pegar, não puxem: corda gosta disso. Cortem as pontas soltas primeiro, o Vô Jorge sempre dizia, e o nó se desfaz sozinho.",
                ("Ends first. The knot follows.", "Pontas primeiro. O nó vem depois."),
                ("It doesn't know how to let go. Poor thing.", "Ela não sabe soltar. Coitada."),
                ("Rope with a face. I've tied my boots with worse.", "Corda com cara. Já amarrei a bota com coisa pior."),
                ("Sailors' knots remember the load. This one still remembers the shadow.", "Nó de marinheiro lembra da carga. Este ainda lembra da sombra."),
                ("It wants to hold hands! It has no hands! It's trying so hard!", "Ela quer dar a mão! Não tem mão! Está tentando tanto!"));

            // -- Roda Celeste (94) --
            Page(entries, "enemy_forest_toy_archon",
                "The stories say twelve archons were tied to the Wheel, and that is where the signs in the sky came from. What the stories do not say is what happens after so very long going round and round. They wore down. There is one to a jar now, a ram or a bull or a crab about as tall as your knee, wooden and tin, with a wind-up key in the back and a crown that doesn't fit, and it is just as angry as it ever was. Do not laugh where it can see you. I laughed. It bit me on the shin.",
                "As histórias dizem que doze arcontes foram amarrados na Roda, e daí vieram os signos do céu. O que as histórias não dizem é o que acontece depois de tanto tempo girando. Eles gastaram. Agora tem um em cada jarra, um carneiro, um touro, um caranguejo da altura do joelho, de madeira e lata, com uma chave de dar corda nas costas e uma coroa que não serve, e bravo como sempre foi. Não riam onde ele possa ver. Eu ri. Levei uma mordida na canela.",
                ("Small and elite. Don't get careless about the size.", "Pequeno e forte. Não se engana com o tamanho."),
                ("Someone should wind him up gently. Once.", "Alguém devia dar corda nele com jeitinho. Uma vez."),
                ("I'm not fighting a toy. I'm fighting a toy. Mom can't know.", "Não vou lutar contra um brinquedo. Vou lutar contra um brinquedo. A mãe não pode saber."),
                ("Twelve signs, twelve jars. The cosmology is literal up here.", "Doze signos, doze jarras. Aqui a cosmologia é literal."),
                ("He has a key! Can I turn the key? Just one turn! PLEASE!", "Ele tem chave! Posso girar? Só uma voltinha! POR FAVOR!"));

            Page(entries, "enemy_forest_cracked_jar",
                "Twelve jars go round the Wheel and carry the light from the bottom of the sky to the top, and that is all a jar ought to have to think about. One or two of them got knocked, and a cracked jar doesn't pour, it leaks, and it leaks sideways, and it leaks at you. They float about the rim where the Wheel would have taken them, trying to finish the trip. Keep moving, and keep some distance from the crack. If you can get close, you'll find it is only a jar. Adda mends them, but he has to catch them first.",
                "Doze jarras giram na Roda e levam a luz do pé do céu até o alto, e isso é tudo que uma jarra devia ter que pensar. Uma ou duas levaram uma pancada, e jarra rachada não derrama, vaza, e vaza de lado, e vaza em cima de vocês. Ficam flutuando na borda por onde a Roda as levaria, tentando terminar a viagem. Fiquem andando, e longe da rachadura. Se chegarem perto, vão ver que é só uma jarra. O Adda conserta, mas primeiro tem que pegar.",
                ("The shot comes from the crack. Stay off that side.", "O tiro sai da rachadura. Sai desse lado."),
                ("It's still trying to get to the top with what it has left.", "Ainda está tentando chegar no alto com o que sobrou."),
                ("A jar shot me. I have been shot by a jar. Put that in the notebook.", "Uma jarra atirou em mim. Fui baleado por uma jarra. Bota isso no caderno."),
                ("Ceramic stress follows the crack. So does the light.", "A tensão da cerâmica segue a rachadura. A luz também."),
                ("It's leaking sparkles! It's the prettiest leak!", "Ela está vazando brilhinho! É o vazamento mais bonito!"));

            // -- Esferas dos Arcontes (95) --
            Page(entries, "enemy_forest_chained_archon",
                "You will have heard me say the archons swallowed too much light and would not give it back. Up here is where the Living Spirit put them: in the spheres, in chains, so they could not eat any more. This is one of the small ones, and it has had a very long time to get cross. It shoves. That is its whole plan, and it's a good one, because these spheres are high up. Keep your back to something solid, and watch the belly, where the light it never gave back still shows. It would like to be asked nicely. I have tried. It says no.",
                "Vocês já me ouviram dizer que os arcontes engoliram luz demais e não quiseram devolver. Aqui em cima é onde o Espírito Vivo guardou eles: nas esferas, de corrente, pra não comerem mais. Este é dos pequenos, e teve muito tempo pra ficar bravo. Empurra. É o plano inteiro dele, e é bom, porque essas esferas ficam alto. Fiquem de costas pra alguma coisa firme, e olhem a barriga, onde ainda aparece a luz que ele nunca devolveu. Ele gostaria que pedissem com educação. Eu pedi. Ele disse não.",
                ("It pushes. Don't fight with your back to the edge.", "Ele empurra. Não luta de costas pra beirada."),
                ("All that time, and he still won't give it back.", "Esse tempo todo, e ele ainda não devolve."),
                ("Nice chains. Did they come with the room?", "Bonitas correntes. Vieram com o quarto?"),
                ("Chained by the Living Spirit, in Theodore bar Konai's telling. They've been grumbling since.", "Acorrentado pelo Espírito Vivo, no relato de Teodoro bar Konai. Resmungando desde então."),
                ("He's so sulky! He has the biggest sulk in the sky!", "Ele está tão emburrado! É o maior emburro do céu!"));

            Page(entries, "enemy_forest_balloon_giant",
                "Some of them hold on so hard to what they swallowed that they swell. This one has gone quite round, with two thin legs that shouldn't be able to carry it and a very small, very offended head on top. It floats a little, and it would float off altogether if it weren't so heavy with spite. It takes its time getting to you and then it hits like a door. Don't stand underneath. When it lets go at last, it goes round the sky making a noise I will not describe, and Lavine will tell you all about it anyway.",
                "Alguns seguram com tanta força o que engoliram que incham. Este ficou redondinho, com duas pernas finas que não deviam aguentar e uma cabeça bem pequena e bem ofendida em cima. Flutua um pouco, e sairia voando de vez se não pesasse tanto de birra. Demora pra chegar em vocês e depois bate como porta. Não fiquem embaixo. Quando finalmente solta, sai dando volta no céu fazendo um barulho que eu não vou descrever, e a Lavine vai contar pra vocês de qualquer jeito.",
                ("Slow. Heavy. Stay out from under it.", "Lento. Pesado. Fica fora de baixo dele."),
                ("Imagine holding on that tight for that long.", "Imagina segurar com essa força por tanto tempo."),
                ("Someone is going to pop that and I am volunteering.", "Alguém vai estourar isso e eu me ofereço."),
                ("Pressure is only a thing refusing to be anywhere else.", "Pressão é só uma coisa se recusando a estar em outro lugar."),
                ("PFFFFFRRRRT! That's the noise! I told you! PFFFRT!", "PFFFFFRRRRT! É esse o barulho! Eu falei! PFFFRT!"));

            // -- Jardim de Luz (96) --
            Page(entries, "enemy_forest_az_thief",
                "Āz is Greed, and she has never in all her life had enough, and she wants this whole garden put away in one chest. She doesn't do her own picking. She has people for that. Hooded people, very quick with their hands, each with a sack of light on the back that is already full, still reaching. They aren't cruel about it; they step around the flowers, which is more than I can say for most. But the light they take isn't theirs. Grab the sack, and they have nothing left to run for. Mihrshah keeps a list.",
                "Āz é a Cobiça, e nunca na vida teve o bastante, e quer esse jardim inteiro guardado numa arca só. Ela não colhe por conta própria. Tem gente pra isso. Gente de capuz, muito rápida com as mãos, cada um com um saco de luz nas costas já cheio e ainda pegando mais. Não são cruéis; dão a volta nas flores, o que é mais do que eu posso dizer de muita gente. Mas a luz que levam não é deles. Peguem o saco, e eles não têm mais pra que correr. O Mihrshah guarda uma lista.",
                ("Go for the sack. Then they stop being quick.", "Vai no saco. Aí eles param de ser rápidos."),
                ("They walk around the flowers. Even they.", "Eles dão a volta nas flores. Até eles."),
                ("That sack is bigger than he is and he is still taking things.", "Esse saco é maior que ele e ele ainda está pegando coisa."),
                ("Āz, Greed, mother of the demons, in the old texts. Down here she does her shopping by proxy.", "Āz, a Cobiça, mãe dos demônios, nos textos antigos. Aqui ela faz compras por procuração."),
                ("His sack is glowing! Is it full of stars? Can I see? I'll give it back!", "O saco dele está brilhando! Está cheio de estrela? Posso ver? Eu devolvo!"));

            // -- Barca do Sol (97) --
            Page(entries, "enemy_forest_dimmed_light_gull",
                "Gulls follow boats. It is what gulls are for. These followed the Sun's boat for so long and so close that the light they had went clear out of them, and there's just one coal left, in the chest, that they cough up and spit at anyone who looks as if they might have some to spare. Look up. They shoot from a long way off and they don't miss the same place twice. Get close, and you'll find it's only a thin, tired bird. Bring something for it, afterwards, if you can. Ammo says they like bread.",
                "Gaivota segue barco. É pra isso que serve gaivota. Estas seguiram a barca do Sol por tanto tempo e tão de perto que a luz que tinham saiu toda, e só restou uma brasinha no peito, que elas tossem e cospem em quem parece ter um pouco sobrando. Olhem pra cima. Atiram de muito longe e não erram duas vezes no mesmo lugar. Cheguem perto, e vão ver que é só um passarinho magro e cansado. Depois tragam alguma coisa pra ela, se der. O Ammo diz que gostam de pão.",
                ("It shoots from up there. Close the gap fast.", "Ela atira lá de cima. Encurta a distância rápido."),
                ("It gave away everything and it's still following the boat.", "Deu tudo que tinha e ainda segue a barca."),
                ("I have fought eleven gulls today and this one has the nerve.", "Já lutei com onze gaivotas hoje e esta ainda tem coragem."),
                ("Burnt out from proximity. There's a sermon in that somewhere.", "Queimada de tanta proximidade. Tem um sermão nisso em algum lugar."),
                ("The Moon boat is a shoe and the Sun boat has a mean bird! I keep telling you!", "A barca da Lua é um sapato e a barca do Sol tem um passarinho bravo! Eu vivo dizendo!"));

            // -- Mirante do que Segura o Céu (99) --
            Page(entries, "enemy_forest_sky_stone",
                "The sky has a ceiling, and the ceiling has cracks, and now and then a piece comes down. It lands with a lot of noise and sits for a while, and then, slowly, it grows four stumps and a mouth and begins walking in the direction it came from, because it would very much like to go home. It doesn't care who is in between. It is the slowest thing up here and it hits the hardest, so don't be between. Gold in the cracks means it is still alive in there. It is only trying to get back up. So are we all.",
                "O céu tem teto, e o teto tem rachadura, e de vez em quando um pedaço cai. Cai com muito barulho e fica quieto um tempo, e depois, devagar, cria quatro tocos e uma boca e sai andando pra onde veio, porque queria muito voltar pra casa. Não liga pra quem está no caminho. É a coisa mais lenta daqui de cima e a que bate mais forte, então não fiquem no caminho. Ouro na rachadura quer dizer que ainda tem vida lá dentro. Só está tentando subir de volta. Todos nós estamos.",
                ("Slowest thing on the Lookout. Don't stand in its line.", "A coisa mais lenta do Mirante. Não fica na linha dela."),
                ("It just wants to go home. I understand it completely.", "Só quer voltar pra casa. Entendo perfeitamente."),
                ("A piece of ceiling tried to bite me. Grandma, what is this house?", "Um pedaço de teto tentou me morder. Vó, que casa é essa?"),
                ("Gold fills the seams where the sky has already mended itself once.", "O ouro preenche as emendas onde o céu já se consertou uma vez."),
                ("It's a walking rock with a tiny tail! I named it Tuesday!", "É uma pedra que anda com um rabinho! Dei o nome de Terça-Feira!"));
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
