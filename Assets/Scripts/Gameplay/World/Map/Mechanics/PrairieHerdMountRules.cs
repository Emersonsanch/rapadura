namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The rare mount of the herding of the Pradaria dos Mil Cavalos (Docs/Mapas100.md 51: "pastorear: levar cavalos soltos até o acampamento de
    /// Soslan dá pontos e, raramente, uma montaria"; "Falta: a montaria rara"), pure and server-runnable, on top of <see cref="PrairieHerdRules"/>.
    /// Among the loose horses runs, now and then, a heavenly horse of Ferghana (the sheet's mount of the map, <c>mount_pinto_horse</c>), and
    /// the herders of Soslan give it to the cousin who brought it in:
    /// <code>
    /// each horse delivered: 3 % (a roll the server can repeat: day, horse and the herding points)
    /// sure on the delivery that reaches 40 herding points (eight days of full herding) · never if the cousin already has it
    /// </code>
    /// Decision (2026-11-23): an existing mount instead of a new one (the Cavalo Celeste de Fergana is the sheet's own, sold in Srosh's Stable,
    /// Uncommon): herding is the slow free way to it; the 40-point floor keeps bad luck from lasting forever.
    /// </summary>
    public static class PrairieHerdMountRules
    {
        public const string MountId = "mount_pinto_horse";
        public const double Chance = 0.03d;
        public const long SurePoints = 40L;

        /// <summary>A roll in [0, 1) for the delivery of <paramref name="horse"/> on UTC day <paramref name="day"/> with <paramref name="points"/> after it.</summary>
        public static double Roll(long day, int horse, long points)
        {
            unchecked
            {
                ulong h = 1469598103934665603UL;
                h = (h ^ (ulong)day) * 1099511628211UL;
                h = (h ^ (ulong)(horse + 1)) * 1099511628211UL;
                h = (h ^ (ulong)points) * 1099511628211UL;
                h ^= h >> 33;
                h *= 0xff51afd7ed558ccdUL;
                h ^= h >> 33;
                return (h >> 11) * (1.0 / (1UL << 53));
            }
        }

        /// <summary>Whether this delivery brings the heavenly horse: not owned, and the roll under <see cref="Chance"/> or the points at <see cref="SurePoints"/>.</summary>
        public static bool Earns(bool owned, long pointsAfter, double roll) => !owned && (pointsAfter >= SurePoints || roll < Chance);
    }
}
