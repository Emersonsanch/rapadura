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
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 25, 26, 32, 37, 38, 43, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>): kept by the world bosses'
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

        private static readonly string[] Ids =
        {
            RegionBossRules.RopeCutterId, RegionBossRules.EndlessBellId, RegionBossRules.WalrusQueenId,
            RegionBossRules.GrandfatherMammothId, RegionBossRules.ShisaKingId, RegionBossRules.FurnaceMasterId,
            RegionBossRules.MonsoonOctopusId, RegionBossRules.SmokeAurochsId, RegionBossRules.ForgetfulBridgeId,
            RegionBossRules.RunawayCartId, RegionBossRules.GreatAlmasId, RegionBossRules.UnleapedCarpId,
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

        private static readonly string[] StoryMaps = LateStoryMaps.Concat(MidStoryMaps).ToArray();

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
            foreach (string id in MidIds)
            {
                Assert.Less(Load(id).AttackDamage, Load(RegionBossRules.FourGluttonsId).AttackDamage, $"{id}: hits softer than the last world boss of Regions 5–8");
            }
            foreach (string id in Ids)
            {
                Assert.Less(Load(id).BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, id);
            }
        }

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
            Assert.IsTrue(string.IsNullOrEmpty(OverworldRules.ZoneAt(world, arena).RequirementTag), $"{id}: on foot, nothing asked");
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
