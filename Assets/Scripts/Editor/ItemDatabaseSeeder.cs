using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates a starter set of example ItemDefinition assets
    /// under Assets/Resources/Items, covering materials, consumables, weapons, armor and
    /// tools, so the inventory system ships with real content to test against. Safe to
    /// re-run: existing assets with a matching item id are updated in place.
    /// Run via Rapadura > Seed Example Items.
    /// </summary>
    public static class ItemDatabaseSeeder
    {
        private const string ItemFolder = "Assets/Resources/Items";

        private class ItemSeed
        {
            public string id, name, description;
            public ItemType type;
            public ItemCategory category;
            public ItemRarity rarity = ItemRarity.Common;
            public float weight = 1f;
            public int value = 1;
            public int maxStack = 1;
            public bool hasDurability = false;
            public float maxDurability = 100f;
            public EquipmentSlot equipSlot = EquipmentSlot.None;
            public float armorDefense = 0f;
            public float weaponDamage = 0f;
            public float weaponAttackSpeed = 1f;
            public float healthRestore = 0f;
            public float staminaRestore = 0f;
            public float manaRestore = 0f;
        }

        [MenuItem("Rapadura/Seed Example Items")]
        public static void SeedItems()
        {
            if (!Directory.Exists(ItemFolder))
            {
                Directory.CreateDirectory(ItemFolder);
            }

            foreach (ItemSeed seed in BuildSeeds())
            {
                CreateOrUpdateItem(seed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ItemDatabase.Invalidate();
            Debug.Log("[ItemDatabaseSeeder] Example items created/updated under " + ItemFolder);
        }

        private static void CreateOrUpdateItem(ItemSeed seed)
        {
            string path = Path.Combine(ItemFolder, seed.id + ".asset");
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            var so = new SerializedObject(item);
            so.FindProperty("_itemId").stringValue = seed.id;
            so.FindProperty("_displayName").stringValue = seed.name;
            so.FindProperty("_description").stringValue = seed.description;
            so.FindProperty("_type").enumValueIndex = (int)seed.type;
            so.FindProperty("_category").enumValueIndex = (int)seed.category;
            so.FindProperty("_rarity").enumValueIndex = (int)seed.rarity;
            so.FindProperty("_weight").floatValue = seed.weight;
            so.FindProperty("_value").intValue = seed.value;
            so.FindProperty("_maxStack").intValue = seed.maxStack;
            so.FindProperty("_hasDurability").boolValue = seed.hasDurability;
            so.FindProperty("_maxDurability").floatValue = seed.maxDurability;
            so.FindProperty("_equipSlot").enumValueIndex = (int)seed.equipSlot;
            so.FindProperty("_armorDefense").floatValue = seed.armorDefense;
            so.FindProperty("_weaponDamage").floatValue = seed.weaponDamage;
            so.FindProperty("_weaponAttackSpeed").floatValue = seed.weaponAttackSpeed;
            so.FindProperty("_healthRestore").floatValue = seed.healthRestore;
            so.FindProperty("_staminaRestore").floatValue = seed.staminaRestore;
            so.FindProperty("_manaRestore").floatValue = seed.manaRestore;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static List<ItemSeed> BuildSeeds()
        {
            return new List<ItemSeed>
            {
                // Materials
                new ItemSeed { id = "item_wood", name = "Madeira", description = "Tora de madeira coletada de árvores. Usada em construção e crafting.", type = ItemType.Material, category = ItemCategory.Wood, weight = 1.5f, value = 2, maxStack = 99 },
                new ItemSeed { id = "item_stone", name = "Pedra", description = "Pedra bruta coletada de rochas. Usada em construção e crafting.", type = ItemType.Material, category = ItemCategory.Stone, weight = 2f, value = 1, maxStack = 99 },
                new ItemSeed { id = "item_iron_ore", name = "Minério de Ferro", description = "Minério bruto extraído de veios rochosos. Pode ser fundido em barras.", type = ItemType.Material, category = ItemCategory.Metal, weight = 3f, value = 5, maxStack = 50 },
                new ItemSeed { id = "item_herb", name = "Erva Medicinal", description = "Planta com propriedades curativas, usada em poções.", type = ItemType.Material, category = ItemCategory.Herb, weight = 0.2f, value = 3, maxStack = 50 },
                new ItemSeed { id = "item_fiber", name = "Fibra Vegetal", description = "Fibras retiradas de plantas, usadas em cordas e tecidos.", type = ItemType.Material, category = ItemCategory.Fiber, weight = 0.1f, value = 1, maxStack = 99 },

                // Consumables
                new ItemSeed { id = "item_health_potion", name = "Poção de Vida", description = "Restaura uma quantidade de vida ao ser consumida.", type = ItemType.Consumable, category = ItemCategory.Potion, weight = 0.5f, value = 15, maxStack = 20, healthRestore = 40f },
                new ItemSeed { id = "item_mana_potion", name = "Poção de Mana", description = "Restaura uma quantidade de mana ao ser consumida.", type = ItemType.Consumable, category = ItemCategory.Potion, weight = 0.5f, value = 15, maxStack = 20, manaRestore = 40f },
                new ItemSeed { id = "item_cooked_meat", name = "Carne Assada", description = "Alimento que restaura energia.", type = ItemType.Consumable, category = ItemCategory.Food, weight = 0.4f, value = 6, maxStack = 20, staminaRestore = 30f, healthRestore = 5f },

                // Weapons
                new ItemSeed { id = "item_wooden_sword", name = "Espada de Madeira", description = "Uma espada simples de treino, feita de madeira.", type = ItemType.Weapon, category = ItemCategory.MeleeWeapon, rarity = ItemRarity.Common, weight = 2f, value = 10, maxStack = 1, hasDurability = true, maxDurability = 60f, equipSlot = EquipmentSlot.MainHand, weaponDamage = 8f, weaponAttackSpeed = 1.2f },
                new ItemSeed { id = "item_iron_sword", name = "Espada de Ferro", description = "Espada forjada em ferro, resistente e afiada.", type = ItemType.Weapon, category = ItemCategory.MeleeWeapon, rarity = ItemRarity.Uncommon, weight = 3f, value = 45, maxStack = 1, hasDurability = true, maxDurability = 120f, equipSlot = EquipmentSlot.MainHand, weaponDamage = 18f, weaponAttackSpeed = 1f },
                new ItemSeed { id = "item_hunting_bow", name = "Arco de Caça", description = "Arco leve usado para ataques à distância.", type = ItemType.Weapon, category = ItemCategory.RangedWeapon, rarity = ItemRarity.Uncommon, weight = 1.5f, value = 35, maxStack = 1, hasDurability = true, maxDurability = 80f, equipSlot = EquipmentSlot.MainHand, weaponDamage = 14f, weaponAttackSpeed = 0.9f },

                // Armor
                new ItemSeed { id = "item_leather_helmet", name = "Capacete de Couro", description = "Proteção básica para a cabeça.", type = ItemType.Armor, category = ItemCategory.Head, rarity = ItemRarity.Common, weight = 1f, value = 12, maxStack = 1, hasDurability = true, maxDurability = 60f, equipSlot = EquipmentSlot.Head, armorDefense = 4f },
                new ItemSeed { id = "item_leather_chest", name = "Peitoral de Couro", description = "Proteção básica para o tronco.", type = ItemType.Armor, category = ItemCategory.Chest, rarity = ItemRarity.Common, weight = 2.5f, value = 20, maxStack = 1, hasDurability = true, maxDurability = 90f, equipSlot = EquipmentSlot.Chest, armorDefense = 8f },
                new ItemSeed { id = "item_leather_boots", name = "Botas de Couro", description = "Proteção básica para os pés.", type = ItemType.Armor, category = ItemCategory.Boots, rarity = ItemRarity.Common, weight = 1f, value = 10, maxStack = 1, hasDurability = true, maxDurability = 60f, equipSlot = EquipmentSlot.Boots, armorDefense = 3f },

                // Tools
                new ItemSeed { id = "item_axe", name = "Machado", description = "Ferramenta usada para cortar árvores e coletar madeira mais rápido.", type = ItemType.Tool, category = ItemCategory.GatheringTool, rarity = ItemRarity.Common, weight = 2.5f, value = 18, maxStack = 1, hasDurability = true, maxDurability = 100f, equipSlot = EquipmentSlot.Tool },
                new ItemSeed { id = "item_pickaxe", name = "Picareta", description = "Ferramenta usada para minerar pedras e minérios.", type = ItemType.Tool, category = ItemCategory.GatheringTool, rarity = ItemRarity.Common, weight = 2.5f, value = 18, maxStack = 1, hasDurability = true, maxDurability = 100f, equipSlot = EquipmentSlot.Tool },
                new ItemSeed { id = "item_hammer", name = "Martelo de Construção", description = "Ferramenta usada para construir e reparar estruturas.", type = ItemType.Tool, category = ItemCategory.BuildingTool, rarity = ItemRarity.Common, weight = 2f, value = 15, maxStack = 1, hasDurability = true, maxDurability = 100f, equipSlot = EquipmentSlot.Tool },

                // Misc
                new ItemSeed { id = "item_gold_coin", name = "Moeda de Ouro", description = "A moeda corrente do mundo.", type = ItemType.Currency, category = ItemCategory.Currency, weight = 0f, value = 1, maxStack = 9999 },
            };
        }
    }
}
