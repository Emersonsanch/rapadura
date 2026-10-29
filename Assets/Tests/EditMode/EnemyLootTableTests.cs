using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Crafting;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Encounter loot (<see cref="EnemyLootRoller"/> + the biome loot tables). Regression for TODO.md
    /// "Próximos passos" item 13: enemies only ever dropped <c>item_gold</c>, so no CollectItem objective on
    /// wood/fiber/iron ore could finish and crafting materials were unobtainable. The content half checks
    /// the real assets under Resources (every collect objective and recipe input can be had in the map ⇄
    /// encounter loop, within a sane number of fights); the rules half drives the roller with an injected
    /// <see cref="System.Random"/> on plain test objects.
    /// </summary>
    public class EnemyLootTableTests
    {
        /// <summary>Average enemies per random encounter (WorldMap.unity: 1-2).</summary>
        private const float EnemiesPerFight = 1.5f;

        /// <summary>Most fights a collect objective may take in its best biome (Docs/Balance/README.md §8).</summary>
        private const float MaxFightsPerCollectObjective = 12f;

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            BiomeRegistry.Invalidate();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            BiomeRegistry.Invalidate();
            ItemDatabase.Invalidate();
        }

        // ------------------------------------------------------------------
        // Content (real Resources assets)
        // ------------------------------------------------------------------

        [Test]
        public void EveryEnemyAsset_BelongsToExactlyOneBiome()
        {
            EnemyDefinition[] enemies = Resources.LoadAll<EnemyDefinition>("Enemies");
            Assert.IsNotEmpty(enemies);

            foreach (EnemyDefinition enemy in enemies)
            {
                int biomes = BiomeRegistry.GetAll().Count(b => b.ContainsEnemy(enemy));
                Assert.AreEqual(1, biomes, $"'{enemy.name}' must be in exactly one biome spawn table (its loot table comes from there).");
                Assert.IsNotNull(BiomeRegistry.FindBiomeOf(enemy), enemy.name);
            }
        }

        [Test]
        public void EveryBiome_HasALootTableAndRollsIt()
        {
            foreach (BiomeType type in (BiomeType[])Enum.GetValues(typeof(BiomeType)))
            {
                BiomeDefinition biome = BiomeRegistry.Get(type);
                Assert.IsNotNull(biome, type.ToString());
                Assert.IsTrue(biome.Loot.Any(e => e?.Item != null && e.Weight > 0), $"{type} has no loot entries.");
                Assert.Greater(biome.LootDropChance * biome.LootRollsPerKill, 0f, $"{type}: commons never roll the loot table.");
                Assert.Greater(biome.BossLootRolls, biome.LootRollsPerKill * biome.LootDropChance, $"{type}: bosses must roll more loot than commons.");

                foreach (BiomeLootEntry entry in biome.Loot)
                {
                    Assert.IsNotNull(entry.Item, $"{type} has a loot entry with no item.");
                    Assert.LessOrEqual(entry.QuantityMin, entry.QuantityMax, $"{type}/{entry.Item.ItemId}");
                }
            }
        }

        [Test]
        public void EveryCollectObjectiveItem_IsDroppableInAReachableBiome_OrCraftableFromDrops()
        {
            HashSet<string> obtainable = BuildObtainableItemIds();

            foreach ((string questId, string itemId) in AllCollectObjectives())
            {
                Assert.IsNotNull(ItemDatabase.GetById(itemId), $"{questId}: item '{itemId}' doesn't exist.");
                Assert.IsTrue(obtainable.Contains(itemId), $"{questId}: '{itemId}' can't be obtained in the map ⇄ encounter loop (no common enemy drops it and no recipe makes it from drops).");
            }
        }

        [Test]
        public void EveryRawMaterialCollectObjective_DropsDirectly_FromCommonEnemies()
        {
            HashSet<string> dropped = BuildCommonDropItemIds();
            foreach ((string questId, string itemId) in AllCollectObjectives())
            {
                ItemDefinition item = ItemDatabase.GetById(itemId);
                if (item != null && item.Type == ItemType.Material)
                {
                    Assert.IsTrue(dropped.Contains(itemId), $"{questId}: material '{itemId}' must drop from a common (non-boss) enemy.");
                }
            }
        }

        [Test]
        public void EveryRecipeInput_IsObtainable()
        {
            HashSet<string> obtainable = BuildObtainableItemIds();
            RecipeDefinition[] recipes = RecipeDatabase.GetAll().ToArray();
            Assert.IsNotEmpty(recipes);

            foreach (RecipeDefinition recipe in recipes)
            {
                foreach (RecipeIngredient ingredient in recipe.Ingredients)
                {
                    Assert.IsNotNull(ingredient.item, $"{recipe.RecipeId} has an empty ingredient.");
                    Assert.IsTrue(obtainable.Contains(ingredient.item.ItemId), $"{recipe.RecipeId}: input '{ingredient.item.ItemId}' is unobtainable.");
                }
            }
        }

        [Test]
        public void CollectObjectives_FinishWithinTheFightBudget()
        {
            foreach ((string questId, string itemId, int amount) in AllCollectObjectivesWithAmount())
            {
                if (EnemyLootRoller.IsRolledAsEquipment(ItemDatabase.GetById(itemId)))
                {
                    // Gear (Joaquim's item_iron_sword) is a rare drop on purpose; the main routes are
                    // crafting, the Caves boss bonus and a region quest reward.
                    continue;
                }

                float best = BestDropPerFight(itemId);
                if (best <= 0f)
                {
                    // Not a drop (crafted result, e.g. item_health_potion/item_iron_sword); covered by the obtainable test.
                    continue;
                }

                float fights = amount / best;
                Assert.LessOrEqual(fights, MaxFightsPerCollectObjective,
                    $"{questId}: {amount}x '{itemId}' takes ~{fights:0.0} fights in its best biome ({best:0.00}/fight).");
            }
        }

        [Test]
        public void RealForestWolf_DropsWoodFiberOrHerb_OverManyKills()
        {
            EnemyDefinition wolf = Resources.Load<EnemyDefinition>("Enemies/enemy_forest_wolf");
            Assert.IsNotNull(wolf);
            BiomeDefinition forest = BiomeRegistry.FindBiomeOf(wolf);
            Assert.AreEqual(BiomeType.Forest, forest.Type);

            var random = new System.Random(7);
            var totals = new Dictionary<string, int>();
            for (int i = 0; i < 200; i++)
            {
                foreach (LootDrop drop in EnemyLootRoller.Roll(wolf, forest, random, Array.Empty<ItemAffixDefinition>()))
                {
                    totals.TryGetValue(drop.Item.ItemId, out int count);
                    totals[drop.Item.ItemId] = count + drop.Quantity;
                }
            }

            foreach (string id in new[] { "item_gold", "item_wood", "item_fiber", "item_herb" })
            {
                Assert.IsTrue(totals.ContainsKey(id) && totals[id] > 0, $"200 forest wolves never dropped '{id}'.");
            }
        }

        [Test]
        public void ArenaLoot_IsCarriedBackToTheMap_WithAffixes()
        {
            // Arena: a Mountain boss and a few golems die; their drops land in the hero's inventory.
            EnemyDefinition golem = Resources.Load<EnemyDefinition>("Enemies/enemy_mountain_stone_golem");
            EnemyDefinition warlord = Resources.Load<EnemyDefinition>("Enemies/enemy_mountain_warlord");
            Assert.IsNotNull(golem);
            Assert.IsNotNull(warlord);
            BiomeDefinition mountain = BiomeRegistry.FindBiomeOf(golem);

            var arenaHero = new GameObject("ArenaHero");
            _created.Add(arenaHero);
            InventoryManager arenaInventory = arenaHero.AddComponent<InventoryManager>();

            var random = new System.Random(2026);
            var drops = new List<LootDrop>(EnemyLootRoller.Roll(warlord, mountain, random));
            for (int i = 0; i < 4; i++)
            {
                drops.AddRange(EnemyLootRoller.Roll(golem, mountain, random));
            }

            foreach (LootDrop drop in drops)
            {
                if (drop.IsGenerated)
                {
                    Assert.IsTrue(arenaInventory.AddGeneratedItem(drop.Generated));
                }
                else
                {
                    arenaInventory.AddItem(drop.Item, drop.Quantity);
                }
            }

            Assert.Greater(arenaInventory.GetTotalCount("item_iron_ore"), 0, "The warlord's 4 guaranteed table rolls gave no ore at all.");
            InventorySlotData bonus = arenaInventory.Slots.FirstOrDefault(slot => slot.itemId == "item_leather_chest" && slot.HasGeneratedData);
            Assert.IsNotNull(bonus, "The warlord's Epic bonus chest is missing.");

            var expected = new Dictionary<string, int>();
            foreach (InventorySlotData slot in arenaInventory.Slots.Where(slot => !slot.IsEmpty))
            {
                expected.TryGetValue(slot.itemId, out int count);
                expected[slot.itemId] = count + slot.quantity;
            }

            // Scene unload → SaveManager carries the state → the next scene's fresh inventory gets it.
            var saves = new SaveManager();
            saves.Register(arenaInventory);
            saves.Unregister(arenaInventory);
            Assert.IsTrue(saves.HasCarriedState(arenaInventory.SaveKey));

            var nextHero = new GameObject("NextSceneHero");
            _created.Add(nextHero);
            InventoryManager nextInventory = nextHero.AddComponent<InventoryManager>();
            saves.Register(nextInventory);
            Assert.AreEqual(1, saves.RestoreCarriedState());

            foreach (KeyValuePair<string, int> pair in expected)
            {
                Assert.AreEqual(pair.Value, nextInventory.GetTotalCount(pair.Key), pair.Key);
            }

            InventorySlotData carriedBonus = nextInventory.Slots.First(slot => slot.itemId == "item_leather_chest" && slot.HasGeneratedData);
            Assert.AreEqual(ItemRarity.Epic, carriedBonus.generatedRarity);
            Assert.AreEqual(bonus.generatedName, carriedBonus.generatedName);
            CollectionAssert.AreEqual(bonus.generatedAffixIds, carriedBonus.generatedAffixIds);
        }

        // ------------------------------------------------------------------
        // Rules (plain test objects, injected RNG)
        // ------------------------------------------------------------------

        [Test]
        public void Roll_IsDeterministic_ForTheSameSeed()
        {
            ItemDefinition gold = CreateItem("item_gold", maxStack: 999);
            ItemDefinition wood = CreateItem("item_wood", maxStack: 99);
            ItemDefinition sword = CreateItem("item_sword", maxStack: 1, slot: EquipmentSlot.MainHand);
            ItemAffixDefinition[] pool = { CreateAffix("affix_a", StatType.AttackDamage), CreateAffix("affix_b", StatType.MaxHealth) };
            EnemyDefinition enemy = CreateEnemy(false, gold, 0.5f, 1, 3);
            BiomeDefinition biome = CreateBiome(enemy, 0.7f, 2, 4, new BiomeLootEntry(wood, 5, 1, 3), new BiomeLootEntry(sword, 2));

            string first = Describe(Enumerable.Range(0, 40).SelectMany(_ => EnemyLootRoller.Roll(enemy, biome, Rng(99), pool)));
            string second = Describe(Enumerable.Range(0, 40).SelectMany(_ => EnemyLootRoller.Roll(enemy, biome, Rng(99), pool)));
            Assert.AreEqual(first, second);

            var a = new System.Random(1234);
            var b = new System.Random(1234);
            string sequenceA = Describe(Enumerable.Range(0, 40).SelectMany(_ => EnemyLootRoller.Roll(enemy, biome, a, pool)));
            string sequenceB = Describe(Enumerable.Range(0, 40).SelectMany(_ => EnemyLootRoller.Roll(enemy, biome, b, pool)));
            Assert.AreEqual(sequenceA, sequenceB);
            StringAssert.Contains("item_wood", sequenceA);
            StringAssert.Contains("item_sword", sequenceA);
        }

        [Test]
        public void CommonEnemy_WithZeroBiomeChance_DropsOnlyItsBaseLoot()
        {
            ItemDefinition gold = CreateItem("item_gold", maxStack: 999);
            ItemDefinition wood = CreateItem("item_wood", maxStack: 99);
            EnemyDefinition enemy = CreateEnemy(false, gold, 1f, 2, 2);
            BiomeDefinition biome = CreateBiome(enemy, 0f, 3, 4, new BiomeLootEntry(wood, 1));

            for (int seed = 0; seed < 50; seed++)
            {
                List<LootDrop> drops = EnemyLootRoller.Roll(enemy, biome, Rng(seed));
                Assert.AreEqual(1, drops.Count);
                Assert.AreSame(gold, drops[0].Item);
                Assert.AreEqual(2, drops[0].Quantity);
            }
        }

        [Test]
        public void BiomeEntryQuantity_StaysInItsRange()
        {
            ItemDefinition ore = CreateItem("item_iron_ore", maxStack: 50);
            EnemyDefinition enemy = CreateEnemy(false, null, 0f, 0, 0);
            BiomeDefinition biome = CreateBiome(enemy, 1f, 1, 4, new BiomeLootEntry(ore, 1, 2, 4));

            var seen = new HashSet<int>();
            var random = Rng(5);
            for (int i = 0; i < 300; i++)
            {
                List<LootDrop> drops = EnemyLootRoller.Roll(enemy, biome, random);
                Assert.AreEqual(1, drops.Count);
                Assert.That(drops[0].Quantity, Is.InRange(2, 4));
                seen.Add(drops[0].Quantity);
            }

            CollectionAssert.AreEquivalent(new[] { 2, 3, 4 }, seen);
        }

        [Test]
        public void Boss_RollsTheBiomeTableMoreTimes_EveryRollGuaranteed_PlusBonusDrop()
        {
            ItemDefinition gold = CreateItem("item_gold", maxStack: 999);
            ItemDefinition ore = CreateItem("item_iron_ore", maxStack: 50);
            ItemDefinition sword = CreateItem("item_sword", maxStack: 1, slot: EquipmentSlot.MainHand);
            ItemAffixDefinition[] pool = { CreateAffix("affix_a", StatType.AttackDamage), CreateAffix("affix_b", StatType.MaxHealth), CreateAffix("affix_c", StatType.Defense) };
            EnemyDefinition common = CreateEnemy(false, gold, 0.5f, 1, 1);
            EnemyDefinition boss = CreateEnemy(true, gold, 0.1f, 5, 5);
            SetPrivateField(boss, "_bossBonusDropItem", sword);
            SetPrivateField(boss, "_bossBonusDropRarity", ItemRarity.Epic);
            BiomeDefinition biome = CreateBiome(common, 0.25f, 1, 4, new BiomeLootEntry(ore, 1, 1, 1));

            Assert.Greater(EnemyLootRoller.ExpectedBiomeQuantity(boss, biome, "item_iron_ore"),
                EnemyLootRoller.ExpectedBiomeQuantity(common, biome, "item_iron_ore"));
            Assert.AreEqual(4f, EnemyLootRoller.ExpectedBiomeQuantity(boss, biome, "item_iron_ore"), 0.0001f);

            for (int seed = 0; seed < 30; seed++)
            {
                List<LootDrop> drops = EnemyLootRoller.Roll(boss, biome, Rng(seed), pool);
                Assert.AreEqual(5, drops.Where(d => d.Item == gold).Sum(d => d.Quantity), "Boss base loot is guaranteed.");
                Assert.AreEqual(4, drops.Count(d => d.Item == ore), "Every boss roll of the biome table succeeds.");
                LootDrop bonus = drops.Single(d => d.IsGenerated);
                Assert.AreSame(sword, bonus.Generated.BaseItem);
                Assert.AreEqual(ItemRarity.Epic, bonus.Generated.Rarity);
                Assert.That(bonus.Generated.Affixes.Count, Is.InRange(2, 3));
            }
        }

        [Test]
        public void EquipmentEntry_DropsAsOneGeneratedPiece_NeverBelowItsBaseRarity()
        {
            ItemDefinition bow = CreateItem("item_bow", maxStack: 1, slot: EquipmentSlot.MainHand);
            SetPrivateField(bow, "_rarity", ItemRarity.Uncommon);
            ItemAffixDefinition[] pool = { CreateAffix("affix_a", StatType.AttackDamage), CreateAffix("affix_b", StatType.MaxHealth) };
            EnemyDefinition common = CreateEnemy(false, null, 0f, 0, 0);
            EnemyDefinition boss = CreateEnemy(true, null, 0f, 0, 0);
            BiomeDefinition biome = CreateBiome(common, 1f, 1, 1, new BiomeLootEntry(bow, 1, 3, 5));

            var random = Rng(11);
            for (int i = 0; i < 100; i++)
            {
                LootDrop drop = EnemyLootRoller.Roll(common, biome, random, pool).Single();
                Assert.IsTrue(drop.IsGenerated);
                Assert.AreEqual(1, drop.Quantity, "Equipment ignores the quantity range.");
                Assert.GreaterOrEqual(drop.Generated.Rarity, ItemRarity.Uncommon);
                Assert.LessOrEqual(drop.Generated.Rarity, ItemRarity.Rare);

                LootDrop bossDrop = EnemyLootRoller.Roll(boss, biome, random, pool).Single();
                Assert.GreaterOrEqual(bossDrop.Generated.Rarity, ItemRarity.Uncommon);
            }
        }

        [Test]
        public void NullBiome_StillGrantsBaseLoot()
        {
            ItemDefinition gold = CreateItem("item_gold", maxStack: 999);
            EnemyDefinition enemy = CreateEnemy(false, gold, 1f, 1, 1);

            List<LootDrop> drops = EnemyLootRoller.Roll(enemy, null, Rng(1));

            Assert.AreEqual(1, drops.Count);
            Assert.AreSame(gold, drops[0].Item);
            Assert.IsEmpty(EnemyLootRoller.Roll(null, null, Rng(1)));
        }

        [Test]
        public void FindBiomeOf_ResolvesTheSpawnTableOwner()
        {
            EnemyDefinition wolf = CreateEnemy(false, null, 0f, 0, 0);
            EnemyDefinition golem = CreateEnemy(false, null, 0f, 0, 0);
            BiomeDefinition forest = CreateBiome(wolf, 0.5f, 1, 3);
            BiomeDefinition mountain = CreateBiome(golem, 0.5f, 1, 3);
            mountain.SetDataForTests(BiomeType.Mountain, "biome.mountain.name", "", 0f, 10f, "",
                new List<BiomeEnemySpawnEntry> { new BiomeEnemySpawnEntry(golem, 1) }, new List<BiomeLootEntry>());
            BiomeRegistry.SetForTests(new[] { forest, mountain });

            Assert.AreSame(forest, BiomeRegistry.FindBiomeOf(wolf));
            Assert.AreSame(mountain, BiomeRegistry.FindBiomeOf(golem));
            Assert.IsNull(BiomeRegistry.FindBiomeOf(CreateEnemy(false, null, 0f, 0, 0)));
            Assert.IsNull(BiomeRegistry.FindBiomeOf(null));
        }

        // ------------------------------------------------------------------
        // Helpers — content
        // ------------------------------------------------------------------

        private static IEnumerable<(string questId, string itemId)> AllCollectObjectives()
        {
            return AllCollectObjectivesWithAmount().Select(o => (o.questId, o.itemId));
        }

        private static IEnumerable<(string questId, string itemId, int amount)> AllCollectObjectivesWithAmount()
        {
            var result = new List<(string, string, int)>();
            foreach (QuestDefinition quest in Resources.LoadAll<QuestDefinition>("Quests"))
            {
                foreach (QuestObjective objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.CollectItem)
                    {
                        result.Add((quest.QuestId, objective.TargetId, objective.RequiredAmount));
                    }
                }
            }

            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Objectives.Where(o => o.Type == QuestObjectiveType.CollectItem))
                {
                    result.Add((spec.QuestId, objective.TargetId, objective.RequiredAmount));
                }
            }

            Assert.IsNotEmpty(result, "No CollectItem objectives found — did Resources/Quests move?");
            return result;
        }

        /// <summary>Items a common (non-boss, spawn weight &gt; 0) enemy of some biome can drop.</summary>
        private static HashSet<string> BuildCommonDropItemIds()
        {
            var ids = new HashSet<string>();
            foreach (BiomeDefinition biome in BiomeRegistry.GetAll())
            {
                List<EnemyDefinition> commons = biome.Enemies
                    .Where(e => e?.Enemy != null && e.Weight > 0 && !e.Enemy.IsBoss)
                    .Select(e => e.Enemy).ToList();
                if (commons.Count == 0)
                {
                    continue;
                }

                foreach (EnemyDefinition enemy in commons)
                {
                    if (enemy.LootItem != null && enemy.LootDropChance > 0f && enemy.LootQuantityMax > 0)
                    {
                        ids.Add(enemy.LootItem.ItemId);
                    }
                }

                if (biome.LootDropChance > 0f && biome.LootRollsPerKill > 0)
                {
                    foreach (BiomeLootEntry entry in biome.Loot.Where(e => e?.Item != null && e.Weight > 0))
                    {
                        ids.Add(entry.Item.ItemId);
                    }
                }
            }

            return ids;
        }

        /// <summary>Common drops, closed over the default-known recipes whose every input is obtainable.</summary>
        private static HashSet<string> BuildObtainableItemIds()
        {
            HashSet<string> ids = BuildCommonDropItemIds();
            RecipeDefinition[] recipes = RecipeDatabase.GetAll().Where(r => r != null && r.ResultItem != null && r.KnownByDefault).ToArray();

            bool grew = true;
            while (grew)
            {
                grew = false;
                foreach (RecipeDefinition recipe in recipes)
                {
                    if (!ids.Contains(recipe.ResultItem.ItemId)
                        && recipe.Ingredients.All(i => i.item != null && ids.Contains(i.item.ItemId)))
                    {
                        ids.Add(recipe.ResultItem.ItemId);
                        grew = true;
                    }
                }
            }

            return ids;
        }

        /// <summary>Expected amount of <paramref name="itemId"/> per random encounter in the best biome (commons only).</summary>
        private static float BestDropPerFight(string itemId)
        {
            float best = 0f;
            foreach (BiomeDefinition biome in BiomeRegistry.GetAll())
            {
                List<BiomeEnemySpawnEntry> commons = biome.Enemies.Where(e => e?.Enemy != null && e.Weight > 0 && !e.Enemy.IsBoss).ToList();
                int totalWeight = commons.Sum(e => e.Weight);
                if (totalWeight <= 0)
                {
                    continue;
                }

                float perKill = 0f;
                foreach (BiomeEnemySpawnEntry entry in commons)
                {
                    EnemyDefinition enemy = entry.Enemy;
                    float own = enemy.LootItem != null && enemy.LootItem.ItemId == itemId
                        ? BossBonusRules.GetEffectiveLootDropChance(enemy) * (enemy.LootQuantityMin + enemy.LootQuantityMax) * 0.5f
                        : 0f;
                    perKill += (float)entry.Weight / totalWeight * (own + EnemyLootRoller.ExpectedBiomeQuantity(enemy, biome, itemId));
                }

                best = Mathf.Max(best, perKill * EnemiesPerFight);
            }

            return best;
        }

        // ------------------------------------------------------------------
        // Helpers — plain objects
        // ------------------------------------------------------------------

        private static System.Random Rng(int seed) => new System.Random(seed);

        private static string Describe(IEnumerable<LootDrop> drops)
        {
            return string.Join("|", drops.Select(d => d.IsGenerated
                ? $"{d.Item.ItemId}:{d.Generated.Rarity}:{d.Generated.GeneratedName}:{string.Join(",", d.Generated.Affixes.Select(a => a.Affix.AffixId + "=" + a.RolledValue.ToString("0.000")))}"
                : $"{d.Item.ItemId}x{d.Quantity}"));
        }

        private EnemyDefinition CreateEnemy(bool isBoss, ItemDefinition lootItem, float dropChance, int quantityMin, int quantityMax)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(enemy);
            SetPrivateField(enemy, "_isBoss", isBoss);
            SetPrivateField(enemy, "_lootItem", lootItem);
            SetPrivateField(enemy, "_lootDropChance", dropChance);
            SetPrivateField(enemy, "_lootQuantityMin", quantityMin);
            SetPrivateField(enemy, "_lootQuantityMax", quantityMax);
            return enemy;
        }

        private BiomeDefinition CreateBiome(EnemyDefinition enemy, float chance, int rolls, int bossRolls, params BiomeLootEntry[] loot)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            _created.Add(biome);
            biome.SetDataForTests(BiomeType.Forest, "biome.forest.name", "", 10f, 20f, "",
                new List<BiomeEnemySpawnEntry> { new BiomeEnemySpawnEntry(enemy, 1) }, loot.ToList());
            biome.SetLootRollsForTests(chance, rolls, bossRolls);
            return biome;
        }

        private ItemDefinition CreateItem(string id, int maxStack, EquipmentSlot slot = EquipmentSlot.None)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(item);
            SetPrivateField(item, "_itemId", id);
            SetPrivateField(item, "_displayName", id);
            SetPrivateField(item, "_maxStack", maxStack);
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
