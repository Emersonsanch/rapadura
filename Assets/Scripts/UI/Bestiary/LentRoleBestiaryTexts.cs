using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of the Estepe dos Nartes (Região 9, Docs/Mapas100.md 47–52) that the overworld still played
    /// with another region's beast after the sixteenth pass (bible §3.2.1, seventeenth pass): lore in Grandma Edna's voice and one line per
    /// cousin, en + pt-BR. Kept apart like <see cref="BorrowedRoleBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class LentRoleBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_dimmed_golden_eagle", "enemy_desert_golden_stag", "enemy_desert_giant_ibex", "enemy_forest_hill_boar",
            "enemy_cave_smoke_messenger", "enemy_mountain_smoke_woodpecker", "enemy_cave_dimmed_fungus", "enemy_forest_taiga_lynx",
            "enemy_forest_dimmed_stallion", "enemy_mountain_iron_eagle", "enemy_mountain_caucasus_bear", "enemy_forest_snow_wolf",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Acampamento de Soslan, Estepe dos Ventos (47) --
            Page(entries, "enemy_mountain_dimmed_golden_eagle",
                "The riders of Soslan's camp hunt with golden eagles on the fist, and an eagle always came back to the fist it left. The shadow came in on the wind over the steppe and settled in the gold of their napes like ash. Now they stoop on the riders instead of the hares, and throw their hard feathers from high up. Keep moving under them, and hit them when they come down to land.",
                "Os cavaleiros do acampamento do Soslan caçam com águias-douradas no punho, e a águia sempre voltava pro punho de onde saiu. A sombra chegou no vento da estepe e assentou no dourado da nuca delas que nem cinza. Agora elas mergulham nos cavaleiros em vez das lebres, e jogam as penas duras lá de cima. Fiquem andando embaixo delas, e batam quando descerem pra pousar.",
                ("They throw from up high. Close in when they land.", "Elas jogam lá de cima. Chega perto quando pousarem."),
                ("Somewhere a rider is still holding out his fist for her.", "Em algum lugar um cavaleiro ainda tá esticando o punho pra ela."),
                ("A feather hit me in the ear. A FEATHER. It hurt.", "Uma pena acertou minha orelha. Uma PENA. Doeu."),
                ("The gold on its neck went grey. The shadow always starts with what shines.", "O dourado da nuca ficou cinza. A sombra sempre começa pelo que brilha."),
                ("She has fluffy trousers! Eagles wear trousers!", "Ela tem calça de pena! Águia usa calça!"));

            // -- Kurgans de Ouro (48) --
            Page(entries, "enemy_desert_golden_stag",
                "In the golden kurgans the old horsemen were buried with stags of gold, legs folded under, antlers rolled into spirals, the way they drew every animal. The shadow got into the graves, and some of the stags stood up and ran out. They run the kurgans now like real deer, cold to the touch and dull where the soot is. They are quick. Let them charge past and hit them on the turn.",
                "Nos kurgans de ouro os cavaleiros antigos foram enterrados com cervos de ouro, pernas dobradas, galhada enrolada em espiral, do jeito que eles desenhavam todo bicho. A sombra entrou nas covas, e alguns cervos levantaram e saíram correndo. Agora correm pelos kurgans que nem cervo de verdade, frios na mão e foscos onde tem fuligem. São rápidos. Deixem passar no tranco e batam na virada.",
                ("Fast on the charge, slow on the turn. Hit the turn.", "Rápido na investida, lento na virada. Bate na virada."),
                ("It was made to lie still and keep someone company. Let's let it rest again.", "Ele foi feito pra ficar quietinho fazendo companhia pra alguém. Vamos deixar ele descansar de novo."),
                ("Solid gold and I can't keep any of it. Worst treasure ever.", "Ouro maciço e eu não posso ficar com nada. Pior tesouro do mundo."),
                ("Gold does not want anything. Whatever is making it run, it isn't the gold.", "Ouro não quer nada. O que faz ele correr não é o ouro."),
                ("Its horns are curly like my hair in the morning!", "Os chifres dele são enroladinhos que nem meu cabelo de manhã!"));

            // -- Pastos dos Gigantes (49) --
            Page(entries, "enemy_desert_giant_ibex",
                "The hill giants keep ibexes as big as their own round houses and milk them into stone pots. A giant's goat is a gentle thing with a giant, because everything is small next to it. The shadow got into the pens one night, and the goats came out butting at anything smaller than a giant, which is all of us. Don't stand in front of the horns.",
                "Os gigantes da colina criam cabras-montesas do tamanho das casas redondas deles e tiram o leite em panelas de pedra. Cabra de gigante é mansa com gigante, porque tudo fica pequeno do lado dele. A sombra entrou nos cercados uma noite, e as cabras saíram dando chifrada em tudo que é menor que gigante, que é todo mundo nós. Não fiquem na frente dos chifres.",
                ("When the head goes down, step aside.", "Quando a cabeça abaixar, sai de lado."),
                ("Baraqel will want her back in the pen. Gently.", "O Baraqel vai querer ela de volta no cercado. Com jeito."),
                ("That's not a goat. That's a goat that ate a house.", "Isso não é cabra. É uma cabra que comeu uma casa."),
                ("Every knot on those horns is a year. She was old before the shadow came.", "Cada nó desses chifres é um ano. Ela já era velha antes da sombra chegar."),
                ("Her beard is longer than Grandpa Jorge's!", "A barba dela é maior que a do Vô Jorge!"));

            // -- Pastos dos Gigantes e Fortim (49) --
            Page(entries, "enemy_forest_hill_boar",
                "The boars of the hills root under the giants' oaks, and the giants always left the acorns to them; a giant does not need acorns. Since the shadow the boars come down to the Fortim at night and eat the pantry, pots and all, and they don't leave when you ask. They charge low and hard. Jump aside, and hit them from the flank while they turn round.",
                "Os javalis da colina fuçam debaixo dos carvalhos dos gigantes, e os gigantes sempre deixaram as bolotas pra eles; gigante não precisa de bolota. Desde a sombra os javalis descem pro Fortim de noite e comem a despensa, panela e tudo, e não vão embora quando a gente pede. Atacam baixo e com força. Pulem de lado, e batam no flanco enquanto ele vira.",
                ("Low and hard. Sidestep, then the flank.", "Baixo e com força. Desvia, depois o flanco."),
                ("They're just hungry. Hungry isn't wicked.", "Eles só tão com fome. Fome não é maldade."),
                ("It ate a POT. A stone pot. I saw it.", "Ele comeu uma PANELA. De pedra. Eu vi."),
                ("The giants gave them the acorns for nothing. The shadow taught them to take the rest.", "Os gigantes davam as bolotas de graça. A sombra ensinou eles a pegar o resto."),
                ("Its nose is a little round plate! Boop!", "O nariz dele é um pratinho redondo! Bup!"));

            // -- Fortim dos Gigantes, Vale das Tábuas, Torres Svan (49, 52) --
            Page(entries, "enemy_cave_smoke_messenger",
                "In the Book of the Giants two of the giants carry their dreams far away to be read, and come back with the reading. The crows of the Fortim did the same with the small dreams, a tablet tied to the leg. The shadow made them smoke, and now they carry the bad dreams back and throw them at you as hard feathers. Get under them, and they have nothing to throw.",
                "No Livro dos Gigantes dois gigantes levam os sonhos deles pra longe pra serem lidos, e voltam com a leitura. Os corvos do Fortim faziam o mesmo com os sonhos pequenos, uma tabuinha amarrada na perna. A sombra fez deles fumaça, e agora eles trazem os sonhos ruins de volta e jogam em vocês feito pena dura. Fiquem embaixo deles, e eles não têm o que jogar.",
                ("They throw from afar. Get under them.", "Jogam de longe. Fica embaixo deles."),
                ("The tablet on its leg still has a dream on it. I hope it's a good one.", "A tabuinha na perna ainda tem um sonho escrito. Tomara que seja bom."),
                ("A crow made of smoke. You hit it and it just goes 'puff'.", "Um corvo de fumaça. Você bate e ele faz 'puf'."),
                ("A messenger who forgot the message keeps flying anyway. That's the sad part.", "Mensageiro que esqueceu o recado continua voando mesmo assim. Essa é a parte triste."),
                ("Can I read the tablet? Please? Just one dream?", "Posso ler a tabuinha? Por favor? Só um sonho?"));

            // -- Arvoredo de Haripushpa (50) --
            Page(entries, "enemy_mountain_smoke_woodpecker",
                "The woodpeckers of the taiga take the beetles out of the trunk folk's bark, and the trunk folk sleep better for it. Since the shadow, when they drum the bark starts to smoke, and the smoke stays in their feathers. Now they drum on travellers instead, from the branches, and throw splinters. They are small and quick. Watch the red crest, it bobs before they throw.",
                "Os pica-paus da taiga tiram os besouros da casca do povo-tronco, e o povo-tronco dorme melhor por isso. Desde a sombra, quando eles batucam a casca começa a soltar fumaça, e a fumaça fica nas penas. Agora batucam nos viajantes, lá dos galhos, e jogam lascas. São pequenos e ligeiros. Olhem o topete vermelho, ele balança antes de jogar.",
                ("The red crest bobs before the throw.", "O topete vermelho balança antes de jogar."),
                ("Haripushpa misses them. They were her doctors.", "A Haripushpa sente falta deles. Eram os médicos dela."),
                ("Tok tok tok on my HEAD. I'm not a tree.", "Toc toc toc na minha CABEÇA. Eu não sou árvore."),
                ("A good habit in the wrong place is a bad habit. That's all the shadow needs.", "Hábito bom no lugar errado vira hábito ruim. É só disso que a sombra precisa."),
                ("It has a red hat! I want a red hat!", "Ele tem um chapéu vermelho! Eu quero um chapéu vermelho!"));

            // -- Arvoredo de Haripushpa e Taiga de Lariços (50) --
            Page(entries, "enemy_cave_dimmed_fungus",
                "Under the larches of Haripushpa the fungus eats the dead wood and gives it back as earth, slowly, and nobody minds. The shadow made it hungry for the living trees, and it walks out of the moss on its stump to settle on the sleeping trunk folk. When it squeezes, a cloud of spores comes out that makes your legs heavy. Hit it from the side and step out of the cloud.",
                "Debaixo dos lariços da Haripushpa o fungo come a madeira morta e devolve em terra, devagarinho, e ninguém se importa. A sombra deu nele fome de árvore viva, e ele sai do musgo no toco dele pra pousar no povo-tronco que dorme. Quando ele aperta, sai uma nuvem de esporo que deixa as pernas pesadas. Batam de lado e saiam da nuvem.",
                ("When it squeezes, step out of the spores.", "Quando ele apertar, sai dos esporos."),
                ("It used to turn old wood into soil. That's a good job.", "Ele transformava madeira velha em terra. É um trabalho bom."),
                ("It smells like Grandma's cellar. No offence, Grandma.", "Tem cheiro de porão da Vó. Sem ofensa, Vó."),
                ("Slow things are the hardest to notice. It was on the trees for weeks before anyone saw.", "Coisa lenta é a mais difícil de perceber. Ele ficou semanas nas árvores antes de alguém ver."),
                ("It's glowing underneath like a little lamp!", "Ele brilha embaixo que nem uma lampadinha!"));

            // -- Taiga de Lariços e Lago de Degelo (50) --
            Page(entries, "enemy_forest_taiga_lynx",
                "The lynxes of the taiga are as old as the larches and as quiet. The shadow did not make them louder; it made them hungry. They lie along the bark trails and wait for the trunk folk to pass, and they jump from very close. If you see two black tufts in the grass, stop, and let it come to you.",
                "Os linces da taiga são velhos que nem os lariços e quietos que nem eles. A sombra não deixou eles mais barulhentos; deixou com fome. Ficam deitados na beira das trilhas de casca esperando o povo-tronco passar, e pulam de muito perto. Se virem dois tufinhos pretos no capim, parem, e deixem ele vir até vocês.",
                ("It jumps from close. Let it come, then hit.", "Pula de perto. Deixa vir, depois bate."),
                ("It's only quiet. Quiet is not the same as mean.", "Ele só é quieto. Quieto não é a mesma coisa que malvado."),
                ("I didn't hear it. Not once. That's the worst part.", "Eu não ouvi ele. Nem uma vez. Essa é a pior parte."),
                ("The best hunters wait. The shadow just changed what it waits for.", "Os melhores caçadores esperam. A sombra só mudou o que ele espera."),
                ("It has paintbrushes on its ears!", "Ele tem pincel nas orelhas!"));

            // -- Pradaria dos Mil Cavalos (51) --
            Page(entries, "enemy_forest_dimmed_stallion",
                "On the Thousand-Horse Prairie the herds run free and come to the Narts' cup to drink. The shadow went into the stallions first, the ones that lead, and their manes turned to smoke. They lead the herd away from the well now, or straight at whoever stands near it. Don't run from a horse in the open. Stand your ground and hit it as it comes.",
                "Na Pradaria dos Mil Cavalos os rebanhos correm soltos e vêm beber na taça dos Nartes. A sombra entrou primeiro nos garanhões, os que guiam, e a crina deles virou fumaça. Agora levam o rebanho pra longe do poço, ou direto em cima de quem estiver perto dele. Não corram de cavalo no campo aberto. Fiquem firmes e batam quando ele vier.",
                ("You can't outrun it. Stand and hit.", "Não dá pra correr mais que ele. Fica firme e bate."),
                ("The herd still follows him. If he comes back, they all come back.", "O rebanho ainda segue ele. Se ele voltar, todos voltam."),
                ("His tail is smoke. I grabbed it. I'm holding nothing.", "O rabo dele é fumaça. Eu peguei. Tô segurando nada."),
                ("Whoever leads carries the herd with him, either way. That's why the shadow went for him first.", "Quem guia leva o rebanho junto, pra um lado ou pro outro. Por isso a sombra foi nele primeiro."),
                ("When he's better can I ride him? Just a little?", "Quando ele melhorar eu posso andar nele? Só um pouquinho?"));

            // -- Torre de Vigia, Corrente de Amirani, Cume de Ohya (52) --
            Page(entries, "enemy_mountain_iron_eagle",
                "The tale of the chained hero says a bird came to the mountain every day. Whatever bird that was, the eagles of these crags nest on the old chain and line their nests with its rust, and the shadow turned the rust into feathers. They throw them like knives from the wind. The plates are hard; hit where the plates part, under the wings.",
                "O conto do herói acorrentado diz que um pássaro vinha à montanha todo dia. Seja qual for esse pássaro, as águias destes penhascos fazem ninho na corrente velha e forram com a ferrugem dela, e a sombra transformou a ferrugem em pena. Elas jogam as penas que nem faca, de dentro do vento. As placas são duras; batam onde elas se abrem, debaixo das asas.",
                ("Hard plates. Hit under the wings.", "Placas duras. Bate debaixo das asas."),
                ("It has a link of the chain on its leg. Someone should take it off.", "Ela tem um elo da corrente na perna. Alguém devia tirar."),
                ("It clangs when you hit it. Like a pot. An angry pot.", "Ela faz 'blém' quando você bate. Que nem panela. Uma panela brava."),
                ("It grew up on the chain. It doesn't know any other place.", "Ela cresceu na corrente. Não conhece outro lugar."),
                ("Iron feathers! Does it go rusty in the rain?", "Pena de ferro! Ela enferruja na chuva?"));

            // -- Torre de Vigia e Cume de Ohya (52) --
            Page(entries, "enemy_mountain_caucasus_bear",
                "The bears of the Caucasus sleep the winter in the caves under the chain, and the svan towers leave honey out for them in spring, so they don't come looking. The shadow woke one too early and too angry. It walks the crags in the snow wearing a broken link of the chain round its neck like a collar. It is slow and terribly strong. Never trade blows with it; hit and step back.",
                "Os ursos do Cáucaso dormem o inverno nas cavernas debaixo da corrente, e as torres svan deixam mel pra eles na primavera, pra eles não virem procurar. A sombra acordou um cedo demais e bravo demais. Ele anda pelos penhascos na neve com um elo quebrado da corrente no pescoço, feito coleira. É lento e forte que só. Nunca troquem pancada com ele; batam e recuem.",
                ("Hit and step back. Never trade blows.", "Bate e recua. Nunca troca pancada."),
                ("He just wants to go back to sleep. I know the feeling.", "Ele só quer voltar a dormir. Eu sei como é."),
                ("I left him my honey cake. He didn't care. Rude.", "Eu deixei meu bolo de mel pra ele. Ele nem ligou. Mal-educado."),
                ("Someone put that link round his neck. Or he broke it off himself. I'm not sure which is worse.", "Alguém pôs esse elo no pescoço dele. Ou ele mesmo arrancou. Não sei o que é pior."),
                ("He's so big and fluffy! Can I hug him when he's nice?", "Ele é tão grande e fofo! Posso abraçar quando ele ficar bonzinho?"));

            // -- Cume de Ohya (52) --
            Page(entries, "enemy_forest_snow_wolf",
                "The snow wolves live above the svan towers and came down only in the worst winters. Since the shadow, and Ohya sitting on the crest, every winter is the worst one, and they hunt the climbers of the chain in the white. You won't see them until they move. When the snow beside you stands up, it is a wolf. Keep your back to the rock.",
                "Os lobos de neve vivem acima das torres svan e só desciam nos piores invernos. Desde a sombra, e o Ohya sentado no cume, todo inverno é o pior, e eles caçam quem sobe a corrente no meio do branco. Vocês não vão ver eles até eles se mexerem. Quando a neve do lado de vocês levantar, é lobo. Fiquem de costas pra pedra.",
                ("Back to the rock. They come from the sides.", "Costas pra pedra. Eles vêm pelos lados."),
                ("They're cold all the time now. Poor things.", "Eles agora vivem com frio. Coitados."),
                ("The snow BIT me. Write that down. The snow bit me.", "A neve me MORDEU. Anota aí. A neve me mordeu."),
                ("White on white. The shadow likes hiding in the brightest places too.", "Branco no branco. A sombra também gosta de se esconder nos lugares mais claros."),
                ("Its fur is like a snowman's! A snowman with teeth!", "O pelo dele é de boneco de neve! Um boneco de neve com dente!"));
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
