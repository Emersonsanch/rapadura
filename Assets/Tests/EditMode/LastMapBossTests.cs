using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The last map bosses of Regions 1–9 (Docs/Mapas100.md 16, 28, 29, 44, 49, 50, 51): the list, their strength against their region's world boss, each one's
    /// own ability as pure rules, the herd's calls and the arms that rise on the windup. The shared checks (asset, biome, model, page, arena) are
    /// <see cref="MapBossTests"/>'s, which lists them too.
    /// </summary>
    public class LastMapBossTests
    {
        private static readonly string[] LastIds =
        {
            RegionBossRules.MazanMasonId, RegionBossRules.GreyMorayId, RegionBossRules.PalaceCrabId, RegionBossRules.GoldenScarecrowId,
            RegionBossRules.TabletNightmareId, RegionBossRules.AzSawId, RegionBossRules.GreyStallionId,
        };

        private static readonly string[] LastMaps =
        {
            OverworldLayouts.PedraSecaId, OverworldLayouts.KirtaMapId, OverworldLayouts.KilwaId, OverworldLayouts.ArrozaisMapId,
            OverworldLayouts.FortimMapId, OverworldLayouts.HaripushpaMapId, OverworldLayouts.MilCavalosMapId,
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

        private static float EventWorldBossHealth(BiomeType region) => Load(WorldBossRules.For(region).DayEnemyId).MaxHealth * WorldBossRules.HealthCommons;

        // ---------------------------------------------------------------- the list

        [Test]
        public void TheLastMaps_GetTheirFichaBoss_AtTheTopOfTheirBand_WhereNoOtherBossStands()
        {
            for (int i = 0; i < LastIds.Length; i++)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(LastIds[i]);
                Assert.IsNotNull(spawn, LastIds[i]);
                Assert.IsTrue(RegionBossRules.IsMapBoss(LastIds[i]), LastIds[i]);
                Assert.AreEqual(LastMaps[i], spawn.MapId, LastIds[i]);
                MapDefinition map = MapCatalog.Get(spawn.MapId);
                Assert.IsNotNull(map, spawn.MapId);
                Assert.AreEqual(map.MonsterLevelMax, spawn.Level, $"{LastIds[i]}: the top of its map's band");
                Assert.That(map.MonsterLevelMax, Is.InRange(1, 149), $"{LastIds[i]}: Regions 1–9");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == spawn.MapId), $"{spawn.MapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == spawn.MapId), spawn.MapId);
                Assert.Less(spawn.Level, RegionBossRules.SpawnOf(RegionBossRules.OhyaId).Level, $"{LastIds[i]}: below the band of the letters");
            }
        }

        [Test]
        public void TheLastMapBosses_AreWeakerThanTheWorldBossOfTheirRegion()
        {
            // Region 3 against its scheduled event, 5 against Hedammu, 8 against the Four Gluttons, 9 against Ohya.
            Assert.Less(Load(RegionBossRules.MazanMasonId).MaxHealth, EventWorldBossHealth(BiomeType.Mountain));
            var against = new[]
            {
                (RegionBossRules.GreyMorayId, RegionBossRules.HedammuId), (RegionBossRules.PalaceCrabId, RegionBossRules.HedammuId),
                (RegionBossRules.GoldenScarecrowId, RegionBossRules.FourGluttonsId), (RegionBossRules.TabletNightmareId, RegionBossRules.OhyaId),
                (RegionBossRules.AzSawId, RegionBossRules.OhyaId), (RegionBossRules.GreyStallionId, RegionBossRules.OhyaId),
            };
            foreach ((string map, string world) in against)
            {
                Assert.Less(Load(map).MaxHealth, Load(world).MaxHealth, map);
                Assert.Less(Load(map).AttackDamage, Load(world).AttackDamage, map);
            }

            foreach (string id in LastIds)
            {
                Assert.Less(Load(id).MaxHealth, 1180, $"{id}: below the strongest boss of the first regions (Tages)");
                Assert.Less(Load(id).BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, id);
            }
        }

        [Test]
        public void EachSignatureIsNewToTheDirector()
        {
            RegionBossSignature[] last =
            {
                RegionBossSignature.TurningWalls, RegionBossSignature.StillPrey, RegionBossSignature.SidewaysScuttle, RegionBossSignature.ScareOff,
                RegionBossSignature.FearGrip, RegionBossSignature.OutrunSaw, RegionBossSignature.HerdRun,
            };
            CollectionAssert.AreEquivalent(last, LastIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in last)
            {
                Assert.Greater((int)signature, (int)RegionBossSignature.SmokeCorral, $"{signature}: after the ones that already exist");
                Assert.AreEqual(1, RegionBossRules.DirectorSpawns.Count(s => s.Signature == signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureInterval(signature), RegionBossRules.SignatureWindup(signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureWindup(signature), 1f, $"{signature}: time to read the warning");
            }
        }

        // ---------------------------------------------------------------- the abilities

        [Test]
        public void MazanMason_CrushesTheQuarterTheHeroStoodIn_TheWallsTurn()
        {
            Vector2 mason = new Vector2(20f, -5f);
            Assert.AreEqual(0f, RegionBossRules.MasonWallTurn(0));
            Assert.AreEqual(RegionBossRules.MasonWallTurnDegrees, RegionBossRules.MasonWallTurn(1));
            Assert.AreEqual(0f, RegionBossRules.MasonWallTurn(2), "straight again");

            // Build 0 (a +): north-east of him is one quarter, south-east the next.
            Vector2 northEast = mason + new Vector2(4f, 4f);
            Vector2 southEast = mason + new Vector2(4f, -4f);
            int walled = RegionBossRules.MasonQuarterOf(0, mason, northEast);
            Assert.AreNotEqual(walled, RegionBossRules.MasonQuarterOf(0, mason, southEast));
            Assert.IsTrue(RegionBossRules.MasonWallHits(0, mason, walled, northEast), "stayed in the walled quarter");
            Assert.IsTrue(RegionBossRules.MasonWallHits(0, mason, walled, mason + new Vector2(1f, 8f)), "anywhere in it");
            Assert.IsFalse(RegionBossRules.MasonWallHits(0, mason, walled, southEast), "climbed over into the next quarter");
            Assert.IsFalse(RegionBossRules.MasonWallHits(0, mason, walled, mason + new Vector2(-4f, 4f)), "or the other one");
            Assert.IsFalse(RegionBossRules.MasonWallHits(0, mason, walled, mason + new Vector2(1f, 1f).normalized * (RegionBossRules.MasonReach + 0.5f)), "out of his reach");

            // Build 1 (an ×): due north and due east are no longer in the same quarter.
            Vector2 north = mason + Vector2.up * 5f;
            Vector2 east = mason + Vector2.right * 5f;
            Assert.AreEqual(RegionBossRules.MasonQuarterOf(0, mason, north), RegionBossRules.MasonQuarterOf(0, mason, mason + new Vector2(1f, 5f)));
            Assert.AreNotEqual(RegionBossRules.MasonQuarterOf(1, mason, north), RegionBossRules.MasonQuarterOf(1, mason, east), "the slanted walls split them");
            for (int build = 0; build < 4; build++)
            {
                Assert.That(RegionBossRules.MasonQuarterOf(build, mason, mason + new Vector2(-3f, -7f)), Is.InRange(0, 3));
            }

            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.MazanMasonId).StrongerAtNight);
        }

        [Test]
        public void GreyMoray_BitesWhoeverStoodStill_KeepingOnTheMoveIsSafe()
        {
            Vector2 a = new Vector2(3f, 3f);
            Assert.IsTrue(RegionBossRules.MorayLookIsStill(a, a));
            Assert.IsTrue(RegionBossRules.MorayLookIsStill(a, a + Vector2.right * (RegionBossRules.MorayStillStep - 0.1f)), "a shuffle is still standing");
            Assert.IsFalse(RegionBossRules.MorayLookIsStill(a, a + Vector2.right * (RegionBossRules.MorayStillStep + 0.1f)), "a step");
            Assert.IsTrue(RegionBossRules.MorayBites(RegionBossRules.MorayStillSeconds, 5f), "stood still two looks running");
            Assert.IsTrue(RegionBossRules.MorayBites(RegionBossRules.MorayLookSeconds * 2f, 5f));
            Assert.IsFalse(RegionBossRules.MorayBites(RegionBossRules.MorayLookSeconds, 5f), "one still look is not enough");
            Assert.IsFalse(RegionBossRules.MorayBites(0f, 1f), "kept moving, even right next to her");
            Assert.IsFalse(RegionBossRules.MorayBites(RegionBossRules.MorayWindupSeconds, RegionBossRules.MorayReach + 0.5f), "out of her reach");
            Assert.Less(RegionBossRules.MorayStillSeconds, RegionBossRules.MorayWindupSeconds, "the warning is long enough to be caught standing");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreyMorayId).StrongerAtNight, "the ficha: at night the grey coral is dark and the sharks come out");
        }

        [Test]
        public void PalaceCrab_ScuttlesAcrossTheHeroSpot_ForwardOrBackIsSafe_SidewaysIsNot()
        {
            Vector2 crab = new Vector2(0f, 0f);
            Vector2 mark = new Vector2(0f, 8f);
            Assert.IsTrue(RegionBossRules.ScuttleHits(crab, mark, mark), "stayed on the spot");
            Assert.IsTrue(RegionBossRules.ScuttleHits(crab, mark, mark + Vector2.right * 5f), "stepped sideways: still on the row");
            Assert.IsTrue(RegionBossRules.ScuttleHits(crab, mark, mark + Vector2.left * (RegionBossRules.ScuttleHalfLength - 0.1f)));
            Assert.IsFalse(RegionBossRules.ScuttleHits(crab, mark, mark + Vector2.down * (RegionBossRules.ScuttleHalfWidth + 0.1f)), "a step towards him");
            Assert.IsFalse(RegionBossRules.ScuttleHits(crab, mark, mark + Vector2.up * (RegionBossRules.ScuttleHalfWidth + 0.1f)), "a step away");
            Assert.IsFalse(RegionBossRules.ScuttleHits(crab, mark, mark + Vector2.right * (RegionBossRules.ScuttleHalfLength + 0.1f)), "past the end of the row");
            Assert.IsTrue(RegionBossRules.ScuttleHits(crab, crab, crab), "right under him");
            Assert.Less(RegionBossRules.ScuttleHalfWidth * 2f, RegionBossRules.ScuttleHalfLength, "a long row, narrow across");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.PalaceCrabId).StrongerAtNight);
        }

        [Test]
        public void GoldenScarecrow_OnlyThoseWhoBackedAwayAreSafe()
        {
            Assert.IsTrue(RegionBossRules.ScareHits(5f, 5f), "stood their ground");
            Assert.IsTrue(RegionBossRules.ScareHits(5f, 3f), "came closer");
            Assert.IsTrue(RegionBossRules.ScareHits(5f, 5f + RegionBossRules.ScareFlee - 0.1f), "not quite far enough");
            Assert.IsFalse(RegionBossRules.ScareHits(5f, 5f + RegionBossRules.ScareFlee + 0.1f), "backed away");
            Assert.IsFalse(RegionBossRules.ScareHits(RegionBossRules.ScareReach - 1f, RegionBossRules.ScareReach + 0.5f), "left his reach");
            Assert.IsFalse(RegionBossRules.ScareHits(20f, 20f), "never was in reach");
            Assert.Less(RegionBossRules.ScareFlee, RegionBossRules.ScareReach);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GoldenScarecrowId).StrongerAtNight, "the ficha: the moon in the paddies");
        }

        [Test]
        public void TabletNightmare_GripsTheWeak_TheHealthyOnlyHaveABadDream()
        {
            float max = 400f;
            Assert.AreEqual(RegionBossRules.NightmareGripFactor, RegionBossRules.NightmareDamageFactor(max * 0.3f, max, 5f), 1e-4f, "below half: gripped");
            Assert.AreEqual(RegionBossRules.NightmareDreamFactor, RegionBossRules.NightmareDamageFactor(max, max, 5f), 1e-4f, "at full health: a bad dream");
            Assert.AreEqual(RegionBossRules.NightmareDreamFactor, RegionBossRules.NightmareDamageFactor(max * RegionBossRules.NightmareFearFraction, max, 5f), 1e-4f, "exactly half is not afraid");
            Assert.AreEqual(0f, RegionBossRules.NightmareDamageFactor(max * 0.1f, max, RegionBossRules.NightmareReach + 0.5f), "out of its reach");
            Assert.AreEqual(0f, RegionBossRules.NightmareDamageFactor(10f, 0f, 1f), "no health to read");
            Assert.Greater(RegionBossRules.NightmareGripFactor, RegionBossRules.NightmareDreamFactor * 3f, "healing pays");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.TabletNightmareId).StrongerAtNight, "bad dreams at night");
        }

        [Test]
        public void AzSaw_CutsWhoeverDidNotGetRoundIt_RunningStraightAwayIsNoGood()
        {
            Vector2 saw = new Vector2(-10f, 40f);
            Vector2 start = saw + Vector2.up * 6f;
            Assert.IsTrue(RegionBossRules.SawHits(saw, start, start), "stood still");
            Assert.IsTrue(RegionBossRules.SawHits(saw, start, saw + Vector2.up * 10f), "ran straight away, still in reach");
            Vector2 round = saw + new Vector2(Mathf.Sin(70f * Mathf.Deg2Rad), Mathf.Cos(70f * Mathf.Deg2Rad)) * 6f;
            Assert.IsFalse(RegionBossRules.SawHits(saw, start, round), "got 70 degrees round it");
            Vector2 short_ = saw + new Vector2(Mathf.Sin(30f * Mathf.Deg2Rad), Mathf.Cos(30f * Mathf.Deg2Rad)) * 6f;
            Assert.IsTrue(RegionBossRules.SawHits(saw, start, short_), "only 30 degrees round");
            Assert.IsFalse(RegionBossRules.SawHits(saw, start, saw + Vector2.up * (RegionBossRules.SawReach + 0.5f)), "out of its reach");
            Assert.IsTrue(RegionBossRules.SawHits(saw, start, saw), "under the blade");
            Assert.Less(RegionBossRules.SawOutrunDegrees, 90f, "a quarter round is always enough");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.AzSawId).StrongerAtNight);
        }

        [Test]
        public void GreyStallion_TurnsTheHerdClockwise_RunningWithItIsSafe()
        {
            Assert.AreEqual(HerdWay.North, RegionBossRules.HerdWayFor(0));
            Assert.AreEqual(HerdWay.East, RegionBossRules.HerdWayFor(1));
            Assert.AreEqual(HerdWay.South, RegionBossRules.HerdWayFor(2));
            Assert.AreEqual(HerdWay.West, RegionBossRules.HerdWayFor(3));
            Assert.AreEqual(HerdWay.North, RegionBossRules.HerdWayFor(4), "and round again");
            Assert.AreEqual(Vector2.up, RegionBossRules.HerdDirection(HerdWay.North), "+y is north");
            Assert.AreEqual(Vector2.right, RegionBossRules.HerdDirection(HerdWay.East));

            Vector2 stallion = new Vector2(100f, 100f);
            Vector2 start = stallion + new Vector2(4f, 0f);
            Assert.IsTrue(RegionBossRules.HerdRunHits(HerdWay.North, stallion, start, start), "stood still");
            Assert.IsFalse(RegionBossRules.HerdRunHits(HerdWay.North, stallion, start, start + Vector2.up * (RegionBossRules.HerdRunWith + 0.1f)), "ran north with the herd");
            Assert.IsTrue(RegionBossRules.HerdRunHits(HerdWay.North, stallion, start, start + Vector2.right * 5f), "ran across it");
            Assert.IsTrue(RegionBossRules.HerdRunHits(HerdWay.North, stallion, start, start + Vector2.down * 5f), "ran against it");
            Assert.IsFalse(RegionBossRules.HerdRunHits(HerdWay.East, stallion, start, start + Vector2.right * 5f), "the next run is east");
            Assert.IsFalse(RegionBossRules.HerdRunHits(HerdWay.South, stallion, stallion + Vector2.left * 20f, stallion + Vector2.left * 20f), "far from the herd");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.GreyStallionId).StrongerAtNight);
        }

        // ---------------------------------------------------------------- the page and the model

        [Test]
        public void GreyStallion_CallsOutEachWay_InBothLanguages()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = System.Enum.GetValues(typeof(HerdWay)).Cast<HerdWay>().Select(RegionBossTexts.HerdWayKey).ToList();
            Assert.AreEqual(RegionBossRules.HerdWays, keys.Count);
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
            foreach (string id in LastIds)
            {
                CollectionAssert.Contains(RegionBossTexts.EnemyIds, id);
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.SignatureKey(id)), id);
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.WithdrawKey(id)), id);
            }
        }

        [TestCase(ProceduralEnemyKind.MazanMason)]
        [TestCase(ProceduralEnemyKind.PalaceCrab)]
        [TestCase(ProceduralEnemyKind.GoldenScarecrow)]
        [TestCase(ProceduralEnemyKind.TabletNightmare)]
        [TestCase(ProceduralEnemyKind.AzSaw)]
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

        [Test]
        public void EveryOne_IsAMapBossModel_OfItsOwnKind_InTheReservedRange()
        {
            var kinds = new HashSet<ProceduralEnemyKind>();
            foreach (string id in LastIds)
            {
                ProceduralEnemyKind kind = ProceduralEnemyModels.KindFor(id);
                Assert.That((int)kind, Is.InRange(270, 279), id);
                Assert.IsTrue(ProceduralEnemyModels.IsMapBoss(kind), id);
                CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kind);
                Assert.IsTrue(kinds.Add(kind), $"{id}: a model of its own");
            }
        }
    }
}
