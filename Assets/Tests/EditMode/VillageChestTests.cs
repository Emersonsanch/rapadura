using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Managers;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Baú da Vó Zefa" (<see cref="VillageChestStorage"/>, <see cref="VillageChestTransfer"/>, opened from the map's
    /// village hub): per-save persistence (old saves included), the bulk buttons against the quests' needs, the
    /// bag's grid limit (the weight limit is gone), the 60-slot cap, the carried-inventory lifecycle and the new-session reset.
    /// </summary>
    public class VillageChestTests
    {
        private const int TestSlot = 90098;
        private const string Wood = "item_wood";
        private const string Fiber = "item_fiber";
        private const string Ore = "item_iron_ore";
        private const string Stone = "item_stone";
        private const string Potion = "item_health_potion";
        private const string Sword = "item_iron_sword";

        private readonly List<Object> _created = new List<Object>();
        private readonly List<QuestManager> _quests = new List<QuestManager>();
        private SaveManager _save;
        private GameObject _gameManagerObject;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
            _save = new SaveManager();
            _save.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (QuestManager quests in _quests)
            {
                quests.Shutdown();
            }

            _quests.Clear();

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
        // Save / load
        // ------------------------------------------------------------------

        [Test]
        public void SaveLoad_RoundTripsStacksAndGeneratedPieces()
        {
            var chest = new VillageChestStorage();
            _save.Register(chest);
            Assert.AreEqual(150, chest.Store(Plain(Wood, 150), 150), "no weight limit");
            InventorySlotData rolled = Generated(Sword);
            Assert.AreEqual(1, chest.Store(rolled, 1));

            _save.Save(TestSlot);
            Assert.IsTrue(_save.PeekSlot(TestSlot).entries.Any(e => e.key == VillageChestStorage.Key), "chest written to the slot");

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            var loadedChest = new VillageChestStorage();
            loadedSave.Register(loadedChest);
            Assert.IsTrue(loadedSave.Load(TestSlot));

            Assert.AreEqual(150, loadedChest.GetTotalCount(Wood));
            Assert.AreEqual(3, loadedChest.UsedSlotCount, "99 + 51 wood + the sword");
            InventorySlotData sword = loadedChest.Slots.Single(s => s.itemId == Sword);
            Assert.AreEqual("Espada do Teste", sword.generatedName);
            Assert.AreEqual(ItemRarity.Rare, sword.generatedRarity);
            CollectionAssert.AreEqual(rolled.generatedAffixIds, sword.generatedAffixIds);
            Assert.AreEqual(rolled.currentDurability, sword.currentDurability, 0.001f);
        }

        [Test]
        public void OldSave_WithoutAChest_LoadsAnEmptyChest()
        {
            // A save written before the chest existed: only the other systems.
            var oldProgress = new WorldMapProgress();
            _save.Register(oldProgress);
            _save.Save(TestSlot);
            Assert.IsFalse(_save.PeekSlot(TestSlot).entries.Any(e => e.key == VillageChestStorage.Key), "precondition: no chest entry");

            // Loaded into a session whose chest is already full of things (the previous run).
            var reset = new NewSessionReset();
            var chest = new VillageChestStorage();
            reset.Register(chest);
            var loadedSave = new SaveManager { SessionReset = reset };
            loadedSave.Initialize();
            loadedSave.Register(chest);
            loadedSave.Register(new WorldMapProgress());
            chest.Store(Plain(Ore, 12), 12);

            Assert.IsTrue(loadedSave.Load(TestSlot));
            Assert.IsTrue(chest.IsEmpty, "no chest data = an empty chest");
            Assert.AreEqual(VillageChestStorage.DefaultSlotCount, chest.Slots.Count);

            // Null / garbage state is ignored, never throws.
            Assert.DoesNotThrow(() => chest.RestoreState(new VillageChestSaveState { slots = null }));
            Assert.IsTrue(chest.IsEmpty);
        }

        // ------------------------------------------------------------------
        // Guardar materiais / Pegar o que as missões pedem
        // ------------------------------------------------------------------

        [Test]
        public void StoreMaterials_KeepsWhatActiveQuestsStillNeed()
        {
            QuestManager quests = StartCollectQuest(("wood", Wood, 6));
            InventoryManager bag = NewBag();
            Assert.AreEqual(0, bag.AddItem(Item(Wood), 10));
            Assert.AreEqual(0, bag.AddItem(Item(Fiber), 5));
            Assert.AreEqual(0, bag.AddItem(Item(Potion), 2));
            Assert.AreEqual(0, bag.AddItem(Item(Sword), 1));
            var chest = new VillageChestStorage();

            VillageChestBulkResult result = VillageChestTransfer.StoreMaterials(bag, chest, quests.GetOutstandingCollectAmount);

            Assert.AreEqual(9, result.Moved, "4 spare wood + 5 fiber");
            Assert.AreEqual(0, result.LeftBehind);
            Assert.AreEqual(6, bag.GetTotalCount(Wood), "the quest's 6 wood stay in the bag");
            Assert.AreEqual(4, chest.GetTotalCount(Wood));
            Assert.AreEqual(0, bag.GetTotalCount(Fiber));
            Assert.AreEqual(5, chest.GetTotalCount(Fiber));
            Assert.AreEqual(2, bag.GetTotalCount(Potion), "not a Material");
            Assert.AreEqual(1, bag.GetTotalCount(Sword), "not a Material");

            // Nothing spare left: a second press moves nothing.
            VillageChestBulkResult again = VillageChestTransfer.StoreMaterials(bag, chest, quests.GetOutstandingCollectAmount);
            Assert.AreEqual(0, again.Moved);
            StringAssert.StartsWith("Nenhum material sobrando", VillageChestUIController.DescribeBulkResult(true, again, null));
        }

        [Test]
        public void StoreMaterials_WithoutQuests_StoresEveryMaterial()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(Item(Wood), 3);
            bag.AddItem(Item(Ore), 2);
            var chest = new VillageChestStorage();

            VillageChestBulkResult result = VillageChestTransfer.StoreMaterials(bag, chest, null);

            Assert.AreEqual(5, result.Moved);
            Assert.AreEqual(0, bag.GetTotalWeight(), 0.001f);
            Assert.AreEqual(3, chest.GetTotalCount(Wood));
            Assert.AreEqual(2, chest.GetTotalCount(Ore));
        }

        [Test]
        public void TakeForQuests_RespectsTheBagGrid()
        {
            ItemDefinition stone = Item(Stone);
            ItemDefinition ore = Item(Ore);
            QuestManager quests = StartCollectQuest(("ore", Ore, ore.MaxStack + 2), ("fiber", Fiber, 2));
            InventoryManager bag = NewBag();

            // Every cell but one holds a full stack of stone; the last one an ore stack with room for 3 more.
            Assert.AreEqual(0, bag.AddItem(stone, stone.MaxStack * (bag.CellCount - 1)));
            Assert.AreEqual(0, bag.AddItem(ore, ore.MaxStack - 3));
            Assert.AreEqual(0, bag.FreeCellCount);
            int expectedOre = 3; // the quest still needs 5; only 3 fit

            var chest = new VillageChestStorage();
            chest.Store(Plain(Ore, 10), 10);
            chest.Store(Plain(Wood, 8), 8);

            VillageChestBulkResult result = VillageChestTransfer.TakeForQuests(chest, bag, quests.GetOutstandingCollectAmount);

            Assert.AreEqual(ore.MaxStack - 3 + expectedOre, bag.GetTotalCount(Ore), "only what the grid allows");
            Assert.AreEqual(10 - expectedOre, chest.GetTotalCount(Ore));
            Assert.AreEqual(expectedOre, result.Moved);
            Assert.AreEqual(5 - expectedOre, result.LeftBehind, "the rest the quest asks for stays in the chest");
            Assert.AreEqual(0, bag.FreeCellCount);
            Assert.AreEqual(8, chest.GetTotalCount(Wood), "no quest asks for wood");
            Assert.AreEqual(0, bag.GetTotalCount(Fiber), "the chest has no fiber");
            Assert.AreEqual($"Na mochila: {expectedOre}. Ficou no baú: {5 - expectedOre} (não coube mais na mochila).", VillageChestUIController.DescribeBulkResult(false, result, null));
        }

        [Test]
        public void TakeForQuests_OnlyTakesWhatTheBagStillLacks()
        {
            QuestManager quests = StartCollectQuest(("wood", Wood, 6));
            InventoryManager bag = NewBag();
            bag.AddItem(Item(Wood), 4);
            var chest = new VillageChestStorage();
            chest.Store(Plain(Wood, 20), 20);

            VillageChestBulkResult result = VillageChestTransfer.TakeForQuests(chest, bag, quests.GetOutstandingCollectAmount);

            Assert.AreEqual(2, result.Moved);
            Assert.AreEqual(6, bag.GetTotalCount(Wood));
            Assert.AreEqual(18, chest.GetTotalCount(Wood));
        }

        [Test]
        public void TakeToBag_RefusesWhatTheBagCannotCarry_AndKeepsGeneratedData()
        {
            InventoryManager bag = NewBag();
            ItemDefinition stone = Item(Stone);
            ItemDefinition sword = Item(Sword);
            Assume.That(sword.Weight, Is.GreaterThan(0f));
            // Fill every cell with stone: the sword (1×3) has nowhere to go.
            Assert.AreEqual(0, bag.AddItem(stone, stone.MaxStack * bag.CellCount));
            var chest = new VillageChestStorage();
            chest.Store(Generated(Sword), 1);
            int swordSlot = IndexOf(chest, Sword);

            Assert.AreEqual(0, VillageChestTransfer.TakeToBag(chest, swordSlot, 1, bag), "no free 1x3 area");
            Assert.AreEqual(1, chest.GetTotalCount(Sword), "stays in the chest");

            // Free a 1×3 column (cells (0,0), (0,1), (0,2)): now it fits.
            for (int y = 0; y < 3; y++)
            {
                int index = bag.GetSlotIndexAt(0, y);
                Assert.IsTrue(bag.RemoveFromSlot(index, bag.Slots[index].quantity));
            }

            Assert.AreEqual(1, VillageChestTransfer.TakeToBag(chest, swordSlot, 1, bag));
            InventorySlotData carried = bag.Slots.Single(s => s.itemId == Sword);
            Assert.AreEqual("Espada do Teste", carried.generatedName);
            Assert.AreEqual(ItemRarity.Rare, carried.generatedRarity);
            Assert.IsTrue(chest.IsEmpty);
        }

        [Test]
        public void TakeToBag_RespectsBagGrid()
        {
            InventoryManager bag = NewBag();
            int swords = 0;
            while (bag.AddItem(Item("item_wooden_sword"), 1) == 0)
            {
                swords++;
            }

            Assert.AreEqual(16, swords, "1×3 swords: 8 columns × 2 per column on an 8×8 grid");
            Assert.AreEqual(0, bag.AddItem(Item(Stone), Item(Stone).MaxStack * 16), "the last two rows take 1×1 stacks");
            Assert.AreEqual(0, bag.FreeCellCount);

            var chest = new VillageChestStorage();
            chest.Store(Plain(Fiber, 3), 3);

            Assert.AreEqual(0, VillageChestTransfer.TakeToBag(chest, IndexOf(chest, Fiber), 3, bag), "no free slot, no stack");
            Assert.AreEqual(3, chest.GetTotalCount(Fiber));
        }

        // ------------------------------------------------------------------
        // Slot cap
        // ------------------------------------------------------------------

        [Test]
        public void Chest_HasSixtySlots_AndStacksLikeTheBag()
        {
            var chest = new VillageChestStorage();
            Assert.AreEqual(60, chest.SlotCount);

            Assert.AreEqual(10, chest.Store(Plain(Wood, 10), 10));
            for (int i = 1; i < VillageChestStorage.DefaultSlotCount; i++)
            {
                Assert.AreEqual(1, chest.Store(Plain("item_wooden_sword", 1), 1), "piece " + i);
            }

            Assert.AreEqual(0, chest.FreeSlotCount);
            Assert.AreEqual(0, chest.Store(Plain("item_wooden_sword", 1), 1), "61st slot refused");
            Assert.AreEqual(0, chest.Store(Generated(Sword), 1), "a generated roll needs its own slot");
            Assert.AreEqual(Item(Wood).MaxStack - 10, chest.Store(Plain(Wood, 500), 500), "a full chest still tops up its stacks");
            Assert.AreEqual(Item(Wood).MaxStack, chest.GetTotalCount(Wood));

            // Storing from the bag into the full chest leaves the bag untouched.
            InventoryManager bag = NewBag();
            bag.AddItem(Item(Ore), 4);
            Assert.AreEqual(0, VillageChestTransfer.StoreFromBag(bag, IndexOf(bag, Ore), 4, chest));
            Assert.AreEqual(4, bag.GetTotalCount(Ore));

            VillageChestBulkResult result = VillageChestTransfer.StoreMaterials(bag, chest, null);
            Assert.AreEqual(0, result.Moved);
            Assert.AreEqual(4, result.LeftBehind);
            Assert.AreEqual("O baú tá cheio — não cabe nem agulha.", VillageChestUIController.DescribeBulkResult(true, result, null));
        }

        [Test]
        public void AmountPicker_ClampsBetweenOneAndTheStack()
        {
            Assert.AreEqual(1, VillageChestUIController.ClampAmount(0, 12));
            Assert.AreEqual(12, VillageChestUIController.ClampAmount(40, 12));
            Assert.AreEqual(5, VillageChestUIController.ClampAmount(5, 12));
            Assert.IsTrue(VillageChestUIController.NeedsAmountPicker(Plain(Wood, 2)));
            Assert.IsFalse(VillageChestUIController.NeedsAmountPicker(Plain(Wood, 1)));
            Assert.IsFalse(VillageChestUIController.NeedsAmountPicker(InventorySlotData.Empty()));
        }

        [Test]
        public void PartialStoreFromBag_SplitsTheStack()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(Item(Wood), 12);
            var chest = new VillageChestStorage();

            Assert.AreEqual(5, VillageChestTransfer.StoreFromBag(bag, IndexOf(bag, Wood), 5, chest));
            Assert.AreEqual(7, bag.GetTotalCount(Wood));
            Assert.AreEqual(5, chest.GetTotalCount(Wood));
            Assert.AreEqual(3, VillageChestTransfer.TakeToBag(chest, IndexOf(chest, Wood), 3, bag));
            Assert.AreEqual(10, bag.GetTotalCount(Wood));
            Assert.AreEqual(2, chest.GetTotalCount(Wood));
        }

        // ------------------------------------------------------------------
        // Map lifecycle
        // ------------------------------------------------------------------

        [Test]
        public void MovesOnTheMap_ReachTheNextEncounter_AndTheSave()
        {
            var chest = new VillageChestStorage();
            _save.Register(chest);
            chest.Store(Plain(Ore, 3), 3);
            CarryInventory((Wood, 10), (Potion, 1));

            var session = new CarriedInventorySession(_save);
            try
            {
                InventoryManager live = session.Open();
                Assert.AreEqual(10, VillageChestTransfer.StoreFromBag(live, IndexOf(live, Wood), 10, chest));
                Assert.AreEqual(3, VillageChestTransfer.TakeToBag(chest, IndexOf(chest, Ore), 3, live));

                // Saved while the chest window is open.
                _save.Save(TestSlot);
            }
            finally
            {
                session.Close();
            }

            InventoryManager player = NextEncounterPlayer(_save);
            Assert.AreEqual(0, player.GetTotalCount(Wood), "wood stayed in the chest");
            Assert.AreEqual(3, player.GetTotalCount(Ore), "ore taken from the chest");
            Assert.AreEqual(1, player.GetTotalCount(Potion));
            Assert.AreEqual(10, chest.GetTotalCount(Wood));

            // Load on the map: the chest and the carried bag both come back.
            var loaded = new SaveManager();
            loaded.Initialize();
            var loadedChest = new VillageChestStorage();
            loaded.Register(loadedChest);
            Assert.IsTrue(loaded.Load(TestSlot));
            Assert.AreEqual(10, loadedChest.GetTotalCount(Wood));
            Assert.AreEqual(0, loadedChest.GetTotalCount(Ore));

            var mapSession = new CarriedInventorySession(loaded);
            InventoryManager map = mapSession.Open();
            Assert.AreEqual(3, map.GetTotalCount(Ore));
            Assert.AreEqual(0, map.GetTotalCount(Wood));
            mapSession.Close();
        }

        [Test]
        public void HubInventorySession_StaysOpenWhileTheChestIsOpen()
        {
            Assert.IsFalse(VillageHubButton.ShouldEndInventorySession(true, false, false, true));
            Assert.IsTrue(VillageHubButton.ShouldEndInventorySession(true, false, false, false));
            Assert.IsTrue(VillageHubButton.ShouldEndInventorySession(true, false, false), "old overload unchanged");
            Assert.IsNotNull(Resources.Load<VisualTreeAsset>(VillageHubButton.ChestViewResourcePath));
        }

        // ------------------------------------------------------------------
        // New session
        // ------------------------------------------------------------------

        [Test]
        public void NewGame_EmptiesTheChest_AndGameManagerRegistersIt()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            _gameManagerObject = new GameObject("VillageChestTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.VillageChest, Is.Not.Null);

            Assert.IsTrue(_gm.NewSessionReset.Resettables.Contains(_gm.VillageChest));
            Assert.AreSame(_gm.VillageChest, VillageChestStorage.ResolveShared());

            _gm.VillageChest.Store(Plain(Wood, 7), 7);
            _gm.SaveManager.Save(TestSlot);
            Assert.IsTrue(_gm.SaveManager.PeekSlot(TestSlot).entries.Any(e => e.key == VillageChestStorage.Key), "registered with SaveManager");

            _gm.GameFlowService.EnterFrontEnd();
            Assert.IsTrue(_gm.GameFlowService.StartNewGame());
            Assert.IsTrue(_gm.VillageChest.IsEmpty, "Novo jogo starts with an empty chest");

            Assert.IsTrue(_gm.SaveManager.Load(TestSlot));
            Assert.AreEqual(7, _gm.VillageChest.GetTotalCount(Wood), "loading the slot brings it back");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static ItemDefinition Item(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            Assert.IsNotNull(item, itemId);
            return item;
        }

        private static InventorySlotData Plain(string itemId, int quantity)
        {
            ItemDefinition item = Item(itemId);
            return new InventorySlotData { itemId = itemId, quantity = quantity, currentDurability = item.HasDurability ? item.MaxDurability : 0f };
        }

        private static InventorySlotData Generated(string itemId)
        {
            InventorySlotData slot = Plain(itemId, 1);
            slot.currentDurability = Mathf.Max(1f, slot.currentDurability * 0.5f);
            slot.generatedAffixIds = new[] { "affix_test_sharp" };
            slot.generatedAffixValues = new[] { 4f };
            slot.generatedName = "Espada do Teste";
            slot.generatedRarity = ItemRarity.Rare;
            return slot;
        }

        private static int IndexOf(InventoryManager bag, string itemId)
        {
            for (int i = 0; i < bag.Slots.Count; i++)
            {
                if (bag.Slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            Assert.Fail(itemId + " not in the bag");
            return -1;
        }

        private static int IndexOf(VillageChestStorage chest, string itemId)
        {
            for (int i = 0; i < chest.Slots.Count; i++)
            {
                if (chest.Slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            Assert.Fail(itemId + " not in the chest");
            return -1;
        }

        private InventoryManager NewBag()
        {
            var go = new GameObject("ChestTestBag");
            _created.Add(go);
            return go.AddComponent<InventoryManager>();
        }

        private QuestManager StartCollectQuest(params (string ObjectiveId, string ItemId, int Amount)[] objectives)
        {
            var quests = new QuestManager();
            quests.Initialize();
            _quests.Add(quests);

            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            _created.Add(quest);
            quest.SetDataForTests("q_chest_test", "q_chest_test.title", "q_chest_test.description", QuestType.Side,
                objectives.Select(o => new QuestObjective
                {
                    ObjectiveId = o.ObjectiveId,
                    Type = QuestObjectiveType.CollectItem,
                    TargetId = o.ItemId,
                    RequiredAmount = o.Amount,
                    DescriptionKey = "obj." + o.ObjectiveId,
                }).ToList());
            Assert.IsTrue(quests.StartQuest(quest));
            return quests;
        }

        /// <summary>The previous encounter's Player: holds <paramref name="items"/>, then unloads (its inventory becomes carried).</summary>
        private void CarryInventory(params (string ItemId, int Quantity)[] items)
        {
            var go = new GameObject("PreviousScenePlayer");
            _created.Add(go);
            var inventory = go.AddComponent<InventoryManager>();
            _save.Register(inventory);
            foreach ((string itemId, int quantity) in items)
            {
                Assert.AreEqual(0, inventory.AddItem(Item(itemId), quantity), itemId);
            }

            _save.Unregister(inventory);
        }

        private InventoryManager NextEncounterPlayer(SaveManager save)
        {
            var go = new GameObject("NextScenePlayer");
            _created.Add(go);
            var inventory = go.AddComponent<InventoryManager>();
            save.Register(inventory);
            save.RestoreCarriedState();
            return inventory;
        }
    }
}
