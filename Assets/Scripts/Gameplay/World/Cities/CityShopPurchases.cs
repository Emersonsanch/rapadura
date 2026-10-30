using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Shop;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>What a city-shop purchase did besides putting an item in the bag.</summary>
    public enum CityShopPurchaseEffect
    {
        None = 0,
        /// <summary>A Alforje de Feltro grew the Mochila (<see cref="CityShopPurchaseAppliedEvent.Value"/> = rows).</summary>
        BagExpanded = 1,
        /// <summary>A Alforje de Feltro was bought but the bag was already at its cap: it stays in the bag.</summary>
        BagAlreadyFull = 2,
        /// <summary>A house item went to the house depot (<see cref="CityShopPurchaseAppliedEvent.Value"/> = quantity).</summary>
        HouseItemToDepot = 3,
        /// <summary>The freguesia reached a new tier (<see cref="CityShopPurchaseAppliedEvent.Value"/> = tier).</summary>
        ReputationUp = 4,
    }

    /// <summary>Raised after <see cref="CityShopPurchases.Apply"/> did something the HUD can tell (toasts: <see cref="CityShopTexts"/>).</summary>
    public readonly struct CityShopPurchaseAppliedEvent : IGameEvent
    {
        public readonly string ShopId;
        public readonly string ItemId;
        public readonly CityShopPurchaseEffect Effect;
        public readonly int Value;

        public CityShopPurchaseAppliedEvent(string shopId, string itemId, CityShopPurchaseEffect effect, int value)
        {
            ShopId = shopId;
            ItemId = itemId;
            Effect = effect;
            Value = value;
        }
    }

    /// <summary>
    /// After-purchase rules of the themed shops (<see cref="CityProgress"/> calls it on every
    /// <see cref="ItemPurchasedEvent"/>):
    /// <list type="bullet">
    /// <item>Freguesia: the gold spent at a themed counter becomes points of that shop (<see cref="ShopReputation"/>).</item>
    /// <item>Alforje de Feltro: the buyer's Mochila gets <see cref="CityShopCatalog.BornalRows"/> rows for good
    /// (<see cref="InventoryManager.AddBagRows"/>, saved in the bag) and the satchel leaves the bag, like a mount whistle.</item>
    /// <item>House items: the bag only carried the purchase for a moment; they go to the house depot
    /// (<see cref="HouseLayout.GrantToDepot"/>, never lost) and leave the bag.</item>
    /// </list>
    /// <see cref="Classify"/> is the pure part (the server applies the same decision to its own bag and depot).
    /// </summary>
    public static class CityShopPurchases
    {
        public enum Kind
        {
            Plain = 0,
            Bornal = 1,
            HouseItem = 2,
        }

        /// <summary>What buying <paramref name="itemId"/> at <paramref name="shopId"/> is.</summary>
        public static Kind Classify(string shopId, string itemId)
        {
            if (itemId == CityShopCatalog.BornalItemId)
            {
                return Kind.Bornal;
            }

            CityShopEntry entry = CityShopCatalog.EntryOf(shopId, itemId);
            return entry != null && entry.Kind == CityShopEntryKind.HouseItem ? Kind.HouseItem : Kind.Plain;
        }

        /// <summary>Applies <paramref name="evt"/> to <paramref name="cities"/>, the buyer's bag (open shop) and the house.</summary>
        public static void Apply(ItemPurchasedEvent evt, CityProgress cities)
        {
            ServiceLocator.TryGet(out ShopManager shops);
            Apply(evt, cities, shops?.BuyerInventory, HouseService.ResolveShared());
        }

        /// <summary>Testable core of <see cref="Apply(ItemPurchasedEvent, CityProgress)"/>.</summary>
        public static void Apply(ItemPurchasedEvent evt, CityProgress cities, InventoryManager buyer, HouseService house)
        {
            if (CityShopCatalog.Get(evt.ShopId) == null && evt.ItemId != CityShopCatalog.BornalItemId)
            {
                return;
            }

            int quantity = Math.Max(1, evt.Quantity);
            switch (Classify(evt.ShopId, evt.ItemId))
            {
                case Kind.Bornal:
                    cities?.RecordBornal();
                    if (buyer != null && buyer.AddBagRows(CityShopCatalog.BornalRows))
                    {
                        buyer.RemoveById(evt.ItemId, 1);
                        Publish(evt, CityShopPurchaseEffect.BagExpanded, CityShopCatalog.BornalRows);
                    }
                    else
                    {
                        Publish(evt, CityShopPurchaseEffect.BagAlreadyFull, 0);
                    }

                    break;

                case Kind.HouseItem:
                    if (house != null)
                    {
                        house.Layout.GrantToDepot(evt.ItemId, quantity);
                        buyer?.RemoveById(evt.ItemId, quantity);
                        Publish(evt, CityShopPurchaseEffect.HouseItemToDepot, quantity);
                    }

                    break;
            }

            if (cities != null && CityShopCatalog.Get(evt.ShopId) != null
                && cities.AddReputation(evt.ShopId, ShopReputation.PointsFor(evt.TotalPrice)))
            {
                Publish(evt, CityShopPurchaseEffect.ReputationUp, cities.ReputationTier(evt.ShopId));
            }
        }

        private static void Publish(ItemPurchasedEvent evt, CityShopPurchaseEffect effect, int value)
        {
            EventBus.Publish(new CityShopPurchaseAppliedEvent(evt.ShopId, evt.ItemId, effect, value));
        }
    }
}
