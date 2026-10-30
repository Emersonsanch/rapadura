using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Localization entries for the region side quests and their quest-givers (<see cref="RegionQuestCatalog"/>,
    /// <c>RegionNpcDialogues</c>): <c>quest.region_*</c> (title, description, objectives, journal),
    /// <c>dialogue.&lt;npc&gt;.*</c> for the four region NPCs, <c>dialogue.region_npc.*</c> (shared choice labels)
    /// and the world-map "Conversar com..." button (<c>ui.worldmap.talk*</c>). Same pattern as
    /// <c>QuestGiverTexts</c>/<c>DailyQuestTexts</c>: merged at runtime by <see cref="EnsureLoaded"/> (only keys
    /// not yet registered), by <c>DialogueManager</c>, <c>QuestLogDataSource</c> and the map talk button.
    /// </summary>
    public static class RegionQuestTexts
    {
        public const string TalkButtonKey = "ui.worldmap.talk";
        public const string TalkButtonNewsKey = "ui.worldmap.talk_news";

        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            // -- World map button --
            new LocalizationEntry(TalkButtonKey, "Talk to {0}", "Conversar com {0}"),
            new LocalizationEntry(TalkButtonNewsKey, "Talk to {0} (!)", "Conversar com {0} (!)"),

            // -- Shared choice labels --
            new LocalizationEntry("dialogue.region_npc.choice_accept", "Leave it to us!", "Pode deixar com a gente!"),
            new LocalizationEntry("dialogue.region_npc.choice_decline", "Not now, I'll come back later.", "Agora não, depois eu volto."),
            new LocalizationEntry("dialogue.region_npc.choice_progress", "About that favor...", "Sobre aquele pedido..."),
            new LocalizationEntry("dialogue.region_npc.choice_news", "Any news around here?", "Alguma novidade por aqui?"),
            new LocalizationEntry("dialogue.region_npc.choice_boss", "What do you know about the boss around here?", "O que você sabe sobre o chefe daqui?"),
            new LocalizationEntry("dialogue.region_npc.choice_cleared", "The boss around here is down!", "O chefe daqui caiu!"),
            new LocalizationEntry("dialogue.region_npc.choice_goodbye", "See you!", "Até mais!"),

            // ================= Floresta — Dona Cida, a Rezadeira =================
            new LocalizationEntry("quest.region_forest_howling_trail.title", "Howls on the Trail", "Uivos na Trilha"),
            new LocalizationEntry("quest.region_forest_howling_trail.description", "The scrub wolves lost their fear and prowl the creek trail. Dona Cida, the healer at the forest's edge, asked the family for help.", "Os lobos da mata perderam o medo e rondam a trilha do riacho. Dona Cida, a rezadeira da beira da floresta, pediu ajuda à família."),
            new LocalizationEntry("quest.region_forest_howling_trail.obj_hunt_scrub_wolves", "Drive off scrub wolves", "Espantar lobos da mata"),
            new LocalizationEntry("quest.region_forest_howling_trail.journal", "Dona Cida knew our grandparents. She says wolves smell fear, so we walk with our heads up — all five of us.", "Dona Cida conheceu nossos avós. Ela diz que lobo fareja medo, então a gente anda de cabeça erguida — os cinco."),
            new LocalizationEntry("quest.region_forest_spider_webs.title", "Webs in the Herb Garden", "Teias no Roçado"),
            new LocalizationEntry("quest.region_forest_spider_webs.description", "Forest spiders have webbed over Dona Cida's herb garden. No herbs means no tea and no medicine for anyone.", "Aranhas da floresta teceram teias no roçado de ervas de Dona Cida. Sem erva, não tem chá nem remédio pra ninguém."),
            new LocalizationEntry("quest.region_forest_spider_webs.obj_clear_forest_spiders", "Defeat forest spiders", "Derrotar aranhas da floresta"),
            new LocalizationEntry("quest.region_forest_spider_webs.journal", "Dona Cida's rule: whoever gets caught in a web calls their cousins. That's what family's for.", "Regra da Dona Cida: quem enrosca na teia chama os primos. Família é pra isso."),

            new LocalizationEntry("dialogue.forest_rezadeira.name", "Dona Cida, the Healer", "Dona Cida, a Rezadeira"),
            new LocalizationEntry("dialogue.forest_rezadeira.short_name", "Dona Cida", "Dona Cida"),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_maithe", "Maithe, dear! The eldest of the bunch, bow on your back and an eye on everyone. Sit a while — even protectors need a cup of tea.", "Maithe, minha filha! A mais velha da turma, de arco nas costas e olho em todo mundo. Senta um pouco, que até protetora precisa de um chá."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_maria", "Maria, light of the temple! Your grandma Zefa used to pray with me under this very mango tree. You calm people down just the way she does, you know?", "Maria, luz do templo! Sua avó Zefa rezava comigo debaixo desta mesma mangueira. Você acalma a gente do mesmo jeitinho dela, sabia?"),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_joaquim", "Joaquim, shield up as always. Fair warning: nobody here needs a wall, just someone to eat my cornmeal cake.", "Joaquim, de escudo em punho, como sempre. Já vou avisando: aqui ninguém precisa de muralha, só de alguém que coma meu bolo de fubá."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_icaro", "Ícaro! If that drone of yours scares my hens again, no coconut candy for you. ...Fine, have one.", "Ícaro! Se esse seu drone espantar minhas galinhas de novo, você fica sem cocada. ...Tá bom, pega uma."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_lavine", "Look at the little one! So small and already walking the woods on her own... Don't make that face, Lavine, I know you can handle it.", "Olha só a caçula! Pequenininha e já andando sozinha pela mata... Não faz essa cara, Lavine, eu sei que você dá conta."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_any", "Another one of the family! The woods are restless, dear. Sit down and I'll tell you.", "Chegou mais um da família! A mata anda agitada, meu bem. Senta, que eu te conto."),
            new LocalizationEntry("dialogue.forest_rezadeira.menu", "Anything else, dear?", "Mais alguma coisa, meu bem?"),
            new LocalizationEntry("dialogue.forest_rezadeira.choice_q1", "What's all that howling on the trail?", "Que uivos são esses na trilha?"),
            new LocalizationEntry("dialogue.forest_rezadeira.q1_offer", "The scrub wolves lost their fear and are prowling the creek trail. Nobody dares fetch water anymore. Will you drive off five of them? I'll give you some potions I brewed myself.", "Os lobos da mata perderam o medo e estão rondando a trilha do riacho. Ninguém mais vai buscar água. Espanta cinco deles pra mim? Te dou umas poções que eu mesma preparei."),
            new LocalizationEntry("dialogue.forest_rezadeira.choice_accept_special", "Leave it to me! I'm not a little kid anymore.", "Deixa comigo! Eu não sou mais criança."),
            new LocalizationEntry("dialogue.forest_rezadeira.q1_accepted", "Go with my blessing. And remember: wolves smell fear, so walk with your head up, like your whole family does.", "Vai com Deus. E lembra: lobo fareja medo, então anda de cabeça erguida, igual à sua família inteira."),
            new LocalizationEntry("dialogue.forest_rezadeira.q1_active", "I still hear howling by the creek. Five wolves, dear, then come back for some tea.", "Ainda escuto uivo perto do riacho. Cinco lobos, meu bem, e depois volta pra tomar um chá."),
            new LocalizationEntry("dialogue.forest_rezadeira.choice_q2", "Need any more help, Dona Cida?", "Precisa de mais ajuda, Dona Cida?"),
            new LocalizationEntry("dialogue.forest_rezadeira.q2_offer", "The trail is quiet again, thank you! But now the spiders have webbed over my herb garden. No herbs means no tea and no medicine for anyone's family. Will you take down four of them?", "A trilha está quieta de novo, obrigada! Mas agora as aranhas teceram teia no meu roçado de ervas. Sem erva, não tem chá nem remédio pra família de ninguém. Derruba quatro delas?"),
            new LocalizationEntry("dialogue.forest_rezadeira.q2_accepted", "Watch out for the sticky webs — whoever gets tangled calls their cousins. That's what family's for.", "Cuidado com a teia grudenta: quem enrosca chama os primos. Família é pra isso."),
            new LocalizationEntry("dialogue.forest_rezadeira.q2_active", "Four spiders, dear. They hide where the shade is coldest.", "Quatro aranhas, meu bem. Elas se escondem onde a sombra é mais fria."),
            new LocalizationEntry("dialogue.forest_rezadeira.all_done", "The herb garden is green again. Every time I brew tea I'll think of you five. Give your cousins a hug from me!", "O roçado está verdinho de novo. Toda vez que eu fizer chá, vou lembrar de vocês cinco. Manda um abraço pros primos!"),
            new LocalizationEntry("dialogue.forest_rezadeira.declined", "No rush. The woods will wait, and so will the tea.", "Sem pressa. A mata espera, e o chá também."),
            new LocalizationEntry("dialogue.forest_rezadeira.boss", "If you're facing the Alpha Wolf, listen to this old woman: he never attacks alone at first. Let the pack come, drop the small ones, and only then face the big one.", "Se vai enfrentar o Lobo Alfa, escuta esta velha: ele nunca ataca sozinho no começo. Deixa a matilha chegar, derruba os pequenos, e só então encara o grandão."),
            new LocalizationEntry("dialogue.forest_rezadeira.boss_special", "Maithe, you're the family's huntress, so you'll get this: the Alpha Wolf charges in a straight line when he's angry. Keep your distance, aim steady and don't let him corner you. Your cousins are counting on you.", "Maithe, você é a caçadora da família, então vai entender: o Lobo Alfa avança em linha reta quando fica bravo. Mantém distância, mira firme e não deixa ele te encurralar. Seus primos contam com você."),
            new LocalizationEntry("dialogue.forest_rezadeira.cleared", "The Alpha Wolf is down? Then the woods sleep in peace tonight! I made coconut candy to celebrate — take some to your cousins, okay?", "O Lobo Alfa caiu? Então hoje a mata dorme em paz! Fiz cocada pra comemorar — leva pros primos, viu?"),

            // -- forest_rezadeira after the boss (greet_after_*, RegionNpcDialogues: region cleared) --
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_maithe", "Maithe! Since the Alpha Wolf fell, the pack walks past here with their tails down. Sit — today the tea is on me. And tell your cousins on the radio, alright?", "Maithe! Desde que o Lobo Alfa caiu, a matilha passa aqui de rabo baixo. Senta, que hoje o chá é por minha conta. E conta pros primos pelo rádio, viu?"),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_maria", "Maria, my flower! The wolves are just wolves again. You were the one who said someone had put out his light, weren't you? Well, the woods are lit again.", "Maria, minha flor! Os lobos voltaram a ser só lobos. Foi você que disse que alguém tinha apagado a luz dele, né? Pois a mata acendeu de novo."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_joaquim", "Joaquim! The whole forest sleeps in peace since you beat the Alpha Wolf. Now you can lower that shield and eat a slice of cake, boy.", "Joaquim! A mata inteira dorme em paz desde que vocês venceram o Lobo Alfa. Agora pode baixar esse escudo e comer um pedaço de bolo, menino."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_icaro", "Ícaro! I heard you found golden sand on the Alpha Wolf's trail. You've already put some in a jar to study, I bet.", "Ícaro! Contaram que vocês acharam areia dourada na trilha do Lobo Alfa. Já guardou num potinho pra estudar, aposto."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_lavine", "Lavine, the little one who helped beat the Alpha Wolf! I told you you could handle it. The woods are calm as can be now.", "Lavine, a caçula que ajudou a vencer o Lobo Alfa! Eu bem que disse que você dava conta. A mata tá mansinha, mansinha."),
            new LocalizationEntry("dialogue.forest_rezadeira.greet_after_any", "The woods have been at peace since the Alpha Wolf fell. Sit down, dear, the tea is hot.", "A mata tá em paz desde que o Lobo Alfa caiu. Senta, meu bem, que o chá tá quente."),

            // ================= Deserto — Seu Bento, o Tropeiro =================
            new LocalizationEntry("quest.region_desert_stinger_road.title", "Stingers on the Road", "Ferrões no Caminho"),
            new LocalizationEntry("quest.region_desert_stinger_road.description", "Scorpions nested on the road to the well and Old Bento's caravan is stuck in the middle of the dunes.", "Escorpiões fizeram ninho no caminho do poço e a caravana de Seu Bento empacou no meio das dunas."),
            new LocalizationEntry("quest.region_desert_stinger_road.obj_chase_dune_scorpions", "Drive off dune scorpions", "Afastar escorpiões das dunas"),
            new LocalizationEntry("quest.region_desert_stinger_road.journal", "Old Bento says the sting comes from above and the desert doesn't forgive anyone who forgets to drink water. Noted, Bento.", "Seu Bento diz que o ferrão vem de cima e que deserto não perdoa quem esquece de beber água. Anotado, Seu Bento."),
            new LocalizationEntry("quest.region_desert_crossing_reserve.title", "Savings for the Crossing", "Reserva pra Travessia"),
            new LocalizationEntry("quest.region_desert_crossing_reserve.description", "Old Bento won't let anyone in the family head for the mountains without gold saved up — or with scorpions still circling the caravan.", "Seu Bento não deixa ninguém da família seguir pra montanha sem uma reserva de ouro — nem com escorpião rondando a caravana."),
            new LocalizationEntry("quest.region_desert_crossing_reserve.obj_guard_the_caravan", "Guard the caravan from scorpions", "Proteger a caravana dos escorpiões"),
            new LocalizationEntry("quest.region_desert_crossing_reserve.obj_save_crossing_gold", "Save gold for the crossing", "Juntar ouro para a travessia"),
            new LocalizationEntry("quest.region_desert_crossing_reserve.journal", "Saved gold means peace on the road, says the caravan driver. He sounds just like our parents.", "Ouro guardado é sossego na estrada, diz o tropeiro. Ele fala igualzinho aos nossos pais."),

            new LocalizationEntry("dialogue.desert_tropeiro.name", "Old Bento, the Caravan Driver", "Seu Bento, o Tropeiro"),
            new LocalizationEntry("dialogue.desert_tropeiro.short_name", "Old Bento", "Seu Bento"),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_maithe", "Maithe! The dune hunters talk about a girl who hits a scorpion at a hundred paces. Had to be the eldest cousin of that noisy family.", "Maithe! Os caçadores das dunas falam de uma moça que acerta escorpião a cem passos. Só podia ser a prima mais velha daquela família barulhenta."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_maria", "Miss Maria! Your blessing still guards my caravan — not one of my donkeys has fallen ill since you blessed them at Grandma Zefa's farm.", "Dona Maria! Sua bênção ainda protege minha caravana — nenhum jumento meu adoeceu desde que você benzeu eles lá no sítio da Vó Zefa."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_joaquim", "Joaquim, boy, get that shield out of the sun — it gets hot enough to fry an egg! Stubborn as your grandpa Chico, rest his soul — we drove mules together for years.", "Joaquim, menino, tira esse escudo do sol, que ele esquenta até fritar ovo! Teimoso igual ao teu avô Chico, Deus o tenha — tocamos tropa juntos muitos anos."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_icaro", "Ícaro, the inventor! That compass you fixed for me at the farm never lost north again. Want a look at the ruins? There's old machinery there that's right up your alley.", "Ícaro, o inventor! Aquela bússola que você me consertou lá no sítio nunca mais errou o norte. Quer dar uma olhada nas ruínas? Tem engenhoca velha lá que é a sua cara."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_lavine", "Lavine? The little one crossed the desert? ...Sorry, sorry. The great Lavine crossed the desert. Better?", "Lavine? A caçulinha atravessou o deserto? ...Desculpa, desculpa. A grande Lavine atravessou o deserto. Assim está melhor?"),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_any", "Well, hello! Another one of the family reaching the dunes. Drink some water first, talk later.", "Ô de casa! Mais um da família chegando nas dunas. Bebe água primeiro, prosa depois."),
            new LocalizationEntry("dialogue.desert_tropeiro.menu", "Anything else before the sun fries us?", "Mais alguma coisa antes do sol fritar a gente?"),
            new LocalizationEntry("dialogue.desert_tropeiro.choice_q1", "What's holding the caravan back?", "O que está segurando a caravana?"),
            new LocalizationEntry("dialogue.desert_tropeiro.q1_offer", "Scorpions, that's what. They nested right on the road to the well and my donkeys won't budge. Clear five of them off the road and I'll pay you with the best I've got: travel potions.", "Escorpião, é isso. Fizeram ninho bem no caminho do poço e meus jumentos empacaram. Afasta cinco deles da estrada e eu te pago com o que tenho de melhor: poção de viagem."),
            new LocalizationEntry("dialogue.desert_tropeiro.choice_accept_special", "Five scorpions? My drone will handle that before lunch!", "Cinco escorpiões? Meu drone resolve antes do almoço!"),
            new LocalizationEntry("dialogue.desert_tropeiro.q1_accepted", "Watch their tails — the sting comes from above. And drink water; the desert doesn't forgive the forgetful.", "Olho no rabo deles, que o ferrão vem de cima. E bebe água, que deserto não perdoa quem esquece."),
            new LocalizationEntry("dialogue.desert_tropeiro.q1_active", "The donkeys still won't move. There are scorpions left on the well road.", "Os jumentos ainda não saem do lugar. Tem escorpião sobrando no caminho do poço."),
            new LocalizationEntry("dialogue.desert_tropeiro.choice_q2", "What now, Old Bento?", "E agora, Seu Bento?"),
            new LocalizationEntry("dialogue.desert_tropeiro.q2_offer", "The road is open, thank you! Now listen to an old caravan driver: nobody crosses to the mountains without savings. Gather twelve gold for the crossing and chase three more scorpions away from the caravan. I won't see anyone from this family going without.", "A estrada abriu, obrigado! Agora escuta um velho tropeiro: ninguém atravessa pra montanha sem reserva. Junta doze de ouro pra travessia e espanta mais três escorpiões da caravana. Não quero ver ninguém dessa família passando aperto."),
            new LocalizationEntry("dialogue.desert_tropeiro.q2_accepted", "Saved gold means peace on the road. Your parents would be glad to know you're looking after each other.", "Ouro guardado é sossego na estrada. Seus pais iam gostar de saber que vocês estão se cuidando."),
            new LocalizationEntry("dialogue.desert_tropeiro.q2_active", "Twelve gold saved and three fewer scorpions. Gold shines in the sand — just keep your eyes open.", "Doze de ouro guardados e três escorpiões a menos. Ouro brilha na areia, é só prestar atenção."),
            new LocalizationEntry("dialogue.desert_tropeiro.all_done", "Savings set, caravan safe. When you reach the mountain, tell Master Tonho that Bento says hello. And share your water!", "Reserva feita, caravana segura. Quando chegarem na montanha, digam ao Mestre Tonho que o Bento mandou lembranças. E dividam a água!"),
            new LocalizationEntry("dialogue.desert_tropeiro.declined", "Fair enough. The desert teaches patience.", "Tá certo. O deserto ensina paciência."),
            new LocalizationEntry("dialogue.desert_tropeiro.boss", "The Ruin Guardian never sleeps and never tires. But he's made of old stone: when his core glows, get out of the way. Strike after the glow, never before.", "O Guardião da Ruína não dorme e não cansa. Mas ele é feito de pedra velha: quando o núcleo dele brilhar, sai da frente. Bate depois do brilho, nunca antes."),
            new LocalizationEntry("dialogue.desert_tropeiro.boss_special", "Ícaro, you know machines better than I do. The Guardian is an old-world contraption: its core glows right before it strikes. Dodge, let it cool down, and hit it hard. Maithe would be proud.", "Ícaro, você entende de máquina melhor que eu. O Guardião é engenhoca dos antigos: o núcleo brilha logo antes do golpe. Desvia, deixa ele esfriar e acerta em cheio. A Maithe ia ficar orgulhosa."),
            new LocalizationEntry("dialogue.desert_tropeiro.cleared", "The Guardian fell! Even the donkeys brayed for joy. The mountain road is yours.", "O Guardião caiu! Até os jumentos zurraram de alegria. A estrada da montanha é de vocês."),

            // -- desert_tropeiro after the boss (greet_after_*, RegionNpcDialogues: region cleared) --
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_maithe", "Maithe! Since the Ruin Guardian fell, my caravan passes the old stones singing. Is your brother still out there reading that panel?", "Maithe! Desde que o Guardião da Ruína caiu, minha caravana passa pelas pedras velhas cantando. Teu irmão ainda tá lá decifrando aquele painel?"),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_maria", "Miss Maria! The Ruin Guardian finally gets to rest. My donkeys and I sleep soundly again.", "Dona Maria! O Guardião da Ruína finalmente descansou. Meus jumentos e eu dormimos sossegados de novo."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_joaquim", "Joaquim! I heard the Guardian hit your shield and you didn't even blink. Your grandpa Chico would be laughing with pride, rest his soul.", "Joaquim! Ouvi dizer que o Guardião bateu no teu escudo e você nem piscou. Teu avô Chico ia rir de orgulho, Deus o tenha."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_icaro", "Ícaro! The Guardian fell and you still found a message from the ancients in its gears? That's going to be a caravan story, lad.", "Ícaro! O Guardião caiu e você ainda achou recado dos antigos nas engrenagens dele? Isso vai virar história de tropeiro, rapaz."),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_lavine", "The great Lavine! Crossed the desert and helped bring down the Ruin Guardian. Nobody calls you the little one anymore, hear?", "A grande Lavine! Atravessou o deserto e ainda ajudou a derrubar o Guardião da Ruína. Ninguém mais te chama de caçulinha, viu?"),
            new LocalizationEntry("dialogue.desert_tropeiro.greet_after_any", "The Ruin Guardian is resting, and the dune road is yours. Have some water and stay for a chat.", "O Guardião da Ruína descansou, e a estrada das dunas é de vocês. Bebe uma água e fica pra prosa."),

            // ================= Montanha — Mestre Tonho, o Mineiro =================
            new LocalizationEntry("quest.region_mountain_walking_stones.title", "Walking Stones", "Pedras que Andam"),
            new LocalizationEntry("quest.region_mountain_walking_stones.description", "Stone golems come down the slope and crush the rails of Master Tonho's mine.", "Golens de pedra descem a encosta e esmagam os trilhos da mina de Mestre Tonho."),
            new LocalizationEntry("quest.region_mountain_walking_stones.obj_topple_stone_golems", "Topple stone golems", "Derrubar golens de pedra"),
            new LocalizationEntry("quest.region_mountain_walking_stones.journal", "Master Tonho's tip: hit the joints, where stone cracks. And nobody charges ahead alone — that one was for Joaquim.", "Dica do Mestre Tonho: bater nas juntas, onde a pedra racha. E ninguém vai na frente sozinho — essa foi pro Joaquim."),
            new LocalizationEntry("quest.region_mountain_mine_pass.title", "The Mine Pass", "A Passagem da Mina"),
            new LocalizationEntry("quest.region_mountain_mine_pass.description", "The deep mine pass is overrun by golems. Once it's clear, the mine bell will ring again after years of silence.", "A passagem funda da mina está tomada por golens. Quando ela for liberada, o sino da mina volta a tocar depois de anos de silêncio."),
            new LocalizationEntry("quest.region_mountain_mine_pass.obj_open_the_mine_pass", "Clear the deep mine pass", "Liberar a passagem funda da mina"),
            new LocalizationEntry("quest.region_mountain_mine_pass.journal", "Master Tonho promised to tell his grandkids about the five cousins who made the mountain sing.", "Mestre Tonho prometeu contar pros netos a história dos cinco primos que fizeram a montanha cantar."),

            new LocalizationEntry("dialogue.mountain_miner.name", "Master Tonho, the Miner", "Mestre Tonho, o Mineiro"),
            new LocalizationEntry("dialogue.mountain_miner.short_name", "Master Tonho", "Mestre Tonho"),
            new LocalizationEntry("dialogue.mountain_miner.greet_maithe", "Maithe! From up here I saw an arrow cross the whole valley and thought: that has to be the eldest of those cousins.", "Maithe! Lá de cima eu vi uma flecha atravessar o vale inteiro e pensei: só pode ser a mais velha daqueles primos."),
            new LocalizationEntry("dialogue.mountain_miner.greet_maria", "Maria, good to see you. Up here the light comes in weak; you bringing yours warms the whole mine.", "Maria, que bom te ver. Aqui em cima a luz chega fraquinha; você trazendo a sua já esquenta a mina inteira."),
            new LocalizationEntry("dialogue.mountain_miner.greet_joaquim", "Joaquim! I heard you held back an avalanche with your shield. That's a tall tale, right? ...It isn't? This boy!", "Joaquim! Ouvi dizer que você segurou uma avalanche com o escudo. Conversa, né? ...Não é conversa? Esse menino!"),
            new LocalizationEntry("dialogue.mountain_miner.greet_icaro", "Ícaro, my boy! Did you bring the electric pickaxe you promised in that note you sent with Old Bento? The golems here would get quite a scare.", "Ícaro, rapaz! Trouxe a picareta elétrica que prometeu no recado que mandou pelo Seu Bento? Os golens daqui iam levar um susto."),
            new LocalizationEntry("dialogue.mountain_miner.greet_lavine", "Lavine, you climbed the whole mountain? At your age I was scared of the mine ladder. You're braver than plenty of bearded miners.", "Lavine, você subiu a montanha inteira? Eu, na sua idade, tinha medo até da escada da mina. Você é mais corajosa que muito mineiro barbado."),
            new LocalizationEntry("dialogue.mountain_miner.greet_any", "Hey, a visitor! Someone from the cousins' family climbs the mountain and old Tonho puts the coffee on.", "Ê, visita! Gente da família dos primos sobe a montanha e o velho Tonho já põe o café no fogo."),
            new LocalizationEntry("dialogue.mountain_miner.menu", "Anything else to chat about?", "Mais alguma prosa?"),
            new LocalizationEntry("dialogue.mountain_miner.choice_q1", "Why is the mine closed?", "Por que a mina está parada?"),
            new LocalizationEntry("dialogue.mountain_miner.q1_offer", "The stones decided to walk! Stone golems come down the slope and crush the cart rails. Topple four of them and I'll pay you in gold and potions — a miner's word.", "As pedras resolveram andar! Golens de pedra descem a encosta e esmagam os trilhos dos carrinhos. Derruba quatro deles e eu te pago em ouro e poção, palavra de mineiro."),
            new LocalizationEntry("dialogue.mountain_miner.choice_accept_special", "Stone against stone? Let me hold the line.", "Pedra contra pedra? Deixa que eu seguro a linha."),
            new LocalizationEntry("dialogue.mountain_miner.q1_accepted", "Hit the joints, that's where stone cracks. And don't charge ahead of everyone, alright? The mountain likes to punish the hasty.", "Bate nas juntas, que é onde a pedra racha. E não vai na frente de todo mundo, viu? Montanha gosta de cobrar dos apressados."),
            new LocalizationEntry("dialogue.mountain_miner.q1_active", "I still hear golems stomping up there. Just a few left, keep at it!", "Ainda escuto golem pisando lá em cima. Faltam poucos, coragem!"),
            new LocalizationEntry("dialogue.mountain_miner.choice_q2", "Can the mine reopen now?", "A mina já pode reabrir?"),
            new LocalizationEntry("dialogue.mountain_miner.q2_offer", "Almost! The rails are safe, but the deep mine pass is full of golems. Topple six and the mine bell rings again. It hasn't rung in years.", "Quase! Os trilhos estão salvos, mas a passagem funda da mina está cheia de golem. Derruba seis e o sino da mina volta a tocar. Faz anos que ele não toca."),
            new LocalizationEntry("dialogue.mountain_miner.q2_accepted", "If the echo gets too loud, step out and breathe. None of your cousins want to dig you out from under a rock.", "Se o eco ficar alto demais, sai e respira. Nenhum primo seu quer te tirar de debaixo de pedra."),
            new LocalizationEntry("dialogue.mountain_miner.q2_active", "Six golems in the deep pass. When the last one falls, you'll hear the bell.", "Seis golens na passagem funda. Quando o último cair, você vai ouvir o sino."),
            new LocalizationEntry("dialogue.mountain_miner.all_done", "Hear that? It's the mine bell! I'll tell my grandkids about this: the five cousins who made the mountain sing again.", "Escutou? É o sino da mina! Vou contar essa pros meus netos: os cinco primos que fizeram a montanha cantar de novo."),
            new LocalizationEntry("dialogue.mountain_miner.declined", "No problem. Stone has patience to spare.", "Sem problema. Pedra tem paciência de sobra."),
            new LocalizationEntry("dialogue.mountain_miner.boss", "The Mountain Warlord commands the golems from the ridge. He hits hard and wide: whoever stands in front of him doesn't get up. Move sideways, strike from behind, and don't give up.", "O Senhor da Guerra manda nos golens lá da crista. Ele bate forte e bate largo: quem fica parado na frente dele não levanta. Anda de lado, ataca pelas costas e não desiste."),
            new LocalizationEntry("dialogue.mountain_miner.boss_special", "Joaquim, I know you'll want to stand in front to protect everyone. Against the Warlord, protecting means knowing when to step aside. Shield up, quick feet, and come back in one piece so Maria doesn't pull my ear.", "Joaquim, eu sei que você vai querer ficar na frente pra proteger todo mundo. Contra o Senhor da Guerra, proteger é saber sair do caminho na hora certa. Escudo erguido, pé ligeiro, e volta inteiro pra Maria não me puxar a orelha."),
            new LocalizationEntry("dialogue.mountain_miner.cleared", "The Warlord has fallen! The whole mountain sighed with relief. Head down to the caves carefully — down there is another world.", "O Senhor da Guerra caiu! A montanha inteira suspirou de alívio. Desçam pras cavernas com cuidado — lá embaixo é outro mundo."),

            // -- mountain_miner after the boss (greet_after_*, RegionNpcDialogues: region cleared) --
            new LocalizationEntry("dialogue.mountain_miner.greet_after_maithe", "Maithe! Since the Warlord fell, the mountain wind has stopped shouting. Looks like it finally listened to you.", "Maithe! Depois que o Senhor da Guerra caiu, o vento da serra parou de gritar. Parece que ele finalmente te escutou."),
            new LocalizationEntry("dialogue.mountain_miner.greet_after_maria", "Maria! They say the Warlord lowered his sword and thanked you all. Turns out people who shout a lot really are just afraid.", "Maria! Dizem que o Senhor da Guerra baixou a espada e agradeceu vocês. Quem grita muito é porque tá com medo, né?"),
            new LocalizationEntry("dialogue.mountain_miner.greet_after_joaquim", "Joaquim! I heard that against the Warlord you stepped aside at just the right moment. See? That's protecting too. Maria won't be pulling my ear after all.", "Joaquim! Soube que contra o Senhor da Guerra você saiu da frente na hora certa. Viu só? Proteger também é isso. A Maria não vai puxar minha orelha, não."),
            new LocalizationEntry("dialogue.mountain_miner.greet_after_icaro", "Ícaro! You found the weak spot in the joints of the Warlord's armour, didn't you? I told you: hit the joints, that's where it cracks.", "Ícaro! Achou o ponto fraco nas juntas da armadura do Senhor da Guerra, né? Eu bem que disse: bate nas juntas, que é onde racha."),
            new LocalizationEntry("dialogue.mountain_miner.greet_after_lavine", "Lavine! The girl who climbed the mountain and stood up to the Warlord. Not one bearded miner can say the same.", "Lavine! A menina que subiu a montanha e encarou o Senhor da Guerra. Mineiro barbado nenhum pode dizer o mesmo."),
            new LocalizationEntry("dialogue.mountain_miner.greet_after_any", "The Warlord remembered who he was and the mountain sighed with relief. Sit, the coffee is brewing.", "O Senhor da Guerra lembrou quem era e a serra suspirou de alívio. Senta, que o café tá passando."),

            // ================= Cavernas — Benedita das Lamparinas =================
            new LocalizationEntry("quest.region_caves_dark_lamps.title", "Snuffed Lanterns", "Lamparinas Apagadas"),
            new LocalizationEntry("quest.region_caves_dark_lamps.description", "Bat swarms blow out Benedita's lanterns, and in the dark no one can find the way out of the caves.", "Enxames de morcegos apagam as lamparinas de Benedita, e no escuro ninguém acha a saída das cavernas."),
            new LocalizationEntry("quest.region_caves_dark_lamps.obj_scatter_bat_swarms", "Scatter bat swarms", "Espantar enxames de morcegos"),
            new LocalizationEntry("quest.region_caves_dark_lamps.journal", "Bats hate bright light and noise. Maria has plenty of light; Ícaro and Joaquim have plenty of noise.", "Morcego odeia luz forte e barulho. Luz, a Maria tem de sobra; barulho, o Ícaro e o Joaquim."),
            new LocalizationEntry("quest.region_caves_last_lantern.title", "The Last Lantern", "A Última Lamparina"),
            new LocalizationEntry("quest.region_caves_last_lantern.description", "In the deepest tunnel stands the oldest lantern in the caves, and the bats won't let anyone near it.", "No túnel mais fundo fica a lamparina mais antiga das cavernas, e os morcegos não deixam ninguém chegar perto."),
            new LocalizationEntry("quest.region_caves_last_lantern.obj_light_the_deep_tunnel", "Clear the way to the last lantern", "Abrir caminho até a última lamparina"),
            new LocalizationEntry("quest.region_caves_last_lantern.journal", "Benedita will light the last lantern in the names of the five of us. As long as it shines, the family won't get lost.", "Benedita vai acender a última lamparina com o nome de nós cinco. Enquanto ela brilhar, a família não se perde."),

            new LocalizationEntry("dialogue.caves_lampkeeper.name", "Benedita of the Lanterns", "Benedita das Lamparinas"),
            new LocalizationEntry("dialogue.caves_lampkeeper.short_name", "Benedita", "Benedita"),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_maithe", "Maithe. Your hunter's eye is good in the dark, but here the dark looks back. Stay close to the light and to your own.", "Maithe. Seu olho de caçadora é bom no escuro, mas aqui o escuro olha de volta. Fica perto da luz e dos seus."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_maria", "Maria! The lanterns flickered when you came in, as if they recognized your light. Stay a while; they like you.", "Maria! As lamparinas tremeram quando você entrou, como se reconhecessem a sua luz. Fica um pouco; elas gostam de você."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_joaquim", "Joaquim, lower that shield — the ceiling's low down here and the echo is treacherous. Protectors can rest too, you know?", "Joaquim, abaixa esse escudo, que aqui embaixo o teto é baixo e o eco é traiçoeiro. Protetor também pode descansar, sabia?"),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_icaro", "Ícaro, the pocket lanterns you left with Master Tonho already saved two miners down here. I still prefer my oil, but... thank you.", "Ícaro, as lanternas de bolso que você deixou com o Mestre Tonho já salvaram dois mineiros aqui embaixo. Eu ainda prefiro meu óleo, mas... obrigada."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_lavine", "Lavine. The shadows here grow quiet when you pass... Don't fear what you carry, child. In your hands, the night protects.", "Lavine. As sombras daqui se aquietam quando você passa... Não tenha medo do que você carrega, menina. Na sua mão, a noite protege."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_any", "Another cousin came all the way down? Take a lantern; the path is long and cold.", "Mais um dos primos desceu até aqui? Pega uma lamparina, que o caminho é longo e frio."),
            new LocalizationEntry("dialogue.caves_lampkeeper.menu", "Anything else before you head into the dark?", "Mais alguma coisa antes de seguir no escuro?"),
            new LocalizationEntry("dialogue.caves_lampkeeper.choice_q1", "Why are the lanterns out?", "Por que as lamparinas estão apagadas?"),
            new LocalizationEntry("dialogue.caves_lampkeeper.q1_offer", "The bat swarms. They beat their wings so hard they blow out my lanterns, and in the dark nobody finds the way out. Scatter six swarms and I'll give you the potions I keep for emergencies.", "Os enxames de morcegos. Batem as asas tão forte que apagam minhas lamparinas, e no escuro ninguém acha a saída. Espanta seis enxames e eu te dou as poções que guardo pra emergência."),
            new LocalizationEntry("dialogue.caves_lampkeeper.choice_accept_special", "I'll carry the light with me. You can count on us.", "Eu levo a luz comigo. Pode contar com a gente."),
            new LocalizationEntry("dialogue.caves_lampkeeper.q1_accepted", "Bats hate bright light and noise. Make both, and call your cousins if you need to.", "Morcego odeia luz forte e barulho. Faz os dois, e chama seus primos se precisar."),
            new LocalizationEntry("dialogue.caves_lampkeeper.q1_active", "There are still swarms flapping in the tunnels. Follow the screeching.", "Ainda tem enxame batendo asa nos túneis. Segue o chiado."),
            new LocalizationEntry("dialogue.caves_lampkeeper.choice_q2", "What about the deepest tunnel?", "E o túnel mais fundo?"),
            new LocalizationEntry("dialogue.caves_lampkeeper.q2_offer", "The entrance lanterns shine again, thanks to you. But in the deep tunnel there's the last lantern, the oldest one, and the bats won't let anyone near. Scatter eight more swarms and I'll light it for your family.", "As lamparinas da entrada voltaram a brilhar, graças a você. Mas no túnel fundo ainda tem a última lamparina, a mais antiga, e os morcegos não deixam ninguém chegar. Espanta mais oito enxames e eu acendo ela pra sua família."),
            new LocalizationEntry("dialogue.caves_lampkeeper.q2_accepted", "Go slowly. At the bottom of the caves, silence speaks louder than any scream.", "Vai devagar. No fundo da caverna, o silêncio fala mais alto que qualquer grito."),
            new LocalizationEntry("dialogue.caves_lampkeeper.q2_active", "Eight swarms in the deep tunnel. The last lantern is waiting for you.", "Oito enxames no túnel fundo. A última lamparina espera por vocês."),
            new LocalizationEntry("dialogue.caves_lampkeeper.all_done", "I lit the last lantern in the names of you five. As long as it shines, this family will never get lost in the dark.", "Acendi a última lamparina com o nome de vocês cinco. Enquanto ela brilhar, essa família nunca vai se perder no escuro."),
            new LocalizationEntry("dialogue.caves_lampkeeper.declined", "That's alright. The light waits for those who come back.", "Tudo bem. A luz espera quem volta."),
            new LocalizationEntry("dialogue.caves_lampkeeper.boss", "The Ancient Dweller lives where no lantern reaches. He sees by sound: when he stops to listen, stay still. Then strike with everything.", "O Habitante Ancestral vive onde nenhuma lamparina chega. Ele enxerga pelo som: quando ele parar pra escutar, fica quieto. Depois ataca com tudo."),
            new LocalizationEntry("dialogue.caves_lampkeeper.boss_special", "Lavine, the Dweller will sense the night in you and try to frighten you. Don't let him. You're the youngest, but not the weakest: your shadow shields your family. Show him.", "Lavine, o Habitante vai sentir a noite em você e tentar te assustar. Não deixa. Você é a caçula, mas não é a mais fraca: a sua sombra é escudo da sua família. Mostra pra ele."),
            new LocalizationEntry("dialogue.caves_lampkeeper.cleared", "The Ancient Dweller is gone... The depths are silent — a good silence. You five brought light where it had never reached.", "O Habitante Ancestral se foi... O fundo da caverna está em silêncio, um silêncio bom. Vocês cinco levaram luz aonde ela nunca tinha chegado."),

            // -- caves_lampkeeper after the boss (greet_after_*, RegionNpcDialogues: region cleared) --
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_maithe", "Maithe. The Dweller doesn't steal any more light... he even comes to borrow a lantern, can you believe it? Did you count your cousins? All there?", "Maithe. O Habitante não rouba mais luz nenhuma... ele até vem pedir lamparina emprestada, acredita? Contou os primos? Estão todos?"),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_maria", "Maria! I heard you lit your light slowly for him. Since then the lanterns down here have never flickered again.", "Maria! Soube que você acendeu a sua luz devagarinho pra ele. Desde então as lamparinas daqui nunca mais tremeram."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_joaquim", "Joaquim, you can lower that shield for good: down at the bottom there's only a neighbour who's afraid of the dark — with a brand-new lantern.", "Joaquim, pode baixar o escudo de vez: lá no fundo agora só mora um vizinho com medo do escuro — e com lamparina nova."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_icaro", "Ícaro, the Dweller asked for one of your pocket lanterns. I said I'd ask you. An inventor has customers even at the bottom of a cave!", "Ícaro, o Habitante pediu uma das suas lanternas de bolso. Eu disse que ia te perguntar. Inventor tem freguês até no fundo da caverna!"),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_lavine", "Lavine. You showed him a shadow can be soft too. Now the shadows down here greet you when you pass.", "Lavine. Você mostrou pra ele que a sombra também pode ser macia. Agora as sombras daqui te cumprimentam quando você passa."),
            new LocalizationEntry("dialogue.caves_lampkeeper.greet_after_any", "The depths are at peace and the light went home to the village. Take a lantern anyway — an old woman's habit.", "O fundo da caverna está em paz, e a luz voltou pra vila. Pega uma lamparina mesmo assim — mania de velha."),
        };

        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }

        /// <summary>pt-BR text of <paramref name="key"/> without a manager (same no-manager fallback rule as the other helpers), or null.</summary>
        public static string GetFallback(string key)
        {
            LocalizationEntry entry = All.FirstOrDefault(e => e.Key == key);
            return entry?.PtBR;
        }
    }
}
