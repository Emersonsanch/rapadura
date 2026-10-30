using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Dressing;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// World-map driver of "Segredos e áreas ocultas" (WorldMap.unity, self-bootstrapped like the dressing):
    /// <list type="bullet">
    /// <item>Once the <see cref="WorldMapDressing"/> is built, places each secret's hint prop next to its region's
    /// node (<see cref="WorldMapSecretHints.FindSpot"/>) in the dressing's palette material, plus a floating gold
    /// marker shown while the secret is revealed but not yet investigated.</item>
    /// <item>Every <see cref="TickInterval"/> it fills a <see cref="SecretRevealContext"/> (token idle time on the
    /// current region, night, story beats seen) and reveals what <see cref="SecretRules.ShouldReveal"/> allows.</item>
    /// <item><see cref="TryInvestigate"/> (the HUD's "Investigar" button, <c>SecretsMapButtons</c>) claims a
    /// reward secret on the spot or starts its guardian fight through <see cref="SceneTransitionService"/>.</item>
    /// </list>
    /// Persistent state lives in <see cref="SecretsProgress"/>; this only holds per-visit state (idle timer, props).
    /// </summary>
    public class WorldMapSecretsDirector : MonoBehaviour
    {
        private const string LogCategory = "Secrets";
        public const float TickInterval = 0.2f;
        private const float MarkerSpinDegrees = 90f;
        private const float MarkerBob = 0.12f;

        public static bool AutoCreate = true;

        private sealed class Hint
        {
            public WorldSecretDefinition Secret;
            public GameObject Prop;
            public GameObject Marker;
            public Vector3 MarkerBase;
        }

        private readonly List<Hint> _hints = new List<Hint>();
        private readonly List<Mesh> _ownedMeshes = new List<Mesh>();
        private WorldMapController _controller;
        private WorldMapDressing _dressing;
        private Transform _root;
        private bool _hintsBuilt;
        private float _nextTick;
        private float _idleSeconds;
        private BiomeType? _idleRegion;

        public static WorldMapSecretsDirector Current { get; private set; }

        public bool HintsBuilt => _hintsBuilt;
        public int HintCount => _hints.Count;

        /// <summary>Seconds the token has been idle on the current region (reveal timer).</summary>
        public float IdleSeconds => _idleSeconds;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Ensure(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure(scene);

        /// <summary>Adds (once) a director to <paramref name="scene"/> when it holds a world map.</summary>
        public static WorldMapSecretsDirector Ensure(Scene scene)
        {
            if (!AutoCreate || !scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            WorldMapController controller = null;
            foreach (WorldMapController candidate in FindObjectsByType<WorldMapController>(FindObjectsSortMode.None))
            {
                if (candidate.gameObject.scene == scene)
                {
                    controller = candidate;
                    break;
                }
            }

            if (controller == null)
            {
                return null;
            }

            foreach (WorldMapSecretsDirector existing in FindObjectsByType<WorldMapSecretsDirector>(FindObjectsSortMode.None))
            {
                if (existing.gameObject.scene == scene)
                {
                    return existing;
                }
            }

            var go = new GameObject("[WorldMapSecrets]");
            if (go.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(go, scene);
            }

            var director = go.AddComponent<WorldMapSecretsDirector>();
            director._controller = controller;
            return director;
        }

        private void OnEnable()
        {
            Current = this;
            EventBus.Subscribe<SecretRevealedEvent>(OnSecretChanged);
            EventBus.Subscribe<SecretClaimedEvent>(OnSecretClaimed);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            if (Current == this)
            {
                Current = null;
            }

            EventBus.Unsubscribe<SecretRevealedEvent>(OnSecretChanged);
            EventBus.Unsubscribe<SecretClaimedEvent>(OnSecretClaimed);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in _ownedMeshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }

            _ownedMeshes.Clear();
            if (_root != null)
            {
                Destroy(_root.gameObject);
            }
        }

        private void OnSecretChanged(SecretRevealedEvent _) => RefreshMarkers();

        private void OnSecretClaimed(SecretClaimedEvent _) => RefreshMarkers();

        private void OnGameLoaded(GameLoadedEvent _)
        {
            _idleSeconds = 0f;
            RefreshMarkers();
        }

        // ------------------------------------------------------------------
        // Tick
        // ------------------------------------------------------------------

        private void Update()
        {
            if (!_hintsBuilt)
            {
                TryBuildHints();
            }

            AnimateMarkers(Time.deltaTime);

            if (Time.unscaledTime < _nextTick)
            {
                return;
            }

            float elapsed = TickInterval + Mathf.Max(0f, Time.unscaledTime - _nextTick);
            _nextTick = Time.unscaledTime + TickInterval;
            Tick(Mathf.Min(elapsed, 1f));
        }

        /// <summary>Advances the idle timer by <paramref name="deltaSeconds"/> and reveals what the rules allow now.</summary>
        public void Tick(float deltaSeconds)
        {
            if (_controller == null)
            {
                _controller = FindAnyObjectByType<WorldMapController>();
            }

            if (_controller == null || _controller.CurrentNode == null || _controller.Progress == null)
            {
                return;
            }

            BiomeType region = _controller.CurrentNode.Region;
            bool idle = _controller.CanExplore && !GameplayPause.IsPaused;
            if (!idle || _idleRegion != region)
            {
                _idleSeconds = 0f;
                _idleRegion = idle ? region : (BiomeType?)null;
            }
            else
            {
                _idleSeconds += deltaSeconds;
            }

            ServiceLocator.TryGet(out TimeOfDayManager time);
            ServiceLocator.TryGet(out StoryProgress story);
            var context = new SecretRevealContext
            {
                Region = region,
                RegionUnlocked = _controller.Progress.IsUnlocked(region),
                TokenIdle = idle,
                IdleSeconds = _idleSeconds,
                IsNight = time != null && time.Phase == TimeOfDayPhase.Night,
                HasSeenBeat = story != null ? story.HasSeen : (System.Func<StoryBeat, bool>)null,
            };

            RevealDue(SecretsProgress.ResolveShared(), context);
        }

        /// <summary>Reveals every not-yet-revealed secret the context allows. Returns how many were revealed.</summary>
        public static int RevealDue(SecretsProgress secrets, in SecretRevealContext context)
        {
            int revealed = 0;
            foreach (WorldSecretDefinition secret in WorldSecretCatalog.All)
            {
                if (!secrets.IsRevealed(secret.Id) && SecretRules.ShouldReveal(secret, context) && secrets.Reveal(secret.Id))
                {
                    revealed++;
                }
            }

            return revealed;
        }

        // ------------------------------------------------------------------
        // Investigate
        // ------------------------------------------------------------------

        /// <summary>The revealed, not-yet-investigated secret of the region the idle token stands on (null otherwise).</summary>
        public WorldSecretDefinition InvestigableHere
        {
            get
            {
                if (_controller == null || !_controller.CanExplore || _controller.CurrentNode == null)
                {
                    return null;
                }

                SecretsProgress secrets = SecretsProgress.ResolveShared();
                foreach (WorldSecretDefinition secret in WorldSecretCatalog.InRegion(_controller.CurrentNode.Region))
                {
                    if (secrets.IsInvestigable(secret.Id))
                    {
                        return secret;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// "Investigar": a reward secret is claimed right away (and auto-saved); a guarded one starts its fight — the
        /// region's normal enemy roll — and is claimed by <see cref="SecretsProgress"/> on victory.
        /// </summary>
        public bool TryInvestigate(out bool startedFight)
        {
            startedFight = false;
            WorldSecretDefinition secret = InvestigableHere;
            if (secret == null || GameplayPause.IsPaused)
            {
                return false;
            }

            SecretsProgress secrets = SecretsProgress.ResolveShared();
            if (secret.Visit == SecretVisitKind.Reward)
            {
                if (!secrets.Claim(secret.Id))
                {
                    return false;
                }

                if (ServiceLocator.TryGet(out SaveManager save))
                {
                    save.AutoSave();
                }

                return true;
            }

            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || _controller.Encounters == null)
            {
                GameLogger.Warning(LogCategory, $"Secret {secret.Id} needs a fight but no scene transition is available.");
                return false;
            }

            // A second tap while the arena loads must not replace (and then cancel) the fight already under way.
            if (transition.HasPendingEncounter || secrets.PendingEncounterSecret != null)
            {
                return false;
            }

            EncounterPayload payload = _controller.Encounters.BuildPayload(BiomeRegistry.Get(secret.Region));
            if (payload == null || !secrets.BeginSecretEncounter(secret.Id, payload))
            {
                return false;
            }

            if (!transition.BeginEncounter(payload))
            {
                secrets.CancelSecretEncounter();
                return false;
            }

            startedFight = true;
            return true;
        }

        // ------------------------------------------------------------------
        // Hint props
        // ------------------------------------------------------------------

        private void TryBuildHints()
        {
            if (_dressing == null)
            {
                _dressing = FindAnyObjectByType<WorldMapDressing>();
            }

            if (_dressing == null || !_dressing.IsBuilt || _dressing.Terrain == null || _dressing.PaletteMaterial == null)
            {
                return;
            }

            BuildHints(_dressing);
        }

        /// <summary>Places every secret's hint prop and marker on <paramref name="dressing"/>'s island (once).</summary>
        public void BuildHints(WorldMapDressing dressing)
        {
            if (_hintsBuilt || dressing == null || dressing.Terrain == null)
            {
                return;
            }

            _hintsBuilt = true;
            _dressing = dressing;
            WorldMapTerrain terrain = dressing.Terrain;

            _root = new GameObject("SecretHints").transform;
            _root.SetParent(dressing.transform, false);

            var builder = new WorldMapMeshBuilder();
            builder.Clear();
            WorldMapSecretHints.AppendMarker(builder);
            Mesh markerMesh = Own(builder.ToMesh("WorldMapSecretMarker"));

            var taken = new List<Vector2>();
            for (int site = 0; site < terrain.Sites.Count; site++)
            {
                foreach (WorldSecretDefinition secret in WorldSecretCatalog.InRegion(terrain.Sites[site].Region))
                {
                    Vector2 spot = WorldMapSecretHints.FindSpot(terrain, site, secret.PreferredAngle, taken, dressing.Props);
                    taken.Add(spot);
                    var ground = new Vector3(spot.x, terrain.Height(spot) - 0.03f, spot.y);

                    builder.Clear();
                    WorldMapSecretHints.AppendHint(builder, secret.Hint);
                    Mesh propMesh = Own(builder.ToMesh("WorldMapSecretHint_" + secret.Id));

                    var hint = new Hint { Secret = secret };
                    hint.Prop = CreateMeshObject("Hint_" + secret.Id, propMesh, dressing.PaletteMaterial, true);
                    hint.Prop.transform.localPosition = ground;
                    hint.Prop.transform.localRotation = Quaternion.Euler(0f, (StableHash(secret.Id) & 31) - 15f, 0f);

                    hint.Marker = CreateMeshObject("Marker", markerMesh, dressing.PaletteMaterial, false);
                    hint.MarkerBase = ground + Vector3.up * WorldMapSecretHints.MarkerHeight;
                    hint.Marker.transform.localPosition = hint.MarkerBase;
                    _hints.Add(hint);
                }
            }

            RefreshMarkers();
            GameLogger.Info(LogCategory, $"World map secrets placed: {_hints.Count} hints.");
        }

        /// <summary>Markers over revealed, not-yet-investigated secrets only.</summary>
        public void RefreshMarkers()
        {
            SecretsProgress secrets = SecretsProgress.ResolveShared();
            foreach (Hint hint in _hints)
            {
                if (hint.Marker != null)
                {
                    hint.Marker.SetActive(secrets.IsInvestigable(hint.Secret.Id));
                }
            }
        }

        /// <summary>For tests/debug: whether <paramref name="secretId"/>'s marker is showing.</summary>
        public bool IsMarkerShown(string secretId)
        {
            foreach (Hint hint in _hints)
            {
                if (hint.Secret.Id == secretId)
                {
                    return hint.Marker != null && hint.Marker.activeSelf;
                }
            }

            return false;
        }

        private void AnimateMarkers(float deltaTime)
        {
            float bob = Mathf.Sin(Time.time * 2.2f) * MarkerBob;
            foreach (Hint hint in _hints)
            {
                if (hint.Marker == null || !hint.Marker.activeSelf)
                {
                    continue;
                }

                Transform t = hint.Marker.transform;
                t.localPosition = hint.MarkerBase + Vector3.up * bob;
                t.Rotate(0f, MarkerSpinDegrees * deltaTime, 0f, Space.World);
            }
        }

        private GameObject CreateMeshObject(string objectName, Mesh mesh, Material material, bool castShadows)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(_root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return go;
        }

        /// <summary>string.GetHashCode is randomized per process on some runtimes; the yaw jitter must not move between runs.</summary>
        private static int StableHash(string text)
        {
            int hash = 17;
            foreach (char c in text)
            {
                hash = unchecked(hash * 31 + c);
            }

            return hash & int.MaxValue;
        }

        private Mesh Own(Mesh mesh)
        {
            if (mesh != null)
            {
                _ownedMeshes.Add(mesh);
            }

            return mesh;
        }
    }
}
