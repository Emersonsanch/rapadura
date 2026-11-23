using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>The scheduled online events of FASE 12.3 (Docs/Network.md §14). Serialized by the server: never renumber.</summary>
    public enum TimedEventKind
    {
        None = 0,

        /// <summary>"Vigília da Coluna": hold the lamps while the Coluna da Glória lifts the light; survive 8 waves of the Treva.</summary>
        VigiliaDaColuna = 1,

        /// <summary>"Cerco da Treva": defend the city's Lâmpada do Portão from 6 assaults through 3 gates.</summary>
        CercoDaTreva = 2,

        /// <summary>"Caçada a Ohyah": many players against Ohyah, the old giant; loot by contribution (damage, healing, tanking).</summary>
        CacadaAOhyah = 3,
    }

    /// <summary>Where one event stands on the server clock.</summary>
    public enum TimedEventPhase
    {
        /// <summary>Nothing announced yet: the next occurrence is more than <see cref="TimedEventDefinition.WarningMinutes"/> away.</summary>
        Closed = 0,

        /// <summary>Announced ("começa em X min"): the gate is still shut.</summary>
        Announced = 1,

        /// <summary>The gate is open: players enter (alone or with their party) until the start.</summary>
        EntryOpen = 2,

        /// <summary>The fight is on. Nobody new enters; whoever is already in the room may reconnect.</summary>
        Running = 3,
    }

    /// <summary>Why an entry (or a rejoin) was refused. Texts: <see cref="TimedEventTexts.RefusalKey"/>. Never renumber.</summary>
    public enum TimedEventRefusal
    {
        None = 0,
        InvalidPlayer = 1,
        UnknownEvent = 2,

        /// <summary>Outside the entry window (before it opens or after the start).</summary>
        EntryClosed = 3,

        /// <summary>Below the first room's minimum level.</summary>
        LevelTooLow = 4,

        /// <summary>This character already used today's entries of this event (server-local day).</summary>
        AlreadyEnteredToday = 5,

        /// <summary>The event needs a ticket and has no gold alternative.</summary>
        NoTicket = 6,

        /// <summary>No ticket and not enough gold for the fee.</summary>
        NotEnoughGold = 7,

        /// <summary>Every instance of the band is full (<see cref="TimedEventRules.MaxInstancesPerBand"/>).</summary>
        RoomFull = 8,

        /// <summary>Only the party leader takes the party in.</summary>
        NotPartyLeader = 9,

        /// <summary>One present member cannot enter (see the result's <c>FailedPlayerId</c> and <c>MemberRefusal</c>).</summary>
        PartyMemberNotEligible = 10,

        /// <summary>The present members fall into different rooms (level bands): the party cannot go in together.</summary>
        PartyRoomMismatch = 11,

        /// <summary>The Cerco da Treva does not come during the Bema (the truce of the Bema).</summary>
        SuspendedForBema = 12,

        /// <summary>Rejoin asked by someone who is not in any room of this occurrence.</summary>
        NotInRoom = 13,

        /// <summary>The actor is not at the gate (or the leader is not among the present members).</summary>
        NotPresent = 14,
    }

    /// <summary>One room (level band) of an event, like the Blood Castle rooms: 1-based, inclusive levels.</summary>
    public readonly struct TimedEventRoomBand
    {
        public readonly int Index;
        public readonly int MinLevel;
        public readonly int MaxLevel;

        public TimedEventRoomBand(int index, int minLevel, int maxLevel)
        {
            Index = index;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        public bool Contains(int level) => level >= MinLevel && level <= MaxLevel;

        /// <summary>Middle of the band: the enemy level of the room before the per-wave step.</summary>
        public int MidLevel => (MinLevel + MaxLevel) / 2;
    }

    /// <summary>
    /// One scheduled event: when it happens (server-local minutes of the day, Brasília like <c>WorldEventCalendar</c>),
    /// how long the warning, entry window and fight last, its rooms, ticket/fee, room size and entries per day.
    /// </summary>
    public sealed class TimedEventDefinition
    {
        public readonly TimedEventKind Kind;
        public readonly string Id;
        public readonly string NameEn;
        public readonly string NamePt;

        /// <summary>Starts (fight begins) in server-local minutes after midnight, ascending.</summary>
        public readonly int[] StartMinutes;

        /// <summary>Extra starts only on Bema days (empty for most events).</summary>
        public readonly int[] BemaExtraStartMinutes;

        /// <summary>The announcement goes out this many minutes before the start (≥ <see cref="EntryMinutes"/>).</summary>
        public readonly int WarningMinutes;

        /// <summary>The gate opens this many minutes before the start and closes at the start.</summary>
        public readonly int EntryMinutes;

        public readonly int DurationMinutes;
        public readonly int MaxPlayersPerRoom;
        public readonly TimedEventRoomBand[] Rooms;

        /// <summary>Ticket item (null = no ticket) and how many one entry burns.</summary>
        public readonly string TicketItemId;
        public readonly int TicketCount;

        /// <summary>Gold paid instead of the ticket, per room (index 0 = room 1). 0 = free when no ticket; empty = ticket only.</summary>
        public readonly int[] GoldFeeByRoom;

        public readonly int EntriesPerDay;
        public readonly int BemaEntriesPerDay;

        /// <summary>The event does not happen on Bema days (the truce of the Bema).</summary>
        public readonly bool SuspendedDuringBema;

        public TimedEventDefinition(TimedEventKind kind, string id, string nameEn, string namePt, int[] startMinutes,
            int[] bemaExtraStartMinutes, int warningMinutes, int entryMinutes, int durationMinutes, int maxPlayersPerRoom,
            TimedEventRoomBand[] rooms, string ticketItemId, int ticketCount, int[] goldFeeByRoom, int entriesPerDay,
            int bemaEntriesPerDay, bool suspendedDuringBema)
        {
            Kind = kind;
            Id = id;
            NameEn = nameEn;
            NamePt = namePt;
            StartMinutes = startMinutes ?? Array.Empty<int>();
            BemaExtraStartMinutes = bemaExtraStartMinutes ?? Array.Empty<int>();
            WarningMinutes = warningMinutes;
            EntryMinutes = entryMinutes;
            DurationMinutes = durationMinutes;
            MaxPlayersPerRoom = maxPlayersPerRoom;
            Rooms = rooms ?? Array.Empty<TimedEventRoomBand>();
            TicketItemId = ticketItemId;
            TicketCount = ticketCount;
            GoldFeeByRoom = goldFeeByRoom ?? Array.Empty<int>();
            EntriesPerDay = entriesPerDay;
            BemaEntriesPerDay = bemaEntriesPerDay;
            SuspendedDuringBema = suspendedDuringBema;
        }

        public bool NeedsTicket => !string.IsNullOrEmpty(TicketItemId) && TicketCount > 0;

        /// <summary>Gold alternative of room <paramref name="roomIndex"/> (1-based); -1 = none (ticket only).</summary>
        public int GoldFee(int roomIndex)
        {
            if (GoldFeeByRoom.Length == 0)
            {
                return NeedsTicket ? -1 : 0;
            }

            int i = Math.Max(0, Math.Min(GoldFeeByRoom.Length - 1, roomIndex - 1));
            return GoldFeeByRoom[i];
        }

        public int EntriesPerDayOn(bool bema) => bema ? BemaEntriesPerDay : EntriesPerDay;

        public TimedEventRoomBand Room(int roomIndex)
        {
            foreach (TimedEventRoomBand band in Rooms)
            {
                if (band.Index == roomIndex)
                {
                    return band;
                }
            }

            return default;
        }

        /// <summary>Room of a level (0 = none: below the first room).</summary>
        public int RoomFor(int level)
        {
            foreach (TimedEventRoomBand band in Rooms)
            {
                if (band.Contains(level))
                {
                    return band.Index;
                }
            }

            return 0;
        }

        public int MinLevel => Rooms.Length > 0 ? Rooms[0].MinLevel : 1;
    }

    /// <summary>
    /// The three scheduled events (decided 2026-11-23, Manichaean re-theme of the old "Noite do Lobisomem",
    /// "Cerco ao Engenho" and "Caçada ao Habitante Ancestral"). Times are Brasília (server zone), never overlapping
    /// one another from the gate opening to the end (EditMode-tested), and the Bema is the seasonal one
    /// (<see cref="TimedBemaRules"/>).
    /// </summary>
    public static class TimedEventCatalog
    {
        public const string VigiliaId = "vigilia_da_coluna";
        public const string CercoId = "cerco_da_treva";
        public const string CacadaId = "cacada_a_ohyah";

        /// <summary>"Vela da Vigília": the Vigília ticket (drops in Regions 5+, sold by no one for real money).</summary>
        public const string VigiliaTicketId = "item_ticket_vela_vigilia";

        /// <summary>"Marca do Caçador": the Caçada ticket (crafted from 5 Fragmentos de Ohyah or bought with gold at the gate).</summary>
        public const string CacadaTicketId = "item_ticket_marca_cacador";

        /// <summary>The 7 Blood Castle-like rooms of the Vigília and the Cerco (levels 15–500).</summary>
        public static readonly TimedEventRoomBand[] SevenRooms =
        {
            new TimedEventRoomBand(1, 15, 79),
            new TimedEventRoomBand(2, 80, 149),
            new TimedEventRoomBand(3, 150, 219),
            new TimedEventRoomBand(4, 220, 289),
            new TimedEventRoomBand(5, 290, 359),
            new TimedEventRoomBand(6, 360, 429),
            new TimedEventRoomBand(7, 430, 500),
        };

        /// <summary>The 3 strengths of Ohyah (levels 100–500).</summary>
        public static readonly TimedEventRoomBand[] OhyahRooms =
        {
            new TimedEventRoomBand(1, 100, 249),
            new TimedEventRoomBand(2, 250, 399),
            new TimedEventRoomBand(3, 400, 500),
        };

        private static int Hm(int hour, int minute) => hour * 60 + minute;

        public static readonly TimedEventDefinition Vigilia = new TimedEventDefinition(
            TimedEventKind.VigiliaDaColuna, VigiliaId, "Vigil of the Column", "Vigília da Coluna",
            new[] { Hm(1, 0), Hm(5, 0), Hm(9, 0), Hm(13, 0), Hm(17, 0), Hm(21, 0) },
            new[] { Hm(23, 0) },
            warningMinutes: 15, entryMinutes: 5, durationMinutes: 15, maxPlayersPerRoom: 10, SevenRooms,
            VigiliaTicketId, 1, new[] { 2000, 6000, 15000, 30000, 50000, 80000, 120000 },
            entriesPerDay: 1, bemaEntriesPerDay: 2, suspendedDuringBema: false);

        public static readonly TimedEventDefinition Cerco = new TimedEventDefinition(
            TimedEventKind.CercoDaTreva, CercoId, "Siege of the Darkness", "Cerco da Treva",
            new[] { Hm(8, 30), Hm(14, 30), Hm(20, 30) },
            null,
            warningMinutes: 15, entryMinutes: 5, durationMinutes: 20, maxPlayersPerRoom: 20, SevenRooms,
            null, 0, null,
            entriesPerDay: 1, bemaEntriesPerDay: 1, suspendedDuringBema: true);

        public static readonly TimedEventDefinition Cacada = new TimedEventDefinition(
            TimedEventKind.CacadaAOhyah, CacadaId, "Hunt for Ohyah", "Caçada a Ohyah",
            new[] { Hm(11, 0), Hm(22, 0) },
            null,
            warningMinutes: 30, entryMinutes: 10, durationMinutes: 30, maxPlayersPerRoom: 40, OhyahRooms,
            CacadaTicketId, 1, new[] { 25000, 60000, 120000 },
            entriesPerDay: 1, bemaEntriesPerDay: 1, suspendedDuringBema: false);

        public static readonly IReadOnlyList<TimedEventDefinition> All = new[] { Vigilia, Cerco, Cacada };

        public static TimedEventDefinition Find(TimedEventKind kind)
        {
            foreach (TimedEventDefinition definition in All)
            {
                if (definition.Kind == kind)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
