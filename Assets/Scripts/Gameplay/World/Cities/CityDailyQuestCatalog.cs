using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// The town dailies: <see cref="QuestType.Daily"/> quests that belong to one city's own mechanic and are handed out there (not by the
    /// random calendar pool of <see cref="DailyQuestService"/>, which can send the cousins anywhere). Built in code like
    /// <see cref="CityQuestCatalog"/> and found through it (quest log, dialogue, saves: <see cref="CityQuestCatalog.GetAllQuests"/>).
    /// The town's mechanic driver starts one each game day and resets it with <see cref="QuestManager.ResetDailyQuest"/>.
    /// Palafitas de Tarchon (33): old Tarchon asks the cousins to wake three dimmed neighbours with the mast's lantern
    /// (<see cref="TarchonDailyRules"/>).
    /// </summary>
    public static class CityDailyQuestCatalog
    {
        public const string TarchonWake = TarchonDailyRules.QuestId;

        private static readonly Dictionary<string, QuestDefinition> Built = new Dictionary<string, QuestDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static readonly IReadOnlyList<string> QuestIds = new[] { TarchonWake };

        public static bool IsTownDaily(string questId) => !string.IsNullOrEmpty(questId) && QuestIds.Contains(questId);

        /// <summary>The XP of the Tarchon daily: a third of a Tarchon city quest at the town's level, never under the authored number.</summary>
        public static int TarchonXp() => Mathf.Max(TarchonDailyRules.AuthoredXp,
            QuestXpRules.For(QuestXpKind.City, CityQuestCatalog.LevelOf(CityCatalog.PalafitasTarchon)) / 3);

        /// <summary>The built definition (cached), or null if <paramref name="questId"/> isn't a town daily.</summary>
        public static QuestDefinition GetQuest(string questId)
        {
            if (!IsTownDaily(questId))
            {
                return null;
            }

            if (Built.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = BuildTarchon(ItemDatabase.GetById);
            Built[questId] = definition;
            return definition;
        }

        public static IEnumerable<QuestDefinition> GetAllQuests() => QuestIds.Select(GetQuest);

        /// <summary>A fresh definition of the Tarchon daily; <paramref name="itemLookup"/> resolves the reward items.</summary>
        public static QuestDefinition BuildTarchon(System.Func<string, ItemDefinition> itemLookup)
        {
            string keyBase = RegionQuestCatalog.QuestKeyBase(TarchonWake);
            var definition = ScriptableObject.CreateInstance<QuestDefinition>();
            definition.name = TarchonWake;
            definition.hideFlags = HideFlags.DontUnloadUnusedAsset;
            var objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    ObjectiveId = TarchonDailyRules.ObjectiveId,
                    Type = QuestObjectiveType.CustomFlag,
                    TargetId = TarchonDailyRules.ObjectiveTarget,
                    RequiredAmount = TarchonDailyRules.Required,
                    DescriptionKey = keyBase + ".obj_" + TarchonDailyRules.ObjectiveId,
                },
            };

            var rewards = new List<QuestItemReward>
            {
                new QuestItemReward { Item = itemLookup?.Invoke("item_gold"), Quantity = TarchonDailyRules.RewardGold },
                new QuestItemReward { Item = itemLookup?.Invoke("item_health_potion"), Quantity = 2 },
            };

            definition.SetDataForTests(TarchonWake, keyBase + ".title", keyBase + ".description", QuestType.Daily, objectives, new List<string>(), TarchonXp(), rewards);
            return definition;
        }
    }

    /// <summary>Localization (en + pt-BR) of the town dailies (<see cref="CityDailyQuestCatalog"/>), merged by <see cref="CityQuestTexts.EnsureLoaded"/>.</summary>
    public static class CityDailyQuestTexts
    {
        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            new LocalizationEntry("quest.daily_tarchon_wake_neighbours.title", "Wake the Neighbours", "Despertar os Vizinhos"),
            new LocalizationEntry("quest.daily_tarchon_wake_neighbours.description",
                "Every night some trols of the stilts stare at the will-o'-wisps until they forget themselves again. Old Tarchon asked the cousins to light lanterns at the Mast of the Marsh Lights and wake three of them before the day is over.",
                "Toda noite alguns trols das palafitas olham para os fogos-fátuos até se esquecerem de novo. O velho Tarchon pediu aos primos que acendam lanternas no Mastro dos Fogos-Fátuos e acordem três deles antes do fim do dia."),
            new LocalizationEntry("quest.daily_tarchon_wake_neighbours.obj_wake_neighbours", "Wake dimmed neighbours with a lantern from the mast", "Acordar vizinhos apagados com uma lanterna do mastro"),
            new LocalizationEntry("quest.daily_tarchon_wake_neighbours.journal",
                "Grandma Edna always says a neighbour is someone you knock on twice. In Tarchon you knock with a lantern, every day, and every day someone opens.",
                "A Vó Edna sempre diz que vizinho é quem a gente bate na porta duas vezes. Em Tarchon se bate com uma lanterna, todo dia, e todo dia alguém abre."),
        };

        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }

        public static string GetFallback(string key) => All.FirstOrDefault(e => e.Key == key)?.PtBR;
    }
}
