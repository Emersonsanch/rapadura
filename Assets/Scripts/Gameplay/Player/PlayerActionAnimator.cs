using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player.States;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Fires the hero's one-shot action clips (mocap: attack, cast, hit reaction, death/revive) on
    /// the rigged visual's <see cref="Animator"/> from gameplay events, when the controller has
    /// them (built by <c>Rapadura/Animation/Rebuild Hero Animators</c>). On a controller without
    /// those parameters (Maria's generic rig, Maithe's rigid body, old controllers) every call is a
    /// silent no-op and <see cref="PlayerProceduralAnimator"/> keeps posing the body itself; when
    /// the clips exist, the procedural layer only adds lean/head-look/flash on top (it reads
    /// <see cref="HasClipActions"/> / <see cref="HasDeathClip"/>).
    ///
    /// Added to the Player at runtime by <see cref="PlayerController"/> if missing, so the scene
    /// needs no wiring; <see cref="PlayerVisualSwapper"/> re-targets it after swapping the model.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class PlayerActionAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private PlayerStats _stats;
        private RuntimeAnimatorController _cachedController;
        private bool _hasAttack;
        private bool _hasCast;
        private bool _hasHit;
        private bool _hasDeath;
        private bool _wasDead;
        private int _attackVariant;

        /// <summary>Controller plays real attack/cast clips (the procedural arm whip should stand down).</summary>
        public bool HasClipActions => Refresh() && (_hasAttack || _hasCast);
        /// <summary>Controller plays a real death clip (the procedural topple should stand down; the sink stays).</summary>
        public bool HasDeathClip => Refresh() && _hasDeath;
        public Animator Animator => _animator;

        /// <summary>
        /// Real-time seconds from an attack input to the moment the hand is fastest, as baked by the
        /// rebuilder (<see cref="PlayerAnimatorHashes.AttackStrikeTimeName"/>); the contract default
        /// when the controller has no clips so the damage timing never changes with the model.
        /// </summary>
        public float AttackStrikeDelay
        {
            get
            {
                if (Refresh() && _hasAttack)
                {
                    // The swing about to play is the other variant from the last one fired.
                    int next = 1 - _attackVariant;
                    int param = next == 1 && PlayerAnimatorHashes.HasParameter(_animator, PlayerAnimatorHashes.AttackStrikeTimeB)
                        ? PlayerAnimatorHashes.AttackStrikeTimeB
                        : PlayerAnimatorHashes.AttackStrikeTime;
                    if (PlayerAnimatorHashes.HasParameter(_animator, param))
                    {
                        float baked = _animator.GetFloat(param);
                        if (baked > 0f)
                        {
                            return baked;
                        }
                    }
                }

                return HeroActionTiming.StrikeDelay;
            }
        }

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>Re-targets the Animator after the visual model was swapped.</summary>
        public void SetAnimator(Animator animator)
        {
            _animator = animator;
            _cachedController = null;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            _wasDead = _stats != null && _stats.IsDead;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void Update()
        {
            if (_stats == null)
            {
                return;
            }

            bool dead = _stats.IsDead;
            if (dead != _wasDead)
            {
                _wasDead = dead;
                if (Refresh() && _hasDeath)
                {
                    if (dead)
                    {
                        _animator.SetInteger(PlayerAnimatorHashes.DeathVariant, Random.value < 0.5f ? 0 : 1);
                        _animator.ResetTrigger(PlayerAnimatorHashes.Revive);
                        _animator.SetTrigger(PlayerAnimatorHashes.Die);
                    }
                    else
                    {
                        _animator.ResetTrigger(PlayerAnimatorHashes.Die);
                        _animator.SetTrigger(PlayerAnimatorHashes.Revive);
                    }
                }
            }
        }

        private bool Refresh()
        {
            if (_animator == null)
            {
                return false;
            }

            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
            if (controller == null)
            {
                _cachedController = null;
                return false;
            }

            if (!ReferenceEquals(controller, _cachedController))
            {
                _cachedController = controller;
                _hasAttack = PlayerAnimatorHashes.HasParameter(_animator, PlayerAnimatorHashes.Attack);
                _hasCast = PlayerAnimatorHashes.HasParameter(_animator, PlayerAnimatorHashes.Cast);
                _hasHit = PlayerAnimatorHashes.HasParameter(_animator, PlayerAnimatorHashes.Hit);
                _hasDeath = PlayerAnimatorHashes.HasParameter(_animator, PlayerAnimatorHashes.Die);
            }

            return _animator.isActiveAndEnabled;
        }

        private bool IsDead => _stats != null && _stats.IsDead;

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker != gameObject || IsDead || !Refresh() || !_hasAttack)
            {
                return;
            }

            _attackVariant = 1 - _attackVariant;
            _animator.SetInteger(PlayerAnimatorHashes.AttackVariant, _attackVariant);
            _animator.ResetTrigger(PlayerAnimatorHashes.Hit);
            _animator.SetTrigger(PlayerAnimatorHashes.Attack);
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Caster != gameObject || IsDead || !Refresh() || !_hasCast)
            {
                return;
            }

            _animator.ResetTrigger(PlayerAnimatorHashes.Hit);
            _animator.SetTrigger(PlayerAnimatorHashes.Cast);
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Target != gameObject || IsDead || !Refresh() || !_hasHit)
            {
                return;
            }

            _animator.SetTrigger(PlayerAnimatorHashes.Hit);
        }

#if UNITY_EDITOR
        public void OnAttackPerformedForTests(PlayerAttackPerformedEvent evt) => OnAttackPerformed(evt);
        public void OnSkillCastForTests(SkillCastEvent evt) => OnSkillCast(evt);
        public void OnDamageAppliedForTests(DamageAppliedEvent evt) => OnDamageApplied(evt);
#endif
    }
}
