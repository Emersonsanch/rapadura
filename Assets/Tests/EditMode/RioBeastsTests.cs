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
    /// The beasts of the Rio das Doze Horas (Docs/Mapas100.md 71–74) that the overworld layout played with stand-ins: Crocodilo do Rio
    /// Escuro, Íbis Apagada, Escaravelho de Sombra, Junco-Laço (71), Gafanhoto de Sombra, Hipopótamo de Lodo, Cobra do Junco (72), Serpente
    /// de Três Cabeças, Escorpião Negro, Ushabti de Areia (73), Mordedor de Sombra and Treva Rastejante (74), all in the Forest biome like
    /// the region's <c>OverworldLayouts.Rio</c>. Same contract as <see cref="HinokuraBeastsTests"/>: each definition loads, is a weight-0
    /// common of its biome with the region tier's numbers and no pinned level (it takes the area's), has its bestiary page and name, and
    /// its own procedural model with the motion its rig adds. The sheets have no [distância] here: everyone swings.
    /// </summary>
    public class RioBeastsTests
    {
        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_dark_river_crocodile", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.DarkRiverCrocodile, 260, 268, "enemy_forest_mangrove_crocodile"),
            new OverworldBeastsTests.Beast("enemy_forest_dimmed_ibis", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DimmedIbis, 260, 268, "enemy_forest_thief_gull"),
            new OverworldBeastsTests.Beast("enemy_forest_shadow_scarab", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.ShadowScarab, 260, 320, "enemy_mountain_bark_beetle"),
            new OverworldBeastsTests.Beast("enemy_forest_reed_snare", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ReedSnare, 260, 268, "enemy_mountain_ill_wind_mist"),
            new OverworldBeastsTests.Beast("enemy_forest_shadow_locust", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ShadowLocust, 266, 276, "enemy_desert_firefly_swarm"),
            new OverworldBeastsTests.Beast("enemy_forest_mud_hippo", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MudHippo, 266, 276, "enemy_forest_mud_buffalo"),
            new OverworldBeastsTests.Beast("enemy_forest_reed_snake", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.ReedSnake, 266, 276, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast("enemy_forest_three_headed_serpent", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.ThreeHeadedSerpent, 274, 286, "enemy_forest_dimmed_python"),
            new OverworldBeastsTests.Beast("enemy_forest_black_scorpion", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.BlackScorpion, 274, 286, "enemy_desert_scorpion"),
            new OverworldBeastsTests.Beast("enemy_forest_sand_ushabti", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SandUshabti, 274, 286, "enemy_mountain_rope_ushabti"),
            new OverworldBeastsTests.Beast("enemy_forest_shadow_biter", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ShadowBiter, 284, 296, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_forest_creeping_dark", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.CreepingDark, 284, 296, "enemy_cave_ooze"),
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

        /// <summary>The commons that were in <paramref name="biome"/> before the seven beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(MingchengNartesBestiaryTexts.EnemyIds).Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds)
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds).Concat(LentRoleBestiaryTexts.EnemyIds).Concat(HighlandOwnBestiaryTexts.EnemyIds).Concat(SkyOwnBestiaryTexts.EnemyIds).Concat(CoastOwnBestiaryTexts.EnemyIds));
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
        public void EveryOne_Swings_NoSheetHereAsksForADistanceAttack([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            Assert.IsFalse(definition.IsRanged, beast.Id);
            Assert.AreEqual(EnemyProjectileKind.None, definition.Projectile, beast.Id);
            Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
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
            Assert.IsFalse(page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: no [distância] tip");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(RioBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), RioBestiaryTexts.EnemyIds);
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

        [TestCase(ProceduralEnemyKind.DarkRiverCrocodile)]
        [TestCase(ProceduralEnemyKind.DimmedIbis)]
        [TestCase(ProceduralEnemyKind.ReedSnare)]
        [TestCase(ProceduralEnemyKind.ShadowLocust)]
        [TestCase(ProceduralEnemyKind.MudHippo)]
        [TestCase(ProceduralEnemyKind.ReedSnake)]
        [TestCase(ProceduralEnemyKind.ThreeHeadedSerpent)]
        [TestCase(ProceduralEnemyKind.ShadowBiter)]
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

        [TestCase(ProceduralEnemyKind.ShadowScarab)]
        [TestCase(ProceduralEnemyKind.BlackScorpion)]
        public void Pincers_OpenOnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform claw = rig.GetPart(ProceduralEnemyParts.ClawRight);
            Assert.IsNotNull(claw, kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.ClawLeft), kind.ToString());
            Quaternion rest = claw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, claw.localRotation), 20f, $"{kind}: the pincers open before the pinch");
        }

        [Test]
        public void BlackScorpion_CocksItsSting_OnTheWindup()
        {
            Make(ProceduralEnemyKind.BlackScorpion, out ProceduralEnemyRig rig);
            Transform tail = rig.GetPart(ProceduralEnemyParts.Tail);
            Assert.IsNotNull(tail);
            Quaternion rest = tail.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, tail.localRotation), 20f, "the sting draws back before it strikes");
        }

        [Test]
        public void SandUshabti_RaisesBothArms_OnTheWindup()
        {
            Make(ProceduralEnemyKind.SandUshabti, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion leftRest = left.localRotation;
            Quaternion rightRest = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(leftRest, left.localRotation), 30f, "left arm");
            Assert.Greater(Quaternion.Angle(rightRest, right.localRotation), 30f, "right arm");
        }

        [TestCase(ProceduralEnemyKind.ReedSnake)]
        [TestCase(ProceduralEnemyKind.ThreeHeadedSerpent)]
        public void TheSnakes_WaveTheWholeTail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.TailSegmentPrefix + i), $"{kind}: Tail_{i}");
            }

            Transform tail = rig.GetPart(ProceduralEnemyParts.TailSegmentPrefix + 2);
            Quaternion rest = tail.localRotation;
            bool moved = false;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.07f);
                moved |= Quaternion.Angle(rest, tail.localRotation) > 1f;
            }

            Assert.IsTrue(moved, $"{kind}: the wave runs down the tail");
        }

        [Test]
        public void TheSerpent_HasThreeHeads_ThatEachMove()
        {
            Make(ProceduralEnemyKind.ThreeHeadedSerpent, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(RioBeastRecipes.SerpentHeadLeft);
            Transform right = rig.GetPart(RioBeastRecipes.SerpentHeadRight);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Head));
            Assert.IsNotNull(left);
            Assert.IsNotNull(right);
            Quaternion leftRest = left.localRotation;
            Quaternion rightRest = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(leftRest, left.localRotation), 10f, "left head");
            Assert.Greater(Quaternion.Angle(rightRest, right.localRotation), 10f, "right head");
        }

        [TestCase(ProceduralEnemyKind.ReedSnare)]
        [TestCase(ProceduralEnemyKind.ShadowLocust)]
        public void TheDrifters_MoveTheirWings_AndTail(ProceduralEnemyKind kind)
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

            Assert.IsTrue(moved, $"{kind}: wings/nooses stir");
        }

        [Test]
        public void CreepingDark_FlattensOnTheWindup_AndTheStolenLightFlares()
        {
            Make(ProceduralEnemyKind.CreepingDark, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform light = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(light);
            Vector3 bodyRest = body.localScale;
            Vector3 lightRest = light.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, "flattens before it strikes");
            Assert.Greater(light.localScale.x, lightRest.x, "the swallowed gold flares");
        }
    }
}
