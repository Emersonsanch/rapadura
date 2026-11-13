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
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The map bosses of the other pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89): the list, their strength against
    /// the region's world boss, each one's own ability as pure rules, their call-outs, the arms that rise on the windup, and the shared checks (asset,
    /// biome, model, page, arena) for these five.
    /// </summary>
    public class DeepMapBossTests
    {
        private static readonly string[] DeepIds =
        {
            RegionBossRules.SmokeKingId, RegionBossRules.DevouringLionessId, RegionBossRules.BlackTailFishId,
            RegionBossRules.DeepDragonId, RegionBossRules.NoveloGuardianId,
        };

        private static readonly string[] DeepMaps =
        {
            OverworldLayouts.CovaFumacaMapId, OverworldLayouts.CovaFogoMapId, OverworldLayouts.CovaLodoMapId,
            OverworldLayouts.CovaTrevaMapId, OverworldLayouts.PonteNoveloMapId,
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
        }

        private static EnemyDefinition Load(string id)
        {
            EnemyDefinition definition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{id}");
            Assert.IsNotNull(definition, id);
            return definition;
        }

        // ---------------------------------------------------------------- the list

        [Test]
        public void ThePits_GetTheirFichaBoss_AtTheTopOfTheirBand_OnePerMap()
        {
            for (int i = 0; i < DeepIds.Length; i++)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(DeepIds[i]);
                Assert.IsNotNull(spawn, DeepIds[i]);
                Assert.IsTrue(RegionBossRules.IsMapBoss(DeepIds[i]), DeepIds[i]);
                Assert.AreEqual(DeepMaps[i], spawn.MapId, DeepIds[i]);
                MapDefinition map = MapCatalog.Get(spawn.MapId);
                Assert.IsNotNull(map, spawn.MapId);
                Assert.AreEqual(MapRegion.LimiarProfundo, map.Region, DeepIds[i]);
                Assert.AreEqual(map.MonsterLevelMax, spawn.Level, $"{DeepIds[i]}: the top of its map's band");
                Assert.IsFalse(RegionBossRules.AllSpawns.Any(s => s.MapId == spawn.MapId), $"{spawn.MapId}: no world boss on it");
                Assert.AreEqual(1, RegionBossRules.MapSpawns.Count(s => s.MapId == spawn.MapId), spawn.MapId);
                Assert.IsFalse(spawn.StrongerAtNight, $"{DeepIds[i]}: the fichas: night is the same in the pits");
            }

            // With A Águia (86) every pit and the bridge has its own, 84–89.
            int[] numbers = RegionBossRules.MapSpawns.Select(s => MapCatalog.Get(s.MapId)).Where(m => m != null && m.Region == MapRegion.LimiarProfundo)
                .Select(m => m.MapId).OrderBy(n => n).ToArray();
            CollectionAssert.AreEqual(new[] { 84, 85, 86, 87, 88, 89 }, numbers);
        }

        [Test]
        public void ThePitBosses_AreWeakerThanTheGreatDark_AndGrowWithTheBand()
        {
            EnemyDefinition world = Load(RegionBossRules.GreatDarkId);
            int previousLevel = 0;
            foreach (string id in DeepIds)
            {
                EnemyDefinition boss = Load(id);
                Assert.Less(boss.MaxHealth, world.MaxHealth, id);
                Assert.Less(boss.AttackDamage, world.AttackDamage, id);
                Assert.LessOrEqual(boss.MaxHealth, 1180, id);
                Assert.Less(boss.BossExperienceMultiplier, Load(RegionBossRules.WindKingId).BossExperienceMultiplier, id);
                Assert.AreEqual(RegionBossRules.MapBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(id, true), id);
                Assert.Greater(boss.Level, previousLevel, $"{id}: the pits go deeper");
                previousLevel = boss.Level;
            }
        }

        [Test]
        public void EachSignatureIsNewToTheDirector()
        {
            RegionBossSignature[] deep =
            {
                RegionBossSignature.SmokeCopy, RegionBossSignature.BurningStripes, RegionBossSignature.TailSweep,
                RegionBossSignature.LampHaven, RegionBossSignature.NoveloWave,
            };
            CollectionAssert.AreEquivalent(deep, DeepIds.Select(id => RegionBossRules.SpawnOf(id).Signature));
            foreach (RegionBossSignature signature in deep)
            {
                Assert.AreEqual(1, RegionBossRules.DirectorSpawns.Count(s => s.Signature == signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureInterval(signature), RegionBossRules.SignatureWindup(signature), signature.ToString());
                Assert.Greater(RegionBossRules.SignatureWindup(signature), 1f, $"{signature}: time to read the warning");
                Assert.IsTrue(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Storm), signature.ToString());
                Assert.IsFalse(RegionBossRules.SignatureEnabled(signature, RegionBossPhase.Wrath), signature.ToString());
            }

            foreach (string id in DeepIds)
            {
                RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
                Assert.IsTrue(spawn.NeverDies, $"{id}: freed, it leaves with its line");
                Assert.IsFalse(spawn.ReturnsNextNight, id);
            }
        }

        // ---------------------------------------------------------------- the abilities

        [Test]
        public void SmokeKing_ChokesWhoeverIsNearerTheCopy_StepInPastHalfwayIsSafe()
        {
            Vector2 king = new Vector2(10f, 10f);
            Vector2 mark = king + Vector2.up * 8f;
            Vector2 copy = RegionBossRules.SmokeCopySpot(king, mark);
            Assert.AreEqual(0f, Vector2.Distance(mark, copy), 1e-4f, "the copy rises on the hero's spot");
            Assert.IsTrue(RegionBossRules.SmokeCopyHits(king, copy, mark), "stayed with the copy");
            Assert.IsTrue(RegionBossRules.SmokeCopyHits(king, copy, mark + Vector2.right * 4f), "a step aside is still nearer the copy");
            Assert.IsTrue(RegionBossRules.SmokeCopyHits(king, copy, king + Vector2.up * 4.5f), "just short of halfway");
            Assert.IsFalse(RegionBossRules.SmokeCopyHits(king, copy, king + Vector2.up * 3.5f), "past halfway, nearer the king");
            Assert.IsFalse(RegionBossRules.SmokeCopyHits(king, copy, king + Vector2.down * 3f), "behind him");
            Assert.IsFalse(RegionBossRules.SmokeCopyHits(king, copy, king + Vector2.up * (RegionBossRules.SmokeCopyReach + 0.5f)), "out of his reach");

            Vector2 close = RegionBossRules.SmokeCopySpot(king, king + Vector2.right * 0.5f);
            Assert.AreEqual(RegionBossRules.SmokeCopyMinDistance, Vector2.Distance(king, close), 1e-3f, "never on top of him");
            Assert.IsFalse(RegionBossRules.SmokeCopyHits(king, close, king), "against him is always safe");
            Assert.DoesNotThrow(() => RegionBossRules.SmokeCopySpot(king, king));
            Assert.Less(RegionBossRules.SmokeCopyReach, RegionBossRules.AbilityRange);
        }

        [Test]
        public void DevouringLioness_EveryOtherStripeBurns_OneStepAcrossIsSafe()
        {
            Vector2 lioness = new Vector2(-30f, 20f);
            Vector2 mark = lioness + new Vector2(5f, 3f);
            float stripe = RegionBossRules.LionessStripe;
            Assert.IsTrue(RegionBossRules.LionessStripesRunNorth(0));
            Assert.IsFalse(RegionBossRules.LionessStripesRunNorth(1), "east–west the next roar");
            Assert.IsTrue(RegionBossRules.LionessStripesRunNorth(2));

            // North–south stripes: along the stripe (north) still burns, a step east or west does not.
            Assert.IsTrue(RegionBossRules.LionessStripeBurns(true, lioness, mark, mark), "stayed put");
            Assert.IsTrue(RegionBossRules.LionessStripeBurns(true, lioness, mark, mark + Vector2.up * 6f), "ran along the stripe");
            Assert.IsFalse(RegionBossRules.LionessStripeBurns(true, lioness, mark, mark + Vector2.right * stripe), "one stripe east");
            Assert.IsFalse(RegionBossRules.LionessStripeBurns(true, lioness, mark, mark + Vector2.left * stripe), "one stripe west");
            Assert.IsTrue(RegionBossRules.LionessStripeBurns(true, lioness, mark, mark + Vector2.right * stripe * 2f), "two stripes over burns again");
            // East–west stripes: now north is the step across.
            Assert.IsFalse(RegionBossRules.LionessStripeBurns(false, lioness, mark, mark + Vector2.up * stripe), "one stripe north");
            Assert.IsTrue(RegionBossRules.LionessStripeBurns(false, lioness, mark, mark + Vector2.right * 6f), "ran along it");
            // Behind her, on the other side of her own spot: the counting holds across zero.
            Assert.AreEqual(RegionBossRules.LionessStripeBurns(true, lioness, mark, lioness + new Vector2(-3f, 0f)),
                !RegionBossRules.LionessStripeBurns(true, lioness, mark, lioness + new Vector2(-3f + stripe, 0f)), "neighbours always differ");
            Assert.IsFalse(RegionBossRules.LionessStripeBurns(true, lioness, mark, lioness + Vector2.up * (RegionBossRules.LionessReach + 0.5f)), "beyond her reach");
            Assert.Less(stripe, 6f, "a dark stripe is always a short step away");
        }

        [Test]
        public void BlackTailFish_SweepsTheHeroSide_TheOtherSideAndFarAwayAreSafe()
        {
            Vector2 fish = new Vector2(0f, 0f);
            Vector2 mark = new Vector2(4f, 0f);
            Assert.IsTrue(RegionBossRules.TailSweepHits(fish, mark, mark), "stayed on its side");
            Assert.IsTrue(RegionBossRules.TailSweepHits(fish, mark, new Vector2(1f, 5f)), "beside it but still on that side");
            Assert.IsFalse(RegionBossRules.TailSweepHits(fish, mark, new Vector2(-2f, 0f)), "slipped round to its other side");
            Assert.IsFalse(RegionBossRules.TailSweepHits(fish, mark, new Vector2(-1f, 4f)), "just over the line, by its head");
            Assert.IsFalse(RegionBossRules.TailSweepHits(fish, mark, new Vector2(RegionBossRules.TailReach + 0.5f, 0f)), "backed off out of reach");
            Assert.IsFalse(RegionBossRules.TailSweepHits(fish, fish, new Vector2(0f, -3f)), "no side to sweep: south is safe");
            Assert.Less(RegionBossRules.TailReach, RegionBossRules.AbilityRange);
        }

        [Test]
        public void DeepDragon_OnlyTheLampIsLit_AndItSwapsSides()
        {
            Vector2 dragon = new Vector2(50f, -20f);
            Vector2 mark = dragon + Vector2.up * 5f;
            Vector2 first = RegionBossRules.LampSpot(0, dragon, mark);
            Vector2 second = RegionBossRules.LampSpot(1, dragon, mark);
            Assert.AreEqual(RegionBossRules.LampDistance, Vector2.Distance(dragon, first), 1e-3f);
            Assert.AreEqual(0f, Vector2.Dot(first - dragon, mark - dragon), 1e-3f, "beside the line to the hero");
            Assert.AreEqual(0f, Vector2.Distance(first, dragon + Vector2.left * RegionBossRules.LampDistance), 1e-3f, "first on his left (west, facing north)");
            Assert.AreEqual(0f, Vector2.Distance(second, dragon + Vector2.right * RegionBossRules.LampDistance), 1e-3f, "then on his right");
            Assert.AreEqual(0f, Vector2.Distance(first, RegionBossRules.LampSpot(2, dragon, mark)), 1e-3f, "and round again");

            Assert.IsTrue(RegionBossRules.DarkBreathHits(first, dragon, mark), "stayed in the dark");
            Assert.IsTrue(RegionBossRules.DarkBreathHits(first, dragon, second), "the other side is dark this time");
            Assert.IsFalse(RegionBossRules.DarkBreathHits(first, dragon, first), "in the light");
            Assert.IsFalse(RegionBossRules.DarkBreathHits(first, dragon, first + Vector2.up * (RegionBossRules.LampRadius - 0.3f)), "the edge of the light");
            Assert.IsFalse(RegionBossRules.DarkBreathHits(first, dragon, dragon + Vector2.down * (RegionBossRules.DarkReach + 0.5f)), "beyond his reach");
            Assert.Less(RegionBossRules.LampDistance - RegionBossRules.LampRadius, 4f, "the light starts close to him");
            Assert.DoesNotThrow(() => RegionBossRules.LampSpot(0, dragon, dragon));
        }

        [Test]
        public void NoveloGuardian_ShelterIsCloseBehindHim_AwayFromTheNovelo()
        {
            Vector2 guardian = new Vector2(1564f, 100f);
            Vector2 lee = -RegionBossRules.NoveloWaveFrom;
            Assert.IsTrue(RegionBossRules.InGuardianShelter(guardian, guardian + lee * 2f), "right behind him");
            Assert.IsFalse(RegionBossRules.NoveloWaveHits(guardian, guardian + lee * 2f));
            Assert.IsFalse(RegionBossRules.NoveloWaveHits(guardian, guardian + lee * (RegionBossRules.ShelterDepth - 0.5f) + Vector2.up * (RegionBossRules.ShelterHalfWidth - 0.5f)), "the far corner");
            Assert.IsTrue(RegionBossRules.NoveloWaveHits(guardian, guardian + RegionBossRules.NoveloWaveFrom * 3f), "in front of him, on the Novelo's side");
            Assert.IsTrue(RegionBossRules.NoveloWaveHits(guardian, guardian + Vector2.up * 4f), "beside him");
            Assert.IsTrue(RegionBossRules.NoveloWaveHits(guardian, guardian + lee * (RegionBossRules.ShelterDepth + 1f)), "too far behind");
            Assert.IsTrue(RegionBossRules.NoveloWaveHits(guardian, guardian + lee * 3f + Vector2.up * (RegionBossRules.ShelterHalfWidth + 0.5f)), "out of his shadow");
            Assert.IsFalse(RegionBossRules.NoveloWaveHits(guardian, guardian + Vector2.up * (RegionBossRules.NoveloReach + 0.5f)), "beyond the wave");

            // The waves really come from the Novelo: the globe lies on that side of his arena.
            Vector2 toGlobe = OverworldLayouts.NoveloGlobeCenter - OverworldLayouts.GuardiaoDoNoveloArena;
            Assert.Greater(Vector2.Dot(toGlobe.normalized, RegionBossRules.NoveloWaveFrom.normalized), 0.9f);
        }

        // ---------------------------------------------------------------- the call-outs, the page and the model

        [Test]
        public void ThePitBosses_CallOutTheirCopyStripesLampAndShelter()
        {
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string>
            {
                RegionBossTexts.SmokeCopyKey, RegionBossTexts.LionessStripesKey(true), RegionBossTexts.LionessStripesKey(false),
                RegionBossTexts.LionessDarkKey, RegionBossTexts.LampKey, RegionBossTexts.ShelterKey,
            };
            foreach (string key in keys)
            {
                Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
            }

            CollectionAssert.AllItemsAreUnique(keys);
        }

        [TestCaseSource(nameof(DeepIds))]
        public void Asset_Biome_AndPage(string id)
        {
            EnemyDefinition boss = Load(id);
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            Assert.IsTrue(boss.IsBoss, id);
            Assert.AreEqual(spawn.Level, boss.Level, id);
            Assert.IsNotNull(boss.LootItem, id);
            Assert.IsNotNull(ItemDatabase.GetById(boss.LootItem.ItemId), id);
            Assert.IsTrue(BossBonusRules.HasBonusDrop(boss), id);
            Assert.LessOrEqual(boss.MaxHealth, Load("enemy_cave_dweller").MaxHealth, id);

            BiomeRegistry.Invalidate();
            BiomeDefinition home = BiomeRegistry.FindBiomeOf(boss);
            Assert.IsNotNull(home, id);
            Assert.AreEqual(BiomeType.Mountain, home.Type, id);
            Assert.AreEqual(0, home.Enemies.First(e => e.Enemy == boss).Weight, id);

            BestiaryCatalog.Invalidate();
            BestiaryEntry page = BestiaryCatalog.Get(id);
            Assert.IsNotNull(page, id);
            Assert.IsTrue(page.IsBoss, id);
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string>
            {
                page.LoreKey, RegionBossTexts.PhaseKey(id, RegionBossPhase.Storm), RegionBossTexts.PhaseKey(id, RegionBossPhase.LastStand),
                RegionBossTexts.GreatBlowKey(id), RegionBossTexts.SignatureKey(id), RegionBossTexts.WithdrawKey(id),
            };
            keys.AddRange(BestiaryTexts.Cousins.Select(page.CommentKey));
            foreach (string key in keys)
            {
                Assert.IsTrue(table.ContainsKey(key), $"{key}: missing");
            }

            LocalizationEntry name = ContentTexts.Entries.FirstOrDefault(e => e.Key == ContentText.EnemyNameKey(id));
            Assert.IsNotNull(name, id);
            Assert.AreEqual(boss.EnemyName, name.PtBR, id);
            CollectionAssert.Contains(RegionBossTexts.EnemyIds, id);
        }

        [TestCaseSource(nameof(DeepIds))]
        public void Model_IsAMapBossModel_WithinTheBounds(string id)
        {
            ProceduralEnemyKind kind = ProceduralEnemyModels.KindFor(id);
            Assert.AreNotEqual(ProceduralEnemyKind.None, kind, id);
            Assert.That((int)kind, Is.InRange(345, 349), id);
            Assert.IsTrue(ProceduralEnemyModels.IsMapBoss(kind), id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kind);

            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            Assert.IsNotNull(visual.GetComponent<RegionBossAbilities>(), id);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Core), id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f), id);

            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds), id);
            Assert.That(bounds.size.x, Is.InRange(0.5f, 3.5f), $"{id}: width");
            Assert.That(bounds.size.z, Is.InRange(0.4f, 3.5f), $"{id}: depth");
            Assert.That(bounds.size.y, Is.InRange(2.4f, 4f), $"{id}: height");
            Assert.That(bounds.min.y, Is.InRange(-0.15f, 0.15f), $"{id}: feet at {bounds.min.y}");
            Assert.Less(Mathf.Abs(bounds.center.x), 0.3f, $"{id}: centred");
            Assert.Less(ProceduralEnemyModels.GetBlueprint(kind).TriangleCount, 8000, $"{id}: the boss budget");
        }

        [TestCase(ProceduralEnemyKind.SmokeKing)]
        [TestCase(ProceduralEnemyKind.BlackTailFish)]
        [TestCase(ProceduralEnemyKind.NoveloGuardian)]
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

        // ---------------------------------------------------------------- the arena

        [TestCaseSource(nameof(DeepIds))]
        public void Arena_IsInItsArea_OnARing_FlatAndByATrail(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            OverworldLayout world = OverworldLayouts.StitchedWorld;
            Vector2? found = RegionBossRules.ArenaIn(world, spawn);
            Assert.IsNotNull(found, $"{id}: the arena is not in its area");
            Vector2 arena = found.Value;
            Assert.AreEqual(spawn.MapId, OverworldRules.ZoneAt(world, arena).MapIdOrOwner, id);
            Assert.IsTrue(world.World.Landmarks.Any(l => l.Kind == MapLandmarkKind.StoneRing && Vector2.Distance(l.Position, arena) < 1f), $"{id}: a stone ring");
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

            OverworldRelief relief = OverworldRelief.For(world);
            float centre = relief.HeightAt(arena);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Assert.AreEqual(centre, relief.HeightAt(arena + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 9f), 0.6f, $"{id}: the ring is level");
            }

            Vector2 trail = world.World.Paths.SelectMany(p => new[] { p.A, p.B }).OrderBy(p => Vector2.Distance(p, arena)).First();
            Assert.Less(Vector2.Distance(trail, arena), 40f, $"{id}: the ring stands by a trail");
            float previous = centre;
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
    }
}
