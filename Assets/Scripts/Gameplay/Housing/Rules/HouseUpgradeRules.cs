using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>A quantity of one bag item (gold or a biome material) spent on an upgrade or a house quest.</summary>
    [Serializable]
    public struct HouseMaterialCost
    {
        public string ItemId;
        public int Quantity;

        public HouseMaterialCost(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>One line of the "Melhorar a casa" panel.</summary>
    public enum HouseRequirementKind
    {
        CharacterLevel,
        Boss,
        Gold,
        Material,
        Keepsake,
    }

    /// <summary>A requirement with what is needed and what the player has (<see cref="Met"/> when have ≥ need).</summary>
    public sealed class HouseRequirementRow
    {
        public HouseRequirementKind Kind;
        /// <summary>Item id (gold, material, keepsake) or boss id; null for the character level.</summary>
        public string Id;
        public int Need;
        public int Have;

        public bool Met => Have >= Need;
    }

    public enum HouseUpgradeBlock
    {
        None,
        NoHouse,
        MaxLevel,
        Requirements,
        PaymentFailed,
    }

    /// <summary>The result of <see cref="HouseUpgradeRules.Evaluate"/>: the next level and every requirement row.</summary>
    public sealed class HouseUpgradeCheck
    {
        public int FromLevel;
        public int ToLevel;
        public HouseLevelSpec Target;
        public readonly List<HouseRequirementRow> Rows = new List<HouseRequirementRow>();
        public HouseUpgradeBlock Block;

        public bool CanUpgrade => Block == HouseUpgradeBlock.None;
        public IEnumerable<HouseRequirementRow> Missing => Rows.Where(r => !r.Met);
    }

    /// <summary>What happened on a successful upgrade (for the result card).</summary>
    public sealed class HouseUpgradeResult
    {
        public HouseUpgradeCheck Check;
        public int FromLevel;
        public int ToLevel;
        /// <summary>Placed pieces that did not fit the new layout and went to the Depósito (never lost).</summary>
        public int MovedToDepot;
        /// <summary>"Missões da Casa" completed by this upgrade.</summary>
        public readonly List<string> CompletedQuests = new List<string>();

        public bool Success => Check != null && Check.CanUpgrade && ToLevel > FromLevel;
    }

    /// <summary>Where the upgrade and the delivery quests take gold and materials from (offline: the bag, then the Baú da casa).</summary>
    public interface IHouseWallet
    {
        int Count(string itemId);

        /// <summary>Removes exactly <paramref name="quantity"/> copies; false (and nothing removed) when there are fewer.</summary>
        bool Remove(string itemId, int quantity);
    }

    /// <summary>
    /// H4 "Melhorar a casa" (Housing.md §2.2, §10.3): one level at a time, gated by the hero's level and the region
    /// boss (DefeatedEnemyRegistry), paid in gold + biome materials (provisional prices, user decision pending) and, for
    /// the casarão, one keepsake of each boss that must be in the house (never spent). Nothing is ever lost: the
    /// layout is re-validated on the bigger grid and what no longer fits goes to the Depósito
    /// (<see cref="HouseLayout.Restore"/>). Unity-free (the online server validates the same way).
    /// </summary>
    public static class HouseUpgradeRules
    {
        public const string GoldId = "item_gold";
        public const string WoodId = "item_wood";
        public const string FiberId = "item_fiber";
        public const string StoneId = "item_stone";
        /// <summary>"Ferro (lingote)".</summary>
        public const string IronId = "item_iron";

        /// <summary>The "lembrança" plaque of each boss (§7.2): Lobo Alfa, Guardião, Senhor da Guerra, Habitante Ancestral.</summary>
        public static readonly string[] BossKeepsakes = { "casa_trofeu_001", "casa_trofeu_007", "casa_trofeu_013", "casa_trofeu_019" };

        /// <summary>The region bosses in campaign order (the trophy wall shows them in this order).</summary>
        public static readonly string[] BossIds = { "enemy_forest_alpha_wolf", "enemy_desert_ruin_guardian", "enemy_mountain_warlord", "enemy_cave_dweller" };

        /// <summary>
        /// The next level's requirements. <paramref name="bossDefeated"/>, <paramref name="owned"/> (bag + chest count of an
        /// item id) and <paramref name="keepsakeInHouse"/> may be null (= false / 0).
        /// </summary>
        public static HouseUpgradeCheck Evaluate(int currentLevel, bool hasHouse, int heroLevel, Func<string, bool> bossDefeated,
            Func<string, int> owned, Func<string, bool> keepsakeInHouse)
        {
            int from = Math.Max(HouseLevels.MinLevel, Math.Min(HouseLevels.MaxLevel, currentLevel));
            var check = new HouseUpgradeCheck { FromLevel = from, ToLevel = from };
            if (!hasHouse)
            {
                check.Block = HouseUpgradeBlock.NoHouse;
                return check;
            }

            if (from >= HouseLevels.MaxLevel)
            {
                check.Block = HouseUpgradeBlock.MaxLevel;
                return check;
            }

            HouseLevelSpec target = HouseLevels.Get(from + 1);
            check.ToLevel = target.Level;
            check.Target = target;
            check.Rows.Add(new HouseRequirementRow { Kind = HouseRequirementKind.CharacterLevel, Need = target.RequiredCharacterLevel, Have = Math.Max(1, heroLevel) });
            if (!string.IsNullOrEmpty(target.RequiredBossId))
            {
                check.Rows.Add(new HouseRequirementRow { Kind = HouseRequirementKind.Boss, Id = target.RequiredBossId, Need = 1, Have = bossDefeated != null && bossDefeated(target.RequiredBossId) ? 1 : 0 });
            }

            if (target.UpgradeGold > 0)
            {
                check.Rows.Add(new HouseRequirementRow { Kind = HouseRequirementKind.Gold, Id = GoldId, Need = target.UpgradeGold, Have = Count(owned, GoldId) });
            }

            foreach (HouseMaterialCost cost in target.UpgradeMaterials ?? Array.Empty<HouseMaterialCost>())
            {
                if (string.IsNullOrEmpty(cost.ItemId) || cost.Quantity <= 0) continue;
                check.Rows.Add(new HouseRequirementRow { Kind = HouseRequirementKind.Material, Id = cost.ItemId, Need = cost.Quantity, Have = Count(owned, cost.ItemId) });
            }

            foreach (string keepsake in target.RequiredKeepsakes ?? Array.Empty<string>())
            {
                check.Rows.Add(new HouseRequirementRow { Kind = HouseRequirementKind.Keepsake, Id = keepsake, Need = 1, Have = keepsakeInHouse != null && keepsakeInHouse(keepsake) ? 1 : 0 });
            }

            check.Block = check.Rows.All(r => r.Met) ? HouseUpgradeBlock.None : HouseUpgradeBlock.Requirements;
            return check;
        }

        private static int Count(Func<string, int> owned, string itemId) => owned != null ? Math.Max(0, owned(itemId)) : 0;

        /// <summary>The gold + materials of <paramref name="target"/> (keepsakes are not spent).</summary>
        public static IEnumerable<HouseMaterialCost> SpentCosts(HouseLevelSpec target)
        {
            if (target == null) yield break;
            if (target.UpgradeGold > 0) yield return new HouseMaterialCost(GoldId, target.UpgradeGold);
            foreach (HouseMaterialCost cost in target.UpgradeMaterials ?? Array.Empty<HouseMaterialCost>())
            {
                if (!string.IsNullOrEmpty(cost.ItemId) && cost.Quantity > 0) yield return cost;
            }
        }

        /// <summary>
        /// Takes every cost from <paramref name="wallet"/>, all or nothing: first checks everything is there, then removes.
        /// (A removal that still fails after the check — a wallet that lied — is reported as false; what was taken stays taken.)
        /// </summary>
        public static bool Pay(IEnumerable<HouseMaterialCost> costs, IHouseWallet wallet)
        {
            if (wallet == null) return false;
            List<HouseMaterialCost> list = Merge(costs);
            if (list.Any(c => wallet.Count(c.ItemId) < c.Quantity)) return false;
            foreach (HouseMaterialCost cost in list)
            {
                if (!wallet.Remove(cost.ItemId, cost.Quantity)) return false;
            }

            return true;
        }

        /// <summary>Sums costs of the same item id (keeps the first-seen order).</summary>
        public static List<HouseMaterialCost> Merge(IEnumerable<HouseMaterialCost> costs)
        {
            var order = new List<string>();
            var sums = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (HouseMaterialCost cost in costs ?? Enumerable.Empty<HouseMaterialCost>())
            {
                if (string.IsNullOrEmpty(cost.ItemId) || cost.Quantity <= 0) continue;
                if (!sums.ContainsKey(cost.ItemId))
                {
                    sums[cost.ItemId] = 0;
                    order.Add(cost.ItemId);
                }

                sums[cost.ItemId] += cost.Quantity;
            }

            return order.Select(id => new HouseMaterialCost(id, sums[id])).ToList();
        }

        /// <summary>
        /// The saved layout moved to the next level's grids, keeping each piece where it was relative to the house: the
        /// interior grows from the back-left corner (same cells), the yard grows around the house (it is centred on the
        /// house and lies in front of the porch, so a piece keeps its distance to the porch and its side offset).
        /// Pieces are cloned; validity is checked afterwards by <see cref="HouseLayout.Restore"/>.
        /// </summary>
        public static HousePlacedData[] ShiftForUpgrade(IEnumerable<HousePlacedData> placed, HouseLevelSpec from, HouseLevelSpec to)
        {
            if (placed == null) return Array.Empty<HousePlacedData>();
            int dx = from != null && to != null ? (to.YardWidth - from.YardWidth) / 2 : 0;
            int dy = from != null && to != null ? to.YardDepth - from.YardDepth : 0;
            var result = new List<HousePlacedData>();
            foreach (HousePlacedData piece in placed)
            {
                if (piece == null) continue;
                HousePlacedData copy = piece.Clone();
                if (copy.layer == HouseLayer.Yard)
                {
                    copy.x += dx;
                    copy.y += dy;
                }

                result.Add(copy);
            }

            return result.ToArray();
        }

        /// <summary>How many item copies a layout holds in all (placed + depot): an upgrade must keep this number.</summary>
        public static int TotalItems(HouseLayout layout)
        {
            if (layout == null) return 0;
            return layout.Placed.Count + layout.Depot.Sum(e => e.quantity);
        }
    }
}
