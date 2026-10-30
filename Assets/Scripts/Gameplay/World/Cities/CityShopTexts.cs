using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Localization (en + pt-BR) of the Vila shops added in 2026-10-30 (Ferraria do Seu Severino, Marcenaria do Mestre
    /// Damião, Costureira Dona Fátima): shop and keeper names, one line per cousin, the freguesia tiers
    /// (<see cref="ShopReputation"/>), the purchase notices, the Bornal de couro item and the shop names of every house
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
                Shop(CityShopCatalog.FerrariaSeverino, "Severino's Smithy", "Ferraria do Seu Severino"),
                Shop(CityShopCatalog.MarcenariaDamiao, "Master Damião's Woodshop", "Marcenaria do Mestre Damião"),
                Shop(CityShopCatalog.CostureiraFatima, "Dona Fátima, the Seamstress", "Costureira Dona Fátima"),

                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.NewCustomer), "New customer", "Freguês novo"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.Regular), "Regular", "Freguês"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.HouseRegular), "House regular", "Freguês de casa"),
                new LocalizationEntry(ShopReputation.TierNameKey(ShopReputation.Compadre), "Family friend", "Compadre"),
                new LocalizationEntry(ReputationUp, "{0}: you are now a {1} here!", "{0}: agora vocês são {1} aqui!"),
                new LocalizationEntry(ReputationStatus, "{0} · {1} gold to the next step", "{0} · faltam {1} de ouro para o próximo passo"),

                new LocalizationEntry(BornalDone, "Leather satchel: the backpack grew by {0} rows!", "Bornal de couro: a mochila ganhou {0} linhas!"),
                new LocalizationEntry(BornalAlready, "The backpack can't grow any more: the satchel stays in the bag.", "A mochila não cresce mais: o bornal fica na mochila."),
                new LocalizationEntry(HouseItemSent, "{0} went to the house depot.", "{0} foi para o depósito da casa."),

                new LocalizationEntry(ContentText.ItemNameKey(CityShopCatalog.BornalItemId), "Leather Satchel", "Bornal de couro"),
                new LocalizationEntry(ContentText.ItemDescriptionKey(CityShopCatalog.BornalItemId),
                    "A cowhide satchel stitched for the road. Buying it adds 2 rows to the backpack for good.",
                    "Bornal de couro de boi costurado para a estrada. Ao comprar, a mochila ganha 2 linhas para sempre."),
            };

            AddResident(entries, "severino", "Severino, the Blacksmith", "Seu Severino, o Ferreiro",
                ("Bring that shield here, Joaquim. A dent today is a hole tomorrow.", "Traz esse escudo aqui, Joaquim. Amassado hoje é furo amanhã."),
                ("Your brother breaks, I mend and you heal him, Maria. Between the three of us Joaquim stays on his feet.", "Seu irmão quebra, eu conserto e você cura, Maria. Entre nós três o Joaquim fica de pé."),
                ("Arrowheads hammered cold, Maithe, the way your grandpa Jorge liked them.", "Ponta de flecha batida a frio, Maithe, do jeito que seu avô Jorge gostava."),
                ("Your sister says you took my bellows apart, Ícaro. ...Does it blow better now? Then leave it.", "Sua irmã disse que você desmontou meu fole, Ícaro. ...Está soprando melhor? Então deixa."),
                ("The sparks are pretty, Lavine, but they bite. Watch from the bench.", "As faíscas são bonitas, Lavine, mas mordem. Olha lá do banco."),
                ("Iron from the hills, angico fire: whoever buys here comes back.", "Ferro da serra, fogo de angico: quem compra aqui volta."));

            AddResident(entries, "damiao", "Master Damião, the Carpenter", "Mestre Damião, o Marceneiro",
                ("A good chair holds a guardian. I tested this one with a sack of beans and your grandpa.", "Cadeira boa aguenta guardião. Testei esta com um saco de feijão e com o seu avô."),
                ("Your grandma's rocking chair came out of this shop, Maria. Forty years and it still doesn't creak.", "A cadeira de balanço da sua avó saiu desta oficina, Maria. Quarenta anos e não range."),
                ("You and Ícaro grew up climbing my lumber pile, Maithe. Now you come to buy. Time flies.", "Você e o Ícaro cresceram subindo na minha pilha de tábua, Maithe. Agora vêm comprar. O tempo voa."),
                ("A folding bed with springs? Draw it, Ícaro, and I'll carve it. The springs are on you.", "Uma cama dobrável de mola? Desenha, Ícaro, que eu entalho. A mola fica por sua conta."),
                ("I carved a little star on this headboard, Lavine. For the girl who counts them.", "Entalhei uma estrelinha nesta cabeceira, Lavine. Para a menina que conta estrelas."),
                ("Aroeira, cedar and ipê: furniture that outlives whoever made it.", "Aroeira, cedro e ipê: móvel que dura mais que quem fez."));

            AddResident(entries, "fatima", "Dona Fátima, the Seamstress", "Dona Fátima, a Costureira",
                ("Stand still, Joaquim, I need your shoulders. You've grown since your sister's last fitting.", "Fica parado, Joaquim, que eu preciso dos seus ombros. Você cresceu desde a última prova da sua irmã."),
                ("White linen for the priestess, Maria, and a blue ribbon from Dona Cida to match.", "Linho branco para a sacerdotisa, Maria, e uma fita azul da Dona Cida para combinar."),
                ("Rough cotton that doesn't snag on thorns, Maithe. The eldest needs clothes that last.", "Algodão cru que não engancha no espinho, Maithe. A mais velha precisa de roupa que dure."),
                ("More pockets? Ícaro, that jumpsuit already has eleven. ...Fine, twelve.", "Mais bolso? Ícaro, esse macacão já tem onze. ...Está bom, doze."),
                ("Purple chintz, the youngest's favourite. I saved the last length for you, Lavine.", "Chita roxa, a preferida da caçula. Guardei o último corte para você, Lavine."),
                ("Needle, thread and patience: every seam has a story.", "Agulha, linha e paciência: cada costura tem um causo."));

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
