using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>Tunables of <see cref="WorldEventDirector"/> (values documented in Docs/Balance/README.md).</summary>
    public sealed class WorldEventSettings
    {
        public const float DefaultBaseChancePerTrip = 0.25f;
        public const float DefaultChanceRampPerTrip = 0.10f;
        public const float DefaultMaxChancePerTrip = 0.50f;
        public const int DefaultCooldownTrips = 2;
        public const float DefaultMinTriggerProgress = 0.3f;
        public const float DefaultMaxTriggerProgress = 0.7f;
        public const int DefaultMinVictories = 1;

        /// <summary>Chance that an eligible trip (off cooldown) has an event.</summary>
        public float BaseChancePerTrip = DefaultBaseChancePerTrip;

        /// <summary>Added per eligible trip in a row without one, so droughts end.</summary>
        public float ChanceRampPerTrip = DefaultChanceRampPerTrip;

        public float MaxChancePerTrip = DefaultMaxChancePerTrip;

        /// <summary>Trips after an event during which no event can happen.</summary>
        public int CooldownTrips = DefaultCooldownTrips;

        /// <summary>The card pops somewhere in this stretch of the trip (0 = start, 1 = arrival).</summary>
        public float MinTriggerProgress = DefaultMinTriggerProgress;
        public float MaxTriggerProgress = DefaultMaxTriggerProgress;

        /// <summary>Encounters won before any event (the first fight is the tutorial).</summary>
        public int MinVictories = DefaultMinVictories;
    }

    /// <summary>
    /// Pure rules of the world map road events (no Unity objects, EditMode-tested by <c>WorldEventTests</c>):
    /// when events may happen at all, which <see cref="WorldEventSpec"/> fits a region/time of day, their weights,
    /// the no-repeat pick and how rewards scale with the region.
    /// </summary>
    public static class WorldEventRules
    {
        /// <summary>
        /// Events are off during the prologue/tutorial: the prologue must be seen, no story beat may be waiting
        /// (its panel is about to show), the first fight must be won, and the map must be in play (HUD up, no
        /// modal screen, no conversation).
        /// </summary>
        public static bool CanStartEvents(bool prologueSeen, bool storyBeatPending, int totalVictories, int minVictories,
            bool hudVisible, bool paused, bool dialogueActive)
        {
            return prologueSeen && !storyBeatPending && totalVictories >= minVictories && hudVisible && !paused && !dialogueActive;
        }

        public static bool IsEligible(WorldEventSpec spec, BiomeType region, TimeOfDayPhase phase)
        {
            if (spec == null || spec.Weight <= 0)
            {
                return false;
            }

            if (spec.Regions.Count > 0 && !Contains(spec.Regions, region))
            {
                return false;
            }

            return spec.Phases.Count == 0 || Contains(spec.Phases, phase);
        }

        /// <summary>Selection weight in this region/time (0 = not eligible). Night applies <see cref="WorldEventSpec.NightWeightMultiplier"/>.</summary>
        public static float GetWeight(WorldEventSpec spec, BiomeType region, TimeOfDayPhase phase)
        {
            if (!IsEligible(spec, region, phase))
            {
                return 0f;
            }

            float weight = spec.Weight;
            if (phase == TimeOfDayPhase.Night)
            {
                weight *= Mathf.Max(0f, spec.NightWeightMultiplier);
            }

            return weight;
        }

        /// <summary>
        /// Weighted pick among the events eligible here, skipping <paramref name="lastEventId"/> (no repeats in a
        /// row) unless it is the only one. Null when nothing fits. Uses exactly one <c>NextDouble</c> when it picks.
        /// </summary>
        public static WorldEventSpec Pick(IReadOnlyList<WorldEventSpec> events, BiomeType region, TimeOfDayPhase phase,
            string lastEventId, System.Random random)
        {
            if (events == null || random == null)
            {
                return null;
            }

            var candidates = new List<(WorldEventSpec Spec, float Weight)>();
            WorldEventSpec repeat = null;
            float total = 0f;

            foreach (WorldEventSpec spec in events)
            {
                float weight = GetWeight(spec, region, phase);
                if (weight <= 0f)
                {
                    continue;
                }

                if (lastEventId != null && spec.Id == lastEventId)
                {
                    repeat = spec;
                    continue;
                }

                candidates.Add((spec, weight));
                total += weight;
            }

            if (candidates.Count == 0)
            {
                return repeat;
            }

            double roll = random.NextDouble() * total;
            foreach ((WorldEventSpec spec, float weight) in candidates)
            {
                roll -= weight;
                if (roll < 0d)
                {
                    return spec;
                }
            }

            return candidates[candidates.Count - 1].Spec;
        }

        /// <summary>
        /// XP rewards × this: Floresta 1, Deserto 1.8, Montanha 2.8, Cavernas 4 — the regions' enemy XP (10/18/28/40,
        /// Docs/Balance/enemies.csv), so an event is worth the same number of fights everywhere. Gold is not scaled:
        /// enemies drop the same 1–3 gold in every region.
        /// </summary>
        public static float RegionRewardMultiplier(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return 1.8f;
                case BiomeType.Mountain: return 2.8f;
                case BiomeType.Caves: return 4f;
                default: return 1f;
            }
        }

        /// <summary>The amount <paramref name="effect"/> really grants/costs in <paramref name="region"/>.</summary>
        public static int ScaledAmount(WorldEventEffect effect, BiomeType region)
        {
            if (effect == null)
            {
                return 0;
            }

            return effect.ScalesWithRegion ? Mathf.RoundToInt(effect.Amount * RegionRewardMultiplier(region)) : effect.Amount;
        }

        /// <summary>Whether a bag holding <paramref name="countHeld"/> of each item can pay every cost of <paramref name="choice"/>.</summary>
        public static bool CanAfford(WorldEventChoiceSpec choice, Func<string, int> countHeld)
        {
            if (choice == null)
            {
                return false;
            }

            foreach (WorldEventEffect cost in choice.Costs)
            {
                if (countHeld == null || countHeld(cost.ItemId) < cost.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Contains<T>(IReadOnlyList<T> list, T value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], value))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Decides <i>when</i> a road event happens: once per trip between regions (WorldMap's trips are short, about
    /// ten encounter steps, so a per-step roll would mostly land on the last metres). Off cooldown, each trip rolls
    /// base + ramp × (eligible trips without an event), capped; a hit gives the point of the trip
    /// (<see cref="WorldEventSettings.MinTriggerProgress"/>..<see cref="WorldEventSettings.MaxTriggerProgress"/>) where
    /// the card pops. Only an event that actually popped (<see cref="MarkTriggered"/>) starts the cooldown: a trip cut
    /// short by a random encounter first keeps its luck. Counters live in the saved <see cref="WorldEventState"/>;
    /// all randomness goes through one injected <see cref="System.Random"/>, so a seed replays the same sequence.
    /// </summary>
    public sealed class WorldEventDirector
    {
        private readonly WorldEventSettings _settings;
        private readonly WorldEventState _state;
        private readonly System.Random _random;

        public WorldEventDirector(WorldEventSettings settings, WorldEventState state, System.Random random)
        {
            _settings = settings ?? new WorldEventSettings();
            _state = state ?? new WorldEventState();
            _random = random ?? new System.Random();
        }

        public WorldEventSettings Settings => _settings;
        public WorldEventState State => _state;
        public System.Random Random => _random;

        /// <summary>Chance of the next eligible trip (0 while on cooldown).</summary>
        public float ChanceForNextTrip
        {
            get
            {
                if (_state.TripsUntilEligible > 0)
                {
                    return 0f;
                }

                float chance = _settings.BaseChancePerTrip + _settings.ChanceRampPerTrip * _state.TripsWithoutEvent;
                return Mathf.Clamp01(Mathf.Min(_settings.MaxChancePerTrip, chance));
            }
        }

        /// <summary>
        /// Called when a trip starts. Returns the trip progress (0..1) at which an event pops, or null for none.
        /// A trip while events are not allowed (<see cref="WorldEventRules.CanStartEvents"/>) changes nothing;
        /// a trip on cooldown only counts the cooldown down.
        /// </summary>
        public float? RollTrip(bool eventsAllowed)
        {
            if (!eventsAllowed)
            {
                return null;
            }

            if (_state.TripsUntilEligible > 0)
            {
                _state.TripsUntilEligible--;
                return null;
            }

            float chance = ChanceForNextTrip;
            if (chance <= 0f || _random.NextDouble() >= chance)
            {
                _state.TripsWithoutEvent++;
                return null;
            }

            float min = Mathf.Clamp01(Mathf.Min(_settings.MinTriggerProgress, _settings.MaxTriggerProgress));
            float max = Mathf.Clamp01(Mathf.Max(_settings.MinTriggerProgress, _settings.MaxTriggerProgress));
            return min + (float)_random.NextDouble() * (max - min);
        }

        /// <summary>Which event pops here and now (null when none fits: then nothing pops and no cooldown starts).</summary>
        public WorldEventSpec PickEvent(BiomeType region, TimeOfDayPhase phase, IReadOnlyList<WorldEventSpec> events = null)
        {
            return WorldEventRules.Pick(events ?? WorldEventCatalog.Events, region, phase, _state.LastEventId, _random);
        }

        /// <summary>The card for <paramref name="spec"/> is on screen: cooldown starts, pity resets, the event is remembered.</summary>
        public void MarkTriggered(WorldEventSpec spec)
        {
            if (spec == null)
            {
                return;
            }

            _state.TripsUntilEligible = Mathf.Max(0, _settings.CooldownTrips);
            _state.TripsWithoutEvent = 0;
            _state.RecordSeen(spec.Id);
        }
    }
}
