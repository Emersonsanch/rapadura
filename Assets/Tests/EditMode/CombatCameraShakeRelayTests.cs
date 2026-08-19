using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests verifying that <see cref="CombatCameraShakeRelay"/> scales the shake
    /// magnitude it forwards to <see cref="PlayerCamera"/> by
    /// <see cref="AccessibilitySettings.ScaleShakeMagnitude"/> when an
    /// <see cref="AccessibilitySettings"/> instance is registered with the
    /// <see cref="ServiceLocator"/>, and falls back to the raw magnitude when it is not.
    /// </summary>
    public class CombatCameraShakeRelayTests
    {
        private const string ShakeMagnitudeFieldName = "_shakeMagnitude";

        private GameObject _go;
        private PlayerCamera _playerCamera;
        private CombatCameraShakeRelay _relay;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();

            _go = new GameObject("CombatCameraShakeRelayTestTarget");
            _playerCamera = _go.AddComponent<PlayerCamera>();
            _relay = _go.AddComponent<CombatCameraShakeRelay>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }

            ServiceLocator.Clear();
        }

        private float GetAppliedShakeMagnitude()
        {
            FieldInfo field = typeof(PlayerCamera).GetField(ShakeMagnitudeFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected PlayerCamera to have a private field named {ShakeMagnitudeFieldName}.");
            return (float)field.GetValue(_playerCamera);
        }

        [Test]
        public void OnCameraShakeRequested_WithAccessibilitySettingsRegistered_ScalesMagnitude()
        {
            var settings = new AccessibilitySettings();
            settings.Initialize();
            settings.SetScreenShakeIntensity(0.25f);
            ServiceLocator.Register(settings);

            EventBus.Publish(new CameraShakeRequestedEvent(0.5f, 2f));

            Assert.AreEqual(0.5f, GetAppliedShakeMagnitude(), 0.0001f);

            settings.Shutdown();
        }

        [Test]
        public void OnCameraShakeRequested_WithoutAccessibilitySettingsRegistered_UsesRawMagnitude()
        {
            Assert.IsFalse(ServiceLocator.IsRegistered<AccessibilitySettings>());

            EventBus.Publish(new CameraShakeRequestedEvent(0.5f, 2f));

            Assert.AreEqual(2f, GetAppliedShakeMagnitude(), 0.0001f);
        }

        [Test]
        public void OnCameraShakeRequested_ZeroIntensity_ProducesNoShake()
        {
            var settings = new AccessibilitySettings();
            settings.Initialize();
            settings.SetScreenShakeIntensity(0f);
            ServiceLocator.Register(settings);

            EventBus.Publish(new CameraShakeRequestedEvent(0.5f, 2f));

            Assert.AreEqual(0f, GetAppliedShakeMagnitude(), 0.0001f);

            settings.Shutdown();
        }
    }
}
