using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Sets
{
    /// <summary>One two-piece armour set of a region (Docs/Items.md: Regions 1–16) and the region beasts that drop it.</summary>
    public sealed class ArmorSetDefinition
    {
        public readonly string Id;
        public readonly string ChestItemId;
        public readonly string HelmItemId;
        public readonly IReadOnlyList<string> Beasts;

        /// <summary>Chance of a piece per kill of one of <see cref="Beasts"/> (either piece, evenly).</summary>
        public readonly double DropChance;

        public ArmorSetDefinition(string id, string chestItemId, string helmItemId, IReadOnlyList<string> beasts)
            : this(id, chestItemId, helmItemId, ArmorSetRules.DropChance, beasts)
        {
        }

        public ArmorSetDefinition(string id, string chestItemId, string helmItemId, double dropChance, IReadOnlyList<string> beasts)
        {
            Id = id;
            ChestItemId = chestItemId;
            HelmItemId = helmItemId;
            DropChance = dropChance;
            Beasts = beasts;
        }
    }

    /// <summary>
    /// Pure rules of the region armour sets (same shape as the Armadura de Marfim in <c>DungeonRules</c>): which beasts drop which
    /// piece, how many pieces are worn and when each two-piece bonus fires. <see cref="ArmorSetBonuses"/> only wires them to combat.
    /// </summary>
    public static class ArmorSetRules
    {
        public const int PiecesForBonus = 2;

        /// <summary>Chance of a piece per kill of one of the region's own beasts (one kill in 200; either piece, evenly).</summary>
        public const double DropChance = 0.005;

        /// <summary>
        /// The same chance for the sets of Regions 5–9 (one kill in 100): their regions have 6–9 own beasts among many borrowed
        /// ones, against 12 in Regions 13–16, so at 1/200 the pair would cost more than the 1 500 kills of the economy budget.
        /// </summary>
        public const double MiddleDropChance = 0.01;

        /// <summary>
        /// The chance for the Uncommon sets of Regions 1–4, the hub regions (one kill in 200, like the far sets): there the region's own
        /// beasts are almost everything that walks (the base beasts of each biome plus the early ones) and the hero spends levels 1–50
        /// killing them, so at 1/100 the pair would come long before the region's world boss.
        /// </summary>
        public const double EarlyDropChance = 0.005;

        /// <summary>
        /// The chance for the sets of Regions 10–12 (one kill in ~133), between the middle and the far ones: 12 own beasts each, but among
        /// 17–32 species (the Picos de Farnkesh borrow the most), so at 1/200 the pair would pass the 1 500 kills of the economy budget.
        /// </summary>
        public const double HighDropChance = 0.0075;

        /// <summary>
        /// The chance for the second sets of Regions 13–16, the old sets of the fichas (one kill in 50): each drops from the 3 own beasts the
        /// first set of the region left out, among 21–25 species, so at 1/200 the pair would cost ~5 000 kills hunting the region; at 1/50 it
        /// is 1 050–1 250, inside the 1 500 of the economy budget.
        /// </summary>
        public const double AncientDropChance = 0.02;

        public const string PapyrusChestId = "armor_papiro_chest";
        public const string PapyrusHelmId = "armor_papiro_helm";
        public const string BlueBrickChestId = "armor_tijolo_chest";
        public const string BlueBrickHelmId = "armor_tijolo_helm";
        public const string SealChestId = "armor_selo_chest";
        public const string SealHelmId = "armor_selo_helm";
        public const string LightChestId = "armor_luz_chest";
        public const string LightHelmId = "armor_luz_helm";
        public const string CoralChestId = "armor_coral_chest";
        public const string CoralHelmId = "armor_coral_helm";
        public const string LotusChestId = "armor_lotus_chest";
        public const string LotusHelmId = "armor_lotus_helm";
        public const string YakChestId = "armor_iaque_chest";
        public const string YakHelmId = "armor_iaque_helm";
        public const string BronzeChestId = "armor_bronze_chest";
        public const string BronzeHelmId = "armor_bronze_helm";
        public const string KurganChestId = "armor_kurgan_chest";
        public const string KurganHelmId = "armor_kurgan_helm";
        public const string GardenChestId = "armor_jardim_chest";
        public const string GardenHelmId = "armor_jardim_helm";
        public const string RedLeatherChestId = "armor_rubro_chest";
        public const string RedLeatherHelmId = "armor_rubro_helm";
        public const string DrystoneChestId = "armor_pedra_chest";
        public const string DrystoneHelmId = "armor_pedra_helm";
        public const string WickChestId = "armor_pavio_chest";
        public const string WickHelmId = "armor_pavio_helm";
        public const string WindBellChestId = "armor_sino_chest";
        public const string WindBellHelmId = "armor_sino_helm";
        public const string AuroraSilverChestId = "armor_aurora_chest";
        public const string AuroraSilverHelmId = "armor_aurora_helm";
        public const string CelestialIronChestId = "armor_ferro_chest";
        public const string CelestialIronHelmId = "armor_ferro_helm";
        public const string MehenScaleChestId = "armor_mehen_chest";
        public const string MehenScaleHelmId = "armor_mehen_helm";
        public const string GiantsChestId = "armor_gigantes_chest";
        public const string GiantsHelmId = "armor_gigantes_helm";
        public const string FiveLightsChestId = "armor_cinco_chest";
        public const string FiveLightsHelmId = "armor_cinco_helm";
        public const string GloryColumnChestId = "armor_coluna_chest";
        public const string GloryColumnHelmId = "armor_coluna_helm";

        // ---- Regions 1–4, the hub regions: Uncommon sets below the Marfim, from the region's own beasts (Docs/Status_100Mapas.md).

        /// <summary>Jardim de Mihrshah (Região 1, Bosque do Zarafshan): the "conjunto antigo do jardim" of the sheet of map 06.</summary>
        public static readonly ArmorSetDefinition Garden = new ArmorSetDefinition("jardim_de_mihrshah", GardenChestId, GardenHelmId, EarlyDropChance, new[]
        {
            "enemy_forest_wolf", "enemy_forest_spider", "enemy_forest_peccary", "enemy_forest_shroud_owl", "enemy_forest_werewolf",
            "enemy_forest_dimmed_stork", "enemy_forest_bubble_toad", "enemy_forest_smoke_mosquito", "enemy_forest_silk_moth",
            "enemy_forest_laurel_pigeon", "enemy_forest_laurel_lizard", "enemy_forest_smoke_peacock",
        });

        /// <summary>Couro Rubro (Região 2, Areias Flamejantes).</summary>
        public static readonly ArmorSetDefinition RedLeather = new ArmorSetDefinition("couro_rubro", RedLeatherChestId, RedLeatherHelmId, EarlyDropChance, new[]
        {
            "enemy_desert_scorpion", "enemy_desert_ember_vulture", "enemy_desert_broken_lantern", "enemy_desert_firefly_swarm",
            "enemy_desert_jackal", "enemy_desert_sand_snake", "enemy_desert_crowned_orc", "enemy_desert_thorn_lizard",
        });

        /// <summary>Pedra Seca (Região 3, Serra de Kaggen).</summary>
        public static readonly ArmorSetDefinition Drystone = new ArmorSetDefinition("pedra_seca", DrystoneChestId, DrystoneHelmId, EarlyDropChance, new[]
        {
            "enemy_mountain_stone_golem", "enemy_mountain_rolling_stone", "enemy_mountain_crested_hawk", "enemy_mountain_potoo",
            "enemy_mountain_ill_wind_mist", "enemy_mountain_baboon", "enemy_mountain_dimmed_lynx", "enemy_mountain_smudge",
            "enemy_mountain_stone_jackdaw",
        });

        /// <summary>Pavio do Limiar (Região 4, Grutas do Limiar).</summary>
        public static readonly ArmorSetDefinition Wick = new ArmorSetDefinition("pavio_do_limiar", WickChestId, WickHelmId, EarlyDropChance, new[]
        {
            "enemy_cave_bat_swarm", "enemy_cave_vampire_bat", "enemy_cave_lantern_fish", "enemy_cave_ooze", "enemy_cave_dark_crow",
            "enemy_cave_young_ubir",
        });

        // ---- Regions 5–9, the road of the middle story: each set drops from the region's own beasts (Docs/Status_100Mapas.md).

        /// <summary>Coral de Kilwa (Região 5, Costa das Monções).</summary>
        public static readonly ArmorSetDefinition Coral = new ArmorSetDefinition("coral_de_kilwa", CoralChestId, CoralHelmId, MiddleDropChance, new[]
        {
            "enemy_forest_thief_gull", "enemy_forest_coconut_crab", "enemy_forest_mangrove_crocodile", "enemy_forest_grass_lion",
            "enemy_forest_dust_rhino", "enemy_forest_dimmed_moray", "enemy_forest_smoke_urchin", "enemy_forest_lightning_bird",
            "enemy_forest_fiddler_crab",
            // Three of the nineteenth pass (CoastOwnBeastRecipes), the reef's own beasts in the roles it had lent (Kirta and Kilwa, where the
            // coral is); Meroë's three and the kongamato stay out, since a set takes at most 12.
            "enemy_cave_thief_octopus", "enemy_cave_mud_jellyfish", "enemy_cave_dimmed_lionfish",
        });

        /// <summary>Seda de Lótus (Região 6, Delta de Jhilpur).</summary>
        public static readonly ArmorSetDefinition Lotus = new ArmorSetDefinition("seda_de_lotus", LotusChestId, LotusHelmId, MiddleDropChance, new[]
        {
            "enemy_forest_thief_monkey", "enemy_forest_thief_otter", "enemy_forest_dimmed_python", "enemy_forest_dimmed_troll",
            "enemy_forest_spear_heron", "enemy_forest_mire_leech",
            // Three of the nineteenth pass (CoastOwnBeastRecipes), the Delta's own beasts in the roles it had lent: the heron and the leech
            // are listed in the Costa's file, so tools/balance counted 4 own of 19 species and the farm went up to 1425 kills (budget 1500).
            "enemy_forest_snare_root", "enemy_forest_ghost_heron", "enemy_cave_smoke_dragonfly",
        });

        /// <summary>Lã de Iaque (Região 7, Teto do Mundo).</summary>
        public static readonly ArmorSetDefinition Yak = new ArmorSetDefinition("la_de_iaque", YakChestId, YakHelmId, MiddleDropChance, new[]
        {
            "enemy_mountain_brown_bear", "enemy_mountain_wild_yak", "enemy_mountain_irbis", "enemy_mountain_almas",
            "enemy_mountain_giant_marmot", "enemy_mountain_red_billed_chough", "enemy_mountain_bearded_vulture", "enemy_mountain_ice_wolf",
            // Four of the twentieth pass (HighlandOwnBeastRecipes), the Teto's own beasts in the roles it had lent: with 23 species in the
            // region the 8 alone took ~1380 kills to farm the set, at the edge of the budget, and a set takes at most 12.
            "enemy_mountain_dimmed_argali", "enemy_desert_az_bandit", "enemy_mountain_moraine_roller", "enemy_mountain_chute_ushabti",
        });

        /// <summary>Vasos de Bronze (Região 8, Planície de Mingcheng): the "conjunto antigo" of the sheet of map 46.</summary>
        public static readonly ArmorSetDefinition Bronze = new ArmorSetDefinition("vasos_de_bronze", BronzeChestId, BronzeHelmId, MiddleDropChance, new[]
        {
            "enemy_forest_stone_lion", "enemy_forest_lantern_fox", "enemy_forest_terracotta_soldier", "enemy_forest_stone_turtle",
            "enemy_forest_mud_buffalo", "enemy_forest_nian_cub", "enemy_forest_dragon_carp", "enemy_forest_terracotta_horse",
        });

        /// <summary>Ouro dos Kurgans (Região 9, Estepe dos Nartes).</summary>
        public static readonly ArmorSetDefinition Kurgan = new ArmorSetDefinition("ouro_dos_kurgans", KurganChestId, KurganHelmId, MiddleDropChance, new[]
        {
            "enemy_mountain_young_griffin", "enemy_mountain_saiga", "enemy_mountain_bronze_wolf", "enemy_mountain_sleepwalker_giant",
            "enemy_mountain_bark_beetle", "enemy_mountain_ochokochi", "enemy_mountain_felt_swan",
            // Five of the seventeenth pass (LentRoleBeastRecipes), the steppe's own beasts in the roles it had lent: with 19 own species
            // the 7 alone took 1600 kills to farm the set, over the budget, and a set takes at most 12.
            "enemy_desert_golden_stag", "enemy_desert_giant_ibex", "enemy_forest_hill_boar", "enemy_forest_taiga_lynx", "enemy_forest_snow_wolf",
        });

        // ---- Regions 10–12, the high north before the Rio: Rare sets between the middle and the far ones, from the 12 own beasts of
        // each region (Docs/Status_100Mapas.md), at HighDropChance.

        /// <summary>Sino de Vento (Região 10, Picos de Farnkesh).</summary>
        public static readonly ArmorSetDefinition WindBell = new ArmorSetDefinition("sino_de_vento", WindBellChestId, WindBellHelmId, HighDropChance, new[]
        {
            "enemy_forest_smoke_goat", "enemy_forest_golden_bee", "enemy_forest_flute_bird", "enemy_forest_vine_fox",
            "enemy_mountain_alpine_chough", "enemy_mountain_takin", "enemy_mountain_blue_sheep", "enemy_mountain_candle_bat",
            "enemy_mountain_rope_ushabti", "enemy_mountain_pallas_cat", "enemy_mountain_storm_eagle", "enemy_mountain_smoke_harpy",
        });

        /// <summary>Prata da Aurora (Região 11, Mar Gelado).</summary>
        public static readonly ArmorSetDefinition AuroraSilver = new ArmorSetDefinition("prata_da_aurora", AuroraSilverChestId, AuroraSilverHelmId, HighDropChance, new[]
        {
            "enemy_mountain_leopard_seal", "enemy_mountain_smoke_walrus", "enemy_mountain_ice_gull", "enemy_mountain_arctic_fox",
            "enemy_mountain_polar_bear", "enemy_mountain_clay_reindeer", "enemy_mountain_stone_hare", "enemy_mountain_twig_bird",
            "enemy_mountain_burrow_mammoth", "enemy_mountain_sea_wolf", "enemy_mountain_giant_pike", "enemy_mountain_shadow_seal",
        });

        /// <summary>Kurdalægon (Região 12, Arquipélago de Hinokura): the "conjunto antigo" of the sheet of map 69.</summary>
        public static readonly ArmorSetDefinition CelestialIron = new ArmorSetDefinition("kurdalaegon", CelestialIronChestId, CelestialIronHelmId, HighDropChance, new[]
        {
            "enemy_mountain_volcano_crab", "enemy_mountain_lava_lizard", "enemy_mountain_ember_gull", "enemy_mountain_dimmed_shisa",
            "enemy_mountain_tanuki", "enemy_mountain_tibicena", "enemy_mountain_giant_lava_lizard", "enemy_mountain_blade_fish",
            "enemy_mountain_ember_coral", "enemy_mountain_lacquer_ushabti", "enemy_mountain_furnace_ogre", "enemy_mountain_slag_giant",
        });

        // ---- Regions 13–16.

        /// <summary>Cais do Papiro (Região 13, Rio das Doze Horas).</summary>
        public static readonly ArmorSetDefinition Papyrus = new ArmorSetDefinition("cais_do_papiro", PapyrusChestId, PapyrusHelmId, new[]
        {
            "enemy_forest_black_scorpion", "enemy_forest_creeping_dark", "enemy_forest_dark_river_crocodile", "enemy_forest_dimmed_ibis",
            "enemy_forest_mud_hippo", "enemy_forest_reed_snake", "enemy_forest_reed_snare", "enemy_forest_sand_ushabti",
            "enemy_forest_shadow_biter", "enemy_forest_shadow_locust", "enemy_forest_shadow_scarab", "enemy_forest_three_headed_serpent",
        });

        /// <summary>Tijolo Azul (Região 14, Margens da Mistura).</summary>
        public static readonly ArmorSetDefinition BlueBrick = new ArmorSetDefinition("tijolo_azul", BlueBrickChestId, BlueBrickHelmId, new[]
        {
            "enemy_forest_angry_cedar", "enemy_forest_bitumen_locust", "enemy_forest_brick_lion", "enemy_forest_broken_tablet",
            "enemy_forest_garden_shadow", "enemy_forest_glow_mantle", "enemy_forest_late_shadow", "enemy_forest_living_hourglass",
            "enemy_forest_living_tar", "enemy_forest_tar_heron", "enemy_forest_ullikummi_son", "enemy_forest_young_mushussu",
            // The fourteenth pass of beasts (FarStandInBeastRecipes): the roles of the sheets of 81 and 82 that were stand-ins.
            "enemy_forest_diorite_stone", "enemy_forest_shoulder_bird", "enemy_forest_dream_bird",
        });

        /// <summary>Selo dos Artífices (Região 15, Limiar Profundo).</summary>
        public static readonly ArmorSetDefinition Seal = new ArmorSetDefinition("selo_dos_artifices", SealChestId, SealHelmId, new[]
        {
            "enemy_forest_black_eagle", "enemy_forest_copy_shadow", "enemy_forest_dark_fish", "enemy_forest_ember_wolf",
            "enemy_forest_hot_smoke_lion", "enemy_forest_lesser_archon", "enemy_forest_lesser_shadow_dragon", "enemy_forest_mud_octopus",
            "enemy_forest_shadow_lizard", "enemy_forest_smoke_archer", "enemy_forest_smoke_giant", "enemy_forest_wind_harpy",
            // The fourteenth pass: the Sombra Pequena and the Morcego da Treva of the fortress (83). The Chama Faminta (70, 75, 85) burns in three
            // regions and belongs to none of the sets.
            "enemy_forest_little_shadow", "enemy_forest_dark_bat",
        });

        /// <summary>Cais de Luz (Região 16, Reinos do Céu).</summary>
        public static readonly ArmorSetDefinition Light = new ArmorSetDefinition("cais_de_luz", LightChestId, LightHelmId, new[]
        {
            "enemy_forest_az_thief", "enemy_forest_balloon_giant", "enemy_forest_chained_archon", "enemy_forest_cracked_jar",
            "enemy_forest_dark_stowaway", "enemy_forest_dimmed_cloud_bird", "enemy_forest_dimmed_light_fish", "enemy_forest_dimmed_light_gull",
            "enemy_forest_hold_shadow", "enemy_forest_living_rope", "enemy_forest_sky_stone", "enemy_forest_toy_archon",
            // The fourteenth pass: the Lanterna Fugitiva (92) and the Vento de Órbita (95, 99, 100).
            "enemy_forest_runaway_lantern", "enemy_forest_orbit_wind",
            // The eighteenth pass (SkyOwnBeastRecipes): the Faísca Teimosa (93), so the ten new species of the Sky do not dilute the farm past the budget.
            "enemy_forest_stubborn_spark",
        });

        // ---- Regions 13–16, the second set of each: the old sets of the fichas (74, 82, the Primal Man's armour of the Cinco Covas, 98), from
        // the 3 own beasts each region's first set left out, at AncientDropChance.

        /// <summary>Escamas de Mehen (Região 13): the coiled serpent of map 74, who guards the barge from Apep.</summary>
        public static readonly ArmorSetDefinition MehenScale = new ArmorSetDefinition("escamas_de_mehen", MehenScaleChestId, MehenScaleHelmId, AncientDropChance, new[]
        {
            "enemy_forest_child_of_apep", "enemy_forest_quicksand", "enemy_forest_thief_goose",
        });

        /// <summary>Gigantes (Região 14): the "conjunto antigo" of the sheet of map 82, of the dream tablets of Hahya and Ohya.</summary>
        public static readonly ArmorSetDefinition Giants = new ArmorSetDefinition("gigantes", GiantsChestId, GiantsHelmId, AncientDropChance, new[]
        {
            "enemy_forest_scorpion_man", "enemy_forest_cedar_lynx", "enemy_forest_resin_bee",
        });

        /// <summary>Cinco Luzes (Região 15): the armour of the five Light elements the Primal Man wore down into the Land of Darkness.</summary>
        public static readonly ArmorSetDefinition FiveLights = new ArmorSetDefinition("cinco_luzes", FiveLightsChestId, FiveLightsHelmId, AncientDropChance, new[]
        {
            "enemy_forest_ash_bull", "enemy_forest_blade_bat", "enemy_forest_great_eel",
        });

        /// <summary>Coluna da Glória (Região 16): the Column of Glory of map 98, the road of the freed light.</summary>
        public static readonly ArmorSetDefinition GloryColumn = new ArmorSetDefinition("coluna_da_gloria", GloryColumnChestId, GloryColumnHelmId, AncientDropChance, new[]
        {
            "enemy_cave_dark_drop", "enemy_desert_confused_spark", "enemy_forest_smoke_rope",
            // The eighteenth pass: the Última Sombra and the Vento do Alto of the Pés da Coluna (98), the set's own map.
            "enemy_forest_last_shadow", "enemy_mountain_high_wind",
        });

        /// <summary>The four sets of Regions 1–4, in region order.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> Early = new[] { Garden, RedLeather, Drystone, Wick };

        /// <summary>The five sets of Regions 5–9, in region order.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> Middle = new[] { Coral, Lotus, Yak, Bronze, Kurgan };

        /// <summary>The three sets of Regions 10–12, in region order.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> High = new[] { WindBell, AuroraSilver, CelestialIron };

        /// <summary>The four sets of Regions 13–16, in region order.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> Far = new[] { Papyrus, BlueBrick, Seal, Light };

        /// <summary>The second sets of Regions 13–16 (the old sets of the fichas), in region order.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> Ancient = new[] { MehenScale, Giants, FiveLights, GloryColumn };

        /// <summary>Every set, in region order; the second sets of Regions 13–16 last.</summary>
        public static readonly IReadOnlyList<ArmorSetDefinition> All = new[]
        {
            Garden, RedLeather, Drystone, Wick, Coral, Lotus, Yak, Bronze, Kurgan, WindBell, AuroraSilver, CelestialIron, Papyrus, BlueBrick, Seal, Light,
            MehenScale, Giants, FiveLights, GloryColumn,
        };

        // "Orvalho do Jardim" (Jardim de Mihrshah): a kill may heal the hero a little.
        public const double GardenDewChance = 0.15;
        public const float GardenDewHealFraction = 0.03f;

        // "Areia que Queima" (Couro Rubro): a hit taken may burn back at the attacker.
        public const double BurningSandChance = 0.10;
        public const float BurningSandFraction = 0.20f;

        // "Rajada do Kaggen" (Pedra Seca): a critical hit may gust into the enemies right next to the target.
        public const double KaggenGustChance = 0.10;
        public const float KaggenGustDamageFraction = 0.20f;
        public const float KaggenGustRadius = 3f;
        public const int KaggenGustMaxTargets = 2;

        // "Pavio Aceso" (Pavio do Limiar): a kill may give a little mana back.
        public const double LitWickChance = 0.15;
        public const float LitWickManaFraction = 0.04f;

        // "Badalada das Ermidas" (Sino de Vento): a critical hit may toll into the enemies around the target.
        public const double HermitageTollChance = 0.12;
        public const float HermitageTollDamageFraction = 0.35f;
        public const float HermitageTollRadius = 4.5f;
        public const int HermitageTollMaxTargets = 3;

        // "Calor da Lanterna" (Prata da Aurora): a kill may heal the hero.
        public const double LanternWarmthChance = 0.22;
        public const float LanternWarmthHealFraction = 0.05f;

        // "Brasa da Bigorna" (Kurdalægon): a hit taken may burn back at the attacker.
        public const double AnvilEmberChance = 0.20;
        public const float AnvilEmberFraction = 0.30f;

        // "Coral que Fecha" (Coral de Kilwa): a hit taken may close up, healing part of it back.
        public const double ClosingCoralChance = 0.15;
        public const float ClosingCoralHealFraction = 0.30f;

        // "Seiva do Lótus" (Seda de Lótus): a kill may give mana back.
        public const double LotusSapChance = 0.20;
        public const float LotusSapManaFraction = 0.05f;

        // "Fôlego do Pamir" (Lã de Iaque): a critical hit may heal the hero.
        public const double PamirBreathChance = 0.20;
        public const float PamirBreathHealFraction = 0.03f;

        // "Gongo de Bronze" (Vasos de Bronze): a hit taken may ring out at the enemies around the hero.
        public const double BronzeGongChance = 0.10;
        public const float BronzeGongDamageFraction = 0.20f;
        public const float BronzeGongRadius = 4f;
        public const int BronzeGongMaxTargets = 3;

        // "Galope dos Nartes" (Ouro dos Kurgans): a critical hit may run on into the enemies nearby.
        public const double NartGallopChance = 0.10;
        public const float NartGallopDamageFraction = 0.30f;
        public const float NartGallopRadius = 4f;
        public const int NartGallopMaxTargets = 3;

        // "Hora da Barca" (Cais do Papiro): a kill may heal the hero.
        public const double BargeHourChance = 0.25;
        public const float BargeHourHealFraction = 0.05f;

        // "Reflexo Vidrado" (Tijolo Azul): a hit taken may bounce back at the attacker.
        public const double GlazedReflectChance = 0.20;
        public const float GlazedReflectFraction = 0.35f;

        // "Selo Reaberto" (Selo dos Artífices): a critical hit may give mana back.
        public const double ReopenedSealChance = 0.25;
        public const float ReopenedSealManaFraction = 0.08f;

        // "Raio da Coluna" (Cais de Luz): a critical hit may strike the enemies nearby.
        public const double ColumnRayChance = 0.15;
        public const float ColumnRayDamageFraction = 0.40f;
        public const float ColumnRayRadius = 5f;
        public const int ColumnRayMaxTargets = 4;

        // "Anel de Mehen" (Escamas de Mehen): a hit taken may coil up, healing part of it back.
        public const double MehenCoilChance = 0.18;
        public const float MehenCoilHealFraction = 0.35f;

        // "Tremor dos Gigantes" (Gigantes): a hit taken may shake the ground under the enemies around the hero.
        public const double GiantsTremorChance = 0.15;
        public const float GiantsTremorDamageFraction = 0.30f;
        public const float GiantsTremorRadius = 4.5f;
        public const int GiantsTremorMaxTargets = 4;

        // "Luz que Volta" (Cinco Luzes): a kill may give back both health and mana.
        public const double ReturningLightChance = 0.20;
        public const float ReturningLightHealFraction = 0.04f;
        public const float ReturningLightManaFraction = 0.04f;

        // "Subida da Coluna" (Coluna da Glória): a critical hit may heal the hero.
        public const double ColumnRiseChance = 0.20;
        public const float ColumnRiseHealFraction = 0.05f;

        /// <summary>The set whose own beasts include <paramref name="enemyId"/> (the asset name), or null.</summary>
        public static ArmorSetDefinition SetOfBeast(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return null;
            }

            foreach (ArmorSetDefinition set in All)
            {
                foreach (string beast in set.Beasts)
                {
                    if (beast == enemyId)
                    {
                        return set;
                    }
                }
            }

            return null;
        }

        /// <summary>The item id one kill of <paramref name="enemyId"/> drops (<paramref name="roll"/>, <paramref name="pieceRoll"/> in [0,1)), or null.</summary>
        public static string RollDrop(string enemyId, double roll, double pieceRoll)
        {
            ArmorSetDefinition set = SetOfBeast(enemyId);
            if (set == null || roll >= set.DropChance)
            {
                return null;
            }

            return pieceRoll < 0.5 ? set.ChestItemId : set.HelmItemId;
        }

        /// <summary>Distinct pieces of <paramref name="set"/> among <paramref name="equippedItemIds"/>.</summary>
        public static int CountPieces(ArmorSetDefinition set, IEnumerable<string> equippedItemIds)
        {
            if (set == null || equippedItemIds == null)
            {
                return 0;
            }

            bool chest = false;
            bool helm = false;
            foreach (string id in equippedItemIds)
            {
                chest |= id == set.ChestItemId;
                helm |= id == set.HelmItemId;
            }

            return (chest ? 1 : 0) + (helm ? 1 : 0);
        }

        /// <summary>The set completely worn among <paramref name="equippedItemIds"/>, or null (two slots: at most one can be complete).</summary>
        public static ArmorSetDefinition CompleteSet(IEnumerable<string> equippedItemIds)
        {
            if (equippedItemIds == null)
            {
                return null;
            }

            var ids = new List<string>(equippedItemIds);
            foreach (ArmorSetDefinition set in All)
            {
                if (CountPieces(set, ids) >= PiecesForBonus)
                {
                    return set;
                }
            }

            return null;
        }

        public static bool BargeHourProcs(ArmorSetDefinition worn, double roll) => worn == Papyrus && roll < BargeHourChance;

        public static float BargeHourHeal(float maxHealth) => maxHealth > 0f ? maxHealth * BargeHourHealFraction : 0f;

        public static bool GlazedReflectProcs(ArmorSetDefinition worn, double roll) => worn == BlueBrick && roll < GlazedReflectChance;

        /// <summary>Damage bounced back: 35% of the hit taken (never below 1 for a positive hit).</summary>
        public static float GlazedReflectDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * GlazedReflectFraction) : 0f;

        public static bool ReopenedSealProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Seal && roll < ReopenedSealChance;

        public static float ReopenedSealMana(float maxMana) => maxMana > 0f ? maxMana * ReopenedSealManaFraction : 0f;

        public static bool ColumnRayProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Light && roll < ColumnRayChance;

        /// <summary>Damage of each ray: 40% of the critical hit (never below 1 for a positive hit).</summary>
        public static float ColumnRayDamage(float criticalDamage) => criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * ColumnRayDamageFraction) : 0f;

        public static bool ClosingCoralProcs(ArmorSetDefinition worn, double roll) => worn == Coral && roll < ClosingCoralChance;

        /// <summary>Health given back: 30% of the hit taken.</summary>
        public static float ClosingCoralHeal(float damageTaken) => damageTaken > 0f ? damageTaken * ClosingCoralHealFraction : 0f;

        public static bool LotusSapProcs(ArmorSetDefinition worn, double roll) => worn == Lotus && roll < LotusSapChance;

        public static float LotusSapMana(float maxMana) => maxMana > 0f ? maxMana * LotusSapManaFraction : 0f;

        public static bool PamirBreathProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Yak && roll < PamirBreathChance;

        public static float PamirBreathHeal(float maxHealth) => maxHealth > 0f ? maxHealth * PamirBreathHealFraction : 0f;

        public static bool BronzeGongProcs(ArmorSetDefinition worn, double roll) => worn == Bronze && roll < BronzeGongChance;

        /// <summary>Damage of the gong to each enemy around: 20% of the hit taken (never below 1 for a positive hit).</summary>
        public static float BronzeGongDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * BronzeGongDamageFraction) : 0f;

        public static bool NartGallopProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Kurgan && roll < NartGallopChance;

        /// <summary>Damage of the gallop to each enemy nearby: 30% of the critical hit (never below 1 for a positive hit).</summary>
        public static float NartGallopDamage(float criticalDamage) => criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * NartGallopDamageFraction) : 0f;

        public static bool GardenDewProcs(ArmorSetDefinition worn, double roll) => worn == Garden && roll < GardenDewChance;

        public static float GardenDewHeal(float maxHealth) => maxHealth > 0f ? maxHealth * GardenDewHealFraction : 0f;

        public static bool BurningSandProcs(ArmorSetDefinition worn, double roll) => worn == RedLeather && roll < BurningSandChance;

        /// <summary>Damage burnt back: 20% of the hit taken (never below 1 for a positive hit).</summary>
        public static float BurningSandDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * BurningSandFraction) : 0f;

        public static bool KaggenGustProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == Drystone && roll < KaggenGustChance;

        /// <summary>Damage of the gust to each enemy next to the target: 20% of the critical hit (never below 1 for a positive hit).</summary>
        public static float KaggenGustDamage(float criticalDamage) => criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * KaggenGustDamageFraction) : 0f;

        public static bool LitWickProcs(ArmorSetDefinition worn, double roll) => worn == Wick && roll < LitWickChance;

        public static float LitWickMana(float maxMana) => maxMana > 0f ? maxMana * LitWickManaFraction : 0f;

        public static bool HermitageTollProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == WindBell && roll < HermitageTollChance;

        /// <summary>Damage of the toll to each enemy around the target: 35% of the critical hit (never below 1 for a positive hit).</summary>
        public static float HermitageTollDamage(float criticalDamage) => criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * HermitageTollDamageFraction) : 0f;

        public static bool LanternWarmthProcs(ArmorSetDefinition worn, double roll) => worn == AuroraSilver && roll < LanternWarmthChance;

        public static float LanternWarmthHeal(float maxHealth) => maxHealth > 0f ? maxHealth * LanternWarmthHealFraction : 0f;

        public static bool AnvilEmberProcs(ArmorSetDefinition worn, double roll) => worn == CelestialIron && roll < AnvilEmberChance;

        /// <summary>Damage burnt back: 30% of the hit taken (never below 1 for a positive hit).</summary>
        public static float AnvilEmberDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * AnvilEmberFraction) : 0f;

        public static bool MehenCoilProcs(ArmorSetDefinition worn, double roll) => worn == MehenScale && roll < MehenCoilChance;

        /// <summary>Health given back: 35% of the hit taken.</summary>
        public static float MehenCoilHeal(float damageTaken) => damageTaken > 0f ? damageTaken * MehenCoilHealFraction : 0f;

        public static bool GiantsTremorProcs(ArmorSetDefinition worn, double roll) => worn == Giants && roll < GiantsTremorChance;

        /// <summary>Damage of the tremor to each enemy around: 30% of the hit taken (never below 1 for a positive hit).</summary>
        public static float GiantsTremorDamage(float damageTaken) => damageTaken > 0f ? Mathf.Max(1f, damageTaken * GiantsTremorDamageFraction) : 0f;

        public static bool ReturningLightProcs(ArmorSetDefinition worn, double roll) => worn == FiveLights && roll < ReturningLightChance;

        public static float ReturningLightHeal(float maxHealth) => maxHealth > 0f ? maxHealth * ReturningLightHealFraction : 0f;

        public static float ReturningLightMana(float maxMana) => maxMana > 0f ? maxMana * ReturningLightManaFraction : 0f;

        public static bool ColumnRiseProcs(ArmorSetDefinition worn, bool critical, double roll) => critical && worn == GloryColumn && roll < ColumnRiseChance;

        public static float ColumnRiseHeal(float maxHealth) => maxHealth > 0f ? maxHealth * ColumnRiseHealFraction : 0f;
    }
}
