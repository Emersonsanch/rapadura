using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Housing;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The straw training dummy (<see cref="TrainingDummyRules"/>, <see cref="TrainingDummyLoadoutBuilder"/>,
    /// Docs/Housing.md §11.8): the real damage pipeline on a no-armor dummy, crits, hits per session, damage per second,
    /// no XP ever, which attacks the cousin gets to try and the texts (Joaquim brags about his numbers now).
    /// </summary>
    public class TrainingDummyTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created.Where(o => o != null))
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        [Test]
        public void Hit_UsesTheOutgoingPipeline_OnAStrawDummy()
        {
            var loadout = new TrainingLoadout { LevelMultiplier = 2f, FlatAttackDamage = 5f, AttackDamagePercent = 0.2f, CriticalMultiplier = 1.5f };

            // (15 × 2 + 5) × 1.2 = 42.
            Assert.AreEqual(42f, TrainingDummyRules.ResolveOutgoing(15f, loadout, false), 0.001f);
            Assert.AreEqual(42, TrainingDummyRules.ResolveHit(15f, loadout, false));
            // Crit: 42 × 1.5 = 63.
            Assert.AreEqual(63, TrainingDummyRules.ResolveHit(15f, loadout, true));

            // Same numbers as the arena's OutgoingDamage with no buffs: base × level, no crit source.
            Assert.AreEqual(OutgoingDamage.Resolve(15f, 2f, null, null, null, out bool crit), TrainingDummyRules.ResolveOutgoing(15f, new TrainingLoadout { LevelMultiplier = 2f }, false), 0.001f);
            Assert.IsFalse(crit);

            // Never 0 for a real attack; nothing for a move with no damage.
            Assert.AreEqual(1, TrainingDummyRules.ResolveHit(0.2f, new TrainingLoadout(), false));
            Assert.AreEqual(0, TrainingDummyRules.ResolveHit(0f, new TrainingLoadout(), false));

            // Armor would go through DamageCalculator's curve (defense 100 halves it); the straw dummy has none.
            Assert.AreEqual(0f, new TrainingLoadout().DummyDefense);
            Assert.AreEqual(20, TrainingDummyRules.ResolveHit(40f, new TrainingLoadout { DummyDefense = 100f }, false));
        }

        [Test]
        public void Session_CountsHits_AndDamagePerSecond()
        {
            var basic = new TrainingMove("basic_attack", 15f, 0.5f, true);
            TrainingSessionResult result = TrainingDummyRules.Simulate(basic, new TrainingLoadout(), null);

            Assert.AreEqual(10f, TrainingDummyRules.SessionSeconds);
            Assert.AreEqual(20, result.HitCount, "0 s, 0.5 s … 9.5 s");
            Assert.AreEqual(300, result.TotalDamage);
            Assert.AreEqual(15, result.MinHit);
            Assert.AreEqual(15, result.MaxHit);
            Assert.AreEqual(15f, result.AverageHit);
            Assert.AreEqual(30f, result.DamagePerSecond);
            Assert.AreEqual(0, result.CriticalHits, "no crit chance, no crits");

            // A 3 s skill lands at 0, 3, 6 and 9 s.
            TrainingMove skill = TrainingMove.ForSkill("skill_x", 40f, 3f, 0.5f, 0.2f);
            Assert.AreEqual(3f, skill.IntervalSeconds);
            TrainingSessionResult skillResult = TrainingDummyRules.Simulate(skill, new TrainingLoadout(), new System.Random(1));
            Assert.AreEqual(4, skillResult.HitCount);
            Assert.AreEqual(16f, skillResult.DamagePerSecond);

            // A long cast counts more than a short cooldown; a zero interval can't loop forever.
            Assert.AreEqual(1.5f, TrainingMove.ForSkill("slow", 10f, 0.2f, 1f, 0.5f).IntervalSeconds);
            Assert.AreEqual(TrainingDummyRules.MinimumIntervalSeconds, new TrainingMove("zero", 1f, 0f, false).IntervalSeconds);
            Assert.AreEqual(100, TrainingDummyRules.Simulate(new TrainingMove("zero", 1f, 0f, false), new TrainingLoadout(), null).HitCount);
        }

        [Test]
        public void Crits_FollowTheChance_AndUseTheCritHit()
        {
            var move = new TrainingMove("basic_attack", 20f, 0.5f, true);
            TrainingSessionResult always = TrainingDummyRules.Simulate(move, new TrainingLoadout { CriticalChance = 1f, CriticalMultiplier = 2f }, new System.Random(3));
            Assert.AreEqual(always.HitCount, always.CriticalHits);
            Assert.AreEqual(40, always.CriticalHit);
            Assert.AreEqual(20, always.NormalHit);
            Assert.IsTrue(always.Hits.All(h => h.Critical && h.Damage == 40));
            Assert.AreEqual(1f, always.CriticalRate);

            // Seeded: same seed, same session.
            var loadout = new TrainingLoadout { CriticalChance = 0.3f };
            TrainingSessionResult a = TrainingDummyRules.Simulate(move, loadout, new System.Random(42), 600f);
            TrainingSessionResult b = TrainingDummyRules.Simulate(move, loadout, new System.Random(42), 600f);
            Assert.AreEqual(a.TotalDamage, b.TotalDamage);
            Assert.AreEqual(0.3, a.CriticalRate, 0.05, "about the crit chance over a long session");
            Assert.AreEqual(20, a.MinHit);
            Assert.AreEqual(30, a.MaxHit);
        }

        [Test]
        public void Dummy_NeverGivesExperience()
        {
            Assert.AreEqual(0, TrainingDummyRules.ExperienceGranted);
            TrainingSessionResult result = TrainingDummyRules.Simulate(new TrainingMove("basic_attack", 999f, 0.1f, true), new TrainingLoadout(), new System.Random(5));
            Assert.Greater(result.TotalDamage, 0);
            Assert.AreEqual(0, result.ExperienceGranted);
        }

        [Test]
        public void Attributes_GiveCritAndAttackPercent()
        {
            TrainingLoadout loadout = TrainingDummyRules.FromAttributes(1.2f, 10, 5, 0.05f, 1.5f);
            Assert.AreEqual(1.2f, loadout.LevelMultiplier);
            Assert.AreEqual(0.05f + 10 * Rapadura.Gameplay.Player.AttributeSet.DexterityCriticalChancePerPoint, loadout.CriticalChance, 0.0001f);
            Assert.AreEqual(5 * Rapadura.Gameplay.Player.AttributeSet.ArcanePowerAttackDamagePerPoint, loadout.AttackDamagePercent, 0.0001f);
            Assert.AreEqual(1.5f, loadout.CriticalMultiplier);
            Assert.AreEqual(1f, TrainingDummyRules.FromAttributes(1f, 100000, 0, 0f, 0.5f).CriticalChance, "chance capped at 100%");
            Assert.AreEqual(1f, TrainingDummyRules.FromAttributes(1f, 0, 0, 0f, 0.5f).CriticalMultiplier, "multiplier never below 1");
        }

        [Test]
        public void Moves_BasicFirst_ThenUsableDamagingSkills()
        {
            SkillDefinition strike = Skill("dummy_strike", 30f, 2f, 1, 5f);
            SkillDefinition heal = Skill("dummy_heal", 0f, 4f, 1, 0f);
            SkillDefinition ultimate = Skill("dummy_ultimate", 200f, 60f, 20, 0f);
            var skills = new[] { strike, heal, ultimate };

            // New hero (no skill book): open skills at level 1; the heal does no damage, the ultimate is locked.
            List<TrainingMoveOption> fresh = TrainingDummyLoadoutBuilder.BuildMoves(skills, null, 1, 15f, 0.5f);
            Assert.AreEqual(2, fresh.Count);
            Assert.IsTrue(fresh[0].Move.IsBasicAttack);
            Assert.AreEqual(15f, fresh[0].Move.BaseDamage);
            Assert.AreSame(strike, fresh[1].Skill);
            Assert.AreEqual(1, fresh[1].SkillLevel);
            Assert.AreEqual(30f, fresh[1].Move.BaseDamage);

            // With a skill book: only learned skills, at their level (30 + 5 × 2 = 40).
            var book = new SkillManagerSaveState
            {
                skills = new[]
                {
                    new LearnedSkillSaveEntry { skillId = "dummy_strike", level = 3 },
                    new LearnedSkillSaveEntry { skillId = "dummy_ultimate", level = 1 },
                },
            };
            List<TrainingMoveOption> learned = TrainingDummyLoadoutBuilder.BuildMoves(skills, book, 20, 15f, 0.5f);
            Assert.AreEqual(3, learned.Count);
            Assert.AreEqual(3, learned[1].SkillLevel);
            Assert.AreEqual(40f, learned[1].Move.BaseDamage);
            Assert.AreSame(ultimate, learned[2].Skill);
            Assert.AreEqual(60f, learned[2].Move.IntervalSeconds);

            Assert.AreEqual(1, TrainingDummyLoadoutBuilder.BuildMoves(null, null, 1, 15f, 0.5f).Count, "always the basic attack");
        }

        [Test]
        public void HitsLine_MarksCrits_AndTrims()
        {
            var hits = new List<TrainingHit> { new TrainingHit(0f, 18, false), new TrainingHit(0.5f, 27, true), new TrainingHit(1f, 18, false) };
            Assert.AreEqual("18 · 27! · 18", TrainingDummyPanel.FormatHits(hits, 12));
            Assert.AreEqual("18 · 27! …", TrainingDummyPanel.FormatHits(hits, 2));
            Assert.AreEqual("—", TrainingDummyPanel.FormatHits(null, 12));
            Assert.AreEqual("30", TrainingDummyPanel.Number(30f));
            Assert.AreEqual("16.5", TrainingDummyPanel.Number(16.49f));
        }

        [Test]
        public void Texts_AreLocalized_AndTheJokeIsGone()
        {
            Dictionary<string, LocalizationEntry> byKey = HouseLifeTexts.Entries.ToDictionary(e => e.Key);
            string[] keys =
            {
                HouseLifeTexts.Dummy, HouseLifeTexts.DummyTitle, HouseLifeTexts.DummyHero, HouseLifeTexts.DummyNoHero, HouseLifeTexts.DummyPick,
                HouseLifeTexts.DummyBasicAttack, HouseLifeTexts.DummySkill, HouseLifeTexts.DummyNoSkills, HouseLifeTexts.DummyResultHeader,
                HouseLifeTexts.DummyPerHit, HouseLifeTexts.DummyCrits, HouseLifeTexts.DummyDps, HouseLifeTexts.DummyLastHits,
                HouseLifeTexts.DummyNoXp, HouseLifeTexts.DummySourceNote, HouseLifeTexts.DummyReactionKey(null),
                HouseLifeTexts.DummyReactionKey(CharacterId.Maithe), HouseLifeTexts.DummyReactionKey(CharacterId.Maria),
                HouseLifeTexts.DummyReactionKey(CharacterId.Joaquim), HouseLifeTexts.DummyReactionKey(CharacterId.Icaro),
                HouseLifeTexts.DummyReactionKey(CharacterId.Lavine),
            };

            foreach (string key in keys)
            {
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(byKey[key].En), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(byKey[key].PtBR), key);
            }

            Assert.IsFalse(byKey[HouseLifeTexts.Dummy].PtBR.Contains("enfeite"), "no longer just decoration");
            StringAssert.Contains("XP", byKey[HouseLifeTexts.Dummy].PtBR);
            StringAssert.Contains("{0}", byKey[HouseLifeTexts.DummyReactionKey(CharacterId.Joaquim)].PtBR, "Joaquim brags about his numbers");
            Assert.IsFalse(HouseLifeTexts.Entries.Any(e => e.PtBR.Contains("Amanhã eu treino naquele boneco")), "Joaquim trains now");
        }

        // ------------------------------------------------------------------

        private SkillDefinition Skill(string id, float damage, float cooldown, int minLevel, float perLevel)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var serialized = new SerializedObject(skill);
            serialized.FindProperty("_skillId").stringValue = id;
            serialized.FindProperty("_baseDamage").floatValue = damage;
            serialized.FindProperty("_damagePerLevel").floatValue = perLevel;
            serialized.FindProperty("_cooldownSeconds").floatValue = cooldown;
            serialized.FindProperty("_castTimeSeconds").floatValue = 0f;
            serialized.FindProperty("_recoverySeconds").floatValue = 0.2f;
            serialized.FindProperty("_requirement.minimumPlayerLevel").intValue = minLevel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(skill);
            return skill;
        }
    }
}
