using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    public enum HouseQuestKind
    {
        /// <summary>Bring materials to Vó Edna (paid from the bag/chest when claimed).</summary>
        Deliver,
        /// <summary>Done by reaching a house level (the upgrade is the quest's scene).</summary>
        Upgrade,
        /// <summary>Reach a level and have cousins sign the guest book ("Casa Cheia").</summary>
        UpgradeAndGuests,
    }

    public enum HouseQuestState
    {
        Locked,
        /// <summary>Open, objective not met yet.</summary>
        Active,
        /// <summary>Objective met: deliver/claim.</summary>
        Ready,
        Completed,
    }

    /// <summary>One of the "Missões da Casa" 2–6 (Housing.md §7.3); #1 "Um Cantinho Seu" is <see cref="HouseRules"/>.</summary>
    public sealed class HouseQuestDefinition
    {
        public int Number;
        public string Id;
        public HouseQuestKind Kind;
        /// <summary>Quest that must be done first (null = "Um Cantinho Seu").</summary>
        public string AfterQuestId;
        public string RequiredBossId;
        public int RequiredHouseLevel;
        public int RequiredCousins;
        public HouseMaterialCost[] Deliver = Array.Empty<HouseMaterialCost>();
        /// <summary>House items, to the Depósito.</summary>
        public string[] Rewards = Array.Empty<string>();
        /// <summary>Wardrobe pieces (<c>CatalogoRoupas.md</c>); kept for H5, recorded as owed until the wardrobe exists.</summary>
        public string[] WardrobeRewards = Array.Empty<string>();

        public string NameKey => "ui.house_life.quest." + ShortId + ".name";
        public string GoalKey => "ui.house_life.quest." + ShortId + ".goal";
        public string ShortId => Id.StartsWith("house_quest_", StringComparison.Ordinal) ? Id.Substring("house_quest_".Length) : Id;
    }

    /// <summary>What <see cref="HouseQuestChain.StateOf"/> needs to know.</summary>
    public struct HouseQuestContext
    {
        public bool FirstQuestDone;
        public int HouseLevel;
        public ICollection<string> Completed;
        public Func<string, bool> BossDefeated;
        public Func<string, int> Owned;
        public int CousinsSigned;
    }

    /// <summary>
    /// The Vó Edna chain "Missões da Casa" 2–6 (Housing.md §7.3): Água Fresca (10 pedra + 5 madeira → poço), Fogo na
    /// Cozinha (after the Lobo Alfa: 3 carne assada + 8 madeira → fogão a lenha + filtro), Parede de Barro (reach N3 →
    /// colcha), Tijolo por Tijolo (reach N4 → rede da Vó + Baú-correio), Casa Cheia (N5 + four cousins in the guest
    /// book → retrato de casamento). Unity-free.
    /// </summary>
    public static class HouseQuestChain
    {
        public const string AguaFresca = "house_quest_agua_fresca";
        public const string FogoNaCozinha = "house_quest_fogo_cozinha";
        public const string ParedeDeBarro = "house_quest_parede_barro";
        public const string TijoloPorTijolo = "house_quest_tijolo_por_tijolo";
        public const string CasaCheia = "house_quest_casa_cheia";
        public const string CookedMeatId = "item_cooked_meat";

        public static readonly IReadOnlyList<HouseQuestDefinition> All = new[]
        {
            new HouseQuestDefinition
            {
                Number = 2, Id = AguaFresca, Kind = HouseQuestKind.Deliver,
                Deliver = new[] { new HouseMaterialCost(HouseUpgradeRules.StoneId, 10), new HouseMaterialCost(HouseUpgradeRules.WoodId, 5) },
                Rewards = new[] { "casa_quintal_001" },
            },
            new HouseQuestDefinition
            {
                Number = 3, Id = FogoNaCozinha, Kind = HouseQuestKind.Deliver, RequiredBossId = "enemy_forest_alpha_wolf",
                Deliver = new[] { new HouseMaterialCost(CookedMeatId, 3), new HouseMaterialCost(HouseUpgradeRules.WoodId, 8) },
                Rewards = new[] { "casa_cozinha_001", "casa_cozinha_002" },
            },
            new HouseQuestDefinition
            {
                Number = 4, Id = ParedeDeBarro, Kind = HouseQuestKind.Upgrade, RequiredHouseLevel = 3,
                Rewards = new[] { "casa_quarto_006" }, WardrobeRewards = new[] { "roupa_comum_010" },
            },
            new HouseQuestDefinition
            {
                Number = 5, Id = TijoloPorTijolo, Kind = HouseQuestKind.Upgrade, RequiredHouseLevel = 4,
                Rewards = new[] { "casa_familia_002", "casa_util_005" },
            },
            new HouseQuestDefinition
            {
                Number = 6, Id = CasaCheia, Kind = HouseQuestKind.UpgradeAndGuests, RequiredHouseLevel = 5,
                RequiredCousins = HouseVisitRules.CousinsForCasaCheia,
                Rewards = new[] { "casa_familia_001" }, WardrobeRewards = new[] { "acess_014" },
            },
        };

        public static HouseQuestDefinition Get(string id) => All.FirstOrDefault(q => q.Id == id);

        public static HouseQuestState StateOf(HouseQuestDefinition quest, HouseQuestContext context)
        {
            if (quest == null) return HouseQuestState.Locked;
            if (context.Completed != null && context.Completed.Contains(quest.Id)) return HouseQuestState.Completed;
            if (!context.FirstQuestDone) return HouseQuestState.Locked;
            if (!string.IsNullOrEmpty(quest.AfterQuestId) && (context.Completed == null || !context.Completed.Contains(quest.AfterQuestId))) return HouseQuestState.Locked;
            if (!string.IsNullOrEmpty(quest.RequiredBossId) && (context.BossDefeated == null || !context.BossDefeated(quest.RequiredBossId))) return HouseQuestState.Locked;

            switch (quest.Kind)
            {
                case HouseQuestKind.Deliver:
                    return quest.Deliver.All(c => context.Owned != null && context.Owned(c.ItemId) >= c.Quantity) ? HouseQuestState.Ready : HouseQuestState.Active;
                case HouseQuestKind.Upgrade:
                    return context.HouseLevel >= quest.RequiredHouseLevel ? HouseQuestState.Ready : HouseQuestState.Active;
                default:
                    return context.HouseLevel >= quest.RequiredHouseLevel && context.CousinsSigned >= quest.RequiredCousins ? HouseQuestState.Ready : HouseQuestState.Active;
            }
        }

        /// <summary>Quests that complete on their own once ready (level / guests), i.e. not deliveries.</summary>
        public static bool CompletesByItself(HouseQuestDefinition quest) => quest != null && quest.Kind != HouseQuestKind.Deliver;
    }
}
