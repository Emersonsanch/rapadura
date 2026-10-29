using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Environment.Impact
{
    /// <summary>
    /// Turns gameplay events into ground strikes and drives the rocks' reactions:
    /// <list type="bullet">
    /// <item>an Earth-element area skill cast (Terremoto, Muralha de Pedra...) strikes at the caster
    ///   with the skill's area radius (+ a margin so the rocks just outside also rattle);</item>
    /// <item>a heavy enemy footfall at full intensity (the attack slam of golems/warlords) strikes
    ///   a small radius;</item>
    /// <item>a boss dying strikes a wide radius where it fell.</item>
    /// </list>
    /// Added to "[Environment]" by <see cref="EnvironmentPresentationBootstrap"/>. One Update walks
    /// only the shakers currently reacting (usually none), scaled time so pausing freezes them.
    /// </summary>
    public class GroundImpactSystem : MonoBehaviour
    {
        public const float SkillRadiusMargin = 2f;
        public const float StompRadius = 4f;
        public const float StompStrength = 0.6f;
        public const float StompIntensityThreshold = 0.95f;
        public const float BossDeathRadius = 8f;
        public const float BossDeathStrength = 1f;

        private readonly List<GroundImpactShaker> _reacting = new List<GroundImpactShaker>(64);

        private void OnEnable()
        {
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Subscribe<EnemyFootstepEvent>(OnFootstep);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Unsubscribe<EnemyFootstepEvent>(OnFootstep);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            _reacting.Clear();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = _reacting.Count - 1; i >= 0; i--)
            {
                GroundImpactShaker shaker = _reacting[i];
                if (shaker == null || !shaker.Step(deltaTime))
                {
                    _reacting.RemoveAt(i);
                }
            }
        }

        /// <summary>Strikes the ground: every registered shaker within <paramref name="radius"/> is kicked with distance falloff.</summary>
        public void Strike(Vector3 position, float radius, float strength)
        {
            if (radius <= 0f || strength <= 0f)
            {
                return;
            }

            var shakers = GroundImpactShaker.Registered;
            for (int i = 0; i < shakers.Count; i++)
            {
                GroundImpactShaker shaker = shakers[i];
                if (shaker == null)
                {
                    continue;
                }

                Vector3 toProp = shaker.transform.position - position;
                toProp.y = 0f;
                float distance = toProp.magnitude;
                float kick = GroundImpactMath.Kick(strength, distance, radius, shaker.Kind);
                if (kick <= 0.01f)
                {
                    continue;
                }

                Vector2 away = distance > 1e-4f ? new Vector2(toProp.x / distance, toProp.z / distance) : new Vector2(0f, 1f);
                bool wasReacting = shaker.IsReacting;
                shaker.Kick(kick, away);
                if (!wasReacting && shaker.IsReacting)
                {
                    _reacting.Add(shaker);
                }
            }
        }

        /// <summary>Whether a skill cast counts as a ground strike, and how wide.</summary>
        public static bool TryGetSkillStrike(SkillDefinition skill, out float radius)
        {
            radius = 0f;
            if (skill == null || skill.Element != ElementType.Earth || skill.AreaRadius <= 0f)
            {
                return false;
            }

            radius = skill.AreaRadius + SkillRadiusMargin;
            return true;
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Caster == null || !TryGetSkillStrike(evt.Skill, out float radius))
            {
                return;
            }

            Strike(evt.Caster.transform.position, radius, 1f);
        }

        private void OnFootstep(EnemyFootstepEvent evt)
        {
            if (evt.Intensity < StompIntensityThreshold)
            {
                return;
            }

            Strike(evt.Position, StompRadius, StompStrength * evt.Intensity);
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!evt.WasBoss || evt.Enemy == null)
            {
                return;
            }

            Strike(evt.Enemy.transform.position, BossDeathRadius, BossDeathStrength);
        }

#if UNITY_EDITOR
        /// <summary>Test-only: advance reactions without Play Mode.</summary>
        public void StepForTests(float deltaTime)
        {
            for (int i = _reacting.Count - 1; i >= 0; i--)
            {
                if (_reacting[i] == null || !_reacting[i].Step(deltaTime))
                {
                    _reacting.RemoveAt(i);
                }
            }
        }

        public int ReactingCountForTests => _reacting.Count;
        public void OnSkillCastForTests(SkillCastEvent evt) => OnSkillCast(evt);
        public void OnFootstepForTests(EnemyFootstepEvent evt) => OnFootstep(evt);
        public void OnEnemyDiedForTests(EnemyDiedEvent evt) => OnEnemyDied(evt);
#endif
    }
}
