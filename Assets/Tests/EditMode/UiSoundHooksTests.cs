using NUnit.Framework;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="UiSoundHooks"/>: it must never throw, whether or not an
    /// <see cref="AudioManager"/> is registered in the <see cref="ServiceLocator"/> — mirrors the
    /// "no scene/manager present yet" situations <c>AudioManagerTests</c> already covers for
    /// AudioManager itself.
    /// </summary>
    public class UiSoundHooksTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Unregister<AudioManager>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Unregister<AudioManager>();
        }

        [Test]
        public void PlayClick_DoesNotThrow_WhenAudioManagerNotRegistered()
        {
            Assert.DoesNotThrow(() => UiSoundHooks.PlayClick());
        }

        [Test]
        public void PlayHover_DoesNotThrow_WhenAudioManagerNotRegistered()
        {
            Assert.DoesNotThrow(() => UiSoundHooks.PlayHover());
        }

        [Test]
        public void PlayError_DoesNotThrow_WhenAudioManagerNotRegistered()
        {
            Assert.DoesNotThrow(() => UiSoundHooks.PlayError());
        }

        [Test]
        public void PlayClick_DoesNotThrow_WhenAudioManagerRegisteredButCueMissing()
        {
            var manager = new AudioManager();
            manager.Initialize();
            ServiceLocator.Register(manager);

            try
            {
                Assert.DoesNotThrow(() => UiSoundHooks.PlayClick());
            }
            finally
            {
                manager.Shutdown();
            }
        }

        [Test]
        public void PlayClick_PlaysConfiguredCue_WhenDatabaseHasIt()
        {
            var database = ScriptableObject.CreateInstance<AudioCueDatabase>();
            database.AddOrReplaceCue(new AudioCue(UiSoundHooks.ClickCueId, null, AudioCategory.Ui, 1f));

            var manager = new AudioManager(database);
            manager.Initialize();
            ServiceLocator.Register(manager);

            try
            {
                // Clip is null, so PlaySfx no-ops internally, but PlayOneShot must still resolve
                // the cue and run without throwing.
                Assert.DoesNotThrow(() => UiSoundHooks.PlayClick());
            }
            finally
            {
                manager.Shutdown();
                Object.DestroyImmediate(database);
            }
        }
    }
}
