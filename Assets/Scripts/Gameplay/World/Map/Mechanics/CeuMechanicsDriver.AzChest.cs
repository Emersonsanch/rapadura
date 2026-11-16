using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The last Arca Resgatada +5 (96, <see cref="AzRescuedChestRules"/>): when Āz, a Capitã Cobiçosa, is beaten with the hero by her ring, she gives
    /// back the chest once per real day (UTC). The day claimed is a time mark written when the chest is paid; the chest itself is not saved.
    /// </summary>
    public sealed partial class CeuMechanicsDriver
    {
        private const string AzGoldItemId = "item_gold";

        private void OnAzDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null || !FindPlayer())
            {
                return;
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            string enemyId = enemy != null && enemy.Definition != null ? enemy.Definition.name : null;
            if (enemyId != RegionBossRules.AzCaptainId)
            {
                return;
            }

            Vector3 at = evt.Enemy.transform.position;
            Vector2 flat = new Vector2(at.x - _player.position.x, at.z - _player.position.z);
            if (flat.sqrMagnitude > AzRescuedChestRules.WitnessRange * AzRescuedChestRules.WitnessRange)
            {
                return;
            }

            long today = (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalDays;
            bool known = ServiceLocator.TryGet(out WorldMapProgress progress);
            long lastDay = known ? progress.GetTimeMark(AzRescuedChestRules.ClaimKey) : 0L;
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (!AzRescuedChestRules.CanClaim(lastDay, today))
            {
                SayText(LateBossLinkTexts.Get(localization, LateBossLinkTexts.AzChestToday));
                return;
            }

            if (known)
            {
                progress.SetTimeMark(AzRescuedChestRules.ClaimKey, today);
            }

            CangacoChest chest = AzRescuedChestRules.Chest(today);
            if (chest.Experience > 0 && _stats != null)
            {
                _stats.AddExperience(chest.Experience);
            }

            bool overflow = GrantAzItem(AzGoldItemId, chest.Gold);
            overflow |= GrantAzItem(EventRoomRules.BlessingStoneItemId, chest.BlessingStones);
            overflow |= GrantAzItem(EventRoomRules.SoulStoneItemId, chest.SoulStones);
            SayText(string.Format(LateBossLinkTexts.Get(localization, LateBossLinkTexts.AzChest), chest.Gold, chest.Experience));
            SayText(string.Format(LateBossLinkTexts.Get(localization, LateBossLinkTexts.AzChestStones), chest.BlessingStones, chest.SoulStones));
            if (overflow)
            {
                SayText(LateBossLinkTexts.Get(localization, LateBossLinkTexts.BagFull));
            }

            SayText(LateBossLinkTexts.Get(localization, LateBossLinkTexts.LineFor(LateBossLinkTexts.Moment.AzChest, MapRequirementHeroState.Capture(_bag).Hero)));
        }

        /// <summary>Puts <paramref name="amount"/> of <paramref name="itemId"/> in the bag; true when some of it did not fit.</summary>
        private bool GrantAzItem(string itemId, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(itemId);
            return item == null || _bag == null || _bag.AddItem(item, amount) > 0;
        }
    }
}
