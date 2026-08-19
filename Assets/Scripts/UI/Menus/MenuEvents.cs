using Rapadura.Core.EventBus;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Pause/resume events for the gameplay loop, published by <see cref="PauseMenuController"/>.
    ///
    /// GameEvents.cs has no "game paused" event yet, and this task's scope explicitly excludes
    /// editing that file, so these live in their own file instead — same <see cref="IGameEvent"/>
    /// contract, just declared in the UI.Menus namespace. If a "real" pause event is ever added to
    /// GameEvents.cs, these can be removed and callers repointed with a find/replace.
    /// </summary>
    public readonly struct GamePausedEvent : IGameEvent
    {
    }

    /// <summary>Raised when gameplay resumes after being paused (see <see cref="GamePausedEvent"/>).</summary>
    public readonly struct GameResumedEvent : IGameEvent
    {
    }
}
