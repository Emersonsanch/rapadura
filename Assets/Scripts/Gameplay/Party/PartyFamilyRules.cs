using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Duo;

namespace Rapadura.Gameplay.Party
{
    /// <summary>
    /// Family bonuses of a party (TODO.md 12.3), counted over the members <b>present at a kill</b> (eligible for its XP,
    /// <see cref="PartyXpRules.IsEligible"/>), so they reward fighting together, not just sharing a roster:
    /// <list type="bullet">
    /// <item><b>Família Completa</b>: the 5 cousins (Maithe, Ícaro, Joaquim, Maria, Lavine), one each, present —
    /// +<see cref="FamilyCompleteXpBonus"/> kill XP for every present member.</item>
    /// <item><b>Laço de Irmãos</b>: a member whose sibling (Maithe↔Ícaro, Joaquim↔Maria, <see cref="DuoStrikeRules.AreSiblings"/>)
    /// is also present — +<see cref="SiblingBondXpBonus"/> kill XP for those two.</item>
    /// </list>
    /// Both stack (×1,10 × 1,05 ≈ +15,5% at most) and are small on purpose: a pick-up group of 5 strangers still gets the
    /// MU member bonus (<see cref="PartyXpRules.PoolMultiplier"/>), the family only adds a nod. A member with no
    /// "primo da classe" (<see cref="PartyMember.Cousin"/> null, decision 12.1 still open) counts for neither.
    /// </summary>
    public static class PartyFamilyRules
    {
        public const float FamilyCompleteXpBonus = 0.10f;
        public const float SiblingBondXpBonus = 0.05f;

        public const int CousinCount = 5;

        /// <summary>True when the five different cousins are among <paramref name="cousins"/>.</summary>
        public static bool IsFamilyComplete(IEnumerable<CharacterId?> cousins)
        {
            if (cousins == null)
            {
                return false;
            }

            var distinct = new HashSet<CharacterId>();
            foreach (CharacterId? cousin in cousins)
            {
                if (cousin.HasValue)
                {
                    distinct.Add(cousin.Value);
                }
            }

            return distinct.Count >= CousinCount;
        }

        /// <summary>True when <paramref name="cousin"/>'s sibling is among <paramref name="present"/> (the member itself may be in the list).</summary>
        public static bool HasSiblingBond(CharacterId? cousin, IEnumerable<CharacterId?> present)
        {
            if (!cousin.HasValue || present == null)
            {
                return false;
            }

            foreach (CharacterId? other in present)
            {
                if (other.HasValue && DuoStrikeRules.AreSiblings(cousin.Value, other.Value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>True when some sibling pair (Maithe+Ícaro or Joaquim+Maria) is among <paramref name="present"/>: the HUD tag.</summary>
        public static bool AnySiblingBond(IEnumerable<CharacterId?> present)
        {
            if (present == null)
            {
                return false;
            }

            var list = new List<CharacterId?>(present);
            foreach (CharacterId? cousin in list)
            {
                if (HasSiblingBond(cousin, list))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Kill-XP multiplier of one member: ×1,10 with the full family, ×1,05 with the sibling present.</summary>
        public static float XpMultiplier(bool familyComplete, bool siblingBond) =>
            (familyComplete ? 1f + FamilyCompleteXpBonus : 1f) * (siblingBond ? 1f + SiblingBondXpBonus : 1f);

        /// <summary>The roster's cousins, for the HUD tags (who could earn the bonuses if all came along).</summary>
        public static List<CharacterId?> CousinsOf(PartyState party)
        {
            var cousins = new List<CharacterId?>();
            if (party != null)
            {
                foreach (PartyMember member in party.Members)
                {
                    cousins.Add(member.Cousin);
                }
            }

            return cousins;
        }
    }
}
