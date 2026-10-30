using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Environment;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>Seeded random source for the fishing rules (System.Random: same seed, same fishing trip).</summary>
    public sealed class FishingRandom
    {
        private readonly Random _random;

        public FishingRandom(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>Uniform in [0, 1).</summary>
        public double Next01() => _random.NextDouble();

        public float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        /// <summary>Uniform integer in [min, maxExclusive).</summary>
        public int Range(int min, int maxExclusive) => _random.Next(min, maxExclusive);
    }

    /// <summary>One row of a catch table: a species and its relative chance right now.</summary>
    public readonly struct CatchTableEntry
    {
        public readonly FishSpec Fish;
        public readonly float Weight;

        public CatchTableEntry(FishSpec fish, float weight)
        {
            Fish = fish;
            Weight = weight;
        }
    }

    /// <summary>When things happen between the cast and the bite (seconds since the float landed).</summary>
    public sealed class BitePlan
    {
        /// <summary>Fake nibbles ("beliscadas"): the float twitches, but hooking now is too early.</summary>
        public readonly IReadOnlyList<float> NibbleTimes;
        /// <summary>The float starts trembling: the readable "get ready" cue, always <see cref="FishingRules.AnticipationLead"/> before the bite.</summary>
        public readonly float TrembleAt;
        /// <summary>The real bite: the float sinks.</summary>
        public readonly float BiteAt;

        public BitePlan(IReadOnlyList<float> nibbleTimes, float trembleAt, float biteAt)
        {
            NibbleTimes = nibbleTimes ?? Array.Empty<float>();
            TrembleAt = trembleAt;
            BiteAt = biteAt;
        }
    }

    /// <summary>What pressing "Fisgar" at a given moment did.</summary>
    public enum HookResult
    {
        /// <summary>Before the bite (a nibble, or nothing): the fish got scared.</summary>
        TooEarly,
        Hooked,
        /// <summary>After the window closed: the fish took the bait and left.</summary>
        TooLate,
    }

    /// <summary>How a fight with a hooked fish is going.</summary>
    public enum ReelOutcome
    {
        Fighting,
        Landed,
        /// <summary>Tension stayed at the top too long: the line broke.</summary>
        Snapped,
        /// <summary>Slack line too long (or the fish took all the line back): it spat the hook.</summary>
        Escaped,
    }

    /// <summary>Tuning of one fight (<see cref="FishingRules.ReelParamsFor"/>).</summary>
    public readonly struct ReelParams
    {
        /// <summary>Tension gained per second while reeling (before the fish's pull).</summary>
        public readonly float ReelTensionRate;
        /// <summary>Tension lost per second while the line is released.</summary>
        public readonly float RelaxRate;
        /// <summary>Constant pull of the fish (tension per second).</summary>
        public readonly float BasePull;
        /// <summary>Extra pull during a surge ("tranco").</summary>
        public readonly float SurgePull;
        /// <summary>Progress per second while reeling.</summary>
        public readonly float ReelSpeed;
        /// <summary>Progress lost per second while the fish surges and the line is released (it runs with the line).</summary>
        public readonly float SwimBack;
        /// <summary>Seconds at full tension before the line snaps.</summary>
        public readonly float SnapGrace;
        /// <summary>Seconds of slack line before the fish spits the hook.</summary>
        public readonly float SlackGrace;

        public ReelParams(float reelTensionRate, float relaxRate, float basePull, float surgePull, float reelSpeed, float swimBack, float snapGrace, float slackGrace)
        {
            ReelTensionRate = reelTensionRate;
            RelaxRate = relaxRate;
            BasePull = basePull;
            SurgePull = surgePull;
            ReelSpeed = reelSpeed;
            SwimBack = swimBack;
            SnapGrace = snapGrace;
            SlackGrace = slackGrace;
        }
    }

    /// <summary>State of a fight, advanced by <see cref="FishingRules.StepReel"/>.</summary>
    public struct ReelState
    {
        /// <summary>0..1 line tension (the bar).</summary>
        public float Tension;
        /// <summary>0..1 line reeled in; 1 = landed.</summary>
        public float Progress;
        public float OverTensionSeconds;
        public float SlackSeconds;
        public bool Surging;
        public float SurgeSecondsLeft;
        public float NextSurgeIn;
        public ReelOutcome Outcome;
    }

    /// <summary>
    /// The rules of "Pesca no açude", pure and Unity-free (tested by <c>FishingTests</c>). Every random number comes
    /// from a <see cref="FishingRandom"/>, so a seed replays a whole trip.
    /// <list type="bullet">
    /// <item><b>Catch table</b>: a species bites at a spot it lives in, in a phase it allows. Its weight is the base
    /// of its rarity, ×<see cref="PreferredPhaseMultiplier"/> in a favourite phase and ×<see cref="PreferredWeatherMultiplier"/>
    /// in a favourite weather (a sheltered spot always counts as clear).</item>
    /// <item><b>Bite</b>: after a random wait (shorter in the rain, scaled per spot) the float trembles
    /// (<see cref="AnticipationLead"/>) and then sinks. Before that, up to two nibbles twitch it: hooking on a nibble
    /// is too early. The hook window is shorter for rarer fish and much wider with assist (Reduzir Movimento or
    /// "Pesca tranquila").</item>
    /// <item><b>Tension</b>: holding reels in (progress) and raises tension; releasing relaxes it. The fish surges now
    /// and then ("tranco"): reeling through a surge spikes the tension toward a snap, releasing lets it run with some
    /// line; releasing for too long lets it spit the hook. One input (hold/release), so it plays with one finger.</item>
    /// </list>
    /// </summary>
    public static class FishingRules
    {
        public const float CommonWeight = 60f;
        public const float UncommonWeight = 24f;
        public const float RareWeight = 8f;
        public const float LegendaryWeight = 1.5f;
        public const float PreferredPhaseMultiplier = 2.5f;
        public const float PreferredWeatherMultiplier = 2f;

        public const float MinBiteDelay = 2.5f;
        public const float MaxBiteDelay = 8f;
        public const float RainBiteDelayScale = 0.85f;
        /// <summary>Seconds of trembling before the real bite (assist: <see cref="AssistAnticipationLead"/>).</summary>
        public const float AnticipationLead = 0.5f;
        public const float AssistAnticipationLead = 0.9f;
        /// <summary>A press this much before the bite still hooks (reaction to the tremble).</summary>
        public const float EarlyGrace = 0.1f;
        public const float AssistEarlyGrace = 0.4f;
        public const float AssistWindowScale = 2f;
        public const float AssistMinWindow = 1.2f;

        public const float StartTension = 0.3f;
        public const float StartProgress = 0.15f;
        public const float SlackThreshold = 0.05f;
        public const float MaxStep = 0.1f;

        public static float BaseWeight(FishRarity rarity) => rarity switch
        {
            FishRarity.Uncommon => UncommonWeight,
            FishRarity.Rare => RareWeight,
            FishRarity.Legendary => LegendaryWeight,
            _ => CommonWeight,
        };

        public static PhaseMask ToMask(TimeOfDayPhase phase) => phase switch
        {
            TimeOfDayPhase.Dawn => PhaseMask.Dawn,
            TimeOfDayPhase.Dusk => PhaseMask.Dusk,
            TimeOfDayPhase.Night => PhaseMask.Night,
            _ => PhaseMask.Day,
        };

        /// <summary>The weather the water feels: a sheltered spot (the cave lake) is always clear.</summary>
        public static WeatherType EffectiveWeather(FishingSpotSpec spot, WeatherType weather) =>
            spot != null && spot.Sheltered ? WeatherType.Clear : weather;

        /// <summary>Relative chance of <paramref name="fish"/> right now (0 = it does not bite here / now).</summary>
        public static float CatchWeight(FishSpec fish, FishingSpotSpec spot, TimeOfDayPhase phase, WeatherType weather)
        {
            if (fish == null || spot == null || !fish.LivesIn(spot.Id))
            {
                return 0f;
            }

            PhaseMask mask = ToMask(phase);
            if ((fish.AllowedPhases & mask) == 0)
            {
                return 0f;
            }

            float weight = BaseWeight(fish.Rarity);
            if ((fish.PreferredPhases & mask) != 0)
            {
                weight *= PreferredPhaseMultiplier;
            }

            WeatherType felt = EffectiveWeather(spot, weather);
            foreach (WeatherType preferred in fish.PreferredWeather)
            {
                if (preferred == felt)
                {
                    weight *= PreferredWeatherMultiplier;
                    break;
                }
            }

            return weight;
        }

        /// <summary>Every species that bites at <paramref name="spot"/> now, with its weight (catalog order).</summary>
        public static List<CatchTableEntry> BuildCatchTable(FishingSpotSpec spot, TimeOfDayPhase phase, WeatherType weather, IReadOnlyList<FishSpec> catalog = null)
        {
            var table = new List<CatchTableEntry>();
            foreach (FishSpec fish in catalog ?? FishCatalog.Fish)
            {
                float weight = CatchWeight(fish, spot, phase, weather);
                if (weight > 0f)
                {
                    table.Add(new CatchTableEntry(fish, weight));
                }
            }

            return table;
        }

        /// <summary>The species <paramref name="roll01"/> (uniform in [0, 1)) lands on, or null for an empty table.</summary>
        public static FishSpec RollCatch(IReadOnlyList<CatchTableEntry> table, double roll01)
        {
            if (table == null || table.Count == 0)
            {
                return null;
            }

            double total = 0d;
            foreach (CatchTableEntry entry in table)
            {
                total += entry.Weight;
            }

            double target = Math.Max(0d, Math.Min(roll01, 0.999999d)) * total;
            double cumulative = 0d;
            foreach (CatchTableEntry entry in table)
            {
                cumulative += entry.Weight;
                if (target < cumulative)
                {
                    return entry.Fish;
                }
            }

            return table[table.Count - 1].Fish;
        }

        /// <summary>Chance (0..1) of <paramref name="fishId"/> in <paramref name="table"/>.</summary>
        public static float Chance(IReadOnlyList<CatchTableEntry> table, string fishId)
        {
            float total = 0f;
            float mine = 0f;
            foreach (CatchTableEntry entry in table)
            {
                total += entry.Weight;
                if (entry.Fish.Id == fishId)
                {
                    mine += entry.Weight;
                }
            }

            return total > 0f ? mine / total : 0f;
        }

        // ------------------------------------------------------------------
        // Bite timing
        // ------------------------------------------------------------------

        public static float Lead(bool assist) => assist ? AssistAnticipationLead : AnticipationLead;

        /// <summary>Rolls the wait, the nibbles and the bite for one cast.</summary>
        public static BitePlan PlanBite(FishingRandom random, FishingSpotSpec spot, WeatherType weather, bool assist)
        {
            float scale = spot != null ? spot.BiteDelayScale : 1f;
            if (EffectiveWeather(spot, weather) == WeatherType.Rain)
            {
                scale *= RainBiteDelayScale;
            }

            float lead = Lead(assist);
            float biteAt = Math.Max(lead + 0.5f, random.Range(MinBiteDelay, MaxBiteDelay) * scale);
            float trembleAt = biteAt - lead;

            int nibbles = random.Range(0, 3);
            var times = new List<float>(nibbles);
            float at = trembleAt;
            for (int i = 0; i < nibbles; i++)
            {
                at -= random.Range(0.7f, 1.4f);
                if (at < 0.6f)
                {
                    break;
                }

                times.Insert(0, at);
            }

            return new BitePlan(times, trembleAt, biteAt);
        }

        /// <summary>Seconds the float stays down: 0.9 s for a common fish down to 0.6 s for a legend; ×2 (at least 1.2 s) with assist.</summary>
        public static float HookWindowSeconds(FishRarity rarity, bool assist)
        {
            float window = 0.9f - 0.1f * (int)rarity;
            return assist ? Math.Max(AssistMinWindow, window * AssistWindowScale) : window;
        }

        /// <summary>
        /// Pressing at <paramref name="pressTime"/> (seconds since the float landed): hooked inside
        /// [bite − grace, bite + window], too early before, too late after.
        /// </summary>
        public static HookResult EvaluateHook(float pressTime, BitePlan plan, float window, bool assist)
        {
            float grace = assist ? AssistEarlyGrace : EarlyGrace;
            if (pressTime < plan.BiteAt - grace)
            {
                return HookResult.TooEarly;
            }

            return pressTime <= plan.BiteAt + window ? HookResult.Hooked : HookResult.TooLate;
        }

        // ------------------------------------------------------------------
        // Size
        // ------------------------------------------------------------------

        /// <summary>Weight of a landed fish, skewed toward small ones (a record really is rare). Grams, at least 1.</summary>
        public static int RollWeightGrams(FishSpec fish, double u1, double u2)
        {
            if (fish == null)
            {
                return 0;
            }

            double t = Math.Pow((Clamp01(u1) + Clamp01(u2)) * 0.5d, 1.6d);
            double kg = fish.MinKg + (fish.MaxKg - fish.MinKg) * t;
            return Math.Max(1, (int)Math.Round(kg * 1000d));
        }

        // ------------------------------------------------------------------
        // Tension physics
        // ------------------------------------------------------------------

        public static ReelParams ReelParamsFor(FishSpec fish, bool assist)
        {
            float strength = fish != null ? Clamp01f(fish.Strength) : 0.3f;
            float stamina = fish != null ? Math.Max(1f, fish.Stamina) : 3f;
            float surge = 0.2f + 0.45f * strength;
            return new ReelParams(
                reelTensionRate: assist ? 0.32f : 0.45f,
                relaxRate: assist ? 1f : 0.9f,
                basePull: 0.15f * strength * (assist ? 0.6f : 1f),
                surgePull: surge * (assist ? 0.6f : 1f),
                reelSpeed: 1f / stamina,
                swimBack: 0.03f + 0.05f * strength,
                snapGrace: assist ? 0.7f : 0.25f,
                slackGrace: assist ? 3f : 1.6f);
        }

        public static ReelState StartReel(FishingRandom random, FishSpec fish)
        {
            return new ReelState
            {
                Tension = StartTension,
                Progress = StartProgress,
                NextSurgeIn = NextSurgeDelay(random, fish),
                Outcome = ReelOutcome.Fighting,
            };
        }

        /// <summary>Advances a fight by <paramref name="deltaTime"/> (clamped to <see cref="MaxStep"/>) with the line held or released.</summary>
        public static ReelState StepReel(ReelState state, ReelParams tuning, FishSpec fish, bool reeling, float deltaTime, FishingRandom random)
        {
            if (state.Outcome != ReelOutcome.Fighting || deltaTime <= 0f)
            {
                return state;
            }

            float dt = Math.Min(deltaTime, MaxStep);

            if (state.Surging)
            {
                state.SurgeSecondsLeft -= dt;
                if (state.SurgeSecondsLeft <= 0f)
                {
                    state.Surging = false;
                    state.NextSurgeIn = NextSurgeDelay(random, fish);
                }
            }
            else
            {
                state.NextSurgeIn -= dt;
                if (state.NextSurgeIn <= 0f)
                {
                    state.Surging = true;
                    state.SurgeSecondsLeft = random.Range(0.45f, 0.9f);
                }
            }

            float pull = tuning.BasePull + (state.Surging ? tuning.SurgePull : 0f);
            if (reeling)
            {
                state.Tension += (tuning.ReelTensionRate + pull) * dt;
                state.Progress += tuning.ReelSpeed * (state.Surging ? 0.35f : 1f) * dt;
            }
            else
            {
                state.Tension += (pull - tuning.RelaxRate) * dt;
                if (state.Surging)
                {
                    state.Progress -= tuning.SwimBack * dt;
                }
            }

            state.Tension = Clamp01f(state.Tension);
            state.Progress = Clamp01f(state.Progress);

            state.OverTensionSeconds = state.Tension >= 1f ? state.OverTensionSeconds + dt : 0f;
            state.SlackSeconds = state.Tension <= SlackThreshold ? state.SlackSeconds + dt : 0f;

            if (state.Progress >= 1f)
            {
                state.Outcome = ReelOutcome.Landed;
            }
            else if (state.OverTensionSeconds >= tuning.SnapGrace)
            {
                state.Outcome = ReelOutcome.Snapped;
            }
            else if (state.SlackSeconds >= tuning.SlackGrace || state.Progress <= 0f)
            {
                state.Outcome = ReelOutcome.Escaped;
            }

            return state;
        }

        /// <summary>Seconds until the next surge: stronger fish surge more often.</summary>
        public static float NextSurgeDelay(FishingRandom random, FishSpec fish)
        {
            float strength = fish != null ? Clamp01f(fish.Strength) : 0.3f;
            return random.Range(1.5f, 3.5f) / (0.7f + 0.5f * strength);
        }

        private static double Clamp01(double value) => value < 0d ? 0d : (value > 1d ? 1d : value);

        private static float Clamp01f(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);
    }
}
