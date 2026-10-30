using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Localization (en + pt-BR) of the Vila shops added in 2026-10-30 (Ferraria do Mestre Adamas, Marcenaria do Mestre
    /// Omóforo, Costureira Mestra Narisah; ids severino/damiao/fatima): shop and keeper names, one line per cousin, the
    /// freguesia tiers (<see cref="ShopReputation"/>), the purchase notices, the Alforje de Feltro item (id
    /// <c>item_bornal_couro</c>) and the shop names of every house
    /// item a counter sells (<c>item.&lt;casa_id&gt;.*</c>, so the shop UI's <c>LocalizedName</c> finds them). Appended to
    /// <c>CityTexts.Entries</c> (one table for the city hub); kept apart so the city table stays readable.
    /// </summary>
    public static class CityShopTexts
    {
        /// <summary>{0} = rows added.</summary>
        public const string BornalDone = "city.shop.bornal_done";
        public const string BornalAlready = "city.shop.bornal_already";
        /// <summary>{0} = house item name.</summary>
        public const string HouseItemSent = "city.shop.house_item_sent";
        /// <summary>{0} = keeper/shop name, {1} = tier name.</summary>
        public const string ReputationUp = "city.reputation.up";
        /// <summary>{0} = tier name, {1} = points to the next tier.</summary>
        public const string ReputationStatus = "city.reputation.status";

        private static List<LocalizationEntry> _entries;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                Shop(CityShopCatalog.FerrariaSeverino, "Master Adamas's Smithy", "Ferraria do Mestre Adamas"),
                Shop(CityShopCatalog.MarcenariaDamiao, "Master Omophoros's Woodshop", "Marcenaria do Mestre Omóforo"),
                Shop(CityShopCatalog.CostureiraFatima, "Mistress Narisah, the Seamstress", "Costureira Mestra Narisah"),

                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.NewCustomer), "Visitor", "Visitante"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.Regular), "Regular", "Freguês"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.HouseRegular), "Friend of the House", "Amigo da Casa"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.Compadre), "Caravan Companion", "Companheiro de Caravana"),
                new LocalizationEntry(ReputationUp, "{0}: you are now {1} here!", "{0}: agora vocês são {1} aqui!"),
                new LocalizationEntry(ReputationStatus, "{0} · {1} gold to the next step", "{0} · faltam {1} de ouro para o próximo passo"),

                new LocalizationEntry(BornalDone, "Felt saddlebag: the backpack grew by {0} rows!", "Alforje de feltro: a mochila ganhou {0} linhas!"),
                new LocalizationEntry(BornalAlready, "The backpack can't grow any more: the saddlebag stays in the bag.", "A mochila não cresce mais: o alforje fica na mochila."),
                new LocalizationEntry(HouseItemSent, "{0} went to the house depot.", "{0} foi para o depósito da casa."),

                new LocalizationEntry(ContentText.ItemNameKey(CityShopCatalog.BornalItemId), "Felt Saddlebag", "Alforje de Feltro"),
                new LocalizationEntry(ContentText.ItemDescriptionKey(CityShopCatalog.BornalItemId),
                    "A felt saddlebag of the steppe nomads, stitched for the road. Buying it adds 2 rows to the backpack for good.",
                    "Alforje de feltro dos nômades da estepe, costurado para a estrada. Ao comprar, a mochila ganha 2 linhas para sempre."),
            };

            AddResident(entries, "severino", "Master Adamas, the Blacksmith", "Mestre Adamas, o Ferreiro",
                ("Bring that shield here, Joaquim. A dent today is a hole tomorrow.", "Traz esse escudo aqui, Joaquim. Amassado hoje é furo amanhã."),
                ("Your brother breaks, I mend and you heal him, Maria. Between the three of us Joaquim stays on his feet.", "Seu irmão quebra, eu conserto e você cura, Maria. Entre nós três o Joaquim fica de pé."),
                ("Arrowheads hammered cold, Maithe, the way your grandpa Jorge liked them.", "Ponta de flecha batida a frio, Maithe, do jeito que seu avô Jorge gostava."),
                ("Your sister says you took my bellows apart, Ícaro. ...Does it blow better now? Then leave it.", "Sua irmã disse que você desmontou meu fole, Ícaro. ...Está soprando melhor? Então deixa."),
                ("The sparks are pretty, Lavine, but they bite. Watch from the bench.", "As faíscas são bonitas, Lavine, mas mordem. Olha lá do banco."),
                ("Iron from the hills, walnut-wood fire: whoever buys here comes back. Right, old friend?", "Ferro da serra, fogo de nogueira: quem compra aqui volta. Não é, meu amigo?"));

            AddResident(entries, "damiao", "Master Omophoros, the Carpenter", "Mestre Omóforo, o Marceneiro",
                ("A good chair holds a guardian. I tested this one with a sack of beans and your grandpa.", "Cadeira boa aguenta guardião. Testei esta com um saco de feijão e com o seu avô."),
                ("Your grandpa Jorge's rocking chair came out of this shop, Maria. Forty years and it still doesn't creak.", "A cadeira de balanço do seu avô Jorge saiu desta oficina, Maria. Quarenta anos e não range."),
                ("You and Ícaro grew up climbing my lumber pile, Maithe. Now you come to buy. Time flies.", "Você e o Ícaro cresceram subindo na minha pilha de tábua, Maithe. Agora vêm comprar. O tempo voa."),
                ("A folding bed with springs? Draw it, Ícaro, and I'll carve it. The springs are on you.", "Uma cama dobrável de mola? Desenha, Ícaro, que eu entalho. A mola fica por sua conta."),
                ("I carved a little star on this headboard, Lavine. For the girl who counts them.", "Entalhei uma estrelinha nesta cabeceira, Lavine. Para a menina que conta estrelas."),
                ("Walnut, cedar and poplar: furniture that outlives whoever made it. I'll carry it to your door myself.", "Nogueira, cedro e choupo: móvel que dura mais que quem fez. E eu mesmo carrego até a porta."));

            AddResident(entries, "fatima", "Mistress Narisah, the Seamstress", "Mestra Narisah, a Costureira",
                ("Stand still, Joaquim, I need your shoulders. You've grown since your sister's last fitting.", "Fica parado, Joaquim, que eu preciso dos seus ombros. Você cresceu desde a última prova da sua irmã."),
                ("White linen for our healer, Maria, and a blue ribbon from Mariam to match. Lovely!", "Linho branco para a nossa curandeira, Maria, e uma fita azul da Mariam para combinar. Que lindeza!"),
                ("Rough cotton that doesn't snag on thorns, Maithe. The eldest needs clothes that last.", "Algodão cru que não engancha no espinho, Maithe. A mais velha precisa de roupa que dure."),
                ("More pockets? Ícaro, that jumpsuit already has eleven. ...Fine, twelve.", "Mais bolso? Ícaro, esse macacão já tem onze. ...Está bom, doze."),
                ("Purple ikat, the youngest's favourite. I saved the last length for you, Lavine.", "Ikat roxo, o preferido da caçula. Guardei o último corte para você, Lavine."),
                ("Needle, thread and patience: every seam has a story. And yours looks splendid today.", "Agulha, linha e paciência: cada costura tem uma história. E a sua hoje está esplêndida."));

            // Shop names of the house items on any counter (the bag only carries them for a moment: they go to the depot).
            var seen = new HashSet<string>();
            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                foreach (CityShopEntry entry in shop.Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem))
                {
                    HouseItemDefinition item = HouseCatalog.Get(entry.ItemId);
                    if (item == null || !seen.Add(item.ItemId))
                    {
                        continue;
                    }

                    entries.Add(new LocalizationEntry(ContentText.ItemNameKey(item.ItemId), HouseCatalog.EnglishName(item), item.NamePtBR));
                    entries.Add(new LocalizationEntry(ContentText.ItemDescriptionKey(item.ItemId),
                        HouseItemTexts.Describe(item, false) + " · Goes to the house depot.",
                        HouseItemTexts.Describe(item, true) + " · Vai para o depósito da casa."));
                }
            }

            return entries;
        }

        private static LocalizationEntry Shop(string shopId, string en, string pt) => new LocalizationEntry("city.shop." + shopId + ".name", en, pt);

        private static void AddResident(List<LocalizationEntry> entries, string residentId, string nameEn, string namePt,
            (string En, string Pt) joaquim, (string En, string Pt) maria, (string En, string Pt) maithe, (string En, string Pt) icaro,
            (string En, string Pt) lavine, (string En, string Pt) anyone)
        {
            string keyBase = "city.resident." + residentId;
            entries.Add(new LocalizationEntry(keyBase + ".name", nameEn, namePt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Joaquim), joaquim.En, joaquim.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Maria), maria.En, maria.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Maithe), maithe.En, maithe.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Icaro), icaro.En, icaro.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line." + Hero(CharacterId.Lavine), lavine.En, lavine.Pt));
            entries.Add(new LocalizationEntry(keyBase + ".line.any", anyone.En, anyone.Pt));
        }

        private static string Hero(CharacterId hero) => hero.ToString().ToLowerInvariant();

        /// <summary>pt-BR text of <paramref name="key"/> from <see cref="Entries"/>, or null.</summary>
        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            LocalizationEntry entry = Entries.FirstOrDefault(e => e.Key == key);
            return entry?.PtBR;
        }
    }
}
