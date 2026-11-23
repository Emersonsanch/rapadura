using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Market
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the market: every <see cref="MarketRefusal"/> and <see cref="MailKind"/>. Same
    /// self-contained pattern as <see cref="Trade.TradeTexts"/>: merged into the live <see cref="LocalizationManager"/> on
    /// first use (keys it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class MarketTexts
    {
        public static string RefusalKey(MarketRefusal refusal) => "ui.market.refusal." + MarketRecord.Snake(refusal.ToString());
        public static string MailKey(MailKind kind) => "ui.market.mail." + MarketRecord.Snake(kind.ToString());

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            R(MarketRefusal.NotInCity, "Only inside a city's safe zone.", "Só dentro da zona segura de uma cidade."),
            R(MarketRefusal.WrongCity, "That is in another city.", "Isso fica em outra cidade."),
            R(MarketRefusal.NotAvailable, "Not available right now.", "Não dá agora."),
            R(MarketRefusal.InvalidSpot, "That is not a stall spot.", "Esse não é um lugar de barraca."),
            R(MarketRefusal.SpotTaken, "Someone is already using that spot.", "Alguém já está usando esse lugar."),
            R(MarketRefusal.SpotNotReserved, "Pick a free stall spot first.", "Escolha primeiro um lugar de barraca livre."),
            R(MarketRefusal.AlreadyHasStall, "You already have a stall open.", "Você já tem uma barraca aberta."),
            R(MarketRefusal.OwnStall, "That is your own stall.", "Essa barraca é sua."),
            R(MarketRefusal.StallClosed, "This stall is closed.", "Essa barraca fechou."),
            R(MarketRefusal.NotOwner, "Only the owner can do that.", "Só o dono pode fazer isso."),
            R(MarketRefusal.NoLines, "Put at least one item up.", "Coloque pelo menos um item."),
            R(MarketRefusal.TooManyLines, "No more room on the stall.", "Não cabe mais nada na barraca."),
            R(MarketRefusal.InvalidSlot, "That item is not there.", "Esse item não está aí."),
            R(MarketRefusal.EmptySlot, "That space is empty.", "Esse espaço está vazio."),
            R(MarketRefusal.AlreadyListed, "That item is already up.", "Esse item já foi colocado."),
            R(MarketRefusal.InvalidQuantity, "Invalid amount.", "Quantidade inválida."),
            R(MarketRefusal.NotEnoughItems, "You do not have that many.", "Você não tem tudo isso."),
            R(MarketRefusal.InvalidPrice, "Invalid price.", "Preço inválido."),
            R(MarketRefusal.NotTradeable, "That item cannot be sold to other players.", "Esse item não pode ser vendido para outros jogadores."),
            R(MarketRefusal.ItemChanged, "That item changed. Look again.", "Esse item mudou. Olhe de novo."),
            R(MarketRefusal.SoldOut, "Sold out! Someone was quicker.", "Esgotou! Alguém foi mais rápido."),
            R(MarketRefusal.NotEnoughStock, "There are not that many left.", "Não sobrou tanto assim."),
            R(MarketRefusal.PriceChanged, "The price changed. Look again.", "O preço mudou. Olhe de novo."),
            R(MarketRefusal.NotEnoughGold, "Not enough gold.", "Ouro insuficiente."),
            R(MarketRefusal.GoldOverTradeLimit, "Too much gold for one purchase.", "Ouro demais para uma compra só."),
            R(MarketRefusal.GoldOverHeldLimit, "That would be more gold than one hero can carry.", "Seria mais ouro do que um herói pode carregar."),
            R(MarketRefusal.BagFull, "No room in the backpack.", "Não cabe na mochila."),
            R(MarketRefusal.TooHeavy, "Too heavy to carry.", "Pesado demais para levar."),
            R(MarketRefusal.DuplicateRequest, "Already done.", "Isso já foi feito."),
            R(MarketRefusal.InvalidDuration, "Pick 12, 24 or 48 hours.", "Escolha 12, 24 ou 48 horas."),
            R(MarketRefusal.BuyoutBelowStart, "The buy-now price must be at least the starting bid.", "O preço de compra imediata precisa ser pelo menos o lance inicial."),
            R(MarketRefusal.TooManyListings, "You have too many items at the auction house.", "Você já tem itens demais na casa de leilões."),
            R(MarketRefusal.ListingNotFound, "That auction is gone.", "Esse leilão não existe mais."),
            R(MarketRefusal.ListingEnded, "That auction has ended.", "Esse leilão já terminou."),
            R(MarketRefusal.OwnListing, "You cannot bid on your own item.", "Não dá para dar lance no próprio item."),
            R(MarketRefusal.BidTooLow, "Bid higher than that.", "Dê um lance maior."),
            R(MarketRefusal.AlreadyHighestBidder, "Your bid is already the highest.", "Seu lance já é o maior."),
            R(MarketRefusal.HasBids, "Someone already bid: it cannot be cancelled.", "Alguém já deu lance: não dá para cancelar."),
            R(MarketRefusal.NewAccountGoldLimit, "New account: today's gold limit was reached.", "Conta nova: o limite de ouro de hoje foi atingido."),
            R(MarketRefusal.NewAccountItemLimit, "New account: today's item limit was reached.", "Conta nova: o limite de itens de hoje foi atingido."),
            R(MarketRefusal.SellerAccountLimit, "The seller cannot sell more today.", "O vendedor não pode vender mais hoje."),
            R(MarketRefusal.NewAccountPriceLimit, "New account: that price is too high for now.", "Conta nova: esse preço ainda é alto demais."),
            R(MarketRefusal.MailNotFound, "That mail is gone.", "Essa carta não existe mais."),
            R(MarketRefusal.NotRecipient, "That mail is not yours.", "Essa carta não é sua."),
            R(MarketRefusal.MailExpired, "That mail expired and went back to your chest.", "Essa carta expirou e voltou para o seu baú."),

            M(MailKind.StallReturn, "Your stall closed: here is what was left.", "Sua barraca fechou: aqui está o que sobrou."),
            M(MailKind.StallProceeds, "Your stall's sales.", "As vendas da sua barraca."),
            M(MailKind.AuctionWon, "You won the auction!", "Você ganhou o leilão!"),
            M(MailKind.AuctionProceeds, "Your item sold at the auction house.", "Seu item foi vendido na casa de leilões."),
            M(MailKind.AuctionUnsold, "Nobody bid: your item is back.", "Ninguém deu lance: seu item voltou."),
            M(MailKind.AuctionOutbidRefund, "Someone bid higher: here is your gold back.", "Alguém deu um lance maior: aqui está seu ouro de volta."),
            M(MailKind.AuctionCancelled, "Auction cancelled: your item is back.", "Leilão cancelado: seu item voltou."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

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

        public static string Get(LocalizationManager manager, MarketRefusal refusal) =>
            refusal == MarketRefusal.None ? string.Empty : Get(manager, RefusalKey(refusal));

        public static string Get(LocalizationManager manager, MailKind kind) => Get(manager, MailKey(kind));

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

        private static LocalizationEntry R(MarketRefusal refusal, string en, string pt) => new LocalizationEntry(RefusalKey(refusal), en, pt);
        private static LocalizationEntry M(MailKind kind, string en, string pt) => new LocalizationEntry(MailKey(kind), en, pt);
    }
}
