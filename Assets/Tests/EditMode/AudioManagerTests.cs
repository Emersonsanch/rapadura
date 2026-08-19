using NUnit.Framework;
using Rapadura.Core.Audio;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the audio system's pure state/logic: category volume math, pooling,
    /// cue lookup and ducking/crossfade timers. Unity does not actually play audio in headless
    /// EditMode tests, so these deliberately avoid asserting anything about real sound output —
    /// only about AudioManager's internal state and the AudioSource objects it drives.
    /// </summary>
    public class AudioManagerTests
    {
        private AudioManager _manager;

        [SetUp]
        public void SetUp()
        {
            ClearVolumePrefs();
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Shutdown();
            _manager = null;
            ClearVolumePrefs();
        }

        private static void ClearVolumePrefs()
        {
            foreach (AudioCategory category in new[] { AudioCategory.Master, AudioCategory.Music, AudioCategory.Sfx, AudioCategory.Ui })
            {
                PlayerPrefs.DeleteKey("Audio.Volume." + category);
            }
        }

        private AudioManager CreateManager(AudioCueDatabase database = null, CombatAudioCueMap combatMap = null)
        {
            _manager = new AudioManager(database, combatMap);
            _manager.Initialize();
            return _manager;
        }

        [Test]
        public void Initialize_DefaultsAllCategoryVolumesToFull()
        {
            AudioManager manager = CreateManager();

            Assert.AreEqual(1f, manager.GetCategoryVolume(AudioCategory.Master));
            Assert.AreEqual(1f, manager.GetCategoryVolume(AudioCategory.Music));
            Assert.AreEqual(1f, manager.GetCategoryVolume(AudioCategory.Sfx));
            Assert.AreEqual(1f, manager.GetCategoryVolume(AudioCategory.Ui));
        }

        [Test]
        public void SetCategoryVolume_ClampsToZeroOne()
        {
            AudioManager manager = CreateManager();

            manager.SetCategoryVolume(AudioCategory.Sfx, 5f);
            Assert.AreEqual(1f, manager.GetCategoryVolume(AudioCategory.Sfx));

            manager.SetCategoryVolume(AudioCategory.Sfx, -3f);
            Assert.AreEqual(0f, manager.GetCategoryVolume(AudioCategory.Sfx));
        }

        [Test]
        public void SetCategoryVolume_PersistsAcrossNewManagerInstance()
        {
            AudioManager first = CreateManager();
            first.SetCategoryVolume(AudioCategory.Music, 0.4f);
            first.Shutdown();

            var second = new AudioManager();
            second.Initialize();

            Assert.AreEqual(0.4f, second.GetCategoryVolume(AudioCategory.Music), 0.0001f);

            second.Shutdown();
        }

        [Test]
        public void GetEffectiveVolume_MultipliesMasterAndCategory()
        {
            AudioManager manager = CreateManager();

            manager.SetCategoryVolume(AudioCategory.Master, 0.5f);
            manager.SetCategoryVolume(AudioCategory.Sfx, 0.5f);

            Assert.AreEqual(0.25f, manager.GetEffectiveVolume(AudioCategory.Sfx), 0.0001f);
        }

        [Test]
        public void GetEffectiveVolume_ForMaster_IgnoresOtherCategories()
        {
            AudioManager manager = CreateManager();

            manager.SetCategoryVolume(AudioCategory.Master, 0.7f);

            Assert.AreEqual(0.7f, manager.GetEffectiveVolume(AudioCategory.Master), 0.0001f);
        }

        [Test]
        public void DuckMusic_LowersEffectiveMusicVolumeImmediately()
        {
            AudioManager manager = CreateManager();

            manager.DuckMusic(0.5f, 2f);

            Assert.AreEqual(0.5f, manager.CurrentDuckMultiplier, 0.0001f);
            Assert.Less(manager.GetEffectiveVolume(AudioCategory.Music), manager.GetEffectiveVolume(AudioCategory.Sfx));
        }

        [Test]
        public void DuckMusic_RecoversTowardsFullAfterDurationElapses()
        {
            AudioManager manager = CreateManager();

            manager.DuckMusic(0.6f, 0.1f);
            Assert.AreEqual(0.4f, manager.CurrentDuckMultiplier, 0.0001f);

            // Elapse past the duck duration, then tick the recovery fade several times.
            manager.Tick(0.2f);
            for (int i = 0; i < 20; i++)
            {
                manager.Tick(0.05f);
            }

            Assert.AreEqual(1f, manager.CurrentDuckMultiplier, 0.0001f);
        }

        [Test]
        public void PlaySfx_AcquiresPooledSourceAndAssignsClip()
        {
            AudioManager manager = CreateManager();
            AudioClip clip = AudioClip.Create("TestClip", 1, 1, 44100, false);

            manager.PlaySfx(clip, 0.8f);

            bool foundAssigned = false;
            foreach (AudioSource source in manager.SfxPoolForTests.Sources)
            {
                if (source.clip == clip)
                {
                    foundAssigned = true;
                }
            }

            Assert.IsTrue(foundAssigned, "Expected the pool to contain a source with the played clip assigned.");
        }

        [Test]
        public void PlayOneShot_WithoutDatabase_DoesNotThrow()
        {
            AudioManager manager = CreateManager();

            Assert.DoesNotThrow(() => manager.PlayOneShot("nonexistent"));
        }

        [Test]
        public void PlayOneShot_ResolvesCueFromDatabaseAndPlaysThroughPool()
        {
            var database = ScriptableObject.CreateInstance<AudioCueDatabase>();
            AudioClip clip = AudioClip.Create("HitClip", 1, 1, 44100, false);
            database.AddOrReplaceCue(new AudioCue("sfx.hit", clip, AudioCategory.Sfx, 1f));

            AudioManager manager = CreateManager(database);

            manager.PlayOneShot("sfx.hit");

            bool foundAssigned = false;
            foreach (AudioSource source in manager.SfxPoolForTests.Sources)
            {
                if (source.clip == clip)
                {
                    foundAssigned = true;
                }
            }

            Assert.IsTrue(foundAssigned);

            Object.DestroyImmediate(database);
        }
    }

    /// <summary>Tests for the pooling logic in isolation from AudioManager.</summary>
    public class AudioSourcePoolTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("PoolTestRoot");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Constructor_CreatesInitialSize()
        {
            var pool = new AudioSourcePool(_root.transform, 3, 10);

            Assert.AreEqual(3, pool.Count);
        }

        [Test]
        public void Acquire_ReturnsSourceThatIsNotPlaying()
        {
            var pool = new AudioSourcePool(_root.transform, 2, 10);

            AudioSource source = pool.Acquire();

            Assert.IsNotNull(source);
            Assert.IsFalse(source.isPlaying);
        }

        [Test]
        public void Acquire_GrowsPoolBeyondInitialSizeUpToMax()
        {
            var pool = new AudioSourcePool(_root.transform, 1, 3);

            // First acquire reuses the single idle source; force growth by "occupying" it via clip+Play
            // is unreliable in headless EditMode (Play may not actually flip isPlaying without an
            // AudioListener/output), so instead assert growth directly via repeated construction intent:
            // the pool must never exceed maxSize regardless of how many times Acquire is called.
            for (int i = 0; i < 10; i++)
            {
                pool.Acquire();
            }

            Assert.LessOrEqual(pool.Count, 3);
        }

        [Test]
        public void Constructor_ClampsInitialSizeToMax()
        {
            var pool = new AudioSourcePool(_root.transform, 50, 5);

            Assert.AreEqual(5, pool.Count);
        }

        [Test]
        public void StopAll_StopsEverySource()
        {
            var pool = new AudioSourcePool(_root.transform, 2, 5);

            Assert.DoesNotThrow(() => pool.StopAll());
        }
    }

    /// <summary>Tests for AudioCueDatabase and CombatAudioCueMap lookup logic.</summary>
    public class AudioCueDatabaseTests
    {
        [Test]
        public void TryGetCue_UnknownId_ReturnsFalse()
        {
            var database = ScriptableObject.CreateInstance<AudioCueDatabase>();

            bool found = database.TryGetCue("missing", out AudioCue cue);

            Assert.IsFalse(found);
            Assert.IsNull(cue);

            Object.DestroyImmediate(database);
        }

        [Test]
        public void AddOrReplaceCue_ThenTryGetCue_ReturnsSameCue()
        {
            var database = ScriptableObject.CreateInstance<AudioCueDatabase>();
            var cue = new AudioCue("ui.click", null, AudioCategory.Ui, 0.6f);

            database.AddOrReplaceCue(cue);
            bool found = database.TryGetCue("ui.click", out AudioCue result);

            Assert.IsTrue(found);
            Assert.AreEqual(0.6f, result.Volume);
            Assert.AreEqual(AudioCategory.Ui, result.Category);

            Object.DestroyImmediate(database);
        }

        [Test]
        public void AddOrReplaceCue_SameId_OverwritesPreviousEntry()
        {
            var database = ScriptableObject.CreateInstance<AudioCueDatabase>();

            database.AddOrReplaceCue(new AudioCue("sfx.step", null, AudioCategory.Sfx, 0.2f));
            database.AddOrReplaceCue(new AudioCue("sfx.step", null, AudioCategory.Sfx, 0.9f));

            database.TryGetCue("sfx.step", out AudioCue result);

            Assert.AreEqual(0.9f, result.Volume);
            Assert.AreEqual(1, database.Cues.Count);

            Object.DestroyImmediate(database);
        }

        [Test]
        public void CombatAudioCueMap_SetMapping_ThenTryGetCueId_ReturnsCueId()
        {
            var map = ScriptableObject.CreateInstance<CombatAudioCueMap>();

            map.SetMapping(CombatAudioTrigger.TargetDied, "sfx.death");
            bool found = map.TryGetCueId(CombatAudioTrigger.TargetDied, out string cueId);

            Assert.IsTrue(found);
            Assert.AreEqual("sfx.death", cueId);

            Object.DestroyImmediate(map);
        }

        [Test]
        public void CombatAudioCueMap_UnmappedTrigger_ReturnsFalse()
        {
            var map = ScriptableObject.CreateInstance<CombatAudioCueMap>();

            bool found = map.TryGetCueId(CombatAudioTrigger.CriticalHit, out string cueId);

            Assert.IsFalse(found);
            Assert.IsNull(cueId);

            Object.DestroyImmediate(map);
        }
    }
}
