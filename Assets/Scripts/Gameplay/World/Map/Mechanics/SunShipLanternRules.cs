using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What happens to a carried lantern this frame (<see cref="SunShipLanternRules.Step"/>).</summary>
    public enum LanternStep
    {
        Burning = 0,

        /// <summary>It burnt down on the way.</summary>
        BurntOut = 1,

        /// <summary>The hero was carried (a ride, a teleport): the light does not travel by lift.</summary>
        Carried = 2,
    }

    /// <summary>
    /// The lanterns between the barques (Docs/Mapas100.md 97: "carregar a luz: levar lanternas da Barca da Lua (mapa 93) para cá completa a viagem
    /// da luz"), pure and server-runnable. At the lantern rack by the Cabine da Proa on the Moon's deck [E] takes a lantern of the moon; the hero
    /// walks it up the river of light to the stand by the Cabine da Popa on the Sun's deck and [E] sets it there. The flame burns down as it goes
    /// and every blow the hero takes shakes some of it out; a ride, a teleport or the barque would carry the hero, and the light does not travel by
    /// lift: it goes out. So the lantern is a walk (on foot or mounted) by the safest road, not the quickest trick. Every lantern set is paid; the
    /// fifth of the UTC day completes the day's voyage of the light and pays the voyage.
    /// <code>
    /// flame: 600 s (the walk up the river of light, ≈ 1.9 km, is ≈ 5 min) · each blow −15 s · a jump of more than 25 m in one frame, or another ride
    /// holding the controls, puts it out · 5 lanterns a UTC day: 60 gold each, +300 with the fifth (the count of the day in one time mark)
    /// </code>
    /// </summary>
    public static class SunShipLanternRules
    {
        public const float FlameSeconds = 600f;
        public const float BlowSeconds = 15f;
        public const float CarriedJump = 25f;
        public const int LanternsPerVoyage = 5;
        public const int GoldPerLantern = 60;
        public const int VoyageGold = 300;
        public const float StationReach = 4f;

        /// <summary>The flame left after <paramref name="deltaTime"/> seconds and <paramref name="blows"/> blows.</summary>
        public static float Burn(float flame, float deltaTime, int blows) => Mathf.Max(0f, flame - Mathf.Max(0f, deltaTime) - Mathf.Max(0, blows) * BlowSeconds);

        /// <summary>The save key of the lanterns set today (a time mark: <see cref="StoreSet"/>).</summary>
        public const string SetKey = "ceu.sun_ship.lanterns_set";

        /// <summary>
        /// What happens to the lantern after the hero moved <paramref name="jump"/> metres in a frame with <paramref name="flame"/> seconds left;
        /// <paramref name="carriedByRide"/>: something else holds the hero's controls (a ride of another station, the barque).
        /// </summary>
        public static LanternStep Step(float flame, float jump, bool carriedByRide = false)
        {
            if (carriedByRide || jump > CarriedJump)
            {
                return LanternStep.Carried;
            }

            return flame <= 0f ? LanternStep.BurntOut : LanternStep.Burning;
        }

        /// <summary>Whether another lantern may be set today (<paramref name="setToday"/> already set).</summary>
        public static bool CanSet(int setToday) => setToday < LanternsPerVoyage;

        /// <summary>The gold for setting a lantern when <paramref name="setToday"/> were already set today (the fifth also pays the voyage).</summary>
        public static int GoldFor(int setToday)
        {
            if (!CanSet(setToday))
            {
                return 0;
            }

            return GoldPerLantern + (setToday + 1 == LanternsPerVoyage ? VoyageGold : 0);
        }

        /// <summary>The lanterns set on UTC day <paramref name="day"/>, read from the stored mark (0 when it is another day's).</summary>
        public static int SetOn(long stored, long day) => stored > 0L && stored / 10L == day ? (int)(stored % 10L) : 0;

        /// <summary>The mark for <paramref name="count"/> lanterns set on UTC day <paramref name="day"/> (day × 10 + count).</summary>
        public static long StoreSet(long day, int count) => day * 10L + System.Math.Max(0, System.Math.Min(9, count));

        /// <summary>Whether a point is within reach of a station.</summary>
        public static bool AtStation(Vector2 station, Vector2 p) => (station - p).sqrMagnitude <= StationReach * StationReach;
    }
}
