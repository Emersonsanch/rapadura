using System;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Content;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// A barred iron gate of the ossuary, locked with a Runic Bone Key (<see cref="DungeonRules.TryUnlockGate"/>): walking up
    /// to it with a key in the bag spends the key (when <see cref="_consumeKeyOnUnlock"/>), plays the rusty-gear cue and raises
    /// the bars smoothly over <see cref="_openDuration"/> (eased in Update: no coroutine, no allocation); without one it says
    /// what is missing. The open state lives in <see cref="DungeonProgress"/>, so a gate opened on one visit stays open on the
    /// next (until the Colosso falls and the ossuary re-seals). Solid while closed (its collider sits on the bars).
    /// </summary>
    [DisallowMultipleComponent]
    public class LockedDungeonGate : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _gateId = DungeonRules.GateGalleryId;
        [SerializeField] private string _requiredKeyItemId = DungeonRules.BoneKeyItemId;
        [SerializeField] private bool _consumeKeyOnUnlock = true;
        [SerializeField] private Transform _gateBarsTransform;
        [SerializeField] private float _openHeight = 3.5f;
        [SerializeField] private float _openDuration = 1.2f;
        [SerializeField] private Light _highlight;

        private readonly DungeonProximity _proximity = new DungeonProximity();
        private Vector3 _closedLocalPosition;
        private bool _barsCaptured;
        private float _openProgress;
        private bool _opening;
        private float _lastWarnAt = -10f;

        /// <summary>Explicit progress (tests); null = the shared one.</summary>
        public DungeonProgress Progress { get; set; }

        public string GateId => _gateId;
        public string RequiredKeyItemId => _requiredKeyItemId;
        public bool ConsumeKeyOnUnlock => _consumeKeyOnUnlock;
        public bool IsOpen => ResolveProgress().IsGateOpen(_gateId);

        /// <summary>0 = bars down, 1 = fully raised.</summary>
        public float OpenProgress => _openProgress;

        public bool CanInteract => !IsOpen;

        /// <summary>Raised once when this gate opens (not when it loads already open).</summary>
        public event Action<LockedDungeonGate> Opened;

        public void Configure(string gateId, Transform bars, Light highlight = null, string keyItemId = DungeonRules.BoneKeyItemId, bool consumeKey = true)
        {
            _gateId = gateId;
            _gateBarsTransform = bars;
            _highlight = highlight;
            _requiredKeyItemId = keyItemId;
            _consumeKeyOnUnlock = consumeKey;
            _barsCaptured = false;
            CaptureBars();
            if (IsOpen)
            {
                SnapOpen();
            }
        }

        private DungeonProgress ResolveProgress() => Progress ?? DungeonProgress.ResolveShared();

        private void CaptureBars()
        {
            if (_barsCaptured || _gateBarsTransform == null)
            {
                return;
            }

            _barsCaptured = true;
            _closedLocalPosition = _gateBarsTransform.localPosition;
        }

        /// <summary>
        /// Tries the gate with the keys in <paramref name="inventory"/>. Without a key nothing changes; with one the key is
        /// spent (if configured) and the gate opens; an open gate never asks again.
        /// </summary>
        public GateUnlockResult TryUnlock(InventoryManager inventory)
        {
            GateUnlockResult result = DungeonRules.TryUnlockGate(ResolveProgress(), _gateId, _requiredKeyItemId, _consumeKeyOnUnlock,
                id => inventory != null ? inventory.GetTotalCount(id) : 0,
                (id, amount) => inventory != null && inventory.RemoveById(id, amount));

            if (result == GateUnlockResult.Unlocked)
            {
                BeginOpening();
                DungeonFx.Play(DungeonFx.GateCue, transform.position);
                DungeonFx.Say(transform.position + Vector3.up * 3f, DungeonFx.Text(DungeonTexts.GateOpened));
                Opened?.Invoke(this);
            }

            return result;
        }

        public bool Interact(GameObject actor)
        {
            InventoryManager inventory = actor != null ? actor.GetComponentInParent<InventoryManager>() : null;
            GateUnlockResult result = TryUnlock(inventory);
            if (result == GateUnlockResult.NoKey && Time.time - _lastWarnAt > 2f)
            {
                _lastWarnAt = Time.time;
                string key = ItemDatabase.TryGetById(_requiredKeyItemId, out ItemDefinition item) && item != null
                    ? item.LocalizedName()
                    : DungeonFx.Text(DungeonTexts.ItemNameKey(_requiredKeyItemId));
                DungeonFx.Play(DungeonFx.LockedCue, transform.position);
                DungeonFx.Warn(transform.position + Vector3.up * 3f, DungeonFx.Format(DungeonTexts.GateNeedKey, key));
            }

            return result == GateUnlockResult.Unlocked;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (_highlight != null)
            {
                _highlight.intensity = highlighted ? 2.2f : 0.8f;
            }
        }

        private void BeginOpening()
        {
            CaptureBars();
            _opening = true;
            SetSolid(false);
        }

        /// <summary>Raised instantly (loading a gate that is already open).</summary>
        public void SnapOpen()
        {
            CaptureBars();
            _opening = false;
            _openProgress = 1f;
            ApplyBars();
            SetSolid(false);
            SetHighlighted(false);
        }

        private void SetSolid(bool solid)
        {
            // Only the bars' own box: the pillars (SolidPropBinder colliders) stay solid.
            Transform bars = _gateBarsTransform != null ? _gateBarsTransform : transform;
            foreach (Collider body in bars.GetComponentsInChildren<Collider>())
            {
                body.enabled = solid;
            }
        }

        private void Update()
        {
            _proximity.Tick(this, transform, Time.deltaTime);
            if (_opening)
            {
                StepOpening(Time.deltaTime);
            }
        }

        /// <summary>Advances the bars' rise (public for tests: EditMode has no Update).</summary>
        public void StepOpening(float deltaTime)
        {
            _openProgress = Mathf.Clamp01(_openProgress + deltaTime / Mathf.Max(0.01f, _openDuration));
            ApplyBars();
            if (_openProgress >= 1f)
            {
                _opening = false;
            }
        }

        private void ApplyBars()
        {
            if (_gateBarsTransform == null)
            {
                return;
            }

            float eased = Mathf.SmoothStep(0f, 1f, _openProgress);
            _gateBarsTransform.localPosition = _closedLocalPosition + Vector3.up * (_openHeight * eased);
        }
    }
}
