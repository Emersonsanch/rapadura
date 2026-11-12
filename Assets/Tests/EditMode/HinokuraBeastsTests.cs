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
    /// The beasts of the Arquipélago de Hinokura (Docs/Mapas100.md 65–70) that the overworld layout played with stand-ins:
    /// Caranguejo-Vulcão, Lagarto de Lava, Gaivota-Brasa (65), Shisa Apagada, Tanuki Ladrão (66), Tibicena, Lagarto-Gigante de Lava
    /// (67), Peixe-Lâmina, Coral de Brasa (68), Ushabti de Laca, Ogro-Fornalha (69) and Gigante de Escória (70), all in the Mountain
    /// biome like the region's <c>OverworldLayouts.Hinokura</c>. Same contract as <see cref="MarGeladoBeastsTests"/>: each
    /// definition loads, is a weight-0 common of its biome with the region tier's numbers and no pinned level (it takes the
    /// area's), has its bestiary page and name, and its own procedural model with the motion its rig adds. The ember gull and
    /// the ember coral are the sheets' [distância] ones (<see cref="RangedEnemiesTests"/>); everyone else swings.
    /// </summary>
    public class HinokuraBeastsTests
    {
        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_mountain_volcano_crab", BiomeType.Mountain, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.VolcanoCrab, 220, 227, "enemy_forest_coconut_crab"),
            new OverworldBeastsTests.Beast("enemy_mountain_lava_lizard", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.LavaLizard, 220, 227, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast("enemy_mountain_ember_gull", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.EmberGull, 220, 227, "enemy_desert_ember_vulture"),
            new OverworldBeastsTests.Beast("enemy_mountain_dimmed_shisa", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.DimmedShisa, 226, 235, "enemy_forest_stone_lion"),
            new OverworldBeastsTests.Beast("enemy_mountain_tanuki", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Tanuki, 226, 234, "enemy_forest_thief_otter"),
            new OverworldBeastsTests.Beast("enemy_mountain_tibicena", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Tibicena, 234, 242, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_mountain_giant_lava_lizard", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.GiantLavaLizard, 236, 246, "enemy_forest_mangrove_crocodile"),
            new OverworldBeastsTests.Beast("enemy_mountain_blade_fish", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BladeFish, 244, 254, "enemy_mountain_giant_pike"),
            new OverworldBeastsTests.Beast("enemy_mountain_ember_coral", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.EmberCoral, 244, 250, "enemy_desert_broken_lantern"),
            new OverworldBeastsTests.Beast("enemy_mountain_lacquer_ushabti", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.LacquerUshabti, 252, 260, "enemy_forest_terracotta_soldier"),
            new OverworldBeastsTests.Beast("enemy_mountain_furnace_ogre", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.FurnaceOgre, 252, 260, "enemy_forest_dimmed_troll"),
            new OverworldBeastsTests.Beast("enemy_mountain_slag_giant", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SlagGiant, 264, 280, "enemy_mountain_stone_golem"),
        };

        /// <summary>The sheets' [distância] ones: they spit embers like the ember vulture (same shot and sound).</summary>
        private static readonly string[] Shooters = { "enemy_mountain_ember_gull", "enemy_mountain_ember_coral" };

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

        /// <summary>The commons that were in <paramref name="biome"/> before the six beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(MingchengNartesBestiaryTexts.EnemyIds).Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds));
            BiomeDefinition home = BiomeRegistry.Get(biome);
            Assert.IsNotNull(home, biome.ToString());
            return home.Enemies.Where(e => e?.Enemy != null && !e.Enemy.IsBoss && !newer.Contains(e.Enemy.name)).Select(e => e.Enemy).ToList();
        }

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
            StringAssert.StartsWith("enemy_mountain_", beast.Id, "named after its home biome (enemy_<biome>_, BalanceInvariantsTests)");
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
        public void Stats_FollowTheRegionTier_AndScaleIntoTheAreasLevelBand([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            List<EnemyDefinition> commons = OlderCommonsOf(beast.Biome);
            Assert.IsNotEmpty(commons, beast.Biome.ToString());

            Assert.IsTrue(commons.Any(c => c.ExperienceReward == definition.ExperienceReward), $"{beast.Id}: XP of its biome's commons");
            float hp = commons.Average(c => c.MaxHealth);
            float dps = commons.Average(c => c.AttackDamage / c.AttackCooldown);
            Assert.That(definition.MaxHealth / hp, Is.InRange(0.6f, 1.6f), $"{beast.Id}: health against its biome's commons");
            Assert.That(definition.AttackDamage / definition.AttackCooldown / dps, Is.InRange(0.5f, 2f), $"{beast.Id}: damage per second against its biome's commons");

            Assert.AreEqual(0, definition.Level, $"{beast.Id}: takes the area's level");
            Assert.AreEqual(beast.MinLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MinLevel), beast.Id);
            Assert.AreEqual(beast.MaxLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MaxLevel), beast.Id);
            float low = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MinLevel);
            float high = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MaxLevel);
            Assert.GreaterOrEqual(high, low, beast.Id);
            Assert.GreaterOrEqual(EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MaxLevel), EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MinLevel), beast.Id);

            // Swapping it for the stand-in keeps the fight about as long at the same level.
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.That(high / EnemyLevelScaling.HealthFor(standIn.MaxHealth, beast.MaxLevel), Is.InRange(0.5f, 2.5f), $"{beast.Id} vs {beast.StandIn}");
        }

        [Test]
        public void TheShooters_SpitEmbers_FromRange_AndTheRestSwing([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            bool shoots = Shooters.Contains(beast.Id);
            Assert.AreEqual(shoots, definition.IsRanged, beast.Id);
            if (shoots)
            {
                Assert.AreEqual(EnemyProjectileKind.Ember, definition.Projectile, beast.Id);
                Assert.AreEqual(EnemyMotionArchetype.Flyer, definition.MotionArchetype, $"{beast.Id}: hovers like the other shooters");
                Assert.That(definition.AttackRange, Is.InRange(6f, 10f), $"{beast.Id}: readable on the isometric camera");
                Assert.Greater(definition.LoseTargetRadius, definition.AttackRange, $"{beast.Id}: does not lose the hero while shooting");
                Assert.AreEqual(EnemyStrikeKind.Projectile, EnemyRangedAttack.StrikeKindOf(definition.IsRanged, false), beast.Id);
            }
            else
            {
                Assert.AreEqual(EnemyProjectileKind.None, definition.Projectile, beast.Id);
                Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
            }
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
            Assert.AreEqual(Shooters.Contains(beast.Id), page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: the [distância] tip");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(HinokuraBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), HinokuraBestiaryTexts.EnemyIds);
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreEqual(beast.Model, ProceduralEnemyModels.KindFor(beast.Id), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(beast.Model), $"{beast.Id}: a day enemy");
            Assert.IsFalse(ProceduralEnemyModels.IsOssuaryGuardian(beast.Model), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(beast.Model), beast.Id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, beast.Model);

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

        [TestCase(ProceduralEnemyKind.LavaLizard)]
        [TestCase(ProceduralEnemyKind.EmberGull)]
        [TestCase(ProceduralEnemyKind.DimmedShisa)]
        [TestCase(ProceduralEnemyKind.Tanuki)]
        [TestCase(ProceduralEnemyKind.Tibicena)]
        [TestCase(ProceduralEnemyKind.GiantLavaLizard)]
        [TestCase(ProceduralEnemyKind.BladeFish)]
        [TestCase(ProceduralEnemyKind.EmberCoral)]
        public void Biters_OpenTheirJawOrBeak_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Assert.AreEqual(ProceduralEnemyParts.Head, jaw.parent.name, $"{kind}: the lower jaw/beak hangs from the head");
            Quaternion rest = jaw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: opens before the strike");
        }

        [Test]
        public void VolcanoCrab_OpensItsPincers_OnTheWindup()
        {
            Make(ProceduralEnemyKind.VolcanoCrab, out ProceduralEnemyRig rig);
            Transform claw = rig.GetPart(ProceduralEnemyParts.ClawRight);
            Assert.IsNotNull(claw);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.ClawLeft));
            Quaternion rest = claw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, claw.localRotation), 20f, "the pincers open before the pinch");
        }

        [TestCase(ProceduralEnemyKind.LacquerUshabti)]
        [TestCase(ProceduralEnemyKind.FurnaceOgre)]
        [TestCase(ProceduralEnemyKind.SlagGiant)]
        public void TheBipeds_RaiseBothArms_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion leftRest = left.localRotation;
            Quaternion rightRest = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(leftRest, left.localRotation), 30f, $"{kind}: left arm");
            Assert.Greater(Quaternion.Angle(rightRest, right.localRotation), 30f, $"{kind}: right arm");
        }

        [TestCase(ProceduralEnemyKind.BladeFish)]
        [TestCase(ProceduralEnemyKind.EmberCoral)]
        public void TheSwimmers_MoveTheirFins_AndTail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform fin = rig.GetPart(ProceduralEnemyParts.WingRight);
            Transform tail = rig.GetPart(ProceduralEnemyParts.Tail);
            Assert.IsNotNull(fin, kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            Assert.IsNotNull(tail, kind.ToString());
            Quaternion finRest = fin.localRotation;
            Quaternion tailRest = tail.localRotation;
            bool finMoved = false;
            bool tailMoved = false;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.07f);
                finMoved |= Quaternion.Angle(finRest, fin.localRotation) > 1f;
                tailMoved |= Quaternion.Angle(tailRest, tail.localRotation) > 1f;
            }

            Assert.IsTrue(finMoved, $"{kind}: fins/fronds stir");
            Assert.IsTrue(tailMoved, $"{kind}: tail/root sweeps");
        }

        [Test]
        public void TheShooters_HaveNoLanternPart()
        {
            // RangedEnemiesTests: the light a shooter fires comes from a Lantern part; these spit embers.
            foreach (ProceduralEnemyKind kind in new[] { ProceduralEnemyKind.EmberGull, ProceduralEnemyKind.EmberCoral })
            {
                Make(kind, out ProceduralEnemyRig rig);
                Assert.IsNull(rig.GetPart(ProceduralEnemyParts.Lantern), kind.ToString());
                Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            }
        }
    }
}
