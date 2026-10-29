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

        /// <summary>Units let past the weight limit because an active quest needs them (<see cref="LootGrantRules"/>).</summary>
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
    /// Raised by <see cref="EnemyLootGranter"/> when part of a kill's loot did not fit (bag over its weight limit or
    /// out of slots), or when quest items were let past the weight limit. The arena's bag-full toast
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
    /// How a rolled drop goes into the killer's bag (Docs/Balance/README.md §9). Plain static rules so EditMode
    /// tests drive them on a bare <see cref="InventoryManager"/>:
    /// <list type="number">
    /// <item>Normal add (<see cref="InventoryManager.AddItem(ItemDefinition,int)"/>): stacks first, then free slots, capped by weight.</item>
    /// <item>What the weight cap refused is kept anyway when it is <b>currency</b> (gold weighs 0, so this only
    /// matters for a full set of slots, which then also refuses it) or when an active quest still needs it
    /// (<c>outstandingForQuests</c>, i.e. <c>QuestManager.GetOutstandingCollectAmount</c>): those
    /// units go in over the limit (<see cref="InventoryManager.AddItemIgnoringWeight"/>). Slots are never bypassed.</item>
    /// <item>The rest is lost and reported, never silently: <see cref="LootOverflowEvent"/>.</item>
    /// </list>
    /// Generated gear only needs a free slot (<see cref="InventoryManager.AddGeneratedItem"/>).
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
            int keptOverWeight = 0;

            if (leftover > 0)
            {
                int protectedAmount = ProtectedAmount(drop.Item, leftover, inventory.GetTotalCount(drop.Item.ItemId), outstandingForQuests);
                if (protectedAmount > 0)
                {
                    int notPlaced = inventory.AddItemIgnoringWeight(drop.Item, protectedAmount);
                    keptOverWeight = protectedAmount - notPlaced;
                    leftover -= keptOverWeight;
                }
            }

            return new LootGrantResult(drop, drop.Quantity - leftover, leftover, keptOverWeight);
        }

        /// <summary>
        /// Of <paramref name="leftover"/> units the weight cap refused, how many must be kept regardless: all of them
        /// for currency, otherwise what the active quests still need beyond <paramref name="held"/>.
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
