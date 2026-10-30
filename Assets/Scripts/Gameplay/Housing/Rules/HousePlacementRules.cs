using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// Pure placement validation for "Arrumar a casa" (Housing.md §2.4), Unity-free so the online server can run
    /// the exact same checks (§4.5, §4.7). Footprint math mirrors <c>GridPlacementUtility.GetFootprintBounds</c> /
    /// <c>Overlaps</c> (90/270° swap width and height; touching edges do not overlap); a test keeps the two equal.
    /// <list type="bullet">
    /// <item>Floor, rug and ceiling share the interior grid (0.5 m cells) but each has its own occupancy: a rug
    /// lies under the furniture and never blocks the way.</item>
    /// <item>Wall: per wall a grid of 0.5 m columns × 5 rows; no rotation (mirror instead); doors and windows are
    /// holes, and curtains only go over a window.</item>
    /// <item>Surface: one of the parent furniture's spots, any furniture with <c>SurfaceSlots</c> &gt; 0.</item>
    /// <item>Yard: 1 m cells; the cajá tree is fixed and the cell in front of the porch stays free. Floor items
    /// allowed outside (benches, log tables) use half as many 1 m cells.</item>
    /// <item>Doors: the cells just inside the entrance stay free, and a flood fill from them must reach one free
    /// cell in front of every functional piece (chest, bed, workbench...), so nobody locks themselves out.</item>
    /// <item>Limits: interior (floor + rug + wall + ceiling + surface) and yard item counts per level; extra
    /// chests per level.</item>
    /// </list>
    /// </summary>
    public static class HousePlacementRules
    {
        public static HousePlacement FlagOf(HouseLayer layer)
        {
            switch (layer)
            {
                case HouseLayer.Floor: return HousePlacement.Floor;
                case HouseLayer.Rug: return HousePlacement.Rug;
                case HouseLayer.Wall: return HousePlacement.Wall;
                case HouseLayer.Ceiling: return HousePlacement.Ceiling;
                case HouseLayer.Surface: return HousePlacement.Surface;
                case HouseLayer.Yard: return HousePlacement.Yard;
                default: return HousePlacement.None;
            }
        }

        public static bool IsInterior(HouseLayer layer) => layer != HouseLayer.Yard;

        /// <summary>The layer an item goes to first (the "Encaixe" column of the catalog).</summary>
        public static HouseLayer DefaultLayer(HouseItemDefinition item)
        {
            if (item == null) return HouseLayer.Floor;
            HousePlacement p = item.Placement;
            if ((p & HousePlacement.Floor) != 0) return HouseLayer.Floor;
            if ((p & HousePlacement.Rug) != 0) return HouseLayer.Rug;
            if ((p & HousePlacement.Wall) != 0) return HouseLayer.Wall;
            if ((p & HousePlacement.Ceiling) != 0) return HouseLayer.Ceiling;
            if ((p & HousePlacement.Surface) != 0) return HouseLayer.Surface;
            return HouseLayer.Yard;
        }

        /// <summary>Rotation positions an item has: 90° → 4, 45° → 8, 0 → 1 (does not turn).</summary>
        public static int RotationPositions(HouseItemDefinition item)
        {
            if (item == null || item.RotationStep <= 0) return 1;
            return Math.Max(1, 360 / item.RotationStep);
        }

        public static int NormalizeRotation(int rotation, int positions)
        {
            positions = Math.Max(1, positions);
            return ((rotation % positions) + positions) % positions;
        }

        /// <summary>Next rotation step (Q/E, ↺).</summary>
        public static int Rotate(HouseItemDefinition item, int rotation, int direction)
        {
            int positions = RotationPositions(item);
            return NormalizeRotation(rotation + (direction >= 0 ? 1 : -1), positions);
        }

        /// <summary>Quarter turns of a rotation (45° items: every two steps).</summary>
        public static int QuarterTurns(HouseItemDefinition item, int rotation)
        {
            return item != null && item.RotationStep == 45 ? NormalizeRotation(rotation, 8) / 2 : NormalizeRotation(rotation, 4);
        }

        /// <summary>Unrotated footprint on <paramref name="layer"/>'s grid.</summary>
        public static (int W, int H) BaseFootprint(HouseItemDefinition item, HouseLayer layer)
        {
            if (item == null || layer == HouseLayer.Surface)
            {
                return (1, 1);
            }

            int w = Math.Max(1, item.FootprintX);
            int h = Math.Max(1, item.FootprintY);
            if (layer == HouseLayer.Yard && (item.Placement & HousePlacement.Yard) == 0)
            {
                // An indoor piece allowed outside: its 0.5 m footprint on 1 m cells.
                w = Math.Max(1, (w + 1) / 2);
                h = Math.Max(1, (h + 1) / 2);
            }

            return (w, h);
        }

        /// <summary>Same as <c>GridPlacementUtility.GetFootprintBounds</c>: odd quarter turns swap width and height.</summary>
        public static CellRect FootprintBounds(int x, int y, int width, int height, int quarterTurns)
        {
            int q = ((quarterTurns % 4) + 4) % 4;
            bool swapped = q == 1 || q == 3;
            return new CellRect(x, y, Math.Max(1, swapped ? height : width), Math.Max(1, swapped ? width : height));
        }

        public static CellRect Footprint(HouseItemDefinition item, HouseLayer layer, int x, int y, int rotation)
        {
            (int w, int h) = BaseFootprint(item, layer);
            if (layer == HouseLayer.Wall)
            {
                return new CellRect(x, y, w, h);
            }

            return FootprintBounds(x, y, w, h, QuarterTurns(item, rotation));
        }

        /// <summary>Front direction (grid dx, dy) by quarter turns: an unturned piece faces south (−y, the door / camera).</summary>
        public static (int Dx, int Dy) FrontDirection(int quarterTurns)
        {
            switch (((quarterTurns % 4) + 4) % 4)
            {
                case 1: return (-1, 0);
                case 2: return (0, 1);
                case 3: return (1, 0);
                default: return (0, -1);
            }
        }

        /// <summary>The row of cells touching the front edge of <paramref name="rect"/>.</summary>
        public static IEnumerable<(int X, int Y)> FrontCells(CellRect rect, int quarterTurns)
        {
            (int dx, int dy) = FrontDirection(quarterTurns);
            if (dy != 0)
            {
                int row = dy < 0 ? rect.Y - 1 : rect.YMax;
                for (int x = rect.X; x < rect.XMax; x++) yield return (x, row);
            }
            else
            {
                int column = dx < 0 ? rect.X - 1 : rect.XMax;
                for (int y = rect.Y; y < rect.YMax; y++) yield return (column, y);
            }
        }

        /// <summary>Pieces that are used by walking up to them and so need a free, reachable cell in front.</summary>
        public static bool NeedsFrontAccess(HouseItemDefinition item, HouseLayer layer)
        {
            return item != null && (layer == HouseLayer.Floor || layer == HouseLayer.Yard) && item.Function != HouseItemFunction.None
                && item.Function != HouseItemFunction.Planter && item.Function != HouseItemFunction.Bell && item.Function != HouseItemFunction.Mailbox;
        }

        public static bool IsExtraChest(HouseItemDefinition item) => item != null && item.Function == HouseItemFunction.Chest && item.FunctionParam > 0;

        /// <summary>
        /// Checks <paramref name="request"/> against <paramref name="layout"/>. <paramref name="movingInstanceId"/> &gt; 0:
        /// a move of that instance (it is ignored for overlap and does not count again for the limits).
        /// </summary>
        public static HousePlacementResult Validate(HouseLayout layout, HousePlacementRequest request, int movingInstanceId = 0)
        {
            HouseItemDefinition item = layout.Resolve(request.ItemId);
            if (item == null) return HousePlacementResult.UnknownItem;
            if (!item.Allows(request.Layer)) return HousePlacementResult.WrongLayer;

            HouseLevelSpec spec = layout.Spec;
            if (item.MinHouseLevel > spec.Level) return HousePlacementResult.LevelTooLow;

            int positions = RotationPositions(item);
            if (request.Layer == HouseLayer.Wall ? request.Rotation != 0 : (request.Rotation < 0 || request.Rotation >= positions))
            {
                return HousePlacementResult.RotationNotAllowed;
            }

            HousePlacedData moving = movingInstanceId > 0 ? layout.Find(movingInstanceId) : null;
            if (movingInstanceId > 0 && moving == null) return HousePlacementResult.UnknownInstance;

            bool countsAgain = moving == null || IsInterior(moving.layer) != IsInterior(request.Layer);
            if (countsAgain)
            {
                if (IsInterior(request.Layer) && layout.InteriorCount >= spec.InteriorItemLimit) return HousePlacementResult.InteriorLimitReached;
                if (!IsInterior(request.Layer) && layout.YardCount >= spec.YardItemLimit) return HousePlacementResult.YardLimitReached;
            }

            if (IsExtraChest(item) && (moving == null || !IsExtraChest(layout.Resolve(moving.itemId))) && layout.CountExtraChests() >= spec.MaxExtraChests)
            {
                return HousePlacementResult.TooManyExtraChests;
            }

            switch (request.Layer)
            {
                case HouseLayer.Surface: return ValidateSurface(layout, request, movingInstanceId);
                case HouseLayer.Wall: return ValidateWall(layout, item, request, movingInstanceId);
                default: return ValidateGrid(layout, item, request, movingInstanceId);
            }
        }

        private static HousePlacementResult ValidateSurface(HouseLayout layout, HousePlacementRequest request, int movingInstanceId)
        {
            HousePlacedData parent = layout.Find(request.ParentInstanceId);
            HouseItemDefinition parentItem = parent != null ? layout.Resolve(parent.itemId) : null;
            if (parent == null || parent.instanceId == movingInstanceId || parentItem == null || parent.layer == HouseLayer.Surface
                || request.Slot < 0 || request.Slot >= parentItem.SurfaceSlots)
            {
                return HousePlacementResult.NoSurfaceParent;
            }

            foreach (HousePlacedData other in layout.Placed)
            {
                if (other.instanceId != movingInstanceId && other.layer == HouseLayer.Surface && other.parentInstanceId == parent.instanceId && other.slot == request.Slot)
                {
                    return HousePlacementResult.SurfaceSlotTaken;
                }
            }

            return HousePlacementResult.Ok;
        }

        private static HousePlacementResult ValidateWall(HouseLayout layout, HouseItemDefinition item, HousePlacementRequest request, int movingInstanceId)
        {
            HouseLevelSpec spec = layout.Spec;
            CellRect rect = Footprint(item, HouseLayer.Wall, request.X, request.Y, 0);
            if (!rect.Inside(spec.WallLength(request.Wall), HouseLevelSpec.WallRows)) return HousePlacementResult.OutOfBounds;

            bool curtain = item.Category == HouseItemCategory.Curtain;
            bool overWindow = false;
            foreach (HouseOpening opening in spec.Openings)
            {
                if (opening.Side != request.Wall) continue;
                CellRect hole = opening.WallRect;
                if (curtain && !opening.IsDoor && rect.X >= hole.X && rect.XMax <= hole.XMax)
                {
                    overWindow = true;
                    continue;
                }

                if (rect.Overlaps(hole)) return HousePlacementResult.OverOpening;
            }

            if (curtain && !overWindow) return HousePlacementResult.CurtainNeedsWindow;

            foreach (HousePlacedData other in layout.Placed)
            {
                if (other.instanceId == movingInstanceId || other.layer != HouseLayer.Wall || other.wall != request.Wall) continue;
                CellRect otherRect = Footprint(layout.Resolve(other.itemId), HouseLayer.Wall, other.x, other.y, 0);
                if (rect.Overlaps(otherRect)) return HousePlacementResult.Overlaps;
            }

            return HousePlacementResult.Ok;
        }

        private static HousePlacementResult ValidateGrid(HouseLayout layout, HouseItemDefinition item, HousePlacementRequest request, int movingInstanceId)
        {
            HouseLevelSpec spec = layout.Spec;
            bool yard = request.Layer == HouseLayer.Yard;
            int width = yard ? spec.YardWidth : spec.InteriorWidth;
            int depth = yard ? spec.YardDepth : spec.InteriorDepth;
            CellRect rect = Footprint(item, request.Layer, request.X, request.Y, request.Rotation);
            if (!rect.Inside(width, depth)) return HousePlacementResult.OutOfBounds;

            if (yard)
            {
                if (rect.Overlaps(spec.FixedTree)) return HousePlacementResult.FixedDecoration;
                if (rect.Contains(spec.YardEntranceX, spec.YardEntranceY)) return HousePlacementResult.BlocksDoor;
            }
            else if (request.Layer == HouseLayer.Floor)
            {
                foreach ((int x, int y) in spec.EntranceInsideCells())
                {
                    if (rect.Contains(x, y)) return HousePlacementResult.BlocksDoor;
                }
            }

            foreach (HousePlacedData other in layout.Placed)
            {
                if (other.instanceId == movingInstanceId || other.layer != request.Layer) continue;
                if (rect.Overlaps(layout.FootprintOf(other))) return HousePlacementResult.Overlaps;
            }

            if (request.Layer != HouseLayer.Floor && request.Layer != HouseLayer.Yard)
            {
                return HousePlacementResult.Ok;
            }

            // Access: blocked cells with the candidate in place, then a flood fill from the way in.
            bool[,] blocked = BuildBlocked(layout, request.Layer, movingInstanceId);
            Mark(blocked, rect);
            bool[,] reachable = FloodFill(blocked, StartCells(spec, request.Layer));

            if (NeedsFrontAccess(item, request.Layer) && !AnyReachable(reachable, FrontCells(rect, QuarterTurns(item, request.Rotation))))
            {
                return HousePlacementResult.NeedsFreeFront;
            }

            foreach (HousePlacedData other in layout.Placed)
            {
                if (other.instanceId == movingInstanceId || other.layer != request.Layer) continue;
                HouseItemDefinition otherItem = layout.Resolve(other.itemId);
                if (NeedsFrontAccess(otherItem, other.layer)
                    && !AnyReachable(reachable, FrontCells(layout.FootprintOf(other), QuarterTurns(otherItem, other.rotation))))
                {
                    return HousePlacementResult.BlocksAccess;
                }
            }

            return HousePlacementResult.Ok;
        }

        private static IEnumerable<(int X, int Y)> StartCells(HouseLevelSpec spec, HouseLayer layer)
        {
            if (layer == HouseLayer.Yard)
            {
                yield return (spec.YardEntranceX, spec.YardEntranceY);
                yield break;
            }

            foreach ((int X, int Y) cell in spec.EntranceInsideCells())
            {
                yield return cell;
            }
        }

        private static bool[,] BuildBlocked(HouseLayout layout, HouseLayer layer, int ignoreInstanceId)
        {
            HouseLevelSpec spec = layout.Spec;
            bool yard = layer == HouseLayer.Yard;
            var blocked = new bool[yard ? spec.YardWidth : spec.InteriorWidth, yard ? spec.YardDepth : spec.InteriorDepth];
            if (yard)
            {
                Mark(blocked, spec.FixedTree);
            }

            foreach (HousePlacedData placed in layout.Placed)
            {
                if (placed.instanceId != ignoreInstanceId && placed.layer == layer)
                {
                    Mark(blocked, layout.FootprintOf(placed));
                }
            }

            return blocked;
        }

        private static void Mark(bool[,] grid, CellRect rect)
        {
            for (int x = Math.Max(0, rect.X); x < Math.Min(grid.GetLength(0), rect.XMax); x++)
            {
                for (int y = Math.Max(0, rect.Y); y < Math.Min(grid.GetLength(1), rect.YMax); y++)
                {
                    grid[x, y] = true;
                }
            }
        }

        /// <summary>4-neighbour flood fill over free cells from <paramref name="starts"/> (blocked starts are skipped).</summary>
        public static bool[,] FloodFill(bool[,] blocked, IEnumerable<(int X, int Y)> starts)
        {
            int w = blocked.GetLength(0);
            int h = blocked.GetLength(1);
            var reached = new bool[w, h];
            var queue = new Queue<(int, int)>();
            foreach ((int x, int y) in starts)
            {
                if (x >= 0 && y >= 0 && x < w && y < h && !blocked[x, y] && !reached[x, y])
                {
                    reached[x, y] = true;
                    queue.Enqueue((x, y));
                }
            }

            while (queue.Count > 0)
            {
                (int x, int y) = queue.Dequeue();
                Visit(x + 1, y);
                Visit(x - 1, y);
                Visit(x, y + 1);
                Visit(x, y - 1);
            }

            return reached;

            void Visit(int nx, int ny)
            {
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || blocked[nx, ny] || reached[nx, ny]) return;
                reached[nx, ny] = true;
                queue.Enqueue((nx, ny));
            }
        }

        private static bool AnyReachable(bool[,] reachable, IEnumerable<(int X, int Y)> cells)
        {
            foreach ((int x, int y) in cells)
            {
                if (x >= 0 && y >= 0 && x < reachable.GetLength(0) && y < reachable.GetLength(1) && reachable[x, y])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
