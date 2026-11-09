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
                Shop(CityShopCatalog.AgasalhosSisinnios, "Sisinnios's Mountain Gear", "Agasalhos do Sisinnios"),
                Shop(CityShopCatalog.LanternasMozak, "Master Mōzak's Lanterns", "Lanternas do Mestre Mōzak"),
                Shop(CityShopCatalog.EspelhosNatakamani, "Natakamani's Mirrors", "Espelhos de Natakamani"),
                Shop(CityShopCatalog.SelariaNomade, "Nomad Saddlery", "Selaria Nômade"),
                Shop(CityShopCatalog.PanelaGrande, "The Great Pot", "Panela Grande"),
                Shop(CityShopCatalog.ViveiroSuvarnapushpa, "Suvarnapushpa's Nursery", "Viveiro da Suvarnapushpa"),
                Shop(CityShopCatalog.FlautaDupla, "The Double Flute", "Flauta Dupla"),
                Shop(CityShopCatalog.CordasSisin, "Sisin's Ropes", "Cordas do Sisin"),
                Shop(CityShopCatalog.PelesPamour, "Pamour's Furs and Boats", "Peles e Barcos do Pamour"),
                Shop(CityShopCatalog.FerroPretoKurogane, "Kurogane's Black Iron", "Ferro Preto do Kurogane"),
                Shop(CityShopCatalog.TrocasKhunanup, "Khunanup's Trades", "Trocas do Khunanup"),
                Shop(CityShopCatalog.BetumeAtrahasis, "Atrahasis' Bitumen Yard", "Betume do Atrahasis"),
                Shop(CityShopCatalog.PaviosTehat, "Tehat's Wicks", "Pavios da Tehat"),
                Shop(CityShopCatalog.UltimaCaravanaAmmo, "Ammo's Last Caravan", "A Última Caravana do Ammo"),
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

            AddCity(entries, CityCatalog.Sangkent, "Sangkent, the Stair City", "Sangkent, a Cidade-Escadaria", "Sangkent", "Sangkent",
                "The capital of the Roof of the World, built in terraces of stone up a high valley: houses of stone and loam laced with juniper, lattice balconies, apricot trees in blossom and, above it all, the fort of the seventh terrace. Polished stone chutes carry plank carts down between the levels. The caravans that took the hymns east crossed here, and the five lights met: five colours on the ribbons of every cart.",
                "A capital do Teto do Mundo, construída em terraços de pedra subindo um vale alto: casas de pedra e barro amarradas com vigas de zimbro, varandas de treliça, damasqueiros em flor e, acima de tudo, o forte do sétimo terraço. Calhas de pedra polida levam carrinhos de tábua para baixo entre os níveis. As caravanas que levaram os hinos para o leste passavam por aqui, e as cinco luzes se encontraram: cinco cores nas fitas de cada carrinho.");

            AddCity(entries, CityCatalog.Mingcheng, "Mingcheng, City of Nine Rings", "Mingcheng, a Cidade dos Nove Anéis", "Mingcheng", "Mingcheng",
                "The capital of the river plain, a walled city of nine rings one inside the other: courtyard halls of grey brick with red-lacquered columns and curved tile roofs, humpback bridges over the canals, willows, and thousands of paper lanterns at night. In the 6th ring the Drum and Bell Tower keeps the hours: the drum opens the gates, the bell shuts them. Here the Light was called the Doctrine of Light.",
                "A capital da planície do rio, uma cidade murada de nove anéis um dentro do outro: salões de tijolo cinza com colunas de laca vermelha e telhados curvos de telha, pontes de corcunda sobre os canais, salgueiros e milhares de lanternas de papel à noite. No 6º anel a Torre do Tambor e do Sino marca as horas: o tambor abre os portões, o sino fecha. Aqui a Luz era chamada de Doutrina da Luz.");

            AddCity(entries, CityCatalog.TorresBrancas, "White Spires of Amanirenas", "Torres Brancas de Amanirenas", "White Spires", "Torres Brancas",
                "The high elves' city on the white hills east of Mingcheng: slender white spires with glass domes on low pointed pyramids, leaf bridges of curved stone between them and garden terraces below. Bronze mirrors pass the sunlight down from the towers to the ground, by day and, with the moon, by night. The elves are astronomers and builders: proud, but gentle with children.",
                "A cidade dos altos-elfos nas colinas brancas a leste de Mingcheng: torres brancas finas com cúpula de vidro sobre pirâmides baixas e pontudas, pontes de folha de pedra curva entre elas e jardins em terraço embaixo. Espelhos de bronze passam a luz do sol das torres até o chão, de dia e, com a lua, de noite. Os elfos são astrônomos e construtores: orgulhosos, mas gentis com as crianças.");

            AddCity(entries, CityCatalog.AcampamentoSoslan, "Soslan's Camp", "Acampamento de Soslan", "Soslan's Camp", "Acampamento de Soslan",
                "The centaurs' camp on the open steppe of the Narts: big round felt tents on wagons with spoked wheels, dyed felt bands red and blue, horsehair standards topped with gold stags, rope corrals and the stone oven everyone bakes in. The camp moves three times a week and the Cais da Barca rolls along with it. The centaurs tell sagas and run with the Living Wind.",
                "O acampamento dos centauros na estepe aberta dos Nartes: grandes tendas redondas de feltro sobre carroças de rodas de raio, faixas de feltro tingido de vermelho e azul, estandartes de crina com cervos de ouro no alto, cercados de corda e o forno de pedra onde todo mundo assa o pão. O acampamento muda de lugar três vezes por semana e o Cais da Barca vai junto. Os centauros contam sagas e correm com o Vento Vivo.");

            AddCity(entries, CityCatalog.Fortim, "Hill-Giant Fort", "Fortim dos Gigantes-da-Colina", "Hill-Giant Fort", "Fortim",
                "The hill giants' fort: round houses of dry stone under turf roofs, much bigger than any house of the cousins, earth ramparts in rings faced with stone, stone tables, giant spoons the cousins sit on and a pot the size of a house over the Living Fire. Long ago the giants dreamt of the end and were sorry; now they cook for everyone and build doors too tall for anyone to bend.",
                "O fortim dos gigantes-da-colina: casas redondas de pedra seca debaixo de telhados de grama, muito maiores que qualquer casa dos primos, muralhas de terra em anéis revestidas de pedra, mesas de pedra, colheres gigantes onde os primos sentam e uma panela do tamanho de uma casa sobre o Fogo Vivo. Faz muito tempo os gigantes sonharam com o fim e se arrependeram; agora cozinham para todo mundo e fazem portas altas demais para alguém precisar se abaixar.");

            AddCity(entries, CityCatalog.Arvoredo, "Haripushpa Grove", "Arvoredo de Haripushpa", "Haripushpa Grove", "Arvoredo",
                "The trunk folk's grove at the edge of the steppe, where the taiga begins: houses inside huge hollow trunks with root steps and nest windows, larches, a circle of mossy standing stones and streams of meltwater crossed by split logs. The trunk folk drink the Living Water of the thaw and remember everything; they say one sentence a minute, and here the clock walks at half speed.",
                "O arvoredo do povo-tronco na beira da estepe, onde começa a taiga: casas dentro de troncos ocos enormes com degraus de raiz e janelas de ninho, lariços, um círculo de pedras com musgo e riachos de degelo cruzados por troncos rachados. O povo-tronco bebe a Água Viva do degelo e lembra de tudo; fala uma frase por minuto, e aqui o relógio anda pela metade.");

            AddCity(entries, CityCatalog.ValeDosSatiros, "Satyr Vale", "Vale dos Sátiros", "Satyr Vale", "Vale dos Sátiros",
                "The satyrs' valley at the foot of the peaks of Farnkesh: gourd-shaped wicker houses round clearings of grass, stone stages for the dance, rows of vines, broad oaks and a river of golden pebbles, the Pactolus, where King Midas washes the gold off his hands. The satyrs play the Living Wind on double flutes, and at dusk the whole valley dances in a ring.",
                "O vale dos sátiros ao pé dos picos de Farnkesh: casas de vime em forma de cabaça em volta de clareiras de grama, palcos de pedra para o baile, fileiras de videiras, carvalhos largos e um rio de seixos dourados, o Pactolo, onde o rei Midas lava o ouro das mãos. Os sátiros tocam o Vento Vivo em flautas duplas, e ao entardecer o vale inteiro dança em roda.");

            AddCity(entries, CityCatalog.Farnkesh, "Farnkesh, the Stone Nests", "Farnkesh, os Ninhos de Pedra", "Farnkesh", "Farnkesh",
                "The capital of the Living Wind, above the clouds: white houses with leaning walls on sandstone pinnacles, windows of painted red wood, plank roofs held down with stones and balconies over the void. Rope bridges cross the gaps of cloud, Sisin pulls the rope baskets up and down, and on the plaza of the largest pinnacle hangs the giant wind bell. Farn is the glory that lives up high; here people like wind and silence.",
                "A capital do Vento Vivo, acima das nuvens: casas brancas de parede inclinada sobre pináculos de arenito, janelas de madeira pintada de vermelho, telhados de tábua presos com pedras e varandas para o vazio. Pontes de corda atravessam os vãos de nuvem, o Sisin puxa os cestos de corda para cima e para baixo, e na praça do pináculo maior fica o sino de vento gigante. Farn é a glória que mora no alto; aqui as pessoas gostam de vento e de silêncio.");

            AddCity(entries, CityCatalog.PortoDaLuaBranca, "White Moon Harbor", "Porto da Lua Branca", "White Moon", "Lua Branca",
                "The capital of the Ar Sereno, in the far north: a harbour on islands of ice joined by canals of black water and stone bridges shaped like whale ribs, long plank houses with a moon painted on the gable, round snow domes, crescent boats drawn up on the ice and racks of silver fish. In the middle stands the Cais da Lua, where the Moon Ship moors on the full moon to gather the light set free; at night the aurora dances over the canals.",
                "A capital do Ar Sereno, no extremo norte: um porto em ilhas de gelo ligadas por canais de água negra e pontes de pedra em forma de costela de baleia, casas compridas de tábua com uma lua pintada na empena, cúpulas de neve redondas, barcos em meia-lua puxados para o gelo e varais de peixe de prata. No meio fica o Cais da Lua, onde a Barca da Lua atraca na lua cheia para recolher a luz libertada; à noite a aurora dança sobre os canais.");

            AddCity(entries, CityCatalog.Hinokura, "Hinokura, the Forge Isle", "Hinokura, a Ilha-Forja", "Hinokura", "Hinokura",
                "The capital of the Living Fire, on a volcanic isle: halls of red lacquer on curved walls of coral stone, roofs of red tiles with white ridges, courtyards of black stone and channels of tamed lava that move the forges, opened and closed by sluices. On the plaza burns the Forja de Hinokura; up the hill, the palace in its three courts; down by the sea, the stone harbour. Hi no kura: the granary of fire, where the light that refuses to sleep is kept.",
                "A capital do Fogo Vivo, numa ilha vulcânica: salões de laca vermelha sobre muros curvos de pedra de coral, telhados de telha vermelha com cumeeiras brancas, pátios de pedra preta e canais de lava domada que movem as forjas, abertos e fechados por comportas. Na praça arde a Forja de Hinokura; no alto, o palácio nos seus três pátios; lá embaixo, junto ao mar, o porto de pedra. Hi no kura: o celeiro do fogo, onde se guarda a luz que se recusa a dormir.");

            AddCity(entries, CityCatalog.CaisDeMesektet, "Mesektet Quay", "Cais de Mesektet", "Mesektet", "Mesektet",
                "The quay of the night barque on the Rio das Doze Horas, where it is always night: halls of sandstone on rafts and pilings with fronts of papyrus columns and roofs of reed mats, boardwalks of dark planks over the black water, thickets of papyrus and reed skiffs on the bank. In the middle the Mesektet moors at the Cais da Barca before going down the river; around it, the twelve piers of the hours, one opening each time the barque passes. Stars are painted under every roof.",
                "O cais da barca da noite no Rio das Doze Horas, onde é sempre noite: salões de arenito sobre balsas e pilares com frente de colunas de papiro e telhado de esteira de junco, passarelas de tábua escura sobre a água preta, moitas de papiro e barquinhos de junco na margem. No meio, a Mesektet atraca no Cais da Barca antes de descer o rio; em volta, os doze píeres das horas, um abrindo cada vez que a barca passa. Debaixo de todo telhado há estrelas pintadas.");

            AddCity(entries, CityCatalog.DurJarras, "Dur-Jarras, the Ziggurat City", "Dur-Jarras, a Cidade-Zigurate", "Dur-Jarras", "Dur-Jarras",
                "The ziggurat city of the Margens da Mistura, where light and shadow are most mixed: houses of baked brick with glazed blue bands and stepped parapets, canals of black water sealed with bitumen, date palms and piers of glazed brick with lions on them. At the head of the plaza rises a ziggurat of five steps, which lights from top to bottom at dusk; in front of it the Relógio de Zurvan marks the hours.",
                "A cidade-zigurate das Margens da Mistura, onde luz e sombra estão mais misturadas: casas de tijolo cozido com faixas de azul vidrado e parapeito em degraus, canais de água negra calafetados com betume, palmeiras e pilares de tijolo vidrado com leões. Na cabeça da praça sobe um zigurate de cinco degraus, que acende de cima para baixo ao anoitecer; na frente dele, o Relógio de Zurvan marca as horas.");

            AddCity(entries, CityCatalog.FortalezaUltimaLampada, "Fortress of the Last Lamp", "Fortaleza da Última Lâmpada", "Fortress", "Fortaleza",
                "The fortress of the guardians of the lamps, cut into the basalt on the edge of the Land of Darkness: keeps of dark stone with rows of lit niches, a great arc of rampart over the abyss with a thousand little flames and five sealed gates, one for each pit of the dark. In the middle of it burns the Lâmpada-Mãe Grande, sister of the Vila das Lamparinas. There is no sky here: the guards keep the hour by the oil.",
                "A fortaleza dos guardiões das lâmpadas, escavada no basalto na borda da Terra da Treva: torres de pedra escura com fileiras de nichos acesos, um grande arco de muralha sobre o abismo com mil chamas pequenas e cinco portões selados, um para cada cova da treva. No meio arde a Lâmpada-Mãe Grande, irmã da Vila das Lamparinas. Aqui não tem céu: os guardas contam a hora pelo óleo.");

            AddCity(entries, CityCatalog.CaisDaColuna, "Column Quay", "Cais da Coluna", "Quay", "Cais",
                "The quay above the clouds where the Barcas de Luz moor: pavilions of glass and gold on firm cloud, the pier of the Sun to the east and the pier of the Moon to the west, a round plaza with five fountains, one for each light, and to the north the foot of the Coluna da Glória going up out of sight. Far below, through the gaps in the cloud, the twenty-seven cities of the island show as little lights.",
                "O cais sobre as nuvens onde as Barcas de Luz encostam: pavilhões de vidro e ouro em nuvem firme, o píer do Sol a leste e o píer da Lua a oeste, uma praça redonda com cinco fontes, uma para cada luz, e ao norte o pé da Coluna da Glória subindo até sumir. Lá embaixo, pelos buracos da nuvem, as vinte e sete cidades da ilha aparecem como luzinhas.");

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

            // ---- Sangkent, a Cidade-Escadaria (names of Manichaean missionaries and scribes) ----
            AddResident(entries, "aba", "Abā, Chief of the Chutes", "Abā, o Chefe das Calhas",
                ("Joaquim, a guardian who is afraid of a cart? Sit at the back, hold the ribbons and shout. Everyone shouts the first time.", "Joaquim, um guardião com medo de carrinho? Senta atrás, segura as fitas e grita. Todo mundo grita na primeira vez."),
                ("Maria, at dusk the terraces light up one by one, from the market to the fort. Count them with me: the seventh is always the last.", "Maria, no fim da tarde os terraços acendem um por um, do mercado até o forte. Conta comigo: o sétimo é sempre o último."),
                ("Maithe, the eldest goes down first and waits at the bottom for the others. That is the rule of the chutes, and of families too.", "Maithe, a mais velha desce primeiro e espera os outros lá embaixo. Essa é a regra das calhas, e das famílias também."),
                ("Ícaro, no, you cannot put a sail on the cart. ...Who told you about the sail? Kustai? I will have a word with Kustai.", "Ícaro, não, você não pode pôr vela no carrinho. ...Quem te contou da vela? O Kustai? Vou ter uma conversa com o Kustai."),
                ("Lavine, your grandpa Jorge rode my first chute when it was a plank on the snow. He went down backwards, laughing. Nobody has beaten his time.", "Lavine, o seu avô Jorge desceu na minha primeira calha quando ela era uma tábua na neve. Desceu de costas, rindo. Ninguém bateu o tempo dele."),
                ("Going down is quick, going up takes the stairs. Seven terraces, one cart at a time. Wait your turn.", "Descer é rápido, subir é de escada. Sete terraços, um carrinho por vez. Espera a sua vez."));

            AddResident(entries, "sisinnios", "Sisinnios the Outfitter", "Sisinnios, o Agasalheiro",
                ("Joaquim, felt over silk, silk over skin. Up here the cold does not bite, it waits. Dress before it starts waiting for you.", "Joaquim, feltro por cima da seda, seda por cima da pele. Aqui em cima o frio não morde, ele espera. Se agasalha antes que ele comece a esperar por você."),
                ("Maria, the thermal clothes of the monks of Sangkent: you wear them, you do not carry them. The gate of the city knows the difference.", "Maria, a roupa térmica dos monges de Sangkent: é para vestir, não para levar na mochila. O portão da cidade sabe a diferença."),
                ("Maithe, crampons for the glacier, the eldest takes two pairs. One for her, one for whoever forgot.", "Maithe, grampos para a geleira, a mais velha leva dois pares. Um para ela, outro para quem esqueceu."),
                ("Ícaro, the storm cloak has twelve straps. Each one has a reason. ...No, you cannot remove the eleventh.", "Ícaro, a capa de tempestade tem doze correias. Cada uma tem um motivo. ...Não, você não pode tirar a décima primeira."),
                ("Lavine, Grandma Edna sent me a jar of apricot jam with a note: for the man who keeps my grandchildren warm. I keep the note in my coat.", "Lavine, a Vó Edna me mandou um pote de doce de damasco com um bilhete: para o homem que mantém os meus netos aquecidos. Eu guardo o bilhete no casaco."),
                ("Clothes for the stair city, crampons for the glacier, a cloak for the pass. Everything for the roof of the world, at a fair price.", "Roupa para a cidade-escadaria, grampos para a geleira, capa para o passo. Tudo para o teto do mundo, a preço justo."));

            AddResident(entries, "kustai", "Kustai the Scribe", "Kustai, o Escriba",
                ("Joaquim, this notice says a caravan of salt needs a guard to the pass. I wrote your name next to it. In pencil. For now.", "Joaquim, esse aviso diz que uma caravana de sal precisa de guarda até o passo. Escrevi o seu nome do lado. A lápis. Por enquanto."),
                ("Maria, I copy every notice twice: one for the board, one for the box. The wind takes the first. The light keeps the second.", "Maria, eu copio todo aviso duas vezes: um para o quadro, outro para a caixa. O vento leva o primeiro. A luz guarda o segundo."),
                ("Maithe, you read aloud for those who cannot read. A scribe could not ask for a better neighbour on the terrace.", "Maithe, você lê em voz alta para quem não sabe ler. Um escriba não podia pedir vizinha melhor no terraço."),
                ("Ícaro, I did not tell Abā about the sail. I only wrote it down. ...Writing is not telling.", "Ícaro, eu não contei da vela para o Abā. Só escrevi. ...Escrever não é contar."),
                ("Lavine, Grandpa Jorge left a notice on this board forty years ago: looking for a girl named Edna. Someone answered it, it seems.", "Lavine, o Vô Jorge deixou um aviso nesse quadro há quarenta anos: procuro uma moça chamada Edna. Parece que alguém respondeu."),
                ("East or west, wool or silk, salt or hymns: every caravan that crosses the terraces leaves a word on my board.", "Leste ou oeste, lã ou seda, sal ou hinos: toda caravana que cruza os terraços deixa uma palavra no meu quadro."));

            AddResident(entries, "baraies", "Baraies the Teacher", "Baraies, o Professor",
                ("Joaquim, a map is a promise: this way there is water. Keep the promises of the maps and they will keep you.", "Joaquim, um mapa é uma promessa: por aqui tem água. Cumpre as promessas dos mapas e elas cuidam de você."),
                ("Maria, from the fort you can see four valleys. The fifth is behind the light. Every student asks where. I let them guess.", "Maria, do forte dá para ver quatro vales. O quinto fica atrás da luz. Todo aluno pergunta onde. Eu deixo adivinhar."),
                ("Maithe, the wind of the Fort of the Winds turns with the vanes. Learn which way, and the whole valley will speak to you.", "Maithe, o vento do Forte dos Ventos gira com os cata-ventos. Aprende para que lado, e o vale inteiro vai falar com você."),
                ("Ícaro, you counted the steps of all seven terraces? Four hundred and twelve. ...Four hundred and thirteen? Count again. With me.", "Ícaro, você contou os degraus dos sete terraços? Quatrocentos e doze. ...Quatrocentos e treze? Conta de novo. Comigo."),
                ("Lavine, Grandma Edna was my best student. She drew the map of the Zarafshan from memory, with every apricot tree on it.", "Lavine, a Vó Edna foi a minha melhor aluna. Ela desenhou o mapa do Zarafshan de memória, com todos os damasqueiros."),
                ("Seven terraces, four chutes, one pass. Learn the stair city by heart and the mountain will never lose you.", "Sete terraços, quatro calhas, um passo. Aprende a cidade-escadaria de cor e a montanha nunca vai perder você."));

            // ---- Mingcheng, a Cidade dos Nove Anéis (names of the Mingjiao and of the Uyghurs) ----
            AddResident(entries, "bogu", "Bögü, Chief of the Ring Guard", "Bögü, o Chefe da Guarda dos Anéis",
                ("Joaquim, a guard stands still and sees everything. You stand still and see everything too. The ring guard would take you tomorrow.", "Joaquim, guarda fica parado e vê tudo. Você fica parado e vê tudo também. A guarda dos anéis te aceitava amanhã."),
                ("Maria, nine rings, nine gates each, one drum. Count with me: when the drum stops, the last gate is the seventh. Always.", "Maria, nove anéis, nove portões cada, um tambor. Conta comigo: quando o tambor para, o último portão é o do sétimo anel. Sempre."),
                ("Maithe, the eldest walks last through the gate and checks that nobody stayed outside. I did that for forty years.", "Maithe, a mais velha passa por último pelo portão e confere se ninguém ficou do lado de fora. Eu fiz isso por quarenta anos."),
                ("Ícaro, no, you may not ring the bell. ...Fine. Once. At dusk. And the whole city will know it was you.", "Ícaro, não, você não pode tocar o sino. ...Tá bom. Uma vez. No fim da tarde. E a cidade inteira vai saber que foi você."),
                ("Lavine, your grandpa Jorge came to my gate after the bell, soaked, with flowers for a girl named Edna. I opened it. Best mistake of my watch.", "Lavine, o seu avô Jorge chegou no meu portão depois do sino, encharcado, com flores para uma moça chamada Edna. Eu abri. O melhor erro do meu plantão."),
                ("The drum opens the rings, the bell shuts them. Whoever is outside at the bell stays outside until the drum.", "O tambor abre os anéis, o sino fecha. Quem estiver fora quando o sino tocar fica fora até o tambor."));

            AddResident(entries, "mozak", "Master Mōzak, Keeper of the Lanterns", "Mestre Mōzak, o Guardião das Lanternas",
                ("Joaquim, a lantern does not fight the dark. It only stays lit. You understand that better than anyone in the ring.", "Joaquim, a lanterna não briga com o escuro. Ela só fica acesa. Você entende isso melhor do que qualquer um no anel."),
                ("Maria, there is a lamp in this city that waits for you. Not mine to sell: yours to earn. The nine rings will tell you when.", "Maria, tem uma lâmpada nesta cidade esperando por você. Não é minha para vender: é sua para merecer. Os nove anéis vão te dizer quando."),
                ("Maithe, every lantern has a riddle under it. You answered three before I finished hanging them. Take one for your brother.", "Maithe, toda lanterna tem uma charada embaixo. Você respondeu três antes de eu terminar de pendurar. Leva uma para o seu irmão."),
                ("Ícaro, the fish lantern is not a kite. ...It flew? Bring it back down before the bell, please.", "Ícaro, a lanterna-peixe não é pipa. ...Ela voou? Traz ela de volta antes do sino, por favor."),
                ("Lavine, Grandma Edna made a lantern here once, round and a little crooked. I kept it. It still lights the best.", "Lavine, a Vó Edna fez uma lanterna aqui uma vez, redonda e meio torta. Eu guardei. Ainda é a que acende melhor."),
                ("The Sun Seal for the White Spires, oil for the outer rings, a riddle for free. Light is the only trade of my lanterns.", "O Selo do Sol para as Torres Brancas, óleo para os anéis de fora, uma charada de graça. Luz é o único ofício das minhas lanternas."));

            AddResident(entries, "lin_deng", "Lin Deng, Master of the Canal", "Lin Deng, o Mestre do Canal",
                ("Joaquim, the canal is like you: calm on top, strong underneath. Help me push the barge and you will see.", "Joaquim, o canal é que nem você: calmo em cima, forte embaixo. Me ajuda a empurrar a barca e você vai ver."),
                ("Maria, four locks, four levels of water. Open them in order and the low quarters breathe. Out of order, we all get wet.", "Maria, quatro eclusas, quatro níveis de água. Abre na ordem e os bairros baixos respiram. Fora de ordem, todo mundo se molha."),
                ("Maithe, the carp that jumps the waterfall becomes a dragon, they say. The one that helps the others jump becomes the eldest.", "Maithe, a carpa que pula a cascata vira dragão, dizem. A que ajuda as outras a pular vira a mais velha."),
                ("Ícaro, the pole is for pushing the barge, not for vaulting over the canal. ...Twice? Go dry off.", "Ícaro, a vara é para empurrar a barca, não para pular o canal. ...Duas vezes? Vai se secar."),
                ("Lavine, your grandpa Jorge fell in my canal on his wedding day. Grandma Edna laughed so hard she fell in too.", "Lavine, o seu avô Jorge caiu no meu canal no dia do casamento. A Vó Edna riu tanto que caiu também."),
                ("Barges of silk, paper, lacquer and tea. Every craft of the middle rings floats past my locks.", "Barcas de seda, papel, laca e chá. Todo ofício dos anéis do meio passa boiando pelas minhas eclusas."));

            AddResident(entries, "fuduodan", "Fuduodan the Reader", "Fuduodan, o Leitor",
                ("Joaquim, two principles: the Light and the Dark. You do not need the book to know which one you guard.", "Joaquim, dois princípios: a Luz e a Escuridão. Você não precisa do livro para saber qual dos dois você guarda."),
                ("Maria, I brought the book over the mountains on a camel. You would have brought it in a notebook, with diagrams.", "Maria, eu trouxe o livro pelas montanhas num camelo. Você teria trazido num caderno, com diagramas."),
                ("Maithe, you read aloud like a lantern: slowly, and everyone around you can see. Read me the next page.", "Maithe, você lê em voz alta que nem uma lanterna: devagar, e todo mundo em volta enxerga. Lê para mim a próxima página."),
                ("Ícaro, the three times: before, now and after. You are always in the now. It is not the worst place to live.", "Ícaro, os três tempos: antes, agora e depois. Você está sempre no agora. Não é o pior lugar para morar."),
                ("Lavine, Grandpa Jorge sat on this step and asked me to read him the part about the moon. Every night, for a month.", "Lavine, o Vô Jorge sentava neste degrau e me pedia para ler a parte da lua. Toda noite, durante um mês."),
                ("Two principles, three times. Sit on the step of the 6th ring and I will read them to you, as I read them to everyone.", "Dois princípios, três tempos. Senta no degrau do 6º anel e eu leio para você, como leio para todo mundo."));

            // ---- Torres Brancas de Amanirenas (altos-elfos; names of the candaces and kings of Kush/Meroë) ----
            AddResident(entries, "amanirenas", "Queen Amanirenas, the Guardian", "Rainha Amanirenas, a Guardiã",
                ("Joaquim, a guardian sees the whole hill from one tower. I have one eye and I see it all. You have two: use them.", "Joaquim, um guardião vê o morro inteiro de uma torre só. Eu tenho um olho e vejo tudo. Você tem dois: usa."),
                ("Maria, the light of Mingcheng reaches my towers by the mirrors. Without them we would be white stone in the dark.", "Maria, a luz de Mingcheng chega às minhas torres pelos espelhos. Sem eles a gente seria pedra branca no escuro."),
                ("Maithe, my people are proud, but our children follow you down the leaf bridges. That says more about you than about us.", "Maithe, o meu povo é orgulhoso, mas as nossas crianças seguem você pelas pontes de folha. Isso diz mais de você do que de nós."),
                ("Ícaro, the tallest tower is not for climbing from the outside. ...You are already halfway? Then wave to me from the top.", "Ícaro, a torre mais alta não é para escalar por fora. ...Já está na metade? Então acena para mim lá de cima."),
                ("Lavine, Grandma Edna came to my towers with the Sun Seal when she was your age. She said the mirrors looked like the moon on the water.", "Lavine, a Vó Edna veio às minhas torres com o Selo do Sol quando tinha a sua idade. Disse que os espelhos pareciam a lua na água."),
                ("Seven spires, one light. Whoever carries the Sun Seal is a guest; whoever breaks a mirror answers to me.", "Sete torres, uma luz. Quem leva o Selo do Sol é hóspede; quem quebra um espelho responde para mim."));

            AddResident(entries, "amanitore", "Amanitore the Builder", "Amanitore, a Construtora",
                ("Joaquim, stone holds stone. A good tower is like a good guardian: it does not move, and everyone leans on it.", "Joaquim, pedra segura pedra. Torre boa é que nem guardião bom: não se mexe, e todo mundo se apoia nela."),
                ("Maria, every spire is a pyramid that learned to grow thin. Measure the base and you will know the height.", "Maria, toda torre é uma pirâmide que aprendeu a ficar fina. Mede a base e você vai saber a altura."),
                ("Maithe, I need someone to hold the plans while I tap the stone. You hold them straight. Stay.", "Maithe, preciso de alguém para segurar os planos enquanto eu bato na pedra. Você segura reto. Fica."),
                ("Ícaro, no, we will not build a tower for your grenades. ...A small one? A very small one. Behind the garden.", "Ícaro, não, a gente não vai construir uma torre para as suas granadas. ...Uma pequena? Bem pequena. Atrás do jardim."),
                ("Lavine, Grandpa Jorge carried stones for my first tower. He dropped one on his foot and kept singing.", "Lavine, o Vô Jorge carregou pedras para a minha primeira torre. Deixou uma cair no pé e continuou cantando."),
                ("A dome of glass, a band of gold, a mirror for the sun. Ask me, and your house will have a corner of the White Spires.", "Uma cúpula de vidro, uma faixa de ouro, um espelho para o sol. Me pede, e a sua casa vai ter um canto das Torres Brancas."));

            AddResident(entries, "natakamani", "Natakamani, Master of the Mirrors", "Natakamani, o Mestre dos Espelhos",
                ("Joaquim, a mirror does not make light, it only passes it on. That is not small: that is the whole work of a guardian.", "Joaquim, o espelho não faz luz, só passa adiante. Isso não é pouco: é o trabalho inteiro de um guardião."),
                ("Maria, the angle of the beam changes with the hour. I have the tables. You will want the tables.", "Maria, o ângulo do raio muda com a hora. Eu tenho as tabelas. Você vai querer as tabelas."),
                ("Maithe, polish slowly, in circles. The mirror shows you the face you bring to it. Bring a kind one.", "Maithe, pule devagar, em círculos. O espelho mostra o rosto que você traz para ele. Traz um gentil."),
                ("Ícaro, if you point the big mirror at the market, the awnings catch fire. ...How did you know? Do not answer.", "Ícaro, se você apontar o espelho grande para a feira, os toldos pegam fogo. ...Como você sabia? Não responde."),
                ("Lavine, Grandma Edna polished this mirror for a whole afternoon and gave it back to me without one scratch. I still use it.", "Lavine, a Vó Edna poliu este espelho uma tarde inteira e me devolveu sem um arranhão. Eu ainda uso."),
                ("Stones for the forge, refined with reflected light. Less spark-stone wasted on the hardest steps.", "Pedras para a forja, refinadas com luz refletida. Menos pedra-faísca desperdiçada nos degraus mais difíceis."));

            AddResident(entries, "shanakdakhete", "Shanakdakhete, Who Teaches the Light", "Shanakdakhete, a que Ensina a Luz",
                ("Joaquim, your light is slow and deep, like water. Do not rush it. The deep ones last longer.", "Joaquim, a sua luz é lenta e funda, que nem água. Não apressa. As fundas duram mais."),
                ("Maria, you are a lamp, cousin of my students. Come to class at dawn: the first beam is the most honest.", "Maria, você é uma lâmpada, prima dos meus alunos. Vem para a aula de manhã cedo: o primeiro raio é o mais honesto."),
                ("Maithe, the wind carries light too, did you know? Ask the leaves on the bridges. They are always shining.", "Maithe, o vento carrega luz também, sabia? Pergunta para as folhas das pontes. Elas estão sempre brilhando."),
                ("Ícaro, fire is light that is in a hurry. Breathe. Now throw it. ...Better. Much better.", "Ícaro, fogo é luz com pressa. Respira. Agora joga. ...Melhor. Muito melhor."),
                ("Lavine, the moon is the barge that carries the light home. Grandpa Jorge used to say that too, did you know?", "Lavine, a lua é a barca que leva a luz para casa. O Vô Jorge também dizia isso, sabia?"),
                ("Turn the crystal, catch the beam, pass it on. That is the first lesson, and the last.", "Gira o cristal, pega o raio, passa adiante. Essa é a primeira lição, e a última."));

            AddResident(entries, "arkamani", "Arkamani, Reader of the Stars", "Arkamani, o que Lê as Estrelas",
                ("Joaquim, the slowest stars are the ones that hold the sky together. Guardians, like you.", "Joaquim, as estrelas mais lentas são as que seguram o céu inteiro. Guardiãs, que nem você."),
                ("Maria, I have counted every star over the White Spires. Four thousand and twelve. Check my count; I trust you.", "Maria, eu contei todas as estrelas em cima das Torres Brancas. Quatro mil e doze. Confere a minha conta; eu confio em você."),
                ("Maithe, the crane of the north points to the pass. Follow it and you will never lose your cousins.", "Maithe, o grou do norte aponta para o passo. Segue ele e você nunca perde os seus primos."),
                ("Ícaro, that is not a falling star, it is a lantern from Mingcheng. ...Yes, it is still beautiful. Make the wish.", "Ícaro, isso não é estrela cadente, é uma lanterna de Mingcheng. ...Sim, continua bonita. Faz o pedido."),
                ("Lavine, there is a little star I named after your grandma Edna. It blinks twice, then rests. Just like her laugh.", "Lavine, tem uma estrelinha que eu batizei com o nome da sua avó Edna. Pisca duas vezes, depois descansa. Igualzinho à risada dela."),
                ("The sky is a bestiary: lions, cranes, a carp that jumps. Bring me what you see at night and I will write it down.", "O céu é um bestiário: leões, grous, uma carpa que pula. Me traz o que você vê de noite e eu anoto."));

            // ---- Acampamento de Soslan (names of the Nart sagas of Ossetia) ----
            AddResident(entries, "soslan", "Soslan, the Steel-Skinned", "Soslan, o de Pele de Aço",
                ("Joaquim, a guardian runs with the herd, not in front of it. Learn the pace of the slowest foal and you will never lose one.", "Joaquim, um guardião corre com a manada, não na frente dela. Aprende o passo do potro mais lento e você nunca vai perder nenhum."),
                ("Maria, my skin was tempered in the smith's water when I was a boy. It does not hurt. It only clinks when I laugh.", "Maria, a minha pele foi temperada na água do ferreiro quando eu era menino. Não dói. Só tilinta quando eu rio."),
                ("Maithe, the camp moves three times a week and the eldest always knows where the next fire will be. That is you, now.", "Maithe, o acampamento muda três vezes por semana e a mais velha sempre sabe onde vai ser a próxima fogueira. Agora é você."),
                ("Ícaro, a race? Across the whole steppe? ...Your grandpa asked me the same. Ask Satana how that ended.", "Ícaro, uma corrida? Pela estepe inteira? ...O seu avô me pediu a mesma coisa. Pergunta para a Satana como terminou."),
                ("Lavine, Grandpa Jorge lost to me by a nose, and Grandma Edna said my nose was cheating. She was right. She is always right.", "Lavine, o Vô Jorge perdeu para mim por um focinho, e a Vó Edna disse que o meu focinho estava roubando. Ela tinha razão. Ela sempre tem."),
                ("Three camps a week, one herd, no foal left behind. That is the law of the felt tents.", "Três acampamentos por semana, uma manada, nenhum potro para trás. Essa é a lei das tendas de feltro."));

            AddResident(entries, "batradz", "Batradz the Smith-Warrior", "Batradz, o Ferreiro-Guerreiro",
                ("Joaquim, I forge horseshoes for four legs and buckles for two arms. You only need the buckles. Lucky.", "Joaquim, eu forjo ferradura para quatro patas e fivela para dois braços. Você só precisa das fivelas. Sorte sua."),
                ("Maria, the hammer falls three times and rests once. Count with me; the iron listens to the count.", "Maria, o martelo cai três vezes e descansa uma. Conta comigo; o ferro escuta a contagem."),
                ("Maithe, a wheel of the wagons broke last night. The eldest holds the rim while I strike. Steady hands, like yours.", "Maithe, uma roda das carroças quebrou ontem à noite. A mais velha segura o aro enquanto eu bato. Mão firme, igual à sua."),
                ("Ícaro, do not touch the red iron. ...Do not touch the black iron either, it was red a minute ago.", "Ícaro, não encosta no ferro vermelho. ...Nem no ferro preto, ele estava vermelho faz um minuto."),
                ("Lavine, Grandma Edna mended my apron with felt thread and it has outlived three anvils. Look, her stitches are still here.", "Lavine, a Vó Edna remendou o meu avental com linha de feltro e ele já durou mais que três bigornas. Olha, os pontos dela ainda estão aqui."),
                ("Horseshoes, wheel rims, a buckle for a saddle. In a camp that moves, the forge moves too.", "Ferradura, aro de roda, fivela de sela. Num acampamento que anda, a forja anda junto."));

            AddResident(entries, "satana", "Satana, the Wise of the House", "Satana, a Sábia da Casa",
                ("Joaquim, a guardian who listens is worth two who shout. You listen. Keep doing it.", "Joaquim, um guardião que escuta vale por dois que gritam. Você escuta. Continua assim."),
                ("Maria, the guardian lamp burns felt oil and good words. I give it the oil; you give it the words.", "Maria, a lâmpada guardiã queima óleo de feltro e palavras boas. Eu dou o óleo; você dá as palavras."),
                ("Maithe, the eldest of a family is like the lamp in a camp: everyone looks at her when the night comes.", "Maithe, a mais velha de uma família é como a lâmpada num acampamento: todo mundo olha para ela quando a noite chega."),
                ("Ícaro, you asked how the race ended? Soslan won, Jorge laughed, and the two of them fell asleep by the fire. That is how races should end.", "Ícaro, você perguntou como terminou a corrida? O Soslan ganhou, o Jorge riu, e os dois dormiram perto da fogueira. É assim que corrida deve terminar."),
                ("Lavine, Grandma Edna sat with me under this lamp for a whole night and we did not say a word. Some nights are like that, and they are good.", "Lavine, a Vó Edna sentou comigo debaixo desta lâmpada uma noite inteira e a gente não disse uma palavra. Tem noite assim, e é boa."),
                ("A camp that moves needs a light that stays. Sit, eat, and tell me where you are going.", "Um acampamento que anda precisa de uma luz que fica. Senta, come e me conta para onde vocês vão."));

            AddResident(entries, "syrdon", "Syrdon the Trickster", "Syrdon, o Trapaceiro",
                ("Joaquim, this saddle has a little bell sewn inside. When you sit, it plays. ...No, I will not tell you which saddle.", "Joaquim, esta sela tem um sininho costurado por dentro. Quando você senta, ele toca. ...Não, não vou contar qual sela."),
                ("Maria, the prices are fair. The prank is free. You only find out about the prank later.", "Maria, os preços são justos. A pegadinha é de graça. Você só descobre a pegadinha depois."),
                ("Maithe, the eldest always checks the straps twice. That is why I tie them three times. Fair is fair.", "Maithe, a mais velha sempre confere as correias duas vezes. Por isso eu amarro três. Justo é justo."),
                ("Ícaro, finally a customer with taste. Want to hide a frog in Soslan's felt cap? I have the frog.", "Ícaro, finalmente um freguês de bom gosto. Quer esconder um sapo no gorro de feltro do Soslan? O sapo eu tenho."),
                ("Lavine, Grandpa Jorge once sold me a saddle blanket that turned out to be my own. Best trick anyone ever played on me. I still owe him one.", "Lavine, o Vô Jorge uma vez me vendeu uma manta de sela que era minha mesmo. O melhor truque que já fizeram comigo. Ainda devo uma para ele."),
                ("Felt blankets, feed, boots for the steppe. And, with every purchase, one good prank. Tell no one.", "Mantas de feltro, ração, botas para a estepe. E, a cada compra, uma pegadinha boa. Não conta para ninguém."));

            AddResident(entries, "uryzmaeg", "Uryzmæg, the Old Chief", "Uryzmæg, o Velho Chefe",
                ("Joaquim, I have led this camp for two hundred moves. The secret is to always look back once before leaving.", "Joaquim, eu guio este acampamento há duzentas mudanças. O segredo é sempre olhar para trás uma vez antes de partir."),
                ("Maria, the Uasamonga, the cup of the Narts, fills by itself for the bravest of the day. Some days it fills for a child.", "Maria, a Uasamonga, a taça dos Nartes, se enche sozinha para o mais corajoso do dia. Tem dia em que ela se enche para uma criança."),
                ("Maithe, a chief speaks last. You already know that. I saw you wait for your cousins to finish.", "Maithe, um chefe fala por último. Você já sabe disso. Eu vi você esperar os seus primos terminarem."),
                ("Ícaro, my legs are old but they still know the way to the river. Walk with me; I will not race you.", "Ícaro, as minhas pernas são velhas mas ainda sabem o caminho do rio. Anda comigo; não vou apostar corrida com você."),
                ("Lavine, the cup filled once for Grandma Edna, when she carried a lost foal back on her shoulders. She was smaller than you.", "Lavine, a taça se encheu uma vez para a Vó Edna, quando ela trouxe um potro perdido de volta nos ombros. Ela era menor que você."),
                ("The steppe is wide and the camp is small. That is why we keep it full of good people.", "A estepe é larga e o acampamento é pequeno. Por isso a gente enche ele de gente boa."));

            // ---- Fortim dos Gigantes-da-Colina (names of the Manichaean Book of Giants) ----
            AddResident(entries, "baraqel", "Baraqel, Chief of the Fort", "Baraqel, o Chefe do Fortim",
                ("Joaquim, a guardian your size! Climb the spoon and stand by me; from up here you can see the whole hill.", "Joaquim, um guardião do seu tamanho! Sobe na colher e fica do meu lado; daqui de cima dá para ver o morro inteiro."),
                ("Maria, we giants count slowly because our numbers are big. One, ...two. See? It takes a while.", "Maria, nós gigantes contamos devagar porque os nossos números são grandes. Um, ...dois. Viu? Demora."),
                ("Maithe, the doors of the Fort are tall so that nobody has to bend to come in. Not even a giant who made mistakes.", "Maithe, as portas do Fortim são altas para que ninguém precise se abaixar para entrar. Nem um gigante que errou."),
                ("Ícaro, do not jump from the table. ...Fine, jump, I will catch you. ...Again? Only once more.", "Ícaro, não pula da mesa. ...Tá bom, pula, eu te pego. ...De novo? Só mais uma vez."),
                ("Lavine, Grandpa Jorge fell asleep on that spoon before the soup. Grandma Edna ate his bowl and said it was justice.", "Lavine, o Vô Jorge dormiu naquela colher antes da sopa. A Vó Edna comeu a tigela dele e disse que era justiça."),
                ("Long ago we dreamt of the end and we were sorry. Now we cook, we keep the fire, and we build doors that are too tall.", "Faz muito tempo a gente sonhou com o fim e se arrependeu. Agora a gente cozinha, cuida do fogo e faz portas altas demais."));

            AddResident(entries, "virogdad", "Virogdad the Dreamer", "Virogdad, o Sonhador",
                ("Joaquim, last night I dreamt of a guardian made of mud who never sank. Was it you? It looked like you.", "Joaquim, ontem à noite eu sonhei com um guardião feito de lama que nunca afundava. Era você? Parecia você."),
                ("Maria, I write every dream on a clay tablet. Good ones on the left, bad ones on the right. The left shelf is longer.", "Maria, eu escrevo todo sonho numa tábua de barro. Os bons na esquerda, os ruins na direita. A prateleira da esquerda é mais comprida."),
                ("Maithe, one dream a day points to a hidden chest on the hill. The eldest reads the map; I only read the dream.", "Maithe, um sonho por dia aponta um baú escondido no morro. A mais velha lê o mapa; eu só leio o sonho."),
                ("Ícaro, you dreamt of flying? Then it is written. Left shelf, top row, next to the dream of the big soup.", "Ícaro, você sonhou que voava? Então está escrito. Prateleira da esquerda, fileira de cima, do lado do sonho da sopa grande."),
                ("Lavine, Grandma Edna told me her dream of a little girl who carries gold nobody else can lift. I wrote it down. I think it is you.", "Lavine, a Vó Edna me contou um sonho dela, de uma menina pequena que carrega um ouro que ninguém mais consegue levantar. Eu escrevi. Acho que é você."),
                ("We giants once had a terrible dream and woke up sorry. Since then, I read the dreams so that they come true kindly.", "Nós gigantes tivemos um sonho terrível uma vez e acordamos arrependidos. Desde então eu leio os sonhos para que eles se cumpram com gentileza."));

            AddResident(entries, "shahmizad", "Shahmizad, the Old Patriarch", "Shahmizad, o Patriarca",
                ("Joaquim, a bowl of the Panela Grande feeds five cousins. Or one giant. Or Joaquim after a long march.", "Joaquim, uma tigela da Panela Grande alimenta cinco primos. Ou um gigante. Ou o Joaquim depois de uma marcha comprida."),
                ("Maria, the secret of the soup is patience and turnips. Mostly turnips.", "Maria, o segredo da sopa é paciência e nabo. Mais nabo."),
                ("Maithe, I was the oldest of the giants who were sorry. Being the eldest means being the first to say: I was wrong.", "Maithe, eu era o mais velho dos gigantes que se arrependeram. Ser o mais velho é ser o primeiro a dizer: eu errei."),
                ("Ícaro, the ladle is not a boat. ...It is not a sword either. Put it back, little one.", "Ícaro, a concha não é um barco. ...Nem uma espada. Põe de volta, pequeno."),
                ("Lavine, Grandma Edna taught me to put cumin in the soup. A giant learned a recipe from a girl. Never too old.", "Lavine, a Vó Edna me ensinou a pôr cominho na sopa. Um gigante aprendeu uma receita com uma menina. Nunca é tarde."),
                ("Food for the whole group from one pot, crampons for the chain of Amirani. Sit; the soup is hot.", "Comida para o grupo inteiro numa panela só, grampos para a corrente de Amirani. Senta; a sopa está quente."));

            // ---- Arvoredo de Haripushpa (the kings of Kucha; Aryaman Puhr of the Bezeklik letters) ----
            AddResident(entries, "haripushpa", "Haripushpa, the Eldest", "Haripushpa, o Mais Velho",
                ("Joaquim... a guardian... stands still... like a tree. ...You are learning.", "Joaquim... um guardião... fica parado... como uma árvore. ...Você está aprendendo."),
                ("Maria... I count... rings. Inside me... there are... nine hundred. ...And one. Today.", "Maria... eu conto... anéis. Dentro de mim... tem... novecentos. ...E um. Hoje."),
                ("Maithe... the eldest... waits. ...For everyone. ...I wait for the snow... to melt.", "Maithe... a mais velha... espera. ...Por todos. ...Eu espero... o degelo."),
                ("Ícaro... do not... run. ...Here... the roots... are listening.", "Ícaro... não... corre. ...Aqui... as raízes... estão escutando."),
                ("Lavine... Grandpa Jorge... sat by me... a whole summer. ...Grandma Edna... came... to take him home.", "Lavine... o Vô Jorge... sentou do meu lado... um verão inteiro. ...A Vó Edna... veio... buscar ele."),
                ("In the grove... time walks... slowly. ...Sit. ...The water... of the thaw... is good.", "No arvoredo... o tempo... anda devagar. ...Senta. ...A água... do degelo... é boa."));

            AddResident(entries, "suvarnapushpa", "Suvarnapushpa the Gardener", "Suvarnapushpa, a Jardineira",
                ("Joaquim, plant it and stand by it. A seedling likes a guardian more than water. Well, almost.", "Joaquim, planta e fica do lado. Muda gosta mais de guardião do que de água. Bom, quase."),
                ("Maria, in the grove the clock walks at half speed, so your plants at home grow twice as fast while you are here.", "Maria, no arvoredo o relógio anda pela metade, então as plantas da sua casa crescem duas vezes mais rápido enquanto você está aqui."),
                ("Maithe, a larch is the only pine that drops its needles in winter. Even trees need a rest, eldest.", "Maithe, o lariço é o único pinheiro que derruba as folhas no inverno. Até árvore precisa descansar, mais velha."),
                ("Ícaro, if you water the seedling twice it does not grow twice. ...Yes, I tried too.", "Ícaro, se você regar a muda duas vezes ela não cresce duas vezes. ...É, eu também tentei."),
                ("Lavine, Grandma Edna took a larch seedling home in her apron. Is it still alive? Tell her I said hello to it.", "Lavine, a Vó Edna levou uma muda de lariço para casa no avental. Ela ainda está viva? Fala para ela que eu mandei um oi para a muda."),
                ("Herbs, fibre, the healing rune. Everything here grew slowly, so everything here lasts.", "Ervas, fibra, a runa de cura. Tudo aqui cresceu devagar, então tudo aqui dura."));

            AddResident(entries, "aryaman", "Aryaman Puhr the Herbalist", "Aryaman Puhr, o Herbolário",
                ("Joaquim, I come from the Zarafshan every spring for the herbs of the grove. Your valley sends its greetings.", "Joaquim, eu venho do Zarafshan toda primavera buscar as ervas do arvoredo. O seu vale manda lembranças."),
                ("Maria, the old trees write letters with their roots. I write mine with ink. Both arrive late, and both arrive.", "Maria, as árvores velhas escrevem cartas com as raízes. Eu escrevo as minhas com tinta. As duas chegam atrasadas, e as duas chegam."),
                ("Maithe, moss on the north side, sap on the south. A herbalist reads a tree the way the eldest reads her cousins.", "Maithe, musgo do lado norte, seiva do lado sul. Um herbolário lê uma árvore do jeito que a mais velha lê os primos."),
                ("Ícaro, that mushroom is not for eating. That one neither. ...That one is a stone, Ícaro.", "Ícaro, esse cogumelo não é de comer. Esse também não. ...Esse aí é uma pedra, Ícaro."),
                ("Lavine, I carry a letter from Grandma Edna to Haripushpa. It is three lines long. He will take a week to answer.", "Lavine, eu trago uma carta da Vó Edna para o Haripushpa. Tem três linhas. Ele vai levar uma semana para responder."),
                ("A Sogdian among trees: I bring letters, I take herbs. The road between them is my house.", "Um sogdiano entre as árvores: eu trago cartas, levo ervas. A estrada entre um e outro é a minha casa."));

            // ---- Vale dos Sátiros (the Phrygian myths of Midas, Marsyas and Gordias; satyrs of the Living Wind) ----
            AddResident(entries, "midas", "Midas, the King of Golden Hands", "Midas, o Rei de Mãos Douradas",
                ("Joaquim, do not pass me the bread. Really. Last time it became a very heavy, very shiny loaf.", "Joaquim, não me passa o pão. Sério. Da última vez virou um pão muito pesado e muito brilhante."),
                ("Maria, gold is pretty for a day. Then you miss the taste of grapes. Remember that when someone offers you gold.", "Maria, ouro é bonito por um dia. Depois você sente falta do gosto da uva. Lembra disso quando alguém te oferecer ouro."),
                ("Maithe, the eldest of you looks at my hands the way a doctor does. Kind and serious. Thank you for not laughing.", "Maithe, a mais velha de vocês olha para as minhas mãos como um médico olha. Gentil e séria. Obrigado por não rir."),
                ("Ícaro, no, you cannot touch my crown to see if you turn gold. ...Yes, it would work. That is why no.", "Ícaro, não, você não pode encostar na minha coroa para ver se vira ouro. ...Sim, ia funcionar. Por isso mesmo não."),
                ("Lavine, Grandma Edna once washed my hands in the river herself and said: there, a king again. I have tried to deserve it since.", "Lavine, a Vó Edna uma vez lavou as minhas mãos no rio ela mesma e disse: pronto, rei de novo. Desde então eu tento merecer."),
                ("Everything I touch turns to gold, except when I wash in the river. Kings should wash more often.", "Tudo o que eu toco vira ouro, menos quando eu me lavo no rio. Rei devia se lavar mais vezes."));

            AddResident(entries, "marsyas", "Marsyas, the Master of the Flute", "Marsyas, o Mestre da Flauta",
                ("Joaquim, you breathe slow and deep, like a guardian. That is half of the flute already. The other half is daring.", "Joaquim, você respira devagar e fundo, como um guardião. Isso já é metade da flauta. A outra metade é coragem."),
                ("Maria, every song of the road is a little map. Learn this one and you will never lose the way to the Vale.", "Maria, toda canção de estrada é um mapinha. Aprende esta e você nunca mais perde o caminho do Vale."),
                ("Maithe, you are the one the Living Wind listens to. Play one note, just one, and hear the peaks answer you.", "Maithe, você é quem o Vento Vivo escuta. Toca uma nota, uma só, e escuta os picos te responderem."),
                ("Ícaro, the double flute is two flutes, so you can play two wrong notes at once. You are a natural.", "Ícaro, a flauta dupla são duas flautas, então dá para tocar duas notas erradas ao mesmo tempo. Você nasceu para isso."),
                ("Lavine, Grandpa Jorge danced at the Baile do Vale until the moon set, and he could not dance at all. That is the best kind of dancer.", "Lavine, o Vô Jorge dançou no Baile do Vale até a lua se pôr, e ele não sabia dançar nada. É o melhor tipo de dançarino."),
                ("Flutes, songs for the road, wind runes. At dusk, the dance. Everyone dances, even the oaks, a little.", "Flautas, canções de estrada, runas de vento. Ao entardecer, o baile. Todo mundo dança, até os carvalhos, um pouquinho."));

            AddResident(entries, "gordias", "Gordias, the Farmer of the Knots", "Gordias, o Fazendeiro dos Nós",
                ("Joaquim, a good knot is like a good guardian: it holds when pulled and lets go when asked.", "Joaquim, um nó bom é como um guardião bom: segura quando puxam e solta quando pedem."),
                ("Maria, I tie the vines so the grapes face the sun. A vine that sees the light grows sweet. People too.", "Maria, eu amarro as videiras para as uvas ficarem de frente para o sol. Videira que vê a luz fica doce. Gente também."),
                ("Maithe, here, hold this end. ...See? The eldest always knows which end to hold. Your brother grabbed the middle.", "Maithe, toma, segura esta ponta. ...Viu? A mais velha sempre sabe qual ponta segurar. O seu irmão pegou no meio."),
                ("Ícaro, that chest opens with a knot puzzle. Solve it and what is inside is yours. ...Take your time. Really.", "Ícaro, aquele baú abre com um quebra-cabeça de nó. Resolve e o que tiver dentro é seu. ...Sem pressa. Sério."),
                ("Lavine, Grandma Edna tied the knot on that gate forty years ago. Nobody has opened it since, and nobody wants to.", "Lavine, a Vó Edna deu o nó daquele portão há quarenta anos. Ninguém abriu desde então, e ninguém quer."),
                ("Rope, vines and knots. Some knots are for opening, some are for keeping. I tie both.", "Corda, videira e nó. Tem nó de abrir e nó de guardar. Eu amarro os dois."));

            // ---- Farnkesh (names of the Manichaean mission to the east; the Living Wind) ----
            AddResident(entries, "shad_ohrmizd", "Shad-Ohrmizd, the Keeper of the Bells", "Shad-Ohrmizd, o Guardião dos Sinos",
                ("Joaquim, up here a guardian guards with his ears. Close your eyes: which bell is out of tune? ...Good. The third.", "Joaquim, aqui em cima um guardião guarda com os ouvidos. Fecha os olhos: qual sino está desafinado? ...Isso. O terceiro."),
                ("Maria, farn is the glory that lives up high, the good luck of the light. It is not ours. We only keep it company.", "Maria, farn é a glória que mora no alto, a boa sorte da luz. Não é nossa. A gente só faz companhia para ela."),
                ("Maithe, the Living Wind is born in these nests. You are the only one of the cousins who can hear it. It has been waiting for you.", "Maithe, o Vento Vivo nasce nestes ninhos. Você é a única dos primos que escuta ele. Ele estava esperando você."),
                ("Ícaro, yes, the big bell rings if you hit it. Everyone in the Vale will know it was you. ...Still want to?", "Ícaro, sim, o sino grande toca se você bater nele. Todo mundo no Vale vai saber que foi você. ...Ainda quer?"),
                ("Lavine, Grandma Edna and Grandpa Jorge sat on that balcony a whole night counting stars between the pinnacles. They lost count and laughed.", "Lavine, a Vó Edna e o Vô Jorge sentaram naquela varanda uma noite inteira contando estrelas entre os pináculos. Perderam a conta e riram."),
                ("Nine pinnacles, a hundred bells, one wind. When the bells all ring, the ill wind stays out.", "Nove pináculos, cem sinos, um vento. Quando os sinos tocam todos, o vento ruim fica do lado de fora."));

            AddResident(entries, "sisin", "Sisin, the Master of the Bridges", "Sisin, o Mestre das Pontes",
                ("Joaquim, step where the plank is tied, not where it is pretty. A bridge is honest about where it holds.", "Joaquim, pisa onde a tábua está amarrada, não onde ela é bonita. A ponte é honesta sobre onde ela segura."),
                ("Maria, I pull the baskets up by hand. Slow going up, fast coming down. Like learning things.", "Maria, eu puxo os cestos na mão. Devagar para subir, rápido para descer. Igual aprender as coisas."),
                ("Maithe, you walk my bridges like they were floors. The others hold the rope; you hold your brother. Good eldest.", "Maithe, você anda nas minhas pontes como se fosse chão. Os outros seguram a corda; você segura o seu irmão. Boa mais velha."),
                ("Ícaro, if you jump on the bridge it bounces. If it bounces I come out. If I come out, no more basket rides for you.", "Ícaro, se você pular na ponte ela balança. Se ela balançar eu apareço. Se eu aparecer, acabou o passeio de cesto para você."),
                ("Lavine, Grandpa Jorge went up in my basket shouting the whole way. Then he asked to go again. Your family is like that.", "Lavine, o Vô Jorge subiu no meu cesto gritando o caminho todo. Depois pediu para ir de novo. A sua família é assim."),
                ("Rope, planks, poles and cloaks for the gorge. Wings of canvas for the islands. Hold the rope when the gust comes.", "Corda, tábua, vara e capa para a garganta. Asas de lona para as ilhas. Segura a corda quando vier a rajada."));

            AddResident(entries, "tadi", "Tadi, the Folder of Wind Paper", "Tadi, a Dobradora de Papel de Vento",
                ("Joaquim, this paper is folded so the wind carries it home. Write a word on it for your sister, and let it go.", "Joaquim, este papel é dobrado para o vento levar de volta para casa. Escreve uma palavra para a sua irmã e solta."),
                ("Maria, my sister Nafsha was healed of a long sickness once, and since then we both make things that fly. Like thank-yous.", "Maria, a minha irmã Nafsha foi curada de uma doença comprida uma vez, e desde então a gente faz coisas que voam. Como obrigados."),
                ("Maithe, the wind paper flies best when the Living Wind is near. Today it flies for you without even trying.", "Maithe, o papel de vento voa melhor quando o Vento Vivo está perto. Hoje ele voa para você sem nem tentar."),
                ("Ícaro, no, you cannot fly holding six kites. ...I tried. I was your age. Sisin still tells the story.", "Ícaro, não, você não consegue voar segurando seis pipas. ...Eu tentei. Tinha a sua idade. O Sisin ainda conta a história."),
                ("Lavine, Grandma Edna folded a wind paper with me and wrote your names on it, all five. It flew so high we never saw it come down.", "Lavine, a Vó Edna dobrou um papel de vento comigo e escreveu o nome de vocês nele, os cinco. Voou tão alto que a gente nunca viu cair."),
                ("Wind paper, kite cloth and a needle. What the wind takes, it brings back to someone.", "Papel de vento, pano de pipa e agulha. O que o vento leva, ele devolve para alguém."));

            AddResident(entries, "ardawan", "Ardawan, the Braider of Rope", "Ardawan, o Trançador de Corda",
                ("Joaquim, nine strands of yak wool make a bridge rope. One strand is weak. Nine is a family. Remember that.", "Joaquim, nove fios de lã de iaque fazem uma corda de ponte. Um fio é fraco. Nove é uma família. Lembra disso."),
                ("Maria, I was a prince of the Parthians once, far away. Now I braid rope. The rope is more useful than the crown ever was.", "Maria, eu fui um príncipe dos partas uma vez, longe daqui. Agora tranço corda. A corda é mais útil do que a coroa jamais foi."),
                ("Maithe, hold this while I braid. ...You hold it like the wind holds a kite: firm, never tight. That is rare.", "Maithe, segura isto enquanto eu tranço. ...Você segura como o vento segura uma pipa: firme, nunca apertado. Isso é raro."),
                ("Ícaro, tug-of-war with a rope maker is a bad idea. ...Fine. One pull. Then you help me sweep.", "Ícaro, cabo de guerra com um cordoeiro é má ideia. ...Tá bom. Uma puxada. Depois você me ajuda a varrer."),
                ("Lavine, the rope of the big bell was braided by me and your grandpa Jorge, one summer. He braided crooked. It still holds.", "Lavine, a corda do sino grande foi trançada por mim e pelo vô Jorge de vocês, num verão. Ele trançava torto. Ainda segura."),
                ("Yak wool, nine strands, a long afternoon. Every bridge in Farnkesh hangs from someone's patience.", "Lã de iaque, nove fios, uma tarde comprida. Toda ponte de Farnkesh fica pendurada na paciência de alguém."));

            // ---- Porto da Lua Branca (the Komi tales of the ficha and the Manichaean letters of Kellis; the Ar Sereno) ----
            AddResident(entries, "yirkap", "Yirkap, the Ski Hunter", "Yirkap, o Caçador de Esqui",
                ("Joaquim, on skis you lean forward and trust the snow. Like a guardian trusts the ones behind him.", "Joaquim, no esqui você inclina para a frente e confia na neve. Igual um guardião confia em quem está atrás dele."),
                ("Maria, I read tracks on the ice like you read books. This one is a fox. This one is your cousin, running.", "Maria, eu leio rastro no gelo como você lê livro. Este é de raposa. Este é do seu primo, correndo."),
                ("Maithe, the eldest always counts the others before crossing the canal. I saw you do it. Good hunter's habit.", "Maithe, a mais velha sempre conta os outros antes de atravessar o canal. Eu vi você contando. Costume de bom caçador."),
                ("Ícaro, no, you cannot ski down the roof of the long house. ...I did, once. The whole quarter still laughs.", "Ícaro, não, você não pode descer de esqui pelo telhado da casa comprida. ...Eu desci, uma vez. O bairro inteiro ainda ri."),
                ("Lavine, Grandpa Jorge raced me across the frozen bay and lost by a whole island. Then he asked Grandma Edna to say he won.", "Lavine, o Vô Jorge apostou corrida comigo pela baía congelada e perdeu por uma ilha inteira. Depois pediu para a Vó Edna dizer que ele ganhou."),
                ("Skis, a bow and a long breath. On the ice, the fast one eats and the patient one comes home.", "Esqui, arco e fôlego comprido. No gelo, o rápido come e o paciente volta para casa."));

            AddResident(entries, "pamour", "Pamour, the Furrier of the Quays", "Pamour, o Peleiro do Cais",
                ("Joaquim, take the thick furs. A guardian who freezes guards nobody. ...Yes, they itch. Cold itches more.", "Joaquim, leva as peles grossas. Guardião que congela não guarda ninguém. ...Sim, pinica. O frio pinica mais."),
                ("Maria, my family writes letters from one end of the world to the other. Write to your grandma from here: the boats carry it.", "Maria, a minha família escreve cartas de uma ponta do mundo à outra. Escreve para a sua avó daqui: os barcos levam."),
                ("Maithe, a boat for the canals, a lamp for the galleries, furs for the tundra. You think of everything before your brother asks.", "Maithe, barco para os canais, lamparina para as galerias, pele para a tundra. Você pensa em tudo antes do seu irmão pedir."),
                ("Ícaro, the white fur is not a costume. ...Fine, one roar. Then you put it back folded.", "Ícaro, a pele branca não é fantasia. ...Tá bom, um rugido. Depois você devolve dobradinha."),
                ("Lavine, Grandma Edna bought a lantern from my father and paid with a jar of apricots. He kept the jar on the shelf for years.", "Lavine, a Vó Edna comprou uma lanterna do meu pai e pagou com um pote de damasco. Ele guardou o pote na prateleira por anos."),
                ("Furs, boats, lamps and tea. Whatever the ice asks for, Pamour has. Pay before the moon turns.", "Pele, barco, lamparina e chá. O que o gelo pedir, o Pamour tem. Paga antes da lua virar."));

            AddResident(entries, "pera", "Pera, the Good Giant", "Pera, o Gigante Bom",
                ("Joaquim, you stand in front of the others like a little wall. Good. Big walls started small too.", "Joaquim, você fica na frente dos outros como uma parede pequena. Bom. Parede grande também começou pequena."),
                ("Maria, every morning I break the ice with one knock, here, where it sings. Listen... There. That note is the canal waking up.", "Maria, toda manhã eu quebro o gelo com uma batida, aqui, onde ele canta. Escuta... Isso. Essa nota é o canal acordando."),
                ("Maithe, you carry your brother's worries and yours. Give me one of them. I am big, I can carry it.", "Maithe, você carrega as preocupações do seu irmão e as suas. Me dá uma delas. Eu sou grande, eu carrego."),
                ("Ícaro, yes, you can sit on my shoulder. No, you cannot steer me with my ears.", "Ícaro, sim, você pode sentar no meu ombro. Não, você não pode me guiar pelas orelhas."),
                ("Lavine, Grandpa Jorge was the only man who ever tried to arm-wrestle me. He lost, laughing, and I let him keep the table.", "Lavine, o Vô Jorge foi o único homem que tentou ganhar de mim na queda de braço. Perdeu rindo, e eu deixei a mesa com ele."),
                ("I am Pera. I break the ice so the boats go out. Giants are good for that, and for reaching high shelves.", "Eu sou o Pera. Quebro o gelo para os barcos saírem. Gigante serve para isso, e para alcançar prateleira alta."));

            AddResident(entries, "makarios", "Makarios, the Keeper of the Lanterns", "Makarios, o Guardião das Lanternas",
                ("Joaquim, a lantern at the end of the quay is a promise: someone is waiting. Keep yours lit for your sister.", "Joaquim, lanterna na ponta do cais é promessa: tem alguém esperando. Mantém a sua acesa para a sua irmã."),
                ("Maria, I write to my sons every week, though the boats are slow. A letter is light you can fold and send.", "Maria, eu escrevo para os meus filhos toda semana, mesmo com os barcos lentos. Carta é luz que dá para dobrar e mandar."),
                ("Maithe, on the full moon the canals freeze and you can walk to the Cais da Lua. Bring your brother: the moon likes families.", "Maithe, na lua cheia os canais congelam e dá para andar até o Cais da Lua. Traz o seu irmão: a lua gosta de família."),
                ("Ícaro, no, the moon lantern is not a ball. ...It would bounce, yes. It would also break. Both.", "Ícaro, não, a lanterna de lua não é bola. ...Ia quicar, sim. Também ia quebrar. As duas coisas."),
                ("Lavine, the Moon Ship knows you. The Ar Sereno is yours; one day you will steer it, the Barqueira da Lua. Until then, help me light the quay.", "Lavine, a Barca da Lua conhece você. O Ar Sereno é seu; um dia você vai guiar ela, a Barqueira da Lua. Até lá, me ajuda a acender o cais."),
                ("Every night I light the lanterns of the Cais da Lua. When the Moon Ship comes, she finds the way by them.", "Toda noite eu acendo as lanternas do Cais da Lua. Quando a Barca da Lua vem, ela acha o caminho por elas."));

            // ---- Hinokura, a Ilha-Forja (Japanese names chosen for their sense, as the ficha asks; the Fogo Vivo) ----
            AddResident(entries, "akane", "Mestra Akane, the Lacquer Master", "Mestra Akane, a Mestra da Laca",
                ("Joaquim, a guardian is like lacquer: nine thin coats, never one thick one. Thick cracks. Thin holds.", "Joaquim, guardião é igual laca: nove demãos finas, nunca uma grossa. Grossa racha. Fina segura."),
                ("Maria, you read slowly and remember everything. That is how lacquer is made: slowly, and it remembers every hand.", "Maria, você lê devagar e lembra de tudo. É assim que se faz laca: devagar, e ela lembra de cada mão."),
                ("Maithe, you dust your brother off before you dust yourself. I saw it. That is a master's habit, not only an elder sister's.", "Maithe, você tira a poeira do seu irmão antes de tirar a sua. Eu vi. Isso é costume de mestra, não só de irmã mais velha."),
                ("Ícaro, do not touch the red door. ...You touched it. Now it has your finger in it forever. Fine. It is a good finger.", "Ícaro, não encosta na porta vermelha. ...Encostou. Agora ela tem o seu dedo nela para sempre. Tudo bem. É um dedo bom."),
                ("Lavine, Grandma Edna sat on my porch a whole afternoon and said the red was the colour of her grandchildren's cheeks in winter.", "Lavine, a Vó Edna sentou na minha varanda uma tarde inteira e disse que o vermelho era a cor da bochecha dos netos dela no inverno."),
                ("Red lacquer, nine coats, a long patience. A hall of Hinokura lasts because nobody hurried it.", "Laca vermelha, nove demãos, uma paciência comprida. Salão de Hinokura dura porque ninguém teve pressa com ele."));

            AddResident(entries, "kurogane", "Kurogane, the Smith of Black Iron", "Kurogane, o Ferreiro do Ferro Preto",
                ("Joaquim, black iron for the front of the line. Heavy, yes. The front is supposed to be heavy.", "Joaquim, ferro preto para a frente da fila. Pesado, sim. A frente é para ser pesada mesmo."),
                ("Maria, crampons for the volcano, a boat for the reef, theriac for the fumaroles. Write it down; your cousins will forget.", "Maria, grampo para o vulcão, barco para o recife, teriaga para as fumarolas. Anota; os seus primos vão esquecer."),
                ("Maithe, you count coins like a smith counts hammer blows: every one, out loud. I like that.", "Maithe, você conta moeda como ferreiro conta martelada: cada uma, em voz alta. Gostei."),
                ("Ícaro, the Living Fire is yours, they say. Then come to the Forja de Hinokura one day and make something that warms people, not only iron.", "Ícaro, o Fogo Vivo é seu, dizem. Então vem um dia na Forja de Hinokura e faz alguma coisa que aqueça gente, não só ferro."),
                ("Lavine, your grandpa Jorge sharpened a kitchen knife here for an hour and then asked if I had any rice. I did.", "Lavine, o seu vô Jorge afiou uma faca de cozinha aqui por uma hora e depois perguntou se eu tinha arroz. Eu tinha."),
                ("Black iron, white heat, cold water. Whatever the isles ask of you, Kurogane forged it first.", "Ferro preto, calor branco, água fria. O que as ilhas pedirem de vocês, o Kurogane forjou antes."));

            AddResident(entries, "sango", "Sango, the Boatwoman", "Sango, a Barqueira",
                ("Joaquim, sit in the middle of the boat. The middle is where the strong one sits, so the boat does not tip.", "Joaquim, senta no meio do barco. O meio é lugar do forte, para o barco não virar."),
                ("Maria, the coral grows one finger in a whole year. Patient, like your books. Do not step on it.", "Maria, o coral cresce um dedo num ano inteiro. Paciente, igual aos seus livros. Não pisa nele."),
                ("Maithe, you look at the sea before your brother jumps in. Good. The sea here is warm, but the reef is glass.", "Maithe, você olha o mar antes do seu irmão pular. Bom. O mar aqui é quente, mas o recife é de vidro."),
                ("Ícaro, no, the lighthouse boat does not go faster if you blow on the lamp. ...It does go a little. Do not tell Akane.", "Ícaro, não, o barco-farol não anda mais rápido se você soprar a lâmpada. ...Anda um pouquinho. Não conta para a Akane."),
                ("Lavine, Grandma Edna rowed out with me to the reef once and sang the whole way. The fish still come up when someone sings.", "Lavine, a Vó Edna remou comigo até o recife uma vez e cantou o caminho todo. Os peixes ainda sobem quando alguém canta."),
                ("Every night I row the Barco-Farol out among the black islets. A light at sea is a promise kept.", "Toda noite eu remo o Barco-Farol entre as ilhotas pretas. Luz no mar é promessa cumprida."));

            AddResident(entries, "hikari", "Hikari, the Keeper of the Sluices", "Hikari, o Guardião das Comportas",
                ("Joaquim, a sluice is a guardian of lava: it opens for the forge that needs it and closes for the house that does not.", "Joaquim, comporta é guardiã de lava: abre para a forja que precisa e fecha para a casa que não precisa."),
                ("Maria, the channels change when the volcano trembles. I write each change in this book. You may read it, slowly.", "Maria, os canais mudam quando o vulcão treme. Eu escrevo cada mudança neste livro. Você pode ler, devagar."),
                ("Maithe, you hold your brother's hand on the bridges. Keep holding. The lava is tame, but it is still lava.", "Maithe, você segura a mão do seu irmão nas pontes. Continua segurando. A lava é mansa, mas ainda é lava."),
                ("Ícaro, the Living Fire is light that refuses to sleep. That is why the forges never go out. You do not sleep much either, I hear.", "Ícaro, o Fogo Vivo é luz que se recusa a dormir. Por isso as forjas nunca apagam. Você também dorme pouco, ouvi dizer."),
                ("Lavine, Grandpa Jorge once opened the wrong sluice and cooked a whole pot of rice in the channel. Grandma Edna never let him forget.", "Lavine, o Vô Jorge uma vez abriu a comporta errada e cozinhou uma panela inteira de arroz no canal. A Vó Edna nunca deixou ele esquecer."),
                ("Open the sluice, the forge wakes; close it, the forge sleeps. All day I choose which light burns.", "Abre a comporta, a forja acorda; fecha, a forja dorme. O dia inteiro eu escolho qual luz arde."));

            // ---- Cais de Mesektet (names of the tales of Kemet, as the ficha asks; the river of the night) ----
            AddResident(entries, "sinuhe", "Sinuhe, the Chief Boatman", "Sinuhe, o Barqueiro-Chefe",
                ("Joaquim, a boatman is like a guardian: he goes first into the dark and comes back last. I went far and came home. You will too.", "Joaquim, barqueiro é igual guardião: entra primeiro no escuro e volta por último. Eu fui longe e voltei para casa. Você também vai."),
                ("Maria, I wrote my whole journey on one papyrus, so I would not forget the way home. Read it someday; it is short, and it is true.", "Maria, eu escrevi a minha viagem inteira num papiro só, para não esquecer o caminho de casa. Lê um dia; é curto, e é verdade."),
                ("Maithe, you count your cousins every time the barque leaves. One, two, three, four, five. Good. I count my rowers the same way.", "Maithe, você conta os seus primos toda vez que a barca sai. Um, dois, três, quatro, cinco. Bom. Eu conto os meus remadores do mesmo jeito."),
                ("Ícaro, no, you cannot steer the Mesektet. ...Just the next hour, then. Hold the oar with both hands. Both.", "Ícaro, não, você não pode guiar a Mesektet. ...Só a próxima hora, então. Segura o remo com as duas mãos. As duas."),
                ("Lavine, the river of the night carries light down so it can rise again. Grandpa Jorge said that is how his rice works too. I did not argue.", "Lavine, o rio da noite leva a luz para baixo para ela poder subir de novo. O Vô Jorge disse que o arroz dele também funciona assim. Eu não discuti."),
                ("Twelve hours, twelve piers, one barque. Every night it goes down the dark river, and every morning it comes back. So did I.", "Doze horas, doze píeres, uma barca. Toda noite ela desce o rio escuro, e toda manhã ela volta. Eu também voltei."));

            AddResident(entries, "khunanup", "Khunanup, the Merchant of the Quay", "Khunanup, o Comerciante do Cais",
                ("Joaquim, a boot of root for the mud, my friend, because a guardian who sinks guards nothing. Beautiful, no? I could say it nine more ways.", "Joaquim, bota de raiz para o lodo, meu amigo, porque guardião que afunda não guarda nada. Bonito, não é? Eu sei dizer de mais nove jeitos."),
                ("Maria, write it down: a boat for the river, a lamp for the night, a water skin for the sands, theriac for the scorpions. I say it well; you write it well.", "Maria, anota: barco para o rio, lamparina para a noite, odre para o areal, teriaga para os escorpiões. Eu falo bonito; você escreve bonito."),
                ("Maithe, you bargain like the river: slow, patient, and you always win in the end. I lowered the price. Do not tell anyone.", "Maithe, você pechincha igual o rio: devagar, paciente, e sempre ganha no fim. Eu baixei o preço. Não conta para ninguém."),
                ("Ícaro, the runic lamp does not get brighter if you shake it. ...It got brighter. How did you do that? Do it again, slowly.", "Ícaro, a lamparina rúnica não fica mais forte se você sacudir. ...Ficou mais forte. Como você fez isso? Faz de novo, devagar."),
                ("Lavine, Grandma Edna bought one lamp from me and talked me down three times. Nobody has ever spoken better than Khunanup on this quay. Except her.", "Lavine, a Vó Edna comprou uma lamparina comigo e me fez baixar o preço três vezes. Ninguém nunca falou mais bonito que o Khunanup neste cais. Menos ela."),
                ("A good word is worth more than gold, they say. Here, a good word and a little gold buys you a boat for the dark river.", "Palavra boa vale mais que ouro, dizem. Aqui, palavra boa e um pouco de ouro compram um barco para o rio escuro."));

            AddResident(entries, "djedi", "Djedi, the Magician", "Djedi, o Mágico",
                ("Joaquim, magic is a shield too: it stands between the dark and the ones behind you. Stand still and let me show you.", "Joaquim, magia também é escudo: fica entre o escuro e quem está atrás de você. Fica parado e deixa eu mostrar."),
                ("Maria, every spell is a sentence. Say it slowly and in the right order, like your books, and the light comes.", "Maria, todo feitiço é uma frase. Fala devagar e na ordem certa, igual nos seus livros, e a luz vem."),
                ("Maithe, you draw your circle and then fix your brother's. That is a teacher's habit. I am a hundred and ten, I know.", "Maithe, você desenha o seu círculo e depois arruma o do seu irmão. Isso é costume de professora. Eu tenho cento e dez anos, eu sei."),
                ("Ícaro, the circle of light is not a game of jumping. ...You jumped. And it held. Hm. Do it again; this time on purpose.", "Ícaro, círculo de luz não é brincadeira de pular. ...Você pulou. E segurou. Hm. Faz de novo; dessa vez de propósito."),
                ("Lavine, Grandma Edna lit a lamp in my circle at the first try. I still tell that story to every pupil. You have her hands.", "Lavine, a Vó Edna acendeu a lamparina no meu círculo de primeira. Eu ainda conto essa história para todo aluno. Você tem as mãos dela."),
                ("A hundred and ten years, and the river is still dark. So I keep drawing circles of light on the sand, one a night.", "Cento e dez anos, e o rio ainda é escuro. Então eu continuo desenhando círculos de luz na areia, um por noite."));

            AddResident(entries, "setne", "Setne, the Librarian of the Caderno", "Setne, o Bibliotecário do Caderno",
                ("Joaquim, I searched all my life for one old book. Now I keep the Caderno, and every creature you face is written in it. Look before you fight.", "Joaquim, eu procurei a vida toda um livro antigo. Agora eu guardo o Caderno, e todo bicho que você enfrenta está escrito nele. Olha antes de lutar."),
                ("Maria, you may borrow any papyrus on this quay. Any one. Just bring it back before the twelfth hour; that is the only rule.", "Maria, você pode levar emprestado qualquer papiro deste cais. Qualquer um. Só traz de volta antes da décima segunda hora; é a única regra."),
                ("Maithe, you put the scrolls back where they were, in order. Do you know how rare that is? Come and help me whenever you like.", "Maithe, você guarda os rolos onde estavam, em ordem. Sabe como isso é raro? Vem me ajudar quando quiser."),
                ("Ícaro, no, the crocodile page does not need a drawing of you riding it. ...It is a good drawing. I will keep it.", "Ícaro, não, a página do crocodilo não precisa de um desenho seu montado nele. ...É um desenho bom. Vou guardar."),
                ("Lavine, Grandpa Jorge wrote a recipe for rice on the back of one of my papyri. I never erased it. It is filed under 'wisdom'.", "Lavine, o Vô Jorge escreveu uma receita de arroz no verso de um dos meus papiros. Eu nunca apaguei. Está guardada em 'sabedoria'."),
                ("A book is a lamp you can carry down the river of the night. I keep as many as the quay can hold.", "Livro é lamparina que dá para levar pelo rio da noite. Eu guardo quantos o cais aguentar."));

            // ---- Dur-Jarras (names of the Mesopotamian tales, as the ficha asks; the ziggurat of five steps) ----
            AddResident(entries, "utnapishtim", "Utnapishtim, Keeper of the Clock", "Utnapishtim, o Guardião do Relógio",
                ("Joaquim, stand on the plinth and tell me where the shadow falls. Good. A boy who can read the shadow does not get lost in the desert.", "Joaquim, sobe no pedestal e me diz onde a sombra cai. Bom. Menino que lê a sombra não se perde no deserto."),
                ("Maria, I have seen the water rise over everything, and I still keep this clock wound. Somebody must know what time it is when the water goes down.", "Maria, eu vi a água subir por cima de tudo, e mesmo assim dou corda neste relógio. Alguém precisa saber que horas são quando a água baixa."),
                ("Maithe, you walk the top of the canal wall without looking down. I did not look down either, the day of the flood. Be careful anyway.", "Maithe, você anda em cima do muro do canal sem olhar para baixo. Eu também não olhei, no dia do dilúvio. Cuidado assim mesmo."),
                ("Ícaro, no, the clock does not go faster if you push the gnomon. ...Put it back. Exactly where it was.", "Ícaro, não, o relógio não anda mais rápido se você empurrar a haste. ...Põe de volta. Direitinho onde estava."),
                ("Lavine, Grandma Edna once sat here a whole afternoon and asked me nothing about the flood. Only whether I had eaten. Nobody had asked that in a thousand years.", "Lavine, a Vó Edna sentou aqui uma tarde inteira e não me perguntou nada do dilúvio. Só se eu tinha comido. Ninguém perguntava isso fazia mil anos."),
                ("Zurvan is only a clock here, child. An hour is enough to keep; no one needs to worship it.", "Zurvan aqui é só um relógio, criança. Uma hora já basta para cuidar; ninguém precisa adorar ela."));

            AddResident(entries, "atrahasis", "Atrahasis, Master of the Rafts", "Atrahasis, o Mestre das Balsas",
                ("Joaquim, smell that? Bitumen. It holds a brick wall and it holds a boat, and it will hold your boots if you step in it. Buy something before you try.", "Joaquim, sentiu o cheiro? Betume. Segura muro de tijolo e segura barco, e segura a sua bota também se você pisar. Compra alguma coisa antes de tentar."),
                ("Maria, I sealed one boat for the whole world once. Your raft is much easier. Sit down and hold the rope.", "Maria, eu já calafetei um barco para o mundo inteiro. A sua balsa é bem mais fácil. Senta e segura a corda."),
                ("Maithe, you tie a better knot than my apprentices. Do not tell them; they already sulk.", "Maithe, você dá um nó melhor que os meus aprendizes. Não conta para eles; eles já ficam de cara amarrada."),
                ("Ícaro, the pitch burns, yes. Everyone who is standing next to it burns too. Stand far away, and then we can talk.", "Ícaro, o piche queima, sim. Queima também todo mundo que estiver do lado. Fica bem longe, e aí a gente conversa."),
                ("Lavine, your grandpa Jorge came for bitumen to fix a roof and stayed to tell me how I should have built the boat. He was right about the door.", "Lavine, o Vô Jorge de vocês veio buscar betume para arrumar um telhado e ficou me ensinando como eu devia ter feito o barco. Ele tinha razão sobre a porta."),
                ("Bitumen, reeds, rope, water skins. The desert asks for one and the pitch for the other. I sell both.", "Betume, junco, corda, odre. O deserto pede um e o piche pede o outro. Eu vendo os dois."));

            AddResident(entries, "sin_leqi_unninni", "Sin-leqi-unninni, the Librarian", "Sin-leqi-unninni, o Bibliotecário",
                ("Joaquim, read me this tablet. Slowly. Yes, that sign is 'friend'; I copied it so many times I forgot what it meant. Thank you.", "Joaquim, lê essa tábua para mim. Devagar. Isso, esse sinal é 'amigo'; copiei tantas vezes que esqueci o que queria dizer. Obrigado."),
                ("Maria, you may write your name on the wet clay if you do it small. In a thousand years someone will be glad you were here.", "Maria, você pode escrever o seu nome no barro molhado, desde que pequeno. Daqui a mil anos alguém vai gostar de saber que você esteve aqui."),
                ("Maithe, you never drop a tablet. I watched. Three kings have held these tablets, and two of them dropped one.", "Maithe, você nunca derruba uma tábua. Eu reparei. Três reis já seguraram estas tábuas, e dois deles derrubaram uma."),
                ("Ícaro, no, the tablets do not need a drawing of you on a lion. ...It is a good lion. I will fire it with the others.", "Ícaro, não, as tábuas não precisam de um desenho seu em cima de um leão. ...O leão ficou bom. Vou queimar junto com as outras."),
                ("Lavine, Vó Edna's recipe is the most copied tablet in this house. I am not joking. People come from the Cedral for it.", "Lavine, a receita da Vó Edna é a tábua mais copiada desta casa. Não estou brincando. Vem gente do Cedral atrás dela."),
                ("Someone copied the tale before me, and someone will copy it after. That is what a tablet is for.", "Alguém copiou o conto antes de mim, e alguém vai copiar depois. Tábua serve para isso."));

            AddResident(entries, "siduri", "Siduri, the Tavern Keeper", "Siduri, a Taberneira",
                ("Joaquim, sit down, you came a long way. Everyone who gets to this end of the canal came a long way. Eat first.", "Joaquim, senta, você veio de longe. Todo mundo que chega nesta ponta do canal veio de longe. Primeiro come."),
                ("Maria, a king once came to my door looking for how never to die. I told him to go home and wash his hair. I still think that was good advice.", "Maria, um rei já veio na minha porta procurando como nunca morrer. Eu falei para ele voltar para casa e lavar o cabelo. Ainda acho que foi um bom conselho."),
                ("Maithe, you helped carry the jars without anyone asking. Your grandmother would notice. I noticed.", "Maithe, você ajudou a carregar os jarros sem ninguém pedir. A sua avó ia reparar. Eu reparei."),
                ("Ícaro, the jar is not a drum. ...Fine, one song. Then you eat.", "Ícaro, jarro não é tambor. ...Tá bom, uma música. Depois você come."),
                ("Lavine, Vô Jorge drank one cup here and told me it needed rapadura. It did not. I put some in anyway, and now people ask for it.", "Lavine, o Vô Jorge tomou um copo aqui e disse que faltava rapadura. Não faltava. Botei assim mesmo, e agora o pessoal pede."),
                ("The world ends a little past my door, they say. It is a good place for a tavern.", "Dizem que o mundo acaba um pouco depois da minha porta. É um bom lugar para uma taberna."));

            // ---- Fortaleza da Última Lâmpada (names of the Kellis letters, as the ficha asks; a lamp and its oil) ----
            AddResident(entries, "pamour_lampadas", "Pamour, Commander of the Lamps", "Pamour, o Comandante das Lâmpadas",
                ("Joaquim, stand here on the wall and count the niches with me. Whoever counts the lamps does not sleep. I have not slept since the Bema before last.", "Joaquim, fica aqui na muralha e conta os nichos comigo. Quem conta as lâmpadas não dorme. Eu não durmo desde o penúltimo Bema."),
                ("Maria, your light goes farther into the dark than any of my lamps. Don't tell the lamps.", "Maria, a sua luz vai mais fundo na treva do que qualquer lâmpada minha. Não conta para as lâmpadas."),
                ("Maithe, you walked along the top of the rampart over the abyss and did not look down once. I did not order that. Do it again and I will make you a sergeant.", "Maithe, você andou em cima da muralha sobre o abismo sem olhar para baixo nenhuma vez. Eu não mandei isso. Faz de novo que eu te ponho de sargento."),
                ("Ícaro, no, the abyss does not throw the stone back. ...Yes, I know you already tried. Leave my rampart in one piece.", "Ícaro, não, o abismo não devolve a pedra. ...Sim, eu sei que você já tentou. Deixa a minha muralha inteira."),
                ("Lavine, Grandpa Jorge climbed up here to tell me the rampart was crooked. It is crooked. Five hundred years, and nobody else had noticed.", "Lavine, o Vô Jorge subiu aqui para dizer que a muralha estava torta. Ela está torta. Quinhentos anos, e ninguém mais tinha reparado."),
                ("Lit: a thousand. Out: too many. Whoever walks past a dark niche with oil in hand lights it. That is the whole of the law here.", "Acesas: mil. Apagadas: demais. Quem passa por um nicho escuro com óleo na mão acende. Essa é toda a lei daqui."));

            AddResident(entries, "tehat_pavios", "Tehat, Weaver of Wicks", "Tehat, a Tecelã de Pavios",
                ("Joaquim, take this wick; hold it and pull. See? It does not break. That is all I can do for your courage, and it is quite a lot.", "Joaquim, pega esse pavio; segura e puxa. Viu? Não quebra. É tudo que eu posso fazer pela sua coragem, e já é bastante."),
                ("Maria, every wick I twist has a little of the wool from your grandmother's shawl in it. She sent me a whole bag and said to burn it well.", "Maria, todo pavio que eu torço tem um pouco da lã do xale da sua avó. Ela me mandou um saco inteiro e disse para queimar direito."),
                ("Maithe, you tie the knot at the end of a wick better than my apprentices. I am keeping that wick. It will burn the longest.", "Maithe, você dá o nó no fim do pavio melhor que os meus aprendizes. Vou guardar esse pavio. É o que vai queimar mais."),
                ("Ícaro, wicks are not fuses. ...I can see from your face that you already knew that, and that you checked anyway.", "Ícaro, pavio não é estopim. ...Dá para ver na sua cara que você já sabia, e que testou assim mesmo."),
                ("Lavine, Vó Edna sent a jar of oil and a letter that only said: 'Eat something.' I keep the letter next to the oil.", "Lavine, a Vó Edna mandou um jarro de óleo e uma carta que só dizia: 'Come alguma coisa.' Eu guardo a carta do lado do óleo."),
                ("Bubbles for the drowned halls, wings for the shaft, oil for the dark. What goes down must see. I sell the seeing.", "Bolha para os salões afogados, asa para o poço, óleo para o escuro. Quem desce precisa enxergar. Eu vendo o enxergar."));

            AddResident(entries, "makarios_cartas", "Makarios, Writer of Letters", "Makarios, o Escritor de Cartas",
                ("Joaquim, tell your grandpa Jorge his letter arrived. All nine pages of it. It was about the rain.", "Joaquim, fala para o seu Vô Jorge que a carta dele chegou. As nove páginas. Era sobre a chuva."),
                ("Maria, sit and I will read you what I wrote to my sons this week. Nothing happened. I wrote it down anyway. That is what a letter is for.", "Maria, senta que eu leio o que escrevi para os meus filhos esta semana. Não aconteceu nada. Escrevi assim mesmo. Carta serve para isso."),
                ("Maithe, you carried my bag of letters to the gate without reading one. My own sons would not have managed it.", "Maithe, você levou o meu saco de cartas até o portão sem ler nenhuma. Nem os meus filhos conseguiriam."),
                ("Ícaro, the ink is not paint. ...It is a very good drawing of the Treva. I will send it to my son; he needs to laugh.", "Ícaro, tinta não é tinta de pintar. ...É um desenho muito bom da Treva. Vou mandar para o meu filho; ele precisa rir."),
                ("Lavine, write to your grandmother. Vó Edna will not say she is waiting. I am telling you she is waiting.", "Lavine, escreve para a sua avó. A Vó Edna não vai dizer que está esperando. Eu estou dizendo que ela está esperando."),
                ("Far away is only a place letters have not reached yet. I write, and I wait for the road to clear.", "Longe é só um lugar onde a carta ainda não chegou. Eu escrevo, e espero a estrada abrir."));

            // ---- Cais da Coluna (Ammo and Adda at the end of the road; the Remadores do not speak) ----
            AddResident(entries, "ammo_caravaneiro", "Ammo, the Caravaneer", "Ammo, o Caravaneiro",
                ("Joaquim, I walked every road on that island below, and now the road just stops at a pier. Good thing you came. I was running out of people to tell it to.", "Joaquim, eu andei todas as estradas dessa ilha aí embaixo, e agora a estrada acaba num píer. Ainda bem que você veio. Eu já estava ficando sem gente para contar."),
                ("Maria, every city sent a present up with me, and every one of them says it is for you cousins. I told them you would only share it. They said that was the point.", "Maria, cada cidade mandou um presente comigo, e todos dizem que é para vocês primos. Eu falei que vocês iam dividir. Disseram que era essa a ideia."),
                ("Maithe, you went up the barge's rope without touching the gangway. Teimosa would have respected that. Teimosa respected nobody.", "Maithe, você subiu pela corda da barca sem encostar na prancha. A Teimosa teria respeitado isso. A Teimosa não respeitava ninguém."),
                ("Ícaro, no, you cannot take the mast off to see how it works. ...You already asked the Remadores, didn't you. They do not answer. That is not a yes.", "Ícaro, não, você não pode tirar o mastro para ver como funciona. ...Você já perguntou para os Remadores, não foi. Eles não respondem. Isso não é um sim."),
                ("Lavine, your Vô Jorge and I shared a road for thirty years, and he never once let me tell the story of the bridge my way. Vó Edna says he was right. I am still telling it my way.", "Lavine, o seu Vô Jorge e eu dividimos estrada por trinta anos, e ele nunca me deixou contar a história da ponte do meu jeito. A Vó Edna diz que ele tinha razão. Eu continuo contando do meu jeito."),
                ("This is the last caravan. I have said that before. This time the road agrees with me.", "Essa é a última caravana. Eu já disse isso antes. Dessa vez a estrada concorda comigo."));

            AddResident(entries, "adda_roda", "Adda, Keeper of the Wheel", "Adda, o Guardião da Roda",
                ("Joaquim, the Roda Celeste has twelve jars and I have counted them every morning since the Tower let me go. There are still twelve. You would be surprised how good that feels.", "Joaquim, a Roda Celeste tem doze jarras e eu conto todas elas toda manhã desde que a Torre me dispensou. Continuam doze. Você nem imagina como isso faz bem."),
                ("Maria, every jar that comes round is full of light going home, and I cry every time one comes round. Twelve jars, Maria. It is a long day.", "Maria, cada jarra que passa vem cheia de luz voltando para casa, e eu choro toda vez que uma passa. Doze jarras, Maria. O dia é comprido."),
                ("Maithe, you stood on the rim of the Wheel while it turned and did not hold on. I kept the Tower for forty years and never once did that. Don't do it again; do it again.", "Maithe, você ficou de pé na borda da Roda com ela girando e não se segurou. Eu guardei a Torre quarenta anos e nunca fiz isso. Não faz de novo; faz de novo."),
                ("Ícaro, the Wheel is not stuck. It is slow. ...Put the wrench down, Ícaro. The Wheel is older than wrenches.", "Ícaro, a Roda não está emperrada. Ela é devagar. ...Larga a chave, Ícaro. A Roda é mais velha que chave."),
                ("Lavine, Vó Edna wrote to ask if I am eating. I am a very old man who looks after a wheel in the sky, and I went and made soup.", "Lavine, a Vó Edna escreveu perguntando se eu estou comendo. Eu sou um homem muito velho que cuida de uma roda no céu, e fui lá fazer sopa."),
                ("The Barca da Lua collects it, the Roda lifts it, the Coluna takes it home. I only count the jars.", "A Barca da Lua junta, a Roda sobe, a Coluna leva para casa. Eu só conto as jarras."));

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
