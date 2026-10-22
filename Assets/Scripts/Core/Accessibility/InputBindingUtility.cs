using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Rapadura.Core.Accessibility
{
    /// <summary>Which column of the "Controles" settings tab a binding belongs to.</summary>
    public enum BindingDeviceKind
    {
        /// <summary>Keyboard (and mouse buttons, e.g. Attack's default &lt;Mouse&gt;/leftButton).</summary>
        Keyboard,
        Gamepad,
        /// <summary>Touch / on-screen controls / pointer — never shown or rebound by the settings UI.</summary>
        Other
    }

    /// <summary>One other binding that resolves to the same control as the binding being checked.</summary>
    public readonly struct BindingConflict
    {
        public readonly string ActionName;
        /// <summary>Composite part name (e.g. "up" for Move/WASD), or null for a plain binding.</summary>
        public readonly string PartName;
        public readonly int BindingIndex;

        public BindingConflict(string actionName, string partName, int bindingIndex)
        {
            ActionName = actionName;
            PartName = partName;
            BindingIndex = bindingIndex;
        }
    }

    /// <summary>
    /// Pure helpers used by the settings "Controles" tab on top of <see cref="InputRebindManager"/>:
    /// classify bindings into keyboard/gamepad columns, find the binding index for a given
    /// action (+ composite part) and device column, format display strings, and detect conflicts
    /// (two bindings in the same action map resolving to the same control path).
    ///
    /// Classification is by the binding's ORIGINAL path (and groups when present) rather than its
    /// override, so a keyboard slot stays a keyboard slot after being rebound. PlayerControls'
    /// bindings currently have empty <c>groups</c>, so the path's device layout is what decides.
    /// </summary>
    public static class InputBindingUtility
    {
        /// <summary>Shown when a slot has no binding at all.</summary>
        public const string EmptyBindingText = "—";

        /// <summary>Suffix appended to a binding the player has overridden (visible on touch too, unlike hover hints).</summary>
        public const string OverriddenSuffix = " *";

        public const string KeyboardPathPrefix = "<Keyboard>";
        public const string GamepadPathPrefix = "<Gamepad>";

        private static readonly HashSet<string> KeyboardLayouts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Keyboard", "Mouse"
        };

        private static readonly HashSet<string> GamepadLayouts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Gamepad", "XInputController", "DualShockGamepad", "DualSenseGamepadHID", "SwitchProControllerHID", "Joystick"
        };

        /// <summary>Classifies a binding by its control-scheme groups (if any) or its path's device layout.</summary>
        public static BindingDeviceKind Classify(string path, string groups)
        {
            if (!string.IsNullOrEmpty(groups))
            {
                foreach (string rawGroup in groups.Split(';'))
                {
                    string group = rawGroup.Trim();
                    if (group.Equals("Gamepad", StringComparison.OrdinalIgnoreCase))
                    {
                        return BindingDeviceKind.Gamepad;
                    }
                    if (group.Equals("KeyboardMouse", StringComparison.OrdinalIgnoreCase) || group.Equals("Keyboard", StringComparison.OrdinalIgnoreCase))
                    {
                        return BindingDeviceKind.Keyboard;
                    }
                }
            }

            string layout = ExtractDeviceLayout(path);
            if (layout == null)
            {
                return BindingDeviceKind.Other;
            }
            if (KeyboardLayouts.Contains(layout))
            {
                return BindingDeviceKind.Keyboard;
            }
            if (GamepadLayouts.Contains(layout))
            {
                return BindingDeviceKind.Gamepad;
            }
            return BindingDeviceKind.Other;
        }

        /// <summary>"&lt;Keyboard&gt;/space" -> "Keyboard". Returns null when the path doesn't start with a &lt;layout&gt;.</summary>
        public static string ExtractDeviceLayout(string path)
        {
            if (string.IsNullOrEmpty(path) || path[0] != '<')
            {
                return null;
            }

            int close = path.IndexOf('>');
            return close > 1 ? path.Substring(1, close - 1) : null;
        }

        /// <summary>Path prefixes an interactive rebind for <paramref name="kind"/> must match (null for <see cref="BindingDeviceKind.Other"/>).</summary>
        public static string[] GetRebindPathPrefixes(BindingDeviceKind kind)
        {
            switch (kind)
            {
                case BindingDeviceKind.Keyboard:
                    return new[] { KeyboardPathPrefix };
                case BindingDeviceKind.Gamepad:
                    return new[] { GamepadPathPrefix };
                default:
                    return null;
            }
        }

        /// <summary>
        /// Index of the binding of <paramref name="action"/> for the given column. With a
        /// <paramref name="partName"/> (e.g. "up"), only composite parts with that name match;
        /// without one, composite heads and parts are skipped. Returns -1 when there is none.
        /// </summary>
        public static int FindBindingIndex(InputAction action, string partName, BindingDeviceKind kind)
        {
            if (action == null)
            {
                return -1;
            }

            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isComposite)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(partName))
                {
                    if (!binding.isPartOfComposite || !string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                else if (binding.isPartOfComposite)
                {
                    continue;
                }

                if (Classify(binding.path, binding.groups) == kind)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Formats a raw display string for a rebind button: trims, "—" when empty, " *" suffix when overridden.</summary>
        public static string FormatBindingDisplay(string rawDisplay, bool isOverridden)
        {
            string text = rawDisplay?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return EmptyBindingText;
            }

            return isOverridden ? text + OverriddenSuffix : text;
        }

        /// <summary>True when the binding at <paramref name="bindingIndex"/> has an override that differs from its original path.</summary>
        public static bool IsOverridden(InputAction action, int bindingIndex)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return false;
            }

            InputBinding binding = action.bindings[bindingIndex];
            return !string.IsNullOrEmpty(binding.overridePath) && !PathsEqual(binding.overridePath, binding.path);
        }

        /// <summary>Display-ready text for one binding slot (index -1 shows "—").</summary>
        public static string GetFormattedDisplay(InputAction action, int bindingIndex)
        {
            return GetFormattedDisplay(action, bindingIndex, null);
        }

        /// <summary>
        /// <see cref="GetFormattedDisplay(InputAction, int)"/> with the Input System's English control
        /// names ("Left Stick Press", "D-Pad/Up") passed through <paramref name="translateName"/> first
        /// (see <see cref="LocalizeDisplay"/>), before the override suffix is appended.
        /// </summary>
        public static string GetFormattedDisplay(InputAction action, int bindingIndex, Func<string, string> translateName)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return EmptyBindingText;
            }

            string raw = LocalizeDisplay(action.GetBindingDisplayString(bindingIndex), translateName);
            return FormatBindingDisplay(raw, IsOverridden(action, bindingIndex));
        }

        /// <summary>
        /// Translates an Input System display string: the whole string first ("Left Stick Press"),
        /// else each "/"-separated part ("D-Pad/Up" → "Direcional/Cima"). <paramref name="translateName"/>
        /// returns null for names it doesn't know, which are kept as-is (single letters, F1...).
        /// </summary>
        public static string LocalizeDisplay(string display, Func<string, string> translateName)
        {
            if (translateName == null || string.IsNullOrEmpty(display))
            {
                return display;
            }

            string trimmed = display.Trim();
            string whole = translateName(trimmed);
            if (!string.IsNullOrEmpty(whole))
            {
                return whole;
            }

            if (trimmed.IndexOf('/') < 0)
            {
                return display;
            }

            string[] parts = trimmed.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                string translated = translateName(part);
                parts[i] = string.IsNullOrEmpty(translated) ? part : translated;
            }

            return string.Join("/", parts);
        }

        /// <summary>Case-insensitive equality of two control paths; empty paths never conflict.</summary>
        public static bool PathsConflict(string a, string b)
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && PathsEqual(a, b);
        }

        private static bool PathsEqual(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Every OTHER binding in the same action map (or the same action, when it has no map)
        /// whose effective path equals the effective path of <paramref name="action"/>'s binding
        /// at <paramref name="bindingIndex"/>. Composite heads (no real control) are ignored.
        /// </summary>
        public static List<BindingConflict> FindConflicts(InputAction action, int bindingIndex)
        {
            var conflicts = new List<BindingConflict>();
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return conflicts;
            }

            string targetPath = action.bindings[bindingIndex].effectivePath;
            if (string.IsNullOrEmpty(targetPath))
            {
                return conflicts;
            }

            IEnumerable<InputAction> candidates = action.actionMap != null
                ? (IEnumerable<InputAction>)action.actionMap.actions
                : new[] { action };

            foreach (InputAction other in candidates)
            {
                var bindings = other.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (other == action && i == bindingIndex)
                    {
                        continue;
                    }

                    InputBinding binding = bindings[i];
                    if (binding.isComposite)
                    {
                        continue;
                    }

                    if (PathsConflict(targetPath, binding.effectivePath))
                    {
                        conflicts.Add(new BindingConflict(other.name, binding.isPartOfComposite ? binding.name : null, i));
                    }
                }
            }

            return conflicts;
        }
    }
}
