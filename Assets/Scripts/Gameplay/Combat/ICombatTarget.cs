using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Contract implemented by anything that can take damage or be healed — the player, NPCs,
    /// enemies, destructible world objects. Skills and the future combat system target this
    /// interface instead of concrete types, so a fireball works identically against the player,
    /// an NPC or a wooden crate.
    /// </summary>
    public interface ICombatTarget
    {
        bool IsAlive { get; }
        Transform Transform { get; }

        void ApplyDamage(float amount, ElementType element, GameObject source);
        void ApplyHealing(float amount, GameObject source);
    }
}
