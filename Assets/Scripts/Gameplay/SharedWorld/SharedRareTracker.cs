using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Night;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>
    /// The rare monsters of the shared world, on top of what offline already has (not a second system):
    /// <list type="bullet">
    /// <item><b>When</b>: the night species only roll in their period, from the clearing's own day/dusk/night tables
    /// (<c>PeriodSpawnTable.RollEntry</c>, <c>MapSpawnZone.EntriesFor</c>); their weight in the table is their chance.</item>
    /// <item><b>How many per clearing</b>: never more than <see cref="NightSpecies.MaxPerWave"/> (the offline <c>AllowedNow</c>).</item>
    /// <item><b>New online</b>: a <see cref="SpawnTier.Rare"/> species (Mardagayl, the Twig-Bird) stands <b>one at a time
    /// per region</b>, and once killed it does not roll again in that region for <see cref="SharedSpawnRules.RareSeconds"/>
    /// (15 min ± 30%, server seed). Its clearing slot refills on the common time with whatever the table rolls.</item>
    /// <item>World and map bosses keep <c>RegionBossReturns</c> (5 min, or "the next night"): it is pure already; the server
    /// holds one per boss for everyone.</item>
    /// </list>
    /// Pure; the server owns one tracker for the whole world, keyed by region id (<c>OverworldPart.Id</c>).
    /// </summary>
    public sealed class SharedRareTracker
    {
        private sealed class Entry
        {
            public int Alive;
            public float LockedUntil;
            public int Kills;
        }

        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

        private static string Key(string regionId, string speciesId) => (regionId ?? string.Empty) + "|" + (speciesId ?? string.Empty);

        private Entry Get(string regionId, string speciesId, bool create)
        {
            string key = Key(regionId, speciesId);
            if (!_entries.TryGetValue(key, out Entry entry) && create)
            {
                entry = new Entry();
                _entries[key] = entry;
            }

            return entry;
        }

        public int AliveIn(string regionId, string speciesId) => Get(regionId, speciesId, false)?.Alive ?? 0;

        /// <summary>When a killed rare may roll again in the region (0 = it may).</summary>
        public float LockedUntil(string regionId, string speciesId) => Get(regionId, speciesId, false)?.LockedUntil ?? 0f;

        /// <summary>May a rare species roll in this region now: none of it alive there and its wait over?</summary>
        public bool CanRoll(string regionId, string speciesId, float now)
        {
            Entry entry = Get(regionId, speciesId, false);
            return entry == null || (entry.Alive <= 0 && entry.LockedUntil <= now);
        }

        public void OnSpawned(string regionId, string speciesId)
        {
            Get(regionId, speciesId, true).Alive++;
        }

        /// <summary>Killed: held back region-wide for the rare wait. Returns when it may roll again.</summary>
        public float OnKilled(string regionId, string speciesId, float now, int serverSeed)
        {
            Entry entry = Get(regionId, speciesId, true);
            entry.Alive = Math.Max(0, entry.Alive - 1);
            float seconds = SharedSpawnRules.RespawnSeconds(SpawnTier.Rare, 0f, serverSeed, Key(regionId, speciesId), entry.Kills, 1);
            entry.Kills++;
            entry.LockedUntil = now + seconds;
            return entry.LockedUntil;
        }

        /// <summary>Gone without dying (its clearing slept): no wait, it may roll again when the clearing wakes.</summary>
        public void OnRemoved(string regionId, string speciesId)
        {
            Entry entry = Get(regionId, speciesId, false);
            if (entry != null)
            {
                entry.Alive = Math.Max(0, entry.Alive - 1);
            }
        }

        /// <summary>
        /// The filter for <c>PeriodSpawnTable.RollEntry</c> in a clearing of <paramref name="regionId"/>: normal and variant
        /// entries always; a species only while the clearing holds fewer than its <see cref="NightSpecies.MaxPerWave"/>
        /// (<paramref name="aliveInClearing"/>: species id → alive in this clearing; null = none) and, for a rare, only when
        /// <see cref="CanRoll"/>.
        /// </summary>
        public Predicate<PeriodSpawnEntry> Allowed(string regionId, float now, Func<string, int> aliveInClearing)
        {
            return entry =>
            {
                if (entry == null || entry.Form != NightForm.Species)
                {
                    return entry != null;
                }

                NightSpecies species = NightVariantRules.FindSpecies(entry.SpeciesId);
                if (species == null)
                {
                    return false;
                }

                int here = aliveInClearing != null ? aliveInClearing(species.Id) : 0;
                if (here >= species.MaxPerWave)
                {
                    return false;
                }

                return SharedSpawnRules.TierOf(NightForm.Species, species) != SpawnTier.Rare || CanRoll(regionId, species.Id, now);
            };
        }
    }
}
