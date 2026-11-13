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
    /// The beasts of the Planície de Mingcheng and the Estepe dos Nartes (Docs/Mapas100.md 41–52) that the overworld layouts
    /// played with stand-ins: Leão de Pedra, Raposa de Cauda Lanterna, Soldado de Terracota, Tartaruga de Pedra, Búfalo de
    /// Lama, Filhote de Nian (Mingcheng: Forest biome, the Living Water), Grifo Jovem, Saiga Assustada, Lobo de Bronze,
    /// Gigante Sonâmbulo, Besouro-Casca and Ochokochi (Nartes: Mountain biome). Same contract as
    /// <see cref="LateRegionBeastsTests"/>: each definition loads, is a weight-0 common of its biome with the region tier's
    /// numbers and no pinned level (it takes the area's), has its bestiary page and name, and its own procedural model with
    /// the motion its rig adds.
    /// </summary>
    public class MingchengNartesBeastsTests
    {
        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_stone_lion", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.StoneLion, 91, 109, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_forest_lantern_fox", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.LanternFox, 90, 95, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_forest_terracotta_soldier", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.TerracottaSoldier, 91, 124, "enemy_desert_crowned_orc"),
            new OverworldBeastsTests.Beast("enemy_forest_stone_turtle", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.StoneTurtle, 92, 99, "enemy_forest_coconut_crab"),
            new OverworldBeastsTests.Beast("enemy_forest_mud_buffalo", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MudBuffalo, 92, 101, "enemy_forest_peccary"),
            new OverworldBeastsTests.Beast("enemy_forest_nian_cub", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.NianCub, 116, 126, "enemy_mountain_baboon"),
            new OverworldBeastsTests.Beast("enemy_mountain_young_griffin", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.YoungGriffin, 101, 110, "enemy_desert_ember_vulture"),
            new OverworldBeastsTests.Beast("enemy_mountain_saiga", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Saiga, 100, 104, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_mountain_bronze_wolf", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.BronzeWolf, 108, 114, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_mountain_sleepwalker_giant", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.SleepwalkerGiant, 112, 117, "enemy_desert_crowned_orc"),
            new OverworldBeastsTests.Beast("enemy_mountain_bark_beetle", BiomeType.Mountain, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.BarkBeetle, 118, 126, "enemy_forest_coconut_crab"),
            new OverworldBeastsTests.Beast("enemy_mountain_ochokochi", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.Ochokochi, 112, 146, "enemy_mountain_baboon"),
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

        /// <summary>The commons that were in <paramref name="biome"/> before the three beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(OverworldBestiaryTexts.EnemyIds).Concat(LateRegionBestiaryTexts.EnemyIds)
                .Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds).Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds).Concat(FarStandInBestiaryTexts.EnemyIds).Concat(WideStandInBestiaryTexts.EnemyIds).Concat(BorrowedRoleBestiaryTexts.EnemyIds).Concat(LentRoleBestiaryTexts.EnemyIds));
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
        public void AllOfThem_Swing_InMelee([ValueSource(nameof(All))] OverworldBeastsTests.Beast beast)
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

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { page.LoreKey };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"{key}: en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: pt-BR");
            }

            CollectionAssert.Contains(MingchengNartesBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
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

        [TestCase(ProceduralEnemyKind.StoneLion)]
        [TestCase(ProceduralEnemyKind.LanternFox)]
        [TestCase(ProceduralEnemyKind.StoneTurtle)]
        [TestCase(ProceduralEnemyKind.NianCub)]
        [TestCase(ProceduralEnemyKind.YoungGriffin)]
        [TestCase(ProceduralEnemyKind.BronzeWolf)]
        public void Biters_GapeTheirJaw_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Quaternion rest = jaw.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: jaws open before the bite");
        }

        [TestCase(ProceduralEnemyKind.MudBuffalo)]
        [TestCase(ProceduralEnemyKind.Saiga)]
        public void Chargers_DropTheirHead_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform head = rig.GetPart(ProceduralEnemyParts.Head);
            Quaternion rest = head.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, head.localRotation), 12f, $"{kind}: horns down to charge (a telegraph: kept with Reduzir Movimento)");
            CollectionAssert.Contains(ProceduralEnemyRig.PartsAnimatedBy(kind), ProceduralEnemyParts.Head, "the head belongs to the rig");
        }

        [TestCase(ProceduralEnemyKind.TerracottaSoldier)]
        [TestCase(ProceduralEnemyKind.SleepwalkerGiant)]
        [TestCase(ProceduralEnemyKind.Ochokochi)]
        public void Humanoids_RaiseTheirArm_OnTheWindup(ProceduralEnemyKind kind)
        {
            Make(kind, out ProceduralEnemyRig rig);
            Transform arm = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion rest = arm.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, arm.localRotation), 45f, $"{kind}: the arm goes up for the blow");
        }

        [Test]
        public void TerracottaSoldier_HoldsItsHalberd_InTheRightHand()
        {
            Make(ProceduralEnemyKind.TerracottaSoldier, out ProceduralEnemyRig rig);
            Transform halberd = rig.GetPart(ProceduralEnemyParts.Weapon);
            Assert.IsNotNull(halberd);
            Assert.AreEqual(ProceduralEnemyParts.ArmRight, halberd.parent.name);
        }

        [Test]
        public void Griffin_SpreadsItsWings_OnTheWindup()
        {
            Make(ProceduralEnemyKind.YoungGriffin, out ProceduralEnemyRig rig);
            Transform wing = rig.GetPart(ProceduralEnemyParts.WingLeft);
            Quaternion rest = wing.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(rest, wing.localRotation), 40f, "the wings open before the pounce");
        }

        [Test]
        public void LanternFox_CarriesItsLantern_OnTheTailTip_AndItFlares()
        {
            Make(ProceduralEnemyKind.LanternFox, out ProceduralEnemyRig rig);
            Transform lantern = rig.GetPart(ProceduralEnemyParts.Lantern);
            Assert.IsNotNull(lantern);
            Assert.AreEqual(ProceduralEnemyParts.TailSegmentPrefix + "1", lantern.parent.name);
            Vector3 rest = lantern.localScale;
            Windup(rig);
            Assert.Greater(lantern.localScale.x, rest.x * 1.1f, "the lantern flares before the bite");
        }

        [Test]
        public void BarkBeetle_HasSixLegs_AndOpensItsMandibles_OnTheWindup()
        {
            GameObject visual = Make(ProceduralEnemyKind.BarkBeetle, out ProceduralEnemyRig rig);
            int legs = visual.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith(ProceduralEnemyParts.LegPrefix));
            Assert.AreEqual(NartesBeastRecipes.BeetleLegPairs * 2, legs);
            Transform left = rig.GetPart(ProceduralEnemyParts.ClawLeft);
            Transform right = rig.GetPart(ProceduralEnemyParts.ClawRight);
            Quaternion restLeft = left.localRotation;
            Quaternion restRight = right.localRotation;
            Windup(rig);
            Assert.Greater(Quaternion.Angle(restLeft, left.localRotation), 20f, "left mandible opens");
            Assert.Greater(Quaternion.Angle(restRight, right.localRotation), 20f, "right mandible opens");
        }
    }
}
