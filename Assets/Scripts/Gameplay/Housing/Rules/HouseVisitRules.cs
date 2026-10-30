using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>One signature in the guest book (Housing.md §12.2 <c>guestBook[]</c>).</summary>
    [Serializable]
    public class HouseGuestEntry
    {
        /// <summary>A cousin (<c>CharacterId</c> name) or an NPC id of <see cref="HouseVisitRules"/>.</summary>
        public string visitorId;
        /// <summary>Which of the visitor's lines (0-based).</summary>
        public int line;
        /// <summary>A placed item the visitor commented on (may be empty).</summary>
        public string itemId;
        /// <summary>In-game day of the visit.</summary>
        public int day;
    }

    /// <summary>
    /// Offline visits (Housing.md §4.6): at most one per in-game day, when the cousin comes home and the guest book is
    /// placed. The visitor is one of the four cousins who are not playing, Vó Zefa, or a regional NPC already met
    /// (Dona Cida from the start; Seu Bento, Mestre Tonho and Benedita as the bosses before their towns fall). They sit,
    /// comment on a piece and sign. Deterministic by day (no RNG state in the save). Unity-free.
    /// </summary>
    public static class HouseVisitRules
    {
        public const int MaxEntries = 100;
        public const int LinesPerVisitor = 3;
        /// <summary>"Casa Cheia": distinct cousins who must have signed (offline: the four NPC cousins).</summary>
        public const int CousinsForCasaCheia = 4;

        public const string VoZefa = "vo_zefa";
        public const string DonaCida = "dona_cida";
        public const string SeuBento = "seu_bento";
        public const string MestreTonho = "mestre_tonho";
        public const string Benedita = "benedita";

        /// <summary>Same names as <c>CharacterId</c> (kept as strings so this stays Unity/Characters-free).</summary>
        public static readonly string[] Cousins = { "Maithe", "Maria", "Joaquim", "Icaro", "Lavine" };
        public static readonly string[] Npcs = { VoZefa, DonaCida, SeuBento, MestreTonho, Benedita };

        public static bool IsCousin(string visitorId) => visitorId != null && Cousins.Contains(visitorId);

        public static bool CanVisit(int lastVisitDay, int today, bool guestBookPlaced) => guestBookPlaced && today > lastVisitDay;

        /// <summary>Who may visit: the cousins not playing, Vó Zefa, Dona Cida, and one NPC per boss beaten (in campaign order).</summary>
        public static List<string> Candidates(string activeCousin, int bossesDefeated)
        {
            var list = Cousins.Where(c => !string.Equals(c, activeCousin, StringComparison.Ordinal)).ToList();
            list.Add(VoZefa);
            list.Add(DonaCida);
            if (bossesDefeated >= 1) list.Add(SeuBento);
            if (bossesDefeated >= 2) list.Add(MestreTonho);
            if (bossesDefeated >= 3) list.Add(Benedita);
            return list;
        }

        /// <summary>
        /// Today's visitor. Cousins who have not signed yet get priority every other day (so "Casa Cheia" is reachable
        /// in about a week of play, not by luck); otherwise a day-seeded pick among <paramref name="candidates"/>.
        /// </summary>
        public static string PickVisitor(int day, IReadOnlyList<string> candidates, ICollection<string> signedCousins)
        {
            if (candidates == null || candidates.Count == 0) return null;
            uint hash = Mix((uint)day * 2654435761u);
            if (signedCousins != null && hash % 2 == 0)
            {
                List<string> unsigned = candidates.Where(c => IsCousin(c) && !signedCousins.Contains(c)).ToList();
                if (unsigned.Count > 0) return unsigned[(int)((hash >> 8) % (uint)unsigned.Count)];
            }

            return candidates[(int)((hash >> 4) % (uint)candidates.Count)];
        }

        public static int LineFor(int day, string visitorId)
        {
            uint hash = Mix((uint)day * 40503u ^ StableHash(visitorId));
            return (int)(hash % LinesPerVisitor);
        }

        /// <summary>The piece the visitor comments on (day-seeded) among the comfortable ones, or null.</summary>
        public static string PickItem(int day, IReadOnlyList<string> placedItemIds)
        {
            if (placedItemIds == null || placedItemIds.Count == 0) return null;
            return placedItemIds[(int)(Mix((uint)day * 97u + 13u) % (uint)placedItemIds.Count)];
        }

        /// <summary>Appends, keeping the newest <see cref="MaxEntries"/>.</summary>
        public static void Append(List<HouseGuestEntry> book, HouseGuestEntry entry)
        {
            if (book == null || entry == null || string.IsNullOrEmpty(entry.visitorId)) return;
            book.Add(entry);
            if (book.Count > MaxEntries) book.RemoveRange(0, book.Count - MaxEntries);
        }

        public static HashSet<string> SignedCousins(IEnumerable<HouseGuestEntry> book)
        {
            return new HashSet<string>((book ?? Enumerable.Empty<HouseGuestEntry>()).Where(e => e != null && IsCousin(e.visitorId)).Select(e => e.visitorId), StringComparer.Ordinal);
        }

        public static uint StableHash(string text)
        {
            uint hash = 2166136261u;
            if (text == null) return hash;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return hash;
        }

        private static uint Mix(uint x)
        {
            x ^= x >> 16;
            x *= 0x7feb352du;
            x ^= x >> 15;
            x *= 0x846ca68bu;
            x ^= x >> 16;
            return x;
        }
    }
}
