using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the code-built low-poly enemy models (<see cref="ProceduralEnemyModels"/>):
    /// determinism, the part names the animators look for, mobile triangle budgets, sane bounds for
    /// ground seating, and every shipped model-less enemy ending up with a non-empty visual.
    /// </summary>
    public class ProceduralEnemyModelsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<ProceduralEnemyBlueprint> _blueprints = new List<ProceduralEnemyBlueprint>();

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
        }

        private ProceduralEnemyBlueprint Fresh(ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint blueprint = ProceduralEnemyModels.CreateBlueprint(kind);
            _blueprints.Add(blueprint);
            return blueprint;
        }

        private static IEnumerable<ProceduralEnemyKind> Kinds() => ProceduralEnemyModels.AllKinds;

        private static readonly Dictionary<ProceduralEnemyKind, string[]> RequiredParts = new Dictionary<ProceduralEnemyKind, string[]>
        {
            { ProceduralEnemyKind.BatSwarm, new[] { ProceduralEnemyParts.Swarm, "Bat_0", ProceduralEnemyParts.WingLeft, ProceduralEnemyParts.WingRight } },
            { ProceduralEnemyKind.StoneGolem, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight,
                ProceduralEnemyParts.FistLeft, ProceduralEnemyParts.FistRight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight } },
            { ProceduralEnemyKind.Warlord, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Cape, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.Weapon, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight } },
            { ProceduralEnemyKind.RuinGuardian, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.Runes, ProceduralEnemyParts.ArmLeft,
                ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.FistLeft, ProceduralEnemyParts.FistRight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight } },
            { ProceduralEnemyKind.AncientDweller, new[] { ProceduralEnemyParts.Body, ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight,
                ProceduralEnemyParts.ArmLeft, ProceduralEnemyParts.ArmRight, ProceduralEnemyParts.StolenLight, ProceduralEnemyParts.LegLeft, ProceduralEnemyParts.LegRight } },
        };

        [Test]
        public void EveryKind_IsDeterministic([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint a = Fresh(kind);
            ProceduralEnemyBlueprint b = Fresh(kind);

            Assert.AreEqual(a.Parts.Count, b.Parts.Count);
            Assert.AreEqual(a.Meshes.Count, b.Meshes.Count);
            for (int i = 0; i < a.Parts.Count; i++)
            {
                Assert.AreEqual(a.Parts[i].Name, b.Parts[i].Name);
                Assert.AreEqual(a.Parts[i].Parent, b.Parts[i].Parent);
                Assert.AreEqual(a.Parts[i].Position, b.Parts[i].Position);
            }

            for (int m = 0; m < a.Meshes.Count; m++)
            {
                CollectionAssert.AreEqual(a.Meshes[m].vertices, b.Meshes[m].vertices, $"{kind} mesh {a.Meshes[m].name}");
                CollectionAssert.AreEqual(a.Meshes[m].triangles, b.Meshes[m].triangles);
                CollectionAssert.AreEqual(a.Meshes[m].uv, b.Meshes[m].uv);
            }
        }

        [Test]
        public void EveryKind_HasTheNamedPartsTheAnimatorsUse([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            Assert.AreEqual(EnemyVisualSwapper.VisualName, visual.name);

            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Assert.IsNotNull(rig);
            Assert.AreEqual(kind, rig.Kind);
            foreach (string part in RequiredParts[kind])
            {
                Assert.IsNotNull(rig.GetPart(part), $"{kind}: missing part '{part}'");
            }

            // Legs are found by EnemyProceduralAnimator's own naming rule.
            if (kind != ProceduralEnemyKind.BatSwarm)
            {
                Assert.IsTrue(EnemyProceduralAnimator.IsLegName(ProceduralEnemyParts.LegLeft));
                Assert.IsTrue(EnemyProceduralAnimator.IsLegName(ProceduralEnemyParts.LegRight));
            }

            // No other part may accidentally match the leg rule (it would be twitched like a leg).
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (t != visual.transform && t.name != ProceduralEnemyParts.LegLeft && t.name != ProceduralEnemyParts.LegRight)
                {
                    Assert.IsFalse(EnemyProceduralAnimator.IsLegName(t.name), $"{kind}: '{t.name}' reads as a leg");
                }
            }

            Assert.IsEmpty(visual.GetComponentsInChildren<Collider>(true), "procedural models must not add colliders");
        }

        [Test]
        public void BatSwarm_HasFiveToSevenBats_EachWithTwoWings()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.BatSwarm, null);
            _created.Add(visual);
            Transform swarm = visual.GetComponent<ProceduralEnemyRig>().GetPart(ProceduralEnemyParts.Swarm);

            Assert.That(swarm.childCount, Is.InRange(5, 7));
            foreach (Transform bat in swarm)
            {
                StringAssert.StartsWith(ProceduralEnemyParts.BatPrefix, bat.name);
                Assert.IsNotNull(bat.GetComponent<MeshFilter>(), bat.name);
                Assert.IsNotNull(bat.Find(ProceduralEnemyParts.WingLeft), bat.name);
                Assert.IsNotNull(bat.Find(ProceduralEnemyParts.WingRight), bat.name);
            }
        }

        [Test]
        public void EveryKind_StaysWithinItsMobileTriangleBudget([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            ProceduralEnemyBlueprint blueprint = Fresh(kind);
            int budget = ProceduralEnemyModels.TriangleBudget(kind);
            Assert.AreEqual(ProceduralEnemyModels.IsBoss(kind) ? 8000 : 3000, budget);
            Assert.Greater(blueprint.TriangleCount, 200, $"{kind}: suspiciously empty");
            Assert.Less(blueprint.TriangleCount, budget, $"{kind}: {blueprint.TriangleCount} triangles");
            TestContext.WriteLine($"{kind}: {blueprint.TriangleCount} triangles, {blueprint.Meshes.Count} meshes, {blueprint.Parts.Count} parts");
        }

        [Test]
        public void EveryKind_SharesOnePaletteMaterial_AndCachedMeshes()
        {
            var materials = new HashSet<Material>();
            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                GameObject first = ProceduralEnemyModels.Instantiate(kind, null);
                GameObject second = ProceduralEnemyModels.Instantiate(kind, null);
                _created.Add(first);
                _created.Add(second);

                MeshFilter[] a = first.GetComponentsInChildren<MeshFilter>();
                MeshFilter[] b = second.GetComponentsInChildren<MeshFilter>();
                Assert.AreEqual(a.Length, b.Length);
                for (int i = 0; i < a.Length; i++)
                {
                    Assert.AreSame(a[i].sharedMesh, b[i].sharedMesh, $"{kind}: meshes must be shared, not rebuilt per instance");
                }

                foreach (MeshRenderer renderer in first.GetComponentsInChildren<MeshRenderer>())
                {
                    materials.Add(renderer.sharedMaterial);
                }
            }

            Assert.AreEqual(1, materials.Count, "all procedural enemies draw with one shared palette material");
        }

        [Test]
        public void EveryKind_HasSaneBounds_FeetNearThePivot_BossesBigger([ValueSource(nameof(Kinds))] ProceduralEnemyKind kind)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, null);
            _created.Add(visual);
            var root = new GameObject("root");
            _created.Add(root);
            visual.transform.SetParent(root.transform, false);

            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual.transform, out Bounds bounds));
            Assert.That(bounds.min.y, Is.InRange(-0.15f, kind == ProceduralEnemyKind.BatSwarm ? 0.9f : 0.15f), $"{kind}: feet at {bounds.min.y}");
            Assert.That(bounds.size.y, Is.InRange(0.6f, 4f), $"{kind}: height {bounds.size.y}");
            Assert.That(bounds.size.x, Is.InRange(0.5f, 3.5f), $"{kind}: width {bounds.size.x}");
            Assert.That(bounds.size.z, Is.InRange(0.4f, 3.5f), $"{kind}: depth {bounds.size.z}");
            Assert.That(Mathf.Abs(bounds.center.x), Is.LessThan(0.3f), $"{kind}: model must be centred on the pivot");

            if (ProceduralEnemyModels.IsBoss(kind))
            {
                Assert.Greater(bounds.size.y, 2.4f, $"{kind}: a boss must stand taller than the 2 m capsule");
            }
        }

        [Test]
        public void EveryKind_UsesGlowSwatches()
        {
            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                bool glows = false;
                foreach (Mesh mesh in ProceduralEnemyModels.GetBlueprint(kind).Meshes)
                {
                    foreach (Vector2 uv in mesh.uv)
                    {
                        glows |= EnemyModelPalette.IsGlow(EnemyModelPalette.SwatchAt(uv));
                    }
                }

                Assert.IsTrue(glows, $"{kind}: no glowing eyes/cracks/runes/crystals");
            }
        }

        [Test]
        public void Rig_Tick_MovesParts_AndRespectsZeroMotion()
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.BatSwarm, null);
            _created.Add(visual);
            var rig = visual.GetComponent<ProceduralEnemyRig>();
            Transform wing = rig.GetPart(ProceduralEnemyParts.WingRight);
            Quaternion rest = wing.localRotation;

            rig.Tick(0.11f);
            Assert.AreNotEqual(rest, wing.localRotation, "wings flap");

            GameObject golem = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.StoneGolem, null);
            _created.Add(golem);
            var golemRig = golem.GetComponent<ProceduralEnemyRig>();
            Transform arm = golemRig.GetPart(ProceduralEnemyParts.ArmLeft);
            Quaternion armRest = arm.localRotation;
            golemRig.Tick(0.5f, 0f);
            Assert.AreEqual(armRest, arm.localRotation, "zero motion scale keeps the rest pose");

            foreach (ProceduralEnemyKind kind in ProceduralEnemyModels.AllKinds)
            {
                GameObject model = ProceduralEnemyModels.Instantiate(kind, null);
                _created.Add(model);
                Assert.DoesNotThrow(() => model.GetComponent<ProceduralEnemyRig>().Tick(0.016f), kind.ToString());
            }
        }

        [Test]
        public void KindFor_MapsTheFiveModellessEnemies_AndNothingElse()
        {
            Assert.AreEqual(ProceduralEnemyKind.BatSwarm, ProceduralEnemyModels.KindFor("enemy_cave_bat_swarm"));
            Assert.AreEqual(ProceduralEnemyKind.AncientDweller, ProceduralEnemyModels.KindFor("enemy_cave_dweller"));
            Assert.AreEqual(ProceduralEnemyKind.RuinGuardian, ProceduralEnemyModels.KindFor("enemy_desert_ruin_guardian"));
            Assert.AreEqual(ProceduralEnemyKind.StoneGolem, ProceduralEnemyModels.KindFor("enemy_mountain_stone_golem"));
            Assert.AreEqual(ProceduralEnemyKind.Warlord, ProceduralEnemyModels.KindFor("enemy_mountain_warlord"));
            Assert.AreEqual(ProceduralEnemyKind.None, ProceduralEnemyModels.KindFor("enemy_forest_wolf"));
            Assert.AreEqual(ProceduralEnemyKind.None, ProceduralEnemyModels.KindFor((string)null));

            foreach (KeyValuePair<string, ProceduralEnemyKind> entry in ProceduralEnemyModels.KindsById)
            {
                var definition = Resources.Load<EnemyDefinition>("Enemies/" + entry.Key);
                Assert.IsNotNull(definition, entry.Key);
                Assert.AreEqual(definition.IsBoss, ProceduralEnemyModels.IsBoss(entry.Value), $"{entry.Key}: boss flag vs model budget");
            }
        }

        [Test]
        public void EveryShippedEnemy_EndsUpWithANonEmptyVisual()
        {
            EnemyDefinition[] definitions = Resources.LoadAll<EnemyDefinition>("Enemies");
            Assert.IsNotEmpty(definitions);
            foreach (EnemyDefinition definition in definitions)
            {
                var root = new GameObject("Enemy_" + definition.name);
                _created.Add(root);
                CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
                capsule.height = 2f;

                Transform visual = EnemyVisualSwapper.Apply(root, definition);
                Assert.IsNotNull(visual, definition.name);
                Assert.IsNull(visual.Find(EnemyVisualSwapper.SnoutName), $"{definition.name}: still a primitive placeholder");

                int triangles = 0;
                foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh != null && filter.GetComponent<Renderer>() != null)
                    {
                        triangles += filter.sharedMesh.triangles.Length / 3;
                    }
                }

                foreach (SkinnedMeshRenderer skinned in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (skinned.sharedMesh != null)
                    {
                        triangles += skinned.sharedMesh.triangles.Length / 3;
                    }
                }

                Assert.Greater(triangles, 0, $"{definition.name}: empty visual");
            }
        }

        [Test]
        public void BossProceduralModel_IsNotBossTinted_WhenBakedAsDefinitionPrefab()
        {
            GameObject prefab = ProceduralEnemyModels.Instantiate(ProceduralEnemyKind.Warlord, null);
            _created.Add(prefab);
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = "enemy_mountain_warlord";
            _created.Add(definition);
            typeof(EnemyDefinition).GetField("_visualPrefab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, prefab);
            typeof(EnemyDefinition).GetField("_isBoss", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, true);
            typeof(EnemyDefinition).GetField("_visualScale", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, 1f);
            typeof(EnemyDefinition).GetField("_visualLocalEulerAngles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(definition, Vector3.zero);

            var root = new GameObject("root");
            _created.Add(root);
            root.AddComponent<CapsuleCollider>().height = 2f;
            Transform visual = EnemyVisualSwapper.Apply(root, definition);

            Assert.AreEqual(EnemyVisualSource.DefinitionPrefab, visual.GetComponent<EnemyVisualMarker>().Kind);
            foreach (MeshRenderer renderer in visual.GetComponentsInChildren<MeshRenderer>())
            {
                Assert.AreSame(ProceduralEnemyModels.SharedMaterial, renderer.sharedMaterial);
            }

            Assert.IsTrue(EnemyVisualSwapper.TryGetMeshBoundsInRootSpace(root.transform, visual, out Bounds bounds));
            Assert.AreEqual(-1f, bounds.min.y, 0.01f);
        }
    }
}
