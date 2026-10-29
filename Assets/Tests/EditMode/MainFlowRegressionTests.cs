using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regressions found reading the main flow (title → map → encounter → back, save/load in the
    /// middle of each). One test per demonstrated defect.
    /// </summary>
    public class MainFlowRegressionTests
    {
        private readonly List<Object> _objects = new List<Object>();

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Loaded = new List<string>();

            public void LoadScene(string sceneName) => Loaded.Add(sceneName);
        }

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            GameplayPause.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _objects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _objects.Clear();
            ServiceLocator.Clear();
            GameplayPause.Reset();
            EventBus.Clear();
        }

        private static void Invoke(object target, string method)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(info, $"Expected private method '{method}' on {target.GetType().Name}.");
            info.Invoke(target, null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private WorldMapRegionNode Node(BiomeType region, Vector3 position)
        {
            var go = new GameObject("Node_" + region);
            _objects.Add(go);
            go.transform.position = position;
            return go.AddComponent<WorldMapRegionNode>();
        }

        // ------------------------------------------------------------------
        // Pause menu "Carregar" on the world map: WorldMapController used to keep the previous
        // session's current node / token, so Explore rolled (and re-saved) the wrong region.
        // ------------------------------------------------------------------

        [Test]
        public void WorldMap_LoadWhileOnMap_ResyncsCurrentNodeAndToken()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);
            ServiceLocator.Register(progress);

            WorldMapRegionNode forest = Node(BiomeType.Forest, Vector3.zero);
            WorldMapRegionNode desert = Node(BiomeType.Desert, new Vector3(10f, 0f, 0f));
            forest.SetDataForTests(BiomeType.Forest, new[] { desert }, false, BiomeType.Forest, 0);
            desert.SetDataForTests(BiomeType.Desert, new[] { forest }, true, BiomeType.Forest, 3);

            var token = new GameObject("Token");
            _objects.Add(token);
            var host = new GameObject("WorldMap");
            _objects.Add(host);
            host.SetActive(false);
            WorldMapController map = host.AddComponent<WorldMapController>();
            SetPrivateField(map, "_nodes", new[] { forest, desert });
            SetPrivateField(map, "_playerToken", token.transform);
            SetPrivateField(map, "_encountersEnabled", false);

            Invoke(map, "Awake");
            Invoke(map, "OnEnable");
            Invoke(map, "Start");
            Assert.AreEqual(BiomeType.Forest, map.CurrentNode.Region);

            // SaveManager.Load restores WorldMapProgress in place (session reset + RestoreState) and
            // then publishes GameLoadedEvent — the slot was saved standing on the Desert.
            progress.RestoreState(new WorldMapSaveState
            {
                currentRegion = (int)BiomeType.Desert,
                unlockedRegions = new List<int> { (int)BiomeType.Forest, (int)BiomeType.Desert },
            });
            EventBus.Publish(new GameLoadedEvent());

            try
            {
                Assert.AreEqual(BiomeType.Desert, map.CurrentNode.Region, "the controller must follow the loaded region");
                Assert.AreEqual(desert.Position.x, token.transform.position.x, 0.001f, "the token must stand on the loaded region");
                Assert.IsFalse(map.IsTraveling);
            }
            finally
            {
                Invoke(map, "OnDisable");
            }
        }

        [Test]
        public void WorldMap_GameLoadedBeforeStart_IsIgnored()
        {
            var host = new GameObject("WorldMap");
            _objects.Add(host);
            host.SetActive(false);
            WorldMapController map = host.AddComponent<WorldMapController>();

            Invoke(map, "Awake");
            Invoke(map, "OnEnable");
            try
            {
                Assert.DoesNotThrow(() => EventBus.Publish(new GameLoadedEvent()));
                Assert.IsNull(map.CurrentNode);
            }
            finally
            {
                Invoke(map, "OnDisable");
            }
        }
    
        // ------------------------------------------------------------------
        // Victory decided by the arena, then the player dies during its return delay: the death
        // screen (which pauses, freezing that delay) completed the encounter as a Defeat.
        // ------------------------------------------------------------------

        private EncounterPayload BossPayload()
        {
            var boss = ScriptableObject.CreateInstance<EnemyDefinition>();
            boss.name = "enemy_test_flow_boss";
            _objects.Add(boss);
            return EncounterPayload.Create(BiomeType.Forest, new[] { boss }, seed: 3, isBossChallenge: true);
        }

        [Test]
        public void Encounter_DecidedVictory_SurvivesDeathScreenDefeat()
        {
            var progress = new WorldMapProgress(BiomeType.Forest);
            var loader = new FakeSceneLoader();
            var transitions = new SceneTransitionService(progress, loader);
            EncounterOutcome? ended = null;
            System.Action<EncounterEndedEvent> onEnded = evt => ended = evt.Outcome;
            EventBus.Subscribe(onEnded);

            Assert.IsTrue(transitions.BeginEncounter(BossPayload()));
            transitions.DecideOutcome(EncounterOutcome.Victory);  // last enemy (the boss) died
            transitions.DecideOutcome(EncounterOutcome.Defeat);   // first decision wins

            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Defeat)); // death screen "Continuar"

            Assert.AreEqual(EncounterOutcome.Victory, transitions.LastOutcome);
            Assert.AreEqual(EncounterOutcome.Victory, ended);
            Assert.AreEqual(1, progress.GetVictories(BiomeType.Forest));
            Assert.IsTrue(progress.IsRegionCleared(BiomeType.Forest), "the boss challenge win must clear the region");
            Assert.IsNull(transitions.DecidedOutcome);

            // The arena's own delayed completion afterwards is a no-op.
            Assert.IsFalse(transitions.CompleteEncounter(EncounterOutcome.Victory));
        }

        [Test]
        public void Encounter_DecidedOutcome_IsClearedByNextEncounterAndAbandon()
        {
            var transitions = new SceneTransitionService(new WorldMapProgress(), new FakeSceneLoader());

            transitions.DecideOutcome(EncounterOutcome.Victory); // nothing pending: ignored
            Assert.IsNull(transitions.DecidedOutcome);

            Assert.IsTrue(transitions.BeginEncounter(BossPayload()));
            transitions.DecideOutcome(EncounterOutcome.Victory);
            transitions.AbandonPendingEncounter();
            Assert.IsNull(transitions.DecidedOutcome);

            Assert.IsTrue(transitions.BeginEncounter(BossPayload()));
            Assert.IsTrue(transitions.CompleteEncounter(EncounterOutcome.Defeat));
            Assert.AreEqual(EncounterOutcome.Defeat, transitions.LastOutcome, "an undecided encounter keeps the requested outcome");
        }
    }
}
