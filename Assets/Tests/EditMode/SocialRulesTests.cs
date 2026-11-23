using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Guild;
using Rapadura.Gameplay.Party;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.Social;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.WorldMap;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure chat and moderation of TODO.md 12.3 (Gameplay/Social): channels and their recipients, token bucket,
    /// repetition and automatic mute, the word filter (no false positive on the game's own texts), emotes, the block list
    /// and its optional hooks in the party / guild / trade / duel rules, reports and the staff commands, and the texts.
    /// </summary>
    public class SocialRulesTests
    {
        private static List<ChatPresence> Online(params (string Id, string Region)[] players) =>
            players.Select(p => new ChatPresence(p.Id, p.Region)).ToList();

        private static ChatContext Ctx(string sender, IReadOnlyList<ChatPresence> online, string region = "r1", int level = 30,
            PartyState party = null, GuildState guild = null, string whisperTo = null, Func<string, PlayerBlockList> blocks = null) =>
            new ChatContext
            {
                SenderId = sender,
                SenderLevel = level,
                SenderRegionId = region,
                Online = online,
                Party = party,
                Guild = guild,
                WhisperTargetId = whisperTo,
                BlockListOf = blocks,
            };

        private static PartyState PartyOf(params string[] ids)
        {
            var party = new PartyState(ids[0], null);
            for (int i = 1; i < ids.Length; i++)
            {
                Assert.IsTrue(PartyRules.Invite(party, ids[0], ids[i], null, false, 0d).Ok);
                Assert.IsTrue(PartyRules.Accept(party, ids[i], 1d).Ok);
            }

            return party;
        }

        private static GuildState GuildOf(params string[] ids)
        {
            var guild = new GuildState("g1", "Coluna", "", default, ids[0], 0d);
            for (int i = 1; i < ids.Length; i++)
            {
                Assert.AreEqual(GuildRefusal.None, GuildRules.Invite(guild, ids[0], ids[i], false, -1d, 0d).Refusal);
                Assert.AreEqual(GuildRefusal.None, GuildRules.Accept(guild, ids[i], 1d).Refusal);
            }

            return guild;
        }

        // ------------------------------------------------------------------ channels and recipients

        [Test]
        public void General_GoesToTheSenderRegionOnly()
        {
            var online = Online(("a", "r1"), ("b", "r1"), ("c", "r2"));
            ChatSendResult result = ChatRules.TrySend(new ChatSpamState("a"), ChatChannel.General, "Oi, gente!", Ctx("a", online), 0d);

            Assert.IsTrue(result.Ok, result.Refusal.ToString());
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, result.Recipients);
            Assert.AreEqual("Oi, gente!", result.Message.text);
            Assert.AreEqual(ChatChannel.General, result.Message.channel);
        }

        [Test]
        public void General_NeedsLevelAndRegion()
        {
            var online = Online(("a", "r1"));
            Assert.AreEqual(ChatRefusal.LevelTooLow, ChatRules.TrySend(null, ChatChannel.General, "oi", Ctx("a", online, level: 4), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.None, ChatRules.TrySend(null, ChatChannel.General, "oi", Ctx("a", online, level: ChatRules.MinLevelForGeneral), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.NoRegion, ChatRules.TrySend(null, ChatChannel.General, "oi", Ctx("a", online, region: null), 0d).Refusal);
        }

        [Test]
        public void Party_GoesToOnlineMembersAndNeedsAParty()
        {
            PartyState party = PartyOf("a", "b", "c");
            var online = Online(("a", "r1"), ("b", "r9"), ("x", "r1"));
            ChatSendResult result = ChatRules.TrySend(null, ChatChannel.Party, "bora", Ctx("a", online, party: party), 0d);

            Assert.IsTrue(result.Ok);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, result.Recipients, "c is offline, x is not in the party; region does not matter");
            Assert.AreEqual(ChatRefusal.NotInParty, ChatRules.TrySend(null, ChatChannel.Party, "bora", Ctx("x", online, party: party), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.NotInParty, ChatRules.TrySend(null, ChatChannel.Party, "bora", Ctx("x", online), 0d).Refusal);
        }

        [Test]
        public void Guild_GoesToOnlineMembersIncludingRecruits()
        {
            GuildState guild = GuildOf("a", "b", "c");
            var online = Online(("a", "r1"), ("b", "r2"), ("c", "r3"), ("x", "r1"));
            ChatSendResult result = ChatRules.TrySend(null, ChatChannel.Guild, "reunião hoje", Ctx("b", online, guild: guild), 0d);

            Assert.IsTrue(result.Ok);
            CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, result.Recipients);
            Assert.AreEqual(ChatRefusal.NotInGuild, ChatRules.TrySend(null, ChatChannel.Guild, "oi", Ctx("x", online, guild: guild), 0d).Refusal);
        }

        [Test]
        public void Whisper_ReachesOnlyTheTargetAndTheSender()
        {
            var online = Online(("a", "r1"), ("b", "r5"), ("c", "r1"));
            ChatSendResult result = ChatRules.TrySend(null, ChatChannel.Whisper, "psiu", Ctx("a", online, whisperTo: "b"), 0d);

            Assert.IsTrue(result.Ok);
            CollectionAssert.AreEqual(new[] { "b", "a" }, result.Recipients);
            Assert.AreEqual("b", result.Message.targetId);
            Assert.AreEqual(ChatRefusal.NoWhisperTarget, ChatRules.TrySend(null, ChatChannel.Whisper, "psiu", Ctx("a", online), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.CannotWhisperSelf, ChatRules.TrySend(null, ChatChannel.Whisper, "psiu", Ctx("a", online, whisperTo: "a"), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.RecipientUnavailable, ChatRules.TrySend(null, ChatChannel.Whisper, "psiu", Ctx("a", online, whisperTo: "zz"), 0d).Refusal);
        }

        [Test]
        public void System_OnlyTheServerWritesAndItReachesEveryoneOrARegion()
        {
            var online = Online(("a", "r1"), ("b", "r2"));
            Assert.AreEqual(ChatRefusal.NotAllowed, ChatRules.TrySend(null, ChatChannel.System, "sou o sistema", Ctx("a", online), 0d).Refusal);

            ChatSendResult all = ChatRules.SystemMessage("O Bema começa em 5 minutos.", null, online, 0d);
            Assert.IsTrue(all.Ok);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, all.Recipients);
            Assert.IsNull(all.Message.senderId);

            ChatSendResult region = ChatRules.SystemMessage("Um chefe acordou.", "r2", online, 0d);
            CollectionAssert.AreEqual(new[] { "b" }, region.Recipients);
            Assert.AreEqual(ChatRefusal.TooLong, ChatRules.SystemMessage(new string('a', 301), null, online, 0d).Refusal);
        }

        [Test]
        public void Text_IsCleanedAndLengthIsPerChannel()
        {
            var online = Online(("a", "r1"), ("b", "r1"));
            Assert.AreEqual(ChatRefusal.Empty, ChatRules.TrySend(null, ChatChannel.General, "  ​ \n ", Ctx("a", online), 0d).Refusal);

            ChatSendResult spaced = ChatRules.TrySend(null, ChatChannel.General, "  oi \t\t gente  ", Ctx("a", online), 0d);
            Assert.AreEqual("oi gente", spaced.Message.text);

            Assert.AreEqual(ChatRefusal.None, ChatRules.TrySend(null, ChatChannel.General, new string('a', 120), Ctx("a", online), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.TooLong, ChatRules.TrySend(null, ChatChannel.General, new string('a', 121), Ctx("a", online), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.None, ChatRules.TrySend(null, ChatChannel.Whisper, new string('a', 200), Ctx("a", online, whisperTo: "b"), 0d).Refusal);
            Assert.AreEqual(ChatRefusal.TooLong, ChatRules.TrySend(null, ChatChannel.Whisper, new string('a', 201), Ctx("a", online, whisperTo: "b"), 0d).Refusal);
        }

        [Test]
        public void Limits_AreTheDecidedOnes()
        {
            Assert.AreEqual(10d, ChatRules.GeneralLimits.MessagesPerMinute, 1e-9);
            Assert.AreEqual(3, ChatRules.GeneralLimits.BurstMessages);
            Assert.AreEqual(200, ChatRules.LimitsOf(ChatChannel.Party).MaxLength);
            Assert.AreEqual(0, ChatRules.LimitsOf(ChatChannel.System).BurstMessages);
            foreach (ChatChannel channel in ChatRules.PlayerChannels)
            {
                Assert.Greater(ChatRules.LimitsOf(channel).BurstMessages, 0, channel.ToString());
            }
        }

        // ------------------------------------------------------------------ anti-flood

        [Test]
        public void TokenBucket_BurstThenSteadyPace()
        {
            ChatTokenBucket bucket = ChatTokenBucket.Full(3);
            Assert.IsTrue(bucket.TryTake(3, 6d, 0d));
            Assert.IsTrue(bucket.TryTake(3, 6d, 0d));
            Assert.IsTrue(bucket.TryTake(3, 6d, 0d));
            Assert.IsFalse(bucket.TryTake(3, 6d, 0d));
            Assert.AreEqual(6d, bucket.SecondsUntilToken(3, 6d, 0d), 1e-9);
            Assert.IsFalse(bucket.TryTake(3, 6d, 5.9d));
            Assert.IsTrue(bucket.TryTake(3, 6d, 6.1d));
            Assert.IsTrue(bucket.TryTake(3, 6d, 1000d), "refills to capacity, never above");
            Assert.IsTrue(bucket.TryTake(3, 6d, 1000d));
            Assert.IsTrue(bucket.TryTake(3, 6d, 1000d));
            Assert.IsFalse(bucket.TryTake(3, 6d, 1000d));
        }

        [Test]
        public void TokenBucket_ClockGoingBackGivesNothing()
        {
            ChatTokenBucket bucket = ChatTokenBucket.Full(1);
            Assert.IsTrue(bucket.TryTake(1, 10d, 100d));
            Assert.IsFalse(bucket.TryTake(1, 10d, 50d));
            Assert.IsFalse(bucket.TryTake(1, 10d, 105d));
            Assert.IsTrue(bucket.TryTake(1, 10d, 110d));
        }

        [Test]
        public void Flood_IsRefusedWithRetryAfter()
        {
            var spam = new ChatSpamState("a");
            var ctx = Ctx("a", Online(("a", "r1")));
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "msg " + (char)('a' + i) + "x", ctx, 0d).Ok);
            }

            ChatSendResult flood = ChatRules.TrySend(spam, ChatChannel.General, "outra coisa", ctx, 1d);
            Assert.AreEqual(ChatRefusal.Flood, flood.Refusal);
            Assert.AreEqual(5d, flood.RetryAfterSeconds, 1e-9);
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.Party, "outra coisa", Ctx("a", Online(("a", "r1"), ("b", "r1")), party: PartyOf("a", "b")), 1d).Ok,
                "each channel has its own bucket");
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "agora sim", ctx, 6.5d).Ok);
        }

        [Test]
        public void Repetition_SameMessageSameChannelWithinWindow()
        {
            var spam = new ChatSpamState("a");
            var online = Online(("a", "r1"), ("b", "r1"));
            var ctx = Ctx("a", online);
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "Vendo espada", ctx, 0d).Ok);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "VENDO ESPADA!!!", ctx, 10d).Refusal);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "vend0 espadaaa", ctx, 20d).Refusal);
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.Whisper, "Vendo espada", Ctx("a", online, whisperTo: "b"), 20d).Ok,
                "another channel is another conversation");
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "Vendo espada", ctx, 31d).Ok, "after 30 s it may be said again");
        }

        [Test]
        public void Repetition_DoesNotSpendTokens()
        {
            var spam = new ChatSpamState("a");
            var ctx = Ctx("a", Online(("a", "r1")));
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "um", ctx, 0d).Ok);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "um", ctx, 0d).Refusal);
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "dois", ctx, 0d).Ok);
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "tres", ctx, 0d).Ok);
        }

        [Test]
        public void AutoMute_AfterFourStrikesAndEscalates()
        {
            var spam = new ChatSpamState("a");
            var ctx = Ctx("a", Online(("a", "r1")));
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "compre ouro", ctx, 0d).Ok);
            for (int i = 1; i < ChatRules.StrikesToAutoMute; i++)
            {
                Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "compre ouro", ctx, i).Refusal);
            }

            ChatSendResult muted = ChatRules.TrySend(spam, ChatChannel.General, "compre ouro", ctx, 4d);
            Assert.AreEqual(ChatRefusal.AutoMuted, muted.Refusal);
            Assert.AreEqual(4d + 60d, muted.MutedUntil, 1e-9);
            Assert.IsTrue(spam.IsAutoMuted(30d));

            ChatSendResult whileMuted = ChatRules.TrySend(spam, ChatChannel.Whisper, "oi", Ctx("a", Online(("a", "r1"), ("b", "r1")), whisperTo: "b"), 30d);
            Assert.AreEqual(ChatRefusal.Muted, whileMuted.Refusal, "the mute covers every channel");
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "desculpa", ctx, 65d).Ok);

            // Insisting again soon: 5 minutes.
            for (int i = 0; i < ChatRules.StrikesToAutoMute - 1; i++)
            {
                Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "desculpa", ctx, 66d + i).Refusal);
            }

            ChatSendResult second = ChatRules.TrySend(spam, ChatChannel.General, "desculpa", ctx, 70d);
            Assert.AreEqual(ChatRefusal.AutoMuted, second.Refusal);
            Assert.AreEqual(70d + 300d, second.MutedUntil, 1e-9);
            Assert.AreEqual(2, spam.AutoMuteLevel);
        }

        [Test]
        public void AutoMute_StrikesExpireAndLevelDecaysAfterACleanHour()
        {
            var spam = new ChatSpamState("a");
            var ctx = Ctx("a", Online(("a", "r1")));
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "x1", ctx, 0d).Ok);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "x1", ctx, 1d).Refusal);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "x1", ctx, 2d).Refusal);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "x1", ctx, 3d).Refusal);
            Assert.AreEqual(3, spam.StrikeCount);

            // 61 s later the old strikes are gone: one more is just a refusal.
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "y1", ctx, 64d).Ok);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySend(spam, ChatChannel.General, "y1", ctx, 65d).Refusal);
            Assert.AreEqual(1, spam.StrikeCount);

            // Mute once, wait more than an hour after it ends, mute again: back to 1 minute.
            for (int i = 0; i < 3; i++)
            {
                ChatRules.TrySend(spam, ChatChannel.General, "y1", ctx, 66d + i);
            }

            Assert.AreEqual(1, spam.AutoMuteLevel);
            double later = spam.AutoMuteUntil + ChatRules.AutoMuteDecaySeconds + 1d;
            Assert.IsTrue(ChatRules.TrySend(spam, ChatChannel.General, "z1", ctx, later).Ok);
            ChatSendResult last = null;
            for (int i = 1; i <= ChatRules.StrikesToAutoMute; i++)
            {
                last = ChatRules.TrySend(spam, ChatChannel.General, "z1", ctx, later + i);
            }

            Assert.AreEqual(ChatRefusal.AutoMuted, last.Refusal);
            Assert.AreEqual(1, spam.AutoMuteLevel);
            Assert.AreEqual(later + ChatRules.StrikesToAutoMute + 60d, last.MutedUntil, 1e-9);
        }

        [Test]
        public void StaffMute_WinsOverEverything()
        {
            ChatContext ctx = Ctx("a", Online(("a", "r1")));
            ctx.StaffMutedUntil = 1000d;
            ChatSendResult result = ChatRules.TrySend(new ChatSpamState("a"), ChatChannel.General, "oi", ctx, 10d);
            Assert.AreEqual(ChatRefusal.MutedByStaff, result.Refusal);
            Assert.AreEqual(1000d, result.MutedUntil);
            Assert.IsTrue(ChatRules.TrySend(null, ChatChannel.General, "oi", ctx, 1000d).Ok);
        }

        // ------------------------------------------------------------------ filter

        [TestCase("que merda", "que *****")]
        [TestCase("MERDAAA", "*******")]
        [TestCase("m3rd4", "*****")]
        [TestCase("p u t a", "* * * *")]
        [TestCase("f.u.c.k this", "*.*.*.* this")]
        [TestCase("você é um otário", "você é um ******")]
        [TestCase("what the fuck!", "what the ****!")]
        [TestCase("seu fdp", "seu ***")]
        [TestCase("this is bullshit", "this is ********")]
        [TestCase("pu​ta", "*****")]
        public void Filter_MasksAndKeepsTheLength(string input, string expected)
        {
            ChatFilterResult result = ChatFilter.Apply(input);
            Assert.AreEqual(expected, result.Text);
            Assert.AreEqual(input.Length, result.Text.Length);
            Assert.IsTrue(result.Masked);
        }

        [Test]
        public void Filter_MasksInsideTheMessageThatGoesOut()
        {
            var online = Online(("a", "r1"), ("b", "r1"));
            ChatSendResult result = ChatRules.TrySend(null, ChatChannel.General, "vai a merda, Tarchon", Ctx("a", online), 0d);
            Assert.IsTrue(result.Ok, "masked, never refused");
            Assert.AreEqual("vai a *****, Tarchon", result.Message.text);
            Assert.AreEqual("vai a merda, Tarchon", result.Message.raw, "the log keeps what was typed");
            Assert.IsTrue(result.Message.masked);
        }

        [TestCase("Escudo das Raízes Eternas")]
        [TestCase("Disputa na Espora do Cuco")]
        [TestCase("o pé do filho ficou bom")]
        [TestCase("Cuidado com o Glutão, ele cuspiu a Pedra da Mistura")]
        [TestCase("computador, curto, escuta, cume, assar, classe, passa")]
        [TestCase("Sexta-feira tem Bema na Vila da Vó Edna")]
        [TestCase("efeito retardado, piranha do rio, Peças de Shulgi")]
        [TestCase("assassin, class, pass, grass, Scunthorpe, cocktail")]
        [TestCase("1v1? 10 ouro, 5 poções, 3 flechas @Maithe")]
        [TestCase("Tarchon, Larthia, Puzur, Djedi, Setne, Ninki, Kul, Apep, Kirta, Tages, Mehen, Ohya, Hahya")]
        [TestCase("Qocho, Litema, Taa Pwani, Napirasu, Garajonay, Aratta, Dur-Jarras, Kurdalægon")]
        [TestCase("Maithe, Ícaro, Joaquim, Maria, Lavine, Vó Edna, Vô Jorge")]
        public void Filter_LeavesCommonWordsAndNamesAlone(string text)
        {
            Assert.AreEqual(text, ChatFilter.Apply(text).Text);
        }

        [Test]
        public void Filter_NoFalsePositiveOnContentAndCityTexts()
        {
            var flagged = new List<string>();
            foreach (List<LocalizationEntry> table in new[] { ContentTexts.Entries, CityTexts.Entries })
            {
                foreach (LocalizationEntry entry in table)
                {
                    foreach (string text in new[] { entry.En, entry.PtBR })
                    {
                        if (!string.IsNullOrEmpty(text) && ChatFilter.IsOffensive(text))
                        {
                            flagged.Add(entry.Key + ": " + ChatFilter.Apply(text).Text);
                        }
                    }
                }
            }

            Assert.Greater(ContentTexts.Entries.Count, 100);
            CollectionAssert.IsEmpty(flagged, string.Join("\n", flagged.Take(20)));
        }

        [Test]
        public void Filter_SharesTheGuildListAndIsExtensible()
        {
            foreach (string word in GuildNameRules.WholeWords.Concat(GuildNameRules.ContainsWords))
            {
                Assert.IsTrue(ChatFilter.IsOffensive(word), word);
            }

            Assert.IsFalse(ChatFilter.IsOffensive("zorbaxo"));
            ChatFilter.AddWholeWord("zorbaxo");
            Assert.IsTrue(ChatFilter.IsOffensive("ZORBAXO!"));
            Assert.AreEqual(GuildRefusal.None, GuildNameRules.ValidateText("zorbaxo", 50, false), "chat-only words do not change the guild");
        }

        [Test]
        public void RepeatKey_IgnoresCaseAccentsLeetAndPunctuation()
        {
            Assert.AreEqual(ChatFilter.RepeatKey("Olá, Vó!!"), ChatFilter.RepeatKey("ola vo"));
            Assert.AreEqual(ChatFilter.RepeatKey("v3nd0 3sp4d4"), ChatFilter.RepeatKey("vendo espada"));
            Assert.AreNotEqual(ChatFilter.RepeatKey("vendo espada"), ChatFilter.RepeatKey("compro espada"));
        }

        // ------------------------------------------------------------------ emotes

        [Test]
        public void Emotes_AreAShortWheelWithEveryCousinVoice()
        {
            Assert.That(ChatEmotes.All.Count, Is.InRange(6, 10));
            Assert.AreEqual(ChatEmotes.All.Count, ChatEmotes.All.Select(e => e.Id).Distinct().Count());
            foreach (ChatEmote emote in ChatEmotes.All)
            {
                Assert.IsNotEmpty(emote.NameEn);
                Assert.IsNotEmpty(emote.NamePt);
                StringAssert.Contains("{0}", emote.LineEn);
                StringAssert.Contains("{0}", emote.LinePt);
                foreach (CharacterId cousin in Enum.GetValues(typeof(CharacterId)))
                {
                    Assert.IsTrue(emote.HasCousinLine(cousin), emote.Id + " " + cousin);
                    string key = ChatEmotes.LineKey(emote.Id, cousin);
                    StringAssert.Contains("{0}", SocialTexts.GetFallback(key), key);
                    StringAssert.Contains("{0}", SocialTexts.GetEnglish(key), key);
                    Assert.IsFalse(ChatFilter.IsOffensive(SocialTexts.GetFallback(key)), key);
                }

                Assert.AreEqual("social.emote." + emote.Id + ".line", ChatEmotes.LineKey(emote.Id, null));
            }
        }

        [Test]
        public void Emote_GoesThroughTheChatRules()
        {
            var spam = new ChatSpamState("a");
            ChatContext ctx = Ctx("a", Online(("a", "r1"), ("b", "r1")), party: PartyOf("a", "b"));
            ctx.SenderCousin = CharacterId.Lavine;
            ChatSendResult wave = ChatRules.TrySendEmote(spam, ChatChannel.Party, ChatEmotes.Wave, ctx, 0d);
            Assert.IsTrue(wave.Ok);
            Assert.AreEqual(ChatEmotes.Wave, wave.Message.emoteId);
            Assert.AreEqual("social.emote.wave.line.lavine", wave.Message.emoteKey);
            Assert.AreEqual(ChatRefusal.Repeated, ChatRules.TrySendEmote(spam, ChatChannel.Party, ChatEmotes.Wave, ctx, 1d).Refusal);
            Assert.IsTrue(ChatRules.TrySendEmote(spam, ChatChannel.Party, ChatEmotes.Thanks, ctx, 1d).Ok);
            Assert.AreEqual(ChatRefusal.UnknownEmote, ChatRules.TrySendEmote(spam, ChatChannel.Party, "dab", ctx, 2d).Refusal);
            Assert.AreEqual(ChatRefusal.NotInGuild, ChatRules.TrySendEmote(spam, ChatChannel.Guild, ChatEmotes.Help, ctx, 2d).Refusal);
        }

        // ------------------------------------------------------------------ block list

        [Test]
        public void BlockList_BlockUnblockAndLimits()
        {
            var list = new PlayerBlockList("me");
            Assert.AreEqual(BlockRefusal.CannotBlockSelf, list.Block("me"));
            Assert.AreEqual(BlockRefusal.InvalidPlayer, list.Block(""));
            Assert.AreEqual(BlockRefusal.None, list.Block("troll"));
            Assert.AreEqual(BlockRefusal.AlreadyBlocked, list.Block("troll"));
            Assert.IsTrue(list.Blocks("troll"));
            Assert.AreEqual(BlockRefusal.None, list.Unblock("troll"));
            Assert.AreEqual(BlockRefusal.NotBlocked, list.Unblock("troll"));

            for (int i = 0; i < PlayerBlockList.MaxBlocked; i++)
            {
                Assert.AreEqual(BlockRefusal.None, list.Block("p" + i));
            }

            Assert.AreEqual(BlockRefusal.ListFull, list.Block("one-more"));
            var reloaded = new PlayerBlockList("me", list.Ids);
            CollectionAssert.AreEqual(list.Ids, reloaded.Ids);
        }

        [Test]
        public void Block_StopsWhispersAndHidesOtherChannels()
        {
            var lists = new Dictionary<string, PlayerBlockList> { ["b"] = new PlayerBlockList("b", new[] { "a" }) };
            Func<string, PlayerBlockList> of = id => lists.TryGetValue(id, out PlayerBlockList l) ? l : null;
            var online = Online(("a", "r1"), ("b", "r1"), ("c", "r1"));

            Assert.AreEqual(ChatRefusal.RecipientUnavailable,
                ChatRules.TrySend(null, ChatChannel.Whisper, "oi", Ctx("a", online, whisperTo: "b", blocks: of), 0d).Refusal,
                "same answer as offline: the block is never revealed");
            Assert.AreEqual(ChatRefusal.YouBlockedRecipient,
                ChatRules.TrySend(null, ChatChannel.Whisper, "oi", Ctx("b", online, whisperTo: "a", blocks: of), 0d).Refusal);

            ChatSendResult general = ChatRules.TrySend(null, ChatChannel.General, "oi", Ctx("a", online, blocks: of), 0d);
            CollectionAssert.AreEquivalent(new[] { "a", "c" }, general.Recipients);

            ChatSendResult system = ChatRules.SystemMessage("aviso", null, online, 0d);
            Assert.AreEqual(3, system.Recipients.Count);
        }

        [Test]
        public void Block_PartyGuildTradeDuelHooksAreOptional()
        {
            var bBlocksA = new PlayerBlockList("b", new[] { "a" });

            // Party.
            var party = new PartyState("a", null);
            Assert.AreEqual(PartyRefusal.InvalidPlayer, PartyRules.Invite(party, "a", "b", null, false, 0d, bBlocksA).Refusal);
            Assert.IsTrue(PartyRules.Invite(party, "a", "b", null, false, 0d).Ok, "no list = the old behaviour");

            // Guild.
            var guild = new GuildState("g", "Coluna", "", default, "a", 0d);
            Assert.AreEqual(GuildRefusal.InvalidPlayer, GuildRules.Invite(guild, "a", "b", false, -1d, 0d, bBlocksA).Refusal);
            Assert.AreEqual(GuildRefusal.None, GuildRules.Invite(guild, "a", "b", false, -1d, 0d).Refusal);

            // Trade.
            var pa = new TradePresence("a", ZoneKind.City, "c1");
            var pb = new TradePresence("b", ZoneKind.City, "c1");
            Assert.AreEqual(TradeRefusal.PartnerUnavailable, TradeRules.CanOpen(pa, pb, 3f, bBlocksA));
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(pa, pb, 3f));
            Assert.AreEqual(TradeRefusal.PartnerUnavailable, TradeSession.TryOpen("t1", pa, pb, 3f, 0d, out TradeSession none, bBlocksA));
            Assert.IsNull(none);
            Assert.AreEqual(TradeRefusal.None, TradeSession.TryOpen("t2", pa, pb, 3f, 0d, out TradeSession session));
            Assert.IsNotNull(session);

            // Duel: DuelRules.Challenge already takes targetBlocksDuels.
            Assert.IsTrue(PlayerBlockRules.TargetRefusesDuel(false, bBlocksA, "a"));
            Assert.IsTrue(PlayerBlockRules.TargetRefusesDuel(true, null, "a"));
            Assert.IsFalse(PlayerBlockRules.TargetRefusesDuel(false, bBlocksA, "c"));
            Assert.IsFalse(PlayerBlockRules.TargetRefusesDuel(false, null, "a"));

            // One-way: b blocked a, a did not block b.
            Assert.IsTrue(PlayerBlockRules.CanContact(new PlayerBlockList("a"), "b", SocialContactKind.PartyInvite));
            foreach (SocialContactKind kind in Enum.GetValues(typeof(SocialContactKind)))
            {
                Assert.IsFalse(PlayerBlockRules.CanContact(bBlocksA, "a", kind), kind.ToString());
            }
        }

        // ------------------------------------------------------------------ reports

        private static List<ChatMessage> LogOf(int fromTroll, string reporter = "me")
        {
            var log = new List<ChatMessage>();
            for (int i = 0; i < fromTroll; i++)
            {
                log.Add(new ChatMessage { messageId = i, channel = ChatChannel.General, senderId = "troll", raw = "spam " + i, at = i });
                log.Add(new ChatMessage { messageId = 1000 + i, channel = ChatChannel.General, senderId = "other", raw = "oi", at = i });
            }

            log.Add(new ChatMessage { messageId = 5000, channel = ChatChannel.Whisper, senderId = reporter, targetId = "troll", raw = "para", at = 99 });
            log.Add(new ChatMessage { messageId = 5001, channel = ChatChannel.Whisper, senderId = reporter, targetId = "friend", raw = "oi", at = 100 });
            return log;
        }

        [Test]
        public void Report_AttachesTheLastMessagesOfTheTarget()
        {
            var tally = new ReportDailyTally();
            ReportRefusal refusal = ReportRules.TryFile("r1", "me", "troll", ReportReason.Spam, " flood no geral ", "r1", LogOf(15), tally,
                1_800_000_000L, out PlayerReport report);

            Assert.AreEqual(ReportRefusal.None, refusal);
            Assert.AreEqual(ReportRules.EvidenceMessages, report.evidence.Length);
            Assert.AreEqual(6, report.evidence[0].messageId, "oldest first, the last 10 kept");
            Assert.AreEqual(5000, report.evidence[report.evidence.Length - 1].messageId, "the reporter's whisper to the target is context");
            Assert.IsFalse(report.evidence.Any(m => m.senderId == "other" || m.targetId == "friend"));
            Assert.AreEqual("flood no geral", report.comment);
            Assert.AreEqual(1, tally.Count);

            string line = report.ToLogLine();
            StringAssert.StartsWith("report=r1 at=1800000000 by=me on=troll reason=spam region=r1 evidence=[6;", line);
            StringAssert.EndsWith(";5000] comment=\"flood no geral\"", line);
        }

        [Test]
        public void Report_LimitsAndChecks()
        {
            var tally = new ReportDailyTally();
            long day = 1_800_000_000L;
            Assert.AreEqual(ReportRefusal.CannotReportSelf, ReportRules.TryFile("x", "me", "me", ReportReason.Spam, null, null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.InvalidPlayer, ReportRules.TryFile("x", "me", "", ReportReason.Spam, null, null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.InvalidReason, ReportRules.TryFile("x", "me", "t", (ReportReason)99, null, null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.CommentRequired, ReportRules.TryFile("x", "me", "t", ReportReason.Other, "  ", null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.CommentTooLong,
                ReportRules.TryFile("x", "me", "t", ReportReason.Harassment, new string('a', 201), null, null, tally, day, out _));
            Assert.AreEqual(0, tally.Count, "refusals count nothing");

            for (int i = 0; i < ReportRules.MaxReportsPerDay; i++)
            {
                Assert.AreEqual(ReportRefusal.None, ReportRules.TryFile("x", "me", "t" + i, ReportReason.Botting, null, null, null, tally, day + i, out _));
            }

            Assert.AreEqual(ReportRefusal.AlreadyReportedToday, ReportRules.TryFile("x", "me", "t0", ReportReason.Scam, null, null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.DailyLimitReached, ReportRules.TryFile("x", "me", "t9", ReportReason.Scam, null, null, null, tally, day, out _));
            Assert.AreEqual(ReportRefusal.None, ReportRules.TryFile("x", "me", "t9", ReportReason.Scam, null, null, null, tally, day + 86_400L, out PlayerReport next),
                "a new UTC day");
            Assert.AreEqual(0, next.evidence.Length);
            Assert.AreEqual(ReportRefusal.None, ReportRules.TryFile("x", "gm", "t9", ReportReason.Scam, null, null, null, null, day, out _), "no tally = no limit");
        }

        // ------------------------------------------------------------------ staff

        private static GmCommand Cmd(GmCommandKind kind, StaffRole role, string target = "p", StaffRole targetRole = StaffRole.Player,
            long duration = 0, string reason = "spam no geral") =>
            new GmCommand { Kind = kind, ActorId = "staff", ActorRole = role, TargetId = target, TargetRole = targetRole, DurationSeconds = duration, Reason = reason };

        [Test]
        public void Gm_PermissionsByRole()
        {
            Assert.AreEqual(GmRefusal.NotStaff, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Player)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Moderator)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.Moderator, duration: 3600)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Unmute, StaffRole.Moderator, reason: null)));
            Assert.AreEqual(GmRefusal.NoPermission, GmRules.Validate(Cmd(GmCommandKind.Ban, StaffRole.Moderator, duration: 86400)));
            Assert.AreEqual(GmRefusal.NoPermission, GmRules.Validate(Cmd(GmCommandKind.Unban, StaffRole.Moderator)));
            Assert.AreEqual(GmRefusal.NoPermission, GmRules.Validate(Cmd(GmCommandKind.Teleport, StaffRole.Moderator)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Ban, StaffRole.GameMaster, duration: 86400)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Unban, StaffRole.GameMaster)));
        }

        [Test]
        public void Gm_TargetsReasonsAndDurations()
        {
            Assert.AreEqual(GmRefusal.InvalidTarget, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Moderator, target: null)));
            Assert.AreEqual(GmRefusal.CannotTargetSelf, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Moderator, target: "staff")));
            Assert.AreEqual(GmRefusal.TargetOutranks, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.Moderator, targetRole: StaffRole.Moderator, duration: 600)));
            Assert.AreEqual(GmRefusal.TargetOutranks, GmRules.Validate(Cmd(GmCommandKind.Ban, StaffRole.GameMaster, targetRole: StaffRole.GameMaster, duration: 86400)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.GameMaster, targetRole: StaffRole.Moderator, duration: 600)));
            Assert.AreEqual(GmRefusal.ReasonRequired, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Moderator, reason: " a ")));
            Assert.AreEqual(GmRefusal.ReasonTooLong, GmRules.Validate(Cmd(GmCommandKind.Kick, StaffRole.Moderator, reason: new string('a', 201))));

            Assert.AreEqual(GmRefusal.InvalidDuration, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.Moderator, duration: 59)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.Moderator, duration: 24 * 3600)));
            Assert.AreEqual(GmRefusal.DurationTooLong, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.Moderator, duration: 24 * 3600 + 1)));
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(Cmd(GmCommandKind.Mute, StaffRole.GameMaster, duration: 7 * 24 * 3600)));
            Assert.AreEqual(GmRefusal.InvalidDuration, GmRules.Validate(Cmd(GmCommandKind.Ban, StaffRole.GameMaster, duration: 3599)));
            Assert.AreEqual(GmRefusal.DurationTooLong, GmRules.Validate(Cmd(GmCommandKind.Ban, StaffRole.GameMaster, duration: GmRules.MaxBanSeconds + 1)));

            GmCommand permanent = Cmd(GmCommandKind.Ban, StaffRole.GameMaster);
            permanent.Permanent = true;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(permanent));
        }

        [Test]
        public void Gm_Teleport()
        {
            GmCommand go = Cmd(GmCommandKind.Teleport, StaffRole.GameMaster, targetRole: StaffRole.GameMaster);
            go.TeleportMode = GmTeleportMode.GoToPlayer;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(go), "visiting moves only the staff member");

            GmCommand bring = Cmd(GmCommandKind.Teleport, StaffRole.GameMaster, targetRole: StaffRole.GameMaster);
            bring.TeleportMode = GmTeleportMode.BringPlayer;
            Assert.AreEqual(GmRefusal.TargetOutranks, GmRules.Validate(bring));
            bring.TargetRole = StaffRole.Player;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(bring));

            GmCommand send = Cmd(GmCommandKind.Teleport, StaffRole.GameMaster, target: null);
            send.TeleportMode = GmTeleportMode.ToPosition;
            Assert.AreEqual(GmRefusal.InvalidPosition, GmRules.Validate(send));
            send.MapId = "map_05";
            send.X = float.NaN;
            Assert.AreEqual(GmRefusal.InvalidPosition, GmRules.Validate(send));
            send.X = 10f;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(send), "sending oneself");
        }

        [Test]
        public void Gm_LogQuery()
        {
            GmCommand query = Cmd(GmCommandKind.QueryLog, StaffRole.Moderator, target: null);
            query.LogKind = GmLogKind.Chat;
            query.LogFromUnix = 1000;
            query.LogToUnix = 1000 + 3600;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(query));
            query.LogKind = GmLogKind.Trade;
            Assert.AreEqual(GmRefusal.NoPermission, GmRules.Validate(query), "trade log is for game masters");
            query.ActorRole = StaffRole.GameMaster;
            Assert.AreEqual(GmRefusal.None, GmRules.Validate(query));
            query.LogToUnix = query.LogFromUnix + GmRules.MaxLogSpanSeconds + 1;
            Assert.AreEqual(GmRefusal.LogRangeTooLong, GmRules.Validate(query));
            query.LogToUnix = query.LogFromUnix;
            Assert.AreEqual(GmRefusal.InvalidLogQuery, GmRules.Validate(query));
            query.LogToUnix = query.LogFromUnix + 10;
            query.LogLimit = GmRules.MaxLogLimit + 1;
            Assert.AreEqual(GmRefusal.InvalidLogQuery, GmRules.Validate(query));
        }

        [Test]
        public void Gm_RecordAndLogLine()
        {
            GmRefusal ok = GmRules.TryIssue(Cmd(GmCommandKind.Ban, StaffRole.GameMaster, duration: 86400, reason: "dup de item"), "c1", 1_800_000_000L,
                out GmCommandRecord record);
            Assert.AreEqual(GmRefusal.None, ok);
            Assert.AreEqual(1_800_086_400L, record.expiresAtUnix);
            Assert.IsTrue(GmRules.IsInForce(record, 1_800_000_001L));
            Assert.IsFalse(GmRules.IsInForce(record, 1_800_086_400L));
            Assert.AreEqual("gm=c1 at=1800000000 by=staff(game_master) cmd=ban on=p dur=86400 until=1800086400 ok=1 reason=\"dup de item\"",
                record.ToLogLine());

            GmRefusal refused = GmRules.TryIssue(Cmd(GmCommandKind.Ban, StaffRole.Moderator, duration: 86400), "c2", 5L, out GmCommandRecord denied);
            Assert.AreEqual(GmRefusal.NoPermission, refused);
            Assert.IsFalse(denied.Ok);
            Assert.AreEqual(0L, denied.expiresAtUnix);
            Assert.IsFalse(GmRules.IsInForce(denied, 6L));
            StringAssert.Contains("ok=0 refusal=no_permission", denied.ToLogLine(), "refused attempts are logged too");

            GmCommand perma = Cmd(GmCommandKind.Ban, StaffRole.GameMaster);
            perma.Permanent = true;
            GmRules.TryIssue(perma, "c3", 10L, out GmCommandRecord forever);
            Assert.IsTrue(GmRules.IsInForce(forever, long.MaxValue));
            StringAssert.Contains("dur=permanent", forever.ToLogLine());
        }

        [Test]
        public void ChatMessage_LogLineIsStableAndEscaped()
        {
            var message = new ChatMessage
            {
                messageId = 42, channel = ChatChannel.Whisper, senderId = "a", targetId = "b", regionId = "r1", raw = "diz \"oi\"", at = 12.5d,
            };
            Assert.AreEqual("chat=42 at=12.5 ch=whisper from=a to=b region=r1 masked=0 emote=- text=\"diz \\\"oi\\\"\"", message.ToLogLine());
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_EveryRefusalReasonAndChannelHasPtAndEn()
        {
            var keys = new List<string>();
            keys.AddRange(Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>().Select(SocialTexts.ChannelKey));
            keys.AddRange(Enum.GetValues(typeof(ChatRefusal)).Cast<ChatRefusal>().Select(SocialTexts.ChatRefusalKey));
            keys.AddRange(Enum.GetValues(typeof(BlockRefusal)).Cast<BlockRefusal>().Select(SocialTexts.BlockRefusalKey));
            keys.AddRange(Enum.GetValues(typeof(ReportReason)).Cast<ReportReason>().Select(SocialTexts.ReportReasonKey));
            keys.AddRange(Enum.GetValues(typeof(ReportRefusal)).Cast<ReportRefusal>().Select(SocialTexts.ReportRefusalKey));
            keys.AddRange(Enum.GetValues(typeof(GmRefusal)).Cast<GmRefusal>().Select(SocialTexts.GmRefusalKey));
            foreach (string key in keys)
            {
                Assert.IsNotEmpty(SocialTexts.GetFallback(key) ?? "", key);
                Assert.IsNotEmpty(SocialTexts.GetEnglish(key) ?? "", key);
            }

            Assert.AreEqual(SocialTexts.Entries.Count, SocialTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");
            Assert.AreEqual("Calma! Espere 5 s para mandar outra mensagem.", SocialTexts.Format(null, SocialTexts.FloodWait, 5));
        }
    }
}
