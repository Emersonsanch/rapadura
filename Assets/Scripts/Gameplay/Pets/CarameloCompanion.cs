using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Pets.Visuals;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Pets
{
    /// <summary>
    /// Caramelo, Vó Zefa's caramel vira-lata, trotting along with the hero in Main.unity (the encounter
    /// arenas and the standalone sandbox). Purely friendly and kid-safe: no collider, no hurtbox, nothing
    /// can target or damage him, and he never deals damage either. What he does, by priority
    /// (<see cref="CompanionMath.ChooseMode"/>):
    /// <list type="bullet">
    /// <item><b>Hide</b>: when a boss is introduced (<see cref="BossEncounterIntroEvent"/>, the "roar") or a
    /// boss winds up an attack he ducks behind the hero, ears flat, tail tucked.</item>
    /// <item><b>Alert</b>, a gentle telegraph helper: when a normal enemy near the hero starts its attack
    /// windup (<see cref="EnemyAttackWindupEvent"/>, the same moment as the <c>enemy.windup</c> sound) he
    /// barks and dashes toward it, stopping <see cref="CompanionMath.AlertStandOff"/> m short. The bark
    /// points at the threat; gameplay is untouched.</item>
    /// <item><b>Fetch</b>: loot already goes straight into the bag (<see cref="EnemyLootGranter"/>), so this is
    /// presentation only: after <see cref="EnemyLootDroppedEvent"/> he runs to the corpse, picks up a coin
    /// (gold) or a little bundle (materials), brings it to the hero and drops it with a "Caramelo trouxe!".</item>
    /// <item><b>Follow</b>: behind and beside the hero (<see cref="CompanionMath.FollowSlot"/>), eased arrival,
    /// catch-up boost, teleport when far (arena placement); sits after a moment still, sniffs around.</item>
    /// </list>
    /// Motion reuses the hero's feel math (<see cref="MotionFeelMath"/>): spring-eased velocity, trot gait
    /// with body bob, idle breathing, follow-through springs on the ears and the two-part tail, volume-
    /// preserving squash on barks, idle glances. "Reduzir Movimento" halves the flourishes. Scaled time, so
    /// pauses and hit-stop freeze him too. Sounds go through the <see cref="SfxEventRouter"/> cues
    /// <c>pet.bark</c>, <c>pet.sniff</c>, <c>pet.fetch</c> and <c>pet.yip</c>.
    ///
    /// Self-bootstrapping (no scene edit): after every scene load, if the scene has a
    /// <see cref="PlayerController"/> and <see cref="CompanionPreferences.ShouldAccompany"/> allows it (option on;
    /// in a real encounter only once the prologue was seen) one Caramelo is spawned in that scene, so he
    /// unloads with it. Turning the option off removes him live; on brings him back. A pending cuddle buff
    /// (<see cref="CarameloCuddle"/>) is applied to the hero when he arrives in an encounter.
    /// </summary>
    public class CarameloCompanion : MonoBehaviour
    {
        public const string GameObjectName = "Caramelo";
        public const string BarkCue = "pet.bark";
        public const string SniffCue = "pet.sniff";
        public const string FetchCue = "pet.fetch";
        public const string YipCue = "pet.yip";

        private const float WalkSpeed = 4.6f;
        private const float DashSpeed = 7.5f;
        private const float AlertCooldown = 2.5f;
        private const float AlertLinger = 0.8f;
        private const float BarkInterval = 0.45f;
        private const int MaxBarks = 3;
        private const float RoarHideSeconds = 3.5f;
        private const float FetchLifetime = 10f;
        private const float PickupRadius = 0.5f;
        private const float DeliverRadius = 1.1f;
        private const float HappySeconds = 1.6f;
        private const float GroundProbeHeight = 1.5f;
        private const float GroundProbeLength = 4f;

        private static CarameloCompanion _instance;
        private static readonly RaycastHit[] GroundHits = new RaycastHit[8];

        /// <summary>The live Caramelo, or null.</summary>
        public static CarameloCompanion Instance => _instance;

        private Transform _hero;
        private Transform _visual;
        private AccessibilitySettings _accessibility;
        private int _seed;

        // Parts and their rest poses.
        private Transform _body, _head, _earLeft, _earRight, _tail, _tailTip, _tongue, _carryGold, _carryBundle;
        private readonly Transform[] _paws = new Transform[4];
        private readonly Quaternion[] _pawRest = new Quaternion[4];
        private Vector3 _bodyRestPosition;
        private Quaternion _bodyRest, _headRest, _earLeftRest, _earRightRest, _tailRest, _tailTipRest;

        // Locomotion.
        private CompanionMode _mode;
        private Vector2 _velocity;
        private Vector2 _velocityRate;
        private float _yaw;
        private float _yawRate;
        private float _side = 1f;
        private float _idleTime;
        private Vector3 _lastHeroPosition;
        private float _heroSpeed;
        private float _groundY;
        private bool _hasGround;

        // Behaviours.
        private float _time;
        private float _hideUntil = -1f;
        private GameObject _alertEnemy;
        private float _alertUntil = -1f;
        private float _alertCooldownUntil = -1f;
        private float _nextBark;
        private int _barks;
        private readonly List<Vector3> _windupPositions = new List<Vector3>(4);
        private readonly List<GameObject> _windupEnemies = new List<GameObject>(4);
        private readonly List<float> _windupSeconds = new List<float>(4);
        private readonly List<Vector3> _fetchSpots = new List<Vector3>(8);
        private readonly List<float> _fetchExpiry = new List<float>(8);
        private readonly List<bool> _fetchGold = new List<bool>(8);
        private bool _carrying;
        private float _happyUntil = -1f;
        private bool _wasSniffing;

        // Feel springs.
        private float _gaitPhase;
        private float _sit, _sitVelocity;
        private float _crouch, _crouchVelocity;
        private Vector2 _ears, _earsVelocity;
        private float _tailSwing, _tailSwingVelocity;
        private float _tailTipSwing, _tailTipSwingVelocity;
        private float _barkKick, _barkKickVelocity;
        private float _pant;

        public CompanionMode Mode => _mode;
        public bool IsCarrying => _carrying;
        public int PendingFetchCount => _fetchSpots.Count;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            CompanionPreferences.Changed -= OnPreferencesChanged;
            CompanionPreferences.Changed += OnPreferencesChanged;
            TrySpawn();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TrySpawn();

        private static void OnPreferencesChanged()
        {
            if (!CompanionPreferences.CarameloEnabled)
            {
                if (_instance != null)
                {
                    Destroy(_instance.gameObject);
                    _instance = null;
                }

                return;
            }

            TrySpawn();
        }

        /// <summary>Spawns Caramelo next to the scene's hero when allowed and not already there.</summary>
        public static CarameloCompanion TrySpawn()
        {
            if (_instance != null)
            {
                return _instance;
            }

            PlayerController hero = FindFirstObjectByType<PlayerController>();
            if (hero == null)
            {
                return null;
            }

            bool inEncounter = ServiceLocator.TryGet(out SceneTransitionService transitions) && transitions != null && transitions.HasPendingEncounter;
            bool prologueSeen = ServiceLocator.TryGet(out StoryProgress story) && story != null
                                && (story.HasSeen(StoryBeat.Prologue) || story.CampaignComplete);
            if (!CompanionPreferences.ShouldAccompany(CompanionPreferences.CarameloEnabled, inEncounter, prologueSeen))
            {
                return null;
            }

            var go = new GameObject(GameObjectName);
            // Lives and unloads with the hero's scene (Main.unity), not DontDestroyOnLoad.
            SceneManager.MoveGameObjectToScene(go, hero.gameObject.scene);
            var companion = go.AddComponent<CarameloCompanion>();
            companion.Bind(hero.transform);
            if (inEncounter)
            {
                companion.ApplyPendingCuddleBuff();
            }

            return companion;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _seed = 7;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyAttackWindupEvent>(OnEnemyWindup);
            EventBus.Subscribe<EnemyLootDroppedEvent>(OnLootDropped);
            EventBus.Subscribe<BossEncounterIntroEvent>(OnBossIntro);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyAttackWindupEvent>(OnEnemyWindup);
            EventBus.Unsubscribe<EnemyLootDroppedEvent>(OnLootDropped);
            EventBus.Unsubscribe<BossEncounterIntroEvent>(OnBossIntro);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>Attaches to <paramref name="hero"/>, builds the model and drops him at his follow slot.</summary>
        public void Bind(Transform hero)
        {
            _hero = hero;
            if (_visual == null)
            {
                _visual = CarameloModel.Instantiate(transform).transform;
                CacheParts();
            }

            if (_hero != null)
            {
                _lastHeroPosition = _hero.position;
                SnapToSlot();
            }
        }

        private void ApplyPendingCuddleBuff()
        {
            if (_hero == null || !CarameloCuddle.TryConsumeBuff())
            {
                return;
            }

            BuffController buffs = _hero.GetComponent<BuffController>();
            if (buffs == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string name = CarameloTexts.Get(localization, CarameloTexts.BuffName);
            buffs.ApplyEffect(CarameloCuddle.Buff(name), CarameloCuddle.BuffSource, gameObject);
            CombatFxManager.SpawnLootText(_hero.position + Vector3.up * 2.2f, name + " +5%");
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        private void OnEnemyWindup(EnemyAttackWindupEvent evt)
        {
            if (evt.Enemy == null)
            {
                return;
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            if (enemy != null && enemy.IsBoss)
            {
                // Kid-friendly: he never runs at a boss; he ducks behind the hero until the swing is over.
                _hideUntil = Mathf.Max(_hideUntil, _time + evt.WindupSeconds + 0.6f);
                return;
            }

            _windupPositions.Add(evt.Position);
            _windupEnemies.Add(evt.Enemy);
            _windupSeconds.Add(evt.WindupSeconds);
        }

        private void OnLootDropped(EnemyLootDroppedEvent evt)
        {
            if (evt.Enemy == null || evt.Drops == null || evt.Drops.Count == 0)
            {
                return;
            }

            bool gold = false;
            foreach (LootDrop drop in evt.Drops)
            {
                if (drop.Item != null && (drop.Item.Type == ItemType.Currency || drop.Item.Category == ItemCategory.Currency))
                {
                    gold = true;
                    break;
                }
            }

            _fetchSpots.Add(evt.Enemy.transform.position);
            _fetchExpiry.Add(_time + FetchLifetime);
            _fetchGold.Add(gold);
        }

        private void OnBossIntro(BossEncounterIntroEvent evt)
        {
            _hideUntil = Mathf.Max(_hideUntil, _time + RoarHideSeconds);
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        private void Update()
        {
            if (_hero == null)
            {
                Destroy(gameObject);
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            _time += dt;
            Vector3 heroPosition = _hero.position;
            _heroSpeed = Mathf.Lerp(_heroSpeed, CompanionMath.PlanarDistance(heroPosition, _lastHeroPosition) / dt, 1f - Mathf.Exp(-8f * dt));
            _lastHeroPosition = heroPosition;

            if (CompanionMath.ShouldTeleport(transform.position, heroPosition))
            {
                SnapToSlot();
                return;
            }

            ExpireFetches();
            ConsumeWindups(heroPosition);

            bool hiding = _time < _hideUntil;
            bool alerting = _alertEnemy != null && _time < _alertUntil && IsAlive(_alertEnemy);
            int fetchIndex = _carrying ? -1 : CompanionMath.SelectFetch(_fetchSpots, heroPosition, transform.position);
            _mode = CompanionMath.ChooseMode(hiding, alerting, _carrying || fetchIndex >= 0);

            Vector3 target;
            float maxSpeed;
            float stopRadius = CompanionMath.StopRadius;
            switch (_mode)
            {
                case CompanionMode.Hide:
                    target = heroPosition - Flat(_hero.forward).normalized * 0.75f;
                    maxSpeed = DashSpeed;
                    stopRadius = 0.15f;
                    break;
                case CompanionMode.Alert:
                    target = CompanionMath.AlertStandPoint(_alertEnemy.transform.position, heroPosition);
                    maxSpeed = DashSpeed;
                    TickBarks();
                    break;
                case CompanionMode.Fetch:
                    if (_carrying)
                    {
                        Vector3 away = Flat(transform.position - heroPosition);
                        target = heroPosition + (away.sqrMagnitude > 1e-4f ? away.normalized : Flat(_hero.forward).normalized) * 0.8f;
                        maxSpeed = WalkSpeed * 1.2f;
                        if (CompanionMath.PlanarDistance(transform.position, heroPosition) <= DeliverRadius)
                        {
                            Deliver();
                        }
                    }
                    else
                    {
                        target = _fetchSpots[fetchIndex];
                        maxSpeed = DashSpeed * 0.9f;
                        stopRadius = 0.1f;
                        if (CompanionMath.PlanarDistance(transform.position, target) <= PickupRadius)
                        {
                            PickUp(fetchIndex);
                        }
                    }

                    break;
                default:
                    ChooseSide(heroPosition);
                    target = CompanionMath.FollowSlot(heroPosition, _hero.forward, _side);
                    maxSpeed = CompanionMath.CatchUpSpeed(CompanionMath.PlanarDistance(transform.position, heroPosition), _heroSpeed, WalkSpeed);
                    break;
            }

            Move(target, maxSpeed, stopRadius, dt);
            Face(heroPosition, dt);
            Animate(dt);
        }

        private void ConsumeWindups(Vector3 heroPosition)
        {
            if (_windupPositions.Count == 0)
            {
                return;
            }

            if (_time >= _alertCooldownUntil && _time >= _hideUntil)
            {
                int index = CompanionMath.SelectAlert(_windupPositions, heroPosition);
                if (index >= 0 && IsAlive(_windupEnemies[index]))
                {
                    _alertEnemy = _windupEnemies[index];
                    _alertUntil = _time + _windupSeconds[index] + AlertLinger;
                    _alertCooldownUntil = _time + AlertCooldown;
                    _barks = 0;
                    _nextBark = _time;
                }
            }

            _windupPositions.Clear();
            _windupEnemies.Clear();
            _windupSeconds.Clear();
        }

        private void TickBarks()
        {
            if (_barks >= MaxBarks || _time < _nextBark)
            {
                return;
            }

            _barks++;
            _nextBark = _time + BarkInterval;
            _barkKickVelocity += MotionFeelMath.SpringVelocityForPeak(1f, 5f, 0.4f);
            PlayCue(BarkCue);
        }

        private void PickUp(int index)
        {
            _carrying = true;
            bool gold = _fetchGold[index];
            _fetchSpots.RemoveAt(index);
            _fetchExpiry.RemoveAt(index);
            _fetchGold.RemoveAt(index);
            SetVisible(_carryGold, gold);
            SetVisible(_carryBundle, !gold);
            PlayCue(SniffCue);
        }

        private void Deliver()
        {
            _carrying = false;
            SetVisible(_carryGold, false);
            SetVisible(_carryBundle, false);
            _happyUntil = _time + HappySeconds;
            PlayCue(FetchCue);
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootText(transform.position + Vector3.up * 1.1f, CarameloTexts.Get(localization, CarameloTexts.Fetched));
        }

        private void ExpireFetches()
        {
            for (int i = _fetchSpots.Count - 1; i >= 0; i--)
            {
                if (_time >= _fetchExpiry[i])
                {
                    _fetchSpots.RemoveAt(i);
                    _fetchExpiry.RemoveAt(i);
                    _fetchGold.RemoveAt(i);
                }
            }
        }

        /// <summary>Switches sides only when the other slot is clearly closer (never cuts through the hero for nothing).</summary>
        private void ChooseSide(Vector3 heroPosition)
        {
            Vector3 here = transform.position;
            float current = CompanionMath.PlanarDistance(here, CompanionMath.FollowSlot(heroPosition, _hero.forward, _side));
            float other = CompanionMath.PlanarDistance(here, CompanionMath.FollowSlot(heroPosition, _hero.forward, -_side));
            if (other + 1f < current)
            {
                _side = -_side;
            }
        }

        private void Move(Vector3 target, float maxSpeed, float stopRadius, float dt)
        {
            Vector3 position = transform.position;
            Vector3 toTarget = Flat(target - position);
            float distance = toTarget.magnitude;
            float speed = CompanionMath.ArrivalSpeed(distance, maxSpeed, stopRadius);
            Vector2 desired = distance > 1e-4f ? new Vector2(toTarget.x, toTarget.z) / distance * speed : Vector2.zero;

            // Critically damped ease on the velocity: starts and stops have weight, no snapping.
            MotionFeelMath.StepSpring(ref _velocity, ref _velocityRate, desired, MotionFeelMath.EaseFrequency(0.22f), 1f, dt, Vector2.zero);
            position.x += _velocity.x * dt;
            position.z += _velocity.y * dt;

            float groundY = ProbeGround(position, out bool hit);
            if (hit)
            {
                _groundY = _hasGround ? Mathf.Lerp(_groundY, groundY, 1f - Mathf.Exp(-20f * dt)) : groundY;
                _hasGround = true;
            }
            else
            {
                _groundY = _hero.position.y;
            }

            position.y = _groundY;
            transform.position = position;

            _idleTime = _velocity.magnitude < 0.15f ? _idleTime + dt : 0f;
            if (_velocity.magnitude > 4f)
            {
                _pant = Mathf.Min(4f, _pant + dt * 2f);
            }
            else
            {
                _pant = Mathf.Max(0f, _pant - dt * 0.5f);
            }
        }

        private void Face(Vector3 heroPosition, float dt)
        {
            float targetYaw = _yaw;
            if (_velocity.magnitude > 0.3f)
            {
                targetYaw = Mathf.Atan2(_velocity.x, _velocity.y) * Mathf.Rad2Deg;
            }
            else if (_mode == CompanionMode.Alert && _alertEnemy != null)
            {
                targetYaw = YawTo(_alertEnemy.transform.position);
            }
            else if (_mode == CompanionMode.Hide)
            {
                targetYaw = _hero.eulerAngles.y;
            }
            else if (CompanionMath.PlanarDistance(transform.position, heroPosition) > 0.5f)
            {
                targetYaw = YawTo(heroPosition);
            }

            float previous = _yaw;
            _yaw = Mathf.MoveTowardsAngle(_yaw, targetYaw, 540f * dt);
            _yawRate = Mathf.DeltaAngle(previous, _yaw) / dt;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        // ------------------------------------------------------------------
        // Procedural animation
        // ------------------------------------------------------------------

        private void Animate(float dt)
        {
            if (_body == null)
            {
                return;
            }

            float motion = MotionScale();
            float speed = _velocity.magnitude;
            float speed01 = Mathf.Clamp01(speed / DashSpeed);
            bool hiding = _mode == CompanionMode.Hide;
            bool happy = _time < _happyUntil;

            // Trot: diagonal pairs swing together; stride frequency rises with speed.
            _gaitPhase = Mathf.Repeat(_gaitPhase + dt * Mathf.Lerp(1.6f, 3.4f, speed01) * Mathf.Clamp01(speed / 0.6f), 1f);
            float swing = Mathf.Sin(_gaitPhase * Mathf.PI * 2f) * Mathf.Lerp(0f, 34f, Mathf.Clamp01(speed / 3f));

            bool sitting = CompanionMath.ShouldSit(_mode, _idleTime) && !_carrying;
            float sitFrequency = MotionFeelMath.EaseFrequency(0.35f);
            MotionFeelMath.StepSpring(ref _sit, ref _sitVelocity, sitting ? 1f : 0f, sitFrequency, 1f, dt);
            MotionFeelMath.StepSpring(ref _crouch, ref _crouchVelocity, hiding ? 1f : 0f, MotionFeelMath.EaseFrequency(0.18f), 1f, dt);
            MotionFeelMath.StepSpring(ref _barkKick, ref _barkKickVelocity, 0f, 5f, 0.4f, dt);

            // Body: trot bob, breathing (faster while panting), sit tilt, crouch.
            float bob = Mathf.Abs(Mathf.Sin(_gaitPhase * Mathf.PI * 2f)) * 0.025f * Mathf.Clamp01(speed / 2f) * motion;
            float breathRate = _pant > 0.5f ? 2.6f : 0.45f;
            float breath = Mathf.Sin(_time * Mathf.PI * 2f * breathRate) * (_pant > 0.5f ? 0.02f : 0.012f) * motion;
            Vector3 bodyOffset = new Vector3(0f, bob - 0.09f * _sit - 0.1f * _crouch, -0.07f * _sit);
            _body.localPosition = _bodyRestPosition + bodyOffset;
            _body.localRotation = _bodyRest * Quaternion.Euler(-24f * _sit + 6f * _crouch - 4f * _barkKick, 0f, 0f);
            _body.localScale = MotionFeelMath.SquashScale(breath + 0.04f * _barkKick * motion);

            // Legs: trot swing; sitting folds the back legs forward and the fronts plant upright.
            for (int i = 0; i < _paws.Length; i++)
            {
                if (_paws[i] == null)
                {
                    continue;
                }

                bool back = i >= 2;
                bool diagonalA = i == 0 || i == 3;
                float legSwing = (diagonalA ? swing : -swing) * (1f - _sit);
                float fold = back ? 65f * _sit : 10f * _sit;
                _paws[i].localRotation = _pawRest[i] * Quaternion.Euler(legSwing - fold + 12f * _crouch, 0f, 0f);
            }

            // Head: sniff dips the nose, idle glances look around, alert/bark lifts it, hiding lowers it.
            float sniff = _mode == CompanionMode.Follow ? CompanionMath.SniffWeight(_idleTime, 4.5f, _seed) : 0f;
            if (sniff > 0.05f && !_wasSniffing)
            {
                PlayCue(SniffCue);
            }

            _wasSniffing = sniff > 0.05f;
            float glance = _mode == CompanionMode.Follow ? MotionFeelMath.IdleGlanceYaw(_idleTime, 3.2f, 0.9f, 32f, _seed) : 0f;
            float headPitch = 28f * sniff - 14f * (_mode == CompanionMode.Alert ? 1f : 0f) - 12f * _barkKick + 16f * _crouch;
            _head.localRotation = _headRest * Quaternion.Euler(headPitch + 24f * _sit, glance * (1f - sniff), 0f);

            // Ears: underdamped spring driven by acceleration and the bob (they lag behind a start and fling forward
            // on a stop: follow-through); swept back flat when hiding. Positive pitch = back.
            Vector2 accel = _velocityRate;
            Vector3 localAccel = Quaternion.Euler(0f, -_yaw, 0f) * new Vector3(accel.x, 0f, accel.y);
            Vector2 earTarget = new Vector2(Mathf.Clamp(localAccel.z * 2.2f, -25f, 25f) + bob * 400f + 50f * _crouch,
                Mathf.Clamp(_yawRate * 0.05f, -20f, 20f));
            MotionFeelMath.StepSpring(ref _ears, ref _earsVelocity, earTarget, 3.2f, 0.3f, dt, Vector2.zero);
            Vector2 ears = Vector2.Lerp(earTarget, _ears, motion);
            if (_earLeft != null) _earLeft.localRotation = _earLeftRest * Quaternion.Euler(ears.x, 0f, -ears.y * 0.5f - 6f * _barkKick);
            if (_earRight != null) _earRight.localRotation = _earRightRest * Quaternion.Euler(ears.x, 0f, ears.y * 0.5f + 6f * _barkKick);

            // Tail: wag, plus a follow-through spring against turns; the tip lags the base.
            float wagFrequency = CompanionMath.TailWagFrequency(speed, happy || _carrying, hiding);
            float wagAmplitude = happy ? 38f : Mathf.Lerp(16f, 26f, speed01) * (1f - 0.5f * _sit);
            float wag = wagFrequency > 0f ? Mathf.Sin(_time * Mathf.PI * 2f * wagFrequency) * wagAmplitude : 0f;
            MotionFeelMath.StepSpring(ref _tailSwing, ref _tailSwingVelocity, wag - _yawRate * 0.08f, 4f, 0.35f, dt);
            MotionFeelMath.StepSpring(ref _tailTipSwing, ref _tailTipSwingVelocity, _tailSwing, 3f, 0.3f, dt);
            float tailSwing = Mathf.Lerp(wag, _tailSwing, motion);
            float tipSwing = Mathf.Lerp(tailSwing, _tailTipSwing, motion) - tailSwing;
            // Negative pitch swings the tail down/back: tucked when hiding, lying behind when sitting, streaming back at a run.
            float tailPitch = -80f * _crouch - 40f * _sit - 15f * speed01;
            if (_tail != null) _tail.localRotation = _tailRest * Quaternion.Euler(tailPitch, 0f, tailSwing);
            if (_tailTip != null) _tailTip.localRotation = _tailTipRest * Quaternion.Euler(0f, 0f, tipSwing * 0.8f);

            SetVisible(_tongue, (_pant > 0.5f || happy) && !_carrying && !hiding);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private void CacheParts()
        {
            _body = Find(CarameloParts.Body);
            _head = Find(CarameloParts.Head);
            _earLeft = Find(CarameloParts.EarLeft);
            _earRight = Find(CarameloParts.EarRight);
            _tail = Find(CarameloParts.Tail);
            _tailTip = Find(CarameloParts.TailTip);
            _tongue = Find(CarameloParts.Tongue);
            _carryGold = Find(CarameloParts.CarryGold);
            _carryBundle = Find(CarameloParts.CarryBundle);
            _paws[0] = Find(CarameloParts.PawFrontLeft);
            _paws[1] = Find(CarameloParts.PawFrontRight);
            _paws[2] = Find(CarameloParts.PawBackLeft);
            _paws[3] = Find(CarameloParts.PawBackRight);

            if (_body == null || _head == null)
            {
                _body = null;
                return;
            }

            _bodyRestPosition = _body.localPosition;
            _bodyRest = _body.localRotation;
            _headRest = _head.localRotation;
            _earLeftRest = _earLeft != null ? _earLeft.localRotation : Quaternion.identity;
            _earRightRest = _earRight != null ? _earRight.localRotation : Quaternion.identity;
            _tailRest = _tail != null ? _tail.localRotation : Quaternion.identity;
            _tailTipRest = _tailTip != null ? _tailTip.localRotation : Quaternion.identity;
            for (int i = 0; i < _paws.Length; i++)
            {
                _pawRest[i] = _paws[i] != null ? _paws[i].localRotation : Quaternion.identity;
            }

            SetVisible(_carryGold, false);
            SetVisible(_carryBundle, false);
            SetVisible(_tongue, false);
        }

        private Transform Find(string partName)
        {
            foreach (Transform t in _visual.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == partName)
                {
                    return t;
                }
            }

            return null;
        }

        private void SnapToSlot()
        {
            Vector3 slot = CompanionMath.FollowSlot(_hero.position, _hero.forward, _side);
            float groundY = ProbeGround(slot, out bool hit);
            slot.y = hit ? groundY : _hero.position.y;
            _groundY = slot.y;
            _hasGround = hit;
            transform.position = slot;
            _velocity = Vector2.zero;
            _velocityRate = Vector2.zero;
            _yaw = _hero.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        /// <summary>Ground height under <paramref name="position"/>, ignoring the hero and every combatant (allocation-free).</summary>
        private float ProbeGround(Vector3 position, out bool hit)
        {
            Vector3 origin = new Vector3(position.x, (_hero != null ? _hero.position.y : position.y) + GroundProbeHeight, position.z);
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, GroundHits, GroundProbeLength, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            float y = position.y;
            hit = false;
            for (int i = 0; i < count; i++)
            {
                Collider c = GroundHits[i].collider;
                if (c == null || (_hero != null && c.transform.IsChildOf(_hero)) || c.GetComponentInParent<Health>() != null)
                {
                    continue;
                }

                if (GroundHits[i].distance < best)
                {
                    best = GroundHits[i].distance;
                    y = GroundHits[i].point.y;
                    hit = true;
                }
            }

            return y;
        }

        private float MotionScale()
        {
            if (_accessibility == null)
            {
                ServiceLocator.TryGet(out _accessibility);
            }

            return AccessibilitySettings.MotionScaleFor(_accessibility);
        }

        private void PlayCue(string cueId)
        {
            if (ServiceLocator.TryGet(out SfxEventRouter router) && router != null && router.IsInitialized)
            {
                router.PlayCue(cueId, transform.position);
            }
        }

        private float YawTo(Vector3 point)
        {
            Vector3 d = Flat(point - transform.position);
            return d.sqrMagnitude < 1e-6f ? _yaw : Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        }

        private static bool IsAlive(GameObject enemy)
        {
            if (enemy == null || !enemy.activeInHierarchy)
            {
                return false;
            }

            Health health = enemy.GetComponent<Health>();
            return health == null || !health.IsDead;
        }

        private static void SetVisible(Transform part, bool visible)
        {
            if (part != null && part.gameObject.activeSelf != visible)
            {
                part.gameObject.SetActive(visible);
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
