using System;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>
    /// The consented duel of FASE 12.3 (TODO.md), MU style: challenge / accept / decline / cancel / expire, a countdown,
    /// a sealed area, and an end by life at 1, forfeit, leaving the area, disconnect or time. Pure C# (only
    /// <see cref="Vector3"/>), deterministic and exception-free: the server keeps one <see cref="DuelState"/> per duel and
    /// calls these with its own clock.
    /// <list type="bullet">
    /// <item>Only outside cities (<see cref="SafeZoneRules"/>), at most <see cref="ChallengeRange"/> apart, never with a
    /// member of the same party (leave the party first), never while either one is busy (another duel / arena).</item>
    /// <item>Allowed in every region, even the starting ones: it is consented, so it is not "forced PvP".</item>
    /// <item>Nobody dies in a duel: a hit never takes life below 1 (<see cref="ApplyHit"/>), and reaching 1 loses.
    /// No item, no XP, no durability and no outlaw stain (<see cref="PkRules"/>) come from a duel.</item>
    /// <item>The area is a circle of <see cref="AreaRadius"/> around the midpoint at the accept; being outside it during the
    /// countdown or the fight loses at once.</item>
    /// <item>After <see cref="FightSeconds"/>: higher life fraction wins; equal (to 0.1%) = draw.</item>
    /// </list>
    /// Who may hit whom meanwhile (outsiders never): <see cref="PvpCombatRules"/>.
    /// </summary>
    public static class DuelRules
    {
        /// <summary>Metres between the two to challenge and to accept (talking distance, a little more than a trade's 5 m).</summary>
        public const float ChallengeRange = 10f;

        /// <summary>Seconds a challenge waits for the answer.</summary>
        public const double ChallengeSeconds = 30d;

        /// <summary>Countdown after the accept, before hits count.</summary>
        public const double CountdownSeconds = 5d;

        /// <summary>Length of the fight itself.</summary>
        public const double FightSeconds = 180d;

        /// <summary>Radius of the duel area around its centre, in metres.</summary>
        public const float AreaRadius = 25f;

        /// <summary>Life fractions closer than this at time-up are a draw.</summary>
        public const double DrawTolerance = 0.001d;

        /// <summary>The duel never applies the death penalty (XP / item / durability) to anyone.</summary>
        public const bool AppliesDeathPenalty = false;

        public static DuelResult Challenge(string duelId, DuelCandidate challenger, DuelCandidate target, bool sameParty,
            bool targetBlocksDuels, double now)
        {
            PvpRefusal check = CheckPair(challenger, target, sameParty);
            if (check != PvpRefusal.None)
            {
                return DuelResult.Refused(check);
            }

            if (targetBlocksDuels)
            {
                return DuelResult.Refused(PvpRefusal.TargetBlocksDuels);
            }

            return new DuelResult(PvpRefusal.None, new DuelState(duelId, challenger.Id, target.Id, now + ChallengeSeconds));
        }

        /// <summary>The target says yes: checks again (zones, range, party, alive), seals the area and starts the countdown.</summary>
        public static DuelResult Accept(DuelState duel, DuelCandidate target, DuelCandidate challenger, bool sameParty, double now)
        {
            if (duel == null)
            {
                return DuelResult.Refused(PvpRefusal.NoChallenge);
            }

            if (duel.Phase == DuelPhase.Ended)
            {
                return DuelResult.Refused(PvpRefusal.AlreadyEnded, duel);
            }

            if (target.Id != duel.TargetId || challenger.Id != duel.ChallengerId)
            {
                return DuelResult.Refused(PvpRefusal.NotParticipant, duel);
            }

            if (duel.Phase != DuelPhase.Pending)
            {
                return DuelResult.Refused(PvpRefusal.WrongPhase, duel);
            }

            if (now >= duel.ChallengeExpiresAt)
            {
                End(duel, DuelEndReason.Expired, null, now);
                return DuelResult.Refused(PvpRefusal.ChallengeExpired, duel);
            }

            // Busy is not re-checked: both are "busy" with this very challenge now.
            PvpRefusal check = CheckPair(Unbusy(target), Unbusy(challenger), sameParty);
            if (check != PvpRefusal.None)
            {
                return DuelResult.Refused(check, duel);
            }

            duel.Phase = DuelPhase.Countdown;
            duel.Center = (target.Position + challenger.Position) * 0.5f;
            duel.FightStartsAt = now + CountdownSeconds;
            duel.FightEndsAt = duel.FightStartsAt + FightSeconds;
            return new DuelResult(PvpRefusal.None, duel);
        }

        public static DuelResult Decline(DuelState duel, string targetId, double now)
        {
            PvpRefusal check = CheckPending(duel, targetId, duel?.TargetId);
            if (check != PvpRefusal.None)
            {
                return DuelResult.Refused(check, duel);
            }

            End(duel, DuelEndReason.Declined, null, now);
            return new DuelResult(PvpRefusal.None, duel);
        }

        /// <summary>The challenger withdraws before the answer.</summary>
        public static DuelResult Cancel(DuelState duel, string challengerId, double now)
        {
            PvpRefusal check = CheckPending(duel, challengerId, duel?.ChallengerId);
            if (check != PvpRefusal.None)
            {
                return DuelResult.Refused(check, duel);
            }

            End(duel, DuelEndReason.Cancelled, null, now);
            return new DuelResult(PvpRefusal.None, duel);
        }

        /// <summary>Either one gives up (countdown or fight): the other wins.</summary>
        public static DuelResult Forfeit(DuelState duel, string playerId, double now)
        {
            if (duel == null)
            {
                return DuelResult.Refused(PvpRefusal.NoChallenge);
            }

            if (duel.Phase == DuelPhase.Ended)
            {
                return DuelResult.Refused(PvpRefusal.AlreadyEnded, duel);
            }

            if (!duel.IsParticipant(playerId))
            {
                return DuelResult.Refused(PvpRefusal.NotParticipant, duel);
            }

            if (duel.Phase == DuelPhase.Pending)
            {
                return DuelResult.Refused(PvpRefusal.WrongPhase, duel);
            }

            End(duel, DuelEndReason.Forfeit, duel.OpponentOf(playerId), now);
            return new DuelResult(PvpRefusal.None, duel);
        }

        /// <summary>
        /// Server tick: expires a pending challenge, turns the countdown into the fight, and ends the duel on a disconnect
        /// (counts as forfeit), on leaving the area (both out at once = draw by area, no winner) or at time-up. Returns Ok
        /// always; <see cref="DuelResult.Ended"/> says whether this tick ended it.
        /// </summary>
        public static DuelResult Tick(DuelState duel, DuelFighterStatus challenger, DuelFighterStatus target, double now)
        {
            if (duel == null)
            {
                return DuelResult.Refused(PvpRefusal.NoChallenge);
            }

            switch (duel.Phase)
            {
                case DuelPhase.Ended:
                    return new DuelResult(PvpRefusal.None, duel);
                case DuelPhase.Pending:
                    if (now >= duel.ChallengeExpiresAt)
                    {
                        End(duel, DuelEndReason.Expired, null, now);
                    }

                    return new DuelResult(PvpRefusal.None, duel);
            }

            bool challengerGone = !challenger.Connected;
            bool targetGone = !target.Connected;
            if (challengerGone || targetGone)
            {
                End(duel, DuelEndReason.Disconnected, challengerGone && targetGone ? null : challengerGone ? duel.TargetId : duel.ChallengerId, now);
                return new DuelResult(PvpRefusal.None, duel);
            }

            bool challengerOut = !IsInsideArea(duel, challenger.Position);
            bool targetOut = !IsInsideArea(duel, target.Position);
            if (challengerOut || targetOut)
            {
                End(duel, DuelEndReason.LeftArea, challengerOut && targetOut ? null : challengerOut ? duel.TargetId : duel.ChallengerId, now);
                return new DuelResult(PvpRefusal.None, duel);
            }

            if (duel.Phase == DuelPhase.Countdown && now >= duel.FightStartsAt)
            {
                duel.Phase = DuelPhase.Fighting;
            }

            if (duel.Phase == DuelPhase.Fighting && now >= duel.FightEndsAt)
            {
                double diff = challenger.HpFraction - target.HpFraction;
                string winner = Math.Abs(diff) <= DrawTolerance ? null : diff > 0d ? duel.ChallengerId : duel.TargetId;
                End(duel, DuelEndReason.TimeUp, winner, now);
            }

            return new DuelResult(PvpRefusal.None, duel);
        }

        /// <summary>
        /// One duel hit from one duelist on the other. Only while fighting; the damage is clamped so life stays at 1 at least,
        /// and reaching 1 ends the duel (<see cref="DuelEndReason.Defeat"/>). Healing is not a hit (negative damage = 0).
        /// </summary>
        public static DuelHit ApplyHit(DuelState duel, string attackerId, string targetId, int targetHp, int damage, double now)
        {
            if (duel == null)
            {
                return DuelHit.Refused(PvpRefusal.NoChallenge, targetHp);
            }

            if (duel.Phase == DuelPhase.Ended)
            {
                return DuelHit.Refused(PvpRefusal.AlreadyEnded, targetHp);
            }

            if (!duel.IsParticipant(attackerId) || duel.OpponentOf(attackerId) != targetId)
            {
                return DuelHit.Refused(PvpRefusal.NotParticipant, targetHp);
            }

            if (duel.Phase != DuelPhase.Fighting || now < duel.FightStartsAt)
            {
                return DuelHit.Refused(PvpRefusal.WrongPhase, targetHp);
            }

            int hp = Math.Max(1, targetHp);
            int applied = Math.Max(0, Math.Min(damage, hp - 1));
            int newHp = hp - applied;
            bool ended = newHp <= 1 && damage > 0;
            if (ended)
            {
                End(duel, DuelEndReason.Defeat, attackerId, now);
            }

            return new DuelHit(PvpRefusal.None, applied, newHp, ended);
        }

        /// <summary>Something outside the duel downed a duelist: the duel stops with no winner (normal death rules apply to that death).</summary>
        public static DuelResult Interrupt(DuelState duel, double now)
        {
            if (duel == null)
            {
                return DuelResult.Refused(PvpRefusal.NoChallenge);
            }

            if (duel.Phase == DuelPhase.Ended)
            {
                return DuelResult.Refused(PvpRefusal.AlreadyEnded, duel);
            }

            End(duel, DuelEndReason.Interrupted, null, now);
            return new DuelResult(PvpRefusal.None, duel);
        }

        public static bool IsInsideArea(DuelState duel, Vector3 position)
        {
            if (duel == null)
            {
                return false;
            }

            Vector3 d = position - duel.Center;
            d.y = 0f;
            return d.sqrMagnitude <= AreaRadius * AreaRadius;
        }

        /// <summary>Horizontal distance check used by the challenge and the accept (inclusive).</summary>
        public static bool InChallengeRange(Vector3 a, Vector3 b)
        {
            Vector3 d = a - b;
            d.y = 0f;
            return d.sqrMagnitude <= ChallengeRange * ChallengeRange;
        }

        private static PvpRefusal CheckPair(DuelCandidate a, DuelCandidate b, bool sameParty)
        {
            if (string.IsNullOrEmpty(a.Id) || string.IsNullOrEmpty(b.Id))
            {
                return PvpRefusal.InvalidPlayer;
            }

            if (a.Id == b.Id)
            {
                return PvpRefusal.CannotTargetSelf;
            }

            if (!a.Alive || !b.Alive)
            {
                return PvpRefusal.Dead;
            }

            if (a.Busy)
            {
                return PvpRefusal.Busy;
            }

            if (b.Busy)
            {
                return PvpRefusal.TargetBusy;
            }

            if (SafeZoneRules.IsSafe(a.Zone) || SafeZoneRules.IsSafe(b.Zone))
            {
                return PvpRefusal.InSafeZone;
            }

            if (sameParty)
            {
                return PvpRefusal.SameParty;
            }

            return InChallengeRange(a.Position, b.Position) ? PvpRefusal.None : PvpRefusal.TooFar;
        }

        private static PvpRefusal CheckPending(DuelState duel, string actorId, string expectedId)
        {
            if (duel == null)
            {
                return PvpRefusal.NoChallenge;
            }

            if (duel.Phase == DuelPhase.Ended)
            {
                return PvpRefusal.AlreadyEnded;
            }

            if (string.IsNullOrEmpty(actorId) || actorId != expectedId)
            {
                return PvpRefusal.NotParticipant;
            }

            return duel.Phase == DuelPhase.Pending ? PvpRefusal.None : PvpRefusal.WrongPhase;
        }

        private static DuelCandidate Unbusy(DuelCandidate c) => new DuelCandidate(c.Id, c.Position, c.Zone, c.Alive, false);

        private static void End(DuelState duel, DuelEndReason reason, string winnerId, double now)
        {
            duel.Phase = DuelPhase.Ended;
            duel.EndReason = reason;
            duel.WinnerId = winnerId;
            duel.EndedAt = now;
        }
    }

    public readonly struct DuelHit
    {
        public readonly PvpRefusal Refusal;

        /// <summary>Damage actually applied (clamped so life stays at 1+).</summary>
        public readonly int Applied;

        public readonly int NewHp;

        /// <summary>This hit brought the target to 1 and ended the duel.</summary>
        public readonly bool EndedDuel;

        public DuelHit(PvpRefusal refusal, int applied, int newHp, bool endedDuel)
        {
            Refusal = refusal;
            Applied = applied;
            NewHp = newHp;
            EndedDuel = endedDuel;
        }

        public bool Ok => Refusal == PvpRefusal.None;

        public static DuelHit Refused(PvpRefusal reason, int hp) => new DuelHit(reason, 0, hp, false);
    }
}
