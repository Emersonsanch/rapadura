using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Margens da Mistura mechanics (the rules are <see cref="MisturaTimeBubbleRules"/>, <see cref="MisturaTarRules"/> and
    /// <see cref="MisturaShoulderRules"/>): added to the overworld root, idle unless the layout holds the region and the hero stands in it.
    /// Speed changes are short-lived effects on the BuffController, refreshed while they apply, so they end by themselves if this driver stops;
    /// the shoulder pushes through the CharacterController, so walls still hold. Nothing here takes the hero's controls away.
    /// </summary>
    public sealed class MisturaMechanicsDriver : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const string BubbleSource = "mistura_time_bubble";
        private const string TarSource = "mistura_tar";
        private const string BurnChannelKey = "mistura_tar_burn";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 6f;
        private const float EffectSeconds = 1.2f;
        private const float EffectRefresh = 0.5f;
        private const float BeastPassSeconds = 0.5f;
        private const float BeastRange = 70f;
        private const float RingRange = 60f;
        private const float StoneRingRange = 30f;
        private const float FuseWarnExtra = 6f;
        private const int RingPoints = 40;
        private const int FlamePoints = 28;
        private const int StreakCount = 6;
        private const int BubbleKeys = 0;
        private const int TarKeys = 1000;
        private const int StoneKeys = 2000;

        private static readonly Color SlowColor = new Color(0.45f, 0.6f, 1f, 1f);
        private static readonly Color FastColor = new Color(1f, 0.8f, 0.3f, 1f);
        private static readonly Color StickyColor = new Color(0.55f, 0.4f, 0.2f, 1f);
        private static readonly Color FuseColor = new Color(1f, 0.3f, 0.15f, 1f);
        private static readonly Color FlameColor = new Color(1f, 0.55f, 0.15f, 1f);
        private static readonly Color ScorchedColor = new Color(0.35f, 0.32f, 0.3f, 1f);
        private static readonly Color StoneColor = new Color(0.6f, 0.65f, 0.6f, 0.6f);
        private static readonly Color StoneWarnColor = new Color(0.55f, 1f, 0.6f, 1f);

        /// <summary>One source of speed change on the hero: what it is worth now and when to refresh it.</summary>
        private sealed class HeroEffect
        {
            public readonly string Source;
            public float Factor = 1f;
            public float NextRefresh;
            public StatModifierDefinition Definition;

            public HeroEffect(string source) => Source = source;
        }

        private readonly Dictionary<int, LineRenderer> _rings = new Dictionary<int, LineRenderer>();
        private readonly Dictionary<int, LineRenderer> _flames = new Dictionary<int, LineRenderer>();
        private readonly Dictionary<int, Light> _lights = new Dictionary<int, Light>();
        private readonly LineRenderer[] _streaks = new LineRenderer[StreakCount];
        private readonly HeroEffect _bubbleEffect = new HeroEffect(BubbleSource);
        private readonly HeroEffect _tarEffect = new HeroEffect(TarSource);
        private readonly StatModifierDefinition _beastSlow = NewEffect(MisturaTimeBubbleRules.SlowFactor);
        private readonly StatModifierDefinition _beastFast = NewEffect(MisturaTimeBubbleRules.FastFactor);

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private int _burnChannel;
        private BuffController _buffs;
        private Material _material;

        private MisturaTarState[] _tar;
        private bool[] _fuseWarned;
        private bool[] _burnWarned;
        private bool _inRegion;
        private int _hintBubble = -1;
        private int _hintPatch = -1;
        private float _nextPatchHint;
        private float _nextBurnTick;
        private float _nextBeastPass;
        private int _warnedBreath = -1;
        private int _tiltedBreath = -1;
        private int _heldBreath = -1;

        private void OnEnable() => EventBus.Subscribe<SkillCastEvent>(OnSkillCast);

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            ClearHeroEffects();
        }

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.MisturaAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _tar = new MisturaTarState[OverworldLayouts.TarPatches.Length];
            _fuseWarned = new bool[_tar.Length];
            _burnWarned = new bool[_tar.Length];
            ResetTar();
            _burnChannel = DamageChannel.ForDamageOverTime(BurnChannelKey, StatType.MoveSpeed);
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
            if (_tar == null || !FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.MisturaBounds.Contains(local))
            {
                LeaveRegion();
                return;
            }

            _inRegion = true;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;
            TickBubbles(local, night);
            TickTar(local);
            TickBeasts(local, night);
            TickShoulder(local, night);
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
                _buffs = go.GetComponent<BuffController>();
            }

            return _controller != null && _character != null;
        }

        private void LeaveRegion()
        {
            if (!_inRegion)
            {
                return;
            }

            _inRegion = false;
            ClearHeroEffects();
            ResetTar();
            SetStreaks(false, Vector2.zero, false);
            foreach (LineRenderer ring in _rings.Values)
            {
                ring.enabled = false;
            }

            foreach (LineRenderer flame in _flames.Values)
            {
                flame.enabled = false;
            }

            foreach (Light light in _lights.Values)
            {
                light.enabled = false;
            }
        }

        private void ResetTar()
        {
            for (int i = 0; i < _tar.Length; i++)
            {
                _tar[i] = MisturaTarRules.Resting;
                _fuseWarned[i] = false;
                _burnWarned[i] = false;
            }
        }

        // ------------------------------------------------------------------
        // Speed effects
        // ------------------------------------------------------------------

        private static StatModifierDefinition NewEffect(float factor) => new StatModifierDefinition
        {
            displayName = "Mistura",
            affectedStat = StatType.MoveSpeed,
            application = ModifierApplication.PercentAdditive,
            value = MisturaTimeBubbleRules.MoveBonus(factor),
            durationSeconds = EffectSeconds,
            isDebuff = factor < 1f,
        };

        private void SetHeroEffect(HeroEffect effect, float factor)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!Mathf.Approximately(effect.Factor, factor))
            {
                _buffs.RemoveEffectsBySource(effect.Source);
                effect.Factor = factor;
                effect.NextRefresh = 0f;
                effect.Definition = Mathf.Approximately(factor, 1f) ? null : NewEffect(factor);
            }

            if (effect.Definition != null && Time.time >= effect.NextRefresh)
            {
                effect.NextRefresh = Time.time + EffectRefresh;
                _buffs.ApplyEffect(effect.Definition, effect.Source);
            }
        }

        private void ClearHeroEffects()
        {
            SetHeroEffect(_bubbleEffect, 1f);
            SetHeroEffect(_tarEffect, 1f);
        }

        // ------------------------------------------------------------------
        // Time bubbles
        // ------------------------------------------------------------------

        private void TickBubbles(Vector2 local, bool night)
        {
            MisturaTimeBubble[] bubbles = OverworldLayouts.TimeBubbles;
            int here = MisturaTimeBubbleRules.IndexAt(bubbles, local, night);
            SetHeroEffect(_bubbleEffect, here >= 0 ? bubbles[here].Factor : 1f);
            if (here >= 0 && here != _hintBubble)
            {
                Say(bubbles[here].Factor < 1f ? MisturaMechanicsTexts.BubbleSlow : MisturaMechanicsTexts.BubbleFast);
            }

            _hintBubble = here;
            for (int i = 0; i < bubbles.Length; i++)
            {
                bool show = !night && NearRim(local, bubbles[i].Center, bubbles[i].Radius, RingRange);
                ShowRing(BubbleKeys + i, bubbles[i].Center, bubbles[i].Radius, bubbles[i].Factor < 1f ? SlowColor : FastColor, show);
            }
        }

        private static bool NearRim(Vector2 local, Vector2 center, float radius, float range) => Vector2.Distance(local, center) - radius <= range;

        // ------------------------------------------------------------------
        // The pitch
        // ------------------------------------------------------------------

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (_tar == null || !_inRegion || evt.Skill == null || evt.Skill.Element != ElementType.Fire || _player == null || evt.Caster == null
                || (evt.Caster != _player.gameObject && !evt.Caster.transform.IsChildOf(_player)))
            {
                return;
            }

            Vector2 target = new Vector2(evt.TargetPoint.x, evt.TargetPoint.z) - _origin;
            Vector2 caster = new Vector2(_player.position.x, _player.position.z) - _origin;
            MisturaTarPatch[] patches = OverworldLayouts.TarPatches;
            bool lit = false;
            for (int i = 0; i < patches.Length; i++)
            {
                bool reached = MisturaTarRules.Reaches(patches[i], target, evt.Skill.AreaRadius) || MisturaTarRules.Reaches(patches[i], caster, 0f);
                if (reached && _tar[i].Stage == MisturaTarStage.Sticky)
                {
                    _tar[i] = MisturaTarRules.Ignite(_tar[i]);
                    _fuseWarned[i] = false;
                    _burnWarned[i] = false;
                    lit = true;
                }
            }

            if (lit)
            {
                Say(MisturaMechanicsTexts.TarLit);
            }
        }

        private void TickTar(Vector2 local)
        {
            MisturaTarPatch[] patches = OverworldLayouts.TarPatches;
            bool burnNow = Time.time >= _nextBurnTick;
            if (burnNow)
            {
                _nextBurnTick = Time.time + MisturaTarRules.BurnTickSeconds;
            }

            bool burned = false;
            int warnBurn = -1;
            for (int i = 0; i < patches.Length; i++)
            {
                _tar[i] = MisturaTarRules.Advance(_tar[i], Time.deltaTime);
                MisturaTarStage stage = _tar[i].Stage;
                if (stage == MisturaTarStage.Fuse && !_fuseWarned[i] && Vector2.Distance(patches[i].Center, local) <= patches[i].Radius + MisturaTarRules.BurnMargin + FuseWarnExtra)
                {
                    _fuseWarned[i] = true;
                    Say(MisturaMechanicsTexts.TarFuse);
                }

                if (stage == MisturaTarStage.Burning && MisturaTarRules.InBurnReach(patches[i], local))
                {
                    burned = true;
                    if (!_burnWarned[i])
                    {
                        _burnWarned[i] = true;
                        warnBurn = i;
                    }
                }

                ShowTar(i, patches[i], stage, local);
            }

            if (burned && burnNow)
            {
                BurnHero();
            }

            if (warnBurn >= 0)
            {
                Say(MisturaMechanicsTexts.TarBurns);
            }

            if (burnNow)
            {
                BurnBeasts(local, patches);
            }

            int here = MisturaTarRules.PatchAt(patches, local);
            MisturaTarStage hereStage = here >= 0 ? _tar[here].Stage : MisturaTarStage.Scorched;
            SetHeroEffect(_tarEffect, here >= 0 ? MisturaTarRules.SpeedFactor(hereStage) : 1f);
            if (here >= 0 && hereStage == MisturaTarStage.Sticky && (here != _hintPatch || Time.unscaledTime >= _nextPatchHint))
            {
                _nextPatchHint = Time.unscaledTime + HintCooldown;
                Say(MisturaMechanicsTexts.TarSticky);
            }

            _hintPatch = here;
        }

        private void BurnHero()
        {
            if (_stats == null || !_stats.IsAlive)
            {
                return;
            }

            float bite = MisturaTarRules.HeroBurn(_stats.CurrentHealth, _stats.MaxHealth);
            if (bite > 0f)
            {
                _stats.ApplyHazardDamage(bite);
            }
        }

        private void BurnBeasts(Vector2 local, MisturaTarPatch[] patches)
        {
            bool any = false;
            for (int i = 0; i < patches.Length && !any; i++)
            {
                any = _tar[i].Stage == MisturaTarStage.Burning;
            }

            if (!any)
            {
                return;
            }

            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (Vector2.Distance(at, local) > BeastRange)
                {
                    continue;
                }

                for (int i = 0; i < patches.Length; i++)
                {
                    if (_tar[i].Stage == MisturaTarStage.Burning && MisturaTarRules.InBurnReach(patches[i], at))
                    {
                        enemy.Health.ApplyChanneledDamage(MisturaTarRules.BeastBurn(enemy.Health.MaxHealth), ElementType.Fire, _player.gameObject, false, _burnChannel);
                        break;
                    }
                }
            }
        }

        private void ShowTar(int index, MisturaTarPatch patch, MisturaTarStage stage, Vector2 local)
        {
            bool near = NearRim(local, patch.Center, patch.Radius, RingRange);
            Color color = stage == MisturaTarStage.Sticky ? StickyColor : stage == MisturaTarStage.Fuse ? FuseColor : stage == MisturaTarStage.Burning ? FlameColor : ScorchedColor;
            ShowRing(TarKeys + index, patch.Center, patch.Radius, color, near);
            ShowFlames(index, patch, near && stage == MisturaTarStage.Burning);
        }

        private void ShowFlames(int index, MisturaTarPatch patch, bool show)
        {
            _flames.TryGetValue(index, out LineRenderer flame);
            _lights.TryGetValue(index, out Light light);
            if (!show)
            {
                if (flame != null)
                {
                    flame.enabled = false;
                }

                if (light != null)
                {
                    light.enabled = false;
                }

                return;
            }

            if (flame == null)
            {
                flame = NewLine("Mx_TarFlame", 0.45f);
                flame.loop = true;
                flame.positionCount = FlamePoints;
                flame.startColor = flame.endColor = FlameColor;
                _flames[index] = flame;
            }

            if (light == null)
            {
                var go = new GameObject("Mx_TarLight");
                go.transform.SetParent(transform, false);
                light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = FlameColor;
                light.range = patch.Radius * 2.5f;
                light.shadows = LightShadows.None;
                Vector2 world = _origin + patch.Center;
                go.transform.position = new Vector3(world.x, Ground(world) + 2.5f, world.y);
                _lights[index] = light;
            }

            flame.enabled = true;
            light.enabled = true;
            light.intensity = 3f + Mathf.PerlinNoise(Time.time * 6f, index) * 2f;
            for (int i = 0; i < FlamePoints; i++)
            {
                float a = i * Mathf.PI * 2f / FlamePoints;
                Vector2 world = _origin + patch.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * patch.Radius;
                float lick = Mathf.PerlinNoise(i * 0.7f + Time.time * 5f, index + 0.5f) * 1.8f;
                flame.SetPosition(i, new Vector3(world.x, Ground(world) + 0.3f + lick, world.y));
            }
        }

        // ------------------------------------------------------------------
        // Beasts in the bubbles
        // ------------------------------------------------------------------

        private void TickBeasts(Vector2 local, bool night)
        {
            if (night || Time.time < _nextBeastPass)
            {
                return;
            }

            _nextBeastPass = Time.time + BeastPassSeconds;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Buffs == null)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if (Vector2.Distance(at, local) > BeastRange)
                {
                    continue;
                }

                float factor = MisturaTimeBubbleRules.FactorAt(OverworldLayouts.TimeBubbles, at, false);
                if (!Mathf.Approximately(factor, 1f))
                {
                    enemy.Buffs.ApplyEffect(factor < 1f ? _beastSlow : _beastFast, BubbleSource);
                }
            }
        }

        // ------------------------------------------------------------------
        // The breathing shoulder
        // ------------------------------------------------------------------

        private void TickShoulder(Vector2 local, bool night)
        {
            Vector2[] stones = OverworldLayouts.BreathStones;
            bool onShoulder = OverworldLayouts.ShoulderArea.Contains(local) && OverworldRules.SettlementAt(_layout, _origin + local) == null;
            MisturaBreathPhase phase = onShoulder ? MisturaShoulderRules.PhaseAt(Time.time, night) : MisturaBreathPhase.Still;
            bool alert = phase != MisturaBreathPhase.Still;
            for (int i = 0; i < stones.Length; i++)
            {
                bool show = onShoulder && Vector2.Distance(local, stones[i]) <= StoneRingRange;
                ShowRing(StoneKeys + i, stones[i], MisturaShoulderRules.StoneReach, alert ? StoneWarnColor : StoneColor, show);
            }

            if (!alert)
            {
                SetStreaks(false, Vector2.zero, false);
                return;
            }

            int breath = MisturaShoulderRules.BreathIndex(Time.time);
            Vector2 direction = MisturaShoulderRules.Direction(breath);
            bool tilting = phase == MisturaBreathPhase.Tilting;
            SetStreaks(true, direction, tilting);
            if (!tilting)
            {
                if (_warnedBreath != breath)
                {
                    _warnedBreath = breath;
                    Say(MisturaMechanicsTexts.BreathWarning);
                }

                return;
            }

            bool sheltered = MisturaShoulderRules.Sheltered(stones, local);
            Vector2 slide = MisturaShoulderRules.SlideVelocity(phase, direction, _controller.HasMoveIntent, sheltered);
            if (sheltered && _heldBreath != breath)
            {
                _heldBreath = breath;
                Say(MisturaMechanicsTexts.BreathHeld);
            }

            if (slide == Vector2.zero || !_character.enabled)
            {
                return;
            }

            GroundedPush.Shove(_character, slide * Time.deltaTime);
            if (_tiltedBreath != breath)
            {
                _tiltedBreath = breath;
                Say(MisturaMechanicsTexts.BreathTilts);
            }
        }

        private void SetStreaks(bool show, Vector2 direction, bool sliding)
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

                if (streak == null)
                {
                    streak = _streaks[i] = NewLine("Mx_BreathStreak", 0.08f);
                    streak.startColor = streak.endColor = new Color(0.8f, 0.78f, 0.7f, 0.6f);
                }

                streak.enabled = true;
                float sweep = Mathf.Repeat(Time.time * (sliding ? 0.9f : 0.3f) + i * 0.173f, 1f);
                var dir = new Vector3(direction.x, 0f, direction.y);
                Vector3 side = Vector3.Cross(Vector3.up, dir);
                Vector3 a = _player.position + side * ((i - (StreakCount - 1) * 0.5f) * 1.6f) + dir * Mathf.Lerp(-6f, 6f, sweep) + Vector3.up * 0.15f;
                streak.SetPosition(0, a);
                streak.SetPosition(1, a + dir * 1.8f);
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private float Ground(Vector2 world) => Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world));

        private void ShowRing(int key, Vector2 center, float radius, Color color, bool show)
        {
            _rings.TryGetValue(key, out LineRenderer ring);
            if (!show)
            {
                if (ring != null)
                {
                    ring.enabled = false;
                }

                return;
            }

            if (ring == null)
            {
                ring = NewLine("Mx_Ring", 0.25f);
                ring.loop = true;
                ring.positionCount = RingPoints;
                for (int i = 0; i < RingPoints; i++)
                {
                    float a = i * Mathf.PI * 2f / RingPoints;
                    Vector2 world = _origin + center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    ring.SetPosition(i, new Vector3(world.x, Ground(world) + 0.2f, world.y));
                }

                _rings[key] = ring;
            }

            ring.startColor = ring.endColor = color;
            ring.enabled = true;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_MisturaLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, MisturaMechanicsTexts.Get(localization, key));
        }
    }
}
