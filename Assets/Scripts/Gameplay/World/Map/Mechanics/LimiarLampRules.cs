using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One lamp niche on the way (<see cref="Id"/> is what the save keeps, so the list can grow).</summary>
    public readonly struct LimiarLamp
    {
        public readonly string Id;
        public readonly Vector2 Position;

        public LimiarLamp(string id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }

    public enum LimiarLampAction
    {
        None = 0,
        Light = 1,
        ReturnToVila = 2,
        GoToDeepest = 3,
    }

    /// <summary>
    /// The lamps of the Grutas do Limiar (Docs/Mapas100.md 19: "cada lâmpada acesa no caminho fica acesa (salva) e encurta a volta da
    /// cidade"), pure (no MonoBehaviour, a server holds the same set of ids). Lamp 0 is the Vila's, always lit: from it [E] goes to the
    /// deepest lamp the hero has lit; from any other lit lamp [E] goes back to the Vila; an unlit lamp is lit by [E]. The list runs from
    /// the Vila outwards, so "deepest" is simply the highest index lit. Numbers: reach 5 m; 20 s between trips, so a lamp is a
    /// shortcut between fights and not a way out of one. Nothing here costs or blocks: the roads stay open with no lamp lit.
    /// </summary>
    public static class LimiarLampRules
    {
        public const int VilaLamp = 0;
        public const float ReachRadius = 5f;
        public const float TravelCooldownSeconds = 20f;

        /// <summary>Index of the lamp within reach of <paramref name="local"/> (the closest), or -1.</summary>
        public static int LampAt(Vector2 local, IReadOnlyList<LimiarLamp> lamps)
        {
            int best = -1;
            float bestSqr = ReachRadius * ReachRadius;
            for (int i = 0; lamps != null && i < lamps.Count; i++)
            {
                float sqr = (lamps[i].Position - local).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        public static bool IsLit(int lamp, IReadOnlyList<LimiarLamp> lamps, ICollection<string> lit)
        {
            return lamp == VilaLamp || (lamps != null && lamp >= 0 && lamp < lamps.Count && lit != null && lit.Contains(lamps[lamp].Id));
        }

        /// <summary>Highest lamp lit other than the Vila's, or -1 when the hero has lit none yet.</summary>
        public static int DeepestLit(IReadOnlyList<LimiarLamp> lamps, ICollection<string> lit)
        {
            for (int i = (lamps?.Count ?? 0) - 1; i > VilaLamp; i--)
            {
                if (IsLit(i, lamps, lit))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>What [E] does at this lamp.</summary>
        public static LimiarLampAction ActionAt(int lamp, IReadOnlyList<LimiarLamp> lamps, ICollection<string> lit)
        {
            if (lamp < 0 || lamps == null || lamp >= lamps.Count)
            {
                return LimiarLampAction.None;
            }

            if (!IsLit(lamp, lamps, lit))
            {
                return LimiarLampAction.Light;
            }

            if (lamp != VilaLamp)
            {
                return LimiarLampAction.ReturnToVila;
            }

            return DeepestLit(lamps, lit) >= 0 ? LimiarLampAction.GoToDeepest : LimiarLampAction.None;
        }

        /// <summary>The lamp a trip from <paramref name="lamp"/> ends at, or -1 when it goes nowhere.</summary>
        public static int TravelTarget(int lamp, IReadOnlyList<LimiarLamp> lamps, ICollection<string> lit)
        {
            switch (ActionAt(lamp, lamps, lit))
            {
                case LimiarLampAction.ReturnToVila: return VilaLamp;
                case LimiarLampAction.GoToDeepest: return DeepestLit(lamps, lit);
                default: return -1;
            }
        }

        public static bool CanTravel(float now, float lastTravel) => now - lastTravel >= TravelCooldownSeconds;
    }
}
