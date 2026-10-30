using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Chatter
{
    /// <summary>Tuning of the road conversations (defaults are what the game uses).</summary>
    public sealed class RoadChatterSettings
    {
        /// <summary>Chance that an eligible trip gets a conversation.</summary>
        public float ChancePerTrip = 0.5f;

        /// <summary>Trips skipped after a conversation (the cooldown).</summary>
        public int CooldownTrips = 2;

        /// <summary>Trip progress (0..1) at which the bubble pops: early, well before road events (0.3+).</summary>
        public float TriggerProgress = 0.08f;

        /// <summary>Real-time seconds the bubble stays up.</summary>
        public float ShowSeconds = 7f;
    }

    /// <summary>
    /// Pure rules of "Conversas entre primos na estrada" (unit-tested): the campaign stage from the story beats seen,
    /// when a bubble may appear, which exchange plays (progress gating, unheard ones first, the soonest-to-expire first; then repeats, oldest-heard first) and
    /// the per-trip roll with its cooldown, kept in <see cref="RoadChatterState"/>.
    /// </summary>
    public static class RoadChatterRules
    {
        /// <summary>Stage from the interludes seen (a beat seen = its reveal is known to every cousin).</summary>
        public static RoadChatterStage StageFrom(bool forestInterludeSeen, bool desertInterludeSeen, bool mountainInterludeSeen, bool endingSeen)
        {
            if (endingSeen) return RoadChatterStage.Home;
            if (mountainInterludeSeen) return RoadChatterStage.Caves;
            if (desertInterludeSeen) return RoadChatterStage.Mountain;
            if (forestInterludeSeen) return RoadChatterStage.Desert;
            return RoadChatterStage.Forest;
        }

        /// <summary>
        /// A bubble may pop: the token is travelling with a cousin chosen, the prologue was seen and no story beat waits,
        /// the map HUD is up, nothing pauses the game (road event card, story panel, menus), no conversation and no road
        /// event card is open.
        /// </summary>
        public static bool CanShow(bool traveling, bool hasHero, bool prologueSeen, bool storyPending, bool hudVisible,
            bool paused, bool dialogueActive, bool eventCardOpen)
        {
            return traveling && hasHero && prologueSeen && !storyPending && hudVisible && !paused && !dialogueActive && !eventCardOpen;
        }

        /// <summary>Whether <paramref name="spec"/> can play for <paramref name="active"/> at this point of the campaign.</summary>
        public static bool IsEligible(RoadChatterSpec spec, CharacterId active, RoadChatterStage stage, int cycle)
        {
            return spec != null && spec.PlayableBy(active) && stage >= spec.MinStage && stage <= spec.MaxStage && cycle >= spec.MinCycle;
        }

        /// <summary>
        /// The exchange to play, or null: eligible and never seen; among those, the ones whose window closes soonest
        /// (lowest <see cref="RoadChatterSpec.MaxStage"/>) first, so stage-specific lines are heard before they expire;
        /// ties broken by <paramref name="random"/> (or catalog order without one).
        /// </summary>
        public static RoadChatterSpec Pick(IReadOnlyList<RoadChatterSpec> catalog, CharacterId? active, RoadChatterStage stage, int cycle,
            Func<string, bool> seen, Random random)
        {
            return Pick(catalog, active, stage, cycle, seen, random, null);
        }

        /// <summary>
        /// Like <see cref="Pick(IReadOnlyList{RoadChatterSpec}, CharacterId?, RoadChatterStage, int, Func{string, bool}, Random)"/>,
        /// but once every eligible exchange was heard it repeats them: the oldest-heard first (lowest
        /// <paramref name="heardRank"/>; -1 = no record, oldest of all; ties in catalog order). Without a rank, no repeats.
        /// The cooldown is the same (<see cref="MarkShown"/>).
        /// </summary>
        public static RoadChatterSpec Pick(IReadOnlyList<RoadChatterSpec> catalog, CharacterId? active, RoadChatterStage stage, int cycle,
            Func<string, bool> seen, Random random, Func<string, int> heardRank)
        {
            RoadChatterSpec fresh = PickUnheard(catalog, active, stage, cycle, seen, random);
            if (fresh != null || heardRank == null || catalog == null || !active.HasValue)
            {
                return fresh;
            }

            RoadChatterSpec oldest = null;
            int oldestRank = int.MaxValue;
            foreach (RoadChatterSpec spec in catalog)
            {
                if (!IsEligible(spec, active.Value, stage, cycle))
                {
                    continue;
                }

                int rank = heardRank(spec.Id);
                if (oldest == null || rank < oldestRank)
                {
                    oldest = spec;
                    oldestRank = rank;
                }
            }

            return oldest;
        }

        private static RoadChatterSpec PickUnheard(IReadOnlyList<RoadChatterSpec> catalog, CharacterId? active, RoadChatterStage stage, int cycle,
            Func<string, bool> seen, Random random)
        {
            if (catalog == null || !active.HasValue)
            {
                return null;
            }

            var best = new List<RoadChatterSpec>();
            RoadChatterStage bestMax = RoadChatterStage.Home;
            foreach (RoadChatterSpec spec in catalog)
            {
                if (!IsEligible(spec, active.Value, stage, cycle) || (seen != null && seen(spec.Id)))
                {
                    continue;
                }

                if (best.Count == 0 || spec.MaxStage < bestMax)
                {
                    best.Clear();
                    bestMax = spec.MaxStage;
                }

                if (spec.MaxStage == bestMax)
                {
                    best.Add(spec);
                }
            }

            if (best.Count == 0)
            {
                return null;
            }

            return best[random != null ? random.Next(best.Count) : 0];
        }

        /// <summary>
        /// Rolls a new trip: false while the cooldown runs (spending one trip of it) or when the trip isn't allowed;
        /// otherwise <see cref="RoadChatterSettings.ChancePerTrip"/>.
        /// </summary>
        public static bool RollTrip(RoadChatterState state, RoadChatterSettings settings, bool allowed, Random random)
        {
            if (state == null || settings == null)
            {
                return false;
            }

            if (state.TripsUntilEligible > 0)
            {
                state.TripsUntilEligible--;
                return false;
            }

            if (!allowed)
            {
                return false;
            }

            double roll = random != null ? random.NextDouble() : 0d;
            return roll < settings.ChancePerTrip;
        }

        /// <summary>Records that <paramref name="spec"/> was shown (now the most recently heard), and the cooldown starts.</summary>
        public static void MarkShown(RoadChatterState state, RoadChatterSpec spec, RoadChatterSettings settings)
        {
            if (state == null || spec == null)
            {
                return;
            }

            state.MarkSeen(spec.Id);
            state.TripsUntilEligible = settings != null ? Math.Max(0, settings.CooldownTrips) : 0;
        }
    }
}
