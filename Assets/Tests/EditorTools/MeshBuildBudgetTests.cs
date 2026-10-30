using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rapadura.Editor;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests.EditorTools
{
    /// <summary>
    /// Build-size guard for meshes. Everything under a <c>Resources</c> folder ships whether or not it
    /// is used, so each mesh asset there must fit <see cref="DefaultBudgetBytes"/> unless allow-listed
    /// below with a reason and its own cap; and every mesh that ships (Resources or build scenes) must
    /// be compressed and not Read/Write. Regressions this catches: the 29 MB uncompressed, readable
    /// <c>Joaquim_NoPedestal.asset</c> of 2026-10, or a new FBX imported with default settings.
    /// Fix: menu <c>Rapadura ▸ Build ▸ Apply Mesh Import Settings</c>.
    /// </summary>
    public class MeshBuildBudgetTests
    {
        private const long MiB = 1024L * 1024L;

        /// <summary>Per mesh asset in Resources, estimated build size.</summary>
        public const long DefaultBudgetBytes = 2 * MiB;

        /// <summary>Resources meshes allowed over the default budget: path → (cap, reason).</summary>
        public static readonly Dictionary<string, (long cap, string reason)> AllowList = new Dictionary<string, (long, string)>
        {
            ["Assets/Resources/CleanedMeshes/Joaquim_NoPedestal.asset"] = (12 * MiB,
                "Joaquim's only mesh in the build (rigged FBX mesh deduped by HeroMeshDeduplicator): raw Meshy ~300k vertices, Medium compression. Shrinks with decimation (TODO 'Próximos passos' 1)."),
            ["Assets/Resources/CleanedMeshes/Icaro_NoPedestal.asset"] = (12 * MiB,
                "Ícaro's only mesh in the build (rigged FBX mesh deduped by HeroMeshDeduplicator): raw Meshy ~340k vertices, Medium compression. Shrinks with decimation (TODO 'Próximos passos' 1)."),
        };

        [Test]
        public void ResourcesMeshAssets_StayWithinSizeBudget()
        {
            var failures = new List<string>();
            foreach (string path in ResourcesMeshPaths())
            {
                long estimate = EstimatedBuildBytes(path);
                long cap = AllowList.TryGetValue(path, out (long cap, string reason) allowed) ? allowed.cap : DefaultBudgetBytes;
                if (estimate > cap)
                {
                    failures.Add($"{path}: ~{estimate / (double)MiB:0.0} MB > {cap / (double)MiB:0.0} MB");
                }
            }

            Assert.IsEmpty(failures, "Resources mesh over budget (run Rapadura ▸ Build ▸ Apply Mesh Import Settings, decimate, or allow-list with a reason):\n" + string.Join("\n", failures));
        }

        [Test]
        public void ResourcesMeshAssets_AreCompressedAndNotReadable()
        {
            var failures = new List<string>();
            foreach (string path in ResourcesMeshPaths())
            {
                if (!(AssetDatabase.LoadMainAssetAtPath(path) is Mesh mesh))
                {
                    continue; // model files in Resources are covered by the shipped-models test
                }

                if (MeshUtility.GetMeshCompression(mesh) == ModelImporterMeshCompression.Off) failures.Add(path + ": uncompressed");
                if (mesh.isReadable) failures.Add(path + ": Read/Write enabled");
            }

            Assert.IsEmpty(failures, "Run Rapadura ▸ Build ▸ Apply Mesh Import Settings:\n" + string.Join("\n", failures));
        }

        [Test]
        public void ShippedModels_AreCompressedAndNotReadable()
        {
            HashSet<string> colliderGuids = MeshImportSettingsApplier.CollectMeshColliderModelGuids();
            var failures = new List<string>();
            foreach (string path in ShippedModelPaths())
            {
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                bool mayBeReadable = MeshImportSettingsApplier.ReadableAllowList.ContainsKey(path) || colliderGuids.Contains(AssetDatabase.AssetPathToGUID(path));
                if (importer.meshCompression == ModelImporterMeshCompression.Off) failures.Add(path + ": mesh compression Off");
                if (importer.isReadable && !mayBeReadable) failures.Add(path + ": Read/Write enabled");
            }

            Assert.IsEmpty(failures, "Run Rapadura ▸ Build ▸ Apply Mesh Import Settings:\n" + string.Join("\n", failures));
        }

        [Test]
        public void AllowListEntries_ExistAndHaveReasons()
        {
            foreach (KeyValuePair<string, (long cap, string reason)> kv in AllowList)
            {
                Assert.IsTrue(File.Exists(kv.Key), "stale allow-list entry: " + kv.Key);
                Assert.That(kv.Value.reason, Is.Not.Null.And.Length.GreaterThan(20), "allow-list entry needs a real reason: " + kv.Key);
                Assert.Greater(kv.Value.cap, DefaultBudgetBytes, "allow-list cap should exceed the default budget: " + kv.Key);
            }
        }

        /// <summary>
        /// Text-serialized assets store mesh buffers as hex, two characters per byte, which dominates the
        /// file; so half the file size tracks the build size (Joaquim_NoPedestal: 61 MB file, 29 MB in the
        /// 2026-10 build report; Icaro_NoPedestal: 19.7 MB file, 9.4 MB in build). Binary assets: file size.
        /// </summary>
        public static long EstimatedBuildBytes(string path)
        {
            long length = new FileInfo(path).Length;
            using (var reader = new StreamReader(path))
            {
                var head = new char[5];
                int read = reader.Read(head, 0, head.Length);
                return read == 5 && new string(head) == "%YAML" ? length / 2 : length;
            }
        }

        public static List<string> ResourcesMeshPaths()
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Resources/") && !paths.Contains(path)) paths.Add(path);
            }

            return paths;
        }

        /// <summary>Model files reachable from the enabled build scenes or from anything under a Resources folder.</summary>
        public static List<string> ShippedModelPaths()
        {
            var roots = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) roots.Add(scene.path);
            }

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Resources/") && !AssetDatabase.IsValidFolder(path)) roots.Add(path);
            }

            var models = new List<string>();
            foreach (string dependency in AssetDatabase.GetDependencies(roots.ToArray(), true))
            {
                if (dependency.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) && !models.Contains(dependency)) models.Add(dependency);
            }

            return models;
        }
    }
}
