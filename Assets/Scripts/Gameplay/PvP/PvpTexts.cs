using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>
    /// Localization (en + pt-BR) of PvP: refusals (<see cref="PvpRefusal"/>), duel ends (<see cref="DuelEndReason"/>), PK
    /// standings (<see cref="PkStanding"/>), area kinds (<see cref="PvpAreaKind"/>), arena brackets, tiers and titles, and
    /// the boards. Same self-contained pattern as <c>PartyTexts</c>: merged into the live <see cref="LocalizationManager"/>
    /// on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class PvpTexts
    {
        /// <summary>"{0} desafiou você para um duelo."</summary>
        public const string ChallengeReceived = "pvp.duel.challenge_received";

        /// <summary>"O duelo começa em {0}…"</summary>
        public const string Countdown = "pvp.duel.countdown";

        /// <summary>"{0} venceu o duelo."</summary>
        public const string DuelWon = "pvp.duel.won";

        public const string DuelDraw = "pvp.duel.draw";

        /// <summary>"Você saiu da área do duelo" warning line for the HUD.</summary>
        public const string AreaWarning = "pvp.duel.area_warning";

        /// <summary>"Manchas: {0}. A próxima some em {1} min de jogo."</summary>
        public const string StainsStatus = "pvp.pk.status";

        public static string RefusalKey(PvpRefusal reason) => "pvp.refusal." + Snake(reason.ToString());

        public static string EndReasonKey(DuelEndReason reason) => "pvp.duel.end." + Snake(reason.ToString());

        public static string StandingKey(PkStanding standing) => "pvp.pk.standing." + Snake(standing.ToString());

        public static string AreaKey(PvpAreaKind kind) => "pvp.area." + Snake(kind.ToString());

        public static string BracketKey(ArenaBracket bracket) => "pvp.arena.bracket." + Snake(bracket.ToString());

        public static string ArenaTierKey(ArenaTier tier) => "pvp.arena.tier." + Snake(tier.ToString());

        public static string ArenaTitleKey(ArenaTier tier) => "pvp.arena.title." + Snake(tier.ToString());

        public static string BoardKey(RankingBoard board) => "pvp.ranking.board." + Snake(board.ToString());

        private static readonly (PvpRefusal Reason, string En, string Pt)[] RefusalTexts =
        {
            (PvpRefusal.None, "Done.", "Feito."),
            (PvpRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (PvpRefusal.CannotTargetSelf, "You cannot do that to yourself.", "Você não pode fazer isso consigo mesmo."),
            (PvpRefusal.InSafeZone, "No duels inside a city.", "Não há duelo dentro da cidade."),
            (PvpRefusal.TooFar, "Get closer (10 m) to challenge.", "Chegue mais perto (10 m) para desafiar."),
            (PvpRefusal.SameParty, "Leave the party first to duel a member.", "Saia do grupo antes de duelar com um membro."),
            (PvpRefusal.Busy, "You are already in a duel or in the arena.", "Você já está num duelo ou na arena."),
            (PvpRefusal.TargetBusy, "That player is already in a duel or in the arena.", "Esse jogador já está num duelo ou na arena."),
            (PvpRefusal.TargetBlocksDuels, "That player is not accepting duels.", "Esse jogador não está aceitando duelos."),
            (PvpRefusal.Dead, "Nobody can duel while down.", "Ninguém duela caído."),
            (PvpRefusal.NoChallenge, "There is no challenge to answer.", "Não há desafio para responder."),
            (PvpRefusal.NotParticipant, "You are not part of this duel.", "Você não faz parte deste duelo."),
            (PvpRefusal.ChallengeExpired, "The challenge expired.", "O desafio expirou."),
            (PvpRefusal.WrongPhase, "Not now.", "Agora não."),
            (PvpRefusal.AlreadyEnded, "That duel is already over.", "Esse duelo já acabou."),
            (PvpRefusal.AlreadyQueued, "Already in the arena queue.", "Já está na fila da arena."),
            (PvpRefusal.NotQueued, "Not in the arena queue.", "Não está na fila da arena."),
            (PvpRefusal.WrongTeamSize, "Wrong team size for this bracket.", "Tamanho de equipe errado para esta modalidade."),
            (PvpRefusal.TeamNotOneParty, "The whole team must be in the same party.", "A equipe toda precisa estar no mesmo grupo."),
            (PvpRefusal.OutlawCannotQueue, "Murderers cannot enter the ranked arena until their stains fade.",
                "Assassinos não entram na arena ranqueada até as manchas sumirem."),
            (PvpRefusal.LevelTooLow, "The ranked arena opens at level 30.", "A arena ranqueada abre no nível 30."),
            (PvpRefusal.NotInCity, "The bail is paid in a city.", "A fiança é paga numa cidade."),
            (PvpRefusal.NotEnoughGold, "Not enough gold.", "Ouro insuficiente."),
            (PvpRefusal.NoStains, "You have no stains.", "Você não tem manchas."),
        };

        private static readonly (DuelEndReason Reason, string En, string Pt)[] EndTexts =
        {
            (DuelEndReason.None, "", ""),
            (DuelEndReason.Declined, "The challenge was declined.", "O desafio foi recusado."),
            (DuelEndReason.Expired, "The challenge expired.", "O desafio expirou."),
            (DuelEndReason.Cancelled, "The challenge was withdrawn.", "O desafio foi retirado."),
            (DuelEndReason.Defeat, "Beaten down to the last breath.", "Derrotado até o último fôlego."),
            (DuelEndReason.Forfeit, "Gave up the duel.", "Desistiu do duelo."),
            (DuelEndReason.LeftArea, "Left the duel area.", "Saiu da área do duelo."),
            (DuelEndReason.TimeUp, "Time is up.", "O tempo acabou."),
            (DuelEndReason.Disconnected, "Disconnected.", "Desconectou."),
            (DuelEndReason.Interrupted, "The duel was interrupted.", "O duelo foi interrompido."),
        };

        private static readonly (PkStanding Standing, string En, string Pt)[] StandingTexts =
        {
            (PkStanding.Clean, "Clean", "Limpo"),
            (PkStanding.Stained, "Stained", "Manchado"),
            (PkStanding.Outlaw, "Outlaw", "Fora da lei"),
            (PkStanding.Murderer, "Murderer", "Assassino"),
        };

        private static readonly (PvpAreaKind Kind, string En, string Pt)[] AreaTexts =
        {
            (PvpAreaKind.Protected, "Protected land: no open PvP.", "Terra protegida: sem PvP aberto."),
            (PvpAreaKind.Frontier, "Frontier: open PvP from level 80; killing leaves a stain.",
                "Fronteira: PvP aberto a partir do nível 80; matar deixa mancha."),
            (PvpAreaKind.DisputeLand, "Disputed Lands: open PvP from level 80, no stain.",
                "Terras de Disputa: PvP aberto a partir do nível 80, sem mancha."),
        };

        private static readonly (ArenaBracket Bracket, string En, string Pt)[] BracketTexts =
        {
            (ArenaBracket.Solo, "1v1", "1x1"),
            (ArenaBracket.Duo, "2v2", "2x2"),
            (ArenaBracket.Trio, "3v3", "3x3"),
        };

        private static readonly (ArenaTier Tier, string En, string Pt, string TitleEn, string TitlePt)[] TierTexts =
        {
            (ArenaTier.None, "Unranked", "Sem classificação", "", ""),
            (ArenaTier.Centelha, "Spark", "Centelha", "Spark of the Arena", "Centelha da Arena"),
            (ArenaTier.Chama, "Flame", "Chama", "Flame of the Arena", "Chama da Arena"),
            (ArenaTier.Lampada, "Lamp", "Lâmpada", "Lamp of the Arena", "Lâmpada da Arena"),
            (ArenaTier.Estrela, "Star", "Estrela", "Star of the Arena", "Estrela da Arena"),
            (ArenaTier.ColunaDaGloria, "Column of Glory", "Coluna da Glória", "Of the Column of Glory", "Da Coluna da Glória"),
        };

        private static readonly (RankingBoard Board, string En, string Pt)[] BoardTexts =
        {
            (RankingBoard.Level, "Level", "Nível"),
            (RankingBoard.Pvp, "Arena", "Arena"),
            (RankingBoard.Events, "Events", "Eventos"),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(ChallengeReceived, "{0} challenged you to a duel.", "{0} desafiou você para um duelo."),
            new LocalizationEntry(Countdown, "The duel starts in {0}…", "O duelo começa em {0}…"),
            new LocalizationEntry(DuelWon, "{0} won the duel.", "{0} venceu o duelo."),
            new LocalizationEntry(DuelDraw, "The duel ended in a draw.", "O duelo terminou empatado."),
            new LocalizationEntry(AreaWarning, "Leaving the duel area loses the duel.", "Sair da área do duelo perde o duelo."),
            new LocalizationEntry(StainsStatus, "Stains: {0}. The next fades in {1} min of play.", "Manchas: {0}. A próxima some em {1} min de jogo."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            foreach (var t in RefusalTexts)
            {
                entries.Add(new LocalizationEntry(RefusalKey(t.Reason), t.En, t.Pt));
            }

            foreach (var t in EndTexts)
            {
                entries.Add(new LocalizationEntry(EndReasonKey(t.Reason), t.En, t.Pt));
            }

            foreach (var t in StandingTexts)
            {
                entries.Add(new LocalizationEntry(StandingKey(t.Standing), t.En, t.Pt));
            }

            foreach (var t in AreaTexts)
            {
                entries.Add(new LocalizationEntry(AreaKey(t.Kind), t.En, t.Pt));
            }

            foreach (var t in BracketTexts)
            {
                entries.Add(new LocalizationEntry(BracketKey(t.Bracket), t.En, t.Pt));
            }

            foreach (var t in TierTexts)
            {
                entries.Add(new LocalizationEntry(ArenaTierKey(t.Tier), t.En, t.Pt));
                entries.Add(new LocalizationEntry(ArenaTitleKey(t.Tier), t.TitleEn, t.TitlePt));
            }

            foreach (var t in BoardTexts)
            {
                entries.Add(new LocalizationEntry(BoardKey(t.Board), t.En, t.Pt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(ChallengeReceived)))
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

        public static string Refusal(LocalizationManager manager, PvpRefusal reason) => Get(manager, RefusalKey(reason));

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

        /// <summary>"OutlawCannotQueue" → "outlaw_cannot_queue".</summary>
        public static string Snake(string pascal)
        {
            if (string.IsNullOrEmpty(pascal))
            {
                return pascal;
            }

            var builder = new System.Text.StringBuilder(pascal.Length + 8);
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
