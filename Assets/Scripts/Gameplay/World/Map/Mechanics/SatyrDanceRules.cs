using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How a step of the Baile do Vale landed (<see cref="SatyrDanceRun.Press"/>).</summary>
    public enum SatyrStep
    {
        /// <summary>Out of time: no beat near enough (a stumble, −1).</summary>
        Stumble = 0,

        /// <summary>Near the beat (within <see cref="SatyrDanceRules.OkWindow"/>): 1 point.</summary>
        Close = 1,

        /// <summary>On the beat (within <see cref="SatyrDanceRules.GoodWindow"/>): 2 points.</summary>
        OnBeat = 2,
    }

    /// <summary>
    /// The Baile do Vale of the Vale dos Sátiros (Docs/Mapas100.md 53: "Baile do Vale (§3.3)", "Noite: o baile"), pure and server-runnable. At
    /// night, on the stone stage of the square, [E] asks Marsyas for a dance: after a count-in his double flute plays twelve beats that speed up
    /// (four slow, four middling, four quick), and every [E] is a step. A step on the beat is worth 2, near it 1, out of time a stumble (−1, so
    /// mashing never wins); a beat let past is just lost. The medal comes from the points (24 at most); the first medal of the UTC day pays
    /// gold nuggets, and every medal leaves the cousin light on their feet for a while (stamina recovery).
    /// <code>
    /// night only · stage reach 6 m · count-in 3 s · beats every 1.2 s ×4, 0.9 s ×4, 0.7 s ×4 (the first at 3 s, the last at 13 s)
    /// on the beat ±0.15 s: 2 · close ±0.3 s: 1 · stumble: −1 · gold ≥ 20 · silver ≥ 14 · bronze ≥ 8
    /// first medal of the UTC day: 50 / 30 / 15 nuggets · any medal: +15 % (gold) or +10 % stamina recovery for 300 s
    /// leaving the stage (8 m), mounting, taking a hit or the dawn ends the dance without a medal
    /// </code>
    /// Decision (2026-11-23): [E] instead of new keys or a screen of its own, and the beat shown on the stage (a column that flashes) and by the
    /// flute's words, so it plays on every device; the pay is a race's (map 47) but smaller, since a dance takes 16 s.
    /// </summary>
    public static class SatyrDanceRules
    {
        public const int BeatCount = 12;
        public const float CountIn = 3f;
        public const float GoodWindow = 0.15f;
        public const float OkWindow = 0.3f;
        public const float StageReach = 6f;
        public const float LeaveReach = 8f;
        public const int GoldPoints = 20;
        public const int SilverPoints = 14;
        public const int BronzePoints = 8;
        public const float BuffSeconds = 300f;
        public const float BuffValue = 0.1f;
        public const float GoldBuffValue = 0.15f;

        /// <summary>The daily mark of the paid medal (the driver's daily mark key).</summary>
        public const string PaidKey = "farnkesh.dance.paid";

        /// <summary>The save mark of the first gold (the cousin's line, once).</summary>
        public const string FirstGoldKey = "farnkesh.dance.gold";

        /// <summary>Where the dance stage stands, from the town's centre (the recipe's big stone stage in the square).</summary>
        public static readonly Vector2 StageFromTown = new Vector2(-3.6f, -3.9f);

        /// <summary>The seconds between beat <paramref name="beat"/> and the one before it: 1.2, then 0.9, then 0.7.</summary>
        public static float GapBefore(int beat) => beat < 4 ? 1.2f : beat < 8 ? 0.9f : 0.7f;

        /// <summary>Seconds after the start when beat <paramref name="beat"/> (0 first) plays.</summary>
        public static float BeatTime(int beat)
        {
            int b = Mathf.Clamp(beat, 0, BeatCount - 1);
            float t = CountIn;
            for (int i = 1; i <= b; i++)
            {
                t += GapBefore(i);
            }

            return t;
        }

        /// <summary>When the dance is over: the last beat and its window.</summary>
        public static float EndTime => BeatTime(BeatCount - 1) + OkWindow;

        public static int MaxPoints => BeatCount * (int)SatyrStep.OnBeat;

        /// <summary>The grade of a step <paramref name="offset"/> seconds off a beat.</summary>
        public static SatyrStep Grade(float offset)
        {
            float off = Mathf.Abs(offset);
            if (off <= GoodWindow)
            {
                return SatyrStep.OnBeat;
            }

            return off <= OkWindow ? SatyrStep.Close : SatyrStep.Stumble;
        }

        public static int PointsOf(SatyrStep step) => step == SatyrStep.Stumble ? -1 : (int)step;

        /// <summary>The medal of <paramref name="points"/>: 3 gold, 2 silver, 1 bronze, 0 none (the race's numbering).</summary>
        public static int Medal(int points) => points >= GoldPoints ? 3 : points >= SilverPoints ? 2 : points >= BronzePoints ? 1 : 0;

        /// <summary>The nuggets of a medal: only the first medal of the UTC day pays.</summary>
        public static int Purse(int medal, bool firstToday)
        {
            if (!firstToday)
            {
                return 0;
            }

            switch (medal)
            {
                case 3: return 50;
                case 2: return 30;
                case 1: return 15;
                default: return 0;
            }
        }

        /// <summary>The stamina recovery bonus a medal leaves (0 without one).</summary>
        public static float BuffOf(int medal) => medal >= 3 ? GoldBuffValue : medal > 0 ? BuffValue : 0f;

        public static bool OnStage(Vector2 stage, Vector2 p) => (stage - p).sqrMagnitude <= StageReach * StageReach;

        public static bool LeftStage(Vector2 stage, Vector2 p) => (stage - p).sqrMagnitude > LeaveReach * LeaveReach;
    }

    /// <summary>One dance (<see cref="SatyrDanceRules"/>): times are seconds since it started, from any clock the caller keeps.</summary>
    public sealed class SatyrDanceRun
    {
        private readonly bool[] _judged = new bool[SatyrDanceRules.BeatCount];

        public int Points { get; private set; }
        public int OnBeat { get; private set; }
        public int Close { get; private set; }
        public int Stumbles { get; private set; }
        public int Missed { get; private set; }

        /// <summary>The first beat not yet stepped on nor let past.</summary>
        public int NextBeat { get; private set; }

        public bool Over(float t) => NextBeat >= SatyrDanceRules.BeatCount || t > SatyrDanceRules.EndTime;

        /// <summary>A step at <paramref name="t"/>: graded against the nearest open beat (each beat takes one step).</summary>
        public SatyrStep Press(float t)
        {
            Tick(t);
            int best = -1;
            float bestOff = float.MaxValue;
            for (int i = NextBeat; i < SatyrDanceRules.BeatCount; i++)
            {
                if (_judged[i])
                {
                    continue;
                }

                float off = Mathf.Abs(t - SatyrDanceRules.BeatTime(i));
                if (off < bestOff)
                {
                    best = i;
                    bestOff = off;
                }
            }

            SatyrStep step = best >= 0 ? SatyrDanceRules.Grade(bestOff) : SatyrStep.Stumble;
            if (step == SatyrStep.Stumble)
            {
                Stumbles++;
                Points = Mathf.Max(0, Points - 1);
                return step;
            }

            _judged[best] = true;
            Points += SatyrDanceRules.PointsOf(step);
            if (step == SatyrStep.OnBeat)
            {
                OnBeat++;
            }
            else
            {
                Close++;
            }

            Advance();
            return step;
        }

        /// <summary>Lets past the beats whose window closed before <paramref name="t"/>; returns how many were lost now.</summary>
        public int Tick(float t)
        {
            int lost = 0;
            for (int i = NextBeat; i < SatyrDanceRules.BeatCount; i++)
            {
                if (_judged[i] || SatyrDanceRules.BeatTime(i) + SatyrDanceRules.OkWindow >= t)
                {
                    continue;
                }

                _judged[i] = true;
                Missed++;
                lost++;
            }

            Advance();
            return lost;
        }

        private void Advance()
        {
            while (NextBeat < SatyrDanceRules.BeatCount && _judged[NextBeat])
            {
                NextBeat++;
            }
        }
    }
}
