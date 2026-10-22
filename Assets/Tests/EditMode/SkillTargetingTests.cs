using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Playtest regression: hotbar skills always aimed at position + forward * Range, so enemies
    /// closer than Range were missed (4 casts into a boss at 6m did 0). Covers the pure
    /// <see cref="SkillTargeting"/> selection, <see cref="SkillHotbar"/> casting through it, and
    /// <see cref="SkillManager"/>'s area hit resolution (child colliders, one hit per target).
    /// </summary>
    public class SkillTargetingTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();

            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        // ---------------------------------------------------------------
        // Pure selection
        // ---------------------------------------------------------------

        [Test]
        public void SelectTargetIndex_PrefersNearestInsideForwardCone_OverNearerBehind()
        {
            var positions = new List<Vector3> { new Vector3(0f, 0f, -2f), new Vector3(0f, 0f, 6f), new Vector3(1f, 0f, 4f) };

            int index = SkillTargeting.SelectTargetIndex(Vector3.zero, Vector3.forward, 10f, 70f, positions);

            Assert.AreEqual(2, index, "Nearest candidate inside the cone wins even though one behind is closer.");
        }

        [Test]
        public void SelectTargetIndex_NoneInCone_FallsBackToNearestInReach()
        {
            var positions = new List<Vector3> { new Vector3(0f, 0f, -5f), new Vector3(-3f, 0f, -1f) };

            Assert.AreEqual(1, SkillTargeting.SelectTargetIndex(Vector3.zero, Vector3.forward, 10f, 70f, positions));
        }

        [Test]
        public void SelectTargetIndex_OutOfReach_ReturnsMinusOne()
        {
            var positions = new List<Vector3> { new Vector3(0f, 0f, 12f) };

            Assert.AreEqual(-1, SkillTargeting.SelectTargetIndex(Vector3.zero, Vector3.forward, 10f, 70f, positions));
        }

        [Test]
        public void SelectTargetIndex_IgnoresHeightDifference()
        {
            var positions = new List<Vector3> { new Vector3(0f, 30f, 5f) };

            Assert.AreEqual(0, SkillTargeting.SelectTargetIndex(Vector3.zero, Vector3.forward, 6f, 70f, positions));
        }

        [Test]
        public void GetAimMode_ClassifiesSelfAreaAndSingleTarget()
        {
            Assert.AreEqual(SkillAimMode.Self, SkillTargeting.GetAimMode(CreateSkill(damage: 0f, range: 5f, area: 0f)), "Pure buff/heal");
            Assert.AreEqual(SkillAimMode.Self, SkillTargeting.GetAimMode(CreateSkill(damage: 20f, range: 0f, area: 3f)), "Point-blank");
            Assert.AreEqual(SkillAimMode.Area, SkillTargeting.GetAimMode(CreateSkill(damage: 20f, range: 10f, area: 3f)));
            Assert.AreEqual(SkillAimMode.SingleTarget, SkillTargeting.GetAimMode(CreateSkill(damage: 20f, range: 10f, area: 0f)));
        }

        [Test]
        public void ResolveAim_SingleTarget_AimsAtEnemyCloserThanRange()
        {
            FakeTarget boss = CreateFakeTarget(new Vector3(0f, 0f, 6f));
            SkillDefinition bolt = CreateSkill(damage: 20f, range: 20f, area: 0f);

            SkillAim aim = SkillTargeting.ResolveAim(bolt, Vector3.zero, Vector3.forward, new ICombatTarget[] { boss });

            Assert.AreSame(boss, aim.Target);
            Assert.AreEqual(new Vector3(0f, 0f, 6f), aim.Point);
        }

        [Test]
        public void ResolveAim_Area_TargetBeyondRangeButWithinRadius_ClampsToRangeTowardTarget()
        {
            FakeTarget enemy = CreateFakeTarget(new Vector3(6f, 0f, 0f));
            SkillDefinition quake = CreateSkill(damage: 30f, range: 3f, area: 5f);

            SkillAim aim = SkillTargeting.ResolveAim(quake, Vector3.zero, Vector3.forward, new ICombatTarget[] { enemy });

            Assert.AreSame(enemy, aim.Target);
            Assert.That(Vector3.Distance(aim.Point, new Vector3(3f, 0f, 0f)), Is.LessThan(0.001f));
        }

        [Test]
        public void ResolveAim_SelfSkill_AimsAtCaster_EvenWithEnemyNearby()
        {
            FakeTarget enemy = CreateFakeTarget(new Vector3(0f, 0f, 2f));
            SkillDefinition heal = CreateSkill(damage: 0f, range: 5f, area: 0f);
            var origin = new Vector3(1f, 0f, 1f);

            SkillAim aim = SkillTargeting.ResolveAim(heal, origin, Vector3.forward, new ICombatTarget[] { enemy });

            Assert.IsFalse(aim.HasTarget);
            Assert.AreEqual(origin, aim.Point);
        }

        [Test]
        public void ResolveAim_DeadOrNoCandidates_KeepsForwardAimAtRange()
        {
            FakeTarget dead = CreateFakeTarget(new Vector3(0f, 0f, 3f));
            dead.Alive = false;
            SkillDefinition bolt = CreateSkill(damage: 20f, range: 8f, area: 0f);

            SkillAim aim = SkillTargeting.ResolveAim(bolt, Vector3.zero, Vector3.right, new ICombatTarget[] { dead, null });

            Assert.IsFalse(aim.HasTarget);
            Assert.AreEqual(new Vector3(8f, 0f, 0f), aim.Point);
        }

        // ---------------------------------------------------------------
        // Casting end-to-end
        // ---------------------------------------------------------------

        [Test]
        public void HotbarCast_SingleTarget_DamagesBossCloserThanRange_AndFacesIt()
        {
            SkillHotbar hotbar = CreateCaster(out SkillManager skillManager);
            SkillDefinition bolt = CreateSkill(damage: 25f, range: 20f, area: 0f);
            Health boss = CreateEnemyHealth(new Vector3(6f, 0f, 1f));
            hotbar.TargetCandidatesProvider = () => new ICombatTarget[] { boss };

            skillManager.LearnSkill(bolt, 1, force: true);
            Assert.IsTrue(hotbar.AssignSlot(0, bolt.SkillId));
            float before = boss.CurrentHealth;

            Assert.IsTrue(hotbar.TryCastSlot(0));

            Assert.Less(boss.CurrentHealth, before, "A cast at a boss 6m away (Range 20) must land.");
            Assert.AreSame(boss, hotbar.LastCastTarget);
            Assert.Greater(Vector3.Dot(hotbar.transform.forward, (boss.transform.position - hotbar.transform.position).normalized), 0.99f,
                "Caster should turn to face the soft target.");
        }

        [Test]
        public void AreaCast_HitsEnemyWhoseColliderIsOnAChild_ExactlyOnce()
        {
            SkillHotbar hotbar = CreateCaster(out SkillManager skillManager);
            SkillDefinition blast = CreateSkill(damage: 25f, range: 10f, area: 3f);
            Health enemy = CreateEnemyHealth(new Vector3(0f, 0f, 4f));

            // Two colliders under one enemy, both on children (mesh + hurtbox style).
            AddChildCollider(enemy.gameObject, Vector3.up);
            AddChildCollider(enemy.gameObject, Vector3.up * 0.5f);
            Physics.SyncTransforms();

            hotbar.TargetCandidatesProvider = () => new ICombatTarget[] { enemy };
            skillManager.LearnSkill(blast, 1, force: true);
            Assert.IsTrue(hotbar.AssignSlot(0, blast.SkillId));

            // No i-frames, so a double hit from the second collider would show up as a second event.
            var so = new SerializedObject(enemy);
            so.FindProperty("_invulnerabilityDuration").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            int hits = 0;
            System.Action<DamageAppliedEvent> onDamage = evt =>
            {
                if (evt.Target == enemy.gameObject)
                {
                    hits++;
                }
            };
            EventBus.Subscribe(onDamage);

            Assert.IsTrue(hotbar.TryCastSlot(0));
            EventBus.Unsubscribe(onDamage);

            Assert.AreEqual(1, hits, "Child colliders must resolve to their owning ICombatTarget, once per cast.");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private SkillHotbar CreateCaster(out SkillManager skillManager)
        {
            var go = new GameObject("SkillTargetingCaster");
            _created.Add(go);
            skillManager = go.AddComponent<SkillManager>();
            return go.AddComponent<SkillHotbar>();
        }

        private Health CreateEnemyHealth(Vector3 position)
        {
            var go = new GameObject("SkillTargetingEnemy");
            _created.Add(go);
            go.transform.position = position;
            return go.AddComponent<Health>();
        }

        private static void AddChildCollider(GameObject parent, Vector3 localPosition)
        {
            var child = new GameObject("Collider");
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPosition;
            child.AddComponent<SphereCollider>().radius = 0.6f;
        }

        private FakeTarget CreateFakeTarget(Vector3 position)
        {
            var go = new GameObject("FakeTarget");
            _created.Add(go);
            go.transform.position = position;
            return new FakeTarget(go.transform);
        }

        private SkillDefinition CreateSkill(float damage, float range, float area)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _created.Add(skill);
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = "skill_targeting_test_" + _created.Count;
            so.FindProperty("_skillType").enumValueIndex = (int)SkillType.Active;
            so.FindProperty("_cooldownSeconds").floatValue = 0f;
            so.FindProperty("_manaCost").floatValue = 0f;
            so.FindProperty("_baseDamage").floatValue = damage;
            so.FindProperty("_range").floatValue = range;
            so.FindProperty("_areaRadius").floatValue = area;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private sealed class FakeTarget : ICombatTarget
        {
            public FakeTarget(Transform transform)
            {
                Transform = transform;
            }

            public bool Alive = true;
            public bool IsAlive => Alive;
            public Transform Transform { get; }

            public void ApplyDamage(float amount, ElementType element, GameObject source)
            {
            }

            public void ApplyHealing(float amount, GameObject source)
            {
            }
        }
    }
}
