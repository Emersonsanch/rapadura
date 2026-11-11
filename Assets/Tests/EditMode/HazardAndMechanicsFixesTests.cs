using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items.Sets;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>Regressions: hazard damage and i-frames, the Hora da Barca heal, saved daily chests, and a death during a Farnkesh ride.</summary>
    public class HazardAndMechanicsFixesTests
    {
        private GameObject _go;
        private PlayerStats _stats;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("HazardHero");
            _stats = _go.AddComponent<PlayerStats>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Hazard_DoesNotGrantInvulnerability()
        {
            _stats.ApplyHazardDamage(1f);

            Assert.IsFalse(_stats.IsInvulnerable);
        }

        [Test]
        public void Hazard_LandsDuringInvulnerability()
        {
            _stats.GrantInvulnerability(5f);
            float before = _stats.CurrentHealth;

            _stats.ApplyHazardDamage(3f);

            Assert.AreEqual(before - 3f, _stats.CurrentHealth, 1e-3f);
            Assert.IsTrue(_stats.IsInvulnerable);
        }

        [Test]
        public void Hazard_PublishesTheDamageEvent()
        {
            float published = 0f;
            EventBus.Subscribe<DamageAppliedEvent>(evt => published = evt.FinalDamage);

            float taken = _stats.ApplyHazardDamage(4f);

            Assert.AreEqual(4f, taken, 1e-3f);
            Assert.AreEqual(4f, published, 1e-3f);
        }

        [Test]
        public void Hazard_NeverKills()
        {
            _stats.ApplyHazardDamage(_stats.MaxHealth * 100f);

            Assert.IsTrue(_stats.IsAlive);
            Assert.AreEqual(1f, _stats.CurrentHealth, 1e-3f);
        }

        [Test]
        public void Hazard_DefenseDebuffCannotGrowTheBite()
        {
            BuffController buffs = _go.AddComponent<BuffController>();
            buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Exposto",
                affectedStat = StatType.Defense,
                application = ModifierApplication.PercentAdditive,
                value = -1f,
                durationSeconds = 60f,
                isDebuff = true,
            }, "test_debuff");
            Assume.That(_stats.DefenseMultiplier, Is.GreaterThan(1f));
            float before = _stats.CurrentHealth;

            _stats.ApplyHazardDamage(before - 1f);

            Assert.AreEqual(1f, _stats.CurrentHealth, 1e-3f);
            Assert.IsTrue(_stats.IsAlive);
        }

        [Test]
        public void BargeHour_HealsTheHeroThroughPlayerStats()
        {
            InventoryManager inventory = _go.AddComponent<InventoryManager>();
            _stats.ApplyDamage(_stats.MaxHealth * 0.5f);
            float before = _stats.CurrentHealth;

            bool healed = ArmorSetBonuses.TryBargeHourHeal(inventory, ArmorSetRules.Papyrus, 0.0);

            Assert.IsTrue(healed);
            Assert.Greater(_stats.CurrentHealth, before);
        }

        [Test]
        public void DailyMarks_SurviveASaveAndAreClearedOnNewGame()
        {
            var progress = new WorldMapProgress();
            Assert.AreEqual(0, progress.GetDailyMark("chest"));
            progress.SetDailyMark("chest", 7);

            string json = JsonUtility.ToJson(progress.CaptureState());
            var reloaded = new WorldMapProgress();
            reloaded.RestoreState(JsonUtility.FromJson<WorldMapSaveState>(json));

            Assert.AreEqual(7, reloaded.GetDailyMark("chest"));
            Assert.IsFalse(MingchengReflectionRules.CanOpen(reloaded.GetDailyMark("chest"), 7));

            reloaded.ResetToNewGame();
            Assert.AreEqual(0, reloaded.GetDailyMark("chest"));
        }

        [Test]
        public void FarnkeshRide_EndsWhenTheHeroDies()
        {
            FarnkeshMechanicsDriver driver = _go.AddComponent<FarnkeshMechanicsDriver>();
            SetField(driver, "_player", _go.transform);
            SetField(driver, "_stats", _stats);
            SetField(driver, "_ride", OverworldLayouts.SkyRoutes[0]);
            SetField(driver, "_rideSeconds", 1000f);
            SetField(driver, "_lastRidePosition", _go.transform.position);
            _stats.ApplyDamage(_stats.MaxHealth * 100f);
            Assume.That(_stats.IsAlive, Is.False);

            typeof(FarnkeshMechanicsDriver).GetMethod("TickRide", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(driver, null);

            Assert.IsNull(Field("_ride").GetValue(driver));
        }

        private static FieldInfo Field(string name)
        {
            FieldInfo field = typeof(FarnkeshMechanicsDriver).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name);
            return field;
        }

        private static void SetField(object target, string name, object value) => Field(name).SetValue(target, value);
    }
}
