using Rapadura.Core.Events;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Raised by <see cref="TimeOfDayManager"/> every time the in-game clock crosses a phase boundary
    /// (dawn/day/dusk/night), and on load when the restored time lands in a different phase.
    /// </summary>
    public readonly struct TimeOfDayPhaseChangedEvent : IGameEvent
    {
        public readonly TimeOfDayPhase PreviousPhase;
        public readonly TimeOfDayPhase CurrentPhase;
        public readonly int Day;

        public TimeOfDayPhaseChangedEvent(TimeOfDayPhase previousPhase, TimeOfDayPhase currentPhase, int day)
        {
            PreviousPhase = previousPhase;
            CurrentPhase = currentPhase;
            Day = day;
        }
    }

    /// <summary>
    /// Raised by <see cref="TimeOfDayManager"/> whenever the whole in-game hour changes (at most once
    /// per tick). Intended for a future HUD clock; finer readouts can poll
    /// <see cref="TimeOfDayManager.CurrentHour"/> through the ServiceLocator.
    /// </summary>
    public readonly struct TimeOfDayHourChangedEvent : IGameEvent
    {
        public readonly int Hour;
        public readonly int Day;
        public readonly TimeOfDayPhase Phase;

        public TimeOfDayHourChangedEvent(int hour, int day, TimeOfDayPhase phase)
        {
            Hour = hour;
            Day = day;
            Phase = phase;
        }
    }

    /// <summary>
    /// Raised by <see cref="WeatherSystem"/> when a new weather state starts blending in (or snaps in,
    /// with <see cref="TransitionSeconds"/> = 0, after a load). Intended for HUD/audio hooks.
    /// </summary>
    public readonly struct WeatherChangedEvent : IGameEvent
    {
        public readonly WeatherType PreviousWeather;
        public readonly WeatherType CurrentWeather;
        public readonly BiomeType Biome;
        public readonly float TransitionSeconds;

        public WeatherChangedEvent(WeatherType previousWeather, WeatherType currentWeather, BiomeType biome, float transitionSeconds)
        {
            PreviousWeather = previousWeather;
            CurrentWeather = currentWeather;
            Biome = biome;
            TransitionSeconds = transitionSeconds;
        }
    }
}
