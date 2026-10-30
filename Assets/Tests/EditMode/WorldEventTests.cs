using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Events;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Eventos aleatórios" of the world map: the pure selection rules (<see cref="WorldEventRules"/>: gating, region/time
    /// eligibility, weights, no repeats), the trip roller (<see cref="WorldEventDirector"/>: cooldown, pity, seed), the
    /// catalog's content rules, the outcomes on a real bag/PlayerStats/BuffController, the elite ambush, the saved
    /// state (round trip, old saves, new game, GameManager registration) and the card texts in both languages.
    /// </summary>
    public class WorldEventTests
    {
        private const int TestSlot = 90131;

        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly BiomeType[] Regions = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };
        private static readonly TimeOfDayPhase[] Phases = (TimeOfDayPhase[])Enum.GetValues(typeof(TimeOfDayPhase));

        private readonly List<Object> _created = new List<Object>();
        private GameObject _gameManagerObject;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gm != null)
            {
                // Awake never ran in EditMode, so OnDestroy won't shut the managers down.
                var managers = (List<IManager>)typeof(GameManager)
                    .GetField("_managers", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_gm);
                foreach (IManager manager in managers)
                {
                    manager.Shutdown();
                }

                _gm = null;
            }

            if (_gameManagerObject != null)
            {
                Object.DestroyImmediate(_gameManagerObject);
            }

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
            TestSaveRoot.Exit();
        }

        // ------------------------------------------------------------------
        // Gating
        // ------------------------------------------------------------------

        [Test]
        public void CanStartEvents_OffDuringPrologueTutorialAndModals()
        {
            Assert.IsTrue(WorldEventRules.CanStartEvents(true, false, 1, 1, true, false, false));
            Assert.IsFalse(WorldEventRules.CanStartEvents(false, false, 5, 1, true, false, false), "prologue not seen");
            Assert.IsFalse(WorldEventRules.CanStartEvents(true, true, 5, 1, true, false, false), "a story beat is about to show");
            Assert.IsFalse(WorldEventRules.CanStartEvents(true, false, 0, 1, true, false, false), "first fight (tutorial) not won");
            Assert.IsFalse(WorldEventRules.CanStartEvents(true, false, 5, 1, false, false, false), "HUD hidden");
            Assert.IsFalse(WorldEventRules.CanStartEvents(true, false, 5, 1, true, true, false), "modal screen");
            Assert.IsFalse(WorldEventRules.CanStartEvents(true, false, 5, 1, true, false, true), "conversation");
        }

        // ------------------------------------------------------------------
        // Selection rules
        // ------------------------------------------------------------------

        [Test]
        public void RegionalEvents_OnlyInTheirRegion()
        {
            Assert.IsTrue(WorldEventRules.IsEligible(Spec(WorldEventCatalog.Sandstorm), BiomeType.Desert, TimeOfDayPhase.Day));
            Assert.IsFalse(WorldEventRules.IsEligible(Spec(WorldEventCatalog.Sandstorm), BiomeType.Forest, TimeOfDayPhase.Day));
            Assert.IsTrue(WorldEventRules.IsEligible(Spec(WorldEventCatalog.MountainFog), BiomeType.Mountain, TimeOfDayPhase.Night));
            Assert.IsFalse(WorldEventRules.IsEligible(Spec(WorldEventCatalog.MountainFog), BiomeType.Caves, TimeOfDayPhase.Night));
            Assert.IsTrue(WorldEventRules.IsEligible(Spec(WorldEventCatalog.CaveEcho), BiomeType.Caves, TimeOfDayPhase.Dawn));
            Assert.IsFalse(WorldEventRules.IsEligible(Spec(WorldEventCatalog.CashewRain), BiomeType.Caves, TimeOfDayPhase.Day));
        }

        [Test]
        public void Campfire_IsNightOnly_AndDaytimeEvents_SkipTheNight()
        {
            WorldEventSpec campfire = Spec(WorldEventCatalog.Campfire);
            Assert.IsTrue(WorldEventRules.IsEligible(campfire, BiomeType.Forest, TimeOfDayPhase.Night));
            Assert.IsFalse(WorldEventRules.IsEligible(campfire, BiomeType.Forest, TimeOfDayPhase.Day));
            Assert.IsFalse(WorldEventRules.IsEligible(Spec(WorldEventCatalog.Peddler), BiomeType.Forest, TimeOfDayPhase.Night));
            Assert.IsFalse(WorldEventRules.IsEligible(Spec(WorldEventCatalog.Pilgrims), BiomeType.Desert, TimeOfDayPhase.Night));
        }

        [Test]
        public void EliteAmbush_IsMoreLikelyAtNight()
        {
            WorldEventSpec elite = Spec(WorldEventCatalog.EliteAmbush);
            Assert.AreEqual(elite.Weight, WorldEventRules.GetWeight(elite, BiomeType.Forest, TimeOfDayPhase.Day), 0.0001f);
            Assert.AreEqual(elite.Weight * 1.5f, WorldEventRules.GetWeight(elite, BiomeType.Forest, TimeOfDayPhase.Night), 0.0001f);
            Assert.AreEqual(0f, WorldEventRules.GetWeight(Spec(WorldEventCatalog.Sandstorm), BiomeType.Forest, TimeOfDayPhase.Day));
        }

        [Test]
        public void EveryRegionAndTimeOfDay_HasAtLeastTwoEvents()
        {
            foreach (BiomeType region in Regions)
            {
                foreach (TimeOfDayPhase phase in Phases)
                {
                    int count = WorldEventCatalog.Events.Count(e => WorldEventRules.IsEligible(e, region, phase));
                    Assert.GreaterOrEqual(count, 2, $"{region} at {phase}: no-repeat needs an alternative");
                }
            }
        }

        [Test]
        public void Pick_NeverRepeatsInARow_WhenThereIsAnAlternative()
        {
            var random = new System.Random(7);
            string last = null;
            foreach (BiomeType region in Regions)
            {
                foreach (TimeOfDayPhase phase in Phases)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        WorldEventSpec picked = WorldEventRules.Pick(WorldEventCatalog.Events, region, phase, last, random);
                        Assert.IsNotNull(picked);
                        Assert.AreNotEqual(last, picked.Id, $"{region}/{phase} repeated {last}");
                        Assert.IsTrue(WorldEventRules.IsEligible(picked, region, phase));
                        last = picked.Id;
                    }
                }
            }
        }

        [Test]
        public void Pick_RepeatsOnlyWhenItIsTheOnlyOption_AndNullWhenNothingFits()
        {
            var onlyOne = new List<WorldEventSpec> { Spec(WorldEventCatalog.Sandstorm) };
            Assert.AreEqual(WorldEventCatalog.Sandstorm,
                WorldEventRules.Pick(onlyOne, BiomeType.Desert, TimeOfDayPhase.Day, WorldEventCatalog.Sandstorm, new System.Random(1)).Id);
            Assert.IsNull(WorldEventRules.Pick(onlyOne, BiomeType.Forest, TimeOfDayPhase.Day, null, new System.Random(1)));
        }

        [Test]
        public void Pick_FollowsTheWeights()
        {
            var random = new System.Random(123);
            var counts = new Dictionary<string, int>();
            const int picks = 20000;
            for (int i = 0; i < picks; i++)
            {
                string id = WorldEventRules.Pick(WorldEventCatalog.Events, BiomeType.Desert, TimeOfDayPhase.Day, null, random).Id;
                counts[id] = counts.TryGetValue(id, out int c) ? c + 1 : 1;
            }

            float total = WorldEventCatalog.Events.Sum(e => WorldEventRules.GetWeight(e, BiomeType.Desert, TimeOfDayPhase.Day));
            foreach (WorldEventSpec spec in WorldEventCatalog.Events)
            {
                float expected = WorldEventRules.GetWeight(spec, BiomeType.Desert, TimeOfDayPhase.Day) / total;
                float actual = counts.TryGetValue(spec.Id, out int n) ? n / (float)picks : 0f;
                Assert.AreEqual(expected, actual, 0.02f, spec.Id);
            }
        }

        // ------------------------------------------------------------------
        // Trip roller
        // ------------------------------------------------------------------

        [Test]
        public void Director_CooldownBlocksTheNextTrips_ThenRollsAgain()
        {
            var state = new WorldEventState();
            var settings = new WorldEventSettings { BaseChancePerTrip = 1f, MaxChancePerTrip = 1f, CooldownTrips = 2 };
            var director = new WorldEventDirector(settings, state, new System.Random(3));

            float? first = director.RollTrip(true);
            Assert.IsTrue(first.HasValue, "chance 1 off cooldown");
            Assert.That(first.Value, Is.InRange(settings.MinTriggerProgress, settings.MaxTriggerProgress));

            director.MarkTriggered(Spec(WorldEventCatalog.Peddler));
            Assert.AreEqual(2, state.TripsUntilEligible);
            Assert.AreEqual(WorldEventCatalog.Peddler, state.LastEventId);
            Assert.AreEqual(1, state.TimesSeen(WorldEventCatalog.Peddler));

            Assert.IsNull(director.RollTrip(true));
            Assert.IsNull(director.RollTrip(true));
            Assert.AreEqual(0, state.TripsUntilEligible);
            Assert.IsTrue(director.RollTrip(true).HasValue, "cooldown over");
        }

        [Test]
        public void Director_TripsWhileNotAllowed_ChangeNothing()
        {
            var state = new WorldEventState { TripsUntilEligible = 1, TripsWithoutEvent = 2 };
            var director = new WorldEventDirector(new WorldEventSettings(), state, new System.Random(3));

            Assert.IsNull(director.RollTrip(false));
            Assert.AreEqual(1, state.TripsUntilEligible);
            Assert.AreEqual(2, state.TripsWithoutEvent);
        }

        [Test]
        public void Director_ChanceRampsWithDroughts_UpToTheCap()
        {
            var state = new WorldEventState();
            var settings = new WorldEventSettings { BaseChancePerTrip = 0f, ChanceRampPerTrip = 0.2f, MaxChancePerTrip = 0.5f };
            var director = new WorldEventDirector(settings, state, new System.Random(5));

            Assert.AreEqual(0f, director.ChanceForNextTrip, 0.0001f);
            Assert.IsNull(director.RollTrip(true), "chance 0 never hits");
            Assert.AreEqual(1, state.TripsWithoutEvent);
            Assert.AreEqual(0.2f, director.ChanceForNextTrip, 0.0001f);

            state.TripsWithoutEvent = 10;
            Assert.AreEqual(0.5f, director.ChanceForNextTrip, 0.0001f, "capped");

            director.MarkTriggered(Spec(WorldEventCatalog.Peddler));
            Assert.AreEqual(0, state.TripsWithoutEvent, "pity resets on an event");
            Assert.AreEqual(0f, director.ChanceForNextTrip, "on cooldown");
        }

        [Test]
        public void Director_SameSeed_SameSequence()
        {
            List<string> Run(int seed)
            {
                var director = new WorldEventDirector(new WorldEventSettings { CooldownTrips = 0 }, new WorldEventState(), new System.Random(seed));
                var log = new List<string>();
                for (int i = 0; i < 60; i++)
                {
                    float? at = director.RollTrip(true);
                    if (!at.HasValue)
                    {
                        log.Add("-");
                        continue;
                    }

                    WorldEventSpec spec = director.PickEvent(Regions[i % Regions.Length], Phases[i % Phases.Length]);
                    director.MarkTriggered(spec);
                    log.Add($"{at.Value:0.000}:{spec?.Id}");
                }

                return log;
            }

            CollectionAssert.AreEqual(Run(42), Run(42));
            CollectionAssert.AreNotEqual(Run(42), Run(43));
        }

        [Test]
        public void Director_DefaultSettings_GiveAnEventEveryFewTrips()
        {
            var state = new WorldEventState();
            var director = new WorldEventDirector(new WorldEventSettings(), state, new System.Random(99));
            int events = 0;
            const int trips = 3000;
            for (int i = 0; i < trips; i++)
            {
                if (director.RollTrip(true).HasValue)
                {
                    events++;
                    director.MarkTriggered(Spec(i % 2 == 0 ? WorldEventCatalog.Peddler : WorldEventCatalog.EliteAmbush));
                }
            }

            // Docs/Balance/README.md: about one event every 5 trips (cooldown 2 + ~2 rolls).
            Assert.That(trips / (float)events, Is.InRange(4f, 6f));
        }

        // ------------------------------------------------------------------
        // Content
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_IdsAreUnique_ItemsExist_AndChoiceBIsAlwaysFree()
        {
            CollectionAssert.AllItemsAreUnique(WorldEventCatalog.Events.Select(e => e.Id));
            foreach (WorldEventSpec spec in WorldEventCatalog.Events)
            {
                Assert.Greater(spec.Weight, 0, spec.Id);
                Assert.IsNotNull(spec.ChoiceA, spec.Id);
                Assert.IsNotNull(spec.ChoiceB, spec.Id);
                CollectionAssert.IsEmpty(spec.ChoiceB.Costs.ToList(), $"{spec.Id}: choice B must be free so the card can always be closed");

                foreach (WorldEventEffect effect in spec.ChoiceA.Effects.Concat(spec.ChoiceB.Effects))
                {
                    if (effect.Kind == WorldEventEffectKind.GainItem || effect.Kind == WorldEventEffectKind.LoseItem)
                    {
                        Assert.IsNotNull(ItemDatabase.GetById(effect.ItemId), $"{spec.Id}: item {effect.ItemId}");
                        Assert.Greater(effect.Amount, 0, spec.Id);
                    }
                }
            }
        }

        [Test]
        public void Peddler_IsCheaperThanTheVillageShop()
        {
            int shopPrice = ItemDatabase.GetById(WorldEventCatalog.HealthPotion).Value * WorldEventCatalog.PeddlerPotions;
            Assert.Less(WorldEventCatalog.PeddlerPrice, shopPrice);
        }

        // ------------------------------------------------------------------
        // Outcomes
        // ------------------------------------------------------------------

        [Test]
        public void Peddler_TradesGoldForPotions_AndRefusesWithoutGold()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(ItemDatabase.GetById(WorldEventCatalog.Gold), 30);
            var state = new WorldEventState();

            WorldEventOutcome outcome = WorldEventOutcomes.Apply(Spec(WorldEventCatalog.Peddler).ChoiceA, BiomeType.Desert, bag, state);
            Assert.IsNotNull(outcome);
            Assert.AreEqual(30 - WorldEventCatalog.PeddlerPrice, bag.GetTotalCount(WorldEventCatalog.Gold), "price is not region-scaled");
            Assert.AreEqual(WorldEventCatalog.PeddlerPotions, bag.GetTotalCount(WorldEventCatalog.HealthPotion));

            Assert.IsNull(WorldEventOutcomes.Apply(Spec(WorldEventCatalog.Peddler).ChoiceA, BiomeType.Desert, bag, state), "12 gold is not enough");
            Assert.AreEqual(12, bag.GetTotalCount(WorldEventCatalog.Gold), "a refused choice changes nothing");
            Assert.AreEqual(WorldEventCatalog.PeddlerPotions, bag.GetTotalCount(WorldEventCatalog.HealthPotion));
        }

        [Test]
        public void Pilgrims_GivePotion_ForGoldAndRegionScaledNextFightXp()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(ItemDatabase.GetById(WorldEventCatalog.HealthPotion), 1);
            var state = new WorldEventState();

            WorldEventOutcome outcome = WorldEventOutcomes.Apply(Spec(WorldEventCatalog.Pilgrims).ChoiceA, BiomeType.Caves, bag, state);
            Assert.IsNotNull(outcome);
            Assert.AreEqual(0, bag.GetTotalCount(WorldEventCatalog.HealthPotion));
            Assert.AreEqual(WorldEventCatalog.PilgrimsGold, bag.GetTotalCount(WorldEventCatalog.Gold), "gold is not region-scaled");
            Assert.AreEqual(60, state.PendingExperience, "15 × 4 in the caves, waiting for the next fight");
        }

        [Test]
        public void TravelClockAndFightEffects_AreReturnedForTheMap()
        {
            var state = new WorldEventState();
            WorldEventOutcome storm = WorldEventOutcomes.Apply(Spec(WorldEventCatalog.Sandstorm).ChoiceA, BiomeType.Desert, NewBag(), state);
            Assert.AreEqual(0.5f, storm.TravelSpeedMultiplier, 0.0001f);

            WorldEventOutcome shelter = WorldEventOutcomes.Apply(Spec(WorldEventCatalog.Sandstorm).ChoiceB, BiomeType.Desert, NewBag(), state);
            Assert.AreEqual(2f, shelter.HoursToAdvance, 0.0001f);
            Assert.AreEqual(1f, shelter.TravelSpeedMultiplier, 0.0001f);

            Assert.IsTrue(WorldEventOutcomes.Apply(Spec(WorldEventCatalog.EliteAmbush).ChoiceA, BiomeType.Forest, NewBag(), state).StartElite);
            WorldEventOutcome echo = WorldEventOutcomes.Apply(Spec(WorldEventCatalog.CaveEcho).ChoiceA, BiomeType.Caves, NewBag(), state);
            Assert.IsTrue(echo.StartEncounter);
            Assert.AreEqual(80, state.PendingExperience, "20 × 4, applied in the fight the echo starts");
        }

        [Test]
        public void PendingBoons_StackSensibly()
        {
            var state = new WorldEventState();
            state.AddPendingHeal(0.6f);
            state.AddPendingHeal(0.6f);
            Assert.AreEqual(1f, state.PendingHealFraction, 0.0001f, "capped at a full heal");

            state.AddPendingBuff(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.15f);
            state.AddPendingBuff(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.15f);
            state.AddPendingBuff(StatType.Defense, ModifierApplication.PercentAdditive, 0.1f);
            Assert.AreEqual(2, state.PendingBuffs.Count, "same stat replaces, like the BuffController would");
        }

        [Test]
        public void ApplyPendingBoons_GivesXpHealAndBuffsToTheHero_AndClearsTheQueue()
        {
            var go = new GameObject("WorldEventTests_Hero");
            _created.Add(go);
            BuffController buffs = go.AddComponent<BuffController>();
            PlayerStats stats = go.AddComponent<PlayerStats>();
            stats.ApplyDamage(stats.MaxHealth * 0.6f);
            float before = stats.CurrentHealth;
            int xpBefore = stats.CurrentExperience;

            var state = new WorldEventState();
            state.AddPendingExperience(10);
            state.AddPendingHeal(0.25f);
            state.AddPendingBuff(StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.15f);

            Assert.IsTrue(WorldEventOutcomes.ApplyPendingBoons(state, stats, buffs));
            Assert.AreEqual(xpBefore + 10, stats.CurrentExperience);
            Assert.AreEqual(before + stats.MaxHealth * 0.25f, stats.CurrentHealth, 0.01f);
            Assert.AreEqual(0.15f, buffs.GetPercentTotal(StatType.AttackDamage), 0.0001f);
            Assert.IsFalse(state.HasPendingBoons);
            Assert.IsFalse(WorldEventOutcomes.ApplyPendingBoons(state, stats, buffs), "nothing left");
        }

        // ------------------------------------------------------------------
        // Elite ambush
        // ------------------------------------------------------------------

        [Test]
        public void ElitePayload_LeadsWithARegularEnemyOfTheRegion()
        {
            BiomeDefinition forest = BiomeRegistry.Get(BiomeType.Forest);
            Assume.That(forest, Is.Not.Null);
            for (int seed = 0; seed < 20; seed++)
            {
                EncounterPayload payload = WorldEventElite.BuildPayload(forest, new System.Random(seed));
                Assert.IsNotNull(payload);
                Assert.AreEqual(BiomeType.Forest, payload.biome);
                Assert.AreEqual(1 + WorldEventElite.Minions, payload.EnemyCount);
                Assert.IsFalse(payload.isBossChallenge);
                Assert.IsTrue(payload.ResolveEnemies().All(e => !e.IsBoss), "never a boss");
            }
        }

        [Test]
        public void MakeElite_DoublesHealth_BuffsDamage_AndGrows()
        {
            var go = new GameObject("WorldEventTests_Elite");
            _created.Add(go);
            Health health = go.AddComponent<Health>();
            BuffController buffs = go.AddComponent<BuffController>();
            health.ConfigureMaxHealth(80f);

            WorldEventArenaHooks.MakeElite(go);

            Assert.AreEqual(160f, health.MaxHealth, 0.01f);
            Assert.AreEqual(160f, health.CurrentHealth, 0.01f);
            Assert.AreEqual(WorldEventElite.AttackDamageBonus, buffs.GetPercentTotal(StatType.AttackDamage), 0.0001f);
            Assert.AreEqual(WorldEventElite.ScaleMultiplier, go.transform.localScale.x, 0.0001f);
            Assert.AreEqual(56, WorldEventElite.ScaledExperience(BiomeType.Mountain), "20 × 2.8");
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_RoundTripsCooldownSeenAndBoons()
        {
            var save = new SaveManager();
            save.Initialize();
            var state = new WorldEventState { TripsUntilEligible = 2, TripsWithoutEvent = 1 };
            state.RecordSeen(WorldEventCatalog.CaveEcho);
            state.RecordSeen(WorldEventCatalog.CaveEcho);
            state.AddPendingExperience(70);
            state.AddPendingHeal(0.35f);
            state.AddPendingBuff(StatType.Defense, ModifierApplication.PercentAdditive, 0.1f);
            save.Register(state);
            save.Save(TestSlot);

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            var loaded = new WorldEventState();
            loadedSave.Register(loaded);
            Assert.IsTrue(loadedSave.Load(TestSlot));

            Assert.AreEqual(2, loaded.TripsUntilEligible);
            Assert.AreEqual(1, loaded.TripsWithoutEvent);
            Assert.AreEqual(WorldEventCatalog.CaveEcho, loaded.LastEventId);
            Assert.AreEqual(2, loaded.TimesSeen(WorldEventCatalog.CaveEcho));
            Assert.AreEqual(70, loaded.PendingExperience);
            Assert.AreEqual(0.35f, loaded.PendingHealFraction, 0.0001f);
            Assert.AreEqual(1, loaded.PendingBuffs.Count);
            Assert.AreEqual(StatType.Defense, loaded.PendingBuffs[0].affectedStat);
        }

        [Test]
        public void OldSave_WithoutEvents_KeepsAFreshState()
        {
            var save = new SaveManager();
            save.Initialize();
            save.Save(TestSlot);

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            var loaded = new WorldEventState();
            loadedSave.Register(loaded);
            Assert.IsTrue(loadedSave.Load(TestSlot));
            Assert.AreEqual(0, loaded.TripsUntilEligible);
            Assert.IsNull(loaded.LastEventId);
            Assert.IsFalse(loaded.HasPendingBoons);
        }

        [Test]
        public void GameManager_RegistersTheState_AndNewGameResetsIt()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            _gameManagerObject = new GameObject("WorldEventTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.WorldEvents, Is.Not.Null);

            Assert.IsTrue(_gm.NewSessionReset.Resettables.Contains(_gm.WorldEvents));
            Assert.AreSame(_gm.WorldEvents, WorldEventState.ResolveShared());

            _gm.WorldEvents.AddPendingExperience(30);
            _gm.WorldEvents.RecordSeen(WorldEventCatalog.Peddler);
            _gm.SaveManager.Save(TestSlot);
            Assert.IsTrue(_gm.SaveManager.PeekSlot(TestSlot).entries.Any(e => e.key == WorldEventState.Key), "registered with SaveManager");

            _gm.GameFlowService.EnterFrontEnd();
            Assert.IsTrue(_gm.GameFlowService.StartNewGame());
            Assert.IsFalse(_gm.WorldEvents.HasPendingBoons, "Novo jogo starts clean");
            Assert.IsNull(_gm.WorldEvents.LastEventId);

            Assert.IsTrue(_gm.SaveManager.Load(TestSlot));
            Assert.AreEqual(30, _gm.WorldEvents.PendingExperience);
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void EveryEventKey_HasBothLanguages_AndALinePerCousin()
        {
            Dictionary<string, LocalizationEntry> table = WorldEventTexts.Entries.ToDictionary(e => e.Key);
            foreach (WorldEventSpec spec in WorldEventCatalog.Events)
            {
                var keys = new List<string> { spec.TitleKey, spec.BodyKey, spec.ChoiceKey(0), spec.ChoiceKey(1), spec.ResultKey(0), spec.ResultKey(1) };
                keys.AddRange(Heroes.Select(spec.LineKey));
                foreach (string key in keys)
                {
                    Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"missing {key}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key} en");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key} pt-BR");
                }
            }

            foreach (FieldInfo field in typeof(WorldEventTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && f.FieldType == typeof(string)))
            {
                string value = (string)field.GetRawConstantValue();
                if (value.StartsWith("ui.", StringComparison.Ordinal))
                {
                    Assert.IsTrue(table.ContainsKey(value), $"WorldEventTexts.{field.Name}");
                }
            }
        }

        [Test]
        public void CardTexts_FollowTheLanguage()
        {
            WorldEventSpec peddler = Spec(WorldEventCatalog.Peddler);
            var english = new LocalizationManager();
            english.SetLanguage(LanguageCode.en);
            var portuguese = new LocalizationManager();
            portuguese.SetLanguage(LanguageCode.ptBR);

            Assert.AreEqual("Wandering peddler", WorldEventTexts.Get(english, peddler.TitleKey));
            Assert.AreEqual("Mascate ambulante", WorldEventTexts.Get(portuguese, peddler.TitleKey));
            Assert.AreEqual("Mascate ambulante", WorldEventTexts.Get(null, peddler.TitleKey), "pt-BR without a manager");

            string ptLine = WorldEventCardController.BuildHeroLine(peddler, CharacterId.Lavine, portuguese);
            StringAssert.Contains("Não confio", ptLine);
            StringAssert.StartsWith(WorldMapHudPresenter.HeroName(portuguese, CharacterId.Lavine), ptLine);
            StringAssert.Contains("I don't trust him", WorldEventCardController.BuildHeroLine(peddler, CharacterId.Lavine, english));
            Assert.AreEqual(string.Empty, WorldEventCardController.BuildHeroLine(peddler, null, portuguese));

            string ptChoice = WorldEventCardController.DescribeChoice(peddler.ChoiceA, BiomeType.Forest, portuguese);
            StringAssert.Contains("-" + WorldEventCatalog.PeddlerPrice, ptChoice);
            StringAssert.Contains("+" + WorldEventCatalog.PeddlerPotions, ptChoice);
            Assert.AreEqual("Nada muda.", WorldEventCardController.DescribeChoice(peddler.ChoiceB, BiomeType.Forest, portuguese));
            Assert.AreEqual("Nothing changes.", WorldEventCardController.DescribeChoice(peddler.ChoiceB, BiomeType.Forest, english));

            StringAssert.Contains("+15% de dano", WorldEventCardController.DescribeChoice(Spec(WorldEventCatalog.Campfire).ChoiceA, BiomeType.Forest, portuguese));
            StringAssert.Contains("+60 XP", WorldEventCardController.DescribeChoice(Spec(WorldEventCatalog.Pilgrims).ChoiceA, BiomeType.Caves, english));
            StringAssert.Contains("50% slower", WorldEventCardController.DescribeChoice(Spec(WorldEventCatalog.Sandstorm).ChoiceA, BiomeType.Desert, english));
        }

        [Test]
        public void TotalVictories_SumsEveryRegion()
        {
            var progress = new WorldMapProgress();
            Assert.AreEqual(0, WorldEventCardController.TotalVictories(progress));
            progress.RecordVictory(BiomeType.Forest);
            progress.RecordVictory(BiomeType.Desert);
            progress.RecordVictory(BiomeType.Desert);
            Assert.AreEqual(3, WorldEventCardController.TotalVictories(progress));
            Assert.AreEqual(0, WorldEventCardController.TotalVictories(null));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static WorldEventSpec Spec(string id)
        {
            WorldEventSpec spec = WorldEventCatalog.Get(id);
            Assert.IsNotNull(spec, id);
            return spec;
        }

        private InventoryManager NewBag()
        {
            var go = new GameObject("WorldEventTests_Bag");
            _created.Add(go);
            return go.AddComponent<InventoryManager>();
        }
    }
}
