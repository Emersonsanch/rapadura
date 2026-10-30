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
        public void Catalog_EveryCousinHasThreeGraduations_WithRisingLevels()
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
                    GraduationSkillSpec skill = ClassEvolutionCatalog.GetSkill(spec.SkillId);
                    Assert.IsNotNull(skill, spec.SkillId);
                    Assert.AreEqual(hero, skill.Character);
                    Assert.AreEqual(tier, skill.Tier);
                }
            }

            Assert.AreEqual(Heroes.Length * ClassEvolutionCatalog.MaxTier, ClassEvolutionCatalog.Graduations.Count);
            Assert.AreEqual(ClassEvolutionCatalog.Graduations.Count, ClassEvolutionCatalog.Skills.Count);
            Assert.AreEqual(ClassEvolutionCatalog.Skills.Count, ClassEvolutionCatalog.Skills.Select(s => s.SkillId).Distinct().Count());
        }

        [Test]
        public void Catalog_MentorsAreFamily_NeverThemselves_GrandmaEdnaForTheLast()
        {
            var cousins = new HashSet<string>(Heroes.Select(h => h.ToString().ToLowerInvariant()));
            foreach (GraduationSpec spec in ClassEvolutionCatalog.Graduations)
            {
                Assert.AreNotEqual(spec.Character.ToString().ToLowerInvariant(), spec.MentorId, spec.Id);
                if (spec.Tier == ClassEvolutionCatalog.MaxTier)
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
        public void Catalog_TintIsALightWash()
        {
            Assert.AreEqual(0f, ClassEvolutionCatalog.TintStrengthForTier(0));
            Assert.LessOrEqual(ClassEvolutionCatalog.TintStrengthForTier(3), 0.4f);
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
        public void Sanitize_RepairsBadRecords()
        {
            var record = new GraduationRecord { tier = 9, activeMissionTier = 2, missionKills = -4, masterLevel = 99, masterExperience = -1 };
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
        public void Master_OnlyWithTheThirdGraduationAtMaxLevel()
        {
            var record = Fresh(CharacterId.Icaro);
            record.tier = 2;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel));
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, MasterLevelRules.RequiredLevel, 1_000_000));
            record.tier = 3;
            Assert.IsFalse(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel - 1));
            Assert.IsTrue(MasterLevelRules.IsMaster(record, MasterLevelRules.RequiredLevel));
        }

        [Test]
        public void Master_ExperienceLevelsUp_AndStopsAtTheCap()
        {
            GraduationRecord record = Master(0);
            long first = MasterLevelRules.ExperienceToNext(0);
            Assert.Greater(MasterLevelRules.ExperienceToNext(1), first);
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, 50, first - 1));
            Assert.AreEqual(1, MasterLevelRules.AddExperience(record, 50, 1));
            Assert.AreEqual(1, record.masterLevel);
            Assert.AreEqual(0, record.masterExperience);
            Assert.AreEqual(1, MasterLevelRules.AvailablePoints(record));

            MasterLevelRules.AddExperience(record, 50, long.MaxValue / 4);
            Assert.AreEqual(MasterLevelRules.MaxMasterLevel, record.masterLevel);
            Assert.AreEqual(0, record.masterExperience);
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, 50, 1000));
        }

        [Test]
        public void MasterTree_RowTwoNeedsThreeRanksAbove_AndResetRefunds()
        {
            GraduationRecord record = Master(10);
            Assert.AreEqual(MasterSpendResult.Locked, MasterLevelRules.Spend(record, 50, "olho_vivo"));
            Assert.AreEqual(MasterSpendResult.UnknownNode, MasterLevelRules.Spend(record, 50, "nope"));
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, 50, "mao_firme"));
            }

            Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, 50, "olho_vivo"));
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
                MasterLevelRules.Spend(record, 50, "couro_curtido");
            }

            Assert.AreEqual(MasterSpendResult.MaxRank, MasterLevelRules.Spend(record, 50, "couro_curtido"));
            GraduationRecord poor = Master(0);
            Assert.AreEqual(MasterSpendResult.NoPoints, MasterLevelRules.Spend(poor, 50, "couro_curtido"));

            int totalRanks = MasterLevelRules.Nodes.Sum(n => n.MaxRank);
            Assert.AreEqual(MasterLevelRules.MaxMasterLevel * MasterLevelRules.PointsPerMasterLevel, totalRanks, "max master level fills the whole tree");
            foreach (MasterNodeSpec node in MasterLevelRules.Nodes)
            {
                Assert.LessOrEqual(node.MaxValue, node.Stat == StatType.ManaRegen ? 0.10001f : 0.05001f, node.NodeId);
            }
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
                foreach (string key in new[] { spec.TitleKey, spec.MissionTitleKey, spec.OfferKey, spec.ProgressKey, spec.TurnInKey, spec.ReplyKey, spec.AccessoryKey,
                             ContentText.SkillNameKey(spec.SkillId), ContentText.SkillDescriptionKey(spec.SkillId), ClassEvolutionCatalog.MentorNameKey(spec.MentorId) })
                {
                    Assert.IsTrue(keys.Contains(key), key);
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
            }

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
        }

        [Test]
        public void Presenter_MasterLines()
        {
            GraduationRecord record = Master(3);
            StringAssert.Contains("3", ClassEvolutionPresenter.MasterHeader(null, record, 50));
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
