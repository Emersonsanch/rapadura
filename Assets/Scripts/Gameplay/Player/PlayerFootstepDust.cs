using Rapadura.Core.Events;
using Rapadura.Gameplay.Skills.Vfx;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Small dust puff at the planted foot on this hero's harder footsteps (<see cref="PlayerFootstepEvent"/>
    /// with <see cref="PlayerFootstepEvent.Intensity"/> ≥ <see cref="_minIntensity"/>: jogging/running, not a
    /// stroll). One world-space ParticleSystem per hero, emitted on demand (no instantiation, capped particles),
    /// with the shared soft-dot material (<see cref="SkillVfxManager.GetSharedParticleMaterial"/>). Added by
    /// <see cref="PlayerFootsteps"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerFootstepDust : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _minIntensity = 0.5f;
        [SerializeField, Min(0)] private int _maxPuffParticles = 4;
        [SerializeField] private Color _color = new Color(0.72f, 0.63f, 0.5f, 0.45f);

        private ParticleSystem _particles;

        /// <summary>Puffs emitted (tests/debug).</summary>
        public int PuffCount { get; private set; }

        private void OnEnable() => EventBus.Subscribe<PlayerFootstepEvent>(OnFootstep);

        private void OnDisable() => EventBus.Unsubscribe<PlayerFootstepEvent>(OnFootstep);

        private void OnDestroy()
        {
            if (_particles != null)
            {
                if (Application.isPlaying) Destroy(_particles.gameObject);
                else DestroyImmediate(_particles.gameObject);
            }
        }

        private void OnFootstep(PlayerFootstepEvent evt)
        {
            if (evt.Player != gameObject || evt.Intensity < _minIntensity || _maxPuffParticles <= 0)
            {
                return;
            }

            EnsureParticles();
            int count = Mathf.Max(1, Mathf.RoundToInt(_maxPuffParticles * Mathf.InverseLerp(_minIntensity, 1f, evt.Intensity)));
            var emit = new ParticleSystem.EmitParams
            {
                position = evt.Position + Vector3.up * 0.03f,
                applyShapeToPosition = true,
            };
            _particles.Emit(emit, count);
            PuffCount++;
        }

        private void EnsureParticles()
        {
            if (_particles != null)
            {
                return;
            }

            var go = new GameObject("FootstepDust");
            go.transform.SetParent(transform, false);
            _particles = go.AddComponent<ParticleSystem>();
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = _particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 32;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            main.startColor = _color;
            main.gravityModifier = -0.05f;

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.08f;

            ParticleSystem.SizeOverLifetimeModule size = _particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.4f)));

            ParticleSystem.ColorOverLifetimeModule color = _particles.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SkillVfxManager.GetSharedParticleMaterial(false);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
