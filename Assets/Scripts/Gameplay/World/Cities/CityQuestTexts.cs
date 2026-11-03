using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Localization (en + pt-BR) of the city side quests (<see cref="CityQuestCatalog"/>): title, description, objectives
    /// and journal (<c>quest.city_*</c>, the same shape as the region quests) and the giver's offer/progress lines. Merged
    /// into the live manager by <see cref="EnsureLoaded"/> (keys it already has win), like <c>RegionQuestTexts</c>.
    /// </summary>
    public static class CityQuestTexts
    {
        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            // ================= Copas de Garajonay — Gara, guardiã das copas =================
            new LocalizationEntry("quest.city_garajonay_dry_mist.title", "The Mist That Forgot the Water", "A Névoa Que Esqueceu a Água"),
            new LocalizationEntry("quest.city_garajonay_dry_mist.description",
                "Since the shadow drank the mist, the laurel lizards of the forest floor run wild and dry. Gara, the canopy warden of Copas de Garajonay, asked the cousins to calm them before they climb the trunks.",
                "Desde que a sombra bebeu a névoa, os lagartos do chão da mata de louro andam bravos e secos. Gara, a guardiã das copas de Garajonay, pediu aos primos que os acalmem antes que subam pelos troncos."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.obj_calm_mist_lizards", "Calm the wild beasts of the laurel floor", "Acalmar os bichos bravos do chão da mata de louro"),
            new LocalizationEntry("quest.city_garajonay_dry_mist.journal",
                "Gara says the laurels drink the cloud and the cloud remembers everything. Grandpa Jorge's tales said the same about the rain that comes sideways. Five of us, one mist.",
                "A Gara diz que o louro bebe a nuvem e a nuvem lembra de tudo. Os contos do Vô Jorge falavam a mesma coisa da chuva que vem de lado. Somos cinco, a névoa é uma só."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.offer",
                "Grandma Edna's grandchildren, at last! The mist forgot the water and the forest floor went wild. Calm six of the beasts down there and the bridges will sing for you.",
                "Os netos da Vó Edna, enfim! A névoa esqueceu a água e o chão da mata ficou bravo. Acalmem seis bichos lá embaixo e as pontes vão cantar para vocês."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.active",
                "The mist is still dry down there. Walk in pairs on the trails: whoever falls from a bridge comes back the long way.",
                "A névoa ainda está seca lá embaixo. Andem em dupla pelas trilhas: quem cai da ponte volta pelo caminho comprido."),

            // ================= Copas de Garajonay — Tinguaro, o rastreador =================
            new LocalizationEntry("quest.city_garajonay_moss_tracks.title", "Tracks in the Moss", "Rastros no Musgo"),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.description",
                "Moss boars dimmed by the shadow are digging up the roots of the giant laurels. Tinguaro, the elves' tracker, read their tracks and needs the cousins to chase them off.",
                "Javalis de musgo apagados pela sombra estão revirando as raízes dos louros gigantes. Tinguaro, o rastreador dos elfos, leu os rastros e precisa dos primos para espantá-los."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.obj_follow_moss_boars", "Chase off the moss boars", "Espantar javalis de musgo"),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.journal",
                "Tinguaro reads moss the way Ícaro reads gears. He says a track is a letter the animal forgot to send. Maithe wants to learn it all.",
                "O Tinguaro lê musgo do jeito que o Ícaro lê engrenagem. Ele diz que rastro é carta que o bicho esqueceu de mandar. A Maithe quer aprender tudo."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.offer",
                "Gara told me you calmed the floor. Now the moss boars are digging at the laurel roots. Five of them, and I'll give you one of Jonay's bows.",
                "A Gara me contou que vocês acalmaram o chão. Agora os javalis de musgo estão cavando as raízes dos louros. Cinco deles, e eu te dou um dos arcos do Jonay."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.active",
                "Follow the turned moss, not the noise. A boar that is running is not where the noise is.",
                "Sigam o musgo revirado, não o barulho. Javali que corre não está onde o barulho está."),

            // ================= Taa Pwani — Capitão Liyongo, o nakhoda =================
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.title", "Thieves of the Sails", "Ladras de Vela"),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.description",
                "Gulls dimmed by the shadow swoop on the dhows of Taa Pwani, tear the triangular sails and steal the rope. Captain Liyongo, the nakhoda of the harbour, cannot sail for the reef until they are driven off.",
                "Gaivotas apagadas pela sombra mergulham nos dhows de Taa Pwani, rasgam as velas triangulares e roubam a corda. O Capitão Liyongo, o nakhoda do porto, não consegue sair para o recife enquanto elas não forem espantadas."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.obj_drive_off_thief_gulls", "Drive off the thief gulls", "Espantar as gaivotas ladras"),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.journal",
                "Captain Liyongo says the monsoon pushes one way half the year and back the other half. Grandpa Jorge sailed with these people once: Mbaraka still remembers his off-key songs on deck.",
                "O Capitão Liyongo diz que a monção empurra para um lado meio ano e volta no outro meio. O Vô Jorge já navegou com esse povo: o Mbaraka ainda lembra das cantorias desafinadas dele no convés."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.offer",
                "Grandma Edna's five, on my quay! Karibu. The gulls went grey and mean, they tear my sails faster than we can sew. Drive six of them off and the first wind of the monsoon is yours.",
                "Os cinco da Vó Edna, no meu cais! Karibu. As gaivotas ficaram cinzentas e malvadas, rasgam as velas mais rápido do que a gente costura. Espantem seis delas e o primeiro vento da monção é de vocês."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.active",
                "Still gulls over the reef. Watch the sky at low tide: they dive when the sails are drying.",
                "Ainda tem gaivota em cima do recife. Olhem o céu na maré baixa: elas mergulham quando as velas estão secando."),

            // ================= Taa Pwani — Zawadi, o trançador de raízes =================
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.title", "Claws in the Mangrove", "Garras no Mangue"),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.description",
                "Giant crabs gone dim are cutting the living roots of the mangroves Zawadi braids his boots from. Without the roots, nobody walks the mud of Liyongo's mangroves.",
                "Caranguejos gigantes apagados estão cortando as raízes vivas dos mangues de onde o Zawadi trança as botas. Sem as raízes, ninguém anda no lodo dos mangues de Liyongo."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.obj_cut_free_the_roots", "Free the roots from the giant claws", "Livrar as raízes das garras gigantes"),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.journal",
                "Zawadi only braids the roots that fall on their own. He says the mangrove gives what it can spare, like Grandma Edna's orchard. Joaquim doesn't sink in the mud, but he wants a pair anyway.",
                "O Zawadi só trança as raízes que caem sozinhas. Ele diz que o mangue dá o que pode, igual ao pomar da Vó Edna. O Joaquim não afunda no lodo, mas quer um par mesmo assim."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.offer",
                "The captain says you can handle gulls. Can you handle claws? Five giant crabs are cutting my roots. Free them and the first pair of root boots of the tide is yours.",
                "O capitão diz que vocês dão conta de gaivota. E de garra? Cinco caranguejos gigantes estão cortando as minhas raízes. Livrem elas e o primeiro par de botas de raiz da maré é de vocês."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.active",
                "Pole pole: a crab backs off sideways. Wait for it to open its claws, then strike.",
                "Pole pole: caranguejo recua de lado. Espera ele abrir a garra, aí bate."),

            // ================= Tambor Rubro — Gurparanzah, a chefe-arqueira =================
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.title", "Red Tails on the Mesa", "Caudas Vermelhas na Mesa"),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.description",
                "Scorpions gone dim by the shadow nest in the cracks of the badlands and sting the clans' wolves on the way to the forge. Gurparanzah, the archer chief of the Tambor Rubro, wants the road to the canyons clear before the next drum.",
                "Escorpiões apagados pela sombra fazem ninho nas fendas dos ermos e picam os lobos dos clãs no caminho da forja. A Gurparanzah, chefe-arqueira do Tambor Rubro, quer a estrada dos cânions limpa antes do próximo tambor."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.obj_clear_red_scorpions", "Clear the red scorpions from the canyon road", "Limpar os escorpiões rubros da estrada dos cânions"),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.journal",
                "Gurparanzah says an orc's word weighs more than an orc's axe. Grandpa Jorge once gave her his word he'd bring a sack of Grandma Edna's apricots, and he did, through a sandstorm.",
                "A Gurparanzah diz que palavra de orc pesa mais que machado de orc. O Vô Jorge uma vez deu a palavra de trazer um saco de damascos da Vó Edna, e trouxe, no meio de uma tempestade de areia."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.offer",
                "The grandchildren of Jorge and Edna, inside my palisade! Then you keep your word like he did. Six red scorpions are nesting on the canyon road. Clear them and the forge drum will play your names tonight.",
                "Os netos do Jorge e da Edna, dentro da minha paliçada! Então vocês cumprem a palavra como ele cumpria. Seis escorpiões rubros fizeram ninho na estrada dos cânions. Limpem e o tambor da forja toca o nome de vocês hoje à noite."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.active",
                "Still tails on the road. They hide in the shade of the chimneys of stone at noon: look there first.",
                "Ainda tem cauda na estrada. Ao meio-dia eles se escondem na sombra das chaminés de pedra: olhem lá primeiro."),

            // ================= Tambor Rubro — Kessi, o caçador =================
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.title", "Hawks of Ember", "Gaviões de Brasa"),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.description",
                "Hawks gone dim dive on the camp's hide racks and carry off the meat the drums were going to share. Kessi, the hunter of the Tambor Rubro, never hunts more than the camp eats: five of them, and not one more.",
                "Gaviões apagados mergulham nos varais de couro do acampamento e levam a carne que os tambores iam repartir. O Kessi, caçador do Tambor Rubro, nunca caça mais do que o acampamento come: cinco deles, nem um a mais."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.obj_hunt_ember_hawks", "Hunt the ember hawks", "Caçar os gaviões de brasa"),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.journal",
                "Kessi hunts with the drum's rhythm: three beats to aim, one to loose. Maithe says she'll try it. Lavine says the hawks were only hungry, and Kessi agrees with her.",
                "O Kessi caça no ritmo do tambor: três batidas para mirar, uma para soltar. A Maithe diz que vai tentar. A Lavine diz que os gaviões só estavam com fome, e o Kessi concorda com ela."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.offer",
                "Gurparanzah says you keep your word. Good: so do I. Five ember hawks are robbing the racks. Bring them down and take this theriac: the Throne of Namrael is all poison, you'll need it.",
                "A Gurparanzah diz que vocês cumprem a palavra. Bom: eu também. Cinco gaviões de brasa estão roubando os varais. Derrubem eles e levem esta teriaga: o Trono de Namrael é só veneno, vocês vão precisar."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.active",
                "Watch the sky over the canyons. A hawk that is circling is still deciding; strike when it dives.",
                "Olhem o céu em cima dos cânions. Gavião que está rodando ainda está decidindo; acertem quando ele mergulhar."),

            // ================= Salões de Bronze — Untash, o mestre de obras =================
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.title", "Slag on the Stairs", "Escória na Escada"),
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.description",
                "Since the Grande Forja went cold, badly cast golems of slag wander the stairs between the halls and the mines, and the dwarves can no longer carry ore down. Untash, master builder of the Bronze Halls, wants the stairs clear before he starts on the new bellows.",
                "Desde que a Grande Forja apagou, golens mal fundidos de escória andam pelas escadas entre os salões e as minas, e os anões não conseguem mais descer o minério. O Untash, mestre de obras dos Salões de Bronze, quer as escadas livres antes de começar o fole novo."),
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.obj_break_slag_golems", "Break the slag golems on the mine stairs", "Quebrar os golens de escória da escada das minas"),
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.journal",
                "Untash measures everything twice and builds once. He says Grandpa Jorge once fixed a hinge here with a piece of string, and that the hinge still holds, which annoys him very much.",
                "O Untash mede tudo duas vezes e constrói uma. Ele diz que o Vô Jorge uma vez consertou uma dobradiça daqui com um pedaço de barbante, e que a dobradiça segura até hoje, o que irrita ele muito."),
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.offer",
                "Jorge's grandchildren! Good. I need hands that don't ask how long it will take. Five slag golems are blocking the mine stairs: bad castings, poor things, walking without anyone telling them where. Break them, and bring back any bronze you find.",
                "Os netos do Jorge! Bom. Preciso de mãos que não perguntam quanto tempo vai levar. Cinco golens de escória estão trancando a escada das minas: fundição ruim, coitados, andando sem ninguém dizer para onde. Quebrem eles, e tragam o bronze que acharem."),
            new LocalizationEntry("quest.city_saloes_de_bronze_slag_golems.active",
                "A slag golem is slow but it doesn't stop. Hit the cracks where the casting cooled wrong.",
                "Golem de escória é lento, mas não para. Batam nas rachaduras onde a fundição esfriou errado."),

            // ================= Salões de Bronze — Shilhak, o minerador =================
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.title", "Bats in the Seam", "Morcegos no Veio"),
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.description",
                "Bats that nested in the last warm cracks of the forge have gone dim and swarm whoever lights a lamp in Shilhak's seam. Without that seam there is no bronze, and without bronze there is no new bellows for the Grande Forja.",
                "Morcegos que se aninharam nas últimas fendas quentes da forja ficaram apagados e atacam quem acende lamparina no veio do Shilhak. Sem esse veio não tem bronze, e sem bronze não tem fole novo para a Grande Forja."),
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.obj_drive_out_ember_bats", "Drive the ember bats out of the seam", "Expulsar os morcegos de brasa do veio"),
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.journal",
                "Shilhak sings to the rock before he swings the pick. Joaquim tried it and the rock answered with an echo. Maria says that counts.",
                "O Shilhak canta para a pedra antes de bater a picareta. O Joaquim tentou e a pedra respondeu com eco. A Maria diz que conta."),
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.offer",
                "Untash says you break slag like real dwarves. Then come down to my seam: six swarms of ember bats won't let a lamp burn. Clear them out and I'll give you a pot of ginger tea. Up on the Mazans' summit the wind cuts like a chisel.",
                "O Untash diz que vocês quebram escória como anão de verdade. Então desçam no meu veio: seis bandos de morcegos de brasa não deixam lamparina acesa. Tirem eles de lá e eu dou um pote de chá de gengibre. Lá no cume dos Mazans o vento corta feito talhadeira."),
            new LocalizationEntry("quest.city_saloes_de_bronze_ember_bats.active",
                "Bats go for the light. Hold the lamp high, let them come, and swing low.",
                "Morcego vai atrás da luz. Segurem a lamparina no alto, deixem eles virem, e batam baixo."),

            // ================= Mercado dos Túneis — Takhsich, o inventor =================
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.title", "Webs in the Pipes", "Teia nos Tubos"),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.description",
                "Spiders have spun so much web over the mouths of the air pipes that Takhsich's fans can no longer pull the Ar Sereno down from the cracks, and the market's air is going still. The inventor of the Tunnel Market wants his pipes breathing again.",
                "As aranhas teceram tanta teia na boca dos tubos de ar que os cata-ventos do Takhsich não conseguem mais puxar o Ar Sereno das frestas, e o ar do mercado está ficando parado. O inventor do Mercado dos Túneis quer os tubos respirando de novo."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.obj_clear_webbed_pipes", "Clear the spiders from the air pipes", "Tirar as aranhas dos tubos de ar"),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.journal",
                "Takhsich and Ícaro spent an hour drawing a fan with seven blades. Then an hour taking two away. Lavine says the spiders only wanted somewhere warm.",
                "O Takhsich e o Ícaro passaram uma hora desenhando um cata-vento de sete pás. Depois uma hora tirando duas. A Lavine diz que as aranhas só queriam um lugar quentinho."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.offer",
                "You came in through the north gallery? Then you felt it: the air is still. Six spiders are webbing my pipes shut. Clear them and I'll show you how the fan works. And here, an oil lamp, for the dark bits, and two bottles of the air they were stealing.",
                "Vocês entraram pela galeria norte? Então sentiram: o ar está parado. Seis aranhas estão tecendo meus tubos fechados. Tirem elas e eu mostro como o cata-vento funciona. E toma, uma lamparina, para os pedaços escuros, e duas garrafas do ar que elas estavam roubando."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_webbed_pipes.active",
                "Follow the pipes: where the fan stops, there's a spider.",
                "Sigam os tubos: onde o cata-vento para, tem aranha."),

            // ================= Mercado dos Túneis — Farnkhund, o vagoneiro =================
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.title", "Stones on the Rails", "Pedras nos Trilhos"),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.description",
                "Stone golems have wandered onto the cart rails that bring the market's goods from the Wheel Canyon, and Farnkhund's carts stop dead in front of them. The cart driver of the tunnels wants the line clear, and pays in the oil that never goes out.",
                "Golens de pedra entraram nos trilhos dos vagonetes que trazem as mercadorias do Cânion da Roda, e os vagonetes do Farnkhund param em seco na frente deles. O vagoneiro dos túneis quer a linha livre, e paga com o óleo que nunca apaga."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.obj_clear_the_rails", "Clear the stone golems off the cart rails", "Tirar os golens de pedra dos trilhos dos vagonetes"),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.journal",
                "Farnkhund knows every switch of the tunnels by its squeak. Maithe timed one of his carts and it beat her. She wants a rematch.",
                "O Farnkhund conhece cada desvio dos túneis pelo rangido. A Maithe cronometrou um vagonete dele e perdeu. Ela quer revanche."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.offer",
                "Takhsich's pipes are breathing, so you're good with your hands. Four stone golems are sitting on my rails like they bought them. Get them off and take this oil: down in the Wheel Canyon only the eternal fire stays lit.",
                "Os tubos do Takhsich estão respirando, então vocês são bons de mão. Quatro golens de pedra estão sentados nos meus trilhos como se tivessem comprado. Tirem eles e levem este óleo: lá no Cânion da Roda só o fogo eterno fica aceso."),
            new LocalizationEntry("quest.city_mercado_dos_tuneis_stones_on_the_rails.active",
                "A golem on a rail can't turn. Come at it from the side, never from in front.",
                "Golem em cima do trilho não vira. Cheguem pelo lado, nunca pela frente."),

            // ================= Aratta — Enmerkar, o chefe construtor =================
            new LocalizationEntry("quest.city_aratta_grass_jackals.title", "Jackals in the Dry Grass", "Chacais no Capim Seco"),
            new LocalizationEntry("quest.city_aratta_grass_jackals.description",
                "Jackals dimmed by the shadow hide in the old grass that the Fogo Vivo has not renewed yet, and fall on the calves of Aratta's herds. Enmerkar, the chief builder, wants the pens safe before the next low burn.",
                "Chacais apagados pela sombra se escondem no capim velho que o Fogo Vivo ainda não renovou, e caem em cima dos bezerros dos rebanhos de Aratta. Enmerkar, o chefe construtor, quer os cercados seguros antes da próxima queimada baixa."),
            new LocalizationEntry("quest.city_aratta_grass_jackals.obj_drive_off_grass_jackals", "Drive the jackals out of the dry grass", "Espantar os chacais do capim seco"),
            new LocalizationEntry("quest.city_aratta_grass_jackals.journal",
                "Enmerkar writes everything on clay: how many calves, how many pens, how many jackals. Maria says he even wrote our names. Ícaro wants to know how the clay keeps the words.",
                "O Enmerkar escreve tudo na argila: quantos bezerros, quantos cercados, quantos chacais. A Maria diz que ele até escreveu os nossos nomes. O Ícaro quer saber como a argila guarda as palavras."),
            new LocalizationEntry("quest.city_aratta_grass_jackals.offer",
                "Edna's grandchildren! I wrote you would come, and here you are: the clay never lies. Six jackals are hiding in the old grass and the calves cry at night. Drive them off and Aratta will owe you a song.",
                "Os netos da Edna! Eu escrevi que vocês viriam, e aqui estão: a argila nunca mente. Seis chacais estão escondidos no capim velho e os bezerros choram de noite. Espantem eles e Aratta vai dever uma canção a vocês."),
            new LocalizationEntry("quest.city_aratta_grass_jackals.active",
                "The jackals hate the burnt strips. Walk the black paths and they come to you in the open.",
                "Os chacais detestam as faixas queimadas. Andem pelos caminhos pretos e eles vêm até vocês no aberto."),

            // ================= Aratta — Shulgi, o corredor =================
            new LocalizationEntry("quest.city_aratta_shulgi_course.title", "The Course of Shulgi", "O Percurso de Shulgi"),
            new LocalizationEntry("quest.city_aratta_shulgi_course.description",
                "Baboons gone wild sit on the marked course of the Corrida de Shulgi and throw stones at whoever runs past. Shulgi, the runner of Aratta, cannot train, and the race ribbons are waiting.",
                "Babuínos bravos sentaram no percurso marcado da Corrida de Shulgi e jogam pedra em quem passa correndo. Shulgi, o corredor de Aratta, não consegue treinar, e as fitas de corrida estão esperando."),
            new LocalizationEntry("quest.city_aratta_shulgi_course.obj_clear_the_course", "Clear the baboons off the race course", "Tirar os babuínos do percurso da corrida"),
            new LocalizationEntry("quest.city_aratta_shulgi_course.journal",
                "Shulgi says he once ran from one city to another in a single day, and back. Maithe believes him. Joaquim asked for a rest halfway through the explanation.",
                "O Shulgi diz que uma vez correu de uma cidade até outra num dia só, e voltou. A Maithe acredita. O Joaquim pediu para descansar no meio da explicação."),
            new LocalizationEntry("quest.city_aratta_shulgi_course.offer",
                "Enmerkar's pens are safe, so you're fast enough for me. Five baboons are sitting on my course like judges. Get them off and take these waterskins: out on the savanna, whoever drinks last runs first.",
                "Os cercados do Enmerkar estão seguros, então vocês são rápidos o bastante para mim. Cinco babuínos estão sentados no meu percurso feito juízes. Tirem eles de lá e levem estes odres: lá na savana, quem bebe por último corre na frente."),
            new LocalizationEntry("quest.city_aratta_shulgi_course.active",
                "Follow the horn posts with the blue ribbons: the course runs from one to the next.",
                "Sigam os postes de chifre com fita azul: o percurso vai de um para o outro."),

            // ================= Kirta — Kirta, o rei =================
            new LocalizationEntry("quest.city_kirta_reef_ooze.title", "Ooze on the Reef", "Lodo no Recife"),
            new LocalizationEntry("quest.city_kirta_reef_ooze.description",
                "The Lodo has crept over the reef in living lumps that smother the coral and put out its colour. King Kirta, who lost everything to the sea once and rebuilt it all, asked the cousins to scrape the ooze off before it reaches the mother coral.",
                "O Lodo se espalhou pelo recife em bolotas vivas que abafam o coral e apagam a cor dele. O rei Kirta, que já perdeu tudo para o mar uma vez e reconstruiu tudo, pediu aos primos que raspem o lodo antes que chegue ao coral-mãe."),
            new LocalizationEntry("quest.city_kirta_reef_ooze.obj_scrape_the_ooze", "Scrape the living ooze off the reef", "Raspar o lodo vivo do recife"),
            new LocalizationEntry("quest.city_kirta_reef_ooze.journal",
                "Where the ooze came off, the coral blushed pink in a breath. Lavine says it was holding its breath all this time. Kirta smiled for the first time today.",
                "Onde o lodo saiu, o coral ficou rosa num suspiro. A Lavine diz que ele estava prendendo a respiração esse tempo todo. O Kirta sorriu pela primeira vez hoje."),
            new LocalizationEntry("quest.city_kirta_reef_ooze.offer",
                "Edna's grandchildren, walking on my reef. Jorge told me you would come when the coral went grey. Six lumps of the Lodo are smothering the east reef. Scrape them off and the coral will remember its colour.",
                "Os netos da Edna, andando no meu recife. O Jorge me disse que vocês viriam quando o coral ficasse cinza. Seis bolotas do Lodo estão abafando o recife do leste. Raspem elas e o coral vai lembrar da cor."),
            new LocalizationEntry("quest.city_kirta_reef_ooze.active",
                "The ooze hides where the coral is greyest. Follow the grey.",
                "O lodo se esconde onde o coral está mais cinza. Sigam o cinza."),

            // ================= Kirta — Pughat, a que cuida de todos =================
            new LocalizationEntry("quest.city_kirta_divers_rescue.title", "The Cornered Divers", "Os Mergulhadores Encurralados"),
            new LocalizationEntry("quest.city_kirta_divers_rescue.description",
                "Lantern fish dimmed by the Lodo have cornered three of Kirta's pearl divers in a grotto of the reef, and their air is running out. Pughat, who looks after everyone, is going in, and she wants the cousins with her.",
                "Peixes-lanterna apagados pelo Lodo encurralaram três mergulhadores de pérola de Kirta numa gruta do recife, e o ar deles está acabando. A Pughat, que cuida de todo mundo, vai entrar, e quer os primos junto."),
            new LocalizationEntry("quest.city_kirta_divers_rescue.obj_free_the_divers", "Drive off the lantern fish around the divers' grotto", "Afastar os peixes-lanterna da gruta dos mergulhadores"),
            new LocalizationEntry("quest.city_kirta_divers_rescue.journal",
                "Pughat counted the divers twice when they came out: three in, three out. Then she counted us. Five. She says that is the only sum that matters.",
                "A Pughat contou os mergulhadores duas vezes quando eles saíram: três entraram, três saíram. Depois contou a gente. Cinco. Ela diz que essa é a única conta que importa."),
            new LocalizationEntry("quest.city_kirta_divers_rescue.offer",
                "You scraped the reef for my king, now help me with my divers. Five lantern fish have them cornered in the grotto. Drive them off and take this bubble of air: down in Sunken Kilwa every house is under water.",
                "Vocês rasparam o recife para o meu rei, agora me ajudem com os meus mergulhadores. Cinco peixes-lanterna encurralaram eles na gruta. Afastem os peixes e levem esta bolha de ar: lá em Kilwa Afundada toda casa fica debaixo d'água."),
            new LocalizationEntry("quest.city_kirta_divers_rescue.active",
                "Lantern fish chase the light. Cover your lamps, come from the dark side.",
                "Peixe-lanterna corre atrás da luz. Cubram as lamparinas, cheguem pelo lado escuro."),

            // ================= Jhilpur — Salmaios, o velho da Raiz-Mãe =================
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.title", "Ooze on the Ghats", "Lodo nos Ghats"),
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.description",
                "Living lumps of the Lodo climb the brick steps of the ghats a little higher every night, and old Salmaios, who has tended the Raiz-Mãe since before the city had a name, fears they will reach its roots. He asked the cousins to scrape them off the steps.",
                "Bolotas vivas do Lodo sobem os degraus de tijolo dos ghats um pouco mais a cada noite, e o velho Salmaios, que cuida da Raiz-Mãe desde antes de a cidade ter nome, tem medo de que cheguem às raízes dela. Ele pediu aos primos que raspem o lodo dos degraus."),
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.obj_scrape_the_ghats", "Scrape the living ooze off the ghats", "Raspar o lodo vivo dos ghats"),
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.journal",
                "When the last lump slid back into the water, the ring at the foot of the Raiz-Mãe shone green for a moment. Joaquim swears the tree sighed. Salmaios just nodded, as if it had said thank you.",
                "Quando a última bolota escorregou de volta para a água, o anel no pé da Raiz-Mãe brilhou verde por um instante. O Joaquim jura que a árvore suspirou. O Salmaios só acenou com a cabeça, como se ela tivesse dito obrigado."),
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.offer",
                "Edna's grandchildren, in my roots. Jorge sat on these same steps forty monsoons ago. Seven lumps of the Lodo are climbing the ghats tonight. Scrape them off before they touch the Raiz-Mãe, and the Living Water will run clean at her feet again.",
                "Os netos da Edna, nas minhas raízes. O Jorge sentou nesses mesmos degraus há quarenta monções. Sete bolotas do Lodo estão subindo os ghats hoje. Raspem elas antes que encostem na Raiz-Mãe, e a Água Viva vai correr limpa nos pés dela de novo."),
            new LocalizationEntry("quest.city_jhilpur_ooze_on_the_ghats.active",
                "The ooze comes up where the steps are darkest. Follow the dark steps down.",
                "O lodo sobe onde os degraus estão mais escuros. Desçam pelos degraus escuros."),

            // ================= Jhilpur — Abiesous, o mestre da escola de barco =================
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.title", "Crows on the Oars", "Corvos nos Remos"),
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.description",
                "Crows dimmed by the Lodo have taken the roof of Abiesous's boat school and steal the oars right out of the pupils' hands. The master teaches patience, but the monsoon is coming and nobody can learn to row without an oar.",
                "Corvos apagados pelo Lodo tomaram o telhado da escola de barco do Abiesous e roubam os remos da mão dos alunos. O mestre ensina paciência, mas a monção está chegando e ninguém aprende a remar sem remo."),
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.obj_chase_the_crows", "Chase the dimmed crows off the boat school", "Espantar os corvos apagados da escola de barco"),
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.journal",
                "The oars came back, one by one, and Abiesous counted them like a teacher counts pupils after a storm. Then he gave Lavine the first lesson for free. She rowed in a circle. He called it a good start.",
                "Os remos voltaram, um por um, e o Abiesous contou todos como um professor conta os alunos depois de uma tempestade. Depois deu a primeira aula de graça para a Lavine. Ela remou em círculo. Ele disse que foi um bom começo."),
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.offer",
                "You cleaned Salmaios's steps, so you are patient enough for my school. Six crows are sitting on my roof with my oars. Chase them off and take this purifying fruit: down in the Heart of the Mire the water is so black that only it lets you breathe.",
                "Vocês limparam os degraus do Salmaios, então têm paciência o bastante para a minha escola. Seis corvos estão sentados no meu telhado com os meus remos. Espantem eles e levem esta fruta purificadora: lá no Coração do Lodo a água é tão preta que só ela deixa a gente respirar."),
            new LocalizationEntry("quest.city_jhilpur_boat_school_crows.active",
                "Crows love what shines. Wave a lantern and they will come down to you.",
                "Corvo gosta do que brilha. Balancem uma lanterna e eles descem até vocês."),

            // ================= Palafitas de Tarchon — Tarchon, o fundador =================
            new LocalizationEntry("quest.city_tarchon_caught_wisps.title", "Caught Wisps", "Fogos-Fátuos Presos"),
            new LocalizationEntry("quest.city_tarchon_caught_wisps.description",
                "Every night the swamp round the stilts fills with will-o'-wisps: sparks of light caught in the reeds, buzzing and angry. Half the trols have stared at them so long that they forgot who they were. Old Tarchon, who drove the first pile into this mud, asked the cousins to set them free.",
                "Toda noite o pântano em volta das palafitas enche de fogos-fátuos: faíscas de luz presas no junco, zumbindo, bravas. Metade dos trols olhou tanto para elas que esqueceu quem era. O velho Tarchon, que fincou a primeira estaca nessa lama, pediu aos primos que soltem elas."),
            new LocalizationEntry("quest.city_tarchon_caught_wisps.obj_free_the_wisps", "Set the caught will-o'-wisps free", "Libertar os fogos-fátuos presos"),
            new LocalizationEntry("quest.city_tarchon_caught_wisps.journal",
                "The last wisp went up slowly, like a bubble, and hung over the mast for a moment before it went on to the sky. On the boardwalk, a trol who had not spoken for a year said good evening. Tarchon cried a little. Very slowly.",
                "O último fogo-fátuo subiu devagar, como uma bolha, e ficou um instante em cima do mastro antes de seguir para o céu. Na passarela, um trol que não falava fazia um ano disse boa noite. O Tarchon chorou um pouco. Bem devagar."),
            new LocalizationEntry("quest.city_tarchon_caught_wisps.offer",
                "Edna's... grandchildren. Jorge... rowed here... once. Lost. Laughing. Eight sparks... are caught... in the reeds tonight. They are not... wicked. Only... lost. Let them... go home.",
                "Os netos... da Edna. O Jorge... remou até aqui... uma vez. Perdido. Rindo. Oito faíscas... estão presas... no junco hoje. Elas não são... más. Só... perdidas. Deixem elas... voltar para casa."),
            new LocalizationEntry("quest.city_tarchon_caught_wisps.active",
                "Wisps... buzz louder... near the water. Follow... the buzzing.",
                "Os fogos-fátuos... zumbem mais alto... perto da água. Sigam... o zumbido."),

            // ================= Palafitas de Tarchon — Vel, o pescador =================
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.title", "Herons on the Nets", "Garças nas Redes"),
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.description",
                "Pale ghost birds, dimmed by the Lodo, stand on the poles of Vel's nets all night and steal every catfish he catches. Vel is too gentle to chase anything away; he asked the cousins to do it for him.",
                "Pássaros pálidos, apagados pelo Lodo, ficam a noite inteira nos varais das redes do Vel e roubam todo bagre que ele pega. O Vel é manso demais para espantar qualquer coisa; pediu aos primos que façam isso por ele."),
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.obj_chase_the_herons", "Chase the ghost birds off Vel's nets", "Espantar os pássaros-fantasma das redes do Vel"),
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.journal",
                "The nets came up full for the first time in weeks. Vel counted the catfish twice, gave the fattest one to Maria and laughed so hard the boardwalk shook. Then he hung an amulet round Lavine's neck: \"For the bog. The dragon is shy, but the snakes are not.\"",
                "As redes subiram cheias pela primeira vez em semanas. O Vel contou os bagres duas vezes, deu o mais gordo para a Maria e riu tanto que a passarela tremeu. Depois pendurou um amuleto no pescoço da Lavine: \"Para o charco. A dragoa é tímida, mas as cobras não são.\""),
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.offer",
                "You freed the wisps. Tarchon smiled. Took him... a year. Now... my nets. Six pale birds... on my poles. Every night. Chase them... and take this amulet. In the Ninki Nanka's bog... the snakes bite.",
                "Vocês soltaram os fogos-fátuos. O Tarchon sorriu. Levou... um ano. Agora... as minhas redes. Seis pássaros pálidos... nos meus varais. Toda noite. Espantem eles... e levem este amuleto. No charco do Ninki Nanka... as cobras mordem."),
            new LocalizationEntry("quest.city_tarchon_herons_on_the_nets.active",
                "They come... when the mist is lilac. Wait... by the nets.",
                "Eles vêm... quando a névoa fica lilás. Esperem... perto das redes."),

            // ================= Sangkent — Abā, o chefe das calhas =================
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.title", "Stones in the Chutes", "Pedras nas Calhas"),
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.description",
                "Stones dimmed by the cold have come loose from the slopes above Sangkent and roll down the polished chutes on their own, jamming the carts between the terraces. Abā, who keeps every chute of the city running, asked the cousins to break them up before a cart full of people meets one.",
                "Pedras apagadas pelo frio se soltaram das encostas acima de Sangkent e descem sozinhas pelas calhas polidas, travando os carrinhos entre os terraços. O Abā, que mantém todas as calhas da cidade funcionando, pediu aos primos que quebrem as pedras antes que um carrinho cheio de gente encontre uma."),
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.obj_clear_the_chutes", "Break the rolling stones off the chutes", "Quebrar as pedras-rolantes das calhas"),
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.journal",
                "With the last stone in pieces, Abā sent an empty cart down the long chute and listened to it all the way to the market terrace. Not a single bump. Ícaro asked to go down in the next one. Abā said yes, then said no, and then went down with him.",
                "Com a última pedra em pedaços, o Abā mandou um carrinho vazio pela calha comprida e ficou escutando até ele chegar ao terraço do mercado. Nenhum solavanco. O Ícaro pediu para descer no próximo. O Abā disse que sim, depois disse que não, e depois desceu junto com ele."),
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.offer",
                "Edna's grandchildren, on my terraces! Jorge rode my first chute when it was still a plank on the snow. Now seven stones roll down my chutes with nobody in them. Break them before they meet a cart, and the whole city will ride in peace again.",
                "Os netos da Edna, nos meus terraços! O Jorge desceu na minha primeira calha quando ela ainda era uma tábua na neve. Agora sete pedras descem pelas minhas calhas sem ninguém dentro. Quebrem elas antes que encontrem um carrinho, e a cidade inteira vai voltar a descer em paz."),
            new LocalizationEntry("quest.city_sangkent_stones_in_the_chutes.active",
                "A stone that rolls on its own always comes back up for another go. Wait for it at the top of the chute.",
                "Pedra que rola sozinha sempre sobe de novo para mais uma descida. Esperem por ela no alto da calha."),

            // ================= Sangkent — Kustai, o escriba do quadro das caravanas =================
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.title", "Mist on the Board", "Neblina no Quadro"),
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.description",
                "A bad-wind mist has settled over the caravan notice board that Kustai keeps by the plaza: the ink runs, the notices blow away and the caravans no longer know who is going east and who is going west. The scribe asked the cousins to chase it off the terraces.",
                "Uma neblina de vento ruim pousou no quadro de avisos das caravanas que o Kustai cuida perto da praça: a tinta escorre, os avisos voam e as caravanas já não sabem quem vai para o leste e quem vai para o oeste. O escriba pediu aos primos que espantem a neblina dos terraços."),
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.obj_clear_the_board", "Chase the ill-wind mist off the notice board", "Espantar a neblina do vento ruim do quadro de avisos"),
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.journal",
                "Kustai copied every notice again, in his small careful hand, and pinned them up one by one. Maithe read them all aloud for the caravaneers who cannot read. He gave her the storm cloak and said the Wakhan Pass is the only notice that never changes: go in warm.",
                "O Kustai copiou todos os avisos de novo, com a letra pequena e caprichada dele, e pregou um por um. A Maithe leu todos em voz alta para os caravaneiros que não sabem ler. Ele deu a capa de tempestade para ela e disse que o Passo de Wakhan é o único aviso que nunca muda: entre agasalhado."),
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.offer",
                "You cleared Abā's chutes; now my board. Six puffs of bad-wind mist sit on it and the ink runs off every notice. Chase them off and take this storm cloak: up in the Wakhan Pass the wind tears the words out of your mouth.",
                "Vocês limparam as calhas do Abā; agora o meu quadro. Seis nuvens de neblina do vento ruim estão sentadas nele e a tinta escorre de todos os avisos. Espantem elas e levem esta capa de tempestade: lá no Passo de Wakhan o vento arranca as palavras da boca da gente."),
            new LocalizationEntry("quest.city_sangkent_mist_on_the_board.active",
                "The mist hates the dry wind from the valley. Stand with your back to the valley and it comes to you.",
                "A neblina detesta o vento seco do vale. Fiquem de costas para o vale e ela vem até vocês."),
        };

        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }

        /// <summary>pt-BR text of <paramref name="key"/> without a manager, or null.</summary>
        public static string GetFallback(string key)
        {
            LocalizationEntry entry = All.FirstOrDefault(e => e.Key == key);
            return entry?.PtBR;
        }
    }
}
