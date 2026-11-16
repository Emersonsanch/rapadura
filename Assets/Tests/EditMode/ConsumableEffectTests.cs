using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// What a consumable does besides restoring (Docs/Mapas100.md 01, 03, 05 and 22): the Pomar's apricot, the Gota de Névoa the Coletor hands
    /// over, the potion that is Living Water in the Brejo and the Garrafa de Ar Sereno that shakes off the slows, plus the event the bag raises.
    /// </summary>
    public class ConsumableEffectTests
    {
        private GameObject _owner;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            if (_owner != null)
            {
                Object.DestroyImmediate(_owner);
            }
        }

        [Test]
        public void PureAirBottle_CleansSlows_NothingElseDoes()
        {
            Assert.IsTrue(ConsumableEffectRules.CleansSlows(ConsumableEffectRules.PureAirBottle));
            Assert.AreEqual(TunnelStillAirRules.BottleItemId, ConsumableEffectRules.PureAirBottle, "the same bottle as the still air of the Tunnel Market");
            Assert.IsFalse(ConsumableEffectRules.CleansSlows("item_health_potion"));
            Assert.IsFalse(ConsumableEffectRules.CleansSlows(GarajonayFogRules.DropItemId));
            Assert.IsFalse(ConsumableEffectRules.CleansSlows(null));
        }

        [Test]
        public void MistDrop_IsTheCollectorsOwnPotion_RestoresMana()
        {
            ItemDefinition drop = ItemDatabase.GetById(GarajonayFogRules.DropItemId);
            Assert.IsNotNull(drop, GarajonayFogRules.DropItemId + " is missing from Resources/Items");
            Assert.IsTrue(drop.IsConsumable);
            Assert.AreEqual(ItemCategory.Potion, drop.Category);
            Assert.Greater(drop.ManaRestore, 0f);
            Assert.IsNotNull(drop.Icon, "the Gota de Névoa has its icon");
        }

        [Test]
        public void Apricot_IsFood_RestoresHealthAndStamina()
        {
            ItemDefinition apricot = ItemDatabase.GetById(ZarafshanHarvestRules.Apricot);
            Assert.IsNotNull(apricot, ZarafshanHarvestRules.Apricot + " is missing from Resources/Items");
            Assert.IsTrue(apricot.IsConsumable);
            Assert.AreEqual(ItemCategory.Food, apricot.Category);
            Assert.Greater(apricot.HealthRestore, 0f);
            Assert.Greater(apricot.StaminaRestore, 0f);
            Assert.IsNotNull(apricot.Icon, "the apricot has its icon");
        }

        [Test]
        public void Mire_PotionsAreLivingWater_FoodIsNot()
        {
            Assert.IsTrue(ZarafshanMireRules.PotionCleans(ItemCategory.Potion));
            Assert.IsFalse(ZarafshanMireRules.PotionCleans(ItemCategory.Food), "an apricot does not clean the mud");

            var pools = new List<ZarafshanCleanPool>();
            ZarafshanMireRules.AddPool(pools, Vector2.zero, 10f);
            Assert.IsTrue(ZarafshanMireRules.Cleaned(pools, new Vector2(ZarafshanMireRules.CleanRadius - 0.5f, 0f), 10f + ZarafshanMireRules.CleanSeconds - 1f));
            Assert.IsFalse(ZarafshanMireRules.Cleaned(pools, Vector2.zero, 10f + ZarafshanMireRules.CleanSeconds + 1f), "the pool dries");
        }

        [Test]
        public void UseItemInSlot_RaisesItemConsumed_WithCategoryAndUser()
        {
            _owner = new GameObject("ConsumableOwner");
            var bag = _owner.AddComponent<InventoryManager>();
            ItemDefinition drop = ItemDatabase.GetById(GarajonayFogRules.DropItemId);
            Assert.IsNotNull(drop);
            Assert.AreEqual(0, bag.AddItem(drop, 2));

            var seen = new List<ItemConsumedEvent>();
            System.Action<ItemConsumedEvent> handler = e => seen.Add(e);
            EventBus.Subscribe(handler);
            try
            {
                int slot = -1;
                for (int i = 0; i < bag.Slots.Count; i++)
                {
                    if (!bag.Slots[i].IsEmpty && bag.Slots[i].itemId == drop.ItemId)
                    {
                        slot = i;
                        break;
                    }
                }

                Assert.GreaterOrEqual(slot, 0);
                Assert.IsTrue(bag.UseItemInSlot(slot));
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(1, seen.Count);
            Assert.AreEqual(drop.ItemId, seen[0].ItemId);
            Assert.AreEqual(ItemCategory.Potion, seen[0].Category);
            Assert.AreSame(_owner, seen[0].User);
        }
    }
}
