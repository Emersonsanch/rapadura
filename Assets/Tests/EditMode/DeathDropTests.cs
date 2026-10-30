using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Death;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 9% death drop (<see cref="DeathDropRules"/>, <see cref="DeathDropService"/>) and Vó Zefa's "Achados e Perdidos"
    /// (<see cref="LostAndFoundList"/>, saved with the village chest): the constant, a seeded rate, eligibility (no gold,
    /// quest/story items, bound items or relics; equipped gear is never in the backpack), the whole entry moving with its
    /// affixes, the buy-back fee by rarity, gold/room checks and the save round trip.
    /// </summary>
    public class DeathDropTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        private ItemDefinition _sword;
        private ItemDefinition _potion;
        private ItemDefinition _gold;
        private ItemDefinition _questItem;
        private ItemDefinition _bound;
        private ItemDefinition _relic;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _sword = Create("drop_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 1, ItemRarity.Uncommon, EquipmentSlot.MainHand);
            _potion = Create("drop_potion", ItemType.Consumable, ItemCategory.Potion, 20, ItemRarity.Common);
            _gold = Create(LostAndFoundList.FeeCurrencyItemId, ItemType.Currency, ItemCategory.Currency, 9999, ItemRarity.Common);
            _questItem = Create("drop_letter", ItemType.QuestItem, ItemCategory.QuestItem, 1, ItemRarity.Common);
            _bound = Create("drop_amulet", ItemType.Armor, ItemCategory.Accessory, 1, ItemRarity.Epic, EquipmentSlot.Accessory, bound: true);
            _relic = Create(DeathDropRules.RelicIdPrefix + "carranca", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Legendary);
            RegisterInDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            foreach (Object obj in _created.Where(o => o != null))
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            _items.Clear();
        }

        [Test]
        public void Chance_IsNinePercent()
        {
            Assert.AreEqual(0.09f, DeathDropRules.DropChance);
        }

        [Test]
        public void Roll_IsSeeded_AndHitsAboutNinePercent()
        {
            var random = new System.Random(1234);
            int hits = 0;
            const int Deaths = 20000;
            for (int i = 0; i < Deaths; i++)
            {
                if (DeathDropRules.RollsDrop(random))
                {
                    hits++;
                }
            }

            Assert.AreEqual(0.09, hits / (double)Deaths, 0.01);

            // Same seed, same story.
            var a = new System.Random(7);
            var b = new System.Random(7);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(DeathDropRules.RollsDrop(a), DeathDropRules.RollsDrop(b));
            }

            Assert.IsFalse(DeathDropRules.RollsDrop(null));
        }

        [Test]
        public void Eligibility_ExcludesGoldQuestBoundAndRelics()
        {
            Assert.IsTrue(DeathDropRules.IsEligible(Slot(_sword, 1), _sword));
            Assert.IsTrue(DeathDropRules.IsEligible(Slot(_potion, 5), _potion));
            Assert.IsFalse(DeathDropRules.IsEligible(Slot(_gold, 100), _gold), "gold");
            Assert.IsFalse(DeathDropRules.IsEligible(Slot(_questItem, 1), _questItem), "quest/story item");
            Assert.IsFalse(DeathDropRules.IsEligible(Slot(_bound, 1), _bound), "bound");
            Assert.IsFalse(DeathDropRules.IsEligible(Slot(_relic, 1), _relic), "relic");
            Assert.IsFalse(DeathDropRules.IsEligible(Slot(_sword, 1), null), "unknown id");
            Assert.IsFalse(DeathDropRules.IsEligible(InventorySlotData.Empty(), _sword));
        }

        [Test]
        public void PickDrop_OnlyEverPicksEligibleEntries()
        {
            InventorySlotData[] slots =
            {
                Slot(_gold, 300), Slot(_questItem, 1), Slot(_sword, 1), InventorySlotData.Empty(), Slot(_bound, 1), Slot(_potion, 4),
            };

            var random = new System.Random(99);
            var picked = new HashSet<int>();
            int drops = 0;
            for (int i = 0; i < 5000; i++)
            {
                int index = DeathDropRules.PickDrop(slots, Resolve, random);
                if (index >= 0)
                {
                    drops++;
                    picked.Add(index);
                }
            }

            CollectionAssert.AreEquivalent(new[] { 2, 5 }, picked, "only the sword and the potions");
            Assert.AreEqual(0.09, drops / 5000.0, 0.02);

            InventorySlotData[] nothingToDrop = { Slot(_gold, 10), Slot(_questItem, 1) };
            var always = new AlwaysRandom();
            Assert.AreEqual(-1, DeathDropRules.PickDrop(nothingToDrop, Resolve, always), "a successful roll with nothing eligible drops nothing");
        }

        [Test]
        public void Fee_ScalesWithRarity_GeneratedRollCountsAtItsRarity()
        {
            CollectionAssert.AreEqual(new[] { 10, 25, 60, 150, 400 }, DeathDropRules.BuybackFeeByRarity);
            Assert.AreEqual(25, DeathDropRules.BuybackFee(DeathDropRules.RarityOf(Slot(_sword, 1), _sword)));

            InventorySlotData rolled = Slot(_sword, 1);
            rolled.generatedName = "Peixeira Épica";
            rolled.generatedRarity = ItemRarity.Epic;
            Assert.AreEqual(ItemRarity.Epic, DeathDropRules.RarityOf(rolled, _sword));
            Assert.AreEqual(150, DeathDropRules.BuybackFee(ItemRarity.Epic));
        }

        [Test]
        public void Apply_MovesTheWholeEntry_ToLostAndFound_AndNeverTouchesEquipment()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_gold, 500);
            bag.AddItem(_sword, 1);
            Assert.IsTrue(bag.EquipFromSlot(Index(bag, _sword.ItemId)), "the equipped sword is not in the grid");
            bag.AddItem(_potion, 6);

            var lostAndFound = new LostAndFoundList();
            int events = 0;
            EventBus.Subscribe<ItemDroppedOnDeathEvent>(_ => events++);

            DeathDropResult result = DeathDropService.Apply(bag, lostAndFound, new AlwaysRandom(), Resolve);

            Assert.IsTrue(result.Dropped);
            Assert.AreEqual(_potion.ItemId, result.Item.itemId, "the only eligible backpack entry");
            Assert.AreEqual(6, result.Item.quantity, "the whole stack");
            Assert.AreEqual(10, result.Fee);
            Assert.AreEqual(0, bag.GetTotalCount(_potion.ItemId));
            Assert.AreEqual(500, bag.GetTotalCount(_gold.ItemId));
            Assert.AreEqual(_sword.ItemId, bag.Equipped[EquipmentSlot.MainHand].itemId);
            Assert.AreEqual(1, lostAndFound.Count);
            Assert.AreEqual(1, events);
            Assert.AreEqual("6 drop_potion", DeathScreenController.DescribeDrop(result.Item));

            Assert.IsFalse(DeathDropService.Apply(bag, lostAndFound, new NeverRandom(), Resolve).Dropped, "91%: nothing happens");
            Assert.AreEqual(1, lostAndFound.Count);
        }

        [Test]
        public void BuyBack_NeedsGoldAndRoom_AndRestoresAffixes()
        {
            InventoryManager bag = NewBag();
            var lostAndFound = new LostAndFoundList();
            InventorySlotData rolled = Slot(_sword, 1);
            rolled.generatedName = "Peixeira Épica";
            rolled.generatedRarity = ItemRarity.Epic;
            rolled.generatedAffixIds = new[] { "affix_x" };
            rolled.generatedAffixValues = new[] { 3f };
            lostAndFound.Add(rolled, 150);

            bag.AddItem(_gold, 149);
            Assert.AreEqual(LostAndFoundBuyback.NotEnoughGold, lostAndFound.TryBuyBack(0, bag));
            Assert.AreEqual(149, bag.GetTotalCount(_gold.ItemId));

            bag.AddItem(_gold, 51);
            Assert.AreEqual(LostAndFoundBuyback.Ok, lostAndFound.TryBuyBack(0, bag));
            Assert.AreEqual(50, bag.GetTotalCount(_gold.ItemId));
            InventorySlotData back = bag.Slots[Index(bag, _sword.ItemId)];
            Assert.AreEqual("Peixeira Épica", back.generatedName);
            CollectionAssert.AreEqual(new[] { "affix_x" }, back.generatedAffixIds);
            Assert.AreEqual(0, lostAndFound.Count);
            Assert.AreEqual(LostAndFoundBuyback.Invalid, lostAndFound.TryBuyBack(0, bag));

            // No room: nothing paid, the entry stays.
            InventoryManager full = NewBag(1, 2);
            full.AddItem(_gold, 999);
            full.AddItem(_potion, 1);
            lostAndFound.Add(Slot(_sword, 1), 25);
            Assert.AreEqual(LostAndFoundBuyback.BagFull, lostAndFound.TryBuyBack(0, full));
            Assert.AreEqual(999, full.GetTotalCount(_gold.ItemId));
            Assert.AreEqual(1, lostAndFound.Count);
        }

        [Test]
        public void LostAndFound_IsSavedWithTheChest_AndClearedOnNewGame()
        {
            var chest = new VillageChestStorage(resolveItem: Resolve);
            InventorySlotData rolled = Slot(_sword, 1);
            rolled.generatedName = "Peixeira";
            rolled.generatedRarity = ItemRarity.Rare;
            chest.LostAndFound.Add(rolled, 60);
            chest.LostAndFound.Add(Slot(_potion, 3), 10);

            var saved = JsonUtility.FromJson<VillageChestSaveState>(JsonUtility.ToJson(chest.CaptureState()));
            var loaded = new VillageChestStorage(resolveItem: Resolve);
            loaded.RestoreState(saved);

            Assert.AreEqual(2, loaded.LostAndFound.Count);
            Assert.AreEqual("Peixeira", loaded.LostAndFound.Entries[0].item.generatedName);
            Assert.AreEqual(60, loaded.LostAndFound.Entries[0].fee);
            Assert.AreEqual(3, loaded.LostAndFound.Entries[1].item.quantity);

            // An old chest save (before Achados e Perdidos) loads with an empty list.
            var old = JsonUtility.FromJson<VillageChestSaveState>("{\"slots\":[]}");
            loaded.RestoreState(old);
            Assert.AreEqual(0, loaded.LostAndFound.Count);

            chest.Clear();
            Assert.AreEqual(0, chest.LostAndFound.Count, "Novo jogo empties it");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>NextDouble() = 0: every roll succeeds; Next(n) = 0.</summary>
        private sealed class AlwaysRandom : System.Random
        {
            public override double NextDouble() => 0.0;
            public override int Next(int maxValue) => 0;
        }

        /// <summary>NextDouble() just under 1: every roll fails.</summary>
        private sealed class NeverRandom : System.Random
        {
            public override double NextDouble() => 0.999;
        }

        private ItemDefinition Resolve(string itemId) => itemId != null && _items.TryGetValue(itemId, out ItemDefinition item) ? item : null;

        private ItemDefinition Create(string id, ItemType type, ItemCategory category, int maxStack, ItemRarity rarity, EquipmentSlot equipSlot = EquipmentSlot.None, bool bound = false)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.FindProperty("_isBound").boolValue = bound;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            _items[id] = item;
            return item;
        }

        private static InventorySlotData Slot(ItemDefinition item, int quantity) => new InventorySlotData { itemId = item.ItemId, quantity = quantity };

        private void RegisterInDatabase()
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, new Dictionary<string, ItemDefinition>(_items));
        }

        private InventoryManager NewBag(int columns = InventoryManager.DefaultColumns, int rows = InventoryManager.DefaultRows)
        {
            var go = new GameObject("DeathDropTests_Bag");
            _created.Add(go);
            InventoryManager bag = go.AddComponent<InventoryManager>();
            SetField(bag, "_columns", columns);
            SetField(bag, "_rows", rows);
            SetField(bag, "_slots", null);
            return bag;
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(target, value);
        }

        private static int Index(InventoryManager bag, string itemId)
        {
            for (int i = 0; i < bag.Slots.Count; i++)
            {
                if (bag.Slots[i].itemId == itemId)
                {
                    return i;
                }
            }

            Assert.Fail(itemId + " not in the bag");
            return -1;
        }
    }
}
