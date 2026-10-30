using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.UI.Housing;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// H1 house state (Housing.md §12.2): the "House" save round trip through JsonUtility, old saves without a house,
    /// the "Um Cantinho Seu" beat, chest capacity with extra chests, the bed, access rules (one house per account,
    /// owner offline = locked), and the small pure helpers of the view and the screen.
    /// </summary>
    public class HouseServiceTests
    {
        private int _forestVictories;

        private HouseService NewHouse() => new HouseService(() => _forestVictories);

        [SetUp]
        public void SetUp() => _forestVictories = 0;

        private static HousePlacedData[] PlaceStarterLayout(HouseService house)
        {
            HouseLayout layout = house.Layout;
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quarto_016", HouseLayer.Floor, 6, 5, 0), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_util_001", HouseLayer.Floor, 0, 7, 0), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 4, 2, 1), out int table));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnSurface("casa_luz_001", table, 2, 5), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnWall("casa_parede_001", HouseWallSide.North, 1, 2, mirrored: true), out _));
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quintal_002", HouseLayer.Yard, 5, 2), out _));
            return layout.CapturePlaced();
        }

        [Test]
        public void NewGame_HasNoHouse_UntilTheQuestBeat()
        {
            HouseService house = NewHouse();
            Assert.IsFalse(house.HasHouse);
            Assert.IsFalse(house.IsOffered, "no button before 3 Forest victories");
            _forestVictories = 2;
            Assert.IsFalse(house.IsOffered);
            _forestVictories = 3;
            Assert.IsTrue(house.IsOffered);
            Assert.IsTrue(house.HasQuestNews, "(!) while Vó Zefa waits");
        }

        [Test]
        public void GrantHouse_GivesTheRewardsAndStarterSet_Once_AndEnteringCompletesTheQuest()
        {
            _forestVictories = 3;
            HouseService house = NewHouse();
            Assert.IsTrue(house.GrantHouse());
            Assert.IsFalse(house.GrantHouse(), "only once");
            Assert.AreEqual(HouseQuestStep.HouseGiven, house.QuestStep);
            foreach (string id in HouseRules.FirstQuestRewards.Concat(HouseRules.StarterItems))
            {
                Assert.AreEqual(1, house.Layout.DepotCount(id), id);
            }

            Assert.IsTrue(house.HasQuestNews, "(!) until the cousin goes in");
            Assert.IsTrue(house.MarkEntered());
            Assert.IsFalse(house.MarkEntered());
            Assert.AreEqual(HouseQuestStep.Completed, house.QuestStep);
            Assert.IsFalse(house.HasQuestNews);
            _forestVictories = 0;
            Assert.IsTrue(house.IsOffered, "an owned house is always offered");
        }

        [Test]
        public void Save_RoundTripsThroughJsonUtility()
        {
            HouseService house = NewHouse();
            house.GrantHouse();
            house.MarkEntered();
            HousePlacedData[] placed = PlaceStarterLayout(house);
            Assert.AreEqual(6, placed.Length);
            Assert.AreEqual(20, house.Chest.SlotCount);
            Assert.AreEqual(1, house.Chest.Store(new InventorySlotData { itemId = "test_unknown_item", quantity = 1 }, 1));
            house.RecordSleep(4);

            string json = JsonUtility.ToJson((HouseSaveState)house.CaptureState());
            var saved = JsonUtility.FromJson<HouseSaveState>(json);
            Assert.AreEqual(HouseRules.LocalAccountId, saved.ownerAccountId, "one house per account");
            Assert.AreEqual(HouseService.DefaultCityId, saved.cityId);

            HouseService loaded = NewHouse();
            loaded.RestoreState(saved);
            Assert.AreEqual(HouseQuestStep.Completed, loaded.QuestStep);
            Assert.AreEqual(4, loaded.LastSleepDay);
            Assert.AreEqual(placed.Length, loaded.Layout.Placed.Count);
            foreach (HousePlacedData original in placed)
            {
                Assert.IsTrue(original.SameAs(loaded.Layout.Find(original.instanceId)), $"{original.itemId} #{original.instanceId}");
            }

            CollectionAssert.AreEqual(house.Layout.CaptureDepot().Select(e => e.itemId + "x" + e.quantity), loaded.Layout.CaptureDepot().Select(e => e.itemId + "x" + e.quantity));
            Assert.AreEqual(1, loaded.Chest.GetTotalCount("test_unknown_item"));
            Assert.AreEqual(house.Layout.NextInstanceId, loaded.Layout.NextInstanceId);
            Assert.AreEqual(json, JsonUtility.ToJson((HouseSaveState)loaded.CaptureState()), "capture → restore → capture is stable");
        }

        [Test]
        public void Load_MovesWhatNoLongerFitsToTheDepot_NeverLosesIt()
        {
            HouseService house = NewHouse();
            house.GrantHouse();
            var state = (HouseSaveState)house.CaptureState();
            state.placed = new[]
            {
                new HousePlacedData { instanceId = 1, itemId = "casa_util_001", layer = HouseLayer.Floor, x = 0, y = 7 },
                new HousePlacedData { instanceId = 2, itemId = "casa_sala_008", layer = HouseLayer.Floor, x = 0, y = 7 }, // overlaps
                new HousePlacedData { instanceId = 3, itemId = "casa_luz_001", layer = HouseLayer.Surface, parentInstanceId = 99 }, // no parent
                new HousePlacedData { instanceId = 4, itemId = "casa_removed_999", layer = HouseLayer.Floor, x = 5, y = 5 }, // unknown id
            };
            int depotBefore = state.depot.Where(e => e.itemId == "casa_sala_008").Sum(e => e.quantity);

            HouseService loaded = NewHouse();
            loaded.RestoreState(state);
            Assert.AreEqual(1, loaded.Layout.Placed.Count);
            Assert.AreEqual(depotBefore + 1, loaded.Layout.DepotCount("casa_sala_008"));
            Assert.AreEqual(1, loaded.Layout.DepotCount("casa_removed_999"), "unknown ids wait in the depot");
            Assert.GreaterOrEqual(loaded.Layout.NextInstanceId, 2);
        }

        [Test]
        public void OldSave_WithoutAHouseEntry_LoadsWithoutAHouse()
        {
            // SaveManager resets every registered system and only restores the entries a save has: a save from
            // before houses existed never calls RestoreState, so the reset state is what the player gets.
            _forestVictories = 5;
            HouseService house = NewHouse();
            house.GrantHouse();
            house.ResetForNewSession();
            Assert.IsFalse(house.HasHouse);
            Assert.AreEqual(0, house.Layout.Placed.Count);
            Assert.AreEqual(0, house.Layout.DepotEntryCount);
            Assert.IsTrue(house.IsOffered, "with 3+ Forest victories the quest beat comes right away");

            house.RestoreState(new HouseSaveState());
            Assert.IsFalse(house.HasHouse, "a default state is no house");
        }

        [Test]
        public void ExtraChest_AddsSlots_AndCannotBePutAwayWhileItsSlotsAreInUse()
        {
            HouseService house = NewHouse();
            house.GrantHouse();
            house.Layout.AddToDepot("casa_util_002");
            Assert.AreEqual(HousePlacementResult.Ok, house.Layout.Place(HousePlacementRequest.At("casa_util_002", HouseLayer.Floor, 4, 7, 0), out int extra));
            Assert.AreEqual(30, house.Chest.SlotCount, "20 + couro 10");

            for (int i = 0; i < 25; i++)
            {
                Assert.AreEqual(1, house.Chest.Store(new InventorySlotData { itemId = "test_piece_" + i, quantity = 1 }, 1));
            }

            Assert.IsFalse(house.TryStoreInDepot(extra), "25 used > 20 after");
            for (int i = 0; i < 5; i++) house.Chest.RemoveFromSlot(i, 1);
            Assert.IsTrue(house.TryStoreInDepot(extra));
            Assert.AreEqual(20, house.Chest.SlotCount);
            Assert.AreEqual(20, house.Chest.UsedSlotCount, "everything kept");
        }

        [Test]
        public void Bed_SleepsUntilSevenInTheMorning()
        {
            Assert.AreEqual(1f, HouseRules.HoursUntilMorning(6f), 1e-4f);
            Assert.AreEqual(9f, HouseRules.HoursUntilMorning(22f), 1e-4f);
            Assert.AreEqual(24f, HouseRules.HoursUntilMorning(7f), 1e-4f, "at 7:00 sharp: the next morning");
            Assert.AreEqual(16f, HouseRules.HoursUntilMorning(15f), 1e-4f);
        }

        [Test]
        public void Access_OwnerAlways_OthersOnlyWithTheOwnerOnline_NoVisitorOpensAChest()
        {
            Assert.IsTrue(HouseRules.CanEnter(isOwner: true, ownerOnline: false, invitedOrListed: false));
            Assert.IsFalse(HouseRules.CanEnter(false, ownerOnline: false, invitedOrListed: true), "owner offline: locked even for the house list");
            Assert.IsTrue(HouseRules.CanEnter(false, ownerOnline: true, invitedOrListed: true));
            Assert.IsFalse(HouseRules.CanEnter(false, ownerOnline: true, invitedOrListed: false));
            Assert.IsTrue(HouseRules.CanOpenChest(true));
            Assert.IsFalse(HouseRules.CanOpenChest(false));
        }

        [Test]
        public void Levels_AreTunableData_WithProvisionalPrices()
        {
            Assert.AreEqual(5, HouseLevels.All.Count);
            Assert.AreEqual(new[] { 0, 150, 400, 1000, 2500 }, HouseLevels.All.Select(l => l.UpgradeGold).ToArray());
            Assert.AreSame(HouseLevels.Get(1), HouseLevels.Get(0), "clamped");
            Assert.AreSame(HouseLevels.Get(5), HouseLevels.Get(9));
            foreach (HouseLevelSpec spec in HouseLevels.All)
            {
                Assert.IsNotNull(spec.EntranceDoor, $"N{spec.Level} has a way in");
                Assert.IsFalse(spec.FixedTree.Contains(spec.YardEntranceX, spec.YardEntranceY));
            }
        }

        // ------------------------------------------------------------------
        // View / screen helpers
        // ------------------------------------------------------------------

        [Test]
        public void Cutaway_HidesTheWallFacingTheCamera()
        {
            Assert.IsTrue(HouseInteriorView.IsWallCutAway(HouseWallSide.South, 0));
            Assert.IsFalse(HouseInteriorView.IsWallCutAway(HouseWallSide.North, 0));
            Assert.IsTrue(HouseInteriorView.IsWallCutAway(HouseWallSide.West, 1));
            Assert.IsTrue(HouseInteriorView.IsWallCutAway(HouseWallSide.North, 2));
            Assert.IsTrue(HouseInteriorView.IsWallCutAway(HouseWallSide.East, 3));
            Assert.IsTrue(HouseInteriorView.IsWallCutAway(HouseWallSide.East, -1));
        }

        [Test]
        public void Screen_Helpers()
        {
            Assert.AreEqual(new Vector2Int(4, 5), HouseScreenController.CenteredOrigin(new Vector2Int(5, 5), 3, 1));
            Assert.AreEqual(new Vector2Int(5, 4), HouseScreenController.CenteredOrigin(new Vector2Int(5, 5), 2, 2));
            HouseItemDefinition bench = HouseCatalog.Get("casa_sala_002");
            Assert.AreEqual(HouseLayer.Yard, HouseScreenController.LayerFor(bench, HouseLayer.Yard), "a bench may go outside");
            Assert.AreEqual(HouseLayer.Floor, HouseScreenController.LayerFor(bench, HouseLayer.Wall));
            Assert.IsTrue(HouseScreenController.ClickConfirms(touch: false));
            Assert.IsFalse(HouseScreenController.ClickConfirms(touch: true), "a tap only moves the ghost; ✓ places");

            Assert.IsTrue(MyHouseButton.ShouldShow(true, true, false, false, Rapadura.Gameplay.World.Biomes.BiomeType.Forest, true));
            Assert.IsFalse(MyHouseButton.ShouldShow(true, true, false, false, Rapadura.Gameplay.World.Biomes.BiomeType.Desert, true), "only in the Vila");
            Assert.IsFalse(MyHouseButton.ShouldShow(true, true, false, false, Rapadura.Gameplay.World.Biomes.BiomeType.Forest, false), "not before the quest beat");
            Assert.IsFalse(MyHouseButton.ShouldShow(true, false, false, false, Rapadura.Gameplay.World.Biomes.BiomeType.Forest, true), "token travelling");
            Assert.IsFalse(MyHouseButton.ShouldShow(true, true, true, false, Rapadura.Gameplay.World.Biomes.BiomeType.Forest, true), "paused");
        }

        [Test]
        public void Hull_StaysWithinTheN1Budget_AndFurnitureWithinItsRarityBudget()
        {
            HouseHullMeshes hull = HouseHullBuilder.Build(HouseLevels.Get(1));
            try
            {
                Assert.LessOrEqual(hull.TriangleCount, 2500, "N1 hull ≤ 2,500 triangles (§3.5)");
                Assert.Greater(hull.Walls.Sum(w => (int)w.GetIndexCount(0)), 0);
            }
            finally
            {
                Object.DestroyImmediate(hull.Base);
                Object.DestroyImmediate(hull.Roof);
                foreach (Mesh wall in hull.Walls) Object.DestroyImmediate(wall);
            }

            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                var builder = new HouseMeshBuilder();
                HouseFurnitureMeshes.Build(builder, item, HousePlacementRules.DefaultLayer(item));
                Assert.Greater(builder.TriangleCount, 0, item.ItemId);
                Assert.LessOrEqual(builder.TriangleCount, 300, $"{item.ItemId}: even the stand-ins stay at the Common budget");
            }
        }

        [Test]
        public void Palette_UvsRoundTrip()
        {
            foreach (HouseSwatch swatch in System.Enum.GetValues(typeof(HouseSwatch)))
            {
                Assert.AreEqual(swatch, HousePalette.SwatchAt(HousePalette.UvOf(swatch)));
            }
        }

        [Test]
        public void Texts_EveryPlacementResultHasAMessage()
        {
            var keys = HouseTexts.Entries.Select(e => e.Key).ToHashSet();
            foreach (HousePlacementResult result in System.Enum.GetValues(typeof(HousePlacementResult)))
            {
                Assert.IsTrue(keys.Contains(HouseTexts.ResultKey(result)), result.ToString());
            }

            foreach (HouseLevelSpec spec in HouseLevels.All)
            {
                Assert.IsTrue(keys.Contains(spec.NameKey), spec.NameKey);
            }

            Assert.AreEqual("Minha casa", HouseTexts.Get(null, HouseTexts.Button));
            Assert.AreEqual("Um Cantinho Seu", HouseTexts.Get(null, HouseTexts.QuestTitle));
        }
    }
}
