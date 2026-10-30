using System;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>The three "fôlego diário" tiers (Docs/Referencias_MMO.md §6, Docs/Balance/README.md "Fôlego diário").</summary>
    public enum StaminaTier
    {
        /// <summary>"Fôlego cheio": the first 3 hours after a good rest, +30% XP and loot.</summary>
        Full,

        /// <summary>From 3 to 5 hours: normal XP and loot.</summary>
        Normal,

        /// <summary>After 5 hours: −50% XP and loot and the gentle "Hora de descansar" reminder. Never blocks play.</summary>
        Tired,
    }

    /// <summary>How one tick of real time is counted.</summary>
    public enum StaminaTickKind
    {
        /// <summary>Playing (map or arena, not paused, input in the last <see cref="DailyStaminaRules.IdleThresholdSeconds"/>): drains the fôlego.</summary>
        Active,

        /// <summary>Away from the game (title screen, paused, idle, app in the background): refills it 1:1.</summary>
        Resting,
    }

    /// <summary>
    /// Plain-C# fôlego state: what is left of the two buckets plus the clock bookkeeping for offline rest.
    /// Serialized as JSON by <see cref="DailyStaminaService"/> (per device, PlayerPrefs: the fôlego belongs to the
    /// child playing, not to a save slot, so a new save does not refill it).
    /// </summary>
    [Serializable]
    public class DailyStaminaState
    {
        /// <summary>Seconds of "fôlego cheio" (+30%) left, 0..<see cref="DailyStaminaRules.FullSeconds"/>.</summary>
        public double FullLeftSeconds = DailyStaminaRules.FullSeconds;

        /// <summary>Seconds of normal fôlego left after the full bucket, 0..<see cref="DailyStaminaRules.NormalSeconds"/>.</summary>
        public double NormalLeftSeconds = DailyStaminaRules.NormalSeconds;

        /// <summary>
        /// High-water mark of the (skew-corrected) UTC clock, in ticks; 0 = never seen. Offline rest is measured from it
        /// and it never moves backwards, so turning the device clock back earns nothing.
        /// </summary>
        public long LastSeenUtcTicks;

        /// <summary>
        /// Seconds the wall clock jumped ahead of the monotonic real-time clock while the game was open (someone changed
        /// the date). Subtracted from the wall clock before measuring offline rest, so the jump is not "rest".
        /// </summary>
        public double ClockSkewSeconds;

        /// <summary>Lifetime active play seconds (informative; the parental reminder uses the session count).</summary>
        public double LifetimeActiveSeconds;

        public DailyStaminaState Clone()
        {
            return (DailyStaminaState)MemberwiseClone();
        }
    }

    /// <summary>Outcome of <see cref="DailyStaminaRules.ResolveOffline"/>.</summary>
    public readonly struct OfflineRestResult
    {
        public readonly double RestSeconds;
        public readonly long NewLastSeenUtcTicks;
        public readonly double NewClockSkewSeconds;

        public OfflineRestResult(double restSeconds, long newLastSeenUtcTicks, double newClockSkewSeconds)
        {
            RestSeconds = restSeconds;
            NewLastSeenUtcTicks = newLastSeenUtcTicks;
            NewClockSkewSeconds = newClockSkewSeconds;
        }
    }

    /// <summary>
    /// Pure rules of the "fôlego diário" (user decision, 2026-10-29), tested in <c>DailyStaminaTests</c>:
    /// <list type="bullet">
    /// <item><b>Tiers</b>: two buckets drained in order by active play: <see cref="FullSeconds"/> (3 h) at
    /// ×<see cref="FullMultiplier"/>, then <see cref="NormalSeconds"/> (2 h, up to 5 h) at ×1, then
    /// ×<see cref="TiredMultiplier"/> with no floor on time. Play is never blocked.</item>
    /// <item><b>Rest</b>: every second away refills one second, "fôlego cheio" first (the decision: 1 hour away gives
    /// back 1 hour of full fôlego), then the normal bucket. 5 hours away fill both. There is no midnight reset: a
    /// night's sleep refills everything, and playing across midnight does not hand out a fresh 3 hours.</item>
    /// <item><b>Active play only</b>: <see cref="ClassifyTick"/> counts a tick as play only in a gameplay scene (map or
    /// arena), not paused, app focused, with input in the last <see cref="IdleThresholdSeconds"/>; anything else rests.</item>
    /// <item><b>Clock tampering</b> (offline): the monotonic real-time clock measures in-session time; offline rest is
    /// the UTC wall clock minus a high-water mark (<see cref="ResolveOffline"/>), never negative and capped at what
    /// refills both buckets; an in-session wall/real mismatch over <see cref="ClockJumpToleranceSeconds"/> is a date
    /// jump (<see cref="DetectClockJump"/>) whose size is remembered as skew and not counted as rest later.
    /// <b>Online, the server owns the fôlego</b> (Fase 12): the client only shows the server's value.</item>
    /// </list>
    /// </summary>
    public static class DailyStaminaRules
    {
        public const double FullSeconds = 3 * 3600;
        public const double NormalSeconds = 2 * 3600;

        /// <summary>Full + normal: when the "tired" tier starts (5 h).</summary>
        public const double CapacitySeconds = FullSeconds + NormalSeconds;

        public const float FullMultiplier = 1.3f;
        public const float NormalMultiplier = 1f;
        public const float TiredMultiplier = 0.5f;

        /// <summary>
        /// Campaign pace of kill XP: the enemies' XP numbers (r8, Docs/Balance/README.md §10) were tuned for the pace a
        /// child actually plays at — almost always "fôlego cheio" (under 3 h a day). So kill XP is
        /// <c>base × tier × KillXpPace</c>: fôlego cheio gives exactly the tuned XP, normal 1/1.3 of it (≈ −23%), tired
        /// 0.5/1.3 (≈ −62%). The tiers keep the decided ratios (+30% over normal, −50% under it) and the power budget
        /// (§14.3) stays exactly where r8 put it; without this pace a full-fôlego hero arrives 1–2 levels above the r8
        /// route and the full optional stack breaks the budget (§23). Loot is not paced (coins and materials only).
        /// </summary>
        public const float KillXpPace = 1f / FullMultiplier;

        /// <summary>No input for this long (3 min) and the game counts as idle (resting) until the next input.</summary>
        public const double IdleThresholdSeconds = 180;

        /// <summary>A wall-clock step this much bigger/smaller than the real elapsed time is a date change, not drift.</summary>
        public const double ClockJumpToleranceSeconds = 120;

        /// <summary>While tired, the "Hora de descansar" reminder repeats every 30 minutes of active play.</summary>
        public const double TiredReminderRepeatSeconds = 30 * 60;

        // ------------------------------------------------------------------
        // Tiers and multipliers
        // ------------------------------------------------------------------

        public static StaminaTier TierOf(DailyStaminaState state)
        {
            if (state == null)
            {
                return StaminaTier.Normal;
            }

            if (state.FullLeftSeconds > 0)
            {
                return StaminaTier.Full;
            }

            return state.NormalLeftSeconds > 0 ? StaminaTier.Normal : StaminaTier.Tired;
        }

        public static float MultiplierOf(StaminaTier tier)
        {
            switch (tier)
            {
                case StaminaTier.Full:
                    return FullMultiplier;
                case StaminaTier.Tired:
                    return TiredMultiplier;
                default:
                    return NormalMultiplier;
            }
        }

        /// <summary>Kill-XP multiplier of a tier: <see cref="MultiplierOf"/> × <see cref="KillXpPace"/> (1 / ≈0.77 / ≈0.38).</summary>
        public static float ExperienceMultiplierOf(StaminaTier tier) => MultiplierOf(tier) * KillXpPace;

        /// <summary>Seconds of play until the next tier (0 when already tired).</summary>
        public static double SecondsToNextTier(DailyStaminaState state)
        {
            if (state == null)
            {
                return 0;
            }

            if (state.FullLeftSeconds > 0)
            {
                return state.FullLeftSeconds;
            }

            return Math.Max(0, state.NormalLeftSeconds);
        }

        /// <summary>0..1 fill of the HUD meter: what is left of the 5 h.</summary>
        public static float Fill(DailyStaminaState state)
        {
            if (state == null)
            {
                return 1f;
            }

            return (float)Math.Max(0, Math.Min(1, (state.FullLeftSeconds + state.NormalLeftSeconds) / CapacitySeconds));
        }

        /// <summary>Kill XP with the tier applied; never takes the last point of a positive reward away.</summary>
        public static int ScaleExperience(int experience, float multiplier)
        {
            if (experience <= 0)
            {
                return experience;
            }

            return Math.Max(1, (int)Math.Round(experience * (double)multiplier, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// A stackable loot quantity with the tier applied: floor(q × m), plus one more with the probability of the
        /// fraction (<paramref name="roll01"/> in [0, 1)), so ×1.3 on 2 gold gives 2 or 3 averaging 2.6, and ×0.5 on
        /// 1 herb gives 0 or 1. ×1 returns <paramref name="quantity"/> unchanged without using the roll.
        /// </summary>
        public static int ScaleLootQuantity(int quantity, float multiplier, double roll01)
        {
            if (quantity <= 0 || Math.Abs(multiplier - 1f) < 1e-6f)
            {
                return quantity;
            }

            double scaled = quantity * (double)Math.Max(0f, multiplier);
            int whole = (int)Math.Floor(scaled + 1e-9);
            double fraction = scaled - whole;
            return whole + (fraction > 1e-9 && roll01 < fraction ? 1 : 0);
        }

        // ------------------------------------------------------------------
        // Play and rest
        // ------------------------------------------------------------------

        /// <summary>Whether a tick counts as play (see the class summary) or as rest.</summary>
        public static StaminaTickKind ClassifyTick(bool inGameplayScene, bool paused, bool appFocused, double secondsSinceInput)
        {
            bool active = inGameplayScene && !paused && appFocused && !IsIdle(secondsSinceInput);
            return active ? StaminaTickKind.Active : StaminaTickKind.Resting;
        }

        public static bool IsIdle(double secondsSinceInput) => secondsSinceInput >= IdleThresholdSeconds;

        /// <summary>Drains <paramref name="seconds"/> of active play: the full bucket first, then the normal one.</summary>
        public static void Play(DailyStaminaState state, double seconds)
        {
            if (state == null || seconds <= 0)
            {
                return;
            }

            state.LifetimeActiveSeconds += seconds;
            double fromFull = Math.Min(state.FullLeftSeconds, seconds);
            state.FullLeftSeconds -= fromFull;
            double rest = seconds - fromFull;
            state.NormalLeftSeconds = Math.Max(0, state.NormalLeftSeconds - rest);
            Clamp(state);
        }

        /// <summary>Refills <paramref name="seconds"/> of rest 1:1: "fôlego cheio" first, then the normal bucket.</summary>
        public static void Rest(DailyStaminaState state, double seconds)
        {
            if (state == null || seconds <= 0)
            {
                return;
            }

            double toFull = Math.Min(FullSeconds - state.FullLeftSeconds, seconds);
            state.FullLeftSeconds += Math.Max(0, toFull);
            double left = seconds - Math.Max(0, toFull);
            state.NormalLeftSeconds = Math.Min(NormalSeconds, state.NormalLeftSeconds + Math.Max(0, left));
            Clamp(state);
        }

        private static void Clamp(DailyStaminaState state)
        {
            state.FullLeftSeconds = Math.Max(0, Math.Min(FullSeconds, state.FullLeftSeconds));
            state.NormalLeftSeconds = Math.Max(0, Math.Min(NormalSeconds, state.NormalLeftSeconds));
        }

        // ------------------------------------------------------------------
        // Clock guard
        // ------------------------------------------------------------------

        /// <summary>
        /// In-session date-jump guard: the wall clock moved <paramref name="wallElapsedSeconds"/> while the monotonic
        /// clock moved <paramref name="realElapsedSeconds"/>. Returns the jump (wall − real) when it exceeds
        /// <see cref="ClockJumpToleranceSeconds"/> either way, else 0 (normal drift, NTP nudges).
        /// </summary>
        public static double DetectClockJump(double realElapsedSeconds, double wallElapsedSeconds)
        {
            double jump = wallElapsedSeconds - realElapsedSeconds;
            return Math.Abs(jump) > ClockJumpToleranceSeconds ? jump : 0;
        }

        /// <summary>
        /// Offline rest when the game starts (or comes back from the background) at <paramref name="wallNowUtcTicks"/>:
        /// <list type="bullet">
        /// <item>First run (<paramref name="lastSeenUtcTicks"/> 0): no rest, the state starts full anyway.</item>
        /// <item>Clock corrected by the remembered skew; ahead of the high-water mark: the difference, capped at
        /// <see cref="CapacitySeconds"/> (more cannot refill anything).</item>
        /// <item>Behind the high-water mark (clock turned back): no rest; a remembered forward skew shrinks by the
        /// step back (the clock is returning to the truth) and the high-water mark stays.</item>
        /// </list>
        /// </summary>
        public static OfflineRestResult ResolveOffline(long lastSeenUtcTicks, double clockSkewSeconds, long wallNowUtcTicks)
        {
            double skew = Math.Max(0, clockSkewSeconds);
            long adjusted = wallNowUtcTicks - SecondsToTicks(skew);
            if (lastSeenUtcTicks <= 0)
            {
                return new OfflineRestResult(0, adjusted, skew);
            }

            if (adjusted >= lastSeenUtcTicks)
            {
                double rest = Math.Min(CapacitySeconds, TicksToSeconds(adjusted - lastSeenUtcTicks));
                return new OfflineRestResult(rest, adjusted, skew);
            }

            double back = TicksToSeconds(lastSeenUtcTicks - adjusted);
            double newSkew = Math.Max(0, skew - back);
            long readjusted = wallNowUtcTicks - SecondsToTicks(newSkew);
            return new OfflineRestResult(0, Math.Max(lastSeenUtcTicks, readjusted), newSkew);
        }

        /// <summary>Adds an in-session forward jump to the skew (backward jumps are handled by the high-water mark).</summary>
        public static double AccumulateSkew(double clockSkewSeconds, double jumpSeconds)
        {
            return Math.Max(0, clockSkewSeconds) + Math.Max(0, jumpSeconds);
        }

        public static long SecondsToTicks(double seconds) => (long)(seconds * TimeSpan.TicksPerSecond);

        public static double TicksToSeconds(long ticks) => ticks / (double)TimeSpan.TicksPerSecond;

        // ------------------------------------------------------------------
        // Reminders
        // ------------------------------------------------------------------

        /// <summary>
        /// The "Hora de descansar, a Vó Edna chamou para o chá" reminder: on entering the tired tier, then every
        /// <see cref="TiredReminderRepeatSeconds"/> of active play while still tired.
        /// </summary>
        public static bool ShouldRemindTired(StaminaTier previous, StaminaTier current, double activeSecondsSinceLastReminder)
        {
            if (current != StaminaTier.Tired)
            {
                return false;
            }

            return previous != StaminaTier.Tired || activeSecondsSinceLastReminder >= TiredReminderRepeatSeconds;
        }

        /// <summary>Choices of the parental "Lembrete de tempo de jogo" (minutes; 0 = off, the default).</summary>
        public static readonly int[] PlayTimeReminderChoices = { 0, 30, 45, 60, 90, 120 };

        /// <summary>Closest valid choice of <see cref="PlayTimeReminderChoices"/> (unknown values snap to the nearest; negatives = off).</summary>
        public static int SanitizeReminderMinutes(int minutes)
        {
            if (minutes <= 0)
            {
                return 0;
            }

            int best = PlayTimeReminderChoices[1];
            foreach (int choice in PlayTimeReminderChoices)
            {
                if (choice > 0 && Math.Abs(choice - minutes) < Math.Abs(best - minutes))
                {
                    best = choice;
                }
            }

            return best;
        }

        /// <summary>
        /// Parental play-time reminder: due when <paramref name="sessionActiveSeconds"/> crossed the next multiple of
        /// <paramref name="intervalMinutes"/> after the last one shown (<paramref name="lastRemindedAtSeconds"/>). Off at 0.
        /// </summary>
        public static bool IsPlayTimeReminderDue(double sessionActiveSeconds, double lastRemindedAtSeconds, int intervalMinutes)
        {
            if (intervalMinutes <= 0)
            {
                return false;
            }

            return sessionActiveSeconds - Math.Max(0, lastRemindedAtSeconds) >= intervalMinutes * 60.0;
        }
    }
}
