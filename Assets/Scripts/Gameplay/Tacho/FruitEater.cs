using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>Raised after a Fruta do Sertão was eaten (it did something; a refused fruit raises nothing).</summary>
    public readonly struct FruitEatenEvent : IGameEvent
    {
        public readonly string ItemId;
        /// <summary>Fruta do Umbu: the attribute the point came back from.</summary>
        public readonly AttributeType Taken;

        public FruitEatenEvent(string itemId, AttributeType taken)
        {
            ItemId = itemId;
            Taken = taken;
        }
    }

    /// <summary>
    /// Eats a fruit from the bag (<see cref="InventoryManager.UseItemInSlot"/> hands fruits here): the hero's
    /// <see cref="AttributeSet"/> (same GameObject as the bag, i.e. the Player in an encounter) applies it and only then is
    /// one fruit removed. On the world map there is no AttributeSet, so the fruit stays in the bag (<see cref="FruitCheck.NoAttributes"/>).
    /// </summary>
    public static class FruitEater
    {
        public static FruitCheck TryEat(InventoryManager bag, int slotIndex, string itemId)
        {
            if (bag == null || !FruitRules.IsFruit(itemId))
            {
                return FruitCheck.NotAFruit;
            }

            AttributeSet attributes = bag.GetComponent<AttributeSet>();
            if (attributes == null)
            {
                return FruitCheck.NoAttributes;
            }

            AttributeType taken = default;
            FruitCheck check = itemId == FruitRules.GrowthFruit
                ? attributes.TryEatGrowthFruit()
                : attributes.TryEatResetFruit(out taken);
            if (check != FruitCheck.Ok)
            {
                return check;
            }

            bag.RemoveFromSlot(slotIndex, 1);
            EventBus.Publish(new FruitEatenEvent(itemId, taken));
            return check;
        }
    }
}
