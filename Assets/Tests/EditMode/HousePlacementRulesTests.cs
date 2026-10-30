using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// H1 placement rules (Housing.md §2.4), pure: grid bounds, rotation, overlap, rugs under furniture, walls with
    /// doors/windows/curtains, ceiling, surface spots, the yard (cajá tree, entrance), the door flood fill and the
    /// per-level limits. Uses synthetic items so each rule is tested on its own.
    /// </summary>
    public class HousePlacementRulesTests
    {
        private Dictionary<string, HouseItemDefinition> _items;
        private HouseLayout _layout;

        private static HouseItemDefinition Def(string id, HousePlacement placement, int w, int h, int rotationStep,
            int surfaceSlots = 0, HouseItemFunction function = HouseItemFunction.None, int param = 0,
            HouseItemCategory category = HouseItemCategory.LivingRoom, int minLevel = 1)
        {
            return new HouseItemDefinition(id, id, category, ItemRarity.Common, placement, w, h, rotationStep, surfaceSlots, function, param, 1,
                string.Empty, 10, 0, true, true, false, minLevel, HouseSeason.None, new HouseItemSource(HouseItemSourceType.Shop, "shop_test"));
        }

        [SetUp]
        public void SetUp()
        {
            _items = new[]
            {
                Def("box", HousePlacement.Floor, 2, 2, 90),
                Def("long", HousePlacement.Floor, 3, 1, 90),
                Def("chest", HousePlacement.Floor, 2, 1, 90, function: HouseItemFunction.Chest),
                Def("xchest", HousePlacement.Floor, 2, 1, 90, function: HouseItemFunction.Chest, param: 10),
                Def("table", HousePlacement.Floor, 3, 2, 90, surfaceSlots: 4),
                Def("rug", HousePlacement.Rug, 4, 3, 90, category: HouseItemCategory.Rug),
                Def("pic", HousePlacement.Wall, 2, 2, 0, category: HouseItemCategory.WallDecor),
                Def("curtain", HousePlacement.Wall, 2, 3, 0, category: HouseItemCategory.Curtain),
                Def("lamp", HousePlacement.Ceiling, 1, 1, 90, category: HouseItemCategory.Lighting),
                Def("cup", HousePlacement.Surface, 1, 1, 45),
                Def("tree", HousePlacement.Yard, 2, 2, 90, category: HouseItemCategory.Yard),
                Def("bench", HousePlacement.Floor | HousePlacement.Yard, 3, 1, 90),
                Def("fountain", HousePlacement.Yard, 2, 2, 90, minLevel: 3),
            }.ToDictionary(i => i.ItemId);

            _layout = NewLayout(HouseLevels.Get(1));
        }

        private HouseLayout NewLayout(HouseLevelSpec spec)
        {
            var layout = new HouseLayout(spec, id => _items.TryGetValue(id, out HouseItemDefinition item) ? item : null);
            foreach (string id in _items.Keys) layout.AddToDepot(id, 5);
            return layout;
        }

        private int Place(HousePlacementRequest request)
        {
            Assert.AreEqual(HousePlacementResult.Ok, _layout.Place(request, out int id), request.ItemId);
            return id;
        }

        private HousePlacementResult Check(HousePlacementRequest request) => _layout.CanPlace(request);

        // ------------------------------------------------------------------

        [Test]
        public void Footprint_MatchesGridPlacementUtility_ForEveryRotation()
        {
            for (int turns = -1; turns <= 5; turns++)
            {
                RectInt expected = GridPlacementUtility.GetFootprintBounds(new Vector2Int(2, 3), new Vector2Int(3, 1), turns);
                CellRect actual = HousePlacementRules.FootprintBounds(2, 3, 3, 1, turns);
                Assert.AreEqual(new CellRect(expected.x, expected.y, expected.width, expected.height), actual, $"turns {turns}");
            }

            var a = new CellRect(0, 0, 2, 2);
            Assert.IsFalse(a.Overlaps(new CellRect(2, 0, 1, 1)), "touching edges do not overlap");
            Assert.AreEqual(GridPlacementUtility.Overlaps(new RectInt(0, 0, 2, 2), new RectInt(1, 1, 2, 2)), a.Overlaps(new CellRect(1, 1, 2, 2)));
        }

        [Test]
        public void Floor_InsideTheGrid_Fits_OutsideDoesNot()
        {
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 6, 4)));
            Assert.AreEqual(HousePlacementResult.OutOfBounds, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 9, 4)));
            Assert.AreEqual(HousePlacementResult.OutOfBounds, Check(HousePlacementRequest.At("box", HouseLayer.Floor, -1, 4)));
            Assert.AreEqual(HousePlacementResult.WrongLayer, Check(HousePlacementRequest.At("box", HouseLayer.Wall, 0, 0)));
        }

        [Test]
        public void Rotation_SwapsTheFootprint_AndWallsDoNotTurn()
        {
            // 3×1 along the east wall only fits turned.
            Assert.AreEqual(HousePlacementResult.OutOfBounds, Check(HousePlacementRequest.At("long", HouseLayer.Floor, 9, 3, 0)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("long", HouseLayer.Floor, 9, 3, 1)));
            Assert.AreEqual(HousePlacementResult.RotationNotAllowed, Check(HousePlacementRequest.At("long", HouseLayer.Floor, 5, 3, 4)));

            HousePlacementRequest wall = HousePlacementRequest.OnWall("pic", HouseWallSide.North, 0, 0);
            wall.Rotation = 1;
            Assert.AreEqual(HousePlacementResult.RotationNotAllowed, Check(wall));
            Assert.AreEqual(8, HousePlacementRules.RotationPositions(_items["cup"]), "surface pieces turn in 45° steps");
            Assert.AreEqual(1, HousePlacementRules.RotationPositions(_items["pic"]));
            Assert.AreEqual(0, HousePlacementRules.Rotate(_items["long"], 3, 1), "rotation wraps");
            Assert.AreEqual(3, HousePlacementRules.Rotate(_items["long"], 0, -1));
        }

        [Test]
        public void Floor_Overlap_IsRefused_RugsLieUnderFurniture()
        {
            Place(HousePlacementRequest.At("box", HouseLayer.Floor, 5, 4));
            Assert.AreEqual(HousePlacementResult.Overlaps, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 6, 5)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 7, 4)), "side by side is fine");

            Place(HousePlacementRequest.At("rug", HouseLayer.Rug, 4, 3));
            Assert.AreEqual(HousePlacementResult.Overlaps, Check(HousePlacementRequest.At("rug", HouseLayer.Rug, 5, 4)), "rug over rug");
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("lamp", HouseLayer.Ceiling, 5, 4)), "the ceiling has its own grid");
        }

        [Test]
        public void Door_StaysFree_AndFunctionalPiecesNeedAReachableFront()
        {
            // Entrance: south wall, inside cells (2,0) and (3,0).
            Assert.AreEqual(HousePlacementResult.BlocksDoor, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 2, 0)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("rug", HouseLayer.Rug, 1, 0)), "a doormat-like rug may lie at the door");

            // A chest facing the south outer wall (row 0, front = row -1) cannot be opened.
            Assert.AreEqual(HousePlacementResult.NeedsFreeFront, Check(HousePlacementRequest.At("chest", HouseLayer.Floor, 6, 0, 0)));
            // Turned to face north (rotation 2) it can.
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("chest", HouseLayer.Floor, 6, 0, 2)));

            // Chest against the north wall facing south; then block the cells in front of it.
            Place(HousePlacementRequest.At("chest", HouseLayer.Floor, 4, 7, 0));
            Assert.AreEqual(HousePlacementResult.BlocksAccess, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 4, 5)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 6, 5)));
        }

        [Test]
        public void FloodFill_FindsAPieceWalledOffBehindFurniture()
        {
            // Furniture across the room (rows 4-5) with a gap on the east side; a chest behind it.
            Place(HousePlacementRequest.At("box", HouseLayer.Floor, 0, 4));
            Place(HousePlacementRequest.At("box", HouseLayer.Floor, 2, 4));
            Place(HousePlacementRequest.At("box", HouseLayer.Floor, 4, 4));
            Place(HousePlacementRequest.At("box", HouseLayer.Floor, 6, 4));
            Place(HousePlacementRequest.At("chest", HouseLayer.Floor, 2, 7, 0));
            // Closing the gap would wall the chest off from the door.
            Assert.AreEqual(HousePlacementResult.BlocksAccess, Check(HousePlacementRequest.At("box", HouseLayer.Floor, 8, 4)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("rug", HouseLayer.Rug, 6, 2)), "a rug never blocks the way");

            var blocked = new bool[3, 3];
            blocked[1, 0] = blocked[1, 1] = blocked[1, 2] = true;
            bool[,] reached = HousePlacementRules.FloodFill(blocked, new[] { (0, 0) });
            Assert.IsTrue(reached[0, 2]);
            Assert.IsFalse(reached[2, 0], "the column of blocked cells splits the grid");
        }

        [Test]
        public void Walls_RespectDoorsAndWindows_CurtainsOnlyOverWindows()
        {
            // North window: columns 6-7, rows 2-3. South door: columns 2-3, rows 0-3.
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 1, 2)));
            Assert.AreEqual(HousePlacementResult.OverOpening, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 5, 2)));
            Assert.AreEqual(HousePlacementResult.OverOpening, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.South, 2, 1)));
            Assert.AreEqual(HousePlacementResult.OutOfBounds, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.East, 7, 0)), "east wall is 8 columns");
            Assert.AreEqual(HousePlacementResult.OutOfBounds, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 0, 4)), "5 rows");

            Assert.AreEqual(HousePlacementResult.CurtainNeedsWindow, Check(HousePlacementRequest.OnWall("curtain", HouseWallSide.North, 1, 1)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.OnWall("curtain", HouseWallSide.North, 6, 1)));

            Place(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 1, 2));
            Assert.AreEqual(HousePlacementResult.Overlaps, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 2, 3)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.East, 1, 2)), "each wall has its own grid");
        }

        [Test]
        public void Surface_NeedsAFreeSpotOnAParent_AndGoesBackWithIt()
        {
            Assert.AreEqual(HousePlacementResult.NoSurfaceParent, Check(HousePlacementRequest.OnSurface("cup", 999, 0)));
            int box = Place(HousePlacementRequest.At("box", HouseLayer.Floor, 6, 5));
            Assert.AreEqual(HousePlacementResult.NoSurfaceParent, Check(HousePlacementRequest.OnSurface("cup", box, 0)), "a box offers no spots");

            int table = Place(HousePlacementRequest.At("table", HouseLayer.Floor, 4, 3));
            Assert.AreEqual(HousePlacementResult.NoSurfaceParent, Check(HousePlacementRequest.OnSurface("cup", table, 4)), "4 spots: 0..3");
            int cup = Place(HousePlacementRequest.OnSurface("cup", table, 1, 3));
            Assert.AreEqual(HousePlacementResult.SurfaceSlotTaken, Check(HousePlacementRequest.OnSurface("cup", table, 1)));
            Assert.AreEqual(0, _layout.FreeSurfaceSlot(table));

            int depotCups = _layout.DepotCount("cup");
            Assert.AreEqual(2, _layout.StoreInDepot(table), "the table and the cup on it");
            Assert.IsNull(_layout.Find(cup));
            Assert.AreEqual(depotCups + 1, _layout.DepotCount("cup"));
        }

        [Test]
        public void Yard_KeepsTheCajaTreeAndTheEntrance_IndoorBenchesUseHalfTheCells()
        {
            // Cajá at (0,0) 2×2; entrance (3,7).
            Assert.AreEqual(HousePlacementResult.FixedDecoration, Check(HousePlacementRequest.At("tree", HouseLayer.Yard, 1, 1)));
            Assert.AreEqual(HousePlacementResult.BlocksDoor, Check(HousePlacementRequest.At("tree", HouseLayer.Yard, 3, 6)));
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("tree", HouseLayer.Yard, 5, 2)));
            Assert.AreEqual(HousePlacementResult.LevelTooLow, Check(HousePlacementRequest.At("fountain", HouseLayer.Yard, 5, 2)));

            Assert.AreEqual(new CellRect(0, 0, 2, 1), HousePlacementRules.Footprint(_items["bench"], HouseLayer.Yard, 0, 0, 0), "3×1 of 0.5 m → 2×1 of 1 m");
            Assert.AreEqual(HousePlacementResult.Ok, Check(HousePlacementRequest.At("bench", HouseLayer.Yard, 5, 5)));
            Assert.AreEqual(HousePlacementResult.WrongLayer, Check(HousePlacementRequest.At("box", HouseLayer.Yard, 5, 5)));
        }

        [Test]
        public void Limits_InteriorYardAndExtraChests()
        {
            HouseLevelSpec tight = HouseLevels.Get(1);
            var spec = new HouseLevelSpec
            {
                Level = 1, InteriorWidth = tight.InteriorWidth, InteriorDepth = tight.InteriorDepth, YardWidth = tight.YardWidth, YardDepth = tight.YardDepth,
                InteriorItemLimit = 2, YardItemLimit = 1, ChestSlots = 20, MaxExtraChests = 1, Openings = tight.Openings,
                YardEntranceX = tight.YardEntranceX, YardEntranceY = tight.YardEntranceY, FixedTree = tight.FixedTree,
            };
            _layout = NewLayout(spec);

            Place(HousePlacementRequest.At("xchest", HouseLayer.Floor, 6, 4, 0));
            Assert.AreEqual(HousePlacementResult.TooManyExtraChests, Check(HousePlacementRequest.At("xchest", HouseLayer.Floor, 6, 6, 0)));
            Assert.AreEqual(10, _layout.ExtraChestSlots());
            int lamp = Place(HousePlacementRequest.At("lamp", HouseLayer.Ceiling, 1, 1));
            Assert.AreEqual(HousePlacementResult.InteriorLimitReached, Check(HousePlacementRequest.OnWall("pic", HouseWallSide.North, 0, 0)), "walls and ceiling count too");
            Assert.AreEqual(HousePlacementResult.Ok, _layout.CanMove(lamp, HousePlacementRequest.At("lamp", HouseLayer.Ceiling, 2, 2)), "moving does not count again");

            Place(HousePlacementRequest.At("tree", HouseLayer.Yard, 5, 2));
            Assert.AreEqual(HousePlacementResult.YardLimitReached, Check(HousePlacementRequest.At("tree", HouseLayer.Yard, 5, 5)));

            Assert.AreEqual(40, HouseLevels.Get(1).InteriorItemLimit);
            Assert.AreEqual(15, HouseLevels.Get(1).YardItemLimit);
            Assert.AreEqual(20, HouseLevels.Get(1).ChestSlots);
        }

        [Test]
        public void Move_IgnoresItself_AndPlacingNeedsACopyInTheDepot()
        {
            int box = Place(HousePlacementRequest.At("box", HouseLayer.Floor, 5, 4));
            Assert.AreEqual(HousePlacementResult.Ok, _layout.Move(box, HousePlacementRequest.At("box", HouseLayer.Floor, 6, 4)), "overlapping its old spot is fine");
            Assert.AreEqual(6, _layout.Find(box).x);
            Assert.AreEqual(HousePlacementResult.UnknownInstance, _layout.CanMove(12345, HousePlacementRequest.At("box", HouseLayer.Floor, 0, 4)));

            var empty = new HouseLayout(HouseLevels.Get(1), id => _items.TryGetValue(id, out HouseItemDefinition item) ? item : null);
            Assert.AreEqual(HousePlacementResult.NotInDepot, empty.CanPlace(HousePlacementRequest.At("box", HouseLayer.Floor, 5, 4)));
            Assert.AreEqual(HousePlacementResult.UnknownItem, empty.CanPlace(HousePlacementRequest.At("nope", HouseLayer.Floor, 5, 4)));
        }

        [Test]
        public void Depot_StacksCopies_AndLimitsEntries()
        {
            var layout = new HouseLayout(HouseLevels.Get(1), id => null, depotEntryLimit: 2);
            Assert.IsTrue(layout.AddToDepot("a"));
            Assert.IsTrue(layout.AddToDepot("a", 3));
            Assert.IsTrue(layout.AddToDepot("b"));
            Assert.IsFalse(layout.AddToDepot("c"), "a new id needs a free entry");
            Assert.IsTrue(layout.AddToDepot("b"), "copies always stack");
            Assert.AreEqual(4, layout.DepotCount("a"));
            Assert.AreEqual(2, layout.DepotEntryCount);
            Assert.AreEqual(300, HouseLevels.OfflineDepotEntryLimit);
        }

        [Test]
        public void RealCatalog_StarterSet_CanBePlacedInTheWoodenHouse()
        {
            var layout = new HouseLayout(HouseLevels.Get(1), HouseCatalog.Get);
            foreach (string id in HouseRules.FirstQuestRewards.Concat(HouseRules.StarterItems)) layout.AddToDepot(id);

            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quarto_016", HouseLayer.Floor, 6, 5, 0), out _), "bed");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_util_001", HouseLayer.Floor, 0, 7, 0), out _), "chest");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_sala_004", HouseLayer.Floor, 4, 2, 0), out int table), "table");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnSurface("casa_luz_001", table, 0), out _), "lamp on the table");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_tapete_001", HouseLayer.Rug, 3, 1), out _), "rug");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_planta_001", HouseLayer.Ceiling, 1, 1), out _), "fern");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnWall("casa_parede_001", HouseWallSide.North, 1, 2), out _), "clock");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.OnWall("casa_cortina_001", HouseWallSide.West, 3, 1), out _), "curtain");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_quintal_002", HouseLayer.Yard, 5, 2), out _), "vegetable bed");
            Assert.AreEqual(HousePlacementResult.Ok, layout.Place(HousePlacementRequest.At("casa_curral_001", HouseLayer.Yard, 6, 6, 0), out _), "doghouse");
            Assert.IsTrue(layout.HasPlaced(HouseItemFunction.Bed));
            Assert.IsTrue(layout.HasPlaced(HouseItemFunction.Chest));
        }
    }
}
