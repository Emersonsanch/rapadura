using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What Ammo's caravan on the Estrada das Caravanas is doing (<see cref="CaravanRoadEscortRules.StateFor"/>).</summary>
    public enum CaravanRoadState
    {
        Walking = 0,

        /// <summary>The hero fell behind in the calm: the camels wait on the road.</summary>
        Waiting = 1,

        /// <summary>The sand blows and nobody walks beside the camels: they kneel and the caravan asks for its escort.</summary>
        Kneeling = 2,
    }

    /// <summary>
    /// The escort of Ammo's caravan through the sandstorm on the Estrada das Caravanas (Docs/Mapas100.md 07: "tempestade de areia a cada ~10 min:
    /// visão 6 m, a caravana para e pede escolta"), pure and server-runnable on the storm's own clock (<see cref="AreiasStormRules"/>). Ammo sets out
    /// from his camp below the Pomar gate only when the sand is coming (from <see cref="DepartLead"/> s before the storm's warning, or in the storm
    /// itself), so every escort walks into one. In the calm the camels walk while the hero keeps within <see cref="LeashRange"/>; in the storm they
    /// walk only with the hero at their side (<see cref="StormBesideRange"/>), otherwise they kneel and ask for the escort. Walking beside them is a
    /// moving shelter: within <see cref="ShelterRange"/> of a camel the storm does not wear the hero down, so with the caravan the road goes on
    /// through the storm instead of stopping at a fire or an arch. When the sand starts, the beasts round the caravan come for it. Leaving it far
    /// behind or falling ends the escort; arriving at Qocho pays <see cref="Purse"/> gold nuggets, once per UTC day.
    /// <code>
    /// road ≈ 650 m at 2.2 m/s (≈ 295 s, more than the 270 s lead + the 15 s warning: the storm always meets the caravan)
    /// calm: walks with the hero ≤ 25 m · storm: walks with the hero ≤ 6 m, kneels otherwise · shelter ≤ 7 m of a camel, storm only
    /// sand starts: beasts ≤ 50 m of the head (no boss) come at it · hero &gt; 120 m away: escort lost · 30 gold, once per UTC day
    /// </code>
    /// </summary>
    public static class CaravanRoadEscortRules
    {
        public const float Speed = 2.2f;
        public const float LeashRange = 25f;
        public const float StormBesideRange = 6f;
        public const float ShelterRange = 7f;
        public const float AmbushRange = 50f;
        public const float LostRange = 120f;
        public const float StartReach = 5f;
        public const float DepartLead = 270f;
        public const int Purse = 30;

        /// <summary>The daily mark (UTC day) of the escort's pay.</summary>
        public const string RewardKey = "areias.caravan_road.escort";

        /// <summary>Seconds from <paramref name="unixSeconds"/> to the next storm warning (0 while the warning or the storm is on).</summary>
        public static float SecondsToWarning(double unixSeconds)
        {
            float t = AreiasStormRules.SecondOfCycle(unixSeconds);
            return t >= AreiasStormRules.WarningStart ? 0f : AreiasStormRules.WarningStart - t;
        }

        /// <summary>Whether Ammo sets out now: the sand is coming within <see cref="DepartLead"/> s, or already here.</summary>
        public static bool CanDepart(double unixSeconds) => SecondsToWarning(unixSeconds) <= DepartLead;

        /// <summary>Seconds until Ammo will set out (0 when he would now).</summary>
        public static float SecondsToDeparture(double unixSeconds) => Mathf.Max(0f, SecondsToWarning(unixSeconds) - DepartLead);

        /// <summary>What the caravan does with the hero <paramref name="heroDistance"/> m from its head in <paramref name="phase"/>.</summary>
        public static CaravanRoadState StateFor(AreiasStormPhase phase, float heroDistance)
        {
            if (phase == AreiasStormPhase.Storm)
            {
                return heroDistance <= StormBesideRange ? CaravanRoadState.Walking : CaravanRoadState.Kneeling;
            }

            return heroDistance <= LeashRange ? CaravanRoadState.Walking : CaravanRoadState.Waiting;
        }

        /// <summary>Whether the camels shelter a hero <paramref name="nearestCamel"/> m from the nearest of them (only the storm wears the hero).</summary>
        public static bool Shelters(AreiasStormPhase phase, float nearestCamel) => phase == AreiasStormPhase.Storm && nearestCamel <= ShelterRange;

        /// <summary>Whether the hero left the caravan behind for good.</summary>
        public static bool Lost(float heroDistance) => heroDistance > LostRange;

        /// <summary>Whether the sand started between two frames (the beasts come for the caravan then).</summary>
        public static bool SandStarted(AreiasStormPhase before, AreiasStormPhase now) => before != AreiasStormPhase.Storm && now == AreiasStormPhase.Storm;

        public static bool Paid(long paidDay, long today) => paidDay == today;
    }
}
