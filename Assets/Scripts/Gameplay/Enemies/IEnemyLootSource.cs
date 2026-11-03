using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Optional component on one enemy instance that changes where its loot comes from (read by <see cref="EnemyLootGranter"/>,
    /// next to <see cref="IKillExperienceScaler"/> for the XP). Used by the dungeons (<c>DungeonEnemyTag</c>), whose enemies
    /// are in no region's spawn table: they roll the dungeon's own table instead, and a reanimated guardian drops nothing.
    /// </summary>
    public interface IEnemyLootSource
    {
        /// <summary>True = this death drops nothing at all (base loot, table, refine stones, parts, entry items).</summary>
        bool SuppressLoot { get; }

        /// <summary>The loot table rolled instead of the region's (a BiomeDefinition used for its loot entries); null = the region's.</summary>
        BiomeDefinition LootTable { get; }
    }
}
