using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A circle of the Desert of the Clock where time runs at <see cref="Factor"/> (region coordinates).</summary>
    public readonly struct MisturaTimeBubble
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public readonly float Factor;

        public MisturaTimeBubble(Vector2 center, float radius, float factor)
        {
            Center = center;
            Radius = radius;
            Factor = factor;
        }
    }

    /// <summary>
    /// The time bubbles of the Deserto do Relógio (Docs/Mapas100.md, map 78: "áreas onde tudo fica a ×0,7 ou a ×1,3, herói e bicho
    /// igual, marcadas no chão"; "de noite as bolhas somem"), pure. A bubble only changes the walking pace, the same for the hero and the
    /// beasts, and never stops anyone: the slow one is still 70 % of the pace, so nothing can hold the hero.
    /// <code>
    /// slow bubble × 0.7 · fast bubble × 1.3 · 1 outside and at night (no bubble at all)
    /// </code>
    /// </summary>
    public static class MisturaTimeBubbleRules
    {
        public const float SlowFactor = 0.7f;
        public const float FastFactor = 1.3f;

        /// <summary>The first bubble containing <paramref name="p"/> (region coordinates), or −1. Night leaves no bubble.</summary>
        public static int IndexAt(IReadOnlyList<MisturaTimeBubble> bubbles, Vector2 p, bool night)
        {
            if (night)
            {
                return -1;
            }

            for (int i = 0; i < bubbles.Count; i++)
            {
                if ((p - bubbles[i].Center).sqrMagnitude <= bubbles[i].Radius * bubbles[i].Radius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The time factor at <paramref name="p"/>: the bubble's, or 1 outside them and at night.</summary>
        public static float FactorAt(IReadOnlyList<MisturaTimeBubble> bubbles, Vector2 p, bool night)
        {
            int index = IndexAt(bubbles, p, night);
            return index >= 0 ? bubbles[index].Factor : 1f;
        }

        /// <summary>The MoveSpeed modifier (PercentAdditive) that gives <paramref name="factor"/>: × 0.7 is −0.3.</summary>
        public static float MoveBonus(float factor) => factor - 1f;
    }
}
