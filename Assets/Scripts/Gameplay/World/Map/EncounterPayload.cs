using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>How an encounter ended, reported by the combat arena back to <see cref="SceneTransitionService"/>.</summary>
    public enum EncounterOutcome
    {
        Victory = 0,
        Fled = 1,
        Defeat = 2,
    }

    /// <summary>
    /// Everything the combat scene needs to know about an encounter rolled on the world map: which
    /// biome it happened in and which enemies were rolled (via <see cref="BiomeDefinition.RollEnemy"/>).
    /// Carried in memory from WorldMap.unity to Main.unity by <see cref="SceneTransitionService"/>
    /// (which survives the scene load because GameManager is DontDestroyOnLoad).
    ///
    /// Enemies are stored by id — the <see cref="EnemyDefinition"/> asset name, which is also its
    /// Resources path under <see cref="EnemyResourcesFolder"/> — so the payload is plain-JSON
    /// serializable (<see cref="ToJson"/>/<see cref="FromJson"/>) for logging, analytics or a future
    /// "resume mid-encounter" save. Direct asset references captured at roll time are cached in a
    /// non-serialized field so the in-memory hand-off never needs a Resources lookup.
    /// </summary>
    [Serializable]
    public class EncounterPayload
    {
        /// <summary>Resources folder EnemyDefinition assets live in (see EnemyDatabaseSeeder).</summary>
        public const string EnemyResourcesFolder = "Enemies";

        public BiomeType biome;
        public List<string> enemyIds = new List<string>();

        /// <summary>Seed for any per-encounter randomness in the arena (spawn-slot shuffle, future modifiers).</summary>
        public int seed;

        [NonSerialized] private List<EnemyDefinition> _cachedEnemies;

        /// <summary>Builds a payload from rolled enemy definitions, skipping nulls.</summary>
        public static EncounterPayload Create(BiomeType biome, IReadOnlyList<EnemyDefinition> enemies, int seed)
        {
            var payload = new EncounterPayload
            {
                biome = biome,
                seed = seed,
                _cachedEnemies = new List<EnemyDefinition>(),
            };

            if (enemies != null)
            {
                foreach (EnemyDefinition enemy in enemies)
                {
                    if (enemy == null)
                    {
                        continue;
                    }

                    payload.enemyIds.Add(enemy.name);
                    payload._cachedEnemies.Add(enemy);
                }
            }

            return payload;
        }

        public int EnemyCount => enemyIds != null ? enemyIds.Count : 0;

        public string ToJson() => JsonUtility.ToJson(this);

        /// <summary>Parses a payload produced by <see cref="ToJson"/>. Returns null for null/empty/invalid JSON instead of throwing.</summary>
        public static EncounterPayload FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                EncounterPayload payload = JsonUtility.FromJson<EncounterPayload>(json);
                if (payload != null && payload.enemyIds == null)
                {
                    payload.enemyIds = new List<string>();
                }

                return payload;
            }
            catch (Exception)
            {
                // JsonUtility throws ArgumentException on malformed JSON; treat any parse failure as "no payload".
                return null;
            }
        }

        /// <summary>
        /// Returns the encounter's enemy definitions. Uses the references cached at roll time when
        /// available; otherwise (e.g. after <see cref="FromJson"/>) resolves each id through
        /// <paramref name="lookup"/>, defaulting to <c>Resources.Load</c> under
        /// <see cref="EnemyResourcesFolder"/>. Ids that fail to resolve are skipped.
        /// </summary>
        public List<EnemyDefinition> ResolveEnemies(Func<string, EnemyDefinition> lookup = null)
        {
            if (_cachedEnemies != null && _cachedEnemies.Count == EnemyCount)
            {
                return new List<EnemyDefinition>(_cachedEnemies);
            }

            lookup ??= id => Resources.Load<EnemyDefinition>($"{EnemyResourcesFolder}/{id}");

            var resolved = new List<EnemyDefinition>();
            if (enemyIds == null)
            {
                return resolved;
            }

            foreach (string id in enemyIds)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                EnemyDefinition enemy = lookup(id);
                if (enemy != null)
                {
                    resolved.Add(enemy);
                }
            }

            return resolved;
        }
    }
}
