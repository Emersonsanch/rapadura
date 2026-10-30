using System.Collections.Generic;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.NPC.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Turns a <see cref="CityLayout"/> into scene objects, cheaply (mobile, Docs/Cities.md §5.5):
    /// <list type="bullet">
    /// <item>one shared material (<see cref="TownPalette.SharedMaterial"/>) for everything; meshes cached for the session
    /// in <see cref="TownMeshCache"/> (equal buildings share a mesh, re-entering a city rebuilds nothing);</item>
    /// <item>one renderer per building (culled on its own), the props merged in four quadrant chunks plus one backdrop chunk
    /// (no shadows), the ground as one mesh: about 30 renderers for a whole town;</item>
    /// <item>all colliders on one "Colliders" object, straight from <see cref="CitySolids"/> (boxes for walls, stalls and
    /// yard walls, capsules for columns, posts and trunks; porch decks as low steps; one ground slab; four invisible walls),
    /// never a MeshCollider; the root carries <see cref="NonSolidPropZone"/> so the scene-wide solid binder leaves it alone;</item>
    /// <item>the keepers (<see cref="CityResidentPost"/>), the safe-zone volume, the exits, the respawn point and the night
    /// lights (<see cref="CityNightLights"/>).</item>
    /// </list>
    /// </summary>
    public sealed class CityTownView : MonoBehaviour
    {
        public const string RootPrefix = "CityTown_";

        private readonly List<CityResidentPost> _posts = new List<CityResidentPost>();
        private readonly List<CityExitTrigger> _exits = new List<CityExitTrigger>();

        public CityLayout Layout { get; private set; }
        public SpawnPoint Respawn { get; private set; }
        public CitySafeZoneVolume SafeZone { get; private set; }
        public CityNightLights NightLights { get; private set; }
        public IReadOnlyList<CityResidentPost> Posts => _posts;
        public IReadOnlyList<CityExitTrigger> Exits => _exits;
        public int RendererCount { get; private set; }
        public int ColliderCount { get; private set; }
        public int TriangleCount { get; private set; }

        public CityResidentPost PostOf(string lotId) => _posts.Find(p => p.LotId == lotId);

        /// <summary>Builds the whole town under a new root (at the world origin, y = 0).</summary>
        public static CityTownView Build(CityLayout layout, Transform parent = null, string cacheKey = null)
        {
            var root = new GameObject(RootPrefix + layout.CityId);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            NonSolidPropZone.Ensure(root);
            CityTownView view = root.AddComponent<CityTownView>();
            view.Layout = layout;
            view.BuildAll(cacheKey ?? layout.CityId);
            return view;
        }

        private void BuildAll(string cacheKey)
        {
            Material material = TownPalette.SharedMaterial;
            TownStyle style = Layout.Style;

            // Ground.
            Mesh ground = TownMeshCache.Get("ground:" + cacheKey, b => TownGroundBuilder.Build(b, Layout));
            AddRenderer("Ground", ground, material, Vector3.zero, 0f, ShadowCastingMode.Off);

            // Buildings.
            foreach (CityLot lot in Layout.Lots)
            {
                if (lot.Building == null || lot.Kind == CityLotKind.Market)
                {
                    continue;
                }

                TownBuildingSpec spec = lot.Building;
                Mesh mesh = TownMeshCache.Get("bld:" + Layout.Region + ":" + spec.CacheKey, b => TownBuildingBuilder.Build(b, spec));
                AddRenderer("Lot_" + lot.Id, mesh, material, new Vector3(lot.Center.x, 0f, lot.Center.y), lot.Yaw, ShadowCastingMode.On);
            }

            // Props, merged by quadrant (+ the backdrop beyond the walk bounds).
            for (int chunk = 0; chunk < 5; chunk++)
            {
                int captured = chunk;
                Mesh mesh = TownMeshCache.Get("props:" + cacheKey + ":" + chunk, b => BuildPropChunk(b, captured, style));
                if (mesh.vertexCount > 0)
                {
                    AddRenderer(chunk == 4 ? "Backdrop" : "Props_" + chunk, mesh, material, Vector3.zero, 0f, chunk == 4 ? ShadowCastingMode.Off : ShadowCastingMode.On);
                }
            }

            BuildKeepers(material, style);
            BuildColliders();
            BuildZones();
            BuildLights();
        }

        private int ChunkOf(Vector2 p)
        {
            if (!Layout.WalkBounds.Contains(p))
            {
                return 4;
            }

            return (p.x >= 0f ? 1 : 0) + (p.y >= 0f ? 2 : 0);
        }

        private void BuildPropChunk(TownMeshBuilder b, int chunk, TownStyle style)
        {
            foreach (CityProp prop in Layout.Props)
            {
                Vector2 anchor = prop.Kind == CityPropKind.FenceRun || prop.Kind == CityPropKind.Gate ? (prop.Position + prop.End) * 0.5f : prop.Position;
                if (ChunkOf(anchor) != chunk)
                {
                    continue;
                }

                var p = new Vector3(prop.Position.x, 0f, prop.Position.y);
                switch (prop.Kind)
                {
                    case CityPropKind.FenceRun:
                        TownKit.WallRun(b, p, new Vector3(prop.End.x, 0f, prop.End.y), style);
                        break;
                    case CityPropKind.Gate:
                        TownKit.Gate(b, p, new Vector3(prop.End.x, 0f, prop.End.y), style);
                        break;
                    default:
                        b.With(p, prop.Yaw, () => TownKit.Prop(b, prop, style));
                        break;
                }
            }
        }

        private void AddRenderer(string objectName, Mesh mesh, Material material, Vector3 position, float yaw, ShadowCastingMode shadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            RendererCount++;
            TriangleCount += mesh != null ? (int)(mesh.GetIndexCount(0) / 3) : 0;
        }

        // ------------------------------------------------------------------
        // Keepers
        // ------------------------------------------------------------------

        private void BuildKeepers(Material material, TownStyle style)
        {
            List<Rect> decks = CitySolids.Decks(Layout);
            foreach (CityLot lot in Layout.Lots)
            {
                if (!lot.HasKeeper)
                {
                    continue;
                }

                // The keeper's own look (Docs/NPCs.md): the named resident's recipe, or a villager seeded by the lot in
                // the region's palette; one skinned renderer on the shared NPC material, idle + head turn included.
                string figureKey = lot.ResidentId ?? lot.Id;
                float y = decks.Exists(d => d.Contains(lot.StandPoint)) ? CitySolids.DeckHeight : 0f;
                Vector2 look = lot.DoorApproach - lot.StandPoint;
                float yaw = look.sqrMagnitude > 0.01f ? Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg : lot.Yaw + 180f;

                var go = new GameObject("Npc_" + lot.Id);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(lot.StandPoint.x, y, lot.StandPoint.y);
                go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                NpcFigure figure = NpcFigure.Create(go.transform, figureKey, Layout.Region, Vector3.zero);
                RendererCount++;
                TriangleCount += figure.Model != null ? figure.Model.Triangles : 0;

                var trigger = go.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = lot.InteractRadius;
                trigger.center = new Vector3(0f, 0.9f, 0f);
                ColliderCount++;

                CityResidentPost post = go.AddComponent<CityResidentPost>();
                post.Configure(Layout.CityId, lot.Id, lot.ResidentId, lot.ShopId);
                _posts.Add(post);
            }
        }

        // ------------------------------------------------------------------
        // Colliders, zones, exits, respawn, lights
        // ------------------------------------------------------------------

        private void BuildColliders()
        {
            var go = new GameObject("Colliders");
            go.transform.SetParent(transform, false);
            go.isStatic = true;

            float e = Layout.GroundHalfExtent;
            AddBox(go, new Vector3(0f, -0.5f, 0f), new Vector3(e * 2f, 1f, e * 2f));

            foreach (CitySolid solid in Layout.Solids)
            {
                Vector2 c = solid.Center;
                if (solid.IsCapsule)
                {
                    var capsule = go.AddComponent<CapsuleCollider>();
                    capsule.direction = 1;
                    capsule.radius = solid.Radius;
                    capsule.height = Mathf.Max(solid.Height, solid.Radius * 2f);
                    capsule.center = new Vector3(c.x, solid.Height * 0.5f, c.y);
                    ColliderCount++;
                }
                else
                {
                    AddBox(go, new Vector3(c.x, solid.Height * 0.5f, c.y), new Vector3(solid.Footprint.width, solid.Height, solid.Footprint.height));
                }
            }

            foreach (Rect deck in CitySolids.Decks(Layout))
            {
                AddBox(go, new Vector3(deck.center.x, CitySolids.DeckHeight * 0.5f, deck.center.y), new Vector3(deck.width, CitySolids.DeckHeight, deck.height));
            }

            // Invisible walls at the walk bounds.
            Rect w = Layout.WalkBounds;
            const float h = 4f;
            const float t = 1f;
            AddBox(go, new Vector3(w.center.x, h * 0.5f, w.yMax + t * 0.5f), new Vector3(w.width + t * 2f, h, t));
            AddBox(go, new Vector3(w.center.x, h * 0.5f, w.yMin - t * 0.5f), new Vector3(w.width + t * 2f, h, t));
            AddBox(go, new Vector3(w.xMax + t * 0.5f, h * 0.5f, w.center.y), new Vector3(t, h, w.height));
            AddBox(go, new Vector3(w.xMin - t * 0.5f, h * 0.5f, w.center.y), new Vector3(t, h, w.height));
        }

        private void AddBox(GameObject go, Vector3 center, Vector3 size)
        {
            var box = go.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
            ColliderCount++;
        }

        private void BuildZones()
        {
            var zone = new GameObject("SafeZone_" + Layout.CityId);
            zone.transform.SetParent(transform, false);
            Rect r = Layout.SafeZone;
            zone.transform.localPosition = new Vector3(r.center.x, 0f, r.center.y);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 5f, 0f);
            box.size = new Vector3(r.width, 12f, r.height);
            SafeZone = zone.AddComponent<CitySafeZoneVolume>();
            SafeZone.Configure(Layout.CityId);
            ColliderCount++;

            foreach (CityExit exit in Layout.Exits)
            {
                var go = new GameObject("Exit_" + exit.Id);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(exit.Trigger.center.x, 0f, exit.Trigger.center.y);
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.2f, 0f);
                trigger.size = new Vector3(exit.Trigger.width, 2.4f, exit.Trigger.height);
                CityExitTrigger component = go.AddComponent<CityExitTrigger>();
                component.Configure(exit.Id, exit.Kind);
                _exits.Add(component);
                ColliderCount++;
            }

            var spawn = new GameObject("Respawn_Plaza_" + Layout.CityId);
            spawn.transform.SetParent(transform, false);
            spawn.transform.localPosition = new Vector3(Layout.Respawn.x, 0f, Layout.Respawn.y);
            spawn.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Respawn = spawn.AddComponent<SpawnPoint>();
        }

        private void BuildLights()
        {
            NightLights = gameObject.AddComponent<CityNightLights>();
            var candidates = new List<Vector3>();
            foreach (CityProp prop in Layout.Props)
            {
                if (prop.Kind == CityPropKind.LampPost && Vector2.Distance(prop.Position, Layout.PlazaCenter) < Layout.PlazaRadius + 1.5f)
                {
                    candidates.Add(new Vector3(prop.Position.x, 2.4f, prop.Position.y));
                }
            }

            CityLot lamp = Layout.Lots.Find(l => l.Kind == CityLotKind.LampHouse);
            if (lamp != null)
            {
                candidates.Add(new Vector3(lamp.DoorApproach.x, 2.6f, lamp.DoorApproach.y));
            }

            CityLot dock = Layout.Lots.Find(l => l.Kind == CityLotKind.ShipDock);
            if (dock != null)
            {
                candidates.Add(new Vector3(dock.Center.x, 1.5f, dock.Center.y));
            }

            for (int i = 0; i < candidates.Count && i < 6; i++)
            {
                NightLights.AddLight(candidates[i]);
            }

            NightLights.Refresh();
        }
    }
}
