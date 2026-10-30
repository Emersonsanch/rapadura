using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>Serializable snapshot of the whole inventory + equipment for the save system.</summary>
    [Serializable]
    public class InventorySaveState
    {
        public InventorySlotData[] slots = Array.Empty<InventorySlotData>();
        public EquippedEntry[] equipped = Array.Empty<EquippedEntry>();

        /// <summary>
        /// 0 = a save from before the MU-style grid (slot list + weight limit): its entries carry no cell positions and
        /// are auto-placed on load. <see cref="InventoryManager.GridSaveVersion"/> (1) = positions are valid and kept.
        /// </summary>
        public int gridVersion;

        /// <summary>Rows added by bag expansions (<see cref="InventoryManager.AddBagRows"/>). Old saves: 0.</summary>
        public int extraRows;

        [Serializable]
        public class EquippedEntry
        {
            public EquipmentSlot slot;
            public string itemId;
            public float durability;

            // Procedural data of the equipped item (mirrors InventorySlotData's generated* fields).
            // Before these existed, equipping a generated item and saving/loading turned it back
            // into a plain base item (affixes, generated name and rolled rarity were all dropped).
            // Old saves simply lack these keys: JsonUtility leaves them null/empty/Common, which
            // InventorySlotData already treats as "plain item" - so old saves load unchanged.
            public string[] generatedAffixIds;
            public float[] generatedAffixValues;
            public string generatedName;
            public ItemRarity generatedRarity;

            // Refine data of the equipped item (InventorySlotData.refineLevel/refineOptionLevel, RefineRules).
            // Old saves lack the keys: 0 = a plain +0 item.
            public int refineLevel;
            public int refineOptionLevel;

            // MU-style options of the equipped item (InventorySlotData.excellentMask/luck/ancientSetId/sockets,
            // ItemOptionRules). Old saves lack the keys: 0 / false / null = a plain piece.
            public int excellentMask;
            public bool luck;
            public string ancientSetId;
            public string[] sockets;
        }
    }

    /// <summary>
    /// Owns the player's carried items and equipped gear. The backpack is an MU Online-style grid
    /// (<see cref="Columns"/> × <see cref="Rows"/>, 8 × 8 by default): every entry of <see cref="Slots"/> is one item
    /// (or stack) with a top-left cell (<see cref="InventorySlotData.gridX"/>/<see cref="InventorySlotData.gridY"/>) and a
    /// footprint from its category (<see cref="ItemFootprint"/>). The grid is the only capacity limit — an item fits only
    /// when a free area of its footprint exists (or a plain stack of it has room). Weight never blocks a pickup: it
    /// only slows the hero and the mount (<see cref="GetCarriedWeight"/>, <see cref="EncumbranceRules"/>). Everything that asks "does it fit?" goes through the shared fit API
    /// (<see cref="CanFit"/>, <see cref="GetFittableAmount"/>, <see cref="CanFitAll"/>, <see cref="CanFitCopy"/>), which
    /// simulates exactly what <see cref="AddItem(ItemDefinition,int)"/> would do (<see cref="BagFitSimulator"/>).
    /// Also: moving on the grid (<see cref="MoveItem"/>: move / merge / swap), splitting, "Organizar"
    /// (<see cref="SortInventory"/>), equipping/unequipping (applying equip stat bonuses through BuffController) and
    /// consuming consumables (healing/mana/stamina/buffs through PlayerStats). Persists itself to JSON through ISaveable,
    /// storing only item ids and cell positions; an old (pre-grid) save is auto-placed on load and anything that does not
    /// fit goes to the Baú da Vó Edna, never lost (<see cref="RestoreState"/>).
    /// </summary>
    public class InventoryManager : MonoBehaviour, ISaveable, IRestoreOrder
    {
        public const int DefaultColumns = 8;
        public const int DefaultRows = 8;

        /// <summary>Most rows a bag expansion ("Bornal de couro") can add (<see cref="AddBagRows"/>).</summary>
        public const int MaxExtraRows = 2;

        /// <summary><see cref="InventorySaveState.gridVersion"/> written by this build (0 = a save from before the grid).</summary>
        public const int GridSaveVersion = 1;

        [Header("Mochila (grade estilo MU)")]
        [SerializeField, Min(1)] private int _columns = DefaultColumns;
        [SerializeField, Min(1)] private int _rows = DefaultRows;

        [Header("Optional References")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private BuffController _buffController;

        private InventorySlotData[] _slots;
        private int _extraRows;
        private readonly Dictionary<EquipmentSlot, InventorySlotData> _equipped = new Dictionary<EquipmentSlot, InventorySlotData>();
        private readonly List<InventorySlotData> _lastRestoreOverflow = new List<InventorySlotData>();

        /// <summary>
        /// Every bag entry (one per grid cell, so the bag can never hold more entries than cells); empty ones have no
        /// position. An entry's index is stable while it stays in the bag, so slot-index APIs (use, equip, sell) keep working.
        /// </summary>
        public IReadOnlyList<InventorySlotData> Slots
        {
            get
            {
                EnsureSlotsInitialized();
                return _slots;
            }
        }
        public IReadOnlyDictionary<EquipmentSlot, InventorySlotData> Equipped => _equipped;

        public int Columns => Mathf.Max(1, _columns);
        public int Rows => Mathf.Max(1, _rows) + _extraRows;
        public int CellCount => Columns * Rows;

        /// <summary>Rows added by bag expansions (0..<see cref="MaxExtraRows"/>); saved.</summary>
        public int ExtraRows => _extraRows;

        /// <summary>
        /// Where <see cref="RestoreState"/> sends what an old save's layout could not keep. Null = the shared chest
        /// (<see cref="VillageChestStorage.ResolveShared"/>). Tests set their own.
        /// </summary>
        public VillageChestStorage OverflowChest { get; set; }

        /// <summary>Copies of the entries the last <see cref="RestoreState"/> sent to the chest (empty when everything fit).</summary>
        public IReadOnlyList<InventorySlotData> LastRestoreOverflow => _lastRestoreOverflow;

        public string SaveKey => "Inventory";

        /// <summary>RestoreState clears every BuffController effect, so it runs before AttributeSet/SkillManager re-apply theirs.</summary>
        public int RestoreOrder => RestoreOrders.Equipment;

        private void Awake()
        {
            EnsureSlotsInitialized();

            if (_playerStats == null)
            {
                _playerStats = GetComponent<PlayerStats>();
            }

            if (_buffController == null)
            {
                _buffController = GetComponent<BuffController>();
            }

            // Durability (FASE 16): the hero's gear wears with attacks, hits and deaths. Only on the player object.
            if (CompareTag("Player") && GetComponent<Rapadura.Gameplay.Items.Durability.EquipmentWearTracker>() == null)
            {
                gameObject.AddComponent<Rapadura.Gameplay.Items.Durability.EquipmentWearTracker>();
            }
        }

        /// <summary>
        /// Guards every public entry point below against being called before Unity has run this
        /// component's <see cref="Awake"/> — which, unlike Play Mode, is NOT synchronous right
        /// after <c>AddComponent</c> in Edit Mode (confirmed: <c>_slots</c> was still null
        /// immediately after <c>AddComponent&lt;InventoryManager&gt;()</c> outside Play Mode),
        /// so EditMode tests calling <see cref="AddItem(ItemDefinition,int)"/> right after adding
        /// the component hit a NullReferenceException here. Idempotent and cheap to call from
        /// every public method.
        /// </summary>
        private void EnsureSlotsInitialized()
        {
            if (_slots != null)
            {
                return;
            }

            _slots = CreateEmptySlots(CellCount);
        }

        private static InventorySlotData[] CreateEmptySlots(int count)
        {
            var slots = new InventorySlotData[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = InventorySlotData.Empty();
            }

            return slots;
        }

        /// <summary>ItemDatabase lookup that tolerates null/empty ids (empty entries).</summary>
        private static ItemDefinition ResolveItem(string itemId)
        {
            return string.IsNullOrEmpty(itemId) ? null : ItemDatabase.GetById(itemId);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Unregister(this);
            }
        }

        // ---------------------------------------------------------------
        // Grid
        // ---------------------------------------------------------------

        /// <summary>Current cell occupancy (each cell = the entry index covering it, or <see cref="BagGrid.Empty"/>).</summary>
        public BagGrid BuildGrid()
        {
            EnsureSlotsInitialized();
            return BagGridRules.Build(Columns, Rows, _slots, ResolveItem);
        }

        /// <summary>Footprint of <paramref name="item"/> on this bag (category default or override, clamped to the grid).</summary>
        public GridSize GetFootprint(ItemDefinition item) => ItemFootprint.For(item, Columns, Rows);

        /// <summary>Area entry <paramref name="slotIndex"/> covers (meaningless for an empty entry).</summary>
        public GridRect GetSlotRect(int slotIndex)
        {
            EnsureSlotsInitialized();
            return IsValidSlot(slotIndex)
                ? BagGridRules.RectOf(_slots[slotIndex], ResolveItem, Columns, Rows)
                : new GridRect(-1, -1, 1, 1);
        }

        /// <summary>Entry covering cell (<paramref name="x"/>, <paramref name="y"/>), or -1.</summary>
        public int GetSlotIndexAt(int x, int y) => BuildGrid().At(x, y);

        public int FreeCellCount => BuildGrid().FreeCellCount;

        public int UsedCellCount => CellCount - FreeCellCount;

        /// <summary>
        /// Adds <paramref name="rows"/> rows to the bottom of the grid (bag expansion, e.g. "Bornal de couro"), up to
        /// <see cref="MaxExtraRows"/> in total. Returns true if the bag grew. Saved (<see cref="InventorySaveState.extraRows"/>).
        /// </summary>
        public bool AddBagRows(int rows)
        {
            EnsureSlotsInitialized();
            int target = Mathf.Clamp(_extraRows + Mathf.Max(0, rows), 0, MaxExtraRows);
            if (target == _extraRows)
            {
                return false;
            }

            _extraRows = target;
            var grown = CreateEmptySlots(CellCount);
            Array.Copy(_slots, grown, Mathf.Min(_slots.Length, grown.Length));
            _slots = grown;
            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>Index of the first empty entry, or -1.</summary>
        private int FirstEmptyEntry()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = InventorySlotData.Empty();
                }

                if (_slots[i].IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Reserves a new entry for <paramref name="item"/> at the first free area of its footprint (MU order) and returns
        /// its index with the position already set (the caller fills it), or -1 when no entry or area is free.
        /// </summary>
        private int ReserveNewEntry(ItemDefinition item, BagGrid grid)
        {
            int entry = FirstEmptyEntry();
            if (entry < 0)
            {
                return -1;
            }

            GridSize size = GetFootprint(item);
            if (!grid.TryFindFirstFit(size, out int x, out int y))
            {
                return -1;
            }

            _slots[entry].Clear();
            _slots[entry].gridX = x;
            _slots[entry].gridY = y;
            grid.TryOccupy(entry, new GridRect(x, y, size));
            return entry;
        }

        /// <summary>Non-empty entry indices in grid order (top row first, left to right), ties by index.</summary>
        private List<int> EntriesInGridOrder()
        {
            var order = new List<int>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null && !_slots[i].IsEmpty)
                {
                    order.Add(i);
                }
            }

            order.Sort((a, b) =>
            {
                int byRow = _slots[a].gridY.CompareTo(_slots[b].gridY);
                if (byRow != 0)
                {
                    return byRow;
                }

                int byColumn = _slots[a].gridX.CompareTo(_slots[b].gridX);
                return byColumn != 0 ? byColumn : a.CompareTo(b);
            });

            return order;
        }

        // ---------------------------------------------------------------
        // Shared fit API ("does it fit?")
        // ---------------------------------------------------------------

        /// <summary>A simulation of this bag's current room; several <see cref="BagFitSimulator.Add"/> calls check a bundle.</summary>
        public BagFitSimulator CreateFitSimulator()
        {
            EnsureSlotsInitialized();
            return new BagFitSimulator(BuildGrid(), _slots, ResolveItem);
        }

        /// <summary>How many of <paramref name="quantity"/> units <see cref="AddItem(ItemDefinition,int)"/> would place right now.</summary>
        public int GetFittableAmount(ItemDefinition item, int quantity)
        {
            return item == null || quantity <= 0 ? 0 : CreateFitSimulator().Add(item, quantity);
        }

        /// <summary>True when all <paramref name="quantity"/> units fit (plain stacks with room first, then free areas).</summary>
        public bool CanFit(ItemDefinition item, int quantity)
        {
            if (quantity <= 0)
            {
                return true;
            }

            return item != null && GetFittableAmount(item, quantity) >= quantity;
        }

        /// <summary>True when every (item, quantity) fits at the same time (a reward bundle, a craft's outputs).</summary>
        public bool CanFitAll(IEnumerable<KeyValuePair<ItemDefinition, int>> items)
        {
            if (items == null)
            {
                return true;
            }

            BagFitSimulator simulator = CreateFitSimulator();
            foreach (KeyValuePair<ItemDefinition, int> pair in items)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                if (pair.Key == null || simulator.Add(pair.Key, pair.Value) < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when <see cref="TryAddCopy"/> would succeed: a generated roll / non-stackable piece needs a free area of its
        /// own; a plain stack may also fill existing stacks.
        /// </summary>
        public bool CanFitCopy(InventorySlotData data)
        {
            if (data == null || data.IsEmpty)
            {
                return false;
            }

            ItemDefinition item = ResolveItem(data.itemId);
            if (item == null)
            {
                return false;
            }

            bool asPiece = data.HasGeneratedData || !item.IsStackable;
            return CreateFitSimulator().Add(item, data.quantity, asPiece) >= data.quantity;
        }

        /// <summary>Legacy name (the weight limit is gone): same as <see cref="CanFit"/>. Prefer <see cref="CanFit"/>.</summary>
        public bool HasWeightCapacityFor(ItemDefinition item, int quantity)
        {
            return CanFit(item, quantity);
        }

        // ---------------------------------------------------------------
        // Weight (carry load, EncumbranceRules)
        // ---------------------------------------------------------------

        /// <summary>
        /// Weight of the backpack only (stacks per unit, <see cref="ItemWeightRules.UnitWeight(ItemDefinition)"/>). The grid
        /// still decides what fits; weight only slows the hero/mount (<see cref="GetCarriedWeight"/>).
        /// </summary>
        public float GetTotalWeight()
        {
            EnsureSlotsInitialized();
            return EncumbranceRules.WeightOf(_slots, ResolveItem);
        }

        /// <summary>Weight of the equipped pieces.</summary>
        public float GetEquippedWeight() => EncumbranceRules.WeightOf(_equipped.Values, ResolveItem);

        /// <summary>The carry load: backpack + equipped (Tibia style; see <see cref="EncumbranceRules"/>).</summary>
        public float GetCarriedWeight() => GetTotalWeight() + GetEquippedWeight();

        // ---------------------------------------------------------------
        // Add / Remove
        // ---------------------------------------------------------------

        /// <summary>Attempts to add a quantity of an item, filling existing plain stacks first (grid order), then new stacks at the first free area. Returns leftover that could not fit.</summary>
        public int AddItem(ItemDefinition item, int quantity)
        {
            return AddItem(item, quantity, null);
        }

        /// <summary>
        /// Same as <see cref="AddItem(ItemDefinition,int)"/>, but when the item lands in a fresh
        /// entry, <paramref name="preserveDurability"/> (if given) is used instead of resetting
        /// to <c>MaxDurability</c>. Used when returning an item that already existed at some
        /// durability (e.g. unequipping) so it doesn't come back fully repaired. A brand-new item
        /// (preserveDurability == null) still starts at MaxDurability as before.
        /// </summary>
        public int AddItem(ItemDefinition item, int quantity, float? preserveDurability)
        {
            EnsureSlotsInitialized();

            if (item == null || quantity <= 0)
            {
                return quantity;
            }

            int remaining = quantity;

            if (item.IsStackable)
            {
                foreach (int index in EntriesInGridOrder())
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    InventorySlotData slot = _slots[index];

                    // Never stack into a slot holding a generated item: the plain items would
                    // inherit (or, when later split/removed, be confused with) that roll's affixes.
                    if (slot.itemId != item.ItemId || slot.quantity >= item.MaxStack || slot.HasGeneratedData)
                    {
                        continue;
                    }

                    int amountToAdd = Mathf.Min(item.MaxStack - slot.quantity, remaining);
                    slot.quantity += amountToAdd;
                    remaining -= amountToAdd;
                }
            }

            if (remaining > 0)
            {
                BagGrid grid = BuildGrid();

                while (remaining > 0)
                {
                    int entry = ReserveNewEntry(item, grid);
                    if (entry < 0)
                    {
                        break;
                    }

                    int amountToAdd = Mathf.Min(item.MaxStack, remaining);
                    InventorySlotData slot = _slots[entry];
                    slot.itemId = item.ItemId;
                    slot.quantity = amountToAdd;
                    slot.currentDurability = item.HasDurability
                        ? (preserveDurability ?? item.MaxDurability)
                        : 0f;
                    remaining -= amountToAdd;
                }
            }

            if (remaining != quantity)
            {
                EventBus.Publish(new InventoryChangedEvent());
            }

            return remaining;
        }

        /// <summary>
        /// Legacy name from the weight-limit era (quest items used to go in past the weight cap). The grid has no
        /// "past the limit": this is <see cref="AddItem(ItemDefinition,int)"/>. Returns the leftover.
        /// </summary>
        public int AddItemIgnoringWeight(ItemDefinition item, int quantity)
        {
            return AddItem(item, quantity);
        }

        /// <summary>
        /// Adds a single procedurally-generated item (with rolled affixes) at the first free area of
        /// its footprint. Generated items are never stacked with anything (each roll is unique), unlike
        /// <see cref="AddItem(ItemDefinition,int)"/>. Returns false if no area/entry was available.
        /// </summary>
        public bool AddGeneratedItem(GeneratedItemInstance instance)
        {
            EnsureSlotsInitialized();

            if (instance?.BaseItem == null)
            {
                return false;
            }

            int entry = ReserveNewEntry(instance.BaseItem, BuildGrid());
            if (entry < 0)
            {
                return false;
            }

            InventorySlotData slot = _slots[entry];
            slot.itemId = instance.BaseItem.ItemId;
            slot.quantity = 1;
            slot.currentDurability = instance.BaseItem.HasDurability ? instance.BaseItem.MaxDurability : 0f;
            slot.generatedAffixIds = instance.AffixIds;
            slot.generatedAffixValues = instance.AffixValues;
            slot.generatedName = instance.GeneratedName;
            slot.generatedRarity = instance.Rarity;
            instance.Options.ApplyTo(slot);

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// Puts a copy of an existing item back in the bag exactly as it is (quantity, durability, affixes, name, rarity):
        /// the chest's "take", unequipping a generated roll. All or nothing
        /// (<see cref="CanFitCopy"/>): a generated roll or non-stackable piece gets an area of its own, a plain stack fills
        /// existing stacks first. <paramref name="data"/> itself is not changed.
        /// </summary>
        public bool TryAddCopy(InventorySlotData data)
        {
            EnsureSlotsInitialized();

            if (!CanFitCopy(data))
            {
                return false;
            }

            ItemDefinition item = ResolveItem(data.itemId);
            if (!data.HasGeneratedData && item.IsStackable)
            {
                return AddItem(item, data.quantity, item.HasDurability ? data.currentDurability : (float?)null) == 0;
            }

            int entry = ReserveNewEntry(item, BuildGrid());
            if (entry < 0)
            {
                return false;
            }

            int x = _slots[entry].gridX;
            int y = _slots[entry].gridY;
            _slots[entry] = data.CloneWithQuantity(data.quantity);
            _slots[entry].gridX = x;
            _slots[entry].gridY = y;
            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        public bool RemoveFromSlot(int slotIndex, int quantity)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty || _slots[slotIndex].quantity < quantity)
            {
                return false;
            }

            _slots[slotIndex].quantity -= quantity;

            if (_slots[slotIndex].quantity <= 0)
            {
                _slots[slotIndex].Clear();
            }

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        public bool RemoveById(string itemId, int quantity)
        {
            EnsureSlotsInitialized();
            int remaining = quantity;

            // Two passes: plain stacks first, generated (affixed/named) items only if still short.
            // Callers that remove "by id" (crafting/building ingredients, the shop's buy rollback)
            // don't care which instance goes, so a single first-match pass used to happily eat a
            // player's Rare/Epic roll while an identical plain copy sat in a later slot - and in
            // ShopManager.BuyItem's rollback it would remove the generated item instead of the
            // plain one that was just added.
            remaining = RemoveByIdPass(itemId, remaining, generatedPass: false);
            remaining = RemoveByIdPass(itemId, remaining, generatedPass: true);

            bool success = remaining <= 0;

            if (success)
            {
                EventBus.Publish(new InventoryChangedEvent());
            }

            return success;
        }

        private int RemoveByIdPass(string itemId, int remaining, bool generatedPass)
        {
            foreach (InventorySlotData slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.itemId != itemId || slot.HasGeneratedData != generatedPass)
                {
                    continue;
                }

                int amountToRemove = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= amountToRemove;
                remaining -= amountToRemove;

                if (slot.quantity <= 0)
                {
                    slot.Clear();
                }
            }

            return remaining;
        }

        public int GetTotalCount(string itemId)
        {
            EnsureSlotsInitialized();
            return _slots.Where(s => s.itemId == itemId).Sum(s => s.quantity);
        }

        // ---------------------------------------------------------------
        // Move / Split
        // ---------------------------------------------------------------

        /// <summary>What dropping entry <paramref name="slotIndex"/> with its top-left on cell (x, y) would do (the UI's ghost preview).</summary>
        public BagMovePlan PlanMove(int slotIndex, int x, int y)
        {
            EnsureSlotsInitialized();
            return BagGridRules.PlanMove(BuildGrid(), _slots, ResolveItem, slotIndex, x, y);
        }

        /// <summary>
        /// Grid drag-and-drop: puts entry <paramref name="slotIndex"/> with its top-left on cell (<paramref name="x"/>,
        /// <paramref name="y"/>) — moves into a free area, merges into a plain stack of the same item, or swaps with the one
        /// item in the way (<see cref="BagGridRules.PlanMove"/>). Returns false (nothing changes) when it does not fit.
        /// </summary>
        public bool MoveItem(int slotIndex, int x, int y)
        {
            BagMovePlan plan = PlanMove(slotIndex, x, y);

            switch (plan.Kind)
            {
                case BagMoveKind.Unchanged:
                    return true;

                case BagMoveKind.Move:
                    _slots[slotIndex].gridX = x;
                    _slots[slotIndex].gridY = y;
                    break;

                case BagMoveKind.Merge:
                {
                    InventorySlotData from = _slots[slotIndex];
                    InventorySlotData to = _slots[plan.OtherIndex];
                    ItemDefinition item = ResolveItem(from.itemId);
                    int amountToMove = Mathf.Min(item.MaxStack - to.quantity, from.quantity);
                    to.quantity += amountToMove;
                    from.quantity -= amountToMove;
                    if (from.quantity <= 0)
                    {
                        from.Clear();
                    }

                    break;
                }

                case BagMoveKind.Swap:
                    _slots[slotIndex].gridX = x;
                    _slots[slotIndex].gridY = y;
                    _slots[plan.OtherIndex].gridX = plan.OtherX;
                    _slots[plan.OtherIndex].gridY = plan.OtherY;
                    break;

                default:
                    return false;
            }

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// List-order move (kept from the slot-list bag; the grid UI uses <see cref="MoveItem"/>): merges plain stacks of
        /// the same item, otherwise swaps the two entries' places in <see cref="Slots"/>. Each item keeps its grid cell —
        /// on the grid nothing moves unless units merge.
        /// </summary>
        public void MoveOrMergeSlot(int fromIndex, int toIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(fromIndex) || !IsValidSlot(toIndex) || fromIndex == toIndex)
            {
                return;
            }

            InventorySlotData from = _slots[fromIndex];
            InventorySlotData to = _slots[toIndex];

            if (to.IsEmpty)
            {
                _slots[toIndex] = from;
                _slots[fromIndex] = InventorySlotData.Empty();
            }
            else if (to.itemId == from.itemId && !to.HasGeneratedData && !from.HasGeneratedData)
            {
                // Merging is only safe between plain stacks: merging a generated item into (or
                // with) another stack of the same base id would keep just one side's
                // affixes/name/rarity and silently discard the other's. Generated items swap below.
                ItemDefinition item = ResolveItem(from.itemId);
                int maxStack = item != null ? item.MaxStack : int.MaxValue;
                int spaceInStack = maxStack - to.quantity;
                int amountToMove = Mathf.Min(spaceInStack, from.quantity);

                to.quantity += amountToMove;
                from.quantity -= amountToMove;

                if (from.quantity <= 0)
                {
                    from.Clear();
                }
            }
            else
            {
                _slots[fromIndex] = to;
                _slots[toIndex] = from;
            }

            EventBus.Publish(new InventoryChangedEvent());
        }

        /// <summary>Splits <paramref name="amount"/> units into a new stack at the first free area. False when there is none.</summary>
        public bool SplitStack(int slotIndex, int amount)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].quantity <= amount || amount <= 0)
            {
                return false;
            }

            InventorySlotData source = _slots[slotIndex];
            int target = ReserveNewEntry(ResolveItem(source.itemId), BuildGrid());

            if (target < 0)
            {
                return false;
            }

            _slots[target].itemId = source.itemId;
            _slots[target].quantity = amount;
            _slots[target].currentDurability = source.currentDurability;
            _slots[target].CopyGeneratedDataFrom(source);

            source.quantity -= amount;

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _slots.Length;
        }

        // ---------------------------------------------------------------
        // Auto-sort
        // ---------------------------------------------------------------

        // Gear first, then what you use, then what you craft with; quest items/currency last.
        private static readonly ItemType[] SortTypeOrder =
        {
            ItemType.Weapon,
            ItemType.Armor,
            ItemType.Tool,
            ItemType.Consumable,
            ItemType.Material,
            ItemType.Blueprint,
            ItemType.QuestItem,
            ItemType.Currency,
        };

        /// <summary>
        /// "Organizar" button: orders the backpack with <see cref="BuildSortedSlots"/> (type → rarity →
        /// name, plain stacks merged, generated items untouched), then packs it MU-style from the top-left
        /// (<see cref="BagGridRules.TryPackForSort"/>: in that order, else biggest first). If neither packing fits
        /// (a fragmented, nearly full bag), nothing changes. Equipment is not affected.
        /// </summary>
        public void SortInventory()
        {
            EnsureSlotsInitialized();
            InventorySlotData[] sorted = BuildSortedSlots(_slots, ResolveItem);

            var placed = new List<InventorySlotData>();
            var sizes = new List<GridSize>();
            foreach (InventorySlotData slot in sorted)
            {
                if (slot != null && !slot.IsEmpty)
                {
                    placed.Add(slot);
                    sizes.Add(GetFootprint(ResolveItem(slot.itemId)));
                }
            }

            if (!BagGridRules.TryPackForSort(Columns, Rows, sizes, out GridRect[] rects))
            {
                return;
            }

            for (int i = 0; i < placed.Count; i++)
            {
                placed[i].gridX = rects[i].X;
                placed[i].gridY = rects[i].Y;
            }

            var result = CreateEmptySlots(CellCount);
            for (int i = 0; i < sorted.Length && i < result.Length; i++)
            {
                if (sorted[i] != null)
                {
                    result[i] = sorted[i];
                }
            }

            _slots = result;
            EventBus.Publish(new InventoryChangedEvent());
        }

        /// <summary>
        /// Pure sort used by <see cref="SortInventory"/> (static so EditMode tests can feed it any
        /// resolver). Returns a new array of the same length: occupied slots first, ordered by item type
        /// (<see cref="SortTypeOrder"/>), then rarity (highest first; a generated item counts at its
        /// rolled rarity, like the tooltip shows it), then the on-screen name — exactly what the UI shows,
        /// <see cref="GeneratedItemName.Compose(ItemDefinition, InventorySlotData, LocalizationManager, Func{string, ItemAffixDefinition})"/>
        /// in the current language (<paramref name="localization"/>, default the registered manager), not the
        /// pt-BR <see cref="InventorySlotData.generatedName"/> stored at roll time — then item id; empty slots at the end. Plain stackable items of the same id are merged into
        /// as few stacks as <see cref="ItemDefinition.MaxStack"/> allows. Generated items
        /// (<see cref="InventorySlotData.HasGeneratedData"/>) and items with durability are never merged
        /// — they're carried over as the same slot objects, so affixes/name/rarity/durability survive.
        /// Ids the resolver doesn't know are kept unchanged after every known item.
        /// </summary>
        /// <param name="resolveAffix">Affix lookup for generated names; defaults to <see cref="AffixDatabase.GetById"/>.</param>
        public static InventorySlotData[] BuildSortedSlots(
            IReadOnlyList<InventorySlotData> slots,
            Func<string, ItemDefinition> resolveItem,
            LocalizationManager localization = null,
            Func<string, ItemAffixDefinition> resolveAffix = null)
        {
            localization = localization ?? ContentText.CurrentManager;

            if (slots == null)
            {
                return Array.Empty<InventorySlotData>();
            }

            var entries = new List<(InventorySlotData slot, ItemDefinition item, int order)>();
            var mergedTotals = new Dictionary<string, (ItemDefinition item, int total, int order)>();

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];

                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                ItemDefinition item = resolveItem?.Invoke(slot.itemId);
                bool mergeable = item != null && item.IsStackable && !item.HasDurability && !slot.HasGeneratedData;

                if (!mergeable)
                {
                    entries.Add((slot, item, i));
                    continue;
                }

                mergedTotals.TryGetValue(slot.itemId, out var existing);
                mergedTotals[slot.itemId] = (item, existing.total + slot.quantity, existing.item != null ? existing.order : i);
            }

            foreach (var merged in mergedTotals.Values)
            {
                int remaining = merged.total;

                while (remaining > 0)
                {
                    int amount = Mathf.Min(merged.item.MaxStack, remaining);
                    entries.Add((new InventorySlotData { itemId = merged.item.ItemId, quantity = amount }, merged.item, merged.order));
                    remaining -= amount;
                }
            }

            List<InventorySlotData> ordered = entries
                .OrderBy(e => e.item == null ? 1 : 0)
                .ThenBy(e => GetSortTypeRank(e.item))
                .ThenByDescending(e => (int)GetSortRarity(e.item, e.slot))
                .ThenBy(e => GetSortName(e.item, e.slot, localization, resolveAffix), StringComparer.InvariantCultureIgnoreCase)
                .ThenBy(e => e.slot.itemId, StringComparer.Ordinal)
                .ThenByDescending(e => e.slot.quantity)
                .ThenBy(e => e.order)
                .Select(e => e.slot)
                .ToList();

            var result = new InventorySlotData[Mathf.Max(slots.Count, ordered.Count)];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = i < ordered.Count ? ordered[i] : InventorySlotData.Empty();
            }

            return result;
        }

        private static int GetSortTypeRank(ItemDefinition item)
        {
            if (item == null)
            {
                return int.MaxValue;
            }

            int rank = Array.IndexOf(SortTypeOrder, item.Type);
            return rank >= 0 ? rank : SortTypeOrder.Length;
        }

        private static ItemRarity GetSortRarity(ItemDefinition item, InventorySlotData slot)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            return slot.HasGeneratedAffixes && slot.generatedRarity > baseRarity ? slot.generatedRarity : baseRarity;
        }

        /// <summary>The name the inventory UI shows for this slot (composed + localized for generated items).</summary>
        private static string GetSortName(ItemDefinition item, InventorySlotData slot, LocalizationManager localization, Func<string, ItemAffixDefinition> resolveAffix)
        {
            if (item == null)
            {
                return slot.itemId ?? string.Empty;
            }

            return GeneratedItemName.Compose(item, slot, localization, resolveAffix) ?? string.Empty;
        }

        // ---------------------------------------------------------------
        // Filtering
        // ---------------------------------------------------------------

        public IEnumerable<InventorySlotData> GetSlotsByType(ItemType type)
        {
            EnsureSlotsInitialized();
            return _slots.Where(slot => !slot.IsEmpty && ItemDatabase.GetById(slot.itemId)?.Type == type);
        }

        public IEnumerable<InventorySlotData> GetSlotsByCategory(ItemCategory category)
        {
            EnsureSlotsInitialized();
            return _slots.Where(slot => !slot.IsEmpty && ItemDatabase.GetById(slot.itemId)?.Category == category);
        }

        // ---------------------------------------------------------------
        // Equipment
        // ---------------------------------------------------------------

        public bool EquipFromSlot(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty)
            {
                return false;
            }

            InventorySlotData slot = _slots[slotIndex];
            ItemDefinition item = ItemDatabase.GetById(slot.itemId);

            if (item == null || !item.IsEquippable)
            {
                return false;
            }

            // Deep copy (affixes, generated name, rolled rarity, durability) captured BEFORE
            // RemoveFromSlot below — InventorySlotData is a class, so `slot` and
            // `_slots[slotIndex]` are the same object, and RemoveFromSlot's Clear() (once quantity
            // hits 0) wipes the generated fields on that shared instance.
            InventorySlotData equippedData = slot.CloneWithQuantity(1);

            // Take the new item out of the inventory FIRST, then return whatever was equipped in
            // that slot. This makes a swap work even with a full inventory (the old item lands in
            // the slot the new one just freed). It used to be the other way round, ignoring
            // UnequipSlot's result: with no free slot the old item was never returned to the
            // inventory and was then silently overwritten in _equipped below — destroyed.
            RemoveFromSlot(slotIndex, 1);

            if (_equipped.ContainsKey(item.EquipSlot) && !UnequipSlot(item.EquipSlot))
            {
                // Couldn't make room for the previously equipped item — put the new one back
                // exactly as it was (including its generated data) and abort.
                InventorySlotData source = _slots[slotIndex];

                if (source.IsEmpty)
                {
                    _slots[slotIndex] = equippedData;
                }
                else
                {
                    source.quantity += 1;
                }

                EventBus.Publish(new InventoryChangedEvent());
                return false;
            }

            _equipped[item.EquipSlot] = equippedData;
            ApplyEquipEffects(item, equippedData, item.EquipSlot);

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// Applies an equipped item's base <see cref="ItemDefinition.EquipStatModifiers"/> plus its
        /// procedurally-rolled affixes as permanent buffs under the slot's source key. Shared by
        /// <see cref="EquipFromSlot"/> and <see cref="RestoreState"/> — the latter used to re-apply
        /// only the base modifiers, so a generated item equipped across a save/load lost its
        /// affix bonuses even when its data survived.
        /// </summary>
        private void ApplyEquipEffects(ItemDefinition item, InventorySlotData equippedData, EquipmentSlot equipSlot)
        {
            if (_buffController == null || item == null)
            {
                return;
            }

            // A broken piece (durability 0, DurabilityRules) gives no stats at all until the ferreiro repairs it.
            if (Rapadura.Gameplay.Items.Durability.DurabilityRules.IsBroken(item, equippedData))
            {
                RefreshAncientSetBonuses();
                return;
            }

            // Base modifiers and affixes each get their own sub-key under "equip_<slot>/": they used to
            // share "equip_<slot>", so an affix on the same stat as a base modifier (or a second base
            // modifier on the same stat) replaced it in BuffController.ApplyEffect instead of stacking.
            // UnequipSlot drops the whole prefix.
            StatModifierDefinition[] baseModifiers = item.EquipStatModifiers;
            for (int i = 0; i < baseModifiers.Length; i++)
            {
                _buffController.ApplyEffect(WithPermanentDuration(baseModifiers[i]), GetEquipBaseSourceKey(equipSlot, i));
            }

            // Procedurally-rolled affixes (ProceduralItemGenerator/GeneratedItemInstance) were
            // stored on the slot (generatedAffixIds/generatedAffixValues) but nothing anywhere in
            // the codebase ever read them back out to apply a real stat bonus — a Rare/Epic/
            // Legendary item's rolled affixes only ever showed up in its generated display name
            // ("Espada Flamejante da Fúria"), with zero actual gameplay effect once equipped, same
            // stats as a plain Common item. Mirrors the EquipStatModifiers path above exactly.
            string[] generatedAffixIds = equippedData.generatedAffixIds;
            float[] generatedAffixValues = equippedData.generatedAffixValues ?? Array.Empty<float>();

            if (generatedAffixIds != null && generatedAffixIds.Length > 0)
            {
                // Grouped by (stat, application) and summed before calling ApplyEffects: each call
                // to BuffController.ApplyEffect removes any existing effect for the same
                // affectedStat before adding its own (see BuffController.ApplyEffect), so two
                // rolled affixes hitting the same stat (e.g. this session's test roll had both
                // "affix_brutal" +10.6 and "affix_sharp" +2.5 AttackDamage) would otherwise have the
                // second silently wipe out the first instead of both contributing.
                var byStat = new Dictionary<(StatType, ModifierApplication), float>();
                for (int i = 0; i < generatedAffixIds.Length; i++)
                {
                    ItemAffixDefinition affix = AffixDatabase.GetById(generatedAffixIds[i]);
                    if (affix == null)
                    {
                        continue;
                    }

                    float value = i < generatedAffixValues.Length ? generatedAffixValues[i] : 0f;
                    var key = (affix.AffectedStat, affix.Application);
                    byStat.TryGetValue(key, out float existing);
                    byStat[key] = existing + value;
                }

                int affixIndex = 0;
                foreach (KeyValuePair<(StatType, ModifierApplication), float> kvp in byStat)
                {
                    var generated = new StatModifierDefinition
                    {
                        displayName = "generated_" + kvp.Key.Item1,
                        affectedStat = kvp.Key.Item1,
                        application = kvp.Key.Item2,
                        value = kvp.Value,
                    };

                    _buffController.ApplyEffect(WithPermanentDuration(generated), GetEquipAffixSourceKey(equipSlot, affixIndex++));
                }
            }

            ApplyRefineEffects(item, equippedData, equipSlot);
            ApplyWingEffects(item, equipSlot);
            ApplyItemOptionEffects(item, equippedData, equipSlot);
            RefreshAncientSetBonuses();
        }

        /// <summary>BuffController source prefix of the ancient set bonuses (whole equipment, not one slot).</summary>
        public const string AncientSetSourcePrefix = "ancient_set/";

        /// <summary>
        /// The piece's own MU-style options (<see cref="Rapadura.Gameplay.Items.Options.ItemOptionRules.PieceModifiers"/>:
        /// excellent lines, luck, engraved sockets), summed per (stat, application) under <c>equip_&lt;slot&gt;/option/*</c>.
        /// </summary>
        private void ApplyItemOptionEffects(ItemDefinition item, InventorySlotData equippedData, EquipmentSlot equipSlot)
        {
            if (_buffController == null || equippedData == null || !equippedData.HasItemOptions)
            {
                return;
            }

            int index = 0;
            foreach (KeyValuePair<(StatType, ModifierApplication), float> kvp in SumOptionModifiers(
                         Rapadura.Gameplay.Items.Options.ItemOptionRules.PieceModifiers(item, equippedData)))
            {
                _buffController.ApplyEffect(WithPermanentDuration(new StatModifierDefinition
                {
                    displayName = "option_" + kvp.Key.Item1,
                    affectedStat = kvp.Key.Item1,
                    application = kvp.Key.Item2,
                    value = kvp.Value,
                }), $"{GetEquipSourceKey(equipSlot)}/option/{index++}");
            }
        }

        /// <summary>
        /// Re-applies every ancient set bonus from the current equipment (<see cref="Rapadura.Gameplay.Items.Options.ItemOptionRules.SetBonuses"/>;
        /// broken pieces do not count) under <see cref="AncientSetSourcePrefix"/>. Called whenever the equipment changes.
        /// </summary>
        private void RefreshAncientSetBonuses()
        {
            if (_buffController == null)
            {
                return;
            }

            _buffController.RemoveEffectsBySourcePrefix(AncientSetSourcePrefix);
            var pieces = new List<(ItemDefinition, InventorySlotData, bool)>();
            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in _equipped)
            {
                ItemDefinition piece = ResolveItem(pair.Value?.itemId);
                pieces.Add((piece, pair.Value, Rapadura.Gameplay.Items.Durability.DurabilityRules.IsBroken(piece, pair.Value)));
            }

            int index = 0;
            foreach (KeyValuePair<(StatType, ModifierApplication), float> kvp in SumOptionModifiers(
                         Rapadura.Gameplay.Items.Options.ItemOptionRules.SetBonuses(pieces)))
            {
                _buffController.ApplyEffect(WithPermanentDuration(new StatModifierDefinition
                {
                    displayName = "ancient_" + kvp.Key.Item1,
                    affectedStat = kvp.Key.Item1,
                    application = kvp.Key.Item2,
                    value = kvp.Value,
                }), AncientSetSourcePrefix + index++);
            }
        }

        private static Dictionary<(StatType, ModifierApplication), float> SumOptionModifiers(
            IEnumerable<Rapadura.Gameplay.Items.Options.OptionModifier> modifiers)
        {
            var byStat = new Dictionary<(StatType, ModifierApplication), float>();
            foreach (Rapadura.Gameplay.Items.Options.OptionModifier modifier in modifiers)
            {
                var key = (modifier.Stat, modifier.Application);
                byStat.TryGetValue(key, out float existing);
                byStat[key] = existing + modifier.Value;
            }

            return byStat;
        }

        /// <summary>
        /// A wing item's bonuses (<see cref="Rapadura.Gameplay.Tacho.WingCatalog.ModifiersFor"/>: damage %, defense %, speed %,
        /// absorb from level 2) as permanent buffs under <c>equip_&lt;slot&gt;/wing/*</c>. Nothing for any other item.
        /// </summary>
        private void ApplyWingEffects(ItemDefinition item, EquipmentSlot equipSlot)
        {
            if (_buffController == null || item == null)
            {
                return;
            }

            List<Rapadura.Gameplay.Tacho.WingModifier> modifiers = Rapadura.Gameplay.Tacho.WingCatalog.ModifiersFor(item.ItemId);
            for (int i = 0; i < modifiers.Count; i++)
            {
                var modifier = new StatModifierDefinition
                {
                    displayName = modifiers[i].IsAbsorb ? "wing_absorb" : "wing_" + modifiers[i].Stat,
                    affectedStat = modifiers[i].Stat,
                    application = modifiers[i].Application,
                    value = modifiers[i].Value,
                };
                _buffController.ApplyEffect(WithPermanentDuration(modifier), $"{GetEquipSourceKey(equipSlot)}/wing/{i}");
            }
        }

        /// <summary>
        /// Takes the item out of <paramref name="equipSlot"/> WITHOUT returning it to the bag (it is consumed: the Tacho
        /// Encantado turning an equipped piece into wings, or an equipped wing into the next level). Its bonuses go too.
        /// </summary>
        public bool RemoveEquipped(EquipmentSlot equipSlot)
        {
            EnsureSlotsInitialized();
            if (!_equipped.ContainsKey(equipSlot))
            {
                return false;
            }

            _buffController?.RemoveEffectsBySourcePrefix(GetEquipSourceKey(equipSlot) + "/");
            _buffController?.RemoveEffectsBySource(GetEquipSourceKey(equipSlot));
            _equipped.Remove(equipSlot);
            RefreshAncientSetBonuses();
            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// The refine level and extra option of an equipped piece (<see cref="Rapadura.Gameplay.Refining.RefineRules"/>), as
        /// permanent buffs under <c>equip_&lt;slot&gt;/refine/*</c>: a weapon raises AttackDamage (PercentAdditive), an armour
        /// piece adds the refined part of its armour defense as Defense (Flat) and its option as MaxHealth (PercentAdditive).
        /// Nothing at +0 without option, so plain items apply exactly what they did before.
        /// </summary>
        private void ApplyRefineEffects(ItemDefinition item, InventorySlotData equippedData, EquipmentSlot equipSlot)
        {
            if (_buffController == null || item == null || equippedData == null || !equippedData.HasRefineData)
            {
                return;
            }

            int level = equippedData.refineLevel;
            int option = equippedData.refineOptionLevel;
            int index = 0;
            if (item.WeaponDamage > 0f)
            {
                ApplyRefineModifier(StatType.AttackDamage, ModifierApplication.PercentAdditive,
                    Rapadura.Gameplay.Refining.RefineRules.WeaponAttackPercent(level, option), equipSlot, index++);
            }

            if (item.ArmorDefense > 0f)
            {
                ApplyRefineModifier(StatType.Defense, ModifierApplication.Flat,
                    Rapadura.Gameplay.Refining.RefineRules.ArmourDefenseBonus(item.ArmorDefense, level), equipSlot, index++);
                ApplyRefineModifier(StatType.MaxHealth, ModifierApplication.PercentAdditive,
                    Rapadura.Gameplay.Refining.RefineRules.ArmourHealthPercent(option), equipSlot, index++);
            }
        }

        private void ApplyRefineModifier(StatType stat, ModifierApplication application, float value, EquipmentSlot equipSlot, int index)
        {
            if (value == 0f)
            {
                return;
            }

            var modifier = new StatModifierDefinition
            {
                displayName = "refine_" + stat,
                affectedStat = stat,
                application = application,
                value = value,
            };
            _buffController.ApplyEffect(WithPermanentDuration(modifier), GetEquipRefineSourceKey(equipSlot, index));
        }

        /// <summary>BuffController source key of an equipped item's <paramref name="index"/>-th refine bonus.</summary>
        public static string GetEquipRefineSourceKey(EquipmentSlot slot, int index)
        {
            return $"{GetEquipSourceKey(slot)}/refine/{index}";
        }

        /// <summary>
        /// Call after changing an item instance's own data in place (refine level, option): re-applies the equip
        /// effects when <paramref name="equippedSlot"/> holds it (None = a bag item) and raises InventoryChangedEvent.
        /// </summary>
        public void NotifyItemDataChanged(EquipmentSlot equippedSlot = EquipmentSlot.None)
        {
            EnsureSlotsInitialized();
            if (equippedSlot != EquipmentSlot.None && _equipped.TryGetValue(equippedSlot, out InventorySlotData data) && _buffController != null)
            {
                _buffController.RemoveEffectsBySourcePrefix(GetEquipSourceKey(equippedSlot) + "/");
                ApplyEquipEffects(ItemDatabase.GetById(data.itemId), data, equippedSlot);
            }

            EventBus.Publish(new InventoryChangedEvent());
        }

                public bool UnequipSlot(EquipmentSlot equipSlot)
        {
            EnsureSlotsInitialized();

            if (!_equipped.TryGetValue(equipSlot, out InventorySlotData equippedData))
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(equippedData.itemId);

            if (item != null && !ReturnToInventory(item, equippedData))
            {
                // No inventory space to receive the unequipped item back — abort unequip.
                return false;
            }

            _buffController?.RemoveEffectsBySourcePrefix(GetEquipSourceKey(equipSlot) + "/");
            // Pre-sub-key effects (if any were applied under the bare slot key) go too.
            _buffController?.RemoveEffectsBySource(GetEquipSourceKey(equipSlot));
            _equipped.Remove(equipSlot);
            RefreshAncientSetBonuses();

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// Puts a single previously-existing item (e.g. one being unequipped) back into the
        /// inventory without losing anything about it. Plain items go through
        /// <see cref="AddItem(ItemDefinition,int,float?)"/> as before (may stack, keeps
        /// durability). Generated items used to go through that same AddItem too, which only
        /// knows the base ItemDefinition — so unequipping a Rare/Epic/Legendary roll turned it
        /// into a plain base item (affixes, generated name and rarity gone). They now go, as a
        /// deep copy, into a free area of their own (never stacked) via <see cref="TryAddCopy"/>.
        /// </summary>
        private bool ReturnToInventory(ItemDefinition item, InventorySlotData data)
        {
            if (!data.HasGeneratedData)
            {
                return AddItem(item, 1, data.currentDurability) == 0;
            }

            return TryAddCopy(data.CloneWithQuantity(1));
        }

        private static string GetEquipSourceKey(EquipmentSlot slot)
        {
            return $"equip_{slot}";
        }

        /// <summary>BuffController source key of an equipped item's <paramref name="index"/>-th base <see cref="ItemDefinition.EquipStatModifiers"/> entry.</summary>
        public static string GetEquipBaseSourceKey(EquipmentSlot slot, int index)
        {
            return $"{GetEquipSourceKey(slot)}/base/{index}";
        }

        /// <summary>BuffController source key of an equipped item's <paramref name="index"/>-th (stat, application)-summed affix bonus.</summary>
        public static string GetEquipAffixSourceKey(EquipmentSlot slot, int index)
        {
            return $"{GetEquipSourceKey(slot)}/affix/{index}";
        }

        /// <summary>BuffController source key for a consumed item's <see cref="ItemDefinition.ConsumeBuffs"/>.</summary>
        public static string GetConsumableSourceKey(string itemId)
        {
            return $"consumable_{itemId}";
        }

        private static StatModifierDefinition WithPermanentDuration(StatModifierDefinition source)
        {
            return new StatModifierDefinition
            {
                displayName = source.displayName,
                affectedStat = source.affectedStat,
                application = source.application,
                value = source.value,
                durationSeconds = float.MaxValue / 2f,
                isDebuff = source.isDebuff,
                ticksOverTime = false,
                tickIntervalSeconds = source.tickIntervalSeconds,
                tickDamageOrHeal = 0f
            };
        }

        // ---------------------------------------------------------------
        // Consumables
        // ---------------------------------------------------------------

        public bool UseItemInSlot(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty)
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(_slots[slotIndex].itemId);

            if (item == null || !item.IsConsumable)
            {
                return false;
            }

            // Frutas do Sertão (Tacho Encantado): they change the AttributeSet and are only eaten when they did something.
            if (Rapadura.Gameplay.Tacho.FruitRules.IsFruit(item.ItemId))
            {
                return Rapadura.Gameplay.Tacho.FruitEater.TryEat(this, slotIndex, item.ItemId) == Rapadura.Gameplay.Tacho.FruitCheck.Ok;
            }

            if (_playerStats != null)
            {
                if (item.HealthRestore > 0f)
                {
                    _playerStats.Heal(item.HealthRestore);
                }

                if (item.ManaRestore > 0f)
                {
                    _playerStats.RestoreMana(item.ManaRestore);
                }

                if (item.StaminaRestore > 0f)
                {
                    _playerStats.RestoreStamina(item.StaminaRestore);
                }
            }

            if (_buffController != null && item.ConsumeBuffs.Length > 0)
            {
                // Own source per item: a null source let any two consumables' (or a consumable's and a
                // skill's) same-stat buffs replace each other; drinking the same potion again still refreshes.
                _buffController.ApplyEffects(item.ConsumeBuffs, GetConsumableSourceKey(item.ItemId));
            }

            return RemoveFromSlot(slotIndex, 1);
        }

        // ---------------------------------------------------------------
        // Persistence
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            EnsureSlotsInitialized();
            var equippedEntries = _equipped.Select(pair => new InventorySaveState.EquippedEntry
            {
                slot = pair.Key,
                itemId = pair.Value.itemId,
                durability = pair.Value.currentDurability,
                generatedAffixIds = pair.Value.generatedAffixIds,
                generatedAffixValues = pair.Value.generatedAffixValues,
                generatedName = pair.Value.generatedName,
                generatedRarity = pair.Value.generatedRarity,
                refineLevel = pair.Value.refineLevel,
                refineOptionLevel = pair.Value.refineOptionLevel,
                excellentMask = pair.Value.excellentMask,
                luck = pair.Value.luck,
                ancientSetId = pair.Value.ancientSetId,
                sockets = pair.Value.sockets
            }).ToArray();

            return new InventorySaveState
            {
                slots = _slots,
                equipped = equippedEntries,
                gridVersion = GridSaveVersion,
                extraRows = _extraRows
            };
        }

        public void RestoreState(object state)
        {
            if (state is not InventorySaveState saveState)
            {
                return;
            }

            RestoreBagLayout(saveState);

            _equipped.Clear();
            _buffController?.ClearAllEffects();

            foreach (InventorySaveState.EquippedEntry entry in saveState.equipped)
            {
                // Old saves (before EquippedEntry carried generated data) deserialize the generated
                // fields as null/empty/Common, which InventorySlotData treats as a plain item.
                var equippedData = new InventorySlotData
                {
                    itemId = entry.itemId,
                    quantity = 1,
                    currentDurability = entry.durability,
                    generatedAffixIds = entry.generatedAffixIds,
                    generatedAffixValues = entry.generatedAffixValues,
                    generatedName = entry.generatedName,
                    generatedRarity = entry.generatedRarity,
                    refineLevel = entry.refineLevel,
                    refineOptionLevel = entry.refineOptionLevel,
                    excellentMask = entry.excellentMask,
                    luck = entry.luck,
                    ancientSetId = entry.ancientSetId,
                    sockets = entry.sockets
                };
                _equipped[entry.slot] = equippedData;

                ApplyEquipEffects(ItemDatabase.GetById(entry.itemId), equippedData, entry.slot);
            }

            EventBus.Publish(new InventoryChangedEvent());
        }

        /// <summary>
        /// Rebuilds the bag from a save. Entries keep their saved index where it exists. A grid save
        /// (<see cref="InventorySaveState.gridVersion"/> &gt;= 1) keeps every valid position; an old pre-grid save (or an
        /// entry whose area became invalid, e.g. a footprint changed between versions) is auto-placed MU-style
        /// (<see cref="BagGridRules.LayoutForRestore"/>). Whatever does not fit goes to the Baú da Vó Edna
        /// (<see cref="VillageChestStorage.EnqueueOverflow"/>) — never lost — and is listed in <see cref="LastRestoreOverflow"/>.
        /// </summary>
        private void RestoreBagLayout(InventorySaveState saveState)
        {
            _extraRows = Mathf.Clamp(saveState.extraRows, 0, MaxExtraRows);
            _slots = CreateEmptySlots(CellCount);
            _lastRestoreOverflow.Clear();

            InventorySlotData[] saved = saveState.slots ?? Array.Empty<InventorySlotData>();
            var late = new List<InventorySlotData>();

            for (int i = 0; i < saved.Length; i++)
            {
                InventorySlotData slot = saved[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (i < _slots.Length)
                {
                    _slots[i] = slot;
                }
                else
                {
                    late.Add(slot);
                }
            }

            var overflow = new List<InventorySlotData>();
            foreach (InventorySlotData slot in late)
            {
                int free = FirstEmptyEntry();
                if (free < 0)
                {
                    overflow.Add(slot);
                    continue;
                }

                _slots[free] = slot;
            }

            bool keepPositions = saveState.gridVersion >= GridSaveVersion;
            foreach (int index in BagGridRules.LayoutForRestore(Columns, Rows, _slots, ResolveItem, keepPositions))
            {
                overflow.Add(_slots[index]);
                _slots[index] = InventorySlotData.Empty();
            }

            if (overflow.Count == 0)
            {
                return;
            }

            VillageChestStorage chest = OverflowChest ?? VillageChestStorage.ResolveShared();
            foreach (InventorySlotData slot in overflow)
            {
                InventorySlotData copy = slot.CloneWithQuantity(slot.quantity);
                _lastRestoreOverflow.Add(copy);
                chest.EnqueueOverflow(copy);
            }

            Debug.LogWarning($"[InventoryManager] {overflow.Count} item(s) did not fit the {Columns}x{Rows} backpack grid on load and went to the Baú da Vó Edna.");
        }
    }
}
