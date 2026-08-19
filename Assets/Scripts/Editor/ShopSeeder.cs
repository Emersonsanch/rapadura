using System.IO;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates a single example <see cref="ShopDefinition"/> asset
    /// (a general goods merchant) under Assets/Resources/Shops, analogous to
    /// <see cref="DialogueSeeder"/>/<see cref="ResourceItemSeeder"/>: safe to re-run, the existing
    /// asset is updated in place instead of duplicated. Run via Rapadura > Seed Example Shop.
    ///
    /// Sells item_wood/item_stone/item_iron (seeded by <see cref="ResourceItemSeeder"/>) for gold,
    /// priced off each item's own <see cref="ItemDefinition.Value"/>, at 50% player sell-back rate.
    /// Requires ResourceItemSeeder to have run first (Rapadura > Seed Crafting Resources) so
    /// item_wood/item_stone/item_iron/item_gold exist; logs a warning per missing item otherwise.
    ///
    /// Localization keys referenced here (shop.*) are NOT added to LocalizationManager's
    /// DefaultEntries by this seeder — LocalizationManager.cs is out of scope to edit for this
    /// task. Missing keys just render as "[key]" in-game (LocalizationManager's documented
    /// fallback behaviour) until real translations are authored.
    /// </summary>
    public static class ShopSeeder
    {
        private const string ShopFolder = "Assets/Resources/Shops";

        [MenuItem("Rapadura/Seed Example Shop")]
        public static void SeedShop()
        {
            if (!Directory.Exists(ShopFolder))
            {
                Directory.CreateDirectory(ShopFolder);
            }

            CreateOrUpdateGeneralStore();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ShopSeeder] Example shop created/updated under " + ShopFolder);
        }

        private static void CreateOrUpdateGeneralStore()
        {
            const string shopId = "shop_general_store";
            string path = Path.Combine(ShopFolder, shopId + ".asset");
            var shop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(path);

            if (shop == null)
            {
                shop = ScriptableObject.CreateInstance<ShopDefinition>();
                AssetDatabase.CreateAsset(shop, path);
            }

            var so = new SerializedObject(shop);
            so.FindProperty("_shopId").stringValue = shopId;
            so.FindProperty("_displayNameKey").stringValue = "shop.general_store.name";
            so.FindProperty("_currencyItemId").stringValue = "item_gold";
            so.FindProperty("_sellBackRate").floatValue = 0.5f;

            string[] itemIds = { "item_wood", "item_stone", "item_iron" };
            int[] stocks = { 40, 40, 15 };

            SerializedProperty catalog = so.FindProperty("_catalog");
            catalog.arraySize = itemIds.Length;

            for (int i = 0; i < itemIds.Length; i++)
            {
                ItemDefinition item = ItemDatabase.GetById(itemIds[i]);

                if (item == null)
                {
                    Debug.LogWarning($"[ShopSeeder] Item '{itemIds[i]}' not found — run 'Rapadura > Seed Crafting Resources' first.");
                }

                SerializedProperty entry = catalog.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_item").objectReferenceValue = item;
                entry.FindPropertyRelative("_priceOverride").intValue = 0; // use ItemDefinition.Value
                entry.FindPropertyRelative("_stock").intValue = stocks[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(shop);
        }
    }
}
