using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Drives one spawned damage-number <see cref="TextMesh"/>: floats upward, fades out, faces the
    /// main camera each frame (billboarded — a <see cref="TextMesh"/> has no built-in billboarding),
    /// then destroys itself. Self-contained so <see cref="CombatFxManager"/> only needs to
    /// instantiate and forget.
    /// </summary>
    public class FloatingDamageNumber : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float RiseSpeed = 1.3f;

        private float _age;
        private TextMesh _textMesh;
        private Color _startColor;
        private Camera _camera;

        private void Awake()
        {
            _textMesh = GetComponent<TextMesh>();
            _startColor = _textMesh != null ? _textMesh.color : Color.white;
            _camera = Camera.main;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += Vector3.up * RiseSpeed * Time.deltaTime;

            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }

            if (_textMesh != null)
            {
                float t = Mathf.Clamp01(_age / Lifetime);
                Color c = _startColor;
                c.a = 1f - t;
                _textMesh.color = c;
            }

            if (_age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
