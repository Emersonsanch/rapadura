using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The world bosses of Regions 5–16 (Docs/Mapas100.md 30, 35, 40, 46, 52, 58, 64, 70, 76, 82, 90, 96): the asset, the model, the page, the arena and the rules of the fight.</summary>
    public class RegionBossTests
    {
        private static readonly string[] MidIds =
        {
            RegionBossRules.HedammuId, RegionBossRules.SwallowingMireId, RegionBossRules.AydaharId, RegionBossRules.FourGluttonsId, RegionBossRules.OhyaId,
        };

        private static readonly string[] LateIds =
        {
            RegionBossRules.ApepId, RegionBossRules.DreamOfDestructionId, RegionBossRules.GreatDarkId, RegionBossRules.AzCaptainId,
            RegionBossRules.HedammuId, RegionBossRules.SwallowingMireId, RegionBossRules.AydaharId, RegionBossRules.FourGluttonsId, RegionBossRules.OhyaId,
        };

        private static readonly string[] Ids =
        {
            RegionBossRules.WindKingId, RegionBossRules.OldKulId, RegionBossRules.EmberGluttonId,
            RegionBossRules.ApepId, RegionBossRules.DreamOfDestructionId, RegionBossRules.GreatDarkId, RegionBossRules.AzCaptainId,
            RegionBossRules.HedammuId, RegionBossRules.SwallowingMireId, RegionBossRules.AydaharId, RegionBossRules.FourGluttonsId, RegionBossRules.OhyaId,
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

        private static ProceduralEnemyKind KindOf(string id) => ProceduralEnemyModels.KindFor(id);

        [TestCase(1f, RegionBossPhase.Wrath)]
        [TestCase(0.67f, RegionBossPhase.Wrath)]
        [TestCase(0.66f, RegionBossPhase.Storm)]
        [TestCase(0.33f, RegionBossPhase.Storm)]
        [TestCase(0.32f, RegionBossPhase.LastStand)]
        [TestCase(0f, RegionBossPhase.LastStand)]
        public void Phases_FollowHealth(float fraction, RegionBossPhase phase)
        {
            Assert.AreEqual(phase, RegionBossRules.PhaseFor(fraction));
        }

        [Test]
        public void Abilities_GrowWithThePhases()
        {
            Assert.AreEqual(0, RegionBossRules.ShotsFor(RegionBossPhase.Wrath));
            Assert.AreEqual(3, RegionBossRules.ShotsFor(RegionBossPhase.Storm));
            Assert.AreEqual(5, RegionBossRules.ShotsFor(RegionBossPhase.LastStand));
            Assert.IsFalse(RegionBossRules.GreatBlowEnabled(RegionBossPhase.Storm));
            Assert.IsTrue(RegionBossRules.GreatBlowEnabled(RegionBossPhase.LastStand));
        }

        [Test]
        public void Shots_AreCentredOnTheMuzzle()
        {
            Assert.AreEqual(0f, RegionBossRules.ShotOffset(1, 3), 1e-4f);
            Assert.AreEqual(-RegionBossRules.ShotOffset(4, 5), RegionBossRules.ShotOffset(0, 5), 1e-4f);
        }

        [Test]
        public void GreatBlow_MissesWhoeverWalked()
        {
            Vector2 impact = new Vector2(10f, 10f);
            Assert.IsTrue(RegionBossRules.GreatBlowHits(impact, impact + Vector2.right * (RegionBossRules.GreatBlowRadius - 0.1f)));
            Assert.IsFalse(RegionBossRules.GreatBlowHits(impact, impact + Vector2.right * (RegionBossRules.GreatBlowRadius + 0.1f)));
        }

        [Test]
        public void Night_OnlyStrengthensTheOnesTheFichasSay()
        {
            Assert.Greater(RegionBossRules.AbilityMultiplier(true, true), 1f);
            Assert.AreEqual(1f, RegionBossRules.AbilityMultiplier(true, false));
            Assert.AreEqual(1f, RegionBossRules.AbilityMultiplier(false, true));
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.WindKingId).StrongerAtNight);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.OldKulId).StrongerAtNight);
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.EmberGluttonId).StrongerAtNight);
        }

        [Test]
        public void Wake_And_Sleep_HaveAGapBetweenThem()
        {
            Assert.Greater(RegionBossRules.SleepRadius, RegionBossRules.ActiveRadius);
            Assert.IsTrue(RegionBossRules.ShouldWake(RegionBossRules.ActiveRadius));
            Assert.IsFalse(RegionBossRules.ShouldSleep(RegionBossRules.SleepRadius));
            Assert.IsTrue(RegionBossRules.ShouldSleep(RegionBossRules.SleepRadius + 1f));
        }

        [Test]
        public void Boss_SleepsBefore_TheGroundUnderItUnloads()
        {
            Assert.Less(RegionBossRules.SleepRadius, OverworldRules.UnloadRadius);
        }

        [TestCaseSource(nameof(Ids))]
        public void Asset_IsABoss_WithRealRewards_AtTheTopOfItsMap(string id)
        {
            EnemyDefinition boss = Load(id);
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            Assert.IsTrue(boss.IsBoss, id);
            Assert.AreEqual(spawn.Level, boss.Level, id);
            Assert.AreEqual(MapCatalog.Get(spawn.MapId).MonsterLevelMax, spawn.Level, $"{id}: the top of its map's band");
            Assert.IsNotNull(boss.LootItem, id);
            Assert.IsTrue(BossBonusRules.HasBonusDrop(boss), id);
            Assert.Greater(boss.BossExperienceMultiplier, 1, id);
            StringAssert.StartsWith("enemy_mountain_", id, "BalanceInvariantsTests' naming");
            Assert.LessOrEqual(boss.MaxHealth, Load("enemy_cave_dweller").MaxHealth, $"{id}: the level scaler does the rest (BossHealth_DoesNotDropInLaterBiomes)");
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

        [TestCaseSource(nameof(Ids))]
        public void Model_IsABossModel_WithItsFightBuiltIn(string id)
        {
            ProceduralEnemyKind kind = KindOf(id);
            Assert.AreNotEqual(ProceduralEnemyKind.None, kind, id);
            Assert.IsTrue(ProceduralEnemyModels.IsBoss(kind), id);
            CollectionAssert.Contains(ProceduralEnemyModels.AllKinds, kind);
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            Assert.IsNotNull(visual.GetComponent<RegionBossAbilities>(), id);
            ProceduralEnemyRig rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig.GetPart(ProceduralEnemyParts.Core), id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f), id);
            Assert.DoesNotThrow(() => rig.Tick(0.1f, 0f), $"{id}: Reduzir Movimento");
        }

        [TestCase(ProceduralEnemyKind.OldKul)]
        [TestCase(ProceduralEnemyKind.EmberGlutton)]
        [TestCase(ProceduralEnemyKind.DreamOfDestruction)]
        [TestCase(ProceduralEnemyKind.AzCaptain)]
        [TestCase(ProceduralEnemyKind.SwallowingMire)]
        [TestCase(ProceduralEnemyKind.FourGluttons)]
        [TestCase(ProceduralEnemyKind.Ohya)]
        public void TheWalkers_RaiseTheirArms_OnTheWindup(ProceduralEnemyKind kind)
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

        [TestCaseSource(nameof(Ids))]
        public void Core_SwellsAsTheFightGoesOn(string id)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(KindOf(id), null);
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

        [TestCaseSource(nameof(Ids))]
        public void Page_HasLoreFiveCousinsAndAName(string id)
        {
            BestiaryCatalog.Invalidate();
            BestiaryEntry page = BestiaryCatalog.Get(id);
            Assert.IsNotNull(page, id);
            Assert.IsTrue(page.IsBoss, id);
            Assert.AreEqual(BiomeType.Mountain, page.Region, id);
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var keys = new List<string>
            {
                page.LoreKey, RegionBossTexts.PhaseKey(id, RegionBossPhase.Storm), RegionBossTexts.PhaseKey(id, RegionBossPhase.LastStand), RegionBossTexts.GreatBlowKey(id),
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
        }

        [TestCaseSource(nameof(Ids))]
        public void Arena_IsInTheBossArea_OnARing_WithNoClearingOnIt(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            OverworldLayout world = OverworldLayouts.StitchedWorld;
            Vector2? found = RegionBossRules.ArenaIn(world, spawn);
            Assert.IsNotNull(found, $"{id}: the arena is not in its area");
            Vector2 arena = found.Value;
            Assert.AreEqual(spawn.LocalArena + spawn.WorldOffset, arena, id);
            Assert.IsTrue(world.World.Landmarks.Any(l => l.Kind == MapLandmarkKind.StoneRing && Vector2.Distance(l.Position, arena) < 1f), $"{id}: a stone ring marks the arena");
            foreach (MapSpawnZone clearing in world.World.Zones)
            {
                Assert.GreaterOrEqual(Vector2.Distance(clearing.Center, arena), RegionBossRules.ArenaClearRadius, $"{id}: {clearing.Id} is on the arena");
            }

            Assert.IsFalse(MapLayoutRules.InRiver(world.World, arena, 8f), $"{id}: dry ground");
            Assert.IsFalse(world.Settlements.Any(s => s.GroundRect.Contains(arena)), $"{id}: not in a settlement");
        }

        [TestCaseSource(nameof(LateIds))]
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
            float previous = relief.HeightAt(arena);
            int steps = Mathf.CeilToInt(Vector2.Distance(arena, trail) / 4f);
            for (int s = 1; s <= steps; s++)
            {
                Vector2 point = Vector2.Lerp(arena, trail, (float)s / steps);
                float height = relief.HeightAt(point);
                float run = Vector2.Distance(arena, trail) / steps;
                Assert.Less(Mathf.Atan(Mathf.Abs(height - previous) / run) * Mathf.Rad2Deg, WalkableDegrees, $"{id}: steep at {point}");
                previous = height;
            }
        }

        [TestCaseSource(nameof(LateIds))]
        public void LateBosses_HaveAnAbilityOfTheirOwn_AndItsLines(string id)
        {
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(id);
            Assert.AreNotEqual(RegionBossSignature.None, spawn.Signature, id);
            Assert.IsFalse(RegionBossRules.SignatureEnabled(spawn.Signature, RegionBossPhase.Wrath), id);
            Assert.IsTrue(RegionBossRules.SignatureEnabled(spawn.Signature, RegionBossPhase.Storm), id);
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            if (spawn.Signature != RegionBossSignature.FiveForms)
            {
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.SignatureKey(id)), id);
            }

            if (spawn.NeverDies)
            {
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.WithdrawKey(id)), id);
            }
        }

        [Test]
        public void Regions10To12_KeepNoSignature_AndDie()
        {
            foreach (string id in new[] { RegionBossRules.WindKingId, RegionBossRules.OldKulId, RegionBossRules.EmberGluttonId })
            {
                Assert.AreEqual(RegionBossSignature.None, RegionBossRules.SpawnOf(id).Signature, id);
                Assert.IsFalse(RegionBossRules.SpawnOf(id).NeverDies, id);
            }
        }

        [Test]
        public void Fichas_ApepAndTheOthersWithdraw_TheDreamEnds()
        {
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.ApepId).NeverDies);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.ApepId).ReturnsNextNight);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.GreatDarkId).NeverDies);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.AzCaptainId).NeverDies);
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.DreamOfDestructionId).NeverDies);
        }

        [Test]
        public void Apep_ReachesOnlyThoseWhoStayFar_AndPeaksAtMidnight()
        {
            Assert.IsFalse(RegionBossRules.DrinkHits(RegionBossRules.DrinkSafeRadius));
            Assert.IsTrue(RegionBossRules.DrinkHits(RegionBossRules.DrinkSafeRadius + 0.1f));
            Assert.AreEqual(RegionBossRules.MidnightPeakMultiplier, RegionBossRules.MidnightMultiplier(0f), 1e-4f);
            Assert.AreEqual(RegionBossRules.MidnightPeakMultiplier, RegionBossRules.MidnightMultiplier(24f), 1e-4f);
            Assert.AreEqual(1f, RegionBossRules.MidnightMultiplier(12f), 1e-4f);
            Assert.AreEqual(1f, RegionBossRules.MidnightMultiplier(3f), 1e-4f);
            Assert.AreEqual(RegionBossRules.MidnightMultiplier(1f), RegionBossRules.MidnightMultiplier(23f), 1e-4f, "symmetric round midnight");
            Assert.Greater(RegionBossRules.MidnightMultiplier(23.5f), 1f);
        }

        [Test]
        public void Apep_ComesBackOnlyOnALaterNight()
        {
            Assert.IsFalse(RegionBossRules.NightReturnReady(false, true), "the same night doesn't count");
            Assert.IsFalse(RegionBossRules.NightReturnReady(true, false));
            Assert.IsTrue(RegionBossRules.NightReturnReady(true, true));
        }

        [Test]
        public void DreamFall_SparesTheCentre_AndTheFarAway()
        {
            Vector2 impact = new Vector2(5f, 5f);
            Assert.IsFalse(RegionBossRules.DreamFallHits(impact, impact), "whoever stood still");
            Assert.IsTrue(RegionBossRules.DreamFallHits(impact, impact + Vector2.right * RegionBossRules.DreamFallInnerRadius));
            Assert.IsTrue(RegionBossRules.DreamFallHits(impact, impact + Vector2.up * RegionBossRules.DreamFallOuterRadius));
            Assert.IsFalse(RegionBossRules.DreamFallHits(impact, impact + Vector2.up * (RegionBossRules.DreamFallOuterRadius + 0.1f)));
        }

        [Test]
        public void GreatDark_WearsTheFiveFormsInTurn_AndTheyDiffer()
        {
            Assert.AreEqual(GreatDarkForm.Lion, RegionBossRules.FormAt(0f));
            Assert.AreEqual(GreatDarkForm.Eagle, RegionBossRules.FormAt(RegionBossRules.FormSeconds));
            Assert.AreEqual(GreatDarkForm.Demon, RegionBossRules.FormAt(RegionBossRules.FormSeconds * 4f + 0.1f));
            Assert.AreEqual(GreatDarkForm.Lion, RegionBossRules.FormAt(RegionBossRules.FormSeconds * 5f));
            Assert.AreEqual(GreatDarkForm.Lion, RegionBossRules.FormAt(-3f));
            Assert.Greater(RegionBossRules.FormExtraShots(GreatDarkForm.Eagle), 0);
            Assert.AreEqual(0, RegionBossRules.FormExtraShots(GreatDarkForm.Lion));
            Assert.Greater(RegionBossRules.FormDamageMultiplier(GreatDarkForm.Lion), RegionBossRules.FormDamageMultiplier(GreatDarkForm.Fish));
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (GreatDarkForm form in System.Enum.GetValues(typeof(GreatDarkForm)))
            {
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.FormKey(form)), form.ToString());
            }
        }

        [Test]
        public void Greed_TakesAShareOfWhatTheHeroHasLeft()
        {
            Assert.AreEqual(150f, RegionBossRules.GreedDamage(1000f), 1e-3f);
            Assert.Greater(RegionBossRules.GreedDamage(2000f), RegionBossRules.GreedDamage(500f), "the fuller, the worse");
            Assert.AreEqual(1f, RegionBossRules.GreedDamage(0f), 1e-3f);
        }

        [TestCase(ProceduralEnemyKind.Apep, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.DreamOfDestruction, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.GreatDark, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.AzCaptain, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.Hedammu, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.SwallowingMire, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.Aydahar, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.FourGluttons, 0.5f, 3.5f)]
        [TestCase(ProceduralEnemyKind.Ohya, 0.5f, 3.5f)]
        public void LateModels_StayWithinTheBounds_AndGlow(ProceduralEnemyKind kind, float minWidth, float maxSize)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds), kind.ToString());
            Assert.That(bounds.size.x, Is.InRange(minWidth, maxSize), $"{kind}: width");
            Assert.That(bounds.size.z, Is.InRange(0.4f, maxSize), $"{kind}: depth");
            Assert.Greater(bounds.size.y, 2.4f, $"{kind}: height");
            Assert.That(bounds.min.y, Is.InRange(-0.15f, 0.15f), $"{kind}: feet at {bounds.min.y}");
            Assert.Less(Mathf.Abs(bounds.center.x), 0.3f, $"{kind}: centred");
            bool glows = false;
            foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>())
            {
                foreach (Vector2 uv in filter.sharedMesh.uv)
                {
                    glows |= EnemyModelPalette.IsGlow(EnemyModelPalette.SwatchAt(uv));
                }
            }

            Assert.IsTrue(glows, $"{kind}: at least one colour that glows");
        }

        [Test]
        public void Spawns_KeepTheFarListApart_AndTheDirectorKeepsThemAll()
        {
            Assert.AreEqual(RegionBossRules.Spawns.Count + RegionBossRules.MidSpawns.Count, RegionBossRules.AllSpawns.Count);
            CollectionAssert.AreEquivalent(MidIds, RegionBossRules.MidSpawns.Select(s => s.EnemyId));
            CollectionAssert.IsEmpty(RegionBossRules.Spawns.Select(s => s.EnemyId).Intersect(MidIds), "ach_bosses_far_all counts only Regions 10–16");
            CollectionAssert.AllItemsAreUnique(RegionBossRules.AllSpawns.Select(s => s.EnemyId));
            foreach (string id in MidIds)
            {
                Assert.IsTrue(RegionBossRules.IsRegionBoss(id), id);
            }
        }

        [Test]
        public void Fichas_TheMidBossesThatWithdraw()
        {
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.HedammuId).NeverDies, "Hedammu goes back to the bottom");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.SwallowingMireId).NeverDies, "the Lodo is never beaten for good");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.AydaharId).NeverDies, "Aydahar flies away");
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.FourGluttonsId).NeverDies);
            Assert.IsFalse(RegionBossRules.SpawnOf(RegionBossRules.OhyaId).NeverDies);
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.SwallowingMireId).StrongerAtNight, "the mud grows by itself at night");
        }

        [Test]
        public void HedammuBite_ReachesOnlyTheConeInFront()
        {
            Vector2 boss = Vector2.zero;
            Vector2 aim = Vector2.up;
            Assert.IsTrue(RegionBossRules.BiteHits(boss, aim, new Vector2(0f, RegionBossRules.BiteRange - 0.1f)));
            Assert.IsFalse(RegionBossRules.BiteHits(boss, aim, new Vector2(0f, RegionBossRules.BiteRange + 0.1f)), "out of reach");
            Assert.IsFalse(RegionBossRules.BiteHits(boss, aim, new Vector2(4f, 0f)), "beside her");
            Assert.IsFalse(RegionBossRules.BiteHits(boss, aim, new Vector2(0f, -3f)), "behind her");
            Assert.IsTrue(RegionBossRules.BiteHits(boss, aim, new Vector2(2f, 4f)), "inside the cone");
        }

        [Test]
        public void Mire_GrowsInTheLastStand()
        {
            Assert.AreEqual(0f, RegionBossRules.MireRadius(RegionBossPhase.Wrath));
            Assert.Greater(RegionBossRules.MireRadius(RegionBossPhase.LastStand), RegionBossRules.MireRadius(RegionBossPhase.Storm));
            Assert.IsTrue(RegionBossRules.MireHits(RegionBossRules.MireRadiusStorm, RegionBossPhase.Storm));
            Assert.IsFalse(RegionBossRules.MireHits(RegionBossRules.MireRadiusStorm + 0.1f, RegionBossPhase.Storm));
            Assert.IsTrue(RegionBossRules.MireHits(RegionBossRules.MireRadiusStorm + 0.1f, RegionBossPhase.LastStand), "further out in the end");
        }

        [Test]
        public void AydaharBreath_IsALine_ASideStepClearsIt()
        {
            Vector2 origin = Vector2.zero;
            Vector2 aim = new Vector2(0f, 6f);
            Assert.IsTrue(RegionBossRules.BreathHits(origin, aim, aim));
            Assert.IsTrue(RegionBossRules.BreathHits(origin, aim, new Vector2(0f, RegionBossRules.BreathLength - 0.1f)), "the fire goes on past the mark");
            Assert.IsFalse(RegionBossRules.BreathHits(origin, aim, new Vector2(0f, RegionBossRules.BreathLength + 0.5f)));
            Assert.IsFalse(RegionBossRules.BreathHits(origin, aim, aim + Vector2.right * (RegionBossRules.BreathHalfWidth + 0.1f)), "one step aside");
            Assert.IsFalse(RegionBossRules.BreathHits(origin, aim, new Vector2(0f, -4f)), "behind it");
            Assert.IsTrue(RegionBossRules.BreathHits(origin, origin, new Vector2(1f, 0f)), "aimed at its feet");
        }

        [Test]
        public void Gluttons_OneAWeek_TheNextAtNight_EachWithItsSafePlace()
        {
            Assert.AreEqual(GluttonAppetite.Taotie, RegionBossRules.AppetiteFor(0, false));
            Assert.AreEqual(GluttonAppetite.Taotie, RegionBossRules.AppetiteFor(6, false), "the same week");
            Assert.AreEqual(GluttonAppetite.Hundun, RegionBossRules.AppetiteFor(7, false));
            Assert.AreEqual(GluttonAppetite.Taotie, RegionBossRules.AppetiteFor(28, false), "four weeks round");
            Assert.AreEqual(GluttonAppetite.Hundun, RegionBossRules.AppetiteFor(0, true), "the palaces swap at night");
            Assert.AreEqual(GluttonAppetite.Taotie, RegionBossRules.AppetiteFor(21, true));
            Assert.AreEqual(GluttonAppetite.Taowu, RegionBossRules.AppetiteFor(-1, false), "before the epoch still turns");

            Vector2 boss = Vector2.zero;
            Vector2 mark = new Vector2(8f, 0f);
            Assert.IsTrue(RegionBossRules.AppetiteHits(GluttonAppetite.Taotie, mark, boss, mark));
            Assert.IsFalse(RegionBossRules.AppetiteHits(GluttonAppetite.Taotie, mark, boss, mark + Vector2.up * 5f), "Taotie: move");
            Assert.IsFalse(RegionBossRules.AppetiteHits(GluttonAppetite.Hundun, mark, boss, mark), "Hundun: stay on the lantern");
            Assert.IsTrue(RegionBossRules.AppetiteHits(GluttonAppetite.Hundun, mark, boss, mark + Vector2.up * 5f));
            Assert.IsFalse(RegionBossRules.AppetiteHits(GluttonAppetite.Qiongqi, mark, boss, new Vector2(2f, 0f)), "Qiongqi: up close");
            Assert.IsTrue(RegionBossRules.AppetiteHits(GluttonAppetite.Qiongqi, mark, boss, mark));
            Assert.IsTrue(RegionBossRules.AppetiteHits(GluttonAppetite.Taowu, mark, boss, new Vector2(2f, 0f)));
            Assert.IsFalse(RegionBossRules.AppetiteHits(GluttonAppetite.Taowu, mark, boss, mark), "Taowu: keep away");
            Assert.Greater(RegionBossRules.AppetiteMultiplier(GluttonAppetite.Hundun, true), 1f, "Hundun is stronger at night");
            Assert.AreEqual(1f, RegionBossRules.AppetiteMultiplier(GluttonAppetite.Taotie, true));

            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (GluttonAppetite appetite in System.Enum.GetValues(typeof(GluttonAppetite)))
            {
                Assert.IsTrue(table.ContainsKey(RegionBossTexts.AppetiteKey(appetite)), appetite.ToString());
            }
        }

        [Test]
        public void OhyaChain_SparesWhoeverHugsHimOrStaysFar()
        {
            Assert.IsFalse(RegionBossRules.ChainSweepHits(RegionBossRules.ChainInnerRadius - 0.1f));
            Assert.IsTrue(RegionBossRules.ChainSweepHits(RegionBossRules.ChainInnerRadius));
            Assert.IsTrue(RegionBossRules.ChainSweepHits(RegionBossRules.ChainOuterRadius));
            Assert.IsFalse(RegionBossRules.ChainSweepHits(RegionBossRules.ChainOuterRadius + 0.1f));
        }

        [Test]
        public void Arena_IsNullInAWorldWithoutTheRegion()
        {
            OverworldLayout layout = OverworldLayouts.StitchedWorld;
            var wrongArea = new RegionBossSpawn("x", "y", "no_such_area", Vector2.zero, Vector2.zero, 1, EnemyProjectileKind.Ember, false);
            Assert.IsNull(RegionBossRules.ArenaIn(layout, wrongArea));
            Assert.IsNull(RegionBossRules.ArenaIn(null, RegionBossRules.Spawns[0]));
        }
    }
}
