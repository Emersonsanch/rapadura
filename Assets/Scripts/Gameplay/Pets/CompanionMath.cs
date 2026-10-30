using System.Collections.Generic;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Pets
{
    /// <summary>What Caramelo is doing, by priority (highest first: hiding from a boss roar, barking at a telegraph, fetching loot, following).</summary>
    public enum CompanionMode
    {
        Follow = 0,
        Fetch = 1,
        Alert = 2,
        Hide = 3,
    }

    /// <summary>
    /// Pure, allocation-free rules behind <see cref="CarameloCompanion"/> (EditMode-tested): where the dog
    /// trots relative to the hero, how fast (eased arrival, catch-up), when he teleports back, which
    /// telegraphing enemy he barks at, which loot spot he fetches, which behaviour wins, when he sits and
    /// sniffs. The feel layers (springs, squash, idle timing) reuse <see cref="MotionFeelMath"/>, like the
    /// hero and the enemies. Planar math works on XZ; heights are left to the ground probe.
    /// </summary>
    public static class CompanionMath
    {
        /// <summary>Behind and a bit to the side of the hero (m), so he never blocks the camera or the hero's swing.</summary>
        public const float FollowBack = 1.3f;
        public const float FollowSide = 0.85f;
        /// <summary>Inside this distance of the slot he stops (no jitter around the target).</summary>
        public const float StopRadius = 0.35f;
        /// <summary>From here on he runs at full speed; closer he eases down.</summary>
        public const float SlowRadius = 2.2f;
        /// <summary>Farther than this from the hero (teleport, scene start, arena placement) he pops back next to them.</summary>
        public const float TeleportDistance = 14f;
        /// <summary>Only enemies telegraphing this close to the hero get barked at.</summary>
        public const float AlertRadius = 12f;
        /// <summary>He stops this far from the enemy he warns about (never touches it).</summary>
        public const float AlertStandOff = 2.3f;
        /// <summary>Loot spots farther than this from the hero are left alone.</summary>
        public const float FetchRadius = 12f;
        /// <summary>Standing still this long (s) → he sits.</summary>
        public const float SitDelay = 2.5f;

        /// <summary>
        /// The follow slot: <see cref="FollowBack"/> behind and <see cref="FollowSide"/> to the
        /// <paramref name="side"/> (+1 right, −1 left) of the hero, on the hero's height. A degenerate
        /// forward falls back to world +Z.
        /// </summary>
        public static Vector3 FollowSlot(Vector3 heroPosition, Vector3 heroForward, float side, float back = FollowBack, float lateral = FollowSide)
        {
            Vector3 forward = Flat(heroForward);
            if (forward.sqrMagnitude < 1e-6f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            return heroPosition - forward * back + right * (Mathf.Sign(side == 0f ? 1f : side) * lateral);
        }

        /// <summary>
        /// Target speed toward a point <paramref name="distance"/> m away: 0 inside <paramref name="stopRadius"/>,
        /// smoothstep up to <paramref name="maxSpeed"/> at <paramref name="slowRadius"/> (an ease-out arrival
        /// instead of a hard stop). Degenerate radii give full speed outside the stop radius.
        /// </summary>
        public static float ArrivalSpeed(float distance, float maxSpeed, float stopRadius = StopRadius, float slowRadius = SlowRadius)
        {
            if (distance <= stopRadius || maxSpeed <= 0f)
            {
                return 0f;
            }

            if (slowRadius <= stopRadius)
            {
                return maxSpeed;
            }

            float t = Mathf.Clamp01((distance - stopRadius) / (slowRadius - stopRadius));
            return maxSpeed * t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Run speed while following: <paramref name="baseSpeed"/>, boosted up to <paramref name="maxBoost"/>×
        /// as the hero gets farther than <see cref="SlowRadius"/> (so he catches up with a running hero),
        /// never below what the hero is moving at.
        /// </summary>
        public static float CatchUpSpeed(float distanceToHero, float heroSpeed, float baseSpeed, float maxBoost = 1.6f)
        {
            float boost = Mathf.Lerp(1f, Mathf.Max(1f, maxBoost), Mathf.Clamp01((distanceToHero - SlowRadius) / 6f));
            return Mathf.Max(baseSpeed * boost, heroSpeed * 1.1f);
        }

        /// <summary>True when the dog is so far from the hero that walking back would look broken.</summary>
        public static bool ShouldTeleport(Vector3 petPosition, Vector3 heroPosition, float maxDistance = TeleportDistance)
        {
            return PlanarDistance(petPosition, heroPosition) > maxDistance;
        }

        /// <summary>
        /// Which telegraphing enemy to bark at: the one closest to the hero within <paramref name="radius"/>
        /// (the most urgent threat); −1 when none qualifies.
        /// </summary>
        public static int SelectAlert(IReadOnlyList<Vector3> enemyPositions, Vector3 heroPosition, float radius = AlertRadius)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            if (enemyPositions == null)
            {
                return best;
            }

            for (int i = 0; i < enemyPositions.Count; i++)
            {
                float d = PlanarDistance(enemyPositions[i], heroPosition);
                if (d <= radius && d < bestDistance)
                {
                    best = i;
                    bestDistance = d;
                }
            }

            return best;
        }

        /// <summary>
        /// Where he stands to warn about an enemy: on the enemy → hero line, <paramref name="standOff"/> m from
        /// the enemy (but never past the hero). Same point as the enemy's when they overlap the hero.
        /// </summary>
        public static Vector3 AlertStandPoint(Vector3 enemyPosition, Vector3 heroPosition, float standOff = AlertStandOff)
        {
            Vector3 toHero = Flat(heroPosition - enemyPosition);
            float distance = toHero.magnitude;
            if (distance < 1e-4f)
            {
                return heroPosition;
            }

            float along = Mathf.Min(standOff, distance);
            Vector3 point = enemyPosition + toHero / distance * along;
            point.y = heroPosition.y;
            return point;
        }

        /// <summary>
        /// Which loot spot to fetch next: among the spots within <paramref name="radius"/> of the hero, the one
        /// closest to the dog (a short dash, not across the arena); −1 when none.
        /// </summary>
        public static int SelectFetch(IReadOnlyList<Vector3> spots, Vector3 heroPosition, Vector3 petPosition, float radius = FetchRadius)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            if (spots == null)
            {
                return best;
            }

            for (int i = 0; i < spots.Count; i++)
            {
                if (PlanarDistance(spots[i], heroPosition) > radius)
                {
                    continue;
                }

                float d = PlanarDistance(spots[i], petPosition);
                if (d < bestDistance)
                {
                    best = i;
                    bestDistance = d;
                }
            }

            return best;
        }

        /// <summary>Priority between the behaviours: hide (boss roar) &gt; alert (telegraph) &gt; fetch &gt; follow.</summary>
        public static CompanionMode ChooseMode(bool hiding, bool alerting, bool fetching)
        {
            if (hiding) return CompanionMode.Hide;
            if (alerting) return CompanionMode.Alert;
            return fetching ? CompanionMode.Fetch : CompanionMode.Follow;
        }

        /// <summary>He sits after standing still for <paramref name="sitDelay"/> s while following (never mid-alert, mid-fetch or hiding).</summary>
        public static bool ShouldSit(CompanionMode mode, float idleSeconds, float sitDelay = SitDelay)
        {
            return mode == CompanionMode.Follow && idleSeconds >= sitDelay;
        }

        /// <summary>
        /// Sniff weight (0..1) after <paramref name="idleSeconds"/> standing: every ~<paramref name="interval"/> s
        /// (jittered by <see cref="MotionFeelMath.Hash01"/>, no Random) the nose dips for ~1.2 s with an eased
        /// in/out. 0 during the first interval, deterministic for a given <paramref name="seed"/>.
        /// </summary>
        public static float SniffWeight(float idleSeconds, float interval, int seed)
        {
            if (interval <= 0f || idleSeconds < interval)
            {
                return 0f;
            }

            const float hold = 1.2f;
            int index = Mathf.FloorToInt(idleSeconds / interval);
            float start = index * interval + MotionFeelMath.Hash01(index, seed) * interval * 0.3f;
            float local = idleSeconds - start;
            if (local < 0f || local > hold)
            {
                return 0f;
            }

            return Mathf.Sin(local / hold * Mathf.PI);
        }

        /// <summary>Tail wag frequency (Hz): lazy when sitting, quicker when trotting, fastest when happy (cuddle, fetch delivered).</summary>
        public static float TailWagFrequency(float speed, bool happy, bool hiding)
        {
            if (hiding) return 0f;
            if (happy) return 6f;
            return Mathf.Lerp(1.4f, 3.2f, Mathf.Clamp01(speed / 5f));
        }

        public static float PlanarDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
