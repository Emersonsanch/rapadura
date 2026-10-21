using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Resolves a <see cref="BiomeType"/> to its <see cref="BiomeArenaProfile"/>: the asset in
    /// Resources/Biomes/Arena when present, otherwise a cached
    /// <see cref="BiomeArenaProfile.CreateBuiltInDefault"/> — so <see cref="Get"/> never returns null.
    /// Same load-on-demand/invalidate/SetForTests shape as <see cref="BiomeRegistry"/>.
    /// </summary>
    public static class BiomeArenaProfiles
    {
        public const string ResourcesFolder = "Biomes/Arena";

        private static List<BiomeArenaProfile> _assets;
        private static readonly Dictionary<BiomeType, BiomeArenaProfile> _builtIn = new Dictionary<BiomeType, BiomeArenaProfile>();

        private static void EnsureLoaded()
        {
            if (_assets != null)
            {
                return;
            }

            _assets = new List<BiomeArenaProfile>(Resources.LoadAll<BiomeArenaProfile>(ResourcesFolder));
        }

        /// <summary>Asset for <paramref name="biome"/>, else the built-in default (never null).</summary>
        public static BiomeArenaProfile Get(BiomeType biome)
        {
            EnsureLoaded();
            BiomeArenaProfile asset = BiomeArenaRules.SelectProfile(biome, _assets);
            if (asset != null)
            {
                return asset;
            }

            if (!_builtIn.TryGetValue(biome, out BiomeArenaProfile fallback) || fallback == null)
            {
                fallback = BiomeArenaProfile.CreateBuiltInDefault(biome);
                _builtIn[biome] = fallback;
            }

            return fallback;
        }

        /// <summary>True if a Resources asset (not the code fallback) exists for <paramref name="biome"/>.</summary>
        public static bool HasAsset(BiomeType biome)
        {
            EnsureLoaded();
            return BiomeArenaRules.SelectProfile(biome, _assets) != null;
        }

        /// <summary>Forces a re-scan of Resources/Biomes/Arena on the next lookup.</summary>
        public static void Invalidate()
        {
            _assets = null;
        }

        /// <summary>EditMode injection path: replaces the Resources scan until <see cref="Invalidate"/>.</summary>
        public static void SetForTests(IEnumerable<BiomeArenaProfile> profiles)
        {
            _assets = profiles != null ? new List<BiomeArenaProfile>(profiles) : new List<BiomeArenaProfile>();
        }
    }
}
