using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One rising current of the Cova do Vento Destruidor (map 86), in the region's own coordinates: the vent on the floor of the shaft beside
    /// the Lâmpada 3 and the landing on the far ring of the shaft (<c>OverworldLayouts.ProfundoUpdrafts</c>). One way only: up.
    /// </summary>
    public readonly struct ProfundoUpdraft
    {
        public readonly string Id;
        public readonly Vector2 Vent;
        public readonly Vector2 Landing;

        public ProfundoUpdraft(string id, Vector2 vent, Vector2 landing)
        {
            Id = id;
            Vent = vent;
            Landing = landing;
        }
    }

    /// <summary>Why a ride up may not start (<see cref="ProfundoUpdraftRules.Check"/>).</summary>
    public enum ProfundoUpdraftCheck
    {
        Ok = 0,
        ZoneBlocked = 1,
        Resting = 2,
        TideShut = 3,
    }

    /// <summary>
    /// The rising currents of the Pit of Ruinous Wind (Docs/Mapas100.md 86, "correntes: pular nas correntes de ar para subir"), pure and
    /// server-runnable. Four vents on the floor of the shaft, round the Lâmpada 3, each carry the hero up to a landing on the outer ring, over
    /// the five terraces and their beasts: the roads leave the post only west, east and south, and the four landings are 160–200 m off them,
    /// on the rings of the Plataformas Soltas and of the Correntes Ascendentes. While the Tide of Darkness rises (<see cref="ProfundoLampRules"/>) the wind turns down and no
    /// vent lifts. The harpies peck at whoever hangs in the wind, never past a floor of life; the gust of the landing knocks the beasts there.
    /// <code>
    /// vent reach 4 m · 3 s between rides · lift 26 m/s on an arc 24 m above the line · 4 currents of 340–410 m: 13–16 s
    /// walking instead: 340–410 m straight up the terraces (62–75 s at 5.5 m/s, through the clearings), 550–600 m by the roads
    /// harpies: a peck every 3 s in the air, 2 % of max life (3 % at night), never under 30 %
    /// landing gust: beasts within 9 m lose 6 % of max life (a boss 1 %)
    /// </code>
    /// </summary>
    public static class ProfundoUpdraftRules
    {
        public const float Reach = 4f;
        public const float CooldownSeconds = 3f;
        public const float LiftSpeed = 26f;
        public const float LiftArc = 24f;
        public const float PeckSeconds = 3f;
        public const float PeckFraction = 0.02f;
        public const float NightPeckFraction = 0.03f;
        public const float PeckFloorFraction = 0.3f;
        public const float GustRadius = 9f;
        public const float GustFraction = 0.06f;
        public const float GustBossFraction = 0.01f;

        /// <summary>The vent within <see cref="Reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int VentAt(IReadOnlyList<ProfundoUpdraft> updrafts, Vector2 p)
        {
            int best = -1;
            float bestSq = Reach * Reach;
            for (int i = 0; i < updrafts.Count; i++)
            {
                float sq = (updrafts[i].Vent - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Can the ride start? <paramref name="zonesOpen"/> = the soft barrier lets the hero over every zone under the current.</summary>
        public static ProfundoUpdraftCheck Check(bool tideRising, bool zonesOpen, float sinceRide)
        {
            if (tideRising)
            {
                return ProfundoUpdraftCheck.TideShut;
            }

            if (!zonesOpen)
            {
                return ProfundoUpdraftCheck.ZoneBlocked;
            }

            return sinceRide < CooldownSeconds ? ProfundoUpdraftCheck.Resting : ProfundoUpdraftCheck.Ok;
        }

        /// <summary>Seconds of the ride up.</summary>
        public static float RideSeconds(ProfundoUpdraft updraft) => Vector2.Distance(updraft.Vent, updraft.Landing) / LiftSpeed;

        /// <summary>How many pecks are due after <paramref name="elapsed"/> seconds in the air (the first at <see cref="PeckSeconds"/>).</summary>
        public static int PecksDue(float elapsed) => Mathf.FloorToInt(Mathf.Max(0f, elapsed) / PeckSeconds);

        /// <summary>One peck of the harpies: a slice of max life that never takes the hero under <see cref="PeckFloorFraction"/>.</summary>
        public static float PeckAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightPeckFraction : PeckFraction, PeckFloorFraction);

        /// <summary>The landing gust on a beast within <see cref="GustRadius"/>.</summary>
        public static float GustAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? GustBossFraction : GustFraction);
    }
}
