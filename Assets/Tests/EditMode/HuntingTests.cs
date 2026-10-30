using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Hunting;
using Rapadura.Gameplay.Quests;
using Rapadura.UI.Hunting;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// Hunting features (Docs/Balance/README.md § "Caça"): task generation and progress, daily seeding, prey and highlight
    /// bonuses, charm costs and effects, the reward/combat hooks and the save round-trip.
    /// </summary>
    public class HuntingTests
    {
        private sealed class FakeClock : IDailyClock
        {
            public DateTime Now { get; set; } = new DateTime(2026, 10, 29, 10, 0, 0);
        }

        private static readonly HuntingCreature Wolf = new HuntingCreature("enemy_forest_wolf", EnemyMotionArchetype.Quadruped, false, 0, 10);
        private static readonly HuntingCreature Spider = new HuntingCreature("enemy_forest_spider", EnemyMotionArchetype.Arthropod, false, 0, 10);
        private static readonly HuntingCreature Scorpion = new HuntingCreature("enemy_desert_scorpion", EnemyMotionArchetype.Arthropod, false, 1, 18);
        private static readonly HuntingCreature Bats = new HuntingCreature("enemy_cave_bat_swarm", EnemyMotionArchetype.Flyer, false, 3, 28);
        private static readonly HuntingCreature Dweller = new HuntingCreature("enemy_cave_dweller", EnemyMotionArchetype.Heavy, false, 3, 40);
        private static readonly HuntingCreature AlphaWolf = new HuntingCreature("enemy_forest_alpha_wolf", EnemyMotionArchetype.Quadruped, true, 0, 40);
        private static readonly HuntingCreature Unplaced = new HuntingCreature("enemy_unplaced", EnemyMotionArchetype.Quadruped, false, int.MaxValue, 10);

        private static readonly HuntingCreature[] All = { Wolf, Spider, Scorpion, Bats, Dweller, AlphaWolf, Unplaced };

        private Dictionary<string, int> _kills;
        private FakeClock _clock;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CombatModifierHooks.ClearAll();
            EnemyRewardHooks.ClearAll();
            _kills = new Dictionary<string, int>();
            _clock = new FakeClock();
        }

        [TearDown]
        public void TearDown()
        {
            CombatModifierHooks.ClearAll();
            EnemyRewardHooks.ClearAll();
            EventBus.Clear();
        }

        private HuntingProgress NewProgress(Func<double> random = null)
        {
            return new HuntingProgress(_clock, () => All, id => id != null && _kills.TryGetValue(id, out int k) ? k : 0, random ?? (() => 0.5));
        }

        // ------------------------------------------------------------------ tasks

        [Test]
        public void CreateTask_SizesAndRewardsByTier()
        {
            HuntingTask small = HuntingRules.CreateTask(Wolf, HuntingTaskTier.Small);
            HuntingTask medium = HuntingRules.CreateTask(Wolf, HuntingTaskTier.Medium);
            HuntingTask large = HuntingRules.CreateTask(Bats, HuntingTaskTier.Large);

            Assert.AreEqual((10, 50, 15, 1), (small.required, small.rewardExperience, small.rewardGold, small.rewardHuntPoints));
            Assert.AreEqual((25, 150, 45, 3), (medium.required, medium.rewardExperience, medium.rewardGold, medium.rewardHuntPoints));
            Assert.AreEqual((60, 1260, 120, 8), (large.required, large.rewardExperience, large.rewardGold, large.rewardHuntPoints));
            Assert.AreEqual(0, small.progress);
        }

        [Test]
        public void CreateTask_NeverForBossesOrEmptyIds()
        {
            Assert.IsNull(HuntingRules.CreateTask(AlphaWolf, HuntingTaskTier.Small));
            Assert.IsNull(HuntingRules.CreateTask(default, HuntingTaskTier.Small));
        }

        [Test]
        public void RecordKill_CountsOnlyItsCreature_AndCompletesOnce()
        {
            HuntingTask task = HuntingRules.CreateTask(Wolf, HuntingTaskTier.Small);
            Assert.IsFalse(HuntingRules.RecordKill(task, Spider.Id));
            for (int i = 0; i < 9; i++)
            {
                Assert.IsFalse(HuntingRules.RecordKill(task, Wolf.Id));
            }

            Assert.IsTrue(HuntingRules.RecordKill(task, Wolf.Id));
            Assert.IsFalse(HuntingRules.RecordKill(task, Wolf.Id), "a finished task never completes twice");
            Assert.AreEqual(10, task.progress);
        }

        [Test]
        public void CanTakeTask_OneAtATime_AndDailyCap()
        {
            Assert.IsTrue(HuntingRules.CanTakeTask(null, 0));
            Assert.IsFalse(HuntingRules.CanTakeTask(HuntingRules.CreateTask(Wolf, HuntingTaskTier.Small), 0));
            Assert.IsTrue(HuntingRules.CanTakeTask(new HuntingTask(), 0), "an empty (deserialized) task is no task");
            Assert.IsFalse(HuntingRules.CanTakeTask(null, HuntingRules.MaxTasksPerDay));
        }

        [Test]
        public void EligibleCreatures_UnlockedRegulars_ElseFirstRegion()
        {
            CollectionAssert.AreEquivalent(new[] { Wolf.Id, Spider.Id }, HuntingRules.EligibleCreatures(All, _ => false).Select(c => c.Id));
            CollectionAssert.AreEquivalent(new[] { Scorpion.Id, Bats.Id },
                HuntingRules.EligibleCreatures(All, id => id == Scorpion.Id || id == Bats.Id || id == AlphaWolf.Id || id == Unplaced.Id).Select(c => c.Id));
        }

        [Test]
        public void Service_TakeProgressCompleteAndCancel()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            int completedEvents = 0;
            EventBus.Subscribe<HuntingTaskCompletedEvent>(_ => completedEvents++);

            Assert.IsFalse(progress.TakeTask(Bats.Id, HuntingTaskTier.Small), "not eligible before its page is unlocked");
            Assert.IsTrue(progress.TakeTask(Wolf.Id, HuntingTaskTier.Small));
            Assert.IsFalse(progress.TakeTask(Spider.Id, HuntingTaskTier.Small), "one task at a time");

            Assert.IsTrue(progress.CancelTask());
            Assert.IsNull(progress.ActiveTask);
            Assert.IsTrue(progress.TakeTask(Spider.Id, HuntingTaskTier.Small));
            for (int i = 0; i < HuntingRules.SmallKills; i++)
            {
                progress.RecordKill(Spider.Id);
            }

            Assert.IsNull(progress.ActiveTask);
            Assert.AreEqual(1, completedEvents);
            Assert.AreEqual(1, progress.TasksCompletedToday);
            Assert.AreEqual(1, progress.HuntPointsAvailable);
            Assert.AreEqual(50, progress.PendingExperience, "no hero to deliver to: the reward waits");
            Assert.AreEqual(15, progress.PendingGold);
        }

        // ------------------------------------------------------------------ daily seeding

        [Test]
        public void PickForDay_IsDeterministic_AndOrderIndependent()
        {
            string[] ids = { "a", "b", "c", "d", "e" };
            List<string> first = HuntingRules.PickForDay("2026-10-29", "prey:0", ids, 3);
            CollectionAssert.AreEqual(first, HuntingRules.PickForDay("2026-10-29", "prey:0", ids.Reverse(), 3));
            Assert.AreEqual(3, first.Distinct().Count());
            Assert.AreEqual(2, HuntingRules.PickForDay("2026-10-29", "x", new[] { "a", "b" }, 3).Count);
        }

        [Test]
        public void PickForDay_ChangesAcrossDays()
        {
            string[] ids = Enumerable.Range(0, 20).Select(i => "enemy_" + i).ToArray();
            var picks = new HashSet<string>();
            for (int day = 1; day <= 10; day++)
            {
                picks.Add(string.Join(",", HuntingRules.PickForDay($"2026-11-{day:00}", "prey:0", ids, 3)));
            }

            Assert.Greater(picks.Count, 5);
        }

        [Test]
        public void Boosted_IsANonBossPlacedCreature_SameAllDay()
        {
            for (int day = 1; day <= 28; day++)
            {
                string key = $"2026-02-{day:00}";
                string boosted = HuntingRules.BoostedCreature(key, All);
                Assert.AreNotEqual(AlphaWolf.Id, boosted);
                Assert.AreNotEqual(Unplaced.Id, boosted);
                Assert.AreEqual(boosted, HuntingRules.BoostedCreature(key, All.Reverse()));
            }

            Assert.IsNull(HuntingRules.BoostedCreature("2026-02-01", new[] { AlphaWolf }));
        }

        [Test]
        public void PreyOffers_ThreeDistinct_SkipBoostedWhenPossible_RerollChanges()
        {
            HuntingCreature[] eligible = { Wolf, Spider, Scorpion, Bats, Dweller };
            List<PreyOffer> offers = HuntingRules.PreyOffers("2026-10-29", 0, eligible, Wolf.Id);
            Assert.AreEqual(HuntingRules.PreyOfferCount, offers.Count);
            Assert.AreEqual(3, offers.Select(o => o.enemyId).Distinct().Count());
            Assert.IsFalse(offers.Any(o => o.enemyId == Wolf.Id));

            bool differs = false;
            for (int r = 1; r <= 5 && !differs; r++)
            {
                differs = !HuntingRules.PreyOffers("2026-10-29", r, eligible, Wolf.Id).Select(o => o.enemyId + o.bonus)
                    .SequenceEqual(offers.Select(o => o.enemyId + o.bonus));
            }

            Assert.IsTrue(differs, "a reroll draws a new set");
            Assert.AreEqual(2, HuntingRules.PreyOffers("2026-10-29", 0, new[] { Wolf, Spider }, Wolf.Id).Count, "small pools keep the boosted one");
        }

        [Test]
        public void IsNewDay_IgnoresClockRollback()
        {
            Assert.IsTrue(HuntingRules.IsNewDay(null, "2026-10-29"));
            Assert.IsTrue(HuntingRules.IsNewDay("2026-10-29", "2026-10-30"));
            Assert.IsFalse(HuntingRules.IsNewDay("2026-10-29", "2026-10-29"));
            Assert.IsFalse(HuntingRules.IsNewDay("2026-10-30", "2026-10-29"));
        }

        [Test]
        public void Service_DayRollover_ResetsPreyAndCap_KeepsTask()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            Assert.IsTrue(progress.PickPrey(0));
            Assert.IsTrue(progress.TakeTask(Wolf.Id, HuntingTaskTier.Medium));
            Assert.IsTrue(progress.RerollPrey(100, _ => true));
            Assert.AreEqual(-1, progress.PreyPickIndex, "a reroll clears the pick");
            Assert.IsFalse(progress.RerollPrey(100, _ => true), "one reroll a day");

            _clock.Now = _clock.Now.AddDays(-3);
            Assert.IsFalse(progress.Refresh(), "clock rollback: no new day");

            _clock.Now = new DateTime(2026, 10, 30, 8, 0, 0);
            Assert.IsTrue(progress.Refresh());
            Assert.AreEqual(0, progress.PreyRerollsToday);
            Assert.AreEqual(-1, progress.PreyPickIndex);
            Assert.IsNotNull(progress.ActiveTask, "the task survives the night");
        }

        [Test]
        public void Service_Reroll_NeedsGold_AndPays()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            int paid = 0;
            Assert.IsFalse(progress.RerollPrey(HuntingRules.PreyRerollGold - 1, cost => { paid += cost; return true; }));
            Assert.IsFalse(progress.RerollPrey(100, _ => false), "a failed payment changes nothing");
            Assert.AreEqual(0, progress.PreyRerollsToday);
            Assert.IsTrue(progress.RerollPrey(100, cost => { paid += cost; return true; }));
            Assert.AreEqual(HuntingRules.PreyRerollGold, paid);
        }

        // ------------------------------------------------------------------ bonuses

        [Test]
        public void PreyAndBoosted_Bonuses_StackOnTheirCreatureOnly()
        {
            var xpPrey = new PreyOffer(Wolf.Id, PreyBonusKind.Experience);
            var lootPrey = new PreyOffer(Wolf.Id, PreyBonusKind.Loot);
            var shieldPrey = new PreyOffer(Wolf.Id, PreyBonusKind.Protection);

            Assert.AreEqual(1.25f, HuntingRules.ExperienceMultiplier(Wolf, xpPrey, null), 1e-5f);
            Assert.AreEqual(1.75f, HuntingRules.ExperienceMultiplier(Wolf, xpPrey, Wolf.Id), 1e-5f);
            Assert.AreEqual(1f, HuntingRules.ExperienceMultiplier(Spider, xpPrey, Wolf.Id), 1e-5f);
            Assert.AreEqual(1f, HuntingRules.ExperienceMultiplier(Wolf, lootPrey, null), 1e-5f);

            Assert.AreEqual(0.75f, HuntingRules.ExtraLootChance(Wolf, lootPrey, Wolf.Id), 1e-5f);
            Assert.AreEqual(0f, HuntingRules.ExtraLootChance(Wolf, shieldPrey, null), 1e-5f);

            Assert.AreEqual(0.9f, HuntingRules.DamageTakenMultiplier(Wolf, shieldPrey, null), 1e-5f);
            Assert.AreEqual(0.82f, HuntingRules.DamageTakenMultiplier(Wolf, shieldPrey, new[] { HuntingCharmCatalog.DenteDeOnca }), 1e-5f);
            Assert.AreEqual(1f, HuntingRules.DamageTakenMultiplier(Spider, shieldPrey, new[] { HuntingCharmCatalog.DenteDeOnca }), 1e-5f);
        }

        [Test]
        public void Bonuses_NeverApplyToBosses()
        {
            var prey = new PreyOffer(AlphaWolf.Id, PreyBonusKind.Protection);
            Assert.AreEqual(1f, HuntingRules.ExperienceMultiplier(AlphaWolf, new PreyOffer(AlphaWolf.Id, PreyBonusKind.Experience), AlphaWolf.Id));
            Assert.AreEqual(0f, HuntingRules.ExtraLootChance(AlphaWolf, new PreyOffer(AlphaWolf.Id, PreyBonusKind.Loot), AlphaWolf.Id));
            Assert.AreEqual(1f, HuntingRules.DamageTakenMultiplier(AlphaWolf, prey, new[] { HuntingCharmCatalog.DenteDeOnca }));
            Assert.AreEqual(0f, HuntingCharmCatalog.DodgeChanceAgainst(new[] { HuntingCharmCatalog.FigaDeGuine }, attackerIsBoss: true));
            Assert.AreEqual(0f, HuntingCharmCatalog.HealOnKill(new[] { HuntingCharmCatalog.RaminhoDeArruda }, true, 100f));
            Assert.AreEqual(1f, HuntingCharmCatalog.StunMultiplier(new[] { HuntingCharmCatalog.NoDeVaqueiro }, true));
        }

        [Test]
        public void ExtraLootRolls_WholePartPlusFraction()
        {
            Assert.AreEqual(0, HuntingRules.ExtraLootRolls(0f, 0.0));
            Assert.AreEqual(1, HuntingRules.ExtraLootRolls(0.5f, 0.49));
            Assert.AreEqual(0, HuntingRules.ExtraLootRolls(0.5f, 0.5));
            Assert.AreEqual(1, HuntingRules.ExtraLootRolls(1f, 0.99));
            Assert.AreEqual(2, HuntingRules.ExtraLootRolls(1.25f, 0.1));
        }

        // ------------------------------------------------------------------ charms / points

        [Test]
        public void MilestonePoints_10_50_200()
        {
            Assert.AreEqual(0, HuntingRules.MilestonePointsFor(9));
            Assert.AreEqual(1, HuntingRules.MilestonePointsFor(10));
            Assert.AreEqual(3, HuntingRules.MilestonePointsFor(50));
            Assert.AreEqual(6, HuntingRules.MilestonePointsFor(5000));
            Assert.AreEqual(50, HuntingRules.NextMilestone(12));
            Assert.AreEqual(0, HuntingRules.NextMilestone(200));
            Assert.AreEqual(5, HuntingRules.TotalMilestonePoints(new[] { 10, 50, 0, 49 }));
        }

        [Test]
        public void CharmCatalog_CostsAndEffects()
        {
            Assert.AreEqual(7, HuntingCharmCatalog.All.Count);
            Assert.AreEqual(HuntingCharmCatalog.All.Count, HuntingCharmCatalog.All.Select(c => c.Id).Distinct().Count());
            Assert.AreEqual(32, HuntingCharmCatalog.TotalCost(HuntingCharmCatalog.All.Select(c => c.Id)));
            Assert.AreEqual(0, HuntingCharmCatalog.TotalCost(new[] { "nope" }));

            string[] worn = { HuntingCharmCatalog.CascaDeBesouro, HuntingCharmCatalog.FigaDeGuine, HuntingCharmCatalog.RaminhoDeArruda };
            Assert.AreEqual(0.08f, HuntingCharmCatalog.TypeProtectionBonus(worn, EnemyMotionArchetype.Arthropod), 1e-5f);
            Assert.AreEqual(0f, HuntingCharmCatalog.TypeProtectionBonus(worn, EnemyMotionArchetype.Quadruped));
            Assert.IsFalse(HuntingCharmCatalog.All.Any(c => c.Effect.ToString().Contains("Damage")), "no charm raises the hero's damage (seção Caça)");
            Assert.AreEqual(0.03f, HuntingCharmCatalog.DodgeChanceAgainst(worn, false), 1e-5f);
            Assert.AreEqual(0f, HuntingCharmCatalog.HealOnKill(worn, false, 100f), 1e-5f, "only the first two are on the cord");
            Assert.IsTrue(HuntingCharmCatalog.RollDodge(0.03f, 0.02));
            Assert.IsFalse(HuntingCharmCatalog.RollDodge(0.03f, 0.03));
            Assert.IsFalse(HuntingCharmCatalog.RollDodge(0f, 0.0));
            Assert.AreEqual(1.1f, HuntingCharmCatalog.StunMultiplier(new[] { HuntingCharmCatalog.NoDeVaqueiro }, false), 1e-5f);
        }

        [Test]
        public void Service_Charms_BuyWithMilestones_CordHoldsTwo()
        {
            HuntingProgress progress = NewProgress();
            Assert.IsFalse(progress.UnlockCharm(HuntingCharmCatalog.DenteDeOnca), "no points yet");

            _kills[Wolf.Id] = 200;  // 6
            _kills[Spider.Id] = 50; // 3
            _kills[AlphaWolf.Id] = 10; // 1 (bosses' pages count too)
            Assert.AreEqual(10, progress.CadernoPointsAvailable);

            Assert.IsTrue(progress.UnlockCharm(HuntingCharmCatalog.DenteDeOnca));
            Assert.IsTrue(progress.UnlockCharm(HuntingCharmCatalog.FigaDeGuine));
            Assert.AreEqual(0, progress.CadernoPointsAvailable);
            Assert.IsFalse(progress.UnlockCharm(HuntingCharmCatalog.DenteDeOnca), "bought once");
            CollectionAssert.AreEqual(new[] { HuntingCharmCatalog.DenteDeOnca, HuntingCharmCatalog.FigaDeGuine }, progress.EquippedCharms);

            _kills[Scorpion.Id] = 200;
            Assert.IsTrue(progress.UnlockCharm(HuntingCharmCatalog.CascaDeBesouro));
            Assert.IsFalse(progress.IsCharmEquipped(HuntingCharmCatalog.CascaDeBesouro), "cord full: bought but not worn");
            Assert.IsFalse(progress.EquipCharm(HuntingCharmCatalog.CascaDeBesouro));
            Assert.IsTrue(progress.UnequipCharm(HuntingCharmCatalog.FigaDeGuine));
            Assert.IsTrue(progress.EquipCharm(HuntingCharmCatalog.CascaDeBesouro));
        }

        [Test]
        public void Service_TradeHuntPoints_FiveForOne()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            Assert.IsFalse(progress.TradeHuntPoints());
            progress.TakeTask(Wolf.Id, HuntingTaskTier.Large);
            for (int i = 0; i < HuntingRules.LargeKills; i++)
            {
                progress.RecordKill(Wolf.Id);
            }

            Assert.AreEqual(8, progress.HuntPointsAvailable);
            Assert.IsTrue(progress.TradeHuntPoints());
            Assert.AreEqual(3, progress.HuntPointsAvailable);
            Assert.AreEqual(8, progress.HuntPointsEarned);
            Assert.AreEqual(1, progress.CadernoPointsAvailable);
            Assert.AreEqual(1, progress.HunterRank);
        }

        // ------------------------------------------------------------------ hooks

        [Test]
        public void Hooks_NeutralWithoutService_InstalledAndRemovedByIt()
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            try
            {
                Assert.AreEqual(1f, CombatModifierHooks.ResolveDamageTakenMultiplier(null, null));
                Assert.IsFalse(CombatModifierHooks.ShouldDodge(null, null));
                Assert.AreEqual(1f, EnemyRewardHooks.ResolveExperienceMultiplier(definition));
                Assert.AreEqual(0f, EnemyRewardHooks.ResolveExtraLootChance(definition));
                Assert.AreEqual(40, EnemyRewardHooks.ScaleExperience(definition, 40));

                EnemyRewardHooks.ExperienceMultiplier = _ => throw new InvalidOperationException();
                Assert.AreEqual(1f, EnemyRewardHooks.ResolveExperienceMultiplier(definition), "a throwing hook is ignored");
                EnemyRewardHooks.ExperienceMultiplier = _ => 1.5f;
                Assert.AreEqual(60, EnemyRewardHooks.ScaleExperience(definition, 40));
                EnemyRewardHooks.ClearAll();

                HuntingProgress progress = NewProgress();
                progress.Initialize();
                Assert.IsNotNull(EnemyRewardHooks.ExperienceMultiplier);
                Assert.IsNotNull(CombatModifierHooks.DamageTakenMultiplier);
                Assert.IsNotNull(CombatModifierHooks.DodgeCheck);
                Assert.IsNotNull(CombatModifierHooks.StunReceivedMultiplier);
                progress.Shutdown();
                Assert.IsNull(EnemyRewardHooks.ExperienceMultiplier);
                Assert.IsNull(CombatModifierHooks.DamageTakenMultiplier);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        // ------------------------------------------------------------------ save

        [Test]
        public void Save_RoundTripsThroughJson()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            _kills[Wolf.Id] = 200;
            progress.PickPrey(1);
            progress.TakeTask(Wolf.Id, HuntingTaskTier.Medium);
            progress.RecordKill(Wolf.Id);
            progress.RecordKill(Wolf.Id);
            progress.UnlockCharm(HuntingCharmCatalog.DenteDeOnca);

            string json = JsonUtility.ToJson(progress.CaptureState());
            HuntingProgress loaded = NewProgress();
            loaded.RestoreState(JsonUtility.FromJson<HuntingSaveState>(json));

            Assert.AreEqual(progress.DayKey, loaded.DayKey);
            Assert.AreEqual(1, loaded.PreyPickIndex);
            CollectionAssert.AreEqual(progress.PreyOffers.Select(o => o.enemyId + o.bonus), loaded.PreyOffers.Select(o => o.enemyId + o.bonus));
            Assert.AreEqual(Wolf.Id, loaded.ActiveTask.enemyId);
            Assert.AreEqual(2, loaded.ActiveTask.progress);
            Assert.AreEqual(HuntingTaskTier.Medium, loaded.ActiveTask.tier);
            CollectionAssert.AreEqual(new[] { HuntingCharmCatalog.DenteDeOnca }, loaded.UnlockedCharms);
            CollectionAssert.AreEqual(new[] { HuntingCharmCatalog.DenteDeOnca }, loaded.EquippedCharms);
            Assert.AreEqual(progress.CadernoPointsAvailable, loaded.CadernoPointsAvailable);
        }

        [Test]
        public void Save_EmptyOrOldState_LoadsClean_AndSanitizes()
        {
            HuntingProgress progress = NewProgress();
            progress.RestoreState(JsonUtility.FromJson<HuntingSaveState>("{}"));
            Assert.IsNull(progress.ActiveTask);
            Assert.AreEqual("2026-10-29", progress.DayKey);
            Assert.AreEqual(2, progress.PreyOffers.Count, "a new notebook: offers from the Floresta creatures");

            var bad = new HuntingSaveState
            {
                dayKey = "garbage",
                preyPick = 7,
                preyRerolls = 9,
                tasksCompletedToday = 99,
                huntPointsEarned = 3,
                huntPointsAvailable = 50,
                task = new HuntingTask { enemyId = Wolf.Id, required = 10, progress = 99, tier = HuntingTaskTier.Small },
                unlockedCharms = new List<string> { "unknown", HuntingCharmCatalog.FigaDeGuine },
                equippedCharms = new List<string> { HuntingCharmCatalog.DenteDeOnca, HuntingCharmCatalog.FigaDeGuine },
            };
            HuntingSaveState clean = HuntingProgress.Sanitize(bad);
            Assert.IsNull(clean.dayKey);
            Assert.AreEqual(-1, clean.preyPick);
            Assert.AreEqual(HuntingRules.MaxPreyRerollsPerDay, clean.preyRerolls);
            Assert.AreEqual(HuntingRules.MaxTasksPerDay, clean.tasksCompletedToday);
            Assert.AreEqual(3, clean.huntPointsAvailable);
            Assert.AreEqual(10, clean.task.progress);
            CollectionAssert.AreEqual(new[] { HuntingCharmCatalog.FigaDeGuine }, clean.unlockedCharms);
            CollectionAssert.AreEqual(new[] { HuntingCharmCatalog.FigaDeGuine }, clean.equippedCharms);
        }

        [Test]
        public void ResetForNewSession_ClearsEverything()
        {
            HuntingProgress progress = NewProgress();
            progress.Refresh();
            progress.TakeTask(Wolf.Id, HuntingTaskTier.Small);
            progress.PickPrey(0);
            progress.ResetForNewSession();
            Assert.IsNull(progress.ActiveTask);
            Assert.AreEqual(-1, progress.PreyPickIndex);
            Assert.AreEqual(0, progress.UnlockedCharms.Count);
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_CharmEffectsQuoteTheirValues_AndPresenterFormats()
        {
            foreach (HuntingCharmSpec charm in HuntingCharmCatalog.All)
            {
                StringAssert.Contains(HuntingTexts.Percent(charm.Value) + "%", HuntingTexts.GetFallback(charm.EffectKey), charm.Id);
            }

            Assert.AreEqual("+25% de XP", HuntingPresenter.PreyBonusText(null, PreyBonusKind.Experience).Replace(" ", " "));
            Assert.AreEqual("-10% de dano recebido", HuntingPresenter.PreyBonusText(null, PreyBonusKind.Protection));
            Assert.AreEqual("Pequena (10)", HuntingPresenter.TierButton(null, HuntingTaskTier.Small));
            Assert.AreEqual("Lobo: 3/10", HuntingPresenter.TrackerLine(null, new HuntingTask { progress = 3, required = 10 }, "Lobo"));
            Assert.AreEqual("Destaque do dia: Lobo (+50% de XP e saque)", HuntingPresenter.BoostedHudText(null, "Lobo"));
            Assert.IsNull(HuntingPresenter.BoostedHudText(null, null));
            Assert.AreEqual("Batedor", HuntingTexts.GetFallback(HuntingTexts.RankKey(1)));
        }
    }
}
