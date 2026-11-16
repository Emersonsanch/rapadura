using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The fire that burns O Rei do Piche's tar (map 79, Docs/Mapas100.md: "Fogo Vivo (skills do Ícaro) queima poças e abre caminho; cuidado:
    /// queima todo mundo perto"), pure and server-runnable. While the King warns that his tar spreads (2.6 s, 6 m round him), any fire skill of
    /// the hero that lands on that tar (or is cast from inside it) sets it alight:
    /// <code>
    /// burnt tar never hardens: nobody is stuck by it, however long they stood in it
    /// the flash: whoever still stands within 6 m of the King when it would harden takes 0.5× the boss's hit, as fire
    /// the King himself loses 3% of his health to his own burning pitch
    /// </code>
    /// Decision (2026-11-16): it is the same fire as the pools of the map (<see cref="MisturaTarRules"/>), so Ícaro's play reads the same in
    /// and out of the ring; the flash keeps "queima todo mundo perto" (burn it and step out). Times are any clock the caller keeps.
    /// </summary>
    public static class PitchKingBurnRules
    {
        public const float TarRadius = RegionBossRules.TarNearRadius;
        public const float KingBurnFraction = 0.03f;
        public const float FlashDamageFactor = 0.5f;

        /// <summary>Does a fire that lands at <paramref name="point"/> with <paramref name="radius"/> of area reach the King's tar at <paramref name="king"/>?</summary>
        public static bool Reaches(Vector2 king, Vector2 point, float radius)
        {
            float reach = TarRadius + Mathf.Max(0f, radius);
            return (point - king).sqrMagnitude <= reach * reach;
        }

        public static bool InWarning(double at, double warnedAt, double strikeAt) => at >= warnedAt && at <= strikeAt;

        /// <summary>Does a fire cast at <paramref name="at"/> burn the tar the King warned of at <paramref name="warnedAt"/>?</summary>
        public static bool Burns(Vector2 king, Vector2 point, float radius, double at, double warnedAt, double strikeAt) =>
            InWarning(at, warnedAt, strikeAt) && Reaches(king, point, radius);

        /// <summary>Does the flash of the burning tar catch someone <paramref name="distance"/> m from the King?</summary>
        public static bool FlashHits(float distance) => distance <= TarRadius;
    }

    /// <summary>
    /// The last fires the hero cast (world coordinates), as the King's tar reads them (<c>RegionBossAbilities</c>, TarSoak): written by
    /// <see cref="MisturaMechanicsDriver"/> on every fire skill in the Margens da Mistura. Online the server keeps them, one list per map.
    /// </summary>
    public static class PitchKingBurnBoard
    {
        private const int Slots = 4;

        private static readonly Vector2[] s_points = new Vector2[Slots];
        private static readonly float[] s_radii = new float[Slots];
        private static readonly double[] s_times = new double[Slots];
        private static int s_next;

        static PitchKingBurnBoard() => Reset();

        /// <summary>How many times fire burnt the King's tar (the driver speaks when it changes).</summary>
        public static int Burns { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            for (int i = 0; i < Slots; i++)
            {
                s_points[i] = Vector2.zero;
                s_radii[i] = 0f;
                s_times[i] = double.NegativeInfinity;
            }

            s_next = 0;
            Burns = 0;
        }

        public static void Record(Vector2 pointWorld, float radius, double at)
        {
            s_points[s_next] = pointWorld;
            s_radii[s_next] = radius;
            s_times[s_next] = at;
            s_next = (s_next + 1) % Slots;
        }

        /// <summary>Whether a fire recorded during the warning (from <paramref name="warnedAt"/> to <paramref name="now"/>) burnt the tar round <paramref name="kingWorld"/>.</summary>
        public static bool BurnsTar(Vector2 kingWorld, double warnedAt, double now)
        {
            for (int i = 0; i < Slots; i++)
            {
                if (PitchKingBurnRules.Burns(kingWorld, s_points[i], s_radii[i], s_times[i], warnedAt, now))
                {
                    Burns++;
                    return true;
                }
            }

            return false;
        }
    }
}
