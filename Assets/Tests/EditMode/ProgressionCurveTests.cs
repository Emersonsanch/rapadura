using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="ProgressionCurve"/> (the balance source of truth that replaced the
    /// formula hardcoded in <see cref="PlayerStats"/>) and its spreadsheet round-trip through
    /// <see cref="ProgressionCsv"/>.
    /// </summary>
    public class ProgressionCurveTests
    {
        private const int Levels = 50;

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        /// <summary>The formula that used to live in <c>PlayerStats.GetExperienceToNextLevel</c>, verbatim.</summary>
        private static int OldExperienceFormula(int level)
        {
            return Mathf.RoundToInt(100 * Mathf.Pow(1.25f, level - 1));
        }

        private ProgressionCurve NewCurve()
        {
            var curve = ScriptableObject.CreateInstance<ProgressionCurve>();
            _created.Add(curve);
            return curve;
        }

        private static void AssertMatchesOldBehaviour(ProgressionCurve curve)
        {
            for (int level = 1; level <= Levels; level++)
            {
                Assert.AreEqual(OldExperienceFormula(level), curve.GetExperienceToNextLevel(level), $"XP to next at level {level}");
                Assert.AreEqual(0f, curve.GetMaxHealthBonus(level), $"Max health bonus at level {level}");
                Assert.AreEqual(0f, curve.GetMaxManaBonus(level), $"Max mana bonus at level {level}");
                Assert.AreEqual(0f, curve.GetMaxStaminaBonus(level), $"Max stamina bonus at level {level}");
                Assert.AreEqual(1f, curve.GetDamageMultiplier(level), $"Damage multiplier at level {level}");

                if (level > 1)
                {
                    Assert.AreEqual(2, curve.GetAttributePointsGained(level - 1, level), $"Attribute points reaching level {level}");
                    Assert.AreEqual(1, curve.GetSkillPointsGained(level - 1, level), $"Skill points reaching level {level}");
                }
            }
        }

        // ---------------------------------------------------------------
        // Defaults == old hardcoded numbers
        // ---------------------------------------------------------------

        [Test]
        public void DefaultCurve_FormulaOnly_MatchesOldHardcodedFormula()
        {
            ProgressionCurve curve = NewCurve();

            Assert.AreEqual(0, curve.TableMaxLevel, "A fresh curve has no explicit table and falls back to the formula.");
            AssertMatchesOldBehaviour(curve);
        }

        [Test]
        public void RegeneratedTable_MatchesOldHardcodedFormula()
        {
            ProgressionCurve curve = NewCurve();
            curve.RegenerateFromFormula(Levels);

            Assert.AreEqual(Levels, curve.TableMaxLevel);
            AssertMatchesOldBehaviour(curve);
        }

        [Test]
        public void ShippedResourcesCurve_MatchesOldHardcodedFormula()
        {
            ProgressionCurve curve = ProgressionCurve.LoadDefault();

            Assert.IsNotNull(curve, $"Expected Resources/{ProgressionCurve.ResourcesPath}.asset to exist.");
            Assert.AreEqual(Levels, curve.TableMaxLevel);
            AssertMatchesOldBehaviour(curve);
        }

        [Test]
        public void PastTheTable_FallsBackToFormula()
        {
            ProgressionCurve curve = NewCurve();
            curve.RegenerateFromFormula(10);

            Assert.AreEqual(OldExperienceFormula(11), curve.GetExperienceToNextLevel(11));
            Assert.AreEqual(OldExperienceFormula(20), curve.GetExperienceToNextLevel(20));
        }

        [Test]
        public void PlayerStats_LevelUps_UseSameNumbersAsOldFormula()
        {
            var go = new GameObject("ProgressionCurveTests_Player");
            _created.Add(go);
            var stats = go.AddComponent<PlayerStats>();

            for (int level = 1; level <= 15; level++)
            {
                Assert.AreEqual(level, stats.Level);
                Assert.AreEqual(OldExperienceFormula(level), stats.GetExperienceToNextLevel(), $"XP to next at level {level}");
                Assert.AreEqual(100f, stats.MaxHealth, "Default curve has no health growth.");
                stats.AddExperience(stats.GetExperienceToNextLevel());
            }
        }

        // ---------------------------------------------------------------
        // CSV round-trip
        // ---------------------------------------------------------------

        [Test]
        public void Csv_ExportImportExport_IsIdentical()
        {
            ProgressionCurve source = NewCurve();
            source.RegenerateFromFormula(Levels);
            string first = ProgressionCsv.Export(source);

            ProgressionCurve copy = NewCurve();
            copy.SetRows(ProgressionCsv.Import(first));
            string second = ProgressionCsv.Export(copy);

            Assert.AreEqual(first, second);
            Assert.AreEqual(Levels, copy.TableMaxLevel);
            AssertMatchesOldBehaviour(copy);
        }

        [Test]
        public void Csv_RoundTrip_PreservesDesignerEditedValues()
        {
            var rows = new List<ProgressionLevelRow>();
            for (int level = 1; level <= 5; level++)
            {
                rows.Add(new ProgressionLevelRow
                {
                    level = level,
                    experienceToNext = 90 + level * 37,
                    maxHealth = 100f + 12.5f * (level - 1),
                    maxMana = 100f + 0.1f * level,
                    maxStamina = 95.25f,
                    damageMultiplier = 1f + 0.035f * (level - 1),
                    attributePoints = level == 1 ? 0 : 3,
                    skillPoints = level % 2
                });
            }

            ProgressionCurve curve = NewCurve();
            curve.SetRows(ProgressionCsv.Import(ProgressionCsv.Export(rows)));

            for (int i = 0; i < rows.Count; i++)
            {
                ProgressionLevelRow expected = rows[i];
                ProgressionLevelRow actual = curve.Rows[i];
                Assert.AreEqual(expected.level, actual.level);
                Assert.AreEqual(expected.experienceToNext, actual.experienceToNext);
                Assert.AreEqual(expected.maxHealth, actual.maxHealth);
                Assert.AreEqual(expected.maxMana, actual.maxMana);
                Assert.AreEqual(expected.maxStamina, actual.maxStamina);
                Assert.AreEqual(expected.damageMultiplier, actual.damageMultiplier);
                Assert.AreEqual(expected.attributePoints, actual.attributePoints);
                Assert.AreEqual(expected.skillPoints, actual.skillPoints);
            }

            Assert.AreEqual(12.5f * 2, curve.GetMaxHealthBonus(3), 0.0001f);
        }

        [Test]
        public void CsvImport_ToleratesReorderedColumnsCommentsCrlfAndDecimalIntegers()
        {
            string csv = "# designer note\r\n" +
                         "xp_to_next,level,hp,attribute_points\r\n" +
                         "\r\n" +
                         "100,1,100,0\r\n" +
                         "130.0,2,110,2\r\n";

            List<ProgressionLevelRow> rows = ProgressionCsv.Import(csv);

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(2, rows[1].level);
            Assert.AreEqual(130, rows[1].experienceToNext);
            Assert.AreEqual(110f, rows[1].maxHealth);
            Assert.AreEqual(100f, rows[1].maxMana, "Missing columns fall back to defaults.");
            Assert.AreEqual(1f, rows[1].damageMultiplier);
        }

        [Test]
        public void CsvImport_RejectsMalformedValues()
        {
            Assert.Throws<FormatException>(() => ProgressionCsv.Import("level,xp_to_next\n1,abc\n"));
            Assert.Throws<FormatException>(() => ProgressionCsv.Import("level,hp\n1,100\n"), "xp_to_next is required.");
        }

        [Test]
        public void SetRows_RejectsGapsAndNonPositiveXp()
        {
            ProgressionCurve curve = NewCurve();

            Assert.Throws<ArgumentException>(() => curve.SetRows(ProgressionCsv.Import("level,xp_to_next\n1,100\n3,150\n")));
            Assert.Throws<ArgumentException>(() => curve.SetRows(ProgressionCsv.Import("level,xp_to_next\n1,0\n")));
            Assert.AreEqual(0, curve.TableMaxLevel, "A rejected import must not partially apply.");
        }

        [Test]
        public void DocsSpreadsheet_MatchesShippedCurve()
        {
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Docs", "Balance", "progression.csv");
            if (!File.Exists(path))
            {
                Assert.Ignore("Docs/Balance/progression.csv not present in this checkout.");
            }

            ProgressionCurve fromCsv = NewCurve();
            fromCsv.SetRows(ProgressionCsv.Import(File.ReadAllText(path)));
            ProgressionCurve shipped = ProgressionCurve.LoadDefault();

            Assert.IsNotNull(shipped);
            Assert.AreEqual(ProgressionCsv.Export(shipped), ProgressionCsv.Export(fromCsv),
                "progression.csv and Resources/Balance/ProgressionCurve.asset drifted apart — run Rapadura > Balance > Import (or Export) Progression CSV.");
        }
    }
}
