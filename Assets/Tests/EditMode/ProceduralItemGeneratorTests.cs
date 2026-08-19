using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 3 "Sistema de geração procedural de itens (afixos)":
    /// <see cref="ProceduralItemGenerator"/> + <see cref="ItemAffixDefinition"/>. ScriptableObjects
    /// are built with private [SerializeField] fields populated via reflection, the same pattern
    /// used by <c>CraftingManagerTests</c>/<c>WeaponSystemTests</c>.
    /// </summary>
    public class ProceduralItemGeneratorTests
    {
        private ItemDefinition _sword;
        private ItemAffixDefinition _fireDamagePrefix;
        private ItemAffixDefinition _furySuffix;
        private ItemAffixDefinition _wrongSlotAffix;

        [SetUp]
        public void SetUp()
        {
            _sword = CreateItem("item_sword", "Espada", EquipmentSlot.MainHand);

            _fireDamagePrefix = CreateAffix(
                "affix_fire_damage",
                fallbackText: "Flamejante",
                slotKind: AffixSlotKind.Prefix,
                stat: StatType.AttackDamage,
                min: 5f,
                max: 10f,
                weight: 100,
                requiredSlot: EquipmentSlot.None);

            _furySuffix = CreateAffix(
                "affix_fury",
                fallbackText: "da Fúria",
                slotKind: AffixSlotKind.Suffix,
                stat: StatType.CriticalChance,
                min: 1f,
                max: 3f,
                weight: 50,
                requiredSlot: EquipmentSlot.MainHand);

            _wrongSlotAffix = CreateAffix(
                "affix_head_only",
                fallbackText: "do Guardião",
                slotKind: AffixSlotKind.Suffix,
                stat: StatType.Defense,
                min: 1f,
                max: 2f,
                weight: 100,
                requiredSlot: EquipmentSlot.Head);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sword);
            Object.DestroyImmediate(_fireDamagePrefix);
            Object.DestroyImmediate(_furySuffix);
            Object.DestroyImmediate(_wrongSlotAffix);
        }

        [Test]
        public void Generate_CommonRarity_RollsNoAffixes()
        {
            GeneratedItemInstance result = ProceduralItemGenerator.Generate(
                _sword, ItemRarity.Common, seed: 1, affixPool: new[] { _fireDamagePrefix, _furySuffix });

            Assert.AreEqual(0, result.Affixes.Count);
            Assert.AreEqual(_sword.DisplayName, result.GeneratedName);
        }

        [TestCase(ItemRarity.Rare, 1, 2)]
        [TestCase(ItemRarity.Epic, 2, 3)]
        [TestCase(ItemRarity.Legendary, 3, 4)]
        public void Generate_ForRarity_RollsAffixCountWithinExpectedRange(ItemRarity rarity, int expectedMin, int expectedMax)
        {
            // Use a bigger pool so higher rarities aren't capped by pool size.
            ItemAffixDefinition[] bigPool =
            {
                _fireDamagePrefix,
                _furySuffix,
                CreateAffix("affix_c", "C", AffixSlotKind.Prefix, StatType.MoveSpeed, 1, 2, 100, EquipmentSlot.None),
                CreateAffix("affix_d", "D", AffixSlotKind.Suffix, StatType.MaxHealth, 1, 2, 100, EquipmentSlot.None),
                CreateAffix("affix_e", "E", AffixSlotKind.Prefix, StatType.MaxMana, 1, 2, 100, EquipmentSlot.None),
            };

            for (int seed = 0; seed < 50; seed++)
            {
                GeneratedItemInstance result = ProceduralItemGenerator.Generate(_sword, rarity, seed: seed, affixPool: bigPool);
                Assert.GreaterOrEqual(result.Affixes.Count, expectedMin, $"seed={seed}");
                Assert.LessOrEqual(result.Affixes.Count, expectedMax, $"seed={seed}");
            }

            foreach (ItemAffixDefinition affix in bigPool)
            {
                if (affix != _fireDamagePrefix && affix != _furySuffix)
                {
                    Object.DestroyImmediate(affix);
                }
            }
        }

        [Test]
        public void Generate_WithFixedSeed_IsDeterministic()
        {
            ItemAffixDefinition[] pool = { _fireDamagePrefix, _furySuffix };

            GeneratedItemInstance first = ProceduralItemGenerator.Generate(_sword, ItemRarity.Epic, seed: 42, affixPool: pool);
            GeneratedItemInstance second = ProceduralItemGenerator.Generate(_sword, ItemRarity.Epic, seed: 42, affixPool: pool);

            Assert.AreEqual(first.Affixes.Count, second.Affixes.Count);
            CollectionAssert.AreEqual(first.AffixIds, second.AffixIds);
            CollectionAssert.AreEqual(first.AffixValues, second.AffixValues);
            Assert.AreEqual(first.GeneratedName, second.GeneratedName);
        }

        [Test]
        public void Generate_DifferentSeeds_CanProduceDifferentResults()
        {
            ItemAffixDefinition[] pool =
            {
                _fireDamagePrefix,
                _furySuffix,
                CreateAffix("affix_c", "C", AffixSlotKind.Prefix, StatType.MoveSpeed, 1, 2, 100, EquipmentSlot.None),
            };

            bool foundDifference = false;
            GeneratedItemInstance baseline = ProceduralItemGenerator.Generate(_sword, ItemRarity.Legendary, seed: 0, affixPool: pool);

            for (int seed = 1; seed < 30; seed++)
            {
                GeneratedItemInstance other = ProceduralItemGenerator.Generate(_sword, ItemRarity.Legendary, seed: seed, affixPool: pool);
                if (!baseline.AffixIds.SequenceEqual(other.AffixIds))
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.IsTrue(foundDifference, "Expected varying seeds to eventually produce a different affix roll.");

            Object.DestroyImmediate(pool[2]);
        }

        [Test]
        public void Generate_RolledAffixValues_AreWithinDefinedRange()
        {
            ItemAffixDefinition[] pool = { _fireDamagePrefix, _furySuffix };

            for (int seed = 0; seed < 30; seed++)
            {
                GeneratedItemInstance result = ProceduralItemGenerator.Generate(_sword, ItemRarity.Epic, seed: seed, affixPool: pool);

                foreach (RolledAffix rolled in result.Affixes)
                {
                    Assert.GreaterOrEqual(rolled.RolledValue, rolled.Affix.MinValue, $"seed={seed}, affix={rolled.Affix.AffixId}");
                    Assert.LessOrEqual(rolled.RolledValue, rolled.Affix.MaxValue, $"seed={seed}, affix={rolled.Affix.AffixId}");
                }
            }
        }

        [Test]
        public void Generate_ExcludesAffixesIncompatibleWithEquipSlot()
        {
            ItemAffixDefinition[] pool = { _fireDamagePrefix, _furySuffix, _wrongSlotAffix };

            for (int seed = 0; seed < 30; seed++)
            {
                GeneratedItemInstance result = ProceduralItemGenerator.Generate(_sword, ItemRarity.Legendary, seed: seed, affixPool: pool);
                Assert.IsFalse(result.AffixIds.Contains(_wrongSlotAffix.AffixId), $"seed={seed}");
            }
        }

        [Test]
        public void Generate_BuildsNameFromPrefixAndSuffixAroundBaseName()
        {
            // Force a deterministic 2-affix roll: one prefix + one suffix from a 2-item pool.
            ItemAffixDefinition[] pool = { _fireDamagePrefix, _furySuffix };

            GeneratedItemInstance result = ProceduralItemGenerator.Generate(_sword, ItemRarity.Epic, seed: 7, affixPool: pool);

            if (result.Affixes.Count == 2)
            {
                Assert.AreEqual("Flamejante Espada da Fúria", result.GeneratedName);
            }
            else
            {
                // With only 2 affixes in the pool but Epic potentially wanting up to 3, a 2-affix
                // pool always yields exactly 2 (capped by pool size) — assert that invariant holds.
                Assert.AreEqual(2, result.Affixes.Count);
            }

            Assert.IsTrue(result.GeneratedName.Contains(_sword.DisplayName));
        }

        [Test]
        public void Generate_UsesLocalizerWhenProvided()
        {
            ItemAffixDefinition[] pool = { _fireDamagePrefix };

            GeneratedItemInstance result = ProceduralItemGenerator.Generate(
                _sword,
                ItemRarity.Rare,
                seed: 3,
                localizeName: key => key == _fireDamagePrefix.NameLocalizationKey ? "Incandescente" : null,
                affixPool: pool);

            if (result.Affixes.Count > 0)
            {
                Assert.AreEqual("Incandescente Espada", result.GeneratedName);
            }
        }

        private static ItemDefinition CreateItem(string itemId, string displayName, EquipmentSlot equipSlot)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(item, "_itemId", itemId);
            SetPrivateField(item, "_displayName", displayName);
            SetPrivateField(item, "_equipSlot", equipSlot);
            SetPrivateField(item, "_type", ItemType.Weapon);
            return item;
        }

        private static ItemAffixDefinition CreateAffix(
            string affixId,
            string fallbackText,
            AffixSlotKind slotKind,
            StatType stat,
            float min,
            float max,
            int weight,
            EquipmentSlot requiredSlot)
        {
            var affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
            SetPrivateField(affix, "_affixId", affixId);
            SetPrivateField(affix, "_nameLocalizationKey", $"affix.{affixId}.name");
            SetPrivateField(affix, "_fallbackDisplayText", fallbackText);
            SetPrivateField(affix, "_slotKind", slotKind);
            SetPrivateField(affix, "_affectedStat", stat);
            SetPrivateField(affix, "_minValue", min);
            SetPrivateField(affix, "_maxValue", max);
            SetPrivateField(affix, "_rarityWeight", weight);
            SetPrivateField(affix, "_requiredEquipSlot", requiredSlot);
            return affix;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
