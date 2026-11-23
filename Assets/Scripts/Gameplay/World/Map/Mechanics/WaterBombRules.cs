using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A water bomb's splash: where it burst (flat world x/z) and when (the clock of whoever keeps the board).</summary>
    public readonly struct WaterSplash
    {
        public readonly Vector2 Center;
        public readonly double At;

        public WaterSplash(Vector2 center, double at)
        {
            Center = center;
            At = at;
        }
    }

    /// <summary>
    /// The Bomba de Água of the Recife de Obsidiana (Docs/Mapas100.md 68: "resfriar: bombas de água (consumível) ... endurecem a lava em ponte"),
    /// pure and server-runnable. Used from the bag, it bursts at the hero's feet:
    /// <code>
    /// a splash of 5 m for 20 s; inside it the burning lava hardens into a crust you can stand on:
    ///   the Tartaruga-Vulcão's wake (68): every hot spot inside goes cold at once, and new ones do not take there while it lasts
    ///   Echeyde's eruption route (67): no burn inside the splash
    ///   the Caldera's heat (70): the splash counts as a cold stone (the meter falls to zero)
    ///   the Grande Tremor's spill (65): the pool on the road does not burn whoever stands on the crust
    /// </code>
    /// The tamed lava channels and the lakes of lava are scenery (they never burn and are not walked), so "lava em ponte" is the crust over the lava
    /// that does burn. The splash holds no controls and needs no save.
    /// </summary>
    public static class WaterBombRules
    {
        public const string ItemId = "item_bomba_agua";
        public const float Radius = 5f;
        public const float Seconds = 20f;

        public static bool Active(WaterSplash splash, double now) => now >= splash.At && now - splash.At < Seconds;

        public static bool Covers(WaterSplash splash, Vector2 point, double now) =>
            Active(splash, now) && (point - splash.Center).sqrMagnitude <= Radius * Radius;

        public static bool Covers(IReadOnlyList<WaterSplash> splashes, Vector2 point, double now)
        {
            if (splashes == null)
            {
                return false;
            }

            for (int i = 0; i < splashes.Count; i++)
            {
                if (Covers(splashes[i], point, now))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Takes out of <paramref name="wake"/> every hot spot a live splash covers; returns how many went cold.</summary>
        public static int CoolWake(List<Vector2> wake, IReadOnlyList<WaterSplash> splashes, double now)
        {
            if (wake == null || splashes == null || splashes.Count == 0)
            {
                return 0;
            }

            return wake.RemoveAll(spot => Covers(splashes, spot, now));
        }

        /// <summary>Drops the splashes that dried.</summary>
        public static void Prune(List<WaterSplash> splashes, double now) => splashes?.RemoveAll(s => !Active(s, now));
    }

    /// <summary>The live splashes in the world (the hero's; a server would keep everyone's), read by the Hinokura driver and the turtle's fight.</summary>
    public static class WaterBombBoard
    {
        private static readonly List<WaterSplash> Splashes = new List<WaterSplash>();

        public static IReadOnlyList<WaterSplash> All => Splashes;

        public static void Splash(Vector2 center, double now)
        {
            WaterBombRules.Prune(Splashes, now);
            Splashes.Add(new WaterSplash(center, now));
        }

        public static bool Covers(Vector2 point, double now) => WaterBombRules.Covers(Splashes, point, now);

        public static int CoolWake(List<Vector2> wake, double now) => Splashes.Count == 0 ? 0 : WaterBombRules.CoolWake(wake, Splashes, now);

        /// <summary>Also on each Play (domain reload is off: a stale time from the last session would light it again).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear() => Splashes.Clear();
    }
}
