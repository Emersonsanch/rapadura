namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Optional component on one enemy instance that rescales its kill XP (applied by <see cref="EnemyRewardGranter"/>
    /// before the "Nova Jornada+" scale). Used by the event rooms (<c>EventRoomEnemyScaler</c>), whose enemies come
    /// from every region and are scaled to the hero's level bracket.
    /// </summary>
    public interface IKillExperienceScaler
    {
        int ScaleKillExperience(int baseExperience);
    }
}
