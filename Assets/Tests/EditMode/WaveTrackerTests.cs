using NUnit.Framework;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.Tests
{
    /// <summary>Tests the plain-C# wave/pool bookkeeping logic used by EnemySpawner, without touching any MonoBehaviour or the EventBus.</summary>
    public class WaveTrackerTests
    {
        [Test]
        public void StartWave_InitializesCounts()
        {
            var tracker = new WaveTracker();

            tracker.StartWave(waveIndex: 2, enemyCount: 5);

            Assert.AreEqual(2, tracker.WaveIndex);
            Assert.AreEqual(5, tracker.TotalSpawned);
            Assert.AreEqual(5, tracker.AliveCount);
            Assert.IsTrue(tracker.IsActive);
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void StartWave_WithZeroEnemies_IsNotActive()
        {
            var tracker = new WaveTracker();

            tracker.StartWave(waveIndex: 0, enemyCount: 0);

            Assert.IsFalse(tracker.IsActive);
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void RegisterDeath_DecrementsAliveCount()
        {
            var tracker = new WaveTracker();
            tracker.StartWave(0, 3);

            bool completed = tracker.RegisterDeath();

            Assert.AreEqual(2, tracker.AliveCount);
            Assert.IsFalse(completed);
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void RegisterDeath_ReturnsTrueOnlyOnTheKillThatEmptiesTheWave()
        {
            var tracker = new WaveTracker();
            tracker.StartWave(0, 2);

            Assert.IsFalse(tracker.RegisterDeath());
            Assert.IsTrue(tracker.RegisterDeath());
            Assert.IsTrue(tracker.IsComplete);
            Assert.IsFalse(tracker.IsActive);
        }

        [Test]
        public void RegisterDeath_IsNoOpAfterWaveAlreadyComplete()
        {
            var tracker = new WaveTracker();
            tracker.StartWave(0, 1);

            Assert.IsTrue(tracker.RegisterDeath());
            Assert.IsFalse(tracker.RegisterDeath());
            Assert.AreEqual(0, tracker.AliveCount);
        }

        [Test]
        public void RegisterDeath_IsNoOp_WhenWaveNeverStarted()
        {
            var tracker = new WaveTracker();

            Assert.IsFalse(tracker.RegisterDeath());
            Assert.AreEqual(0, tracker.AliveCount);
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            var tracker = new WaveTracker();
            tracker.StartWave(3, 4);

            tracker.Reset();

            Assert.AreEqual(0, tracker.WaveIndex);
            Assert.AreEqual(0, tracker.TotalSpawned);
            Assert.AreEqual(0, tracker.AliveCount);
            Assert.IsFalse(tracker.IsActive);
            Assert.IsFalse(tracker.IsComplete);
        }
    }
}
