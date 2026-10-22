using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="PedestalMeshUtility"/>: the cleaned (no "toy stand") meshes must
    /// reach static MeshFilter models too — the character-select preview shows Joaquim from the static
    /// Joaquim_RO.fbx, which kept its pedestal while only SkinnedMeshRenderers were swapped.
    /// </summary>
    public class PedestalMeshUtilityTests
    {
        private const string JoaquimStaticFbx = "Assets/Models/Characters/Joaquim_RO/Joaquim_RO.fbx";

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private Mesh MakeBoxMesh(Vector3 size)
        {
            var mesh = new Mesh();
            _created.Add(mesh);
            Vector3 e = size * 0.5f;
            mesh.vertices = new[] { -e, e, new Vector3(e.x, -e.y, -e.z) };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            return mesh;
        }

        [Test]
        public void ApplyToStaticMesh_ZUpOriginal_YUpCleaned_SwapsMeshAndStandsItUp()
        {
            var go = new GameObject("StaticHero");
            _created.Add(go);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = MakeBoxMesh(new Vector3(80f, 68f, 100f)); // Z-up static export
            go.transform.localRotation = Quaternion.Euler(270f, 180f, 0f);
            Quaternion before = go.transform.localRotation;

            Mesh cleaned = MakeBoxMesh(new Vector3(80f, 100f, 68f)); // Y-up rigged export
            PedestalMeshUtility.ApplyToStaticMesh(filter, cleaned);

            Assert.AreSame(cleaned, filter.sharedMesh);
            Assert.That(Quaternion.Angle(before * Quaternion.Euler(90f, 0f, 0f), go.transform.localRotation), Is.LessThan(0.01f));
        }

        [Test]
        public void ApplyToStaticMesh_SameUpAxis_KeepsRotation()
        {
            var go = new GameObject("StaticHero");
            _created.Add(go);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = MakeBoxMesh(new Vector3(80f, 100f, 68f));
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            PedestalMeshUtility.ApplyToStaticMesh(filter, MakeBoxMesh(new Vector3(80f, 100f, 68f)));

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 180f, 0f), go.transform.localRotation), Is.LessThan(0.01f));
        }

        [Test]
        public void ApplyIfAvailable_StaticJoaquimPreviewModel_GetsCleanedMesh_Upright()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(JoaquimStaticFbx);
            Assert.IsNotNull(prefab, JoaquimStaticFbx);
            Mesh cleaned = PedestalMeshUtility.LoadCleanedMesh(CharacterId.Joaquim);
            Assert.IsNotNull(cleaned, "Resources/CleanedMeshes/Joaquim_NoPedestal");

            // Same placement as the WorldMap CharacterPreviewController entry.
            GameObject instance = Object.Instantiate(prefab);
            _created.Add(instance);
            instance.transform.localEulerAngles = new Vector3(270f, 180f, 0f);
            instance.transform.localScale = Vector3.one * 0.014f;
            Assert.IsNull(instance.GetComponentInChildren<SkinnedMeshRenderer>(), "static model expected");

            PedestalMeshUtility.ApplyIfAvailable(instance, CharacterId.Joaquim);

            MeshFilter filter = instance.GetComponentInChildren<MeshFilter>();
            Assert.AreSame(cleaned, filter.sharedMesh);

            // Upright: the cleaned mesh's own up (+Y) must point up in the world.
            Vector3 meshUp = filter.transform.TransformDirection(Vector3.up);
            Assert.Greater(Vector3.Dot(meshUp.normalized, Vector3.up), 0.99f);
        }

        [Test]
        public void ApplyIfAvailable_CharacterWithoutCleanedMesh_IsNoOp()
        {
            var go = new GameObject("Hero");
            _created.Add(go);
            MeshFilter filter = go.AddComponent<MeshFilter>();
            Mesh original = MakeBoxMesh(Vector3.one);
            filter.sharedMesh = original;

            PedestalMeshUtility.ApplyIfAvailable(go, CharacterId.Maria);

            Assert.AreSame(original, filter.sharedMesh);
        }
    }
}
