using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the settings "Controles" tab logic: binding classification / lookup /
    /// display formatting and conflict detection (<see cref="InputBindingUtility"/>), plus the
    /// row definitions and localization fallback (<see cref="SettingsMenuTexts"/>). Uses throwaway
    /// InputActionAssets shaped like PlayerControls.inputactions (empty groups, WASD composite),
    /// never the real asset.
    /// </summary>
    public class ControlsRebindTests
    {
        private InputActionAsset _asset;

        [TearDown]
        public void TearDown()
        {
            if (_asset != null)
            {
                Object.DestroyImmediate(_asset);
                _asset = null;
            }
        }

        private InputActionAsset CreatePlayerLikeAsset()
        {
            _asset = ScriptableObject.CreateInstance<InputActionAsset>();
            _asset.name = "RebindTestAsset";
            InputActionMap map = _asset.AddActionMap("Player");

            InputAction move = map.AddAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("up", "<Keyboard>/w")
                .With("down", "<Keyboard>/s")
                .With("left", "<Keyboard>/a")
                .With("right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            InputAction jump = map.AddAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");

            InputAction interact = map.AddAction("Interact", InputActionType.Button);
            interact.AddBinding("<Keyboard>/e");
            interact.AddBinding("<Gamepad>/buttonWest");

            InputAction attack = map.AddAction("Attack", InputActionType.Button);
            attack.AddBinding("<Mouse>/leftButton");
            attack.AddBinding("<Gamepad>/buttonNorth");

            return _asset;
        }

        // ------------------------------------------------------------------
        // Classification.
        // ------------------------------------------------------------------

        [Test]
        public void Classify_UsesPathDeviceLayout_WhenGroupsAreEmpty()
        {
            Assert.AreEqual(BindingDeviceKind.Keyboard, InputBindingUtility.Classify("<Keyboard>/space", ""));
            Assert.AreEqual(BindingDeviceKind.Keyboard, InputBindingUtility.Classify("<Mouse>/leftButton", null));
            Assert.AreEqual(BindingDeviceKind.Gamepad, InputBindingUtility.Classify("<Gamepad>/buttonSouth", ""));
            Assert.AreEqual(BindingDeviceKind.Gamepad, InputBindingUtility.Classify("<XInputController>/buttonSouth", ""));
            Assert.AreEqual(BindingDeviceKind.Other, InputBindingUtility.Classify("<Touchscreen>/primaryTouch/tap", ""));
            Assert.AreEqual(BindingDeviceKind.Other, InputBindingUtility.Classify("<OnScreenStick>/stick", ""));
            Assert.AreEqual(BindingDeviceKind.Other, InputBindingUtility.Classify("", ""));
        }

        [Test]
        public void Classify_ControlSchemeGroups_TakePrecedenceOverPath()
        {
            Assert.AreEqual(BindingDeviceKind.Gamepad, InputBindingUtility.Classify("<Keyboard>/space", "Gamepad"));
            Assert.AreEqual(BindingDeviceKind.Keyboard, InputBindingUtility.Classify("<Gamepad>/buttonSouth", "Touch;KeyboardMouse"));
        }

        [Test]
        public void ExtractDeviceLayout_ParsesAngleBracketPrefix()
        {
            Assert.AreEqual("Keyboard", InputBindingUtility.ExtractDeviceLayout("<Keyboard>/e"));
            Assert.IsNull(InputBindingUtility.ExtractDeviceLayout("*/{Submit}"));
            Assert.IsNull(InputBindingUtility.ExtractDeviceLayout(null));
        }

        [Test]
        public void GetRebindPathPrefixes_RestrictsEachColumnToItsDevice()
        {
            CollectionAssert.AreEqual(new[] { "<Keyboard>" }, InputBindingUtility.GetRebindPathPrefixes(BindingDeviceKind.Keyboard));
            CollectionAssert.AreEqual(new[] { "<Gamepad>" }, InputBindingUtility.GetRebindPathPrefixes(BindingDeviceKind.Gamepad));
            Assert.IsNull(InputBindingUtility.GetRebindPathPrefixes(BindingDeviceKind.Other));
        }

        // ------------------------------------------------------------------
        // Binding lookup.
        // ------------------------------------------------------------------

        [Test]
        public void FindBindingIndex_CompositePart_ReturnsThatPartsIndex()
        {
            InputAction move = CreatePlayerLikeAsset().FindAction("Move");

            int up = InputBindingUtility.FindBindingIndex(move, "up", BindingDeviceKind.Keyboard);
            int right = InputBindingUtility.FindBindingIndex(move, "right", BindingDeviceKind.Keyboard);

            Assert.AreEqual("<Keyboard>/w", move.bindings[up].path);
            Assert.AreEqual("<Keyboard>/d", move.bindings[right].path);
            Assert.AreEqual(-1, InputBindingUtility.FindBindingIndex(move, "up", BindingDeviceKind.Gamepad));
        }

        [Test]
        public void FindBindingIndex_WithoutPart_SkipsCompositeAndFindsStick()
        {
            InputAction move = CreatePlayerLikeAsset().FindAction("Move");

            int stick = InputBindingUtility.FindBindingIndex(move, null, BindingDeviceKind.Gamepad);

            Assert.AreEqual("<Gamepad>/leftStick", move.bindings[stick].path);
            Assert.AreEqual(-1, InputBindingUtility.FindBindingIndex(move, null, BindingDeviceKind.Keyboard));
        }

        [Test]
        public void FindBindingIndex_PlainAction_SplitsKeyboardAndGamepad()
        {
            InputActionAsset asset = CreatePlayerLikeAsset();
            InputAction attack = asset.FindAction("Attack");

            Assert.AreEqual(0, InputBindingUtility.FindBindingIndex(attack, null, BindingDeviceKind.Keyboard));
            Assert.AreEqual(1, InputBindingUtility.FindBindingIndex(attack, null, BindingDeviceKind.Gamepad));
            Assert.AreEqual(-1, InputBindingUtility.FindBindingIndex(null, null, BindingDeviceKind.Keyboard));
        }

        [Test]
        public void FindBindingIndex_UsesOriginalPath_AfterOverride()
        {
            InputAction jump = CreatePlayerLikeAsset().FindAction("Jump");
            jump.ApplyBindingOverride(0, "<Keyboard>/k");

            Assert.AreEqual(0, InputBindingUtility.FindBindingIndex(jump, null, BindingDeviceKind.Keyboard));
        }

        // ------------------------------------------------------------------
        // Display formatting.
        // ------------------------------------------------------------------

        [Test]
        public void FormatBindingDisplay_EmptyOrWhitespace_ShowsDash()
        {
            Assert.AreEqual(InputBindingUtility.EmptyBindingText, InputBindingUtility.FormatBindingDisplay(null, false));
            Assert.AreEqual(InputBindingUtility.EmptyBindingText, InputBindingUtility.FormatBindingDisplay("   ", true));
        }

        [Test]
        public void FormatBindingDisplay_TrimsAndMarksOverrides()
        {
            Assert.AreEqual("E", InputBindingUtility.FormatBindingDisplay("  E ", false));
            Assert.AreEqual("Space" + InputBindingUtility.OverriddenSuffix, InputBindingUtility.FormatBindingDisplay("Space", true));
        }

        [Test]
        public void IsOverridden_OnlyWhenOverrideDiffersFromOriginal()
        {
            InputAction jump = CreatePlayerLikeAsset().FindAction("Jump");

            Assert.IsFalse(InputBindingUtility.IsOverridden(jump, 0));

            jump.ApplyBindingOverride(0, "<Keyboard>/space");
            Assert.IsFalse(InputBindingUtility.IsOverridden(jump, 0), "Override equal to the original path isn't a real change.");

            jump.ApplyBindingOverride(0, "<Keyboard>/k");
            Assert.IsTrue(InputBindingUtility.IsOverridden(jump, 0));
            Assert.IsFalse(InputBindingUtility.IsOverridden(jump, -1));
        }

        [Test]
        public void GetFormattedDisplay_InvalidIndex_ShowsDash_OverriddenHasSuffix()
        {
            InputAction jump = CreatePlayerLikeAsset().FindAction("Jump");

            Assert.AreEqual(InputBindingUtility.EmptyBindingText, InputBindingUtility.GetFormattedDisplay(jump, -1));
            Assert.AreEqual(InputBindingUtility.EmptyBindingText, InputBindingUtility.GetFormattedDisplay(null, 0));

            jump.ApplyBindingOverride(0, "<Keyboard>/k");
            string display = InputBindingUtility.GetFormattedDisplay(jump, 0);
            StringAssert.EndsWith(InputBindingUtility.OverriddenSuffix, display);
            Assert.AreNotEqual(InputBindingUtility.EmptyBindingText + InputBindingUtility.OverriddenSuffix, display);
        }

        // ------------------------------------------------------------------
        // Conflict detection.
        // ------------------------------------------------------------------

        [Test]
        public void PathsConflict_IsCaseInsensitive_AndEmptyNeverConflicts()
        {
            Assert.IsTrue(InputBindingUtility.PathsConflict("<Keyboard>/E", "<keyboard>/e"));
            Assert.IsFalse(InputBindingUtility.PathsConflict("<Keyboard>/e", "<Keyboard>/f"));
            Assert.IsFalse(InputBindingUtility.PathsConflict(null, null));
            Assert.IsFalse(InputBindingUtility.PathsConflict("", ""));
        }

        [Test]
        public void FindConflicts_DefaultBindings_HaveNone()
        {
            InputAction jump = CreatePlayerLikeAsset().FindAction("Jump");

            Assert.IsEmpty(InputBindingUtility.FindConflicts(jump, 0));
            Assert.IsEmpty(InputBindingUtility.FindConflicts(jump, 1));
        }

        [Test]
        public void FindConflicts_OverrideToAnotherActionsKey_ReportsThatAction()
        {
            InputActionAsset asset = CreatePlayerLikeAsset();
            InputAction interact = asset.FindAction("Interact");
            interact.ApplyBindingOverride(0, "<Keyboard>/space");

            var conflicts = InputBindingUtility.FindConflicts(interact, 0);

            Assert.AreEqual(1, conflicts.Count);
            Assert.AreEqual("Jump", conflicts[0].ActionName);
            Assert.IsNull(conflicts[0].PartName);
            Assert.AreEqual(0, conflicts[0].BindingIndex);
        }

        [Test]
        public void FindConflicts_CompositePart_ReportsPartName()
        {
            InputActionAsset asset = CreatePlayerLikeAsset();
            InputAction move = asset.FindAction("Move");
            int up = InputBindingUtility.FindBindingIndex(move, "up", BindingDeviceKind.Keyboard);
            int down = InputBindingUtility.FindBindingIndex(move, "down", BindingDeviceKind.Keyboard);
            move.ApplyBindingOverride(up, "<Keyboard>/s");

            var conflicts = InputBindingUtility.FindConflicts(move, up);

            Assert.AreEqual(1, conflicts.Count);
            Assert.AreEqual("Move", conflicts[0].ActionName);
            Assert.AreEqual("down", conflicts[0].PartName);
            Assert.AreEqual(down, conflicts[0].BindingIndex);
        }

        [Test]
        public void FindConflicts_DifferentDevicesNeverConflict()
        {
            InputActionAsset asset = CreatePlayerLikeAsset();
            InputAction interact = asset.FindAction("Interact");
            interact.ApplyBindingOverride(1, "<Gamepad>/buttonSouth");

            var conflicts = InputBindingUtility.FindConflicts(interact, 1);

            Assert.AreEqual(1, conflicts.Count);
            Assert.AreEqual("Jump", conflicts[0].ActionName);
            Assert.AreEqual(1, conflicts[0].BindingIndex, "Only Jump's gamepad binding should match, not its keyboard one.");
        }

        [Test]
        public void FindConflicts_InvalidInput_ReturnsEmpty()
        {
            InputAction jump = CreatePlayerLikeAsset().FindAction("Jump");

            Assert.IsEmpty(InputBindingUtility.FindConflicts(null, 0));
            Assert.IsEmpty(InputBindingUtility.FindConflicts(jump, 99));
        }

        // ------------------------------------------------------------------
        // Row definitions / localization.
        // ------------------------------------------------------------------

        [Test]
        public void Rows_CoverEveryRebindableAction()
        {
            var actions = new System.Collections.Generic.HashSet<string>();
            foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
            {
                actions.Add(row.ActionName);
            }

            foreach (string expected in new[] { "Move", "Jump", "Run", "Crouch", "Interact", "Attack", "Skill1", "Skill2", "Skill3", "Skill4", "Skill5" })
            {
                Assert.IsTrue(actions.Contains(expected), $"Missing row for {expected}");
            }
        }

        [Test]
        public void Rows_EveryLabelKeyHasAFallbackText()
        {
            foreach (RebindRowDefinition row in SettingsMenuTexts.Rows)
            {
                Assert.IsFalse(string.IsNullOrEmpty(SettingsMenuTexts.GetFallback(row.LabelKey)), row.LabelKey);
            }
        }

        [Test]
        public void Get_WithoutLocalizationManager_UsesPortugueseFallback()
        {
            Assert.AreEqual("Pular", SettingsMenuTexts.Get(null, "ui.settings.controls.action.jump"));
            Assert.AreEqual("unknown.key", SettingsMenuTexts.Get(null, "unknown.key"));
        }

        [Test]
        public void Format_ConflictMessage_IncludesKeyAndActionNames()
        {
            string message = SettingsMenuTexts.Format(null, SettingsMenuTexts.StatusConflict, "Space", "Pular");

            StringAssert.Contains("Space", message);
            StringAssert.Contains("Pular", message);
        }

        [Test]
        public void DescribeConflict_UsesRowLabel_ForKnownActionAndPart()
        {
            Assert.AreEqual("Mover para baixo", SettingsMenuTexts.DescribeConflict(null, new BindingConflict("Move", "down", 2)));
            Assert.AreEqual("Pular", SettingsMenuTexts.DescribeConflict(null, new BindingConflict("Jump", null, 0)));
            Assert.AreEqual("MoveClick", SettingsMenuTexts.DescribeConflict(null, new BindingConflict("MoveClick", null, 0)));
        }

        [Test]
        public void ColorblindKey_EveryModeHasAFallbackText()
        {
            foreach (ColorblindMode mode in (ColorblindMode[])System.Enum.GetValues(typeof(ColorblindMode)))
            {
                Assert.IsNotNull(SettingsMenuTexts.GetFallback(SettingsMenuTexts.ColorblindKey(mode)), mode.ToString());
            }
        }
    }
}
