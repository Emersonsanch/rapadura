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
    /// The beasts of the Margens da Mistura (Docs/Mapas100.md 77–82) that the overworld layout still plays with stand-ins: Leão de Tijolo
    /// Apagado, Mušhuššu Jovem, Gafanhoto de Betume (77), Ampulheta Viva, Sombra Atrasada (78), Betume Vivo, Garça de Piche (79), Cedro
    /// Raivoso, Manto de Brilho (80), Filho de Ullikummi (81), Tábua Quebrada and Jardim-Sombra (82), all in the Forest biome like the
    /// region's <c>OverworldLayouts.Mistura</c>. Same contract as <see cref="RioBeastsTests"/>: each definition loads, is a weight-0 common
    /// of its biome with the region tier's numbers and no pinned level (it takes the area's), has its bestiary page and name, and its own
    /// procedural model with the motion its rig adds. The heron and the tablet are the region's [distância] shooters.
    /// </summary>
    public class MisturaBeastsTests
    {
        private const string Heron = "enemy_forest_tar_heron";
        private const string Tablet = "enemy_forest_broken_tablet";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_brick_lion", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.BrickLion, 300, 307, "enemy_forest_stone_lion"),
            new OverworldBeastsTests.Beast("enemy_forest_young_mushussu", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.YoungMushussu, 302, 306, "enemy_mountain_young_griffin"),
            new OverworldBeastsTests.Beast("enemy_forest_bitumen_locust", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BitumenLocust, 300, 307, "enemy_mountain_bark_beetle"),
            new OverworldBeastsTests.Beast("enemy_forest_living_hourglass", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.LivingHourglass, 306, 311, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_forest_late_shadow", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.LateShadow, 306, 313, "enemy_mountain_ill_wind_mist"),
            new OverworldBeastsTests.Beast("enemy_forest_living_tar", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.LivingTar, 314, 326, "enemy_cave_ooze"),
            new OverworldBeastsTests.Beast(Heron, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.TarHeron, 316, 326, "enemy_mountain_ember_gull"),
            new OverworldBeastsTests.Beast("enemy_forest_angry_cedar", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.AngryCedar, 322, 336, "enemy_forest_dimmed_troll"),
            new OverworldBeastsTests.Beast("enemy_forest_glow_mantle", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.GlowMantle, 322, 336, "enemy_desert_broken_lantern"),
            new OverworldBeastsTests.Beast("enemy_forest_ullikummi_son", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.UllikummiSon, 340, 346, "enemy_mountain_slag_giant"),
            new OverworldBeastsTests.Beast(Tablet, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BrokenTablet, 344, 360, "enemy_forest_terracotta_soldier"),
            new OverworldBeastsTests.Beast("enemy_forest_garden_shadow", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.GardenShadow, 346, 360, "enemy_mountain_sleepwalker_giant"),
        };

        private static readonly Dictionary<string, EnemyProjectileKind> Shooters = new Dictionary<string, EnemyProjectileKind>
        {
            { Heron, EnemyProjectileKind.LureGlob },
            { Tablet, EnemyProjectileKind.LanternBolt },
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

        /// <summary>The commons that were in <paramref name="biome"/> before the beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(MingchengNartesBestiaryTexts.EnemyIds).Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds)
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds));
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
            StringAssert.StartsWith("enemy_forest_", beast.Id, "named after its home biome (enemy_<biome>_, BalanceInvariantsTests)");
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
        public void TheShooters_ThrowFromRange_AndTheRestSwing([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            bool shoots = Shooters.TryGetValue(beast.Id, out EnemyProjectileKind projectile);
            Assert.AreEqual(shoots, definition.IsRanged, beast.Id);
            if (shoots)
            {
                Assert.AreEqual(projectile, definition.Projectile, beast.Id);
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
            Assert.AreEqual(Shooters.ContainsKey(beast.Id), page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: the [distância] tip only on the shooters");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(MisturaBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), MisturaBestiaryTexts.EnemyIds);
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

        [TestCase(ProceduralEnemyKind.BrickLion)]
        [TestCase(ProceduralEnemyKind.YoungMushussu)]
        [TestCase(ProceduralEnemyKind.BitumenLocust)]
        [TestCase(ProceduralEnemyKind.LateShadow)]
        [TestCase(ProceduralEnemyKind.TarHeron)]
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

        [TestCase(ProceduralEnemyKind.LivingHourglass)]
        [TestCase(ProceduralEnemyKind.AngryCedar)]
        [TestCase(ProceduralEnemyKind.UllikummiSon)]
        [TestCase(ProceduralEnemyKind.GardenShadow)]
        public void TheWalkers_RaiseBothArms_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Assert.IsNotNull(left, kind.ToString());
            Assert.IsNotNull(right, kind.ToString());
            Quaternion leftRest = left.localRotation;
            Quaternion rightRest = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(leftRest, left.localRotation), 30f, $"{kind}: left arm");
            Assert.Greater(Quaternion.Angle(rightRest, right.localRotation), 30f, $"{kind}: right arm");
        }

        [TestCase(ProceduralEnemyKind.LateShadow)]
        [TestCase(ProceduralEnemyKind.GlowMantle)]
        public void TheDrifters_MoveTheirSleeves_AndTail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform wing = rig.GetPart(ProceduralEnemyParts.WingRight);
            Assert.IsNotNull(wing, kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Tail), kind.ToString());
            Quaternion rest = wing.localRotation;
            bool moved = false;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.07f);
                moved |= Quaternion.Angle(rest, wing.localRotation) > 1f;
            }

            Assert.IsTrue(moved, $"{kind}: the sleeves stir");
        }

        [Test]
        public void LivingTar_FlattensOnTheWindup_AndTheEmbersFlare()
        {
            Make(ProceduralEnemyKind.LivingTar, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform light = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(light);
            Vector3 bodyRest = body.localScale;
            Vector3 lightRest = light.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, "flattens before it strikes");
            Assert.Greater(light.localScale.x, lightRest.x, "the embers flare");
        }

        [Test]
        public void BrokenTablet_LeansBack_AndItsLineFlares_OnTheWindup()
        {
            Make(ProceduralEnemyKind.BrokenTablet, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform light = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(light);
            Quaternion rest = body.localRotation;
            Vector3 lightRest = light.localScale;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, body.localRotation), 10f, "leans back before it throws");
            Assert.Greater(light.localScale.x, lightRest.x, "the broken line flares");
        }
    }
}
