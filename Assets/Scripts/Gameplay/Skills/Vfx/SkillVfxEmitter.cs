using UnityEngine;

namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>Emission shape for a skill VFX burst.</summary>
    public enum SkillVfxShape
    {
        /// <summary>Small sphere — casts and single-target impacts.</summary>
        Sphere,

        /// <summary>Flat ring on the ground (XZ plane) — area impacts, auras, summons.</summary>
        Ring,

        /// <summary>Upward cone — rising columns (summons, heals, ultimates).</summary>
        Column
    }

    /// <summary>Fully-resolved settings for one emitter play (already passed through <see cref="SkillVfxFlashPolicy"/>). Plain struct: no allocations.</summary>
    public struct SkillVfxPlayback
    {
        public SkillVfxBurst Burst;
        public SkillVfxBurst Flash;
        public Color SecondaryColor;
        public float Speed;
        public float Lifetime;
        public float Gravity;
        public float Radius;
        public SkillVfxShape Shape;
        public bool Additive;

        /// <summary>Particles per second while travelling / while an aura is active.</summary>
        public float Rate;
    }

    /// <summary>
    /// One pooled, procedurally-built particle emitter (a main ParticleSystem plus a child "flash"
    /// ParticleSystem) owned by <see cref="SkillVfxManager"/>. Reconfigured per play through
    /// ParticleSystem module structs only, so playing an effect never allocates. Only ticks while
    /// checked out of the pool (inactive GameObjects get no Update), to move a projectile, follow an
    /// aura target and hand itself back once its particles have died.
    /// </summary>
    public class SkillVfxEmitter : MonoBehaviour
    {
        private const int MaxParticles = 64;
        private const float FlashLifetime = 0.14f;

        private enum Mode
        {
            Idle,
            Burst,
            Travel,
            Aura
        }

        private SkillVfxManager _owner;
        private ParticleSystem _main;
        private ParticleSystem _flash;
        private ParticleSystemRenderer _mainRenderer;
        private Material _alphaMaterial;
        private Material _additiveMaterial;

        private Mode _mode;
        private float _releaseAt;
        private float _lingerSeconds;
        private Vector3 _travelTarget;
        private float _travelSpeed;
        private Transform _follow;
        private Vector3 _followOffset;
        private float _auraEndAt;

        private SkillVfxPlayback _pendingImpact;
        private bool _hasPendingImpact;

        public bool HasPendingImpact => _hasPendingImpact;
        public SkillVfxPlayback PendingImpact => _pendingImpact;

        /// <summary>Builds a new inactive emitter (main + flash ParticleSystems) under <paramref name="parent"/>.</summary>
        public static SkillVfxEmitter Create(SkillVfxManager owner, Transform parent, Material alphaMaterial, Material additiveMaterial, Gradient fadeGradient, AnimationCurve shrinkCurve, AnimationCurve flashGrowCurve)
        {
            var go = new GameObject("SkillVfx_Emitter");
            go.SetActive(false);
            go.transform.SetParent(parent, false);

            var emitter = go.AddComponent<SkillVfxEmitter>();
            emitter._owner = owner;
            emitter._alphaMaterial = alphaMaterial;
            emitter._additiveMaterial = additiveMaterial;

            emitter._main = go.AddComponent<ParticleSystem>();
            ConfigureStatic(emitter._main, fadeGradient, shrinkCurve);
            emitter._mainRenderer = go.GetComponent<ParticleSystemRenderer>();
            emitter._mainRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            emitter._mainRenderer.sharedMaterial = alphaMaterial;

            var flashGo = new GameObject("Flash");
            flashGo.transform.SetParent(go.transform, false);
            emitter._flash = flashGo.AddComponent<ParticleSystem>();
            ConfigureStatic(emitter._flash, fadeGradient, flashGrowCurve);
            var flashMain = emitter._flash.main;
            flashMain.maxParticles = 2;
            flashMain.startLifetime = FlashLifetime;
            flashMain.startSpeed = 0f;
            var flashShape = emitter._flash.shape;
            flashShape.enabled = false;
            var flashRenderer = flashGo.GetComponent<ParticleSystemRenderer>();
            flashRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            flashRenderer.sharedMaterial = additiveMaterial;

            return emitter;
        }

        private static void ConfigureStatic(ParticleSystem ps, Gradient fadeGradient, AnimationCurve sizeCurve)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.maxParticles = MaxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        /// <summary>Called by the pool when the emitter is handed out.</summary>
        public void OnTakenFromPool()
        {
            gameObject.SetActive(true);
            _main.Play(true);
        }

        /// <summary>Called by the pool when the emitter is returned.</summary>
        public void OnReturnedToPool()
        {
            _mode = Mode.Idle;
            _follow = null;
            _hasPendingImpact = false;
            _main.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gameObject.SetActive(false);
        }

        /// <summary>One-shot burst at <paramref name="position"/>.</summary>
        public void PlayBurst(Vector3 position, in SkillVfxPlayback playback)
        {
            transform.position = position;
            Apply(playback, ParticleSystemSimulationSpace.World, 0f);
            EmitBurst(playback);
            _mode = Mode.Burst;
            _releaseAt = Time.time + playback.Lifetime + FlashLifetime + 0.1f;
        }

        /// <summary>Moves from <paramref name="from"/> to <paramref name="to"/> leaving a trail; on arrival the owner plays <paramref name="impact"/> there.</summary>
        public void PlayTravel(Vector3 from, Vector3 to, float speed, in SkillVfxPlayback trail, in SkillVfxPlayback impact, bool hasImpact)
        {
            transform.position = from;
            Apply(trail, ParticleSystemSimulationSpace.World, trail.Rate);
            _travelTarget = to;
            _travelSpeed = Mathf.Max(0.1f, speed);
            _pendingImpact = impact;
            _hasPendingImpact = hasImpact;
            _lingerSeconds = trail.Lifetime + 0.1f;
            _mode = Mode.Travel;
        }

        /// <summary>Follows <paramref name="follow"/> for <paramref name="duration"/> seconds, emitting continuously (buff/heal auras).</summary>
        public void PlayAura(Transform follow, Vector3 offset, float duration, in SkillVfxPlayback playback)
        {
            _follow = follow;
            _followOffset = offset;
            transform.position = follow != null ? follow.position + offset : transform.position;
            Apply(playback, ParticleSystemSimulationSpace.Local, playback.Rate);
            EmitBurst(playback);
            _auraEndAt = Time.time + duration;
            _lingerSeconds = playback.Lifetime + 0.1f;
            _mode = Mode.Aura;
        }

        private void Update()
        {
            switch (_mode)
            {
                case Mode.Travel:
                    TickTravel();
                    break;
                case Mode.Aura:
                    TickAura();
                    break;
                case Mode.Burst:
                    if (Time.time >= _releaseAt)
                    {
                        Finish();
                    }
                    break;
            }
        }

        private void TickTravel()
        {
            Vector3 position = Vector3.MoveTowards(transform.position, _travelTarget, _travelSpeed * Time.deltaTime);
            transform.position = position;

            if ((position - _travelTarget).sqrMagnitude > 0.0001f)
            {
                return;
            }

            StopEmitting();
            _mode = Mode.Burst;
            _releaseAt = Time.time + _lingerSeconds;

            if (_hasPendingImpact)
            {
                _hasPendingImpact = false;
                if (_owner != null)
                {
                    _owner.HandleProjectileArrived(position, _pendingImpact);
                }
            }
        }

        private void TickAura()
        {
            if (_follow != null)
            {
                transform.position = _follow.position + _followOffset;
            }

            if (_follow == null || Time.time >= _auraEndAt)
            {
                StopEmitting();
                _mode = Mode.Burst;
                _releaseAt = Time.time + _lingerSeconds;
            }
        }

        private void Finish()
        {
            _mode = Mode.Idle;
            if (_owner != null)
            {
                _owner.ReleaseEmitter(this);
            }
            else
            {
                OnReturnedToPool();
            }
        }

        private void StopEmitting()
        {
            var emission = _main.emission;
            emission.rateOverTime = 0f;
        }

        private void Apply(in SkillVfxPlayback playback, ParticleSystemSimulationSpace space, float rate)
        {
            var main = _main.main;
            main.simulationSpace = space;
            main.startColor = new ParticleSystem.MinMaxGradient(playback.Burst.Color, playback.SecondaryColor);
            main.startSize = new ParticleSystem.MinMaxCurve(playback.Burst.Size * 0.6f, playback.Burst.Size);
            main.startSpeed = new ParticleSystem.MinMaxCurve(playback.Speed * 0.5f, playback.Speed);
            main.startLifetime = new ParticleSystem.MinMaxCurve(playback.Lifetime * 0.7f, playback.Lifetime);
            main.gravityModifier = playback.Gravity;

            var shape = _main.shape;
            shape.enabled = true;
            switch (playback.Shape)
            {
                case SkillVfxShape.Ring:
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.rotation = new Vector3(90f, 0f, 0f);
                    shape.radius = Mathf.Max(0.05f, playback.Radius);
                    shape.radiusThickness = 0.2f;
                    shape.arc = 360f;
                    break;
                case SkillVfxShape.Column:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.rotation = new Vector3(-90f, 0f, 0f);
                    shape.angle = 8f;
                    shape.radius = Mathf.Max(0.05f, playback.Radius);
                    shape.radiusThickness = 1f;
                    shape.arc = 360f;
                    break;
                default:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.rotation = Vector3.zero;
                    shape.radius = Mathf.Max(0.05f, playback.Radius);
                    shape.radiusThickness = 1f;
                    break;
            }

            var emission = _main.emission;
            emission.rateOverTime = Mathf.Max(0f, rate);

            _mainRenderer.sharedMaterial = playback.Additive ? _additiveMaterial : _alphaMaterial;
        }

        private void EmitBurst(in SkillVfxPlayback playback)
        {
            if (!playback.Burst.IsSkipped)
            {
                _main.Emit(Mathf.Min(playback.Burst.Count, MaxParticles));
            }

            if (!playback.Flash.IsSkipped)
            {
                var flashMain = _flash.main;
                flashMain.startColor = playback.Flash.Color;
                flashMain.startSize = playback.Flash.Size;
                _flash.Emit(1);
            }
        }
    }
}
