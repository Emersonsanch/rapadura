using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>Which of its two mouths O Rei Shisa do Telhado roars with (<see cref="RegionBossRules.ShisaMouthFor"/>): the shisa pairs of the roofs, one open and one shut.</summary>
    public enum ShisaMouth
    {
        /// <summary>The open mouth breathes the whole garden in: everyone further than its reach is hit; up close, under the chin, is safe.</summary>
        Open = 0,

        /// <summary>The shut mouth keeps the bad out: everyone within its reach is hit; far away is safe.</summary>
        Closed = 1,
    }

    /// <summary>Which colour O Grande Borrão has not eaten yet (<see cref="RegionBossRules.SmudgeColourFor"/>): the one band round it the smear leaves clean.</summary>
    public enum SmudgeColour
    {
        /// <summary>The ochre of the eland's body: the band right next to it is clean.</summary>
        Ochre = 0,

        /// <summary>The red of the dancers: the middle band is clean.</summary>
        Red = 1,

        /// <summary>The white of the painted rain: the far band is clean.</summary>
        White = 2,
    }

    /// <summary>Where the wind blows from in O Senhor do Cata-Vento's gust (<see cref="RegionBossRules.VaneWindFor"/>): it turns a quarter, clockwise, each gust.</summary>
    public enum VaneWind
    {
        /// <summary>From the north: it blows toward the south, and north of him is safe.</summary>
        North = 0,

        /// <summary>From the east: east of him is safe.</summary>
        East = 1,

        /// <summary>From the south: south of him is safe.</summary>
        South = 2,

        /// <summary>From the west: west of him is safe.</summary>
        West = 3,
    }

    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69; on the road of the middle story, 25, 26, 32, 37, 38, 43; in the first regions,
    /// that every player walks, 04, 09, 15; and 34, 45, 48): smaller than the world bosses, one per map the story walks through in the 150–300 band, in
    /// Regions 5–8 and in Regions 1–3. They are kept by the same <see cref="RegionBossDirector"/>, fight with the same <see cref="RegionBossAbilities"/>
    /// (phases, ring, barrage, great blow) and stand on a stone ring of their map, each with one ability of its own
    /// (<see cref="RegionBossSignature"/> 10–27). They stay out of <see cref="AllSpawns"/> (the world bosses: keepsakes, trophies,
    /// the progression's boss steps) and drop items that already exist. Beaten, each is freed and leaves (<see cref="RegionBossSpawn.NeverDies"/>).
    /// </summary>
    public static partial class RegionBossRules
    {
        public const string RopeCutterId = "enemy_mountain_rope_cutter";
        public const string EndlessBellId = "enemy_mountain_endless_bell";
        public const string WalrusQueenId = "enemy_mountain_walrus_queen";
        public const string GrandfatherMammothId = "enemy_mountain_grandfather_mammoth";
        public const string ShisaKingId = "enemy_mountain_shisa_king";
        public const string FurnaceMasterId = "enemy_mountain_nine_furnaces_master";

        // The middle story's road (MidStoryCatalog: Docs/Mapas100.md 25, 26, 32, 37, 38, 43), Regions 5–8, which had no map boss.
        public const string MonsoonOctopusId = "enemy_mountain_monsoon_octopus";
        public const string SmokeAurochsId = "enemy_mountain_smoke_aurochs";
        public const string ForgetfulBridgeId = "enemy_mountain_forgetful_bridge";
        public const string RunawayCartId = "enemy_mountain_runaway_cart";
        public const string GreatAlmasId = "enemy_mountain_great_almas";
        public const string UnleapedCarpId = "enemy_mountain_unleaped_carp";

        // The maps every player walks through (Regions 1–3: Docs/Mapas100.md 04, 09, 15), with no boss of the campaign on them, and 34, 45, 48.
        public const string ForgottenSpinnerId = "enemy_mountain_forgotten_spinner";
        public const string WellDrinkingRootId = "enemy_mountain_well_drinking_root";
        public const string GreatSmudgeId = "enemy_mountain_great_smudge";
        public const string NinkiNankaId = "enemy_mountain_ninki_nanka";
        public const string NianId = "enemy_mountain_nian";
        public const string OldKurganGriffinId = "enemy_mountain_old_kurgan_griffin";

        // Six more of Regions 1–8, one per region the earlier levas left without one: Docs/Mapas100.md 05, 10, 22, 33, 39, 42.
        public const string MistElderId = "enemy_mountain_mist_elder";
        public const string DuneChoirId = "enemy_mountain_dune_choir";
        public const string GreedyBottlerId = "enemy_mountain_greedy_bottler";
        public const string LightFisherId = "enemy_mountain_light_fisher";
        public const string VaneLordId = "enemy_mountain_vane_lord";
        public const string LittleEclipseId = "enemy_mountain_little_eclipse";

        /// <summary>The map bosses (04, 05, 09, 10, 15, 22; 25, 26, 32, 33, 34, 37, 38, 39, 42, 43, 45, 48; 55, 56, 60, 62, 66, 69), each at the top of its map's band.</summary>
        public static readonly IReadOnlyList<RegionBossSpawn> MapSpawns = new[]
        {
            new RegionBossSpawn(RopeCutterId, OverworldLayouts.PontesVentoMapId, "pontes_de_vento", OverworldLayouts.CortadorDeCordasArena,
                OverworldLayouts.FarnkeshWorldOffset, 160, EnemyProjectileKind.Feather, false, RegionBossSignature.RopeCut, neverDies: true),
            new RegionBossSpawn(EndlessBellId, OverworldLayouts.ErmidasMapId, "pinaculo_do_sino", OverworldLayouts.SinoQueNaoParaArena,
                OverworldLayouts.FarnkeshWorldOffset, 168, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.EchoToll, neverDies: true),
            new RegionBossSpawn(WalrusQueenId, OverworldLayouts.CanaisGeloMapId, "canais_de_gelo", OverworldLayouts.MorsaRainhaArena,
                OverworldLayouts.MarGeladoWorldOffset, 196, EnemyProjectileKind.LureGlob, true, RegionBossSignature.BellyFlop, neverDies: true),
            new RegionBossSpawn(GrandfatherMammothId, OverworldLayouts.GaleriasMamutesMapId, "galerias_dos_mamutes", OverworldLayouts.MamuteAvoArena,
                OverworldLayouts.MarGeladoWorldOffset, 214, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.CeilingFall, neverDies: true),
            new RegionBossSpawn(ShisaKingId, OverworldLayouts.JardinsLavaMapId, "pontes_vermelhas", OverworldLayouts.ReiShisaArena,
                OverworldLayouts.HinokuraWorldOffset, 236, EnemyProjectileKind.Ember, false, RegionBossSignature.TwoMouths, neverDies: true),
            new RegionBossSpawn(FurnaceMasterId, OverworldLayouts.NoveFornalhasMapId, "patio_das_fornalhas", OverworldLayouts.MestreDasFornalhasArena,
                OverworldLayouts.HinokuraWorldOffset, 264, EnemyProjectileKind.Ember, true, RegionBossSignature.ForgeStroke, neverDies: true),
            new RegionBossSpawn(MonsoonOctopusId, OverworldLayouts.TaaPwaniMapId, "praia_dos_dhows", OverworldLayouts.PolvoDasMoncoesArena,
                OverworldLayouts.CostaWorldOffset, 50, EnemyProjectileKind.LureGlob, false, RegionBossSignature.MonsoonArms, neverDies: true),
            new RegionBossSpawn(SmokeAurochsId, OverworldLayouts.AuroquesId, "savana_dos_auroques", OverworldLayouts.TouroDeFumacaArena,
                OverworldLayouts.CostaWorldOffset, 55, EnemyProjectileKind.Ember, false, RegionBossSignature.HerdSnort, neverDies: true),
            new RegionBossSpawn(ForgetfulBridgeId, OverworldLayouts.RaizVivaId, "cascatas_da_chuva", OverworldLayouts.PonteQueEsqueceuArena,
                OverworldLayouts.DeltaWorldOffset, 70, EnemyProjectileKind.Feather, false, RegionBossSignature.ForgottenPlanks, neverDies: true),
            new RegionBossSpawn(RunawayCartId, OverworldLayouts.CalhasId, "encosta_dos_zimbros", OverworldLayouts.CarrinhoDesgovernadoArena,
                OverworldLayouts.TetoWorldOffset, 80, EnemyProjectileKind.LanternBolt, true, RegionBossSignature.RunawayRun, neverDies: true),
            new RegionBossSpawn(GreatAlmasId, OverworldLayouts.GeleiraId, "lago_turquesa", OverworldLayouts.GrandeAlmasArena,
                OverworldLayouts.TetoWorldOffset, 86, EnemyProjectileKind.LureGlob, true, RegionBossSignature.SnowSparks, neverDies: true),
            new RegionBossSpawn(UnleapedCarpId, OverworldLayouts.CanalLanternasMapId, "canal_das_lanternas", OverworldLayouts.CarpaQueNaoPulouArena,
                OverworldLayouts.MingchengWorldOffset, 110, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.FailedLeap, neverDies: true),
            new RegionBossSpawn(ForgottenSpinnerId, MapLayouts.AmoreirasId, "amoreiras", OverworldLayouts.FiandeiraEsquecidaArena,
                OverworldLayouts.ZarafshanWorldOffset, 24, EnemyProjectileKind.Feather, false, RegionBossSignature.TautThread, neverDies: true),
            new RegionBossSpawn(WellDrinkingRootId, OverworldLayouts.KarezId, "galerias_do_karez", OverworldLayouts.RaizQueBebeuOPocoArena,
                OverworldLayouts.AreiasWorldOffset, 26, EnemyProjectileKind.LureGlob, false, RegionBossSignature.RootTrail, neverDies: true),
            new RegionBossSpawn(GreatSmudgeId, OverworldLayouts.AbrigosId, "abrigos_pintados", OverworldLayouts.GrandeBorraoArena,
                OverworldLayouts.KaggenWorldOffset, 32, EnemyProjectileKind.LanternBolt, true, RegionBossSignature.StolenColours, neverDies: true),
            new RegionBossSpawn(NinkiNankaId, OverworldLayouts.NinkiNankaId, "brejo_das_palmeiras", OverworldLayouts.NinkiNankaArena,
                OverworldLayouts.DeltaWorldOffset, 82, EnemyProjectileKind.LureGlob, false, RegionBossSignature.ShyGaze, neverDies: true),
            new RegionBossSpawn(NianId, OverworldLayouts.MuralhaMapId, "portao_do_nian", OverworldLayouts.NianArena,
                OverworldLayouts.MingchengWorldOffset, 125, EnemyProjectileKind.Ember, true, RegionBossSignature.NoiseFright, neverDies: true),
            new RegionBossSpawn(OldKurganGriffinId, OverworldLayouts.KurgansMapId, "kurgans_de_ouro", OverworldLayouts.GrifoVelhoArena,
                OverworldLayouts.NartesWorldOffset, 114, EnemyProjectileKind.Feather, false, RegionBossSignature.HoardGuard, neverDies: true),
            new RegionBossSpawn(MistElderId, MapLayouts.GarajonayId, "garajonay", OverworldLayouts.VelhoDaNevoaArena,
                OverworldLayouts.ZarafshanWorldOffset, 26, EnemyProjectileKind.LureGlob, false, RegionBossSignature.MistStare, neverDies: true),
            new RegionBossSpawn(DuneChoirId, OverworldLayouts.DunasId, "dunas_que_cantam", OverworldLayouts.CoralDaDunaArena,
                OverworldLayouts.AreiasWorldOffset, 28, EnemyProjectileKind.Ember, false, RegionBossSignature.DuneVerses, neverDies: true),
            new RegionBossSpawn(GreedyBottlerId, OverworldLayouts.MercadoTuneisMapId, "mercado_dos_tuneis", OverworldLayouts.EngarrafadorGulosoArena,
                OverworldLayouts.LimiarWorldOffset, 42, EnemyProjectileKind.LanternBolt, true, RegionBossSignature.StaleAir, neverDies: true),
            new RegionBossSpawn(LightFisherId, OverworldLayouts.TarchonMapId, "bancos_de_lama", OverworldLayouts.PescadorDeLuzesArena,
                OverworldLayouts.DeltaWorldOffset, 78, EnemyProjectileKind.LureGlob, true, RegionBossSignature.LeadCast, neverDies: true),
            new RegionBossSpawn(VaneLordId, OverworldLayouts.ForteVentosId, "torres_de_cata_vento", OverworldLayouts.SenhorDoCataVentoArena,
                OverworldLayouts.TetoWorldOffset, 92, EnemyProjectileKind.Feather, false, RegionBossSignature.VaneGust, neverDies: true),
            new RegionBossSpawn(LittleEclipseId, OverworldLayouts.TorresBrancasMapId, "colinas_dos_espelhos", OverworldLayouts.PequenoEclipseArena,
                OverworldLayouts.MingchengWorldOffset, 102, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.GrowingEclipse, neverDies: true),
        };

        /// <summary>Every boss the <see cref="RegionBossDirector"/> keeps: the world bosses (<see cref="AllSpawns"/>) and the map bosses (<see cref="MapSpawns"/>).</summary>
        /// <remarks>Built on first use: the order of the static initializers of the two files of this partial class is not defined.</remarks>
        public static IReadOnlyList<RegionBossSpawn> DirectorSpawns => _directorSpawns ??= Concat(AllSpawns, MapSpawns);

        private static RegionBossSpawn[] _directorSpawns;

        public static bool IsMapBoss(string enemyId)
        {
            foreach (RegionBossSpawn spawn in MapSpawns)
            {
                if (spawn.EnemyId == enemyId)
                {
                    return true;
                }
            }

            return false;
        }

        // ---- O Cortador de Cordas (55): the rope snaps under whoever is walking ----

        public const float RopeCutIntervalSeconds = 9f;
        public const float RopeCutWindupSeconds = 2f;
        public const float RopeCutDamageFactor = 1.4f;

        /// <summary>The last stretch of the windup in which the hero must already be standing still (the ficha's "pare e segure").</summary>
        public const float RopeCutHoldSeconds = 0.6f;

        /// <summary>How far the hero may drift in that stretch and still count as holding on.</summary>
        public const float RopeCutHoldTolerance = 0.5f;

        // ---- O Sino que Não Para (56): the toll and its echo ----

        public const float TollIntervalSeconds = 9f;
        public const float TollWindupSeconds = 1.8f;
        public const float TollDamageFactor = 1.2f;
        public const float TollRadius = 3.5f;

        // ---- A Morsa-Rainha (60): the belly flop ----

        public const float FlopIntervalSeconds = 11f;
        public const float FlopWindupSeconds = 2f;
        public const float FlopRadius = 7f;
        public const float FlopCentreFactor = 1.6f;
        public const float FlopEdgeFactor = 0.3f;

        // ---- O Mamute Avô (62): the ceiling of the gallery comes down ----

        public const float CeilingIntervalSeconds = 10f;
        public const float CeilingWindupSeconds = 2.2f;
        public const float CeilingDamageFactor = 1.4f;
        public const float CeilingStoneRadius = 2.2f;

        /// <summary>How far from the marked spot the four other stones fall (north, south, east, west).</summary>
        public const float CeilingStoneSpacing = 4.5f;

        // ---- O Rei Shisa do Telhado (66): the two mouths ----

        public const float ShisaIntervalSeconds = 10f;
        public const float ShisaWindupSeconds = 2f;
        public const float ShisaDamageFactor = 1.3f;
        public const float ShisaReach = 6f;

        // ---- O Mestre das Nove Fornalhas (69): the strokes that heat the forge ----

        public const float ForgeIntervalSeconds = 8f;
        public const float ForgeWindupSeconds = 1.8f;
        public const float ForgeDamageFactor = 1.1f;
        public const float ForgeRadius = 3.5f;

        /// <summary>What each stroke since the last vent adds to the next one.</summary>
        public const float ForgeHeatPerStroke = 0.08f;

        /// <summary>The ninth stroke vents the nine furnaces and the heat starts over.</summary>
        public const int ForgeFurnaces = 9;

        // ---- O Polvo das Monções (25): eight arms like spokes ----

        public const float ArmsIntervalSeconds = 10f;
        public const float ArmsWindupSeconds = 2f;
        public const float ArmsDamageFactor = 1.3f;
        public const int OctopusArms = 8;
        public const float ArmLength = 9f;

        /// <summary>Half the width of an arm slapping the sand: a hero this close to its line is under it.</summary>
        public const float ArmHalfWidth = 1.2f;

        /// <summary>Each slam the arms come down half the gap round from the last (degrees).</summary>
        public const float ArmTurnDegrees = 22.5f;

        // ---- O Touro de Fumaça (26): the snort, all round but behind ----

        public const float SnortIntervalSeconds = 10f;
        public const float SnortWindupSeconds = 1.8f;
        public const float SnortDamageFactor = 1.3f;
        public const float SnortRadius = 8f;

        /// <summary>Half the opening (degrees) of the clean air behind the bull, where Aratta's herd walks.</summary>
        public const float SnortRearHalfAngle = 45f;

        // ---- A Ponte que Esqueceu (32): every other plank ----

        public const float PlanksIntervalSeconds = 10f;
        public const float PlanksWindupSeconds = 2.2f;
        public const float PlanksDamageFactor = 1.35f;

        /// <summary>The width of one plank (a row across the line from the bridge to the hero's spot).</summary>
        public const float PlankWidth = 2.5f;

        /// <summary>Half the length of a plank: sideways the whole row falls together.</summary>
        public const float PlankHalfLength = 6f;

        /// <summary>How far before and past the hero's spot the planks go.</summary>
        public const float PlanksReach = 7.5f;

        // ---- O Carrinho Desgovernado (37): the run-up ----

        public const float RunawayIntervalSeconds = 9f;
        public const float RunawayWindupSeconds = 2f;
        public const float RunawayRadius = 3f;
        public const float RunawayMinFactor = 0.7f;
        public const float RunawayMaxFactor = 1.8f;

        /// <summary>The run-up (m from the cart to the hero's spot) at which it hits as hard as it can.</summary>
        public const float RunawayFullRunUp = 18f;

        // ---- O Grande Almas (38): the sparks hidden in the snow ----

        public const float SnowIntervalSeconds = 11f;
        public const float SnowWindupSeconds = 2.4f;
        public const float SnowDamageFactor = 1.3f;
        public const float SnowBurstRadius = 8f;
        public const int SnowSparkCount = 3;
        public const float SnowSparkDistance = 4.5f;

        /// <summary>Standing this close to a hidden spark, the burst passes you by.</summary>
        public const float SnowSparkRadius = 1.6f;

        /// <summary>Each throw the Almas hides the sparks this many degrees further round.</summary>
        public const float SnowSparkTurnDegrees = 40f;

        // ---- A Carpa que Não Pulou (43): short, then past ----

        public const float LeapIntervalSeconds = 10f;
        public const float LeapWindupSeconds = 2f;
        public const float LeapDamageFactor = 1.4f;
        public const float LeapSplashRadius = 3f;

        /// <summary>A short leap lands this far along the way from the carp to the hero's spot.</summary>
        public const float LeapShortFraction = 0.5f;

        /// <summary>A long leap lands this many metres past the hero's spot.</summary>
        public const float LeapOvershoot = 4.5f;

        // ---- A Fiandeira Esquecida (04): the thread pulled taut ----

        public const float ThreadIntervalSeconds = 10f;
        public const float ThreadWindupSeconds = 2.2f;
        public const float ThreadDamageFactor = 1.3f;

        /// <summary>How much closer than when the thread was tied the hero must be for it to hang slack when she pulls.</summary>
        public const float ThreadSlack = 2f;

        /// <summary>Right at her legs there is no thread to pull.</summary>
        public const float ThreadHugDistance = 2.5f;

        /// <summary>The longest thread she spins: a hero further than this when she ties it has nothing tied to them.</summary>
        public const float ThreadReach = 14f;

        // ---- A Raiz que Bebeu o Poço (09): the roots come up on the hero's steps ----

        public const float RootIntervalSeconds = 10f;
        public const float RootWindupSeconds = 2.4f;
        public const float RootDamageFactor = 1.3f;

        /// <summary>How often the root marks the hero's step during the warning (the first mark is where the warning began).</summary>
        public const float RootStepSeconds = 0.4f;

        /// <summary>How near a marked step the root coming up reaches.</summary>
        public const float RootRadius = 1.6f;

        // ---- O Grande Borrão (15): the colours it ate ----

        public const float SmudgeIntervalSeconds = 10f;
        public const float SmudgeWindupSeconds = 2f;
        public const float SmudgeDamageFactor = 1.3f;

        /// <summary>The width of each colour's band round it: ochre next to it, red in the middle, white outside.</summary>
        public const float SmudgeBandWidth = 3f;

        public const int SmudgeColours = 3;

        // ---- Ninki Nanka (34): don't look ----

        public const float GazeIntervalSeconds = 9f;
        public const float GazeWindupSeconds = 1.8f;
        public const float GazeDamageFactor = 1.35f;
        public const float GazeReach = 14f;

        /// <summary>How far off her (degrees, either side) a hero may be facing and still be looking at her when she shines.</summary>
        public const float GazeHalfAngle = 60f;

        // ---- Nian (45): noise scares it off ----

        public const float NianIntervalSeconds = 10f;
        public const float NianWindupSeconds = 2.4f;
        public const float NianDamageFactor = 1.5f;
        public const float NianPounceRadius = 4.5f;

        /// <summary>The share of its health the hero must knock off it during the warning for the noise to frighten it out of the pounce.</summary>
        public const float NianNoiseFraction = 0.04f;

        // ---- O Grifo Velho do Kurgan (48): the hoard ----

        public const float HoardIntervalSeconds = 11f;
        public const float HoardWindupSeconds = 2f;
        public const float HoardDamageFactor = 1.4f;

        /// <summary>The gold he keeps lies within this many metres of where he was found: the stone ring of his arena.</summary>
        public const float HoardRadius = 8f;

        // ---- O Velho da Névoa (05): don't lose sight of him in the mist ----

        public const float MistIntervalSeconds = 10f;
        public const float MistWindupSeconds = 2.2f;
        public const float MistDamageFactor = 1.3f;

        /// <summary>How far his snap out of the mist reaches.</summary>
        public const float MistReach = 16f;

        /// <summary>How far off him (degrees, either side) a hero may be facing and still have him in sight when the mist parts.</summary>
        public const float MistSightHalfAngle = 80f;

        // ---- O Coral da Duna (10): each verse further ----

        public const float VerseIntervalSeconds = 10f;
        public const float VerseWindupSeconds = 2f;
        public const float VerseDamageFactor = 1.3f;

        /// <summary>How far the first verse makes the sand burst; each verse after reaches <see cref="VerseStep"/> m further, then it starts again.</summary>
        public const float VerseFirstRadius = 4f;

        public const float VerseStep = 3f;

        public const int DuneVerseCount = 3;

        // ---- O Engarrafador Guloso (22): behind the bottles ----

        public const float StaleIntervalSeconds = 11f;
        public const float StaleWindupSeconds = 2.4f;
        public const float StaleDamageFactor = 1.3f;

        /// <summary>How far the stale air he lets out reaches.</summary>
        public const float StaleReach = 14f;

        public const int BottleCount = 3;

        /// <summary>The bottles stand on a ring of this radius round where he uncorked them.</summary>
        public const float BottleRingRadius = 5f;

        /// <summary>Each uncorking turns the ring of bottles this many degrees.</summary>
        public const float BottleTurnDegrees = 40f;

        /// <summary>How far to the side of the line from him a bottle still shelters a hero standing behind it.</summary>
        public const float BottleShelter = 1.5f;

        // ---- O Pescador de Luzes (33): the lure leads the fish ----

        public const float LureIntervalSeconds = 10f;
        public const float LureWindupSeconds = 2.4f;
        public const float LureDamageFactor = 1.35f;

        /// <summary>How long before the cast he takes his first look at where the hero is going, and his second.</summary>
        public const float LureFirstLookSeconds = 1.2f;

        public const float LureSecondLookSeconds = 0.8f;

        /// <summary>He casts as far ahead of the second look as the hero would go before the cast at the pace seen between the looks.</summary>
        public static float LureLeadFactor => LureSecondLookSeconds / (LureFirstLookSeconds - LureSecondLookSeconds);

        /// <summary>The farthest ahead he casts.</summary>
        public const float LureMaxLead = 8f;

        /// <summary>How near the lure the hook catches.</summary>
        public const float LureRadius = 2.5f;

        // ---- O Senhor do Cata-Vento (39): upwind is safe ----

        public const float GustIntervalSeconds = 10f;
        public const float GustWindupSeconds = 2f;
        public const float GustDamageFactor = 1.35f;
        public const float GustReach = 16f;

        /// <summary>The vane's own width: a hero this far upwind of his line is still in the gust.</summary>
        public const float GustLee = 1f;

        public const int VaneWinds = 4;

        // ---- O Pequeno Eclipse (42): a wider slice each time ----

        public const float EclipseIntervalSeconds = 11f;
        public const float EclipseWindupSeconds = 2.2f;
        public const float EclipseDamageFactor = 1.4f;

        /// <summary>How far its shadow falls.</summary>
        public const float EclipseReach = 12f;

        /// <summary>Each cover adds this many degrees either side of the hero's bearing: 45, 90, 135, then the whole round (180), then again.</summary>
        public const float EclipseStepDegrees = 45f;

        public const int EclipsePhases = 4;

        private static float MapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.RopeCut: return RopeCutIntervalSeconds;
                case RegionBossSignature.EchoToll: return TollIntervalSeconds;
                case RegionBossSignature.BellyFlop: return FlopIntervalSeconds;
                case RegionBossSignature.CeilingFall: return CeilingIntervalSeconds;
                case RegionBossSignature.TwoMouths: return ShisaIntervalSeconds;
                case RegionBossSignature.MonsoonArms: return ArmsIntervalSeconds;
                case RegionBossSignature.HerdSnort: return SnortIntervalSeconds;
                case RegionBossSignature.ForgottenPlanks: return PlanksIntervalSeconds;
                case RegionBossSignature.RunawayRun: return RunawayIntervalSeconds;
                case RegionBossSignature.SnowSparks: return SnowIntervalSeconds;
                case RegionBossSignature.FailedLeap: return LeapIntervalSeconds;
                case RegionBossSignature.TautThread: return ThreadIntervalSeconds;
                case RegionBossSignature.RootTrail: return RootIntervalSeconds;
                case RegionBossSignature.StolenColours: return SmudgeIntervalSeconds;
                case RegionBossSignature.ShyGaze: return GazeIntervalSeconds;
                case RegionBossSignature.NoiseFright: return NianIntervalSeconds;
                case RegionBossSignature.HoardGuard: return HoardIntervalSeconds;
                case RegionBossSignature.MistStare: return MistIntervalSeconds;
                case RegionBossSignature.DuneVerses: return VerseIntervalSeconds;
                case RegionBossSignature.StaleAir: return StaleIntervalSeconds;
                case RegionBossSignature.LeadCast: return LureIntervalSeconds;
                case RegionBossSignature.VaneGust: return GustIntervalSeconds;
                case RegionBossSignature.GrowingEclipse: return EclipseIntervalSeconds;
                default: return ForgeIntervalSeconds;
            }
        }

        private static float MapBossWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.RopeCut: return RopeCutWindupSeconds;
                case RegionBossSignature.EchoToll: return TollWindupSeconds;
                case RegionBossSignature.BellyFlop: return FlopWindupSeconds;
                case RegionBossSignature.CeilingFall: return CeilingWindupSeconds;
                case RegionBossSignature.TwoMouths: return ShisaWindupSeconds;
                case RegionBossSignature.MonsoonArms: return ArmsWindupSeconds;
                case RegionBossSignature.HerdSnort: return SnortWindupSeconds;
                case RegionBossSignature.ForgottenPlanks: return PlanksWindupSeconds;
                case RegionBossSignature.RunawayRun: return RunawayWindupSeconds;
                case RegionBossSignature.SnowSparks: return SnowWindupSeconds;
                case RegionBossSignature.FailedLeap: return LeapWindupSeconds;
                case RegionBossSignature.TautThread: return ThreadWindupSeconds;
                case RegionBossSignature.RootTrail: return RootWindupSeconds;
                case RegionBossSignature.StolenColours: return SmudgeWindupSeconds;
                case RegionBossSignature.ShyGaze: return GazeWindupSeconds;
                case RegionBossSignature.NoiseFright: return NianWindupSeconds;
                case RegionBossSignature.HoardGuard: return HoardWindupSeconds;
                case RegionBossSignature.MistStare: return MistWindupSeconds;
                case RegionBossSignature.DuneVerses: return VerseWindupSeconds;
                case RegionBossSignature.StaleAir: return StaleWindupSeconds;
                case RegionBossSignature.LeadCast: return LureWindupSeconds;
                case RegionBossSignature.VaneGust: return GustWindupSeconds;
                case RegionBossSignature.GrowingEclipse: return EclipseWindupSeconds;
                default: return ForgeWindupSeconds;
            }
        }

        /// <summary>
        /// Does the cut rope throw down a hero who was at <paramref name="heldAt"/> when the last <see cref="RopeCutHoldSeconds"/> began and is at
        /// <paramref name="hero"/> when it snaps? Only one who kept walking: stopping and holding on is the way across.
        /// </summary>
        public static bool RopeCutHits(Vector2 heldAt, Vector2 hero) => (hero - heldAt).sqrMagnitude > RopeCutHoldTolerance * RopeCutHoldTolerance;

        /// <summary>
        /// Does the toll on <paramref name="mark"/> reach a hero at <paramref name="hero"/>? The echo of the previous toll (<paramref name="echo"/>,
        /// null on the first) rings again at the same time, so the spot the hero fled to last time is no refuge.
        /// </summary>
        public static bool TollHits(Vector2 mark, Vector2? echo, Vector2 hero)
        {
            float r2 = TollRadius * TollRadius;
            return (hero - mark).sqrMagnitude <= r2 || (echo.HasValue && (hero - echo.Value).sqrMagnitude <= r2);
        }

        /// <summary>How hard the flop on the marked spot hits a hero <paramref name="distance"/> m from it: <see cref="FlopCentreFactor"/> at the middle, down to <see cref="FlopEdgeFactor"/> at the edge, 0 beyond.</summary>
        public static float FlopDamageFactor(float distance)
        {
            if (distance > FlopRadius)
            {
                return 0f;
            }

            return Mathf.Lerp(FlopCentreFactor, FlopEdgeFactor, Mathf.Clamp01(distance / FlopRadius));
        }

        /// <summary>The five spots the gallery's stones fall on round <paramref name="mark"/>: the mark and four more in a cross.</summary>
        public static Vector2[] CeilingStones(Vector2 mark) => new[]
        {
            mark,
            mark + new Vector2(CeilingStoneSpacing, 0f),
            mark - new Vector2(CeilingStoneSpacing, 0f),
            mark + new Vector2(0f, CeilingStoneSpacing),
            mark - new Vector2(0f, CeilingStoneSpacing),
        };

        /// <summary>Is a hero at <paramref name="hero"/> under one of the stones that fall round <paramref name="mark"/>? A diagonal step is enough.</summary>
        public static bool CeilingFallHits(Vector2 mark, Vector2 hero)
        {
            foreach (Vector2 stone in CeilingStones(mark))
            {
                if ((hero - stone).sqrMagnitude <= CeilingStoneRadius * CeilingStoneRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The mouth of roar number <paramref name="roar"/> of the fight (0 first): open, shut, open… in turn.</summary>
        public static ShisaMouth ShisaMouthFor(int roar) => (Mathf.Abs(roar) % 2) == 0 ? ShisaMouth.Open : ShisaMouth.Closed;

        /// <summary>Does the roar of <paramref name="mouth"/> reach a hero <paramref name="distance"/> m from the Shisa King?</summary>
        public static bool ShisaHits(ShisaMouth mouth, float distance) => mouth == ShisaMouth.Open ? distance > ShisaReach : distance <= ShisaReach;

        /// <summary>The heat of stroke number <paramref name="stroke"/> of the fight (0 first): +<see cref="ForgeHeatPerStroke"/> per stroke since the last vent; every ninth vents.</summary>
        public static float ForgeHeatMultiplier(int stroke) => 1f + ForgeHeatPerStroke * (Mathf.Max(0, stroke) % ForgeFurnaces);

        /// <summary>Does the stroke on <paramref name="mark"/> reach a hero at <paramref name="hero"/>?</summary>
        public static bool ForgeStrokeHits(Vector2 mark, Vector2 hero) => (hero - mark).sqrMagnitude <= ForgeRadius * ForgeRadius;

        /// <summary>
        /// Is a hero at <paramref name="hero"/> under one of the arms of slam number <paramref name="slam"/> (0 first) of the octopus at
        /// <paramref name="octopus"/>? <see cref="OctopusArms"/> arms <see cref="ArmLength"/> long, every 45°, turned half a gap on every other
        /// slam: close in, under the body, everything is hit; a few steps out, between two arms, nothing is; the next slam the gaps are arms.
        /// </summary>
        public static bool OctopusArmsHit(int slam, Vector2 octopus, Vector2 hero)
        {
            Vector2 offset = hero - octopus;
            float distance = offset.magnitude;
            if (distance > ArmLength)
            {
                return false;
            }

            if (distance < 0.01f)
            {
                return true;
            }

            float spoke = 360f / OctopusArms;
            float turn = (Mathf.Abs(slam) % 2) * ArmTurnDegrees;
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg - turn;
            float fromArm = Mathf.Abs(Mathf.Repeat(angle + spoke * 0.5f, spoke) - spoke * 0.5f);
            return distance * Mathf.Sin(fromArm * Mathf.Deg2Rad) <= ArmHalfWidth;
        }

        /// <summary>
        /// Does the Touro de Fumaça's snort reach a hero at <paramref name="hero"/>, the bull at <paramref name="bull"/> facing <paramref name="facing"/>?
        /// Everyone within <see cref="SnortRadius"/> but the ones straight behind it (<see cref="SnortRearHalfAngle"/> either side).
        /// </summary>
        public static bool SnortHits(Vector2 bull, Vector2 facing, Vector2 hero)
        {
            Vector2 offset = hero - bull;
            if (offset.sqrMagnitude > SnortRadius * SnortRadius)
            {
                return false;
            }

            if (offset.sqrMagnitude < 0.25f || facing.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector2.Angle(-facing, offset) > SnortRearHalfAngle;
        }

        /// <summary>
        /// Does stomp number <paramref name="stomp"/> (0 first) of the Ponte que Esqueceu, from <paramref name="bridge"/> at <paramref name="mark"/>,
        /// drop a hero at <paramref name="hero"/>? The planks lie across the line from the bridge to the mark, <see cref="PlankWidth"/> each, the mark's
        /// own plank number 0: on even stomps the even planks are forgotten, on odd ones the odd. A step toward or away from the bridge saves you; a step
        /// sideways does not (a plank is twice <see cref="PlankHalfLength"/> long).
        /// </summary>
        public static bool ForgottenPlankHits(int stomp, Vector2 bridge, Vector2 mark, Vector2 hero)
        {
            Vector2 axis = mark - bridge;
            axis = axis.sqrMagnitude < 1e-6f ? Vector2.up : axis.normalized;
            Vector2 offset = hero - mark;
            float along = Vector2.Dot(offset, axis);
            float across = Vector2.Dot(offset, new Vector2(-axis.y, axis.x));
            if (Mathf.Abs(along) > PlanksReach || Mathf.Abs(across) > PlankHalfLength)
            {
                return false;
            }

            int plank = Mathf.Abs(Mathf.RoundToInt(along / PlankWidth));
            return plank % 2 == Mathf.Abs(stomp) % 2;
        }

        /// <summary>How hard the runaway cart hits after <paramref name="runUp"/> m of run-up: <see cref="RunawayMinFactor"/> from a standstill up to <see cref="RunawayMaxFactor"/> at <see cref="RunawayFullRunUp"/>.</summary>
        public static float RunawayDamageFactor(float runUp) => Mathf.Lerp(RunawayMinFactor, RunawayMaxFactor, Mathf.Clamp01(runUp / RunawayFullRunUp));

        /// <summary>Does the cart rolling onto <paramref name="mark"/> run over a hero at <paramref name="hero"/>?</summary>
        public static bool RunawayHits(Vector2 mark, Vector2 hero) => (hero - mark).sqrMagnitude <= RunawayRadius * RunawayRadius;

        /// <summary>The <see cref="SnowSparkCount"/> sparks the Almas at <paramref name="almas"/> hides for throw number <paramref name="throwIndex"/> (0 first), turned <see cref="SnowSparkTurnDegrees"/> further each throw.</summary>
        public static Vector2[] SnowSparkSpots(Vector2 almas, int throwIndex)
        {
            var spots = new Vector2[SnowSparkCount];
            float turn = (Mathf.Abs(throwIndex) * SnowSparkTurnDegrees) % 360f;
            for (int i = 0; i < SnowSparkCount; i++)
            {
                float a = (turn + i * 360f / SnowSparkCount) * Mathf.Deg2Rad;
                spots[i] = almas + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * SnowSparkDistance;
            }

            return spots;
        }

        /// <summary>Does throw number <paramref name="throwIndex"/> of the Almas at <paramref name="almas"/> bury a hero at <paramref name="hero"/>? Everyone within <see cref="SnowBurstRadius"/> who is not standing on a hidden spark.</summary>
        public static bool SnowBurstHits(Vector2 almas, int throwIndex, Vector2 hero)
        {
            if ((hero - almas).sqrMagnitude > SnowBurstRadius * SnowBurstRadius)
            {
                return false;
            }

            foreach (Vector2 spark in SnowSparkSpots(almas, throwIndex))
            {
                if ((hero - spark).sqrMagnitude <= SnowSparkRadius * SnowSparkRadius)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Does leap number <paramref name="leap"/> (0 first) of the carp fall short? Short, past, short… in turn: it never lands where it aimed.</summary>
        public static bool LeapFallsShort(int leap) => Mathf.Abs(leap) % 2 == 0;

        /// <summary>Where leap number <paramref name="leap"/> of the carp at <paramref name="carp"/>, aimed at <paramref name="mark"/>, comes down.</summary>
        public static Vector2 LeapLanding(int leap, Vector2 carp, Vector2 mark)
        {
            Vector2 way = mark - carp;
            if (LeapFallsShort(leap))
            {
                return carp + way * LeapShortFraction;
            }

            return way.sqrMagnitude < 1e-6f ? mark : mark + way.normalized * LeapOvershoot;
        }

        /// <summary>Does leap number <paramref name="leap"/> splash a hero at <paramref name="hero"/>?</summary>
        public static bool LeapHits(int leap, Vector2 carp, Vector2 mark, Vector2 hero) =>
            (hero - LeapLanding(leap, carp, mark)).sqrMagnitude <= LeapSplashRadius * LeapSplashRadius;

        /// <summary>
        /// Does the Fiandeira's thread, tied to a hero <paramref name="tiedAt"/> m from her when the warning began, throw down that hero now
        /// <paramref name="distanceNow"/> m away? Only if it is taut: the hero did not come <see cref="ThreadSlack"/> m closer, nor is hugging her legs.
        /// Running away is the worst of it; a hero beyond <see cref="ThreadReach"/> had no thread.
        /// </summary>
        public static bool TautThreadHits(float tiedAt, float distanceNow)
        {
            if (tiedAt > ThreadReach)
            {
                return false;
            }

            return distanceNow > ThreadHugDistance && distanceNow > tiedAt - ThreadSlack;
        }

        /// <summary>Is it time to mark another of the hero's steps, <paramref name="sinceLastStep"/> s after the last one?</summary>
        public static bool RootMarksStep(float sinceLastStep) => sinceLastStep >= RootStepSeconds;

        /// <summary>The most steps the root can mark in one warning (the first included).</summary>
        public static int RootMaxSteps => Mathf.FloorToInt(RootWindupSeconds / RootStepSeconds) + 1;

        /// <summary>Does the root come up under a hero at <paramref name="hero"/>, having marked the steps <paramref name="steps"/> during the warning? Only new ground is safe.</summary>
        public static bool RootTrailHits(IReadOnlyList<Vector2> steps, Vector2 hero)
        {
            if (steps == null)
            {
                return false;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                if ((hero - steps[i]).sqrMagnitude <= RootRadius * RootRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The colour the Borrão has not eaten yet on smear number <paramref name="smear"/> of the fight (0 first): ochre, red, white, ochre…</summary>
        public static SmudgeColour SmudgeColourFor(int smear) => (SmudgeColour)(Mathf.Abs(smear) % SmudgeColours);

        /// <summary>
        /// Does the smear reach a hero <paramref name="distance"/> m from the Borrão when <paramref name="clean"/> is the colour left? It covers the three bands
        /// of <see cref="SmudgeBandWidth"/> round it but the clean one; beyond the last band is safe too.
        /// </summary>
        public static bool SmudgeHits(SmudgeColour clean, float distance)
        {
            if (distance > SmudgeBandWidth * SmudgeColours)
            {
                return false;
            }

            int band = Mathf.Min(SmudgeColours - 1, Mathf.FloorToInt(Mathf.Max(0f, distance) / SmudgeBandWidth));
            return band != (int)clean;
        }

        /// <summary>
        /// Is a hero at <paramref name="hero"/> facing <paramref name="heroFacing"/> looking at Ninki Nanka (at <paramref name="dragon"/>) when she shines?
        /// Within <see cref="GazeReach"/>, facing her within <see cref="GazeHalfAngle"/>: turning one's back is the way; nose to nose there is no looking away.
        /// </summary>
        public static bool GazeHits(Vector2 dragon, Vector2 hero, Vector2 heroFacing)
        {
            Vector2 toHer = dragon - hero;
            if (toHer.sqrMagnitude > GazeReach * GazeReach)
            {
                return false;
            }

            if (toHer.sqrMagnitude < 0.25f || heroFacing.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector2.Angle(heroFacing, toHer) <= GazeHalfAngle;
        }

        /// <summary>Did the hero make enough noise to frighten Nian, knocking it from <paramref name="healthAtWarning"/> to <paramref name="healthNow"/> of <paramref name="maxHealth"/> during the warning?</summary>
        public static bool NianTakesFright(float healthAtWarning, float healthNow, float maxHealth) =>
            maxHealth > 0f && healthAtWarning - healthNow >= maxHealth * NianNoiseFraction;

        /// <summary>Does Nian's pounce on <paramref name="mark"/> land on a hero at <paramref name="hero"/>? Never once it took fright.</summary>
        public static bool NianPounceHits(bool frightened, Vector2 mark, Vector2 hero) =>
            !frightened && (hero - mark).sqrMagnitude <= NianPounceRadius * NianPounceRadius;

        /// <summary>Is a hero at <paramref name="hero"/> standing on the old griffin's hoard (at <paramref name="hoard"/>)? Wherever the griffin is.</summary>
        public static bool HoardHits(Vector2 hoard, Vector2 hero) => (hero - hoard).sqrMagnitude <= HoardRadius * HoardRadius;

        /// <summary>
        /// Does O Velho da Névoa (at <paramref name="elder"/>) snap at a hero at <paramref name="hero"/> facing <paramref name="heroFacing"/> when the mist parts?
        /// Within <see cref="MistReach"/>, unless the hero kept him in sight (facing him within <see cref="MistSightHalfAngle"/>). No facing is no sight;
        /// right at his snout there is no mist to hide in.
        /// </summary>
        public static bool MistSnapHits(Vector2 elder, Vector2 hero, Vector2 heroFacing)
        {
            Vector2 toHim = elder - hero;
            if (toHim.sqrMagnitude > MistReach * MistReach || toHim.sqrMagnitude < 0.25f)
            {
                return false;
            }

            if (heroFacing.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector2.Angle(heroFacing, toHim) > MistSightHalfAngle;
        }

        /// <summary>Which verse O Coral da Duna sings on song number <paramref name="song"/> of the fight (0 first): 0, 1, 2, 0…</summary>
        public static int DuneVerseOf(int song) => Mathf.Abs(song) % DuneVerseCount;

        /// <summary>How far verse <paramref name="verse"/> makes the sand burst round the worm: 4, 7, 10 m.</summary>
        public static float VerseRadius(int verse) => VerseFirstRadius + VerseStep * DuneVerseOf(verse);

        /// <summary>Does verse <paramref name="verse"/> reach a hero <paramref name="distance"/> m from the worm? Everything inside its radius; beyond is safe.</summary>
        public static bool VerseHits(int verse, float distance) => distance <= VerseRadius(verse);

        /// <summary>Where the three great bottles stand on uncorking number <paramref name="uncorking"/> (0 first), round <paramref name="origin"/>: 120° apart, turning 40° each time.</summary>
        public static Vector2[] BottleSpots(Vector2 origin, int uncorking)
        {
            var spots = new Vector2[BottleCount];
            float start = Mathf.Abs(uncorking) * BottleTurnDegrees;
            for (int i = 0; i < BottleCount; i++)
            {
                float a = (start + i * 360f / BottleCount) * Mathf.Deg2Rad;
                spots[i] = origin + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * BottleRingRadius;
            }

            return spots;
        }

        /// <summary>
        /// Does the stale air O Engarrafador Guloso blows out from <paramref name="origin"/> reach a hero at <paramref name="hero"/>? Within <see cref="StaleReach"/>,
        /// unless one of <paramref name="bottles"/> stands between them (nearer him than the hero, within <see cref="BottleShelter"/> of the line).
        /// </summary>
        public static bool StaleAirHits(Vector2 origin, IReadOnlyList<Vector2> bottles, Vector2 hero)
        {
            Vector2 line = hero - origin;
            float reach = line.magnitude;
            if (reach > StaleReach)
            {
                return false;
            }

            if (bottles == null || reach < 0.01f)
            {
                return true;
            }

            Vector2 along = line / reach;
            for (int i = 0; i < bottles.Count; i++)
            {
                Vector2 toBottle = bottles[i] - origin;
                float ahead = Vector2.Dot(toBottle, along);
                if (ahead <= 0f || ahead >= reach)
                {
                    continue;
                }

                if ((toBottle - along * ahead).sqrMagnitude <= BottleShelter * BottleShelter)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Where O Pescador de Luzes casts his lure, having seen the hero at <paramref name="firstLook"/> and then at <paramref name="secondLook"/>: as far
        /// ahead of the second look as the hero would go before the cast at the same pace (<see cref="LureLeadFactor"/>), at most <see cref="LureMaxLead"/> m.
        /// </summary>
        public static Vector2 LureSpot(Vector2 firstLook, Vector2 secondLook)
        {
            Vector2 lead = (secondLook - firstLook) * LureLeadFactor;
            return secondLook + Vector2.ClampMagnitude(lead, LureMaxLead);
        }

        /// <summary>Does the lure cast on <paramref name="lure"/> hook a hero at <paramref name="hero"/>? Only whoever kept going as they were.</summary>
        public static bool LureHits(Vector2 lure, Vector2 hero) => (hero - lure).sqrMagnitude <= LureRadius * LureRadius;

        /// <summary>Where the wind blows from on gust number <paramref name="gust"/> of the fight (0 first): north, east, south, west, north…</summary>
        public static VaneWind VaneWindFor(int gust) => (VaneWind)(Mathf.Abs(gust) % VaneWinds);

        /// <summary>The flat direction the wind blows toward when it comes from <paramref name="from"/> (+y is north).</summary>
        public static Vector2 WindBlowsToward(VaneWind from)
        {
            switch (from)
            {
                case VaneWind.North: return Vector2.down;
                case VaneWind.East: return Vector2.left;
                case VaneWind.South: return Vector2.up;
                default: return Vector2.right;
            }
        }

        /// <summary>
        /// Does the gust of O Senhor do Cata-Vento (at <paramref name="vane"/>), with the wind from <paramref name="from"/>, reach a hero at <paramref name="hero"/>?
        /// Within <see cref="GustReach"/> and downwind of him (or no more than <see cref="GustLee"/> upwind of his line); upwind is safe.
        /// </summary>
        public static bool VaneGustHits(VaneWind from, Vector2 vane, Vector2 hero)
        {
            Vector2 offset = hero - vane;
            if (offset.sqrMagnitude > GustReach * GustReach)
            {
                return false;
            }

            return Vector2.Dot(offset, WindBlowsToward(from)) >= -GustLee;
        }

        /// <summary>Which cover O Pequeno Eclipse makes on cast number <paramref name="cast"/> of the fight (0 first): 0, 1, 2, 3 (the whole round), 0…</summary>
        public static int EclipsePhaseOf(int cast) => Mathf.Abs(cast) % EclipsePhases;

        /// <summary>How many degrees either side of the hero's bearing cover <paramref name="phase"/> darkens: 45, 90, 135, 180.</summary>
        public static float EclipseHalfAngle(int phase) => EclipseStepDegrees * (EclipsePhaseOf(phase) + 1);

        /// <summary>
        /// Does cover <paramref name="phase"/> of O Pequeno Eclipse (at <paramref name="eclipse"/>), aimed at <paramref name="aim"/> (where the hero stood when it began),
        /// darken a hero at <paramref name="hero"/>? Within <see cref="EclipseReach"/> and inside the slice; right under the disc is dark too.
        /// </summary>
        public static bool EclipseHits(int phase, Vector2 eclipse, Vector2 aim, Vector2 hero)
        {
            Vector2 offset = hero - eclipse;
            if (offset.sqrMagnitude > EclipseReach * EclipseReach)
            {
                return false;
            }

            Vector2 bearing = aim - eclipse;
            if (offset.sqrMagnitude < 0.25f || bearing.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector2.Angle(bearing, offset) <= EclipseHalfAngle(phase) + 0.01f;
        }
    }
}
