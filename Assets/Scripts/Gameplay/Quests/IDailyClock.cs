using System;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Source of "what day is it" for <see cref="DailyQuestService"/>. Only the local calendar date
    /// of <see cref="Now"/> matters. Injected so EditMode tests can move time forwards/backwards
    /// without touching the system clock.
    /// </summary>
    public interface IDailyClock
    {
        /// <summary>Current local time (DateTimeKind.Local or Unspecified; only <c>.Date</c> is used).</summary>
        DateTime Now { get; }
    }

    /// <summary>Runtime default: the device's local clock (<see cref="DateTime.Now"/>).</summary>
    public sealed class SystemDailyClock : IDailyClock
    {
        public static readonly SystemDailyClock Instance = new SystemDailyClock();

        public DateTime Now => DateTime.Now;
    }
}
