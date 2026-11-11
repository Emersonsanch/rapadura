using System.Collections.Generic;
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

    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69 and, on the road of the middle story, 25, 26, 32, 37, 38, 43): smaller
    /// than the world bosses, one per map the story walks through in the 150–300 band and in Regions 5–8. They are kept by the same <see cref="RegionBossDirector"/>, fight with the same <see cref="RegionBossAbilities"/>
    /// (phases, ring, barrage, great blow) and stand on a stone ring of their map, each with one ability of its own
    /// (<see cref="RegionBossSignature"/> 10–21). They stay out of <see cref="AllSpawns"/> (the world bosses: keepsakes, trophies,
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

        /// <summary>The map bosses (25, 26, 32, 37, 38, 43; 55, 56, 60, 62, 66, 69), each at the top of its map's band.</summary>
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
    }
}
