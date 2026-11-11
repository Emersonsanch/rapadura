using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum KaggenWindPhase
    {
        Calm = 0,
        /// <summary>The wind is coming: stand still and it passes.</summary>
        Warning = 1,
        Blowing = 2,
    }

    /// <summary>
    /// The winds of the Serra de Kaggen (Docs/Mapas100.md 13 and 14), a pure function of the clock: every 25 s, 3 s of warning and
    /// 2,5 s of wind. On the windy slopes (Encosta do Vento, Passo do Hahya) a gust shoves a walking hero 2,4 m/s (6 m at most,
    /// through the character controller, so walls hold); on a rope bridge the wind never shoves (nobody falls off) but running there
    /// spends twice the stamina. Standing still lets the wind pass; Maithe (Vento Vivo) ignores all of it.
    /// </summary>
    public static class KaggenWindRules
    {
        public const float CycleSeconds = 25f;
        public const float WarningSeconds = 3f;
        public const float BlowSeconds = 2.5f;
        public const float PushSpeed = 2.4f;
        public const float BridgeReach = 8f;

        /// <summary>Stamina drained on a bridge on top of the normal run, in multiples of the normal drain (1 = twice as fast in all).</summary>
        public const float BridgeExtraDrain = 1f;

        /// <summary>The second of the cycle the warning starts.</summary>
        public const float WarningStart = CycleSeconds - BlowSeconds - WarningSeconds;

        public static KaggenWindPhase PhaseAt(double seconds)
        {
            double t = seconds % CycleSeconds;
            t = t < 0d ? t + CycleSeconds : t;
            return t < WarningStart ? KaggenWindPhase.Calm : t < WarningStart + WarningSeconds ? KaggenWindPhase.Warning : KaggenWindPhase.Blowing;
        }

        public static long GustIndex(double seconds) => (long)System.Math.Floor(seconds / CycleSeconds);

        /// <summary>The gust's side alternates: east, then west, across the slope.</summary>
        public static Vector2 Direction(long gust) => (gust & 1L) == 0L ? Vector2.right : Vector2.left;

        public static bool InsideAny(Vector2 local, IReadOnlyList<Rect> areas)
        {
            for (int i = 0; areas != null && i < areas.Count; i++)
            {
                if (areas[i].Contains(local))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool OnBridge(Vector2 local, IReadOnlyList<Vector2> bridges)
        {
            for (int i = 0; bridges != null && i < bridges.Count; i++)
            {
                if ((bridges[i] - local).sqrMagnitude <= BridgeReach * BridgeReach)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Ignores(CharacterId? hero) => hero == CharacterId.Maithe;

        /// <summary>Velocity the gust adds; zero when the hero stands still or is Maithe.</summary>
        public static Vector2 PushVelocity(Vector2 direction, bool holdingStill, CharacterId? hero)
        {
            return holdingStill || Ignores(hero) ? Vector2.zero : direction.normalized * PushSpeed;
        }

        /// <summary>Extra run-drain seconds this frame (feed to <c>PlayerStats.DrainStamina</c>); zero off the bridge, walking or for Maithe.</summary>
        public static float ExtraBridgeDrain(bool onBridge, bool running, CharacterId? hero, float deltaTime)
        {
            return onBridge && running && !Ignores(hero) ? deltaTime * BridgeExtraDrain : 0f;
        }
    }
}
