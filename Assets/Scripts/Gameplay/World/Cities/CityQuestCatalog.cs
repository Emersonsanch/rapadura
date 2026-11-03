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
    /// Taa Pwani (map 25): Captain Liyongo sends the cousins after the thief gulls that tear the dhows' sails, then Zawadi
    /// asks them to free the mangrove roots from the giant crabs and pays with his own Botas de Raiz Viva (the gate of map 30).
    /// Tambor Rubro (map 11): Gurparanzah sends the cousins after the red scorpions of the badlands, then Kessi takes them
    /// hunting the ember hawks and pays with the Teriaga do Oásis (the gate of map 12).
    /// </summary>
    public static class CityQuestCatalog
    {
        public const string GarajonayMist = "quest_city_garajonay_dry_mist";
        public const string GarajonayMoss = "quest_city_garajonay_moss_tracks";
        public const string TaaPwaniGulls = "quest_city_taa_pwani_thief_gulls";
        public const string TaaPwaniClaws = "quest_city_taa_pwani_mangrove_claws";
        public const string TamborRubroScorpions = "quest_city_tambor_rubro_red_scorpions";
        public const string TamborRubroHawks = "quest_city_tambor_rubro_ember_hawks";

        public static readonly IReadOnlyList<CityQuestSpec> Quests = new List<CityQuestSpec>
        {
            // -- Copas de Garajonay: Gara, guardiã das copas, then Tinguaro, the tracker --
            new CityQuestSpec(CityCatalog.CopasGarajonay, "gara", new RegionQuestSpec(GarajonayMist, BiomeType.Forest, null, 320,
                new[] { new RegionObjectiveSpec("calm_mist_lizards", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 3), new RegionRewardSpec("item_mana_potion", 2) })),
            new CityQuestSpec(CityCatalog.CopasGarajonay, "tinguaro", new RegionQuestSpec(GarajonayMoss, BiomeType.Forest, GarajonayMist, 380,
                new[] { new RegionObjectiveSpec("follow_moss_boars", QuestObjectiveType.KillEnemyType, "enemy_forest_peccary", 5) },
                new[] { new RegionRewardSpec("item_hunting_bow", 1), new RegionRewardSpec("item_gold", 25) })),

            // -- Taa Pwani: Captain Liyongo, the nakhoda, then Zawadi, the root weaver. Map 25's own beasts are not in the game yet:
            // the ember vultures stand in for the Gaivota Ladra, the scorpions for the giant coconut crab (Docs/Mapas100.md 25). --
            new CityQuestSpec(CityCatalog.TaaPwani, "liyongo", new RegionQuestSpec(TaaPwaniGulls, BiomeType.Forest, null, 700,
                new[] { new RegionObjectiveSpec("drive_off_thief_gulls", QuestObjectiveType.KillEnemyType, "enemy_desert_ember_vulture", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_mana_potion", 3), new RegionRewardSpec("item_gold", 60) })),
            new CityQuestSpec(CityCatalog.TaaPwani, "zawadi", new RegionQuestSpec(TaaPwaniClaws, BiomeType.Forest, TaaPwaniGulls, 820,
                new[] { new RegionObjectiveSpec("cut_free_the_roots", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 5) },
                new[] { new RegionRewardSpec("item_botas_raiz", 1), new RegionRewardSpec("item_gold", 80) })),

            // -- Tambor Rubro: Gurparanzah, the archer chief, then Kessi, the hunter. The desert scorpions stand in for the
            // Escorpião-Rubro and the crested hawks for the Gavião de Brasa (Docs/Mapas100.md 11). --
            new CityQuestSpec(CityCatalog.TamborRubro, "gurparanzah", new RegionQuestSpec(TamborRubroScorpions, BiomeType.Desert, null, 460,
                new[] { new RegionObjectiveSpec("clear_red_scorpions", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_pedra_bencao", 2), new RegionRewardSpec("item_gold", 45) })),
            new CityQuestSpec(CityCatalog.TamborRubro, "kessi", new RegionQuestSpec(TamborRubroHawks, BiomeType.Desert, TamborRubroScorpions, 540,
                new[] { new RegionObjectiveSpec("hunt_ember_hawks", QuestObjectiveType.KillEnemyType, "enemy_mountain_crested_hawk", 5) },
                new[] { new RegionRewardSpec("item_teriaga", 1), new RegionRewardSpec("item_gold", 55) })),
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
