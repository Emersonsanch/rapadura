using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Guards hand-written .meta files. A UXML/USS meta whose <c>script:</c> points at its own guid
    /// (instead of Unity's built-in importer) imports as nothing, so <c>Resources.Load</c> returns
    /// null and a self-bootstrapped screen silently disables itself — the story panel shipped
    /// that way once, with every logic test green.
    /// </summary>
    public class AssetMetaIntegrityTests
    {
        private const string BuiltinImporterGuid = "0000000000000000e000000000000000";
        private static readonly Regex GuidLine = new Regex(@"^guid:\s*([0-9a-f]{32})\s*$", RegexOptions.Multiline);
        private static readonly Regex ScriptLine = new Regex(@"^\s*script:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32}),\s*type:\s*\d+\}\s*$", RegexOptions.Multiline);

        [Test]
        public void UiToolkitMetas_UseTheBuiltinImporter_AndAreComplete()
        {
            var broken = new List<string>();
            foreach (string pattern in new[] { "*.uxml.meta", "*.uss.meta" })
            {
                foreach (string path in Directory.GetFiles(Application.dataPath, pattern, SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(path);
                    Match script = ScriptLine.Match(text);
                    if (!script.Success || script.Groups[1].Value != BuiltinImporterGuid)
                    {
                        broken.Add(path.Substring(Application.dataPath.Length - "Assets".Length));
                    }
                }
            }

            Assert.That(broken, Is.Empty, "UXML/USS metas with a malformed or self-referencing importer script line");
        }

        [Test]
        public void Metas_HaveAUniqueGuid()
        {
            var seen = new Dictionary<string, string>();
            var duplicates = new List<string>();
            foreach (string path in Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories))
            {
                Match guid = GuidLine.Match(File.ReadAllText(path));
                if (!guid.Success)
                {
                    continue;
                }

                if (seen.TryGetValue(guid.Groups[1].Value, out string other))
                {
                    duplicates.Add($"{other} <-> {path}");
                }
                else
                {
                    seen[guid.Groups[1].Value] = path;
                }
            }

            Assert.That(duplicates, Is.Empty, "Two assets share a guid (copied .meta without a fresh guid)");
        }
    }
}
