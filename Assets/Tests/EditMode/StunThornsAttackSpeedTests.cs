using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.CharacterSheet;
using Rapadura.UI.Common;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Combat gaps closed on 2026-10-29: skills that stun (and enemies that honour it, bosses at half
    /// duration), <see cref="StatType.AttackSpeed"/> on the basic-attack cooldown,
    /// <see cref="StatType.Thorns"/> reflecting melee damage without recursion, and the removal of the
    /// never-used <c>EnemyDefinition._contactDamage</c>.
    /// </summary>
    public class StunThornsAttackSpeedTests
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

        // ------------------------------------------------------------------
        // Stun: rules
        // ------------------------------------------------------------------

        [Test]
        public void StunRules_ScaleDuration_HalvesForBosses_AndIgnoresNonPositive()
        {
            Assert.AreEqual(1f, StunRules.ScaleDuration(1f, StunRules.MultiplierFor(isBoss: false)), 1e-5f);
            Assert.AreEqual(0.5f, StunRules.ScaleDuration(1f, StunRules.MultiplierFor(isBoss: true)), 1e-5f);
            Assert.AreEqual(0f, StunRules.ScaleDuration(0f, 1f));
            Assert.AreEqual(0f, StunRules.ScaleDuration(-1f, 1f));
            Assert.AreEqual(0f, StunRules.ScaleDuration(1f, 0f), "multiplier 0 = immune");
        }

        [Test]
        public void ApplyStun_OnBoss_IsHalved_OnRegularEnemy_IsFull()
        {
            (EnemyController regular, BuffController regularBuffs, _) = CreateEnemy(isBoss: false);
            (EnemyController boss, BuffController bossBuffs, _) = CreateEnemy(isBoss: true);

            Assert.AreEqual(1f, regularBuffs.ApplyStun(1f), 1e-5f);
            Assert.AreEqual(1f, regularBuffs.StunTimeRemaining, 1e-5f);
            Assert.AreEqual(0.5f, bossBuffs.ApplyStun(1f), 1e-5f);
            Assert.AreEqual(0.5f, bossBuffs.StunTimeRemaining, 1e-5f);
            Assert.IsTrue(regular.IsStunned);
            Assert.IsTrue(boss.IsStunned);
        }

        [Test]
        public void ApplyStun_PublishesTheScaledDuration_ForTheVisualCue()
        {
            (_, BuffController bossBuffs, _) = CreateEnemy(isBoss: true);
            var events = new List<StunAppliedEvent>();
            System.Action<StunAppliedEvent> onStun = evt => events.Add(evt);
            EventBus.Subscribe(onStun);

            bossBuffs.ApplyStun(0.8f);
            bossBuffs.ApplyStun(0f);
            EventBus.Unsubscribe(onStun);

            Assert.AreEqual(1, events.Count, "a zero stun shows no cue");
            Assert.AreSame(bossBuffs.gameObject, events[0].Target);
            Assert.AreEqual(0.4f, events[0].DurationSeconds, 1e-5f);
        }

        [Test]
        public void TickStun_CountsDown_AndEnds()
        {
            (_, BuffController buffs, _) = CreateEnemy(isBoss: false);
            buffs.ApplyStun(0.5f);

            buffs.TickStun(0.3f);
            Assert.IsTrue(buffs.IsStunned);
            buffs.TickStun(0.3f);
            Assert.IsFalse(buffs.IsStunned);
            Assert.AreEqual(0f, buffs.StunTimeRemaining);
        }

        // ------------------------------------------------------------------
        // Stun: enemy AI
        // ------------------------------------------------------------------

        [Test]
        public void StunnedEnemy_InAttackRange_NeitherMovesNorAttacks_ThenResumes()
        {
            (EnemyController enemy, BuffController buffs, _) = CreateEnemy(isBoss: false);
            Health target = CreateTarget(enemy, new Vector3(1f, 0f, 0f));
            float before = target.CurrentHealth;

            buffs.ApplyStun(2f);
            for (int i = 0; i < 19; i++)
            {
                enemy.TickBrain(0.1f);
                buffs.TickStun(0.1f);
            }

            Assert.IsTrue(enemy.IsStunned);
            Assert.AreEqual(before, target.CurrentHealth, "no attack while stunned");
            Assert.AreEqual(Vector3.zero, enemy.transform.position, "no movement while stunned");
            Assert.IsFalse(enemy.StateMachine.IsInState<EnemyAttackState>(), "no attack stance while stunned");

            buffs.TickStun(0.2f);
            Assert.IsFalse(enemy.IsStunned);
            for (int i = 0; i < 10; i++) // patrol -> chase -> attack, then the telegraphed windup
            {
                enemy.TickBrain(0.1f);
            }

            Assert.Less(target.CurrentHealth, before, "attacks resume once the stun ends");
        }

        [Test]
        public void StunDuringWindup_CancelsTheAttack_AndTheNextOneTelegraphsFromScratch()
        {
            (EnemyController enemy, BuffController buffs, _) = CreateEnemy(isBoss: false);
            Health target = CreateTarget(enemy, new Vector3(1f, 0f, 0f));
            float before = target.CurrentHealth;

            for (int i = 0; i < 20 && !enemy.IsWindingUp; i++)
            {
                enemy.TickBrain(0.05f);
            }

            Assert.IsTrue(enemy.IsWindingUp, "setup: the enemy must be mid-windup");
            Assert.AreEqual(before, target.CurrentHealth, "setup: the strike has not landed yet");
            int cancelsBefore = enemy.AttackCancelSequence;

            buffs.ApplyStun(0.5f);
            enemy.TickBrain(0.05f);

            Assert.IsFalse(enemy.IsWindingUp);
            Assert.AreEqual(cancelsBefore + 1, enemy.AttackCancelSequence, "the animator is told the swing was cancelled");
            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyChaseState>());

            buffs.ClearStun();
            // Chase -> Attack (Enter re-arms the full windup), then less than one windup of time.
            float windup = enemy.ResolvedAttackWindup;
            enemy.TickBrain(0.05f);
            enemy.TickBrain(windup * 0.5f);

            Assert.AreEqual(before, target.CurrentHealth, "no instant strike with the time left over from the cancelled windup");
        }

        [Test]
        public void SkillWithStun_StunsTheTargetHit_BossForHalf()
        {
            SkillDefinition skill = CreateSkill(damage: 5f, stunSeconds: 1f);
            (_, BuffController regularBuffs, Health regularHealth) = CreateEnemy(isBoss: false);
            (_, BuffController bossBuffs, Health bossHealth) = CreateEnemy(isBoss: true);

            CastAt(skill, regularHealth);
            CastAt(skill, bossHealth);

            Assert.AreEqual(1f, regularBuffs.StunTimeRemaining, 1e-5f);
            Assert.AreEqual(0.5f, bossBuffs.StunTimeRemaining, 1e-5f);
        }

        [Test]
        public void SkillWithStun_KillingBlow_DoesNotStunTheCorpse()
        {
            SkillDefinition skill = CreateSkill(damage: 100000f, stunSeconds: 1f);
            (_, BuffController buffs, Health health) = CreateEnemy(isBoss: false);

            CastAt(skill, health);

            Assert.IsTrue(health.IsDead);
            Assert.IsFalse(buffs.IsStunned);
        }

        [TestCase("Characters/Lavine/skill_lavine_correntes_das_trevas")]
        [TestCase("Characters/Icaro/skill_icaro_granada_eletrica")]
        [TestCase("Characters/Joaquim/skill_joaquim_terremoto")]
        public void ShippedCrowdControlSkills_Stun_Briefly(string path)
        {
            var skill = Resources.Load<SkillDefinition>("Skills/" + path);
            Assert.IsNotNull(skill, path);
            Assert.Greater(skill.StunDurationSeconds, 0f);
            Assert.LessOrEqual(skill.StunDurationSeconds, 1f, "short stuns only: the skills' cooldowns are 8-12 s");
            Assert.Less(skill.StunDurationSeconds, skill.CooldownSeconds);
        }

        // ------------------------------------------------------------------
        // Attack speed
        // ------------------------------------------------------------------

        [Test]
        public void ResolveCooldown_DividesByOnePlusBonus_ClampsTheBonus_AndFloorsAtTheStrikeDelay()
        {
            Assert.AreEqual(0.5f, PlayerBasicAttack.ResolveCooldown(0.5f, 0f, 0.22f), 1e-5f);
            Assert.AreEqual(0.4f, PlayerBasicAttack.ResolveCooldown(0.5f, 0.25f, 0.22f), 1e-5f);
            Assert.AreEqual(0.25f, PlayerBasicAttack.ResolveCooldown(0.5f, 1f, 0f), 1e-5f);
            Assert.AreEqual(0.25f, PlayerBasicAttack.ResolveCooldown(0.5f, 5f, 0f), 1e-5f, "bonus capped at +100%");
            Assert.AreEqual(1f, PlayerBasicAttack.ResolveCooldown(0.5f, -0.9f, 0f), 1e-5f, "malus capped at -50%");
            Assert.AreEqual(0.3f, PlayerBasicAttack.ResolveCooldown(0.5f, 1f, 0.3f), 1e-5f, "never faster than the swing lands");
        }

        [Test]
        public void BasicAttack_AttackSpeedModifier_ShortensTheArmedCooldown()
        {
            PlayerBasicAttack attack = CreateAttacker(out BuffController buffs);
            buffs.ApplyEffect(Modifier(StatType.AttackSpeed, 0.25f), "test_affix");

            attack.PerformAttackWithCooldownForTests();

            Assert.AreEqual(0.25f, attack.AttackSpeedBonus, 1e-5f);
            Assert.AreEqual(attack.BaseCooldown / 1.25f, attack.CooldownRemaining, 1e-5f);
        }

        [Test]
        public void BasicAttack_VeryFastSwing_NeverStartsBeforeThePendingStrikeLands()
        {
            PlayerBasicAttack attack = CreateAttacker(out BuffController buffs);
            var so = new SerializedObject(attack);
            so.FindProperty("_cooldownSeconds").floatValue = 0.3f; // /2 = 0.15 < 0.22 strike delay
            so.ApplyModifiedPropertiesWithoutUndo();
            buffs.ApplyEffect(Modifier(StatType.AttackSpeed, 1f), "test_affix");

            attack.PerformAttackWithCooldownForTests();

            Assert.IsTrue(attack.IsStrikePending);
            Assert.AreEqual(HeroActionTiming.StrikeDelay, attack.CooldownRemaining, 1e-5f);
        }

        // ------------------------------------------------------------------
        // Thorns
        // ------------------------------------------------------------------

        [Test]
        public void ComputeReflected_IsAShareOfTheDamageTaken_CappedAtHalf()
        {
            Assert.AreEqual(10f, ThornsRules.ComputeReflected(50f, 0.2f), 1e-4f);
            Assert.AreEqual(25f, ThornsRules.ComputeReflected(50f, 3f), 1e-4f, "capped at MaxFraction");
            Assert.AreEqual(0f, ThornsRules.ComputeReflected(0f, 0.2f));
            Assert.AreEqual(0f, ThornsRules.ComputeReflected(50f, -0.2f));
        }

        [Test]
        public void EnemyMeleeHit_OnHeroWithThorns_ReflectsTheShareOfDamageTaken()
        {
            (EnemyController enemy, _, Health enemyHealth) = CreateEnemy(isBoss: false, attackDamage: 50f);
            (PlayerStats hero, BuffController heroBuffs) = CreateHero();
            heroBuffs.ApplyEffect(Modifier(StatType.Thorns, 0.2f), "test_affix");
            enemy.Target = hero.transform;
            float heroBefore = hero.CurrentHealth;
            float enemyBefore = enemyHealth.CurrentHealth;

            enemy.ApplyDamageToTarget(50f);

            float taken = heroBefore - hero.CurrentHealth;
            Assert.Greater(taken, 0f);
            Assert.AreEqual(taken * 0.2f, enemyBefore - enemyHealth.CurrentHealth, 1e-3f);
        }

        [Test]
        public void NonMeleeDamage_DoesNotTriggerThorns()
        {
            (_, _, Health enemyHealth) = CreateEnemy(isBoss: false);
            (PlayerStats hero, BuffController heroBuffs) = CreateHero();
            heroBuffs.ApplyEffect(Modifier(StatType.Thorns, 0.5f), "test_affix");
            float enemyBefore = enemyHealth.CurrentHealth;

            ((ICombatTarget)hero).ApplyDamage(40f, ElementType.Physical, enemyHealth.gameObject);

            Assert.Less(hero.CurrentHealth, hero.MaxHealth);
            Assert.AreEqual(enemyBefore, enemyHealth.CurrentHealth);
        }

        [Test]
        public void ReflectedDamage_NeverReflectsAgain_EvenBetweenTwoThornsUsers()
        {
            (PlayerStats hero, BuffController heroBuffs) = CreateHero();
            heroBuffs.ApplyEffect(Modifier(StatType.Thorns, 0.5f), "test_affix");

            // The attacker has thorns too. A test-only MonoBehaviour can't be attached from this
            // Editor-only assembly, so the worst case (the attacker reflecting the reflection) is
            // simulated by calling TryReflect from inside the reflected hit's damage event.
            (_, BuffController enemyBuffs, Health enemyHealth) = CreateEnemy(isBoss: false);
            enemyBuffs.ApplyEffect(Modifier(StatType.Thorns, 0.5f), "test_affix");
            float enemyBefore = enemyHealth.CurrentHealth;

            int heroHits = 0;
            int enemyHits = 0;
            float nestedReflection = -1f;
            System.Action<DamageAppliedEvent> onDamage = evt =>
            {
                if (evt.Target == hero.gameObject)
                {
                    heroHits++;
                }
                else if (evt.Target == enemyHealth.gameObject)
                {
                    enemyHits++;
                    nestedReflection = ThornsRules.TryReflect(enemyHealth.gameObject, hero.gameObject, evt.FinalDamage);
                }
            };
            EventBus.Subscribe(onDamage);

            hero.ApplyMeleeDamage(40f, ElementType.Physical, enemyHealth.gameObject);
            EventBus.Unsubscribe(onDamage);

            Assert.AreEqual(1, enemyHits, "the attacker gets exactly one reflection");
            Assert.AreEqual(20f, enemyBefore - enemyHealth.CurrentHealth, 1e-3f);
            Assert.AreEqual(0f, nestedReflection, "its own thorns were suppressed by the guard");
            Assert.AreEqual(1, heroHits, "the hero is hit once (the original blow), never by a counter-reflection");
            Assert.IsFalse(ThornsRules.IsReflecting);
        }

        [Test]
        public void TryReflect_SelfOrDeadOrMissingAttacker_DoesNothing()
        {
            (PlayerStats hero, BuffController heroBuffs) = CreateHero();
            heroBuffs.ApplyEffect(Modifier(StatType.Thorns, 0.5f), "test_affix");
            (_, _, Health enemyHealth) = CreateEnemy(isBoss: false);
            enemyHealth.ApplyDamage(1_000_000f, ElementType.Physical, null);

            Assert.AreEqual(0f, ThornsRules.TryReflect(hero.gameObject, hero.gameObject, 10f));
            Assert.AreEqual(0f, ThornsRules.TryReflect(hero.gameObject, null, 10f));
            Assert.AreEqual(0f, ThornsRules.TryReflect(hero.gameObject, enemyHealth.gameObject, 10f), "dead attacker");
        }

        // ------------------------------------------------------------------
        // UI surfacing (tooltip + character sheet)
        // ------------------------------------------------------------------

        [Test]
        public void Tooltip_ShowsStun_AndTheNewStatsAsPercent()
        {
            SkillDefinition skill = CreateSkill(damage: 5f, stunSeconds: 1f);

            TooltipContent content = TooltipContentBuilder.ForSkill(skill, currentLevel: 1);

            Assert.IsTrue(content.StatLines.Exists(line => line.StartsWith("Atordoa por 1", System.StringComparison.Ordinal)),
                string.Join(" | ", content.StatLines));
            Assert.AreEqual("+10% Velocidade de ataque", TooltipContentBuilder.FormatModifier(StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.1f));
            Assert.AreEqual("+15% Espinhos (dano refletido)", TooltipContentBuilder.FormatModifier(StatType.Thorns, ModifierApplication.PercentAdditive, 0.15f));
        }

        [Test]
        public void CharacterSheet_DerivedStats_ReadAttackSpeedAndClampedThorns()
        {
            (PlayerStats hero, BuffController buffs) = CreateHero();
            buffs.ApplyEffect(Modifier(StatType.AttackSpeed, 0.1f), "a");
            buffs.ApplyEffect(Modifier(StatType.Thorns, 0.8f), "b");

            CharacterDerivedStats stats = CharacterSheetPresenter.BuildDerivedStats(hero, buffs, null);

            Assert.AreEqual(0.1f, stats.AttackSpeedBonus, 1e-5f);
            Assert.AreEqual(ThornsRules.MaxFraction, stats.Thorns, 1e-5f);
        }

        // ------------------------------------------------------------------
        // Contact damage (removed)
        // ------------------------------------------------------------------

        [Test]
        public void EnemyDefinition_HasNoContactDamage_AndTheBalanceSheetDropsTheColumn()
        {
            Assert.IsNull(typeof(EnemyDefinition).GetField("_contactDamage", BindingFlags.NonPublic | BindingFlags.Instance),
                "contact damage was removed: enemy damage is only the telegraphed attack");
            Assert.IsNull(typeof(EnemyDefinition).GetProperty("ContactDamage"));

            string csv = Path.Combine(Application.dataPath, "..", "Docs", "Balance", "enemies.csv");
            Assert.IsTrue(File.Exists(csv), csv);
            string header = File.ReadAllLines(csv)[0];
            StringAssert.DoesNotContain("contact_damage", header);
            StringAssert.Contains("attack_damage", header);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private (EnemyController enemy, BuffController buffs, Health health) CreateEnemy(bool isBoss, float attackDamage = 15f)
        {
            var go = new GameObject(isBoss ? "StunBoss" : "StunEnemy");
            _created.Add(go);
            Health health = go.AddComponent<Health>();
            BuffController buffs = go.AddComponent<BuffController>();
            EnemyController enemy = go.AddComponent<EnemyController>();

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_isBoss", isBoss);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_detectionRadius", 20f);
            SetPrivateField(definition, "_loseTargetRadius", 50f);
            SetPrivateField(definition, "_attackRange", 5f);
            SetPrivateField(definition, "_attackCooldown", 1f);
            SetPrivateField(definition, "_attackDamage", attackDamage);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            SetPrivateField(definition, "_chaseSpeed", 4f);
            enemy.ResetForSpawn(definition, Vector3.zero);
            return (enemy, buffs, health);
        }

        private Health CreateTarget(EnemyController enemy, Vector3 position)
        {
            var go = new GameObject("StunTarget");
            _created.Add(go);
            go.transform.position = position;
            Health health = go.AddComponent<Health>();
            enemy.Target = go.transform;
            return health;
        }

        private (PlayerStats stats, BuffController buffs) CreateHero()
        {
            var go = new GameObject("ThornsHero");
            _created.Add(go);
            PlayerStats stats = go.AddComponent<PlayerStats>();
            BuffController buffs = go.AddComponent<BuffController>();
            return (stats, buffs);
        }

        private PlayerBasicAttack CreateAttacker(out BuffController buffs)
        {
            var go = new GameObject("AttackSpeedHero");
            _created.Add(go);
            buffs = go.AddComponent<BuffController>();
            return go.AddComponent<PlayerBasicAttack>();
        }

        private SkillDefinition CreateSkill(float damage, float stunSeconds)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _created.Add(skill);
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = "skill_stun_test_" + _created.Count;
            so.FindProperty("_skillType").enumValueIndex = (int)SkillType.Active;
            so.FindProperty("_cooldownSeconds").floatValue = 0f;
            so.FindProperty("_manaCost").floatValue = 0f;
            so.FindProperty("_baseDamage").floatValue = damage;
            so.FindProperty("_range").floatValue = 20f;
            so.FindProperty("_areaRadius").floatValue = 0f;
            so.FindProperty("_stunDurationSeconds").floatValue = stunSeconds;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }

        private void CastAt(SkillDefinition skill, Health target)
        {
            var go = new GameObject("StunCaster");
            _created.Add(go);
            go.transform.position = target.transform.position + new Vector3(-3f, 0f, 0f);
            SkillManager skillManager = go.AddComponent<SkillManager>();
            SkillHotbar hotbar = go.AddComponent<SkillHotbar>();
            hotbar.TargetCandidatesProvider = () => new ICombatTarget[] { target };
            skillManager.LearnSkill(skill, 1, force: true);
            Assert.IsTrue(hotbar.AssignSlot(0, skill.SkillId));
            Assert.IsTrue(hotbar.TryCastSlot(0));
        }

        private static StatModifierDefinition Modifier(StatType stat, float value)
        {
            return new StatModifierDefinition
            {
                displayName = stat.ToString(),
                affectedStat = stat,
                application = ModifierApplication.PercentAdditive,
                value = value,
                durationSeconds = 0f,
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
