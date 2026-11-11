using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of Regions 12–16 that the overworld still played with stand-ins (bible §3.2.1, eleventh pass):
    /// lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="CeuBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends
    /// these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3); Apep's children are only snakes, the scorpion-man only as the
    /// old tale gives him, and no god of Kemet or Babylon appears.
    /// </summary>
    public static class LateStandInBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_devouring_salamander", "enemy_mountain_volcanic_bomb", "enemy_forest_thief_goose", "enemy_forest_child_of_apep",
            "enemy_forest_quicksand", "enemy_forest_scorpion_man", "enemy_forest_cedar_lynx", "enemy_forest_resin_bee",
            "enemy_forest_blade_bat", "enemy_forest_great_eel", "enemy_forest_ash_bull", "enemy_forest_smoke_rope",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Caldeira do Fogo Devorador (70) --
            Page(entries, "enemy_mountain_devouring_salamander",
                "Most salamanders are happy with a warm stone. This kind ate the fire off the caldera's edge one mouthful at a time and found out that the more it swallows, the hungrier it gets. You can see the last mouthful still going down the stripe on its back. Don't stand near the embers when it comes, it will take those first and you second. Be quick, and don't feed it.",
                "A maioria das salamandras se contenta com uma pedra quentinha. Esta comeu o fogo da borda da caldeira, uma bocada de cada vez, e descobriu que quanto mais engole, mais fome dá. Dá pra ver a última bocada ainda descendo na listra das costas. Não fiquem perto das brasas quando ela vier, ela pega as brasas primeiro e vocês depois. Sejam rápidos, e não deem de comer.",
                ("Low and quick. Watch its feet, not its mouth.", "Baixa e rápida. Olha os pés, não a boca."),
                ("It is always hungry. I do feel sorry about that part.", "Ela está sempre com fome. Dessa parte eu tenho pena."),
                ("It ate a whole campfire! MY campfire!", "Ela comeu uma fogueira inteira! A MINHA fogueira!"),
                ("Fire does not fill anything. That is rather the point of fire.", "Fogo não enche nada. Esse é justamente o problema do fogo."),
                ("Its back is glowing! Can I have one? I'd feed it!", "As costas dela brilham! Posso ficar com uma? Eu dava comida!"));

            // -- Monte Echeyde (67) --
            Page(entries, "enemy_mountain_volcanic_bomb",
                "Echeyde throws stones up when it is in a temper, and most of them come down again like sensible stones. Some never did. They hang about the slopes, cracked, trailing smoke, and when they notice you they spit what is left in them. Keep moving and look up. If one comes apart, stay away till it has finished. And don't let Joaquim tell you he can catch them.",
                "O Echeyde joga pedra pro alto quando está de mau humor, e a maioria desce de volta como pedra ajuizada. Algumas nunca desceram. Ficam penduradas pelas encostas, rachadas, soltando fumaça, e quando reparam em vocês cospem o que ainda têm dentro. Continuem andando e olhem pra cima. Se uma se desfizer, fiquem longe até acabar. E não deixem o Joaquim dizer que consegue pegar.",
                ("It spits from far. Close the distance or get out of the line.", "Cospe de longe. Chega perto ou sai da linha."),
                ("It's just a stone that never got to land. Poor thing.", "É só uma pedra que nunca conseguiu pousar. Coitada."),
                ("I can catch it. I'm telling you. I just need better gloves.", "Eu consigo pegar. Tô dizendo. Só preciso de luvas melhores."),
                ("It should fall. It doesn't. Somebody ought to write that down.", "Devia cair. Não cai. Alguém devia anotar isso."),
                ("It's a floating coal! Don't touch it, Joaquim!", "É um carvão voador! Não encosta, Joaquim!"));

            // -- Campos de Juncos (72) --
            Page(entries, "enemy_forest_thief_goose",
                "Now, I have raised geese, so listen to me. A goose is a bad-tempered animal that thinks everything is its own. In the Reed Fields there is nobody to tell it otherwise, and the shadow only had to give it a small push. These take bread, rings, ribbons and the laundry, and they bite on the way out. Do not run; a goose knows when you run. Stand up tall, and give it a good smack.",
                "Olhem, eu já criei ganso, então me escutem. Ganso é bicho de mau gênio que acha que tudo é dele. Nos Campos de Juncos não tem ninguém pra dizer o contrário, e a sombra só precisou dar um empurrãozinho. Esses levam pão, anel, fita e roupa do varal, e ainda mordem na saída. Não corram; ganso sabe quando a gente corre. Fiquem bem retos e deem uma boa palmada.",
                ("Don't run. Don't ask me why, just don't.", "Não corre. Não pergunta por quê, só não corre."),
                ("It has a ring on. That was somebody's wedding.", "Está com um anel. Isso foi o casamento de alguém."),
                ("It bit me. Then it took my sandwich. In that order.", "Me mordeu. Depois pegou meu sanduíche. Nessa ordem."),
                ("A goose is proof that the world was not made to be kind.", "Ganso é a prova de que o mundo não foi feito pra ser gentil."),
                ("Hello goose! Ow! OW! Goose, no!", "Oi, ganso! Ai! AI! Ganso, não!"));

            // -- Banco de Areia de Apep (76) --
            Page(entries, "enemy_forest_child_of_apep",
                "The big one drinks the river, and I won't tell you about her here. These are only snakes that were lying on her when she went by, and came away thinking they were her too. They rear up exactly the way she does and they are about as tall as your knee. Don't laugh, they are very fast, and they bite. But don't be afraid of them either. A snake that thinks it's a river is mostly just confused.",
                "A grandona bebe o rio, e dela eu não vou falar aqui. Estas são só cobras que estavam deitadas em cima dela quando ela passou, e saíram achando que também eram ela. Levantam a cabeça igualzinho a ela e têm mais ou menos a altura do joelho de vocês. Não riam, são muito rápidas e mordem. Mas também não tenham medo. Cobra que acha que é rio está, no fundo, só confusa.",
                ("Small, fast. Strike when the head goes back.", "Pequena e rápida. Bate quando a cabeça recuar."),
                ("It thinks it's her. I wonder if anyone has told it.", "Acha que é ela. Será que alguém já contou?"),
                ("You're not scary. You're knee height. Ow. OK, a bit scary.", "Você não dá medo. Tem altura de joelho. Ai. Tá, um pouco."),
                ("Imitation is how small things survive near very large ones.", "Imitar é como as coisas pequenas sobrevivem perto das muito grandes."),
                ("Baby snakes! Why are the baby snakes angry!", "Cobrinhas filhotes! Por que as cobrinhas estão bravas!"));

            Page(entries, "enemy_forest_quicksand",
                "Sand that has been lied to. It was a good firm sandbank until the shadow told it that it could have whatever stands on it, and so it took to walking, in its way, a little at a time. It doesn't chase you. It simply is where you were going to step. There is usually something shiny sunk in it, a lamp or a ring. Do not reach for it, whatever it is, and don't stand still.",
                "Areia que levou uma mentira. Era um bom banco de areia firme até a sombra dizer que podia ficar com tudo o que pisasse nela, e aí começou a andar, do jeito dela, de pouquinho em pouquinho. Não corre atrás de vocês. Simplesmente está onde vocês iam pisar. Quase sempre tem alguma coisa brilhando afundada nela, uma lâmpada, um anel. Não estiquem a mão, seja o que for, e não fiquem parados.",
                ("Keep moving. It's the standing still that gets you.", "Continua andando. O que pega é ficar parado."),
                ("There's a lamp in there. Somebody is still looking for that lamp.", "Tem uma lâmpada ali dentro. Alguém ainda procura essa lâmpada."),
                ("I stepped in it. My boot is still in it. Keep it, sand.", "Pisei nela. Minha bota ainda tá lá. Pode ficar, areia."),
                ("All ground is patient. This ground has merely got ideas.", "Todo chão é paciente. Este só arrumou ideias."),
                ("It's a sandcastle that's alive! Don't step on it, it'll step on you!", "É um castelo de areia vivo! Não pisa nele, ele pisa em você!"));

            // -- Dur-Jarras (77) --
            Page(entries, "enemy_forest_scorpion_man",
                "In the old tale from between the rivers there are men with the tail of a scorpion who keep the gate of the mountains. That is all I'll say about them, because it's not my tale. These stood the same watch at the brick gate of Dur-Jarras, and when the shadow came they kept on watching for so long they forgot what the gate was for. They shoot anything that walks up. The ground no longer knows them, so they go about a hand above it. Stay out of the line and go in close. They aren't wicked. They have just been on duty for a very long time.",
                "No conto antigo de entre os rios tem homens com rabo de escorpião que guardam o portão das montanhas. Só isso eu vou dizer, porque o conto não é meu. Estes fizeram a mesma guarda no portão de tijolo de Dur-Jarras, e quando a sombra veio continuaram de guarda por tanto tempo que esqueceram pra que servia o portão. Atiram em tudo o que chega. O chão já não conhece eles, por isso andam um palmo acima dele. Fiquem fora da linha e cheguem perto. Não são maus. Só estão de serviço faz tempo demais.",
                ("Don't stand in the road. Come in from the side.", "Não fica na estrada. Chega pela lateral."),
                ("He's still guarding. He doesn't remember what.", "Ainda está de guarda. Não lembra do quê."),
                ("Half man, half scorpion, all rude.", "Metade homem, metade escorpião, inteiro mal-educado."),
                ("A gate without a purpose is a wall with an opinion.", "Portão sem motivo é parede com opinião."),
                ("He's floating! Why is he floating? Is it the tail? Is it the tail!", "Ele está flutuando! Por que ele flutua? É o rabo? É o rabo!"));

            // -- Cedral de Hobabish (80) --
            Page(entries, "enemy_forest_cedar_lynx",
                "A lynx is a shy creature and a very clean one, and that's how you know something has gone wrong with these: there's resin all through their coats. They rubbed up against the angry cedars until the trees took them for one of their own, and now they hunt for the cedars, or think they do. They come fast and quiet at the edge of the wood. Watch the tufts on the ears, they go flat just before it jumps.",
                "Lince é bicho tímido e muito limpo, e é assim que se sabe que alguma coisa deu errado com estes: estão com resina no pelo todo. Se esfregaram nos cedros raivosos até as árvores acharem que eram parentes, e agora caçam pelos cedros, ou acham que caçam. Vêm rápidos e calados na beira da mata. Olhem as pontinhas das orelhas, elas abaixam logo antes do pulo.",
                ("Ears flat means jump. Remember it.", "Orelha baixa é pulo. Grava isso."),
                ("All that sticky fur. It must itch so much.", "Esse pelo todo grudado. Deve coçar tanto."),
                ("That's the most stuck-up cat I've ever met. Get it? Stuck?", "É o gato mais grudento que eu já vi. Entendeu? Grudento?"),
                ("Belonging to the trees costs you your own coat, it seems.", "Pertencer às árvores custa o próprio pelo, pelo visto."),
                ("Fluffy ears! Sticky fluffy ears! I'm not allowed to pet it, am I?", "Orelhas peludas! Orelhas peludas grudentas! Não posso fazer carinho, né?"));

            Page(entries, "enemy_forest_resin_bee",
                "A bee is the best thing in a forest, and I won't hear a word against them. But when the cedars started to bleed resin, these went and carried it home instead of honey, and now the abdomen of each one is a lit drop of it, far too heavy, and they are very cross about the whole business. They come in a crowd, they stick to you, and the sting is the least of it. Move out of the swarm. Don't swat. They'll only stick to your hand.",
                "Abelha é a melhor coisa que tem numa mata, e não aceito uma palavra contra. Mas quando os cedros começaram a sangrar resina, estas foram carregar isso pra casa em vez de mel, e agora a barriga de cada uma é uma gota acesa disso, pesada demais, e estão muito irritadas com a história toda. Vêm em bando, grudam em vocês, e a ferroada é o de menos. Saiam do enxame. Não deem tapa. Só vai grudar na mão.",
                ("Get out of the swarm first. Then fight.", "Sai do enxame primeiro. Depois briga."),
                ("They're carrying something too heavy and they won't put it down.", "Estão carregando uma coisa pesada demais e não largam."),
                ("I hit one and now it's my friend forever. It's on my hand. Forever.", "Bati numa e agora é minha amiga pra sempre. Tá na minha mão. Pra sempre."),
                ("Sweetness is only a matter of which tree you visit.", "Doçura é só uma questão de qual árvore se visita."),
                ("Bees! Glowing bees! Can we keep the glowing bees!", "Abelhas! Abelhas brilhantes! Podemos ficar com as abelhas brilhantes!"));

            // -- Cova do Vento Destruidor (86) --
            Page(entries, "enemy_forest_blade_bat",
                "They nest in the shaft of the Destroying Wind, and that wind wears everything down to an edge. It took the fur off these first, and then the softness, and what was left is a bat with blades for fingers. They come in a crowd and they don't turn. Stay against the wall so they can only reach you from one side. And wear something you don't mind getting cut; your grandfather learned that the hard way.",
                "Fazem ninho no poço do Vento Destruidor, e aquele vento gasta tudo até virar fio. Tirou primeiro o pelo destes, depois a moleza, e sobrou um morcego com lâmina no lugar dos dedos. Vêm em bando e não fazem curva. Fiquem encostados na parede pra só chegarem de um lado. E usem uma roupa que possa rasgar; o avô de vocês aprendeu isso do jeito difícil.",
                ("Back to the wall. One side only.", "Costas na parede. Um lado só."),
                ("The wind took everything but the sharp part of them.", "O vento levou tudo menos a parte afiada deles."),
                ("Ow. My hat. My good hat.", "Ai. Meu chapéu. Meu chapéu bom."),
                ("A blade is merely what remains of a thing that kept being sharpened.", "Lâmina é só o que resta de algo que não parou de ser afiado."),
                ("They're so shiny and so mean!", "Eles são tão brilhantes e tão malvados!"));

            // -- Cova do Lodo (87) --
            Page(entries, "enemy_forest_great_eel",
                "The water in those halls goes up and down by the Artificers' sluices, and it has been a very long while since anybody worked them. This old eel went on lying where the water had been and has not once checked whether the water is still there. It is as thick as your waist, it coils round whatever passes, and it does not let go readily. Knock it loose before it gets a good grip, and go round it if you can. It hasn't noticed.",
                "A água daqueles salões sobe e desce pelas comportas dos Artífices, e faz muito tempo que ninguém mexe nelas. Esta enguia velha continuou deitada onde a água estava e nunca foi conferir se a água ainda está lá. É grossa como a cintura de vocês, se enrola no que passa e não solta fácil. Derrubem antes que ela firme o aperto, e se der, passem pelo lado. Ela nem reparou.",
                ("Don't let it wrap you. Hit it early.", "Não deixa enrolar. Bate logo."),
                ("Nobody told it the water left.", "Ninguém contou pra ela que a água foi embora."),
                ("It's swimming. In nothing. With a very serious face.", "Tá nadando. No nada. Com uma cara muito séria."),
                ("It is a poor argument for trusting your habits.", "É um argumento fraco a favor de confiar nos próprios hábitos."),
                ("She has whiskers! Hello whiskers!", "Ela tem bigode! Oi, bigode!"));

            // -- Cova do Fogo Devorador (85) --
            Page(entries, "enemy_forest_ash_bull",
                "This one I'll tell you plainly, because I've seen what's left of a barn. It was a bull, and it stood too close to the Devouring Fire, and it burned all the way through without noticing, so now it is a bull-shaped heap of ash with embers in the cracks and all of its temper still inside. When the head goes down it's coming. It doesn't turn well. Step aside and let it go by, and it will run into something, and it won't mind.",
                "Desse eu falo sem rodeio, porque já vi o que sobra de um celeiro. Era um touro, ficou perto demais do Fogo Devorador e queimou de ponta a ponta sem perceber, e agora é um monte de cinza em forma de touro, com brasa nas rachaduras e o mau gênio inteiro lá dentro. Quando a cabeça abaixar, ele vem. Faz curva mal. Saiam do caminho e deixem passar, ele vai bater em alguma coisa e nem vai ligar.",
                ("Head down, you sidestep. That's all there is.", "Cabeça baixou, desvia. É só isso."),
                ("It burned away and never found out. Please don't tell it.", "Queimou inteiro e nunca soube. Por favor, não contem."),
                ("Sir, you are on fire. Sir. Sir, you are on fire.", "Moço, o senhor tá pegando fogo. Moço. Moço, o senhor tá pegando fogo."),
                ("The temper, one observes, is the last thing to burn.", "O mau gênio, nota-se, é a última coisa a queimar."),
                ("It's so warm! And it's coming this way! And it's so warm!", "Ele é tão quentinho! E tá vindo pra cá! E é tão quentinho!"));

            // -- Barca do Sol (97) --
            Page(entries, "enemy_forest_smoke_rope",
                "The ropes on the Sun's boat get very hot, as you can imagine, and from time to time one of them burns away. The smoke of it doesn't always hear about that. It stays up on the sail deck in the shape of a coil, and goes on tying and tying, at anything, at you. You can't cut it, there's nothing to cut. Wave it apart with a good hard swing, and when it comes loose, let it go; it only wanted something to hold.",
                "As cordas da barca do Sol ficam muito quentes, vocês imaginam, e de vez em quando uma delas queima inteira. Nem sempre a fumaça fica sabendo. Continua no convés da vela em forma de rolo, e segue amarrando e amarrando, qualquer coisa, vocês. Não dá pra cortar, não tem o que cortar. Desfaçam com um golpe bem forte, e quando soltar, deixem ir; só queria alguma coisa pra segurar.",
                ("Don't try to cut it. Break it up.", "Não tenta cortar. Desmancha."),
                ("It only wanted something to hold on to.", "Só queria alguma coisa pra segurar."),
                ("I got tied up by smoke. Don't tell Lavine.", "Fui amarrado por fumaça. Não conta pra Lavine."),
                ("Half a rope's duty is remembering what it was tied to.", "Metade do trabalho de uma corda é lembrar ao que estava amarrada."),
                ("Joaquim got tied up by smoke! I'm telling everybody!", "O Joaquim foi amarrado por fumaça! Vou contar pra todo mundo!"));
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
