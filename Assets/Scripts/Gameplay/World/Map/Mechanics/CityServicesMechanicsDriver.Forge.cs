using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The mouth of the Grande Forja in the Salões de Bronze (<see cref="GreatForgeRules"/>): [E] lights the next of the five stages when its
    /// boss or quest is done and the cousin carries what it burns (Napirasu takes it, pays the stage at once and the city keeps its service);
    /// otherwise it gives the forge's Pedras da Bênção of the day, or Napirasu says what the next stage still waits for.
    /// </summary>
    public sealed partial class CityServicesMechanicsDriver
    {
        private void UseForge(long today)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            int lit = GreatForgeRules.StagesLit(progress);
            ServiceLocator.TryGet(out BestiaryProgress bestiary);
            ServiceLocator.TryGet(out QuestManager quests);
            GreatForgeCheck check = GreatForgeRules.Check(lit,
                id => bestiary != null && bestiary.GetKills(id) > 0,
                id => quests != null && quests.IsCompleted(id),
                id => _bag != null ? _bag.GetTotalCount(id) : 0);

            if (check == GreatForgeCheck.Ready && _bag != null)
            {
                LightStage(progress, GreatForgeRules.Next(lit));
                return;
            }

            long givenDay = progress != null ? progress.GetDailyMark(GreatForgeRules.GiftDailyKey) : 0;
            if (GreatForgeRules.GiftReady(lit, givenDay, today))
            {
                GiveGift(progress, lit, today);
                return;
            }

            GreatForgeStage next = GreatForgeRules.Next(lit);
            if (next == null)
            {
                Say(GreatForgeRules.DailyBlessings(lit) > 0 ? CityServiceTexts.ForgeGiftTaken : CityServiceTexts.ForgeAllLit);
                return;
            }

            Say(CityServiceTexts.ForgeNeed(next.Id));
        }

        private void LightStage(WorldMapProgress progress, GreatForgeStage stage)
        {
            foreach ((string itemId, int count) in stage.Delivery)
            {
                _bag.RemoveById(itemId, count);
            }

            foreach ((string itemId, int count) in stage.Reward)
            {
                ItemDefinition item = ItemDatabase.GetById(itemId);
                if (item != null && _bag.AddItem(item, count) > 0)
                {
                    Say(CityServiceTexts.BagFull);
                }
            }

            progress?.SetDailyMark(GreatForgeRules.StageKey, stage.Number);
            SayAbove(CityServiceTexts.Get(Localization(), CityServiceTexts.ForgeLit(stage.Id)));
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                Say(stage.Number >= GreatForgeRules.StageCount ? CityServiceTexts.ForgeDoneLine(cousin.Value) : CityServiceTexts.ForgeLine(cousin.Value));
            }
        }

        private void GiveGift(WorldMapProgress progress, int lit, long today)
        {
            ItemDefinition stone = ItemDatabase.GetById(GreatForgeRules.BlessingStone);
            int count = GreatForgeRules.DailyBlessings(lit);
            int left = stone != null && _bag != null ? _bag.AddItem(stone, count) : count;
            if (left >= count)
            {
                Say(CityServiceTexts.BagFull);
                return;
            }

            progress?.SetDailyMark(GreatForgeRules.GiftDailyKey, (int)today);
            LocalizationManager localization = Localization();
            SayAbove(string.Format(CityServiceTexts.Get(localization, CityServiceTexts.ForgeGift), stone.DisplayName, count - left));
        }
    }
}
