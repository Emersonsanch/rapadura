using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The enemies built from code (see <see cref="ProceduralEnemyModels"/>): the five that never had a
    /// Meshy model, plus jointed low-poly animals replacing the static Meshy wolf, spider and scorpion so
    /// their legs can step (<see cref="Rapadura.Gameplay.Locomotion.ProceduralLegLocomotion"/>).
    /// </summary>
    public enum ProceduralEnemyKind
    {
        None = 0,
        BatSwarm = 1,
        StoneGolem = 2,
        Warlord = 3,
        RuinGuardian = 4,
        AncientDweller = 5,
        Wolf = 6,
        AlphaWolf = 7,
        Spider = 8,
        Scorpion = 9,

        // Night-only species (Docs/DayNight.md), each with its own model since the night-models pass.
        /// <summary>Lobisomem-do-sertão: shaggy quadruped that rears up on its hind legs to strike.</summary>
        Werewolf = 10,
        /// <summary>Morcego-vampiro gigante: one big bat with a wide wingspan.</summary>
        VampireBat = 11,
        /// <summary>Coruja-rasga-mortalha: barn owl with a pale heart-shaped face.</summary>
        ShroudOwl = 12,
        /// <summary>Enxame de vaga-lumes-de-fogo: a cloud of glowing fireflies that moves as one enemy.</summary>
        FireflySwarm = 13,
        /// <summary>Porco-do-mato da Caipora (queixada): bristly peccary with a pale collar, comes in herds.</summary>
        Peccary = 14,
        /// <summary>Mãe-da-lua (urutau): potoo with huge yellow eyes and a gaping mouth, carrying its branch.</summary>
        Potoo = 15,

        // [distância] (ranged) commons of the playable maps 07/08/13/14/19/20 (Docs/Mapas100.md), shoot via EnemyRangedAttack.
        /// <summary>Abutre de Brasa: bald-headed vulture with ember-tipped wings that spits embers.</summary>
        EmberVulture = 16,
        /// <summary>Lanterna Quebrada dos Artífices: a floating cracked lantern engine that shoots bolts of trapped light.</summary>
        BrokenLantern = 17,
        /// <summary>Gavião-de-Penacho: crested hawk that throws hard flight feathers like darts.</summary>
        CrestedHawk = 18,
        /// <summary>Peixe-Lanterna Apagado: a dimmed anglerfish swimming in the cave air, flicking globs from its lure.</summary>
        LanternFish = 19,

        // Guardians of the Ossuário dos Artífices (Docs/Dungeons.md), spawned by the DungeonDirector (OssuaryEnemyRecipes).
        /// <summary>Caveira Flutuante: an ivory skull with soul-fire sockets and a trail of green wisps.</summary>
        FloatingSkull = 20,
        /// <summary>Guerreiro Esqueleto de Cobre: copper-boned skeleton with a crested helm, bone shield and copper sword.</summary>
        SkeletonWarrior = 21,
        /// <summary>Arqueiro Esqueleto do Fosso: hooded ivory skeleton with a recurved longbow and rune arrows.</summary>
        SkeletonArcher = 22,
        /// <summary>Feiticeiro das Ossadas: hunched skeleton in a burial robe, bone staff with a violet orb.</summary>
        BoneSorcerer = 23,
        /// <summary>O Colosso de Crânios (boss): a giant of stacked skulls round a soul-fire core, with a femur club.</summary>
        BoneColossus = 24,

        // The beasts the map sheets of the continuous worlds (Docs/Mapas100.md 07–24) and Taa Pwani (25) asked for, which
        // used to be played by stand-ins (OverworldBeastRecipes, OverworldOddityRecipes).
        /// <summary>Chacal Apagado (Areias): lean sandy jackal with tall ears and a dark saddle, ember eyes.</summary>
        Jackal = 25,
        /// <summary>Cobra-de-Areia Apagada (Areias): horned sand viper, forepart raised, body lying in an S.</summary>
        SandSnake = 26,
        /// <summary>Orc Coroado de Āz (Areias, Tambor Rubro): a big orc wearing one of Āz's tin crowns, with a club.</summary>
        CrownedOrc = 27,
        /// <summary>Pedra que Rola (Kaggen): a ushabti boulder that rolls at you, molten cracks and runes.</summary>
        RollingStone = 28,
        /// <summary>Neblina do Vento Ruim (Kaggen): a knot of cold fog with two pale eyes and puffs circling it.</summary>
        IllWindMist = 29,
        /// <summary>Babuíno Apagado (Kaggen): grey-maned baboon, long pink muzzle, arched tail.</summary>
        Baboon = 30,
        /// <summary>Lodo de Gruta (Limiar): a mossy dome of cave mud with swallowed crystals poking out.</summary>
        CaveOoze = 31,
        /// <summary>Corvo-da-Treva (Limiar): black raven with violet-tipped feathers, throws them like darts.</summary>
        DarkCrow = 32,
        /// <summary>Gaivota Ladra (Costa das Monções): white-and-grey gull with a stolen coin in its beak.</summary>
        ThiefGull = 33,
        /// <summary>Caranguejo-dos-Coqueiros Gigante (Costa das Monções): red giant crab, one big pincer, eyes on stalks.</summary>
        CoconutCrab = 34,

        // The beasts of the late continuous worlds (Docs/Mapas100.md 25–40: Costa das Monções, Delta de Jhilpur, Teto do
        // Mundo) that the overworld layouts played with stand-ins (CoastDeltaBeastRecipes, PamirBeastRecipes).
        /// <summary>Crocodilo do Mangue Apagado (Costa, Delta): long armoured crocodile, back scutes, jaw that gapes.</summary>
        MangroveCrocodile = 35,
        /// <summary>Leão do Capim Apagado (Costa, 26): tawny lion with a dark mane and a tufted tail.</summary>
        GrassLion = 36,
        /// <summary>Rinoceronte-de-Pó (Costa, 26): dust-grey rhino with two horns, lowers its head to charge.</summary>
        DustRhino = 37,
        /// <summary>Macaco Ladrão (Delta, 32): grey langur with a black face, long curled tail, a stolen bangle.</summary>
        ThiefMonkey = 38,
        /// <summary>Lontra Ladra (Delta, 31): long low otter, head up, a stolen glowing fish in its mouth.</summary>
        ThiefOtter = 39,
        /// <summary>Píton Apagada (Delta, 32): big blotched python, forepart raised, body in a wide S (slithers like the sand snake).</summary>
        Python = 40,
        /// <summary>Trol Apagado (Delta, 33): mossy swamp troll with long arms and a caged will-o'-wisp on its back.</summary>
        DimmedTroll = 41,
        /// <summary>Urso-Pardo do Pamir Apagado (Teto, 36): brown bear with a shoulder hump and a jaw that gapes.</summary>
        BrownBear = 42,
        /// <summary>Iaque Selvagem Apagado (Teto, 38): shaggy black yak with a skirt of long hair and wide horns.</summary>
        WildYak = 43,
        /// <summary>Irbis Apagado (Teto, 40): pale snow leopard with dark rosettes and a long thick tail.</summary>
        Irbis = 44,
        /// <summary>Almas (Teto, 38): stooped, hairy wild man of the glacier holding a snowball.</summary>
        Almas = 45,
        /// <summary>Marmota Gigante (Teto, 36–37): chubby golden marmot with big incisors and a bushy tail.</summary>
        GiantMarmot = 46,

        // The beasts of the Planície de Mingcheng and the Estepe dos Nartes (Docs/Mapas100.md 41–52) that the overworld
        // layouts played with stand-ins (MingchengBeastRecipes, NartesBeastRecipes).
        /// <summary>Leão de Pedra Apagado (Mingcheng, 41–42): grey guardian lion statue, curly mane, a jaw that gapes.</summary>
        StoneLion = 47,
        /// <summary>Raposa de Cauda Lanterna (Mingcheng, 41): red fox whose bushy tail ends in a glowing paper lantern.</summary>
        LanternFox = 48,
        /// <summary>Soldado de Terracota Apagado (Mingcheng, 41, 45): clay soldier in lamellar armour with a halberd.</summary>
        TerracottaSoldier = 49,
        /// <summary>Tartaruga de Pedra (Mingcheng, 43): stone turtle carrying a stele on its shell, a jaw that snaps.</summary>
        StoneTurtle = 50,
        /// <summary>Búfalo de Lama (Mingcheng, 44): grey water buffalo with swept-back horns, lowers its head to charge.</summary>
        MudBuffalo = 51,
        /// <summary>Filhote de Nian (Mingcheng, 45): red-and-gold lion-dog cub with one horn and a big jaw.</summary>
        NianCub = 52,
        /// <summary>Grifo Jovem (Nartes, 47–48): lion body, eagle head and folded wings in Scythian gold; wings spread to strike.</summary>
        YoungGriffin = 53,
        /// <summary>Saiga Assustada (Nartes, 47): pale steppe antelope with the drooping nose and amber horns; lowers its head to butt.</summary>
        Saiga = 54,
        /// <summary>Lobo de Bronze (Nartes, 48): Scythian bronze wolf from the kurgans, verdigris spirals, a jaw that gapes.</summary>
        BronzeWolf = 55,
        /// <summary>Gigante Sonâmbulo (Nartes, 49): huge hill giant walking in his sleep, eyes shut, arms out; slams with both.</summary>
        SleepwalkerGiant = 56,
        /// <summary>Besouro-Casca (Nartes, 50): six-legged beetle with bark-ridged wing cases and pincer mandibles.</summary>
        BarkBeetle = 57,
        /// <summary>Ochokochi (Nartes, 52): shaggy Georgian forest man with the axe-blade on his chest; arms up to hug and cut.</summary>
        Ochokochi = 58,

        // The beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58) that the overworld layout played with stand-ins
        // (FarnkeshBeastRecipes).
        /// <summary>Bode de Fumaça (Vale, 53): goat whose fleece is smoke with embers, black curled horns; lowers its head to butt.</summary>
        SmokeGoat = 59,
        /// <summary>Abelha Dourada (Vale, 53): dog-sized bee turned to gold by Midas's touch; buzzing wings, the sting swings under.</summary>
        GoldenBee = 60,
        /// <summary>Pássaro-Flauta Apagado (Vale, 53): long-tailed songbird with a reed-pipe beak; [distância], the beak opens to shoot.</summary>
        FluteBird = 61,
        /// <summary>Raposa da Vinha (Vale, 53): red fox with a grape-stained muzzle and vines in its brush; a jaw that gapes.</summary>
        VineFox = 62,
        /// <summary>Gralha-Alpina (Farnkesh, 54): black chough with yellow beak and red legs; the beak opens on the windup.</summary>
        AlpineChough = 63,
        /// <summary>Takin Apagado (Farnkesh, 54): golden takin with the Roman nose, greyed by the shadow; lowers its head to charge.</summary>
        Takin = 64,
        /// <summary>Cabra-Azul (Farnkesh, 54): the bharal, slate blue with the black stripe, horns out; lowers its head to butt.</summary>
        BlueSheep = 65,
        /// <summary>Morcego-Vela (Pontes, 55): bat with a lit candle on its head; the flame flares before the bite.</summary>
        CandleBat = 66,
        /// <summary>Ushabti de Corda (Pontes, 55): servant figure of rope and clay with its hoe; both arms go up to strike.</summary>
        RopeUshabti = 67,
        /// <summary>Gato-de-Pallas Apagado (Ermidas, 56): stocky long-furred manul, flat ears, ringed tail; a jaw that gapes.</summary>
        PallasCat = 68,
        /// <summary>Águia da Tempestade (Garganta, 57): dark grey eagle with lightning in its wing tips; [distância].</summary>
        StormEagle = 69,
        /// <summary>Harpia de Fumaça (Ninho, 58): smoky bird of prey with a pale masked face and long talons; the beak opens to strike.</summary>
        SmokeHarpy = 70,

        // The beasts of the Mar Gelado (Docs/Mapas100.md 59–64) that the overworld layout played with stand-ins
        // (MarGeladoBeastRecipes).
        /// <summary>Foca-Leopardo Apagada (Porto, 59): long spotted seal with the big reptile head; flippers, a jaw that gapes.</summary>
        LeopardSeal = 71,
        /// <summary>Morsa de Fumaça (Porto, 59): heavy walrus whose hide smokes, ivory tusks; lowers its head to gore.</summary>
        SmokeWalrus = 72,
        /// <summary>Gaivota-do-Gelo (Porto, 59): white gull with frosted grey wings; [distância], the beak opens to shoot.</summary>
        IceGull = 73,
        /// <summary>Raposa-do-Ártico Ladra (Porto, 59): small white fox with a stolen silver fish in its brush; a jaw that gapes.</summary>
        ArcticFox = 74,
        /// <summary>Urso-Polar Apagado (Canais, 60): big white bear streaked with shadow-grey; a jaw that gapes.</summary>
        PolarBear = 75,
        /// <summary>Rena de Barro (Tundra, 61): Omöl's clumsy reindeer of cracked clay and crooked antlers; lowers its head to charge.</summary>
        ClayReindeer = 76,
        /// <summary>Lebre-Pedra (Tundra, 61): Omöl's hare of grey stone with long ears; throws itself head first.</summary>
        StoneHare = 77,
        /// <summary>Ave-Graveto de Omöl (Tundra, 61): clumsy bird of bark and twigs, big yellow eyes; [distância].</summary>
        TwigBird = 78,
        /// <summary>Mamute de Baixo da Terra (Galerias, 62): shaggy mammoth that digs the valleys, frost and ice crystals on its back; lowers its tusks to charge.</summary>
        BurrowMammoth = 79,
        /// <summary>Lobo-do-Mar (Mar das Auroras, 63): slate-blue wolf that swims, webbed paws and a fin on the back; a jaw that gapes.</summary>
        SeaWolf = 80,
        /// <summary>Lúcio Gigante (Fundo do Kul, 64): long olive pike that swims in the cold air over the fishing holes; a jaw that gapes.</summary>
        GiantPike = 81,
        /// <summary>Foca-Sombra (Fundo do Kul, 64): round dark seal with pale glowing rings; a jaw that gapes.</summary>
        ShadowSeal = 82,

        // The beasts of the Arquipélago de Hinokura (Docs/Mapas100.md 65–70) that the overworld layout played with stand-ins
        // (HinokuraBeastRecipes).
        /// <summary>Caranguejo-Vulcão (Hinokura, 65): basalt crab with a smoking cone on its back; the pincers open on the windup.</summary>
        VolcanoCrab = 83,
        /// <summary>Lagarto de Lava (Hinokura, 65): low black lizard with a glowing stripe; a jaw that gapes.</summary>
        LavaLizard = 84,
        /// <summary>Gaivota-Brasa (Hinokura, 65): soot-grey gull with ember wing tips; [distância], the beak opens to spit.</summary>
        EmberGull = 85,
        /// <summary>Shisa Apagada (Jardins, 66): red-glazed ceramic lion-dog with a curled mane; a wide jaw.</summary>
        DimmedShisa = 86,
        /// <summary>Tanuki Ladrão (Jardins, 66): round masked raccoon-dog, ringed brush, a leaf on its head; a jaw that gapes.</summary>
        Tanuki = 87,
        /// <summary>Tibicena (Echeyde, 67): lean black dog, ribs and ember eyes; a jaw that gapes.</summary>
        Tibicena = 88,
        /// <summary>Lagarto-Gigante de Lava (Echeyde, 67): broad lizard plated in basalt, lava in the seams; a jaw that gapes.</summary>
        GiantLavaLizard = 89,
        /// <summary>Peixe-Lâmina (Recife, 68): flat fish of black glass that swims in the steam; a jaw that gapes, blade fins that row.</summary>
        BladeFish = 90,
        /// <summary>Coral de Brasa (Recife, 68): drifting coral with embers at its tips; [distância], the mouth opens to spit.</summary>
        EmberCoral = 91,
        /// <summary>Ushabti de Laca (Castelo, 69): red-lacquered servant figure with forge tongs; both arms go up to strike.</summary>
        LacquerUshabti = 92,
        /// <summary>Ogro-Fornalha (Castelo, 69): iron ogre with a furnace in its belly and a hammer; both arms go up to strike.</summary>
        FurnaceOgre = 93,
        /// <summary>Gigante de Escória (Caldeira, 70): heap of slag with lava in the seams, huge fists; both arms go up to strike.</summary>
        SlagGiant = 94,

        // The beasts of the Rio das Doze Horas (Docs/Mapas100.md 71–74) that the overworld layout played with stand-ins (RioBeastRecipes).
        /// <summary>Crocodilo do Rio Escuro (71): night-blue crocodile, gold eyes; the jaw gapes on the windup.</summary>
        DarkRiverCrocodile = 95,
        /// <summary>Íbis Apagada (71): ash-grey ibis, black head, long down-curved bill; the bill opens on the windup.</summary>
        DimmedIbis = 96,
        /// <summary>Escaravelho de Sombra (71): black scarab with gold seams, a dim sun on its brow; the mandibles open.</summary>
        ShadowScarab = 97,
        /// <summary>Junco-Laço (71): drifting sheaf of reeds with a noose on each side; a mouth that gapes.</summary>
        ReedSnare = 98,
        /// <summary>Gafanhoto de Sombra (72): dark locust, glassy wings; the mouth opens with the wings held still.</summary>
        ShadowLocust = 99,
        /// <summary>Hipopótamo de Lodo (72): barrel of mud with reeds on its back, huge square mouth that gapes.</summary>
        MudHippo = 100,
        /// <summary>Cobra do Junco (72): reed-green snake with a raised head and a jaw; the tail waves.</summary>
        ReedSnake = 101,
        /// <summary>Serpente de Três Cabeças (73): heavy sand-gold serpent, three raised heads, the tail waves.</summary>
        ThreeHeadedSerpent = 102,
        /// <summary>Escorpião Negro (73): black scorpion, gold-banded tail, ember sting; the claws open and the sting draws back.</summary>
        BlackScorpion = 103,
        /// <summary>Ushabti de Areia (73): servant figure of packed sand with a hoe; both arms go up to strike.</summary>
        SandUshabti = 104,
        /// <summary>Mordedor de Sombra (74): lean dog of the dark, all mouth; a jaw that gapes.</summary>
        ShadowBiter = 105,
        /// <summary>Treva Rastejante (74): low heap of dark with crawling tendrils; flattens on the windup, the swallowed gold flares.</summary>
        CreepingDark = 106,

        // The beasts of the Margens da Mistura (Docs/Mapas100.md 77–82) that the overworld layout played with stand-ins (MisturaBeastRecipes).
        /// <summary>Leão de Tijolo Apagado (77): baked-brick lion with courses of blue glaze; the jaw gapes on the windup.</summary>
        BrickLion = 107,
        /// <summary>Mušḫuššu Jovem (77): scaled dragon-snake, one horn, a sting on the tail; a jaw that gapes.</summary>
        YoungMushussu = 108,
        /// <summary>Gafanhoto de Betume (77): pitch-black locust, embers in the seams; the mouth opens with the wings held still.</summary>
        BitumenLocust = 109,
        /// <summary>Ampulheta Viva (78): walking stone hourglass with lit sand; both arms go up to strike.</summary>
        LivingHourglass = 110,
        /// <summary>Sombra Atrasada (78): ragged shadow with a long trail; a mouth that gapes, the sleeves stir.</summary>
        LateShadow = 111,
        /// <summary>Betume Vivo (79): heap of pitch with embers inside; flattens on the windup, the embers flare.</summary>
        LivingTar = 112,
        /// <summary>Garça de Piche (79): black heron on long legs; the bill opens on the windup to spit.</summary>
        TarHeron = 113,
        /// <summary>Cedro Raivoso (80): walking cedar with a crown of three tiers; both branches go up to strike.</summary>
        AngryCedar = 114,
        /// <summary>Manto de Brilho (80): an empty golden cloak that hovers; the sleeves and the hem stir.</summary>
        GlowMantle = 115,
        /// <summary>Filho de Ullikummi (81): diorite that grew legs, crystals on the shoulders; both arms go up to strike.</summary>
        UllikummiSon = 116,
        /// <summary>Tábua Quebrada (82): standing slab with a broken corner and lit cuneiform; leans back and flares to throw.</summary>
        BrokenTablet = 117,
        /// <summary>Jardim-Sombra (82): cut tree of the dream, sawn flat at the top; both arms go up to strike.</summary>
        GardenShadow = 118,

        // The beasts of the Limiar Profundo (Docs/Mapas100.md 84–90) that the overworld layout played with stand-ins (ProfundoBeastRecipes).
        /// <summary>Sombra-Cópia (84): thin faceless copy with an amber seam; both arms go up to strike.</summary>
        CopyShadow = 119,
        /// <summary>Gigante de Fumaça (84): rings of smoke stacked into a giant; both arms go up to strike.</summary>
        SmokeGiant = 120,
        /// <summary>Arqueiro de Fumaça (84): drifting smoke with a copper bow; a mouth that gapes, the sleeves stir.</summary>
        SmokeArcher = 121,
        /// <summary>Lobo de Brasa (85): ash wolf with embers on the spine; a jaw that gapes.</summary>
        EmberWolf = 122,
        /// <summary>Leão de Fumaça Quente (85): basalt lion, a mane of smoke with fire inside; the jaw gapes on the windup.</summary>
        HotSmokeLion = 123,
        /// <summary>Águia Negra (86): black eagle with white eyes; the beak opens with the wings held still.</summary>
        BlackEagle = 124,
        /// <summary>Harpia de Vento (86): grey-blue harpy; the mouth opens with the wings held still to throw.</summary>
        WindHarpy = 125,
        /// <summary>Polvo de Lodo (87): dome of mire on eight arms; flattens on the windup, the amber flares.</summary>
        MudOctopus = 126,
        /// <summary>Peixe da Treva (87): flat black fish swimming in the air; the fins row, the jaw gapes.</summary>
        DarkFish = 127,
        /// <summary>Lagarto-Sombra (88): low and very wide, nearly no eyes; the jaw gapes and the tail sways.</summary>
        ShadowLizard = 128,
        /// <summary>Dragão-Sombra Menor (88): small dark dragon with unfinished wings; a jaw that gapes.</summary>
        LesserShadowDragon = 129,
        /// <summary>Arconte Menor (90): fat with swallowed light, a crown of dead lamps; both arms go up to strike.</summary>
        LesserArchon = 130,

        // The beasts of the Reinos do Céu (Docs/Mapas100.md 91–99) that the overworld layout played with stand-ins (CeuBeastRecipes).
        /// <summary>Ave de Nuvem Apagada (91): grey bird of cloud gone out; the beak opens with the wings held still.</summary>
        DimmedCloudBird = 131,
        /// <summary>Peixe de Luz Apagado (92): dull gold fish swimming in the air, a spent drop on a thread; the fins row, the jaw gapes.</summary>
        DimmedLightFish = 132,
        /// <summary>Clandestino de Treva (93): low shadow with a stolen boarding tag; a jaw that gapes.</summary>
        DarkStowaway = 133,
        /// <summary>Sombra do Porão (93): heap of shadow with cargo stuck in it; flattens on the windup, the lantern flares.</summary>
        HoldShadow = 134,
        /// <summary>Corda Viva (93, 97): coil of ship's rope with a noose for a head; a mouth that gapes, the loose ends stir.</summary>
        LivingRope = 135,
        /// <summary>Um dos Doze (94): a toy ram with a tin crown and a wind-up key; a jaw that gapes.</summary>
        ToyArchon = 136,
        /// <summary>Jarra Rachada (94): a jar of the Wheel, floating; leans back on the windup and the crack flares.</summary>
        CrackedJar = 137,
        /// <summary>Arconte Acorrentado (95): sulking giant in cuffs, the chains reach the ground; both arms go up to strike.</summary>
        ChainedArchon = 138,
        /// <summary>Gigante-Balão (95): an archon blown up round on two thin legs; both arms go up to strike.</summary>
        BalloonGiant = 139,
        /// <summary>Ladrão de Āz (96): hooded, a sack of light on its back; both arms go up to strike.</summary>
        AzThief = 140,
        /// <summary>Gaivota de Luz Apagada (97): pale gull, spits the last ember of its light; the beak opens with the wings held still.</summary>
        DimmedLightGull = 141,
        /// <summary>Pedra do Céu (99): a slab of the sky on four stumps, gold in the cracks; a jaw that gapes.</summary>
        SkyStone = 142,
        /// <summary>O Rei-Vendaval (58): the faceless wind-bird, world boss of the Picos de Farnkesh.</summary>
        WindKing = 143,
        /// <summary>O Velho Kul (64): the sour old water spirit, world boss of the Mar Gelado.</summary>
        OldKul = 144,
        /// <summary>O Glutão de Brasa (70): the mouth of the Devouring Fire, world boss of Hinokura.</summary>
        EmberGlutton = 145,
        /// <summary>Apep (76): the serpent that drinks the river, coiled with her neck up; world boss of the Rio das Doze Horas.</summary>
        Apep = 146,
        /// <summary>O Sonho da Destruição (82): a robed shadow of broken tablets; world boss of the Margens da Mistura.</summary>
        DreamOfDestruction = 147,
        /// <summary>A Grande Treva (90): the Prince of Darkness in silhouette (lion head, eagle wings, dragon body, fish tail, demon feet); world boss of the Limiar Profundo.</summary>
        GreatDark = 148,
        /// <summary>Āz, a Capitã Cobiçosa (96): the greedy captain holding the Última Arca; world boss of the Reinos do Céu.</summary>
        AzCaptain = 149,
        /// <summary>Hedammu, a Serpente Gulosa do Mar (30): coiled, mouth open, the swallowed sparks lit; world boss of the Costa das Monções.</summary>
        Hedammu = 150,
        /// <summary>O Lodo-que-Engole (35): the faceless mud of the Dark standing up; world boss of the Delta de Jhilpur.</summary>
        SwallowingMire = 151,
        /// <summary>Aydahar, o Dragão do Passo (40): the snow-pale dragon on Āz's gold; world boss of the Teto do Mundo.</summary>
        Aydahar = 152,
        /// <summary>Os Quatro Gulosos (46): a walking bronze vessel with four faces; world boss of the Planície de Mingcheng.</summary>
        FourGluttons = 153,
        /// <summary>Ohya, o Gigante Irmão (52): Hahya's chained brother; world boss of the Estepe dos Nartes.</summary>
        Ohya = 154,

        // The eleventh pass of beasts (bible §3.2.1): the roles of Regions 12–16 that still used stand-ins (LateStandInBeastRecipes).
        /// <summary>Salamandra Devoradora (70): low dark salamander with an ember stripe; jaw and tail.</summary>
        DevouringSalamander = 180,
        /// <summary>Bomba Vulcânica (67): cracked ball of slag; tilts on the windup and the core flares.</summary>
        VolcanicBomb = 181,
        /// <summary>Ganso Ladrão (72): big goose with a stolen ring; beak opens with the wings held still.</summary>
        ThiefGoose = 182,
        /// <summary>Filho de Apep (76): small violet serpent, head up; the tail waves, jaw gapes.</summary>
        ChildOfApep = 183,
        /// <summary>Areia Movediça (76): slow heap of sand; flattens on the windup, the sunken lamp flares.</summary>
        Quicksand = 184,
        /// <summary>Homem-Escorpião (77): bow in hand, a hand above the ground; a jaw that gapes, the tail sways.</summary>
        ScorpionMan = 185,
        /// <summary>Lince do Cedro (80): tufted, resin in the fur; jaw and tail.</summary>
        CedarLynx = 186,
        /// <summary>Abelha-Resina (80): fat bee with a lit abdomen; fast wings, the sting swings.</summary>
        ResinBee = 187,
        /// <summary>Morcego-Lâmina (86): iron bat with blade fingers; bites with the wings held still.</summary>
        BladeBat = 188,
        /// <summary>Enguia Grande (87): thick dark eel with barbels; the tail waves, jaw gapes.</summary>
        GreatEel = 189,
        /// <summary>Touro de Cinza (85): bull of packed ash with embers in the seams; the head drops, horns first.</summary>
        AshBull = 190,
        /// <summary>Corda de Fumaça (97): a coil of smoke shaped like a rope; a mouth that gapes, the loose ends stir.</summary>
        SmokeRope = 191,

        // The twelfth pass of beasts (bible §3.2.1): the roles of Regions 1–4 that still used stand-ins (EarlyStandInBeastRecipes).
        /// <summary>Cegonha Apagada (02): grey-edged stork with twigs to throw; the beak opens, wings held still.</summary>
        DimmedStork = 192,
        /// <summary>Sapo-Bolha do Lodo (03): fat mud toad; flattens on the windup and the bubble in its throat flares.</summary>
        BubbleToad = 193,
        /// <summary>Mosquito de Fumaça (03): a smoke mosquito; fast wings, the proboscis swings up.</summary>
        SmokeMosquito = 194,
        /// <summary>Mariposa de Seda Gigante (04): pale moth with lit eyespots; the palps open, wings held still.</summary>
        SilkMoth = 195,
        /// <summary>Pombo-do-Louro Apagado (05): plump grey pigeon with a green band; the beak opens.</summary>
        LaurelPigeon = 196,
        /// <summary>Lagarto-Gigante do Louro (05): green lizard with a crest of bark; jaw and tail.</summary>
        LaurelLizard = 197,
        /// <summary>Pavão de Fumaça (06): a peacock of smoke with ember eyes in its fan; the beak opens.</summary>
        SmokePeacock = 198,
        /// <summary>Lagarto-Espinho (10): sand lizard all thorns, the tips lit; jaw and tail.</summary>
        ThornLizard = 199,
        /// <summary>Lince Apagado (14): grey lynx with a dark patch; jaw and tail.</summary>
        DimmedLynx = 200,
        /// <summary>Borrão (15): the shadow out of the paintings; the mouth gapes, the smeared arms stir.</summary>
        Smudge = 201,
        /// <summary>Gralha de Pedra (16): slate jackdaw with a pebble in the beak; the beak opens.</summary>
        StoneJackdaw = 202,
        /// <summary>Ubır Jovem (20): floating ball of dark hair with an ember mouth; the mouth gapes, the claws stir.</summary>
        YoungUbir = 203,

        // The thirteenth pass of beasts (bible §3.2.1): the roles of Regions 5–9 that still used stand-ins (MidStandInBeastRecipes).
        /// <summary>Ave-Relâmpago (26): black-and-white savanna bird crackling with caught light; the beak opens, wings held still.</summary>
        LightningBird = 220,
        /// <summary>Garça-Lança Apagada (31): grey heron with a spearhead beak; the beak opens.</summary>
        SpearHeron = 221,
        /// <summary>Abutre-Barbudo Apagado (37): bearded vulture with a stone in its claws; the beak opens.</summary>
        BeardedVulture = 222,
        /// <summary>Cisne de Feltro (48): a swan of white felt with sewn-on colours; the beak opens.</summary>
        FeltSwan = 223,
        /// <summary>Gralha-de-Bico-Vermelho (36): glossy black chough with a red bill; the beak opens.</summary>
        RedBilledChough = 224,
        /// <summary>Moreia Apagada (25): a moray coiled on the reef, head raised; jaw and tail.</summary>
        DimmedMoray = 225,
        /// <summary>Carpa-Dragão Apagada (43): grey carp swimming in the air, a few gold scales left; jaw and tail.</summary>
        DragonCarp = 226,
        /// <summary>Lobo de Gelo (38): a wolf of packed snow, ice along the spine; jaw and tail.</summary>
        IceWolf = 227,
        /// <summary>Cavalo de Terracota (45): a clay horse lit through its cracks; jaw and tail.</summary>
        TerracottaHorse = 228,
        /// <summary>Caranguejo-Violinista (30): dark mangrove crab with one huge pale claw (the Jaw) that swings down on the windup.</summary>
        FiddlerCrab = 229,
        /// <summary>Ouriço de Fumaça (25): a smoke urchin; squashes on the windup and the ember on top flares.</summary>
        SmokeUrchin = 230,
        /// <summary>Sanguessuga do Lodo (32): a rearing leech; squashes on the windup and the lit sucker flares.</summary>
        MireLeech = 231,

        // The map bosses of the fichas (RegionBossRules.MapSpawns, MapBossRecipes): one per map the story walks through in the 150–300 band.
        /// <summary>O Cortador de Cordas (55): a shadow bird with blade-edged wings and a frayed rope in its beak; map boss of the Pontes de Vento.</summary>
        RopeCutter = 204,
        /// <summary>O Sino que Não Para (56): a giant bronze bell walking on two legs, its clapper the tongue, its sound lit inside; map boss of the Ermidas Suspensas.</summary>
        EndlessBell = 205,
        /// <summary>A Morsa-Rainha (60): a crowned walrus sitting up on her flippers, fish shining in her belly; map boss of the Canais de Gelo.</summary>
        WalrusQueen = 206,
        /// <summary>O Mamute Avô (62): the old mammoth of the galleries, hair to the ground, tusks curling, earth on its back; map boss of the Galerias dos Mamutes.</summary>
        GrandfatherMammoth = 207,
        /// <summary>O Rei Shisa do Telhado (66): the ceramic lion-dog of the palace roof, grown huge, its mane of glazed curls; map boss of the Jardins de Lava Laqueada.</summary>
        ShisaKing = 208,
        /// <summary>O Mestre das Nove Fornalhas (69): an iron forging engine with a furnace for a chest and a hammer arm; map boss of the Castelo das Nove Fornalhas.</summary>
        FurnaceMaster = 209,
    }

    /// <summary>
    /// A procedural enemy model as data: a flat list of named parts (index of the parent, local
    /// pose, optional mesh). Meshes can be shared by several parts (the bats of the swarm). Turned
    /// into a GameObject hierarchy by <see cref="ProceduralEnemyModels.Instantiate(ProceduralEnemyBlueprint, Material, Transform)"/>
    /// or baked to assets by the editor menu. Owns its meshes: <see cref="DestroyMeshes"/> them when
    /// the blueprint is not cached.
    /// </summary>
    public sealed class ProceduralEnemyBlueprint
    {
        public struct Part
        {
            public string Name;
            /// <summary>Index of the parent part, or -1 for a child of the model root.</summary>
            public int Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            /// <summary>Null for a pivot-only node.</summary>
            public Mesh Mesh;
        }

        private readonly List<Part> _parts = new List<Part>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly Dictionary<string, Mesh> _meshByKey = new Dictionary<string, Mesh>(StringComparer.Ordinal);
        private readonly EnemyMeshBuilder _builder = new EnemyMeshBuilder();

        private readonly string _meshNamePrefix;

        /// <param name="meshNamePrefix">Prefix of the mesh names; default "ProcEnemy_&lt;kind&gt;_" (the pet model passes its own).</param>
        public ProceduralEnemyBlueprint(ProceduralEnemyKind kind, bool isBoss, string meshNamePrefix = null)
        {
            Kind = kind;
            IsBoss = isBoss;
            _meshNamePrefix = meshNamePrefix ?? "ProcEnemy_" + kind + "_";
        }

        public ProceduralEnemyKind Kind { get; }
        public bool IsBoss { get; }
        public IReadOnlyList<Part> Parts => _parts;

        /// <summary>Distinct meshes (each listed once even when several parts share it).</summary>
        public IReadOnlyList<Mesh> Meshes => _meshes;

        /// <summary>Triangles drawn by the whole model (a shared mesh counts once per part using it).</summary>
        public int TriangleCount
        {
            get
            {
                int total = 0;
                foreach (Part part in _parts)
                {
                    if (part.Mesh != null)
                    {
                        total += part.Mesh.triangles.Length / 3;
                    }
                }

                return total;
            }
        }

        public int IndexOf(string partName)
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i].Name == partName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Adds a pivot-only node; returns its index (pass as <c>parent</c> of its children).</summary>
        public int Node(string name, int parent, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        {
            return AddPart(name, parent, position, rotation, scale, null);
        }

        /// <summary>
        /// Adds a part whose mesh is drawn by <paramref name="draw"/> in the part's local space. Parts
        /// passing the same <paramref name="meshKey"/> share one mesh (drawn once, by the first).
        /// </summary>
        public int Mesh(string name, int parent, Vector3 position, Action<EnemyMeshBuilder> draw, Quaternion? rotation = null,
            Vector3? scale = null, string meshKey = null)
        {
            string key = meshKey ?? name;
            if (!_meshByKey.TryGetValue(key, out Mesh mesh))
            {
                _builder.Clear();
                draw(_builder);
                mesh = _builder.ToMesh(_meshNamePrefix + key);
                _meshByKey[key] = mesh;
                _meshes.Add(mesh);
            }

            return AddPart(name, parent, position, rotation, scale, mesh);
        }

        private int AddPart(string name, int parent, Vector3 position, Quaternion? rotation, Vector3? scale, Mesh mesh)
        {
            if (parent >= _parts.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(parent), "A part's parent must be added before it.");
            }

            _parts.Add(new Part
            {
                Name = name,
                Parent = parent,
                Position = position,
                Rotation = rotation ?? Quaternion.identity,
                Scale = scale ?? Vector3.one,
                Mesh = mesh,
            });
            return _parts.Count - 1;
        }

        /// <summary>Destroys the meshes (only for blueprints that are not cached/baked).</summary>
        public void DestroyMeshes()
        {
            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();
            _meshByKey.Clear();
        }
    }
}
