using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>
    /// Stays in place attacking the target on a cooldown, using the shared Hitbox if one is assigned, or
    /// applying damage directly to the target's ICombatTarget otherwise.
    ///
    /// Every attack is telegraphed: a windup of <see cref="EnemyController.ResolvedAttackWindup"/>
    /// (≥ <see cref="EnemyMotionMath.MinTelegraph"/>, per archetype) runs before the damage moment
    /// ("strike"), and EnemyProceduralAnimator plays lean-back → lunge → recover timed to it (the lunge
    /// peaks exactly at the strike). Strike-to-strike spacing is still exactly
    /// <see cref="EnemyDefinition.AttackCooldown"/> and damage is unchanged; the windup is taken out of
    /// the cooldown, so only the first hit after entering range comes later (by the windup). Leaving
    /// reach during the windup cancels it — that is the dodge window.
    /// </summary>
    public class EnemyAttackState : IState<EnemyController>
    {
        /// <summary>
        /// Hysteresis (m) beyond <see cref="EnemyDefinition.AttackRange"/> before the enemy gives up the
        /// stance and chases again; stops chase↔attack flicker at the range edge (each flicker restarted
        /// the windup, and reads as jitter).
        /// </summary>
        public const float RangeHysteresis = 0.2f;

        /// <summary>Seconds until the next strike (damage). The windup plays during its last <see cref="_windup"/> seconds.</summary>
        private float _timeToStrike;
        private float _windup;
        private bool _windingUp;

        public float TimeToStrike => _timeToStrike;
        public bool IsWindingUp => _windingUp;

        public void Enter(EnemyController context)
        {
            // Attack as soon as the telegraph allows on entering range, then respect the cooldown.
            _windup = context.ResolvedAttackWindup;
            _timeToStrike = _windup;
            _windingUp = false;
        }

        public void Tick(EnemyController context, float deltaTime)
        {
            EnemyDefinition definition = context.Definition;
            if (definition == null || context.Target == null)
            {
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            if (context.CanFlee && context.Health != null && EnemyDetectionUtility.ShouldFlee(context.Health.CurrentHealth, context.Health.MaxHealth, definition.FleeHealthThreshold))
            {
                context.PublishFleeing();
                context.StateMachine.ChangeState<EnemyFleeState>();
                return;
            }

            Vector3 selfPosition = context.transform.position;
            Vector3 targetPosition = context.Target.position;

            if (!EnemyDetectionUtility.IsWithinAttackRange(selfPosition, targetPosition, definition.AttackRange + RangeHysteresis))
            {
                context.StateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            context.StopMoving(deltaTime);
            context.FaceTarget(deltaTime);

            _timeToStrike -= deltaTime;
            if (!_windingUp && _timeToStrike <= _windup)
            {
                _windingUp = true;
                context.NotifyAttackWindup(Mathf.Max(0f, _timeToStrike));
            }

            if (_timeToStrike <= 0f)
            {
                _windingUp = false;
                context.NotifyAttackStrike();
                PerformAttack(context);
                _timeToStrike = Mathf.Max(definition.AttackCooldown, _windup);
            }
        }

        private static void PerformAttack(EnemyController context)
        {
            // AttackDamage debuffs (Provocação Heroica, Névoa da Ilusão, Purificação) weaken the swing.
            float damage = context.EffectiveAttackDamage;

            if (context.Hitbox != null)
            {
                context.Hitbox.Damage = damage;
                context.Hitbox.ResetHitTargets();
            }

            context.ApplyDamageToTarget(damage);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
            _windingUp = false;
            context.CancelAttackWindup();
        }
    }
}
