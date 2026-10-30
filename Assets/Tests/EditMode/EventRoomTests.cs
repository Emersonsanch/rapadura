using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.EventRooms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests.EditMode
{
    /// <summary>Timed event rooms (Terreiro das Assombrações / Torre do Engenho Velho): rules, progress, content.</summary>
    public class EventRoomTests
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
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        private static EventRoomBracket Bracket(int index) => EventRoomRules.Brackets[index];

        // ------------------------------------------------------------------
        // Schedule
        // ------------------------------------------------------------------

        [Test]
        public void Terreiro_OpensEveryFourHours_ForOneHour()
        {
            EventRoomSchedule s = EventRoomRules.TerreiroSchedule;
            EventRoomWindow open = EventRoomRules.WindowAt(s, 1, 0.5f);
            Assert.IsTrue(open.IsOpen);
            Assert.AreEqual(0, open.WindowIndex);
            Assert.AreEqual(0.5f, open.HoursRemaining, 1e-4f);
            Assert.AreEqual(1f, open.NextChangeHourOfDay, 1e-4f, "closes at 01:00");

            EventRoomWindow closed = EventRoomRules.WindowAt(s, 1, 1.5f);
            Assert.IsFalse(closed.IsOpen);
            Assert.AreEqual(2.5f, closed.HoursRemaining, 1e-4f);
            Assert.AreEqual(4f, closed.NextChangeHourOfDay, 1e-4f, "opens at 04:00");

            Assert.IsTrue(EventRoomRules.WindowAt(s, 1, 16.2f).IsOpen);
            Assert.IsFalse(EventRoomRules.WindowAt(s, 1, 17f).IsOpen, "the window end is exclusive");
            Assert.AreEqual(0f, EventRoomRules.WindowAt(s, 1, 23.5f).NextChangeHourOfDay, 1e-4f, "wraps to midnight");
        }

        [Test]
        public void WindowIndex_CountsAcrossDays()
        {
            Assert.AreEqual(6, EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 2, 0.1f).WindowIndex);
            Assert.AreEqual(6 * 9 + 2, EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 10, 8.5f).WindowIndex);
        }

        [Test]
        public void Torre_IsStaggered_RoomsNeverOverlap_AndEachOpensSixTimesADay()
        {
            Assert.IsFalse(EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 1, 1f).IsOpen);
            EventRoomWindow before = EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 1, 1f);
            Assert.AreEqual(2f, before.NextChangeHourOfDay, 1e-4f);
            Assert.AreEqual(-1, before.WindowIndex, "before the first opening");
            Assert.IsTrue(EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 1, 2f).IsOpen);

            int terreiro = 0, torre = 0;
            for (int step = 0; step < 24 * 12; step++)
            {
                float hour = step / 12f;
                bool a = EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 3, hour).IsOpen;
                bool b = EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 3, hour).IsOpen;
                Assert.IsFalse(a && b, $"overlap at {hour}");
                terreiro += a ? 1 : 0;
                torre += b ? 1 : 0;
            }

            Assert.AreEqual(6 * 12, terreiro);
            Assert.AreEqual(6 * 12, torre);
        }

        [Test]
        public void OneEntryPerOpening()
        {
            EventRoomWindow open = EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 1, 4.2f);
            Assert.IsTrue(EventRoomRules.CanEnterWindow(open, -1));
            Assert.IsFalse(EventRoomRules.CanEnterWindow(open, open.WindowIndex));
            Assert.IsTrue(EventRoomRules.CanEnterWindow(open, open.WindowIndex - 1));
            Assert.IsFalse(EventRoomRules.CanEnterWindow(EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 1, 5.5f), -1));
        }

        [Test]
        public void InvalidSchedule_IsNeverOpen()
        {
            Assert.IsFalse(new EventRoomSchedule(4f, 0f, 4f).IsValid);
            Assert.IsFalse(EventRoomRules.WindowAt(new EventRoomSchedule(0f, 0f, 1f), 1, 0.5f).IsOpen);
        }

        // ------------------------------------------------------------------
        // Brackets and scaling
        // ------------------------------------------------------------------

        [TestCase(1, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 1)]
        [TestCase(8, 1)]
        [TestCase(9, 2)]
        [TestCase(13, 3)]
        [TestCase(15, 3)]
        [TestCase(16, 4)]
        [TestCase(40, 4)]
        [TestCase(0, 0)]
        public void Brackets_CoverEveryLevel(int level, int bracket)
        {
            Assert.AreEqual(bracket, EventRoomRules.BracketFor(level).Index);
        }

        [Test]
        public void EnemyScale_BringsEveryTierToTheBracket()
        {
            EventRoomEnemyScale same = EventRoomRules.EnemyScale(2, Bracket(2));
            Assert.AreEqual(1f, same.HealthMultiplier, 1e-4f);
            Assert.AreEqual(0f, same.DamageBonus, 1e-4f);
            Assert.AreEqual(1f, same.ExperienceMultiplier, 1e-4f);

            EventRoomEnemyScale wolfInCaves = EventRoomRules.EnemyScale(0, Bracket(3));
            Assert.AreEqual(170f / 70f, wolfInCaves.HealthMultiplier, 1e-3f);
            Assert.AreEqual(4f, wolfInCaves.ExperienceMultiplier, 1e-3f);
            Assert.Greater(wolfInCaves.DamageBonus, 0f);

            EventRoomEnemyScale batInForest = EventRoomRules.EnemyScale(3, Bracket(0));
            Assert.AreEqual(70f / 170f, batInForest.HealthMultiplier, 1e-3f);
            Assert.AreEqual(0.25f, batInForest.ExperienceMultiplier, 1e-3f, "no cheap caves XP for a level-3 hero");
            Assert.Less(batInForest.DamageBonus, 0f);

            for (int tier = 0; tier < 4; tier++)
            {
                for (int b = 0; b < EventRoomRules.Brackets.Count; b++)
                {
                    EventRoomEnemyScale scale = EventRoomRules.EnemyScale(tier, Bracket(b));
                    Assert.That(scale.HealthMultiplier, Is.InRange(EventRoomRules.MinHealthMultiplier, EventRoomRules.MaxHealthMultiplier));
                    Assert.That(scale.DamageBonus, Is.InRange(EventRoomRules.MinDamageBonus, EventRoomRules.MaxDamageBonus));
                    // Effective health and XP land on the bracket's reference regardless of the native region.
                    Assert.AreEqual(EventRoomRules.TierExperience[Bracket(b).ReferenceTier] * Bracket(b).ExtraMultiplier,
                        EventRoomRules.TierExperience[tier] * scale.ExperienceMultiplier, 1e-2f);
                }
            }
        }

        [Test]
        public void Guardian_IsAFourTimesTougherRegularEnemy()
        {
            EventRoomEnemyScale guardian = EventRoomRules.GuardianScale(2, Bracket(2));
            Assert.AreEqual(4f, guardian.HealthMultiplier, 1e-4f);
            Assert.AreEqual(4f, guardian.ExperienceMultiplier, 1e-4f);
            Assert.AreEqual(EventRoomRules.GuardianExtraDamageBonus, guardian.DamageBonus, 1e-4f);
            Assert.AreEqual(1.5f, guardian.VisualScale, 1e-4f);
            Assert.AreEqual(14, EventRoomRules.ScaleExperience(28, 0.5f));
            Assert.AreEqual(1, EventRoomRules.ScaleExperience(1, 0.1f), "never zero");
            Assert.AreEqual(0, EventRoomRules.ScaleExperience(0, 3f));
        }

        [Test]
        public void TierTables_MatchTheRegularEnemyAssets()
        {
            string[] regulars = { "enemy_forest_wolf", "enemy_desert_scorpion", "enemy_mountain_stone_golem", "enemy_cave_bat_swarm" };
            for (int tier = 0; tier < regulars.Length; tier++)
            {
                EnemyDefinition enemy = Resources.Load<EnemyDefinition>("Enemies/" + regulars[tier]);
                Assert.IsNotNull(enemy, regulars[tier]);
                Assert.IsFalse(enemy.IsBoss);
                Assert.AreEqual(EventRoomRules.TierHealth[tier], enemy.MaxHealth, 1e-3f, regulars[tier]);
                Assert.AreEqual(EventRoomRules.TierExperience[tier], enemy.ExperienceReward, regulars[tier]);
            }

            EnemyDefinition guardian = Resources.Load<EnemyDefinition>("Enemies/" + EventRoomRules.GuardianEnemyId);
            Assert.IsNotNull(guardian);
            Assert.IsFalse(guardian.IsBoss, "no boss bookkeeping for the room's guardian");
        }

        [Test]
        public void Pool_HasEveryRegionsRegularEnemies_AndNoBoss()
        {
            List<EnemyDefinition> pool = EventRoomEntry.BuildPool(BiomeRegistry.GetAll());
            Assert.GreaterOrEqual(pool.Count, 4);
            Assert.IsFalse(pool.Any(e => e.IsBoss));
            Assert.AreEqual(pool.Count, pool.Distinct().Count());
            var tiers = new HashSet<BiomeType>(pool.Select(e => BiomeRegistry.FindBiomeOf(e).Type));
            Assert.AreEqual(4, tiers.Count, "mixed enemies from all biomes");
            Assert.IsTrue(pool.Any(e => e.name == EventRoomRules.GuardianEnemyId));
        }

        // ------------------------------------------------------------------
        // Waves
        // ------------------------------------------------------------------

        [Test]
        public void Waves_GrowAndRespectTheCap()
        {
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6, 6 }, Enumerable.Range(0, 5).Select(EventRoomRules.TerreiroWaveSize).ToArray());
            CollectionAssert.AreEqual(new[] { 2, 3, 4, 4 }, Enumerable.Range(0, 4).Select(EventRoomRules.TorreWaveSize).ToArray());
            Assert.IsTrue(EventRoomRules.ShouldSpawnWave(1, 2f, 20f, 8), "field almost clear");
            Assert.IsFalse(EventRoomRules.ShouldSpawnWave(3, 2f, 20f, 8));
            Assert.IsTrue(EventRoomRules.ShouldSpawnWave(3, 20f, 20f, 8), "interval passed");
            Assert.IsFalse(EventRoomRules.ShouldSpawnWave(8, 99f, 20f, 8), "cap");
            Assert.AreEqual(2, EventRoomRules.FitWave(5, 6, 8));
            Assert.AreEqual(0, EventRoomRules.FitWave(5, 9, 8));
        }

        // ------------------------------------------------------------------
        // Score and combo
        // ------------------------------------------------------------------

        [Test]
        public void Combo_ChainsWithinTheWindow_CapsAtTwo_AndBreaks()
        {
            var score = new EventRoomScore();
            Assert.AreEqual(100, score.RegisterKill(0f, 100));
            Assert.AreEqual(110, score.RegisterKill(3f, 100));
            Assert.AreEqual(120, score.RegisterKill(6.9f, 100));
            Assert.AreEqual(3, score.Combo);
            Assert.IsTrue(score.IsComboAlive(10f));
            Assert.IsFalse(score.IsComboAlive(11.5f));
            Assert.AreEqual(100, score.RegisterKill(12f, 100), "broken: back to ×1");
            Assert.AreEqual(3, score.BestCombo);
            Assert.AreEqual(4, score.Kills);

            for (int i = 0; i < 20; i++)
            {
                score.RegisterKill(12f + i, 100);
            }

            Assert.AreEqual(2f, EventRoomRules.ComboMultiplier(score.Combo));
            Assert.AreEqual(2f, EventRoomRules.ComboMultiplier(11));
            Assert.AreEqual(1.9f, EventRoomRules.ComboMultiplier(10), 1e-4f);
            score.AddBonus(-50);
            int total = score.Total;
            score.AddBonus(500);
            Assert.AreEqual(total + 500, score.Total);
            Assert.AreEqual(450, EventRoomRules.TorreTimeBonus(45.9f));
        }

        // ------------------------------------------------------------------
        // Tiers and rewards
        // ------------------------------------------------------------------

        [Test]
        public void Tiers_ByScore_TorreWithoutMoendaCapsAtBronze()
        {
            Assert.AreEqual(EventRoomTier.None, EventRoomRules.TierFor(999, EventRoomKind.Terreiro, false));
            Assert.AreEqual(EventRoomTier.Bronze, EventRoomRules.TierFor(1000, EventRoomKind.Terreiro, false));
            Assert.AreEqual(EventRoomTier.Prata, EventRoomRules.TierFor(2500, EventRoomKind.Terreiro, false));
            Assert.AreEqual(EventRoomTier.Ouro, EventRoomRules.TierFor(4500, EventRoomKind.Terreiro, true));
            Assert.AreEqual(EventRoomTier.Mestre, EventRoomRules.TierFor(9000, EventRoomKind.Terreiro, false), "dying early has no cap");
            Assert.AreEqual(EventRoomTier.Bronze, EventRoomRules.TierFor(9000, EventRoomKind.Torre, false));
            Assert.AreEqual(EventRoomTier.Mestre, EventRoomRules.TierFor(9000, EventRoomKind.Torre, true));
            Assert.AreEqual(EventRoomRules.TierThresholds.Last(), EventRoomRules.HighScoreAchievementTarget);
        }

        [Test]
        public void Rewards_GrowWithTierAndBracket()
        {
            EventRoomReward none = EventRoomRules.RewardFor(EventRoomTier.None, Bracket(0));
            Assert.IsTrue(none.IsEmpty);

            EventRoomReward bronze = EventRoomRules.RewardFor(EventRoomTier.Bronze, Bracket(0));
            Assert.AreEqual(50, bronze.Experience);
            Assert.AreEqual(8, bronze.Gold);
            Assert.AreEqual(0, bronze.BlessingStones);

            EventRoomReward master = EventRoomRules.RewardFor(EventRoomTier.Mestre, Bracket(4));
            Assert.AreEqual(1300, master.Experience);
            Assert.AreEqual(120, master.Gold);
            Assert.AreEqual(2, master.BlessingStones);
            Assert.AreEqual(1, master.SoulStones);

            for (int b = 0; b < EventRoomRules.Brackets.Count; b++)
            {
                for (int t = 1; t < 4; t++)
                {
                    EventRoomReward lower = EventRoomRules.RewardFor((EventRoomTier)t, Bracket(b));
                    EventRoomReward higher = EventRoomRules.RewardFor((EventRoomTier)(t + 1), Bracket(b));
                    Assert.Greater(higher.Experience, lower.Experience);
                    Assert.Greater(higher.Gold, lower.Gold);
                }
            }
        }

        [Test]
        public void Cleared_MeansSurvivingOrDelivering()
        {
            Assert.IsTrue(EventRoomRules.IsCleared(EventRoomKind.Terreiro, true, false));
            Assert.IsFalse(EventRoomRules.IsCleared(EventRoomKind.Terreiro, false, true));
            Assert.IsTrue(EventRoomRules.IsCleared(EventRoomKind.Torre, false, true));
            Assert.IsFalse(EventRoomRules.IsCleared(EventRoomKind.Torre, true, false));
        }

        [Test]
        public void RefiningStoneIds_ExistAsItems()
        {
            Assert.IsNotNull(ItemDatabase.GetById(EventRoomRules.BlessingStoneItemId));
            Assert.IsNotNull(ItemDatabase.GetById(EventRoomRules.SoulStoneItemId));
        }

        // ------------------------------------------------------------------
        // Torre phases and gate
        // ------------------------------------------------------------------

        [Test]
        public void Torre_PhasesFollowTheOrder()
        {
            Assert.AreEqual(TorrePhase.OpenBridge, EventRoomRules.TorrePhaseFor(new TorreProgress(9, true, true, true, true)), "nothing skips the bridge");
            Assert.AreEqual(TorrePhase.BreakGate, EventRoomRules.TorrePhaseFor(new TorreProgress(10, false, false, false, false)));
            Assert.AreEqual(TorrePhase.Guardian, EventRoomRules.TorrePhaseFor(new TorreProgress(12, true, false, false, false)));
            Assert.AreEqual(TorrePhase.PickUpMoenda, EventRoomRules.TorrePhaseFor(new TorreProgress(12, true, true, false, false)));
            Assert.AreEqual(TorrePhase.Deliver, EventRoomRules.TorrePhaseFor(new TorreProgress(12, true, true, true, false)));
            Assert.AreEqual(TorrePhase.Done, EventRoomRules.TorrePhaseFor(new TorreProgress(12, true, true, true, true)));
            Assert.IsTrue(EventRoomRules.GateAcceptsDamage(TorrePhase.BreakGate));
            Assert.IsFalse(EventRoomRules.GateAcceptsDamage(TorrePhase.OpenBridge));
            Assert.AreEqual(420f, EventRoomRules.GateHealth(Bracket(0)), 1e-3f);
        }

        [Test]
        public void Gate_TakesDamageOnlyWhenOpen_AndBreaksOnce()
        {
            var go = new GameObject("gate");
            _created.Add(go);
            go.AddComponent<BoxCollider>();
            EventRoomGate gate = go.AddComponent<EventRoomGate>();
            gate.Configure(100f);
            int broken = 0;
            gate.Broken += () => broken++;

            gate.ApplyDamage(60f, default, null);
            Assert.AreEqual(100f, gate.CurrentHealth, "locked before the bridge");

            gate.AcceptsDamage = true;
            gate.ApplyDamage(60f, default, null);
            Assert.AreEqual(0.4f, gate.Fraction, 1e-4f);
            gate.ApplyDamage(60f, default, null);
            gate.ApplyDamage(60f, default, null);
            Assert.IsTrue(gate.IsBroken);
            Assert.IsFalse(gate.IsAlive);
            Assert.AreEqual(1, broken);
            Assert.IsFalse(go.GetComponent<BoxCollider>().enabled);
        }

        // ------------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------------

        [Test]
        public void Entry_ChecksWindowLevelAndItem()
        {
            EventRoomWindow open = EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 1, 2.5f);
            EventRoomWindow closed = EventRoomRules.WindowAt(EventRoomRules.TorreSchedule, 1, 4.5f);
            Assert.AreEqual(EventRoomEntryBlock.Closed, EventRoomEntry.CheckEntry(closed, -1, 20, EventRoomKind.Torre, 5));
            Assert.AreEqual(EventRoomEntryBlock.AlreadyEntered, EventRoomEntry.CheckEntry(open, open.WindowIndex, 20, EventRoomKind.Torre, 5));
            Assert.AreEqual(EventRoomEntryBlock.Level, EventRoomEntry.CheckEntry(open, -1, EventRoomRules.TorreMinLevel - 1, EventRoomKind.Torre, 5));
            Assert.AreEqual(EventRoomEntryBlock.NoItem, EventRoomEntry.CheckEntry(open, -1, 20, EventRoomKind.Torre, 0));
            Assert.AreEqual(EventRoomEntryBlock.None, EventRoomEntry.CheckEntry(open, -1, 20, EventRoomKind.Torre, 1));
        }

        [Test]
        public void EntryDrop_RareOutsideRooms_NeverInside()
        {
            Assert.AreEqual(EventRoomKind.Terreiro, EventRoomRules.RollEntryDrop(false, false, 10, 0.01, 0.9));
            Assert.AreEqual(EventRoomKind.None, EventRoomRules.RollEntryDrop(false, false, 10, 0.02, 0.9));
            Assert.AreEqual(EventRoomKind.Torre, EventRoomRules.RollEntryDrop(false, false, 10, 0.5, 0.005));
            Assert.AreEqual(EventRoomKind.None, EventRoomRules.RollEntryDrop(false, false, 4, 0.5, 0.005), "Torre key only from its min level");
            Assert.AreEqual(EventRoomKind.Terreiro, EventRoomRules.RollEntryDrop(true, false, 10, 0.2, 0.9));
            Assert.AreEqual(EventRoomKind.None, EventRoomRules.RollEntryDrop(true, true, 10, 0.0, 0.0), "no farming inside");
            Assert.AreEqual(EventRoomKind.None, EventRoomRules.RollEntryDrop(true, false, 1, 0.0, 0.0), "not before level 3");
        }

        [Test]
        public void Payload_CarriesTheRoom_ThroughJson()
        {
            List<EnemyDefinition> pool = EventRoomEntry.BuildPool(BiomeRegistry.GetAll());
            EncounterPayload payload = EventRoomEntry.BuildPayload(EventRoomKind.Torre, BiomeType.Desert, 11, 42, pool);
            Assert.IsTrue(payload.IsEventRoom);
            Assert.IsFalse(payload.IsBossChallenge);
            Assert.IsNull(payload.FindBoss(), "boss music stays off");
            EncounterPayload back = EncounterPayload.FromJson(payload.ToJson());
            Assert.AreEqual(EventRoomKind.Torre, back.eventRoom);
            Assert.AreEqual(11, back.eventRoomLevel);
            Assert.AreEqual(pool.Count, back.EnemyCount);
            Assert.IsNull(EventRoomEntry.BuildPayload(EventRoomKind.None, BiomeType.Desert, 11, 42, pool));
            Assert.IsFalse(EncounterPayload.Create(BiomeType.Forest, pool, 1).IsEventRoom);
        }

        // ------------------------------------------------------------------
        // Progress (save, bests, achievements)
        // ------------------------------------------------------------------

        private AchievementDefinition Definition(string id, AchievementConditionType type, int target, string[] ids)
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests(id, type, target, ids);
            return definition;
        }

        [Test]
        public void RecordRun_KeepsBests_FirstClear_AndFeedsAchievements()
        {
            AchievementDefinition terreiro = Definition("t_terreiro", AchievementConditionType.EventRoomCleared, 1, new[] { "terreiro" });
            AchievementDefinition torre = Definition("t_torre", AchievementConditionType.EventRoomCleared, 1, new[] { "torre" });
            AchievementDefinition master = Definition("t_master", AchievementConditionType.EventRoomBestScore, 7000, new string[0]);
            var achievements = new AchievementManager(new[] { terreiro, torre, master });
            var progress = new EventRoomProgress { Achievements = achievements };
            int events = 0;
            EventBus.Subscribe<EventRoomFinishedEvent>(_ => events++);

            EventRoomRunOutcome first = progress.RecordRun(EventRoomKind.Terreiro, 3000, EventRoomTier.Prata, false);
            Assert.IsTrue(first.NewBest);
            Assert.IsFalse(first.FirstClear);
            Assert.IsFalse(achievements.IsUnlocked("t_terreiro"));

            EventRoomRunOutcome second = progress.RecordRun(EventRoomKind.Terreiro, 2000, EventRoomTier.Bronze, true);
            Assert.IsFalse(second.NewBest);
            Assert.IsTrue(second.FirstClear);
            Assert.AreEqual(3000, second.BestScore);
            Assert.IsTrue(achievements.IsUnlocked("t_terreiro"));
            Assert.IsFalse(achievements.IsUnlocked("t_torre"));
            Assert.IsFalse(progress.RecordRun(EventRoomKind.Terreiro, 100, EventRoomTier.None, true).FirstClear);

            progress.RecordRun(EventRoomKind.Torre, 7200, EventRoomTier.Mestre, true);
            Assert.IsTrue(achievements.IsUnlocked("t_torre"));
            Assert.IsTrue(achievements.IsUnlocked("t_master"));
            Assert.AreEqual(EventRoomTier.Prata, progress.BestTier(EventRoomKind.Terreiro));
            Assert.AreEqual(3, progress.Runs(EventRoomKind.Terreiro));
            Assert.AreEqual(2, progress.Clears(EventRoomKind.Terreiro));
            Assert.AreEqual(4, events);
        }

        [Test]
        public void Save_RoundTripsRecordsWindowsAndSchedule()
        {
            var progress = new EventRoomProgress();
            progress.RecordRun(EventRoomKind.Torre, 5100, EventRoomTier.Ouro, true);
            progress.MarkEntered(EventRoomKind.Terreiro, 17);
            Assert.IsTrue(progress.SetSchedule(EventRoomKind.Torre, new EventRoomSchedule(6f, 1f, 2f)));
            Assert.IsFalse(progress.SetSchedule(EventRoomKind.Torre, new EventRoomSchedule(2f, 0f, 3f)), "invalid ignored");

            string json = JsonUtility.ToJson(progress.CaptureState());
            var loaded = new EventRoomProgress();
            loaded.RestoreState(JsonUtility.FromJson<EventRoomSaveState>(json));
            Assert.AreEqual(5100, loaded.BestScore(EventRoomKind.Torre));
            Assert.AreEqual(EventRoomTier.Ouro, loaded.BestTier(EventRoomKind.Torre));
            Assert.AreEqual(1, loaded.Clears(EventRoomKind.Torre));
            Assert.AreEqual(17, loaded.LastEnteredWindow(EventRoomKind.Terreiro));
            Assert.AreEqual(-1, loaded.LastEnteredWindow(EventRoomKind.Torre));
            Assert.AreEqual(6f, loaded.GetSchedule(EventRoomKind.Torre).PeriodHours);
            Assert.AreEqual(2f, loaded.GetSchedule(EventRoomKind.Torre).WindowHours);
            Assert.IsTrue(loaded.WindowAt(EventRoomKind.Torre, 1, 7.5f).IsOpen);

            loaded.ResetForNewSession();
            Assert.AreEqual(0, loaded.BestScore(EventRoomKind.Torre));
            Assert.AreEqual(EventRoomRules.TorreSchedule.PeriodHours, loaded.GetSchedule(EventRoomKind.Torre).PeriodHours);

            loaded.RestoreState(null);
            loaded.RestoreState(new EventRoomSaveState { rooms = null, schedules = null });
            Assert.AreEqual(0, loaded.Runs(EventRoomKind.Terreiro), "old saves load empty");
        }

        [Test]
        public void CanEnter_UsesTheEnteredWindow()
        {
            var progress = new EventRoomProgress();
            EventRoomWindow open = progress.WindowAt(EventRoomKind.Terreiro, 1, 8.5f);
            Assert.IsTrue(progress.CanEnter(EventRoomKind.Terreiro, open));
            progress.MarkEntered(EventRoomKind.Terreiro, open.WindowIndex);
            Assert.IsFalse(progress.CanEnter(EventRoomKind.Terreiro, open));
            Assert.IsTrue(progress.CanEnter(EventRoomKind.Terreiro, progress.WindowAt(EventRoomKind.Terreiro, 1, 12.5f)));
        }

        // ------------------------------------------------------------------
        // Content
        // ------------------------------------------------------------------

        [Test]
        public void EntryItems_ExistAndMatchTheirTexts()
        {
            ItemDefinition ticket = ItemDatabase.GetById(EventRoomRules.TerreiroTicketItemId);
            ItemDefinition key = ItemDatabase.GetById(EventRoomRules.TorreKeyItemId);
            Assert.IsNotNull(ticket);
            Assert.IsNotNull(key);
            Assert.AreEqual(EventRoomTexts.TicketNamePt, ticket.DisplayName);
            Assert.AreEqual(EventRoomTexts.TicketDescriptionPt, ticket.Description);
            Assert.AreEqual(EventRoomTexts.KeyNamePt, key.DisplayName);
            Assert.AreEqual(EventRoomTexts.KeyDescriptionPt, key.Description);
        }

        [Test]
        public void EntryItems_AreSoldByThePeddler()
        {
            CityShopSpec mascate = CityShopCatalog.Get(CityShopCatalog.MascateBento);
            Assert.IsTrue(mascate.Entries.Any(e => e.ItemId == EventRoomRules.TerreiroTicketItemId));
            Assert.IsTrue(mascate.Entries.Any(e => e.ItemId == EventRoomRules.TorreKeyItemId));
        }

        [Test]
        public void Achievements_AreSeeded()
        {
            AchievementDefinition[] seeded = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);
            AchievementDefinition terreiro = seeded.Single(d => d.AchievementId == "ach_room_terreiro_clear");
            AchievementDefinition torre = seeded.Single(d => d.AchievementId == "ach_room_torre_clear");
            AchievementDefinition master = seeded.Single(d => d.AchievementId == "ach_room_master");
            Assert.AreEqual(AchievementConditionType.EventRoomCleared, terreiro.ConditionType);
            CollectionAssert.AreEqual(new[] { "terreiro" }, terreiro.TargetIds.ToArray());
            CollectionAssert.AreEqual(new[] { "torre" }, torre.TargetIds.ToArray());
            Assert.AreEqual(AchievementConditionType.EventRoomBestScore, master.ConditionType);
            Assert.AreEqual(EventRoomRules.HighScoreAchievementTarget, master.TargetValue);
        }

        [Test]
        public void EveryCousin_HasALine_ForEveryRoomAndMoment()
        {
            var keys = new HashSet<string>(EventRoomTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(EventRoomTexts.Entries.Count, keys.Count, "duplicate keys");
            foreach (EventRoomKind kind in EventRoomProgress.Rooms)
            {
                foreach (EventRoomMoment moment in new[] { EventRoomMoment.Start, EventRoomMoment.Win, EventRoomMoment.Lose })
                {
                    foreach (CharacterId hero in System.Enum.GetValues(typeof(CharacterId)))
                    {
                        LocalizationEntry entry = EventRoomTexts.Entries.SingleOrDefault(e => e.Key == EventRoomTexts.LineKey(kind, moment, hero));
                        Assert.IsNotNull(entry, $"{kind} {moment} {hero}");
                        Assert.IsNotEmpty(entry.En);
                        Assert.IsNotEmpty(entry.PtBR);
                    }
                }
            }
        }

        [Test]
        public void MapAndHud_Format()
        {
            Assert.AreEqual("16:00", EventRoomMapButton.FormatHour(16f));
            Assert.AreEqual("01:30", EventRoomMapButton.FormatHour(25.5f));
            Assert.AreEqual("00:00", EventRoomMapButton.FormatHour(23.9999f));
            Assert.AreEqual("3:00", EventRoomHud.FormatClock(180f));
            Assert.AreEqual("0:01", EventRoomHud.FormatClock(0.2f));
            Assert.AreEqual("0:00", EventRoomHud.FormatClock(-1f));

            EventRoomWindow open = EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 1, 16.5f);
            StringAssert.Contains("ABERTO até 17:00", EventRoomMapButton.PillLine(null, EventRoomKind.Terreiro, open, false));
            StringAssert.Contains("20:00", EventRoomMapButton.PillLine(null, EventRoomKind.Terreiro, EventRoomRules.WindowAt(EventRoomRules.TerreiroSchedule, 1, 17.5f), false));
            StringAssert.Contains("Convite do Terreiro", EventRoomMapButton.BlockMessage(null, EventRoomEntryBlock.NoItem, EventRoomKind.Terreiro));
            StringAssert.Contains("6", EventRoomMapButton.BlockMessage(null, EventRoomEntryBlock.Level, EventRoomKind.Torre));
        }

        [Test]
        public void Results_ListTierRewardsAndTheCousin()
        {
            var result = new EventRoomResult
            {
                Kind = EventRoomKind.Torre,
                Cleared = true,
                Score = 5200,
                Kills = 14,
                BestCombo = 6,
                Tier = EventRoomTier.Ouro,
                Reward = EventRoomRules.RewardFor(EventRoomTier.Ouro, Bracket(1)),
                BestScore = 5200,
                NewBest = true,
                BlessingStonesGranted = 2,
                CousinLine = "Oi",
            };

            List<string> lines = EventRoomHud.ResultLines(null, result);
            StringAssert.Contains("Torre do Engenho Velho", lines[0]);
            StringAssert.Contains("Ouro", lines[1]);
            Assert.IsTrue(lines.Any(l => l.Contains("Nova melhor marca")));
            Assert.IsTrue(lines.Any(l => l.Contains("Pedra da Bênção")));
            StringAssert.Contains("Oi", lines.Last());

            VisualElementCheck();
        }

        private static void VisualElementCheck()
        {
            VisualElement view = EventRoomHud.CreateView();
            Assert.IsNotNull(view.Q("er-hud-timer"));
            Assert.IsNotNull(view.Q("er-results-card"));
            Assert.AreEqual(PickingMode.Ignore, view.pickingMode);
        }

        [Test]
        public void Scaler_AppliesHealthOnce_AndRescalesKillXp()
        {
            var go = new GameObject("enemy");
            _created.Add(go);
            go.AddComponent<Rapadura.Gameplay.Combat.Health>().ConfigureMaxHealth(100f);
            EventRoomEnemyScaler scaler = EventRoomEnemyScaler.Attach(go, new EventRoomEnemyScale(2f, 0f, 3f, 1f), applyNow: true);
            scaler.ApplyNow();
            Assert.AreEqual(200f, go.GetComponent<Rapadura.Gameplay.Combat.Health>().MaxHealth, 1e-3f);
            Assert.AreEqual(30, ((IKillExperienceScaler)scaler).ScaleKillExperience(10));
        }
    }
}
