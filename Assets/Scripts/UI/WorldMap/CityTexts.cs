using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the cities (<see cref="CityCatalog"/>, <see cref="CityShopCatalog"/>): city names
    /// and flavour, themed shop names, residents and their line for each cousin, the "Você está em zona segura" HUD
    /// indicator and the Barcas de Luz window (fast travel, Docs/WorldBible_Maniqueismo.md §3.16). Residents who are also
    /// region quest-givers (Mariam, Mestre Mihr, Parthenos, Ammo; ids cida/tonho/benedita/bento) reuse their <c>dialogue.*.name</c> key from <see cref="RegionQuestTexts"/>. Same
    /// self-contained pattern as <see cref="VillageHubTexts"/>: merged into the live <see cref="LocalizationManager"/> on
    /// first use (keys it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class CityTexts
    {
        public const string SafeZone = "ui.city.safe_zone";
        /// <summary>{0} = city name.</summary>
        public const string SafeZoneIn = "ui.city.safe_zone_in";
        /// <summary>{0} = city name.</summary>
        public const string NewStop = "ui.city.new_stop";
        /// <summary>{0} = city name.</summary>
        public const string Respawned = "ui.city.respawned";
        /// <summary>{0} = city button label.</summary>
        public const string ButtonNews = "ui.city.button_news";
        /// <summary>{0} = resident, {1} = what they say.</summary>
        public const string Speech = "ui.city.speech";
        public const string PeddlerToday = "ui.city.peddler_today";

        public const string FastTravel = "ui.city.fast_travel";
        public const string FastTravelTitle = "ui.city.fast_travel.title";
        public const string FastTravelSubtitle = "ui.city.fast_travel.subtitle";
        /// <summary>{0} = gold in the bag.</summary>
        public const string FastTravelGold = "ui.city.fast_travel.gold";
        /// <summary>{0} = gold cost.</summary>
        public const string FastTravelGo = "ui.city.fast_travel.go";
        /// <summary>{0} = roads between the cities.</summary>
        public const string FastTravelRoads = "ui.city.fast_travel.roads";
        public const string FastTravelHere = "ui.city.fast_travel.here";
        public const string FastTravelNotVisited = "ui.city.fast_travel.not_visited";
        public const string FastTravelLocked = "ui.city.fast_travel.locked";
        public const string FastTravelNoRoute = "ui.city.fast_travel.no_route";
        /// <summary>{0} = gold cost.</summary>
        public const string FastTravelNoGold = "ui.city.fast_travel.no_gold";
        public const string FastTravelNotInCity = "ui.city.fast_travel.not_in_city";
        /// <summary>{0} = city name, {1} = gold paid.</summary>
        public const string FastTravelDone = "ui.city.fast_travel.done";
        public const string FastTravelClose = "ui.city.fast_travel.close";
        public const string FastTravelBossLocked = "ui.city.fast_travel.boss_locked";
        public const string FastTravelEventActive = "ui.city.fast_travel.event_active";
        /// <summary>{0} = destination city. The Barca do Sol line; at dusk and night <see cref="Get"/> serves <see cref="FastTravelOnTheWayMoon"/>.</summary>
        public const string FastTravelOnTheWay = "ui.city.fast_travel.on_the_way";
        /// <summary>{0} = destination city. The Barca da Lua line (dusk and night, <see cref="FastTravelTransition.IsMoonShipNow"/>).</summary>
        public const string FastTravelOnTheWayMoon = "ui.city.fast_travel.on_the_way_moon";

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                // ---- HUD / hub ----
                new LocalizationEntry(SafeZone, "You are in a safe zone", "Você está em zona segura"),
                new LocalizationEntry(SafeZoneIn, "You are in a safe zone · {0}", "Você está em zona segura · {0}"),
                new LocalizationEntry(NewStop, "New ship dock: {0}", "Novo cais da barca: {0}"),
                new LocalizationEntry(Respawned, "You woke up in {0}. Nobody fights in here.", "Vocês acordaram em {0}. Aqui ninguém briga."),
                new LocalizationEntry(ButtonNews, "{0} (!)", "{0} (!)"),
                new LocalizationEntry(Speech, "{0}: “{1}”", "{0}: “{1}”"),
                new LocalizationEntry(PeddlerToday, "Ammo's caravan is in town today!", "A caravana do Ammo está na cidade hoje!"),

                // ---- Barcas de Luz (Sun ship by day, Moon ship at night) ----
                new LocalizationEntry(FastTravel, "Ship Dock", "Cais da Barca"),
                new LocalizationEntry(FastTravelTitle, "Light Ships", "Barcas de Luz"),
                new LocalizationEntry(FastTravelSubtitle, "The Sun Ship by day and the Moon Ship by night sail the river of light to the towns you have already been to. No beasts on the way.", "A Barca do Sol de dia e a Barca da Lua de noite navegam pelo rio de luz até as cidades onde vocês já estiveram. Nenhum bicho no caminho."),
                new LocalizationEntry(FastTravelGold, "Gold in the bag: {0}", "Ouro na mochila: {0}"),
                new LocalizationEntry(FastTravelGo, "Board (fare: {0} gold)", "Embarcar (passagem: {0} ouro)"),
                new LocalizationEntry(FastTravelRoads, "{0} road(s) away", "a {0} estrada(s)"),
                new LocalizationEntry(FastTravelHere, "You are here", "Vocês estão aqui"),
                new LocalizationEntry(FastTravelNotVisited, "Not visited yet: walk there once", "Ainda não visitada: vá a pé uma vez"),
                new LocalizationEntry(FastTravelLocked, "Region closed", "Região fechada"),
                new LocalizationEntry(FastTravelNoRoute, "No road there", "Sem estrada até lá"),
                new LocalizationEntry(FastTravelNoGold, "Not enough gold for the fare ({0})", "Falta ouro para a passagem ({0})"),
                new LocalizationEntry(FastTravelNotInCity, "The ship only leaves from a town's dock", "A barca só sai do cais de uma cidade"),
                new LocalizationEntry(FastTravelDone, "You arrived in {0}! (-{1} gold)", "Chegaram em {0}! (-{1} ouro)"),
                new LocalizationEntry(FastTravelClose, "Close", "Fechar"),
                new LocalizationEntry(FastTravelBossLocked, "The ship only docks where the light was set free: beat this region's boss", "A barca só atraca onde a luz já foi libertada: vençam o chefe da região"),
                new LocalizationEntry(FastTravelEventActive, "The ship won't sail with trouble on the road", "A barca não sai com encrenca na estrada"),
                new LocalizationEntry(FastTravelOnTheWay, "The Sun Ship glides down the river of light to {0}...", "A Barca do Sol desliza pelo rio de luz até {0}..."),
                new LocalizationEntry(FastTravelOnTheWayMoon, "The Moon Ship glides in silence to {0}...", "A Barca da Lua desliza em silêncio até {0}..."),

                // ---- Shops ----
                Shop(CityShopCatalog.MerceariaLazaro, "Zaku's Grocery", "Mercearia do Zaku"),
                Shop(CityShopCatalog.ArmarinhoCida, "Mariam's Thread Bazaar", "Bazar de Linhas da Mariam"),
                Shop(CityShopCatalog.FeiraLivre, "Nana's Bazaar", "Bazar da Nana"),
                Shop(CityShopCatalog.SelariaVava, "Vandak's Saddlery", "Selaria do Vandak"),
                Shop(CityShopCatalog.ForjaTonho, "Master Mihr's Forge", "Forja do Mestre Mihr"),
                Shop(CityShopCatalog.LamparinasBenedita, "Parthenos's Lamps", "Lamparinas da Parthenos"),
                Shop(CityShopCatalog.MascateBento, "Ammo's Caravan", "Caravana do Ammo"),
                // Same text as MountTexts.HarasShopName (the mounts' own table registers it too).
                Shop(CityShopCatalog.HarasVila, "Srosh's Stable", "Estrebaria do Srosh"),
                Shop(CityShopCatalog.ErvariaDacil, "Dácil's Herb House", "Ervaria da Dácil"),
                Shop(CityShopCatalog.BazarMwana, "Mwana's Pearl Bazaar", "Bazar de Pérolas da Mwana"),
                Shop(CityShopCatalog.ForjaTambor, "The Drum Forge", "Forja de Tambor"),
                Shop(CityShopCatalog.PortoesRuna, "Puzur's Rune Gates", "Portões de Runa do Puzur"),
                Shop(CityShopCatalog.FeiraMiwnay, "Miwnay's Fair", "Feira da Miwnay"),
                Shop(CityShopCatalog.AlforjeLugalbanda, "Lugalbanda's Saddlebag", "Alforje do Lugalbanda"),
                Shop(CityShopCatalog.MergulhoDanel, "Danel's Diving Tools", "Ferramentas de Mergulho do Danel"),
                Shop(CityShopCatalog.TrocasNuhzadag, "Nuhzadag's Boats and Trades", "Barcos e Trocas do Nuhzadag"),
                Shop(CityShopCatalog.CestariaLarthia, "Larthia's Baskets", "Cestaria da Larthia"),
            };

            // ---- Cities ----
            AddCity(entries, CityCatalog.VilaZefa, "Grandma Edna's Village", "Vila da Vó Edna", "Village", "Vila",
                "Talk to the villagers, shop at the bazaar and the stores, use the workbench, Grandma's chest and her clay oven.",
                "Converse com os moradores, compre no bazar e nas lojas, use a bancada, o baú e o forno da Vó.");
            AddCity(entries, CityCatalog.AreiaBranca, "Qocho Oasis", "Oásis de Qocho", "Qocho", "Qocho",
                "Rose mud-brick houses around the cistern, lattice walls for drying grapes and the Flaming Mountains glowing on the horizon. Every caravan on the road stops here to drink from Yardena's cistern.",
                "Casas de taipa rosada em volta da cisterna, paredes vazadas para secar uva e as Montanhas Flamejantes ardendo no horizonte. Toda caravana da estrada para aqui para beber da cisterna da Yardena.");
            AddCity(entries, CityCatalog.SerraAlta, "Litema Village", "Vila de Litema", "Litema", "Litema",
                "Round thatched houses with litema-painted walls, high on the Kaggen Range: forge smoke, striped blankets and a drum at night. The wind bites, but the tea is hot.",
                "Casas redondas de teto de palha e paredes pintadas de litema, no alto da Serra de Kaggen: fumaça de forja, cobertores listrados e tambor à noite. O vento corta, mas o chá é quente.");
            AddCity(entries, CityCatalog.Lamparinas, "Lampkeepers' Village", "Vila das Lamparinas", "Lampkeepers", "Lamparinas",
                "At the mouth of the Threshold Caves, every house carved into the rose tuff keeps a lamp lit in the window. Since the light was stolen, nobody here lets the dark win.",
                "Na boca das Grutas do Limiar, cada casa escavada no tufo rosa deixa uma lamparina acesa na janela. Desde que a luz foi roubada, ninguém aqui deixa o escuro ganhar.");

            AddCity(entries, CityCatalog.TaaPwani, "Taa Pwani", "Taa Pwani", "Taa Pwani", "Taa Pwani",
                "The Coast of the Lamp: whitewashed coral-stone houses with carved teak doors, a pearl market on the square, dhows with triangular sails at the quay and the Coral Light that guides them home through the monsoon.",
                "A Costa da Lâmpada: casas de pedra de coral caiada com portas de teca entalhada, mercado de pérolas na praça, dhows de vela triangular no cais e o Farol de Coral que guia todos de volta na monção.");

            AddCity(entries, CityCatalog.SaloesDeBronze, "Napirasu's Bronze Halls", "Salões de Bronze de Napirasu", "Bronze Halls", "Salões",
                "The dwarves' city cut into the mountain: granite halls on square pillars capped in bronze, massive doors under rune lintels that light one line at a time, pits of cold lava and chain lifts. The Grande Forja was lit by the Light held in a crystal; the shadow put it out, and now they forge in the cold.",
                "A cidade dos anões escavada na montanha: salões de granito sobre pilares quadrados com capitel de bronze, portas maciças sob lintéis de runa que acendem uma linha por vez, poços de lava apagada e elevadores de corrente. A Grande Forja era acesa pela Luz presa num cristal; a sombra a apagou, e agora eles forjam no frio.");

            AddCity(entries, CityCatalog.MercadoDosTuneis, "Tunnel Market", "Mercado dos Túneis", "Tunnel Market", "Mercado",
                "The kobolds' market at the crossing of the four galleries: stalls set into niches under arches, cart rails, pulleys, copper pipes with fans that pull the air down from the cracks, and lanterns of coloured bottle glass. Here they bottle the Ar Sereno, and sell it with mended scrap.",
                "O mercado dos kobolds no cruzamento das quatro galerias: barracas encaixadas em nichos sob arcos, trilhos de vagonete, roldanas, canos de cobre com cata-ventos que puxam o ar das frestas e lanternas de vidro colorido de garrafa. Aqui eles engarrafam o Ar Sereno, e vendem junto com sucata consertada.");

            AddCity(entries, CityCatalog.Aratta, "Aratta, City of Horns", "Aratta, a Cidade dos Chifres", "Aratta", "Aratta",
                "The city of the bull folk on the Aurochs Savanna: great halls of reed bound in arches and covered in mats, round stone pens, horn posts with blue ribbons marking the ways and the bull-headed lyre on the square. They keep the Living Fire of the sun: the low burn that renews the grass.",
                "A cidade do povo-touro na Savana dos Auroques: grandes salões de junco amarrado em arcos e cobertos de esteira, cercados redondos de pedra, postes de chifre com fitas azuis marcando os caminhos e a lira de cabeça de touro na praça. Eles guardam o Fogo Vivo do sol: o fogo baixo que renova o capim.");

            AddCity(entries, CityCatalog.Kirta, "Kirta Reef City", "Cidade-Recife de Kirta", "Kirta", "Kirta",
                "The sea folk's city on its atoll: towers of coral with round windows, half drowned, domes of mother-of-pearl, shell bridges over the reef water and gardens of kelp. They tended the coral lit by the Living Water; the Lodo put its colour out, and every spark set free lights a piece of it again.",
                "A cidade do povo-do-mar no seu atol: torres de coral com janelas redondas, meio afogadas, cúpulas de madrepérola, pontes de concha sobre a água do recife e jardins de alga. Eles cuidavam do coral aceso pela Água Viva; o Lodo apagou a cor dele, e cada faísca libertada acende um pedaço de novo.");

            AddCity(entries, CityCatalog.Jhilpur, "Jhilpur, City in the Roots", "Jhilpur, a Cidade nas Raízes", "Jhilpur", "Jhilpur",
                "The capital of the delta, built in the roots of giant fig trees over a brown-green lake: plank houses on root columns under curved thatch roofs, bridges of living root over the canals, brick ghats stepping down into the water and, in the middle of the round plaza, the Raiz-Mãe. Mani's journey to the east passed through here: the Living Water against the Lodo.",
                "A capital do delta, construída nas raízes de figueiras gigantes sobre um lago marrom-esverdeado: casas de tábua sobre colunas de raiz debaixo de telhados de palha curvos, pontes de raiz viva sobre os canais, ghats de tijolo descendo para a água e, no meio da praça redonda, a Raiz-Mãe. A viagem de Mani para o leste passou por aqui: a Água Viva contra o Lodo.");

            AddCity(entries, CityCatalog.PalafitasTarchon, "Tarchon Stilts", "Palafitas de Tarchon", "Tarchon", "Tarchon",
                "The trols' city in the open swamp: long houses on tall piles with leaf roofs down to the floor, plank boardwalks over the bog water, nets drying, baskets as tall as a cousin and floating reed islands. At night the swamp fills with will-o'-wisps, sparks caught in the Ar Sereno; half the trols stared at them until they forgot themselves, and every spark set free wakes a neighbour.",
                "A cidade dos trols no meio do pântano: casas compridas sobre estacas altas com telhado de folha até o chão, passarelas de tábua sobre a água do charco, redes secando, cestos do tamanho de um primo e ilhas de junco que boiam. À noite o pântano enche de fogos-fátuos, faíscas presas no Ar Sereno; metade dos trols olhou para elas até se esquecer, e cada faísca libertada acorda um vizinho.");

            AddCity(entries, CityCatalog.TamborRubro, "Red Drum", "Tambor Rubro", "Red Drum", "Tambor Rubro",
                "The orcs' war camp in the red badlands: a palisade of sharpened logs, the Great Hall under its steep hide roof, clan tents dyed ochre and the drums that mark the hours and wake the Living Fire of the forge. Here a given word is worth more than gold.",
                "O acampamento dos orcs nos ermos vermelhos: paliçada de troncos com ponta, o Salão Grande sob o telhado alto de couro, tendas de clã tingidas de ocre e os tambores que marcam a hora e acordam o Fogo Vivo da forja. Aqui palavra dada vale mais que ouro.");

            AddCity(entries, CityCatalog.CopasGarajonay, "Garajonay Canopy", "Copas de Garajonay", "Garajonay", "Garajonay",
                "The city of the forest elves, up in the laurels that drink the mist: round plank houses, rope bridges from trunk to trunk and cocoon lanterns. Nobody here fells a tree.",
                "A cidade dos elfos-da-mata, no alto dos louros que bebem a névoa: casas redondas de tábua, pontes de cipó de tronco em tronco e lanternas de casulo. Aqui ninguém derruba árvore.");

            // ---- Residents: name (unless shared), then Joaquim, Maria, Maithe, Ícaro, Lavine, anyone ----
            // Mariam, the Herbalist (resident id "cida").
            AddResident(entries, "cida", null, null,
                ("That shield is bigger than you, boy. Let me reinforce the strap with good silk thread.", "Esse escudo é maior que você, menino. Deixa eu reforçar a alça com uma linha de seda boa."),
                ("Your grandmother used to come here for hair ribbons too. Take a blue one, it suits the Temple.", "Sua avó também vinha aqui escolher fita de cabelo. Leve uma azul, combina com o Templo."),
                ("From your face, you tore your clothes in the tugai again. Sit, I'll patch it before Grandma sees.", "Pela sua cara, já rasgou a roupa no tugai de novo. Senta que eu remendo antes da Vó ver."),
                ("Thread, needle, buttons... no, Ícaro, a button is not a machine part. Give it back.", "Linha, agulha, botão... não, Ícaro, botão não é peça de máquina. Devolve."),
                ("The youngest! I've kept a red scrap for you since the Bema.", "A caçula! Guardei um retalho vermelho para você desde o Bema."),
                ("Thread, needle and mint tea: a bit of everything here.", "Linha, agulha e chá de hortelã: aqui tem de tudo um pouco."));

            AddResident(entries, "lazaro", "Zaku the Grocer", "Zaku, o Merceeiro",
                ("Warm plov for the one holding the front line. A hungry guardian protects nobody.", "Plov quentinho para quem segura a frente. Guardião de barriga vazia não protege ninguém."),
                ("Maria! I set aside the freshest potions. I counted them twice: they're all there.", "Maria! Separei as poções mais fresquinhas. Contei duas vezes: estão todas aí."),
                ("The eldest cousin always makes the list. How many potions this time? Five cousins, five potions?", "A mais velha dos primos sempre faz a lista. Quantas poções desta vez? Cinco primos, cinco poções?"),
                ("No credit, Ícaro. Not even for a fixed radio. ...Well, depends on the radio.", "Fiado, não, Ícaro. Nem em troca de rádio consertado. ...Bom, depende do rádio."),
                ("Here's a free dried apricot, Lavine. Don't tell your cousins, or there'll be a queue.", "Toma um damasco seco de brinde, Lavine. Mas não conta para os primos, senão vira fila."),
                ("Open since before sunrise. Something new on the counter today.", "Mercearia aberta desde antes do sol nascer. Hoje tem coisa nova no balcão."));

            AddResident(entries, "nenem", "Nana the Market Lady", "Nana, a Feirante",
                ("Hold this basket for me, Joaquim? You're as strong as your grandpa.", "Segura esse cesto para mim, Joaquim? Você é forte que nem seu avô."),
                ("Mint to calm you down and rosemary for courage, girl. The bazaar heals too.", "Hortelã para acalmar e alecrim para a coragem, menina. O bazar também cura."),
                ("You hunt, so tell me: are the wild herbs of the tugai growing well? A huntress's eye never misses.", "Você que caça, me diz: a erva do tugai está nascendo boa? Olho de caçadora não erra."),
                ("No, the scale doesn't need a motor, Ícaro. It has worked for forty years.", "Não, a balança não precisa de motor, Ícaro. Ela funciona há quarenta anos."),
                ("Lavine, try this pomegranate. If you make a face, it's a good one.", "Lavine, prova essa romã. Se fizer careta, é porque está boa."),
                ("Today's bazaar, today's prices: tomorrow the stall is different.", "Bazar de hoje, preço de hoje: amanhã a barraca é outra."));

            AddResident(entries, "tonico_haras", "Srosh the Stablekeeper", "Srosh, o Tratador",
                ("A guardian needs a mount that doesn't spook. The alabai stands firm by your shield, Joaquim.", "Guardião precisa de montaria que não se assusta. O alabai fica firme do lado do seu escudo, Joaquim."),
                ("Listen, Maria: the pony likes you. Your grandpa Jorge rode with me for thirty years and never fell.", "Escuta, Maria: o pônei gosta de você. Seu avô Jorge andou comigo trinta anos de estrada e nunca caiu."),
                ("The eldest picks first, Maithe. Look at the legs, not the coat: that's how Jorge taught me.", "A mais velha escolhe primeiro, Maithe. Olha as pernas, não o pelo: foi assim que o Jorge me ensinou."),
                ("No, Ícaro, a donkey doesn't need a motor. He has a temper, and that's enough.", "Não, Ícaro, asno não precisa de motor. Ele tem é gênio, e isso já basta."),
                ("Lavine, the pony likes apricots as much as you do. Give him half and he's yours.", "Lavine, o pônei gosta de damasco que nem você. Dá a metade e ele é seu."),
                ("Pony, alabai or donkey: a good mount knows the road home on its own.", "Pônei, alabai ou asno: montaria boa sabe sozinha o caminho de casa."));

            // Ammo, the Caravaneer (resident id "bento").
            AddResident(entries, "bento", null, null,
                ("A guardian at my caravan! I've got things that can take a beating, son.", "Um guardião na minha caravana! Tenho coisa que aguenta pancada, meu filho."),
                ("Maria, my camels and I crossed the whole Dawn Isle. I brought what I found on the road.", "Maria, meus camelos e eu atravessamos a Ilha da Aurora inteira. Trouxe o que achei pelo caminho."),
                ("Maithe again! You walk more than my camels. Tell me which road is safe.", "A Maithe de novo! Você anda mais que os meus camelos. Me conta qual estrada está segura."),
                ("I don't sell the Artificers' engine parts, Ícaro. But ask tomorrow — tomorrow's another town.", "Peça de engenho dos Artífices eu não vendo, Ícaro. Mas pergunta amanhã, que amanhã é outra cidade."),
                ("The little one! A proper caravaneer always has a sweet at the bottom of the chest.", "A caçulinha! Caravaneiro que se preza sempre tem um doce no fundo do baú."),
                ("Here today, wherever the road says tomorrow. Buy now, a caravan doesn't wait.", "Hoje estou aqui, amanhã é a estrada que diz. Aproveita, que caravana não espera."));

            AddResident(entries, "vava", "Vandak the Saddler", "Vandak, o Seleiro",
                ("Camel leather withstands tamarisk thorns. It'll withstand your stubbornness too.", "Couro de camelo aguenta espinho de tamargueira. Vai aguentar a sua teimosia também."),
                ("A healer in the Flaming Sands needs good boots. Hot sand respects no one.", "Curandeira nas Areias Flamejantes precisa de bota boa. Areia quente não respeita ninguém."),
                ("A leather vest, girl: the scorpion strikes and can't get in. A smart huntress dresses for it.", "Colete de couro, menina: o escorpião bate e não entra. Caçadora esperta anda vestida."),
                ("You can look at the stitching, Ícaro. Just don't take the saddle apart to see how it works.", "Pode olhar a costura, Ícaro. Só não desmonta a sela para ver como funciona."),
                ("I made a little felt cap just the youngest's size. The sun of the Sands shows no mercy.", "Fiz um gorrinho de feltro do tamanho da caçula. Sol das Areias não perdoa."),
                ("Leather cured in the shade of the vines. Made to last a lifetime.", "Couro curtido na sombra da parreira. Coisa que dura uma vida."));

            AddResident(entries, "socorro", "Yardena of the Cistern", "Yardena, a da Cisterna",
                ("Drink slowly, boy. Water in the desert is gold, and you're always first to share.", "Bebe devagar, menino. Água no deserto é ouro, e você é sempre o primeiro a dividir."),
                ("Since the Crystal went dark, the cistern sank. Could your light bring it back up, Maria?", "Desde que o Cristal apagou, a cisterna baixou. Será que a sua luz faz ela subir, Maria?"),
                ("You watch over the young ones like I watch this water. Rest a little, child.", "Você vigia os mais novos que nem eu vigio esta água. Descansa um pouco, filha."),
                ("A geared water pump? If it works, Ícaro, I'll pay you in dates.", "Uma bomba d'água de engrenagem? Se funcionar, Ícaro, eu te pago em tâmaras."),
                ("Sit in the shade, Lavine. No beast comes inside the oasis — you can rest.", "Senta na sombra, Lavine. Dentro do oásis nenhum bicho entra, pode descansar."),
                ("No fighting in here: whoever arrives thirsty just asks for water.", "Aqui dentro ninguém briga: quem chega com sede só pede água."));

            AddResident(entries, "tiao", "Uzzai the Herder", "Uzzai, o Pastor",
                ("You're the one with the shield? My goats fear nothing, but they respect you.", "Você é o do escudo? Minhas cabras não têm medo de nada, mas de você elas têm respeito."),
                ("My mom said the healer would pass by. Can you look at Pretinha the goat? She's been coughing... baaa.", "Minha mãe disse que a curandeira ia passar. Olha a cabra Pretinha? Ela anda tossindo... méééé."),
                ("Teach me to shoot a bow? I only know how to throw pebbles at goats that run off.", "Me ensina a atirar de arco? Eu só sei jogar pedrinha nas cabras que fogem."),
                ("Does your light meter find lost goats? If it does, you're the greatest inventor on the isle.", "Seu medidor de luz acha cabra perdida? Se achar, você é o maior inventor da ilha."),
                ("Lavine! You're my age and you faced a giant scorpion? I've only faced a billy goat.", "Lavine! Você tem a minha idade e já enfrentou escorpião gigante? Eu só enfrentei bode."),
                ("Careful, Pretinha the goat eats whatever's in your pocket. The camel too, but slower.", "Cuidado que a cabra Pretinha come o que estiver no seu bolso. O camelo também, só que devagar."));

            // Master Mihr, the Miner (resident id "tonho").
            AddResident(entries, "tonho", null, null,
                ("Let me see that shield, Joaquim. Ancient roots, but the rivet's loose. I'll tighten it.", "Deixa eu ver esse escudo, Joaquim. Raiz antiga, mas o rebite está frouxo. Já aperto."),
                ("Forge fire is light too, girl. Until your Crystal returns, this is how we warm the mountain.", "Fogo de forja também é luz, menina. Enquanto o Cristal não volta, é assim que a gente esquenta a serra."),
                ("Iron arrowheads, for the huntress who hits what nobody sees.", "Ponta de flecha de ferro, para a caçadora que acerta o que ninguém vê."),
                ("You can strike the iron, Ícaro, but with the small hammer. The big one nearly took your finger.", "Pode bater no ferro, Ícaro, mas com o martelo pequeno. O grande quase levou seu dedo."),
                ("The forge gets hot, Lavine, but it won't burn whoever stays behind the stone line. Stay there.", "A forja esquenta, Lavine, mas não queima quem fica atrás da linha de pedra. Fica aí."),
                ("Mountain iron, beaten by the bellows. What leaves here doesn't break.", "Ferro da serra, batido no fole. O que sai daqui não quebra."));

            AddResident(entries, "graca", "Coti the Weaver", "Coti, a Tecelã",
                ("A striped blanket for the guardian's shoulders. Mountain cold knocks down even heroes.", "Um cobertor listrado para os ombros do guardião. Frio da serra derruba até herói."),
                ("I weave watching the road. I saw you five arrive and counted: all of you. What a relief.", "Teço olhando a estrada. Vi vocês cinco chegando e contei: estavam todos. Que alívio."),
                ("You climb like a young goat, Maithe. But come down slowly, loose stones are tricky.", "Você sobe a serra que nem cabrito, Maithe. Mas desce devagar, que pedra solta engana."),
                ("A wind-powered loom? Ícaro, if you build that, I'll weave twice as much.", "Um tear movido a vento? Ícaro, se você fizer isso, eu teço o dobro."),
                ("Wool socks for the youngest, red ones. The mantis on my window says they bring luck in the dark.", "Meias de lã para a caçula, vermelhas. O louva-a-deus da janela disse que dão sorte no escuro."),
                ("No beast climbs into the village: the wind and the forge fire scare them off.", "Aqui na vila bicho não sobe: o vento e o fogo da forja espantam."));

            AddResident(entries, "quinzinho", "Innaios the Player", "Innaios, o Tocador",
                ("I'll play a soft lyre tune so the guardian can rest. Even a shield deserves a night off.", "Toco uma música de lira bem mansa para o guardião descansar. Até escudo merece uma noite de folga."),
                ("Sing with me, Maria? They say a healer's voice scares off shadows.", "Canta comigo, Maria? Dizem que voz de curandeira espanta sombra."),
                ("Does the eldest dance with the young ones or keep watch? Tonight, at least, dance.", "A mais velha dança com os mais novos ou fica de vigia? Hoje, pelo menos, dança."),
                ("No, Ícaro, a drum isn't better with gears. ...Fine, show me the drawing.", "Não, Ícaro, tambor não fica melhor com engrenagem. ...Mostra o desenho, vai."),
                ("I wrote this song for the girl who isn't afraid of the dark. I think it's you, Lavine.", "Fiz essa música para a menina que não tem medo do escuro. Acho que é você, Lavine."),
                ("Forge bellows by day, drum and lyre by night: the mountain is never quiet.", "Fole de forja de dia, tambor e lira de noite: a serra nunca fica calada."));

            // Parthenos of the Lamps (resident id "benedita").
            AddResident(entries, "benedita", null, null,
                ("Hang a lamp on your shield, Joaquim. Down there, whoever protects needs to see.", "Pendura uma lamparina no escudo, Joaquim. Lá embaixo, quem protege precisa enxergar."),
                ("The light you seek passed through here, Maria. Every lit lamp is a piece of it kept safe.", "A luz que você procura passou por aqui, Maria. Cada candeia acesa é um pedaço dela guardado."),
                ("Does the Eye of the Wind see in the dark? Take oil anyway. Good eyes get tired too.", "O Olho do Vento enxerga no escuro? Mesmo assim, leva óleo. Olho bom também cansa."),
                ("A self-lighting wick? Ícaro, if it works, the village will throw you a party.", "Um pavio que acende sozinho? Ícaro, se der certo, a Vila das Lamparinas faz festa para você."),
                ("You're the youngest and the one who shakes least in the dark. Parthenos notices these things.", "Você é a mais nova e a que menos treme no escuro. A Parthenos repara nessas coisas."),
                ("Oil, wick and potion: nobody goes down into the Caves without stopping here.", "Óleo, pavio e poção: ninguém desce às Grutas sem passar aqui."));

            AddResident(entries, "candinho", "Old Mihrshah", "Mihrshah, o Velho Mineiro",
                ("I worked forty years in these tunnels. I know where the ceiling holds a guardian, and where it doesn't.", "Trabalhei quarenta anos nessas galerias. Sei onde o teto aguenta um guardião, e onde não."),
                ("I heard the stone sing when the light passed, girl. It was a sad sound, not an angry one.", "Ouvi a pedra cantar quando a luz passou, menina. Era um som triste, não de raiva."),
                ("If you lose the way, Maithe, follow the water. Cave water always finds the way out.", "Se perder o caminho, Maithe, segue a água. Água de gruta sempre acha a saída."),
                ("Take my old tunnel map, Ícaro. Your meter and my map: nobody gets lost that way.", "Toma meu mapa velho das galerias, Ícaro. Seu medidor e meu mapa: aí ninguém se perde."),
                ("The bats don't enter the village, Lavine. They hit the lamplight and turn back.", "Os morcegos não entram na vila, Lavine. Batem na luz das candeias e voltam."),
                ("An old miner never forgets a tunnel, nor the garden of light he saw when he was young. Ask, and I'll answer.", "Mineiro velho não esquece galeria, nem o jardim de luz que viu quando era moço. Pergunta, que eu respondo."));

            AddResident(entries, "fia", "Aso the Storyteller", "Aso, a Contadora",
                ("Did I tell you how Ananse tried to carry all the stories by himself? Stubborn just like you.", "Já te contei de quando Ananse quis carregar todas as histórias sozinho? Teimoso que nem você."),
                ("Folks say the Dweller is afraid of the dark. Being afraid isn't evil, Maria. Just lonely.", "O povo conta que o Habitante tem medo do escuro. Quem tem medo não é mau, Maria. É só sozinho."),
                ("The eldest carries everyone's weight. Today, Maithe, I carry the story: sit.", "A mais velha carrega o peso de todos. Hoje, Maithe, quem carrega a história sou eu: senta."),
                ("Your experiment notebook looks like a storybook, Ícaro. Just with more drawings.", "Seu caderno de experimentos parece livro de histórias, Ícaro. Só que com mais desenho."),
                ("I'll tell you the story of the girl who lit the last lamp. She was your age.", "Vou te contar a história da menina que acendeu a última lamparina. Ela tinha a sua idade."),
                ("A good story is told in the light, and here there's plenty. Want one about Ananse, the clever spider?", "História boa se conta na luz, e aqui luz não falta. Quer uma de Ananse, a aranha esperta?"));

            AddResident(entries, "zezinho", "Gabryab of the Wick", "Gabryab, o do Pavio",
                ("Joaquim, can I hold the shield for a bit? Just a little bit!", "Joaquim, deixa eu segurar o escudo um pouquinho? Só um pouquinho!"),
                ("I trim and light the street lamps every day. It's my job to keep the light, just like yours.", "Eu corto o pavio e acendo as candeias da rua todo dia. Meu trabalho é guardar a luz, igual o seu."),
                ("Is Maithe your boss? She sounds like my big sister when she yells my name.", "A Maithe é a chefe de vocês? Ela parece minha irmã mais velha quando grita meu nome."),
                ("Ícaro, teach me to make sparks without flint? I promise not to burn my bangs again.", "Ícaro, me ensina a fazer faísca sem pederneira? Prometo não queimar a franja de novo."),
                ("Lavine, bet I trim wicks faster than you cast spells!", "Lavine, aposto que eu corto pavio mais rápido do que você solta feitiço!"),
                ("Wick trimmed, lamp lit: darkness doesn't rule here.", "Pavio cortado, candeia acesa: o escuro aqui não manda."));

            // ---- The Vila's lore cast without a counter (TODO.md "História e Arcos dos NPCs") ----
            AddResident(entries, "vo_edna", "Grandma Edna", "Vó Edna",
                ("Sit down, Joaquim, and lean that shield on the bench. A guardian eats first and protects afterwards.", "Senta, Joaquim, e encosta esse escudo no banco. Guardião come primeiro e protege depois."),
                ("Maria, my light. Your grandpa Jorge used to say you shine like the Sun Ship at dawn. Take some bread for the road.", "Maria, minha luz. Seu avô Jorge dizia que você brilha que nem a Barca do Sol de manhã. Leva um pão para a estrada."),
                ("Maithe, you count your cousins before you count the beasts. That's how your grandpa did it on the caravan road.", "Maithe, você conta os primos antes de contar os bichos. Era assim que o seu avô fazia na estrada das caravanas."),
                ("Ícaro, the little radio you built works fine: I hear the news of all five of you every night.", "Ícaro, o radinho que você montou funciona direitinho: escuto a notícia de vocês cinco toda noite."),
                ("Lavine, my youngest, the plov is still warm. Eat, then tell me about the lilac mist again.", "Lavine, minha caçula, o plov ainda está quentinho. Come, depois me conta de novo da névoa lilás."),
                ("The tandir is hot and the door is open. Whoever arrives hungry leaves fed.", "O tandir está quente e a porta aberta. Quem chega com fome sai alimentado."));

            AddResident(entries, "socorro_vila", "Ramrat the Lampkeeper", "Ramrat, a Guardiã da Lâmpada",
                ("A lamp for the one who walks in front, Joaquim. A light behind the shield is still a light.", "Uma lamparina para quem vai na frente, Joaquim. Luz atrás do escudo ainda é luz."),
                ("You and I keep the same flame, Maria. Mine stays in the square, yours goes out on the road.", "Você e eu guardamos a mesma chama, Maria. A minha fica na praça, a sua vai para a estrada."),
                ("Every time you leave, Maithe, I light one for each cousin. Five little flames in a row.", "Toda vez que vocês saem, Maithe, eu acendo uma para cada primo. Cinco chaminhas enfileiradas."),
                ("No, Ícaro, the lamp doesn't need a switch. It needs someone who remembers to light it.", "Não, Ícaro, a lâmpada não precisa de interruptor. Precisa de alguém que lembre de acender."),
                ("Your grandpa Jorge lit the first lamp of this square, Lavine. I just never let it go out.", "Seu avô Jorge acendeu a primeira lâmpada desta praça, Lavine. Eu só nunca deixei apagar."),
                ("Whoever leaves the village takes a little of the lamp's light. Ask for the blessing before the road.", "Quem sai da vila leva um pouco da luz da lâmpada. Peça a bênção antes da estrada."));

            AddResident(entries, "xroshtag", "Master Xroshtag", "Mestre Xroshtag",
                ("JOAQUIM! Sing the Bema hymn with me: a guardian's voice must reach the last house!", "JOAQUIM! Canta o hino do Bema comigo: voz de guardião tem que chegar na última casa!"),
                ("Maria, the Call needs an Answer. My apprentice is shy; sing the answer with him, will you?", "Maria, o Chamado precisa de Resposta. Meu aprendiz é tímido; canta a resposta com ele?"),
                ("I call, the mountains answer! Maithe, you call your cousins the same way, I've heard you.", "Eu chamo, a serra responde! Maithe, você chama os primos do mesmo jeito, já ouvi."),
                ("A machine to make my voice louder, Ícaro? My voice is already louder than any machine!", "Uma máquina para deixar minha voz mais alta, Ícaro? Minha voz já é mais alta que qualquer máquina!"),
                ("Lavine, Grandpa Jorge sang off-key and proud at every Bema. Sing like him!", "Lavine, o Vô Jorge cantava desafinado e orgulhoso em todo Bema. Canta igual a ele!"),
                ("The Call goes out from this square to the whole Dawn Isle. Who answers?", "O Chamado sai desta praça para a Ilha da Aurora inteira. Quem responde?"));

            AddResident(entries, "padwaxtag", "Padwaxtag the Answer", "Padwaxtag, a Resposta",
                ("Y-you're very brave, Joaquim. I only answer when my master calls...", "V-você é muito corajoso, Joaquim. Eu só respondo quando o mestre chama..."),
                ("Maria... if you sing the answer with me, I won't be scared of my own voice.", "Maria... se você cantar a resposta comigo, eu não fico com medo da minha voz."),
                ("Maithe, how do you call so loud? I practise every day and only the birds answer.", "Maithe, como você chama tão alto? Eu treino todo dia e só os passarinhos respondem."),
                ("Ícaro, could a little machine sing the high part? ...No, never mind, the master would hear.", "Ícaro, será que uma maquininha canta a parte aguda? ...Não, esquece, o mestre ia ouvir."),
                ("Lavine, you're the youngest and you're not shy at all. Teach me?", "Lavine, você é a caçula e não tem vergonha nenhuma. Me ensina?"),
                ("When the Call goes out, someone has to answer. Today it's me... I think.", "Quando o Chamado sai, alguém tem que responder. Hoje sou eu... acho."));

            AddResident(entries, "patig", "Patig the Tracker", "Patig, o Rastreador",
                ("A guardian walks loud, Joaquim. Out in the tugai, step on the moss, not on the dry leaves.", "Guardião anda fazendo barulho, Joaquim. No tugai, pisa no musgo, não na folha seca."),
                ("Hunting is honouring the light of the animal, Maria. You'd understand that better than anyone.", "Caçar é honrar a luz do bicho, Maria. Você entende isso melhor que ninguém."),
                ("Maithe, you read the wind; I read the mud. Between the two of us, nothing gets lost in the woods.", "Maithe, você lê o vento; eu leio a lama. Entre nós dois, nada se perde no bosque."),
                ("No, Ícaro, a drone doesn't read tracks. It scares off whatever made them.", "Não, Ícaro, drone não lê pegada. Ele espanta quem deixou a pegada."),
                ("These little tracks here are a fox's, Lavine. Your grandpa Jorge taught me that one.", "Essas pegadinhas aqui são de raposa, Lavine. Foi o seu avô Jorge que me ensinou."),
                ("Every track is a story the animal left behind. Today's prey is on the hunting board.", "Todo rastro é uma história que o bicho deixou para trás. A presa do dia está no quadro de caça."));

            // ---- Qocho: Náfsha the potter (resident id "rosa") ----
            AddResident(entries, "rosa", "Náfsha the Potter", "Náfsha, a Oleira",
                ("A clay jar is like a shield, Joaquim: it holds what matters and doesn't complain about the heat.", "Jarra de barro é que nem escudo, Joaquim: segura o que importa e não reclama do calor."),
                ("Maria, put your hand on the wheel. See? The clay calms down just like your patients.", "Maria, põe a mão no torno. Viu? O barro acalma igual aos seus doentes."),
                ("A water jar for the crossing, Maithe, one per cousin. The eldest carries the biggest, I know.", "Uma jarra d'água para a travessia, Maithe, uma por primo. A mais velha carrega a maior, eu sei."),
                ("A pedal wheel with gears, Ícaro? ...Draw it in the clay and I'll think about it.", "Um torno de pedal com engrenagem, Ícaro? ...Desenha no barro que eu penso."),
                ("Your grandma Edna still uses the black pot I made for her wedding, Lavine. Tell her I said hello.", "A sua avó Edna ainda usa a panela preta que eu fiz para o casamento dela, Lavine. Manda um abraço."),
                ("I sing to the rhythm of the wheel: red clay, cool water, the whole desert drinks from my jars.", "Canto no compasso do torno: barro vermelho, água fresca, o deserto inteiro bebe das minhas jarras."));

            // ---- Copas de Garajonay (the elfos-da-mata) ----
            AddResident(entries, "gara", "Gara, Warden of the Canopy", "Gara, a Guardiã das Copas",
                ("Up here a shield won't stop a fall, Joaquim. Hold the rope with both hands, then be brave.", "Aqui em cima escudo não segura queda, Joaquim. Segura a corda com as duas mãos, depois seja corajoso."),
                ("Your light makes the mist glow, Maria. The laurels lean towards you when you cross the bridge.", "A sua luz faz a névoa brilhar, Maria. Os louros se inclinam para você quando atravessa a ponte."),
                ("Maithe, you count the cousins on every bridge. Jonay noticed. So did I.", "Maithe, você conta os primos em cada ponte. O Jonay reparou. Eu também."),
                ("No nails in the trunks, Ícaro, please. The bridges are tied, never nailed. The laurels feel it.", "Nada de prego nos troncos, Ícaro, por favor. As pontes são amarradas, nunca pregadas. O louro sente."),
                ("Your grandpa Jorge crossed these bridges once, Lavine, singing out of tune. The mist still remembers.", "Seu avô Jorge atravessou estas pontes uma vez, Lavine, cantando desafinado. A névoa ainda lembra."),
                ("The laurels drink the cloud and the cloud remembers everything. Walk lightly on the bridges.", "O louro bebe a nuvem e a nuvem lembra de tudo. Pise de leve nas pontes."));

            AddResident(entries, "jonay", "Jonay the Archer", "Jonay, o Arqueiro",
                ("Your shield, my arrows, Joaquim: whoever comes up the trunk meets both.", "Seu escudo, minhas flechas, Joaquim: quem subir pelo tronco encontra os dois."),
                ("Maria, an archer shoots better calm. Your light calms even me.", "Maria, arqueiro atira melhor calmo. Sua luz acalma até a mim."),
                ("Maithe! Breathe out, then let go. Again. You already shoot better than half my apprentices.", "Maithe! Solta o ar, depois a corda. De novo. Você já atira melhor que metade dos meus aprendizes."),
                ("An arrow with a little propeller, Ícaro? ...It turned left. Arrows don't turn left.", "Uma flecha com hélice, Ícaro? ...Ela virou para a esquerda. Flecha não vira para a esquerda."),
                ("A small bow for the youngest, Lavine. Don't aim at the lanterns, they're cocoons.", "Um arco pequeno para a caçula, Lavine. Não mira nas lanternas, são casulos."),
                ("A bowstring of laurel fibre and a quiet hand: the canopy has no better guard.", "Corda de fibra de louro e mão quieta: as copas não têm guarda melhor."));

            AddResident(entries, "dacil", "Dácil the Herbalist", "Dácil, a Ervateira",
                ("For the one who takes the hits, Joaquim: a mist potion. It tastes of rain, drink it anyway.", "Para quem leva as pancadas, Joaquim: poção de névoa. Tem gosto de chuva, bebe assim mesmo."),
                ("Maria, a healer from the Vila! Mariam taught me her mint tea; will you teach me the light?", "Maria, curandeira da Vila! A Mariam me ensinou o chá de hortelã; você me ensina a luz?"),
                ("The rope for the bridges is on my counter, Maithe. One for each cousin: the eldest checks the knots.", "A corda das pontes está no meu balcão, Maithe. Uma para cada primo: a mais velha confere os nós."),
                ("A machine that catches mist, Ícaro? The laurels already do it. Look at the leaves dripping.", "Uma máquina de pegar névoa, Ícaro? O louro já faz isso. Olha as folhas pingando."),
                ("Grandma Edna sent me pomegranate seeds once, Lavine. They grew up here, in the mist. Want to see?", "A Vó Edna me mandou sementes de romã uma vez, Lavine. Nasceram aqui em cima, na névoa. Quer ver?"),
                ("Herbs, mist potions and the canopy rope. The counter changes every day, like the cloud.", "Ervas, poções de névoa e a corda das copas. O balcão muda todo dia, igual à nuvem."));

            AddResident(entries, "bencomo", "Bencomo the Old Chief", "Bencomo, o Velho Chefe",
                ("A guardian of the ground among the people of the trees. Bencomo greets you, Joaquim.", "Um guardião do chão entre o povo das árvores. Bencomo te saúda, Joaquim."),
                ("Maria, the light you carry is the same the laurels drink. You are welcome at my fire.", "Maria, a luz que você carrega é a mesma que os louros bebem. Você é bem-vinda na minha fogueira."),
                ("The eldest who listens before she speaks. Our people admire that, Maithe.", "A mais velha que escuta antes de falar. O nosso povo admira isso, Maithe."),
                ("Jorge brought me a whistle he had carved himself, Ícaro. You have his hands, boy.", "O Jorge me trouxe um apito que ele mesmo entalhou, Ícaro. Você tem as mãos dele, menino."),
                ("Tell Edna that the old chief still keeps her place by the fire, Lavine.", "Diz para a Edna que o velho chefe ainda guarda o lugar dela perto do fogo, Lavine."),
                ("We do not fell trees and we do not forget friends. That is the whole law of the canopy.", "Não derrubamos árvores e não esquecemos amigos. Essa é toda a lei das copas."));

            AddResident(entries, "tinguaro", "Tinguaro the Tracker", "Tinguaro, o Rastreador",
                ("A heavy step, Joaquim, but an honest one. The boars hear you coming; that's not always bad.", "Passo pesado, Joaquim, mas honesto. Os javalis te ouvem chegando; nem sempre isso é ruim."),
                ("Maria, even a dimmed beast leaves a soft track. It wants to be found.", "Maria, até bicho apagado deixa rastro macio. Ele quer ser encontrado."),
                ("Patig of the Vila told me about you, Maithe. Moss or mud, a good eye is a good eye.", "O Patig da Vila me falou de você, Maithe. Musgo ou lama, olho bom é olho bom."),
                ("Your drone saw the boar, Ícaro? I saw it yesterday, by the moss. Slower, but no noise.", "Seu drone viu o javali, Ícaro? Eu vi ontem, pelo musgo. Mais devagar, mas sem barulho."),
                ("Lavine, put your hand here. Still warm: the lizard passed a moment ago.", "Lavine, põe a mão aqui. Ainda está quente: o lagarto passou agora há pouco."),
                ("Turned moss, broken fern, a dry drop on the leaf. The forest floor writes everything down.", "Musgo revirado, samambaia quebrada, gota seca na folha. O chão da mata anota tudo."));

            // ---- Taa Pwani (Swahili sailors and merchants of the Costa das Monções) ----
            AddResident(entries, "liyongo", "Captain Liyongo, the Nakhoda", "Capitão Liyongo, o Nakhoda",
                ("Joaquim, they say a guardian doesn't sink in the mangrove mud. On my deck, though, everybody learns to swim first.", "Joaquim, dizem que guardião não afunda no lodo do mangue. No meu convés, porém, todo mundo aprende a nadar primeiro."),
                ("Maria, sailors say a healer aboard is worth two sails. Come with us to the reef when the monsoon turns.", "Maria, os marinheiros dizem que curandeira a bordo vale por duas velas. Vem com a gente até o recife quando a monção virar."),
                ("Maithe, did your Grandpa Jorge tell you of Liyongo, the archer-poet of this coast? I carry his name, not his bow. The bow I leave to you.", "Maithe, o Vô Jorge te contou de Liyongo, o arqueiro-poeta desta costa? Eu carrego o nome dele, não o arco. O arco eu deixo com você."),
                ("Ícaro, a motor on a dhow? The monsoon has pushed these sails for a thousand years without a single gear. ...How much faster, you said?", "Ícaro, motor num dhow? A monção empurra estas velas há mil anos sem uma engrenagem. ...Quanto mais rápido, você disse?"),
                ("Lavine, the Moon Ship sails over us every night and you call yourself its boatwoman? Then you outrank me, little one.", "Lavine, a Barca da Lua navega sobre nós toda noite e você se diz barqueira dela? Então você manda mais que eu, pequena."),
                ("The monsoon blows one way half the year and back the other half. A good captain waits for the right wind.", "A monção sopra para um lado meio ano e volta no outro meio. Capitão bom espera o vento certo."));

            AddResident(entries, "mwana", "Mwana the Pearl Merchant", "Mwana, a Mercadora de Pérolas",
                ("Root boots for the guardian, Joaquim? Ah, you don't sink. Then take a pair for your sister.", "Botas de raiz para o guardião, Joaquim? Ah, você não afunda. Então leva um par para a sua irmã."),
                ("Maria, a pearl is a grain of sand the oyster wrapped in light until it stopped hurting. You understand that better than anyone.", "Maria, pérola é um grão de areia que a ostra embrulhou em luz até parar de doer. Você entende disso melhor que ninguém."),
                ("A dhow passage for each cousin, Maithe: five cedar tablets, and the eldest keeps them. Karibu, welcome aboard.", "Uma passagem de dhow para cada primo, Maithe: cinco tábuas de cedro, e a mais velha guarda. Karibu, bem-vindos a bordo."),
                ("No, Ícaro, the breathing shell has no valve to open. You breathe, it sings. Don't take it apart.", "Não, Ícaro, a concha respirável não tem válvula para abrir. Você respira, ela canta. Não desmonta."),
                ("Your Grandma Edna wears a pearl my mother sold to Grandpa Jorge, Lavine. He haggled for an hour and paid full price anyway.", "A sua Vó Edna usa uma pérola que a minha mãe vendeu para o Vô Jorge, Lavine. Ele pechinchou uma hora e pagou o preço cheio mesmo assim."),
                ("Pearls, passages and the gear of the whole coast. The counter turns with the tide, every day.", "Pérolas, passagens e o equipamento da costa inteira. O balcão vira com a maré, todo dia."));

            AddResident(entries, "nuru", "Nuru, Keeper of the Coral Light", "Nuru, a Guardiã do Farol",
                ("You stand in front of your cousins the way my tower stands in front of the sea, Joaquim. We both keep watch.", "Você fica na frente dos primos do jeito que a minha torre fica na frente do mar, Joaquim. Nós dois vigiamos."),
                ("Nuru means light, Maria. We are almost namesakes, you and I.", "Nuru quer dizer luz, Maria. Somos quase xarás, você e eu."),
                ("Every dhow that comes home counts itself by my lamp, Maithe. Just the way you count your cousins.", "Todo dhow que volta para casa se conta pela minha lâmpada, Maithe. Igualzinho você conta os primos."),
                ("A mirror behind the flame, Ícaro? ...Oh. Oh, now it reaches the reef. Don't tell the captain it was your idea.", "Um espelho atrás da chama, Ícaro? ...Ah. Ah, agora chega até o recife. Não conta para o capitão que a ideia foi sua."),
                ("Taa Pwani means the Coast of the Lamp, Lavine. I hear your Grandma Edna's square has a lamp too; ours just stands taller.", "Taa Pwani quer dizer a Costa da Lâmpada, Lavine. Ouvi dizer que a praça da Vó Edna também tem uma lâmpada; a nossa só é mais alta."),
                ("The shadow put out the glow of the reef, but not my lamp. While it burns, the dhows find their way home.", "A sombra apagou o brilho do recife, mas não a minha lâmpada. Enquanto ela queimar, os dhows acham o caminho de casa."));

            AddResident(entries, "zawadi", "Zawadi the Root Weaver", "Zawadi, o Trançador de Raízes",
                ("You walk the mud without sinking, Joaquim? Then you don't need my boots... but your feet will thank you anyway.", "Você anda no lodo sem afundar, Joaquim? Então nem precisa das minhas botas... mas o seu pé agradece assim mesmo."),
                ("The mangrove roots are alive, Maria. I only braid the ones that fall; your light helps them take root again.", "As raízes do mangue são vivas, Maria. Eu só trancio as que caem; a sua luz ajuda elas a pegar de novo."),
                ("Maithe, measure your cousins' feet for me. The boots have to fit five heroes, not one.", "Maithe, mede o pé dos primos para mim. A bota tem que servir em cinco heróis, não em um."),
                ("A braiding machine, Ícaro? The roots braid themselves if you let them. I only tell them where.", "Uma máquina de trançar, Ícaro? As raízes se trançam sozinhas se você deixar. Eu só digo onde."),
                ("Lavine, it was Grandpa Jorge who brought me my first root knife. Tell Grandma Edna the blade is still sharp.", "Lavine, foi o Vô Jorge que me trouxe a minha primeira faca de cortar raiz. Diz para a Vó Edna que o fio ainda está bom."),
                ("Living-root boots grip the mud of Liyongo's mangroves. Pole pole, slowly: a good braid takes a whole tide.", "Bota de raiz viva agarra o lodo dos mangues de Liyongo. Pole pole, devagar: trança boa leva uma maré inteira."));

            AddResident(entries, "mbaraka", "Mbaraka the Star Navigator", "Mbaraka, o Navegador das Estrelas",
                ("Strong arms, Joaquim, but a sailor rows with his eyes first. Look up: the Moon Ship shows the way.", "Braço forte, Joaquim, mas marinheiro rema primeiro com os olhos. Olha para cima: a Barca da Lua mostra o caminho."),
                ("When the moon is full, Maria, she is carrying the light back home. A healer's work, if you ask me.", "Quando a lua está cheia, Maria, ela está levando a luz de volta para casa. Trabalho de curandeira, se quer saber."),
                ("Maithe, the eldest reads the sky for the others. I did it for my brothers sixty monsoons ago.", "Maithe, a mais velha lê o céu pelos outros. Eu fazia isso pelos meus irmãos sessenta monções atrás."),
                ("Your compass spins near the reef, Ícaro? The stars never do. Your Grandpa Jorge learned that on my deck.", "A sua bússola gira perto do recife, Ícaro? As estrelas nunca giram. O seu avô Jorge aprendeu isso no meu convés."),
                ("The boatwoman of the Moon! Lavine, ask the Moon Ship for a gentle monsoon this year. The old man asks.", "A barqueira da Lua! Lavine, pede para a Barca da Lua mandar uma monção mansa este ano. O velho pede."),
                ("Sixty monsoons I have read these stars. The Moon Ship fills, empties and fills again: the light always comes home.", "Sessenta monções eu li estas estrelas. A Barca da Lua enche, esvazia e enche de novo: a luz sempre volta para casa."));

            // ---- Tambor Rubro (orcs of the Red Drum; names from the Hurrian tales and Hittite/Mitanni history) ----
            AddResident(entries, "gurparanzah", "Gurparanzah, the Archer Chief", "Gurparanzah, a Chefe-Arqueira",
                ("Joaquim, you stand in front of your cousins without being asked. In this camp that is called a shield-brother. Sit at my fire.", "Joaquim, você fica na frente dos primos sem ninguém pedir. Neste acampamento isso se chama irmão de escudo. Senta na minha fogueira."),
                ("Maria, the drums say the Living Fire warms, it does not burn. Your light says the same thing without a single beat.", "Maria, os tambores dizem que o Fogo Vivo aquece, não queima. A sua luz diz a mesma coisa sem uma batida só."),
                ("Maithe, draw. Slower. Breathe out with the drum... There. Now you shoot like a Red Drum archer.", "Maithe, puxa. Mais devagar. Solta o ar com o tambor... Isso. Agora você atira como arqueira do Tambor Rubro."),
                ("Ícaro, your trial waits in my hall when you are ready: a fire-arrow that flies true. Gears allowed. Shortcuts, no.", "Ícaro, a sua prova te espera no meu salão quando você estiver pronto: uma flecha de fogo que voe reta. Engrenagem pode. Atalho, não."),
                ("Lavine, Grandpa Jorge gave me his word once and kept it through a sandstorm. You have his stubborn chin.", "Lavine, o Vô Jorge me deu a palavra uma vez e cumpriu no meio de uma tempestade de areia. Você tem o queixo teimoso dele."),
                ("Some clans took Āz's tin crowns and fight for gold. They are still ours. We want them home, not beaten.", "Alguns clãs aceitaram as coroinhas de lata de Āz e brigam por ouro. Continuam sendo nossos. A gente quer eles em casa, não derrotados."));

            AddResident(entries, "kessi", "Kessi the Hunter", "Kessi, o Caçador",
                ("Joaquim, you walk like a wolf on a full belly: no hurry, no noise. Good. The hawks won't hear you coming.", "Joaquim, você anda feito lobo de barriga cheia: sem pressa, sem barulho. Bom. Os gaviões não vão te ouvir chegar."),
                ("Maria, the hides on my rack are from the beasts the camp ate, nothing more. Will you bless the next hunt with me?", "Maria, os couros do meu varal são dos bichos que o acampamento comeu, nada mais. Abençoa a próxima caçada comigo?"),
                ("Maithe, three beats to aim, one to loose. That's how my mother taught me, and hers taught her.", "Maithe, três batidas para mirar, uma para soltar. Foi assim que a minha mãe me ensinou, e a dela ensinou a ela."),
                ("Ícaro, a snare that resets itself? ...Make it let the small ones go, and I'll set it by the canyon.", "Ícaro, uma armadilha que se arma sozinha? ...Faz ela soltar os pequenos, e eu ponho na beira do cânion."),
                ("Lavine, you can tell when a hawk is just hungry? Then you are a better hunter than I am. Grandma Edna would be proud.", "Lavine, você sabe quando um gavião só está com fome? Então você caça melhor que eu. A Vó Edna ia ficar orgulhosa."),
                ("The canyons echo: in the Red Drum Badlands the beasts hear your steps twice. Walk on the sand, not the stone.", "Os cânions ecoam: nos Ermos do Tambor Rubro os bichos escutam o seu passo duas vezes. Pisa na areia, não na pedra."));

            AddResident(entries, "appu", "Appu the Kind Merchant", "Appu, o Mercador Gentil",
                ("Joaquim! My little ones want to see the guardian lift the water barrel. Just once? They've been waiting all morning.", "Joaquim! Os meus pequenos querem ver o guardião levantar o barril de água. Só uma vez? Estão esperando desde cedo."),
                ("Maria, the youngest has a cough. Would you look at her? I traded a whole bolt of red leather for honey and it didn't help.", "Maria, a menorzinha está com tosse. Você olha ela? Troquei uma peça inteira de couro vermelho por mel e não adiantou."),
                ("Maithe, I dreamed for years of a big family. Now I have nineteen orphans and a camp full of aunts. Dreams come in odd sizes.", "Maithe, sonhei anos com uma família grande. Agora tenho dezenove órfãos e um acampamento cheio de tias. Sonho vem em tamanho esquisito."),
                ("Ícaro, the children broke the toy you made them. ...No, they loved it. That is why it broke. Make two?", "Ícaro, as crianças quebraram o brinquedo que você fez. ...Não, elas amaram. Foi por isso que quebrou. Faz dois?"),
                ("Lavine, Grandpa Jorge always paid me double and pretended he couldn't count. Tell Grandma Edna I noticed.", "Lavine, o Vô Jorge sempre me pagava em dobro e fingia que não sabia contar. Diz para a Vó Edna que eu percebi."),
                ("Bread for the road, water for the wolves, a place by the fire. Nobody leaves my tent empty-handed.", "Pão para a estrada, água para os lobos, um lugar na fogueira. Ninguém sai da minha tenda de mão vazia."));

            AddResident(entries, "ashmunikal", "Ashmunikal, Matriarch of the Drums", "Ashmunikal, a Matriarca dos Tambores",
                ("Joaquim, hold the big drum steady while I tune it. Your arms are the only ones here that don't shake.", "Joaquim, segura o tambor grande firme enquanto eu afino. Os seus braços são os únicos aqui que não tremem."),
                ("Maria, the lamp on my tower and the light in your hands are cousins, child. Both keep the dark outside the palisade.", "Maria, a lâmpada da minha torre e a luz das suas mãos são primas, menina. As duas deixam o escuro do lado de fora da paliçada."),
                ("Maithe, every ten turns of the hourglass the drums play for everyone in the camp. Come play with us; the eldest leads.", "Maithe, a cada dez voltas da ampulheta os tambores tocam para todo mundo no acampamento. Vem tocar com a gente; a mais velha puxa."),
                ("Ícaro, you counted the beats and found a pattern? Of course there is. It is a hundred years old. Keep counting.", "Ícaro, você contou as batidas e achou um padrão? Claro que tem. Tem cem anos. Continua contando."),
                ("Lavine, Grandma Edna once danced to my drum until sunrise. Ask her. She'll say it was Grandpa Jorge who wouldn't stop.", "Lavine, a Vó Edna uma vez dançou no meu tambor até o sol nascer. Pergunta para ela. Ela vai dizer que foi o Vô Jorge que não parava."),
                ("The drums mark the hours and wake the Living Fire of the forge. While they beat, the shadow keeps its distance.", "Os tambores marcam a hora e acordam o Fogo Vivo da forja. Enquanto eles batem, a sombra fica longe."));

            AddResident(entries, "tushratta", "Tushratta the Drum Smith", "Tushratta, o Ferreiro do Tambor",
                ("Joaquim, a guardian with a dented shield? Give it here. I'll beat it straight to the rhythm of the drum.", "Joaquim, guardião de escudo amassado? Passa para cá. Eu desamasso no ritmo do tambor."),
                ("Maria, the forge's fire was asleep until the drums woke it. Fire and light are family, like you and your brother.", "Maria, o fogo da forja estava dormindo até os tambores acordarem ele. Fogo e luz são família, igual você e o seu irmão."),
                ("Maithe, bronze arrowheads, balanced on my own finger. For the eldest, the first of the batch.", "Maithe, pontas de flecha de bronze, equilibradas no meu próprio dedo. Para a mais velha, a primeira da fornada."),
                ("Ícaro, you want to see how the bellows work? Pump. Faster. ...Now you know why my arms are this size.", "Ícaro, quer ver como o fole funciona? Bombeia. Mais rápido. ...Agora você sabe por que o meu braço é desse tamanho."),
                ("Lavine, going up to Namrael's Throne? Take theriac. The salt there bites, and Grandpa Jorge would scold me if I forgot to tell you.", "Lavine, vai subir para o Trono de Namrael? Leva teriaga. O sal de lá morde, e o Vô Jorge ia brigar comigo se eu esquecesse de avisar."),
                ("Copper, bronze and iron, tempered to the beat of the drum. Repairs, stones of the Captured Light, theriac for the Throne.", "Cobre, bronze e ferro, temperados na batida do tambor. Conserto, pedras da Luz Presa, teriaga para o Trono."));

            // ---- Salões de Bronze de Napirasu (dwarves; Elamite names: queen Napirasu, Untash-Napirisha, Puzur-Inshushinak, Shilhak-Inshushinak, Kidin-Hutran) ----
            AddResident(entries, "napirasu", "Napirasu, the Master Founder", "Napirasu, a Mestra Fundidora",
                ("Joaquim, you stand like a pillar, and a pillar is the most honest thing a dwarf can say about anyone. Hold this crucible for me?", "Joaquim, você fica parado feito pilar, e pilar é a coisa mais honesta que um anão pode dizer de alguém. Segura este cadinho para mim?"),
                ("Maria, put your hand on the crystal of the Grande Forja. ...Did you feel it? It's not dead. It's only asleep, waiting for Light.", "Maria, põe a mão no cristal da Grande Forja. ...Sentiu? Não está morto. Só está dormindo, esperando a Luz."),
                ("Maithe, I cast arrowheads for the eldest of the cousins. Bronze, cold-forged. They will be better the day the forge is lit again.", "Maithe, fundi pontas de flecha para a mais velha dos primos. Bronze, forjado a frio. Vão ficar melhores no dia em que a forja acender de novo."),
                ("Ícaro, you want to know the alloy? Nine of copper, one of tin, and a dwarf who knows when to stop. The last part you can't write down.", "Ícaro, quer saber a liga? Nove de cobre, um de estanho, e um anão que sabe a hora de parar. A última parte não dá para escrever."),
                ("Lavine, Grandma Edna sent me a jar of apricot jam the winter our forge went out. Tell her it kept the whole hall warm.", "Lavine, a Vó Edna me mandou um pote de doce de damasco no inverno em que a nossa forja apagou. Diz para ela que esquentou o salão inteiro."),
                ("The Grande Forja was lit by the Light held in a crystal. The shadow put it out. We forge in the cold now, but we haven't forgotten how.", "A Grande Forja era acesa pela Luz presa num cristal. A sombra apagou. Agora a gente forja no frio, mas não esqueceu como."));

            AddResident(entries, "untash", "Untash, the Master Builder", "Untash, o Mestre de Obras",
                ("Joaquim, measure twice, cut once. And hold the ladder. ...No, the other ladder. Thank you.", "Joaquim, mede duas vezes, corta uma. E segura a escada. ...Não, a outra escada. Obrigado."),
                ("Maria, the halls were cut so the light of the forge reached every door. Now it doesn't. Walk them with your light and tell me where it's darkest.", "Maria, os salões foram cortados para a luz da forja chegar em toda porta. Agora não chega. Anda por eles com a sua luz e me diz onde está mais escuro."),
                ("Maithe, a good arch and a good bow work the same way: the strain goes where you tell it to. You understood that before I did.", "Maithe, um arco de pedra e um arco de flecha funcionam igual: a força vai para onde você manda. Você entendeu isso antes de mim."),
                ("Ícaro, I've been drawing a new bellows for the Grande Forja for three years. You found my mistake in three minutes. I'm not happy. I'm grateful. Mostly.", "Ícaro, faz três anos que eu desenho um fole novo para a Grande Forja. Você achou o meu erro em três minutos. Não estou feliz. Estou grato. Quase."),
                ("Lavine, Grandpa Jorge fixed a hinge in this hall with a piece of string forty years ago. It still holds. It still annoys me.", "Lavine, o Vô Jorge consertou uma dobradiça deste salão com um pedaço de barbante quarenta anos atrás. Segura até hoje. Me irrita até hoje."),
                ("Pillars first, then the roof, then the door. Halls fall when someone starts with the door.", "Primeiro os pilares, depois o teto, depois a porta. Salão cai quando alguém começa pela porta."));

            AddResident(entries, "puzur", "Puzur, Keeper of the Runes", "Puzur, o Guardião das Runas",
                ("Joaquim, this rune means \"I stay\". We carve it on the threshold of every hall. I think it was carved for you.", "Joaquim, esta runa quer dizer \"eu fico\". A gente grava na soleira de todo salão. Acho que foi gravada para você."),
                ("Maria, the rune gates light one line at a time, like someone learning to read. Yours lit all at once when you walked in.", "Maria, os portões de runa acendem uma linha por vez, como quem aprende a ler. Os seus acenderam todos de uma vez quando você entrou."),
                ("Maithe, a rune of wind on the bowstring. Free, the first one, for the eldest. Don't tell Untash, he thinks I give too much away.", "Maithe, uma runa de vento na corda do arco. De graça, a primeira, para a mais velha. Não conta para o Untash, ele acha que eu dou demais."),
                ("Ícaro, the runes are not magic words. They are instructions for the Light. You'd make a good keeper; you already read the instructions.", "Ícaro, as runas não são palavras mágicas. São instruções para a Luz. Você daria um bom guardião; você já lê as instruções."),
                ("Lavine, Grandma Edna's name is carved on the guest stone by the gate. Vô Jorge's too, but smaller. He asked for it smaller.", "Lavine, o nome da Vó Edna está gravado na pedra dos hóspedes perto do portão. O do Vô Jorge também, mas menor. Ele pediu menor."),
                ("Bronze runes, stones of the Captured Light, picks and lamps for the deep halls, ginger tea for the Mazans' summit. Choose slowly.", "Runas de bronze, pedras da Luz Presa, picaretas e lamparinas para os salões fundos, chá de gengibre para o cume dos Mazans. Escolham devagar."));

            AddResident(entries, "shilhak", "Shilhak the Miner", "Shilhak, o Minerador",
                ("Joaquim, you go first in the seam. If the roof speaks, you'll hear it before the rest of us. That's what shoulders like yours are for.", "Joaquim, você vai na frente no veio. Se o teto falar, você escuta antes de todo mundo. É para isso que serve um ombro desse."),
                ("Maria, sing to the rock with me before the pick. It isn't a prayer, it's manners. The rock was here first.", "Maria, canta para a pedra comigo antes da picareta. Não é reza, é educação. A pedra chegou primeiro."),
                ("Maithe, in the seam you can't shoot far. Count your steps instead. Eleven from the lamp to the bend. Remember it.", "Maithe, no veio não dá para atirar longe. Conta os passos em vez disso. Onze da lamparina até a curva. Guarda isso."),
                ("Ícaro, a lamp that hangs from the helmet and turns with your head? ...I'll wear it if you make it. I'll complain, but I'll wear it.", "Ícaro, uma lamparina que pendura no capacete e vira com a cabeça? ...Eu uso se você fizer. Vou reclamar, mas uso."),
                ("Lavine, you heard the bats crying? They're not bad. They nested in the forge's last warm cracks and the cold went to their heads. Grandpa Jorge would say the same.", "Lavine, você ouviu os morcegos chorando? Eles não são maus. Fizeram ninho nas últimas fendas quentes da forja e o frio subiu à cabeça. O Vô Jorge diria igual."),
                ("Copper above, tin below, and in the middle, if you're lucky, a vein of something that still remembers the Light.", "Cobre em cima, estanho embaixo, e no meio, com sorte, um veio de alguma coisa que ainda lembra a Luz."));

            AddResident(entries, "kidin", "Kidin, Cook of the Hall", "Kidin, o Cozinheiro do Salão",
                ("Joaquim! A guardian eats twice. Sit. No, sit properly. Lentils with mountain onion, the bowl the size of your shield.", "Joaquim! Guardião come duas vezes. Senta. Não, senta direito. Lentilha com cebola da montanha, a tigela do tamanho do seu escudo."),
                ("Maria, since the forge went cold my oven is the warmest place in the halls. The children sleep beside it. Will you stay for the bread?", "Maria, desde que a forja apagou, o meu forno é o lugar mais quente dos salões. As crianças dormem do lado dele. Fica para o pão?"),
                ("Maithe, the eldest gets the crust. That's the rule of my kitchen, and the crust is the best part, so stop arguing.", "Maithe, a mais velha fica com a casca. É a regra da minha cozinha, e a casca é a melhor parte, então para de discutir."),
                ("Ícaro, you made my spit turn by itself and now the roast is perfect and I have nothing to do. I'm going to need another invention.", "Ícaro, você fez o meu espeto girar sozinho e agora o assado sai perfeito e eu não tenho nada para fazer. Vou precisar de outra invenção."),
                ("Lavine, this is Grandma Edna's apricot bread. She taught me in this very kitchen. Don't tell Napirasu I put honey in it.", "Lavine, este é o pão de damasco da Vó Edna. Ela me ensinou nesta mesma cozinha. Não conta para a Napirasu que eu pus mel."),
                ("A hall without a warm pot is just a hole in the mountain. Eat something before you go down.", "Salão sem panela quente é só um buraco na montanha. Comam alguma coisa antes de descer."));

            // ---- Mercado dos Túneis (kobolds; Sogdian names from the Ancient Letters and the Mug documents) ----
            AddResident(entries, "miwnay", "Miwnay, Mistress of the Fair", "Miwnay, a Mestra da Feira",
                ("Joaquim, stand by my stall a moment. Nobody haggles badly with a guardian watching. Ha! Just teasing. Mostly.", "Joaquim, fica do lado da minha banca um pouquinho. Ninguém pechincha feio com guardião olhando. Ha! Brincadeira. Quase."),
                ("Maria, smell this bottle. Still air from the cracks, the Ar Sereno. It calms whoever breathes it. Take one for your brother.", "Maria, cheira esta garrafa. Ar parado das frestas, o Ar Sereno. Acalma quem respira. Leva uma para o seu irmão."),
                ("Maithe, the fair moves gallery by gallery, one each day. You'll always find us: follow the lanterns of coloured glass.", "Maithe, a feira muda de galeria todo dia, uma por dia. Vocês sempre acham a gente: sigam as lanternas de vidro colorido."),
                ("Ícaro, my Shayn says you fixed her fan with a hairpin. I'm writing that in my letters. I write everything in my letters.", "Ícaro, a minha Shayn diz que você consertou o cata-vento dela com um grampo de cabelo. Vou escrever isso nas minhas cartas. Eu escrevo tudo nas minhas cartas."),
                ("Lavine, Grandma Edna trades me her apricots for bottled air every spring. She says it's for the house. I think it's for Grandpa Jorge's temper.", "Lavine, a Vó Edna me troca damasco por ar engarrafado toda primavera. Ela diz que é para a casa. Eu acho que é para o gênio do Vô Jorge."),
                ("Bottled Ar Sereno, mended scrap, oil that never goes out. A fair price, and a bottle on the house for anyone who brings a lantern back lit.", "Ar Sereno engarrafado, sucata consertada, óleo que nunca apaga. Preço justo, e uma garrafa de brinde para quem devolve uma lanterna acesa."));

            AddResident(entries, "shayn", "Shayn the Mender", "Shayn, a Consertadora",
                ("Joaquim, give me that shield. ...Who hit it, a mountain? I can fix it. I'll fix it and paint a little lantern on it.", "Joaquim, me dá esse escudo. ...Quem bateu nele, uma montanha? Eu conserto. Conserto e pinto uma lanterninha nele."),
                ("Maria, nothing is really broken, Mother says. It just stopped being what it was. Then I make it something else.", "Maria, nada está quebrado de verdade, a mãe diz. Só parou de ser o que era. Aí eu faço virar outra coisa."),
                ("Maithe, a quiver made from an old air pipe. Light, it doesn't rattle, and it whistles a little when you run. I like that.", "Maithe, uma aljava feita de cano de ar velho. Leve, não chacoalha, e assobia um pouco quando você corre. Eu gosto disso."),
                ("Ícaro, the hairpin trick, can you show me again? Slowly. ...Again. I almost had it.", "Ícaro, o truque do grampo, mostra de novo? Devagar. ...De novo. Quase peguei."),
                ("Lavine, Grandpa Jorge brought me a broken music box when I was small. I fixed it. It plays backwards now. He says it's better.", "Lavine, o Vô Jorge me trouxe uma caixinha de música quebrada quando eu era pequena. Eu consertei. Agora ela toca de trás para frente. Ele diz que ficou melhor."),
                ("Bring me what's broken. I'll give you back something that works. Maybe not the same thing.", "Me traz o que quebrou. Eu devolvo uma coisa que funciona. Talvez não a mesma."));

            AddResident(entries, "farnkhund", "Farnkhund the Cart Driver", "Farnkhund, o Vagoneiro",
                ("Joaquim, push my cart up the slope? It's not heavy. It's just... a lot of cart. Thank you, big one.", "Joaquim, empurra o meu vagonete na subida? Não é pesado. É só... muito vagonete. Obrigado, grandão."),
                ("Maria, I know every switch of these tunnels by its squeak. That one, hear it? It squeaks sad since the dark came in.", "Maria, eu conheço cada desvio destes túneis pelo rangido. Aquele, ouviu? Range triste desde que o escuro entrou."),
                ("Maithe, you timed my cart and lost. Tomorrow, from the north gallery to the wheel. Bring your fastest legs.", "Maithe, você cronometrou o meu vagonete e perdeu. Amanhã, da galeria norte até a roda. Traz as suas pernas mais rápidas."),
                ("Ícaro, a brake that doesn't screech? Takhsich has been trying for years. If you do it, I'll name a cart after you.", "Ícaro, um freio que não guincha? O Takhsich tenta faz anos. Se você fizer, eu dou o seu nome a um vagonete."),
                ("Lavine, Grandpa Jorge rode my cart once, all the way to the Wheel Canyon. Screaming. Laughing too. Mostly screaming.", "Lavine, o Vô Jorge andou no meu vagonete uma vez, até o Cânion da Roda. Gritando. Rindo também. Mais gritando."),
                ("Rails north, south, east and west. If it can be carried, I carry it. If it can't, I carry it slower.", "Trilho para o norte, sul, leste e oeste. Se dá para levar, eu levo. Se não dá, eu levo mais devagar."));

            AddResident(entries, "takhsich", "Takhsich the Inventor", "Takhsich, o Inventor",
                ("Joaquim, stand there. No, there. I'm measuring how much wind a guardian blocks. For science. And for the pipes.", "Joaquim, fica ali. Não, ali. Estou medindo quanto vento um guardião segura. Pela ciência. E pelos tubos."),
                ("Maria, the fans pull the Ar Sereno down from the cracks. Without them the tunnels go still, and still air makes people sad. Your light does the same as my fans.", "Maria, os cata-ventos puxam o Ar Sereno das frestas. Sem eles os túneis ficam parados, e ar parado deixa todo mundo triste. A sua luz faz o mesmo que os meus cata-ventos."),
                ("Maithe, an arrow with a little fan on the tail. It flies crooked. Very crooked. ...Do you want it anyway? It's pretty.", "Maithe, uma flecha com um cata-ventinho no rabo. Voa torta. Muito torta. ...Você quer mesmo assim? Ela é bonita."),
                ("Ícaro! Seven blades or five? I say seven. You say five. Let's build both and let the wind decide.", "Ícaro! Sete pás ou cinco? Eu digo sete. Você diz cinco. Vamos fazer os dois e deixar o vento decidir."),
                ("Lavine, Grandma Edna says I talk faster than my fans spin. Grandpa Jorge says that's impossible. They're both right.", "Lavine, a Vó Edna diz que eu falo mais rápido do que os meus cata-ventos giram. O Vô Jorge diz que é impossível. Os dois têm razão."),
                ("Pipes, fans, gears, and one idea too many every morning. The air has to move, or the tunnels forget how to breathe.", "Canos, cata-ventos, engrenagens, e uma ideia a mais toda manhã. O ar tem que andar, senão os túneis esquecem como respirar."));

            // ---- Aratta (povo-touro; Sumerian names from the epics of Enmerkar and Lugalbanda and the Hymn of Shulgi) ----
            AddResident(entries, "enmerkar", "Enmerkar, the Chief Builder", "Enmerkar, o Chefe Construtor",
                ("Joaquim, a guardian with horns would be a fine thing. You have the shoulders already. Help me lift this arch of reed?", "Joaquim, um guardião de chifre ia ser uma beleza. Os ombros você já tem. Me ajuda a levantar este arco de junco?"),
                ("Maria, the Fogo Vivo is a small fire, low, that walks ahead of the grass. Your light is like it: it doesn't burn, it renews.", "Maria, o Fogo Vivo é um fogo pequeno, baixo, que anda na frente do capim. A sua luz é parecida: não queima, renova."),
                ("Maithe, I wrote your name on clay, so the halls remember you. The eldest gets the first tablet. That is the rule.", "Maithe, eu escrevi o seu nome na argila, para os salões lembrarem de você. A mais velha ganha a primeira tabuinha. É a regra."),
                ("Ícaro, you want to know how the clay keeps the words? You press, you dry, you wait. Like everything good.", "Ícaro, quer saber como a argila guarda as palavras? Aperta, seca, espera. Como tudo que é bom."),
                ("Lavine, Grandma Edna once slept in my reed hall during the monsoon. She says the mats sing in the rain. They do.", "Lavine, a Vó Edna dormiu uma vez no meu salão de junco durante a monção. Ela diz que as esteiras cantam na chuva. Cantam mesmo."),
                ("A hall of reed lasts a lifetime if you retie it every year. A city too.", "Salão de junco dura uma vida inteira se você amarrar de novo todo ano. Cidade também."));

            AddResident(entries, "lugalbanda", "Lugalbanda the Wanderer", "Lugalbanda, o Andarilho",
                ("Joaquim, I got lost in the mountains once, alone and sick. A guardian like you would have carried me home. Take a rope, just in case.", "Joaquim, eu me perdi nas montanhas uma vez, sozinho e doente. Um guardião como você teria me carregado para casa. Leva uma corda, por via das dúvidas."),
                ("Maria, when I was lost, a little light in a cave kept me alive. I think about it every time you pass.", "Maria, quando eu me perdi, uma luzinha numa gruta me manteve vivo. Eu lembro dela toda vez que você passa."),
                ("Maithe, this map has a path nobody walks. It goes past the pyramids of Meroë. Want it? You look like someone who walks paths nobody walks.", "Maithe, este mapa tem um caminho que ninguém anda. Passa pelas pirâmides de Meroé. Quer? Você tem cara de quem anda caminho que ninguém anda."),
                ("Ícaro, I found this relic of the Artífices under the sand. It hums when the sun sets. Don't take it apart. ...Fine, take it apart, but put it back.", "Ícaro, achei esta relíquia dos Artífices debaixo da areia. Ela zumbe quando o sol se põe. Não desmonta. ...Tá bom, desmonta, mas monta de novo."),
                ("Lavine, Grandpa Jorge found me on the road the year I came back. He gave me half his bread and didn't ask a thing. I still owe him the other half.", "Lavine, o Vô Jorge me achou na estrada no ano em que eu voltei. Me deu metade do pão dele e não perguntou nada. Ainda devo a outra metade."),
                ("Rope, waterskins, a map with one path too many. Everything a wanderer needs to come back.", "Corda, odre, um mapa com um caminho a mais. Tudo que um andarilho precisa para voltar."));

            AddResident(entries, "shulgi", "Shulgi the Runner", "Shulgi, o Corredor",
                ("Joaquim, a race? You and me, to the horn post and back. ...You carry the shield. That's fair, I carry nothing.", "Joaquim, uma corrida? Você e eu, até o poste de chifre e de volta. ...Você leva o escudo. É justo, eu não levo nada."),
                ("Maria, run with me in the morning, when the Fogo Vivo has passed and the black paths are still warm. The ground pushes you.", "Maria, corre comigo de manhã, quando o Fogo Vivo já passou e os caminhos pretos ainda estão mornos. O chão empurra você."),
                ("Maithe! Finally someone who doesn't stop to breathe. From the gate to the big acacia, now. Go!", "Maithe! Finalmente alguém que não para para respirar. Do portão até a acácia grande, agora. Vai!"),
                ("Ícaro, shoes that run on their own? No. ...Maybe. Show me the drawing.", "Ícaro, sapato que corre sozinho? Não. ...Talvez. Mostra o desenho."),
                ("Lavine, the aurochs calves race me every dawn. They win. Grandpa Jorge says they cheat with four legs.", "Lavine, os bezerros de auroque apostam corrida comigo toda madrugada. Eles ganham. O Vô Jorge diz que eles roubam com quatro pernas."),
                ("From one city to the next in a single day, and back. Ask me how. Better: run with me.", "De uma cidade até a outra num dia só, e de volta. Pergunta como. Melhor: corre comigo."));

            AddResident(entries, "ensuhkeshdanna", "Ensuhkeshdanna, the Friendly Rival", "Ensuhkeshdanna, o Rival Amigo",
                ("Joaquim! A duel of shoves, shield against horns. Whoever leaves the circle buys the milk. Ha!", "Joaquim! Um duelo de empurrão, escudo contra chifre. Quem sair do círculo paga o leite. Ha!"),
                ("Maria, I lost a duel of riddles to Enmerkar once, a long time ago. Now I ask the riddles. Want one? What grows when the fire passes?", "Maria, eu perdi um duelo de charadas para o Enmerkar uma vez, faz muito tempo. Agora eu que faço as charadas. Quer uma? O que cresce quando o fogo passa?"),
                ("Maithe, shoot at my drum from the edge of the square. If it sounds, you win. If it doesn't, you try again. Nobody loses here.", "Maithe, atira no meu tambor da beira da praça. Se ele soar, você ganha. Se não soar, tenta de novo. Aqui ninguém perde."),
                ("Ícaro, you made my drum sound twice as loud? Enmerkar is going to hate it. I love it.", "Ícaro, você fez o meu tambor soar duas vezes mais alto? O Enmerkar vai detestar. Eu adorei."),
                ("Lavine, Grandma Edna says a rival is a friend who arrived early. I didn't understand it for years. Now I do.", "Lavine, a Vó Edna diz que rival é amigo que chegou cedo. Eu levei anos para entender. Agora entendo."),
                ("A duel a day, no one hurt, everyone hungry after. That's how Aratta keeps its horns sharp and its heart soft.", "Um duelo por dia, ninguém se machuca, todo mundo com fome depois. É assim que Aratta mantém os chifres afiados e o coração mole."));

            // ---- Cidade-Recife de Kirta (povo-do-mar; Ugaritic names from the epics of Kirta and Aqhat) ----
            AddResident(entries, "kirta", "Kirta, King of the Reef", "Kirta, o Rei do Recife",
                ("Joaquim, I lost my whole house to the sea once. I rebuilt it stone by stone. A guardian understands: you hold what can be held.", "Joaquim, eu perdi a minha casa inteira para o mar uma vez. Reconstruí pedra por pedra. Um guardião entende: a gente segura o que dá para segurar."),
                ("Maria, the coral was lit by the Living Water. Put your hand near the mother coral. ...See? The east side remembers.", "Maria, o coral era aceso pela Água Viva. Chega a mão perto do coral-mãe. ...Viu? O lado do leste lembra."),
                ("Maithe, every spark you free out there lights a piece of my reef. I count them from the tower. You are winning.", "Maithe, cada faísca que você liberta lá fora acende um pedaço do meu recife. Eu conto da torre. Você está ganhando."),
                ("Ícaro, a lamp that works under water? The divers would bless your name. So would I.", "Ícaro, uma lamparina que funciona debaixo d'água? Os mergulhadores iam abençoar o seu nome. Eu também."),
                ("Lavine, Grandpa Jorge came here by dhow, years ago, and cried when he saw the coral lit. Tell him it will be lit again.", "Lavine, o Vô Jorge veio aqui de dhow, anos atrás, e chorou quando viu o coral aceso. Diz para ele que vai acender de novo."),
                ("The coral went grey, not dead. Grey is a colour that is waiting.", "O coral ficou cinza, não morreu. Cinza é uma cor que está esperando."));

            AddResident(entries, "pughat", "Pughat, Who Looks After Everyone", "Pughat, a Que Cuida de Todos",
                ("Joaquim, can you hold your breath? No? Then you hold the rope and I go down. Deal?", "Joaquim, você prende a respiração? Não? Então você segura a corda e eu desço. Combinado?"),
                ("Maria, the divers came up cold and scared. Sit with them a little. Your light warms better than any blanket.", "Maria, os mergulhadores subiram com frio e com medo. Senta um pouco com eles. A sua luz esquenta mais que qualquer cobertor."),
                ("Maithe, you count your arrows the way I count my divers. Three in, three out. Always.", "Maithe, você conta as suas flechas do jeito que eu conto os meus mergulhadores. Três entram, três saem. Sempre."),
                ("Ícaro, a rope that pulls back on its own when someone tugs twice? Make it and I'll never sleep badly again.", "Ícaro, uma corda que puxa sozinha quando alguém dá dois puxões? Faz isso e eu nunca mais durmo mal."),
                ("Lavine, Grandma Edna sent me a jar of apricot jam by dhow. The divers ate it all. I saved you the spoon.", "Lavine, a Vó Edna me mandou um pote de doce de damasco de dhow. Os mergulhadores comeram tudo. Guardei a colher para você."),
                ("Nobody stays down there alone. Not on my reef. Not while the coral is grey.", "Ninguém fica lá embaixo sozinho. Não no meu recife. Não enquanto o coral estiver cinza."));

            AddResident(entries, "danel", "Danel the Just", "Danel, o Juiz Justo",
                ("Joaquim, a breathing shell for the guardian: hold it between your teeth and breathe slowly. Slowly. That's the whole secret.", "Joaquim, uma concha respirável para o guardião: segura entre os dentes e respira devagar. Devagar. Esse é o segredo todo."),
                ("Maria, I judge fairly: one bubble of air for each diver, never two for the same. Even the king waits in line.", "Maria, eu julgo com justiça: uma bolha de ar para cada mergulhador, nunca duas para o mesmo. Até o rei espera na fila."),
                ("Maithe, the eldest asks the price first. Good. The price is fair. Danel never tips the scale.", "Maithe, a mais velha pergunta o preço primeiro. Bom. O preço é justo. O Danel nunca pende a balança."),
                ("Ícaro, how does the shell keep the air? The coral teaches it. Ask the coral. ...It answers slowly.", "Ícaro, como a concha guarda o ar? O coral ensina. Pergunta para o coral. ...Ele responde devagar."),
                ("Lavine, my son Aqhat says you talk to the fish. Grandpa Jorge did too, and they always told him where the pearls were.", "Lavine, o meu filho Aqhat diz que você conversa com os peixes. O Vô Jorge também conversava, e eles sempre contavam onde estavam as pérolas."),
                ("Shells to breathe, bubbles to walk the bottom, stones to sink. A fair price for each, and a word of advice for free.", "Concha para respirar, bolha para andar no fundo, pedra para afundar. Preço justo para cada uma, e um conselho de graça."));

            AddResident(entries, "aqhat", "Aqhat the Archer", "Aqhat, o Arqueiro",
                ("Joaquim, stand behind the target with the shield up. If the arrow passes the shield, I owe you a pearl. It never passes.", "Joaquim, fica atrás do alvo com o escudo levantado. Se a flecha passar do escudo, eu te devo uma pérola. Nunca passa."),
                ("Maria, under water the arrow bends. Up here it flies straight, like your light. Watch.", "Maria, debaixo d'água a flecha entorta. Aqui em cima ela voa reta, como a sua luz. Olha."),
                ("Maithe! An archer! Hold the bow lower, breathe out, and let the wind of the reef choose the moment. Again.", "Maithe! Uma arqueira! Segura o arco mais baixo, solta o ar, e deixa o vento do recife escolher a hora. De novo."),
                ("Ícaro, a bow that shoots under water? My father says it can't be done. My father also said that about the shell.", "Ícaro, um arco que atira debaixo d'água? O meu pai diz que não dá. O meu pai também disse isso da concha."),
                ("Lavine, Grandma Edna made me promise never to shoot at a gull. I keep it. Even the thieving ones.", "Lavine, a Vó Edna me fez prometer nunca atirar numa gaivota. Eu cumpro. Até nas ladras."),
                ("My bow was a gift of the reef. I teach anyone who asks. Nobody here takes a bow by force.", "O meu arco foi um presente do recife. Eu ensino quem pedir. Aqui ninguém toma arco à força."));

            // ---- Jhilpur, a Cidade nas Raízes (names of Manichaean missionaries and of the Kellis letters) ----
            AddResident(entries, "salmaios", "Salmaios, Keeper of the Mother Root", "Salmaios, o Guardião da Raiz-Mãe",
                ("Joaquim, the Raiz-Mãe drinks the Living Water the way you drink tea: slowly, every day, without missing one. Your trial is in her roots. When you're ready, not before.", "Joaquim, a Raiz-Mãe bebe a Água Viva do jeito que você bebe chá: devagar, todo dia, sem faltar nenhum. A sua prova está nas raízes dela. Quando você estiver pronto, não antes."),
                ("Maria, put your hand on the bark. Feel it? She is warm on the side the sun touches. Light and water, always together.", "Maria, põe a mão na casca. Sentiu? Ela é morna do lado onde o sol bate. Luz e água, sempre juntas."),
                ("Maithe, I have eaten one bowl of rice a day for sixty years. The eldest asks why. Because the roots taught me: take little, give back a lot.", "Maithe, eu como uma tigela de arroz por dia há sessenta anos. A mais velha pergunta por quê. Porque as raízes me ensinaram: pegar pouco, devolver muito."),
                ("Ícaro, you want to know how a root becomes a bridge? Forty years of patience and a hollow palm trunk to guide it. No machine is that slow. Yet.", "Ícaro, quer saber como uma raiz vira ponte? Quarenta anos de paciência e um tronco oco de palmeira para guiar. Nenhuma máquina é tão lenta. Ainda."),
                ("Lavine, your grandpa Jorge carved his name on a root here when he was a boy. The root grew around it. Now the tree remembers him better than I do.", "Lavine, o seu avô Jorge entalhou o nome numa raiz aqui quando era menino. A raiz cresceu em volta. Agora a árvore lembra dele melhor do que eu."),
                ("The Lodo rises a step every night. The roots rise a little every year. We will see who is more patient.", "O Lodo sobe um degrau toda noite. As raízes sobem um pouco todo ano. Vamos ver quem tem mais paciência."));

            AddResident(entries, "nuhzadag", "Nuhzadag the Boatman", "Nuhzadag, o Barqueiro",
                ("Joaquim, a guardian on a reed boat? Sit in the middle and don't move. ...No, the middle. The boat chooses where the middle is.", "Joaquim, um guardião num barco de junco? Senta no meio e não se mexe. ...Não, no meio. Quem escolhe onde é o meio é o barco."),
                ("Maria, I speak eleven tongues of the delta, and in every one of them light is a word that also means welcome. Funny, no?", "Maria, eu falo onze línguas do delta, e em todas elas luz é uma palavra que também quer dizer bem-vindo. Engraçado, né?"),
                ("Maithe, the eldest wants the pole for the root bridges. Good choice. Hold it in the middle and look at the far bank, never at your feet.", "Maithe, a mais velha quer a vara para as pontes de raiz. Boa escolha. Segura no meio e olha para a outra margem, nunca para os pés."),
                ("Ícaro, the trols of Tarchon sent me a reed boat that folds in two. Don't take it apart. ...Fine, take it apart, but fold it back.", "Ícaro, os trols de Tarchon me mandaram um barco de junco que dobra em dois. Não desmonta. ...Tá bom, desmonta, mas dobra de volta."),
                ("Lavine, Grandma Edna sent jars of apricot jam on my boat once. I traded one for a whole basket of purifying fruit. Best trade of my life.", "Lavine, a Vó Edna mandou potes de doce de damasco no meu barco uma vez. Troquei um por um cesto inteiro de fruta purificadora. Melhor troca da minha vida."),
                ("Poles for the bridges, boats for the stilts, amulets for the bog, fruit for the black water. Every road of the delta, paid in fair coin.", "Vara para as pontes, barco para as palafitas, amuleto para o charco, fruta para a água preta. Todo caminho do delta, pago em moeda justa."));

            AddResident(entries, "abiesous", "Abiesous, Master of the Boat School", "Abiesous, o Mestre da Escola de Barco",
                ("Joaquim, you row like you hold a shield: too hard. The water is not an enemy. Let it push back a little.", "Joaquim, você rema como quem segura escudo: forte demais. A água não é inimiga. Deixa ela empurrar de volta um pouco."),
                ("Maria, at dusk my pupils light lotus lanterns and let them float. Yours would float the farthest, I think.", "Maria, no fim da tarde os meus alunos acendem lanternas de lótus e deixam boiar. A sua ia boiar mais longe, eu acho."),
                ("Maithe, you already know how to listen to the wind. Rowing is the same thing, but with the water. Lesson one is free.", "Maithe, você já sabe escutar o vento. Remar é a mesma coisa, só que com a água. A primeira aula é de graça."),
                ("Ícaro, an oar that rows on its own? Then who learns anything? ...Draw it anyway. I want to see.", "Ícaro, um remo que rema sozinho? E quem aprende alguma coisa? ...Desenha mesmo assim. Eu quero ver."),
                ("Lavine, Grandpa Jorge was my worst pupil and my best friend. He rowed in circles for a whole monsoon, laughing.", "Lavine, o Vô Jorge foi o meu pior aluno e o meu melhor amigo. Remou em círculo uma monção inteira, rindo."),
                ("In the monsoon the streets become canals. Whoever can row goes home. That's the whole lesson.", "Na monção as ruas viram canais. Quem sabe remar volta para casa. Essa é a aula toda."));

            AddResident(entries, "tehat", "Tehat the Herbalist", "Tehat, a Erveira",
                ("Joaquim, you've got mud up to the knees. Here, a leaf of neem in your boots: the leeches will look for another guardian.", "Joaquim, você está com lama até o joelho. Toma, uma folha de nim na bota: as sanguessugas vão procurar outro guardião."),
                ("Maria, the lotus grows out of the black mud and comes up clean. That is the whole of my medicine, and maybe of yours.", "Maria, o lótus nasce da lama preta e sobe limpo. Esse é todo o meu remédio, e talvez o seu também."),
                ("Maithe, I write to my sister every week, a long letter on palm leaf. The eldest understands: someone has to keep the family together.", "Maithe, eu escrevo para a minha irmã toda semana, uma carta comprida em folha de palmeira. A mais velha entende: alguém tem que manter a família junta."),
                ("Ícaro, a still that makes medicine by itself? You'll have to taste every drop it makes. ...Still want it?", "Ícaro, um alambique que faz remédio sozinho? Você vai ter que provar cada gota que ele fizer. ...Ainda quer?"),
                ("Lavine, Grandma Edna's mint grows in my window. She sent the cutting in a letter, wrapped in a wet cloth. It arrived alive.", "Lavine, a hortelã da Vó Edna cresce na minha janela. Ela mandou a muda numa carta, enrolada num pano molhado. Chegou viva."),
                ("Neem for the leeches, turmeric for the cuts, lotus for the heart. The roots give everything, if you ask nicely.", "Nim para as sanguessugas, açafrão-da-terra para os cortes, lótus para o coração. As raízes dão tudo, se a gente pedir com jeito."));

            // ---- Palafitas de Tarchon (trols; Etruscan names from Tarquinia and Cerveteri). Trols speak slowly. ----
            AddResident(entries, "tarchon", "Tarchon, the Founder", "Tarchon, o Fundador",
                ("Joaquim... you stand like a pile. Good. Piles hold the house... when the water rises. Your trial... is in the deep bog. Not today. When you are... ready.", "Joaquim... você fica de pé como uma estaca. Bom. Estaca segura a casa... quando a água sobe. A sua prova... está no charco fundo. Hoje não. Quando você estiver... pronto."),
                ("Maria... a wisp is a spark... that lost its way. You have the light... that shows the way home. Stay... a little. My people... need it.", "Maria... um fogo-fátuo é uma faísca... que perdeu o caminho. Você tem a luz... que mostra o caminho de casa. Fica... um pouco. O meu povo... precisa."),
                ("Maithe... the eldest. I drove the first pile... into this mud... three hundred years ago. The eldest... always drives the first pile. Then... the others follow.", "Maithe... a mais velha. Eu finquei a primeira estaca... nessa lama... há trezentos anos. A mais velha... sempre finca a primeira estaca. Depois... os outros vêm atrás."),
                ("Ícaro... you talk fast. Like a dragonfly. I like... dragonflies. Tell me again... slowly... about the machine that pulls nets.", "Ícaro... você fala depressa. Que nem libélula. Eu gosto... de libélula. Me conta de novo... devagar... da máquina que puxa rede."),
                ("Lavine... your grandpa Jorge... came here lost, in a reed boat... laughing. He stayed a whole moon. He taught us... to whistle. We still whistle... badly.", "Lavine... o seu avô Jorge... chegou aqui perdido, num barco de junco... rindo. Ficou uma lua inteira. Ensinou a gente... a assobiar. A gente ainda assobia... mal."),
                ("The water rises... the piles hold. The wisps come... the lanterns hold them. Slowly... everything holds.", "A água sobe... as estacas seguram. Os fogos-fátuos vêm... as lanternas seguram. Devagar... tudo segura."));

            AddResident(entries, "ramtha", "Ramtha the Healer", "Ramtha, a Curandeira",
                ("Joaquim... you have a cut, and you did not say. Guardians never say. Sit. Moss from the north side of the pile... closes it by morning.", "Joaquim... você tem um corte e não falou. Guardião nunca fala. Senta. Musgo do lado norte da estaca... fecha até amanhã."),
                ("Maria... the guardian lamp of the stilts burns over my door. When the wisps buzz too close... it hums back at them. Like you do.", "Maria... a lâmpada guardiã das palafitas queima em cima da minha porta. Quando os fogos-fátuos zumbem perto demais... ela zumbe de volta. Que nem você."),
                ("Maithe... you carry everyone's worries... in your shoulders. Here. Lilac tea. Drink it... slowly. Slower. ...Good.", "Maithe... você carrega a preocupação de todo mundo... nos ombros. Toma. Chá de lilás. Bebe... devagar. Mais devagar. ...Isso."),
                ("Ícaro... no, the mortar does not need a motor. The moss likes... to be ground by hand. It heals better. ...Maybe a small motor.", "Ícaro... não, o pilão não precisa de motor. O musgo gosta... de ser moído na mão. Cura melhor. ...Talvez um motorzinho."),
                ("Lavine... Grandma Edna sent me seeds once... in a letter. Mint. It grows on my roof now... between the leaves. The whole house smells of her.", "Lavine... a Vó Edna me mandou sementes uma vez... numa carta. Hortelã. Agora cresce no meu telhado... no meio das folhas. A casa inteira tem o cheiro dela."),
                ("Trols heal fast. Their hearts... heal slowly. For the heart... I have only tea and time.", "Trol sara depressa. O coração dele... sara devagar. Para o coração... eu só tenho chá e tempo."));

            AddResident(entries, "larthia", "Larthia the Basket Weaver", "Larthia, a Cesteira",
                ("Joaquim! A basket for your shield? No? A basket for your helmet? ...A basket for your baskets?", "Joaquim! Um cesto para o seu escudo? Não? Um cesto para o seu elmo? ...Um cesto para os seus cestos?"),
                ("Maria, I weave the lantern baskets for the wisps the cousins set free. Each one... holds a little light. Like a pocket.", "Maria, eu teço os cestos de lanterna para os fogos-fátuos que os primos soltam. Cada um... guarda um pouco de luz. Que nem um bolso."),
                ("Maithe, the eldest carries the most. So... a bigger bag. One more row. I sew it while you wait... slowly.", "Maithe, a mais velha carrega mais. Então... uma bolsa maior. Uma fileira a mais. Eu costuro enquanto você espera... devagar."),
                ("Ícaro, this reed boat folds in two. It took me a week. If you take it apart... you weave it back. Another week.", "Ícaro, esse barco de junco dobra em dois. Levei uma semana. Se você desmontar... você tece de volta. Mais uma semana."),
                ("Lavine, your grandpa Jorge bought a basket here and wore it on his head... as a hat. All the way home. Grandma Edna kept it.", "Lavine, o seu avô Jorge comprou um cesto aqui e usou na cabeça... de chapéu. Até chegar em casa. A Vó Edna guardou."),
                ("Boats for the swamp, amulets for the bog, bags for the road. Everything woven... by hand... very slowly.", "Barco para o pântano, amuleto para o charco, bolsa para a estrada. Tudo tecido... à mão... bem devagar."));

            AddResident(entries, "vel", "Vel the Fisher", "Vel, o Pescador",
                ("Joaquim... catfish this big. Bigger than you. ...Bigger than me. It ate my boat once. Then gave it back.", "Joaquim... bagre deste tamanho. Maior que você. ...Maior que eu. Comeu o meu barco uma vez. Depois devolveu."),
                ("Maria... the Fisher of Lights is a catfish... who swallowed the wisps... to have light just for himself. He is not bad. Only... afraid of the dark.", "Maria... o Pescador de Luzes é um bagre... que engoliu os fogos-fátuos... para ter luz só para ele. Ele não é mau. Só... tem medo do escuro."),
                ("Maithe... you pull the net like a trol. Slowly, all the way. Want to come... at dawn? Bring your patience.", "Maithe... você puxa a rede que nem trol. Devagar, até o fim. Quer vir... de madrugada? Traz a sua paciência."),
                ("Ícaro... yes, the net has holes. On purpose. The little fish... go home. Only the big ones stay.", "Ícaro... sim, a rede tem buraco. De propósito. Os peixinhos... voltam para casa. Só os grandes ficam."),
                ("Lavine... Grandma Edna's recipe for catfish with lemon. I make it every full moon. The whole stilts come to my door.", "Lavine... a receita de bagre com limão da Vó Edna. Eu faço toda lua cheia. As palafitas inteiras vêm na minha porta."),
                ("Net in the water... wait. Wait more. ...Fish. That is all fishing is.", "Rede na água... espera. Espera mais. ...Peixe. Pescar é só isso."));

            AddResident(entries, "thanchvil", "Thanchvil the Storyteller", "Thanchvil, a Contadora de Histórias",
                ("Joaquim... come, sit on the planks. Tonight's story is about a trol who held a falling house... with one arm. It took all night. Then... he slept a week.", "Joaquim... vem, senta nas tábuas. A história de hoje é de um trol que segurou uma casa caindo... com um braço. Levou a noite toda. Depois... dormiu uma semana."),
                ("Maria... my sister is one of the dimmed ones. She sits by the mast... and looks at the wisps. I tell her stories every night. One day... she will answer.", "Maria... a minha irmã é uma das apagadas. Ela fica sentada perto do mastro... olhando os fogos-fátuos. Eu conto histórias para ela toda noite. Um dia... ela vai responder."),
                ("Maithe... the eldest of the story always arrives last... because she waited for everyone. Did you know? Now you do.", "Maithe... a mais velha da história sempre chega por último... porque esperou todo mundo. Sabia? Agora sabe."),
                ("Ícaro... you want the end first? Trols tell the end... last. Very last. ...Fine. They lived. Happy?", "Ícaro... você quer o fim primeiro? Trol conta o fim... por último. Bem por último. ...Tá. Eles viveram. Feliz?"),
                ("Lavine... I know a story about a boy named Jorge... who whistled so badly the herons flew away. It is true. Ask Tarchon.", "Lavine... eu sei uma história de um menino chamado Jorge... que assobiava tão mal que as garças voavam embora. É verdade. Pergunta para o Tarchon."),
                ("Every spark set free... is a story that comes back. The stilts are full of stories... waiting to come back.", "Cada faísca libertada... é uma história que volta. As palafitas estão cheias de histórias... esperando para voltar."));

            // The Vila shops of 2026-10-30, the freguesia and the house items on the counters (CityShopTexts).
            entries.AddRange(CityShopTexts.Entries);

            return entries;
        }

        private static void AddCity(List<LocalizationEntry> entries, string id, string nameEn, string namePt, string buttonEn, string buttonPt,
            string flavourEn, string flavourPt)
        {
            entries.Add(new LocalizationEntry("city." + id + ".name", nameEn, namePt));
            entries.Add(new LocalizationEntry("city." + id + ".button", buttonEn, buttonPt));
            entries.Add(new LocalizationEntry("city." + id + ".flavour", flavourEn, flavourPt));
        }

        private static LocalizationEntry Shop(string shopId, string en, string pt) => new LocalizationEntry("city.shop." + shopId + ".name", en, pt);

        private static void AddResident(List<LocalizationEntry> entries, string residentId, string nameEn, string namePt,
            (string En, string Pt) joaquim, (string En, string Pt) maria, (string En, string Pt) maithe, (string En, string Pt) icaro,
            (string En, string Pt) lavine, (string En, string Pt) anyone)
        {
            string keyBase = "city.resident." + residentId;
            if (nameEn != null)
            {
                entries.Add(new LocalizationEntry(keyBase + ".name", nameEn, namePt));
            }

            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Joaquim), joaquim.En, joaquim.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Maria), maria.En, maria.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Maithe), maithe.En, maithe.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Icaro), icaro.En, icaro.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Lavine), lavine.En, lavine.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line.any", anyone.En, anyone.Pt));
        }

        private static string Hero(CharacterId hero) => hero.ToString().ToLowerInvariant();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> (and the region quest texts, for the shared NPC names) into <paramref name="manager"/>, skipping keys it already has.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            RegionQuestTexts.EnsureLoaded(manager);
            CityQuestTexts.EnsureLoaded(manager);
            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            // Barca do Sol by day, Barca da Lua at dusk and night: same key for the caller, the line follows the clock.
            if (key == FastTravelOnTheWay && FastTravelTransition.IsMoonShipNow())
            {
                key = FastTravelOnTheWayMoon;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>pt-BR text from <see cref="Entries"/>, the region quest texts or the core table, or null.</summary>
        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                foreach (LocalizationEntry entry in RegionQuestTexts.All.Concat(CityQuestTexts.All).Concat(LocalizationManager.DefaultEntries.All))
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.Key) && !table.ContainsKey(entry.Key))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        // ------------------------------------------------------------------
        // Helpers used by the hub and the HUD
        // ------------------------------------------------------------------

        public static string CityName(LocalizationManager manager, CitySpec city) => city != null ? Get(manager, city.NameKey) : string.Empty;

        public static string ResidentName(LocalizationManager manager, CityResidentSpec resident) => resident != null ? Get(manager, resident.NameKey) : string.Empty;

        /// <summary>What <paramref name="resident"/> says to <paramref name="hero"/> (the generic line without a cousin).</summary>
        public static string ResidentLine(LocalizationManager manager, CityResidentSpec resident, CharacterId? hero)
        {
            if (resident == null)
            {
                return string.Empty;
            }

            return Get(manager, hero.HasValue ? resident.LineKey(hero.Value) : resident.AnyLineKey);
        }

        /// <summary>"Vandak, o Seleiro: “...”".</summary>
        public static string ResidentSpeech(LocalizationManager manager, CityResidentSpec resident, CharacterId? hero)
        {
            return Format(manager, Speech, ResidentName(manager, resident), ResidentLine(manager, resident, hero));
        }

        public static string ShopName(LocalizationManager manager, string shopId) => Get(manager, CityShopCatalog.NameKeyOf(shopId));
    }
}
