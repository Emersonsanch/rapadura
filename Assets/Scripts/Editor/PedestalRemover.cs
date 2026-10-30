using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Meshy's rigged FBX exports include a flat circular "action figure" stand fused into the same
    /// skinned mesh as the character body (confirmed by vertex-height histogram: a dense flat cluster
    /// at the very bottom, then another dense flat cluster a bit higher for the stand's top surface,
    /// with almost nothing in between — classic pedestal silhouette). There's no separate submesh/
    /// material to hide, and we have no mesh-editing tool (Blender etc.) in this environment, so this
    /// strips it by removing any triangle whose 3 vertices are all below a height threshold and saves
    /// the result as a sibling .asset (the source .fbx is never modified — it would just get
    /// overwritten on the next reimport). Reassign the result to the SkinnedMeshRenderer(s) that used
    /// the original mesh (scene instances and/or prefabs).
    /// </summary>
    public static class PedestalRemover
    {
        [MenuItem("Rapadura/Setup/Remove Pedestal From Selected SkinnedMeshRenderer")]
        public static void RemoveFromSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("[PedestalRemover] Select a GameObject with a SkinnedMeshRenderer (or a parent of one) first.");
                return;
            }

            SkinnedMeshRenderer smr = selected.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null || smr.sharedMesh == null)
            {
                Debug.LogError("[PedestalRemover] No SkinnedMeshRenderer with a mesh found under " + selected.name);
                return;
            }

            Mesh cleaned = RemovePedestal(smr.sharedMesh, 6.5f, out int removedTriangles);
            if (cleaned == null)
            {
                Debug.LogWarning("[PedestalRemover] Nothing removed for " + smr.sharedMesh.name);
                return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(smr.sharedMesh);
            string dir = Path.GetDirectoryName(sourcePath);
            string outPath = Path.Combine(dir, smr.sharedMesh.name + "_NoPedestal.asset").Replace('\\', '/');

            // Overwrite in place when the asset exists: deleting and recreating it would give it a new
            // GUID and break every prefab variant / scene override that points at the cleaned mesh.
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(outPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(cleaned, existing);
                Object.DestroyImmediate(cleaned);
                cleaned = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(cleaned, outPath);
            }

            // Ship state right away (the 29 MB Joaquim_NoPedestal of 2026-10 was saved uncompressed and
            // readable: float32 position/normal/tangent/UV + skin weights for 304k vertices).
            bool keepTangents = !(AssetImporter.GetAtPath(sourcePath) is ModelImporter sourceImporter)
                || MeshImportSettingsApplier.ModelNeedsTangents(sourcePath, sourceImporter);
            MeshImportSettingsApplier.FinalizeCleanedMesh(cleaned, keepTangents);
            AssetDatabase.SaveAssets();

            smr.sharedMesh = cleaned;
            EditorUtility.SetDirty(smr);

            Debug.Log("[PedestalRemover] Removed " + removedTriangles + " pedestal triangles. Saved cleaned mesh to " + outPath + " and assigned it to " + selected.name);
        }

        /// <summary>
        /// Removes triangles fully below <paramref name="heightThreshold"/> (mesh-local Y) and
        /// returns a new Mesh preserving vertices/normals/uvs/tangents/bone weights/bindposes so
        /// skinning keeps working. Vertices no kept triangle uses are compacted out
        /// (<see cref="MeshImportSettingsApplier.BuildCompacted"/> remaps every index and channel).
        /// </summary>
        public static Mesh RemovePedestal(Mesh source, float heightThreshold, out int removedTriangleCount)
        {
            removedTriangleCount = 0;
            Vector3[] vertices = source.vertices;
            int[] triangles = source.triangles;

            var keptTriangles = new List<int>(triangles.Length);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                bool allBelow = vertices[a].y < heightThreshold && vertices[b].y < heightThreshold && vertices[c].y < heightThreshold;
                if (allBelow)
                {
                    removedTriangleCount++;
                    continue;
                }

                keptTriangles.Add(a);
                keptTriangles.Add(b);
                keptTriangles.Add(c);
            }

            if (removedTriangleCount == 0)
            {
                return null;
            }

            // Build on a temporary copy with only the kept triangles, then compact: the old version kept
            // every pedestal vertex in the buffer and wrote the asset uncompressed and readable.
            var cut = new Mesh
            {
                name = source.name + "_NoPedestal",
                indexFormat = source.indexFormat,
                vertices = vertices,
                normals = source.normals,
                tangents = source.tangents,
                uv = source.uv,
                uv2 = source.uv2,
                colors = source.colors,
                boneWeights = source.boneWeights,
                bindposes = source.bindposes,
            };
            cut.triangles = keptTriangles.ToArray();
            cut.RecalculateBounds();

            Mesh mesh = MeshImportSettingsApplier.BuildCompacted(cut, dropTangents: false, out _);
            Object.DestroyImmediate(cut);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
