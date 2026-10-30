using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The region side quests (<see cref="RegionQuestCatalog"/>), their quest-giver conversations
    /// (<see cref="RegionNpcDialogues"/>), texts (<see cref="RegionQuestTexts"/>) and the world-map talk
    /// button rules (<see cref="RegionNpcTalkButton"/>); plus the regression for rewards of quests completed
    /// while no Player exists (world map), queued by <see cref="QuestManager"/> until one does.
    /// Kill targets are checked against the real biome spawn tables — the TODO.md history has two whole
    /// objective types that never progressed because of id prefix mismatches.
    /// </summary>
    public class RegionQuestContentTests
    {
        private static readonly BiomeType[] Regions = (BiomeType[])Enum.GetValues(typeof(BiomeType));

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        private QuestDefinition BuildQuest(string questId)
        {
            return Track(RegionQuestCatalog.Build(RegionQuestCatalog.FindSpec(questId), ItemDatabase.GetById));
        }

        private static LocalizationManager LoadedManager()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            RegionQuestTexts.EnsureLoaded(manager);
            return manager;
        }

        // ------------------------------------------------------------------
        // Quest definitions
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_HasTwoChainedSideQuestsAndOneGiverPerRegion()
        {
            foreach (BiomeType region in Regions)
            {
                List<RegionQuestSpec> quests = RegionQuestCatalog.QuestsFor(region).ToList();
                Assert.AreEqual(2, quests.Count, $"{region} should have 2 side quests.");

                RegionNpcSpec npc = RegionQuestCatalog.NpcFor(region);
                Assert.IsNotNull(npc, $"{region} has no quest-giver.");
                Assert.AreEqual(quests[0].QuestId, npc.FirstQuestId);
                Assert.AreEqual(quests[1].QuestId, npc.SecondQuestId);
                Assert.IsTrue(string.IsNullOrEmpty(quests[0].PrerequisiteQuestId), $"{quests[0].QuestId} should open with the region.");
                Assert.AreEqual(quests[0].QuestId, quests[1].PrerequisiteQuestId, $"{quests[1].QuestId} should require the first quest.");
            }

            Assert.AreEqual(RegionQuestCatalog.Quests.Count, RegionQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count(), "Duplicate quest id.");
        }

        [Test]
        public void QuestIds_DoNotCollideWithResourcesQuests()
        {
            HashSet<string> assetIds = new HashSet<string>(Resources.LoadAll<QuestDefinition>("Quests").Select(q => q.QuestId));
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                Assert.IsFalse(assetIds.Contains(spec.QuestId), $"{spec.QuestId} also exists as a Resources asset.");
                StringAssert.StartsWith("quest_region_", spec.QuestId);
            }
        }

        [Test]
        public void KillObjectives_TargetNonBossEnemiesOfTheirRegionByAssetName()
        {
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                BiomeDefinition biome = BiomeRegistry.Get(spec.Region);
                Assert.IsNotNull(biome, $"No BiomeDefinition for {spec.Region}.");

                foreach (RegionObjectiveSpec objective in spec.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    StringAssert.StartsWith("enemy_", objective.TargetId, $"{spec.QuestId}/{objective.ObjectiveId}: QuestManager matches EnemyDefinition asset names.");
                    BiomeEnemySpawnEntry entry = biome.Enemies.FirstOrDefault(e => e != null && e.Enemy != null && e.Enemy.name == objective.TargetId);
                    Assert.IsNotNull(entry, $"{spec.QuestId}/{objective.ObjectiveId}: '{objective.TargetId}' never spawns in {spec.Region}.");
                    Assert.Greater(entry.Weight, 0, $"{objective.TargetId} has spawn weight 0 in {spec.Region}.");
                    Assert.IsFalse(entry.Enemy.IsBoss, $"{spec.QuestId}: '{objective.TargetId}' is a boss (fought once; the quest could become impossible).");
                }
            }
        }

        [Test]
        public void CollectObjectives_TargetItemsDroppedByTheRegionsCommonEnemies()
        {
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                BiomeDefinition biome = BiomeRegistry.Get(spec.Region);
                foreach (RegionObjectiveSpec objective in spec.Objectives.Where(o => o.Type == QuestObjectiveType.CollectItem))
                {
                    Assert.IsNotNull(ItemDatabase.GetById(objective.TargetId), $"{spec.QuestId}: item '{objective.TargetId}' doesn't exist.");
                    bool dropped = biome.Enemies.Any(e => e != null && e.Enemy != null && !e.Enemy.IsBoss && e.Weight > 0
                                                          && e.Enemy.LootItem != null && e.Enemy.LootItem.ItemId == objective.TargetId);
                    Assert.IsTrue(dropped, $"{spec.QuestId}: no common {spec.Region} enemy drops '{objective.TargetId}'.");
                }
            }
        }

        [Test]
        public void Objectives_OnlyProgressInCombat_AndHaveSaneAmounts()
        {
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                Assert.IsNotEmpty(spec.Objectives, spec.QuestId);
                foreach (RegionObjectiveSpec objective in spec.Objectives)
                {
                    Assert.That(objective.Type, Is.EqualTo(QuestObjectiveType.KillEnemyType).Or.EqualTo(QuestObjectiveType.CollectItem),
                        $"{spec.QuestId}/{objective.ObjectiveId}: only combat-driven objectives (rewards land where the Player is live).");
                    Assert.That(objective.RequiredAmount, Is.InRange(1, 30), $"{spec.QuestId}/{objective.ObjectiveId}");
                }

                Assert.AreEqual(spec.Objectives.Count, spec.Objectives.Select(o => o.ObjectiveId).Distinct().Count(), $"{spec.QuestId}: duplicate objective id.");
            }
        }

        [Test]
        public void BuiltDefinitions_AreSideQuestsWithExistingRewards()
        {
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                QuestDefinition quest = BuildQuest(spec.QuestId);

                Assert.AreEqual(spec.QuestId, quest.QuestId);
                Assert.AreEqual(QuestType.Side, quest.QuestType);
                Assert.Greater(quest.RewardExperience, 0, spec.QuestId);
                Assert.IsNotEmpty(quest.RewardItems, spec.QuestId);
                foreach (QuestItemReward reward in quest.RewardItems)
                {
                    Assert.IsNotNull(reward.Item, $"{spec.QuestId}: reward item missing from Resources/Items.");
                    Assert.Greater(reward.Quantity, 0, spec.QuestId);
                }

                Assert.AreEqual(spec.Objectives.Count, quest.Objectives.Count);
                Assert.IsTrue(quest.Objectives.All(o => o.DescriptionKey == spec.ObjectiveKey(o.ObjectiveId)));
            }
        }

        [Test]
        public void GetQuest_IsCachedAndUnknownIdsReturnNull()
        {
            Assert.AreSame(RegionQuestCatalog.GetQuest(RegionQuestCatalog.ForestFirst), RegionQuestCatalog.GetQuest(RegionQuestCatalog.ForestFirst));
            Assert.IsNull(RegionQuestCatalog.GetQuest("quest_maithe_windtrail"));
            Assert.IsNull(RegionQuestCatalog.GetQuest(null));
        }

        [Test]
        public void QuestDialogueContext_DefaultResolver_FindsRegionQuests()
        {
            var quests = new QuestManager();
            quests.Initialize();
            try
            {
                var context = new QuestDialogueContext(() => quests, null, () => null);
                Assert.AreEqual(DialogueQuestState.Available, context.GetQuestState(RegionQuestCatalog.ForestFirst));
                Assert.AreEqual(DialogueQuestState.NotStarted, context.GetQuestState(RegionQuestCatalog.ForestSecond), "Second quest waits for the first.");
                Assert.IsTrue(context.StartQuest(RegionQuestCatalog.ForestFirst));
                Assert.IsTrue(quests.IsActive(RegionQuestCatalog.ForestFirst));
            }
            finally
            {
                quests.Shutdown();
            }
        }

        // ------------------------------------------------------------------
        // Localization
        // ------------------------------------------------------------------

        [Test]
        public void QuestKeys_AllLocalizedInBothLanguages()
        {
            Dictionary<string, LocalizationEntry> entries = RegionQuestTexts.All.ToDictionary(e => e.Key);
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                var keys = new List<string> { spec.TitleKey, spec.DescriptionKey, spec.JournalKey };
                keys.AddRange(spec.Objectives.Select(o => spec.ObjectiveKey(o.ObjectiveId)));
                foreach (string key in keys)
                {
                    Assert.IsTrue(entries.TryGetValue(key, out LocalizationEntry entry), $"Missing key '{key}'.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"'{key}' has no en text.");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"'{key}' has no pt-BR text.");
                }
            }
        }

        [Test]
        public void Dialogues_HaveValidLinksAndLocalizedText()
        {
            LocalizationManager localization = LoadedManager();

            foreach (RegionNpcSpec npc in RegionQuestCatalog.Npcs)
            {
                DialogueDefinition dialogue = Track(RegionNpcDialogues.Build(npc));
                Assert.AreEqual(npc.DialogueId, dialogue.DialogueId);
                Assert.IsNotNull(dialogue.GetStartNode());
                Assert.IsTrue(localization.HasKey(npc.KeyBase + ".short_name"), $"{npc.NpcKey}: no short name.");

                foreach (DialogueNode node in dialogue.Nodes)
                {
                    string where = $"{npc.DialogueId}/{node.Id}";
                    Assert.IsTrue(localization.HasKey(node.SpeakerNameKey), $"{where}: speaker name '{node.SpeakerNameKey}'.");

                    foreach (DialogueRedirect redirect in node.Redirects)
                    {
                        Assert.IsNotNull(dialogue.GetNode(redirect.TargetNodeId), $"{where}: redirect to missing '{redirect.TargetNodeId}'.");
                    }

                    if (node.Id == RegionNpcDialogues.HubNodeId)
                    {
                        Assert.IsTrue(node.Redirects.Any(r => r.Conditions.Count == 0), $"{where}: hub needs an unconditional fallback.");
                        continue;
                    }

                    Assert.IsTrue(localization.HasKey(node.TextKey), $"{where}: line '{node.TextKey}'.");
                    Assert.IsFalse(node.IsTerminal, $"{where}: a shown node needs a choice or auto-advance (the box has no close button otherwise).");

                    if (!string.IsNullOrEmpty(node.AutoAdvanceToNodeId))
                    {
                        Assert.IsNotNull(dialogue.GetNode(node.AutoAdvanceToNodeId), $"{where}: auto-advance to missing node.");
                    }

                    foreach (DialogueChoice choice in node.Choices)
                    {
                        Assert.IsTrue(localization.HasKey(choice.TextKey), $"{where}: choice '{choice.TextKey}'.");
                        Assert.IsTrue(string.IsNullOrEmpty(choice.NextNodeId) || dialogue.GetNode(choice.NextNodeId) != null, $"{where}: choice to missing '{choice.NextNodeId}'.");
                    }
                }

                HashSet<string> started = new HashSet<string>(dialogue.Nodes.SelectMany(n => n.Choices).SelectMany(c => c.Actions)
                    .Where(a => a.Type == DialogueActionType.StartQuest).Select(a => a.Key));
                CollectionAssert.AreEquivalent(new[] { npc.FirstQuestId, npc.SecondQuestId }, started, $"{npc.DialogueId} must offer exactly its two quests.");
            }
        }

        // ------------------------------------------------------------------
        // Conversation flow (real QuestManager, hero conditions)
        // ------------------------------------------------------------------

        private sealed class Flow : IDisposable
        {
            public readonly QuestManager Quests = new QuestManager();
            public readonly DialogueManager Dialogue = new DialogueManager();
            public string Hero;

            public Flow()
            {
                Quests.Initialize();
                Dialogue.Context = new QuestDialogueContext(() => Quests, RegionQuestCatalog.GetQuest, () => Hero);
                Dialogue.Initialize();
            }

            public void Start(BiomeType region, bool bossReady = false, bool cleared = false)
            {
                RegionNpcDialogues.ApplyRegionFlags(Dialogue, bossReady, cleared);
                Dialogue.StartDialogue(RegionNpcDialogues.Get(region));
            }

            public List<string> ChoiceKeys() => Dialogue.AvailableChoices.Select(c => c.TextKey).ToList();

            public void Pick(string textKey)
            {
                int index = Dialogue.AvailableChoices.ToList().FindIndex(c => c.TextKey == textKey);
                Assert.GreaterOrEqual(index, 0, $"Choice '{textKey}' not offered on '{Dialogue.CurrentNode.Id}'. Offered: {string.Join(", ", ChoiceKeys())}");
                Dialogue.SelectChoice(index);
            }

            public void Dispose()
            {
                Dialogue.Shutdown();
                Quests.Shutdown();
            }
        }

        [Test]
        public void Greeting_IsPickedByTheActiveHero()
        {
            using var flow = new Flow();
            foreach (CharacterId hero in Enum.GetValues(typeof(CharacterId)))
            {
                flow.Hero = hero.ToString();
                flow.Start(BiomeType.Forest);
                Assert.AreEqual(RegionNpcDialogues.GreetNodeId(hero), flow.Dialogue.CurrentNode.Id);
                flow.Dialogue.EndDialogue();
            }

            flow.Hero = null;
            flow.Start(BiomeType.Forest);
            Assert.AreEqual(RegionNpcDialogues.GreetAnyNodeId, flow.Dialogue.CurrentNode.Id);
        }

        [Test]
        public void FirstQuestOffered_SecondOnlyAfterFirstCompletes()
        {
            using var flow = new Flow { Hero = "Maria" };
            RegionNpcSpec npc = RegionQuestCatalog.NpcFor(BiomeType.Forest);

            flow.Start(BiomeType.Forest);
            CollectionAssert.Contains(flow.ChoiceKeys(), npc.KeyBase + ".choice_q1");
            CollectionAssert.DoesNotContain(flow.ChoiceKeys(), npc.KeyBase + ".choice_q2");

            flow.Pick(npc.KeyBase + ".choice_q1");
            flow.Pick(RegionNpcDialogues.ChoiceAcceptKey);
            Assert.IsTrue(flow.Quests.IsActive(npc.FirstQuestId));
            Assert.AreEqual("q1_accepted", flow.Dialogue.CurrentNode.Id);

            flow.Dialogue.AdvanceCurrentLine();
            Assert.AreEqual(RegionNpcDialogues.MenuNodeId, flow.Dialogue.CurrentNode.Id);
            CollectionAssert.DoesNotContain(flow.ChoiceKeys(), npc.KeyBase + ".choice_q1");
            CollectionAssert.Contains(flow.ChoiceKeys(), RegionNpcDialogues.ChoiceProgressKey);
            flow.Dialogue.EndDialogue();

            // The kill objective progresses with the exact id QuestManager resolves for a dead enemy.
            flow.Quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 5);
            Assert.IsTrue(flow.Quests.IsCompleted(npc.FirstQuestId));

            flow.Start(BiomeType.Forest);
            flow.Pick(npc.KeyBase + ".choice_q2");
            flow.Pick(RegionNpcDialogues.ChoiceAcceptKey);
            Assert.IsTrue(flow.Quests.IsActive(npc.SecondQuestId));
        }

        [Test]
        public void SpecialAcceptLine_OnlyForTheNpcsAcceptHero()
        {
            foreach (RegionNpcSpec npc in RegionQuestCatalog.Npcs)
            {
                foreach (CharacterId hero in Enum.GetValues(typeof(CharacterId)))
                {
                    using var flow = new Flow { Hero = hero.ToString() };
                    flow.Start(npc.Region);
                    flow.Pick(npc.KeyBase + ".choice_q1");

                    List<string> choices = flow.ChoiceKeys();
                    bool special = hero == npc.AcceptHero;
                    Assert.AreEqual(special, choices.Contains(npc.KeyBase + ".choice_accept_special"), $"{npc.NpcKey}/{hero}");
                    Assert.AreEqual(!special, choices.Contains(RegionNpcDialogues.ChoiceAcceptKey), $"{npc.NpcKey}/{hero}");
                    CollectionAssert.Contains(choices, RegionNpcDialogues.ChoiceDeclineKey);
                }
            }
        }

        [Test]
        public void Decline_DoesNotStartTheQuest()
        {
            using var flow = new Flow { Hero = "Joaquim" };
            RegionNpcSpec npc = RegionQuestCatalog.NpcFor(BiomeType.Mountain);
            flow.Start(BiomeType.Mountain);
            flow.Pick(npc.KeyBase + ".choice_q1");
            flow.Pick(RegionNpcDialogues.ChoiceDeclineKey);

            Assert.IsFalse(flow.Quests.IsActive(npc.FirstQuestId));
            Assert.AreEqual("declined", flow.Dialogue.CurrentNode.Id);
        }

        [Test]
        public void BossHint_OnlyWhileBossReady_WithSpecialLineForTheBossHintHero()
        {
            foreach (RegionNpcSpec npc in RegionQuestCatalog.Npcs)
            {
                using var flow = new Flow { Hero = "Lavine" == npc.BossHintHero.ToString() ? "Maria" : "Lavine" };
                flow.Start(npc.Region);
                CollectionAssert.DoesNotContain(flow.ChoiceKeys(), RegionNpcDialogues.ChoiceBossKey, npc.NpcKey);
                CollectionAssert.DoesNotContain(flow.ChoiceKeys(), RegionNpcDialogues.ChoiceClearedKey, npc.NpcKey);
                flow.Dialogue.EndDialogue();

                flow.Start(npc.Region, bossReady: true);
                flow.Pick(RegionNpcDialogues.ChoiceBossKey);
                Assert.AreEqual(RegionNpcDialogues.BossNodeId, flow.Dialogue.CurrentNode.Id, npc.NpcKey);
                flow.Dialogue.EndDialogue();

                flow.Hero = npc.BossHintHero.ToString();
                flow.Start(npc.Region, bossReady: true);
                flow.Pick(RegionNpcDialogues.ChoiceBossKey);
                Assert.AreEqual(RegionNpcDialogues.BossSpecialNodeId, flow.Dialogue.CurrentNode.Id, npc.NpcKey);
                flow.Dialogue.EndDialogue();

                flow.Start(npc.Region, cleared: true);
                flow.Pick(RegionNpcDialogues.ChoiceClearedKey);
                Assert.AreEqual("cleared", flow.Dialogue.CurrentNode.Id, npc.NpcKey);
            }
        }

        [Test]
        public void Goodbye_EndsTheConversation()
        {
            using var flow = new Flow { Hero = "Icaro" };
            flow.Start(BiomeType.Desert);
            flow.Pick(RegionNpcDialogues.ChoiceGoodbyeKey);
            Assert.IsFalse(flow.Dialogue.IsActive);
        }

        // ------------------------------------------------------------------
        // World-map talk button rules
        // ------------------------------------------------------------------

        [Test]
        public void TalkButton_ShowsOnlyWhenIdleInAnUnlockedRegionWithAGiver()
        {
            Assert.IsTrue(RegionNpcTalkButton.ShouldShow(true, true, false, false, true, true));
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(false, true, false, false, true, true), "HUD hidden");
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(true, false, false, false, true, true), "traveling");
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(true, true, true, false, true, true), "paused");
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(true, true, false, true, true, true), "dialogue open");
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(true, true, false, false, false, true), "locked region");
            Assert.IsFalse(RegionNpcTalkButton.ShouldShow(true, true, false, false, true, false), "no giver");
        }

        [Test]
        public void TalkButton_NewsMarker_ForOffersAndBoss()
        {
            Assert.IsTrue(RegionNpcTalkButton.HasNews(DialogueQuestState.Available, DialogueQuestState.NotStarted, false));
            Assert.IsFalse(RegionNpcTalkButton.HasNews(DialogueQuestState.Active, DialogueQuestState.NotStarted, false));
            Assert.IsTrue(RegionNpcTalkButton.HasNews(DialogueQuestState.Completed, DialogueQuestState.Available, false));
            Assert.IsFalse(RegionNpcTalkButton.HasNews(DialogueQuestState.Completed, DialogueQuestState.Completed, false));
            Assert.IsTrue(RegionNpcTalkButton.HasNews(DialogueQuestState.Completed, DialogueQuestState.Completed, true));
        }

        [Test]
        public void TalkButton_Label_UsesShortNameInBothLanguagesAndPtBRWithoutManager()
        {
            RegionNpcSpec npc = RegionQuestCatalog.NpcFor(BiomeType.Forest);
            Assert.AreEqual("Conversar com Mariam", RegionNpcTalkButton.BuildLabel(npc, false, null));
            Assert.AreEqual("Conversar com Mariam (!)", RegionNpcTalkButton.BuildLabel(npc, true, null));

            LocalizationManager localization = LoadedManager();
            localization.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Talk to Mariam", RegionNpcTalkButton.BuildLabel(npc, false, localization));
        }

        // ------------------------------------------------------------------
        // Regression: rewards of quests completed with no live Player (world map)
        // ------------------------------------------------------------------

        [Test]
        public void QuestCompletedWithoutPlayer_QueuesRewards_DeliveredOnceAPlayerExists()
        {
            var quests = new QuestManager();
            quests.Initialize();
            GameObject owner = Track(new GameObject("Player"));
            try
            {
                QuestDefinition quest = BuildQuest(RegionQuestCatalog.ForestFirst);
                Assert.IsTrue(quests.StartQuest(quest));

                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 5);

                Assert.IsTrue(quests.IsCompleted(quest.QuestId));
                CollectionAssert.AreEqual(new[] { quest.QuestId }, quests.PendingRewardQuestIds);

                var inventory = owner.AddComponent<InventoryManager>();
                quests.SetRewardTargets(inventory, null);
                quests.DeliverPendingRewards();

                Assert.AreEqual(2, inventory.GetTotalCount("item_health_potion"));
                CollectionAssert.IsEmpty(quests.PendingRewardQuestIds);

                quests.DeliverPendingRewards();
                Assert.AreEqual(2, inventory.GetTotalCount("item_health_potion"), "Rewards must be delivered only once.");
            }
            finally
            {
                quests.Shutdown();
            }
        }

        [Test]
        public void DestroyedPlayerFromThePreviousScene_CountsAsNoPlayer()
        {
            var quests = new QuestManager();
            quests.Initialize();
            try
            {
                var oldPlayer = new GameObject("OldPlayer");
                quests.SetRewardTargets(oldPlayer.AddComponent<InventoryManager>(), null);
                Object.DestroyImmediate(oldPlayer);

                Assert.IsFalse(quests.HasRewardTargets);
                QuestDefinition quest = BuildQuest(RegionQuestCatalog.ForestFirst);
                quests.StartQuest(quest);
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 5);

                CollectionAssert.Contains(quests.PendingRewardQuestIds, quest.QuestId);
            }
            finally
            {
                quests.Shutdown();
            }
        }

        [Test]
        public void PendingRewards_SurviveSaveAndLoad()
        {
            var quests = new QuestManager();
            quests.Initialize();
            var restored = new QuestManager();
            restored.Initialize();
            try
            {
                QuestDefinition quest = BuildQuest(RegionQuestCatalog.ForestFirst);
                quests.StartQuest(quest);
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 5);

                string json = JsonUtility.ToJson(quests.CaptureState());
                restored.RestoreState(JsonUtility.FromJson<QuestSaveState>(json));

                CollectionAssert.AreEqual(new[] { quest.QuestId }, restored.PendingRewardQuestIds);
                Assert.IsTrue(restored.IsCompleted(quest.QuestId));
            }
            finally
            {
                quests.Shutdown();
                restored.Shutdown();
            }
        }

        [Test]
        public void DeliverPendingRewards_EvaluatesCollectObjectivesAgainstTheNewInventory()
        {
            var quests = new QuestManager();
            quests.Initialize();
            GameObject owner = Track(new GameObject("Player"));
            try
            {
                QuestDefinition quest = BuildQuest(RegionQuestCatalog.DesertSecond);
                quests.RegisterDefinition(BuildQuest(RegionQuestCatalog.DesertFirst));
                quests.StartQuest(BuildQuest(RegionQuestCatalog.DesertFirst));
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 5);
                Assert.IsTrue(quests.StartQuest(quest), "Second quest starts once the first is done.");
                quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 3);

                var inventory = owner.AddComponent<InventoryManager>();
                inventory.AddItem(ItemDatabase.GetById("item_gold"), 12);
                quests.SetRewardTargets(inventory, null);
                Assert.IsTrue(quests.IsActive(quest.QuestId), "Gold held before the targets existed hasn't been counted yet.");

                quests.DeliverPendingRewards();

                Assert.IsTrue(quests.IsCompleted(quest.QuestId));
                Assert.AreEqual(1, inventory.GetTotalCount("item_leather_boots"));
            }
            finally
            {
                quests.Shutdown();
            }
        }
    }
}
