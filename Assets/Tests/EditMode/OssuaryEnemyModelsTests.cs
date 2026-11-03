using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The guardians of the Ossuário dos Artífices (Docs/Dungeons.md): each dungeon asset resolves to its own procedural model
    /// (<see cref="OssuaryEnemyRecipes"/>, through the same <see cref="EnemyVisualSwapper"/> the DungeonDirector's arena uses)
    /// with valid meshes inside the mobile triangle budget, the parts the dungeon behaviours move, the colossus' phase look
    /// (halo from 70%, open rib cage below 30%), and a "Caderno da Vó Edna" page per guardian after the region pages.
    /// </summary>
    public class OssuaryEnemyModelsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<ProceduralEnemyBlueprint> _blueprints = new List<ProceduralEnemyBlueprint>();

        private static readonly Dictionary<string, ProceduralEnemyKind> Expected = new Dictionary<string, ProceduralEnemyKind>
        {
            { DungeonRules.FloatingSkullId, ProceduralEnemyKind.FloatingSkull },
            { DungeonRules.SkeletonWarriorId, ProceduralEnemyKind.SkeletonWarrior },
            { DungeonRules.SkeletonArcherId, ProceduralEnemyKind.SkeletonArcher },
            { DungeonRules.BoneSorcererId, ProceduralEnemyKind.BoneSorcerer },
            { DungeonRules.BoneColossusId, ProceduralEnemyKind.BoneColossus },
        };

        private static IEnumerable<string> GuardianIds() => DungeonRules.EnemyIds;

        [SetUp]
        public void SetUp()
        {
            BiomeRegistry.Invalidate();
            BestiaryCatalog.Invalidate();
        }

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

            foreach (ProceduralEnemyBlueprint blueprint in _blueprints)
            {
                blueprint.DestroyMeshes();
            }

            _created.Clear();
            _blueprints.Clear();
            BestiaryCatalog.Invalidate();
        }

        private static EnemyDefinition Load(string id)
        {
            var definition = Resources.Load<EnemyDefinition>(DungeonRules.EnemyResourcePath(id));
            Assert.IsNotNull(definition, id);
            return definition;
        }

        [Test]
        public void EveryGuardian_HasItsOwnProceduralModel([ValueSource(nameof(GuardianIds))] string id)
        {
            EnemyDefinition definition = Load(id);
            Assert.AreEqual(Expected[id], ProceduralEnemyModels.KindFor(definition));
            Assert.AreEqual(EnemyVisualSource.Procedural, EnemyVisualSwapper.ResolveSource(definition), $"{id}: no more primitive placeholder");
            Assert.AreEqual(definition.IsBoss, ProceduralEnemyModels.IsBoss(Expected[id]));
            Assert.IsTrue(ProceduralEnemyModels.IsOssuaryGuardian(Expected[id]));
        }

        [Test]
        public void EveryGuardianModel_IsAValidMesh_WithinTheMobileBudget([ValueSource(nameof(GuardianIds))] string id)
        {
            ProceduralEnemyKind kind = Expected[id];
            ProceduralEnemyBlueprint blueprint = ProceduralEnemyModels.CreateBlueprint(kind);
            _blueprints.Add(blueprint);

            int budget = ProceduralEnemyModels.TriangleBudget(kind);
            Assert.AreEqual(kind == ProceduralEnemyKind.BoneColossus ? ProceduralEnemyModels.BossTriangleBudget : ProceduralEnemyModels.CommonTriangleBudget, budget);
            Assert.Greater(blueprint.TriangleCount, 200, $"{kind}: suspiciously empty");
            Assert.Less(blueprint.TriangleCount, budget, $"{kind}: {blueprint.TriangleCount} triangles");

            foreach (Mesh mesh in blueprint.Meshes)
            {
                Assert.Greater(mesh.vertexCount, 0, mesh.name);
                Assert.AreEqual(0, mesh.triangles.Length % 3, mesh.name);
                Assert.AreEqual(mesh.vertexCount, mesh.normals.Length, mesh.name);
                Assert.AreEqual(mesh.vertexCount, mesh.uv.Length, mesh.name);
                Assert.IsTrue(mesh.triangles.All(i => i >= 0 && i < mesh.vertexCount), mesh.name);
                Assert.IsTrue(mesh.vertices.All(v => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z)), mesh.name);
                Assert.IsTrue(mesh.uv.All(uv => uv.x > 0f && uv.x < 1f && uv.y > 0f && uv.y < 1f), $"{mesh.name}: UV outside the palette");
            }

            // The new palette row: copper/ivory bones and a glowing soul-fire or sorcery spark in every one.
            var swatches = new HashSet<EnemySwatch>(blueprint.Meshes.SelectMany(m => m.uv).Select(EnemyModelPalette.SwatchAt));
            Assert.IsTrue(swatches.Contains(EnemySwatch.Ivory) || swatches.Contains(EnemySwatch.Copper), $"{kind}: bones of ivory or copper");
            Assert.IsTrue(swatches.Contains(EnemySwatch.SoulFire) || swatches.Contains(EnemySwatch.Sorcery), $"{kind}: glowing eyes");
            Assert.IsTrue(EnemyModelPalette.IsGlow(EnemySwatch.SoulFire) && EnemyModelPalette.IsGlow(EnemySwatch.Sorcery));
        }

        [Test]
        public void EveryGuardian_SpawnsWithItsModel_ThroughTheArenaVisualSwap([ValueSource(nameof(GuardianIds))] string id)
        {
            EnemyDefinition definition = Load(id);
            var root = new GameObject("Enemy_" + id);
            _created.Add(root);
            root.AddComponent<CapsuleCollider>().height = 2f;

            Transform visual = EnemyVisualSwapper.Apply(root, definition);
            Assert.IsNotNull(visual, id);
            Assert.IsNull(visual.Find(EnemyVisualSwapper.SnoutName), $"{id}: still a primitive placeholder");
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig, id);
            Assert.AreEqual(Expected[id], rig.Kind);
            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual, out Bounds bounds));
            Assert.AreEqual(-1f, bounds.min.y, 0.02f, $"{id}: feet on the capsule bottom");
            Assert.DoesNotThrow(() => rig.Tick(0.02f));
        }

        [Test]
        public void SkeletonWarrior_HasTheShieldArmItsGuardRaises_AndTheArcherHoldsItsBowInTheLeftHand()
        {
            GameObject warrior = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.SkeletonWarrior, null);
            _created.Add(warrior);
            var rig = warrior.GetComponent<ProceduralEnemyRig>();
            Transform arm = rig.GetPart(ProceduralEnemyParts.ArmLeft);
            Assert.AreSame(arm, rig.GetPart(ProceduralEnemyParts.Shield).parent, "the shield rides the arm SkeletonGuardBehaviour swings up");

            // The rig never fights the guard behaviour over that arm.
            Quaternion rest = arm.localRotation;
            Transform swordArm = rig.GetPart(ProceduralEnemyParts.ArmRight);
            Quaternion swordRest = swordArm.localRotation;
            rig.AttackPhaseOverride = EnemyAttackVisualPhase.Windup;
            rig.Tick(0.5f);
            Assert.AreEqual(rest, arm.localRotation);
            Assert.Greater(Quaternion.Angle(swordRest, swordArm.localRotation), 30f, "the sword arm rises");

            GameObject archer = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.SkeletonArcher, null);
            _created.Add(archer);
            var archerRig = archer.GetComponent<ProceduralEnemyRig>();
            Assert.AreSame(archerRig.GetPart(ProceduralEnemyParts.ArmLeft), archerRig.GetPart(ProceduralEnemyParts.Weapon).parent);
        }

        [Test]
        public void BoneColossus_ShowsItsPhases_HaloFrom70Percent_OpenRibsBelow30()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.BoneColossus, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform halo = rig.GetPart(ProceduralEnemyParts.Halo);
            Transform ribs = rig.GetPart(ProceduralEnemyParts.RibsLeft);
            Assert.AreEqual(OssuaryEnemyRecipes.HaloSkulls, Enumerable.Range(0, OssuaryEnemyRecipes.HaloSkulls)
                .Count(i => rig.GetPart(ProceduralEnemyParts.HaloSkullPrefix + i) != null));
            Quaternion ribsRest = ribs.localRotation;

            rig.Health01Override = 1f;
            rig.Tick(2f, 0f);
            Assert.Less(halo.localScale.x, 0.05f, "phase 1: the halo is folded away");
            Assert.AreEqual(0f, rig.Cage01);

            rig.Health01Override = 0.5f;
            rig.Tick(2f, 0f);
            Assert.AreEqual(1f, rig.Halo01, 1e-4f);
            Assert.AreEqual(1f, halo.localScale.x, 1e-3f, "phase 2: the skull halo is out");
            Assert.AreEqual(0f, rig.Cage01);

            rig.Health01Override = 0.1f;
            rig.Tick(2f, 0f);
            Assert.AreEqual(1f, rig.Cage01, 1e-4f);
            Assert.Greater(Quaternion.Angle(ribsRest, ribs.localRotation), 60f, "phase 3: the rib cage swings open");
            Assert.Greater(rig.GetPart(ProceduralEnemyParts.Core).localScale.x, 1.2f, "and the core blazes");
        }

        [Test]
        public void EveryGuardian_HasABestiaryPage_UnderTheOssuary_WithLoreAndTheCousinsInBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            foreach (string id in DungeonRules.EnemyIds)
            {
                BestiaryEntry page = BestiaryCatalog.Get(id);
                Assert.IsNotNull(page, $"{id}: no page in the notebook");
                Assert.AreEqual(DungeonRules.OssuaryId, page.DungeonId);
                Assert.AreEqual(DungeonRules.Region, page.Region);
                Assert.AreEqual(id == DungeonRules.BoneColossusId, page.IsBoss);
                Assert.IsNotEmpty(page.Drops, $"{id}: drops from the catacombs' loot table");
                Assert.AreEqual("Ossuário dos Artífices", BestiaryPresenter.RegionName(null, page));

                foreach (string key in new[] { page.LoreKey }.Concat(BestiaryTexts.Cousins.Select(page.CommentKey)))
                {
                    Assert.IsTrue(table.TryGetValue(key, out var entry), $"{key}: missing");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En) || string.IsNullOrWhiteSpace(entry.PtBR), $"{key}: missing a language");
                }
            }

            IReadOnlyList<BestiaryEntry> entries = BestiaryCatalog.Entries;
            CollectionAssert.AreEqual(
                new[] { DungeonRules.BoneSorcererId, DungeonRules.FloatingSkullId, DungeonRules.SkeletonArcherId, DungeonRules.SkeletonWarriorId, DungeonRules.BoneColossusId },
                entries.Where(e => e.IsDungeon).Select(e => e.Id).ToArray(),
                "the guardians, regulars first and the colossus last");
            Assert.AreEqual(entries.Count - DungeonRules.EnemyIds.Count, entries.ToList().FindIndex(e => e.IsDungeon), "after every region page");
        }
    }
}
