using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Static lookup that resolves a <see cref="BiomeType"/> back to its <see cref="BiomeDefinition"/>
    /// asset, loaded from Resources/Biomes (see <see cref="Rapadura.Editor.BiomeSeeder"/> for the
    /// example content generator). Mirrors the load-on-demand/invalidate pattern used by
    /// <c>ItemDatabase</c> and the fixed-dictionary pattern used by <c>CharacterRegistry</c>.
    /// </summary>
    public static class BiomeRegistry
    {
        private const string ResourcesFolder = "Biomes";

        private static Dictionary<BiomeType, BiomeDefinition> _biomesByType;

        private static void EnsureLoaded()
        {
            if (_biomesByType != null)
            {
                return;
            }

            _biomesByType = new Dictionary<BiomeType, BiomeDefinition>();

            foreach (BiomeDefinition biome in Resources.LoadAll<BiomeDefinition>(ResourcesFolder))
            {
                if (biome == null)
                {
                    continue;
                }

                _biomesByType[biome.Type] = biome;
            }
        }

        public static BiomeDefinition Get(BiomeType type)
        {
            EnsureLoaded();
            return _biomesByType.TryGetValue(type, out BiomeDefinition biome) ? biome : null;
        }

        public static bool TryGet(BiomeType type, out BiomeDefinition biome)
        {
            EnsureLoaded();
            return _biomesByType.TryGetValue(type, out biome);
        }

        public static IEnumerable<BiomeDefinition> GetAll()
        {
            EnsureLoaded();
            return _biomesByType.Values;
        }

        /// <summary>Forces the registry to re-scan Resources/Biomes. Call after generating/removing biome assets in the editor.</summary>
        public static void Invalidate()
        {
            _biomesByType = null;
        }
    }
}
