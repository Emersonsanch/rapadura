using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="TooltipContentBuilder"/> — pure text formatting for the
    /// inventory/equipment/shop/skill-tree tooltips, no UIDocument required (same rationale as
    /// <c>TooltipPositioningTests</c>). ScriptableObjects are populated via reflection like
    /// <c>ProceduralItemGeneratorTests</c>.
    /// </summary>
    public class TooltipContentBuilderTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        // ---------------------------------------------------------------
        // Items
        // ---------------------------------------------------------------

        [Test]
        public void ForItem_Weapon_ShowsNameRaritySlotDamageAndDescription()
        {
            ItemDefinition sword = CreateItem("item_iron_sword", "Iron Sword", ItemType.Weapon, ItemRarity.Uncommon, EquipmentSlot.MainHand);
            SetPrivateField(sword, "_weaponDamage", 12f);
            SetPrivateField(sword, "_weaponAttackSpeed", 1.5f);
            SetPrivateField(sword, "_description", "A sturdy blade.");

            TooltipContent content = TooltipContentBuilder.ForItem(sword);

            Assert.AreEqual("Iron Sword", content.Title);
            Assert.AreEqual("Incomum · Mão principal", content.Subtitle);
            Assert.AreEqual(ItemRarity.Uncommon, content.Rarity);
            CollectionAssert.Contains(content.StatLines, "Dano: 12");
            CollectionAssert.Contains(content.StatLines, "Velocidade de ataque: 1.5");
            Assert.AreEqual("A sturdy blade.", content.Description);
        }

        [Test]
        public void ForItem_NonEquippable_UsesItemTypeInSubtitle()
        {
            ItemDefinition potion = CreateItem("item_health_potion", "Health Potion", ItemType.Consumable, ItemRarity.Common, EquipmentSlot.None);
            SetPrivateField(potion, "_healthRestore", 25f);

            TooltipContent content = TooltipContentBuilder.ForItem(potion);

            Assert.AreEqual("Comum · Consumível", content.Subtitle);
            CollectionAssert.Contains(content.StatLines, "Restaura 25 de vida");
        }

        [Test]
        public void ForItem_GeneratedSlot_UsesGeneratedNameRolledRarityAndAffixLines()
        {
            ItemDefinition sword = CreateItem("item_iron_sword", "Iron Sword", ItemType.Weapon, ItemRarity.Common, EquipmentSlot.MainHand);
            ItemAffixDefinition sharp = CreateAffix("affix_sharp", StatType.AttackDamage, ModifierApplication.Flat);
            ItemAffixDefinition swift = CreateAffix("affix_swift", StatType.MoveSpeed, ModifierApplication.PercentAdditive);

            var slot = new InventorySlotData
            {
                itemId = "item_iron_sword",
                quantity = 1,
                generatedAffixIds = new[] { "affix_sharp", "affix_swift", "affix_missing" },
                generatedAffixValues = new[] { 4f, 0.1f, 99f },
                generatedName = "Afiada Iron Sword Ligeira",
                generatedRarity = ItemRarity.Rare,
            };

            var affixes = new Dictionary<string, ItemAffixDefinition> { { "affix_sharp", sharp }, { "affix_swift", swift } };
            TooltipContent content = TooltipContentBuilder.ForItem(sword, slot, resolveAffix: id => affixes.TryGetValue(id, out ItemAffixDefinition a) ? a : null);

            Assert.AreEqual("Afiada Iron Sword Ligeira", content.Title);
            Assert.AreEqual(ItemRarity.Rare, content.Rarity);
            StringAssert.StartsWith("Raro", content.Subtitle);
            CollectionAssert.Contains(content.StatLines, "+4 Dano de ataque");
            CollectionAssert.Contains(content.StatLines, "+10% Velocidade de movimento");
            Assert.AreEqual(2, content.StatLines.FindAll(l => l.StartsWith("+")).Count, "unknown affix ids are skipped, not printed");
        }

        [Test]
        public void ResolveRarity_NeverShowsLowerThanBaseItemRarity()
        {
            ItemDefinition epicBase = CreateItem("item_x", "X", ItemType.Armor, ItemRarity.Epic, EquipmentSlot.Chest);
            var oldSaveSlot = new InventorySlotData { itemId = "item_x", quantity = 1, generatedAffixIds = new[] { "a" }, generatedAffixValues = new[] { 1f } };

            Assert.AreEqual(ItemRarity.Epic, TooltipContentBuilder.ResolveRarity(epicBase, oldSaveSlot));
            Assert.AreEqual(ItemRarity.Epic, TooltipContentBuilder.ResolveRarity(epicBase, null));
        }

        [Test]
        public void ForItem_WithSlotAndPrices_AddsDurabilityQuantityAndPriceLines()
        {
            ItemDefinition helmet = CreateItem("item_leather_helmet", "Leather Helmet", ItemType.Armor, ItemRarity.Common, EquipmentSlot.Head);
            SetPrivateField(helmet, "_armorDefense", 3f);
            SetPrivateField(helmet, "_hasDurability", true);
            SetPrivateField(helmet, "_maxDurability", 100f);
            var slot = new InventorySlotData { itemId = "item_leather_helmet", quantity = 1, currentDurability = 80f };

            TooltipContent content = TooltipContentBuilder.ForItem(helmet, slot, buyPrice: 30, sellPrice: 12);

            CollectionAssert.Contains(content.StatLines, "Defesa: 3");
            CollectionAssert.Contains(content.StatLines, "Durabilidade: 80/100");
            CollectionAssert.Contains(content.StatLines, "Preço: 30");
            CollectionAssert.Contains(content.StatLines, "Vende por: 12");
        }

        [Test]
        public void ForItem_Null_ReturnsEmptyContent()
        {
            TooltipContent content = TooltipContentBuilder.ForItem(null);

            Assert.AreEqual(string.Empty, content.Title);
            Assert.IsEmpty(content.StatLines);
            Assert.AreEqual(string.Empty, content.ToPlainText());
        }

        // ---------------------------------------------------------------
        // Modifiers
        // ---------------------------------------------------------------

        [Test]
        public void FormatModifier_FlatPercentNegativeAndFractionStats()
        {
            Assert.AreEqual("+5 Dano de ataque", TooltipContentBuilder.FormatModifier(StatType.AttackDamage, ModifierApplication.Flat, 5f));
            Assert.AreEqual("+10% Velocidade de movimento", TooltipContentBuilder.FormatModifier(StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.1f));
            Assert.AreEqual("-2 Defesa", TooltipContentBuilder.FormatModifier(StatType.Defense, ModifierApplication.Flat, -2f));
            // Crit chance / cost reduction are fractions even when Flat (AffixDatabaseSeeder authors 0.03 = 3%).
            Assert.AreEqual("+3% Redução de custo", TooltipContentBuilder.FormatModifier(StatType.ResourceCostReduction, ModifierApplication.Flat, 0.03f));
        }

        // ---------------------------------------------------------------
        // Localization
        // ---------------------------------------------------------------

        [Test]
        public void Localizer_IsUsed_AndMissingKeyMarkerFallsBackToPtBR()
        {
            ItemDefinition sword = CreateItem("item_sword", "Espada", ItemType.Weapon, ItemRarity.Rare, EquipmentSlot.MainHand);
            SetPrivateField(sword, "_weaponDamage", 8f);

            var ptBR = new Dictionary<string, string>();
            foreach (var entry in TooltipLocalization.Entries)
            {
                ptBR[entry.Key] = entry.PtBR;
            }

            var en = new Dictionary<string, string>();
            foreach (var entry in TooltipLocalization.Entries)
            {
                en[entry.Key] = entry.En;
            }

            en.Remove("ui.tooltip.slot.main_hand");

            TooltipContent content = TooltipContentBuilder.ForItem(sword, localize: key => en.TryGetValue(key, out string text) ? text : $"[{key}]");

            Assert.AreEqual("Rare · Mão principal", content.Subtitle, "missing key '[...]' marker should fall back to the pt-BR text");
            CollectionAssert.Contains(content.StatLines, "Damage: 8");
        }

        [Test]
        public void EveryEnumBasedKey_HasALocalizationEntry()
        {
            foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
            {
                AssertHasKey("ui.tooltip.rarity." + TooltipLocalization.ToSnakeCase(rarity.ToString()));
            }

            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                AssertHasKey("ui.tooltip.stat." + TooltipLocalization.ToSnakeCase(stat.ToString()));
            }

            foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            {
                AssertHasKey("ui.tooltip.type." + TooltipLocalization.ToSnakeCase(type.ToString()));
            }

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot != EquipmentSlot.None)
                {
                    AssertHasKey("ui.tooltip.slot." + TooltipLocalization.ToSnakeCase(slot.ToString()));
                }
            }

            foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
            {
                if (element != ElementType.None)
                {
                    AssertHasKey("ui.tooltip.element." + TooltipLocalization.ToSnakeCase(element.ToString()));
                }
            }
        }

        [Test]
        public void ToSnakeCase_ConvertsPascalCase()
        {
            Assert.AreEqual("attack_damage", TooltipLocalization.ToSnakeCase("AttackDamage"));
            Assert.AreEqual("main_hand", TooltipLocalization.ToSnakeCase("MainHand"));
            Assert.AreEqual("rare", TooltipLocalization.ToSnakeCase("Rare"));
        }

        // ---------------------------------------------------------------
        // Skills
        // ---------------------------------------------------------------

        [Test]
        public void ForSkill_Learned_ShowsLevelElementScaledDamageCostsAndBuffs()
        {
            SkillDefinition fireball = CreateSkill("skill_fireball", "Fireball", ElementType.Fire);
            SetPrivateField(fireball, "_baseDamage", 20f);
            SetPrivateField(fireball, "_damagePerLevel", 5f);
            SetPrivateField(fireball, "_manaCost", 15f);
            SetPrivateField(fireball, "_cooldownSeconds", 4f);
            SetPrivateField(fireball, "_maxLevel", 5);
            SetPrivateField(fireball, "_description", "Hurls a ball of fire.");
            SetPrivateField(fireball, "_debuffs", new[]
            {
                new StatModifierDefinition { affectedStat = StatType.MoveSpeed, application = ModifierApplication.PercentAdditive, value = -0.2f, durationSeconds = 3f, isDebuff = true },
            });

            TooltipContent content = TooltipContentBuilder.ForSkill(fireball, currentLevel: 3);

            Assert.AreEqual("Fireball", content.Title);
            Assert.AreEqual("Nível 3/5 · Fogo", content.Subtitle);
            Assert.IsNull(content.Rarity);
            CollectionAssert.Contains(content.StatLines, "Dano: 30"); // 20 + 5 * (3 - 1)
            CollectionAssert.Contains(content.StatLines, "Custo de mana: 15");
            CollectionAssert.Contains(content.StatLines, "Recarga: 4s");
            CollectionAssert.Contains(content.StatLines, "-20% Velocidade de movimento por 3s");
            Assert.AreEqual("Hurls a ball of fire.", content.Description);
        }

        [Test]
        public void ForSkill_NotLearned_ShowsLevelOneValuesAndNoElementWhenNone()
        {
            SkillDefinition dash = CreateSkill("skill_dash", "Dash", ElementType.None);
            SetPrivateField(dash, "_baseDamage", 10f);
            SetPrivateField(dash, "_damagePerLevel", 5f);
            SetPrivateField(dash, "_manaCost", 0f);

            TooltipContent content = TooltipContentBuilder.ForSkill(dash, currentLevel: 0);

            Assert.AreEqual("Não aprendida", content.Subtitle);
            CollectionAssert.Contains(content.StatLines, "Dano: 10");
            Assert.IsFalse(content.StatLines.Exists(l => l.StartsWith("Custo de mana")), "zero costs are omitted");
        }

        [Test]
        public void ToPlainText_JoinsNonEmptyPartsWithNewlines()
        {
            var content = new TooltipContent { Title = "A", Subtitle = "", Description = "D" };
            content.StatLines.Add("S1");
            content.StatLines.Add("S2");

            Assert.AreEqual("A\nS1\nS2\nD", content.ToPlainText());
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static void AssertHasKey(string key)
        {
            Assert.IsNotNull(TooltipLocalization.GetFallback(key), $"missing tooltip localization key '{key}'");
        }

        private ItemDefinition CreateItem(string id, string displayName, ItemType type, ItemRarity rarity, EquipmentSlot slot)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            SetPrivateField(item, "_itemId", id);
            SetPrivateField(item, "_displayName", displayName);
            SetPrivateField(item, "_type", type);
            SetPrivateField(item, "_rarity", rarity);
            SetPrivateField(item, "_equipSlot", slot);
            return item;
        }

        private ItemAffixDefinition CreateAffix(string id, StatType stat, ModifierApplication application)
        {
            var affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
            _created.Add(affix);
            SetPrivateField(affix, "_affixId", id);
            SetPrivateField(affix, "_affectedStat", stat);
            SetPrivateField(affix, "_application", application);
            return affix;
        }

        private SkillDefinition CreateSkill(string id, string displayName, ElementType element)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _created.Add(skill);
            SetPrivateField(skill, "_skillId", id);
            SetPrivateField(skill, "_displayName", displayName);
            SetPrivateField(skill, "_element", element);
            return skill;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
