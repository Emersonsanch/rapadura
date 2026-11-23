using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the trade between players: every <see cref="TradeRefusal"/> and
    /// <see cref="TradeWarning"/> plus the labels of the final summary. Same self-contained pattern as the other *Texts
    /// helpers (<c>EncumbranceTexts</c>): merged into the live <see cref="LocalizationManager"/> on first use (keys it
    /// already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class TradeTexts
    {
        public const string SummaryTitle = "ui.trade.summary.title";
        public const string SummaryYouGive = "ui.trade.summary.you_give";
        public const string SummaryYouGet = "ui.trade.summary.you_get";
        /// <summary>{0} = gold.</summary>
        public const string SummaryGold = "ui.trade.summary.gold";
        public const string SummaryNothing = "ui.trade.summary.nothing";
        public const string Lock = "ui.trade.lock";
        public const string Confirm = "ui.trade.confirm";
        public const string Cancel = "ui.trade.cancel";
        /// <summary>{0} = player name.</summary>
        public const string PartnerLocked = "ui.trade.partner_locked";
        public const string ChangedUnlocked = "ui.trade.changed_unlocked";
        public const string Completed = "ui.trade.completed";
        /// <summary>{0} = gold left today, {1} = item lines left today.</summary>
        public const string NewAccountLeft = "ui.trade.new_account_left";

        public static string RefusalKey(TradeRefusal refusal) => "ui.trade.refusal." + Snake(refusal.ToString());
        public static string WarningKey(TradeWarning warning) => "ui.trade.warning." + Snake(warning.ToString());

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(SummaryTitle, "Trade summary", "Resumo da troca"),
            new LocalizationEntry(SummaryYouGive, "You give", "Você entrega"),
            new LocalizationEntry(SummaryYouGet, "You receive", "Você recebe"),
            new LocalizationEntry(SummaryGold, "{0} gold", "{0} de ouro"),
            new LocalizationEntry(SummaryNothing, "Nothing", "Nada"),
            new LocalizationEntry(Lock, "Lock", "Travar"),
            new LocalizationEntry(Confirm, "Confirm trade", "Confirmar troca"),
            new LocalizationEntry(Cancel, "Cancel", "Cancelar"),
            new LocalizationEntry(PartnerLocked, "{0} locked the trade.", "{0} travou a troca."),
            new LocalizationEntry(ChangedUnlocked, "The offer changed: both locks were undone. Check it again.",
                "A oferta mudou: as duas travas foram desfeitas. Confiram de novo."),
            new LocalizationEntry(Completed, "Trade done.", "Troca feita."),
            new LocalizationEntry(NewAccountLeft, "New account: today you can still trade {0} gold and {1} items.",
                "Conta nova: hoje ainda dá para trocar {0} de ouro e {1} itens."),

            // ---- Refusals: opening ----
            R(TradeRefusal.SamePlayer, "You cannot trade with yourself.", "Não dá para trocar consigo mesmo."),
            R(TradeRefusal.PartnerUnavailable, "That player cannot trade right now.", "Esse jogador não pode trocar agora."),
            R(TradeRefusal.AlreadyTrading, "One of you is already in another trade.", "Um de vocês já está em outra troca."),
            R(TradeRefusal.TooFar, "Too far away: get closer, or meet in the same city.",
                "Longe demais: cheguem mais perto ou se encontrem na mesma cidade."),
            R(TradeRefusal.InCombat, "No trading in the middle of a fight. Wait a few seconds.",
                "Não dá para trocar no meio da luta. Esperem alguns segundos."),

            // ---- Refusals: session ----
            R(TradeRefusal.SessionClosed, "This trade is closed.", "Esta troca já foi fechada."),
            R(TradeRefusal.Expired, "The trade expired: nobody moved for too long.", "A troca expirou: ninguém mexeu por muito tempo."),
            R(TradeRefusal.AlreadySettled, "This trade was already done.", "Esta troca já foi concluída."),
            R(TradeRefusal.NotLocked, "Both sides must lock before confirming.", "Os dois lados precisam travar antes de confirmar."),
            R(TradeRefusal.StaleSummary, "The offer changed since you looked. Check the summary again.",
                "A oferta mudou desde que você olhou. Confira o resumo de novo."),
            R(TradeRefusal.NotConfirmed, "Both sides must confirm the summary.", "Os dois lados precisam confirmar o resumo."),
            R(TradeRefusal.EmptyTrade, "Nobody put anything in the trade.", "Ninguém pôs nada na troca."),

            // ---- Refusals: putting items and gold ----
            R(TradeRefusal.InvalidSlot, "That item is not in the backpack.", "Esse item não está na mochila."),
            R(TradeRefusal.EmptySlot, "That space of the backpack is empty.", "Esse espaço da mochila está vazio."),
            R(TradeRefusal.AlreadyOffered, "That stack is already in the trade.", "Essa pilha já está na troca."),
            R(TradeRefusal.InvalidQuantity, "Invalid quantity.", "Quantidade inválida."),
            R(TradeRefusal.NotEnoughItems, "You do not have that many.", "Você não tem tudo isso."),
            R(TradeRefusal.TooManyLines, "The trade window is full.", "A janela de troca está cheia."),
            R(TradeRefusal.GoldItemUseGoldField, "Gold goes in the gold field, not as an item.", "O ouro vai no campo de ouro, não como item."),
            R(TradeRefusal.NotEnoughGold, "You do not have that much gold.", "Você não tem todo esse ouro."),
            R(TradeRefusal.GoldOverTradeLimit, "Too much gold for a single trade.", "Ouro demais para uma troca só."),

            // ---- Refusals: cannot be traded ----
            R(TradeRefusal.UnknownItem, "Unknown item: it cannot be traded.", "Item desconhecido: não pode ser trocado."),
            R(TradeRefusal.Bound, "This item is bound to you and cannot be traded.", "Este item é vinculado a você e não pode ser trocado."),
            R(TradeRefusal.StoryItem, "Story items stay with whoever earned them.", "Itens da história ficam com quem os conquistou."),
            R(TradeRefusal.FamilyRelic, "Relics and family keepsakes are never traded.", "Relíquias e lembranças da família não se trocam."),
            R(TradeRefusal.AchievementItem, "Achievement items cannot be traded.", "Itens de conquista não podem ser trocados."),
            R(TradeRefusal.MountItem, "A mount stays with the cousin who tamed it.", "A montaria fica com o primo que a amansou."),
            R(TradeRefusal.HouseItem, "House items stay in the house.", "Itens da casa ficam na casa."),
            R(TradeRefusal.Currency, "This currency cannot be traded.", "Esta moeda não pode ser trocada."),

            // ---- Refusals: settlement ----
            R(TradeRefusal.ItemNoLongerHeld, "An item of the trade is no longer there. The trade was unlocked.",
                "Um item da troca não está mais lá. A troca foi destravada."),
            R(TradeRefusal.ItemChanged, "An item of the trade changed. The trade was unlocked.",
                "Um item da troca mudou. A troca foi destravada."),
            R(TradeRefusal.GoldNoLongerHeld, "The gold of the trade is no longer there. The trade was unlocked.",
                "O ouro da troca não está mais lá. A troca foi destravada."),
            R(TradeRefusal.GoldOverHeldLimit, "That would be more gold than one hero can carry.", "Seria mais ouro do que um herói pode carregar."),
            R(TradeRefusal.BagFull, "No room in the backpack for what comes in.", "Não cabe na mochila o que vai chegar."),
            R(TradeRefusal.TooHeavy, "Too heavy: that would go over the carry capacity.", "Pesado demais: passaria da carga que dá para levar."),
            R(TradeRefusal.NewAccountGoldLimit, "New account: today's gold trading limit was reached.",
                "Conta nova: o limite de ouro trocado hoje foi atingido."),
            R(TradeRefusal.NewAccountItemLimit, "New account: today's item trading limit was reached.",
                "Conta nova: o limite de itens trocados hoje foi atingido."),

            // ---- Warnings ----
            W(TradeWarning.RareItemGiven, "Careful: you are giving a rare item.", "Atenção: você está entregando um item raro."),
            W(TradeWarning.GivingForNothing, "Careful: you give and receive nothing in return.", "Atenção: você entrega e não recebe nada em troca."),
            W(TradeWarning.LargeGold, "Careful: you are giving a lot of gold.", "Atenção: você está entregando muito ouro."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

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

        public static string Get(LocalizationManager manager, TradeRefusal refusal) =>
            refusal == TradeRefusal.None ? string.Empty : Get(manager, RefusalKey(refusal));

        public static string Get(LocalizationManager manager, TradeWarning warning) => Get(manager, WarningKey(warning));

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        private static LocalizationEntry R(TradeRefusal refusal, string en, string pt) => new LocalizationEntry(RefusalKey(refusal), en, pt);
        private static LocalizationEntry W(TradeWarning warning, string en, string pt) => new LocalizationEntry(WarningKey(warning), en, pt);

        /// <summary>"GoldItemUseGoldField" → "gold_item_use_gold_field".</summary>
        private static string Snake(string pascal)
        {
            var sb = new StringBuilder(pascal.Length + 8);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        sb.Append('_');
                    }

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
