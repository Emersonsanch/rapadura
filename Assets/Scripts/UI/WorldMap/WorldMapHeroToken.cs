using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.UI.CharacterSelect;
using UnityEngine;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Shows the selected hero's model on the world-map player token instead of the bare capsule
    /// (which was tiny and hidden under the region label). Reuses the character-select preview
    /// entries (<see cref="CharacterPreviewController"/>) so every hero gets the same model, pedestal
    /// clean-up and framing as on the select screen. Falls back to the capsule when no hero is
    /// selected or no preview entry exists. Adds a small hop while the token travels.
    /// </summary>
    public class WorldMapHeroToken : MonoBehaviour
    {
        [Tooltip("Model height on the map, in world units (region nodes are ~2 units wide).")]
        [SerializeField] private float _heroHeight = 3.6f;
        [Tooltip("Max footprint diagonal, keeps wide poses (shields, bows) from covering neighbours.")]
        [SerializeField] private float _heroMaxWidth = 3.2f;
        [Tooltip("Distance from the token's pivot down to the map ground (WorldMapController token height offset).")]
        [SerializeField] private float _groundOffset = 0.5f;
        [SerializeField] private float _hopHeight = 0.12f;
        [SerializeField] private float _hopFrequency = 3.5f;

        private Renderer[] _capsuleRenderers;
        private GameObject _model;
        private CharacterId? _shownId;
        private float _modelBaseY;
        private Vector3 _lastPosition;
        private float _hopPhase;

        private void Awake()
        {
            _capsuleRenderers = GetComponents<Renderer>();
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            Refresh();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
        }

        private void Start() => Refresh();

        private void OnCharacterSelected(CharacterSelectedEvent _) => Refresh();

        private void OnCharacterRestored(CharacterSelectionRestoredEvent _) => Refresh();

        private void Refresh()
        {
            CharacterId? id = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            if (id == _shownId && (_model != null || id == null))
            {
                return;
            }

            if (_model != null)
            {
                Destroy(_model);
                _model = null;
            }

            _shownId = id;
            bool shown = id.HasValue && SpawnModel(id.Value);
            SetCapsuleVisible(!shown);
        }

        private bool SpawnModel(CharacterId id)
        {
            CharacterPreviewController preview = FindFirstObjectByType<CharacterPreviewController>(FindObjectsInactive.Include);
            if (preview == null || !preview.TryGetEntry(id, out CharacterPreviewEntry entry))
            {
                return false;
            }

            _model = Instantiate(entry.modelPrefab, transform);
            _model.name = "HeroToken_" + id;
            _model.transform.localEulerAngles = entry.localEulerAngles;
            PedestalMeshUtility.ApplyIfAvailable(_model, id);

            // Frame in world space: the token itself is scaled (capsule), so size by bounds.
            if (!CharacterPreviewController.TryGetModelBounds(_model.transform, out Bounds bounds) || bounds.size.y <= 0.0001f)
            {
                Destroy(_model);
                _model = null;
                return false;
            }

            float horizontal = Mathf.Sqrt(bounds.size.x * bounds.size.x + bounds.size.z * bounds.size.z);
            float factor = _heroHeight / bounds.size.y;
            if (horizontal > 0.0001f)
            {
                factor = Mathf.Min(factor, _heroMaxWidth / horizontal);
            }

            _model.transform.localScale *= factor;

            if (CharacterPreviewController.TryGetModelBounds(_model.transform, out bounds))
            {
                // Feet on the ground, centred on the token.
                Vector3 ground = transform.position + Vector3.down * _groundOffset;
                _model.transform.position += new Vector3(ground.x - bounds.center.x, ground.y - bounds.min.y, ground.z - bounds.center.z);
            }

            _modelBaseY = _model.transform.localPosition.y;
            foreach (Collider c in _model.GetComponentsInChildren<Collider>())
            {
                c.enabled = false; // never block map clicks
            }

            return true;
        }

        private void SetCapsuleVisible(bool visible)
        {
            if (_capsuleRenderers == null)
            {
                return;
            }

            foreach (Renderer r in _capsuleRenderers)
            {
                if (r != null) r.enabled = visible;
            }
        }

        private void LateUpdate()
        {
            if (_model == null)
            {
                return;
            }

            Vector3 position = transform.position;
            float moved = new Vector2(position.x - _lastPosition.x, position.z - _lastPosition.z).magnitude;
            _lastPosition = position;

            Vector3 local = _model.transform.localPosition;
            if (moved > 0.0001f)
            {
                _hopPhase += Time.deltaTime * _hopFrequency * Mathf.PI * 2f;
                float hop = Mathf.Abs(Mathf.Sin(_hopPhase)) * _hopHeight;
                // Local units are divided by the token's scale.
                local.y = _modelBaseY + hop / Mathf.Max(0.0001f, transform.lossyScale.y);
            }
            else
            {
                _hopPhase = 0f;
                local.y = Mathf.MoveTowards(local.y, _modelBaseY, Time.deltaTime * 2f);
            }

            _model.transform.localPosition = local;
        }
    }
}
