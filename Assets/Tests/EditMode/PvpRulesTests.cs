using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure PvP of TODO.md 12.3: consented duel (challenge / accept / decline / cancel / expire, countdown, area, life at
    /// 1, forfeit, time, disconnect), "pode ferir?" isolation, PvP areas by region and level, the PK stains, the ranked
    /// arena (Elo, placement, queue band, season, cosmetic rewards), the boards and the texts.
    /// </summary>
    public class PvpRulesTests
    {
        private static DuelCandidate C(string id, float x = 0f, ZoneKind zone = ZoneKind.Wilds, bool alive = true, bool busy = false) =>
            new DuelCandidate(id, new Vector3(x, 0f, 0f), zone, alive, busy);

        private static DuelFighterStatus F(float x, int hp = 100, int max = 100, bool connected = true) =>
            new DuelFighterStatus(new Vector3(x, 0f, 0f), hp, max, connected);

        private static DuelState Fighting(out double fightAt)
        {
            DuelResult r = DuelRules.Challenge("d1", C("a"), C("b", 4f), false, false, 0d);
            Assert.IsTrue(r.Ok);
            Assert.IsTrue(DuelRules.Accept(r.Duel, C("b", 4f), C("a"), false, 1d).Ok);
            fightAt = r.Duel.FightStartsAt;
            DuelRules.Tick(r.Duel, F(0f), F(4f), fightAt);
            Assert.AreEqual(DuelPhase.Fighting, r.Duel.Phase);
            return r.Duel;
        }

        private static PvpCombatant P(string id, MapRegion region = MapRegion.CostaDasMoncoes, int level = 100,
            ZoneKind zone = ZoneKind.Wilds, DuelState duel = null, string match = null, int team = 0) =>
            new PvpCombatant(id, zone, region, level, duel, match, team);

        // ------------------------------------------------------------------ duel: challenge

        [Test]
        public void Challenge_RefusesCityRangePartyBusyBlockedDeadSelf()
        {
            Assert.AreEqual(PvpRefusal.InSafeZone, DuelRules.Challenge("d", C("a", 0f, ZoneKind.City), C("b", 2f), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.InSafeZone, DuelRules.Challenge("d", C("a"), C("b", 2f, ZoneKind.City), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.TooFar, DuelRules.Challenge("d", C("a"), C("b", 10.5f), false, false, 0d).Refusal);
            Assert.IsTrue(DuelRules.Challenge("d", C("a"), C("b", 10f), false, false, 0d).Ok, "10 m is inclusive");
            Assert.AreEqual(PvpRefusal.SameParty, DuelRules.Challenge("d", C("a"), C("b", 2f), true, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.Busy, DuelRules.Challenge("d", C("a", busy: true), C("b", 2f), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.TargetBusy, DuelRules.Challenge("d", C("a"), C("b", 2f, busy: true), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.TargetBlocksDuels, DuelRules.Challenge("d", C("a"), C("b", 2f), false, true, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.Dead, DuelRules.Challenge("d", C("a"), C("b", 2f, alive: false), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.CannotTargetSelf, DuelRules.Challenge("d", C("a"), C("a"), false, false, 0d).Refusal);
            Assert.AreEqual(PvpRefusal.InvalidPlayer, DuelRules.Challenge("d", C(""), C("b"), false, false, 0d).Refusal);
        }

        [Test]
        public void Challenge_DeclineCancelAndExpire()
        {
            DuelState d = DuelRules.Challenge("d", C("a"), C("b", 3f), false, false, 0d).Duel;
            Assert.AreEqual(DuelPhase.Pending, d.Phase);
            Assert.AreEqual(PvpRefusal.NotParticipant, DuelRules.Decline(d, "a", 1d).Refusal, "only the target declines");
            Assert.IsTrue(DuelRules.Decline(d, "b", 1d).Ok);
            Assert.AreEqual(DuelEndReason.Declined, d.EndReason);
            Assert.IsNull(d.WinnerId);
            Assert.AreEqual(PvpRefusal.AlreadyEnded, DuelRules.Accept(d, C("b", 3f), C("a"), false, 2d).Refusal);

            DuelState c = DuelRules.Challenge("d2", C("a"), C("b", 3f), false, false, 0d).Duel;
            Assert.AreEqual(PvpRefusal.NotParticipant, DuelRules.Cancel(c, "b", 1d).Refusal);
            Assert.IsTrue(DuelRules.Cancel(c, "a", 1d).Ok);
            Assert.AreEqual(DuelEndReason.Cancelled, c.EndReason);

            DuelState e = DuelRules.Challenge("d3", C("a"), C("b", 3f), false, false, 0d).Duel;
            DuelRules.Tick(e, F(0f), F(3f), DuelRules.ChallengeSeconds - 0.1d);
            Assert.AreEqual(DuelPhase.Pending, e.Phase);
            DuelRules.Tick(e, F(0f), F(3f), DuelRules.ChallengeSeconds);
            Assert.AreEqual(DuelEndReason.Expired, e.EndReason);

            DuelState late = DuelRules.Challenge("d4", C("a"), C("b", 3f), false, false, 0d).Duel;
            Assert.AreEqual(PvpRefusal.ChallengeExpired, DuelRules.Accept(late, C("b", 3f), C("a"), false, 31d).Refusal);
            Assert.AreEqual(DuelPhase.Ended, late.Phase);
        }

        [Test]
        public void Accept_RechecksAndSealsAreaWithCountdown()
        {
            DuelState d = DuelRules.Challenge("d", C("a"), C("b", 4f), false, false, 0d).Duel;
            Assert.AreEqual(PvpRefusal.TooFar, DuelRules.Accept(d, C("b", 20f), C("a"), false, 1d).Refusal);
            Assert.AreEqual(PvpRefusal.InSafeZone, DuelRules.Accept(d, C("b", 4f, ZoneKind.City), C("a"), false, 1d).Refusal);
            Assert.AreEqual(PvpRefusal.SameParty, DuelRules.Accept(d, C("b", 4f), C("a"), true, 1d).Refusal);
            Assert.AreEqual(PvpRefusal.NotParticipant, DuelRules.Accept(d, C("x", 4f), C("a"), false, 1d).Refusal);
            Assert.AreEqual(DuelPhase.Pending, d.Phase);

            Assert.IsTrue(DuelRules.Accept(d, C("b", 4f, busy: true), C("a", busy: true), false, 1d).Ok, "busy with this very challenge");
            Assert.AreEqual(DuelPhase.Countdown, d.Phase);
            Assert.AreEqual(new Vector3(2f, 0f, 0f), d.Center);
            Assert.AreEqual(1d + DuelRules.CountdownSeconds, d.FightStartsAt);
            Assert.AreEqual(d.FightStartsAt + DuelRules.FightSeconds, d.FightEndsAt);
            Assert.AreEqual(PvpRefusal.WrongPhase, DuelRules.Accept(d, C("b", 4f), C("a"), false, 2d).Refusal);

            Assert.AreEqual(PvpRefusal.WrongPhase, DuelRules.ApplyHit(d, "a", "b", 100, 10, 3d).Refusal, "no hits in the countdown");
            DuelRules.Tick(d, F(0f), F(4f), d.FightStartsAt);
            Assert.AreEqual(DuelPhase.Fighting, d.Phase);
        }

        // ------------------------------------------------------------------ duel: end

        [Test]
        public void Hit_NeverKills_LifeOneLoses()
        {
            DuelState d = Fighting(out double t);
            DuelHit hit = DuelRules.ApplyHit(d, "a", "b", 100, 30, t + 1d);
            Assert.IsTrue(hit.Ok);
            Assert.AreEqual(70, hit.NewHp);
            Assert.IsFalse(hit.EndedDuel);
            Assert.AreEqual(PvpRefusal.NotParticipant, DuelRules.ApplyHit(d, "x", "b", 70, 10, t + 1d).Refusal);
            Assert.AreEqual(PvpRefusal.NotParticipant, DuelRules.ApplyHit(d, "a", "a", 70, 10, t + 1d).Refusal);

            DuelHit last = DuelRules.ApplyHit(d, "a", "b", 70, 999, t + 2d);
            Assert.AreEqual(69, last.Applied);
            Assert.AreEqual(1, last.NewHp);
            Assert.IsTrue(last.EndedDuel);
            Assert.AreEqual(DuelEndReason.Defeat, d.EndReason);
            Assert.AreEqual("a", d.WinnerId);
            Assert.IsFalse(DuelRules.AppliesDeathPenalty);
            Assert.AreEqual(PvpRefusal.AlreadyEnded, DuelRules.ApplyHit(d, "b", "a", 50, 5, t + 3d).Refusal);
        }

        [Test]
        public void Forfeit_LeavingArea_Disconnect_Interrupt()
        {
            DuelState f = Fighting(out double t);
            Assert.IsTrue(DuelRules.Forfeit(f, "b", t + 1d).Ok);
            Assert.AreEqual("a", f.WinnerId);
            Assert.AreEqual(DuelEndReason.Forfeit, f.EndReason);

            DuelState pending = DuelRules.Challenge("p", C("a"), C("b", 2f), false, false, 0d).Duel;
            Assert.AreEqual(PvpRefusal.WrongPhase, DuelRules.Forfeit(pending, "a", 1d).Refusal);

            DuelState area = Fighting(out t);
            DuelRules.Tick(area, F(2f + DuelRules.AreaRadius), F(4f), t + 1d); // exactly on the edge: still inside
            Assert.AreEqual(DuelPhase.Fighting, area.Phase);
            DuelRules.Tick(area, F(2f + DuelRules.AreaRadius + 0.5f), F(4f), t + 2d);
            Assert.AreEqual(DuelEndReason.LeftArea, area.EndReason);
            Assert.AreEqual("b", area.WinnerId);

            // Leaving during the countdown already loses.
            DuelState cd = DuelRules.Challenge("cd", C("a"), C("b", 4f), false, false, 0d).Duel;
            DuelRules.Accept(cd, C("b", 4f), C("a"), false, 1d);
            DuelRules.Tick(cd, F(0f), F(60f), 2d);
            Assert.AreEqual(DuelEndReason.LeftArea, cd.EndReason);
            Assert.AreEqual("a", cd.WinnerId);

            DuelState dc = Fighting(out t);
            DuelRules.Tick(dc, F(0f, connected: false), F(4f), t + 1d);
            Assert.AreEqual(DuelEndReason.Disconnected, dc.EndReason);
            Assert.AreEqual("b", dc.WinnerId);

            DuelState it = Fighting(out t);
            Assert.IsTrue(DuelRules.Interrupt(it, t + 1d).Ok);
            Assert.AreEqual(DuelEndReason.Interrupted, it.EndReason);
            Assert.IsNull(it.WinnerId);
        }

        [Test]
        public void TimeUp_HigherLifeFractionWins_EqualIsDraw()
        {
            DuelState d = Fighting(out double t);
            DuelRules.Tick(d, F(0f, 50, 100), F(4f, 300, 1000), d.FightEndsAt - 0.1d);
            Assert.AreEqual(DuelPhase.Fighting, d.Phase);
            DuelRules.Tick(d, F(0f, 50, 100), F(4f, 300, 1000), d.FightEndsAt);
            Assert.AreEqual(DuelEndReason.TimeUp, d.EndReason);
            Assert.AreEqual("a", d.WinnerId, "50% beats 30%");

            DuelState draw = Fighting(out t);
            DuelRules.Tick(draw, F(0f, 40, 100), F(4f, 400, 1000), draw.FightEndsAt);
            Assert.IsTrue(draw.IsDraw);
        }

        // ------------------------------------------------------------------ pode ferir?

        [Test]
        public void SealedDuel_IsolatesBothWays_AndOnlyDuringFight()
        {
            DuelState d = DuelRules.Challenge("d", C("a"), C("b", 4f), false, false, 0d).Duel;
            DuelRules.Accept(d, C("b", 4f), C("a"), false, 1d);
            PvpCombatant a = P("a", MapRegion.Zarafshan, 5, duel: d);
            PvpCombatant b = P("b", MapRegion.Zarafshan, 5, duel: d);
            PvpCombatant x = P("x", MapRegion.Zarafshan, 5);

            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, a, b), "countdown: no hits");
            DuelRules.Tick(d, F(0f), F(4f), d.FightStartsAt);
            Assert.IsTrue(PvpCombatRules.CanHarmPlayer(null, a, b), "duel works even in a protected starting region");
            Assert.AreEqual(PvpHarmBasis.Duel, PvpCombatRules.Evaluate(null, b, a));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, x, a), "outsiders never hurt a duelist");
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, a, x), "a duelist never hurts an outsider");
            Assert.IsFalse(PvpCombatRules.CanAssist(x, a), "nor heals one");
            Assert.IsTrue(PvpCombatRules.CanAssist(a, a));

            // High-level outsider in an open area still cannot touch a duelist there.
            DuelState d2 = Fighting(out _);
            PvpCombatant a2 = P("a", duel: d2);
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, P("y"), a2));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, a2, P("y")));

            DuelRules.Forfeit(d2, "a", 999d);
            Assert.AreEqual(PvpHarmBasis.None, PvpCombatRules.HarmBasis(P("a", MapRegion.Zarafshan, duel: d2), P("b", MapRegion.Zarafshan, duel: d2)),
                "after the end, no basis in a protected region");
        }

        [Test]
        public void CanHarm_RespectsPartyAndCity()
        {
            var party = new PartyState("a", null);
            PartyRules.Invite(party, "a", "b", null, false, 0d);
            PartyRules.Accept(party, "b", 1d);
            Assert.AreEqual(PvpHarmBasis.OpenArea, PvpCombatRules.HarmBasis(P("a"), P("b")));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(party, P("a"), P("b")), "no friendly fire, even in open PvP");
            Assert.IsTrue(PvpCombatRules.CanHarmPlayer(party, P("a"), P("c")));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, P("a"), P("c", zone: ZoneKind.City)));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, P("a"), P("a")));
        }

        [Test]
        public void Arena_OnlyOtherTeamOfSameMatch()
        {
            PvpCombatant a1 = P("a1", MapRegion.Zarafshan, 30, match: "m1", team: 0);
            PvpCombatant a2 = P("a2", MapRegion.Zarafshan, 30, match: "m1", team: 0);
            PvpCombatant b1 = P("b1", MapRegion.Zarafshan, 30, match: "m1", team: 1);
            PvpCombatant other = P("o", MapRegion.Zarafshan, 30, match: "m2", team: 1);
            Assert.AreEqual(PvpHarmBasis.Arena, PvpCombatRules.Evaluate(null, a1, b1));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, a1, a2));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, a1, other));
            Assert.IsFalse(PvpCombatRules.CanHarmPlayer(null, P("x"), b1));
            Assert.IsTrue(PvpCombatRules.CanAssist(a1, a2));
            Assert.IsFalse(PvpCombatRules.CanAssist(a1, b1));
            Assert.IsFalse(PvpCombatRules.MonsterCanHarm(a1));
        }

        [Test]
        public void GuildWar_OutsideProtectedRegionsOnly_NoLevelGate()
        {
            Assert.AreEqual(PvpHarmBasis.GuildWar, PvpCombatRules.Evaluate(null, P("a", level: 40), P("b", level: 40), atGuildWar: true));
            Assert.AreEqual(PvpHarmBasis.None, PvpCombatRules.Evaluate(null, P("a", level: 40), P("b", level: 40)));
            Assert.AreEqual(PvpHarmBasis.None, PvpCombatRules.Evaluate(null, P("a", MapRegion.GrutasDoLimiar), P("b", MapRegion.GrutasDoLimiar), atGuildWar: true));
        }

        // ------------------------------------------------------------------ areas

        [Test]
        public void Areas_StartingRegionsProtected_DisputeLandMarked_LevelGate()
        {
            foreach (MapRegion r in new[] { MapRegion.Zarafshan, MapRegion.AreiasFlamejantes, MapRegion.SerraDeKaggen, MapRegion.GrutasDoLimiar })
            {
                Assert.AreEqual(PvpAreaKind.Protected, PvpAreaRules.AreaOf(r));
                Assert.IsFalse(PvpAreaRules.OpenPvpAllowed(r, ZoneKind.Wilds, 500));
            }

            Assert.AreEqual(PvpAreaKind.DisputeLand, PvpAreaRules.AreaOf(MapRegion.MargensDaMistura));
            Assert.AreEqual(PvpAreaKind.Frontier, PvpAreaRules.AreaOf(MapRegion.CostaDasMoncoes));
            Assert.AreEqual(PvpAreaKind.Frontier, PvpAreaRules.AreaOf(MapRegion.ReinosDoCeu));
            Assert.AreEqual(4, MapCatalog.Regions.Count(r => PvpAreaRules.AreaOf(r) == PvpAreaKind.Protected));
            Assert.AreEqual(11, MapCatalog.Regions.Count(r => PvpAreaRules.AreaOf(r) == PvpAreaKind.Frontier));
            Assert.AreEqual(1, MapCatalog.Regions.Count(r => PvpAreaRules.AreaOf(r) == PvpAreaKind.DisputeLand));

            Assert.IsTrue(PvpAreaRules.OpenPvpAllowed(MapRegion.CostaDasMoncoes, ZoneKind.Wilds, PvpAreaRules.OpenPvpMinLevel));
            Assert.IsFalse(PvpAreaRules.OpenPvpAllowed(MapRegion.CostaDasMoncoes, ZoneKind.Wilds, PvpAreaRules.OpenPvpMinLevel - 1));
            Assert.IsFalse(PvpAreaRules.OpenPvpAllowed(MapRegion.CostaDasMoncoes, ZoneKind.City, 300));
            Assert.AreEqual(PvpHarmBasis.None, PvpCombatRules.HarmBasis(P("a", level: 300), P("b", level: 79)), "a low-level visitor is never a target");
            Assert.AreEqual(PvpHarmBasis.None, PvpCombatRules.HarmBasis(P("a", level: 79), P("b", level: 300)), "nor a hunter");

            Assert.AreEqual(PvpAreaKind.Protected, PvpAreaRules.AreaOfMap(MapCatalog.Get(1).Id));
            Assert.AreEqual(PvpAreaKind.DisputeLand, PvpAreaRules.AreaOfMap(MapCatalog.Get(77).Id));
            Assert.AreEqual(PvpAreaKind.Protected, PvpAreaRules.AreaOfMap("no_such_map"));
        }

        // ------------------------------------------------------------------ PK

        [Test]
        public void Pk_OnlyUnjustifiedFrontierKillsStain()
        {
            Assert.IsTrue(PkRules.KillStains(PvpHarmBasis.OpenArea, PvpAreaKind.Frontier, PkStanding.Clean, -1d));
            Assert.IsTrue(PkRules.KillStains(PvpHarmBasis.OpenArea, PvpAreaKind.Frontier, PkStanding.Stained, 61d));
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.OpenArea, PvpAreaKind.Frontier, PkStanding.Clean, 30d), "self-defence");
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.OpenArea, PvpAreaKind.Frontier, PkStanding.Outlaw, -1d), "hunting an outlaw");
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.OpenArea, PvpAreaKind.DisputeLand, PkStanding.Clean, -1d));
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.GuildWar, PvpAreaKind.Frontier, PkStanding.Clean, -1d));
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.Duel, PvpAreaKind.Frontier, PkStanding.Clean, -1d));
            Assert.IsFalse(PkRules.KillStains(PvpHarmBasis.Arena, PvpAreaKind.Frontier, PkStanding.Clean, -1d));
        }

        [Test]
        public void Pk_StandingsEffectsFadeAndBail()
        {
            var pk = new PkState();
            Assert.AreEqual(PkStanding.Clean, pk.Standing);
            for (int i = 0; i < 12; i++)
            {
                PkRules.RegisterKill(pk, PvpHarmBasis.OpenArea, PvpAreaKind.Frontier, PkStanding.Clean, -1d);
            }

            Assert.AreEqual(PkRules.MaxStains, pk.Stains, "capped");
            Assert.AreEqual(PkStanding.Murderer, pk.Standing);
            Assert.AreEqual(1.4f, PkRules.ShopPriceMultiplier(pk.Standing));
            Assert.AreEqual(2f, PkRules.DeathXpLossMultiplier(pk.Standing));
            Assert.IsFalse(PkRules.CanQueueArena(pk.Standing));
            Assert.AreEqual(1f, PkRules.DeathItemDropMultiplier(pk.Standing), "never more item loss");
            Assert.AreEqual(10 * 3600d, PkRules.SecondsUntilClean(pk));

            Assert.AreEqual(0, PkRules.AdvancePlayTime(pk, 3599d));
            Assert.AreEqual(1, PkRules.AdvancePlayTime(pk, 1d));
            Assert.AreEqual(9, pk.Stains);
            Assert.AreEqual(5, PkRules.AdvancePlayTime(pk, 5 * 3600d));
            Assert.AreEqual(PkStanding.Outlaw, pk.Standing);
            Assert.AreEqual(1.2f, PkRules.ShopPriceMultiplier(pk.Standing));
            Assert.AreEqual(1.5f, PkRules.DeathXpLossMultiplier(pk.Standing));
            Assert.IsTrue(PkRules.CanQueueArena(pk.Standing));

            Assert.AreEqual(PvpRefusal.NotInCity, PkRules.PayBail(pk, false, 1000000, out _));
            Assert.AreEqual(PvpRefusal.NotEnoughGold, PkRules.PayBail(pk, true, 100, out _));
            Assert.AreEqual(PvpRefusal.None, PkRules.PayBail(pk, true, 1000000, out long cost));
            Assert.AreEqual(PkRules.BailCostPerStain, cost);
            Assert.AreEqual(3, pk.Stains);
            PkRules.AdvancePlayTime(pk, 99999d);
            Assert.AreEqual(PkStanding.Clean, pk.Standing);
            Assert.AreEqual(0d, pk.FadeProgressSeconds);
            Assert.AreEqual(PvpRefusal.NoStains, PkRules.PayBail(pk, true, 1000000, out _));

            Assert.AreEqual(PkStanding.Stained, PkRules.StandingOf(1));
            Assert.AreEqual(PkStanding.Outlaw, PkRules.StandingOf(4));
            Assert.AreEqual(PkStanding.Murderer, PkRules.StandingOf(5));
        }

        // ------------------------------------------------------------------ arena: rating

        [Test]
        public void Elo_PlacementMovesMore_ThenKByRating()
        {
            var a = new ArenaRating();
            var b = new ArenaRating();
            int[] d = ArenaRules.ApplyMatch(new[] { a }, new[] { b }, ArenaOutcome.TeamAWins);
            Assert.AreEqual(new[] { 32, -32 }, d, "equal ratings, K 64 while placing");
            Assert.AreEqual(1532, a.Rating);
            Assert.AreEqual(1, a.Wins);
            Assert.AreEqual(1, b.Losses);
            Assert.AreEqual(4, a.PlacementLeft);
            Assert.AreEqual(0, a.SeasonHigh, "no high while placing");

            var placed = new ArenaRating(1500, 10, 5, 5);
            Assert.AreEqual(32, ArenaRules.KFactor(placed));
            Assert.AreEqual(24, ArenaRules.KFactor(new ArenaRating(1900, 10)));
            Assert.AreEqual(16, ArenaRules.KFactor(new ArenaRating(2200, 10)));

            var p2 = new ArenaRating(1500, 10);
            ArenaRules.ApplyMatch(new[] { placed }, new[] { p2 }, ArenaOutcome.Draw);
            Assert.AreEqual(1500, placed.Rating);
            Assert.AreEqual(1, placed.Draws);

            // Favourite gains little, underdog gains a lot.
            var strong = new ArenaRating(1800, 10);
            var weak = new ArenaRating(1400, 10);
            int[] fav = ArenaRules.ApplyMatch(new[] { strong }, new[] { weak }, ArenaOutcome.TeamAWins);
            Assert.AreEqual(2, fav[0]);
            Assert.AreEqual(-3, fav[1]);
            Assert.AreEqual(0.5d, ArenaRules.ExpectedScore(1500, 1500), 1e-9);

            var floor = new ArenaRating(ArenaRules.MinRating, 10);
            ArenaRules.ApplyMatch(new[] { floor }, new[] { new ArenaRating(ArenaRules.MinRating, 10) }, ArenaOutcome.TeamBWins);
            Assert.AreEqual(ArenaRules.MinRating, floor.Rating);
        }

        [Test]
        public void Elo_TeamsUseAverageAndOwnK()
        {
            var a1 = new ArenaRating(1600, 10);
            var a2 = new ArenaRating(1400); // still placing
            var b1 = new ArenaRating(1500, 10);
            var b2 = new ArenaRating(1500, 10);
            Assert.AreEqual(1500, ArenaRules.TeamRating(new[] { a1, a2 }));
            int[] d = ArenaRules.ApplyMatch(new[] { a1, a2 }, new[] { b1, b2 }, ArenaOutcome.TeamAWins);
            Assert.AreEqual(new[] { 16, 32, -16, -16 }, d);
        }

        [Test]
        public void Season_SoftResetAndCosmeticRewardsOnly()
        {
            var r = new ArenaRating(2100, 30, 20, 10, 0, 2150);
            ArenaSeasonReward reward = ArenaRules.SeasonReward(r, ArenaBracket.Duo, 3);
            Assert.AreEqual(ArenaTier.ColunaDaGloria, reward.Tier);
            Assert.AreEqual("pvp.arena.title.coluna_da_gloria", reward.TitleKey);
            Assert.AreEqual("cosmetic_arena_s3_duo_coluna_da_gloria", reward.CosmeticId);
            Assert.IsFalse(reward.GrantsStats);

            Assert.IsFalse(ArenaRules.SeasonReward(new ArenaRating(2000, 9), ArenaBracket.Solo, 1).HasReward, "needs 10 matches");
            Assert.AreEqual(ArenaTier.Centelha, ArenaRules.TierOf(1399));
            Assert.AreEqual(ArenaTier.Chama, ArenaRules.TierOf(1400));
            Assert.AreEqual(ArenaTier.Lampada, ArenaRules.TierOf(1600));
            Assert.AreEqual(ArenaTier.Estrela, ArenaRules.TierOf(1999));

            ArenaRules.SoftReset(r);
            Assert.AreEqual(1800, r.Rating);
            Assert.IsFalse(r.IsPlaced);
            Assert.AreEqual(0, r.SeasonHigh);
            var low = new ArenaRating(1201, 10);
            ArenaRules.SoftReset(low);
            Assert.AreEqual(1351, low.Rating, "1500 − 149.5 rounds away from zero");
        }

        // ------------------------------------------------------------------ arena: queue

        private static ArenaQueueMember M(string id, int rating, int level = 50, PkStanding s = PkStanding.Clean, bool busy = false) =>
            new ArenaQueueMember(id, rating, level, s, busy);

        [Test]
        public void Queue_EntryChecks()
        {
            var q = new ArenaQueue();
            Assert.AreEqual(PvpRefusal.WrongTeamSize, q.Enqueue("e", ArenaBracket.Duo, new[] { M("a", 1500) }, null, 0d));
            Assert.AreEqual(PvpRefusal.TeamNotOneParty, q.Enqueue("e", ArenaBracket.Duo, new[] { M("a", 1500), M("b", 1500) }, null, 0d));
            Assert.AreEqual(PvpRefusal.LevelTooLow, q.Enqueue("e", ArenaBracket.Solo, new[] { M("a", 1500, 29) }, null, 0d));
            Assert.AreEqual(PvpRefusal.OutlawCannotQueue, q.Enqueue("e", ArenaBracket.Solo, new[] { M("a", 1500, 50, PkStanding.Murderer) }, null, 0d));
            Assert.AreEqual(PvpRefusal.Busy, q.Enqueue("e", ArenaBracket.Solo, new[] { M("a", 1500, busy: true) }, null, 0d));
            Assert.AreEqual(PvpRefusal.None, q.Enqueue("a", ArenaBracket.Solo, new[] { M("a", 1500, 50, PkStanding.Outlaw) }, null, 0d));
            Assert.AreEqual(PvpRefusal.AlreadyQueued, q.Enqueue("a", ArenaBracket.Solo, new[] { M("a", 1500) }, null, 0d));

            var party = new PartyState("a", null);
            PartyRules.Invite(party, "a", "b", null, false, 0d);
            PartyRules.Accept(party, "b", 1d);
            Assert.AreEqual(PvpRefusal.AlreadyQueued, q.Enqueue("p", ArenaBracket.Duo, new[] { M("a", 1500), M("b", 1600) }, party, 0d), "a is already queued solo");
            Assert.AreEqual(PvpRefusal.None, q.Leave("a"));
            Assert.AreEqual(PvpRefusal.NotQueued, q.Leave("a"));
            Assert.AreEqual(PvpRefusal.None, q.Enqueue("p", ArenaBracket.Duo, new[] { M("a", 1500), M("b", 1600) }, party, 0d));
            Assert.AreEqual(1550, q.Entries[0].Rating);
        }

        [Test]
        public void Queue_BandWidensWithWait()
        {
            Assert.AreEqual(100, ArenaRules.SearchBand(0d));
            Assert.AreEqual(100, ArenaRules.SearchBand(9.9d));
            Assert.AreEqual(125, ArenaRules.SearchBand(10d));
            Assert.AreEqual(500, ArenaRules.SearchBand(10000d));

            var q = new ArenaQueue();
            q.Enqueue("a", ArenaBracket.Solo, new[] { M("a", 1500) }, null, 0d);
            q.Enqueue("b", ArenaBracket.Solo, new[] { M("b", 1700) }, null, 0d);
            Assert.AreEqual(0, q.FindMatches(0d, 7).Count, "200 apart, band 100");
            Assert.AreEqual(0, q.FindMatches(30d, 7).Count, "band 175");
            List<ArenaPairing> m = q.FindMatches(40d, 7);
            Assert.AreEqual(1, m.Count, "band 200");
            Assert.AreEqual(0, q.Entries.Count);
        }

        [Test]
        public void Queue_OldestFirstClosestRating_BracketsApart_Deterministic()
        {
            ArenaQueue Build()
            {
                var q = new ArenaQueue();
                q.Enqueue("old", ArenaBracket.Solo, new[] { M("old", 1500) }, null, 0d);
                q.Enqueue("far", ArenaBracket.Solo, new[] { M("far", 1590) }, null, 1d);
                q.Enqueue("near", ArenaBracket.Solo, new[] { M("near", 1520) }, null, 2d);
                q.Enqueue("same", ArenaBracket.Solo, new[] { M("same", 1520) }, null, 3d);
                q.Enqueue("trio", ArenaBracket.Trio, new[] { M("t1", 1500), M("t2", 1500), M("t3", 1500) }, Trio(), 0d);
                return q;
            }

            var q1 = Build();
            List<ArenaPairing> m1 = q1.FindMatches(3d, 42);
            Assert.AreEqual(2, m1.Count);
            var firstIds = new[] { m1[0].TeamA.EntryId, m1[0].TeamB.EntryId };
            CollectionAssert.AreEquivalent(new[] { "old", "near" }, firstIds, "closest, older on a tie");
            CollectionAssert.AreEquivalent(new[] { "far", "same" }, new[] { m1[1].TeamA.EntryId, m1[1].TeamB.EntryId });
            Assert.AreEqual(1, q1.Entries.Count);
            Assert.AreEqual("trio", q1.Entries[0].EntryId, "no opponent in its bracket");

            List<ArenaPairing> m2 = Build().FindMatches(3d, 42);
            for (int i = 0; i < m1.Count; i++)
            {
                Assert.AreEqual(m1[i].TeamA.EntryId, m2[i].TeamA.EntryId);
                Assert.AreEqual(m1[i].FieldIndex, m2[i].FieldIndex);
                Assert.That(m1[i].FieldIndex, Is.InRange(0, ArenaRules.FieldCount - 1));
            }

            Assert.AreEqual(ArenaRules.PickIndex(1, 2, 3), ArenaRules.PickIndex(1, 2, 3));
            Assert.AreEqual(0, ArenaRules.PickIndex(1, 2, 1));
        }

        private static PartyState Trio()
        {
            var party = new PartyState("t1", null);
            foreach (string id in new[] { "t2", "t3" })
            {
                PartyRules.Invite(party, "t1", id, null, false, 0d);
                PartyRules.Accept(party, id, 1d);
            }

            return party;
        }

        // ------------------------------------------------------------------ ranking

        [Test]
        public void Ranking_StableOrderAndSharedRanks()
        {
            var entries = new[]
            {
                new RankingEntry("c", "C", 50, 1000, reachedAt: 5d),
                new RankingEntry("a", "A", 50, 1000, reachedAt: 5d),
                new RankingEntry("b", "B", 50, 1000, reachedAt: 3d),
                new RankingEntry("d", "D", 60, 10, reachedAt: 9d),
                new RankingEntry("e", "E", 50, 900, reachedAt: 1d),
                new RankingEntry("", "?", 99, 0),
            };

            List<RankedEntry> board = RankingRules.Rank(entries, RankingBoard.Level);
            Assert.AreEqual(new[] { "d", "b", "a", "c", "e" }, board.Select(r => r.Entry.PlayerId).ToArray());
            Assert.AreEqual(new[] { 1, 2, 2, 2, 5 }, board.Select(r => r.Rank).ToArray());
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }, board.Select(r => r.Position).ToArray());
            Assert.IsTrue(board[2].TiedWithPrevious);
            Assert.IsFalse(board[4].TiedWithPrevious);

            List<RankedEntry> reversed = RankingRules.Rank(entries.Reverse(), RankingBoard.Level);
            CollectionAssert.AreEqual(board.Select(r => r.Entry.PlayerId).ToArray(), reversed.Select(r => r.Entry.PlayerId).ToArray(), "input order never matters");
            Assert.AreEqual(2, RankingRules.Rank(entries, RankingBoard.Level, 2).Count);
            Assert.AreEqual(3, RankingRules.PositionOf(entries, RankingBoard.Level, "a"));
            Assert.AreEqual(0, RankingRules.PositionOf(entries, RankingBoard.Level, "zzz"));
        }

        [Test]
        public void Ranking_PvpPlacedOnly_AndEvents()
        {
            var entries = new[]
            {
                new RankingEntry("a", "A", 10, 0, 1800, 20, 30, true, 50),
                new RankingEntry("b", "B", 10, 0, 1800, 20, 25, true, 70),
                new RankingEntry("c", "C", 10, 0, 2400, 4, 4, false, 70, 1d),
                new RankingEntry("d", "D", 10, 0, 1700, 40, 60, true, 70, 2d),
            };

            Assert.AreEqual(new[] { "b", "a", "d" }, RankingRules.Rank(entries, RankingBoard.Pvp).Select(r => r.Entry.PlayerId).ToArray(),
                "unplaced out; same rating and wins: fewer matches ahead");
            List<RankedEntry> ev = RankingRules.Rank(entries, RankingBoard.Events);
            Assert.AreEqual(new[] { "b", "c", "d", "a" }, ev.Select(r => r.Entry.PlayerId).ToArray());
            Assert.AreEqual(new[] { 1, 1, 1, 4 }, ev.Select(r => r.Rank).ToArray());
            Assert.IsFalse(RankingRules.ResetsWithSeason(RankingBoard.Level));
            Assert.IsTrue(RankingRules.ResetsWithSeason(RankingBoard.Pvp));
            Assert.IsTrue(RankingRules.ResetsWithSeason(RankingBoard.Events));
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_EveryEnumHasPtAndEn()
        {
            var keys = new HashSet<string>(PvpTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(PvpTexts.Entries.Count, keys.Count, "no duplicate keys");
            foreach (PvpRefusal r in System.Enum.GetValues(typeof(PvpRefusal)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.RefusalKey(r)), r.ToString());
            }

            foreach (DuelEndReason r in System.Enum.GetValues(typeof(DuelEndReason)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.EndReasonKey(r)), r.ToString());
            }

            foreach (PkStanding s in System.Enum.GetValues(typeof(PkStanding)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.StandingKey(s)), s.ToString());
            }

            foreach (PvpAreaKind k in System.Enum.GetValues(typeof(PvpAreaKind)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.AreaKey(k)), k.ToString());
            }

            foreach (ArenaTier t in System.Enum.GetValues(typeof(ArenaTier)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.ArenaTierKey(t)), t.ToString());
                Assert.IsTrue(keys.Contains(PvpTexts.ArenaTitleKey(t)), t.ToString());
            }

            foreach (ArenaBracket b in System.Enum.GetValues(typeof(ArenaBracket)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.BracketKey(b)), b.ToString());
            }

            foreach (RankingBoard b in System.Enum.GetValues(typeof(RankingBoard)))
            {
                Assert.IsTrue(keys.Contains(PvpTexts.BoardKey(b)), b.ToString());
            }

            foreach (var e in PvpTexts.Entries.Where(e => e.Key != PvpTexts.EndReasonKey(DuelEndReason.None)
                         && e.Key != PvpTexts.ArenaTitleKey(ArenaTier.None)))
            {
                Assert.IsFalse(string.IsNullOrEmpty(e.En), e.Key);
                Assert.IsFalse(string.IsNullOrEmpty(e.PtBR), e.Key);
            }

            Assert.AreEqual("Fora da lei", PvpTexts.Get(null, PvpTexts.StandingKey(PkStanding.Outlaw)));
            Assert.AreEqual("Ana desafiou você para um duelo.", PvpTexts.Format(null, PvpTexts.ChallengeReceived, "Ana"));
            Assert.AreEqual("outlaw_cannot_queue", PvpTexts.Snake("OutlawCannotQueue"));
        }
    }
}
