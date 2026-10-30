using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>The "Primeira Sela" mission just ended: the pony joined the cousins (the HUD plays the little scene).</summary>
    public readonly struct FirstSaddleCompletedEvent : IGameEvent
    {
        public readonly string MountGuid;

        public FirstSaddleCompletedEvent(string mountGuid)
        {
            MountGuid = mountGuid;
        }
    }

    /// <summary>
    /// "Primeira Sela" (Docs/Mounts.md §8): after the Lobo Alfa (the Floresta cleared), Vó Zefa takes the cousins to Seu
    /// Tonico's Haras and gives the five of them a pony; Lavine names him Pé-de-Pano. A side quest built in code (like
    /// <c>RegionQuestCatalog</c>) with one <see cref="QuestObjectiveType.CustomFlag"/> objective, "visit the Haras",
    /// set when the Haras shop opens (<see cref="HarasShopId"/>) or when the cousins stand in the Vila on the map
    /// (<see cref="ReportVisitedVillage"/>). Completing it grants the pony (<see cref="MountCollection.SourceQuest"/>); a save
    /// where the quest is done but the pony is missing gets it back on the next <see cref="Evaluate"/>.
    /// </summary>
    public static class FirstSaddleQuest
    {
        public const string QuestId = "quest_mount_first_saddle";
        public const string ObjectiveId = "visit_haras";
        public const string Flag = "mount_first_saddle_haras";
        public const string HarasShopId = "shop_haras_vila";
        public const BiomeType GiftRegion = BiomeType.Forest;

        private static QuestDefinition _definition;
        private static QuestManager _installedOn;
        private static MountCollection _collection;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Uninstall();
            _definition = null;
        }

        public static QuestDefinition Definition
        {
            get
            {
                if (_definition == null)
                {
                    _definition = Build();
                }

                return _definition;
            }
        }

        /// <summary>A fresh definition (uncached).</summary>
        public static QuestDefinition Build()
        {
            var definition = ScriptableObject.CreateInstance<QuestDefinition>();
            definition.name = QuestId;
            definition.hideFlags = HideFlags.DontUnloadUnusedAsset;
            definition.SetDataForTests(QuestId, MountTexts.QuestTitle, MountTexts.QuestDescription, QuestType.Side,
                new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveId = ObjectiveId,
                        Type = QuestObjectiveType.CustomFlag,
                        TargetId = Flag,
                        RequiredAmount = 1,
                        DescriptionKey = MountTexts.QuestObjective,
                    },
                },
                new List<string>(), 40, new List<QuestItemReward>());
            return definition;
        }

        /// <summary>Whether the quest should start now: Floresta cleared, not active, not done.</summary>
        public static bool ShouldStart(bool forestCleared, bool active, bool completed) => forestCleared && !active && !completed;

        /// <summary>Whether a finished quest still owes the pony (old save, or the grant was missed).</summary>
        public static bool OwesPony(bool completed, IEnumerable<OwnedMount> mounts)
        {
            return completed && (mounts == null || !mounts.Any(m => m.mountId == MountCatalog.Pony && m.source == MountCollection.SourceQuest));
        }

        /// <summary>Registers the definition with <paramref name="quests"/> and listens for the Haras, the boss and the completion.</summary>
        public static void Install(QuestManager quests, MountCollection collection)
        {
            if (quests == null || collection == null || (ReferenceEquals(quests, _installedOn) && ReferenceEquals(collection, _collection)))
            {
                return;
            }

            Uninstall();
            _installedOn = quests;
            _collection = collection;
            quests.RegisterDefinition(Definition);
            EventBus.Subscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
        }

        public static void Uninstall()
        {
            if (_installedOn == null)
            {
                return;
            }

            EventBus.Unsubscribe<ShopOpenedEvent>(OnShopOpened);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            _installedOn = null;
            _collection = null;
        }

        /// <summary>Starts the quest when due and repays a missing pony (call after loads and on map entry).</summary>
        public static void Evaluate(WorldMapProgress map)
        {
            QuestManager quests = _installedOn;
            if (quests == null || _collection == null)
            {
                return;
            }

            bool completed = quests.IsCompleted(QuestId);
            if (OwesPony(completed, _collection.Mounts))
            {
                GrantPony();
                return;
            }

            bool cleared = map != null && map.IsRegionCleared(GiftRegion);
            if (ShouldStart(cleared, quests.IsActive(QuestId), completed))
            {
                quests.StartQuest(Definition);
            }
        }

        /// <summary>The cousins are in the Vila da Vó Zefa on the map: counts as the visit to the Haras.</summary>
        public static void ReportVisitedVillage()
        {
            if (_installedOn != null && _installedOn.IsActive(QuestId))
            {
                _installedOn.SetCustomFlag(Flag);
            }
        }

        private static void OnShopOpened(ShopOpenedEvent evt)
        {
            if (evt.ShopId == HarasShopId)
            {
                ReportVisitedVillage();
            }
        }

        private static void OnRegionCleared(WorldMapRegionClearedEvent evt)
        {
            if (evt.Region == GiftRegion && _installedOn != null && Rapadura.Core.DI.ServiceLocator.TryGet(out WorldMapProgress map))
            {
                Evaluate(map);
            }
        }

        private static void OnQuestCompleted(QuestCompletedEvent evt)
        {
            if (evt.QuestId == QuestId)
            {
                GrantPony();
            }
        }

        private static void GrantPony()
        {
            OwnedMount pony = _collection?.Grant(MountCatalog.Pony, MountCollection.SourceQuest);
            if (pony == null)
            {
                return;
            }

            // The gift becomes the active mount: it is the one Vó Zefa just handed over.
            _collection.SetActive(pony.guid);
            EventBus.Publish(new FirstSaddleCompletedEvent(pony.guid));
        }
    }
}
