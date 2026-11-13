using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Areias Flamejantes sandstorm (the rule is <see cref="AreiasStormRules"/>): added to the overworld root, idle
    /// unless the layout holds the region and the hero stands in it. The hero keeps the controls all the time; the storm only wears
    /// health down to the rule's floor, after the warning, and the towns and the oasis areas are always safe.
    /// </summary>
    public sealed partial class AreiasMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const int StreakCount = 10;
        private const float WearTickSeconds = 1f;
        private const float FeedbackHeight = 2.4f;
        private const float BeaconRange = 220f;
        private const float BeaconHeight = 18f;

        private static readonly Color DustColor = new Color(0.86f, 0.68f, 0.4f, 0.65f);
        private static readonly Color BeaconColor = new Color(1f, 0.82f, 0.45f, 1f);

        private readonly LineRenderer[] _streaks = new LineRenderer[StreakCount];

        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerStats _stats;
        private Material _material;
        private LineRenderer _beacon;
        private int _beaconShelter = -1;

        private long _warned = long.MinValue;
        private long _hit = long.MinValue;
        private long _sheltered = long.MinValue;
        private float _wearTick;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.AreiasAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.AreiasBounds, _origin) };
            StartSouth();
        }

        private void OnDisable()
        {
            Hide();
            LeaveSouth();
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
            if (!OverworldLayouts.AreiasBounds.Contains(local))
            {
                Hide();
                LeaveSouth();
                return;
            }

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            AreiasStormPhase phase = AreiasStormRules.PhaseAt(now);
            long index = AreiasStormRules.StormIndex(now);
            bool exempt = phase != AreiasStormPhase.Calm && (OverworldRules.SettlementAt(_layout, world) != null
                || OverworldLayouts.AreiasStormFreeZones.Contains(OverworldRules.ZoneAt(_layout, world)?.Id ?? string.Empty));

            // In the storm the hero sees 6 m (07); the far south (09, 10, 12: AreiasMechanicsDriver.South.cs) runs under it, whatever its phase.
            _stormSight = AreiasStormRules.SightRange(phase, exempt, _stats.IsAlive);
            TickSouth(world, local);
            TickSight();
            if (phase == AreiasStormPhase.Calm)
            {
                if (_sheltered == index - 1L)
                {
                    Say(AreiasMechanicsTexts.StormPassed);
                    _sheltered = long.MinValue;
                }

                Hide();
                return;
            }

            if (exempt)
            {
                Hide();
                return;
            }

            ShowFeedback(phase, local);
            if (phase == AreiasStormPhase.Warning)
            {
                if (_warned != index)
                {
                    _warned = index;
                    Say(AreiasMechanicsTexts.StormWarning);
                }

                return;
            }

            // Walking beside Ammo's escorted caravan is a moving shelter (07, RouteMechanicsDriver.Caravan.cs).
            if (AreiasStormRules.IsSheltered(local, OverworldLayouts.AreiasStormShelters) || RouteMechanicsDriver.HeroBesideCaravan)
            {
                if (_sheltered != index)
                {
                    _sheltered = index;
                    Say(AreiasMechanicsTexts.StormSheltered);
                }

                _wearTick = 0f;
                return;
            }

            TickWear(index);
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
            }

            return _stats != null;
        }

        private void TickWear(long index)
        {
            if (!_stats.IsAlive)
            {
                return;
            }

            if (_hit != index)
            {
                _hit = index;
                Say(AreiasMechanicsTexts.StormHit);
            }

            _wearTick += Time.deltaTime;
            if (_wearTick < WearTickSeconds)
            {
                return;
            }

            float wear = AreiasStormRules.WearOver(_stats.CurrentHealth, _stats.MaxHealth, _wearTick);
            _wearTick = 0f;
            if (wear > 0f)
            {
                _stats.ApplyHazardDamage(wear);
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowFeedback(AreiasStormPhase phase, Vector2 local)
        {
            bool storm = phase == AreiasStormPhase.Storm;
            for (int i = 0; i < StreakCount; i++)
            {
                LineRenderer streak = _streaks[i] != null ? _streaks[i] : (_streaks[i] = NewLine("Ar_DustStreak", 0.1f, DustColor));
                streak.enabled = true;
                float sweep = Mathf.Repeat(Time.time * (storm ? 1.4f : 0.5f) + i * 0.13f, 1f);
                float across = (i - (StreakCount - 1) * 0.5f) * (storm ? 1.3f : 2.6f);
                float height = storm ? 0.4f + (i % 3) * 0.9f : 8f + (i % 3) * 2f;
                Vector3 a = _player.position + new Vector3(Mathf.Lerp(9f, -9f, sweep), height, across);
                streak.SetPosition(0, a);
                streak.SetPosition(1, a + Vector3.left * (storm ? 3f : 1.6f));
            }

            int nearest = AreiasStormRules.NearestShelter(local, OverworldLayouts.AreiasStormShelters, out float distance);
            bool show = nearest >= 0 && distance <= BeaconRange;
            _beacon = _beacon != null ? _beacon : NewLine("Ar_ShelterBeacon", 0.5f, BeaconColor);
            _beacon.enabled = show;
            if (show && nearest != _beaconShelter)
            {
                // The shelters never move: the beacon is placed again (one ground sample) only when the nearest one changes.
                _beaconShelter = nearest;
                Vector2 point = _origin + OverworldLayouts.AreiasStormShelters[nearest];
                float y = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, point));
                _beacon.SetPosition(0, new Vector3(point.x, y, point.y));
                _beacon.SetPosition(1, new Vector3(point.x, y + BeaconHeight, point.y));
            }
        }

        private void Hide()
        {
            for (int i = 0; i < StreakCount; i++)
            {
                if (_streaks[i] != null)
                {
                    _streaks[i].enabled = false;
                }
            }

            if (_beacon != null)
            {
                _beacon.enabled = false;
            }
        }

        private LineRenderer NewLine(string objectName, float width, Color color)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_AreiasLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, AreiasMechanicsTexts.Get(localization, key));
        }
    }
}
