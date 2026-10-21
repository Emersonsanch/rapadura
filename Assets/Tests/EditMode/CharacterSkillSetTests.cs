using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the 25 per-character signature skills (5 heroes × 5 skills) shipped as real
    /// <see cref="SkillDefinition"/> assets under Resources/Skills/Characters/&lt;CharacterId&gt; and
    /// resolved by <see cref="CharacterSkillSet"/>. Loads the actual Resources assets (not seeded
    /// copies), so a missing/misnamed asset or a broken script GUID fails here.
    /// </summary>
    public class CharacterSkillSetTests
    {
        private static readonly CharacterId[] AllCharacters = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            CharacterSkillSet.Invalidate();
            EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            CharacterSkillSet.Invalidate();
            EventBus.Clear();

            if (_go != null)
            {
                UnityEngine.Object.DestroyImmediate(_go);
                _go = null;
            }
        }

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void EachCharacter_HasExactlyFiveSkills_LoadedFromResources(CharacterId id)
        {
            SkillDefinition[] loaded = Resources.LoadAll<SkillDefinition>(CharacterSkillSet.GetResourcesFolder(id));

            Assert.AreEqual(CharacterSkillSet.SkillsPerCharacter, loaded.Length, $"Resources folder for {id}.");
            Assert.AreEqual(CharacterSkillSet.SkillsPerCharacter, CharacterSkillSet.GetSkills(id).Count);
            Assert.That(CharacterSkillSet.GetSkills(id), Has.All.Not.Null);
        }

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void Skills_MatchAbilityNames_InOrder(CharacterId id)
        {
            string[] expected = CharacterRegistry.Get(id).AbilityNames;
            string[] actual = CharacterSkillSet.GetSkills(id).Select(s => s.DisplayName).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void FourActivesThenOneUltimate(CharacterId id)
        {
            IReadOnlyList<SkillDefinition> skills = CharacterSkillSet.GetSkills(id);

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(SkillType.Active, skills[i].SkillType, skills[i].SkillId);
            }

            Assert.AreEqual(SkillType.Ultimate, skills[4].SkillType, skills[4].SkillId);
        }

        [Test]
        public void AllSkillIds_AreUnique_AndPrefixedWithCharacter()
        {
            var ids = new List<string>();

            foreach (CharacterId id in AllCharacters)
            {
                string prefix = "skill_" + id.ToString().ToLowerInvariant() + "_";

                foreach (SkillDefinition skill in CharacterSkillSet.GetSkills(id))
                {
                    StringAssert.StartsWith(prefix, skill.SkillId);
                    Assert.AreEqual(skill.SkillId, skill.name, "Asset file name should match skill id.");
                    ids.Add(skill.SkillId);
                }
            }

            Assert.AreEqual(25, ids.Count);
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Character skill ids must be unique.");
        }

        [Test]
        public void CharacterSkillIds_DoNotCollideWithGenericSkills()
        {
            var genericIds = new HashSet<string>(
                Resources.LoadAll<SkillDefinition>("Skills")
                    .Where(s => string.IsNullOrEmpty(s.Requirement.requiredClassOrSpecialization))
                    .Select(s => s.SkillId));

            foreach (CharacterId id in AllCharacters)
            {
                foreach (SkillDefinition skill in CharacterSkillSet.GetSkills(id))
                {
                    Assert.IsFalse(genericIds.Contains(skill.SkillId), skill.SkillId);
                }
            }
        }

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void Values_AreSane(CharacterId id)
        {
            IReadOnlyList<SkillDefinition> skills = CharacterSkillSet.GetSkills(id);
            float longestActiveCooldown = 0f;

            foreach (SkillDefinition skill in skills)
            {
                string label = skill.SkillId;

                Assert.AreEqual(id.ToString(), skill.Requirement.requiredClassOrSpecialization, label);
                Assert.IsFalse(string.IsNullOrWhiteSpace(skill.Description), label);
                Assert.IsFalse(string.IsNullOrWhiteSpace(skill.AnimationTrigger), label);
                Assert.Greater(skill.CooldownSeconds, 0f, label);
                Assert.GreaterOrEqual(skill.ManaCost, 0f, label);
                Assert.LessOrEqual(skill.ManaCost, 100f, label);
                Assert.GreaterOrEqual(skill.CastTimeSeconds, 0f, label);
                Assert.LessOrEqual(skill.CastTimeSeconds, 2f, label);
                Assert.Greater(skill.Range, 0f, label);
                Assert.GreaterOrEqual(skill.AreaRadius, 0f, label);
                Assert.GreaterOrEqual(skill.MaxLevel, 1, label);
                Assert.GreaterOrEqual(skill.GetDamageForLevel(skill.MaxLevel), skill.BaseDamage, label);
                Assert.GreaterOrEqual(skill.GetHealingForLevel(skill.MaxLevel), skill.BaseHealing, label);

                bool doesSomething = skill.BaseDamage > 0f || skill.BaseHealing > 0f
                    || skill.Buffs.Length > 0 || skill.Debuffs.Length > 0;
                Assert.IsTrue(doesSomething, $"{label} has no damage, healing, buff or debuff.");

                AssertEffectsSane(skill.Buffs, expectDebuff: false, label);
                AssertEffectsSane(skill.Debuffs, expectDebuff: true, label);

                if (skill.SkillType == SkillType.Active)
                {
                    longestActiveCooldown = Mathf.Max(longestActiveCooldown, skill.CooldownSeconds);
                    Assert.AreEqual(1, skill.Requirement.minimumPlayerLevel, $"{label} should be usable from level 1.");
                }
            }

            SkillDefinition ultimate = skills[4];
            Assert.Greater(ultimate.CooldownSeconds, longestActiveCooldown, "Ultimate should have the longest cooldown.");
            Assert.Greater(ultimate.Requirement.minimumPlayerLevel, 1, "Ultimate should be level-gated.");
            CollectionAssert.Contains(ultimate.Requirement.requiredSkills, skills[0], "Ultimate should require the first signature skill.");
        }

        private static void AssertEffectsSane(StatModifierDefinition[] effects, bool expectDebuff, string label)
        {
            var seenStats = new HashSet<StatType>();

            foreach (StatModifierDefinition effect in effects)
            {
                Assert.IsNotNull(effect, label);
                Assert.AreEqual(expectDebuff, effect.isDebuff, $"{label}/{effect.displayName} isDebuff flag.");
                Assert.Greater(effect.durationSeconds, 0f, $"{label}/{effect.displayName}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(effect.displayName), label);

                // BuffController replaces effects sharing the same stat+debuff flag, so a second one
                // on the same stat inside one skill would silently overwrite the first.
                Assert.IsTrue(seenStats.Add(effect.affectedStat), $"{label} has two effects on {effect.affectedStat}.");

                if (effect.ticksOverTime)
                {
                    Assert.Greater(effect.tickIntervalSeconds, 0f, label);
                    Assert.Greater(effect.tickDamageOrHeal, 0f, label);
                }
                else
                {
                    Assert.AreNotEqual(0f, effect.value, $"{label}/{effect.displayName} has no effect.");
                    Assert.LessOrEqual(Mathf.Abs(effect.value), 1f, $"{label}/{effect.displayName} percent out of range.");
                }
            }
        }

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Maria)]
        [TestCase(CharacterId.Joaquim)]
        [TestCase(CharacterId.Icaro)]
        [TestCase(CharacterId.Lavine)]
        public void SkillTree_ContainsTheFiveCharacterSkills(CharacterId id)
        {
            SkillTreeDefinition tree = CharacterSkillSet.GetSkillTree(id);

            Assert.IsNotNull(tree, CharacterSkillSet.GetSkillTreeResourcesPath(id));
            CollectionAssert.AreEquivalent(
                CharacterSkillSet.GetSkills(id).ToArray(),
                tree.Nodes.Select(n => n.skill).ToArray());
            Assert.AreEqual(tree.Nodes.Length, tree.Nodes.Select(n => n.gridPosition).Distinct().Count(), "Grid positions must be unique.");
        }

        [Test]
        public void GrantTo_AtLevelOne_LearnsAndEquipsTheFourActives()
        {
            _go = new GameObject("CharacterSkillSetTestCaster");
            SkillManager skillManager = _go.AddComponent<SkillManager>();
            IReadOnlyList<SkillDefinition> skills = CharacterSkillSet.GetSkills(CharacterId.Maithe);

            int learned = CharacterSkillSet.GrantTo(skillManager, CharacterId.Maithe, casterLevel: 1);

            Assert.AreEqual(4, learned);

            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(skillManager.IsSkillLearned(skills[i].SkillId));
                Assert.AreEqual(skills[i], skillManager.EquippedSlots[i].Definition);
            }

            Assert.IsFalse(skillManager.IsSkillLearned(skills[4].SkillId), "Ultimate must stay locked at level 1.");
            Assert.IsTrue(skillManager.IsSkillRegistered(skills[4].SkillId), "Ultimate must still be resolvable for save/load.");
        }

        [Test]
        public void GrantTo_AtUltimateLevel_LearnsAllFive()
        {
            _go = new GameObject("CharacterSkillSetTestCaster");
            SkillManager skillManager = _go.AddComponent<SkillManager>();
            SkillDefinition ultimate = CharacterSkillSet.GetSkills(CharacterId.Lavine)[4];

            int learned = CharacterSkillSet.GrantTo(skillManager, CharacterId.Lavine, ultimate.Requirement.minimumPlayerLevel);

            Assert.AreEqual(5, learned);
            Assert.IsTrue(skillManager.IsSkillLearned(ultimate.SkillId));
        }

        [Test]
        public void GrantTo_CalledTwice_DoesNotDuplicate()
        {
            _go = new GameObject("CharacterSkillSetTestCaster");
            SkillManager skillManager = _go.AddComponent<SkillManager>();

            CharacterSkillSet.GrantTo(skillManager, CharacterId.Joaquim, casterLevel: 1);
            int secondPass = CharacterSkillSet.GrantTo(skillManager, CharacterId.Joaquim, casterLevel: 1);

            Assert.AreEqual(0, secondPass);
            Assert.AreEqual(4, skillManager.LearnedSkills.Count);
        }

        [Test]
        public void GrantTo_NullManager_ReturnsZero()
        {
            Assert.AreEqual(0, CharacterSkillSet.GrantTo(null, CharacterId.Maria, casterLevel: 1));
        }
    }
}
