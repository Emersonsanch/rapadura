using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Planície de Mingcheng mechanics (the rules are <see cref="MingchengWallRules"/>, <see cref="MingchengBeaconRules"/> and
    /// <see cref="MingchengReflectionRules"/>): added to the overworld root, idle unless the layout holds the region and the hero stands in it.
    /// The wall's deck is a pool of thin colliders laid along the line around the hero, so the walk costs the same however long the wall is; the
    /// towers carry the hero up and down in one press. Speed changes are short-lived effects, refreshed while they apply. The deck's sides are
    /// open and a fall does no harm, so nothing here can hold or kill the hero.
    /// </summary>
    public sealed class MingchengMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string CorridorSource = "mingcheng_corridor";
        private const string TrailSource = "mingcheng_reflection";
        private const string ChestMarkPrefix = "mingcheng_chest_";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 4f;
        private const float EffectSeconds = 1.2f;
        private const float EffectRefresh = 0.5f;
        private const float DeckThickness = 0.3f;
        private const float DeckOverlap = 0.15f;
        private const float BeaconRange = 90f;
        private const float BeaconTop = 6.7f;
        private const float RibbonLift = 0.12f;
        private const float MirrorRange = 120f;
        private const int RingPoints = 28;

        private static readonly Color BeaconColor = new Color(1f, 0.55f, 0.2f, 1f);
        private static readonly Color CorridorColor = new Color(1f, 0.85f, 0.45f, 0.75f);
        private static readonly Color PoolColor = new Color(0.65f, 0.82f, 1f, 0.8f);
        private static readonly Color TrailColor = new Color(0.75f, 0.9f, 1f, 0.85f);
        private static readonly Color ChestColor = new Color(1f, 0.9f, 0.5f, 1f);

        private sealed class HeroEffect
        {
            public readonly string Source;
            public float Factor = 1f;
            public float NextRefresh;
            public StatModifierDefinition Definition;

            public HeroEffect(string source) => Source = source;
        }

        private readonly HeroEffect _corridor = new HeroEffect(CorridorSource);
        private readonly HeroEffect _trail = new HeroEffect(TrailSource);
        private readonly BoxCollider[] _deck = new BoxCollider[MingchengWallRules.WindowPieces * 2 + 1];
        private readonly Dictionary<int, LineRenderer> _flames = new Dictionary<int, LineRenderer>();
        private readonly Dictionary<int, Light> _lights = new Dictionary<int, Light>();
        private readonly List<Vector3> _ribbonPoints = new List<Vector3>();
        private readonly LineRenderer[] _trailLines = new LineRenderer[OverworldLayouts.ReflectedTrails.Length];
        private readonly LineRenderer[] _poolRings = new LineRenderer[OverworldLayouts.MirrorPools.Length];
        private readonly LineRenderer[] _chestRings = new LineRenderer[OverworldLayouts.ReflectedTrails.Length];

        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerMotor _motor;
        private BuffController _buffs;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer _ribbon;

        private float[] _beacons;
        private MingchengReflectionState _reflection;
        private bool _inRegion;
        private int _deckCenter = int.MinValue;
        private bool _deckOn;
        private float _nextHint;
        private int _hintTower = -1;
        private int _hintPool = -1;
        private bool _wasInCorridor;
        private bool _wasVision;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.MingchengAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.MingchengBounds, _origin) };
            _beacons = new float[OverworldLayouts.GreatWallTowerXs.Length];
        }

        private void OnDisable()
        {
            ClearHeroEffects();
            LeaveRegion();
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
            if (_beacons == null || !FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.MingchengBounds.Contains(local))
            {
                LeaveRegion();
                return;
            }

            _inRegion = true;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;
            MingchengBeaconRules.Advance(_beacons, Time.deltaTime, night);
            TickWall(local);
            TickReflection(local, night, clock != null ? clock.Day : 1);
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
                _motor = go.GetComponent<PlayerMotor>();
                _buffs = go.GetComponent<BuffController>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _motor != null;
        }

        private void LeaveRegion()
        {
            if (!_inRegion)
            {
                return;
            }

            _inRegion = false;
            ClearHeroEffects();
            SetDeck(false);
            ShowBeacons(Vector2.zero, false);
            ShowRibbon(false, 0);
            HideMirror();
            _hintTower = -1;
            _hintPool = -1;
        }

        // ------------------------------------------------------------------
        // Speed effects
        // ------------------------------------------------------------------

        private static StatModifierDefinition NewEffect(float factor) => new StatModifierDefinition
        {
            displayName = "Mingcheng",
            affectedStat = StatType.MoveSpeed,
            application = ModifierApplication.PercentAdditive,
            value = factor - 1f,
            durationSeconds = EffectSeconds,
            isDebuff = false,
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
            SetHeroEffect(_corridor, 1f);
            SetHeroEffect(_trail, 1f);
        }

        // ------------------------------------------------------------------
        // The wall and its beacons
        // ------------------------------------------------------------------

        private void TickWall(Vector2 local)
        {
            bool near = MingchengWallRules.NearLine(local, MingchengWallRules.ActiveRange);
            SetDeck(near);
            ShowBeacons(local, near);
            if (!near)
            {
                ShowRibbon(false, 0);
                SetHeroEffect(_corridor, 1f);
                _wasInCorridor = false;
                _hintTower = -1;
                return;
            }

            int center = MingchengWallRules.PieceAt(local.x);
            LayDeck(center);
            float ground = Ground(_origin + local);
            bool onDeck = MingchengWallRules.OnDeck(local, _player.position.y, ground);
            float[] towers = OverworldLayouts.GreatWallTowerXs;
            bool inCorridor = MingchengBeaconRules.InCorridor(towers, _beacons, local.x);
            bool mounted = MountController.Active != null && MountController.Active.IsMounted;
            SetHeroEffect(_corridor, MingchengBeaconRules.SpeedFactor(onDeck, inCorridor, mounted));
            bool lit = onDeck && inCorridor;
            if (lit && !_wasInCorridor)
            {
                Say(MingchengMechanicsTexts.CorridorOn);
            }

            _wasInCorridor = lit;
            ShowRibbon(onDeck, center);
            TickTower(local, onDeck, towers);
        }

        private void TickTower(Vector2 local, bool onDeck, float[] towers)
        {
            int tower = MingchengWallRules.TowerAt(towers, local);
            if (tower < 0)
            {
                _hintTower = -1;
                return;
            }

            MingchengWallAction action = MingchengWallRules.ActionAt(true, onDeck, MingchengBeaconRules.IsLit(_beacons, tower));
            if (tower != _hintTower || Time.unscaledTime >= _nextHint)
            {
                _hintTower = tower;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(action == MingchengWallAction.Climb ? MingchengMechanicsTexts.ClimbPrompt
                    : action == MingchengWallAction.LightBeacon ? MingchengMechanicsTexts.LightPrompt : MingchengMechanicsTexts.DescendPrompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            switch (action)
            {
                case MingchengWallAction.Climb:
                    LayDeck(MingchengWallRules.PieceAt(towers[tower]));
                    Physics.SyncTransforms();
                    MoveHero(MingchengWallRules.LineAt(towers[tower]), MingchengWallRules.DeckHeight + 0.1f);
                    break;
                case MingchengWallAction.LightBeacon:
                    MingchengBeaconRules.Light(_beacons, tower);
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayText(MingchengMechanicsTexts.Format(localization, MingchengMechanicsTexts.BeaconLit, MingchengBeaconRules.LitCount(_beacons) + "/" + towers.Length));
                    break;
                case MingchengWallAction.Descend:
                    MoveHero(MingchengWallRules.FootOf(towers[tower]), 0.1f);
                    break;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
        }

        private void MoveHero(Vector2 local, float lift)
        {
            Vector2 world = _origin + local;
            _controller.ClickToMove?.ClearDestination();
            _motor.Teleport(new Vector3(world.x, Ground(world) + lift, world.y), _player.rotation);
        }

        private void SetDeck(bool on)
        {
            if (_deckOn == on)
            {
                return;
            }

            _deckOn = on;
            _deckCenter = int.MinValue;
            foreach (BoxCollider piece in _deck)
            {
                if (piece != null)
                {
                    piece.enabled = false;
                }
            }
        }

        /// <summary>Lays the pool of deck pieces round piece <paramref name="center"/>, each pitched from the wall's height at one end to the other.</summary>
        private void LayDeck(int center)
        {
            if (center == _deckCenter)
            {
                return;
            }

            _deckCenter = center;
            for (int k = 0; k < _deck.Length; k++)
            {
                int index = center - MingchengWallRules.WindowPieces + k;
                BoxCollider piece = _deck[k];
                if (index < MingchengWallRules.MinIndex || index > MingchengWallRules.MaxIndex)
                {
                    if (piece != null)
                    {
                        piece.enabled = false;
                    }

                    continue;
                }

                if (piece == null)
                {
                    var go = new GameObject("Mc_WallDeck");
                    go.transform.SetParent(transform, false);
                    piece = _deck[k] = go.AddComponent<BoxCollider>();
                }

                Vector3 a = DeckPoint(index);
                Vector3 c = DeckPoint(index + 1);
                Vector3 along = c - a;
                piece.size = new Vector3(MingchengWallRules.DeckWidth, DeckThickness, along.magnitude + DeckOverlap);
                piece.center = Vector3.down * (DeckThickness * 0.5f);
                piece.transform.SetPositionAndRotation((a + c) * 0.5f, Quaternion.LookRotation(along));
                piece.enabled = true;
            }
        }

        private Vector3 DeckPoint(int index)
        {
            Vector2 world = _origin + MingchengWallRules.PointOf(index);
            return new Vector3(world.x, Ground(world) + MingchengWallRules.DeckHeight, world.y);
        }

        /// <summary>The fires of the towers in range, lit or out, and a light on each lit one.</summary>
        private void ShowBeacons(Vector2 local, bool near)
        {
            float[] towers = OverworldLayouts.GreatWallTowerXs;
            for (int i = 0; i < towers.Length; i++)
            {
                Vector2 line = MingchengWallRules.LineAt(towers[i]);
                bool show = near && MingchengBeaconRules.IsLit(_beacons, i) && Vector2.Distance(local, line) <= BeaconRange;
                _flames.TryGetValue(i, out LineRenderer flame);
                _lights.TryGetValue(i, out Light light);
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

                    continue;
                }

                Vector2 world = _origin + line;
                float y = Ground(world) + BeaconTop;
                if (flame == null)
                {
                    flame = _flames[i] = NewLine("Mc_BeaconFlame", 0.35f);
                    flame.loop = true;
                    flame.positionCount = RingPoints;
                    flame.startColor = flame.endColor = BeaconColor;
                }

                if (light == null)
                {
                    var go = new GameObject("Mc_BeaconLight");
                    go.transform.SetParent(transform, false);
                    go.transform.position = new Vector3(world.x, y + 1f, world.y);
                    light = _lights[i] = go.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = BeaconColor;
                    light.range = 26f;
                    light.shadows = LightShadows.None;
                }

                flame.enabled = true;
                light.enabled = true;
                light.intensity = 3f + Mathf.PerlinNoise(Time.time * 5f, i) * 2f;
                for (int p = 0; p < RingPoints; p++)
                {
                    float a = p * Mathf.PI * 2f / RingPoints;
                    float lick = Mathf.PerlinNoise(p * 0.8f + Time.time * 6f, i + 0.5f) * 1.4f;
                    flame.SetPosition(p, new Vector3(world.x + Mathf.Cos(a) * 0.8f, y + lick, world.y + Mathf.Sin(a) * 0.8f));
                }
            }
        }

        /// <summary>A strip of light along the deck over the pieces of the window that lie in a lit stretch.</summary>
        private void ShowRibbon(bool show, int center)
        {
            if (show)
            {
                _ribbonPoints.Clear();
                for (int k = 0; k < _deck.Length; k++)
                {
                    int index = center - MingchengWallRules.WindowPieces + k;
                    float x = index * MingchengWallRules.Step;
                    if (index < MingchengWallRules.MinIndex || index > MingchengWallRules.MaxIndex || !MingchengBeaconRules.InCorridor(OverworldLayouts.GreatWallTowerXs, _beacons, x))
                    {
                        continue;
                    }

                    _ribbonPoints.Add(DeckPoint(index) + Vector3.up * RibbonLift);
                }
            }

            if (!show || _ribbonPoints.Count < 2)
            {
                if (_ribbon != null)
                {
                    _ribbon.enabled = false;
                }

                return;
            }

            _ribbon = _ribbon != null ? _ribbon : NewLine("Mc_LightCorridor", 1.2f);
            _ribbon.startColor = _ribbon.endColor = CorridorColor;
            _ribbon.positionCount = _ribbonPoints.Count;
            for (int i = 0; i < _ribbonPoints.Count; i++)
            {
                _ribbon.SetPosition(i, _ribbonPoints[i]);
            }

            _ribbon.enabled = true;
        }

        // ------------------------------------------------------------------
        // The mirror paddies
        // ------------------------------------------------------------------

        private void TickReflection(Vector2 local, bool night, int day)
        {
            _reflection = MingchengReflectionRules.Advance(_reflection, Time.deltaTime);
            if (_wasVision && !_reflection.Active)
            {
                Say(MingchengMechanicsTexts.VisionEnd);
            }

            _wasVision = _reflection.Active;
            bool inPaddies = OverworldLayouts.MirrorPaddiesArea.Contains(local);
            ShowMirror(local, inPaddies);
            SetHeroEffect(_trail, inPaddies ? MingchengReflectionRules.SpeedFactor(OverworldLayouts.ReflectedTrails, local, _reflection) : 1f);
            if (!inPaddies)
            {
                _hintPool = -1;
                return;
            }

            int chest = MingchengReflectionRules.ChestAt(OverworldLayouts.ReflectedTrails, local, _reflection);
            if (chest >= 0)
            {
                TickChest(chest, day);
                return;
            }

            int pool = MingchengReflectionRules.PoolAt(OverworldLayouts.MirrorPools, local);
            if (pool < 0)
            {
                _hintPool = -1;
                return;
            }

            if (!_reflection.Active && (pool != _hintPool || Time.unscaledTime >= _nextHint))
            {
                _hintPool = pool;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(_reflection.Ready ? MingchengMechanicsTexts.LookPrompt : MingchengMechanicsTexts.LookResting);
            }

            if (_controller.Input.InteractPressedThisFrame && _reflection.Ready)
            {
                _reflection = MingchengReflectionRules.Look(_reflection, night);
                _wasVision = true;
                Say(MingchengMechanicsTexts.VisionStart);
            }
        }

        private void TickChest(int chest, int day)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            string mark = ChestMarkPrefix + chest;
            bool canOpen = MingchengReflectionRules.CanOpen(progress != null ? progress.GetDailyMark(mark) : 0, day);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(canOpen ? MingchengMechanicsTexts.ChestPrompt : MingchengMechanicsTexts.ChestEmpty);
            }

            if (!canOpen || !_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(MingchengReflectionRules.ChestItem(chest, day));
            if (item == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int leftover = _bag != null ? _bag.AddItem(item, 1) : 1;
            if (leftover == 0)
            {
                progress?.SetDailyMark(mark, day);
            }

            SayText(MingchengMechanicsTexts.Format(localization, leftover == 0 ? MingchengMechanicsTexts.ChestFound : MingchengMechanicsTexts.ChestBagFull, item.DisplayName));
            _nextHint = Time.unscaledTime + HintCooldown;
        }

        private void ShowMirror(Vector2 local, bool inPaddies)
        {
            MingchengPool[] pools = OverworldLayouts.MirrorPools;
            for (int i = 0; i < pools.Length; i++)
            {
                bool show = inPaddies && Vector2.Distance(local, pools[i].Center) <= MirrorRange;
                _poolRings[i] = SetRing(_poolRings[i], "Mc_Pool", pools[i].Center, pools[i].Radius, PoolColor, show);
            }

            MingchengReflectedTrail[] trails = OverworldLayouts.ReflectedTrails;
            for (int i = 0; i < trails.Length; i++)
            {
                bool show = inPaddies && _reflection.Active && Vector2.Distance(local, trails[i].Chest) <= MirrorRange * 1.5f;
                _chestRings[i] = SetRing(_chestRings[i], "Mc_Chest", trails[i].Chest, 1.4f, ChestColor, show);
                ShowTrail(i, trails[i], show);
            }
        }

        private void HideMirror()
        {
            ShowMirror(Vector2.zero, false);
            SetHeroEffect(_trail, 1f);
        }

        private void ShowTrail(int index, MingchengReflectedTrail trail, bool show)
        {
            LineRenderer line = _trailLines[index];
            if (!show)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            if (line == null)
            {
                line = _trailLines[index] = NewLine("Mc_ReflectedTrail", MingchengReflectionRules.TrailHalfWidth * 1.4f);
                line.startColor = line.endColor = TrailColor;
                line.positionCount = trail.Points.Length;
                for (int i = 0; i < trail.Points.Length; i++)
                {
                    Vector2 world = _origin + trail.Points[i];
                    line.SetPosition(i, new Vector3(world.x, Ground(world) + 0.2f, world.y));
                }
            }

            line.enabled = true;
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private float Ground(Vector2 world) => Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world));

        private LineRenderer SetRing(LineRenderer ring, string objectName, Vector2 center, float radius, Color color, bool show)
        {
            if (!show)
            {
                if (ring != null)
                {
                    ring.enabled = false;
                }

                return ring;
            }

            if (ring == null)
            {
                ring = NewLine(objectName, 0.2f);
                ring.loop = true;
                ring.positionCount = RingPoints;
                ring.startColor = ring.endColor = color;
                for (int i = 0; i < RingPoints; i++)
                {
                    float a = i * Mathf.PI * 2f / RingPoints;
                    Vector2 world = _origin + center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    ring.SetPosition(i, new Vector3(world.x, Ground(world) + 0.2f, world.y));
                }
            }

            ring.enabled = true;
            return ring;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_MingchengLine_Runtime" };
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
            SayText(MingchengMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
