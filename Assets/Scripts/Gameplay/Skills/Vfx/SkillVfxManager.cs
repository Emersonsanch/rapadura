using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Pooling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>
    /// Per-skill procedural VFX, driven entirely off the existing <see cref="SkillCastEvent"/>
    /// (published by <see cref="SkillManager.TryCast"/>) — subscribed, never polled, and no
    /// skill/combat code needs to know this exists (same pattern as <c>CombatFxManager</c>).
    ///
    /// For each cast: the look comes from <see cref="SkillVfxLibrary.Resolve(SkillDefinition)"/>
    /// (override &gt; element &gt; category &gt; default) and the spawn pattern from
    /// <see cref="SkillVfxClassifier"/>:
    ///   - cast burst at the caster (skipped when the skill has its own CastParticlesPrefab);
    ///   - Projectile: a trail travelling caster → target point, then an impact burst on arrival
    ///     (damage itself is applied instantly by SkillManager; the trail is purely cosmetic and fast);
    ///   - Area/Debuff/Summon/Ultimate: ground ring (+ column/flash) at the target point;
    ///   - Heal/Buff, or any skill with self buffs: an aura following the caster.
    /// Impacts are skipped when the skill has its own ImpactVfxPrefab (SkillManager spawns that).
    ///
    /// Everything (materials, soft-dot texture, ParticleSystems) is generated in code and pooled via
    /// <see cref="GenericObjectPool{T}"/>; playing an effect allocates nothing. "Reduce flashes"
    /// (<see cref="AccessibilitySettings.ReduceFlashEffects"/>) is applied per burst through
    /// <see cref="SkillVfxFlashPolicy"/>.
    ///
    /// Self-bootstrapping (no scene wiring needed), like <c>UiAccessibilityApplier</c>; a scene may
    /// also contain one explicitly (e.g. to assign a different library), which is then reused.
    /// </summary>
    public class SkillVfxManager : MonoBehaviour
    {
        private const int MaxBurstCount = 32;

        [SerializeField] private SkillVfxLibrary _library;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int _prewarmCount = 6;
        [SerializeField, Min(1)] private int _maxActiveEmitters = 20;
        [SerializeField, Min(1)] private int _maxPooledEmitters = 24;

        [Header("Placement")]
        [SerializeField] private float _castHeight = 1f;
        [SerializeField] private float _impactHeight = 0.4f;
        [SerializeField] private float _groundHeight = 0.1f;

        [Header("Timing")]
        [SerializeField, Min(1f)] private float _projectileSpeed = 24f;
        [SerializeField, Min(0.05f)] private float _maxProjectileTravelSeconds = 0.45f;
        [SerializeField, Min(0.1f)] private float _auraSeconds = 1.2f;

        private static SkillVfxManager _instance;
        private static Material _alphaMaterial;
        private static Material _additiveMaterial;
        private static Texture2D _softDotTexture;

        private GenericObjectPool<SkillVfxEmitter> _pool;
        private Gradient _fadeGradient;
        private AnimationCurve _shrinkCurve;
        private AnimationCurve _flashGrowCurve;

        public SkillVfxLibrary Library => _library;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<SkillVfxManager>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(SkillVfxManager));
            host.AddComponent<SkillVfxManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureInitialized();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Pooled emitters are children of this GameObject and are destroyed along with it.
            _pool = null;
        }

        private void EnsureInitialized()
        {
            if (_pool != null)
            {
                return;
            }

            if (_library == null)
            {
                _library = Resources.Load<SkillVfxLibrary>(SkillVfxLibrary.ResourcePath);
            }

            if (_library == null)
            {
                _library = SkillVfxLibrary.CreateBuiltInDefaults();
            }

            BuildSharedCurves();
            EnsureMaterials();

            _pool = new GenericObjectPool<SkillVfxEmitter>(
                CreateEmitter,
                emitter => emitter.OnTakenFromPool(),
                emitter => emitter.OnReturnedToPool(),
                emitter =>
                {
                    if (emitter == null)
                    {
                        return;
                    }

                    if (Application.isPlaying)
                    {
                        Destroy(emitter.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(emitter.gameObject);
                    }
                },
                _maxPooledEmitters);
            _pool.Prewarm(_prewarmCount);
        }

        private SkillVfxEmitter CreateEmitter()
        {
            return SkillVfxEmitter.Create(this, transform, _alphaMaterial, _additiveMaterial, _fadeGradient, _shrinkCurve, _flashGrowCurve);
        }

        // ---------------------------------------------------------------
        // Event handling
        // ---------------------------------------------------------------

        private void OnSkillCast(SkillCastEvent evt)
        {
            SkillDefinition skill = evt.Skill;
            if (skill == null || evt.Caster == null || skill.SkillType == SkillType.Passive)
            {
                return;
            }

            EnsureInitialized();

            bool reduceFlashes = IsReduceFlashEnabled();
            SkillVfxPreset preset = _library.Resolve(skill);
            SkillVfxCategory category = SkillVfxClassifier.Classify(skill);
            Transform caster = evt.Caster.transform;
            Vector3 casterPoint = caster.position + Vector3.up * _castHeight;
            Vector3 impactPoint = evt.TargetPoint + Vector3.up * _impactHeight;
            Vector3 groundPoint = evt.TargetPoint + Vector3.up * _groundHeight;
            bool customImpact = skill.ImpactVfxPrefab != null;
            bool isUltimate = category == SkillVfxCategory.Ultimate;

            if (skill.CastParticlesPrefab == null)
            {
                SkillVfxPlayback cast = Build(preset, reduceFlashes, isUltimate ? 1f : 0.6f, isUltimate ? 1.2f : 0.9f, SkillVfxShape.Sphere, 0.3f, withFlash: isUltimate || preset.flashLike, flashSize: isUltimate ? 1.6f : 0.9f);
                SpawnBurst(casterPoint, cast);
            }

            float area = Mathf.Max(0f, skill.AreaRadius);

            switch (category)
            {
                case SkillVfxCategory.Projectile:
                    SpawnProjectile(preset, reduceFlashes, casterPoint, impactPoint, area, !customImpact);
                    break;

                case SkillVfxCategory.Area:
                    if (!customImpact)
                    {
                        float countScale = Mathf.Clamp(1f + area * 0.15f, 1f, 2f);
                        SpawnBurst(groundPoint, Build(preset, reduceFlashes, countScale, 1f, SkillVfxShape.Ring, Mathf.Max(0.5f, area * 0.85f), preset.flashLike, 1.2f + area * 0.2f));
                    }
                    break;

                case SkillVfxCategory.Debuff:
                    if (!customImpact)
                    {
                        SkillVfxPlayback debuff = Build(preset, reduceFlashes, 1f, 1f, SkillVfxShape.Ring, Mathf.Max(0.6f, area * 0.7f), false, 0f);
                        debuff.Gravity = Mathf.Abs(debuff.Gravity) + 0.3f;
                        SpawnBurst(impactPoint + Vector3.up * 0.6f, debuff);
                    }
                    break;

                case SkillVfxCategory.Summon:
                    SpawnBurst(groundPoint, Build(preset, reduceFlashes, 0.8f, 1f, SkillVfxShape.Ring, 1f, false, 0f));
                    SkillVfxPlayback column = Build(preset, reduceFlashes, 0.8f, 0.9f, SkillVfxShape.Column, 0.35f, preset.flashLike, 1f);
                    column.Gravity = -Mathf.Abs(column.Gravity) - 0.3f;
                    SpawnBurst(groundPoint, column);
                    break;

                case SkillVfxCategory.Ultimate:
                    if (!customImpact)
                    {
                        // Self-centred ultimates (no damage: big heals/buffs) erupt around the caster instead of the aim point.
                        Vector3 center = skill.BaseDamage > 0f ? groundPoint : caster.position + Vector3.up * _groundHeight;
                        float radius = Mathf.Max(2.5f, area * 0.85f);
                        SpawnBurst(center, Build(preset, reduceFlashes, 1.5f, 1.2f, SkillVfxShape.Ring, radius, true, 2.2f));
                        SkillVfxPlayback pillar = Build(preset, reduceFlashes, 1f, 1f, SkillVfxShape.Column, radius * 0.4f, false, 0f);
                        pillar.Gravity = -Mathf.Abs(pillar.Gravity) - 0.5f;
                        SpawnBurst(center, pillar);
                    }
                    break;
            }

            bool hasSelfBuff = skill.Buffs != null && skill.Buffs.Length > 0;
            if (category == SkillVfxCategory.Heal || category == SkillVfxCategory.Buff || hasSelfBuff)
            {
                SpawnAura(preset, reduceFlashes, caster, category == SkillVfxCategory.Heal, isUltimate ? _auraSeconds * 1.5f : _auraSeconds);
            }
        }

        private void SpawnProjectile(SkillVfxPreset preset, bool reduceFlashes, Vector3 from, Vector3 to, float area, bool withImpact)
        {
            bool splash = area > 0.5f;
            SkillVfxPlayback impact = splash
                ? Build(preset, reduceFlashes, 1.2f, 1f, SkillVfxShape.Ring, area * 0.6f, preset.flashLike, 1f + area * 0.2f)
                : Build(preset, reduceFlashes, 1f, 1f, SkillVfxShape.Sphere, 0.2f, preset.flashLike, 0.8f);

            float distance = Vector3.Distance(from, to);
            SkillVfxEmitter emitter = distance > 0.5f ? Acquire() : null;

            if (emitter == null)
            {
                if (withImpact)
                {
                    SpawnBurst(to, impact);
                }
                return;
            }

            SkillVfxPlayback trail = Build(preset, reduceFlashes, 1f, 0.8f, SkillVfxShape.Sphere, 0.08f, false, 0f);
            trail.Speed *= 0.15f;
            trail.Lifetime *= 0.7f;
            trail.Rate = preset.trailRate * (reduceFlashes && preset.flashLike ? SkillVfxFlashPolicy.ReducedCountMultiplier : 1f);

            float speed = Mathf.Max(_projectileSpeed, distance / _maxProjectileTravelSeconds);
            emitter.PlayTravel(from, to, speed, trail, impact, withImpact);
        }

        private void SpawnAura(SkillVfxPreset preset, bool reduceFlashes, Transform caster, bool isHeal, float duration)
        {
            SkillVfxEmitter emitter = Acquire();
            if (emitter == null)
            {
                return;
            }

            SkillVfxPlayback aura = Build(preset, reduceFlashes, 0.6f, 0.8f, SkillVfxShape.Ring, 0.6f, false, 0f);
            aura.Speed *= 0.4f;
            aura.Gravity = -Mathf.Abs(aura.Gravity) - (isHeal ? 0.35f : 0.15f);
            aura.Rate = preset.trailRate * 0.5f * (reduceFlashes && preset.flashLike ? SkillVfxFlashPolicy.ReducedCountMultiplier : 1f);
            emitter.PlayAura(caster, Vector3.up * _groundHeight, duration, aura);
        }

        private void SpawnBurst(Vector3 position, in SkillVfxPlayback playback)
        {
            if (playback.Burst.IsSkipped && playback.Flash.IsSkipped)
            {
                return;
            }

            SkillVfxEmitter emitter = Acquire();
            if (emitter != null)
            {
                emitter.PlayBurst(position, playback);
            }
        }

        private SkillVfxEmitter Acquire()
        {
            if (_pool == null || _pool.ActiveCount >= _maxActiveEmitters)
            {
                return null;
            }

            return _pool.Get();
        }

        /// <summary>Called by a travelling <see cref="SkillVfxEmitter"/> when it reaches its target.</summary>
        internal void HandleProjectileArrived(Vector3 position, in SkillVfxPlayback impact)
        {
            SpawnBurst(position, impact);
        }

        /// <summary>Called by an emitter once all its particles have died.</summary>
        internal void ReleaseEmitter(SkillVfxEmitter emitter)
        {
            if (_pool != null)
            {
                _pool.Release(emitter);
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        /// <summary>Builds a playback from a preset with the reduce-flash policy already applied.</summary>
        public static SkillVfxPlayback Build(SkillVfxPreset preset, bool reduceFlashes, float countScale, float sizeScale, SkillVfxShape shape, float radius, bool withFlash, float flashSize)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(preset.burstCount * countScale), 1, MaxBurstCount);
            Color flashColor = Color.Lerp(preset.primaryColor, Color.white, 0.5f);

            return new SkillVfxPlayback
            {
                Burst = SkillVfxFlashPolicy.Apply(new SkillVfxBurst(count, preset.primaryColor, preset.particleSize * sizeScale, false, preset.flashLike), reduceFlashes),
                Flash = SkillVfxFlashPolicy.Apply(new SkillVfxBurst(withFlash ? 1 : 0, flashColor, flashSize, true, true), reduceFlashes),
                SecondaryColor = SkillVfxFlashPolicy.Apply(new SkillVfxBurst(1, preset.secondaryColor, 1f, false, preset.flashLike), reduceFlashes).Color,
                Speed = preset.particleSpeed,
                Lifetime = Mathf.Max(0.05f, preset.particleLifetime),
                Gravity = preset.gravity,
                Radius = radius,
                Shape = shape,
                Additive = SkillVfxFlashPolicy.UseAdditive(preset.additive, preset.flashLike, reduceFlashes),
                Rate = 0f
            };
        }

        private static bool IsReduceFlashEnabled()
        {
            return ServiceLocator.TryGet(out AccessibilitySettings settings) && settings != null && settings.ReduceFlashEffects;
        }

        private void BuildSharedCurves()
        {
            _fadeGradient = new Gradient();
            _fadeGradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            _shrinkCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.2f));
            _flashGrowCurve = new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1f));
        }

        /// <summary>
        /// Shared runtime particle material (URP Particles/Unlit + generated soft-dot texture), alpha or
        /// additive. Lets other procedural effects (e.g. <c>AmbientVfx</c>) reuse the same materials.
        /// </summary>
        public static Material GetSharedParticleMaterial(bool additive)
        {
            EnsureMaterials();
            return additive ? _additiveMaterial : _alphaMaterial;
        }

        private static void EnsureMaterials()
        {
            if (_softDotTexture == null)
            {
                _softDotTexture = CreateSoftDotTexture(32);
            }

            if (_alphaMaterial == null)
            {
                _alphaMaterial = CreateParticleMaterial("Mat_SkillVfx_Alpha_Runtime", additive: false);
            }

            if (_additiveMaterial == null)
            {
                _additiveMaterial = CreateParticleMaterial("Mat_SkillVfx_Additive_Runtime", additive: true);
            }
        }

        private static Material CreateParticleMaterial(string materialName, bool additive)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = materialName };

            material.mainTexture = _softDotTexture;
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", _softDotTexture);
            }

            float src = (float)BlendMode.SrcAlpha;
            float dst = additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha;
            SetFloatIfPresent(material, "_Surface", 1f);
            SetFloatIfPresent(material, "_Blend", additive ? 2f : 0f);
            SetFloatIfPresent(material, "_SrcBlend", src);
            SetFloatIfPresent(material, "_DstBlend", dst);
            SetFloatIfPresent(material, "_SrcBlendAlpha", additive ? (float)BlendMode.One : src);
            SetFloatIfPresent(material, "_DstBlendAlpha", dst);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static void SetFloatIfPresent(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static Texture2D CreateSoftDotTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Tex_SkillVfxSoftDot_Runtime",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float alpha = 1f - Mathf.SmoothStep(0f, 1f, d);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only entry point for the private <see cref="OnSkillCast"/> handler (OnEnable isn't called synchronously by AddComponent in Edit Mode).</summary>
        public void OnSkillCastForTests(SkillCastEvent evt) => OnSkillCast(evt);

        /// <summary>Editor/test-only: number of emitters currently checked out of the pool.</summary>
        public int ActiveEmitterCountForTests => _pool != null ? _pool.ActiveCount : 0;

        /// <summary>Editor/test-only: injects a library before first use.</summary>
        public void SetLibraryForTests(SkillVfxLibrary library) => _library = library;
#endif
    }
}
