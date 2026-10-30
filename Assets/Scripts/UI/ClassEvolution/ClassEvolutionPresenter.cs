using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.ClassEvolution;
using Rapadura.Gameplay.Content;

namespace Rapadura.UI.ClassEvolution
{
    /// <summary>
    /// Text builders of the graduation panel (<see cref="ClassEvolutionPanel"/>, Docs/ClassEvolution.md §5): the cousin's
    /// title, the mission status and the family dialogue of the next graduation, the class combo and the master tree lines.
    /// Pure (no Unity objects) so EditMode tests cover every state.
    /// </summary>
    public static class ClassEvolutionPresenter
    {
        public static string HeroName(CharacterId hero) => CharacterRegistry.Get(hero)?.DisplayName ?? hero.ToString();

        public static string MentorName(LocalizationManager localization, GraduationSpec spec) =>
            spec == null ? string.Empty : ClassEvolutionTexts.Get(localization, ClassEvolutionCatalog.MentorNameKey(spec.MentorId));

        public static string Speaker(LocalizationManager localization, string speaker, string key) =>
            ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.SpeakerLine, speaker, ClassEvolutionTexts.Get(localization, key));

        public static string TitleLine(LocalizationManager localization, CharacterId hero, int tier) =>
            ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.CurrentTitle, ClassEvolutionTexts.TitleOf(localization, hero, tier));

        public static string TierLine(LocalizationManager localization, int tier) =>
            ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.TierLine, ClassEvolutionRules.ClampTier(tier), ClassEvolutionCatalog.MaxTier);

        public static string SkillName(LocalizationManager localization, string skillId) =>
            ClassEvolutionTexts.Get(localization, ContentText.SkillNameKey(skillId));

        public static string RegionName(LocalizationManager localization, GraduationSpec spec) =>
            ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.RegionKey(spec.MissionRegion));

        /// <summary>The status line of the next graduation (one line, always present).</summary>
        public static string StatusLine(LocalizationManager localization, CharacterId hero, GraduationRecord record, int heroLevel)
        {
            int tier = record != null ? record.tier : 0;
            GraduationSpec next = ClassEvolutionRules.NextGraduation(hero, tier);
            switch (ClassEvolutionRules.Status(hero, record, heroLevel))
            {
                case GraduationMissionStatus.AllDone:
                    return ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.StatusAllDone);
                case GraduationMissionStatus.LevelTooLow:
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.StatusLevelTooLow, next.MinLevel);
                case GraduationMissionStatus.Available:
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.StatusAvailable, MentorName(localization, next),
                        ClassEvolutionTexts.Get(localization, next.MissionTitleKey));
                case GraduationMissionStatus.InProgress:
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.StatusInProgress, ClassEvolutionTexts.Get(localization, next.MissionTitleKey),
                        record.missionKills, next.MissionKills, RegionName(localization, next));
                default:
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.StatusReady, MentorName(localization, next));
            }
        }

        /// <summary>
        /// The family dialogue for the current state: the mentor's offer (available), the mentor's encouragement with the
        /// count (in progress), or the mentor's turn-in line and the cousin's reply (ready). Empty when there is nothing to say.
        /// </summary>
        public static List<string> DialogueLines(LocalizationManager localization, CharacterId hero, GraduationRecord record, int heroLevel)
        {
            var lines = new List<string>();
            int tier = record != null ? record.tier : 0;
            GraduationSpec next = ClassEvolutionRules.NextGraduation(hero, tier);
            if (next == null)
            {
                return lines;
            }

            string mentor = MentorName(localization, next);
            switch (ClassEvolutionRules.Status(hero, record, heroLevel))
            {
                case GraduationMissionStatus.Available:
                    lines.Add(Speaker(localization, mentor, next.OfferKey));
                    break;
                case GraduationMissionStatus.InProgress:
                    lines.Add(ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.SpeakerLine, mentor,
                        ClassEvolutionTexts.Format(localization, next.ProgressKey, record.missionKills, next.MissionKills, RegionName(localization, next))));
                    break;
                case GraduationMissionStatus.ReadyToTurnIn:
                    lines.Add(Speaker(localization, mentor, next.TurnInKey));
                    lines.Add(Speaker(localization, HeroName(hero), next.ReplyKey));
                    break;
            }

            return lines;
        }

        /// <summary>What a fresh graduation brought: new title, skill, accessory and (tier 2) the class combo.</summary>
        public static List<string> GraduatedLines(LocalizationManager localization, CharacterId hero, GraduationSpec done)
        {
            var lines = new List<string>();
            if (done == null)
            {
                return lines;
            }

            lines.Add(ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.Graduated, ClassEvolutionTexts.Get(localization, done.TitleKey)));
            lines.Add(ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.NewSkill, SkillName(localization, done.SkillId)));
            lines.Add(ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.NewAccessory, ClassEvolutionTexts.Get(localization, done.AccessoryKey)));
            ClassComboSpec combo = ClassEvolutionCatalog.GetCombo(hero);
            if (combo != null && done.Tier == combo.RequiredTier)
            {
                lines.Add(ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.ComboUnlocked,
                    ClassEvolutionTexts.Get(localization, ContentText.ComboNameKey(combo.ComboId))));
            }

            return lines;
        }

        /// <summary>Combo name + description when unlocked, or the "opens with graduation N" line.</summary>
        public static List<string> ComboLines(LocalizationManager localization, CharacterId hero, int tier)
        {
            var lines = new List<string>();
            ClassComboSpec combo = ClassEvolutionCatalog.GetCombo(hero);
            if (combo == null)
            {
                return lines;
            }

            lines.Add(ClassEvolutionTexts.Get(localization, ContentText.ComboNameKey(combo.ComboId)));
            lines.Add(tier >= combo.RequiredTier
                ? ClassEvolutionTexts.Get(localization, ContentText.ComboDescriptionKey(combo.ComboId))
                : ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.ComboLocked, combo.RequiredTier));
            return lines;
        }

        /// <summary>Master header line: "Master Level N" or the lock reason.</summary>
        public static string MasterHeader(LocalizationManager localization, GraduationRecord record, int heroLevel) =>
            MasterLevelRules.IsMaster(record, heroLevel)
                ? ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterTitle, record.masterLevel)
                : ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterLocked, MasterLevelRules.RequiredLevel);

        public static string MasterXpLine(LocalizationManager localization, GraduationRecord record) =>
            record.masterLevel >= MasterLevelRules.MaxMasterLevel
                ? ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.MasterMax)
                : ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterXp, record.masterExperience, MasterLevelRules.ExperienceToNext(record.masterLevel));

        /// <summary>"Name 2/5 — description", and the lock reason when the node above is short.</summary>
        public static string NodeLine(LocalizationManager localization, GraduationRecord record, MasterNodeSpec node)
        {
            string rank = ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterRank, MasterLevelRules.RankOf(record, node.NodeId), node.MaxRank);
            return ClassEvolutionTexts.Get(localization, node.NameKey) + " " + rank + " — " + ClassEvolutionTexts.Get(localization, node.DescriptionKey);
        }

        /// <summary>Why a spend failed (null on success or when there is nothing to tell).</summary>
        public static string SpendFailure(LocalizationManager localization, MasterSpendResult result, MasterNodeSpec node)
        {
            switch (result)
            {
                case MasterSpendResult.NoPoints:
                    return ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.MasterNoPoints);
                case MasterSpendResult.MaxRank:
                    return ClassEvolutionTexts.Get(localization, ClassEvolutionTexts.MasterMaxRank);
                case MasterSpendResult.Locked:
                    MasterNodeSpec required = node != null ? MasterLevelRules.GetNode(node.RequiredNodeId) : null;
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterNodeLocked, node?.RequiredRank ?? 0,
                        required != null ? ClassEvolutionTexts.Get(localization, required.NameKey) : string.Empty);
                case MasterSpendResult.NotMaster:
                    return ClassEvolutionTexts.Format(localization, ClassEvolutionTexts.MasterLocked, MasterLevelRules.RequiredLevel);
                default:
                    return null;
            }
        }
    }
}
