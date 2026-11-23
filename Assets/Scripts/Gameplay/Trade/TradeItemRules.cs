using System;
using System.Globalization;
using System.Text;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>
    /// "Pode ser trocado?" and "é raro?" for one bag entry (TODO.md Fase 12.3). Decided 2026-11-23 from what the
    /// <see cref="ItemDefinition"/> already says, with no new asset field (the assets stay untouched); the checks run in
    /// this order and the first one that matches is the reason shown:
    /// <list type="number">
    /// <item>No definition → <see cref="TradeRefusal.UnknownItem"/>.</item>
    /// <item>The bag's gold (<see cref="TradeRules.GoldItemId"/>) → <see cref="TradeRefusal.GoldItemUseGoldField"/>: gold
    /// goes through the gold field, so it is never counted twice. Any other currency → <see cref="TradeRefusal.Currency"/>.</item>
    /// <item>Story rewards: <see cref="ItemType.QuestItem"/> / <see cref="ItemCategory.QuestItem"/> (keys, invitations,
    /// passages) and the ids starting with <see cref="StoryPrefixes"/> → <see cref="TradeRefusal.StoryItem"/>.</item>
    /// <item>Family relics: the Patuá da Vó Edna (<see cref="PatuaRules.ItemId"/>) and <see cref="RelicPrefixes"/>
    /// (<see cref="DeathDropRules.RelicIdPrefix"/> included) → <see cref="TradeRefusal.FamilyRelic"/>.</item>
    /// <item>Achievement items (<see cref="AchievementPrefixes"/>) → <see cref="TradeRefusal.AchievementItem"/>.</item>
    /// <item>Mounts: their whistles (<see cref="MountPrefixes"/>; the mount belongs to the cousin who tamed or bought it,
    /// and the stable — <c>MountCollection</c> — is not a bag) → <see cref="TradeRefusal.MountItem"/>.</item>
    /// <item>House items (<see cref="HousePrefixes"/>, the <c>casa_*</c> catalog; they live in the house, not the bag) →
    /// <see cref="TradeRefusal.HouseItem"/>.</item>
    /// <item>Anything flagged <see cref="ItemDefinition.IsBound"/> (the Tacho wings and cloaks, gate keys and seals) →
    /// <see cref="TradeRefusal.Bound"/>. The same flag that already keeps an item from falling on death
    /// (<see cref="DeathDropRules"/>), so "vinculado" means one thing everywhere.</item>
    /// </list>
    /// Everything else trades: gear (generated, refined and with options intact), materials, potions, fish, boss trophies.
    /// </summary>
    public static class TradeItemRules
    {
        public static readonly string[] StoryPrefixes = { "story_", "item_historia_", "item_story_" };
        public static readonly string[] RelicPrefixes = { DeathDropRules.RelicIdPrefix, "item_reliquia_", "item_relic_", "item_patua_" };
        public static readonly string[] AchievementPrefixes = { "achievement_", "conquista_", "item_conquista_", "item_achievement_" };
        public static readonly string[] MountPrefixes = { "item_whistle_", "mount_", "item_mount_whistle_" };
        public static readonly string[] HousePrefixes = { "casa_", "item_casa_", "house_" };

        /// <summary>A refine this high makes a piece "rare" for the warning even at common rarity.</summary>
        public const int RareRefineLevel = 7;

        /// <summary><see cref="CanTrade(InventorySlotData, ItemDefinition)"/> for a bare definition (no rolled data).</summary>
        public static TradeRefusal CanTrade(ItemDefinition item) => CanTrade(null, item);

        /// <summary>Why <paramref name="item"/> (in <paramref name="slot"/>, may be null) cannot be traded, or <see cref="TradeRefusal.None"/>.</summary>
        public static TradeRefusal CanTrade(InventorySlotData slot, ItemDefinition item)
        {
            if (item == null)
            {
                return TradeRefusal.UnknownItem;
            }

            string id = item.ItemId ?? string.Empty;
            if (string.Equals(id, TradeRules.GoldItemId, StringComparison.Ordinal))
            {
                return TradeRefusal.GoldItemUseGoldField;
            }

            if (item.Type == ItemType.Currency || item.Category == ItemCategory.Currency || item.Category == ItemCategory.Gold)
            {
                return TradeRefusal.Currency;
            }

            if (item.Type == ItemType.QuestItem || item.Category == ItemCategory.QuestItem || StartsWithAny(id, StoryPrefixes))
            {
                return TradeRefusal.StoryItem;
            }

            if (string.Equals(id, PatuaRules.ItemId, StringComparison.Ordinal) || StartsWithAny(id, RelicPrefixes))
            {
                return TradeRefusal.FamilyRelic;
            }

            if (StartsWithAny(id, AchievementPrefixes))
            {
                return TradeRefusal.AchievementItem;
            }

            if (StartsWithAny(id, MountPrefixes))
            {
                return TradeRefusal.MountItem;
            }

            if (StartsWithAny(id, HousePrefixes))
            {
                return TradeRefusal.HouseItem;
            }

            return item.IsBound ? TradeRefusal.Bound : TradeRefusal.None;
        }

        public static bool IsTradeable(InventorySlotData slot, ItemDefinition item) => CanTrade(slot, item) == TradeRefusal.None;

        /// <summary>
        /// "Item raro" for the summary warning: rolled or base rarity Rare or above (<see cref="DeathDropRules.RarityOf"/>),
        /// refined to +<see cref="RareRefineLevel"/> or more, or with an excellent line or an ancient set.
        /// </summary>
        public static bool IsRare(InventorySlotData slot, ItemDefinition item)
        {
            if (DeathDropRules.RarityOf(slot, item) >= ItemRarity.Rare)
            {
                return true;
            }

            return slot != null && (slot.refineLevel >= RareRefineLevel || slot.excellentMask != 0 || !string.IsNullOrEmpty(slot.ancientSetId));
        }

        /// <summary>
        /// What makes this entry this entry (id, durability, rolled data, refine, options): two entries with the same
        /// fingerprint are the same thing for the trade. Settlement compares it with the one taken when the item was put
        /// in, so swapping a +15 for a +0 of the same id in the same bag entry after locking is caught
        /// (<see cref="TradeRefusal.ItemChanged"/>). Quantity and grid position are not part of it.
        /// </summary>
        public static string Fingerprint(InventorySlotData slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(64);
            sb.Append(slot.itemId).Append('|');
            sb.Append(((int)Math.Floor(slot.currentDurability)).ToString(CultureInfo.InvariantCulture)).Append('|');
            sb.Append(slot.generatedName ?? string.Empty).Append('|');
            sb.Append((int)slot.generatedRarity).Append('|');
            if (slot.generatedAffixIds != null)
            {
                for (int i = 0; i < slot.generatedAffixIds.Length; i++)
                {
                    float value = slot.generatedAffixValues != null && i < slot.generatedAffixValues.Length ? slot.generatedAffixValues[i] : 0f;
                    sb.Append(slot.generatedAffixIds[i]).Append('=').Append(value.ToString("R", CultureInfo.InvariantCulture)).Append(',');
                }
            }

            sb.Append('|').Append(slot.refineLevel).Append('/').Append(slot.refineOptionLevel);
            sb.Append('|').Append(slot.excellentMask).Append('/').Append(slot.luck ? 1 : 0).Append('/').Append(slot.ancientSetId ?? string.Empty);
            sb.Append('|');
            if (slot.sockets != null)
            {
                sb.Append(string.Join(",", slot.sockets));
            }

            return sb.ToString();
        }

        private static bool StartsWithAny(string id, string[] prefixes)
        {
            foreach (string prefix in prefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
