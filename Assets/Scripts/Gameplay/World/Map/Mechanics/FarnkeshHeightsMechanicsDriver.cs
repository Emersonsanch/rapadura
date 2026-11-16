using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of two mechanics of the high Picos de Farnkesh (2026-11-16): the headwind of the Garganta dos Ays (57,
    /// <see cref="GorgeHeadwindRules"/>: a short-lived MoveSpeed effect while the hero walks into the wind, none behind a pedra-abrigo, each stone
    /// a grey pillar) and the Huma's shadow in the fight with the Rei-Vendaval (58, <see cref="HumaShadowRules"/>: a golden ring on the ground
    /// that heals and strengthens whoever steps in). Added to the overworld root, idle unless the layout holds the Farnkesh region; never takes the
    /// controls and nothing is saved (the gusts of map 55 stay on <see cref="FarnkeshMechanicsDriver"/>).
    /// </summary>
    public sealed class FarnkeshHeightsMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string WindSource = "farnkesh_gorge_headwind";
        private const string HumaSource = "farnkesh_huma_shadow";
        private const float FeedbackHeight = 2.4f;
        private const float EffectSeconds = 1.5f;
        private const float EffectRefresh = 1f;
        private const float HeadwindHintCooldown = 10f;
        private const float StoneViewRange = 150f;
        private const float StoneHeight = 3.2f;
        private const int RingPoints = 33;

        private static readonly Color StoneColor = new Color(0.62f, 0.6f, 0.58f, 1f);
        private static readonly Color HumaColor = new Color(1f, 0.82f, 0.3f, 1f);

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerStats _stats;
        private CharacterController _character;
        private BuffController _buffs;
        private Material _material;
        private LineRenderer[] _stones;
        private LineRenderer _ring;

        private bool _inGorge;
        private float _windApplied = 1f;
        private float _nextWindRefresh;
        private float _nextHeadwindHint;
        private int _shelterSaid = -1;

        private float _fightStart = -1f;
        private int _ringPass = -1;
        private Vector2 _ringCenter;
        private bool _ringTaken;
        private bool _ringSpoken;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.FarnkeshAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            // The hub keeps the driver on over the whole region (like the silence's driver): Update idles outside the gorge and the nest.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.FarnkeshBounds, _origin) };
        }

        private void OnDisable()
        {
            LeaveGorge();
            EndFight();
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
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            Vector3 position = _player.position;
            Vector2 world = new Vector2(position.x, position.z);
            Vector2 local = world - _origin;
            TickGorge(local, world);
            TickHuma(local);
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
                _stats = go.GetComponent<PlayerStats>();
                _character = go.GetComponent<CharacterController>();
                _buffs = go.GetComponent<BuffController>();
            }

            return _stats != null;
        }

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static CharacterId? Hero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        // ------------------------------------------------------------------
        // 57 · the headwind of the gorge
        // ------------------------------------------------------------------

        private void TickGorge(Vector2 local, Vector2 world)
        {
            ShowStones(local);
            bool inside = OverworldLayouts.GustAreas[0].Contains(local) && OverworldRules.SettlementAt(_layout, world) == null && _stats.IsAlive;
            if (!inside)
            {
                LeaveGorge();
                return;
            }

            if (!_inGorge)
            {
                _inGorge = true;
                Say(FarnkeshHeightsTexts.GorgeEnter);
                SayLine(FarnkeshHeightsTexts.GorgeLineFor(Hero()));
            }

            int shelter = GorgeHeadwindRules.ShelterAt(OverworldLayouts.GorgeShelterStones, local);
            if (shelter >= 0 && shelter != _shelterSaid)
            {
                Say(FarnkeshHeightsTexts.Sheltered);
            }

            _shelterSaid = shelter;
            Vector3 v = _character != null && _character.enabled ? _character.velocity : Vector3.zero;
            Vector2 wind = GorgeHeadwindRules.Wind(OverworldLayouts.GorgeWindAxis);
            float factor = GorgeHeadwindRules.SpeedFactor(new Vector2(v.x, v.z), wind, shelter >= 0, IsNight());
            if (factor < 1f && Time.unscaledTime >= _nextHeadwindHint)
            {
                _nextHeadwindHint = Time.unscaledTime + HeadwindHintCooldown;
                Say(FarnkeshHeightsTexts.Headwind);
            }

            SetWind(factor);
        }

        private void SetWind(float factor)
        {
            if (_buffs == null)
            {
                return;
            }

            if (factor >= 0.999f)
            {
                if (_windApplied != 1f)
                {
                    _buffs.RemoveEffectsBySource(WindSource);
                    _windApplied = 1f;
                }

                return;
            }

            if (Mathf.Abs(factor - _windApplied) < 0.001f && Time.time < _nextWindRefresh)
            {
                return;
            }

            _windApplied = factor;
            _nextWindRefresh = Time.time + EffectRefresh;
            _buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Vento contra",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = factor - 1f,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            }, WindSource);
        }

        private void LeaveGorge()
        {
            _inGorge = false;
            _shelterSaid = -1;
            if (_windApplied != 1f && _buffs != null)
            {
                _buffs.RemoveEffectsBySource(WindSource);
            }

            _windApplied = 1f;
        }

        private void ShowStones(Vector2 local)
        {
            Vector2[] stones = OverworldLayouts.GorgeShelterStones;
            if (_stones == null)
            {
                _stones = new LineRenderer[stones.Length];
                for (int i = 0; i < stones.Length; i++)
                {
                    Vector2 w = _origin + stones[i];
                    Vector3 foot = new Vector3(w.x, _ground.GroundAt(_layout, w), w.y);
                    LineRenderer stone = NewLine("Fk_ShelterStone", 1.4f, 2);
                    stone.SetPosition(0, foot);
                    stone.SetPosition(1, foot + Vector3.up * StoneHeight);
                    stone.startColor = stone.endColor = StoneColor;
                    _stones[i] = stone;
                }
            }

            for (int i = 0; i < stones.Length; i++)
            {
                _stones[i].enabled = (stones[i] - local).sqrMagnitude < StoneViewRange * StoneViewRange;
            }
        }

        // ------------------------------------------------------------------
        // 58 · the Huma's shadow
        // ------------------------------------------------------------------

        private void TickHuma(Vector2 local)
        {
            bool near = (local - OverworldLayouts.ReiVendavalArena).sqrMagnitude < HumaShadowRules.FightRange * HumaShadowRules.FightRange;
            EnemyController king = near && _stats.IsAlive ? FindWindKing() : null;
            if (king == null)
            {
                EndFight();
                return;
            }

            if (_fightStart < 0f)
            {
                _fightStart = Time.time;
                Say(FarnkeshHeightsTexts.HumaArrives);
                SayLine(FarnkeshHeightsTexts.HumaLineFor(Hero()));
            }

            float t = Time.time - _fightStart;
            int pass = HumaShadowRules.PassIndex(t);
            bool marked = HumaShadowRules.Marked(t);
            Vector2 world = local + _origin;
            if (marked && pass != _ringPass)
            {
                _ringPass = pass;
                _ringCenter = HumaShadowRules.RingCenter(world, pass);
                _ringTaken = false;
                _ringSpoken = false;
                Say(FarnkeshHeightsTexts.HumaShadow);
            }

            if (marked && !_ringTaken && HumaShadowRules.Inside(_ringCenter, world))
            {
                _ringTaken = true;
                Bless();
            }

            if (!marked && _ringPass >= 0 && !_ringTaken && !_ringSpoken)
            {
                _ringSpoken = true;
                Say(FarnkeshHeightsTexts.HumaMissed);
            }

            ShowRing(marked && !_ringTaken);
        }

        private void Bless()
        {
            float heal = HumaShadowRules.HealAmount(_stats.CurrentHealth, _stats.MaxHealth);
            if (heal > 0f)
            {
                _stats.Heal(heal);
            }

            _buffs?.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Sombra da Huma",
                affectedStat = StatType.AttackDamage,
                application = ModifierApplication.PercentAdditive,
                value = HumaShadowRules.DamageBonus,
                durationSeconds = HumaShadowRules.BuffSeconds,
                isDebuff = false,
            }, HumaSource);
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(FarnkeshHeightsTexts.Format(localization, FarnkeshHeightsTexts.HumaBlessed, Mathf.RoundToInt(heal)));
        }

        private static EnemyController FindWindKing()
        {
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && enemy.Definition != null && enemy.Definition.name == RegionBossRules.WindKingId && enemy.Health != null && enemy.Health.IsAlive)
                {
                    return enemy;
                }
            }

            return null;
        }

        private void EndFight()
        {
            _fightStart = -1f;
            _ringPass = -1;
            ShowRing(false);
        }

        private void ShowRing(bool show)
        {
            if (!show)
            {
                if (_ring != null)
                {
                    _ring.enabled = false;
                }

                return;
            }

            _ring ??= NewLine("Fk_HumaShadow", 0.25f, RingPoints);
            float y = _player.position.y + 0.15f;
            for (int i = 0; i < RingPoints; i++)
            {
                float a = Mathf.PI * 2f * i / (RingPoints - 1);
                _ring.SetPosition(i, new Vector3(_ringCenter.x + Mathf.Cos(a) * HumaShadowRules.RingRadius, y, _ringCenter.y + Mathf.Sin(a) * HumaShadowRules.RingRadius));
            }

            _ring.startColor = _ring.endColor = HumaColor;
            _ring.enabled = true;
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private LineRenderer NewLine(string objectName, float width, int count)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_FarnkeshHeightsLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = count;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(FarnkeshHeightsTexts.Get(localization, key));
        }

        private void SayLine(string key)
        {
            if (key == null || _player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), FarnkeshHeightsTexts.Get(localization, key));
        }

        private void SayText(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
