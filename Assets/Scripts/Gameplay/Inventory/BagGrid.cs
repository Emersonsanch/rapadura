using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>Width × height of an item on the backpack grid, in cells.</summary>
    [Serializable]
    public readonly struct GridSize : IEquatable<GridSize>
    {
        public readonly int Width;
        public readonly int Height;

        public GridSize(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
        }

        public int Area => Width * Height;

        public bool Equals(GridSize other) => Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is GridSize other && Equals(other);
        public override int GetHashCode() => Width * 397 ^ Height;
        public override string ToString() => $"{Width}x{Height}";
    }

    /// <summary>An area on the backpack grid: top-left cell (<see cref="X"/>, <see cref="Y"/>) plus a size.</summary>
    public readonly struct GridRect
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public GridRect(int x, int y, GridSize size) : this(x, y, size.Width, size.Height)
        {
        }

        public GridRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
        }

        public bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;

        public override string ToString() => $"({X},{Y}) {Width}x{Height}";
    }

    /// <summary>
    /// How many cells an item takes in the Mochila (MU Online style). The size comes from the item's
    /// <see cref="ItemCategory"/> (<see cref="ForCategory"/>); <see cref="ItemDefinition.GridWidthOverride"/> /
    /// <see cref="ItemDefinition.GridHeightOverride"/> (&gt; 0) win per item. Always clamped to the grid, so nothing
    /// can be bigger than the bag. Unknown items (no definition) take 1×1.
    /// </summary>
    public static class ItemFootprint
    {
        public static readonly GridSize OneByOne = new GridSize(1, 1);

        /// <summary>Suggested override for shields (no shield category yet): 2×2.</summary>
        public static readonly GridSize Shield = new GridSize(2, 2);

        /// <summary>The category default (Docs/Balance/README.md, "Mochila em grade").</summary>
        public static GridSize ForCategory(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.MeleeWeapon: return new GridSize(1, 3);   // espada
                case ItemCategory.RangedWeapon: return new GridSize(2, 4);  // arco
                case ItemCategory.MagicWeapon: return new GridSize(1, 4);   // cajado
                case ItemCategory.Head: return new GridSize(2, 2);          // chapéu / elmo
                case ItemCategory.Chest: return new GridSize(2, 3);         // gibão / armadura
                case ItemCategory.Legs: return new GridSize(2, 2);
                case ItemCategory.Boots: return new GridSize(2, 2);
                case ItemCategory.GatheringTool: return new GridSize(1, 3); // machado, picareta
                case ItemCategory.BuildingTool: return new GridSize(1, 2);  // martelo
                default: return OneByOne;                                   // materiais, poções, comida, anel, ouro, missão
            }
        }

        public static GridSize For(ItemDefinition item)
        {
            if (item == null)
            {
                return OneByOne;
            }

            GridSize byCategory = ForCategory(item.Category);
            int width = item.GridWidthOverride > 0 ? item.GridWidthOverride : byCategory.Width;
            int height = item.GridHeightOverride > 0 ? item.GridHeightOverride : byCategory.Height;
            return new GridSize(width, height);
        }

        /// <summary><see cref="For(ItemDefinition)"/> clamped to a <paramref name="columns"/> × <paramref name="rows"/> grid.</summary>
        public static GridSize For(ItemDefinition item, int columns, int rows)
        {
            GridSize size = For(item);
            return new GridSize(Mathf.Min(size.Width, Mathf.Max(1, columns)), Mathf.Min(size.Height, Mathf.Max(1, rows)));
        }
    }

    /// <summary>
    /// Cell occupancy of a backpack grid: each cell holds the index of the bag entry covering it, or -1.
    /// Plain data, no Unity lifecycle (EditMode-tested through <see cref="BagGridRules"/>).
    /// Scan order everywhere is MU's: top-left first, left to right, then down.
    /// </summary>
    public sealed class BagGrid
    {
        public const int Empty = -1;

        private readonly int[] _cells;

        public BagGrid(int columns, int rows)
        {
            Columns = Mathf.Max(1, columns);
            Rows = Mathf.Max(1, rows);
            _cells = new int[Columns * Rows];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = Empty;
            }
        }

        private BagGrid(BagGrid source)
        {
            Columns = source.Columns;
            Rows = source.Rows;
            _cells = (int[])source._cells.Clone();
        }

        public int Columns { get; }
        public int Rows { get; }
        public int CellCount => _cells.Length;

        public int FreeCellCount
        {
            get
            {
                int free = 0;
                foreach (int cell in _cells)
                {
                    if (cell == Empty)
                    {
                        free++;
                    }
                }

                return free;
            }
        }

        public BagGrid Clone() => new BagGrid(this);

        /// <summary>The entry covering cell (<paramref name="x"/>, <paramref name="y"/>), or <see cref="Empty"/> (also outside the grid).</summary>
        public int At(int x, int y) => x >= 0 && y >= 0 && x < Columns && y < Rows ? _cells[y * Columns + x] : Empty;

        public bool InBounds(GridRect rect) => rect.X >= 0 && rect.Y >= 0 && rect.X + rect.Width <= Columns && rect.Y + rect.Height <= Rows;

        /// <summary>True when <paramref name="rect"/> is inside the grid and every cell is empty (or held by <paramref name="ignoreId"/>).</summary>
        public bool IsFree(GridRect rect, int ignoreId = Empty)
        {
            if (!InBounds(rect))
            {
                return false;
            }

            for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            {
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    int cell = _cells[y * Columns + x];
                    if (cell != Empty && cell != ignoreId)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>Marks <paramref name="rect"/> as held by <paramref name="id"/>. False (nothing changes) unless it is free.</summary>
        public bool TryOccupy(int id, GridRect rect)
        {
            if (id < 0 || !IsFree(rect, id))
            {
                return false;
            }

            for (int y = rect.Y; y < rect.Y + rect.Height; y++)
            {
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    _cells[y * Columns + x] = id;
                }
            }

            return true;
        }

        /// <summary>Frees every cell held by <paramref name="id"/>.</summary>
        public void Release(int id)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == id)
                {
                    _cells[i] = Empty;
                }
            }
        }

        /// <summary>First free <paramref name="size"/> area in MU order (row by row from the top, left to right).</summary>
        public bool TryFindFirstFit(GridSize size, out int x, out int y)
        {
            for (y = 0; y + size.Height <= Rows; y++)
            {
                for (x = 0; x + size.Width <= Columns; x++)
                {
                    if (IsFree(new GridRect(x, y, size)))
                    {
                        return true;
                    }
                }
            }

            x = -1;
            y = -1;
            return false;
        }

        /// <summary>Distinct entries (other than <paramref name="ignoreId"/>) covering any cell of <paramref name="rect"/>, in scan order.</summary>
        public List<int> IdsIn(GridRect rect, int ignoreId = Empty)
        {
            var ids = new List<int>();
            for (int y = Mathf.Max(0, rect.Y); y < Mathf.Min(Rows, rect.Y + rect.Height); y++)
            {
                for (int x = Mathf.Max(0, rect.X); x < Mathf.Min(Columns, rect.X + rect.Width); x++)
                {
                    int cell = _cells[y * Columns + x];
                    if (cell != Empty && cell != ignoreId && !ids.Contains(cell))
                    {
                        ids.Add(cell);
                    }
                }
            }

            return ids;
        }
    }

    /// <summary>What dropping a bag item on a grid cell does (<see cref="BagGridRules.PlanMove"/>).</summary>
    public enum BagMoveKind
    {
        /// <summary>Does not fit there (outside the grid, two or more items in the way, or the swap partner has nowhere to go).</summary>
        Invalid,
        /// <summary>Dropped where it already is.</summary>
        Unchanged,
        /// <summary>The area is free: the item moves.</summary>
        Move,
        /// <summary>Lands on a plain stack of the same item with room: units merge into it.</summary>
        Merge,
        /// <summary>Lands on exactly one other item: they trade places (the other goes to the old spot, or the first free area).</summary>
        Swap,
    }

    /// <summary>Result of <see cref="BagGridRules.PlanMove"/>: what happens and, for a swap, where the other item ends up.</summary>
    public readonly struct BagMovePlan
    {
        public readonly BagMoveKind Kind;
        public readonly int OtherIndex;
        public readonly int OtherX;
        public readonly int OtherY;

        public BagMovePlan(BagMoveKind kind, int otherIndex = -1, int otherX = -1, int otherY = -1)
        {
            Kind = kind;
            OtherIndex = otherIndex;
            OtherX = otherX;
            OtherY = otherY;
        }

        public bool IsValid => Kind != BagMoveKind.Invalid;

        public static BagMovePlan Invalid => new BagMovePlan(BagMoveKind.Invalid);
    }

    /// <summary>
    /// Pure placement rules of the MU-style Mochila (EditMode-tested): building the occupancy from bag entries, the
    /// shared "does it fit?" simulation (<see cref="BagFitSimulator"/>), moving/merging/swapping on a cell, packing a
    /// list for "Organizar", and laying out a restored (possibly old, pre-grid) save. Entries are
    /// <see cref="InventorySlotData"/>s whose <see cref="InventorySlotData.gridX"/>/<see cref="InventorySlotData.gridY"/> hold
    /// their top-left cell; the entry index is the id stored in <see cref="BagGrid"/>. No rotation (MU doesn't rotate either).
    /// </summary>
    public static class BagGridRules
    {
        public static GridSize SizeOf(InventorySlotData slot, Func<string, ItemDefinition> resolve, int columns, int rows)
        {
            ItemDefinition item = slot == null || slot.IsEmpty ? null : resolve?.Invoke(slot.itemId);
            return ItemFootprint.For(item, columns, rows);
        }

        public static GridRect RectOf(InventorySlotData slot, Func<string, ItemDefinition> resolve, int columns, int rows)
        {
            return new GridRect(slot.gridX, slot.gridY, SizeOf(slot, resolve, columns, rows));
        }

        /// <summary>
        /// Occupancy of <paramref name="slots"/> (entry index = id). Entries are stamped in index order; one that is out of
        /// bounds or overlaps an earlier one is not stamped and is listed in <paramref name="unplaced"/> (if given).
        /// </summary>
        public static BagGrid Build(int columns, int rows, IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve, List<int> unplaced = null)
        {
            var grid = new BagGrid(columns, rows);
            if (slots == null)
            {
                return grid;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (!grid.TryOccupy(i, RectOf(slot, resolve, grid.Columns, grid.Rows)))
                {
                    unplaced?.Add(i);
                }
            }

            return grid;
        }

        /// <summary>
        /// What dropping entry <paramref name="index"/> with its top-left on (<paramref name="x"/>, <paramref name="y"/>) does.
        /// Never changes <paramref name="grid"/>.
        /// </summary>
        public static BagMovePlan PlanMove(BagGrid grid, IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve, int index, int x, int y)
        {
            if (grid == null || slots == null || index < 0 || index >= slots.Count || slots[index] == null || slots[index].IsEmpty)
            {
                return BagMovePlan.Invalid;
            }

            InventorySlotData moving = slots[index];
            GridSize size = SizeOf(moving, resolve, grid.Columns, grid.Rows);
            var target = new GridRect(x, y, size);

            if (!grid.InBounds(target))
            {
                return BagMovePlan.Invalid;
            }

            if (moving.gridX == x && moving.gridY == y)
            {
                return new BagMovePlan(BagMoveKind.Unchanged);
            }

            List<int> blockers = grid.IdsIn(target, index);
            if (blockers.Count == 0)
            {
                return new BagMovePlan(BagMoveKind.Move);
            }

            if (blockers.Count > 1)
            {
                return BagMovePlan.Invalid;
            }

            int other = blockers[0];
            InventorySlotData otherSlot = slots[other];

            if (CanMerge(moving, otherSlot, resolve))
            {
                return new BagMovePlan(BagMoveKind.Merge, other, otherSlot.gridX, otherSlot.gridY);
            }

            // Swap: the moving item takes the target, the other goes back to where the moving one was, else the first free area.
            BagGrid trial = grid.Clone();
            trial.Release(index);
            trial.Release(other);
            if (!trial.TryOccupy(index, target))
            {
                return BagMovePlan.Invalid;
            }

            GridSize otherSize = SizeOf(otherSlot, resolve, grid.Columns, grid.Rows);
            var oldSpot = new GridRect(moving.gridX, moving.gridY, otherSize);
            if (trial.IsFree(oldSpot))
            {
                return new BagMovePlan(BagMoveKind.Swap, other, oldSpot.X, oldSpot.Y);
            }

            return trial.TryFindFirstFit(otherSize, out int ox, out int oy)
                ? new BagMovePlan(BagMoveKind.Swap, other, ox, oy)
                : BagMovePlan.Invalid;
        }

        /// <summary>Plain stacks of the same stackable item merge (never a generated roll: its affixes would be lost).</summary>
        public static bool CanMerge(InventorySlotData from, InventorySlotData to, Func<string, ItemDefinition> resolve)
        {
            if (from == null || to == null || from.IsEmpty || to.IsEmpty || from.itemId != to.itemId || from.HasGeneratedData || to.HasGeneratedData)
            {
                return false;
            }

            ItemDefinition item = resolve?.Invoke(from.itemId);
            return item != null && item.IsStackable && to.quantity < item.MaxStack;
        }

        /// <summary>
        /// Places <paramref name="sizes"/> in order, each at the first free area of an empty <paramref name="columns"/> ×
        /// <paramref name="rows"/> grid. False (and null rects) if one does not fit.
        /// </summary>
        public static bool TryPack(int columns, int rows, IReadOnlyList<GridSize> sizes, out GridRect[] rects)
        {
            var grid = new BagGrid(columns, rows);
            rects = new GridRect[sizes?.Count ?? 0];

            for (int i = 0; i < rects.Length; i++)
            {
                if (!grid.TryFindFirstFit(sizes[i], out int x, out int y))
                {
                    rects = null;
                    return false;
                }

                rects[i] = new GridRect(x, y, sizes[i]);
                grid.TryOccupy(i, rects[i]);
            }

            return true;
        }

        /// <summary>
        /// "Organizar" packing: <paramref name="sizes"/> in the given (sorted) order; if that leaves a piece out, biggest
        /// first (stable). False when neither fits — the caller keeps the current layout.
        /// </summary>
        public static bool TryPackForSort(int columns, int rows, IReadOnlyList<GridSize> sizes, out GridRect[] rects)
        {
            if (TryPack(columns, rows, sizes, out rects))
            {
                return true;
            }

            int[] order = BiggestFirst(sizes);
            var ordered = new GridSize[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                ordered[i] = sizes[order[i]];
            }

            if (!TryPack(columns, rows, ordered, out GridRect[] packed))
            {
                rects = null;
                return false;
            }

            rects = new GridRect[order.Length];
            for (int i = 0; i < order.Length; i++)
            {
                rects[order[i]] = packed[i];
            }

            return true;
        }

        /// <summary>
        /// Lays out restored entries in place (sets their grid position) and returns the indices that do not fit.
        /// <paramref name="keepPositions"/> (a save written by the grid bag): entries whose saved area is valid stay put,
        /// in index order. Everything else — every entry of an old pre-grid save — is auto-placed MU-style, currency and
        /// quest items first, then the biggest pieces first (fewest leftovers), ties by index. The caller sends the
        /// leftovers to the Baú da Vó Edna (<see cref="VillageChestStorage.EnqueueOverflow"/>), never loses them.
        /// </summary>
        public static List<int> LayoutForRestore(int columns, int rows, IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve, bool keepPositions)
        {
            var overflow = new List<int>();
            if (slots == null)
            {
                return overflow;
            }

            var grid = new BagGrid(columns, rows);
            var pending = new List<int>();

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (keepPositions && grid.TryOccupy(i, RectOf(slot, resolve, grid.Columns, grid.Rows)))
                {
                    continue;
                }

                pending.Add(i);
            }

            pending.Sort((a, b) =>
            {
                int byPriority = PlacementPriority(slots[a], resolve).CompareTo(PlacementPriority(slots[b], resolve));
                if (byPriority != 0)
                {
                    return byPriority;
                }

                int byArea = SizeOf(slots[b], resolve, grid.Columns, grid.Rows).Area.CompareTo(SizeOf(slots[a], resolve, grid.Columns, grid.Rows).Area);
                return byArea != 0 ? byArea : a.CompareTo(b);
            });

            foreach (int index in pending)
            {
                GridSize size = SizeOf(slots[index], resolve, grid.Columns, grid.Rows);
                if (grid.TryFindFirstFit(size, out int x, out int y))
                {
                    slots[index].gridX = x;
                    slots[index].gridY = y;
                    grid.TryOccupy(index, new GridRect(x, y, size));
                }
                else
                {
                    overflow.Add(index);
                }
            }

            overflow.Sort();
            return overflow;
        }

        /// <summary>0 = must stay in the bag if at all possible (gold, quest/story items), 1 = everything else.</summary>
        private static int PlacementPriority(InventorySlotData slot, Func<string, ItemDefinition> resolve)
        {
            ItemDefinition item = resolve?.Invoke(slot.itemId);
            return item != null && (item.Type == ItemType.Currency || item.Type == ItemType.QuestItem || item.Category == ItemCategory.QuestItem) ? 0 : 1;
        }

        private static int[] BiggestFirst(IReadOnlyList<GridSize> sizes)
        {
            var order = new List<int>();
            for (int i = 0; i < sizes.Count; i++)
            {
                order.Add(i);
            }

            order.Sort((a, b) =>
            {
                int byArea = sizes[b].Area.CompareTo(sizes[a].Area);
                if (byArea != 0)
                {
                    return byArea;
                }

                int byHeight = sizes[b].Height.CompareTo(sizes[a].Height);
                return byHeight != 0 ? byHeight : a.CompareTo(b);
            });

            return order.ToArray();
        }
    }

    /// <summary>
    /// The shared "does it fit?" simulation: a copy of the bag's occupancy plus the room left in its plain stacks and the
    /// free entries. <see cref="Add"/> places like <see cref="InventoryManager.AddItem(ItemDefinition,int)"/> does — fill
    /// plain stacks of the same id first, then new stacks of up to <see cref="ItemDefinition.MaxStack"/> at the first free
    /// area — without touching the bag, so several additions can be checked together (a reward bundle, a craft's output).
    /// </summary>
    public sealed class BagFitSimulator
    {
        private readonly BagGrid _grid;
        private readonly Func<string, ItemDefinition> _resolve;
        private readonly Dictionary<string, int> _stackRoom = new Dictionary<string, int>();
        private int _freeEntries;
        private int _nextVirtualId;

        public BagFitSimulator(BagGrid grid, IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve)
        {
            _grid = grid != null ? grid.Clone() : new BagGrid(1, 1);
            _resolve = resolve;
            _nextVirtualId = slots?.Count ?? 0;

            if (slots == null)
            {
                return;
            }

            foreach (InventorySlotData slot in slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    _freeEntries++;
                    continue;
                }

                ItemDefinition item = resolve?.Invoke(slot.itemId);
                if (item == null || !item.IsStackable || slot.HasGeneratedData || slot.quantity >= item.MaxStack)
                {
                    continue;
                }

                _stackRoom.TryGetValue(slot.itemId, out int room);
                _stackRoom[slot.itemId] = room + (item.MaxStack - slot.quantity);
            }
        }

        /// <summary>
        /// Adds up to <paramref name="quantity"/> units of <paramref name="item"/> to the simulation; returns how many fit.
        /// <paramref name="asPiece"/>: a single generated roll / stored piece (never stacks, one entry).
        /// </summary>
        public int Add(ItemDefinition item, int quantity, bool asPiece = false)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;

            if (!asPiece && item.IsStackable && _stackRoom.TryGetValue(item.ItemId, out int room) && room > 0)
            {
                int used = Mathf.Min(room, remaining);
                _stackRoom[item.ItemId] = room - used;
                remaining -= used;
            }

            GridSize size = ItemFootprint.For(item, _grid.Columns, _grid.Rows);
            int perEntry = asPiece ? quantity : item.MaxStack;

            while (remaining > 0 && _freeEntries > 0 && _grid.TryFindFirstFit(size, out int x, out int y))
            {
                _grid.TryOccupy(_nextVirtualId++, new GridRect(x, y, size));
                _freeEntries--;

                int placed = Mathf.Min(perEntry, remaining);
                remaining -= placed;

                if (!asPiece && item.IsStackable && placed < item.MaxStack)
                {
                    _stackRoom.TryGetValue(item.ItemId, out int left);
                    _stackRoom[item.ItemId] = left + (item.MaxStack - placed);
                }
            }

            return quantity - remaining;
        }
    }
}
