using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Hunting;

namespace Rapadura.UI.Hunting
{
    /// <summary>
    /// Text builders shared by Seu Zé's board (<see cref="HuntingBoardPanel"/>), the quest-tracker card
    /// (<see cref="HuntingTrackerCard"/>) and the Caderno's "Amuletos" tab (<see cref="BestiaryCharmsTab"/>). Static
    /// and free of UI Toolkit so EditMode tests check them with or without a <see cref="LocalizationManager"/>.
    /// </summary>
    public static class HuntingPresenter
    {
        /// <summary>A creature's localized name (its Caderno page), or the id when unknown.</summary>
        public static string CreatureName(LocalizationManager manager, string enemyId)
        {
            BestiaryEntry entry = BestiaryCatalog.Get(enemyId);
            return entry?.Definition != null ? entry.Definition.LocalizedName(manager) : enemyId ?? string.Empty;
        }

        public static string TrackerLine(LocalizationManager manager, HuntingTask task, string creatureName) =>
            task == null ? null : HuntingTexts.Format(manager, HuntingTexts.TrackerLine, creatureName, task.progress, task.required);

        public static string ActiveTaskText(LocalizationManager manager, HuntingTask task, string creatureName) =>
            task == null ? HuntingTexts.Get(manager, HuntingTexts.TaskNone) : HuntingTexts.Format(manager, HuntingTexts.TaskActive, creatureName, task.progress, task.required);

        public static string RewardText(LocalizationManager manager, HuntingTask task) =>
            task == null ? null : HuntingTexts.Format(manager, HuntingTexts.TaskReward, task.rewardExperience, task.rewardGold, task.rewardHuntPoints);

        public static string CompletedText(LocalizationManager manager, HuntingTask task) =>
            task == null ? null : HuntingTexts.Format(manager, HuntingTexts.TaskCompleted, task.rewardExperience, task.rewardGold, task.rewardHuntPoints);

        /// <summary>"Pequena (10)".</summary>
        public static string TierButton(LocalizationManager manager, HuntingTaskTier tier) =>
            HuntingTexts.Format(manager, HuntingTexts.TaskTierButton, HuntingTexts.Get(manager, HuntingTexts.TierKey(tier)), HuntingRules.KillsFor(tier));

        public static string PreyBonusText(LocalizationManager manager, PreyBonusKind kind)
        {
            float value = kind == PreyBonusKind.Protection ? HuntingRules.PreyProtectionBonus
                : kind == PreyBonusKind.Loot ? HuntingRules.PreyLootBonus
                : HuntingRules.PreyExperienceBonus;
            return HuntingTexts.Format(manager, HuntingTexts.PreyBonusKey(kind), HuntingTexts.Percent(value));
        }

        public static string PreyStatus(LocalizationManager manager, PreyOffer active, int offerCount, string creatureName)
        {
            if (active != null)
            {
                return HuntingTexts.Format(manager, HuntingTexts.PreyActive, creatureName, PreyBonusText(manager, active.bonus));
            }

            return HuntingTexts.Get(manager, offerCount > 0 ? HuntingTexts.PreyNone : HuntingTexts.PreyEmpty);
        }

        /// <summary>The map HUD's highlight chip ("Destaque do dia: Lobo (+50% de XP e saque)"), null without a highlight.</summary>
        public static string BoostedHudText(LocalizationManager manager, string creatureName) =>
            string.IsNullOrEmpty(creatureName) ? null : HuntingTexts.Format(manager, HuntingTexts.BoostedHud, creatureName, HuntingTexts.Percent(HuntingRules.BoostedExperienceBonus));

        public static string BoostedCadernoText(LocalizationManager manager, string creatureName) =>
            string.IsNullOrEmpty(creatureName) ? null : HuntingTexts.Format(manager, HuntingTexts.BoostedCaderno, creatureName, HuntingTexts.Percent(HuntingRules.BoostedExperienceBonus));

        public static string Greeting(LocalizationManager manager, CharacterId? hero) =>
            HuntingTexts.Get(manager, hero.HasValue ? HuntingTexts.GreetingKey(hero.Value) : HuntingTexts.GreetingNone);

        public static string RankText(LocalizationManager manager, int rank) =>
            HuntingTexts.Format(manager, HuntingTexts.RankLine, HuntingTexts.Get(manager, HuntingTexts.RankKey(rank)));

        /// <summary>Label of a charm's action button: buy (with cost), wear, take off, or why it can't.</summary>
        public static string CharmActionText(LocalizationManager manager, HuntingCharmSpec charm, bool unlocked, bool equipped, bool canBuy, bool cordFull)
        {
            if (charm == null)
            {
                return string.Empty;
            }

            if (!unlocked)
            {
                return canBuy ? HuntingTexts.Format(manager, HuntingTexts.CharmBuy, charm.Cost) : HuntingTexts.Format(manager, HuntingTexts.CharmLocked, charm.Cost);
            }

            if (equipped)
            {
                return HuntingTexts.Get(manager, HuntingTexts.CharmUnequip);
            }

            return HuntingTexts.Get(manager, cordFull ? HuntingTexts.CharmFull : HuntingTexts.CharmEquip);
        }
    }
}
