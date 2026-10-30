using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// The voice of a night species model (added by <see cref="ProceduralEnemyModels.Instantiate(ProceduralEnemyBlueprint, Material, Transform)"/>
    /// on its visual root, so it is also in the baked prefab): a call shortly after it appears and now and then while it
    /// prowls (<see cref="NightCreatureAudio.CallCue"/>), and the attack cue on every <see cref="EnemyAttackWindupEvent"/>
    /// of its own enemy — the sound is part of the telegraph. Pool-safe: everything is re-armed in <see cref="OnEnable"/>
    /// and silent once the enemy is dead; no per-frame allocation.
    /// </summary>
    [DisallowMultipleComponent]
    public class NightCreatureVoice : MonoBehaviour
    {
        [SerializeField] private ProceduralEnemyKind _kind;

        private Action<EnemyAttackWindupEvent> _onWindup;
        private Transform _enemyRoot;
        private Health _health;
        private float _nextCall;
        private uint _rng;

        public ProceduralEnemyKind Kind => _kind;

        /// <summary>Seconds until the next prowl call (tests).</summary>
        public float NextCallIn => _nextCall;

        public void Configure(ProceduralEnemyKind kind)
        {
            _kind = kind;
        }

        private void OnEnable()
        {
            EnemyController controller = GetComponentInParent<EnemyController>();
            _enemyRoot = controller != null ? controller.transform : transform.root;
            _health = _enemyRoot != null ? _enemyRoot.GetComponent<Health>() : null;
            _rng = (uint)(Mathf.Abs(GetInstanceID()) * 2654435761L % uint.MaxValue) | 1u;
            // First call soon after the creature arrives (staggered so a herd doesn't grunt in unison).
            _nextCall = 0.5f + Random01() * 1.2f;
            _onWindup ??= OnWindup;
            EventBus.Subscribe(_onWindup);
        }

        private void OnDisable()
        {
            if (_onWindup != null)
            {
                EventBus.Unsubscribe(_onWindup);
            }
        }

        private void Update()
        {
            if (_health != null && _health.IsDead)
            {
                return;
            }

            _nextCall -= Time.deltaTime;
            if (_nextCall > 0f)
            {
                return;
            }

            Vector2 interval = NightCreatureAudio.CallInterval(_kind);
            _nextCall = Mathf.Lerp(interval.x, interval.y, Random01());
            NightCreatureAudio.Play(NightCreatureAudio.CallCue(_kind), transform.position);
        }

        private void OnWindup(EnemyAttackWindupEvent evt)
        {
            if (evt.Enemy == null || _enemyRoot == null || evt.Enemy.transform != _enemyRoot)
            {
                return;
            }

            NightCreatureAudio.Play(NightCreatureAudio.AttackCue(_kind), evt.Position);
        }

        /// <summary>xorshift: cheap, allocation-free, independent from UnityEngine.Random's global state.</summary>
        private float Random01()
        {
            _rng ^= _rng << 13;
            _rng ^= _rng >> 17;
            _rng ^= _rng << 5;
            return (_rng & 0xFFFFFF) / 16777216f;
        }
    }
}
