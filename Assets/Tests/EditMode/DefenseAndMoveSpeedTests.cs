using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression tests for the "dead Defense/MoveSpeed" bug (Docs/Balance/README.md §4): ~20 skills
    /// applied Defense and MoveSpeed modifiers that nothing read, and enemies had no BuffController at
    /// all. These assert gameplay outcomes (health lost, distance moved, swing damage) for both the
    /// player and enemies, using the shipped skill assets where it matters.
    /// </summary>
    public class DefenseAndMoveSpeedTests
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

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
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

        private static StatModifierDefinition Effect(StatType stat, ModifierApplication application, float value, bool isDebuff = false)
        {
            return new StatModifierDefinition
            {
                displayName = "Test",
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = 10f,
                isDebuff = isDebuff
            };
        }

        private static SkillDefinition LoadSkill(string path)
        {
            SkillDefinition skill = Resources.Load<SkillDefinition>("Skills/" + path);
            Assert.IsNotNull(skill, "Expected skill asset Resources/Skills/" + path);
            return skill;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private (PlayerStats stats, BuffController buffs) NewPlayer()
        {
            GameObject go = NewObject("DefensePlayer");
            PlayerStats stats = go.AddComponent<PlayerStats>();
            BuffController buffs = go.AddComponent<BuffController>();
            return (stats, buffs);
        }

        private (Health health, BuffController buffs) NewEnemyHealth(float maxHealth = 1000f)
        {
            GameObject go = NewObject("DefenseEnemy");
            Health health = go.AddComponent<Health>();
            BuffController buffs = go.AddComponent<BuffController>();
            health.ConfigureMaxHealth(maxHealth);
            return (health, buffs);
        }

        private static float DamageTaken(PlayerStats stats, float amount)
        {
            stats.ClearInvulnerability();
            float before = stats.CurrentHealth;
            ((ICombatTarget)stats).ApplyDamage(amount, ElementType.Physical, null);
            return before - stats.CurrentHealth;
        }

        private static float DamageTaken(Health health, float amount)
        {
            health.ClearInvulnerability();
            float before = health.CurrentHealth;
            health.ApplyDamage(amount, ElementType.Physical, null);
            return before - health.CurrentHealth;
        }

        // ------------------------------------------------------------------
        // Formula
        // ------------------------------------------------------------------

        [Test]
        public void DefenseMultiplier_IsOneWithoutDefense()
        {
            Assert.AreEqual(1f, DamageCalculator.GetDefenseMultiplier(0f, 0f, null));
        }

        [Test]
        public void DefenseMultiplier_PercentBuffReducesAndDebuffIncreases()
        {
            Assert.AreEqual(0.75f, DamageCalculator.GetDefenseMultiplier(0f, 0.25f, null), 0.0001f);
            Assert.AreEqual(1.2f, DamageCalculator.GetDefenseMultiplier(0f, -0.2f, null), 0.0001f);
        }

        [Test]
        public void DefenseMultiplier_FlatArmorUsesTheArmorCurve_AndStacksWithPercent()
        {
            Assert.AreEqual(0.5f, DamageCalculator.GetDefenseMultiplier(100f, 0f, null), 0.0001f);
            Assert.AreEqual(0.25f, DamageCalculator.GetDefenseMultiplier(100f, 0.5f, null), 0.0001f);
            Assert.AreEqual(1f, DamageCalculator.GetDefenseMultiplier(-50f, 0f, null), 0.0001f, "negative armor is ignored");
        }

        [Test]
        public void DefenseMultiplier_ClampsStackedPercent()
        {
            Assert.AreEqual(1f - DamageCalculator.MaxPercentDefense, DamageCalculator.GetDefenseMultiplier(0f, 5f, null), 0.0001f);
            Assert.AreEqual(1f - DamageCalculator.MinPercentDefense, DamageCalculator.GetDefenseMultiplier(0f, -5f, null), 0.0001f);
            Assert.AreEqual(2f, DamageCalculator.GetDefenseMultiplier(0f, -5f, null), 0.0001f, "debuffs at most double the damage");
        }

        [Test]
        public void ComputeDamage_AppliesPercentDefense()
        {
            DamageInfo info = DamageInfo.Simple(100f, ElementType.Physical, null);

            Assert.AreEqual(75f, DamageCalculator.ComputeDamage(info, 0f, 0.25f, null, null));
            Assert.AreEqual(130f, DamageCalculator.ComputeDamage(info, 0f, -0.3f, null, null));
            Assert.AreEqual(100f, DamageCalculator.ComputeDamage(info, 0f, null), "old overload unchanged");
        }

        [Test]
        public void EverySkillExpressesDefenseAndMoveSpeedAsPercent()
        {
            // The formula reads skill Defense as a percent change of damage taken and MoveSpeed as a
            // percent of base speed; a Flat one on a skill would silently mean something else.
            SkillDefinition[] skills = Resources.LoadAll<SkillDefinition>("Skills");
            Assert.IsNotEmpty(skills);

            foreach (SkillDefinition skill in skills)
            {
                foreach (StatModifierDefinition effect in skill.Buffs)
                {
                    AssertPercent(skill, effect);
                }

                foreach (StatModifierDefinition effect in skill.Debuffs)
                {
                    AssertPercent(skill, effect);
                }
            }
        }

        private static void AssertPercent(SkillDefinition skill, StatModifierDefinition effect)
        {
            if (effect.affectedStat == StatType.Defense || effect.affectedStat == StatType.MoveSpeed)
            {
                Assert.AreEqual(ModifierApplication.PercentAdditive, effect.application, $"{skill.SkillId}: {effect.displayName}");
            }
        }

        // ------------------------------------------------------------------
        // Player defense
        // ------------------------------------------------------------------

        [Test]
        public void Player_WithoutDefense_TakesTheRawAmount()
        {
            (PlayerStats stats, _) = NewPlayer();

            Assert.AreEqual(17f, DamageTaken(stats, 17f), 0.0001f);
        }

        [Test]
        public void Player_CampoMagnetico_CutsDamageTakenBy25Percent()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            SkillDefinition campo = LoadSkill("Characters/Icaro/skill_icaro_campo_magnetico");
            buffs.ApplyEffects(campo.Buffs, SkillManager.SelfBuffSourceKey(campo.SkillId));

            Assert.AreEqual(30f, DamageTaken(stats, 40f), 0.0001f);
        }

        [Test]
        public void Player_BerserkDefenseDebuff_IncreasesDamageTaken()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            SkillDefinition berserk = LoadSkill("skill_berserk");
            // Berserk's "Exposto" (-30% Defense) — normally put on its targets; here on the player.
            buffs.ApplyEffects(berserk.Debuffs, "exposto");

            Assert.AreEqual(13f, DamageTaken(stats, 10f), 0.0001f);
        }

        [Test]
        public void Player_FlatArmorAffix_GoesThroughTheArmorCurve()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            buffs.ApplyEffect(Effect(StatType.Defense, ModifierApplication.Flat, 100f), "equip_Chest");

            Assert.AreEqual(100f, stats.FlatDefense, 0.0001f);
            Assert.AreEqual(10f, DamageTaken(stats, 20f), 0.0001f);
        }

        [Test]
        public void Player_StackedDefenseBuffs_AreClamped()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            buffs.ApplyEffects(LoadSkill("Characters/Joaquim/skill_joaquim_muralha_de_pedra").Buffs, "a");
            buffs.ApplyEffects(LoadSkill("Characters/Joaquim/skill_joaquim_ultima_resistencia").Buffs, "b");

            // 0.5 + 0.8 = 1.3 → capped at 80% reduction, never immune.
            Assert.AreEqual(1f - DamageCalculator.MaxPercentDefense, stats.DefenseMultiplier, 0.0001f);
            Assert.AreEqual(10f, DamageTaken(stats, 50f), 0.0001f);
        }

        [Test]
        public void Player_DefenseBuffExpiring_RestoresFullDamage()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            buffs.ApplyEffect(Effect(StatType.Defense, ModifierApplication.PercentAdditive, 0.45f), "escudo");
            Assert.AreEqual(5.5f, DamageTaken(stats, 10f), 0.0001f);

            buffs.RemoveEffectsBySource("escudo");

            Assert.AreEqual(10f, DamageTaken(stats, 10f), 0.0001f);
        }

        // ------------------------------------------------------------------
        // Enemy defense (Health)
        // ------------------------------------------------------------------

        [Test]
        public void Enemy_DroneDefenseDebuff_IncreasesDamageTakenBy20Percent()
        {
            (Health health, BuffController buffs) = NewEnemyHealth();
            Assert.AreEqual(100f, DamageTaken(health, 100f), 0.0001f, "precondition: no defense");

            SkillDefinition drone = LoadSkill("Characters/Icaro/skill_icaro_drone_de_ataque");
            buffs.ApplyEffects(drone.Debuffs, SkillManager.DebuffSourceKey(drone.SkillId));

            Assert.AreEqual(120f, DamageTaken(health, 100f), 0.0001f);
        }

        [Test]
        public void Enemy_StackedDefenseDebuffs_AtMostDoubleDamage()
        {
            (Health health, BuffController buffs) = NewEnemyHealth();
            buffs.ApplyEffects(LoadSkill("Characters/Lavine/skill_lavine_eclipse_eterno").Debuffs, "a"); // -0.3
            buffs.ApplyEffects(LoadSkill("Characters/Lavine/skill_lavine_correntes_das_trevas").Debuffs, "b"); // -0.15
            buffs.ApplyEffects(LoadSkill("Characters/Icaro/skill_icaro_drone_de_ataque").Debuffs, "c"); // -0.2

            Assert.AreEqual(165f, DamageTaken(health, 100f), 0.0001f);

            buffs.ApplyEffect(Effect(StatType.Defense, ModifierApplication.PercentAdditive, -2f, isDebuff: true), "d");

            Assert.AreEqual(200f, DamageTaken(health, 100f), 0.0001f);
        }

        [Test]
        public void Enemy_DefenseBuffAndBaseArmor_ReduceDamage()
        {
            (Health health, BuffController buffs) = NewEnemyHealth();
            SetPrivateField(health, "_defense", 100f);
            Assert.AreEqual(50f, DamageTaken(health, 100f), 0.0001f, "base armor from the inspector");

            buffs.ApplyEffect(Effect(StatType.Defense, ModifierApplication.PercentAdditive, 0.5f), "boss_shield");

            Assert.AreEqual(25f, DamageTaken(health, 100f), 0.0001f);
        }

        // ------------------------------------------------------------------
        // MoveSpeed
        // ------------------------------------------------------------------

        [Test]
        public void MoveSpeedMultiplier_SlowHasteRootAndClamps()
        {
            GameObject go = NewObject("Speed");
            BuffController buffs = go.AddComponent<BuffController>();
            Assert.AreEqual(1f, buffs.GetMoveSpeedMultiplier());

            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.5f, true), "slow");
            Assert.AreEqual(0.5f, buffs.GetMoveSpeedMultiplier(), 0.0001f);

            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.7f, true), "slow2");
            Assert.IsFalse(buffs.IsRooted, "stacked partial slows never root");
            Assert.AreEqual(BuffController.MinMoveSpeedMultiplier, buffs.GetMoveSpeedMultiplier(), 0.0001f);

            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -1f, true), "root");
            Assert.IsTrue(buffs.IsRooted);
            Assert.AreEqual(0f, buffs.GetMoveSpeedMultiplier());

            buffs.ClearAllEffects();
            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, 1.5f), "dash");
            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.5f), "wind");
            Assert.AreEqual(BuffController.MaxMoveSpeedMultiplier, buffs.GetMoveSpeedMultiplier(), 0.0001f);
        }

        [Test]
        public void Player_SlowAndHaste_ChangeRealMotorSpeed()
        {
            GameObject go = NewObject("MotorPlayer");
            PlayerMotor motor = go.AddComponent<PlayerMotor>();
            BuffController buffs = go.AddComponent<BuffController>();
            Vector3 far = new Vector3(100f, 0f, 0f);

            motor.MoveToward(far, isRunning: false);
            float baseSpeed = motor.HorizontalVelocity.magnitude;
            Assert.Greater(baseSpeed, 0f, "precondition: walking");

            buffs.ApplyEffects(LoadSkill("Characters/Icaro/skill_icaro_granada_eletrica").Debuffs, "granada"); // -35%
            motor.MoveToward(far, isRunning: false);
            Assert.AreEqual(baseSpeed * 0.65f, motor.HorizontalVelocity.magnitude, 0.0001f);

            buffs.ClearAllEffects();
            buffs.ApplyEffects(LoadSkill("Characters/Maithe/skill_maithe_passo_do_vento").Buffs, "passo"); // +50%
            motor.MoveToward(far, isRunning: false);
            Assert.AreEqual(baseSpeed * 1.5f, motor.HorizontalVelocity.magnitude, 0.0001f);
        }

        [Test]
        public void Player_Rooted_DoesNotMove()
        {
            GameObject go = NewObject("RootedPlayer");
            PlayerMotor motor = go.AddComponent<PlayerMotor>();
            BuffController buffs = go.AddComponent<BuffController>();
            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -1f, true), "root");

            motor.MoveToward(new Vector3(100f, 0f, 0f), isRunning: true);

            Assert.AreEqual(0f, motor.HorizontalVelocity.magnitude, 0.0001f);
        }

        private EnemyController NewEnemy(out BuffController buffs, float attackDamage = 10f)
        {
            GameObject go = NewObject("MovingEnemy");
            go.AddComponent<Health>();
            EnemyController controller = go.AddComponent<EnemyController>();
            if (!go.TryGetComponent(out buffs))
            {
                buffs = go.AddComponent<BuffController>();
            }

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_attackDamage", attackDamage);
            controller.ResetForSpawn(definition, Vector3.zero);
            return controller;
        }

        [Test]
        public void Enemy_Slow_ReducesDistanceMoved()
        {
            EnemyController enemy = NewEnemy(out BuffController buffs);

            enemy.MoveInDirection(Vector3.forward, 4f, 1f);
            Assert.AreEqual(4f, enemy.transform.position.magnitude, 0.0001f, "precondition: full speed");

            enemy.transform.position = Vector3.zero;
            SkillDefinition terremoto = LoadSkill("Characters/Joaquim/skill_joaquim_terremoto"); // -50%
            buffs.ApplyEffects(terremoto.Debuffs, SkillManager.DebuffSourceKey(terremoto.SkillId));
            enemy.MoveInDirection(Vector3.forward, 4f, 1f);

            Assert.AreEqual(2f, enemy.transform.position.magnitude, 0.0001f);
        }

        [Test]
        public void Enemy_Freeze_KeepsTenPercentSpeed_AndRootStopsIt()
        {
            EnemyController enemy = NewEnemy(out BuffController buffs);
            buffs.ApplyEffects(LoadSkill("skill_freeze").Debuffs, "freeze"); // -90%
            enemy.MoveInDirection(Vector3.forward, 4f, 1f);
            Assert.AreEqual(0.4f, enemy.transform.position.magnitude, 0.0001f);

            enemy.transform.position = Vector3.zero;
            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -1f, true), "root");
            enemy.MoveInDirection(Vector3.forward, 4f, 1f);

            Assert.AreEqual(Vector3.zero, enemy.transform.position);
        }

        [Test]
        public void Enemy_ResetForSpawn_ClearsLastLifesDebuffs()
        {
            EnemyController enemy = NewEnemy(out BuffController buffs);
            buffs.ApplyEffect(Effect(StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.5f, true), "slow");

            enemy.ResetForSpawn(enemy.Definition, Vector3.zero);

            Assert.AreEqual(1f, enemy.MoveSpeedMultiplier);
        }

        [Test]
        public void Enemy_AttackDamageDebuff_WeakensItsSwing()
        {
            EnemyController enemy = NewEnemy(out BuffController buffs, attackDamage: 20f);
            Assert.AreEqual(20f, enemy.EffectiveAttackDamage, 0.0001f);

            SkillDefinition provocacao = LoadSkill("Characters/Joaquim/skill_joaquim_provocacao_heroica"); // -25%
            buffs.ApplyEffects(provocacao.Debuffs, SkillManager.DebuffSourceKey(provocacao.SkillId));

            Assert.AreEqual(15f, enemy.EffectiveAttackDamage, 0.0001f);
        }

        // ------------------------------------------------------------------
        // Other stats wired in the same pass
        // ------------------------------------------------------------------

        [Test]
        public void Player_RegenAndMaxStaminaModifiers_AreRead()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            float baseRegen = stats.HealthRegenPerSecond;
            float baseStamina = stats.MaxStamina;

            buffs.ApplyEffect(Effect(StatType.HealthRegen, ModifierApplication.Flat, 1.5f), "equip_Ring");
            buffs.ApplyEffect(Effect(StatType.MaxStamina, ModifierApplication.Flat, 18f), "equip_Feet");

            Assert.AreEqual(baseRegen + 1.5f, stats.HealthRegenPerSecond, 0.0001f);
            Assert.AreEqual(baseStamina + 18f, stats.MaxStamina, 0.0001f);
        }

        [Test]
        public void Player_DotStyleHealthRegenEffect_DoesNotChangeRegen()
        {
            (PlayerStats stats, BuffController buffs) = NewPlayer();
            float baseRegen = stats.HealthRegenPerSecond;
            SkillDefinition poison = LoadSkill("skill_poison_dart");
            buffs.ApplyEffects(poison.Debuffs, "poison");

            Assert.AreEqual(baseRegen, stats.HealthRegenPerSecond, 0.0001f, "DoT effects carry value 0 — damage comes from ticks");
        }
    }
}
