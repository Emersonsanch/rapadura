using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>Why the hero cannot go into a room right now (in the order checked).</summary>
    public enum EventRoomEntryBlock
    {
        None = 0,
        Closed = 1,
        AlreadyEntered = 2,
        Level = 3,
        NoItem = 4,
    }

    /// <summary>
    /// Builds the encounter payload of an event room and checks entry. The payload's enemy ids are the room's pool:
    /// every regular (non-boss) enemy of every region, ordered by region tier (Floresta → Cavernas), so the arena's
    /// usual hand-off (and the "is there a boss" music check) still works and the director draws its waves from it.
    /// </summary>
    public static class EventRoomEntry
    {
        public static EventRoomEntryBlock CheckEntry(EventRoomWindow window, long lastEnteredWindow, int heroLevel, EventRoomKind kind, int entryItemsHeld)
        {
            if (!window.IsOpen)
            {
                return EventRoomEntryBlock.Closed;
            }

            if (window.WindowIndex == lastEnteredWindow)
            {
                return EventRoomEntryBlock.AlreadyEntered;
            }

            if (heroLevel < EventRoomRules.MinLevel(kind))
            {
                return EventRoomEntryBlock.Level;
            }

            return entryItemsHeld <= 0 ? EventRoomEntryBlock.NoItem : EventRoomEntryBlock.None;
        }

        /// <summary>Regular enemies of every biome, by tier then name (deduplicated).</summary>
        public static List<EnemyDefinition> BuildPool(IEnumerable<BiomeDefinition> biomes)
        {
            var pool = new List<EnemyDefinition>();
            var byTier = new List<BiomeDefinition>();
            if (biomes != null)
            {
                foreach (BiomeDefinition biome in biomes)
                {
                    if (biome != null)
                    {
                        byTier.Add(biome);
                    }
                }
            }

            byTier.Sort((a, b) => ((int)a.Type).CompareTo((int)b.Type));
            foreach (BiomeDefinition biome in byTier)
            {
                var regional = new List<EnemyDefinition>();
                foreach (BiomeEnemySpawnEntry entry in biome.Enemies)
                {
                    EnemyDefinition enemy = entry?.Enemy;
                    if (enemy != null && !enemy.IsBoss && !pool.Contains(enemy) && !regional.Contains(enemy))
                    {
                        regional.Add(enemy);
                    }
                }

                regional.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
                pool.AddRange(regional);
            }

            return pool;
        }

        public static EncounterPayload BuildPayload(EventRoomKind kind, BiomeType region, int heroLevel, int seed, IReadOnlyList<EnemyDefinition> pool)
        {
            if (kind == EventRoomKind.None || pool == null || pool.Count == 0)
            {
                return null;
            }

            EncounterPayload payload = EncounterPayload.Create(region, pool, seed);
            payload.eventRoom = kind;
            payload.eventRoomLevel = heroLevel < 1 ? 1 : heroLevel;
            return payload;
        }

        /// <summary>Region tier (0 Floresta … 3 Cavernas) an enemy belongs to; the bracket's tier when unknown.</summary>
        public static int NativeTier(EnemyDefinition enemy, EventRoomBracket bracket)
        {
            BiomeDefinition biome = enemy != null ? BiomeRegistry.FindBiomeOf(enemy) : null;
            return biome != null ? (int)biome.Type : bracket.ReferenceTier;
        }
    }
}
