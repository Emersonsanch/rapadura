namespace Rapadura.Gameplay.Guild
{
    /// <summary>
    /// The guild board ("mural de avisos") and the message of the day. Officers and up post and remove notices
    /// (<see cref="GuildPermission.EditBoard"/>); Vices and the Leader set the message of the day
    /// (<see cref="GuildPermission.EditMotd"/>). Every text goes through <see cref="GuildNameRules.ValidateText"/> (size
    /// and word filter); a full board drops its oldest notice, so posting never fails for lack of room.
    /// </summary>
    public static class GuildBoardRules
    {
        public const int MaxNoticeLength = 300;
        public const int MaxNotices = 20;
        public const int MaxMotdLength = 120;

        public static GuildResult PostNotice(GuildState guild, string authorId, string text, double now)
        {
            GuildRefusal check = GuildRules.CheckActor(guild, authorId, GuildPermission.EditBoard, out _);
            if (check == GuildRefusal.None)
            {
                check = GuildNameRules.ValidateText(text, MaxNoticeLength, allowEmpty: false);
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            while (guild.Notices.Count >= MaxNotices)
            {
                guild.RemoveNoticeAt(0);
            }

            guild.AddNotice(new GuildNotice(guild.NextNoticeId++, authorId, GuildNameRules.Clean(text), now));
            return GuildRules.Done(guild);
        }

        public static GuildResult RemoveNotice(GuildState guild, string actorId, long noticeId)
        {
            GuildRefusal check = GuildRules.CheckActor(guild, actorId, GuildPermission.EditBoard, out _);
            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            for (int i = 0; i < guild.Notices.Count; i++)
            {
                if (guild.Notices[i].Id == noticeId)
                {
                    guild.RemoveNoticeAt(i);
                    return GuildRules.Done(guild);
                }
            }

            return GuildRules.Refused(guild, GuildRefusal.NoticeNotFound);
        }

        /// <summary>Sets (or, with an empty text, clears) the message of the day.</summary>
        public static GuildResult SetMessageOfTheDay(GuildState guild, string actorId, string text)
        {
            GuildRefusal check = GuildRules.CheckActor(guild, actorId, GuildPermission.EditMotd, out _);
            if (check == GuildRefusal.None)
            {
                check = GuildNameRules.ValidateText(text, MaxMotdLength, allowEmpty: true);
            }

            if (check != GuildRefusal.None)
            {
                return GuildRules.Refused(guild, check);
            }

            guild.MessageOfTheDay = GuildNameRules.Clean(text);
            return GuildRules.Done(guild);
        }
    }
}
