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
    /// The beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58) that the overworld layout played with stand-ins: Bode de
    /// Fumaça, Abelha Dourada, Pássaro-Flauta Apagado, Raposa da Vinha (Vale dos Sátiros: Forest biome, the Living Water),
    /// Gralha-Alpina, Takin Apagado, Cabra-Azul, Morcego-Vela, Ushabti de Corda, Gato-de-Pallas Apagado, Águia da Tempestade and
    /// Harpia de Fumaça (the peaks: Mountain biome). Same contract as <see cref="MingchengNartesBeastsTests"/>: each definition
    /// loads, is a weight-0 common of its biome with the region tier's numbers and no pinned level (it takes the area's), has its
    /// bestiary page and name, and its own procedural model with the motion its rig adds. The flute bird and the storm eagle
    /// are the sheets' [distância] ones (<see cref="RangedEnemiesTests"/>); everyone else swings.
    /// </summary>
    public class FarnkeshBeastsTests
    {
        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_smoke_goat", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SmokeGoat, 140, 148, "enemy_mountain_wild_yak"),
            new OverworldBeastsTests.Beast("enemy_forest_golden_bee", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.GoldenBee, 140, 148, "enemy_desert_firefly_swarm"),
            new OverworldBeastsTests.Beast("enemy_forest_flute_bird", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.FluteBird, 140, 148, "enemy_mountain_crested_hawk"),
            new OverworldBeastsTests.Beast("enemy_forest_vine_fox", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.VineFox, 140, 148, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_mountain_alpine_chough", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.AlpineChough, 145, 152, "enemy_cave_dark_crow"),
            new OverworldBeastsTests.Beast("enemy_mountain_takin", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.Takin, 145, 152, "enemy_mountain_wild_yak"),
            new OverworldBeastsTests.Beast("enemy_mountain_blue_sheep", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.BlueSheep, 145, 152, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_mountain_candle_bat", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.CandleBat, 150, 160, "enemy_cave_vampire_bat"),
            new OverworldBeastsTests.Beast("enemy_mountain_rope_ushabti", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.RopeUshabti, 150, 160, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_mountain_pallas_cat", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.PallasCat, 158, 168, "enemy_mountain_irbis"),
            new OverworldBeastsTests.Beast("enemy_mountain_storm_eagle", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.StormEagle, 166, 180, "enemy_mountain_crested_hawk"),
            new OverworldBeastsTests.Beast("enemy_mountain_smoke_harpy", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokeHarpy, 180, 200, "enemy_cave_dark_crow"),
        };

        /// <summary>The sheets' [distância] ones: a stiff feather thrown like the hawk's (same shot and sound).</summary>
        private static readonly string[] Shooters = { "enemy_forest_flute_bird", "enemy_mountain_storm_eagle" };

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

        /// <summary>The commons that were in <paramref name="biome"/> before the four beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(MingchengNartesBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds).Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds));
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
            string prefix = beast.Biome == BiomeType.Forest ? "enemy_forest_" : "enemy_mountain_";
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

            // The Forest commons stay under every Desert/Mountain common (BalanceInvariantsTests).
            if (beast.Biome == BiomeType.Forest)
            {
                Assert.Less(definition.MaxHealth, 110f, $"{beast.Id}: a Forest common under the Desert's weakest");
            }

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
        public void TheShooters_ThrowFeathers_FromRange_AndTheRestSwing([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            bool shoots = Shooters.Contains(beast.Id);
            Assert.AreEqual(shoots, definition.IsRanged, beast.Id);
            if (shoots)
            {
                Assert.AreEqual(EnemyProjectileKind.Feather, definition.Projectile, beast.Id);
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

            CollectionAssert.Contains(FarnkeshBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), FarnkeshBestiaryTexts.EnemyIds);
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

        [TestCase(ProceduralEnemyKind.VineFox)]
        [TestCase(ProceduralEnemyKind.PallasCat)]
        [TestCase(ProceduralEnemyKind.FluteBird)]
        [TestCase(ProceduralEnemyKind.AlpineChough)]
        [TestCase(ProceduralEnemyKind.StormEagle)]
        [TestCase(ProceduralEnemyKind.SmokeHarpy)]
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

        [TestCase(ProceduralEnemyKind.SmokeGoat)]
        [TestCase(ProceduralEnemyKind.Takin)]
        [TestCase(ProceduralEnemyKind.BlueSheep)]
        public void Chargers_DropTheirHead_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform head = rig.GetPart(ProceduralEnemyParts.Head);
            Quaternion rest = head.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, head.localRotation), 12f, $"{kind}: horns down to charge (a telegraph: kept with Reduzir Movimento)");
            CollectionAssert.Contains(ProceduralEnemyRig.PartsAnimatedBy(kind), ProceduralEnemyParts.Head, "the head belongs to the rig");
        }

        [Test]
        public void RopeUshabti_RaisesBothArms_OnTheWindup()
        {
            Make(ProceduralEnemyKind.RopeUshabti, out ProceduralEnemyRig rig);
            Transform left = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion restLeft = left.localRotation;
            Quaternion restRight = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(restLeft, left.localRotation), 45f, "left arm up for the hoe");
            Assert.Greater(Quaternion.Angle(restRight, right.localRotation), 45f, "right arm up for the hoe");
        }

        [Test]
        public void GoldenBee_SwingsItsSting_OnTheWindup_AndHasWings()
        {
            Make(ProceduralEnemyKind.GoldenBee, out ProceduralEnemyRig rig);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft));
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingRight));
            Transform sting = rig.GetPart(ProceduralEnemyParts.Stinger);
            Assert.IsNotNull(sting);
            Assert.AreEqual(ProceduralEnemyParts.Body, sting.parent.name);
            Quaternion rest = sting.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, sting.localRotation), 30f, "the sting swings under before it strikes");
        }

        [Test]
        public void CandleBat_CarriesItsFlame_OnTheHead_AndItFlares()
        {
            Make(ProceduralEnemyKind.CandleBat, out ProceduralEnemyRig rig);
            Transform flame = rig.GetPart(ProceduralEnemyParts.Lantern);
            Assert.IsNotNull(flame);
            Assert.AreEqual(ProceduralEnemyParts.Body, flame.parent.name);
            Vector3 rest = flame.localScale;
            Windup(rig);
            Assert.Greater(flame.localScale.x, rest.x * 1.1f, "the flame flares before the bite");
        }

        [Test]
        public void TheShooters_HaveNoLanternPart()
        {
            // RangedEnemiesTests: the light a shooter fires comes from a Lantern part; these throw feathers.
            foreach (ProceduralEnemyKind kind in new[] { ProceduralEnemyKind.FluteBird, ProceduralEnemyKind.StormEagle })
            {
                Make(kind, out ProceduralEnemyRig rig);
                Assert.IsNull(rig.GetPart(ProceduralEnemyParts.Lantern), kind.ToString());
                Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft), kind.ToString());
            }
        }
    }
}
