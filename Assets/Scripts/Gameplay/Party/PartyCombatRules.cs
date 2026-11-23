using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Party
{
    /// <summary>
    /// "Sem fogo amigo" (TODO.md 12.3): one pure question the server asks before every hit — on top of
    /// <see cref="SafeZoneRules.CanAttack"/>, which already rules out cities, NPCs and monster-against-monster — a player
    /// never hurts a member of their own party, not even where PvP is allowed (a duel needs leaving the party first).
    /// Self-damage (skill costs) is untouched: it is still the wilds-only case of <see cref="SafeZoneRules"/>.
    /// Area skills ask this once per target, so a blast in the middle of the party hits only the monsters.
    /// </summary>
    public static class PartyCombatRules
    {
        /// <param name="sameParty">Attacker and target are both players in the same live party (<see cref="PartyRules.AreInSameParty"/>).</param>
        public static bool CanHarm(ZoneActor attacker, ZoneKind attackerZone, ZoneActor target, ZoneKind targetZone,
            bool sameParty, bool targetIsSelf = false, bool pvpAllowed = false)
        {
            if (!SafeZoneRules.CanAttack(attacker, attackerZone, target, targetZone, targetIsSelf, pvpAllowed))
            {
                return false;
            }

            bool playerOnPlayer = attacker == ZoneActor.Player && target == ZoneActor.Player && !targetIsSelf;
            return !(playerOnPlayer && sameParty);
        }

        /// <summary><see cref="CanHarm(ZoneActor,ZoneKind,ZoneActor,ZoneKind,bool,bool,bool)"/> reading the party: two player ids.</summary>
        public static bool CanHarmPlayer(PartyState party, string attackerId, ZoneKind attackerZone, string targetId, ZoneKind targetZone,
            bool pvpAllowed = false)
        {
            bool self = !string.IsNullOrEmpty(attackerId) && attackerId == targetId;
            return CanHarm(ZoneActor.Player, attackerZone, ZoneActor.Player, targetZone,
                PartyRules.AreInSameParty(party, attackerId, targetId), self, pvpAllowed);
        }
    }
}
