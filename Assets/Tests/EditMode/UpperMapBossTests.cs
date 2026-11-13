using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Six more map bosses of Regions 10–16 (Docs/Mapas100.md 57, 63, 67, 74, 86, 97): the list, their strength against their region's world boss, each one's
    /// own ability as pure rules, their call-outs and the arms that rise on the windup. The shared checks (asset, biome, model, page, arena) are
    /// <see cref="MapBossTests"/>'s, which lists them too.
    /// </summary>
    public class UpperMapBossTests
    {
        private static readonly string[] UpperIds =
        {
            RegionBossRules.AysKingId, RegionBossRules.TrappedAuroraId, RegionBossRules.GuayotaId,
            RegionBossRules.WrongKnotId, RegionBossRules.DarkWingedEagleId, RegionBossRules.SailShadowId,
        };

        private static readonly string[] UpperMaps =
        {
            OverworldLayouts.GargantaAysMapId, OverworldLayouts.MarAurorasMapId, OverworldLayouts.MonteEcheydeMapId,
            OverworldLayouts.AneisMehenMapId, OverworldLayouts.CovaVentoMapId, OverworldLayouts.BarcaDoSolMapId,
        };

        /// <summary>The world boss of each one's region (10 Rei-Vendaval, 11 Velho Kul, 12 Glutão, 13 Apep, 15 Grande Treva, 16 Capitã de Āz).</summary>
        private static readonly string[] WorldBosses =
        {
            RegionBossRules.WindKingId, RegionBossRules.OldKulId, RegionBossRules.EmberGluttonId,
            RegionBossRules.ApepId, RegionBossRules.GreatDarkId, RegionBossRules.AzCaptainId,
        };

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, id);
            return definition;
        }

        // ---------------------------------------------------------------- the list

        [Test]
        public void TheUpperMaps_GetTheirFichaBoss_OnePerRegion_AtTheTopOfTheirBand()
        {
            for (int i = 0; i < UpperIds.Length; i++)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(UpperIds[i]);
                Assert.IsNotNull(spawn, UpperIds[i]);
                Assert.IsTrue(RegionBossRules.IsMapBoss(UpperIds[i]), UpperIds[i]);
                Assert.AreEqual(UpperMaps[i], spawn.MapId, UpperIds[i]);
                MapDefinition map = MapCatalog.Get(spawn.MapId);
                Assert.IsNotNull(map, spawn.MapId);
                Assert.AreEqual(map.MonsterLevelMax, spawn.Level, $"{UpperIds[i]}: the top of its map's band");
                Assert.That(map.MonsterLevelMax, Is.InRange(140, 500), $"{UpperIds[i]}: Regions 10–16");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == spawn.MapId), $"{spawn.MapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == spawn.MapId), spawn.MapId);
            }

            CollectionAssert.AllItemsAreUnique(UpperMaps.Select(m => MapCatalog.Get(m).Region), "one per region");
            Assert.IsTrue(UpperMaps.Any(m => MapCatalog.Get(m).Region == MapRegion.LimiarProfundo), "the Limiar Profundo, which had none, gets one");
        }

        [Test]
        public void TheUpperMapBosses_AreWeakerThanTheWorldBossOfTheirRegion()
        {
            for (int i = 0; i < UpperIds.Length; i++)
            {
                Assert.Less(Load(UpperIds[i]).MaxHealth, Load(WorldBosses[i]).MaxHealth, UpperIds[i]);
                Assert.Less(Load(UpperIds[i]).AttackDamage, Load(WorldBosses[i]).AttackDamage, UpperIds[i]);
                Assert.LessOrEqual(Load(UpperIds[i]).MaxHealth, 1180, UpperIds[i]);
                Assert.Less(Load(UpperIds[i]).BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, UpperIds[i]);
                Assert.AreEqual(RegionBossRules.MapBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(UpperIds[i], true), UpperIds[i]);
            }
        }

        [Test]
        public void EachSignatureIsNewToTheDirector()
        {
            RegionBossSignature[] upper =
            {
                RegionBossSignature.GorgeGust, RegionBossSignature.AuroraRoute, RegionBossSignature.CraterSpit,
                RegionBossSignature.CoilLoops, RegionBossSignature.WingSpan, RegionBossSignature.BoomSwing,
            };
            CollectionAssert.AreEquivalent(upper, UpperIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in upper)
            {
                Assert.AreEqual(1, RegionBossRules.DirectorSpawns.Count(s => s.Signature == signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureInterval(signature), RegionBossRules.SignatureWindup(signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureWindup(signature), 1f, $"{signature}: time to read the warning");
                Assert.IsTrue(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Storm), signature.ToString());
                Assert.IsFalse(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Wrath), signature.ToString());
            }
        }

        // ---------------------------------------------------------------- the abilities

        [Test]
        public void AysKing_TheGustRunsDownTheGorgeFromHim_UpwindAndAsideAreSafe()
        {
            Vector2 king = new Vector2(30f, 10f);
            Vector2 up = RegionBossRules.AysGorgeAxis.normalized;
            Vector2 aside = new Vector2(-up.y, up.x);
            Assert.AreEqual(up, RegionBossRules.AysGustDirection(0));
            Assert.AreEqual(-up, RegionBossRules.AysGustDirection(1), "down the gorge the next time");
            Assert.AreEqual(up, RegionBossRules.AysGustDirection(2));
            Assert.IsTrue(RegionBossRules.AysGustHits(0, king, king + up * 6f), "downwind, on the line");
            Assert.IsFalse(RegionBossRules.AysGustHits(0, king, king - up * 6f), "upwind, behind him");
            Assert.IsTrue(RegionBossRules.AysGustHits(1, king, king - up * 6f), "the wind turned: now that side is downwind");
            Assert.IsFalse(RegionBossRules.AysGustHits(0, king, king + up * 6f + aside * (RegionBossRules.AysGustHalfWidth + 0.2f)), "a step aside");
            Assert.IsFalse(RegionBossRules.AysGustHits(0, king, king + up * (RegionBossRules.AysGustLength + 0.5f)), "past the gust's end");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.AysKingId).StrongerAtNight, "the ficha: the wind drops by half at night");
        }

        [Test]
        public void TrappedAurora_OnlyTheLitRouteHolds_AndItTurnsEachDance()
        {
            Vector2 aurora = new Vector2(-5f, 5f);
            Assert.AreEqual(0, RegionBossRules.AuroraDanceOf(RegionBossRules.AuroraDances), "the dances come round");
            Vector2 north = RegionBossRules.AuroraRouteDirection(0);
            Assert.AreEqual(0f, Vector2.Distance(north, Vector2.up), 1e-4f);
            Assert.IsFalse(RegionBossRules.AuroraRouteHits(0, aurora, aurora + Vector2.up * 6f), "on the lit route");
            Assert.IsTrue(RegionBossRules.AuroraRouteHits(0, aurora, aurora + Vector2.down * 6f), "behind her the ice cracks");
            Assert.IsTrue(RegionBossRules.AuroraRouteHits(0, aurora, aurora + new Vector2(RegionBossRules.AuroraRouteHalfWidth + 0.5f, 6f)), "beside the route");
            Assert.IsTrue(RegionBossRules.AuroraRouteHits(1, aurora, aurora + Vector2.up * 6f), "the next dance lights another route");
            Assert.IsFalse(RegionBossRules.AuroraRouteHits(1, aurora, aurora + RegionBossRules.AuroraRouteDirection(1) * 6f));
            Assert.IsFalse(RegionBossRules.AuroraRouteHits(0, aurora, aurora + Vector2.down * (RegionBossRules.AuroraReach + 0.5f)), "beyond her reach");
            Assert.Less(RegionBossRules.AuroraReach, RegionBossRules.AbilityRange, "the far side is still inside the fight");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.TrappedAuroraId).StrongerAtNight);
        }

        [Test]
        public void Guayota_SpitsOnEveryOtherSquare_TheOthersTheNextTime()
        {
            Vector2 crater = new Vector2(100f, 100f);
            float half = RegionBossRules.CraterSquare * 0.5f;
            Vector2 square = crater + new Vector2(half, half);
            Vector2 next = square + Vector2.right * RegionBossRules.CraterSquare;
            Assert.AreNotEqual(RegionBossRules.CraterSquareIsHot(0, crater, square), RegionBossRules.CraterSquareIsHot(0, crater, next), "neighbouring squares differ");
            Assert.AreEqual(RegionBossRules.CraterSquareIsHot(0, crater, square), RegionBossRules.CraterSquareIsHot(0, crater, square + new Vector2(1f, 1f) * RegionBossRules.CraterSquare), "diagonal squares match");
            Assert.AreNotEqual(RegionBossRules.CraterSquareIsHot(0, crater, square), RegionBossRules.CraterSquareIsHot(1, crater, square), "the next eruption flips the board");
            Assert.IsTrue(RegionBossRules.CraterSpitHits(0, crater, square), "his own square, the first time");
            Assert.IsFalse(RegionBossRules.CraterSpitHits(0, crater, next), "a cold square");
            Assert.IsFalse(RegionBossRules.CraterSpitHits(0, crater, crater + new Vector2(half, RegionBossRules.CraterReach + 2f * RegionBossRules.CraterSquare + half)), "beyond his reach");
            Assert.Less(RegionBossRules.CraterSquare, 6f, "a cold square is always a short step away");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GuayotaId).StrongerAtNight, "the one who keeps the sun in the dark");
        }

        [Test]
        public void WrongKnot_TheLoopsCloseAside_TheLineIsSafe()
        {
            Vector2 knot = new Vector2(0f, 0f);
            Vector2 mark = new Vector2(0f, 6f);
            Vector2[] loops = RegionBossRules.KnotLoopCentres(knot, mark);
            Assert.AreEqual(2, loops.Length);
            foreach (Vector2 loop in loops)
            {
                Assert.AreEqual(RegionBossRules.KnotLoopOffset, Vector2.Distance(knot, loop), 1e-4f);
                Assert.AreEqual(0f, Vector2.Dot(loop - knot, mark - knot), 1e-3f, "across the line to the hero");
                Assert.IsTrue(RegionBossRules.KnotLoopsHit(knot, mark, loop), "stepped into a loop");
            }

            Assert.IsFalse(RegionBossRules.KnotLoopsHit(knot, mark, mark), "stayed on the line, where it was");
            Assert.IsFalse(RegionBossRules.KnotLoopsHit(knot, mark, knot + Vector2.up * 2f), "came in along the line");
            Assert.IsFalse(RegionBossRules.KnotLoopsHit(knot, mark, knot + Vector2.down * 3f), "behind it, on the line");
            Assert.IsFalse(RegionBossRules.KnotLoopsHit(knot, mark, knot), "the middle of the knot");
            Assert.IsTrue(RegionBossRules.KnotLoopsHit(knot, mark, knot + Vector2.right * 3f), "a step aside");
            Assert.IsFalse(RegionBossRules.KnotLoopsHit(knot, mark, knot + Vector2.right * (RegionBossRules.KnotLoopOffset + RegionBossRules.KnotLoopRadius + 0.2f)), "far aside");
            Assert.Less(RegionBossRules.KnotLoopRadius, RegionBossRules.KnotLoopOffset, "the line between the loops is clear");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.WrongKnotId).StrongerAtNight, "the hours of the night are Mehen's");
        }

        [Test]
        public void DarkWingedEagle_TheWingsFallInTwoBars_UnderHerIsSafe()
        {
            Vector2 eagle = new Vector2(10f, 0f);
            Vector2 mark = new Vector2(10f, 8f);
            Assert.IsTrue(RegionBossRules.WingSpanHits(eagle, mark, mark), "stayed on the mark");
            Assert.IsTrue(RegionBossRules.WingSpanHits(eagle, mark, mark + Vector2.right * (RegionBossRules.WingSpanHalf - 0.5f)), "ran aside: the wings are too wide");
            Assert.IsTrue(RegionBossRules.WingSpanHits(eagle, mark, mark + Vector2.up * RegionBossRules.WingBeatGap), "ran on: the second beat");
            Assert.IsFalse(RegionBossRules.WingSpanHits(eagle, mark, mark + Vector2.up * (RegionBossRules.WingBeatGap * 0.5f)), "between the two beats");
            Assert.IsFalse(RegionBossRules.WingSpanHits(eagle, mark, mark + Vector2.down * (RegionBossRules.WingBarHalfDepth + 0.5f)), "stepped in, under her");
            Assert.IsFalse(RegionBossRules.WingSpanHits(eagle, mark, mark + Vector2.right * (RegionBossRules.WingSpanHalf + 0.5f)), "past the wingtip");
            Assert.Greater(RegionBossRules.WingBeatGap, RegionBossRules.WingBarHalfDepth * 2f, "a gap between the beats");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.DarkWingedEagleId).StrongerAtNight, "the ficha: night is the same in the pit");
        }

        [Test]
        public void SailShadow_TheBoomSweepsOneSide_TheMastAndTheOtherSideAreSafe()
        {
            Vector2 mast = new Vector2(-20f, 40f);
            Assert.IsTrue(RegionBossRules.BoomSwingsStarboard(0));
            Assert.IsFalse(RegionBossRules.BoomSwingsStarboard(1));
            Assert.IsTrue(RegionBossRules.BoomSwingsStarboard(2));
            Assert.IsTrue(RegionBossRules.BoomSwingHits(true, mast, mast + Vector2.right * 5f), "starboard, under the boom");
            Assert.IsFalse(RegionBossRules.BoomSwingHits(true, mast, mast + Vector2.left * 5f), "the other side");
            Assert.IsTrue(RegionBossRules.BoomSwingHits(false, mast, mast + Vector2.left * 5f), "port, the next swing");
            Assert.IsFalse(RegionBossRules.BoomSwingHits(true, mast, mast + Vector2.right * (RegionBossRules.BoomInner - 0.5f)), "hugging the mast");
            Assert.IsFalse(RegionBossRules.BoomSwingHits(true, mast, mast + Vector2.right * (RegionBossRules.BoomOuter + 0.5f)), "past the boom's end");
            Assert.IsFalse(RegionBossRules.BoomSwingHits(true, mast, mast + Vector2.up * 5f), "straight fore of the mast, on the keel line");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.SailShadowId).StrongerAtNight, "there is no night on the Sun deck");
        }

        // ---------------------------------------------------------------- the call-outs and the model

        [Test]
        public void TheUpperMapBosses_CallOutTheirGustsRouteLoopsAndBoom()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string>
            {
                RegionBossTexts.AysGustKey(0), RegionBossTexts.AysGustKey(1), RegionBossTexts.AuroraRouteKey, RegionBossTexts.KnotLoopKey,
                RegionBossTexts.BoomSideKey(true), RegionBossTexts.BoomSideKey(false),
            };
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
            Assert.AreEqual(RegionBossTexts.AysGustKey(0), RegionBossTexts.AysGustKey(2), "the gusts come round");
        }

        [TestCase(ProceduralEnemyKind.Guayota)]
        [TestCase(ProceduralEnemyKind.WrongKnot)]
        [TestCase(ProceduralEnemyKind.SailShadow)]
        public void TheOnesWithArms_RaiseThem_OnTheWindup(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform arm = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion rest = arm.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }

            Assert.Greater(Quaternion.Angle(rest, arm.localRotation), 30f, kind.ToString());
        }
    }
}
