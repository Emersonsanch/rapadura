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
    /// <para>
    /// Auto-framing: the imported heroes come from different pipelines (static Meshy FBX vs rigged
    /// FBX) with wildly different pivots and scales, so after spawning, the model is uniformly
    /// rescaled to fit <see cref="_frameHeight"/>/<see cref="_frameWidth"/> and recentred on the spawn
    /// point by its renderer bounds. The model spins inside a pivot sitting on the spawn point, so an
    /// off-centre mesh pivot can no longer swing the hero out of the camera's view.
    /// </para>
    /// </summary>
    public class CharacterPreviewController : MonoBehaviour
    {
        [Tooltip("One entry per playable character, mapping CharacterId to its preview model prefab and local placement on the turntable.")]
        [SerializeField] private CharacterPreviewEntry[] _entries = Array.Empty<CharacterPreviewEntry>();
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _rotationSpeedDegPerSec = 25f;

        [Header("Auto-framing")]
        [Tooltip("Rescale and recentre each model by its renderer bounds so every hero fills the preview the same way.")]
        [SerializeField] private bool _autoFrame = true;
        [Tooltip("Target model height (world units) after auto-framing.")]
        [SerializeField] private float _frameHeight = 1.4f;
        [Tooltip("Max horizontal footprint diagonal (world units) — keeps wide poses (bows, shields) inside the frame while spinning.")]
        [SerializeField] private float _frameWidth = 1.6f;
        [Tooltip("Where the framed model's bounds centre lands, relative to the spawn point. Matches the preview camera's height so heroes sit in the middle of the shot.")]
        [SerializeField] private Vector3 _frameCenterOffset = new Vector3(0f, 0.1f, 0f);

        private Transform _pivot;
        private GameObject _currentInstance;
        private CharacterId? _currentId;

        private void Update()
        {
            if (_currentInstance != null && _pivot != null)
            {
                // Unscaled: the select screen pauses gameplay (timeScale 0) while this turntable spins.
                _pivot.Rotate(Vector3.up, _rotationSpeedDegPerSec * Time.unscaledDeltaTime, Space.World);
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

            Transform pivot = EnsurePivot();
            pivot.localRotation = Quaternion.identity;

            _currentInstance = Instantiate(entry.modelPrefab, pivot);
            _currentInstance.transform.localPosition = entry.localPosition;
            _currentInstance.transform.localEulerAngles = entry.localEulerAngles;
            _currentInstance.transform.localScale = Vector3.one * entry.uniformScale;
            PedestalMeshUtility.ApplyIfAvailable(_currentInstance, id);

            if (_autoFrame)
            {
                FrameInstance(_currentInstance.transform, pivot);
            }

            _currentId = id;
        }

        public void Clear()
        {
            ClearInstance();
            _currentId = null;
        }

        /// <summary>The preview entry (model prefab + placement) for <paramref name="id"/> — reused by the world-map hero token.</summary>
        public bool TryGetEntry(CharacterId id, out CharacterPreviewEntry entry)
        {
            entry = Array.Find(_entries, e => e.characterId == id);
            return entry != null && entry.modelPrefab != null;
        }

        /// <summary>World-space bounds of <paramref name="root"/>'s renderers, using rigged meshes' real bind-pose bounds (see <see cref="TryGetBounds"/>).</summary>
        public static bool TryGetModelBounds(Transform root, out Bounds bounds) => TryGetBounds(root, out bounds);

        private Transform EnsurePivot()
        {
            if (_pivot == null)
            {
                var go = new GameObject("TurntablePivot");
                _pivot = go.transform;
                _pivot.SetParent(_spawnPoint, false);
            }

            return _pivot;
        }

        private void FrameInstance(Transform model, Transform pivot)
        {
            if (!TryGetBounds(model, out Bounds bounds) || bounds.size.y <= 0.0001f)
            {
                return;
            }

            float horizontal = Mathf.Sqrt(bounds.size.x * bounds.size.x + bounds.size.z * bounds.size.z);
            float factor = _frameHeight / bounds.size.y;
            if (horizontal > 0.0001f)
            {
                factor = Mathf.Min(factor, _frameWidth / horizontal);
            }

            model.localScale *= factor;

            if (!TryGetBounds(model, out bounds))
            {
                return;
            }

            // Recentre: bounds centre goes onto the pivot (the spawn point), so the spin axis passes
            // through the middle of the hero regardless of where the mesh's own pivot sits.
            model.position += pivot.position + _frameCenterOffset - bounds.center;
        }

        private static bool TryGetBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            bool found = false;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                Bounds rb = r.bounds;
                // SkinnedMeshRenderer.bounds is a padded, animation-safe box (much bigger than the
                // rigged heroes' bind pose), which made them frame tiny and off-centre. Use the
                // shared mesh's own bounds in the renderer's space instead.
                if (r is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                {
                    rb = TransformBounds(smr.transform.localToWorldMatrix, smr.sharedMesh.bounds);
                }

                if (!found) { bounds = rb; found = true; }
                else bounds.Encapsulate(rb);
            }

            return found;
        }

        private static Bounds TransformBounds(Matrix4x4 m, Bounds local)
        {
            Vector3 c = local.center;
            Vector3 e = local.extents;
            var result = new Bounds(m.MultiplyPoint3x4(c + new Vector3(-e.x, -e.y, -e.z)), Vector3.zero);
            for (int i = 1; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z);
                result.Encapsulate(m.MultiplyPoint3x4(c + corner));
            }

            return result;
        }

        private void ClearInstance()
        {
            if (_currentInstance != null)
            {
                // Hide first: Destroy is deferred to end of frame, and two heroes overlapping on the
                // turntable for a frame shows up in the RenderTexture as a flicker.
                _currentInstance.SetActive(false);
                Destroy(_currentInstance);
                _currentInstance = null;
            }
        }
    }
}
