using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Options;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Loot of the ossuary's containers (sarcophagi, the legendary chest), built on the same pieces as an enemy's death
    /// (<see cref="EnemyLootRoller"/>): the catacombs' table (a BiomeDefinition's loot entries), equipment as one procedural
    /// piece with rarity, affixes and MU options, and the bag's shared fit rules (<see cref="LootGrantRules"/>) with the usual
    /// floating "+ item" texts. Rolling is pure (a <see cref="System.Random"/> passed in).
    /// </summary>
    public static class DungeonLoot
    {
        private static BiomeDefinition _table;

        /// <summary>The catacombs' table (Resources, cached); null when the asset is missing.</summary>
        public static BiomeDefinition CatacombsTable
        {
            get
            {
                if (_table == null)
                {
                    _table = Resources.Load<BiomeDefinition>(DungeonRules.LootTableResourcePath);
                }

                return _table;
            }
        }

        /// <summary><paramref name="rolls"/> sure rolls of <paramref name="table"/>; <paramref name="rich"/> = boss rarity weights for gear.</summary>
        public static List<LootDrop> RollTable(BiomeDefinition table, int rolls, bool rich, System.Random random)
        {
            var drops = new List<LootDrop>();
            if (table == null || random == null)
            {
                return drops;
            }

            for (int i = 0; i < rolls; i++)
            {
                BiomeLootEntry entry = table.RollLootEntry(random);
                if (entry?.Item == null)
                {
                    continue;
                }

                drops.Add(MakeDrop(entry.Item, random.Next(entry.QuantityMin, entry.QuantityMax + 1), rich, random));
            }

            return drops;
        }

        /// <summary>One drop of <paramref name="item"/>: a procedural piece for equipment (at least <paramref name="minRarity"/>), else a stack.</summary>
        public static LootDrop MakeDrop(ItemDefinition item, int quantity, bool rich, System.Random random, ItemRarity minRarity = ItemRarity.Common)
        {
            if (!EnemyLootRoller.IsRolledAsEquipment(item))
            {
                return new LootDrop(item, Mathf.Max(1, quantity));
            }

            ItemRarity rarity = EnemyLootRoller.RollEquipmentRarity(item, rich, random);
            if (rarity < minRarity)
            {
                rarity = minRarity;
            }

            int seed = random.Next();
            GeneratedItemInstance piece = ProceduralItemGenerator.Generate(item, rarity, seed, null, null);
            piece.Options = ItemOptionRules.Roll(item, rich, ItemOptionRules.OptionSeed(seed));
            return new LootDrop(piece);
        }

        /// <summary>Puts the drops in the hero's bag (shared fit rules), floats a text per drop over <paramref name="source"/>. Returns how many landed.</summary>
        public static int GrantToHero(InventoryManager inventory, IReadOnlyList<LootDrop> drops, GameObject source)
        {
            if (inventory == null || drops == null || drops.Count == 0)
            {
                return 0;
            }

            Vector3 label = (source != null ? source.transform.position : inventory.transform.position) + Vector3.up * 1.8f;
            var overflow = new List<LootGrantResult>();
            int line = 0;
            int granted = 0;
            foreach (LootDrop drop in drops)
            {
                LootGrantResult result = LootGrantRules.Grant(inventory, drop, null);
                if (result.Granted > 0)
                {
                    granted++;
                    string name = drop.IsGenerated ? drop.Generated.GeneratedName : drop.Item.LocalizedName();
                    if (Application.isPlaying)
                    {
                        CombatFxManager.SpawnLootText(label + Vector3.up * (0.35f * line++), result.Granted > 1 ? $"+{result.Granted} {name}" : "+ " + name);
                    }
                }

                if (result.Lost > 0 || result.KeptOverWeight > 0)
                {
                    overflow.Add(result);
                }
            }

            if (source != null)
            {
                EventBus.Publish(new EnemyLootDroppedEvent(source, inventory.gameObject, new List<LootDrop>(drops)));
            }

            if (overflow.Count > 0)
            {
                EventBus.Publish(new LootOverflowEvent(label + Vector3.up * (0.35f * line), overflow));
            }

            return granted;
        }
    }
}
