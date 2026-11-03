using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// A city side quest, handed out by a resident standing at their door in a walkable town (<c>CityResidentPost</c>):
    /// the same plain data as a <see cref="RegionQuestSpec"/> (built by the same <see cref="RegionQuestCatalog.Build"/>), plus
    /// the city and the giver. Texts: <c>quest.&lt;id without "quest_"&gt;.*</c> (title, description, objectives, journal) and
    /// the giver's <see cref="OfferKey"/> / <see cref="ActiveKey"/> lines, in <see cref="CityQuestTexts"/>.
    /// </summary>
    public sealed class CityQuestSpec
    {
        public readonly string CityId;
        public readonly string GiverResidentId;
        public readonly RegionQuestSpec Quest;

        public CityQuestSpec(string cityId, string giverResidentId, RegionQuestSpec quest)
        {
            CityId = cityId;
            GiverResidentId = giverResidentId;
            Quest = quest;
        }

        public string QuestId => Quest.QuestId;
        public string KeyBase => Quest.KeyBase;

        /// <summary>What the giver says when the quest starts (the cousins step up to them and take it).</summary>
        public string OfferKey => KeyBase + ".offer";

        /// <summary>What the giver says while the quest runs.</summary>
        public string ActiveKey => KeyBase + ".active";
    }

    /// <summary>What a quest-giving resident does when the cousins step up to them.</summary>
    public enum CityQuestTalk
    {
        /// <summary>Nothing to hand out (all done, or the previous quest of the chain is not done yet): their usual line.</summary>
        None = 0,
        /// <summary>Starts the quest and says <see cref="CityQuestSpec.OfferKey"/>.</summary>
        Offer = 1,
        /// <summary>The quest runs: says <see cref="CityQuestSpec.ActiveKey"/>.</summary>
        Active = 2,
    }

    /// <summary>
    /// The city side quests (content as code, like <see cref="RegionQuestCatalog"/>; kept apart from it because that one
    /// is "two chained quests and one giver per world-map region"). Same content rules as the region quests: kill targets
    /// are non-boss <c>enemy_*</c> assets of the city's own map, rewards are <c>Resources/Items</c> ids, every objective
    /// progresses in combat (the quest completes on its own and the reward lands, or waits in the pending queue).
    /// Copas de Garajonay (map 05, Docs/Mapas100.md): Gara asks the cousins to calm the lizards of the dry mist (the wolves
    /// of the forest floor stand in for the Lagarto-Gigante do Louro), then Tinguaro sends them after the moss boars.
    /// </summary>
    public static class CityQuestCatalog
    {
        public const string GarajonayMist = "quest_city_garajonay_dry_mist";
        public const string GarajonayMoss = "quest_city_garajonay_moss_tracks";

        public static readonly IReadOnlyList<CityQuestSpec> Quests = new List<CityQuestSpec>
        {
            // -- Copas de Garajonay: Gara, guardiã das copas, then Tinguaro, the tracker --
            new CityQuestSpec(CityCatalog.CopasGarajonay, "gara", new RegionQuestSpec(GarajonayMist, BiomeType.Forest, null, 320,
                new[] { new RegionObjectiveSpec("calm_mist_lizards", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 3), new RegionRewardSpec("item_mana_potion", 2) })),
            new CityQuestSpec(CityCatalog.CopasGarajonay, "tinguaro", new RegionQuestSpec(GarajonayMoss, BiomeType.Forest, GarajonayMist, 380,
                new[] { new RegionObjectiveSpec("follow_moss_boars", QuestObjectiveType.KillEnemyType, "enemy_forest_peccary", 5) },
                new[] { new RegionRewardSpec("item_hunting_bow", 1), new RegionRewardSpec("item_gold", 25) })),
        };

        private static readonly Dictionary<string, QuestDefinition> Built = new Dictionary<string, QuestDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static CityQuestSpec FindSpec(string questId) =>
            string.IsNullOrEmpty(questId) ? null : Quests.FirstOrDefault(q => q.QuestId == questId);

        public static bool IsCityQuest(string questId) => FindSpec(questId) != null;

        /// <summary>The quests <paramref name="residentId"/> hands out, in chain order.</summary>
        public static IEnumerable<CityQuestSpec> QuestsOf(string residentId) => Quests.Where(q => q.GiverResidentId == residentId);

        public static IEnumerable<CityQuestSpec> QuestsIn(string cityId) => Quests.Where(q => q.CityId == cityId);

        /// <summary>
        /// What <paramref name="residentId"/> does now (pure: the caller passes the quest state): the first of their quests
        /// that is running → <see cref="CityQuestTalk.Active"/>; else the first not done whose prerequisite is done →
        /// <see cref="CityQuestTalk.Offer"/>; else <see cref="CityQuestTalk.None"/>.
        /// </summary>
        public static CityQuestTalk Decide(string residentId, Func<string, bool> isActive, Func<string, bool> isCompleted, out CityQuestSpec quest)
        {
            quest = null;
            if (string.IsNullOrEmpty(residentId) || isActive == null || isCompleted == null)
            {
                return CityQuestTalk.None;
            }

            foreach (CityQuestSpec spec in QuestsOf(residentId))
            {
                if (isActive(spec.QuestId))
                {
                    quest = spec;
                    return CityQuestTalk.Active;
                }

                if (isCompleted(spec.QuestId))
                {
                    continue;
                }

                string prerequisite = spec.Quest.PrerequisiteQuestId;
                if (string.IsNullOrEmpty(prerequisite) || isCompleted(prerequisite))
                {
                    quest = spec;
                    return CityQuestTalk.Offer;
                }

                return CityQuestTalk.None;
            }

            return CityQuestTalk.None;
        }

        /// <summary>
        /// The cousins step up to <paramref name="residentId"/>: starts the offered quest on <paramref name="quests"/> and
        /// returns the line key to say (offer or progress), or null for the resident's usual line.
        /// </summary>
        public static string Talk(QuestManager quests, string residentId)
        {
            if (quests == null)
            {
                return null;
            }

            switch (Decide(residentId, quests.IsActive, quests.IsCompleted, out CityQuestSpec spec))
            {
                case CityQuestTalk.Active:
                    return spec.ActiveKey;
                case CityQuestTalk.Offer:
                    return quests.StartQuest(GetQuest(spec.QuestId)) ? spec.OfferKey : null;
                default:
                    return null;
            }
        }

        /// <summary>The built definition of a city quest (cached), or null if <paramref name="questId"/> isn't one.</summary>
        public static QuestDefinition GetQuest(string questId)
        {
            CityQuestSpec spec = FindSpec(questId);
            if (spec == null)
            {
                return null;
            }

            if (Built.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = RegionQuestCatalog.Build(spec.Quest, ItemDatabase.GetById);
            Built[questId] = definition;
            return definition;
        }

        public static IEnumerable<QuestDefinition> GetAllQuests() => Quests.Select(q => GetQuest(q.QuestId));

        /// <summary>Registers every city quest with <paramref name="quests"/> (a quest restored from a save needs its rewards).</summary>
        public static void RegisterAll(QuestManager quests)
        {
            if (quests == null)
            {
                return;
            }

            foreach (QuestDefinition definition in GetAllQuests())
            {
                quests.RegisterDefinition(definition);
            }
        }
    }
}
