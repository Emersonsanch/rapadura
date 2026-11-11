using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One deep stretch of the Cova do Lodo (map 87) and the sluice of the Artificers that drains it, in the region's own coordinates
    /// (<c>OverworldLayouts.ProfundoSluices</c>): the water lies over the rectangle <see cref="Water"/>, across the whole column of the pit, and the
    /// lever of the sluice stands at <see cref="Lever"/>, on the Lâmpada 4 side, off the road.
    /// </summary>
    public readonly struct ProfundoSluice
    {
        public readonly string Id;
        public readonly Rect Water;
        public readonly Vector2 Lever;

        public ProfundoSluice(string id, Rect water, Vector2 lever)
        {
            Id = id;
            Water = water;
            Lever = lever;
        }
    }

    /// <summary>
    /// The water level of the Cova do Lodo (Docs/Mapas100.md 87, "nível da água: comportas dos Artífices sobem e descem a água; Bolhas de Ar
    /// para os trechos fundos"), pure and server-runnable. Two deep stretches cross the column of the pit, one in the Salões Alagados (between
    /// the Lâmpada 4 and the Cova da Treva) and one in the Comportas (between the Lâmpada 4 and the Túneis Cegos). While the water is up, a hero
    /// wades them slowly on the Air Bubble the zone asks for, and once the bubble's breath runs out the black water bites (never past a floor of
    /// life). Pulling the sluice's lever lowers that stretch for the rest of the in-game day (a daily mark in <c>WorldMapProgress</c>), and the
    /// rush of the draining water knocks the beasts standing in it.
    /// <code>
    /// 2 stretches across the column (x 840–1320, 56 m deep along the road) · lever reach 3.5 m, 190–200 m east of the road
    /// water up: move speed −50 % · the bubble holds 4 s, then 2 % of max life every 1.5 s (3 % at night), never under 30 %
    /// lever: drained until the end of the in-game day · the rush: beasts in the stretch lose 6 % of max life (a boss 1 %)
    /// wading the 56 m: ~20 s and ~22 % of life; the detour to the lever and back: ~390 m (~71 s), then dry for the day
    /// </code>
    /// </summary>
    public static class ProfundoSluiceRules
    {
        public const float LeverReach = 3.5f;
        public const float SpeedFactor = 0.5f;
        public const float BubbleSeconds = 4f;
        public const float BiteSeconds = 1.5f;
        public const float BiteFraction = 0.02f;
        public const float NightBiteFraction = 0.03f;
        public const float BiteFloorFraction = 0.3f;
        public const float RushFraction = 0.06f;
        public const float RushBossFraction = 0.01f;

        /// <summary>The MoveSpeed modifier (percent additive) while wading: −0.5.</summary>
        public static float MoveBonus => SpeedFactor - 1f;

        /// <summary>The save key of a sluice (a daily mark: the in-game day it was drained).</summary>
        public static string MarkKey(string id) => "profundo.sluice." + id;

        /// <summary>True when the stretch was drained on <paramref name="today"/> (the mark is that day).</summary>
        public static bool DrainedToday(int markedDay, int today) => markedDay > 0 && markedDay == today;

        /// <summary>The stretch whose deep water is under <paramref name="p"/>, or −1.</summary>
        public static int WaterAt(IReadOnlyList<ProfundoSluice> sluices, Vector2 p)
        {
            for (int i = 0; i < sluices.Count; i++)
            {
                if (sluices[i].Water.Contains(p))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The sluice whose lever is within <see cref="LeverReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int LeverAt(IReadOnlyList<ProfundoSluice> sluices, Vector2 p)
        {
            for (int i = 0; i < sluices.Count; i++)
            {
                if ((sluices[i].Lever - p).sqrMagnitude <= LeverReach * LeverReach)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>How many bites are due after <paramref name="wading"/> seconds in deep water (none while the bubble holds).</summary>
        public static int BitesDue(float wading) => wading < BubbleSeconds ? 0 : 1 + Mathf.FloorToInt((wading - BubbleSeconds) / BiteSeconds);

        /// <summary>One bite of the black water: a slice of max life that never takes the hero under <see cref="BiteFloorFraction"/>.</summary>
        public static float BiteAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightBiteFraction : BiteFraction, BiteFloorFraction);

        /// <summary>The rush of the draining water on a beast standing in the stretch.</summary>
        public static float RushAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? RushBossFraction : RushFraction);
    }
}
