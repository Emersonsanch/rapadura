namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// When a world boss that withdrew may come back (<see cref="RegionBossDirector"/>): after <see cref="RegionBossRules.RespawnSeconds"/>, and for
    /// the bosses that return "the next night" (<see cref="RegionBossSpawn.ReturnsNextNight"/>) only on a later night of the game clock.
    /// Pure (no Unity): the director feeds it the time and the clock; the future server can hold the same state.
    /// It belongs to one game: <see cref="Reset"/> on a new game and when a save is read from disk, so nothing leaks between save slots.
    /// </summary>
    public sealed class RegionBossReturns
    {
        /// <summary>Earliest time (the director's clock, <c>Time.time</c>) at which it may come back; 0 = no timer.</summary>
        public float BackAt { get; private set; }

        /// <summary>Also waits for a later night than <see cref="WithdrawnNight"/>.</summary>
        public bool WaitingForNight { get; private set; }

        /// <summary><see cref="RegionBossRules.NightIndex"/> of the game clock when it withdrew.</summary>
        public int WithdrawnNight { get; private set; }

        public void Withdraw(float now, bool returnsNextNight, int night)
        {
            BackAt = now + RegionBossRules.RespawnSeconds;
            WaitingForNight = returnsNextNight;
            WithdrawnNight = night;
        }

        /// <summary>
        /// May it come back now? <paramref name="hasClock"/> false (no <c>TimeOfDayManager</c>) keeps a boss waiting for the night held back.
        /// Once the night it waits for comes, it stops waiting (it can then still wait for the hero to come near).
        /// </summary>
        public bool CanReturn(float now, bool hasClock, int day, float hour, bool isNight, float nightStartHour)
        {
            if (WaitingForNight)
            {
                if (!hasClock || !RegionBossRules.NightReturnReady(WithdrawnNight, day, hour, isNight, nightStartHour))
                {
                    return false;
                }

                WaitingForNight = false;
            }

            return BackAt <= now;
        }

        /// <summary>A brand-new game, or a save read from disk: back at the arena, no timer, no night to wait for.</summary>
        public void Reset()
        {
            BackAt = 0f;
            WaitingForNight = false;
            WithdrawnNight = 0;
        }

        /// <summary>Whether a <c>GameLoadedEvent</c> starts the timers over: a save read from disk does, a scene swap handing carried state back does not.</summary>
        public static bool LoadResets(bool fromCarriedState) => !fromCarriedState;
    }
}
