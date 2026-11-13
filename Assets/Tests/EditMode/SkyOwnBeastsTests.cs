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
    /// The eighteenth pass of beasts (bible §3.2.1): the roles of the Reinos do Céu (Região 16, Docs/Mapas100.md 91–99) that the overworld still
    /// played with another region's beast after the seventeenth pass. Same contract as <see cref="LentRoleBeastsTests"/>: each one keeps its
    /// stand-in's biome (the prefix) and numbers, so the economy of the region does not move. None of them shoots (none of the stand-ins did).
    /// </summary>
    public class SkyOwnBeastsTests
    {
        private const string LesserArchon = "enemy_forest_lesser_archon";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_runaway_archon", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.RunawayArchon, 420, 430, LesserArchon),
            new OverworldBeastsTests.Beast("enemy_forest_stubborn_spark", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.StubbornSpark, 436, 450, "enemy_forest_glow_mantle"),
            new OverworldBeastsTests.Beast("enemy_forest_sphere_smoke", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SphereSmoke, 456, 470, "enemy_forest_smoke_goat"),
            new OverworldBeastsTests.Beast("enemy_forest_flower_treader", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.FlowerTreader, 466, 478, "enemy_forest_garden_shadow"),
            new OverworldBeastsTests.Beast("enemy_forest_smoke_thorn", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SmokeThorn, 466, 478, "enemy_forest_angry_cedar"),
            new OverworldBeastsTests.Beast("enemy_mountain_ember_stowaway", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.EmberStowaway, 474, 486, "enemy_mountain_bronze_wolf"),
            new OverworldBeastsTests.Beast("enemy_forest_noon_shadow", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.NoonShadow, 474, 486, "enemy_forest_late_shadow"),
            new OverworldBeastsTests.Beast("enemy_forest_last_shadow", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.LastShadow, 482, 492, "enemy_forest_creeping_dark"),
            new OverworldBeastsTests.Beast("enemy_mountain_high_wind", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.HighWind, 482, 492, "enemy_mountain_smoke_harpy"),
            new OverworldBeastsTests.Beast("enemy_forest_fled_archon", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.FledArchon, 488, 498, LesserArchon),
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
                .Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds).Concat(LentRoleBestiaryTexts.EnemyIds).Concat(HighlandOwnBestiaryTexts.EnemyIds));
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
            string prefix = beast.Biome == BiomeType.Mountain ? "enemy_mountain_" : "enemy_forest_";
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
        public void NoneOfThem_Shoots_LikeTheStandInsTheyReplace([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            Assert.IsFalse(definition.IsRanged, beast.Id);
            Assert.AreEqual(EnemyProjectileKind.None, definition.Projectile, beast.Id);
            Assert.AreEqual(Load(beast.StandIn).IsRanged, definition.IsRanged, $"{beast.Id}: swings like {beast.StandIn}");
            Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
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
            Assert.IsFalse(page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged), $"{beast.Id}: no [distância] tip, it does not shoot");

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(SkyOwnBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), SkyOwnBestiaryTexts.EnemyIds);
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            Assert.AreEqual(beast.Model, ProceduralEnemyModels.KindFor(beast.Id), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(beast.Model), $"{beast.Id}: a day enemy");
            Assert.IsFalse(ProceduralEnemyModels.IsOssuaryGuardian(beast.Model), beast.Id);
            Assert.IsFalse(ProceduralEnemyModels.IsBoss(beast.Model), beast.Id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, beast.Model);
            Assert.That((int)beast.Model, Is.InRange(380, 391), $"{beast.Id}: the kinds reserved for this pass");

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
        public void TheTwoArchons_AreTwoBodies_NotTheLimiarsArconteMenor()
        {
            Assert.AreNotEqual(ProceduralEnemyKind.RunawayArchon, ProceduralEnemyKind.FledArchon);
            Assert.AreNotEqual(ProceduralEnemyModels.KindFor(LesserArchon), ProceduralEnemyKind.RunawayArchon);
            Assert.AreNotEqual(ProceduralEnemyModels.KindFor(LesserArchon), ProceduralEnemyKind.FledArchon);
        }

        [TestCase(ProceduralEnemyKind.FlowerTreader)]
        [TestCase(ProceduralEnemyKind.EmberStowaway)]
        [TestCase(ProceduralEnemyKind.NoonShadow)]
        [TestCase(ProceduralEnemyKind.HighWind)]
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

        [TestCase(ProceduralEnemyKind.HighWind)]
        public void TheWind_HasTwoWings_AndAFanTail(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingRight), kind.ToString());
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Tail), kind.ToString());
        }

        [TestCase(ProceduralEnemyKind.RunawayArchon)]
        [TestCase(ProceduralEnemyKind.LastShadow)]
        [TestCase(ProceduralEnemyKind.FledArchon)]
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

        [TestCase(ProceduralEnemyKind.StubbornSpark)]
        [TestCase(ProceduralEnemyKind.SphereSmoke)]
        [TestCase(ProceduralEnemyKind.SmokeThorn)]
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
