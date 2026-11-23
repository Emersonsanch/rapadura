using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>
    /// Localization (en + pt-BR) of the guild: refusals, ranks, permissions, war states, bank actions, emblem parts and the
    /// guild messages. Same self-contained pattern as <c>PartyTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class GuildTexts
    {
        /// <summary>"{0} convidou você para a guilda {1}."</summary>
        public const string InviteReceived = "guild.invite.received";

        /// <summary>"{0} entrou na guilda."</summary>
        public const string MemberJoined = "guild.member.joined";

        public const string MemberLeft = "guild.member.left";

        /// <summary>"{0} foi expulso da guilda por {1}."</summary>
        public const string MemberKicked = "guild.member.kicked";

        /// <summary>"{0} agora é {1}."; {1} = rank name.</summary>
        public const string RankChanged = "guild.member.rank_changed";

        public const string NewLeader = "guild.member.new_leader";

        /// <summary>"{0} ficou {1} dias sem aparecer; {2} agora lidera a guilda."</summary>
        public const string LeaderInactiveHandover = "guild.member.leader_inactive";

        /// <summary>"A guilda chegou ao nível {0}."</summary>
        public const string LevelUp = "guild.level.up";

        /// <summary>"{0} declarou guerra a {1}."</summary>
        public const string WarDeclared = "guild.war.declared";

        /// <summary>"A guerra contra {0} começa em {1} minutos. Na cidade ninguém luta."</summary>
        public const string WarStartsSoon = "guild.war.starts_soon";

        /// <summary>"Fim da guerra: {0} {1} × {2} {3}."</summary>
        public const string WarScore = "guild.war.score";

        public const string WarDraw = "guild.war.draw";

        /// <summary>"{0} venceu a guerra."</summary>
        public const string WarWinner = "guild.war.winner";

        public const string MessageOfTheDayTitle = "guild.motd.title";

        public static string RefusalKey(GuildRefusal reason) => "guild.refusal." + Snake(reason.ToString());
        public static string RankKey(GuildRank rank) => "guild.rank." + Snake(rank.ToString());
        public static string PermissionKey(GuildPermission permission) => "guild.permission." + Snake(permission.ToString());
        public static string WarStateKey(GuildWarState state) => "guild.war_state." + Snake(state.ToString());
        public static string WarEndKey(GuildWarEndReason reason) => "guild.war_end." + Snake(reason.ToString());
        public static string BankActionKey(GuildBankAction action) => "guild.bank." + Snake(action.ToString());
        public static string ShapeKey(GuildEmblemShape shape) => "guild.emblem.shape." + Snake(shape.ToString());
        public static string ColorKey(GuildEmblemColor color) => "guild.emblem.color." + Snake(color.ToString());
        public static string SymbolKey(GuildEmblemSymbol symbol) => "guild.emblem.symbol." + Snake(symbol.ToString());

        /// <summary>Every single permission (no None, no combinations): the rows of the permission screen.</summary>
        public static readonly GuildPermission[] SinglePermissions =
        {
            GuildPermission.Invite, GuildPermission.Kick, GuildPermission.Promote, GuildPermission.Demote, GuildPermission.BankDeposit,
            GuildPermission.BankWithdraw, GuildPermission.EditBoard, GuildPermission.EditMotd, GuildPermission.DeclareWar,
            GuildPermission.EditIdentity, GuildPermission.Disband,
        };

        private static readonly (GuildRefusal Reason, string En, string Pt)[] RefusalTexts =
        {
            (GuildRefusal.None, "Done.", "Feito."),
            (GuildRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (GuildRefusal.NotMember, "You are not in this guild.", "Você não está nesta guilda."),
            (GuildRefusal.NoPermission, "Your rank cannot do that.", "Seu cargo não permite isso."),
            (GuildRefusal.TargetNotMember, "That player is not in your guild.", "Esse jogador não está na sua guilda."),
            (GuildRefusal.CannotTargetSelf, "You cannot do that to yourself.", "Você não pode fazer isso consigo mesmo."),
            (GuildRefusal.TargetRankTooHigh, "Only ranks below yours.", "Só cargos abaixo do seu."),
            (GuildRefusal.AlreadyInGuild, "That player is already in a guild.", "Esse jogador já está em uma guilda."),
            (GuildRefusal.AlreadyInvited, "An invite is already waiting for that player.", "Esse jogador já tem um convite esperando."),
            (GuildRefusal.NoInvite, "There is no invite to answer.", "Não há convite para responder."),
            (GuildRefusal.InviteExpired, "It expired.", "O prazo acabou."),
            (GuildRefusal.GuildFull, "The guild is full for its level.", "A guilda está cheia para o nível dela."),
            (GuildRefusal.RejoinCooldown, "Left a guild less than 24 hours ago.", "Saiu de uma guilda há menos de 24 horas."),
            (GuildRefusal.Disbanded, "The guild no longer exists.", "A guilda não existe mais."),
            (GuildRefusal.LevelTooLow, "Founding a guild needs level 30.", "Fundar uma guilda pede nível 30."),
            (GuildRefusal.NotEnoughGold, "Founding a guild costs 5000 gold.", "Fundar uma guilda custa 5000 de ouro."),
            (GuildRefusal.NameTooShort, "The name needs at least 3 characters.", "O nome precisa de pelo menos 3 letras."),
            (GuildRefusal.NameTooLong, "The name allows at most 16 characters.", "O nome aceita no máximo 16 letras."),
            (GuildRefusal.NameInvalidCharacters, "Only letters, spaces and up to 3 digits; start with a letter.",
                "Só letras, espaços e até 3 números; comece com uma letra."),
            (GuildRefusal.NameOffensive, "That name is not allowed.", "Esse nome não é permitido."),
            (GuildRefusal.NameTaken, "Another guild already has that name.", "Outra guilda já tem esse nome."),
            (GuildRefusal.TextTooLong, "The text is too long.", "O texto está longo demais."),
            (GuildRefusal.TextOffensive, "The text has a word that is not allowed.", "O texto tem uma palavra não permitida."),
            (GuildRefusal.EmblemInvalid, "The emblem's border and symbol need a color other than the field's.",
                "A borda e o símbolo do brasão precisam de cor diferente da do campo."),
            (GuildRefusal.InvalidRank, "That rank cannot be given this way.", "Esse cargo não pode ser dado assim."),
            (GuildRefusal.RankFull, "The guild already has the most of that rank.", "A guilda já tem o máximo desse cargo."),
            (GuildRefusal.InvalidAmount, "Invalid amount.", "Quantidade inválida."),
            (GuildRefusal.NotEnoughInBank, "The bank does not hold that much.", "O banco não tem tudo isso."),
            (GuildRefusal.DailyLimitReached, "Your rank's daily limit would be passed.", "Passaria do limite diário do seu cargo."),
            (GuildRefusal.ItemNotTradable, "Bound items cannot go into the bank.", "Itens vinculados não entram no banco."),
            (GuildRefusal.BankFull, "The guild bank is full.", "O banco da guilda está cheio."),
            (GuildRefusal.BankNotEmpty, "Empty the bank before disbanding.", "Esvazie o banco antes de desfazer a guilda."),
            (GuildRefusal.NoticeNotFound, "That notice is gone.", "Esse aviso não existe mais."),
            (GuildRefusal.AtWar, "Not while a war is declared or running.", "Não enquanto houver guerra declarada ou em curso."),
            (GuildRefusal.WarSameGuild, "A guild cannot war on itself.", "Uma guilda não declara guerra a si mesma."),
            (GuildRefusal.WarAlreadyExists, "There is already a war between these guilds.", "Já existe uma guerra entre essas guildas."),
            (GuildRefusal.WarTooManyWars, "A guild has at most 3 open wars.", "Uma guilda tem no máximo 3 guerras abertas."),
            (GuildRefusal.WarTooFewMembers, "Both guilds need at least 5 members.", "As duas guildas precisam de pelo menos 5 membros."),
            (GuildRefusal.WarCooldown, "These guilds fought less than 24 hours ago.", "Essas guildas lutaram há menos de 24 horas."),
            (GuildRefusal.WarWrongState, "The war is not at that stage.", "A guerra não está nessa etapa."),
            (GuildRefusal.WarNotParticipant, "Your guild is not in that war.", "Sua guilda não está nessa guerra."),
            (GuildRefusal.WarInvalidTerms, "Duration 1–72 h and a kill target up to 500.", "Duração de 1 a 72 h e meta de até 500 abates."),
            (GuildRefusal.TextEmpty, "Write something first.", "Escreva algo primeiro."),
            (GuildRefusal.ItemHasOwnData, "Generated, refined or worn pieces cannot go into the bank; it keeps only plain items.",
                "Peças geradas, refinadas ou gastas não entram no banco; ele só guarda itens comuns."),
        };

        private static readonly (GuildRank Rank, string En, string Pt)[] RankTexts =
        {
            (GuildRank.Recruit, "Recruit", "Recruta"),
            (GuildRank.Member, "Member", "Membro"),
            (GuildRank.Officer, "Officer", "Oficial"),
            (GuildRank.Vice, "Vice-leader", "Vice-líder"),
            (GuildRank.Leader, "Leader", "Líder"),
        };

        private static readonly (GuildPermission Permission, string En, string Pt)[] PermissionTexts =
        {
            (GuildPermission.Invite, "Invite", "Convidar"),
            (GuildPermission.Kick, "Kick", "Expulsar"),
            (GuildPermission.Promote, "Promote", "Promover"),
            (GuildPermission.Demote, "Demote", "Rebaixar"),
            (GuildPermission.BankDeposit, "Deposit in the bank", "Depositar no banco"),
            (GuildPermission.BankWithdraw, "Withdraw from the bank", "Sacar do banco"),
            (GuildPermission.EditBoard, "Edit the board", "Editar o mural"),
            (GuildPermission.EditMotd, "Message of the day", "Mensagem do dia"),
            (GuildPermission.DeclareWar, "Declare war", "Declarar guerra"),
            (GuildPermission.EditIdentity, "Motto and emblem", "Lema e brasão"),
            (GuildPermission.Disband, "Disband the guild", "Desfazer a guilda"),
        };

        private static readonly (GuildWarState State, string En, string Pt)[] WarStateTexts =
        {
            (GuildWarState.Declared, "Declared", "Declarada"),
            (GuildWarState.Accepted, "Starting soon", "Começa em breve"),
            (GuildWarState.Active, "At war", "Em guerra"),
            (GuildWarState.Ended, "Ended", "Encerrada"),
            (GuildWarState.Declined, "Declined", "Recusada"),
            (GuildWarState.Cancelled, "Withdrawn", "Retirada"),
            (GuildWarState.Expired, "Unanswered", "Sem resposta"),
        };

        private static readonly (GuildWarEndReason Reason, string En, string Pt)[] WarEndTexts =
        {
            (GuildWarEndReason.None, "—", "—"),
            (GuildWarEndReason.TimeUp, "Time is up.", "O tempo acabou."),
            (GuildWarEndReason.ScoreReached, "Kill target reached.", "Meta de abates alcançada."),
            (GuildWarEndReason.Surrender, "Surrender.", "Rendição."),
        };

        private static readonly (GuildBankAction Action, string En, string Pt)[] BankActionTexts =
        {
            (GuildBankAction.DepositGold, "deposited gold", "depositou ouro"),
            (GuildBankAction.WithdrawGold, "withdrew gold", "sacou ouro"),
            (GuildBankAction.DepositItem, "deposited an item", "depositou item"),
            (GuildBankAction.WithdrawItem, "withdrew an item", "sacou item"),
            (GuildBankAction.ContributeGold, "contributed gold", "contribuiu com ouro"),
        };

        private static readonly (GuildEmblemShape Shape, string En, string Pt)[] ShapeTexts =
        {
            (GuildEmblemShape.Shield, "Shield", "Escudo"),
            (GuildEmblemShape.Circle, "Circle", "Círculo"),
            (GuildEmblemShape.Diamond, "Diamond", "Losango"),
            (GuildEmblemShape.Banner, "Banner", "Estandarte"),
            (GuildEmblemShape.Star, "Star", "Estrela"),
            (GuildEmblemShape.Lamp, "Lamp", "Lâmpada"),
        };

        private static readonly (GuildEmblemColor Color, string En, string Pt)[] ColorTexts =
        {
            (GuildEmblemColor.White, "White", "Branco"),
            (GuildEmblemColor.Gold, "Gold", "Dourado"),
            (GuildEmblemColor.Amber, "Amber", "Âmbar"),
            (GuildEmblemColor.Crimson, "Crimson", "Carmim"),
            (GuildEmblemColor.Violet, "Violet", "Violeta"),
            (GuildEmblemColor.Indigo, "Indigo", "Anil"),
            (GuildEmblemColor.Azure, "Azure", "Azul-celeste"),
            (GuildEmblemColor.Teal, "Teal", "Verde-água"),
            (GuildEmblemColor.Green, "Green", "Verde"),
            (GuildEmblemColor.Earth, "Earth", "Terra"),
            (GuildEmblemColor.Ash, "Ash", "Cinza"),
            (GuildEmblemColor.Black, "Black", "Preto"),
        };

        private static readonly (GuildEmblemSymbol Symbol, string En, string Pt)[] SymbolTexts =
        {
            (GuildEmblemSymbol.None, "None", "Nenhum"),
            (GuildEmblemSymbol.Sun, "Sun", "Sol"),
            (GuildEmblemSymbol.Moon, "Moon", "Lua"),
            (GuildEmblemSymbol.LightShip, "Ship of Light", "Barca de Luz"),
            (GuildEmblemSymbol.Column, "Column of Glory", "Coluna da Glória"),
            (GuildEmblemSymbol.Pearl, "Pearl", "Pérola"),
            (GuildEmblemSymbol.Tree, "Tree of Life", "Árvore da Vida"),
            (GuildEmblemSymbol.Wheel, "Wheel of Light", "Roda da Luz"),
            (GuildEmblemSymbol.Flame, "Flame", "Chama"),
            (GuildEmblemSymbol.Lotus, "Lotus", "Lótus"),
            (GuildEmblemSymbol.Eye, "Eye", "Olho"),
            (GuildEmblemSymbol.Wing, "Wing", "Asa"),
            (GuildEmblemSymbol.Crown, "Crown", "Coroa"),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(InviteReceived, "{0} invited you to the guild {1}.", "{0} convidou você para a guilda {1}."),
            new LocalizationEntry(MemberJoined, "{0} joined the guild.", "{0} entrou na guilda."),
            new LocalizationEntry(MemberLeft, "{0} left the guild.", "{0} saiu da guilda."),
            new LocalizationEntry(MemberKicked, "{0} was removed from the guild by {1}.", "{0} foi expulso da guilda por {1}."),
            new LocalizationEntry(RankChanged, "{0} is now {1}.", "{0} agora é {1}."),
            new LocalizationEntry(NewLeader, "{0} now leads the guild.", "{0} agora lidera a guilda."),
            new LocalizationEntry(LeaderInactiveHandover, "{0} was away for {1} days; {2} now leads the guild.",
                "{0} ficou {1} dias sem aparecer; {2} agora lidera a guilda."),
            new LocalizationEntry(LevelUp, "The guild reached level {0}.", "A guilda chegou ao nível {0}."),
            new LocalizationEntry(WarDeclared, "{0} declared war on {1}.", "{0} declarou guerra a {1}."),
            new LocalizationEntry(WarStartsSoon, "The war against {0} starts in {1} minutes. Nobody fights inside a city.",
                "A guerra contra {0} começa em {1} minutos. Na cidade ninguém luta."),
            new LocalizationEntry(WarScore, "War over: {0} {1} × {2} {3}.", "Fim da guerra: {0} {1} × {2} {3}."),
            new LocalizationEntry(WarDraw, "The war ended in a draw.", "A guerra terminou empatada."),
            new LocalizationEntry(WarWinner, "{0} won the war.", "{0} venceu a guerra."),
            new LocalizationEntry(MessageOfTheDayTitle, "Message of the day", "Mensagem do dia"),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            entries.AddRange(RefusalTexts.Select(t => new LocalizationEntry(RefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(RankTexts.Select(t => new LocalizationEntry(RankKey(t.Rank), t.En, t.Pt)));
            entries.AddRange(PermissionTexts.Select(t => new LocalizationEntry(PermissionKey(t.Permission), t.En, t.Pt)));
            entries.AddRange(WarStateTexts.Select(t => new LocalizationEntry(WarStateKey(t.State), t.En, t.Pt)));
            entries.AddRange(WarEndTexts.Select(t => new LocalizationEntry(WarEndKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(BankActionTexts.Select(t => new LocalizationEntry(BankActionKey(t.Action), t.En, t.Pt)));
            entries.AddRange(ShapeTexts.Select(t => new LocalizationEntry(ShapeKey(t.Shape), t.En, t.Pt)));
            entries.AddRange(ColorTexts.Select(t => new LocalizationEntry(ColorKey(t.Color), t.En, t.Pt)));
            entries.AddRange(SymbolTexts.Select(t => new LocalizationEntry(SymbolKey(t.Symbol), t.En, t.Pt)));
            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(MemberJoined)))
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

        public static string Refusal(LocalizationManager manager, GuildRefusal reason) => Get(manager, RefusalKey(reason));

        public static string Rank(LocalizationManager manager, GuildRank rank) => Get(manager, RankKey(rank));

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

        /// <summary>"AlreadyInGuild" → "already_in_guild".</summary>
        private static string Snake(string pascal)
        {
            var builder = new StringBuilder(pascal.Length + 8);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
