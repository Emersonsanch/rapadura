using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Party
{
    /// <summary>How a party shares drops (picked by the leader, <see cref="PartyRules.SetLootMode"/>). Texts: <see cref="PartyTexts.LootModeKey"/>. Never renumber.</summary>
    public enum PartyLootMode
    {
        /// <summary>"Livre": anyone present picks anything up.</summary>
        FreeForAll = 0,

        /// <summary>"Rodízio": each drop goes to the next present member, in join order.</summary>
        RoundRobin = 1,

        /// <summary>"Por necessidade": each present member says Need / Greed / Pass and rolls 1–100.</summary>
        NeedRoll = 2,

        /// <summary>"O líder distribui": the leader names who gets each drop.</summary>
        LeaderAssigns = 3,
    }

    /// <summary>A member's answer to one drop in <see cref="PartyLootMode.NeedRoll"/>.</summary>
    public enum LootClaim
    {
        Pass = 0,
        Greed = 1,
        Need = 2,
    }

    /// <summary>Who owns drop <see cref="DropIndex"/>. <see cref="OwnerId"/> null = free for any present member.</summary>
    public readonly struct PartyLootAssignment
    {
        public readonly int DropIndex;
        public readonly string OwnerId;

        /// <summary>The winning 1–100 roll and claim (<see cref="PartyLootMode.NeedRoll"/> only; 0 / Pass otherwise).</summary>
        public readonly int WinningRoll;
        public readonly LootClaim WinningClaim;

        public PartyLootAssignment(int dropIndex, string ownerId, int winningRoll = 0, LootClaim winningClaim = LootClaim.Pass)
        {
            DropIndex = dropIndex;
            OwnerId = ownerId;
            WinningRoll = winningRoll;
            WinningClaim = winningClaim;
        }

        public bool IsFree => OwnerId == null;
    }

    /// <summary>
    /// Group loot (TODO.md 12.3): which member owns each drop of a kill, for the four modes. Pure and deterministic:
    /// works on drop indices (the caller maps them to its <c>LootDrop</c> list from <c>EnemyLootRoller.Roll</c>), on the
    /// present members in join order (<see cref="PresentInJoinOrder"/>: alive and within <see cref="PartyXpRules.ShareRadius"/>,
    /// the same test as the XP) and on a seed chosen by the server, never the client. The drops themselves are still
    /// rolled once per kill (no extra loot for being a party), and the owner then puts them in the bag through
    /// <c>LootGrantRules.Grant</c> as today.
    /// <list type="bullet">
    /// <item><b>Livre</b>: every drop is free (<see cref="PartyLootAssignment.IsFree"/>).</item>
    /// <item><b>Rodízio</b>: the next present member after <see cref="PartyState.RoundRobinCursor"/>, wrapping; absent
    /// members are skipped, not owed.</item>
    /// <item><b>Por necessidade</b>: everyone present rolls 1–100 for every drop (in join order, from
    /// <c>seed</c> + drop index, so an answer never changes another member's roll); Need beats Greed, then the higher
    /// roll, then the older member; all Pass leaves it free.</item>
    /// <item><b>O líder distribui</b>: the leader's choice when it names a present member, otherwise the leader keeps it to
    /// hand over; a leader not present falls back to the round robin.</item>
    /// </list>
    /// An owned drop opens to everyone present after <see cref="OwnerLockSeconds"/> (<see cref="CanPickUp"/>), so nothing
    /// rots on the ground when its owner walks off.
    /// </summary>
    public static class PartyLootRules
    {
        public const float OwnerLockSeconds = 30f;

        public const int RollMin = 1;
        public const int RollMax = 100;

        /// <summary>The members of <paramref name="party"/> eligible at the kill (<see cref="PartyXpRules.IsEligible"/>), in join order.</summary>
        public static List<PartyMember> PresentInJoinOrder(PartyState party, IReadOnlyList<PartyKillMember> statuses, Vector3 killPosition)
        {
            var present = new List<PartyMember>();
            if (party == null || statuses == null)
            {
                return present;
            }

            foreach (PartyMember member in party.Members)
            {
                foreach (PartyKillMember status in statuses)
                {
                    if (status.PlayerId == member.PlayerId)
                    {
                        if (PartyXpRules.IsEligible(status, killPosition))
                        {
                            present.Add(member);
                        }

                        break;
                    }
                }
            }

            present.Sort((a, b) => a.JoinOrder.CompareTo(b.JoinOrder));
            return present;
        }

        /// <summary>The party's mode, updating its round-robin cursor. <paramref name="claimOf"/> (drop, player) and
        /// <paramref name="leaderChoiceOf"/> (drop → player) are only read by their mode; null = everyone passes / no choice.</summary>
        public static List<PartyLootAssignment> Assign(PartyState party, int dropCount, IReadOnlyList<PartyMember> present, int seed,
            Func<int, string, LootClaim> claimOf = null, Func<int, string> leaderChoiceOf = null)
        {
            if (party == null)
            {
                return FreeForAll(dropCount);
            }

            long cursor = party.RoundRobinCursor;
            List<PartyLootAssignment> result;
            switch (party.LootMode)
            {
                case PartyLootMode.RoundRobin:
                    result = RoundRobin(dropCount, present, ref cursor);
                    break;
                case PartyLootMode.NeedRoll:
                    result = NeedRoll(dropCount, present, seed, claimOf);
                    break;
                case PartyLootMode.LeaderAssigns:
                    result = LeaderAssigns(dropCount, present, party.LeaderId, leaderChoiceOf, ref cursor);
                    break;
                default:
                    result = FreeForAll(dropCount);
                    break;
            }

            party.RoundRobinCursor = cursor;
            return result;
        }

        public static List<PartyLootAssignment> FreeForAll(int dropCount)
        {
            var result = new List<PartyLootAssignment>();
            for (int i = 0; i < dropCount; i++)
            {
                result.Add(new PartyLootAssignment(i, null));
            }

            return result;
        }

        /// <summary><paramref name="cursor"/> = JoinOrder of the last recipient (-1 = none yet); advanced past each drop handed out.</summary>
        public static List<PartyLootAssignment> RoundRobin(int dropCount, IReadOnlyList<PartyMember> present, ref long cursor)
        {
            var result = new List<PartyLootAssignment>();
            for (int i = 0; i < dropCount; i++)
            {
                PartyMember next = NextAfter(present, cursor);
                if (next == null)
                {
                    result.Add(new PartyLootAssignment(i, null));
                    continue;
                }

                cursor = next.JoinOrder;
                result.Add(new PartyLootAssignment(i, next.PlayerId));
            }

            return result;
        }

        public static List<PartyLootAssignment> NeedRoll(int dropCount, IReadOnlyList<PartyMember> present, int seed, Func<int, string, LootClaim> claimOf)
        {
            var result = new List<PartyLootAssignment>();
            for (int i = 0; i < dropCount; i++)
            {
                var random = new System.Random(DropSeed(seed, i));
                string owner = null;
                int bestRoll = 0;
                LootClaim bestClaim = LootClaim.Pass;
                if (present != null)
                {
                    foreach (PartyMember member in present)
                    {
                        int roll = random.Next(RollMin, RollMax + 1);
                        LootClaim claim = claimOf != null ? claimOf(i, member.PlayerId) : LootClaim.Pass;
                        if (claim == LootClaim.Pass)
                        {
                            continue;
                        }

                        // Strictly better only: an equal claim and roll keeps the older member (join order).
                        if (claim > bestClaim || (claim == bestClaim && roll > bestRoll))
                        {
                            owner = member.PlayerId;
                            bestRoll = roll;
                            bestClaim = claim;
                        }
                    }
                }

                result.Add(owner == null ? new PartyLootAssignment(i, null) : new PartyLootAssignment(i, owner, bestRoll, bestClaim));
            }

            return result;
        }

        public static List<PartyLootAssignment> LeaderAssigns(int dropCount, IReadOnlyList<PartyMember> present, string leaderId,
            Func<int, string> leaderChoiceOf, ref long cursor)
        {
            if (!Contains(present, leaderId))
            {
                return RoundRobin(dropCount, present, ref cursor);
            }

            var result = new List<PartyLootAssignment>();
            for (int i = 0; i < dropCount; i++)
            {
                string choice = leaderChoiceOf?.Invoke(i);
                result.Add(new PartyLootAssignment(i, Contains(present, choice) ? choice : leaderId));
            }

            return result;
        }

        /// <summary>
        /// May <paramref name="pickerId"/> take the drop? A free drop: anyone present. An owned one: its owner, or anyone
        /// present once <see cref="OwnerLockSeconds"/> have passed.
        /// </summary>
        public static bool CanPickUp(PartyLootAssignment assignment, string pickerId, bool pickerPresent, double secondsSinceDrop)
        {
            if (string.IsNullOrEmpty(pickerId))
            {
                return false;
            }

            if (assignment.OwnerId == pickerId)
            {
                return true;
            }

            return pickerPresent && (assignment.IsFree || secondsSinceDrop >= OwnerLockSeconds);
        }

        /// <summary>Per-drop seed: the server's kill seed and the drop index.</summary>
        public static int DropSeed(int seed, int dropIndex) => unchecked(seed * 486187739 + (dropIndex + 1) * 16777619);

        private static PartyMember NextAfter(IReadOnlyList<PartyMember> present, long cursor)
        {
            if (present == null || present.Count == 0)
            {
                return null;
            }

            PartyMember first = null;
            PartyMember after = null;
            foreach (PartyMember member in present)
            {
                if (first == null || member.JoinOrder < first.JoinOrder)
                {
                    first = member;
                }

                if (member.JoinOrder > cursor && (after == null || member.JoinOrder < after.JoinOrder))
                {
                    after = member;
                }
            }

            return after ?? first;
        }

        private static bool Contains(IReadOnlyList<PartyMember> present, string playerId)
        {
            if (present == null || string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            foreach (PartyMember member in present)
            {
                if (member.PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
