using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Ecos that rise from the noise of the Ermidas Suspensas (Docs/Mapas100.md 56: "cheio, os Ecos aparecem"), pure, on top of
    /// <see cref="FarnkeshSilenceRules"/> and <see cref="HermitageStillBellRules"/>. When the meter fills, the Echo still marks the ground and
    /// strikes the mark 1.5 s later; now it stays: an Eco Apagado (the common of the cliff's "ecos_apagados", its own beast since the
    /// twenty-eighth pass) rises on the mark and comes for the hero, unless enough of them are already up. Leaving the cliff sends the risen
    /// ones back into the stone.
    /// <code>
    /// on each Echo strike: raise one Eco on the mark if fewer than 2 risen ones are alive (the cliff's own groups do not count)
    /// level: the middle of the area's band (Ermidas 158–168 → 163; Pináculo do Sino 158–162 → 160) · it chases at once
    /// leaving the hermitages (the silence area) or dying: the risen Ecos go back into the stone (no corpse left behind after 5 s)
    /// running the whole cliff from empty: the first Eco after 12.5 s, the second ~6 s later, then the strikes only (2 alive)
    /// </code>
    /// Decision (2026-11-23): two at most, so running is answered but never buried; the bell stilled or a quiet walk raise fewer, because
    /// they come only from full meters. The Echo's ground strike stays as it was (the warning ring is how the hero learns where they come from).
    /// </summary>
    public static class HermitageEchoRules
    {
        /// <summary>The beast that rises (the Eco Apagado, <c>OverworldLayouts.PeaksOwnBeasts.DimmedEcho</c>).</summary>
        public const string EchoEnemyId = "enemy_mountain_dimmed_echo";

        /// <summary>Risen Ecos alive at once, at most.</summary>
        public const int MaxRisen = 2;

        /// <summary>Seconds a fallen risen Eco stays before it is cleared.</summary>
        public const float CorpseSeconds = 5f;

        /// <summary>Whether an Echo strike raises one more Eco, with <paramref name="alive"/> risen ones still up.</summary>
        public static bool CanRaise(int alive) => alive < MaxRisen;

        /// <summary>The level of a risen Eco in an area of band <paramref name="minLevel"/>–<paramref name="maxLevel"/>: the middle, rounded down.</summary>
        public static int RisenLevel(int minLevel, int maxLevel)
        {
            int low = Mathf.Max(1, Mathf.Min(minLevel, maxLevel));
            int high = Mathf.Max(low, Mathf.Max(minLevel, maxLevel));
            return low + (high - low) / 2;
        }
    }
}
