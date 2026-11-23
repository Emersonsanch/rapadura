namespace Rapadura.Gameplay.Guild
{
    /// <summary>
    /// The guild bank ("banco / baú da guilda"): gold and item stacks, deposits by anyone, withdrawals with a daily limit
    /// per rank, and gold given as contribution. Every movement returns its <see cref="GuildBankLogEntry"/> (pure data the
    /// server appends to its persistent log: who took what, when, at which rank, the balance after) and keeps the last
    /// <see cref="GuildState.BankLogInMemory"/> in the guild. The server moves the gold / items in the player's own
    /// inventory only after <see cref="GuildResult.Ok"/>, in the same transaction.
    /// <code>
    /// Daily withdrawal  Leader  Vice    Officer  Member  Recruit
    /// Gold              free    20000   5000     1000    0
    /// Item units        free    200     50       20      0
    /// </code>
    /// The leader is unlimited but logged like everyone. Limits reset on the server's UTC day (<see cref="GuildRules.DayIndex"/>).
    /// </summary>
    public static class GuildBankRules
    {
        public const long MaxBankGold = 1_000_000_000L;

        /// <summary>Gold given for one contribution point.</summary>
        public const long GoldPerPoint = 10;

        /// <summary>Contribution points one member can earn from gold per day (= 2000 gold); more gold still goes in, without points.</summary>
        public const long MaxGoldPointsPerDay = 200;

        public static long DailyGoldLimit(GuildRank rank)
        {
            switch (rank)
            {
                case GuildRank.Leader: return long.MaxValue;
                case GuildRank.Vice: return 20000;
                case GuildRank.Officer: return 5000;
                case GuildRank.Member: return 1000;
                default: return 0;
            }
        }

        public static int DailyItemLimit(GuildRank rank)
        {
            switch (rank)
            {
                case GuildRank.Leader: return int.MaxValue;
                case GuildRank.Vice: return 200;
                case GuildRank.Officer: return 50;
                case GuildRank.Member: return 20;
                default: return 0;
            }
        }

        /// <summary>Gold the member can still take today.</summary>
        public static long GoldLeftToday(GuildMember member, double now)
        {
            if (member == null)
            {
                return 0;
            }

            member.RollDay(GuildRules.DayIndex(now));
            long limit = DailyGoldLimit(member.Rank);
            return limit == long.MaxValue ? long.MaxValue : System.Math.Max(0, limit - member.GoldWithdrawnToday);
        }

        public static int ItemsLeftToday(GuildMember member, double now)
        {
            if (member == null)
            {
                return 0;
            }

            member.RollDay(GuildRules.DayIndex(now));
            int limit = DailyItemLimit(member.Rank);
            return limit == int.MaxValue ? int.MaxValue : System.Math.Max(0, limit - member.ItemsWithdrawnToday);
        }

        public static GuildResult DepositGold(GuildState guild, string playerId, long gold, double now)
        {
            GuildRefusal check = CheckMove(guild, playerId, GuildPermission.BankDeposit, gold, out GuildMember member);
            if (check == GuildRefusal.None && gold > MaxBankGold - guild.BankGold)
            {
                check = GuildRefusal.BankFull;
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            guild.BankGold += gold;
            return Log(guild, member, GuildBankAction.DepositGold, null, gold, guild.BankGold, now);
        }

        public static GuildResult WithdrawGold(GuildState guild, string playerId, long gold, double now)
        {
            GuildRefusal check = CheckMove(guild, playerId, GuildPermission.BankWithdraw, gold, out GuildMember member);
            if (check == GuildRefusal.None && gold > guild.BankGold)
            {
                check = GuildRefusal.NotEnoughInBank;
            }

            if (check == GuildRefusal.None && gold > GoldLeftToday(member, now))
            {
                check = GuildRefusal.DailyLimitReached;
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            guild.BankGold -= gold;
            member.GoldWithdrawnToday += gold;
            return Log(guild, member, GuildBankAction.WithdrawGold, null, gold, guild.BankGold, now);
        }

        /// <param name="itemTradable">False for bound items (story rewards, family relics, achievements): they never go in.</param>
        public static GuildResult DepositItem(GuildState guild, string playerId, string itemId, int count, bool itemTradable, double now)
        {
            GuildRefusal check = CheckMove(guild, playerId, GuildPermission.BankDeposit, count, out GuildMember member);
            if (check == GuildRefusal.None && string.IsNullOrEmpty(itemId))
            {
                check = GuildRefusal.InvalidAmount;
            }

            if (check == GuildRefusal.None && !itemTradable)
            {
                check = GuildRefusal.ItemNotTradable;
            }

            long have = check == GuildRefusal.None ? guild.ItemCount(itemId) : 0;
            if (check == GuildRefusal.None && have == 0 && guild.BankItems.Count >= GuildLevelRules.BankItemSlots(guild.Level))
            {
                check = GuildRefusal.BankFull;
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            guild.SetItemCount(itemId, have + count);
            return Log(guild, member, GuildBankAction.DepositItem, itemId, count, have + count, now);
        }

        public static GuildResult WithdrawItem(GuildState guild, string playerId, string itemId, int count, double now)
        {
            GuildRefusal check = CheckMove(guild, playerId, GuildPermission.BankWithdraw, count, out GuildMember member);
            if (check == GuildRefusal.None && string.IsNullOrEmpty(itemId))
            {
                check = GuildRefusal.InvalidAmount;
            }

            long have = check == GuildRefusal.None ? guild.ItemCount(itemId) : 0;
            if (check == GuildRefusal.None && count > have)
            {
                check = GuildRefusal.NotEnoughInBank;
            }

            if (check == GuildRefusal.None && count > ItemsLeftToday(member, now))
            {
                check = GuildRefusal.DailyLimitReached;
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            guild.SetItemCount(itemId, have - count);
            member.ItemsWithdrawnToday += count;
            return Log(guild, member, GuildBankAction.WithdrawItem, itemId, count, have - count, now);
        }

        /// <summary>
        /// Gold given to the guild: it goes into the bank (logged as <see cref="GuildBankAction.ContributeGold"/>) and earns
        /// 1 point per <see cref="GoldPerPoint"/>, up to <see cref="MaxGoldPointsPerDay"/> per member per day. Unlike a plain
        /// deposit it counts toward the guild level. Recruits contribute too.
        /// </summary>
        public static GuildResult Contribute(GuildState guild, string playerId, long gold, double now, out long pointsGained)
        {
            pointsGained = 0;
            GuildResult deposit = DepositGold(guild, playerId, gold, now);
            if (!deposit.Ok)
            {
                return deposit;
            }

            GuildMember member = guild.Find(playerId);
            member.RollDay(GuildRules.DayIndex(now));
            pointsGained = System.Math.Min(gold / GoldPerPoint, System.Math.Max(0, MaxGoldPointsPerDay - member.GoldPointsToday));
            member.GoldPointsToday += pointsGained;
            member.ContributionPoints += pointsGained;
            guild.TotalPoints += pointsGained;

            // The deposit's log line becomes a contribution line.
            var entry = new GuildBankLogEntry(now, playerId, member.Rank, GuildBankAction.ContributeGold, null, gold, guild.BankGold);
            guild.ReplaceLastLog(entry);
            return GuildRules.Done(guild, entry);
        }

        /// <summary>Points the server awards from other sources (weekly guild missions, events). Not capped here: the source caps itself.</summary>
        public static GuildResult AddContributionPoints(GuildState guild, string playerId, long points)
        {
            GuildRefusal check = GuildRules.CheckActor(guild, playerId, GuildPermission.None, out GuildMember member);
            if (check == GuildRefusal.None && points <= 0)
            {
                check = GuildRefusal.InvalidAmount;
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            member.ContributionPoints += points;
            guild.TotalPoints += points;
            return GuildRules.Done(guild);
        }

        private static GuildRefusal CheckMove(GuildState guild, string playerId, GuildPermission permission, long amount, out GuildMember member)
        {
            GuildRefusal check = GuildRules.CheckActor(guild, playerId, permission, out member);
            if (check != GuildRefusal.None)
            {
                return check;
            }

            return amount <= 0 ? GuildRefusal.InvalidAmount : GuildRefusal.None;
        }

        private static GuildResult Log(GuildState guild, GuildMember member, GuildBankAction action, string itemId, long amount, long balanceAfter,
            double now)
        {
            var entry = new GuildBankLogEntry(now, member.PlayerId, member.Rank, action, itemId, amount, balanceAfter);
            guild.AppendLog(entry);
            return GuildRules.Done(guild, entry);
        }
    }
}
