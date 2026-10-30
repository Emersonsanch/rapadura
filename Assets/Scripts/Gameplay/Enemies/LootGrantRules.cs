using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>What happened to one <see cref="LootDrop"/> when it was put in the bag.</summary>
    public readonly struct LootGrantResult
    {
        public readonly LootDrop Drop;

        /// <summary>Units that went in (0 or 1 for a generated piece).</summary>
        public readonly int Granted;

        /// <summary>Units that did not fit and are gone.</summary>
        public readonly int Lost;

        /// <summary>
        /// Dormant since the MU-style grid bag (always 0): quest items used to go in past the old weight limit, but the
        /// grid has no "past the limit". Kept so event consumers (<c>BagFullToastController</c>) stay source-compatible.
        /// </summary>
        public readonly int KeptOverWeight;

        public LootGrantResult(LootDrop drop, int granted, int lost, int keptOverWeight)
        {
            Drop = drop;
            Granted = granted;
            Lost = lost;
            KeptOverWeight = keptOverWeight;
        }

        public bool AnyLost => Lost > 0;
    }

    /// <summary>
    /// Raised by <see cref="EnemyLootGranter"/> when part of a kill's loot did not fit (no free area of its footprint on
    /// the Mochila's grid and no plain stack with room). The arena's bag-full toast
    /// (<c>BagFullToastController</c>) shows "Mochila cheia" and a warning-coloured text over the corpse.
    /// </summary>
    public readonly struct LootOverflowEvent : IGameEvent
    {
        /// <summary>World position of the corpse label.</summary>
        public readonly Vector3 Position;

        /// <summary>Only the drops with <see cref="LootGrantResult.Lost"/> or <see cref="LootGrantResult.KeptOverWeight"/> &gt; 0.</summary>
        public readonly IReadOnlyList<LootGrantResult> Results;

        public LootOverflowEvent(Vector3 position, IReadOnlyList<LootGrantResult> results)
        {
            Position = position;
            Results = results ?? Array.Empty<LootGrantResult>();
        }
    }

    /// <summary>
    /// How a rolled drop goes into the killer's bag (Docs/Balance/README.md §9 and "Mochila em grade"). Plain static
    /// rules so EditMode tests drive them on a bare <see cref="InventoryManager"/>:
    /// <list type="number">
    /// <item>Normal add (<see cref="InventoryManager.AddItem(ItemDefinition,int)"/>): plain stacks with room first, then a
    /// new stack at the first free area of the item's footprint (the shared grid fit, <see cref="InventoryManager.CanFit"/>).</item>
    /// <item>The grid is the only limit and cannot be bypassed (the old weight-limit exception for currency and quest
    /// items is gone with the weight limit; <c>outstandingForQuests</c> is kept for callers and <see cref="ProtectedAmount"/>).</item>
    /// <item>The rest is lost and reported, never silently: <see cref="LootOverflowEvent"/>.</item>
    /// </list>
    /// Generated gear needs a free area of its footprint (<see cref="InventoryManager.AddGeneratedItem"/>).
    /// </summary>
    public static class LootGrantRules
    {
        /// <param name="outstandingForQuests">(itemId, held now) → units active quests still need; null = none.</param>
        public static LootGrantResult Grant(InventoryManager inventory, LootDrop drop, Func<string, int, int> outstandingForQuests)
        {
            if (inventory == null || drop.Item == null)
            {
                return new LootGrantResult(drop, 0, 0, 0);
            }

            if (drop.IsGenerated)
            {
                bool added = inventory.AddGeneratedItem(drop.Generated);
                return new LootGrantResult(drop, added ? 1 : 0, added ? 0 : 1, 0);
            }

            if (drop.Quantity <= 0)
            {
                return new LootGrantResult(drop, 0, 0, 0);
            }

            int leftover = inventory.AddItem(drop.Item, drop.Quantity);
            return new LootGrantResult(drop, drop.Quantity - leftover, leftover, 0);
        }

        /// <summary>
        /// Of <paramref name="leftover"/> units that did not fit, how many matter most: all of them for currency and quest
        /// items, otherwise what the active quests still need beyond <paramref name="held"/>. Informational since the grid
        /// bag (nothing can go past the grid); kept for tests and future "make room" hints.
        /// </summary>
        public static int ProtectedAmount(ItemDefinition item, int leftover, int held, Func<string, int, int> outstandingForQuests)
        {
            if (item == null || leftover <= 0)
            {
                return 0;
            }

            if (item.Type == ItemType.Currency || item.Type == ItemType.QuestItem)
            {
                return leftover;
            }

            int outstanding = outstandingForQuests != null ? outstandingForQuests(item.ItemId, held) : 0;
            return Mathf.Clamp(outstanding, 0, leftover);
        }
    }
}
