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
    /// The twelfth pass of beasts (bible §3.2.1): the most visible roles of Regions 1–4 that the overworld still played with stand-ins
    /// (Docs/Mapas100.md 02–06, 10, 14–16, 20). Same contract as <see cref="LateStandInBeastsTests"/>, each under the prefix of its biome. The stork,
    /// the bubble toad, the silk moth, the laurel pigeon, the smoke peacock and the stone jackdaw are the [distância] shooters.
    /// </summary>
    public class EarlyStandInBeastsTests
    {
        private const string Stork = "enemy_forest_dimmed_stork";
        private const string Toad = "enemy_forest_bubble_toad";
        private const string Moth = "enemy_forest_silk_moth";
        private const string Pigeon = "enemy_forest_laurel_pigeon";
        private const string Peacock = "enemy_forest_smoke_peacock";
        private const string Jackdaw = "enemy_mountain_stone_jackdaw";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast(Stork, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DimmedStork, 4, 8, "enemy_mountain_potoo"),
            new OverworldBeastsTests.Beast(Toad, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BubbleToad, 16, 22, "enemy_cave_bat_swarm"),
            new OverworldBeastsTests.Beast("enemy_forest_smoke_mosquito", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokeMosquito, 16, 22, "enemy_cave_bat_swarm"),
            new OverworldBeastsTests.Beast(Moth, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SilkMoth, 18, 24, "enemy_mountain_potoo"),
            new OverworldBeastsTests.Beast(Pigeon, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.LaurelPigeon, 20, 26, "enemy_mountain_potoo"),
            new OverworldBeastsTests.Beast("enemy_forest_laurel_lizard", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.LaurelLizard, 20, 26, "enemy_forest_wolf"),
            new OverworldBeastsTests.Beast(Peacock, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokePeacock, 26, 32, "enemy_mountain_potoo"),
            new OverworldBeastsTests.Beast("enemy_desert_thorn_lizard", BiomeType.Desert, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ThornLizard, 22, 28, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_mountain_dimmed_lynx", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.DimmedLynx, 15, 20, "enemy_forest_wolf"),
            new OverworldBeastsTests.Beast("enemy_mountain_smudge", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.Smudge, 26, 32, "enemy_forest_wolf"),
            new OverworldBeastsTests.Beast(Jackdaw, BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.StoneJackdaw, 28, 34, "enemy_mountain_potoo"),
            new OverworldBeastsTests.Beast("enemy_cave_young_ubir", BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.YoungUbir, 19, 24, "enemy_forest_spider"),
        };

        private static readonly Dictionary<string, EnemyProjectileKind> Shooters = new Dictionary<string, EnemyProjectileKind>
        {
            { Stork, EnemyProjectileKind.Feather },
            { Toad, EnemyProjectileKind.LureGlob },
            { Moth, EnemyProjectileKind.LureGlob },
            { Pigeon, EnemyProjectileKind.Feather },
            { Peacock, EnemyProjectileKind.Ember },
            { Jackdaw, EnemyProjectileKind.Feather },
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
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds));
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

            CollectionAssert.Contains(EarlyStandInBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), EarlyStandInBestiaryTexts.EnemyIds);
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

        [TestCase(ProceduralEnemyKind.DimmedStork)]
        [TestCase(ProceduralEnemyKind.SilkMoth)]
        [TestCase(ProceduralEnemyKind.LaurelPigeon)]
        [TestCase(ProceduralEnemyKind.LaurelLizard)]
        [TestCase(ProceduralEnemyKind.SmokePeacock)]
        [TestCase(ProceduralEnemyKind.ThornLizard)]
        [TestCase(ProceduralEnemyKind.DimmedLynx)]
        [TestCase(ProceduralEnemyKind.Smudge)]
        [TestCase(ProceduralEnemyKind.StoneJackdaw)]
        [TestCase(ProceduralEnemyKind.YoungUbir)]
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
        public void BubbleToad_FlattensOnTheWindup_AndTheBubbleFlares()
        {
            Make(ProceduralEnemyKind.BubbleToad, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform bubble = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(bubble);
            Vector3 bodyRest = body.localScale;
            Vector3 bubbleRest = bubble.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, "flattens before it spits");
            Assert.Greater(bubble.localScale.x, bubbleRest.x, "the mud bubble flares");
        }

        [Test]
        public void SmokeMosquito_SwingsItsProboscis_OnTheWindup()
        {
            Make(ProceduralEnemyKind.SmokeMosquito, out ProceduralEnemyRig rig);
            Transform sting = rig.GetPart(ProceduralEnemyParts.Stinger);
            Assert.IsNotNull(sting);
            Quaternion rest = sting.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, sting.localRotation), 30f, "the proboscis swings");
        }

        [TestCase(ProceduralEnemyKind.Smudge)]
        [TestCase(ProceduralEnemyKind.YoungUbir)]
        public void TheDrifters_StirTheirArms(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform end = rig.GetPart(ProceduralEnemyParts.WingRight);
            Assert.IsNotNull(end);
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.WingLeft));
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Tail));
            Quaternion rest = end.localRotation;
            bool moved = false;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.07f);
                moved |= Quaternion.Angle(rest, end.localRotation) > 1f;
            }

            Assert.IsTrue(moved, $"{kind}: the arms stir");
        }
    }
}
