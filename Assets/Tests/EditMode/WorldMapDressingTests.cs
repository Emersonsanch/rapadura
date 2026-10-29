using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;

namespace Rapadura.Tests
{
    public class WorldMapDressingTests
    {
        private const int Seed = WorldMapDressing.DefaultSeed;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Unregister<WorldMapProgress>();
            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        /// <summary>The authored WorldMap.unity layout: Forest -> Desert -> Mountain -> Caves.</summary>
        private static WorldMapTerrain DefaultTerrain(int seed = Seed, bool river = true)
        {
            var sites = new List<WorldMapSite>
            {
                new WorldMapSite(BiomeType.Forest, new Vector2(0f, 0f)),
                new WorldMapSite(BiomeType.Desert, new Vector2(10f, 0f)),
                new WorldMapSite(BiomeType.Mountain, new Vector2(20f, 4f)),
                new WorldMapSite(BiomeType.Caves, new Vector2(30f, 0f)),
            };

            var paths = new List<WorldMapPath>
            {
                new WorldMapPath(0, 1, sites[0].Position, sites[1].Position),
                new WorldMapPath(1, 2, sites[1].Position, sites[2].Position),
                new WorldMapPath(2, 3, sites[2].Position, sites[3].Position),
            };

            return new WorldMapTerrain(sites, paths, seed, river);
        }

        // ---------------------------------------------------------------
        // Determinism
        // ---------------------------------------------------------------

        [Test]
        public void SameSeed_GivesTheSameLayout()
        {
            List<WorldMapPropPlacement> a = WorldMapDressingLayout.BuildProps(DefaultTerrain(), Seed);
            List<WorldMapPropPlacement> b = WorldMapDressingLayout.BuildProps(DefaultTerrain(), Seed);

            Assert.That(a.Count, Is.GreaterThan(0));
            Assert.That(b.Count, Is.EqualTo(a.Count));
            for (int i = 0; i < a.Count; i++)
            {
                Assert.That(b[i].Kind, Is.EqualTo(a[i].Kind), $"prop {i}");
                Assert.That(b[i].Region, Is.EqualTo(a[i].Region), $"prop {i}");
                Assert.That(Vector3.Distance(b[i].Position, a[i].Position), Is.LessThan(1e-5f), $"prop {i}");
                Assert.That(b[i].Yaw, Is.EqualTo(a[i].Yaw).Within(1e-5f), $"prop {i}");
                Assert.That(b[i].Scale, Is.EqualTo(a[i].Scale).Within(1e-5f), $"prop {i}");
            }

            List<WorldMapCloudPlacement> cloudsA = WorldMapDressingLayout.BuildClouds(DefaultTerrain(), Seed);
            List<WorldMapCloudPlacement> cloudsB = WorldMapDressingLayout.BuildClouds(DefaultTerrain(), Seed);
            Assert.That(cloudsB.Count, Is.EqualTo(cloudsA.Count));
            for (int i = 0; i < cloudsA.Count; i++)
            {
                Assert.That(cloudsB[i].Position, Is.EqualTo(cloudsA[i].Position));
                Assert.That(cloudsB[i].Seed, Is.EqualTo(cloudsA[i].Seed));
            }
        }

        [Test]
        public void SameSeed_GivesTheSameGround()
        {
            WorldMapTerrain a = DefaultTerrain();
            WorldMapTerrain b = DefaultTerrain();
            for (float x = -8f; x <= 38f; x += 2.3f)
            {
                for (float z = -8f; z <= 12f; z += 1.7f)
                {
                    var p = new Vector2(x, z);
                    Assert.That(b.Height(p), Is.EqualTo(a.Height(p)), $"height at {p}");
                    Assert.That(b.GroundColor(p), Is.EqualTo(a.GroundColor(p)), $"colour at {p}");
                }
            }
        }

        [Test]
        public void DifferentSeed_GivesADifferentLayout()
        {
            List<WorldMapPropPlacement> a = WorldMapDressingLayout.BuildProps(DefaultTerrain(1), 1);
            List<WorldMapPropPlacement> b = WorldMapDressingLayout.BuildProps(DefaultTerrain(2), 2);

            bool anyDifferent = a.Count != b.Count;
            for (int i = 0; !anyDifferent && i < a.Count; i++)
            {
                anyDifferent = Vector3.Distance(a[i].Position, b[i].Position) > 0.01f;
            }

            Assert.That(anyDifferent, Is.True);
        }

