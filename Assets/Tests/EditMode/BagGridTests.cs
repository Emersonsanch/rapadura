using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Inventory;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The MU Online-style Mochila (Docs/Balance/README.md, "Mochila em grade"): footprints by category with per-item
    /// overrides, the pure placement rules (<see cref="BagGrid"/>, <see cref="BagGridRules"/>: first fit from the top-left,
    /// explicit placement, move / merge / swap, "Organizar" packing), the shared fit API (<see cref="BagFitSimulator"/>,
    /// <see cref="InventoryManager.CanFit"/>), the grid on a real <see cref="InventoryManager"/>, the old-save migration
    /// (auto-place, overflow to the Baú da Vó Edna, never lost) and the UI's pure tap/cell helpers.
    /// </summary>
    public class BagGridTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        private ItemDefinition _potion;
        private ItemDefinition _herb;
        private ItemDefinition _sword;
        private ItemDefinition _armour;
        private ItemDefinition _bow;
        private ItemDefinition _ring;
        private ItemDefinition _gold;
        private ItemDefinition _shield;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _potion = Create("grid_potion", ItemType.Consumable, ItemCategory.Potion, maxStack: 20);
            _herb = Create("grid_herb", ItemType.Material, ItemCategory.Herb, maxStack: 10);
            _sword = Create("grid_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, maxStack: 1, equipSlot: EquipmentSlot.MainHand);
            _armour = Create("grid_armour", ItemType.Armor, ItemCategory.Chest, maxStack: 1, equipSlot: EquipmentSlot.Chest);
            _bow = Create("grid_bow", ItemType.Weapon, ItemCategory.RangedWeapon, maxStack: 1, equipSlot: EquipmentSlot.MainHand);
            _ring = Create("grid_ring", ItemType.Armor, ItemCategory.Accessory, maxStack: 1, equipSlot: EquipmentSlot.Accessory);
            _gold = Create("grid_gold", ItemType.Currency, ItemCategory.Currency, maxStack: 9999);
            _shield = Create("grid_shield", ItemType.Armor, ItemCategory.Miscellaneous, maxStack: 1, equipSlot: EquipmentSlot.OffHand, gridWidth: 2, gridHeight: 2);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            foreach (Object obj in _created.Where(o => o != null))
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            _items.Clear();
        }

        // ------------------------------------------------------------------
        // Footprints
        // ------------------------------------------------------------------

        [Test]
        public void Footprint_ComesFromTheCategory()
        {
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(_potion));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(_herb));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(_ring));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(_gold));
            Assert.AreEqual(new GridSize(1, 3), ItemFootprint.For(_sword));
            Assert.AreEqual(new GridSize(2, 3), ItemFootprint.For(_armour));
            Assert.AreEqual(new GridSize(2, 4), ItemFootprint.For(_bow));
            Assert.AreEqual(new GridSize(2, 2), ItemFootprint.ForCategory(ItemCategory.Head));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(null), "unknown item");
        }

        [Test]
        public void Footprint_PerItemOverrideWins_AndIsClampedToTheGrid()
        {
            Assert.AreEqual(ItemFootprint.Shield, ItemFootprint.For(_shield));

            ItemDefinition huge = Create("grid_huge", ItemType.Material, ItemCategory.Miscellaneous, maxStack: 1, gridWidth: 12, gridHeight: 3);
            Assert.AreEqual(new GridSize(8, 3), ItemFootprint.For(huge, 8, 8));

            ItemDefinition tallOnly = Create("grid_tall", ItemType.Weapon, ItemCategory.MeleeWeapon, maxStack: 1, gridHeight: 4);
            Assert.AreEqual(new GridSize(1, 4), ItemFootprint.For(tallOnly), "width stays the category's");
        }

        [Test]
        public void Footprint_OfEveryShippedItem_IsSensible()
        {
            ItemDatabase.Invalidate();
            var shipped = ItemDatabase.GetAll().Where(i => i != null).ToList();
            CollectionAssert.IsNotEmpty(shipped);

            foreach (ItemDefinition item in shipped)
            {
                GridSize size = ItemFootprint.For(item);
                Assert.LessOrEqual(size.Width, InventoryManager.DefaultColumns, item.ItemId);
                Assert.LessOrEqual(size.Height, InventoryManager.DefaultRows, item.ItemId);
                Assert.LessOrEqual(size.Area, 8, $"{item.ItemId} ({size}) would eat more than 1/8 of the bag");

                if (item.IsStackable)
                {
                    Assert.AreEqual(1, size.Area, $"{item.ItemId}: stackables (gold, materials, potions, food) take one cell");
                }
            }

            Assert.AreEqual(new GridSize(1, 3), ItemFootprint.For(ItemDatabase.GetById("item_iron_sword")));
            Assert.AreEqual(new GridSize(2, 4), ItemFootprint.For(ItemDatabase.GetById("item_hunting_bow")));
            Assert.AreEqual(new GridSize(2, 3), ItemFootprint.For(ItemDatabase.GetById("item_leather_chest")));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(ItemDatabase.GetById("item_health_potion")));
            Assert.AreEqual(new GridSize(1, 1), ItemFootprint.For(ItemDatabase.GetById("item_gold")));
        }

        // ------------------------------------------------------------------
        // BagGrid
        // ------------------------------------------------------------------

        [Test]
        public void FirstFit_ScansFromTheTopLeft_RowByRow()
        {
            var grid = new BagGrid(4, 4);
            Assert.IsTrue(grid.TryFindFirstFit(new GridSize(1, 1), out int x, out int y));
            Assert.AreEqual((0, 0), (x, y));

            Assert.IsTrue(grid.TryOccupy(0, new GridRect(0, 0, 2, 1)));
            Assert.IsTrue(grid.TryFindFirstFit(new GridSize(1, 1), out x, out y));
            Assert.AreEqual((2, 0), (x, y), "same row first, left to right");

            Assert.IsTrue(grid.TryFindFirstFit(new GridSize(3, 1), out x, out y));
            Assert.AreEqual((0, 1), (x, y), "a 3-wide piece no longer fits on row 0");

            Assert.IsFalse(grid.TryFindFirstFit(new GridSize(5, 1), out _, out _), "wider than the grid");
        }

        [Test]
        public void Occupy_RefusesOverlapsAndOutOfBounds_ReleaseFrees()
        {
            var grid = new BagGrid(3, 3);
            Assert.IsTrue(grid.TryOccupy(7, new GridRect(1, 0, 1, 3)));
            Assert.AreEqual(7, grid.At(1, 2));
            Assert.AreEqual(6, grid.FreeCellCount);

            Assert.IsFalse(grid.TryOccupy(8, new GridRect(0, 1, 2, 1)), "overlaps entry 7");
            Assert.IsFalse(grid.TryOccupy(8, new GridRect(2, 2, 2, 1)), "outside the grid");
            Assert.IsTrue(grid.IsFree(new GridRect(1, 0, 1, 2), ignoreId: 7), "its own cells don't block it");

            grid.Release(7);
            Assert.AreEqual(9, grid.FreeCellCount);
            Assert.AreEqual(BagGrid.Empty, grid.At(1, 1));
        }

        // ------------------------------------------------------------------
        // Move / merge / swap
        // ------------------------------------------------------------------

        [Test]
        public void PlanMove_FreeArea_Moves_SameSpot_IsUnchanged_Edge_IsInvalid()
        {
            InventorySlotData[] slots = { At(_sword, 1, 0, 0) };
            BagGrid grid = BagGridRules.Build(4, 4, slots, Resolve);

            Assert.AreEqual(BagMoveKind.Move, BagGridRules.PlanMove(grid, slots, Resolve, 0, 3, 1).Kind);
            Assert.AreEqual(BagMoveKind.Move, BagGridRules.PlanMove(grid, slots, Resolve, 0, 0, 1).Kind, "overlapping only itself");
            Assert.AreEqual(BagMoveKind.Unchanged, BagGridRules.PlanMove(grid, slots, Resolve, 0, 0, 0).Kind);
            Assert.AreEqual(BagMoveKind.Invalid, BagGridRules.PlanMove(grid, slots, Resolve, 0, 0, 2).Kind, "1×3 from row 2 sticks out of a 4-row grid");
            Assert.AreEqual(BagMoveKind.Invalid, BagGridRules.PlanMove(grid, slots, Resolve, 0, -1, 0).Kind);
        }

        [Test]
        public void PlanMove_OntoAPlainStackOfTheSameItem_Merges_ButNeverAGeneratedRoll()
        {
            InventorySlotData[] slots = { At(_herb, 4, 0, 0), At(_herb, 3, 1, 0), At(_herb, 1, 2, 0) };
            slots[2].generatedName = "Erva Rara";
            slots[2].generatedRarity = ItemRarity.Rare;
            BagGrid grid = BagGridRules.Build(4, 1, slots, Resolve);

            BagMovePlan merge = BagGridRules.PlanMove(grid, slots, Resolve, 0, 1, 0);
            Assert.AreEqual(BagMoveKind.Merge, merge.Kind);
            Assert.AreEqual(1, merge.OtherIndex);

            Assert.AreEqual(BagMoveKind.Swap, BagGridRules.PlanMove(grid, slots, Resolve, 0, 2, 0).Kind, "generated: swap, never merge");
        }

        [Test]
        public void PlanMove_OntoOneItem_Swaps_BackToTheOldSpot_OrTheFirstFreeArea()
        {
            // Two potions side by side: a plain swap of places.
            InventorySlotData[] pair = { At(_potion, 1, 0, 0), At(_ring, 1, 1, 0) };
            BagGrid pairGrid = BagGridRules.Build(2, 1, pair, Resolve);
            BagMovePlan swap = BagGridRules.PlanMove(pairGrid, pair, Resolve, 0, 1, 0);
            Assert.AreEqual(BagMoveKind.Swap, swap.Kind);
            Assert.AreEqual((1, 0, 0), (swap.OtherIndex, swap.OtherX, swap.OtherY));

            // A ring dropped on a sword's foot: the sword (1×3) can't go to the ring's old spot (it would stick out), so it
            // takes the first free area instead.
            InventorySlotData[] slots = { At(_sword, 1, 0, 0), At(_ring, 1, 1, 1) };
            BagGrid grid = BagGridRules.Build(2, 3, slots, Resolve);
            BagMovePlan moved = BagGridRules.PlanMove(grid, slots, Resolve, 1, 0, 2);
            Assert.AreEqual(BagMoveKind.Swap, moved.Kind, "ring onto the sword's foot");
            Assert.AreEqual((0, 1, 0), (moved.OtherIndex, moved.OtherX, moved.OtherY), "sword to column 1, the first free 1x3");

            // The sword dropped on column 1 (where the ring is): the ring goes back to the sword's old top-left.
            BagMovePlan back = BagGridRules.PlanMove(grid, slots, Resolve, 0, 1, 0);
            Assert.AreEqual(BagMoveKind.Swap, back.Kind);
            Assert.AreEqual((1, 0, 0), (back.OtherIndex, back.OtherX, back.OtherY));
        }

        [Test]
        public void PlanMove_OverTwoItems_IsInvalid()
        {
            InventorySlotData[] slots = { At(_sword, 1, 0, 0), At(_potion, 1, 1, 0), At(_potion, 1, 1, 1) };
            BagGrid grid = BagGridRules.Build(2, 3, slots, Resolve);

            Assert.AreEqual(BagMoveKind.Invalid, BagGridRules.PlanMove(grid, slots, Resolve, 0, 1, 0).Kind);
        }

        // ------------------------------------------------------------------
        // Packing ("Organizar") and restore layout (migration)
        // ------------------------------------------------------------------

        [Test]
        public void TryPack_PlacesInOrder_AndSortPacking_FallsBackToBiggestFirst()
        {
            Assert.IsTrue(BagGridRules.TryPack(4, 2, new[] { new GridSize(1, 1), new GridSize(2, 2) }, out GridRect[] rects));
            Assert.AreEqual((0, 0), (rects[0].X, rects[0].Y));
            Assert.AreEqual((1, 0), (rects[1].X, rects[1].Y));

            // Two smalls first leave no 2x2 hole in a 3x2 grid; biggest first packs all 6 cells.
            GridSize[] sizes = { new GridSize(1, 1), new GridSize(1, 1), new GridSize(2, 2) };
            Assert.IsFalse(BagGridRules.TryPack(3, 2, sizes, out _));
            Assert.IsTrue(BagGridRules.TryPackForSort(3, 2, sizes, out rects));
            Assert.AreEqual(3, rects.Length, "rects come back in the given order");
            Assert.AreEqual((0, 0), (rects[2].X, rects[2].Y), "the 2x2 went first");
            Assert.AreEqual((2, 0), (rects[0].X, rects[0].Y));
            Assert.AreEqual((2, 1), (rects[1].X, rects[1].Y));
            Assert.IsFalse(BagGridRules.TryPackForSort(2, 2, sizes, out _), "6 cells never fit in 4");
        }

        [Test]
        public void LayoutForRestore_GridSave_KeepsValidPositions_ReplacesOnlyBrokenOnes()
        {
            InventorySlotData[] slots =
            {
                At(_potion, 1, 3, 3),
                At(_ring, 1, 3, 3), // overlaps the potion (e.g. a footprint changed): re-placed
                InventorySlotData.Empty(),
            };

            List<int> overflow = BagGridRules.LayoutForRestore(4, 4, slots, Resolve, keepPositions: true);

            CollectionAssert.IsEmpty(overflow);
            Assert.AreEqual((3, 3), (slots[0].gridX, slots[0].gridY), "kept");
            Assert.AreEqual((0, 0), (slots[1].gridX, slots[1].gridY), "first free area");
        }

        [Test]
        public void LayoutForRestore_OldSave_AutoPlaces_GoldAndQuestFirst_ThenBiggest_AndListsOverflow()
        {
            // 2x4 grid = 8 cells. Old save: potion, bow (2x4 = 8 cells!), gold. Gold first, then the bow can't fit any more.
            InventorySlotData[] slots = { At(_potion, 3, 0, 0), At(_bow, 1, 0, 0), At(_gold, 50, 0, 0) };

            List<int> overflow = BagGridRules.LayoutForRestore(2, 4, slots, Resolve, keepPositions: false);

            Assert.AreEqual((0, 0), (slots[2].gridX, slots[2].gridY), "gold is placed first");
            CollectionAssert.AreEqual(new[] { 1 }, overflow, "the bow (8 cells) no longer fits");
            Assert.AreEqual((1, 0), (slots[0].gridX, slots[0].gridY));
        }

        // ------------------------------------------------------------------
        // Shared fit simulation
        // ------------------------------------------------------------------

        [Test]
        public void FitSimulator_FillsPlainStacksFirst_ThenFreeAreas_AndChecksBundles()
        {
            InventorySlotData[] slots = { At(_herb, 8, 0, 0), InventorySlotData.Empty(), InventorySlotData.Empty(), InventorySlotData.Empty() };
            BagGrid grid = BagGridRules.Build(2, 2, slots, Resolve);

            Assert.AreEqual(32, new BagFitSimulator(grid, slots, Resolve).Add(_herb, 50), "2 in the stack + 3 new stacks of 10");
            Assert.AreEqual(0, new BagFitSimulator(grid, slots, Resolve).Add(_bow, 1), "a bow (clamped to 2x2 here) needs the whole grid");

            var bundle = new BagFitSimulator(grid, slots, Resolve);
            Assert.AreEqual(1, bundle.Add(_ring, 1));
            Assert.AreEqual(1, bundle.Add(_potion, 1));
            Assert.AreEqual(1, bundle.Add(_potion, 1), "into the potion stack the simulation just created");
            Assert.AreEqual(10, bundle.Add(_herb, 10), "2 into the stack + 8 in the last free cell");
            Assert.AreEqual(0, bundle.Add(_ring, 1), "no cell left");
        }

        // ------------------------------------------------------------------
        // InventoryManager on the grid
        // ------------------------------------------------------------------

        [Test]
        public void Inventory_IsEightByEight_AndItemsTakeTheirFootprint()
        {
            InventoryManager bag = NewBag();
            Assert.AreEqual(8, bag.Columns);
            Assert.AreEqual(8, bag.Rows);
            Assert.AreEqual(64, bag.Slots.Count, "one entry per cell at most");

            Assert.AreEqual(0, bag.AddItem(_armour, 1));
            Assert.AreEqual(0, bag.AddItem(_sword, 1));
            Assert.AreEqual(0, bag.AddItem(_potion, 25));

            Assert.AreEqual(6 + 3 + 2, bag.UsedCellCount, "armour 2x3, sword 1x3, two potion stacks (20 + 5)");
            int armour = Index(bag, _armour.ItemId);
            int sword = Index(bag, _sword.ItemId);
            Assert.AreEqual((0, 0), (bag.Slots[armour].gridX, bag.Slots[armour].gridY));
            Assert.AreEqual((2, 0), (bag.Slots[sword].gridX, bag.Slots[sword].gridY), "next free area to the right");
            Assert.AreEqual(sword, bag.GetSlotIndexAt(2, 2));
            AssertNoOverlap(bag);
        }

        [Test]
        public void CanFit_MatchesWhatAddItemDoes()
        {
            InventoryManager bag = NewBag();
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(0, bag.AddItem(_sword, 1), "column " + i);
            }

            // Rows 0-2 are full of swords; 5 rows (40 cells) left. Bows are 2×4: 4 fit side by side on rows 3-6.
            Assert.IsTrue(bag.CanFit(_bow, 4));
            Assert.IsFalse(bag.CanFit(_bow, 5));
            Assert.AreEqual(4, bag.GetFittableAmount(_bow, 9));
            Assert.AreEqual(5, bag.AddItem(_bow, 9), "AddItem agrees: 5 left over");

            Assert.AreEqual(8, bag.FreeCellCount, "row 7");
            Assert.IsTrue(bag.CanFitAll(new[] { new KeyValuePair<ItemDefinition, int>(_potion, 40), new KeyValuePair<ItemDefinition, int>(_ring, 6) }));
            Assert.IsFalse(bag.CanFitAll(new[] { new KeyValuePair<ItemDefinition, int>(_potion, 40), new KeyValuePair<ItemDefinition, int>(_ring, 7) }));
            Assert.IsFalse(bag.CanFit(_sword, 1), "no 1x3 area left");
        }

        [Test]
        public void MoveItem_MovesSwapsAndMerges_OnTheGrid()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_sword, 1);
            bag.AddItem(_potion, 5);
            int sword = Index(bag, _sword.ItemId);
            int potion = Index(bag, _potion.ItemId);
            Assert.AreEqual((1, 0), (bag.Slots[potion].gridX, bag.Slots[potion].gridY));

            Assert.IsTrue(bag.MoveItem(sword, 7, 5));
            Assert.AreEqual((7, 5), (bag.Slots[sword].gridX, bag.Slots[sword].gridY));
            Assert.IsFalse(bag.MoveItem(sword, 7, 6), "would stick out of the bottom");

            Assert.IsTrue(bag.MoveItem(potion, 7, 7), "onto the sword's foot: swap");
            Assert.AreEqual((7, 7), (bag.Slots[potion].gridX, bag.Slots[potion].gridY));
            Assert.AreEqual((1, 0), (bag.Slots[sword].gridX, bag.Slots[sword].gridY), "the sword took the potion's old spot");
            AssertNoOverlap(bag);

            // A split stack lands at the first free area and merges back when dropped on the original.
            Assert.IsTrue(bag.SplitStack(potion, 2));
            int split = bag.Slots.ToList().FindIndex(s => s.itemId == _potion.ItemId && s.quantity == 2);
            Assert.GreaterOrEqual(split, 0);
            Assert.AreEqual((0, 0), (bag.Slots[split].gridX, bag.Slots[split].gridY));
            Assert.IsTrue(bag.MoveItem(split, 7, 7));
            Assert.AreEqual(5, bag.Slots[potion].quantity);
            Assert.AreEqual(5, bag.GetTotalCount(_potion.ItemId));
        }

        [Test]
        public void SortInventory_PacksFromTheTopLeft_WithoutOverlaps()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_potion, 3);
            bag.AddItem(_ring, 1);
            bag.AddItem(_bow, 1);
            bag.AddItem(_sword, 1);
            bag.MoveItem(Index(bag, _bow.ItemId), 6, 4);

            bag.SortInventory();

            AssertNoOverlap(bag);
            InventorySlotData first = bag.Slots[0];
            Assert.AreEqual((0, 0), (first.gridX, first.gridY), "the first sorted entry is at the top-left");
            Assert.AreEqual(1 + 1 + 8 + 3, bag.UsedCellCount);
        }

        [Test]
        public void EquipFromSlot_Swap_ReturnsTheOldPieceToTheGrid()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_sword, 1);
            Assert.IsTrue(bag.EquipFromSlot(Index(bag, _sword.ItemId)));
            Assert.AreEqual(0, bag.UsedCellCount);

            bag.AddItem(_bow, 1);
            Assert.IsTrue(bag.EquipFromSlot(Index(bag, _bow.ItemId)), "the bow's 2x4 area takes the sword back");
            Assert.AreEqual(_bow.ItemId, bag.Equipped[EquipmentSlot.MainHand].itemId);
            Assert.AreEqual(3, bag.UsedCellCount);
        }

        [Test]
        public void TryAddCopy_KeepsGeneratedData_AndIsAllOrNothing()
        {
            InventoryManager bag = NewBag();
            var rolled = new InventorySlotData { itemId = _sword.ItemId, quantity = 1, generatedName = "Peixeira Lendária", generatedRarity = ItemRarity.Legendary };

            Assert.IsTrue(bag.CanFitCopy(rolled));
            Assert.IsTrue(bag.TryAddCopy(rolled));
            InventorySlotData placed = bag.Slots[Index(bag, _sword.ItemId)];
            Assert.AreEqual("Peixeira Lendária", placed.generatedName);
            Assert.AreNotSame(rolled, placed);

            InventoryManager tiny = NewBag(1, 3);
            tiny.AddItem(_potion, 1);
            Assert.IsFalse(tiny.CanFitCopy(rolled));
            Assert.IsFalse(tiny.TryAddCopy(rolled), "1x3 in a 1x3 bag that already holds a potion");
            Assert.AreEqual(1, tiny.UsedCellCount);
        }

        [Test]
        public void AddBagRows_GrowsTheGrid_UpToTheCap_AndIsSaved()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_potion, 1);
            Assert.IsTrue(bag.AddBagRows(2));
            Assert.AreEqual(10, bag.Rows);
            Assert.AreEqual(80, bag.Slots.Count);
            Assert.IsFalse(bag.AddBagRows(1), "MaxExtraRows = 2");
            Assert.AreEqual(1, bag.GetTotalCount(_potion.ItemId));

            var state = (InventorySaveState)bag.CaptureState();
            Assert.AreEqual(2, state.extraRows);
            InventoryManager loaded = NewBag();
            loaded.RestoreState(Json(state));
            Assert.AreEqual(10, loaded.Rows);
        }

        // ------------------------------------------------------------------
        // Save / migration
        // ------------------------------------------------------------------

        [Test]
        public void SaveAndLoad_KeepsCellPositions()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_sword, 1);
            bag.AddItem(_potion, 4);
            Assert.IsTrue(bag.MoveItem(Index(bag, _sword.ItemId), 5, 2));

            var state = (InventorySaveState)bag.CaptureState();
            Assert.AreEqual(InventoryManager.GridSaveVersion, state.gridVersion);

            InventoryManager loaded = NewBag();
            loaded.RestoreState(Json(state));

            InventorySlotData sword = loaded.Slots[Index(loaded, _sword.ItemId)];
            Assert.AreEqual((5, 2), (sword.gridX, sword.gridY));
            Assert.AreEqual(4, loaded.GetTotalCount(_potion.ItemId));
            CollectionAssert.IsEmpty(loaded.LastRestoreOverflow);
        }

        [Test]
        public void OldSave_IsAutoPlaced_AndWhatDoesNotFit_GoesToTheChest_NeverLost()
        {
            // A pre-grid save: 30 slot entries, no positions (all 0/0), more gear than 64 cells can hold.
            var old = new InventorySaveState { slots = new InventorySlotData[30] };
            for (int i = 0; i < old.slots.Length; i++)
            {
                old.slots[i] = InventorySlotData.Empty();
            }

            for (int i = 0; i < 9; i++)
            {
                old.slots[i] = Plain(_bow, 1); // 9 × 8 cells = 72 > 64
            }

            old.slots[9] = Plain(_gold, 120);
            old.slots[10] = Plain(_potion, 7);
            old.slots[11] = Plain(_herb, 10);
            old.slots[11].generatedName = "Erva Rara";
            old.slots[11].generatedRarity = ItemRarity.Rare;
            string json = JsonUtility.ToJson(old);
            StringAssert.DoesNotContain("gridVersion\":1", json);

            InventoryManager bag = NewBag();
            var chest = new VillageChestStorage(resolveItem: Resolve);
            bag.OverflowChest = chest;
            bag.RestoreState(JsonUtility.FromJson<InventorySaveState>(json));

            AssertNoOverlap(bag);
            Assert.AreEqual(120, bag.GetTotalCount(_gold.ItemId), "gold is placed first");
            Assert.AreEqual(7, bag.GetTotalCount(_potion.ItemId));

            int bowsInBag = bag.GetTotalCount(_bow.ItemId);
            int bowsInChest = chest.Inbox.Count(s => s.itemId == _bow.ItemId);
            int herbInChest = chest.Inbox.Where(s => s.itemId == _herb.ItemId).Sum(s => s.quantity);
            Assert.AreEqual(9, bowsInBag + bowsInChest, "every bow is somewhere");
            Assert.AreEqual(10, bag.GetTotalCount(_herb.ItemId) + herbInChest, "the generated herb too");
            Assert.Greater(bowsInChest, 0, "72 cells of bows can't fit 64");
            Assert.AreEqual(bag.LastRestoreOverflow.Count, chest.Inbox.Count);

            // The chest's own restore later in the same load does not wipe them; they move into its slots.
            chest.RestoreState(new VillageChestSaveState());
            Assert.AreEqual(0, chest.Inbox.Count);
            Assert.AreEqual(bowsInChest, chest.GetTotalCount(_bow.ItemId));

            // And a grid save of the migrated bag loads back exactly (gridVersion 1, no second migration).
            InventoryManager again = NewBag();
            again.OverflowChest = new VillageChestStorage(resolveItem: Resolve);
            again.RestoreState(Json((InventorySaveState)bag.CaptureState()));
            Assert.AreEqual(bowsInBag, again.GetTotalCount(_bow.ItemId));
            Assert.AreEqual(0, again.OverflowChest.Inbox.Count);
        }

        [Test]
        public void ChestInbox_WaitsWhenTheChestIsFull_AndIsSaved()
        {
            var chest = new VillageChestStorage(slotCount: 1, resolveItem: Resolve);
            chest.Store(Plain(_sword, 1), 1);
            chest.EnqueueOverflow(Plain(_bow, 1));

            Assert.AreEqual(0, chest.FlushInbox(), "no slot free");
            var saved = JsonUtility.FromJson<VillageChestSaveState>(JsonUtility.ToJson(chest.CaptureState()));
            Assert.AreEqual(1, saved.overflowInbox.Length, "kept in the save");

            var loaded = new VillageChestStorage(slotCount: 1, resolveItem: Resolve);
            loaded.RestoreState(saved);
            Assert.AreEqual(1, loaded.Inbox.Count);

            Assert.IsTrue(loaded.RemoveFromSlot(0, 1), "taking the sword out makes room");
            Assert.AreEqual(0, loaded.Inbox.Count);
            Assert.AreEqual(1, loaded.GetTotalCount(_bow.ItemId));
        }

        // ------------------------------------------------------------------
        // UI pure helpers
        // ------------------------------------------------------------------

        [Test]
        public void DragDrop_CellUnderPointer_AndGrabbedAnchor()
        {
            Assert.IsTrue(InventoryDragDrop.TryGetCell(new Vector2(53f, 10f), 52f, 8, 8, out int x, out int y));
            Assert.AreEqual((1, 0), (x, y));
            Assert.IsFalse(InventoryDragDrop.TryGetCell(new Vector2(-1f, 10f), 52f, 8, 8, out _, out _));
            Assert.IsFalse(InventoryDragDrop.TryGetCell(new Vector2(8 * 52f + 1f, 10f), 52f, 8, 8, out _, out _));

            Assert.AreEqual(new Vector2Int(3, 1), InventoryDragDrop.AnchorFor(3, 3, 0, 2), "a sword held by its tip");
        }

        [Test]
        public void TapToPick_TapToPlace()
        {
            Assert.AreEqual(BagTapAction.None, InventoryDragDrop.ResolveTap(-1, -1));
            Assert.AreEqual(BagTapAction.Pick, InventoryDragDrop.ResolveTap(-1, 4));
            Assert.AreEqual(BagTapAction.UseOrEquip, InventoryDragDrop.ResolveTap(4, 4));
            Assert.AreEqual(BagTapAction.Place, InventoryDragDrop.ResolveTap(4, 7));
            Assert.AreEqual(BagTapAction.Place, InventoryDragDrop.ResolveTap(4, -1));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private ItemDefinition Resolve(string itemId) => itemId != null && _items.TryGetValue(itemId, out ItemDefinition item) ? item : null;

        private ItemDefinition Create(string id, ItemType type, ItemCategory category, int maxStack, EquipmentSlot equipSlot = EquipmentSlot.None, int gridWidth = 0, int gridHeight = 0)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.FindProperty("_gridWidth").intValue = gridWidth;
            serialized.FindProperty("_gridHeight").intValue = gridHeight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            _items[id] = item;
            return item;
        }

        private static InventorySlotData At(ItemDefinition item, int quantity, int x, int y)
        {
            return new InventorySlotData { itemId = item.ItemId, quantity = quantity, gridX = x, gridY = y };
        }

        private static InventorySlotData Plain(ItemDefinition item, int quantity)
        {
            return new InventorySlotData { itemId = item.ItemId, quantity = quantity };
        }

        private InventoryManager NewBag(int columns = InventoryManager.DefaultColumns, int rows = InventoryManager.DefaultRows)
        {
            RegisterInDatabase();
            var go = new GameObject("BagGridTests_Bag");
            _created.Add(go);
            InventoryManager bag = go.AddComponent<InventoryManager>();
            SetField(bag, "_columns", columns);
            SetField(bag, "_rows", rows);
            SetField(bag, "_slots", null);
            return bag;
        }

        private void RegisterInDatabase()
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, new Dictionary<string, ItemDefinition>(_items));
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(target, value);
        }

        private static InventorySaveState Json(InventorySaveState state) => JsonUtility.FromJson<InventorySaveState>(JsonUtility.ToJson(state));

        private static int Index(InventoryManager bag, string itemId)
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

        private static void AssertNoOverlap(InventoryManager bag)
        {
            var unplaced = new List<int>();
            BagGridRules.Build(bag.Columns, bag.Rows, bag.Slots, ItemDatabase.GetById, unplaced);
            CollectionAssert.IsEmpty(unplaced, "every entry has a valid, non-overlapping area");
        }
    }
}
