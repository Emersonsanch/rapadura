using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Duo;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Duo;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Golpe em dupla": catalog (one combo per pair, order-independent lookup, sibling combos on top), charge /
    /// cooldown / once-per-encounter rules, damage scaled like skills, boss stun/knockback reduction, accessibility
    /// presentation, partner choice + save, texts, input action and the HUD view.
    /// </summary>
    public class DuoStrikeTests
    {
        private const int TestSlot = 90177;
        private static readonly CharacterId[] Cousins = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private readonly List<Object> _created = new List<Object>();
        private GameObject _gameManagerObject;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gm != null)
            {
                var managers = (List<IManager>)typeof(GameManager)
                    .GetField("_managers", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_gm);
                foreach (IManager manager in managers)
                {
                    manager.Shutdown();
                }

                _gm = null;
            }

            if (_gameManagerObject != null)
            {
                Object.DestroyImmediate(_gameManagerObject);
            }

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            ServiceLocator.Clear();
            TestSaveRoot.Exit();
        }

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        // ------------------------------------------------------------------
        // Catalog
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_HasExactlyOneComboPerPairOfCousins()
        {
            Assert.AreEqual(10, DuoComboCatalog.All.Count);
            var pairs = new HashSet<string>(DuoComboCatalog.All.Select(c => DuoStrikeRules.PairKey(c.Elder, c.Younger)));
            Assert.AreEqual(10, pairs.Count, "two combos share a pair");
            Assert.AreEqual(10, DuoComboCatalog.All.Select(c => c.Id).Distinct().Count(), "duplicate ids");

            foreach (CharacterId a in Cousins)
            {
                foreach (CharacterId b in Cousins)
                {
                    if (a != b)
                    {
                        Assert.IsNotNull(DuoComboCatalog.Find(a, b), $"{a}+{b} has no combo");
                    }
                }
            }
        }

        [Test]
        public void Find_IsOrderIndependent_AndNullForTheSameCousin()
        {
            foreach (CharacterId a in Cousins)
            {
                Assert.IsNull(DuoComboCatalog.Find(a, a));
                foreach (CharacterId b in Cousins.Where(b => b != a))
                {
                    Assert.AreSame(DuoComboCatalog.Find(a, b), DuoComboCatalog.Find(b, a));
                    DuoComboSpec combo = DuoComboCatalog.Find(a, b);
                    Assert.IsTrue(combo.Involves(a) && combo.Involves(b));
                    Assert.AreEqual(b, combo.PartnerOf(a));
                }
            }

            Assert.AreEqual(DuoStrikeRules.PairKey(CharacterId.Icaro, CharacterId.Maithe), DuoStrikeRules.PairKey(CharacterId.Maithe, CharacterId.Icaro));
        }

        [Test]
        public void Pairs_AreStoredEldestFirst_ByBirthOrder()
        {
            CollectionAssert.AreEqual(new[] { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine },
                DuoStrikeRules.ByBirthOrder);
            foreach (DuoComboSpec combo in DuoComboCatalog.All)
            {
                Assert.Less(DuoStrikeRules.BirthOrder(combo.Elder), DuoStrikeRules.BirthOrder(combo.Younger), combo.Id);
                Assert.AreEqual(combo.Elder, combo.Speaker(0));
                Assert.AreEqual(combo.Younger, combo.Speaker(1));
            }
        }

        [Test]
        public void SiblingBond_IsOnlyTheTwoSiblingPairs_AndTheyAreTheStrongestCombos()
        {
            List<DuoComboSpec> siblings = DuoComboCatalog.All.Where(c => c.IsSiblingBond).ToList();
            Assert.AreEqual(2, siblings.Count);
            Assert.IsTrue(DuoComboCatalog.Find(CharacterId.Maithe, CharacterId.Icaro).IsSiblingBond);
            Assert.IsTrue(DuoComboCatalog.Find(CharacterId.Joaquim, CharacterId.Maria).IsSiblingBond);
            Assert.IsFalse(DuoComboCatalog.Find(CharacterId.Lavine, CharacterId.Maria).IsSiblingBond, "Lavine is cousin to all");

            float bestCousin = DuoComboCatalog.All.Where(c => !c.IsSiblingBond).Max(c => c.PowerScore);
            foreach (DuoComboSpec sibling in siblings)
            {
                Assert.Greater(sibling.PowerScore, bestCousin * 1.15f, $"{sibling.Id} should clearly out-do every cousin combo");
            }
        }

        [Test]
        public void EveryCombo_DoesSomething_BuiltFromExistingSystems()
        {
            var effects = new HashSet<string>();
            foreach (DuoComboSpec combo in DuoComboCatalog.All)
            {
                Assert.Greater(combo.Radius, 0f, combo.Id);
                Assert.Greater(combo.PowerScore, 0.5f, combo.Id);
                if (combo.DamageMultiplier > 0f) effects.Add("damage");
                if (combo.StunSeconds > 0f) effects.Add("stun");
                if (combo.HealFraction > 0f) effects.Add("heal");
                if (combo.KnockbackForce > 0f) effects.Add("knockback");
                if (combo.HeroBuffs.Count > 0) effects.Add("buff");
                if (combo.EnemyDebuffs.Count > 0) effects.Add("debuff");
                Assert.LessOrEqual(combo.StunSeconds, 1.5f, $"{combo.Id}: a boss would still take {combo.StunSeconds * 0.5f} s");
                Assert.LessOrEqual(combo.HealFraction, 0.3f, combo.Id);
            }

            CollectionAssert.IsSupersetOf(effects, new[] { "damage", "stun", "heal", "knockback", "buff", "debuff" });
        }

        // ------------------------------------------------------------------
        // Charge / meter
        // ------------------------------------------------------------------

        [Test]
        public void Charge_HalfABossOrTwoCommons_FillsTheMeter_WithAPerHitCap()
        {
            Assert.AreEqual(0.1f, DuoStrikeRules.ChargeFromDealt(20f, 100f, isBoss: false), 1e-4f);
            Assert.AreEqual(0.2f, DuoStrikeRules.ChargeFromDealt(52f, 520f, isBoss: true), 1e-4f);
            Assert.AreEqual(DuoStrikeRules.MaxChargePerHit, DuoStrikeRules.ChargeFromDealt(500f, 100f, false), "one huge hit is capped");
            Assert.AreEqual(0f, DuoStrikeRules.ChargeFromDealt(-5f, 100f, false));
            Assert.AreEqual(0f, DuoStrikeRules.ChargeFromDealt(5f, 0f, false));

            var meter = new DuoStrikeMeter();
            for (int i = 0; i < 10; i++)
            {
                meter.AddDealt(26f, 520f, isBoss: true); // 10 hits of 26 = 50% of the Lobo Alfa
            }

            Assert.IsTrue(meter.IsFull);

            meter = new DuoStrikeMeter();
            for (int i = 0; i < 4; i++)
            {
                meter.AddDealt(50f, 100f, isBoss: false); // two 100-HP commons, two hits each
            }

            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void Charge_TakingDamage_AlsoFills_LosingEightyPercentIsEnough()
        {
            Assert.AreEqual(0.125f, DuoStrikeRules.ChargeFromTaken(10f, 100f), 1e-4f);
            var meter = new DuoStrikeMeter();
            for (int i = 0; i < 8; i++)
            {
                meter.AddTaken(10f, 100f);
            }

            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void Meter_ArmsAfterTheIntro_FiresOnce_ThenIsSpentForTheEncounter()
        {
            var meter = new DuoStrikeMeter();
            Assert.AreEqual(DuoStrikeReadiness.Arming, meter.Readiness);
            meter.AddTaken(80f, 100f);
            meter.AddTaken(80f, 100f);
            meter.AddTaken(80f, 100f);
            meter.AddTaken(80f, 100f);
            Assert.IsTrue(meter.IsFull);
            Assert.IsFalse(meter.TryBegin(), "still arming");

            meter.Tick(DuoStrikeRules.ArmDelaySeconds + 0.01f);
            Assert.AreEqual(DuoStrikeReadiness.Ready, meter.Readiness);
            Assert.IsTrue(meter.TryBegin());
            Assert.AreEqual(DuoStrikeReadiness.Executing, meter.Readiness);
            Assert.AreEqual(0f, meter.Charge);

            meter.AddDealt(100f, 100f, false);
            Assert.AreEqual(0f, meter.Charge, "the strike's own damage never refills it");

            meter.EndExecution();
            Assert.AreEqual(DuoStrikeReadiness.Spent, meter.Readiness);
            meter.AddTaken(100f, 100f);
            Assert.AreEqual(0f, meter.Charge, "no charge once spent");
            meter.Tick(DuoStrikeRules.CooldownSeconds * 2f);
            Assert.IsFalse(meter.TryBegin());

            meter.BeginEncounter();
            Assert.AreEqual(0, meter.UsesThisEncounter);
            Assert.AreEqual(DuoStrikeReadiness.Arming, meter.Readiness);
        }

        [Test]
        public void Meter_WithMoreUses_WaitsTheCooldownBetweenThem()
        {
            Assert.AreEqual(1, DuoStrikeRules.MaxUsesPerEncounter);
            var meter = new DuoStrikeMeter(maxUses: 2);
            meter.ForceReadyForTests();
            Assert.IsTrue(meter.TryBegin());
            meter.EndExecution();
            meter.ForceReadyForTests();
            Assert.AreEqual(DuoStrikeReadiness.Cooldown, meter.Readiness);
            meter.Tick(DuoStrikeRules.CooldownSeconds - 1f);
            Assert.IsFalse(meter.TryBegin());
            meter.Tick(1.01f);
            Assert.IsTrue(meter.TryBegin());
        }

        [Test]
        public void Controller_ChargesFromDamageDealtAndTaken_ButNotFromOthers()
        {
            GameObject player = NewObject("DuoPlayer");
            PlayerStats stats = player.AddComponent<PlayerStats>();
            player.AddComponent<BuffController>();
            Health enemy = NewObject("DuoEnemy").AddComponent<Health>();
            enemy.ConfigureMaxHealth(100f);
            GameObject stranger = NewObject("Stranger");

            DuoStrikeController controller = NewObject("DuoController").AddComponent<DuoStrikeController>();
            controller.BindPlayerForTests(player);

            controller.OnDamageAppliedForTests(new DamageAppliedEvent(enemy.gameObject, player, 20f, ElementType.Physical, false));
            Assert.AreEqual(0.1f, controller.Meter.Charge, 1e-4f);

            controller.OnDamageAppliedForTests(new DamageAppliedEvent(enemy.gameObject, stranger, 20f, ElementType.Physical, false));
            Assert.AreEqual(0.1f, controller.Meter.Charge, 1e-4f, "someone else's hit doesn't count");

            float taken = stats.MaxHealth * 0.1f;
            controller.OnDamageAppliedForTests(new DamageAppliedEvent(player, enemy.gameObject, taken, ElementType.Physical, false));
            Assert.AreEqual(0.1f + 0.1f * DuoStrikeRules.TakenFactor, controller.Meter.Charge, 1e-4f);
        }

        // ------------------------------------------------------------------
        // Power
        // ------------------------------------------------------------------

        [Test]
        public void Damage_UsesTheSkillPipeline_LevelMultiplierAndAttackDamageBuffs()
        {
            DuoComboSpec combo = DuoComboCatalog.FindById(DuoComboCatalog.SkyThunderId);
            float expected = DuoStrikeRules.BaseDamage * combo.DamageMultiplier * 1.2f;
            Assert.AreEqual(expected, DuoStrikeRules.ResolveDamage(combo, 1.2f, null, null, () => 0.99f, out bool crit), 1e-3f);
            Assert.IsFalse(crit);

            BuffController buffs = NewObject("DuoBuffs").AddComponent<BuffController>();
            buffs.ApplyEffect(new StatModifierDefinition { affectedStat = StatType.AttackDamage, application = ModifierApplication.PercentAdditive, value = 0.5f, durationSeconds = 0f }, "test");
            Assert.AreEqual(expected * 1.5f, DuoStrikeRules.ResolveDamage(combo, 1.2f, buffs, null, () => 0.99f, out _), 1e-3f);

            Assert.AreEqual(0.3f * 200f, DuoStrikeRules.HealingFor(DuoComboCatalog.FindById(DuoComboCatalog.HolyBastionId), 200f), 1e-3f);
        }

        [Test]
        public void Damage_SitsBetweenTheActiveSkillsAndTheUltimates()
        {
            SkillDefinition[] heroSkills = Resources.LoadAll<SkillDefinition>("Skills/Characters");
            Assume.That(heroSkills.Length, Is.GreaterThanOrEqualTo(25));
            float strongestActive = heroSkills.Where(s => s.SkillType == SkillType.Active).Max(s => s.BaseDamage);
            float weakestDamagingUltimate = heroSkills.Where(s => s.SkillType == SkillType.Ultimate && s.BaseDamage > 0f).Min(s => s.BaseDamage);

            foreach (DuoComboSpec combo in DuoComboCatalog.All)
            {
                float duo = DuoStrikeRules.BaseDamageFor(combo);
                Assert.Less(duo, weakestDamagingUltimate, $"{combo.Id} out-hits an Ultimate");
                Assert.Greater(duo, strongestActive, $"{combo.Id} is weaker than an active skill");
            }
        }

        [Test]
        public void Bosses_TakeHalfTheStun_AndAThirdOfTheKnockback()
        {
            foreach (DuoComboSpec combo in DuoComboCatalog.All.Where(c => c.StunSeconds > 0f))
            {
                Assert.AreEqual(combo.StunSeconds * StunRules.BossStunDurationMultiplier, DuoStrikeRules.StunFor(combo, isBoss: true), 1e-4f);
                Assert.AreEqual(combo.StunSeconds, DuoStrikeRules.StunFor(combo, isBoss: false), 1e-4f);
            }

            Assert.AreEqual(0.5f, StunRules.BossStunDurationMultiplier);
            Assert.AreEqual(6f * DuoStrikeRules.BossKnockbackMultiplier, DuoStrikeRules.KnockbackFor(6f, true), 1e-4f);
            Assert.AreEqual(6f, DuoStrikeRules.KnockbackFor(6f, false), 1e-4f);
            Assert.AreEqual(0f, DuoStrikeRules.KnockbackFor(0f, false));
        }

        [Test]
        public void Impact_HealsAndBuffsTheHero()
        {
            GameObject player = NewObject("DuoPlayer");
            PlayerStats stats = player.AddComponent<PlayerStats>();
            BuffController buffs = player.AddComponent<BuffController>();
            stats.ApplyDamage(stats.MaxHealth * 0.5f);
            float before = stats.CurrentHealth;

            DuoStrikeController controller = NewObject("DuoController").AddComponent<DuoStrikeController>();
            controller.BindPlayerForTests(player);
            DuoComboSpec bastion = DuoComboCatalog.FindById(DuoComboCatalog.HolyBastionId);
            controller.ApplyImpactForTests(bastion, new Vector3(500f, 0f, 500f));

            Assert.Greater(stats.CurrentHealth, before);
            Assert.IsTrue(buffs.HasEffectsFromSource(bastion.SourceKey));
            Assert.Greater(buffs.GetPercentTotal(StatType.Defense), 0f);
        }

        // ------------------------------------------------------------------
        // Presentation
        // ------------------------------------------------------------------

        [Test]
        public void Presentation_RespectsReduceFlashesAndReduceMotion()
        {
            DuoStrikePresentation normal = DuoStrikePresentation.Resolve(false, false, false);
            Assert.IsTrue(normal.AdditiveSilhouette && normal.Lunge && normal.Pulse && normal.BannerFade);
            Assert.Greater(normal.ShakeMagnitude, 0f);
            Assert.Greater(normal.HitStopDuration, 0f);
            Assert.IsFalse(normal.SecondRing);

            DuoStrikePresentation flashes = DuoStrikePresentation.Resolve(true, reduceFlashes: true, reduceMotion: false);
            Assert.IsFalse(flashes.AdditiveSilhouette);
            Assert.IsFalse(flashes.Pulse);
            Assert.Less(flashes.SilhouetteAlpha, normal.SilhouetteAlpha);
            Assert.AreEqual(0f, flashes.HitStopDuration);
            Assert.IsFalse(flashes.BannerFade);

            DuoStrikePresentation motion = DuoStrikePresentation.Resolve(true, reduceFlashes: false, reduceMotion: true);
            Assert.IsFalse(motion.Lunge);
            Assert.AreEqual(0f, motion.ShakeMagnitude);
            Assert.AreEqual(0f, motion.HitStopDuration);
            Assert.IsFalse(motion.BannerFade);

            DuoStrikePresentation sibling = DuoStrikePresentation.Resolve(true, false, false);
            Assert.IsTrue(sibling.SecondRing);
            Assert.Greater(sibling.ShakeMagnitude, normal.ShakeMagnitude);
        }

        [Test]
        public void Silhouette_FadesInAndOut_AndLungePeaksAtTheImpact()
        {
            Assert.AreEqual(0f, DuoPartnerSilhouette.Envelope(0f, false));
            Assert.AreEqual(1f, DuoPartnerSilhouette.Envelope(0.5f, false));
            Assert.AreEqual(0f, DuoPartnerSilhouette.Envelope(1f, false), 1e-4f);
            Assert.AreEqual(1f, DuoPartnerSilhouette.Envelope(0.01f, true), "reduced motion: no fade-in");
            Assert.AreEqual(1f, DuoPartnerSilhouette.LungeCurve(0.3f, 0.3f), 1e-4f);
            Assert.Less(DuoPartnerSilhouette.LungeCurve(0.15f, 0.3f), 1f);
            Assert.Less(DuoPartnerSilhouette.LungeCurve(1f, 0.3f), 1f);

            Color gold = DuoStrikePresentation.SilhouetteColor(Color.blue, siblingBond: true);
            Assert.Greater(gold.r, gold.b, "siblings glow gold");
        }

        // ------------------------------------------------------------------
        // Partner choice + save
        // ------------------------------------------------------------------

        [Test]
        public void Partner_DefaultsToTheSibling_AndLavineCallsMaria()
        {
            var state = new DuoPartnerState();
            Assert.AreEqual(CharacterId.Icaro, state.GetPartnerFor(CharacterId.Maithe));
            Assert.AreEqual(CharacterId.Maithe, state.GetPartnerFor(CharacterId.Icaro));
            Assert.AreEqual(CharacterId.Maria, state.GetPartnerFor(CharacterId.Joaquim));
            Assert.AreEqual(CharacterId.Joaquim, state.GetPartnerFor(CharacterId.Maria));
            Assert.AreEqual(CharacterId.Maria, state.GetPartnerFor(CharacterId.Lavine));
        }

        [Test]
        public void Partner_CanBeChosenAmongUnlockedCousins_NeverTheHeroThemselves()
        {
            var state = new DuoPartnerState();
            Assert.IsFalse(state.SetPartner(CharacterId.Maithe, CharacterId.Maithe));
            Assert.IsTrue(state.SetPartner(CharacterId.Maithe, CharacterId.Lavine));
            Assert.AreEqual(CharacterId.Lavine, state.GetPartnerFor(CharacterId.Maithe));
            Assert.AreEqual(CharacterId.Maria, state.GetPartnerFor(CharacterId.Lavine), "each hero keeps their own pick");

            // Cycling walks the others in birth order and wraps, skipping the hero.
            var seen = new List<CharacterId>();
            for (int i = 0; i < 4; i++)
            {
                seen.Add(state.CyclePartner(CharacterId.Maithe).Value);
            }

            CollectionAssert.AreEqual(new[] { CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine }, seen);

            state.UnlockedFilter = id => id != CharacterId.Icaro;
            Assert.IsFalse(state.SetPartner(CharacterId.Joaquim, CharacterId.Icaro), "locked cousin");
            CollectionAssert.DoesNotContain(state.AvailablePartnersFor(CharacterId.Maithe), CharacterId.Icaro);
            var locked = new DuoPartnerState { UnlockedFilter = id => id != CharacterId.Icaro };
            Assert.AreEqual(CharacterId.Maria, locked.GetPartnerFor(CharacterId.Maithe), "a locked default falls back to the eldest available cousin");
        }

        [Test]
        public void PartnerState_RoundTripsThroughJson_AndIgnoresBadEntries()
        {
            var state = new DuoPartnerState();
            state.SetPartner(CharacterId.Maithe, CharacterId.Joaquim);
            state.SetPartner(CharacterId.Lavine, CharacterId.Icaro);
            string json = JsonUtility.ToJson(state.CaptureState());

            var loaded = new DuoPartnerState();
            loaded.RestoreState(JsonUtility.FromJson<DuoPartnerSaveState>(json));
            Assert.AreEqual(CharacterId.Joaquim, loaded.GetPartnerFor(CharacterId.Maithe));
            Assert.AreEqual(CharacterId.Icaro, loaded.GetPartnerFor(CharacterId.Lavine));

            var bad = new DuoPartnerSaveState();
            bad.partners.Add(new DuoPartnerSaveEntry { hero = "Maria", partner = "Maria" });
            bad.partners.Add(new DuoPartnerSaveEntry { hero = "Nobody", partner = "Icaro" });
            bad.partners.Add(null);
            loaded.RestoreState(bad);
            Assert.AreEqual(CharacterId.Joaquim, loaded.GetPartnerFor(CharacterId.Maria), "bad entries fall back to the default");
            Assert.AreEqual(CharacterId.Icaro, loaded.GetPartnerFor(CharacterId.Maithe), "a restore replaces the old picks");

            loaded.RestoreState(null);
            Assert.AreEqual(CharacterId.Icaro, loaded.GetPartnerFor(CharacterId.Maithe), "old save without the entry: defaults");

            state.ResetForNewSession();
            Assert.AreEqual(CharacterId.Icaro, state.GetPartnerFor(CharacterId.Maithe));
        }

        [Test]
        public void GameManager_RegistersThePartnerState_SavesIt_AndNewGameResetsIt()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            _gameManagerObject = new GameObject("DuoStrikeTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.DuoPartners, Is.Not.Null);

            Assert.IsTrue(_gm.NewSessionReset.Resettables.Contains(_gm.DuoPartners));
            Assert.AreSame(_gm.DuoPartners, DuoPartnerState.ResolveShared());

            _gm.DuoPartners.SetPartner(CharacterId.Maria, CharacterId.Lavine);
            _gm.SaveManager.Save(TestSlot);
            Assert.IsTrue(_gm.SaveManager.PeekSlot(TestSlot).entries.Any(e => e.key == DuoPartnerState.Key), "registered with SaveManager");

            _gm.GameFlowService.EnterFrontEnd();
            Assert.IsTrue(_gm.GameFlowService.StartNewGame());
            Assert.AreEqual(CharacterId.Joaquim, _gm.DuoPartners.GetPartnerFor(CharacterId.Maria), "Novo jogo starts with the defaults");

            Assert.IsTrue(_gm.SaveManager.Load(TestSlot));
            Assert.AreEqual(CharacterId.Lavine, _gm.DuoPartners.GetPartnerFor(CharacterId.Maria));
        }

        // ------------------------------------------------------------------
        // Texts / input / HUD
        // ------------------------------------------------------------------

        [Test]
        public void EveryCombo_HasANameAndTwoShoutLines_InBothLanguages()
        {
            Dictionary<string, LocalizationEntry> table = DuoTexts.Entries.ToDictionary(e => e.Key);
            foreach (DuoComboSpec combo in DuoComboCatalog.All)
            {
                foreach (string key in new[] { combo.NameKey, combo.LineKey(0), combo.LineKey(1) })
                {
                    Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"missing {key}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key} en");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key} pt-BR");
                }

                Assert.AreNotEqual(combo.LineKey(0), combo.LineKey(1));
                StringAssert.StartsWith(DuoTexts.CousinName(combo.Elder) + ": ", DuoTexts.ShoutLine(null, combo, 0));
                StringAssert.StartsWith(DuoTexts.CousinName(combo.Younger) + ": ", DuoTexts.ShoutLine(null, combo, 1));
            }

            Assert.AreEqual("Flecha-Trovão", DuoTexts.Get(null, DuoComboCatalog.FindById(DuoComboCatalog.SkyThunderId).NameKey));
        }

        [Test]
        public void PlayerControls_DefinesTheDuoStrikeAction_WithKeyboardAndGamepad_AndARebindRow()
        {
            string json = File.ReadAllText(Path.Combine("Assets", "InputActions", "PlayerControls.inputactions"));
            InputActionAsset asset = InputActionAsset.FromJson(json);
            _created.Add(asset);
            InputAction action = asset.FindAction(DuoStrikeController.InputActionName, throwIfNotFound: false);
            Assert.IsNotNull(action);
            Assert.IsTrue(action.bindings.Any(b => b.path.StartsWith("<Keyboard>", StringComparison.Ordinal)));
            Assert.IsTrue(action.bindings.Any(b => b.path.StartsWith("<Gamepad>", StringComparison.Ordinal)));

            // The default keys don't collide with another action.
            foreach (InputBinding binding in action.bindings)
            {
                foreach (InputAction other in asset.actionMaps[0].actions.Where(a => a != action))
                {
                    Assert.IsFalse(other.bindings.Any(b => b.path == binding.path), $"{binding.path} is also bound to {other.name}");
                }
            }

            Assert.IsTrue(SettingsMenuTexts.Rows.Any(r => r.ActionName == DuoStrikeController.InputActionName), "rebindable in Configurações → Controles");
        }

        [Test]
        public void Hud_View_HasTheButtonAndBanner_HiddenAndNonBlocking()
        {
            VisualElement layer = DuoStrikeHud.CreateView(reducedMotion: false);
            Assert.AreEqual(PickingMode.Ignore, layer.pickingMode);
            Button button = layer.Q<Button>(DuoStrikeHud.ButtonName);
            Assert.IsNotNull(button);
            Assert.AreEqual(DisplayStyle.None, button.style.display.value);
            VisualElement banner = layer.Q<VisualElement>(DuoStrikeHud.BannerName);
            Assert.IsNotNull(banner);
            Assert.AreEqual(PickingMode.Ignore, banner.pickingMode);
            Assert.IsTrue(layer.ClassListContains(DuoStrikeStyles.ThemeClass));
            Assert.IsNotNull(DuoStrikeStyles.Sheet, "Resources/UI/DuoStrike.uss must import");
        }

        [Test]
        public void Hud_Rules_ShowTheButtonOnlyInAFight_AndLabelTheState()
        {
            Assert.IsTrue(DuoStrikeHud.ShouldShowButton(true, true, hasEnemies: true, executing: false, paused: false));
            Assert.IsFalse(DuoStrikeHud.ShouldShowButton(true, true, hasEnemies: false, executing: false, paused: false));
            Assert.IsTrue(DuoStrikeHud.ShouldShowButton(true, true, hasEnemies: false, executing: true, paused: false), "the last enemy died mid-strike");
            Assert.IsFalse(DuoStrikeHud.ShouldShowButton(true, true, true, false, paused: true));
            Assert.IsFalse(DuoStrikeHud.ShouldShowButton(false, true, true, false, false));
            Assert.IsFalse(DuoStrikeHud.ShouldShowButton(true, false, true, false, false));

            Assert.AreEqual("Dupla 40%", DuoStrikeHud.ButtonLabel(null, DuoStrikeReadiness.Charging, 0.4f));
            Assert.AreEqual("Dupla!", DuoStrikeHud.ButtonLabel(null, DuoStrikeReadiness.Ready, 1f));
            Assert.AreEqual("Usado", DuoStrikeHud.ButtonLabel(null, DuoStrikeReadiness.Spent, 0f));
            Assert.AreEqual("com Ícaro · F", DuoStrikeHud.PartnerCaption(null, CharacterId.Icaro, "F"));
            Assert.AreEqual("Parceiro da dupla: Maria", DuoPartnerPauseButton.Label(null, CharacterId.Maria));
        }
    }
}
