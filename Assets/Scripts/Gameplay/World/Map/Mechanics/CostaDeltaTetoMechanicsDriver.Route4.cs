using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Helpers shared by the mechanics of maps 27, 29, 30, 32 and 40 on this driver: texts, the game day, rewards and the gold purse.</summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const string GoldItemId = "item_gold";

        private static int GameDay() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;

        private static float GameHour() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.CurrentHour : 12f;

        private static float Daylight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.DaylightFactor : 1f;

        private void SayR4(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CoastDeltaPassTexts.Get(localization, key));
        }

        private void SayR4(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(string.Format(CoastDeltaPassTexts.Get(localization, key), args));
        }

        private string StoneName(int stone)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return CoastDeltaPassTexts.Get(localization, CoastDeltaPassTexts.StoneKeys[Mathf.Clamp(stone, 0, CoastDeltaPassTexts.StoneKeys.Length - 1)]);
        }

        /// <summary>One item into the bag; false (and the bag-full line) when it does not fit.</summary>
        private bool GiveR4(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            if (item == null || _bag == null || _bag.AddItem(item, 1) != 0)
            {
                SayR4(CoastDeltaPassTexts.BagFull);
                return false;
            }

            SayR4(CoastDeltaPassTexts.Found, item.DisplayName);
            return true;
        }

        private int GoldInBag() => _bag != null ? _bag.GetTotalCount(GoldItemId) : 0;

        private bool SpendGold(int amount) => _bag != null && _bag.GetTotalCount(GoldItemId) >= amount && _bag.RemoveById(GoldItemId, amount);

        private bool PayGold(int amount)
        {
            ItemDefinition gold = ItemDatabase.GetById(GoldItemId);
            if (gold == null || _bag == null || _bag.AddItem(gold, amount) > 0)
            {
                SayR4(CoastDeltaPassTexts.BagFull);
                return false;
            }

            return true;
        }

        private LineRenderer Pillar(LineRenderer line, string objectName, Vector2 world, float width, float height, Color color)
        {
            line = line != null ? line : NewLine(objectName, width);
            float y = Mathf.Max(0f, Ground(world));
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, new Vector3(world.x, y, world.y));
            line.SetPosition(1, new Vector3(world.x, y + height, world.y));
            line.enabled = true;
            return line;
        }

        private static void Off(LineRenderer line)
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }

        private static void Off(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (LineRenderer line in lines)
            {
                Off(line);
            }
        }
    }
}
