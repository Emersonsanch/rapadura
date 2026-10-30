using System.Collections.Generic;
using Rapadura.Gameplay.Building;
using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>Marks a furniture GameObject of the interior view with its placed instance (picking).</summary>
    public sealed class HousePlacedMarker : MonoBehaviour
    {
        public int InstanceId;
    }

    /// <summary>
    /// The house in 3D (Housing.md §3.4): a self-built scene root far from the world map (no scene edit) with the N1
    /// hull (<see cref="HouseHullBuilder"/>), one GameObject per placed item (<see cref="HouseFurnitureMeshes"/>), its own
    /// 3/4 top-down camera with 90° turns and a cutaway that hides the roof and the walls facing the camera while the
    /// interior is in view, the thin grid of "Arrumar a casa" and the green/red placement ghost. Pointer → grid
    /// conversion goes through <see cref="GridPlacementUtility"/>; the rules themselves are the pure
    /// <see cref="HousePlacementRules"/>.
    /// </summary>
    public sealed class HouseInteriorView : MonoBehaviour
    {
        public static readonly Vector3 WorldOrigin = new Vector3(5000f, 0f, 5000f);
        public const float InteriorDistance = 7.2f;
        public const float YardDistance = 12f;
        public const float Pitch = 52f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private HouseService _house;
        private HouseHullMeshes _hull;
        private MeshRenderer _baseRenderer;
        private MeshRenderer _roofRenderer;
        private readonly MeshRenderer[] _wallRenderers = new MeshRenderer[4];
        private readonly Dictionary<int, GameObject> _instances = new Dictionary<int, GameObject>();
        private Transform _furnitureRoot;
        private Camera _camera;
        private Light _lamp;
        private GameObject _grid;
        private Mesh _gridMesh;
        private GameObject _ghost;
        private MeshFilter _ghostFilter;
        private MeshRenderer _ghostRenderer;
        private GameObject _ghostPlate;
        private MeshFilter _ghostPlateFilter;
        private MaterialPropertyBlock _block;
        private int _hiddenInstance;
        private int _selectedInstance;
        private int _yawIndex;
        private bool _yardFocus;
        private float _zoom = 1f;

        public Camera ViewCamera => _camera;
        /// <summary>The house level the hull was built for (an upgrade rebuilds it, <see cref="RebuildHull"/>).</summary>
        public int BuiltLevel { get; private set; }
        public HouseService House => _house;
        public Transform FurnitureRoot => _furnitureRoot;
        public Light WarmLamp => _lamp;

        /// <summary>Raised after <see cref="RebuildFurniture"/> (the life decorations re-attach to the new pieces).</summary>
        public event System.Action FurnitureRebuilt;

        /// <summary>Camera distance factor for bigger houses (1 at N1's 5 × 4 m).</summary>
        public static float SizeScale(HouseLevelSpec spec)
        {
            if (spec == null) return 1f;
            float w = HouseHullBuilder.InteriorWidth(spec), d = HouseHullBuilder.InteriorDepth(spec);
            return Mathf.Max(1f, Mathf.Max(w / 5f, d / 4f) * 0.8f);
        }

        public bool TryGetInstanceObject(int instanceId, out GameObject go) => _instances.TryGetValue(instanceId, out go) && go != null;
        public bool YardFocus => _yardFocus;
        public int YawIndex => _yawIndex;
        private HouseLevelSpec Spec => _house.Spec;

        public static HouseInteriorView Create(HouseService house)
        {
            var root = new GameObject("HouseInterior");
            root.transform.position = WorldOrigin;
            var view = root.AddComponent<HouseInteriorView>();
            view._house = house;
            view.BuildHull();
            view.BuildCamera();
            view.RebuildFurniture();
            root.SetActive(false);
            return view;
        }

        private void OnDestroy()
        {
            DestroyHullMeshes();
            if (_gridMesh != null) Destroy(_gridMesh);
        }

        private void DestroyHullMeshes()
        {
            if (_hull == null) return;
            Destroy(_hull.Base);
            Destroy(_hull.Roof);
            foreach (Mesh wall in _hull.Walls) Destroy(wall);
            _hull = null;
        }

        /// <summary>
        /// Swaps the hull for the house's current level (after "Melhorar a casa"): the kit's modules are rebuilt, the
        /// furniture re-posed on the new grids, lamp and camera re-centred.
        /// </summary>
        public void RebuildHull()
        {
            if (_baseRenderer != null) Destroy(_baseRenderer.gameObject);
            if (_roofRenderer != null) Destroy(_roofRenderer.gameObject);
            for (int i = 0; i < 4; i++)
            {
                if (_wallRenderers[i] != null) Destroy(_wallRenderers[i].gameObject);
            }

            DestroyHullMeshes();
            CreateHullRenderers();
            PlaceLamp();
            ShowGrid(null);
            ApplyCamera();
            RebuildFurniture();
        }

        private void CreateHullRenderers()
        {
            _hull = HouseHullBuilder.Build(Spec);
            BuiltLevel = Spec.Level;
            _baseRenderer = AddMesh("Base", _hull.Base, transform);
            _roofRenderer = AddMesh("Roof", _hull.Roof, transform);
            for (int i = 0; i < 4; i++)
            {
                _wallRenderers[i] = AddMesh("Wall_" + (HouseWallSide)i, _hull.Walls[i], transform);
            }
        }

        private void PlaceLamp()
        {
            if (_lamp == null) return;
            _lamp.transform.localPosition = new Vector3(HouseHullBuilder.InteriorWidth(Spec) * 0.5f, HouseHullBuilder.FloorY + 2.1f, HouseHullBuilder.InteriorDepth(Spec) * 0.5f);
            _lamp.range = 6f * SizeScale(Spec);
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        private void BuildHull()
        {
            CreateHullRenderers();

            _furnitureRoot = new GameObject("Furniture").transform;
            _furnitureRoot.SetParent(transform, false);
            _block = new MaterialPropertyBlock();

            var lampGo = new GameObject("WarmLamp");
            lampGo.transform.SetParent(transform, false);
            lampGo.transform.localPosition = new Vector3(HouseHullBuilder.InteriorWidth(Spec) * 0.5f, HouseHullBuilder.FloorY + 2.1f, HouseHullBuilder.InteriorDepth(Spec) * 0.5f);
            _lamp = lampGo.AddComponent<Light>();
            _lamp.type = LightType.Point;
            _lamp.range = 6f;
            _lamp.intensity = 1.4f;
            _lamp.color = new Color(1f, 0.86f, 0.62f);
            _lamp.shadows = LightShadows.None;
            PlaceLamp();
        }

        private static MeshRenderer AddMesh(string name, Mesh mesh, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = HousePalette.SharedMaterial;
            return renderer;
        }

        private void BuildCamera()
        {
            var cameraGo = new GameObject("HouseCamera");
            cameraGo.transform.SetParent(transform, false);
            _camera = cameraGo.AddComponent<Camera>();
            _camera.fieldOfView = 45f;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 80f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            _camera.depth = 5f;
            ApplyCamera();
        }

        /// <summary>Re-creates every furniture GameObject from the layout (cheap: one mesh per item id is cached).</summary>
        public void RebuildFurniture()
        {
            foreach (GameObject go in _instances.Values)
            {
                if (go != null) Destroy(go);
            }

            _instances.Clear();
            var pending = new List<HousePlacedData>();
            foreach (HousePlacedData placed in _house.Layout.Placed)
            {
                if (placed.layer == HouseLayer.Surface) pending.Add(placed);
                else CreateInstance(placed);
            }

            foreach (HousePlacedData placed in pending) CreateInstance(placed);
            ApplyHidden();
            ApplySelection();
            Physics.SyncTransforms();
            FurnitureRebuilt?.Invoke();
        }

        private void CreateInstance(HousePlacedData placed)
        {
            HouseItemDefinition item = _house.Layout.Resolve(placed.itemId);
            Mesh mesh = HouseFurnitureMeshes.Get(item, placed.layer);
            if (mesh == null) return;

            var go = new GameObject(placed.itemId + "#" + placed.instanceId);
            go.transform.SetParent(_furnitureRoot, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = HousePalette.SharedMaterial;
            go.AddComponent<HousePlacedMarker>().InstanceId = placed.instanceId;
            var box = go.AddComponent<BoxCollider>();
            box.center = mesh.bounds.center;
            box.size = Vector3.Max(mesh.bounds.size, new Vector3(0.12f, 0.12f, 0.12f));
            Pose(placed, item, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
            _instances[placed.instanceId] = go;
        }

        /// <summary>Local pose (under the view root) of a placed or would-be-placed piece.</summary>
        public void Pose(HousePlacedData placed, HouseItemDefinition item, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            scale = Vector3.one;
            rotation = Quaternion.identity;
            float cell = HouseLevelSpec.InteriorCellSize;
            CellRect rect = HousePlacementRules.Footprint(item, placed.layer, placed.x, placed.y, placed.rotation);
            switch (placed.layer)
            {
                case HouseLayer.Wall:
                {
                    HouseHullBuilder.WallFrame(Spec, placed.wall, out _, out _, out Vector3 outward);
                    position = HouseHullBuilder.WallPoint(Spec, placed.wall, (rect.X + rect.Width * 0.5f) * cell, rect.Y * cell + 0.02f, -0.005f);
                    rotation = Quaternion.LookRotation(outward, Vector3.up);
                    if (placed.mirrored) scale = new Vector3(-1f, 1f, 1f);
                    return;
                }
                case HouseLayer.Yard:
                {
                    Vector2 origin = HouseHullBuilder.YardOrigin(Spec);
                    position = new Vector3(origin.x + rect.X + rect.Width * 0.5f, 0f, origin.y + rect.Y + rect.Height * 0.5f);
                    rotation = Quaternion.Euler(0f, HousePlacementRules.QuarterTurns(item, placed.rotation) * 90f, 0f);
                    return;
                }
                case HouseLayer.Surface:
                {
                    HousePlacedData parent = _house.Layout.Find(placed.parentInstanceId);
                    HouseItemDefinition parentItem = parent != null ? _house.Layout.Resolve(parent.itemId) : null;
                    if (parent == null || parentItem == null)
                    {
                        position = Vector3.zero;
                        return;
                    }

                    Pose(parent, parentItem, out Vector3 parentPosition, out Quaternion parentRotation, out Vector3 parentScale);
                    Vector3 local = SurfaceSlotLocal(parentItem, parent.layer, placed.slot);
                    position = parentPosition + parentRotation * Vector3.Scale(parentScale, local);
                    rotation = parentRotation * Quaternion.Euler(0f, placed.rotation * 45f, 0f);
                    return;
                }
                default:
                {
                    float y = placed.layer == HouseLayer.Ceiling ? HouseHullBuilder.FloorY + HouseHullBuilder.WallHeight : HouseHullBuilder.FloorY;
                    position = new Vector3((rect.X + rect.Width * 0.5f) * cell, y, (rect.Y + rect.Height * 0.5f) * cell);
                    rotation = Quaternion.Euler(0f, HousePlacementRules.QuarterTurns(item, placed.rotation) * 90f, 0f);
                    return;
                }
            }
        }

        /// <summary>Where spot <paramref name="slot"/> is on top of a parent piece (parent-local; rows × columns over its top).</summary>
        public static Vector3 SurfaceSlotLocal(HouseItemDefinition parent, HouseLayer parentLayer, int slot)
        {
            Mesh mesh = HouseFurnitureMeshes.Get(parent, parentLayer);
            Bounds bounds = mesh != null ? mesh.bounds : new Bounds(Vector3.up * 0.5f, Vector3.one * 0.5f);
            int slots = Mathf.Max(1, parent.SurfaceSlots);
            int columns = Mathf.CeilToInt(Mathf.Sqrt(slots));
            int rows = Mathf.CeilToInt(slots / (float)columns);
            int column = slot % columns;
            int row = slot / columns;
            float x = Mathf.Lerp(bounds.min.x, bounds.max.x, (column + 0.5f) / columns);
            float z = Mathf.Lerp(bounds.min.z, bounds.max.z, (row + 0.5f) / rows);
            float y = parentLayer == HouseLayer.Wall ? 0.04f : bounds.max.y;
            return new Vector3(x, y, z);
        }

        // ------------------------------------------------------------------
        // Visibility, camera, cutaway
        // ------------------------------------------------------------------

        public void Show(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible) Physics.SyncTransforms();
        }

        public void TurnCamera(int direction)
        {
            _yawIndex = ((_yawIndex + (direction >= 0 ? 1 : -1)) % 4 + 4) % 4;
            ApplyCamera();
        }

        public void SetYardFocus(bool yard)
        {
            if (_yardFocus == yard) return;
            _yardFocus = yard;
            ApplyCamera();
        }

        public void Zoom(float delta)
        {
            _zoom = Mathf.Clamp(_zoom * (1f - delta), 0.6f, 1.6f);
            ApplyCamera();
        }

        private void ApplyCamera()
        {
            if (_camera == null) return;
            Vector3 focus;
            float distance;
            if (_yardFocus)
            {
                Vector2 origin = HouseHullBuilder.YardOrigin(Spec);
                focus = new Vector3(origin.x + Spec.YardWidth * 0.5f, 0f, origin.y + Spec.YardDepth * 0.5f + 1.5f);
                distance = YardDistance * Mathf.Max(1f, Spec.YardWidth / 8f * 0.85f);
            }
            else
            {
                focus = new Vector3(HouseHullBuilder.InteriorWidth(Spec) * 0.5f, HouseHullBuilder.FloorY + 0.4f, HouseHullBuilder.InteriorDepth(Spec) * 0.5f);
                distance = InteriorDistance * SizeScale(Spec);
            }

            Quaternion orbit = Quaternion.Euler(Pitch, _yawIndex * 90f, 0f);
            _camera.transform.localPosition = focus + orbit * new Vector3(0f, 0f, -distance * _zoom);
            _camera.transform.localRotation = orbit;
            ApplyCutaway();
        }

        /// <summary>Which walls hide for a camera turned <paramref name="yawIndex"/> × 90° (pure; 0 = from the south).</summary>
        public static bool IsWallCutAway(HouseWallSide side, int yawIndex)
        {
            // Camera offset direction on the ground: yaw 0 → south (−z), 1 → west, 2 → north, 3 → east.
            switch (((yawIndex % 4) + 4) % 4)
            {
                case 1: return side == HouseWallSide.West;
                case 2: return side == HouseWallSide.North;
                case 3: return side == HouseWallSide.East;
                default: return side == HouseWallSide.South;
            }
        }

        private void ApplyCutaway()
        {
            bool inside = !_yardFocus;
            if (_roofRenderer != null) _roofRenderer.enabled = !inside;
            for (int i = 0; i < 4; i++)
            {
                if (_wallRenderers[i] != null) _wallRenderers[i].enabled = !inside || !IsWallCutAway((HouseWallSide)i, _yawIndex);
            }

            // Pieces on a cut-away wall and ceiling pieces would float in front of the camera: hide them too.
            foreach (KeyValuePair<int, GameObject> pair in _instances)
            {
                HousePlacedData placed = _house.Layout.Find(pair.Key);
                if (placed == null || pair.Value == null) continue;
                bool hide = inside && placed.layer == HouseLayer.Wall && IsWallCutAway(placed.wall, _yawIndex);
                pair.Value.SetActive(!hide);
            }
        }

        /// <summary>Walls that can take a wall piece right now (not cut away).</summary>
        public bool IsWallVisible(HouseWallSide side) => _yardFocus || !IsWallCutAway(side, _yawIndex);

        // ------------------------------------------------------------------
        // Pointer → grid
        // ------------------------------------------------------------------

        private Ray LocalRay(Vector2 screen)
        {
            Ray world = _camera.ScreenPointToRay(screen);
            return new Ray(transform.InverseTransformPoint(world.origin), transform.InverseTransformDirection(world.direction));
        }

        private static bool HitPlaneY(Ray ray, float y, out Vector3 point)
        {
            point = default;
            if (Mathf.Abs(ray.direction.y) < 1e-5f) return false;
            float t = (y - ray.origin.y) / ray.direction.y;
            if (t <= 0f) return false;
            point = ray.origin + ray.direction * t;
            return true;
        }

        /// <summary>Interior (floor/rug/ceiling) cell under the pointer; may be outside the grid.</summary>
        public bool ScreenToInteriorCell(Vector2 screen, out Vector2Int cell)
        {
            cell = default;
            if (!HitPlaneY(LocalRay(screen), HouseHullBuilder.FloorY, out Vector3 p)) return false;
            float size = HouseLevelSpec.InteriorCellSize;
            cell = GridPlacementUtility.WorldToGridCoord(p - new Vector3(size * 0.5f, 0f, size * 0.5f), size);
            return true;
        }

        public bool ScreenToYardCell(Vector2 screen, out Vector2Int cell)
        {
            cell = default;
            if (!HitPlaneY(LocalRay(screen), 0f, out Vector3 p)) return false;
            Vector2 origin = HouseHullBuilder.YardOrigin(Spec);
            float size = HouseLevelSpec.YardCellSize;
            cell = GridPlacementUtility.WorldToGridCoord(new Vector3(p.x - origin.x - size * 0.5f, 0f, p.z - origin.y - size * 0.5f), size);
            return true;
        }

        /// <summary>Nearest visible wall under the pointer: which one, its column and row.</summary>
        public bool ScreenToWallCell(Vector2 screen, out HouseWallSide side, out Vector2Int cell)
        {
            side = HouseWallSide.North;
            cell = default;
            Ray ray = LocalRay(screen);
            float best = float.MaxValue;
            bool found = false;
            float size = HouseLevelSpec.InteriorCellSize;
            foreach (HouseWallSide candidate in new[] { HouseWallSide.South, HouseWallSide.East, HouseWallSide.North, HouseWallSide.West })
            {
                if (!IsWallVisible(candidate)) continue;
                HouseHullBuilder.WallFrame(Spec, candidate, out Vector3 origin, out Vector3 along, out Vector3 outward);
                float denom = Vector3.Dot(ray.direction, outward);
                if (Mathf.Abs(denom) < 1e-5f) continue;
                float t = Vector3.Dot(origin - ray.origin, outward) / denom;
                if (t <= 0f || t >= best) continue;
                Vector3 local = ray.origin + ray.direction * t - origin;
                float u = Vector3.Dot(local, along);
                float v = local.y;
                float length = Spec.WallLength(candidate) * size;
                if (u < 0f || u > length || v < 0f || v > HouseHullBuilder.WallHeight) continue;
                best = t;
                side = candidate;
                cell = new Vector2Int(Mathf.FloorToInt(u / size), Mathf.FloorToInt(v / size));
                found = true;
            }

            return found;
        }

        /// <summary>The placed piece under the pointer (0 = none).</summary>
        public int PickInstance(Vector2 screen)
        {
            Ray world = _camera.ScreenPointToRay(screen);
            RaycastHit[] hits = Physics.RaycastAll(world, 200f);
            float best = float.MaxValue;
            int found = 0;
            foreach (RaycastHit hit in hits)
            {
                HousePlacedMarker marker = hit.collider != null ? hit.collider.GetComponent<HousePlacedMarker>() : null;
                if (marker == null || marker.InstanceId == _hiddenInstance || !hit.collider.gameObject.activeInHierarchy) continue;
                // Small pieces on top of furniture win over the furniture they stand on.
                HousePlacedData placed = _house.Layout.Find(marker.InstanceId);
                float distance = hit.distance - (placed != null && placed.layer == HouseLayer.Surface ? 0.5f : 0f);
                if (distance < best)
                {
                    best = distance;
                    found = marker.InstanceId;
                }
            }

            return found;
        }

        /// <summary>Nearest free spot of <paramref name="parentInstanceId"/> to the pointer, or -1.</summary>
        public int NearestSurfaceSlot(int parentInstanceId, Vector2 screen, int ignoreInstanceId)
        {
            HousePlacedData parent = _house.Layout.Find(parentInstanceId);
            HouseItemDefinition item = parent != null ? _house.Layout.Resolve(parent.itemId) : null;
            if (item == null || item.SurfaceSlots <= 0 || !_instances.TryGetValue(parentInstanceId, out GameObject go)) return -1;

            int bestSlot = -1;
            float best = float.MaxValue;
            for (int slot = 0; slot < item.SurfaceSlots; slot++)
            {
                bool taken = false;
                foreach (HousePlacedData child in _house.Layout.ChildrenOf(parentInstanceId))
                {
                    if (child.slot == slot && child.instanceId != ignoreInstanceId) taken = true;
                }

                if (taken) continue;
                Vector3 world = go.transform.TransformPoint(SurfaceSlotLocal(item, parent.layer, slot));
                Vector3 onScreen = _camera.WorldToScreenPoint(world);
                float distance = (new Vector2(onScreen.x, onScreen.y) - screen).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    bestSlot = slot;
                }
            }

            return bestSlot;
        }

        // ------------------------------------------------------------------
        // Grid, ghost, selection
        // ------------------------------------------------------------------

        /// <summary>Thin grid lines for the layer being arranged (null hides the grid).</summary>
        public void ShowGrid(HouseLayer? layer, HouseWallSide wall = HouseWallSide.North)
        {
            if (_grid == null)
            {
                _grid = new GameObject("ArrangeGrid");
                _grid.transform.SetParent(transform, false);
                _grid.AddComponent<MeshFilter>();
                _grid.AddComponent<MeshRenderer>().sharedMaterial = HousePalette.SharedMaterial;
            }

            if (!layer.HasValue)
            {
                _grid.SetActive(false);
                return;
            }

            var b = new HouseMeshBuilder();
            const float line = 0.012f;
            float cell = HouseLevelSpec.InteriorCellSize;
            switch (layer.Value)
            {
                case HouseLayer.Yard:
                {
                    Vector2 o = HouseHullBuilder.YardOrigin(Spec);
                    for (int x = 0; x <= Spec.YardWidth; x++) b.BoxMinMax(new Vector3(o.x + x - line, 0.005f, o.y), new Vector3(o.x + x + line, 0.015f, o.y + Spec.YardDepth), HouseSwatch.GridLine);
                    for (int y = 0; y <= Spec.YardDepth; y++) b.BoxMinMax(new Vector3(o.x, 0.005f, o.y + y - line), new Vector3(o.x + Spec.YardWidth, 0.015f, o.y + y + line), HouseSwatch.GridLine);
                    break;
                }
                case HouseLayer.Wall:
                {
                    float length = Spec.WallLength(wall) * cell;
                    for (int c = 0; c <= Spec.WallLength(wall); c++)
                        AddWallLine(b, wall, c * cell - line, c * cell + line, 0f, HouseHullBuilder.WallHeight);
                    for (int r = 0; r <= HouseLevelSpec.WallRows; r++)
                        AddWallLine(b, wall, 0f, length, r * cell - line, r * cell + line);
                    break;
                }
                default:
                {
                    float y0 = HouseHullBuilder.FloorY + 0.004f, y1 = HouseHullBuilder.FloorY + 0.012f;
                    float w = HouseHullBuilder.InteriorWidth(Spec), d = HouseHullBuilder.InteriorDepth(Spec);
                    for (int x = 0; x <= Spec.InteriorWidth; x++) b.BoxMinMax(new Vector3(x * cell - line, y0, 0f), new Vector3(x * cell + line, y1, d), HouseSwatch.GridLine);
                    for (int z = 0; z <= Spec.InteriorDepth; z++) b.BoxMinMax(new Vector3(0f, y0, z * cell - line), new Vector3(w, y1, z * cell + line), HouseSwatch.GridLine);
                    break;
                }
            }

            _gridMesh = b.ToMesh("HouseArrangeGrid", _gridMesh);
            _grid.GetComponent<MeshFilter>().sharedMesh = _gridMesh;
            _grid.SetActive(true);
        }

        private void AddWallLine(HouseMeshBuilder b, HouseWallSide wall, float u0, float u1, float v0, float v1)
        {
            Vector3 p0 = HouseHullBuilder.WallPoint(Spec, wall, u0, v0, -0.004f);
            Vector3 p1 = HouseHullBuilder.WallPoint(Spec, wall, u1, v1, -0.012f);
            b.BoxMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), HouseSwatch.GridLine);
        }

        /// <summary>Shows the ghost of <paramref name="item"/> at <paramref name="candidate"/>, green when valid, red when not.</summary>
        public void ShowGhost(HouseItemDefinition item, HousePlacedData candidate, bool valid)
        {
            if (_ghost == null)
            {
                _ghost = new GameObject("PlacementGhost");
                _ghost.transform.SetParent(transform, false);
                _ghostFilter = _ghost.AddComponent<MeshFilter>();
                _ghostRenderer = _ghost.AddComponent<MeshRenderer>();
                _ghostRenderer.sharedMaterial = HousePalette.SharedMaterial;
                _ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                _ghostPlate = new GameObject("PlacementGhostPlate");
                _ghostPlate.transform.SetParent(transform, false);
                _ghostPlateFilter = _ghostPlate.AddComponent<MeshFilter>();
                _ghostPlate.AddComponent<MeshRenderer>().sharedMaterial = HousePalette.SharedMaterial;
            }

            if (item == null || candidate == null)
            {
                HideGhost();
                return;
            }

            _ghostFilter.sharedMesh = HouseFurnitureMeshes.Get(item, candidate.layer);
            Pose(candidate, item, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            _ghost.transform.localPosition = position + Vector3.up * 0.01f;
            _ghost.transform.localRotation = rotation;
            _ghost.transform.localScale = scale;
            _block.SetColor(BaseColorId, valid ? new Color(0.75f, 1f, 0.75f) : new Color(1f, 0.55f, 0.55f));
            _ghostRenderer.SetPropertyBlock(_block);
            _ghost.SetActive(true);

            // Footprint plate (floor, rug, ceiling shadow, yard).
            if (candidate.layer == HouseLayer.Wall || candidate.layer == HouseLayer.Surface)
            {
                _ghostPlate.SetActive(false);
                return;
            }

            CellRect rect = HousePlacementRules.Footprint(item, candidate.layer, candidate.x, candidate.y, candidate.rotation);
            var b = new HouseMeshBuilder();
            HouseSwatch swatch = valid ? HouseSwatch.GhostOk : HouseSwatch.GhostBad;
            if (candidate.layer == HouseLayer.Yard)
            {
                Vector2 o = HouseHullBuilder.YardOrigin(Spec);
                b.BoxMinMax(new Vector3(o.x + rect.X + 0.03f, 0.01f, o.y + rect.Y + 0.03f), new Vector3(o.x + rect.XMax - 0.03f, 0.03f, o.y + rect.YMax - 0.03f), swatch);
            }
            else
            {
                float cell = HouseLevelSpec.InteriorCellSize;
                float y = HouseHullBuilder.FloorY;
                b.BoxMinMax(new Vector3(rect.X * cell + 0.02f, y + 0.005f, rect.Y * cell + 0.02f), new Vector3(rect.XMax * cell - 0.02f, y + 0.02f, rect.YMax * cell - 0.02f), swatch);
            }

            _ghostPlateFilter.sharedMesh = b.ToMesh("HouseGhostPlate", _ghostPlateFilter.sharedMesh);
            _ghostPlate.SetActive(true);
        }

        public void HideGhost()
        {
            if (_ghost != null) _ghost.SetActive(false);
            if (_ghostPlate != null) _ghostPlate.SetActive(false);
        }

        /// <summary>Hides the piece being moved (its ghost stands in for it) — 0 shows everything.</summary>
        public void SetHiddenInstance(int instanceId)
        {
            _hiddenInstance = instanceId;
            ApplyHidden();
        }

        public void SetSelectedInstance(int instanceId)
        {
            _selectedInstance = instanceId;
            ApplySelection();
        }

        private void ApplyHidden()
        {
            foreach (KeyValuePair<int, GameObject> pair in _instances)
            {
                MeshRenderer renderer = pair.Value != null ? pair.Value.GetComponent<MeshRenderer>() : null;
                if (renderer == null) continue;
                bool hiddenSelf = pair.Key == _hiddenInstance;
                HousePlacedData placed = _house.Layout.Find(pair.Key);
                bool hiddenParent = placed != null && placed.layer == HouseLayer.Surface && placed.parentInstanceId == _hiddenInstance && _hiddenInstance > 0;
                renderer.enabled = !hiddenSelf && !hiddenParent;
            }

            ApplyCutaway();
        }

        private void ApplySelection()
        {
            foreach (KeyValuePair<int, GameObject> pair in _instances)
            {
                MeshRenderer renderer = pair.Value != null ? pair.Value.GetComponent<MeshRenderer>() : null;
                if (renderer == null) continue;
                if (pair.Key == _selectedInstance && _selectedInstance > 0)
                {
                    _block.SetColor(BaseColorId, new Color(1f, 0.93f, 0.6f));
                    renderer.SetPropertyBlock(_block);
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }
        }

        /// <summary>Screen position of a placed piece (status bubbles, focus).</summary>
        public bool TryGetScreenPoint(int instanceId, out Vector2 screen)
        {
            screen = default;
            if (!_instances.TryGetValue(instanceId, out GameObject go) || go == null) return false;
            Vector3 p = _camera.WorldToScreenPoint(go.transform.position);
            screen = new Vector2(p.x, p.y);
            return p.z > 0f;
        }
    }
}
