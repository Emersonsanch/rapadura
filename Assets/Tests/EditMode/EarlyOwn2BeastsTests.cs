using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-sixth pass of beasts (bible §3.2.1): the roles of Regions 2 and 3 (Docs/Mapas100.md 09, 12, 15 and 17) that the continuous world
    /// still played with another beast after the twenty-third pass (<see cref="EarlyOwnBeastsTests"/>). Same contract: each one keeps its
    /// stand-in's biome (the prefix) and numbers, so the economy of the region does not move, and shoots exactly when its stand-in did (the black
    /// eagle throws the crested hawk's feathers). The Águia da Tempestade of 18 is the Farnkesh's own beast (<see cref="FarnkeshBeastsTests"/>).
    /// </summary>
    public class EarlyOwn2BeastsTests
    {
        private const string Scorpion = "enemy_desert_scorpion";
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Bats = "enemy_cave_bat_swarm";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_desert_thirsty_root", BiomeType.Desert, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.ThirstyRoot, 20, 26, Scorpion),
            new OverworldBeastsTests.Beast("enemy_desert_crystal_scorpion", BiomeType.Desert, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.CrystalScorpion, 30, 38, Scorpion),
            new OverworldBeastsTests.Beast("enemy_desert_salt_serpent", BiomeType.Desert, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.SaltSerpent, 30, 38, Scorpion),
            new OverworldBeastsTests.Beast("enemy_mountain_dimmed_black_eagle", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DimmedBlackEagle, 18, 32, Hawk),
            new OverworldBeastsTests.Beast("enemy_cave_cold_salamander", BiomeType.Caves, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ColdSalamander, 30, 38, Bats),
        };

        private static IEnumerable<OverworldBeastsTests.Beast> All() => Beasts;

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
            BestiaryCatalog.Invalidate();
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, $"{id}: not in Resources/Enemies");
            return definition;
        }

        private static string PrefixOf(BiomeType biome) =>
            biome == BiomeType.Mountain ? "enemy_mountain_" : biome == BiomeType.Caves ? "enemy_cave_" : biome == BiomeType.Desert ? "enemy_desert_" : "enemy_forest_";

        private GameObject Make(ProceduralEnemyKind kind, out ProceduralEnemyRig rig)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            rig = visual.GetComponent<ProceduralEnemyRig>();
            return visual;
        }

        private static void Windup(ProceduralEnemyRig rig)
        {
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }
        }

        [Test]
        public void Definitions_Load_AreCommons_WithTheSheetsArchetype([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            Assert.IsFalse(definition.IsBoss, beast.Id);
            Assert.AreEqual(beast.Archetype, definition.MotionArchetype, beast.Id);
            Assert.Greater(definition.MaxHealth, 0f, beast.Id);
            Assert.Greater(definition.AttackDamage, 0f, beast.Id);
            Assert.Greater(definition.AttackCooldown, 0f, beast.Id);
            Assert.Greater(definition.ExperienceReward, 0, beast.Id);
            Assert.Greater(definition.LoseTargetRadius, definition.DetectionRadius, $"{beast.Id}: keeps chasing a little past where it noticed the hero");
            Assert.GreaterOrEqual(definition.DetectionRadius, definition.AttackRange, beast.Id);
            Assert.IsNotNull(definition.LootItem, $"{beast.Id}: its base drop (the biome table adds the rest)");
            StringAssert.StartsWith(PrefixOf(beast.Biome), beast.Id, "named after its home biome (enemy_<biome>_, BalanceInvariantsTests)");
        }

        [Test]
        public void EachOne_LivesInItsBiome_AtWeightZero_LikeTheOtherPlacedCommons([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            BiomeRegistry.Invalidate();
            EnemyDefinition definition = Load(beast.Id);
            BiomeDefinition home = BiomeRegistry.FindBiomeOf(definition);
            Assert.IsNotNull(home, $"{beast.Id}: in a biome table (loot + bestiary region)");
            Assert.AreEqual(beast.Biome, home.Type, beast.Id);
            Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == definition).Weight, $"{beast.Id}: weight 0, never rolled by the world-map encounters");
        }

        [Test]
        public void Stats_TakeTheAreasLevel_AndScaleIntoTheBand([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            Assert.AreEqual(0, definition.Level, $"{beast.Id}: takes the area's level");
            Assert.AreEqual(beast.MinLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MinLevel), beast.Id);
            Assert.AreEqual(beast.MaxLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MaxLevel), beast.Id);
            Assert.GreaterOrEqual(EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MaxLevel), EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MinLevel), beast.Id);
            Assert.GreaterOrEqual(EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MaxLevel), EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MinLevel), beast.Id);
        }

        [Test]
        public void EachOne_ShootsExactly_WhenItsStandInDid([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.AreEqual(standIn.IsRanged, definition.IsRanged, $"{beast.Id}: strikes like {beast.StandIn}");
            Assert.AreEqual(standIn.Projectile, definition.Projectile, $"{beast.Id}: the shot of {beast.StandIn}");
            if (definition.IsRanged)
            {
                Assert.AreEqual(EnemyMotionArchetype.Flyer, definition.MotionArchetype, $"{beast.Id}: hovers like the other shooters");
                Assert.That(definition.AttackRange, Is.InRange(6f, 10f), $"{beast.Id}: readable on the isometric camera");
                Assert.Greater(definition.LoseTargetRadius, definition.AttackRange, $"{beast.Id}: does not lose the hero while shooting");
            }
            else
            {
                Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
            }
        }

        [Test]
        public void EachOne_KeepsItsStandInsNumbers_SoTheEconomyDoesNotMove([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.AreEqual(standIn.MaxHealth, definition.MaxHealth, beast.Id);
            Assert.AreEqual(standIn.ExperienceReward, definition.ExperienceReward, beast.Id);
            Assert.AreEqual(standIn.AttackDamage, definition.AttackDamage, beast.Id);
            Assert.AreEqual(standIn.AttackCooldown, definition.AttackCooldown, beast.Id);
            Assert.AreEqual(standIn.AttackRange, definition.AttackRange, beast.Id);
            Assert.AreEqual(standIn.LootItem, definition.LootItem, beast.Id);
            Assert.AreEqual(standIn.LootDropChance, definition.LootDropChance, beast.Id);
            BiomeRegistry.Invalidate();
            Assert.AreEqual(BiomeRegistry.FindBiomeOf(standIn).Type, BiomeRegistry.FindBiomeOf(definition).Type, $"{beast.Id}: the stand-in's biome");
        }

        [Test]
        public void EachOne_HasABestiaryPage_WithLoreAndFiveCousins_AndAName([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            BestiaryCatalog.Invalidate();
            BestiaryEntry page = BestiaryCatalog.Get(beast.Id);
            Assert.IsNotNull(page, $"{beast.Id}: bestiary page");
            Assert.AreEqual(beast.Biome, page.Region, beast.Id);
            Assert.IsFalse(page.IsDungeon, beast.Id);
            Assert.IsFalse(page.IsBoss, beast.Id);
            Assert.IsNotEmpty(page.Drops, $"{beast.Id}: drops from its biome's loot table");
            Assert.IsNotEmpty(page.Tips, beast.Id);
            Assert.AreEqual(Load(beast.Id).IsRanged, page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: a [distância] tip only for the shooter");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(EarlyOwn2BestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts_AtMostTwelve()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), EarlyOwn2BestiaryTexts.EnemyIds);
            Assert.LessOrEqual(Beasts.Length, 12, "a pass of beasts is at most twelve");
        }

        [Test]
        public void TheNames_AreNotTakenByAnotherBeast()
        {
            var mine = new HashSet<string>(Beasts.Select(b => b.Id));
            var names = new HashSet<string>(Beasts.Select(b => Load(b.Id).EnemyName));
            foreach (EnemyDefinition other in Resources.LoadAll<EnemyDefinition>(EncounterPayload.EnemyResourcesFolder))
            {
                if (!mine.Contains(other.name))
                {
                    Assert.IsFalse(names.Contains(other.EnemyName), $"{other.name} is also called {other.EnemyName}");
                }
            }
        }

        [Test]
        public void TheStormEagleOf18_IsTheFarnkeshsOwn_NotANewBeast()
        {
            EnemyDefinition eagle = Load(Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.EarlyOwn2Beasts.StormEagle);
            Assert.AreEqual("Águia da Tempestade", eagle.EnemyName, "the name of the sheet 18");
            Assert.AreEqual(Load(Hawk).IsRanged, eagle.IsRanged, "it throws like the crested hawk it replaces");
            Assert.AreEqual(ProceduralEnemyKind.StormEagle, ProceduralEnemyModels.KindFor(eagle.name));
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreEqual(beast.Model, ProceduralEnemyModels.KindFor(beast.Id), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(beast.Model), $"{beast.Id}: a day enemy");
            Assert.IsFalse(ProceduralEnemyModels.IsOssuaryGuardian(beast.Model), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(beast.Model), beast.Id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, beast.Model);
            Assert.That((int)beast.Model, Is.InRange(560, 569), $"{beast.Id}: the kinds reserved for this pass");

            GameObject visual = Make(beast.Model, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig, beast.Id);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Body), $"{beast.Id}: Body");
            Assert.Greater(visual.GetComponentsInChildren<MeshFilter>().Length, 0, beast.Id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f), beast.Id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f, 0f), $"{beast.Id}: Reduzir Movimento");
        }

        [Test]
        public void TheModels_AreNotTheStandInsModels([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreNotEqual(ProceduralEnemyModels.KindFor(beast.StandIn), beast.Model, $"{beast.Id}: its own body, not {beast.StandIn}'s");
        }

        [TestCase(ProceduralEnemyKind.CrystalScorpion)]
        [TestCase(ProceduralEnemyKind.SaltSerpent)]
        [TestCase(ProceduralEnemyKind.DimmedBlackEagle)]
        [TestCase(ProceduralEnemyKind.ColdSalamander)]
        public void Biters_OpenTheirJawOrBeak_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Assert.AreEqual(ProceduralEnemyParts.Head, jaw.parent.name, $"{kind}: the lower jaw hangs from the head");
            Quaternion rest = jaw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: opens before the strike");
        }

        [TestCase(ProceduralEnemyKind.DimmedBlackEagle)]
        public void TheEagle_HasTwoWings_AndAFanTail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingRight), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Tail), kind.ToString());
        }

        [TestCase(ProceduralEnemyKind.ThirstyRoot)]
        public void TheSquashers_FlattenOnTheWindup_AndTheirLightFlares(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform light = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(light, kind.ToString());
            Vector3 bodyRest = body.localScale;
            Vector3 lightRest = light.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, $"{kind}: flattens before it strikes");
            Assert.Greater(light.localScale.x, lightRest.x, $"{kind}: the light flares");
        }
    }
}
