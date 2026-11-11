using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Mounts;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Keeps one <see cref="MapRequirementHeroState.Capture(InventoryManager)"/> per driver instead of one per frame: the state is
    /// captured again only after <see cref="RefreshSeconds"/>, when the bag changes, or when an inventory / mount / hero event
    /// marks it stale (<see cref="Attach"/> listens to them, <see cref="Detach"/> stops and drops the state). Callers only read
    /// the state; they must not change it.
    /// </summary>
    public sealed class HeroRequirementCache
    {
        /// <summary>Longest a state is kept without an event (the Costa already refreshed the boat at this rate).</summary>
        public const float RefreshSeconds = 1f;

        private MapRequirementState _state;
        private Func<string, bool> _lookup;
        private object _source;
        private float _capturedAt;
        private bool _stale = true;
        private bool _attached;

        /// <summary>How many captures were made (tests).</summary>
        public int Captures { get; private set; }

        /// <summary>The cached state for <paramref name="bag"/>, captured again only when due.</summary>
        public MapRequirementState Get(InventoryManager bag) => Get(Time.unscaledTime, bag, CaptureBag);

        private static readonly Func<InventoryManager, MapRequirementState> CaptureBag = MapRequirementHeroState.Capture;

        /// <summary>The same with the clock and the capture given (tests): captures when stale, the source changed or a second passed.</summary>
        public MapRequirementState Get<TSource>(float now, TSource source, Func<TSource, MapRequirementState> capture) where TSource : class
        {
            if (_state == null || _stale || !ReferenceEquals(source, _source) || now - _capturedAt >= RefreshSeconds || now < _capturedAt)
            {
                _state = capture(source);
                _lookup = null;
                _source = source;
                _capturedAt = now;
                _stale = false;
                Captures++;
            }

            return _state;
        }

        /// <summary>The <see cref="MapRequirementRules.Lookup"/> of the last state, built once per capture (call after Get).</summary>
        public Func<string, bool> Lookup => _lookup ??= MapRequirementRules.Lookup(_state);

        /// <summary>Marks the state stale: the next <see cref="Get(InventoryManager)"/> captures again.</summary>
        public void Invalidate() => _stale = true;

        /// <summary>Starts listening to inventory, mount and hero changes (call from OnEnable).</summary>
        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            _stale = true;
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<MountCollectionChangedEvent>(OnMountsChanged);
            EventBus.Subscribe<CharacterSelectedEvent>(OnHeroSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnHeroRestored);
        }

        /// <summary>Stops listening and drops the state (call from OnDisable).</summary>
        public void Detach()
        {
            _stale = true;
            _state = null;
            _lookup = null;
            _source = null;
            if (!_attached)
            {
                return;
            }

            _attached = false;
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<MountCollectionChangedEvent>(OnMountsChanged);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnHeroSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnHeroRestored);
        }

        private void OnInventoryChanged(InventoryChangedEvent _) => _stale = true;

        private void OnMountsChanged(MountCollectionChangedEvent _) => _stale = true;

        private void OnHeroSelected(CharacterSelectedEvent _) => _stale = true;

        private void OnHeroRestored(CharacterSelectionRestoredEvent _) => _stale = true;
    }
}
