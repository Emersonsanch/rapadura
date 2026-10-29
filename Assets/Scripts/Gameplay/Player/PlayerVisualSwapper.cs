using System;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>Maps a playable character to the rigged model (and per-character placement/animator) used as the live Player visual.</summary>
    [Serializable]
    public class PlayerVisualEntry
    {
        public CharacterId characterId;
        public GameObject modelPrefab;
        public RuntimeAnimatorController animatorController;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        public float uniformScale = 1f;
        [Tooltip("Optional authored motion profile; empty = the hero's code defaults (HeroMotionProfile.For).")]
        public HeroMotionProfileAsset motionProfile;
    }

    /// <summary>
    /// Swaps the Player's visual model to match the hero chosen on the character select screen.
    /// Until this existed, Joaquim's rigged model was hardcoded as the only playable visual — any
    /// other hero selected on <see cref="Rapadura.UI.CharacterSelect.CharacterSelectController"/>
    /// still played as Joaquim's body. Subscribes to <c>OnCharacterSelected</c> (wire in the
    /// Inspector) or call <see cref="SwapTo"/> directly.
    /// </summary>
    public class PlayerVisualSwapper : MonoBehaviour
    {
        [SerializeField] private PlayerVisualEntry[] _entries = Array.Empty<PlayerVisualEntry>();
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private PlayerAnimatorSpeedDriver _speedDriver;
        [SerializeField] private PlayerProceduralAnimator _proceduralAnimator;

        private GameObject _currentInstance;

        public void SwapTo(CharacterId id)
        {
            PlayerVisualEntry entry = Array.Find(_entries, e => e.characterId == id);
            if (entry == null || entry.modelPrefab == null || _visualRoot == null)
            {
                return;
            }

            // Remove every existing child, not just one this swapper itself instantiated — the
            // very first swap needs to remove the hand-placed original model (e.g. Joaquim's rig,
            // wired directly in the scene before this component existed) too. Deactivate AND
            // reparent out before Destroy(): Destroy() only marks the object for removal at the
            // end of the frame, so simply calling it left the old model both visible and
            // overlapping the new one for that whole frame (looked like the swap silently failed).
            for (int i = _visualRoot.childCount - 1; i >= 0; i--)
            {
                GameObject old = _visualRoot.GetChild(i).gameObject;
                old.SetActive(false);
                old.transform.SetParent(null);
                Destroy(old);
            }
            _currentInstance = null;

            _currentInstance = Instantiate(entry.modelPrefab, _visualRoot);
            _currentInstance.transform.localPosition = entry.localPosition;
            _currentInstance.transform.localEulerAngles = entry.localEulerAngles;
            _currentInstance.transform.localScale = Vector3.one * entry.uniformScale;
            Characters.PedestalMeshUtility.ApplyIfAvailable(_currentInstance, id);

            var animator = _currentInstance.GetComponentInChildren<Animator>();
            if (animator != null && entry.animatorController != null)
            {
                animator.runtimeAnimatorController = entry.animatorController;
            }

            if (_speedDriver != null)
            {
                _speedDriver.SetAnimator(animator);
            }

            PlayerActionAnimator actions = _speedDriver != null ? _speedDriver.GetComponent<PlayerActionAnimator>() : GetComponentInParent<PlayerActionAnimator>();
            if (actions != null)
            {
                actions.SetAnimator(animator);
            }

            if (_proceduralAnimator != null)
            {
                _proceduralAnimator.SetMotionProfile(entry.motionProfile != null && entry.motionProfile.profile != null
                    ? entry.motionProfile.profile
                    : HeroMotionProfile.For(id));
                _proceduralAnimator.SetVisualRoot(_currentInstance.transform);
            }
        }
    }
}
