using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a tick of <see cref="CeuSkyHoldSession"/> brought.</summary>
    public enum CeuSkyEvent
    {
        None = 0,
        /// <summary>A drip of shadow falls in <see cref="CeuSkyHoldRules.DripWarningSeconds"/>: step out to dodge it.</summary>
        DripWarning = 1,
        /// <summary>The drip falls on whoever is still holding (<see cref="CeuSkyHoldRules.DripAmount"/>).</summary>
        Drip = 2,
        /// <summary>The crack closed.</summary>
        Sealed = 3,
        /// <summary>The hero stayed away until the pillar went out.</summary>
        Faded = 4,
    }

    /// <summary>
    /// "Segurar o céu" (Docs/Mapas100.md, map 99), pure: the five cracks round the Mão Aberta. The hero raises a pillar of light at a crack and holds it by staying
    /// close, fighting freely meanwhile; walking off only lets the pillar run down, so nothing ever pins the hero. The shadow drips on a clock with a warning first,
    /// and a drip can never take the hero below <see cref="DripFloor"/> of the health, so holding cannot kill. Sealed cracks stay sealed on the server's clock.
    /// <code>
    /// hold 20 s · reach 7 m · away the pillar runs down 2 s per second
    /// drip every 6 s (warned 1.5 s before): 5% of max health, never below 30% of it (at most 15% in a whole hold)
    /// sealing mends 30% of max health and bites the beasts within 20 m for 15% of their max health (bosses untouched)
    /// sealed for 600 s
    /// </code>
    /// </summary>
    public static class CeuSkyHoldRules
    {
        public const int Cracks = 5;
        public const float HoldSeconds = 20f;
        public const float Reach = 7f;
        public const float DecayRate = 2f;
        public const float DripInterval = 6f;
        public const float DripWarningSeconds = 1.5f;
        public const float DripFraction = 0.05f;
        public const float DripFloor = 0.3f;
        public const float SealHealFraction = 0.3f;
        public const float SealBiteFraction = 0.15f;
        public const float SealBiteRadius = 20f;
        public const long SealSeconds = 600;

        /// <summary>Health a drip takes: <see cref="DripFraction"/> of the max, cut short so the hero is left at <see cref="DripFloor"/> at the lowest.</summary>
        public static float DripAmount(float current, float max)
        {
            if (max <= 0f || current <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(max * DripFraction, Mathf.Max(0f, current - max * DripFloor));
        }

        public static float SealHeal(float max) => Mathf.Max(0f, max) * SealHealFraction;

        public static float SealBite(float enemyMax, bool isBoss) => isBoss ? 0f : Mathf.Max(0f, enemyMax) * SealBiteFraction;

        /// <summary>The nearest crack within <see cref="Reach"/> of <paramref name="p"/>, or −1.</summary>
        public static int NearestCrack(IReadOnlyList<Vector2> cracks, Vector2 p)
        {
            int best = -1;
            float bestSqr = Reach * Reach;
            for (int i = 0; i < cracks.Count; i++)
            {
                float sqr = (cracks[i] - p).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }
    }

    /// <summary>One hero's holding of the sky: at most one pillar up, and when each crack reopens (Unix seconds). No Unity object: it can run on the server.</summary>
    public sealed class CeuSkyHoldSession
    {
        private readonly long[] _sealedUntil = new long[CeuSkyHoldRules.Cracks];
        private float _dripClock;

        /// <summary>The crack whose pillar is up, or −1.</summary>
        public int ActiveCrack { get; private set; } = -1;

        /// <summary>Seconds of hold accumulated on the pillar that is up.</summary>
        public float Progress { get; private set; }

        /// <summary>True between the warning and the drip.</summary>
        public bool DripImminent { get; private set; }

        public float Fraction => Mathf.Clamp01(Progress / CeuSkyHoldRules.HoldSeconds);

        public bool IsSealed(int crack, long now) => crack >= 0 && crack < _sealedUntil.Length && now < _sealedUntil[crack];

        public int SealedCount(long now)
        {
            int count = 0;
            for (int i = 0; i < _sealedUntil.Length; i++)
            {
                count += IsSealed(i, now) ? 1 : 0;
            }

            return count;
        }

        /// <summary>Raises a pillar; false if the crack is sealed or invalid, or a pillar is already up.</summary>
        public bool TryRaise(int crack, long now)
        {
            if (ActiveCrack >= 0 || crack < 0 || crack >= _sealedUntil.Length || IsSealed(crack, now))
            {
                return false;
            }

            ActiveCrack = crack;
            Progress = 0f;
            ResetDrip();
            return true;
        }

        public void Cancel()
        {
            ActiveCrack = -1;
            Progress = 0f;
            ResetDrip();
        }

        /// <summary>One step. <paramref name="inReach"/>: the hero is within <see cref="CeuSkyHoldRules.Reach"/> of the active crack.</summary>
        public CeuSkyEvent Tick(float deltaTime, bool inReach, long now)
        {
            if (ActiveCrack < 0)
            {
                return CeuSkyEvent.None;
            }

            if (!inReach)
            {
                ResetDrip();
                Progress -= deltaTime * CeuSkyHoldRules.DecayRate;
                if (Progress > 0f)
                {
                    return CeuSkyEvent.None;
                }

                Cancel();
                return CeuSkyEvent.Faded;
            }

            Progress += deltaTime;
            if (Progress >= CeuSkyHoldRules.HoldSeconds)
            {
                _sealedUntil[ActiveCrack] = now + CeuSkyHoldRules.SealSeconds;
                Cancel();
                return CeuSkyEvent.Sealed;
            }

            _dripClock += deltaTime;
            if (_dripClock >= CeuSkyHoldRules.DripInterval)
            {
                ResetDrip();
                return CeuSkyEvent.Drip;
            }

            if (!DripImminent && _dripClock >= CeuSkyHoldRules.DripInterval - CeuSkyHoldRules.DripWarningSeconds)
            {
                DripImminent = true;
                return CeuSkyEvent.DripWarning;
            }

            return CeuSkyEvent.None;
        }

        private void ResetDrip()
        {
            _dripClock = 0f;
            DripImminent = false;
        }
    }
}
