using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Process-wide cache of generated arena meshes, keyed by (biome, terrain variant). Main.unity is
    /// reloaded for every encounter, so the meshes live here (not in the scene) and survive the
    /// unload: a second fight in the same biome/variant reuses them instead of regenerating. The
    /// ground collider data is pre-baked once per mesh (Physics.BakeMesh), so
    /// assigning it to a fresh MeshCollider is cheap. LRU-bounded to <see cref="Capacity"/> entries
    /// (the newest is never evicted, so the arena in use always keeps its meshes).
    /// </summary>
    public static class BiomeArenaTerrainCache
    {
        public const int Capacity = 4;

        public sealed class Entry
        {
            public BiomeType Biome;
            public int Variant;
            public Mesh Ground;
            public Mesh Backdrop;

            public bool IsAlive => Ground != null && Backdrop != null;
        }

        // Most recently used last.
        private static readonly List<Entry> _entries = new List<Entry>();

        /// <summary>Total generations since the last <see cref="Clear"/> (tests: stays flat on cache hits).</summary>
        public static int BuildCount { get; private set; }

        public static int Count => _entries.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Enter Play Mode without a domain reload keeps statics: drop the previous session's meshes.
            Clear();
        }

        // No HideFlags.DontSave: Resources.UnloadUnusedAssets treats static fields as roots, so cached
        // meshes survive scene loads; after a domain reload they are simply collected.

        /// <summary>Cached (or newly generated) meshes for <paramref name="biome"/> / <paramref name="variant"/>.</summary>
        public static Entry Get(BiomeType biome, int variant)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (entry.Biome != biome || entry.Variant != variant)
                {
                    continue;
                }

                if (!entry.IsAlive)
                {
                    DestroyMeshes(entry);
                    _entries.RemoveAt(i);
                    break;
                }

                _entries.RemoveAt(i);
                _entries.Add(entry);
                return entry;
            }

            var field = new BiomeArenaTerrainField(biome, variant);
            var built = new Entry
            {
                Biome = biome,
                Variant = variant,
                Ground = BiomeArenaTerrainGenerator.BuildGround(field),
                Backdrop = BiomeArenaTerrainGenerator.BuildBackdrop(field),
            };
            Physics.BakeMesh(built.Ground.GetEntityId(), false);
            BuildCount++;

            _entries.Add(built);
            while (_entries.Count > Capacity)
            {
                DestroyMeshes(_entries[0]);
                _entries.RemoveAt(0);
            }

            return built;
        }

        /// <summary>Destroys every cached mesh (tests; also safe at any time the arena is not shown).</summary>
        public static void Clear()
        {
            foreach (Entry entry in _entries)
            {
                DestroyMeshes(entry);
            }

            _entries.Clear();
            BuildCount = 0;
        }

        private static void DestroyMeshes(Entry entry)
        {
            BiomeArenaPropFactory.SafeDestroy(entry.Ground);
            BiomeArenaPropFactory.SafeDestroy(entry.Backdrop);
            entry.Ground = null;
            entry.Backdrop = null;
        }
    }
}
