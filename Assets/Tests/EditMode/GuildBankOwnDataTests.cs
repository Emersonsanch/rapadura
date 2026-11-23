using NUnit.Framework;
using Rapadura.Gameplay.Guild;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The guild bank keeps only item id + count (<see cref="GuildBankRules"/>), so a bag entry with its own data — a generated
    /// roll, refine, MU options or worn durability — is refused (<see cref="GuildRefusal.ItemHasOwnData"/>) instead of
    /// coming back out plain and new.
    /// </summary>
    public class GuildBankOwnDataTests
    {
        private ItemDefinition _sword;
        private ItemDefinition _potion;

        [SetUp]
        public void SetUp()
        {
            _sword = CreateItem("bank_test_sword", true, 100f);
            _potion = CreateItem("bank_test_potion", false, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sword);
            Object.DestroyImmediate(_potion);
        }

        private static GuildState NewGuild()
        {
            GuildCreateResult result = GuildRules.Create("g1", "L", 40, 10000, false, -1d, "Coluna da Glória", "Pela Luz", GuildEmblem.Default, null, 0d);
            Assert.IsTrue(result.Ok, result.Refusal.ToString());
            return result.Guild;
        }

        private static InventorySlotData Slot(string id, int quantity, float durability) =>
            new InventorySlotData { itemId = id, quantity = quantity, currentDurability = durability };

        [Test]
        public void PlainEntries_GoIn()
        {
            GuildState guild = NewGuild();
            Assert.IsFalse(GuildBankRules.CarriesOwnData(Slot(_potion.ItemId, 5, 0f), _potion));
            Assert.IsFalse(GuildBankRules.CarriesOwnData(Slot(_sword.ItemId, 1, 100f), _sword));
            Assert.IsTrue(GuildBankRules.DepositItem(guild, "L", Slot(_potion.ItemId, 5, 0f), _potion, 5, true, 1d).Ok);
            Assert.IsTrue(GuildBankRules.DepositItem(guild, "L", Slot(_sword.ItemId, 1, 100f), _sword, 1, true, 2d).Ok);
            Assert.AreEqual(5, guild.ItemCount(_potion.ItemId));
            Assert.AreEqual(1, guild.ItemCount(_sword.ItemId));
        }

        [Test]
        public void EntriesWithOwnData_AreRefused()
        {
            InventorySlotData refined = Slot(_sword.ItemId, 1, 100f);
            refined.refineLevel = 7;
            InventorySlotData rolled = Slot(_sword.ItemId, 1, 100f);
            rolled.generatedAffixIds = new[] { "affix_str" };
            rolled.generatedAffixValues = new[] { 3f };
            InventorySlotData excellent = Slot(_sword.ItemId, 1, 100f);
            excellent.excellentMask = 1;
            InventorySlotData worn = Slot(_sword.ItemId, 1, 42f);

            foreach (InventorySlotData slot in new[] { refined, rolled, excellent, worn })
            {
                GuildState guild = NewGuild();
                Assert.IsTrue(GuildBankRules.CarriesOwnData(slot, _sword));
                GuildResult result = GuildBankRules.DepositItem(guild, "L", slot, _sword, 1, true, 1d);
                Assert.AreEqual(GuildRefusal.ItemHasOwnData, result.Refusal);
                Assert.AreEqual(0, guild.ItemCount(_sword.ItemId));
                Assert.AreEqual(0, guild.BankLog.Count);
            }
        }

        [Test]
        public void Refusal_HasPortugueseAndEnglishText()
        {
            string key = GuildTexts.RefusalKey(GuildRefusal.ItemHasOwnData);
            Assert.AreEqual("guild.refusal.item_has_own_data", key);
            StringAssert.Contains("não entram no banco", GuildTexts.Refusal(null, GuildRefusal.ItemHasOwnData));
            Assert.IsFalse(string.IsNullOrEmpty(System.Linq.Enumerable.Single(GuildTexts.Entries, e => e.Key == key).En));
        }

        private static ItemDefinition CreateItem(string itemId, bool hasDurability, float maxDurability)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)(hasDurability ? ItemType.Weapon : ItemType.Consumable);
            serialized.FindProperty("_maxStack").intValue = hasDurability ? 1 : 20;
            serialized.FindProperty("_hasDurability").boolValue = hasDurability;
            serialized.FindProperty("_maxDurability").floatValue = maxDurability;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }
    }
}
