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
    /// The last four map bosses of the Reinos do Céu (Docs/Mapas100.md 92, 94, 98, 99): the list, their strength against the region's world boss, each one's
    /// own ability as pure rules, their call-outs and the arms that rise on the windup. The shared checks (asset, biome, model, page, arena) are
    /// <see cref="MapBossTests"/>'s, which lists them too.
    /// </summary>
    public class SkyMapBossTests
    {
        private static readonly string[] SkyIds =
        {
            RegionBossRules.LostRowerId, RegionBossRules.WheelArchonId, RegionBossRules.LastStepArchonId, RegionBossRules.GreatCrackId,
        };

        private static readonly string[] SkyMaps =
        {
            OverworldLayouts.RioDeLuzMapId, OverworldLayouts.RodaCelesteMapId, OverworldLayouts.PesDaColunaMapId, OverworldLayouts.MiranteCeuMapId,
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
        public void TheSkyMaps_GetTheirFichaBoss_AtTheTopOfTheirBand()
        {
            for (int i = 0; i < SkyIds.Length; i++)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(SkyIds[i]);
                Assert.IsNotNull(spawn, SkyIds[i]);
                Assert.IsTrue(RegionBossRules.IsMapBoss(SkyIds[i]), SkyIds[i]);
                Assert.AreEqual(SkyMaps[i], spawn.MapId, SkyIds[i]);
                MapDefinition map = MapCatalog.Get(spawn.MapId);
                Assert.IsNotNull(map, spawn.MapId);
                Assert.AreEqual(MapRegion.ReinosDoCeu, map.Region, spawn.MapId);
                Assert.AreEqual(map.MonsterLevelMax, spawn.Level, $"{SkyIds[i]}: the top of its map's band");
                Assert.AreEqual(OverworldLayouts.CeuWorldOffset, spawn.WorldOffset, SkyIds[i]);
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == spawn.MapId), $"{spawn.MapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == spawn.MapId), spawn.MapId);
                Assert.IsTrue(spawn.NeverDies, $"{SkyIds[i]}: freed, it leaves with its line");
            }

            Assert.IsFalse(RegionBossRules.MapSpawns.Any(s => s.MapId == OverworldLayouts.VarandaMapId), "map 100 has no fight by its ficha");
            CollectionAssert.AllItemsAreUnique(SkyIds.Select(id => RegionBossRules.SpawnOf(id).LocalArena));
        }

        [Test]
        public void TheSkyMapBosses_AreWeakerThanTheWorldBossOfTheirRegion()
        {
            foreach (string id in SkyIds)
            {
                EnemyDefinition boss = Load(id);
                Assert.IsTrue(boss.IsBoss, id);
                Assert.AreEqual(RegionBossRules.SpawnOf(id).Level, boss.Level, id);
                Assert.Less(boss.MaxHealth, Load(RegionBossRules.AzCaptainId).MaxHealth, id);
                Assert.Less(boss.AttackDamage, Load(RegionBossRules.AzCaptainId).AttackDamage, id);
                Assert.Less(boss.BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, id);
                Assert.AreEqual(RegionBossRules.MapBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(id, true), id);
            }
        }

        [Test]
        public void EachSignatureIsNewToTheDirector()
        {
            RegionBossSignature[] sky =
            {
                RegionBossSignature.DownstreamWake, RegionBossSignature.TurningJars, RegionBossSignature.HeldStep, RegionBossSignature.WideningCrack,
            };
            CollectionAssert.AreEquivalent(sky, SkyIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in sky)
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
        public void LostRower_TheWakeRunsDownstream_UpstreamAndAsideAreSafe()
        {
            Vector2 mark = new Vector2(12f, -4f);
            Vector2 down = RegionBossRules.RowerDownstream;
            Vector2 aside = new Vector2(-down.y, down.x);
            Assert.AreEqual(Vector2.right, down, "the Rio de Luz runs east");
            Assert.IsTrue(RegionBossRules.RowerWakeHits(mark, mark), "stayed on the mark");
            Assert.IsTrue(RegionBossRules.RowerWakeHits(mark, mark + down * (RegionBossRules.RowerWakeLength - 0.5f)), "ran downstream, with the current");
            Assert.IsFalse(RegionBossRules.RowerWakeHits(mark, mark - down * (RegionBossRules.RowerWakeHalfWidth + 0.2f)), "a step upstream");
            Assert.IsFalse(RegionBossRules.RowerWakeHits(mark, mark + down * 3f + aside * (RegionBossRules.RowerWakeHalfWidth + 0.2f)), "aside of the wake");
            Assert.IsFalse(RegionBossRules.RowerWakeHits(mark, mark + down * (RegionBossRules.RowerWakeLength + RegionBossRules.RowerWakeHalfWidth + 0.2f)), "past its end");
            Assert.Less(RegionBossRules.RowerWakeHalfWidth, 3f, "a step is enough");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.LostRowerId).StrongerAtNight, "the ficha: at night the river only turns silver");
        }

        [Test]
        public void WheelArchon_EveryJarSpillsButTheOneThatTurns_AndTheHub()
        {
            Vector2 centre = new Vector2(-30f, 50f);
            Assert.AreEqual(0, RegionBossRules.WheelJarOf(centre, centre + Vector2.up * 5f), "jar 0 is north");
            Assert.AreEqual(3, RegionBossRules.WheelJarOf(centre, centre + Vector2.right * 5f), "clockwise: east is a quarter on");
            Assert.AreEqual(0f, Vector2.Distance(RegionBossRules.WheelJarPosition(centre, 3), centre + Vector2.right * RegionBossRules.WheelJarRing), 1e-3f);
            Assert.AreEqual(RegionBossRules.WheelJarPosition(centre, 0), RegionBossRules.WheelJarPosition(centre, RegionBossRules.WheelJars), "the jars come round");

            Vector2 mark = centre + Vector2.up * RegionBossRules.WheelJarRing;
            int lit = RegionBossRules.WheelLitJar(centre, mark);
            Assert.AreEqual(RegionBossRules.WheelTurnJars, lit, "the hero's jar, a quarter on");
            Assert.IsTrue(RegionBossRules.WheelLockHits(centre, lit, mark), "stayed by the jar that stopped");
            Assert.IsFalse(RegionBossRules.WheelLockHits(centre, lit, RegionBossRules.WheelJarPosition(centre, lit)), "went where the jar went");
            Assert.IsFalse(RegionBossRules.WheelLockHits(centre, lit, centre + Vector2.left * (RegionBossRules.WheelHubRadius - 0.3f)), "on the hub");
            Assert.IsTrue(RegionBossRules.WheelLockHits(centre, lit, centre + Vector2.down * 5f), "the other side of the wheel");
            Assert.IsFalse(RegionBossRules.WheelLockHits(centre, lit, centre + Vector2.down * (RegionBossRules.WheelReach + 0.5f)), "beyond his reach");
            Assert.Less(RegionBossRules.WheelJarRing + RegionBossRules.WheelJarRadius, RegionBossRules.WheelReach, "the lit jar is inside the reach");
            Assert.Less(RegionBossRules.WheelReach, RegionBossRules.AbilityRange, "the far side is still inside the fight");
            Assert.Less(RegionBossRules.WheelHubRadius, RegionBossRules.WheelJarRing - RegionBossRules.WheelJarRadius, "the hub and the jars don't touch");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.WheelArchonId).StrongerAtNight, "the ficha: at night the wheel only turns slower");
        }

        [Test]
        public void LastStepArchon_NobodyGoesUp_AStepBelowIsSafe()
        {
            Vector2 archon = new Vector2(4f, 4f);
            Vector2 up = RegionBossRules.StepUphill;
            Vector2 column = OverworldLayouts.GloryColumnCenter - OverworldLayouts.UltimoArconteArena;
            Assert.Greater(Vector2.Dot(up, column), 0f, "uphill is towards the Coluna");
            Assert.AreEqual(1f, up.magnitude, 1e-4f);
            Vector2 aside = new Vector2(-up.y, up.x);
            Assert.IsTrue(RegionBossRules.HeldStepHits(archon, up, archon + up * 3f), "above him");
            Assert.IsTrue(RegionBossRules.HeldStepHits(archon, up, archon + aside * 5f), "on his step, beside him");
            Assert.IsTrue(RegionBossRules.HeldStepHits(archon, up, archon - up * (RegionBossRules.StepHalfDepth - 0.3f)), "still on his step");
            Assert.IsFalse(RegionBossRules.HeldStepHits(archon, up, archon - up * (RegionBossRules.StepHalfDepth + 0.5f)), "a step below him");
            Assert.IsFalse(RegionBossRules.HeldStepHits(archon, up, archon + up * (RegionBossRules.StepReach + 0.5f)), "beyond his reach");
            Assert.Less(RegionBossRules.StepReach, RegionBossRules.AbilityRange);
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.LastStepArchonId).StrongerAtNight);
        }

        [Test]
        public void GreatCrack_OpensWiderEachDrip_AsideIsSafe()
        {
            Vector2 crack = new Vector2(-8f, 20f);
            Vector2 along = RegionBossRules.CrackAxis;
            Vector2 aside = new Vector2(-along.y, along.x);
            Assert.AreEqual(1f, along.magnitude, 1e-4f);
            for (int drip = 1; drip < RegionBossRules.CrackHalfWidths.Length; drip++)
            {
                Assert.Greater(RegionBossRules.CrackHalfWidthOf(drip), RegionBossRules.CrackHalfWidthOf(drip - 1), "wider each drip");
            }

            Assert.AreEqual(RegionBossRules.CrackHalfWidthOf(0), RegionBossRules.CrackHalfWidthOf(RegionBossRules.CrackHalfWidths.Length), "then it closes and starts over");
            Assert.IsTrue(RegionBossRules.CrackDripHits(0, crack, crack + along * 6f), "on its line");
            Vector2 twoSteps = crack + aside * 2.2f;
            Assert.IsFalse(RegionBossRules.CrackDripHits(0, crack, twoSteps), "narrow: aside is safe");
            Assert.IsTrue(RegionBossRules.CrackDripHits(1, crack, twoSteps), "wider: the same spot is caught");
            Assert.IsFalse(RegionBossRules.CrackDripHits(2, crack, crack + aside * (RegionBossRules.CrackHalfWidthOf(2) + 0.3f)), "far aside, even at its widest");
            Assert.IsFalse(RegionBossRules.CrackDripHits(2, crack, crack + along * (RegionBossRules.CrackHalfLength + 0.5f)), "past its end");
            Assert.Less(RegionBossRules.CrackHalfLength, RegionBossRules.AbilityRange);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreatCrackId).StrongerAtNight, "the ficha: shadow drips better in the dark");
        }

        // ---------------------------------------------------------------- the call-outs and the model

        [Test]
        public void TheSkyMapBosses_CallOutTheirWakeJarStepAndCrack()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { RegionBossTexts.RowerWakeKey, RegionBossTexts.WheelJarKey, RegionBossTexts.StepBelowKey };
            keys.AddRange(Enumerable.Range(0, RegionBossRules.CrackHalfWidths.Length).Select(RegionBossTexts.CrackWidthKey));
            foreach (string id in SkyIds)
            {
                keys.Add(RegionBossTexts.SignatureKey(id));
                keys.Add(RegionBossTexts.WithdrawKey(id));
            }

            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
            Assert.AreEqual(RegionBossTexts.CrackWidthKey(0), RegionBossTexts.CrackWidthKey(RegionBossRules.CrackHalfWidths.Length), "the widths come round");
            foreach (string id in SkyIds)
            {
                CollectionAssert.Contains(RegionBossTexts.EnemyIds, id);
            }
        }

        [TestCase(ProceduralEnemyKind.LostRower)]
        [TestCase(ProceduralEnemyKind.WheelArchon)]
        [TestCase(ProceduralEnemyKind.LastStepArchon)]
        [TestCase(ProceduralEnemyKind.GreatCrack)]
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

        [TestCase(ProceduralEnemyKind.LostRower)]
        [TestCase(ProceduralEnemyKind.WheelArchon)]
        [TestCase(ProceduralEnemyKind.LastStepArchon)]
        [TestCase(ProceduralEnemyKind.GreatCrack)]
        public void Model_IsAMapBoss_ForItsId(ProceduralEnemyKind kind)
        {
            Assert.IsTrue(ProceduralEnemyModels.IsMapBoss(kind), kind.ToString());
            Assert.IsTrue(ProceduralEnemyModels.IsBoss(kind), kind.ToString());
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kind);
            Assert.AreEqual(kind, ProceduralEnemyModels.KindFor(SkyIds.Single(id => ProceduralEnemyModels.KindFor(id) == kind)));
        }
    }
}
