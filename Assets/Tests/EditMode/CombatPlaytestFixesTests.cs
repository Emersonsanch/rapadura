using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Encounter;
using Rapadura.UI.Tutorial;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regressions from the 40-fight combat playtest: per-channel enemy hit windows (skills cast right
    /// after a hit used to deal nothing), capped/tiring flee + click-to-move auto-run, forced encounter
    /// aggro, gravity/ground snap for the hero, tutorial hint vs. intro banner, and hero switching
    /// cleaning up the previous hero's skills.
    /// </summary>
    public class CombatPlaytestFixesTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CharacterSkillSet.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            CharacterSkillSet.Invalidate();
            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        // ------------------------------------------------------------------
        // #1 Per-channel hit windows on enemy Health
        // ------------------------------------------------------------------

        [Test]
        public void Health_SkillRightAfterBasicAttack_FromSameAttacker_BothLand()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            GameObject player = NewObject("Player");
            float max = health.CurrentHealth;

            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Physical, player, false);
            OutgoingDamage.DeliverDamage(health, 7f, ElementType.Dark, player, false, DamageChannel.ForSkill("skill_lavine_correntes_das_trevas"));

            Assert.AreEqual(max - 12f, health.CurrentHealth, 0.001f, "the skill must not be swallowed by the basic attack's window");
        }

        [Test]
        public void Health_TwoDifferentSkillsBackToBack_BothLand()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            GameObject player = NewObject("Player");
            float max = health.CurrentHealth;

            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Physical, player, false, DamageChannel.ForSkill("skill_a"));
            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Physical, player, false, DamageChannel.ForSkill("skill_b"));

            Assert.AreEqual(max - 10f, health.CurrentHealth, 0.001f);
        }

        [Test]
        public void Health_SameChannelTwiceWithinWindow_CountsOnce_ThenLandsAfterIt()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            GameObject player = NewObject("Player");
            int channel = DamageChannel.ForSkill("skill_icaro_granada_eletrica");

            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Lightning, player, false, channel);
            float afterFirst = health.CurrentHealth;
            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Lightning, player, false, channel);

            Assert.AreEqual(afterFirst, health.CurrentHealth, "one cast's overlapping checks must not double count");
            Assert.IsTrue(health.IsChannelOnCooldown(player, channel));
            Assert.IsFalse(health.IsInvulnerable, "regular hits no longer make the whole target immune");

            health.TickInvulnerability(0.25f);
            Assert.IsFalse(health.IsChannelOnCooldown(player, channel));
            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Lightning, player, false, channel);
            Assert.Less(health.CurrentHealth, afterFirst);
        }

        [Test]
        public void Health_SameChannel_FromDifferentAttackers_BothLand()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            float max = health.CurrentHealth;

            health.ApplyDamage(5f, ElementType.Physical, NewObject("A"));
            health.ApplyDamage(5f, ElementType.Physical, NewObject("B"));

            Assert.AreEqual(max - 10f, health.CurrentHealth, 0.001f);
        }

        [Test]
        public void Health_GrantInvulnerability_StillBlocksEveryChannel_AndClearResetsAll()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            GameObject player = NewObject("Player");
            float max = health.CurrentHealth;

            health.GrantInvulnerability(1f);
            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Physical, player, false, DamageChannel.ForSkill("x"));
            Assert.AreEqual(max, health.CurrentHealth);

            health.ClearInvulnerability();
            OutgoingDamage.DeliverDamage(health, 5f, ElementType.Physical, player, false);
            health.ClearInvulnerability();
            Assert.IsFalse(health.IsChannelOnCooldown(player, DamageChannel.Default));
        }

        [Test]
        public void Health_ChanneledCritical_IsFlaggedButNotMultipliedAgain()
        {
            Health health = NewObject("Enemy").AddComponent<Health>();
            GameObject player = NewObject("Player");
            DamageAppliedEvent? received = null;
            EventBus.Subscribe<DamageAppliedEvent>(evt => received = evt);

            OutgoingDamage.DeliverDamage(health, 10f, ElementType.Fire, player, true, DamageChannel.ForSkill("crit_skill"));

            Assert.IsTrue(received.HasValue);
            Assert.IsTrue(received.Value.IsCritical);
            Assert.AreEqual(10f, received.Value.FinalDamage, 0.001f, "the crit multiplier was already applied by OutgoingDamage.Resolve");
            Assert.AreSame(player, received.Value.Source);
        }

        [Test]
        public void DamageChannel_IsStable_DistinctPerSkill_AndNeverTheDefault()
        {
            Assert.AreEqual(DamageChannel.ForSkill("skill_a"), DamageChannel.ForSkill("skill_a"));
            Assert.AreNotEqual(DamageChannel.ForSkill("skill_a"), DamageChannel.ForSkill("skill_b"));
            Assert.AreNotEqual(DamageChannel.Default, DamageChannel.ForSkill("skill_a"));
            Assert.AreEqual(DamageChannel.Default, DamageChannel.For(null));
            Assert.AreNotEqual(DamageChannel.ForSkill("skill_a"),
                DamageChannel.ForDamageOverTime(SkillManager.DebuffSourceKey("skill_a"), StatType.HealthRegen),
                "a DoT tick must not share the window of its skill's direct hit");
        }

        [Test]
        public void ActiveStatEffect_KeepsInstigator_AndPrecomputedChannel()
        {
            var definition = new StatModifierDefinition { affectedStat = StatType.HealthRegen, isDebuff = true, durationSeconds = 5f };
            GameObject player = NewObject("Player");
            string key = SkillManager.DebuffSourceKey("skill_icaro_torreta_automatica");

            var effect = new ActiveStatEffect(definition, key, player);

            Assert.AreSame(player, effect.Instigator, "DoT ticks are credited to the caster (kill XP/loot)");
            Assert.AreEqual(DamageChannel.ForDamageOverTime(key, StatType.HealthRegen), effect.DamageChannel);
        }

        // ------------------------------------------------------------------
        // #2 Flee cap / tiring, forced encounter aggro, auto-run
        // ------------------------------------------------------------------

        [Test]
        public void FleeSpeed_IsCappedBelowHeroRun()
        {
            Assert.AreEqual(4.4f, EnemyDetectionUtility.MaxFleeSpeed, 0.0001f);
            Assert.AreEqual(4.4f, EnemyDetectionUtility.GetEffectiveFleeSpeed(6f), 0.0001f, "bats");
            Assert.AreEqual(4.4f, EnemyDetectionUtility.GetEffectiveFleeSpeed(5f), 0.0001f, "wolves");
            Assert.AreEqual(4.0f, EnemyDetectionUtility.GetEffectiveFleeSpeed(4f), 0.0001f, "slower flee speeds are kept");
            Assert.AreEqual(0f, EnemyDetectionUtility.GetEffectiveFleeSpeed(-1f));
            Assert.LessOrEqual(EnemyDetectionUtility.MaxFleeSpeed, 0.8f * 5.5f + 0.0001f);
        }

        [Test]
        public void IsFleeExhausted_AfterMaxDuration()
        {
            Assert.IsFalse(EnemyDetectionUtility.IsFleeExhausted(2.9f, 3f));
            Assert.IsTrue(EnemyDetectionUtility.IsFleeExhausted(3f, 3f));
            Assert.IsFalse(EnemyDetectionUtility.IsFleeExhausted(100f, 0f), "0 = never tires");
        }

        private EnemyDefinition CreateDefinition(float detectionRadius = 20f, float loseTargetRadius = 50f,
            float fleeThreshold = 0f, float fleeSpeed = 6f, float attackRange = 1f)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_detectionRadius", detectionRadius);
            SetPrivateField(definition, "_loseTargetRadius", loseTargetRadius);
            SetPrivateField(definition, "_attackRange", attackRange);
            SetPrivateField(definition, "_attackCooldown", 1f);
            SetPrivateField(definition, "_attackDamage", 1f);
            SetPrivateField(definition, "_fleeHealthThreshold", fleeThreshold);
            SetPrivateField(definition, "_fleeSpeed", fleeSpeed);
            SetPrivateField(definition, "_fleeSafeDistance", 100f);
            SetPrivateField(definition, "_chaseSpeed", 4f);
            SetPrivateField(definition, "_patrolSpeed", 0f);
            return definition;
        }

        private EnemyController NewEnemy(EnemyDefinition definition, Vector3 targetPosition, out Transform target)
        {
            GameObject enemyObject = NewObject("Enemy");
            enemyObject.AddComponent<Health>();
            EnemyController controller = enemyObject.AddComponent<EnemyController>();
            target = NewObject("Target").transform;
            target.position = targetPosition;
            controller.Target = target;
            controller.ResetForSpawn(definition, Vector3.zero);
            return controller;
        }

        [Test]
        public void FleeingEnemy_RunsAtMostTheCap_ThenTiresAndTurnsBack_ForGood()
        {
            EnemyController enemy = NewEnemy(CreateDefinition(fleeThreshold: 0.2f, fleeSpeed: 6f), new Vector3(2f, 0f, 0f), out _);
            enemy.Health.ApplyDamage(90f, ElementType.Physical, null);

            enemy.TickBrain(0.1f); // patrol -> chase
            enemy.TickBrain(0.1f); // chase -> flee
            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyFleeState>());

            Vector3 start = enemy.transform.position;
            for (int i = 0; i < 10; i++)
            {
                enemy.TickBrain(0.1f);
            }

            float movedInOneSecond = Vector3.Distance(start, enemy.transform.position);
            Assert.Greater(movedInOneSecond, 0f);
            Assert.LessOrEqual(movedInOneSecond, EnemyDetectionUtility.MaxFleeSpeed + 0.01f, "flee speed 6 must be capped at 4.4");

            for (int i = 0; i < 25; i++)
            {
                enemy.TickBrain(0.1f);
            }

            Assert.IsFalse(enemy.StateMachine.IsInState<EnemyFleeState>(), "tired after 3 s");
            Assert.IsFalse(enemy.CanFlee);

            for (int i = 0; i < 20; i++)
            {
                enemy.TickBrain(0.1f);
                Assert.IsFalse(enemy.StateMachine.IsInState<EnemyFleeState>(), "a spent flee never restarts this life");
            }

            enemy.ResetForSpawn(enemy.Definition, Vector3.zero);
            Assert.IsTrue(enemy.CanFlee, "a re-spawn can flee again");
        }

        [Test]
        public void EncounterHold_WithForcedAggro_EngagesBeyondDetection_AndNeverLosesTheTarget()
        {
            // Spider: 7 m detection, 9 m lose radius, spawned 8.5 m away.
            EnemyController enemy = NewEnemy(CreateDefinition(detectionRadius: 7f, loseTargetRadius: 9f), new Vector3(0f, 0f, 8.5f), out Transform target);

            enemy.TickBrain(0.1f);
            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyPatrolState>(), "precondition: outside detection it would never aggro");

            enemy.HoldAggro(1f, forceAggroWhenHoldEnds: true);
            for (int i = 0; i < 9; i++)
            {
                enemy.TickBrain(0.1f);
            }

            Assert.IsFalse(enemy.IsAggroLocked, "still holding during the intro");
            enemy.TickBrain(0.15f);

            Assert.IsTrue(enemy.IsAggroLocked);
            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyChaseState>());

            target.position = new Vector3(0f, 0f, 15f); // beyond the 9 m lose radius
            enemy.TickBrain(0.1f);
            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyChaseState>(), "an encounter enemy keeps chasing");

            enemy.ResetForSpawn(enemy.Definition, Vector3.zero);
            Assert.IsFalse(enemy.IsAggroLocked);
        }

        [Test]
        public void ForcedAggro_ZeroGrace_OrReleaseAggro_EngagesAtOnce()
        {
            EnemyController immediate = NewEnemy(CreateDefinition(detectionRadius: 7f), new Vector3(0f, 0f, 9f), out _);
            immediate.HoldAggro(0f, forceAggroWhenHoldEnds: true);
            Assert.IsTrue(immediate.IsAggroLocked);
            Assert.IsTrue(immediate.StateMachine.IsInState<EnemyChaseState>());

            EnemyController released = NewEnemy(CreateDefinition(detectionRadius: 7f), new Vector3(0f, 0f, 9f), out _);
            released.HoldAggro(5f, forceAggroWhenHoldEnds: true);
            released.ReleaseAggro();
            Assert.IsFalse(released.IsHoldingAggro);
            Assert.IsTrue(released.IsAggroLocked);

            EnemyController plain = NewEnemy(CreateDefinition(detectionRadius: 7f), new Vector3(0f, 0f, 9f), out _);
            plain.HoldAggro(5f);
            plain.ReleaseAggro();
            Assert.IsFalse(plain.IsAggroLocked, "a plain hold keeps the old behaviour");
        }

        [Test]
        public void ClickToMove_AutoRunsOnlyWhenTheTargetIsFar()
        {
            Assert.AreEqual(4f, PlayerClickToMove.DefaultAutoRunDistance);
            Assert.IsTrue(PlayerClickToMove.ShouldAutoRun(6f, 4f));
            Assert.IsFalse(PlayerClickToMove.ShouldAutoRun(3f, 4f));
            Assert.IsFalse(PlayerClickToMove.ShouldAutoRun(4f, 4f));
            Assert.IsFalse(PlayerClickToMove.ShouldAutoRun(50f, 0f), "0 disables auto-run");
        }

        // ------------------------------------------------------------------
        // #3 Ground snap / gravity-only step
        // ------------------------------------------------------------------

        private PlayerMotor NewPlayerAboveGround(float height, out GameObject ground)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _created.Add(ground);
            ground.transform.localScale = new Vector3(20f, 1f, 20f); // top at y = 0.5
            GameObject player = NewObject("Player");
            player.transform.position = new Vector3(0f, height, 0f);
            return player.AddComponent<PlayerMotor>();
        }

        [Test]
        public void SnapToGround_PutsAHoveringHeroOnTheFloor()
        {
            PlayerMotor motor = NewPlayerAboveGround(0.5f + 1.08f, out _);

            Assert.IsTrue(motor.SnapToGround(~0));

            Assert.AreEqual(0.5f, motor.transform.position.y, 0.01f);
            Assert.IsTrue(motor.IsGrounded);
        }

        [Test]
        public void SnapToGround_NothingBelow_LeavesThePlayerAlone()
        {
            PlayerMotor motor = NewPlayerAboveGround(3f, out GameObject ground);
            ground.transform.position = new Vector3(500f, 0f, 0f);
            // Probe far from the world origin: editor-only DontSave objects (e.g. the
            // Rapadura/Cities "Preview Walkable Town" menu, whose town ground spans the origin)
            // survive the Test Runner's fresh scene and would be "something below".
            motor.transform.position = new Vector3(-9000f, 3f, 9000f);
            Vector3 before = motor.transform.position;

            Assert.IsFalse(motor.SnapToGround(~0));
            Assert.AreEqual(before, motor.transform.position);
        }

        [Test]
        public void ApplyGravityOnly_RefreshesAStaleGroundedFlag()
        {
            PlayerMotor motor = NewPlayerAboveGround(0.5f, out _);
            motor.SnapToGround(~0);
            Assert.IsTrue(motor.IsGrounded, "precondition");

            // Teleported up (arena spawn above the floor): idle used to keep IsGrounded = true forever.
            motor.Teleport(new Vector3(0f, 3f, 0f), Quaternion.identity);
            motor.ApplyGravityOnly();

            Assert.IsFalse(motor.IsGrounded, "the idle step must notice it is airborne so it falls");
        }

        // ------------------------------------------------------------------
        // #4 Tutorial hint waits for the intro banner
        // ------------------------------------------------------------------

        [Test]
        public void TutorialHint_DefersWhileTheIntroBannerShows()
        {
            Assert.IsTrue(TutorialHintController.ShouldDefer(false, introBannerShowing: true));
            Assert.IsTrue(TutorialHintController.ShouldDefer(true, introBannerShowing: false));
            Assert.IsFalse(TutorialHintController.ShouldDefer(false, introBannerShowing: false));
            Assert.IsFalse(TutorialHintController.ShouldDefer(false), "old single-argument call keeps its meaning");
        }

        [Test]
        public void IntroBanner_IsActive_ForCountdownOrBoss()
        {
            Assert.IsTrue(BossIntroBannerController.IsActive(showingPrepare: true, showingBoss: false));
            Assert.IsTrue(BossIntroBannerController.IsActive(showingPrepare: false, showingBoss: true));
            Assert.IsFalse(BossIntroBannerController.IsActive(false, false));
            Assert.IsFalse(BossIntroBannerController.IsAnyShowing, "no banner instance in EditMode");
        }

        // ------------------------------------------------------------------
        // #5 Hero switch drops the previous hero's skills
        // ------------------------------------------------------------------

        [Test]
        public void GrantTo_NewHero_RevokesThePreviousHerosSkills_AndResetsTheHotbar()
        {
            GameObject caster = NewObject("Caster");
            SkillManager skillManager = caster.AddComponent<SkillManager>();
            SkillHotbar hotbar = caster.AddComponent<SkillHotbar>();
            IReadOnlyList<SkillDefinition> icaro = CharacterSkillSet.GetSkills(CharacterId.Icaro);
            IReadOnlyList<SkillDefinition> lavine = CharacterSkillSet.GetSkills(CharacterId.Lavine);

            CharacterSkillSet.GrantTo(skillManager, CharacterId.Icaro, casterLevel: 1);
            skillManager.GrantSkillPoints(1);
            Assert.IsTrue(skillManager.LevelUpSkillWithPoint(icaro[0].SkillId));
            Assert.AreEqual(0, skillManager.AvailableSkillPoints);

            int learned = CharacterSkillSet.GrantTo(skillManager, CharacterId.Lavine, casterLevel: 1);

            Assert.AreEqual(4, learned);
            foreach (SkillDefinition skill in icaro)
            {
                Assert.IsFalse(skillManager.IsSkillLearned(skill.SkillId), skill.SkillId);
                Assert.IsFalse(skillManager.IsSkillRegistered(skill.SkillId), skill.SkillId);
            }

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(lavine[i], skillManager.EquippedSlots[i].Definition, $"slot {i}");
            }

            for (int i = 4; i < skillManager.EquippedSlots.Count; i++)
            {
                Assert.IsNull(skillManager.EquippedSlots[i], $"slot {i}");
            }

            for (int i = 0; i < hotbar.Loadout.SlotCount; i++)
            {
                string id = hotbar.Loadout.GetSkillId(i);
                if (i < 4)
                {
                    Assert.AreEqual(lavine[i].SkillId, id, $"hotbar slot {i}");
                }
                else
                {
                    Assert.IsTrue(string.IsNullOrEmpty(id), $"hotbar slot {i} still holds {id}");
                }
            }

            Assert.AreEqual(1, skillManager.AvailableSkillPoints, "the point spent on the old hero's skill level comes back; free starters don't mint points");
        }

        [Test]
        public void ForgetSkill_RefundsExactly_AndUnregisterKeepsSerializedSkills()
        {
            SkillManager skillManager = NewObject("Caster").AddComponent<SkillManager>();
            SkillDefinition skill = CharacterSkillSet.GetSkills(CharacterId.Maria)[0];
            skillManager.RegisterSkills(new[] { skill });
            skillManager.LearnSkill(skill, 1);
            skillManager.EquipSkill(0, skill.SkillId);

            Assert.IsTrue(skillManager.ForgetSkill(skill.SkillId, 0));
            Assert.IsFalse(skillManager.IsSkillLearned(skill.SkillId));
            Assert.IsNull(skillManager.EquippedSlots[0]);
            Assert.AreEqual(0, skillManager.AvailableSkillPoints);
            Assert.IsFalse(skillManager.ForgetSkill(skill.SkillId, 0), "already forgotten");

            skillManager.UnregisterSkills(new[] { skill });
            Assert.IsFalse(skillManager.IsSkillRegistered(skill.SkillId));
        }

        [Test]
        public void IsStarterSkill_ActivesYes_UltimateNo()
        {
            IReadOnlyList<SkillDefinition> skills = CharacterSkillSet.GetSkills(CharacterId.Joaquim);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(CharacterSkillSet.IsStarterSkill(skills[i]), skills[i].SkillId);
            }

            Assert.IsFalse(CharacterSkillSet.IsStarterSkill(skills[4]));
            Assert.IsFalse(CharacterSkillSet.IsStarterSkill(null));
        }
    }
}
