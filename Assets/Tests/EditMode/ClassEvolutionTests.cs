using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.ClassEvolution;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.ClassEvolution;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// Class evolution (Docs/ClassEvolution.md): the graduation catalog and its family missions, the pure mission/master
    /// rules, the class combo tracker, group-support targeting, the skill factory, save round-trip, texts and the panel presenter.
    /// </summary>
    public class ClassEvolutionTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (UnityEngine.Object obj in _created)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private static GraduationRecord Fresh(CharacterId hero) => new GraduationRecord { characterId = hero.ToString() };

        private static GraduationRecord Master(int masterLevel)
        {
            var record = Fresh(CharacterId.Maithe);
            record.tier = ClassEvolutionCatalog.MaxTier;
            record.masterLevel = masterLevel;
            return record;
        }

        // ------------------------------------------------------------------ catalog

        [Test]
        public void Catalog_EveryCousinHasFiveGraduations_WithRisingLevels()
        {
            foreach (CharacterId hero in Heroes)
            {
                int previous = 0;
                for (int tier = 1; tier <= ClassEvolutionCatalog.MaxTier; tier++)
                {
                    GraduationSpec spec = ClassEvolutionCatalog.GetGraduation(hero, tier);
                    Assert.IsNotNull(spec, $"{hero} tier {tier}");
                    Assert.Greater(spec.MinLevel, previous, $"{hero} tier {tier} level");
                    Assert.LessOrEqual(spec.MinLevel, MasterLevelRules.RequiredLevel);
                    Assert.Greater(spec.MissionKills, 0);
                    previous = spec.MinLevel;
                    if (tier == ClassEvolutionCatalog.UpgradeTier)
                    {
                        GraduationUpgradeSpec passive = ClassEvolutionCatalog.GetUpgrade(spec.SkillId);
                        Assert.IsNotNull(passive, spec.SkillId);
                        Assert.IsTrue(spec.IsPassive);
                        Assert.AreEqual(hero, passive.Character);
                        Assert.AreSame(passive, ClassEvolutionCatalog.GetUpgrade(hero));
                        continue;
                    }

                    GraduationSkillSpec skill = ClassEvolutionCatalog.GetSkill(spec.SkillId);
                    Assert.IsNotNull(skill, spec.SkillId);
                    Assert.IsFalse(spec.IsPassive);
                    Assert.AreEqual(hero, skill.Character);
                    Assert.AreEqual(tier, skill.Tier);
                }
            }

            Assert.AreEqual(5, ClassEvolutionCatalog.MaxTier);
            Assert.AreEqual(Heroes.Length * ClassEvolutionCatalog.MaxTier, ClassEvolutionCatalog.Graduations.Count);
            Assert.AreEqual(ClassEvolutionCatalog.Graduations.Count - Heroes.Length, ClassEvolutionCatalog.Skills.Count, "G4 is a passive");
            Assert.AreEqual(Heroes.Length, ClassEvolutionCatalog.Upgrades.Count);
            Assert.AreEqual(ClassEvolutionCatalog.Skills.Count, ClassEvolutionCatalog.Skills.Select(s => s.SkillId).Distinct().Count());
        }

        [Test]
        public void Catalog_MentorsAreFamily_NeverThemselves_GrandmaEdnaForTheThirdAndTheFifth()
        {
            var cousins = new HashSet<string>(Heroes.Select(h => h.ToString().ToLowerInvariant()));
            var pairs = new HashSet<string>();
            foreach (GraduationSpec spec in ClassEvolutionCatalog.Graduations)
            {
                Assert.AreNotEqual(spec.Character.ToString().ToLowerInvariant(), spec.MentorId, spec.Id);
                if (spec.MentorId != ClassEvolutionCatalog.VoEdna)
                {
                    Assert.IsTrue(pairs.Add(spec.MentorId + ">" + spec.Character), "each cousin mentors another cousin once: " + spec.Id);
                }

                if (spec.Tier == ClassEvolutionCatalog.CampaignMaxTier || spec.Tier == ClassEvolutionCatalog.MaxTier)
                {
                    Assert.AreEqual(ClassEvolutionCatalog.VoEdna, spec.MentorId, spec.Id);
                }
                else
                {
                    Assert.IsTrue(cousins.Contains(spec.MentorId), spec.Id);
                }
            }

            Assert.AreEqual("vo_edna", ClassEvolutionCatalog.VoEdna);
        }

        [Test]
        public void Catalog_TierOneIsAGroupBuff_TierTwoClosesTheClassCombo()
        {
            foreach (CharacterId hero in Heroes)
            {
                GraduationSkillSpec first = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 1).SkillId);
                Assert.AreEqual(GraduationSkillKind.GroupBuff, first.Kind, hero.ToString());
                Assert.IsTrue(first.HasEffect);
                Assert.Greater(first.AreaRadius, 0f);

                ClassComboSpec combo = ClassEvolutionCatalog.GetCombo(hero);
                Assert.IsNotNull(combo, hero.ToString());
                Assert.AreEqual(3, combo.SkillIds.Count);
                Assert.AreEqual(ClassEvolutionCatalog.GetGraduation(hero, ClassEvolutionCatalog.ComboTier).SkillId, combo.SkillIds[2]);
                IReadOnlyList<SkillDefinition> own = CharacterSkillSet.GetSkills(hero);
                for (int i = 0; i < 2; i++)
                {
                    Assert.IsTrue(own.Any(s => s != null && s.SkillId == combo.SkillIds[i]), $"{combo.ComboId}: {combo.SkillIds[i]} is not {hero}'s");
                }

                Assert.LessOrEqual(combo.BonusValue, 0.25f, "modest combo bonus");
            }
        }

        [Test]
        public void Catalog_HighTiers_CountByCreatureLevel_AtTheirMilestones()
        {
            int[] expected = { 8, 14, 25, 150, 350 };
            for (int tier = 1; tier <= 5; tier++)
            {
                Assert.AreEqual(expected[tier - 1], ClassEvolutionCatalog.MilestoneLevel(tier), $"tier {tier}");
                Assert.IsTrue(ClassEvolutionCatalog.IsTierImplemented(tier));
            }

            Assert.IsFalse(ClassEvolutionCatalog.IsTierImplemented(6));
            int[] capitals = { 31, 41, 54, 59, 65 };
            foreach (CharacterId hero in Heroes)
            {
                for (int tier = 1; tier <= ClassEvolutionCatalog.MaxTier; tier++)
                {
                    GraduationSpec spec = ClassEvolutionCatalog.GetGraduation(hero, tier);
                    bool high = tier > ClassEvolutionCatalog.CampaignMaxTier;
                    Assert.AreEqual(high, spec.CountsByCreatureLevel, spec.Id);
                    Assert.AreEqual(high, spec.MissionRegion == null, spec.Id);
                    if (!high)
                    {
                        continue;
                    }

                    Assert.Less(spec.MissionMinCreatureLevel, spec.MinLevel, spec.Id);
                    if (tier == 4)
                    {
                        CollectionAssert.Contains(capitals, spec.MissionMapId, "G4 at the capital of the element");
                    }
                    else
                    {
                        Assert.AreEqual(91, spec.MissionMapId, "G5 in the Sky Realms");
                    }
                }
            }

            var capitalsUsed = Heroes.Select(h => ClassEvolutionCatalog.GetGraduation(h, 4).MissionMapId).Distinct().Count();
            Assert.AreEqual(Heroes.Length, capitalsUsed, "one capital per cousin");
        }

        [Test]
        public void Catalog_G4Passive_WidensTheGroupBuff_AndAddsControl_NeverDamage()
        {
            foreach (CharacterId hero in Heroes)
            {
                GraduationUpgradeSpec up = ClassEvolutionCatalog.GetUpgrade(hero);
                GraduationSkillSpec buff = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 1).SkillId);
                GraduationSkillSpec second = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 2).SkillId);
                Assert.AreEqual(second.SkillId, up.TargetSkillId, hero.ToString());

                Assert.AreSame(buff, ClassEvolutionCatalog.EffectiveSkill(buff, 3), "nothing before G4");
                Assert.AreSame(second, ClassEvolutionCatalog.EffectiveSkill(second, 3));

                GraduationSkillSpec wide = ClassEvolutionCatalog.EffectiveSkill(buff, 4);
                Assert.AreEqual(10f, buff.AreaRadius);
                Assert.AreEqual(14f, wide.AreaRadius, hero.ToString());
                Assert.AreEqual(buff.EffectValue, wide.EffectValue);

                GraduationSkillSpec control = ClassEvolutionCatalog.EffectiveSkill(second, 5);
                Assert.AreEqual(second.BaseDamage, control.BaseDamage, "no damage");
                Assert.AreEqual(second.DamagePerLevel, control.DamagePerLevel);
                Assert.AreEqual(second.CooldownSeconds, control.CooldownSeconds);
                bool moreStun = control.StunSeconds > second.StunSeconds;
                bool newSlow = !second.HasEffect && control.HasEffect && control.EffectStat == StatType.MoveSpeed && control.EffectValue < 0f;
                Assert.IsTrue(moreStun || newSlow, hero + ": a secondary control effect");
                Assert.LessOrEqual(control.StunSeconds - second.StunSeconds, 0.5f);

                GraduationSkillSpec third = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 3).SkillId);
                Assert.AreSame(third, ClassEvolutionCatalog.EffectiveSkill(third, 5), "only tiers 1 and 2 change");
            }
        }

        [Test]
        public void Catalog_G5Skill_SameBudgetPerSecondAsTheTierThreeOne()
        {
            foreach (CharacterId hero in Heroes)
            {
                GraduationSkillSpec t3 = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 3).SkillId);
                GraduationSkillSpec t5 = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(hero, 5).SkillId);
                Assert.AreEqual(GraduationSkillKind.Active, t5.Kind);
                Assert.AreEqual(30f, t5.CooldownSeconds, hero.ToString());
                Assert.Greater(t5.AreaRadius, 0f, "area");
                Assert.LessOrEqual(t5.AreaRadius, t3.AreaRadius);
                Assert.LessOrEqual(t5.BaseDamage / t5.CooldownSeconds, t3.BaseDamage / t3.CooldownSeconds + 1e-4f, hero + " damage/s");
                Assert.LessOrEqual(t5.DamagePerLevel / t5.CooldownSeconds, t3.DamagePerLevel / t3.CooldownSeconds + 1e-4f, hero + " damage/level/s");
                Assert.LessOrEqual(t5.StunSeconds / t5.CooldownSeconds, t3.StunSeconds / t3.CooldownSeconds + 1e-4f, hero + " stun/s");
                Assert.LessOrEqual(t5.BaseHealing / t5.CooldownSeconds, t3.BaseHealing / t3.CooldownSeconds + 1e-4f, hero + " heal/s");
                Assert.LessOrEqual(t5.EffectUptime, t3.EffectUptime + 1e-4f, hero + " slow uptime");
            }
        }

        [Test]
        public void Catalog_TintIsALightWash()
        {
            Assert.AreEqual(0f, ClassEvolutionCatalog.TintStrengthForTier(0));
            Assert.LessOrEqual(ClassEvolutionCatalog.TintStrengthForTier(3), 0.4f);
            Assert.LessOrEqual(ClassEvolutionCatalog.TintStrengthForTier(ClassEvolutionCatalog.MaxTier), 0.4f);
            Assert.Greater(ClassEvolutionCatalog.TintStrengthForTier(4), ClassEvolutionCatalog.TintStrengthForTier(3));
            GraduationSpec spec = ClassEvolutionCatalog.GetGraduation(CharacterId.Lavine, 1);
            Color tinted = ClassEvolutionVisuals.TintedColor(new Color(1f, 1f, 1f, 0.5f), spec);
            Assert.AreEqual(0.5f, tinted.a, 1e-5f);
            Assert.AreEqual(Mathf.Lerp(1f, spec.TintG, spec.TintStrength), tinted.g, 1e-5f);
            Assert.AreEqual(Color.red, ClassEvolutionVisuals.TintedColor(Color.red, null));
        }

        // ------------------------------------------------------------------ missions

        [Test]
        public void Mission_FullFlow_LevelGate_RegionOnly_TurnIn()
        {
            CharacterId hero = CharacterId.Joaquim;
            GraduationRecord record = Fresh(hero);
            GraduationSpec first = ClassEvolutionCatalog.GetGraduation(hero, 1);

            Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, record, first.MinLevel - 1));
            Assert.IsFalse(ClassEvolutionRules.Accept(hero, record, first.MinLevel - 1));
            Assert.AreEqual(GraduationMissionStatus.Available, ClassEvolutionRules.Status(hero, record, first.MinLevel));
            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, first.MissionRegion), "no kills before accepting");
            Assert.IsTrue(ClassEvolutionRules.Accept(hero, record, first.MinLevel));
            Assert.IsFalse(ClassEvolutionRules.Accept(hero, record, first.MinLevel), "already accepted");
            Assert.AreEqual(GraduationMissionStatus.InProgress, ClassEvolutionRules.Status(hero, record, first.MinLevel));

            BiomeType other = first.MissionRegion == BiomeType.Forest ? BiomeType.Caves : BiomeType.Forest;
            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, other));
            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, null));
            Assert.IsNull(ClassEvolutionRules.TurnIn(hero, record, first.MinLevel), "kills missing");
            for (int i = 0; i < first.MissionKills + 5; i++)
            {
                ClassEvolutionRules.RecordKill(hero, record, first.MissionRegion);
            }

            Assert.AreEqual(first.MissionKills, record.missionKills, "never past the target");
            Assert.AreEqual(GraduationMissionStatus.ReadyToTurnIn, ClassEvolutionRules.Status(hero, record, first.MinLevel));
            Assert.AreSame(first, ClassEvolutionRules.TurnIn(hero, record, first.MinLevel));
            Assert.AreEqual(1, record.tier);
            Assert.AreEqual(0, record.activeMissionTier);
            Assert.AreEqual(0, record.missionKills);
            Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, record, first.MinLevel));

            record.tier = ClassEvolutionCatalog.MaxTier;
            Assert.AreEqual(GraduationMissionStatus.AllDone, ClassEvolutionRules.Status(hero, record, 99));
            Assert.IsNull(ClassEvolutionRules.NextGraduation(hero, record.tier));
        }

        [Test]
        public void Mission_HighTiers_CountCreaturesByLevel_AnyRegion()
        {
            CharacterId hero = CharacterId.Icaro;
            GraduationRecord record = Fresh(hero);
            record.tier = ClassEvolutionCatalog.CampaignMaxTier;
            GraduationSpec g4 = ClassEvolutionCatalog.GetGraduation(hero, 4);
            Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, record, 100));
            Assert.AreEqual(GraduationMissionStatus.Available, ClassEvolutionRules.Status(hero, record, g4.MinLevel));
            Assert.IsTrue(ClassEvolutionRules.Accept(hero, record, g4.MinLevel));

            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, BiomeType.Caves, 15), "campaign creatures do not count");
            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, null, g4.MissionMinCreatureLevel - 1));
            Assert.IsTrue(ClassEvolutionRules.RecordKill(hero, record, null, g4.MissionMinCreatureLevel));
            Assert.IsTrue(ClassEvolutionRules.RecordKill(hero, record, BiomeType.Forest, 200), "any map");
            record.missionKills = g4.MissionKills;
            Assert.AreSame(g4, ClassEvolutionRules.TurnIn(hero, record, g4.MinLevel));
            Assert.AreEqual(4, record.tier);

            GraduationSpec g5 = ClassEvolutionCatalog.GetGraduation(hero, 5);
            Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, record, g5.MinLevel - 1));
            Assert.IsTrue(ClassEvolutionRules.Accept(hero, record, g5.MinLevel));
            Assert.IsFalse(ClassEvolutionRules.RecordKill(hero, record, null, g4.MissionMinCreatureLevel), "G5 asks for stronger creatures");
            record.missionKills = g5.MissionKills;
            Assert.AreSame(g5, ClassEvolutionRules.TurnIn(hero, record, g5.MinLevel));
            Assert.AreEqual(GraduationMissionStatus.AllDone, ClassEvolutionRules.Status(hero, record, 500));
        }

        [Test]
        public void Sanitize_RepairsBadRecords()
        {
            var record = new GraduationRecord { tier = 9, activeMissionTier = 2, missionKills = -4, masterLevel = 999, masterExperience = -1 };
            ClassEvolutionRules.Sanitize(CharacterId.Maria, record);
            Assert.AreEqual(ClassEvolutionCatalog.MaxTier, record.tier);
            Assert.AreEqual(0, record.activeMissionTier);
            Assert.AreEqual(0, record.missionKills);
            Assert.AreEqual(MasterLevelRules.MaxMasterLevel, record.masterLevel);
            Assert.AreEqual(0, record.masterExperience);
            Assert.AreEqual("Maria", record.characterId);

            var mission = new GraduationRecord { tier = 1, activeMissionTier = 2, missionKills = 999 };
            ClassEvolutionRules.Sanitize(CharacterId.Maria, mission);
            Assert.AreEqual(ClassEvolutionCatalog.GetGraduation(CharacterId.Maria, 2).MissionKills, mission.missionKills);
        }

        // ------------------------------------------------------------------ master level

        [Test]
        public void Master_OnlyWithTheFifthGraduationAtMaxLevel()
        {
            var record = Fresh(CharacterId.Icaro);
            record.tier = 2;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel));
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, 1_000_000));
            record.tier = 3;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel), "the 3rd graduation is not enough anymore");
            record.tier = 4;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel));
            record.tier = 5;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel - 1));
            Assert.IsTrue(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel));
        }

        [Test]
        public void Master_ExperienceLevelsUp_AndStopsAtTheCap()
        {
            GraduationRecord record = Master(0);
            long first = MasterLevelRules.ExperienceToNext(0);
            Assert.Greater(MasterLevelRules.ExperienceToNext(1), first);
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, first - 1));
            Assert.AreEqual(1, MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, 1));
            Assert.AreEqual(1, record.masterLevel);
            Assert.AreEqual(0, record.masterExperience);
            Assert.AreEqual(1, MasterLevelRules.AvailablePoints(record));

            MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, long.MaxValue / 4);
            Assert.AreEqual(MasterLevelRules.MaxMasterLevel, record.masterLevel);
            Assert.AreEqual(0, record.masterExperience);
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, 1000));
        }

        [Test]
        public void MasterTree_RowTwoNeedsThreeRanksAbove_AndResetRefunds()
        {
            GraduationRecord record = Master(10);
            Assert.AreEqual(MasterSpendResult.Locked, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "olho_vivo"));
            Assert.AreEqual(MasterSpendResult.UnknownNode, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "nope"));
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "mao_firme"));
            }

            Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "olho_vivo"));
            Assert.AreEqual(3, MasterLevelRules.RankOf(record, "mao_firme"));
            Assert.AreEqual(0.015f, MasterLevelRules.Bonus(record, StatType.AttackDamage), 1e-6f);
            Assert.AreEqual(6, MasterLevelRules.AvailablePoints(record));
            Assert.AreEqual(MasterSpendResult.NotMaster, MasterLevelRules.Spend(record, 49, "mao_firme"));

            Assert.AreEqual(4, MasterLevelRules.Reset(record));
            Assert.AreEqual(10, MasterLevelRules.AvailablePoints(record));
            Assert.AreEqual(0, MasterLevelRules.Modifiers(record).Count);
        }

        [Test]
        public void MasterTree_MaxRank_NoPoints_AndModestTotals()
        {
            GraduationRecord record = Master(MasterLevelRules.MaxMasterLevel);
            for (int i = 0; i < 5; i++)
            {
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "couro_curtido");
            }

            Assert.AreEqual(MasterSpendResult.MaxRank, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "couro_curtido"));
            GraduationRecord poor = Master(0);
            Assert.AreEqual(MasterSpendResult.NoPoints, MasterLevelRules.Spend(poor, MasterLevelRules.RequiredLevel, "couro_curtido"));

            int totalRanks = MasterLevelRules.Nodes.Sum(n => n.MaxRank);
            Assert.AreEqual(100, MasterLevelRules.MaxMasterLevel);
            Assert.AreEqual(MasterLevelRules.MaxMasterLevel * MasterLevelRules.PointsPerMasterLevel, totalRanks, "max master level fills the whole tree");
            Assert.AreEqual(totalRanks, MasterLevelRules.TreePoints);
            Assert.AreEqual(30, MasterLevelRules.Nodes.Where(n => n.Branch == MasterTreeBranch.Core).Sum(n => n.MaxRank));
            Assert.AreEqual(25, MasterLevelRules.Nodes.Where(n => n.Branch == MasterTreeBranch.Support).Sum(n => n.MaxRank));
            Assert.AreEqual(25, MasterLevelRules.Nodes.Where(n => n.Branch == MasterTreeBranch.Utility).Sum(n => n.MaxRank));
            Assert.AreEqual(20, MasterLevelRules.Nodes.Where(n => n.Branch == MasterTreeBranch.Bond).Sum(n => n.MaxRank));
            Assert.AreEqual(MasterLevelRules.Nodes.Count, MasterLevelRules.Nodes.Select(n => n.NodeId).Distinct().Count());

            var offence = new[] { StatType.AttackDamage, StatType.CriticalChance, StatType.CriticalDamage, StatType.AttackSpeed,
                StatType.CooldownReduction, StatType.ResourceCostReduction, StatType.Thorns };
            foreach (MasterNodeSpec node in MasterLevelRules.Nodes)
            {
                if (node.RequiredNodeId != null)
                {
                    MasterNodeSpec above = MasterLevelRules.GetNode(node.RequiredNodeId);
                    Assert.IsNotNull(above, node.NodeId);
                    Assert.AreEqual(node.Branch, above.Branch, node.NodeId + " needs a node of its own line");
                }

                if (node.Branch == MasterTreeBranch.Core)
                {
                    Assert.LessOrEqual(node.MaxValue, node.Stat == StatType.ManaRegen ? 0.10001f : 0.05001f, node.NodeId);
                    continue;
                }

                Assert.IsFalse(node.IsHeroStat && offence.Contains(node.Stat), node.NodeId + ": the new lines never add damage");
                Assert.AreEqual(node.Branch == MasterTreeBranch.Bond, node.Effect >= MasterNodeEffect.MountStamina, node.NodeId + ": Bond = mount and Ari");
            }
        }

        [Test]
        public void MasterTree_NewLines_SpecialEffects()
        {
            GraduationRecord record = Master(100);
            Assert.AreEqual(0f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.GroupBuffRadius));
            Assert.AreEqual(MasterSpendResult.Locked, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "hino_do_chamado"));
            for (int i = 0; i < 5; i++)
            {
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "luz_partilhada");
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "faro_do_ari");
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "laco_da_sela");
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "agua_viva");
            }

            Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "hino_do_chamado"));
            Assert.AreEqual(2f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.GroupBuffRadius), 1e-5f);
            Assert.AreEqual(0.04f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.GroupBuffDuration), 1e-5f);
            Assert.AreEqual(0.3f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.CompanionAlertRadius), 1e-5f);
            Assert.AreEqual(0.1f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.MountStamina), 1e-5f);
            Assert.AreEqual(0f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.HeroStat));
            Assert.AreEqual(1f, MasterLevelRules.Bonus(record, StatType.HealthRegen), 1e-5f);
            // Only hero-stat nodes become modifiers (agua_viva); the special ones are read by their systems.
            Assert.AreEqual(1, MasterLevelRules.Modifiers(record).Count);
            Assert.AreEqual(0f, MasterLevelRules.Bonus(record, default(StatType)), "special nodes never leak into a stat");
        }

        [Test]
        public void MasterTree_HearersMemory_BoostsMasterXp()
        {
            GraduationRecord record = Master(10);
            for (int i = 0; i < 3; i++)
            {
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "taca_do_bema");
            }

            for (int i = 0; i < 5; i++)
            {
                MasterLevelRules.Spend(record, MasterLevelRules.RequiredLevel, "memoria_do_ouvinte");
            }

            Assert.AreEqual(0.1f, MasterLevelRules.EffectBonus(record, MasterNodeEffect.MasterExperience), 1e-5f);
            MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, 1000);
            Assert.AreEqual(1100, record.masterExperience);
        }

        [Test]
        public void MasterTree_SanitizeTrimsOverspentAndUnknownNodes()
        {
            GraduationRecord record = Master(2);
            record.masterNodes.Add(new MasterNodeRank { nodeId = "couro_curtido", rank = 9 });
            record.masterNodes.Add(new MasterNodeRank { nodeId = "ghost", rank = 1 });
            record.masterNodes.Add(new MasterNodeRank { nodeId = "couro_curtido", rank = 1 });
            MasterLevelRules.Sanitize(record);
            Assert.AreEqual(1, record.masterNodes.Count);
            Assert.AreEqual(2, MasterLevelRules.SpentPoints(record));
        }

        // ------------------------------------------------------------------ combo + group support

        [Test]
        public void ClassCombo_InOrderWithinWindow_Completes_WrongCastResets()
        {
            ClassComboSpec combo = ClassEvolutionCatalog.GetCombo(CharacterId.Maithe);
            var tracker = new ClassComboTracker(combo);
            Assert.IsFalse(tracker.OnCast(combo.SkillIds[0], 0));
            Assert.IsFalse(tracker.OnCast(combo.SkillIds[1], 1));
            Assert.IsTrue(tracker.OnCast(combo.SkillIds[2], 2));
            Assert.AreEqual(0, tracker.Step);

            Assert.IsFalse(tracker.OnCast(combo.SkillIds[0], 10));
            Assert.IsFalse(tracker.OnCast(combo.SkillIds[1], 10 + combo.WindowSeconds + 0.5));
            Assert.AreEqual(0, tracker.Step, "too slow");

            tracker.OnCast(combo.SkillIds[0], 20);
            tracker.OnCast("skill_other", 20.5);
            Assert.AreEqual(0, tracker.Step);
            tracker.OnCast(combo.SkillIds[0], 21);
            tracker.OnCast(combo.SkillIds[0], 21.5);
            Assert.AreEqual(1, tracker.Step, "repeating the first skill restarts the chain");
        }

        [Test]
        public void HandleCast_NeedsTheComboTier()
        {
            var progress = new ClassEvolutionProgress();
            var caster = new GameObject("ClassComboCaster");
            _created.Add(caster);
            ClassComboSpec combo = ClassEvolutionCatalog.GetCombo(CharacterId.Lavine);
            Assert.IsFalse(progress.HandleCast(caster, CharacterId.Lavine, combo.SkillIds[0], 0));
            progress.Record(CharacterId.Lavine).tier = ClassEvolutionCatalog.ComboTier;
            progress.HandleCast(caster, CharacterId.Lavine, combo.SkillIds[0], 0);
            progress.HandleCast(caster, CharacterId.Lavine, combo.SkillIds[1], 1);
            Assert.IsTrue(progress.HandleCast(caster, CharacterId.Lavine, combo.SkillIds[2], 2));
        }

        [Test]
        public void PartySupport_ClosestLivingAlliesInRadius_UpToFour()
        {
            var candidates = new List<SupportCandidate>
            {
                new SupportCandidate("far", 50, 0),
                new SupportCandidate("dead", 1, 0, alive: false),
                new SupportCandidate("a", 5, 0),
                new SupportCandidate("b", 1, 1),
                new SupportCandidate("c", 0, 3),
                new SupportCandidate("d", 2, 2),
                new SupportCandidate("e", 0, 4),
                new SupportCandidate("b", 1, 1),
            };
            List<string> picked = PartySupportRules.SelectAllies(0, 0, 10, candidates);
            CollectionAssert.AreEqual(new[] { "b", "d", "c", "e" }, picked);
            Assert.IsEmpty(PartySupportRules.SelectAllies(0, 0, 0, candidates));
            Assert.IsEmpty(PartySupportRules.SelectAllies(0, 0, 10, null));
        }

        [Test]
        public void SpreadGroupBuff_ReachesAlliesInRange_NotTheCasterNorFarOnes()
        {
            GraduationSkillSpec spec = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(CharacterId.Joaquim, 1).SkillId);
            var caster = new GameObject("Caster");
            var near = new GameObject("Near");
            var far = new GameObject("Far");
            _created.AddRange(new UnityEngine.Object[] { caster, near, far });
            caster.AddComponent<BuffController>();
            BuffController nearBuffs = near.AddComponent<BuffController>();
            BuffController farBuffs = far.AddComponent<BuffController>();
            near.transform.position = new Vector3(2, 0, 0);
            far.transform.position = new Vector3(spec.AreaRadius + 5, 0, 0);

            Assert.AreEqual(1, ClassEvolutionProgress.SpreadGroupBuff(caster, spec, new[] { caster, near, far }));
            Assert.IsTrue(nearBuffs.HasEffectsFromSource(ClassEvolutionProgress.GroupBuffSourcePrefix + spec.SkillId));
            Assert.IsFalse(farBuffs.HasEffectsFromSource(ClassEvolutionProgress.GroupBuffSourcePrefix + spec.SkillId));
            // G4 (14 m) + "Luz Partilhada" (+2 m) reach the far one at 15 m.
            Assert.AreEqual(1, ClassEvolutionProgress.SpreadGroupBuff(caster, ClassEvolutionCatalog.EffectiveSkill(spec, 4), new[] { far }, 2f, 1.2f));
            Assert.IsTrue(farBuffs.HasEffectsFromSource(ClassEvolutionProgress.GroupBuffSourcePrefix + spec.SkillId));
            Assert.AreEqual(spec.EffectDurationSeconds * 1.2f, GraduationSkillFactory.AllyEffect(spec, 1.2f).durationSeconds, 1e-4f);
            GraduationSkillSpec active = ClassEvolutionCatalog.GetSkill(ClassEvolutionCatalog.GetGraduation(CharacterId.Joaquim, 2).SkillId);
            Assert.AreEqual(0, ClassEvolutionProgress.SpreadGroupBuff(caster, active, new[] { near }));
        }

        // ------------------------------------------------------------------ skills

        [Test]
        public void Factory_BuildsGroupBuffsAndActives()
        {
            foreach (GraduationSkillSpec spec in ClassEvolutionCatalog.Skills)
            {
                SkillDefinition skill = GraduationSkillFactory.Build(spec);
                _created.Add(skill);
                Assert.AreEqual(spec.SkillId, skill.SkillId);
                Assert.AreEqual(spec.CooldownSeconds, skill.CooldownSeconds, 1e-5f);
                Assert.AreEqual(spec.ManaCost, skill.ManaCost, 1e-5f);
                Assert.AreEqual(ClassEvolutionCatalog.GetGraduation(spec.Character, spec.Tier).MinLevel, skill.Requirement.minimumPlayerLevel);
                Assert.AreNotEqual(spec.SkillId, skill.DisplayName, "pt-BR fallback name");
                if (spec.Kind == GraduationSkillKind.GroupBuff)
                {
                    Assert.AreEqual(1, skill.Buffs.Length, spec.SkillId);
                    Assert.IsNotNull(GraduationSkillFactory.AllyEffect(spec));
                }
                else
                {
                    Assert.Greater(skill.BaseDamage, 0f, spec.SkillId);
                    Assert.AreEqual(spec.HasEffect ? 1 : 0, skill.Debuffs.Length, spec.SkillId);
                    Assert.IsNull(GraduationSkillFactory.AllyEffect(spec));
                }
            }

            Assert.AreSame(GraduationSkillFactory.Get("skill_grad_maria_sol_do_meio_dia"), GraduationSkillFactory.Get("skill_grad_maria_sol_do_meio_dia"));

            // G4 rewrites the same cached instance (learned skills keep their level and slot), and back.
            const string coil = "skill_grad_icaro_bobina_de_cata_vento";
            SkillDefinition before = GraduationSkillFactory.Get(coil, 2);
            Assert.AreEqual(0, before.Debuffs.Length);
            Assert.AreSame(before, GraduationSkillFactory.Get(coil, 4));
            Assert.AreEqual(1, before.Debuffs.Length, "the G4 slow");
            Assert.AreSame(before, GraduationSkillFactory.Get(coil), "no tier: the cached variant as is");
            Assert.AreEqual(1, before.Debuffs.Length);
            GraduationSkillFactory.Get(coil, 0);
            Assert.AreEqual(0, before.Debuffs.Length);
            Assert.AreSame(GraduationSkillFactory.Get("skill_grad_maria_sol_do_meio_dia"), CharacterSkillSet.FindSkill("skill_grad_maria_sol_do_meio_dia"), "save restore resolves it");
            Assert.IsNull(GraduationSkillFactory.Get("skill_nope"));
        }

        [Test]
        public void GrantSkills_LearnsOwnTiers_ForgetsOtherCousins()
        {
            var go = new GameObject("GradSkillCaster");
            _created.Add(go);
            SkillManager skills = go.AddComponent<SkillManager>();
            Assert.AreEqual(2, ClassEvolutionProgress.GrantSkills(skills, CharacterId.Maithe, 2, 14));
            Assert.IsTrue(skills.IsSkillLearned("skill_grad_maithe_olhar_do_carcara"));
            Assert.IsTrue(skills.IsSkillLearned("skill_grad_maithe_flecha_da_ventania"));
            Assert.IsFalse(skills.IsSkillLearned("skill_grad_maithe_redemoinho_do_sertao"));
            Assert.AreEqual(0, ClassEvolutionProgress.GrantSkills(skills, CharacterId.Maithe, 2, 14), "idempotent");

            ClassEvolutionProgress.GrantSkills(skills, CharacterId.Lavine, 0, 14);
            Assert.IsFalse(skills.IsSkillLearned("skill_grad_maithe_olhar_do_carcara"));
            Assert.AreEqual(0, ClassEvolutionProgress.GrantSkills(null, CharacterId.Lavine, 3, 50));
        }

        [Test]
        public void ApplyTo_AddsAccessoriesPerTier_AndClearsAtTierZero()
        {
            var progress = new ClassEvolutionProgress();
            var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _created.Add(hero);
            progress.Record(CharacterId.Maria).tier = 2;
            progress.ApplyTo(hero, CharacterId.Maria, 14);
            int accessories = 0;
            foreach (Transform child in hero.transform)
            {
                accessories += child.name.StartsWith(ClassEvolutionVisuals.AccessoryPrefix) ? 1 : 0;
            }

            Assert.AreEqual(2, accessories);
            ClassEvolutionVisuals.Apply(hero, CharacterId.Maria, 0);
            foreach (Transform child in hero.transform)
            {
                Assert.IsFalse(child.name.StartsWith(ClassEvolutionVisuals.AccessoryPrefix));
            }
        }

        // ------------------------------------------------------------------ save

        [Test]
        public void Save_RoundTrip_ThroughJson()
        {
            var progress = new ClassEvolutionProgress();
            GraduationRecord record = progress.Record(CharacterId.Icaro);
            record.tier = 3;
            record.masterLevel = 4;
            record.masterExperience = 123;
            record.masterNodes.Add(new MasterNodeRank { nodeId = "mao_firme", rank = 3 });
            progress.Record(CharacterId.Maria).tier = 1;
            progress.Record(CharacterId.Maria).activeMissionTier = 2;
            progress.Record(CharacterId.Maria).missionKills = 7;

            string json = JsonUtility.ToJson(progress.CaptureState());
            var restored = new ClassEvolutionProgress();
            restored.RestoreState(JsonUtility.FromJson<ClassEvolutionSaveState>(json));

            Assert.AreEqual(3, restored.Tier(CharacterId.Icaro));
            Assert.AreEqual(4, restored.Record(CharacterId.Icaro).masterLevel);
            Assert.AreEqual(123, restored.Record(CharacterId.Icaro).masterExperience);
            Assert.AreEqual(3, MasterLevelRules.RankOf(restored.Record(CharacterId.Icaro), "mao_firme"));
            Assert.AreEqual(7, restored.Record(CharacterId.Maria).missionKills);
            Assert.AreEqual(0, restored.Tier(CharacterId.Lavine));

            restored.ResetForNewSession();
            Assert.AreEqual(0, restored.Tier(CharacterId.Icaro));
            restored.RestoreState("garbage");
            Assert.AreEqual(0, restored.Tier(CharacterId.Icaro));
        }

        // ------------------------------------------------------------------ texts + presenter

        [Test]
        public void Texts_EveryGraduationKeyExists_AndUseTheNewGrandparents()
        {
            var keys = new HashSet<string>(ClassEvolutionTexts.Entries.Select(e => e.Key));
            foreach (GraduationSpec spec in ClassEvolutionCatalog.Graduations)
            {
                GraduationUpgradeSpec passive = ClassEvolutionCatalog.GetUpgrade(spec.SkillId);
                foreach (string key in new[] { spec.TitleKey, spec.MissionTitleKey, spec.OfferKey, spec.ProgressKey, spec.TurnInKey, spec.ReplyKey, spec.AccessoryKey,
                             passive != null ? passive.NameKey : ContentText.SkillNameKey(spec.SkillId),
                             passive != null ? passive.DescriptionKey : ContentText.SkillDescriptionKey(spec.SkillId),
                             ClassEvolutionCatalog.MentorNameKey(spec.MentorId), ClassEvolutionTexts.MissionPlaceKey(spec) })
                {
                    Assert.IsTrue(keys.Contains(key), key);
                }

                if (spec.CountsByCreatureLevel)
                {
                    StringAssert.Contains(spec.MissionMinCreatureLevel.ToString(), ClassEvolutionTexts.GetFallback(spec.OfferKey), spec.Id + " offer names the creature level");
                }
            }

            foreach (ClassComboSpec combo in ClassEvolutionCatalog.Combos)
            {
                Assert.IsTrue(keys.Contains(ContentText.ComboNameKey(combo.ComboId)));
                Assert.IsTrue(keys.Contains(ContentText.ComboDescriptionKey(combo.ComboId)));
            }

            foreach (MasterNodeSpec node in MasterLevelRules.Nodes)
            {
                Assert.IsTrue(keys.Contains(node.NameKey) && keys.Contains(node.DescriptionKey), node.NodeId);
                Assert.IsTrue(keys.Contains(ClassEvolutionTexts.BranchKey(node.Branch)), node.Branch.ToString());
            }

            Assert.AreEqual(keys.Count, ClassEvolutionTexts.Entries.Count, "no duplicated key");

            foreach (var entry in ClassEvolutionTexts.Entries)
            {
                foreach (string text in new[] { entry.En, entry.PtBR })
                {
                    StringAssert.DoesNotContain("Zefa", text, entry.Key);
                    StringAssert.DoesNotContain("Chico", text, entry.Key);
                }
            }
        }

        [Test]
        public void Presenter_DialogueFollowsTheMissionState()
        {
            CharacterId hero = CharacterId.Lavine;
            GraduationRecord record = Fresh(hero);
            GraduationSpec first = ClassEvolutionCatalog.GetGraduation(hero, 1);
            Assert.IsEmpty(ClassEvolutionPresenter.DialogueLines(null, hero, record, 1));
            StringAssert.Contains(first.MinLevel.ToString(), ClassEvolutionPresenter.StatusLine(null, hero, record, 1));

            List<string> offer = ClassEvolutionPresenter.DialogueLines(null, hero, record, first.MinLevel);
            Assert.AreEqual(1, offer.Count);
            StringAssert.StartsWith("Maithe", offer[0]);

            ClassEvolutionRules.Accept(hero, record, first.MinLevel);
            ClassEvolutionRules.RecordKill(hero, record, first.MissionRegion);
            StringAssert.Contains("1/" + first.MissionKills, ClassEvolutionPresenter.DialogueLines(null, hero, record, first.MinLevel)[0]);

            record.missionKills = first.MissionKills;
            List<string> turnIn = ClassEvolutionPresenter.DialogueLines(null, hero, record, first.MinLevel);
            Assert.AreEqual(2, turnIn.Count);
            StringAssert.StartsWith("Lavine", turnIn[1]);

            GraduationSpec done = ClassEvolutionRules.TurnIn(hero, record, first.MinLevel);
            Assert.AreEqual(3, ClassEvolutionPresenter.GraduatedLines(null, hero, done).Count);
            Assert.AreEqual(4, ClassEvolutionPresenter.GraduatedLines(null, hero, ClassEvolutionCatalog.GetGraduation(hero, 2)).Count, "tier 2 unlocks the combo");
            StringAssert.Contains(ClassEvolutionCatalog.ComboTier.ToString(), ClassEvolutionPresenter.ComboLines(null, hero, 1)[1]);

            GraduationSpec g4 = ClassEvolutionCatalog.GetGraduation(hero, 4);
            StringAssert.StartsWith(ClassEvolutionTexts.GetFallback(ClassEvolutionTexts.NewPassive).Split('{')[0],
                ClassEvolutionPresenter.GraduatedLines(null, hero, g4)[1], "G4 brings a passive, not a skill");

            var high = Fresh(hero);
            high.tier = 3;
            ClassEvolutionRules.Accept(hero, high, g4.MinLevel);
            string status = ClassEvolutionPresenter.StatusLine(null, hero, high, g4.MinLevel);
            StringAssert.Contains(g4.MissionMinCreatureLevel + "+", status);
            StringAssert.Contains(ClassEvolutionTexts.GetFallback(ClassEvolutionTexts.MapPlaceKey(g4.MissionMapId)), status);
        }

        [Test]
        public void Presenter_MasterLines()
        {
            GraduationRecord record = Master(3);
            StringAssert.Contains("3", ClassEvolutionPresenter.MasterHeader(null, record, MasterLevelRules.RequiredLevel));
            StringAssert.Contains(MasterLevelRules.RequiredLevel.ToString(), ClassEvolutionPresenter.MasterHeader(null, record, 49));
            Assert.IsNull(ClassEvolutionPresenter.SpendFailure(null, MasterSpendResult.Spent, null));
            MasterNodeSpec locked = MasterLevelRules.GetNode("olho_vivo");
            StringAssert.Contains(ClassEvolutionTexts.GetFallback(MasterLevelRules.GetNode("mao_firme").NameKey),
                ClassEvolutionPresenter.SpendFailure(null, MasterSpendResult.Locked, locked));
            record.masterLevel = MasterLevelRules.MaxMasterLevel;
            Assert.AreEqual(ClassEvolutionTexts.GetFallback(ClassEvolutionTexts.MasterMax), ClassEvolutionPresenter.MasterXpLine(null, record));
        }
    }
}
