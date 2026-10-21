using System.IO;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.CharacterSheet;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// Ficha do personagem: the "+1" preview deltas and derived stats (<see cref="CharacterSheetPresenter"/>,
    /// read against the real <see cref="AttributeSet"/>/<see cref="PlayerStats"/>/<see cref="OutgoingDamage"/>
    /// code), the spending flow through <see cref="AttributeSet.TrySpendPoint"/>, the HUD "Ficha" badge
    /// (<see cref="HudController.ApplyBadge"/>) and the wiring the sheet relies on (view in Resources,
    /// tutorial outline target, HUD button name).
    /// </summary>
    public class CharacterSheetTests
    {
        private GameObject _go;
        private PlayerStats _stats;
        private BuffController _buffs;
        private AttributeSet _attributes;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _go = new GameObject("CharacterSheetTests_Player");
            _stats = _go.AddComponent<PlayerStats>();
            _buffs = _go.AddComponent<BuffController>();
            _attributes = _go.AddComponent<AttributeSet>();
            _attributes.ForceInitializeForTests();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
        }

        private static LocalizationManager English()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            return manager;
        }

        // ---------------------------------------------------------------
        // Preview deltas
        // ---------------------------------------------------------------

        [Test]
        public void PreviewDelta_MatchesAttributeSetConstants_InDisplayUnits()
        {
            Assert.AreEqual(AttributeSet.VitalityHealthPerPoint, CharacterSheetPresenter.GetPreviewDelta(AttributeType.Vitality), 1e-4f);
            Assert.AreEqual(AttributeSet.SpiritManaPerPoint, CharacterSheetPresenter.GetPreviewDelta(AttributeType.Spirit), 1e-4f);
            Assert.AreEqual(AttributeSet.DexterityCriticalChancePerPoint * 100f, CharacterSheetPresenter.GetPreviewDelta(AttributeType.Dexterity), 1e-4f);
            Assert.AreEqual(AttributeSet.IntelligenceCooldownReductionPerPoint * 100f, CharacterSheetPresenter.GetPreviewDelta(AttributeType.Intelligence), 1e-4f);
            Assert.AreEqual(AttributeSet.ArcanePowerAttackDamagePerPoint * 100f, CharacterSheetPresenter.GetPreviewDelta(AttributeType.ArcanePower), 1e-4f);
        }

        [Test]
        public void PreviewText_PtBRFallback_AndEnglish()
        {
            Assert.AreEqual("+10 Vida máx.", CharacterSheetPresenter.FormatPreview(AttributeType.Vitality, null));
            Assert.AreEqual("+8 Mana máx.", CharacterSheetPresenter.FormatPreview(AttributeType.Spirit, null));
            Assert.AreEqual("+0,5% Chance de crítico", CharacterSheetPresenter.FormatPreview(AttributeType.Dexterity, null));
            Assert.AreEqual("+0,4% Redução de recarga", CharacterSheetPresenter.FormatPreview(AttributeType.Intelligence, null));
            Assert.AreEqual("+1% Dano de habilidade", CharacterSheetPresenter.FormatPreview(AttributeType.ArcanePower, null));

            LocalizationManager en = English();
            Assert.AreEqual("+10 Max HP", CharacterSheetPresenter.FormatPreview(AttributeType.Vitality, en));
            Assert.AreEqual("+0.5% Crit chance", CharacterSheetPresenter.FormatPreview(AttributeType.Dexterity, en));
            Assert.AreEqual("Arcane Power", CharacterSheetPresenter.GetAttributeName(AttributeType.ArcanePower, en));
            Assert.AreEqual("Poder Arcano", CharacterSheetPresenter.GetAttributeName(AttributeType.ArcanePower, null));
        }

        [Test]
        public void AttributeOrder_ListsEveryAttributeOnce()
        {
            CollectionAssert.AreEquivalent(System.Enum.GetValues(typeof(AttributeType)), CharacterSheetPresenter.AttributeOrder);
        }

        [TestCase(0.125f, "12,5%")]
        [TestCase(0f, "0%")]
        [TestCase(0.05f, "5%")]
        public void FormatPercent_PtBRUsesDecimalComma(float fraction, string expected)
        {
            Assert.AreEqual(expected, CharacterSheetPresenter.FormatPercent(fraction, null));
        }

        [Test]
        public void FormatMultiplier_PerLanguage()
        {
            Assert.AreEqual("x1,5", CharacterSheetPresenter.FormatMultiplier(1.5f, null));
            Assert.AreEqual("x1.5", CharacterSheetPresenter.FormatMultiplier(1.5f, English()));
            Assert.AreEqual("x2", CharacterSheetPresenter.FormatMultiplier(2f, null));
        }

        [Test]
        public void ExperienceFraction_ClampsAndHandlesZero()
        {
            Assert.AreEqual(0.25f, CharacterSheetPresenter.ExperienceFraction(25, 100), 1e-4f);
            Assert.AreEqual(1f, CharacterSheetPresenter.ExperienceFraction(250, 100), 1e-4f);
            Assert.AreEqual(0f, CharacterSheetPresenter.ExperienceFraction(10, 0), 1e-4f);
        }

        // ---------------------------------------------------------------
        // Derived stats + spending flow
        // ---------------------------------------------------------------

        [Test]
        public void DerivedStats_NoComponents_FallBackToBalanceDefaults()
        {
            CharacterDerivedStats stats = CharacterSheetPresenter.BuildDerivedStats(null, null, null);

            Assert.AreEqual(0f, stats.CriticalChance);
            Assert.AreEqual(PlayerCombatBalance.DefaultCriticalMultiplier, stats.CriticalMultiplier, 1e-4f);
            Assert.AreEqual(0f, stats.CooldownReduction);
            Assert.AreEqual(0f, stats.SkillDamageBonus);
        }

        [Test]
        public void SpendingPoints_UpdatesDerivedStatsByThePreviewedAmount()
        {
            var balance = PlayerCombatBalance.CreateForTests(15f, 0.05f, 1.5f);

            try
            {
                CharacterDerivedStats before = CharacterSheetPresenter.BuildDerivedStats(_stats, _buffs, balance);
                _attributes.GrantAttributePoints(5);

                foreach (AttributeType type in CharacterSheetPresenter.AttributeOrder)
                {
                    Assert.IsTrue(CharacterSheetPresenter.TrySpend(_attributes, type), type.ToString());
                }

                CharacterDerivedStats after = CharacterSheetPresenter.BuildDerivedStats(_stats, _buffs, balance);

                Assert.AreEqual(0, _attributes.AvailableAttributePoints);
                Assert.AreEqual(before.MaxHealth + AttributeSet.VitalityHealthPerPoint, after.MaxHealth, 1e-3f);
                Assert.AreEqual(before.MaxMana + AttributeSet.SpiritManaPerPoint, after.MaxMana, 1e-3f);
                Assert.AreEqual(before.MaxStamina, after.MaxStamina, 1e-3f, "no attribute raises stamina");
                Assert.AreEqual(0.05f + AttributeSet.DexterityCriticalChancePerPoint, after.CriticalChance, 1e-5f);
                Assert.AreEqual(OutgoingDamage.GetCriticalChance(_buffs, balance), after.CriticalChance, 1e-6f, "same number combat rolls against");
                Assert.AreEqual(1.5f, after.CriticalMultiplier, 1e-5f);
                Assert.AreEqual(AttributeSet.IntelligenceCooldownReductionPerPoint, after.CooldownReduction, 1e-5f);
                Assert.AreEqual(AttributeSet.ArcanePowerAttackDamagePerPoint, after.SkillDamageBonus, 1e-5f);
            }
            finally
            {
                Object.DestroyImmediate(balance);
            }
        }

        [Test]
        public void SkillDamageBonus_IgnoresFlatAttackDamageFromGear()
        {
            _attributes.GrantAttributePoints(3);
            _attributes.TrySpendPoint(AttributeType.ArcanePower, 3);
            _buffs.ApplyEffect(new StatModifierDefinition
            {
                affectedStat = StatType.AttackDamage,
                application = ModifierApplication.Flat,
                value = 5f,
                durationSeconds = float.MaxValue / 2f
            }, "equip_test");

            CharacterDerivedStats stats = CharacterSheetPresenter.BuildDerivedStats(_stats, _buffs, null);

            Assert.AreEqual(3 * AttributeSet.ArcanePowerAttackDamagePerPoint, stats.SkillDamageBonus, 1e-5f);
        }

        [Test]
        public void TrySpend_WithoutPoints_FailsAndChangesNothing()
        {
            Assert.IsFalse(CharacterSheetPresenter.CanSpend(_attributes.AvailableAttributePoints));
            Assert.IsFalse(CharacterSheetPresenter.TrySpend(_attributes, AttributeType.Vitality));
            Assert.AreEqual(0, _attributes.GetAllocatedPoints(AttributeType.Vitality));
            Assert.IsFalse(CharacterSheetPresenter.TrySpend(null, AttributeType.Vitality));
        }

        [Test]
        public void TrySpend_OnePointPerPress_RaisesChangeEvent()
        {
            int changes = 0;
            _attributes.GrantAttributePoints(2);
            _attributes.OnAttributesChanged += () => changes++;

            Assert.IsTrue(CharacterSheetPresenter.TrySpend(_attributes, AttributeType.Dexterity));

            Assert.AreEqual(1, _attributes.AvailableAttributePoints);
            Assert.AreEqual(1, _attributes.GetAllocatedPoints(AttributeType.Dexterity));
            Assert.AreEqual(1, changes, "the sheet refreshes itself from OnAttributesChanged");
            Assert.IsTrue(CharacterSheetPresenter.CanSpend(_attributes.AvailableAttributePoints));
        }

        // ---------------------------------------------------------------
        // HUD badge
        // ---------------------------------------------------------------

        [TestCase(0, false, "")]
        [TestCase(-1, false, "")]
        [TestCase(1, true, "1")]
        [TestCase(9, true, "9")]
        [TestCase(12, true, "9+")]
        public void Badge_VisibleOnlyWithUnspentPoints(int points, bool visible, string text)
        {
            var badge = new Label();
            HudController.ApplyBadge(badge, points);

            Assert.AreEqual(visible, CharacterSheetPresenter.ShouldShowBadge(points));
            Assert.AreEqual(!visible, badge.ClassListContains(HudController.BadgeHiddenClass));
            Assert.AreEqual(text, badge.text);
        }

        [Test]
        public void Badge_FollowsAttributeSet_GrantThenSpendAll()
        {
            var badge = new Label();
            _attributes.OnAttributesChanged += () => HudController.ApplyBadge(badge, _attributes.AvailableAttributePoints);
            HudController.ApplyBadge(badge, _attributes.AvailableAttributePoints);
            Assert.IsTrue(badge.ClassListContains(HudController.BadgeHiddenClass));

            _attributes.GrantAttributePoints(2);
            Assert.IsFalse(badge.ClassListContains(HudController.BadgeHiddenClass));
            Assert.AreEqual("2", badge.text);

            _attributes.TrySpendPoint(AttributeType.Spirit, 2);
            Assert.IsTrue(badge.ClassListContains(HudController.BadgeHiddenClass));
        }

        // ---------------------------------------------------------------
        // Wiring
        // ---------------------------------------------------------------

        [Test]
        public void View_IsLoadableFromResources()
        {
            Assert.IsNotNull(Resources.Load<VisualTreeAsset>(CharacterSheetController.ViewResourcePath));
        }

        [Test]
        public void HudView_HasCharacterSheetButtonWithBadge_AndTutorialOutlinesIt()
        {
            string uxml = File.ReadAllText("Assets/UI/HUD/HudView.uxml");
            StringAssert.Contains($"name=\"{HudController.CharacterSheetButtonName}\"", uxml);
            StringAssert.Contains($"name=\"{HudController.CharacterSheetBadgeName}\"", uxml);
            StringAssert.Contains($"view-data-key=\"{CharacterSheetTexts.HudButton}\"", uxml);

            TutorialStepDefinition levelUp = null;
            foreach (TutorialStepDefinition step in TutorialSteps.All)
            {
                if (step.Id == TutorialSteps.LevelUp) levelUp = step;
            }

            Assert.IsNotNull(levelUp);
            Assert.AreEqual(HudController.CharacterSheetButtonName, levelUp.HighlightElement);
        }

        [Test]
        public void HudResolver_CoversSheetKeys_AndDefersOthersToUiTexts()
        {
            Assert.AreEqual("Ficha", CharacterSheetTexts.Get(null, CharacterSheetTexts.HudButton));
            Assert.AreEqual("Sheet", CharacterSheetTexts.Get(English(), CharacterSheetTexts.HudButton));
            Assert.AreEqual(Rapadura.UI.Common.UiTexts.Get(null, Rapadura.UI.Common.UiTexts.HudSkills),
                CharacterSheetTexts.Get(null, Rapadura.UI.Common.UiTexts.HudSkills));
        }
    }
}
