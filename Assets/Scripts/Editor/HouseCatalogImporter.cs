using System.IO;
using System.Text;
using Rapadura.Gameplay.Housing;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// "Rapadura ▸ Housing ▸ Import House Catalog": parses the item tables of <c>Docs/Housing/CatalogoCasa.md</c>
    /// (580 items) with <see cref="HouseCatalogImport"/> and writes the generated table
    /// <c>Assets/Scripts/Gameplay/Housing/Catalog/HouseCatalogData.g.cs</c> (plus English names from the optional
    /// <c>Docs/Housing/CatalogoCasa.en.tsv</c>). Idempotent: the file is only rewritten when its content changes,
    /// so running it twice is a no-op (and <c>HouseCatalogTests</c> fails when the committed file is stale).
    /// </summary>
    public static class HouseCatalogImporter
    {
        public const string MenuPath = "Rapadura/Housing/Import House Catalog";

        [MenuItem(MenuPath)]
        public static void ImportFromMenu()
        {
            bool changed = Import(out int count, out string message);
            Debug.Log($"[HouseCatalogImporter] {message}");
            if (changed)
            {
                AssetDatabase.ImportAsset(HouseCatalogImport.GeneratedCodePath);
            }

            EditorUtility.DisplayDialog("Import House Catalog", message, "OK");
        }

        /// <summary>Returns true when the generated file changed.</summary>
        public static bool Import(out int count, out string message)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string markdownPath = Path.Combine(root, HouseCatalogImport.CatalogMarkdownPath);
            if (!File.Exists(markdownPath))
            {
                count = 0;
                message = $"Missing {HouseCatalogImport.CatalogMarkdownPath}.";
                return false;
            }

            var items = HouseCatalogImport.Parse(File.ReadAllText(markdownPath, Encoding.UTF8));
            string englishPath = Path.Combine(root, HouseCatalogImport.EnglishNamesPath);
            var english = File.Exists(englishPath) ? HouseCatalogImport.ParseEnglishNames(File.ReadAllText(englishPath, Encoding.UTF8)) : null;
            string code = HouseCatalogImport.GenerateCode(items, english);
            count = items.Count;

            string outputPath = Path.Combine(root, HouseCatalogImport.GeneratedCodePath);
            string current = File.Exists(outputPath) ? File.ReadAllText(outputPath, Encoding.UTF8).Replace("\r\n", "\n") : null;
            if (current == code)
            {
                message = $"{count} house items, generated table already up to date.";
                return false;
            }

            File.WriteAllText(outputPath, code, new UTF8Encoding(false));
            message = $"{count} house items written to {HouseCatalogImport.GeneratedCodePath}.";
            return true;
        }
    }
}
