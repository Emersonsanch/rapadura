using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>
    /// The objective of the Cerco da Treva: the Lâmpada do Portão of the city (the server keeps one per room instance).
    /// Health is plain numbers; time is the server's seconds since the start of the fight, passed in.
    /// </summary>
    public sealed class TimedLampState
    {
        public readonly int MaxHealth;
        private readonly Dictionary<string, double> _lastRekindle = new Dictionary<string, double>();

        public TimedLampState(int maxHealth)
        {
            MaxHealth = Math.Max(1, maxHealth);
            Health = MaxHealth;
            LastDefenderAliveAt = 0d;
        }

        public int Health { get; internal set; }

        /// <summary>Last instant some defender was standing (for "portão abandonado").</summary>
        public double LastDefenderAliveAt { get; internal set; }

        public bool IsOut => Health <= 0;

        public float Fraction => Health / (float)MaxHealth;

        internal bool TryGetLastRekindle(string playerId, out double at) => _lastRekindle.TryGetValue(playerId, out at);

        internal void SetLastRekindle(string playerId, double at) => _lastRekindle[playerId] = at;
    }

    /// <summary>
    /// "Cerco da Treva" (defense): the Treva comes for the Lâmpada do Portão in 6 assaults through 3 gates; the last one
    /// brings the "Arconte do Cerco". Enemies that reach the lamp hit it by role (a fraction of its health, so every room
    /// lasts as long). Defenders rekindle it. Defeat: the lamp goes out, or no defender stands for 45 s in a row.
    /// Victory: the last assault cleared, or the 20 minutes end with the lamp lit.
    /// </summary>
    public static class TimedCercoRules
    {
        public const int AssaultCount = 6;
        public const int Gates = 3;
        public const int BaseBudget = 10;
        public const int BudgetPerAssault = 4;
        public const double AssaultSeconds = 160d;
        public const int ClearThreshold = 3;

        public const int BaseLampHealth = 4000;
        public const float LampHealthPerRoom = 0.6f;
        public const float LampHealthPerExtraPlayer = 0.15f;

        /// <summary>Rekindle: +4% of the max, once per 20 s per player, never above the max, never a lamp that went out.</summary>
        public const float RekindleFraction = 0.04f;
        public const double RekindleCooldownSeconds = 20d;

        public const double AbandonedSeconds = 45d;

        /// <summary>Lamp ≥ 75% at the end: "Lâmpada intacta" (score bonus to everyone in the room).</summary>
        public const float IntactFraction = 0.75f;

        public static int LampMaxHealth(int roomIndex, int playerCount)
        {
            double health = BaseLampHealth * (1d + LampHealthPerRoom * Math.Max(0, roomIndex - 1)) * (1d + LampHealthPerExtraPlayer * Math.Max(0, playerCount - 1));
            return (int)Math.Round(health);
        }

        /// <summary>Fraction of the lamp's max health one hit of <paramref name="role"/> takes.</summary>
        public static float LampDamageFraction(TimedEnemyRole role)
        {
            switch (role)
            {
                case TimedEnemyRole.Shade: return 0.01f;
                case TimedEnemyRole.Runner: return 0.0075f;
                case TimedEnemyRole.Caster: return 0.015f;
                case TimedEnemyRole.Brute: return 0.03f;
                case TimedEnemyRole.Elite: return 0.06f;
                case TimedEnemyRole.Archon: return 0.12f;
                default: return 0f;
            }
        }

        public static TimedWave Assault(TimedEventRoomBand band, int assaultIndex, int playerCount, int seed)
        {
            int budget = TimedWaveRules.Budget(BaseBudget, BudgetPerAssault, assaultIndex, playerCount);
            return TimedWaveRules.Compose(band, assaultIndex, budget, seed, Gates, assaultIndex == AssaultCount - 1);
        }

        public static bool ShouldStartNextAssault(int assaultsStarted, double secondsSinceAssault, int enemiesAlive) =>
            TimedWaveRules.ShouldStartNextWave(assaultsStarted, AssaultCount, secondsSinceAssault, enemiesAlive, AssaultSeconds, ClearThreshold);

        /// <summary>An enemy of <paramref name="role"/> hits the lamp. Returns the damage dealt (0 on a lamp already out).</summary>
        public static int HitLamp(TimedLampState lamp, TimedEnemyRole role)
        {
            if (lamp == null || lamp.IsOut)
            {
                return 0;
            }

            int damage = Math.Max(1, (int)Math.Round(lamp.MaxHealth * LampDamageFraction(role)));
            damage = Math.Min(damage, lamp.Health);
            lamp.Health -= damage;
            return damage;
        }

        /// <summary>A defender rekindles the lamp. Returns the health restored (0 = refused: cooldown, full or out).</summary>
        public static int Rekindle(TimedLampState lamp, string playerId, double now)
        {
            if (lamp == null || lamp.IsOut || string.IsNullOrEmpty(playerId) || lamp.Health >= lamp.MaxHealth)
            {
                return 0;
            }

            if (lamp.TryGetLastRekindle(playerId, out double last) && now - last < RekindleCooldownSeconds)
            {
                return 0;
            }

            int heal = Math.Min(lamp.MaxHealth - lamp.Health, Math.Max(1, (int)Math.Round(lamp.MaxHealth * RekindleFraction)));
            lamp.Health += heal;
            lamp.SetLastRekindle(playerId, now);
            return heal;
        }

        /// <summary>Call every tick with the defenders standing now: keeps the "last time someone stood" clock.</summary>
        public static void NoteDefenders(TimedLampState lamp, int defendersAlive, double now)
        {
            if (lamp != null && defendersAlive > 0)
            {
                lamp.LastDefenderAliveAt = now;
            }
        }

        public static TimedEventOutcome Outcome(TimedLampState lamp, int assaultsCleared, double now, bool timeUp)
        {
            if (lamp == null || lamp.IsOut)
            {
                return TimedEventOutcome.Defeat;
            }

            if (now - lamp.LastDefenderAliveAt >= AbandonedSeconds)
            {
                return TimedEventOutcome.Defeat;
            }

            if (assaultsCleared >= AssaultCount || timeUp)
            {
                return TimedEventOutcome.Victory;
            }

            return TimedEventOutcome.InProgress;
        }

        public static bool IsIntact(TimedLampState lamp) => lamp != null && !lamp.IsOut && lamp.Fraction >= IntactFraction;
    }
}
