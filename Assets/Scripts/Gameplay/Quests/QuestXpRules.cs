using System;
using System.Linq;
using Rapadura.Gameplay.Player;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>Which rule a quest's XP follows (<see cref="QuestXpRules"/>).</summary>
    public enum QuestXpKind
    {
        /// <summary>The authored number as it is: the campaign (Atos I–V, region quests), personal and daily quests.</summary>
        Authored = 0,

        /// <summary>A step of "A Lua que Não Enche" (<see cref="MidStoryCatalog"/>).</summary>
        MidStory = 1,

        /// <summary>One of "As Cartas da Estrada" (<see cref="LateStoryCatalog"/>).</summary>
        RoadLetter = 2,

        /// <summary>A city quest (<c>CityQuestCatalog</c>), at the walk level of its city's map.</summary>
        City = 3,
    }

    /// <summary>
    /// Quest XP that follows the level (Docs/Balanco100a500.md §5.3, sugestão 2): above level <see cref="ScaledAboveLevel"/> (50)
    /// a middle-story step is worth <see cref="MidStoryShare"/> of the XP to pass the level it opens at, a letter
    /// <see cref="RoadLetterShare"/> and a city quest <see cref="CityShare"/> (the shipped curve,
    /// <see cref="LevelCurveMath.ShippedExperienceToNext"/>, the one PlayerStats and EnemyLevelScaling follow), never less than
    /// the authored number. Without it quests paid 0% of the XP of every band above 100 (the doc asks 20–30%).
    /// Pure C#: the server computes the same reward.
    /// </summary>
    public static class QuestXpRules
    {
        public const double MidStoryShare = 0.5;
        public const double RoadLetterShare = 1.5;
        public const double CityShare = 0.25;

        /// <summary>Quests up to this level keep their authored XP (the campaign curve, levels 1–50).</summary>
        public const int ScaledAboveLevel = 50;

        /// <summary>The share of the level's XP <paramref name="kind"/> is worth at <paramref name="level"/> (0 = authored only).</summary>
        public static double ShareOf(QuestXpKind kind, int level)
        {
            if (level <= ScaledAboveLevel)
            {
                return 0.0;
            }

            switch (kind)
            {
                case QuestXpKind.MidStory: return MidStoryShare;
                case QuestXpKind.RoadLetter: return RoadLetterShare;
                case QuestXpKind.City: return CityShare;
                default: return 0.0;
            }
        }

        /// <summary>The level-scaled part alone: round(share × XP to pass <paramref name="level"/>), 0 when the kind doesn't scale there.</summary>
        public static int For(QuestXpKind kind, int level)
        {
            double share = ShareOf(kind, level);
            return share <= 0.0 ? 0 : (int)Math.Floor(share * LevelCurveMath.ShippedExperienceToNext(level) + 0.5);
        }

        /// <summary>The XP the quest pays: the larger of <paramref name="authoredXp"/> and <see cref="For(QuestXpKind,int)"/>.</summary>
        public static int For(QuestXpKind kind, int level, int authoredXp) => Math.Max(Math.Max(0, authoredXp), For(kind, level));

        /// <summary><paramref name="spec"/> with its XP set by the rule (the same spec when nothing changes).</summary>
        public static RegionQuestSpec Apply(RegionQuestSpec spec, QuestXpKind kind, int level)
        {
            if (spec == null)
            {
                return null;
            }

            int xp = For(kind, level, spec.RewardExperience);
            return xp == spec.RewardExperience
                ? spec
                : new RegionQuestSpec(spec.QuestId, spec.Region, spec.PrerequisiteQuestId, xp, spec.Objectives.ToArray(), spec.Rewards.ToArray());
        }
    }
}
