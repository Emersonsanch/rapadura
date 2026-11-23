namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>
    /// Optional bridge for the offline spawner (<c>MapMonsterSpawner</c>) to read the shared-world respawn time. Off by default
    /// (<c>MapMonsterSpawner.UseSharedSchedule</c> = false): it then returns the clearing's own <c>RespawnSeconds</c>, exactly
    /// as before. On, a death waits its tier's time with the seeded variation for a lone player (density ×1). The offline
    /// clearing still sleeps and refills as it does today, and the rare region lock (<see cref="SharedRareTracker"/>) is
    /// online only. Pure.
    /// </summary>
    public static class SharedSpawnBridge
    {
        /// <summary>The seed the offline bridge uses (offline has no server seed; any fixed value keeps it repeatable).</summary>
        public const int OfflineSeed = 0x5EED;

        public static float RespawnSeconds(bool useSharedSchedule, float clearingRespawnSeconds, string clearingId, string speciesId, int deathIndex)
        {
            if (!useSharedSchedule)
            {
                return clearingRespawnSeconds;
            }

            SpawnTier tier = SharedSpawnRules.SlotTier(SharedSpawnRules.TierOfSpecies(speciesId));
            return SharedSpawnRules.RespawnSeconds(tier, clearingRespawnSeconds, OfflineSeed, clearingId, deathIndex, 1);
        }
    }
}
