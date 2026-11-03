using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// "Asas da Coluna" (Docs/Mounts.md §3.8): how the cousins get the first flying mount, the Grifo da Coluna da Glória
    /// (<see cref="MountSkyCatalog.ColumnGriffin"/>). When a cousin reaches level <see cref="StartLevel"/> (the Grutas do
    /// Limiar, just before the Ninho de Mahaway, map 24, levels 42–50, which needs wings or a flyer), Srosh of the
    /// Estrebaria hears a griffin of light fallen from the Coluna da Glória. Each critter beaten is a freed spark that
    /// climbs the Coluna; <see cref="SparksRequired"/> of them light the griffin again and it comes down to the cousins.
    /// <para>A side quest built in code like <see cref="FirstSaddleQuest"/>: one <see cref="QuestObjectiveType.KillEnemyType"/>
    /// objective with an empty target (any kill counts, the QuestManager counts it by itself). The mount is granted by
    /// <see cref="MountCollection.ReportHook"/> through the <see cref="MountSourceHooks.QuestAsasDaColuna"/> hook when the
    /// quest completes; a save where the quest is done and the griffin is missing gets it back on the next
    /// <see cref="Evaluate"/>. Pure decisions in <see cref="ShouldStart"/>/<see cref="OwesGriffin"/> (the server reuses them).</para>
    /// </summary>
    public static class ColumnGriffinQuest
    {
        public const string QuestId = MountSourceHooks.QuestAsasDaColuna;
        public const string ObjectiveId = "free_sparks";

        /// <summary>Hero level that opens the quest (map 23 Cânion da Roda is 38–46, map 24 starts at 42).</summary>
        public const int StartLevel = 40;

        /// <summary>Critters to beat (sparks to free) once the quest is open.</summary>
        public const int SparksRequired = 40;

        public const int RewardExperience = 600;

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
            definition.SetDataForTests(QuestId, MountSkyTexts.QuestTitle, MountSkyTexts.QuestDescription, QuestType.Side,
                new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        ObjectiveId = ObjectiveId,
                        Type = QuestObjectiveType.KillEnemyType,
                        TargetId = string.Empty,
                        RequiredAmount = SparksRequired,
                        DescriptionKey = MountSkyTexts.QuestObjective,
                    },
                },
                new List<string>(), RewardExperience, new List<QuestItemReward>());
            return definition;
        }

        /// <summary>Whether the quest should start now: the hero is level <see cref="StartLevel"/>+, not active, not done, griffin not owned.</summary>
        public static bool ShouldStart(int heroLevel, bool active, bool completed, bool ownsGriffin)
        {
            return heroLevel >= StartLevel && !active && !completed && !ownsGriffin;
        }

        /// <summary>Whether a finished quest still owes the griffin (old save, or the grant was missed).</summary>
        public static bool OwesGriffin(bool completed, IEnumerable<OwnedMount> mounts)
        {
            return completed && (mounts == null || !mounts.Any(m => m != null && m.mountId == MountSkyCatalog.ColumnGriffin));
        }

        /// <summary>Registers the definition with <paramref name="quests"/> and listens for the hero's level.</summary>
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
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
        }

        public static void Uninstall()
        {
            if (_installedOn == null)
            {
                return;
            }

            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            _installedOn = null;
            _collection = null;
        }

        /// <summary>Starts the quest when due and repays a missing griffin. <paramref name="heroLevel"/> 0 = unknown (only repays).</summary>
        public static void Evaluate(int heroLevel)
        {
            QuestManager quests = _installedOn;
            if (quests == null || _collection == null)
            {
                return;
            }

            bool completed = quests.IsCompleted(QuestId);
            if (OwesGriffin(completed, _collection.Mounts))
            {
                _collection.ReportHook(MountSourceKind.Quest, QuestId);
                return;
            }

            if (ShouldStart(heroLevel, quests.IsActive(QuestId), completed, _collection.Owns(MountSkyCatalog.ColumnGriffin)))
            {
                quests.StartQuest(Definition);
            }
        }

        private static void OnExperienceChanged(PlayerExperienceChangedEvent evt) => Evaluate(evt.Level);
    }
}
