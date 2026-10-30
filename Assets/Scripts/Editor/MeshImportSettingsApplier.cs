using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Rapadura.Gameplay.Characters;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Editor
{
    /// <summary>
    /// Build-size pass over every mesh the game ships, without changing how anything looks
    /// (menu <c>Rapadura ▸ Build ▸ Apply Mesh Import Settings</c>). Idempotent: a second run
    /// changes nothing and reimports nothing.
    /// <list type="bullet">
    /// <item><b>FBX importers</b> (<see cref="ModelImporter"/>): mesh compression raised to at least
    /// Medium (never lowered: High stays High), Read/Write off unless a MeshCollider in a build scene or
    /// prefab uses the model (nothing at runtime reads imported mesh data — only bounds, which need no
    /// Read/Write), all mesh optimisations on, tangents dropped when none of the model's materials
    /// samples a normal/parallax/detail map (URP Lit only reads tangents under <c>_NORMALMAP</c>
    /// and friends). Blend shapes are left as imported — none of the Meshy FBXs has any today, and the
    /// hero code looks for a "blink" one. Animation, rig and avatar settings are never touched.</item>
    /// <item><b>Cleaned hero meshes</b> (<c>Resources/CleanedMeshes/*_NoPedestal.asset</c>, written by
    /// <see cref="PedestalRemover"/>): Medium compression, Read/Write off, unused vertices (the cut
    /// pedestal's) compacted out, tangents dropped like their source FBX. Bounds are kept as they were so
    /// preview framing and map-token grounding do not move. Same object/GUID (data copied in place).</item>
    /// </list>
    /// </summary>
    public static class MeshImportSettingsApplier
    {
        public const string MenuPath = "Rapadura/Build/Apply Mesh Import Settings";
        public const string CleanedMeshFolder = "Assets/Resources/CleanedMeshes";
        public const ModelImporterMeshCompression TargetCompression = ModelImporterMeshCompression.Medium;

        /// <summary>Models that must stay Read/Write enabled whatever the scan says, with the reason. Empty today.</summary>
        public static readonly Dictionary<string, string> ReadableAllowList = new Dictionary<string, string>();

        /// <summary>The mesh-only import settings this pass owns. Everything else on the importer is left alone.</summary>
        public struct MeshImportSettings : IEquatable<MeshImportSettings>
        {
            public ModelImporterMeshCompression Compression;
            public bool IsReadable;
            public MeshOptimizationFlags Optimization;
            public ModelImporterTangents Tangents;
            public bool WeldVertices;

            public bool Equals(MeshImportSettings other) =>
                Compression == other.Compression && IsReadable == other.IsReadable && Optimization == other.Optimization
                && Tangents == other.Tangents && WeldVertices == other.WeldVertices;

            public override bool Equals(object obj) => obj is MeshImportSettings other && Equals(other);

            public override int GetHashCode() => HashCode.Combine((int)Compression, IsReadable, (int)Optimization, (int)Tangents, WeldVertices);

            public override string ToString() => $"compression={Compression} readable={IsReadable} optimize={Optimization} tangents={Tangents} weld={WeldVertices}";
        }

        [MenuItem(MenuPath)]
        public static void ApplyFromMenu()
        {
            int changedModels = ApplyToAllModels(out int scannedModels);
            int changedMeshes = ApplyToCleanedMeshes(out int scannedMeshes);
            Debug.Log($"[MeshImportSettings] Models: {changedModels}/{scannedModels} updated. Cleaned meshes: {changedMeshes}/{scannedMeshes} updated. Run again: nothing should change.");
        }

        // ---------------------------------------------------------------- policy (pure)

        /// <summary>
        /// The settings a model should have. Pure and idempotent: <c>Decide(Decide(x)) == Decide(x)</c>.
        /// </summary>
        public static MeshImportSettings Decide(MeshImportSettings current, bool mustStayReadable, bool needsTangents)
        {
            MeshImportSettings desired = current;
            if (current.Compression < TargetCompression)
            {
                desired.Compression = TargetCompression;
            }

            desired.IsReadable = mustStayReadable;
            desired.Optimization = MeshOptimizationFlags.Everything;
            desired.WeldVertices = true;
            if (!needsTangents)
            {
                desired.Tangents = ModelImporterTangents.None;
            }
            else if (current.Tangents == ModelImporterTangents.None)
            {
                desired.Tangents = ModelImporterTangents.CalculateMikk;
            }

            return desired;
        }

        public static MeshImportSettings Read(ModelImporter importer) => new MeshImportSettings
        {
            Compression = importer.meshCompression,
            IsReadable = importer.isReadable,
            Optimization = importer.meshOptimizationFlags,
            Tangents = importer.importTangents,
            WeldVertices = importer.weldVertices,
        };

        /// <summary>Writes only the differing fields (in memory). Returns whether anything changed.</summary>
        public static bool Write(ModelImporter importer, MeshImportSettings desired)
        {
            bool changed = false;
            if (importer.meshCompression != desired.Compression) { importer.meshCompression = desired.Compression; changed = true; }
            if (importer.isReadable != desired.IsReadable) { importer.isReadable = desired.IsReadable; changed = true; }
            if (importer.meshOptimizationFlags != desired.Optimization) { importer.meshOptimizationFlags = desired.Optimization; changed = true; }
            if (importer.importTangents != desired.Tangents) { importer.importTangents = desired.Tangents; changed = true; }
            if (importer.weldVertices != desired.WeldVertices) { importer.weldVertices = desired.WeldVertices; changed = true; }
            return changed;
        }

        // ---------------------------------------------------------------- FBX importers

        public static int ApplyToAllModels(out int scanned)
        {
            HashSet<string> colliderGuids = CollectMeshColliderModelGuids();
            scanned = 0;
            int changed = 0;
            foreach (string path in FindModelPaths())
            {
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
                {
                    continue;
                }

                scanned++;
                MeshImportSettings desired = DesiredFor(path, importer, colliderGuids);
                if (Write(importer, desired))
                {
                    importer.SaveAndReimport();
                    changed++;
                    Debug.Log($"[MeshImportSettings] {path}: {desired}");
                }
            }

            return changed;
        }

        public static MeshImportSettings DesiredFor(string path, ModelImporter importer, HashSet<string> colliderGuids = null)
        {
            colliderGuids ??= CollectMeshColliderModelGuids();
            bool readable = ReadableAllowList.ContainsKey(path) || colliderGuids.Contains(AssetDatabase.AssetPathToGUID(path));
            return Decide(Read(importer), readable, ModelNeedsTangents(path, importer));
        }

        /// <summary>FBX files under Assets/ (packages excluded).</summary>
        public static List<string> FindModelPaths()
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) && !paths.Contains(path))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        /// <summary>
        /// Whether any material this model renders with reads tangents: a normal/bump, parallax/height or
        /// detail map, a <c>*NORMALMAP*</c>/<c>*PARALLAX*</c> keyword, or a shader outside the stock
        /// URP/Standard set (unknown shaders keep tangents — safe default). No material found: keep them.
        /// </summary>
        public static bool ModelNeedsTangents(string path, ModelImporter importer)
        {
            var materials = new List<Material>();
            foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (o is Material m) materials.Add(m);
            }

            foreach (KeyValuePair<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> kv in importer.GetExternalObjectMap())
            {
                if (kv.Value is Material m && !materials.Contains(m)) materials.Add(m);
            }

            if (materials.Count == 0)
            {
                return true;
            }

            foreach (Material m in materials)
            {
                if (MaterialNeedsTangents(m))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly string[] TangentFreeShaders =
        {
            "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/Baked Lit", "Universal Render Pipeline/Complex Lit", "Standard", "Standard (Specular setup)",
        };

        public static bool MaterialNeedsTangents(Material m)
        {
            if (m == null) return false;
            if (m.shader == null || Array.IndexOf(TangentFreeShaders, m.shader.name) < 0) return true;
            foreach (string keyword in m.shaderKeywords)
            {
                if (keyword.Contains("NORMALMAP") || keyword.Contains("PARALLAX") || keyword.Contains("DETAIL")) return true;
            }

            foreach (string prop in m.GetTexturePropertyNames())
            {
                bool tangentMap = prop.Contains("Bump") || prop.Contains("Normal") || prop.Contains("Parallax") || prop.Contains("Height") || prop.Contains("Detail");
                if (tangentMap && m.GetTexture(prop) != null) return true;
            }

            return false;
        }

        private static readonly Regex MeshColliderMeshGuid = new Regex(@"m_Mesh: \{fileID: -?\d+, guid: ([0-9a-f]{32})", RegexOptions.Compiled);

        /// <summary>GUIDs of model files whose meshes a MeshCollider in a build scene or prefab uses (those keep Read/Write).</summary>
        public static HashSet<string> CollectMeshColliderModelGuids()
        {
            var guids = new HashSet<string>();
            var files = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) files.Add(scene.path);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                files.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            foreach (string file in files)
            {
                if (!File.Exists(file)) continue;
                bool inCollider = false;
                foreach (string line in File.ReadLines(file))
                {
                    if (line.StartsWith("--- ", StringComparison.Ordinal)) { inCollider = false; continue; }
                    if (line.StartsWith("MeshCollider:", StringComparison.Ordinal)) { inCollider = true; continue; }
                    if (!inCollider) continue;
                    Match match = MeshColliderMeshGuid.Match(line);
                    if (match.Success) guids.Add(match.Groups[1].Value);
                }
            }

            return guids;
        }

        // ---------------------------------------------------------------- cleaned hero meshes

        public static int ApplyToCleanedMeshes(out int scanned)
        {
            scanned = 0;
            int changed = 0;
            if (!AssetDatabase.IsValidFolder(CleanedMeshFolder))
            {
                return 0;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { CleanedMeshFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Mesh mesh = AssetDatabase.LoadMainAssetAtPath(path) as Mesh;
                if (mesh == null) continue;
                scanned++;
                if (FinalizeCleanedMesh(mesh, KeepTangentsForCleanedMesh(mesh.name)))
                {
                    changed++;
                    Debug.Log($"[MeshImportSettings] {path}: compacted/compressed, Read/Write off.");
                }
            }

            AssetDatabase.SaveAssets();
            return changed;
        }

        /// <summary>The rigged hero FBX a cleaned mesh was cut from (by the project's naming convention), or null.</summary>
        public static string SourceModelPathFor(CharacterId id) => $"Assets/Models/Characters/{id}_RO/Rigged/{id}_Rigged_1.fbx";

        private static bool KeepTangentsForCleanedMesh(string meshName)
        {
            foreach (CharacterId id in Enum.GetValues(typeof(CharacterId)))
            {
                if (meshName == id + "_NoPedestal" && AssetImporter.GetAtPath(SourceModelPathFor(id)) is ModelImporter source)
                {
                    // Match the source FBX's decision so the hero renders with the same material inputs.
                    return ModelNeedsTangents(SourceModelPathFor(id), source);
                }
            }

            return true;
        }

        /// <summary>
        /// Brings a saved Mesh asset to ship state in place (same object and GUID): when its data is
        /// readable, drops unused vertices (and tangents if <paramref name="keepTangents"/> is false) while
        /// keeping its bounds; then Medium compression and Read/Write off. Returns whether anything changed.
        /// </summary>
        public static bool FinalizeCleanedMesh(Mesh mesh, bool keepTangents)
        {
            bool changed = false;
            if (mesh.isReadable)
            {
                bool dropTangents = !keepTangents && mesh.HasVertexAttribute(VertexAttribute.Tangent);
                if (dropTangents || CountReferencedVertices(mesh) < mesh.vertexCount)
                {
                    // Rewritten through the Mesh API: EditorUtility.CopySerialized onto a Mesh asset does not
                    // replace its vertex data.
                    Mesh snapshot = UnityEngine.Object.Instantiate(mesh);
                    BuildCompacted(snapshot, dropTangents, out _, mesh);
                    UnityEngine.Object.DestroyImmediate(snapshot);
                    changed = true;
                }
            }

            if (MeshUtility.GetMeshCompression(mesh) < TargetCompression)
            {
                MeshUtility.SetMeshCompression(mesh, TargetCompression);
                changed = true;
            }

            var so = new SerializedObject(mesh);
            SerializedProperty readable = so.FindProperty("m_IsReadable");
            if (readable != null && readable.boolValue)
            {
                readable.boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(mesh);
            }

            return changed;
        }

        /// <summary>
        /// A copy of <paramref name="source"/> without vertices no triangle references, every channel kept at
        /// its own dimension (UVs, colours, skin weights, blend shapes, bind poses), the same submeshes and
        /// the same bounds. 16-bit indices when the compacted mesh fits. Written into <paramref name="target"/>
        /// (cleared first) when one is given.
        /// </summary>
        public static Mesh BuildCompacted(Mesh source, bool dropTangents, out int removedVertices, Mesh target = null)
        {
            int count = source.vertexCount;
            var remap = new int[count];
            for (int i = 0; i < count; i++) remap[i] = -1;

            var subIndices = new int[source.subMeshCount][];
            int next = 0;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                int[] indices = source.GetIndices(s);
                subIndices[s] = indices;
                foreach (int index in indices)
                {
                    if (remap[index] < 0) remap[index] = next++;
                }
            }

            removedVertices = count - next;
            var order = new int[next];
            for (int i = 0; i < count; i++)
            {
                if (remap[i] >= 0) order[remap[i]] = i;
            }

            Mesh mesh = target;
            if (mesh == null)
            {
                mesh = new Mesh { name = source.name };
            }
            else
            {
                mesh.Clear();
                mesh.ClearBlendShapes();
            }

            mesh.indexFormat = next > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

            mesh.SetVertices(Pick(source.vertices, order));
            if (source.HasVertexAttribute(VertexAttribute.Normal)) mesh.SetNormals(Pick(source.normals, order));
            if (!dropTangents && source.HasVertexAttribute(VertexAttribute.Tangent)) mesh.SetTangents(Pick(source.tangents, order));
            if (source.HasVertexAttribute(VertexAttribute.Color)) mesh.SetColors(Pick(source.colors, order));

            for (int channel = 0; channel < 8; channel++)
            {
                var attribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
                if (!source.HasVertexAttribute(attribute)) continue;
                switch (source.GetVertexAttributeDimension(attribute))
                {
                    case 2: { var uvs = new List<Vector2>(); source.GetUVs(channel, uvs); mesh.SetUVs(channel, Pick(uvs, order)); break; }
                    case 3: { var uvs = new List<Vector3>(); source.GetUVs(channel, uvs); mesh.SetUVs(channel, Pick(uvs, order)); break; }
                    default: { var uvs = new List<Vector4>(); source.GetUVs(channel, uvs); mesh.SetUVs(channel, Pick(uvs, order)); break; }
                }
            }

            CopyBoneWeights(source, mesh, order);
            mesh.bindposes = source.bindposes;

            mesh.subMeshCount = source.subMeshCount;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                int[] indices = subIndices[s];
                var remapped = new int[indices.Length];
                for (int i = 0; i < indices.Length; i++) remapped[i] = remap[indices[i]];
                mesh.SetIndices(remapped, source.GetTopology(s), s, false);
            }

            CopyBlendShapes(source, mesh, order);
            mesh.bounds = source.bounds;
            return mesh;
        }

        private static int CountReferencedVertices(Mesh mesh)
        {
            var used = new bool[mesh.vertexCount];
            int count = 0;
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                foreach (int index in mesh.GetIndices(s))
                {
                    if (!used[index]) { used[index] = true; count++; }
                }
            }

            return count;
        }

        private static T[] Pick<T>(IList<T> values, int[] order)
        {
            var result = new T[order.Length];
            for (int i = 0; i < order.Length; i++) result[i] = values[order[i]];
            return result;
        }

        private static void CopyBoneWeights(Mesh source, Mesh target, int[] order)
        {
            NativeArray<byte> perVertex = source.GetBonesPerVertex();
            NativeArray<BoneWeight1> weights = source.GetAllBoneWeights();
            if (perVertex.Length != source.vertexCount || weights.Length == 0)
            {
                return;
            }

            var starts = new int[perVertex.Length];
            int running = 0;
            for (int i = 0; i < perVertex.Length; i++) { starts[i] = running; running += perVertex[i]; }

            var newPerVertex = new NativeArray<byte>(order.Length, Allocator.Temp);
            var newWeights = new List<BoneWeight1>(weights.Length);
            for (int i = 0; i < order.Length; i++)
            {
                int src = order[i];
                byte n = perVertex[src];
                newPerVertex[i] = n;
                for (int k = 0; k < n; k++) newWeights.Add(weights[starts[src] + k]);
            }

            var weightArray = new NativeArray<BoneWeight1>(newWeights.ToArray(), Allocator.Temp);
            target.SetBoneWeights(newPerVertex, weightArray);
            newPerVertex.Dispose();
            weightArray.Dispose();
        }

        private static void CopyBlendShapes(Mesh source, Mesh target, int[] order)
        {
            int count = source.vertexCount;
            var dv = new Vector3[count];
            var dn = new Vector3[count];
            var dt = new Vector3[count];
            for (int shape = 0; shape < source.blendShapeCount; shape++)
            {
                string name = source.GetBlendShapeName(shape);
                for (int frame = 0; frame < source.GetBlendShapeFrameCount(shape); frame++)
                {
                    source.GetBlendShapeFrameVertices(shape, frame, dv, dn, dt);
                    target.AddBlendShapeFrame(name, source.GetBlendShapeFrameWeight(shape, frame), Pick(dv, order), Pick(dn, order), Pick(dt, order));
                }
            }
        }
    }
}
