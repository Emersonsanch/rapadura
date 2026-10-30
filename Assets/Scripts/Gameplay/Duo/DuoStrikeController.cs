using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Skills.Vfx;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>Raised when a duo strike starts (the partner appears): the HUD shows the combo name and the shout.</summary>
    public readonly struct DuoStrikeStartedEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly CharacterId Partner;
        public readonly DuoComboSpec Combo;
        public readonly bool Fade;

        public DuoStrikeStartedEvent(CharacterId hero, CharacterId partner, DuoComboSpec combo, bool fade)
        {
            Hero = hero;
            Partner = partner;
            Combo = combo;
            Fade = fade;
        }
    }

    /// <summary>Raised when the combined move lands.</summary>
    public readonly struct DuoStrikeImpactEvent : IGameEvent
    {
        public readonly DuoComboSpec Combo;
        public readonly Vector3 Center;
        public readonly int EnemiesHit;
        public readonly float Damage;

        public DuoStrikeImpactEvent(DuoComboSpec combo, Vector3 center, int enemiesHit, float damage)
        {
            Combo = combo;
            Center = center;
            EnemiesHit = enemiesHit;
            Damage = damage;
        }
    }

    /// <summary>
    /// "Golpe em dupla" in the arena. Self-bootstrapping, no scene edit (same pattern as <c>MealEffectApplier</c>):
    /// after every scene load that has a hero (<see cref="PlayerStats"/>) an instance is created in that scene and
    /// dies with it, so each encounter starts a fresh <see cref="DuoStrikeMeter"/> (it is also reset on
    /// <see cref="EncounterIntroEvent"/>).
    /// <list type="bullet">
    /// <item>Charge: <see cref="DamageAppliedEvent"/> dealt by the hero (basic attack, skills, DoTs credited to them)
    /// or taken by the hero, scaled by <see cref="DuoStrikeRules"/>.</item>
    /// <item>Trigger: the <see cref="InputActionName"/> action of PlayerControls.inputactions (F / right stick press,
    /// rebindable in Configurações → Controles) or the HUD button (<c>DuoStrikeHud</c> → <see cref="TryTrigger"/>).
    /// Blocked while paused, while the hero is stunned or down, or with no enemy alive.</item>
    /// <item>Strike: the partner (<see cref="DuoPartnerState"/>) appears as a <see cref="DuoPartnerSilhouette"/> beside
    /// the hero, the hero swings (<see cref="PlayerAttackPerformedEvent"/>, the animators' public hook), and after
    /// <see cref="DuoStrikeRules.ImpactDelaySeconds"/> the combo of the pair (<see cref="DuoComboCatalog"/>) lands
    /// around the nearest enemy.</item>
    /// </list>
    /// </summary>
    public class DuoStrikeController : MonoBehaviour
    {
        public const string InputActionName = "DuoStrike";
        private const string PlayerTag = "Player";
        private const float ResolveInterval = 0.5f;

        private static DuoStrikeController _active;

        /// <summary>The live controller of the loaded arena (null outside one).</summary>
        public static DuoStrikeController Active => _active;

        private readonly DuoStrikeMeter _meter = new DuoStrikeMeter();
        private GameObject _player;
        private PlayerStats _stats;
        private BuffController _heroBuffs;
        private InputAction _action;
        private ProgressionCurve _curve;
        private PlayerCombatBalance _balance;
        private float _nextResolve;
        private bool _applyingStrike;

        public DuoStrikeMeter Meter => _meter;

        public bool IsExecuting => _meter.IsExecuting;

        /// <summary>The hero being played, if one was chosen.</summary>
        public static CharacterId? CurrentHero => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        /// <summary>The cousin the current hero would call.</summary>
        public static CharacterId? CurrentPartner
        {
            get
            {
                CharacterId? hero = CurrentHero;
                return hero.HasValue ? DuoPartnerState.ResolveShared().GetPartnerFor(hero.Value) : null;
            }
        }

        /// <summary>The combo the current hero + partner would perform.</summary>
        public static DuoComboSpec CurrentCombo
        {
            get
            {
                CharacterId? hero = CurrentHero;
                CharacterId? partner = CurrentPartner;
                return hero.HasValue && partner.HasValue ? DuoComboCatalog.Find(hero.Value, partner.Value) : null;
            }
        }

        /// <summary>Whether there is something to fight (the HUD hides the button otherwise).</summary>
        public static bool HasEnemies => EnemyController.CountAlive() > 0;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachIfHeroScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachIfHeroScene(scene);

        private static void AttachIfHeroScene(Scene loaded)
        {
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats == null)
            {
                return;
            }

            // One per hero scene (an additive scene loading next to the arena must not add a second one).
            Scene heroScene = stats.gameObject.scene;
            foreach (DuoStrikeController existing in FindObjectsByType<DuoStrikeController>(FindObjectsSortMode.None))
            {
                if (existing.gameObject.scene == heroScene)
                {
                    return;
                }
            }

            var host = new GameObject(nameof(DuoStrikeController));
            if (stats.gameObject.scene.buildIndex == -1 && stats.transform.root.gameObject.scene.name == "DontDestroyOnLoad")
            {
                DontDestroyOnLoad(host);
            }
            else if (heroScene.IsValid() && heroScene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(host, heroScene);
            }

            host.AddComponent<DuoStrikeController>();
        }

        private void Awake()
        {
            // The newest arena wins; an older instance only clears Active if it is still the current one.
            _active = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<EncounterIntroEvent>(OnEncounterIntro);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<EncounterIntroEvent>(OnEncounterIntro);
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }
        }

        private void OnEncounterIntro(EncounterIntroEvent evt)
        {
            if (!_meter.IsExecuting)
            {
                _meter.BeginEncounter();
            }
        }

        // ------------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------------

        private static bool IsGameplayFrozen => GameplayPause.IsPaused || Time.timeScale == 0f;

        private void Update()
        {
            ResolvePlayer();
            if (IsGameplayFrozen)
            {
                return;
            }

            _meter.Tick(Time.deltaTime);
            if (_action != null && _action.WasPressedThisFrame())
            {
                TryTrigger();
            }
        }

        private void ResolvePlayer()
        {
            if (_player != null || Time.unscaledTime < _nextResolve)
            {
                return;
            }

            _nextResolve = Time.unscaledTime + ResolveInterval;
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null || !player.TryGetComponent(out PlayerStats stats))
            {
                return;
            }

            _player = player;
            _stats = stats;
            _heroBuffs = player.GetComponent<BuffController>();
            PlayerInput input = player.GetComponent<PlayerInput>();
            _action = input != null && input.actions != null ? input.actions.FindAction(InputActionName, throwIfNotFound: false) : null;
        }

        // ------------------------------------------------------------------
        // Charge
        // ------------------------------------------------------------------

        private bool IsHero(GameObject candidate)
        {
            return candidate != null && _player != null && (candidate == _player || candidate.transform.IsChildOf(_player.transform));
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (_player == null || _applyingStrike || evt.FinalDamage <= 0f || evt.Target == null)
            {
                return;
            }

            if (IsHero(evt.Target))
            {
                _meter.AddTaken(evt.FinalDamage, _stats != null ? _stats.MaxHealth : 0f);
                return;
            }

            if (!IsHero(evt.Source))
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponentInParent<EnemyController>();
            Health health = enemy != null && enemy.Health != null ? enemy.Health : evt.Target.GetComponentInParent<Health>();
            if (health != null)
            {
                _meter.AddDealt(evt.FinalDamage, health.MaxHealth, enemy != null && enemy.IsBoss);
            }
        }

        // ------------------------------------------------------------------
        // Trigger
        // ------------------------------------------------------------------

        /// <summary>What the HUD shows: the meter's readiness, or "not available" (no hero / partner / enemies) as null.</summary>
        public DuoStrikeReadiness? CurrentReadiness => CurrentCombo != null ? _meter.Readiness : (DuoStrikeReadiness?)null;

        /// <summary>Display string of the first (keyboard) binding of the action, e.g. "F" (empty without one).</summary>
        public string KeyHint => _action != null && _action.bindings.Count > 0 ? _action.GetBindingDisplayString(0) : string.Empty;

        /// <summary>Whether the hero is able to act (alive, not stunned).</summary>
        private bool HeroCanAct => _stats != null && _stats.IsAlive && (_heroBuffs == null || !_heroBuffs.IsStunned);

        /// <summary>Calls the partner when everything allows it (full meter, armed, hero free, an enemy around, not paused).</summary>
        public bool TryTrigger()
        {
            CharacterId? hero = CurrentHero;
            CharacterId? partner = CurrentPartner;
            DuoComboSpec combo = hero.HasValue && partner.HasValue ? DuoComboCatalog.Find(hero.Value, partner.Value) : null;
            if (combo == null || _player == null || !HeroCanAct || IsGameplayFrozen || !HasEnemies || !_meter.TryBegin())
            {
                return false;
            }

            StartCoroutine(Execute(hero.Value, partner.Value, combo));
            return true;
        }

        private IEnumerator Execute(CharacterId hero, CharacterId partner, DuoComboSpec combo)
        {
            ServiceLocator.TryGet(out AccessibilitySettings accessibility);
            bool reduceFlashes = accessibility != null && accessibility.ReduceFlashEffects;
            bool reduceMotion = accessibility != null && accessibility.ReduceMotion;
            DuoStrikePresentation look = DuoStrikePresentation.Resolve(combo.IsSiblingBond, reduceFlashes, reduceMotion);

            Transform heroTransform = _player.transform;
            EnemyController focus = FindFocus(heroTransform.position);
            Vector3 center = focus != null ? focus.transform.position : heroTransform.position + heroTransform.forward * 2f;
            Vector3 facing = center - heroTransform.position;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.01f)
            {
                facing = heroTransform.forward;
            }

            facing.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, facing).normalized;
            Vector3 spawnAt = heroTransform.position + side * 1.1f - facing * 0.2f;
            Color tint = DuoStrikePresentation.SilhouetteColor(CharacterRegistry.Get(partner).AccentColor, combo.IsSiblingBond);
            DuoPartnerSilhouette.Spawn(spawnAt, facing, HeroHeight(), tint, look, DuoStrikeRules.StrikeDurationSeconds, DuoStrikeRules.ImpactDelaySeconds);

            EventBus.Publish(new DuoStrikeStartedEvent(hero, partner, combo, look.BannerFade));
            EventBus.Publish(new PlayerAttackPerformedEvent(_player));

            yield return WaitGameSeconds(DuoStrikeRules.ImpactDelaySeconds);

            if (focus != null && focus.Health != null && focus.Health.IsAlive)
            {
                center = focus.transform.position;
            }

            if (_player != null)
            {
                ApplyImpact(combo, center, look);
            }

            if (look.SecondRing)
            {
                yield return WaitGameSeconds(0.15f);
                SkillVfxManager.PlayAreaBurst(center, ElementOf(partner), combo.Radius * 0.55f, big: false);
            }

            yield return WaitGameSeconds(Mathf.Max(0f, DuoStrikeRules.StrikeDurationSeconds - DuoStrikeRules.ImpactDelaySeconds - (look.SecondRing ? 0.15f : 0f)));
            _meter.EndExecution();
        }

        /// <summary>Game-time wait that holds while gameplay is paused (modals, pause menu).</summary>
        private static IEnumerator WaitGameSeconds(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                if (!IsGameplayFrozen)
                {
                    remaining -= Time.deltaTime;
                }

                yield return null;
            }
        }

        private void ApplyImpact(DuoComboSpec combo, Vector3 center, DuoStrikePresentation look)
        {
            if (_curve == null)
            {
                _curve = ProgressionCurve.LoadDefault();
            }

            if (_balance == null)
            {
                _balance = PlayerCombatBalance.LoadDefault();
            }
            float levelMultiplier = _curve != null && _stats != null ? _curve.GetDamageMultiplier(_stats.Level) : 1f;
            float damage = DuoStrikeRules.ResolveDamage(combo, levelMultiplier, _heroBuffs, _balance, null, out bool isCritical);
            StatModifierDefinition[] debuffs = BuildEffects(combo.EnemyDebuffs, combo, isDebuff: true);
            int channel = DamageChannel.For(combo.SourceKey);

            // Copy first: a kill disables the enemy, which edits EnemyController's enabled set.
            var targets = new List<EnemyController>(EnemyController.EnabledEnemies);
            int hits = 0;
            _applyingStrike = true;
            try
            {
                foreach (EnemyController enemy in targets)
                {
                    if (enemy == null || enemy.Health == null || enemy.Health.IsDead || FlatDistance(enemy.transform.position, center) > combo.Radius)
                    {
                        continue;
                    }

                    hits++;
                    ApplyToEnemy(enemy, combo, center, damage, isCritical, debuffs, channel);
                }
            }
            finally
            {
                _applyingStrike = false;
            }

            if (_stats != null)
            {
                float healing = DuoStrikeRules.HealingFor(combo, _stats.MaxHealth);
                if (healing > 0f)
                {
                    _stats.Heal(healing);
                }
            }

            if (_heroBuffs != null && combo.HeroBuffs.Count > 0)
            {
                _heroBuffs.ApplyEffects(BuildEffects(combo.HeroBuffs, combo, isDebuff: false), combo.SourceKey, _player);
            }

            SkillVfxManager.PlayAreaBurst(center, combo.Element, combo.Radius, big: combo.IsSiblingBond);
            if (look.ShakeMagnitude > 0f)
            {
                EventBus.Publish(new CameraShakeRequestedEvent(look.ShakeDuration, look.ShakeMagnitude));
            }

            if (look.HitStopDuration > 0f && hits > 0)
            {
                EventBus.Publish(new HitStopRequestedEvent(look.HitStopDuration, 0.15f));
            }

            EventBus.Publish(new DuoStrikeImpactEvent(combo, center, hits, damage));
        }

        private void ApplyToEnemy(EnemyController enemy, DuoComboSpec combo, Vector3 center, float damage, bool isCritical,
            StatModifierDefinition[] debuffs, int channel)
        {
            Health health = enemy.Health;
            if (damage > 0f)
            {
                OutgoingDamage.DeliverDamage(health, damage, combo.Element, _player, isCritical, channel);
            }

            if (!health.IsAlive)
            {
                return;
            }

            BuffController buffs = enemy.GetComponent<BuffController>();
            if (buffs != null && debuffs.Length > 0)
            {
                buffs.ApplyEffects(debuffs, combo.SourceKey, _player);
            }

            // ApplyStun scales by the target's IStunDurationModifier: bosses take StunRules' 50%.
            if (buffs != null && combo.StunSeconds > 0f)
            {
                buffs.ApplyStun(combo.StunSeconds);
            }

            float force = DuoStrikeRules.KnockbackFor(combo.KnockbackForce, enemy.IsBoss);
            if (force > 0f)
            {
                Vector3 direction = enemy.transform.position - center;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.01f)
                {
                    direction = enemy.transform.position - _player.transform.position;
                    direction.y = 0f;
                }

                KnockbackReceiver receiver = enemy.GetComponent<KnockbackReceiver>();
                if (receiver == null)
                {
                    receiver = enemy.gameObject.AddComponent<KnockbackReceiver>();
                }

                receiver.ApplyKnockback(direction, force);
            }
        }

        private static StatModifierDefinition[] BuildEffects(IReadOnlyList<DuoEffectSpec> specs, DuoComboSpec combo, bool isDebuff)
        {
            var result = new StatModifierDefinition[specs.Count];
            for (int i = 0; i < specs.Count; i++)
            {
                result[i] = specs[i].Build(combo.Id, isDebuff);
            }

            return result;
        }

        private EnemyController FindFocus(Vector3 heroPosition)
        {
            EnemyController best = null;
            float bestDistance = DuoStrikeRules.TargetSearchRange;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                float distance = FlatDistance(enemy.transform.position, heroPosition);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }

        private float HeroHeight()
        {
            if (_player == null)
            {
                return 1.5f;
            }

            float height = 0f;
            foreach (Renderer renderer in _player.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled && (renderer is SkinnedMeshRenderer || renderer is MeshRenderer))
                {
                    height = Mathf.Max(height, renderer.bounds.max.y - _player.transform.position.y);
                }
            }

            return height > 0.5f ? height : 1.5f;
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        /// <summary>Each cousin's signature element (the sibling combos' second ring is in the partner's colour).</summary>
        public static ElementType ElementOf(CharacterId id)
        {
            switch (id)
            {
                case CharacterId.Maithe: return ElementType.Wind;
                case CharacterId.Maria: return ElementType.Holy;
                case CharacterId.Joaquim: return ElementType.Earth;
                case CharacterId.Icaro: return ElementType.Lightning;
                default: return ElementType.Dark;
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor/test hook: binds a hero without the Player tag lookup.</summary>
        public void BindPlayerForTests(GameObject player)
        {
            _player = player;
            _stats = player != null ? player.GetComponent<PlayerStats>() : null;
            _heroBuffs = player != null ? player.GetComponent<BuffController>() : null;
        }

        /// <summary>Editor/test hook: runs the impact of <paramref name="combo"/> at <paramref name="center"/> right away.</summary>
        public void ApplyImpactForTests(DuoComboSpec combo, Vector3 center)
        {
            ApplyImpact(combo, center, DuoStrikePresentation.Resolve(combo.IsSiblingBond, true, true));
        }

        /// <summary>Editor/test hook: feeds one damage event through the charge logic.</summary>
        public void OnDamageAppliedForTests(DamageAppliedEvent evt) => OnDamageApplied(evt);
#endif
    }
}
