namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Optional component on one enemy instance that carries its own creature level (Docs/Progressao500.md §1.2/§4): the
    /// open-map monsters (<c>MapMonsterInfo</c>). Read by <see cref="EnemyRewardGranter"/> (kill XP by level and by level
    /// difference, <see cref="EnemyLevelScaling.LeveledKillExperience"/>) and <see cref="EnemyLootGranter"/> (no equipment
    /// from a creature far below the hero). Campaign encounters have none, so their XP and loot stay as they are.
    /// </summary>
    public interface ICreatureLevelSource
    {
        int CreatureLevel { get; }
    }
}
