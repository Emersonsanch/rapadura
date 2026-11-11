using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Serra de Kaggen winds (the rules are <see cref="KaggenWindRules"/>): added to the overworld root, idle unless
    /// the layout holds the region and the hero stands in it. Gusts go through the CharacterController, so walls hold, and never
    /// touch a hero on a rope bridge. The hero keeps the controls all the time.
    /// </summary>
    public sealed class KaggenMechanicsDriver : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const int StreakCount = 8;
        private const float HintCooldown = 8f;
        private const float FeedbackHeight = 2.4f;

        private static readonly Color StreakColor = new Color(0.92f, 0.95f, 1f, 0.55f);

        private readonly LineRenderer[] _streaks = new LineRenderer[StreakCount];

        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;

        private long _warnedGust = long.MinValue;
        private long _hitGust = long.MinValue;
        private float _nextBridgeHint;
        private bool _onBridgeBefore;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.KaggenAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Update()
        {
            if (!FindPlayer())
            {
                return;
            }

            Vector2 world = new Vector2(_player.position.x, _player.position.z);
            Vector2 local = world - _origin;
            if (!OverworldLayouts.KaggenBounds.Contains(local) || !_stats.IsAlive)
            {
                ShowStreaks(false, Vector2.zero, false);
                return;
            }

            CharacterId? hero = MapRequirementHeroState.Capture(_bag).Hero;
            bool onBridge = KaggenWindRules.OnBridge(local, OverworldLayouts.KaggenBridges);
            TickBridge(onBridge, hero);
            TickWind(local, world, onBridge, hero);
        }

        private bool FindPlayer()
        {
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return false;
                }

                _player = go.transform;
                _controller = go.GetComponent<PlayerController>();
                _character = go.GetComponent<CharacterController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _character != null && _stats != null;
        }

        private void TickBridge(bool onBridge, CharacterId? hero)
        {
            bool running = _controller.Input.IsRunHeld && _controller.HasMoveIntent;
            float extra = KaggenWindRules.ExtraBridgeDrain(onBridge, running, hero, Time.deltaTime);
            if (extra > 0f)
            {
                _stats.DrainStamina(extra);
            }

            if (onBridge && !_onBridgeBefore && Time.unscaledTime >= _nextBridgeHint && !KaggenWindRules.Ignores(hero))
            {
                _nextBridgeHint = Time.unscaledTime + HintCooldown;
                Say(KaggenMechanicsTexts.BridgeHint);
            }

            _onBridgeBefore = onBridge;
        }

        private void TickWind(Vector2 local, Vector2 world, bool onBridge, CharacterId? hero)
        {
            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            KaggenWindPhase phase = KaggenWindRules.PhaseAt(now);
            bool inWind = phase != KaggenWindPhase.Calm && !onBridge && KaggenWindRules.InsideAny(local, OverworldLayouts.KaggenWindAreas)
                && OverworldRules.SettlementAt(_layout, world) == null;
            if (!inWind)
            {
                ShowStreaks(false, Vector2.zero, false);
                return;
            }

            long gust = KaggenWindRules.GustIndex(now);
            Vector2 direction = KaggenWindRules.Direction(gust);
            ShowStreaks(true, direction, phase == KaggenWindPhase.Blowing);
            if (phase == KaggenWindPhase.Warning)
            {
                if (_warnedGust != gust)
                {
                    _warnedGust = gust;
                    Say(KaggenWindRules.Ignores(hero) ? KaggenMechanicsTexts.MaitheCalm : KaggenMechanicsTexts.WindWarning);
                }

                return;
            }

            Vector2 push = KaggenWindRules.PushVelocity(direction, !_controller.HasMoveIntent, hero);
            if (push == Vector2.zero || !_character.enabled)
            {
                return;
            }

            GroundedPush.Shove(_character, push * Time.deltaTime);
            if (_hitGust != gust)
            {
                _hitGust = gust;
                Say(KaggenMechanicsTexts.WindHit);
            }
        }

        private void ShowStreaks(bool show, Vector2 direction, bool blowing)
        {
            for (int i = 0; i < StreakCount; i++)
            {
                LineRenderer streak = _streaks[i];
                if (!show)
                {
                    if (streak != null)
                    {
                        streak.enabled = false;
                    }

                    continue;
                }

                streak = streak != null ? streak : (_streaks[i] = NewLine());
                streak.enabled = true;
                float sweep = Mathf.Repeat(Time.time * (blowing ? 1.6f : 0.5f) + i * 0.137f, 1f);
                var dir = new Vector3(direction.x, 0f, direction.y);
                Vector3 side = Vector3.Cross(Vector3.up, dir);
                float across = (i - (StreakCount - 1) * 0.5f) * (blowing ? 1.1f : 2.2f);
                float height = blowing ? 0.5f + (i % 3) * 0.7f : 8f + (i % 3) * 1.5f;
                Vector3 a = _player.position + side * across + dir * Mathf.Lerp(-7f, 7f, sweep) + Vector3.up * height;
                streak.SetPosition(0, a);
                streak.SetPosition(1, a + dir * (blowing ? 2.6f : 1.4f));
            }
        }

        private LineRenderer NewLine()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_KaggenLine_Runtime" };
            }

            var go = new GameObject("Kg_WindStreak");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.06f;
            line.startColor = line.endColor = StreakColor;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, KaggenMechanicsTexts.Get(localization, key));
        }
    }
}
