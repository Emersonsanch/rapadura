using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What happened in one tick of a defence (<see cref="RioBarqueDefense.Tick"/>); several at once are possible.</summary>
    [Flags]
    public enum RioDefenseEvent
    {
        None = 0,
        PullWarning = 1,
        Pull = 2,
        Oars = 4,
        HeroAway = 8,
        Freed = 16,
        Sunk = 32,
        Abandoned = 64,
    }

    /// <summary>Why the barque will not start (<see cref="RioBarqueDefenseRules.CheckStart"/>).</summary>
    public enum RioDefenseStart
    {
        Ok = 0,
        AlreadyFreed = 1,
        RowersResting = 2,
        ZoneBlocked = 3,
    }

    /// <summary>
    /// Defending the stranded barque on Apep's sandbank (Docs/Mapas100.md 76, "defender a barca: a barca tem vida; o grupo protege enquanto
    /// os remadores a desencalham"), pure and server-runnable. The hero calls the rowers at the hull; they push for
    /// <see cref="RunSeconds"/> of guarded time while the beasts near the hull gnaw at it and Apep, every <see cref="PullEvery"/> s, drinks the
    /// river under it. A hero on the bow when Apep pulls takes the pull instead of the hull (a hazard, never past a floor of life). Freed,
    /// the barque floats on the river Apep spits back and, for the rest of the in-game day, carries the hero along the dry river to the
    /// far ends of the bank and back.
    /// <code>
    /// call the rowers within 6 m of the hull · guard within 25 m · away 15 s in a row: the rowers drop the oars (abandoned)
    /// run: 90 s of guarded time · hull 100 · beasts within 12 m gnaw 1.5 a second each (a boss 4), at most 6 beasts counted
    /// Apep's pull: warned 3 s before, at 20, 40, 60, 80 s of the run · 12 off the hull, or 4 % of the hero's max life on the bow (5 m), never under 30 %
    /// oars: every 2 s the rowers strike the beasts within 6 m of the hull for 4 % of max life (a boss 0.5 %)
    /// sunk or abandoned: the rowers rest 30 s · freed: 20 % of max life back, the beasts within 20 m lose 12 % (a boss 2 %), once per in-game day
    /// freed barque: 55 m/s, 550–630 m to each end of the bank (10–12 s), arc 1.5 m and never under the sand of Apep's coils
    /// </code>
    /// The hero runs at 5.5 m/s: the two ends of the bank are 100–115 s away on foot, over the ridges of Apep's body.
    /// </summary>
    public static class RioBarqueDefenseRules
    {
        public const float HullReach = 6f;
        public const float GuardRadius = 25f;
        public const float AwaySeconds = 15f;
        public const float RunSeconds = 90f;
        public const float HullMax = 100f;

        public const float GnawRadius = 12f;
        public const float GnawPerSecond = 1.5f;
        public const float BossGnawPerSecond = 4f;
        public const int MaxGnawers = 6;

        public const float PullEvery = 20f;
        public const float PullWarning = 3f;
        public const float PullHullDamage = 12f;
        public const float BowReach = 5f;
        public const float PullHeroFraction = 0.04f;
        public const float PullFloorFraction = 0.3f;

        public const float OarEvery = 2f;
        public const float OarRadius = 6f;
        public const float OarFraction = 0.04f;
        public const float OarBossFraction = 0.005f;

        public const float RetrySeconds = 30f;
        public const float FreedHealFraction = 0.2f;
        public const float BurstRadius = 20f;
        public const float BurstFraction = 0.12f;
        public const float BurstBossFraction = 0.02f;

        public const float SailSpeed = 55f;
        public const float SailArc = 1.5f;
        public const float StationReach = 5f;

        /// <summary>Hull lost per second to <paramref name="beasts"/> common beasts and <paramref name="bosses"/> bosses by the hull (at most <see cref="MaxGnawers"/> counted, bosses first).</summary>
        public static float GnawRate(int beasts, int bosses)
        {
            int b = Mathf.Clamp(bosses, 0, MaxGnawers);
            int c = Mathf.Clamp(beasts, 0, MaxGnawers - b);
            return b * BossGnawPerSecond + c * GnawPerSecond;
        }

        /// <summary>The rowers' strike on a beast within <see cref="OarRadius"/> of the hull.</summary>
        public static float OarAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? OarBossFraction : OarFraction);

        /// <summary>The light of the freed barque on a beast within <see cref="BurstRadius"/>.</summary>
        public static float BurstAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? BurstBossFraction : BurstFraction);

        /// <summary>The pull a hero on the bow takes in the hull's place: a slice of max life that never goes under <see cref="PullFloorFraction"/>.</summary>
        public static float PullOnHero(float currentHealth, float maxHealth) => ProfundoHazardRules.Bite(currentHealth, maxHealth, PullHeroFraction, PullFloorFraction);

        /// <summary>True when the barque was freed on in-game day <paramref name="today"/> (the daily mark stores the day; 0 = never).</summary>
        public static bool FreedToday(int markedDay, int today) => markedDay > 0 && markedDay == today;

        /// <summary>Can the rowers be called? <paramref name="sinceFailure"/> = seconds since the last sunk or abandoned run.</summary>
        public static RioDefenseStart CheckStart(bool freedToday, float sinceFailure, bool zoneOpen)
        {
            if (freedToday)
            {
                return RioDefenseStart.AlreadyFreed;
            }

            if (!zoneOpen)
            {
                return RioDefenseStart.ZoneBlocked;
            }

            return sinceFailure < RetrySeconds ? RioDefenseStart.RowersResting : RioDefenseStart.Ok;
        }

        /// <summary>Seconds of a sail of <paramref name="metres"/> on the freed barque.</summary>
        public static float SailSeconds(float metres) => Mathf.Max(0.5f, metres / SailSpeed);
    }

    /// <summary>One defence of the stranded barque (pure state, ticked by the driver; see <see cref="RioBarqueDefenseRules"/>).</summary>
    public sealed class RioBarqueDefense
    {
        public bool Running { get; private set; }

        /// <summary>Seconds since the rowers were called (Apep's pulls follow it, guarded or not).</summary>
        public float Elapsed { get; private set; }

        /// <summary>Seconds of guarded pushing, up to <see cref="RioBarqueDefenseRules.RunSeconds"/>.</summary>
        public float Pushed { get; private set; }

        public float Hull { get; private set; }

        public float Away { get; private set; }

        /// <summary>Whether the last pull fell on the hero on the bow rather than on the hull.</summary>
        public bool LastPullOnHero { get; private set; }

        public float Progress => Mathf.Clamp01(Pushed / RioBarqueDefenseRules.RunSeconds);

        public float HullFraction => Mathf.Clamp01(Hull / RioBarqueDefenseRules.HullMax);

        /// <summary>True from the warning until the pull lands.</summary>
        public bool PullImminent => Running && SecondsToNextPull <= RioBarqueDefenseRules.PullWarning;

        public float SecondsToNextPull => RioBarqueDefenseRules.PullEvery - Mathf.Repeat(Elapsed, RioBarqueDefenseRules.PullEvery);

        public void Begin()
        {
            Running = true;
            Elapsed = 0f;
            Pushed = 0f;
            Away = 0f;
            Hull = RioBarqueDefenseRules.HullMax;
            LastPullOnHero = false;
        }

        public void Cancel() => Running = false;

        /// <summary>
        /// Advances the defence by <paramref name="dt"/> seconds with the hero <paramref name="heroDistance"/> metres from the hull and the
        /// beasts by it gnawing <paramref name="gnawPerSecond"/> (<see cref="RioBarqueDefenseRules.GnawRate"/>).
        /// </summary>
        public RioDefenseEvent Tick(float dt, float heroDistance, float gnawPerSecond)
        {
            if (!Running || dt <= 0f)
            {
                return RioDefenseEvent.None;
            }

            var happened = RioDefenseEvent.None;
            float before = Elapsed;
            Elapsed += dt;
            bool guarded = heroDistance <= RioBarqueDefenseRules.GuardRadius;
            if (guarded)
            {
                Away = 0f;
                Pushed = Mathf.Min(RioBarqueDefenseRules.RunSeconds, Pushed + dt);
            }
            else
            {
                Away += dt;
                happened |= RioDefenseEvent.HeroAway;
            }

            Hull -= Mathf.Max(0f, gnawPerSecond) * dt;

            float every = RioBarqueDefenseRules.PullEvery;
            if (Crossed(before, Elapsed, every, every - RioBarqueDefenseRules.PullWarning))
            {
                happened |= RioDefenseEvent.PullWarning;
            }

            if (Crossed(before, Elapsed, every, 0f))
            {
                happened |= RioDefenseEvent.Pull;
                LastPullOnHero = heroDistance <= RioBarqueDefenseRules.BowReach;
                if (!LastPullOnHero)
                {
                    Hull -= RioBarqueDefenseRules.PullHullDamage;
                }
            }

            if (Crossed(before, Elapsed, RioBarqueDefenseRules.OarEvery, 0f))
            {
                happened |= RioDefenseEvent.Oars;
            }

            if (Hull <= 0f)
            {
                Hull = 0f;
                Running = false;
                return happened | RioDefenseEvent.Sunk;
            }

            if (Away >= RioBarqueDefenseRules.AwaySeconds)
            {
                Running = false;
                return happened | RioDefenseEvent.Abandoned;
            }

            if (Pushed >= RioBarqueDefenseRules.RunSeconds)
            {
                Running = false;
                return happened | RioDefenseEvent.Freed;
            }

            return happened;
        }

        /// <summary>True when (before, after] holds a moment k·period + offset (k ≥ 0 when offset &gt; 0, k ≥ 1 when it is 0: nothing fires at the start).</summary>
        private static bool Crossed(float before, float after, float period, float offset)
        {
            return Mathf.FloorToInt((after - offset) / period) > Mathf.FloorToInt((before - offset) / period);
        }
    }
}
