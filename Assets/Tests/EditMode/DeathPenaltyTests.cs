using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Morrer perde XP" with its two protections (<see cref="DeathXpLossRules"/>, <see cref="DeathPenaltyRules"/>,
    /// <see cref="DeathPenaltyService"/>, <see cref="DeathProtectionState"/>): the 5% loss, never de-levelling, the
    /// grace/boss exemptions, Bênção da Romaria (−60% XP, no drop) vs Patuá da Vó Zefa (nothing lost, used first), the
    /// oratory price and purchase, Vó Zefa's Patuá gift (1×1 in the bag, else her chest) and the save round trip.
    /// </summary>
    public class DeathPenaltyTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        private ItemDefinition _sword;
        private ItemDefinition _potion;
        private ItemDefinition _gold;
        private ItemDefinition _patua;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _sword = Create("pen_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 1);
            _potion = Create("pen_potion", ItemType.Consumable, ItemCategory.Potion, 20);
            _gold = Create(RomariaBlessingRules.CurrencyItemId, ItemType.Currency, ItemCategory.Currency, 9999);
            _patua = Create(PatuaRules.ItemId, ItemType.Material, ItemCategory.Miscellaneous, 5);
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, new Dictionary<string, ItemDefinition>(_items));
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

        // ------------------------------------------------------------------
        // XP loss math
        // ------------------------------------------------------------------

        [Test]
        public void Loss_IsFivePercentOfTheLevelsXp()
        {
            Assert.AreEqual(0.05f, DeathXpLossRules.LossFraction);
            Assert.AreEqual(7, DeathXpLossRules.Compute(5, 100, 146, DeathProtection.None));   // 7.3
            Assert.AreEqual(12, DeathXpLossRules.Compute(10, 200, 236, DeathProtection.None)); // 11.8
            Assert.AreEqual(31, DeathXpLossRules.Compute(20, 500, 612, DeathProtection.None)); // 30.6
        }

        [Test]
        public void Loss_NeverDeLevels()
        {
            Assert.AreEqual(3, DeathXpLossRules.Compute(10, 3, 236, DeathProtection.None), "only what is inside the level");
            Assert.AreEqual(0, DeathXpLossRules.Compute(10, 0, 236, DeathProtection.None), "just levelled up: nothing to lose");
            Assert.AreEqual(0, DeathXpLossRules.Compute(10, -5, 236, DeathProtection.None));

            for (int xp = 0; xp < 300; xp += 7)
            {
                int loss = DeathXpLossRules.Compute(12, xp, 285, DeathProtection.None);
                Assert.That(loss, Is.InRange(0, xp));
            }
        }

        [Test]
        public void NoLoss_InTheFirstLevels_NorInFirstCampaignBossFights()
        {
            Assert.AreEqual(0, DeathXpLossRules.Compute(1, 50, 100, DeathProtection.None), "tutorial / first fights");
            Assert.AreEqual(0, DeathXpLossRules.Compute(DeathXpLossRules.GraceLevel - 1, 50, 110, DeathProtection.None));
            Assert.Greater(DeathXpLossRules.Compute(DeathXpLossRules.GraceLevel, 50, 121, DeathProtection.None), 0);

            Assert.AreEqual(0, DeathXpLossRules.Compute(9, 100, 214, DeathProtection.None, bossChallenge: true, newGamePlusCycle: 0));
            Assert.AreEqual(11, DeathXpLossRules.Compute(9, 100, 214, DeathProtection.None, bossChallenge: true, newGamePlusCycle: 1),
                "Nova Jornada+ bosses cost XP");
        }

        [Test]
        public void Blessing_CutsSixtyPercent_AndPatuaCancelsTheLoss()
        {
            int full = DeathXpLossRules.Compute(20, 500, 612, DeathProtection.None);
            int blessed = DeathXpLossRules.Compute(20, 500, 612, DeathProtection.Blessing);
            Assert.AreEqual(31, full);
            Assert.AreEqual(12, blessed); // 31 × 0.4 = 12.4
            Assert.AreEqual(0, DeathXpLossRules.Compute(20, 500, 612, DeathProtection.Patua));
        }

        [Test]
        public void Patua_GoesFirst_AndAnyProtectionStopsTheDrop()
        {
            Assert.AreEqual(DeathProtection.Patua, DeathPenaltyRules.Resolve(hasPatua: true, blessed: true));
            Assert.AreEqual(DeathProtection.Patua, DeathPenaltyRules.Resolve(true, false));
            Assert.AreEqual(DeathProtection.Blessing, DeathPenaltyRules.Resolve(false, true));
            Assert.AreEqual(DeathProtection.None, DeathPenaltyRules.Resolve(false, false));

            Assert.IsTrue(DeathPenaltyRules.AllowsItemDrop(DeathProtection.None));
            Assert.IsFalse(DeathPenaltyRules.AllowsItemDrop(DeathProtection.Blessing));
            Assert.IsFalse(DeathPenaltyRules.AllowsItemDrop(DeathProtection.Patua));
        }

        // ------------------------------------------------------------------
        // Service: three deaths in a row
        // ------------------------------------------------------------------

        [Test]
        public void Service_UsesPatuaThenBlessingThenNothing()
        {
            InventoryManager bag = NewBag();
            bag.AddItem(_sword, 1);
            bag.AddItem(_patua, 1);
            var lostAndFound = new LostAndFoundList();
            var state = new DeathProtectionState();
            state.Bless();
            var context = new DeathPenaltyContext(10, 200, 236);

            // 1st death: the Patuá is used up, the Bênção stays, nothing lost even with a sure drop roll.
            DeathPenaltyResult first = DeathPenaltyService.Apply(context, bag, lostAndFound, state, new AlwaysRandom(), Resolve);
            Assert.AreEqual(DeathProtection.Patua, first.Protection);
            Assert.AreEqual(0, first.ExperienceLost);
            Assert.IsFalse(first.Drop.Dropped);
            Assert.AreEqual(0, bag.GetTotalCount(PatuaRules.ItemId));
            Assert.IsTrue(state.IsBlessed);

            // 2nd death: the Bênção acts and ends; 40% of the XP loss, no drop.
            DeathPenaltyResult second = DeathPenaltyService.Apply(context, bag, lostAndFound, state, new AlwaysRandom(), Resolve);
            Assert.AreEqual(DeathProtection.Blessing, second.Protection);
            Assert.AreEqual(5, second.ExperienceLost); // 12 × 0.4 = 4.8
            Assert.IsFalse(second.Drop.Dropped);
            Assert.IsFalse(state.IsBlessed);
            Assert.AreEqual(1, bag.GetTotalCount(_sword.ItemId));

            // 3rd death: unprotected, full loss and the 9% roll (forced here) takes the sword.
            DeathPenaltyResult third = DeathPenaltyService.Apply(context, bag, lostAndFound, state, new AlwaysRandom(), Resolve);
            Assert.AreEqual(DeathProtection.None, third.Protection);
            Assert.AreEqual(12, third.ExperienceLost);
            Assert.IsTrue(third.Drop.Dropped);
            Assert.AreEqual(1, lostAndFound.Count);
        }

        // ------------------------------------------------------------------
        // Oratório: price and purchase
        // ------------------------------------------------------------------

        [Test]
        public void BlessingPrice_ScalesWithLevel_AndIsCapped()
        {
            Assert.AreEqual(6, RomariaBlessingRules.Price(1));
            Assert.AreEqual(10, RomariaBlessingRules.Price(5));
            Assert.AreEqual(15, RomariaBlessingRules.Price(10));
            Assert.AreEqual(25, RomariaBlessingRules.Price(20));
            Assert.AreEqual(RomariaBlessingRules.MaxPrice, RomariaBlessingRules.Price(200));
        }

        [Test]
        public void Oratory_InEveryCity_SellsTheBlessingOnce()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                Assert.IsNotNull(RomariaOratory.ForCity(city.CityId), city.CityId + " has no oratório");
                Assert.IsFalse(string.IsNullOrEmpty(DeathProtectionTexts.GetFallback(RomariaOratory.ForCity(city.CityId).NameKey)));
            }

            foreach (OratorySpec oratory in RomariaOratory.Oratories.Where(o => o.KeeperResidentId != null))
            {
                Assert.IsTrue(CityCatalog.AllResidents().Any(r => r.ResidentId == oratory.KeeperResidentId), oratory.KeeperResidentId);
            }

            InventoryManager bag = NewBag();
            var state = new DeathProtectionState();
            bag.AddItem(_gold, 14);
            Assert.AreEqual(BlessingPurchase.NotEnoughGold, RomariaOratory.TryBuy(bag, state, 10));
            Assert.AreEqual(14, bag.GetTotalCount(_gold.ItemId), "nothing charged");

            bag.AddItem(_gold, 6);
            Assert.AreEqual(BlessingPurchase.Ok, RomariaOratory.TryBuy(bag, state, 10));
            Assert.AreEqual(5, bag.GetTotalCount(_gold.ItemId));
            Assert.IsTrue(state.IsBlessed);
            Assert.AreEqual(BlessingPurchase.AlreadyBlessed, RomariaOratory.TryBuy(bag, state, 10));
            Assert.AreEqual(5, bag.GetTotalCount(_gold.ItemId));
        }

        // ------------------------------------------------------------------
        // Vó Zefa's Patuá (1×1, bag first, chest otherwise)
        // ------------------------------------------------------------------

        [Test]
        public void PatuaGift_TakesOneCell_InTheBag_OrGoesToTheChest()
        {
            Assert.AreEqual(ItemFootprint.OneByOne, ItemFootprint.For(_patua));

            var state = new DeathProtectionState();
            var chest = new VillageChestStorage(10, Resolve);
            InventoryManager bag = NewBag();
            Assert.IsTrue(state.GiftPatua(bag, chest, Resolve));
            Assert.AreEqual(1, bag.GetTotalCount(PatuaRules.ItemId));

            InventoryManager full = NewBag(1, 1);
            full.AddItem(_potion, 1);
            Assert.IsFalse(full.CanFit(_patua, 1));
            Assert.IsTrue(state.GiftPatua(full, chest, Resolve));
            Assert.AreEqual(0, full.GetTotalCount(PatuaRules.ItemId));
            Assert.AreEqual(1, chest.GetTotalCount(PatuaRules.ItemId));
            Assert.AreEqual(2, state.PatuasGifted);
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        [Test]
        public void Save_RoundTripsTheBlessing_AndOldSavesLoadUnblessed()
        {
            var state = new DeathProtectionState();
            state.Bless();
            string json = JsonUtility.ToJson(state.CaptureState());

            var loaded = new DeathProtectionState();
            loaded.RestoreState(JsonUtility.FromJson<DeathProtectionSaveState>(json));
            Assert.IsTrue(loaded.IsBlessed);
            Assert.AreEqual(1, loaded.BlessingsBought);

            var old = new DeathProtectionState();
            old.RestoreState(null);
            Assert.IsFalse(old.IsBlessed, "a save from before the blessing has no entry");

            loaded.ResetForNewSession();
            Assert.IsFalse(loaded.IsBlessed);
            Assert.AreEqual(0, loaded.BlessingsBought);
        }

        // ------------------------------------------------------------------

        private sealed class AlwaysRandom : System.Random
        {
            public override double NextDouble() => 0.0;
            protected override double Sample() => 0.0;
        }

        private ItemDefinition Resolve(string itemId) => itemId != null && _items.TryGetValue(itemId, out ItemDefinition item) ? item : null;

        private ItemDefinition Create(string id, ItemType type, ItemCategory category, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            _items[id] = item;
            return item;
        }

        private InventoryManager NewBag(int columns = InventoryManager.DefaultColumns, int rows = InventoryManager.DefaultRows)
        {
            var go = new GameObject("DeathPenaltyTests_Bag");
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
    }
}
