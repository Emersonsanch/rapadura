using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>Why a fusion can't start.</summary>
    public enum FusionCheck
    {
        Ok,
        /// <summary>Needs 2 or 3 pieces.</summary>
        WrongCount,
        /// <summary>Every piece must be the same base item (same id), equipment, not stackable.</summary>
        NotSameItem,
        /// <summary>The best piece is already at the tier's cap (<see cref="CraftTierRules.RarityCap"/>).</summary>
        AtCap,
        MissingGold,
        MissingTrophy,
        /// <summary>A piece carries MU options (excellent / luck / ancient / sockets): the forge refuses it.</summary>
        HasOptions,
    }

    /// <summary>What one fusion roll did.</summary>
    public readonly struct FusionOutcome
    {
        public readonly bool Success;
        /// <summary>Index (into the given list) of the piece that survives: the upgraded one, or kept as it was on failure.</summary>
        public readonly int KeptIndex;
        public readonly ItemRarity FromRarity;
        public readonly ItemRarity ToRarity;
        /// <summary>Consolation on failure: units of <see cref="FusionRules.ConsolationItemId"/> returned.</summary>
        public readonly int Consolation;
        public readonly float Chance;

        public FusionOutcome(bool success, int keptIndex, ItemRarity fromRarity, ItemRarity toRarity, int consolation, float chance)
        {
            Success = success;
            KeptIndex = keptIndex;
            FromRarity = fromRarity;
            ToRarity = toRarity;
            Consolation = consolation;
            Chance = chance;
        }
    }

    /// <summary>
    /// "Forja de fusão" (FASE 16; Docs/Balance/README.md "Crafting avançado"), kept by Adamas, the forge-keeper of the
    /// city: 2 or 3 copies of the same piece melt into one a rarity step better, with a chance. Never lose everything:
    /// <list type="bullet">
    /// <item>The best piece (highest rarity, then refine level, then the lowest index) is the one that is upgraded and
    /// keeps its refine level; only the extra copies melt.</item>
    /// <item>On failure the best piece stays exactly as it was and each melted copy returns
    /// <see cref="ConsolationPerPiece"/> iron (the Troféu and the gold are spent either way).</item>
    /// <item>The result never passes the tier cap (<see cref="CraftTierRules.RarityCap"/>): bad-luck protection, not new power.</item>
    /// <item>Pieces with MU options (excellent, luck, ancient set, sockets) don't go in: a slot is either excellent (a drop)
    /// or forged, which is what keeps the whole power pile in budget (README "Crafting avançado").</item>
    /// </list>
    /// Pure: the caller passes the RNG (online: the server).
    /// </summary>
    public static class FusionRules
    {
        public const float TwoPieceChance = 0.5f;
        public const float ThreePieceChance = 0.75f;
        /// <summary>Going up to Epic is harder.</summary>
        public const float EpicPenalty = 0.15f;
        /// <summary>The Troféu da Fera as catalyst.</summary>
        public const float TrophyBonus = 0.15f;
        public const float MaxChance = 0.9f;
        public const int ConsolationPerPiece = 2;
        public const string ConsolationItemId = "item_iron";
        /// <summary>Gold per target rarity step (Uncommon 1 … Epic 3).</summary>
        public const int GoldPerRarity = 15;

        public static ItemRarity EffectiveRarity(InventorySlotData slot, ItemDefinition item)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            ItemRarity rolled = slot != null ? slot.generatedRarity : ItemRarity.Common;
            return (ItemRarity)Math.Max((int)baseRarity, (int)rolled);
        }

        /// <summary>Index of the piece that is upgraded / kept.</summary>
        public static int BestIndex(IReadOnlyList<InventorySlotData> pieces, Func<string, ItemDefinition> resolveItem)
        {
            int best = -1;
            for (int i = 0; i < pieces.Count; i++)
            {
                if (best < 0)
                {
                    best = i;
                    continue;
                }

                ItemRarity a = EffectiveRarity(pieces[i], resolveItem(pieces[i].itemId));
                ItemRarity b = EffectiveRarity(pieces[best], resolveItem(pieces[best].itemId));
                if (a > b || (a == b && pieces[i].refineLevel > pieces[best].refineLevel))
                {
                    best = i;
                }
            }

            return best;
        }

        public static ItemRarity TargetRarity(ItemRarity best) => (ItemRarity)Math.Min((int)ItemRarity.Legendary, (int)best + 1);

        public static int GoldCost(ItemRarity target) => GoldPerRarity * (int)target;

        public static float Chance(int pieces, ItemRarity target, bool withTrophy)
        {
            float chance = pieces >= 3 ? ThreePieceChance : TwoPieceChance;
            if (target >= ItemRarity.Epic)
            {
                chance -= EpicPenalty;
            }

            if (withTrophy)
            {
                chance += TrophyBonus;
            }

            return Math.Max(0f, Math.Min(MaxChance, chance));
        }

        public static FusionCheck Validate(IReadOnlyList<InventorySlotData> pieces, int tier, bool withTrophy, Func<string, ItemDefinition> resolveItem,
            Func<string, int> countInBag, string goldItemId, string trophyItemId)
        {
            if (pieces == null || pieces.Count < 2 || pieces.Count > 3)
            {
                return FusionCheck.WrongCount;
            }

            string id = null;
            foreach (InventorySlotData piece in pieces)
            {
                if (piece == null || piece.IsEmpty)
                {
                    return FusionCheck.NotSameItem;
                }

                ItemDefinition item = resolveItem(piece.itemId);
                if (item == null || !item.IsEquippable || item.IsStackable || (id != null && id != piece.itemId))
                {
                    return FusionCheck.NotSameItem;
                }

                if (piece.HasItemOptions)
                {
                    return FusionCheck.HasOptions;
                }

                id = piece.itemId;
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                for (int j = i + 1; j < pieces.Count; j++)
                {
                    if (ReferenceEquals(pieces[i], pieces[j]))
                    {
                        return FusionCheck.NotSameItem;
                    }
                }
            }

            int best = BestIndex(pieces, resolveItem);
            ItemRarity from = EffectiveRarity(pieces[best], resolveItem(pieces[best].itemId));
            if (from >= CraftTierRules.RarityCap(tier))
            {
                return FusionCheck.AtCap;
            }

            if (countInBag != null && countInBag(goldItemId) < GoldCost(TargetRarity(from)))
            {
                return FusionCheck.MissingGold;
            }

            if (withTrophy && (countInBag == null || countInBag(trophyItemId) < 1))
            {
                return FusionCheck.MissingTrophy;
            }

            return FusionCheck.Ok;
        }

        /// <summary>Rolls a validated fusion (one draw of <paramref name="random"/>).</summary>
        public static FusionOutcome Roll(IReadOnlyList<InventorySlotData> pieces, bool withTrophy, Func<string, ItemDefinition> resolveItem, Random random)
        {
            int best = BestIndex(pieces, resolveItem);
            ItemRarity from = EffectiveRarity(pieces[best], resolveItem(pieces[best].itemId));
            ItemRarity to = TargetRarity(from);
            float chance = Chance(pieces.Count, to, withTrophy);
            bool success = random.NextDouble() < chance;
            int consolation = success ? 0 : ConsolationPerPiece * (pieces.Count - 1);
            return new FusionOutcome(success, best, from, success ? to : from, consolation, chance);
        }
    }
}
