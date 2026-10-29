using Rapadura.Core.Events;
using Rapadura.Gameplay.Content;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Central combat "juice" — hit-spark particle bursts and floating damage numbers — driven
    /// entirely off the existing <see cref="DamageAppliedEvent"/>/<see cref="CombatTargetDiedEvent"/>
    /// (already published by the real damage pipeline, see CombatEvents.cs), so no combat code needs
    /// to know this exists. Everything here is generated procedurally at runtime (particle material,
    /// font, meshes) — no external assets/API cost, matching the rest of this session's "zero-cost
    /// procedural over paid generation where zero-cost gets the job done" pattern.
    ///
    /// Was entirely missing before this change: hits landed with only a camera shake/hit-stop and no
    /// per-hit visual feedback at all, and there was no floating damage number anywhere in the project.
    /// </summary>
    public class CombatFxManager : MonoBehaviour
    {
        private static Material _sharedParticleMaterial;
        private static Font _sharedFont;

        private void OnEnable()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnTargetDied);
            EventBus.Subscribe<StunAppliedEvent>(OnStunApplied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnTargetDied);
            EventBus.Unsubscribe<StunAppliedEvent>(OnStunApplied);
        }

        /// <summary>Stun cue: a pale-blue spark ring above the head and a floating "Atordoado!" (localized).</summary>
        private void OnStunApplied(StunAppliedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            Vector3 position = evt.Target.transform.position + Vector3.up * 2f;
            var stunColor = new Color(0.55f, 0.8f, 1f);
            SpawnHitSpark(position, stunColor, 0.8f, burstCount: 12);
            string text = ContentText.Resolve(ContentText.CurrentManager, "combat.stunned", "Atordoado!");
            SpawnFloatingText(position + Vector3.up * 0.3f, text, 0.1f, stunColor);
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            Vector3 position = evt.Target.transform.position + Vector3.up * 1.2f;
            Color sparkColor = evt.IsCritical ? new Color(1f, 0.55f, 0.1f) : new Color(1f, 0.9f, 0.4f);
            SpawnHitSpark(position, sparkColor, evt.IsCritical ? 1.6f : 1f);
            SpawnDamageNumber(position, evt.FinalDamage, evt.IsCritical);
        }

        private void OnTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            SpawnHitSpark(evt.Target.transform.position + Vector3.up * 0.8f, new Color(0.75f, 0.75f, 0.75f), 2.2f, burstCount: 18);
        }

        private static Material GetParticleMaterial()
        {
            if (_sharedParticleMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Sprites/Default");
                _sharedParticleMaterial = new Material(shader) { name = "Mat_CombatSpark_Runtime" };
            }
            return _sharedParticleMaterial;
        }

        private static void SpawnHitSpark(Vector3 position, Color color, float scale, int burstCount = 8)
        {
            var go = new GameObject("HitSpark_FX");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 3f * scale;
            main.startSize = 0.12f * scale;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.2f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
            Object.Destroy(go, main.duration + main.startLifetime.constantMax + 0.1f);
        }

        private static void SpawnDamageNumber(Vector3 position, float damage, bool isCritical)
        {
            SpawnFloatingText(position + new Vector3(Random.Range(-0.25f, 0.25f), 0f, 0f), Mathf.RoundToInt(damage).ToString(),
                isCritical ? 0.18f : 0.12f, isCritical ? new Color(1f, 0.5f, 0.1f) : Color.white);
        }

        /// <summary>Floating loot label ("+2 Madeira") over a corpse, used by <c>EnemyLootGranter</c>.</summary>
        public static void SpawnLootText(Vector3 position, string text)
        {
            if (string.IsNullOrEmpty(text) || !Application.isPlaying)
            {
                return;
            }

            SpawnFloatingText(position, text, 0.09f, new Color(1f, 0.85f, 0.35f));
        }

        private static void SpawnFloatingText(Vector3 position, string text, float characterSize, Color color)
        {
            var go = new GameObject("DamageNumber_FX");
            go.transform.position = position;

            var textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.characterSize = characterSize;
            textMesh.fontSize = 64;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            if (_sharedFont == null)
            {
                _sharedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            if (_sharedFont != null)
            {
                textMesh.font = _sharedFont;
                textMesh.GetComponent<MeshRenderer>().sharedMaterial = _sharedFont.material;
            }

            go.AddComponent<FloatingDamageNumber>();
        }
    }
}
