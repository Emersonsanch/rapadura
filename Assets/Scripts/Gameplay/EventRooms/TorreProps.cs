using System;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>
    /// The Portão da Torre of the Torre da Roda das Doze Jarras: an <see cref="ICombatTarget"/> with a solid collider and HP, so
    /// the existing combat loop breaks it (tap → walk over → auto-attack), like <c>ArenaSecretObject</c>. Only the
    /// hero's hits count, and only while <see cref="AcceptsDamage"/> (phase "Quebrar o portão"). Not an
    /// EnemyController: skills' soft-targeting and the kill count ignore it.
    /// </summary>
    public class EventRoomGate : MonoBehaviour, ICombatTarget
    {
        private const float PunchSeconds = 0.18f;
        private const float BreakSeconds = 0.6f;

        private float _breakTimer = -1f;
        private float _punch;
        private Vector3 _baseScale = Vector3.one;

        public float MaxHealth { get; private set; } = 1f;
        public float CurrentHealth { get; private set; } = 1f;
        public bool IsBroken { get; private set; }
        public bool AcceptsDamage { get; set; }
        public float Fraction => MaxHealth > 0f ? Mathf.Clamp01(CurrentHealth / MaxHealth) : 0f;

        /// <summary>Raised once, when the gate breaks.</summary>
        public event Action Broken;

        public bool IsAlive => !IsBroken;
        public Transform Transform => transform;

        public void Configure(float maxHealth)
        {
            MaxHealth = Mathf.Max(1f, maxHealth);
            CurrentHealth = MaxHealth;
            _baseScale = transform.localScale;
        }

        public void ApplyDamage(float amount, ElementType element, GameObject source)
        {
            if (IsBroken || !AcceptsDamage || amount <= 0f || (source != null && source.GetComponentInParent<EnemyController>() != null))
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            _punch = PunchSeconds;
            if (Application.isPlaying)
            {
                CombatFxManager.SpawnLootText(transform.position + Vector3.up * 2.6f, Mathf.CeilToInt(amount).ToString());
            }

            if (CurrentHealth <= 0f)
            {
                Break();
            }
        }

        public void ApplyHealing(float amount, GameObject source)
        {
        }

        private void Break()
        {
            IsBroken = true;
            _breakTimer = BreakSeconds;
            foreach (Collider body in GetComponentsInChildren<Collider>())
            {
                body.enabled = false;
            }

            Broken?.Invoke();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_breakTimer >= 0f)
            {
                _breakTimer -= dt;
                float t = Mathf.Clamp01(1f - _breakTimer / BreakSeconds);
                transform.localScale = Vector3.Lerp(_baseScale, new Vector3(_baseScale.x * 1.2f, 0.01f, _baseScale.z * 1.2f), t * t);
                if (_breakTimer <= 0f)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            if (_punch > 0f)
            {
                _punch -= dt;
                float k = Mathf.Sin(Mathf.Clamp01(_punch / PunchSeconds) * Mathf.PI);
                transform.localScale = new Vector3(_baseScale.x * (1f + 0.06f * k), _baseScale.y * (1f - 0.05f * k), _baseScale.z);
                if (_punch <= 0f)
                {
                    transform.localScale = _baseScale;
                }
            }
        }
    }

    /// <summary>Slow spin + bob for the Jarra de Ouro while it waits on the floor.</summary>
    public class EventRoomSpinner : MonoBehaviour
    {
        private Vector3 _base;

        private void Start() => _base = transform.localPosition;

        private void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            transform.localPosition = _base + Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.12f);
        }
    }

    /// <summary>
    /// Builds the Torre da Roda das Doze Jarras's props from primitives (no assets, no scene edit), laid out on a line from
    /// the hero toward the arena centre: Adda (the tower keeper) at the entrance, the bridge barrier (sharp stakes that sink when
    /// the bridge opens), the tower gate and, after Ullikummi, the Jarra de Ouro (prop names keep the old ids). Decorative parts have no collider;
    /// the gate has one solid box.
    /// </summary>
    public static class TorreProps
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static readonly Color Wood = new Color(0.45f, 0.29f, 0.16f);
        public static readonly Color DarkWood = new Color(0.3f, 0.18f, 0.1f);
        public static readonly Color Iron = new Color(0.32f, 0.32f, 0.36f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.25f);

        /// <summary>Distances (m) along the entrance → tower line.</summary>
        public const float BridgeDistance = 6f;
        public const float GateDistance = 10f;
        public const float GuardianDistance = 14f;

        public static GameObject BuildKeeper(Vector3 position, Vector3 lookAt)
        {
            GameObject root = Root("SeuMalaquias", position, lookAt);
            var block = new MaterialPropertyBlock();
            // Long robe in tower-keeper indigo with a pale sash.
            Part(root.transform, PrimitiveType.Capsule, new Vector3(0f, 0.85f, 0f), Vector3.zero, new Vector3(0.6f, 0.85f, 0.6f), new Color(0.24f, 0.25f, 0.42f), block);
            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.05f, 0f), Vector3.zero, new Vector3(0.64f, 0.05f, 0.64f), new Color(0.95f, 0.93f, 0.88f), block);
            Part(root.transform, PrimitiveType.Sphere, new Vector3(0f, 1.85f, 0f), Vector3.zero, Vector3.one * 0.42f, new Color(0.62f, 0.45f, 0.32f), block);
            // Tall white felt cap (no brim).
            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 2.12f, 0f), Vector3.zero, new Vector3(0.36f, 0.16f, 0.36f), new Color(0.94f, 0.92f, 0.86f), block);
            Part(root.transform, PrimitiveType.Sphere, new Vector3(0f, 2.28f, 0f), Vector3.zero, new Vector3(0.34f, 0.16f, 0.34f), new Color(0.94f, 0.92f, 0.86f), block);
            // Lantern on a stick, so the hero sees where to bring the Golden Jar.
            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0.5f, 1.2f, 0f), Vector3.zero, new Vector3(0.05f, 1.2f, 0.05f), DarkWood, block);
            Part(root.transform, PrimitiveType.Cube, new Vector3(0.5f, 2.45f, 0f), Vector3.zero, Vector3.one * 0.22f, new Color(1f, 0.85f, 0.45f), block);
            var light = new GameObject("Lantern").AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.localPosition = new Vector3(0.5f, 2.45f, 0f);
            light.type = LightType.Point;
            light.range = 6f;
            light.intensity = 1.6f;
            light.color = new Color(1f, 0.8f, 0.45f);
            light.shadows = LightShadows.None;
            return root;
        }

        /// <summary>A row of sharp stakes across the path. Sink it with <see cref="SinkBarrier"/> when the bridge opens.</summary>
        public static GameObject BuildBarrier(Vector3 position, Vector3 lookAt)
        {
            GameObject root = Root("PonteFechada", position, lookAt);
            var block = new MaterialPropertyBlock();
            for (int i = -3; i <= 3; i++)
            {
                float x = i * 0.7f;
                Part(root.transform, PrimitiveType.Cylinder, new Vector3(x, 0.7f, 0f), new Vector3(0f, 0f, i * 4f), new Vector3(0.18f, 0.7f, 0.18f), Wood, block);
                Part(root.transform, PrimitiveType.Cube, new Vector3(x, 1.5f, 0f), new Vector3(45f, 0f, 45f), Vector3.one * 0.18f, DarkWood, block);
            }

            Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.9f, 0.05f), Vector3.zero, new Vector3(4.6f, 0.12f, 0.12f), DarkWood, block);
            return root;
        }

        public static void SinkBarrier(GameObject barrier)
        {
            if (barrier != null)
            {
                barrier.SetActive(false);
            }
        }

        public static EventRoomGate BuildGate(Vector3 position, Vector3 lookAt, float health)
        {
            GameObject root = Root("PortaoDoEngenho", position, lookAt);
            var block = new MaterialPropertyBlock();
            // Two posts, planks and iron bands.
            Part(root.transform, PrimitiveType.Cube, new Vector3(-1.7f, 1.4f, 0f), Vector3.zero, new Vector3(0.45f, 2.8f, 0.45f), DarkWood, block);
            Part(root.transform, PrimitiveType.Cube, new Vector3(1.7f, 1.4f, 0f), Vector3.zero, new Vector3(0.45f, 2.8f, 0.45f), DarkWood, block);
            for (int i = 0; i < 5; i++)
            {
                Part(root.transform, PrimitiveType.Cube, new Vector3(-1.2f + i * 0.6f, 1.15f, 0f), Vector3.zero, new Vector3(0.55f, 2.3f, 0.2f), Wood, block);
            }

            Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.6f, -0.12f), Vector3.zero, new Vector3(3f, 0.15f, 0.05f), Iron, block);
            Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 1.8f, -0.12f), Vector3.zero, new Vector3(3f, 0.15f, 0.05f), Iron, block);

            var body = root.AddComponent<BoxCollider>();
            body.center = new Vector3(0f, 1.15f, 0f);
            body.size = new Vector3(3.6f, 2.3f, 0.5f);

            var gate = root.AddComponent<EventRoomGate>();
            gate.Configure(health);
            return gate;
        }

        public static GameObject BuildMoenda(Vector3 position)
        {
            var root = new GameObject("MoendaDeOuro");
            root.transform.position = position;
            var block = new MaterialPropertyBlock();
            var spin = new GameObject("Spin");
            spin.transform.SetParent(root.transform, false);
            spin.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            BuildMoendaModel(spin.transform, block);
            spin.AddComponent<EventRoomSpinner>();

            var light = new GameObject("Glow").AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.localPosition = Vector3.up;
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 2f;
            light.color = Gold;
            light.shadows = LightShadows.None;
            return root;
        }

        /// <summary>The small Golden Jar carried over the hero's head (no light, no spin).</summary>
        public static GameObject BuildCarried(Transform hero)
        {
            var root = new GameObject("MoendaCarregada");
            root.transform.SetParent(hero, false);
            root.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            root.transform.localScale = Vector3.one * 0.6f;
            BuildMoendaModel(root.transform, new MaterialPropertyBlock());
            return root;
        }

        private static void BuildMoendaModel(Transform parent, MaterialPropertyBlock block)
        {
            // The Golden Jar (the twelfth jar of the wheel): foot, round belly, neck and lip, all in gold.
            Part(parent, PrimitiveType.Cylinder, new Vector3(0f, -0.36f, 0f), Vector3.zero, new Vector3(0.34f, 0.05f, 0.34f), Gold * 0.85f, block);
            Part(parent, PrimitiveType.Sphere, new Vector3(0f, -0.06f, 0f), Vector3.zero, new Vector3(0.62f, 0.56f, 0.62f), Gold, block);
            Part(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.26f, 0f), Vector3.zero, new Vector3(0.24f, 0.1f, 0.24f), Gold * 0.9f, block);
            Part(parent, PrimitiveType.Cylinder, new Vector3(0f, 0.37f, 0f), Vector3.zero, new Vector3(0.34f, 0.03f, 0.34f), Gold * 0.85f, block);
        }

        private static GameObject Root(string name, Vector3 position, Vector3 lookAt)
        {
            var root = new GameObject(name);
            Vector3 to = lookAt - position;
            to.y = 0f;
            root.transform.SetPositionAndRotation(position, to.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(to) : Quaternion.identity);
            return root;
        }

        private static GameObject Part(Transform parent, PrimitiveType type, Vector3 position, Vector3 euler, Vector3 scale, Color color, MaterialPropertyBlock block)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = type.ToString();
            Transform t = part.transform;
            t.SetParent(parent, false);
            t.localPosition = position;
            t.localRotation = Quaternion.Euler(euler);
            t.localScale = scale;

            if (part.TryGetComponent(out Collider collider))
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(collider);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            Renderer renderer = part.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
            return part;
        }
    }
}
