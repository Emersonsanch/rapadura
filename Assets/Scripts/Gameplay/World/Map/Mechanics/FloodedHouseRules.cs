using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The flooded houses of Kilwa Afundada (Docs/Mapas100.md 29: "casas alagadas: entrar numa casa submersa pede Bolha de Ar; dentro,
    /// quebra-cabeças de porta entalhada"), pure and server-runnable. Four drowned houses keep a carved door each, with four carved stones round
    /// it: fish, star, moon and shell. The lintel shows three of them in an order that changes every game day (the same for everyone). With a
    /// Bolha de Ar Engarrafada in the bag the hero can dive to the door; [E] at a stone (from 2 m) presses it. The three in the lintel's order
    /// open the house (its chest, once per game day); a wrong stone and the door shuts, starting again.
    /// <code>
    /// 4 houses · stones fish/star/moon/shell 3 m round the door · lintel: 3 of the 4, order by (house, day) · [E] 2 m · needs the Air Bubble
    /// right order → chest, once per house per game day · wrong → start again
    /// </code>
    /// </summary>
    public static class FloodedHouseRules
    {
        public const int StoneCount = 4;
        public const int CodeLength = 3;
        public const float StoneRing = 3f;
        public const float PressReach = 2f;
        public const float DoorReach = 8f;

        private const string KeyPrefix = "costa.kilwa.house.";

        private static string[] s_keys;

        /// <summary>The daily mark of house <paramref name="house"/> (the game day it was last opened), built once.</summary>
        public static string HouseKey(int house)
        {
            if (s_keys == null)
            {
                var keys = new string[8];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = KeyPrefix + i;
                }

                s_keys = keys;
            }

            return house >= 0 && house < s_keys.Length ? s_keys[house] : KeyPrefix + house;
        }

        /// <summary>Where stone <paramref name="stone"/> (0 fish north, 1 star east, 2 moon south, 3 shell west) stands round a door.</summary>
        public static Vector2 StoneAt(Vector2 door, int stone) => door + WindVaneRules.Direction(stone) * StoneRing;

        /// <summary>The <paramref name="step"/>th stone of the lintel's code for <paramref name="house"/> on <paramref name="day"/>: three different stones.</summary>
        public static int CodeStone(int house, int day, int step)
        {
            // The stone left out, then a rotation of the other three: 4 × 3 = 12 codes, changing with the day.
            int seed = Mathf.Abs(house * 7 + day * 5);
            int left = seed % StoneCount;
            int start = (seed / StoneCount) % CodeLength;
            int k = (start + step) % CodeLength;
            int stone = k >= left ? k + 1 : k;
            return stone;
        }

        /// <summary>
        /// The code's progress after pressing <paramref name="stone"/> with <paramref name="pressed"/> right ones already: one more when it
        /// is the next of the code, 0 when it is wrong.
        /// </summary>
        public static int Press(int pressed, int stone, int house, int day)
        {
            if (pressed < 0 || pressed >= CodeLength)
            {
                pressed = 0;
            }

            return CodeStone(house, day, pressed) == stone ? pressed + 1 : 0;
        }

        public static bool Opened(int pressed) => pressed >= CodeLength;

        /// <summary>The stone within <see cref="PressReach"/> of <paramref name="p"/> round <paramref name="door"/>, or −1.</summary>
        public static int StoneNear(Vector2 door, Vector2 p)
        {
            for (int i = 0; i < StoneCount; i++)
            {
                if ((StoneAt(door, i) - p).sqrMagnitude <= PressReach * PressReach)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
