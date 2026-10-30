using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>
    /// The partner cousin of a duo strike, drawn as a light golden "ghost": 5 primitive meshes (body, head, two arms,
    /// and a ground halo) sharing one runtime unlit transparent material, tinted per strike with a
    /// <see cref="MaterialPropertyBlock"/>. No skinning, no Animator, no asset load.
    /// <para>
    /// Why not the partner's real rigged model: loading a second hero means a <c>Resources.Load</c> of an FBX
    /// prefab (a hitch exactly at the climax of the fight), a second SkinnedMeshRenderer with GPU/CPU skinning, its
    /// own materials and shadow casting, and a second procedural animator (whose files belong to the mocap work).
    /// On the mobile target that's ~2× the hero's per-frame cost for 1.5 s; the silhouette costs 5 tiny draw calls
    /// with no shadows and reads as "the cousin came to help from the village" — the others are not in the arena.
    /// </para>
    /// Motion: fades in beside the hero, lunges toward the target with the arms raised at the impact, then fades out;
    /// with "Reduzir Movimento" it only fades in place, with "Reduzir Flashes" it is dimmer and alpha-blended.
    /// </summary>
    public class DuoPartnerSilhouette : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static Mesh _capsule;
        private static Mesh _sphere;
        private static Mesh _quad;
        private static Material _additiveMaterial;
        private static Material _alphaMaterial;

        private Renderer[] _renderers;
        private Transform _leftArm;
        private Transform _rightArm;
        private MaterialPropertyBlock _block;
        private Vector3 _origin;
        private Vector3 _lungeDirection;
        private Color _color;
        private float _alpha;
        private float _duration;
        private float _impactAt;
        private float _elapsed;
        private bool _lunge;
        private bool _pulse;
        private float _scale;

        /// <summary>Normalised time (0..1) of the strike.</summary>
        public float Progress => _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;

        /// <summary>
        /// Alpha envelope (pure): fades in over the first 15%, holds, fades out over the last 30%.
        /// <paramref name="instant"/> (reduced motion) skips the fades.
        /// </summary>
        public static float Envelope(float t, bool instant)
        {
            if (t < 0f || t > 1f)
            {
                return 0f;
            }

            if (instant)
            {
                return t < 0.85f ? 1f : 0f;
            }

            if (t < 0.15f)
            {
                return t / 0.15f;
            }

            return t > 0.7f ? Mathf.Clamp01((1f - t) / 0.3f) : 1f;
        }

        /// <summary>Lunge distance (0..1 of the max) at normalised time <paramref name="t"/>: peaks at the impact, then settles back.</summary>
        public static float LungeCurve(float t, float impactAt)
        {
            if (t <= 0f || impactAt <= 0f)
            {
                return 0f;
            }

            if (t < impactAt)
            {
                float k = t / impactAt;
                return k * k;
            }

            return Mathf.Clamp01(1f - (t - impactAt) / Mathf.Max(0.01f, 1f - impactAt) * 0.6f);
        }

        /// <summary>Builds and starts a silhouette at <paramref name="position"/> facing <paramref name="facing"/>.</summary>
        public static DuoPartnerSilhouette Spawn(Vector3 position, Vector3 facing, float heroHeight, Color color, DuoStrikePresentation look,
            float durationSeconds, float impactSeconds)
        {
            var root = new GameObject("DuoPartnerSilhouette");
            root.transform.position = position;
            facing.y = 0f;
            root.transform.rotation = facing.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(facing.normalized, Vector3.up) : Quaternion.identity;

            DuoPartnerSilhouette silhouette = root.AddComponent<DuoPartnerSilhouette>();
            silhouette.Build(Mathf.Clamp(heroHeight, 0.8f, 2.5f), look.AdditiveSilhouette);
            silhouette._origin = position;
            silhouette._lungeDirection = root.transform.forward;
            silhouette._color = color;
            silhouette._alpha = look.SilhouetteAlpha;
            silhouette._lunge = look.Lunge;
            silhouette._pulse = look.Pulse;
            silhouette._duration = Mathf.Max(0.1f, durationSeconds);
            silhouette._impactAt = Mathf.Clamp01(impactSeconds / silhouette._duration);
            silhouette.Apply(0f);
            return silhouette;
        }

        private void Build(float height, bool additive)
        {
            EnsureShared();
            _scale = height / 1.5f;
            Material material = additive ? _additiveMaterial : _alphaMaterial;

            // Proportions of a ~1.5 m kid: body capsule, round head, arms pivoting at the shoulders, ground halo.
            Part("Body", _capsule, material, transform, new Vector3(0f, 0.62f, 0f), new Vector3(0.42f, 0.42f, 0.36f));
            Part("Head", _sphere, material, transform, new Vector3(0f, 1.28f, 0f), new Vector3(0.36f, 0.38f, 0.36f));
            _leftArm = Pivot("LeftShoulder", new Vector3(-0.27f, 1.02f, 0f));
            _rightArm = Pivot("RightShoulder", new Vector3(0.27f, 1.02f, 0f));
            Part("LeftArm", _capsule, material, _leftArm, new Vector3(0f, -0.26f, 0f), new Vector3(0.12f, 0.26f, 0.12f));
            Part("RightArm", _capsule, material, _rightArm, new Vector3(0f, -0.26f, 0f), new Vector3(0.12f, 0.26f, 0.12f));
            Transform halo = Part("Halo", _quad, material, transform, new Vector3(0f, 0.03f, 0f), new Vector3(1.1f, 1.1f, 1f));
            halo.localRotation = Quaternion.Euler(90f, 0f, 0f);

            transform.localScale = Vector3.one * _scale;
            _renderers = GetComponentsInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private Transform Pivot(string pivotName, Vector3 localPosition)
        {
            var pivot = new GameObject(pivotName).transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = localPosition;
            return pivot;
        }

        private static Transform Part(string partName, Mesh mesh, Material material, Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            var part = new GameObject(partName);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return part.transform;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Progress;
            Apply(t);
            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void Apply(float t)
        {
            float alpha = Envelope(t, instant: !_lunge) * _alpha;
            if (_pulse)
            {
                // Short golden swell right at the impact.
                float distance = Mathf.Abs(t - _impactAt);
                alpha *= 1f + Mathf.Clamp01(1f - distance / 0.12f) * 0.5f;
            }

            if (_lunge)
            {
                transform.position = _origin + _lungeDirection * (LungeCurve(t, _impactAt) * 0.9f * _scale);
                float raise = Mathf.Clamp01(t / Mathf.Max(0.01f, _impactAt));
                float arm = Mathf.Lerp(10f, 150f, raise * raise) * (t > _impactAt ? Mathf.Clamp01(1f - (t - _impactAt) * 2f) * 0.6f + 0.4f : 1f);
                _leftArm.localRotation = Quaternion.Euler(-arm, 0f, 12f);
                _rightArm.localRotation = Quaternion.Euler(-arm, 0f, -12f);
            }
            else
            {
                _leftArm.localRotation = Quaternion.Euler(-60f, 0f, 12f);
                _rightArm.localRotation = Quaternion.Euler(-60f, 0f, -12f);
            }

            Color color = _color;
            color.a = Mathf.Clamp01(alpha);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);
            foreach (Renderer renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(_block);
                }
            }
        }

        private static void EnsureShared()
        {
            if (_capsule == null || _sphere == null || _quad == null)
            {
                _capsule = GrabPrimitiveMesh(PrimitiveType.Capsule);
                _sphere = GrabPrimitiveMesh(PrimitiveType.Sphere);
                _quad = GrabPrimitiveMesh(PrimitiveType.Quad);
            }

            if (_additiveMaterial == null)
            {
                _additiveMaterial = CreateMaterial("Mat_DuoSilhouette_Additive_Runtime", additive: true);
            }

            if (_alphaMaterial == null)
            {
                _alphaMaterial = CreateMaterial("Mat_DuoSilhouette_Alpha_Runtime", additive: false);
            }
        }

        private static Mesh GrabPrimitiveMesh(PrimitiveType type)
        {
            // CreatePrimitive is the one supported way to reach the built-in meshes at runtime; the temp object
            // (and its collider) is destroyed right away, only the shared mesh is kept.
            GameObject temp = GameObject.CreatePrimitive(type);
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Collider collider = temp.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Destroy(temp);
            return mesh;
        }

        private static Material CreateMaterial(string materialName, bool additive)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = materialName };
            float src = (float)BlendMode.SrcAlpha;
            float dst = additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha;
            SetFloat(material, "_Surface", 1f);
            SetFloat(material, "_Blend", additive ? 2f : 0f);
            SetFloat(material, "_SrcBlend", src);
            SetFloat(material, "_DstBlend", dst);
            SetFloat(material, "_SrcBlendAlpha", additive ? (float)BlendMode.One : src);
            SetFloat(material, "_DstBlendAlpha", dst);
            SetFloat(material, "_ZWrite", 0f);
            SetFloat(material, "_Cull", (float)CullMode.Back);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }
    }
}
