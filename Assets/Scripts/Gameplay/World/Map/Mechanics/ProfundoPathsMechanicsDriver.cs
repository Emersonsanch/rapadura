using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the ways of the pits of the Limiar Profundo that change the hero's road: the rising currents of the Cova do Vento (86,
    /// <see cref="ProfundoUpdraftRules"/>), the sluices of the Cova do Lodo (87, <see cref="ProfundoSluiceRules"/>) and the flights of the Ponte do
    /// Novelo (89, <see cref="NoveloBridgeRules"/>). Added to the overworld root, idle unless the layout holds the region and the hero stands in
    /// one of the three parts. A current takes the controls for 16 s at most and hands them back on arrival, on death, if anything else moves
    /// the hero and in OnDisable; the dark of the bridge and the deep water never take them. Damage to the hero is a hazard on PlayerStats that
    /// stops at a floor of life; the flights lit stay in the save (time marks), the sluices for the in-game day (daily marks).
    /// </summary>
    public sealed partial class ProfundoPathsMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float PushSayCooldown = 4f;
        private const float FeedbackHeight = 2.4f;
        private const float JumpDistance = 10f;
        private const float FlightViewRange = 420f;
        private const float CageViewRange = 140f;
        private const float CageReadyHeight = 9f;
        private const float CageIdleHeight = 3f;
        private const float FlightLift = 0.4f;

        /// <summary>The three parts of the region the driver works in (region coordinates): the shaft of the wind, the column of the Lodo, the column of the bridge.</summary>
        public static readonly Rect[] PartBounds =
        {
            Rect.MinMaxRect(-600f, 720f, 840f, 1200f),
            Rect.MinMaxRect(840f, -720f, 1320f, 720f),
            Rect.MinMaxRect(1320f, -1200f, 1800f, 1200f),
        };

        private static readonly Color LitColor = new Color(1f, 0.82f, 0.38f, 1f);
        private static readonly Color DarkColor = new Color(0.3f, 0.22f, 0.45f, 0.85f);
        private static readonly Color CageColor = new Color(1f, 0.95f, 0.6f, 1f);
        private static readonly Color CageIdleColor = new Color(0.55f, 0.5f, 0.65f, 0.7f);

        private readonly bool[] _litLocal = new bool[NoveloBridgeRules.FlightCount];
        private readonly LineRenderer[] _flightLines = new LineRenderer[NoveloBridgeRules.FlightCount];
        private readonly LineRenderer[] _cageLines = new LineRenderer[NoveloBridgeRules.FlightCount - 1];
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private BuffController _buffs;
        private Material _material;

        private float _nextHint;
        private float _nextDenied;
        private int _hintSpot = -1;

        private Vector2 _lastLocal;
        private bool _haveLast;
        private Vector2 _litPoint;
        private bool _haveLitPoint;
        private float _nextPushSay;

        private int _freeing = -1;
        private float _freeElapsed;
        private int _breathsApplied;
        private bool _saidBreath;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ProfundoAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            // The hub keeps the driver on over the whole region (like the gates): Update itself idles outside the three parts.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.ProfundoBounds, _origin) };

            _sluiceLines = new LineRenderer[OverworldLayouts.ProfundoSluices.Length];
            _leverLines = new LineRenderer[OverworldLayouts.ProfundoSluices.Length];
            _drainedLocalDay = new int[OverworldLayouts.ProfundoSluices.Length];
            _ventLines = new LineRenderer[OverworldLayouts.ProfundoUpdrafts.Length];
            _wadeSlow = new StatModifierDefinition
            {
                displayName = "Água funda",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = ProfundoSluiceRules.MoveBonus,
                durationSeconds = WadeEffectSeconds,
                isDebuff = true,
            };
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndUpdraft(false);
            ClearWade();
            CancelFreeing(false);
            HideAll();
            _hintSpot = -1;
            _haveLast = false;
            _haveLitPoint = false;
            _heroState.Detach();
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

            if (_ride.Active)
            {
                TickUpdraftRide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!InParts(local))
            {
                HideAll();
                ClearWade();
                CancelFreeing(false);
                _hintSpot = -1;
                _haveLast = false;
                _haveLitPoint = false;
                return;
            }

            int lit = LitCount();
            ShowFlights(local, lit);
            ShowSluices(local);
            ShowVents(local);
            if (_stats != null && !_stats.IsAlive)
            {
                ClearWade();
                CancelFreeing(false);
                return;
            }

            PushFromTheDark(local, lit);
            TickWade(local);
            bool spot = TickCages(local, lit) || TickLevers(local) || TickVents(local);
            if (!spot)
            {
                _hintSpot = -1;
            }

            _lastLocal = local;
            _haveLast = true;
        }

        private static bool InParts(Vector2 local)
        {
            foreach (Rect part in PartBounds)
            {
                if (part.Contains(local))
                {
                    return true;
                }
            }

            return false;
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
                _buffs = go.GetComponent<BuffController>();
            }

            return _controller != null;
        }

        // ------------------------------------------------------------------
        // The flights of the Ponte do Novelo (89)
        // ------------------------------------------------------------------

        /// <summary>Whether flight <paramref name="flight"/> was lit in this save (or, without a save service, in this session).</summary>
        public static bool FlightLitIn(WorldMapProgress progress, int flight) => progress != null && progress.GetTimeMark(NoveloBridgeRules.MarkKey(flight)) > 0L;

        private bool FlightMarked(int flight) => ServiceLocator.TryGet(out WorldMapProgress progress) ? FlightLitIn(progress, flight) : _litLocal[flight];

        private int LitCount() => NoveloBridgeRules.LitCount(FlightMarked);

        private Vector2 Pillar => OverworldLayouts.PilarDoMeioOffset;

        /// <summary>The dark flights push the hero back to the lit ground they came from (or the nearest one, after a respawn or a portal).</summary>
        private void PushFromTheDark(Vector2 local, int lit)
        {
            if (!NoveloBridgeRules.IsDark(local, lit, Pillar))
            {
                _litPoint = local;
                _haveLitPoint = true;
                return;
            }

            bool jumped = _haveLast && (local - _lastLocal).sqrMagnitude > JumpDistance * JumpDistance;
            if (!_haveLitPoint || jumped || !_haveLast)
            {
                _litPoint = NoveloBridgeRules.NearestLitPoint(local, lit, Pillar);
                _haveLitPoint = true;
            }

            if (_character != null && _character.enabled)
            {
                GroundedPush.Shove(_character, NoveloBridgeRules.PushStep(local, _litPoint, Time.deltaTime));
            }

            if (Time.unscaledTime >= _nextPushSay)
            {
                _nextPushSay = Time.unscaledTime + PushSayCooldown;
                Say(ProfundoPathsTexts.DarkPush);
            }
        }

        private int CageAt(Vector2 local)
        {
            int best = -1;
            float bestSq = NoveloBridgeRules.CageReach * NoveloBridgeRules.CageReach;
            for (int k = 0; k < NoveloBridgeRules.FlightCount - 1; k++)
            {
                float sq = (OverworldLayouts.NoveloCage(k) - local).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = k;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>True while the hero stands at a cage (or is freeing one).</summary>
        private bool TickCages(Vector2 local, int lit)
        {
            if (_freeing >= 0)
            {
                TickFreeing(local);
                return true;
            }

            int cage = CageAt(local);
            if (cage < 0)
            {
                return false;
            }

            NoveloCageCheck check = NoveloBridgeRules.CheckCage(cage, lit);
            int spot = 100 + cage;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayCage(check);
            }

            if (check == NoveloCageCheck.Ok)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            if (check == NoveloCageCheck.Ok)
            {
                _freeing = cage;
                _freeElapsed = 0f;
                _breathsApplied = 0;
                _saidBreath = false;
                Say(ProfundoPathsTexts.CageFreeing);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayCage(check);
            }

            return true;
        }

        private void SayCage(NoveloCageCheck check)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case NoveloCageCheck.Ok:
                    SayText(ProfundoPathsTexts.Format(localization, ProfundoPathsTexts.CageReady, Mathf.CeilToInt(NoveloBridgeRules.FreeSeconds)));
                    break;
                case NoveloCageCheck.FlightDark:
                    Say(ProfundoPathsTexts.CageDark);
                    break;
                default:
                    Say(ProfundoPathsTexts.CageFree);
                    break;
            }
        }

        private void TickFreeing(Vector2 local)
        {
            Vector2 cage = OverworldLayouts.NoveloCage(_freeing);
            if ((cage - local).sqrMagnitude > NoveloBridgeRules.CageLeash * NoveloBridgeRules.CageLeash)
            {
                CancelFreeing(true);
                return;
            }

            _freeElapsed += Time.deltaTime;
            int due = NoveloBridgeRules.BreathsDue(_freeElapsed);
            if (_stats != null && due > _breathsApplied)
            {
                _breathsApplied = due;
                float breath = NoveloBridgeRules.BreathAmount(_stats.CurrentHealth, _stats.MaxHealth, IsNight());
                if (breath > 0f)
                {
                    _stats.ApplyHazardDamage(breath);
                    if (!_saidBreath)
                    {
                        _saidBreath = true;
                        Say(ProfundoPathsTexts.CageBreath);
                    }
                }
            }

            if (_freeElapsed >= NoveloBridgeRules.FreeSeconds)
            {
                CompleteFreeing();
            }
        }

        private void CancelFreeing(bool say)
        {
            if (_freeing < 0)
            {
                return;
            }

            _freeing = -1;
            if (say && _player != null)
            {
                Say(ProfundoPathsTexts.CageLeft);
            }
        }

        private void CompleteFreeing()
        {
            int cage = _freeing;
            _freeing = -1;
            int flight = cage + 1;
            _litLocal[flight] = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(NoveloBridgeRules.MarkKey(flight), Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(flight >= NoveloBridgeRules.FlightCount - 1
                ? ProfundoPathsTexts.Get(localization, ProfundoPathsTexts.BridgeLit)
                : ProfundoPathsTexts.Format(localization, ProfundoPathsTexts.FlightLit, flight + 1));
            if (Hit(_origin + OverworldLayouts.NoveloCage(cage), NoveloBridgeRules.BurstRadius, NoveloBridgeRules.BurstAmount, ElementType.Holy))
            {
                Say(ProfundoPathsTexts.SparksBurst);
            }
        }

        private void ShowFlights(Vector2 local, int lit)
        {
            bool near = PartBounds[2].Contains(local) || (local - new Vector2(NoveloBridgeRules.DarkWest, local.y)).sqrMagnitude < FlightViewRange * FlightViewRange;
            for (int i = 0; i < NoveloBridgeRules.FlightCount; i++)
            {
                float south = NoveloBridgeRules.FlightSouth(i);
                float mid = south + NoveloBridgeRules.FlightStep * 0.5f;
                bool visible = near && Mathf.Abs(mid - local.y) < FlightViewRange;
                LineRenderer line = _flightLines[i];
                if (!visible)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _flightLines[i] = NewLine("Pf_NoveloFlight", 1.6f);
                }

                Vector2 a = new Vector2(OverworldLayouts.NoveloBridgeX, south + 1f);
                Vector2 b = new Vector2(OverworldLayouts.NoveloBridgeX, south + NoveloBridgeRules.FlightStep - 1f);
                line.enabled = true;
                line.startColor = line.endColor = i < lit ? LitColor : DarkColor;
                line.SetPosition(0, Ground(a) + Vector3.up * FlightLift);
                line.SetPosition(1, Ground(b) + Vector3.up * FlightLift);
            }

            for (int k = 0; k < _cageLines.Length; k++)
            {
                Vector2 at = OverworldLayouts.NoveloCage(k);
                bool visible = (at - local).sqrMagnitude < CageViewRange * CageViewRange;
                LineRenderer line = _cageLines[k];
                if (!visible)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _cageLines[k] = NewLine("Pf_NoveloCage", 0.5f);
                }

                bool ready = NoveloBridgeRules.CheckCage(k, lit) == NoveloCageCheck.Ok;
                bool busy = _freeing == k;
                Vector3 foot = Ground(at);
                float height = busy ? CageIdleHeight + (CageReadyHeight - CageIdleHeight) * Mathf.Clamp01(_freeElapsed / NoveloBridgeRules.FreeSeconds) : ready ? CageReadyHeight : CageIdleHeight;
                line.enabled = true;
                line.startColor = line.endColor = ready || busy ? CageColor : CageIdleColor;
                line.SetPosition(0, foot);
                line.SetPosition(1, foot + Vector3.up * height);
            }
        }

        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static int Today() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        /// <summary>Hits the live beasts within <paramref name="radius"/> of a world point; true when one was hit.</summary>
        private bool Hit(Vector2 world, float radius, Func<float, bool, float> amount, ElementType element)
        {
            bool hit = false;
            float radiusSq = radius * radius;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - world).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                float damage = amount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (damage > 0f)
                {
                    enemy.Health.ApplyDamage(damage, element, _player.gameObject);
                    hit = true;
                }
            }

            return hit;
        }

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
        }

        private void HideAll()
        {
            HideLines(_flightLines);
            HideLines(_cageLines);
            HideLines(_sluiceLines);
            HideLines(_leverLines);
            HideLines(_ventLines);
            if (_landingLine != null)
            {
                _landingLine.enabled = false;
            }
        }

        private static void HideLines(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private LineRenderer NewLine(string objectName, float width, int points = 2)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_ProfundoPathsLine_Runtime" };
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
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(ProfundoPathsTexts.Get(localization, key));
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
