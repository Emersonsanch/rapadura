using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.Rebirth;
using Rapadura.Gameplay.TimedEvents;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The "nível efetivo" of a reset hero (Docs/Network.md §15): <c>max(level, PeakLevel)</c> once it has reset, the bare
    /// level otherwise. A reset keeps the level-400 gear and items have no level requirement, so the arena (minimum and
    /// queue), open PvP, the event rooms and the party's anti power-leveling band read it; XP stays on the current level.
    /// Every gate is unchanged for a hero that never reset (the optional parameter defaults to the level).
    /// </summary>
    public class EffectiveLevelTests
    {
        /// <summary>A record after one "Volta da Luz" at level 400 (the hero is now level 1 with its old gear).</summary>
        private static RebirthState ResetAt400()
        {
            var state = new RebirthState("r");
            RebirthResult result = RebirthRules.TryReset(state, new RebirthCandidate("r", 400, 1000000, RebirthRules.ResetCityId), 0d);
            Assert.IsTrue(result.Ok, result.Refusal.ToString());
            return state;
        }

        // ------------------------------------------------------------------ the rule

        [Test]
        public void EffectiveLevel_IsTheLevelWithoutReset()
        {
            Assert.AreEqual(1, new RebirthState("p").EffectiveLevel(1));
            Assert.AreEqual(250, new RebirthState("p").EffectiveLevel(250));
            Assert.AreEqual(1, new RebirthState("p", peakLevel: 400).EffectiveLevel(1), "a peak without any reset counts for nothing");
            Assert.AreEqual(7, RebirthRules.EffectiveLevel(null, 7));
        }

        [Test]
        public void EffectiveLevel_IsThePeakAfterAReset_AndTheLevelOnceAboveIt()
        {
            RebirthState state = ResetAt400();
            Assert.AreEqual(400, state.PeakLevel);
            Assert.AreEqual(400, state.EffectiveLevel(1));
            Assert.AreEqual(400, RebirthRules.EffectiveLevel(state, 120));
            Assert.AreEqual(450, state.EffectiveLevel(450));
        }

        [Test]
        public void EffectiveLevel_SurvivesTheGrandReset()
        {
            // The grand reset also keeps the gear, so the peak (500) is not lowered by it.
            var state = new RebirthState("g", resets: 0, grandResets: 1, peakLevel: 500);
            Assert.AreEqual(500, state.EffectiveLevel(1));
        }

        // ------------------------------------------------------------------ arena

        [Test]
        public void Arena_MinimumAndQueueReadTheEffectiveLevel()
        {
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);
            Assert.AreEqual(PvpRefusal.LevelTooLow,
                ArenaRules.CheckEntry(ArenaBracket.Solo, new[] { new ArenaQueueMember("n", 1500, 1) }, null), "a true level-1 hero");
            Assert.AreEqual(PvpRefusal.None,
                ArenaRules.CheckEntry(ArenaBracket.Solo, new[] { new ArenaQueueMember("r", 1500, 1, effectiveLevel: effective) }, null));

            var queue = new ArenaQueue();
            Assert.AreEqual(PvpRefusal.LevelTooLow, queue.Enqueue("n", ArenaBracket.Solo, new[] { new ArenaQueueMember("n", 1500, 29) }, null, 0d));
            Assert.AreEqual(PvpRefusal.None,
                queue.Enqueue("r", ArenaBracket.Solo, new[] { new ArenaQueueMember("r", 1500, 1, effectiveLevel: effective) }, null, 0d));
        }

        [Test]
        public void Arena_EffectiveLevelNeverGoesBelowTheLevel()
        {
            Assert.AreEqual(50, new ArenaQueueMember("a", 1500, 50, effectiveLevel: 10).EffectiveLevel);
            Assert.AreEqual(50, new ArenaQueueMember("b", 1500, 50).EffectiveLevel);
        }

        // ------------------------------------------------------------------ open PvP

        private static PvpCombatant At(string id, int level, int effectiveLevel = 0) =>
            new PvpCombatant(id, ZoneKind.Wilds, MapRegion.CostaDasMoncoes, level, effectiveLevel: effectiveLevel);

        [Test]
        public void OpenPvp_GateReadsTheEffectiveLevelOfBothSides()
        {
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);
            Assert.IsFalse(PvpAreaRules.OpenPvpBetween(At("a", 100), At("n", 1)), "a true level-1 hero is never a target");
            Assert.IsFalse(PvpAreaRules.OpenPvpBetween(At("n", 1), At("a", 100)), "nor a hunter");
            Assert.IsTrue(PvpAreaRules.OpenPvpBetween(At("a", 100), At("r", 1, effective)), "the reset hero with level-400 gear is fair game");
            Assert.IsTrue(PvpAreaRules.OpenPvpBetween(At("r", 1, effective), At("a", 100)), "and can hunt");
            Assert.AreEqual(100, At("a", 100, 5).EffectiveLevel);
        }

        // ------------------------------------------------------------------ event rooms

        private static DateTime Brt(int y, int m, int d, int h, int min = 0) => new DateTime(y, m, d, h, min, 0, DateTimeKind.Utc).AddHours(3);

        private static TimedEventRoomSet SetAt(TimedEventDefinition def, DateTime utc) => new TimedEventRoomSet(def, TimedEventAgenda.Next(def, utc));

        [Test]
        public void EventRoom_ByEffectiveLevel()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);

            TimedEventEntryResult lowbie = TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, open),
                new TimedEventEntrant("n", 1, 1, 0), new TimedEventEntryLedger(), open);
            Assert.AreEqual(TimedEventRefusal.LevelTooLow, lowbie.Refusal);

            TimedEventEntryResult reset = TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, open),
                new TimedEventEntrant("r", 1, 1, 0, effectiveLevel: effective), new TimedEventEntryLedger(), open);
            Assert.IsTrue(reset.Ok, reset.Refusal.ToString());
            Assert.AreEqual(TimedEventCatalog.Vigilia.RoomFor(400), reset.RoomIndex, "the room of the level the gear was earned at");

            TimedEventEntryResult plain = TimedEventRules.TryEnter(SetAt(TimedEventCatalog.Vigilia, open),
                new TimedEventEntrant("a", 50, 1, 0), new TimedEventEntryLedger(), open);
            Assert.AreEqual(1, plain.RoomIndex, "unchanged without reset");
        }

        [Test]
        public void EventRoom_PartyWithAResetHeroGoesToTheEffectiveBand()
        {
            DateTime open = Brt(2026, 11, 23, 12, 56);
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);
            var party = new PartyState("L", null);
            Assert.IsTrue(PartyRules.Invite(party, "L", "R", null, false, 0d).Ok);
            Assert.IsTrue(PartyRules.Accept(party, "R", 1d).Ok);
            var ledgers = new Dictionary<string, TimedEventEntryLedger>();
            Func<string, TimedEventEntryLedger> ledger = id =>
            {
                if (!ledgers.TryGetValue(id, out TimedEventEntryLedger l))
                {
                    l = new TimedEventEntryLedger();
                    ledgers[id] = l;
                }

                return l;
            };

            // A real level-15 hero next to a level-400 is two bands.
            var mismatch = new List<TimedEventEntrant> { new TimedEventEntrant("L", 400, 1, 0), new TimedEventEntrant("R", 15, 1, 0) };
            Assert.AreEqual(TimedEventRefusal.PartyRoomMismatch,
                TimedEventRules.TryEnterParty(SetAt(TimedEventCatalog.Vigilia, open), party, "L", mismatch, ledger, open).Refusal);

            // The reset hero at level 1 counts as 400: same room as its friend.
            var together = new List<TimedEventEntrant>
            {
                new TimedEventEntrant("L", 400, 1, 0),
                new TimedEventEntrant("R", 1, 1, 0, effectiveLevel: effective),
            };
            TimedEventEntryResult ok = TimedEventRules.TryEnterParty(SetAt(TimedEventCatalog.Vigilia, open), party, "L", together, ledger, open);
            Assert.IsTrue(ok.Ok, ok.Refusal + " / " + ok.MemberRefusal);
            Assert.AreEqual(TimedEventCatalog.Vigilia.RoomFor(400), ok.RoomIndex);
        }

        // ------------------------------------------------------------------ party band

        private static PartyKillMember Member(string id, int level, int effectiveLevel = 0) =>
            new PartyKillMember(id, level, true, Vector3.zero, effectiveLevel: effectiveLevel);

        [Test]
        public void PartyBand_ResetHeroCarryingALowbieBurnsLikeItsEffectiveLevel()
        {
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);

            // By the bare level (1 and 10) both would be in band and the lowbie would get 120 × 10/11 = 109.
            List<PartyXpShare> shares = PartyXpRules.Split(100, 0, 1, new[] { Member("low", 10), Member("reset", 1, effective) }, Vector3.zero);
            Assert.AreEqual(PartyXpCut.OutOfLevelBand, shares[1].Cut);
            Assert.AreEqual(0, shares[1].Experience);
            Assert.AreEqual(2, shares[0].Experience, "100 × 10/410: power-leveling still pays less than solo");
        }

        [Test]
        public void PartyBand_ResetHeroWithItsOldFriendsStaysInBand_ButEarnsByItsCurrentLevel()
        {
            int effective = RebirthRules.EffectiveLevel(ResetAt400(), 1);
            List<PartyXpShare> shares = PartyXpRules.Split(100, 0, 1, new[] { Member("friend", 400), Member("reset", 1, effective) }, Vector3.zero);
            Assert.AreEqual(PartyXpCut.None, shares[0].Cut, "the level-400 friend is no longer burned by the reset hero's level 1");
            Assert.AreEqual(PartyXpCut.None, shares[1].Cut);
            Assert.AreEqual(60, shares[0].Experience, "120 × 400/800");
            Assert.AreEqual(1, shares[1].Experience, "120 × 1/800 (its own part stays on level 1), floored at 1");
        }

        [Test]
        public void PartyBand_WithoutResetEveryNumberIsTheSame()
        {
            List<PartyXpShare> before = PartyXpRules.Split(100, 0, 1,
                new[] { new PartyKillMember("low", 10, true, Vector3.zero), new PartyKillMember("carrier", 31, true, Vector3.zero) }, Vector3.zero);
            List<PartyXpShare> after = PartyXpRules.Split(100, 0, 1, new[] { Member("low", 10, 10), Member("carrier", 31, 31) }, Vector3.zero);
            Assert.AreEqual(24, before[0].Experience);
            Assert.AreEqual(before[0].Experience, after[0].Experience);
            Assert.AreEqual(before[1].Cut, after[1].Cut);
            Assert.AreEqual(31, Member("x", 31, 5).EffectiveLevel, "an effective level below the level is ignored");
        }
    }
}
