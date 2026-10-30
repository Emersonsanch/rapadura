using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Environment;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for prop collision: the pure rules (<see cref="SolidPropRules"/>: kinds, shapes,
    /// trunk measurement, tight-gap and road rules), the click-to-move stall rule
    /// (<see cref="BlockedMoveDetector"/>) and the binder on real GameObjects (trunk-only capsule,
    /// stale rock collider replaced, Workbench trigger kept, idempotent, world-map zone skipped,
    /// gap seal). See Docs/Collision.md.
    /// </summary>
    public class SolidPropRulesTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                if (o != null)
                {
                    Object.DestroyImmediate(o);
                }
            }

            _created.Clear();
        }

        // ------------------------------------------------------------ classification

        [TestCase("Tree_Broadleaf_RO_3", null, true, SolidPropKind.Tree)]
        [TestCase("Tree_Pine_RO_Map4", null, true, SolidPropKind.Tree)]
        [TestCase("Rock_5", "Sphere", false, SolidPropKind.Rock)]
        [TestCase("Rock_Scattered_2", "ProceduralRock_5002", false, SolidPropKind.Rock)]
        [TestCase("Rock1", null, false, SolidPropKind.Rock)]
        [TestCase("ArenaProp_Boulder_4", null, false, SolidPropKind.Rock)]
        [TestCase("Workbench", "Cube", false, SolidPropKind.Box)]
        [TestCase("Bench_Praça", null, false, SolidPropKind.Box)]
        [TestCase("Barraca_Feira", null, false, SolidPropKind.Box)]
        [TestCase("Cerca_02", null, false, SolidPropKind.Box)]
        [TestCase("ArenaProp_Cactus_7", null, false, SolidPropKind.Capsule)]
        [TestCase("Mandacaru_1", null, false, SolidPropKind.Capsule)]
        [TestCase("Tuft_12", "GrassTuft", false, SolidPropKind.None)]
        [TestCase("GrassTuft_Map3", "GrassTuft", false, SolidPropKind.None)]
        [TestCase("Pebble_7", null, false, SolidPropKind.None)]
        [TestCase("Twig_3", null, false, SolidPropKind.None)]
        [TestCase("Rocks", null, false, SolidPropKind.None)]
        [TestCase("NaturalTrees", null, false, SolidPropKind.None)]
        [TestCase("Npc_Villager", null, false, SolidPropKind.None)]
        [TestCase("Enemy_Wolf", null, false, SolidPropKind.None)]
        [TestCase("ArenaSecret_Desert", null, false, SolidPropKind.None)]
        [TestCase("GlobalPostProcessVolume", null, false, SolidPropKind.None)]
        [TestCase("", null, false, SolidPropKind.None)]
        public void Classify_ByAuthoringName(string name, string mesh, bool lod, SolidPropKind expected)
        {
            Assert.AreEqual(expected, SolidPropRules.Classify(name, mesh, lod));
        }

        [Test]
        public void ShapeFor_PrimitivesEverywhere_ConvexOnlyForSmallReadableRocks()
        {
            Assert.AreEqual(SolidColliderShape.TrunkCapsule, SolidPropRules.ShapeFor(SolidPropKind.Tree, true, 3000));
            Assert.AreEqual(SolidColliderShape.ConvexMesh, SolidPropRules.ShapeFor(SolidPropKind.Rock, true, 642));
            Assert.AreEqual(SolidColliderShape.Box, SolidPropRules.ShapeFor(SolidPropKind.Rock, false, 642), "unreadable rock mesh: fitted box");
            Assert.AreEqual(SolidColliderShape.Box, SolidPropRules.ShapeFor(SolidPropKind.Rock, true, SolidPropRules.MaxConvexSourceVertices + 1));
            Assert.AreEqual(SolidColliderShape.Box, SolidPropRules.ShapeFor(SolidPropKind.Box, true, 24));
            Assert.AreEqual(SolidColliderShape.Capsule, SolidPropRules.ShapeFor(SolidPropKind.Capsule, true, 100));
            Assert.AreEqual(SolidColliderShape.None, SolidPropRules.ShapeFor(SolidPropKind.None, true, 100));
        }

        [Test]
        public void Blocks_PebbleSizedRocksAreWalkedOver_BouldersAndTreesBlock()
        {
            Assert.IsFalse(SolidPropRules.Blocks(SolidPropKind.Rock, 0.15f, 0.4f));
            Assert.IsTrue(SolidPropRules.Blocks(SolidPropKind.Rock, 0.15f, 1.2f), "low but wide slab still blocks");
            Assert.IsTrue(SolidPropRules.Blocks(SolidPropKind.Rock, 0.8f, 1.7f));
            Assert.IsTrue(SolidPropRules.Blocks(SolidPropKind.Tree, 4.5f, 3f));
            Assert.IsFalse(SolidPropRules.Blocks(SolidPropKind.None, 4.5f, 3f));
        }

        [Test]
        public void IsUndersized_HalfSizeColliderIsStale_CloseFitIsKept()
        {
            Assert.IsTrue(SolidPropRules.IsUndersized(new Vector2(0.85f, 0.85f), new Vector2(1.7f, 1.7f)), "the old Sphere mesh collider on Rock_*");
            Assert.IsFalse(SolidPropRules.IsUndersized(new Vector2(1.6f, 1.5f), new Vector2(1.7f, 1.7f)));
        }

        // ------------------------------------------------------------ trunk

        [Test]
        public void TryEstimateTrunk_MeasuresTheTrunkNotTheCanopy()
        {
            List<Vector3> points = TreePoints(trunkRadius: 0.25f, canopyRadius: 1.6f);
            Assert.IsTrue(SolidPropRules.TryEstimateTrunk(points, 1f, out TrunkEstimate trunk));

            Assert.AreEqual(0.25f + SolidPropRules.TrunkPadding, trunk.Radius, 0.03f, "radius from the low band only");
            Assert.AreEqual(0f, trunk.Center.x, 0.02f);
            Assert.AreEqual(0f, trunk.Center.z, 0.02f);
            Assert.AreEqual(0f, trunk.Center.y - trunk.Height * 0.5f, 0.001f, "capsule starts at the tree base");
            Assert.GreaterOrEqual(trunk.Height, SolidPropRules.MinTrunkColliderHeight - 0.001f, "taller than the hero");
            Assert.LessOrEqual(trunk.Height, SolidPropRules.MaxTrunkColliderHeight + 0.001f);
        }

        [Test]
        public void TryEstimateTrunk_ScaledTree_LimitsFollowUnitsPerMeter()
        {
            // Same tree authored at half size and shown at 2x: in local units everything halves.
            var points = new List<Vector3>();
            foreach (Vector3 p in TreePoints(0.25f, 1.6f))
            {
                points.Add(p * 0.5f);
            }

            Assert.IsTrue(SolidPropRules.TryEstimateTrunk(points, 0.5f, out TrunkEstimate trunk));
            Assert.AreEqual((0.25f + SolidPropRules.TrunkPadding) * 0.5f, trunk.Radius, 0.02f);
        }

        [Test]
        public void TryEstimateTrunk_ClampsAndRejectsEmpty()
        {
            Assert.IsFalse(SolidPropRules.TryEstimateTrunk(new List<Vector3>(), 1f, out _));
            Assert.IsTrue(SolidPropRules.TryEstimateTrunk(TreePoints(0.01f, 1.5f), 1f, out TrunkEstimate thin));
            Assert.AreEqual(SolidPropRules.MinTrunkRadius, thin.Radius, 0.001f);
        }

        // ------------------------------------------------------------ gaps & roads

        [Test]
        public void ResolveGap_WideOpen_TouchingOrClosed_Untouched()
        {
            float a = 0.3f, b = 0.3f;
            Assert.AreEqual(SolidGapResolution.Passable, SolidPropRules.ResolveGap(3f, ref a, ref b));
            Assert.AreEqual(SolidGapResolution.AlreadyClosed, SolidPropRules.ResolveGap(0.5f, ref a, ref b));
            Assert.AreEqual(0.3f, a);
            Assert.AreEqual(0.3f, b);
        }

        [Test]
        public void ResolveGap_JustTooTight_ShrinksBothWithinLimit()
        {
            float a = 0.3f, b = 0.3f;
            float need = SolidPropRules.PassableGap();
            float distance = a + b + need - 0.05f; // 5 cm short
            Assert.AreEqual(SolidGapResolution.Shrunk, SolidPropRules.ResolveGap(distance, ref a, ref b));
            Assert.AreEqual(need, distance - a - b, 1e-4f, "gap opened to exactly the passable width");
            Assert.GreaterOrEqual(a, 0.3f - SolidPropRules.MaxTrunkShrink - 1e-5f);
        }

        [Test]
        public void ResolveGap_ClearlyNarrowerThanHero_AsksForSeal()
        {
            float a = 0.3f, b = 0.3f;
            Assert.AreEqual(SolidGapResolution.Seal, SolidPropRules.ResolveGap(1.0f, ref a, ref b), "0.4 m gap vs a 0.7 m hero");
            Assert.AreEqual(0.3f, a, "seal leaves the trunks alone");
        }

        [Test]
        public void ResolveGap_FloorsKeepTotalShrinkBounded()
        {
            float a = 0.3f, b = 0.3f;
            float need = SolidPropRules.PassableGap();
            // Already shrunk to the floor by another neighbour: no room left, so seal.
            Assert.AreEqual(SolidGapResolution.Seal, SolidPropRules.ResolveGap(a + b + need - 0.05f, ref a, ref b, 0.3f, 0.3f, SolidPropRules.HeroRadius));
        }

        [Test]
        public void Corridor_TrunkNeverPokesIntoTheRoad_SealsNeverCrossIt()
        {
            Vector2 a = new Vector2(-10f, 0f), b = new Vector2(10f, 0f);
            float clearance = SolidPropRules.CorridorClearance(new Vector2(0f, 1.6f), a, b, 1.5f);
            Assert.AreEqual(0.1f, clearance, 1e-4f);
            Assert.AreEqual(0.4f, SolidPropRules.RadiusAllowedByCorridor(0.4f, 2f, SolidPropRules.MinTrunkRadius));
            Assert.AreEqual(SolidPropRules.MinTrunkRadius, SolidPropRules.RadiusAllowedByCorridor(0.4f, clearance, SolidPropRules.MinTrunkRadius));
            Assert.AreEqual(0.3f, SolidPropRules.RadiusAllowedByCorridor(0.4f, 0.3f, SolidPropRules.MinTrunkRadius), 1e-5f);

            Assert.IsTrue(SolidPropRules.SegmentTouchesCorridor(new Vector2(0f, 3f), new Vector2(0f, -3f), a, b, 1.5f));
            Assert.IsFalse(SolidPropRules.SegmentTouchesCorridor(new Vector2(0f, 3f), new Vector2(1f, 2f), a, b, 1.5f));
        }

        // ------------------------------------------------------------ click-to-move stall

        [Test]
        public void BlockedMoveDetector_HeadOnIntoTrunk_GivesUpAfterStallTime()
        {
            var detector = new BlockedMoveDetector(0.6f, 0.08f);
            Vector3 destination = new Vector3(0f, 0f, 10f);
            Assert.IsFalse(detector.Tick(destination, 5f, true, 0.02f));
            bool stalled = false;
            for (int i = 0; i < 29 && !stalled; i++)
            {
                stalled = detector.Tick(destination, 5f, true, 0.02f);
            }

            Assert.IsFalse(stalled, "0.58 s is not yet a stall");
            Assert.IsTrue(detector.Tick(destination, 5f, true, 0.03f));
        }

        [Test]
        public void BlockedMoveDetector_SlidingAround_RootedOrNewClick_NeverStall()
        {
            var detector = new BlockedMoveDetector(0.6f, 0.08f);
            Vector3 destination = new Vector3(0f, 0f, 10f);
            float distance = 5f;
            for (int i = 0; i < 200; i++)
            {
                distance -= 0.02f; // sliding along the trunk still closes in
                Assert.IsFalse(detector.Tick(destination, distance, true, 0.02f));
            }

            for (int i = 0; i < 200; i++)
            {
                Assert.IsFalse(detector.Tick(destination, distance, false, 0.02f), "rooted/casting frames don't count");
            }

            for (int i = 0; i < 20; i++)
            {
                Assert.IsFalse(detector.Tick(destination, distance, true, 0.02f));
            }

            Assert.IsFalse(detector.Tick(destination + Vector3.right * 2f, distance, true, 0.5f), "re-aimed destination restarts the watch");
        }

        // ------------------------------------------------------------ binder

        [Test]
        public void Bind_Tree_AddsTrunkCapsuleOnly_AndIsIdempotent()
        {
            GameObject tree = MakeTree("Tree_Test_0", Vector3.zero, 1f);
            Assert.IsTrue(SolidPropBinder.Bind(tree));

            CapsuleCollider[] capsules = tree.GetComponentsInChildren<CapsuleCollider>();
            Assert.AreEqual(1, capsules.Length);
            Assert.AreEqual(SolidPropBinder.ColliderName, capsules[0].name);
            Assert.IsFalse(capsules[0].isTrigger);
            Assert.Less(capsules[0].radius, 0.4f, "trunk, not the 1.6 m canopy");
            Assert.IsNull(tree.GetComponentInChildren<Rigidbody>(), "static: no Rigidbody");

            Assert.IsTrue(SolidPropBinder.Bind(tree));
            SolidPropBinder.Bind(tree, refit: true);
            Assert.AreEqual(1, tree.GetComponentsInChildren<Collider>().Length, "re-binding never duplicates");
        }

        [Test]
        public void Bind_RockWithStaleHalfSizeCollider_ReplacedByFittedHull()
        {
            GameObject rock = MakeRock("Rock_Test", Vector3.zero);
            var stale = rock.AddComponent<MeshCollider>();
            stale.sharedMesh = Track(HalfSizeCopy(rock.GetComponent<MeshFilter>().sharedMesh));

            Assert.IsTrue(SolidPropBinder.Bind(rock));

            Collider[] colliders = rock.GetComponentsInChildren<Collider>();
            Assert.AreEqual(1, colliders.Length, "stale collider removed");
            var hull = colliders[0] as MeshCollider;
            Assert.IsNotNull(hull);
            Assert.IsTrue(hull.convex);
            Assert.AreSame(rock.GetComponent<MeshFilter>().sharedMesh, hull.sharedMesh, "fitted to the visual mesh");
        }

        [Test]
        public void Bind_Workbench_KeepsCraftingTrigger_AddsSolidBox()
        {
            GameObject bench = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            bench.name = "Workbench";
            bench.transform.localScale = new Vector3(2f, 1f, 1f);
            var trigger = bench.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2f, 2f, 3f);

            Assert.IsTrue(SolidPropBinder.Bind(bench));

            Assert.IsTrue(trigger.isTrigger, "the crafting zone stays a trigger");
            Transform solid = bench.transform.Find(SolidPropBinder.ColliderName);
            Assert.IsNotNull(solid);
            var box = solid.GetComponent<BoxCollider>();
            Assert.IsFalse(box.isTrigger);
            Assert.AreEqual(Vector3.one, box.size, "fits the cube mesh (world 2x1x1 through the parent scale)");
        }

        [Test]
        public void Bind_GrassAndCharacters_AndWorldMapZone_GetNothing()
        {
            GameObject tuft = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            Object.DestroyImmediate(tuft.GetComponent<Collider>());
            tuft.name = "Tuft_3";
            Assert.IsFalse(SolidPropBinder.Bind(tuft));
            Assert.IsNull(tuft.GetComponentInChildren<Collider>());

            var zone = Track(new GameObject("[WorldMapDressing]"));
            zone.AddComponent<NonSolidPropZone>();
            GameObject mapTree = MakeTree("Tree_Pine_RO_Map1", Vector3.zero, 1f);
            mapTree.transform.SetParent(zone.transform, true);
            Assert.IsFalse(SolidPropBinder.Bind(mapTree));
            Assert.IsNull(mapTree.GetComponentInChildren<Collider>());
        }

        [Test]
        public void BindHierarchy_TwoTreesTooClose_SealedWithThinBox_FarTreesLeftOpen()
        {
            var group = Track(new GameObject("NaturalTrees"));
            MakeTree("Tree_A", new Vector3(0f, 0f, 0f), 1f).transform.SetParent(group.transform, true);
            MakeTree("Tree_B", new Vector3(0.9f, 0f, 0f), 1f).transform.SetParent(group.transform, true);
            MakeTree("Tree_C", new Vector3(6f, 0f, 0f), 1f).transform.SetParent(group.transform, true);

            SolidPropBinder.Report report = SolidPropBinder.BindHierarchy(group.transform);

            Assert.AreEqual(3, report.Solid);
            Assert.AreEqual(1, report.Seals, "only the A-B pair (gap ~0.3 m < hero 0.7 m)");
            Transform seal = group.transform.Find($"{SolidPropBinder.SealPrefix}[Tree_A|Tree_B]");
            Assert.IsNotNull(seal);
            Assert.IsFalse(seal.GetComponent<BoxCollider>().isTrigger);

            SolidPropBinder.Report again = SolidPropBinder.BindHierarchy(group.transform);
            Assert.AreEqual(0, again.Seals, "existing seal reused");
            Assert.AreEqual(0, again.Added);
        }

        // ------------------------------------------------------------ helpers

        /// <summary>Trunk cylinder (0..2.2 m) + canopy sphere (centre 3.2 m): the low band only sees the trunk.</summary>
        internal static List<Vector3> TreePoints(float trunkRadius, float canopyRadius)
        {
            var points = new List<Vector3>();
            for (int ring = 0; ring <= 22; ring++)
            {
                float y = ring * 0.1f;
                for (int s = 0; s < 16; s++)
                {
                    float a = s * Mathf.PI * 2f / 16f;
                    points.Add(new Vector3(Mathf.Cos(a) * trunkRadius, y, Mathf.Sin(a) * trunkRadius));
                }
            }

            for (int lat = 1; lat < 12; lat++)
            {
                float phi = lat * Mathf.PI / 12f;
                for (int s = 0; s < 16; s++)
                {
                    float a = s * Mathf.PI * 2f / 16f;
                    points.Add(new Vector3(0f, 3.2f, 0f) + new Vector3(Mathf.Sin(phi) * Mathf.Cos(a), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(a)) * canopyRadius);
                }
            }

            return points;
        }

        private GameObject MakeTree(string name, Vector3 position, float scale)
        {
            var root = Track(new GameObject(name));
            var model = new GameObject("LOD0");
            model.transform.SetParent(root.transform, false);
            var mesh = new Mesh { name = name + "_Mesh" };
            List<Vector3> points = TreePoints(0.25f, 1.6f);
            mesh.SetVertices(points);
            var indices = new int[points.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.RecalculateBounds();
            Track(mesh);
            model.AddComponent<MeshFilter>().sharedMesh = mesh;
            Renderer renderer = model.AddComponent<MeshRenderer>();
            var lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(0.1f, new[] { renderer }) });
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            return root;
        }

        private GameObject MakeRock(string name, Vector3 position)
        {
            var rock = Track(new GameObject(name));
            Mesh mesh = Track(ProceduralRockGenerator.BuildRockMesh(4242, 2, 0.3f, 0.7f));
            rock.AddComponent<MeshFilter>().sharedMesh = mesh;
            rock.AddComponent<MeshRenderer>();
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(0.85f, 0.6f, 0.85f);
            return rock;
        }

        private static Mesh HalfSizeCopy(Mesh source)
        {
            var vertices = new List<Vector3>();
            source.GetVertices(vertices);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] *= 0.5f;
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(source.triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private T Track<T>(T o) where T : Object
        {
            _created.Add(o);
            return o;
        }
    }
}