        // ---------------------------------------------------------------
        // Placement rules
        // ---------------------------------------------------------------

        [Test]
        public void NoProp_StandsOnARoad()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            foreach (WorldMapPropPlacement prop in WorldMapDressingLayout.BuildProps(terrain, Seed))
            {
                float clearance = terrain.DistanceToPath(prop.Ground) - WorldMapTerrain.PathHalfWidth - prop.Radius;
                Assert.That(clearance, Is.GreaterThanOrEqualTo(WorldMapDressingLayout.PathClearance - 1e-4f),
                    $"{prop.Kind} at {prop.Position} touches a road");
            }
        }

        [Test]
        public void NoProp_IsInsideANodeClickRadius()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            foreach (WorldMapPropPlacement prop in WorldMapDressingLayout.BuildProps(terrain, Seed))
            {
                foreach (WorldMapSite site in terrain.Sites)
                {
                    float distance = Vector2.Distance(prop.Ground, site.Position);
                    Assert.That(distance - prop.Radius, Is.GreaterThan(site.ClickRadius + WorldMapDressingLayout.NodeClearance - 1e-4f),
                        $"{prop.Kind} at {prop.Position} is inside {site.Region}'s click radius");

                    if (WorldMapDressingLayout.IsTall(prop.Kind))
                    {
                        Assert.That(distance - prop.Radius, Is.GreaterThanOrEqualTo(WorldMapDressingLayout.TallNodeClearRadius - 1e-4f),
                            $"tall {prop.Kind} at {prop.Position} is too close to {site.Region}");
                    }
                }
            }
        }

        [Test]
        public void TallProps_StayOutOfTheStripBetweenTheCameraAndEachNode()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            foreach (WorldMapPropPlacement prop in WorldMapDressingLayout.BuildProps(terrain, Seed))
            {
                if (!WorldMapDressingLayout.IsTall(prop.Kind))
                {
                    continue;
                }

                foreach (WorldMapSite site in terrain.Sites)
                {
                    Vector2 delta = prop.Ground - site.Position;
                    bool inFront = delta.y < 0f && -delta.y < WorldMapDressingLayout.FrontStripDepth &&
                                   Mathf.Abs(delta.x) < WorldMapDressingLayout.FrontStripHalfWidth + prop.Radius;
                    Assert.That(inFront, Is.False, $"{prop.Kind} at {prop.Position} hides {site.Region} from the camera");
                }
            }
        }

        [Test]
        public void NoProp_OverlapsAnotherProp_OrALandmark()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            List<WorldMapPropPlacement> props = WorldMapDressingLayout.BuildProps(terrain, Seed);
            var landmarks = new List<Vector2>();
            WorldMapDressingLayout.LandmarkPositions(terrain, landmarks);

            for (int i = 0; i < props.Count; i++)
            {
                foreach (Vector2 landmark in landmarks)
                {
                    Assert.That(Vector2.Distance(props[i].Ground, landmark), Is.GreaterThan(WorldMapDressingLayout.LandmarkRadius + props[i].Radius));
                }

                for (int j = i + 1; j < props.Count; j++)
                {
                    float spacing = (props[i].Radius + props[j].Radius) * 0.9f;
                    Assert.That(Vector2.Distance(props[i].Ground, props[j].Ground), Is.GreaterThanOrEqualTo(spacing - 1e-4f),
                        $"{props[i].Kind} and {props[j].Kind} overlap");
                }
            }
        }

        [Test]
        public void Props_StandOnDryLand_AtGroundHeight()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            foreach (WorldMapPropPlacement prop in WorldMapDressingLayout.BuildProps(terrain, Seed))
            {
                Assert.That(terrain.IsDryLand(prop.Ground), Is.True, $"{prop.Kind} at {prop.Position} is in the water");
                Assert.That(prop.Position.y, Is.EqualTo(terrain.Height(prop.Ground)).Within(1e-5f));
                Assert.That(terrain.DistanceToRiver(prop.Ground), Is.GreaterThan(WorldMapTerrain.RiverHalfWidth));
            }
        }

        [Test]
        public void EachRegion_GetsItsOwnPropSet()
        {
            var regions = new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };

            // The sets are disjoint.
            for (int i = 0; i < regions.Length; i++)
            {
                for (int j = i + 1; j < regions.Length; j++)
                {
                    foreach (WorldMapPropKind kind in WorldMapDressingLayout.KindsFor(regions[i]))
                    {
                        CollectionAssert.DoesNotContain(WorldMapDressingLayout.KindsFor(regions[j]), kind, $"{kind} shared by {regions[i]} and {regions[j]}");
                    }
                }
            }

            WorldMapTerrain terrain = DefaultTerrain();
            List<WorldMapPropPlacement> props = WorldMapDressingLayout.BuildProps(terrain, Seed);
            var counts = new Dictionary<BiomeType, int>();
            foreach (WorldMapPropPlacement prop in props)
            {
                CollectionAssert.Contains(WorldMapDressingLayout.KindsFor(prop.Region), prop.Kind);
                Assert.That(terrain.DominantRegion(prop.Ground), Is.EqualTo(prop.Region), $"{prop.Kind} at {prop.Position} is outside {prop.Region}");
                counts[prop.Region] = counts.TryGetValue(prop.Region, out int n) ? n + 1 : 1;
            }

            foreach (BiomeType region in regions)
            {
                Assert.That(counts.TryGetValue(region, out int n) ? n : 0, Is.GreaterThanOrEqualTo(5), $"{region} is nearly bare");
            }

            // Signature props of each region actually show up.
            Assert.That(props.Exists(p => p.Kind == WorldMapPropKind.Tree), Is.True);
            Assert.That(props.Exists(p => p.Kind == WorldMapPropKind.Cactus), Is.True);
            Assert.That(props.Exists(p => p.Kind == WorldMapPropKind.SnowRock || p.Kind == WorldMapPropKind.Peak), Is.True);
            Assert.That(props.Exists(p => p.Kind == WorldMapPropKind.Stalagmite), Is.True);
        }

        [Test]
        public void Density_ScalesThePropCount()
        {
            int full = WorldMapDressingLayout.BuildProps(DefaultTerrain(), Seed).Count;
            int half = WorldMapDressingLayout.BuildProps(DefaultTerrain(), Seed, 0.5f).Count;
            Assert.That(WorldMapDressingLayout.BuildProps(DefaultTerrain(), Seed, 0f), Is.Empty);
            Assert.That(half, Is.LessThan(full));
        }

        // ---------------------------------------------------------------
        // Terrain
        // ---------------------------------------------------------------

        [Test]
        public void Roads_AndNodes_AreFlatWalkableGround()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            foreach (WorldMapSite site in terrain.Sites)
            {
                Assert.That(terrain.Height(site.Position), Is.EqualTo(WorldMapTerrain.GroundLevel).Within(1e-4f), $"{site.Region} node");
                Assert.That(terrain.DominantRegion(site.Position), Is.EqualTo(site.Region));
            }

            foreach (WorldMapPath path in terrain.Paths)
            {
                for (float t = 0f; t <= 1f; t += 0.05f)
                {
                    Vector2 p = Vector2.Lerp(path.A, path.B, t);
                    if (terrain.HasRiver && Vector2.Distance(p, terrain.BridgeCenter) < 2.5f)
                    {
                        continue; // the bridge spans the river there
                    }

                    Assert.That(terrain.Height(p), Is.EqualTo(WorldMapTerrain.GroundLevel).Within(1e-4f), $"road at {p}");
                    Assert.That(terrain.IsDryLand(p), Is.True, $"road at {p} is under water");
                }
            }
        }

        [Test]
        public void Roads_FollowTheTravelGraph()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            // Forest-Mountain are not connected: the midpoint of that line is off-road.
            Vector2 forestMountain = (terrain.Sites[0].Position + terrain.Sites[2].Position) * 0.5f;
            Assert.That(terrain.DistanceToPath(forestMountain), Is.GreaterThan(WorldMapTerrain.PathHalfWidth));
            // Every connected pair is joined by a straight road (the token's travel line).
            foreach (WorldMapPath path in terrain.Paths)
            {
                Assert.That(terrain.DistanceToPath((path.A + path.B) * 0.5f), Is.LessThan(1e-4f));
            }
        }

        [Test]
        public void RegionWeights_SumToOne()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            var weights = new float[WorldMapTerrain.RegionCount];
            for (float x = -10f; x <= 40f; x += 3.1f)
            {
                terrain.RegionWeights(new Vector2(x, 1.5f), weights);
                float sum = 0f;
                foreach (float w in weights)
                {
                    Assert.That(w, Is.InRange(0f, 1f));
                    sum += w;
                }

                Assert.That(sum, Is.EqualTo(1f).Within(1e-4f));
            }
        }

        [Test]
        public void TheIsland_EndsInSea()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            Rect land = terrain.LandBounds;
            var corners = new[] { new Vector2(land.xMin, land.yMin), new Vector2(land.xMax, land.yMax), new Vector2(land.xMin, land.yMax), new Vector2(land.xMax, land.yMin) };
            foreach (Vector2 corner in corners)
            {
                Assert.That(terrain.LandMask(corner), Is.LessThan(0.01f), $"land at {corner}");
                Assert.That(terrain.Height(corner), Is.EqualTo(WorldMapTerrain.SeaLevel).Within(1e-4f));
                Color color = terrain.GroundColor(corner);
                Color sea = WorldMapPalette.ColorOf(WorldMapSwatch.DeepWater);
                Assert.That(Mathf.Abs(color.r - sea.r) + Mathf.Abs(color.g - sea.g) + Mathf.Abs(color.b - sea.b), Is.LessThan(1f / 255f),
                    "the open sea must match the sea frame colour");
            }
        }

        [Test]
        public void Clouds_FloatOverTheSea_AndLeaveTheNearEdgeClear()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            List<WorldMapCloudPlacement> clouds = WorldMapDressingLayout.BuildClouds(terrain, Seed);
            Assert.That(clouds.Count, Is.GreaterThan(8));
            Vector2 center = terrain.LandBounds.center;
            foreach (WorldMapCloudPlacement cloud in clouds)
            {
                var p = new Vector2(cloud.Position.x, cloud.Position.z);
                Assert.That(terrain.LandMask(p), Is.LessThan(0.05f), $"cloud over land at {p}");
                Assert.That(cloud.Position.y, Is.GreaterThan(1f));
                Vector2 offset = p - center;
                bool nearArc = offset.y < 0f && Mathf.Abs(offset.x) < -offset.y * Mathf.Tan(30f * Mathf.Deg2Rad);
                Assert.That(nearArc, Is.False, $"cloud at {p} covers the near (HUD) edge");
            }
        }

        [Test]
        public void Landmarks_StayClearOfRoads_AndOtherNodes()
        {
            WorldMapTerrain terrain = DefaultTerrain();
            for (int i = 0; i < terrain.Sites.Count; i++)
            {
                Vector2 landmark = WorldMapDressingLayout.LandmarkPosition(terrain, i);
                Assert.That(Vector2.Distance(landmark, terrain.Sites[i].Position), Is.EqualTo(WorldMapDressingLayout.LandmarkDistance).Within(1e-4f));
                Assert.That(terrain.DistanceToPath(landmark), Is.GreaterThan(WorldMapTerrain.PathHalfWidth + WorldMapDressingLayout.LandmarkRadius),
                    $"{terrain.Sites[i].Region} landmark sits on a road");
                for (int s = 0; s < terrain.Sites.Count; s++)
                {
                    if (s != i)
                    {
                        Assert.That(Vector2.Distance(landmark, terrain.Sites[s].Position), Is.GreaterThan(4f));
                    }
                }
            }
        }

        [Test]
        public void Palette_CellsAreDistinct_AndInsideTheTexture()
        {
            var seen = new HashSet<Vector2>();
            foreach (WorldMapSwatch swatch in System.Enum.GetValues(typeof(WorldMapSwatch)))
            {
                Vector2 uv = WorldMapPalette.UvOf(swatch);
                Assert.That(uv.x, Is.InRange(0f, 1f));
                Assert.That(uv.y, Is.InRange(0f, 1f));
                Assert.That(seen.Add(uv), Is.True, $"{swatch} shares a palette cell");
            }

            Assert.That(seen.Count, Is.LessThanOrEqualTo(WorldMapPalette.Columns * WorldMapPalette.Rows));
        }

        // ---------------------------------------------------------------
        // Component smoke test
        // ---------------------------------------------------------------

        private WorldMapRegionNode MakeNode(BiomeType region, Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Region_" + region;
            go.transform.position = position;
            go.transform.localScale = new Vector3(2f, 0.2f, 2f);
            _created.Add(go);
            var node = go.AddComponent<WorldMapRegionNode>();
            return node;
        }

        [Test]
        public void Dressing_BuildsTheMap_HidesThePlaceholders_AndShowsNodeStates()
        {
            WorldMapRegionNode forest = MakeNode(BiomeType.Forest, new Vector3(0f, 0f, 0f));
            WorldMapRegionNode desert = MakeNode(BiomeType.Desert, new Vector3(10f, 0f, 0f));
            WorldMapRegionNode mountain = MakeNode(BiomeType.Mountain, new Vector3(20f, 0f, 4f));
            WorldMapRegionNode caves = MakeNode(BiomeType.Caves, new Vector3(30f, 0f, 0f));
            forest.SetDataForTests(BiomeType.Forest, new[] { desert }, false, BiomeType.Forest, 0);
            desert.SetDataForTests(BiomeType.Desert, new[] { forest, mountain }, true, BiomeType.Forest, 3);
            mountain.SetDataForTests(BiomeType.Mountain, new[] { desert, caves }, true, BiomeType.Desert, 3);
            caves.SetDataForTests(BiomeType.Caves, new[] { mountain }, true, BiomeType.Mountain, 3);

            var progress = new WorldMapProgress();
            progress.Unlock(BiomeType.Desert);
            progress.MarkRegionCleared(BiomeType.Forest);
            ServiceLocator.Register(progress);

            var go = new GameObject("[WorldMapDressing]");
            _created.Add(go);
            var dressing = go.AddComponent<WorldMapDressing>();
            dressing.Build(new[] { forest, desert, mountain, caves }, null);

            Assert.That(dressing.IsBuilt, Is.True);
            Assert.That(dressing.Terrain.Paths.Count, Is.EqualTo(3), "roads come from the node connections");
            Assert.That(dressing.Props.Count, Is.GreaterThan(40));
            Assert.That(dressing.PaletteMaterial, Is.Not.Null);
            Assert.That(dressing.GroundMaterial, Is.Not.Null);
            Assert.That(forest.GetComponent<Renderer>().enabled, Is.False, "the old disc is replaced by the landmark");
            Assert.That(forest.GetComponent<Collider>().enabled, Is.True, "the node stays clickable");
            Assert.That(forest.GetComponentInChildren<BoxCollider>(), Is.Not.Null, "the landmark is clickable as the node");

            Assert.That(dressing.TryGetLandmarkState(BiomeType.Forest, out bool fog, out bool banner), Is.True);
            Assert.That(fog, Is.False);
            Assert.That(banner, Is.True, "cleared region flies the banner");
            dressing.TryGetLandmarkState(BiomeType.Desert, out fog, out banner);
            Assert.That(fog, Is.False);
            Assert.That(banner, Is.False);
            dressing.TryGetLandmarkState(BiomeType.Caves, out fog, out banner);
            Assert.That(fog, Is.True, "locked region is veiled in fog");

            progress.Unlock(BiomeType.Caves);
            dressing.RefreshStates();
            dressing.TryGetLandmarkState(BiomeType.Caves, out fog, out _);
            Assert.That(fog, Is.False);

            // Every static prop shares one material: the palette.
            foreach (MeshRenderer renderer in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.gameObject.name != "Ground" && renderer.GetComponentInParent<LODGroup>() == null)
                {
                    Assert.That(renderer.sharedMaterial, Is.SameAs(dressing.PaletteMaterial), renderer.gameObject.name);
                }
            }

            dressing.Restore();
            Assert.That(forest.GetComponent<Renderer>().enabled, Is.True, "destroying the dressing restores the placeholders");
            Assert.That(forest.GetComponentInChildren<BoxCollider>(), Is.Null);
        }
    }
}
