using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>Item weight, carry capacity and the encumbrance speed rules (Docs/Balance/README.md "Peso e carga").</summary>
    public class EncumbranceTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        // ------------------------------------------------------------------
        // Item weight
        // ------------------------------------------------------------------

        [Test]
        public void Gold_AndCurrency_AreWeightless_EvenWhenAuthored()
        {
            Assert.AreEqual(0f, ItemWeightRules.UnitWeight(Item("item_gold", ItemType.Material, ItemCategory.Gold, 5f)));
            Assert.AreEqual(0f, ItemWeightRules.UnitWeight(Item("item_coin", ItemType.Currency, ItemCategory.Currency, 1f)));
            Assert.AreEqual(0f, ItemWeightRules.StackWeight(Item("item_gold2", ItemType.Material, ItemCategory.Gold, 0f), 9999));
        }

        [Test]
        public void AuthoredWeight_Wins_OverTheSuggestion()
        {
            ItemDefinition sword = Item("item_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 4.5f);
            Assert.AreEqual(4.5f, ItemWeightRules.UnitWeight(sword), 1e-4f);
        }

        [Test]
        public void ZeroWeight_FallsBackTo_GridCellsTimesCategoryDensity()
        {
            // Sword 1×3 × 1.0 = 3; chest 2×3 × 0.4 = 2.4; a 1×1 quest note never drops below the minimum.
            Assert.AreEqual(3f, ItemWeightRules.UnitWeight(Item("item_s", ItemType.Weapon, ItemCategory.MeleeWeapon, 0f)), 1e-4f);
            Assert.AreEqual(2.4f, ItemWeightRules.UnitWeight(Item("item_c", ItemType.Armor, ItemCategory.Chest, 0f)), 1e-4f);
            Assert.AreEqual(ItemWeightRules.MinUnitWeight, ItemWeightRules.UnitWeight(Item("item_q", ItemType.QuestItem, ItemCategory.QuestItem, 0f)), 1e-4f);
            Assert.Greater(ItemWeightRules.SuggestedUnitWeight(ItemType.Material, ItemCategory.Metal, 1),
                ItemWeightRules.SuggestedUnitWeight(ItemType.Material, ItemCategory.Fiber, 1), "metal is denser than fibre");
        }

        [Test]
        public void EveryNonCurrencyCategory_HasAPositiveWeight()
        {
            foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
            {
                if (ItemWeightRules.IsWeightless(ItemType.Material, category))
                {
                    continue;
                }

                Assert.Greater(ItemWeightRules.SuggestedUnitWeight(ItemType.Material, category, 1), 0f, category.ToString());
            }
        }

        [Test]
        public void Stacks_WeighPerUnit()
        {
            ItemDefinition stone = Item("item_stone_t", ItemType.Material, ItemCategory.Stone, 1f);
            Assert.AreEqual(99f, ItemWeightRules.StackWeight(stone, 99), 1e-3f);
            Assert.AreEqual(0f, ItemWeightRules.StackWeight(stone, 0));
            Assert.AreEqual(0f, ItemWeightRules.UnitWeight(null), "unknown ids weigh nothing");
        }

        [Test]
        public void CarriedLoad_CountsBackpackAndEquipped()
        {
            var items = new Dictionary<string, ItemDefinition>
            {
                { "wood", Item("wood", ItemType.Material, ItemCategory.Wood, 0.5f) },
                { "sword", Item("sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 3f) },
                { "gold", Item("gold", ItemType.Currency, ItemCategory.Gold, 0f) },
            };
            var state = new InventorySaveState
            {
                slots = new[]
                {
                    new InventorySlotData { itemId = "wood", quantity = 20 },
                    new InventorySlotData { itemId = "gold", quantity = 500 },
                    new InventorySlotData { itemId = "missing", quantity = 3 },
                    null,
                },
                equipped = new[] { new InventorySaveState.EquippedEntry { slot = EquipmentSlot.MainHand, itemId = "sword" } },
            };

            float load = EncumbranceRules.WeightOf(state, id => id != null && items.TryGetValue(id, out ItemDefinition item) ? item : null);
            Assert.AreEqual(13f, load, 1e-3f, "10 (wood) + 0 (gold) + 0 (unknown) + 3 (equipped sword)");
        }

        // ------------------------------------------------------------------
        // Capacity
        // ------------------------------------------------------------------

        [Test]
        public void HeroCapacity_GrowsWithLevelAndVitality()
        {
            Assert.AreEqual(60f, EncumbranceRules.HeroCapacity(1, 0));
            Assert.AreEqual(60f + 2f * 49f + 10f, EncumbranceRules.HeroCapacity(50, 10));
            Assert.AreEqual(60f, EncumbranceRules.HeroCapacity(0, -5), "no negative growth");
        }

        [Test]
        public void EveryCatalogMount_HasACapacity_AndThePackDonkeyCarriesMost()
        {
            List<string> ids = MountCatalog.MountIds.ToList();
            Assert.Greater(ids.Count, 0, "the catalog has mounts (21 built today, 30 planned)");
            foreach (string id in ids)
            {
                Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(id), id);
            }

            float donkey = EncumbranceRules.MountBaseCapacity["mount_pack_donkey"];
            Assert.AreEqual(donkey, EncumbranceRules.MountBaseCapacity.Values.Max(), "the Asno de Caravana is the best pack animal");
            Assert.Greater(donkey, EncumbranceRules.MountBaseCapacity["mount_pony"] * 3f);
        }

        [Test]
        public void MountCapacity_GrowsWithMountLevel_AndUnknownIdsUseTheSize()
        {
            Assert.AreEqual(120f, EncumbranceRules.MountCapacity("mount_pony", MountSize.Medio, 1), 1e-3f);
            Assert.AreEqual(120f * 1.38f, EncumbranceRules.MountCapacity("mount_pony", MountSize.Medio, 20), 1e-3f);
            Assert.AreEqual(300f, EncumbranceRules.MountCapacity("mount_unknown", MountSize.Grande, 1), 1e-3f);
        }

        [Test]
        public void RideCapacity_IsMountPlusHalfTheHero()
        {
            var snapshot = new EncumbranceSnapshot(0f, 60f, true, 120f);
            Assert.AreEqual(150f, snapshot.Capacity, 1e-3f);
            Assert.AreEqual(60f, new EncumbranceSnapshot(0f, 60f, false, 120f).Capacity, 1e-3f, "on foot only the hero counts");
        }

        // ------------------------------------------------------------------
        // Speed
        // ------------------------------------------------------------------

        [TestCase(0f, 1f)]
        [TestCase(0.5f, 1f)]
        [TestCase(0.75f, 0.75f)]
        [TestCase(1f, 0.5f)]
        [TestCase(1.25f, 0.4f)]
        [TestCase(3f, 0.3f)]
        public void MountSpeed_FreeToHalf_ThenLinearToMinusHalf_ThenHeavySlowdown(float ratio, float expected)
        {
            Assert.AreEqual(expected, EncumbranceRules.MountSpeedFactor(ratio), 1e-4f);
        }

        [TestCase(0.9f, 1f)]
        [TestCase(1f, 1f)]
        [TestCase(2f, 0.7f)]
        [TestCase(10f, 0.6f)]
        public void HeroSpeed_OnlySlowsOverCapacity_AndMildly(float ratio, float expected)
        {
            Assert.AreEqual(expected, EncumbranceRules.HeroSpeedFactor(ratio), 1e-4f);
        }

        [Test]
        public void Gallop_OnlyUpToFullCapacity()
        {
            Assert.IsTrue(new EncumbranceSnapshot(150f, 60f, true, 120f).CanGallop);
            Assert.IsFalse(new EncumbranceSnapshot(151f, 60f, true, 120f).CanGallop);
            Assert.IsTrue(new EncumbranceSnapshot(500f, 60f, false, 0f).CanGallop, "on foot there is no gallop to block");
        }

        [Test]
        public void ApplyFactor_NeverRaisesTheSpeed_KeepsRootAndFloor()
        {
            Assert.AreEqual(0f, EncumbranceRules.ApplyFactor(0f, 0.5f), "rooted stays rooted");
            Assert.AreEqual(MountRules.MaxRideMultiplier * 0.5f, EncumbranceRules.ApplyFactor(MountRules.MaxRideMultiplier, 0.5f), 1e-4f);
            Assert.AreEqual(1.2f, EncumbranceRules.ApplyFactor(1.2f, 3f), 1e-4f, "a factor above 1 is ignored");
            Assert.AreEqual(MountRules.MinMoveMultiplier, EncumbranceRules.ApplyFactor(0.15f, 0.3f), 1e-4f, "keeps the BuffController floor");
            for (float factor = 0f; factor <= 1f; factor += 0.1f)
            {
                Assert.LessOrEqual(EncumbranceRules.ApplyFactor(MountRules.MaxMoveMultiplier, factor), MountRules.MaxMoveMultiplier + 1e-5f);
            }
        }

        [Test]
        public void Bands_AndToasts()
        {
            Assert.AreEqual(LoadBand.Light, new EncumbranceSnapshot(70f, 60f, true, 120f).Band);
            Assert.AreEqual(LoadBand.Heavy, new EncumbranceSnapshot(100f, 60f, true, 120f).Band);
            Assert.AreEqual(LoadBand.Overloaded, new EncumbranceSnapshot(200f, 60f, true, 120f).Band);
            Assert.AreEqual(LoadBand.Light, new EncumbranceSnapshot(55f, 60f, false, 0f).Band, "on foot no 'heavy' band");
            Assert.AreEqual(LoadBand.Overloaded, new EncumbranceSnapshot(61f, 60f, false, 0f).Band);

            Assert.AreEqual(EncumbranceTexts.ToastMountHeavy, EncumbranceTexts.ToastKey(LoadBand.Heavy, true, false));
            Assert.AreEqual(EncumbranceTexts.ToastMountOverloaded, EncumbranceTexts.ToastKey(LoadBand.Overloaded, true, false));
            Assert.AreEqual(EncumbranceTexts.ToastMountLight, EncumbranceTexts.ToastKey(LoadBand.Light, true, true));
            Assert.AreEqual(EncumbranceTexts.ToastHeroOverloaded, EncumbranceTexts.ToastKey(LoadBand.Overloaded, false, false));
            Assert.AreEqual(EncumbranceTexts.ToastHeroLight, EncumbranceTexts.ToastKey(LoadBand.Light, false, true));
            Assert.IsNull(EncumbranceTexts.ToastKey(LoadBand.Light, true, false));
        }

        // ------------------------------------------------------------------
        // UI text
        // ------------------------------------------------------------------

        [Test]
        public void Tooltip_ShowsUnitAndStackWeight_AndWeightlessGold()
        {
            ItemDefinition wood = Item("item_wood_t", ItemType.Material, ItemCategory.Wood, 0.5f);
            SetPrivateField(wood, "_maxStack", 99);
            Assert.AreEqual("Peso: 0.5", TooltipContentBuilder.WeightLine(wood, 1));
            Assert.AreEqual("Peso: 0.5 cada (20 = 10)", TooltipContentBuilder.WeightLine(wood, 20));
            Assert.AreEqual("Não pesa nada", TooltipContentBuilder.WeightLine(Item("item_gold_t", ItemType.Currency, ItemCategory.Gold, 0f), 300));

            var slot = new InventorySlotData { itemId = "item_wood_t", quantity = 20 };
            CollectionAssert.Contains(TooltipContentBuilder.ForItem(wood, slot).StatLines, "Peso: 0.5 cada (20 = 10)");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private ItemDefinition Item(string id, ItemType type, ItemCategory category, float weight)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            SetPrivateField(item, "_itemId", id);
            SetPrivateField(item, "_displayName", id);
            SetPrivateField(item, "_type", type);
            SetPrivateField(item, "_category", category);
            SetPrivateField(item, "_weight", weight);
            return item;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, fieldName);
            field.SetValue(target, value);
        }
    }
}
