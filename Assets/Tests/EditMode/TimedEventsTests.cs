using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.TimedEvents;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure scheduled events of FASE 12.3 (Docs/Network.md §14): agenda (fixed Brasília times, warning, gate, duration,
    /// next occurrence, no overlaps), entry (rooms by level, ticket or gold, room size and instances, one entry per day,
    /// party entry), the Vigília da Coluna waves, the Cerco da Treva lamp, the Caçada a Ohyah contribution loot, the
    /// board and bands, the rewards (cosmetic-only rare prizes) and the Bema.
    /// </summary>
    public class TimedEventsTests
    {
        /// <summary>UTC instant of a Brasília (UTC−3) wall time.</summary>
        private static DateTime Brt(int y, int m, int d, int h, int min = 0) => new DateTime(y, m, d, h, min, 0, DateTimeKind.Utc).AddHours(3);

        private static readonly DateTime Nov23 = Brt(2026, 11, 23, 0);

        private static TimedEventRoomSet SetAt(TimedEventDefinition def, DateTime utc) => new TimedEventRoomSet(def, TimedEventAgenda.Next(def, utc));

        // ------------------------------------------------------------------ agenda

        [Test]
        public void Agenda_VigiliaPhasesFollowTheBrasiliaClock()
        {
            TimedEventDefinition v = TimedEventCatalog.Vigilia;
            Assert.AreEqual(TimedEventPhase.Closed, TimedEventAgenda.StatusAt(v, Brt(2026, 11, 23, 12, 40)).Phase);
            Assert.AreEqual(TimedEventPhase.Announced, TimedEventAgenda.StatusAt(v, Brt(2026, 11, 23, 12, 45)).Phase);
            Assert.AreEqual(TimedEventPhase.EntryOpen, TimedEventAgenda.StatusAt(v, Brt(2026, 11, 23, 12, 55)).Phase);
            TimedEventStatus running = TimedEventAgenda.StatusAt(v, Brt(2026, 11, 23, 13, 5));
            Assert.AreEqual(TimedEventPhase.Running, running.Phase);
            Assert.AreEqual(Brt(2026, 11, 23, 13), running.Occurrence.StartUtc);
            Assert.AreEqual(Brt(2026, 11, 23, 13, 15), running.Occurrence.EndUtc);
            Assert.AreEqual(600d, running.SecondsToChange, 0.001);
            Assert.AreEqual(TimedEventPhase.Closed, TimedEventAgenda.StatusAt(v, Brt(2026, 11, 23, 13, 15)).Phase);
        }

        [Test]
        public void Agenda_NextOccurrenceAndRollOverMidnight()
        {
            TimedEventOccurrence next = TimedEventAgenda.Next(TimedEventCatalog.Vigilia, Brt(2026, 11, 23, 21, 30));
            Assert.AreEqual(Brt(2026, 11, 24, 1), next.StartUtc);
            Assert.AreEqual(Brt(2026, 11, 24, 0, 45), next.WarningUtc);
            Assert.AreEqual(Brt(2026, 11, 24, 0, 55), next.EntryOpensUtc);

            TimedEventOccurrence cacada = TimedEventAgenda.NextNotYetOpen(TimedEventCatalog.Cacada, Brt(2026, 11, 23, 10, 55));
            Assert.AreEqual(Brt(2026, 11, 23, 22), cacada.StartUtc, "11:00 gate already open → the 22:00 one");
            Assert.AreEqual(Brt(2026, 11, 23, 11), TimedEventAgenda.Next(TimedEventCatalog.Cacada, Brt(2026, 11, 23, 10, 55)).StartUtc);
        }

        [Test]
        public void Agenda_OccurrenceKeysAreUniqueAndStable()
        {
            long day = TimedEventAgenda.LocalDay(Nov23);
            var keys = new HashSet<long>();
            for (long d = day; d < day + 3; d++)
            {
                foreach (TimedEventOccurrence o in TimedEventAgenda.OccurrencesOn(TimedEventCatalog.Vigilia, d))
                {
                    Assert.IsTrue(keys.Add(o.Key));
                }
            }

            Assert.AreEqual(18, keys.Count);
            Assert.AreEqual(TimedEventAgenda.Next(TimedEventCatalog.Vigilia, Brt(2026, 11, 23, 12)).Key,
                TimedEventAgenda.Next(TimedEventCatalog.Vigilia, Brt(2026, 11, 23, 12, 50)).Key);
        }

        [TestCase(2026, 11, 23)]
        [TestCase(2027, 6, 15)]
        public void Agenda_EventsNeverOverlapFromGateToEnd(int y, int m, int d)
        {
            long day = TimedEventAgenda.LocalDay(Brt(y, m, d, 12));
            var spans = new List<(DateTime From, DateTime To, string What)>();
            foreach (TimedEventDefinition def in TimedEventCatalog.All)
            {
                for (long dd = day - 1; dd <= day + 1; dd++)
                {
                    spans.AddRange(TimedEventAgenda.OccurrencesOn(def, dd).Select(o => (o.EntryOpensUtc, o.EndUtc, def.Id)));
                }
            }

            spans.Sort((a, b) => a.From.CompareTo(b.From));
            for (int i = 1; i < spans.Count; i++)
            {
                Assert.LessOrEqual(spans[i - 1].To, spans[i].From, spans[i - 1].What + " overlaps " + spans[i].What);
            }
        }

        [Test]
        public void Agenda_EveryDefinitionIsConsistent()
        {
            foreach (TimedEventDefinition def in TimedEventCatalog.All)
            {
                Assert.GreaterOrEqual(def.WarningMinutes, def.EntryMinutes, def.Id);
                Assert.Greater(def.EntryMinutes, 0, def.Id);
                Assert.Greater(def.DurationMinutes, 0, def.Id);
                Assert.Greater(def.MaxPlayersPerRoom, PartyRules.MaxMembers - 1, def.Id + ": a full party fits a room");
                for (int i = 1; i < def.Rooms.Length; i++)
                {
                    Assert.AreEqual(def.Rooms[i - 1].MaxLevel + 1, def.Rooms[i].MinLevel, def.Id + ": bands are contiguous");
                }

                Assert.AreEqual(500, def.Rooms[def.Rooms.Length - 1].MaxLevel, def.Id);
                Assert.AreSame(def, TimedEventCatalog.Find(def.Kind));
            }
        }

        // ------------------------------------------------------------------ entry

        [Test]
        public void Entry_RoomByLevelBand()
        {
            TimedEventDefinition v = TimedEventCatalog.Vigilia;
            Assert.AreEqual(0, v.RoomFor(14));
            Assert.AreEqual(1, v.RoomFor(15));
            Assert.AreEqual(1, v.RoomFor(79));
            Assert.AreEqual(2, v.RoomFor(80));
            Assert.AreEqual(7, v.RoomFor(500));
            Assert.AreEqual(TimedEventRefusal.LevelTooLow, TimedEventRules.RoomFor(TimedEventCatalog.Cacada, 99, out _));
            Assert.AreEqual(TimedEventRefusal.None, TimedEventRules.RoomFor(TimedEventCatalog.Cacada, 250, out int room));
            Assert.AreEqual(2, room);
        }

        [Test]
        public void Entry_OnlyWhileTheGateIsOpen()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            TimedEventRoomSet set = SetAt(TimedEventCatalog.Vigilia, open);
            var e = new TimedEventEntrant("a", 50, 1, 0);
            Assert.AreEqual(TimedEventRefusal.EntryClosed, TimedEventRules.TryEnter(set, e, new TimedEventEntryLedger(), Brt(2026, 11, 23, 12, 50)).Refusal);
            Assert.AreEqual(TimedEventRefusal.EntryClosed, TimedEventRules.TryEnter(set, e, new TimedEventEntryLedger(), Brt(2026, 11, 23, 13, 1)).Refusal);
            TimedEventEntryResult ok = TimedEventRules.TryEnter(set, e, new TimedEventEntryLedger(), open);
            Assert.IsTrue(ok.Ok, ok.Refusal.ToString());
            Assert.AreEqual(1, ok.RoomIndex);
            Assert.AreEqual(1, ok.Payments[0].Tickets);
            Assert.AreEqual(0, ok.Payments[0].Gold);
        }

        [Test]
        public void Entry_TicketThenGoldThenRefusal()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            TimedEventDefinition v = TimedEventCatalog.Vigilia;
            Assert.AreEqual(TimedEventRefusal.None, TimedEventRules.Cost(v, 2, new TimedEventEntrant("a", 90, 0, 6000), out TimedEventPayment gold));
            Assert.AreEqual(6000, gold.Gold);
            Assert.AreEqual(TimedEventRefusal.NotEnoughGold, TimedEventRules.Cost(v, 2, new TimedEventEntrant("a", 90, 0, 5999), out _));

            // The Cerco is free.
            Assert.AreEqual(TimedEventRefusal.None, TimedEventRules.Cost(TimedEventCatalog.Cerco, 7, new TimedEventEntrant("a", 450, 0, 0), out TimedEventPayment free));
            Assert.AreEqual(0, free.Gold);
            Assert.AreEqual(0, free.Tickets);

            TimedEventRoomSet set = SetAt(v, open);
            TimedEventEntryResult poor = TimedEventRules.TryEnter(set, new TimedEventEntrant("p", 90, 0, 10), new TimedEventEntryLedger(), open);
            Assert.AreEqual(TimedEventRefusal.NotEnoughGold, poor.Refusal);
            Assert.AreEqual(0, set.Rooms.Count, "a refused entry opens nothing");
        }

        [Test]
        public void Entry_OnePerDayPerCharacter_RejoinIsFree()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            var ledger = new TimedEventEntryLedger();
            TimedEventRoomSet first = SetAt(TimedEventCatalog.Vigilia, open);
            Assert.IsTrue(TimedEventRules.TryEnter(first, new TimedEventEntrant("a", 50, 5, 0), ledger, open).Ok);

            TimedEventEntryResult back = TimedEventRules.TryRejoin(first, "a", Brt(2026, 11, 23, 13, 10));
            Assert.IsTrue(back.Ok);
            Assert.IsTrue(back.Rejoined);
            Assert.AreEqual(0, back.Payments.Count);
            Assert.AreEqual(TimedEventRefusal.EntryClosed, TimedEventRules.TryRejoin(first, "a", Brt(2026, 11, 23, 13, 15)).Refusal);
            Assert.AreEqual(TimedEventRefusal.NotInRoom, TimedEventRules.TryRejoin(first, "z", Brt(2026, 11, 23, 13, 10)).Refusal);

            DateTime later = Brt(2026, 11, 23, 16, 56);
            TimedEventRoomSet second = SetAt(TimedEventCatalog.Vigilia, later);
            Assert.AreEqual(TimedEventRefusal.AlreadyEnteredToday, TimedEventRules.TryEnter(second, new TimedEventEntrant("a", 50, 5, 0), ledger, later).Refusal);

            DateTime tomorrow = Brt(2026, 11, 24, 0, 56);
            Assert.IsTrue(TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, tomorrow), new TimedEventEntrant("a", 50, 5, 0), ledger, tomorrow).Ok);
        }

        [Test]
        public void Entry_FullRoomsOpenNewInstancesUpToTheCap()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            TimedEventRoomSet set = SetAt(TimedEventCatalog.Vigilia, open);
            int max = TimedEventCatalog.Vigilia.MaxPlayersPerRoom * TimedEventRules.MaxInstancesPerBand;
            for (int i = 0; i < max; i++)
            {
                TimedEventEntryResult r = TimedEventRules.TryEnter(set, new TimedEventEntrant("p" + i, 20, 1, 0), new TimedEventEntryLedger(), open);
                Assert.IsTrue(r.Ok, i + ": " + r.Refusal);
                Assert.AreEqual(i / TimedEventCatalog.Vigilia.MaxPlayersPerRoom + 1, r.Instance);
            }

            Assert.AreEqual(TimedEventRefusal.RoomFull, TimedEventRules.TryEnter(set, new TimedEventEntrant("late", 20, 1, 0), new TimedEventEntryLedger(), open).Refusal);
            Assert.IsTrue(TimedEventRules.TryEnter(set, new TimedEventEntrant("other_band", 100, 1, 0), new TimedEventEntryLedger(), open).Ok, "other bands have their own rooms");
            Assert.IsTrue(set.Rooms.All(r => r.Players.Count <= r.Capacity));
        }

        private static PartyState Party(params string[] ids)
        {
            var party = new PartyState(ids[0], null);
            for (int i = 1; i < ids.Length; i++)
            {
                Assert.IsTrue(PartyRules.Invite(party, ids[0], ids[i], null, false, 0d).Ok);
                Assert.IsTrue(PartyRules.Accept(party, ids[i], 1d).Ok);
            }

            return party;
        }

        [Test]
        public void Entry_PartyGoesInTogetherOrNotAtAll()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            PartyState party = Party("L", "A", "B", "C");
            var ledgers = new Dictionary<string, TimedEventEntryLedger>();
            TimedEventEntryLedger Ledger(string id) => ledgers.TryGetValue(id, out var l) ? l : ledgers[id] = new TimedEventEntryLedger();

            var entrants = new List<TimedEventEntrant>
            {
                new TimedEventEntrant("L", 60, 1, 0), new TimedEventEntrant("A", 40, 1, 0),
                new TimedEventEntrant("B", 70, 0, 0), new TimedEventEntrant("C", 50, 1, 0, present: false),
            };

            TimedEventRoomSet set = SetAt(TimedEventCatalog.Vigilia, open);
            Assert.AreEqual(TimedEventRefusal.NotPartyLeader, TimedEventRules.TryEnterParty(set, party, "A", entrants, Ledger, open).Refusal);

            TimedEventEntryResult blocked = TimedEventRules.TryEnterParty(set, party, "L", entrants, Ledger, open);
            Assert.AreEqual(TimedEventRefusal.PartyMemberNotEligible, blocked.Refusal);
            Assert.AreEqual("B", blocked.FailedPlayerId);
            Assert.AreEqual(TimedEventRefusal.NotEnoughGold, blocked.MemberRefusal);
            Assert.AreEqual(0, set.Rooms.Count);
            Assert.AreEqual(0, Ledger("L").EntriesOn(TimedEventKind.VigiliaDaColuna, set.Occurrence.Day), "nobody spent an entry");

            entrants[2] = new TimedEventEntrant("B", 70, 0, 2000);
            TimedEventEntryResult ok = TimedEventRules.TryEnterParty(set, party, "L", entrants, Ledger, open);
            Assert.IsTrue(ok.Ok, ok.Refusal.ToString());
            CollectionAssert.AreEqual(new[] { "L", "A", "B" }, ok.Entered, "C was not at the gate");
            Assert.AreEqual(2000, ok.Payments.Single(p => p.PlayerId == "B").Gold);
            Assert.AreEqual(1, set.Rooms.Count);
            Assert.AreEqual(1, Ledger("B").EntriesOn(TimedEventKind.VigiliaDaColuna, set.Occurrence.Day));
        }

        [Test]
        public void Entry_PartyInDifferentBandsIsRefused()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            PartyState party = Party("L", "A");
            var entrants = new List<TimedEventEntrant> { new TimedEventEntrant("L", 70, 1, 0), new TimedEventEntrant("A", 85, 1, 0) };
            TimedEventEntryResult r = TimedEventRules.TryEnterParty(SetAt(TimedEventCatalog.Vigilia, open), party, "L", entrants, _ => new TimedEventEntryLedger(), open);
            Assert.AreEqual(TimedEventRefusal.PartyRoomMismatch, r.Refusal);
            Assert.AreEqual("A", r.FailedPlayerId);
        }

        [Test]
        public void Entry_PartyDoesNotSplitAcrossInstances()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            TimedEventRoomSet set = SetAt(TimedEventCatalog.Vigilia, open);
            for (int i = 0; i < 8; i++)
            {
                Assert.IsTrue(TimedEventRules.TryEnter(set, new TimedEventEntrant("s" + i, 20, 1, 0), null, open).Ok);
            }

            PartyState party = Party("L", "A", "B");
            var entrants = new[] { "L", "A", "B" }.Select(id => new TimedEventEntrant(id, 20, 1, 0)).ToList();
            TimedEventEntryResult r = TimedEventRules.TryEnterParty(set, party, "L", entrants, _ => null, open);
            Assert.IsTrue(r.Ok);
            Assert.AreEqual(2, r.Instance, "3 do not fit in the 2 free slots of instance 1");
        }

        // ------------------------------------------------------------------ waves

        [Test]
        public void Vigilia_WavesAreSeededGrowAndEndWithTheArchon()
        {
            TimedEventRoomBand band = TimedEventCatalog.SevenRooms[2];
            int seed = TimedEventAgenda.Seed(TimedEventKind.VigiliaDaColuna, 12345, 3, 1);
            TimedWave a = TimedVigiliaRules.Wave(band, 4, 5, seed);
            TimedWave b = TimedVigiliaRules.Wave(band, 4, 5, seed);
            CollectionAssert.AreEqual(a.Groups.Select(g => (g.Role, g.Count)).ToList(), b.Groups.Select(g => (g.Role, g.Count)).ToList());
            Assert.AreEqual(TimedWaveRules.Budget(TimedVigiliaRules.BaseBudget, TimedVigiliaRules.BudgetPerWave, 4, 5), a.Cost);

            Assert.AreEqual(0, TimedVigiliaRules.Wave(band, 0, 1, seed).Groups.Count(g => g.Role != TimedEnemyRole.Shade), "wave 1 is only Shades");
            Assert.Greater(TimedVigiliaRules.Wave(band, 6, 1, seed).Cost, TimedVigiliaRules.Wave(band, 1, 1, seed).Cost);
            Assert.Greater(TimedVigiliaRules.Wave(band, 3, 10, seed).Cost, TimedVigiliaRules.Wave(band, 3, 1, seed).Cost, "more players, bigger wave");

            TimedWave last = TimedVigiliaRules.Wave(band, TimedVigiliaRules.WaveCount - 1, 3, seed);
            Assert.AreEqual(1, last.Count(TimedEnemyRole.Archon));
            for (int w = 0; w < TimedVigiliaRules.WaveCount - 1; w++)
            {
                Assert.AreEqual(0, TimedVigiliaRules.Wave(band, w, 3, seed).Count(TimedEnemyRole.Archon));
            }

            Assert.AreEqual(band.MidLevel - 6 + 2 * 4, a.EnemyLevel);
            Assert.AreEqual(500, TimedWaveRules.EnemyLevel(new TimedEventRoomBand(9, 495, 500), 7), "never above the level cap");
        }

        [Test]
        public void Vigilia_DifferentSeedsGiveDifferentWaves()
        {
            TimedEventRoomBand band = TimedEventCatalog.SevenRooms[4];
            var distinct = new HashSet<string>();
            for (int s = 0; s < 20; s++)
            {
                TimedWave wave = TimedVigiliaRules.Wave(band, 6, 4, TimedEventAgenda.Seed(TimedEventKind.VigiliaDaColuna, s, 5, 1));
                distinct.Add(string.Join(",", wave.Groups.Select(g => g.Role + ":" + g.Count)));
            }

            Assert.Greater(distinct.Count, 3);
        }

        [Test]
        public void Vigilia_WaveTimingAndOutcome()
        {
            Assert.IsTrue(TimedVigiliaRules.ShouldStartNextWave(0, 0d, 0));
            Assert.IsFalse(TimedVigiliaRules.ShouldStartNextWave(1, 30d, 5));
            Assert.IsTrue(TimedVigiliaRules.ShouldStartNextWave(1, 30d, 2));
            Assert.IsTrue(TimedVigiliaRules.ShouldStartNextWave(1, 100d, 9));
            Assert.IsFalse(TimedVigiliaRules.ShouldStartNextWave(TimedVigiliaRules.WaveCount, 500d, 0));

            Assert.AreEqual(TimedEventOutcome.InProgress, TimedVigiliaRules.Outcome(3, 4, false));
            Assert.AreEqual(TimedEventOutcome.Defeat, TimedVigiliaRules.Outcome(0, 4, false));
            Assert.AreEqual(TimedEventOutcome.Victory, TimedVigiliaRules.Outcome(1, 5, true), "held until the end");
            Assert.AreEqual(TimedEventOutcome.Victory, TimedVigiliaRules.Outcome(0, 8, false), "last wave cleared");
        }

        // ------------------------------------------------------------------ cerco

        [Test]
        public void Cerco_LampHealthScalesAndHitsAreFractions()
        {
            Assert.AreEqual(4000, TimedCercoRules.LampMaxHealth(1, 1));
            Assert.Greater(TimedCercoRules.LampMaxHealth(3, 1), TimedCercoRules.LampMaxHealth(1, 1));
            Assert.Greater(TimedCercoRules.LampMaxHealth(1, 10), TimedCercoRules.LampMaxHealth(1, 1));

            var lamp = new TimedLampState(TimedCercoRules.LampMaxHealth(1, 1));
            Assert.AreEqual(40, TimedCercoRules.HitLamp(lamp, TimedEnemyRole.Shade));
            Assert.AreEqual(480, TimedCercoRules.HitLamp(lamp, TimedEnemyRole.Archon));
            Assert.AreEqual(4000 - 520, lamp.Health);
        }

        [Test]
        public void Cerco_RekindleHasCooldownAndCap()
        {
            var lamp = new TimedLampState(4000);
            Assert.AreEqual(0, TimedCercoRules.Rekindle(lamp, "a", 0d), "full lamp");
            TimedCercoRules.HitLamp(lamp, TimedEnemyRole.Brute); // −120
            Assert.AreEqual(120, TimedCercoRules.Rekindle(lamp, "a", 1d), "never above the max");
            TimedCercoRules.HitLamp(lamp, TimedEnemyRole.Elite); // −240
            Assert.AreEqual(0, TimedCercoRules.Rekindle(lamp, "a", 10d), "cooldown");
            Assert.AreEqual(160, TimedCercoRules.Rekindle(lamp, "b", 10d), "someone else can");
            Assert.AreEqual(80, TimedCercoRules.Rekindle(lamp, "a", 21d));
        }

        [Test]
        public void Cerco_DefeatWhenTheLampGoesOutOrTheGateIsAbandoned()
        {
            var lamp = new TimedLampState(100);
            TimedCercoRules.NoteDefenders(lamp, 3, 10d);
            Assert.AreEqual(TimedEventOutcome.InProgress, TimedCercoRules.Outcome(lamp, 2, 30d, false));
            Assert.AreEqual(TimedEventOutcome.Defeat, TimedCercoRules.Outcome(lamp, 2, 55d, false), "nobody stood for 45 s");

            TimedCercoRules.NoteDefenders(lamp, 1, 60d);
            Assert.AreEqual(TimedEventOutcome.Victory, TimedCercoRules.Outcome(lamp, TimedCercoRules.AssaultCount, 61d, false));
            Assert.AreEqual(TimedEventOutcome.Victory, TimedCercoRules.Outcome(lamp, 3, 61d, true));
            Assert.IsTrue(TimedCercoRules.IsIntact(lamp));

            for (int i = 0; i < 20; i++)
            {
                TimedCercoRules.HitLamp(lamp, TimedEnemyRole.Archon);
            }

            Assert.IsTrue(lamp.IsOut);
            Assert.AreEqual(0, lamp.Health);
            Assert.AreEqual(0, TimedCercoRules.Rekindle(lamp, "a", 100d), "a lamp that went out stays out");
            Assert.AreEqual(TimedEventOutcome.Defeat, TimedCercoRules.Outcome(lamp, TimedCercoRules.AssaultCount, 61d, true));
        }

        [Test]
        public void Cerco_AssaultsUseThreeGatesAndEndWithTheArchon()
        {
            TimedEventRoomBand band = TimedEventCatalog.SevenRooms[0];
            int seed = TimedEventAgenda.Seed(TimedEventKind.CercoDaTreva, 99, 1, 1);
            var gates = new HashSet<int>();
            for (int a = 0; a < TimedCercoRules.AssaultCount; a++)
            {
                foreach (TimedWaveGroup g in TimedCercoRules.Assault(band, a, 8, seed).Groups)
                {
                    Assert.That(g.Gate, Is.InRange(0, TimedCercoRules.Gates - 1));
                    gates.Add(g.Gate);
                }
            }

            Assert.AreEqual(3, gates.Count);
            Assert.AreEqual(1, TimedCercoRules.Assault(band, TimedCercoRules.AssaultCount - 1, 8, seed).Count(TimedEnemyRole.Archon));
        }

        // ------------------------------------------------------------------ caçada

        [Test]
        public void Cacada_IdleHuntersGetNothingAndActiveOnesGetTheFloor()
        {
            var list = new List<TimedContribution>
            {
                new TimedContribution("dps", 100000, 0, 0, 600, 900),
                new TimedContribution("healer", 0, 40000, 0, 700, 900),
                new TimedContribution("tank", 5000, 0, 60000, 800, 900),
                new TimedContribution("afk", 0, 0, 0, 0, 900),
                new TimedContribution("leech", 300, 0, 0, 30, 900), // hit once or twice and stood around
                new TimedContribution("small", 800, 0, 0, 120, 300),
            };

            List<TimedRaidShare> shares = TimedCacadaRules.Distribute(list, true, 7);
            TimedRaidShare S(string id) => shares.Single(s => s.PlayerId == id);
            Assert.IsFalse(S("afk").Active);
            Assert.IsFalse(S("afk").Floor);
            Assert.IsFalse(S("leech").Active, "under 45 s of actions");
            Assert.IsFalse(S("leech").WonItem);
            Assert.AreEqual(0, TimedCacadaRules.Points(S("leech")));
            Assert.IsTrue(S("small").Floor);
            Assert.AreEqual(1d, shares.Where(s => s.Active).Sum(s => s.Share), 1e-9);

            Assert.IsTrue(S("dps").TopDamage);
            Assert.IsTrue(S("healer").TopHealing);
            Assert.IsTrue(S("tank").TopTank);
            Assert.AreEqual(1, S("dps").Rank);
        }

        [Test]
        public void Cacada_HealingAndTankingCountLikeDamage()
        {
            Assert.AreEqual(1200d, TimedCacadaRules.Score(new TimedContribution("h", 0, 1000, 0, 100, 100)), 1e-9);
            Assert.AreEqual(800d, TimedCacadaRules.Score(new TimedContribution("t", 0, 0, 1000, 100, 100)), 1e-9);
            Assert.AreEqual(1000d, TimedCacadaRules.Score(new TimedContribution("d", 1000, 0, 0, 100, 100)), 1e-9);
            Assert.IsFalse(TimedCacadaRules.IsActive(new TimedContribution("x", 1000, 0, 0, 100, 400)), "25% active");
            Assert.IsTrue(TimedCacadaRules.IsActive(new TimedContribution("x", 1000, 0, 0, 120, 400)));
        }

        [Test]
        public void Cacada_ItemsOnePerHunterSeededAndOnlyOnVictory()
        {
            var list = Enumerable.Range(0, 23).Select(i => new TimedContribution("h" + i, 1000 + i * 500, 0, 0, 300, 600)).ToList();
            Assert.AreEqual(7, TimedCacadaRules.ItemCount(23));
            Assert.AreEqual(10, TimedCacadaRules.ItemCount(200));
            Assert.AreEqual(2, TimedCacadaRules.ItemCount(2));

            List<TimedRaidShare> a = TimedCacadaRules.Distribute(list, true, 42);
            List<TimedRaidShare> b = TimedCacadaRules.Distribute(list, true, 42);
            Assert.AreEqual(7, a.Count(s => s.WonItem));
            CollectionAssert.AreEqual(a.Select(s => s.WonItem).ToList(), b.Select(s => s.WonItem).ToList());

            List<TimedRaidShare> lost = TimedCacadaRules.Distribute(list, false, 42);
            Assert.AreEqual(0, lost.Count(s => s.WonItem || s.TopDamage));
            Assert.IsTrue(lost.All(s => s.Floor));
        }

        [Test]
        public void Cacada_BossHealthGrowsWithHuntersAndRoom()
        {
            Assert.AreEqual(TimedCacadaRules.BossHealth(1, 1), TimedCacadaRules.BossHealth(1, 5), "minimum of 5 hunters");
            Assert.AreEqual(2 * TimedCacadaRules.BossHealth(1, 10), TimedCacadaRules.BossHealth(1, 20));
            Assert.Greater(TimedCacadaRules.BossHealth(3, 10), TimedCacadaRules.BossHealth(1, 10));
        }

        // ------------------------------------------------------------------ board and rewards

        [Test]
        public void Scoring_SheetSumsTheTableAndNeverGoesNegative()
        {
            var sheet = new TimedScoreSheet();
            sheet.Add(TimedScoreAction.Death, 3);
            Assert.AreEqual(0, sheet.Points);
            sheet.Add(TimedScoreAction.KillArchon);
            sheet.Add(TimedScoreAction.AllyRevived, 2);
            Assert.AreEqual(150 + 50 - 60, sheet.Points);
            Assert.AreEqual(3, sheet.Deaths);
            Assert.AreEqual(TimedScoreAction.KillBrute, TimedEventScoring.KillAction(TimedEnemyRole.Brute));
        }

        [Test]
        public void Scoring_RankingTiesAndBands()
        {
            var entries = new List<TimedScoreEntry>();
            for (int i = 0; i < 10; i++)
            {
                entries.Add(new TimedScoreEntry("p" + i, 500 - i * 50, 0, i));
            }

            entries[2] = new TimedScoreEntry("p2", 450, 0, 2); // ties with p1
            entries.Add(new TimedScoreEntry("idle", 900, 0, 99, active: false));
            entries.Add(new TimedScoreEntry("zero", 0, 2, 50));

            List<TimedRankedEntry> board = TimedEventScoring.Rank(entries);
            Assert.AreEqual("p0", board[0].Entry.PlayerId);
            Assert.AreEqual(TimedRewardBand.Gold, board[0].Band);
            Assert.AreEqual(2, board[1].Position);
            Assert.AreEqual(2, board[2].Position, "equal points and deaths share the place");
            Assert.AreEqual(TimedRewardBand.Gold, board[1].Band, "11 active → 2 gold slots");
            Assert.AreEqual(TimedRewardBand.Silver, board[3].Band);
            Assert.AreEqual(TimedRewardBand.Bronze, board.Single(r => r.Entry.PlayerId == "p8").Band);
            Assert.AreEqual(TimedRewardBand.Participation, board.Single(r => r.Entry.PlayerId == "zero").Band);
            Assert.AreEqual("idle", board[board.Count - 1].Entry.PlayerId);
            Assert.AreEqual(TimedRewardBand.None, board[board.Count - 1].Band);
        }

        [Test]
        public void Rewards_ByBandHalfOnDefeatAndCosmeticsOnly()
        {
            TimedEventReward gold = TimedEventRewards.For(TimedEventKind.CercoDaTreva, TimedRewardBand.Gold, 3, TimedEventOutcome.Victory, 1, TimedBemaModifiers.Off);
            TimedEventReward bronze = TimedEventRewards.For(TimedEventKind.CercoDaTreva, TimedRewardBand.Bronze, 3, TimedEventOutcome.Victory, 1, TimedBemaModifiers.Off);
            TimedEventReward lost = TimedEventRewards.For(TimedEventKind.CercoDaTreva, TimedRewardBand.Gold, 3, TimedEventOutcome.Defeat, 1, TimedBemaModifiers.Off);
            Assert.AreEqual(40f, gold.XpCommons, 1e-4);
            Assert.AreEqual(180, gold.Gold);
            Assert.AreEqual(3, gold.BlessingStones);
            Assert.AreEqual(1, gold.SoulStones);
            Assert.AreEqual(TimedEventRewards.CercoMaterial, gold.MaterialId);
            Assert.Less(bronze.Gold, gold.Gold);
            Assert.AreEqual(gold.Gold / 2, lost.Gold);
            Assert.AreEqual(0, lost.BlessingStones);
            Assert.IsNull(lost.CosmeticId);
            Assert.IsTrue(TimedEventRewards.For(TimedEventKind.CercoDaTreva, TimedRewardBand.None, 3, TimedEventOutcome.Victory, 1, TimedBemaModifiers.Off).IsEmpty);

            int cosmetics = 0;
            for (int seed = 0; seed < 4000; seed++)
            {
                TimedEventReward r = TimedEventRewards.For(TimedEventKind.VigiliaDaColuna, TimedRewardBand.Gold, 1, TimedEventOutcome.Victory, seed, TimedBemaModifiers.Off);
                if (r.CosmeticId != null)
                {
                    cosmetics++;
                    Assert.IsTrue(TimedEventRewards.IsCosmetic(r.CosmeticId));
                    Assert.AreNotEqual(TimedBemaRules.BemaCosmeticId, r.CosmeticId, "the Bema lamp only during the Bema");
                }
            }

            Assert.That(cosmetics, Is.InRange(60, 190), "≈3%");
            foreach (string[] table in TimedEventRewards.Cosmetics.Values)
            {
                Assert.IsTrue(table.All(TimedEventRewards.IsCosmetic));
            }
        }

        [Test]
        public void Rewards_SameSeedSameReward()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                TimedEventReward a = TimedEventRewards.For(TimedEventKind.CacadaAOhyah, TimedRewardBand.Silver, 2, TimedEventOutcome.Victory, seed, TimedBemaModifiers.Off);
                TimedEventReward b = TimedEventRewards.For(TimedEventKind.CacadaAOhyah, TimedRewardBand.Silver, 2, TimedEventOutcome.Victory, seed, TimedBemaModifiers.Off);
                Assert.AreEqual(a.CosmeticId, b.CosmeticId);
                Assert.AreEqual(a.Gold, b.Gold);
            }

            Assert.AreNotEqual(TimedEventAgenda.Seed(TimedEventKind.VigiliaDaColuna, 1, 1, 1, "a"), TimedEventAgenda.Seed(TimedEventKind.VigiliaDaColuna, 1, 1, 1, "b"));
        }

        // ------------------------------------------------------------------ bema

        [Test]
        public void Bema_IsJuneOnTheServerDate()
        {
            Assert.IsFalse(TimedBemaRules.IsBema(Brt(2027, 5, 31, 23, 59)));
            Assert.IsTrue(TimedBemaRules.IsBema(Brt(2027, 6, 1, 0)));
            Assert.IsTrue(TimedBemaRules.IsBema(Brt(2027, 6, 30, 23, 59)));
            Assert.IsFalse(TimedBemaRules.IsBema(Brt(2027, 7, 1, 0)));
            // 1 June 01:00 UTC is still 31 May in Brasília.
            Assert.IsFalse(TimedBemaRules.IsBema(new DateTime(2027, 6, 1, 1, 0, 0, DateTimeKind.Utc)));

            Assert.AreEqual(Brt(2027, 6, 1, 0), TimedBemaRules.NextBemaStartUtc(Nov23));
            Assert.AreEqual(Brt(2027, 6, 1, 0), TimedBemaRules.NextBemaStartUtc(Brt(2027, 6, 12, 9)));
            Assert.AreEqual(Brt(2027, 7, 1, 0), TimedBemaRules.NextBemaEndUtc(Brt(2027, 6, 12, 9)));
            Assert.AreEqual(Brt(2028, 6, 1, 0), TimedBemaRules.NextBemaStartUtc(Brt(2027, 7, 2, 9)));

            Assert.IsTrue(TimedBemaRules.ModifiersOn(TimedEventAgenda.LocalDay(Brt(2027, 6, 30, 12))).GreatBema);
            Assert.IsFalse(TimedBemaRules.ModifiersOn(TimedEventAgenda.LocalDay(Brt(2027, 6, 29, 12))).GreatBema);
            Assert.IsFalse(TimedBemaRules.ModifiersOn(TimedEventAgenda.LocalDay(Nov23)).Active);
        }

        [Test]
        public void Bema_TruceSkipsTheCercoAndAddsTheBemaVigil()
        {
            long day = TimedEventAgenda.LocalDay(Brt(2027, 6, 10, 12));
            Assert.AreEqual(0, TimedEventAgenda.OccurrencesOn(TimedEventCatalog.Cerco, day).Count);
            List<TimedEventOccurrence> vigils = TimedEventAgenda.OccurrencesOn(TimedEventCatalog.Vigilia, day);
            Assert.AreEqual(7, vigils.Count);
            Assert.AreEqual(Brt(2027, 6, 10, 23), vigils[6].StartUtc);
            Assert.AreEqual(6, TimedEventAgenda.OccurrencesOn(TimedEventCatalog.Vigilia, day + 30).Count, "10 July: back to normal");

            TimedEventOccurrence nextCerco = TimedEventAgenda.Next(TimedEventCatalog.Cerco, Brt(2027, 6, 10, 12));
            Assert.AreEqual(Brt(2027, 7, 1, 8, 30), nextCerco.StartUtc, "the Cerco comes back on 1 July");

            var bemaSet = new TimedEventRoomSet(TimedEventCatalog.Cerco, default);
            Assert.AreEqual(TimedEventRefusal.SuspendedForBema,
                TimedEventRules.TryEnter(bemaSet, new TimedEventEntrant("a", 100, 0, 0), null, Brt(2027, 6, 10, 8, 27)).Refusal);
        }

        [Test]
        public void Bema_TwoVigilsADayAndRewardsWithOfferings()
        {
            var ledger = new TimedEventEntryLedger();
            DateTime first = Brt(2027, 6, 10, 12, 56);
            DateTime second = Brt(2027, 6, 10, 22, 56);
            DateTime third = Brt(2027, 6, 10, 16, 56);
            Assert.IsTrue(TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, first), new TimedEventEntrant("a", 50, 9, 0), ledger, first).Ok);
            Assert.IsTrue(TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, second), new TimedEventEntrant("a", 50, 9, 0), ledger, second).Ok);
            Assert.AreEqual(TimedEventRefusal.AlreadyEnteredToday,
                TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, third), new TimedEventEntrant("a", 50, 9, 0), ledger, third).Refusal);

            TimedBemaModifiers bema = TimedBemaRules.ModifiersOn(TimedEventAgenda.LocalDay(first));
            TimedEventReward r = TimedEventRewards.For(TimedEventKind.VigiliaDaColuna, TimedRewardBand.Gold, 1, TimedEventOutcome.Victory, 3, bema);
            Assert.AreEqual(44f, r.XpCommons, 1e-3);
            Assert.AreEqual(6, r.Offerings);
            Assert.AreEqual(0, TimedEventRewards.For(TimedEventKind.VigiliaDaColuna, TimedRewardBand.Gold, 1, TimedEventOutcome.Victory, 3, TimedBemaModifiers.Off).Offerings);

            bool sawBemaLamp = false;
            for (int seed = 0; seed < 4000 && !sawBemaLamp; seed++)
            {
                sawBemaLamp = TimedEventRewards.For(TimedEventKind.CacadaAOhyah, TimedRewardBand.Gold, 1, TimedEventOutcome.Victory, seed, bema).CosmeticId == TimedBemaRules.BemaCosmeticId;
            }

            Assert.IsTrue(sawBemaLamp, "the Bema lamp joins the cosmetic table in June");
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_EveryEnumValueHasPtAndEn()
        {
            var keys = new HashSet<string>(TimedEventTexts.Entries.Select(e => e.Key));
            foreach (TimedEventRefusal r in Enum.GetValues(typeof(TimedEventRefusal))) Assert.IsTrue(keys.Contains(TimedEventTexts.RefusalKey(r)), r.ToString());
            foreach (TimedEventKind k in Enum.GetValues(typeof(TimedEventKind))) Assert.IsTrue(keys.Contains(TimedEventTexts.NameKey(k)), k.ToString());
            foreach (TimedEventPhase p in Enum.GetValues(typeof(TimedEventPhase))) Assert.IsTrue(keys.Contains(TimedEventTexts.PhaseKey(p)), p.ToString());
            foreach (TimedRewardBand b in Enum.GetValues(typeof(TimedRewardBand))) Assert.IsTrue(keys.Contains(TimedEventTexts.BandKey(b)), b.ToString());
            foreach (TimedEnemyRole r in Enum.GetValues(typeof(TimedEnemyRole))) Assert.IsTrue(keys.Contains(TimedEventTexts.RoleKey(r)), r.ToString());
            foreach (TimedEventOutcome o in Enum.GetValues(typeof(TimedEventOutcome))) Assert.IsTrue(keys.Contains(TimedEventTexts.OutcomeKey(o)), o.ToString());
            Assert.AreEqual(keys.Count, TimedEventTexts.Entries.Count, "no duplicate keys");
            Assert.IsTrue(TimedEventTexts.Entries.All(e => !string.IsNullOrEmpty(e.En) && !string.IsNullOrEmpty(e.PtBR)));
            Assert.AreEqual("Pode entrar.", TimedEventTexts.Get(null, TimedEventTexts.RefusalKey(TimedEventRefusal.None)));
            Assert.AreEqual("Caçada a Ohyah", TimedEventTexts.Name(null, TimedEventKind.CacadaAOhyah));
        }
    }
}
