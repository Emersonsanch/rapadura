using System;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>One item reward entry (item + quantity) granted on quest completion.</summary>
    [Serializable]
    public class QuestItemReward
    {
        public ItemDefinition Item;
        public int Quantity = 1;
    }
}
