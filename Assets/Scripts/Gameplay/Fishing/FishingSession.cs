using System.Collections.Generic;
using Rapadura.Gameplay.Environment;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>Where a cast is (<see cref="FishingSession.Phase"/>).</summary>
    public enum FishingPhase
    {
        /// <summary>Line in hand: the action casts.</summary>
        Ready,
        /// <summary>Line in the air (short): the float lands after <see cref="FishingSession.CastSeconds"/>.</summary>
        Casting,
        /// <summary>Float on the water: nibbles, tremble, bite. The action hooks.</summary>
        Waiting,
        /// <summary>Fish on the line: hold to reel, release to relax.</summary>
        Reeling,
        /// <summary>Over (<see cref="FishingSession.Result"/>): the action goes back to <see cref="Ready"/>.</summary>
        Result,
    }

    /// <summary>How a cast ended.</summary>
    public enum FishingResult
    {
        None,
        Landed,
        TooEarly,
        TooLate,
        Snapped,
        Escaped,
    }

    /// <summary>Things the UI shows or plays this tick (<see cref="FishingSession.Cues"/>), in order.</summary>
    public enum FishingCue
    {
        Cast,
        Splash,
        Nibble,
        Tremble,
        Bite,
        Hooked,
        Surge,
        Landed,
        Snapped,
        Escaped,
        TooEarly,
        TooLate,
    }

    /// <summary>
    /// One fishing trip at a spot, as a Unity-free state machine (driven by <c>FishingPanelController</c>, tested by
    /// <c>FishingTests</c>). One action does everything — tap to cast, tap to hook, hold to reel — so it plays with
    /// one finger, Space or the gamepad's south button.
    /// <list type="bullet">
    /// <item><see cref="Press"/>: cast (Ready), hook (Waiting), next cast (Result).</item>
    /// <item><see cref="Tick"/>: advances time; <paramref name="held"/> is whether the action is held (reeling).</item>
    /// <item>The species is rolled at the cast from <see cref="FishingRules.BuildCatchTable"/> for the conditions
    /// given at construction (the panel rebuilds the session when time of day or weather change).</item>
    /// </list>
    /// </summary>
    public sealed class FishingSession
    {
        public const float CastSeconds = 0.6f;

        private readonly FishingRandom _random;
        private readonly List<CatchTableEntry> _table;
        private readonly List<FishingCue> _cues = new List<FishingCue>();
        private int _nextNibble;
        private bool _trembled;
        private bool _bitten;
        private float _window;
        private ReelParams _reelParams;

        public FishingSession(FishingSpotSpec spot, TimeOfDayPhase phase, WeatherType weather, bool assist, int seed, IReadOnlyList<FishSpec> catalog = null)
        {
            Spot = spot;
            TimeOfDay = phase;
            Weather = weather;
            Assist = assist;
            _random = new FishingRandom(seed);
            _table = FishingRules.BuildCatchTable(spot, phase, weather, catalog);
        }

        public FishingSpotSpec Spot { get; }
        public TimeOfDayPhase TimeOfDay { get; }
        public WeatherType Weather { get; }
        public bool Assist { get; set; }

        public FishingPhase Phase { get; private set; } = FishingPhase.Ready;
        public FishingResult Result { get; private set; } = FishingResult.None;

        /// <summary>Seconds in the current phase (Casting/Waiting: since the cast/the float landed).</summary>
        public float PhaseTime { get; private set; }

        /// <summary>The species on the other end of the line (rolled at the cast), or null.</summary>
        public FishSpec Fish { get; private set; }

        public BitePlan Plan { get; private set; }
        public ReelState Reel { get; private set; }

        /// <summary>Grams of the fish just landed (0 otherwise).</summary>
        public int LandedGrams { get; private set; }

        /// <summary>The float is twitching right now (a nibble or the tremble), for the anticipation visual.</summary>
        public bool FloatTwitching { get; private set; }

        /// <summary>The float is under water (bite window open).</summary>
        public bool FloatSunk => Phase == FishingPhase.Waiting && _bitten;

        public IReadOnlyList<CatchTableEntry> CatchTable => _table;

        /// <summary>Cues raised by the last <see cref="Press"/>/<see cref="Tick"/> (cleared at the start of each).</summary>
        public IReadOnlyList<FishingCue> Cues => _cues;

        /// <summary>The single action: cast, hook, or start over.</summary>
        public void Press()
        {
            _cues.Clear();
            switch (Phase)
            {
                case FishingPhase.Ready:
                case FishingPhase.Result:
                    Cast();
                    break;

                case FishingPhase.Waiting:
                    Hook();
                    break;
            }
        }

        public void Tick(float deltaTime, bool held)
        {
            _cues.Clear();
            if (deltaTime <= 0f)
            {
                return;
            }

            switch (Phase)
            {
                case FishingPhase.Casting:
                    PhaseTime += deltaTime;
                    if (PhaseTime >= CastSeconds)
                    {
                        Enter(FishingPhase.Waiting);
                        _cues.Add(FishingCue.Splash);
                    }

                    break;

                case FishingPhase.Waiting:
                    TickWaiting(deltaTime);
                    break;

                case FishingPhase.Reeling:
                    TickReeling(deltaTime, held);
                    break;
            }
        }

        private void Cast()
        {
            Result = FishingResult.None;
            LandedGrams = 0;
            Fish = FishingRules.RollCatch(_table, _random.Next01());
            Plan = FishingRules.PlanBite(_random, Spot, Weather, Assist);
            _window = FishingRules.HookWindowSeconds(Fish != null ? Fish.Rarity : FishRarity.Common, Assist);
            _nextNibble = 0;
            _trembled = false;
            _bitten = false;
            FloatTwitching = false;
            Enter(FishingPhase.Casting);
            _cues.Add(FishingCue.Cast);
        }

        private void TickWaiting(float deltaTime)
        {
            PhaseTime += deltaTime;
            FloatTwitching = false;

            while (_nextNibble < Plan.NibbleTimes.Count && PhaseTime >= Plan.NibbleTimes[_nextNibble])
            {
                _nextNibble++;
                FloatTwitching = true;
                _cues.Add(FishingCue.Nibble);
            }

            if (!_trembled && PhaseTime >= Plan.TrembleAt)
            {
                _trembled = true;
                _cues.Add(FishingCue.Tremble);
            }

            if (_trembled && !_bitten)
            {
                FloatTwitching = true;
            }

            if (!_bitten && PhaseTime >= Plan.BiteAt)
            {
                _bitten = true;
                FloatTwitching = false;
                _cues.Add(FishingCue.Bite);
            }

            if (Fish == null || PhaseTime > Plan.BiteAt + _window)
            {
                Finish(FishingResult.TooLate, FishingCue.TooLate);
            }
        }

        private void Hook()
        {
            HookResult hook = FishingRules.EvaluateHook(PhaseTime, Plan, _window, Assist);
            if (Fish == null || hook == HookResult.TooLate)
            {
                Finish(FishingResult.TooLate, FishingCue.TooLate);
                return;
            }

            if (hook == HookResult.TooEarly)
            {
                Finish(FishingResult.TooEarly, FishingCue.TooEarly);
                return;
            }

            _reelParams = FishingRules.ReelParamsFor(Fish, Assist);
            Reel = FishingRules.StartReel(_random, Fish);
            Enter(FishingPhase.Reeling);
            _cues.Add(FishingCue.Hooked);
        }

        private void TickReeling(float deltaTime, bool held)
        {
            // Big frame hitches are split so the physics stays the same at any frame rate.
            float remaining = deltaTime;
            while (remaining > 0f && Phase == FishingPhase.Reeling)
            {
                float step = remaining > FishingRules.MaxStep ? FishingRules.MaxStep : remaining;
                remaining -= step;
                PhaseTime += step;

                bool wasSurging = Reel.Surging;
                Reel = FishingRules.StepReel(Reel, _reelParams, Fish, held, step, _random);
                if (Reel.Surging && !wasSurging)
                {
                    _cues.Add(FishingCue.Surge);
                }

                switch (Reel.Outcome)
                {
                    case ReelOutcome.Landed:
                        LandedGrams = FishingRules.RollWeightGrams(Fish, _random.Next01(), _random.Next01());
                        Finish(FishingResult.Landed, FishingCue.Landed);
                        break;
                    case ReelOutcome.Snapped:
                        Finish(FishingResult.Snapped, FishingCue.Snapped);
                        break;
                    case ReelOutcome.Escaped:
                        Finish(FishingResult.Escaped, FishingCue.Escaped);
                        break;
                }
            }
        }

        private void Finish(FishingResult result, FishingCue cue)
        {
            Result = result;
            FloatTwitching = false;
            Enter(FishingPhase.Result);
            _cues.Add(cue);
        }

        private void Enter(FishingPhase phase)
        {
            Phase = phase;
            PhaseTime = 0f;
        }
    }
}
