using NUnit.Framework;
using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Playtest regression: rain rendered as a big stretched pale-blue quad on the map and in the
    /// arena (emitter column centred on the camera, Hierarchy scaling, no on-screen size cap).
    /// Covers <see cref="WeatherVisuals.ConfigurePrecipitation"/> and the emitter anchor fallback.
    /// </summary>
    public class WeatherVisualsTests
    {
        private GameObject _go;
        private ParticleSystem _system;
        private ParticleSystemRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("WeatherVisualsTestSystem");
            _system = _go.AddComponent<ParticleSystem>();
            _renderer = _go.GetComponent<ParticleSystemRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Rain_IsThinStretchedStreak_CappedOnScreen()
        {
            WeatherVisuals.ConfigurePrecipitation(WeatherType.Rain, _system, _renderer);

            Assert.AreEqual(ParticleSystemRenderMode.Stretch, _renderer.renderMode);
            Assert.LessOrEqual(_system.main.startSize.constant, 0.03f, "Drops must be thin.");

            float streakLength = _system.main.startSize.constant * _renderer.lengthScale
                + _system.main.startSpeed.constant * _renderer.velocityScale;
            Assert.That(streakLength, Is.InRange(0.3f, 1.5f), "Streak length at fall speed should read as a drop, not a sheet.");
            Assert.AreEqual(0f, _renderer.cameraVelocityScale, "Camera motion must not stretch drops.");
            AssertSafeScaling();
        }

        [Test]
        public void SnowAndSand_AreSmallAndCapped()
        {
            WeatherVisuals.ConfigurePrecipitation(WeatherType.Snow, _system, _renderer);
            Assert.AreEqual(ParticleSystemRenderMode.Billboard, _renderer.renderMode);
            Assert.LessOrEqual(_system.main.startSize.constantMax, 0.15f);
            AssertSafeScaling();

            WeatherVisuals.ConfigurePrecipitation(WeatherType.Sandstorm, _system, _renderer);
            Assert.AreEqual(ParticleSystemRenderMode.Stretch, _renderer.renderMode);
            Assert.LessOrEqual(_system.main.startSize.constantMax, 0.06f);
            AssertSafeScaling();
        }

        [Test]
        public void CameraGroundPoint_TopDownCamera_LandsOnGroundAhead_NotAtTheLens()
        {
            Vector3 cameraPosition = new Vector3(0f, 14f, -10f);
            Vector3 forward = new Vector3(0f, -14f, 10f).normalized;

            Vector3 anchor = WeatherVisuals.ResolveCameraGroundPoint(cameraPosition, forward, 0f);

            Assert.That(Vector3.Distance(anchor, Vector3.zero), Is.LessThan(0.01f));
        }

        [Test]
        public void CameraGroundPoint_LevelCamera_DropsLookAheadToGround()
        {
            Vector3 anchor = WeatherVisuals.ResolveCameraGroundPoint(new Vector3(0f, 2f, 0f), Vector3.forward, 0f);

            Assert.AreEqual(new Vector3(0f, 0f, 6f), anchor);
        }

        private void AssertSafeScaling()
        {
            Assert.AreEqual(ParticleSystemScalingMode.Local, _system.main.scalingMode, "Parent scale must not inflate drops.");
            Assert.LessOrEqual(_renderer.maxParticleSize, WeatherVisuals.MaxParticleScreenSize);
        }
    }
}
