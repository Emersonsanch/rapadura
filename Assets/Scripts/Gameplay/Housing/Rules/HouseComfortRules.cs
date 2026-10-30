using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>The "efeito de ambiente" a furniture set shows with 6 pieces placed (Housing.md §6.2). Cosmetic only.</summary>
    public enum HouseSetAmbient
    {
        None,
        /// <summary>Varanda da Vó Edna: the warm lamp glows a little warmer (cheiro de café).</summary>
        WarmLamp,
        /// <summary>Oficina do Ícaro: sparks over the workbench.</summary>
        WorkshopSparks,
        /// <summary>Noite Rubra da Lavine: lilac fireflies.</summary>
        LilacFireflies,
        /// <summary>Cantinho do Caramelo: Caramelo wags his tail more while he naps.</summary>
        CarameloWag,
        /// <summary>Arraiá: soft forró (audio, H7 with the event music).</summary>
        Forro,
    }

    /// <summary>The small "mimo" of a complete set (§6.2). Only <see cref="RestedExtraFights"/> is live offline today.</summary>
    public enum HouseSetPerk
    {
        None,
        /// <summary>Luz da Maria: "Descansado" lasts +5 fights.</summary>
        RestedExtraFights,
        /// <summary>Cozinha da Vó: stove meals last +10% (pending: MealService hook).</summary>
        LongerMeals,
        /// <summary>Horta Caprichada: +1 harvest per bed (pending: harvests, §11.9).</summary>
        ExtraHarvest,
        /// <summary>Oficina do Ícaro: 10% chance the bench gives 1 wood back (pending: crafting hook).</summary>
        WoodRefund,
    }

    /// <summary>One of the 21 furniture sets (Housing.md §6.2, <c>CatalogoCasa.md</c> "Conjuntos").</summary>
    public sealed class HouseSetDefinition
    {
        public readonly string Id;
        public readonly HouseSetAmbient Ambient;
        public readonly HouseSetPerk Perk;

        public HouseSetDefinition(string id, HouseSetAmbient ambient = HouseSetAmbient.None, HouseSetPerk perk = HouseSetPerk.None)
        {
            Id = id;
            Ambient = ambient;
            Perk = perk;
        }

        /// <summary>"set_varanda_vo_zefa" → "varanda_vo_zefa".</summary>
        public string ShortId => Id.StartsWith("set_", StringComparison.Ordinal) ? Id.Substring(4) : Id;
        public string NameKey => "ui.house_life.set." + ShortId;
        /// <summary>The guest-book title a complete set gives the house ("Casa de caravaneiro").</summary>
        public string TitleKey => "ui.house_life.set_title." + ShortId;
    }

    /// <summary>The 21 sets, in the catalog's order.</summary>
    public static class HouseSetCatalog
    {
        public const int FirstStagePieces = 3;
        public const int SecondStagePieces = 6;
        public const int FirstStageComfort = 5;
        public const int SecondStageComfort = 12;
        public const int CompleteComfort = 25;
        public const int LuzMariaExtraFights = 5;

        public static readonly IReadOnlyList<HouseSetDefinition> All = new[]
        {
            new HouseSetDefinition("set_varanda_vo_zefa", HouseSetAmbient.WarmLamp),
            new HouseSetDefinition("set_tropeiro_vo_chico"),
            new HouseSetDefinition("set_casa_farinha"),
            new HouseSetDefinition("set_cozinha_vo", HouseSetAmbient.None, HouseSetPerk.LongerMeals),
            new HouseSetDefinition("set_oficina_icaro", HouseSetAmbient.WorkshopSparks, HouseSetPerk.WoodRefund),
            new HouseSetDefinition("set_ventania_maithe"),
            new HouseSetDefinition("set_luz_maria", HouseSetAmbient.None, HouseSetPerk.RestedExtraFights),
            new HouseSetDefinition("set_raizes_joaquim"),
            new HouseSetDefinition("set_noite_rubra_lavine", HouseSetAmbient.LilacFireflies),
            new HouseSetDefinition("set_cantinho_caramelo", HouseSetAmbient.CarameloWag),
            new HouseSetDefinition("set_mata_fechada"),
            new HouseSetDefinition("set_duna_dourada"),
            new HouseSetDefinition("set_pico_nevado"),
            new HouseSetDefinition("set_cristal_cavernas"),
            new HouseSetDefinition("set_feira_livre"),
            new HouseSetDefinition("set_cordel_xilogravura"),
            new HouseSetDefinition("set_horta_caprichada", HouseSetAmbient.None, HouseSetPerk.ExtraHarvest),
            new HouseSetDefinition("set_cristal_sol"),
            new HouseSetDefinition("set_arraia", HouseSetAmbient.Forro),
            new HouseSetDefinition("set_natal_sertanejo"),
            new HouseSetDefinition("set_frevo_maracatu"),
        };

        public static HouseSetDefinition Get(string setId) => string.IsNullOrEmpty(setId) ? null : All.FirstOrDefault(s => s.Id == setId);

        /// <summary>Distinct catalog items of <paramref name="setId"/> ("Todas" = every one of them placed).</summary>
        public static int PieceCount(string setId, IEnumerable<HouseItemDefinition> catalog)
        {
            return catalog == null || string.IsNullOrEmpty(setId) ? 0 : catalog.Where(i => i != null && i.SetId == setId).Select(i => i.ItemId).Distinct().Count();
        }

        /// <summary>0 = nothing, 1 = 3 pieces, 2 = 6 pieces, 3 = all pieces (the highest stage reached counts, not the sum).</summary>
        public static int StageOf(int placedPieces, int totalPieces)
        {
            if (totalPieces > 0 && placedPieces >= totalPieces) return 3;
            if (placedPieces >= SecondStagePieces) return 2;
            if (placedPieces >= FirstStagePieces) return 1;
            return 0;
        }

        public static int ComfortOfStage(int stage)
        {
            switch (stage)
            {
                case 1: return FirstStageComfort;
                case 2: return SecondStageComfort;
                case 3: return CompleteComfort;
                default: return 0;
            }
        }
    }

    /// <summary>A set's progress in the house.</summary>
    public sealed class HouseSetProgress
    {
        public HouseSetDefinition Set;
        public int Placed;
        public int Total;
        public int Stage;

        public int Comfort => HouseSetCatalog.ComfortOfStage(Stage);
        public bool AmbientActive => Stage >= 2 && Set.Ambient != HouseSetAmbient.None;
        public bool PerkActive => Stage >= 3 && Set.Perk != HouseSetPerk.None;
        public bool Complete => Stage >= 3;
    }

    /// <summary>The house's comfort, item by item and from sets.</summary>
    public sealed class HouseComfortReport
    {
        public int ItemComfort;
        public int SetComfort;
        public int WiltedPlants;
        public readonly List<HouseSetProgress> Sets = new List<HouseSetProgress>();

        public int Total => ItemComfort + SetComfort;
        public int Tier => HouseRestedRules.TierOf(Total);

        public bool HasPerk(HouseSetPerk perk) => Sets.Any(s => s.PerkActive && s.Set.Perk == perk);
        public bool HasAmbient(HouseSetAmbient ambient) => Sets.Any(s => s.AmbientActive && s.Set.Ambient == ambient);
        public IEnumerable<HouseSetProgress> Started => Sets.Where(s => s.Placed > 0);
        public IEnumerable<HouseSetProgress> Completed => Sets.Where(s => s.Complete);
    }

    /// <summary>
    /// Conforto (Housing.md §6.1): each placed piece adds its catalog comfort (the catalog already halves utilities),
    /// at most <see cref="MaxCopiesCounted"/> copies of the same id count, the Depósito counts nothing, a wilted plant
    /// counts half (§11.9). Set pieces (distinct ids placed, §6.2): 3 → +5, 6 → +12, all → +25. Unity-free.
    /// </summary>
    public static class HouseComfortRules
    {
        public const int MaxCopiesCounted = 2;

        /// <summary>A placed piece's comfort: a wilted plant loses half (rounded down: 5 → 2, 1 → 0) until it is watered.</summary>
        public static int PieceComfort(HouseItemDefinition item, bool wilted)
        {
            if (item == null) return 0;
            int comfort = Math.Max(0, item.Comfort);
            return wilted ? comfort / 2 : comfort;
        }

        public static HouseComfortReport Evaluate(IEnumerable<HousePlacedData> placed, Func<string, HouseItemDefinition> resolve,
            Func<int, bool> isWilted = null, IEnumerable<HouseItemDefinition> catalog = null)
        {
            var report = new HouseComfortReport();
            if (placed == null || resolve == null) return report;

            var byId = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            var setPieces = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (HousePlacedData piece in placed)
            {
                HouseItemDefinition item = piece != null ? resolve(piece.itemId) : null;
                if (item == null) continue;
                bool wilted = isWilted != null && HousePlantRules.IsPlant(item) && isWilted(piece.instanceId);
                if (wilted) report.WiltedPlants++;
                if (!byId.TryGetValue(item.ItemId, out List<int> values)) byId[item.ItemId] = values = new List<int>();
                values.Add(PieceComfort(item, wilted));
                if (!string.IsNullOrEmpty(item.SetId))
                {
                    if (!setPieces.TryGetValue(item.SetId, out HashSet<string> ids)) setPieces[item.SetId] = ids = new HashSet<string>(StringComparer.Ordinal);
                    ids.Add(item.ItemId);
                }
            }

            foreach (List<int> values in byId.Values)
            {
                report.ItemComfort += values.OrderByDescending(v => v).Take(MaxCopiesCounted).Sum();
            }

            IEnumerable<HouseItemDefinition> table = catalog ?? HouseCatalog.All;
            foreach (HouseSetDefinition set in HouseSetCatalog.All)
            {
                int count = setPieces.TryGetValue(set.Id, out HashSet<string> ids) ? ids.Count : 0;
                int total = HouseSetCatalog.PieceCount(set.Id, table);
                var progress = new HouseSetProgress { Set = set, Placed = count, Total = total, Stage = HouseSetCatalog.StageOf(count, total) };
                report.Sets.Add(progress);
                report.SetComfort += progress.Comfort;
            }

            return report;
        }
    }
}
