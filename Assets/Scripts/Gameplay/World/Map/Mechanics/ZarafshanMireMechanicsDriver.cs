using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the mud of the Brejo do Lodo Escuro (03, <see cref="ZarafshanMireRules"/>) and the webs of the Bosque das Amoreiras (04,
    /// <see cref="ZarafshanWebRules"/>). Added to the overworld root, idle unless the layout holds the Zarafshan region; it works only inside the
    /// two maps' bounds. The mud is a short-lived slow on the BuffController, refreshed while it applies (so it ends by itself if this driver
    /// stops); Joaquim's skills open clean pools (rings of Living Water). A whole web holds the hero with a 1.5 s stun and tears; a hero skill that
    /// reaches a web cuts it. Never takes the controls and deals no damage; the moment each web broke stays in the save (time marks).
    /// </summary>
    public sealed class ZarafshanMireMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string MudSource = "zarafshan_mud";
        private const float FeedbackHeight = 2.4f;
        private const float MudHintCooldown = 20f;
        private const float WebHintCooldown = 12f;
        private const float WebHintRange = 9f;
        private const float WebViewRange = 70f;
        private const float EffectSeconds = 1.2f;
        private const float EffectRefresh = 0.5f;
        private const float WebLow = 0.5f;
        private const float WebHigh = 2.1f;
        private const int RingPoints = 32;

        private static readonly Color WebColor = new Color(0.96f, 0.93f, 0.88f, 0.95f);
        private static readonly Color PoolColor = new Color(0.31f, 0.765f, 0.69f, 1f);

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly List<ZarafshanCleanPool> _pools = new List<ZarafshanCleanPool>();
        private readonly List<MapPath> _walkways = new List<MapPath>();
        private readonly List<Vector2> _bridges = new List<Vector2>();
        private readonly List<LineRenderer> _poolRings = new List<LineRenderer>();

        private OverworldLayout _layout;
        private Vector2 _origin;
        private Rect _mire;
        private Rect _grove;
        private Transform _player;
        private PlayerStats _stats;
        private BuffController _buffs;
        private Material _material;
        private LineRenderer[] _webLines;
        private long[] _broken;
        private long[] _brokenLocal;

        private StatModifierDefinition _mudEffect;
        private bool _mudOn;
        private float _nextMudRefresh;
        private float _nextMudHint;
        private float _nextWebHint;
        private bool _inMire;
        private bool _inGrove;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _mire = OverworldLayouts.BrejoMireBounds;
            _grove = OverworldLayouts.AmoreirasGroveBounds;

            // The hub keeps the driver on over the whole region (like the garden's driver): Update itself idles outside the two maps.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.ZarafshanBounds, _origin) };
            CollectWalkways();
            int webs = OverworldLayouts.AmoreirasWebs.Length;
            _webLines = new LineRenderer[webs];
            _broken = new long[webs];
            _brokenLocal = new long[webs];
            _mudEffect = new StatModifierDefinition
            {
                displayName = "Lama",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = ZarafshanMireRules.MoveBonus,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            };
        }

        /// <summary>The trails and bridges of the stitched world that touch the marsh, moved into the region's coordinates once.</summary>
        private void CollectWalkways()
        {
            MapLayout world = _layout.World;
            if (world == null)
            {
                return;
            }

            Rect near = new Rect(_mire.position - Vector2.one * 10f, _mire.size + Vector2.one * 20f);
            foreach (MapPath path in world.Paths)
            {
                MapPath local = new MapPath(path.A - _origin, path.B - _origin, path.Width);
                Rect box = Rect.MinMaxRect(Mathf.Min(local.A.x, local.B.x), Mathf.Min(local.A.y, local.B.y), Mathf.Max(local.A.x, local.B.x), Mathf.Max(local.A.y, local.B.y));
                if (near.Overlaps(box))
                {
                    _walkways.Add(local);
                }
            }

            foreach (MapRiver river in world.Rivers)
            {
                foreach (MapCrossing crossing in river.Crossings)
                {
                    Vector2 at = crossing.At - _origin;
                    if (near.Contains(at))
                    {
                        _bridges.Add(at);
                    }
                }
            }
        }

        private void OnEnable() => EventBus.Subscribe<SkillCastEvent>(HandleSkillCast);

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(HandleSkillCast);
            SetMud(false);
            LeaveMire();
            LeaveGrove();
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

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (_mire.Contains(local))
            {
                _inMire = true;
                TickMud(local);
            }
            else
            {
                LeaveMire();
            }

            if (_grove.Contains(local))
            {
                _inGrove = true;
                TickWebs(local);
            }
            else
            {
                LeaveGrove();
            }
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
                _buffs = go.GetComponent<BuffController>();
            }

            return _stats != null;
        }

        private static CharacterId? Hero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private static long Now() => Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        // ------------------------------------------------------------------
        // 03 · The mud
        // ------------------------------------------------------------------

        private void TickMud(Vector2 local)
        {
            bool safe = OverworldRules.SettlementAt(_layout, _origin + local) != null;
            bool walkway = ZarafshanMireRules.OnWalkway(_walkways, _bridges, local);
            bool cleaned = ZarafshanMireRules.Cleaned(_pools, local, Time.time);
            bool slowed = _stats.IsAlive && ZarafshanMireRules.SpeedFactor(true, walkway, safe, cleaned) < 1f;
            SetMud(slowed);
            if (slowed && Time.unscaledTime >= _nextMudHint)
            {
                _nextMudHint = Time.unscaledTime + MudHintCooldown;
                Say(ZarafshanMireRules.Cleans(Hero()) ? ZarafshanMireTexts.MudJoaquim : ZarafshanMireTexts.MudSlows);
            }

            ShowPools();
        }

        private void SetMud(bool on)
        {
            if (_buffs == null || _mudEffect == null)
            {
                return;
            }

            if (!on)
            {
                if (_mudOn)
                {
                    _buffs.RemoveEffectsBySource(MudSource);
                    _mudOn = false;
                }

                return;
            }

            if (!_mudOn || Time.time >= _nextMudRefresh)
            {
                _mudOn = true;
                _nextMudRefresh = Time.time + EffectRefresh;
                _buffs.ApplyEffect(_mudEffect, MudSource);
            }
        }

        private void LeaveMire()
        {
            if (!_inMire)
            {
                return;
            }

            _inMire = false;
            SetMud(false);
            _pools.Clear();
            foreach (LineRenderer ring in _poolRings)
            {
                if (ring != null)
                {
                    ring.enabled = false;
                }
            }
        }

        private void ShowPools()
        {
            float now = Time.time;
            int shown = 0;
            foreach (ZarafshanCleanPool pool in _pools)
            {
                if (now >= pool.Until)
                {
                    continue;
                }

                if (shown >= _poolRings.Count)
                {
                    _poolRings.Add(NewLine("Zf_MireCleanPool", 0.25f, RingPoints + 1));
                }

                LineRenderer ring = _poolRings[shown++];
                ring.enabled = true;
                float fade = Mathf.Clamp01((pool.Until - now) / 3f);
                ring.startColor = ring.endColor = new Color(PoolColor.r, PoolColor.g, PoolColor.b, 0.35f + 0.6f * fade);
                for (int i = 0; i <= RingPoints; i++)
                {
                    float a = i * Mathf.PI * 2f / RingPoints;
                    Vector2 p = pool.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ZarafshanMireRules.CleanRadius;
                    ring.SetPosition(i, Ground(p) + Vector3.up * 0.15f);
                }
            }

            for (int i = shown; i < _poolRings.Count; i++)
            {
                if (_poolRings[i] != null)
                {
                    _poolRings[i].enabled = false;
                }
            }
        }

        // ------------------------------------------------------------------
        // 04 · The webs
        // ------------------------------------------------------------------

        private void ReadMarks()
        {
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            ZarafshanWeb[] webs = OverworldLayouts.AmoreirasWebs;
            for (int i = 0; i < webs.Length; i++)
            {
                _broken[i] = saved ? progress.GetTimeMark(ZarafshanWebRules.BrokenKey(webs[i].Id)) : _brokenLocal[i];
            }
        }

        private void Break(int web, long now)
        {
            _brokenLocal[web] = now;
            _broken[web] = now;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(ZarafshanWebRules.BrokenKey(OverworldLayouts.AmoreirasWebs[web].Id), now);
            }
        }

        private void TickWebs(Vector2 local)
        {
            long now = Now();
            ReadMarks();
            ShowWebs(local, now);
            if (!_stats.IsAlive)
            {
                return;
            }

            ZarafshanWeb[] webs = OverworldLayouts.AmoreirasWebs;
            int caught = ZarafshanWebRules.CatchAt(webs, _broken, local, now);
            if (caught >= 0)
            {
                Break(caught, now);
                if (_buffs != null)
                {
                    _buffs.ApplyStun(ZarafshanWebRules.HoldSeconds);
                }

                Say(ZarafshanMireTexts.WebCaught);
                return;
            }

            if (Time.unscaledTime < _nextWebHint)
            {
                return;
            }

            for (int i = 0; i < webs.Length; i++)
            {
                if (ZarafshanWebRules.Whole(_broken[i], now) && ZarafshanWebRules.Distance(webs[i], local) <= WebHintRange)
                {
                    _nextWebHint = Time.unscaledTime + WebHintCooldown;
                    Say(ZarafshanMireTexts.WebAhead);
                    return;
                }
            }
        }

        private void LeaveGrove()
        {
            if (!_inGrove)
            {
                return;
            }

            _inGrove = false;
            if (_webLines == null)
            {
                return;
            }

            foreach (LineRenderer line in _webLines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private void ShowWebs(Vector2 local, long now)
        {
            ZarafshanWeb[] webs = OverworldLayouts.AmoreirasWebs;
            for (int i = 0; i < webs.Length; i++)
            {
                bool show = ZarafshanWebRules.Whole(_broken[i], now) && ZarafshanWebRules.Distance(webs[i], local) <= WebViewRange;
                if (!show)
                {
                    if (_webLines[i] != null)
                    {
                        _webLines[i].enabled = false;
                    }

                    continue;
                }

                if (_webLines[i] == null)
                {
                    _webLines[i] = NewLine("Zf_AmoreirasWeb", 0.08f, 6);
                    _webLines[i].startColor = _webLines[i].endColor = WebColor;
                }

                LineRenderer line = _webLines[i];
                line.enabled = true;
                Vector3 a = Ground(webs[i].A);
                Vector3 b = Ground(webs[i].B);

                // A zigzag of silk between the two trunks, low to high and back.
                for (int k = 0; k < 6; k++)
                {
                    float t = k / 5f;
                    Vector3 p = Vector3.Lerp(a, b, t);
                    line.SetPosition(k, p + Vector3.up * ((k & 1) == 0 ? WebLow : WebHigh));
                }
            }
        }

        // ------------------------------------------------------------------
        // Skills: Living Water cleans the mud, any skill cuts a web
        // ------------------------------------------------------------------

        private void HandleSkillCast(SkillCastEvent evt)
        {
            if (_areas == null || _player == null || evt.Skill == null || evt.Caster == null
                || (evt.Caster != _player.gameObject && !evt.Caster.transform.IsChildOf(_player)))
            {
                return;
            }

            Vector2 caster = new Vector2(_player.position.x, _player.position.z) - _origin;
            Vector2 target = new Vector2(evt.TargetPoint.x, evt.TargetPoint.z) - _origin;
            if (_inMire && ZarafshanMireRules.Cleans(Hero()))
            {
                ZarafshanMireRules.AddPool(_pools, caster, Time.time);
                if (_mire.Contains(target) && (target - caster).sqrMagnitude > ZarafshanMireRules.CleanRadius * ZarafshanMireRules.CleanRadius)
                {
                    ZarafshanMireRules.AddPool(_pools, target, Time.time);
                }

                Say(ZarafshanMireTexts.MudCleaned);
            }

            if (!_inGrove)
            {
                return;
            }

            long now = Now();
            ReadMarks();
            ZarafshanWeb[] webs = OverworldLayouts.AmoreirasWebs;
            bool cut = false;
            for (int i = 0; i < webs.Length; i++)
            {
                if (ZarafshanWebRules.Whole(_broken[i], now) && ZarafshanWebRules.Cuts(webs[i], target, evt.Skill.AreaRadius, caster))
                {
                    Break(i, now);
                    cut = true;
                }
            }

            if (cut)
            {
                Say(ZarafshanMireTexts.WebCut);
            }
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
        }

        private LineRenderer NewLine(string objectName, float width, int points)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_ZarafshanMireLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = points;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, ZarafshanMireTexts.Get(localization, key));
        }
    }
}
