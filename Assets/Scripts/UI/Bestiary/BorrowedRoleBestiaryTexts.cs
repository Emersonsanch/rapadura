using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of Regions 7–16 that the overworld still played with another region's beast after the
    /// fifteenth pass (bible §3.2.1, sixteenth pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="WideStandInBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class BorrowedRoleBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_cave_living_crevasse", "enemy_mountain_ice_mole", "enemy_mountain_flying_pumice", "enemy_forest_glass_urchin",
            "enemy_mountain_instant_lizard", "enemy_mountain_black_catfish", "enemy_mountain_tremor", "enemy_mountain_erasing_water",
            "enemy_forest_stowaway_shadow", "enemy_forest_swimming_shadow", "enemy_mountain_wrong_current", "enemy_mountain_sky_crack",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Geleira do Teto do Mundo e Campo de Seracs (38) --
            Page(entries, "enemy_cave_living_crevasse",
                "The glacier on the Roof of the World splits and closes every year, and the yak herders always knew where, so they walked round. Since the shadow the cracks move about. One of them lies down across the path, waits for a foot, and closes. If the snow in front of you shows a blue line, it is not a shadow of the sky. Go round it, or hit it before it opens.",
                "A geleira do Teto do Mundo racha e fecha todo ano, e os pastores de iaque sempre sabiam onde, então davam a volta. Desde a sombra as rachaduras andam. Uma delas deita atravessada no caminho, espera um pé e fecha. Se a neve na frente de vocês tiver uma linha azul, não é sombra do céu. Deem a volta, ou batam antes que ela abra.",
                ("It bites when it opens. Hit it while it's shut.", "Morde quando abre. Bate enquanto tá fechada."),
                ("It's just cold and lonely. Like the whole glacier.", "Ela só tá com frio e sozinha. Que nem a geleira inteira."),
                ("I'm not stepping on the blue. I'm not stepping on ANY blue.", "Eu não piso no azul. Não piso em NENHUM azul."),
                ("A crack is only a place where the ice let go. This one learned to let go on purpose.", "Rachadura é só onde o gelo soltou. Essa aprendeu a soltar de propósito."),
                ("It's got teeth made of icicles! Can I keep one?", "Ela tem dentes de pingente de gelo! Posso ficar com um?"));

            // -- Galerias dos Mamutes e Terra Congelada (62) --
            Page(entries, "enemy_mountain_ice_mole",
                "The ice moles dig the little tunnels beside the mammoth's big ones, and they kept the galleries from falling in; the mammoth never minded them. The cold went into their claws when the shadow came under the earth, and now they dig at whoever walks the tunnels. They come up out of the floor in a crowd. Keep moving, and watch for the snow that bulges.",
                "As toupeiras-gelo cavam os túneis pequenos do lado dos túneis grandes do mamute, e seguravam as galerias pra não desabar; o mamute nunca se importou com elas. O frio entrou nas unhas delas quando a sombra chegou debaixo da terra, e agora elas cavam em quem anda pelos túneis. Saem do chão em bando. Continuem andando, e olhem a neve que estufa.",
                ("They swarm out of the floor. Keep moving.", "Saem do chão em bando. Não para."),
                ("They held the tunnels up. They can do it again.", "Elas seguravam os túneis. Podem fazer de novo."),
                ("One bit my boot from UNDER the snow. That's cheating.", "Uma mordeu minha bota por BAIXO da neve. Isso é trapaça."),
                ("An animal that lives by feel. The shadow made it feel the wrong things.", "Um bicho que vive no tato. A sombra fez ele sentir as coisas erradas."),
                ("Its nose is a little pink star! Look, it's wiggling!", "O nariz dela é uma estrelinha rosa! Olha, tá mexendo!"));

            // -- Hinokura, a Ilha-Forja, e os arredores (65) --
            Page(entries, "enemy_mountain_flying_pumice",
                "The vents of Hinokura blow pumice up into the air, and it used to fall back down as grit for the polishers of the forges. Since the shadow it does not come down: it drifts about the fumaroles in flocks, light as bread, with the ember of the volcano still in its holes, and drops on anyone below. Look up near the vents. Break them before they gather.",
                "Os respiros de Hinokura sopram pedra-pomes pro alto, e ela caía de volta como areia pros polidores das forjas. Desde a sombra ela não desce mais: fica boiando em volta das fumarolas, em bando, leve que nem pão, com a brasa do vulcão ainda nos furinhos, e cai em cima de quem passa embaixo. Olhem pra cima perto dos respiros. Quebrem antes que juntem.",
                ("They flock. Break them before they gather.", "Vêm em bando. Quebra antes que juntem."),
                ("It just wants to come down. Poor thing.", "Ela só quer descer. Coitada."),
                ("A rock that FLOATS. I hit it and it floated AWAY.", "Uma pedra que BOIA. Eu bati e ela boiou pra LONGE."),
                ("A stone lighter than water is already a strange thing. Lighter than air is the shadow's joke.", "Pedra mais leve que a água já é coisa estranha. Mais leve que o ar é piada da sombra."),
                ("It's full of tiny holes, like a sponge! A hot sponge!", "Ela é cheia de furinhos, que nem esponja! Uma esponja quente!"));

            // -- Recife de Obsidiana e Ilhotas de Vidro (68) --
            Page(entries, "enemy_forest_glass_urchin",
                "When the lava ran into the sea it turned to black glass all along the reef, and some of it, the shadow says, kept walking. It sits on the islets like an urchin with the heat of the volcano still inside it, and every spine cuts. It is slow and very hard. Don't kick it. Hit it with something long, and step out of the way when it rolls.",
                "Quando a lava correu pro mar ela virou vidro preto ao longo do recife inteiro, e um pouco dela, diz a sombra, continuou andando. Fica nas ilhotas que nem ouriço, com o calor do vulcão ainda dentro, e todo espinho corta. É lenta e muito dura. Não chutem. Batam com coisa comprida, e saiam da frente quando ela rolar.",
                ("Slow and very hard. Stay out of its roll.", "Lento e muito duro. Sai da frente quando rolar."),
                ("It's still warm inside. Somewhere in there it remembers being lava.", "Por dentro ainda tá quente. Lá dentro ele lembra que era lava."),
                ("I KICKED it. I would like everyone to know I regret it.", "Eu CHUTEI ele. Quero que todo mundo saiba que me arrependo."),
                ("Glass is fire that stood still. This one is impatient about it.", "Vidro é fogo que ficou parado. Esse aqui tá sem paciência."),
                ("It's so shiny! I can see my face in it! Upside down!", "Ele é tão brilhante! Dá pra ver minha cara! De ponta-cabeça!"));

            // -- Deserto do Relógio e Areia das Horas (78) --
            Page(entries, "enemy_mountain_instant_lizard",
                "In the desert of the gnomons the shadows come a moment late, and these lizards live inside that moment. They are already where you are about to look, and the smudge you see is where they were. Their backs carry a little sundial, the only one in the desert that is right. Don't chase the smudge. Wait for the bite, and turn to where the real one is.",
                "No deserto dos gnômons as sombras chegam um instante atrasadas, e esses lagartos moram dentro desse instante. Já estão onde vocês iam olhar, e a mancha que vocês veem é onde eles estavam. Nas costas eles carregam um relogiozinho de sol, o único do deserto que acerta a hora. Não corram atrás da mancha. Esperem a mordida, e virem pra onde o de verdade está.",
                ("Fast. Hit where it's going, not where it is.", "Rápido. Bate onde ele vai estar, não onde está."),
                ("It's a moment early. That must be lonely.", "Ele tá um instante adiantado. Deve ser solitário."),
                ("I hit it! ...I hit where it WAS.", "Acertei! ...Acertei onde ele ESTAVA."),
                ("A clock on the back of something that won't wait. The desert has a sense of humour.", "Um relógio nas costas de um bicho que não espera. O deserto tem senso de humor."),
                ("There's two of it! No, one! No, two! Make it stop!", "Tem dois! Não, um! Não, dois! Faz parar!"));

            // -- Poços de Betume (79) --
            Page(entries, "enemy_mountain_black_catfish",
                "The catfish of the bitumen wells lived at the bottom and kept the tar soft for the caulkers who sealed the reed boats, like in Atrahasis's time. The pitch went dark, and the catfish went dark with it. Now it lies under the rafts, as black as the wells, and tips them over. It is big and it does not tire. Fight it on the firm edge, never on the raft.",
                "O bagre dos poços de betume morava no fundo e deixava o piche mole pros calafates que vedavam os barcos de junco, como no tempo de Atrahasis. O piche escureceu, e o bagre escureceu junto. Agora ele fica debaixo das balsas, preto que nem os poços, e vira elas. É grande e não cansa. Briguem com ele na beira firme, nunca em cima da balsa.",
                ("Big and tireless. Fight on solid ground.", "Grande e não cansa. Briga em chão firme."),
                ("It kept the boats safe once. It's in there somewhere.", "Ele já protegeu os barcos. Ainda tá ali dentro."),
                ("It has a MUSTACHE. A long one. With lights on the end.", "Ele tem BIGODE. Comprido. Com luzinha na ponta."),
                ("The ones who keep things working are always the last to be thanked, and the first to be forgotten.", "Quem mantém as coisas funcionando é sempre o último a ganhar obrigado, e o primeiro a ser esquecido."),
                ("Its whiskers glow like fishing lines! Is it fishing for us?", "Os bigodes dele brilham que nem linha de pesca! Ele tá pescando a gente?"));

            // -- Ombro de Upelluri (81) --
            Page(entries, "enemy_mountain_tremor",
                "On Upelluri's shoulder the ground is skin of stone, and it shudders where the diorite grows into it, the way your arm twitches when a fly lands. The shudders took a shape of their own, a heap of stone with a seam down the middle, and they walk about knocking people off their feet. When the seam lights up, the ground is about to jump. Step off it.",
                "No ombro de Upelluri o chão é pele de pedra, e ele estremece onde o diorito cresce dentro, do jeito que o braço da gente treme quando pousa uma mosca. Os tremores ganharam forma própria, um monte de pedra com uma costura no meio, e andam por aí derrubando as pessoas. Quando a costura acender, o chão vai pular. Saiam de cima.",
                ("When the seam lights, step away. It hits hard.", "Quando a costura acender, sai de perto. Bate forte."),
                ("The giant's shoulder hurts. That's all this is.", "O ombro do gigante tá doendo. É só isso."),
                ("It shook me so hard my teeth are still going.", "Me sacudiu tanto que meus dentes ainda tão batendo."),
                ("A shiver that walks. The giant itself never even noticed.", "Um arrepio que anda. O gigante nem percebeu."),
                ("It's like a jelly made of rock! Wobble wobble!", "Parece gelatina de pedra! Treme-treme!"));

            // -- Campo das Tábuas do Sonho (82) --
            Page(entries, "enemy_mountain_erasing_water",
                "In the field of the dream tablets the dreams were written down so that they would not be lost; your grandfather wrote one there once and never told me what. The still water came up between the tablets and began to wash them blank, and it stands up like a person when someone comes to read. Inside it you can see the signs it washed off, sinking. Don't let it touch your notebook.",
                "No campo das tábuas do sonho os sonhos eram escritos pra não se perderem; o avô de vocês escreveu um lá uma vez e nunca me contou qual. A água parada subiu entre as tábuas e começou a lavar tudo até ficar em branco, e ela fica de pé que nem gente quando alguém chega pra ler. Dentro dela dá pra ver os sinais que ela apagou, afundando. Não deixem ela encostar no caderno de vocês.",
                ("Slow, but it hits hard. Don't stand next to it.", "Lenta, mas bate forte. Não fica do lado."),
                ("It's carrying all those dreams. Let's give them back.", "Ela tá carregando todos esses sonhos. Vamos devolver."),
                ("It splashed my map! Where am I? Where is ANYONE?", "Espirrou no meu mapa! Onde eu tô? Onde tá TODO MUNDO?"),
                ("Forgetting is not the enemy. Forgetting on purpose is.", "Esquecer não é o inimigo. Esquecer de propósito é."),
                ("I can see letters swimming inside! Is that Grandpa's dream?", "Dá pra ver letrinhas nadando dentro! É o sonho do Vô?"));

            // -- Cais da Coluna (91) --
            Page(entries, "enemy_forest_stowaway_shadow",
                "The Barcas carry the freed light up to the Moon, and a shadow that wants to go along hides in the hold under a rag of old sail, hoping no one will look. The quay is full of them, at the edges, under the crates. They steal a little of the light that waits for the boats, and they hold on to whoever shakes them out. Pull the rag off and they have nowhere to hide.",
                "As Barcas levam a luz liberta até a Lua, e uma sombra que quer ir junto se esconde no porão debaixo de um trapo de vela velha, torcendo pra ninguém olhar. O cais tá cheio delas, nas beiradas, debaixo dos caixotes. Roubam um pouquinho da luz que espera as barcas, e agarram quem tenta sacudir elas pra fora. Tirem o trapo e elas não têm onde se esconder.",
                ("It hits hard from under the rag. Keep moving.", "Bate forte de baixo do trapo. Não para."),
                ("It only wanted to go along. Everyone wants to go home.", "Ela só queria ir junto. Todo mundo quer voltar pra casa."),
                ("I said BOO to the rag and the rag said BOO back.", "Eu falei BU pro trapo e o trapo falou BU de volta."),
                ("A stowaway on the boat to the light. The shadow wants what we want, it just won't pay the fare.", "Um clandestino no barco pra luz. A sombra quer o que a gente quer, só não quer pagar a passagem."),
                ("It's wearing a sail like a blanket! Like when we play ghost!", "Tá usando uma vela de cobertor! Que nem quando a gente brinca de fantasma!"));

            // -- Rio de Luz (92) --
            Page(entries, "enemy_forest_swimming_shadow",
                "A shadow can't swim in water, children, but it can swim in light. These ones slide along under the surface of the River of Light like eels and drink the glow of the boats from underneath. You only see them by the dark ribbon going the other way. They pull at whoever stands too near the bank. Step back from the edge and let them come up to bite.",
                "Sombra não nada na água, crianças, mas nada na luz. Essas aqui deslizam por baixo da superfície do Rio de Luz que nem enguia e bebem o brilho dos barcos por baixo. Só dá pra ver pela fita escura indo pro outro lado. Puxam quem fica muito perto da margem. Afastem da beira e deixem elas subirem pra morder.",
                ("It comes up from the light to bite. Step back from the edge.", "Sobe da luz pra morder. Afasta da beira."),
                ("It's drinking the light because it's thirsty. I know the feeling.", "Ela bebe a luz porque tá com sede. Eu sei como é."),
                ("I tried to grab it and got a handful of NOTHING.", "Tentei pegar e fiquei com a mão cheia de NADA."),
                ("Every river has something swimming against it. This one is just honest about it.", "Todo rio tem alguma coisa nadando contra. Esse pelo menos é sincero."),
                ("It's a shadow eel! It wiggles like a ribbon!", "É uma enguia de sombra! Mexe que nem fita!"));

            Page(entries, "enemy_mountain_wrong_current",
                "The River of Light runs one way, up toward the Moon and the Sun, carrying the freed light home. Here and there a strand of it runs the other way, back down, coiled tight round a knot where it turned on itself. It pulls the boats off course and holds the swimmers in place. Don't fight the pull. Strike the knot when it glows, and the strand lets go.",
                "O Rio de Luz corre pra um lado só, pra cima, pra Lua e pro Sol, levando a luz liberta pra casa. Aqui e ali um fio dele corre ao contrário, de volta pra baixo, enrolado num nó onde ele virou sobre si mesmo. Puxa os barcos pra fora do caminho e segura quem tá nadando. Não briguem contra o puxão. Batam no nó quando ele brilhar, que o fio solta.",
                ("When the knot glows, it lashes out. Strike first.", "Quando o nó brilhar, ela chicoteia. Bate antes."),
                ("It just got turned around. It can find the way again.", "Ela só se perdeu. Pode achar o caminho de novo."),
                ("I swam for ages and I'm exactly where I started.", "Nadei um tempão e tô exatamente onde comecei."),
                ("Not every wrong turn is wicked. But this one is pulling the boats with it.", "Nem toda curva errada é maldade. Mas essa tá puxando os barcos junto."),
                ("It's a spring made of light! Boing!", "É uma mola de luz! Boing!"));

            // -- Mirante do que Segura o Céu (99) --
            Page(entries, "enemy_mountain_sky_crack",
                "From the lookout you can see the cracks in the sky where the shadow drips through, and the one who holds the sky up holds it a little tighter every day. Some of the small cracks came loose with their edges, gold on one side and white on the other, and they walk about the platform trying to let more of it in. When the dark inside one swells, it is about to split wider. Close it with a good hit.",
                "Do mirante dá pra ver as rachaduras no céu por onde a sombra pinga, e quem segura o céu aperta um pouquinho mais a cada dia. Algumas rachaduras pequenas se soltaram com as beiradas, dourada de um lado e branca do outro, e andam pela plataforma tentando deixar entrar mais. Quando a treva dentro de uma inchar, ela vai abrir mais. Fechem com uma boa pancada.",
                ("When the dark swells, it splits wider. Hit it first.", "Quando a treva inchar, ela abre mais. Bate antes."),
                ("Every one we close is a little less for him to hold.", "Cada uma que a gente fecha é um pouco menos pra ele segurar."),
                ("I looked inside it. Don't look inside it.", "Eu olhei dentro dela. Não olhem dentro dela."),
                ("The sky was always whole. Cracks are what it looks like when someone forgets that.", "O céu sempre foi inteiro. Rachadura é o que aparece quando alguém esquece disso."),
                ("It's a zigzag that walks! Like lightning with legs!", "É um zigue-zague que anda! Que nem raio com perna!"));
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
