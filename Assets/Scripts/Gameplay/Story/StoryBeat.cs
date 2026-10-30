using System;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// The campaign's narrative beats, in the order they play: the prologue on a new game's first
    /// world map, one interlude after each region boss on the way (Floresta, Deserto, Montanha) and
    /// the ending after the Cavernas boss. Persisted by <see cref="Id"/> (a stable string), never by
    /// the numeric value, so the enum can be reordered/extended without breaking saves.
    /// </summary>
    public enum StoryBeat
    {
        None = -1,
        Prologue = 0,
        InterludeForest = 1,
        InterludeDesert = 2,
        InterludeMountain = 3,
        Ending = 4,
    }

    /// <summary>Stable ids and region mapping for <see cref="StoryBeat"/>.</summary>
    public static class StoryBeats
    {
        /// <summary>The region whose boss ends the campaign (its "interlude" is the <see cref="StoryBeat.Ending"/>).</summary>
        public const BiomeType FinalRegion = BiomeType.Caves;

        /// <summary>Every playable beat, in play order (<see cref="StoryBeat.None"/> excluded).</summary>
        public static readonly StoryBeat[] InOrder =
        {
            StoryBeat.Prologue,
            StoryBeat.InterludeForest,
            StoryBeat.InterludeDesert,
            StoryBeat.InterludeMountain,
            StoryBeat.Ending,
        };

        /// <summary>Save/localization id: <c>prologue</c>, <c>interlude_forest</c>, ..., <c>ending</c>; null for None.</summary>
        public static string Id(StoryBeat beat)
        {
            switch (beat)
            {
                case StoryBeat.Prologue: return "prologue";
                case StoryBeat.InterludeForest: return "interlude_forest";
                case StoryBeat.InterludeDesert: return "interlude_desert";
                case StoryBeat.InterludeMountain: return "interlude_mountain";
                case StoryBeat.Ending: return "ending";
                default: return null;
            }
        }

        /// <summary>Inverse of <see cref="Id"/>; unknown ids (a beat removed in a later version) give None.</summary>
        public static StoryBeat FromId(string id)
        {
            foreach (StoryBeat beat in InOrder)
            {
                if (Id(beat) == id)
                {
                    return beat;
                }
            }

            return StoryBeat.None;
        }

        /// <summary>The beat that plays once <paramref name="region"/>'s boss is beaten (the ending for the final region).</summary>
        public static StoryBeat AfterRegionCleared(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Forest: return StoryBeat.InterludeForest;
                case BiomeType.Desert: return StoryBeat.InterludeDesert;
                case BiomeType.Mountain: return StoryBeat.InterludeMountain;
                case FinalRegion: return StoryBeat.Ending;
                default: return StoryBeat.None;
            }
        }

        /// <summary>
        /// Parses a dialogue key: a <see cref="Id"/> ("interlude_desert") or the enum name ("InterludeDesert"),
        /// case-insensitive. False (and None) for empty/unknown keys.
        /// </summary>
        public static bool TryParse(string key, out StoryBeat beat)
        {
            beat = StoryBeat.None;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            foreach (StoryBeat candidate in InOrder)
            {
                if (string.Equals(Id(candidate), key, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(candidate.ToString(), key, StringComparison.OrdinalIgnoreCase))
                {
                    beat = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>The last beat seen in play order (<see cref="StoryBeat.None"/> when none / no lookup).</summary>
        public static StoryBeat LatestSeen(Func<StoryBeat, bool> seen)
        {
            if (seen == null)
            {
                return StoryBeat.None;
            }

            for (int i = InOrder.Length - 1; i >= 0; i--)
            {
                if (seen(InOrder[i]))
                {
                    return InOrder[i];
                }
            }

            return StoryBeat.None;
        }

        /// <summary>
        /// The region the cousins last came back from: the region of the latest interlude (or the ending) seen; null
        /// before the Forest interlude. Used by Vó Edna's kitchen and Ari's (Caramelo) cuddles (Docs/Narrative.md §9).
        /// </summary>
        public static BiomeType? LastRegionCleared(Func<StoryBeat, bool> seen) => RegionFor(LatestSeen(seen));

        /// <summary>The region whose clear unlocks <paramref name="beat"/> (null for the prologue/None).</summary>
        public static BiomeType? RegionFor(StoryBeat beat)
        {
            switch (beat)
            {
                case StoryBeat.InterludeForest: return BiomeType.Forest;
                case StoryBeat.InterludeDesert: return BiomeType.Desert;
                case StoryBeat.InterludeMountain: return BiomeType.Mountain;
                case StoryBeat.Ending: return FinalRegion;
                default: return null;
            }
        }
    }
}
