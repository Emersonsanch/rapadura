using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="EnemyVisualSwapper"/> — the EnemyDefinition → visual mapping used
    /// by EncounterArenaController/EnemySpawner: definition model first, then a per-enemy primitive
    /// placeholder, then the shared prefab's own visual. EnemyDefinition fields are set via
    /// reflection like <c>BossBonusRulesTests</c>.
    /// </summary>
    public class EnemyVisualSwapperTests
    {
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

        // ------------------------------------------------------------------
        // Helpers.
        // ------------------------------------------------------------------

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private EnemyDefinition MakeEnemy(string id, GameObject visualPrefab = null, bool usePlaceholder = true)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            SetField(enemy, "_visualPrefab", visualPrefab);
            SetField(enemy, "_usePlaceholderVisual", usePlaceholder);
            _created.Add(enemy);
            return enemy;
        }

        /// <summary>Mimics Enemy_Encounter.prefab: root with a 2 m capsule and an "EnemyVisual" child.</summary>
        private GameObject MakeEnemyRoot(out Transform originalVisual)
        {
            var root = new GameObject("Enemy_Test");
            _created.Add(root);
            CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = Vector3.zero;

            originalVisual = new GameObject(EnemyVisualSwapper.VisualName).transform;
            originalVisual.SetParent(root.transform, false);
            originalVisual.localPosition = new Vector3(0f, -0.5f, 0f);
            return root;
        }

        private GameObject MakeModelPrefab(string name)
        {
            var model = new GameObject(name);
            _created.Add(model);
            return model;
        }

        private static List<Transform> VisualChildren(GameObject root)
        {
            var visuals = new List<Transform>();
            foreach (Transform child in root.transform)
            {
                if (child.name == EnemyVisualSwapper.VisualName)
                {
                    visuals.Add(child);
                }
            }

            return visuals;
        }

        // ------------------------------------------------------------------
        // ResolveSource (pure mapping/fallback rules).
        // ------------------------------------------------------------------

        [Test]
        public void ResolveSource_NullDefinition_FallsBackToSharedPrefab()
        {
            Assert.AreEqual(EnemyVisualSource.SharedPrefab, EnemyVisualSwapper.ResolveSource(null));
        }

        [Test]
        public void ResolveSource_DefinitionWithModel_UsesDefinitionPrefab()
        {
            EnemyDefinition scorpion = MakeEnemy("enemy_desert_scorpion", MakeModelPrefab("Scorpion"));

            Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, EnemyVisualSwapper.ResolveSource(scorpion));
        }

        [Test]
        public void ResolveSource_NoModel_UsesPlaceholder_UnlessDisabled()
        {
            Assert.AreEqual(EnemyVisualSource.Placeholder, EnemyVisualSwapper.ResolveSource(MakeEnemy("enemy_golem")));
            Assert.AreEqual(EnemyVisualSource.SharedPrefab, EnemyVisualSwapper.ResolveSource(MakeEnemy("enemy_plain", usePlaceholder: false)));
        }

        // ------------------------------------------------------------------
        // Apply.
        // ------------------------------------------------------------------

        [Test]
        public void Apply_WithModel_InstantiatesItWithDefinitionTransform_AndHidesSharedVisual()
        {
            GameObject root = MakeEnemyRoot(out Transform original);
            EnemyDefinition scorpion = MakeEnemy("enemy_desert_scorpion", MakeModelPrefab("Scorpion"));
            SetField(scorpion, "_visualLocalPosition", new Vector3(0f, -0.4f, 0f));
            SetField(scorpion, "_visualLocalEulerAngles", new Vector3(-90f, 0f, 0f));
            SetField(scorpion, "_visualScale", 0.018f);

            Transform visual = EnemyVisualSwapper.Apply(root, scorpion);

            Assert.IsNotNull(visual);
            Assert.AreNotSame(original, visual);
            Assert.AreEqual(root.transform, visual.parent);
            Assert.AreEqual(EnemyVisualSwapper.VisualName, visual.name);
            Assert.AreEqual(-0.4f, visual.localPosition.y, 0.0001f);
            Assert.AreEqual(0.018f, visual.localScale.x, 0.0001f);
            Assert.That(Quaternion.Angle(Quaternion.Euler(-90f, 0f, 0f), visual.localRotation), Is.LessThan(0.01f));

            EnemyVisualMarker marker = visual.GetComponent<EnemyVisualMarker>();
            Assert.IsNotNull(marker);
            Assert.AreSame(scorpion, marker.Source);
            Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, marker.Kind);

            Assert.IsTrue(original != null, "The shared prefab visual must be kept (only hidden) so pooled enemies can return to it.");
            Assert.IsFalse(original.gameObject.activeSelf);
        }

        [Test]
        public void Apply_WithoutModel_BuildsColliderlessPlaceholderRestingOnCapsuleBottom()
        {
            GameObject root = MakeEnemyRoot(out Transform original);
            EnemyDefinition golem = MakeEnemy("enemy_mountain_stone_golem");
            SetField(golem, "_placeholderShape", EnemyPlaceholderShape.Cube);
            SetField(golem, "_placeholderScale", new Vector3(1.2f, 1.4f, 1f));

            Transform visual = EnemyVisualSwapper.Apply(root, golem);

            Assert.IsNotNull(visual);
            Assert.IsFalse(original.gameObject.activeSelf);
            Assert.AreEqual(EnemyVisualSource.Placeholder, visual.GetComponent<EnemyVisualMarker>().Kind);
            Assert.AreEqual(new Vector3(1.2f, 1.4f, 1f), visual.localScale);

            // Cube half height 0.5 * 1.4 above the capsule bottom (-1).
            Assert.AreEqual(-1f + 0.7f, visual.localPosition.y, 0.0001f);

            Assert.IsNotNull(visual.GetComponent<MeshRenderer>());
            Assert.IsEmpty(visual.GetComponentsInChildren<Collider>(true), "Placeholder primitives must not add colliders to the enemy.");
            Assert.IsNotNull(visual.Find(EnemyVisualSwapper.SnoutName), "Placeholder should mark its front with a snout.");
        }

        [Test]
        public void Apply_TwoModellessEnemies_GetDistinctPlaceholderLooks()
        {
            GameObject rootA = MakeEnemyRoot(out _);
            GameObject rootB = MakeEnemyRoot(out _);
            EnemyDefinition bats = MakeEnemy("enemy_cave_bat_swarm");
            EnemyDefinition dweller = MakeEnemy("enemy_cave_dweller");

            Renderer a = EnemyVisualSwapper.Apply(rootA, bats).GetComponent<Renderer>();
            Renderer b = EnemyVisualSwapper.Apply(rootB, dweller).GetComponent<Renderer>();

            Assert.AreNotEqual(EnemyVisualSwapper.GetPlaceholderColor(bats), EnemyVisualSwapper.GetPlaceholderColor(dweller));
            Assert.AreNotSame(a.sharedMaterial, b.sharedMaterial);
        }

        [Test]
        public void GetPlaceholderColor_UsesAuthoredColorOpaque_ElseStableAutoColor()
        {
            EnemyDefinition authored = MakeEnemy("enemy_warlord");
            SetField(authored, "_placeholderColor", new Color(0.6f, 0.12f, 0.1f, 0.3f));
            Assert.AreEqual(new Color(0.6f, 0.12f, 0.1f, 1f), EnemyVisualSwapper.GetPlaceholderColor(authored));

            EnemyDefinition auto = MakeEnemy("enemy_unpainted");
            Color first = EnemyVisualSwapper.GetPlaceholderColor(auto);
            Assert.AreEqual(first, EnemyVisualSwapper.GetPlaceholderColor(auto), "Auto color must be deterministic.");
            Assert.AreEqual(1f, first.a);
        }

        [Test]
        public void Apply_SameDefinitionTwice_ReusesVisual()
        {
            GameObject root = MakeEnemyRoot(out _);
            EnemyDefinition spider = MakeEnemy("enemy_forest_spider", MakeModelPrefab("Spider"));

            Transform first = EnemyVisualSwapper.Apply(root, spider);
            Transform second = EnemyVisualSwapper.Apply(root, spider);

            Assert.AreSame(first, second);
            Assert.AreEqual(2, VisualChildren(root).Count, "Only the hidden shared visual and one built visual should exist.");
        }

        [Test]
        public void Apply_PooledEnemyReconfigured_ReplacesBuiltVisual_AndCanFallBackToSharedPrefab()
        {
            GameObject root = MakeEnemyRoot(out Transform original);
            EnemyDefinition scorpion = MakeEnemy("enemy_desert_scorpion", MakeModelPrefab("Scorpion"));
            EnemyDefinition golem = MakeEnemy("enemy_mountain_stone_golem");
            EnemyDefinition plain = MakeEnemy("enemy_plain", usePlaceholder: false);

            EnemyVisualSwapper.Apply(root, scorpion);
            Transform golemVisual = EnemyVisualSwapper.Apply(root, golem);

            Assert.AreEqual(2, VisualChildren(root).Count);
            Assert.AreSame(golem, golemVisual.GetComponent<EnemyVisualMarker>().Source);

            Transform back = EnemyVisualSwapper.Apply(root, plain);

            Assert.AreSame(original, back);
            Assert.IsTrue(original.gameObject.activeSelf);
            Assert.AreEqual(1, VisualChildren(root).Count, "Built visual must be removed when falling back to the shared prefab.");
            Assert.IsNull(root.GetComponentInChildren<EnemyVisualMarker>(true));
        }

        [Test]
        public void Apply_NullDefinition_KeepsSharedVisual()
        {
            GameObject root = MakeEnemyRoot(out Transform original);

            Assert.AreSame(original, EnemyVisualSwapper.Apply(root, null));
            Assert.IsTrue(original.gameObject.activeSelf);
            Assert.IsNull(EnemyVisualSwapper.Apply(null, null));
        }

        // ------------------------------------------------------------------
        // Standing on the ground (regression: encounter enemies were buried).
        // The shared prefab's pivot is its capsule centre; EncounterArenaController used to put
        // that pivot on the raycast ground point, sinking the capsule bottom — and every model
        // resting on it — a full metre under the floor.
        // ------------------------------------------------------------------

        private static readonly string[] ModelEnemyIds =
        {
            "enemy_forest_wolf", "enemy_forest_alpha_wolf", "enemy_forest_spider", "enemy_desert_scorpion",
        };

        private static readonly string[] PlaceholderEnemyIds =
        {
            "enemy_cave_bat_swarm", "enemy_cave_dweller", "enemy_desert_ruin_guardian",
            "enemy_mountain_stone_golem", "enemy_mountain_warlord",
        };

        private GameObject MakeMeshModelPrefab(string name, Vector3 meshScale)
        {
            // Collider-less cube mesh under a child, like an imported FBX (root + mesh node).
            var model = new GameObject(name);
            _created.Add(model);
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(mesh.GetComponent<Collider>());
            mesh.transform.SetParent(model.transform, false);
            mesh.transform.localScale = meshScale;
            return model;
        }

        private static float MeshBottomInRootSpace(GameObject root, Transform visual)
        {
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual, out Bounds bounds), "visual has no mesh");
            return bounds.min.y;
        }

        [Test]
        public void GetFootHeight_IsCapsuleBottom_AndStandOnGroundLiftsThePivotByIt()
        {
            GameObject root = MakeEnemyRoot(out _);
            Assert.AreEqual(-1f, EnemyVisualSwapper.GetFootHeight(root.transform), 0.0001f, "2 m capsule centred on the pivot");

            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.5f, 0f);
            capsule.height = 1f;
            Assert.AreEqual(0f, EnemyVisualSwapper.GetFootHeight(root.transform), 0.0001f, "pivot already at the feet");

            Object.DestroyImmediate(capsule);
            Assert.AreEqual(EnemyVisualSwapper.DefaultFootHeight, EnemyVisualSwapper.GetFootHeight(root.transform));

            Vector3 standing = Rapadura.Gameplay.World.Map.EncounterSpawnLayout.StandOnGround(new Vector3(3f, 0.25f, 4f), -1f);
            Assert.AreEqual(new Vector3(3f, 1.25f, 4f), standing, "pivot must sit one capsule half-height above the ground hit");
        }

        [Test]
        public void Apply_WithModel_RestsMeshBottomOnCapsuleBottom_WhateverTheAuthoredHeight()
        {
            // Authored y/scale that would bury the model (alpha wolf case: scale raised, y kept).
            GameObject root = MakeEnemyRoot(out _);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", MakeMeshModelPrefab("Wolf", new Vector3(70f, 100f, 50f)));
            SetField(alpha, "_visualLocalPosition", new Vector3(0.1f, -0.5f, 0.2f));
            SetField(alpha, "_visualLocalEulerAngles", new Vector3(-90f, 0f, 0f));
            SetField(alpha, "_visualScale", 0.019f);

            Transform visual = EnemyVisualSwapper.Apply(root, alpha);

            Assert.AreEqual(-1f, MeshBottomInRootSpace(root, visual), 0.001f);
            Assert.AreEqual(0.1f, visual.localPosition.x, 0.0001f, "only the height is corrected");
            Assert.AreEqual(0.2f, visual.localPosition.z, 0.0001f);

            // And a floating one (spider case) comes down.
            GameObject root2 = MakeEnemyRoot(out _);
            EnemyDefinition spider = MakeEnemy("enemy_forest_spider", MakeMeshModelPrefab("Spider", new Vector3(100f, 70f, 60f)));
            SetField(spider, "_visualLocalPosition", new Vector3(0f, 0.3f, 0f));
            SetField(spider, "_visualScale", 0.0145f);
            Assert.AreEqual(-1f, MeshBottomInRootSpace(root2, EnemyVisualSwapper.Apply(root2, spider)), 0.001f);
        }

        [Test]
        public void Apply_BossWithModel_GetsTintedMaterialCopy_RegularEnemyKeepsTheShared()
        {
            GameObject prefab = MakeMeshModelPrefab("Wolf", new Vector3(70f, 100f, 50f));
            Material shared = prefab.GetComponentInChildren<Renderer>().sharedMaterial;
            Color sharedColor = shared.color;

            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", prefab);
            EnemyDefinition alpha = MakeEnemy("enemy_forest_alpha_wolf", prefab);
            SetField(alpha, "_isBoss", true);

            Renderer wolfRenderer = EnemyVisualSwapper.Apply(MakeEnemyRoot(out _), wolf).GetComponentInChildren<Renderer>();
            Renderer alphaRenderer = EnemyVisualSwapper.Apply(MakeEnemyRoot(out _), alpha).GetComponentInChildren<Renderer>();

            Assert.AreSame(shared, wolfRenderer.sharedMaterial, "a regular enemy keeps the model's own material");
            Assert.AreNotSame(shared, alphaRenderer.sharedMaterial, "a boss gets a tinted copy");
            Assert.AreEqual(shared.color * EnemyVisualSwapper.BossModelTint, alphaRenderer.sharedMaterial.color);
            Assert.AreEqual(sharedColor, shared.color, "the shared source material is not modified");
        }

        [Test]
        public void ShippedModelEnemies_ResolveToTheirModel_AndStandOnTheGroundWhenSpawnedOnIt()
        {
            foreach (string id in ModelEnemyIds)
            {
                EnemyDefinition definition = Resources.Load<EnemyDefinition>("Enemies/" + id);
                Assert.IsNotNull(definition, id);
                Assert.IsNotNull(definition.VisualPrefab, $"{id}: model reference must load (else it falls back to a capsule placeholder)");
                Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, EnemyVisualSwapper.ResolveSource(definition), id);

                GameObject root = MakeEnemyRoot(out Transform original);
                root.transform.position = Rapadura.Gameplay.World.Map.EncounterSpawnLayout.StandOnGround(
                    new Vector3(2f, 0f, 5f), EnemyVisualSwapper.GetFootHeight(root.transform));

                Transform visual = EnemyVisualSwapper.Apply(root, definition);

                Assert.IsFalse(original.gameObject.activeSelf, id);
                Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, visual.GetComponent<EnemyVisualMarker>().Kind, id);
                Assert.IsNotNull(visual.GetComponentInChildren<MeshRenderer>(), id);
                Assert.IsNull(visual.Find(EnemyVisualSwapper.SnoutName), $"{id}: a model enemy must not also get a placeholder");
                Assert.AreEqual(1, root.GetComponentsInChildren<EnemyVisualMarker>(true).Length, id);

                // Mesh bottom on the capsule bottom, i.e. on the ground (world y 0).
                Assert.AreEqual(-1f, MeshBottomInRootSpace(root, visual), 0.01f, id);
                Assert.Greater(root.transform.position.y, 0.5f, $"{id}: pivot must be lifted above the ground point");
            }
        }

        [Test]
        public void ShippedPlaceholderEnemies_StandOnTheGroundWhenSpawnedOnIt()
        {
            foreach (string id in PlaceholderEnemyIds)
            {
                EnemyDefinition definition = Resources.Load<EnemyDefinition>("Enemies/" + id);
                Assert.IsNotNull(definition, id);
                Assert.AreEqual(EnemyVisualSource.Placeholder, EnemyVisualSwapper.ResolveSource(definition), id);

                GameObject root = MakeEnemyRoot(out _);
                Transform visual = EnemyVisualSwapper.Apply(root, definition);

                Assert.AreEqual(-1f, MeshBottomInRootSpace(root, visual), 0.01f, id);
            }
        }

        [Test]
        public void Apply_RetargetsProceduralAnimatorToNewVisual()
        {
            GameObject root = MakeEnemyRoot(out Transform original);
            EnemyProceduralAnimator animator = root.AddComponent<EnemyProceduralAnimator>();
            SetField(animator, "_visualRoot", original);
            EnemyDefinition wolf = MakeEnemy("enemy_forest_wolf", MakeModelPrefab("Wolf"));

            Transform visual = EnemyVisualSwapper.Apply(root, wolf);

            Assert.AreSame(visual, animator.VisualRoot);
            Assert.DoesNotThrow(() => animator.Tick(0.016f));
        }
    }
}
