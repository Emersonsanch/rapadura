using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 04, 05, 09, 10, 15, 22, 25, 26, 32, 33, 34, 37, 38, 39, 42, 43, 45, 48, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>): kept by the world bosses'
    /// director and fought with their abilities, each with an ability of its own. Pure rules and data: the procedural models are built with the
    /// runtime's own components (no MonoBehaviour of this assembly is added).
    /// </summary>
    public class MapBossTests
    {
        /// <summary>The map bosses on the road of the middle story (MidStoryCatalog, Regions 5–8: 25, 26, 32, 37, 38, 43).</summary>
        private static readonly string[] MidIds =
        {
            RegionBossRules.MonsoonOctopusId, RegionBossRules.SmokeAurochsId, RegionBossRules.ForgetfulBridgeId,
            RegionBossRules.RunawayCartId, RegionBossRules.GreatAlmasId, RegionBossRules.UnleapedCarpId,
        };

        /// <summary>The map bosses of the maps every player walks (Regions 1–3: 04, 09, 15) and of the three left out of the middle story's leva (34, 45, 48).</summary>
        private static readonly string[] FirstIds =
        {
            RegionBossRules.ForgottenSpinnerId, RegionBossRules.WellDrinkingRootId, RegionBossRules.GreatSmudgeId,
            RegionBossRules.NinkiNankaId, RegionBossRules.NianId, RegionBossRules.OldKurganGriffinId,
        };

        /// <summary>Six more, one in each of Regions 1, 2, 4, 6, 7 and 8 (05, 10, 22, 33, 39, 42).</summary>
        private static readonly string[] MoreIds =
        {
            RegionBossRules.MistElderId, RegionBossRules.DuneChoirId, RegionBossRules.GreedyBottlerId,
            RegionBossRules.LightFisherId, RegionBossRules.VaneLordId, RegionBossRules.LittleEclipseId,
        };

        private static readonly string[] Ids =
        {
            RegionBossRules.RopeCutterId, RegionBossRules.EndlessBellId, RegionBossRules.WalrusQueenId,
            RegionBossRules.GrandfatherMammothId, RegionBossRules.ShisaKingId, RegionBossRules.FurnaceMasterId,
            RegionBossRules.MonsoonOctopusId, RegionBossRules.SmokeAurochsId, RegionBossRules.ForgetfulBridgeId,
            RegionBossRules.RunawayCartId, RegionBossRules.GreatAlmasId, RegionBossRules.UnleapedCarpId,
            RegionBossRules.ForgottenSpinnerId, RegionBossRules.WellDrinkingRootId, RegionBossRules.GreatSmudgeId,
            RegionBossRules.NinkiNankaId, RegionBossRules.NianId, RegionBossRules.OldKurganGriffinId,
            RegionBossRules.MistElderId, RegionBossRules.DuneChoirId, RegionBossRules.GreedyBottlerId,
            RegionBossRules.LightFisherId, RegionBossRules.VaneLordId, RegionBossRules.LittleEclipseId,
        };

        /// <summary>The maps the story walks through in the 150–300 band (LateStoryCatalog) that have a boss in their ficha.</summary>
        private static readonly string[] LateStoryMaps =
        {
            OverworldLayouts.PontesVentoMapId, OverworldLayouts.ErmidasMapId, OverworldLayouts.CanaisGeloMapId,
            OverworldLayouts.GaleriasMamutesMapId, OverworldLayouts.JardinsLavaMapId, OverworldLayouts.NoveFornalhasMapId,
        };

        /// <summary>The maps of the middle story's steps (MidStoryCatalog) in Regions 5–8 whose ficha has a boss.</summary>
        private static readonly string[] MidStoryMaps =
        {
            OverworldLayouts.TaaPwaniMapId, OverworldLayouts.AuroquesId, OverworldLayouts.RaizVivaId,
            OverworldLayouts.CalhasId, OverworldLayouts.GeleiraId, OverworldLayouts.CanalLanternasMapId,
        };

        /// <summary>The maps of Regions 1–3 whose ficha has a boss the campaign does not already fight there, and 34, 45, 48.</summary>
        private static readonly string[] FirstMaps =
        {
            MapLayouts.AmoreirasId, OverworldLayouts.KarezId, OverworldLayouts.AbrigosId,
            OverworldLayouts.NinkiNankaId, OverworldLayouts.MuralhaMapId, OverworldLayouts.KurgansMapId,
        };

        /// <summary>The maps of Regions 1–4 where the campaign already has its boss (Kök-Böri 02, O Grande Ushabti 08, Hahya 14, Tages 20).</summary>
        private static readonly int[] CampaignBossMaps = { 2, 8, 14, 20 };

        /// <summary>The maps of the six more (05, 10, 22, 33, 39, 42), each in a region the earlier levas left without one of its own.</summary>
        private static readonly string[] MoreMaps =
        {
            MapLayouts.GarajonayId, OverworldLayouts.DunasId, OverworldLayouts.MercadoTuneisMapId,
            OverworldLayouts.TarchonMapId, OverworldLayouts.ForteVentosId, OverworldLayouts.TorresBrancasMapId,
        };

        private static readonly string[] StoryMaps = LateStoryMaps.Concat(MidStoryMaps).Concat(FirstMaps).Concat(MoreMaps).ToArray();

        private const float WalkableDegrees = 45f;

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
            Assert.IsNotNull(definition, id);
            return definition;
        }

        // ---------------------------------------------------------------- the list

        [Test]
        public void MapBosses_AreKeptByTheDirector_ButStayOutOfTheWorldBossList()
        {
            CollectionAssert.AreEquivalent(Ids, RegionBossRules.MapSpawns.Select(s => s.EnemyId));
            CollectionAssert.AreEquivalent(StoryMaps, RegionBossRules.MapSpawns.Select(s => s.MapId));
            CollectionAssert.IsEmpty(RegionBossRules.AllSpawns.Select(s => s.EnemyId).Intersect(Ids), "no keepsake, trophy or progression step of a world boss");
            Assert.AreEqual(RegionBossRules.AllSpawns.Count + RegionBossRules.MapSpawns.Count, RegionBossRules.DirectorSpawns.Count);
            CollectionAssert.AllItemsAreUnique(RegionBossRules.DirectorSpawns.Select(s => s.EnemyId));
            foreach (string id in Ids)
            {
                Assert.IsTrue(RegionBossRules.IsRegionBoss(id), id);
                Assert.IsTrue(RegionBossRules.IsMapBoss(id), id);
                Assert.AreSame(RegionBossRules.MapSpawns.Single(s => s.EnemyId == id), RegionBossRules.SpawnOf(id), id);
            }

            Assert.IsFalse(RegionBossRules.IsMapBoss(RegionBossRules.OhyaId));
        }

        [Test]
        public void TheStoryMapsOfTheBand_AreTheOnesThatGotABoss()
        {
            foreach (string mapId in LateStoryMaps)
            {
                MapDefinition map = MapCatalog.Get(mapId);
                Assert.IsNotNull(map, mapId);
                Assert.That(map.MonsterLevelMax, Is.InRange(150, 300), mapId);
            }
        }

        [Test]
        public void TheMiddleStoryWalksThroughEachMidMapBoss_AndNoneOfThemHadOne()
        {
            foreach (string mapId in MidStoryMaps)
            {
                MapDefinition map = MapCatalog.Get(mapId);
                Assert.IsNotNull(map, mapId);
                Assert.That(map.MonsterLevelMax, Is.InRange(45, 150), mapId);
                Assert.IsTrue(MidStoryCatalog.Steps.Any(step => step.MapId == mapId), $"{mapId}: a step of the middle story is there");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == mapId), $"{mapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == mapId), mapId);
            }

            CollectionAssert.AreEquivalent(MidIds, RegionBossRules.MapSpawns.Where(s => MidStoryMaps.Contains(s.MapId)).Select(s => s.EnemyId));
        }

        [Test]
        public void TheFirstRegionsAndTheLeftOutMaps_GetTheirFichaBoss_WhereNoOtherBossStands()
        {
            foreach (string mapId in FirstMaps)
            {
                MapDefinition map = MapCatalog.Get(mapId);
                Assert.IsNotNull(map, mapId);
                CollectionAssert.DoesNotContain(CampaignBossMaps, map.MapId, $"{mapId}: the campaign already has its boss there");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == mapId), $"{mapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == mapId), mapId);
            }

            foreach (string mapId in FirstMaps.Take(3))
            {
                Assert.That(MapCatalog.Get(mapId).MonsterLevelMax, Is.InRange(1, 44), $"{mapId}: Regions 1–3, before the middle story");
            }

            CollectionAssert.AreEquivalent(FirstIds, RegionBossRules.MapSpawns.Where(s => FirstMaps.Contains(s.MapId)).Select(s => s.EnemyId));
        }

        [Test]
        public void TheSixMoreMaps_GetTheirFichaBoss_OnePerRegion_WhereNoOtherBossStands()
        {
            foreach (string mapId in MoreMaps)
            {
                MapDefinition map = MapCatalog.Get(mapId);
                Assert.IsNotNull(map, mapId);
                CollectionAssert.DoesNotContain(CampaignBossMaps, map.MapId, $"{mapId}: the campaign already has its boss there");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == mapId), $"{mapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == mapId), mapId);
                Assert.That(map.MonsterLevelMax, Is.InRange(1, 149), $"{mapId}: Regions 1–8, below the band of the letters");
            }

            CollectionAssert.AllItemsAreUnique(MoreMaps.Select(m => MapCatalog.Get(m).Region), "one per region");
            CollectionAssert.AreEquivalent(MoreIds, RegionBossRules.MapSpawns.Where(s => MoreMaps.Contains(s.MapId)).Select(s => s.EnemyId));
        }

        [Test]
        public void TheLodo_StillWaitsForItsFiveFountains_TheMapBossesDoNot()
        {
            RegionBossSpawn lodo = RegionBossRules.SpawnOf(RegionBossRules.SwallowingMireId);
            Assert.IsFalse(RegionBossRules.SummonReady(lodo, LodoFountainRules.Count - 1));
            Assert.IsTrue(RegionBossRules.SummonReady(lodo, LodoFountainRules.Count));
            foreach (RegionBossSpawn spawn in RegionBossRules.MapSpawns)
            {
                Assert.IsTrue(RegionBossRules.SummonReady(spawn, 0), spawn.EnemyId);
            }

            Assert.Less(RegionBossRules.SleepRadius, OverworldRules.UnloadRadius);
        }

        // ---------------------------------------------------------------- the asset

        [TestCaseSource(nameof(Ids))]
        public void Asset_IsABoss_AtTheTopOfItsMap_WithLootThatExists(string id)
        {
            EnemyDefinition boss = Load(id);
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            Assert.IsTrue(boss.IsBoss, id);
            Assert.AreEqual(spawn.Level, boss.Level, id);
            Assert.AreEqual(MapCatalog.Get(spawn.MapId).MonsterLevelMax, spawn.Level, $"{id}: the top of its map's band");
            Assert.IsNotNull(boss.LootItem, id);
            Assert.IsNotNull(ItemDatabase.GetById(boss.LootItem.ItemId), $"{id}: {boss.LootItem.ItemId} is in Resources/Items");
            Assert.That(boss.LootDropChance, Is.InRange(0.01f, 0.99f), id);
            Assert.GreaterOrEqual(boss.LootQuantityMax, boss.LootQuantityMin, id);
            Assert.IsTrue(BossBonusRules.HasBonusDrop(boss), id);
            Assert.Greater(boss.BossExperienceMultiplier, 1, id);
            StringAssert.StartsWith("enemy_mountain_", id, "BalanceInvariantsTests' naming");
            Assert.LessOrEqual(boss.MaxHealth, Load("enemy_cave_dweller").MaxHealth, $"{id}: the level scaler does the rest (BossHealth_DoesNotDropInLaterBiomes)");
        }

        [Test]
        public void MapBosses_AreWeakerThanTheWorldBossOfTheirRegion()
        {
            Assert.Less(Load(RegionBossRules.RopeCutterId).MaxHealth, Load(RegionBossRules.WindKingId).MaxHealth);
            Assert.Less(Load(RegionBossRules.EndlessBellId).MaxHealth, Load(RegionBossRules.WindKingId).MaxHealth);
            Assert.Less(Load(RegionBossRules.WalrusQueenId).MaxHealth, Load(RegionBossRules.OldKulId).MaxHealth);
            Assert.Less(Load(RegionBossRules.GrandfatherMammothId).MaxHealth, Load(RegionBossRules.OldKulId).MaxHealth);
            Assert.Less(Load(RegionBossRules.ShisaKingId).MaxHealth, Load(RegionBossRules.EmberGluttonId).MaxHealth);
            Assert.Less(Load(RegionBossRules.FurnaceMasterId).MaxHealth, Load(RegionBossRules.EmberGluttonId).MaxHealth);
            Assert.Less(Load(RegionBossRules.MonsoonOctopusId).MaxHealth, Load(RegionBossRules.HedammuId).MaxHealth);
            Assert.Less(Load(RegionBossRules.SmokeAurochsId).MaxHealth, Load(RegionBossRules.HedammuId).MaxHealth);
            Assert.Less(Load(RegionBossRules.ForgetfulBridgeId).MaxHealth, Load(RegionBossRules.SwallowingMireId).MaxHealth);
            Assert.Less(Load(RegionBossRules.RunawayCartId).MaxHealth, Load(RegionBossRules.AydaharId).MaxHealth);
            Assert.Less(Load(RegionBossRules.GreatAlmasId).MaxHealth, Load(RegionBossRules.AydaharId).MaxHealth);
            Assert.Less(Load(RegionBossRules.UnleapedCarpId).MaxHealth, Load(RegionBossRules.FourGluttonsId).MaxHealth);
            // Regions 1–3: their world boss is the scheduled event (WorldBossRules: one common of the region ×15).
            Assert.Less(Load(RegionBossRules.ForgottenSpinnerId).MaxHealth, EventWorldBossHealth(BiomeType.Forest));
            Assert.Less(Load(RegionBossRules.WellDrinkingRootId).MaxHealth, EventWorldBossHealth(BiomeType.Desert));
            Assert.Less(Load(RegionBossRules.GreatSmudgeId).MaxHealth, EventWorldBossHealth(BiomeType.Mountain));
            Assert.Less(Load(RegionBossRules.NinkiNankaId).MaxHealth, Load(RegionBossRules.SwallowingMireId).MaxHealth);
            Assert.Less(Load(RegionBossRules.NianId).MaxHealth, Load(RegionBossRules.FourGluttonsId).MaxHealth);
            Assert.Less(Load(RegionBossRules.OldKurganGriffinId).MaxHealth, Load(RegionBossRules.OhyaId).MaxHealth);
            Assert.Less(Load(RegionBossRules.NinkiNankaId).AttackDamage, Load(RegionBossRules.SwallowingMireId).AttackDamage);
            Assert.Less(Load(RegionBossRules.NianId).AttackDamage, Load(RegionBossRules.FourGluttonsId).AttackDamage);
            Assert.Less(Load(RegionBossRules.OldKurganGriffinId).AttackDamage, Load(RegionBossRules.OhyaId).AttackDamage);
            foreach (string id in FirstIds)
            {
                Assert.Less(Load(id).MaxHealth, 1180, $"{id}: below the strongest boss of the first regions (Tages)");
            }

            // The six more: Regions 1, 2 and 4 against their scheduled event, 6, 7 and 8 against their world boss.
            Assert.Less(Load(RegionBossRules.MistElderId).MaxHealth, EventWorldBossHealth(BiomeType.Forest));
            Assert.Less(Load(RegionBossRules.DuneChoirId).MaxHealth, EventWorldBossHealth(BiomeType.Desert));
            Assert.Less(Load(RegionBossRules.GreedyBottlerId).MaxHealth, EventWorldBossHealth(BiomeType.Caves));
            Assert.Less(Load(RegionBossRules.LightFisherId).MaxHealth, Load(RegionBossRules.SwallowingMireId).MaxHealth);
            Assert.Less(Load(RegionBossRules.VaneLordId).MaxHealth, Load(RegionBossRules.AydaharId).MaxHealth);
            Assert.Less(Load(RegionBossRules.LittleEclipseId).MaxHealth, Load(RegionBossRules.FourGluttonsId).MaxHealth);
            Assert.Less(Load(RegionBossRules.LightFisherId).AttackDamage, Load(RegionBossRules.SwallowingMireId).AttackDamage);
            Assert.Less(Load(RegionBossRules.VaneLordId).AttackDamage, Load(RegionBossRules.AydaharId).AttackDamage);
            Assert.Less(Load(RegionBossRules.LittleEclipseId).AttackDamage, Load(RegionBossRules.FourGluttonsId).AttackDamage);
            foreach (string id in MoreIds)
            {
                Assert.Less(Load(id).MaxHealth, 1180, $"{id}: below the strongest boss of the first regions (Tages)");
            }

            foreach (string id in MidIds)
            {
                Assert.Less(Load(id).AttackDamage, Load(RegionBossRules.FourGluttonsId).AttackDamage, $"{id}: hits softer than the last world boss of Regions 5–8");
            }
            foreach (string id in Ids)
            {
                Assert.Less(Load(id).BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, id);
            }
        }

        private static float EventWorldBossHealth(BiomeType region) => Load(WorldBossRules.For(region).DayEnemyId).MaxHealth * WorldBossRules.HealthCommons;

        [TestCaseSource(nameof(Ids))]
        public void Biome_ListsIt_AtWeightZero_SoEncountersNeverRollIt(string id)
        {
            BiomeRegistry.Invalidate();
            EnemyDefinition boss = Load(id);
            BiomeDefinition home = BiomeRegistry.FindBiomeOf(boss);
            Assert.IsNotNull(home, id);
            Assert.AreEqual(BiomeType.Mountain, home.Type, id);
            Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == boss).Weight, id);
        }

        // ---------------------------------------------------------------- the model

        [TestCaseSource(nameof(Ids))]
        public void Model_IsABossModel_WithTheFightBuiltIn_AndStaysWithinTheBounds(string id)
        {
            ProceduralEnemyKind kind = ProceduralEnemyModels.KindFor(id);
            Assert.AreNotEqual(ProceduralEnemyKind.None, kind, id);
            Assert.IsTrue(ProceduralEnemyModels.IsBoss(kind), id);
            Assert.IsTrue(ProceduralEnemyModels.IsMapBoss(kind), id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kind);

            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            Assert.IsNotNull(visual.GetComponent<RegionBossAbilities>(), id);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Core), id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f), id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f, 0f), $"{id}: Reduzir Movimento");

            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds), id);
            Assert.That(bounds.size.x, Is.InRange(0.5f, 3.5f), $"{id}: width");
            Assert.That(bounds.size.z, Is.InRange(0.4f, 3.5f), $"{id}: depth");
            Assert.That(bounds.size.y, Is.InRange(2.4f, 4f), $"{id}: height");
            Assert.That(bounds.min.y, Is.InRange(-0.15f, 0.15f), $"{id}: feet at {bounds.min.y}");
            Assert.Less(Mathf.Abs(bounds.center.x), 0.3f, $"{id}: centred");

            ProceduralEnemyBlueprint blueprint = ProceduralEnemyModels.GetBlueprint(kind);
            Assert.Less(blueprint.TriangleCount, 8000, $"{id}: the boss budget");
            bool glows = blueprint.Meshes.Any(m => m.uv.Any(uv => EnemyModelPalette.IsGlow(EnemyModelPalette.SwatchAt(uv))));
            Assert.IsTrue(glows, $"{id}: at least one colour that glows");
        }

        [TestCaseSource(nameof(Ids))]
        public void Core_SwellsAsTheFightGoesOn(string id)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyModels.KindFor(id), null);
            _created.Add(visual);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform core = rig.GetPart(ProceduralEnemyParts.Core);
            rig.Health01Override = 1f;
            rig.Tick(0.1f, 0f);
            float calm = core.localScale.x;
            rig.Health01Override = 0.1f;
            rig.Tick(0.1f, 0f);
            Assert.Greater(core.localScale.x, calm, id);
        }

        [TestCase(ProceduralEnemyKind.EndlessBell)]
        [TestCase(ProceduralEnemyKind.WalrusQueen)]
        [TestCase(ProceduralEnemyKind.GrandfatherMammoth)]
        [TestCase(ProceduralEnemyKind.FurnaceMaster)]
        [TestCase(ProceduralEnemyKind.MonsoonOctopus)]
        [TestCase(ProceduralEnemyKind.ForgetfulBridge)]
        [TestCase(ProceduralEnemyKind.RunawayCart)]
        [TestCase(ProceduralEnemyKind.GreatAlmas)]
        [TestCase(ProceduralEnemyKind.UnleapedCarp)]
        [TestCase(ProceduralEnemyKind.ForgottenSpinner)]
        [TestCase(ProceduralEnemyKind.WellDrinkingRoot)]
        [TestCase(ProceduralEnemyKind.GreatSmudge)]
        [TestCase(ProceduralEnemyKind.NinkiNanka)]
        [TestCase(ProceduralEnemyKind.OldKurganGriffin)]
        [TestCase(ProceduralEnemyKind.GreedyBottler)]
        [TestCase(ProceduralEnemyKind.LightFisher)]
        [TestCase(ProceduralEnemyKind.VaneLord)]
        [TestCase(ProceduralEnemyKind.LittleEclipse)]
        public void TheOnesWithArms_RaiseThem_OnTheWindup(ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform arm = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion rest = arm.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            for (int i = 0; i < 20; i++)
            {
                rig.Tick(0.05f, 0f);
            }

            Assert.Greater(Quaternion.Angle(rest, arm.localRotation), 30f, kind.ToString());
        }

        // ---------------------------------------------------------------- the page

        [TestCaseSource(nameof(Ids))]
        public void Page_HasLoreFiveCousinsTheFightLinesAndAName(string id)
        {
            BestiaryCatalog.Invalidate();
            BestiaryEntry page = BestiaryCatalog.Get(id);
            Assert.IsNotNull(page, id);
            Assert.IsTrue(page.IsBoss, id);
            Assert.AreEqual(BiomeType.Mountain, page.Region, id);
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string>
            {
                page.LoreKey, RegionBossTexts.PhaseKey(id, RegionBossPhase.Storm), RegionBossTexts.PhaseKey(id, RegionBossPhase.LastStand),
                RegionBossTexts.GreatBlowKey(id), RegionBossTexts.SignatureKey(id), RegionBossTexts.WithdrawKey(id),
            };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key}: missing");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
            }

            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(id));
            Assert.IsNotNull(name, id);
            Assert.AreEqual(Load(id).EnemyName, name.PtBR, id);
            CollectionAssert.Contains(RegionBossTexts.EnemyIds, id);
        }

        [Test]
        public void ShisaKing_CallsOutEachMouth()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (ShisaMouth mouth in System.Enum.GetValues(typeof(ShisaMouth)))
            {
                Assert.IsTrue(table.TryGetValue(RegionBossTexts.ShisaMouthKey(mouth), out LocalizationEntry entry), mouth.ToString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), mouth.ToString());
            }
        }

        [Test]
        public void UnleapedCarp_CallsOutEachLeap_AndTheAlmasLabelsItsSparks()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (string key in new[] { RegionBossTexts.CarpLeapKey(true), RegionBossTexts.CarpLeapKey(false), RegionBossTexts.SnowSparkKey })
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            Assert.AreNotEqual(RegionBossTexts.CarpLeapKey(true), RegionBossTexts.CarpLeapKey(false));
        }

        [Test]
        public void GreatSmudge_CallsOutEachColour_AndNianItsFright()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = System.Enum.GetValues(typeof(SmudgeColour)).Cast<SmudgeColour>().Select(RegionBossTexts.SmudgeColourKey).ToList();
            keys.Add(RegionBossTexts.NianFrightKey);
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
        }

        [Test]
        public void TheSixMore_CallOutTheirVersesBottlesLureWindsAndCovers()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string> { RegionBossTexts.BottleKey, RegionBossTexts.LureKey };
            keys.AddRange(Enumerable.Range(0, RegionBossRules.DuneVerseCount).Select(RegionBossTexts.DuneVerseKey));
            keys.AddRange(System.Enum.GetValues(typeof(VaneWind)).Cast<VaneWind>().Select(RegionBossTexts.VaneWindKey));
            keys.AddRange(Enumerable.Range(0, RegionBossRules.EclipsePhases).Select(RegionBossTexts.EclipseCoverKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
            Assert.AreEqual(RegionBossTexts.DuneVerseKey(0), RegionBossTexts.DuneVerseKey(RegionBossRules.DuneVerseCount), "the verses come round");
            Assert.AreEqual(RegionBossTexts.EclipseCoverKey(0), RegionBossTexts.EclipseCoverKey(RegionBossRules.EclipsePhases), "the covers come round");
        }

        // ---------------------------------------------------------------- the arena

        [TestCaseSource(nameof(Ids))]
        public void Arena_IsInItsArea_OnARing_WithNoClearingOnIt(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            OverworldLayout world = OverworldLayouts.StitchedWorld;
            Vector2? found = RegionBossRules.ArenaIn(world, spawn);
            Assert.IsNotNull(found, $"{id}: the arena is not in its area");
            Vector2 arena = found.Value;
            Assert.AreEqual(spawn.LocalArena + spawn.WorldOffset, arena, id);
            Assert.AreEqual(spawn.MapId, OverworldRules.ZoneAt(world, arena).MapIdOrOwner, $"{id}: an area of its own map");
            // On foot, nothing asked beyond what the whole map asks (the Muralha das Mil Torres, 45, wants its lamp in every one of its areas).
            string tag = OverworldRules.ZoneAt(world, arena).RequirementTag;
            if (!string.IsNullOrEmpty(tag))
            {
                Assert.AreEqual(MapCatalog.Get(spawn.MapId).SpecialRequirementTag, tag, $"{id}: on foot, nothing asked but the map's own");
                Assert.IsTrue(world.Zones.Where(z => z.MapIdOrOwner == spawn.MapId).All(z => z.RequirementTag == tag), $"{id}: no area of its map asks less");
            }
            Assert.IsTrue(world.World.Landmarks.Any(l => l.Kind == MapLandmarkKind.StoneRing && Vector2.Distance(l.Position, arena) < 1f), $"{id}: a stone ring marks the arena");
            foreach (MapSpawnZone clearing in world.World.Zones)
            {
                Assert.GreaterOrEqual(Vector2.Distance(clearing.Center, arena), RegionBossRules.ArenaClearRadius, $"{id}: {clearing.Id} is on the arena");
            }

            Assert.IsFalse(MapLayoutRules.InRiver(world.World, arena, 8f), $"{id}: dry ground");
            Assert.IsFalse(world.Settlements.Any(s => s.GroundRect.Contains(arena)), $"{id}: not in a settlement");
            foreach (RegionBossSpawn other in RegionBossRules.DirectorSpawns.Where(s => s != spawn))
            {
                Vector2? there = RegionBossRules.ArenaIn(world, other);
                if (there.HasValue)
                {
                    Assert.Greater(Vector2.Distance(there.Value, arena), RegionBossRules.SleepRadius, $"{id}: {other.EnemyId} would wake with it");
                }
            }
        }

        [TestCaseSource(nameof(Ids))]
        public void Arena_IsFlat_AndReachableOnFoot_FromATrail(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            OverworldLayout world = OverworldLayouts.StitchedWorld;
            Vector2 arena = RegionBossRules.ArenaIn(world, spawn).Value;
            OverworldRelief relief = OverworldRelief.For(world);
            float centre = relief.HeightAt(arena);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Assert.AreEqual(centre, relief.HeightAt(arena + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 9f), 0.6f, $"{id}: the ring is level");
            }

            Vector2 trail = world.World.Paths.SelectMany(p => new[] { p.A, p.B }).OrderBy(p => Vector2.Distance(p, arena)).First();
            Assert.Less(Vector2.Distance(trail, arena), 40f, $"{id}: the ring stands by a trail");
            float previous = relief.HeightAt(arena);
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(arena, trail) / 4f));
            for (int s = 1; s <= steps; s++)
            {
                Vector2 point = Vector2.Lerp(arena, trail, (float)s / steps);
                float height = relief.HeightAt(point);
                float run = Mathf.Max(0.01f, Vector2.Distance(arena, trail) / steps);
                Assert.Less(Mathf.Atan(Mathf.Abs(height - previous) / run) * Mathf.Rad2Deg, WalkableDegrees, $"{id}: steep at {point}");
                previous = height;
            }
        }

        // ---------------------------------------------------------------- the abilities

        [TestCaseSource(nameof(Ids))]
        public void EachHasAnAbilityOfItsOwn_FromTheStormPhase(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            Assert.AreNotEqual(RegionBossSignature.None, spawn.Signature, id);
            Assert.AreEqual(1, RegionBossRules.DirectorSpawns.Count(s => s.Signature == spawn.Signature), $"{id}: its own");
            Assert.IsFalse(RegionBossRules.SignatureEnabled(spawn.Signature, RegionBossPhase.Wrath), id);
            Assert.IsTrue(RegionBossRules.SignatureEnabled(spawn.Signature, RegionBossPhase.Storm), id);
            Assert.Greater(RegionBossRules.SignatureInterval(spawn.Signature), RegionBossRules.SignatureWindup(spawn.Signature), id);
            Assert.Greater(RegionBossRules.SignatureWindup(spawn.Signature), 1f, $"{id}: time to read the warning");
            Assert.IsTrue(spawn.NeverDies, $"{id}: freed, it leaves with its line");
            Assert.IsFalse(spawn.ReturnsNextNight, id);
        }

        [Test]
        public void RopeCutter_OnlyThrowsDownWhoeverKeptWalking()
        {
            Vector2 held = new Vector2(10f, 4f);
            Assert.IsFalse(RegionBossRules.RopeCutHits(held, held), "stopped and held on");
            Assert.IsFalse(RegionBossRules.RopeCutHits(held, held + Vector2.right * (RegionBossRules.RopeCutHoldTolerance - 0.05f)), "a sway is not a step");
            Assert.IsTrue(RegionBossRules.RopeCutHits(held, held + Vector2.up * (RegionBossRules.RopeCutHoldTolerance + 0.1f)));
            Assert.IsTrue(RegionBossRules.RopeCutHits(held, held + new Vector2(6f, 0f)), "running is no escape");
            Assert.Less(RegionBossRules.RopeCutHoldSeconds, RegionBossRules.RopeCutWindupSeconds, "the warning comes before the moment to stop");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.RopeCutterId).StrongerAtNight);
        }

        [Test]
        public void EndlessBell_TheEchoRingsWhereTheLastTollFell()
        {
            Vector2 mark = new Vector2(0f, 0f);
            Vector2 echo = new Vector2(8f, 0f);
            Assert.IsTrue(RegionBossRules.TollHits(mark, null, mark));
            Assert.IsFalse(RegionBossRules.TollHits(mark, null, echo), "the first toll has no echo");
            Assert.IsTrue(RegionBossRules.TollHits(mark, echo, echo), "going back to the old spot");
            Assert.IsTrue(RegionBossRules.TollHits(mark, echo, echo + Vector2.up * (RegionBossRules.TollRadius - 0.1f)));
            Assert.IsFalse(RegionBossRules.TollHits(mark, echo, new Vector2(4f, 3f)), "a new quiet place between them");
            Assert.IsFalse(RegionBossRules.TollHits(mark, echo, mark + Vector2.down * (RegionBossRules.TollRadius + 0.1f)));
        }

        [Test]
        public void WalrusQueen_HitsHardestInTheMiddle_OnlySplashesAtTheEdge()
        {
            Assert.AreEqual(RegionBossRules.FlopCentreFactor, RegionBossRules.FlopDamageFactor(0f), 1e-4f);
            Assert.AreEqual(RegionBossRules.FlopEdgeFactor, RegionBossRules.FlopDamageFactor(RegionBossRules.FlopRadius), 1e-4f);
            Assert.AreEqual(0f, RegionBossRules.FlopDamageFactor(RegionBossRules.FlopRadius + 0.1f));
            Assert.Greater(RegionBossRules.FlopDamageFactor(2f), RegionBossRules.FlopDamageFactor(5f), "walking out pays");
            Assert.Less(RegionBossRules.FlopEdgeFactor, RegionBossRules.FlopCentreFactor * 0.25f);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.WalrusQueenId).StrongerAtNight, "the ficha: the ice cracks more at night");
        }

        [Test]
        public void GrandfatherMammoth_TheStonesFallInACross_TheDiagonalsAreSafe()
        {
            Vector2 mark = new Vector2(20f, -5f);
            Vector2[] stones = RegionBossRules.CeilingStones(mark);
            Assert.AreEqual(5, stones.Length);
            CollectionAssert.AllItemsAreUnique(stones);
            Assert.IsTrue(RegionBossRules.CeilingFallHits(mark, mark), "standing still");
            Assert.IsTrue(RegionBossRules.CeilingFallHits(mark, mark + Vector2.right * RegionBossRules.CeilingStoneSpacing), "a step straight aside");
            Assert.IsTrue(RegionBossRules.CeilingFallHits(mark, mark + Vector2.down * RegionBossRules.CeilingStoneSpacing), "a step straight back");
            Assert.IsFalse(RegionBossRules.CeilingFallHits(mark, mark + new Vector2(3f, 3f)), "slantwise");
            Assert.IsFalse(RegionBossRules.CeilingFallHits(mark, mark + new Vector2(-3f, 3f)));
            Assert.IsFalse(RegionBossRules.CeilingFallHits(mark, mark + Vector2.right * (RegionBossRules.CeilingStoneSpacing + RegionBossRules.CeilingStoneRadius + 0.1f)), "well out");
            Assert.Greater(RegionBossRules.CeilingStoneSpacing, RegionBossRules.CeilingStoneRadius * 2f, "a gap between the stones");
        }

        [Test]
        public void ShisaKing_OpenMouthUpClose_ShutMouthFarAway_InTurn()
        {
            Assert.AreEqual(ShisaMouth.Open, RegionBossRules.ShisaMouthFor(0));
            Assert.AreEqual(ShisaMouth.Closed, RegionBossRules.ShisaMouthFor(1));
            Assert.AreEqual(ShisaMouth.Open, RegionBossRules.ShisaMouthFor(2));
            Assert.AreEqual(ShisaMouth.Closed, RegionBossRules.ShisaMouthFor(-1), "never throws on a bad count");
            Assert.IsFalse(RegionBossRules.ShisaHits(ShisaMouth.Open, 1f), "under the chin");
            Assert.IsTrue(RegionBossRules.ShisaHits(ShisaMouth.Open, RegionBossRules.ShisaReach + 0.1f));
            Assert.IsTrue(RegionBossRules.ShisaHits(ShisaMouth.Closed, RegionBossRules.ShisaReach));
            Assert.IsFalse(RegionBossRules.ShisaHits(ShisaMouth.Closed, RegionBossRules.ShisaReach + 0.1f), "far away");
            Assert.Less(RegionBossRules.ShisaReach, RegionBossRules.AbilityRange, "far away is still inside the fight");
        }

        [Test]
        public void FurnaceMaster_EachStrokeHotter_TheNinthVents()
        {
            Assert.AreEqual(1f, RegionBossRules.ForgeHeatMultiplier(0), 1e-4f);
            for (int i = 1; i < RegionBossRules.ForgeFurnaces; i++)
            {
                Assert.Greater(RegionBossRules.ForgeHeatMultiplier(i), RegionBossRules.ForgeHeatMultiplier(i - 1), i.ToString());
            }

            Assert.AreEqual(1f, RegionBossRules.ForgeHeatMultiplier(RegionBossRules.ForgeFurnaces), 1e-4f, "vented, it starts over");
            Assert.AreEqual(1f, RegionBossRules.ForgeHeatMultiplier(-3), 1e-4f);
            Assert.Less(RegionBossRules.ForgeHeatMultiplier(RegionBossRules.ForgeFurnaces - 1), 2f, "the hottest stroke stays readable");
            Vector2 mark = new Vector2(3f, 3f);
            Assert.IsTrue(RegionBossRules.ForgeStrokeHits(mark, mark + Vector2.left * (RegionBossRules.ForgeRadius - 0.1f)));
            Assert.IsFalse(RegionBossRules.ForgeStrokeHits(mark, mark + Vector2.left * (RegionBossRules.ForgeRadius + 0.1f)));
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.FurnaceMasterId).StrongerAtNight, "the ficha: the furnaces burn hotter at night");
        }

        [Test]
        public void MonsoonOctopus_TheGapsBetweenTheArms_TurnIntoArmsNextSlam()
        {
            Vector2 octopus = new Vector2(5f, -2f);
            Vector2 onAnArm = octopus + Vector2.right * 4f;
            Vector2 inAGap = octopus + new Vector2(Mathf.Cos(22.5f * Mathf.Deg2Rad), Mathf.Sin(22.5f * Mathf.Deg2Rad)) * 5f;
            Assert.IsTrue(RegionBossRules.OctopusArmsHit(0, octopus, onAnArm));
            Assert.IsFalse(RegionBossRules.OctopusArmsHit(0, octopus, inAGap), "between two arms");
            Assert.IsFalse(RegionBossRules.OctopusArmsHit(1, octopus, onAnArm), "the arms turned half a gap");
            Assert.IsTrue(RegionBossRules.OctopusArmsHit(1, octopus, inAGap), "the old gap is an arm now");
            Assert.IsTrue(RegionBossRules.OctopusArmsHit(0, octopus, octopus + new Vector2(1f, 0.6f)), "under its body everything is hit");
            Assert.IsTrue(RegionBossRules.OctopusArmsHit(0, octopus, octopus), "right underneath");
            Assert.IsFalse(RegionBossRules.OctopusArmsHit(0, octopus, octopus + Vector2.right * (RegionBossRules.ArmLength + 0.1f)), "out of reach");
            Assert.IsFalse(RegionBossRules.OctopusArmsHit(-3, octopus, onAnArm), "never throws on a bad count");
            Assert.Greater(4f * Mathf.Sin(22.5f * Mathf.Deg2Rad), RegionBossRules.ArmHalfWidth, "four metres out, a gap is wider than an arm");
            Assert.AreEqual(RegionBossRules.ArmTurnDegrees * 2f, 360f / RegionBossRules.OctopusArms, 1e-4f, "half a gap per slam");
        }

        [Test]
        public void SmokeAurochs_OnlyStraightBehindIsClean()
        {
            Vector2 bull = Vector2.zero;
            Vector2 facing = Vector2.up;
            Assert.IsTrue(RegionBossRules.SnortHits(bull, facing, new Vector2(0f, 4f)), "in front");
            Assert.IsTrue(RegionBossRules.SnortHits(bull, facing, new Vector2(4f, 0f)), "beside it");
            Assert.IsTrue(RegionBossRules.SnortHits(bull, facing, new Vector2(3f, -2f)), "behind but too far to the side");
            Assert.IsFalse(RegionBossRules.SnortHits(bull, facing, new Vector2(0f, -4f)), "straight behind, with the herd");
            Assert.IsFalse(RegionBossRules.SnortHits(bull, facing, new Vector2(1f, -4f)));
            Assert.IsFalse(RegionBossRules.SnortHits(bull, facing, new Vector2(0f, RegionBossRules.SnortRadius + 0.1f)), "far away");
            Assert.IsTrue(RegionBossRules.SnortHits(bull, Vector2.zero, new Vector2(0f, -4f)), "no facing: all round");
            Assert.IsTrue(RegionBossRules.SnortHits(bull, facing, new Vector2(0.1f, -0.2f)), "under its hooves");
        }

        [Test]
        public void ForgetfulBridge_EveryOtherPlank_SidewaysNeverHelps()
        {
            Vector2 bridge = new Vector2(0f, -6f);
            Vector2 mark = Vector2.zero;
            Assert.IsTrue(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark), "the hero's own plank first");
            Assert.IsTrue(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.right * 5f), "sideways along the same plank");
            Assert.IsFalse(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.up * RegionBossRules.PlankWidth), "one plank forward");
            Assert.IsFalse(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.down * RegionBossRules.PlankWidth), "one plank back");
            Assert.IsTrue(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.up * RegionBossRules.PlankWidth * 2f), "two planks on falls too");
            Assert.IsFalse(RegionBossRules.ForgottenPlankHits(1, bridge, mark, mark), "the next stomp forgets the other planks");
            Assert.IsTrue(RegionBossRules.ForgottenPlankHits(1, bridge, mark, mark + Vector2.up * RegionBossRules.PlankWidth));
            Assert.IsFalse(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.right * (RegionBossRules.PlankHalfLength + 0.1f)), "off the end of the plank");
            Assert.IsFalse(RegionBossRules.ForgottenPlankHits(0, bridge, mark, mark + Vector2.up * (RegionBossRules.PlanksReach + 0.1f)), "past the last plank");
            Assert.IsTrue(RegionBossRules.ForgottenPlankHits(0, mark, mark, mark), "the bridge on the mark still forgets");
        }

        [Test]
        public void RunawayCart_TheLongerTheRunUp_TheHarderItHits()
        {
            Assert.AreEqual(RegionBossRules.RunawayMinFactor, RegionBossRules.RunawayDamageFactor(0f), 1e-4f);
            Assert.AreEqual(RegionBossRules.RunawayMaxFactor, RegionBossRules.RunawayDamageFactor(RegionBossRules.RunawayFullRunUp), 1e-4f);
            Assert.AreEqual(RegionBossRules.RunawayMaxFactor, RegionBossRules.RunawayDamageFactor(RegionBossRules.AbilityRange), 1e-4f, "it never goes past its top speed");
            Assert.Greater(RegionBossRules.RunawayDamageFactor(12f), RegionBossRules.RunawayDamageFactor(4f), "staying close pays");
            Assert.Less(RegionBossRules.RunawayMinFactor, 1f, "a bump from close up");
            Assert.LessOrEqual(RegionBossRules.RunawayFullRunUp, RegionBossRules.AbilityRange);
            Vector2 mark = new Vector2(-3f, 8f);
            Assert.IsTrue(RegionBossRules.RunawayHits(mark, mark + Vector2.left * (RegionBossRules.RunawayRadius - 0.1f)));
            Assert.IsFalse(RegionBossRules.RunawayHits(mark, mark + Vector2.left * (RegionBossRules.RunawayRadius + 0.1f)), "a step out of the way");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.RunawayCartId).StrongerAtNight, "the ficha: the chutes freeze at night, faster");
        }

        [Test]
        public void GreatAlmas_OnlyTheHiddenSparksAreSafe_AndTheyMoveRound()
        {
            Vector2 almas = new Vector2(10f, 10f);
            Vector2[] first = RegionBossRules.SnowSparkSpots(almas, 0);
            Vector2[] second = RegionBossRules.SnowSparkSpots(almas, 1);
            Assert.AreEqual(RegionBossRules.SnowSparkCount, first.Length);
            CollectionAssert.AllItemsAreUnique(first);
            foreach (Vector2 spark in first)
            {
                Assert.AreEqual(RegionBossRules.SnowSparkDistance, Vector2.Distance(spark, almas), 1e-3f);
                Assert.IsFalse(RegionBossRules.SnowBurstHits(almas, 0, spark), "standing on a spark");
                Assert.IsFalse(RegionBossRules.SnowBurstHits(almas, 0, spark + Vector2.up * (RegionBossRules.SnowSparkRadius - 0.1f)));
                Assert.IsTrue(second.All(s => Vector2.Distance(s, spark) > RegionBossRules.SnowSparkRadius), "the next throw hides them elsewhere");
                Assert.IsTrue(RegionBossRules.SnowBurstHits(almas, 1, spark), "last throw's spark is only snow now");
            }

            Assert.IsTrue(RegionBossRules.SnowBurstHits(almas, 0, almas), "next to him");
            Assert.IsFalse(RegionBossRules.SnowBurstHits(almas, 0, almas + Vector2.down * (RegionBossRules.SnowBurstRadius + 0.1f)), "far away");
            Assert.Less(RegionBossRules.SnowSparkDistance + RegionBossRules.SnowSparkRadius, RegionBossRules.SnowBurstRadius, "a spark is inside the burst");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreatAlmasId).StrongerAtNight, "the ficha: he comes out at night and the cold doubles");
        }

        [Test]
        public void UnleapedCarp_ShortThenLong_NeverOnTheMark()
        {
            Vector2 carp = Vector2.zero;
            Vector2 mark = new Vector2(0f, 10f);
            Assert.IsTrue(RegionBossRules.LeapFallsShort(0));
            Assert.IsFalse(RegionBossRules.LeapFallsShort(1));
            Assert.IsTrue(RegionBossRules.LeapFallsShort(2));
            Assert.IsFalse(RegionBossRules.LeapFallsShort(-1), "never throws on a bad count");
            Assert.AreEqual(new Vector2(0f, 5f), RegionBossRules.LeapLanding(0, carp, mark));
            Assert.AreEqual(new Vector2(0f, 10f + RegionBossRules.LeapOvershoot), RegionBossRules.LeapLanding(1, carp, mark));
            Assert.IsFalse(RegionBossRules.LeapHits(0, carp, mark, mark), "a short leap misses whoever stays");
            Assert.IsTrue(RegionBossRules.LeapHits(0, carp, mark, new Vector2(0f, 6f)), "running toward a short leap");
            Assert.IsFalse(RegionBossRules.LeapHits(1, carp, mark, mark), "a long leap flies past whoever stays");
            Assert.IsTrue(RegionBossRules.LeapHits(1, carp, mark, new Vector2(0f, 13f)), "backing away into a long leap");
            Assert.AreEqual(mark, RegionBossRules.LeapLanding(1, mark, mark), "on top of the mark there is nowhere to overshoot");
            Assert.Greater(RegionBossRules.LeapOvershoot, RegionBossRules.LeapSplashRadius, "the mark is clear of a long leap");
        }

        [Test]
        public void ForgottenSpinner_ATautThreadThrowsDown_ASlackOneDoesNot()
        {
            Assert.IsTrue(RegionBossRules.TautThreadHits(8f, 8f), "stood still: the thread is taut");
            Assert.IsTrue(RegionBossRules.TautThreadHits(8f, 12f), "ran away: worse");
            Assert.IsTrue(RegionBossRules.TautThreadHits(8f, 8f - RegionBossRules.ThreadSlack + 0.1f), "not close enough");
            Assert.IsFalse(RegionBossRules.TautThreadHits(8f, 8f - RegionBossRules.ThreadSlack), "came in: slack");
            Assert.IsFalse(RegionBossRules.TautThreadHits(3f, 2f), "at her legs there is nothing to pull");
            Assert.IsFalse(RegionBossRules.TautThreadHits(3f, RegionBossRules.ThreadHugDistance), "hugging her");
            Assert.IsTrue(RegionBossRules.TautThreadHits(3f, 3f), "close but standing still");
            Assert.IsFalse(RegionBossRules.TautThreadHits(RegionBossRules.ThreadReach + 0.1f, 20f), "too far to be tied");
            Assert.Less(RegionBossRules.ThreadReach, RegionBossRules.AbilityRange);
            Assert.Less(RegionBossRules.ThreadSlack, RegionBossRules.ThreadReach - RegionBossRules.ThreadHugDistance, "there is always room to come in");
        }

        [Test]
        public void WellDrinkingRoot_ComesUpOnTheHeroSteps_NewGroundIsSafe()
        {
            var steps = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1.5f, 0f), new Vector2(3f, 0f) };
            Assert.IsTrue(RegionBossRules.RootTrailHits(steps, Vector2.zero), "stood where the warning began");
            Assert.IsTrue(RegionBossRules.RootTrailHits(steps, new Vector2(1.5f, 0.8f)), "on an old step");
            Assert.IsTrue(RegionBossRules.RootTrailHits(steps, new Vector2(-1f, 0f)), "went back the way it came");
            Assert.IsFalse(RegionBossRules.RootTrailHits(steps, new Vector2(3f + RegionBossRules.RootRadius + 0.1f, 0f)), "kept walking onto new ground");
            Assert.IsFalse(RegionBossRules.RootTrailHits(steps, new Vector2(1.5f, RegionBossRules.RootRadius + 0.1f)), "stepped off the trail");
            Assert.IsFalse(RegionBossRules.RootTrailHits(new List<Vector2>(), Vector2.zero));
            Assert.IsFalse(RegionBossRules.RootTrailHits(null, Vector2.zero));
            Assert.IsFalse(RegionBossRules.RootMarksStep(RegionBossRules.RootStepSeconds - 0.01f));
            Assert.IsTrue(RegionBossRules.RootMarksStep(RegionBossRules.RootStepSeconds));
            Assert.AreEqual(7, RegionBossRules.RootMaxSteps, "the first spot and one every 0.4 s of the 2.4 s warning");
            Assert.Greater(5f * RegionBossRules.RootStepSeconds, RegionBossRules.RootRadius, "walking, the hero is off the last step by the time it comes up");
        }

        [Test]
        public void GreatSmudge_OnlyTheColourLeftIsClean_AndItTurnsRound()
        {
            Assert.AreEqual(SmudgeColour.Ochre, RegionBossRules.SmudgeColourFor(0));
            Assert.AreEqual(SmudgeColour.Red, RegionBossRules.SmudgeColourFor(1));
            Assert.AreEqual(SmudgeColour.White, RegionBossRules.SmudgeColourFor(2));
            Assert.AreEqual(SmudgeColour.Ochre, RegionBossRules.SmudgeColourFor(3));
            Assert.AreEqual(SmudgeColour.Red, RegionBossRules.SmudgeColourFor(-1), "never throws on a bad count");
            float near = 1f, middle = RegionBossRules.SmudgeBandWidth + 1f, far = RegionBossRules.SmudgeBandWidth * 2f + 1f;
            Assert.IsFalse(RegionBossRules.SmudgeHits(SmudgeColour.Ochre, near), "ochre: right next to it");
            Assert.IsTrue(RegionBossRules.SmudgeHits(SmudgeColour.Ochre, middle));
            Assert.IsTrue(RegionBossRules.SmudgeHits(SmudgeColour.Ochre, far));
            Assert.IsTrue(RegionBossRules.SmudgeHits(SmudgeColour.Red, near));
            Assert.IsFalse(RegionBossRules.SmudgeHits(SmudgeColour.Red, middle), "red: the middle band");
            Assert.IsTrue(RegionBossRules.SmudgeHits(SmudgeColour.White, middle));
            Assert.IsFalse(RegionBossRules.SmudgeHits(SmudgeColour.White, far), "white: the far band");
            Assert.IsFalse(RegionBossRules.SmudgeHits(SmudgeColour.Ochre, RegionBossRules.SmudgeBandWidth * RegionBossRules.SmudgeColours + 0.1f), "out of the smear");
            Assert.IsTrue(RegionBossRules.SmudgeHits(SmudgeColour.Red, 0f), "under it");
            Assert.Less(RegionBossRules.SmudgeBandWidth * RegionBossRules.SmudgeColours, RegionBossRules.AbilityRange);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreatSmudgeId).StrongerAtNight, "the ficha: the paintings only glow faintly at night");
        }

        [Test]
        public void NinkiNanka_WhoeverLooksIsHit_TurningYourBackIsSafe()
        {
            Vector2 dragon = new Vector2(0f, 6f);
            Vector2 hero = Vector2.zero;
            Assert.IsTrue(RegionBossRules.GazeHits(dragon, hero, Vector2.up), "staring at her");
            Assert.IsTrue(RegionBossRules.GazeHits(dragon, hero, new Vector2(0.8f, 1f)), "half looking");
            Assert.IsFalse(RegionBossRules.GazeHits(dragon, hero, Vector2.down), "back turned");
            Assert.IsFalse(RegionBossRules.GazeHits(dragon, hero, Vector2.right), "looking away");
            Assert.IsFalse(RegionBossRules.GazeHits(dragon, new Vector2(0f, -RegionBossRules.GazeReach), Vector2.up), "too far to be seen");
            Assert.IsTrue(RegionBossRules.GazeHits(dragon, dragon + new Vector2(0.2f, 0f), Vector2.down), "nose to nose there is no looking away");
            Assert.IsTrue(RegionBossRules.GazeHits(dragon, hero, Vector2.zero), "no facing: caught looking");
            Assert.Less(RegionBossRules.GazeHalfAngle, 90f, "turning sideways is enough");
        }

        [Test]
        public void Nian_NoiseDuringTheWarning_CallsOffThePounce()
        {
            Vector2 mark = new Vector2(4f, 4f);
            float max = 1000f;
            Assert.IsTrue(RegionBossRules.NianTakesFright(800f, 800f - max * RegionBossRules.NianNoiseFraction, max), "hit hard enough");
            Assert.IsFalse(RegionBossRules.NianTakesFright(800f, 800f - max * RegionBossRules.NianNoiseFraction + 1f, max), "not enough noise");
            Assert.IsFalse(RegionBossRules.NianTakesFright(800f, 800f, max), "stood there quietly");
            Assert.IsFalse(RegionBossRules.NianTakesFright(800f, 900f, max), "healing is no noise");
            Assert.IsFalse(RegionBossRules.NianTakesFright(800f, 0f, 0f), "no health to read");
            Assert.IsTrue(RegionBossRules.NianPounceHits(false, mark, mark + Vector2.left * (RegionBossRules.NianPounceRadius - 0.1f)));
            Assert.IsFalse(RegionBossRules.NianPounceHits(false, mark, mark + Vector2.left * (RegionBossRules.NianPounceRadius + 0.1f)), "out of its leap");
            Assert.IsFalse(RegionBossRules.NianPounceHits(true, mark, mark), "frightened, it jumps back");
            Assert.Less(RegionBossRules.NianNoiseFraction, 0.1f, "a few blows in the warning are enough");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.NianId).StrongerAtNight, "the ficha: the nian is bolder at night");
        }

        [Test]
        public void OldKurganGriffin_StrikesWhoeverStandsOnTheHoard_WhereverItIs()
        {
            Vector2 hoard = new Vector2(-10f, 30f);
            Assert.IsTrue(RegionBossRules.HoardHits(hoard, hoard));
            Assert.IsTrue(RegionBossRules.HoardHits(hoard, hoard + Vector2.up * (RegionBossRules.HoardRadius - 0.1f)), "on the edge of the gold");
            Assert.IsFalse(RegionBossRules.HoardHits(hoard, hoard + Vector2.up * (RegionBossRules.HoardRadius + 0.1f)), "off the ring");
            Assert.Less(RegionBossRules.HoardRadius, RegionBossRules.ArenaClearRadius, "no clearing reaches the hoard");
            Assert.Less(RegionBossRules.HoardRadius, RegionBossRules.AbilityRange, "there is ground to fight on outside the ring");
        }

        [Test]
        public void FirstMapBosses_EachSignatureIsNewToTheDirector_AtTheTopOfItsMap()
        {
            RegionBossSignature[] first =
            {
                RegionBossSignature.TautThread, RegionBossSignature.RootTrail, RegionBossSignature.StolenColours,
                RegionBossSignature.ShyGaze, RegionBossSignature.NoiseFright, RegionBossSignature.HoardGuard,
            };
            CollectionAssert.AreEquivalent(first, FirstIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in first)
            {
                Assert.Greater((int)signature, (int)RegionBossSignature.FailedLeap, $"{signature}: after the ones that already exist");
            }

            foreach (string id in FirstIds)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
                Assert.AreEqual(MapCatalog.Get(spawn.MapId).MonsterLevelMax, spawn.Level, id);
                Assert.Less(spawn.Level, RegionBossRules.SpawnOf(RegionBossRules.OhyaId).Level, $"{id}: below the band of the letters");
            }

            for (int i = 0; i < FirstIds.Length; i++)
            {
                for (int j = i + 1; j < FirstIds.Length; j++)
                {
                    Assert.AreNotEqual(RegionBossRules.SpawnOf(FirstIds[i]).MapId, RegionBossRules.SpawnOf(FirstIds[j]).MapId);
                }
            }
        }

        [Test]
        public void MistElder_SnapsAtWhoeverTurnedTheirBack_FacingHimIsSafe()
        {
            Vector2 elder = new Vector2(0f, 6f);
            Vector2 hero = Vector2.zero;
            Assert.IsFalse(RegionBossRules.MistSnapHits(elder, hero, Vector2.up), "facing him");
            Assert.IsFalse(RegionBossRules.MistSnapHits(elder, hero, new Vector2(1f, 0.3f)), "he is still at the edge of sight");
            Assert.IsTrue(RegionBossRules.MistSnapHits(elder, hero, Vector2.down), "back turned");
            Assert.IsTrue(RegionBossRules.MistSnapHits(elder, hero, new Vector2(1f, -0.3f)), "looking away");
            Assert.IsTrue(RegionBossRules.MistSnapHits(elder, hero, Vector2.zero), "no facing: lost him in the mist");
            Assert.IsFalse(RegionBossRules.MistSnapHits(elder, new Vector2(0f, 6f - RegionBossRules.MistReach - 0.1f), Vector2.down), "out of his snap");
            Assert.IsFalse(RegionBossRules.MistSnapHits(elder, elder + new Vector2(0.2f, 0f), Vector2.down), "right at his snout there is no mist");
            Assert.Greater(RegionBossRules.MistSightHalfAngle, RegionBossRules.GazeHalfAngle, "keeping him in sight is easier than looking away from Ninki Nanka");
            Assert.Less(RegionBossRules.MistSightHalfAngle, 90f, "running away from him is turning your back");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.MistElderId).StrongerAtNight);
        }

        [Test]
        public void DuneChoir_EachVerseReachesFurther_ThenItStartsAgain()
        {
            Assert.AreEqual(0, RegionBossRules.DuneVerseOf(0));
            Assert.AreEqual(1, RegionBossRules.DuneVerseOf(1));
            Assert.AreEqual(2, RegionBossRules.DuneVerseOf(2));
            Assert.AreEqual(0, RegionBossRules.DuneVerseOf(3));
            Assert.AreEqual(1, RegionBossRules.DuneVerseOf(-1), "never throws on a bad count");
            Assert.AreEqual(4f, RegionBossRules.VerseRadius(0));
            Assert.AreEqual(7f, RegionBossRules.VerseRadius(1));
            Assert.AreEqual(10f, RegionBossRules.VerseRadius(2));
            Assert.IsTrue(RegionBossRules.VerseHits(0, 0f), "under it");
            Assert.IsTrue(RegionBossRules.VerseHits(0, 4f));
            Assert.IsFalse(RegionBossRules.VerseHits(0, 4.1f), "just past the first verse");
            Assert.IsTrue(RegionBossRules.VerseHits(1, 5f), "where the first verse was safe");
            Assert.IsFalse(RegionBossRules.VerseHits(1, 7.1f));
            Assert.IsTrue(RegionBossRules.VerseHits(2, 9.5f));
            Assert.IsFalse(RegionBossRules.VerseHits(2, 10.1f), "past the third verse");
            Assert.Less(RegionBossRules.VerseRadius(2), RegionBossRules.AbilityRange, "there is always ground past the last verse");
        }

        [Test]
        public void GreedyBottler_OnlyBehindABottleIsSafe_AndTheBottlesTurn()
        {
            Vector2 origin = new Vector2(10f, -4f);
            Vector2[] bottles = RegionBossRules.BottleSpots(origin, 0);
            Assert.AreEqual(RegionBossRules.BottleCount, bottles.Length);
            foreach (Vector2 bottle in bottles)
            {
                Assert.AreEqual(RegionBossRules.BottleRingRadius, Vector2.Distance(origin, bottle), 0.001f);
            }

            Assert.AreEqual(0f, Vector2.Distance(origin + Vector2.up * RegionBossRules.BottleRingRadius, bottles[0]), 0.001f, "the first bottle due north");
            Vector2 north = Vector2.up;
            Assert.IsFalse(RegionBossRules.StaleAirHits(origin, bottles, origin + north * 8f), "behind the north bottle");
            Assert.IsFalse(RegionBossRules.StaleAirHits(origin, bottles, origin + north * 8f + Vector2.right * (RegionBossRules.BottleShelter - 0.2f)), "still in its lee");
            Assert.IsTrue(RegionBossRules.StaleAirHits(origin, bottles, origin + north * 3f), "between him and the bottle");
            Assert.IsTrue(RegionBossRules.StaleAirHits(origin, bottles, origin + Vector2.down * 8f), "no bottle to the south");
            Assert.IsTrue(RegionBossRules.StaleAirHits(origin, bottles, origin), "on top of him");
            Assert.IsFalse(RegionBossRules.StaleAirHits(origin, bottles, origin + Vector2.down * (RegionBossRules.StaleReach + 0.1f)), "out of the stale air");
            Assert.IsTrue(RegionBossRules.StaleAirHits(origin, null, origin + north * 8f), "no bottles, no shelter");

            Vector2[] next = RegionBossRules.BottleSpots(origin, 1);
            Assert.IsTrue(RegionBossRules.StaleAirHits(origin, next, origin + north * 8f), "the bottles turned: the old shelter is gone");
            Assert.AreEqual(0f, Vector2.Distance(bottles[0], RegionBossRules.BottleSpots(origin, 9)[0]), 0.001f, "nine turns of 40° come round");
            Assert.Less(RegionBossRules.BottleRingRadius + 1f, RegionBossRules.StaleReach, "room to stand behind a bottle inside the reach");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreedyBottlerId).StrongerAtNight, "the ficha: the ones touched by Āz come out double at night");
        }

        [Test]
        public void LightFisher_CastsWhereTheHeroIsGoing_AChangeOfCourseIsSafe()
        {
            Vector2 first = new Vector2(0f, 0f);
            Vector2 second = new Vector2(2f, 0f);
            Vector2 lure = RegionBossRules.LureSpot(first, second);
            Assert.AreEqual(0f, Vector2.Distance(new Vector2(6f, 0f), lure), 0.001f, "0.4 s from the first look to the second, 0.8 s to the cast: twice as far again");
            Assert.IsTrue(RegionBossRules.LureHits(lure, new Vector2(6f, 0.5f)), "kept running straight");
            Assert.IsFalse(RegionBossRules.LureHits(lure, new Vector2(2f, 3f)), "turned aside");
            Assert.IsFalse(RegionBossRules.LureHits(lure, new Vector2(0f, 0f)), "turned back");
            Assert.IsFalse(RegionBossRules.LureHits(lure, second), "stopped after the second look");

            Vector2 still = new Vector2(4f, 4f);
            Assert.AreEqual(0f, Vector2.Distance(still, RegionBossRules.LureSpot(still, still)), 0.001f);
            Assert.IsTrue(RegionBossRules.LureHits(RegionBossRules.LureSpot(still, still), still), "standing still waiting is caught");
            Assert.AreEqual(RegionBossRules.LureMaxLead, Vector2.Distance(new Vector2(0f, 10f), RegionBossRules.LureSpot(Vector2.zero, new Vector2(0f, 10f))), 0.001f, "he never casts too far ahead");
            Assert.Less(RegionBossRules.LureSecondLookSeconds, RegionBossRules.LureFirstLookSeconds);
            Assert.Less(RegionBossRules.LureFirstLookSeconds, RegionBossRules.LureWindupSeconds, "both looks fall inside the warning");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.LightFisherId).StrongerAtNight, "the ficha: the will-o'-wisps double at night");
        }

        [Test]
        public void VaneLord_TheWindTurnsAQuarterEachGust_UpwindIsSafe()
        {
            Assert.AreEqual(VaneWind.North, RegionBossRules.VaneWindFor(0));
            Assert.AreEqual(VaneWind.East, RegionBossRules.VaneWindFor(1));
            Assert.AreEqual(VaneWind.South, RegionBossRules.VaneWindFor(2));
            Assert.AreEqual(VaneWind.West, RegionBossRules.VaneWindFor(3));
            Assert.AreEqual(VaneWind.North, RegionBossRules.VaneWindFor(4));
            Assert.AreEqual(VaneWind.East, RegionBossRules.VaneWindFor(-1), "never throws on a bad count");
            Vector2 vane = new Vector2(-5f, 20f);
            Assert.IsFalse(RegionBossRules.VaneGustHits(VaneWind.North, vane, vane + Vector2.up * 5f), "wind from the north: north of him is safe");
            Assert.IsTrue(RegionBossRules.VaneGustHits(VaneWind.North, vane, vane + Vector2.down * 5f), "downwind");
            Assert.IsTrue(RegionBossRules.VaneGustHits(VaneWind.North, vane, vane + Vector2.right * 5f), "beside him is still in the gust");
            Assert.IsTrue(RegionBossRules.VaneGustHits(VaneWind.North, vane, vane + new Vector2(3f, RegionBossRules.GustLee - 0.1f)), "not upwind enough");
            Assert.IsFalse(RegionBossRules.VaneGustHits(VaneWind.East, vane, vane + Vector2.right * 5f), "wind from the east: east of him is safe");
            Assert.IsTrue(RegionBossRules.VaneGustHits(VaneWind.East, vane, vane + Vector2.left * 5f));
            Assert.IsFalse(RegionBossRules.VaneGustHits(VaneWind.South, vane, vane + Vector2.down * 5f));
            Assert.IsFalse(RegionBossRules.VaneGustHits(VaneWind.West, vane, vane + Vector2.left * 5f));
            Assert.IsFalse(RegionBossRules.VaneGustHits(VaneWind.North, vane, vane + Vector2.down * (RegionBossRules.GustReach + 0.1f)), "out of the gust");
            Assert.IsTrue(RegionBossRules.VaneGustHits(VaneWind.West, vane, vane), "on top of him");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.VaneLordId).StrongerAtNight);
        }

        [Test]
        public void LittleEclipse_EachCoverIsWider_UntilTheWholeRoundIsDark()
        {
            Assert.AreEqual(45f, RegionBossRules.EclipseHalfAngle(0));
            Assert.AreEqual(90f, RegionBossRules.EclipseHalfAngle(1));
            Assert.AreEqual(135f, RegionBossRules.EclipseHalfAngle(2));
            Assert.AreEqual(180f, RegionBossRules.EclipseHalfAngle(3));
            Assert.AreEqual(0, RegionBossRules.EclipsePhaseOf(4));
            Assert.AreEqual(1, RegionBossRules.EclipsePhaseOf(-1), "never throws on a bad count");
            Vector2 eclipse = new Vector2(30f, 30f);
            Vector2 aim = eclipse + Vector2.up * 6f;
            Vector2 aside = eclipse + Vector2.right * 6f;
            Vector2 behind = eclipse + Vector2.down * 6f;
            Vector2 halfBack = eclipse + new Vector2(5f, -5f);
            Assert.IsTrue(RegionBossRules.EclipseHits(0, eclipse, aim, aim), "the slice falls toward the hero");
            Assert.IsFalse(RegionBossRules.EclipseHits(0, eclipse, aim, aside), "a quarter: a step aside");
            Assert.IsTrue(RegionBossRules.EclipseHits(1, eclipse, aim, aside), "half: aside is dark now");
            Assert.IsFalse(RegionBossRules.EclipseHits(1, eclipse, aim, halfBack));
            Assert.IsTrue(RegionBossRules.EclipseHits(2, eclipse, aim, halfBack), "three quarters");
            Assert.IsFalse(RegionBossRules.EclipseHits(2, eclipse, aim, behind), "three quarters: only behind it is light");
            Assert.IsTrue(RegionBossRules.EclipseHits(3, eclipse, aim, behind), "the whole round");
            Assert.IsFalse(RegionBossRules.EclipseHits(3, eclipse, aim, eclipse + Vector2.down * (RegionBossRules.EclipseReach + 0.1f)), "beyond its shadow");
            Assert.IsTrue(RegionBossRules.EclipseHits(0, eclipse, aim, eclipse), "under the disc");
            Assert.IsTrue(RegionBossRules.EclipseHits(0, eclipse, eclipse, behind), "no bearing: the slice is everywhere");
            Assert.Less(RegionBossRules.EclipseReach, RegionBossRules.AbilityRange, "there is ground beyond the shadow");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.LittleEclipseId).StrongerAtNight);
        }

        [Test]
        public void MoreMapBosses_EachSignatureIsNewToTheDirector_AtTheTopOfItsMap()
        {
            RegionBossSignature[] more =
            {
                RegionBossSignature.MistStare, RegionBossSignature.DuneVerses, RegionBossSignature.StaleAir,
                RegionBossSignature.LeadCast, RegionBossSignature.VaneGust, RegionBossSignature.GrowingEclipse,
            };
            CollectionAssert.AreEquivalent(more, MoreIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in more)
            {
                Assert.Greater((int)signature, (int)RegionBossSignature.HoardGuard, $"{signature}: after the ones that already exist");
            }

            foreach (string id in MoreIds)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
                Assert.AreEqual(MapCatalog.Get(spawn.MapId).MonsterLevelMax, spawn.Level, id);
                Assert.Less(spawn.Level, RegionBossRules.SpawnOf(RegionBossRules.OhyaId).Level, $"{id}: below the band of the letters");
            }
        }

        [Test]
        public void MidMapBosses_EachSignatureIsNewToTheDirector()
        {
            RegionBossSignature[] mid =
            {
                RegionBossSignature.MonsoonArms, RegionBossSignature.HerdSnort, RegionBossSignature.ForgottenPlanks,
                RegionBossSignature.RunawayRun, RegionBossSignature.SnowSparks, RegionBossSignature.FailedLeap,
            };
            CollectionAssert.AreEquivalent(mid, MidIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (string id in MidIds)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
                Assert.AreEqual(MapCatalog.Get(spawn.MapId).MonsterLevelMax, spawn.Level, id);
                Assert.Less(spawn.Level, RegionBossRules.SpawnOf(RegionBossRules.OhyaId).Level, $"{id}: below the band of the letters");
            }
        }
    }
}
