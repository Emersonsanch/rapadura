using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The [distância] commons of the playable maps (Docs/Mapas100.md 07 Abutre de Brasa, 08 Lanterna Quebrada dos
    /// Artífices, 13 Gavião-de-Penacho, 19 Peixe-Lanterna Apagado, 24 Corvo-da-Treva): real definitions, not bosses, shooting
    /// (<see cref="EnemyDefinition.IsRanged"/>) from farther than any melee swing, in their biome (bestiary + loot table)
    /// and placed in the clearings of 07/08/13/14/19/20.
    /// </summary>
    public class RangedEnemiesTests
    {
        private const string Vulture = "enemy_desert_ember_vulture";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Fish = "enemy_cave_lantern_fish";
        private const string Crow = "enemy_cave_dark_crow";

        private static readonly (string id, BiomeType biome, EnemyProjectileKind projectile, ProceduralEnemyKind model)[] Ranged =
        {
            (Vulture, BiomeType.Desert, EnemyProjectileKind.Ember, ProceduralEnemyKind.EmberVulture),
            (Lantern, BiomeType.Desert, EnemyProjectileKind.LanternBolt, ProceduralEnemyKind.BrokenLantern),
            (Hawk, BiomeType.Mountain, EnemyProjectileKind.Feather, ProceduralEnemyKind.CrestedHawk),
            (Fish, BiomeType.Caves, EnemyProjectileKind.LureGlob, ProceduralEnemyKind.LanternFish),
            // Ninho de Mahaway (24): throws its feathers like the hawk (same shot and sound), placed by the Limiar overworld.
            (Crow, BiomeType.Caves, EnemyProjectileKind.Feather, ProceduralEnemyKind.DarkCrow),
            // Picos de Farnkesh (53, 57): the flute bird's and the storm eagle's stiff feathers (FarnkeshBeastsTests; placed later).
            ("enemy_forest_flute_bird", BiomeType.Forest, EnemyProjectileKind.Feather, ProceduralEnemyKind.FluteBird),
            ("enemy_mountain_storm_eagle", BiomeType.Mountain, EnemyProjectileKind.Feather, ProceduralEnemyKind.StormEagle),
            // Mar Gelado (59, 61, 63): the ice gull's frozen feathers and the twig bird's sticks (MarGeladoBeastsTests; placed by
            // the Mar Gelado overworld, MarGeladoBeastsPlacementTests).
            ("enemy_mountain_ice_gull", BiomeType.Mountain, EnemyProjectileKind.Feather, ProceduralEnemyKind.IceGull),
            ("enemy_mountain_twig_bird", BiomeType.Mountain, EnemyProjectileKind.Feather, ProceduralEnemyKind.TwigBird),
            // Hinokura (65, 68): the ember gull and the ember coral spit embers like the vulture (HinokuraBeastsTests; placed by
            // the Hinokura overworld, HinokuraBeastsPlacementTests).
            ("enemy_mountain_ember_gull", BiomeType.Mountain, EnemyProjectileKind.Ember, ProceduralEnemyKind.EmberGull),
            ("enemy_mountain_ember_coral", BiomeType.Mountain, EnemyProjectileKind.Ember, ProceduralEnemyKind.EmberCoral),
            // Margens da Mistura (79, 82): the tar heron's glob of pitch and the broken tablet's bolt of words (MisturaBeastsTests; placed by the Margens overworld, MisturaBeastsPlacementTests).
            ("enemy_forest_tar_heron", BiomeType.Forest, EnemyProjectileKind.LureGlob, ProceduralEnemyKind.TarHeron),
            ("enemy_forest_broken_tablet", BiomeType.Forest, EnemyProjectileKind.LanternBolt, ProceduralEnemyKind.BrokenTablet),
            // Limiar Profundo (84, 86): the smoke archer's bolt and the wind harpy's thrown feather (ProfundoBeastsTests).
            ("enemy_forest_smoke_archer", BiomeType.Forest, EnemyProjectileKind.LanternBolt, ProceduralEnemyKind.SmokeArcher),
            ("enemy_forest_wind_harpy", BiomeType.Forest, EnemyProjectileKind.Feather, ProceduralEnemyKind.WindHarpy),
            // Reinos do Céu (94, 97): the cracked jar leaks a bolt of light, the gull spits its last ember (CeuBeastsTests; not placed yet).
            ("enemy_forest_cracked_jar", BiomeType.Forest, EnemyProjectileKind.LanternBolt, ProceduralEnemyKind.CrackedJar),
            ("enemy_forest_dimmed_light_gull", BiomeType.Forest, EnemyProjectileKind.Ember, ProceduralEnemyKind.DimmedLightGull),
        };

        /// <summary>Map → the [distância] enemies its clearings must spawn (by day).</summary>
        private static readonly Dictionary<string, string[]> ExpectedByMap = new Dictionary<string, string[]>
        {
            { MapLayouts.CaravanasId, new[] { Vulture } },
            { MapLayouts.ArtificesId, new[] { Lantern } },
            { MapLayouts.RondaveisId, new[] { Hawk } },
            { MapLayouts.HahyaId, new[] { Hawk } },
            { MapLayouts.LagoLimiarId, new[] { Fish } },
            { MapLayouts.TagesId, new[] { Fish, Lantern } },
        };

        /// <summary>
        /// Overworld-only maps (no layout of their own: their clearings are the region's) → the [distância] enemies that lead a
        /// [distância] group there by day (they may also come along as companions in other groups).
        /// </summary>
        private static readonly (string region, string map, string[] enemies)[] ExpectedByOverworldMap =
        {
            (OverworldLayouts.MarGeladoRegionId, OverworldLayouts.PortoLuaBrancaMapId, new[] { "enemy_mountain_ice_gull" }),
            (OverworldLayouts.MarGeladoRegionId, OverworldLayouts.TundraOmolMapId, new[] { "enemy_mountain_twig_bird" }),
            (OverworldLayouts.MarGeladoRegionId, OverworldLayouts.MarAurorasMapId, new[] { "enemy_mountain_ice_gull" }),
            (OverworldLayouts.HinokuraRegionId, OverworldLayouts.HinokuraMapId, new[] { "enemy_mountain_ember_gull" }),
            (OverworldLayouts.HinokuraRegionId, OverworldLayouts.RecifeObsidianaMapId, new[] { "enemy_mountain_ember_coral" }),
            (OverworldLayouts.MisturaRegionId, OverworldLayouts.PocosBetumeMapId, new[] { "enemy_forest_tar_heron" }),
            (OverworldLayouts.MisturaRegionId, OverworldLayouts.TabuasSonhoMapId, new[] { "enemy_forest_broken_tablet" }),
            (OverworldLayouts.ProfundoRegionId, OverworldLayouts.CovaFumacaMapId, new[] { "enemy_forest_smoke_archer" }),
            (OverworldLayouts.ProfundoRegionId, OverworldLayouts.CovaVentoMapId, new[] { "enemy_forest_wind_harpy" }),
        };

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp() => MapLayouts.ClearCache();

        [TearDown]
        public void TearDown()
        {
            MapLayouts.ClearCache();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, $"{id}: not in Resources/Enemies");
            return definition;
        }

        [Test]
        public void Definitions_Exist_AreCommons_AndShootTheirOwnProjectile()
        {
            foreach (var (id, _, projectile, _) in Ranged)
            {
                EnemyDefinition definition = Load(id);
                Assert.IsFalse(definition.IsBoss, id);
                Assert.IsTrue(definition.IsRanged, id);
                Assert.AreEqual(projectile, definition.Projectile, id);
                Assert.AreEqual(EnemyMotionArchetype.Flyer, definition.MotionArchetype, $"{id}: hovers like the other flyers");
                Assert.Greater(definition.AttackDamage, 0f, id);
                Assert.Greater(definition.MaxHealth, 0f, id);
                Assert.Greater(definition.ExperienceReward, 0, id);
                Assert.GreaterOrEqual(definition.DetectionRadius, definition.AttackRange, $"{id}: notices the hero before it can shoot");
                Assert.Greater(definition.LoseTargetRadius, definition.AttackRange, $"{id}: does not lose the hero while shooting");
            }
        }

        [Test]
        public void AttackRange_IsWellBeyondEveryMeleeSwing()
        {
            EnemyDefinition[] all = Resources.LoadAll<EnemyDefinition>(EncounterPayload.EnemyResourcesFolder);
            float longestSwing = all.Where(e => !e.IsRanged).Max(e => e.AttackRange);
            foreach (var (id, _, _, _) in Ranged)
            {
                EnemyDefinition definition = Load(id);
                Assert.Greater(definition.AttackRange, longestSwing * 2f, $"{id}: shoots from range");
                Assert.That(definition.AttackRange, Is.InRange(6f, 10f), $"{id}: readable on the isometric camera");
            }

            CollectionAssert.AreEquivalent(Ranged.Select(r => r.id), all.Where(e => e.IsRanged).Select(DefeatedEnemyRegistry.GetId),
                "Only the [distância] commons shoot (melee enemies keep EnemyProjectileKind.None)");
        }

        [Test]
        public void EachOne_LivesInItsBiome_HasABestiaryPage_AndAProceduralModel()
        {
            BestiaryCatalog.Invalidate();
            BiomeRegistry.Invalidate();
            foreach (var (id, biome, _, model) in Ranged)
            {
                EnemyDefinition definition = Load(id);
                BiomeDefinition home = BiomeRegistry.FindBiomeOf(definition);
                Assert.IsNotNull(home, $"{id}: in a biome table (loot + bestiary region)");
                Assert.AreEqual(biome, home.Type, id);
                Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == definition).Weight, $"{id}: weight 0, never rolled by world-map encounters");

                BestiaryEntry page = BestiaryCatalog.Get(id);
                Assert.IsNotNull(page, $"{id}: bestiary page");
                Assert.IsTrue(page.Tips.Any(t => t.Kind == BestiaryTipKind.Ranged && Mathf.Approximately(t.Value, definition.AttackRange)), $"{id}: the [distância] tip");
                Assert.IsNotEmpty(page.Drops, $"{id}: drops from its biome's loot table");

                Assert.AreEqual(model, ProceduralEnemyModels.KindFor(id), id);
                Assert.IsFalse(ProceduralEnemyModels.IsNightSpecies(model), $"{id}: a day enemy (no night voice)");
            }
        }

        [Test]
        public void Launch_BuildsAFlyingShot_AimedAtTheTarget_AndNoneBuildsNothing()
        {
            var hero = new GameObject("Hero");
            _created.Add(hero);
            hero.transform.position = new Vector3(0f, 0f, 8f);

            Assert.IsNull(EnemyRangedAttack.Launch(EnemyProjectileKind.None, Vector3.up, hero.transform, null, 10f, 8f));
            foreach (var (_, _, projectile, _) in Ranged)
            {
                DungeonProjectile shot = EnemyRangedAttack.Launch(projectile, Vector3.up * EnemyRangedAttack.MuzzleHeight, hero.transform, null, 10f, 8f);
                Assert.IsNotNull(shot, projectile.ToString());
                _created.Add(shot.gameObject);
                Assert.Greater(shot.transform.childCount, 0, $"{projectile}: has a visible body");
                Assert.Greater(Vector3.Dot(shot.transform.forward, Vector3.forward), 0.9f, $"{projectile}: flies at the hero");
                Assert.Greater(EnemyRangedAttack.SpeedOf(projectile), 0f, projectile.ToString());
            }
        }

        [Test]
        public void Clearings_Of07_08_13_14_19_20_SpawnTheirRangedEnemies()
        {
            foreach (KeyValuePair<string, string[]> map in ExpectedByMap)
            {
                MapLayout layout = MapLayouts.For(map.Key);
                Assert.IsNotNull(layout, map.Key);
                foreach (string enemyId in map.Value)
                {
                    List<MapSpawnZone> zones = layout.Zones.Where(z => z.Day.Any(e => e.EnemyId == enemyId)).ToList();
                    Assert.IsNotEmpty(zones, $"{map.Key}: no clearing spawns {enemyId} by day");
                    foreach (MapSpawnZone zone in zones)
                    {
                        StringAssert.Contains("[distância]", zone.Archetypes, $"{map.Key}/{zone.Id}: tagged [distância]");
                    }
                }
            }
        }

        [Test]
        public void OverworldClearings_OfTheFrozenSea_SpawnTheirRangedEnemies()
        {
            OverworldLayouts.ClearCache();
            foreach (var (region, map, enemies) in ExpectedByOverworldMap)
            {
                OverworldLayout w = OverworldLayouts.Get(region);
                Assert.IsNotNull(w, region);
                List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
                Assert.IsNotEmpty(clearings, $"{map}: has clearings in {region}");
                foreach (string enemyId in enemies)
                {
                    List<MapSpawnZone> zones = clearings.Where(z => z.Day.Any(e => e.EnemyId == enemyId)).ToList();
                    Assert.IsNotEmpty(zones, $"{map}: no clearing spawns {enemyId} by day");
                    Assert.IsTrue(zones.Any(z => z.Archetypes != null && z.Archetypes.Contains("[distância]")),
                        $"{map}: {enemyId} leads at least one [distância] group");
                }
            }
        }

        [Test]
        public void ProceduralModels_HaveTheirAnimatedParts()
        {
            foreach (var (id, _, _, model) in Ranged)
            {
                GameObject visual = ProceduralEnemyModels.Instantiate(model, null);
                _created.Add(visual);
                var rig = visual.GetComponent<ProceduralEnemyRig>();
                Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Body), $"{id}: Body");
                bool lit = model == ProceduralEnemyKind.BrokenLantern || model == ProceduralEnemyKind.LanternFish;
                Assert.AreEqual(lit, rig.GetPart(ProceduralEnemyParts.Lantern) != null, $"{id}: the light it shoots comes from a Lantern part");
                Assert.DoesNotThrow(() => rig.Tick(0.1f), id);
            }
        }
    }
}
