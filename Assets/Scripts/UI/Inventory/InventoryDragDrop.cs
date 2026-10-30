using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.UI.Inventory
{
    /// <summary>What a drag picked up or what it was dropped on: a backpack slot or an equipment slot.</summary>
    public enum InventoryDragLocation
    {
        None,
        BackpackSlot,
        EquipmentSlot,
    }

    /// <summary>A drag source or drop target (backpack slot index, or equipment slot).</summary>
    public readonly struct InventoryDragEndpoint
    {
        public readonly InventoryDragLocation Location;
        public readonly int SlotIndex;
        public readonly EquipmentSlot EquipSlot;

        private InventoryDragEndpoint(InventoryDragLocation location, int slotIndex, EquipmentSlot equipSlot)
        {
            Location = location;
            SlotIndex = slotIndex;
            EquipSlot = equipSlot;
        }

        public static InventoryDragEndpoint None => default;
        public static InventoryDragEndpoint Backpack(int slotIndex) => new InventoryDragEndpoint(InventoryDragLocation.BackpackSlot, slotIndex, EquipmentSlot.None);
        public static InventoryDragEndpoint Equipment(EquipmentSlot slot) => new InventoryDragEndpoint(InventoryDragLocation.EquipmentSlot, -1, slot);

        public override string ToString() => Location == InventoryDragLocation.EquipmentSlot ? $"Equipment({EquipSlot})" : $"{Location}({SlotIndex})";
    }

    /// <summary>What dropping a dragged item does — mapped 1:1 onto InventoryManager calls by the UI.</summary>
    public enum InventoryDropAction
    {
        /// <summary>Invalid or no-op drop (same slot, wrong equipment slot, outside any target).</summary>
        None,
        /// <summary><c>MoveOrMergeSlot(from, to)</c>: move/merge/swap between backpack slots.</summary>
        MoveOrMerge,
        /// <summary><c>EquipFromSlot(from)</c>: backpack item dropped on its matching equipment slot.</summary>
        Equip,
        /// <summary><c>UnequipSlot(slot)</c>, then move the returned item onto the target slot.</summary>
        Unequip,
    }

    /// <summary>What a tap on the grid does (<see cref="InventoryDragDrop.ResolveTap"/>).</summary>
    public enum BagTapAction
    {
        /// <summary>Nothing held, empty cell tapped.</summary>
        None,
        /// <summary>Nothing held, an item tapped: it is picked up (highlighted, ghost shown).</summary>
        Pick,
        /// <summary>The held item tapped again: use it (consumable) or equip it (gear), then let go.</summary>
        UseOrEquip,
        /// <summary>Something held, another cell/item tapped: place it there (move / merge / swap) if it fits, else red ghost.</summary>
        Place,
    }

    /// <summary>
    /// Pure drag-and-drop rules for the Mochila window (no panel/UIDocument, so EditMode tests cover
    /// them): when a press becomes a drag, which target a point is over, and what a drop means.
    /// The actual moves go through InventoryManager (MoveOrMergeSlot / EquipFromSlot / UnequipSlot),
    /// which already keep generated items' affixes intact.
    /// </summary>
    public static class InventoryDragDrop
    {
        /// <summary>
        /// A press becomes a drag once the pointer moved strictly farther than this. Same value as
        /// <c>TooltipTrigger.MoveTolerancePixels</c>, so the move that starts a drag is the same one
        /// that cancels the long-press tooltip — hold still = tooltip, move = drag, never both.
        /// </summary>
        public const float DragThresholdPixels = Rapadura.UI.Common.TooltipTrigger.MoveTolerancePixels;

        public static bool HasExceededDragThreshold(Vector2 pressPosition, Vector2 currentPosition, float threshold = DragThresholdPixels)
        {
            return (currentPosition - pressPosition).sqrMagnitude > threshold * threshold;
        }

        /// <summary>
        /// Index of the target rect containing <paramref name="point"/>, or -1. A target only counts
        /// when the point is also inside its clip rect (the visible viewport of its ScrollView, when
        /// <paramref name="clipRects"/> is given), so rows scrolled out of view can't catch a drop.
        /// When rects overlap the last one wins (drawn on top, same as UI Toolkit picking).
        /// </summary>
        public static int FindTargetIndex(Vector2 point, IReadOnlyList<Rect> targetRects, IReadOnlyList<Rect> clipRects = null)
        {
            if (targetRects == null)
            {
                return -1;
            }

            for (int i = targetRects.Count - 1; i >= 0; i--)
            {
                if (!targetRects[i].Contains(point))
                {
                    continue;
                }

                if (clipRects != null && i < clipRects.Count && !clipRects[i].Contains(point))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        /// <summary>
        /// Grid cell under <paramref name="localPoint"/> (pixels from the grid's top-left), or false outside the grid.
        /// </summary>
        public static bool TryGetCell(Vector2 localPoint, float cellSize, int columns, int rows, out int x, out int y)
        {
            x = -1;
            y = -1;
            if (cellSize <= 0f || localPoint.x < 0f || localPoint.y < 0f)
            {
                return false;
            }

            int cx = Mathf.FloorToInt(localPoint.x / cellSize);
            int cy = Mathf.FloorToInt(localPoint.y / cellSize);
            if (cx >= columns || cy >= rows)
            {
                return false;
            }

            x = cx;
            y = cy;
            return true;
        }

        /// <summary>
        /// Top-left cell an item lands on when the cell under the pointer is (<paramref name="cellX"/>, <paramref name="cellY"/>)
        /// and it was grabbed by its (<paramref name="grabX"/>, <paramref name="grabY"/>) cell — so a sword picked by its hilt
        /// keeps the hilt under the finger. May be outside the grid (then the move is invalid: red ghost).
        /// </summary>
        public static Vector2Int AnchorFor(int cellX, int cellY, int grabX, int grabY) => new Vector2Int(cellX - grabX, cellY - grabY);

        /// <summary>
        /// Tap-to-pick / tap-to-place (mobile, also mouse clicks and gamepad A) on the grid: what a tap on item
        /// <paramref name="tappedIndex"/> (-1 = an empty cell) does while <paramref name="heldIndex"/> is held (-1 = nothing).
        /// </summary>
        public static BagTapAction ResolveTap(int heldIndex, int tappedIndex)
        {
            if (heldIndex < 0)
            {
                return tappedIndex >= 0 ? BagTapAction.Pick : BagTapAction.None;
            }

            if (tappedIndex == heldIndex)
            {
                return BagTapAction.UseOrEquip;
            }

            return BagTapAction.Place;
        }

        /// <summary>
        /// What dropping <paramref name="source"/> on <paramref name="target"/> does.
        /// <paramref name="sourceItemEquipSlot"/> is the dragged item's <c>ItemDefinition.EquipSlot</c>
        /// (<see cref="EquipmentSlot.None"/> for non-equippables).
        /// </summary>
        public static InventoryDropAction ResolveDrop(InventoryDragEndpoint source, InventoryDragEndpoint target, EquipmentSlot sourceItemEquipSlot)
        {
            switch (source.Location)
            {
                case InventoryDragLocation.BackpackSlot:
                    if (target.Location == InventoryDragLocation.BackpackSlot)
                    {
                        return target.SlotIndex >= 0 && source.SlotIndex >= 0 && target.SlotIndex != source.SlotIndex
                            ? InventoryDropAction.MoveOrMerge
                            : InventoryDropAction.None;
                    }

                    if (target.Location == InventoryDragLocation.EquipmentSlot)
                    {
                        return sourceItemEquipSlot != EquipmentSlot.None && sourceItemEquipSlot == target.EquipSlot
                            ? InventoryDropAction.Equip
                            : InventoryDropAction.None;
                    }

                    return InventoryDropAction.None;

                case InventoryDragLocation.EquipmentSlot:
                    // Equipped item dragged back into the backpack (any slot: it lands there, swapping
                    // with what's in it). Equipment → equipment is meaningless (one slot per item kind).
                    return target.Location == InventoryDragLocation.BackpackSlot && target.SlotIndex >= 0 && source.EquipSlot != EquipmentSlot.None
                        ? InventoryDropAction.Unequip
                        : InventoryDropAction.None;

                default:
                    return InventoryDropAction.None;
            }
        }
    }
}
