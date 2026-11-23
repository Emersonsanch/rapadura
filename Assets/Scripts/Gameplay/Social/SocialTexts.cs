using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Social
{
    /// <summary>
    /// Localization (en + pt-BR) of the chat and the moderation: channel names, every refusal of the chat
    /// (<see cref="ChatRefusal"/>), the block list (<see cref="BlockRefusal"/>), the reports (<see cref="ReportReason"/>,
    /// <see cref="ReportRefusal"/>) and the staff commands (<see cref="GmRefusal"/>), plus the emotes (<see cref="ChatEmotes"/>).
    /// Same self-contained pattern as <c>PartyTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class SocialTexts
    {
        /// <summary>"Você foi silenciado até {0}."; {0} = time.</summary>
        public const string MutedNotice = "social.chat.muted_notice";

        /// <summary>"Calma! Espere {0} s para mandar outra mensagem."</summary>
        public const string FloodWait = "social.chat.flood_wait";

        /// <summary>"Denúncia enviada. Obrigado por ajudar a cuidar da ilha."</summary>
        public const string ReportSent = "social.report.sent";

        /// <summary>"{0} foi bloqueado."</summary>
        public const string BlockedNotice = "social.block.blocked";

        /// <summary>"{0} foi desbloqueado."</summary>
        public const string UnblockedNotice = "social.block.unblocked";

        public static string ChannelKey(ChatChannel channel) => "social.chat.channel." + SocialLog.Snake(channel.ToString());
        public static string ChatRefusalKey(ChatRefusal reason) => "social.chat.refusal." + SocialLog.Snake(reason.ToString());
        public static string BlockRefusalKey(BlockRefusal reason) => "social.block.refusal." + SocialLog.Snake(reason.ToString());
        public static string ReportReasonKey(ReportReason reason) => "social.report.reason." + SocialLog.Snake(reason.ToString());
        public static string ReportRefusalKey(ReportRefusal reason) => "social.report.refusal." + SocialLog.Snake(reason.ToString());
        public static string GmRefusalKey(GmRefusal reason) => "social.gm.refusal." + SocialLog.Snake(reason.ToString());

        private static readonly (ChatChannel Channel, string En, string Pt)[] ChannelTexts =
        {
            (ChatChannel.General, "General", "Geral"),
            (ChatChannel.Party, "Party", "Grupo"),
            (ChatChannel.Guild, "Guild", "Guilda"),
            (ChatChannel.Whisper, "Whisper", "Sussurro"),
            (ChatChannel.System, "System", "Sistema"),
        };

        private static readonly (ChatRefusal Reason, string En, string Pt)[] ChatRefusalTexts =
        {
            (ChatRefusal.None, "Sent.", "Enviada."),
            (ChatRefusal.InvalidSender, "You cannot chat right now.", "Você não pode usar o chat agora."),
            (ChatRefusal.Empty, "Write something first.", "Escreva alguma coisa primeiro."),
            (ChatRefusal.TooLong, "That message is too long.", "Essa mensagem está comprida demais."),
            (ChatRefusal.NotAllowed, "Only the island itself speaks in this channel.", "Só a própria ilha fala neste canal."),
            (ChatRefusal.NotInParty, "You are not in a party.", "Você não está em um grupo."),
            (ChatRefusal.NotInGuild, "You are not in a guild.", "Você não está em uma guilda."),
            (ChatRefusal.NoWhisperTarget, "Who do you want to whisper to?", "Para quem você quer sussurrar?"),
            (ChatRefusal.CannotWhisperSelf, "You cannot whisper to yourself.", "Você não pode sussurrar para si mesmo."),
            (ChatRefusal.RecipientUnavailable, "That player is not available.", "Esse jogador não está disponível."),
            (ChatRefusal.YouBlockedRecipient, "You blocked that player. Unblock them to whisper.", "Você bloqueou esse jogador. Desbloqueie para sussurrar."),
            (ChatRefusal.LevelTooLow, "The general chat opens at level 5.", "O chat geral abre no nível 5."),
            (ChatRefusal.Flood, "Easy! Wait a moment before the next message.", "Calma! Espere um pouco antes da próxima mensagem."),
            (ChatRefusal.Repeated, "You just said that.", "Você acabou de dizer isso."),
            (ChatRefusal.AutoMuted, "Too many messages: you are muted for a while.", "Mensagens demais: você ficou em silêncio por um tempo."),
            (ChatRefusal.Muted, "You are muted for a while.", "Você está em silêncio por um tempo."),
            (ChatRefusal.MutedByStaff, "A moderator muted you.", "Um moderador silenciou você."),
            (ChatRefusal.UnknownEmote, "That emote does not exist.", "Esse gesto não existe."),
            (ChatRefusal.UnknownChannel, "That channel does not exist.", "Esse canal não existe."),
            (ChatRefusal.NoRegion, "The general chat comes back once you arrive.", "O chat geral volta quando você chegar."),
        };

        private static readonly (BlockRefusal Reason, string En, string Pt)[] BlockRefusalTexts =
        {
            (BlockRefusal.None, "Done.", "Feito."),
            (BlockRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (BlockRefusal.CannotBlockSelf, "You cannot block yourself.", "Você não pode bloquear a si mesmo."),
            (BlockRefusal.AlreadyBlocked, "That player is already blocked.", "Esse jogador já está bloqueado."),
            (BlockRefusal.ListFull, "Your block list is full (100).", "Sua lista de bloqueio está cheia (100)."),
            (BlockRefusal.NotBlocked, "That player is not blocked.", "Esse jogador não está bloqueado."),
        };

        private static readonly (ReportReason Reason, string En, string Pt)[] ReportReasonTexts =
        {
            (ReportReason.Spam, "Spam or flood", "Spam ou flood"),
            (ReportReason.OffensiveLanguage, "Offensive language", "Linguagem ofensiva"),
            (ReportReason.Harassment, "Harassment", "Assédio ou perseguição"),
            (ReportReason.Cheating, "Cheating", "Trapaça"),
            (ReportReason.Botting, "Bot", "Uso de robô"),
            (ReportReason.Scam, "Scam", "Golpe"),
            (ReportReason.InappropriateName, "Inappropriate name", "Nome impróprio"),
            (ReportReason.Other, "Other", "Outro"),
        };

        private static readonly (ReportRefusal Reason, string En, string Pt)[] ReportRefusalTexts =
        {
            (ReportRefusal.None, "Report sent.", "Denúncia enviada."),
            (ReportRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (ReportRefusal.CannotReportSelf, "You cannot report yourself.", "Você não pode denunciar a si mesmo."),
            (ReportRefusal.DailyLimitReached, "You already sent 5 reports today.", "Você já mandou 5 denúncias hoje."),
            (ReportRefusal.AlreadyReportedToday, "You already reported that player today.", "Você já denunciou esse jogador hoje."),
            (ReportRefusal.CommentTooLong, "The comment is too long (200).", "O comentário está comprido demais (200)."),
            (ReportRefusal.InvalidReason, "Pick a reason.", "Escolha um motivo."),
            (ReportRefusal.CommentRequired, "Tell us what happened.", "Conte o que aconteceu."),
        };

        private static readonly (GmRefusal Reason, string En, string Pt)[] GmRefusalTexts =
        {
            (GmRefusal.None, "Command accepted.", "Comando aceito."),
            (GmRefusal.NotStaff, "Staff only.", "Só para a equipe."),
            (GmRefusal.NoPermission, "Your role cannot do that.", "Seu cargo não pode fazer isso."),
            (GmRefusal.InvalidTarget, "Pick a player.", "Escolha um jogador."),
            (GmRefusal.CannotTargetSelf, "Not on yourself.", "Não em você mesmo."),
            (GmRefusal.TargetOutranks, "That player's role is equal to or above yours.", "O cargo desse jogador é igual ou maior que o seu."),
            (GmRefusal.ReasonRequired, "Write the reason.", "Escreva o motivo."),
            (GmRefusal.ReasonTooLong, "The reason is too long (200).", "O motivo está comprido demais (200)."),
            (GmRefusal.InvalidDuration, "Invalid duration.", "Duração inválida."),
            (GmRefusal.DurationTooLong, "Longer than your role allows.", "Mais longo do que seu cargo permite."),
            (GmRefusal.InvalidPosition, "Invalid map or position.", "Mapa ou posição inválidos."),
            (GmRefusal.InvalidLogQuery, "Invalid log query.", "Consulta de registro inválida."),
            (GmRefusal.LogRangeTooLong, "Query at most 7 days at a time.", "Consulte no máximo 7 dias por vez."),
            (GmRefusal.UnknownCommand, "Unknown command.", "Comando desconhecido."),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(MutedNotice, "You are muted until {0}.", "Você está em silêncio até {0}."),
            new LocalizationEntry(FloodWait, "Easy! Wait {0} s to send another message.", "Calma! Espere {0} s para mandar outra mensagem."),
            new LocalizationEntry(ReportSent, "Report sent. Thank you for helping look after the island.",
                "Denúncia enviada. Obrigado por ajudar a cuidar da ilha."),
            new LocalizationEntry(BlockedNotice, "{0} was blocked.", "{0} foi bloqueado."),
            new LocalizationEntry(UnblockedNotice, "{0} was unblocked.", "{0} foi desbloqueado."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            entries.AddRange(ChannelTexts.Select(t => new LocalizationEntry(ChannelKey(t.Channel), t.En, t.Pt)));
            entries.AddRange(ChatRefusalTexts.Select(t => new LocalizationEntry(ChatRefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(BlockRefusalTexts.Select(t => new LocalizationEntry(BlockRefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(ReportReasonTexts.Select(t => new LocalizationEntry(ReportReasonKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(ReportRefusalTexts.Select(t => new LocalizationEntry(ReportRefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(GmRefusalTexts.Select(t => new LocalizationEntry(GmRefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(ChatEmotes.Entries());
            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(MutedNotice)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                var map = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    map[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = map;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>The English text of <paramref name="key"/> (tests and the server's English log), or null.</summary>
        public static string GetEnglish(string key)
        {
            foreach (LocalizationEntry entry in Entries)
            {
                if (entry.Key == key)
                {
                    return entry.En;
                }
            }

            return null;
        }
    }
}
