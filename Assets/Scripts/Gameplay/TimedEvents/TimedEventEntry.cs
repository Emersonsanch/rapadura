using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Party;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What the server knows of one character at the gate (it reads its own database, never the client).</summary>
    public readonly struct TimedEventEntrant
    {
        public readonly string PlayerId;
        public readonly int Level;

        /// <summary>Tickets of this event in the bag (any count; the rule only burns <see cref="TimedEventDefinition.TicketCount"/>).</summary>
        public readonly int Tickets;

        public readonly long Gold;

        /// <summary>At the event's gate now (party members elsewhere stay behind).</summary>
        public readonly bool Present;

        /// <summary>
        /// Level the room band reads: the "nível efetivo" of a reset hero (<c>RebirthRules.EffectiveLevel</c>, Network.md §15),
        /// never below <see cref="Level"/>, so old gear goes to the room of the level it was earned at. Equal to it when not given.
        /// </summary>
        public readonly int EffectiveLevel;

        public TimedEventEntrant(string playerId, int level, int tickets, long gold, bool present = true, int effectiveLevel = 0)
        {
            PlayerId = playerId;
            Level = level;
            Tickets = tickets;
            Gold = gold;
            Present = present;
            EffectiveLevel = Math.Max(level, effectiveLevel);
        }
    }

    /// <summary>What one entry costs one character: tickets or gold (the server takes it in the same transaction).</summary>
    public readonly struct TimedEventPayment
    {
        public readonly string PlayerId;
        public readonly int Tickets;
        public readonly long Gold;

        public TimedEventPayment(string playerId, int tickets, long gold)
        {
            PlayerId = playerId;
            Tickets = tickets;
            Gold = gold;
        }
    }

    /// <summary>Outcome of an entry: no exceptions, a refusal reason; on success the room/instance and the payments.</summary>
    public sealed class TimedEventEntryResult
    {
        public TimedEventRefusal Refusal { get; internal set; }

        /// <summary>For party entries: the member who blocked it, and why.</summary>
        public string FailedPlayerId { get; internal set; }

        public TimedEventRefusal MemberRefusal { get; internal set; }

        public int RoomIndex { get; internal set; }
        public int Instance { get; internal set; }

        /// <summary>Already in a room of this occurrence: back in, nothing paid, no entry spent.</summary>
        public bool Rejoined { get; internal set; }

        public List<string> Entered { get; } = new List<string>();
        public List<TimedEventPayment> Payments { get; } = new List<TimedEventPayment>();

        public bool Ok => Refusal == TimedEventRefusal.None;

        internal static TimedEventEntryResult Refused(TimedEventRefusal reason, string player = null, TimedEventRefusal member = TimedEventRefusal.None)
        {
            return new TimedEventEntryResult { Refusal = reason, FailedPlayerId = player, MemberRefusal = member };
        }
    }

    /// <summary>
    /// The entries of one character (the server keeps one per character, saved with it): how many times it entered each
    /// event on a server-local day. Only the latest day per event is kept.
    /// </summary>
    public sealed class TimedEventEntryLedger
    {
        private readonly Dictionary<TimedEventKind, (long Day, int Count)> _entries = new Dictionary<TimedEventKind, (long, int)>();

        public int EntriesOn(TimedEventKind kind, long day) => _entries.TryGetValue(kind, out var e) && e.Day == day ? e.Count : 0;

        public void Record(TimedEventKind kind, long day)
        {
            int count = EntriesOn(kind, day);
            _entries[kind] = (day, count + 1);
        }
    }

    /// <summary>One room instance of an occurrence: its band, instance number and the players in it (join order).</summary>
    public sealed class TimedEventRoom
    {
        public readonly int RoomIndex;
        public readonly int Instance;
        public readonly int Capacity;
        private readonly List<string> _players = new List<string>();

        public TimedEventRoom(int roomIndex, int instance, int capacity)
        {
            RoomIndex = roomIndex;
            Instance = instance;
            Capacity = capacity;
        }

        public IReadOnlyList<string> Players => _players;
        public int FreeSlots => Capacity - _players.Count;
        public bool Contains(string playerId) => _players.Contains(playerId);
        internal void Add(string playerId) => _players.Add(playerId);
    }

    /// <summary>All room instances of one occurrence of one event (the server keeps one while it is open or running).</summary>
    public sealed class TimedEventRoomSet
    {
        public readonly TimedEventDefinition Definition;
        public readonly TimedEventOccurrence Occurrence;
        private readonly List<TimedEventRoom> _rooms = new List<TimedEventRoom>();

        public TimedEventRoomSet(TimedEventDefinition definition, TimedEventOccurrence occurrence)
        {
            Definition = definition;
            Occurrence = occurrence;
        }

        public IReadOnlyList<TimedEventRoom> Rooms => _rooms;

        public TimedEventRoom RoomOf(string playerId)
        {
            foreach (TimedEventRoom room in _rooms)
            {
                if (room.Contains(playerId))
                {
                    return room;
                }
            }

            return null;
        }

        /// <summary>First instance of the band with room for <paramref name="count"/>, opening a new one if allowed (null = full).</summary>
        internal TimedEventRoom FindOrOpen(int roomIndex, int count)
        {
            int instances = 0;
            foreach (TimedEventRoom room in _rooms)
            {
                if (room.RoomIndex != roomIndex)
                {
                    continue;
                }

                instances++;
                if (room.FreeSlots >= count)
                {
                    return room;
                }
            }

            if (instances >= TimedEventRules.MaxInstancesPerBand || count > Definition.MaxPlayersPerRoom)
            {
                return null;
            }

            var opened = new TimedEventRoom(roomIndex, instances + 1, Definition.MaxPlayersPerRoom);
            _rooms.Add(opened);
            return opened;
        }
    }

    /// <summary>
    /// Entering a scheduled event (Docs/Network.md §14), pure: the gate window from <see cref="TimedEventAgenda"/>, the room
    /// by level band (Blood Castle style), the ticket or the gold fee, the room size (full instances open a new one, up to
    /// <see cref="MaxInstancesPerBand"/>), the entries per character per server-local day and the party entry (the leader
    /// takes every present member in, all into the same instance, or nobody enters). Nothing throws.
    /// </summary>
    public static class TimedEventRules
    {
        /// <summary>Parallel instances of one band per occurrence (20 × 10 = 200 players in one Vigília room).</summary>
        public const int MaxInstancesPerBand = 20;

        /// <summary>One character alone at the gate.</summary>
        public static TimedEventEntryResult TryEnter(TimedEventRoomSet set, TimedEventEntrant entrant, TimedEventEntryLedger ledger, DateTime utc)
        {
            if (set?.Definition == null)
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.UnknownEvent);
            }

            if (string.IsNullOrEmpty(entrant.PlayerId))
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.InvalidPlayer);
            }

            TimedEventEntryResult rejoin = TryRejoinInternal(set, entrant.PlayerId, utc);
            if (rejoin != null)
            {
                return rejoin;
            }

            TimedEventRefusal window = CheckWindow(set, utc);
            if (window != TimedEventRefusal.None)
            {
                return TimedEventEntryResult.Refused(window);
            }

            TimedEventRefusal personal = CheckPersonal(set, entrant, ledger, out int roomIndex, out TimedEventPayment payment);
            if (personal != TimedEventRefusal.None)
            {
                return TimedEventEntryResult.Refused(personal, entrant.PlayerId);
            }

            TimedEventRoom room = set.FindOrOpen(roomIndex, 1);
            if (room == null)
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.RoomFull);
            }

            room.Add(entrant.PlayerId);
            ledger?.Record(set.Definition.Kind, set.Occurrence.Day);
            var result = new TimedEventEntryResult { RoomIndex = room.RoomIndex, Instance = room.Instance };
            result.Entered.Add(entrant.PlayerId);
            result.Payments.Add(payment);
            return result;
        }

        /// <summary>
        /// The leader takes the party in. <paramref name="entrants"/> = the server's view of the members (absent ones stay
        /// behind). Every present member must be able to enter, in the same band; then all pay and enter one instance.
        /// </summary>
        public static TimedEventEntryResult TryEnterParty(TimedEventRoomSet set, PartyState party, string actorId,
            IReadOnlyList<TimedEventEntrant> entrants, Func<string, TimedEventEntryLedger> ledgers, DateTime utc)
        {
            if (set?.Definition == null)
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.UnknownEvent);
            }

            if (party == null || string.IsNullOrEmpty(actorId) || !party.IsMember(actorId))
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.InvalidPlayer);
            }

            if (!party.IsLeader(actorId))
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.NotPartyLeader);
            }

            TimedEventRefusal window = CheckWindow(set, utc);
            if (window != TimedEventRefusal.None)
            {
                return TimedEventEntryResult.Refused(window);
            }

            var going = new List<TimedEventEntrant>();
            foreach (PartyMember member in party.Members)
            {
                if (TryFind(entrants, member.PlayerId, out TimedEventEntrant entrant) && entrant.Present && set.RoomOf(member.PlayerId) == null)
                {
                    going.Add(entrant);
                }
            }

            if (!going.Exists(e => e.PlayerId == actorId))
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.NotPresent, actorId);
            }

            int roomIndex = 0;
            var payments = new List<TimedEventPayment>();
            foreach (TimedEventEntrant entrant in going)
            {
                TimedEventRefusal personal = CheckPersonal(set, entrant, ledgers?.Invoke(entrant.PlayerId), out int memberRoom, out TimedEventPayment payment);
                if (personal != TimedEventRefusal.None)
                {
                    return TimedEventEntryResult.Refused(TimedEventRefusal.PartyMemberNotEligible, entrant.PlayerId, personal);
                }

                if (roomIndex == 0)
                {
                    roomIndex = memberRoom;
                }
                else if (memberRoom != roomIndex)
                {
                    return TimedEventEntryResult.Refused(TimedEventRefusal.PartyRoomMismatch, entrant.PlayerId);
                }

                payments.Add(payment);
            }

            TimedEventRoom room = set.FindOrOpen(roomIndex, going.Count);
            if (room == null)
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.RoomFull);
            }

            var result = new TimedEventEntryResult { RoomIndex = room.RoomIndex, Instance = room.Instance };
            foreach (TimedEventEntrant entrant in going)
            {
                room.Add(entrant.PlayerId);
                ledgers?.Invoke(entrant.PlayerId)?.Record(set.Definition.Kind, set.Occurrence.Day);
                result.Entered.Add(entrant.PlayerId);
            }

            result.Payments.AddRange(payments);
            return result;
        }

        /// <summary>Back into one's room after a disconnect (gate open or fight running): free, no entry spent.</summary>
        public static TimedEventEntryResult TryRejoin(TimedEventRoomSet set, string playerId, DateTime utc)
        {
            if (set?.Definition == null)
            {
                return TimedEventEntryResult.Refused(TimedEventRefusal.UnknownEvent);
            }

            return TryRejoinInternal(set, playerId, utc) ?? TimedEventEntryResult.Refused(
                set.RoomOf(playerId) == null ? TimedEventRefusal.NotInRoom : TimedEventRefusal.EntryClosed);
        }

        /// <summary>Room band of a level, or <see cref="TimedEventRefusal.LevelTooLow"/>.</summary>
        public static TimedEventRefusal RoomFor(TimedEventDefinition definition, int level, out int roomIndex)
        {
            roomIndex = definition?.RoomFor(level) ?? 0;
            return roomIndex > 0 ? TimedEventRefusal.None : TimedEventRefusal.LevelTooLow;
        }

        /// <summary>Ticket first; without it, the gold fee of the room (if the event has one).</summary>
        public static TimedEventRefusal Cost(TimedEventDefinition definition, int roomIndex, TimedEventEntrant entrant, out TimedEventPayment payment)
        {
            payment = new TimedEventPayment(entrant.PlayerId, 0, 0);
            if (definition.NeedsTicket && entrant.Tickets >= definition.TicketCount)
            {
                payment = new TimedEventPayment(entrant.PlayerId, definition.TicketCount, 0);
                return TimedEventRefusal.None;
            }

            int fee = definition.GoldFee(roomIndex);
            if (fee < 0)
            {
                return TimedEventRefusal.NoTicket;
            }

            if (entrant.Gold < fee)
            {
                return TimedEventRefusal.NotEnoughGold;
            }

            payment = new TimedEventPayment(entrant.PlayerId, 0, fee);
            return TimedEventRefusal.None;
        }

        private static TimedEventRefusal CheckWindow(TimedEventRoomSet set, DateTime utc)
        {
            if (!set.Occurrence.IsValid)
            {
                return TimedBemaRules.IsBema(utc) && set.Definition.SuspendedDuringBema ? TimedEventRefusal.SuspendedForBema : TimedEventRefusal.EntryClosed;
            }

            return utc >= set.Occurrence.EntryOpensUtc && utc < set.Occurrence.StartUtc ? TimedEventRefusal.None : TimedEventRefusal.EntryClosed;
        }

        /// <summary>Level band → entries today → ticket / gold (in that order).</summary>
        private static TimedEventRefusal CheckPersonal(TimedEventRoomSet set, TimedEventEntrant entrant, TimedEventEntryLedger ledger,
            out int roomIndex, out TimedEventPayment payment)
        {
            payment = default;
            TimedEventRefusal band = RoomFor(set.Definition, entrant.EffectiveLevel, out roomIndex);
            if (band != TimedEventRefusal.None)
            {
                return band;
            }

            bool bema = TimedBemaRules.IsBemaDay(set.Occurrence.Day);
            if (ledger != null && ledger.EntriesOn(set.Definition.Kind, set.Occurrence.Day) >= set.Definition.EntriesPerDayOn(bema))
            {
                return TimedEventRefusal.AlreadyEnteredToday;
            }

            return Cost(set.Definition, roomIndex, entrant, out payment);
        }

        private static TimedEventEntryResult TryRejoinInternal(TimedEventRoomSet set, string playerId, DateTime utc)
        {
            TimedEventRoom room = set.RoomOf(playerId);
            if (room == null || !set.Occurrence.IsValid || utc < set.Occurrence.EntryOpensUtc || utc >= set.Occurrence.EndUtc)
            {
                return null;
            }

            var result = new TimedEventEntryResult { RoomIndex = room.RoomIndex, Instance = room.Instance, Rejoined = true };
            result.Entered.Add(playerId);
            return result;
        }

        private static bool TryFind(IReadOnlyList<TimedEventEntrant> entrants, string playerId, out TimedEventEntrant found)
        {
            if (entrants != null)
            {
                foreach (TimedEventEntrant entrant in entrants)
                {
                    if (entrant.PlayerId == playerId)
                    {
                        found = entrant;
                        return true;
                    }
                }
            }

            found = default;
            return false;
        }
    }
}
