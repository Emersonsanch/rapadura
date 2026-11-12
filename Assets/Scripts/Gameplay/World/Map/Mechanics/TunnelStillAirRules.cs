using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The still air of the Mercado dos Túneis (Docs/Mapas100.md 22: "ar parado: longe dos tubos o fôlego cai devagar; as Garrafas de Ar Sereno
    /// resolvem"), pure and server-runnable. The kobolds' copper air pipes stand at the market and every 100 m along its four galleries, and
    /// breathe 30 m round them. Farther from every pipe, in the map and off the market, the hero's stamina does not come back (StaminaRegen
    /// −100 %) and slowly falls (1.5 per second at the default stamina, a tenth of the run's drain), never under 30 % of the maximum, so the
    /// hero can always run a little. An open Garrafa de Ar Sereno (the shops' bottle, +20 % stamina recovery for 60 s) gives the breath back:
    /// while it lasts the still air does nothing.
    /// <code>
    /// pipe air 30 m · far from the pipes: regen × 0, drain 0.1 × run drain per second, floor 30 % · bottle open → nothing
    /// </code>
    /// </summary>
    public static class TunnelStillAirRules
    {
        public const float PipeReach = 30f;
        public const float DrainFraction = 0.1f;
        public const float FloorFraction = 0.3f;

        /// <summary>The PercentAdditive value of the still-air effect on StaminaRegen (−1: no regen).</summary>
        public const float StaminaRegenBonus = -1f;

        /// <summary>The bottle the kobolds sell (its effect's source key on the BuffController is <c>consumable_</c> + this).</summary>
        public const string BottleItemId = "item_garrafa_ar_sereno";

        /// <summary>Whether <paramref name="p"/> is within <see cref="PipeReach"/> of a pipe.</summary>
        public static bool NearPipe(IReadOnlyList<Vector2> pipes, Vector2 p)
        {
            for (int i = 0; pipes != null && i < pipes.Count; i++)
            {
                if ((pipes[i] - p).sqrMagnitude <= PipeReach * PipeReach)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether the still air takes hold: in the map, off the market, far from every pipe, no bottle open.</summary>
        public static bool Stifles(bool inMap, bool onMarket, bool nearPipe, bool bottleOpen) => inMap && !onMarket && !nearPipe && !bottleOpen;

        /// <summary>
        /// The run-drain seconds to feed <c>PlayerStats.DrainStamina</c> over <paramref name="deltaTime"/> (it drains <c>run drain × seconds</c>):
        /// a tenth of the time, nothing at or under the floor.
        /// </summary>
        public static float DrainSeconds(float current, float max, float deltaTime)
        {
            return deltaTime > 0f && current > max * FloorFraction ? deltaTime * DrainFraction : 0f;
        }
    }
}
