using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum NartesSkyGoldCheck
    {
        /// <summary>Lavine, and this object has cooled since she last took one from it.</summary>
        Take = 0,
        /// <summary>Burning hot for this hero (no harm, only the refusal).</summary>
        TooHot = 1,
        /// <summary>Already taken this UTC day.</summary>
        Cooled = 2,
    }

    /// <summary>
    /// The Golds of Heaven in the Kurgans (Docs/Mapas100.md, map 48: "quatro objetos de ouro (arado com canga, machado, taça)
    /// quentes demais para os maiores; só a Lavine consegue pegar"; Herodotus IV.5–7: only the youngest can lift them). Pure.
    /// Four objects on the kurgan floors (<c>OverworldLayouts.NartesSkyGolds</c>): plough, yoke, axe and cup. Anyone can find
    /// them; only Lavine picks one up. Nobody is hurt: the others just feel the heat.
    /// <code>
    /// reach 4 m · each object gives 45 nuggets, once per UTC day
    /// </code>
    /// </summary>
    public static class NartesSkyGoldRules
    {
        public const float Reach = 4f;
        public const int Purse = 45;

        /// <summary>Never-taken objects keep this as their day.</summary>
        public const long NeverTaken = -1L;

        public static NartesSkyGoldCheck Check(CharacterId? hero, long lastTakenDay, long today)
        {
            if (hero != CharacterId.Lavine)
            {
                return NartesSkyGoldCheck.TooHot;
            }

            return lastTakenDay == today ? NartesSkyGoldCheck.Cooled : NartesSkyGoldCheck.Take;
        }
    }
}
