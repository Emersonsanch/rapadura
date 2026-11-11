using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;
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
    /// The map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>): kept by the world bosses'
    /// director and fought with their abilities, each with an ability of its own. Pure rules and data: the procedural models are built with the
    /// runtime's own components (no MonoBehaviour of this assembly is added).
    /// </summary>
    public class MapBossTests
    {
        private static readonly string[] Ids =
        {
            RegionBossRules.RopeCutterId, RegionBossRules.EndlessBellId, RegionBossRules.WalrusQueenId,
            RegionBossRules.GrandfatherMammothId, RegionBossRules.ShisaKingId, RegionBossRules.FurnaceMasterId,
        };

        /// <summary>The maps the story walks through in the 150–300 band (LateStoryCatalog) that have a boss in their ficha.</summary>
        private static readonly string[] StoryMaps =
        {
            OverworldLayouts.PontesVentoMapId, OverworldLayouts.ErmidasMapId, OverworldLayouts.CanaisGeloMapId,
            OverworldLayouts.GaleriasMamutesMapId, OverworldLayouts.JardinsLavaMapId, OverworldLayouts.NoveFornalhasMapId,
        };

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
            foreach (string mapId in StoryMaps)
            {
                MapDefinition map = MapCatalog.Get(mapId);
                Assert.IsNotNull(map, mapId);
                Assert.That(map.MonsterLevelMax, Is.InRange(150, 300), mapId);
            }
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
    }
}
