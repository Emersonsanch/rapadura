using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// When the hero last dealt or took damage (<see cref="DamageAppliedEvent"/>), for the mechanics that only work out of a fight (the lit
    /// canals of map 06) or that hear the fight (the silence of map 56). Not a MonoBehaviour: a driver owns one, calls <see cref="Attach"/> from
    /// OnEnable and <see cref="Detach"/> from OnDisable.
    /// </summary>
    public sealed class HeroCombatClock
    {
        private readonly Action<DamageAppliedEvent> _handler;
        private GameObject _hero;
        private bool _attached;

        public HeroCombatClock() => _handler = OnDamage;

        /// <summary>Time.time of the hero's last hit given or taken (−∞ = none since Attach).</summary>
        public float LastCombat { get; private set; } = float.NegativeInfinity;

        /// <summary>Hits given or taken since the counter was last read with <see cref="TakeHits"/>.</summary>
        public int PendingHits { get; private set; }

        public float SecondsSinceCombat => Time.time - LastCombat;

        /// <summary>Which GameObject is the hero (the drivers find it after OnEnable).</summary>
        public void Track(GameObject hero) => _hero = hero;

        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            EventBus.Subscribe(_handler);
        }

        public void Detach()
        {
            if (!_attached)
            {
                return;
            }

            _attached = false;
            EventBus.Unsubscribe(_handler);
            PendingHits = 0;
        }

        /// <summary>The hits since the last call, and the counter back to 0.</summary>
        public int TakeHits()
        {
            int hits = PendingHits;
            PendingHits = 0;
            return hits;
        }

        private void OnDamage(DamageAppliedEvent evt)
        {
            if (_hero == null || (evt.Target != _hero && evt.Source != _hero))
            {
                return;
            }

            LastCombat = Time.time;
            PendingHits++;
        }
    }
}
