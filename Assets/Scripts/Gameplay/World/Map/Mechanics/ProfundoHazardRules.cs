using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The one rule every hazard of the Limiar Profundo shares (pure): a bite is a slice of max life that stops at a floor, so the pits wear the hero
    /// down but never finish them. The floor is at least 25 % of max life and a bite at most 4 %: even a Defense debuff that doubled the damage
    /// inside PlayerStats could not carry the hero past it.
    /// </summary>
    public static class ProfundoHazardRules
    {
        /// <summary>A bite of <paramref name="fraction"/> of max life that leaves at least <paramref name="floorFraction"/> of max life (0 at or under the floor).</summary>
        public static float Bite(float currentHealth, float maxHealth, float fraction, float floorFraction)
        {
            return Mathf.Clamp(maxHealth * fraction, 0f, Mathf.Max(0f, currentHealth - maxHealth * floorFraction));
        }
    }
}
