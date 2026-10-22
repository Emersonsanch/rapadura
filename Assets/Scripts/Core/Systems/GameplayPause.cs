using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.UI.Menus;
using UnityEngine;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// The one way modal screens freeze gameplay. Every screen that must stop enemies/AI/damage while
    /// it is up (character select, dialogue, shop, inventory, quest log, skill tree, settings,
    /// save/load, crafting, achievements, credits, death screen, the title front-end) calls
    /// <see cref="Acquire"/> when it opens and <see cref="Release"/> when it closes/disables, passing
    /// itself as the owner. Pausing is the project's existing convention — Time.timeScale = 0 plus
    /// <see cref="GamePausedEvent"/>/<see cref="GameResumedEvent"/> (TimeOfDay, Weather and
    /// FrameRateManager already listen) — but reference-counted, so closing one screen no longer
    /// un-pauses the game while another is still open.
    ///
    /// Why this exists: each screen used to write Time.timeScale on its own, so the last writer won.
    /// In Main.unity PauseMenuController.OnEnable → Hide() sets timeScale back to 1, which (depending
    /// on OnEnable order) silently un-froze the character select screen and let enemies kill the
    /// player behind it; HitStopController likewise restores 1 after a freeze-frame. While any owner
    /// holds a pause, <see cref="GameplayPauseDriver"/> (added to the GameManager object) re-asserts
    /// timeScale 0 every frame via <see cref="Enforce"/>.
    ///
    /// PauseMenuController still drives timeScale itself (it is owned elsewhere); its pause is
    /// tracked as "external" through the events it publishes, so releasing the last modal while the
    /// pause menu is open leaves the game paused, and closing the pause menu while a modal is still
    /// open immediately re-freezes and re-announces the pause.
    /// </summary>
    public static class GameplayPause
    {
        private static readonly PauseRequestTracker Tracker = new PauseRequestTracker();
        private static bool _publishing;

        /// <summary>Gameplay is frozen by a modal owner or by the pause menu.</summary>
        public static bool IsPaused => Tracker.IsPaused;

        /// <summary>At least one modal screen holds a pause (excludes the pause menu).</summary>
        public static bool HasRequests => Tracker.HasOwners;

        /// <summary>Number of modal screens currently holding a pause.</summary>
        public static int OwnerCount => Tracker.OwnerCount;

        /// <summary>The pause menu (tracked only through its events) is currently pausing.</summary>
        public static bool IsExternallyPaused => Tracker.ExternalPaused;

        /// <summary>Appends the current pause owners to <paramref name="buffer"/> (diagnostics only).</summary>
        public static void CopyOwnersTo(List<object> buffer) => Tracker.CopyOwnersTo(buffer);

        public static bool IsHeldBy(object owner) => Tracker.IsHeldBy(owner);

        /// <summary>A modal screen other than <paramref name="owner"/> (or the pause menu) is freezing gameplay.</summary>
        public static bool IsPausedByOthers(object owner) => Tracker.IsPausedByOthers(owner);

        public static void Acquire(object owner)
        {
            if (Tracker.Acquire(owner))
            {
                Time.timeScale = 0f;
                Publish(new GamePausedEvent());
            }
            else if (Tracker.HasOwners)
            {
                Time.timeScale = 0f;
            }
        }

        public static void Release(object owner)
        {
            if (Tracker.Release(owner))
            {
                Time.timeScale = 1f;
                Publish(new GameResumedEvent());
            }
        }

        /// <summary>Re-freezes time if an owner holds a pause but something (hit-stop, pause menu) wrote another scale.</summary>
        public static void Enforce()
        {
            if (Tracker.HasOwners && Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }

        /// <summary>Forwarded by <see cref="GameplayPauseDriver"/> for <see cref="GamePausedEvent"/>s this class didn't publish (the pause menu).</summary>
        public static void HandleExternalPaused()
        {
            if (_publishing)
            {
                return;
            }

            Tracker.SetExternalPaused(true);
        }

        /// <summary>Forwarded by <see cref="GameplayPauseDriver"/>: the pause menu resumed — keep gameplay frozen if a modal is still open.</summary>
        public static void HandleExternalResumed()
        {
            if (_publishing)
            {
                return;
            }

            Tracker.SetExternalPaused(false);

            if (Tracker.HasOwners)
            {
                Time.timeScale = 0f;
                Publish(new GamePausedEvent());
            }
        }

        /// <summary>
        /// A scene load destroys whatever external pause owner was open without it ever publishing
        /// "resumed" — forget it so the next release can actually resume the game.
        /// </summary>
        public static void HandleSceneLoaded()
        {
            Tracker.SetExternalPaused(false);
        }

        /// <summary>Test / domain-reload hook. Does not touch Time.timeScale.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Tracker.Clear();
            _publishing = false;
        }

        private static void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
        {
            _publishing = true;

            try
            {
                EventBus.Publish(gameEvent);
            }
            finally
            {
                _publishing = false;
            }
        }
    }
}
