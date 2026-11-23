using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>Life of a guild war. Never renumber.</summary>
    public enum GuildWarState
    {
        /// <summary>Declared, waiting for the other guild (<see cref="GuildWarRules.DeclarationExpirySeconds"/>).</summary>
        Declared = 0,

        /// <summary>Accepted; fighting starts at <see cref="GuildWar.StartsAt"/> (<see cref="GuildWarRules.WarmupSeconds"/>).</summary>
        Accepted = 1,

        Active = 2,

        /// <summary>Over, with a score (<see cref="GuildWar.EndReason"/>, <see cref="GuildWar.WinnerGuildId"/>).</summary>
        Ended = 3,

        Declined = 4,

        /// <summary>The declaring guild withdrew it before the answer.</summary>
        Cancelled = 5,

        /// <summary>Nobody answered in time.</summary>
        Expired = 6,
    }

    public enum GuildWarEndReason
    {
        None = 0,
        TimeUp = 1,
        ScoreReached = 2,
        Surrender = 3,
    }

    /// <summary>One war between two guilds: terms, state and score. Changed only by <see cref="GuildWarRules"/>.</summary>
    public sealed class GuildWar
    {
        public readonly string Id;
        public readonly string AttackerGuildId;
        public readonly string DefenderGuildId;
        public readonly double DeclaredAt;
        public readonly double DurationSeconds;

        /// <summary>Kills that end the war early; 0 = only the clock ends it.</summary>
        public readonly int ScoreTarget;

        public GuildWarState State;
        public double AcceptedAt = -1d;
        public double StartsAt = -1d;
        public double EndsAt = -1d;

        /// <summary>When it reached a final state (ended, declined, cancelled, expired); -1 while open.</summary>
        public double ClosedAt = -1d;

        public int AttackerScore;
        public int DefenderScore;
        public GuildWarEndReason EndReason;

        /// <summary>Null on a draw or when it never ran.</summary>
        public string WinnerGuildId;

        internal readonly Dictionary<string, double> LastScoredKill = new Dictionary<string, double>();

        public GuildWar(string id, string attackerGuildId, string defenderGuildId, double declaredAt, double durationSeconds, int scoreTarget)
        {
            Id = id;
            AttackerGuildId = attackerGuildId;
            DefenderGuildId = defenderGuildId;
            DeclaredAt = declaredAt;
            DurationSeconds = durationSeconds;
            ScoreTarget = scoreTarget;
            State = GuildWarState.Declared;
        }

        /// <summary>Declared, accepted or active: not yet in a final state.</summary>
        public bool IsOpen => State == GuildWarState.Declared || State == GuildWarState.Accepted || State == GuildWarState.Active;

        public bool Involves(string guildId) => !string.IsNullOrEmpty(guildId) && (guildId == AttackerGuildId || guildId == DefenderGuildId);

        public bool IsBetween(string a, string b) => Involves(a) && Involves(b) && a != b;

        public string OpponentOf(string guildId) =>
            guildId == AttackerGuildId ? DefenderGuildId : guildId == DefenderGuildId ? AttackerGuildId : null;

        public int ScoreOf(string guildId) => guildId == AttackerGuildId ? AttackerScore : guildId == DefenderGuildId ? DefenderScore : 0;
    }

    public readonly struct GuildWarResult
    {
        public readonly GuildRefusal Refusal;
        public readonly GuildWar War;

        public GuildWarResult(GuildRefusal refusal, GuildWar war)
        {
            Refusal = refusal;
            War = war;
        }

        public bool Ok => Refusal == GuildRefusal.None;
    }

    /// <summary>
    /// Declared guild war: consented PvP only between the two guilds (TODO.md 12.3). Declare → accept → (warm-up) →
    /// active → ended with a score. The leader or a Vice of each side decides (<see cref="GuildPermission.DeclareWar"/>).
    /// <list type="bullet">
    /// <item>Terms set by the declarer: <see cref="MinDurationHours"/>–<see cref="MaxDurationHours"/> h, and an optional
    /// kill target up to <see cref="MaxScoreTarget"/> that ends it early.</item>
    /// <item>Each guild needs <see cref="MinMembers"/> members, has at most <see cref="MaxOpenWars"/> open wars, and the same
    /// pair waits <see cref="RedeclareCooldownSeconds"/> after a war closes (no declaration spam at a small guild).</item>
    /// <item>A kill scores 1; the same killer on the same victim scores again only after <see cref="RepeatKillSeconds"/>
    /// (no farming a friend who stands still).</item>
    /// <item><see cref="CanHarm"/> is the only door: war opens PvP between the two guilds and nothing else — never inside a
    /// city (<see cref="SafeZoneRules"/>), never on a member of one's own party (<see cref="PartyCombatRules"/>), never
    /// on a guild-mate.</item>
    /// </list>
    /// Alliances and the seasonal castle siege are only noted (Docs/Network.md §12).
    /// </summary>
    public static class GuildWarRules
    {
        public const int MinMembers = 5;
        public const int MaxOpenWars = 3;
        public const double MinDurationHours = 1d;
        public const double MaxDurationHours = 72d;
        public const double DefaultDurationHours = 24d;
        public const int MaxScoreTarget = 500;
        public const double DeclarationExpirySeconds = 24d * 3600d;

        /// <summary>Between the acceptance and the first blow: time to tell everyone (and to stay in the city).</summary>
        public const double WarmupSeconds = 600d;

        public const double RedeclareCooldownSeconds = 24d * 3600d;
        public const double RepeatKillSeconds = 600d;

        /// <param name="wars">Every war the server knows that involves either guild (open or closed recently).</param>
        public static GuildWarResult Declare(string warId, GuildState attacker, string actorId, GuildState defender,
            IReadOnlyList<GuildWar> wars, double durationHours, int scoreTarget, double now)
        {
            GuildRefusal check = GuildRules.CheckActor(attacker, actorId, GuildPermission.DeclareWar, out _);
            if (check != GuildRefusal.None)
            {
                return Fail(check);
            }

            if (string.IsNullOrEmpty(warId))
            {
                return Fail(GuildRefusal.InvalidPlayer);
            }

            if (defender == null || defender.IsDisbanded)
            {
                return Fail(GuildRefusal.Disbanded);
            }

            if (defender.Id == attacker.Id)
            {
                return Fail(GuildRefusal.WarSameGuild);
            }

            if (double.IsNaN(durationHours) || durationHours < MinDurationHours || durationHours > MaxDurationHours ||
                scoreTarget < 0 || scoreTarget > MaxScoreTarget)
            {
                return Fail(GuildRefusal.WarInvalidTerms);
            }

            if (attacker.Count < MinMembers || defender.Count < MinMembers)
            {
                return Fail(GuildRefusal.WarTooFewMembers);
            }

            int attackerOpen = 0;
            int defenderOpen = 0;
            if (wars != null)
            {
                foreach (GuildWar war in wars)
                {
                    if (war == null)
                    {
                        continue;
                    }

                    Tick(war, now);
                    if (war.IsBetween(attacker.Id, defender.Id))
                    {
                        if (war.IsOpen)
                        {
                            return Fail(GuildRefusal.WarAlreadyExists);
                        }

                        if (war.ClosedAt >= 0d && now - war.ClosedAt < RedeclareCooldownSeconds)
                        {
                            return Fail(GuildRefusal.WarCooldown);
                        }
                    }

                    if (war.IsOpen && war.Involves(attacker.Id))
                    {
                        attackerOpen++;
                    }

                    if (war.IsOpen && war.Involves(defender.Id))
                    {
                        defenderOpen++;
                    }
                }
            }

            if (attackerOpen >= MaxOpenWars || defenderOpen >= MaxOpenWars)
            {
                return Fail(GuildRefusal.WarTooManyWars);
            }

            return new GuildWarResult(GuildRefusal.None, new GuildWar(warId, attacker.Id, defender.Id, now, durationHours * 3600d, scoreTarget));
        }

        /// <summary>The defending guild accepts; the war becomes active <see cref="WarmupSeconds"/> later.</summary>
        public static GuildWarResult Accept(GuildWar war, GuildState defender, string actorId, double now)
        {
            GuildRefusal check = CheckAnswer(war, defender, actorId, now);
            if (check != GuildRefusal.None)
            {
                return Fail(check, war);
            }

            war.State = GuildWarState.Accepted;
            war.AcceptedAt = now;
            war.StartsAt = now + WarmupSeconds;
            war.EndsAt = war.StartsAt + war.DurationSeconds;
            return new GuildWarResult(GuildRefusal.None, war);
        }

        public static GuildWarResult Decline(GuildWar war, GuildState defender, string actorId, double now)
        {
            GuildRefusal check = CheckAnswer(war, defender, actorId, now);
            if (check != GuildRefusal.None)
            {
                return Fail(check, war);
            }

            Close(war, GuildWarState.Declined, now);
            return new GuildWarResult(GuildRefusal.None, war);
        }

        /// <summary>The declaring guild withdraws an unanswered declaration.</summary>
        public static GuildWarResult Cancel(GuildWar war, GuildState attacker, string actorId, double now)
        {
            GuildRefusal check = CheckSide(war, attacker, actorId, now);
            if (check == GuildRefusal.None && attacker.Id != war.AttackerGuildId)
            {
                check = GuildRefusal.WarNotParticipant;
            }

            if (check == GuildRefusal.None && war.State != GuildWarState.Declared)
            {
                check = GuildRefusal.WarWrongState;
            }

            if (check != GuildRefusal.None)
            {
                return Fail(check, war);
            }

            Close(war, GuildWarState.Cancelled, now);
            return new GuildWarResult(GuildRefusal.None, war);
        }

        /// <summary>Either side gives up an accepted or active war: the other side wins, whatever the score.</summary>
        public static GuildWarResult Surrender(GuildWar war, GuildState guild, string actorId, double now)
        {
            GuildRefusal check = CheckSide(war, guild, actorId, now);
            if (check == GuildRefusal.None && war.State != GuildWarState.Accepted && war.State != GuildWarState.Active)
            {
                check = GuildRefusal.WarWrongState;
            }

            if (check != GuildRefusal.None)
            {
                return Fail(check, war);
            }

            war.EndReason = GuildWarEndReason.Surrender;
            war.WinnerGuildId = war.OpponentOf(guild.Id);
            Close(war, GuildWarState.Ended, now);
            return new GuildWarResult(GuildRefusal.None, war);
        }

        /// <summary>Moves the war along the clock: an unanswered declaration expires, a warm-up becomes active, time up ends it.</summary>
        public static void Tick(GuildWar war, double now)
        {
            if (war == null)
            {
                return;
            }

            if (war.State == GuildWarState.Declared && now >= war.DeclaredAt + DeclarationExpirySeconds)
            {
                Close(war, GuildWarState.Expired, war.DeclaredAt + DeclarationExpirySeconds);
                return;
            }

            if (war.State == GuildWarState.Accepted && now >= war.StartsAt)
            {
                war.State = GuildWarState.Active;
            }

            if (war.State == GuildWarState.Active && now >= war.EndsAt)
            {
                war.EndReason = GuildWarEndReason.TimeUp;
                war.WinnerGuildId = Leader(war);
                Close(war, GuildWarState.Ended, war.EndsAt);
            }
        }

        /// <summary>
        /// A member of one side killed a member of the other. Scores 1 for the killer's guild when the war is active and the
        /// pair did not score in the last <see cref="RepeatKillSeconds"/>; reaching <see cref="GuildWar.ScoreTarget"/> ends it.
        /// </summary>
        public static bool RecordKill(GuildWar war, string killerGuildId, string killerId, string victimGuildId, string victimId, double now)
        {
            if (war == null || string.IsNullOrEmpty(killerId) || string.IsNullOrEmpty(victimId) || killerId == victimId)
            {
                return false;
            }

            Tick(war, now);
            if (war.State != GuildWarState.Active || !war.IsBetween(killerGuildId, victimGuildId))
            {
                return false;
            }

            string pair = killerId + "\n" + victimId;
            if (war.LastScoredKill.TryGetValue(pair, out double last) && now - last < RepeatKillSeconds)
            {
                return false;
            }

            war.LastScoredKill[pair] = now;
            if (killerGuildId == war.AttackerGuildId)
            {
                war.AttackerScore++;
            }
            else
            {
                war.DefenderScore++;
            }

            if (war.ScoreTarget > 0 && war.ScoreOf(killerGuildId) >= war.ScoreTarget)
            {
                war.EndReason = GuildWarEndReason.ScoreReached;
                war.WinnerGuildId = killerGuildId;
                Close(war, GuildWarState.Ended, now);
            }

            return true;
        }

        /// <summary>Pure query (does not tick): fighting is on between the two sides at <paramref name="now"/>.</summary>
        public static bool IsFighting(GuildWar war, double now)
        {
            if (war == null || war.StartsAt < 0d)
            {
                return false;
            }

            bool running = war.State == GuildWarState.Active || war.State == GuildWarState.Accepted;
            return running && now >= war.StartsAt && now < war.EndsAt;
        }

        /// <summary>Whether guilds <paramref name="a"/> and <paramref name="b"/> are fighting each other right now.</summary>
        public static bool AreAtWar(IEnumerable<GuildWar> wars, string a, string b, double now)
        {
            if (wars == null || string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a == b)
            {
                return false;
            }

            foreach (GuildWar war in wars)
            {
                if (war != null && war.IsBetween(a, b) && IsFighting(war, now))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// "Can member A hurt member B?" for two players. A running war between their guilds is the consent; on top of it
        /// <see cref="PartyCombatRules.CanHarm"/> (and so <see cref="SafeZoneRules.CanAttack"/>) still applies: never in a city,
        /// never a member of one's own party. Guild-mates are never at war with each other. <paramref name="pvpAllowed"/>
        /// is any other consent the server already has (duel, PvP area), unchanged by this.
        /// </summary>
        /// <param name="attackerGuildId">Null / empty when the attacker has no guild.</param>
        public static bool CanHarm(IEnumerable<GuildWar> wars, string attackerGuildId, ZoneKind attackerZone, string targetGuildId,
            ZoneKind targetZone, bool sameParty, double now, bool pvpAllowed = false, MapRegion? region = null)
        {
            // No forced PvP in the starting regions (TODO Fase 12.3): a war never opens blows in Regions 1–4 (PvpAreaRules).
            bool atWar = AreAtWar(wars, attackerGuildId, targetGuildId, now)
                && (region == null || PvpAreaRules.GuildWarAllowedAt(region.Value, attackerZone) && PvpAreaRules.GuildWarAllowedAt(region.Value, targetZone));
            return PartyCombatRules.CanHarm(ZoneActor.Player, attackerZone, ZoneActor.Player, targetZone, sameParty, false, pvpAllowed || atWar);
        }

        /// <summary>The guild ahead on the score, or null on a draw.</summary>
        private static string Leader(GuildWar war) =>
            war.AttackerScore > war.DefenderScore ? war.AttackerGuildId :
            war.DefenderScore > war.AttackerScore ? war.DefenderGuildId : null;

        private static GuildRefusal CheckAnswer(GuildWar war, GuildState defender, string actorId, double now)
        {
            GuildRefusal check = CheckSide(war, defender, actorId, now);
            if (check != GuildRefusal.None)
            {
                return check;
            }

            if (defender.Id != war.DefenderGuildId)
            {
                return GuildRefusal.WarNotParticipant;
            }

            if (war.State == GuildWarState.Expired)
            {
                return GuildRefusal.InviteExpired;
            }

            return war.State == GuildWarState.Declared ? GuildRefusal.None : GuildRefusal.WarWrongState;
        }

        private static GuildRefusal CheckSide(GuildWar war, GuildState guild, string actorId, double now)
        {
            if (war == null)
            {
                return GuildRefusal.WarWrongState;
            }

            GuildRefusal check = GuildRules.CheckActor(guild, actorId, GuildPermission.DeclareWar, out _);
            if (check != GuildRefusal.None)
            {
                return check;
            }

            if (!war.Involves(guild.Id))
            {
                return GuildRefusal.WarNotParticipant;
            }

            Tick(war, now);
            return GuildRefusal.None;
        }

        private static void Close(GuildWar war, GuildWarState state, double at)
        {
            war.State = state;
            war.ClosedAt = at;
        }

        private static GuildWarResult Fail(GuildRefusal reason, GuildWar war = null) => new GuildWarResult(reason, war);
    }
}
