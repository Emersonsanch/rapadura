using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.Rebirth;
using Rapadura.Gameplay.SharedWorld;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure shared world (TODO.md 12.3, Network.md §18): respawn agenda per tier with seeded variation and crowd density,
    /// spawn points away from players, the clearing schedule (wake, fill, sleep), the rare region lock, the damage ledger
    /// (cap, overkill, away decay), MU kill ownership with anti kill-steal, the loot protection over the party modes, the
    /// map-level warning that never blocks, the offline bridge and the texts.
    /// </summary>
    public class SharedWorldRulesTests
    {
        private const int Seed = 12345;

        // ------------------------------------------------------------------
        // Tiers and times
        // ------------------------------------------------------------------

        [Test]
        public void Tiers_FollowTheNightSpecies()
        {
            Assert.AreEqual(SpawnTier.Common, SharedSpawnRules.TierOf(PeriodSpawnEntry.Normal("enemy_forest_wolf", 3)));
            Assert.AreEqual(SpawnTier.Common, SharedSpawnRules.TierOf(PeriodSpawnEntry.Variant("enemy_forest_wolf", 3)));
            Assert.AreEqual(SpawnTier.Elite, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.RasgaMortalha, 1)));
            Assert.AreEqual(SpawnTier.Elite, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.Queixada, 1)));
            Assert.AreEqual(SpawnTier.Elite, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.VagaLume, 1)));
            Assert.AreEqual(SpawnTier.Elite, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.MorcegoVampiro, 1)));
            Assert.AreEqual(SpawnTier.Rare, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.Lobisomem, 1)));
            Assert.AreEqual(SpawnTier.Rare, SharedSpawnRules.TierOf(PeriodSpawnEntry.Species(NightVariantRules.MaeDaLua, 1)));
            Assert.AreEqual(SpawnTier.Common, SharedSpawnRules.TierOf((PeriodSpawnEntry)null));
            Assert.AreEqual(SpawnTier.Common, SharedSpawnRules.TierOfSpecies(null));
            Assert.AreEqual(SpawnTier.Rare, SharedSpawnRules.TierOfSpecies(NightVariantRules.Lobisomem.Id));
        }

        [Test]
        public void BaseSeconds_PerTier()
        {
            Assert.AreEqual(25f, SharedSpawnRules.BaseSeconds(SpawnTier.Common, 25f));
            Assert.AreEqual(25f, SharedSpawnRules.BaseSeconds(SpawnTier.Common, 0f), "no clearing time = the offline default");
            Assert.AreEqual(SharedSpawnRules.MinRespawnSeconds, SharedSpawnRules.BaseSeconds(SpawnTier.Common, 2f));
            Assert.AreEqual(100f, SharedSpawnRules.BaseSeconds(SpawnTier.Elite, 25f));
            Assert.AreEqual(90f, SharedSpawnRules.BaseSeconds(SpawnTier.Elite, 15f));
            Assert.AreEqual(900f, SharedSpawnRules.BaseSeconds(SpawnTier.Rare, 25f));
            Assert.AreEqual(SpawnTier.Common, SharedSpawnRules.SlotTier(SpawnTier.Rare));
            Assert.AreEqual(SpawnTier.Elite, SharedSpawnRules.SlotTier(SpawnTier.Elite));
        }

        [Test]
        public void RespawnSeconds_IsSeeded_WithinItsBand_AndVaries()
        {
            var seen = new HashSet<float>();
            foreach (SpawnTier tier in new[] { SpawnTier.Common, SpawnTier.Elite, SpawnTier.Rare })
            {
                float baseSeconds = SharedSpawnRules.BaseSeconds(tier, 25f);
                float jitter = SharedSpawnRules.JitterOf(tier);
                for (int i = 0; i < 200; i++)
                {
                    float a = SharedSpawnRules.RespawnSeconds(tier, 25f, Seed, "clearing_01", i, 1);
                    float b = SharedSpawnRules.RespawnSeconds(tier, 25f, Seed, "clearing_01", i, 1);
                    Assert.AreEqual(a, b, "same inputs, same time on every machine");
                    Assert.GreaterOrEqual(a, baseSeconds * (1f - jitter) - 0.001f);
                    Assert.LessOrEqual(a, baseSeconds * (1f + jitter) + 0.001f);
                    seen.Add(a);
                }
            }

            Assert.Greater(seen.Count, 300, "the server seed spreads the times");
            Assert.AreNotEqual(SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, 1, "c", 0, 1),
                SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, 2, "c", 0, 1), "another server seed, another time");
        }

        [Test]
        public void Density_GrowsWithPlayers_UpToTheCap()
        {
            Assert.AreEqual(1f, SharedSpawnRules.Density(0));
            Assert.AreEqual(1f, SharedSpawnRules.Density(1));
            Assert.AreEqual(1.5f, SharedSpawnRules.Density(3));
            Assert.AreEqual(2f, SharedSpawnRules.Density(5));
            Assert.AreEqual(2f, SharedSpawnRules.Density(50));

            Assert.AreEqual(3, SharedSpawnRules.TargetAlive(3, 1));
            Assert.AreEqual(5, SharedSpawnRules.TargetAlive(3, 4)); // 3 × 1.75 = 5.25
            Assert.AreEqual(6, SharedSpawnRules.TargetAlive(3, 5));
            Assert.AreEqual(6, SharedSpawnRules.TargetAlive(3, 40));
            Assert.AreEqual(10, SharedSpawnRules.TargetAlive(6, 5), "hard cap per clearing");
            Assert.AreEqual(12, SharedSpawnRules.TargetAlive(12, 5), "a clearing already above the cap keeps its own number");
            Assert.AreEqual(0, SharedSpawnRules.TargetAlive(0, 5));
        }

        [Test]
        public void Density_SpeedsCommonsAndElites_NotRares()
        {
            for (int i = 0; i < 20; i++)
            {
                float alone = SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, Seed, "c", i, 1);
                float crowd = SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, Seed, "c", i, 5);
                Assert.AreEqual(Mathf.Max(SharedSpawnRules.MinRespawnSeconds, alone / 2f), crowd, 0.001f);

                float eliteAlone = SharedSpawnRules.RespawnSeconds(SpawnTier.Elite, 25f, Seed, "c", i, 1);
                Assert.Less(SharedSpawnRules.RespawnSeconds(SpawnTier.Elite, 25f, Seed, "c", i, 3), eliteAlone);

                Assert.AreEqual(SharedSpawnRules.RespawnSeconds(SpawnTier.Rare, 25f, Seed, "c", i, 1),
                    SharedSpawnRules.RespawnSeconds(SpawnTier.Rare, 25f, Seed, "c", i, 9));
            }
        }

        // ------------------------------------------------------------------
        // Spawn point
        // ------------------------------------------------------------------

        [Test]
        public void SpawnPoint_NeverOnTopOfAPlayer_AndPrefersOutOfSight()
        {
            var center = new Vector2(100f, 100f);
            var random = new System.Random(7);

            Assert.IsTrue(SharedSpawnRules.TryPickSpawnPoint(center, 12f, null, random, out Vector2 free));
            Assert.LessOrEqual(Vector2.Distance(free, center), 12f * SharedSpawnRules.ClearingFill + 0.01f);

            // A player in the middle of a 10 m clearing: every point is within 8.5 m of them.
            Assert.IsFalse(SharedSpawnRules.TryPickSpawnPoint(center, 10f, new[] { center }, random, out _));

            // A player 15 m off: a point comes, never within 9 m.
            var near = new[] { center + new Vector2(15f, 0f) };
            for (int i = 0; i < 50; i++)
            {
                if (SharedSpawnRules.TryPickSpawnPoint(center, 12f, near, random, out Vector2 p))
                {
                    Assert.GreaterOrEqual(SharedSpawnRules.NearestPlayerDistance(p, near), SharedSpawnRules.MinPlayerDistance);
                }
            }

            // Players far off: always out of sight.
            var far = new[] { center + new Vector2(60f, 0f) };
            Assert.IsTrue(SharedSpawnRules.TryPickSpawnPoint(center, 12f, far, random, out Vector2 hidden));
            Assert.GreaterOrEqual(SharedSpawnRules.NearestPlayerDistance(hidden, far), SharedSpawnRules.SightRadius);

            // The caller's filter (river, safe zone) is honoured.
            Assert.IsFalse(SharedSpawnRules.TryPickSpawnPoint(center, 12f, null, random, out _, _ => false));
            Assert.IsFalse(SharedSpawnRules.TryPickSpawnPoint(center, 12f, null, null, out _));
        }

        [Test]
        public void SpawnPoint_FallsBackToTheLeastSeenPoint()
        {
            var center = Vector2.zero;
            // A player 18 m off a 12 m clearing: no point is 24 m away from them on the near side, some are on the far side.
            var players = new[] { new Vector2(18f, 0f) };
            int inSight = 0;
            var random = new System.Random(3);
            for (int i = 0; i < 200; i++)
            {
                Assert.IsTrue(SharedSpawnRules.TryPickSpawnPoint(center, 12f, players, random, out Vector2 p));
                float d = SharedSpawnRules.NearestPlayerDistance(p, players);
                Assert.GreaterOrEqual(d, SharedSpawnRules.MinPlayerDistance);
                if (d < SharedSpawnRules.SightRadius)
                {
                    inSight++;
                }
            }

            Assert.Less(inSight, 200, "most picks find a point out of sight");
        }

        [Test]
        public void InRange_UsesWakeAndSleepRadii()
        {
            Assert.IsTrue(SharedSpawnRules.InRange(80f, false));
            Assert.IsFalse(SharedSpawnRules.InRange(100f, false));
            Assert.IsTrue(SharedSpawnRules.InRange(100f, true));
            Assert.IsFalse(SharedSpawnRules.InRange(130f, true));
        }

        // ------------------------------------------------------------------
        // Clearing schedule
        // ------------------------------------------------------------------

        private static int FillAll(SharedClearingSchedule schedule, ref float now, int players)
        {
            int spawned = 0;
            for (int i = 0; i < 100; i++)
            {
                if (schedule.ShouldSpawn(now, players))
                {
                    schedule.MarkSpawned(now);
                    spawned++;
                }

                now += 0.5f;
            }

            return spawned;
        }

        [Test]
        public void Schedule_SleepsUntilAPlayerComes_ThenFillsOneAtATime()
        {
            var schedule = new SharedClearingSchedule("c1", 3, 25f);
            Assert.IsFalse(schedule.ShouldSpawn(0f, 1), "asleep");
            Assert.AreEqual(ClearingTransition.None, schedule.UpdatePresence(0f, 200f));
            Assert.AreEqual(ClearingTransition.Woke, schedule.UpdatePresence(1f, 50f));

            Assert.IsTrue(schedule.ShouldSpawn(1f, 1));
            schedule.MarkSpawned(1f);
            Assert.IsFalse(schedule.ShouldSpawn(1.2f, 1), "fill spacing");
            Assert.IsTrue(schedule.ShouldSpawn(1.5f, 1));

            float now = 1.5f;
            Assert.AreEqual(2, FillAll(schedule, ref now, 1));
            Assert.AreEqual(3, schedule.Alive);

            schedule.MarkSpawnFailed(now);
            Assert.AreEqual(now + SharedSpawnRules.RetrySeconds, schedule.NextFillAt);
        }

        [Test]
        public void Schedule_ADeathWaitsItsTime_ThenTheSlotRefills()
        {
            var schedule = new SharedClearingSchedule("c1", 3, 25f);
            schedule.UpdatePresence(0f, 0f);
            float now = 0f;
            FillAll(schedule, ref now, 1);

            float back = schedule.OnDeath(now, SpawnTier.Common, Seed, 1);
            Assert.AreEqual(2, schedule.Alive);
            Assert.AreEqual(1, schedule.Waiting(now));
            Assert.AreEqual(back, schedule.NextReturnAt(now));
            Assert.IsFalse(schedule.ShouldSpawn(now + 1f, 1), "the slot waits");
            Assert.IsFalse(schedule.ShouldSpawn(back - 0.1f, 1));
            Assert.IsTrue(schedule.ShouldSpawn(back, 1));
            Assert.AreEqual(1, schedule.Deaths);

            float eliteBack = schedule.OnDeath(back, SpawnTier.Elite, Seed, 1);
            Assert.GreaterOrEqual(eliteBack - back, 100f * (1f - SharedSpawnRules.EliteJitter) - 0.01f);

            float rareBack = schedule.OnDeath(back, SpawnTier.Rare, Seed, 1);
            Assert.LessOrEqual(rareBack - back, 25f * (1f + SharedSpawnRules.CommonJitter) + 0.01f, "a rare frees its slot on the common time");

            schedule.OnRemoved();
            Assert.AreEqual(0, schedule.Alive);
            schedule.OnRemoved();
            Assert.AreEqual(0, schedule.Alive);
        }

        [Test]
        public void Schedule_MorePlayersOpenMoreSlots()
        {
            var schedule = new SharedClearingSchedule("c1", 3, 25f);
            schedule.UpdatePresence(0f, 0f);
            float now = 0f;
            Assert.AreEqual(3, FillAll(schedule, ref now, 1));
            Assert.AreEqual(3, FillAll(schedule, ref now, 5), "×2 with 5 players");
            Assert.AreEqual(6, schedule.Alive);
            Assert.AreEqual(0, schedule.FreeSlots(now, 5));
        }

        [Test]
        public void Schedule_SleepKeepsTheTimers_NoRefillByWalkingAway()
        {
            var schedule = new SharedClearingSchedule("c1", 3, 25f);
            schedule.UpdatePresence(0f, 0f);
            float now = 0f;
            FillAll(schedule, ref now, 1);
            float back = schedule.OnDeath(now, SpawnTier.Common, Seed, 1);
            schedule.OnDeath(now, SpawnTier.Common, Seed, 1);

            // The player walks off: past the sleep radius, the clearing holds on for the grace.
            float off = now + 1f;
            Assert.AreEqual(ClearingTransition.None, schedule.UpdatePresence(off, 100f), "still within the sleep radius");
            Assert.AreEqual(ClearingTransition.None, schedule.UpdatePresence(off + 1f, 200f));
            Assert.AreEqual(ClearingTransition.None, schedule.UpdatePresence(off + 30f, 200f));
            Assert.AreEqual(ClearingTransition.Slept, schedule.UpdatePresence(off + 1f + SharedSpawnRules.SleepGraceSeconds, 200f));
            Assert.IsFalse(schedule.Running);
            Assert.AreEqual(0, schedule.Alive, "survivors despawned");

            Assert.AreEqual(2, schedule.Waiting(off), "the dead keep waiting while it sleeps");
            Assert.LessOrEqual(schedule.NextReturnAt(off).Value, back);
            Assert.IsFalse(schedule.ShouldSpawn(off + 100f, 1), "asleep: nothing spawns");
        }

        [Test]
        public void Schedule_WakingBeforeTheTimers_DoesNotRefillTheDead()
        {
            var schedule = new SharedClearingSchedule("c1", 3, 600f); // long common time, so it outlives the sleep grace
            schedule.UpdatePresence(0f, 0f);
            float now = 0f;
            FillAll(schedule, ref now, 1);
            schedule.OnDeath(now, SpawnTier.Common, Seed, 1);
            schedule.OnDeath(now, SpawnTier.Common, Seed, 1);
            Assert.AreEqual(ClearingTransition.None, schedule.UpdatePresence(now, 500f), "the grace starts");
            Assert.AreEqual(ClearingTransition.Slept, schedule.UpdatePresence(now + SharedSpawnRules.SleepGraceSeconds, 500f));

            float wake = now + 100f;
            Assert.AreEqual(ClearingTransition.Woke, schedule.UpdatePresence(wake, 10f));
            float t = wake;
            Assert.AreEqual(1, FillAll(schedule, ref t, 1), "only the survivor's slot comes back");
            Assert.AreEqual(2, schedule.Waiting(t));
        }

        // ------------------------------------------------------------------
        // Rares
        // ------------------------------------------------------------------

        [Test]
        public void Rare_OneAtATimePerRegion_AndLockedAfterTheKill()
        {
            var tracker = new SharedRareTracker();
            string wolf = NightVariantRules.Lobisomem.Id;
            Assert.IsTrue(tracker.CanRoll("floresta", wolf, 0f));
            tracker.OnSpawned("floresta", wolf);
            Assert.IsFalse(tracker.CanRoll("floresta", wolf, 0f), "one alive in the region");
            Assert.IsTrue(tracker.CanRoll("deserto", wolf, 0f), "another region is its own");

            float until = tracker.OnKilled("floresta", wolf, 100f, Seed);
            Assert.AreEqual(until, tracker.LockedUntil("floresta", wolf));
            Assert.GreaterOrEqual(until - 100f, 900f * (1f - SharedSpawnRules.RareJitter) - 0.01f);
            Assert.LessOrEqual(until - 100f, 900f * (1f + SharedSpawnRules.RareJitter) + 0.01f);
            Assert.IsFalse(tracker.CanRoll("floresta", wolf, until - 1f));
            Assert.IsTrue(tracker.CanRoll("floresta", wolf, until));

            tracker.OnSpawned("deserto", wolf);
            tracker.OnRemoved("deserto", wolf);
            Assert.IsTrue(tracker.CanRoll("deserto", wolf, 0f), "despawned by sleep: no wait");
            Assert.AreEqual(0, tracker.AliveIn("deserto", wolf));
        }

        [Test]
        public void Rare_AllowedFilter_ExtendsTheOfflineTable()
        {
            var tracker = new SharedRareTracker();
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies wolf = NightVariantRules.Lobisomem;
            var owls = 0;
            Predicate<PeriodSpawnEntry> allowed = tracker.Allowed("floresta", 0f, id => id == owl.Id ? owls : 0);

            Assert.IsTrue(allowed(PeriodSpawnEntry.Normal("enemy_forest_wolf", 1)));
            Assert.IsTrue(allowed(PeriodSpawnEntry.Variant("enemy_forest_wolf", 1)));
            Assert.IsFalse(allowed(null));
            Assert.IsTrue(allowed(PeriodSpawnEntry.Species(owl, 1)));
            owls = owl.MaxPerWave;
            Assert.IsFalse(allowed(PeriodSpawnEntry.Species(owl, 1)), "the offline MaxPerWave per clearing");
            Assert.IsFalse(allowed(new PeriodSpawnEntry("x", 1, NightForm.Species, "no_such_species")));

            Assert.IsTrue(allowed(PeriodSpawnEntry.Species(wolf, 1)));
            tracker.OnSpawned("floresta", wolf.Id);
            Assert.IsFalse(allowed(PeriodSpawnEntry.Species(wolf, 1)), "a rare already stands in the region");

            // With the table roll: the locked rare never comes, the rest still does.
            var table = new[] { PeriodSpawnEntry.Species(wolf, 50), PeriodSpawnEntry.Normal("enemy_forest_wolf", 1) };
            var random = new System.Random(1);
            for (int i = 0; i < 50; i++)
            {
                PeriodSpawnEntry rolled = PeriodSpawnTable.RollEntry(table, random, allowed);
                Assert.IsNotNull(rolled);
                Assert.AreEqual(NightForm.Normal, rolled.Form);
            }
        }

        // ------------------------------------------------------------------
        // Damage ledger
        // ------------------------------------------------------------------

        [Test]
        public void Ledger_CountsOnlyRealDamage()
        {
            var ledger = new EnemyDamageLedger();
            Assert.AreEqual(30d, ledger.Record("a", null, 30d, 100d, 0f));
            Assert.AreEqual(70d, ledger.Record("b", null, 500d, 70d, 1f), "overkill is not credited");
            Assert.AreEqual(0d, ledger.Record("c", null, 10d, 0d, 2f));
            Assert.AreEqual(0d, ledger.Record("c", null, -5d, 50d, 2f));
            Assert.AreEqual(0d, ledger.Record(null, null, 5d, 50d, 2f));
            Assert.AreEqual(100d, ledger.Total);
            Assert.AreEqual("b", ledger.LastHitPlayerId);
            Assert.AreEqual(70d, ledger.LastHitDamage);
            Assert.AreEqual(2, ledger.Entries.Count);
        }

        [Test]
        public void Ledger_CapsTheEntries_DroppingTheSmallest()
        {
            var ledger = new EnemyDamageLedger();
            for (int i = 0; i < EnemyDamageLedger.MaxEntries; i++)
            {
                ledger.Record("p" + i, null, 10d + i, 10000d, i);
            }

            Assert.AreEqual(EnemyDamageLedger.MaxEntries, ledger.Entries.Count);
            ledger.Record("small", null, 5d, 10000d, 100f);
            Assert.IsNull(ledger.Find("small"), "below the smallest line: not listed");
            ledger.Record("big", null, 50d, 10000d, 101f);
            Assert.IsNotNull(ledger.Find("big"));
            Assert.IsNull(ledger.Find("p0"), "the smallest went");
            Assert.AreEqual(EnemyDamageLedger.MaxEntries, ledger.Entries.Count);
        }

        [Test]
        public void Ledger_DecaysWhenThePlayerWalksOff_AndForgets()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("a", null, 100d, 1000d, 0f);
            ledger.Record("b", null, 100d, 1000d, 0f);
            Func<string, bool> onlyB = id => id == "b";

            ledger.Tick(1f, onlyB);
            ledger.Tick(1f + EnemyDamageLedger.AwayGraceSeconds, onlyB);
            Assert.AreEqual(100d, ledger.Find("a").Damage, 1e-6, "grace");
            ledger.Tick(1f + EnemyDamageLedger.AwayGraceSeconds + EnemyDamageLedger.AwayHalfLifeSeconds, onlyB);
            Assert.AreEqual(50d, ledger.Find("a").Damage, 1e-6, "one half-life");
            Assert.AreEqual(100d, ledger.Find("b").Damage);

            // Back: the decay stops, what was lost stays lost.
            ledger.Tick(25f, id => true);
            ledger.Tick(40f, id => true);
            Assert.AreEqual(50d, ledger.Find("a").Damage, 1e-6);
            Assert.IsNull(ledger.Find("a").AwaySince);

            // Away for good: forgotten after a minute.
            ledger.Tick(41f, onlyB);
            ledger.Tick(41f + EnemyDamageLedger.ForgetAfterAwaySeconds, onlyB);
            Assert.IsNull(ledger.Find("a"));
            Assert.IsNotNull(ledger.Find("b"));

            ledger.Clear();
            Assert.AreEqual(0, ledger.Entries.Count);
            Assert.IsNull(ledger.LastHitPlayerId);
        }

        [Test]
        public void Ledger_HittingAgainStopsTheDecay()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("a", null, 100d, 1000d, 0f);
            ledger.Tick(0f, _ => false);
            ledger.Tick(30f, _ => false); // 20 s past the grace: two half-lives
            Assert.AreEqual(25d, ledger.Find("a").Damage, 1e-6);
            ledger.Record("a", "party1", 10d, 1000d, 31f);
            Assert.AreEqual(35d, ledger.Find("a").Damage, 1e-6);
            Assert.AreEqual("party1", ledger.Find("a").PartyId);
            Assert.IsNull(ledger.Find("a").AwaySince);
            Assert.AreEqual(0f, ledger.Find("a").FirstHitAt);
        }

        // ------------------------------------------------------------------
        // Kill ownership
        // ------------------------------------------------------------------

        [Test]
        public void Ownership_MostDamageWins()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("a", null, 40d, 100d, 0f);
            ledger.Record("b", null, 20d, 60d, 1f);
            ledger.Record("a", null, 10d, 40d, 2f);
            ledger.Record("b", null, 30d, 30d, 3f); // kill
            KillOwner owner = KillOwnershipRules.Resolve(ledger);
            Assert.AreEqual("a", owner.PlayerId, "b's last blow does not count: 20 against 50");
            Assert.IsFalse(owner.IsParty);
            Assert.AreEqual(50d, owner.Damage);
            Assert.IsTrue(KillOwnershipRules.IsOwnerSide(owner, "a", null));
            Assert.IsFalse(KillOwnershipRules.IsOwnerSide(owner, "b", null));
        }

        [Test]
        public void Ownership_AntiKillSteal_TheLastBlowAloneOwnsNothing()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("tank", null, 95d, 100d, 0f);
            ledger.Record("thief", null, 999d, 5d, 1f);
            Assert.AreEqual("tank", KillOwnershipRules.Resolve(ledger, "thief").PlayerId);

            // A one-shot with no one else: the hitter's.
            var oneShot = new EnemyDamageLedger();
            oneShot.Record("solo", "p9", 500d, 80d, 0f);
            KillOwner owner = KillOwnershipRules.Resolve(oneShot);
            Assert.AreEqual("solo", owner.PlayerId);
            Assert.AreEqual("p9", owner.PartyId);

            Assert.IsTrue(KillOwnershipRules.Resolve(new EnemyDamageLedger()).IsNone);
            Assert.IsTrue(KillOwnershipRules.Resolve(null).IsNone);
            Assert.AreEqual("k", KillOwnershipRules.Resolve(new EnemyDamageLedger(), "k").PlayerId);
        }

        [Test]
        public void Ownership_APartyCountsAsOne()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("solo", null, 50d, 200d, 0f);
            ledger.Record("m1", "party", 30d, 150d, 1f);
            ledger.Record("m2", "party", 35d, 120d, 2f);
            ledger.Record("solo", null, 85d, 85d, 3f); // kill: the solo's 85 is not credited
            KillOwner owner = KillOwnershipRules.Resolve(ledger);
            Assert.IsTrue(owner.IsParty);
            Assert.AreEqual("party", owner.PartyId);
            Assert.AreEqual("m2", owner.PlayerId, "the party's top hitter");
            Assert.AreEqual(65d, owner.Damage);
            Assert.IsTrue(KillOwnershipRules.IsOwnerSide(owner, "m1", "party"));
            Assert.IsFalse(KillOwnershipRules.IsOwnerSide(owner, "solo", null));
            Assert.IsFalse(KillOwnershipRules.IsOwnerSide(owner, null, "party"));
        }

        [Test]
        public void Ownership_TieGoesToTheFirstHitter()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("late", null, 40d, 200d, 5f);
            ledger.Record("early", null, 40d, 160d, 1f);
            ledger.Record("x", null, 120d, 120d, 6f);
            Assert.AreEqual("early", KillOwnershipRules.Resolve(ledger).PlayerId);
        }

        [Test]
        public void Ownership_WalkingOffLosesTheClaim()
        {
            var ledger = new EnemyDamageLedger();
            ledger.Record("a", null, 60d, 200d, 0f);
            ledger.Record("b", null, 40d, 140d, 1f);
            ledger.Tick(2f, id => id == "b");
            ledger.Tick(2f + EnemyDamageLedger.AwayGraceSeconds + 2f * EnemyDamageLedger.AwayHalfLifeSeconds, id => id == "b");
            ledger.Record("c", null, 100d, 100d, 40f);
            Assert.AreEqual("b", KillOwnershipRules.Resolve(ledger).PlayerId, "a's 60 decayed to 15");
        }

        // ------------------------------------------------------------------
        // Loot
        // ------------------------------------------------------------------

        private static PartyState PartyOf(PartyLootMode mode, params string[] ids)
        {
            var party = new PartyState(ids[0], null, mode);
            for (int i = 1; i < ids.Length; i++)
            {
                Assert.IsTrue(PartyRules.Invite(party, ids[0], ids[i], null, false, 0d).Ok);
                Assert.IsTrue(PartyRules.Accept(party, ids[i], 1d).Ok);
            }

            return party;
        }

        [Test]
        public void Loot_ProtectionIsThePartyLock()
        {
            Assert.AreEqual(PartyLootRules.OwnerLockSeconds, SharedLootRules.ProtectionSeconds);
        }

        [Test]
        public void Loot_SoloOwnerTakesEverything_ThenItOpens()
        {
            List<SharedLootDrop> drops = SharedLootRules.Assign(new KillOwner("a", null, 50d), 3, null, null, Seed);
            Assert.AreEqual(3, drops.Count);
            foreach (SharedLootDrop drop in drops)
            {
                Assert.AreEqual("a", drop.OwnerPlayerId);
                Assert.IsTrue(SharedLootRules.CanPickUp(drop, "a", null, false, 0d));
                Assert.IsFalse(SharedLootRules.CanPickUp(drop, "b", null, true, 10d));
                Assert.IsFalse(SharedLootRules.CanPickUp(drop, "b", null, true, SharedLootRules.ProtectionSeconds - 0.1d));
                Assert.IsTrue(SharedLootRules.CanPickUp(drop, "b", null, true, SharedLootRules.ProtectionSeconds));
                Assert.IsFalse(SharedLootRules.CanPickUp(drop, "b", null, false, 999d), "must be near");
                Assert.IsFalse(SharedLootRules.CanPickUp(drop, null, null, true, 999d));
                Assert.AreEqual(20d, SharedLootRules.ProtectionLeft(drop, 10d));
            }

            Assert.AreEqual(0, SharedLootRules.Assign(new KillOwner("a", null, 1d), 0, null, null, Seed).Count);
            List<SharedLootDrop> none = SharedLootRules.Assign(KillOwner.None, 2, null, null, Seed);
            Assert.IsTrue(none[0].IsFree);
            Assert.IsTrue(SharedLootRules.CanPickUp(none[0], "anyone", null, true, 0d));
            Assert.AreEqual(0d, SharedLootRules.ProtectionLeft(none[0], 0d));
        }

        [Test]
        public void Loot_APartyOwnerUsesTheLeadersMode()
        {
            PartyState party = PartyOf(PartyLootMode.RoundRobin, "a", "b", "c");
            var owner = new KillOwner("b", "P", 80d);
            List<SharedLootDrop> drops = SharedLootRules.Assign(owner, 3, party, party.Members, Seed);
            Assert.AreEqual("a", drops[0].OwnerPlayerId);
            Assert.AreEqual("b", drops[1].OwnerPlayerId);
            Assert.AreEqual("c", drops[2].OwnerPlayerId);
            Assert.AreEqual("P", drops[0].OwnerPartyId);
            Assert.IsFalse(SharedLootRules.CanPickUp(drops[0], "b", "P", true, 5d), "a party mate waits like the party lock");
            Assert.IsTrue(SharedLootRules.CanPickUp(drops[0], "outsider", null, true, SharedLootRules.ProtectionSeconds));

            PartyState free = PartyOf(PartyLootMode.FreeForAll, "a", "b");
            List<SharedLootDrop> shared = SharedLootRules.Assign(new KillOwner("a", "F", 10d), 2, free, free.Members, Seed);
            Assert.IsNull(shared[0].OwnerPlayerId);
            Assert.AreEqual("F", shared[0].OwnerPartyId);
            Assert.IsFalse(shared[0].IsFree);
            Assert.IsTrue(SharedLootRules.CanPickUp(shared[0], "b", "F", true, 1d), "any member of the owning party");
            Assert.IsFalse(SharedLootRules.CanPickUp(shared[0], "x", null, true, 1d), "not an outsider");
            Assert.IsFalse(SharedLootRules.CanPickUp(shared[0], "x", "other", true, 1d));
            Assert.IsTrue(SharedLootRules.CanPickUp(shared[0], "x", "other", true, 30d));

            // The party is gone by the time of the kill: the top hitter keeps it, under the party id.
            List<SharedLootDrop> orphan = SharedLootRules.Assign(new KillOwner("a", "gone", 10d), 1, null, null, Seed);
            Assert.AreEqual("a", orphan[0].OwnerPlayerId);
        }

        // ------------------------------------------------------------------
        // Map level: a warning, never a wall
        // ------------------------------------------------------------------

        [Test]
        public void MapLevel_WarnsButNeverBlocks()
        {
            Assert.IsFalse(SharedMapLevelRules.BlocksEntry);

            MapLevelNotice ok = SharedMapLevelRules.Check(50, null, 40, 55, null, MapRegionTypes.Forest);
            Assert.AreEqual(MapLevelWarning.None, ok.Warning);
            Assert.AreEqual(5, ok.MonsterGap);

            MapLevelNotice below = SharedMapLevelRules.Check(35, null, 40, 44, null, MapRegionTypes.Forest);
            Assert.AreEqual(MapLevelWarning.BelowWalkLevel, below.Warning);
            Assert.AreEqual(40, below.RequiredLevel);
            Assert.IsFalse(below.Blocks);

            Assert.AreEqual(MapLevelWarning.StrongMonsters, SharedMapLevelRules.Check(35, null, 40, 45, null, MapRegionTypes.Forest).Warning);
            Assert.AreEqual(MapLevelWarning.Lethal, SharedMapLevelRules.Check(10, null, 40, 45, null, MapRegionTypes.Forest).Warning);
            Assert.IsFalse(SharedMapLevelRules.Check(1, null, 400, 500, null, MapRegionTypes.Forest).Blocks);
        }

        [Test]
        public void MapLevel_ReadsTheRebirthEffectiveLevel_AndTheCousinBonus()
        {
            var reset = new RebirthState("r", resets: 1, peakLevel: 400);
            MapLevelNotice notice = SharedMapLevelRules.Check(1, reset, 300, 320, null, MapRegionTypes.Forest);
            Assert.AreEqual(400, notice.EffectiveLevel);
            Assert.AreEqual(MapLevelWarning.None, notice.Warning, "a reset hero keeps the gear of their old level");

            Assert.AreEqual(MapLevelWarning.Lethal, SharedMapLevelRules.Check(1, new RebirthState("n", peakLevel: 400), 300, 320, null, MapRegionTypes.Forest).Warning,
                "a peak without any reset counts for nothing");

            // Maithe walks mountains 5 levels early.
            Assert.AreEqual(MapLevelWarning.None, SharedMapLevelRules.Check(36, null, 40, 42, CharacterId.Maithe, MapRegionTypes.Mountain).Warning);
            Assert.AreEqual(MapLevelWarning.BelowWalkLevel, SharedMapLevelRules.Check(36, null, 40, 42, CharacterId.Joaquim, MapRegionTypes.Mountain).Warning);

            Assert.AreEqual(MapLevelWarning.None, SharedMapLevelRules.Check(5, null, (Rapadura.Gameplay.World.Map.Overworld.OverworldZone)null, null).Warning);
            var zone = new Rapadura.Gameplay.World.Map.Overworld.OverworldZone { Id = "z", WalkLevel = 40, MaxLevel = 45, RegionType = MapRegionTypes.Forest };
            Assert.AreEqual(MapLevelWarning.BelowWalkLevel, SharedMapLevelRules.Check(38, null, zone, null).Warning);
        }

        [Test]
        public void MapLevel_AnnouncesOnceOnEntering()
        {
            MapLevelNotice warn = SharedMapLevelRules.Check(10, null, 40, 45, null, MapRegionTypes.Forest);
            MapLevelNotice none = SharedMapLevelRules.Check(50, null, 40, 45, null, MapRegionTypes.Forest);
            Assert.IsTrue(SharedMapLevelRules.ShouldAnnounce("a", "b", warn));
            Assert.IsTrue(SharedMapLevelRules.ShouldAnnounce(null, "b", warn));
            Assert.IsFalse(SharedMapLevelRules.ShouldAnnounce("b", "b", warn));
            Assert.IsFalse(SharedMapLevelRules.ShouldAnnounce("a", "b", none));
        }

        // ------------------------------------------------------------------
        // Offline bridge and texts
        // ------------------------------------------------------------------

        [Test]
        public void Bridge_OffKeepsTodaysTime_OnUsesTheAgenda()
        {
            Assert.AreEqual(25f, SharedSpawnBridge.RespawnSeconds(false, 25f, "z", NightVariantRules.Lobisomem.Id, 3));
            Assert.AreEqual(7.5f, SharedSpawnBridge.RespawnSeconds(false, 7.5f, "z", null, 0), "off returns the clearing's own number untouched");
            Assert.AreEqual(SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, SharedSpawnBridge.OfflineSeed, "z", 4, 1),
                SharedSpawnBridge.RespawnSeconds(true, 25f, "z", null, 4));
            Assert.AreEqual(SharedSpawnRules.RespawnSeconds(SpawnTier.Elite, 25f, SharedSpawnBridge.OfflineSeed, "z", 4, 1),
                SharedSpawnBridge.RespawnSeconds(true, 25f, "z", NightVariantRules.RasgaMortalha.Id, 4));
            Assert.AreEqual(SharedSpawnRules.RespawnSeconds(SpawnTier.Common, 25f, SharedSpawnBridge.OfflineSeed, "z", 4, 1),
                SharedSpawnBridge.RespawnSeconds(true, 25f, "z", NightVariantRules.Lobisomem.Id, 4), "a rare's slot waits the common time");
        }

        [Test]
        public void Texts_EveryWarningHasPtAndEn()
        {
            foreach (MapLevelWarning warning in Enum.GetValues(typeof(MapLevelWarning)))
            {
                if (warning == MapLevelWarning.None)
                {
                    continue;
                }

                string key = SharedWorldTexts.WarningKey(warning);
                string pt = SharedWorldTexts.GetFallback(key);
                Assert.IsFalse(string.IsNullOrEmpty(pt), key);
                StringAssert.Contains("{0}", pt);
            }

            foreach (var entry in SharedWorldTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }

            MapLevelNotice below = SharedMapLevelRules.Check(35, null, 40, 44, null, MapRegionTypes.Forest);
            StringAssert.Contains("40", SharedWorldTexts.Warning(null, below, 44));
            Assert.AreEqual(string.Empty, SharedWorldTexts.Warning(null, SharedMapLevelRules.Check(50, null, 40, 44, null, MapRegionTypes.Forest), 44));
            StringAssert.Contains("Maithe", SharedWorldTexts.Format(null, SharedWorldTexts.KillOwned, "Maithe"));
        }
    }
}
