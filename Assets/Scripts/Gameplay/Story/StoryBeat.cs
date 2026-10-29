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
