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
    /// The twenty-second pass of beasts (bible §3.2.1): the roles of the Rio das Doze Horas (Região 13, Docs/Mapas100.md 74–76), the Margens
    /// da Mistura (Região 14, 78–79) and the Limiar Profundo (Região 15, 84–90) that the overworld still played with another region's beast.
    /// Same contract as <see cref="HighlandOwnBeastsTests"/>: each one keeps its stand-in's biome (the prefix) and numbers, so the economy of
    /// the region does not move. The Sombra Fervida spits a glob of hot shadow and the Ushabti de Relógio throws its dial's sunlight (their sheets' [distância]
    /// attacks; they hover like every other shooter); the Cardume Escuro bites in a pack instead ([enxame]: melee, five to a group).
    /// </summary>
    public class DeepOwnBeastsTests
    {
        private const string DarkShoal = "enemy_cave_dark_shoal";
        private const string BoiledShadow = "enemy_cave_boiled_shadow";
        private const string ClockUshabti = "enemy_mountain_clock_ushabti";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_desert_coil_scorpion", BiomeType.Desert, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.CoilScorpion, 284, 296, "enemy_desert_scorpion"),
            new OverworldBeastsTests.Beast("enemy_mountain_dimmed_ushabti", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.DimmedUshabti, 284, 296, "enemy_mountain_rope_ushabti"),
            new OverworldBeastsTests.Beast("enemy_desert_shadow_salamander", BiomeType.Desert, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ShadowSalamander, 292, 306, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast(BoiledShadow, BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BoiledShadow, 292, 306, "enemy_cave_ooze"),
            new OverworldBeastsTests.Beast("enemy_forest_cracked_ushabti", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.CrackedUshabti, 292, 306, "enemy_forest_terracotta_soldier"),
            new OverworldBeastsTests.Beast("enemy_forest_mud_crocodile", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MudCrocodile, 304, 320, "enemy_forest_mangrove_crocodile"),
            new OverworldBeastsTests.Beast(ClockUshabti, BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ClockUshabti, 306, 316, "enemy_mountain_lacquer_ushabti"),
            new OverworldBeastsTests.Beast("enemy_mountain_pitch_smoke", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.PitchSmoke, 314, 326, "enemy_mountain_smoke_harpy"),
            new OverworldBeastsTests.Beast("enemy_forest_choking_smoke", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ChokingSmoke, 348, 362, "enemy_forest_smoke_goat"),
            new OverworldBeastsTests.Beast(DarkShoal, BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DarkShoal, 386, 402, "enemy_cave_lantern_fish"),
            new OverworldBeastsTests.Beast("enemy_forest_blind_serpent", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.BlindSerpent, 400, 416, "enemy_forest_dimmed_python"),
            new OverworldBeastsTests.Beast("enemy_mountain_giant_dimmed_ushabti", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.GiantDimmedUshabti, 424, 440, "enemy_mountain_stone_golem"),
        };

        private static readonly Dictionary<string, EnemyProjectileKind> Shooters = new Dictionary<string, EnemyProjectileKind>
        {
            { BoiledShadow, EnemyProjectileKind.LureGlob },
            { ClockUshabti, EnemyProjectileKind.LanternBolt },
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
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds)
                .Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds)
                .Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds).Concat(LentRoleBestiaryTexts.EnemyIds).Concat(SkyOwnBestiaryTexts.EnemyIds).Concat(CoastOwnBestiaryTexts.EnemyIds).Concat(HighlandOwnBestiaryTexts.EnemyIds)
                .Concat(NorthOwnBestiaryTexts.EnemyIds));
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
            string prefix = beast.Biome == BiomeType.Mountain ? "enemy_mountain_" : beast.Biome == BiomeType.Desert ? "enemy_desert_" : beast.Biome == BiomeType.Caves ? "enemy_cave_" : "enemy_forest_";
            StringAssert.StartsWith(prefix, beast.Id, "named after its home biome (enemy_<biome>_, BalanceInvariantsTests)");
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
                Assert.AreEqual(EnemyProjectileKind.None, Load(beast.StandIn).Projectile, $"{beast.Id}: the shot is its sheet's own, the stand-in only swung");
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
        public void EachOne_KeepsItsStandInsNumbers_SoTheEconomyDoesNotMove([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.AreEqual(standIn.MaxHealth, definition.MaxHealth, beast.Id);
            Assert.AreEqual(standIn.ExperienceReward, definition.ExperienceReward, beast.Id);
            Assert.AreEqual(standIn.AttackDamage, definition.AttackDamage, beast.Id);
            Assert.AreEqual(standIn.AttackCooldown, definition.AttackCooldown, beast.Id);
            Assert.AreEqual(standIn.LootItem, definition.LootItem, beast.Id);
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
            Assert.AreEqual(Shooters.ContainsKey(beast.Id), page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: the [distância] tip only for the shooters");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(DeepOwnBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), DeepOwnBestiaryTexts.EnemyIds);
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreEqual(beast.Model, ProceduralEnemyModels.KindFor(beast.Id), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(beast.Model), $"{beast.Id}: a day enemy");
            Assert.IsFalse(ProceduralEnemyModels.IsOssuaryGuardian(beast.Model), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(beast.Model), beast.Id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, beast.Model);
            Assert.That((int)beast.Model, Is.InRange(470, 481), $"{beast.Id}: the kinds reserved for this pass");

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

        [Test]
        public void TheDarkShoal_BitesInAPack_InsteadOfShooting()
        {
            EnemyDefinition shoal = Load(DarkShoal);
            Assert.IsFalse(shoal.IsRanged, "[enxame]: it bites, it no longer spits the lantern fish's glob");
            Assert.AreEqual(EnemyStrikeKind.Swing, EnemyRangedAttack.StrikeKindOf(shoal.IsRanged, false));
            Assert.IsTrue(Load("enemy_cave_lantern_fish").IsRanged, "the stand-in it replaced did shoot");
            BestiaryCatalog.Invalidate();
            Assert.IsFalse(BestiaryCatalog.Get(DarkShoal).Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), "no [distância] tip");
        }

        [TestCase(ProceduralEnemyKind.ShadowSalamander)]
        [TestCase(ProceduralEnemyKind.MudCrocodile)]
        [TestCase(ProceduralEnemyKind.BlindSerpent)]
        public void Biters_OpenTheirJaw_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Assert.AreEqual(ProceduralEnemyParts.Head, jaw.parent.name, $"{kind}: the lower jaw hangs from the head");
            Quaternion rest = jaw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: opens before the strike");
        }

        [Test]
        public void TheBlindSerpent_HasTheWholeTailChain_AndTheSlitherOwnsIt()
        {
            Make(ProceduralEnemyKind.BlindSerpent, out ProceduralEnemyRig rig);
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.TailSegmentPrefix + i), $"Tail_{i}");
            }

            Assert.IsNotNull(ProceduralEnemyRig.PartsAnimatedBy(ProceduralEnemyKind.BlindSerpent), "the slither drives the tail, not the stepping legs");
        }

        [Test]
        public void TheCoilScorpion_OpensItsClaws_AndCocksItsSting_OnTheWindup()
        {
            Make(ProceduralEnemyKind.CoilScorpion, out ProceduralEnemyRig rig);
            Transform claw = rig.GetPart(ProceduralEnemyParts.ClawLeft);
            Transform tail = rig.GetPart(ProceduralEnemyParts.Tail);
            Quaternion clawRest = claw.localRotation;
            Quaternion tailRest = tail.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(clawRest, claw.localRotation), 20f, "the claws open");
            Assert.Greater(Quaternion.Angle(tailRest, tail.localRotation), 20f, "the sting draws back");
        }

        [TestCase(ProceduralEnemyKind.BoiledShadow)]
        [TestCase(ProceduralEnemyKind.PitchSmoke)]
        [TestCase(ProceduralEnemyKind.ChokingSmoke)]
        [TestCase(ProceduralEnemyKind.DarkShoal)]
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

        [TestCase(ProceduralEnemyKind.DimmedUshabti)]
        [TestCase(ProceduralEnemyKind.CrackedUshabti)]
        [TestCase(ProceduralEnemyKind.ClockUshabti)]
        [TestCase(ProceduralEnemyKind.GiantDimmedUshabti)]
        public void TheFigures_RaiseBothArms_OnTheWindup(ProceduralEnemyKind kind)
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

        [Test]
        public void TheGatesFigure_TowersOverTheBargesFigure_ButIsNoBoss()
        {
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(ProceduralEnemyKind.GiantDimmedUshabti));
            Assert.Greater(Height(ProceduralEnemyKind.GiantDimmedUshabti), Height(ProceduralEnemyKind.DimmedUshabti) * 1.4f, "the Gate's figure towers over the barge's");
        }

        private float Height(ProceduralEnemyKind kind)
        {
            GameObject visual = Make(kind, out _);
            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds), kind.ToString());
            return bounds.size.y;
        }
    }
}
