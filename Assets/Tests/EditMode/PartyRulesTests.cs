using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure party ("Grupo", TODO.md 12.3): invite / accept / decline / expire, leave, kick, leadership, the 5-member
    /// cap, XP split by proximity with the MU member bonus and the level band, the four loot modes, the family bonuses,
    /// no friendly fire and the texts.
    /// </summary>
    public class PartyRulesTests
    {
        private static PartyState PartyOf(params string[] ids)
        {
            var party = new PartyState(ids[0], null);
            for (int i = 1; i < ids.Length; i++)
            {
                Assert.IsTrue(PartyRules.Invite(party, ids[0], ids[i], null, false, 0d).Ok);
                Assert.IsTrue(PartyRules.Accept(party, ids[i], 1d).Ok);
            }

            return party;
        }

        private static PartyKillMember At(string id, int level, float x = 0f, bool alive = true, CharacterId? cousin = null, bool away = false) =>
            new PartyKillMember(id, level, alive, new Vector3(x, 0f, 0f), cousin, away);

        // ------------------------------------------------------------------ membership

        [Test]
        public void InviteAndAccept_AddsMemberInJoinOrder()
        {
            var party = new PartyState("a", CharacterId.Maithe);
            PartyResult invite = PartyRules.Invite(party, "a", "b", CharacterId.Icaro, false, 0d);
            Assert.IsTrue(invite.Ok);
            Assert.IsFalse(invite.PartyDisbanded);
            Assert.AreEqual(1, party.Invites.Count);

            Assert.IsTrue(PartyRules.Accept(party, "b", 10d).Ok);
            Assert.AreEqual(new[] { "a", "b" }, party.Members.Select(m => m.PlayerId).ToArray());
            Assert.AreEqual(CharacterId.Icaro, party.Find("b").Cousin);
            Assert.AreEqual(0, party.Invites.Count);
            Assert.AreEqual("a", party.LeaderId);
        }

        [Test]
        public void Invite_RefusalReasons()
        {
            PartyState party = PartyOf("a", "b");
            Assert.AreEqual(PartyRefusal.NotLeader, PartyRules.Invite(party, "b", "c", null, false, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.NotMember, PartyRules.Invite(party, "x", "c", null, false, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.CannotTargetSelf, PartyRules.Invite(party, "a", "a", null, false, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.AlreadyInParty, PartyRules.Invite(party, "a", "b", null, false, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.AlreadyInParty, PartyRules.Invite(party, "a", "c", null, true, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.InvalidPlayer, PartyRules.Invite(party, "a", "", null, false, 0d).Refusal);
            Assert.IsTrue(PartyRules.Invite(party, "a", "c", null, false, 0d).Ok);
            Assert.AreEqual(PartyRefusal.AlreadyInvited, PartyRules.Invite(party, "a", "c", null, false, 1d).Refusal);
            // A stale invite does not block a new one.
            Assert.IsTrue(PartyRules.Invite(party, "a", "c", null, false, PartyRules.InviteSeconds + 1d).Ok);
        }

        [Test]
        public void Party_CapsAtFiveMembers()
        {
            PartyState party = PartyOf("a", "b", "c", "d");
            Assert.IsTrue(PartyRules.Invite(party, "a", "e", null, false, 0d).Ok);
            Assert.IsTrue(PartyRules.Invite(party, "a", "f", null, false, 0d).Ok);
            Assert.IsTrue(PartyRules.Accept(party, "e", 1d).Ok);
            Assert.AreEqual(PartyRules.MaxMembers, party.Count);
            Assert.AreEqual(PartyRefusal.PartyFull, PartyRules.Accept(party, "f", 1d).Refusal);
            Assert.AreEqual(PartyRefusal.PartyFull, PartyRules.Invite(party, "a", "g", null, false, 1d).Refusal);
            Assert.AreEqual(5, party.Count);
        }

        [Test]
        public void Accept_AfterExpiry_IsRefusedAndFormingPartyDisbands()
        {
            var party = new PartyState("a", null);
            PartyRules.Invite(party, "a", "b", null, false, 0d);
            PartyResult late = PartyRules.Accept(party, "b", PartyRules.InviteSeconds);
            Assert.AreEqual(PartyRefusal.InviteExpired, late.Refusal);
            Assert.IsTrue(late.PartyDisbanded);
            Assert.IsTrue(party.IsDisbanded);
            Assert.AreEqual(PartyRefusal.Disbanded, PartyRules.Invite(party, "a", "c", null, false, 0d).Refusal);
        }

        [Test]
        public void Decline_And_NoInvite()
        {
            var party = new PartyState("a", null);
            Assert.AreEqual(PartyRefusal.NoInvite, PartyRules.Accept(party, "b", 0d).Refusal);
            PartyRules.Invite(party, "a", "b", null, false, 0d);
            PartyRules.Invite(party, "a", "c", null, false, 0d);
            Assert.IsTrue(PartyRules.Decline(party, "b").Ok);
            Assert.IsFalse(party.IsDisbanded, "another invite is still pending");
            Assert.AreEqual(PartyRefusal.NoInvite, PartyRules.Decline(party, "b").Refusal);
            Assert.IsTrue(PartyRules.Decline(party, "c").PartyDisbanded);
        }

        [Test]
        public void ExpireInvites_ReturnsExpiredIds()
        {
            PartyState party = PartyOf("a", "b");
            PartyRules.Invite(party, "a", "c", null, false, 0d);
            PartyRules.Invite(party, "a", "d", null, false, 30d);
            CollectionAssert.IsEmpty(PartyRules.ExpireInvites(party, 59d));
            CollectionAssert.AreEqual(new[] { "c" }, PartyRules.ExpireInvites(party, 60d));
            CollectionAssert.AreEqual(new[] { "d" }, PartyRules.ExpireInvites(party, 200d));
            Assert.IsFalse(party.IsDisbanded);
        }

        [Test]
        public void LeaderLeaving_PassesToOldestMember()
        {
            PartyState party = PartyOf("a", "b", "c");
            Assert.IsTrue(PartyRules.PassLeadership(party, "a", "c").Ok);
            Assert.AreEqual("c", party.LeaderId);

            PartyResult left = PartyRules.Leave(party, "c");
            Assert.IsTrue(left.Ok);
            Assert.AreEqual("a", left.LeaderId, "the founder is the oldest");

            PartyRules.Invite(party, "a", "d", null, false, 0d);
            PartyRules.Accept(party, "d", 0d);
            PartyRules.Leave(party, "a");
            Assert.AreEqual("b", party.LeaderId, "b joined before d");
        }

        [Test]
        public void Kick_And_Pass_RefusalReasons()
        {
            PartyState party = PartyOf("a", "b", "c");
            Assert.AreEqual(PartyRefusal.NotLeader, PartyRules.Kick(party, "b", "c").Refusal);
            Assert.AreEqual(PartyRefusal.CannotTargetSelf, PartyRules.Kick(party, "a", "a").Refusal);
            Assert.AreEqual(PartyRefusal.TargetNotMember, PartyRules.Kick(party, "a", "z").Refusal);
            Assert.AreEqual(PartyRefusal.NotMember, PartyRules.PassLeadership(party, "z", "b").Refusal);
            Assert.AreEqual(PartyRefusal.NotLeader, PartyRules.SetLootMode(party, "b", PartyLootMode.NeedRoll).Refusal);
            Assert.IsTrue(PartyRules.SetLootMode(party, "a", PartyLootMode.NeedRoll).Ok);
            Assert.AreEqual(PartyLootMode.NeedRoll, party.LootMode);

            Assert.IsTrue(PartyRules.Kick(party, "a", "c").Ok);
            Assert.IsFalse(party.IsMember("c"));
            PartyResult last = PartyRules.Kick(party, "a", "b");
            Assert.IsTrue(last.Ok);
            Assert.IsTrue(last.PartyDisbanded);
            Assert.IsNull(party.LeaderId);
            Assert.AreEqual(PartyRefusal.Disbanded, PartyRules.Leave(party, "a").Refusal);
        }

        [Test]
        public void Leave_NotMember_AndLastPair()
        {
            PartyState party = PartyOf("a", "b");
            Assert.AreEqual(PartyRefusal.NotMember, PartyRules.Leave(party, "z").Refusal);
            Assert.IsTrue(PartyRules.Leave(party, "a").PartyDisbanded);
            Assert.AreEqual(0, party.Count);
        }

        [Test]
        public void NullParty_IsRefusedNotThrown()
        {
            Assert.AreEqual(PartyRefusal.Disbanded, PartyRules.Invite(null, "a", "b", null, false, 0d).Refusal);
            Assert.AreEqual(PartyRefusal.Disbanded, PartyRules.Accept(null, "a", 0d).Refusal);
            Assert.AreEqual(PartyRefusal.Disbanded, PartyRules.Kick(null, "a", "b").Refusal);
            CollectionAssert.IsEmpty(PartyRules.ExpireInvites(null, 0d));
        }

        // ------------------------------------------------------------------ XP

        [Test]
        public void SoloSplit_MatchesSinglePlayerKillXp()
        {
            int[][] cases =
            {
                new[] { 40, 10, 1, 10 }, new[] { 40, 60, 1, 60 }, new[] { 40, 60, 1, 75 }, new[] { 40, 60, 18, 90 },
                new[] { 300, 120, 23, 125 }, new[] { 12, 3, 1, 40 },
            };
            foreach (int[] c in cases)
            {
                int expected = EnemyLevelScaling.LeveledKillExperience(c[0], c[1], c[2], c[3]);
                List<PartyXpShare> shares = PartyXpRules.Split(c[0], c[1], c[2], new[] { At("solo", c[3]) }, Vector3.zero);
                Assert.AreEqual(expected, shares[0].Experience, string.Join(",", c));
            }

            // A creature without a level (campaign) keeps the asset XP.
            Assert.AreEqual(77, PartyXpRules.Split(77, 0, 1, new[] { At("solo", 30) }, Vector3.zero)[0].Experience);
        }

        [Test]
        public void PoolMultiplier_GrowsWithMembers()
        {
            CollectionAssert.AreEqual(new[] { 1f, 1.2f, 1.5f, 1.8f, 2.2f }, Enumerable.Range(1, 5).Select(PartyXpRules.PoolMultiplier).ToArray());
            Assert.AreEqual(1f, PartyXpRules.PoolMultiplier(0));
            Assert.AreEqual(2.2f, PartyXpRules.PoolMultiplier(9));
        }

        [Test]
        public void Split_EqualLevels_SharesPoolEvenly()
        {
            List<PartyXpShare> two = PartyXpRules.Split(100, 0, 1, new[] { At("a", 10), At("b", 10, 5f) }, Vector3.zero);
            Assert.AreEqual(new[] { 60, 60 }, two.Select(s => s.Experience).ToArray());

            var five = Enumerable.Range(0, 5).Select(i => At("p" + i, 10, i)).ToArray();
            Assert.IsTrue(PartyXpRules.Split(100, 0, 1, five, Vector3.zero).All(s => s.Experience == 44));
        }

        [Test]
        public void Split_IsProportionalToLevel()
        {
            List<PartyXpShare> shares = PartyXpRules.Split(100, 0, 1, new[] { At("low", 10), At("high", 30) }, Vector3.zero);
            Assert.AreEqual(30, shares[0].Experience); // 120 × 10/40
            Assert.AreEqual(90, shares[1].Experience); // 120 × 30/40
        }

        [Test]
        public void Split_DeadFarAndAwayGetNothing()
        {
            var members = new[]
            {
                At("here", 10), At("dead", 10, 1f, alive: false), At("far", 10, PartyXpRules.ShareRadius + 0.5f),
                At("edge", 10, PartyXpRules.ShareRadius), At("away", 10, 0f, away: true),
            };
            List<PartyXpShare> shares = PartyXpRules.Split(100, 0, 1, members, Vector3.zero);
            Assert.AreEqual(60, shares[0].Experience);
            Assert.AreEqual(PartyXpCut.Dead, shares[1].Cut);
            Assert.AreEqual(PartyXpCut.TooFar, shares[2].Cut);
            Assert.AreEqual(PartyXpCut.None, shares[3].Cut);
            Assert.AreEqual(60, shares[3].Experience);
            Assert.AreEqual(PartyXpCut.TooFar, shares[4].Cut);
            Assert.AreEqual(0, shares[1].Experience + shares[2].Experience + shares[4].Experience);
        }

        [Test]
        public void Split_LevelBandBurnsTheCarriersShare()
        {
            // 31 is more than 20 above 10: cut, no member bonus, but its level stays in the sum.
            List<PartyXpShare> shares = PartyXpRules.Split(100, 0, 1, new[] { At("low", 10), At("carrier", 31) }, Vector3.zero);
            Assert.AreEqual(PartyXpCut.OutOfLevelBand, shares[1].Cut);
            Assert.AreEqual(0, shares[1].Experience);
            Assert.AreEqual(24, shares[0].Experience); // 100 × 10/41
            Assert.Less(shares[0].Experience, 100, "power-leveling pays less than playing solo");

            // Exactly 20 above is still in the band.
            Assert.AreEqual(PartyXpCut.None, PartyXpRules.Split(100, 0, 1, new[] { At("low", 10), At("ok", 30) }, Vector3.zero)[1].Cut);
        }

        [Test]
        public void Split_LeveledCreature_AppliesEachMembersLevelCut()
        {
            List<PartyXpShare> shares = PartyXpRules.Split(40, 60, 1, new[] { At("even", 60), At("above", 75) }, Vector3.zero);
            double pool = PartyXpRules.BaseKillExperience(40, 60, 1) * (double)PartyXpRules.PoolMultiplier(2);
            Assert.AreEqual((int)Math.Floor(pool * 60 / 135 + 0.5), shares[0].Experience);
            Assert.AreEqual((int)Math.Floor(pool * 75 / 135 * 0.75 + 0.5), shares[1].Experience);
        }

        [Test]
        public void Split_FamilyBonuses()
        {
            List<PartyXpShare> siblings = PartyXpRules.Split(100, 0, 1,
                new[] { At("m", 10, cousin: CharacterId.Maithe), At("i", 10, cousin: CharacterId.Icaro) }, Vector3.zero);
            Assert.IsTrue(siblings.All(s => s.SiblingBond && !s.FamilyComplete));
            Assert.IsTrue(siblings.All(s => s.Experience == 63)); // 60 × 1,05

            List<PartyXpShare> cousins = PartyXpRules.Split(100, 0, 1,
                new[] { At("m", 10, cousin: CharacterId.Maithe), At("l", 10, cousin: CharacterId.Lavine) }, Vector3.zero);
            Assert.IsTrue(cousins.All(s => !s.SiblingBond && s.Experience == 60));

            var family = ((CharacterId[])Enum.GetValues(typeof(CharacterId))).Select((c, i) => At("p" + i, 10, i, cousin: c)).ToArray();
            List<PartyXpShare> full = PartyXpRules.Split(100, 0, 1, family, Vector3.zero);
            Assert.IsTrue(full.All(s => s.FamilyComplete && s.SiblingBond));
            Assert.IsTrue(full.All(s => s.Experience == 51)); // 44 × 1,10 × 1,05

            // The sibling must be at the fight, not just in the roster.
            List<PartyXpShare> apart = PartyXpRules.Split(100, 0, 1,
                new[] { At("m", 10, cousin: CharacterId.Maithe), At("i", 10, 500f, cousin: CharacterId.Icaro) }, Vector3.zero);
            Assert.IsFalse(apart[0].SiblingBond);
            Assert.AreEqual(100, apart[0].Experience);
        }

        [Test]
        public void FamilyRules_Detection()
        {
            Assert.IsTrue(PartyFamilyRules.IsFamilyComplete(new CharacterId?[] { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine }));
            Assert.IsFalse(PartyFamilyRules.IsFamilyComplete(new CharacterId?[] { CharacterId.Joaquim, CharacterId.Joaquim, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine }));
            Assert.IsFalse(PartyFamilyRules.IsFamilyComplete(new CharacterId?[] { null, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine }));
            Assert.IsTrue(PartyFamilyRules.AnySiblingBond(new CharacterId?[] { CharacterId.Maria, null, CharacterId.Joaquim }));
            Assert.IsFalse(PartyFamilyRules.AnySiblingBond(new CharacterId?[] { CharacterId.Maria, CharacterId.Icaro, CharacterId.Lavine }));
            Assert.IsFalse(PartyFamilyRules.HasSiblingBond(null, new CharacterId?[] { CharacterId.Maithe }));
            Assert.AreEqual(1f, PartyFamilyRules.XpMultiplier(false, false));
            Assert.AreEqual(1.155f, PartyFamilyRules.XpMultiplier(true, true), 1e-5f);

            var party = new PartyState("a", CharacterId.Maria);
            CollectionAssert.AreEqual(new CharacterId?[] { CharacterId.Maria }, PartyFamilyRules.CousinsOf(party));
        }

        // ------------------------------------------------------------------ loot

        private static PartyState FourWithRoster(out List<PartyKillMember> statuses)
        {
            PartyState party = PartyOf("a", "b", "c", "d");
            statuses = new List<PartyKillMember> { At("a", 10), At("b", 10, alive: false), At("c", 10, 3f), At("d", 10, 4f) };
            return party;
        }

        [Test]
        public void PresentInJoinOrder_SkipsDeadAndFar()
        {
            PartyState party = FourWithRoster(out List<PartyKillMember> statuses);
            statuses[3] = At("d", 10, 999f);
            CollectionAssert.AreEqual(new[] { "a", "c" }, PartyLootRules.PresentInJoinOrder(party, statuses, Vector3.zero).Select(m => m.PlayerId));
        }

        [Test]
        public void FreeForAll_LeavesEveryDropFree()
        {
            Assert.IsTrue(PartyLootRules.FreeForAll(3).All(a => a.IsFree));
            Assert.AreEqual(3, PartyLootRules.FreeForAll(3).Count);
        }

        [Test]
        public void RoundRobin_CyclesPresentMembersAcrossKills()
        {
            PartyState party = FourWithRoster(out List<PartyKillMember> statuses);
            PartyRules.SetLootMode(party, "a", PartyLootMode.RoundRobin);
            List<PartyMember> present = PartyLootRules.PresentInJoinOrder(party, statuses, Vector3.zero);

            List<PartyLootAssignment> first = PartyLootRules.Assign(party, 2, present, 1);
            CollectionAssert.AreEqual(new[] { "a", "c" }, first.Select(a => a.OwnerId));
            List<PartyLootAssignment> second = PartyLootRules.Assign(party, 3, present, 2);
            CollectionAssert.AreEqual(new[] { "d", "a", "c" }, second.Select(a => a.OwnerId));

            long cursor = -1;
            Assert.IsTrue(PartyLootRules.RoundRobin(2, new List<PartyMember>(), ref cursor).All(a => a.IsFree));
        }

        [Test]
        public void NeedRoll_IsDeterministicAndNeedBeatsGreed()
        {
            PartyState party = PartyOf("a", "b", "c");
            List<PartyMember> present = party.Members.ToList();

            Func<int, string, LootClaim> greed = (drop, id) => LootClaim.Greed;
            List<PartyLootAssignment> once = PartyLootRules.NeedRoll(20, present, 1234, greed);
            List<PartyLootAssignment> again = PartyLootRules.NeedRoll(20, present, 1234, greed);
            CollectionAssert.AreEqual(once.Select(a => a.OwnerId), again.Select(a => a.OwnerId));
            Assert.Greater(once.Select(a => a.OwnerId).Distinct().Count(), 1, "rolls spread the drops");

            // The winner holds the highest roll of the drop's own sequence (rolled in join order).
            for (int drop = 0; drop < once.Count; drop++)
            {
                var random = new System.Random(PartyLootRules.DropSeed(1234, drop));
                int[] rolls = present.Select(_ => random.Next(PartyLootRules.RollMin, PartyLootRules.RollMax + 1)).ToArray();
                Assert.AreEqual(rolls.Max(), once[drop].WinningRoll);
                Assert.AreEqual(present[Array.IndexOf(rolls, rolls.Max())].PlayerId, once[drop].OwnerId, "ties go to the older member");
            }

            List<PartyLootAssignment> need = PartyLootRules.NeedRoll(20, present, 1234, (drop, id) => id == "c" ? LootClaim.Need : LootClaim.Greed);
            Assert.IsTrue(need.All(a => a.OwnerId == "c" && a.WinningClaim == LootClaim.Need));

            Assert.IsTrue(PartyLootRules.NeedRoll(4, present, 1, null).All(a => a.IsFree), "everyone passing leaves it free");
        }

        [Test]
        public void LeaderAssigns_ChoiceOrLeaderOrRoundRobin()
        {
            PartyState party = FourWithRoster(out List<PartyKillMember> statuses);
            PartyRules.SetLootMode(party, "a", PartyLootMode.LeaderAssigns);
            List<PartyMember> present = PartyLootRules.PresentInJoinOrder(party, statuses, Vector3.zero);

            List<PartyLootAssignment> assigned = PartyLootRules.Assign(party, 3, present, 0, leaderChoiceOf: drop => drop == 0 ? "c" : drop == 1 ? "b" : null);
            CollectionAssert.AreEqual(new[] { "c", "a", "a" }, assigned.Select(a => a.OwnerId), "b is dead, null = no choice: the leader keeps it");

            statuses[0] = At("a", 10, 999f);
            present = PartyLootRules.PresentInJoinOrder(party, statuses, Vector3.zero);
            long cursor = -1;
            CollectionAssert.AreEqual(new[] { "c", "d" },
                PartyLootRules.LeaderAssigns(2, present, "a", drop => "c", ref cursor).Select(a => a.OwnerId), "absent leader: round robin");
        }

        [Test]
        public void CanPickUp_OwnerFirstThenEveryonePresent()
        {
            var owned = new PartyLootAssignment(0, "a");
            Assert.IsTrue(PartyLootRules.CanPickUp(owned, "a", true, 0d));
            Assert.IsFalse(PartyLootRules.CanPickUp(owned, "b", true, 5d));
            Assert.IsTrue(PartyLootRules.CanPickUp(owned, "b", true, PartyLootRules.OwnerLockSeconds));
            Assert.IsFalse(PartyLootRules.CanPickUp(owned, "b", false, 999d));
            var free = new PartyLootAssignment(0, null);
            Assert.IsTrue(PartyLootRules.CanPickUp(free, "b", true, 0d));
            Assert.IsFalse(PartyLootRules.CanPickUp(free, "b", false, 0d));
            Assert.IsFalse(PartyLootRules.CanPickUp(free, null, true, 0d));
        }

        // ------------------------------------------------------------------ friendly fire

        [Test]
        public void NoFriendlyFire_CombinesWithSafeZone()
        {
            Assert.IsFalse(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, sameParty: true, pvpAllowed: true));
            Assert.IsTrue(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, sameParty: false, pvpAllowed: true));
            Assert.IsFalse(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, sameParty: false));
            Assert.IsTrue(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Monster, ZoneKind.Wilds, sameParty: true));
            Assert.IsTrue(PartyCombatRules.CanHarm(ZoneActor.Monster, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, sameParty: false));
            Assert.IsFalse(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.City, ZoneActor.Monster, ZoneKind.Wilds, sameParty: false));
            Assert.IsTrue(PartyCombatRules.CanHarm(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, sameParty: true, targetIsSelf: true));

            PartyState party = PartyOf("a", "b");
            Assert.IsFalse(PartyCombatRules.CanHarmPlayer(party, "a", ZoneKind.Wilds, "b", ZoneKind.Wilds, pvpAllowed: true));
            Assert.IsTrue(PartyCombatRules.CanHarmPlayer(party, "a", ZoneKind.Wilds, "x", ZoneKind.Wilds, pvpAllowed: true));
            Assert.IsFalse(PartyCombatRules.CanHarmPlayer(party, "a", ZoneKind.City, "x", ZoneKind.Wilds, pvpAllowed: true));
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_CoverEveryReasonCutAndMode()
        {
            var keys = new List<string>();
            keys.AddRange(((PartyRefusal[])Enum.GetValues(typeof(PartyRefusal))).Select(PartyTexts.RefusalKey));
            keys.AddRange(((PartyXpCut[])Enum.GetValues(typeof(PartyXpCut))).Select(PartyTexts.XpCutKey));
            keys.AddRange(((PartyLootMode[])Enum.GetValues(typeof(PartyLootMode))).Select(PartyTexts.LootModeKey));
            keys.AddRange(new[]
            {
                PartyTexts.FamilyCompleteName, PartyTexts.FamilyCompleteDescription, PartyTexts.SiblingBondName,
                PartyTexts.SiblingBondDescription, PartyTexts.InviteReceived, PartyTexts.MemberJoined, PartyTexts.MemberLeft, PartyTexts.NewLeader,
            });

            Assert.AreEqual(keys.Count, keys.Distinct().Count());
            foreach (string key in keys)
            {
                Assert.IsFalse(string.IsNullOrEmpty(PartyTexts.GetFallback(key)), key);
                Assert.IsFalse(string.IsNullOrEmpty(PartyTexts.Entries.Single(e => e.Key == key).En), key);
            }

            Assert.AreEqual("party.refusal.already_in_party", PartyTexts.RefusalKey(PartyRefusal.AlreadyInParty));
            Assert.AreEqual("O grupo está cheio (5 membros).", PartyTexts.Refusal(null, PartyRefusal.PartyFull));
            Assert.AreEqual("Família Completa", PartyTexts.Get(null, PartyTexts.FamilyCompleteName));
            Assert.AreEqual("Ana entrou no grupo.", PartyTexts.Format(null, PartyTexts.MemberJoined, "Ana"));
        }
    }
}
