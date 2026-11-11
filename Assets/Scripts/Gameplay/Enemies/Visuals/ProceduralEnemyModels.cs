using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>Part names of the procedural enemies (what <see cref="ProceduralEnemyRig"/> and the procedural animator look for).</summary>
    public static class ProceduralEnemyParts
    {
        public const string Body = "Body";
        public const string Head = "Head";
        public const string ArmLeft = "Arm_L";
        public const string ArmRight = "Arm_R";
        public const string FistLeft = "Fist_L";
        public const string FistRight = "Fist_R";
        /// <summary>Legs contain "Leg", so <c>EnemyProceduralAnimator.IsLegName</c> already finds them.</summary>
        public const string LegLeft = "Leg_L";
        public const string LegRight = "Leg_R";
        /// <summary>
        /// Jointed legs (<c>ProceduralLegLocomotion</c> convention): hip pivot <c>Leg_&lt;id&gt;</c> → knee pivot
        /// <c>Shin_&lt;id&gt;</c> → ankle/foot pivot <c>Foot_&lt;id&gt;</c>. Bipeds use the ids L/R, quadrupeds
        /// FL/FR/HL/HR, arthropods L0..L3/R0..R3 (front to back).
        /// </summary>
        public const string LegPrefix = "Leg_";
        public const string ShinPrefix = "Shin_";
        public const string FootPrefix = "Foot_";
        public const string ShinLeft = "Shin_L";
        public const string ShinRight = "Shin_R";
        public const string FootLeft = "Foot_L";
        public const string FootRight = "Foot_R";
        /// <summary>Follow-through parts (springs on turns and footfalls): tail chain <c>Tail</c> → <c>Tail_1</c>.. → <c>Stinger</c>, spider abdomen, scorpion claws.</summary>
        public const string Tail = "Tail";
        public const string TailSegmentPrefix = "Tail_";
        public const string Stinger = "Stinger";
        public const string Abdomen = "Abdomen";
        public const string ClawLeft = "Claw_L";
        public const string ClawRight = "Claw_R";
        public const string WingLeft = "Wing_L";
        public const string WingRight = "Wing_R";
        /// <summary>Bat swarm: the pivot every bat orbits (bats are "Bat_0".."Bat_n" under it).</summary>
        public const string Swarm = "Swarm";
        public const string BatPrefix = "Bat_";
        public const string Cape = "Cape";
        public const string Weapon = "Weapon";
        public const string EarLeft = "Ear_L";
        public const string EarRight = "Ear_R";
        /// <summary>Ruin guardian: the ring of floating rune tablets.</summary>
        public const string Runes = "Runes";
        /// <summary>Ancient dweller: the pile of stolen light hugged against its chest.</summary>
        public const string StolenLight = "StolenLight";
        /// <summary>Werewolf: chest/shoulders/head pivot at the hips; pitches up when it rears to strike (front legs ride it).</summary>
        public const string Torso = "Torso";
        /// <summary>Lower jaw / lower beak (werewolf, potoo): opens during the attack windup.</summary>
        public const string Jaw = "Jaw";
        /// <summary>Firefly swarm: the glowing units are "Firefly_0".."Firefly_n" under <see cref="Swarm"/>, each with a "Lantern".</summary>
        public const string FireflyPrefix = "Firefly_";
        public const string Lantern = "Lantern";
        /// <summary>Potoo: the dead branch it ambushes from (it takes it along, of course).</summary>
        public const string Branch = "Branch";
        /// <summary>Copper skeleton warrior: the bone shield on the left forearm (rides <see cref="ArmLeft"/>, which its guard behaviour raises).</summary>
        public const string Shield = "Shield";
        /// <summary>Floating skull: the trail of soul-fire wisps under the jaw (flickers).</summary>
        public const string Wisp = "Wisp";
        /// <summary>Bone sorcerer: the violet orb held by the claws on top of its staff (pulses, swells before each cast).</summary>
        public const string Orb = "Orb";
        /// <summary>Bone colossus: the soul-fire heart inside the stacked skulls (blazes in the last phase).</summary>
        public const string Core = "Core";
        /// <summary>Bone colossus: the two halves of the rib cage over its chest, hinged at the sides (swing open in the last phase).</summary>
        public const string RibsLeft = "Ribs_L";
        public const string RibsRight = "Ribs_R";
        /// <summary>Bone colossus: the ring of loose skulls ("HaloSkull_0".."HaloSkull_5") folded away until the skull-throwing phase.</summary>
        public const string Halo = "Halo";
        public const string HaloSkullPrefix = "HaloSkull_";
        /// <summary>Ill-wind mist: the fog puffs ("Puff_0".."Puff_n") circling under <see cref="Swarm"/>.</summary>
        public const string PuffPrefix = "Puff_";
    }

    /// <summary>
    /// Credit-free, stylized low-poly models for the enemies that have no Meshy model (bat swarm,
    /// stone golem, warlord, ruin guardian, ancient dweller) and for the animals whose Meshy model was a
    /// single static mesh (wolf, alpha wolf, spider, scorpion: replaced so their legs can step),
    /// generated in code from separate named parts so they can be animated without a skeleton. Each kind is built once and cached (meshes and
    /// the palette material are shared by every instance); <see cref="EnemyVisualSwapper"/> instantiates
    /// it when an <see cref="EnemyDefinition"/> has no <see cref="EnemyDefinition.VisualPrefab"/>, and
    /// the editor menu "Rapadura/Enemies/Build Procedural Enemy Models" bakes the same blueprints to
    /// prefab/mesh assets. Models face +Z with the pivot at the feet (y = 0), in metres.
    /// </summary>
    public static class ProceduralEnemyModels
    {
        /// <summary>Mobile triangle budgets.</summary>
        public const int CommonTriangleBudget = 3000;
        public const int BossTriangleBudget = 8000;

        private static readonly Dictionary<ProceduralEnemyKind, ProceduralEnemyBlueprint> Cache =
            new Dictionary<ProceduralEnemyKind, ProceduralEnemyBlueprint>();

        private static Material _sharedMaterial;

        /// <summary>Enemy asset id (EnemyDefinition asset name) → model kind.</summary>
        public static readonly IReadOnlyDictionary<string, ProceduralEnemyKind> KindsById = new Dictionary<string, ProceduralEnemyKind>
        {
            { "enemy_cave_bat_swarm", ProceduralEnemyKind.BatSwarm },
            { "enemy_mountain_stone_golem", ProceduralEnemyKind.StoneGolem },
            { "enemy_mountain_warlord", ProceduralEnemyKind.Warlord },
            { "enemy_desert_ruin_guardian", ProceduralEnemyKind.RuinGuardian },
            { "enemy_cave_dweller", ProceduralEnemyKind.AncientDweller },
            { "enemy_forest_wolf", ProceduralEnemyKind.Wolf },
            { "enemy_forest_alpha_wolf", ProceduralEnemyKind.AlphaWolf },
            { "enemy_forest_spider", ProceduralEnemyKind.Spider },
            { "enemy_desert_scorpion", ProceduralEnemyKind.Scorpion },
            { "enemy_forest_werewolf", ProceduralEnemyKind.Werewolf },
            { "enemy_cave_vampire_bat", ProceduralEnemyKind.VampireBat },
            { "enemy_forest_shroud_owl", ProceduralEnemyKind.ShroudOwl },
            { "enemy_desert_firefly_swarm", ProceduralEnemyKind.FireflySwarm },
            { "enemy_forest_peccary", ProceduralEnemyKind.Peccary },
            { "enemy_mountain_potoo", ProceduralEnemyKind.Potoo },
            { "enemy_desert_ember_vulture", ProceduralEnemyKind.EmberVulture },
            { "enemy_desert_broken_lantern", ProceduralEnemyKind.BrokenLantern },
            { "enemy_mountain_crested_hawk", ProceduralEnemyKind.CrestedHawk },
            { "enemy_cave_lantern_fish", ProceduralEnemyKind.LanternFish },
            // The beasts the map sheets asked for (Docs/Mapas100.md 07–25), formerly played by stand-ins.
            { "enemy_desert_jackal", ProceduralEnemyKind.Jackal },
            { "enemy_desert_sand_snake", ProceduralEnemyKind.SandSnake },
            { "enemy_desert_crowned_orc", ProceduralEnemyKind.CrownedOrc },
            { "enemy_mountain_rolling_stone", ProceduralEnemyKind.RollingStone },
            { "enemy_mountain_ill_wind_mist", ProceduralEnemyKind.IllWindMist },
            { "enemy_mountain_baboon", ProceduralEnemyKind.Baboon },
            { "enemy_cave_ooze", ProceduralEnemyKind.CaveOoze },
            { "enemy_cave_dark_crow", ProceduralEnemyKind.DarkCrow },
            { "enemy_forest_thief_gull", ProceduralEnemyKind.ThiefGull },
            { "enemy_forest_coconut_crab", ProceduralEnemyKind.CoconutCrab },
            // The beasts of the late continuous worlds (Docs/Mapas100.md 25–40), formerly played by stand-ins.
            { "enemy_forest_mangrove_crocodile", ProceduralEnemyKind.MangroveCrocodile },
            { "enemy_forest_grass_lion", ProceduralEnemyKind.GrassLion },
            { "enemy_forest_dust_rhino", ProceduralEnemyKind.DustRhino },
            { "enemy_forest_thief_monkey", ProceduralEnemyKind.ThiefMonkey },
            { "enemy_forest_thief_otter", ProceduralEnemyKind.ThiefOtter },
            { "enemy_forest_dimmed_python", ProceduralEnemyKind.Python },
            { "enemy_forest_dimmed_troll", ProceduralEnemyKind.DimmedTroll },
            { "enemy_mountain_brown_bear", ProceduralEnemyKind.BrownBear },
            { "enemy_mountain_wild_yak", ProceduralEnemyKind.WildYak },
            { "enemy_mountain_irbis", ProceduralEnemyKind.Irbis },
            { "enemy_mountain_almas", ProceduralEnemyKind.Almas },
            { "enemy_mountain_giant_marmot", ProceduralEnemyKind.GiantMarmot },
            // The beasts of the Planície de Mingcheng and the Estepe dos Nartes (Docs/Mapas100.md 41–52), formerly played by stand-ins.
            { "enemy_forest_stone_lion", ProceduralEnemyKind.StoneLion },
            { "enemy_forest_lantern_fox", ProceduralEnemyKind.LanternFox },
            { "enemy_forest_terracotta_soldier", ProceduralEnemyKind.TerracottaSoldier },
            { "enemy_forest_stone_turtle", ProceduralEnemyKind.StoneTurtle },
            { "enemy_forest_mud_buffalo", ProceduralEnemyKind.MudBuffalo },
            { "enemy_forest_nian_cub", ProceduralEnemyKind.NianCub },
            { "enemy_mountain_young_griffin", ProceduralEnemyKind.YoungGriffin },
            { "enemy_mountain_saiga", ProceduralEnemyKind.Saiga },
            { "enemy_mountain_bronze_wolf", ProceduralEnemyKind.BronzeWolf },
            { "enemy_mountain_sleepwalker_giant", ProceduralEnemyKind.SleepwalkerGiant },
            { "enemy_mountain_bark_beetle", ProceduralEnemyKind.BarkBeetle },
            { "enemy_mountain_ochokochi", ProceduralEnemyKind.Ochokochi },
            // The beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58), formerly played by stand-ins.
            { "enemy_forest_smoke_goat", ProceduralEnemyKind.SmokeGoat },
            { "enemy_forest_golden_bee", ProceduralEnemyKind.GoldenBee },
            { "enemy_forest_flute_bird", ProceduralEnemyKind.FluteBird },
            { "enemy_forest_vine_fox", ProceduralEnemyKind.VineFox },
            { "enemy_mountain_alpine_chough", ProceduralEnemyKind.AlpineChough },
            { "enemy_mountain_takin", ProceduralEnemyKind.Takin },
            { "enemy_mountain_blue_sheep", ProceduralEnemyKind.BlueSheep },
            { "enemy_mountain_candle_bat", ProceduralEnemyKind.CandleBat },
            { "enemy_mountain_rope_ushabti", ProceduralEnemyKind.RopeUshabti },
            { "enemy_mountain_pallas_cat", ProceduralEnemyKind.PallasCat },
            { "enemy_mountain_storm_eagle", ProceduralEnemyKind.StormEagle },
            { "enemy_mountain_smoke_harpy", ProceduralEnemyKind.SmokeHarpy },
            // The beasts of the Mar Gelado (Docs/Mapas100.md 59–64), formerly played by stand-ins.
            { "enemy_mountain_leopard_seal", ProceduralEnemyKind.LeopardSeal },
            { "enemy_mountain_smoke_walrus", ProceduralEnemyKind.SmokeWalrus },
            { "enemy_mountain_ice_gull", ProceduralEnemyKind.IceGull },
            { "enemy_mountain_arctic_fox", ProceduralEnemyKind.ArcticFox },
            { "enemy_mountain_polar_bear", ProceduralEnemyKind.PolarBear },
            { "enemy_mountain_clay_reindeer", ProceduralEnemyKind.ClayReindeer },
            { "enemy_mountain_stone_hare", ProceduralEnemyKind.StoneHare },
            { "enemy_mountain_twig_bird", ProceduralEnemyKind.TwigBird },
            { "enemy_mountain_burrow_mammoth", ProceduralEnemyKind.BurrowMammoth },
            { "enemy_mountain_sea_wolf", ProceduralEnemyKind.SeaWolf },
            { "enemy_mountain_giant_pike", ProceduralEnemyKind.GiantPike },
            { "enemy_mountain_shadow_seal", ProceduralEnemyKind.ShadowSeal },
            // The beasts of the Arquipélago de Hinokura (Docs/Mapas100.md 65–70), formerly played by stand-ins.
            { "enemy_mountain_volcano_crab", ProceduralEnemyKind.VolcanoCrab },
            { "enemy_mountain_lava_lizard", ProceduralEnemyKind.LavaLizard },
            { "enemy_mountain_ember_gull", ProceduralEnemyKind.EmberGull },
            { "enemy_mountain_dimmed_shisa", ProceduralEnemyKind.DimmedShisa },
            { "enemy_mountain_tanuki", ProceduralEnemyKind.Tanuki },
            { "enemy_mountain_tibicena", ProceduralEnemyKind.Tibicena },
            { "enemy_mountain_giant_lava_lizard", ProceduralEnemyKind.GiantLavaLizard },
            { "enemy_mountain_blade_fish", ProceduralEnemyKind.BladeFish },
            { "enemy_mountain_ember_coral", ProceduralEnemyKind.EmberCoral },
            { "enemy_mountain_lacquer_ushabti", ProceduralEnemyKind.LacquerUshabti },
            { "enemy_mountain_furnace_ogre", ProceduralEnemyKind.FurnaceOgre },
            { "enemy_mountain_slag_giant", ProceduralEnemyKind.SlagGiant },
            // The beasts of the Rio das Doze Horas (Docs/Mapas100.md 71–74), formerly played by stand-ins.
            { "enemy_forest_dark_river_crocodile", ProceduralEnemyKind.DarkRiverCrocodile },
            { "enemy_forest_dimmed_ibis", ProceduralEnemyKind.DimmedIbis },
            { "enemy_forest_shadow_scarab", ProceduralEnemyKind.ShadowScarab },
            { "enemy_forest_reed_snare", ProceduralEnemyKind.ReedSnare },
            { "enemy_forest_shadow_locust", ProceduralEnemyKind.ShadowLocust },
            { "enemy_forest_mud_hippo", ProceduralEnemyKind.MudHippo },
            { "enemy_forest_reed_snake", ProceduralEnemyKind.ReedSnake },
            { "enemy_forest_three_headed_serpent", ProceduralEnemyKind.ThreeHeadedSerpent },
            { "enemy_forest_black_scorpion", ProceduralEnemyKind.BlackScorpion },
            { "enemy_forest_sand_ushabti", ProceduralEnemyKind.SandUshabti },
            { "enemy_forest_shadow_biter", ProceduralEnemyKind.ShadowBiter },
            { "enemy_forest_creeping_dark", ProceduralEnemyKind.CreepingDark },
            // The beasts of the Margens da Mistura (Docs/Mapas100.md 77–82), formerly played by stand-ins.
            { "enemy_forest_brick_lion", ProceduralEnemyKind.BrickLion },
            { "enemy_forest_young_mushussu", ProceduralEnemyKind.YoungMushussu },
            { "enemy_forest_bitumen_locust", ProceduralEnemyKind.BitumenLocust },
            { "enemy_forest_living_hourglass", ProceduralEnemyKind.LivingHourglass },
            { "enemy_forest_late_shadow", ProceduralEnemyKind.LateShadow },
            { "enemy_forest_living_tar", ProceduralEnemyKind.LivingTar },
            { "enemy_forest_tar_heron", ProceduralEnemyKind.TarHeron },
            { "enemy_forest_angry_cedar", ProceduralEnemyKind.AngryCedar },
            { "enemy_forest_glow_mantle", ProceduralEnemyKind.GlowMantle },
            { "enemy_forest_ullikummi_son", ProceduralEnemyKind.UllikummiSon },
            { "enemy_forest_broken_tablet", ProceduralEnemyKind.BrokenTablet },
            { "enemy_forest_garden_shadow", ProceduralEnemyKind.GardenShadow },
            // The beasts of the Limiar Profundo (Docs/Mapas100.md 84–90), formerly played by stand-ins.
            { "enemy_forest_copy_shadow", ProceduralEnemyKind.CopyShadow },
            { "enemy_forest_smoke_giant", ProceduralEnemyKind.SmokeGiant },
            { "enemy_forest_smoke_archer", ProceduralEnemyKind.SmokeArcher },
            { "enemy_forest_ember_wolf", ProceduralEnemyKind.EmberWolf },
            { "enemy_forest_hot_smoke_lion", ProceduralEnemyKind.HotSmokeLion },
            { "enemy_forest_black_eagle", ProceduralEnemyKind.BlackEagle },
            { "enemy_forest_wind_harpy", ProceduralEnemyKind.WindHarpy },
            { "enemy_forest_mud_octopus", ProceduralEnemyKind.MudOctopus },
            { "enemy_forest_dark_fish", ProceduralEnemyKind.DarkFish },
            { "enemy_forest_shadow_lizard", ProceduralEnemyKind.ShadowLizard },
            { "enemy_forest_lesser_shadow_dragon", ProceduralEnemyKind.LesserShadowDragon },
            { "enemy_forest_lesser_archon", ProceduralEnemyKind.LesserArchon },
            // The beasts of the Reinos do Céu (Docs/Mapas100.md 91–99), formerly played by stand-ins.
            { "enemy_forest_dimmed_cloud_bird", ProceduralEnemyKind.DimmedCloudBird },
            { "enemy_forest_dimmed_light_fish", ProceduralEnemyKind.DimmedLightFish },
            { "enemy_forest_dark_stowaway", ProceduralEnemyKind.DarkStowaway },
            { "enemy_forest_hold_shadow", ProceduralEnemyKind.HoldShadow },
            { "enemy_forest_living_rope", ProceduralEnemyKind.LivingRope },
            { "enemy_forest_toy_archon", ProceduralEnemyKind.ToyArchon },
            { "enemy_forest_cracked_jar", ProceduralEnemyKind.CrackedJar },
            { "enemy_forest_chained_archon", ProceduralEnemyKind.ChainedArchon },
            { "enemy_forest_balloon_giant", ProceduralEnemyKind.BalloonGiant },
            { "enemy_forest_az_thief", ProceduralEnemyKind.AzThief },
            { "enemy_forest_dimmed_light_gull", ProceduralEnemyKind.DimmedLightGull },
            { "enemy_forest_sky_stone", ProceduralEnemyKind.SkyStone },
            { "enemy_mountain_devouring_salamander", ProceduralEnemyKind.DevouringSalamander },
            { "enemy_mountain_volcanic_bomb", ProceduralEnemyKind.VolcanicBomb },
            { "enemy_forest_thief_goose", ProceduralEnemyKind.ThiefGoose },
            { "enemy_forest_child_of_apep", ProceduralEnemyKind.ChildOfApep },
            { "enemy_forest_quicksand", ProceduralEnemyKind.Quicksand },
            { "enemy_forest_scorpion_man", ProceduralEnemyKind.ScorpionMan },
            { "enemy_forest_cedar_lynx", ProceduralEnemyKind.CedarLynx },
            { "enemy_forest_resin_bee", ProceduralEnemyKind.ResinBee },
            { "enemy_forest_blade_bat", ProceduralEnemyKind.BladeBat },
            { "enemy_forest_great_eel", ProceduralEnemyKind.GreatEel },
            { "enemy_forest_ash_bull", ProceduralEnemyKind.AshBull },
            { "enemy_forest_smoke_rope", ProceduralEnemyKind.SmokeRope },
            // The world bosses of Regions 10–12 (Docs/Mapas100.md 58, 64, 70), at their arenas.
            { RegionBossRules.WindKingId, ProceduralEnemyKind.WindKing },
            { RegionBossRules.OldKulId, ProceduralEnemyKind.OldKul },
            { RegionBossRules.EmberGluttonId, ProceduralEnemyKind.EmberGlutton },
            { RegionBossRules.ApepId, ProceduralEnemyKind.Apep },
            { RegionBossRules.DreamOfDestructionId, ProceduralEnemyKind.DreamOfDestruction },
            { RegionBossRules.GreatDarkId, ProceduralEnemyKind.GreatDark },
            { RegionBossRules.AzCaptainId, ProceduralEnemyKind.AzCaptain },
            // The world bosses of Regions 5–9 (Docs/Mapas100.md 30, 35, 40, 46, 52).
            { RegionBossRules.HedammuId, ProceduralEnemyKind.Hedammu },
            { RegionBossRules.SwallowingMireId, ProceduralEnemyKind.SwallowingMire },
            { RegionBossRules.AydaharId, ProceduralEnemyKind.Aydahar },
            { RegionBossRules.FourGluttonsId, ProceduralEnemyKind.FourGluttons },
            { RegionBossRules.OhyaId, ProceduralEnemyKind.Ohya },
            // Ossuário dos Artífices (Resources/Dungeons/Ossuario/Enemies, spawned by the DungeonDirector).
            { "enemy_floating_skull", ProceduralEnemyKind.FloatingSkull },
            { "enemy_skeleton_warrior", ProceduralEnemyKind.SkeletonWarrior },
            { "enemy_skeleton_archer", ProceduralEnemyKind.SkeletonArcher },
            { "enemy_bone_sorcerer", ProceduralEnemyKind.BoneSorcerer },
            { "boss_bone_colossus", ProceduralEnemyKind.BoneColossus },
        };

        public static readonly ProceduralEnemyKind[] AllKinds =
        {
            ProceduralEnemyKind.BatSwarm, ProceduralEnemyKind.StoneGolem, ProceduralEnemyKind.Warlord,
            ProceduralEnemyKind.RuinGuardian, ProceduralEnemyKind.AncientDweller,
            ProceduralEnemyKind.Wolf, ProceduralEnemyKind.AlphaWolf, ProceduralEnemyKind.Spider, ProceduralEnemyKind.Scorpion,
            ProceduralEnemyKind.Werewolf, ProceduralEnemyKind.VampireBat, ProceduralEnemyKind.ShroudOwl,
            ProceduralEnemyKind.FireflySwarm, ProceduralEnemyKind.Peccary, ProceduralEnemyKind.Potoo,
            ProceduralEnemyKind.EmberVulture, ProceduralEnemyKind.BrokenLantern, ProceduralEnemyKind.CrestedHawk, ProceduralEnemyKind.LanternFish,
            ProceduralEnemyKind.FloatingSkull, ProceduralEnemyKind.SkeletonWarrior, ProceduralEnemyKind.SkeletonArcher,
            ProceduralEnemyKind.BoneSorcerer, ProceduralEnemyKind.BoneColossus,
            ProceduralEnemyKind.Jackal, ProceduralEnemyKind.SandSnake, ProceduralEnemyKind.CrownedOrc, ProceduralEnemyKind.RollingStone,
            ProceduralEnemyKind.IllWindMist, ProceduralEnemyKind.Baboon, ProceduralEnemyKind.CaveOoze, ProceduralEnemyKind.DarkCrow,
            ProceduralEnemyKind.ThiefGull, ProceduralEnemyKind.CoconutCrab,
            ProceduralEnemyKind.MangroveCrocodile, ProceduralEnemyKind.GrassLion, ProceduralEnemyKind.DustRhino, ProceduralEnemyKind.ThiefMonkey,
            ProceduralEnemyKind.ThiefOtter, ProceduralEnemyKind.Python, ProceduralEnemyKind.DimmedTroll, ProceduralEnemyKind.BrownBear,
            ProceduralEnemyKind.WildYak, ProceduralEnemyKind.Irbis, ProceduralEnemyKind.Almas, ProceduralEnemyKind.GiantMarmot,
            ProceduralEnemyKind.StoneLion, ProceduralEnemyKind.LanternFox, ProceduralEnemyKind.TerracottaSoldier, ProceduralEnemyKind.StoneTurtle,
            ProceduralEnemyKind.MudBuffalo, ProceduralEnemyKind.NianCub, ProceduralEnemyKind.YoungGriffin, ProceduralEnemyKind.Saiga,
            ProceduralEnemyKind.BronzeWolf, ProceduralEnemyKind.SleepwalkerGiant, ProceduralEnemyKind.BarkBeetle, ProceduralEnemyKind.Ochokochi,
            ProceduralEnemyKind.SmokeGoat, ProceduralEnemyKind.GoldenBee, ProceduralEnemyKind.FluteBird, ProceduralEnemyKind.VineFox,
            ProceduralEnemyKind.AlpineChough, ProceduralEnemyKind.Takin, ProceduralEnemyKind.BlueSheep, ProceduralEnemyKind.CandleBat,
            ProceduralEnemyKind.RopeUshabti, ProceduralEnemyKind.PallasCat, ProceduralEnemyKind.StormEagle, ProceduralEnemyKind.SmokeHarpy,
            ProceduralEnemyKind.LeopardSeal, ProceduralEnemyKind.SmokeWalrus, ProceduralEnemyKind.IceGull, ProceduralEnemyKind.ArcticFox,
            ProceduralEnemyKind.PolarBear, ProceduralEnemyKind.ClayReindeer, ProceduralEnemyKind.StoneHare, ProceduralEnemyKind.TwigBird,
            ProceduralEnemyKind.BurrowMammoth, ProceduralEnemyKind.SeaWolf, ProceduralEnemyKind.GiantPike, ProceduralEnemyKind.ShadowSeal,
            ProceduralEnemyKind.VolcanoCrab, ProceduralEnemyKind.LavaLizard, ProceduralEnemyKind.EmberGull, ProceduralEnemyKind.DimmedShisa,
            ProceduralEnemyKind.Tanuki, ProceduralEnemyKind.Tibicena, ProceduralEnemyKind.GiantLavaLizard, ProceduralEnemyKind.BladeFish,
            ProceduralEnemyKind.EmberCoral, ProceduralEnemyKind.LacquerUshabti, ProceduralEnemyKind.FurnaceOgre, ProceduralEnemyKind.SlagGiant,
            ProceduralEnemyKind.DarkRiverCrocodile, ProceduralEnemyKind.DimmedIbis, ProceduralEnemyKind.ShadowScarab, ProceduralEnemyKind.ReedSnare,
            ProceduralEnemyKind.ShadowLocust, ProceduralEnemyKind.MudHippo, ProceduralEnemyKind.ReedSnake, ProceduralEnemyKind.ThreeHeadedSerpent,
            ProceduralEnemyKind.BlackScorpion, ProceduralEnemyKind.SandUshabti, ProceduralEnemyKind.ShadowBiter, ProceduralEnemyKind.CreepingDark,
            ProceduralEnemyKind.BrickLion, ProceduralEnemyKind.YoungMushussu, ProceduralEnemyKind.BitumenLocust, ProceduralEnemyKind.LivingHourglass,
            ProceduralEnemyKind.LateShadow, ProceduralEnemyKind.LivingTar, ProceduralEnemyKind.TarHeron, ProceduralEnemyKind.AngryCedar,
            ProceduralEnemyKind.GlowMantle, ProceduralEnemyKind.UllikummiSon, ProceduralEnemyKind.BrokenTablet, ProceduralEnemyKind.GardenShadow,
            ProceduralEnemyKind.CopyShadow, ProceduralEnemyKind.SmokeGiant, ProceduralEnemyKind.SmokeArcher, ProceduralEnemyKind.EmberWolf,
            ProceduralEnemyKind.HotSmokeLion, ProceduralEnemyKind.BlackEagle, ProceduralEnemyKind.WindHarpy, ProceduralEnemyKind.MudOctopus,
            ProceduralEnemyKind.DarkFish, ProceduralEnemyKind.ShadowLizard, ProceduralEnemyKind.LesserShadowDragon, ProceduralEnemyKind.LesserArchon,
            ProceduralEnemyKind.DimmedCloudBird, ProceduralEnemyKind.DimmedLightFish, ProceduralEnemyKind.DarkStowaway, ProceduralEnemyKind.HoldShadow,
            ProceduralEnemyKind.LivingRope, ProceduralEnemyKind.ToyArchon, ProceduralEnemyKind.CrackedJar, ProceduralEnemyKind.ChainedArchon,
            ProceduralEnemyKind.BalloonGiant, ProceduralEnemyKind.AzThief, ProceduralEnemyKind.DimmedLightGull, ProceduralEnemyKind.SkyStone,
            ProceduralEnemyKind.DevouringSalamander, ProceduralEnemyKind.VolcanicBomb, ProceduralEnemyKind.ThiefGoose, ProceduralEnemyKind.ChildOfApep,
            ProceduralEnemyKind.Quicksand, ProceduralEnemyKind.ScorpionMan, ProceduralEnemyKind.CedarLynx, ProceduralEnemyKind.ResinBee,
            ProceduralEnemyKind.BladeBat, ProceduralEnemyKind.GreatEel, ProceduralEnemyKind.AshBull, ProceduralEnemyKind.SmokeRope,
            ProceduralEnemyKind.WindKing, ProceduralEnemyKind.OldKul, ProceduralEnemyKind.EmberGlutton,
            ProceduralEnemyKind.Apep, ProceduralEnemyKind.DreamOfDestruction, ProceduralEnemyKind.GreatDark, ProceduralEnemyKind.AzCaptain,
            ProceduralEnemyKind.Hedammu, ProceduralEnemyKind.SwallowingMire, ProceduralEnemyKind.Aydahar, ProceduralEnemyKind.FourGluttons, ProceduralEnemyKind.Ohya,
        };

        /// <summary>The Ossuário dos Artífices guardians (<see cref="OssuaryEnemyRecipes"/>).</summary>
        public static bool IsOssuaryGuardian(ProceduralEnemyKind kind) => kind >= ProceduralEnemyKind.FloatingSkull && kind <= ProceduralEnemyKind.BoneColossus;

        /// <summary>The night-only species models (Docs/DayNight.md): they also get a <see cref="Rapadura.Gameplay.Night.NightCreatureVoice"/>.</summary>
        public static bool IsNightSpecies(ProceduralEnemyKind kind) => kind >= ProceduralEnemyKind.Werewolf && kind <= ProceduralEnemyKind.Potoo;

        public static ProceduralEnemyKind KindFor(string enemyId)
        {
            return !string.IsNullOrEmpty(enemyId) && KindsById.TryGetValue(enemyId, out ProceduralEnemyKind kind)
                ? kind
                : ProceduralEnemyKind.None;
        }

        public static ProceduralEnemyKind KindFor(EnemyDefinition definition) => KindFor(DefeatedEnemyRegistry.GetId(definition));

        public static bool IsBoss(ProceduralEnemyKind kind)
        {
            return kind == ProceduralEnemyKind.Warlord || kind == ProceduralEnemyKind.RuinGuardian || kind == ProceduralEnemyKind.AncientDweller
                || kind == ProceduralEnemyKind.AlphaWolf || kind == ProceduralEnemyKind.BoneColossus || IsRegionWorldBoss(kind);
        }

        /// <summary>The world bosses of Regions 5–16 (<see cref="RegionBossRecipes"/>).</summary>
        public static bool IsRegionWorldBoss(ProceduralEnemyKind kind) => kind >= ProceduralEnemyKind.WindKing && kind <= ProceduralEnemyKind.Ohya;

        private static string RegionBossIdOf(ProceduralEnemyKind kind)
        {
            switch (kind)
            {
                case ProceduralEnemyKind.WindKing: return RegionBossRules.WindKingId;
                case ProceduralEnemyKind.OldKul: return RegionBossRules.OldKulId;
                case ProceduralEnemyKind.Apep: return RegionBossRules.ApepId;
                case ProceduralEnemyKind.DreamOfDestruction: return RegionBossRules.DreamOfDestructionId;
                case ProceduralEnemyKind.GreatDark: return RegionBossRules.GreatDarkId;
                case ProceduralEnemyKind.AzCaptain: return RegionBossRules.AzCaptainId;
                case ProceduralEnemyKind.Hedammu: return RegionBossRules.HedammuId;
                case ProceduralEnemyKind.SwallowingMire: return RegionBossRules.SwallowingMireId;
                case ProceduralEnemyKind.Aydahar: return RegionBossRules.AydaharId;
                case ProceduralEnemyKind.FourGluttons: return RegionBossRules.FourGluttonsId;
                case ProceduralEnemyKind.Ohya: return RegionBossRules.OhyaId;
                default: return RegionBossRules.EmberGluttonId;
            }
        }

        public static int TriangleBudget(ProceduralEnemyKind kind) => IsBoss(kind) ? BossTriangleBudget : CommonTriangleBudget;

        /// <summary>
        /// A freshly built blueprint (not cached: the caller owns its meshes). Deterministic: two
        /// calls produce identical meshes.
        /// </summary>
        public static ProceduralEnemyBlueprint CreateBlueprint(ProceduralEnemyKind kind)
        {
            var blueprint = new ProceduralEnemyBlueprint(kind, IsBoss(kind));
            switch (kind)
            {
                case ProceduralEnemyKind.BatSwarm:
                    BatSwarmRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.StoneGolem:
                    StoneGolemRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Warlord:
                    WarlordRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.RuinGuardian:
                    RuinGuardianRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.AncientDweller:
                    AncientDwellerRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Wolf:
                    WolfRecipe.Build(blueprint, false);
                    break;
                case ProceduralEnemyKind.AlphaWolf:
                    WolfRecipe.Build(blueprint, true);
                    break;
                case ProceduralEnemyKind.Spider:
                    SpiderRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Scorpion:
                    ScorpionRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Werewolf:
                    WerewolfRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.VampireBat:
                    VampireBatRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.ShroudOwl:
                    ShroudOwlRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.FireflySwarm:
                    FireflySwarmRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Peccary:
                    PeccaryRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.Potoo:
                    PotooRecipe.Build(blueprint);
                    break;
                case ProceduralEnemyKind.EmberVulture:
                    RangedEnemyRecipes.BuildEmberVulture(blueprint);
                    break;
                case ProceduralEnemyKind.BrokenLantern:
                    RangedEnemyRecipes.BuildBrokenLantern(blueprint);
                    break;
                case ProceduralEnemyKind.CrestedHawk:
                    RangedEnemyRecipes.BuildCrestedHawk(blueprint);
                    break;
                case ProceduralEnemyKind.LanternFish:
                    RangedEnemyRecipes.BuildLanternFish(blueprint);
                    break;
                case ProceduralEnemyKind.FloatingSkull:
                    OssuaryEnemyRecipes.BuildFloatingSkull(blueprint);
                    break;
                case ProceduralEnemyKind.SkeletonWarrior:
                    OssuaryEnemyRecipes.BuildSkeletonWarrior(blueprint);
                    break;
                case ProceduralEnemyKind.SkeletonArcher:
                    OssuaryEnemyRecipes.BuildSkeletonArcher(blueprint);
                    break;
                case ProceduralEnemyKind.BoneSorcerer:
                    OssuaryEnemyRecipes.BuildBoneSorcerer(blueprint);
                    break;
                case ProceduralEnemyKind.BoneColossus:
                    OssuaryEnemyRecipes.BuildBoneColossus(blueprint);
                    break;
                case ProceduralEnemyKind.Jackal:
                    OverworldBeastRecipes.BuildJackal(blueprint);
                    break;
                case ProceduralEnemyKind.SandSnake:
                    OverworldBeastRecipes.BuildSandSnake(blueprint);
                    break;
                case ProceduralEnemyKind.CrownedOrc:
                    OverworldOddityRecipes.BuildCrownedOrc(blueprint);
                    break;
                case ProceduralEnemyKind.RollingStone:
                    OverworldOddityRecipes.BuildRollingStone(blueprint);
                    break;
                case ProceduralEnemyKind.IllWindMist:
                    OverworldOddityRecipes.BuildIllWindMist(blueprint);
                    break;
                case ProceduralEnemyKind.Baboon:
                    OverworldBeastRecipes.BuildBaboon(blueprint);
                    break;
                case ProceduralEnemyKind.CaveOoze:
                    OverworldOddityRecipes.BuildCaveOoze(blueprint);
                    break;
                case ProceduralEnemyKind.DarkCrow:
                    OverworldBeastRecipes.BuildDarkCrow(blueprint);
                    break;
                case ProceduralEnemyKind.ThiefGull:
                    OverworldBeastRecipes.BuildThiefGull(blueprint);
                    break;
                case ProceduralEnemyKind.CoconutCrab:
                    OverworldBeastRecipes.BuildCoconutCrab(blueprint);
                    break;
                case ProceduralEnemyKind.MangroveCrocodile:
                    CoastDeltaBeastRecipes.BuildMangroveCrocodile(blueprint);
                    break;
                case ProceduralEnemyKind.GrassLion:
                    CoastDeltaBeastRecipes.BuildGrassLion(blueprint);
                    break;
                case ProceduralEnemyKind.DustRhino:
                    CoastDeltaBeastRecipes.BuildDustRhino(blueprint);
                    break;
                case ProceduralEnemyKind.ThiefMonkey:
                    CoastDeltaBeastRecipes.BuildThiefMonkey(blueprint);
                    break;
                case ProceduralEnemyKind.ThiefOtter:
                    CoastDeltaBeastRecipes.BuildThiefOtter(blueprint);
                    break;
                case ProceduralEnemyKind.Python:
                    CoastDeltaBeastRecipes.BuildPython(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedTroll:
                    CoastDeltaBeastRecipes.BuildDimmedTroll(blueprint);
                    break;
                case ProceduralEnemyKind.BrownBear:
                    PamirBeastRecipes.BuildBrownBear(blueprint);
                    break;
                case ProceduralEnemyKind.WildYak:
                    PamirBeastRecipes.BuildWildYak(blueprint);
                    break;
                case ProceduralEnemyKind.Irbis:
                    PamirBeastRecipes.BuildIrbis(blueprint);
                    break;
                case ProceduralEnemyKind.Almas:
                    PamirBeastRecipes.BuildAlmas(blueprint);
                    break;
                case ProceduralEnemyKind.GiantMarmot:
                    PamirBeastRecipes.BuildGiantMarmot(blueprint);
                    break;
                case ProceduralEnemyKind.StoneLion:
                    MingchengBeastRecipes.BuildStoneLion(blueprint);
                    break;
                case ProceduralEnemyKind.LanternFox:
                    MingchengBeastRecipes.BuildLanternFox(blueprint);
                    break;
                case ProceduralEnemyKind.TerracottaSoldier:
                    MingchengBeastRecipes.BuildTerracottaSoldier(blueprint);
                    break;
                case ProceduralEnemyKind.StoneTurtle:
                    MingchengBeastRecipes.BuildStoneTurtle(blueprint);
                    break;
                case ProceduralEnemyKind.MudBuffalo:
                    MingchengBeastRecipes.BuildMudBuffalo(blueprint);
                    break;
                case ProceduralEnemyKind.NianCub:
                    MingchengBeastRecipes.BuildNianCub(blueprint);
                    break;
                case ProceduralEnemyKind.YoungGriffin:
                    NartesBeastRecipes.BuildYoungGriffin(blueprint);
                    break;
                case ProceduralEnemyKind.Saiga:
                    NartesBeastRecipes.BuildSaiga(blueprint);
                    break;
                case ProceduralEnemyKind.BronzeWolf:
                    NartesBeastRecipes.BuildBronzeWolf(blueprint);
                    break;
                case ProceduralEnemyKind.SleepwalkerGiant:
                    NartesBeastRecipes.BuildSleepwalkerGiant(blueprint);
                    break;
                case ProceduralEnemyKind.BarkBeetle:
                    NartesBeastRecipes.BuildBarkBeetle(blueprint);
                    break;
                case ProceduralEnemyKind.Ochokochi:
                    NartesBeastRecipes.BuildOchokochi(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeGoat:
                    FarnkeshBeastRecipes.BuildSmokeGoat(blueprint);
                    break;
                case ProceduralEnemyKind.GoldenBee:
                    FarnkeshBeastRecipes.BuildGoldenBee(blueprint);
                    break;
                case ProceduralEnemyKind.FluteBird:
                    FarnkeshBeastRecipes.BuildFluteBird(blueprint);
                    break;
                case ProceduralEnemyKind.VineFox:
                    FarnkeshBeastRecipes.BuildVineFox(blueprint);
                    break;
                case ProceduralEnemyKind.AlpineChough:
                    FarnkeshBeastRecipes.BuildAlpineChough(blueprint);
                    break;
                case ProceduralEnemyKind.Takin:
                    FarnkeshBeastRecipes.BuildTakin(blueprint);
                    break;
                case ProceduralEnemyKind.BlueSheep:
                    FarnkeshBeastRecipes.BuildBlueSheep(blueprint);
                    break;
                case ProceduralEnemyKind.CandleBat:
                    FarnkeshBeastRecipes.BuildCandleBat(blueprint);
                    break;
                case ProceduralEnemyKind.RopeUshabti:
                    FarnkeshBeastRecipes.BuildRopeUshabti(blueprint);
                    break;
                case ProceduralEnemyKind.PallasCat:
                    FarnkeshBeastRecipes.BuildPallasCat(blueprint);
                    break;
                case ProceduralEnemyKind.StormEagle:
                    FarnkeshBeastRecipes.BuildStormEagle(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeHarpy:
                    FarnkeshBeastRecipes.BuildSmokeHarpy(blueprint);
                    break;
                case ProceduralEnemyKind.LeopardSeal:
                    MarGeladoBeastRecipes.BuildLeopardSeal(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeWalrus:
                    MarGeladoBeastRecipes.BuildSmokeWalrus(blueprint);
                    break;
                case ProceduralEnemyKind.IceGull:
                    MarGeladoBeastRecipes.BuildIceGull(blueprint);
                    break;
                case ProceduralEnemyKind.ArcticFox:
                    MarGeladoBeastRecipes.BuildArcticFox(blueprint);
                    break;
                case ProceduralEnemyKind.PolarBear:
                    MarGeladoBeastRecipes.BuildPolarBear(blueprint);
                    break;
                case ProceduralEnemyKind.ClayReindeer:
                    MarGeladoBeastRecipes.BuildClayReindeer(blueprint);
                    break;
                case ProceduralEnemyKind.StoneHare:
                    MarGeladoBeastRecipes.BuildStoneHare(blueprint);
                    break;
                case ProceduralEnemyKind.TwigBird:
                    MarGeladoBeastRecipes.BuildTwigBird(blueprint);
                    break;
                case ProceduralEnemyKind.BurrowMammoth:
                    MarGeladoBeastRecipes.BuildBurrowMammoth(blueprint);
                    break;
                case ProceduralEnemyKind.SeaWolf:
                    MarGeladoBeastRecipes.BuildSeaWolf(blueprint);
                    break;
                case ProceduralEnemyKind.GiantPike:
                    MarGeladoBeastRecipes.BuildGiantPike(blueprint);
                    break;
                case ProceduralEnemyKind.ShadowSeal:
                    MarGeladoBeastRecipes.BuildShadowSeal(blueprint);
                    break;
                case ProceduralEnemyKind.VolcanoCrab:
                    HinokuraBeastRecipes.BuildVolcanoCrab(blueprint);
                    break;
                case ProceduralEnemyKind.LavaLizard:
                    HinokuraBeastRecipes.BuildLavaLizard(blueprint);
                    break;
                case ProceduralEnemyKind.EmberGull:
                    HinokuraBeastRecipes.BuildEmberGull(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedShisa:
                    HinokuraBeastRecipes.BuildDimmedShisa(blueprint);
                    break;
                case ProceduralEnemyKind.Tanuki:
                    HinokuraBeastRecipes.BuildTanuki(blueprint);
                    break;
                case ProceduralEnemyKind.Tibicena:
                    HinokuraBeastRecipes.BuildTibicena(blueprint);
                    break;
                case ProceduralEnemyKind.GiantLavaLizard:
                    HinokuraBeastRecipes.BuildGiantLavaLizard(blueprint);
                    break;
                case ProceduralEnemyKind.BladeFish:
                    HinokuraBeastRecipes.BuildBladeFish(blueprint);
                    break;
                case ProceduralEnemyKind.EmberCoral:
                    HinokuraBeastRecipes.BuildEmberCoral(blueprint);
                    break;
                case ProceduralEnemyKind.LacquerUshabti:
                    HinokuraBeastRecipes.BuildLacquerUshabti(blueprint);
                    break;
                case ProceduralEnemyKind.FurnaceOgre:
                    HinokuraBeastRecipes.BuildFurnaceOgre(blueprint);
                    break;
                case ProceduralEnemyKind.SlagGiant:
                    HinokuraBeastRecipes.BuildSlagGiant(blueprint);
                    break;
                case ProceduralEnemyKind.DarkRiverCrocodile:
                    RioBeastRecipes.BuildDarkRiverCrocodile(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedIbis:
                    RioBeastRecipes.BuildDimmedIbis(blueprint);
                    break;
                case ProceduralEnemyKind.ShadowScarab:
                    RioBeastRecipes.BuildShadowScarab(blueprint);
                    break;
                case ProceduralEnemyKind.ReedSnare:
                    RioBeastRecipes.BuildReedSnare(blueprint);
                    break;
                case ProceduralEnemyKind.ShadowLocust:
                    RioBeastRecipes.BuildShadowLocust(blueprint);
                    break;
                case ProceduralEnemyKind.MudHippo:
                    RioBeastRecipes.BuildMudHippo(blueprint);
                    break;
                case ProceduralEnemyKind.ReedSnake:
                    RioBeastRecipes.BuildReedSnake(blueprint);
                    break;
                case ProceduralEnemyKind.ThreeHeadedSerpent:
                    RioBeastRecipes.BuildThreeHeadedSerpent(blueprint);
                    break;
                case ProceduralEnemyKind.BlackScorpion:
                    RioBeastRecipes.BuildBlackScorpion(blueprint);
                    break;
                case ProceduralEnemyKind.SandUshabti:
                    RioBeastRecipes.BuildSandUshabti(blueprint);
                    break;
                case ProceduralEnemyKind.ShadowBiter:
                    RioBeastRecipes.BuildShadowBiter(blueprint);
                    break;
                case ProceduralEnemyKind.CreepingDark:
                    RioBeastRecipes.BuildCreepingDark(blueprint);
                    break;
                case ProceduralEnemyKind.BrickLion:
                    MisturaBeastRecipes.BuildBrickLion(blueprint);
                    break;
                case ProceduralEnemyKind.YoungMushussu:
                    MisturaBeastRecipes.BuildYoungMushussu(blueprint);
                    break;
                case ProceduralEnemyKind.BitumenLocust:
                    MisturaBeastRecipes.BuildBitumenLocust(blueprint);
                    break;
                case ProceduralEnemyKind.LivingHourglass:
                    MisturaBeastRecipes.BuildLivingHourglass(blueprint);
                    break;
                case ProceduralEnemyKind.LateShadow:
                    MisturaBeastRecipes.BuildLateShadow(blueprint);
                    break;
                case ProceduralEnemyKind.LivingTar:
                    MisturaBeastRecipes.BuildLivingTar(blueprint);
                    break;
                case ProceduralEnemyKind.TarHeron:
                    MisturaBeastRecipes.BuildTarHeron(blueprint);
                    break;
                case ProceduralEnemyKind.AngryCedar:
                    MisturaBeastRecipes.BuildAngryCedar(blueprint);
                    break;
                case ProceduralEnemyKind.GlowMantle:
                    MisturaBeastRecipes.BuildGlowMantle(blueprint);
                    break;
                case ProceduralEnemyKind.UllikummiSon:
                    MisturaBeastRecipes.BuildUllikummiSon(blueprint);
                    break;
                case ProceduralEnemyKind.BrokenTablet:
                    MisturaBeastRecipes.BuildBrokenTablet(blueprint);
                    break;
                case ProceduralEnemyKind.GardenShadow:
                    MisturaBeastRecipes.BuildGardenShadow(blueprint);
                    break;
                case ProceduralEnemyKind.CopyShadow:
                    ProfundoBeastRecipes.BuildCopyShadow(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeGiant:
                    ProfundoBeastRecipes.BuildSmokeGiant(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeArcher:
                    ProfundoBeastRecipes.BuildSmokeArcher(blueprint);
                    break;
                case ProceduralEnemyKind.EmberWolf:
                    ProfundoBeastRecipes.BuildEmberWolf(blueprint);
                    break;
                case ProceduralEnemyKind.HotSmokeLion:
                    ProfundoBeastRecipes.BuildHotSmokeLion(blueprint);
                    break;
                case ProceduralEnemyKind.BlackEagle:
                    ProfundoBeastRecipes.BuildBlackEagle(blueprint);
                    break;
                case ProceduralEnemyKind.WindHarpy:
                    ProfundoBeastRecipes.BuildWindHarpy(blueprint);
                    break;
                case ProceduralEnemyKind.MudOctopus:
                    ProfundoBeastRecipes.BuildMudOctopus(blueprint);
                    break;
                case ProceduralEnemyKind.DarkFish:
                    ProfundoBeastRecipes.BuildDarkFish(blueprint);
                    break;
                case ProceduralEnemyKind.ShadowLizard:
                    ProfundoBeastRecipes.BuildShadowLizard(blueprint);
                    break;
                case ProceduralEnemyKind.LesserShadowDragon:
                    ProfundoBeastRecipes.BuildLesserShadowDragon(blueprint);
                    break;
                case ProceduralEnemyKind.LesserArchon:
                    ProfundoBeastRecipes.BuildLesserArchon(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedCloudBird:
                    CeuBeastRecipes.BuildDimmedCloudBird(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedLightFish:
                    CeuBeastRecipes.BuildDimmedLightFish(blueprint);
                    break;
                case ProceduralEnemyKind.DarkStowaway:
                    CeuBeastRecipes.BuildDarkStowaway(blueprint);
                    break;
                case ProceduralEnemyKind.HoldShadow:
                    CeuBeastRecipes.BuildHoldShadow(blueprint);
                    break;
                case ProceduralEnemyKind.LivingRope:
                    CeuBeastRecipes.BuildLivingRope(blueprint);
                    break;
                case ProceduralEnemyKind.ToyArchon:
                    CeuBeastRecipes.BuildToyArchon(blueprint);
                    break;
                case ProceduralEnemyKind.CrackedJar:
                    CeuBeastRecipes.BuildCrackedJar(blueprint);
                    break;
                case ProceduralEnemyKind.ChainedArchon:
                    CeuBeastRecipes.BuildChainedArchon(blueprint);
                    break;
                case ProceduralEnemyKind.BalloonGiant:
                    CeuBeastRecipes.BuildBalloonGiant(blueprint);
                    break;
                case ProceduralEnemyKind.AzThief:
                    CeuBeastRecipes.BuildAzThief(blueprint);
                    break;
                case ProceduralEnemyKind.DimmedLightGull:
                    CeuBeastRecipes.BuildDimmedLightGull(blueprint);
                    break;
                case ProceduralEnemyKind.DevouringSalamander:
                    LateStandInBeastRecipes.BuildDevouringSalamander(blueprint);
                    break;
                case ProceduralEnemyKind.VolcanicBomb:
                    LateStandInBeastRecipes.BuildVolcanicBomb(blueprint);
                    break;
                case ProceduralEnemyKind.ThiefGoose:
                    LateStandInBeastRecipes.BuildThiefGoose(blueprint);
                    break;
                case ProceduralEnemyKind.ChildOfApep:
                    LateStandInBeastRecipes.BuildChildOfApep(blueprint);
                    break;
                case ProceduralEnemyKind.Quicksand:
                    LateStandInBeastRecipes.BuildQuicksand(blueprint);
                    break;
                case ProceduralEnemyKind.ScorpionMan:
                    LateStandInBeastRecipes.BuildScorpionMan(blueprint);
                    break;
                case ProceduralEnemyKind.CedarLynx:
                    LateStandInBeastRecipes.BuildCedarLynx(blueprint);
                    break;
                case ProceduralEnemyKind.ResinBee:
                    LateStandInBeastRecipes.BuildResinBee(blueprint);
                    break;
                case ProceduralEnemyKind.BladeBat:
                    LateStandInBeastRecipes.BuildBladeBat(blueprint);
                    break;
                case ProceduralEnemyKind.GreatEel:
                    LateStandInBeastRecipes.BuildGreatEel(blueprint);
                    break;
                case ProceduralEnemyKind.AshBull:
                    LateStandInBeastRecipes.BuildAshBull(blueprint);
                    break;
                case ProceduralEnemyKind.SmokeRope:
                    LateStandInBeastRecipes.BuildSmokeRope(blueprint);
                    break;
                case ProceduralEnemyKind.SkyStone:
                    CeuBeastRecipes.BuildSkyStone(blueprint);
                    break;
                case ProceduralEnemyKind.WindKing:
                    RegionBossRecipes.BuildWindKing(blueprint);
                    break;
                case ProceduralEnemyKind.OldKul:
                    RegionBossRecipes.BuildOldKul(blueprint);
                    break;
                case ProceduralEnemyKind.EmberGlutton:
                    RegionBossRecipes.BuildEmberGlutton(blueprint);
                    break;
                case ProceduralEnemyKind.Apep:
                    RegionBossRecipes.BuildApep(blueprint);
                    break;
                case ProceduralEnemyKind.DreamOfDestruction:
                    RegionBossRecipes.BuildDreamOfDestruction(blueprint);
                    break;
                case ProceduralEnemyKind.GreatDark:
                    RegionBossRecipes.BuildGreatDark(blueprint);
                    break;
                case ProceduralEnemyKind.AzCaptain:
                    RegionBossRecipes.BuildAzCaptain(blueprint);
                    break;
                case ProceduralEnemyKind.Hedammu:
                    RegionBossRecipes.BuildHedammu(blueprint);
                    break;
                case ProceduralEnemyKind.SwallowingMire:
                    RegionBossRecipes.BuildSwallowingMire(blueprint);
                    break;
                case ProceduralEnemyKind.Aydahar:
                    RegionBossRecipes.BuildAydahar(blueprint);
                    break;
                case ProceduralEnemyKind.FourGluttons:
                    RegionBossRecipes.BuildFourGluttons(blueprint);
                    break;
                case ProceduralEnemyKind.Ohya:
                    RegionBossRecipes.BuildOhya(blueprint);
                    break;
            }

            return blueprint;
        }

        /// <summary>The cached, shared blueprint of <paramref name="kind"/> (built on first use).</summary>
        public static ProceduralEnemyBlueprint GetBlueprint(ProceduralEnemyKind kind)
        {
            if (!Cache.TryGetValue(kind, out ProceduralEnemyBlueprint blueprint) || !IsAlive(blueprint))
            {
                blueprint = CreateBlueprint(kind);
                Cache[kind] = blueprint;
            }

            return blueprint;
        }

        /// <summary>The one material every procedural enemy shares (palette albedo + glow emission).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_sharedMaterial == null)
                {
                    _sharedMaterial = EnemyModelPalette.CreateMaterial(EnemyModelPalette.CreateTexture(), EnemyModelPalette.CreateEmissionTexture());
                    _sharedMaterial.hideFlags = HideFlags.DontSave;
                }

                return _sharedMaterial;
            }
        }

        /// <summary>Instantiates the cached model of <paramref name="kind"/> (shared meshes + material) under <paramref name="parent"/>.</summary>
        public static GameObject Instantiate(ProceduralEnemyKind kind, Transform parent)
        {
            return kind == ProceduralEnemyKind.None ? null : Instantiate(GetBlueprint(kind), SharedMaterial, parent);
        }

        /// <summary>Builds the part hierarchy of <paramref name="blueprint"/> (root named "EnemyVisual", with a <see cref="ProceduralEnemyRig"/>).</summary>
        public static GameObject Instantiate(ProceduralEnemyBlueprint blueprint, Material material, Transform parent)
        {
            var root = new GameObject(EnemyVisualSwapper.VisualName);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var transforms = new Transform[blueprint.Parts.Count];
            for (int i = 0; i < blueprint.Parts.Count; i++)
            {
                ProceduralEnemyBlueprint.Part part = blueprint.Parts[i];
                var node = new GameObject(part.Name);
                Transform t = node.transform;
                t.SetParent(part.Parent >= 0 ? transforms[part.Parent] : root.transform, false);
                t.SetLocalPositionAndRotation(part.Position, part.Rotation);
                t.localScale = part.Scale;
                transforms[i] = t;

                if (part.Mesh != null)
                {
                    node.AddComponent<MeshFilter>().sharedMesh = part.Mesh;
                    node.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
            }

            root.AddComponent<ProceduralEnemyRig>().Configure(blueprint.Kind);
            if (IsRegionWorldBoss(blueprint.Kind))
            {
                root.AddComponent<RegionBossAbilities>().Configure(RegionBossIdOf(blueprint.Kind));
            }

            if (IsNightSpecies(blueprint.Kind))
            {
                // Arrival call + attack cue through the procedural synth (pool-safe: re-armed on every enable).
                root.AddComponent<Rapadura.Gameplay.Night.NightCreatureVoice>().Configure(blueprint.Kind);
            }

            return root;
        }

        private static bool IsAlive(ProceduralEnemyBlueprint blueprint)
        {
            if (blueprint == null || blueprint.Meshes.Count == 0)
            {
                return false;
            }

            foreach (Mesh mesh in blueprint.Meshes)
            {
                if (mesh == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
