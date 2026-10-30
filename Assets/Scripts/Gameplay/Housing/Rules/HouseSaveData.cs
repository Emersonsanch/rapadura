using System;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>One placed instance (Housing.md §12.2 <c>placed[]</c>). JsonUtility-friendly: public fields, enums as ints.</summary>
    [Serializable]
    public class HousePlacedData
    {
        public int instanceId;
        public string itemId;
        public HouseLayer layer;
        /// <summary>Wall layer only.</summary>
        public HouseWallSide wall;
        /// <summary>Grid origin (min corner) on the layer's grid; wall: column / row.</summary>
        public int x;
        public int y;
        /// <summary>Rotation in steps of the item's rotation step (90° → 0..3, 45° → 0..7).</summary>
        public int rotation;
        /// <summary>Wall items do not turn; they mirror.</summary>
        public bool mirrored;
        /// <summary>Surface layer only: the furniture it stands on and which of its spots.</summary>
        public int parentInstanceId;
        public int slot;

        public HousePlacedData Clone() => (HousePlacedData)MemberwiseClone();

        public bool SameAs(HousePlacedData other)
        {
            return other != null && instanceId == other.instanceId && itemId == other.itemId && layer == other.layer
                && wall == other.wall && x == other.x && y == other.y && rotation == other.rotation && mirrored == other.mirrored
                && parentInstanceId == other.parentInstanceId && slot == other.slot;
        }
    }

    /// <summary>One depot entry: copies of the same id stack (Housing.md §2.4).</summary>
    [Serializable]
    public class HouseDepotEntry
    {
        public string itemId;
        public int quantity;

        public HouseDepotEntry()
        {
        }

        public HouseDepotEntry(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = quantity;
        }
    }

    /// <summary>What the player asks for when placing or moving an item.</summary>
    public struct HousePlacementRequest
    {
        public string ItemId;
        public HouseLayer Layer;
        public HouseWallSide Wall;
        public int X;
        public int Y;
        public int Rotation;
        public bool Mirrored;
        public int ParentInstanceId;
        public int Slot;

        public static HousePlacementRequest At(string itemId, HouseLayer layer, int x, int y, int rotation = 0)
        {
            return new HousePlacementRequest { ItemId = itemId, Layer = layer, X = x, Y = y, Rotation = rotation };
        }

        public static HousePlacementRequest OnWall(string itemId, HouseWallSide wall, int column, int row, bool mirrored = false)
        {
            return new HousePlacementRequest { ItemId = itemId, Layer = HouseLayer.Wall, Wall = wall, X = column, Y = row, Mirrored = mirrored };
        }

        public static HousePlacementRequest OnSurface(string itemId, int parentInstanceId, int slot, int rotation = 0)
        {
            return new HousePlacementRequest { ItemId = itemId, Layer = HouseLayer.Surface, ParentInstanceId = parentInstanceId, Slot = slot, Rotation = rotation };
        }

        public static HousePlacementRequest From(HousePlacedData placed)
        {
            return new HousePlacementRequest
            {
                ItemId = placed.itemId,
                Layer = placed.layer,
                Wall = placed.wall,
                X = placed.x,
                Y = placed.y,
                Rotation = placed.rotation,
                Mirrored = placed.mirrored,
                ParentInstanceId = placed.parentInstanceId,
                Slot = placed.slot,
            };
        }
    }
}
