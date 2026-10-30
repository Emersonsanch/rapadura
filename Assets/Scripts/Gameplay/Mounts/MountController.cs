using System;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>Raised when the rider gets on or off (HUD, sounds, other systems).</summary>
    public readonly struct MountRiderChangedEvent : IGameEvent
    {
        public readonly bool Mounted;
        public readonly string MountId;

        public MountRiderChangedEvent(bool mounted, string mountId)
        {
            Mounted = mounted;
            MountId = mountId;
        }
    }

    /// <summary>
    /// The hero's mount in a 3D scene (Main.unity, and the walkable cities once they exist). Self-bootstrapping, no
    /// scene edit: after every scene load with a hero (<see cref="PlayerStats"/> + <see cref="PlayerMotor"/>) it is
    /// added to the hero's GameObject.
    /// <list type="bullet">
    /// <item><b>Whistle</b>: hold the <see cref="MountActionName"/> action (H / D-pad down; or the HUD button,
    /// <see cref="TouchMountHeld"/>) for <see cref="MountRules.SummonSeconds"/>; refused with a reason by
    /// <see cref="MountRules.CanSummon"/> (fight on, 5 s since the last hit, tired mount, 10 s after a dismount). Taking
    /// damage, attacking, casting or jumping breaks it. The mount then runs in (<see cref="MountActor"/>) and the hero
    /// climbs on. Pressing it again while mounted gets off at once.</item>
    /// <item><b>Riding</b>: <see cref="PlayerMotor.SpeedSource"/> = this, so every stance moves at the run speed × the
    /// species (speed buffs ignored, slows kept, <see cref="MountRules.MountedSpeedMultiplier"/>); holding Run gallops
    /// on the mount's own stamina (the hero's stamina is left untouched); the mount's defense is a
    /// <c>Defense</c> PercentAdditive buff (source <see cref="MountRules.BuffSource"/>); half the damage taken goes to the
    /// mount, which at 0 gets tired and trots home. The hero sits astride (<see cref="MountRiderPose"/>).</item>
    /// <item><b>Places</b>: an arena fight always starts on foot (the active mount waits at the edge and comes at the
    /// whistle once the fight is over); in a safe zone the mount only trots and no ability works. Walkable cities can
    /// report their zone through <see cref="SafeZoneProbe"/> (the hook for <see cref="SafeZoneRules"/>; nothing sets it yet).</item>
    /// <item><b>Ability</b>: <see cref="AbilityActionName"/> (G / D-pad left, or the HUD) fires the species' ability
    /// (<see cref="MountAbilityExecutor"/>); a cowbell rings when it is ready again.</item>
    /// <item>Crouching, attacking or casting a skill while mounted gets off first (phase M1 has no mounted combat).</item>
    /// </list>
    /// </summary>
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public class MountController : MonoBehaviour, IMotorSpeedSource, IMountRider
    {
        public const string MountActionName = "Mount";
        public const string AbilityActionName = "MountAbility";
        public const string DefaultMountBinding = "<Keyboard>/h";
        public const string DefaultMountGamepadBinding = "<Gamepad>/dpad/down";
        public const string DefaultAbilityBinding = "<Keyboard>/g";
        public const string DefaultAbilityGamepadBinding = "<Gamepad>/dpad/left";

        private const float RunInSpeed = 9f;
        private const float RunInDistance = 10f;
        private const float RunInTimeout = 3f;
        private const float MountReach = 0.7f;
        private const float LeaveDistance = 7f;
        private const float LeaveSpeed = 4.5f;

        /// <summary>The live controller of the loaded 3D scene (null on the world map).</summary>
        public static MountController Active { get; private set; }

        /// <summary>
        /// Hook for walkable cities: the zone the hero stands in (see <see cref="SafeZoneRules"/>). Null = everything is
        /// <see cref="ZoneKind.Wilds"/> (the arena has no city).
        /// </summary>
        public static Func<Vector3, ZoneKind> SafeZoneProbe { get; set; }

        private readonly MountSession _session = new MountSession();

        private PlayerMotor _motor;
        private PlayerStats _stats;
        private BuffController _buffs;
        private PlayerInputHandler _input;
        private CharacterController _characterController;
        private PlayerProceduralAnimator _heroAnimator;
        private EncounterArenaController _arena;
        private MountActor _actor;
        private MountActor _leaving;
        private MountRiderPose _riderPose;
        private StatModifierDefinition _defenseBuff;

        private InputAction _mountAction;
        private InputAction _abilityAction;
        private InputActionMap _ownedMap;

        private float _lastCombatTime = float.NegativeInfinity;
        private Vector3 _lastRidePosition;
        private float _runInElapsed;
        private bool _runningIn;
        private bool _prevTouchHeld;
        private float _frozenHeroStamina = -1f;
        private bool _abilityWasReady = true;
        private float _dashRemaining;
        private float _dashSpeed;
        private bool _wasFightActive;

        public MountSession Session => _session;
        public bool IsMounted => _session.IsMounted;
        public bool IsWhistling => _session.IsWhistling;
        public bool IsRunningIn => _runningIn;
        public MountActor Actor => _actor;
        public MountDefinition Definition => MountCollection.ResolveShared().ActiveDefinition;
        public OwnedMount ActiveMount => MountCollection.ResolveShared().Active;

        /// <summary>Held by the HUD's mount button (touch).</summary>
        public bool TouchMountHeld { get; set; }

        /// <summary>One-shot from the HUD's ability button.</summary>
        public bool TouchAbilityRequested { get; set; }

        /// <summary>Where the hero is for riding purposes this frame.</summary>
        public MountPlace Place
        {
            get
            {
                bool fight = _arena != null && _arena.IsEncounterActive;
                bool safe = SafeZoneProbe != null && SafeZoneRules.IsSafe(SafeZoneProbe(transform.position));
                return MountRules.ResolvePlace(onWorldMap: false, inSafeZone: safe, arenaFightActive: fight, indoors: false);
            }
        }

        public float SecondsSinceCombat => Time.time - _lastCombatTime;

        public float Now => Time.time;

        /// <summary>"H" / "D-pad ↓" as shown on the HUD for keyboard/gamepad players.</summary>
        public string KeyHint => _mountAction != null && _mountAction.bindings.Count > 0 ? _mountAction.GetBindingDisplayString(0) : string.Empty;

        public string AbilityKeyHint => _abilityAction != null && _abilityAction.bindings.Count > 0 ? _abilityAction.GetBindingDisplayString(0) : string.Empty;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHero();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHero();

        private static void AttachToHero()
        {
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null && stats.GetComponent<PlayerMotor>() != null && stats.GetComponent<MountController>() == null)
            {
                stats.gameObject.AddComponent<MountController>();
            }
        }

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _stats = GetComponent<PlayerStats>();
            _buffs = GetComponent<BuffController>();
            _input = GetComponent<PlayerInputHandler>();
            _characterController = GetComponent<CharacterController>();
            _heroAnimator = GetComponent<PlayerProceduralAnimator>();
        }

        private void OnEnable()
        {
            Active = this;
            if (_motor != null)
            {
                _motor.SpeedSource = this;
            }

            BindInput();
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Subscribe<MountCollectionChangedEvent>(OnCollectionChanged);
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                MountTexts.EnsureRegistered(localization);
            }
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Unsubscribe<MountCollectionChangedEvent>(OnCollectionChanged);
            if (_session.IsMounted)
            {
                Dismount(startRecall: false, leave: false);
            }

            if (_motor != null && ReferenceEquals(_motor.SpeedSource, this))
            {
                _motor.SpeedSource = null;
            }

            UnbindInput();
            if (Active == this)
            {
                Active = null;
            }
        }

        private void Start()
        {
            _arena = FindFirstObjectByType<EncounterArenaController>();
            _wasFightActive = _arena != null && _arena.IsEncounterActive;
            SpawnWaitingAtEdgeIfFighting();
        }

        private void OnDestroy()
        {
            DestroyActor(ref _actor);
            DestroyActor(ref _leaving);
        }

        // ------------------------------------------------------------------
        // IMotorSpeedSource
        // ------------------------------------------------------------------

        public bool IsActive => _session.IsMounted;

        public float ResolveMultiplier(float buffMultiplier)
        {
            MountDefinition definition = Definition;
            OwnedMount owned = ActiveMount;
            float ride = definition != null ? _session.SpeedMultiplier(buffMultiplier, definition.SpeedMultiplier, Place, owned != null && owned.IsHungry) : buffMultiplier;
            return Rapadura.Gameplay.Inventory.EncumbranceRules.ApplyFactor(ride, LoadFactor);
        }

        /// <summary>
        /// Carry-load factor of the ride (0..1, 1 = light saddlebags), set by <c>EncumbranceTracker</c>
        /// (<c>EncumbranceRules.MountSpeedFactor</c>). Only ever lowers the already-capped ride multiplier.
        /// </summary>
        public float LoadFactor { get; set; } = 1f;

        /// <summary>False while the ride is over capacity (<c>EncumbranceRules.CanGallop</c>): no gallop.</summary>
        public bool LoadAllowsGallop { get; set; } = true;

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.deltaTime;
            bool paused = GameplayPause.IsPaused;
            MountPlace place = Place;

            if (_arena == null)
            {
                _arena = FindFirstObjectByType<EncounterArenaController>();
            }

            bool fight = place == MountPlace.ArenaFight;
            if (fight && !_wasFightActive)
            {
                // A fight just started: on foot (M1), the mount goes to wait at the edge.
                if (_session.IsMounted)
                {
                    Dismount(startRecall: false, leave: true);
                }

                CancelRunIn();
            }

            _wasFightActive = fight;

            bool held = !paused && ((_mountAction != null && _mountAction.IsPressed()) || TouchMountHeld);
            bool pressed = !paused && ((_mountAction != null && _mountAction.WasPressedThisFrame()) || (TouchMountHeld && !_prevTouchHeld));
            _prevTouchHeld = TouchMountHeld;

            if (pressed)
            {
                OnMountPressed(place);
            }

            if (_session.IsWhistling)
            {
                if (_input != null && _input.JumpPressedThisFrame)
                {
                    BreakWhistle();
                }
                else if (_session.TickWhistle(dt, held, MountRideCare.WhistleSeconds(ActiveMount)))
                {
                    StartRunIn();
                }
            }

            TickRunIn(dt);

            if (_session.IsMounted)
            {
                if (MountRules.ShouldForceDismount(place, MountRules.IsInCombat(SecondsSinceCombat)) || (_input != null && _input.IsCrouchHeld))
                {
                    Dismount(startRecall: true, leave: true);
                }
            }

            bool wantsGallop = _input != null && _input.IsRunHeld && LoadAllowsGallop;
            OwnedMount riding = ActiveMount;
            _session.Tick(dt, wantsGallop, place, riding != null && riding.IsHungry);
            if (_session.IsMounted && riding != null)
            {
                Vector3 now = transform.position;
                Vector3 moved = now - _lastRidePosition;
                moved.y = 0f;
                _lastRidePosition = now;
                MountRideCare.TickMounted(MountCollection.ResolveShared(), riding, Definition, dt, Mathf.Min(moved.magnitude, 20f * Mathf.Max(dt, 0.02f)));
            }
            if (_actor != null)
            {
                _actor.Galloping = _session.IsMounted && _session.IsGalloping;
            }

            bool abilityPressed = !paused && ((_abilityAction != null && _abilityAction.WasPressedThisFrame()) || TouchAbilityRequested);
            TouchAbilityRequested = false;
            if (abilityPressed)
            {
                TryUseAbility(place);
            }

            TickDash(dt);
            TickAbilityReadyChime();
        }

        private void LateUpdate()
        {
            if (!_session.IsMounted || _actor == null)
            {
                return;
            }

            // The hero's own stamina does not drain while the mount gallops.
            if (_stats != null)
            {
                if (_frozenHeroStamina >= 0f && _stats.CurrentStamina < _frozenHeroStamina)
                {
                    _stats.RestoreStamina(_frozenHeroStamina - _stats.CurrentStamina);
                }

                _frozenHeroStamina = _stats.CurrentStamina;
            }

            bool reduced = IsReducedMotion();
            _riderPose?.Apply(_actor.Seat.position.y, Time.unscaledTime, Time.deltaTime, reduced);
        }

        // ------------------------------------------------------------------
        // Whistle and run-in
        // ------------------------------------------------------------------

        private void OnMountPressed(MountPlace place)
        {
            if (_session.IsMounted)
            {
                Dismount(startRecall: true, leave: true);
                return;
            }

            if (_session.IsWhistling || _runningIn)
            {
                return;
            }

            OwnedMount mount = ActiveMount;
            MountSummonBlock block = _session.BeginWhistle(place, mount != null && Definition != null, mount != null && mount.IsTired,
                SecondsSinceCombat, Time.time);
            if (block != MountSummonBlock.None)
            {
                Refuse(block, mount);
                return;
            }

            MountAudio.Play(Definition.SfxCallKey, transform.position);
        }

        private void BreakWhistle()
        {
            if (!_session.IsWhistling)
            {
                return;
            }

            _session.CancelWhistle();
            Say(MountTexts.Get(Localization, MountTexts.WhistleBroken));
        }

        private void StartRunIn()
        {
            MountDefinition definition = Definition;
            if (definition == null)
            {
                return;
            }

            OwnedMount owned = ActiveMount;
            if (_actor == null || _actor.Definition != definition || _actor.OwnedGuid != owned?.guid || _actor.CosmeticSignature != MountActor.Signature(owned))
            {
                DestroyActor(ref _actor);
                Vector3 from = transform.position - transform.forward * RunInDistance + transform.right * 3f;
                from = SnapToGround(from);
                Quaternion facing = Quaternion.LookRotation(Flat(transform.position - from), Vector3.up);
                _actor = MountActor.Spawn(definition, from, facing, 1f, null, owned);
            }

            _runningIn = true;
            _runInElapsed = 0f;
        }

        private void TickRunIn(float dt)
        {
            if (!_runningIn || _actor == null)
            {
                return;
            }

            _runInElapsed += dt;
            Vector3 target = transform.position;
            float distance = Flat(target - _actor.transform.position).magnitude;
            if (distance <= MountReach || _runInElapsed >= RunInTimeout)
            {
                _runningIn = false;
                _actor.Stop();
                if (MountRules.CanRideIn(Place) && ActiveMount != null && !ActiveMount.IsTired)
                {
                    Mount();
                }

                return;
            }

            _actor.RunTo(target, RunInSpeed);
        }

        private void CancelRunIn()
        {
            _runningIn = false;
            _session.CancelWhistle();
        }

        // ------------------------------------------------------------------
        // Mount / dismount
        // ------------------------------------------------------------------

        private void Mount()
        {
            MountDefinition definition = Definition;
            OwnedMount owned = ActiveMount;
            if (definition == null || owned == null || _actor == null)
            {
                return;
            }

            _session.Mount(MountCollection.MaxStaminaOf(owned));
            _actor.AttachTo(transform, new Vector3(0f, 0f, -definition.SeatForward));
            _actor.StepSounds = true;

            if (_buffs != null && definition.DefensePercent > 0f)
            {
                _defenseBuff ??= new StatModifierDefinition();
                _defenseBuff.displayName = "mount";
                _defenseBuff.affectedStat = StatType.Defense;
                _defenseBuff.application = ModifierApplication.PercentAdditive;
                _defenseBuff.value = definition.DefensePercent;
                _defenseBuff.durationSeconds = 0f;
                _buffs.RemoveEffectsBySource(MountRules.BuffSource);
                _buffs.ApplyEffect(_defenseBuff, MountRules.BuffSource, gameObject);
            }

            Transform visual = _heroAnimator != null ? _heroAnimator.VisualRoot : null;
            if (visual != null && visual != transform)
            {
                _riderPose = new MountRiderPose(visual, transform);
                _riderPose.Begin(IsReducedMotion());
            }

            _frozenHeroStamina = _stats != null ? _stats.CurrentStamina : -1f;
            MountAudio.Play(MountAudio.MountUp, transform.position);
            _lastRidePosition = transform.position;
            Say(MountRideCare.MountLine(Localization, definition, owned));
            EventBus.Publish(new MountRiderChangedEvent(true, definition.MountId));
        }

        /// <summary>Gets off now (the HUD button, the key, crouching, a fight starting).</summary>
        public void Dismount(bool startRecall, bool leave)
        {
            if (!_session.IsMounted)
            {
                return;
            }

            _session.Dismount(Time.time, startRecall);
            _buffs?.RemoveEffectsBySource(MountRules.BuffSource);
            _riderPose?.Release();
            _riderPose = null;
            _frozenHeroStamina = -1f;
            _dashRemaining = 0f;

            MountDefinition definition = Definition;
            if (_actor != null)
            {
                _actor.Detach();
                _actor.transform.position = SnapToGround(transform.position + transform.right * 0.9f);
                if (leave)
                {
                    SendHome(_actor);
                    _actor = null;
                }
            }

            MountAudio.Play(MountAudio.Dismount, transform.position);
            EventBus.Publish(new MountRiderChangedEvent(false, definition != null ? definition.MountId : string.Empty));
        }

        /// <summary>The actor trots away and disappears (dismount, tired).</summary>
        private void SendHome(MountActor actor)
        {
            DestroyActor(ref _leaving);
            _leaving = actor;
            Vector3 away = SnapToGround(actor.transform.position + transform.right * LeaveDistance - transform.forward * 2f);
            MountActor leaving = actor;
            actor.RunTo(away, LeaveSpeed, () =>
            {
                if (leaving != null)
                {
                    Destroy(leaving.gameObject, 0.3f);
                }
            });
        }

        private void SpawnWaitingAtEdgeIfFighting()
        {
            OwnedMount mount = ActiveMount;
            MountDefinition definition = Definition;
            if (Place != MountPlace.ArenaFight || mount == null || definition == null || mount.IsTired || _actor != null)
            {
                return;
            }

            // The mount waits at the edge of the arena, behind where the hero arrived.
            Vector3 edge = SnapToGround(transform.position - transform.forward * 14f + transform.right * 4f);
            Quaternion facing = Quaternion.LookRotation(Flat(transform.position - edge), Vector3.up);
            _actor = MountActor.Spawn(definition, edge, facing, 1f, null, mount);
        }

        private void OnCollectionChanged(MountCollectionChangedEvent evt)
        {
            // Active mount swapped (or tired) while riding: get off.
            OwnedMount mount = ActiveMount;
            if (_session.IsMounted && (mount == null || mount.IsTired || (_actor != null && _actor.Definition != Definition)))
            {
                Dismount(startRecall: false, leave: true);
            }
        }

        // ------------------------------------------------------------------
        // Combat hooks
        // ------------------------------------------------------------------

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            bool tookDamage = evt.Target == gameObject;
            bool dealtDamage = evt.Source == gameObject;
            if (!tookDamage && !dealtDamage)
            {
                return;
            }

            _lastCombatTime = Time.time;
            if (tookDamage && _session.IsWhistling)
            {
                BreakWhistle();
            }

            if (!tookDamage || !_session.IsMounted)
            {
                return;
            }

            // Half of the hit (already after defense) goes to the mount instead of the hero.
            OwnedMount mount = ActiveMount;
            MountDefinition definition = Definition;
            if (mount == null || definition == null)
            {
                return;
            }

            MountCollection collection = MountCollection.ResolveShared();
            float health = MountRules.SplitDamage(evt.FinalDamage, definition.DamageShareToMount, MountCollection.HealthOf(mount), out _, out float toMount);
            if (toMount > 0f && _stats != null)
            {
                _stats.Heal(toMount);
            }

            if (collection.SetHealth(mount, health))
            {
                // Tired: the cousin rolls off unhurt, the mount trots home to rest.
                Dismount(startRecall: false, leave: true);
                Say(MountTexts.Format(Localization, MountTexts.TiredToast, MountTexts.MountName(Localization, definition)));
            }
        }

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker != gameObject)
            {
                return;
            }

            BreakWhistle();
            if (_session.IsMounted)
            {
                Dismount(startRecall: true, leave: true);
            }
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Caster != gameObject)
            {
                return;
            }

            BreakWhistle();
            if (_session.IsMounted)
            {
                Dismount(startRecall: true, leave: true);
            }
        }

        // ------------------------------------------------------------------
        // Ability
        // ------------------------------------------------------------------

        /// <summary>Fires the mount's ability (HUD/key). False when not allowed right now.</summary>
        public bool TryUseAbility() => TryUseAbility(Place);

        private bool TryUseAbility(MountPlace place)
        {
            MountDefinition definition = Definition;
            MountAbilityDefinition ability = definition != null ? definition.Ability : null;
            OwnedMount owned = ActiveMount;
            if (ability == null || owned == null || !_session.IsMounted)
            {
                return false;
            }

            if (place == MountPlace.SafeZone && !ability.AllowedInSafeZone)
            {
                Say(MountTexts.Get(Localization, MountTexts.SafeZoneNoAbility));
                MountAudio.Play(MountAudio.Refused);
                return false;
            }

            float cooldown = MountRideCare.AbilityCooldown(definition, owned);
            if (!_session.TryUseAbility(Time.time, place, cooldown, ability.AllowedInSafeZone))
            {
                MountAudio.Play(MountAudio.Refused);
                return false;
            }

            _abilityWasReady = false;
            Execute(ability);
            return true;
        }

        private void Execute(MountAbilityDefinition ability)
        {
            switch (ability.Kind)
            {
                case MountAbilityKind.Dash:
                    _dashRemaining = Mathf.Max(0.05f, ability.DurationSeconds);
                    _dashSpeed = ability.Distance / _dashRemaining;
                    MountAudio.Play(MountAudio.Dash, transform.position);
                    break;

                case MountAbilityKind.LootMagnet:
                {
                    MountAudio.Play(MountAudio.Sniff, transform.position);
                    var secrets = MountAbilityExecutor.FindSecrets(transform.position, Mathf.Max(ability.Magnitude, ability.Radius));
                    foreach (var secret in secrets)
                    {
                        CombatFxManager.SpawnLootText(secret.transform.position + Vector3.up * 1.6f, "!");
                    }

                    Say(secrets.Count > 0
                        ? MountTexts.Format(Localization, MountTexts.AbilitySecretsFound, secrets.Count)
                        : MountTexts.Get(Localization, MountTexts.AbilityNoSecrets));
                    break;
                }

                case MountAbilityKind.Fear:
                {
                    MountAudio.Play(MountAudio.Bray, transform.position);
                    int scared = MountAbilityExecutor.Scare(transform, ability);
                    Say(MountTexts.Format(Localization, MountTexts.AbilityScared, scared));
                    break;
                }

                default:
                {
                    // M3 wave 1 (MountWaveCatalog): every other kind.
                    var context = new MountAbilityContext
                    {
                        Hero = gameObject, Session = _session, Buffs = _buffs, Stats = _stats, Motor = _motor, Controller = _characterController,
                    };
                    string toast = MountWaveAbilities.Execute(ability, context);
                    if (!string.IsNullOrEmpty(toast))
                    {
                        Say(MountTexts.Format(Localization, toast, MountTexts.MountName(Localization, Definition)));
                    }

                    break;
                }
            }
        }

        private void TickDash(float dt)
        {
            if (_dashRemaining <= 0f || _characterController == null || !_characterController.enabled)
            {
                return;
            }

            float step = Mathf.Min(dt, _dashRemaining);
            _dashRemaining -= step;
            float slow = _buffs != null ? Mathf.Min(1f, _buffs.GetMoveSpeedMultiplier()) : 1f;
            _characterController.Move(Flat(transform.forward).normalized * (_dashSpeed * slow * step));
        }

        private void TickAbilityReadyChime()
        {
            bool ready = _session.AbilityRemaining(Time.time) <= 0f;
            if (ready && !_abilityWasReady && _session.IsMounted)
            {
                MountAudio.Play(Definition != null ? Definition.SfxAbilityKey : MountAudio.AbilityReady, transform.position);
            }

            _abilityWasReady = ready;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private void Refuse(MountSummonBlock block, OwnedMount mount)
        {
            LocalizationManager localization = Localization;
            string text;
            switch (block)
            {
                case MountSummonBlock.NoMount: text = MountTexts.Get(localization, MountTexts.RefusedNoMount); break;
                case MountSummonBlock.Tired:
                    text = MountTexts.Format(localization, MountTexts.RefusedTired, MountTexts.MountName(localization, Definition), mount != null ? mount.tiredUntilEncounter : 0);
                    break;
                case MountSummonBlock.InCombat: text = MountTexts.Get(localization, MountTexts.RefusedInCombat); break;
                case MountSummonBlock.RecallCooldown:
                    text = MountTexts.Format(localization, MountTexts.RefusedRecall, Mathf.CeilToInt(_session.RecallRemaining(Time.time)));
                    break;
                case MountSummonBlock.NotHere:
                    text = MountTexts.Get(localization, Place == MountPlace.ArenaFight ? MountTexts.RefusedArena : MountTexts.RefusedNotHere);
                    break;
                default: return;
            }

            MountAudio.Play(MountAudio.Refused);
            Say(text);
        }

        private void Say(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!MountNotices.Raise(text))
            {
                CombatFxManager.SpawnLootText(transform.position + Vector3.up * 2.4f, text);
            }
        }

        private void BindInput()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            InputActionAsset asset = playerInput != null ? playerInput.actions : null;
            _mountAction = asset != null ? asset.FindAction(MountActionName, throwIfNotFound: false) : null;
            _abilityAction = asset != null ? asset.FindAction(AbilityActionName, throwIfNotFound: false) : null;
            if (_mountAction == null || _abilityAction == null)
            {
                // Asset without the actions (older build): same bindings and saved rebinds, built in code.
                _ownedMap = MountInput.CreateMap(out InputAction mount, out InputAction ability);
                _mountAction ??= mount;
                _abilityAction ??= ability;
            }
        }

        private void UnbindInput()
        {
            if (_ownedMap != null)
            {
                _ownedMap.Disable();
                _ownedMap.Dispose();
                _ownedMap = null;
            }

            _mountAction = null;
            _abilityAction = null;
        }

        private Vector3 SnapToGround(Vector3 point)
        {
            RaycastHit[] hits = Physics.RaycastAll(point + Vector3.up * 20f, Vector3.down, 60f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float best = float.PositiveInfinity;
            float y = transform.position.y;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform) || hit.collider is CharacterController || hit.distance >= best)
                {
                    continue;
                }

                best = hit.distance;
                y = hit.point.y;
            }

            return new Vector3(point.x, y, point.z);
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v : Vector3.forward;
        }

        private static void DestroyActor(ref MountActor actor)
        {
            if (actor != null)
            {
                Destroy(actor.gameObject);
            }

            actor = null;
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static bool IsReducedMotion()
        {
            return ServiceLocator.TryGet(out AccessibilitySettings accessibility) && accessibility != null && accessibility.ReduceMotion;
        }
    }
}
