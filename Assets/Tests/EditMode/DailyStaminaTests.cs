using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Daily;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Daily;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Fôlego diário" (<see cref="DailyStaminaRules"/>, <see cref="DailyStaminaService"/>, <see cref="StaminaLootRules"/>):
    /// tiers and multipliers, play/rest buckets, idle exclusion, the clock-tamper guard and the reminders
    /// (Docs/Balance/README.md "Fôlego diário").
    /// </summary>
    public class DailyStaminaTests
    {
        private const double Hour = 3600;
        private static readonly long T0 = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc).Ticks;

        private sealed class MemoryStore : IDailyStaminaStore
        {
            public string Json = string.Empty;
            public int Minutes;
            public string LoadJson() => Json;
            public void SaveJson(string json) => Json = json;
            public int LoadReminderMinutes() => Minutes;
            public void SaveReminderMinutes(int minutes) => Minutes = minutes;
        }

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                Object.DestroyImmediate(o);
            }

            _created.Clear();
        }

        private static DailyStaminaState Fresh() => new DailyStaminaState();

        // ---------------- tiers ----------------

        [Test]
        public void FreshState_IsFull_ThenNormalAfter3h_ThenTiredAfter5h()
        {
            DailyStaminaState s = Fresh();
            Assert.AreEqual(StaminaTier.Full, DailyStaminaRules.TierOf(s));
            DailyStaminaRules.Play(s, 3 * Hour - 1);
            Assert.AreEqual(StaminaTier.Full, DailyStaminaRules.TierOf(s));
            DailyStaminaRules.Play(s, 1);
            Assert.AreEqual(StaminaTier.Normal, DailyStaminaRules.TierOf(s));
            DailyStaminaRules.Play(s, 2 * Hour);
            Assert.AreEqual(StaminaTier.Tired, DailyStaminaRules.TierOf(s));
            DailyStaminaRules.Play(s, 10 * Hour);
            Assert.AreEqual(StaminaTier.Tired, DailyStaminaRules.TierOf(s), "Tired never blocks and never goes below.");
            Assert.AreEqual(0, s.FullLeftSeconds);
            Assert.AreEqual(0, s.NormalLeftSeconds);
        }

        [Test]
        public void Multipliers_Are130_100_50()
        {
            Assert.AreEqual(1.3f, DailyStaminaRules.MultiplierOf(StaminaTier.Full), 1e-6f);
            Assert.AreEqual(1f, DailyStaminaRules.MultiplierOf(StaminaTier.Normal), 1e-6f);
            Assert.AreEqual(0.5f, DailyStaminaRules.MultiplierOf(StaminaTier.Tired), 1e-6f);
        }

        [Test]
        public void KillXp_FullTierIsTheTunedPace_OthersKeepTheRatios()
        {
            Assert.AreEqual(1f, DailyStaminaRules.ExperienceMultiplierOf(StaminaTier.Full), 1e-6f, "Fôlego cheio = the r8 XP (power budget §14.3).");
            float normal = DailyStaminaRules.ExperienceMultiplierOf(StaminaTier.Normal);
            float tired = DailyStaminaRules.ExperienceMultiplierOf(StaminaTier.Tired);
            Assert.AreEqual(1.3f, 1f / normal, 1e-4f, "Full is +30% over normal.");
            Assert.AreEqual(0.5f, tired / normal, 1e-4f, "Tired is −50% under normal.");
            Assert.AreEqual(40, DailyStaminaRules.ScaleExperience(40, DailyStaminaRules.ExperienceMultiplierOf(StaminaTier.Full)));
        }

        [Test]
        public void ScaleExperience_RoundsAndKeepsAtLeastOne()
        {
            Assert.AreEqual(13, DailyStaminaRules.ScaleExperience(10, 1.3f));
            Assert.AreEqual(52, DailyStaminaRules.ScaleExperience(40, 1.3f));
            Assert.AreEqual(5, DailyStaminaRules.ScaleExperience(10, 0.5f));
            Assert.AreEqual(1, DailyStaminaRules.ScaleExperience(1, 0.5f));
            Assert.AreEqual(0, DailyStaminaRules.ScaleExperience(0, 1.3f));
            Assert.AreEqual(18, DailyStaminaRules.ScaleExperience(18, 1f));
        }

        [Test]
        public void ScaleLootQuantity_FloorPlusFractionChance()
        {
            Assert.AreEqual(2, DailyStaminaRules.ScaleLootQuantity(2, 1.3f, 0.99));
            Assert.AreEqual(3, DailyStaminaRules.ScaleLootQuantity(2, 1.3f, 0.1));
            Assert.AreEqual(0, DailyStaminaRules.ScaleLootQuantity(1, 0.5f, 0.7));
            Assert.AreEqual(1, DailyStaminaRules.ScaleLootQuantity(1, 0.5f, 0.2));
            Assert.AreEqual(5, DailyStaminaRules.ScaleLootQuantity(10, 0.5f, 0.0));
            Assert.AreEqual(7, DailyStaminaRules.ScaleLootQuantity(7, 1f, 0.0));
        }

        [Test]
        public void ScaleLootQuantity_AveragesTheMultiplier()
        {
            var random = new System.Random(3);
            double sum = 0;
            const int n = 20000;
            for (int i = 0; i < n; i++)
            {
                sum += DailyStaminaRules.ScaleLootQuantity(2, 1.3f, random.NextDouble());
            }

            Assert.AreEqual(2.6, sum / n, 0.03);
        }

        [Test]
        public void StaminaLoot_ScalesStacksOnly_AndKeepsTheListAtX1()
        {
            var gold = ScriptableObject.CreateInstance<ItemDefinition>();
            _created.Add(gold);
            var drops = new List<LootDrop> { new LootDrop(gold, 10), new LootDrop(gold, 1) };

            Assert.AreSame(drops, StaminaLootRules.Apply(drops, 1f, new System.Random(1)));

            List<LootDrop> tired = StaminaLootRules.Apply(drops, 0.5f, new System.Random(1));
            Assert.AreEqual(5, tired[0].Quantity);
            Assert.LessOrEqual(tired.Count, 2);

            List<LootDrop> full = StaminaLootRules.Apply(new List<LootDrop> { new LootDrop(gold, 10) }, 1.3f, new System.Random(1));
            Assert.AreEqual(13, full[0].Quantity);
        }

        // ---------------- rest ----------------

        [Test]
        public void Rest_RefillsFullFirst_OneToOne()
        {
            DailyStaminaState s = Fresh();
            DailyStaminaRules.Play(s, 6 * Hour);
            Assert.AreEqual(StaminaTier.Tired, DailyStaminaRules.TierOf(s));

            DailyStaminaRules.Rest(s, 1 * Hour);
            Assert.AreEqual(StaminaTier.Full, DailyStaminaRules.TierOf(s), "1 hour away gives back 1 hour of full fôlego.");
            Assert.AreEqual(1 * Hour, s.FullLeftSeconds, 1e-6);
            Assert.AreEqual(0, s.NormalLeftSeconds, 1e-6);

            DailyStaminaRules.Rest(s, 3 * Hour);
            Assert.AreEqual(3 * Hour, s.FullLeftSeconds, 1e-6);
            Assert.AreEqual(1 * Hour, s.NormalLeftSeconds, 1e-6);

            DailyStaminaRules.Rest(s, 50 * Hour);
            Assert.AreEqual(1f, DailyStaminaRules.Fill(s), 1e-6f);
        }

        // ---------------- idle exclusion ----------------

        [Test]
        public void ClassifyTick_CountsOnlyActiveGameplay()
        {
            Assert.AreEqual(StaminaTickKind.Active, DailyStaminaRules.ClassifyTick(true, false, true, 10));
            Assert.AreEqual(StaminaTickKind.Resting, DailyStaminaRules.ClassifyTick(false, false, true, 0), "Title screen rests.");
            Assert.AreEqual(StaminaTickKind.Resting, DailyStaminaRules.ClassifyTick(true, true, true, 0), "Paused rests.");
            Assert.AreEqual(StaminaTickKind.Resting, DailyStaminaRules.ClassifyTick(true, false, false, 0), "Background rests.");
            Assert.AreEqual(StaminaTickKind.Active, DailyStaminaRules.ClassifyTick(true, false, true, DailyStaminaRules.IdleThresholdSeconds - 1));
            Assert.AreEqual(StaminaTickKind.Resting, DailyStaminaRules.ClassifyTick(true, false, true, DailyStaminaRules.IdleThresholdSeconds), "Idle rests.");
        }

        // ---------------- clock guard ----------------

        [Test]
        public void ResolveOffline_FirstRun_NoRest()
        {
            OfflineRestResult r = DailyStaminaRules.ResolveOffline(0, 0, T0);
            Assert.AreEqual(0, r.RestSeconds);
            Assert.AreEqual(T0, r.NewLastSeenUtcTicks);
        }

        [Test]
        public void ResolveOffline_NormalGap_IsRest_CappedAtCapacity()
        {
            OfflineRestResult r = DailyStaminaRules.ResolveOffline(T0, 0, T0 + DailyStaminaRules.SecondsToTicks(2 * Hour));
            Assert.AreEqual(2 * Hour, r.RestSeconds, 1e-3);

            OfflineRestResult week = DailyStaminaRules.ResolveOffline(T0, 0, T0 + DailyStaminaRules.SecondsToTicks(7 * 24 * Hour));
            Assert.AreEqual(DailyStaminaRules.CapacitySeconds, week.RestSeconds, 1e-3);
        }

        [Test]
        public void ResolveOffline_ClockTurnedBack_NoRest_HighWaterKept()
        {
            long back = T0 - DailyStaminaRules.SecondsToTicks(10 * Hour);
            OfflineRestResult r = DailyStaminaRules.ResolveOffline(T0, 0, back);
            Assert.AreEqual(0, r.RestSeconds);
            Assert.AreEqual(T0, r.NewLastSeenUtcTicks);

            // Real time passes but the clock is still behind the high-water mark: still nothing.
            OfflineRestResult later = DailyStaminaRules.ResolveOffline(r.NewLastSeenUtcTicks, r.NewClockSkewSeconds, back + DailyStaminaRules.SecondsToTicks(3 * Hour));
            Assert.AreEqual(0, later.RestSeconds);
        }

        [Test]
        public void DetectClockJump_IgnoresDrift_CatchesDateChanges()
        {
            Assert.AreEqual(0, DailyStaminaRules.DetectClockJump(1, 1.5));
            Assert.AreEqual(0, DailyStaminaRules.DetectClockJump(0.016, 60));
            Assert.AreEqual(10 * Hour - 0.016, DailyStaminaRules.DetectClockJump(0.016, 10 * Hour), 1e-6);
            Assert.Less(DailyStaminaRules.DetectClockJump(0.016, -5 * Hour), 0);
        }

        [Test]
        public void ForwardJumpInSession_IsNotCountedAsRestOnTheNextLaunch()
        {
            long now = T0;
            var store = new MemoryStore();
            var service = new DailyStaminaService(store, () => now);
            service.Initialize();
            now += DailyStaminaRules.SecondsToTicks(6 * Hour);
            service.Tick(6 * Hour, StaminaTickKind.Active);
            Assert.AreEqual(StaminaTier.Tired, service.Tier);

            // Next frame: the wall clock jumps 10 h ahead while only 1 s passed.
            now += DailyStaminaRules.SecondsToTicks(10 * Hour);
            service.Tick(1, StaminaTickKind.Active);
            service.Shutdown();

            // Relaunch 30 real minutes later with the clock still forwarded.
            now += DailyStaminaRules.SecondsToTicks(0.5 * Hour);
            var relaunched = new DailyStaminaService(store, () => now);
            relaunched.Initialize();
            Assert.AreEqual(0.5 * Hour, relaunched.Snapshot.FullLeftSeconds, 2.0, "Only the real 30 minutes count as rest.");
        }

        [Test]
        public void Service_PersistsAcrossLaunches_AndRestsOffline()
        {
            long now = T0;
            var store = new MemoryStore();
            var service = new DailyStaminaService(store, () => now);
            service.Initialize();
            Assert.AreEqual(StaminaTier.Full, service.Tier);
            Assert.AreEqual(1.3f, service.Multiplier, 1e-6f);

            now += DailyStaminaRules.SecondsToTicks(4 * Hour);
            service.Tick(4 * Hour, StaminaTickKind.Active);
            Assert.AreEqual(StaminaTier.Normal, service.Tier);
            service.Shutdown();

            now += DailyStaminaRules.SecondsToTicks(2 * Hour);
            var next = new DailyStaminaService(store, () => now);
            next.Initialize();
            Assert.AreEqual(StaminaTier.Full, next.Tier);
            Assert.AreEqual(2 * Hour, next.Snapshot.FullLeftSeconds, 1.0);
        }

        // ---------------- reminders ----------------

        [Test]
        public void TiredReminder_OnEntering_ThenEvery30MinOfPlay()
        {
            Assert.IsTrue(DailyStaminaRules.ShouldRemindTired(StaminaTier.Normal, StaminaTier.Tired, 0));
            Assert.IsFalse(DailyStaminaRules.ShouldRemindTired(StaminaTier.Tired, StaminaTier.Tired, 29 * 60));
            Assert.IsTrue(DailyStaminaRules.ShouldRemindTired(StaminaTier.Tired, StaminaTier.Tired, 30 * 60));
            Assert.IsFalse(DailyStaminaRules.ShouldRemindTired(StaminaTier.Full, StaminaTier.Normal, 999999));
        }

        [Test]
        public void Service_PublishesTiredReminder_OnceWhenCrossing5h()
        {
            long now = T0;
            var service = new DailyStaminaService(new MemoryStore(), () => now);
            service.Initialize();
            int reminders = 0;
            Action<StaminaReminderEvent> handler = e =>
            {
                if (e.Kind == StaminaReminderKind.Tired)
                {
                    reminders++;
                }
            };
            EventBus.Subscribe(handler);
            try
            {
                for (int i = 0; i < 5 * 60 + 10; i++)
                {
                    now += DailyStaminaRules.SecondsToTicks(60);
                    service.Tick(60, StaminaTickKind.Active);
                }
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(1, reminders);
        }

        [Test]
        public void PlayTimeReminder_EveryNMinutes_OffAtZero()
        {
            Assert.IsFalse(DailyStaminaRules.IsPlayTimeReminderDue(10_000, 0, 0));
            Assert.IsFalse(DailyStaminaRules.IsPlayTimeReminderDue(29 * 60, 0, 30));
            Assert.IsTrue(DailyStaminaRules.IsPlayTimeReminderDue(30 * 60, 0, 30));
            Assert.IsFalse(DailyStaminaRules.IsPlayTimeReminderDue(50 * 60, 30 * 60, 30));
            Assert.AreEqual(0, DailyStaminaRules.SanitizeReminderMinutes(-5));
            Assert.AreEqual(60, DailyStaminaRules.SanitizeReminderMinutes(55));
            Assert.AreEqual(120, DailyStaminaRules.SanitizeReminderMinutes(500));
        }

        // ---------------- view ----------------

        [Test]
        public void Meter_HasItsParts_AndTierClasses()
        {
            var meter = DailyStaminaHud.CreateMeter();
            Assert.IsNotNull(meter.Q(DailyStaminaHud.MeterFillName));
            Assert.IsNotNull(meter.Q(DailyStaminaHud.TooltipName));
            Assert.AreEqual(DailyStaminaHud.TiredClass, DailyStaminaHud.TierClass(StaminaTier.Tired));
            StringAssert.Contains("+30%", DailyTexts.Tooltip(null, StaminaTier.Full, 2 * Hour));
            StringAssert.Contains("Vó Edna", DailyTexts.ReminderText(null, StaminaReminderKind.Tired, 0));
            Assert.AreEqual("2 h 05 min", DailyTexts.Duration(null, 2 * Hour + 4 * 60 + 1));
        }
    }
}
