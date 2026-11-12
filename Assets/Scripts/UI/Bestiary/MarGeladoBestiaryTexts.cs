using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the beasts of the Mar Gelado (Docs/Mapas100.md 59–64) that the overworld layout used to
    /// play with stand-ins: lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="FarnkeshBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// The peoples of the Arctic are alive: only the beasts and the shadow appear here, and the komi tales (Yen, Omöl, the
    /// mammoth under the earth, the kul) only as the sheets use them.
    /// </summary>
    public static class MarGeladoBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_leopard_seal", "enemy_mountain_smoke_walrus", "enemy_mountain_ice_gull", "enemy_mountain_arctic_fox",
            "enemy_mountain_polar_bear", "enemy_mountain_clay_reindeer", "enemy_mountain_stone_hare", "enemy_mountain_twig_bird",
            "enemy_mountain_burrow_mammoth", "enemy_mountain_sea_wolf", "enemy_mountain_giant_pike", "enemy_mountain_shadow_seal",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Porto da Lua Branca and its bays (59; listed with the Mountain) --
            Page(entries, "enemy_mountain_leopard_seal",
                "The leopard seal is a real animal of the cold seas, long and spotted, with a head almost like a lizard's, and it is the fastest swimmer under the ice. Yirkap says they used to follow the moon boats into the harbour and play in their wake. The shadow spread their spots like spilled ink and gave them Āz's hunger, so now they slide out of the water at anyone on the shore. On the ice they are quicker than they look: when the mouth opens wide, step back, don't turn your back.",
                "A foca-leopardo é um bicho de verdade dos mares frios, comprida e pintada, com a cabeça quase de lagarto, e é a que nada mais rápido debaixo do gelo. O Yirkap conta que elas seguiam os barcos da lua pra dentro do porto e brincavam na esteira deles. A sombra espalhou as pintas delas feito tinta derramada e deu a fome da Āz, então agora elas escorregam da água pra cima de quem estiver na beira. No gelo elas são mais rápidas do que parecem: quando a boca abre grande, deem um passo pra trás, não deem as costas.",
                ("She comes out of the water fast. I keep three steps from every hole.", "Ela sai da água rápido. Eu fico a três passos de cada buraco."),
                ("She used to play behind the boats. She'll remember the game.", "Ela brincava atrás dos barcos. Ela vai lembrar da brincadeira."),
                ("She slid right between my legs. On the ice. I sat down. On the ice.", "Ela escorregou bem no meio das minhas pernas. No gelo. Eu sentei. No gelo."),
                ("Long body, short flippers, and it still outruns me. Hydrodynamics on land.", "Corpo comprido, nadadeira curta, e ainda corre mais que eu. Hidrodinâmica em terra."),
                ("Her spots look like Vó's ink when the pen leaks!", "As pintas dela parecem a tinta da Vó quando a caneta vaza!"));

            Page(entries, "enemy_mountain_smoke_walrus",
                "The walruses of the Bay of the Seals are big and old and grumpy, and the people of the harbour always left the warm rocks to them. When the shadow came across the frozen sea it got under their thick hide, and now they smoulder: smoke rises off their backs and there are embers in the folds, as if the cold could never touch them again. That is Āz's lie: it is not warmth, it is a fire that eats. Watch the tusks: when the head comes down, the walrus is coming with all its weight.",
                "As morsas da Baía das Focas são grandes, velhas e rabugentas, e o povo do porto sempre deixou as pedras quentes pra elas. Quando a sombra atravessou o mar gelado ela entrou debaixo do couro grosso delas, e agora elas fumegam: sobe fumaça das costas e tem brasa nas dobras, como se o frio nunca mais pudesse tocar nelas. Essa é a mentira da Āz: não é calor, é um fogo que come. Olhem as presas: quando a cabeça abaixa, a morsa vem com o peso todo.",
                ("Slow on land. I shoot, I move, I shoot again.", "Devagar em terra. Eu atiro, ando, atiro de novo."),
                ("She's burning inside and doesn't know. We have to put that fire out.", "Ela tá queimando por dentro e não sabe. A gente tem que apagar esse fogo."),
                ("She leaned on my shield. My arms are still shaking.", "Ela se encostou no meu escudo. Meus braços ainda tão tremendo."),
                ("A walrus that smokes in the snow. The thermodynamics are all wrong.", "Uma morsa que fumega na neve. A termodinâmica tá toda errada."),
                ("She has a moustache like Grandpa Jorge! An angry Grandpa Jorge.", "Ela tem bigode igual o Vô Jorge! Um Vô Jorge bravo."));

            Page(entries, "enemy_mountain_ice_gull",
                "The ice gulls are the harbour's own birds, white with grey wings, and they have always stolen a fish or two from the drying racks; Pamour says a harbour without thieving gulls is a sad harbour. The shadow froze their feathers hard as icicles and gave them a hunger for everything that shines. Now they circle high over the racks and throw those frozen feathers down from far away. Look for the beak opening: the feather comes right after.",
                "As gaivotas-do-gelo são os pássaros do próprio porto, brancas de asa cinza, e sempre roubaram um peixe ou dois dos varais; o Pamour diz que porto sem gaivota ladra é porto triste. A sombra congelou as penas delas, duras feito pingente de gelo, e deu uma fome de tudo que brilha. Agora elas rodam lá no alto em cima dos varais e jogam essas penas congeladas de longe. Olhem o bico abrindo: a pena vem logo depois.",
                ("She throws from high up. I aim where she'll be, not where she is.", "Ela joga lá de cima. Eu miro onde ela vai estar, não onde ela tá."),
                ("Pamour likes them. Let's chase them off without hurting them too much.", "O Pamour gosta delas. Vamos espantar sem machucar muito."),
                ("A frozen feather hit me on the head. Now I've got an icicle hat.", "Uma pena congelada bateu na minha cabeça. Agora eu tenho um chapéu de pingente."),
                ("Feathers hard as ice and they still glide. The wing shape does all the work.", "Pena dura feito gelo e ainda planam. O formato da asa faz tudo."),
                ("She stole the fish from the rack! I saw it! Thief gull!", "Ela roubou o peixe do varal! Eu vi! Gaivota ladra!"));

            Page(entries, "enemy_mountain_arctic_fox",
                "The arctic fox is a real animal, small and white in winter, and it follows the bears to eat what they leave behind; it is the cleverest thing on the ice. The foxes of the harbour learned to sneak into the fishing quarter, and nobody minded much. The shadow made them want every fish, every lantern, every shiny thing, and now they run off with them tied in their tails. If one gets hurt, it runs, and it comes back later for what it left. Keep your pockets closed.",
                "A raposa-do-ártico é um bicho de verdade, pequena e branca no inverno, e segue os ursos pra comer o que eles deixam; é a coisa mais esperta do gelo. As raposas do porto aprenderam a entrar escondidas no bairro da pesca, e ninguém ligava muito. A sombra fez elas quererem todo peixe, toda lamparina, toda coisa brilhante, e agora elas fogem com tudo amarrado no rabo. Se uma se machuca, ela corre, e volta depois pra buscar o que deixou. Fechem os bolsos.",
                ("White on white. I follow the footprints, not the fox.", "Branco no branco. Eu sigo a pegada, não a raposa."),
                ("She just wanted to eat. Pamour can leave her the small fish, like before.", "Ela só queria comer. O Pamour pode deixar os peixinhos pra ela, igual antes."),
                ("She stole my snack again. A different fox. Same snack.", "Ela roubou meu lanche de novo. Outra raposa. Mesmo lanche."),
                ("Short ears and a short muzzle: less skin to lose heat. Smart design.", "Orelha curta e focinho curto: menos pele pra perder calor. Projeto esperto."),
                ("She has a fish in her tail! Vó, can I have a fish in my hair?", "Ela tem um peixe no rabo! Vó, posso ter um peixe no cabelo?"));

            // -- Canais de Gelo (60) --
            Page(entries, "enemy_mountain_polar_bear",
                "The white bear is a real animal, the biggest hunter on the ice, and it walks for days over the frozen sea looking for seals. The boatmen of the canals used to share the catch with them and the bears kept away from the snow houses. The shadow put grey streaks down its white fur and made it believe the canals and everything in them are its own. It is heavy and patient: it waits by the holes for a long time, and then it is very fast. Never stand between a bear and the water.",
                "O urso-branco é um bicho de verdade, o maior caçador do gelo, e anda dias em cima do mar congelado procurando foca. Os barqueiros dos canais dividiam a pesca com eles e os ursos ficavam longe das casas de neve. A sombra pôs listras cinzentas no pelo branco dele e fez ele achar que os canais e tudo que tem dentro são só dele. Ele é pesado e paciente: espera muito tempo do lado dos buracos, e aí fica rapidíssimo. Nunca fiquem entre um urso e a água.",
                ("He waits. I can wait longer. And I shoot from here.", "Ele espera. Eu espero mais. E atiro daqui."),
                ("The boatmen shared their fish with him. We can make that true again.", "Os barqueiros dividiam o peixe com ele. A gente consegue fazer isso voltar."),
                ("I stood between the bear and the water. Vó was right. Don't do that.", "Eu fiquei entre o urso e a água. A Vó tinha razão. Não façam isso."),
                ("White fur, black skin underneath to hold the sun's heat. A living solar panel.", "Pelo branco, pele preta embaixo pra guardar o calor do sol. Um painel solar vivo."),
                ("He's like a giant pillow. A pillow with teeth.", "Ele parece um travesseiro gigante. Um travesseiro com dente."));

            // -- Tundra de Omöl (61) --
            Page(entries, "enemy_mountain_clay_reindeer",
                "The komi tale says that two brothers made the world: Yen made the good, tidy things, and Omöl made the clumsy ones, crooked and lumpy and a little sad. The clay reindeer is one of Omöl's. He shaped it in a hurry out of tundra clay, one leg a bit long, antlers that don't match, and it walked off proud anyway. The shadow found the cracks in the clay and filled them with anger, so now it charges at anyone who looks at it. When the head drops, the crooked antlers are coming. At night it sleeps standing up: walk past quietly.",
                "O conto dos komi diz que dois irmãos fizeram o mundo: o Yen fez as coisas boas e arrumadas, e o Omöl fez as desajeitadas, tortas, encaroçadas e um pouco tristes. A rena de barro é uma das do Omöl. Ele modelou ela com pressa, de barro da tundra, uma perna meio comprida, chifres que não combinam, e ela saiu andando toda orgulhosa mesmo assim. A sombra achou as rachaduras do barro e encheu de raiva, e agora ela carrega em cima de quem olha pra ela. Quando a cabeça abaixa, os chifres tortos estão vindo. De noite ela dorme em pé: passem quietinhos.",
                ("One leg is longer. She always veers left. I stand right.", "Uma perna é mais comprida. Ela sempre puxa pra esquerda. Eu fico na direita."),
                ("She was proud of herself. I want her to feel proud again.", "Ela tinha orgulho de si. Eu quero que ela sinta orgulho de novo."),
                ("Her antler hit my shield and a bit of clay fell off. Sorry, reindeer.", "O chifre dela bateu no meu escudo e caiu um pedaço de barro. Desculpa, rena."),
                ("Fired clay with cracks. Thermal stress. Omöl needed a better kiln.", "Barro queimado com rachadura. Choque térmico. O Omöl precisava de um forno melhor."),
                ("Her antlers don't match! Like my socks! We're the same!", "Os chifres dela não combinam! Igual minhas meias! A gente é igual!"));

            Page(entries, "enemy_mountain_stone_hare",
                "Another of Omöl's creatures: he wanted to make a hare, quick and light, but he made it out of a round grey stone and forgot to make it light. It is still quick, though, and when it jumps at you it hits like a thrown rock. Before the shadow they only bounced about between the lakes and made the herders laugh. Now they throw themselves head first at anything that moves. When it dips its head, jump aside; if it gets hurt, it runs off and hides among the round stones of the tundra.",
                "Outra criatura do Omöl: ele queria fazer uma lebre, rápida e leve, mas fez de uma pedra redonda e cinzenta e esqueceu de fazer leve. Mas ela continua rápida, e quando pula em cima da gente bate feito pedra jogada. Antes da sombra elas só ficavam pulando entre os lagos e faziam os pastores rirem. Agora elas se jogam de cabeça em tudo que se mexe. Quando ela abaixa a cabeça, pulem pro lado; se ela se machuca, foge e se esconde no meio das pedras redondas da tundra.",
                ("She hides among the stones. I count the stones. One too many.", "Ela se esconde no meio das pedras. Eu conto as pedras. Tem uma a mais."),
                ("She made the herders laugh. She was a funny hare. She still can be.", "Ela fazia os pastores rirem. Era uma lebre engraçada. Ainda pode ser."),
                ("A rock jumped at me. A rock with ears. That's not fair.", "Uma pedra pulou em mim. Uma pedra com orelha. Isso não é justo."),
                ("Stone mass, hare speed. Momentum is mass times velocity. Ouch.", "Massa de pedra, velocidade de lebre. Momento é massa vezes velocidade. Ai."),
                ("Her ears are crooked! Omöl made her ears crooked and she's cute anyway!", "As orelhas dela são tortas! O Omöl fez torta e ela é fofa mesmo assim!"));

            Page(entries, "enemy_mountain_twig_bird",
                "The twig bird is the clumsiest of all of Omöl's creatures: he tied it together from bark and sticks and lichen, gave it eyes much too big and one wing shorter than the other, and it flew anyway, crooked and happy. It looks a little like the potoo of the forests, the one that pretends to be a branch. The shadow put a sulk in it: now it sits far off and throws the sticks of its tail like darts. It always flies a little to one side. Mind the beak opening: the stick comes right after.",
                "A ave-graveto é a mais desajeitada de todas as criaturas do Omöl: ele amarrou ela com casca, graveto e líquen, deu uns olhos grandes demais e uma asa mais curta que a outra, e ela voou mesmo assim, torta e feliz. Ela parece um pouco o urutau das matas, aquele que finge que é galho. A sombra pôs um emburramento nela: agora ela fica longe e joga os gravetos do rabo feito dardo. Ela sempre voa um pouco pro lado. Cuidado com o bico abrindo: o graveto vem logo depois.",
                ("She drifts to the short wing every time. I aim a little to that side.", "Ela puxa pro lado da asa curta toda vez. Eu miro um pouco pra esse lado."),
                ("She flew happy before. Crooked and happy. That's allowed.", "Ela voava feliz antes. Torta e feliz. Isso pode."),
                ("She threw a stick at me. I threw it back. She threw two.", "Ela jogou um graveto em mim. Eu joguei de volta. Ela jogou dois."),
                ("Asymmetric wings and it still flies. Constant correction. Impressive.", "Asa assimétrica e ainda voa. Correção o tempo todo. Impressionante."),
                ("She's a flying nest! Can a little bird live in her?", "Ela é um ninho que voa! Um passarinho pode morar nela?"));

            // -- Galerias dos Mamutes (62) --
            Page(entries, "enemy_mountain_burrow_mammoth",
                "In the komi and mansi tales the mammoth is an animal that lives under the earth and digs the valleys as it walks, and when it comes up to the light it melts away. The mammoths of the galleries are that kind: shaggy, with ice crystals growing on their backs in the dark, and great curved tusks that open new tunnels every week. The shadow made them forget the way up and made them guard the tunnels as if they were treasure. Don't stand in front of one when it lowers its tusks. A mammoth that charges doesn't turn.",
                "Nos contos dos komi e dos mansi o mamute é um bicho que vive debaixo da terra e cava os vales enquanto anda, e quando sobe pra luz ele derrete. Os mamutes das galerias são desse tipo: peludos, com cristal de gelo crescendo nas costas no escuro, e presas curvas enormes que abrem túneis novos toda semana. A sombra fez eles esquecerem o caminho pra cima e guardarem os túneis como se fossem tesouro. Não fiquem na frente quando ele abaixa as presas. Mamute que carrega não faz curva.",
                ("He doesn't turn when he charges. I step aside and shoot his back.", "Ele não faz curva quando carrega. Eu saio do caminho e atiro nas costas."),
                ("He forgot the way up. Let's show him, slowly, so he doesn't melt.", "Ele esqueceu o caminho pra cima. Vamos mostrar devagar, pra ele não derreter."),
                ("I held him for one second. One whole second. I'm counting that.", "Eu segurei ele por um segundo. Um segundo inteiro. Isso conta."),
                ("Crystals growing on fur in the dark. That's a whole cave on legs.", "Cristal crescendo no pelo no escuro. Isso é uma caverna inteira de pernas."),
                ("He's so big! Is he the grandpa of the elephants?", "Ele é muito grande! Ele é o vovô dos elefantes?"));

            // -- Mar das Auroras (63) --
            Page(entries, "enemy_mountain_sea_wolf",
                "There is no sea wolf in the books, I made the name up, but the herders of the Mar das Auroras swear they have seen them: wolves that swim between the floes, with webbed paws, a fin down the back and a tail that steers like a fish's. They used to hunt in packs far out at sea and never came near the lighthouse. The shadow sent them up on the ice in packs, after anything that walks. Fight them with your back to something; they like to come from every side at once.",
                "Lobo-do-mar não tem nos livros, o nome eu que inventei, mas os pastores do Mar das Auroras juram que já viram: lobos que nadam entre as placas de gelo, de pata com membrana, uma barbatana nas costas e um rabo que vira leme feito de peixe. Eles caçavam em bando lá longe no mar e nunca chegavam perto do farol. A sombra mandou eles subirem no gelo em bando, atrás de tudo que anda. Lutem com as costas protegidas; eles gostam de vir de todos os lados ao mesmo tempo.",
                ("Back to the lighthouse wall. Now they can only come from the front.", "Costas na parede do farol. Agora eles só vêm pela frente."),
                ("They're a family. They take care of each other. That's a good thing, deep down.", "Eles são uma família. Cuidam uns dos outros. Isso é uma coisa boa, lá no fundo."),
                ("Five wolves, one me. I like those odds. I don't, actually.", "Cinco lobos, um eu. Gosto dessas chances. Na verdade não gosto."),
                ("Webbed paws and a fin. Convergent evolution, or Vó made it up. Or both.", "Pata com membrana e barbatana. Evolução convergente, ou a Vó inventou. Ou os dois."),
                ("A wolf that swims! Can I ride one in the water? Just a little?", "Um lobo que nada! Posso andar num na água? Só um pouquinho?"));

            // -- Fundo do Velho Kul (64) --
            Page(entries, "enemy_mountain_giant_pike",
                "The pike is a real fish of the cold lakes, long and green with pale spots and a duck-bill mouth full of needle teeth, and it hunts by lying still and then shooting forward. In Old Kul's lake they grew huge. The komi tales say the kul, the water spirits, keep the fish of the lakes; the old one under the ice has gone sour, and his pikes swim up out of the fishing holes and through the cold air itself, looking for whoever is fishing. When it goes still, it is about to dart.",
                "O lúcio é um peixe de verdade dos lagos frios, comprido e verde de pinta clara, com a boca de bico de pato cheia de dente de agulha, e ele caça ficando parado e depois disparando pra frente. No lago do Velho Kul eles ficaram enormes. Os contos dos komi dizem que os kul, os espíritos da água, guardam os peixes dos lagos; o velho debaixo do gelo azedou, e os lúcios dele sobem pelos buracos de pesca e nadam no próprio ar frio, procurando quem estiver pescando. Quando ele fica parado, é porque vai disparar.",
                ("He freezes before the dart. That's my moment, not his.", "Ele congela antes de disparar. Esse é o meu momento, não o dele."),
                ("Old Kul is holding the fish. When he lets go, the pikes will go home.", "O Velho Kul tá segurando os peixes. Quando ele soltar, os lúcios voltam pra casa."),
                ("A fish bit my boot. In the air. I'm not even near the water.", "Um peixe mordeu minha bota. No ar. Eu nem tô perto da água."),
                ("A fish swimming in air. The air down here is so cold it's almost water.", "Um peixe nadando no ar. O ar aqui embaixo é tão frio que quase vira água."),
                ("His mouth is like a duck's! A duck with too many teeth.", "A boca dele parece de pato! Um pato com dente demais."));

            Page(entries, "enemy_mountain_shadow_seal",
                "The ringed seal is a real animal, round and grey with pale rings on its coat, and it keeps its breathing holes open all winter by scratching at the ice. The seals of Old Kul's lake turned dark as the water under the ice, and the rings on their coats glow like the fishing holes at night. They lie heavy across the holes and won't let anyone fish, because Āz told them that every fish is theirs. They are slow, but they are heavy, and the bite is hard. Free the holes and the fish come back.",
                "A foca-anelada é um bicho de verdade, redonda e cinza de anel claro no pelo, e ela mantém os buracos de respirar abertos o inverno inteiro arranhando o gelo. As focas do lago do Velho Kul ficaram escuras feito a água debaixo do gelo, e os anéis do pelo brilham igual os buracos de pesca de noite. Elas se deitam pesadas em cima dos buracos e não deixam ninguém pescar, porque a Āz disse que todo peixe é delas. Elas são lentas, mas são pesadas, e a mordida é forte. Liberem os buracos que os peixes voltam.",
                ("She guards the hole. I shoot from the next hole over.", "Ela guarda o buraco. Eu atiro do buraco do lado."),
                ("Her rings are glowing like little lanterns. There's still light in her.", "Os anéis dela brilham feito lamparininha. Ainda tem luz nela."),
                ("I tried to push her off the hole. She didn't move. I moved.", "Eu tentei empurrar ela de cima do buraco. Ela não saiu. Eu saí."),
                ("She keeps the hole open by scratching. A seal with a maintenance schedule.", "Ela mantém o buraco aberto arranhando. Uma foca com cronograma de manutenção."),
                ("She has whiskers like a cat! A big round shadow cat!", "Ela tem bigode de gato! Um gatão redondo de sombra!"));
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
