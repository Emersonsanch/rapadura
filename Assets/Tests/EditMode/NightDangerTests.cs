using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Day/night danger: <see cref="NightDangerRules"/> (multipliers, wave timing/size, gates), the region × period spawn
    /// tables (<see cref="PeriodSpawnTable"/>), <see cref="NightVariantRules"/>/<see cref="NightVariantTag"/> (look,
    /// names, rewards), <see cref="NightVisibilityRules"/> ("menos visão à noite", "Noite mais clara") and the HUD rule.
    /// </summary>
    public class NightDangerTests
    {
        private static readonly TimeOfDayPhase[] AllPhases = (TimeOfDayPhase[])Enum.GetValues(typeof(TimeOfDayPhase));
        private static readonly BiomeType[] AllRegions = (BiomeType[])Enum.GetValues(typeof(BiomeType));

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            NightVisibility.Reset();
            PlayerPrefs.DeleteKey("Accessibility.ClearerNight");
        }

        private static EnemyDefinition Load(string id) => Resources.Load<EnemyDefinition>("Enemies/" + id);

        // ------------------------------------------------------------------
        // NightDangerRules
        // ------------------------------------------------------------------

        [Test]
        public void Night_IsMoreDangerousThanDay_OnEveryAxis()
        {
            NightDangerProfile day = NightDangerRules.Day;
            NightDangerProfile night = NightDangerRules.Night;

            Assert.Greater(night.ReinforcementWaves, day.ReinforcementWaves, "more waves");
            Assert.Less(night.WaveIntervalSeconds, day.WaveIntervalSeconds, "waves come faster");
            Assert.Greater(night.WaveSizeMultiplier, day.WaveSizeMultiplier, "bigger waves");
            Assert.Greater(night.VariantChance, day.VariantChance);
            Assert.Greater(night.HealthMultiplier, 1f);
            Assert.Greater(night.DamageBonus, 0f);
            Assert.Greater(night.AggroRadiusMultiplier, 1f);
            Assert.Less(night.AggroHoldMultiplier, 1f, "engages sooner");
            Assert.Greater(night.ExperienceMultiplier, 1f, "better XP");
            Assert.Greater(night.ExtraLootRolls, 0, "better loot");
            Assert.Greater(night.SpawnRateVersusDay, 1f);
        }

        [Test]
        public void DayKeepsTodaysFight_OneReinforcementNoVariants()
        {
            NightDangerProfile day = NightDangerRules.For(TimeOfDayPhase.Day);
            Assert.AreEqual(0f, day.VariantChance);
            Assert.AreEqual(1f, day.WaveSizeMultiplier);
            Assert.AreEqual(2, NightDangerRules.TotalWaves(TimeOfDayPhase.Day));
            Assert.AreEqual(3, NightDangerRules.TotalWaves(TimeOfDayPhase.Night));
        }

        [Test]
        public void DawnAndDusk_SitBetweenDayAndNight()
        {
            foreach (TimeOfDayPhase phase in new[] { TimeOfDayPhase.Dawn, TimeOfDayPhase.Dusk })
            {
                NightDangerProfile p = NightDangerRules.For(phase);
                Assert.That(p.WaveIntervalSeconds, Is.InRange(NightDangerRules.Night.WaveIntervalSeconds, NightDangerRules.Day.WaveIntervalSeconds), phase.ToString());
                Assert.That(p.VariantChance, Is.InRange(NightDangerRules.Day.VariantChance, NightDangerRules.Night.VariantChance), phase.ToString());
                Assert.That(p.ReinforcementWaves, Is.InRange(NightDangerRules.Day.ReinforcementWaves, NightDangerRules.Night.ReinforcementWaves), phase.ToString());
            }
        }

        [Test]
        public void VariantStats_AreTheNightProfile()
        {
            Assert.AreEqual(NightDangerRules.Night.HealthMultiplier, NightDangerRules.VariantStats.HealthMultiplier);
            Assert.AreEqual(NightDangerRules.Night.ExperienceMultiplier, NightDangerRules.VariantStats.ExperienceMultiplier);
        }

        [Test]
        public void WaveTiming_FollowsThePhaseAtTheMomentTheWaveIsCleared()
        {
            // Day: first wave + 1.
            Assert.IsTrue(NightDangerRules.ShouldSpawnAnotherWave(1, TimeOfDayPhase.Day));
            Assert.IsFalse(NightDangerRules.ShouldSpawnAnotherWave(2, TimeOfDayPhase.Day));

            // Night: first wave + 2.
            Assert.IsTrue(NightDangerRules.ShouldSpawnAnotherWave(1, TimeOfDayPhase.Night));
            Assert.IsTrue(NightDangerRules.ShouldSpawnAnotherWave(2, TimeOfDayPhase.Night));
            Assert.IsFalse(NightDangerRules.ShouldSpawnAnotherWave(3, TimeOfDayPhase.Night));

            // Dusk → Night mid-fight: wave 2 cleared at dusk would end it, but cleared at night brings wave 3.
            Assert.IsFalse(NightDangerRules.ShouldSpawnAnotherWave(2, TimeOfDayPhase.Dusk));
            Assert.IsTrue(NightDangerRules.ShouldSpawnAnotherWave(2, TimeOfDayPhase.Night));

            // Night → Dawn mid-fight: the next decision uses dawn's (smaller) count.
            Assert.IsFalse(NightDangerRules.ShouldSpawnAnotherWave(2, TimeOfDayPhase.Dawn));

            Assert.IsFalse(NightDangerRules.ShouldSpawnAnotherWave(0, TimeOfDayPhase.Night), "no first wave, no reinforcements");

            Assert.AreEqual(4f, NightDangerRules.WaveInterval(TimeOfDayPhase.Day));
            Assert.AreEqual(2.5f, NightDangerRules.WaveInterval(TimeOfDayPhase.Night));
            Assert.Less(NightDangerRules.ReinforcementAggroHold(1.75f, TimeOfDayPhase.Night), NightDangerRules.ReinforcementAggroHold(1.75f, TimeOfDayPhase.Day));
        }

        [TestCase(1, TimeOfDayPhase.Day, 1)]
        [TestCase(2, TimeOfDayPhase.Day, 2)]
        [TestCase(1, TimeOfDayPhase.Night, 2)]
        [TestCase(2, TimeOfDayPhase.Night, 3)]
        [TestCase(3, TimeOfDayPhase.Night, 4)]
        [TestCase(6, TimeOfDayPhase.Night, NightDangerRules.MaxWaveSize)]
        [TestCase(0, TimeOfDayPhase.Day, 1)]
        public void WaveSize_ScalesWithNightAndIsCapped(int firstWave, TimeOfDayPhase phase, int expected)
        {
            Assert.AreEqual(expected, NightDangerRules.WaveSize(firstWave, phase));
        }

        [Test]
        public void Reinforcements_NeverForBossesEliteOrTutorial()
        {
            Assert.IsTrue(NightDangerRules.AllowsReinforcements(false, false, false, 1));
            Assert.IsFalse(NightDangerRules.AllowsReinforcements(true, false, false, 5), "boss challenge");
            Assert.IsFalse(NightDangerRules.AllowsReinforcements(false, true, false, 5), "group with a boss");
            Assert.IsFalse(NightDangerRules.AllowsReinforcements(false, false, true, 5), "elite ambush");
            Assert.IsFalse(NightDangerRules.AllowsReinforcements(false, false, false, 0), "first fight");

            Assert.IsTrue(NightDangerRules.AllowsNightVariants(false, false, 1));
            Assert.IsFalse(NightDangerRules.AllowsNightVariants(true, false, 1));
            Assert.IsFalse(NightDangerRules.AllowsNightVariants(false, true, 1));
            Assert.IsFalse(NightDangerRules.AllowsNightVariants(false, false, 0));
        }

        [Test]
        public void RollVariant_NeverByDay_AboutHalfAtNight_Deterministic()
        {
            var random = new System.Random(7);
            for (int i = 0; i < 200; i++)
            {
                Assert.IsFalse(NightDangerRules.RollVariant(TimeOfDayPhase.Day, random));
            }

            random = new System.Random(7);
            int hits = 0;
            for (int i = 0; i < 2000; i++)
            {
                hits += NightDangerRules.RollVariant(TimeOfDayPhase.Night, random) ? 1 : 0;
            }

            Assert.That(hits / 2000f, Is.InRange(0.45f, 0.55f));
            Assert.AreEqual(NightDangerRules.RollVariant(TimeOfDayPhase.Night, new System.Random(3)), NightDangerRules.RollVariant(TimeOfDayPhase.Night, new System.Random(3)));
        }

        // ------------------------------------------------------------------
        // PeriodSpawnTable
        // ------------------------------------------------------------------

        [Test]
        public void EveryRegion_HasEntriesForEveryPhase_IncludingDayAndNight()
        {
            foreach (BiomeType region in AllRegions)
            {
                foreach (TimeOfDayPhase phase in AllPhases)
                {
                    Assert.Greater(PeriodSpawnTable.TotalWeight(PeriodSpawnTable.GetEntries(region, phase)), 0, $"{region} {phase}");
                }
            }
        }

        [Test]
        public void EveryEntry_ResolvesToANonBossEnemyAsset_AndSpeciesExist()
        {
            foreach (BiomeType region in AllRegions)
            {
                foreach (TimeOfDayPhase phase in AllPhases)
                {
                    foreach (PeriodSpawnEntry entry in PeriodSpawnTable.GetEntries(region, phase))
                    {
                        EnemyDefinition enemy = Load(entry.EnemyId);
                        Assert.IsNotNull(enemy, $"{region} {phase}: {entry.EnemyId} missing from Resources/Enemies");
                        Assert.IsFalse(enemy.IsBoss, $"{region} {phase}: {entry.EnemyId} is a boss (boss fights stay unchanged)");
                        if (entry.Form == NightForm.Species)
                        {
                            NightSpecies species = NightVariantRules.FindSpecies(entry.SpeciesId);
                            Assert.IsNotNull(species, entry.SpeciesId);
                            Assert.AreEqual(species.BaseEnemyId, entry.EnemyId);
                        }
                    }
                }
            }
        }

        [Test]
        public void EveryEntry_BelongsToItsRegion_SoLootAndXpMatch()
        {
            foreach (BiomeType region in AllRegions)
            {
                BiomeDefinition biome = BiomeRegistry.Get(region);
                Assert.IsNotNull(biome, region.ToString());
                foreach (TimeOfDayPhase phase in AllPhases)
                {
                    foreach (PeriodSpawnEntry entry in PeriodSpawnTable.GetEntries(region, phase))
                    {
                        // A night species may visit another region (scaled to it): its definition lives in its home biome.
                        NightSpecies species = entry.Form == NightForm.Species ? NightVariantRules.FindSpecies(entry.SpeciesId) : null;
                        BiomeDefinition home = species != null ? BiomeRegistry.Get(species.HomeRegion) : biome;
                        Assert.IsTrue(home.ContainsEnemy(Load(entry.EnemyId)), $"{entry.EnemyId} is not in the {home.Type} biome table");
                    }
                }
            }
        }

        [Test]
        public void DayTable_IsTodaysRegularEnemies_WithTheBiomeWeights()
        {
            foreach (BiomeType region in AllRegions)
            {
                BiomeDefinition biome = BiomeRegistry.Get(region);
                var expected = biome.Enemies.Where(e => e != null && e.Enemy != null && !e.Enemy.IsBoss && e.Weight > 0)
                    .ToDictionary(e => e.Enemy.name, e => e.Weight);
                var day = PeriodSpawnTable.GetEntries(region, TimeOfDayPhase.Day);

                Assert.IsTrue(day.All(e => e.Form == NightForm.Normal), $"{region}: day has only normal enemies");
                CollectionAssert.AreEquivalent(expected, day.ToDictionary(e => e.EnemyId, e => e.Weight), $"{region}: day table drifted from the biome asset");
            }
        }

        [Test]
        public void NightTable_IsMostlyNightCreatures_DayHasNone()
        {
            foreach (BiomeType region in AllRegions)
            {
                var night = PeriodSpawnTable.GetEntries(region, TimeOfDayPhase.Night);
                int total = PeriodSpawnTable.TotalWeight(night);
                int nightly = night.Where(e => e.Form != NightForm.Normal).Sum(e => e.Weight);
                Assert.Greater(nightly * 2, total, $"{region}: night should be mostly variants/species");

                foreach (TimeOfDayPhase phase in new[] { TimeOfDayPhase.Dawn, TimeOfDayPhase.Dusk })
                {
                    var entries = PeriodSpawnTable.GetEntries(region, phase);
                    int twilight = entries.Where(e => e.Form != NightForm.Normal).Sum(e => e.Weight);
                    Assert.Greater(twilight, 0, $"{region} {phase}: a few night creatures");
                    Assert.Less(twilight * 2, PeriodSpawnTable.TotalWeight(entries), $"{region} {phase}: still mostly day creatures");
                }
            }
        }

        [Test]
        public void PlayableSpecies_AppearOnlyAtNight_InTheirRegion()
        {
            Assert.IsTrue(PeriodSpawnTable.GetEntries(BiomeType.Forest, TimeOfDayPhase.Night).Any(e => e.SpeciesId == NightVariantRules.SpeciesLobisomem));
            Assert.IsTrue(PeriodSpawnTable.GetEntries(BiomeType.Caves, TimeOfDayPhase.Night).Any(e => e.SpeciesId == NightVariantRules.SpeciesMorcegoVampiro));
            foreach (BiomeType region in AllRegions)
            {
                foreach (TimeOfDayPhase phase in new[] { TimeOfDayPhase.Dawn, TimeOfDayPhase.Day, TimeOfDayPhase.Dusk })
                {
                    // Only the dusk-loving species (fireflies) may come before full night.
                    Assert.IsFalse(PeriodSpawnTable.GetEntries(region, phase).Any(e => e.Form == NightForm.Species
                        && !(phase == TimeOfDayPhase.Dusk && NightVariantRules.FindSpecies(e.SpeciesId).ComesAtDusk)), $"{region} {phase}");
                }
            }
        }

        [Test]
        public void RollWave_IsDeterministic_SizedAndRespectsSpeciesCap()
        {
            List<PeriodSpawnPick> a = PeriodSpawnTable.RollWave(BiomeType.Forest, TimeOfDayPhase.Night, 4, new System.Random(42));
            List<PeriodSpawnPick> b = PeriodSpawnTable.RollWave(BiomeType.Forest, TimeOfDayPhase.Night, 4, new System.Random(42));
            Assert.AreEqual(4, a.Count);
            CollectionAssert.AreEqual(a.Select(p => p.Enemy.name + p.Form), b.Select(p => p.Enemy.name + p.Form));

            for (int seed = 0; seed < 300; seed++)
            {
                List<PeriodSpawnPick> wave = PeriodSpawnTable.RollWave(BiomeType.Forest, TimeOfDayPhase.Night, 4, new System.Random(seed));
                Assert.LessOrEqual(wave.Count(p => p.Species == NightVariantRules.Lobisomem), NightVariantRules.Lobisomem.MaxPerWave, "seed " + seed);
                Assert.IsTrue(wave.All(p => !p.Enemy.IsBoss));
            }

            List<PeriodSpawnPick> day = PeriodSpawnTable.RollWave(BiomeType.Desert, TimeOfDayPhase.Day, 3, new System.Random(1));
            Assert.IsTrue(day.All(p => p.Form == NightForm.Normal && p.Look.IsNormal));
            Assert.AreEqual(0, PeriodSpawnTable.RollWave(BiomeType.Desert, TimeOfDayPhase.Day, 0, new System.Random(1)).Count);
        }

        [Test]
        public void RollWave_SkipsUnresolvedIds()
        {
            List<PeriodSpawnPick> wave = PeriodSpawnTable.RollWave(BiomeType.Forest, TimeOfDayPhase.Day, 3, new System.Random(1), id => null);
            Assert.AreEqual(0, wave.Count);
        }

        // ------------------------------------------------------------------
        // NightVariantRules / NightVariantTag
        // ------------------------------------------------------------------

        [Test]
        public void Variant_IsDarkerBiggerTougher_WithGlowingEyes_NeverBlack()
        {
            NightLook look = NightVariantRules.Resolve(NightForm.Variant);
            Assert.AreEqual(NightForm.Variant, look.Form);
            Assert.Less(look.Tint.maxColorComponent, 1f, "darker");
            Assert.GreaterOrEqual(Mathf.Min(look.Tint.r, Mathf.Min(look.Tint.g, look.Tint.b)), NightVariantRules.MinTintChannel, "never a silhouette");
            Assert.Greater(look.EyeColor.a, 0f);
            Assert.Greater(look.ScaleMultiplier, 1f);
            Assert.Greater(look.HealthMultiplier, 1f);
            Assert.Greater(look.DamageBonus, 0f);

            NightLook normal = NightVariantRules.Resolve(NightForm.Normal);
            Assert.IsTrue(normal.IsNormal);
            Assert.AreEqual(1f, normal.HealthMultiplier);

            Assert.AreEqual(NightForm.Variant, NightVariantRules.Resolve(NightForm.Species, null).Form, "species without data falls back to a variant");
        }

        [Test]
        public void Species_LobisomemIsARareMiniBoss_MorcegoIsATougherBat()
        {
            NightLook wolf = NightVariantRules.Resolve(NightForm.Species, NightVariantRules.Lobisomem);
            NightLook variant = NightVariantRules.Resolve(NightForm.Variant);
            Assert.Greater(wolf.HealthMultiplier, variant.HealthMultiplier * 2f);
            // Own big model (no more recoloured wolf): no extra scale, no tint, the model's own glowing eyes.
            Assert.AreEqual("enemy_forest_werewolf", NightVariantRules.Lobisomem.BaseEnemyId);
            Assert.AreEqual(1f, wolf.ScaleMultiplier);
            Assert.AreEqual(1, NightVariantRules.Lobisomem.MaxPerWave);

            var forestNight = PeriodSpawnTable.GetEntries(BiomeType.Forest, TimeOfDayPhase.Night);
            float share = forestNight.Where(e => e.SpeciesId == NightVariantRules.SpeciesLobisomem).Sum(e => e.Weight) / (float)PeriodSpawnTable.TotalWeight(forestNight);
            Assert.Less(share, 0.15f, "rare");

            NightLook bat = NightVariantRules.Resolve(NightForm.Species, NightVariantRules.MorcegoVampiro);
            Assert.Greater(bat.HealthMultiplier, variant.HealthMultiplier);
            Assert.AreEqual(NightVariantRules.MorcegoVampiro, NightVariantRules.FindSpecies(NightVariantRules.SpeciesMorcegoVampiro));
            Assert.IsNull(NightVariantRules.FindSpecies("nope"));
        }

        [Test]
        public void BonusExperience_IsTheExtraOverTheNormalKill()
        {
            NightLook variant = NightVariantRules.Resolve(NightForm.Variant);
            Assert.AreEqual(5, NightVariantRules.BonusExperience(10, variant), "10 XP × 1.5 = 15 → +5");
            Assert.AreEqual(0, NightVariantRules.BonusExperience(10, NightVariantRules.Resolve(NightForm.Normal)));
            Assert.AreEqual(40, NightVariantRules.BonusExperience(10, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.Lobisomem)));
            Assert.AreEqual(0, NightVariantRules.BonusExperience(-5, variant));
        }

        [Test]
        public void DisplayName_AddsTheNocturnalSuffix_OrUsesTheSpeciesName()
        {
            Assert.AreEqual("Lobo da Mata (noturno)", NightVariantRules.DisplayName("Lobo da Mata", NightVariantRules.Resolve(NightForm.Variant), "{0} (noturno)", null));
            Assert.AreEqual("Wolf (nocturnal)", NightVariantRules.DisplayName("Wolf", NightVariantRules.Resolve(NightForm.Variant), "{0} (nocturnal)", null));
            Assert.AreEqual("Lobo da Mata (noturno)", NightVariantRules.DisplayName("Lobo da Mata", NightVariantRules.Resolve(NightForm.Variant), null, null), "fallback pattern");
            Assert.AreEqual("Lobisomem-do-sertão", NightVariantRules.DisplayName("Lobo da Mata", NightVariantRules.Resolve(NightForm.Species, NightVariantRules.Lobisomem), "{0} (noturno)", "Lobisomem-do-sertão"));
            Assert.AreEqual("Lobo da Mata", NightVariantRules.DisplayName("Lobo da Mata", NightVariantRules.Resolve(NightForm.Normal), "{0} (noturno)", null));

            Assert.AreEqual("{0} (noturno)", NightTexts.GetFallback(NightTexts.VariantName));
            Assert.AreEqual("Noite — os bichos estão mais bravos", NightTexts.GetFallback(NightTexts.HudNight));
            Assert.AreEqual("Lobisomem-do-sertão", NightTexts.GetFallback(NightVariantRules.Lobisomem.NameKey));
        }

        [Test]
        public void Tag_AppliesHealthDamageScaleAndEyes_LikeTheElite()
        {
            var enemy = new GameObject("night_enemy");
            _created.Add(enemy);
            Health health = enemy.AddComponent<Health>();
            health.ConfigureMaxHealth(100f);
            BuffController buffs = enemy.AddComponent<BuffController>();
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = EnemyVisualSwapper.VisualName;
            visual.transform.SetParent(enemy.transform, false);
            Material source = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { color = Color.white };
            _created.Add(source);
            visual.GetComponent<Renderer>().sharedMaterial = source;

            NightVariantTag tag = NightVariantTag.Attach(enemy, NightVariantRules.Resolve(NightForm.Variant), applyNow: true);
            Assert.IsNotNull(tag);
            Assert.IsTrue(tag.IsApplied);
            Assert.AreEqual(100f * NightDangerRules.VariantStats.HealthMultiplier, health.MaxHealth, 0.01f);
            Assert.AreEqual(100f * (1f + NightDangerRules.VariantStats.DamageBonus), buffs.GetModifiedValue(StatType.AttackDamage, 100f), 0.01f);
            Assert.AreEqual(NightVariantRules.VariantScale, visual.transform.localScale.x, 0.001f);
            Assert.IsNotNull(tag.Eyes);
            Assert.AreEqual(2, tag.Eyes.transform.childCount);
            Assert.IsNull(tag.Eyes.GetComponentInChildren<Collider>(), "eyes have no collider (DestroyImmediate in EditMode)");

            Material tinted = visual.GetComponent<Renderer>().sharedMaterial;
            Assert.AreNotSame(source, tinted, "recoloured with a copy, the shared material is untouched");
            Assert.Less(tinted.color.r, 1f);
            Assert.AreEqual(Color.white, source.color);

            tag.ApplyNow();
            Assert.AreEqual(100f * NightDangerRules.VariantStats.HealthMultiplier, health.MaxHealth, 0.01f, "idempotent");
            Assert.AreEqual(NightVariantRules.VariantScale, visual.transform.localScale.x, 0.001f, "idempotent");
        }

        [Test]
        public void Tag_IgnoresNormalLooks()
        {
            var enemy = new GameObject("day_enemy");
            _created.Add(enemy);
            Assert.IsNull(NightVariantTag.Attach(enemy, NightVariantRules.Resolve(NightForm.Normal)));
            Assert.IsNull(enemy.GetComponent<NightVariantTag>());
            Assert.IsNull(NightVariantTag.Attach(null, NightVariantRules.Resolve(NightForm.Variant)));
        }

        // ------------------------------------------------------------------
        // NightVisibilityRules
        // ------------------------------------------------------------------

        [Test]
        public void Ambient_DarkerAtNight_NeverPitchBlack_ClearerAndCavesSoften()
        {
            Assert.AreEqual(1f, NightVisibilityRules.AmbientMultiplier(1f, false, false), "day untouched");
            float night = NightVisibilityRules.AmbientMultiplier(0f, false, false);
            float clearer = NightVisibilityRules.AmbientMultiplier(0f, true, false);
            float caves = NightVisibilityRules.AmbientMultiplier(0f, false, true);
            Assert.Less(night, 1f);
            Assert.GreaterOrEqual(night, NightVisibilityRules.AbsoluteAmbientFloor);
            Assert.Greater(clearer, night, "Noite mais clara limits the darkening");
            Assert.Greater(caves, night, "caves are dark already: not doubled");
            Assert.That(NightVisibilityRules.AmbientMultiplier(0.5f, false, false), Is.InRange(night, 1f), "smooth through dusk");
            Assert.AreEqual(1f, NightVisibilityRules.AmbientMultiplier(2f, false, false), "clamped");
        }

        [Test]
        public void Fog_OnlyAtNight_LighterWithOptionAndInCaves()
        {
            Assert.AreEqual(0f, NightVisibilityRules.ExtraFogDensity(1f, false, false));
            float night = NightVisibilityRules.ExtraFogDensity(0f, false, false);
            Assert.AreEqual(NightVisibilityRules.NightFogDensity, night, 1e-6f);
            Assert.Less(NightVisibilityRules.ExtraFogDensity(0f, true, false), night);
            Assert.Less(NightVisibilityRules.ExtraFogDensity(0f, false, true), night);

            NightVisibility.ClearerNightOverride = false;
            NightVisibility.InArena = false;
            Assert.AreEqual(0f, NightVisibility.ExtraFogDensity(0f), "no night fog on the world map");
            NightVisibility.InArena = true;
            Assert.AreEqual(night, NightVisibility.ExtraFogDensity(0f), 1e-6f);
        }

        [Test]
        public void FarClip_ShortensAtNight_NeverHidesTheHero_NeverGrows()
        {
            Assert.AreEqual(1000f, NightVisibilityRules.FarClip(1000f, 1f, false, 10f), "day: scene value");
            Assert.AreEqual(NightVisibilityRules.NightFarClip, NightVisibilityRules.FarClip(1000f, 0f, false, 10f), 0.01f);
            Assert.AreEqual(NightVisibilityRules.ClearerNightFarClip, NightVisibilityRules.FarClip(1000f, 0f, true, 10f), 0.01f);
            Assert.AreEqual(60f + NightVisibilityRules.MinVisibleBeyondHero, NightVisibilityRules.FarClip(1000f, 0f, false, 60f), 0.01f, "camera far from the hero");
            Assert.AreEqual(50f, NightVisibilityRules.FarClip(50f, 0f, false, 5f), "never longer than the scene's own");
        }

        [Test]
        public void Lamparina_LitAtNight_AlwaysInCaves_RadiusBonusCapped()
        {
            Assert.AreEqual(0f, NightVisibilityRules.LightIntensityFor(1f, false));
            Assert.AreEqual(NightVisibilityRules.LightIntensity, NightVisibilityRules.LightIntensityFor(0f, false), 1e-5f);
            Assert.Greater(NightVisibilityRules.LightIntensityFor(1f, true), 0f, "caves: the lamp never goes out");

            Assert.AreEqual(NightVisibilityRules.BaseLightRadius, NightVisibilityRules.LightRadius(0f));
            Assert.AreEqual(NightVisibilityRules.BaseLightRadius + 3f, NightVisibilityRules.LightRadius(3f));
            Assert.AreEqual(NightVisibilityRules.MaxLightRadius, NightVisibilityRules.LightRadius(100f));
            Assert.AreEqual(NightVisibilityRules.BaseLightRadius, NightVisibilityRules.LightRadius(-4f));
        }

        [Test]
        public void ClearerNight_IsAPersistedAccessibilitySetting()
        {
            PlayerPrefs.DeleteKey("Accessibility.ClearerNight");
            var settings = new AccessibilitySettings();
            settings.Initialize();
            Assert.IsFalse(settings.ClearerNight);

            bool changed = false;
            settings.SettingsChanged += () => changed = true;
            settings.SetClearerNight(true);
            Assert.IsTrue(settings.ClearerNight);
            Assert.IsTrue(changed);

            var reloaded = new AccessibilitySettings();
            reloaded.Initialize();
            Assert.IsTrue(reloaded.ClearerNight);
            settings.Shutdown();
            reloaded.Shutdown();
        }

        // ------------------------------------------------------------------
        // HUD
        // ------------------------------------------------------------------

        [Test]
        public void HudIndicator_OnlyAtNight_WithGameplayHud_NotPaused()
        {
            Assert.IsTrue(NightDangerIndicator.ShouldShow(TimeOfDayPhase.Night, true, false));
            Assert.IsFalse(NightDangerIndicator.ShouldShow(TimeOfDayPhase.Dusk, true, false));
            Assert.IsFalse(NightDangerIndicator.ShouldShow(TimeOfDayPhase.Day, true, false));
            Assert.IsFalse(NightDangerIndicator.ShouldShow(TimeOfDayPhase.Night, false, false));
            Assert.IsFalse(NightDangerIndicator.ShouldShow(TimeOfDayPhase.Night, true, true));

            var view = NightDangerIndicator.CreateView();
            Assert.IsNotNull(view.Q(NightDangerIndicator.PillName));
            Assert.IsNotNull(view.Q<UnityEngine.UIElements.Label>(NightDangerIndicator.LabelName));
            Assert.AreEqual(UnityEngine.UIElements.PickingMode.Ignore, view.pickingMode);
        }
    }
}
