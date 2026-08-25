namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Plain C# bookkeeping for a single wave's alive-enemy count. Kept free of any Unity/MonoBehaviour
    /// dependency so the completion logic (when does a wave count as cleared) can be unit tested directly.
    /// </summary>
    public class WaveTracker
    {
        public int WaveIndex { get; private set; }
        public int TotalSpawned { get; private set; }
        public int AliveCount { get; private set; }
        public bool IsActive { get; private set; }
        /// <summary>
        /// True once a started wave has run out of enemies. Deliberately does NOT require
        /// <see cref="IsActive"/> — <see cref="RegisterDeath"/> sets <c>IsActive = false</c> at the
        /// exact moment a wave completes, so requiring it here made this contradictory (always
        /// false: the instant the condition could become true, IsActive had just been cleared).
        /// </summary>
        public bool IsComplete => TotalSpawned > 0 && AliveCount <= 0;

        public void StartWave(int waveIndex, int enemyCount)
        {
            WaveIndex = waveIndex;
            TotalSpawned = enemyCount;
            AliveCount = enemyCount;
            IsActive = enemyCount > 0;
        }

        /// <summary>Registers one enemy death. Returns true the moment this call causes the wave to become complete.</summary>
        public bool RegisterDeath()
        {
            if (!IsActive || AliveCount <= 0)
            {
                return false;
            }

            AliveCount--;

            if (AliveCount <= 0)
            {
                IsActive = false;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            WaveIndex = 0;
            TotalSpawned = 0;
            AliveCount = 0;
            IsActive = false;
        }
    }
}
