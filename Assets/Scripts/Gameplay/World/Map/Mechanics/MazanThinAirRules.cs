using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The thin air of the Cume dos Mazans (Docs/Mapas100.md 18: "ar fino: no platô de cima o fôlego volta 30% mais devagar; a Maithe ignora"),
    /// pure and server-runnable. On the top plateau of the summit (the northern half of the Cume, round Mazan's ring of stones) the hero's
    /// stamina comes back 30 % slower; Maithe (Vento Vivo) breathes the mountain wind as it is and does not feel it. Running and everything else
    /// stay as they are.
    /// <code>
    /// top plateau: stamina regen × 0.7 · Maithe × 1
    /// </code>
    /// </summary>
    public static class MazanThinAirRules
    {
        public const float StaminaRegenFactor = 0.7f;

        /// <summary>The PercentAdditive value of the thin-air effect on StaminaRegen (−0.3).</summary>
        public static float StaminaRegenBonus => StaminaRegenFactor - 1f;

        public static bool Ignores(CharacterId? hero) => hero == CharacterId.Maithe;

        /// <summary>The factor on the hero's stamina regen at <paramref name="local"/> (region coordinates).</summary>
        public static float StaminaFactor(Rect plateau, Vector2 local, CharacterId? hero)
        {
            return plateau.Contains(local) && !Ignores(hero) ? StaminaRegenFactor : 1f;
        }
    }
}
