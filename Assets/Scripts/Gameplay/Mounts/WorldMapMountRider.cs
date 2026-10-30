using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Riding on the world map (WorldMap.unity), self-bootstrapping on the scene's <see cref="WorldMapController"/> (no
    /// scene edit). The same <see cref="MountSession"/> as the arena: hold the mount key (H / D-pad down) or the HUD
    /// button for <see cref="MountRules.SummonSeconds"/>, the mount runs up to the hero token and the hero sits on it
    /// (<see cref="MountActor.Follow"/> + <see cref="MountRiderPose"/>; the mount is scaled to the token's model).
    /// While mounted the token travels faster through <see cref="WorldMapController.MountSpeedProbe"/>: the species ×,
    /// a gallop while the trip runs and the mount has stamina (the map has no run key: the gallop is automatic, and comes
    /// back once <see cref="MountRules.GallopResumeFraction"/> of the stamina refilled), and only trot (×1.00, no ability)
    /// inside a city's safe zone (<see cref="WorldMapCityPresence"/>, i.e. <see cref="SafeZoneRules"/>). Road-event slows
    /// keep applying on top. Map abilities: Arrancada = a short burst of speed, Faro de Tesouro = how many secrets of the
    /// region are still hidden, Zurro = just the bray (there are no critters on the road). If the cousins were riding
    /// when an encounter started, they are back on the mount when the map loads again ("a montaria volta com você").
    /// It also drives "Primeira Sela" (<see cref="FirstSaddleQuest.Evaluate"/>, the visit to the Vila).
    /// </summary>
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public class WorldMapMountRider : MonoBehaviour, IMountRider
    {
        public const string HeroModelPrefix = "HeroToken_";
        private const float GroundOffset = 0.5f;
        private const float ReferenceHeroHeight = 1.55f;
        private const float DashBoost = 2.5f;
        private const float DashSeconds = 0.6f;
        private const float RunInTimeout = 3f;
        private const float EvaluateInterval = 1f;

        private static bool _remountOnReturn;

        /// <summary>Metres of road per map unit (the token walks 4 units/s for the hero's 5.5 m/s run).</summary>
        public const float MapMetersPerUnit = 5.5f / 4f;

        private Vector3 _lastTokenPosition;

        private readonly MountSession _session = new MountSession();
        private WorldMapController _controller;
        private MountActor _actor;
        private MountRiderPose _riderPose;
        private Transform _heroModel;
        private InputAction _mountAction;
        private InputAction _abilityAction;
        private InputActionMap _inputMap;
        private bool _prevTouchHeld;
        private bool _runningIn;
        private float _runInElapsed;
        private float _boostRemaining;
        private bool _abilityWasReady = true;
        private float _nextEvaluate;
        private System.Func<float> _probe;

        public static WorldMapMountRider Current { get; private set; }

        public MountSession Session => _session;
        public bool IsMounted => _session.IsMounted;
        public bool IsWhistling => _session.IsWhistling;
        public MountDefinition Definition => MountCollection.ResolveShared().ActiveDefinition;
        public OwnedMount ActiveMount => MountCollection.ResolveShared().Active;
        public bool TouchMountHeld { get; set; }
        public bool TouchAbilityRequested { get; set; }

        public float Now => Time.unscaledTime;

        public string KeyHint => _mountAction != null && _mountAction.bindings.Count > 0 ? _mountAction.GetBindingDisplayString(0) : string.Empty;

        public bool InSafeZone => WorldMapCityPresence.Current != null && WorldMapCityPresence.Current.TokenInSafeZone;

        public MountPlace Place => MountRules.ResolvePlace(onWorldMap: true, inSafeZone: InSafeZone, arenaFightActive: false, indoors: false);

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            WorldMapController controller = FindFirstObjectByType<WorldMapController>();
            if (controller != null && controller.GetComponent<WorldMapMountRider>() == null)
            {
                controller.gameObject.AddComponent<WorldMapMountRider>();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _remountOnReturn = false;

        private void Awake()
        {
            _controller = GetComponent<WorldMapController>();
            _probe = SpeedFactor;
        }

        private void OnEnable()
        {
            Current = this;
            if (_controller != null)
            {
                _controller.MountSpeedProbe = _probe;
            }

            _inputMap = MountInput.CreateMap(out _mountAction, out _abilityAction);
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                MountTexts.EnsureRegistered(localization);
            }
        }

        private void OnDisable()
        {
            // Scene leaving (an encounter): remember to be back on the mount when the map returns.
            _remountOnReturn = _session.IsMounted;
            if (_session.IsMounted)
            {
                Dismount(startRecall: false);
            }

            if (_controller != null && _controller.MountSpeedProbe == _probe)
            {
                _controller.MountSpeedProbe = null;
            }

            _inputMap?.Disable();
            _inputMap?.Dispose();
            _inputMap = null;
            _mountAction = _abilityAction = null;
            if (Current == this)
            {
                Current = null;
            }
        }

        private void OnDestroy()
        {
            if (_actor != null)
            {
                Destroy(_actor.gameObject);
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out WorldMapProgress map))
            {
                FirstSaddleQuest.Evaluate(map);
            }
        }

        // ------------------------------------------------------------------
        // Speed probe
        // ------------------------------------------------------------------

        /// <summary>Token speed factor (1 on foot): <see cref="MountRules.MountedSpeedMultiplier"/> with no buffs on the map, plus the Arrancada burst.</summary>
        public float SpeedFactor()
        {
            MountDefinition definition = Definition;
            if (!_session.IsMounted || definition == null)
            {
                return 1f;
            }

            OwnedMount owned = ActiveMount;
            float ride = MountRules.MountedSpeedMultiplier(1f, definition.SpeedMultiplier, _session.IsGalloping, InSafeZone, owned != null && owned.IsHungry);
            return _boostRemaining > 0f ? Mathf.Min(MountRules.MaxMoveMultiplier, ride * DashBoost) : ride;
        }

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float now = Time.unscaledTime;
            bool paused = GameplayPause.IsPaused;
            MountPlace place = Place;
            Transform token = _controller != null ? _controller.PlayerToken : null;
            if (token == null)
            {
                return;
            }

            if (_remountOnReturn && !_session.IsMounted && ActiveMount != null && !ActiveMount.IsTired && Definition != null)
            {
                _remountOnReturn = false;
                SpawnActor(token, token.position + Vector3.down * GroundOffset);
                Mount(token, instant: true);
            }

            bool held = !paused && ((_mountAction != null && _mountAction.IsPressed()) || TouchMountHeld);
            bool pressed = !paused && ((_mountAction != null && _mountAction.WasPressedThisFrame()) || (TouchMountHeld && !_prevTouchHeld));
            _prevTouchHeld = TouchMountHeld;

            if (pressed)
            {
                OnMountPressed(place, now);
            }

            if (_session.IsWhistling && _session.TickWhistle(paused ? 0f : dt, held, MountRideCare.WhistleSeconds(ActiveMount)))
            {
                StartRunIn(token);
            }

            TickRunIn(token, paused ? 0f : dt);

            bool wantsGallop = _controller.IsTraveling;
            OwnedMount riding = ActiveMount;
            _session.Tick(paused ? 0f : dt, wantsGallop, place, riding != null && riding.IsHungry);
            if (_session.IsMounted && riding != null)
            {
                Vector3 moved = token.position - _lastTokenPosition;
                moved.y = 0f;
                _lastTokenPosition = token.position;
                MountRideCare.TickMounted(MountCollection.ResolveShared(), riding, Definition, paused ? 0f : dt, moved.magnitude * MapMetersPerUnit);

                // A saddle/blanket changed in the stable panel while riding: rebuild the model in place.
                if (_actor != null && (_actor.OwnedGuid != riding.guid || _actor.CosmeticSignature != MountActor.Signature(riding)))
                {
                    SpawnActor(token, token.position + Vector3.down * GroundOffset);
                    _actor.Follow(token, GroundOffset);
                    BeginRiderPose(token);
                }
            }
            if (_actor != null)
            {
                _actor.Galloping = _session.IsMounted && _session.IsGalloping;
            }

            _boostRemaining = Mathf.Max(0f, _boostRemaining - (paused ? 0f : dt));

            bool abilityPressed = !paused && ((_abilityAction != null && _abilityAction.WasPressedThisFrame()) || TouchAbilityRequested);
            TouchAbilityRequested = false;
            if (abilityPressed)
            {
                TryUseAbility();
            }

            bool ready = _session.AbilityRemaining(now) <= 0f;
            if (ready && !_abilityWasReady && _session.IsMounted)
            {
                MountAudio.Play(MountAudio.AbilityReady);
            }

            _abilityWasReady = ready;

            if (now >= _nextEvaluate)
            {
                _nextEvaluate = now + EvaluateInterval;
                EvaluateQuest();
            }
        }

        private void LateUpdate()
        {
            if (!_session.IsMounted || _actor == null || _controller == null || _controller.PlayerToken == null)
            {
                return;
            }

            Transform token = _controller.PlayerToken;
            if (_riderPose == null || _riderPose.Visual == null)
            {
                BeginRiderPose(token);
            }

            bool reduced = ServiceLocator.TryGet(out AccessibilitySettings accessibility) && accessibility != null && accessibility.ReduceMotion;
            _riderPose?.Apply(_actor.Seat.position.y, Time.unscaledTime, Time.unscaledDeltaTime, reduced);
        }

        // ------------------------------------------------------------------
        // Whistle, run-in, mount
        // ------------------------------------------------------------------

        private void OnMountPressed(MountPlace place, float now)
        {
            if (_session.IsMounted)
            {
                Dismount(startRecall: true);
                return;
            }

            if (_session.IsWhistling || _runningIn)
            {
                return;
            }

            OwnedMount mount = ActiveMount;
            MountSummonBlock block = _session.BeginWhistle(place, mount != null && Definition != null, mount != null && mount.IsTired, float.PositiveInfinity, now);
            if (block != MountSummonBlock.None)
            {
                Refuse(block, mount, now);
                return;
            }

            MountAudio.Play(Definition.SfxCallKey);
        }

        private float ModelScale(Transform token)
        {
            _heroModel = FindHeroModel(token);
            if (_heroModel == null)
            {
                return 2.3f;
            }

            float height = 0f;
            foreach (Renderer renderer in _heroModel.GetComponentsInChildren<Renderer>())
            {
                height = Mathf.Max(height, renderer.bounds.max.y - (token.position.y - GroundOffset));
            }

            return height > 0.1f ? Mathf.Clamp(height / ReferenceHeroHeight, 0.5f, 5f) : 2.3f;
        }

        private void SpawnActor(Transform token, Vector3 at)
        {
            if (_actor != null)
            {
                Destroy(_actor.gameObject);
            }

            Vector3 flat = token.position - at;
            flat.y = 0f;
            Quaternion facing = flat.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(flat, Vector3.up) : Quaternion.Euler(0f, token.eulerAngles.y, 0f);
            _actor = MountActor.Spawn(Definition, at, facing, ModelScale(token), null, ActiveMount);
            _actor.StepSounds = false;
        }

        private void StartRunIn(Transform token)
        {
            float scale = ModelScale(token);
            Vector3 ground = token.position + Vector3.down * GroundOffset;
            Vector3 from = ground - token.forward * 3f * scale + token.right * 1.2f * scale;
            SpawnActor(token, from);
            _runningIn = true;
            _runInElapsed = 0f;
        }

        private void TickRunIn(Transform token, float dt)
        {
            if (!_runningIn || _actor == null)
            {
                return;
            }

            _runInElapsed += dt;
            Vector3 ground = token.position + Vector3.down * GroundOffset;
            Vector3 to = ground - _actor.transform.position;
            to.y = 0f;
            if (to.magnitude <= 0.25f * _actor.transform.localScale.x || _runInElapsed >= RunInTimeout)
            {
                _runningIn = false;
                Mount(token, instant: false);
                return;
            }

            _actor.RunTo(ground, 6f * _actor.transform.localScale.x);
        }

        private void Mount(Transform token, bool instant)
        {
            OwnedMount owned = ActiveMount;
            if (owned == null || _actor == null)
            {
                return;
            }

            _session.Mount(MountCollection.MaxStaminaOf(owned));
            _actor.Follow(token, GroundOffset);
            BeginRiderPose(token);
            if (!instant)
            {
                MountAudio.Play(MountAudio.MountUp);
                MountNotices.Raise(MountRideCare.MountLine(Localization, Definition, owned));
            }

            _lastTokenPosition = token.position;
        }

        private void BeginRiderPose(Transform token)
        {
            _heroModel = FindHeroModel(token);
            _riderPose = _heroModel != null ? new MountRiderPose(_heroModel, token, GroundOffset) : null;
            _riderPose?.Begin(instant: true);
        }

        private void Dismount(bool startRecall)
        {
            _session.Dismount(Time.unscaledTime, startRecall);
            _riderPose?.Release();
            _riderPose = null;
            _boostRemaining = 0f;
            if (_actor != null)
            {
                MountActor leaving = _actor;
                _actor = null;
                leaving.Follow(null, 0f);
                Transform t = leaving.transform;
                Vector3 away = t.position + t.right * 3f * t.localScale.x - t.forward * t.localScale.x;
                leaving.RunTo(away, 3f * t.localScale.x, () =>
                {
                    if (leaving != null)
                    {
                        Destroy(leaving.gameObject, 0.2f);
                    }
                });
                if (startRecall)
                {
                    MountAudio.Play(MountAudio.Dismount);
                }
            }
        }

        // ------------------------------------------------------------------
        // Abilities
        // ------------------------------------------------------------------

        public bool TryUseAbility()
        {
            MountDefinition definition = Definition;
            MountAbilityDefinition ability = definition != null ? definition.Ability : null;
            OwnedMount owned = ActiveMount;
            float now = Time.unscaledTime;
            if (ability == null || owned == null || !_session.IsMounted)
            {
                return false;
            }

            LocalizationManager localization = Localization;
            if (Place == MountPlace.SafeZone)
            {
                MountNotices.Raise(MountTexts.Get(localization, MountTexts.SafeZoneNoAbility));
                MountAudio.Play(MountAudio.Refused);
                return false;
            }

            float cooldown = MountRideCare.AbilityCooldown(definition, owned);
            if (!_session.TryUseAbility(now, Place, cooldown))
            {
                MountAudio.Play(MountAudio.Refused);
                return false;
            }

            _abilityWasReady = false;
            switch (ability.Kind)
            {
                case MountAbilityKind.Dash:
                    _boostRemaining = DashSeconds;
                    MountAudio.Play(MountAudio.Dash);
                    MountNotices.Raise(MountTexts.Get(localization, MountTexts.AbilityDashMap));
                    break;
                case MountAbilityKind.LootMagnet:
                {
                    MountAudio.Play(MountAudio.Sniff);
                    int hidden = HiddenSecretsHere();
                    MountNotices.Raise(hidden > 0
                        ? MountTexts.Format(localization, MountTexts.AbilitySecretsFound, hidden)
                        : MountTexts.Get(localization, MountTexts.AbilityNoSecrets));
                    break;
                }
                case MountAbilityKind.Fear:
                    MountAudio.Play(MountAudio.Bray);
                    MountNotices.Raise(MountTexts.Get(localization, MountTexts.AbilityBrayMap));
                    break;
                case MountAbilityKind.FreeGallop:
                    // Fôlego de Ema works on the road too: the token gallops for free.
                    _session.StartFreeGallop(ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Dash);
                    MountNotices.Raise(MountTexts.Format(localization, MountWaveTexts.ToastFreeGallop, MountTexts.MountName(localization, definition)));
                    break;
                default:
                    // The other wave-1 abilities act on critters and secrets in the arena; on the map the mount just shows off.
                    MountAudio.Play(MountAudio.Cowbell);
                    MountNotices.Raise(MountTexts.Format(localization, MountWaveTexts.MapShowOff, MountTexts.MountName(localization, definition),
                        MountTexts.AbilityName(localization, definition)));
                    break;
            }

            return true;
        }

        /// <summary>Secrets of the token's region not revealed yet.</summary>
        private int HiddenSecretsHere()
        {
            if (_controller == null || _controller.CurrentNode == null || !ServiceLocator.TryGet(out SecretsProgress secrets) || secrets == null)
            {
                return 0;
            }

            int hidden = 0;
            foreach (WorldSecretDefinition secret in WorldSecretCatalog.InRegion(_controller.CurrentNode.Region))
            {
                if (!secrets.IsRevealed(secret.Id))
                {
                    hidden++;
                }
            }

            return hidden;
        }

        // ------------------------------------------------------------------

        private void EvaluateQuest()
        {
            if (ServiceLocator.TryGet(out WorldMapProgress map))
            {
                FirstSaddleQuest.Evaluate(map);
            }

            // Standing in the Vila counts as the visit to the Haras (the city hub may not list its shops yet).
            if (_controller != null && _controller.CanExplore && _controller.CurrentNode != null
                && _controller.CurrentNode.Region == FirstSaddleQuest.GiftRegion && CityCatalog.HasCity(BiomeType.Forest))
            {
                FirstSaddleQuest.ReportVisitedVillage();
            }
        }

        private void Refuse(MountSummonBlock block, OwnedMount mount, float now)
        {
            LocalizationManager localization = Localization;
            string text;
            switch (block)
            {
                case MountSummonBlock.NoMount: text = MountTexts.Get(localization, MountTexts.RefusedNoMount); break;
                case MountSummonBlock.Tired:
                    text = MountTexts.Format(localization, MountTexts.RefusedTired, MountTexts.MountName(localization, Definition), mount != null ? mount.tiredUntilEncounter : 0);
                    break;
                case MountSummonBlock.RecallCooldown:
                    text = MountTexts.Format(localization, MountTexts.RefusedRecall, Mathf.CeilToInt(_session.RecallRemaining(now)));
                    break;
                default: text = MountTexts.Get(localization, MountTexts.RefusedNotHere); break;
            }

            MountAudio.Play(MountAudio.Refused);
            MountNotices.Raise(text);
        }

        private static Transform FindHeroModel(Transform token)
        {
            if (token == null)
            {
                return null;
            }

            for (int i = 0; i < token.childCount; i++)
            {
                Transform child = token.GetChild(i);
                if (child.name.StartsWith(HeroModelPrefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;
    }
}
