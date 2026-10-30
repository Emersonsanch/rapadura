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
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Chatter;
using Rapadura.Save;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Conversas entre primos na estrada" and the narrative connections of Docs/Narrative.md: the catalog's content rules
    /// (every pair of cousins, more for siblings, canon-safe sibling words), the pure selection rules
    /// (<see cref="RoadChatterRules"/>: stage gating, Nova Jornada+, no repeats, cooldown), the saved state, the bubble
    /// texts in both languages, the region NPCs' "after the boss" greetings and a few canon guards on the lore.
    /// </summary>
    public class RoadChatterTests
    {
        private const int TestSlot = 90147;

        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly RoadChatterStage[] Stages = (RoadChatterStage[])Enum.GetValues(typeof(RoadChatterStage));

        private GameObject _gameManagerObject;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
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

            EventBus.Clear();
            ServiceLocator.Clear();
            TestSaveRoot.Exit();
        }

        // ------------------------------------------------------------------
        // Catalog content
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_HasAtLeastFortyUniqueExchanges_WithBothLanguages()
        {
            Assert.GreaterOrEqual(RoadChatterCatalog.All.Count, 40);
            Assert.AreEqual(RoadChatterCatalog.All.Count, RoadChatterCatalog.All.Select(s => s.Id).Distinct().Count(), "duplicate id");
            foreach (RoadChatterSpec spec in RoadChatterCatalog.All)
            {
                Assert.AreNotEqual(spec.First, spec.Second, spec.Id);
                Assert.LessOrEqual(spec.MinStage, spec.MaxStage, spec.Id);
                Assert.GreaterOrEqual(spec.MinCycle, 0, spec.Id);
                foreach (RoadChatterLine line in new[] { spec.FirstLine, spec.SecondLine })
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line.En), spec.Id + " en");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line.Pt), spec.Id + " pt-BR");
                }

                Assert.AreSame(spec, RoadChatterCatalog.Find(spec.Id));
            }
        }

        [Test]
        public void Catalog_CoversEveryPair_WithMoreForTheSiblingPairs()
        {
            int Count(CharacterId a, CharacterId b) => RoadChatterCatalog.All.Count(s => s.Involves(a) && s.Involves(b));

            var cousinCounts = new List<int>();
            for (int i = 0; i < Heroes.Length; i++)
            {
                for (int j = i + 1; j < Heroes.Length; j++)
                {
                    int count = Count(Heroes[i], Heroes[j]);
                    Assert.GreaterOrEqual(count, 3, $"{Heroes[i]} & {Heroes[j]}");
                    bool siblings = HeroFamily.TryGetSibling(Heroes[i], out CharacterId sibling) && sibling == Heroes[j];
                    if (!siblings)
                    {
                        cousinCounts.Add(count);
                    }
                }
            }

            Assert.Greater(Count(CharacterId.Maithe, CharacterId.Icaro), cousinCounts.Max(), "Maithe & Ícaro (siblings)");
            Assert.Greater(Count(CharacterId.Maria, CharacterId.Joaquim), cousinCounts.Max(), "Maria & Joaquim (siblings)");
        }

        [Test]
        public void Catalog_EveryCousinHasSomethingToSayAtEveryStage()
        {
            foreach (CharacterId hero in Heroes)
            {
                foreach (RoadChatterStage stage in Stages)
                {
                    Assert.IsTrue(RoadChatterCatalog.All.Any(s => RoadChatterRules.IsEligible(s, hero, stage, 0)), $"{hero} at {stage}");
                }

                Assert.IsTrue(RoadChatterCatalog.All.Any(s => s.MinCycle > 0 && s.PlayableBy(hero)), $"{hero}: no Nova Jornada+ exchange");
            }
        }

        [Test]
        public void Catalog_SiblingWordsFollowTheBirthOrder()
        {
            // "maninho/maninha" and "pestinha" go from an older sibling down; "mana" goes from a younger sibling up;
            // cousins never call each other brother/sister.
            foreach (RoadChatterSpec spec in RoadChatterCatalog.All)
            {
                for (int index = 0; index < 2; index++)
                {
                    CharacterId speaker = spec.Speaker(index);
                    CharacterId listener = spec.Speaker(1 - index);
                    string pt = (index == 0 ? spec.FirstLine : spec.SecondLine).Pt.ToLowerInvariant();
                    bool siblings = HeroFamily.TryGetSibling(speaker, out CharacterId sibling) && sibling == listener;
                    bool older = HeroFamily.BirthRank(speaker) < HeroFamily.BirthRank(listener);

                    if (pt.Contains("maninh") || pt.Contains("pestinha"))
                    {
                        Assert.IsTrue(siblings && older, $"{spec.Id}: '{pt}' must be said by the older sibling");
                    }

                    if (System.Text.RegularExpressions.Regex.IsMatch(pt, @"\bmana\b"))
                    {
                        Assert.IsTrue(siblings && !older, $"{spec.Id}: 'mana' is said by the younger sibling");
                    }

                    if (!siblings)
                    {
                        StringAssert.DoesNotContain("irmã", pt, spec.Id);
                        StringAssert.DoesNotContain("irmão", pt, spec.Id);
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Rules
        // ------------------------------------------------------------------

        [Test]
        public void StageFrom_FollowsTheInterludesSeen()
        {
            Assert.AreEqual(RoadChatterStage.Forest, RoadChatterRules.StageFrom(false, false, false, false));
            Assert.AreEqual(RoadChatterStage.Desert, RoadChatterRules.StageFrom(true, false, false, false));
            Assert.AreEqual(RoadChatterStage.Mountain, RoadChatterRules.StageFrom(true, true, false, false));
            Assert.AreEqual(RoadChatterStage.Caves, RoadChatterRules.StageFrom(true, true, true, false));
            Assert.AreEqual(RoadChatterStage.Home, RoadChatterRules.StageFrom(true, true, true, true));
        }

        [Test]
        public void CanShow_OnlyWhileTravellingWithNothingElseOnScreen()
        {
            Assert.IsTrue(RoadChatterRules.CanShow(true, true, true, false, true, false, false, false));
            Assert.IsFalse(RoadChatterRules.CanShow(false, true, true, false, true, false, false, false), "idle token");
            Assert.IsFalse(RoadChatterRules.CanShow(true, false, true, false, true, false, false, false), "no cousin");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, false, false, true, false, false, false), "before the prologue");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, true, true, true, false, false, false), "story beat waiting");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, true, false, false, false, false, false), "HUD hidden");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, true, false, true, true, false, false), "paused (story panel, menus)");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, true, false, true, false, true, false), "conversation");
            Assert.IsFalse(RoadChatterRules.CanShow(true, true, true, false, true, false, false, true), "road event card");
        }

        [Test]
        public void IsEligible_GatesByStageCycleAndWhoIsOnTheRoad()
        {
            RoadChatterSpec dream = RoadChatterCatalog.Find("maria_joaquim_dream");
            Assert.IsTrue(RoadChatterRules.IsEligible(dream, CharacterId.Maria, RoadChatterStage.Forest, 0));
            Assert.IsTrue(RoadChatterRules.IsEligible(dream, CharacterId.Joaquim, RoadChatterStage.Forest, 0));
            Assert.IsFalse(RoadChatterRules.IsEligible(dream, CharacterId.Maithe, RoadChatterStage.Forest, 0), "not in this pair");
            Assert.IsFalse(RoadChatterRules.IsEligible(dream, CharacterId.Maria, RoadChatterStage.Desert, 0), "the Alpha Wolf is beaten by then");

            RoadChatterSpec panel = RoadChatterCatalog.Find("maithe_icaro_panel");
            Assert.IsFalse(RoadChatterRules.IsEligible(panel, CharacterId.Icaro, RoadChatterStage.Desert, 0), "before the Desert interlude");

            RoadChatterSpec memory = RoadChatterCatalog.Find("maithe_icaro_caja_machine");
            Assert.AreEqual(RoadChatterKind.Memory, memory.Kind);
            Assert.IsTrue(RoadChatterRules.IsEligible(memory, CharacterId.Maithe, RoadChatterStage.Forest, 0));
            Assert.IsFalse(RoadChatterRules.IsEligible(memory, CharacterId.Icaro, RoadChatterStage.Forest, 0), "only the cousin who remembers");

            RoadChatterSpec ngPlus = RoadChatterCatalog.Find("maithe_icaro_experiment_two");
            Assert.IsFalse(RoadChatterRules.IsEligible(ngPlus, CharacterId.Maithe, RoadChatterStage.Forest, 0));
            Assert.IsTrue(RoadChatterRules.IsEligible(ngPlus, CharacterId.Maithe, RoadChatterStage.Forest, 1));
        }

        [Test]
        public void Pick_NeverRepeats_AndRunsOutCleanly()
        {
            var seen = new HashSet<string>();
            var random = new System.Random(5);
            int eligible = RoadChatterCatalog.All.Count(s => RoadChatterRules.IsEligible(s, CharacterId.Lavine, RoadChatterStage.Desert, 0));
            Assert.Greater(eligible, 0);

            for (int i = 0; i < eligible; i++)
            {
                RoadChatterSpec spec = RoadChatterRules.Pick(RoadChatterCatalog.All, CharacterId.Lavine, RoadChatterStage.Desert, 0, seen.Contains, random);
                Assert.IsNotNull(spec);
                Assert.IsTrue(seen.Add(spec.Id), "repeated " + spec.Id);
                Assert.IsTrue(spec.Involves(CharacterId.Lavine));
            }

            Assert.IsNull(RoadChatterRules.Pick(RoadChatterCatalog.All, CharacterId.Lavine, RoadChatterStage.Desert, 0, seen.Contains, random));
            Assert.IsNull(RoadChatterRules.Pick(RoadChatterCatalog.All, null, RoadChatterStage.Desert, 0, null, random), "no cousin, no talk");
        }

        [Test]
        public void Pick_PlaysTheSoonestToExpireFirst()
        {
            var catalog = new List<RoadChatterSpec>
            {
                Spec("long", RoadChatterStage.Forest, RoadChatterStage.Home),
                Spec("short", RoadChatterStage.Forest, RoadChatterStage.Forest),
            };

            Assert.AreEqual("short", RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, null, new System.Random(1)).Id);
            Assert.AreEqual("long", RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, id => id == "short", new System.Random(1)).Id);
            Assert.AreEqual("long", RoadChatterRules.Pick(catalog, CharacterId.Icaro, RoadChatterStage.Desert, 0, null, null).Id);
        }

        [Test]
        public void Pick_AfterEverythingWasHeard_RepeatsOldestHeardFirst()
        {
            var catalog = new List<RoadChatterSpec>
            {
                Spec("a", RoadChatterStage.Forest, RoadChatterStage.Home),
                Spec("b", RoadChatterStage.Forest, RoadChatterStage.Home),
                Spec("c", RoadChatterStage.Forest, RoadChatterStage.Home),
            };
            var settings = new RoadChatterSettings { CooldownTrips = 2 };
            var state = new RoadChatterState();
            var random = new System.Random(7);

            var firstRound = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                RoadChatterSpec spec = RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, random, state.HeardRank);
                Assert.IsNotNull(spec);
                Assert.IsFalse(state.HasSeen(spec.Id), "unheard ones come first");
                RoadChatterRules.MarkShown(state, spec, settings);
                firstRound.Add(spec.Id);
            }

            Assert.IsNull(RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, random),
                "without a heard rank there are no repeats");

            for (int i = 0; i < 6; i++)
            {
                RoadChatterSpec spec = RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, random, state.HeardRank);
                Assert.AreEqual(firstRound[i % 3], spec.Id, "repeat " + i + " is the oldest heard");
                RoadChatterRules.MarkShown(state, spec, settings);
                Assert.AreEqual(2, state.TripsUntilEligible, "same cooldown on repeats");
                Assert.AreEqual(spec.Id, state.HeardOrder[state.HeardOrder.Count - 1]);
            }

            Assert.IsNull(RoadChatterRules.Pick(catalog, CharacterId.Lavine, RoadChatterStage.Forest, 0, state.HasSeen, random, state.HeardRank),
                "repeats still respect who is on the road");
        }

        [Test]
        public void Pick_RepeatsOnlyOnceEveryEligibleExchangeWasHeard()
        {
            var catalog = new List<RoadChatterSpec>
            {
                Spec("old", RoadChatterStage.Forest, RoadChatterStage.Home),
                Spec("new", RoadChatterStage.Forest, RoadChatterStage.Home),
            };
            var state = new RoadChatterState();
            state.MarkSeen("old");
            Assert.AreEqual("new", RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, null, state.HeardRank).Id);
            state.MarkSeen("new");
            Assert.AreEqual("old", RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, null, state.HeardRank).Id);
            state.MarkSeen("old");
            Assert.AreEqual("new", RoadChatterRules.Pick(catalog, CharacterId.Maithe, RoadChatterStage.Forest, 0, state.HasSeen, null, state.HeardRank).Id);
        }

        [Test]
        public void SaveLoad_KeepsTheHeardOrder_AndOlderSavesCountAsOldest()
        {
            var state = new RoadChatterState();
            state.MarkSeen("b");
            state.MarkSeen("a");
            state.MarkSeen("b");
            var snapshot = (RoadChatterSaveState)state.CaptureState();
            CollectionAssert.AreEqual(new[] { "a", "b" }, snapshot.heardOrder);

            var loaded = new RoadChatterState();
            loaded.RestoreState(snapshot);
            CollectionAssert.AreEqual(new[] { "a", "b" }, loaded.HeardOrder.ToArray());

            var legacy = new RoadChatterSaveState { seenIds = new List<string> { "x", "y" }, heardOrder = new List<string> { "y" } };
            loaded.RestoreState(legacy);
            Assert.AreEqual(0, loaded.HeardRank("x"), "seen without an order = oldest");
            Assert.AreEqual(1, loaded.HeardRank("y"));
            Assert.AreEqual(-1, loaded.HeardRank("a"));

            loaded.RestoreState(new RoadChatterSaveState { seenIds = new List<string> { "z" }, heardOrder = null });
            Assert.IsTrue(loaded.HasSeen("z"));
            Assert.AreEqual(0, loaded.HeardRank("z"));
        }

        [Test]
        public void RollTrip_RespectsTheCooldown_AndMarkShownStartsIt()
        {
            var settings = new RoadChatterSettings { ChancePerTrip = 1f, CooldownTrips = 2 };
            var state = new RoadChatterState();
            var random = new System.Random(3);

            Assert.IsTrue(RoadChatterRules.RollTrip(state, settings, true, random));
            Assert.IsFalse(RoadChatterRules.RollTrip(state, settings, false, random), "not allowed");

            RoadChatterSpec spec = RoadChatterCatalog.All[0];
            RoadChatterRules.MarkShown(state, spec, settings);
            Assert.IsTrue(state.HasSeen(spec.Id));
            Assert.AreEqual(2, state.TripsUntilEligible);
            Assert.IsFalse(RoadChatterRules.RollTrip(state, settings, true, random));
            Assert.IsFalse(RoadChatterRules.RollTrip(state, settings, true, random));
            Assert.AreEqual(0, state.TripsUntilEligible);
            Assert.IsTrue(RoadChatterRules.RollTrip(state, settings, true, random));

            Assert.IsFalse(RoadChatterRules.RollTrip(state, new RoadChatterSettings { ChancePerTrip = 0f }, true, random), "zero chance");
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_RoundTripsSeenAndCooldown_AndOldSavesStartFresh()
        {
            var save = new SaveManager();
            save.Initialize();
            var state = new RoadChatterState { TripsUntilEligible = 1 };
            state.MarkSeen("maithe_icaro_radio_test");
            state.MarkSeen("maria_joaquim_dream");
            save.Register(state);
            save.Save(TestSlot);

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            var loaded = new RoadChatterState();
            loadedSave.Register(loaded);
            Assert.IsTrue(loadedSave.Load(TestSlot));
            Assert.AreEqual(1, loaded.TripsUntilEligible);
            Assert.IsTrue(loaded.HasSeen("maithe_icaro_radio_test"));
            Assert.IsTrue(loaded.HasSeen("maria_joaquim_dream"));

            var oldSave = new SaveManager();
            oldSave.Initialize();
            oldSave.Save(TestSlot);
            var fresh = new RoadChatterState { TripsUntilEligible = 4 };
            fresh.MarkSeen("x");
            fresh.ResetForNewSession();
            var oldLoaded = new SaveManager();
            oldLoaded.Initialize();
            oldLoaded.Register(fresh);
            Assert.IsTrue(oldLoaded.Load(TestSlot));
            Assert.AreEqual(0, fresh.TripsUntilEligible);
            Assert.AreEqual(0, fresh.SeenIds.Count);
        }

        [Test]
        public void GameManager_RegistersTheState_AndNewGameResetsIt()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            _gameManagerObject = new GameObject("RoadChatterTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.RoadChatter, Is.Not.Null);

            Assert.IsTrue(_gm.NewSessionReset.Resettables.Contains(_gm.RoadChatter));
            Assert.AreSame(_gm.RoadChatter, RoadChatterState.ResolveShared());

            _gm.RoadChatter.MarkSeen("maria_icaro_crackle");
            _gm.SaveManager.Save(TestSlot);
            Assert.IsTrue(_gm.SaveManager.PeekSlot(TestSlot).entries.Any(e => e.key == RoadChatterState.Key), "registered with SaveManager");

            _gm.GameFlowService.EnterFrontEnd();
            Assert.IsTrue(_gm.GameFlowService.StartNewGame());
            Assert.IsFalse(_gm.RoadChatter.HasSeen("maria_icaro_crackle"), "Novo jogo starts clean");

            Assert.IsTrue(_gm.SaveManager.Load(TestSlot));
            Assert.IsTrue(_gm.RoadChatter.HasSeen("maria_icaro_crackle"));
        }

        // ------------------------------------------------------------------
        // Bubble texts
        // ------------------------------------------------------------------

        [Test]
        public void BubbleLines_LabelTheCousinOnTheRadioOrRemembered()
        {
            RoadChatterSpec radio = RoadChatterCatalog.Find("maithe_icaro_radio_test");
            Assert.AreEqual("Radinho de pilha", RoadChatterBubble.BuildHeader(radio, null));
            Assert.AreEqual("Ícaro: Testando, testando... Maithe, tá me ouvindo? Câmbio!", RoadChatterBubble.BuildLine(radio, 0, CharacterId.Icaro, null));
            StringAssert.StartsWith("Maithe (no rádio): ", RoadChatterBubble.BuildLine(radio, 1, CharacterId.Icaro, null));
            StringAssert.StartsWith("Ícaro (no rádio): ", RoadChatterBubble.BuildLine(radio, 0, CharacterId.Maithe, null));

            RoadChatterSpec memory = RoadChatterCatalog.Find("maria_joaquim_spinning_top");
            Assert.AreEqual("Lembrança da estrada", RoadChatterBubble.BuildHeader(memory, null));
            StringAssert.StartsWith("Maria (lembrança): ", RoadChatterBubble.BuildLine(memory, 1, CharacterId.Joaquim, null));

            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Battery radio", RoadChatterBubble.BuildHeader(radio, manager));
            StringAssert.StartsWith("Maithe (on the radio): Loud and clear.", RoadChatterBubble.BuildLine(radio, 1, CharacterId.Icaro, manager));
        }

        [Test]
        public void EveryLineKey_IsInTheTextTable()
        {
            Dictionary<string, LocalizationEntry> table = RoadChatterTexts.Entries.ToDictionary(e => e.Key);
            foreach (RoadChatterSpec spec in RoadChatterCatalog.All)
            {
                Assert.IsTrue(table.ContainsKey(spec.LineKey(0)), spec.Id);
                Assert.IsTrue(table.ContainsKey(spec.LineKey(1)), spec.Id);
            }
        }

        // ------------------------------------------------------------------
        // Connections between systems (Docs/Narrative.md)
        // ------------------------------------------------------------------

        [Test]
        public void RegionNpcs_RememberTheBoss_OnceTheRegionIsCleared()
        {
            var quests = new QuestManager();
            quests.Initialize();
            var dialogue = new DialogueManager();
            string hero = null;
            dialogue.Context = new QuestDialogueContext(() => quests, RegionQuestCatalog.GetQuest, () => hero);
            dialogue.Initialize();
            var localization = new LocalizationManager();
            localization.Initialize();
            RegionQuestTexts.EnsureLoaded(localization);

            try
            {
                foreach (RegionNpcSpec npc in RegionQuestCatalog.Npcs)
                {
                    foreach (CharacterId cousin in Heroes)
                    {
                        hero = cousin.ToString();
                        RegionNpcDialogues.ApplyRegionFlags(dialogue, false, true);
                        dialogue.StartDialogue(RegionNpcDialogues.Get(npc.Region));
                        Assert.AreEqual(RegionNpcDialogues.GreetAfterNodeId(cousin), dialogue.CurrentNode.Id, npc.NpcKey);
                        Assert.IsTrue(localization.HasKey(dialogue.CurrentNode.TextKey), dialogue.CurrentNode.TextKey);
                        Assert.IsTrue(dialogue.AvailableChoices.Any(c => c.TextKey == RegionNpcDialogues.ChoiceGoodbyeKey));
                        dialogue.EndDialogue();

                        RegionNpcDialogues.ApplyRegionFlags(dialogue, false, false);
                        dialogue.StartDialogue(RegionNpcDialogues.Get(npc.Region));
                        Assert.AreEqual(RegionNpcDialogues.GreetNodeId(cousin), dialogue.CurrentNode.Id, npc.NpcKey + " before the boss");
                        dialogue.EndDialogue();
                    }

                    hero = null;
                    RegionNpcDialogues.ApplyRegionFlags(dialogue, false, true);
                    dialogue.StartDialogue(RegionNpcDialogues.Get(npc.Region));
                    Assert.AreEqual(RegionNpcDialogues.GreetAfterAnyNodeId, dialogue.CurrentNode.Id, npc.NpcKey);
                    dialogue.EndDialogue();
                }
            }
            finally
            {
                dialogue.Shutdown();
                quests.Shutdown();
            }
        }

        [Test]
        public void Lore_RespectsTheFamilyTree()
        {
            string maria = CharacterRegistry.Get(CharacterId.Maria).Lore;
            StringAssert.Contains("prima mais velha Maithe", maria);
            StringAssert.DoesNotContain("irmã mais velha Maithe", maria);

            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            StringAssert.Contains("eldest cousin Maithe", CharacterRegistry.Get(CharacterId.Maria).LocalizedLore(manager));
            StringAssert.Contains("Noite Rubra", CharacterRegistry.Get(CharacterId.Lavine).LocalizedLore(manager));

            // Joaquim never calls his older sister "maninha" (QuestGiverTexts / the convergence dialogue).
            foreach (LocalizationEntry entry in QuestGiverTexts.All.Concat(LocalizationManager.DefaultEntries.All))
            {
                if (entry.Key.StartsWith("dialogue.maria_gathering_point.") || entry.Key.StartsWith("dialogue.convergence_first_meeting.joaquim"))
                {
                    StringAssert.DoesNotContain("Maninha", entry.PtBR, entry.Key);
                    StringAssert.DoesNotContain("Little sister", entry.En, entry.Key);
                }
            }
        }

        private static RoadChatterSpec Spec(string id, RoadChatterStage min, RoadChatterStage max)
        {
            return new RoadChatterSpec(id, CharacterId.Maithe, CharacterId.Icaro, RoadChatterKind.Radio, min, max, 0,
                new RoadChatterLine("a", "a"), new RoadChatterLine("b", "b"));
        }
    }
}
