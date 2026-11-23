using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Party;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>One attacker's line in an <see cref="EnemyDamageLedger"/>.</summary>
    public sealed class DamageEntry
    {
        public string PlayerId { get; internal set; }

        /// <summary>The party the attacker was in at their latest hit (null = alone): the group that may own the kill.</summary>
        public string PartyId { get; internal set; }

        /// <summary>Damage counted so far (only what the monster had left to lose), after the away decay.</summary>
        public double Damage { get; internal set; }

        public float FirstHitAt { get; internal set; }
        public float LastHitAt { get; internal set; }

        /// <summary>Since when the attacker is away (not present near the monster); null while present.</summary>
        public float? AwaySince { get; internal set; }

        internal float DecayedUntil;
    }

    /// <summary>
    /// Who hurt one monster of the shared world, and how much (the input of <see cref="KillOwnershipRules"/>). Pure, one per
    /// living monster on the server:
    /// <list type="bullet">
    /// <item>Only real damage counts: a hit counts at most the health the monster still had (no overkill credit).</item>
    /// <item>At most <see cref="MaxEntries"/> attackers; a newcomer above the smallest line takes its place, otherwise it is not listed.</item>
    /// <item>An attacker who stops being present (dead, gone, or farther than <see cref="AwayRadius"/>) keeps their damage for
    /// <see cref="AwayGraceSeconds"/>, then loses half of it every <see cref="AwayHalfLifeSeconds"/>, and is forgotten after
    /// <see cref="ForgetAfterAwaySeconds"/> away (or under 1 damage). Coming back stops the decay; what was lost stays lost.</item>
    /// <item>The monster's last hit is remembered (<see cref="LastHitPlayerId"/>, <see cref="LastHitDamage"/>): the kill's
    /// last blow earns no ownership (anti kill-steal).</item>
    /// <item>A monster that leashes home and heals whole forgets everything (<see cref="Clear"/>).</item>
    /// </list>
    /// </summary>
    public sealed class EnemyDamageLedger
    {
        public const int MaxEntries = 20;

        /// <summary>Same radius as the party's XP and loot share (<see cref="PartyXpRules.ShareRadius"/>, 40 m).</summary>
        public const float AwayRadius = PartyXpRules.ShareRadius;

        public const float AwayGraceSeconds = 10f;
        public const float AwayHalfLifeSeconds = 10f;
        public const float ForgetAfterAwaySeconds = 60f;

        private readonly List<DamageEntry> _entries = new List<DamageEntry>();

        public IReadOnlyList<DamageEntry> Entries => _entries;

        public string LastHitPlayerId { get; private set; }

        /// <summary>What the last hit counted (after the overkill clamp).</summary>
        public double LastHitDamage { get; private set; }

        public double Total
        {
            get
            {
                double total = 0d;
                foreach (DamageEntry entry in _entries)
                {
                    total += entry.Damage;
                }

                return total;
            }
        }

        public DamageEntry Find(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            foreach (DamageEntry entry in _entries)
            {
                if (entry.PlayerId == playerId)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// A hit of <paramref name="damage"/> by <paramref name="playerId"/> on a monster that had <paramref name="healthBefore"/>
        /// left. Returns what counted (0 for a bad hit). It also becomes the last hit.
        /// </summary>
        public double Record(string playerId, string partyId, double damage, double healthBefore, float now)
        {
            if (string.IsNullOrEmpty(playerId) || !(damage > 0d) || !(healthBefore > 0d))
            {
                return 0d;
            }

            double counted = Math.Min(damage, healthBefore);
            LastHitPlayerId = playerId;
            LastHitDamage = counted;

            DamageEntry entry = Find(playerId);
            if (entry == null)
            {
                if (_entries.Count >= MaxEntries)
                {
                    DamageEntry smallest = Smallest();
                    if (smallest == null || smallest.Damage >= counted)
                    {
                        return counted;
                    }

                    _entries.Remove(smallest);
                }

                entry = new DamageEntry { PlayerId = playerId, FirstHitAt = now };
                _entries.Add(entry);
            }

            entry.PartyId = string.IsNullOrEmpty(partyId) ? null : partyId;
            entry.Damage += counted;
            entry.LastHitAt = now;
            entry.AwaySince = null;
            entry.DecayedUntil = now;
            return counted;
        }

        /// <summary>
        /// The server's periodic pass (any rate): <paramref name="isPresent"/> says whether a player is alive, connected and
        /// within <see cref="AwayRadius"/> of the monster.
        /// </summary>
        public void Tick(float now, Func<string, bool> isPresent)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                DamageEntry entry = _entries[i];
                bool present = isPresent != null && isPresent(entry.PlayerId);
                if (present)
                {
                    entry.AwaySince = null;
                    entry.DecayedUntil = now;
                    continue;
                }

                if (!entry.AwaySince.HasValue)
                {
                    entry.AwaySince = now;
                    entry.DecayedUntil = now;
                }

                float from = Math.Max(entry.DecayedUntil, entry.AwaySince.Value + AwayGraceSeconds);
                if (now > from)
                {
                    entry.Damage *= Math.Pow(0.5d, (now - from) / AwayHalfLifeSeconds);
                    entry.DecayedUntil = now;
                }

                if (now - entry.AwaySince.Value >= ForgetAfterAwaySeconds || entry.Damage < 1d)
                {
                    _entries.RemoveAt(i);
                }
            }
        }

        /// <summary>The monster went home and healed whole: nobody has a claim any more.</summary>
        public void Clear()
        {
            _entries.Clear();
            LastHitPlayerId = null;
            LastHitDamage = 0d;
        }

        private DamageEntry Smallest()
        {
            DamageEntry smallest = null;
            foreach (DamageEntry entry in _entries)
            {
                if (smallest == null || entry.Damage < smallest.Damage
                    || (entry.Damage == smallest.Damage && entry.LastHitAt < smallest.LastHitAt))
                {
                    smallest = entry;
                }
            }

            return smallest;
        }
    }
}
