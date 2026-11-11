using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a hero at the Moon Pier can do now (<see cref="CeuMoonBarqueRules.Check"/>).</summary>
    public enum CeuBarqueCheck
    {
        /// <summary>It is night, and the deck lets the hero in.</summary>
        Board = 0,
        /// <summary>The Barca da Lua only comes in at night.</summary>
        NotNight = 1,
        /// <summary>The soft barrier would throw the hero back on the deck.</summary>
        ZoneBlocked = 2,
    }

    /// <summary>
    /// The Barca da Lua (Docs/Mapas100.md, maps 91 and 93), pure: at night it lies at the Moon Pier and carries the hero up to its deck, by the Cabine da Proa; the
    /// way back from the deck to the pier runs at any hour, and both ends are on the roads, so a missed night is a walk, never a dead end.
    /// <code>
    /// boarding 2.5 s (the only moment the controls are off) · pier and berth reach 6 m · night is the game clock's (20 h to 5 h)
    /// </code>
    /// </summary>
    public static class CeuMoonBarqueRules
    {
        public const float BoardingSeconds = 2.5f;
        public const float Reach = 6f;

        /// <summary>Height (m) of the bob while boarding.</summary>
        public const float BoardingArc = 0.6f;

        public const float HoursPerDay = 24f;

        public static CeuBarqueCheck Check(bool isNight, bool deckOpen)
        {
            if (!isNight)
            {
                return CeuBarqueCheck.NotNight;
            }

            return deckOpen ? CeuBarqueCheck.Board : CeuBarqueCheck.ZoneBlocked;
        }

        public static bool WithinReach(Vector2 point, Vector2 hero) => (point - hero).sqrMagnitude <= Reach * Reach;

        /// <summary>Whole game hours (rounded up) from <paramref name="hour"/> to <paramref name="nightStart"/>, for the texts.</summary>
        public static int HoursUntilNight(float hour, float nightStart) => Mathf.CeilToInt(Mathf.Repeat(nightStart - hour, HoursPerDay));
    }
}
