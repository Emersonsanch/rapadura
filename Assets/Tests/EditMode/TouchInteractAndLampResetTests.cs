using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.UI.HUD;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Review fixes: the touch "Usar" button (<see cref="PlayerInputHandler.TouchInteractRequested"/>; before it, no [E]
    /// point worked on a phone) and <see cref="LimiarLampProgress"/> as an <see cref="Rapadura.Core.Interfaces.IResettable"/>
    /// (lamps lit in one slot leaked into a save without the "LimiarLamps" key).
    /// </summary>
    public class TouchInteractAndLampResetTests
    {
        private const string ActionsPath = "Assets/InputActions/PlayerControls.inputactions";

        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            GameplayPause.Reset();
            InteractPromptRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            GameplayPause.Reset();
            InteractPromptRegistry.Clear();
            EventBus.Clear();
        }

        private static void InvokePrivate(object target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(info, $"Expected private method '{method}' on {target.GetType().Name}.");
            info.Invoke(target, args);
        }

        private PlayerInputHandler CreateInput()
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
            Assert.IsNotNull(actions, ActionsPath);
            _host = new GameObject("Player");
            _host.SetActive(false);
            PlayerInputHandler input = _host.AddComponent<PlayerInputHandler>();
            _host.GetComponent<PlayerInput>().actions = actions;
            InvokePrivate(input, "Awake");
            return input;
        }

        // ------------------------------------------------------------------
        // Touch "Usar"
        // ------------------------------------------------------------------

        [Test]
        public void ReadOneShot_TouchRequest_TrueOnce_ThenConsumed()
        {
            bool request = true;
            Assert.IsTrue(PlayerInputHandler.ReadOneShot(false, ref request));
            Assert.IsFalse(request, "Consumed.");
            Assert.IsFalse(PlayerInputHandler.ReadOneShot(false, ref request), "Next frame: nothing.");
            Assert.IsTrue(PlayerInputHandler.ReadOneShot(true, ref request), "The physical key still works.");
        }

        [Test]
        public void TouchInteractRequested_IsInteractForOneFrame()
        {
            PlayerInputHandler input = CreateInput();

            input.TouchInteractRequested = true;
            InvokePrivate(input, "Update");
            Assert.IsTrue(input.InteractPressedThisFrame, "The frame of the tap.");
            Assert.IsFalse(input.TouchInteractRequested, "Consumed.");

            InvokePrivate(input, "Update");
            Assert.IsFalse(input.InteractPressedThisFrame, "Only one frame.");
        }

        [Test]
        public void TouchInteractRequested_WhilePaused_IsDropped()
        {
            PlayerInputHandler input = CreateInput();
            GameplayPause.Acquire(this);
            try
            {
                input.TouchInteractRequested = true;
                InvokePrivate(input, "Update");
                Assert.IsFalse(input.InteractPressedThisFrame);
                Assert.IsFalse(input.TouchInteractRequested, "Dropped, not kept for after the pause.");
            }
            finally
            {
                GameplayPause.Release(this);
            }
        }

        [Test]
        public void HudUseButton_SetsTouchInteractRequested()
        {
            _host = new GameObject("Hud");
            HudController hud = _host.AddComponent<HudController>();
            PlayerInputHandler input = _host.AddComponent<PlayerInputHandler>();
            typeof(HudController).GetField("_playerInput", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(hud, input);

            InvokePrivate(hud, "OnUseButtonPressed", new object[] { null });

            Assert.IsTrue(input.TouchInteractRequested);
        }

        [Test]
        public void HudUseButton_IsNotNamedLikeAPlayerInputAction()
        {
            // PlayerInput SendMessages "On" + action name to every component: a same-named method with
            // another parameter type throws MissingMethodException.
            Assert.IsNull(typeof(HudController).GetMethod("OnInteract", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            Assert.IsNull(typeof(PlayerInputHandler).GetMethod("OnInteract", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
        }

        [Test]
        public void InteractPromptRegistry_AvailableOnlyWhileHeld()
        {
            Assert.IsFalse(InteractPromptRegistry.IsAvailable(0f));
            InteractPromptRegistry.Ping(10f, 3f);
            Assert.IsTrue(InteractPromptRegistry.IsAvailable(12.9f));
            Assert.IsFalse(InteractPromptRegistry.IsAvailable(13.1f));
            InteractPromptRegistry.Ping(11f, 0.5f);
            Assert.IsTrue(InteractPromptRegistry.IsAvailable(12.9f), "A shorter ping never cuts a longer one.");
            InteractPromptRegistry.Clear();
            Assert.IsFalse(InteractPromptRegistry.IsAvailable(12.9f));
        }

        // ------------------------------------------------------------------
        // Limiar lamps reset
        // ------------------------------------------------------------------

        [Test]
        public void LimiarLampProgress_ResetForNewSession_ClearsLamps()
        {
            var progress = new LimiarLampProgress((NewSessionReset)null);
            progress.Light("a");
            progress.Light("b");

            progress.ResetForNewSession();

            Assert.AreEqual(0, progress.Lit.Count);
        }

        [Test]
        public void LimiarLampProgress_RegistersInSessionReset_ResetAllClearsIt()
        {
            var reset = new NewSessionReset();
            var progress = new LimiarLampProgress(reset);
            progress.Light("a");

            CollectionAssert.Contains(reset.Resettables, progress);
            reset.ResetAll();

            Assert.AreEqual(0, progress.Lit.Count, "A save without LimiarLamps starts with no lamp lit.");
        }

        [Test]
        public void LimiarLampProgress_NewInstance_ReplacesThePreviousRegistration()
        {
            var reset = new NewSessionReset();
            var first = new LimiarLampProgress(reset);
            var second = new LimiarLampProgress(reset);

            CollectionAssert.DoesNotContain(reset.Resettables, first, "A destroyed driver's progress does not pile up.");
            CollectionAssert.Contains(reset.Resettables, second);
        }
    }
}
