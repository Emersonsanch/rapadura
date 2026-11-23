using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>
    /// "Pode ferir?" for player against player (TODO.md 12.3): the one pure question the server asks before every hit
    /// between two players, on top of <see cref="PartyCombatRules.CanHarmPlayer"/> (no friendly fire) and
    /// <see cref="SafeZoneRules"/> (nobody fights in a city). The guild front calls it with <c>atGuildWar</c> = the two
    /// guilds have a declared war running (its own rules decide that).
    /// <list type="number">
    /// <item><b>Arena</b>: a player in an arena match only hurts, and is only hurt by, the other team of the same match.</item>
    /// <item><b>Duel</b>: a sealed duel (countdown or fight) isolates the two: they only hurt each other, and only during
    /// the fight; nobody from outside hurts them and they hurt nobody else.</item>
    /// <item><b>Guild war</b>: outside cities and the protected regions (<see cref="PvpAreaRules.GuildWarAllowedAt"/>).</item>
    /// <item><b>Open area</b>: both qualify for open PvP where each stands (<see cref="PvpAreaRules.OpenPvpBetween"/>).</item>
    /// </list>
    /// The first that applies is the basis (<see cref="HarmBasis"/>); <see cref="PkRules.KillStains"/> reads it on a kill.
    /// </summary>
    public static class PvpCombatRules
    {
        /// <summary>On what ground <paramref name="attacker"/> could hurt <paramref name="target"/>, before the party / city check.</summary>
        public static PvpHarmBasis HarmBasis(PvpCombatant attacker, PvpCombatant target, bool atGuildWar = false)
        {
            if (string.IsNullOrEmpty(attacker.Id) || string.IsNullOrEmpty(target.Id) || attacker.Id == target.Id)
            {
                return PvpHarmBasis.None;
            }

            if (attacker.InArenaMatch || target.InArenaMatch)
            {
                bool opponents = attacker.ArenaMatchId == target.ArenaMatchId && attacker.ArenaTeam != target.ArenaTeam;
                return opponents ? PvpHarmBasis.Arena : PvpHarmBasis.None;
            }

            if (attacker.InSealedDuel || target.InSealedDuel)
            {
                DuelState duel = attacker.Duel;
                bool duelists = attacker.InSealedDuel && ReferenceEquals(duel, target.Duel) && duel.OpponentOf(attacker.Id) == target.Id;
                return duelists && duel.Phase == DuelPhase.Fighting ? PvpHarmBasis.Duel : PvpHarmBasis.None;
            }

            if (atGuildWar && PvpAreaRules.GuildWarAllowedAt(attacker.Region, attacker.Zone)
                && PvpAreaRules.GuildWarAllowedAt(target.Region, target.Zone))
            {
                return PvpHarmBasis.GuildWar;
            }

            return PvpAreaRules.OpenPvpBetween(attacker, target) ? PvpHarmBasis.OpenArea : PvpHarmBasis.None;
        }

        /// <summary>
        /// The full check: a basis exists and <see cref="PartyCombatRules.CanHarmPlayer"/> agrees (city, same party). An arena
        /// match is its own instanced ground, so its zone is the wilds by construction (the server builds the combatants so).
        /// </summary>
        public static bool CanHarmPlayer(PartyState attackerParty, PvpCombatant attacker, PvpCombatant target, bool atGuildWar = false)
        {
            PvpHarmBasis basis = HarmBasis(attacker, target, atGuildWar);
            if (basis == PvpHarmBasis.None)
            {
                return false;
            }

            return PartyCombatRules.CanHarmPlayer(attackerParty, attacker.Id, attacker.Zone, target.Id, target.Zone, pvpAllowed: true);
        }

        /// <summary>Same as <see cref="CanHarmPlayer"/>, returning the basis (None when refused).</summary>
        public static PvpHarmBasis Evaluate(PartyState attackerParty, PvpCombatant attacker, PvpCombatant target, bool atGuildWar = false)
        {
            return CanHarmPlayer(attackerParty, attacker, target, atGuildWar) ? HarmBasis(attacker, target, atGuildWar) : PvpHarmBasis.None;
        }

        /// <summary>
        /// Heals and buffs from <paramref name="helper"/> on <paramref name="target"/>: a sealed duelist only takes their own
        /// (nobody "interferes" with a heal either), and in the arena only from the own team of the same match.
        /// </summary>
        public static bool CanAssist(PvpCombatant helper, PvpCombatant target)
        {
            if (helper.Id == target.Id)
            {
                return true;
            }

            if (helper.InArenaMatch || target.InArenaMatch)
            {
                return helper.ArenaMatchId == target.ArenaMatchId && helper.ArenaTeam == target.ArenaTeam;
            }

            return !helper.InSealedDuel && !target.InSealedDuel;
        }

        /// <summary>A monster may still hurt a duelist (the world goes on); if it downs them, <see cref="DuelRules.Interrupt"/> ends the duel.</summary>
        public static bool MonsterCanHarm(PvpCombatant target) => !target.InArenaMatch && SafeZoneRules.CanTakeDamage(ZoneActor.Player, target.Zone);
    }
}
