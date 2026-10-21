using Rapadura.Core.Events;

namespace Rapadura.Gameplay.Achievements
{
    /// <summary>Raised once when an achievement unlocks (the toast and achievements list listen to it).</summary>
    public readonly struct AchievementUnlockedEvent : IGameEvent
    {
        public readonly AchievementDefinition Achievement;

        public AchievementUnlockedEvent(AchievementDefinition achievement)
        {
            Achievement = achievement;
        }
    }
}
