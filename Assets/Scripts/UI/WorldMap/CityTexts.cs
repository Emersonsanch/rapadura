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

                foreach (LocalizationEntry entry in RegionQuestTexts.All.Concat(LocalizationManager.DefaultEntries.All))
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
