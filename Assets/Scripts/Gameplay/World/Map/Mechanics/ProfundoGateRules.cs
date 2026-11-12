using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One of the five gates of the pits in the rampart of the Fortaleza da Última Lâmpada (map 83), in the region's own coordinates
    /// (<c>OverworldLayouts.ProfundoGates</c>): <see cref="Spot"/> in front of the gate on the plaza, <see cref="Arrival"/> where the way back
    /// sets the hero down on the plaza, <see cref="Post"/> the pit's lamp post (84–88) whose arrival breaks the seal, and <see cref="Landing"/>
    /// on the post's road, where the passage comes out and the way back leaves from.
    /// </summary>
    public readonly struct ProfundoGate
    {
        public readonly string Id;
        public readonly Vector2 Spot;
        public readonly Vector2 Arrival;
        public readonly Vector2 Post;
        public readonly Vector2 Landing;

        public ProfundoGate(string id, Vector2 spot, Vector2 arrival, Vector2 post, Vector2 landing)
        {
            Id = id;
            Spot = spot;
            Arrival = arrival;
            Post = post;
            Landing = landing;
        }
    }

    /// <summary>Why a passage may not start (<see cref="ProfundoGateRules.Check"/>).</summary>
    public enum ProfundoGateCheck
    {
        Ok = 0,
        Sealed = 1,
        TideShut = 2,
        ZoneBlocked = 3,
        Resting = 4,
    }

    /// <summary>
    /// The five gates of the pits (Docs/Mapas100.md 83, "5 portões, um para cada cova"), pure and server-runnable. Each gate is sealed
    /// from the far side: the first time the hero reaches the pit's lamp post on foot, the seal breaks (saved with
    /// <c>WorldMapProgress.SetTimeMark</c>, for good). An open gate is a passage between the plaza and the post, both ways, except while the
    /// Tide of Darkness rises (<see cref="ProfundoLampRules"/>): then the guardians bar every gate. Each passage breathes the pit's dark
    /// element on the hero (a hazard, never past a floor of life); the gate's lamp, coming out, burns the beasts waiting at the landing.
    /// <code>
    /// seal breaks within 20 m of the pit's lamp post · gate reach 3.5 m (plaza), 4 m (landing) · 3 s between passages
    /// shut while the tide rises (40 s of the 240 s cycle)
    /// breath of the pit: 3 % of max life (4 % at the clock's night), never under 30 %
    /// gate lamp: beasts within 10 m of the landing lose 8 % of max life (a boss 1 %)
    /// on foot from the plaza, by the roads: Fumaça ~600 m, Fogo ~950 m, Vento ~1250 m, Lodo ~1080 m, Treva ~1900 m
    /// </code>
    /// </summary>
    public static class ProfundoGateRules
    {
        public const int GateCount = 5;
        public const float UnlockRadius = 20f;
        public const float SpotReach = 3.5f;
        public const float LandingReach = 4f;
        public const float CooldownSeconds = 3f;
        public const float BreathFraction = 0.03f;
        public const float NightBreathFraction = 0.04f;
        public const float BreathFloorFraction = 0.3f;
        public const float LampRadius = 10f;
        public const float LampFraction = 0.08f;
        public const float LampBossFraction = 0.01f;

        /// <summary>The save key of gate <paramref name="gate"/> (a time mark: when its seal broke; 0 = still sealed).</summary>
        public static string MarkKey(int gate)
        {
            // Read every frame by the driver: each key is built once (same text as "profundo.gate." + gate).
            if (gate < 0 || gate >= 256)
            {
                return "profundo.gate." + gate;
            }

            if (s_markKeys == null || gate >= s_markKeys.Length)
            {
                System.Array.Resize(ref s_markKeys, gate + 1);
            }

            return s_markKeys[gate] ??= "profundo.gate." + gate;
        }

        private static string[] s_markKeys;

        /// <summary>True once the seal of a gate has broken (any positive mark).</summary>
        public static bool IsOpen(long mark) => mark > 0L;

        /// <summary>The gate whose pit's lamp post is within <see cref="UnlockRadius"/> of <paramref name="p"/>, or −1.</summary>
        public static int PostAt(IReadOnlyList<ProfundoGate> gates, Vector2 p) => Nearest(gates, p, UnlockRadius, g => g.Post);

        /// <summary>The gate whose plaza spot is within <see cref="SpotReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int SpotAt(IReadOnlyList<ProfundoGate> gates, Vector2 p) => Nearest(gates, p, SpotReach, g => g.Spot);

        /// <summary>The gate whose landing is within <see cref="LandingReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int LandingAt(IReadOnlyList<ProfundoGate> gates, Vector2 p) => Nearest(gates, p, LandingReach, g => g.Landing);

        /// <summary>Can the passage start? <paramref name="tideRising"/> = <see cref="ProfundoLampRules.PhaseAt"/> is Rising.</summary>
        public static ProfundoGateCheck Check(bool open, bool tideRising, bool zoneOpen, float sincePassage)
        {
            if (!open)
            {
                return ProfundoGateCheck.Sealed;
            }

            if (tideRising)
            {
                return ProfundoGateCheck.TideShut;
            }

            if (!zoneOpen)
            {
                return ProfundoGateCheck.ZoneBlocked;
            }

            return sincePassage < CooldownSeconds ? ProfundoGateCheck.Resting : ProfundoGateCheck.Ok;
        }

        /// <summary>The breath of the pit on a passage: a slice of max life that never takes the hero under <see cref="BreathFloorFraction"/>.</summary>
        public static float BreathAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightBreathFraction : BreathFraction, BreathFloorFraction);

        /// <summary>The gate lamp on a beast within <see cref="LampRadius"/> of the landing.</summary>
        public static float LampAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? LampBossFraction : LampFraction);

        private static int Nearest(IReadOnlyList<ProfundoGate> gates, Vector2 p, float reach, System.Func<ProfundoGate, Vector2> at)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; i < gates.Count; i++)
            {
                float sq = (at(gates[i]) - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }
    }
}
