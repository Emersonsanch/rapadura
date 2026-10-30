using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Talents;
using Rapadura.UI.Talents;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// Training by use and the Roda de Talentos (Docs/Balance/README.md §26): proficiency curve and cap, the wheel catalog
    /// shape, spending rules, respec price, save round-trip and sanitising, texts and the wheel layout.
    /// </summary>
    public class TalentTests
    {
        private static IEnumerable<CharacterId> Heroes => (CharacterId[])Enum.GetValues(typeof(CharacterId));

        // ------------------------------------------------------------------ proficiency

        [Test]
        public void Proficiency_EachLevelCostsMoreThanTheLast()
        {
            foreach (ProficiencySpec spec in ProficiencyRules.Specs)
            {
                Assert.AreEqual(spec.BaseTries, ProficiencyRules.TriesToNext(spec.Kind, 0));
                for (int level = 1; level < ProficiencyRules.MaxLevel; level++)
                {
                    Assert.Greater(ProficiencyRules.TriesToNext(spec.Kind, level), ProficiencyRules.TriesToNext(spec.Kind, level - 1));
                }

                Assert.AreEqual(0, ProficiencyRules.TriesToNext(spec.Kind, ProficiencyRules.MaxLevel));
            }
        }

        [Test]
        public void Proficiency_IsSlow_ThousandsOfSwingsToTheCap()
        {
            Assert.Greater(ProficiencyRules.TotalTriesTo(ProficiencyKind.Weapon, ProficiencyRules.MaxLevel), 10000);
            Assert.Greater(ProficiencyRules.TotalTriesTo(ProficiencyKind.Shielding, ProficiencyRules.MaxLevel), 10000);
        }

        [Test]
        public void Proficiency_AddTries_LevelsUpAndStopsAtTheCap()
        {
            var entry = new ProficiencyEntry { kind = ProficiencyKind.Weapon.ToString() };
            Assert.AreEqual(0, ProficiencyRules.AddTries(entry, ProficiencyRules.WeaponBaseTries - 1));
            Assert.AreEqual(1, ProficiencyRules.AddTries(entry, 1));
            Assert.AreEqual(1, entry.level);
            Assert.AreEqual(0, entry.tries);

            ProficiencyRules.AddTries(entry, long.MaxValue / 4);
            Assert.AreEqual(ProficiencyRules.MaxLevel, entry.level);
            Assert.AreEqual(0, entry.tries);
            Assert.AreEqual(0, ProficiencyRules.AddTries(entry, 1000));
            Assert.AreEqual(1f, ProficiencyRules.Progress(entry));
        }

        [Test]
        public void Proficiency_BonusesAreSmall()
        {
            Assert.LessOrEqual(ProficiencyRules.GetSpec(ProficiencyKind.Weapon).MaxValue, 0.02f, "attack speed: inside the §26 budget");
            Assert.LessOrEqual(ProficiencyRules.GetSpec(ProficiencyKind.Magic).MaxValue, 0.05f);
            Assert.LessOrEqual(ProficiencyRules.GetSpec(ProficiencyKind.Shielding).MaxValue, 0.05f);
            foreach (ProficiencySpec spec in ProficiencyRules.Specs)
            {
                Assert.AreNotEqual(StatType.AttackDamage, spec.Stat, "no raw damage from training (NG+1 sits near the 12 s floor)");
            }
        }

        [Test]
        public void Proficiency_SwingOnlyTrainsInAFight()
        {
            Assert.IsFalse(ProficiencyRules.IsInCombat(10, double.NegativeInfinity));
            Assert.IsTrue(ProficiencyRules.IsInCombat(10, 8));
            Assert.IsFalse(ProficiencyRules.IsInCombat(10, 10 - ProficiencyRules.CombatWindowSeconds - 0.1));

            var progress = new TalentProgress();
            Assert.IsFalse(progress.HandleSwing(CharacterId.Joaquim, 5));
            progress.MarkCombat(4);
            Assert.IsTrue(progress.HandleSwing(CharacterId.Joaquim, 5));
            Assert.AreEqual(1, progress.Proficiency(CharacterId.Joaquim, ProficiencyKind.Weapon).tries);
            Assert.AreEqual(0, progress.Proficiency(CharacterId.Maria, ProficiencyKind.Weapon).tries, "per cousin");
        }

        [Test]
        public void Proficiency_MagicTriesAreManaSpent()
        {
            Assert.AreEqual(0, ProficiencyRules.MagicTries(0f));
            Assert.AreEqual(1, ProficiencyRules.MagicTries(0.2f));
            Assert.AreEqual(25, ProficiencyRules.MagicTries(25f));
        }

        [Test]
        public void Proficiency_Modifiers_OnePerTrainedKind()
        {
            var entries = new List<ProficiencyEntry>
            {
                new ProficiencyEntry { kind = "Weapon", level = 10 },
                new ProficiencyEntry { kind = "Magic", level = 0 },
            };
            List<StatModifierDefinition> mods = ProficiencyRules.Modifiers(entries);
            Assert.AreEqual(1, mods.Count);
            Assert.AreEqual(StatType.AttackSpeed, mods[0].affectedStat);
            Assert.AreEqual(10 * ProficiencyRules.WeaponAttackSpeedPerLevel, mods[0].value, 1e-6f);
        }

        // ------------------------------------------------------------------ wheel catalog

        [Test]
        public void Wheel_EveryCousinHasFourQuadrantsWithOneKeystoneEach()
        {
            foreach (CharacterId hero in Heroes)
            {
                List<TalentNodeSpec> nodes = TalentWheelCatalog.NodesOf(hero);
                Assert.AreEqual(16, nodes.Count, hero.ToString());
                foreach (TalentQuadrant quadrant in (TalentQuadrant[])Enum.GetValues(typeof(TalentQuadrant)))
                {
                    List<TalentNodeSpec> inQuadrant = nodes.Where(n => n.Quadrant == quadrant).ToList();
                    Assert.AreEqual(1, inQuadrant.Count(n => n.IsKeystone), $"{hero} {quadrant}");
                    Assert.AreEqual(1, inQuadrant.Count(n => n.Ring == 1));
                    Assert.AreEqual(2, inQuadrant.Count(n => n.Ring == 2));
                    Assert.AreEqual(TalentWheelCatalog.QuadrantCapacity, inQuadrant.Sum(n => n.MaxCost));
                }
            }

            Assert.AreEqual(TalentWheelCatalog.Nodes.Count, TalentWheelCatalog.Nodes.Select(n => n.NodeId).Distinct().Count());
        }

        [Test]
        public void Wheel_IsAChoice_LevelFiftyCannotFillIt()
        {
            int earned = TalentWheelRules.EarnedPoints(50);
            Assert.AreEqual(39, earned);
            Assert.Less(earned, TalentWheelCatalog.QuadrantCapacity * 4);
            Assert.AreEqual(0, TalentWheelRules.EarnedPoints(TalentWheelCatalog.UnlockLevel - 1));
            Assert.AreEqual(1, TalentWheelRules.EarnedPoints(TalentWheelCatalog.UnlockLevel));
        }

        [Test]
        public void Wheel_OffenceStaysSmall()
        {
            foreach (TalentNodeSpec node in TalentWheelCatalog.Nodes)
            {
                if (node.Stat == StatType.AttackDamage || node.Stat == StatType.CriticalChance || node.Stat == StatType.CooldownReduction)
                {
                    Assert.LessOrEqual(node.MaxValue, 0.01f + 1e-6f, node.NodeId);
                }
            }
        }

        // ------------------------------------------------------------------ spending

        [Test]
        public void Spend_FollowsTheRings()
        {
            TalentRecord record = TalentWheelRules.NewRecord(CharacterId.Joaquim);
            const int level = 40;
            Assert.AreEqual(TalentSpendResult.Locked, TalentWheelRules.Spend(record, CharacterId.Joaquim, 5, "mao_certeira"));
            Assert.AreEqual(TalentSpendResult.RequirementMissing, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "golpe_pesado"));
            Assert.AreEqual(TalentSpendResult.Spent, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "mao_certeira"));
            Assert.AreEqual(TalentSpendResult.RequirementMissing, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "golpe_pesado"));
            Assert.AreEqual(TalentSpendResult.Spent, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "mao_certeira"));
            Assert.AreEqual(TalentSpendResult.Spent, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "golpe_pesado"));

            Assert.AreEqual(TalentSpendResult.NotYourNode, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "prece_ardente"));
            Assert.AreEqual(TalentSpendResult.RequirementMissing, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "aboio_de_guerra"), "keystone needs 6 in the quadrant");
            for (int i = 0; i < 3; i++)
            {
                TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "olho_de_gaviao");
            }

            Assert.AreEqual(6, TalentWheelRules.SpentIn(record, TalentQuadrant.Offence));
            Assert.AreEqual(TalentSpendResult.Spent, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "aboio_de_guerra"));
            Assert.AreEqual(9, TalentWheelRules.SpentPoints(record), "keystone costs 3");
            Assert.AreEqual(TalentSpendResult.MaxRank, TalentWheelRules.Spend(record, CharacterId.Joaquim, level, "aboio_de_guerra"));
            Assert.AreEqual(0.01f, TalentWheelRules.Bonus(record, StatType.AttackDamage) - 2 * 0.002f, 1e-6f);
        }

        [Test]
        public void Spend_NeedsPoints()
        {
            TalentRecord record = TalentWheelRules.NewRecord(CharacterId.Maria);
            int level = TalentWheelCatalog.UnlockLevel;
            Assert.AreEqual(TalentSpendResult.Spent, TalentWheelRules.Spend(record, CharacterId.Maria, level, "couro_grosso"));
            Assert.AreEqual(TalentSpendResult.NoPoints, TalentWheelRules.Spend(record, CharacterId.Maria, level, "couro_grosso"));
        }

        [Test]
        public void Respec_CostsGoldAndRefundsEverything()
        {
            TalentRecord record = TalentWheelRules.NewRecord(CharacterId.Lavine);
            Assert.AreEqual(0, TalentWheelRules.RespecCost(record));
            Assert.AreEqual(TalentRespecResult.NothingToReset, TalentWheelRules.CanRespec(record, 9999));
            TalentWheelRules.Spend(record, CharacterId.Lavine, 30, "pe_ligeiro");
            Assert.AreEqual(TalentWheelRules.RespecMinimumGold, TalentWheelRules.RespecCost(record));
            for (int i = 0; i < 2; i++)
            {
                TalentWheelRules.Spend(record, CharacterId.Lavine, 30, "pe_ligeiro");
            }

            for (int i = 0; i < 3; i++)
            {
                TalentWheelRules.Spend(record, CharacterId.Lavine, 30, "folego_largo");
            }

            Assert.AreEqual(6 * TalentWheelRules.RespecGoldPerPoint, TalentWheelRules.RespecCost(record));
            Assert.AreEqual(TalentRespecResult.NotEnoughGold, TalentWheelRules.CanRespec(record, 10));
            Assert.AreEqual(TalentRespecResult.Done, TalentWheelRules.CanRespec(record, 1000));
            Assert.AreEqual(6, TalentWheelRules.Reset(record));
            Assert.AreEqual(0, TalentWheelRules.SpentPoints(record));
            Assert.AreEqual(1, record.respecs);

            var progress = new TalentProgress();
            TalentWheelRules.Spend(progress.Record(CharacterId.Lavine), CharacterId.Lavine, 30, "pe_ligeiro");
            Assert.AreEqual(TalentRespecResult.NotEnoughGold, progress.Respec(CharacterId.Lavine, null, out int refunded));
            Assert.AreEqual(0, refunded);
        }

        // ------------------------------------------------------------------ save

        [Test]
        public void Save_RoundTripsAndSanitises()
        {
            var progress = new TalentProgress();
            progress.Train(CharacterId.Icaro, ProficiencyKind.Magic, ProficiencyRules.MagicBaseTries + 7);
            TalentWheelRules.Spend(progress.Record(CharacterId.Icaro), CharacterId.Icaro, 20, "cura_de_quintal");

            string json = JsonUtility.ToJson(progress.CaptureState());
            var restored = new TalentProgress();
            restored.RestoreState(JsonUtility.FromJson<TalentSaveState>(json));
            Assert.AreEqual(1, restored.Proficiency(CharacterId.Icaro, ProficiencyKind.Magic).level);
            Assert.AreEqual(7, restored.Proficiency(CharacterId.Icaro, ProficiencyKind.Magic).tries);
            Assert.AreEqual(1, TalentWheelRules.RankOf(restored.Record(CharacterId.Icaro), "cura_de_quintal"));

            var broken = new TalentSaveState();
            broken.heroes.Add(new TalentRecord
            {
                characterId = "Maithe",
                proficiencies = new List<ProficiencyEntry> { new ProficiencyEntry { kind = "Weapon", level = 99, tries = 5 }, new ProficiencyEntry { kind = "Nope" } },
                nodes = new List<TalentNodeRank>
                {
                    new TalentNodeRank { nodeId = "pele_de_mandacaru", rank = 2 }, // ring 2 without ring 1
                    new TalentNodeRank { nodeId = "aboio_de_guerra", rank = 1 }, // Joaquim's keystone
                    new TalentNodeRank { nodeId = "pe_ligeiro", rank = 9 },
                    new TalentNodeRank { nodeId = "unknown", rank = 1 },
                },
            });
            restored.RestoreState(broken);
            TalentRecord maithe = restored.Record(CharacterId.Maithe);
            Assert.AreEqual(ProficiencyRules.MaxLevel, restored.Proficiency(CharacterId.Maithe, ProficiencyKind.Weapon).level);
            Assert.AreEqual(0, restored.Proficiency(CharacterId.Maithe, ProficiencyKind.Weapon).tries);
            Assert.AreEqual(ProficiencyRules.Specs.Count, maithe.proficiencies.Count);
            Assert.AreEqual(1, maithe.nodes.Count);
            Assert.AreEqual(TalentWheelCatalog.SmallNodeRanks, TalentWheelRules.RankOf(maithe, "pe_ligeiro"));
            Assert.AreEqual(0, TalentWheelRules.RankOf(restored.Record(CharacterId.Icaro), "cura_de_quintal"), "a restore replaces everything");
        }

        // ------------------------------------------------------------------ texts + layout

        [Test]
        public void Texts_EveryKeyResolves()
        {
            var keys = new List<string>();
            keys.AddRange(TalentWheelCatalog.Nodes.Select(n => n.NameKey));
            keys.AddRange(((TalentQuadrant[])Enum.GetValues(typeof(TalentQuadrant))).Select(TalentTexts.QuadrantKey));
            keys.AddRange(((ProficiencyKind[])Enum.GetValues(typeof(ProficiencyKind))).Select(TalentTexts.ProficiencyKey));
            keys.AddRange(TalentTexts.UsedStats.Select(TalentTexts.StatKey));
            foreach (string key in keys)
            {
                Assert.IsNotNull(TalentTexts.GetFallback(key), key);
            }

            foreach (StatType stat in TalentWheelCatalog.Nodes.Select(n => n.Stat).Concat(ProficiencyRules.Specs.Select(s => s.Stat)))
            {
                CollectionAssert.Contains(TalentTexts.UsedStats, stat);
            }

            Assert.AreEqual("+0,6% dano", TalentTexts.FormatBonus(null, StatType.AttackDamage, 0.006f));
        }

        [Test]
        public void Layout_NodesFitTheWheelAndNeverOverlap()
        {
            foreach (CharacterId hero in Heroes)
            {
                List<Vector2> centers = TalentWheelPresenter.Layout(hero).Select(x => TalentWheelPresenter.NodeCenter(x.Node, x.IndexInRing)).ToList();
                foreach (Vector2 c in centers)
                {
                    Assert.That(c.x - 32f, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(c.y - 32f, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(c.x + 32f, Is.LessThanOrEqualTo(TalentWheelPresenter.WheelSize));
                    Assert.That(c.y + 32f, Is.LessThanOrEqualTo(TalentWheelPresenter.WheelSize));
                }

                for (int i = 0; i < centers.Count; i++)
                {
                    for (int j = i + 1; j < centers.Count; j++)
                    {
                        Assert.Greater(Vector2.Distance(centers[i], centers[j]), 60f, $"{hero} nodes {i}/{j}");
                    }
                }
            }
        }

        [Test]
        public void Presenter_StatesAndTrainingText()
        {
            TalentRecord record = TalentWheelRules.NewRecord(CharacterId.Maithe);
            TalentNodeSpec inner = TalentWheelCatalog.GetNode("mao_certeira");
            TalentNodeSpec outer = TalentWheelCatalog.GetNode("golpe_pesado");
            Assert.AreEqual(TalentNodeState.Available, TalentWheelPresenter.State(record, CharacterId.Maithe, 20, inner));
            Assert.AreEqual(TalentNodeState.Locked, TalentWheelPresenter.State(record, CharacterId.Maithe, 20, outer));
            StringAssert.Contains("Mão Certeira", TalentWheelPresenter.RequirementText(record, CharacterId.Maithe, 20, outer, null));
            TalentWheelRules.Spend(record, CharacterId.Maithe, 20, "mao_certeira");
            Assert.AreEqual(TalentNodeState.Learned, TalentWheelPresenter.State(record, CharacterId.Maithe, 20, inner));
            Assert.AreEqual("1/3", TalentWheelPresenter.RankText(record, inner, null));

            var entry = new ProficiencyEntry { kind = "Shielding", level = 4, tries = 0 };
            StringAssert.StartsWith("Defesa 4 (0%)", TalentWheelPresenter.TrainingText(entry, null));
            StringAssert.Contains("+1% defesa", TalentWheelPresenter.TrainingText(entry, null));
        }
    }
}
