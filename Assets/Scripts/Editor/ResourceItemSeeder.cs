using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates the four base crafting resources (Madeira, Pedra,
    /// Ferro, Ouro — Fase 5 of TODO.md) as <see cref="ItemDefinition"/> assets under
    /// Assets/Resources/Items, analogous to <see cref="ItemDatabaseSeeder"/>. Madeira/Pedra
    /// already existed as example items from ItemDatabaseSeeder (item_wood/item_stone); this
    /// seeder re-declares them (safe to re-run, updates in place) alongside the two new resources
    /// (Ferro/Ouro) so every Fase 5 resource is guaranteed to exist regardless of which seeder ran
    /// last. Run via Rapadura > Seed Crafting Resources.
    /// </summary>
    public static class ResourceItemSeeder
    {
        private const string ItemFolder = "Assets/Resources/Items";

        private class ResourceSeed
        {
            public string id, name, description;
            public ItemCategory category;
            public float weight;
            public int value;
            public int maxStack = 99;
        }

        [MenuItem("Rapadura/Seed Crafting Resources")]
        public static void SeedResources()
        {
            if (!Directory.Exists(ItemFolder))
            {
                Directory.CreateDirectory(ItemFolder);
            }

            foreach (ResourceSeed seed in BuildSeeds())
            {
                CreateOrUpdateItem(seed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ItemDatabase.Invalidate();
            Debug.Log("[ResourceItemSeeder] Crafting resource items created/updated under " + ItemFolder);
        }

        private static void CreateOrUpdateItem(ResourceSeed seed)
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
            so.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            so.FindProperty("_category").enumValueIndex = (int)seed.category;
            so.FindProperty("_rarity").enumValueIndex = (int)ItemRarity.Common;
            so.FindProperty("_weight").floatValue = seed.weight;
            so.FindProperty("_value").intValue = seed.value;
            so.FindProperty("_maxStack").intValue = seed.maxStack;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static List<ResourceSeed> BuildSeeds()
        {
            return new List<ResourceSeed>
            {
                new ResourceSeed { id = "item_wood", name = "Madeira", description = "Tora de madeira coletada de árvores. Usada em construção e crafting.", category = ItemCategory.Wood, weight = 1.5f, value = 2, maxStack = 99 },
                new ResourceSeed { id = "item_stone", name = "Pedra", description = "Pedra bruta coletada de rochas. Usada em construção e crafting.", category = ItemCategory.Stone, weight = 2f, value = 1, maxStack = 99 },
                new ResourceSeed { id = "item_iron", name = "Ferro", description = "Barra de ferro forjada a partir de minério. Usada em construção e crafting avançado.", category = ItemCategory.Metal, weight = 3f, value = 8, maxStack = 99 },
                new ResourceSeed { id = "item_gold", name = "Ouro", description = "Pepita de ouro bruto. Usada em crafting de itens raros/lendários.", category = ItemCategory.Gold, weight = 2f, value = 20, maxStack = 99 },
            };
        }
    }
}
