using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Party
{
    /// <summary>
    /// Localization (en + pt-BR) of the party ("Grupo"): the refusal reasons (<see cref="PartyRefusal"/>), the XP cuts
    /// (<see cref="PartyXpCut"/>), the loot modes (<see cref="PartyLootMode"/>) and the family bonuses. Same
    /// self-contained pattern as <c>DuoTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class PartyTexts
    {
        public const string FamilyCompleteName = "party.bonus.family_complete.name";
        public const string FamilyCompleteDescription = "party.bonus.family_complete.desc";
        public const string SiblingBondName = "party.bonus.sibling_bond.name";
        public const string SiblingBondDescription = "party.bonus.sibling_bond.desc";

        /// <summary>"{0} convidou você para o grupo."; {0} = inviter.</summary>
        public const string InviteReceived = "party.invite.received";

        /// <summary>"{0} entrou no grupo."</summary>
        public const string MemberJoined = "party.member.joined";

        /// <summary>"{0} saiu do grupo."</summary>
        public const string MemberLeft = "party.member.left";

        /// <summary>"{0} agora lidera o grupo."</summary>
        public const string NewLeader = "party.member.new_leader";

        public static string RefusalKey(PartyRefusal reason) => "party.refusal." + Snake(reason.ToString());

        public static string XpCutKey(PartyXpCut cut) => "party.xp_cut." + Snake(cut.ToString());

        public static string LootModeKey(PartyLootMode mode) => "party.loot_mode." + Snake(mode.ToString());

        private static readonly (PartyRefusal Reason, string En, string Pt)[] RefusalTexts =
        {
            (PartyRefusal.None, "Done.", "Feito."),
            (PartyRefusal.InvalidPlayer, "That player was not found.", "Jogador não encontrado."),
            (PartyRefusal.NotLeader, "Only the party leader can do that.", "Só o líder do grupo pode fazer isso."),
            (PartyRefusal.PartyFull, "The party is full (5 members).", "O grupo está cheio (5 membros)."),
            (PartyRefusal.AlreadyInParty, "That player is already in a party.", "Esse jogador já está em um grupo."),
            (PartyRefusal.AlreadyInvited, "An invite is already waiting for that player.", "Esse jogador já tem um convite esperando."),
            (PartyRefusal.CannotTargetSelf, "You cannot do that to yourself.", "Você não pode fazer isso consigo mesmo."),
            (PartyRefusal.NoInvite, "There is no invite to answer.", "Não há convite para responder."),
            (PartyRefusal.InviteExpired, "The invite expired.", "O convite expirou."),
            (PartyRefusal.NotMember, "You are not in this party.", "Você não está neste grupo."),
            (PartyRefusal.TargetNotMember, "That player is not in your party.", "Esse jogador não está no seu grupo."),
            (PartyRefusal.Disbanded, "The party no longer exists.", "O grupo não existe mais."),
        };

        private static readonly (PartyXpCut Cut, string En, string Pt)[] XpCutTexts =
        {
            (PartyXpCut.None, "Shared XP.", "XP dividida."),
            (PartyXpCut.Dead, "No XP: you were down.", "Sem XP: você estava caído."),
            (PartyXpCut.TooFar, "No XP: too far from the fight.", "Sem XP: longe demais da luta."),
            (PartyXpCut.OutOfLevelBand, "No XP: more than 20 levels above the party's lowest member.",
                "Sem XP: mais de 20 níveis acima do menor membro do grupo."),
        };

        private static readonly (PartyLootMode Mode, string En, string Pt)[] LootModeTexts =
        {
            (PartyLootMode.FreeForAll, "Free for all", "Livre"),
            (PartyLootMode.RoundRobin, "Round robin", "Rodízio"),
            (PartyLootMode.NeedRoll, "Need roll", "Por necessidade"),
            (PartyLootMode.LeaderAssigns, "Leader assigns", "O líder distribui"),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(FamilyCompleteName, "Complete Family", "Família Completa"),
            new LocalizationEntry(FamilyCompleteDescription, "All five cousins together: +10% XP for everyone at the fight.",
                "Os cinco primos juntos: +10% de XP para todos na luta."),
            new LocalizationEntry(SiblingBondName, "Sibling Bond", "Laço de Irmãos"),
            new LocalizationEntry(SiblingBondDescription, "Maithe and Ícaro, or Joaquim and Maria, fighting side by side: +5% XP for both.",
                "Maithe e Ícaro, ou Joaquim e Maria, lutando lado a lado: +5% de XP para os dois."),
            new LocalizationEntry(InviteReceived, "{0} invited you to the party.", "{0} convidou você para o grupo."),
            new LocalizationEntry(MemberJoined, "{0} joined the party.", "{0} entrou no grupo."),
            new LocalizationEntry(MemberLeft, "{0} left the party.", "{0} saiu do grupo."),
            new LocalizationEntry(NewLeader, "{0} now leads the party.", "{0} agora lidera o grupo."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            foreach (var text in RefusalTexts)
            {
                entries.Add(new LocalizationEntry(RefusalKey(text.Reason), text.En, text.Pt));
            }

            foreach (var text in XpCutTexts)
            {
                entries.Add(new LocalizationEntry(XpCutKey(text.Cut), text.En, text.Pt));
            }

            foreach (var text in LootModeTexts)
            {
                entries.Add(new LocalizationEntry(LootModeKey(text.Mode), text.En, text.Pt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(FamilyCompleteName)))
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

        public static string Refusal(LocalizationManager manager, PartyRefusal reason) => Get(manager, RefusalKey(reason));

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

        /// <summary>"AlreadyInParty" → "already_in_party".</summary>
        private static string Snake(string pascal)
        {
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
