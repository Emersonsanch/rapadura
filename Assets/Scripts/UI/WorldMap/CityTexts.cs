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
    /// indicator and the carro de boi window. Residents who are also region quest-givers (Dona Cida, Mestre Tonho,
    /// Benedita, Seu Bento) reuse their <c>dialogue.*.name</c> key from <see cref="RegionQuestTexts"/>. Same
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

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                // ---- HUD / hub ----
                new LocalizationEntry(SafeZone, "You are in a safe zone", "Você está em zona segura"),
                new LocalizationEntry(SafeZoneIn, "You are in a safe zone · {0}", "Você está em zona segura · {0}"),
                new LocalizationEntry(NewStop, "New ox-cart stop: {0}", "Nova parada do carro de boi: {0}"),
                new LocalizationEntry(Respawned, "You woke up in {0}. Nobody fights in here.", "Vocês acordaram em {0}. Aqui ninguém briga."),
                new LocalizationEntry(ButtonNews, "{0} (!)", "{0} (!)"),
                new LocalizationEntry(Speech, "{0}: “{1}”", "{0}: “{1}”"),
                new LocalizationEntry(PeddlerToday, "Old Bento's mule is in town today!", "A mula do Seu Bento está na cidade hoje!"),

                // ---- Carro de boi ----
                new LocalizationEntry(FastTravel, "Ox cart", "Carro de boi"),
                new LocalizationEntry(FastTravelTitle, "Ox cart", "Carro de boi"),
                new LocalizationEntry(FastTravelSubtitle, "The ox cart takes you to the towns you have already been to. No beasts on the way.", "O carro de boi leva vocês às cidades onde já estiveram. Nenhum bicho no caminho."),
                new LocalizationEntry(FastTravelGold, "Gold in the bag: {0}", "Ouro na mochila: {0}"),
                new LocalizationEntry(FastTravelGo, "Travel ({0} gold)", "Viajar ({0} ouro)"),
                new LocalizationEntry(FastTravelRoads, "{0} road(s) away", "a {0} estrada(s)"),
                new LocalizationEntry(FastTravelHere, "You are here", "Vocês estão aqui"),
                new LocalizationEntry(FastTravelNotVisited, "Not visited yet: walk there once", "Ainda não visitada: vá a pé uma vez"),
                new LocalizationEntry(FastTravelLocked, "Region closed", "Região fechada"),
                new LocalizationEntry(FastTravelNoRoute, "No road there", "Sem estrada até lá"),
                new LocalizationEntry(FastTravelNoGold, "Not enough gold ({0})", "Falta ouro ({0})"),
                new LocalizationEntry(FastTravelNotInCity, "The cart only leaves from a town", "O carro só sai de uma cidade"),
                new LocalizationEntry(FastTravelDone, "You arrived in {0}! (-{1} gold)", "Chegaram em {0}! (-{1} ouro)"),
                new LocalizationEntry(FastTravelClose, "Close", "Fechar"),

                // ---- Shops ----
                Shop(CityShopCatalog.MerceariaLazaro, "Mr. Lázaro's Grocery", "Mercearia do Seu Lázaro"),
                Shop(CityShopCatalog.ArmarinhoCida, "Dona Cida's Notions Shop", "Armarinho da Dona Cida"),
                Shop(CityShopCatalog.FeiraLivre, "Open-air Market", "Feira livre"),
                Shop(CityShopCatalog.SelariaVava, "Vavá's Saddlery", "Selaria do Seu Vavá"),
                Shop(CityShopCatalog.ForjaTonho, "Master Tonho's Forge", "Forja do Mestre Tonho"),
                Shop(CityShopCatalog.LamparinasBenedita, "Benedita's Lamps", "Lamparinas da Benedita"),
                Shop(CityShopCatalog.MascateBento, "Old Bento the Peddler", "Mascate Seu Bento"),
                // Same text as MountTexts.HarasShopName (the mounts' own table registers it too).
                Shop(CityShopCatalog.HarasVila, "Haras da Vila (Seu Zé)", "Haras da Vila (Seu Zé)"),
            };

            // ---- Cities ----
            AddCity(entries, CityCatalog.VilaZefa, "Grandma Zefa's Village", "Vila da Vó Zefa", "Village", "Vila",
                "Talk to the villagers, shop at the market and the stores, use the workbench, Grandma's chest and her stove.",
                "Converse com os moradores, compre na feira e nas lojas, use a bancada, o baú e o fogão da Vó.");
            AddCity(entries, CityCatalog.AreiaBranca, "White Sand Hamlet", "Povoado de Areia Branca", "Areia Branca", "Areia Branca",
                "Whitewashed mud-brick houses on the white sand, around a juazeiro tree that never loses its leaves. Every backlands caravan stops here to drink from Dona Socorro's cistern.",
                "Casas de taipa caiadas na areia branca, em volta de um juazeiro que nunca perde as folhas. Toda caravana do sertão para aqui para beber da cisterna da Dona Socorro.");
            AddCity(entries, CityCatalog.SerraAlta, "High Ridge Village", "Vila da Serra Alta", "Serra Alta", "Serra Alta",
                "A stone village clinging to the top of the ridge: forge smoke, sheep's wool and an accordion at night. The wind bites, but the coffee is hot.",
                "Uma vila de pedra pendurada no alto da serra, com fumaça de forja, lã de ovelha e sanfona à noite. O vento corta, mas o café é quente.");
            AddCity(entries, CityCatalog.Lamparinas, "Village of Lamps", "Vila das Lamparinas", "Lamparinas", "Lamparinas",
                "At the mouth of the Caves every house keeps an oil lamp lit in the window. Since the light was stolen, nobody here lets the dark win.",
                "Na boca das Cavernas, cada casa deixa uma lamparina acesa na janela. Desde que a luz foi roubada, ninguém aqui deixa o escuro ganhar.");

            // ---- Residents: name (unless shared), then Joaquim, Maria, Maithe, Ícaro, Lavine, anyone ----
            AddResident(entries, "cida", null, null,
                ("That shield is bigger than you, boy. Let me reinforce the strap with good thread.", "Esse escudo é maior que você, menino. Deixa eu reforçar a alça com uma linha boa."),
                ("Your grandmother used to come here for hair ribbons too. Take a blue one, it suits the Temple.", "Sua avó também vinha aqui escolher fita de cabelo. Leve uma azul, combina com o Templo."),
                ("From your face, you tore your clothes in the scrub again. Sit, I'll patch it before Grandma sees.", "Pela sua cara, já rasgou a roupa no mato de novo. Senta que eu remendo antes da Vó ver."),
                ("Thread, needle, buttons... no, Ícaro, a button is not a machine part. Give it back.", "Linha, agulha, botão... não, Ícaro, botão não é peça de máquina. Devolve."),
                ("The youngest! I've kept a red scrap for you since the June festival.", "A caçula! Guardei um retalho vermelho para você desde o São João."),
                ("Thread, needle and a good prayer: a bit of everything here.", "Linha, agulha e reza boa: aqui tem de tudo um pouco."));

            AddResident(entries, "lazaro", "Mr. Lázaro, the Grocer", "Seu Lázaro, o Merceeiro",
                ("Roast meat for the one holding the front line. A hungry guardian protects nobody.", "Carne assada para quem segura a frente. Guardião de barriga vazia não protege ninguém."),
                ("Miss Maria! I set aside the freshest potions. Healing is your thing, but a backup never hurts.", "Dona Maria! Separei as poções mais fresquinhas. Cura é com você, mas um reforço não faz mal."),
                ("The eldest cousin always makes the list. How many potions this time? Five cousins, five potions?", "A mais velha dos primos sempre faz a lista. Quantas poções desta vez? Cinco primos, cinco poções?"),
                ("No credit, Ícaro. Not even for a fixed radio. ...Well, depends on the radio.", "Fiado, não, Ícaro. Nem em troca de rádio consertado. ...Bom, depende do rádio."),
                ("Here's a free rapadura, Lavine. Don't tell your cousins, or there'll be a queue.", "Toma uma rapadura de brinde, Lavine. Mas não conta para os primos, senão vira fila."),
                ("Open since before sunrise. Something new on the counter today.", "Mercearia aberta desde antes do sol nascer. Hoje tem coisa nova no balcão."));

            AddResident(entries, "nenem", "Comadre Neném, the Market Seller", "Comadre Neném, a Feirante",
                ("Hold this basket for me, Joaquim? You're as strong as your grandpa.", "Segura esse cesto para mim, Joaquim? Você é forte que nem seu avô."),
                ("Lemon balm for the prayer and rosemary for courage, girl. The market heals too.", "Erva-cidreira para a reza e alecrim para a coragem, menina. A feira também cura."),
                ("You hunt, so tell me: are the wild herbs growing well? A huntress's eye never misses.", "Você que caça, me diz: a erva do mato está nascendo boa? Olho de caçadora não erra."),
                ("No, the scale doesn't need a motor, Ícaro. It has worked for forty years.", "Não, a balança não precisa de motor, Ícaro. Ela funciona há quarenta anos."),
                ("Lavine, try this cajá. If you make a face, it's a good one.", "Lavine, prova esse cajá. Se fizer careta, é porque está bom."),
                ("Today's market, today's prices: tomorrow the stall is different.", "Feira de hoje, preço de hoje: amanhã a barraca é outra."));

            AddResident(entries, "ze_haras", "Seu Zé of the Haras", "Seu Zé do Haras",
                ("A guardian needs a mount that doesn't spook. The big dog stands firm by your shield, Joaquim.", "Guardião precisa de montaria que não se assusta. O cachorrão fica firme do lado do seu escudo, Joaquim."),
                ("Bless my pony, Maria? Your grandpa Chico rode with me for thirty years and never fell.", "Benze meu pônei, Maria? Seu avô Chico andou comigo trinta anos de estrada e nunca caiu."),
                ("The eldest picks first, Maithe. Look at the legs, not the coat: that's how Chico taught me.", "A mais velha escolhe primeiro, Maithe. Olha as pernas, não o pelo: foi assim que o Chico me ensinou."),
                ("No, Ícaro, a donkey doesn't need a motor. He has a temper, and that's enough.", "Não, Ícaro, jumento não precisa de motor. Ele tem é gênio, e isso já basta."),
                ("Lavine, the pony likes rapadura as much as you do. Give him half and he's yours.", "Lavine, o pônei gosta de rapadura que nem você. Dá a metade e ele é seu."),
                ("Pony, big dog or donkey: a good mount knows the road home on its own.", "Pônei, cachorrão ou jumento: montaria boa sabe sozinha o caminho de casa."));

            AddResident(entries, "bento", null, null,
                ("A guardian among my customers! I've got things that can take a beating, son.", "Um guardião na minha freguesia! Tenho coisa que aguenta pancada, meu filho."),
                ("Miss Maria, my mule and I crossed the whole backlands. I brought what I found on the road.", "Dona Maria, a mula e eu atravessamos o sertão inteiro. Trouxe o que achei pelo caminho."),
                ("Maithe again! You walk more than my mule. Tell me which road is safe.", "A Maithe de novo! Você anda mais que a minha mula. Me conta qual estrada está segura."),
                ("I don't sell ancient ruin parts, Ícaro. But ask tomorrow — tomorrow's another town.", "Peça de ruína antiga eu não vendo, Ícaro. Mas pergunta amanhã, que amanhã é outra cidade."),
                ("The little one! A proper peddler always has a sweet at the bottom of the chest.", "A caçulinha! Mascate que se preza sempre tem um doce no fundo do baú."),
                ("Here today, who knows where tomorrow. Buy now, a peddler doesn't wait.", "Hoje estou aqui, amanhã sabe Deus. Aproveita, que mascate não espera."));

            AddResident(entries, "vava", "Vavá, the Saddler", "Seu Vavá, o Seleiro",
                ("Cowboy leather withstands mandacaru thorns. It'll withstand your stubbornness too.", "Couro de vaqueiro aguenta espinho de mandacaru. Vai aguentar a sua teimosia também."),
                ("A priestess in the backlands needs good boots. Hot sand doesn't respect temples.", "Sacerdotisa no sertão precisa de bota boa. Areia quente não respeita templo."),
                ("A leather jerkin, girl: the scorpion strikes and can't get in. A smart huntress dresses for it.", "Gibão de couro, menina: o escorpião bate e não entra. Caçadora esperta anda vestida."),
                ("You can look at the stitching, Ícaro. Just don't take the saddle apart to see how it works.", "Pode olhar a costura, Ícaro. Só não desmonta a sela para ver como funciona."),
                ("I made a little leather hat just the youngest's size. The desert sun shows no mercy.", "Fiz um chapeuzinho de couro do tamanho da caçula. Sol do Deserto não perdoa."),
                ("Leather cured in the juazeiro's shade. Made to last a lifetime.", "Couro curtido na sombra do juazeiro. Coisa que dura uma vida."));

            AddResident(entries, "socorro", "Dona Socorro of the Cistern", "Dona Socorro da Cisterna",
                ("Drink slowly, boy. Water in the backlands is gold, and you're always first to share.", "Bebe devagar, menino. Água no sertão é ouro, e você é sempre o primeiro a dividir."),
                ("Since the Crystal went dark, the cistern sank. Will you pray for it too, Maria?", "Desde que o Cristal apagou, a cisterna baixou. Reza por ela também, Maria?"),
                ("You watch over the young ones like I watch this water. Rest a little, child.", "Você vigia os mais novos que nem eu vigio esta água. Descansa um pouco, filha."),
                ("A geared water pump? If it works, Ícaro, I'll pay you in rapadura.", "Uma bomba d'água de engrenagem? Se funcionar, Ícaro, eu te pago em rapadura."),
                ("Sit in the shade, Lavine. No beast comes inside the hamlet — you can rest.", "Senta na sombra, Lavine. Dentro do povoado nenhum bicho entra, pode descansar."),
                ("No fighting in here: whoever arrives thirsty just asks for water.", "Aqui dentro ninguém briga: quem chega com sede só pede água."));

            AddResident(entries, "tiao", "Tião of the Goats", "Tião das Cabras",
                ("You're the one with the shield? My goats fear nothing, but they respect you.", "Você é o do escudo? Minhas cabras não têm medo de nada, mas de você elas têm respeito."),
                ("My mom said the priestess would pass by. Can you bless Pretinha the goat? She's been coughing.", "Minha mãe disse que a sacerdotisa ia passar. Benze a cabra Pretinha? Ela anda tossindo."),
                ("Teach me to shoot a bow? I only know how to throw pebbles at goats that run off.", "Me ensina a atirar de arco? Eu só sei jogar pedrinha nas cabras que fogem."),
                ("Does your light meter find lost goats? If it does, you're the greatest inventor in the backlands.", "Seu medidor de luz acha cabra perdida? Se achar, você é o maior inventor do sertão."),
                ("Lavine! You're my age and you faced a giant scorpion? I've only faced a billy goat.", "Lavine! Você tem a minha idade e já enfrentou escorpião gigante? Eu só enfrentei bode."),
                ("Careful, Pretinha the goat eats whatever's in your pocket.", "Cuidado que a cabra Pretinha come o que estiver no seu bolso."));

            AddResident(entries, "tonho", null, null,
                ("Let me see that shield, Joaquim. Ancient roots, but the rivet's loose. I'll tighten it.", "Deixa eu ver esse escudo, Joaquim. Raiz antiga, mas o rebite está frouxo. Já aperto."),
                ("Forge fire is light too, girl. Until your Crystal returns, this is how we warm the mountain.", "Fogo de forja também é luz, menina. Enquanto o Cristal não volta, é assim que a gente esquenta a serra."),
                ("Iron arrowheads, for the huntress who hits what nobody sees.", "Ponta de flecha de ferro, para a caçadora que acerta o que ninguém vê."),
                ("You can strike the iron, Ícaro, but with the small hammer. The big one nearly took your finger.", "Pode bater no ferro, Ícaro, mas com o martelo pequeno. O grande quase levou seu dedo."),
                ("The forge gets hot, Lavine, but it won't burn whoever stays behind the stone line. Stay there.", "A forja esquenta, Lavine, mas não queima quem fica atrás da linha de pedra. Fica aí."),
                ("Mountain iron, beaten by the bellows. What leaves here doesn't break.", "Ferro da serra, batido no fole. O que sai daqui não quebra."));

            AddResident(entries, "graca", "Dona Graça, the Spinner", "Dona Graça, a Fiandeira",
                ("A wool scarf for the guardian's neck. Mountain cold knocks down even heroes.", "Um cachecol de lã para o pescoço do guardião. Frio da serra derruba até herói."),
                ("I spin wool watching the road. I saw you five arrive and counted: all of you. Thank goodness.", "Fio a lã olhando a estrada. Vi vocês cinco chegando e contei: estavam todos. Graças."),
                ("You climb like a young goat, Maithe. But come down slowly, loose stones are tricky.", "Você sobe a serra que nem cabrito, Maithe. Mas desce devagar, que pedra solta engana."),
                ("A wind-powered spinning wheel? Ícaro, if you build that, I'll spin twice as much.", "Uma roca movida a vento? Ícaro, se você fizer isso, eu fio o dobro."),
                ("Wool socks for the youngest, red ones, for luck in the dark.", "Meias de lã para a caçula, vermelhas, que é para dar sorte no escuro."),
                ("No beast climbs into the village: the wind and the forge fire scare them off.", "Aqui na vila bicho não sobe: o vento e o fogo da forja espantam."));

            AddResident(entries, "quinzinho", "Quinzinho and his Accordion", "Seu Quinzinho da Sanfona",
                ("I'll play a xote so the guardian can rest. Even a shield deserves a night off.", "Toco um xote para o guardião descansar. Até escudo merece uma noite de folga."),
                ("Sing with me, Maria? They say a priestess's voice scares off shadows.", "Canta comigo, Maria? Dizem que voz de sacerdotisa espanta sombra."),
                ("Does the eldest dance with the young ones or keep watch? Tonight, at least, dance.", "A mais velha dança com os mais novos ou fica de vigia? Hoje, pelo menos, dança."),
                ("No, Ícaro, an accordion isn't better with gears. ...Fine, show me the drawing.", "Não, Ícaro, sanfona não fica melhor com engrenagem. ...Mostra o desenho, vai."),
                ("I wrote this song for the girl who isn't afraid of the dark. I think it's you, Lavine.", "Fiz essa música para a menina que não tem medo do escuro. Acho que é você, Lavine."),
                ("Forge bellows by day, accordion bellows by night: the mountain is never quiet.", "Fole de forja de dia, fole de sanfona de noite: a serra nunca fica calada."));

            AddResident(entries, "benedita", null, null,
                ("Hang a lamp on your shield, Joaquim. Down there, whoever protects needs to see.", "Pendura uma lamparina no escudo, Joaquim. Lá embaixo, quem protege precisa enxergar."),
                ("The light you seek passed through here, Maria. Every lit lamp is a piece of it kept safe.", "A luz que você procura passou por aqui, Maria. Cada candeia acesa é um pedaço dela guardado."),
                ("Does the Eye of the Wind see in the dark? Take oil anyway. Good eyes get tired too.", "O Olho do Vento enxerga no escuro? Mesmo assim, leva óleo. Olho bom também cansa."),
                ("A self-lighting wick? Ícaro, if it works, the village will throw you a party.", "Um pavio que acende sozinho? Ícaro, se der certo, a Vila das Lamparinas faz festa para você."),
                ("You're the youngest and the one who shakes least in the dark. Benedita notices these things.", "Você é a mais nova e a que menos treme no escuro. A Benedita repara nessas coisas."),
                ("Oil, wick and potion: nobody goes down into the Caves without stopping here.", "Óleo, pavio e poção: ninguém desce às Cavernas sem passar aqui."));

            AddResident(entries, "candinho", "Old Candinho, the Retired Miner", "Seu Candinho, o Mineiro Velho",
                ("I worked forty years in these tunnels. I know where the ceiling holds a guardian, and where it doesn't.", "Trabalhei quarenta anos nessas galerias. Sei onde o teto aguenta um guardião, e onde não."),
                ("I heard the stone sing when the light passed, girl. It was a sad sound, not an angry one.", "Ouvi a pedra cantar quando a luz passou, menina. Era um som triste, não de raiva."),
                ("If you lose the way, Maithe, follow the water. Cave water always finds the way out.", "Se perder o caminho, Maithe, segue a água. Água de caverna sempre acha a saída."),
                ("Take my old tunnel map, Ícaro. Your meter and my map: nobody gets lost that way.", "Toma meu mapa velho das galerias, Ícaro. Seu medidor e meu mapa: aí ninguém se perde."),
                ("The bats don't enter the village, Lavine. They hit the lamplight and turn back.", "Os morcegos não entram na vila, Lavine. Batem na luz das candeias e voltam."),
                ("An old miner never forgets a tunnel. Ask, and I'll answer.", "Mineiro velho não esquece galeria. Pergunta, que eu respondo."));

            AddResident(entries, "fia", "Dona Fia, the Storyteller", "Dona Fia, a Contadora de Causos",
                ("Did I tell you about the cowboy who held the gate against the wind? Stubborn just like you.", "Já te contei do vaqueiro que segurou a porteira contra o vento? Teimoso que nem você."),
                ("Folks say the Dweller is afraid of the dark. Being afraid isn't evil, Maria. Just lonely.", "O povo conta que o Habitante tem medo do escuro. Quem tem medo não é mau, Maria. É só sozinho."),
                ("The eldest carries everyone's weight. Today, Maithe, I carry the story: sit.", "A mais velha carrega o peso de todos. Hoje, Maithe, quem carrega o causo sou eu: senta."),
                ("Your experiment notebook looks like a storybook, Ícaro. Just with more drawings.", "Seu caderno de experimentos parece livro de causo, Ícaro. Só que com mais desenho."),
                ("I'll tell you the story of the girl who lit the last lamp. She was your age.", "Vou te contar o causo da menina que acendeu a última lamparina. Ela tinha a sua idade."),
                ("A good story is told in the light. And here, there's plenty of light.", "Causo bom se conta na luz. E aqui, luz não falta."));

            AddResident(entries, "zezinho", "Zezinho the Wick Boy", "Zezinho do Pavio",
                ("Joaquim, can I hold the shield for a bit? Just a little bit!", "Joaquim, deixa eu segurar o escudo um pouquinho? Só um pouquinho!"),
                ("I trim and light the street lamps every day. It's my job to keep the light, just like yours.", "Eu corto o pavio e acendo as candeias da rua todo dia. Meu trabalho é guardar a luz, igual o seu."),
                ("Is Maithe your boss? She sounds like my big sister when she yells my name.", "A Maithe é a chefe de vocês? Ela parece minha irmã mais velha quando grita meu nome."),
                ("Ícaro, teach me to make sparks without flint? I promise not to burn my bangs again.", "Ícaro, me ensina a fazer faísca sem pederneira? Prometo não queimar a franja de novo."),
                ("Lavine, bet I trim wicks faster than you cast spells!", "Lavine, aposto que eu corto pavio mais rápido do que você solta feitiço!"),
                ("Wick trimmed, lamp lit: darkness doesn't rule here.", "Pavio cortado, candeia acesa: o escuro aqui não manda."));

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

        /// <summary>"Seu Vavá, o Seleiro: “...”".</summary>
        public static string ResidentSpeech(LocalizationManager manager, CityResidentSpec resident, CharacterId? hero)
        {
            return Format(manager, Speech, ResidentName(manager, resident), ResidentLine(manager, resident, hero));
        }

        public static string ShopName(LocalizationManager manager, string shopId) => Get(manager, CityShopCatalog.NameKeyOf(shopId));
    }
}
