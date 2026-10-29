using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Tutorial;
using Rapadura.UI.WorldMap;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 2026-10-29 onboarding hints appended to <see cref="TutorialSteps"/>: "Conversar com…" and
    /// "Vila" on the world map (<see cref="TutorialManager.NotifyWorldMapIdle"/>), the first biome
    /// material drop and the first stun the hero lands. Each fires once from its trigger, respects
    /// "Mostrar dicas"/"Reiniciar tutorial", a pre-existing tutorial.json (9 old ids) still loads, and
    /// hints defer behind story panels, the Vila panel and region dialogue.
    /// </summary>
    public class TutorialOnboardingHintsTests
    {
        private static readonly string[] OriginalStepIds =
        {
            TutorialSteps.WorldMap, TutorialSteps.Encounter, TutorialSteps.Skills, TutorialSteps.Loot, TutorialSteps.Quest,
            TutorialSteps.LevelUp, TutorialSteps.Death, TutorialSteps.Night, TutorialSteps.Crafting,
        };

        private static readonly string[] NewStepIds =
        {
            TutorialSteps.RegionNpc, TutorialSteps.Village, TutorialSteps.Materials, TutorialSteps.Stun,
        };

        private readonly List<TutorialManager> _managers = new List<TutorialManager>();
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            GameplayPause.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (TutorialManager manager in _managers)
            {
                manager.Shutdown();
            }

            _managers.Clear();
            EventBus.Clear();
            GameplayPause.Reset();
            Time.timeScale = 1f;

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            TestSaveRoot.Exit();
        }

        private TutorialManager CreateManager(GameFlowState state)
        {
            var manager = new TutorialManager { CraftableRecipeProbe = null };
            _managers.Add(manager);
            manager.Initialize();
            SetFlow(state);
            return manager;
        }

        private static void SetFlow(GameFlowState state) =>
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Boot, state));

        private ItemDefinition CreateItem(string id, ItemType type)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            return item;
        }

        private void PublishDrop(ItemType type, int quantity = 2)
        {
            var drops = new List<LootDrop> { new LootDrop(CreateItem("test_drop_" + type, type), quantity) };
            EventBus.Publish(new EnemyLootDroppedEvent(null, null, drops));
        }

        private GameObject CreateEnemy()
        {
            var enemy = new GameObject("TestEnemy");
            _created.Add(enemy);
            enemy.AddComponent<EnemyController>();
            return enemy;
        }

        private GameObject CreatePlain(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        // ------------------------------------------------------------------
        // Step table / save compatibility
        // ------------------------------------------------------------------

        [Test]
        public void NewSteps_AreAppended_OriginalIdsAndOrdersUnchanged()
        {
            List<TutorialStepDefinition> all = TutorialSteps.All.ToList();
            Assert.AreEqual(OriginalStepIds.Length + NewStepIds.Length, all.Count);

            for (int i = 0; i < OriginalStepIds.Length; i++)
            {
                Assert.AreEqual(OriginalStepIds[i], all[i].Id, "existing steps keep their position");
                Assert.AreEqual(i, all[i].Order, $"{all[i].Id} keeps its order");
            }

            CollectionAssert.AreEqual(NewStepIds, all.Skip(OriginalStepIds.Length).Select(s => s.Id).ToArray());
            CollectionAssert.AllItemsAreUnique(all.Select(s => s.Id).ToArray());
            CollectionAssert.AllItemsAreUnique(all.Select(s => s.Order).ToArray());
            Assert.IsTrue(all.Skip(OriginalStepIds.Length).All(s => s.Order >= OriginalStepIds.Length));
            Assert.IsTrue(all.All(s => !s.Interrupts || s.Id == TutorialSteps.Death), "only the death tip interrupts");
            Assert.IsTrue(all.All(s => !s.PausesGameplay));
        }

        [Test]
        public void NewSteps_ContextsAndOutlines()
        {
            Assert.AreEqual(TutorialContext.World, TutorialSteps.All.First(s => s.Id == TutorialSteps.RegionNpc).Context);
            Assert.AreEqual(TutorialContext.World, TutorialSteps.All.First(s => s.Id == TutorialSteps.Village).Context);
            Assert.AreEqual(TutorialContext.Any, TutorialSteps.All.First(s => s.Id == TutorialSteps.Materials).Context);
            Assert.AreEqual(TutorialContext.Encounter, TutorialSteps.All.First(s => s.Id == TutorialSteps.Stun).Context);

            Assert.AreEqual(RegionNpcTalkButton.ButtonName, TutorialSteps.All.First(s => s.Id == TutorialSteps.RegionNpc).HighlightElement);
            Assert.AreEqual(VillageHubButton.ButtonName, TutorialSteps.All.First(s => s.Id == TutorialSteps.Village).HighlightElement);
            Assert.AreEqual(RegionNpcTalkButton.ButtonName, TutorialHighlightCache.WorldMapElementName(TutorialSteps.TalkButtonElement), "already a map name: no double prefix");
            Assert.AreEqual(VillageHubButton.ButtonName, TutorialHighlightCache.WorldMapElementName(TutorialSteps.VillageButtonElement));
            Assert.AreEqual("inventory-button", TutorialSteps.All.First(s => s.Id == TutorialSteps.Materials).HighlightElement);
            Assert.IsNull(TutorialSteps.All.First(s => s.Id == TutorialSteps.Stun).HighlightElement);
        }

        [Test]
        public void OldProfile_WithTheNineOriginalSteps_LoadsAndOnlyNewStepsCanFire()
        {
            // A tutorial.json written before these steps existed (sorted ids, as SaveProfile writes them).
            string[] sorted = OriginalStepIds.OrderBy(id => id, System.StringComparer.Ordinal).ToArray();
            string json = "{\n    \"profileVersion\": 1,\n    \"hintsEnabled\": true,\n    \"completedStepIds\": [\n" +
                          string.Join(",\n", sorted.Select(id => $"        \"{id}\"")) + "\n    ]\n}";
            Directory.CreateDirectory(Path.GetDirectoryName(TutorialManager.ProfileFilePath));
            File.WriteAllText(TutorialManager.ProfileFilePath, json);

            TutorialManager manager = CreateManager(GameFlowState.World);
            Assert.IsTrue(manager.HintsEnabled);
            Assert.AreEqual(OriginalStepIds.Length, manager.CompletedCount);
            foreach (string id in OriginalStepIds)
            {
                Assert.IsTrue(manager.IsCompleted(id), id);
            }

            Assert.IsNull(manager.CurrentHint, "entering the map does not repeat the old world-map tip");
            foreach (string id in NewStepIds)
            {
                Assert.IsFalse(manager.IsCompleted(id), id);
            }

            manager.NotifyWorldMapIdle(regionNpcAvailable: true, villageAvailable: false, prologueSeen: true);
            Assert.AreEqual(TutorialSteps.RegionNpc, manager.CurrentHint?.Id);
            Assert.IsTrue(manager.Acknowledge(TutorialSteps.RegionNpc));

            manager.Shutdown();
            _managers.Remove(manager);
            TutorialManager reloaded = CreateManager(GameFlowState.World);
            Assert.AreEqual(OriginalStepIds.Length + 1, reloaded.CompletedCount, "the new id is saved next to the old ones");
            Assert.IsTrue(reloaded.IsCompleted(TutorialSteps.RegionNpc));
        }

        [Test]
        public void OldProfile_WithHintsOff_StaysOff_ForTheNewSteps()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TutorialManager.ProfileFilePath));
            File.WriteAllText(TutorialManager.ProfileFilePath, "{\"profileVersion\":1,\"hintsEnabled\":false,\"completedStepIds\":[\"world_map\"]}");

            TutorialManager manager = CreateManager(GameFlowState.World);
            Assert.IsFalse(manager.HintsEnabled);
            Assert.IsFalse(manager.WantsWorldMapIdleReports, "no HUD probing while hints are off");

            manager.NotifyWorldMapIdle(true, true, true);
            PublishDrop(ItemType.Material);
            Assert.IsFalse(NewStepIds.Any(manager.IsPending), "\"Mostrar dicas\" off: nothing is queued");
        }

        // ------------------------------------------------------------------
        // World map: Conversar / Vila
        // ------------------------------------------------------------------

        [Test]
        public void RegionNpc_FiresOnce_FromWorldMapIdle()
        {
            TutorialManager manager = CreateManager(GameFlowState.World);
            manager.Acknowledge(TutorialSteps.WorldMap);
            Assert.IsTrue(manager.WantsWorldMapIdleReports);

            manager.NotifyWorldMapIdle(false, false, true);
            Assert.IsFalse(manager.IsPending(TutorialSteps.RegionNpc), "no talk button on screen, no tip");

            manager.NotifyWorldMapIdle(true, false, true);
            Assert.AreEqual(TutorialSteps.RegionNpc, manager.CurrentHint?.Id);
            Assert.AreEqual(TutorialSteps.TalkButtonElement, manager.CurrentHint.HighlightElement);

            manager.NotifyWorldMapIdle(true, false, true);
            Assert.AreEqual(1, manager.Steps.Count(s => manager.IsPending(s.Id)));

            Assert.IsTrue(manager.Acknowledge(TutorialSteps.RegionNpc));
            manager.NotifyWorldMapIdle(true, false, true);
            Assert.IsFalse(manager.IsPending(TutorialSteps.RegionNpc), "fired again after being acknowledged");
        }

        [Test]
        public void Village_WaitsForThePrologue_ThenFiresOnce()
        {
            TutorialManager manager = CreateManager(GameFlowState.World);
            manager.Acknowledge(TutorialSteps.WorldMap);

            manager.NotifyWorldMapIdle(false, true, prologueSeen: false);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Village), "the Vila tip must not compete with the prologue");

            manager.NotifyWorldMapIdle(false, true, prologueSeen: true);
            Assert.AreEqual(TutorialSteps.Village, manager.CurrentHint?.Id);
            Assert.AreEqual(TutorialSteps.VillageButtonElement, manager.CurrentHint.HighlightElement);

            Assert.IsTrue(manager.Acknowledge(TutorialSteps.Village));
            manager.NotifyWorldMapIdle(true, true, true);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Village));
            Assert.IsTrue(manager.IsPending(TutorialSteps.RegionNpc));
            Assert.IsTrue(manager.WantsWorldMapIdleReports == false, "both map tips handled: the UI can stop probing");
        }

        [Test]
        public void WorldMapIdle_IgnoredOnFrontEndAndInEncounters()
        {
            TutorialManager manager = CreateManager(GameFlowState.MainMenu);
            manager.NotifyWorldMapIdle(true, true, true);
            Assert.IsFalse(manager.IsPending(TutorialSteps.RegionNpc));
            Assert.IsFalse(manager.WantsWorldMapIdleReports);

            SetFlow(GameFlowState.Encounter);
            manager.NotifyWorldMapIdle(true, true, true);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Village));
        }

        [Test]
        public void MapTips_QueueBehindTheWorldMapTip_AndHideInEncounters()
        {
            TutorialManager manager = CreateManager(GameFlowState.World);
            manager.NotifyWorldMapIdle(true, true, true);
            Assert.AreEqual(TutorialSteps.WorldMap, manager.CurrentHint?.Id, "\"Explorar\" comes first");

            manager.Acknowledge(TutorialSteps.WorldMap);
            Assert.AreEqual(TutorialSteps.RegionNpc, manager.CurrentHint?.Id);

            SetFlow(GameFlowState.Encounter);
            Assert.IsNull(manager.CurrentHint, "world-map tips wait for the map");
            SetFlow(GameFlowState.World);
            Assert.AreEqual(TutorialSteps.RegionNpc, manager.CurrentHint?.Id);
        }

        // ------------------------------------------------------------------
        // Materials / stun
        // ------------------------------------------------------------------

        [Test]
        public void Materials_FiresOnce_OnTheFirstMaterialDrop()
        {
            TutorialManager manager = CreateManager(GameFlowState.Encounter);
            int changes = 0;
            manager.CurrentHintChanged += () => changes++;

            PublishDrop(ItemType.Weapon, 1);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials), "a weapon drop is not a material");

            PublishDrop(ItemType.Material, 0);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials), "nothing granted");

            PublishDrop(ItemType.Material);
            Assert.AreEqual(TutorialSteps.Materials, manager.CurrentHint?.Id);
            Assert.AreEqual(1, changes);

            PublishDrop(ItemType.Material);
            Assert.AreEqual(1, changes, "a second drop while pending does not re-notify");

            Assert.IsTrue(manager.Acknowledge(TutorialSteps.Materials));
            PublishDrop(ItemType.Material);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials));
        }

        [Test]
        public void Materials_IgnoredWhileRestoringOrWithoutDrops()
        {
            TutorialManager manager = CreateManager(GameFlowState.Encounter);
            manager.IsRestoringSave = () => true;
            PublishDrop(ItemType.Material);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials));

            manager.IsRestoringSave = () => false;
            EventBus.Publish(new EnemyLootDroppedEvent(null, null, null));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials));
            Assert.IsFalse(TutorialManager.ContainsMaterial(new List<LootDrop>()));
        }

        [Test]
        public void Stun_FiresOnce_OnlyForStunsOnEnemies()
        {
            TutorialManager manager = CreateManager(GameFlowState.Encounter);

            EventBus.Publish(new StunAppliedEvent(CreatePlain("Hero"), 1f));
            EventBus.Publish(new StunAppliedEvent(null, 1f));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Stun), "a stun on the hero (or nothing) is not the hero's stun");

            GameObject enemy = CreateEnemy();
            EventBus.Publish(new StunAppliedEvent(enemy, 1f));
            Assert.AreEqual(TutorialSteps.Stun, manager.CurrentHint?.Id);

            Assert.IsTrue(manager.Acknowledge(TutorialSteps.Stun));
            EventBus.Publish(new StunAppliedEvent(enemy, 1f));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Stun));
        }

        [Test]
        public void Stun_TriggeredOnTheMap_WaitsForAnEncounter()
        {
            TutorialManager manager = CreateManager(GameFlowState.World);
            manager.Acknowledge(TutorialSteps.WorldMap);
            manager.IsEnemyTarget = _ => true;

            EventBus.Publish(new StunAppliedEvent(CreatePlain("Enemy"), 1f));
            Assert.IsTrue(manager.IsPending(TutorialSteps.Stun));
            Assert.IsNull(manager.CurrentHint, "encounter-only tip");

            SetFlow(GameFlowState.Encounter);
            Assert.AreEqual(TutorialSteps.Stun, manager.CurrentHint?.Id);
        }

        [Test]
        public void ResetTutorial_LetsTheNewStepsFireAgain()
        {
            TutorialManager manager = CreateManager(GameFlowState.Encounter);
            PublishDrop(ItemType.Material);
            EventBus.Publish(new StunAppliedEvent(CreateEnemy(), 1f));
            manager.Acknowledge(TutorialSteps.Materials);
            manager.Acknowledge(TutorialSteps.Stun);
            manager.SetHintsEnabled(false);

            PublishDrop(ItemType.Material);
            Assert.IsFalse(manager.IsPending(TutorialSteps.Materials));

            manager.ResetTutorial();
            PublishDrop(ItemType.Material);
            EventBus.Publish(new StunAppliedEvent(CreateEnemy(), 1f));
            Assert.IsTrue(manager.IsPending(TutorialSteps.Materials));
            Assert.IsTrue(manager.IsPending(TutorialSteps.Stun));
        }

        // ------------------------------------------------------------------
        // Deferral: story panels, Vila panel, region dialogue
        // ------------------------------------------------------------------

        [Test]
        public void Hint_DefersWhileAStoryPanelIsShowingOrAboutToOpen()
        {
            Assert.IsFalse(TutorialHintController.ShouldDefer(false, false, false, false));
            Assert.IsTrue(TutorialHintController.ShouldDefer(false, false, storyPanelShowing: true), "never on top of a story panel");

            Assert.IsTrue(TutorialHintController.IsStoryAboutToShow(true, GameFlowState.World, StoryBeat.Prologue), "prologue about to open on the map");
            Assert.IsTrue(TutorialHintController.IsStoryAboutToShow(true, GameFlowState.World, StoryBeat.Ending));
            Assert.IsFalse(TutorialHintController.IsStoryAboutToShow(true, GameFlowState.World, StoryBeat.None));
            Assert.IsFalse(TutorialHintController.IsStoryAboutToShow(true, GameFlowState.Encounter, StoryBeat.Prologue), "beats only open on the map: encounter tips are not held");
            Assert.IsFalse(TutorialHintController.IsStoryAboutToShow(false, GameFlowState.World, StoryBeat.Prologue), "AutoPresent off (smoke tests): nothing will open");
        }

        [Test]
        public void Hint_DefersWhileTheVilaPanelOrADialogueIsOpen()
        {
            Assert.IsTrue(TutorialHintController.ShouldDefer(false, false, false, worldOverlayOpen: true));

            // The Vila panel and the dialogue box hold GameplayPause while open.
            object hint = new object();
            object villageHub = new object();
            GameplayPause.Acquire(villageHub);
            Assert.IsTrue(TutorialHintController.ShouldDefer(GameplayPause.IsPausedByOthers(hint)));
            GameplayPause.Release(villageHub);
            Assert.IsFalse(TutorialHintController.ShouldDefer(GameplayPause.IsPausedByOthers(hint)));
        }

        [Test]
        public void IsOnScreen_RejectsMissingDetachedOrHiddenElements()
        {
            Assert.IsFalse(TutorialHintController.IsOnScreen(null));
            Assert.IsFalse(TutorialHintController.IsOnScreen(new Button()), "not attached to a panel");
        }
    }
}
