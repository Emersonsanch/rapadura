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
    /// The map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69): smaller than the world bosses, one per map the story walks
    /// through in the 150–300 band. They are kept by the same <see cref="RegionBossDirector"/>, fight with the same <see cref="RegionBossAbilities"/>
    /// (phases, ring, barrage, great blow) and stand on a stone ring of their map, each with one ability of its own
    /// (<see cref="RegionBossSignature"/> 10–15). They stay out of <see cref="AllSpawns"/> (the world bosses: keepsakes, trophies,
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

        /// <summary>The map bosses (55, 56, 60, 62, 66, 69), each at the top of its map's band.</summary>
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

        private static float MapBossInterval(RegionBossSignature signature)
        {
            switch (signature)
            {
                case RegionBossSignature.RopeCut: return RopeCutIntervalSeconds;
                case RegionBossSignature.EchoToll: return TollIntervalSeconds;
                case RegionBossSignature.BellyFlop: return FlopIntervalSeconds;
                case RegionBossSignature.CeilingFall: return CeilingIntervalSeconds;
                case RegionBossSignature.TwoMouths: return ShisaIntervalSeconds;
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
    }
}
