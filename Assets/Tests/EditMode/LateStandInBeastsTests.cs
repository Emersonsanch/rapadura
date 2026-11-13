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
    /// The eleventh pass of beasts (bible §3.2.1): the twelve most visible roles of Regions 12–16 that the overworld still played with stand-ins.
    /// Same contract as <see cref="CeuBeastsTests"/>, each under the prefix of its layout's biome. The Bomba Vulcânica and the Homem-Escorpião are
    /// the [distância] shooters.
    /// </summary>
    public class LateStandInBeastsTests
    {
        private const string Bomb = "enemy_mountain_volcanic_bomb";
        private const string ScorpionMan = "enemy_forest_scorpion_man";

        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_mountain_devouring_salamander", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.DevouringSalamander, 264, 280, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast(Bomb, BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.VolcanicBomb, 234, 242, "enemy_desert_ember_vulture"),
            new OverworldBeastsTests.Beast("enemy_forest_thief_goose", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ThiefGoose, 266, 272, "enemy_forest_thief_gull"),
            new OverworldBeastsTests.Beast("enemy_forest_child_of_apep", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.ChildOfApep, 304, 320, "enemy_forest_dimmed_python"),
            new OverworldBeastsTests.Beast("enemy_forest_quicksand", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.Quicksand, 310, 320, "enemy_cave_ooze"),
            new OverworldBeastsTests.Beast(ScorpionMan, BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ScorpionMan, 301, 307, "enemy_desert_scorpion"),
            new OverworldBeastsTests.Beast("enemy_forest_cedar_lynx", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.CedarLynx, 324, 330, "enemy_mountain_irbis"),
            new OverworldBeastsTests.Beast("enemy_forest_resin_bee", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ResinBee, 324, 330, "enemy_forest_golden_bee"),
            new OverworldBeastsTests.Beast("enemy_forest_blade_bat", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.BladeBat, 372, 378, "enemy_mountain_candle_bat"),
            new OverworldBeastsTests.Beast("enemy_forest_great_eel", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.GreatEel, 390, 396, "enemy_forest_reed_snake"),
            new OverworldBeastsTests.Beast("enemy_forest_ash_bull", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.AshBull, 363, 370, "enemy_mountain_wild_yak"),
            new OverworldBeastsTests.Beast("enemy_forest_smoke_rope", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.SmokeRope, 476, 486, "enemy_forest_reed_snare"),
        };

        private static readonly Dictionary<string, EnemyProjectileKind> Shooters = new Dictionary<string, EnemyProjectileKind>
        {
            { Bomb, EnemyProjectileKind.Ember },
            { ScorpionMan, EnemyProjectileKind.Feather },
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
                .Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds));
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

            CollectionAssert.Contains(LateStandInBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void TheBestiaryList_IsExactlyTheseBeasts()
        {
            CollectionAssert.AreEquivalent(Beasts.Select(b => b.Id), LateStandInBestiaryTexts.EnemyIds);
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

        [TestCase(ProceduralEnemyKind.DevouringSalamander)]
        [TestCase(ProceduralEnemyKind.ThiefGoose)]
        [TestCase(ProceduralEnemyKind.ChildOfApep)]
        [TestCase(ProceduralEnemyKind.ScorpionMan)]
        [TestCase(ProceduralEnemyKind.CedarLynx)]
        [TestCase(ProceduralEnemyKind.BladeBat)]
        [TestCase(ProceduralEnemyKind.GreatEel)]
        [TestCase(ProceduralEnemyKind.SmokeRope)]
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
        public void VolcanicBomb_TiltsBack_AndItsCoreFlares()
        {
            Make(ProceduralEnemyKind.VolcanicBomb, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform core = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(core);
            Quaternion bodyRest = body.localRotation;
            Vector3 coreRest = core.localScale;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(bodyRest, body.localRotation), 8f, "leans back before it spits");
            Assert.Greater(core.localScale.x, coreRest.x, "the core flares");
        }

        [Test]
        public void Quicksand_FlattensOnTheWindup_AndTheLampFlares()
        {
            Make(ProceduralEnemyKind.Quicksand, out ProceduralEnemyRig rig);
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Transform lamp = rig.GetPart(ProceduralEnemyParts.StolenLight);
            Assert.IsNotNull(lamp);
            Vector3 bodyRest = body.localScale;
            Vector3 lampRest = lamp.localScale;
            Windup(rig);
            Assert.Less(body.localScale.y, bodyRest.y, "flattens before it strikes");
            Assert.Greater(lamp.localScale.x, lampRest.x, "the sunken lamp flares");
        }

        [Test]
        public void ResinBee_SwingsItsSting_OnTheWindup()
        {
            Make(ProceduralEnemyKind.ResinBee, out ProceduralEnemyRig rig);
            Transform sting = rig.GetPart(ProceduralEnemyParts.Stinger);
            Assert.IsNotNull(sting);
            Quaternion rest = sting.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, sting.localRotation), 30f, "the sting comes forward");
        }

        [Test]
        public void AshBull_LowersItsHead_OnTheWindup()
        {
            Make(ProceduralEnemyKind.AshBull, out ProceduralEnemyRig rig);
            Transform head = rig.GetPart(ProceduralEnemyParts.Head);
            Assert.IsNotNull(head);
            Quaternion rest = head.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, head.localRotation), 10f, "the head drops before the charge");
        }

        [Test]
        public void SmokeRope_StirsItsLooseEnds()
        {
            Make(ProceduralEnemyKind.SmokeRope, out ProceduralEnemyRig rig);
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

            Assert.IsTrue(moved, "the loose ends stir");
        }
    }
}
