using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Editor;
using UnityEditor;
using UnityEngine;
using Settings = Rapadura.Editor.MeshImportSettingsApplier.MeshImportSettings;

namespace Rapadura.Tests.EditorTools
{
    /// <summary>
    /// <see cref="MeshImportSettingsApplier"/> (menu <c>Rapadura ▸ Build ▸ Apply Mesh Import Settings</c>):
    /// the policy is idempotent, never lowers compression, and writing it to a real FBX importer touches
    /// only the mesh settings it owns, never animation/rig/avatar ones. Importer tests change settings
    /// in memory only (no reimport) and restore them.
    /// </summary>
    public class MeshImportSettingsApplierTests
    {
        private const string RiggedHero = "Assets/Models/Characters/Joaquim_RO/Rigged/Joaquim_Rigged_1.fbx";
        private const string StaticEnemy = "Assets/Models/Enemies/Wolf/Wolf.fbx";
        private const string TempFolder = "Assets/__MeshImportSettingsTest";

        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object o in _created)
            {
                if (o != null && !AssetDatabase.Contains(o)) UnityEngine.Object.DestroyImmediate(o);
            }

            _created.Clear();
            if (AssetDatabase.IsValidFolder(TempFolder)) AssetDatabase.DeleteAsset(TempFolder);
        }

        private static IEnumerable<Settings> AllCombinations()
        {
            foreach (ModelImporterMeshCompression compression in Enum.GetValues(typeof(ModelImporterMeshCompression)))
            foreach (ModelImporterTangents tangents in Enum.GetValues(typeof(ModelImporterTangents)))
            foreach (bool readable in new[] { false, true })
            foreach (bool weld in new[] { false, true })
            foreach (MeshOptimizationFlags optimization in new[] { (MeshOptimizationFlags)0, MeshOptimizationFlags.PolygonOrder, MeshOptimizationFlags.Everything })
            {
                yield return new Settings { Compression = compression, Tangents = tangents, IsReadable = readable, WeldVertices = weld, Optimization = optimization };
            }
        }

        [Test]
        public void Decide_IsIdempotent_ForEveryCombination()
        {
            foreach (Settings current in AllCombinations())
            foreach (bool mustStayReadable in new[] { false, true })
            foreach (bool needsTangents in new[] { false, true })
            {
                Settings once = MeshImportSettingsApplier.Decide(current, mustStayReadable, needsTangents);
                Settings twice = MeshImportSettingsApplier.Decide(once, mustStayReadable, needsTangents);
                Assert.AreEqual(once, twice, $"not idempotent from {current} (readable={mustStayReadable}, tangents={needsTangents})");
            }
        }

        [Test]
        public void Decide_RaisesCompressionToMedium_ButNeverLowersHigh()
        {
            foreach (Settings current in AllCombinations())
            {
                Settings desired = MeshImportSettingsApplier.Decide(current, false, true);
                var expected = current.Compression < ModelImporterMeshCompression.Medium ? ModelImporterMeshCompression.Medium : current.Compression;
                Assert.AreEqual(expected, desired.Compression, current.ToString());
            }
        }

        [Test]
        public void Decide_ReadWriteAndTangents_FollowWhatTheModelNeeds()
        {
            var current = new Settings { Compression = ModelImporterMeshCompression.Off, IsReadable = true, Tangents = ModelImporterTangents.CalculateMikk };

            Settings plain = MeshImportSettingsApplier.Decide(current, mustStayReadable: false, needsTangents: false);
            Assert.IsFalse(plain.IsReadable);
            Assert.AreEqual(ModelImporterTangents.None, plain.Tangents);
            Assert.AreEqual(MeshOptimizationFlags.Everything, plain.Optimization);

            Settings collider = MeshImportSettingsApplier.Decide(current, mustStayReadable: true, needsTangents: true);
            Assert.IsTrue(collider.IsReadable, "a MeshCollider user keeps Read/Write");
            Assert.AreEqual(ModelImporterTangents.CalculateMikk, collider.Tangents, "normal-mapped material keeps tangents");

            var stripped = new Settings { Tangents = ModelImporterTangents.None };
            Assert.AreNotEqual(ModelImporterTangents.None, MeshImportSettingsApplier.Decide(stripped, false, true).Tangents, "tangents come back when a normal map appears");
        }

        [TestCase(RiggedHero)]
        [TestCase(StaticEnemy)]
        public void WritingThePolicyTwice_ChangesNothingTheSecondTime_AndLeavesAnimationUntouched(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            Assert.IsNotNull(importer, path);

            Settings original = MeshImportSettingsApplier.Read(importer);
            Dictionary<string, string> before = Snapshot(importer);
            var animationBefore = AnimationApi(importer);
            try
            {
                Settings desired = MeshImportSettingsApplier.DesiredFor(path, importer);
                MeshImportSettingsApplier.Write(importer, desired);
                Assert.AreEqual(desired, MeshImportSettingsApplier.Read(importer));

                Assert.IsFalse(MeshImportSettingsApplier.Write(importer, MeshImportSettingsApplier.DesiredFor(path, importer)), "second pass must change nothing");
                Assert.AreEqual(animationBefore, AnimationApi(importer), "animation/rig/avatar settings changed");

                Dictionary<string, string> after = Snapshot(importer);
                foreach (KeyValuePair<string, string> kv in before)
                {
                    if (after.TryGetValue(kv.Key, out string value) && value != kv.Value)
                    {
                        Assert.IsTrue(IsMeshOwnedProperty(kv.Key), $"{path}: '{kv.Key}' changed ({kv.Value} -> {value}) but is not a mesh setting the applier owns");
                    }
                }
            }
            finally
            {
                MeshImportSettingsApplier.Write(importer, original);
            }

            Assert.AreEqual(original, MeshImportSettingsApplier.Read(importer), "test must leave the importer as it found it");
        }

        [Test]
        public void BuildCompacted_DropsUnusedVertices_KeepsTrianglesWeightsAndBounds()
        {
            Mesh source = MakeMeshWithUnusedVertices();
            Bounds bounds = source.bounds;

            Mesh compacted = MeshImportSettingsApplier.BuildCompacted(source, dropTangents: true, out int removed);
            _created.Add(compacted);

            Assert.AreEqual(2, removed);
            Assert.AreEqual(3, compacted.vertexCount);
            Assert.AreEqual(bounds, compacted.bounds, "bounds kept so framing/grounding does not move");
            Assert.IsFalse(compacted.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent));
            Assert.AreEqual(source.bindposes.Length, compacted.bindposes.Length);

            int[] tris = compacted.triangles;
            Vector3[] v = compacted.vertices;
            Assert.AreEqual(new Vector3(0, 1, 0), v[tris[0]]);
            Assert.AreEqual(new Vector3(1, 1, 0), v[tris[1]]);
            Assert.AreEqual(new Vector3(0, 2, 0), v[tris[2]]);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), compacted.uv[tris[1]]);
            Assert.AreEqual(1, compacted.boneWeights[tris[2]].boneIndex0);
        }

        [Test]
        public void FinalizeCleanedMesh_CompactsCompressesAndLocks_ThenIsANoOp()
        {
            if (!AssetDatabase.IsValidFolder(TempFolder)) AssetDatabase.CreateFolder("Assets", "__MeshImportSettingsTest");
            Mesh mesh = MakeMeshWithUnusedVertices();
            string path = TempFolder + "/Temp_NoPedestal.asset";
            AssetDatabase.CreateAsset(mesh, path);
            Bounds bounds = mesh.bounds;

            Assert.IsTrue(MeshImportSettingsApplier.FinalizeCleanedMesh(mesh, keepTangents: true));
            AssetDatabase.SaveAssets();

            var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            Assert.AreSame(mesh, saved, "same asset object (GUID and references kept)");
            Assert.AreEqual(3, saved.vertexCount);
            Assert.AreEqual(bounds, saved.bounds);
            Assert.AreEqual(MeshImportSettingsApplier.TargetCompression, MeshUtility.GetMeshCompression(saved));
            Assert.IsFalse(saved.isReadable);

            Assert.IsFalse(MeshImportSettingsApplier.FinalizeCleanedMesh(saved, keepTangents: true), "second pass must change nothing");
        }

        /// <summary>After the menu has run, every FBX already matches the policy (new imports fail here until it runs again).</summary>
        [Test]
        public void EveryModel_AlreadyMatchesThePolicy()
        {
            HashSet<string> colliderGuids = MeshImportSettingsApplier.CollectMeshColliderModelGuids();
            var failures = new List<string>();
            foreach (string path in MeshImportSettingsApplier.FindModelPaths())
            {
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                Settings current = MeshImportSettingsApplier.Read(importer);
                Settings desired = MeshImportSettingsApplier.DesiredFor(path, importer, colliderGuids);
                if (!current.Equals(desired)) failures.Add($"{path}: {current} -> {desired}");
            }

            Assert.IsEmpty(failures, "Run Rapadura ▸ Build ▸ Apply Mesh Import Settings:\n" + string.Join("\n", failures));
        }

        // ---------------------------------------------------------------- helpers

        private Mesh MakeMeshWithUnusedVertices()
        {
            var mesh = new Mesh
            {
                vertices = new[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(5, 0, 0), new Vector3(0, 2, 0) },
                uv = new[] { Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.one, Vector2.up },
                boneWeights = new[]
                {
                    new BoneWeight { boneIndex0 = 0, weight0 = 1 }, new BoneWeight { boneIndex0 = 0, weight0 = 1 },
                    new BoneWeight { boneIndex0 = 0, weight0 = 1 }, new BoneWeight { boneIndex0 = 1, weight0 = 1 },
                    new BoneWeight { boneIndex0 = 1, weight0 = 1 },
                },
                bindposes = new[] { Matrix4x4.identity, Matrix4x4.identity },
                triangles = new[] { 1, 2, 4 },
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private static bool IsMeshOwnedProperty(string path) =>
            (path.Contains("ompression") && !path.Contains("nimation")) || path.Contains("eadable") || path.Contains("ptimization")
            || path.Contains("angent") || path.Contains("eld");

        private static Dictionary<string, string> Snapshot(ModelImporter importer)
        {
            var values = new Dictionary<string, string>();
            SerializedProperty it = new SerializedObject(importer).GetIterator();
            while (it.Next(true))
            {
                values[it.propertyPath] = Describe(it);
            }

            return values;
        }

        private static string Describe(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.longValue.ToString();
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.doubleValue.ToString("R");
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.Enum: return p.intValue.ToString();
                case SerializedPropertyType.ObjectReference: return p.objectReferenceInstanceIDValue.ToString();
                case SerializedPropertyType.ArraySize: return p.intValue.ToString();
                case SerializedPropertyType.Vector3: return p.vector3Value.ToString("R");
                case SerializedPropertyType.Quaternion: return p.quaternionValue.ToString("R");
                default: return string.Empty;
            }
        }

        private static string AnimationApi(ModelImporter i) => string.Join("|",
            i.animationType, i.importAnimation, i.clipAnimations.Length, i.defaultClipAnimations.Length, i.animationCompression,
            i.animationRotationError, i.animationPositionError, i.animationScaleError, i.avatarSetup,
            i.sourceAvatar != null ? AssetDatabase.GetAssetPath(i.sourceAvatar) : "-", i.optimizeGameObjects,
            i.humanDescription.human?.Length ?? 0, i.humanDescription.skeleton?.Length ?? 0, i.resampleCurves, i.motionNodeName,
            i.importBlendShapes, i.importConstraints, i.importAnimatedCustomProperties);
    }
}
