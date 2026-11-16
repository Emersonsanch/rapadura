using Rapadura.Core.Events;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// Raised by <see cref="InventoryManager.UseItemInSlot"/> after a consumable was eaten or drunk (one unit left the bag), so map mechanics can
    /// react to what the hero used without the inventory knowing about them (the Água Viva of a potion cleaning the mud of the Brejo, 03).
    /// </summary>
    public readonly struct ItemConsumedEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly ItemCategory Category;

        /// <summary>The GameObject of the bag (the hero's), to tell the hero's use apart from any other bag.</summary>
        public readonly GameObject User;

        public ItemConsumedEvent(string itemId, ItemCategory category, GameObject user)
        {
            ItemId = itemId;
            Category = category;
            User = user;
        }
    }

    /// <summary>
    /// What a consumable does besides restoring and buffing (pure and server-runnable; <see cref="InventoryManager.UseItemInSlot"/> applies it).
    /// <code>
    /// Garrafa de Ar Sereno (Mercado dos Túneis, 22; Docs/Mapas100.md §3.3 "limpa lentidão e dá +20% de fôlego por 60 s"): removes every slow
    ///   (MoveSpeed debuff) on the hero when opened; an area that keeps slowing (mud, still water) slows again on its next refresh
    /// </code>
    /// </summary>
    public static class ConsumableEffectRules
    {
        public const string PureAirBottle = "item_garrafa_ar_sereno";

        /// <summary>Whether using <paramref name="itemId"/> removes the hero's slows.</summary>
        public static bool CleansSlows(string itemId) => itemId == PureAirBottle;
    }
}
