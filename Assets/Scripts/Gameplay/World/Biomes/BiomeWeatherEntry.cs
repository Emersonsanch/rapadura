using System;
using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// A single weighted entry in a biome's optional weather table (see
    /// <see cref="BiomeDefinition.Weather"/>). Rolled by <see cref="WeatherTable.Roll"/>; when a
    /// biome has no entries (every existing asset, until authored) the per-biome defaults in
    /// <see cref="WeatherTable"/> are used instead.
    /// </summary>
    [Serializable]
    public class BiomeWeatherEntry
    {
        [SerializeField] private WeatherType _weather = WeatherType.Clear;
        [SerializeField] [Min(0)] private int _weight = 1;

        public WeatherType Weather => _weather;
        public int Weight => _weight;

        public BiomeWeatherEntry()
        {
        }

        /// <summary>Plain-C# construction path for EditMode tests.</summary>
        public BiomeWeatherEntry(WeatherType weather, int weight)
        {
            _weather = weather;
            _weight = weight;
        }
    }
}
