using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The mirror clock of the Torres Brancas de Amanirenas (Docs/Mapas100.md 42, §3.3: "Relógio de espelhos: os raios de luz entre as torres
    /// mudam com a hora; pôr os espelhos no lugar (quebra-cabeça) abre salas de baú"), pure and server-runnable. Four mirrors stand round the
    /// white city (north, east, south and west). The light leaves the great spire and goes round them in order: it reaches the next mirror only
    /// if this one faces the right way, and the right way changes every six hours of the game clock. [E] at a mirror (from 4 m) turns it a
    /// quarter. When the light goes all the way round, the vault of the spire opens: a chest, once per game day.
    /// <code>
    /// 4 mirrors · state 0–3 · right state of mirror i in hour band b (6 h each) = (i + b + 1) mod 4 · the beam lights mirrors in order while right
    /// all four right → the vault's chest, once per game day · [E] 4 m: a quarter turn
    /// </code>
    /// </summary>
    public static class MirrorClockRules
    {
        public const int MirrorCount = 4;
        public const float TurnReach = 4f;
        public const float BandHours = 6f;

        /// <summary>The daily mark of the vault (the game day its chest was last taken).</summary>
        public const string VaultKey = "mingcheng.torres.mirror_vault";

        /// <summary>The hour band (0–3) of <paramref name="hour"/>.</summary>
        public static int BandOf(float hour) => Mathf.FloorToInt(Mathf.Repeat(hour, 24f) / BandHours) % 4;

        /// <summary>The way mirror <paramref name="mirror"/> must face in band <paramref name="band"/>.</summary>
        public static int RightState(int mirror, int band) => ((mirror + band + 1) % 4 + 4) % 4;

        public static int Turn(int state) => (((state + 1) % 4) + 4) % 4;

        /// <summary>How many mirrors, in order from the first, the light reaches and sends on (0–4).</summary>
        public static int LitChain(IReadOnlyList<int> states, float hour)
        {
            int band = BandOf(hour);
            int lit = 0;
            for (int i = 0; states != null && i < states.Count && i < MirrorCount; i++)
            {
                if (states[i] != RightState(i, band))
                {
                    break;
                }

                lit++;
            }

            return lit;
        }

        public static bool Solved(IReadOnlyList<int> states, float hour) => LitChain(states, hour) >= MirrorCount;

        /// <summary>Whether the vault gives its chest: solved, and not yet taken today.</summary>
        public static bool VaultOpens(bool solved, int takenDay, int today) => solved && today > takenDay;

        /// <summary>The direction mirror state <paramref name="state"/> faces (x/z): 0 north, 1 east, 2 south, 3 west.</summary>
        public static Vector2 Facing(int state) => WindVaneRules.Direction(state);

        /// <summary>The mirror within <see cref="TurnReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int MirrorAt(IReadOnlyList<Vector2> mirrors, Vector2 p) => WindVaneRules.VaneAt(mirrors, p);
    }
}
