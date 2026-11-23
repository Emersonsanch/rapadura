using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The water of the Pactolo that stops O Ouro de Midas (Docs/Mapas100.md 53: "O Ouro de Midas, uma estátua de ouro que andou sozinha; para em
    /// pé quando o Midas lava as mãos no rio"), pure and server-runnable. Midas stopped his touch by washing his hands in the Pactolo; the
    /// statue is that touch walking about, 400 m from the river, and it goes back to full life if the hero runs off to the river mid-fight. So
    /// the hero brings the river to it: [E] standing in the Pactolo fills the cupped hands with Living Water, three pours, kept for ten minutes
    /// in the Picos de Farnkesh. [E] within 6 m of the statue pours one on its hands: it stands still (a stun, halved on bosses like every
    /// stun) and for 6 s its golden touch does not answer the blows (no gold, no strike).
    /// <code>
    /// [E] in the Pactolo (6.5 m of its middle): 3 pours · kept 600 s, lost leaving the region · [E] 6 m from the statue: 1 pour
    /// a pour: stun 6 s (3 s on the boss) · the golden touch does nothing for 6 s · then fill again at the river
    /// </code>
    /// Decision (2026-11-23): three pours a filling, so one trip before the fight covers it and the hero chooses when to spend them (the touch
    /// punishes hitting during its glow; a pour lets the hero hit through it). The time is any clock the caller keeps; online the server keeps it.
    /// </summary>
    public static class MidasWashRules
    {
        public const int Pours = 3;
        public const double CarrySeconds = 600d;
        public const float PourReach = 6f;
        public const float StunSeconds = 6f;
        public const double StillSeconds = 6d;

        /// <summary>The save mark of the first pour (the cousin's line, once).</summary>
        public const string FirstPourKey = "pactolo_midas_poured";

        /// <summary>The pours a hero still carries at <paramref name="now"/>, filled at <paramref name="filledAt"/> with <paramref name="left"/> left.</summary>
        public static int PoursLeft(int left, double filledAt, double now) => left > 0 && now - filledAt < CarrySeconds ? Mathf.Min(left, Pours) : 0;

        /// <summary>Whether [E] in the river fills the hands: only in the Pactolo and only when they are not full already.</summary>
        public static bool CanFill(bool inRiver, int poursLeft) => inRiver && poursLeft < Pours;

        /// <summary>Whether [E] pours on the statue: a pour left, the statue alive and within <see cref="PourReach"/>.</summary>
        public static bool CanPour(int poursLeft, bool statueAlive, float distance) => poursLeft > 0 && statueAlive && distance <= PourReach;

        /// <summary>Until when the statue stands still after a pour at <paramref name="pouredAt"/> (a later pour extends it, never shortens it).</summary>
        public static double StillUntil(double current, double pouredAt) => System.Math.Max(current, pouredAt + StillSeconds);

        public static bool IsStill(double stillUntil, double now) => now < stillUntil;
    }

    /// <summary>
    /// The statue's stillness as the boss reads it (53, <see cref="MidasWashRules"/>): written by the Farnkesh driver when a pour lands and read
    /// by the golden touch (<c>RegionBossAbilities</c>, GoldenTouch). Online the server keeps it; times are <c>Time.timeAsDouble</c> on both sides.
    /// </summary>
    public static class MidasWashBoard
    {
        public static double StillUntil { get; private set; }

        /// <summary>How many touches the water stopped (the driver speaks when it changes).</summary>
        public static int Stopped { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            StillUntil = 0d;
            Stopped = 0;
        }

        public static void Poured(double now) => StillUntil = MidasWashRules.StillUntil(StillUntil, now);

        /// <summary>Whether the touch resolving at <paramref name="now"/> is stopped by the water (and counts it).</summary>
        public static bool StopsTouch(double now)
        {
            if (!MidasWashRules.IsStill(StillUntil, now))
            {
                return false;
            }

            Stopped++;
            return true;
        }
    }
}
