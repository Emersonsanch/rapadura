using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Items.Sets
{
    /// <summary>
    /// The short floating text of each armour-set bonus. Piece names and descriptions live in <c>ContentTexts</c> like every
    /// item's. Same self-contained pattern as the other *Texts helpers.
    /// </summary>
    public static class ArmorSetTexts
    {
        public const string BargeHour = "armor_set.barge_hour";
        public const string GlazedReflect = "armor_set.glazed_reflect";
        public const string ReopenedSeal = "armor_set.reopened_seal";
        public const string ColumnRay = "armor_set.column_ray";
        public const string ClosingCoral = "armor_set.closing_coral";
        public const string LotusSap = "armor_set.lotus_sap";
        public const string PamirBreath = "armor_set.pamir_breath";
        public const string BronzeGong = "armor_set.bronze_gong";
        public const string NartGallop = "armor_set.nart_gallop";
        public const string GardenDew = "armor_set.garden_dew";
        public const string BurningSand = "armor_set.burning_sand";
        public const string KaggenGust = "armor_set.kaggen_gust";
        public const string LitWick = "armor_set.lit_wick";
        public const string HermitageToll = "armor_set.hermitage_toll";
        public const string LanternWarmth = "armor_set.lantern_warmth";
        public const string AnvilEmber = "armor_set.anvil_ember";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(BargeHour, "Hour of the Barge!", "Hora da Barca!"),
            new LocalizationEntry(GlazedReflect, "Glazed Reflection!", "Reflexo Vidrado!"),
            new LocalizationEntry(ReopenedSeal, "Seal Reopened!", "Selo Reaberto!"),
            new LocalizationEntry(ColumnRay, "Ray of the Column!", "Raio da Coluna!"),
            new LocalizationEntry(ClosingCoral, "Closing Coral!", "Coral que Fecha!"),
            new LocalizationEntry(LotusSap, "Lotus Sap!", "Seiva do Lótus!"),
            new LocalizationEntry(PamirBreath, "Pamir Breath!", "Fôlego do Pamir!"),
            new LocalizationEntry(BronzeGong, "Bronze Gong!", "Gongo de Bronze!"),
            new LocalizationEntry(NartGallop, "Gallop of the Narts!", "Galope dos Nartes!"),
            new LocalizationEntry(GardenDew, "Garden Dew!", "Orvalho do Jardim!"),
            new LocalizationEntry(BurningSand, "Burning Sand!", "Areia que Queima!"),
            new LocalizationEntry(KaggenGust, "Kaggen Gust!", "Rajada do Kaggen!"),
            new LocalizationEntry(LitWick, "Wick Alight!", "Pavio Aceso!"),
            new LocalizationEntry(HermitageToll, "Toll of the Hermitages!", "Badalada das Ermidas!"),
            new LocalizationEntry(LanternWarmth, "Lantern Warmth!", "Calor da Lanterna!"),
            new LocalizationEntry(AnvilEmber, "Anvil Ember!", "Brasa da Bigorna!"),
        };

        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(BargeHour)))
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

            LocalizationEntry entry = Entries.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.PtBR : key;
        }
    }
}
