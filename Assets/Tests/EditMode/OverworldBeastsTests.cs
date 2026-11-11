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
    /// The beasts the map sheets asked for (Docs/Mapas100.md 07–25) that the continuous worlds and Taa Pwani's quests used to
    /// play with stand-ins: Chacal, Cobra-de-Areia, Orc Coroado de Āz, Pedra que Rola, Neblina do Vento Ruim, Babuíno,
    /// Lodo de Gruta, Corvo-da-Treva, Gaivota Ladra and Caranguejo-dos-Coqueiros. Each definition loads, is a common of its
    /// region's tier (home biome at weight 0: bestiary region + loot table, placed by the map/overworld layouts), fights at
    /// its map's level band with the region's numbers, has its bestiary page (lore + five cousins, en + pt-BR) and name, and
    /// its procedural model builds with the parts its rig moves.
    /// </summary>
    public class OverworldBeastsTests
    {
        public readonly struct Beast
        {
            public readonly string Id;
            public readonly BiomeType Biome;
            public readonly EnemyMotionArchetype Archetype;
            public readonly ProceduralEnemyKind Model;
            /// <summary>Lowest and highest level of the map areas the sheet puts it in.</summary>
            public readonly int MinLevel;
            public readonly int MaxLevel;
            /// <summary>The existing common that stood in for it (same region numbers, roughly).</summary>
            public readonly string StandIn;

            public Beast(string id, BiomeType biome, EnemyMotionArchetype archetype, ProceduralEnemyKind model, int minLevel, int maxLevel, string standIn)
            {
                Id = id;
                Biome = biome;
                Archetype = archetype;
                Model = model;
                MinLevel = minLevel;
                MaxLevel = maxLevel;
                StandIn = standIn;
            }

            public override string ToString() => Id;
        }

        private static readonly Beast[] Beasts =
        {
            new Beast("enemy_desert_jackal", BiomeType.Desert, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Jackal, 5, 21, "enemy_forest_wolf"),
            new Beast("enemy_desert_sand_snake", BiomeType.Desert, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.SandSnake, 6, 28, "enemy_desert_scorpion"),
            new Beast("enemy_desert_crowned_orc", BiomeType.Desert, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.CrownedOrc, 24, 32, "enemy_desert_scorpion"),
            new Beast("enemy_mountain_rolling_stone", BiomeType.Mountain, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.RollingStone, 9, 32, "enemy_mountain_stone_golem"),
            new Beast("enemy_mountain_ill_wind_mist", BiomeType.Mountain, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.IllWindMist, 10, 44, "enemy_mountain_potoo"),
            new Beast("enemy_mountain_baboon", BiomeType.Mountain, EnemyMotionArchetype.Quadruped, ProceduralEnemyKind.Baboon, 9, 32, "enemy_forest_wolf"),
            new Beast("enemy_cave_ooze", BiomeType.Caves, EnemyMotionArchetype.Heavy, ProceduralEnemyKind.CaveOoze, 13, 15, "enemy_mountain_stone_golem"),
            new Beast("enemy_cave_dark_crow", BiomeType.Caves, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.DarkCrow, 42, 50, "enemy_mountain_potoo"),
            new Beast("enemy_forest_thief_gull", BiomeType.Forest, EnemyMotionArchetype.Flyer, ProceduralEnemyKind.ThiefGull, 45, 50, "enemy_desert_ember_vulture"),
            new Beast("enemy_forest_coconut_crab", BiomeType.Forest, EnemyMotionArchetype.Arthropod, ProceduralEnemyKind.CoconutCrab, 45, 50, "enemy_desert_scorpion"),
        };

        private static IEnumerable<Beast> All() => Beasts;

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

        /// <summary>The commons that were already in <paramref name="biome"/> before these (the region's own numbers).</summary>
        private static List<EnemyDefinition> OlderCommonsOf(BiomeType biome)
        {
            // Neither these nor the later pass (LateRegionBeastsTests) count as the region's own numbers.
            var ours = new HashSet<string>(Beasts.Select(b => b.Id).Concat(LateRegionBestiaryTexts.EnemyIds).Concat(MingchengNartesBestiaryTexts.EnemyIds)
                .Concat(FarnkeshBestiaryTexts.EnemyIds).Concat(MarGeladoBestiaryTexts.EnemyIds).Concat(HinokuraBestiaryTexts.EnemyIds).Concat(RioBestiaryTexts.EnemyIds).Concat(MisturaBestiaryTexts.EnemyIds).Concat(ProfundoBestiaryTexts.EnemyIds).Concat(CeuBestiaryTexts.EnemyIds).Concat(LateStandInBestiaryTexts.EnemyIds).Concat(MidStandInBestiaryTexts.EnemyIds).Concat(EarlyStandInBestiaryTexts.EnemyIds));
            BiomeDefinition home = BiomeRegistry.Get(biome);
            Assert.IsNotNull(home, biome.ToString());
            return home.Enemies.Where(e => e?.Enemy != null && !e.Enemy.IsBoss && !ours.Contains(e.Enemy.name)).Select(e => e.Enemy).ToList();
        }

        [Test]
        public void Definitions_Load_AreCommons_WithTheSheetsArchetype([ValueSource(nameof(All))] Beast beast)
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
            StringAssert.StartsWith("enemy_" + BiomePrefix(beast.Biome) + "_", beast.Id, "named after its home biome (enemy_<biome>_)");
        }

        [Test]
        public void EachOne_LivesInItsBiome_AtWeightZero_LikeTheOtherPlacedCommons([ValueSource(nameof(All))] Beast beast)
        {
            BiomeRegistry.Invalidate();
            EnemyDefinition definition = Load(beast.Id);
            BiomeDefinition home = BiomeRegistry.FindBiomeOf(definition);
            Assert.IsNotNull(home, $"{beast.Id}: in a biome table (loot + bestiary region)");
            Assert.AreEqual(beast.Biome, home.Type, beast.Id);
            Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == definition).Weight, $"{beast.Id}: weight 0, never rolled by the world-map encounters");
        }

        [Test]
        public void Stats_FollowTheRegionTier_AndScaleIntoTheMapsLevelBand([ValueSource(nameof(All))] Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            List<EnemyDefinition> commons = OlderCommonsOf(beast.Biome);
            Assert.IsNotEmpty(commons, beast.Biome.ToString());

            // The campaign tier of its region: same kill XP as the region's commons, health and damage per second in their range.
            Assert.IsTrue(commons.Any(c => c.ExperienceReward == definition.ExperienceReward), $"{beast.Id}: XP of its region's commons");
            float hp = commons.Average(c => c.MaxHealth);
            float dps = commons.Average(c => c.AttackDamage / c.AttackCooldown);
            Assert.That(definition.MaxHealth / hp, Is.InRange(0.6f, 1.6f), $"{beast.Id}: health against its region's commons");
            Assert.That(definition.AttackDamage / definition.AttackCooldown / dps, Is.InRange(0.5f, 2f), $"{beast.Id}: damage per second against its region's commons");

            // No level pinned on the asset: it fights at its map's level, the band scaling it like every other common there.
            Assert.AreEqual(0, definition.Level, $"{beast.Id}: takes the map's level");
            Assert.AreEqual(beast.MinLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MinLevel), beast.Id);
            Assert.AreEqual(beast.MaxLevel, EnemyLevelScaling.ResolveLevel(definition.Level, beast.MaxLevel), beast.Id);
            float low = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MinLevel);
            float high = EnemyLevelScaling.HealthFor(definition.MaxHealth, beast.MaxLevel);
            Assert.GreaterOrEqual(high, low, beast.Id);
            Assert.GreaterOrEqual(EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MaxLevel), EnemyLevelScaling.DamageFor(definition.AttackDamage, beast.MinLevel), beast.Id);

            // Swapping it for the stand-in keeps the fight about as long at the same level (0.5–2×, by design not identical).
            EnemyDefinition standIn = Load(beast.StandIn);
            Assert.That(high / EnemyLevelScaling.HealthFor(standIn.MaxHealth, beast.MaxLevel), Is.InRange(0.5f, 2.5f), $"{beast.Id} vs {beast.StandIn}");
        }

        [Test]
        public void OnlyTheCrow_Shoots_TheOthersSwing([ValueSource(nameof(All))] Beast beast)
        {
            EnemyDefinition definition = Load(beast.Id);
            bool crow = beast.Model == ProceduralEnemyKind.DarkCrow;
            Assert.AreEqual(crow, definition.IsRanged, beast.Id);
            if (crow)
            {
                Assert.AreEqual(EnemyProjectileKind.Feather, definition.Projectile, "the crow throws its feathers");
                Assert.AreEqual(EnemyStrikeKind.Projectile, EnemyRangedAttack.StrikeKindOf(definition.IsRanged, false));
            }
            else
            {
                Assert.LessOrEqual(definition.AttackRange, 2.5f, $"{beast.Id}: a melee swing");
                Assert.AreEqual(EnemyStrikeKind.Swing, EnemyRangedAttack.StrikeKindOf(definition.IsRanged, false));
            }
        }

        [Test]
        public void EachOne_HasABestiaryPage_WithLoreAndFiveCousins_AndAName([ValueSource(nameof(All))] Beast beast)
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

            CollectionAssert.Contains(OverworldBestiaryTexts.EnemyIds, beast.Id);
            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(beast.Id));
            Assert.IsNotNull(name, $"{beast.Id}: display name");
            Assert.AreEqual(Load(beast.Id).EnemyName, name.PtBR, $"{beast.Id}: pt-BR name is the asset's");
            Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), beast.Id);
        }

        [Test]
        public void EachOne_HasItsOwnProceduralModel_ThatBuildsAndTicks([ValueSource(nameof(All))] Beast beast)
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
        public void RollingStone_Rolls_WhileItTravels()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.RollingStone, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform body = rig.GetPart(ProceduralEnemyParts.Body);
            Quaternion rest = body.localRotation;
            rig.MoveSpeed01 = 1f;
            rig.Tick(0.2f);
            Assert.Greater(Quaternion.Angle(rest, body.localRotation), 10f, "a boulder at full speed turns over visibly");
        }

        [Test]
        public void SandSnake_Slithers_AndOwnsItsTail()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.SandSnake, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform tail = rig.GetPart("Tail_2");
            Quaternion rest = tail.localRotation;
            rig.Tick(0.37f);
            Assert.AreNotEqual(rest, tail.localRotation, "the tail waves");
            CollectionAssert.Contains(ProceduralEnemyRig.PartsAnimatedBy(ProceduralEnemyKind.SandSnake), ProceduralEnemyParts.Tail,
                "the stepping-legs driver leaves the slithering tail to the rig");
        }

        private static string BiomePrefix(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Forest: return "forest";
                case BiomeType.Desert: return "desert";
                case BiomeType.Mountain: return "mountain";
                default: return "cave";
            }
        }
    }
}
