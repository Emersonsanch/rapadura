using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>
    /// The Bema decoration of the Vila da Vó Edna on the world map (the class keeps its old Festa Junina name), built in
    /// code (no scene edit, no asset): poles around the city limit with ribbons in the colours of the five lights and
    /// strings of paper lanterns between them, and, in the widest open side of the plaza, the <b>Bema</b> itself: a dais
    /// of five steps with the empty seat on top (nobody sits in it), embroidered cloth, flowers and oil lamps (the light
    /// offerings), and Grandpa Jorge's rocking chair brought to the square beside it. Self-bootstrapping like
    /// <c>WorldMapCityPresence</c>: after every scene load it sits on the <see cref="WorldMapDressing"/>, uses the
    /// dressing's palette material and meshes (<see cref="WorldMapMeshBuilder"/>), shows up while
    /// <see cref="SeasonCalendar.IsFestaJuninaActive"/> and goes away when the Bema ends (or the debug toggle is turned
    /// off). The layout (<see cref="PoleAngles"/>, <see cref="FireAngle"/> = where the Bema stands) is pure and tested.
    /// </summary>
    [DisallowMultipleComponent]
    public class FestaJuninaDecor : MonoBehaviour
    {
        public const string RootName = "FestaJunina";
        public const float PoleRadius = 1.3f;
        public const float PoleHeight = 0.72f;
        public const int PoleCount = 8;
        /// <summary>How far from the plaza's centre the Bema stands (the old fogueira's spot).</summary>
        public const float FireRadius = 0.85f;
        public const float StringSag = 0.12f;
        /// <summary>Paper lanterns on each string.</summary>
        public const int FlagsPerString = 7;
        public const float RibbonLength = 0.2f;

        /// <summary>The Bema: five steps.</summary>
        public const int BemaSteps = 5;
        public const float BemaBaseWidth = 0.56f;
        public const float BemaStepInset = 0.045f;
        public const float BemaStepHeight = 0.05f;
        public const float LampBodyHeight = 0.022f;
        public const float TallLampHeight = 0.3f;

        private const float RefreshInterval = 1f;

        /// <summary>Where Grandpa Jorge's rocking chair stands, beside the Bema (local, front toward +z).</summary>
        public static readonly Vector3 RockingChairOffset = new Vector3(0.42f, 0f, 0.1f);

        /// <summary>The tall lamp beside the chair: the "first lamp of the Bema" Grandpa Jorge used to light.</summary>
        public static readonly Vector3 TallLampFoot = new Vector3(0.56f, 0f, 0.02f);

        /// <summary>
        /// The five lights: Pure Air (silver: the palette has no lilac cell), Living Wind (blue-white), Light (gold),
        /// Living Water (aqua) and Living Fire (orange).
        /// </summary>
        private static readonly WorldMapSwatch[] ElementColours =
        {
            WorldMapSwatch.Fog, WorldMapSwatch.CloudShade, WorldMapSwatch.Gold, WorldMapSwatch.Crystal, WorldMapSwatch.Lantern,
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

        /// <summary>The middle of the widest gap between roads (the Bema's side of the plaza); 270 (toward the camera) without roads.</summary>
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

        /// <summary>
        /// Yaw (degrees) of the Bema: its front (local +z) turned toward the map camera (world -z), so the empty seat, the
        /// runner and the lamps always read from the player's view wherever the widest gap is.
        /// </summary>
        public const float BemaYaw = 180f;

        /// <summary>Width of step <paramref name="step"/> (0 = the bottom one) of the Bema.</summary>
        public static float StepWidth(int step) => BemaBaseWidth - step * BemaStepInset * 2f;

        /// <summary>Height of the top of step <paramref name="step"/>.</summary>
        public static float StepTop(int step) => (step + 1) * BemaStepHeight;

        /// <summary>
        /// Where the light offerings stand (local to the Bema, front toward +z): two lamps on the front corners of every
        /// step, plus the tall lamp beside Grandpa Jorge's chair. Shared by the lamp bodies and the flames.
        /// </summary>
        public static List<Vector3> LampPositions()
        {
            var lamps = new List<Vector3>(BemaSteps * 2 + 1);
            for (int i = 0; i < BemaSteps; i++)
            {
                float half = StepWidth(i) * 0.5f - 0.03f;
                lamps.Add(new Vector3(-half, StepTop(i), half));
                lamps.Add(new Vector3(half, StepTop(i), half));
            }

            lamps.Add(TallLampFoot + Vector3.up * TallLampHeight);
            return lamps;
        }

        // ------------------------------------------------------------------
        // Meshes (local to the node, y = 0 on the ground)
        // ------------------------------------------------------------------

        /// <summary>
        /// The poles and the strings between them (the method keeps its old bunting name): each pole carries five ribbons
        /// in the colours of the five lights, and each string hangs <see cref="FlagsPerString"/> paper lanterns along its sag.
        /// </summary>
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
                Vector3 top = foot + Vector3.up * PoleHeight;
                tops.Add(top);

                // Five ribbons hanging from the top of the pole, one per light, seen from both sides.
                for (int r = 0; r < ElementColours.Length; r++)
                {
                    Vector2 rd = WorldMapCityDressing.Direction(angle + r * 72f);
                    var outward = new Vector3(rd.x, 0f, rd.y);
                    Vector3 side = Vector3.Cross(Vector3.up, outward).normalized * 0.012f;
                    Vector3 p0 = top + outward * 0.022f + Vector3.down * 0.01f;
                    Vector3 p1 = top + outward * 0.05f + Vector3.down * RibbonLength;
                    b.Quad(p0 - side, p0 + side, p1 + side, p1 - side, ElementColours[r]);
                    b.Quad(p1 - side, p1 + side, p0 + side, p0 - side, ElementColours[r]);
                }
            }

            int colour = 0;
            for (int i = 0; i < tops.Count && tops.Count > 1; i++)
            {
                Vector3 a = tops[i];
                Vector3 c = tops[(i + 1) % tops.Count];

                // Paper lanterns: a short thread, a dark cap and the lantern itself, cycling the five colours.
                for (int f = 0; f < FlagsPerString; f++)
                {
                    Vector3 hook = Sag(a, c, (f + 0.5f) / FlagsPerString);
                    WorldMapSwatch swatch = ElementColours[colour++ % ElementColours.Length];
                    b.Box(hook + Vector3.down * 0.015f, new Vector3(0.006f, 0.03f, 0.006f), WorldMapSwatch.WoodDark);
                    b.Box(hook + Vector3.down * 0.032f, new Vector3(0.03f, 0.008f, 0.03f), WorldMapSwatch.WoodDark, 45f);
                    b.Box(hook + Vector3.down * 0.062f, new Vector3(0.042f, 0.054f, 0.042f), swatch, 45f);
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

        /// <summary>
        /// The Bema (the method keeps its old fire-logs name): a dais of five adobe steps with an embroidered runner, the
        /// empty seat on top under a white cloth, flowers and oil lamps on the steps, and Grandpa Jorge's rocking chair
        /// beside it with the tall first lamp. Front toward local +z.
        /// </summary>
        public static void AppendFireLogs(WorldMapMeshBuilder b)
        {
            for (int i = 0; i < BemaSteps; i++)
            {
                float w = StepWidth(i);
                float mid = StepTop(i) - BemaStepHeight * 0.5f;
                b.Box(new Vector3(0f, mid, 0f), new Vector3(w, BemaStepHeight, w), WorldMapSwatch.Ruin, 0f, i % 2 == 0 ? WorldMapSwatch.Wall : WorldMapSwatch.Ruin);
                b.Box(new Vector3(0f, mid, w * 0.5f), new Vector3(0.12f, BemaStepHeight + 0.004f, 0.012f), WorldMapSwatch.Banner);
                b.Box(new Vector3(0f, StepTop(i) + 0.002f, w * 0.5f - 0.025f), new Vector3(0.12f, 0.004f, 0.05f), WorldMapSwatch.Banner);
            }

            // The empty seat on the top step, a white cloth over it.
            float top = StepTop(BemaSteps - 1);
            b.Box(new Vector3(0f, top + 0.04f, 0f), new Vector3(0.15f, 0.08f, 0.13f), WorldMapSwatch.Gold);
            b.Box(new Vector3(0f, top + 0.084f, 0.005f), new Vector3(0.13f, 0.008f, 0.12f), WorldMapSwatch.Cloud);
            b.Box(new Vector3(0f, top + 0.17f, -0.055f), new Vector3(0.15f, 0.18f, 0.025f), WorldMapSwatch.Gold);
            b.Box(new Vector3(0f, top + 0.15f, -0.04f), new Vector3(0.11f, 0.13f, 0.008f), WorldMapSwatch.Cloud);
            b.Box(new Vector3(-0.07f, top + 0.12f, 0f), new Vector3(0.018f, 0.018f, 0.12f), WorldMapSwatch.Gold);
            b.Box(new Vector3(0.07f, top + 0.12f, 0f), new Vector3(0.018f, 0.018f, 0.12f), WorldMapSwatch.Gold);

            // Flowers on the back corners of the steps.
            for (int i = 0; i < BemaSteps; i++)
            {
                float half = StepWidth(i) * 0.5f - 0.035f;
                WorldMapSwatch petals = i % 2 == 0 ? WorldMapSwatch.Flower : WorldMapSwatch.Cloud;
                var size = new Vector3(0.045f, 0.035f, 0.045f);
                b.Blob(new Vector3(-half, StepTop(i) + 0.02f, -half), size, petals, 0f, true);
                b.Blob(new Vector3(half, StepTop(i) + 0.02f, -half), size, petals, 0f, true);
            }

            // The light offerings: small oil lamps (the tall one's bowl sits on its post).
            foreach (Vector3 lamp in LampPositions())
            {
                b.Cylinder(lamp, 0.022f, 0.016f, LampBodyHeight, 6, WorldMapSwatch.Gold, WorldMapSwatch.WoodDark);
            }

            b.Cylinder(TallLampFoot, 0.02f, 0.012f, TallLampHeight, 5, WorldMapSwatch.WoodDark);

            AppendRockingChair(b, RockingChairOffset);
        }

        /// <summary>Grandpa Jorge's rocking chair, brought from the porch to the square for the day. Front toward +z.</summary>
        public static void AppendRockingChair(WorldMapMeshBuilder b, Vector3 at)
        {
            b.Box(at + new Vector3(-0.055f, 0.012f, 0f), new Vector3(0.016f, 0.016f, 0.22f), WorldMapSwatch.WoodDark);
            b.Box(at + new Vector3(0.055f, 0.012f, 0f), new Vector3(0.016f, 0.016f, 0.22f), WorldMapSwatch.WoodDark);
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -0.055f : 0.055f;
                float z = i < 2 ? -0.05f : 0.05f;
                b.Box(at + new Vector3(x, 0.055f, z), new Vector3(0.014f, 0.08f, 0.014f), WorldMapSwatch.Wood);
            }

            b.Box(at + new Vector3(0f, 0.1f, 0f), new Vector3(0.13f, 0.018f, 0.12f), WorldMapSwatch.Wood);
            b.Box(at + new Vector3(0f, 0.2f, -0.058f), new Vector3(0.13f, 0.18f, 0.016f), WorldMapSwatch.Wood);
            b.Box(at + new Vector3(0f, 0.2f, -0.048f), new Vector3(0.09f, 0.12f, 0.006f), WorldMapSwatch.Trunk);
            b.Box(at + new Vector3(-0.065f, 0.155f, 0f), new Vector3(0.016f, 0.014f, 0.13f), WorldMapSwatch.WoodDark);
            b.Box(at + new Vector3(0.065f, 0.155f, 0f), new Vector3(0.016f, 0.014f, 0.13f), WorldMapSwatch.WoodDark);
        }

        /// <summary>The little flames of the light offerings (the method keeps its old bonfire name), one on every <see cref="LampPositions"/>.</summary>
        public static void AppendFlame(WorldMapMeshBuilder b)
        {
            foreach (Vector3 lamp in LampPositions())
            {
                Vector3 p = lamp + Vector3.up * LampBodyHeight;
                b.Cylinder(p, 0.014f, 0f, 0.05f, 5, WorldMapSwatch.Lantern);
                b.Cylinder(p + Vector3.up * 0.004f, 0.007f, 0f, 0.032f, 4, WorldMapSwatch.Gold);
            }
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
            float n = Mathf.PerlinNoise(t * 2.3f, 0.37f);
            if (_flame != null)
            {
                // The flames sit at different heights, so only a gentle stretch (they barely move off their lamps).
                float s = 0.97f + 0.06f * n;
                _flame.localScale = new Vector3(1f, s, 1f);
            }

            if (_light != null)
            {
                _light.intensity = 1.1f + 0.6f * n;
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
            Spawn("Lanternas", builder, Vector3.zero, _root.transform, castShadows: false);

            float bemaAngle = FireAngle(roads);
            Vector2 bemaDir = WorldMapCityDressing.Direction(bemaAngle);
            Vector2 bemaPoint = center + bemaDir * FireRadius;
            var bema = new GameObject("Bema");
            bema.transform.SetParent(_root.transform, false);
            bema.transform.localPosition = new Vector3(bemaDir.x * FireRadius, terrain.Height(bemaPoint) - terrain.Height(center), bemaDir.y * FireRadius);
            bema.transform.localRotation = Quaternion.Euler(0f, BemaYaw, 0f);

            builder.Clear();
            AppendFireLogs(builder);
            Spawn("Estrado", builder, Vector3.zero, bema.transform, castShadows: true);

            builder.Clear();
            AppendFlame(builder);
            GameObject flame = Spawn("Lamparinas", builder, Vector3.zero, bema.transform, castShadows: false);
            _flame = flame != null ? flame.transform : null;

            var lightObject = new GameObject("Luz");
            lightObject.transform.SetParent(bema.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, StepTop(BemaSteps - 1) + 0.25f, 0.15f);
            _light = lightObject.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(1f, 0.82f, 0.5f);
            _light.range = 2.2f;
            _light.intensity = 1.4f;
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
