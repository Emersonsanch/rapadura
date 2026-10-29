using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Tests
{
    public class BiomeArenaTerrainTests
    {
        private static readonly BiomeType[] AllBiomes = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };

        /// <summary>EncounterArenaController's default _spawnArenaRadius (enemies are clamped inside it).</summary>
        private const float EncounterSpawnArenaRadius = 18f;

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            BiomeArenaTerrainCache.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            BiomeArenaProfiles.Invalidate();
            EnvironmentOverrides.Reset();
            DestroyTerrainRoots();

            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
            BiomeArenaTerrainCache.Clear();
        }

        /// <summary>Terrain roots go inactive on Restore, so GameObject.Find can't see them.</summary>
        internal static void DestroyTerrainRoots()
        {
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t != null && t.parent == null && (t.name == BiomeArenaTerrain.RootName || t.name == "[BiomeArenaProps]"))
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static float Radius(Vector3 v) => Mathf.Sqrt(v.x * v.x + v.z * v.z);

        // ---------------------------------------------------------------
        // Flat combat circle
        // ---------------------------------------------------------------

        [Test]
        public void Layout_FlatCircleCoversTheSpawnArea_AndRimStartsPastTheFleeRadius()
        {
            Assert.GreaterOrEqual(BiomeArenaTerrainGenerator.FlatRadius, EncounterSpawnArenaRadius + 1f,
                "Every enemy spawn (clamped to 18 m) plus its capsule must stand on the flat floor.");
            Assert.GreaterOrEqual(BiomeArenaTerrainGenerator.FlatRadius,
                BiomeArenaRules.MinPropClearRadius + EncounterSpawnLayout.DefaultMinSpacing);
            Assert.GreaterOrEqual(BiomeArenaTerrainField.RimStart, BiomeArenaTerrainGenerator.WalkableRadius,
                "The raised edge must not block the walk out to the flee radius.");
            Assert.Greater(BiomeArenaTerrainGenerator.HalfExtent, BiomeArenaTerrainField.RimEnd);
        }

        [Test]
        public void CombatCircle_IsFlat_ForEveryBiomeAndVariant()
        {
            const float tolerance = 0.01f;
            float flat = BiomeArenaTerrainGenerator.FlatRadius;

            foreach (BiomeType biome in AllBiomes)
            {
                for (int variant = 0; variant < BiomeArenaTerrainGenerator.VariantsPerBiome; variant++)
                {
                    Mesh ground = BiomeArenaTerrainCache.Get(biome, variant).Ground;
                    Vector3[] vertices = ground.vertices;
                    int[] triangles = ground.triangles;
                    int checkedTriangles = 0;

                    // Every triangle with a corner on the combat circle must be completely flat at y = 0
                    // (its other corners are at most one cell diagonal out, still inside ZeroHeightRadius).
                    for (int t = 0; t < triangles.Length; t += 3)
                    {
                        Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                        if (Mathf.Min(Radius(a), Radius(b), Radius(c)) > flat)
                        {
                            continue;
                        }

                        checkedTriangles++;
                        Assert.That(Mathf.Max(Mathf.Abs(a.y), Mathf.Abs(b.y), Mathf.Abs(c.y)), Is.LessThanOrEqualTo(tolerance),
                            $"{biome}/{variant}: triangle near r={Radius(a):0.0} is not flat.");
                    }

                    Assert.Greater(checkedTriangles, 500, $"{biome}/{variant}: the combat circle should span many triangles.");

                    // And the analytic field agrees everywhere inside the circle.
                    var field = new BiomeArenaTerrainField(biome, variant);
                    var random = new System.Random(variant + 11);
                    for (int i = 0; i < 300; i++)
                    {
                        float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                        float r = Mathf.Sqrt((float)random.NextDouble()) * BiomeArenaTerrainGenerator.FlatRadius;
                        float x = Mathf.Cos(angle) * r, z = Mathf.Sin(angle) * r;
                        Assert.AreEqual(0f, field.Height(x, z), 1e-5f, $"{biome}/{variant} at r={r:0.0}");
                    }
                }
            }
        }

        [Test]
        public void WalkableBand_HasGentleSlopes()
        {
            // Between the flat circle and the flee radius the hero must be able to walk (CharacterController slope limit 45°).
            foreach (BiomeType biome in AllBiomes)
            {
                var field = new BiomeArenaTerrainField(biome, 1);
                for (int i = 0; i < 360; i += 4)
                {
                    for (float r = BiomeArenaTerrainGenerator.FlatRadius; r <= BiomeArenaTerrainGenerator.WalkableRadius; r += 1f)
                    {
                        float x = Mathf.Cos(i * Mathf.Deg2Rad) * r, z = Mathf.Sin(i * Mathf.Deg2Rad) * r;
                        float ny = field.NormalY(x, z);
                        Assert.Greater(ny, Mathf.Cos(30f * Mathf.Deg2Rad), $"{biome}: too steep at r={r}, angle={i}");
                        Assert.Less(field.Height(x, z), 2f, $"{biome}: walkable band too high at r={r}");
                    }
                }
            }
        }

        // ---------------------------------------------------------------
        // Determinism / cache
        // ---------------------------------------------------------------

        [Test]
        public void Generation_IsDeterministicPerSeed()
        {
            foreach (BiomeType biome in AllBiomes)
            {
                Mesh groundA = Track(BiomeArenaTerrainGenerator.BuildGround(new BiomeArenaTerrainField(biome, 2)));
                Mesh groundB = Track(BiomeArenaTerrainGenerator.BuildGround(new BiomeArenaTerrainField(biome, 2)));
                Mesh groundOther = Track(BiomeArenaTerrainGenerator.BuildGround(new BiomeArenaTerrainField(biome, 1)));
                CollectionAssert.AreEqual(groundA.vertices, groundB.vertices, biome.ToString());
                CollectionAssert.AreEqual(groundA.colors32, groundB.colors32, biome.ToString());
                CollectionAssert.AreNotEqual(groundA.vertices, groundOther.vertices, $"{biome}: another seed should change the terrain.");

                Mesh backA = Track(BiomeArenaTerrainGenerator.BuildBackdrop(new BiomeArenaTerrainField(biome, 2)));
                Mesh backB = Track(BiomeArenaTerrainGenerator.BuildBackdrop(new BiomeArenaTerrainField(biome, 2)));
                Assert.Greater(backA.vertexCount, 300, $"{biome}: the backdrop should not be empty.");
                CollectionAssert.AreEqual(backA.vertices, backB.vertices, biome.ToString());
                CollectionAssert.AreEqual(backA.colors32, backB.colors32, biome.ToString());
            }
        }

        [Test]
        public void VariantFor_IsStableAndInRange_EvenForNegativeSeeds()
        {
            foreach (int seed in new[] { 0, 1, 2, 3, -1, -7, int.MinValue, int.MaxValue, 123456789 })
            {
                int variant = BiomeArenaTerrainGenerator.VariantFor(seed);
                Assert.That(variant, Is.InRange(0, BiomeArenaTerrainGenerator.VariantsPerBiome - 1), seed.ToString());
                Assert.AreEqual(variant, BiomeArenaTerrainGenerator.VariantFor(seed));
            }
        }

        [Test]
        public void Cache_ReturnsSameMesh_AndEvictsLeastRecentlyUsed()
        {
            BiomeArenaTerrainCache.Entry first = BiomeArenaTerrainCache.Get(BiomeType.Desert, 1);
            BiomeArenaTerrainCache.Entry again = BiomeArenaTerrainCache.Get(BiomeType.Desert, 1);
            Assert.AreSame(first, again);
            Assert.AreSame(first.Ground, again.Ground);
            Assert.AreSame(first.Backdrop, again.Backdrop);
            Assert.AreEqual(1, BiomeArenaTerrainCache.BuildCount, "A cache hit must not regenerate.");

            BiomeArenaTerrainCache.Entry caves = BiomeArenaTerrainCache.Get(BiomeType.Caves, 1);
            Assert.AreNotSame(first.Ground, caves.Ground);
            Assert.AreEqual(2, BiomeArenaTerrainCache.BuildCount);

            // Fill past capacity: the least recently used (Desert/1) is evicted and destroyed.
            for (int i = 0; i < BiomeArenaTerrainCache.Capacity - 1; i++)
            {
                BiomeArenaTerrainCache.Get(BiomeType.Mountain, i % BiomeArenaTerrainGenerator.VariantsPerBiome);
            }

            Assert.AreEqual(BiomeArenaTerrainCache.Capacity, BiomeArenaTerrainCache.Count);
            Assert.IsTrue(first.Ground == null, "Evicted meshes are destroyed, not leaked.");
            int builds = BiomeArenaTerrainCache.BuildCount;
            BiomeArenaTerrainCache.Entry rebuilt = BiomeArenaTerrainCache.Get(BiomeType.Desert, 1);
            Assert.IsTrue(rebuilt.IsAlive);
            Assert.AreEqual(builds + 1, BiomeArenaTerrainCache.BuildCount);
        }

        // ---------------------------------------------------------------
        // Colliders / backdrop clearance
        // ---------------------------------------------------------------

        [Test]
        public void GroundCollider_CoversTheWalkableArea_AndMatchesTheFlatFloor()
        {
            var terrain = new BiomeArenaTerrain(SceneManager.GetActiveScene());
            var origin = new Vector3(4f, 0.5f, -3f);
            try
            {
                foreach (BiomeType biome in AllBiomes)
                {
                    terrain.Show(biome, 0, origin);
                    MeshCollider collider = terrain.GroundCollider;
                    Assert.IsNotNull(collider);
                    Assert.AreSame(terrain.Current.Ground, collider.sharedMesh, "Collider must be the visual mesh.");
                    Assert.AreEqual(0, collider.gameObject.layer, "Ground must stay on Default (PlayerMotor/FootIK/spawn snap masks).");
                    Assert.IsNull(terrain.BackdropRenderer.GetComponent<Collider>(), "The backdrop has no collider.");

                    float step = 3f;
                    float walk = BiomeArenaTerrainGenerator.WalkableRadius;
                    for (float x = -walk; x <= walk; x += step)
                    {
                        for (float z = -walk; z <= walk; z += step)
                        {
                            float r = Mathf.Sqrt(x * x + z * z);
                            if (r > walk)
                            {
                                continue;
                            }

                            var ray = new Ray(origin + new Vector3(x, 60f, z), Vector3.down);
                            Assert.IsTrue(collider.Raycast(ray, out RaycastHit hit, 200f), $"{biome}: no ground at ({x}, {z}).");
                            if (r <= BiomeArenaTerrainGenerator.FlatRadius)
                            {
                                Assert.AreEqual(origin.y, hit.point.y, 0.01f, $"{biome}: combat floor not flat at ({x}, {z}).");
                                Assert.Greater(hit.normal.y, 0.999f);
                            }
                        }
                    }
                }
            }
            finally
            {
                terrain.Dispose();
            }
        }

        [Test]
        public void Backdrop_KeepsTheSpawnRingAndWalkableAreaClear()
        {
            foreach (BiomeType biome in AllBiomes)
            {
                for (int variant = 0; variant < BiomeArenaTerrainGenerator.VariantsPerBiome; variant++)
                {
                    var field = new BiomeArenaTerrainField(biome, variant);
                    Mesh backdrop = BiomeArenaTerrainCache.Get(biome, variant).Backdrop;
                    foreach (Vector3 v in backdrop.vertices)
                    {
                        float r = Radius(v);
                        if (r >= BiomeArenaTerrainGenerator.WalkableRadius)
                        {
                            continue;
                        }

                        float above = v.y - field.Height(v.x, v.z);
                        bool flush = above <= BiomeArenaTerrainGenerator.MaxDecalHeight;
                        bool overhead = above >= BiomeArenaTerrainGenerator.OverheadClearance;
                        Assert.IsTrue(flush || overhead,
                            $"{biome}/{variant}: backdrop vertex {above:0.00} m above the floor at r={r:0.0} would block the arena.");
                        if (r < BiomeArenaRules.MinPropClearRadius)
                        {
                            Assert.IsTrue(flush, $"{biome}/{variant}: only flush decals may lie inside the enemy spawn ring (r={r:0.0}).");
                        }
                    }
                }
            }
        }

        [Test]
        public void Meshes_StayWithinMobileBudget()
        {
            foreach (BiomeType biome in AllBiomes)
            {
                BiomeArenaTerrainCache.Entry entry = BiomeArenaTerrainCache.Get(biome, 0);
                Assert.LessOrEqual(entry.Ground.vertexCount, 4300, biome.ToString());
                Assert.LessOrEqual(entry.Backdrop.vertexCount, 12000, biome.ToString());
                Assert.IsNotNull(entry.Ground.colors32);
                Assert.AreEqual(entry.Ground.vertexCount, entry.Ground.colors32.Length, "Ground is vertex-coloured.");
            }
        }

        // ---------------------------------------------------------------
        // BiomeArenaDressing integration (Edit Mode objects)
        // ---------------------------------------------------------------

        [Test]
        public void Dressing_SwapsAuthoredGroundForTerrain_SwitchesBiomes_AndRestores()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            _created.Add(ground);
            var host = new GameObject("Arena");
            _created.Add(host);
            var dressing = host.AddComponent<BiomeArenaDressing>();

            BiomeArenaProfile desert = BiomeArenaProfile.CreateBuiltInDefault(BiomeType.Desert);
            BiomeArenaProfile forest = BiomeArenaProfile.CreateBuiltInDefault(BiomeType.Forest);
            _created.Add(desert);
            _created.Add(forest);

            Renderer groundRenderer = ground.GetComponent<Renderer>();
            Collider groundCollider = ground.GetComponent<Collider>();

            dressing.Apply(desert, 5);
            Assert.IsTrue(dressing.IsTerrainShown);
            Assert.IsFalse(groundRenderer.enabled, "The authored plane is hidden under the generated ground.");
            Assert.IsFalse(groundCollider.enabled, "Its collider is off too, so only the terrain collider is floor.");
            Assert.IsFalse(groundRenderer.HasPropertyBlock(), "The hidden plane is not tinted.");
            Transform root = dressing.Terrain.Root;
            Mesh desertMesh = dressing.Terrain.Current.Ground;
            Assert.AreEqual(BiomeType.Desert, dressing.Terrain.Current.Biome);

            // Forest's profile is "neutral" (no tint/props) but still gets its own ground.
            dressing.Apply(forest, 5);
            Assert.IsTrue(dressing.IsTerrainShown);
            Assert.AreSame(root, dressing.Terrain.Root, "The terrain GameObjects are reused across encounters.");
            Assert.AreNotSame(desertMesh, dressing.Terrain.Current.Ground);
            Assert.IsFalse(groundRenderer.enabled);

            dressing.Apply(desert, 5 + BiomeArenaTerrainGenerator.VariantsPerBiome);
            Assert.AreSame(desertMesh, dressing.Terrain.Current.Ground, "Same biome + variant reuses the cached mesh.");

            dressing.Restore();
            Assert.IsFalse(dressing.IsTerrainShown);
            Assert.IsTrue(groundRenderer.enabled);
            Assert.IsTrue(groundCollider.enabled);
        }

        [Test]
        public void Dressing_TerrainCanBeTurnedOffPerProfile()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            _created.Add(ground);
            var host = new GameObject("Arena");
            _created.Add(host);
            var dressing = host.AddComponent<BiomeArenaDressing>();

            BiomeArenaProfile profile = BiomeArenaProfile.CreateBuiltInDefault(BiomeType.Desert);
            profile.SetTerrainForTests(false);
            _created.Add(profile);

            dressing.Apply(profile, 1);
            Assert.IsFalse(dressing.IsTerrainShown);
            Assert.IsTrue(ground.GetComponent<Renderer>().enabled);
            Assert.IsTrue(ground.GetComponent<Renderer>().HasPropertyBlock(), "Without terrain the legacy ground tint still applies.");
            dressing.Restore();
        }

        private Mesh Track(Mesh mesh)
        {
            _created.Add(mesh);
            return mesh;
        }
    }
}
