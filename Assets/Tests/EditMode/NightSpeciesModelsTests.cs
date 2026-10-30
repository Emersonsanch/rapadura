using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Locomotion;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Achievements;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The night species' own models and data (Docs/DayNight.md, night-models pass): recipes (bounds, named parts,
    /// jointed legs bound by the gait engine, glowing eyes, budgets — the generic checks also run in
    /// <see cref="ProceduralEnemyModelsTests"/>), the werewolf rearing up, the potoo's gape, the firefly lanterns, the
    /// voice cues, the definitions (home region at weight 0, baseline stats), the spawn tables (night/dusk only, herds,
    /// visitors scaled to the region), bestiary pages and the Lobisomem achievement.
    /// </summary>
    public class NightSpeciesModelsTests
    {
        private static readonly ProceduralEnemyKind[] NightKinds =
        {
            ProceduralEnemyKind.Werewolf, ProceduralEnemyKind.VampireBat, ProceduralEnemyKind.ShroudOwl,
            ProceduralEnemyKind.FireflySwarm, ProceduralEnemyKind.Peccary, ProceduralEnemyKind.Potoo,
        };

        private static readonly EnemySwatch[] EyeSwatches = { EnemySwatch.WarEye, EnemySwatch.MoonEye, EnemySwatch.Ember, EnemySwatch.FireflyGlow };

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
            BiomeRegistry.Invalidate();
        }

        private GameObject Model(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            return visual;
        }

        private static EnemyDefinition Load(string id) => Resources.Load<EnemyDefinition>("Enemies/" + id);

        // ------------------------------------------------------------------
        // Models
        // ------------------------------------------------------------------

        [Test]
        public void EverySpecies_HasItsOwnModelKind_AndDefinition()
        {
            Assert.AreEqual(6, NightVariantRules.PlayableSpecies.Count);
            var kinds = new HashSet<ProceduralEnemyKind>();
            foreach (NightSpecies species in NightVariantRules.PlayableSpecies)
            {
                ProceduralEnemyKind kind = ProceduralEnemyModels.KindFor(species.BaseEnemyId);
                Assert.IsTrue(ProceduralEnemyModels.IsNightSpecies(kind), species.Id);
                Assert.IsTrue(kinds.Add(kind), $"{species.Id}: model shared with another species");
                Assert.IsTrue(ProceduralEnemyModels.AllKinds.Contains(kind));

                EnemyDefinition definition = Load(species.BaseEnemyId);
                Assert.IsNotNull(definition, species.BaseEnemyId);
                Assert.IsFalse(definition.IsBoss, "a night mini-boss is never a region boss");
                // Built at runtime until "Build Procedural Enemy Models" bakes and assigns the prefab.
                EnemyVisualSource source = EnemyVisualSwapper.ResolveSource(definition);
                Assert.IsTrue(source == EnemyVisualSource.Procedural || source == EnemyVisualSource.DefinitionPrefab, $"{species.Id}: {source}");
            }

            CollectionAssert.AreEquivalent(NightKinds, kinds);
        }

        [Test]
        public void EverySpecies_HasGlowingEyes_ThatReadInTheDark([ValueSource(nameof(NightKinds))] ProceduralEnemyKind kind)
        {
            bool eyes = false;
            foreach (Mesh mesh in ProceduralEnemyModels.GetBlueprint(kind).Meshes)
            {
                foreach (Vector2 uv in mesh.uv)
                {
                    eyes |= EyeSwatches.Contains(EnemyModelPalette.SwatchAt(uv));
                }
            }

            Assert.IsTrue(eyes, $"{kind}: no glowing eyes");
            foreach (EnemySwatch swatch in EyeSwatches)
            {
                Assert.GreaterOrEqual(EnemyModelPalette.GlowOf(swatch), 0.9f, swatch.ToString());
            }

            // Species keep their own palette and eyes: no recolour, no extra eye spheres from the night tag.
            NightSpecies species = NightVariantRules.FindByEnemyId(ProceduralEnemyModels.KindsById.First(e => e.Value == kind).Key);
            Assert.IsNotNull(species);
            Assert.AreEqual(Color.white, species.Tint);
            Assert.AreEqual(0f, species.EyeColor.a);
        }

        [Test]
        public void Palette_GrewWithoutMovingTheOldCells()
        {
            Assert.AreEqual(EnemyModelPalette.Columns * EnemyModelPalette.Rows, System.Enum.GetValues(typeof(EnemySwatch)).Length);
            foreach (EnemySwatch swatch in System.Enum.GetValues(typeof(EnemySwatch)))
            {
                Assert.AreEqual(swatch, EnemyModelPalette.SwatchAt(EnemyModelPalette.UvOf(swatch)), swatch.ToString());
            }
        }

        [Test]
        public void LeggedSpecies_BindToTheGaitEngine_AsQuadrupeds([Values(ProceduralEnemyKind.Werewolf, ProceduralEnemyKind.Peccary)] ProceduralEnemyKind kind)
        {
            var root = new GameObject("root");
            _created.Add(root);
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, root.transform);
            var legs = visual.AddComponent<ProceduralLegLocomotion>();

            Assert.IsTrue(legs.Bind(root.transform, visual.transform, 0f));
            Assert.AreEqual(4, legs.LegCount);
            Assert.AreEqual(LegGaitStyle.Quadruped, legs.DetectedStyle);
            Assert.Greater(legs.MeanLegLength, kind == ProceduralEnemyKind.Werewolf ? 0.7f : 0.25f);
            Assert.AreEqual(1f, legs.FrontLegsWeight, "default leaves the gait engine unchanged");
        }

        [Test]
        public void Werewolf_IsAMiniBossSizedHybrid_ThatRearsUpOnTheWindup()
        {
            var root = new GameObject("root");
            _created.Add(root);
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.Werewolf, root.transform);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds));
            Assert.Greater(bounds.size.y, 1.3f, "taller than a wolf on all fours");
            Assert.Greater(bounds.size.z, 1.6f, "long body");

            var legs = visual.AddComponent<ProceduralLegLocomotion>();
            legs.Bind(root.transform, visual.transform, 0f);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform head = rig.GetPart(ProceduralEnemyParts.Head);
            Transform frontPaw = rig.GetPart("Foot_FL");
            float headRest = head.position.y;
            float pawRest = frontPaw.position.y;

            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 30; i++)
            {
                rig.Tick(0.02f);
            }

            Assert.Greater(rig.Attack01, 0.99f);
            Assert.Greater(head.position.y, headRest + 0.4f, "head goes up when it rears");
            Assert.Greater(frontPaw.position.y, pawRest + 0.3f, "forelegs leave the ground");
            Assert.Less(legs.FrontLegsWeight, 0.01f, "the gait engine lets the forelegs go");

            // Reduced motion never shrinks the telegraph.
            rig.Tick(0.02f, 0f);
            Assert.Greater(rig.Attack01, 0.99f);

            rig.AttackPhaseOverride = EnemyAttackVisualPhase.None;
            for (int i = 0; i < 60; i++)
            {
                rig.Tick(0.02f);
            }

            Assert.AreEqual(0f, rig.Attack01, 0.001f);
            Assert.AreEqual(1f, legs.FrontLegsWeight, 0.001f, "back on all fours");
            Assert.AreEqual(headRest, head.position.y, 0.01f);
        }

        [Test]
        public void Potoo_OpensItsGape_AndCarriesItsBranch()
        {
            GameObject visual = Model(ProceduralEnemyKind.Potoo);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Quaternion rest = jaw.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            rig.Tick(0.5f);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 30f);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Branch).GetComponent<MeshFilter>());
        }

        [Test]
        public void FireflySwarm_IsManyGlowingLanterns_ThatBlink()
        {
            GameObject visual = Model(ProceduralEnemyKind.FireflySwarm);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform swarm = rig.GetPart(ProceduralEnemyParts.Swarm);
            Assert.That(swarm.childCount, Is.InRange(7, 12));
            foreach (Transform fly in swarm)
            {
                StringAssert.StartsWith(ProceduralEnemyParts.FireflyPrefix, fly.name);
                Assert.IsNotNull(fly.Find(ProceduralEnemyParts.Lantern), fly.name);
            }

            Transform lantern = swarm.GetChild(0).Find(ProceduralEnemyParts.Lantern);
            Vector3 scale = lantern.localScale;
            rig.Tick(0.37f);
            Assert.AreNotEqual(scale, lantern.localScale, "lanterns blink");
            Assert.That(lantern.localScale.x, Is.InRange(0.75f, 1.3f));
        }

        [Test]
        public void Flyers_FlapTheirWings([Values(ProceduralEnemyKind.VampireBat, ProceduralEnemyKind.ShroudOwl, ProceduralEnemyKind.Potoo)] ProceduralEnemyKind kind)
        {
            GameObject visual = Model(kind);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            rig.MoveSpeed01 = 1f;
            Transform wing = rig.GetPart(ProceduralEnemyParts.WingRight);
            Quaternion rest = wing.localRotation;
            rig.Tick(0.13f);
            Assert.AreNotEqual(rest, wing.localRotation);
        }

        // ------------------------------------------------------------------
        // Sound
        // ------------------------------------------------------------------

        [Test]
        public void EveryNightModel_HasAVoice_WithCallAndAttackCues()
        {
            List<SfxEventMap.Cue> cues = NightCreatureAudio.BuildCues();
            Assert.AreEqual(cues.Count, cues.Select(c => c.id).Distinct().Count());
            foreach (ProceduralEnemyKind kind in NightKinds)
            {
                NightCreatureVoice voice = Model(kind).GetComponent<NightCreatureVoice>();
                Assert.IsNotNull(voice, kind.ToString());
                Assert.AreEqual(kind, voice.Kind);
                foreach (string id in new[] { NightCreatureAudio.CallCue(kind), NightCreatureAudio.AttackCue(kind) })
                {
                    SfxEventMap.Cue cue = cues.FirstOrDefault(c => c.id == id);
                    Assert.IsNotNull(cue, $"{kind}: {id}");
                    Assert.AreNotEqual(ProceduralSfxPreset.None, cue.preset);
                    Assert.IsTrue(cue.spatial && cue.pausable, id);
                }

                Vector2 interval = NightCreatureAudio.CallInterval(kind);
                Assert.Greater(interval.x, 2f);
                Assert.GreaterOrEqual(interval.y, interval.x);
            }

            Assert.IsNull(Model(ProceduralEnemyKind.Wolf).GetComponent<NightCreatureVoice>(), "day creatures stay as they were");
        }

        [Test]
        public void EnsureCues_AddsOnlyMissingIds()
        {
            var map = ScriptableObject.CreateInstance<SfxEventMap>();
            _created.Add(map);
            int first = NightCreatureAudio.EnsureCues(map);
            Assert.AreEqual(NightCreatureAudio.BuildCues().Count, first);
            Assert.AreEqual(0, NightCreatureAudio.EnsureCues(map));
            Assert.IsTrue(map.TryGetCue(NightCreatureAudio.WerewolfHowl, out _));
        }

        // ------------------------------------------------------------------
        // Definitions, tables, rules
        // ------------------------------------------------------------------

        [Test]
        public void Definitions_AreTheirHomeRegionsCommonBaseline_AndNeverOnTheDayMap()
        {
            foreach (NightSpecies species in NightVariantRules.PlayableSpecies)
            {
                EnemyDefinition definition = Load(species.BaseEnemyId);
                Vector3 baseline = NightVariantRules.RegionBaseline(species.HomeRegion);
                Assert.AreEqual(baseline.x, definition.MaxHealth, 0.01f, species.Id);
                Assert.AreEqual(baseline.y, definition.AttackDamage, 0.01f, species.Id);
                Assert.AreEqual((int)baseline.z, definition.ExperienceReward, species.Id);

                BiomeDefinition home = BiomeRegistry.Get(species.HomeRegion);
                Assert.IsTrue(home.ContainsEnemy(definition), species.Id);
                Assert.AreSame(home, BiomeRegistry.FindBiomeOf(definition), "loot table and bestiary region");
                Assert.IsTrue(home.Enemies.Where(e => e.Enemy == definition).All(e => e.Weight == 0), $"{species.Id}: weight 0, night only");
            }
        }

        [Test]
        public void NightNumbers_FollowDocsDayNight()
        {
            NightLook wolf = NightVariantRules.Resolve(NightForm.Species, NightVariantRules.Lobisomem);
            Assert.AreEqual(5f, wolf.HealthMultiplier, 0.001f);
            Assert.AreEqual(0.6f, wolf.DamageBonus, 0.001f);
            Assert.AreEqual(1.8f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.MorcegoVampiro).HealthMultiplier, 0.001f);
            Assert.AreEqual(0.8f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.RasgaMortalha).HealthMultiplier, 0.001f);
            Assert.AreEqual(1.05f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.VagaLume).HealthMultiplier, 0.001f, "3 × 0.35");
            Assert.AreEqual(-0.6f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.VagaLume).DamageBonus, 0.001f, "× 0.4");
            Assert.AreEqual(1.3f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.Queixada).HealthMultiplier, 0.001f);
            Assert.AreEqual(0.6f, NightVariantRules.Resolve(NightForm.Species, NightVariantRules.MaeDaLua).HealthMultiplier, 0.001f);
            Assert.AreEqual(2, NightVariantRules.Queixada.PackMin);
            Assert.AreEqual(3, NightVariantRules.Queixada.PackMax);
            Assert.IsTrue(NightVariantRules.VagaLume.ComesAtDusk);
        }

        [Test]
        public void VisitingSpecies_IsScaledToTheRegion()
        {
            NightLook home = NightVariantRules.Resolve(NightForm.Species, NightVariantRules.RasgaMortalha, BiomeType.Forest);
            NightLook mountain = NightVariantRules.Resolve(NightForm.Species, NightVariantRules.RasgaMortalha, BiomeType.Mountain);
            Assert.AreEqual(NightVariantRules.Resolve(NightForm.Species, NightVariantRules.RasgaMortalha).HealthMultiplier, home.HealthMultiplier, 0.001f);
            // 70 HP owl ×0.8 at home; in the Mountain it is as tough relative to a 140 HP golem.
            Assert.AreEqual(0.8f * 140f / 70f, mountain.HealthMultiplier, 0.001f);
            Assert.AreEqual(1.2f * 13f / 7f, 1f + mountain.DamageBonus, 0.001f);
            Assert.AreEqual(1.5f * 28f / 10f, mountain.ExperienceMultiplier, 0.001f);

            List<PeriodSpawnPick> wave = PeriodSpawnTable.RollWave(BiomeType.Mountain, TimeOfDayPhase.Night, 4, new System.Random(3),
                id => id == NightVariantRules.ShroudOwlEnemyId ? Load(id) : null);
            foreach (PeriodSpawnPick pick in wave)
            {
                Assert.AreEqual(mountain.HealthMultiplier, pick.Look.HealthMultiplier, 0.001f);
            }
        }

        [Test]
        public void Tables_PlaceEverySpecies_WhereAsked()
        {
            bool Has(BiomeType region, TimeOfDayPhase phase, NightSpecies species) =>
                PeriodSpawnTable.GetEntries(region, phase).Any(e => e.SpeciesId == species.Id && e.EnemyId == species.BaseEnemyId);

            Assert.IsTrue(Has(BiomeType.Forest, TimeOfDayPhase.Night, NightVariantRules.Lobisomem));
            Assert.IsTrue(Has(BiomeType.Caves, TimeOfDayPhase.Night, NightVariantRules.MorcegoVampiro));
            Assert.IsTrue(Has(BiomeType.Forest, TimeOfDayPhase.Night, NightVariantRules.RasgaMortalha));
            Assert.IsTrue(Has(BiomeType.Mountain, TimeOfDayPhase.Night, NightVariantRules.RasgaMortalha));
            Assert.IsTrue(Has(BiomeType.Desert, TimeOfDayPhase.Night, NightVariantRules.VagaLume));
            Assert.IsTrue(Has(BiomeType.Desert, TimeOfDayPhase.Dusk, NightVariantRules.VagaLume));
            Assert.IsTrue(Has(BiomeType.Forest, TimeOfDayPhase.Night, NightVariantRules.VagaLume));
            Assert.IsTrue(Has(BiomeType.Forest, TimeOfDayPhase.Night, NightVariantRules.Queixada));
            Assert.IsTrue(Has(BiomeType.Mountain, TimeOfDayPhase.Night, NightVariantRules.MaeDaLua));

            // The old recolours are gone: no species entry points at a day enemy any more.
            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                foreach (PeriodSpawnEntry entry in PeriodSpawnTable.GetEntries(region, TimeOfDayPhase.Night).Where(e => e.Form == NightForm.Species))
                {
                    Assert.IsNotNull(NightVariantRules.FindByEnemyId(entry.EnemyId), entry.EnemyId);
                }
            }
        }

        [Test]
        public void Queixadas_ComeAsAHerd_NeverOverTheWaveCap()
        {
            int herds = 0;
            for (int seed = 0; seed < 400; seed++)
            {
                foreach (int count in new[] { 1, 2, 3, 4 })
                {
                    List<PeriodSpawnPick> wave = PeriodSpawnTable.RollWave(BiomeType.Forest, TimeOfDayPhase.Night, count, new System.Random(seed));
                    Assert.LessOrEqual(wave.Count, NightDangerRules.MaxWaveSize);
                    int pigs = wave.Count(p => p.Species == NightVariantRules.Queixada);
                    Assert.IsTrue(pigs == 0 || (pigs >= 2 && pigs <= 3), $"seed {seed}, count {count}: {pigs} pigs");
                    Assert.LessOrEqual(wave.Count(p => p.Species == NightVariantRules.Lobisomem), 1);
                    herds += pigs > 0 ? 1 : 0;
                    if (pigs == 0)
                    {
                        Assert.LessOrEqual(wave.Count, count, "only a herd may stretch a wave");
                    }
                }
            }

            Assert.Greater(herds, 50, "herds do show up");
        }

        // ------------------------------------------------------------------
        // Bestiary, names, achievement
        // ------------------------------------------------------------------

        [Test]
        public void EverySpecies_HasNames_AndABestiaryPage()
        {
            var bestiary = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (NightSpecies species in NightVariantRules.PlayableSpecies)
            {
                Assert.IsNotNull(NightTexts.GetFallback(species.NameKey), species.Id);
                Assert.IsTrue(bestiary.ContainsKey(BestiaryCatalog.LoreKey(species.BaseEnemyId)), species.BaseEnemyId);
                foreach (var hero in BestiaryTexts.Cousins)
                {
                    var entry = bestiary[BestiaryCatalog.CommentKey(species.BaseEnemyId, hero)];
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En) || string.IsNullOrWhiteSpace(entry.PtBR), $"{species.BaseEnemyId} {hero}");
                }

                Assert.IsTrue(Rapadura.Gameplay.Content.ContentTexts.Entries.Any(e => e.Key == $"enemy.{species.BaseEnemyId}.name"), species.BaseEnemyId);
            }
        }

        [Test]
        public void DefeatingTheLobisomem_HasAnAchievement()
        {
            AchievementDefinition achievement = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder)
                .FirstOrDefault(a => a.AchievementId == "ach_night_werewolf");
            Assert.IsNotNull(achievement);
            Assert.AreEqual(AchievementConditionType.EnemiesKilled, achievement.ConditionType);
            CollectionAssert.AreEqual(new[] { NightVariantRules.WerewolfEnemyId }, achievement.TargetIds.ToArray());
            Assert.AreEqual(1, achievement.TargetValue);
            Assert.IsTrue(AchievementTexts.All.Any(e => e.Key == achievement.TitleKey));
            Assert.IsTrue(AchievementTexts.All.Any(e => e.Key == achievement.DescriptionKey));
        }
    }
}
