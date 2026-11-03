using System.Collections.Generic;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// Turns a <see cref="MapLayout"/> into scene objects (map mode of Main.unity, <see cref="MapModeController"/>):
    /// <list type="bullet">
    /// <item>the settlement through the town kit (<see cref="CityTownView"/>: buildings, plaza props, keepers, its own
    /// colliders, safe zone, respawn and night lights), with the city's gate triggers removed (the map has its own exits)
    /// and the Cais da Barca turned into a <see cref="MapExitTrigger"/>;</item>
    /// <item>the greybox ground of the whole map as one mesh (field, trails, clearings, river banks and water, fords) with
    /// one ground slab collider, the river blockers (invisible boxes everywhere but the crossings) and the log bridges;</item>
    /// <item>the wild props (trees, rocks, bushes) and landmarks as named objects with cached meshes, given their solid
    /// colliders by <see cref="SolidPropBinder"/> (trails registered as keep-clear corridors first);</item>
    /// <item>the exits and one <see cref="MapMonsterSpawner"/> per spawn zone.</item>
    /// </list>
    /// Everything shares <see cref="TownPalette.SharedMaterial"/>; meshes are cached in <see cref="TownMeshCache"/>.
    /// </summary>
    public sealed class MapAreaView : MonoBehaviour
    {
        public const string RootPrefix = "MapArea_";
        private const float BlockerHeight = 3f;

        private readonly List<MapExitTrigger> _exits = new List<MapExitTrigger>();
        private readonly List<MapMonsterSpawner> _spawners = new List<MapMonsterSpawner>();
        private readonly List<SolidPropBinder.Corridor> _corridors = new List<SolidPropBinder.Corridor>();

        public MapLayout Layout { get; private set; }
        public CityTownView Town { get; private set; }
        public Transform Wild { get; private set; }
        public IReadOnlyList<MapExitTrigger> Exits => _exits;
        public IReadOnlyList<MapMonsterSpawner> Spawners => _spawners;
        public int RiverBlockerCount { get; private set; }
        public int WildPropCount { get; private set; }
        public SolidPropBinder.Report SolidReport { get; private set; }

        public static MapAreaView Build(MapLayout layout, GameObject enemyPrefab, Transform parent = null)
        {
            var root = new GameObject(RootPrefix + layout.MapId);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            MapAreaView view = root.AddComponent<MapAreaView>();
            view.Layout = layout;
            view.BuildAll(enemyPrefab);
            return view;
        }

        private void OnDestroy()
        {
            foreach (SolidPropBinder.Corridor corridor in _corridors)
            {
                SolidPropBinder.KeepClear.Remove(corridor);
            }

            _corridors.Clear();
        }

        private void BuildAll(GameObject enemyPrefab)
        {
            Material material = TownPalette.SharedMaterial;
            TownStyle style = TownStyles.For(Layout.Biome);
            string key = "map:" + Layout.MapId;

            // Step 2: the settlement (town kit).
            Town = CityTownView.Build(Layout.Town, transform, key);
            foreach (CityExitTrigger cityExit in Town.Exits)
            {
                if (cityExit == null)
                {
                    continue;
                }

                // The city's gates open onto the map itself; only the map's own exits lead anywhere.
                foreach (Collider c in cityExit.GetComponents<Collider>())
                {
                    c.enabled = false;
                    Destroy(c);
                }

                Destroy(cityExit);
            }

            // Step 1: greybox ground and water.
            Mesh ground = TownMeshCache.Get(key + ":ground", b => BuildGround(b, style));
            AddRenderer(transform, "MapGround", ground, material, ShadowCastingMode.Off);
            BuildGroundColliders();
            BuildRivers(material, key);

            // Step 3: wild props and landmarks, solid through SolidPropBinder.
            foreach (MapPath path in Layout.Paths)
            {
                var corridor = new SolidPropBinder.Corridor(new Vector3(path.A.x, 0f, path.A.y), new Vector3(path.B.x, 0f, path.B.y), path.Width * 0.5f);
                _corridors.Add(corridor);
                SolidPropBinder.KeepClear.Add(corridor);
            }

            Wild = new GameObject("Wild").transform;
            Wild.SetParent(transform, false);
            BuildWildProps(material, style);
            BuildLandmarks(material, key);
            SolidReport = SolidPropBinder.BindHierarchy(Wild);

            // Steps 4 and 6: exits and spawners.
            BuildExits();
            int seed = (int)TownStyle.Hash(Layout.MapId, 77);
            foreach (MapSpawnZone zone in Layout.Zones)
            {
                var go = new GameObject("Spawner_" + zone.Id);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(zone.Center.x, 0f, zone.Center.y);
                MapMonsterSpawner spawner = go.AddComponent<MapMonsterSpawner>();
                spawner.Configure(Layout, zone, enemyPrefab, seed ^ (int)TownStyle.Hash(zone.Id, 3));
                _spawners.Add(spawner);
            }
        }

        // ------------------------------------------------------------------
        // Ground
        // ------------------------------------------------------------------

        private void BuildGround(TownMeshBuilder b, TownStyle style)
        {
            const float cell = 8f;
            const float y0 = -0.01f;
            Rect r = Layout.Bounds;
            Rect outer = CityGeometry.Inflate(r, 14f);
            int ix = 0;
            for (float x = outer.xMin; x < outer.xMax; x += cell, ix++)
            {
                int iz = 0;
                for (float z = outer.yMin; z < outer.yMax; z += cell, iz++)
                {
                    string k = Layout.MapId + ix + "_" + iz;
                    TownSwatch swatch = TownStyle.Hash01(k, 1) < 0.3f ? style.GroundAlt : style.Ground;
                    b.Flat(x, z, Mathf.Min(x + cell, outer.xMax), Mathf.Min(z + cell, outer.yMax), y0, swatch);
                }
            }

            foreach (Vector2 c in Layout.Clearings)
            {
                b.Disc(new Vector3(c.x, 0.004f, c.y), 9f, 18, TownSwatch.GrassDark);
                b.Disc(new Vector3(c.x, 0.006f, c.y), 4.5f, 14, TownSwatch.Dirt);
            }

            foreach (MapPath path in Layout.Paths)
            {
                Strip(b, path.A, path.B, path.Width * 0.5f + 0.25f, 0.008f, TownSwatch.DirtDark);
                Strip(b, path.A, path.B, path.Width * 0.5f, 0.012f, style.Street);
            }

            foreach (MapRiver river in Layout.Rivers)
            {
                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    Strip(b, river.Points[i], river.Points[i + 1], river.Width * 0.5f + 1.4f, 0.014f, TownSwatch.Sand);
                }

                foreach (Vector2 p in river.Points)
                {
                    b.Disc(new Vector3(p.x, 0.014f, p.y), river.Width * 0.5f + 1.4f, 12, TownSwatch.Sand);
                }

                for (int i = 0; i + 1 < river.Points.Count; i++)
                {
                    Strip(b, river.Points[i], river.Points[i + 1], river.Width * 0.5f, 0.02f, TownSwatch.Water);
                }

                foreach (Vector2 p in river.Points)
                {
                    b.Disc(new Vector3(p.x, 0.02f, p.y), river.Width * 0.5f, 12, TownSwatch.Water);
                }

                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (crossing.IsBridge)
                    {
                        continue;
                    }

                    // The ford: shallow, pale sand under the water and a line of stepping stones.
                    b.Disc(new Vector3(crossing.At.x, 0.024f, crossing.At.y), crossing.Width * 0.5f, 14, TownSwatch.SandDark);
                    Vector2 across = RiverNormal(river, crossing.At);
                    for (int s = -3; s <= 3; s++)
                    {
                        Vector2 p = crossing.At + across * (s * 1.3f) + new Vector2(-across.y, across.x) * ((s & 1) * 0.6f - 0.3f);
                        b.Crown(new Vector3(p.x, 0.05f, p.y), new Vector3(0.8f, 0.18f, 0.7f), TownSwatch.Stone, s * 37f);
                    }
                }
            }
        }

        private static void Strip(TownMeshBuilder b, Vector2 a, Vector2 c, float halfWidth, float y, TownSwatch swatch)
        {
            Vector2 d = c - a;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            d.Normalize();
            var n = new Vector2(-d.y, d.x) * halfWidth;
            b.QuadFacing(new Vector3(a.x + n.x, y, a.y + n.y), new Vector3(c.x + n.x, y, c.y + n.y), new Vector3(c.x - n.x, y, c.y - n.y),
                new Vector3(a.x - n.x, y, a.y - n.y), Vector3.up, swatch);
        }

        /// <summary>Unit vector across the river at <paramref name="at"/> (perpendicular to the nearest segment).</summary>
        public static Vector2 RiverNormal(MapRiver river, Vector2 at)
        {
            float best = float.MaxValue;
            Vector2 dir = Vector2.right;
            for (int i = 0; i + 1 < river.Points.Count; i++)
            {
                float d = MapLayoutRules.DistanceToSegment(at, river.Points[i], river.Points[i + 1]);
                if (d < best)
                {
                    best = d;
                    dir = (river.Points[i + 1] - river.Points[i]).normalized;
                }
            }

            return new Vector2(-dir.y, dir.x);
        }

        private void BuildGroundColliders()
        {
            var go = new GameObject("MapColliders");
            go.transform.SetParent(transform, false);
            go.isStatic = true;
            Rect r = Layout.Bounds;
            var slab = go.AddComponent<BoxCollider>();
            slab.center = new Vector3(r.center.x, -0.5f, r.center.y);
            slab.size = new Vector3(r.width + 20f, 1f, r.height + 20f);
        }

        private void BuildRivers(Material material, string key)
        {
            int index = 0;
            foreach (MapRiver river in Layout.Rivers)
            {
                var blockers = new GameObject("RiverBlockers_" + index);
                blockers.transform.SetParent(transform, false);
                blockers.isStatic = true;
                foreach ((Vector2 center, float length, float yaw) in MapLayoutRules.RiverBlockers(river))
                {
                    var piece = new GameObject("RiverBlock");
                    piece.transform.SetParent(blockers.transform, false);
                    piece.transform.localPosition = new Vector3(center.x, BlockerHeight * 0.5f, center.y);
                    piece.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                    var box = piece.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(1f, river.Width - 1.2f), BlockerHeight, length);
                    RiverBlockerCount++;
                }

                int bridgeIndex = 0;
                foreach (MapCrossing crossing in river.Crossings)
                {
                    if (!crossing.IsBridge)
                    {
                        continue;
                    }

                    Vector2 across = RiverNormal(river, crossing.At);
                    float yaw = CityGeometry.YawFacing(across);
                    float span = river.Width + 4f;
                    var bridge = new GameObject("Bridge_" + index + "_" + bridgeIndex++);
                    bridge.transform.SetParent(transform, false);
                    bridge.transform.localPosition = new Vector3(crossing.At.x, 0f, crossing.At.y);
                    bridge.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                    float width = crossing.Width;
                    Mesh mesh = TownMeshCache.Get(key + ":bridge:" + width.ToString("0.0") + ":" + span.ToString("0.0"), b => BridgeMesh(b, width, span));
                    AddRenderer(bridge.transform, "Deck", mesh, material, ShadowCastingMode.On);
                    var deck = bridge.AddComponent<BoxCollider>();
                    deck.center = new Vector3(0f, 0.09f, 0f);
                    deck.size = new Vector3(width, 0.18f, span);

                    // Rails: the hero stays on the deck.
                    foreach (float side in new[] { -1f, 1f })
                    {
                        var rail = bridge.AddComponent<BoxCollider>();
                        rail.center = new Vector3(side * (width * 0.5f + 0.1f), 0.6f, 0f);
                        rail.size = new Vector3(0.2f, 1.2f, span - 2.6f);
                    }
                }

                index++;
            }
        }

        private static void BridgeMesh(TownMeshBuilder b, float width, float span)
        {
            for (float z = -span * 0.5f; z < span * 0.5f - 0.01f; z += 0.5f)
            {
                b.Box(new Vector3(0f, 0.11f, z + 0.25f), new Vector3(width, 0.12f, 0.46f), ((int)(z * 2f) & 1) == 0 ? TownSwatch.WoodLight : TownSwatch.WoodWeathered);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                b.Log(new Vector3(side * width * 0.5f, 0.05f, -span * 0.5f), new Vector3(side * width * 0.5f, 0.05f, span * 0.5f), 0.16f, TownSwatch.WoodDark);
                b.Log(new Vector3(side * (width * 0.5f + 0.1f), 1.0f, -span * 0.5f + 1.3f), new Vector3(side * (width * 0.5f + 0.1f), 1.0f, span * 0.5f - 1.3f), 0.06f, TownSwatch.Walnut, 5);
                for (float z = -span * 0.5f + 1.3f; z <= span * 0.5f - 1.3f + 0.01f; z += 2.2f)
                {
                    b.Log(new Vector3(side * (width * 0.5f + 0.1f), 0.1f, z), new Vector3(side * (width * 0.5f + 0.1f), 1.05f, z), 0.06f, TownSwatch.WoodDark, 5);
                }
            }
        }

        // ------------------------------------------------------------------
        // Wild props and landmarks
        // ------------------------------------------------------------------

        private void BuildWildProps(Material material, TownStyle style)
        {
            int i = 0;
            foreach (CityProp prop in Layout.WildProps)
            {
                string prefix;
                switch (prop.Kind)
                {
                    case CityPropKind.Tree: prefix = "Tree_"; break;
                    case CityPropKind.Rock: prefix = "Boulder_"; break;
                    case CityPropKind.Bush: prefix = "Bush_"; break;
                    default: prefix = "Prop_"; break;
                }

                float scale = prop.Scale <= 0f ? 1f : prop.Scale;
                float bucket = Mathf.Round(scale * 10f) / 10f;
                CityProp captured = new CityProp { Kind = prop.Kind, Variant = prop.Variant, Scale = bucket, Yaw = prop.Kind == CityPropKind.Rock ? prop.Yaw : 0f };
                string meshKey = "mapprop:" + Layout.Biome + ":" + prop.Kind + ":" + prop.Variant + ":" + bucket.ToString("0.0")
                    + (prop.Kind == CityPropKind.Rock ? ":" + Mathf.Round(prop.Yaw / 45f) : string.Empty);
                Mesh mesh = TownMeshCache.Get(meshKey, b => TownKit.Prop(b, captured, style));

                var go = new GameObject(prefix + Layout.MapId + "_" + i++);
                go.transform.SetParent(Wild, false);
                go.transform.localPosition = new Vector3(prop.Position.x, 0f, prop.Position.y);
                go.transform.localRotation = prop.Kind == CityPropKind.Rock ? Quaternion.identity : Quaternion.Euler(0f, prop.Yaw, 0f);
                go.isStatic = true;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = Layout.Bounds.Contains(prop.Position) ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                WildPropCount++;
            }
        }

        private void BuildLandmarks(Material material, string key)
        {
            int i = 0;
            foreach (MapLandmark landmark in Layout.Landmarks)
            {
                float s = landmark.Scale <= 0f ? 1f : landmark.Scale;
                switch (landmark.Kind)
                {
                    case MapLandmarkKind.StoneRing:
                        for (int k = 0; k < 9; k++)
                        {
                            float a = k * Mathf.PI * 2f / 9f;
                            var p = landmark.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 7.5f * s;
                            int variant = k % 3;
                            Mesh stone = TownMeshCache.Get(key + ":ringstone:" + variant, b => RingStone(b, variant));
                            Place("Rock_ring_" + i + "_" + k, stone, material, p, a * Mathf.Rad2Deg + 90f);
                        }

                        break;
                    case MapLandmarkKind.WaterMill:
                        Place("House_mill_" + i, TownMeshCache.Get(key + ":mill", WaterMill), material, landmark.Position, landmark.Yaw);
                        break;
                    case MapLandmarkKind.Signpost:
                        Place("Post_sign_" + i, TownMeshCache.Get(key + ":signpost", Signpost), material, landmark.Position, landmark.Yaw);
                        break;
                    case MapLandmarkKind.Campfire:
                        Place("Tuft_campfire_" + i, TownMeshCache.Get(key + ":campfire", Campfire), material, landmark.Position, landmark.Yaw);
                        break;
                    case MapLandmarkKind.MelonBed:
                        Place("Flower_melons_" + i, TownMeshCache.Get(key + ":melons", MelonBed), material, landmark.Position, landmark.Yaw);
                        break;
                }

                i++;
            }
        }

        private void Place(string objectName, Mesh mesh, Material material, Vector2 at, float yaw)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(Wild, false);
            go.transform.localPosition = new Vector3(at.x, 0f, at.y);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.isStatic = true;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static void RingStone(TownMeshBuilder b, int variant)
        {
            float h = 2.2f + variant * 0.45f;
            b.Box(new Vector3(0f, h * 0.5f, 0f), new Vector3(0.9f, h, 0.55f), TownSwatch.Stone, 0f, TownSwatch.StoneDark);
            b.Crown(new Vector3(0f, h, 0f), new Vector3(0.95f, 0.35f, 0.6f), TownSwatch.Moss);
        }

        private static void WaterMill(TownMeshBuilder b)
        {
            b.Box(new Vector3(0f, 1.5f, 0f), new Vector3(4.2f, 3f, 3.2f), TownSwatch.Adobe, 0f, TownSwatch.Mud);
            b.Box(new Vector3(0f, 3.1f, 0f), new Vector3(4.5f, 0.2f, 3.5f), TownSwatch.Mud);
            b.Box(new Vector3(0f, 0.9f, -1.62f), new Vector3(0.9f, 1.8f, 0.06f), TownSwatch.DoorPlank);
            // The wheel on the east wall: paddles on a vertical circle, the axle and the race of water under it.
            const float r = 1.6f;
            for (int k = 0; k < 12; k++)
            {
                float a = k * Mathf.PI * 2f / 12f;
                b.Box(new Vector3(2.45f, r + 0.2f + Mathf.Sin(a) * r, Mathf.Cos(a) * r), new Vector3(0.5f, 0.32f, 0.32f), TownSwatch.WoodWeathered);
            }

            b.Log(new Vector3(2.1f, r + 0.2f, 0f), new Vector3(2.9f, r + 0.2f, 0f), 0.14f, TownSwatch.WoodDark);
            b.Flat(2.0f, -3.5f, 2.9f, 3.5f, 0.03f, TownSwatch.Water);
        }

        private static void Signpost(TownMeshBuilder b)
        {
            b.Log(new Vector3(0f, 0f, 0f), new Vector3(0f, 2.4f, 0f), 0.08f, TownSwatch.WoodDark);
            b.Box(new Vector3(0.45f, 2.0f, 0f), new Vector3(1.0f, 0.26f, 0.06f), TownSwatch.WoodLight, 0f, TownSwatch.Beam);
            b.Box(new Vector3(-0.4f, 1.6f, 0f), new Vector3(0.9f, 0.24f, 0.06f), TownSwatch.WoodLight, 18f, TownSwatch.Beam);
        }

        private static void Campfire(TownMeshBuilder b)
        {
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI * 2f / 8f;
                b.Crown(new Vector3(Mathf.Cos(a) * 0.7f, 0.08f, Mathf.Sin(a) * 0.7f), new Vector3(0.34f, 0.2f, 0.3f), TownSwatch.Stone, k * 40f);
            }

            b.Log(new Vector3(-0.45f, 0.1f, -0.2f), new Vector3(0.45f, 0.3f, 0.2f), 0.07f, TownSwatch.WoodDark);
            b.Log(new Vector3(-0.3f, 0.3f, 0.4f), new Vector3(0.3f, 0.1f, -0.4f), 0.07f, TownSwatch.WoodDark);
            b.Crown(new Vector3(0f, 0.25f, 0f), new Vector3(0.45f, 0.5f, 0.45f), TownSwatch.Ember);
            b.Box(new Vector3(-1.4f, 0.22f, 0f), new Vector3(0.4f, 0.44f, 1.4f), TownSwatch.WoodWeathered);
        }

        private static void MelonBed(TownMeshBuilder b)
        {
            b.Flat(-2.2f, -1.3f, 2.2f, 1.3f, 0.02f, TownSwatch.DirtDark);
            for (int k = 0; k < 10; k++)
            {
                float x = -1.8f + (k % 5) * 0.9f;
                float z = k < 5 ? -0.6f : 0.6f;
                b.Crown(new Vector3(x, 0.12f, z), new Vector3(0.5f, 0.32f, 0.42f), (k & 1) == 0 ? TownSwatch.LeafGold : TownSwatch.Fruit, k * 25f);
                b.Crown(new Vector3(x + 0.3f, 0.08f, z + 0.25f), new Vector3(0.5f, 0.12f, 0.5f), TownSwatch.Vine);
            }
        }

        // ------------------------------------------------------------------
        // Exits
        // ------------------------------------------------------------------

        private void BuildExits()
        {
            foreach (MapExitSpec exit in Layout.Exits)
            {
                var go = new GameObject("MapExit_" + exit.Id);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(exit.Trigger.center.x, 0f, exit.Trigger.center.y);
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.2f, 0f);
                trigger.size = new Vector3(exit.Trigger.width, 2.4f, exit.Trigger.height);
                MapExitTrigger component = go.AddComponent<MapExitTrigger>();
                component.Configure(exit);
                _exits.Add(component);
            }
        }

        private static void AddRenderer(Transform parent, string objectName, Mesh mesh, Material material, ShadowCastingMode shadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }
}
