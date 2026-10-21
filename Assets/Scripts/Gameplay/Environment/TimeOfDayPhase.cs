namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Coarse phases of the in-game day driven by <see cref="TimeOfDayManager"/>. Boundaries (in
    /// in-game hours) live on <see cref="TimeOfDayManager"/> (<c>DawnStartHour</c> etc.).
    /// </summary>
    public enum TimeOfDayPhase
    {
        Dawn = 0,
        Day = 1,
        Dusk = 2,
        Night = 3,
    }

    /// <summary>Weather states rolled per biome by <see cref="WeatherSystem"/> (see <see cref="WeatherTable"/>).</summary>
    public enum WeatherType
    {
        Clear = 0,
        Rain = 1,
        Fog = 2,

        /// <summary>Desert only by default.</summary>
        Sandstorm = 3,

        /// <summary>Mountain only by default.</summary>
        Snow = 4,
    }
}
