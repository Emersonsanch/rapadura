using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.TimedEvents;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The scheduled events tied to content (Docs/Network.md §14): every item id they burn or pay has an
    /// <see cref="ItemDefinition"/> with icon and PT/EN texts, the tickets have in-game sources only, every role × room of the
    /// enemy roster is a real beast fit for the role, and the room points add up by season on the "Eventos" board.
    /// </summary>
    public class TimedEventContentTests
    {
        private static DateTime Brt(int y, int m, int d, int h, int min = 0) => new DateTime(y, m, d, h, min, 0, DateTimeKind.Utc).AddHours(3);

        private static Dictionary<string, ItemDefinition> ItemsById()
        {
            var map = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
            foreach (ItemDefinition item in Resources.LoadAll<ItemDefinition>("Items"))
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemId) && !map.ContainsKey(item.ItemId))
                {
                    map.Add(item.ItemId, item);
                }
            }

            return map;
        }

        // ------------------------------------------------------------------ items

        [Test]
        public void EveryIdTheEventsUse_IsInTheItemList()
        {
            var ids = new HashSet<string>(TimedEventItems.AllIds);
            Assert.AreEqual(TimedEventItems.AllIds.Count, ids.Count, "no duplicate ids");

            foreach (TimedEventDefinition def in TimedEventCatalog.All)
            {
                if (def.NeedsTicket)
                {
                    Assert.IsTrue(ids.Contains(def.TicketItemId), def.TicketItemId);
                }

                string material = TimedEventRewards.MaterialOf(def.Kind);
                Assert.IsTrue(ids.Contains(material), material);
                if (TimedEventRewards.Cosmetics.TryGetValue(def.Kind, out string[] cosmetics))
                {
                    foreach (string c in cosmetics) Assert.IsTrue(ids.Contains(c), c);
                }
            }

            Assert.IsTrue(ids.Contains(TimedBemaRules.BemaCosmeticId));
            Assert.IsTrue(ids.Contains(TimedEventRewards.OhyahChest));
            Assert.IsTrue(ids.Contains(TimedEventRewards.OhyahMark));
        }

        [Test]
        public void EveryEventItem_HasAnAsset_WithIconAndTexts()
        {
            Dictionary<string, ItemDefinition> items = ItemsById();
            var keys = new HashSet<string>(ContentTexts.Entries.Select(e => e.Key));
            var missing = new List<string>();
            foreach (string id in TimedEventItems.AllIds)
            {
                if (!items.TryGetValue(id, out ItemDefinition item))
                {
                    missing.Add(id + " (no asset in Resources/Items)");
                    continue;
                }

                if (item.Icon == null) missing.Add(id + " (no icon)");
                if (!keys.Contains(ContentText.ItemNameKey(id))) missing.Add(id + " (no name in ContentTexts)");
                if (!keys.Contains(ContentText.ItemDescriptionKey(id))) missing.Add(id + " (no description in ContentTexts)");
            }

            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test]
        public void EventItems_HaveNoStats_AndPrizesAreBound()
        {
            Dictionary<string, ItemDefinition> items = ItemsById();
            foreach (string id in TimedEventItems.AllIds)
            {
                ItemDefinition item = items[id];
                Assert.IsFalse(item.IsEquippable, id + ": no equipment slot");
                Assert.IsEmpty(item.EquipStatModifiers ?? Array.Empty<StatModifierDefinition>(), id);
                Assert.IsEmpty(item.ConsumeBuffs ?? Array.Empty<StatModifierDefinition>(), id);
                Assert.AreEqual(0f, item.ArmorDefense + item.WeaponDamage + item.HealthRestore + item.ManaRestore + item.StaminaRestore, id);
                Assert.AreEqual(TimedEventItems.IsBound(id), item.IsBound, id + ": cosmetics and trophies bound, the rest tradable");
            }

            Assert.AreEqual(TimedEventItemKind.Ticket, TimedEventItems.KindOf(TimedEventCatalog.VigiliaTicketId));
            Assert.AreEqual(TimedEventItemKind.Material, TimedEventItems.KindOf(TimedEventRewards.CercoMaterial));
            Assert.AreEqual(TimedEventItemKind.Chest, TimedEventItems.KindOf(TimedEventRewards.OhyahChest));
            Assert.AreEqual(TimedEventItemKind.Trophy, TimedEventItems.KindOf(TimedEventRewards.OhyahMark));
            Assert.AreEqual(TimedEventItemKind.Cosmetic, TimedEventItems.KindOf(TimedBemaRules.BemaCosmeticId));
        }

        [Test]
        public void Tickets_AreEarnedInGame_NeverSoldForGold()
        {
            foreach (TimedEventDefinition def in TimedEventCatalog.All.Where(d => d.NeedsTicket))
            {
                Assert.IsTrue(TimedEventItems.TryGetTicketSource(def.TicketItemId, out TimedTicketSource s), def.TicketItemId);
                Assert.AreEqual(TimedEventRewards.MaterialOf(def.Kind), s.ExchangeMaterialId, "the exchange takes the event's own material");
                Assert.AreEqual(def.MinLevel, s.DropMinLevel, "drops from the level of the first room");
                Assert.IsNotNull(CityCatalog.Get(s.ExchangeCityId), s.ExchangeCityId);
                Assert.Greater(s.CommonDropChance + s.BossDropChance, 0d);
            }

            string vela = TimedEventCatalog.VigiliaTicketId;
            string marca = TimedEventCatalog.CacadaTicketId;
            Assert.AreEqual(0d, TimedEventItems.TicketDropChance(vela, 14, false));
            Assert.AreEqual(0.004d, TimedEventItems.TicketDropChance(vela, 15, false), 1e-9);
            Assert.AreEqual(0d, TimedEventItems.TicketDropChance(marca, 99, true));
            Assert.AreEqual(0.05d, TimedEventItems.TicketDropChance(marca, 100, true), 1e-9);
            Assert.AreEqual(0d, TimedEventItems.TicketDropChance(marca, 300, false), "commons never drop the Marca");
            Assert.AreEqual(2, TimedEventItems.TicketsFromExchange(vela, 11));
            Assert.AreEqual(0, TimedEventItems.TicketsFromExchange(vela, 4));
            Assert.AreEqual(0, TimedEventItems.TicketsFromExchange("item_gold_coin", 50));
        }

        // ------------------------------------------------------------------ enemy roster

        [Test]
        public void Roster_EveryRoleOfEveryRoom_IsARealBeastFitForTheRole()
        {
            var enemies = new Dictionary<string, EnemyDefinition>(StringComparer.Ordinal);
            foreach (EnemyDefinition e in Resources.LoadAll<EnemyDefinition>("Enemies"))
            {
                if (e != null && !enemies.ContainsKey(e.name)) enemies.Add(e.name, e);
            }

            var all = TimedEventEnemyRoster.All().ToList();
            Assert.AreEqual(TimedEventEnemyRoster.RoomCount * 6, all.Count);
            Assert.AreEqual(TimedEventCatalog.SevenRooms.Length, TimedEventEnemyRoster.RoomCount);

            var problems = new List<string>();
            foreach ((int room, TimedEnemyRole role, string id) in all)
            {
                if (!enemies.TryGetValue(id ?? string.Empty, out EnemyDefinition e))
                {
                    problems.Add($"room {room} {role}: {id} not in Resources/Enemies");
                    continue;
                }

                if (e.IsBoss) problems.Add($"room {room} {role}: {id} is a boss");
                if (role == TimedEnemyRole.Caster && !e.IsRanged) problems.Add($"room {room} Caster {id} is not ranged");
                if ((role == TimedEnemyRole.Brute || role == TimedEnemyRole.Archon) && e.AuthoredMotionArchetype != EnemyMotionArchetype.Heavy)
                {
                    problems.Add($"room {room} {role} {id} is not heavy");
                }
            }

            Assert.That(problems, Is.Empty, string.Join("\n", problems));

            for (int room = 1; room <= TimedEventEnemyRoster.RoomCount; room++)
            {
                var row = TimedEventEnemyRoster.Roles.Select(r => TimedEventEnemyRoster.EnemyFor(r, room)).ToList();
                Assert.AreEqual(row.Count, row.Distinct().Count(), $"room {room}: one beast per role");
                float runner = enemies[TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Runner, room)].ChaseSpeed;
                float brute = enemies[TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Brute, room)].ChaseSpeed;
                Assert.Greater(runner, brute, $"room {room}: the Runner outruns the Brute");
            }
        }

        [Test]
        public void Roster_LevelPicksTheRoom_AndAWaveResolvesToIds()
        {
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Shade, 1), TimedEventEnemyRoster.EnemyForLevel(TimedEnemyRole.Shade, 1));
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Shade, 1), TimedEventEnemyRoster.EnemyForLevel(TimedEnemyRole.Shade, 79));
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Shade, 2), TimedEventEnemyRoster.EnemyForLevel(TimedEnemyRole.Shade, 80));
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Archon, 7), TimedEventEnemyRoster.EnemyForLevel(TimedEnemyRole.Archon, 500));
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Archon, 7), TimedEventEnemyRoster.EnemyForLevel(TimedEnemyRole.Archon, 900));
            Assert.AreEqual(TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Elite, 7), TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Elite, 99), "room clamped");

            TimedEventRoomBand band = TimedEventCatalog.SevenRooms[3];
            TimedWave last = TimedVigiliaRules.Wave(band, TimedVigiliaRules.WaveCount - 1, 6, 4242);
            var resolved = TimedEventEnemyRoster.Resolve(last, band.Index);
            Assert.AreEqual(last.Groups.Count, resolved.Count);
            Assert.AreEqual(last.TotalEnemies, resolved.Sum(r => r.Count));
            Assert.IsTrue(resolved.Any(r => r.EnemyId == TimedEventEnemyRoster.EnemyFor(TimedEnemyRole.Archon, band.Index)), "the last wave brings the room's Archon");
            Assert.IsEmpty(TimedEventEnemyRoster.Resolve(null, 1));
        }

        // ------------------------------------------------------------------ season points

        [Test]
        public void Season_IsAQuarterOfTheBrasiliaCalendar()
        {
            Assert.AreEqual(2026 * 4 + 0, TimedEventSeasonPoints.SeasonOf(Brt(2026, 3, 31, 23, 50)));
            Assert.AreEqual(2026 * 4 + 1, TimedEventSeasonPoints.SeasonOf(Brt(2026, 4, 1, 0, 10)));
            Assert.AreEqual(2026 * 4 + 1, TimedEventSeasonPoints.SeasonOf(Brt(2026, 6, 30, 22)), "the Bema is inside Apr–Jun");
            Assert.AreEqual(2026 * 4 + 3, TimedEventSeasonPoints.SeasonOf(Brt(2026, 12, 31, 23, 59)));
            Assert.AreEqual(2027 * 4 + 0, TimedEventSeasonPoints.SeasonOf(Brt(2027, 1, 1, 0)));
            // 01:00 UTC on 1 April is still 31 March in Brasília.
            Assert.AreEqual(2026 * 4 + 0, TimedEventSeasonPoints.SeasonOf(new DateTime(2026, 4, 1, 1, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void Season_AccumulatesRooms_CountsAReplayOnce_AndSkipsOtherSeasons()
        {
            long day = TimedEventAgenda.LocalDay(Brt(2026, 11, 23, 13));
            long otherDay = TimedEventAgenda.LocalDay(Brt(2026, 9, 30, 13));
            int season = TimedEventSeasonPoints.SeasonOfLocalDay(day);
            var records = new[]
            {
                new TimedEventPointsRecord("ana", TimedEventKind.VigiliaDaColuna, day * 32 + 3, 2, 300, 1000d),
                new TimedEventPointsRecord("ana", TimedEventKind.VigiliaDaColuna, day * 32 + 3, 2, 300, 1500d), // replay
                new TimedEventPointsRecord("ana", TimedEventKind.CercoDaTreva, day * 32 + 1, 2, 200, 2000d),
                new TimedEventPointsRecord("bia", TimedEventKind.CacadaAOhyah, day * 32 + 0, 1, 450, 1200d),
                new TimedEventPointsRecord("bia", TimedEventKind.CacadaAOhyah, day * 32 + 0, 1, 50, 1300d), // worse duplicate
                new TimedEventPointsRecord("bia", TimedEventKind.VigiliaDaColuna, otherDay * 32 + 3, 2, 900, 10d), // last season
                new TimedEventPointsRecord("cai", TimedEventKind.VigiliaDaColuna, day * 32 + 3, 2, 0, 1000d),
                new TimedEventPointsRecord("", TimedEventKind.VigiliaDaColuna, day * 32 + 3, 2, 99, 1000d),
            };

            Dictionary<string, TimedSeasonTotal> totals = TimedEventSeasonPoints.Accumulate(records, season);
            Assert.AreEqual(2, totals.Count, "zero points and empty ids leave no total");
            Assert.AreEqual(500, totals["ana"].Points);
            Assert.AreEqual(2, totals["ana"].Rooms);
            Assert.AreEqual(2000d, totals["ana"].ReachedAt);
            Assert.AreEqual(450, totals["bia"].Points);
            Assert.AreEqual(1200d, totals["bia"].ReachedAt);
            Assert.AreEqual(900, TimedEventSeasonPoints.Accumulate(records, TimedEventSeasonPoints.SeasonOfLocalDay(otherDay))["bia"].Points);
        }

        [Test]
        public void Season_RecordsFromARoom_SkipIdleAndZero()
        {
            TimedEventOccurrence occurrence = TimedEventAgenda.Next(TimedEventCatalog.Vigilia, Brt(2026, 11, 23, 12, 50));
            var board = TimedEventScoring.Rank(new[]
            {
                new TimedScoreEntry("ana", 320, 0, 1),
                new TimedScoreEntry("bia", 0, 2, 2),
                new TimedScoreEntry("cai", 500, 0, 3, active: false),
            });

            List<TimedEventPointsRecord> records = TimedEventSeasonPoints.RecordsFrom(occurrence, 3, board, 42d);
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("ana", records[0].PlayerId);
            Assert.AreEqual(320, records[0].Points);
            Assert.AreEqual(occurrence.Key, records[0].OccurrenceKey);
            Assert.AreEqual(occurrence.Day, records[0].Day);
            Assert.AreEqual(TimedEventKind.VigiliaDaColuna, records[0].Kind);
        }

        [Test]
        public void Season_FeedsTheEventsBoard_KeepingTheOtherFields()
        {
            long day = TimedEventAgenda.LocalDay(Brt(2026, 11, 23, 13));
            int season = TimedEventSeasonPoints.SeasonOfLocalDay(day);
            var entries = new[]
            {
                new RankingEntry("ana", "Ana", 120, 5000, 1700, 3, 5, true, eventPoints: 9999, resets: 2, grandResets: 1),
                new RankingEntry("bia", "Bia", 300, 9000),
                new RankingEntry("cai", "Cai", 80, 100),
            };
            var records = new[]
            {
                new TimedEventPointsRecord("ana", TimedEventKind.VigiliaDaColuna, day * 32 + 3, 2, 400, 2000d),
                new TimedEventPointsRecord("bia", TimedEventKind.CercoDaTreva, day * 32 + 1, 4, 400, 1000d),
                new TimedEventPointsRecord("dan", TimedEventKind.CacadaAOhyah, day * 32 + 0, 1, 100, 500d),
            };

            List<RankedEntry> board = TimedEventSeasonPoints.Board(entries, records, season);
            Assert.AreEqual(new[] { "bia", "ana", "dan", "cai" }, board.Select(r => r.Entry.PlayerId).ToArray(),
                "equal points: who got there first; old points never carried over");
            Assert.AreEqual(new[] { 1, 1, 3, 4 }, board.Select(r => r.Rank).ToArray());
            Assert.AreEqual(0, board[3].Entry.EventPoints);

            RankingEntry ana = board[1].Entry;
            Assert.AreEqual(400, ana.EventPoints);
            Assert.AreEqual(2000d, ana.ReachedAt);
            Assert.AreEqual("Ana", ana.Name);
            Assert.AreEqual(120, ana.Level);
            Assert.AreEqual(5000, ana.Experience);
            Assert.AreEqual(1700, ana.PvpRating);
            Assert.AreEqual(3, ana.PvpWins);
            Assert.AreEqual(5, ana.PvpMatches);
            Assert.IsTrue(ana.PvpPlaced);
            Assert.AreEqual(2, ana.Resets);
            Assert.AreEqual(1, ana.GrandResets);
            Assert.AreEqual("dan", board[2].Entry.Name, "a player with points but no entry is listed by id");

            Assert.AreEqual(0, RankingRules.WithEventPoints(entries[1], -5, 0d).EventPoints, "never negative");
        }
    }
}
