using System;
using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>The ability only one boss has, on top of the shared phases (<see cref="RegionBossSpawn.Signature"/>). Regions 10–12 have none.</summary>
    public enum RegionBossSignature
    {
        None = 0,

        /// <summary>Apep drinks the river: whoever is far from her when she swallows takes the hit; she heals on it.</summary>
        DrinkTheRiver = 1,

        /// <summary>The Dream of Destruction: a ring falls round the spot the hero stood on, and the centre (and well outside) is the only safe place.</summary>
        DreamFall = 2,

        /// <summary>A Grande Treva wears its five forms in turn, each with its own damage (and the eagle's extra shots).</summary>
        FiveForms = 3,

        /// <summary>Āz opens the Ark: takes a share of the health the hero has right now, so the fuller, the worse.</summary>
        GreedOfTheArk = 4,

        /// <summary>Hedammu's gluttonous bite: a cone in front of her, toward where the hero stood; beside or behind her is safe.</summary>
        GluttonBite = 5,

        /// <summary>The Lodo-que-Engole swallows the ground round it: whoever is still inside the mud's radius (it grows in the Last Stand) is hit.</summary>
        SwallowingMire = 6,

        /// <summary>Aydahar breathes down the pass: a long narrow line from it toward the hero's spot; a step to the side is enough.</summary>
        PassBreath = 7,

        /// <summary>The Quatro Gulosos: the appetite of the week (and the next one at night) decides where it is safe.</summary>
        FourAppetites = 8,

        /// <summary>Ohya swings his chain round: a ring at the chain's length; hugging him or keeping well away are safe.</summary>
        ChainSweep = 9,

        // The map bosses of the fichas (RegionBossRules.MapBosses.cs: Docs/Mapas100.md 55, 56, 60, 62, 66, 69).

        /// <summary>O Cortador de Cordas cuts the rope under the hero: whoever is still walking when it snaps falls; whoever stopped and held on is safe.</summary>
        RopeCut = 10,

        /// <summary>O Sino que Não Para tolls on the hero's spot, and the echo of the last toll rings again where it fell: two places to stay out of.</summary>
        EchoToll = 11,

        /// <summary>A Morsa-Rainha flops onto the hero's spot: the closer to the middle, the harder; at the edge it is only a splash.</summary>
        BellyFlop = 12,

        /// <summary>O Mamute Avô shakes the gallery: stones fall on the hero's spot and four more round it in a cross; the diagonals are safe.</summary>
        CeilingFall = 13,

        /// <summary>O Rei Shisa do Telhado roars with the open mouth (far away is hit) and the closed one (up close is hit) in turn.</summary>
        TwoMouths = 14,

        /// <summary>O Mestre das Nove Fornalhas strikes the anvil on the hero's spot; each stroke heats the next until the ninth vents the furnaces.</summary>
        ForgeStroke = 15,
    }

    /// <summary>The four appetites of the Cidadela (46), one a week in turn (<see cref="RegionBossRules.AppetiteFor"/>).</summary>
    public enum GluttonAppetite
    {
        /// <summary>Taotie eats the ground where you stand: move off the mark.</summary>
        Taotie = 0,

        /// <summary>Hundun puts out the light everywhere but the mark (the lantern): stay on it.</summary>
        Hundun = 1,

        /// <summary>Qiongqi turns right into wrong: the safe place is next to it.</summary>
        Qiongqi = 2,

        /// <summary>Taowu won't listen: it strikes round itself, so keep away.</summary>
        Taowu = 3,
    }

    /// <summary>The five forms of the Prince of Darkness (Theodore bar Konai; Kephalaia 6), in the order A Grande Treva wears them.</summary>
    public enum GreatDarkForm
    {
        Lion = 0,
        Eagle = 1,
        Dragon = 2,
        Fish = 3,
        Demon = 4,
    }

    /// <summary>The fight phases of a world boss of Regions 5–16, by health (<see cref="RegionBossRules.PhaseFor"/>).</summary>
    public enum RegionBossPhase
    {
        /// <summary>Above 66%: the swing and a ring of force at each strike.</summary>
        Wrath = 1,

        /// <summary>66%–33%: also a barrage of shots at intervals.</summary>
        Storm = 2,

        /// <summary>Below 33%: also a telegraphed great blow on the spot the hero stood on.</summary>
        LastStand = 3,
    }

    /// <summary>Where a world boss of Regions 5–16 stands and how strong it is.</summary>
    public sealed class RegionBossSpawn
    {
        public readonly string EnemyId;
        public readonly string MapId;
        public readonly string AreaId;
        public readonly Vector2 LocalArena;
        public readonly Vector2 WorldOffset;
        public readonly int Level;
        public readonly EnemyProjectileKind Shot;
        public readonly bool StrongerAtNight;
        public readonly RegionBossSignature Signature;

        /// <summary>The fichas say it doesn't die, it withdraws: it says so on the way out and its body doesn't stay.</summary>
        public readonly bool NeverDies;

        /// <summary>Apep (76): she comes back on the next night, not after the usual wait alone.</summary>
        public readonly bool ReturnsNextNight;

        public RegionBossSpawn(string enemyId, string mapId, string areaId, Vector2 localArena, Vector2 worldOffset, int level,
            EnemyProjectileKind shot, bool strongerAtNight, RegionBossSignature signature = RegionBossSignature.None, bool neverDies = false, bool returnsNextNight = false)
        {
            EnemyId = enemyId;
            MapId = mapId;
            AreaId = areaId;
            LocalArena = localArena;
            WorldOffset = worldOffset;
            Level = level;
            Shot = shot;
            StrongerAtNight = strongerAtNight;
            Signature = signature;
            NeverDies = neverDies;
            ReturnsNextNight = returnsNextNight;
        }
    }

    /// <summary>
    /// Pure rules of the world bosses of the continuous Regions 5–9 (<see cref="MidSpawns"/>: Hedammu, the Lodo-que-Engole, Aydahar, the Quatro Gulosos
    /// and Ohya, maps 30, 35, 40, 46, 52) and 10–16 (Docs/Mapas100.md 58, 64, 70, 76, 82, 90, 96): the Rei-Vendaval, the Velho Kul,
    /// the Glutão de Brasa, Apep, o Sonho da Destruição, A Grande Treva and Āz. Region 16's is on map 96: map 100 has no fight by its ficha. Phases by health, the numbers of their abilities and where they stand. <see cref="RegionBossAbilities"/> plays
    /// the fight and <see cref="RegionBossDirector"/> keeps them in the world.
    /// </summary>
    public static partial class RegionBossRules
    {
        public const string WindKingId = "enemy_mountain_wind_king";
        public const string OldKulId = "enemy_mountain_old_kul";
        public const string EmberGluttonId = "enemy_mountain_ember_glutton";
        public const string ApepId = "enemy_mountain_apep";
        public const string DreamOfDestructionId = "enemy_mountain_dream_of_destruction";
        public const string GreatDarkId = "enemy_mountain_great_dark";
        public const string AzCaptainId = "enemy_mountain_az_captain";

        // Regions 5–9 (Docs/Mapas100.md 30, 35, 40, 46, 52): the last map of each region.
        public const string HedammuId = "enemy_mountain_hedammu";
        public const string SwallowingMireId = "enemy_mountain_swallowing_mire";
        public const string AydaharId = "enemy_mountain_aydahar";
        public const string FourGluttonsId = "enemy_mountain_four_gluttons";
        public const string OhyaId = "enemy_mountain_ohya";

        /// <summary>A Grande Treva's ring (90), in region coordinates: the stone ring before the Gate (Profundo's layout already sets it there).</summary>
        public static readonly Vector2 GrandeTrevaArena = new Vector2(1400f, 1000f);

        /// <summary>Seconds from a boss's death to its return (the commons take their fauna's 20–40 s; a boss takes a coffee break).</summary>
        public const float RespawnSeconds = 300f;

        /// <summary>How long the body of one that "never dies" stays before it has withdrawn.</summary>
        public const float WithdrawSeconds = 2.5f;

        /// <summary>The boss appears when the hero comes within this many metres of the arena.</summary>
        public const float ActiveRadius = 90f;

        /// <summary>It goes away (back to full health) when the hero is further than this: inside <c>OverworldRules.UnloadRadius</c>, so it is gone before the ground under it unloads.</summary>
        public const float SleepRadius = 125f;

        /// <summary>How long a fallen boss lies there, like the commons' corpses.</summary>
        public const float CorpseSeconds = 9f;

        /// <summary>No ordinary clearing centre this close to an arena (the stone ring keeps them off: clearing radius + 12 m, radius at least 10 m).</summary>
        public const float ArenaClearRadius = 22f;

        public const float StormPhaseStart = 0.66f;
        public const float LastStandPhaseStart = 0.33f;

        /// <summary>The ring of force at each strike, as a fraction of the swing.</summary>
        public const float RingDamageFactor = 0.5f;

        public const float BarrageIntervalSeconds = 8f;
        public const float BarrageDamageFactor = 0.4f;
        public const float BarrageRange = 12f;
        /// <summary>Metres between neighbouring shots at the muzzle.</summary>
        public const float BarrageSpread = 0.9f;

        public const float GreatBlowIntervalSeconds = 12f;
        public const float GreatBlowWindupSeconds = 2f;
        public const float GreatBlowRadius = 4.5f;
        public const float GreatBlowDamageFactor = 1.6f;

        public const float NightAbilityMultiplier = 1.25f;

        /// <summary>Beyond this the boss leaves the hero alone with its abilities (it may still be chasing).</summary>
        public const float AbilityRange = 22f;

        // ---- Apep (76): she drinks the river ----

        public const float DrinkIntervalSeconds = 14f;
        public const float DrinkWindupSeconds = 2.5f;
        public const float DrinkDamageFactor = 1.1f;

        /// <summary>Within this many metres of her you are too close to be swallowed with the river.</summary>
        public const float DrinkSafeRadius = 7f;

        /// <summary>What a swallow that lands gives her back, as a fraction of her health.</summary>
        public const float DrinkHealFraction = 0.02f;

        /// <summary>Peak ability multiplier at midnight (ficha: "Apep mais forte perto da meia-noite").</summary>
        public const float MidnightPeakMultiplier = 1.4f;

        /// <summary>Hours either side of midnight in which the clock strengthens her.</summary>
        public const float MidnightWindowHours = 3f;

        // ---- O Sonho da Destruição (82): the ring of cut trees ----

        public const float DreamFallIntervalSeconds = 11f;
        public const float DreamFallWindupSeconds = 2.2f;
        public const float DreamFallDamageFactor = 1.4f;

        /// <summary>Inside this radius of the marked spot the ring passes over you.</summary>
        public const float DreamFallInnerRadius = 2.5f;

        /// <summary>Beyond this radius it never reached.</summary>
        public const float DreamFallOuterRadius = 7f;

        // ---- A Grande Treva (90): five forms ----

        public const float FormSeconds = 9f;
        public const int EagleExtraShots = 2;

        // ---- Āz (96): the greed of the Ark ----

        public const float GreedIntervalSeconds = 16f;
        public const float GreedWindupSeconds = 2f;

        /// <summary>The share of the hero's current health the Ark takes.</summary>
        public const float GreedHealthFraction = 0.15f;

        // ---- Hedammu (30): the gluttonous bite ----

        public const float BiteIntervalSeconds = 10f;
        public const float BiteWindupSeconds = 1.8f;
        public const float BiteDamageFactor = 1.3f;
        public const float BiteRange = 9f;

        /// <summary>Half the opening of the bite (degrees either side of where she aimed).</summary>
        public const float BiteHalfAngle = 50f;

        // ---- O Lodo-que-Engole (35): the mire swallows ----

        public const float MireIntervalSeconds = 12f;
        public const float MireWindupSeconds = 2.4f;
        public const float MireDamageFactor = 1.2f;
        public const float MireRadiusStorm = 6f;
        public const float MireRadiusLastStand = 9f;

        /// <summary>What each Fonte do Lodo gone dark during the fight adds to all its abilities (+10% each, +50% with none lit).</summary>
        public const float MireUnlitFountainBonus = 0.1f;

        // ---- Aydahar (40): the breath down the pass ----

        public const float BreathIntervalSeconds = 11f;
        public const float BreathWindupSeconds = 2f;
        public const float BreathDamageFactor = 1.5f;
        public const float BreathLength = 16f;
        public const float BreathHalfWidth = 2f;

        // ---- Os Quatro Gulosos (46): the appetite of the week ----

        public const float AppetiteIntervalSeconds = 12f;
        public const float AppetiteWindupSeconds = 2.2f;
        public const float AppetiteDamageFactor = 1.3f;

        /// <summary>Taotie's mouth on the marked spot.</summary>
        public const float TaotieRadius = 4f;

        /// <summary>The lantern Hundun leaves lit on the marked spot.</summary>
        public const float HundunLanternRadius = 3f;

        /// <summary>Qiongqi's safe place round itself, and Taowu's stubborn reach.</summary>
        public const float GluttonNearRadius = 6f;

        /// <summary>The ficha: "Hundun mais forte" at night.</summary>
        public const float HundunNightMultiplier = 1.25f;

        // Taotie eats the loot on the ground. The game has no loot lying about (drops go straight into the bag, EnemyLootGranter),
        // so on Taotie's week the vessel spills offerings of coins round itself and Taotie eats the one nearest its mouth.
        // Grab one now (1x, but next to the boss) or let it go: what Taotie swallowed comes back doubled when it falls.

        /// <summary>What an offering is: a stack of the island's coin (not the boss's Bronze, which the house recipes count one by one).</summary>
        public const string TaotieOfferingItemId = "item_gold_coin";
        public const int TaotieOfferingCoins = 40;
        public const float TaotieFirstSpillSeconds = 3f;
        public const float TaotieSpillIntervalSeconds = 8f;
        public const int TaotieMaxOfferingsOnGround = 3;
        public const int TaotieMaxOfferingsPerFight = 6;
        public const float TaotieSpillMinRadius = 3f;
        public const float TaotieSpillMaxRadius = 7f;
        public const float TaotieFirstEatSeconds = 6f;
        public const float TaotieEatIntervalSeconds = 9f;

        /// <summary>The ring on the offering before Taotie eats it: time to run and grab it.</summary>
        public const float TaotieEatWindupSeconds = 2.5f;

        /// <summary>How far from Taotie an offering may lie and still be eaten.</summary>
        public const float TaotieEatReach = 12f;

        /// <summary>Walking this close to an offering picks it up.</summary>
        public const float TaotieOfferingPickupRadius = 1.6f;

        /// <summary>What Taotie swallowed comes back this many times over when it falls.</summary>
        public const int TaotieReturnFactor = 2;

        /// <summary>Qiongqi swaps the hero's steps for this long after its blow (Player/HeroControlMirror caps any swap at 4 s).</summary>
        public const float QiongqiMirrorSeconds = 3f;

        // ---- Ohya (52): the chain swings round ----

        public const float ChainIntervalSeconds = 10f;
        public const float ChainWindupSeconds = 2f;
        public const float ChainDamageFactor = 1.4f;
        public const float ChainInnerRadius = 4f;
        public const float ChainOuterRadius = 10f;

        /// <summary>The world bosses of Regions 10–16 (the "far" bosses: <c>ach_bosses_far_all</c> counts exactly these).</summary>
        public static readonly IReadOnlyList<RegionBossSpawn> Spawns = new[]
        {
            new RegionBossSpawn(WindKingId, OverworldLayouts.NinhoHumaMapId, "ninho_da_huma", OverworldLayouts.ReiVendavalArena,
                OverworldLayouts.FarnkeshWorldOffset, 200, EnemyProjectileKind.Feather, true),
            new RegionBossSpawn(OldKulId, OverworldLayouts.FundoVelhoKulMapId, "fundo_do_velho_kul", OverworldLayouts.VelhoKulArena,
                OverworldLayouts.MarGeladoWorldOffset, 240, EnemyProjectileKind.LureGlob, true),
            new RegionBossSpawn(EmberGluttonId, OverworldLayouts.CaldeiraMapId, "caldeira_do_fogo_devorador", OverworldLayouts.GlutaoArena,
                OverworldLayouts.HinokuraWorldOffset, 280, EnemyProjectileKind.Ember, false),
            new RegionBossSpawn(ApepId, OverworldLayouts.BancoApepMapId, "banco_de_areia_de_apep", OverworldLayouts.ApepArena,
                OverworldLayouts.RioWorldOffset, 320, EnemyProjectileKind.LureGlob, false, RegionBossSignature.DrinkTheRiver, neverDies: true, returnsNextNight: true),
            new RegionBossSpawn(DreamOfDestructionId, OverworldLayouts.TabuasSonhoMapId, "tabuas_do_sonho", OverworldLayouts.SonhoDaDestruicaoArena,
                OverworldLayouts.MisturaWorldOffset, 360, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.DreamFall),
            new RegionBossSpawn(GreatDarkId, OverworldLayouts.PortaoTrevaMapId, "portao_da_terra_da_treva", GrandeTrevaArena,
                OverworldLayouts.ProfundoWorldOffset, 440, EnemyProjectileKind.Ember, false, RegionBossSignature.FiveForms, neverDies: true),
            new RegionBossSpawn(AzCaptainId, OverworldLayouts.JardimDeLuzMapId, "jardim_das_flores", OverworldLayouts.CapitaAzArena,
                OverworldLayouts.CeuWorldOffset, 478, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.GreedOfTheArk, neverDies: true),
        };

        /// <summary>The world bosses of Regions 5–9 (Docs/Mapas100.md 30, 35, 40, 46, 52), at the last map of each region.</summary>
        public static readonly IReadOnlyList<RegionBossSpawn> MidSpawns = new[]
        {
            new RegionBossSpawn(HedammuId, OverworldLayouts.LiyongoId, "mangues_de_liyongo", OverworldLayouts.HedammuArena,
                OverworldLayouts.CostaWorldOffset, 72, EnemyProjectileKind.LureGlob, false, RegionBossSignature.GluttonBite, neverDies: true),
            new RegionBossSpawn(SwallowingMireId, OverworldLayouts.CoracaoLodoId, "olho_do_lodo", OverworldLayouts.LodoArena,
                OverworldLayouts.DeltaWorldOffset, 88, EnemyProjectileKind.LureGlob, true, RegionBossSignature.SwallowingMire, neverDies: true),
            new RegionBossSpawn(AydaharId, OverworldLayouts.WakhanId, "ninho_de_aydahar", OverworldLayouts.AydaharArena,
                OverworldLayouts.TetoWorldOffset, 98, EnemyProjectileKind.Ember, false, RegionBossSignature.PassBreath, neverDies: true),
            new RegionBossSpawn(FourGluttonsId, OverworldLayouts.CidadelaMapId, "cidadela_dos_quatro_gulosos", OverworldLayouts.GulososArena,
                OverworldLayouts.MingchengWorldOffset, 140, EnemyProjectileKind.LanternBolt, false, RegionBossSignature.FourAppetites),
            new RegionBossSpawn(OhyaId, OverworldLayouts.AmiraniMapId, "cume_de_ohya", OverworldLayouts.OhyaArena,
                OverworldLayouts.NartesWorldOffset, 150, EnemyProjectileKind.Feather, false, RegionBossSignature.ChainSweep),
        };

        /// <summary>Every world boss of Regions 5–9 and 10–16 (the director also keeps the map bosses: <see cref="DirectorSpawns"/>).</summary>
        public static readonly IReadOnlyList<RegionBossSpawn> AllSpawns = Concat(MidSpawns, Spawns);

        private static RegionBossSpawn[] Concat(IReadOnlyList<RegionBossSpawn> a, IReadOnlyList<RegionBossSpawn> b)
        {
            var all = new List<RegionBossSpawn>(a);
            all.AddRange(b);
            return all.ToArray();
        }

        public static RegionBossSpawn SpawnOf(string enemyId)
        {
            foreach (RegionBossSpawn spawn in DirectorSpawns)
            {
                if (spawn.EnemyId == enemyId)
                {
                    return spawn;
                }
            }

            return null;
        }

        /// <summary>A boss the <see cref="RegionBossDirector"/> keeps: a world boss or a map boss (<see cref="MapSpawns"/>).</summary>
        public static bool IsRegionBoss(string enemyId) => SpawnOf(enemyId) != null;

        public static RegionBossPhase PhaseFor(float healthFraction)
        {
            if (healthFraction > StormPhaseStart)
            {
                return RegionBossPhase.Wrath;
            }

            return healthFraction >= LastStandPhaseStart ? RegionBossPhase.Storm : RegionBossPhase.LastStand;
        }

        public static int ShotsFor(RegionBossPhase phase)
        {
            switch (phase)
            {
                case RegionBossPhase.Storm: return 3;
                case RegionBossPhase.LastStand: return 5;
                default: return 0;
            }
        }

        /// <summary>Sideways offset (m) of shot <paramref name="index"/> of <paramref name="count"/>: centred on the muzzle.</summary>
        public static float ShotOffset(int index, int count) => (index - (count - 1) * 0.5f) * BarrageSpread;

        public static bool GreatBlowEnabled(RegionBossPhase phase) => phase == RegionBossPhase.LastStand;

        /// <summary>Does a great blow aimed at <paramref name="impact"/> reach someone at <paramref name="hero"/>? (flat distance)</summary>
        public static bool GreatBlowHits(Vector2 impact, Vector2 hero) => (hero - impact).sqrMagnitude <= GreatBlowRadius * GreatBlowRadius;

        /// <summary>Ability damage multiplier: the fichas make the Rei-Vendaval and the Kul stronger at night, the Glutão lives in a caldera that is always night.</summary>
        public static float AbilityMultiplier(bool strongerAtNight, bool isNight) => strongerAtNight && isNight ? NightAbilityMultiplier : 1f;

        public static bool SignatureEnabled(RegionBossSignature signature, RegionBossPhase phase) => signature != RegionBossSignature.None && phase != RegionBossPhase.Wrath;

        public static float SignatureInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.DrinkTheRiver: return DrinkIntervalSeconds;
                case RegionBossSignature.DreamFall: return DreamFallIntervalSeconds;
                case RegionBossSignature.GreedOfTheArk: return GreedIntervalSeconds;
                case RegionBossSignature.GluttonBite: return BiteIntervalSeconds;
                case RegionBossSignature.SwallowingMire: return MireIntervalSeconds;
                case RegionBossSignature.PassBreath: return BreathIntervalSeconds;
                case RegionBossSignature.FourAppetites: return AppetiteIntervalSeconds;
                case RegionBossSignature.ChainSweep: return ChainIntervalSeconds;
                case RegionBossSignature.RopeCut:
                case RegionBossSignature.EchoToll:
                case RegionBossSignature.BellyFlop:
                case RegionBossSignature.CeilingFall:
                case RegionBossSignature.TwoMouths:
                case RegionBossSignature.ForgeStroke:
                    return MapBossInterval(signature);
                default: return FormSeconds;
            }
        }

        public static float SignatureWindup(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.DrinkTheRiver: return DrinkWindupSeconds;
                case RegionBossSignature.DreamFall: return DreamFallWindupSeconds;
                case RegionBossSignature.GreedOfTheArk: return GreedWindupSeconds;
                case RegionBossSignature.GluttonBite: return BiteWindupSeconds;
                case RegionBossSignature.SwallowingMire: return MireWindupSeconds;
                case RegionBossSignature.PassBreath: return BreathWindupSeconds;
                case RegionBossSignature.FourAppetites: return AppetiteWindupSeconds;
                case RegionBossSignature.ChainSweep: return ChainWindupSeconds;
                case RegionBossSignature.RopeCut:
                case RegionBossSignature.EchoToll:
                case RegionBossSignature.BellyFlop:
                case RegionBossSignature.CeilingFall:
                case RegionBossSignature.TwoMouths:
                case RegionBossSignature.ForgeStroke:
                    return MapBossWindup(signature);
                default: return 0f;
            }
        }

        /// <summary>Does Apep's swallow reach a hero <paramref name="distance"/> m from her? Only those who stayed away.</summary>
        public static bool DrinkHits(float distance) => distance > DrinkSafeRadius;

        /// <summary>Ability multiplier for the hour: 1 until <see cref="MidnightWindowHours"/> from midnight, up to the peak at 0:00 (wraps round the day).</summary>
        public static float MidnightMultiplier(float hour)
        {
            float fromMidnight = Mathf.Abs(Mathf.Repeat(hour + 12f, 24f) - 12f);
            return Mathf.Lerp(MidnightPeakMultiplier, 1f, Mathf.Clamp01(fromMidnight / MidnightWindowHours));
        }

        /// <summary>Is a hero at <paramref name="hero"/> under the ring that falls round <paramref name="impact"/>? Staying exactly put, or getting well out, are safe.</summary>
        public static bool DreamFallHits(Vector2 impact, Vector2 hero)
        {
            float distance = Vector2.Distance(impact, hero);
            return distance >= DreamFallInnerRadius && distance <= DreamFallOuterRadius;
        }

        /// <summary>The form worn <paramref name="fightSeconds"/> into the fight.</summary>
        public static GreatDarkForm FormAt(float fightSeconds)
        {
            int index = Mathf.FloorToInt(Mathf.Max(0f, fightSeconds) / FormSeconds);
            return (GreatDarkForm)(index % 5);
        }

        public static float FormDamageMultiplier(GreatDarkForm form)
        {
            switch (form)
            {
                case GreatDarkForm.Lion: return 1.3f;
                case GreatDarkForm.Dragon: return 1.15f;
                case GreatDarkForm.Fish: return 0.85f;
                case GreatDarkForm.Demon: return 1.2f;
                default: return 1f;
            }
        }

        public static int FormExtraShots(GreatDarkForm form) => form == GreatDarkForm.Eagle ? EagleExtraShots : 0;

        /// <summary>What the Ark takes from a hero who has <paramref name="currentHealth"/> left: always something, proportional to what is left.</summary>
        /// <remarks>The Ark never kills: at most <c>currentHealth − 1</c> (0 when the hero has 1 or less left).</remarks>
        public static float GreedDamage(float currentHealth)
        {
            float left = Mathf.Max(0f, currentHealth);
            return Mathf.Min(Mathf.Max(1f, left * GreedHealthFraction), Mathf.Max(0f, left - 1f));
        }

        /// <summary>Does Hedammu's bite, aimed from <paramref name="boss"/> along <paramref name="aim"/>, reach a hero at <paramref name="hero"/>? Within range and inside the cone.</summary>
        public static bool BiteHits(Vector2 boss, Vector2 aim, Vector2 hero)
        {
            Vector2 toHero = hero - boss;
            if (toHero.sqrMagnitude > BiteRange * BiteRange)
            {
                return false;
            }

            if (toHero.sqrMagnitude < 0.01f || aim.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            return Vector2.Angle(aim, toHero) <= BiteHalfAngle;
        }

        /// <summary>How far the swallowing mud reaches in <paramref name="phase"/> (none in the Wrath phase).</summary>
        public static float MireRadius(RegionBossPhase phase)
        {
            switch (phase)
            {
                case RegionBossPhase.Storm: return MireRadiusStorm;
                case RegionBossPhase.LastStand: return MireRadiusLastStand;
                default: return 0f;
            }
        }

        /// <summary>The Lodo-que-Engole rises only with the five Fontes do Lodo lit (<see cref="LodoFountainRules"/>; the ficha of map 35).</summary>
        public static bool MireSummonReady(int litFountains) => litFountains >= LodoFountainRules.Count;

        /// <summary>
        /// Whether a dormant boss may appear at all, besides the hero's distance: the Lodo waits for its five fountains
        /// (<paramref name="mireFountainsLit"/>), every other boss is always ready.
        /// </summary>
        public static bool SummonReady(RegionBossSpawn spawn, int mireFountainsLit) =>
            spawn != null && (spawn.Signature != RegionBossSignature.SwallowingMire || MireSummonReady(mireFountainsLit));

        /// <summary>The Lodo's ability multiplier for <paramref name="litFountains"/> fountains still lit: 1 with the five, +<see cref="MireUnlitFountainBonus"/> per one gone dark.</summary>
        public static float MireFountainMultiplier(int litFountains) =>
            1f + MireUnlitFountainBonus * (LodoFountainRules.Count - Mathf.Clamp(litFountains, 0, LodoFountainRules.Count));

        /// <summary>Is a hero <paramref name="distance"/> m from the Lodo still in the mud when it swallows?</summary>
        public static bool MireHits(float distance, RegionBossPhase phase) => distance <= MireRadius(phase);

        /// <summary>Does Aydahar's breath, from <paramref name="origin"/> toward <paramref name="aim"/>, reach a hero at <paramref name="hero"/>? A line <see cref="BreathLength"/> long, 2 × <see cref="BreathHalfWidth"/> wide.</summary>
        public static bool BreathHits(Vector2 origin, Vector2 aim, Vector2 hero)
        {
            Vector2 axis = aim - origin;
            Vector2 toHero = hero - origin;
            if (axis.sqrMagnitude < 1e-6f)
            {
                return toHero.sqrMagnitude <= BreathHalfWidth * BreathHalfWidth;
            }

            axis.Normalize();
            float along = Vector2.Dot(toHero, axis);
            if (along < -BreathHalfWidth || along > BreathLength)
            {
                return false;
            }

            float across = Mathf.Abs(toHero.x * axis.y - toHero.y * axis.x);
            return across <= BreathHalfWidth;
        }

        /// <summary>The appetite of the Quatro Gulosos on UTC day <paramref name="utcDay"/>: one a week in turn (the ficha's online rotation); at night the palaces swap to the next.</summary>
        public static GluttonAppetite AppetiteFor(long utcDay, bool isNight)
        {
            long week = utcDay >= 0 ? utcDay / 7 : (utcDay - 6) / 7;
            long index = (week + (isNight ? 1 : 0)) % 4;
            return (GluttonAppetite)(index < 0 ? index + 4 : index);
        }

        /// <summary>Does the appetite's blow, marked on <paramref name="impact"/> by the Gulosos at <paramref name="boss"/>, reach a hero at <paramref name="hero"/>?</summary>
        public static bool AppetiteHits(GluttonAppetite appetite, Vector2 impact, Vector2 boss, Vector2 hero)
        {
            switch (appetite)
            {
                case GluttonAppetite.Taotie: return Vector2.Distance(hero, impact) <= TaotieRadius;
                case GluttonAppetite.Hundun: return Vector2.Distance(hero, impact) > HundunLanternRadius;
                case GluttonAppetite.Qiongqi: return Vector2.Distance(hero, boss) > GluttonNearRadius;
                default: return Vector2.Distance(hero, boss) <= GluttonNearRadius;
            }
        }

        /// <summary>Ability multiplier of the appetite: Hundun is stronger at night.</summary>
        public static float AppetiteMultiplier(GluttonAppetite appetite, bool isNight) => appetite == GluttonAppetite.Hundun && isNight ? HundunNightMultiplier : 1f;

        /// <summary>Does the vessel spill another offering? Only while fewer than <see cref="TaotieMaxOfferingsOnGround"/> lie about and the fight's <see cref="TaotieMaxOfferingsPerFight"/> aren't used up.</summary>
        public static bool TaotieShouldSpill(int onGround, int spilledThisFight) =>
            onGround < TaotieMaxOfferingsOnGround && spilledThisFight < TaotieMaxOfferingsPerFight;

        /// <summary>Where an offering lands round Taotie at <paramref name="boss"/>: <paramref name="angle01"/> round the circle, <paramref name="radius01"/> between the spill radii (both 0–1).</summary>
        public static Vector2 TaotieSpillSpot(Vector2 boss, float angle01, float radius01)
        {
            float angle = Mathf.Repeat(angle01, 1f) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(TaotieSpillMinRadius, TaotieSpillMaxRadius, Mathf.Clamp01(radius01));
            return boss + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        /// <summary>
        /// Which offering Taotie goes for: the nearest to its mouth within <see cref="TaotieEatReach"/>; -1 when none is in reach.
        /// One the hero is already standing on is skipped (it is being picked up this very frame).
        /// </summary>
        public static int TaotieChooseOffering(IReadOnlyList<Vector2> offerings, Vector2 boss, Vector2 hero)
        {
            int best = -1;
            float bestDistance = TaotieEatReach * TaotieEatReach;
            for (int i = 0; offerings != null && i < offerings.Count; i++)
            {
                if (TaotieOfferingPicked(offerings[i], hero))
                {
                    continue;
                }

                float distance = (offerings[i] - boss).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    best = i;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>Is a hero at <paramref name="hero"/> close enough to pick up the offering at <paramref name="offering"/>?</summary>
        public static bool TaotieOfferingPicked(Vector2 offering, Vector2 hero) =>
            (hero - offering).sqrMagnitude <= TaotieOfferingPickupRadius * TaotieOfferingPickupRadius;

        /// <summary>
        /// The coins Taotie gives back when it falls: what it swallowed <see cref="TaotieReturnFactor"/> times over, plus the offerings
        /// still lying about (the fight is over, nobody is in a hurry). Nothing comes back if it leaves instead of falling.
        /// </summary>
        public static int TaotieDeathReturn(int eaten, int leftOnGround) =>
            (Mathf.Max(0, eaten) * TaotieReturnFactor + Mathf.Max(0, leftOnGround)) * TaotieOfferingCoins;

        /// <summary>Does Qiongqi's blow swap the steps of a hero <paramref name="distance"/> m from it? On its appetite, everyone in the fight.</summary>
        public static bool QiongqiSwapsSteps(GluttonAppetite appetite, float distance) => appetite == GluttonAppetite.Qiongqi && distance <= AbilityRange;

        /// <summary>Is a hero <paramref name="distance"/> m from Ohya under the chain as it swings round?</summary>
        public static bool ChainSweepHits(float distance) => distance >= ChainInnerRadius && distance <= ChainOuterRadius;

        /// <summary>Apep is back only when a night begins after she withdrew: a day must have passed first (her ficha's "até a noite seguinte").</summary>
        public static bool NightReturnReady(bool sawDaySinceWithdrawing, bool isNight) => sawDaySinceWithdrawing && isNight;

        /// <summary>
        /// Which night the game clock (<paramref name="day"/>, <paramref name="hour"/> of <c>TimeOfDayManager</c>) is in, or last went through:
        /// the night that begins at <paramref name="nightStartHour"/> of day D is night D, and until the next one starts (its small hours of
        /// day D+1, the next morning and afternoon) the clock still reads night D.
        /// </summary>
        public static int NightIndex(int day, float hour, float nightStartHour) => hour >= nightStartHour ? day : day - 1;

        /// <summary>
        /// <see cref="NightReturnReady(bool, bool)"/> by the game's own day: a later night than the one that held the withdrawal
        /// (a withdrawal by day counts the night before), whether or not the hero was in the boss's world while the day went by.
        /// </summary>
        public static bool NightReturnReady(int withdrawnNight, int day, float hour, bool isNight, float nightStartHour) =>
            isNight && NightIndex(day, hour, nightStartHour) > withdrawnNight;

        /// <summary>
        /// Whether a change of phase is called out: only when it reaches a phase graver than <paramref name="gravestSoFar"/> in this
        /// fight, so a boss healing back over a threshold (Apep's drink) and dropping again stays quiet.
        /// </summary>
        public static bool ShouldAnnouncePhase(RegionBossPhase gravestSoFar, RegionBossPhase phase) => phase > gravestSoFar;

        /// <summary>Whether a dormant boss should appear for a hero <paramref name="distance"/> m from its arena.</summary>
        public static bool ShouldWake(float distance) => distance <= ActiveRadius;

        /// <summary>Whether a standing boss should go away for a hero <paramref name="distance"/> m from its arena.</summary>
        public static bool ShouldSleep(float distance) => distance > SleepRadius;

        /// <summary>
        /// The arena in the coordinates of <paramref name="layout"/>: the stitched world (the region's offset) or the region alone (no offset);
        /// null when neither lands in the boss's area (the world being played doesn't contain this region).
        /// </summary>
        public static Vector2? ArenaIn(OverworldLayout layout, RegionBossSpawn spawn)
        {
            if (layout == null || spawn == null)
            {
                return null;
            }

            foreach (Vector2 candidate in new[] { spawn.LocalArena + spawn.WorldOffset, spawn.LocalArena })
            {
                OverworldZone zone = OverworldRules.ZoneAt(layout, candidate);
                if (zone != null && string.Equals(zone.Id, spawn.AreaId, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
