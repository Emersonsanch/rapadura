using System;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Optional, context-aware combat modifiers that don't fit a <c>BuffController</c> stat because they depend on
    /// who is hit by whom (e.g. "8% less damage from flying creatures", "3% chance to dodge a common creature's hit").
    /// Each hook is a single delegate installed by the system that owns the rule (today the hunting features,
    /// <c>HuntingProgress</c>) and cleared on its shutdown; with nothing installed every resolver returns the neutral
    /// value, so the combat pipeline behaves exactly as before. Read by <c>PlayerStats</c> (dodge and damage taken) and <c>BuffController.ApplyStun</c> (stun duration). A throwing hook is ignored.
    /// </summary>
    public static class CombatModifierHooks
    {
        /// <summary>(target, source) → multiplier on the damage the target takes after its defense (1 = unchanged).</summary>
        public static Func<GameObject, GameObject, float> DamageTakenMultiplier;

        /// <summary>(victim, attacker) → true when the victim dodges the hit entirely.</summary>
        public static Func<GameObject, GameObject, bool> DodgeCheck;

        /// <summary>(target) → multiplier on an incoming stun's duration (1 = unchanged).</summary>
        public static Func<GameObject, float> StunReceivedMultiplier;

        public static float ResolveDamageTakenMultiplier(GameObject target, GameObject source)
        {
            Func<GameObject, GameObject, float> hook = DamageTakenMultiplier;
            if (hook == null || target == null)
            {
                return 1f;
            }

            try
            {
                float value = hook(target, source);
                return float.IsNaN(value) || value < 0f ? 1f : value;
            }
            catch (Exception)
            {
                return 1f;
            }
        }

        public static bool ShouldDodge(GameObject victim, GameObject attacker)
        {
            Func<GameObject, GameObject, bool> hook = DodgeCheck;
            if (hook == null || victim == null || attacker == null)
            {
                return false;
            }

            try
            {
                return hook(victim, attacker);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static float ResolveStunMultiplier(GameObject target)
        {
            Func<GameObject, float> hook = StunReceivedMultiplier;
            if (hook == null || target == null)
            {
                return 1f;
            }

            try
            {
                float value = hook(target);
                return float.IsNaN(value) || value < 0f ? 1f : value;
            }
            catch (Exception)
            {
                return 1f;
            }
        }

        /// <summary>Removes every hook (tests, shutdown).</summary>
        public static void ClearAll()
        {
            DamageTakenMultiplier = null;
            DodgeCheck = null;
            StunReceivedMultiplier = null;
        }
    }
}
