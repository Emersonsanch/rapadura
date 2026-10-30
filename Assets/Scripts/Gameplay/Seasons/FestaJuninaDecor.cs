using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>
    /// The Festa Junina decoration of the Vila da Vó Edna on the world map, built in code (no scene edit, no asset):
    /// poles around the city limit with strings of bandeirinhas between them, and a fogueira with a flickering light in
    /// the widest open side of the plaza. Self-bootstrapping like <c>WorldMapCityPresence</c>: after every scene load it
    /// sits on the <see cref="WorldMapDressing"/>, uses the dressing's palette material and meshes
    /// (<see cref="WorldMapMeshBuilder"/>), shows up while <see cref="SeasonCalendar.IsFestaJuninaActive"/> and goes away
    /// when the festa ends (or the debug toggle is turned off). The layout (<see cref="PoleAngles"/>, <see cref="FireAngle"/>)
    /// is pure and tested.
    /// </summary>
    [DisallowMultipleComponent]
    public class FestaJuninaDecor : MonoBehaviour
    {
        public const string RootName = "FestaJunina";
        public const float PoleRadius = 1.3f;
        public const float PoleHeight = 0.72f;
        public const int PoleCount = 8;
        public const float FireRadius = 0.85f;
        public const float StringSag = 0.12f;
        public const int FlagsPerString = 7;
        private const float RefreshInterval = 1f;

        private static readonly WorldMapSwatch[] FlagColours =
        {
            WorldMapSwatch.Banner, WorldMapSwatch.Gold, WorldMapSwatch.Crystal, WorldMapSwatch.Leaf, WorldMapSwatch.Cloud, WorldMapSwatch.Lantern,
        };

        private WorldMapDressing _dressing;
        private GameObject _root;
        private Transform _flame;
        private Light _light;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private float _nextRefresh;

        public bool IsBuilt => _root != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            WorldMapDressing dressing = FindAnyObjectByType<WorldMapDressing>();
            if (dressing != null && dressing.GetComponent<FestaJuninaDecor>() == null)
            {
                dressing.gameObject.AddComponent<FestaJuninaDecor>();
            }
        }

        // ------------------------------------------------------------------
        // Layout (pure)
        // ------------------------------------------------------------------

        /// <summary>
        /// Angles (degrees) of the poles on the city ring: <paramref name="count"/> evenly spaced, each one nudged out of a
        /// road's gate gap so no pole stands on a road.
        /// </summary>
        public static List<float> PoleAngles(IReadOnlyList<float> roadAngles, int count, float radius)
        {
            var angles = new List<float>(count);
            float halfGap = WorldMapCityDressing.GateHalfAngle(radius) + 6f;
            float step = 360f / Mathf.Max(1, count);

            // Free ring angles (1° apart, off every road gate); the poles are spread evenly over them, so a ring whose
            // roads leave only one side free still gets all its poles on that side.
            var free = new List<float>(360);
            for (int d = 0; d < 360; d++)
            {
                float a = 22.5f + d;
                if (!WorldMapCityDressing.IsInGate(a, roadAngles, halfGap))
                {
                    free.Add(Mathf.Repeat(a, 360f));
                }
            }

            for (int i = 0; i < count; i++)
            {
                angles.Add(free.Count == 0
                    ? Mathf.Repeat(22.5f + i * step, 360f)
                    : free[Mathf.Min(free.Count - 1, (int)((i + 0.5f) * free.Count / count))]);
            }

            angles.Sort();
            return angles;
        }

        /// <summary>The middle of the widest gap between roads (the fogueira's side of the plaza); 270 (toward the camera) without roads.</summary>
        public static float FireAngle(IReadOnlyList<float> roadAngles)
        {
            if (roadAngles == null || roadAngles.Count == 0)
            {
                return 270f;
            }

            var sorted = new List<float>();
            foreach (float a in roadAngles)
            {
                sorted.Add(Mathf.Repeat(a, 360f));
            }

            sorted.Sort();
            float best = -1f;
            float bestMid = 270f;
            for (int i = 0; i < sorted.Count; i++)
            {
                float from = sorted[i];
                float to = i + 1 < sorted.Count ? sorted[i + 1] : sorted[0] + 360f;
                float gap = to - from;
                if (gap > best)
                {
                    best = gap;
                    bestMid = Mathf.Repeat(from + gap * 0.5f, 360f);
                }
            }

            return bestMid;
        }

        // ------------------------------------------------------------------
        // Meshes (local to the node, y = 0 on the ground)
        // ------------------------------------------------------------------

        public static void AppendBandeirinhas(WorldMapMeshBuilder b, IReadOnlyList<float> poleAngles, float radius)
        {
            if (poleAngles == null || poleAngles.Count == 0)
            {
                return;
            }

            var tops = new List<Vector3>();
            foreach (float angle in poleAngles)
            {
                Vector2 d = WorldMapCityDressing.Direction(angle);
                var foot = new Vector3(d.x * radius, 0f, d.y * radius);
                b.Cylinder(foot, 0.025f, 0.02f, PoleHeight, 5, WorldMapSwatch.Wood);
                tops.Add(foot + Vector3.up * PoleHeight);
            }

            int colour = 0;
            for (int i = 0; i < tops.Count && tops.Count > 1; i++)
            {
                Vector3 a = tops[i];
                Vector3 c = tops[(i + 1) % tops.Count];
                Vector3 along = c - a;
                Vector3 side = Vector3.Cross(Vector3.up, along).normalized * 0.004f;
                for (int f = 0; f < FlagsPerString; f++)
                {
                    float t0 = (f + 0.15f) / FlagsPerString;
                    float t1 = (f + 0.85f) / FlagsPerString;
                    Vector3 p0 = Sag(a, c, t0);
                    Vector3 p1 = Sag(a, c, t1);
                    Vector3 tip = Sag(a, c, (t0 + t1) * 0.5f) + Vector3.down * 0.11f;
                    WorldMapSwatch swatch = FlagColours[colour++ % FlagColours.Length];
                    // Both faces: the flags are seen from either side of the plaza.
                    b.Triangle(p0 + side, tip + side, p1 + side, swatch);
                    b.Triangle(p1 - side, tip - side, p0 - side, swatch);
                }

                // The string itself: a thin dark ribbon along the sag.
                for (int s = 0; s < FlagsPerString; s++)
                {
                    Vector3 q0 = Sag(a, c, (float)s / FlagsPerString);
                    Vector3 q1 = Sag(a, c, (float)(s + 1) / FlagsPerString);
                    Vector3 up = Vector3.up * 0.008f;
                    b.Quad(q0 - up, q0 + up, q1 + up, q1 - up, WorldMapSwatch.WoodDark);
                    b.Quad(q1 - up, q1 + up, q0 + up, q0 - up, WorldMapSwatch.WoodDark);
                }
            }
        }

        private static Vector3 Sag(Vector3 a, Vector3 c, float t) => Vector3.Lerp(a, c, t) + Vector3.down * (StringSag * Mathf.Sin(Mathf.PI * t));

        public static void AppendFireLogs(WorldMapMeshBuilder b)
        {
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.04f + i * 0.045f, 0f), new Vector3(0.36f, 0.05f, 0.06f), i % 2 == 0 ? WorldMapSwatch.WoodDark : WorldMapSwatch.Trunk, i * 90f + 45f);
            }

            // Stones around the fire.
            for (int i = 0; i < 8; i++)
            {
                Vector2 d = WorldMapCityDressing.Direction(i * 45f);
                b.Box(new Vector3(d.x * 0.24f, 0.025f, d.y * 0.24f), new Vector3(0.07f, 0.05f, 0.06f), WorldMapSwatch.Rock, i * 30f);
            }
        }

        public static void AppendFlame(WorldMapMeshBuilder b)
        {
            b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.15f, 0f, 0.42f, 6, WorldMapSwatch.Banner);
            b.Cylinder(new Vector3(0f, 0.22f, 0f), 0.1f, 0f, 0.34f, 6, WorldMapSwatch.Lantern);
            b.Cylinder(new Vector3(0f, 0.24f, 0f), 0.05f, 0f, 0.22f, 5, WorldMapSwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            _dressing = GetComponent<WorldMapDressing>();
        }

        private void OnDestroy()
        {
            Teardown();
        }

        private void Update()
        {
            if (_root != null)
            {
                Flicker();
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            bool active = SeasonCalendar.IsFestaJuninaActive;
            if (active && _root == null)
            {
                TryBuild();
            }
            else if (!active && _root != null)
            {
                Teardown();
            }
        }

        private void Flicker()
        {
            float t = Time.unscaledTime;
            float n = Mathf.PerlinNoise(t * 3.1f, 0.37f);
            if (_flame != null)
            {
                float s = 0.85f + 0.3f * n;
                _flame.localScale = new Vector3(1f, s, 1f);
            }

            if (_light != null)
            {
                _light.intensity = 1.2f + 1.1f * n;
            }
        }

        private void TryBuild()
        {
            if (_dressing == null || !_dressing.IsBuilt || _dressing.Terrain == null || _dressing.PaletteMaterial == null)
            {
                return;
            }

            WorldMapTerrain terrain = _dressing.Terrain;
            int site = -1;
            for (int i = 0; i < terrain.Sites.Count; i++)
            {
                if (terrain.Sites[i].Region == FastTravelRules.HomeRegion)
                {
                    site = i;
                    break;
                }
            }

            if (site < 0)
            {
                return;
            }

            Vector2 center = terrain.Sites[site].Position;
            List<float> roads = WorldMapCityDressing.RoadAngles(terrain, site);
            _root = new GameObject(RootName);
            _root.transform.SetParent(_dressing.transform, false);
            _root.transform.localPosition = new Vector3(center.x, terrain.Height(center), center.y);

            var builder = new WorldMapMeshBuilder();
            AppendBandeirinhas(builder, PoleAngles(roads, PoleCount, PoleRadius), PoleRadius);
            Spawn("Bandeirinhas", builder, Vector3.zero, _root.transform, castShadows: false);

            Vector2 fireDir = WorldMapCityDressing.Direction(FireAngle(roads));
            Vector2 firePoint = center + fireDir * FireRadius;
            var fire = new GameObject("Fogueira");
            fire.transform.SetParent(_root.transform, false);
            fire.transform.localPosition = new Vector3(fireDir.x * FireRadius, terrain.Height(firePoint) - terrain.Height(center), fireDir.y * FireRadius);

            builder.Clear();
            AppendFireLogs(builder);
            Spawn("Lenha", builder, Vector3.zero, fire.transform, castShadows: true);

            builder.Clear();
            AppendFlame(builder);
            GameObject flame = Spawn("Chama", builder, Vector3.zero, fire.transform, castShadows: false);
            _flame = flame != null ? flame.transform : null;

            var lightObject = new GameObject("Luz");
            lightObject.transform.SetParent(fire.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            _light = lightObject.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(1f, 0.6f, 0.25f);
            _light.range = 2.2f;
            _light.intensity = 1.6f;
            _light.shadows = LightShadows.None;
        }

        private GameObject Spawn(string objectName, WorldMapMeshBuilder builder, Vector3 localPosition, Transform parent, bool castShadows)
        {
            if (builder.IsEmpty)
            {
                return null;
            }

            Mesh mesh = builder.ToMesh("FestaJunina" + objectName);
            _meshes.Add(mesh);
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _dressing.PaletteMaterial;
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            return go;
        }

        private void Teardown()
        {
            if (_root != null)
            {
                Destroy(_root);
                _root = null;
            }

            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }

            _meshes.Clear();
            _flame = null;
            _light = null;
        }
    }
}
