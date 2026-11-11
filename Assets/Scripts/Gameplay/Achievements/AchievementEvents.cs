using Rapadura.Core.Events;

namespace Rapadura.Gameplay.Achievements
{
    /// <summary>Raised once when an achievement unlocks (the toast and achievements list listen to it).</summary>
    public readonly struct AchievementUnlockedEvent : IGameEvent
    {
        public readonly AchievementDefinition Achievement;

        public AchievementUnlockedEvent(AchievementDefinition achievement)
        {
            Achievement = achievement;
        }
    }

    /// <summary>A region mechanic was mastered once (the drivers of <c>World/Map/Mechanics</c> publish it, the manager counts distinct ids).</summary>
    public readonly struct RegionFeatEvent : IGameEvent
    {
        public readonly string FeatId;

        public RegionFeatEvent(string featId)
        {
            FeatId = featId;
        }
    }

    /// <summary>The feat ids <see cref="RegionFeatEvent"/> carries.</summary>
    public static class RegionFeats
    {
        public const string NartesRaceGold = "nartes_race_gold";
        public const string NartesChainTop = "nartes_chain_top";
        public const string CeuPorchStory = "ceu_porch_story";
        public const string CeuSkyWhole = "ceu_sky_whole";
        public const string NartesSkyGoldPrefix = "nartes_sky_gold_";

        /// <summary>The Golds of Heaven are four objects, one feat each (index in <c>OverworldLayouts.NartesSkyGolds</c>).</summary>
        public static string NartesSkyGold(int index) => NartesSkyGoldPrefix + index;

        public static bool IsKnown(string featId, int skyGolds)
        {
            if (featId == NartesRaceGold || featId == NartesChainTop || featId == CeuPorchStory || featId == CeuSkyWhole)
            {
                return true;
            }

            return featId != null && featId.StartsWith(NartesSkyGoldPrefix, System.StringComparison.Ordinal)
                && int.TryParse(featId.Substring(NartesSkyGoldPrefix.Length), out int index) && index >= 0 && index < skyGolds;
        }
    }
}
