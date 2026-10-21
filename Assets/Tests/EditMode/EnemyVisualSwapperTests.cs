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
