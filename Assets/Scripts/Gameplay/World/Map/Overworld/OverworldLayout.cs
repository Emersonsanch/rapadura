using System.Collections.Generic;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The kind of ground of an overworld zone: ground colours, vegetation and scatter density (<see cref="OverworldTerrains"/>).</summary>
    public enum OverworldTerrain
    {
        /// <summary>Apricot and pomegranate rows around the Vila.</summary>
        Orchard = 0,
        /// <summary>Patchwork of melon, wheat and fallow plots along the aryk canals.</summary>
        Fields = 1,
        /// <summary>Dry steppe towards the Areias Flamejantes: straw grass, sand, tamarisks.</summary>
        Steppe = 2,
        /// <summary>Open meadows: green and gold grass, scattered poplars and flowers.</summary>
        Meadow = 3,
        /// <summary>The tugai: gold and green poplars and tamarisks along the river.</summary>
        Riverbank = 4,
        /// <summary>Wet floodplain: reeds, mud and tamarisks.</summary>
        Floodplain = 5,
        /// <summary>Rolling hills: earth mounds, boulders and cedars.</summary>
        Hills = 6,
        /// <summary>High stony ground: big boulders, few trees.</summary>
        HighHills = 7,
        /// <summary>Old ruins: paving, broken walls and standing stones.</summary>
        Ruins = 8,
        /// <summary>Closed wood: dense dark poplars and cedars.</summary>
        DenseWood = 9,
        /// <summary>Dark ravine of basalt with cave mouths.</summary>
        Ravine = 10,
        /// <summary>The Brejo: mud, ponds, reeds and dead trees.</summary>
        Marsh = 11,
        /// <summary>Silk mulberry rows and loom ruins.</summary>
        Mulberry = 12,
        /// <summary>The tall laurel forest of Garajonay.</summary>
        Canopy = 13,
        /// <summary>Mihrshah's walled garden: paving, cypresses, roses, dry canals.</summary>
        Garden = 14,

        // Região 2 — Areias Flamejantes (OverworldLayouts.Areias.cs, OverworldTerrains.Desert.cs): 20–39.
        /// <summary>The caravan road's dry steppe turning to sand: camel thorn, tamarisks, stone markers.</summary>
        CaravanSteppe = 20,
        /// <summary>The Oásis de Qocho: palms and green patches round the water.</summary>
        Oasis = 21,
        /// <summary>Date-palm rows watered by the karez channels.</summary>
        PalmGrove = 22,
        /// <summary>Stony desert (hamada) of the Artífices: gravel, lotus columns, ushabtis.</summary>
        StoneDesert = 23,
        /// <summary>Low rolling dunes.</summary>
        LowDunes = 24,
        /// <summary>The erg: high crested dunes.</summary>
        Erg = 25,
        /// <summary>Red sandstone canyons: chimneys and natural arches.</summary>
        RedCanyon = 26,
        /// <summary>Salt flats: white crust, brine pools.</summary>
        SaltFlat = 27,
        /// <summary>The Meroë pyramid fields: steep pyramids, chapels, tells.</summary>
        MeroeRuins = 28,
        /// <summary>Flat-topped sandstone mesas over a sand floor.</summary>
        Mesas = 29,
        /// <summary>Lines of karez shafts across the clay plain.</summary>
        KarezField = 30,
        /// <summary>A tamarisk thicket round an oasis lake.</summary>
        TamariskOasis = 31,
        /// <summary>Dark sand strewn with bones and broken caravans.</summary>
        BoneValley = 32,
        /// <summary>The Galerias do Karez (09): sunken clay ground over the galleries, shafts and wells.</summary>
        KarezGallery = 33,
        /// <summary>The Dunas que Cantam (10): the tallest dunes, the crescent lake.</summary>
        SingingDunes = 34,
        /// <summary>The Ermos do Tambor Rubro (11): layered red badlands, canyons and chimneys.</summary>
        Badlands = 35,
        /// <summary>The Trono de Namrael (12): terraces of salt and red crystal, rammed-earth towers.</summary>
        SaltThrone = 36,

        // Região 4 — Grutas do Limiar (OverworldLayouts.Limiar.cs, OverworldTerrains.Caves.cs): 40–59.
        /// <summary>The mouth of the caves towards the Hahya: juniper, tuff boulders, the first glowing moss.</summary>
        CaveMouth = 40,
        /// <summary>Black-sand shores of the underground lakes: basalt, moss, glowing mushrooms by the water.</summary>
        LakeShore = 41,
        /// <summary>A hall of stalactites and stalagmites.</summary>
        StalactiteHall = 42,
        /// <summary>A grove of giant glowing mushrooms.</summary>
        MushroomGrove = 43,
        /// <summary>A field of blue crystals on indigo rock.</summary>
        CrystalField = 44,
        /// <summary>Chasms and fissures: high basalt cut by gorges, rope bridges over the abysses.</summary>
        Chasm = 45,
        /// <summary>Stepped pink-tuff terraces.</summary>
        TuffTerraces = 46,
        /// <summary>A forest of natural stone columns.</summary>
        ColumnForest = 47,
        /// <summary>Moss meadows with springs and the vishapakar (21).</summary>
        MossMeadow = 48,
        /// <summary>The kobolds' tunnels: rails, carts, air pipes (22).</summary>
        TunnelMarket = 49,
        /// <summary>The underground canyon of the wheel of twelve jars: tuff, bronze, aqueducts (23).</summary>
        WheelCanyon = 50,
        /// <summary>Mahaway's roost: a giant shaft, nests on the ledges, rope bridges (24).</summary>
        Roost = 51,

        // Região 3 — Serra de Kaggen (OverworldLayouts.Kaggen.cs, OverworldTerrains.Mountain.cs): 60–79.
        /// <summary>Golden grass slopes round the Vila de Litema: aloes, proteas, the odd baobab, pastors' rondavels.</summary>
        GoldenSlopes = 60,
        /// <summary>Farming terraces: dry-stone retaining walls with sorghum rows on each step.</summary>
        FarmTerraces = 61,
        /// <summary>Windy aloe slopes: tree aloes, protea, granite boulders, cairns.</summary>
        AloeSlopes = 62,
        /// <summary>Baobab grassland on the lower plateau.</summary>
        BaobabGrass = 63,
        /// <summary>The high pass of Hahya: granite, snow in the hollows, twisted cedars.</summary>
        HighPass = 64,
        /// <summary>Meltwater torrents in green gullies: cedars, ferns, mossy boulders.</summary>
        Torrents = 65,
        /// <summary>Herders' pastures with round dry-stone kraals (currais) and rondavels.</summary>
        Kraals = 66,
        /// <summary>Narrow passes between rock walls (paredões).</summary>
        NarrowPass = 67,
        /// <summary>Stepped red sandstone escarpments.</summary>
        SandstoneScarp = 68,
        /// <summary>The Abrigos Pintados (15): sandstone overhangs with rock paintings.</summary>
        PaintedShelters = 69,
        /// <summary>The Muralhas de Pedra Seca (16): granite hills, curved dry-stone walls, conical towers, lichen.</summary>
        DryStone = 70,
        /// <summary>The Salões de Bronze de Napirasu (17): dark granite, bronze rune pillars, cold lava.</summary>
        BronzeHalls = 71,
        /// <summary>Snowfields under the summit: snow, granite, sparse cedars, mist.</summary>
        Snowfield = 72,
        /// <summary>The Cume dos Mazans (18): the snowy peak in the mist, giant granite blocks.</summary>
        Summit = 73,
        /// <summary>The Estela Alta plateau (18): snow-dusted steps, the shepherds' shelter.</summary>
        StelaPlateau = 74,

        // Região 5 — Costa das Monções (OverworldLayouts.Costa.cs, OverworldTerrains.Coast.cs): 80–99.
        /// <summary>White coral-sand beaches: leaning coconut palms, beached dhows, nets and fish traps.</summary>
        WhiteBeach = 80,
        /// <summary>The reef flat round Taa Pwani (25): turquoise shallows over coral, coral heads, tide pools.</summary>
        Reef = 81,
        /// <summary>Mangroves: grey mud, stilt-rooted mangroves along the channels (30).</summary>
        Mangrove = 82,
        /// <summary>Coastal dunes held by casuarinas and sea-grape shrubs.</summary>
        CoastalDunes = 83,
        /// <summary>Coral-stone ruins of the coast (Gedi, Kilwa): whitewashed walls, arches, pillar tombs.</summary>
        CoralRuins = 84,
        /// <summary>Clove plantations: rows of small evergreen trees, cloves drying on mats.</summary>
        CloveRows = 85,
        /// <summary>A coconut plantation: palms in rows over grass.</summary>
        CoconutGrove = 86,
        /// <summary>Laterite and coral sea cliffs in steps, sea stacks.</summary>
        SeaCliffs = 87,
        /// <summary>Islets of sand and coral linked by shallows (baixios), deep channels between them.</summary>
        Shoals = 88,
        /// <summary>The Savana dos Auroques (26): golden grass, flat-topped acacias, baobabs, termite mounds, pools.</summary>
        Savanna = 89,
        /// <summary>The Pirâmides Altas de Meroé (27): orange sand, steep pyramids, chapels, ushabtis.</summary>
        KushPyramids = 90,
        /// <summary>Kirta's atoll (28): a ring of coral sand round the lagoon.</summary>
        Atoll = 91,
        /// <summary>Kilwa Afundada (29): coral streets half drowned, flooded houses, arches.</summary>
        SunkenCity = 92,
        /// <summary>The restinga: coastal thicket of shrubs, palms and baobabs on low hills.</summary>
        CoastScrub = 93,
        /// <summary>Tidal lagoons: mud flats, pools, mangrove fringes.</summary>
        TideLagoons = 94,
        /// <summary>Baobab woods on red laterite.</summary>
        BaobabWoods = 95,
        /// <summary>Kelp flats north of Kirta (28): green shallows, kelp drifts, coral heads.</summary>
        KelpFlats = 96,

        // Região 6 — Delta de Jhilpur (OverworldLayouts.Delta.cs, OverworldTerrains.Delta.cs): 100–119.
        /// <summary>Jhilpur's lake (31): murky green water's edge, lotus, banyans, brick ghats.</summary>
        RootLake = 100,
        /// <summary>Giant banyans whose aerial roots drop curtains to the ground; shade, moss, ferns.</summary>
        BanyanGrove = 101,
        /// <summary>Lotus lagoons: shallow murky ponds full of pads and pink flowers, reeds on the banks.</summary>
        LotusMarsh = 102,
        /// <summary>The Pontes de Raiz Viva (32): rain-forest gorges with rivers at the bottom, living-root bridges and arches.</summary>
        RootGorge = 103,
        /// <summary>Rain forest on rolling ground: dark broadleaves, palms, ferns, mossy boulders.</summary>
        RainForest = 104,
        /// <summary>Flooded rice paddies between earthen dikes (the delta's farmland).</summary>
        PaddyLevees = 105,
        /// <summary>The delta's grey mud banks between the channels, low and bare, puddles and crab holes.</summary>
        MudFlats = 106,
        /// <summary>Open swamp of reed islands (33, Tarchon): reeds, stilt huts of the fishers, lilac mist.</summary>
        ReedIslands = 107,
        /// <summary>The Charco do Ninki Nanka (34): tall reeds and raffia palms, hollow logs, dark water.</summary>
        RaffiaBog = 108,
        /// <summary>The Coração do Lodo (35): black mud, rotten roots, violet bubbles, the odd spring of Living Water.</summary>
        BlackMire = 109,
        /// <summary>A drowned dead wood: grey snags, rotten roots, bracket fungi.</summary>
        DeadWood = 110,
        /// <summary>The tidal forest of the delta's mouths: stilt-rooted trees along every creek, palms.</summary>
        TidalForest = 111,

        // Região 7 — Teto do Mundo (OverworldLayouts.Teto.cs, OverworldTerrains.Pamir.cs): 120–139.
        /// <summary>Sangkent's valley (36): stone-walled terraces, apricots in flower, poplars along the irrigation channels.</summary>
        StairTerraces = 120,
        /// <summary>A high valley of apricot orchards in blossom on low terraces, channels of meltwater between them.</summary>
        ApricotValley = 121,
        /// <summary>Poplar rows along the irrigation channels, green meadow, stone field walls.</summary>
        PoplarCanals = 122,
        /// <summary>The high steppe of the Pamir: thin feather grass, dwarf shrubs, cushions of stone, wide and cold.</summary>
        HighSteppe = 123,
        /// <summary>A braided meltwater river on grey gravel bars, willows on the banks.</summary>
        MeltRiver = 124,
        /// <summary>Slopes of old twisted junipers over rock and scree.</summary>
        JuniperSlope = 125,
        /// <summary>Summer pastures of the yaks: short turf, stone pens, the herders' stone huts.</summary>
        YakPasture = 126,
        /// <summary>The shore of a turquoise glacial lake: pale gravel, cushion plants, cairns.</summary>
        TurquoiseLake = 127,
        /// <summary>The Calhas de Pedra (37): steep slopes cut by polished stone chutes on pillars, timber stations.</summary>
        StoneChutes = 128,
        /// <summary>The snowy upper terraces above Sangkent (36): abandoned terraces under snow, stone huts, cairns.</summary>
        SnowTerraces = 129,
        /// <summary>Moraines: long ridges of grey gravel and boulders left by the glacier.</summary>
        Moraine = 130,
        /// <summary>The Geleira do Teto do Mundo (38): a tongue of white ice with blue crevasses.</summary>
        Glacier = 131,
        /// <summary>A field of seracs: towers and blocks of blue ice, snow bridges between them.</summary>
        Seracs = 132,
        /// <summary>Windswept ridges: bare rock and snow cornices, the wooden wind wheels of the fort.</summary>
        WindRidge = 133,
        /// <summary>The Forte dos Ventos (39): a crag of grey rock, watch towers of wood and stone, wind bells.</summary>
        FortRock = 134,
        /// <summary>The upper Wakhan valley: the caravan road along the meltwater river, gravel flats, the caravans' ribbons.</summary>
        WakhanValley = 135,
        /// <summary>The Passo de Wakhan (40): the high pass under snow, cairns and ribbon lines along the road.</summary>
        WakhanPass = 136,
        /// <summary>The dragon's crags (40): black rock, old snow, great shed scales of Aydahar stuck in the ground.</summary>
        DragonCrags = 137,
        /// <summary>The ice peaks at the roof of the world: snow, ice and rock, almost nothing grows.</summary>
        IcePeaks = 138,

        // Região 8 — Planície de Mingcheng (OverworldLayouts.Mingcheng.cs, OverworldTerrains.Mingcheng.cs): 140–159.
        /// <summary>The outer rings of Mingcheng (41): villages of grey brick, willows, lantern posts, little canals and plots.</summary>
        NineRings = 140,
        /// <summary>Golden millet on the yellow earth of the outer rings: low field banks, haystacks, a shrine here and there.</summary>
        MilletFields = 141,
        /// <summary>The upper Rio Dourado on the plain: weeping willows on grassy banks, sandbars, reeds.</summary>
        WillowRiver = 142,
        /// <summary>The Canal das Lanternas (43): stone banks, willows, red lantern posts, stele-bearing stone turtles.</summary>
        LanternCanal = 143,
        /// <summary>The Arrozais Espelhados (44): flooded rice terraces stepping up the slope, mirror water, a water mill, scarecrows.</summary>
        MirrorPaddies = 144,
        /// <summary>Tea hills: rounded slopes combed by rows of tea bushes, pavilions on the tops.</summary>
        TeaTerraces = 145,
        /// <summary>A bamboo grove: tall green culms in clumps, deep shade, fallen leaves.</summary>
        BambooGrove = 146,
        /// <summary>The white hills east of Mingcheng (42): lime-white slopes in garden terraces, cypresses, bronze mirrors.</summary>
        WhiteHills = 147,
        /// <summary>The garden terraces of the Torres Brancas (42): green lawns, white spires and small Meroë pyramids, mirror posts.</summary>
        SpireGardens = 148,
        /// <summary>Karst: limestone towers rising sheer from a flat green floor, pines clinging to them.</summary>
        KarstPeaks = 149,
        /// <summary>Misty ridges of old flat-crowned mountain pines over grey granite.</summary>
        PineRidge = 150,
        /// <summary>The Muralha das Mil Torres (45): the wall riding the crest, its beacon towers, pines and mist below.</summary>
        GreatWall = 151,
        /// <summary>The terracotta pits (41, 45): flat loess with long pits of clay soldiers, burial mounds.</summary>
        TerracottaPits = 152,
        /// <summary>The spirit way into Mingcheng (41): an avenue of stone lions and stele turtles between pines and lawns.</summary>
        SpiritWay = 153,
        /// <summary>Loess gullies: yellow earth cut by deep ravines, earth pillars, terraced field strips.</summary>
        LoessGullies = 154,

        // Região 9 — Estepe dos Nartes (OverworldLayouts.Nartes.cs, OverworldTerrains.Nartes.cs): 160–179.
        /// <summary>The open steppe of tall feather grass (stipa): straw-gold waves, the herders' horse standards, a stone baba.</summary>
        FeatherSteppe = 160,
        /// <summary>The steppe's shallow rivers: wide shingle bars, willows, reeds and the fords (the Vau Largo).</summary>
        SteppeRiver = 161,
        /// <summary>The Acampamento de Soslan (47): trampled grass round the centaurs' felt tents on carts, horse standards, wells.</summary>
        HorseCamp = 162,
        /// <summary>The Kurgans de Ouro (48): burial mounds of earth and stone on the steppe, stone babas, golden stags.</summary>
        KurganField = 163,
        /// <summary>The silver wormwood steppe: grey-green tussocks, salt pans, bones of the herds.</summary>
        WormwoodSteppe = 164,
        /// <summary>The forest-steppe: birch groves in the grass, white trunks, golden leaves.</summary>
        BirchSteppe = 165,
        /// <summary>The green hills of the hill giants (49): earthen ramparts in rings round the tops, giant stone pots.</summary>
        GiantHills = 166,
        /// <summary>The vale of the dream tablets (49): standing slabs carved with the giants' dreams in a mossy valley.</summary>
        DreamVale = 167,
        /// <summary>The taiga at the steppe's edge (50): giant larches and spruces, moss, meltwater.</summary>
        Taiga = 168,
        /// <summary>Rolling prairie red with poppies (51).</summary>
        PoppyPrairie = 169,
        /// <summary>The Pradaria dos Mil Cavalos (51): wide short-grass plains of the wild herds, stone wells, cairns.</summary>
        HorsePlains = 170,
        /// <summary>A high valley of the Caucasus: svan stone towers, birch woods, stone field walls (52).</summary>
        SvanValley = 171,
        /// <summary>The Corrente de Amirani (52): grey crags of the Caucasus, old snow, the giant chain climbing the mountain.</summary>
        ChainPeak = 172,
        /// <summary>The snow crest under the summit where Ohya waits (52): snow, ice and black rock.</summary>
        SnowCrest = 173,

        // Região 10 — Picos de Farnkesh (OverworldLayouts.Farnkesh.cs, OverworldTerrains.Farnkesh.cs): 180–199.
        /// <summary>The Vale dos Sátiros' vineyards (53): vines on stakes up the sunny slopes, wicker, grapes, light green grass.</summary>
        VineVale = 180,
        /// <summary>The Pactolo (53): a river of golden pebbles, plane trees and reeds on its banks.</summary>
        GoldenRiver = 181,
        /// <summary>The satyrs' glades (53): clearings in a ring joined by leafy tunnels, stone stages, gourd-shaped wicker huts.</summary>
        SatyrGlades = 182,
        /// <summary>The woods at the foot of the peaks: chestnuts and oaks on the first slopes, ferns, mossy boulders.</summary>
        FootWoods = 183,
        /// <summary>High flowered meadows under the pinnacles, wind bells on poles, paper streamers.</summary>
        AlpineMeadow = 184,
        /// <summary>A forest of sandstone pinnacles (Meteora's form): towers of rock over pines, hermitages on the tops.</summary>
        PinnacleWood = 185,
        /// <summary>Farnkesh, the Stone Nests (54): white houses on the nine sandstone pinnacles, rope baskets, the giant wind bell.</summary>
        StoneNests = 186,
        /// <summary>The sea of clouds between the peaks: pale ground under drifting cloud banks, the odd pinnacle standing out.</summary>
        CloudSea = 187,
        /// <summary>The Pontes de Vento (55): rope bridges and plank platforms between small pinnacles, cloud below.</summary>
        WindBridges = 188,
        /// <summary>The Ermidas Suspensas (56): a cliff face with houses hung on it, ladders, cracked bells.</summary>
        BellCliffs = 189,
        /// <summary>The Garganta dos Ays (57): a narrow grey gorge where the wind howls, dust, twisted trees, shelter stones.</summary>
        AysGorge = 190,
        /// <summary>Wind-scoured scree slopes above the gorge: grey shingle, twisted junipers, cairns.</summary>
        WindScree = 191,
        /// <summary>The loose stone islands in the clouds round Huma's nest (58): sheer-sided isles, gold-white grass.</summary>
        CloudIsles = 192,
        /// <summary>Huma's Nest (58): the giant nest of golden branches on the highest isle, white rock and gold.</summary>
        HumaNest = 193,

        // Região 11 — Mar Gelado (OverworldLayouts.MarGelado.cs, OverworldTerrains.MarGelado.cs): 200–219.
        /// <summary>The snowy shore of the bay south of the harbour: snow over shingle, whale bones, upturned crescent boats.</summary>
        SnowShore = 200,
        /// <summary>The Porto da Lua Branca's ice islands (59): packed snow, snow domes, long plank houses with painted moons.</summary>
        MoonHarbor = 201,
        /// <summary>The frozen bay: flat sea ice in wide plates, fishing holes, boats frozen in.</summary>
        FrozenBay = 202,
        /// <summary>The Canais de Gelo (60): blocks of blue ice between black-water canals, thin ice.</summary>
        IceCanals = 203,
        /// <summary>Cracked ice (60): pressure ridges of crushed ice, dark cracks, ice blocks thrown up.</summary>
        CrackedIce = 204,
        /// <summary>Omöl's tundra (61): flat moss and lichen, round stones, snow patches.</summary>
        Tundra = 205,
        /// <summary>Omöl's lakes (61): low hummocks between round blue lakes, sedge and lichen.</summary>
        TundraLakes = 206,
        /// <summary>The reindeer pastures round Yen's tent: rolling lichen, herders' tents, drying racks.</summary>
        ReindeerPasture = 207,
        /// <summary>A fjord: high dark walls over a frozen channel, snow ledges.</summary>
        Fjord = 208,
        /// <summary>The Galerias dos Mamutes (62): vaults of ice and frozen earth, giant tusks in the walls, crystals.</summary>
        MammothGalleries = 209,
        /// <summary>Frozen earth at the foot of the peaks: brown permafrost mounds, tusks coming out of the ground.</summary>
        FrozenEarth = 210,
        /// <summary>The pack ice of the Mar das Auroras (63): floating plates with dark leads between them.</summary>
        PackIce = 211,
        /// <summary>The Mar das Auroras (63): icebergs standing out of a frozen sea under the green-lilac light.</summary>
        AuroraSea = 212,
        /// <summary>The isle of the ice lighthouse (63): a rocky snow isle in the pack ice, the beacon of ice.</summary>
        LighthouseIsle = 213,
        /// <summary>Old Kul's frozen lake (64): a huge plain of black-silver ice, fishing holes, the fishers' racks.</summary>
        FrozenLake = 214,
        /// <summary>Black ice (64): clear dark ice over the deep, cracks and frozen bubbles, low ridges.</summary>
        BlackIce = 215,

        // Região 12 — Arquipélago de Hinokura (OverworldLayouts.Hinokura.cs, OverworldTerrains.Hinokura.cs): 220–239.
        /// <summary>Hinokura, the Forge Isle (65): black volcanic stone and pumice round the lacquer town, stone lanterns, warm rocks.</summary>
        ForgeIsle = 220,
        /// <summary>A beach of black sand (65): low dunes of black sand, pumice washed up, steam where the surf meets warm rock.</summary>
        BlackSandBeach = 221,
        /// <summary>The thaw strait towards the Mar Gelado (65): melting ice floes on warm water, steam and sea smoke.</summary>
        ThawStrait = 222,
        /// <summary>The lava channels (65): tamed lava running in stone channels to the forges, black stone, bridges.</summary>
        LavaChannels = 223,
        /// <summary>Hot springs (65): steaming pools among sinter mounds, yellow crust, pines.</summary>
        HotSprings = 224,
        /// <summary>The fumarole coast (65): low basalt cliffs, fumaroles with yellow sulphur rims, steam.</summary>
        FumaroleCoast = 225,
        /// <summary>The Jardins de Lava Laqueada (66): raked gravel, moss, pruned pines, stone lanterns, stone lion-dogs.</summary>
        LavaGardens = 226,
        /// <summary>The cloud pines (66): moss and black rock under pines pruned in cloud pads, bamboo.</summary>
        CloudPines = 227,
        /// <summary>The red bridges (66): islets in warm ponds, red lacquer gates, lanterns.</summary>
        RedBridges = 228,
        /// <summary>The broom fields of Mount Echeyde (67): pumice slopes with yellow broom in flower, stone roses, canary pines.</summary>
        BroomFields = 229,
        /// <summary>Mount Echeyde (67): the volcano's cone in three rings up to the crater, pumice and lava.</summary>
        EcheydeCone = 230,
        /// <summary>The malpaís (67): fields of rough black lava in flows and lobes, glowing cracks, fumaroles.</summary>
        Malpais = 231,
        /// <summary>The hot channels (68): steaming blue water between black glass, ember coral.</summary>
        HotChannels = 232,
        /// <summary>The Recife de Obsidiana (68): a reef of black shining glass in sharp ridges, steam, ember coral.</summary>
        ObsidianReef = 233,
        /// <summary>The glass islets (68): low islets of obsidian in the warm sea, ember coral, steam.</summary>
        GlassIslets = 234,
        /// <summary>The curved walls (69): long curved ramparts of coral stone round the castle, red gates.</summary>
        CurvedWalls = 235,
        /// <summary>The Castelo das Nove Fornalhas (69): courts of black stone, furnace towers, slag, lacquer gates.</summary>
        NineFurnaces = 236,
        /// <summary>Slag fields (69): heaps of slag and cinder from the furnaces, iron-red ground, chimneys.</summary>
        SlagFields = 237,
        /// <summary>The ash plains (70): grey ash on cold stone, burnt stumps, cinder heaps.</summary>
        AshPlain = 238,
        /// <summary>The Caldeira do Fogo Devorador (70): a ring of cold stone round an open lake of lava.</summary>
        Caldera = 239,
    }

    /// <summary>
    /// A group of monsters a spawn clearing of an overworld zone can host (Docs/Mapas100.md §0.5 archetypes): day, dusk and
    /// night tables over the existing enemy definitions, and the night watchers that only look at weak heroes.
    /// </summary>
    public sealed class OverworldFauna
    {
        public string Id;
        public string Archetypes;
        public int MaxAlive = 3;
        public float RespawnSeconds = 25f;
        public PeriodSpawnEntry[] Day = new PeriodSpawnEntry[0];
        public PeriodSpawnEntry[] Dusk;
        public PeriodSpawnEntry[] Night = new PeriodSpawnEntry[0];
        public string PassiveSpeciesId;
        public int PassiveBelowHeroLevel;
    }

    /// <summary>
    /// One area of a continuous region (Docs/Mapas100.md §0.10): a rectangle of the overworld with its level band, the level
    /// (and special gate) to set foot in it, its terrain and its monster groups. A catalog map (01–06) is one or more zones
    /// (<see cref="MapId"/>); the in-between areas (meadows, hills, closed wood…) have no catalog id and count for the map
    /// in <see cref="OwnerMapId"/> (return city, fast travel "you are here", gates).
    /// </summary>
    public sealed class OverworldZone
    {
        public string Id;

        /// <summary>The catalog map this zone is (part of), or null for an in-between area.</summary>
        public string MapId;

        /// <summary>The catalog map an in-between area counts for (equal to <see cref="MapId"/> when that is set).</summary>
        public string OwnerMapId;

        public Rect Bounds;
        public OverworldTerrain Terrain;
        public int MinLevel = 1;
        public int MaxLevel = 1;

        /// <summary>Lowest level that may walk in (hero terrain bonus applies, <see cref="MapAccessRules.EffectiveLevelRequirement"/>).</summary>
        public int WalkLevel = 1;

        public string RegionType = MapRegionTypes.Forest;

        /// <summary>Special gate (<see cref="MapRequirementTags"/>), empty = none.</summary>
        public string RequirementTag = string.Empty;

        public string NameEn;
        public string NamePt;

        /// <summary>Grid spacing of the generated spawn clearings (m): smaller = more monster groups.</summary>
        public float ClearingSpacing = 100f;

        public readonly List<OverworldFauna> Fauna = new List<OverworldFauna>();

        /// <summary>The catalog map the hero is "on" while in this zone.</summary>
        public string MapIdOrOwner => !string.IsNullOrEmpty(MapId) ? MapId : OwnerMapId;

        public string NameKey => "overworld.zone." + Id + ".name";

        /// <summary>
        /// Where the zone's region sits in the stitched world (<see cref="OverworldPart.Offset"/>; zero in a region on its own):
        /// the relief shapes the zone in its region's own coordinates, so a region looks the same alone and stitched.
        /// </summary>
        public Vector2 Origin;

        public override string ToString() => Id;
    }

    /// <summary>
    /// A safe settlement placed in the overworld (the Vila, the outposts of the catalog maps): its town-kit recipe, the
    /// generated town (local coordinates, plaza at its origin) and where it sits (<see cref="Offset"/>), its safe zone and
    /// its Cais da Barca in world coordinates.
    /// </summary>
    public sealed class OverworldSettlement
    {
        public string Id;
        public string MapId;
        public Vector2 Offset;
        public CityTownRecipe Recipe;
        public CityLayout Town;

        /// <summary>Safe zone in world coordinates.</summary>
        public Rect SafeZone;

        public float GroundHalfExtent = 18f;

        /// <summary>A defeat in the region wakes the hero here (the Vila).</summary>
        public bool IsReturnCity;

        public bool HasDock;
        public Rect DockTrigger;
        public Vector2 DockArrival;
        public Vector2 DockArrivalFacing = Vector2.down;

        /// <summary>The square of town ground (world coordinates) the overworld ground leaves to the town kit.</summary>
        public Rect GroundRect => new Rect(Offset.x - GroundHalfExtent, Offset.y - GroundHalfExtent, GroundHalfExtent * 2f, GroundHalfExtent * 2f);
    }

    /// <summary>
    /// One region placed in the stitched world (<see cref="OverworldLayouts.WorldId"/>, Docs/Mapas100.md §0.10 "Mundo
    /// costurado"): its rectangle in world coordinates, how far its own layout was moved (<see cref="Offset"/>), its return
    /// city, its biome (weather, ambience) and the region the rest of the game reads (music, island token, respawn).
    /// </summary>
    public sealed class OverworldPart
    {
        /// <summary>The region's own layout id (<see cref="OverworldLayouts.ZarafshanId"/>…).</summary>
        public string Id;

        public MapRegion Region;
        public BiomeType Biome;

        /// <summary>The region's rectangle in world coordinates.</summary>
        public Rect Bounds;

        /// <summary>Region coordinates + <see cref="Offset"/> = world coordinates.</summary>
        public Vector2 Offset;

        /// <summary>The region's town (a defeat with no return city of its own wakes the hero here).</summary>
        public OverworldSettlement Vila;

        public override string ToString() => Id;
    }

    /// <summary>
    /// A whole region as ONE continuous map (Docs/Mapas100.md §0.10): no scene change between the Vila and the areas around
    /// it. <see cref="World"/> is a <see cref="MapLayout"/> in world coordinates holding what the shared map code reads
    /// (bounds, rivers, trails, landmarks, every spawn clearing, gates and docks, the Vila as the return city), so the
    /// spawners, the river rules and the respawn work unchanged; the zones, settlements and the chunk grid are the
    /// overworld's own. Pure data, deterministic: the online server can rebuild the same world.
    /// </summary>
    public sealed class OverworldLayout
    {
        public string Id;
        public MapRegion Region;
        public Rect Bounds;

        /// <summary>Side of one streamed chunk (m).</summary>
        public float ChunkSize = 48f;

        public MapLayout World;
        public readonly List<OverworldZone> Zones = new List<OverworldZone>();
        public readonly List<OverworldSettlement> Settlements = new List<OverworldSettlement>();

        /// <summary>Spawn clearing id → the zone it belongs to.</summary>
        public readonly Dictionary<string, OverworldZone> ZoneOfSpawn = new Dictionary<string, OverworldZone>();

        /// <summary>The region's town (respawn, start).</summary>
        public OverworldSettlement Vila;

        /// <summary>The bounds of the embedded hand-made maps (world): no generated clearings or noise scatter inside them.</summary>
        public readonly List<Rect> AuthoredAreas = new List<Rect>();

        /// <summary>The wild props of the embedded hand-made maps (world coordinates), streamed with their chunk.</summary>
        public readonly List<CityProp> FixedProps = new List<CityProp>();

        /// <summary>The regions of the stitched world (empty for a region on its own: it is its only part).</summary>
        public readonly List<OverworldPart> Parts = new List<OverworldPart>();

        /// <summary>
        /// Ground kept at y = 0 besides the towns and gates (world coordinates, with the flat core around each): where the
        /// gates between two stitched regions stood, now open passes (<see cref="OverworldRelief.GateRamp"/> around them).
        /// </summary>
        public readonly List<(Rect Rect, float Inner)> OpenFlats = new List<(Rect, float)>();

        /// <summary>The open passes of the stitched world (where a gate between two regions stood): the cave vault opens there.</summary>
        public readonly List<Vector2> Passes = new List<Vector2>();

        /// <summary>The relief of the region, built on first use (<see cref="OverworldRelief.For"/>).</summary>
        internal OverworldRelief ReliefCache;

        private Dictionary<Vector2Int, List<(Vector2 Center, float Length, float Yaw, float Width)>> _riverPieces;
        private Dictionary<Vector2Int, List<CityProp>> _fixedProps;

        public bool InAuthoredArea(Vector2 p)
        {
            foreach (Rect r in AuthoredAreas)
            {
                if (r.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The embedded props standing in <paramref name="chunk"/>.</summary>
        public IReadOnlyList<CityProp> FixedPropsIn(Vector2Int chunk)
        {
            if (_fixedProps == null)
            {
                _fixedProps = new Dictionary<Vector2Int, List<CityProp>>();
                foreach (CityProp prop in FixedProps)
                {
                    Vector2Int c = OverworldRules.ChunkOf(this, prop.Position);
                    if (!_fixedProps.TryGetValue(c, out List<CityProp> list))
                    {
                        list = new List<CityProp>();
                        _fixedProps[c] = list;
                    }

                    list.Add(prop);
                }
            }

            return _fixedProps.TryGetValue(chunk, out List<CityProp> props) ? props : (IReadOnlyList<CityProp>)System.Array.Empty<CityProp>();
        }

        public OverworldZone Zone(string id) => Zones.Find(z => z.Id == id);

        /// <summary>True for the stitched world (several regions); false for a region on its own.</summary>
        public bool IsStitched => Parts.Count > 0;

        private List<Rect> _holes;

        /// <summary>
        /// The rectangles of <see cref="Bounds"/> no region covers (the stitched world need not be a rectangle: Região 5 makes
        /// it an L): walled off like the world's edge, the backdrop rises there. Empty for a region on its own.
        /// </summary>
        public IReadOnlyList<Rect> Holes
        {
            get
            {
                if (_holes == null)
                {
                    _holes = FindHoles();
                }

                return _holes;
            }
        }

        /// <summary>
        /// Signed distance (m) from <paramref name="p"/> to the nearest wall: positive inside the walkable world, negative
        /// beyond the walls (outside <see cref="Bounds"/> or in a <see cref="Holes">hole</see>). Continuous across the walls.
        /// </summary>
        public float WallDistance(Vector2 p)
        {
            Rect b = Bounds;
            float edge = Mathf.Min(Mathf.Min(p.x - b.xMin, b.xMax - p.x), Mathf.Min(p.y - b.yMin, b.yMax - p.y));
            IReadOnlyList<Rect> holes = Holes;
            if (holes.Count == 0 || edge <= 0f)
            {
                return edge;
            }

            float toHole = float.MaxValue;
            foreach (Rect hole in holes)
            {
                if (hole.Contains(p))
                {
                    float toPart = float.MaxValue;
                    foreach (OverworldPart part in Parts)
                    {
                        toPart = Mathf.Min(toPart, CityGeometry.DistanceToRect(p, part.Bounds));
                    }

                    return -toPart;
                }

                toHole = Mathf.Min(toHole, CityGeometry.DistanceToRect(p, hole));
            }

            return Mathf.Min(edge, toHole);
        }

        private List<Rect> FindHoles()
        {
            var holes = new List<Rect>();
            if (Parts.Count == 0)
            {
                return holes;
            }

            var xs = new SortedSet<float> { Bounds.xMin, Bounds.xMax };
            var zs = new SortedSet<float> { Bounds.yMin, Bounds.yMax };
            foreach (OverworldPart part in Parts)
            {
                xs.Add(part.Bounds.xMin);
                xs.Add(part.Bounds.xMax);
                zs.Add(part.Bounds.yMin);
                zs.Add(part.Bounds.yMax);
            }

            var x = new List<float>(xs);
            var z = new List<float>(zs);
            for (int i = 0; i + 1 < x.Count; i++)
            {
                for (int j = 0; j + 1 < z.Count; j++)
                {
                    Rect cell = Rect.MinMaxRect(x[i], z[j], x[i + 1], z[j + 1]);
                    if (cell.width > 0.01f && cell.height > 0.01f && !Parts.Exists(part => part.Bounds.Contains(cell.center)))
                    {
                        holes.Add(cell);
                    }
                }
            }

            return holes;
        }

        /// <summary>The region holding <paramref name="p"/> (half-open rectangles: each point belongs to one), else the nearest; null for a region on its own.</summary>
        public OverworldPart PartAt(Vector2 p)
        {
            OverworldPart best = null;
            float bestDistance = float.MaxValue;
            foreach (OverworldPart part in Parts)
            {
                if (part.Bounds.Contains(p))
                {
                    return part;
                }

                float d = CityGeometry.DistanceToRect(p, part.Bounds);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = part;
                }
            }

            return best;
        }

        /// <summary>The region whose areas include catalog map <paramref name="mapId"/> (stitched world), else null.</summary>
        public OverworldPart PartOfMap(string mapId)
        {
            foreach (OverworldZone zone in Zones)
            {
                if (zone.MapIdOrOwner == mapId && !string.IsNullOrEmpty(mapId))
                {
                    return PartAt(zone.Bounds.center);
                }
            }

            return null;
        }

        /// <summary>The catalog region at <paramref name="p"/> (the layout's own region when it is not stitched).</summary>
        public MapRegion RegionAt(Vector2 p) => PartAt(p)?.Region ?? Region;

        /// <summary>The weather/ambience biome at <paramref name="p"/>.</summary>
        public BiomeType BiomeAt(Vector2 p) => PartAt(p)?.Biome ?? World.Biome;

        /// <summary>The town of the region at <paramref name="p"/> (the layout's Vila when it is not stitched).</summary>
        public OverworldSettlement VilaAt(Vector2 p) => PartAt(p)?.Vila ?? Vila;

        public IEnumerable<OverworldZone> ZonesOfMap(string mapId)
        {
            foreach (OverworldZone zone in Zones)
            {
                if (zone.MapId == mapId)
                {
                    yield return zone;
                }
            }
        }

        public bool ContainsMap(string mapId)
        {
            foreach (OverworldZone zone in Zones)
            {
                if (zone.MapId == mapId && !string.IsNullOrEmpty(mapId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The settlement (and Barca stop) of a catalog map, or null.</summary>
        public OverworldSettlement SettlementOf(string mapId) => string.IsNullOrEmpty(mapId) ? null : Settlements.Find(s => s.MapId == mapId);

        /// <summary>
        /// Where a hero entering the overworld stands: at the gate towards <paramref name="fromMapId"/> when they walked in from
        /// a map outside the region, else at the dock of <paramref name="mapId"/>'s settlement, else at the Vila's dock.
        /// </summary>
        public Vector2 ArrivalFor(string mapId, string fromMapId, out Vector2 facing)
        {
            if (!string.IsNullOrEmpty(fromMapId) && !ContainsMap(fromMapId))
            {
                MapExitSpec gate = World.Exits.Find(e => e.Kind == MapExitKind.Map && e.TargetMapId == fromMapId);
                if (gate != null)
                {
                    facing = gate.ArrivalFacing;
                    return gate.Arrival;
                }
            }

            OverworldSettlement stop = SettlementOf(mapId) ?? Vila;
            if (stop != null && stop.HasDock)
            {
                facing = stop.DockArrivalFacing;
                return stop.DockArrival;
            }

            facing = World.DockArrivalFacing;
            return World.DockArrival;
        }

        /// <summary>The river blocker pieces (all rivers) bucketed by chunk, computed once.</summary>
        public IReadOnlyList<(Vector2 Center, float Length, float Yaw, float Width)> RiverPiecesIn(Vector2Int chunk)
        {
            if (_riverPieces == null)
            {
                _riverPieces = new Dictionary<Vector2Int, List<(Vector2, float, float, float)>>();
                foreach (MapRiver river in World.Rivers)
                {
                    foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                    {
                        Vector2Int c = OverworldRules.ChunkOf(this, center);
                        if (!_riverPieces.TryGetValue(c, out List<(Vector2, float, float, float)> list))
                        {
                            list = new List<(Vector2, float, float, float)>();
                            _riverPieces[c] = list;
                        }

                        list.Add((center, length, yaw, river.Width));
                    }
                }
            }

            return _riverPieces.TryGetValue(chunk, out List<(Vector2, float, float, float)> pieces)
                ? pieces
                : (IReadOnlyList<(Vector2, float, float, float)>)System.Array.Empty<(Vector2, float, float, float)>();
        }
    }
}
