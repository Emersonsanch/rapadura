namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Velho Kul beaten "solta os peixes e vira o dono da pesca" (Docs/Mapas100.md 64), pure and server-runnable (unix seconds in, so a
    /// server keeps one freed lake for everyone fishing it). Beating him frees the lake for a while; a new victory starts the time over (it never
    /// stacks). While the lake is free, on every hole of Old Kul's Deep:
    /// <code>
    /// 30 min from the victory · rare × 1.5 · Peixe-Dourado × 2 · each hole rests 15 s (30 without)
    /// "dono da pesca": 20% of the landed fish come with a second one of the same kind
    /// </code>
    /// It stacks with the night, the Buraco Grande and the bait (<see cref="IceBaitRules"/>). It does not end when the Kul comes back: the fish
    /// he let go stay loose until the time runs out. Only the moment of the last victory is saved (an event mark).
    /// </summary>
    public sealed class KulFreedLake
    {
        public const long FreeSeconds = 1800;
        public const long NeverFreed = long.MinValue;
        public const float RareFactor = 1.5f;
        public const float LegendaryFactor = 2f;
        public const float HoleRestSeconds = 15f;
        public const float ExtraFishChance = 0.2f;

        public long FreedAt { get; private set; } = NeverFreed;

        public bool IsFree(long now) => FreedAt != NeverFreed && now >= FreedAt && now - FreedAt < FreeSeconds;

        /// <summary>Seconds of free lake left at <paramref name="now"/> (0 when not free).</summary>
        public long SecondsLeft(long now) => IsFree(now) ? FreeSeconds - (now - FreedAt) : 0L;

        /// <summary>The Velho Kul was beaten at <paramref name="now"/>: the lake is free from now on (the time starts over).</summary>
        public void Free(long now) => FreedAt = now;

        /// <summary>Back from the save: only the moment of the last victory.</summary>
        public void Restore(long freedAt) => FreedAt = freedAt;

        public static float RareFactorFor(bool free) => free ? RareFactor : 1f;

        public static float LegendaryFactorFor(bool free) => free ? LegendaryFactor : 1f;

        /// <summary>How long a hole rests after a cast.</summary>
        public static float HoleRest(bool free) => free ? HoleRestSeconds : IceFishingRules.HoleCooldownSeconds;

        /// <summary>Whether a landed fish comes with a second one, for a roll in [0, 1).</summary>
        public static bool ExtraFish(bool free, double roll) => free && roll < ExtraFishChance;
    }
}
