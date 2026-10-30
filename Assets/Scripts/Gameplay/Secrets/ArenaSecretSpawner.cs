using System.Collections;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// "Occasionally a breakable secret in the arena": after every scene load that starts an encounter
    /// (<see cref="EncounterArenaController.IsEncounterActive"/>), rolls <see cref="SecretRules.ShouldSpawnArenaSecret"/>
    /// from the payload seed (same encounter = same answer) and, on a hit, places an <see cref="ArenaSecretObject"/>
    /// a few metres beside / behind the hero, on open ground (raycast down, clear of the arena props), so it is in
    /// sight from the first second. Boss challenges never get one. Built from primitives coloured through a
    /// MaterialPropertyBlock (no material created, same as BiomeArenaPropFactory). Self-bootstrapping
    /// DontDestroyOnLoad host; the secret itself dies with the arena scene.
    /// </summary>
    public class ArenaSecretSpawner : MonoBehaviour
    {
        private const string LogCategory = "Secrets";
        private const string PlayerTag = "Player";
        private const float WaitForArenaSeconds = 6f;
        private const float MinDistance = 3.5f;
        private const float MaxDistance = 6.5f;
        private const float ArenaRadius = 14f;
        private const float ClearRadius = 0.9f;
        private const float MaxStepUp = 0.9f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        // Behind-left, behind-right, sides, then the front diagonals (degrees from the hero's forward).
        private static readonly float[] CandidateAngles = { 140f, -140f, 100f, -100f, 165f, -165f, 60f, -60f };

        private static ArenaSecretSpawner _instance;
        private static Mesh _rockMesh;

        /// <summary>Off switch (tests, debugging).</summary>
        public static bool Enabled = true;

        /// <summary>The secret placed in the current arena (null when none).</summary>
        public static ArenaSecretObject Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var host = new GameObject(nameof(ArenaSecretSpawner));
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<ArenaSecretSpawner>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(SpawnWhenArenaStarts());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Current = null;
            StartCoroutine(SpawnWhenArenaStarts());
        }

        private IEnumerator SpawnWhenArenaStarts()
        {
            if (!Enabled || !ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.HasPendingEncounter)
            {
                yield break;
            }

            EncounterPayload payload = transition.PendingEncounter;
            float waited = 0f;
            EncounterArenaController arena = null;
            while (waited < WaitForArenaSeconds)
            {
                arena = FindAnyObjectByType<EncounterArenaController>();
                if (arena != null && arena.IsEncounterActive)
                {
                    break;
                }

                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (arena == null || !arena.IsEncounterActive || transition.PendingEncounter != payload || Current != null)
            {
                yield break;
            }

            RelicDefinition relic = RelicCatalog.ArenaRelicFor(payload.biome);
            bool found = relic == null || SecretsProgress.ResolveShared().HasRelic(relic.Id);
            if (!SecretRules.ShouldSpawnArenaSecret(payload.isBossChallenge, found, SecretRules.ArenaRoll(payload.seed)))
            {
                yield break;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || !TryFindSpot(arena.transform.position, player.transform, payload.seed, out Vector3 spot))
            {
                GameLogger.Info(LogCategory, $"Arena secret rolled in {payload.biome} but no open ground was found; skipped.");
                yield break;
            }

            Current = Build(payload.biome, spot, player.transform.position);
            SceneManager.MoveGameObjectToScene(Current.gameObject, arena.gameObject.scene);
            EventBus.Publish(new ArenaSecretSpawnedEvent(payload.biome, spot));
        }

        /// <summary>Open ground near the hero, inside the arena and level with the hero's feet. False when every candidate is blocked.</summary>
        private static bool TryFindSpot(Vector3 arenaCenter, Transform player, int seed, out Vector3 spot)
        {
            Vector3 forward = player.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            var random = new System.Random(unchecked(seed * 31 + 17));
            float feet = player.position.y;

            foreach (float angle in CandidateAngles)
            {
                float distance = Mathf.Lerp(MinDistance, MaxDistance, (float)random.NextDouble());
                Vector3 flat = Quaternion.Euler(0f, angle, 0f) * forward * distance;
                Vector3 candidate = player.position + flat;

                Vector3 fromCenter = candidate - arenaCenter;
                fromCenter.y = 0f;
                if (fromCenter.magnitude > ArenaRadius)
                {
                    continue;
                }

                if (!Physics.Raycast(new Vector3(candidate.x, feet + 12f, candidate.z), Vector3.down, out RaycastHit hit, 30f, ~0, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                // A prop top (rock, cactus) or a hole: not open ground.
                if (Mathf.Abs(hit.point.y - feet) > MaxStepUp)
                {
                    continue;
                }

                if (IsBlocked(hit.point, hit.collider, player))
                {
                    continue;
                }

                spot = hit.point;
                return true;
            }

            spot = default;
            return false;
        }

        private static bool IsBlocked(Vector3 point, Collider ground, Transform player)
        {
            Collider[] overlaps = Physics.OverlapSphere(point + Vector3.up * (ClearRadius + 0.1f), ClearRadius, ~0, QueryTriggerInteraction.Ignore);
            foreach (Collider other in overlaps)
            {
                if (other != ground && !other.transform.IsChildOf(player))
                {
                    return true;
                }
            }

            return false;
        }

        // ------------------------------------------------------------------
        // Visual
        // ------------------------------------------------------------------

        /// <summary>The secret at <paramref name="position"/>, its glowing face (+z) turned toward <paramref name="lookAt"/> (the hero).</summary>
        private static ArenaSecretObject Build(BiomeType biome, Vector3 position, Vector3 lookAt)
        {
            var root = new GameObject("ArenaSecret_" + biome);
            Vector3 toHero = lookAt - position;
            toHero.y = 0f;
            root.transform.SetPositionAndRotation(position, toHero.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(toHero) : Quaternion.identity);

            var block = new MaterialPropertyBlock();
            Transform sparkle;
            if (biome == BiomeType.Desert)
            {
                // A glinting mound of sand to dig.
                Part(root.transform, PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0f), Vector3.zero, new Vector3(1.5f, 0.45f, 1.3f), new Color(0.93f, 0.8f, 0.52f), block);
                Part(root.transform, PrimitiveType.Sphere, new Vector3(0.35f, 0.12f, -0.2f), Vector3.zero, new Vector3(0.7f, 0.3f, 0.6f), new Color(0.86f, 0.7f, 0.44f), block);
                Part(root.transform, PrimitiveType.Cube, new Vector3(-0.15f, 0.27f, 0.1f), new Vector3(15f, 30f, 10f), new Vector3(0.35f, 0.08f, 0.25f), new Color(0.55f, 0.38f, 0.22f), block);
                sparkle = Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.75f, 0f), new Vector3(45f, 0f, 45f), Vector3.one * 0.16f, new Color(1f, 0.86f, 0.3f), block).transform;
            }
            else
            {
                Color stone = biome == BiomeType.Caves ? new Color(0.36f, 0.32f, 0.42f) : biome == BiomeType.Mountain ? new Color(0.62f, 0.6f, 0.58f) : new Color(0.5f, 0.48f, 0.44f);
                GameObject rock = Part(root.transform, PrimitiveType.Sphere, new Vector3(0f, 0.45f, 0f), new Vector3(0f, 30f, 0f), new Vector3(1.1f, 0.9f, 1f), stone, block);
                rock.GetComponent<MeshFilter>().sharedMesh = RockMesh();

                // The glowing crack: thin gold slabs across the face.
                Color crack = new Color(1f, 0.82f, 0.32f);
                Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, 0.5f), new Vector3(0f, 0f, 62f), new Vector3(0.55f, 0.05f, 0.08f), crack, block);
                Part(root.transform, PrimitiveType.Cube, new Vector3(0.16f, 0.36f, 0.52f), new Vector3(0f, 0f, -48f), new Vector3(0.4f, 0.05f, 0.08f), crack, block);

                if (biome == BiomeType.Caves)
                {
                    Color crystal = new Color(0.45f, 0.85f, 0.95f);
                    Part(root.transform, PrimitiveType.Cube, new Vector3(-0.35f, 0.95f, 0.1f), new Vector3(20f, 10f, 25f), new Vector3(0.14f, 0.5f, 0.14f), crystal, block);
                    Part(root.transform, PrimitiveType.Cube, new Vector3(0.3f, 0.9f, 0.2f), new Vector3(-15f, 40f, -20f), new Vector3(0.12f, 0.4f, 0.12f), crystal, block);
                }

                sparkle = Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 1.45f, 0f), new Vector3(45f, 0f, 45f), Vector3.one * 0.18f, crack, block).transform;
            }

            // One solid collider for the whole secret: the click-to-move raycast ignores triggers.
            var body = root.AddComponent<SphereCollider>();
            body.center = new Vector3(0f, 0.5f, 0f);
            body.radius = biome == BiomeType.Desert ? 0.7f : 0.75f;

            var secret = root.AddComponent<ArenaSecretObject>();
            secret.Configure(biome, sparkle);
            return secret;
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
                Destroy(collider);
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

        private static Mesh RockMesh()
        {
            if (_rockMesh == null)
            {
                _rockMesh = ProceduralRockGenerator.BuildRockMesh(4242, 2, 0.25f, 0.6f);
                _rockMesh.name = "ArenaSecretRock";
            }

            return _rockMesh;
        }
    }
}
