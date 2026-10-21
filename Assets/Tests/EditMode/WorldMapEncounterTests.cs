using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Tests for the world map layer: <see cref="RandomEncounterSystem"/> (chance/grace/seeding and
    /// its use of <see cref="BiomeDefinition.RollEnemy"/>), <see cref="WorldMapGraph"/> +
    /// <see cref="WorldMapProgress"/> (unlock rules, travel, persistence through SaveManager),
    /// <see cref="EncounterPayload"/> JSON round-trip and <see cref="SceneTransitionService"/>
    /// with a fake scene loader.
    /// </summary>
    public class WorldMapEncounterTests
    {
        private const int TestSlot = 90011;

        private readonly List<Object> _created = new List<Object>();

        private class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Loaded = new List<string>();

            public void LoadScene(string sceneName) => Loaded.Add(sceneName);
        }

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
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

            string slotPath = Path.Combine(Application.persistentDataPath, "Saves", $"slot_{TestSlot}.json");
            foreach (string path in new[] { slotPath, slotPath + ".bak", slotPath + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        // ------------------------------------------------------------------
        // Helpers.
        // ------------------------------------------------------------------

        private EnemyDefinition MakeEnemy(string id)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            _created.Add(enemy);
            return enemy;
        }

        private BiomeDefinition MakeBiome(BiomeType type, params (EnemyDefinition enemy, int weight)[] enemies)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            biome.SetDataForTests(
                type,
                $"biome.{type.ToString().ToLowerInvariant()}.name",
                string.Empty,
                0f,
                20f,
                string.Empty,
                enemies.Select(e => new BiomeEnemySpawnEntry(e.enemy, e.weight)).ToList(),
                null);
            _created.Add(biome);
            return biome;
        }

        private static RandomEncounterSettings Settings(
            int grace,
            float baseChance,
            float ramp = 0f,
            float max = 1f,
            EncounterStepMode mode = EncounterStepMode.Distance,
            float stepDistance = 1f,
            float secondsPerStep = 1f,
            int minEnemies = 1,
            int maxEnemies = 1)
        {
            return new RandomEncounterSettings(mode, stepDistance, secondsPerStep, grace, baseChance, ramp, max, minEnemies, maxEnemies);
        }

        private BiomeDefinition SingleEnemyForest(out EnemyDefinition enemy)
        {
            enemy = MakeEnemy("enemy_test_wolf");
            return MakeBiome(BiomeType.Forest, (enemy, 1));
        }

        // ------------------------------------------------------------------
        // RandomEncounterSystem — chance and grace period.
        // ------------------------------------------------------------------

        [Test]
        public void RollStep_NeverTriggersDuringGracePeriod_ThenTriggersAtCertainChance()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 5, baseChance: 1f), seed: 1);

            for (int step = 1; step <= 5; step++)
            {
                Assert.IsFalse(system.RollStep(biome, out _), $"Step {step} is inside the grace period.");
            }

            Assert.IsTrue(system.RollStep(biome, out EncounterPayload payload));
            Assert.IsNotNull(payload);
        }

        [Test]
        public void RollStep_AfterEncounter_GracePeriodRestarts()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 3, baseChance: 1f), seed: 1);

            for (int i = 0; i < 3; i++)
            {
                system.RollStep(biome, out _);
            }

            Assert.IsTrue(system.RollStep(biome, out _));
            Assert.AreEqual(0, system.StepsSinceLastEncounter);
            Assert.IsTrue(system.IsInGracePeriod);

            for (int i = 0; i < 3; i++)
            {
                Assert.IsFalse(system.RollStep(biome, out _));
            }

            Assert.IsTrue(system.RollStep(biome, out _));
        }

        [Test]
        public void GetChanceForStep_ZeroInGrace_RampsAndCapsAfterwards()
        {
            var system = new RandomEncounterSystem(Settings(grace: 2, baseChance: 0.1f, ramp: 0.05f, max: 0.2f), seed: 1);

            Assert.AreEqual(0f, system.GetChanceForStep(1));
            Assert.AreEqual(0f, system.GetChanceForStep(2));
            Assert.AreEqual(0.1f, system.GetChanceForStep(3), 1e-5f);
            Assert.AreEqual(0.15f, system.GetChanceForStep(4), 1e-5f);
            Assert.AreEqual(0.2f, system.GetChanceForStep(5), 1e-5f);
            Assert.AreEqual(0.2f, system.GetChanceForStep(50), 1e-5f);
        }

        [Test]
        public void RollStep_ZeroChance_NeverTriggers()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 0f), seed: 7);

            for (int i = 0; i < 500; i++)
            {
                Assert.IsFalse(system.RollStep(biome, out _));
            }
        }

        [Test]
        public void RollStep_Disabled_NeverTriggers()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 1f), seed: 7) { Enabled = false };

            Assert.IsFalse(system.RollStep(biome, out _));
            Assert.AreEqual(0, system.StepsSinceLastEncounter);
        }

        [Test]
        public void SameSeed_ProducesSameEncounterSequence()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_test_wolf");
            EnemyDefinition spider = MakeEnemy("enemy_test_spider");
            BiomeDefinition biome = MakeBiome(BiomeType.Forest, (wolf, 3), (spider, 1));

            List<string> Run(int seed)
            {
                var system = new RandomEncounterSystem(Settings(grace: 4, baseChance: 0.1f, ramp: 0.02f, max: 0.5f, maxEnemies: 3), seed);
                var log = new List<string>();
                for (int step = 0; step < 400; step++)
                {
                    if (system.RollStep(biome, out EncounterPayload payload))
                    {
                        log.Add($"{step}:{string.Join(",", payload.enemyIds)}:{payload.seed}");
                    }
                }

                return log;
            }

            List<string> first = Run(2024);
            List<string> second = Run(2024);

            Assert.IsNotEmpty(first);
            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void Advance_DistanceMode_ConvertsDistanceIntoSteps()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 2, baseChance: 1f, stepDistance: 2f), seed: 1);

            Assert.IsFalse(system.Advance(5.9f, 0f, biome, out _)); // 2 full steps, both in grace.
            Assert.AreEqual(2, system.StepsSinceLastEncounter);
            Assert.IsTrue(system.Advance(0.2f, 0f, biome, out EncounterPayload payload)); // 3rd step.
            Assert.IsNotNull(payload);
        }

        [Test]
        public void Advance_TimeMode_IgnoresDistanceAndCountsSeconds()
        {
            BiomeDefinition biome = SingleEnemyForest(out _);
            var system = new RandomEncounterSystem(Settings(grace: 1, baseChance: 1f, mode: EncounterStepMode.Time, secondsPerStep: 0.5f), seed: 1);

            Assert.IsFalse(system.Advance(100f, 0f, biome, out _));
            Assert.AreEqual(0, system.StepsSinceLastEncounter);
            Assert.IsFalse(system.Advance(0f, 0.5f, biome, out _));
            Assert.IsTrue(system.Advance(0f, 0.5f, biome, out _));
        }

        [Test]
        public void Advance_NullBiome_NeverTriggers()
        {
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 1f), seed: 1);

            Assert.IsFalse(system.Advance(10f, 10f, null, out EncounterPayload payload));
            Assert.IsNull(payload);
        }

        // ------------------------------------------------------------------
        // RandomEncounterSystem — BiomeDefinition.RollEnemy integration.
        // ------------------------------------------------------------------

        [Test]
        public void Encounter_InBiomeWithEmptySpawnTable_NeverTriggers()
        {
            BiomeDefinition empty = MakeBiome(BiomeType.Caves);
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 1f), seed: 3);

            for (int i = 0; i < 50; i++)
            {
                Assert.IsFalse(system.RollStep(empty, out _));
            }
        }

        [Test]
        public void Encounter_PayloadCarriesBiomeAndRolledEnemies()
        {
            EnemyDefinition scorpion = MakeEnemy("enemy_test_scorpion");
            BiomeDefinition desert = MakeBiome(BiomeType.Desert, (scorpion, 1));
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 1f, minEnemies: 2, maxEnemies: 3), seed: 11);

            Assert.IsTrue(system.RollStep(desert, out EncounterPayload payload));
            Assert.AreEqual(BiomeType.Desert, payload.biome);
            Assert.That(payload.EnemyCount, Is.InRange(2, 3));
            Assert.That(payload.enemyIds, Is.All.EqualTo("enemy_test_scorpion"));
            Assert.That(payload.ResolveEnemies(), Is.All.SameAs(scorpion));
        }

        [Test]
        public void Encounter_EnemySelectionFollowsBiomeWeights()
        {
            EnemyDefinition common = MakeEnemy("enemy_test_common");
            EnemyDefinition rare = MakeEnemy("enemy_test_rare");
            BiomeDefinition biome = MakeBiome(BiomeType.Mountain, (common, 9), (rare, 1));
            var system = new RandomEncounterSystem(Settings(grace: 0, baseChance: 1f), seed: 42);

            int commonCount = 0;
            const int samples = 2000;
            for (int i = 0; i < samples; i++)
            {
                Assert.IsTrue(system.RollStep(biome, out EncounterPayload payload));
                if (payload.ResolveEnemies()[0] == common)
                {
                    commonCount++;
                }
            }

            double ratio = commonCount / (double)samples;
            Assert.Greater(ratio, 0.8);
            Assert.Less(ratio, 0.98);
        }

        // ------------------------------------------------------------------
        // EncounterPayload round-trip.
        // ------------------------------------------------------------------

        [Test]
        public void Payload_JsonRoundTrip_PreservesFieldsAndResolvesById()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_test_wolf");
            EnemyDefinition alpha = MakeEnemy("enemy_test_alpha");
            EncounterPayload original = EncounterPayload.Create(BiomeType.Mountain, new[] { wolf, null, alpha }, seed: 987);

            EncounterPayload copy = EncounterPayload.FromJson(original.ToJson());

            Assert.IsNotNull(copy);
            Assert.AreEqual(BiomeType.Mountain, copy.biome);
            Assert.AreEqual(987, copy.seed);
            CollectionAssert.AreEqual(new[] { "enemy_test_wolf", "enemy_test_alpha" }, copy.enemyIds);

            var lookup = new Dictionary<string, EnemyDefinition> { { wolf.name, wolf }, { alpha.name, alpha } };
            List<EnemyDefinition> resolved = copy.ResolveEnemies(id => lookup.TryGetValue(id, out EnemyDefinition e) ? e : null);
            CollectionAssert.AreEqual(new[] { wolf, alpha }, resolved);
        }

        [Test]
        public void Payload_ResolveEnemies_SkipsUnknownIds()
        {
            EncounterPayload payload = EncounterPayload.FromJson("{\"biome\":0,\"enemyIds\":[\"missing\"],\"seed\":1}");

            Assert.IsNotNull(payload);
            Assert.IsEmpty(payload.ResolveEnemies(_ => null));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not json")]
        public void Payload_FromJson_InvalidInput_ReturnsNull(string json)
        {
            Assert.IsNull(EncounterPayload.FromJson(json));
        }

        [Test]
        public void Payload_ResolveEnemies_DefaultLookupFindsSeededResourcesEnemy()
        {
            // Uses the real seeded asset under Resources/Enemies (EnemyDatabaseSeeder content).
            EncounterPayload payload = EncounterPayload.FromJson("{\"biome\":0,\"enemyIds\":[\"enemy_forest_wolf\"],\"seed\":1}");

            List<EnemyDefinition> resolved = payload.ResolveEnemies();

            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual("enemy_forest_wolf", resolved[0].name);
        }

        // ------------------------------------------------------------------
        // WorldMapGraph + WorldMapProgress — unlock rules and travel.
        // ------------------------------------------------------------------

        [Test]
        public void NewProgress_OnlyStartingRegionUnlocked_AndCurrent()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);

            Assert.AreEqual(BiomeType.Forest, progress.CurrentRegion);
            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest }, progress.UnlockedRegions);
        }

        [Test]
        public void SetCurrentRegion_LockedRegion_IsRejected()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);

            Assert.IsFalse(progress.SetCurrentRegion(BiomeType.Caves));
            Assert.AreEqual(BiomeType.Forest, progress.CurrentRegion);
        }

        [Test]
        public void ApplyUnlocks_OpensNextRegionOnlyAfterRequiredVictories()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress(BiomeType.Forest);

            for (int i = 0; i < WorldMapGraph.DefaultVictoriesToUnlock - 1; i++)
            {
                progress.RecordVictory(BiomeType.Forest);
            }

            Assert.IsEmpty(graph.ApplyUnlocks(progress));
            Assert.IsFalse(progress.IsUnlocked(BiomeType.Desert));

            progress.RecordVictory(BiomeType.Forest);

            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress));
            Assert.IsTrue(progress.IsUnlocked(BiomeType.Desert));
            Assert.IsFalse(progress.IsUnlocked(BiomeType.Mountain));
        }

        [Test]
        public void Unlock_PublishesEventOnlyTheFirstTime()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);
            var unlocked = new List<BiomeType>();
            System.Action<WorldMapRegionUnlockedEvent> handler = e => unlocked.Add(e.Region);
            EventBus.Subscribe(handler);

            try
            {
                Assert.IsTrue(progress.Unlock(BiomeType.Desert));
                Assert.IsFalse(progress.Unlock(BiomeType.Desert));
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, unlocked);
        }

        [Test]
        public void CanTravel_RequiresUnlockedAndConnectedDestination()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            var progress = new WorldMapProgress(BiomeType.Forest);

            Assert.IsFalse(graph.CanTravel(progress, BiomeType.Desert), "Desert is still locked.");

            progress.Unlock(BiomeType.Desert);
            progress.Unlock(BiomeType.Caves);

            Assert.IsTrue(graph.CanTravel(progress, BiomeType.Desert));
            Assert.IsFalse(graph.CanTravel(progress, BiomeType.Caves), "Caves is not connected to Forest.");
            Assert.IsFalse(graph.CanTravel(progress, BiomeType.Forest), "Already there.");
        }

        [Test]
        public void BuildGraph_FromSceneNodes_MakesBidirectionalConnectionsAndRules()
        {
            WorldMapRegionNode forest = new GameObject("Forest").AddComponent<WorldMapRegionNode>();
            WorldMapRegionNode desert = new GameObject("Desert").AddComponent<WorldMapRegionNode>();
            _created.Add(forest.gameObject);
            _created.Add(desert.gameObject);

            forest.SetDataForTests(BiomeType.Forest, new[] { desert }, hasUnlockRule: false, BiomeType.Forest, 0);
            desert.SetDataForTests(BiomeType.Desert, null, hasUnlockRule: true, BiomeType.Forest, 1);

            var byRegion = new Dictionary<BiomeType, WorldMapRegionNode>();
            WorldMapGraph graph = WorldMapController.BuildGraph(new[] { forest, desert }, byRegion);

            Assert.IsTrue(graph.AreConnected(BiomeType.Forest, BiomeType.Desert));
            Assert.IsTrue(graph.AreConnected(BiomeType.Desert, BiomeType.Forest));
            Assert.AreSame(desert, byRegion[BiomeType.Desert]);

            var progress = new WorldMapProgress(BiomeType.Forest);
            progress.RecordVictory(BiomeType.Forest);
            CollectionAssert.AreEqual(new[] { BiomeType.Desert }, graph.ApplyUnlocks(progress));
        }

        [Test]
        public void BuildGraph_NoNodes_FallsBackToDefault()
        {
            WorldMapGraph graph = WorldMapController.BuildGraph(new WorldMapRegionNode[0]);

            Assert.IsTrue(graph.AreConnected(BiomeType.Forest, BiomeType.Desert));
            Assert.IsTrue(graph.AreConnected(BiomeType.Mountain, BiomeType.Caves));
        }

        // ------------------------------------------------------------------
        // WorldMapProgress — persistence.
        // ------------------------------------------------------------------

        [Test]
        public void Progress_CaptureRestore_RoundTripsThroughJson()
        {
            var original = new WorldMapProgress(BiomeType.Forest);
            original.Unlock(BiomeType.Desert);
            original.SetCurrentRegion(BiomeType.Desert);
            original.RecordVictory(BiomeType.Forest);
            original.RecordVictory(BiomeType.Forest);
            original.RecordVictory(BiomeType.Desert);

            string json = JsonUtility.ToJson(original.CaptureState());
            var restored = new WorldMapProgress(BiomeType.Forest);
            restored.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(json));

            Assert.AreEqual(BiomeType.Desert, restored.CurrentRegion);
            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest, BiomeType.Desert }, restored.UnlockedRegions);
            Assert.AreEqual(2, restored.GetVictories(BiomeType.Forest));
            Assert.AreEqual(1, restored.GetVictories(BiomeType.Desert));
            Assert.AreEqual(0, restored.GetVictories(BiomeType.Caves));
        }

        [Test]
        public void Progress_RestoreState_CurrentRegionNotUnlocked_FallsBackToStart()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);
            var state = new WorldMapSaveState { currentRegion = (int)BiomeType.Caves };

            progress.RestoreState(state);

            Assert.AreEqual(BiomeType.Forest, progress.CurrentRegion);
            Assert.IsTrue(progress.IsUnlocked(BiomeType.Forest), "Starting region is always unlocked.");
        }

        [Test]
        public void Progress_SaveManagerSlot_RoundTrip()
        {
            var saveManager = new SaveManager();
            saveManager.Initialize();
            var progress = new WorldMapProgress(BiomeType.Forest);
            saveManager.Register(progress);

            progress.Unlock(BiomeType.Desert);
            progress.Unlock(BiomeType.Mountain);
            progress.SetCurrentRegion(BiomeType.Mountain);
            progress.RecordVictory(BiomeType.Desert);
            saveManager.Save(TestSlot);

            progress.ResetToNewGame();
            Assert.AreEqual(BiomeType.Forest, progress.CurrentRegion);

            Assert.IsTrue(saveManager.Load(TestSlot));
            Assert.AreEqual(BiomeType.Mountain, progress.CurrentRegion);
            Assert.IsTrue(progress.IsUnlocked(BiomeType.Desert));
            Assert.AreEqual(1, progress.GetVictories(BiomeType.Desert));
        }

        // ------------------------------------------------------------------
        // SceneTransitionService.
        // ------------------------------------------------------------------

        private static EncounterPayload MakePayload(EnemyDefinition enemy, BiomeType biome = BiomeType.Forest)
        {
            return EncounterPayload.Create(biome, new[] { enemy }, seed: 5);
        }

        [Test]
        public void BeginEncounter_StoresPayloadPublishesAndLoadsCombatScene()
        {
            var loader = new FakeSceneLoader();
            var service = new SceneTransitionService(new WorldMapProgress(), loader);
            EncounterPayload payload = MakePayload(MakeEnemy("enemy_test_wolf"));

            EncounterPayload published = null;
            System.Action<EncounterStartedEvent> handler = e => published = e.Payload;
            EventBus.Subscribe(handler);

            try
            {
                Assert.IsTrue(service.BeginEncounter(payload));
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreSame(payload, service.PendingEncounter);
            Assert.AreSame(payload, published);
            CollectionAssert.AreEqual(new[] { SceneTransitionService.DefaultCombatSceneName }, loader.Loaded);
        }

        [Test]
        public void BeginEncounter_WhileAnotherPending_OrEmpty_IsRejected()
        {
            var loader = new FakeSceneLoader();
            var service = new SceneTransitionService(new WorldMapProgress(), loader);
            EnemyDefinition enemy = MakeEnemy("enemy_test_wolf");

            Assert.IsFalse(service.BeginEncounter(null));
            Assert.IsFalse(service.BeginEncounter(EncounterPayload.Create(BiomeType.Forest, new EnemyDefinition[0], 1)));
            Assert.IsTrue(service.BeginEncounter(MakePayload(enemy)));
            Assert.IsFalse(service.BeginEncounter(MakePayload(enemy)));
            Assert.AreEqual(1, loader.Loaded.Count);
        }

        [Test]
        public void CompleteEncounter_Victory_RecordsVictoryAndReturnsToMap()
        {
            var loader = new FakeSceneLoader();
            var progress = new WorldMapProgress();
            var service = new SceneTransitionService(progress, loader);
            service.BeginEncounter(MakePayload(MakeEnemy("enemy_test_wolf"), BiomeType.Forest));

            EncounterOutcome? ended = null;
            System.Action<EncounterEndedEvent> handler = e => ended = e.Outcome;
            EventBus.Subscribe(handler);

            try
            {
                Assert.IsTrue(service.CompleteEncounter(EncounterOutcome.Victory));
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(EncounterOutcome.Victory, ended);
            Assert.AreEqual(EncounterOutcome.Victory, service.LastOutcome);
            Assert.IsFalse(service.HasPendingEncounter);
            Assert.AreEqual(1, progress.GetVictories(BiomeType.Forest));
            Assert.AreEqual(SceneTransitionService.DefaultWorldMapSceneName, loader.Loaded.Last());
        }

        [TestCase(EncounterOutcome.Fled)]
        [TestCase(EncounterOutcome.Defeat)]
        public void CompleteEncounter_NonVictory_DoesNotCountTowardsUnlocks(EncounterOutcome outcome)
        {
            var loader = new FakeSceneLoader();
            var progress = new WorldMapProgress();
            var service = new SceneTransitionService(progress, loader);
            service.BeginEncounter(MakePayload(MakeEnemy("enemy_test_wolf")));

            Assert.IsTrue(service.CompleteEncounter(outcome));

            Assert.AreEqual(0, progress.GetVictories(BiomeType.Forest));
            Assert.AreEqual(SceneTransitionService.DefaultWorldMapSceneName, loader.Loaded.Last());
        }

        [Test]
        public void CompleteEncounter_WithoutPending_ReturnsFalseAndLoadsNothing()
        {
            var loader = new FakeSceneLoader();
            var service = new SceneTransitionService(new WorldMapProgress(), loader);

            Assert.IsFalse(service.CompleteEncounter(EncounterOutcome.Victory));
            Assert.IsEmpty(loader.Loaded);
        }
    }
}
