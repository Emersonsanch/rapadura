using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>The four calls of the marcador (each one a direction: arrow keys / WASD / the four buttons).</summary>
    public enum QuadrilhaMove
    {
        /// <summary>"Anarriê!" — everyone steps back (left).</summary>
        Anarrie,
        /// <summary>"Balancê!" — swing in place (up).</summary>
        Balance,
        /// <summary>"Olha a cobra!" — jump (down).</summary>
        Cobra,
        /// <summary>"Caminho da roça!" — the line walks on (right).</summary>
        CaminhoDaRoca,
    }

    public enum QuadrilhaJudgement
    {
        /// <summary>Not judged yet / a press with no step near it (ignored, no penalty).</summary>
        None,
        Perfect,
        Good,
        Miss,
    }

    /// <summary>One call of the dance: when it lands and which step it asks for.</summary>
    public sealed class QuadrilhaStep
    {
        public readonly int Index;
        public readonly float Time;
        public readonly QuadrilhaMove Move;

        public QuadrilhaStep(int index, float time, QuadrilhaMove move)
        {
            Index = index;
            Time = time;
            Move = move;
        }

        public QuadrilhaJudgement Judgement { get; internal set; }
        public bool IsJudged => Judgement != QuadrilhaJudgement.None;
    }

    /// <summary>
    /// The quadrilha of the Festa Junina as a small rhythm game (pure, deterministic, EditMode-tested): the marcador
    /// calls <see cref="StepCount"/> steps, one every <see cref="Beat"/> seconds after a <see cref="LeadIn"/>; each one
    /// must be answered with the right move within <see cref="GoodWindow"/> seconds of its beat (<see cref="PerfectWindow"/>
    /// for a perfect). A wrong move or a step let past is a miss. The order of the calls comes from the seed only
    /// (the panel uses the day, so a day's dance is the same on every replay), never three of the same in a row.
    /// "Dança tranquila" (or Reduzir Movimento) widens both windows and slows the beat.
    /// </summary>
    public sealed class QuadrilhaSession
    {
        public const int DefaultSteps = 16;
        public const float DefaultBeat = 0.9f;
        public const float RelaxedBeat = 1.15f;
        public const float DefaultLeadIn = 2f;
        public const float DefaultPerfectWindow = 0.12f;
        public const float DefaultGoodWindow = 0.28f;
        public const float RelaxedWindowScale = 1.6f;
        public const int PerfectPoints = 2;
        public const int GoodPoints = 1;

        private readonly List<QuadrilhaStep> _steps = new List<QuadrilhaStep>();
        private int _next;

        public QuadrilhaSession(int seed, int steps = DefaultSteps, bool relaxed = false)
        {
            StepCount = Math.Max(1, steps);
            Relaxed = relaxed;
            Beat = relaxed ? RelaxedBeat : DefaultBeat;
            LeadIn = DefaultLeadIn;
            PerfectWindow = DefaultPerfectWindow * (relaxed ? RelaxedWindowScale : 1f);
            GoodWindow = DefaultGoodWindow * (relaxed ? RelaxedWindowScale : 1f);

            foreach (QuadrilhaMove move in Sequence(seed, StepCount))
            {
                _steps.Add(new QuadrilhaStep(_steps.Count, LeadIn + _steps.Count * Beat, move));
            }
        }

        public int StepCount { get; }
        public bool Relaxed { get; }
        public float Beat { get; }
        public float LeadIn { get; }
        public float PerfectWindow { get; }
        public float GoodWindow { get; }

        public IReadOnlyList<QuadrilhaStep> Steps => _steps;
        public float Time { get; private set; }
        public int Score { get; private set; }
        public int MaxScore => StepCount * PerfectPoints;
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int Perfects { get; private set; }
        public int Goods { get; private set; }
        public int Misses { get; private set; }
        public QuadrilhaJudgement LastJudgement { get; private set; }

        /// <summary>The first step still waiting for an answer (null when the dance is over).</summary>
        public QuadrilhaStep NextStep => _next < _steps.Count ? _steps[_next] : null;

        public bool IsFinished => _next >= _steps.Count;

        /// <summary>The whole dance without a single miss.</summary>
        public bool IsFullCombo => IsFinished && Misses == 0;

        /// <summary>Seconds of the whole dance (last beat + its window).</summary>
        public float Duration => LeadIn + (StepCount - 1) * Beat + GoodWindow;

        /// <summary>The calls for <paramref name="seed"/>: never three of the same move in a row.</summary>
        public static List<QuadrilhaMove> Sequence(int seed, int count)
        {
            var random = new Random(seed);
            var moves = new List<QuadrilhaMove>(count);
            for (int i = 0; i < count; i++)
            {
                QuadrilhaMove move = (QuadrilhaMove)random.Next(4);
                if (i >= 2 && moves[i - 1] == move && moves[i - 2] == move)
                {
                    move = (QuadrilhaMove)(((int)move + 1 + random.Next(3)) % 4);
                }

                moves.Add(move);
            }

            return moves;
        }

        /// <summary>Advances the music; steps whose window closed unanswered become misses.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f)
            {
                Time += deltaTime;
            }

            while (_next < _steps.Count && Time > _steps[_next].Time + GoodWindow)
            {
                Judge(_steps[_next], QuadrilhaJudgement.Miss);
            }
        }

        /// <summary>The player answered with <paramref name="move"/> now. A press with no step in its window is ignored (None).</summary>
        public QuadrilhaJudgement Press(QuadrilhaMove move)
        {
            Tick(0f);
            QuadrilhaStep step = NextStep;
            if (step == null)
            {
                return QuadrilhaJudgement.None;
            }

            float offset = Math.Abs(Time - step.Time);
            if (offset > GoodWindow)
            {
                return QuadrilhaJudgement.None;
            }

            QuadrilhaJudgement judgement = step.Move != move
                ? QuadrilhaJudgement.Miss
                : offset <= PerfectWindow ? QuadrilhaJudgement.Perfect : QuadrilhaJudgement.Good;
            Judge(step, judgement);
            return judgement;
        }

        private void Judge(QuadrilhaStep step, QuadrilhaJudgement judgement)
        {
            step.Judgement = judgement;
            LastJudgement = judgement;
            _next++;
            switch (judgement)
            {
                case QuadrilhaJudgement.Perfect:
                    Perfects++;
                    Score += PerfectPoints;
                    Combo++;
                    break;
                case QuadrilhaJudgement.Good:
                    Goods++;
                    Score += GoodPoints;
                    Combo++;
                    break;
                default:
                    Misses++;
                    Combo = 0;
                    break;
            }

            BestCombo = Math.Max(BestCombo, Combo);
        }
    }
}
