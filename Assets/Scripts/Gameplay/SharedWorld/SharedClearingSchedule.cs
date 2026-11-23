using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>What <see cref="SharedClearingSchedule.UpdatePresence"/> did to a clearing.</summary>
    public enum ClearingTransition
    {
        None = 0,

        /// <summary>A player came within <see cref="SharedSpawnRules.WakeRadius"/>: the clearing runs (and fills its free slots).</summary>
        Woke = 1,

        /// <summary>Nobody within <see cref="SharedSpawnRules.SleepRadius"/> for <see cref="SharedSpawnRules.SleepGraceSeconds"/>: the server despawns its survivors.</summary>
        Slept = 2,
    }

    /// <summary>
    /// The respawn agenda of one clearing in the shared world (one per <c>MapSpawnZone</c> of <c>OverworldLayout.World</c>),
    /// held by the server. A clearing has <see cref="SharedSpawnRules.TargetAlive"/> slots; a slot is taken by a monster
    /// alive or by a death still waiting out its time. A free slot fills one monster at a time
    /// (<see cref="SharedSpawnRules.FillIntervalSeconds"/>).
    /// <para>Unlike offline, sleeping does not reset anything: the timers of the fallen keep running while the clearing
    /// sleeps, so walking away and back never refills a clearing that was just cleared (offline the hero's clearing refills
    /// whole). Survivors of a sleeping clearing are despawned and come back as soon as it wakes.</para>
    /// Pure: the server passes its clock (seconds) and its seed, and keeps the actual monsters itself.
    /// </summary>
    public sealed class SharedClearingSchedule
    {
        private readonly List<float> _waitingUntil = new List<float>();

        public SharedClearingSchedule(string clearingId, int baseMaxAlive, float clearingRespawnSeconds)
        {
            ClearingId = clearingId ?? string.Empty;
            BaseMaxAlive = Math.Max(0, baseMaxAlive);
            ClearingRespawnSeconds = clearingRespawnSeconds;
        }

        public string ClearingId { get; }

        /// <summary>The clearing's <c>MaxAlive</c> (one player in the region).</summary>
        public int BaseMaxAlive { get; }

        /// <summary>The clearing's <c>RespawnSeconds</c> (the common time).</summary>
        public float ClearingRespawnSeconds { get; }

        public int Alive { get; private set; }

        /// <summary>Monsters of this clearing fallen so far (feeds the seeded variation).</summary>
        public int Deaths { get; private set; }

        public bool Running { get; private set; }

        /// <summary>Earliest time of the next fill (spacing between spawns, or the retry after no point was found).</summary>
        public float NextFillAt { get; private set; }

        /// <summary>Last time a player was within <see cref="SharedSpawnRules.SleepRadius"/> while it ran.</summary>
        public float LastNearAt { get; private set; }

        /// <summary>Deaths still waiting at <paramref name="now"/>.</summary>
        public int Waiting(float now)
        {
            int count = 0;
            foreach (float until in _waitingUntil)
            {
                if (until > now)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>When the soonest waiting death frees its slot (null when none waits).</summary>
        public float? NextReturnAt(float now)
        {
            float? best = null;
            foreach (float until in _waitingUntil)
            {
                if (until > now && (!best.HasValue || until < best.Value))
                {
                    best = until;
                }
            }

            return best;
        }

        /// <summary>Slots free at <paramref name="now"/> for a region of <paramref name="playersInRegion"/> players.</summary>
        public int FreeSlots(float now, int playersInRegion)
        {
            return Math.Max(0, SharedSpawnRules.TargetAlive(BaseMaxAlive, playersInRegion) - Alive - Waiting(now));
        }

        /// <summary>Should the server spawn one monster in this clearing now? Running, a free slot, and the fill spacing passed.</summary>
        public bool ShouldSpawn(float now, int playersInRegion)
        {
            Prune(now);
            return Running && now >= NextFillAt && FreeSlots(now, playersInRegion) > 0;
        }

        /// <summary>A monster appeared (the server placed it).</summary>
        public void MarkSpawned(float now)
        {
            Alive++;
            NextFillAt = now + SharedSpawnRules.FillIntervalSeconds;
        }

        /// <summary>No allowed point (<see cref="SharedSpawnRules.TryPickSpawnPoint"/> false) or nothing to roll: try again later.</summary>
        public void MarkSpawnFailed(float now)
        {
            NextFillAt = now + SharedSpawnRules.RetrySeconds;
        }

        /// <summary>
        /// A monster of this clearing fell: its slot waits <see cref="SharedSpawnRules.RespawnSeconds"/> (a rare frees its
        /// slot on the common time; the species itself is held back region-wide by <see cref="SharedRareTracker"/>).
        /// Returns when the slot is free again.
        /// </summary>
        public float OnDeath(float now, SpawnTier tier, int serverSeed, int playersInRegion)
        {
            if (Alive > 0)
            {
                Alive--;
            }

            SpawnTier slotTier = SharedSpawnRules.SlotTier(tier);
            float seconds = SharedSpawnRules.RespawnSeconds(slotTier, ClearingRespawnSeconds, serverSeed, ClearingId, Deaths, playersInRegion);
            Deaths++;
            float until = now + seconds;
            _waitingUntil.Add(until);
            return until;
        }

        /// <summary>A monster left the clearing without dying (GM removal, leashed out for good): its slot is free at once.</summary>
        public void OnRemoved()
        {
            if (Alive > 0)
            {
                Alive--;
            }
        }

        /// <summary>
        /// Wake or sleep by the nearest player (<paramref name="nearestEdgeDistance"/>: from the clearing's edge, m;
        /// ∞ = nobody in the region). Sleeping despawns the survivors (<see cref="Alive"/> goes to 0) but keeps the deaths waiting.
        /// </summary>
        public ClearingTransition UpdatePresence(float now, float nearestEdgeDistance)
        {
            if (!Running)
            {
                if (nearestEdgeDistance <= SharedSpawnRules.WakeRadius)
                {
                    Running = true;
                    LastNearAt = now;
                    NextFillAt = Math.Min(NextFillAt, now);
                    return ClearingTransition.Woke;
                }

                return ClearingTransition.None;
            }

            if (nearestEdgeDistance <= SharedSpawnRules.SleepRadius)
            {
                LastNearAt = now;
                return ClearingTransition.None;
            }

            if (now - LastNearAt >= SharedSpawnRules.SleepGraceSeconds)
            {
                Running = false;
                Alive = 0;
                return ClearingTransition.Slept;
            }

            return ClearingTransition.None;
        }

        private void Prune(float now)
        {
            _waitingUntil.RemoveAll(until => until <= now);
        }
    }
}
