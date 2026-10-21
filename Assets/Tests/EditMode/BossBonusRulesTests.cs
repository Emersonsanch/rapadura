using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="BossBonusRules"/> — the extra rewards a boss-flagged
    /// <see cref="EnemyDefinition"/> grants (guaranteed loot, rare affixed bonus drop, XP
    /// multiplier), and that regular enemies keep the original behavior. ScriptableObjects are
    /// populated via reflection like <c>ProceduralItemGeneratorTests</c>.
    /// </summary>
    public class BossBonusRulesTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ItemDefinition _lootItem;
        private ItemDefinition _bonusSword;
        private ItemAffixDefinition[] _affixPool;

        [SetUp]
        public void SetUp()
        {
            _lootItem = CreateItem("item_gold", EquipmentSlot.None);
            _bonusSword = CreateItem("item_iron_sword", EquipmentSlot.MainHand);
            _affixPool = new[]
            {
                CreateAffix("affix_a", StatType.AttackDamage),
                CreateAffix("affix_b", StatType.Defense),
                CreateAffix("affix_c", StatType.MaxHealth),
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        [Test]
        public void RegularEnemy_UsesConfiguredDropChance()
        {
            EnemyDefinition wolf = CreateEnemy(isBoss: false, dropChance: 0.3f);

            Assert.AreEqual(0.3f, BossBonusRules.GetEffectiveLootDropChance(wolf), 0.0001f);
            Assert.IsTrue(BossBonusRules.ShouldDropLoot(wolf, 0.2f));
            Assert.IsFalse(BossBonusRules.ShouldDropLoot(wolf, 0.5f));
        }

        [Test]
        public void Boss_WithGuaranteedLoot_AlwaysDrops_EvenOnWorstRoll()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 0.1f);

            Assert.AreEqual(1f, BossBonusRules.GetEffectiveLootDropChance(boss), 0.0001f);
            Assert.IsTrue(BossBonusRules.ShouldDropLoot(boss, 0.99f));
            Assert.IsTrue(BossBonusRules.ShouldDropLoot(boss, 1f));
        }

        [Test]
        public void Boss_WithGuaranteedLootDisabled_FallsBackToDropChance()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 0.1f);
            SetPrivateField(boss, "_bossGuaranteedLoot", false);

            Assert.IsFalse(BossBonusRules.ShouldDropLoot(boss, 0.5f));
        }

        [Test]
        public void NoLootItem_NeverDrops()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 1f);
            SetPrivateField(boss, "_lootItem", null);

            Assert.IsFalse(BossBonusRules.ShouldDropLoot(boss, 0f));
        }

        [Test]
        public void ExperienceMultiplier_AppliesOnlyToBosses_AndNeverReducesReward()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 1f, experience: 100);
            EnemyDefinition regular = CreateEnemy(isBoss: false, dropChance: 1f, experience: 100);
            SetPrivateField(boss, "_bossExperienceMultiplier", 2.5f);
            SetPrivateField(regular, "_bossExperienceMultiplier", 2.5f);

            Assert.AreEqual(250, BossBonusRules.GetExperienceReward(boss));
            Assert.AreEqual(100, BossBonusRules.GetExperienceReward(regular));

            SetPrivateField(boss, "_bossExperienceMultiplier", 0.5f);
            Assert.AreEqual(100, BossBonusRules.GetExperienceReward(boss));
        }

        [Test]
        public void DefaultBoss_HasNoExtraXp()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 1f, experience: 80);

            Assert.AreEqual(80, BossBonusRules.GetExperienceReward(boss));
        }

        [Test]
        public void BonusDrop_OnlyForBossesWithABonusItem()
        {
            EnemyDefinition bossWithout = CreateEnemy(isBoss: true, dropChance: 1f);
            EnemyDefinition regularWith = CreateEnemy(isBoss: false, dropChance: 1f);
            SetPrivateField(regularWith, "_bossBonusDropItem", _bonusSword);

            Assert.IsFalse(BossBonusRules.HasBonusDrop(bossWithout));
            Assert.IsNull(BossBonusRules.RollBonusDrop(bossWithout, seed: 1, affixPool: _affixPool));
            Assert.IsFalse(BossBonusRules.HasBonusDrop(regularWith));
            Assert.IsNull(BossBonusRules.RollBonusDrop(regularWith, seed: 1, affixPool: _affixPool));
        }

        [Test]
        public void BonusDrop_RollsConfiguredRarityWithAffixes()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 1f);
            SetPrivateField(boss, "_bossBonusDropItem", _bonusSword);

            for (int seed = 0; seed < 20; seed++)
            {
                GeneratedItemInstance drop = BossBonusRules.RollBonusDrop(boss, seed: seed, affixPool: _affixPool);

                Assert.IsNotNull(drop);
                Assert.AreSame(_bonusSword, drop.BaseItem);
                Assert.AreEqual(ItemRarity.Rare, drop.Rarity, "default bonus rarity is Rare");
                (int min, int max) range = ProceduralItemGenerator.AffixCountRange[ItemRarity.Rare];
                Assert.That(drop.Affixes.Count, Is.InRange(range.min, range.max));
            }
        }

        [Test]
        public void BonusDrop_RespectsCustomRarity()
        {
            EnemyDefinition boss = CreateEnemy(isBoss: true, dropChance: 1f);
            SetPrivateField(boss, "_bossBonusDropItem", _bonusSword);
            SetPrivateField(boss, "_bossBonusDropRarity", ItemRarity.Legendary);

            GeneratedItemInstance drop = BossBonusRules.RollBonusDrop(boss, seed: 7, affixPool: _affixPool);

            Assert.AreEqual(ItemRarity.Legendary, drop.Rarity);
            Assert.That(drop.Affixes.Count, Is.EqualTo(3), "Legendary rolls 3-4 affixes; the 3-affix pool caps it at 3");
        }

        [Test]
        public void NullDefinition_IsSafe()
        {
            Assert.AreEqual(0f, BossBonusRules.GetEffectiveLootDropChance(null));
            Assert.IsFalse(BossBonusRules.ShouldDropLoot(null, 0f));
            Assert.AreEqual(0, BossBonusRules.GetExperienceReward(null));
            Assert.IsNull(BossBonusRules.RollBonusDrop(null));
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private EnemyDefinition CreateEnemy(bool isBoss, float dropChance, int experience = 10)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(enemy);
            SetPrivateField(enemy, "_isBoss", isBoss);
            SetPrivateField(enemy, "_lootItem", _lootItem);
            SetPrivateField(enemy, "_lootDropChance", dropChance);
            SetPrivateField(enemy, "_experienceReward", experience);
            return enemy;
        }

        private ItemDefinition CreateItem(string id, EquipmentSlot slot)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            SetPrivateField(item, "_itemId", id);
            SetPrivateField(item, "_displayName", id);
            SetPrivateField(item, "_equipSlot", slot);
            return item;
        }

        private ItemAffixDefinition CreateAffix(string id, StatType stat)
        {
            var affix = ScriptableObject.CreateInstance<ItemAffixDefinition>();
            _created.Add(affix);
            SetPrivateField(affix, "_affixId", id);
            SetPrivateField(affix, "_fallbackDisplayText", id);
            SetPrivateField(affix, "_affectedStat", stat);
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
