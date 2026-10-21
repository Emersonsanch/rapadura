using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the region boss challenge (<see cref="RegionBossProgress"/>): the
    /// "Enfrentar o chefe" offer threshold, region bosses filtered out of random rolls, the next
    /// region unlocking only after the boss falls (<see cref="WorldMapGraph.ApplyUnlocks(WorldMapProgress, System.Predicate{BiomeType})"/>
    /// + <see cref="SceneTransitionService.CompleteEncounter"/>), legacy save compatibility and the
    /// <see cref="EncounterPayload.isBossChallenge"/> JSON round-trip.
    /// </summary>
    public class RegionBossChallengeTests
    {
        private class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Loaded = new List<string>();

            public void LoadScene(string sceneName) => Loaded.Add(sceneName);
        }

        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<BiomeType, BiomeDefinition> _biomes = new Dictionary<BiomeType, BiomeDefinition>();

        private EnemyDefinition _wolf;
        private EnemyDefinition _spider;
        private EnemyDefinition _alphaWolf;
        private EnemyDefinition _scorpion;
        private EnemyDefinition _ruinGuardian;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();

            _wolf = MakeEnemy("enemy_forest_wolf", false);
            _spider = MakeEnemy("enemy_forest_spider", false);
            _alphaWolf = MakeEnemy("enemy_forest_alpha_wolf", true);
            _scorpion = MakeEnemy("enemy_desert_scorpion", false);
            _ruinGuardian = MakeEnemy("enemy_desert_ruin_guardian", true);

            // Boss weight deliberately dominant so an unfiltered roll would pick it almost always.
            _biomes[BiomeType.Forest] = MakeBiome(BiomeType.Forest, (_wolf, 1), (_spider, 1), (_alphaWolf, 50));
            _biomes[BiomeType.Desert] = MakeBiome(BiomeType.Desert, (_scorpion, 1), (_ruinGuardian, 50));
            // Mountain has no boss in this fixture: gated by victories only.
            _biomes[BiomeType.Mountain] = MakeBiome(BiomeType.Mountain, (_scorpion, 1));
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            _biomes.Clear();
        }

        // ------------------------------------------------------------------
        // Helpers.
        // ------------------------------------------------------------------

        private EnemyDefinition MakeEnemy(string id, bool isBoss)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            FieldInfo field = typeof(EnemyDefinition).GetField("_isBoss", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(enemy, isBoss);
            _created.Add(enemy);
            return enemy;
        }

        private BiomeDefinition MakeBiome(BiomeType type, params (EnemyDefinition enemy, int weight)[] enemies)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            var entries = enemies.Select(e => new BiomeEnemySpawnEntry(e.enemy, e.weight)).ToList();
            biome.SetDataForTests(type, $"biome.{type.ToString().ToLowerInvariant()}.name", string.Empty, 15f, 25f, string.Empty, entries, null);
            _created.Add(biome);
            return biome;
        }

        private BiomeDefinition Lookup(BiomeType type) => _biomes.TryGetValue(type, out BiomeDefinition biome) ? biome : null;

        private RegionBossProgress MakeBosses(WorldMapGraph graph, WorldMapProgress progress, DefeatedEnemyRegistry registry = null)
        {
            return new RegionBossProgress(graph, progress, Lookup, registry);
        }

        private static void Win(WorldMapProgress progress, BiomeType region, int times)
        {
            for (int i = 0; i < times; i++)
            {
                progress.RecordVictory(region);
            }
        }

        // ------------------------------------------------------------------
        // Boss lookup and offer threshold.
        // ------------------------------------------------------------------

        [Test]
        public void BossFor_ReturnsTheBossEntryOfTheRegionTable()
        {
            RegionBossProgress bosses = MakeBosses(WorldMapGraph.CreateDefault(), new WorldMapProgress());

            Assert.AreSame(_alphaWolf, bosses.BossFor(BiomeType.Forest));
            Assert.AreSame(_ruinGuardian, bosses.BossFor(BiomeType.Desert));
            Assert.IsNull(bosses.BossFor(BiomeType.Mountain));
            Assert.IsNull(bosses.BossFor(BiomeType.Caves));
            Assert.IsTrue(bosses.IsRegionBoss(_alphaWolf));
            Assert.IsFalse(bosses.IsRegionBoss(_wolf));
        }

        [Test]
        public void CanChallengeBoss_OfferedOnlyAfterTheVictoryThreshold()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(graph, progress);

            Assert.AreEqual(WorldMapGraph.DefaultVictoriesToUnlock, bosses.VictoriesToChallenge(BiomeType.Forest));

            Win(progress, BiomeType.Forest, WorldMapGraph.DefaultVictoriesToUnlock - 1);
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Forest), "Offered before the threshold.");
            Assert.IsNull(bosses.BuildChallengePayload(BiomeType.Forest, new System.Random(1)));

            Win(progress, BiomeType.Forest, 1);
            Assert.IsTrue(bosses.CanChallengeBoss(BiomeType.Forest));

            // Locked or boss-less regions never offer it.
            Win(progress, BiomeType.Desert, 10);
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Desert), "Desert is still locked.");
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Mountain));
        }

        [Test]
        public void CanChallengeBoss_UsesTheNodeThresholdAndStopsOnceCleared()
        {
            var graph = new WorldMapGraph();
            graph.AddConnection(BiomeType.Forest, BiomeType.Desert);
            graph.SetUnlockRule(BiomeType.Desert, BiomeType.Forest, 5);
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(graph, progress);

            Win(progress, BiomeType.Forest, 4);
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Forest));
            Win(progress, BiomeType.Forest, 1);
            Assert.IsTrue(bosses.CanChallengeBoss(BiomeType.Forest));

            Assert.IsTrue(progress.MarkRegionCleared(BiomeType.Forest));
            Assert.IsFalse(progress.MarkRegionCleared(BiomeType.Forest), "Second mark is a no-op.");
            Assert.IsTrue(bosses.IsRegionCleared(BiomeType.Forest));
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Forest), "Cleared region still offers its boss.");
        }

        // ------------------------------------------------------------------
        // Random rolls never pick the region boss.
        // ------------------------------------------------------------------

        [Test]
        public void NormalRollFilter_ExcludesTheRegionBossWhileAlive()
        {
            RegionBossProgress bosses = MakeBosses(WorldMapGraph.CreateDefault(), new WorldMapProgress(), new DefeatedEnemyRegistry());
            var settings = new RandomEncounterSettings(EncounterStepMode.Distance, 1f, 1f, 0, 1f, 0f, 1f, 2, 2);
            var system = new RandomEncounterSystem(settings, 42) { EnemyFilter = bosses.CanRollInNormalEncounter, ChanceMultiplierProvider = null };

            for (int i = 0; i < 200; i++)
            {
                EncounterPayload payload = system.BuildPayload(_biomes[BiomeType.Forest]);
                Assert.IsNotNull(payload);
                CollectionAssert.DoesNotContain(payload.enemyIds, _alphaWolf.name, $"Roll {i} picked the region boss.");
                Assert.IsFalse(payload.isBossChallenge);
            }

            // Sanity: without the filter the dominant boss weight shows up.
            var unfiltered = new RandomEncounterSystem(settings, 42) { ChanceMultiplierProvider = null };
            bool sawBoss = Enumerable.Range(0, 50).Any(_ => unfiltered.BuildPayload(_biomes[BiomeType.Forest]).enemyIds.Contains(_alphaWolf.name));
            Assert.IsTrue(sawBoss);
        }

        [Test]
        public void NormalRollFilter_RejectsRegionBossesAndNull_AllowsRegulars()
        {
            var registry = new DefeatedEnemyRegistry();
            RegionBossProgress bosses = MakeBosses(WorldMapGraph.CreateDefault(), new WorldMapProgress(), registry);

            Assert.IsTrue(bosses.CanRollInNormalEncounter(_wolf));
            Assert.IsFalse(bosses.CanRollInNormalEncounter(_alphaWolf));
            Assert.IsFalse(bosses.CanRollInNormalEncounter(null));

            // A unique boss outside every region table keeps the old DefeatedEnemyRegistry gate.
            EnemyDefinition wanderer = MakeEnemy("enemy_wandering_boss", true);
            Assert.IsTrue(bosses.CanRollInNormalEncounter(wanderer));
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(wanderer));
            Assert.IsFalse(bosses.CanRollInNormalEncounter(wanderer));
        }

        // ------------------------------------------------------------------
        // Challenge payload.
        // ------------------------------------------------------------------

        [Test]
        public void BuildChallengePayload_BossFirstPlusRegularMinions_Flagged()
        {
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(WorldMapGraph.CreateDefault(), progress);
            Win(progress, BiomeType.Forest, 3);

            for (int seed = 0; seed < 30; seed++)
            {
                EncounterPayload payload = bosses.BuildChallengePayload(BiomeType.Forest, new System.Random(seed), 1, 2);
                Assert.IsNotNull(payload);
                Assert.IsTrue(payload.isBossChallenge);
                Assert.IsTrue(payload.IsBossChallenge);
                Assert.AreEqual(BiomeType.Forest, payload.biome);
                Assert.AreEqual(_alphaWolf.name, payload.enemyIds[0]);
                Assert.That(payload.EnemyCount, Is.InRange(2, 3));
                Assert.AreEqual(1, payload.enemyIds.Count(id => id == _alphaWolf.name), "Only one boss per challenge.");
                Assert.AreSame(_alphaWolf, payload.FindBoss());
            }

            EncounterPayload solo = bosses.BuildChallengePayload(BiomeType.Forest, new System.Random(3), 0, 0);
            CollectionAssert.AreEqual(new[] { _alphaWolf.name }, solo.enemyIds);
        }

        [Test]
        public void ChallengePayload_FlagSurvivesJsonRoundTrip()
        {
            EncounterPayload payload = EncounterPayload.Create(BiomeType.Desert, new[] { _ruinGuardian, _scorpion }, 77, isBossChallenge: true);

            EncounterPayload restored = EncounterPayload.FromJson(payload.ToJson());
            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.isBossChallenge);
            Assert.AreEqual(BiomeType.Desert, restored.biome);
            CollectionAssert.AreEqual(payload.enemyIds, restored.enemyIds);

            var lookup = new Dictionary<string, EnemyDefinition> { { _ruinGuardian.name, _ruinGuardian }, { _scorpion.name, _scorpion } };
            Assert.AreSame(_ruinGuardian, restored.FindBoss(id => lookup.TryGetValue(id, out EnemyDefinition e) ? e : null));

            // A random roll (and old JSON without the field) is not a challenge.
            Assert.IsFalse(EncounterPayload.Create(BiomeType.Forest, new[] { _wolf }, 1).isBossChallenge);
            Assert.IsFalse(EncounterPayload.FromJson("{\"biome\":0,\"enemyIds\":[\"enemy_forest_wolf\"],\"seed\":3}").isBossChallenge);
        }

        // ------------------------------------------------------------------
        // Unlock on boss kill.
        // ------------------------------------------------------------------

        [Test]
        public void ApplyUnlocks_WithBossGate_NeedsVictoriesAndTheBossKill()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(graph, progress);

            Win(progress, BiomeType.Forest, 3);
            Assert.IsEmpty(graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied), "Victories alone opened the next region.");
            Assert.IsFalse(progress.IsUnlocked(BiomeType.Desert));

            progress.MarkRegionCleared(BiomeType.Forest);
            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied));
        }

        [Test]
        public void ApplyUnlocks_ClearedButBelowThreshold_StaysLocked()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(graph, progress);

            progress.MarkRegionCleared(BiomeType.Forest);
            Win(progress, BiomeType.Forest, 2);
            Assert.IsEmpty(graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied));
        }

        [Test]
        public void ApplyUnlocks_RegionWithoutBoss_IsGatedByVictoriesOnly()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            progress.Unlock(BiomeType.Desert);
            progress.Unlock(BiomeType.Mountain);
            RegionBossProgress bosses = MakeBosses(graph, progress);

            Win(progress, BiomeType.Mountain, 3);
            CollectionAssert.AreEqual(new[] { BiomeType.Caves }, graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied));
        }

        [Test]
        public void CompleteEncounter_ChallengeVictoryClearsRegion_OthersDoNot()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            RegionBossProgress bosses = MakeBosses(graph, progress);
            var service = new SceneTransitionService(progress, new FakeSceneLoader());
            Win(progress, BiomeType.Forest, 3);

            // A normal victory or a fled challenge never clears the region.
            Assert.IsTrue(service.BeginEncounter(EncounterPayload.Create(BiomeType.Forest, new[] { _wolf }, 1)));
            service.CompleteEncounter(EncounterOutcome.Victory);
            Assert.IsTrue(service.BeginEncounter(bosses.BuildChallengePayload(BiomeType.Forest, new System.Random(2))));
            service.CompleteEncounter(EncounterOutcome.Fled);
            Assert.IsFalse(progress.IsRegionCleared(BiomeType.Forest));
            Assert.IsTrue(bosses.CanChallengeBoss(BiomeType.Forest));

            Assert.IsTrue(service.BeginEncounter(bosses.BuildChallengePayload(BiomeType.Forest, new System.Random(3))));
            service.CompleteEncounter(EncounterOutcome.Victory);

            Assert.IsTrue(progress.IsRegionCleared(BiomeType.Forest));
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Forest));
            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied));
        }

        [Test]
        public void ClearedRegions_RoundTripThroughSaveState()
        {
            var progress = new WorldMapProgress();
            Win(progress, BiomeType.Forest, 3);
            progress.MarkRegionCleared(BiomeType.Forest);

            var json = JsonUtility.ToJson(progress.CaptureState());
            var restored = new WorldMapProgress();
            restored.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(json));

            Assert.IsTrue(restored.IsRegionCleared(BiomeType.Forest));
            Assert.IsFalse(restored.IsRegionCleared(BiomeType.Desert));

            restored.ResetToNewGame();
            Assert.IsFalse(restored.IsRegionCleared(BiomeType.Forest), "New game kept a cleared region.");
        }

        // ------------------------------------------------------------------
        // Legacy saves.
        // ------------------------------------------------------------------

        [Test]
        public void LegacySave_AlreadyUnlockedNextRegion_StaysUnlocked()
        {
            // Pre-boss-challenge save: Desert opened by victories alone, no clearedRegions field, wolf alive.
            string legacyJson = "{\"currentRegion\":1,\"unlockedRegions\":[0,1],\"victoryRegions\":[0],\"victoryCounts\":[3]}";
            var progress = new WorldMapProgress();
            progress.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(legacyJson));
            var graph = WorldMapGraph.CreateDefault();
            RegionBossProgress bosses = MakeBosses(graph, progress, new DefeatedEnemyRegistry());

            graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied);

            Assert.IsTrue(progress.IsUnlocked(BiomeType.Desert), "Legacy unlock was lost.");
            Assert.AreEqual(BiomeType.Desert, progress.CurrentRegion);
            Assert.IsFalse(bosses.IsRegionCleared(BiomeType.Forest));
            Assert.IsTrue(bosses.CanChallengeBoss(BiomeType.Forest), "The (optional) Forest boss fight should still be offered.");
        }

        [Test]
        public void LegacySave_BossAlreadyDefeated_CountsAsCleared()
        {
            var progress = new WorldMapProgress();
            Win(progress, BiomeType.Forest, 3);
            var registry = new DefeatedEnemyRegistry();
            registry.MarkDefeated(DefeatedEnemyRegistry.GetId(_alphaWolf));
            var graph = WorldMapGraph.CreateDefault();
            RegionBossProgress bosses = MakeBosses(graph, progress, registry);

            Assert.IsTrue(bosses.IsBossDefeated(BiomeType.Forest));
            Assert.IsTrue(bosses.IsRegionCleared(BiomeType.Forest));
            Assert.IsFalse(bosses.CanChallengeBoss(BiomeType.Forest));
            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress, bosses.IsUnlockGateSatisfied));

            CollectionAssert.AreEqual(new[] { BiomeType.Forest }, bosses.SyncClearedFromDefeatedBosses());
            Assert.IsTrue(progress.IsRegionCleared(BiomeType.Forest), "Sync did not persist the legacy kill.");
            Assert.IsEmpty(bosses.SyncClearedFromDefeatedBosses(), "Sync is not idempotent.");
        }

        [Test]
        public void ApplyUnlocks_WithoutGate_KeepsTheOldVictoryOnlyBehaviour()
        {
            var graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress();
            Win(progress, BiomeType.Forest, 3);

            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress));
        }
    }
}
