using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>
    /// Localization (en + pt-BR) of the scheduled events: names, phases, refusals, bands, enemy roles and announcements.
    /// Same self-contained pattern as <c>GuildTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class TimedEventTexts
    {
        /// <summary>"{0} começa em {1} min. Portão abre {2} min antes."</summary>
        public const string Announced = "timed_event.announce.soon";

        /// <summary>"O portão de {0} está aberto: {1} para entrar."</summary>
        public const string EntryOpen = "timed_event.announce.entry_open";

        /// <summary>"{0} começou."</summary>
        public const string Started = "timed_event.announce.started";

        /// <summary>"Próxima {0}: {1} (horário de Brasília)."</summary>
        public const string NextAt = "timed_event.next_at";

        /// <summary>"Onda {0} de {1}."</summary>
        public const string WaveOf = "timed_event.wave_of";

        /// <summary>"A Lâmpada do Portão: {0}%."</summary>
        public const string LampHealth = "timed_event.lamp_health";

        public const string LampOut = "timed_event.lamp_out";
        public const string GateAbandoned = "timed_event.gate_abandoned";
        public const string Victory = "timed_event.victory";
        public const string Defeat = "timed_event.defeat";

        /// <summary>"Ohyah acordou. Quem só olhar não leva nada."</summary>
        public const string OhyahAwake = "timed_event.ohyah_awake";

        public const string BemaTruce = "timed_event.bema_truce";
        public const string BemaVigil = "timed_event.bema_vigil";

        /// <summary>"{0}º lugar, {1} pontos: faixa {2}."</summary>
        public const string BoardLine = "timed_event.board_line";

        public static string NameKey(TimedEventKind kind) => "timed_event.name." + Snake(kind.ToString());
        public static string RefusalKey(TimedEventRefusal reason) => "timed_event.refusal." + Snake(reason.ToString());
        public static string PhaseKey(TimedEventPhase phase) => "timed_event.phase." + Snake(phase.ToString());
        public static string BandKey(TimedRewardBand band) => "timed_event.band." + Snake(band.ToString());
        public static string RoleKey(TimedEnemyRole role) => "timed_event.role." + Snake(role.ToString());
        public static string OutcomeKey(TimedEventOutcome outcome) => "timed_event.outcome." + Snake(outcome.ToString());

        private static readonly (TimedEventKind Kind, string En, string Pt)[] NameTexts =
        {
            (TimedEventKind.None, "No event", "Nenhum evento"),
            (TimedEventKind.VigiliaDaColuna, "Vigil of the Column", "Vigília da Coluna"),
            (TimedEventKind.CercoDaTreva, "Siege of the Darkness", "Cerco da Treva"),
            (TimedEventKind.CacadaAOhyah, "Hunt for Ohyah", "Caçada a Ohyah"),
        };

        private static readonly (TimedEventRefusal Reason, string En, string Pt)[] RefusalTexts =
        {
            (TimedEventRefusal.None, "Welcome in.", "Pode entrar."),
            (TimedEventRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (TimedEventRefusal.UnknownEvent, "That event does not exist.", "Esse evento não existe."),
            (TimedEventRefusal.EntryClosed, "The gate is closed. Wait for the next one.", "O portão está fechado. Espere a próxima."),
            (TimedEventRefusal.LevelTooLow, "You are not strong enough for this event yet.", "Você ainda não tem nível para este evento."),
            (TimedEventRefusal.AlreadyEnteredToday, "You already took part today. Come back tomorrow.", "Você já participou hoje. Volte amanhã."),
            (TimedEventRefusal.NoTicket, "You need a ticket to enter.", "Você precisa do ingresso para entrar."),
            (TimedEventRefusal.NotEnoughGold, "No ticket and not enough gold for the fee.", "Sem ingresso e sem ouro para a taxa."),
            (TimedEventRefusal.RoomFull, "Every room of your level is full.", "Todas as salas do seu nível estão cheias."),
            (TimedEventRefusal.NotPartyLeader, "Only the party leader takes the party in.", "Só o líder do grupo entra com o grupo."),
            (TimedEventRefusal.PartyMemberNotEligible, "Someone in your party cannot enter.", "Alguém do seu grupo não pode entrar."),
            (TimedEventRefusal.PartyRoomMismatch, "Your party falls into different rooms. Enter alone or regroup.", "Seu grupo cai em salas diferentes. Entre sozinho ou troque o grupo."),
            (TimedEventRefusal.SuspendedForBema, "During the Bema the Darkness does not come to the city.", "Durante o Bema a Treva não vem à cidade."),
            (TimedEventRefusal.NotInRoom, "You are not in any room of this event.", "Você não está em nenhuma sala deste evento."),
            (TimedEventRefusal.NotPresent, "Go to the event's gate first.", "Vá primeiro até o portão do evento."),
        };

        private static readonly (TimedEventPhase Phase, string En, string Pt)[] PhaseTexts =
        {
            (TimedEventPhase.Closed, "Closed", "Fechado"),
            (TimedEventPhase.Announced, "Coming soon", "Em breve"),
            (TimedEventPhase.EntryOpen, "Gate open", "Portão aberto"),
            (TimedEventPhase.Running, "Happening now", "Acontecendo"),
        };

        private static readonly (TimedRewardBand Band, string En, string Pt)[] BandTexts =
        {
            (TimedRewardBand.None, "No reward", "Sem prêmio"),
            (TimedRewardBand.Participation, "Took part", "Participação"),
            (TimedRewardBand.Bronze, "Bronze", "Bronze"),
            (TimedRewardBand.Silver, "Silver", "Prata"),
            (TimedRewardBand.Gold, "Gold", "Ouro"),
        };

        private static readonly (TimedEnemyRole Role, string En, string Pt)[] RoleTexts =
        {
            (TimedEnemyRole.Shade, "Shade", "Sombra"),
            (TimedEnemyRole.Runner, "Darkness Runner", "Corredor da Treva"),
            (TimedEnemyRole.Caster, "Whisperer", "Sussurrador"),
            (TimedEnemyRole.Brute, "Brute", "Bruto"),
            (TimedEnemyRole.Elite, "Herald of the Darkness", "Arauto da Treva"),
            (TimedEnemyRole.Archon, "Archon", "Arconte"),
        };

        private static readonly (TimedEventOutcome Outcome, string En, string Pt)[] OutcomeTexts =
        {
            (TimedEventOutcome.InProgress, "In progress", "Em andamento"),
            (TimedEventOutcome.Victory, "Victory", "Vitória"),
            (TimedEventOutcome.Defeat, "Defeat", "Derrota"),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(Announced, "{0} starts in {1} min. The gate opens {2} min before.", "{0} começa em {1} min. O portão abre {2} min antes."),
            new LocalizationEntry(EntryOpen, "The gate of the {0} is open: {1} to enter.", "O portão da {0} está aberto: {1} para entrar."),
            new LocalizationEntry(Started, "The {0} has begun.", "A {0} começou."),
            new LocalizationEntry(NextAt, "Next {0}: {1} (Brasília time).", "Próxima {0}: {1} (horário de Brasília)."),
            new LocalizationEntry(WaveOf, "Wave {0} of {1}.", "Onda {0} de {1}."),
            new LocalizationEntry(LampHealth, "The Gate Lamp: {0}%.", "A Lâmpada do Portão: {0}%."),
            new LocalizationEntry(LampOut, "The Gate Lamp went out. The Darkness took the gate this time.", "A Lâmpada do Portão apagou. Desta vez a Treva levou o portão."),
            new LocalizationEntry(GateAbandoned, "Nobody was left at the gate. The lamp could not hold alone.", "Não sobrou ninguém no portão. A lâmpada não aguentou sozinha."),
            new LocalizationEntry(Victory, "The light held! The lamps are still lit.", "A luz aguentou! As lâmpadas continuam acesas."),
            new LocalizationEntry(Defeat, "The Darkness won this round. The light comes back next time.", "A Treva venceu esta rodada. A luz volta na próxima."),
            new LocalizationEntry(OhyahAwake, "Ohyah, the old giant, woke up. Whoever only watches takes nothing home.", "Ohyah, o gigante antigo, acordou. Quem só olhar não leva nada."),
            new LocalizationEntry(BemaTruce, "Bema truce: the Darkness keeps away from the city all month.", "Trégua do Bema: a Treva fica longe da cidade o mês inteiro."),
            new LocalizationEntry(BemaVigil, "Bema Vigil at 23:00: two Vigils a day while the Bema lasts.", "Vigília do Bema às 23h: duas Vigílias por dia enquanto durar o Bema."),
            new LocalizationEntry(BoardLine, "{0}. place, {1} points: {2}.", "{0}º lugar, {1} pontos: {2}."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            entries.AddRange(NameTexts.Select(t => new LocalizationEntry(NameKey(t.Kind), t.En, t.Pt)));
            entries.AddRange(RefusalTexts.Select(t => new LocalizationEntry(RefusalKey(t.Reason), t.En, t.Pt)));
            entries.AddRange(PhaseTexts.Select(t => new LocalizationEntry(PhaseKey(t.Phase), t.En, t.Pt)));
            entries.AddRange(BandTexts.Select(t => new LocalizationEntry(BandKey(t.Band), t.En, t.Pt)));
            entries.AddRange(RoleTexts.Select(t => new LocalizationEntry(RoleKey(t.Role), t.En, t.Pt)));
            entries.AddRange(OutcomeTexts.Select(t => new LocalizationEntry(OutcomeKey(t.Outcome), t.En, t.Pt)));
            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Announced)))
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

        public static string Refusal(LocalizationManager manager, TimedEventRefusal reason) => Get(manager, RefusalKey(reason));

        public static string Name(LocalizationManager manager, TimedEventKind kind) => Get(manager, NameKey(kind));

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

        /// <summary>"AlreadyEnteredToday" → "already_entered_today".</summary>
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
