using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The dark of the Cova da Treva (Docs/Mapas100.md 88: "escuro: bichos no escuro recebem −30% de dano; iluminados (lanterna, Luz da Maria,
    /// skills de luz), dano normal. É um jogo de luz, não de força"), pure and server-runnable. In the Cova da Treva and the Túneis Cegos a beast
    /// out of every light takes 30% less damage (a +30% Defense on it). A beast is lit when it stands within the hero's Runic Lamp (a short
    /// circle), within Maria's own light when Maria plays (a wide one), within a lit lamp stand of the pit, or for a few seconds after a light skill
    /// (Holy) hits it. The stands along the roads are out: [E] at one lights it for 10 min, so where to fight is chosen by where the light is.
    /// <code>
    /// dark: +0.30 Defense (−30% damage taken) · lit: the hero's lamp 5 m, Maria's light 10 m, a lit stand 16 m, a Holy hit 6 s, the Lâmpada 5 (post) 24 m
    /// a stand burns 600 s once lit · reach 4 m
    /// </code>
    /// </summary>
    public static class DarkPitLightRules
    {
        public const float DarkDefense = 0.30f;
        public const float LampRadius = 5f;
        public const float MariaRadius = 10f;
        public const float StandRadius = 16f;
        public const float PostRadius = 24f;
        public const float HolyLitSeconds = 6f;
        public const float StandBurnSeconds = 600f;
        public const float StationReach = 4f;

        /// <summary>The save key of stand <paramref name="stand"/>: a time mark holding the unix second it burns until (written only by [E]).</summary>
        public static string StandKey(int stand) => "profundo.dark_pit.stand_" + stand;

        /// <summary>The radius of the hero's own light: Maria's own (wide), else a carried lamp's (short), else none (0).</summary>
        public static float HeroLightRadius(bool maria, bool lamp) => maria ? MariaRadius : lamp ? LampRadius : 0f;

        /// <summary>The multiplier on the damage a beast takes (1 lit, 0.7 in the dark).</summary>
        public static float DamageTakenMultiplier(bool lit) => lit ? 1f : 1f - DarkDefense;

        /// <summary>Whether a stand lit until <paramref name="litUntil"/> still burns at <paramref name="now"/>.</summary>
        public static bool StandBurns(double litUntil, double now) => now < litUntil;

        /// <summary>The time a stand lit at <paramref name="now"/> burns until.</summary>
        public static double LightStand(double now) => now + StandBurnSeconds;

        /// <summary>
        /// Whether a beast at <paramref name="beast"/> is lit: within <paramref name="heroRadius"/> of the hero (<see cref="HeroLightRadius"/>),
        /// within <see cref="StandRadius"/> of a burning stand, within <see cref="PostRadius"/> of the post, or still glowing from a Holy hit
        /// (<paramref name="holyUntil"/>).
        /// </summary>
        public static bool Lit(Vector2 beast, Vector2 hero, float heroRadius, IReadOnlyList<Vector2> stands, IReadOnlyList<double> standLitUntil, Vector2 post,
            double holyUntil, double now)
        {
            if (now < holyUntil)
            {
                return true;
            }

            float r = Mathf.Max(0f, heroRadius);
            if ((r > 0f && (beast - hero).sqrMagnitude <= r * r) || (beast - post).sqrMagnitude <= PostRadius * PostRadius)
            {
                return true;
            }

            for (int i = 0; stands != null && i < stands.Count; i++)
            {
                if (standLitUntil != null && i < standLitUntil.Count && StandBurns(standLitUntil[i], now) && (beast - stands[i]).sqrMagnitude <= StandRadius * StandRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The stand within <see cref="StationReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int StandAt(IReadOnlyList<Vector2> stands, Vector2 p)
        {
            for (int i = 0; stands != null && i < stands.Count; i++)
            {
                if ((stands[i] - p).sqrMagnitude <= StationReach * StationReach)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
