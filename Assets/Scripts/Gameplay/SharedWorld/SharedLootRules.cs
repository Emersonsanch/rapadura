using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Party;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>
    /// Who may pick up drop <see cref="DropIndex"/> of a shared-world kill during the protection. Both owners null = free
    /// for anyone near. <see cref="OwnerPlayerId"/> set = that player; only <see cref="OwnerPartyId"/> set = any member of
    /// that party (a "Livre" party, or a need roll everyone passed).
    /// </summary>
    public readonly struct SharedLootDrop
    {
        public readonly int DropIndex;
        public readonly string OwnerPlayerId;
        public readonly string OwnerPartyId;

        public SharedLootDrop(int dropIndex, string ownerPlayerId, string ownerPartyId)
        {
            DropIndex = dropIndex;
            OwnerPlayerId = string.IsNullOrEmpty(ownerPlayerId) ? null : ownerPlayerId;
            OwnerPartyId = string.IsNullOrEmpty(ownerPartyId) ? null : ownerPartyId;
        }

        public bool IsFree => OwnerPlayerId == null && OwnerPartyId == null;
    }

    /// <summary>
    /// The loot of a shared-world kill (MU style), on top of <see cref="PartyLootRules"/>: the drops are rolled once
    /// (<c>EnemyLootRoller.Roll</c>, never more because more players hit) and belong to the <see cref="KillOwner"/>:
    /// <list type="bullet">
    /// <item>a player alone: every drop is theirs;</item>
    /// <item>a party: the leader's mode decides per drop (<see cref="PartyLootRules.Assign"/>, on the members present at the
    /// kill); a drop the mode leaves free stays the party's;</item>
    /// <item>after <see cref="ProtectionSeconds"/> (the party's own <see cref="PartyLootRules.OwnerLockSeconds"/>, 30 s)
    /// every drop is free for anyone near.</item>
    /// </list>
    /// Pure; the server keeps the drop list and the drop time and calls <see cref="CanPickUp"/> on each pick-up request.
    /// </summary>
    public static class SharedLootRules
    {
        /// <summary>The protection of a kill's loot: one number with the party's owner lock.</summary>
        public const float ProtectionSeconds = PartyLootRules.OwnerLockSeconds;

        /// <param name="party">The owner's party (null for a solo owner, or a party the server no longer has).</param>
        /// <param name="present">Its members present at the kill (<see cref="PartyLootRules.PresentInJoinOrder"/>).</param>
        public static List<SharedLootDrop> Assign(KillOwner owner, int dropCount, PartyState party, IReadOnlyList<PartyMember> present, int seed,
            Func<int, string, LootClaim> claimOf = null, Func<int, string> leaderChoiceOf = null)
        {
            var result = new List<SharedLootDrop>();
            if (dropCount <= 0)
            {
                return result;
            }

            if (owner.IsNone)
            {
                for (int i = 0; i < dropCount; i++)
                {
                    result.Add(new SharedLootDrop(i, null, null));
                }

                return result;
            }

            if (!owner.IsParty || party == null || party.IsDisbanded)
            {
                for (int i = 0; i < dropCount; i++)
                {
                    result.Add(new SharedLootDrop(i, owner.PlayerId, owner.PartyId));
                }

                return result;
            }

            foreach (PartyLootAssignment assignment in PartyLootRules.Assign(party, dropCount, present, seed, claimOf, leaderChoiceOf))
            {
                result.Add(new SharedLootDrop(assignment.DropIndex, assignment.OwnerId, owner.PartyId));
            }

            return result;
        }

        /// <summary>
        /// May <paramref name="pickerId"/> (in <paramref name="pickerPartyId"/>, null = alone) take the drop? Its owner always
        /// (like <see cref="PartyLootRules.CanPickUp"/>); anyone else must be near, and during the protection only a member of
        /// the owning party may take a drop left to the party.
        /// </summary>
        public static bool CanPickUp(SharedLootDrop drop, string pickerId, string pickerPartyId, bool pickerNear, double secondsSinceDrop)
        {
            if (string.IsNullOrEmpty(pickerId))
            {
                return false;
            }

            if (drop.OwnerPlayerId == pickerId)
            {
                return true;
            }

            if (!pickerNear)
            {
                return false;
            }

            if (drop.IsFree || secondsSinceDrop >= ProtectionSeconds)
            {
                return true;
            }

            if (drop.OwnerPlayerId != null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(pickerPartyId) && pickerPartyId == drop.OwnerPartyId;
        }

        /// <summary>Seconds of protection left (0 once free).</summary>
        public static double ProtectionLeft(SharedLootDrop drop, double secondsSinceDrop) =>
            drop.IsFree ? 0d : Math.Max(0d, ProtectionSeconds - secondsSinceDrop);
    }
}
