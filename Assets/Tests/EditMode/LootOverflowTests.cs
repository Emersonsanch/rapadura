using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Encounter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Full-bag rules (Docs/Balance/README.md §9 and "Mochila em grade"). History: iron ore weighed 3 against a 100 cap
    /// and the bag filled in ~18 Montanha fights; then weights were lowered and quest items went past the weight cap.
    /// Since the MU-style grid bag the 8×8 grid is the only limit (weight is dormant data, still checked below so it
    /// stays sane), nothing goes past the grid, and what is really lost raises <see cref="LootOverflowEvent"/> ("Mochila cheia").
    /// Also covers the quest-side fixes that came with the reachability audit: a boss killed before its quest was
    /// taken is credited, and a saved quest whose definition changed is brought up to date.
    /// </summary>
    public class LootOverflowTests
    {
        /// <summary>Average enemies per random encounter (WorldMap.unity: 1-2), as in EnemyLootTableTests.</summary>
        private const float EnemiesPerFight = 1.5f;

        /// <summary>Fewest random fights in one biome, without selling/crafting, before biome materials fill the 8×8 grid.</summary>
        private const float MinFightsToFillTheBag = 40f;

        private readonly List<Object> _created = new List<Object>();
        private GameObject _owner;
        private InventoryManager _inventory;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            BiomeRegistry.Invalidate();
            _owner = new GameObject("LootOverflowTests_Owner");
            _inventory = _owner.AddComponent<InventoryManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_owner);
            foreach (Object obj in _created.Where(o => o != null))
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            EventBus.Clear();
            ItemDatabase.Invalidate();
            BiomeRegistry.Invalidate();
        }

        // ------------------------------------------------------------------
        // Content
        // ------------------------------------------------------------------

        [Test]
        public void Currency_IsWeightless()
        {
            ItemDefinition[] currencies = ItemDatabase.GetAll().Where(i => i != null && i.Type == ItemType.Currency).ToArray();
            CollectionAssert.IsNotEmpty(currencies);
            foreach (ItemDefinition currency in currencies)
            {
                Assert.AreEqual(0f, currency.Weight, $"{currency.ItemId} must weigh 0: gold can never be what fills the bag.");
            }

            Assert.AreEqual(ItemType.Currency, ItemDatabase.GetById("item_gold").Type);
        }

        [Test]
        public void RawMaterials_AreLighterThanGear()
        {
            Assert.AreEqual(1f, ItemDatabase.GetById("item_iron_ore").Weight);
            Assert.AreEqual(1.5f, ItemDatabase.GetById("item_iron").Weight);
            Assert.AreEqual(1f, ItemDatabase.GetById("item_stone").Weight);
            Assert.AreEqual(0.5f, ItemDatabase.GetById("item_wood").Weight);

            // Smelting compresses: an ingot weighs less than the 2 ore + 1 wood it is made of.
            Assert.Less(ItemDatabase.GetById("item_iron").Weight, 2f * ItemDatabase.GetById("item_iron_ore").Weight + ItemDatabase.GetById("item_wood").Weight);
        }

        [Test]
        public void EveryBiome_TakesAtLeast40Fights_ToFillTheGrid()
        {
            // Biome materials stack (50-99 per cell, 1×1): cells used per fight = expected units / stack size.
            float cap = _inventory.CellCount;
            foreach (BiomeDefinition biome in BiomeRegistry.GetAll())
            {
                List<BiomeEnemySpawnEntry> commons = biome.Enemies.Where(e => e?.Enemy != null && e.Weight > 0 && !e.Enemy.IsBoss).ToList();
                int totalWeight = commons.Sum(e => e.Weight);
                if (totalWeight <= 0)
                {
                    continue;
                }

                float perKill = 0f;
                foreach (BiomeEnemySpawnEntry entry in commons)
                {
                    float cells = 0f;
                    foreach (BiomeLootEntry loot in biome.Loot.Where(l => l?.Item != null && l.Weight > 0))
                    {
                        GridSize size = _inventory.GetFootprint(loot.Item);
                        cells += size.Area * EnemyLootRoller.ExpectedBiomeQuantity(entry.Enemy, biome, loot.Item.ItemId) / loot.Item.MaxStack;
                    }

                    perKill += (float)entry.Weight / totalWeight * cells;
                }

                if (perKill <= 0f)
                {
                    continue;
                }

                float fights = cap / (perKill * EnemiesPerFight);
                Assert.GreaterOrEqual(fights, MinFightsToFillTheBag,
                    $"{biome.Type}: {perKill * EnemiesPerFight:0.00} cells per fight fill the {cap}-cell grid in ~{fights:0} fights.");
            }
        }

        // ------------------------------------------------------------------
        // Grant rules
        // ------------------------------------------------------------------

        [Test]
        public void Grant_FitsNormally_WhenThereIsRoom()
        {
            ItemDefinition ore = ItemDatabase.GetById("item_iron_ore");
            LootGrantResult result = LootGrantRules.Grant(_inventory, new LootDrop(ore, 2), null);

            Assert.AreEqual(2, result.Granted);
            Assert.AreEqual(0, result.Lost);
            Assert.AreEqual(0, result.KeptOverWeight);
            Assert.AreEqual(2, _inventory.GetTotalCount("item_iron_ore"));
        }

        [Test]
        public void Grant_WithNoRoomOnTheGrid_ReportsTheLoss()
        {
            SetGrid(_inventory, 1, 1);
            ItemDefinition ore = ItemDatabase.GetById("item_iron_ore");
            Assert.AreEqual(0, _inventory.AddItem(ore, ore.MaxStack), "one full stack fills the only cell");

            LootGrantResult result = LootGrantRules.Grant(_inventory, new LootDrop(ore, 2), (id, held) => 0);

            Assert.AreEqual(0, result.Granted);
            Assert.AreEqual(2, result.Lost);
            Assert.IsTrue(result.AnyLost);
            Assert.AreEqual(ore.MaxStack, _inventory.GetTotalCount("item_iron_ore"));
        }

        [Test]
        public void Grant_QuestNeeds_CannotBypassTheGrid()
        {
            // The weight-era exception (quest units went in past the weight cap) is gone: a full grid is full.
            SetGrid(_inventory, 1, 1);
            ItemDefinition ore = ItemDatabase.GetById("item_iron_ore");
            _inventory.AddItem(ore, ore.MaxStack);

            LootGrantResult result = LootGrantRules.Grant(_inventory, new LootDrop(ore, 2), (id, held) => id == "item_iron_ore" ? 999 : 0);

            Assert.AreEqual(0, result.Granted);
            Assert.AreEqual(0, result.KeptOverWeight, "dormant field");
            Assert.AreEqual(2, result.Lost);
            Assert.AreEqual(0, _inventory.FreeCellCount);
        }

        [Test]
        public void Grant_Currency_StacksIntoItsCell_EvenWhenTheGridIsFull()
        {
            SetGrid(_inventory, 2, 1);
            _inventory.AddItem(ItemDatabase.GetById("item_iron_ore"), 1);

            LootGrantResult first = LootGrantRules.Grant(_inventory, new LootDrop(ItemDatabase.GetById("item_gold"), 3), null);
            Assert.AreEqual(3, first.Granted);
            Assert.AreEqual(0, _inventory.FreeCellCount, "gold took the last cell");

            LootGrantResult more = LootGrantRules.Grant(_inventory, new LootDrop(ItemDatabase.GetById("item_gold"), 5), null);
            Assert.AreEqual(5, more.Granted, "gold stacks (9999 per cell): it never needs a second cell here");
            Assert.AreEqual(0, more.Lost);
            Assert.AreEqual(8, _inventory.GetTotalCount("item_gold"));
        }

        [Test]
        public void Weight_IsDormant_HeavyLoadsStillFit()
        {
            // 10 full stacks of stone weigh 990 (the old cap was 100) but take 10 of 64 cells.
            ItemDefinition stone = ItemDatabase.GetById("item_stone");
            Assert.AreEqual(0, _inventory.AddItem(stone, stone.MaxStack * 10));
            Assert.Greater(_inventory.GetTotalWeight(), 100f);
            Assert.AreEqual(10, _inventory.UsedCellCount);
            Assert.IsTrue(_inventory.HasWeightCapacityFor(stone, 1), "legacy name = grid fit");
        }

        [Test]
        public void Grant_GeneratedGear_WithoutAFreeSlot_IsLost_NotThrown()
        {
            SetGrid(_inventory, 1, 1);
            _inventory.AddItem(ItemDatabase.GetById("item_herb"), 1);
            ItemDefinition sword = ItemDatabase.GetById("item_iron_sword");
            var generated = new GeneratedItemInstance(sword, ItemRarity.Rare, Array.Empty<RolledAffix>(), "Espada de Ferro Rara");

            LootGrantResult result = LootGrantRules.Grant(_inventory, new LootDrop(generated), null);

            Assert.AreEqual(0, result.Granted);
            Assert.AreEqual(1, result.Lost);
        }

        [Test]
        public void ProtectedAmount_IsCappedByTheLeftover_AndZeroForUnwantedMaterials()
        {
            ItemDefinition ore = ItemDatabase.GetById("item_iron_ore");
            Assert.AreEqual(0, LootGrantRules.ProtectedAmount(ore, 2, 0, null));
            Assert.AreEqual(2, LootGrantRules.ProtectedAmount(ore, 2, 0, (id, held) => 10));
            Assert.AreEqual(1, LootGrantRules.ProtectedAmount(ore, 2, 0, (id, held) => 1));
            Assert.AreEqual(5, LootGrantRules.ProtectedAmount(ItemDatabase.GetById("item_gold"), 5, 0, null));
        }

        [Test]
        public void QuestManager_OutstandingCollectAmount_IsTheLargestNeedMinusWhatIsHeld()
        {
            var quests = new QuestManager();
            quests.Initialize();
            try
            {
                quests.StartQuest(MakeQuest("q_a", Objective("ore", QuestObjectiveType.CollectItem, "item_iron_ore", 4)));
                quests.StartQuest(MakeQuest("q_b", Objective("ore", QuestObjectiveType.CollectItem, "item_iron_ore", 2),
                    Objective("wolf", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 1)));

                Assert.AreEqual(4, quests.GetOutstandingCollectAmount("item_iron_ore", 0));
                Assert.AreEqual(1, quests.GetOutstandingCollectAmount("item_iron_ore", 3));
                Assert.AreEqual(0, quests.GetOutstandingCollectAmount("item_iron_ore", 9));
                Assert.AreEqual(0, quests.GetOutstandingCollectAmount("item_wood", 0));
            }
            finally
            {
                quests.Shutdown();
            }
        }

        [Test]
        public void BagFullToast_DescribesLostAmounts()
        {
            Assert.AreEqual("Espada de Ferro", BagFullToastController.DescribeAmount("Espada de Ferro", 1));
            Assert.AreEqual("2 Minério de Ferro", BagFullToastController.DescribeAmount("Minério de Ferro", 2));
            Assert.AreEqual("2 Minério de Ferro, Pedra", BagFullToastController.JoinLost(new[]
            {
                new KeyValuePair<string, int>("Minério de Ferro", 2),
                new KeyValuePair<string, int>("Pedra", 1),
                new KeyValuePair<string, int>("Nada", 0),
            }));
        }

        // ------------------------------------------------------------------
        // Quest fixes from the reachability audit
        // ------------------------------------------------------------------

        [Test]
        public void StartQuest_CreditsABossThatWasAlreadyDefeated()
        {
            var quests = new QuestManager { DefeatedUniqueEnemyLookup = id => id == "enemy_mountain_warlord" };
            quests.Initialize();
            try
            {
                QuestDefinition quest = MakeQuest("q_boss",
                    Objective("warlord", QuestObjectiveType.KillEnemyType, "enemy_mountain_warlord", 1),
                    Objective("golems", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 2));
                Assert.IsTrue(quests.StartQuest(quest));

                QuestProgress progress = quests.GetProgress("q_boss");
                Assert.IsTrue(progress.Objectives[0].IsComplete, "The warlord fell before the quest: it never respawns, so it counts now.");
                Assert.AreEqual(0, progress.Objectives[1].CurrentAmount, "Common enemies are never pre-credited.");
            }
            finally
            {
                quests.Shutdown();
            }
        }

        [Test]
        public void StartQuest_CompletesAtOnce_WhenTheOnlyObjectiveIsAnAlreadyDefeatedBoss()
        {
            var quests = new QuestManager { DefeatedUniqueEnemyLookup = id => id == "enemy_desert_ruin_guardian" };
            quests.Initialize();
            try
            {
                Assert.IsTrue(quests.StartQuest(MakeQuest("q_guardian", Objective("guardian", QuestObjectiveType.KillEnemyType, "enemy_desert_ruin_guardian", 1))));
                Assert.IsTrue(quests.IsCompleted("q_guardian"));
            }
            finally
            {
                quests.Shutdown();
            }
        }

        [Test]
        public void RestoredQuest_IsBroughtUpToItsCurrentDefinition()
        {
            var quests = new QuestManager { DefeatedUniqueEnemyLookup = id => false };
            quests.Initialize();
            try
            {
                // An old save: Joaquim's quest as it used to be (6 kills of the Caves boss), with the talk done.
                var saved = new QuestSaveState();
                saved.ActiveQuests.Add(new QuestProgress
                {
                    QuestId = "q_old",
                    Objectives = new List<QuestObjective>
                    {
                        new QuestObjective { ObjectiveId = "clear", Type = QuestObjectiveType.KillEnemyType, TargetId = "enemy_cave_dweller", RequiredAmount = 6, CurrentAmount = 0 },
                        new QuestObjective { ObjectiveId = "talk", Type = QuestObjectiveType.TalkToNpc, TargetId = "npc_village_survivor", RequiredAmount = 1, CurrentAmount = 1 },
                    }
                });
                quests.RestoreState(saved);

                quests.RegisterDefinition(MakeQuest("q_old",
                    Objective("clear", QuestObjectiveType.KillEnemyType, "enemy_forest_spider", 5),
                    Objective("talk", QuestObjectiveType.TalkToNpc, "npc_village_survivor", 1)));

                QuestProgress progress = quests.GetProgress("q_old");
                Assert.AreEqual("enemy_forest_spider", progress.Objectives[0].TargetId);
                Assert.AreEqual(5, progress.Objectives[0].RequiredAmount);
                Assert.AreEqual(0, progress.Objectives[0].CurrentAmount);
                Assert.IsTrue(progress.Objectives[1].IsComplete, "Progress on an unchanged objective is kept.");

                for (int i = 0; i < 5; i++)
                {
                    quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, "enemy_forest_spider", 1);
                }

                Assert.IsTrue(quests.IsCompleted("q_old"));
            }
            finally
            {
                quests.Shutdown();
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private QuestDefinition MakeQuest(string id, params QuestObjective[] objectives)
        {
            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.SetDataForTests(id, id + ".title", id + ".description", QuestType.Side, objectives.ToList());
            _created.Add(quest);
            return quest;
        }

        private static QuestObjective Objective(string id, QuestObjectiveType type, string target, int amount)
        {
            return new QuestObjective { ObjectiveId = id, Type = type, TargetId = target, RequiredAmount = amount, DescriptionKey = "obj." + id };
        }

        /// <summary>Resizes a fresh bag's grid (before anything was added).</summary>
        private static void SetGrid(InventoryManager inventory, int columns, int rows)
        {
            SetPrivateField(inventory, "_columns", columns);
            SetPrivateField(inventory, "_rows", rows);
            SetPrivateField(inventory, "_slots", null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, fieldName);
            field.SetValue(target, value);
        }
    }
}
