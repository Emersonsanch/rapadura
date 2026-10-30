using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// The numbers and geometry of the M3 wave-1 abilities (Docs/Mounts.md §3, <see cref="MountWaveCatalog"/>), pure so
    /// the server can run the same checks (who is hit, for how long, by how much). The scene side lives in
    /// <see cref="MountWaveAbilities"/> / <see cref="MountAbilityEffects"/>.
    /// </summary>
    public static class MountWaveAbilityRules
    {
        /// <summary>A trail (Caititu, Ema de Fogo) drops one point every this many seconds.</summary>
        public const float TrailPointSeconds = 0.25f;

        /// <summary>Burn of the Ema de Fogo's trail and the Boitatá's ring lasts this long after the last touch.</summary>
        public const float BurnSeconds = 3f;

        /// <summary>The trail speed buff lingers this long after stepping off the leaves.</summary>
        public const float TrailBuffSeconds = 0.6f;

        /// <summary>Salto da Onça: the guaranteed critical waits this long for the next blow.</summary>
        public const float PounceMarkSeconds = 4f;

        /// <summary>The Onça lands this far short of the critter it jumps on.</summary>
        public const float PounceStopShort = 1.2f;

        /// <summary>Investida do Vaqueiro: how far critters get pushed (§3.2: 3 m).</summary>
        public const float KnockbackPush = 3f;

        /// <summary>Seconds the push takes.</summary>
        public const float KnockbackPushSeconds = 0.3f;

        /// <summary>Distance a common critter is put at outside the Boitatá's ring.</summary>
        public const float RepelMargin = 0.4f;

        /// <summary>Health healed per second by the Unicórnio's aura: <paramref name="fraction"/> of the max health.</summary>
        public static float HealPerSecond(float maxHealth, float fraction) => Mathf.Max(0f, maxHealth) * Mathf.Max(0f, fraction);

        /// <summary>Whether <paramref name="target"/> is inside the flat (XZ) circle of <paramref name="radius"/> around <paramref name="center"/>.</summary>
        public static bool InRadius(Vector3 center, Vector3 target, float radius)
        {
            float dx = target.x - center.x;
            float dz = target.z - center.z;
            return dx * dx + dz * dz <= radius * radius;
        }

        /// <summary>Flat distance from <paramref name="point"/> to the segment <paramref name="a"/>–<paramref name="b"/>.</summary>
        public static float DistanceToSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector2 a2 = new Vector2(a.x, a.z);
            Vector2 b2 = new Vector2(b.x, b.z);
            Vector2 p2 = new Vector2(point.x, point.z);
            Vector2 ab = b2 - a2;
            float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector2.Dot(p2 - a2, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(a2 + ab * t, p2);
        }

        /// <summary>Investida: a critter within <paramref name="radius"/> of the charge's path is pushed.</summary>
        public static bool InChargePath(Vector3 start, Vector3 end, Vector3 point, float radius) => DistanceToSegment(start, end, point) <= radius;

        /// <summary>Flat direction a push sends <paramref name="target"/> away from <paramref name="from"/> (forward when on top of it).</summary>
        public static Vector3 PushDirection(Vector3 from, Vector3 target, Vector3 fallbackForward)
        {
            Vector3 away = target - from;
            away.y = 0f;
            if (away.sqrMagnitude < 1e-4f)
            {
                away = fallbackForward;
                away.y = 0f;
            }

            return away.sqrMagnitude < 1e-6f ? Vector3.forward : away.normalized;
        }

        /// <summary>
        /// Anel de Fogo: where a common critter inside the ring goes (to the edge plus <see cref="RepelMargin"/>), or its
        /// own position when it is already outside. Bosses are never moved (the caller checks <c>BossMultiplier</c>).
        /// </summary>
        public static Vector3 RepelTo(Vector3 center, Vector3 target, float radius)
        {
            if (!InRadius(center, target, radius))
            {
                return target;
            }

            Vector3 dir = PushDirection(center, target, Vector3.forward);
            Vector3 edge = center + dir * (radius + RepelMargin);
            edge.y = target.y;
            return edge;
        }

        /// <summary>Whether <paramref name="point"/> touches a trail made of <paramref name="trail"/> points of <paramref name="radius"/>.</summary>
        public static bool OnTrail(IReadOnlyList<Vector3> trail, Vector3 point, float radius)
        {
            if (trail == null)
            {
                return false;
            }

            for (int i = 0; i < trail.Count; i++)
            {
                if (InRadius(trail[i], point, radius))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Passo Sem Pegada: the radius a critter notices the cousin at (<paramref name="scale"/> = 0.2 while hidden).</summary>
        public static float DetectionRadius(float baseRadius, bool hidden, float scale)
        {
            return hidden ? baseRadius * Mathf.Clamp01(scale) : baseRadius;
        }

        /// <summary>Salto da Onça: where the rider lands, <see cref="PounceStopShort"/> before the target (never behind the start).</summary>
        public static Vector3 PounceLanding(Vector3 from, Vector3 target)
        {
            Vector3 flat = target - from;
            flat.y = 0f;
            float distance = flat.magnitude;
            if (distance <= PounceStopShort)
            {
                return from;
            }

            Vector3 landing = from + flat / distance * (distance - PounceStopShort);
            landing.y = from.y;
            return landing;
        }

        /// <summary>Pulo Duplo: upward speed that reaches <paramref name="height"/> m against <paramref name="gravity"/> (m/s², positive).</summary>
        public static float JumpSpeedFor(float height, float gravity)
        {
            return Mathf.Sqrt(2f * Mathf.Max(0f, height) * Mathf.Max(0.01f, Mathf.Abs(gravity)));
        }
    }
}
