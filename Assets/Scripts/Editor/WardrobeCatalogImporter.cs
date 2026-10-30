using System.IO;
using System.Text;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Wardrobe;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// "Rapadura ▸ Housing ▸ Import Wardrobe Catalog": parses the tables of <c>Docs/Housing/CatalogoRoupas.md</c>
    /// (234 entries: cousin sets, common clothes, accessories, dyes, weapon skins) with <see cref="WardrobeCatalogImport"/>
    /// and writes <c>Assets/Scripts/Gameplay/Wardrobe/WardrobeCatalogData.g.cs</c> (plus English names from the optional
    /// <c>Docs/Housing/CatalogoRoupas.en.tsv</c>). Idempotent like <see cref="HouseCatalogImporter"/>: only rewritten
    /// when the content changes (and <c>WardrobeTests</c> fails when the committed file is stale).
    /// </summary>
    public static class WardrobeCatalogImporter
    {
        public const string MenuPath = "Rapadura/Housing/Import Wardrobe Catalog";

        [MenuItem(MenuPath)]
        public static void ImportFromMenu()
        {
            bool changed = Import(out int count, out string message);
            Debug.Log($"[WardrobeCatalogImporter] {message}");
            if (changed)
            {
                AssetDatabase.ImportAsset(WardrobeCatalogImport.GeneratedCodePath);
            }

            EditorUtility.DisplayDialog("Import Wardrobe Catalog", message, "OK");
        }

        /// <summary>The code the importer would write now (null when the markdown is missing).</summary>
        public static string Generate(out int count)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string markdownPath = Path.Combine(root, WardrobeCatalogImport.CatalogMarkdownPath);
            count = 0;
            if (!File.Exists(markdownPath)) return null;

            var entries = WardrobeCatalogImport.Parse(File.ReadAllText(markdownPath, Encoding.UTF8));
            string englishPath = Path.Combine(root, WardrobeCatalogImport.EnglishNamesPath);
            var english = File.Exists(englishPath) ? HouseCatalogImport.ParseEnglishNames(File.ReadAllText(englishPath, Encoding.UTF8)) : null;
            count = entries.Count;
            return WardrobeCatalogImport.GenerateCode(entries, english);
        }

        /// <summary>Returns true when the generated file changed.</summary>
        public static bool Import(out int count, out string message)
        {
            string code = Generate(out count);
            if (code == null)
            {
                message = $"Missing {WardrobeCatalogImport.CatalogMarkdownPath}.";
                return false;
            }

            string root = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.Combine(root, WardrobeCatalogImport.GeneratedCodePath);
            string current = File.Exists(outputPath) ? File.ReadAllText(outputPath, Encoding.UTF8).Replace("\r\n", "\n") : null;
            if (current == code)
            {
                message = $"{count} wardrobe entries, generated table already up to date.";
                return false;
            }

            File.WriteAllText(outputPath, code, new UTF8Encoding(false));
            message = $"{count} wardrobe entries written to {WardrobeCatalogImport.GeneratedCodePath}.";
            return true;
        }
    }
}
