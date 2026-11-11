using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of Regions 5–9 (the road of the middle story, "A Lua que Não Enche") that the overworld still
    /// played with stand-ins (bible §3.2.1, thirteenth pass): lore in Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="EarlyStandInBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class MidStandInBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_dimmed_moray", "enemy_forest_smoke_urchin", "enemy_forest_lightning_bird", "enemy_forest_fiddler_crab",
            "enemy_forest_spear_heron", "enemy_forest_mire_leech", "enemy_mountain_red_billed_chough", "enemy_mountain_bearded_vulture",
            "enemy_mountain_ice_wolf", "enemy_forest_dragon_carp", "enemy_forest_terracotta_horse", "enemy_mountain_felt_swan",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Taa Pwani e o Recife (25) --
            Page(entries, "enemy_forest_dimmed_moray",
                "The morays live in the holes of the reef and bite whatever hand goes in after a shell. The coral lost its light when the Sun Crystal went out, and this one came out of its hole to look for more. It lies coiled on the flats with only its head up, very patient. Don't wade past it at low tide without looking, and when it lunges, step aside; it is quick, but it is all neck.",
                "As moreias moram nos buracos do recife e mordem qualquer mão que entra atrás de concha. O coral perdeu a luz quando o Cristal do Sol apagou, e esta saiu do buraco pra procurar mais. Fica enrolada nos baixios só com a cabeça de fora, muito paciente. Não passem por ela na maré baixa sem olhar, e quando ela der o bote, saiam de lado; é rápida, mas é só pescoço.",
                ("It strikes straight. Step to the side and hit the neck.", "Ela ataca reto. Sai de lado e bate no pescoço."),
                ("She's not angry. She's just been in the dark a long time.", "Ela não é brava. Só ficou muito tempo no escuro."),
                ("I thought it was a rope. Ropes don't have teeth!", "Achei que era uma corda. Corda não tem dente!"),
                ("All mouth and waiting. I know a few professors like that.", "Só boca e espera. Conheço uns professores assim."),
                ("She's green and spotty! Like the moss on Vó's pot!", "Ela é verde e pintadinha! Igual o musgo do vaso da Vó!"));

            Page(entries, "enemy_forest_smoke_urchin",
                "Urchins graze the stones of the tide pools and never hurt anybody who wore sandals. Smoke, the first of the elements of the Darkness, came in on a tide and filled one of them up. Now it rolls up the beach after warm feet and throws its spines, which are slow but stay hot. It cannot turn quickly. Walk round it, strike from behind, and keep your sandals on, my loves.",
                "Os ouriços pastam as pedras das poças de maré e nunca machucaram quem usava sandália. A Fumaça, o primeiro dos elementos da Treva, chegou numa maré e encheu um deles. Agora ele rola praia acima atrás de pé quente e solta os espinhos, que são lentos mas ficam quentes. Não consegue virar depressa. Deem a volta, batam por trás, e fiquem de sandália, meus amores.",
                ("Slow to turn. Circle it and strike from behind.", "Demora pra virar. Dá a volta e bate por trás."),
                ("There's an ember on top. That's what's left of the tide's light.", "Tem uma brasa em cima. É o que sobrou da luz da maré."),
                ("A spine got stuck in my shoe. My shoe!", "Um espinho ficou preso no meu sapato. No meu sapato!"),
                ("A pincushion that holds a grudge. The coast is full of surprises.", "Uma almofada de alfinetes com rancor. A costa é cheia de surpresas."),
                ("It's a smoky ball! Can I keep a spine? Just one?", "É uma bola de fumaça! Posso ficar com um espinho? Só um?"));

            // -- Savana dos Auroques e Bosque dos Baobás (26) --
            Page(entries, "enemy_forest_lightning_bird",
                "On the savanna they say the storm has a bird, black and white, that comes down with the lightning; where it lands, the grass burns. I think the old tellers saw what I see: in every rain a little of the light falls back to the earth, and the bird catches it. The Darkness has clouded this one. It keeps the spark and throws it. Don't stand in the open when it circles, and hit it while it gathers the next one.",
                "Na savana dizem que a tempestade tem um pássaro, preto e branco, que desce com o raio; onde ele pousa, o capim queima. Acho que os contadores antigos viam o que eu vejo: em toda chuva um pouco da luz cai de volta na terra, e o pássaro pega. A Treva nublou este. Ele guarda a faísca e joga. Não fiquem no descampado quando ele rodar em cima, e batam enquanto ele junta a próxima.",
                ("It throws the spark from far. Rush it while it recharges.", "Joga a faísca de longe. Corre nele enquanto recarrega."),
                ("It's holding light that should've gone back to the sky.", "Está segurando uma luz que devia voltar pro céu."),
                ("My hair stood up. All of it. Even the eyebrows.", "Meu cabelo ficou em pé. Todo. Até a sobrancelha."),
                ("A courier of the storm who forgot the address. Typical.", "Um mensageiro da tempestade que esqueceu o endereço. Típico."),
                ("It goes zap! Zap! Can it do it again? From further away?", "Ele faz zap! Zap! Faz de novo? Mais de longe?"));

            // -- Mangues de Liyongo (30) --
            Page(entries, "enemy_forest_fiddler_crab",
                "In the mangroves the little fiddler crabs wave their one big claw at the tide, as if they were calling it in. Your grandfather used to wave back. These have gone dim and grown to the size of a stool, and they wave the claw at you instead, and then they bring it down. They come out in crowds at the shallow edge. Watch the big claw; the small one only tickles.",
                "Nos mangues os caranguejinhos-violinistas acenam com a única garra grande pra maré, como se chamassem ela. O avô de vocês acenava de volta. Estes ficaram apagados e cresceram do tamanho de um banquinho, e acenam a garra pra vocês, e depois baixam ela. Saem em bando na beira rasa. Cuidado com a garra grande; a pequena só faz cócegas.",
                ("Watch the big claw. When it rises, back off.", "Olha a garra grande. Quando ela sobe, recua."),
                ("They're calling the tide. It just doesn't hear them any more.", "Estão chamando a maré. Ela só não escuta mais."),
                ("I waved back. That was a mistake.", "Eu acenei de volta. Foi um erro."),
                ("A whole species that argues with one hand. Admirable, really.", "Uma espécie inteira que discute com uma mão só. Admirável, na verdade."),
                ("They're waving at me! Hi! Hi! Ow!", "Eles tão me dando tchau! Oi! Oi! Ai!"));

            // -- Jhilpur e o Delta (31) --
            Page(entries, "enemy_forest_spear_heron",
                "The herons of the delta stand in the shallows as still as a post and spear whatever passes. The Darkness that came down the river made this one grey, and now it spears the light off the water and throws the shaft at whoever crosses the dikes. It is patient and it aims well. Keep moving on the open banks, and get under its beak; up close it can't throw.",
                "As garças do delta ficam paradas no raso feito um poste e espetam o que passa. A Treva que desceu o rio deixou esta cinzenta, e agora ela espeta a luz da água e joga a lança em quem atravessa os diques. É paciente e mira bem. Continuem andando nas margens abertas, e cheguem debaixo do bico dela; de perto ela não consegue jogar.",
                ("It throws from range. Close in under the beak.", "Joga de longe. Fecha embaixo do bico."),
                ("It's fishing for light. It's just hungry, like everyone here.", "Está pescando luz. Só tem fome, igual todo mundo aqui."),
                ("It stood so still I thought it was a stick. Sticks don't throw!", "Ficou tão parada que achei que era um galho. Galho não joga!"),
                ("Elegance and bad intentions. The delta has both in abundance.", "Elegância e más intenções. O delta tem as duas de sobra."),
                ("Her legs are so long! Longer than Joaquim's!", "As pernas dela são tão compridas! Mais que as do Joaquim!"));

            // -- Pontes de Raiz Viva (32) --
            Page(entries, "enemy_forest_mire_leech",
                "The leeches of the delta drink a little and let go; the root people even use them for fevers. The mire made these thirsty for light instead, and they never let go. They wait in the warm water under the lotus and rear up when you pass. They are slow on land. Don't let one fasten on, and if it does, Joaquim's water makes it drop off at once.",
                "As sanguessugas do delta bebem um pouco e soltam; o povo das raízes até usa elas pra febre. O lodo deixou estas com sede de luz, e elas nunca soltam. Esperam na água morna debaixo do lótus e se levantam quando vocês passam. Em terra são lentas. Não deixem nenhuma grudar, e se grudar, a água do Joaquim faz ela cair na hora.",
                ("Slow on land. Keep out of the water and it can't follow.", "Lenta em terra. Fica fora da água que ela não segue."),
                ("It only wants a little light. It just can't stop asking.", "Só quer um pouco de luz. Só não consegue parar de pedir."),
                ("It stuck to my arm. I didn't scream. Much.", "Grudou no meu braço. Eu não gritei. Muito."),
                ("A creature that is entirely appetite. I've met worse in libraries.", "Um bicho que é só apetite. Já conheci piores em biblioteca."),
                ("It's like a big wiggly sock! A sock with a mouth!", "Parece uma meia grande que mexe! Uma meia com boca!"));

            // -- Sangkent e os terraços (36) --
            Page(entries, "enemy_mountain_red_billed_chough",
                "The choughs of the terraces are black as a stove and have red bills and red feet, as if they had stepped in the dye. They always stole the apricots off the drying roofs, and the stair city let them. Now they steal the little lamps out of the windows and peck whoever comes for them back. They are quick and they come in a crowd. Swing wide, and don't chase them up the stairs.",
                "As gralhas dos terraços são pretas feito fogão e têm bico vermelho e pé vermelho, como se tivessem pisado na tinta. Sempre roubaram os damascos dos telhados de secar, e a cidade-escadaria deixava. Agora roubam as lamparinas das janelas e bicam quem vem buscar de volta. São rápidas e vêm em bando. Batam largo, e não corram atrás delas escada acima.",
                ("Fast and in a crowd. Hit wide, don't chase.", "Rápidas e em bando. Bate largo, não persegue."),
                ("They took the lamps. Maybe they just wanted to be warm.", "Levaram as lamparinas. Talvez só quisessem se esquentar."),
                ("One took my apricot. Right out of my hand!", "Uma pegou meu damasco. Da minha mão!"),
                ("Red boots and no manners. Very fashionable.", "Botas vermelhas e nenhum modo. Muito na moda."),
                ("Their beaks are like little red pencils!", "O bico delas parece um lapisinho vermelho!"));

            // -- Calhas de Pedra (37) --
            Page(entries, "enemy_mountain_bearded_vulture",
                "The bearded vulture eats bones. It carries them up and drops them on the stones of the chutes to crack them, and it has been doing it since before there were roads. The wind of ruin taught it to drop the stones instead, on whoever climbs. It flies high and has a good eye. When its shadow stops over you, move. Then it has to go and fetch another stone, and that is your moment.",
                "O abutre-barbudo come osso. Leva lá pra cima e solta nas pedras das calhas pra quebrar, e faz isso desde antes de existir estrada. O vento da ruína ensinou ele a soltar as pedras, em quem sobe. Voa alto e tem olho bom. Quando a sombra dele parar em cima de vocês, saiam. Aí ele precisa buscar outra pedra, e essa é a hora de vocês.",
                ("When its shadow stops on you, move. Then strike.", "Quando a sombra dele parar em você, sai. Depois bate."),
                ("He has a beard. He looks like a very tired grandfather.", "Ele tem barba. Parece um avô muito cansado."),
                ("It dropped a rock on my foot. On purpose!", "Ele soltou uma pedra no meu pé. De propósito!"),
                ("The mountain's oldest craftsman, retrained as artillery.", "O artesão mais velho da montanha, retreinado como artilharia."),
                ("He has a beard like Vô Jorge! A pointy one!", "Ele tem barba igual o Vô Jorge! Uma pontuda!"));

            // -- Geleira do Teto do Mundo (38) --
            Page(entries, "enemy_mountain_ice_wolf",
                "The glacier keeps a pack of wolves made of the snow that never melts. They do not eat. They run down whatever is warm and leave it cold, because the warmth is a little of the light and the Darkness wants it put out. They hunt in the crevasse fields and on the frozen peaks, never alone. Stay close together, keep a fire going when you rest, and strike the leader first.",
                "A geleira guarda uma matilha de lobos feitos da neve que nunca derrete. Eles não comem. Correm atrás do que é quente e deixam frio, porque o calor é um pouco da luz e a Treva quer ela apagada. Caçam nos campos de fendas e nos picos gelados, nunca sozinhos. Fiquem juntos, mantenham um fogo aceso quando descansarem, e batam primeiro no da frente.",
                ("They hunt in packs. Back to back, and hit the leader.", "Caçam em bando. Costas com costas, e bate no da frente."),
                ("They're made of the snow that never got to be water.", "São feitos da neve que nunca conseguiu virar água."),
                ("It breathed on me and my nose froze. My NOSE.", "Ele bafejou em mim e meu nariz congelou. Meu NARIZ."),
                ("Wolves with nothing inside but winter. Grim, but consistent.", "Lobos sem nada dentro além de inverno. Sombrio, mas coerente."),
                ("Their ears are icicles! Can I lick one? No? Okay.", "As orelhas deles são pingentes de gelo! Posso lamber? Não? Tá."));

            // -- Canal das Lanternas e a Eclusa da Lua (43) --
            Page(entries, "enemy_forest_dragon_carp",
                "The boatmen of the canal say a carp that leaps the falls becomes a dragon. This one leapt the lock of the moon on the night the light went out, and it stayed in the air, halfway, grey, with only a few scales of gold left. It does not know how to be a dragon and it cannot go back to being a fish, so it bites. It swims fast in the air above the water. Wait for it to turn, and hit the side.",
                "Os barqueiros do canal dizem que a carpa que pula a cachoeira vira dragão. Esta pulou a eclusa da lua na noite em que a luz apagou, e ficou no ar, no meio do caminho, cinzenta, só com umas escamas de ouro. Não sabe ser dragão e não consegue voltar a ser peixe, então morde. Nada rápido no ar em cima da água. Esperem ela virar, e batam no lado.",
                ("It swims fast in a line. Wait for the turn, then strike.", "Nada rápido em linha. Espera ela virar e bate."),
                ("It was almost a dragon. Almost is a hard place to live.", "Quase era um dragão. Quase é um lugar difícil de morar."),
                ("A fish bit me in the air. In the AIR.", "Um peixe me mordeu no ar. No AR."),
                ("Ambition without a finish line. I sympathise, frankly.", "Ambição sem linha de chegada. Sinceramente, eu entendo."),
                ("It has whiskers! A fish with whiskers! Like a cat fish!", "Ele tem bigode! Um peixe de bigode! Tipo um peixe-gato!"));

            // -- Muralha das Mil Torres e os Fossos de Terracota (45) --
            Page(entries, "enemy_forest_terracotta_horse",
                "The clay army of the moats was made to keep watch over a long sleep, soldiers and horses, every one with its own face. The Darkness woke the horses without their riders. They run the moats and the wall looking for someone to carry, and they trample whoever isn't the one. The cracks in them glow where the light got in. They charge in a straight line; step aside and strike the cracks.",
                "O exército de barro dos fossos foi feito pra vigiar um sono comprido, soldados e cavalos, cada um com a própria cara. A Treva acordou os cavalos sem os cavaleiros. Eles correm pelos fossos e pela muralha procurando alguém pra carregar, e pisoteiam quem não é. As rachaduras brilham onde a luz entrou. Atacam em linha reta; saiam de lado e batam nas rachaduras.",
                ("It charges straight. Sidestep and hit the cracks.", "Ataca reto. Sai de lado e bate nas rachaduras."),
                ("He's looking for his rider. He doesn't know the rider's still asleep.", "Ele está procurando o cavaleiro. Não sabe que o cavaleiro ainda dorme."),
                ("I tried to ride it. Do not try to ride it.", "Eu tentei montar. Não tentem montar."),
                ("A horse made to wait, now in a hurry. History is like that.", "Um cavalo feito pra esperar, agora com pressa. A história é assim."),
                ("He's a pottery horse! Vó has a little one on the shelf!", "Ele é um cavalo de barro! A Vó tem um pequenininho na estante!"));

            // -- Kurgans de Ouro e a estepe (48) --
            Page(entries, "enemy_mountain_felt_swan",
                "The steppe people sewed swans of white felt, with red and green cloth for the feathers, and hung them in the tents of their dead so the swans could carry them to the light. This one got up off the hanging. It still flies over the rivers and the salt lake the way it was sewn, stiff and very proud, and it throws its felt feathers like darts. Don't run from it in the open. Get close; it was made to carry, not to fight up close.",
                "O povo da estepe costurava cisnes de feltro branco, com pano vermelho e verde nas penas, e pendurava nas tendas dos mortos pra os cisnes levarem eles pra luz. Este levantou da tapeçaria. Ainda voa sobre os rios e o lago salgado do jeito que foi costurado, duro e muito orgulhoso, e joga as penas de feltro feito dardo. Não corram dele no descampado. Cheguem perto; ele foi feito pra levar, não pra brigar de perto.",
                ("It shoots from range. Close in; it's clumsy up close.", "Atira de longe. Fecha nele; de perto ele é desajeitado."),
                ("It was sewn to carry someone home. It just lost the way.", "Foi costurado pra levar alguém pra casa. Só perdeu o caminho."),
                ("A felt feather hit me and it STILL hurt.", "Uma pena de feltro me acertou e AINDA doeu."),
                ("Someone's love, stitched by hand, flying about without instructions.", "O amor de alguém, costurado à mão, voando por aí sem instruções."),
                ("It has stitches! Big ones! Vó, can you sew me one?", "Ele tem pontos! Grandões! Vó, você costura um pra mim?"));
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
