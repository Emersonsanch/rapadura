using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.WorldEvents;
using Rapadura.UI.WorldEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests.EditMode
{
    /// <summary>World bosses and Invasão do Cangaço (Docs/WorldEvents.md): calendar, rules, rewards, progress, texts, view.</summary>
    public class WorldEventsTests
    {
        private readonly List<Object> _created = new List<Object>();

        /// <summary>Server-local (Brasília, UTC−3) time as UTC.</summary>
        private static DateTime Brt(int year, int month, int day, int hour, int minute, int second = 0)
        {
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddHours(3);
        }

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
            WorldEventCalendar.UtcNow = () => DateTime.UtcNow;
        }

        // ------------------------------------------------------------------
        // Calendar
        // ------------------------------------------------------------------

        [Test]
        public void Calendar_BossWindowsFollowServerLocalTime()
        {
            WorldBossDefinition forest = WorldBossRules.For(BiomeType.Forest);
            RealTimeWindow open = WorldBossRules.WindowAt(forest, Brt(2026, 10, 30, 9, 10));
            Assert.IsTrue(open.IsOpen);
            Assert.AreEqual(Brt(2026, 10, 30, 9, 0), open.StartUtc);
            Assert.AreEqual(20 * 60, open.SecondsToChange, 0.5);

            RealTimeWindow closed = WorldBossRules.WindowAt(forest, Brt(2026, 10, 30, 9, 40));
            Assert.IsFalse(closed.IsOpen);
            Assert.AreEqual(Brt(2026, 10, 30, 12, 0), closed.StartUtc);
            Assert.AreEqual(open.Index + 1, closed.Index);

            RealTimeWindow desert = WorldBossRules.WindowAt(WorldBossRules.For(BiomeType.Desert), Brt(2026, 10, 30, 9, 50));
            Assert.IsTrue(desert.IsOpen, "Desert opens 45 min after the Forest");
        }

        [Test]
        public void Calendar_SomeBossEvery45Minutes_NeverTwoAtOnce()
        {
            DateTime start = Brt(2026, 10, 30, 0, 0);
            for (int minute = 0; minute < 24 * 60; minute += 5)
            {
                DateTime t = start.AddMinutes(minute);
                int open = WorldBossRules.All.Count(b => WorldBossRules.WindowAt(b, t).IsOpen);
                Assert.LessOrEqual(open, 1, $"two bosses at {t}");
            }

            Assert.AreEqual(4, WorldBossRules.All.Select(b => b.OffsetMinutes).Distinct().Count());
            Assert.AreEqual(4, WorldBossRules.All.Select(b => b.Region).Distinct().Count());
        }

        [Test]
        public void Calendar_NightForm_IsDecidedAtWindowStart()
        {
            WorldBossDefinition forest = WorldBossRules.For(BiomeType.Forest);
            Assert.IsFalse(WorldBossRules.IsNightWindow(WorldBossRules.WindowAt(forest, Brt(2026, 10, 30, 15, 5))));
            Assert.IsTrue(WorldBossRules.IsNightWindow(WorldBossRules.WindowAt(forest, Brt(2026, 10, 30, 18, 5))));
            Assert.IsTrue(WorldBossRules.IsNightWindow(WorldBossRules.WindowAt(forest, Brt(2026, 10, 31, 3, 25))));
            Assert.IsFalse(WorldBossRules.IsNightWindow(WorldBossRules.WindowAt(forest, Brt(2026, 10, 31, 6, 10))));
        }

        [Test]
        public void Calendar_InvasionRotatesRegions_SixADay()
        {
            RealTimeWindow first = InvasionRules.WindowAt(Brt(2026, 10, 30, 2, 10));
            Assert.IsTrue(first.IsOpen);
            Assert.IsFalse(InvasionRules.WindowAt(Brt(2026, 10, 30, 2, 40)).IsOpen);
            var regions = new HashSet<BiomeType>();
            for (int i = 0; i < 4; i++)
            {
                regions.Add(InvasionRules.RegionFor(first.Index + i));
            }

            Assert.AreEqual(4, regions.Count);
            Assert.AreEqual(InvasionRules.RegionFor(first.Index), InvasionRules.RegionFor(first.Index + 4));
        }

        [Test]
        public void Calendar_RewardSeedIsDeterministic()
        {
            int a = WorldEventCalendar.RewardSeed("world_boss", "x", 10, "hero:maria");
            Assert.AreEqual(a, WorldEventCalendar.RewardSeed("world_boss", "x", 10, "hero:maria"));
            Assert.AreNotEqual(a, WorldEventCalendar.RewardSeed("world_boss", "x", 11, "hero:maria"));
            Assert.AreNotEqual(a, WorldEventCalendar.RewardSeed("world_boss", "x", 10, "hero:lavine"));
            Assert.GreaterOrEqual(a, 0);
        }

        [Test]
        public void Calendar_FormatCountdown()
        {
            Assert.AreEqual("12:05", WorldEventCalendar.FormatCountdown(725));
            Assert.AreEqual("0:01", WorldEventCalendar.FormatCountdown(0.2));
            Assert.AreEqual("0:00", WorldEventCalendar.FormatCountdown(-5));
            Assert.AreEqual("2h15", WorldEventCalendar.FormatCountdown(2 * 3600 + 14 * 60 + 10));
        }

        // ------------------------------------------------------------------
        // Boss rules
        // ------------------------------------------------------------------

        [Test]
        public void Boss_ScaleIsAboutTwiceTheRegionBoss_NightTougher()
        {
            EventRoomEnemyScale day = WorldBossRules.Scale(false);
            EventRoomEnemyScale night = WorldBossRules.Scale(true);
            Assert.AreEqual(15f, day.HealthMultiplier, 0.001f);
            Assert.Greater(night.HealthMultiplier, day.HealthMultiplier);
            Assert.Greater(night.DamageBonus, day.DamageBonus);
            Assert.Greater(night.ExperienceMultiplier, day.ExperienceMultiplier);
            // Region bosses: 520 / 780 / 900 / 1180 HP; world boss = 15 commons: 1050 / 1650 / 2100 / 2550.
            float[] regionBoss = { 520f, 780f, 900f, 1180f };
            for (int tier = 0; tier < 4; tier++)
            {
                float ratio = EventRoomRules.TierHealth[tier] * day.HealthMultiplier / regionBoss[tier];
                Assert.That(ratio, Is.InRange(1.8f, 2.5f), $"tier {tier}");
            }
        }

        [Test]
        public void Boss_MinionsAndRage()
        {
            Assert.AreEqual(0, WorldBossRules.MinionsDue(5f, 999f, 0));
            Assert.AreEqual(2, WorldBossRules.MinionsDue(13f, 999f, 0));
            Assert.AreEqual(1, WorldBossRules.MinionsDue(40f, 30f, 2));
            Assert.AreEqual(0, WorldBossRules.MinionsDue(40f, 10f, 0));
            Assert.IsTrue(WorldBossRules.ShouldRage(0.5f, false));
            Assert.IsFalse(WorldBossRules.ShouldRage(0.5f, true));
            Assert.IsFalse(WorldBossRules.ShouldRage(0.8f, false));
        }

        [Test]
        public void Boss_RewardIsSeeded_AndHooksMountAndHouse()
        {
            WorldBossDefinition forest = WorldBossRules.For(BiomeType.Forest);
            WorldBossReward a = WorldBossRules.RewardFor(forest, false, 1234, _ => true);
            WorldBossReward b = WorldBossRules.RewardFor(forest, false, 1234, _ => true);
            Assert.AreEqual(a.Gold, b.Gold);
            Assert.AreEqual(a.HouseItem, b.HouseItem);
            Assert.AreEqual(a.MountId, b.MountId);
            Assert.AreEqual(20, a.Gold);
            Assert.AreEqual(2, a.BlessingStones);
            Assert.AreEqual(0, a.SoulStones);

            int mounts = 0, houses = 0;
            for (int seed = 0; seed < 4000; seed++)
            {
                WorldBossReward r = WorldBossRules.RewardFor(forest, true, seed, _ => true);
                mounts += r.MountId != null ? 1 : 0;
                houses += r.HouseItem ? 1 : 0;
            }

            Assert.That(mounts / 4000f, Is.InRange(0.02f, 0.06f), "night mount ~4%");
            Assert.That(houses / 4000f, Is.InRange(0.58f, 0.72f), "night house item ~65%");
            Assert.IsNull(WorldBossRules.RewardFor(forest, true, 1, _ => false).MountId, "unplayable mounts never drop");
        }

        [Test]
        public void Boss_CatalogUsesExistingEnemiesAndMounts()
        {
            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                Assert.IsNotNull(Resources.Load($"{EncounterPayload.EnemyResourcesFolder}/{boss.DayEnemyId}"), boss.DayEnemyId);
                Assert.IsNotNull(Resources.Load($"{EncounterPayload.EnemyResourcesFolder}/{boss.NightEnemyId}"), boss.NightEnemyId);
                Assert.IsTrue(Rapadura.Gameplay.Mounts.MountDropRules.IsPlayable(boss.MountId), boss.MountId);
                Assert.AreNotEqual(boss.DayNamePt, boss.NightNamePt);
            }
        }

        // ------------------------------------------------------------------
        // Invasion rules
        // ------------------------------------------------------------------

        [Test]
        public void Invasion_PointsAndTiers()
        {
            Assert.AreEqual(0, InvasionRules.TierFor(InvasionRules.Points(2, false, 100f)));
            Assert.AreEqual(1, InvasionRules.TierFor(InvasionRules.Points(3, false, 100f)));
            int allWaves = InvasionRules.TotalBandits;
            Assert.AreEqual(18, allWaves);
            Assert.AreEqual(3, InvasionRules.TierFor(InvasionRules.Points(allWaves, false, 0f)), "every bandit, no captain: +3");
            Assert.AreEqual(4, InvasionRules.TierFor(InvasionRules.Points(allWaves, true, 0f)), "captain at the buzzer: +4");
            Assert.AreEqual(5, InvasionRules.TierFor(InvasionRules.Points(allWaves, true, 90f)), "captain with time to spare: +5");
        }

        [Test]
        public void Invasion_WaveFlow()
        {
            Assert.IsTrue(InvasionRules.ShouldSpawnWave(0, 0, 999f));
            Assert.IsFalse(InvasionRules.ShouldSpawnWave(1, 4, 5f));
            Assert.IsTrue(InvasionRules.ShouldSpawnWave(1, 1, 5f));
            Assert.IsFalse(InvasionRules.ShouldSpawnWave(4, 0, 999f));
            Assert.IsFalse(InvasionRules.ShouldSpawnCaptain(4, 1, false));
            Assert.IsTrue(InvasionRules.ShouldSpawnCaptain(4, 0, false));
            Assert.IsFalse(InvasionRules.ShouldSpawnCaptain(4, 0, true));
        }

        [Test]
        public void Invasion_ChestGrowsWithTier_AndIsSeeded()
        {
            CangacoChest previous = InvasionRules.ChestFor(1, BiomeType.Mountain, 7, false, _ => true);
            Assert.AreEqual(3 * 28, previous.Experience);
            for (int tier = 2; tier <= 5; tier++)
            {
                CangacoChest chest = InvasionRules.ChestFor(tier, BiomeType.Mountain, 7, false, _ => true);
                Assert.Greater(chest.Gold, previous.Gold);
                Assert.Greater(chest.Experience, previous.Experience);
                Assert.GreaterOrEqual(chest.BlessingStones, previous.BlessingStones);
                previous = chest;
            }

            Assert.AreEqual(InvasionRules.ChestFor(5, BiomeType.Forest, 99).Gold, InvasionRules.ChestFor(5, BiomeType.Forest, 99).Gold);
            Assert.IsTrue(InvasionRules.ChestFor(0, BiomeType.Forest, 1).IsEmpty);
            for (int seed = 0; seed < 500; seed++)
            {
                Assert.IsNull(InvasionRules.ChestFor(5, BiomeType.Forest, seed, alreadyOwnsMount: true, _ => true).MountId);
            }
        }

        // ------------------------------------------------------------------
        // Progress
        // ------------------------------------------------------------------

        [Test]
        public void Progress_PaysOncePerWindow_AndRoundTrips()
        {
            var progress = new WorldEventProgress();
            string id = WorldBossRules.BossDesertId;
            var open = new RealTimeWindow(true, 50, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30), 600);
            Assert.IsTrue(progress.CanFightBoss(id, open));
            Assert.IsTrue(progress.RecordBossDefeat(id, 50, true));
            Assert.IsFalse(progress.CanFightBoss(id, open));
            Assert.IsFalse(progress.RecordBossDefeat(id, 50, false));
            Assert.AreEqual(2, progress.Defeats(id));

            Assert.IsTrue(progress.CanDefend(open));
            Assert.IsFalse(progress.RecordInvasion(50, 0, false), "no chest: may try again");
            Assert.IsTrue(progress.CanDefend(open));
            Assert.IsTrue(progress.RecordInvasion(50, 4, true));
            Assert.IsFalse(progress.CanDefend(open));
            Assert.IsFalse(progress.RecordInvasion(50, 5, true));

            var copy = new WorldEventProgress();
            copy.RestoreState(JsonUtility.FromJson<WorldEventSaveState>(JsonUtility.ToJson(progress.CaptureState())));
            Assert.AreEqual(2, copy.Defeats(id));
            Assert.AreEqual(1, copy.NightDefeats(id));
            Assert.AreEqual(50, copy.LastRewardedWindow(id));
            Assert.AreEqual(1, copy.Chests(4));
            Assert.AreEqual(4, copy.BestChestTier);
            Assert.AreEqual(2, copy.InvasionsDefended);

            copy.ResetForNewSession();
            Assert.AreEqual(0, copy.Defeats(id));
            Assert.AreEqual(-1, copy.LastChestWindow);
        }

        // ------------------------------------------------------------------
        // Texts and view
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryCousinHasALineForEveryMoment()
        {
            var keys = new HashSet<string>(WorldBossTexts.Entries.Select(e => e.Key));
            foreach (WorldEventMoment moment in Enum.GetValues(typeof(WorldEventMoment)))
            {
                foreach (CharacterId hero in Enum.GetValues(typeof(CharacterId)))
                {
                    Assert.IsTrue(keys.Contains(WorldBossTexts.LineKey(moment, hero)), $"{moment} {hero}");
                }
            }

            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                Assert.IsTrue(keys.Contains(boss.NameKey(false)));
                Assert.IsTrue(keys.Contains(boss.NameKey(true)));
            }

            Assert.IsFalse(WorldBossTexts.Entries.Any(e => e.PtBR.Contains("Zefa") || e.PtBR.Contains("Chico")), "grandparents are Vó Edna / Vô Jorge");
        }

        [Test]
        public void MapLines_ShowOpenBossOrCountdown()
        {
            var progress = new WorldEventProgress();
            string open = WorldEventMapPanel.BossLine(null, Brt(2026, 10, 30, 9, 10), progress);
            StringAssert.Contains("Saklas", open);
            StringAssert.Contains("na Floresta", open);
            StringAssert.Contains("20:00", open);

            string next = WorldEventMapPanel.BossLine(null, Brt(2026, 10, 30, 9, 35), progress);
            StringAssert.Contains("Próximo", next);
            StringAssert.Contains("no Deserto", next);
            StringAssert.Contains("10:00", next);

            StringAssert.Contains("Acaba em", WorldEventMapPanel.InvasionLine(null, Brt(2026, 10, 30, 2, 10), progress));
            StringAssert.Contains("Próxima", WorldEventMapPanel.InvasionLine(null, Brt(2026, 10, 30, 3, 0), progress));
        }

        [Test]
        public void Hud_ViewAndResultLines()
        {
            VisualElement view = WorldEventHud.CreateView();
            Assert.IsNotNull(view.Q("we-hud-bar"));
            Assert.IsNotNull(view.Q("we-results-card"));
            Assert.AreEqual(PickingMode.Ignore, view.pickingMode);

            var result = new WorldEventResult { Kind = WorldEventKind.Invasion, Won = true, Paid = true, ChestTier = 5, Points = 340, Gold = 45, Experience = 220, BlessingStones = 2, CousinLine = "Oba!" };
            List<string> lines = WorldEventHud.ResultLines(null, result);
            Assert.AreEqual("Baú do Cangaço +5", lines[1]);
            StringAssert.StartsWith("“", lines[lines.Count - 1]);
        }

        [Test]
        public void Payload_IsDirected()
        {
            var payload = EncounterPayload.Create(BiomeType.Forest, null, 1);
            Assert.IsFalse(payload.IsDirected);
            payload.worldEvent = WorldEventKind.WorldBoss;
            Assert.IsTrue(payload.IsDirected);
            Assert.IsFalse(payload.IsEventRoom);
            EncounterPayload copy = EncounterPayload.FromJson(payload.ToJson());
            Assert.AreEqual(WorldEventKind.WorldBoss, copy.worldEvent);
        }

        [Test]
        public void Regalia_MeshesBuild()
        {
            Mesh crown = WorldEventRegalia.BuildCrown();
            Mesh hat = WorldEventRegalia.BuildHat();
            _created.Add(crown);
            _created.Add(hat);
            Assert.Greater(crown.triangles.Length, 0);
            Assert.Greater(hat.triangles.Length, 0);
            Assert.Less(crown.triangles.Length / 3 + hat.triangles.Length / 3, 1500, "props stay cheap on mobile");
        }
    }
}
