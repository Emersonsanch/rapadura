using System;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.UI.CharacterSelect
{
    /// <summary>Maps one playable character to the prefab (and local placement) shown on the preview turntable.</summary>
    [Serializable]
    public class CharacterPreviewEntry
    {
        public CharacterId characterId;
        public GameObject modelPrefab;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = new Vector3(270f, 180f, 0f);
        public float uniformScale = 1f;
    }

    /// <summary>
    /// Drives a small isolated turntable stage — its own spawn point, rendered by a dedicated
    /// preview <see cref="Camera"/> into a <see cref="RenderTexture"/> — so the character select
    /// screen can show a real spinning 3D model of the hovered hero instead of just a name label.
    /// The stage lives far from actual gameplay geometry (see the spawn point's scene position)
    /// so the preview camera's culling mask never needs to worry about the real scene.
    /// Swapping models is just Instantiate/Destroy under <see cref="_spawnPoint"/> — cheap enough
    /// to do on every roster hover.
    /// </summary>
    public class CharacterPreviewController : MonoBehaviour
    {
        [Tooltip("One entry per playable character, mapping CharacterId to its preview model prefab and local placement on the turntable.")]
        [SerializeField] private CharacterPreviewEntry[] _entries = Array.Empty<CharacterPreviewEntry>();
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _rotationSpeedDegPerSec = 25f;

        private GameObject _currentInstance;
        private CharacterId? _currentId;

        private void Update()
        {
            if (_currentInstance != null)
            {
                // Unscaled: the select screen pauses gameplay (timeScale 0) while this turntable spins.
                _currentInstance.transform.Rotate(Vector3.up, _rotationSpeedDegPerSec * Time.unscaledDeltaTime, Space.World);
            }
        }

        /// <summary>Swaps the turntable to show the given character's model. No-op if it's already showing.</summary>
        public void ShowCharacter(CharacterId id)
        {
            if (_currentId == id && _currentInstance != null)
            {
                return;
            }

            ClearInstance();

            CharacterPreviewEntry entry = Array.Find(_entries, e => e.characterId == id);

            if (entry == null || entry.modelPrefab == null || _spawnPoint == null)
            {
                _currentId = null;
                return;
            }

            _currentInstance = Instantiate(entry.modelPrefab, _spawnPoint);
            _currentInstance.transform.localPosition = entry.localPosition;
            _currentInstance.transform.localEulerAngles = entry.localEulerAngles;
            _currentInstance.transform.localScale = Vector3.one * entry.uniformScale;
            PedestalMeshUtility.ApplyIfAvailable(_currentInstance, id);
            _currentId = id;
        }

        public void Clear()
        {
            ClearInstance();
            _currentId = null;
        }

        private void ClearInstance()
        {
            if (_currentInstance != null)
            {
                Destroy(_currentInstance);
                _currentInstance = null;
            }
        }
    }
}
