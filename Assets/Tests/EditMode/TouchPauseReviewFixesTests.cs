using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Cameras;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Encounter;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UIElements;
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Third review pass (touch / pause): stuck joystick and Correr after an app switch, the trailing
    /// crafting-hint probe, the "Prepare-se!" banner on scaled time and hidden while paused, the
    /// cached UIDocument list of <see cref="UiPointerQuery"/>, pinch-zoom paused / pair-by-id, the
    /// Esc poll respecting a rebound Pause, plus the r4 playtest HUD items (hotbar names follow a
    /// live language change, camera buttons off the quest tracker, cooldown digits over the name).
    /// The pinch latch itself is covered in <see cref="TouchControlsTests"/>.
    /// </summary>
    public class TouchPauseReviewFixesTests
    {
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            GameplayPause.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            GameplayPause.Reset();
            EventBus.Clear();
        }

        private static object Invoke(object target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(info, $"Expected private method '{method}' on {target.GetType().Name}.");
            return info.Invoke(target, args);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        // ------------------------------------------------------------------
        // 2. Joystick / Correr stuck after Home / app switch
        // ------------------------------------------------------------------

        [Test]
        public void IsTouchControlStuck_OnlyWhenHeldWithNoFingerAndNoMouseButton()
        {
            Assert.IsTrue(HudController.IsTouchControlStuck(held: true, activeTouchCount: 0, mouseButtonPressed: false));
            Assert.IsFalse(HudController.IsTouchControlStuck(true, 1, false), "Thumb still on the glass.");
            Assert.IsFalse(HudController.IsTouchControlStuck(true, 0, true), "Editor: the mouse is driving the joystick.");
            Assert.IsFalse(HudController.IsTouchControlStuck(false, 0, false), "Nothing held: nothing to release.");
        }

        [TestCase("OnApplicationPause", true)]
        [TestCase("OnApplicationFocus", false)]
        public void AppSwitch_ReleasesJoystickAndRun(string callback, bool argument)
        {
            _host = new GameObject("Hud");
            HudController hud = _host.AddComponent<HudController>();
            PlayerInputHandler input = _host.AddComponent<PlayerInputHandler>();
            SetPrivateField(hud, "_playerInput", input);
            SetPrivateField(hud, "_joystickPointerId", 3);
            input.TouchRunHeld = true;
            input.TouchMoveInput = new Vector2(0.5f, 0.5f);

            Invoke(hud, callback, argument);

            Assert.IsFalse(input.TouchRunHeld, "Correr released.");
            Assert.AreEqual(Vector2.zero, input.TouchMoveInput, "Joystick released.");
            Assert.AreEqual(-1, GetPrivateField<int>(hud, "_joystickPointerId"));
        }

        [Test]
        public void AppResume_DoesNotReleaseAnything()
        {
            _host = new GameObject("Hud");
            HudController hud = _host.AddComponent<HudController>();
            PlayerInputHandler input = _host.AddComponent<PlayerInputHandler>();
            SetPrivateField(hud, "_playerInput", input);
            input.TouchRunHeld = true;

            Invoke(hud, "OnApplicationPause", false);
            Invoke(hud, "OnApplicationFocus", true);

            Assert.IsTrue(input.TouchRunHeld);
        }

        // ------------------------------------------------------------------
        // 3. Crafting hint: trailing probe of a throttled burst
        // ------------------------------------------------------------------

        [Test]
        public void CraftableProbe_TrailingChangeOfABurst_IsProbedOnTickAfterTheInterval()
        {
            float now = 10f;
            bool craftable = false;
            int probes = 0;
            var manager = new TutorialManager(TutorialSteps.All)
            {
                CraftableRecipeProbe = () => { probes++; return craftable; },
                Clock = () => now,
            };
            manager.Initialize();

            try
            {
                EventBus.Publish(new InventoryChangedEvent());
                Assert.AreEqual(1, probes);
                Assert.IsFalse(manager.HasPendingCraftableCheck);

                // The last loot of the burst completes the materials, inside the interval.
                craftable = true;
                now += 0.2f;
                EventBus.Publish(new InventoryChangedEvent());
                Assert.AreEqual(1, probes, "throttled");
                Assert.IsTrue(manager.HasPendingCraftableCheck, "skipped change remembered");

                now += 0.3f;
                manager.Tick();
                Assert.AreEqual(1, probes, "interval not over yet");

                now += 0.6f;
                manager.Tick();
                Assert.AreEqual(2, probes, "trailing probe once the interval elapsed");
                Assert.IsTrue(manager.IsPending(TutorialSteps.Crafting), "the hint fires without another inventory change");
                Assert.IsFalse(manager.HasPendingCraftableCheck);

                now += 5f;
                manager.Tick();
                Assert.AreEqual(2, probes, "idle Tick never probes");
            }
            finally
            {
                manager.Shutdown();
            }
        }

        [Test]
        public void CraftableProbe_NextChangeAfterTheInterval_AlsoClearsThePendingCheck()
        {
            float now = 0f;
            int probes = 0;
            var manager = new TutorialManager(TutorialSteps.All)
            {
                CraftableRecipeProbe = () => { probes++; return false; },
                Clock = () => now,
            };
            manager.Initialize();

            try
            {
                EventBus.Publish(new InventoryChangedEvent());
                EventBus.Publish(new InventoryChangedEvent());
                Assert.IsTrue(manager.HasPendingCraftableCheck);

                now += 1.5f;
                EventBus.Publish(new InventoryChangedEvent());
                Assert.AreEqual(2, probes);
                Assert.IsFalse(manager.HasPendingCraftableCheck);

                manager.Tick();
                Assert.AreEqual(2, probes, "no duplicate trailing probe");
            }
            finally
            {
                manager.Shutdown();
            }
        }

        // ------------------------------------------------------------------
        // 4. "Prepare-se!" banner: scaled time, held and hidden while paused
        // ------------------------------------------------------------------

        [Test]
        public void BannerClock_FollowsScaledTime_AndHoldsWhilePaused()
        {
            Assert.AreEqual(1.5f, BossIntroBannerController.AdvanceClock(2f, 0.5f, paused: false), 1e-5f);
            Assert.AreEqual(2f, BossIntroBannerController.AdvanceClock(2f, 0.5f, paused: true), 1e-5f, "pause menu / modal: clock held");
            Assert.AreEqual(2f, BossIntroBannerController.AdvanceClock(2f, 0f, paused: false), 1e-5f, "timeScale 0 → deltaTime 0");
            Assert.AreEqual(2f, BossIntroBannerController.AdvanceClock(2f, -1f, paused: false), 1e-5f);
        }

        [Test]
        public void BannerClock_StaysInStepWithTheEnemyAggroHold_AcrossAPause()
        {
            var enemyGo = new GameObject("Enemy");
            _host = enemyGo;
            var enemy = enemyGo.AddComponent<Rapadura.Gameplay.Enemies.EnemyController>();
            enemy.HoldAggro(1.75f);

            float banner = 1.75f;
            for (int frame = 0; frame < 30; frame++)
            {
                bool paused = frame >= 5 && frame < 20; // pause menu opened mid-countdown
                float scaledDelta = paused ? 0f : 0.05f;
                enemy.TickBrain(scaledDelta);
                banner = BossIntroBannerController.AdvanceClock(banner, scaledDelta, paused);
            }

            Assert.AreEqual(enemy.AggroHoldRemaining, Mathf.Max(0f, banner), 1e-4f, "the countdown never runs ahead of the enemies' grace");
            Assert.AreEqual(BossIntroBannerController.CountdownValue(enemy.AggroHoldRemaining), BossIntroBannerController.CountdownValue(banner));
        }

        [Test]
        public void Banner_IsHiddenWhilePaused()
        {
            Assert.IsTrue(BossIntroBannerController.ShouldDisplay(active: true, paused: false));
            Assert.IsFalse(BossIntroBannerController.ShouldDisplay(true, true), "sort 850 would draw over the pause menu / modals");
            Assert.IsFalse(BossIntroBannerController.ShouldDisplay(false, false));
        }

        // ------------------------------------------------------------------
        // 5. UiPointerQuery: cached documents, zero allocations per query
        // ------------------------------------------------------------------

        [Test]
        public void UiPointerQuery_Register_IsCachedImmediately_UnregisterForgets()
        {
            _host = new GameObject("Doc");
            UIDocument document = _host.AddComponent<UIDocument>();

            UiPointerQuery.Register(document);
            UiPointerQuery.Register(document);
            Assert.IsTrue(UiPointerQuery.IsCached(document));

            UiPointerQuery.Unregister(document);
            Assert.IsFalse(UiPointerQuery.IsCached(document));
            UiPointerQuery.Register(null);
            UiPointerQuery.Unregister(null);
        }

        [Test]
        public void UiPointerQuery_ScanScene_FindsInactiveDocuments_AndDropsDestroyedOnes()
        {
            _host = new GameObject("InactiveDoc");
            _host.SetActive(false);
            UIDocument document = _host.AddComponent<UIDocument>();

            UiPointerQuery.ScanScene();
            Assert.IsTrue(UiPointerQuery.IsCached(document), "a screen enabled later is already known");
            int withDocument = UiPointerQuery.CachedDocumentCount;

            Object.DestroyImmediate(_host);
            _host = null;
            Assert.IsFalse(UiPointerQuery.IsPointerOverUi(new Vector2(10f, 10f)), "destroyed entries are skipped");

            UiPointerQuery.ScanScene();
            Assert.AreEqual(withDocument - 1, UiPointerQuery.CachedDocumentCount);
        }

        [Test]
        public void UiPointerQuery_SceneLoadSchedulesRescans_QueriesDoNotSearchTheScene()
        {
            UiPointerQuery.IsPointerOverUi(Vector2.zero); // first query: subscribe + scan
            UiPointerQuery.CancelScheduledRescans();
            int scans = UiPointerQuery.ScanCount;

            for (int i = 0; i < 20; i++)
            {
                UiPointerQuery.IsPointerOverUi(new Vector2(i, i));
            }

            Assert.AreEqual(scans, UiPointerQuery.ScanCount, "no FindObjectsByType per call");

            UiPointerQuery.InvalidateDocumentCache();
            Assert.AreEqual(UiPointerQuery.RescanDelays.Length, UiPointerQuery.PendingRescanCount);
            UiPointerQuery.IsPointerOverUi(Vector2.zero);
            Assert.AreEqual(scans + 1, UiPointerQuery.ScanCount, "the immediate rescan runs on the next query");
            UiPointerQuery.CancelScheduledRescans();
        }

        [Test]
        public void UiPointerQuery_SteadyStateQuery_DoesNotAllocate()
        {
            _host = new GameObject("Docs");
            var inactive = new GameObject("InactiveDoc");
            inactive.transform.SetParent(_host.transform);
            inactive.SetActive(false);
            UiPointerQuery.Register(inactive.AddComponent<UIDocument>());
            UiPointerQuery.Register(_host.AddComponent<UIDocument>());

            UiPointerQuery.IsPointerOverUi(new Vector2(5f, 5f)); // warm-up (subscribe + first scan)
            UiPointerQuery.CancelScheduledRescans();

            // Explicit TestDelegate: a bool-returning lambda would be treated as an ActualValueDelegate.
            Assert.That((TestDelegate)(() => { UiPointerQuery.IsPointerOverUi(new Vector2(5f, 5f)); }), AllocIs.Not.AllocatingGCMemory());
        }

        [Test]
        public void UiClickSoundBinderRegister_AlsoFeedsThePointerQueryCache()
        {
            _host = new GameObject("BootstrappedDoc");
            UIDocument document = _host.AddComponent<UIDocument>();

            Rapadura.Audio.UiClickSoundBinder.Register(document);
            try
            {
                Assert.IsTrue(UiPointerQuery.IsCached(document));
            }
            finally
            {
                Rapadura.Audio.UiClickSoundBinder.Unregister(document);
            }

            Assert.IsFalse(UiPointerQuery.IsCached(document));
        }

        // ------------------------------------------------------------------
        // 6. Pinch-zoom: never while paused, pair tracked by touch id
        // ------------------------------------------------------------------

        private static CameraZoom CreateZoom(out CameraSettings settings)
        {
            settings = ScriptableObject.CreateInstance<CameraSettings>();
            settings.minZoom = 2f;
            settings.maxZoom = 40f;
            settings.defaultZoom = 10f;
            return new CameraZoom(settings);
        }

        [Test]
        public void Pinch_SamePair_Zooms()
        {
            CameraZoom zoom = CreateZoom(out CameraSettings settings);
            try
            {
                zoom.ApplyInput(false, true, 1, 2, new Vector2(100f, 100f), new Vector2(200f, 100f), 0f, 0.016f);
                Assert.AreEqual(10f, zoom.TargetDistance, 1e-4f, "first frame only sets the baseline");

                zoom.ApplyInput(false, true, 2, 1, new Vector2(300f, 100f), new Vector2(100f, 100f), 0f, 0.016f);
                Assert.AreEqual(10f - 100f * 0.02f, zoom.TargetDistance, 1e-4f, "spread 100px → zoom in (order of ids irrelevant)");
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void Pinch_PairChanges_Rebaselines_WithoutAJump()
        {
            CameraZoom zoom = CreateZoom(out CameraSettings settings);
            try
            {
                zoom.ApplyInput(false, true, 1, 2, new Vector2(100f, 100f), new Vector2(120f, 100f), 0f, 0.016f);

                // Finger 1 lifts while 2 and 3 stay: a new, much wider pair — must not read as a 500px spread.
                zoom.ApplyInput(false, true, 2, 3, new Vector2(120f, 100f), new Vector2(620f, 100f), 0f, 0.016f);
                Assert.AreEqual(10f, zoom.TargetDistance, 1e-4f);

                zoom.ApplyInput(false, true, 2, 3, new Vector2(120f, 100f), new Vector2(570f, 100f), 0f, 0.016f);
                Assert.AreEqual(10f + 50f * 0.02f, zoom.TargetDistance, 1e-4f, "then the new pair zooms normally");
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void Pinch_WhilePaused_DoesNothing_AndResumesFromAFreshBaseline()
        {
            CameraZoom zoom = CreateZoom(out CameraSettings settings);
            try
            {
                zoom.ApplyInput(false, true, 1, 2, new Vector2(100f, 100f), new Vector2(200f, 100f), 0f, 0.016f);

                zoom.ApplyInput(true, true, 1, 2, new Vector2(0f, 100f), new Vector2(600f, 100f), 0f, 0.016f);
                zoom.ApplyInput(true, false, -1, -1, Vector2.zero, Vector2.zero, 5f, 0.016f);
                Assert.AreEqual(10f, zoom.TargetDistance, 1e-4f, "pause menu / full-screen modal: no zoom");

                zoom.ApplyInput(false, true, 1, 2, new Vector2(0f, 100f), new Vector2(600f, 100f), 0f, 0.016f);
                Assert.AreEqual(10f, zoom.TargetDistance, 1e-4f, "unpausing mid-pinch doesn't apply the distance moved while paused");
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        // ------------------------------------------------------------------
        // 7. Esc / Android Back poll vs a rebound Pause
        // ------------------------------------------------------------------

        [Test]
        public void ShouldPollEscape_TouchSchemeOrEscStillBound()
        {
            Assert.IsTrue(HudController.ShouldPollEscapeForPause(false, null, false), "no Pause action: Esc is the only way in");
            Assert.IsTrue(HudController.ShouldPollEscapeForPause(true, HudController.TouchControlScheme, false), "Android Back under Touch");
            Assert.IsTrue(HudController.ShouldPollEscapeForPause(true, "KeyboardMouse", true), "desktop default binding (deduped with the action)");
            Assert.IsFalse(HudController.ShouldPollEscapeForPause(true, "KeyboardMouse", false), "desktop rebound Pause to another key");
            Assert.IsFalse(HudController.ShouldPollEscapeForPause(true, null, false));
        }

        [Test]
        public void IsEscapeBoundTo_FollowsBindingOverrides()
        {
            var pause = new InputAction(HudController.PauseActionName, binding: HudController.EscapeBindingPath);
            pause.AddBinding("<Gamepad>/start");
            try
            {
                Assert.IsTrue(HudController.IsEscapeBoundTo(pause));

                pause.ApplyBindingOverride(0, "<Keyboard>/p");
                Assert.IsFalse(HudController.IsEscapeBoundTo(pause), "rebound in Settings > Controles");

                pause.RemoveAllBindingOverrides();
                Assert.IsTrue(HudController.IsEscapeBoundTo(pause));
                Assert.IsFalse(HudController.IsEscapeBoundTo(null));
            }
            finally
            {
                pause.Dispose();
            }
        }

        [Test]
        public void PlayerControls_DeclaresTheTouchScheme_AndEscOnPause()
        {
            InputActionAsset asset = InputActionAsset.FromJson(File.ReadAllText(Path.Combine("Assets", "InputActions", "PlayerControls.inputactions")));
            try
            {
                bool hasTouch = false;
                foreach (InputControlScheme scheme in asset.controlSchemes)
                {
                    hasTouch |= scheme.name == HudController.TouchControlScheme;
                }

                Assert.IsTrue(hasTouch);
                Assert.IsTrue(HudController.IsEscapeBoundTo(asset.FindAction(HudController.PauseActionName, throwIfNotFound: true)));
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        // ------------------------------------------------------------------
        // r4 playtest HUD items
        // ------------------------------------------------------------------

        [Test]
        public void LanguageChange_ForgetsTheCachedHotbarSkills()
        {
            _host = new GameObject("Hud");
            HudController hud = _host.AddComponent<HudController>();
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            try
            {
                SkillDefinition[] shown = GetPrivateField<SkillDefinition[]>(hud, "_skillSlotShownSkill");
                shown[0] = skill;
                shown[shown.Length - 1] = skill;

                Invoke(hud, "InvalidateSkillSlotNames");

                foreach (SkillDefinition cached in shown)
                {
                    Assert.IsNull(cached, "every slot re-applies its (re-localized) name on the next refresh");
                }
            }
            finally
            {
                Object.DestroyImmediate(skill);
            }
        }

        private static string UssBlock(string uss, string selector)
        {
            Match match = Regex.Match(uss, Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(match.Success, selector);
            return match.Groups["body"].Value;
        }

        [Test]
        public void CameraButtons_SitAboveTheHotbar_NotUnderTheQuestTracker()
        {
            string uss = File.ReadAllText(Path.Combine("Assets", "UI", "HUD", "HudView.uss"));
            string cluster = UssBlock(uss, ".hud-camera-cluster");
            StringAssert.DoesNotMatch(@"(^|[\s;])top\s*:", cluster, "a top offset lands on the QuestTracker card (right edge, top 130px)");

            Match bottom = Regex.Match(cluster, @"bottom\s*:\s*(\d+)px");
            Assert.IsTrue(bottom.Success);
            Match hotbarBottom = Regex.Match(UssBlock(uss, ".hud-skill-hotbar"), @"bottom\s*:\s*(\d+)px");
            Match slotHeight = Regex.Match(UssBlock(uss, ".hud-skill-button"), @"height\s*:\s*(\d+)px");
            Assert.GreaterOrEqual(int.Parse(bottom.Groups[1].Value), int.Parse(hotbarBottom.Groups[1].Value) + int.Parse(slotHeight.Groups[1].Value),
                "clears the top of the skill hotbar");
        }

        [Test]
        public void CooldownDigits_HideTheSkillNameUnderThem()
        {
            string uss = File.ReadAllText(Path.Combine("Assets", "UI", "HUD", "HudView.uss"));
            StringAssert.Contains("visibility: hidden", UssBlock(uss, ".hud-skill-button--cooldown .hud-skill-name"));
            StringAssert.Contains("middle-center", UssBlock(uss, ".hud-skill-cooldown-label"), "the countdown stays centered");
        }
    }
}
