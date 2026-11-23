using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>
    /// Why a PvP action (duel, arena queue, bail) was refused. <see cref="None"/> = done. Shown through
    /// <see cref="PvpTexts.RefusalKey"/>; the server never throws, it answers one of these.
    /// </summary>
    public enum PvpRefusal
    {
        None = 0,
        InvalidPlayer = 1,
        CannotTargetSelf = 2,

        /// <summary>One of the two stands in a city (safe zone): no duel there.</summary>
        InSafeZone = 3,

        /// <summary>Farther than <see cref="DuelRules.ChallengeRange"/> apart.</summary>
        TooFar = 4,

        /// <summary>Same party: leave the party first (no friendly fire, <see cref="Party.PartyCombatRules"/>).</summary>
        SameParty = 5,

        /// <summary>The acting player is already in a duel, a challenge or an arena match / queue.</summary>
        Busy = 6,

        /// <summary>The other player is already in a duel, a challenge or an arena match / queue.</summary>
        TargetBusy = 7,

        /// <summary>The other player turned duel requests off.</summary>
        TargetBlocksDuels = 8,

        /// <summary>One of the two is down.</summary>
        Dead = 9,

        NoChallenge = 10,
        NotParticipant = 11,
        ChallengeExpired = 12,

        /// <summary>The action does not fit the duel's phase (e.g. accepting a duel already fighting).</summary>
        WrongPhase = 13,

        AlreadyEnded = 14,
        AlreadyQueued = 15,
        NotQueued = 16,

        /// <summary>A 2v2 / 3v3 entry needs exactly 2 / 3 players.</summary>
        WrongTeamSize = 17,

        /// <summary>A team entry must be one live party.</summary>
        TeamNotOneParty = 18,

        /// <summary>A "Assassino" (5+ stains) cannot queue for the ranked arena until the stains fade.</summary>
        OutlawCannotQueue = 19,

        /// <summary>Below <see cref="ArenaRules.MinLevel"/>.</summary>
        LevelTooLow = 20,

        /// <summary>The bail (fiança) is paid at a city.</summary>
        NotInCity = 21,

        NotEnoughGold = 22,

        /// <summary>No stain to clear.</summary>
        NoStains = 23,
    }

    public enum DuelPhase
    {
        /// <summary>Challenge sent, waiting for the answer (<see cref="DuelRules.ChallengeSeconds"/>).</summary>
        Pending = 0,

        /// <summary>Accepted: the countdown runs (<see cref="DuelRules.CountdownSeconds"/>), nobody can hit yet, leaving the area already loses.</summary>
        Countdown = 1,

        Fighting = 2,
        Ended = 3,
    }

    public enum DuelEndReason
    {
        None = 0,
        Declined = 1,
        Expired = 2,

        /// <summary>The challenger withdrew the challenge before the answer.</summary>
        Cancelled = 3,

        /// <summary>The loser's life reached 1 (nobody dies in a duel).</summary>
        Defeat = 4,

        Forfeit = 5,

        /// <summary>The loser stepped out of the duel area (<see cref="DuelRules.AreaRadius"/>).</summary>
        LeftArea = 6,

        /// <summary>Time ran out: higher life fraction wins; equal = draw.</summary>
        TimeUp = 7,

        /// <summary>Disconnected: counts as a forfeit.</summary>
        Disconnected = 8,

        /// <summary>Something outside the duel (a monster, an event) downed one of them: no winner, normal death rules.</summary>
        Interrupted = 9,
    }

    /// <summary>One consented duel between two players, kept by the server. All times are server seconds.</summary>
    public sealed class DuelState
    {
        public readonly string Id;
        public readonly string ChallengerId;
        public readonly string TargetId;

        public DuelPhase Phase { get; internal set; }
        public DuelEndReason EndReason { get; internal set; }

        /// <summary>Null on a draw or when there is no winner (declined, expired, interrupted).</summary>
        public string WinnerId { get; internal set; }

        public double ChallengeExpiresAt { get; internal set; }
        public double FightStartsAt { get; internal set; }
        public double FightEndsAt { get; internal set; }
        public double EndedAt { get; internal set; }

        /// <summary>Centre of the duel area: the midpoint of the two at the accept.</summary>
        public Vector3 Center { get; internal set; }

        public DuelState(string id, string challengerId, string targetId, double challengeExpiresAt)
        {
            Id = id;
            ChallengerId = challengerId;
            TargetId = targetId;
            Phase = DuelPhase.Pending;
            ChallengeExpiresAt = challengeExpiresAt;
        }

        public bool IsParticipant(string playerId) =>
            !string.IsNullOrEmpty(playerId) && (playerId == ChallengerId || playerId == TargetId);

        public string OpponentOf(string playerId) =>
            playerId == ChallengerId ? TargetId : playerId == TargetId ? ChallengerId : null;

        /// <summary>Countdown or fighting: the two are sealed off from everybody else (<see cref="PvpCombatRules"/>).</summary>
        public bool IsSealed => Phase == DuelPhase.Countdown || Phase == DuelPhase.Fighting;

        public bool IsDraw => Phase == DuelPhase.Ended && WinnerId == null && EndReason == DuelEndReason.TimeUp;
    }

    public readonly struct DuelResult
    {
        public readonly PvpRefusal Refusal;
        public readonly DuelState Duel;

        public DuelResult(PvpRefusal refusal, DuelState duel)
        {
            Refusal = refusal;
            Duel = duel;
        }

        public bool Ok => Refusal == PvpRefusal.None;

        /// <summary>The action (or the tick) ended the duel.</summary>
        public bool Ended => Duel != null && Duel.Phase == DuelPhase.Ended;

        public static DuelResult Refused(PvpRefusal reason, DuelState duel = null) => new DuelResult(reason, duel);
    }

    /// <summary>What the server knows about one duelist at a tick.</summary>
    public readonly struct DuelFighterStatus
    {
        public readonly Vector3 Position;
        public readonly int Hp;
        public readonly int MaxHp;
        public readonly bool Connected;

        public DuelFighterStatus(Vector3 position, int hp, int maxHp, bool connected = true)
        {
            Position = position;
            Hp = hp;
            MaxHp = maxHp;
            Connected = connected;
        }

        public double HpFraction => MaxHp <= 0 ? 0d : (double)Hp / MaxHp;
    }

    /// <summary>A player as the duel / arena entry checks see them.</summary>
    public readonly struct DuelCandidate
    {
        public readonly string Id;
        public readonly Vector3 Position;
        public readonly ZoneKind Zone;
        public readonly bool Alive;

        /// <summary>Already in a duel, a challenge, an arena match or the arena queue (the server answers).</summary>
        public readonly bool Busy;

        public DuelCandidate(string id, Vector3 position, ZoneKind zone, bool alive = true, bool busy = false)
        {
            Id = id;
            Position = position;
            Zone = zone;
            Alive = alive;
            Busy = busy;
        }
    }

    /// <summary>
    /// A player as <see cref="PvpCombatRules"/> sees them before a hit: where they are, their level, and the duel / arena
    /// match they belong to (null when none). The guild front fills <c>atGuildWar</c> itself (see <see cref="PvpCombatRules.HarmBasis"/>).
    /// </summary>
    public readonly struct PvpCombatant
    {
        public readonly string Id;
        public readonly ZoneKind Zone;
        public readonly MapRegion Region;
        public readonly int Level;
        public readonly DuelState Duel;
        public readonly string ArenaMatchId;
        public readonly int ArenaTeam;

        /// <summary>
        /// Level the open-PvP gate reads (<see cref="PvpAreaRules.OpenPvpMinLevel"/>): the "nível efetivo" of a reset hero
        /// (<c>RebirthRules.EffectiveLevel</c>, Network.md §15), never below <see cref="Level"/>. Equal to it when not given.
        /// </summary>
        public readonly int EffectiveLevel;

        public PvpCombatant(string id, ZoneKind zone, MapRegion region, int level, DuelState duel = null,
            string arenaMatchId = null, int arenaTeam = 0, int effectiveLevel = 0)
        {
            Id = id;
            Zone = zone;
            Region = region;
            Level = level;
            Duel = duel;
            ArenaMatchId = arenaMatchId;
            ArenaTeam = arenaTeam;
            EffectiveLevel = level > effectiveLevel ? level : effectiveLevel;
        }

        public bool InSealedDuel => Duel != null && Duel.IsSealed && Duel.IsParticipant(Id);

        public bool InArenaMatch => !string.IsNullOrEmpty(ArenaMatchId);
    }

    /// <summary>On what ground a player may hurt another player. <see cref="None"/> = not at all.</summary>
    public enum PvpHarmBasis
    {
        None = 0,
        Duel = 1,
        Arena = 2,
        GuildWar = 3,

        /// <summary>Open PvP in a marked area (<see cref="PvpAreaRules"/>), both at <see cref="PvpAreaRules.OpenPvpMinLevel"/>+.</summary>
        OpenArea = 4,
    }
}
