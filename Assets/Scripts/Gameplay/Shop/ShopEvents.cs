using Rapadura.Core.Events;

namespace Rapadura.Gameplay.Shop
{
    /// <summary>Raised when a shop UI/session is opened for a buyer inventory.</summary>
    public readonly struct ShopOpenedEvent : IGameEvent
    {
        public readonly string ShopId;

        public ShopOpenedEvent(string shopId)
        {
            ShopId = shopId;
        }
    }

    /// <summary>Raised after the player successfully buys an item from the active shop.</summary>
    public readonly struct ItemPurchasedEvent : IGameEvent
    {
        public readonly string ShopId;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly int TotalPrice;

        public ItemPurchasedEvent(string shopId, string itemId, int quantity, int totalPrice)
        {
            ShopId = shopId;
            ItemId = itemId;
            Quantity = quantity;
            TotalPrice = totalPrice;
        }
    }

    /// <summary>Raised after the player successfully sells an item to the active shop.</summary>
    public readonly struct ItemSoldEvent : IGameEvent
    {
        public readonly string ShopId;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly int TotalPayout;

        public ItemSoldEvent(string shopId, string itemId, int quantity, int totalPayout)
        {
            ShopId = shopId;
            ItemId = itemId;
            Quantity = quantity;
            TotalPayout = totalPayout;
        }
    }

    /// <summary>Raised when the shop UI/session is closed.</summary>
    public readonly struct ShopClosedEvent : IGameEvent
    {
        public readonly string ShopId;

        public ShopClosedEvent(string shopId)
        {
            ShopId = shopId;
        }
    }
}
