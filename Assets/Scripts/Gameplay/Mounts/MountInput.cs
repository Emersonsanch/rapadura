using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The "Mount" / "MountAbility" actions where there is no <see cref="PlayerInput"/> (the world map) or the asset lacks
    /// them: a code-built "Player" map whose bindings carry the very ids of PlayerControls.inputactions, so the rebinds
    /// saved by <c>InputRebindManager</c> (PlayerPrefs "Accessibility.InputBindings.PlayerControls") apply here too.
    /// </summary>
    public static class MountInput
    {
        public const string MapName = "Player";
        public const string RebindPrefsKey = "Accessibility.InputBindings.PlayerControls";

        public const string MountKeyboardId = "e5886b75-b5bc-4eb5-b3ee-a922c1f9e415";
        public const string MountGamepadId = "54a96c8e-48b9-4813-8ebc-689009084df9";
        public const string AbilityKeyboardId = "540273e7-42b1-4a26-a82e-bf8dba04649f";
        public const string AbilityGamepadId = "f9c5f1b2-7e23-4d93-b912-98ad9fac17a8";

        /// <summary>A new, enabled map with both actions and the player's saved rebinds. Dispose it when done.</summary>
        public static InputActionMap CreateMap(out InputAction mount, out InputAction ability)
        {
            var map = new InputActionMap(MapName);
            mount = map.AddAction(MountController.MountActionName, InputActionType.Button);
            mount.AddBinding(Binding(MountController.DefaultMountBinding, MountKeyboardId));
            mount.AddBinding(Binding(MountController.DefaultMountGamepadBinding, MountGamepadId));
            ability = map.AddAction(MountController.AbilityActionName, InputActionType.Button);
            ability.AddBinding(Binding(MountController.DefaultAbilityBinding, AbilityKeyboardId));
            ability.AddBinding(Binding(MountController.DefaultAbilityGamepadBinding, AbilityGamepadId));

            string json = PlayerPrefs.GetString(RebindPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    map.LoadBindingOverridesFromJson(json);
                }
                catch (Exception)
                {
                    // Overrides for other actions/ids are simply not ours; a malformed blob keeps the defaults.
                }
            }

            map.Enable();
            return map;
        }

        private static InputBinding Binding(string path, string id)
        {
            return new InputBinding { path = path, id = new Guid(id) };
        }
    }
}
