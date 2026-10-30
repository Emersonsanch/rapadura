using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>
    /// Builds the low-poly wing / cape meshes of <see cref="WingCatalog"/> (one side; the other is the mirror) with the
    /// procedural enemy palette (<see cref="EnemyMeshBuilder"/>: flat colour cells, one shared material). Pure geometry,
    /// tested for size and triangle budget in <c>TachoTests</c>.
    /// </summary>
    public static class WingMeshBuilder
    {
        /// <summary>Hard cap of one side (low-poly, mobile): every style and level stays under it.</summary>
        public const int MaxTrianglesPerSide = 96;

        /// <summary>Span multiplier per wing level (1, 2, 3): higher levels are a little bigger.</summary>
        public static float SpanScale(int level) => 1f + 0.15f * (Mathf.Clamp(level, 1, WingCatalog.MaxCraftableLevel) - 1);

        /// <summary>
        /// One side of <paramref name="spec"/> at <paramref name="level"/>, in pivot space: +X out from the spine, +Y up,
        /// -Z behind the hero. <paramref name="mirror"/> builds the left side.
        /// </summary>
        public static Mesh Build(WingStyleSpec spec, int level, bool mirror)
        {
            var b = new EnemyMeshBuilder();
            float s = SpanScale(level);
            b.Matrix = Matrix4x4.Scale(new Vector3(mirror ? -s : s, s, s));
            if (spec.IsCape)
            {
                BuildCapeHalf(b, spec, level);
            }
            else
            {
                BuildWing(b, spec, level);
            }

            Mesh mesh = b.ToMesh((mirror ? "WingL_" : "WingR_") + spec.Style + "_" + level);
            return mesh;
        }

        // Outline of a feathered wing (shoulder at the origin), fanned from the shoulder.
        private static readonly Vector2[] Outline =
        {
            new Vector2(0.02f, 0.06f), new Vector2(0.22f, 0.20f), new Vector2(0.42f, 0.28f), new Vector2(0.60f, 0.26f),
            new Vector2(0.66f, 0.14f), new Vector2(0.56f, 0.02f), new Vector2(0.46f, -0.10f), new Vector2(0.32f, -0.18f),
            new Vector2(0.18f, -0.20f), new Vector2(0.06f, -0.10f),
        };

        private static void BuildWing(EnemyMeshBuilder b, WingStyleSpec spec, int level)
        {
            Vector3 shoulder = Vector3.zero;
            for (int i = 0; i < Outline.Length - 1; i++)
            {
                Vector3 p = Lift(Outline[i]);
                Vector3 q = Lift(Outline[i + 1]);
                // Outer feathers (around the tip) in the tip colour, the leading edge in the accent, the rest the main colour.
                EnemySwatch swatch = i >= 3 && i <= 5 ? spec.TipSwatch : i <= 1 ? spec.AccentSwatch : spec.MainSwatch;
                b.TwoSided(shoulder, p, q, swatch);
            }

            // Level 2+: a smaller covert layer on top (accent); level 3: longer primaries that catch the light.
            if (level >= 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = Lift(Outline[i]) * 0.55f + new Vector3(0f, 0f, -0.012f);
                    Vector3 q = Lift(Outline[i + 1]) * 0.55f + new Vector3(0f, 0f, -0.012f);
                    b.TwoSided(shoulder + new Vector3(0f, 0f, -0.012f), p, q, spec.AccentSwatch);
                }
            }

            if (level >= 3)
            {
                for (int i = 3; i <= 5; i++)
                {
                    Vector3 a = Lift(Outline[i]);
                    Vector3 tip = a + new Vector3(0.10f, (i - 4) * 0.05f, -0.02f);
                    b.TwoSided(a, tip, Lift(Outline[i + 1]), spec.TipSwatch);
                }
            }
        }

        private static void BuildCapeHalf(EnemyMeshBuilder b, WingStyleSpec spec, int level)
        {
            // Half a cape (from the spine out to the shoulder), hanging down and curving away from the back.
            const int rows = 3;
            float length = 0.62f + 0.06f * (level - 1);
            for (int r = 0; r < rows; r++)
            {
                float t0 = r / (float)rows;
                float t1 = (r + 1) / (float)rows;
                Vector3 innerTop = new Vector3(0f, -t0 * length, -0.04f - t0 * 0.10f);
                Vector3 outerTop = new Vector3(0.18f + t0 * 0.06f, -t0 * length, -0.02f - t0 * 0.08f);
                Vector3 innerBottom = new Vector3(0f, -t1 * length, -0.04f - t1 * 0.10f);
                Vector3 outerBottom = new Vector3(0.18f + t1 * 0.06f, -t1 * length, -0.02f - t1 * 0.08f);
                EnemySwatch swatch = r == rows - 1 ? spec.AccentSwatch : spec.MainSwatch;
                b.TwoSided(innerTop, outerTop, outerBottom, swatch);
                b.TwoSided(innerTop, outerBottom, innerBottom, swatch);
            }

            // Golden buckle at the shoulder; level 2+ a hem stripe, level 3 a second buckle.
            b.Box(new Vector3(0.16f, 0.01f, 0.0f), new Vector3(0.05f, 0.05f, 0.02f), spec.TipSwatch);
            if (level >= 2)
            {
                b.Box(new Vector3(0.11f, -length + 0.02f, -0.15f), new Vector3(0.22f, 0.03f, 0.01f), spec.TipSwatch);
            }

            if (level >= 3)
            {
                b.Box(new Vector3(0.12f, -0.30f, -0.09f), new Vector3(0.04f, 0.04f, 0.02f), spec.TipSwatch);
            }
        }

        // The wing outline lies in the XY plane, bent slightly back (-Z) towards the tip.
        private static Vector3 Lift(Vector2 point) => new Vector3(point.x, point.y, -0.18f * point.x);
    }

    /// <summary>
    /// Shows the equipped wing (<see cref="EquipmentSlot.Wings"/>) on the hero's back: two procedural meshes
    /// (<see cref="WingMeshBuilder"/>) under a pivot parented to the upper-chest bone of the current hero model, so it follows
    /// the animation. A gentle flap rides the motion-feel springs (<see cref="MotionFeelMath.StepSpring(ref float, ref float, float, float, float, float, float)"/>):
    /// a slow idle beat plus a fold-back when running. "Reduzir Movimento" turns the idle beat off and halves the response
    /// (<see cref="AccessibilitySettings.MotionScaleFor"/>). Self-bootstrapping on the hero (like the mount controller);
    /// rebuilt when the equipment changes or the hero model is swapped.
    /// </summary>
    [DisallowMultipleComponent]
    public class WingBackVisual : MonoBehaviour
    {
        public const string RootName = "RapaduraWingsPivot";
        public const float IdleAmplitudeDegrees = 6f;
        public const float IdleFrequencyHz = 0.55f;
        public const float RunFoldDegrees = 14f;
        public const float SpringFrequencyHz = 2.2f;
        public const float SpringDamping = 0.35f;
        /// <summary>Speed (m/s) at which the wings are fully folded back.</summary>
        public const float FullFoldSpeed = 6f;
        public const float BackOffset = 0.12f;

        private InventoryManager _bag;
        private AccessibilitySettings _accessibility;
        private Transform _root;
        private Transform _left;
        private Transform _right;
        private Quaternion _leftRest;
        private Quaternion _rightRest;
        private string _shownItemId;
        private bool _dirty = true;
        private float _angle;
        private float _velocity;
        private float _time;
        private Vector3 _lastPosition;

        public string ShownItemId => _shownItemId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHero();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHero();

        private static void AttachToHero()
        {
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null && stats.GetComponent<InventoryManager>() != null && stats.GetComponent<WingBackVisual>() == null)
            {
                stats.gameObject.AddComponent<WingBackVisual>();
            }
        }

        /// <summary>
        /// The flap target (degrees) for an idle clock <paramref name="time"/> and a ground speed: idle beat (off with
        /// "Reduzir Movimento") minus the run fold, both scaled by <paramref name="motionScale"/>. Pure (tests).
        /// </summary>
        public static float FlapTarget(float time, float speed, bool reduceMotion, float motionScale)
        {
            float idle = reduceMotion ? 0f : IdleAmplitudeDegrees * Mathf.Sin(time * IdleFrequencyHz * 2f * Mathf.PI);
            float fold = RunFoldDegrees * Mathf.Clamp01(speed / FullFoldSpeed);
            return (idle - fold) * Mathf.Clamp01(motionScale);
        }

        private void Awake()
        {
            _bag = GetComponent<InventoryManager>();
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            ServiceLocator.TryGet(out _accessibility);
            _dirty = true;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            Clear();
        }

        private void OnInventoryChanged(InventoryChangedEvent _) => _dirty = true;

        private string EquippedWingId()
        {
            if (_bag == null || !_bag.Equipped.TryGetValue(EquipmentSlot.Wings, out InventorySlotData data) || data == null || data.IsEmpty)
            {
                return null;
            }

            return WingCatalog.IsWing(data.itemId) ? data.itemId : null;
        }

        private void LateUpdate()
        {
            // The hero model was swapped (its bones died with it): rebuild on the new one.
            if (_root == null && _shownItemId != null)
            {
                _shownItemId = null;
                _dirty = true;
            }

            if (_dirty)
            {
                _dirty = false;
                string wanted = EquippedWingId();
                if (wanted != _shownItemId)
                {
                    Clear();
                    if (wanted != null)
                    {
                        Build(wanted);
                    }
                }
            }

            if (_root == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            _time += dt;
            Vector3 position = transform.position;
            Vector3 delta = position - _lastPosition;
            delta.y = 0f;
            _lastPosition = position;
            float speed = dt > 0f ? delta.magnitude / dt : 0f;
            bool reduce = _accessibility != null && _accessibility.ReduceMotion;
            float target = FlapTarget(_time, speed, reduce, AccessibilitySettings.MotionScaleFor(_accessibility));
            MotionFeelMath.StepSpring(ref _angle, ref _velocity, target, SpringFrequencyHz, SpringDamping, dt);

            // Wings beat up/down around the hero's forward axis; a cape sways the same way, half as far.
            float angle = _angle * (_shownIsCape ? 0.5f : 1f);
            _right.localRotation = _rightRest * Quaternion.AngleAxis(angle, Vector3.forward);
            _left.localRotation = _leftRest * Quaternion.AngleAxis(-angle, Vector3.forward);
        }

        private bool _shownIsCape;

        private void Build(string itemId)
        {
            if (!WingCatalog.TryParse(itemId, out WingStyle style, out int level))
            {
                return;
            }

            Transform bone = FindBackBone();
            if (bone == null)
            {
                return;
            }

            WingStyleSpec spec = WingCatalog.Get(style);
            float heroScale = Mathf.Max(0.2f, transform.lossyScale.y);
            _root = new GameObject(RootName).transform;
            _root.SetPositionAndRotation(bone.position - transform.forward * BackOffset * heroScale,
                Quaternion.LookRotation(transform.forward, Vector3.up));
            _root.localScale = Vector3.one * heroScale;
            _root.SetParent(bone, true);

            // Wings open sideways and a little back; a cape hangs from the shoulders.
            float sweep = spec.IsCape ? 0f : 25f;
            _right = NewSide(spec, level, false, Quaternion.Euler(0f, -sweep, 0f));
            _left = NewSide(spec, level, true, Quaternion.Euler(0f, sweep, 0f));
            _rightRest = _right.localRotation;
            _leftRest = _left.localRotation;
            _shownIsCape = spec.IsCape;
            _shownItemId = itemId;
            _angle = 0f;
            _velocity = 0f;
        }

        private Transform NewSide(WingStyleSpec spec, int level, bool mirror, Quaternion rest)
        {
            var side = new GameObject(mirror ? "WingLeft" : "WingRight");
            side.transform.SetParent(_root, false);
            side.transform.localPosition = new Vector3(spec.IsCape ? 0f : (mirror ? -0.05f : 0.05f), 0.02f, 0f);
            side.transform.localRotation = rest;
            side.AddComponent<MeshFilter>().sharedMesh = WingMeshBuilder.Build(spec, level, mirror);
            var meshRenderer = side.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = ProceduralEnemyModels.SharedMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return side.transform;
        }

        /// <summary>Upper chest of the hero model (humanoid avatar first, then the Meshy/Mixamo names), else the model root.</summary>
        private Transform FindBackBone()
        {
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                if (animator.isHuman)
                {
                    Transform t = animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? animator.GetBoneTransform(HumanBodyBones.Chest)
                        ?? animator.GetBoneTransform(HumanBodyBones.Spine);
                    if (t != null)
                    {
                        return t;
                    }
                }
            }

            foreach (Animator animator in animators)
            {
                Transform t = FindDeep(animator.transform, "Spine") ?? FindDeep(animator.transform, "Spine01")
                    ?? FindDeep(animator.transform, "mixamorig:Spine2");
                if (t != null && FindDeep(animator.transform, "Head") != null)
                {
                    return t;
                }
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string boneName)
        {
            if (root.name == boneName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), boneName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void Clear()
        {
            if (_root != null)
            {
                foreach (MeshFilter filter in _root.GetComponentsInChildren<MeshFilter>())
                {
                    Destroy(filter.sharedMesh);
                }

                Destroy(_root.gameObject);
            }

            _root = null;
            _left = null;
            _right = null;
            _shownItemId = null;
        }
    }
}
