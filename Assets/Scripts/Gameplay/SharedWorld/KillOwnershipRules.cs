using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>Who owns a kill of the shared world (and so its loot during the protection).</summary>
    public readonly struct KillOwner
    {
        /// <summary>The attacker with the most credited damage in the winning group (the solo owner, or the party's top hitter).</summary>
        public readonly string PlayerId;

        /// <summary>The winning party, or null when the owner is alone.</summary>
        public readonly string PartyId;

        /// <summary>The winning group's credited damage (the last blow taken out).</summary>
        public readonly double Damage;

        public KillOwner(string playerId, string partyId, double damage)
        {
            PlayerId = playerId;
            PartyId = partyId;
            Damage = damage;
        }

        public bool IsNone => string.IsNullOrEmpty(PlayerId);
        public bool IsParty => !string.IsNullOrEmpty(PartyId);

        public static KillOwner None => new KillOwner(null, null, 0d);
    }

    /// <summary>
    /// MU-style kill ownership: the group (a party counts as one, a player alone as their own) with the most damage in the
    /// monster's <see cref="EnemyDamageLedger"/> owns the kill. Anti kill-steal: the kill's last blow is not credited, so a
    /// player who only landed the last hit owns nothing (unless nobody else did any damage: a one-shot is the hitter's).
    /// Ties go to the group that hit first, then to the smaller key (deterministic). Pure.
    /// </summary>
    public static class KillOwnershipRules
    {
        /// <summary>The damage the ledger credits <paramref name="entry"/> for ownership: its damage less the last blow when it is the last hitter's.</summary>
        public static double CreditedDamage(EnemyDamageLedger ledger, DamageEntry entry)
        {
            if (ledger == null || entry == null)
            {
                return 0d;
            }

            double damage = entry.Damage;
            if (entry.PlayerId == ledger.LastHitPlayerId)
            {
                damage -= ledger.LastHitDamage;
            }

            return Math.Max(0d, damage);
        }

        /// <summary>The group key of an entry: its party, else the player alone.</summary>
        public static string GroupKey(string playerId, string partyId) => !string.IsNullOrEmpty(partyId) ? "party:" + partyId : "solo:" + playerId;

        /// <summary>
        /// The owner of the kill. <paramref name="killerId"/> (the last hitter; the ledger's own <see cref="EnemyDamageLedger.LastHitPlayerId"/>
        /// when null) owns it only when no one has credited damage. <see cref="KillOwner.None"/> when nobody can (empty ledger, no killer).
        /// </summary>
        public static KillOwner Resolve(EnemyDamageLedger ledger, string killerId = null)
        {
            if (ledger == null)
            {
                return KillOwner.None;
            }

            var totals = new Dictionary<string, double>();
            var firstHit = new Dictionary<string, float>();
            var party = new Dictionary<string, string>();
            foreach (DamageEntry entry in ledger.Entries)
            {
                string key = GroupKey(entry.PlayerId, entry.PartyId);
                totals.TryGetValue(key, out double sum);
                totals[key] = sum + CreditedDamage(ledger, entry);
                firstHit[key] = firstHit.TryGetValue(key, out float at) ? Math.Min(at, entry.FirstHitAt) : entry.FirstHitAt;
                party[key] = entry.PartyId;
            }

            string best = null;
            foreach (KeyValuePair<string, double> pair in totals)
            {
                if (pair.Value <= 0d)
                {
                    continue;
                }

                if (best == null || pair.Value > totals[best]
                    || (pair.Value == totals[best] && (firstHit[pair.Key] < firstHit[best]
                        || (firstHit[pair.Key] == firstHit[best] && string.CompareOrdinal(pair.Key, best) < 0))))
                {
                    best = pair.Key;
                }
            }

            if (best == null)
            {
                string killer = string.IsNullOrEmpty(killerId) ? ledger.LastHitPlayerId : killerId;
                if (string.IsNullOrEmpty(killer))
                {
                    return KillOwner.None;
                }

                DamageEntry own = ledger.Find(killer);
                return new KillOwner(killer, own?.PartyId, 0d);
            }

            // The top hitter of the winning group (its face on the kill).
            string top = null;
            double topDamage = -1d;
            foreach (DamageEntry entry in ledger.Entries)
            {
                if (GroupKey(entry.PlayerId, entry.PartyId) != best)
                {
                    continue;
                }

                double credited = CreditedDamage(ledger, entry);
                if (credited > topDamage)
                {
                    top = entry.PlayerId;
                    topDamage = credited;
                }
            }

            return new KillOwner(top, party[best], totals[best]);
        }

        /// <summary>Does <paramref name="playerId"/> (in <paramref name="partyId"/>, null = alone) belong to the owning group?</summary>
        public static bool IsOwnerSide(KillOwner owner, string playerId, string partyId)
        {
            if (owner.IsNone || string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            return owner.IsParty ? partyId == owner.PartyId : playerId == owner.PlayerId;
        }
    }
}
