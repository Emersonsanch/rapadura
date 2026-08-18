using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>Raised whenever a hurtbox/<see cref="ICombatTarget"/> takes damage, after mitigation is applied.</summary>
    public readonly struct DamageAppliedEvent : IGameEvent
    {
        public readonly GameObject Target;
        public readonly GameObject Source;
        public readonly float FinalDamage;
        public readonly ElementType Element;
        public readonly bool IsCritical;

        public DamageAppliedEvent(GameObject target, GameObject source, float finalDamage, ElementType element, bool isCritical)
        {
            Target = target;
            Source = source;
            FinalDamage = finalDamage;
            Element = element;
            IsCritical = isCritical;
        }
    }

    /// <summary>Raised once when a <see cref="Health"/> component's value reaches zero.</summary>
    public readonly struct CombatTargetDiedEvent : IGameEvent
    {
        public readonly GameObject Target;
        public readonly GameObject Killer;

        public CombatTargetDiedEvent(GameObject target, GameObject killer)
        {
            Target = target;
            Killer = killer;
        }
    }

    /// <summary>
    /// Generic request for a brief camera shake. Combat raises this instead of calling
    /// <c>PlayerCamera.Shake</c> directly, so hitboxes/health never need a reference to the
    /// camera — <see cref="CombatCameraShakeRelay"/> is what actually forwards it.
    /// </summary>
    public readonly struct CameraShakeRequestedEvent : IGameEvent
    {
        public readonly float Duration;
        public readonly float Magnitude;

        public CameraShakeRequestedEvent(float duration, float magnitude)
        {
            Duration = duration;
            Magnitude = magnitude;
        }
    }

    /// <summary>Request for a brief global time-freeze (hit-stop) on a significant impact.</summary>
    public readonly struct HitStopRequestedEvent : IGameEvent
    {
        public readonly float Duration;
        public readonly float TimeScale;

        public HitStopRequestedEvent(float duration, float timeScale)
        {
            Duration = duration;
            TimeScale = timeScale;
        }
    }
}
