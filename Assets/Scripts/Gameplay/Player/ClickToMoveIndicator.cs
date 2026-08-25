using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Ground marker shown at the click-to-move destination — previously a click/tap did nothing
    /// visible on the ground, so it wasn't clear the input registered or where the character was
    /// actually headed (see TODO.md Fase 2/UX note). Builds its own flat ring mesh procedurally
    /// (no imported model needed) and pulses/fades it in place, matching the classic MU
    /// Online-style click-to-move marker.
    /// </summary>
    [RequireComponent(typeof(PlayerClickToMove))]
    public class ClickToMoveIndicator : MonoBehaviour
    {
        [SerializeField] private float _ringRadius = 0.3f;
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private Color _ringColor = new Color(1f, 0.85f, 0.3f, 0.9f);
        [SerializeField] private float _groundYOffset = 0.03f;

        private PlayerClickToMove _clickToMove;
        private Transform _ringTransform;
        private MeshRenderer _ringRenderer;
        private float _pulseTimeRemaining;

        private void Awake()
        {
            _clickToMove = GetComponent<PlayerClickToMove>();
            BuildRing();
        }

        private void OnEnable()
        {
            _clickToMove.DestinationSet += OnDestinationSet;
            _clickToMove.DestinationCleared += OnDestinationCleared;
        }

        private void OnDisable()
        {
            _clickToMove.DestinationSet -= OnDestinationSet;
            _clickToMove.DestinationCleared -= OnDestinationCleared;
        }

        private void Update()
        {
            if (_pulseTimeRemaining <= 0f)
            {
                return;
            }

            _pulseTimeRemaining -= Time.deltaTime;
            float t = Mathf.Clamp01(_pulseTimeRemaining / _pulseDuration);

            // Starts slightly oversized and shrinks to normal size while fading out — the classic
            // "target acquired" pulse feel, cheap to fake with just scale + alpha over time.
            float scale = Mathf.Lerp(1f, 1.35f, t);
            _ringTransform.localScale = new Vector3(scale, 1f, scale);

            Color color = _ringColor;
            color.a = _ringColor.a * t;
            _ringRenderer.material.color = color;

            if (_pulseTimeRemaining <= 0f)
            {
                _ringTransform.gameObject.SetActive(false);
            }
        }

        private void OnDestinationSet(Vector3 worldPoint)
        {
            _ringTransform.position = worldPoint + Vector3.up * _groundYOffset;
            _ringTransform.gameObject.SetActive(true);
            _pulseTimeRemaining = _pulseDuration;
        }

        private void OnDestinationCleared()
        {
            _pulseTimeRemaining = 0f;
            _ringTransform.gameObject.SetActive(false);
        }

        /// <summary>Builds a simple flat ring (annulus) mesh at runtime — no imported asset needed.</summary>
        private void BuildRing()
        {
            var ringGo = new GameObject("ClickToMoveRing");
            ringGo.transform.SetParent(null);
            _ringTransform = ringGo.transform;

            var meshFilter = ringGo.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = BuildRingMesh(_ringRadius, _ringRadius * 0.7f, 24);

            _ringRenderer = ringGo.AddComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = _ringColor;
            material.SetFloat("_Surface", 1f); // Transparent
            material.SetFloat("_Blend", 0f); // Alpha blend
            material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off); // Ring lies flat on the ground — visible from above regardless of the mesh's winding order.
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_ZWrite", 0);
            _ringRenderer.sharedMaterial = material;

            ringGo.SetActive(false);
        }

        private static Mesh BuildRingMesh(float outerRadius, float innerRadius, int segments)
        {
            var vertices = new Vector3[segments * 2];
            var triangles = new int[segments * 6];

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
                vertices[i * 2 + 1] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);

                int next = (i + 1) % segments;
                int triIndex = i * 6;

                triangles[triIndex] = i * 2;
                triangles[triIndex + 1] = next * 2;
                triangles[triIndex + 2] = i * 2 + 1;

                triangles[triIndex + 3] = next * 2;
                triangles[triIndex + 4] = next * 2 + 1;
                triangles[triIndex + 5] = i * 2 + 1;
            }

            var mesh = new Mesh { name = "ClickToMoveRing" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
