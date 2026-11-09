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
    /// The beasts of the late continuous worlds (Docs/Mapas100.md 25–40) that the Costa das Monções, Delta de Jhilpur and
    /// Teto do Mundo layouts played with stand-ins: Crocodilo do Mangue, Leão do Capim, Rinoceronte-de-Pó, Macaco Ladrão,
    /// Lontra Ladra, Píton Apagada, Trol Apagado (Costa and Delta: Forest biome, the Living Water), Urso-Pardo do Pamir,
    /// Iaque Selvagem, Irbis, Almas and Marmota Gigante (Teto: Mountain biome). Same contract as
    /// <see cref="OverworldBeastsTests"/>: each definition loads, is a weight-0 common of its biome with the region tier's
    /// numbers and no pinned level (it takes the area's), has its bestiary page and name, and its own procedural model with
    /// the motion its rig adds.
    /// </summary>
    public class LateRegionBeastsTests
    {
        private static readonly OverworldBeastsTests.Beast[] Beasts =
        {
            new OverworldBeastsTests.Beast("enemy_forest_mangrove_crocodile", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.MangroveCrocodile, 58, 88, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast("enemy_forest_grass_lion", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.GrassLion, 48, 55, "enemy_forest_wolf"),
            new OverworldBeastsTests.Beast("enemy_forest_dust_rhino", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.DustRhino, 48, 55, "enemy_forest_peccary"),
            new OverworldBeastsTests.Beast("enemy_forest_thief_monkey", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ThiefMonkey, 61, 70, "enemy_mountain_baboon"),
            new OverworldBeastsTests.Beast("enemy_forest_thief_otter", BiomeType.Forest, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.ThiefOtter, 60, 64, "enemy_desert_jackal"),
            new OverworldBeastsTests.Beast("enemy_forest_dimmed_python", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.Python, 64, 70, "enemy_desert_sand_snake"),
            new OverworldBeastsTests.Beast("enemy_forest_dimmed_troll", BiomeType.Forest, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.DimmedTroll, 70, 78, "enemy_mountain_stone_golem"),
            new OverworldBeastsTests.Beast("enemy_mountain_brown_bear", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.BrownBear, 71, 78, "enemy_forest_werewolf"),
            new OverworldBeastsTests.Beast("enemy_mountain_wild_yak", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.WildYak, 71, 96, "enemy_forest_peccary"),
            new OverworldBeastsTests.Beast("enemy_mountain_irbis", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Irbis, 90, 98, "enemy_forest_wolf"),
            new OverworldBeastsTests.Beast("enemy_mountain_almas", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.Almas, 80, 94, "enemy_mountain_baboon"),
            new OverworldBeastsTests.Beast("enemy_mountain_giant_marmot", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.GiantMarmot, 70, 84, "enemy_desert_jackal"),
        };

        /// <summary>The beasts of the earlier pass (<see cref="OverworldBeastsTests"/>): not "older commons" of their biome either.</summary>
        private static readonly string[] EarlierBeasts = OverworldBestiaryTexts.EnemyIds;

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

        /// <summary>The commons that were in <paramref name="biome"/> before both beast passes (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            var newer = new HashSet<string>(Beasts.Select(b => b.Id).Concat(EarlierBeasts).Concat(MingchengNartesBestiaryTexts.EnemyIds)
                .Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds).Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds));
            BiomeDefinition home = BiomeRegistry.Get(biome);
            Assert.IsNotNull(home, biome.ToString());
            return home.Enemies.Where(e => e?.Enemy != null && !e.Enemy.IsBoss && !newer.Contains(e.Enemy.name)).Select(e => e.Enemy).ToList();
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

            CollectionAssert.Contains(LateRegionBestiaryTexts.EnemyIds, beast.Id);
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

            GameObject visual = ProceduralEnemyModels.Instantiate(beast.Model, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
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
        public void Python_Slithers_LikeTheSandSnake_AndOwnsItsTail()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.Python, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform tail = rig.GetPart("Tail_2");
            Assert.IsNotNull(tail);
            Quaternion rest = tail.localRotation;
            rig.Tick(0.37f);
            Assert.AreNotEqual(rest, tail.localRotation, "the tail waves");
            CollectionAssert.Contains(ProceduralEnemyRig.PartsAnimatedBy(ProceduralEnemyKind.Python), ProceduralEnemyParts.Tail,
                "the stepping-legs driver leaves the slithering tail to the rig");
        }

        [TestCase(ProceduralEnemyKind.DustRhino)]
        [TestCase(ProceduralEnemyKind.WildYak)]
        public void Chargers_DropTheirHead_OnTheWindup(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform head = rig.GetPart(ProceduralEnemyParts.Head);
            Quaternion rest = head.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }

            Assert.Greater(Quaternion.Angle(rest, head.localRotation), 12f, $"{kind}: horns down to charge (a telegraph: kept with Reduzir Movimento)");
            CollectionAssert.Contains(ProceduralEnemyRig.PartsAnimatedBy(kind), ProceduralEnemyParts.Head, "the head belongs to the rig");
        }

        [TestCase(ProceduralEnemyKind.MangroveCrocodile)]
        [TestCase(ProceduralEnemyKind.GrassLion)]
        [TestCase(ProceduralEnemyKind.Irbis)]
        [TestCase(ProceduralEnemyKind.BrownBear)]
        public void Biters_GapeTheirJaw_OnTheWindup(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform jaw = rig.GetPart(ProceduralEnemyParts.Jaw);
            Assert.IsNotNull(jaw, kind.ToString());
            Quaternion rest = jaw.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }

            Assert.Greater(Quaternion.Angle(rest, jaw.localRotation), 20f, $"{kind}: jaws open before the bite");
        }

        [Test]
        public void Troll_And_Almas_RaiseTheirArms_OnTheWindup()
        {
            foreach (ProceduralEnemyKind kind in new[] { ProceduralEnemyKind.DimmedTroll, ProceduralEnemyKind.Almas })
            {
                GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
                _created.Add(visual);
                var rig = visual.GetComponent<ProceduralEnemyRig>();
                Transform arm = rig.GetPart(ProceduralEnemyParts.ArmRight);
                Quaternion rest = arm.localRotation;
                rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
                for (int i = 0; i < 20; i++)
                {
                    rig.Tick(0.05f, 0f);
                }

                Assert.Greater(Quaternion.Angle(rest, arm.localRotation), 45f, $"{kind}: the arm goes up for the blow/throw");
            }

            Transform snowball = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.Almas, null).GetComponent<ProceduralEnemyRig>().GetPart(ProceduralEnemyParts.Weapon);
            _created.Add(snowball.root.gameObject);
            Assert.AreEqual(ProceduralEnemyParts.ArmRight, snowball.parent.name, "the Almas holds its snowball in the right hand");
        }
    }
}
