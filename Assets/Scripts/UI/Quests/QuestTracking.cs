using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using UnityEngine;

namespace Rapadura.UI.Quests
{
    /// <summary>Raised when the player picks a different quest to follow in the compact on-screen tracker.</summary>
    public readonly struct QuestTrackedChangedEvent : IGameEvent
    {
        public readonly string QuestId;

        public QuestTrackedChangedEvent(string questId)
        {
            QuestId = questId;
        }
    }

    /// <summary>
    /// Which quest the player chose to follow. Lives here (UI layer) rather than in
    /// <c>QuestManager</c> because it's purely a presentation preference shared by
    /// <see cref="QuestLogController"/> (the "Acompanhar" button) and <see cref="QuestTrackerController"/>
    /// (which displays it) — neither holds a reference to the other; they meet through
    /// <see cref="QuestTrackedChangedEvent"/> on the EventBus, same decoupling as every other UI screen.
    /// Not persisted: after a reload the tracker falls back to the first active quest
    /// (see <see cref="QuestLogPresenter.ResolveTrackedQuestId"/>).
    /// </summary>
    public static class QuestTracking
    {
        public static string PreferredQuestId { get; private set; }

        public static void SetPreferred(string questId)
        {
            if (PreferredQuestId == questId)
            {
                return;
            }

            PreferredQuestId = questId;
            EventBus.Publish(new QuestTrackedChangedEvent(questId));
        }

        /// <summary>Test/scene-reset hook.</summary>
        public static void Reset()
        {
            PreferredQuestId = null;
        }

        /// <summary>
        /// Forgets the followed quest whenever the session is replaced. NewSessionResetEvent fires on
        /// "Novo jogo" and inside every SaveManager.Load (SessionReset.ResetAll runs before the file is
        /// applied), so loading a save no longer keeps following the previous run's quest — it used to
        /// be cleared only by the title screen's "Novo jogo" button. GameLoadedEvent is deliberately
        /// not used: SaveManager.RestoreCarriedState publishes it on every scene hop too.
        /// Idempotent; runs at startup and can be re-run after a test's EventBus.Clear().
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void EnsureSubscribed()
        {
            EventBus.Unsubscribe<NewSessionResetEvent>(OnNewSessionReset);
            EventBus.Subscribe<NewSessionResetEvent>(OnNewSessionReset);
        }

        private static void OnNewSessionReset(NewSessionResetEvent evt) => SetPreferred(null);
    }
}
