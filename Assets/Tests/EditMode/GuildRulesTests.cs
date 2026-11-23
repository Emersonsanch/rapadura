using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Guild;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure guild ("clã", TODO.md 12.3): founding (cost, level, name, filter, motto, emblem), ranks and the permission
    /// table, invite / leave / kick / promote / demote / transfer / inactive leader, bank with daily limits and log,
    /// contribution and level, board and message of the day, war (declare → accept → active → end) and "can A hurt B?".
    /// </summary>
    public class GuildRulesTests
    {
        private const double Day = GuildRules.SecondsPerDay;

        private static GuildState NewGuild(string founder = "L", string name = "Coluna da Glória", double now = 0d, string id = "g1")
        {
            GuildCreateResult result = GuildRules.Create(id, founder, 40, 10000, false, -1d, name, "Pela Luz", GuildEmblem.Default, null, now);
            Assert.IsTrue(result.Ok, result.Refusal.ToString());
            return result.Guild;
        }

        private static void Join(GuildState guild, string id, double now = 1d)
        {
            Assert.IsTrue(GuildRules.Invite(guild, guild.LeaderId, id, false, -1d, now).Ok);
            Assert.IsTrue(GuildRules.Accept(guild, id, now).Ok);
        }

        private static void SetRank(GuildState guild, string id, GuildRank rank)
        {
            guild.Find(id).Rank = rank;
        }

        /// <summary>Leader L, Vice V, Officer O, Member M, Recruit R.</summary>
        private static GuildState FullRanks(double now = 1d)
        {
            GuildState guild = NewGuild();
            foreach (string id in new[] { "V", "O", "M", "R" })
            {
                Join(guild, id, now);
            }

            SetRank(guild, "V", GuildRank.Vice);
            SetRank(guild, "O", GuildRank.Officer);
            SetRank(guild, "M", GuildRank.Member);
            return guild;
        }

        // ------------------------------------------------------------------ founding

        [Test]
        public void Create_MakesFounderLeaderAndCostsGold()
        {
            GuildCreateResult result = GuildRules.Create("g", "a", 30, 5000, false, -1d, "  Barca   de Luz ", "Luz que volta", GuildEmblem.Default, null, 0d);
            Assert.IsTrue(result.Ok);
            Assert.AreEqual(GuildRules.CreationCost, result.GoldCost);
            Assert.AreEqual("Barca de Luz", result.Guild.Name);
            Assert.AreEqual("barca de luz", result.Guild.NameKey);
            Assert.AreEqual("a", result.Guild.LeaderId);
            Assert.AreEqual(GuildRank.Leader, result.Guild.Find("a").Rank);
            Assert.AreEqual(1, result.Guild.Level);
        }

        [Test]
        public void Create_RefusesEachRequirement()
        {
            GuildEmblem ok = GuildEmblem.Default;
            Assert.AreEqual(GuildRefusal.LevelTooLow, GuildRules.Create("g", "a", 29, 9999, false, -1d, "Luz", "", ok, null, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.NotEnoughGold, GuildRules.Create("g", "a", 50, 4999, false, -1d, "Luz", "", ok, null, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.AlreadyInGuild, GuildRules.Create("g", "a", 50, 9999, true, -1d, "Luz", "", ok, null, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.RejoinCooldown, GuildRules.Create("g", "a", 50, 9999, false, 3600d, "Luz", "", ok, null, 0d).Refusal);
            Assert.IsTrue(GuildRules.Create("g", "a", 50, 9999, false, GuildRules.RejoinCooldownSeconds, "Luz", "", ok, null, 0d).Ok);
            Assert.AreEqual(GuildRefusal.InvalidPlayer, GuildRules.Create("", "a", 50, 9999, false, -1d, "Luz", "", ok, null, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.NameTaken,
                GuildRules.Create("g", "a", 50, 9999, false, -1d, "COLUNA DA GLORIA", "", ok, key => key == "coluna da gloria", 0d).Refusal);
            Assert.AreEqual(GuildRefusal.TextTooLong,
                GuildRules.Create("g", "a", 50, 9999, false, -1d, "Luz", new string('a', GuildRules.MaxMottoLength + 1), ok, null, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.TextOffensive, GuildRules.Create("g", "a", 50, 9999, false, -1d, "Luz", "que merda", ok, null, 0d).Refusal);
            var bad = new GuildEmblem(GuildEmblemShape.Circle, GuildEmblemColor.Black, GuildEmblemColor.Black, GuildEmblemSymbol.Sun, GuildEmblemColor.Gold);
            Assert.AreEqual(GuildRefusal.EmblemInvalid, GuildRules.Create("g", "a", 50, 9999, false, -1d, "Luz", "", bad, null, 0d).Refusal);
            Assert.IsNull(GuildRules.Create("g", "a", 1, 0, false, -1d, "Luz", "", ok, null, 0d).Guild);
            Assert.AreEqual(0, GuildRules.Create("g", "a", 1, 0, false, -1d, "Luz", "", ok, null, 0d).GoldCost);
        }

        [Test]
        public void Name_ShapeRules()
        {
            Assert.AreEqual(GuildRefusal.None, GuildNameRules.ValidateName("Árvore da Vida"));
            Assert.AreEqual(GuildRefusal.None, GuildNameRules.ValidateName("Luz 777"));
            Assert.AreEqual(GuildRefusal.NameTooShort, GuildNameRules.ValidateName(" Lu "));
            Assert.AreEqual(GuildRefusal.NameTooLong, GuildNameRules.ValidateName("Coluna da Glória Eterna"));
            Assert.AreEqual(GuildRefusal.NameInvalidCharacters, GuildNameRules.ValidateName("7 Luzes"));
            Assert.AreEqual(GuildRefusal.NameInvalidCharacters, GuildNameRules.ValidateName("Luz_Viva"));
            Assert.AreEqual(GuildRefusal.NameInvalidCharacters, GuildNameRules.ValidateName("Luz 1234"));
            Assert.AreEqual(GuildRefusal.NameInvalidCharacters, GuildNameRules.ValidateName("Свет Луз"));
            Assert.AreEqual(GuildRefusal.NameOffensive, GuildNameRules.ValidateName("GM Oficial"));
            Assert.AreEqual(GuildRefusal.NameOffensive, GuildNameRules.ValidateName("Os Admin"));
            Assert.AreEqual(GuildNameRules.NameKey("Pérola  Viva"), GuildNameRules.NameKey("perola viva"));
        }

        [Test]
        public void Filter_CatchesAccentsLeetRepeatsAndSpacing()
        {
            Assert.IsTrue(GuildNameRules.IsOffensive("Caralho"));
            Assert.IsTrue(GuildNameRules.IsOffensive("C4R@LH0"));
            Assert.IsTrue(GuildNameRules.IsOffensive("cárálhooo"));
            Assert.IsTrue(GuildNameRules.IsOffensive("f.u.c.k"));
            Assert.IsTrue(GuildNameRules.IsOffensive("os fuckers"));
            Assert.IsTrue(GuildNameRules.IsOffensive("p u t a"));
            Assert.IsTrue(GuildNameRules.IsOffensive("Filho da Put4"));
            Assert.IsTrue(GuildNameRules.IsOffensive("porraaa"));
            Assert.IsTrue(GuildNameRules.IsOffensive("Sh1t"));
            Assert.IsTrue(GuildNameRules.IsOffensive("Cu"));

            // Harmless words that hide a short one.
            Assert.IsFalse(GuildNameRules.IsOffensive("Escudo da Luz"));
            Assert.IsFalse(GuildNameRules.IsOffensive("Espora de Prata"));
            Assert.IsFalse(GuildNameRules.IsOffensive("Disputa Justa"));
            Assert.IsFalse(GuildNameRules.IsOffensive("As Barcas de Luz"));
            Assert.IsFalse(GuildNameRules.IsOffensive("Computador"));
            Assert.IsFalse(GuildNameRules.IsOffensive("Classe Assassina"));
            Assert.IsFalse(GuildNameRules.IsOffensive(""));
            Assert.IsFalse(GuildNameRules.IsOffensive(null));
        }

        [Test]
        public void Filter_IsExtensible()
        {
            Assert.IsFalse(GuildNameRules.IsOffensive("Trevoso Zurg"));
            try
            {
                GuildNameRules.AddWholeWord("zurg");
                Assert.IsTrue(GuildNameRules.IsOffensive("Trevoso Zurg"));
                Assert.IsTrue(GuildNameRules.IsOffensive("Trevoso ZUURG"));
                Assert.IsFalse(GuildNameRules.IsOffensive("Zurgada"));
                GuildNameRules.AddContainsWord("blorft");
                Assert.IsTrue(GuildNameRules.IsOffensive("osbl0rftianos"));
            }
            finally
            {
                // The lists are static: without this a second run (no domain reload) fails the first assert.
                GuildNameRules.RemoveWord("zurg");
                GuildNameRules.RemoveWord("blorft");
            }

            Assert.IsFalse(GuildNameRules.IsOffensive("Trevoso Zurg"));
            Assert.IsFalse(GuildNameRules.IsOffensive("osbl0rftianos"));
        }

        [Test]
        public void Emblem_ValidatesAndRoundTrips()
        {
            Assert.IsTrue(GuildEmblem.Default.Validate());
            var emblem = new GuildEmblem(GuildEmblemShape.Lamp, GuildEmblemColor.Indigo, GuildEmblemColor.Gold, GuildEmblemSymbol.LightShip, GuildEmblemColor.White);
            Assert.IsTrue(emblem.Validate());
            Assert.IsTrue(GuildEmblem.TryParse(emblem.ToCode(), out GuildEmblem parsed));
            Assert.AreEqual(emblem, parsed);

            Assert.IsFalse(new GuildEmblem(GuildEmblemShape.Lamp, GuildEmblemColor.Indigo, GuildEmblemColor.Gold, GuildEmblemSymbol.Sun, GuildEmblemColor.Indigo).Validate());
            Assert.IsTrue(new GuildEmblem(GuildEmblemShape.Lamp, GuildEmblemColor.Indigo, GuildEmblemColor.Gold, GuildEmblemSymbol.None, GuildEmblemColor.Indigo).Validate());
            Assert.IsFalse(new GuildEmblem((GuildEmblemShape)99, GuildEmblemColor.Indigo, GuildEmblemColor.Gold, GuildEmblemSymbol.Sun, GuildEmblemColor.White).Validate());
            Assert.IsFalse(new GuildEmblem(GuildEmblemShape.Lamp, (GuildEmblemColor)(-1), GuildEmblemColor.Gold, GuildEmblemSymbol.Sun, GuildEmblemColor.White).Validate());
            Assert.IsFalse(GuildEmblem.TryParse("1-2-3", out _));
            Assert.IsFalse(GuildEmblem.TryParse("0-0-0-1-1", out _));
            Assert.IsFalse(GuildEmblem.TryParse("0-0-x-1-1", out _));
            Assert.IsFalse(GuildEmblem.TryParse("0-0-1--1-1", out _));
            Assert.IsFalse(GuildEmblem.TryParse(null, out GuildEmblem fallback));
            Assert.AreEqual(GuildEmblem.Default, fallback);
        }

        [Test]
        public void MottoAndEmblem_OnlyTheLeaderChangesThem()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.SetMotto(guild, "V", "Novo").Refusal);
            Assert.IsTrue(GuildRules.SetMotto(guild, "L", "  A Luz  volta ").Ok);
            Assert.AreEqual("A Luz volta", guild.Motto);
            Assert.AreEqual(GuildRefusal.TextOffensive, GuildRules.SetMotto(guild, "L", "merda").Refusal);
            var emblem = new GuildEmblem(GuildEmblemShape.Star, GuildEmblemColor.Black, GuildEmblemColor.Gold, GuildEmblemSymbol.Moon, GuildEmblemColor.White);
            Assert.IsTrue(GuildRules.SetEmblem(guild, "L", emblem).Ok);
            Assert.AreEqual(emblem, guild.Emblem);
            Assert.AreEqual(GuildRefusal.EmblemInvalid, GuildRules.SetEmblem(guild, "L", default).Refusal);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.SetEmblem(guild, "O", emblem).Refusal);
        }

        // ------------------------------------------------------------------ permissions

        [Test]
        public void PermissionTable_IsAsDocumented()
        {
            Assert.IsTrue(GuildPermissions.Has(GuildRank.Recruit, GuildPermission.BankDeposit));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Recruit, GuildPermission.BankWithdraw));
            Assert.IsTrue(GuildPermissions.Has(GuildRank.Member, GuildPermission.BankWithdraw));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Member, GuildPermission.Invite));
            Assert.IsTrue(GuildPermissions.Has(GuildRank.Officer, GuildPermission.Invite | GuildPermission.Kick | GuildPermission.EditBoard));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Officer, GuildPermission.Promote));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Officer, GuildPermission.EditMotd));
            Assert.IsTrue(GuildPermissions.Has(GuildRank.Vice, GuildPermission.Promote | GuildPermission.Demote | GuildPermission.EditMotd | GuildPermission.DeclareWar));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Vice, GuildPermission.Disband));
            Assert.IsFalse(GuildPermissions.Has(GuildRank.Vice, GuildPermission.EditIdentity));
            foreach (GuildPermission permission in GuildTexts.SinglePermissions)
            {
                Assert.IsTrue(GuildPermissions.Has(GuildRank.Leader, permission), permission.ToString());
            }

            Assert.IsFalse(GuildPermissions.Has(GuildRank.Leader, GuildPermission.None));

            // Each rank holds everything the rank below holds.
            for (GuildRank rank = GuildRank.Member; rank <= GuildRank.Leader; rank++)
            {
                GuildPermission below = GuildPermissions.Of(rank - 1);
                Assert.AreEqual(below, GuildPermissions.Of(rank) & below, rank.ToString());
            }
        }

        // ------------------------------------------------------------------ membership

        [Test]
        public void InviteAndAccept_JoinsAsRecruit()
        {
            GuildState guild = NewGuild();
            Assert.IsTrue(GuildRules.Invite(guild, "L", "a", false, -1d, 0d).Ok);
            Assert.AreEqual(1, guild.Invites.Count);
            Assert.IsTrue(GuildRules.Accept(guild, "a", 10d).Ok);
            Assert.AreEqual(GuildRank.Recruit, guild.Find("a").Rank);
            Assert.AreEqual(0, guild.Invites.Count);
            Assert.AreEqual(1, guild.Find("a").JoinOrder);
        }

        [Test]
        public void Invite_Refusals()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.Invite(guild, "M", "x", false, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.Invite(guild, "R", "x", false, -1d, 2d).Refusal);
            Assert.IsTrue(GuildRules.Invite(guild, "O", "x", false, -1d, 2d).Ok);
            Assert.AreEqual(GuildRefusal.AlreadyInvited, GuildRules.Invite(guild, "V", "x", false, -1d, 3d).Refusal);
            Assert.AreEqual(GuildRefusal.NotMember, GuildRules.Invite(guild, "zz", "y", false, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.CannotTargetSelf, GuildRules.Invite(guild, "L", "L", false, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.AlreadyInGuild, GuildRules.Invite(guild, "L", "M", false, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.AlreadyInGuild, GuildRules.Invite(guild, "L", "y", true, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.RejoinCooldown, GuildRules.Invite(guild, "L", "y", false, 60d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidPlayer, GuildRules.Invite(guild, "L", "", false, -1d, 2d).Refusal);
            Assert.AreEqual(GuildRefusal.Disbanded, GuildRules.Invite(null, "L", "y", false, -1d, 2d).Refusal);
        }

        [Test]
        public void Invite_ExpiresDeclinesAndReopens()
        {
            GuildState guild = NewGuild();
            Assert.IsTrue(GuildRules.Invite(guild, "L", "a", false, -1d, 0d).Ok);
            Assert.AreEqual(GuildRefusal.InviteExpired, GuildRules.Accept(guild, "a", GuildRules.InviteSeconds).Refusal);
            Assert.IsFalse(guild.IsMember("a"));
            Assert.AreEqual(GuildRefusal.NoInvite, GuildRules.Accept(guild, "a", 1d).Refusal);

            Assert.IsTrue(GuildRules.Invite(guild, "L", "b", false, -1d, 0d).Ok);
            Assert.AreEqual(new List<string> { "b" }, GuildRules.ExpireInvites(guild, GuildRules.InviteSeconds + 1d));
            Assert.IsTrue(GuildRules.Invite(guild, "L", "b", false, -1d, 1000d).Ok);
            Assert.IsTrue(GuildRules.Decline(guild, "b").Ok);
            Assert.AreEqual(GuildRefusal.NoInvite, GuildRules.Decline(guild, "b").Refusal);
        }

        [Test]
        public void Invite_RespectsMemberCapOfTheLevel()
        {
            GuildState guild = NewGuild();
            int cap = GuildLevelRules.MaxMembers(1);
            for (int i = 1; i < cap; i++)
            {
                Join(guild, "p" + i);
            }

            Assert.AreEqual(cap, guild.Count);
            Assert.AreEqual(GuildRefusal.GuildFull, GuildRules.Invite(guild, "L", "extra", false, -1d, 2d).Refusal);
            guild.TotalPoints = GuildLevelRules.PointsFor(2);
            Assert.IsTrue(GuildRules.Invite(guild, "L", "extra", false, -1d, 2d).Ok);
        }

        [Test]
        public void Leave_LeaderHandsOverAndLastMemberDisbands()
        {
            GuildState guild = FullRanks();
            Join(guild, "V2");
            SetRank(guild, "V2", GuildRank.Vice);

            GuildResult left = GuildRules.Leave(guild, "L");
            Assert.IsTrue(left.Ok);
            Assert.AreEqual("V", left.LeaderId); // highest rank, then the oldest
            Assert.AreEqual(GuildRank.Leader, guild.Find("V").Rank);

            Assert.AreEqual(GuildRefusal.NotMember, GuildRules.Leave(guild, "L").Refusal);

            GuildState solo = NewGuild("s");
            GuildResult last = GuildRules.Leave(solo, "s");
            Assert.IsTrue(last.Ok);
            Assert.IsTrue(last.GuildDisbanded);
            Assert.IsTrue(solo.IsDisbanded);
            Assert.AreEqual(GuildRefusal.Disbanded, GuildRules.Leave(solo, "s").Refusal);
        }

        [Test]
        public void Kick_OnlyRanksBelow()
        {
            GuildState guild = FullRanks();
            Join(guild, "O2");
            SetRank(guild, "O2", GuildRank.Officer);

            Assert.AreEqual(GuildRefusal.TargetRankTooHigh, GuildRules.Kick(guild, "O", "O2").Refusal);
            Assert.AreEqual(GuildRefusal.TargetRankTooHigh, GuildRules.Kick(guild, "O", "V").Refusal);
            Assert.AreEqual(GuildRefusal.TargetRankTooHigh, GuildRules.Kick(guild, "V", "L").Refusal);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.Kick(guild, "M", "R").Refusal);
            Assert.AreEqual(GuildRefusal.CannotTargetSelf, GuildRules.Kick(guild, "O", "O").Refusal);
            Assert.AreEqual(GuildRefusal.TargetNotMember, GuildRules.Kick(guild, "O", "nobody").Refusal);
            Assert.IsTrue(GuildRules.Kick(guild, "O", "M").Ok);
            Assert.IsTrue(GuildRules.Kick(guild, "V", "O2").Ok);
            Assert.IsTrue(GuildRules.Kick(guild, "L", "V").Ok);
            Assert.IsFalse(guild.IsMember("M"));
        }

        [Test]
        public void PromoteAndDemote_StopBelowTheActor()
        {
            GuildState guild = FullRanks();
            Assert.IsTrue(GuildRules.Promote(guild, "V", "R", GuildRank.Member).Ok);
            Assert.IsTrue(GuildRules.Promote(guild, "V", "R", GuildRank.Officer).Ok);
            Assert.AreEqual(GuildRefusal.TargetRankTooHigh, GuildRules.Promote(guild, "V", "M", GuildRank.Vice).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidRank, GuildRules.Promote(guild, "L", "M", GuildRank.Leader).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidRank, GuildRules.Promote(guild, "L", "M", GuildRank.Recruit).Refusal);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.Promote(guild, "O", "M", GuildRank.Officer).Refusal);
            Assert.IsTrue(GuildRules.Promote(guild, "L", "M", GuildRank.Vice).Ok);

            // Two Vices already (V, M): the third is refused.
            Assert.AreEqual(GuildRefusal.RankFull, GuildRules.Promote(guild, "L", "O", GuildRank.Vice).Refusal);

            Assert.AreEqual(GuildRefusal.TargetRankTooHigh, GuildRules.Demote(guild, "V", "M", GuildRank.Member).Refusal);
            Assert.IsTrue(GuildRules.Demote(guild, "L", "M", GuildRank.Recruit).Ok);
            Assert.AreEqual(GuildRank.Recruit, guild.Find("M").Rank);
            Assert.AreEqual(GuildRefusal.InvalidRank, GuildRules.Demote(guild, "V", "O", GuildRank.Vice).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidRank, GuildRules.Demote(guild, "V", "M", GuildRank.Recruit).Refusal);
        }

        [Test]
        public void Officers_AreCapped()
        {
            GuildState guild = NewGuild();
            for (int i = 0; i <= GuildRules.MaxOfficers; i++)
            {
                Join(guild, "p" + i);
            }

            for (int i = 0; i < GuildRules.MaxOfficers; i++)
            {
                Assert.IsTrue(GuildRules.Promote(guild, "L", "p" + i, GuildRank.Officer).Ok);
            }

            Assert.AreEqual(GuildRefusal.RankFull, GuildRules.Promote(guild, "L", "p" + GuildRules.MaxOfficers, GuildRank.Officer).Refusal);
        }

        [Test]
        public void TransferLeadership_OldLeaderBecomesVice()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.TransferLeadership(guild, "V", "O").Refusal);
            Assert.AreEqual(GuildRefusal.InvalidRank, GuildRules.TransferLeadership(guild, "L", "R").Refusal);
            Assert.AreEqual(GuildRefusal.CannotTargetSelf, GuildRules.TransferLeadership(guild, "L", "L").Refusal);
            Assert.AreEqual(GuildRefusal.TargetNotMember, GuildRules.TransferLeadership(guild, "L", "zz").Refusal);

            GuildResult result = GuildRules.TransferLeadership(guild, "L", "M");
            Assert.IsTrue(result.Ok);
            Assert.AreEqual("M", result.LeaderId);
            Assert.AreEqual(GuildRank.Leader, guild.Find("M").Rank);
            Assert.AreEqual(GuildRank.Vice, guild.Find("L").Rank);
            Assert.AreEqual(1, guild.CountOfRank(GuildRank.Leader));

            // Vices full (V and L): passing again to O leaves M as Officer.
            Assert.IsTrue(GuildRules.TransferLeadership(guild, "M", "O").Ok);
            Assert.AreEqual(GuildRank.Officer, guild.Find("M").Rank);
        }

        [Test]
        public void InactiveLeader_LosesTheGuildToTheBestActiveMember()
        {
            GuildState guild = FullRanks(now: 0d);
            double later = GuildRules.LeaderInactiveDays * Day;
            GuildRules.Touch(guild, "O", later - 1d);
            GuildRules.Touch(guild, "R", later - 1d);

            // Not yet 30 days.
            Assert.AreEqual("L", GuildRules.CheckLeaderInactivity(guild, later - 1d).LeaderId);

            // V is inactive too; O is the best active non-Recruit.
            GuildResult result = GuildRules.CheckLeaderInactivity(guild, later);
            Assert.AreEqual("O", result.LeaderId);
            Assert.AreEqual(GuildRank.Leader, guild.Find("O").Rank);
            Assert.AreEqual(GuildRank.Member, guild.Find("L").Rank);

            // Nobody active but a Recruit: nothing changes.
            GuildState lonely = NewGuild("x", now: 0d);
            Join(lonely, "r", 0d);
            GuildRules.Touch(lonely, "r", later);
            Assert.AreEqual("x", GuildRules.CheckLeaderInactivity(lonely, later + 1d).LeaderId);

            // A leader who logs in keeps the guild.
            GuildState active = NewGuild("y", now: 0d);
            Join(active, "z", 0d);
            SetRank(active, "z", GuildRank.Vice);
            GuildRules.Touch(active, "z", later);
            GuildRules.Touch(active, "y", later - Day);
            Assert.AreEqual("y", GuildRules.CheckLeaderInactivity(active, later + 1d).LeaderId);
        }

        [Test]
        public void Disband_NeedsLeaderEmptyBankAndNoWar()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildRules.Disband(guild, "V", false).Refusal);
            Assert.IsTrue(GuildBankRules.DepositGold(guild, "M", 10, 2d).Ok);
            Assert.AreEqual(GuildRefusal.BankNotEmpty, GuildRules.Disband(guild, "L", false).Refusal);
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "L", 10, 3d).Ok);
            Assert.AreEqual(GuildRefusal.AtWar, GuildRules.Disband(guild, "L", true).Refusal);
            GuildResult done = GuildRules.Disband(guild, "L", false);
            Assert.IsTrue(done.Ok);
            Assert.IsTrue(done.GuildDisbanded);
            Assert.AreEqual(0, guild.Count);
            Assert.IsFalse(GuildRules.AreInSameGuild(guild, "V", "O"));
        }

        // ------------------------------------------------------------------ bank

        [Test]
        public void Bank_DepositsAndWithdrawalsAreLogged()
        {
            GuildState guild = FullRanks();
            GuildResult deposit = GuildBankRules.DepositGold(guild, "R", 500, 10d);
            Assert.IsTrue(deposit.Ok);
            Assert.AreEqual(500, guild.BankGold);
            Assert.AreEqual(GuildBankAction.DepositGold, deposit.LogEntry.Action);
            Assert.AreEqual("R", deposit.LogEntry.PlayerId);
            Assert.AreEqual(GuildRank.Recruit, deposit.LogEntry.RankAtTime);
            Assert.AreEqual(500, deposit.LogEntry.BalanceAfter);

            Assert.AreEqual(GuildRefusal.NoPermission, GuildBankRules.WithdrawGold(guild, "R", 1, 11d).Refusal);
            GuildResult take = GuildBankRules.WithdrawGold(guild, "M", 200, 12d);
            Assert.IsTrue(take.Ok);
            Assert.AreEqual(300, take.LogEntry.BalanceAfter);
            Assert.AreEqual(GuildRefusal.NotEnoughInBank, GuildBankRules.WithdrawGold(guild, "V", 301, 13d).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidAmount, GuildBankRules.DepositGold(guild, "M", 0, 13d).Refusal);
            Assert.AreEqual(GuildRefusal.InvalidAmount, GuildBankRules.WithdrawGold(guild, "M", -5, 13d).Refusal);
            Assert.AreEqual(GuildRefusal.NotMember, GuildBankRules.DepositGold(guild, "zz", 5, 13d).Refusal);

            Assert.IsTrue(GuildBankRules.DepositItem(guild, "M", "pocao", 10, true, 14d).Ok);
            GuildResult item = GuildBankRules.WithdrawItem(guild, "O", "pocao", 4, 15d);
            Assert.AreEqual("pocao", item.LogEntry.ItemId);
            Assert.AreEqual(6, item.LogEntry.BalanceAfter);
            Assert.AreEqual(GuildRefusal.ItemNotTradable, GuildBankRules.DepositItem(guild, "M", "reliquia", 1, false, 16d).Refusal);
            Assert.AreEqual(GuildRefusal.NotEnoughInBank, GuildBankRules.WithdrawItem(guild, "O", "pocao", 7, 16d).Refusal);
            Assert.IsTrue(GuildBankRules.WithdrawItem(guild, "L", "pocao", 6, 16d).Ok);
            Assert.AreEqual(0, guild.BankItems.Count);

            Assert.AreEqual(new[] { GuildBankAction.DepositGold, GuildBankAction.WithdrawGold, GuildBankAction.DepositItem, GuildBankAction.WithdrawItem, GuildBankAction.WithdrawItem },
                guild.BankLog.Select(e => e.Action).ToArray());
        }

        [Test]
        public void Bank_DailyLimitPerRankResetsNextDay()
        {
            GuildState guild = FullRanks();
            Assert.IsTrue(GuildBankRules.DepositGold(guild, "L", 100000, 1d).Ok);
            Assert.AreEqual(1000, GuildBankRules.GoldLeftToday(guild.Find("M"), 2d));
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "M", 600, 2d).Ok);
            Assert.AreEqual(GuildRefusal.DailyLimitReached, GuildBankRules.WithdrawGold(guild, "M", 401, 3d).Refusal);
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "M", 400, 3d).Ok);
            Assert.AreEqual(GuildRefusal.DailyLimitReached, GuildBankRules.WithdrawGold(guild, "M", 1, 4d).Refusal);
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "M", 1000, Day + 1d).Ok);

            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "O", 5000, 5d).Ok);
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "V", 20000, 5d).Ok);
            Assert.AreEqual(GuildRefusal.DailyLimitReached, GuildBankRules.WithdrawGold(guild, "V", 1, 5d).Refusal);
            Assert.IsTrue(GuildBankRules.WithdrawGold(guild, "L", 50000, 5d).Ok);
            Assert.AreEqual(long.MaxValue, GuildBankRules.GoldLeftToday(guild.Find("L"), 5d));

            Assert.IsTrue(GuildBankRules.DepositItem(guild, "L", "flecha", 500, true, 6d).Ok);
            Assert.IsTrue(GuildBankRules.WithdrawItem(guild, "M", "flecha", GuildBankRules.DailyItemLimit(GuildRank.Member), 6d).Ok);
            Assert.AreEqual(GuildRefusal.DailyLimitReached, GuildBankRules.WithdrawItem(guild, "M", "flecha", 1, 6d).Refusal);
            Assert.AreEqual(0, GuildBankRules.ItemsLeftToday(guild.Find("R"), 6d));

            Assert.AreEqual(0, GuildBankRules.DailyGoldLimit(GuildRank.Recruit));
            Assert.Less(GuildBankRules.DailyGoldLimit(GuildRank.Member), GuildBankRules.DailyGoldLimit(GuildRank.Officer));
            Assert.Less(GuildBankRules.DailyGoldLimit(GuildRank.Officer), GuildBankRules.DailyGoldLimit(GuildRank.Vice));
        }

        [Test]
        public void Bank_SlotsAndGoldCap()
        {
            GuildState guild = NewGuild();
            int slots = GuildLevelRules.BankItemSlots(1);
            for (int i = 0; i < slots; i++)
            {
                Assert.IsTrue(GuildBankRules.DepositItem(guild, "L", "item" + i, 1, true, 1d).Ok);
            }

            Assert.AreEqual(GuildRefusal.BankFull, GuildBankRules.DepositItem(guild, "L", "novo", 1, true, 1d).Refusal);
            Assert.IsTrue(GuildBankRules.DepositItem(guild, "L", "item0", 5, true, 1d).Ok); // same stack
            Assert.AreEqual(6, guild.ItemCount("item0"));
            Assert.AreEqual(GuildRefusal.InvalidAmount, GuildBankRules.DepositItem(guild, "L", "", 1, true, 1d).Refusal);

            guild.BankGold = GuildBankRules.MaxBankGold - 5;
            Assert.AreEqual(GuildRefusal.BankFull, GuildBankRules.DepositGold(guild, "L", 6, 1d).Refusal);
            Assert.IsTrue(GuildBankRules.DepositGold(guild, "L", 5, 1d).Ok);
        }

        [Test]
        public void Bank_LogInMemoryIsCapped()
        {
            GuildState guild = NewGuild();
            for (int i = 0; i < GuildState.BankLogInMemory + 10; i++)
            {
                GuildBankRules.DepositGold(guild, "L", 1, i);
            }

            Assert.AreEqual(GuildState.BankLogInMemory, guild.BankLog.Count);
            Assert.AreEqual(10d, guild.BankLog[0].Time);
        }

        // ------------------------------------------------------------------ contribution and level

        [Test]
        public void Contribute_GivesPointsUpToTheDailyCap()
        {
            GuildState guild = FullRanks();
            GuildResult result = GuildBankRules.Contribute(guild, "R", 1000, 10d, out long points);
            Assert.IsTrue(result.Ok);
            Assert.AreEqual(100, points);
            Assert.AreEqual(GuildBankAction.ContributeGold, result.LogEntry.Action);
            Assert.AreEqual(GuildBankAction.ContributeGold, guild.BankLog.Last().Action);
            Assert.AreEqual(1, guild.BankLog.Count);
            Assert.AreEqual(1000, guild.BankGold);

            GuildBankRules.Contribute(guild, "R", 5000, 11d, out points);
            Assert.AreEqual(GuildBankRules.MaxGoldPointsPerDay - 100, points);
            Assert.AreEqual(6000, guild.BankGold); // the gold still goes in
            GuildBankRules.Contribute(guild, "R", 500, 12d, out points);
            Assert.AreEqual(0, points);
            GuildBankRules.Contribute(guild, "R", 500, Day + 1d, out points);
            Assert.AreEqual(50, points);

            Assert.AreEqual(GuildBankRules.MaxGoldPointsPerDay + 50, guild.Find("R").ContributionPoints);
            Assert.AreEqual(guild.Find("R").ContributionPoints, guild.TotalPoints);

            Assert.AreEqual(GuildRefusal.InvalidAmount, GuildBankRules.Contribute(guild, "R", 0, 13d, out points).Refusal);
            Assert.AreEqual(0, points);

            // Plain deposits never count.
            long before = guild.TotalPoints;
            GuildBankRules.DepositGold(guild, "L", 10000, 14d);
            Assert.AreEqual(before, guild.TotalPoints);

            Assert.IsTrue(GuildBankRules.AddContributionPoints(guild, "M", 700).Ok);
            Assert.AreEqual(before + 700, guild.TotalPoints);
            Assert.AreEqual(GuildRefusal.InvalidAmount, GuildBankRules.AddContributionPoints(guild, "M", 0).Refusal);
            Assert.AreEqual(GuildRefusal.NotMember, GuildBankRules.AddContributionPoints(guild, "zz", 5).Refusal);

            // A member who leaves keeps the guild's points.
            GuildRules.Leave(guild, "M");
            Assert.AreEqual(before + 700, guild.TotalPoints);
        }

        [Test]
        public void Level_ThresholdsAndSmallPerks()
        {
            Assert.AreEqual(1, GuildLevelRules.LevelFor(0));
            Assert.AreEqual(1, GuildLevelRules.LevelFor(999));
            Assert.AreEqual(2, GuildLevelRules.LevelFor(1000));
            Assert.AreEqual(10, GuildLevelRules.LevelFor(200000));
            Assert.AreEqual(10, GuildLevelRules.LevelFor(long.MaxValue));
            Assert.AreEqual(1, GuildLevelRules.LevelFor(-5));
            Assert.AreEqual(2000, GuildLevelRules.PointsToNextLevel(1000));
            Assert.AreEqual(0, GuildLevelRules.PointsToNextLevel(500000));

            for (int level = 1; level <= GuildLevelRules.MaxLevel; level++)
            {
                Assert.AreEqual(level, GuildLevelRules.LevelFor(GuildLevelRules.PointsFor(level)), level.ToString());
                Assert.LessOrEqual(GuildLevelRules.XpBonusPercent(level), 3);
                Assert.LessOrEqual(GuildLevelRules.GoldBonusPercent(level), 2);
                if (level > 1)
                {
                    Assert.GreaterOrEqual(GuildLevelRules.XpBonusPercent(level), GuildLevelRules.XpBonusPercent(level - 1));
                    Assert.Greater(GuildLevelRules.MaxMembers(level), GuildLevelRules.MaxMembers(level - 1));
                }
            }

            Assert.AreEqual(0, GuildLevelRules.XpBonusPercent(1));
            Assert.AreEqual(3, GuildLevelRules.XpBonusPercent(10));
            Assert.AreEqual(2, GuildLevelRules.GoldBonusPercent(10));
            Assert.AreEqual(65, GuildLevelRules.MaxMembers(10));
            Assert.AreEqual(1030, GuildLevelRules.ApplyXpBonus(1000, 10));
            Assert.AreEqual(1000, GuildLevelRules.ApplyXpBonus(1000, 1));

            GuildState guild = NewGuild();
            guild.TotalPoints = 15000;
            Assert.AreEqual(5, guild.Level);
        }

        // ------------------------------------------------------------------ board and message of the day

        [Test]
        public void Board_OfficersPostAndOldestDrops()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildBoardRules.PostNotice(guild, "M", "Oi", 1d).Refusal);
            Assert.AreEqual(GuildRefusal.TextEmpty, GuildBoardRules.PostNotice(guild, "O", "   ", 1d).Refusal);
            Assert.AreEqual(GuildRefusal.TextTooLong, GuildBoardRules.PostNotice(guild, "O", new string('a', GuildBoardRules.MaxNoticeLength + 1), 1d).Refusal);
            Assert.AreEqual(GuildRefusal.TextOffensive, GuildBoardRules.PostNotice(guild, "O", "Bando de c4ralho", 1d).Refusal);

            for (int i = 0; i < GuildBoardRules.MaxNotices + 2; i++)
            {
                Assert.IsTrue(GuildBoardRules.PostNotice(guild, "O", "Aviso " + i, i).Ok);
            }

            Assert.AreEqual(GuildBoardRules.MaxNotices, guild.Notices.Count);
            Assert.AreEqual("Aviso 2", guild.Notices[0].Text);
            long id = guild.Notices[0].Id;
            Assert.AreEqual(GuildRefusal.NoPermission, GuildBoardRules.RemoveNotice(guild, "M", id).Refusal);
            Assert.IsTrue(GuildBoardRules.RemoveNotice(guild, "V", id).Ok);
            Assert.AreEqual(GuildRefusal.NoticeNotFound, GuildBoardRules.RemoveNotice(guild, "V", id).Refusal);
        }

        [Test]
        public void MessageOfTheDay_VicesAndUp()
        {
            GuildState guild = FullRanks();
            Assert.AreEqual(GuildRefusal.NoPermission, GuildBoardRules.SetMessageOfTheDay(guild, "O", "Oi").Refusal);
            Assert.IsTrue(GuildBoardRules.SetMessageOfTheDay(guild, "V", " Caçada às 20h ").Ok);
            Assert.AreEqual("Caçada às 20h", guild.MessageOfTheDay);
            Assert.AreEqual(GuildRefusal.TextTooLong,
                GuildBoardRules.SetMessageOfTheDay(guild, "V", new string('a', GuildBoardRules.MaxMotdLength + 1)).Refusal);
            Assert.AreEqual(GuildRefusal.TextOffensive, GuildBoardRules.SetMessageOfTheDay(guild, "L", "seus fdp").Refusal);
            Assert.IsTrue(GuildBoardRules.SetMessageOfTheDay(guild, "L", "").Ok);
            Assert.AreEqual("", guild.MessageOfTheDay);
        }

        // ------------------------------------------------------------------ war

        private static GuildState WarGuild(string id, string prefix)
        {
            GuildState guild = NewGuild(prefix + "L", "Guilda " + id, 0d, id);
            for (int i = 1; i < GuildWarRules.MinMembers; i++)
            {
                Join(guild, prefix + i, 0d);
            }

            SetRank(guild, prefix + "1", GuildRank.Vice);
            return guild;
        }

        private static GuildWar ActiveWar(GuildState a, GuildState b, int scoreTarget = 0, double hours = 2d)
        {
            GuildWarResult declared = GuildWarRules.Declare("w1", a, a.LeaderId, b, null, hours, scoreTarget, 0d);
            Assert.IsTrue(declared.Ok, declared.Refusal.ToString());
            Assert.IsTrue(GuildWarRules.Accept(declared.War, b, b.LeaderId, 100d).Ok);
            GuildWarRules.Tick(declared.War, 100d + GuildWarRules.WarmupSeconds);
            Assert.AreEqual(GuildWarState.Active, declared.War.State);
            return declared.War;
        }

        [Test]
        public void War_DeclareAcceptWarmupActiveTimeUp()
        {
            GuildState a = WarGuild("A", "a");
            GuildState b = WarGuild("B", "b");
            GuildWarResult declared = GuildWarRules.Declare("w1", a, "a1", b, null, 2d, 0, 0d);
            Assert.IsTrue(declared.Ok);
            GuildWar war = declared.War;
            Assert.AreEqual(GuildWarState.Declared, war.State);
            Assert.IsFalse(GuildWarRules.IsFighting(war, 10d));

            Assert.AreEqual(GuildRefusal.WarNotParticipant, GuildWarRules.Accept(war, a, "aL", 50d).Refusal);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildWarRules.Accept(war, b, "b2", 50d).Refusal);
            Assert.IsTrue(GuildWarRules.Accept(war, b, "b1", 50d).Ok);
            Assert.AreEqual(GuildWarState.Accepted, war.State);
            Assert.AreEqual(50d + GuildWarRules.WarmupSeconds, war.StartsAt);
            Assert.IsFalse(GuildWarRules.IsFighting(war, war.StartsAt - 1d));
            Assert.IsTrue(GuildWarRules.IsFighting(war, war.StartsAt)); // pure query, no tick needed
            Assert.AreEqual(GuildRefusal.WarWrongState, GuildWarRules.Accept(war, b, "b1", 60d).Refusal);

            GuildWarRules.Tick(war, war.StartsAt);
            Assert.AreEqual(GuildWarState.Active, war.State);
            Assert.IsTrue(GuildWarRules.RecordKill(war, "A", "a2", "B", "b2", war.StartsAt + 5d));
            Assert.IsTrue(GuildWarRules.RecordKill(war, "A", "a3", "B", "b2", war.StartsAt + 6d));
            Assert.IsTrue(GuildWarRules.RecordKill(war, "B", "b3", "A", "a3", war.StartsAt + 7d));

            GuildWarRules.Tick(war, war.EndsAt);
            Assert.AreEqual(GuildWarState.Ended, war.State);
            Assert.AreEqual(GuildWarEndReason.TimeUp, war.EndReason);
            Assert.AreEqual("A", war.WinnerGuildId);
            Assert.AreEqual(2, war.AttackerScore);
            Assert.AreEqual(1, war.DefenderScore);
            Assert.IsFalse(GuildWarRules.RecordKill(war, "A", "a2", "B", "b3", war.EndsAt + 1d));
        }

        [Test]
        public void War_DeclareRefusals()
        {
            GuildState a = WarGuild("A", "a");
            GuildState b = WarGuild("B", "b");
            GuildState small = NewGuild("sL", "Pequena", 0d, "S");
            Assert.AreEqual(GuildRefusal.NoPermission, GuildWarRules.Declare("w", a, "a2", b, null, 2d, 0, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.WarSameGuild, GuildWarRules.Declare("w", a, "aL", a, null, 2d, 0, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.WarInvalidTerms, GuildWarRules.Declare("w", a, "aL", b, null, 0.5d, 0, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.WarInvalidTerms, GuildWarRules.Declare("w", a, "aL", b, null, 73d, 0, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.WarInvalidTerms, GuildWarRules.Declare("w", a, "aL", b, null, 2d, 501, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.WarTooFewMembers, GuildWarRules.Declare("w", a, "aL", small, null, 2d, 0, 0d).Refusal);
            Assert.AreEqual(GuildRefusal.Disbanded, GuildWarRules.Declare("w", a, "aL", null, null, 2d, 0, 0d).Refusal);

            GuildWar open = GuildWarRules.Declare("w1", a, "aL", b, null, 2d, 0, 0d).War;
            var wars = new List<GuildWar> { open };
            Assert.AreEqual(GuildRefusal.WarAlreadyExists, GuildWarRules.Declare("w2", b, "bL", a, wars, 2d, 0, 10d).Refusal);

            // Unanswered: it expires, then the pair waits the cooldown.
            double expiry = GuildWarRules.DeclarationExpirySeconds;
            Assert.AreEqual(GuildRefusal.WarCooldown, GuildWarRules.Declare("w2", b, "bL", a, wars, 2d, 0, expiry + 1d).Refusal);
            Assert.AreEqual(GuildWarState.Expired, open.State);
            Assert.AreEqual(GuildRefusal.InviteExpired, GuildWarRules.Accept(open, b, "bL", expiry + 2d).Refusal);
            Assert.IsTrue(GuildWarRules.Declare("w2", b, "bL", a, wars, 2d, 0, expiry + GuildWarRules.RedeclareCooldownSeconds).Ok);

            // At most 3 open wars per guild.
            var many = new List<GuildWar>();
            for (int i = 0; i < GuildWarRules.MaxOpenWars; i++)
            {
                GuildState other = WarGuild("X" + i, "x" + i);
                many.Add(GuildWarRules.Declare("m" + i, a, "aL", other, many, 2d, 0, 0d).War);
                Assert.IsNotNull(many.Last());
            }

            Assert.AreEqual(GuildRefusal.WarTooManyWars, GuildWarRules.Declare("m9", a, "aL", b, many, 2d, 0, 0d).Refusal);
        }

        [Test]
        public void War_DeclineCancelSurrender()
        {
            GuildState a = WarGuild("A", "a");
            GuildState b = WarGuild("B", "b");

            GuildWar w1 = GuildWarRules.Declare("w1", a, "aL", b, null, 2d, 0, 0d).War;
            Assert.IsTrue(GuildWarRules.Decline(w1, b, "bL", 5d).Ok);
            Assert.AreEqual(GuildWarState.Declined, w1.State);
            Assert.AreEqual(5d, w1.ClosedAt);

            GuildWar w2 = GuildWarRules.Declare("w2", a, "aL", b, null, 2d, 0, 0d).War;
            Assert.AreEqual(GuildRefusal.WarNotParticipant, GuildWarRules.Cancel(w2, b, "bL", 1d).Refusal);
            Assert.IsTrue(GuildWarRules.Cancel(w2, a, "a1", 1d).Ok);
            Assert.AreEqual(GuildWarState.Cancelled, w2.State);
            Assert.AreEqual(GuildRefusal.WarWrongState, GuildWarRules.Surrender(w2, a, "aL", 2d).Refusal);

            GuildWar w3 = ActiveWar(a, b);
            GuildWarRules.RecordKill(w3, "A", "aL", "B", "b2", 800d);
            Assert.AreEqual(GuildRefusal.NoPermission, GuildWarRules.Surrender(w3, a, "a3", 900d).Refusal);
            Assert.IsTrue(GuildWarRules.Surrender(w3, a, "aL", 900d).Ok);
            Assert.AreEqual(GuildWarEndReason.Surrender, w3.EndReason);
            Assert.AreEqual("B", w3.WinnerGuildId);
            GuildState c = WarGuild("C", "c");
            Assert.AreEqual(GuildRefusal.WarNotParticipant, GuildWarRules.Surrender(w3, c, "cL", 901d).Refusal);
        }

        [Test]
        public void War_ScoreTargetEndsItAndRepeatKillsDoNotFarm()
        {
            GuildState a = WarGuild("A", "a");
            GuildState b = WarGuild("B", "b");
            GuildWar war = ActiveWar(a, b, scoreTarget: 3);
            double t = war.StartsAt + 1d;

            Assert.IsTrue(GuildWarRules.RecordKill(war, "B", "b1", "A", "a1", t));
            Assert.IsFalse(GuildWarRules.RecordKill(war, "B", "b1", "A", "a1", t + GuildWarRules.RepeatKillSeconds - 1d));
            Assert.IsFalse(GuildWarRules.RecordKill(war, "B", "b1", "B", "b2", t)); // same side
            Assert.IsFalse(GuildWarRules.RecordKill(war, "B", "b1", "Z", "z1", t)); // not in the war
            Assert.IsFalse(GuildWarRules.RecordKill(war, "B", "b1", "A", "b1", t)); // self
            Assert.AreEqual(1, war.DefenderScore);
            Assert.IsTrue(GuildWarRules.RecordKill(war, "B", "b1", "A", "a1", t + GuildWarRules.RepeatKillSeconds));
            Assert.IsTrue(GuildWarRules.RecordKill(war, "B", "b2", "A", "a1", t + GuildWarRules.RepeatKillSeconds + 1d));

            Assert.AreEqual(GuildWarState.Ended, war.State);
            Assert.AreEqual(GuildWarEndReason.ScoreReached, war.EndReason);
            Assert.AreEqual("B", war.WinnerGuildId);
            Assert.AreEqual(3, war.DefenderScore);
        }

        [Test]
        public void War_DrawHasNoWinner()
        {
            GuildWar war = ActiveWar(WarGuild("A", "a"), WarGuild("B", "b"));
            GuildWarRules.Tick(war, war.EndsAt + 1d);
            Assert.AreEqual(GuildWarState.Ended, war.State);
            Assert.IsNull(war.WinnerGuildId);
            Assert.AreEqual(war.EndsAt, war.ClosedAt);
        }

        [Test]
        public void CanHarm_OnlyAtWarOutsideCitiesAndNeverTheParty()
        {
            GuildState a = WarGuild("A", "a");
            GuildState b = WarGuild("B", "b");
            GuildWar war = ActiveWar(a, b);
            var wars = new List<GuildWar> { war };
            double t = war.StartsAt + 10d;

            Assert.IsTrue(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, t));
            Assert.IsTrue(GuildWarRules.CanHarm(wars, "B", ZoneKind.Wilds, "A", ZoneKind.Wilds, false, t));

            // Never in a city, on either end.
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.City, "B", ZoneKind.Wilds, false, t));
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.City, false, t));

            // A member of a warring guild in the same party is still an ally.
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, true, t));

            // Guild-mates, outsiders, guildless and other times: no.
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "A", ZoneKind.Wilds, false, t));
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "C", ZoneKind.Wilds, false, t));
            Assert.IsFalse(GuildWarRules.CanHarm(wars, null, ZoneKind.Wilds, "B", ZoneKind.Wilds, false, t));
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, war.StartsAt - 1d));
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, war.EndsAt));
            // The starting regions never host a war's blows; the frontier does.
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, t, region: Rapadura.Gameplay.World.Map.MapRegion.Zarafshan));
            Assert.IsTrue(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, t, region: Rapadura.Gameplay.World.Map.MapRegion.CostaDasMoncoes));
            Assert.IsFalse(GuildWarRules.CanHarm(null, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, false, t));

            // Other consent (duel / PvP area) passes through unchanged; the party and the city still win.
            Assert.IsTrue(GuildWarRules.CanHarm(null, "A", ZoneKind.Wilds, "A", ZoneKind.Wilds, false, t, pvpAllowed: true));
            Assert.IsFalse(GuildWarRules.CanHarm(null, "A", ZoneKind.City, "A", ZoneKind.Wilds, false, t, pvpAllowed: true));

            // Matches PartyCombatRules with the real party.
            PartyState party = new PartyState("a2", null);
            Assert.IsTrue(PartyRules.Invite(party, "a2", "b2", null, false, 0d).Ok);
            Assert.IsTrue(PartyRules.Accept(party, "b2", 1d).Ok);
            Assert.IsFalse(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, PartyRules.AreInSameParty(party, "a2", "b2"), t));
            Assert.IsTrue(GuildWarRules.CanHarm(wars, "A", ZoneKind.Wilds, "B", ZoneKind.Wilds, PartyRules.AreInSameParty(party, "a2", "b3"), t));
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_CoverEveryEnum()
        {
            var keys = new List<string>();
            keys.AddRange(((GuildRefusal[])Enum.GetValues(typeof(GuildRefusal))).Select(GuildTexts.RefusalKey));
            keys.AddRange(((GuildRank[])Enum.GetValues(typeof(GuildRank))).Select(GuildTexts.RankKey));
            keys.AddRange(GuildTexts.SinglePermissions.Select(GuildTexts.PermissionKey));
            keys.AddRange(((GuildWarState[])Enum.GetValues(typeof(GuildWarState))).Select(GuildTexts.WarStateKey));
            keys.AddRange(((GuildWarEndReason[])Enum.GetValues(typeof(GuildWarEndReason))).Select(GuildTexts.WarEndKey));
            keys.AddRange(((GuildBankAction[])Enum.GetValues(typeof(GuildBankAction))).Select(GuildTexts.BankActionKey));
            keys.AddRange(((GuildEmblemShape[])Enum.GetValues(typeof(GuildEmblemShape))).Select(GuildTexts.ShapeKey));
            keys.AddRange(((GuildEmblemColor[])Enum.GetValues(typeof(GuildEmblemColor))).Select(GuildTexts.ColorKey));
            keys.AddRange(((GuildEmblemSymbol[])Enum.GetValues(typeof(GuildEmblemSymbol))).Select(GuildTexts.SymbolKey));
            keys.AddRange(new[]
            {
                GuildTexts.InviteReceived, GuildTexts.MemberJoined, GuildTexts.MemberLeft, GuildTexts.MemberKicked, GuildTexts.RankChanged,
                GuildTexts.NewLeader, GuildTexts.LeaderInactiveHandover, GuildTexts.LevelUp, GuildTexts.WarDeclared, GuildTexts.WarStartsSoon,
                GuildTexts.WarScore, GuildTexts.WarDraw, GuildTexts.WarWinner, GuildTexts.MessageOfTheDayTitle,
            });

            Assert.AreEqual(keys.Count, keys.Distinct().Count());
            Assert.AreEqual(keys.Count, GuildTexts.Entries.Count);
            foreach (string key in keys)
            {
                Assert.IsFalse(string.IsNullOrEmpty(GuildTexts.GetFallback(key)), key);
                Assert.IsFalse(string.IsNullOrEmpty(GuildTexts.Entries.Single(e => e.Key == key).En), key);
            }

            // Every single permission is listed (and only those).
            int all = GuildTexts.SinglePermissions.Aggregate(0, (acc, p) => acc | (int)p);
            Assert.AreEqual((int)GuildPermissions.Of(GuildRank.Leader), all);

            Assert.AreEqual("guild.refusal.already_in_guild", GuildTexts.RefusalKey(GuildRefusal.AlreadyInGuild));
            Assert.AreEqual("Vice-líder", GuildTexts.Rank(null, GuildRank.Vice));
            Assert.AreEqual("Seu cargo não permite isso.", GuildTexts.Refusal(null, GuildRefusal.NoPermission));
            Assert.AreEqual("Ana entrou na guilda.", GuildTexts.Format(null, GuildTexts.MemberJoined, "Ana"));
            Assert.AreEqual("Barca de Luz", GuildTexts.Get(null, GuildTexts.SymbolKey(GuildEmblemSymbol.LightShip)));
        }
    }
}
