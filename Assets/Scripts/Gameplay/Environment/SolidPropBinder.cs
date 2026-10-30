using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Gives scene and runtime-spawned props the solid collider <see cref="SolidPropRules"/> decides
    /// (trunk capsule for trees, convex hull/box for rocks, box for benches/stalls/houses, capsule for
    /// cacti) so the hero's CharacterController can't walk through them. One system for everything:
    /// <list type="bullet">
    /// <item><see cref="BindAll"/> — a post-load pass over the loaded scenes (run by
    /// <see cref="EnvironmentPresentationBootstrap"/> on every scene load, and by the editor menu
    /// "Rapadura/Environment/Apply Solid Colliders To Open Scene" to bake it into the scene);</item>
    /// <item><see cref="Bind"/> — called by runtime spawners right after placing a prop
    /// (BiomeArenaDressing; the tree prefab build in NaturalTreeSetup; NaturalRockSetup).</item>
    /// </list>
    /// The collider lives on a child named <see cref="ColliderName"/> (static, no Rigidbody, the
    /// prop's own layer — Default), so re-running never duplicates it and the prop's own components
    /// (triggers like the Workbench's crafting zone, wind sway) are untouched. Authored solid colliders
    /// are trusted, except a rock collider clearly smaller than its visual (the stale "Sphere" mesh
    /// collider), which is replaced. Hierarchies under a <see cref="NonSolidPropZone"/> (world map)
    /// are skipped. After binding, close tree pairs are checked against the hero's diameter: a gap that
    /// is just too tight is opened by shrinking both trunks a little, a clearly too-narrow one is sealed
    /// with a thin box (never across a road corridor). See Docs/Collision.md.
    /// </summary>
    public static class SolidPropBinder
    {
        public const string ColliderName = "SolidCollider";
        public const string SealPrefix = "SolidGapSeal";

        /// <summary>Vertex cap per prop when sampling mesh points (stride sampling above it).</summary>
        private const int MaxSampledVertices = 20000;

        /// <summary>Set false to stop the automatic load-time pass (tests, tools).</summary>
        public static bool AutoBind = true;

        /// <summary>A keep-clear corridor (road/path) on the XZ plane: segment a-b, half width.</summary>
        public struct Corridor
        {
            public Vector3 A;
            public Vector3 B;
            public float HalfWidth;

            public Corridor(Vector3 a, Vector3 b, float halfWidth)
            {
                A = a;
                B = b;
                HalfWidth = halfWidth;
            }
        }

        /// <summary>Extra corridors registered by code (roads with no named renderer). Objects named Road*/Estrada*/Trilha*/Path_* are picked up automatically.</summary>
        public static readonly List<Corridor> KeepClear = new List<Corridor>();

        /// <summary>What a pass did (logged by the editor menu, asserted by tests).</summary>
        public struct Report
        {
            public int Solid;
            public int Added;
            public int Replaced;
            public int Trusted;
            public int Shrunk;
            public int Seals;

            public override string ToString()
            {
                return $"{Solid} solid prop(s): {Added} collider(s) added, {Replaced} stale rock collider(s) replaced, {Trusted} authored kept, {Shrunk} trunk(s) shrunk for a gap, {Seals} gap seal(s)";
            }
        }

        private static readonly string[] CorridorPrefixes = { "Road", "Estrada", "Trilha", "Path_", "Caminho" };

        /// <summary>Load-time hook (EnvironmentPresentationBootstrap): <see cref="BindAll"/> unless <see cref="AutoBind"/> is off.</summary>
        public static void BindAllIfEnabled()
        {
            if (AutoBind)
            {
                BindAll();
            }
        }

        /// <summary>Binds every active prop in the loaded scenes, then resolves tight tree gaps.</summary>
        public static Report BindAll(bool refit = false)
        {
            var total = default(Report);
            var roots = new List<GameObject>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded)
                {
                    continue;
                }

                scene.GetRootGameObjects(roots);
                var trunks = new List<CapsuleCollider>();
                List<Corridor> corridors = CollectCorridors(roots);
                foreach (GameObject root in roots)
                {
                    BindUnder(root.transform, refit, ref total, trunks);
                }

                ResolveTreeGaps(trunks, corridors, refit, ref total);
            }

            return total;
        }

        /// <summary>Binds every prop under <paramref name="root"/> (inclusive) — prefab contents, a freshly spawned group.</summary>
        public static Report BindHierarchy(Transform root, bool refit = false)
        {
            var report = default(Report);
            if (root == null)
            {
                return report;
            }

            var trunks = new List<CapsuleCollider>();
            BindUnder(root, refit, ref report, trunks);
            ResolveTreeGaps(trunks, CollectCorridors(null), refit, ref report);
            return report;
        }

        /// <summary>
        /// Gives one prop its solid collider (or keeps the one it has). Returns true when the prop
        /// ends up solid. <paramref name="refit"/> recomputes an existing <see cref="ColliderName"/>
        /// child (editor menu); at runtime an existing one is kept as baked.
        /// </summary>
        public static bool Bind(GameObject prop, bool refit = false)
        {
            var report = default(Report);
            return Bind(prop, refit, ref report, null);
        }

        // ------------------------------------------------------------------
        // Scan
        // ------------------------------------------------------------------

        private static void BindUnder(Transform node, bool refit, ref Report report, List<CapsuleCollider> trunks)
        {
            // Never descend into zones, characters (CharacterController/Rigidbody/Animator: a bone or
            // held prop named "Chest"/"Barrel" is not scenery) or UI.
            if (!node.gameObject.activeInHierarchy || node.TryGetComponent(out NonSolidPropZone _) || IsCharacter(node)
                || node.TryGetComponent(out Animator _) || node.TryGetComponent(out Canvas _) || IsCharacterName(node.name))
            {
                return;
            }

            if (IsOwnCollider(node))
            {
                return;
            }

            GameObject go = node.gameObject;
            SolidPropKind kind = KindOf(go);
            if (kind != SolidPropKind.None)
            {
                Bind(go, refit, ref report, trunks);
                return; // a prop's children are its parts
            }

            for (int i = 0; i < node.childCount; i++)
            {
                BindUnder(node.GetChild(i), refit, ref report, trunks);
            }
        }

        private static bool Bind(GameObject prop, bool refit, ref Report report, List<CapsuleCollider> trunks)
        {
            if (prop == null || prop.GetComponentInParent<NonSolidPropZone>(true) != null || IsCharacter(prop.transform))
            {
                return false;
            }

            SolidPropKind kind = KindOf(prop);
            if (kind == SolidPropKind.None)
            {
                return false;
            }

            var filters = new List<MeshFilter>();
            CollectVisualFilters(prop, filters);
            if (filters.Count == 0)
            {
                return false;
            }

            // Frame the collider child sits in (identity): the single mesh's own transform when there
            // is exactly one (exact fit, incl. non-uniform scale), else the prop root.
            Transform frame = kind != SolidPropKind.Tree && filters.Count == 1 ? filters[0].transform : prop.transform;
            Transform existing = FindOwnCollider(prop.transform);
            RemoveDuplicateOwnColliders(prop.transform, existing);

            if (existing != null && !refit)
            {
                CollectTrunk(existing, kind, trunks);
                report.Solid++;
                return true;
            }

            if (existing == null)
            {
                int authored = CountAuthoredSolids(prop, kind, ref report);
                if (authored > 0)
                {
                    report.Solid++;
                    report.Trusted++;
                    return true;
                }
            }

            var points = new List<Vector3>();
            bool readable = SamplePoints(filters, frame, points, out int vertexCount);
            if (points.Count == 0)
            {
                return false;
            }

            Bounds local = BoundsOf(points);
            Vector3 scale = Abs(frame.lossyScale);
            float worldHeight = local.size.y * scale.y;
            float worldFootprint = Mathf.Max(local.size.x * scale.x, local.size.z * scale.z);
            if (!SolidPropRules.Blocks(kind, worldHeight, worldFootprint))
            {
                return false;
            }

            SolidColliderShape shape = SolidPropRules.ShapeFor(kind, readable && filters.Count == 1, vertexCount);
            if (shape == SolidColliderShape.None)
            {
                return false;
            }

            Transform child = existing != null && existing.parent == frame ? existing : null;
            if (existing != null && child == null)
            {
                SafeDestroy(existing.gameObject);
            }

            if (child == null)
            {
                var go = new GameObject(ColliderName);
                child = go.transform;
                child.SetParent(frame, false);
                report.Added++;
            }

            child.gameObject.layer = frame.gameObject.layer;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            switch (shape)
            {
                case SolidColliderShape.TrunkCapsule:
                {
                    float horizontal = Mathf.Max(scale.x, scale.z, 1e-4f);
                    if (!readable || !SolidPropRules.TryEstimateTrunk(points, 1f / horizontal, out TrunkEstimate trunk))
                    {
                        trunk = SolidPropRules.TrunkFromBounds(local, 1f / horizontal);
                    }

                    CapsuleCollider capsule = Only<CapsuleCollider>(child.gameObject);
                    capsule.direction = 1;
                    capsule.center = trunk.Center;
                    capsule.radius = trunk.Radius;
                    capsule.height = trunk.Height;
                    trunks?.Add(capsule);
                    break;
                }

                case SolidColliderShape.ConvexMesh:
                {
                    MeshCollider hull = Only<MeshCollider>(child.gameObject);
                    hull.sharedMesh = HullSourceFor(filters[0].sharedMesh);
                    hull.convex = true;
                    break;
                }

                case SolidColliderShape.Capsule:
                {
                    SolidPropRules.CapsuleFromBounds(local, out Vector3 center, out float radius, out float height);
                    CapsuleCollider capsule = Only<CapsuleCollider>(child.gameObject);
                    capsule.direction = 1;
                    capsule.center = center;
                    capsule.radius = radius;
                    capsule.height = height;
                    break;
                }

                default:
                {
                    BoxCollider box = Only<BoxCollider>(child.gameObject);
                    box.center = local.center;
                    box.size = local.size;
                    break;
                }
            }

            report.Solid++;
            return true;
        }

        private static SolidPropKind KindOf(GameObject go)
        {
            string meshName = go.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null ? filter.sharedMesh.name : null;
            return SolidPropRules.Classify(go.name, meshName, go.TryGetComponent(out LODGroup _));
        }

        private static bool IsCharacter(Transform node)
        {
            return node.GetComponentInParent<CharacterController>(true) != null || node.GetComponentInParent<Rigidbody>(true) != null;
        }

        private static bool IsCharacterName(string name)
        {
            return name.StartsWith("Npc_", System.StringComparison.Ordinal) || name.StartsWith("Enemy_", System.StringComparison.Ordinal)
                || name.StartsWith("Player", System.StringComparison.Ordinal);
        }

        private static bool IsOwnCollider(Transform node)
        {
            return node.name == ColliderName || node.name.StartsWith(SealPrefix, System.StringComparison.Ordinal);
        }

        private static Transform FindOwnCollider(Transform prop)
        {
            if (prop.name == ColliderName && prop.gameObject.activeSelf)
            {
                return prop;
            }

            for (int i = 0; i < prop.childCount; i++)
            {
                Transform found = FindOwnCollider(prop.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Keeps one <see cref="ColliderName"/> child per prop: a scene-added one and the prefab's own
        /// (baked later) can coexist after a prefab update. Extras that can't be destroyed (owned by
        /// a prefab asset in an Editor instance) are disabled instead.
        /// </summary>
        private static void RemoveDuplicateOwnColliders(Transform prop, Transform keep)
        {
            if (keep == null)
            {
                return;
            }

            foreach (Transform t in prop.GetComponentsInChildren<Transform>(true))
            {
                if (t == keep || t.name != ColliderName)
                {
                    continue;
                }

                try
                {
                    SafeDestroy(t.gameObject);
                }
                catch (System.InvalidOperationException)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>The prop's visual meshes: the coarsest LOD of a LODGroup (fewest vertices, same silhouette), else every mesh below it.</summary>
        private static void CollectVisualFilters(GameObject prop, List<MeshFilter> filters)
        {
            if (prop.TryGetComponent(out LODGroup lodGroup))
            {
                LOD[] lods = lodGroup.GetLODs();
                for (int l = lods.Length - 1; l >= 0 && filters.Count == 0; l--)
                {
                    foreach (Renderer renderer in lods[l].renderers)
                    {
                        if (renderer != null && renderer.TryGetComponent(out MeshFilter f) && f.sharedMesh != null)
                        {
                            filters.Add(f);
                        }
                    }
                }

                if (filters.Count > 0)
                {
                    return;
                }
            }

            foreach (MeshFilter f in prop.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh != null && !IsOwnCollider(f.transform))
                {
                    filters.Add(f);
                }
            }
        }

        /// <summary>
        /// Counts the authored (non-trigger, enabled, not ours) colliders of a prop. For rocks, one
        /// clearly smaller than the renderer on the same object is stale and removed first.
        /// </summary>
        private static int CountAuthoredSolids(GameObject prop, SolidPropKind kind, ref Report report)
        {
            int count = 0;
            foreach (Collider collider in prop.GetComponentsInChildren<Collider>(true))
            {
                if (collider == null || collider.isTrigger || !collider.enabled || IsOwnCollider(collider.transform))
                {
                    continue;
                }

                if (kind == SolidPropKind.Rock && IsStaleRockCollider(collider))
                {
                    collider.enabled = false;
                    SafeDestroy(collider);
                    report.Replaced++;
                    continue;
                }

                count++;
            }

            return count;
        }

        private static bool IsStaleRockCollider(Collider collider)
        {
            if (!collider.TryGetComponent(out MeshFilter filter) || filter.sharedMesh == null)
            {
                return false;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh == filter.sharedMesh)
            {
                return false;
            }

            Vector3 visual = Vector3.Scale(filter.sharedMesh.bounds.size, Abs(collider.transform.lossyScale));
            Vector3 solid = LocalColliderSize(collider);
            solid = Vector3.Scale(solid, Abs(collider.transform.lossyScale));
            return SolidPropRules.IsUndersized(new Vector2(solid.x, solid.z), new Vector2(visual.x, visual.z));
        }

        /// <summary>Collider extent in its own transform's local space (no physics sync needed).</summary>
        private static Vector3 LocalColliderSize(Collider collider)
        {
            switch (collider)
            {
                case BoxCollider box:
                    return box.size;
                case SphereCollider sphere:
                    return Vector3.one * (sphere.radius * 2f);
                case CapsuleCollider capsule:
                    return Vector3.one * (capsule.radius * 2f);
                case MeshCollider mesh when mesh.sharedMesh != null:
                    return mesh.sharedMesh.bounds.size;
                default:
                    return Vector3.one * 1e6f; // unknown: trust it
            }
        }

        private static readonly List<Vector3> Sampled = new List<Vector3>();

        /// <summary>Mesh points in <paramref name="frame"/>'s local space; false when any mesh wasn't readable (bounds corners used for it).</summary>
        private static bool SamplePoints(List<MeshFilter> filters, Transform frame, List<Vector3> points, out int vertexCount)
        {
            bool readable = true;
            vertexCount = 0;
            Matrix4x4 toFrame = frame.worldToLocalMatrix;
            foreach (MeshFilter filter in filters)
            {
                Mesh mesh = filter.sharedMesh;
                Matrix4x4 matrix = toFrame * filter.transform.localToWorldMatrix;
                vertexCount += mesh.vertexCount;
                // The Editor keeps every mesh's CPU copy, so non-readable assets (the tree LODs) are
                // still measured when baking; a Player build only reads Read/Write meshes.
                Sampled.Clear();
                if (mesh.isReadable || Application.isEditor)
                {
                    mesh.GetVertices(Sampled);
                }

                if (Sampled.Count > 0)
                {
                    int stride = Mathf.Max(1, Sampled.Count / MaxSampledVertices);
                    for (int i = 0; i < Sampled.Count; i += stride)
                    {
                        points.Add(matrix.MultiplyPoint3x4(Sampled[i]));
                    }
                }
                else
                {
                    readable = false;
                    Bounds b = mesh.bounds;
                    for (int c = 0; c < 8; c++)
                    {
                        var corner = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                        points.Add(matrix.MultiplyPoint3x4(corner));
                    }
                }
            }

            return readable;
        }

        private static void CollectTrunk(Transform own, SolidPropKind kind, List<CapsuleCollider> trunks)
        {
            if (kind == SolidPropKind.Tree && trunks != null && own.TryGetComponent(out CapsuleCollider capsule))
            {
                trunks.Add(capsule);
            }
        }

        // ------------------------------------------------------------------
        // Gaps & roads
        // ------------------------------------------------------------------

        private struct TrunkInfo
        {
            public CapsuleCollider Collider;
            public Vector2 Center;
            public float Radius;
            public float Floor;
            public float Scale;
            public float BaseY;
            public float Height;
        }

        private static void ResolveTreeGaps(List<CapsuleCollider> trunks, List<Corridor> corridors, bool refit, ref Report report)
        {
            if (trunks.Count == 0)
            {
                return;
            }

            var infos = new List<TrunkInfo>(trunks.Count);
            foreach (CapsuleCollider capsule in trunks)
            {
                Transform t = capsule.transform;
                Vector3 s = Abs(t.lossyScale);
                float scale = Mathf.Max(s.x, s.z, 1e-4f);
                Vector3 world = t.TransformPoint(capsule.center);
                float radius = capsule.radius * scale;
                var info = new TrunkInfo
                {
                    Collider = capsule,
                    Center = new Vector2(world.x, world.z),
                    Radius = radius,
                    Floor = radius - SolidPropRules.MaxTrunkShrink,
                    Scale = scale,
                    Height = capsule.height * s.y,
                };
                info.BaseY = world.y - info.Height * 0.5f;

                // Roads stay clear: a trunk beside a corridor never pokes into it.
                foreach (Corridor corridor in corridors)
                {
                    float clearance = SolidPropRules.CorridorClearance(info.Center, Flat(corridor.A), Flat(corridor.B), corridor.HalfWidth);
                    info.Radius = SolidPropRules.RadiusAllowedByCorridor(info.Radius, clearance, SolidPropRules.MinTrunkRadius);
                }

                infos.Add(info);
            }

            // Refit: seals no pair needs any more (trees moved apart) are removed afterwards.
            var staleSeals = new HashSet<Transform>();
            if (refit)
            {
                foreach (TrunkInfo info in infos)
                {
                    Transform tree = info.Collider.transform.parent;
                    Transform holder = tree != null ? tree.parent : null;
                    if (holder == null)
                    {
                        continue;
                    }

                    for (int c = 0; c < holder.childCount; c++)
                    {
                        Transform child = holder.GetChild(c);
                        if (child.name.StartsWith(SealPrefix, System.StringComparison.Ordinal))
                        {
                            staleSeals.Add(child);
                        }
                    }
                }
            }

            float reach = SolidPropRules.MaxTrunkRadius * 2f + SolidPropRules.PassableGap();
            var shrunk = new HashSet<int>();
            for (int i = 0; i < infos.Count; i++)
            {
                for (int j = i + 1; j < infos.Count; j++)
                {
                    TrunkInfo a = infos[i];
                    TrunkInfo b = infos[j];
                    float distance = (a.Center - b.Center).magnitude;
                    if (distance > reach)
                    {
                        continue;
                    }

                    float ra = a.Radius;
                    float rb = b.Radius;
                    SolidGapResolution resolution = SolidPropRules.ResolveGap(distance, ref ra, ref rb, a.Floor, b.Floor, SolidPropRules.HeroRadius);
                    if (resolution == SolidGapResolution.Shrunk)
                    {
                        a.Radius = ra;
                        b.Radius = rb;
                        infos[i] = a;
                        infos[j] = b;
                        shrunk.Add(i);
                        shrunk.Add(j);
                    }
                    else if (resolution == SolidGapResolution.Seal && !CrossesCorridor(a.Center, b.Center, corridors))
                    {
                        if (Seal(a, b, refit, out Transform seal))
                        {
                            report.Seals++;
                        }

                        staleSeals.Remove(seal);
                    }
                }
            }

            foreach (Transform stale in staleSeals)
            {
                if (stale != null)
                {
                    SafeDestroy(stale.gameObject);
                }
            }

            for (int i = 0; i < infos.Count; i++)
            {
                TrunkInfo info = infos[i];
                float local = info.Radius / info.Scale;
                if (!Mathf.Approximately(local, info.Collider.radius))
                {
                    info.Collider.radius = local;
                    info.Collider.height = Mathf.Max(info.Collider.height, local * 2f);
                }
            }

            report.Shrunk += shrunk.Count;
        }

        private static bool CrossesCorridor(Vector2 a, Vector2 b, List<Corridor> corridors)
        {
            foreach (Corridor corridor in corridors)
            {
                if (SolidPropRules.SegmentTouchesCorridor(a, b, Flat(corridor.A), Flat(corridor.B), corridor.HalfWidth))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>A thin static box between two trunks too close for the hero. Named after both trees so re-runs reuse it.</summary>
        private static bool Seal(TrunkInfo a, TrunkInfo b, bool refit, out Transform seal)
        {
            Transform treeA = a.Collider.transform.parent != null ? a.Collider.transform.parent : a.Collider.transform;
            Transform treeB = b.Collider.transform.parent != null ? b.Collider.transform.parent : b.Collider.transform;
            Transform holder = treeA.parent;
            string sealName = $"{SealPrefix}[{treeA.name}|{treeB.name}]";

            seal = FindSeal(holder, treeA.gameObject.scene, sealName);
            if (seal != null && !refit)
            {
                return false;
            }

            if (seal == null)
            {
                seal = new GameObject(sealName).transform;
                seal.SetParent(holder, false);
                if (holder == null && treeA.gameObject.scene.IsValid() && seal.gameObject.scene != treeA.gameObject.scene)
                {
                    SceneManager.MoveGameObjectToScene(seal.gameObject, treeA.gameObject.scene);
                }
            }

            seal.gameObject.layer = treeA.gameObject.layer;
            Vector2 mid = (a.Center + b.Center) * 0.5f;
            Vector2 along = b.Center - a.Center;
            float baseY = Mathf.Min(a.BaseY, b.BaseY);
            float height = Mathf.Min(a.Height, b.Height);
            seal.SetPositionAndRotation(new Vector3(mid.x, baseY + height * 0.5f, mid.y),
                Quaternion.LookRotation(new Vector3(along.x, 0f, along.y), Vector3.up));
            Vector3 parentScale = holder != null ? Abs(holder.lossyScale) : Vector3.one;
            seal.localScale = new Vector3(1f / Mathf.Max(parentScale.x, 1e-4f), 1f / Mathf.Max(parentScale.y, 1e-4f), 1f / Mathf.Max(parentScale.z, 1e-4f));

            BoxCollider box = Only<BoxCollider>(seal.gameObject);
            float thickness = Mathf.Max(0.2f, Mathf.Min(a.Radius, b.Radius));
            box.center = Vector3.zero;
            box.size = new Vector3(thickness, height, along.magnitude);
            return true;
        }

        private static Transform FindSeal(Transform holder, Scene scene, string sealName)
        {
            if (holder != null)
            {
                return holder.Find(sealName);
            }

            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == sealName)
                {
                    return root.transform;
                }
            }

            return null;
        }

        private static List<Corridor> CollectCorridors(List<GameObject> roots)
        {
            var corridors = new List<Corridor>(KeepClear);
            if (roots == null)
            {
                return corridors;
            }

            foreach (GameObject root in roots)
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(false))
                {
                    string name = renderer.gameObject.name;
                    bool road = false;
                    foreach (string prefix in CorridorPrefixes)
                    {
                        road |= name.StartsWith(prefix, System.StringComparison.Ordinal);
                    }

                    if (!road)
                    {
                        continue;
                    }

                    Bounds b = renderer.bounds;
                    bool alongX = b.size.x >= b.size.z;
                    Vector3 half = alongX ? new Vector3(b.extents.x, 0f, 0f) : new Vector3(0f, 0f, b.extents.z);
                    corridors.Add(new Corridor(b.center - half, b.center + half, alongX ? b.extents.z : b.extents.x));
                }
            }

            return corridors;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>Vertex budget under which a render mesh is cooked as-is into a convex hull.</summary>
        private const int HullDirectVertexLimit = 64;

        private static readonly Dictionary<Mesh, Mesh> HullSources = new Dictionary<Mesh, Mesh>();

        /// <summary>
        /// The mesh a convex <see cref="MeshCollider"/> is cooked from. Dense procedural rocks make PhysX exceed its
        /// 256-polygon hull limit and log an error (which stops Play mode when the Console's Error Pause is on), so
        /// above <see cref="HullDirectVertexLimit"/> vertices the hull is cooked from the mesh's extreme points in
        /// 42 directions instead (at most 80 hull faces). Cached per source mesh.
        /// </summary>
        private static Mesh HullSourceFor(Mesh mesh)
        {
            if (mesh == null || !mesh.isReadable || mesh.vertexCount <= HullDirectVertexLimit)
            {
                return mesh;
            }

            if (HullSources.TryGetValue(mesh, out Mesh cached) && cached != null)
            {
                return cached;
            }

            Vector3[] vertices = mesh.vertices;
            var picked = new List<Vector3>();
            foreach (Vector3 dir in HullDirections())
            {
                int best = 0;
                float bestDot = float.NegativeInfinity;
                for (int i = 0; i < vertices.Length; i++)
                {
                    float dot = Vector3.Dot(vertices[i], dir);
                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        best = i;
                    }
                }

                if (!picked.Contains(vertices[best]))
                {
                    picked.Add(vertices[best]);
                }
            }

            // PhysX builds the hull from the point cloud; the triangle list only has to be valid.
            var triangles = new int[(picked.Count / 3) * 3];
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = i;
            }

            var hullMesh = new Mesh { name = mesh.name + "_Hull", hideFlags = HideFlags.DontSave };
            hullMesh.SetVertices(picked);
            hullMesh.SetTriangles(triangles, 0);
            HullSources[mesh] = hullMesh;
            return hullMesh;
        }

        /// <summary>12 icosahedron vertices + 30 edge midpoints, normalized: 42 well-spread directions.</summary>
        private static List<Vector3> HullDirections()
        {
            float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            var ico = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            var dirs = new List<Vector3>();
            foreach (Vector3 v in ico)
            {
                dirs.Add(v.normalized);
            }

            float edge = (ico[0] - ico[1]).magnitude;
            for (int a = 0; a < ico.Count; a++)
            {
                for (int b = a + 1; b < ico.Count; b++)
                {
                    if (Mathf.Abs((ico[a] - ico[b]).magnitude - edge) < 1e-3f)
                    {
                        dirs.Add((ico[a] + ico[b]).normalized);
                    }
                }
            }

            return dirs;
        }

        /// <summary>The single collider of type T on <paramref name="go"/>, removing any other collider type there.</summary>
        private static T Only<T>(GameObject go) where T : Collider
        {
            foreach (Collider other in go.GetComponents<Collider>())
            {
                if (!(other is T))
                {
                    SafeDestroy(other);
                }
            }

            T collider = go.GetComponent<T>();
            if (collider == null)
            {
                collider = go.AddComponent<T>();
            }

            collider.isTrigger = false;
            collider.enabled = true;
            return collider;
        }

        private static Bounds BoundsOf(List<Vector3> points)
        {
            var bounds = new Bounds(points[0], Vector3.zero);
            for (int i = 1; i < points.Count; i++)
            {
                bounds.Encapsulate(points[i]);
            }

            return bounds;
        }

        private static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private static Vector2 Flat(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        private static void SafeDestroy(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
