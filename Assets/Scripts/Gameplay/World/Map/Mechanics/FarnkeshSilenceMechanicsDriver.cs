using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the silence of the Ermidas Suspensas (56, <see cref="FarnkeshSilenceRules"/>): over the cliff of the hermitages the hero's
    /// pace (measured from how far they moved), the mount and the blows given or taken (<see cref="HeroCombatClock"/>) fill a noise meter, shown
    /// as an arc round the hero's feet; full, an Echo marks the ground where the hero stands and strikes it 1.5 s later as a hazard on PlayerStats
    /// that never kills. Added to the overworld root, idle unless the layout holds the Farnkesh region; never takes the controls and reads no
    /// button. The meter is not saved (it empties on its own); the save keeps the day the hermitages' warning was last given.
    /// </summary>
    public sealed class FarnkeshSilenceMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string ToldKey = "farnkesh.silence.told";
        private const int ArcSegments = 32;
        private const float ArcRadius = 1.1f;
        private const float ArcLift = 0.12f;
        private const float FeedbackHeight = 2.4f;
        private const float MaxMeasuredSpeed = 30f;

        private static readonly Color QuietColor = new Color(0.55f, 0.8f, 0.95f, 0.8f);
        private static readonly Color LoudColor = new Color(1f, 0.35f, 0.25f, 1f);
        private static readonly Color EchoColor = new Color(0.85f, 0.75f, 1f, 1f);

        private readonly HeroCombatClock _combat = new HeroCombatClock();
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerStats _stats;
        private Material _material;
        private LineRenderer _arc;
        private LineRenderer _ring;

        private bool _inside;
        private bool _hasLast;
        private Vector3 _last;
        private float _meter;
        private float _markedAt = -1f;
        private Vector2 _mark;
        private int _toldLocal;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        /// <summary>The noise meter now, 0–100 (tests).</summary>
        public float Meter => _meter;

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
            // The hub keeps the driver on over the whole region (like the rafts' driver): Update itself idles outside the hermitages.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.FarnkeshBounds, _origin) };
        }

        private void OnEnable() => _combat.Attach();

        private void OnDisable()
        {
            _combat.Detach();
            Leave();
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
            bool inside = OverworldLayouts.ErmidasSilenceArea.Contains(local) && OverworldRules.SettlementAt(_layout, world) == null && _stats.IsAlive;
            if (!inside)
            {
                if (_inside)
                {
                    Leave();
                }

                _last = position;
                _hasLast = true;
                return;
            }

            if (!_inside)
            {
                _inside = true;
                _combat.TakeHits();
                Tell();
            }

            float dt = Time.deltaTime;
            float speed = 0f;
            if (_hasLast && dt > 1e-4f)
            {
                Vector3 step = position - _last;
                speed = Mathf.Min(MaxMeasuredSpeed, new Vector2(step.x, step.z).magnitude / dt);
            }

            _last = position;
            _hasLast = true;
            TickMeter(speed, dt);
            TickEcho(world);
            ShowArc(position);
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
                _combat.Track(go);
            }

            return _stats != null;
        }

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static bool IsLavine() => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected == CharacterId.Lavine;

        private static bool Mounted => MountController.Active != null && MountController.Active.IsMounted;

        private void TickMeter(float speed, float dt)
        {
            if (_markedAt >= 0f)
            {
                _combat.TakeHits();
                return;
            }

            FarnkeshPace pace = FarnkeshSilenceRules.PaceOf(speed, Mounted);
            float quiet = FarnkeshSilenceRules.Quiet(IsLavine(), IsNight());
            float before = _meter;
            _meter = FarnkeshSilenceRules.Step(_meter, pace, _combat.TakeHits(), quiet, dt);
            int warning = FarnkeshSilenceRules.WarningCrossed(before, _meter);
            if (warning == 1)
            {
                Say(FarnkeshSilenceTexts.WarnHalf);
            }
            else if (warning == 2)
            {
                Say(FarnkeshSilenceTexts.WarnNear);
            }

            if (FarnkeshSilenceRules.Full(_meter))
            {
                _markedAt = Time.time;
                _mark = new Vector2(_player.position.x, _player.position.z);
                Say(FarnkeshSilenceTexts.EchoMarks);
            }
        }

        private void TickEcho(Vector2 world)
        {
            FarnkeshEchoPhase phase = FarnkeshSilenceRules.EchoPhaseAt(_markedAt >= 0f ? Time.time - _markedAt : -1f);
            if (phase == FarnkeshEchoPhase.None)
            {
                HideRing();
                return;
            }

            if (phase == FarnkeshEchoPhase.Marked)
            {
                ShowRing();
                return;
            }

            _markedAt = -1f;
            _meter = FarnkeshSilenceRules.AfterEcho;
            HideRing();
            if (FarnkeshSilenceRules.StrikeHits(_mark, world))
            {
                float amount = FarnkeshSilenceRules.StrikeAmount(_stats.CurrentHealth, _stats.MaxHealth);
                if (amount > 0f)
                {
                    _stats.ApplyHazardDamage(amount);
                }

                Say(FarnkeshSilenceTexts.EchoHits);
            }
            else
            {
                Say(FarnkeshSilenceTexts.EchoMissed);
            }
        }

        /// <summary>The hermitages' warning, once per UTC day (the day is kept in the save).</summary>
        private void Tell()
        {
            int today = (int)NartesRaceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            int told = saved ? progress.GetDailyMark(ToldKey) : _toldLocal;
            if (told == today)
            {
                return;
            }

            _toldLocal = today;
            progress?.SetDailyMark(ToldKey, today);
            Say(FarnkeshSilenceTexts.Enter);
            if (IsLavine())
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), FarnkeshSilenceTexts.Get(localization, FarnkeshSilenceTexts.EnterLavine));
            }
        }

        /// <summary>Out of the hermitages: the meter and a pending Echo are dropped, the arcs hidden.</summary>
        private void Leave()
        {
            _inside = false;
            _meter = 0f;
            _markedAt = -1f;
            HideRing();
            if (_arc != null)
            {
                _arc.enabled = false;
            }
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        /// <summary>The meter as an arc round the hero's feet: longer and redder as it fills; hidden when empty.</summary>
        private void ShowArc(Vector3 feet)
        {
            float fill = _meter / FarnkeshSilenceRules.Max;
            if (fill <= 0.01f)
            {
                if (_arc != null)
                {
                    _arc.enabled = false;
                }

                return;
            }

            _arc ??= NewLine("Fk_SilenceArc", 0.12f, ArcSegments + 1);
            int points = Mathf.Max(2, Mathf.CeilToInt(fill * ArcSegments) + 1);
            _arc.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float a = Mathf.PI * 0.5f - fill * Mathf.PI * 2f * i / (points - 1);
                _arc.SetPosition(i, feet + new Vector3(Mathf.Cos(a) * ArcRadius, ArcLift, Mathf.Sin(a) * ArcRadius));
            }

            _arc.startColor = _arc.endColor = Color.Lerp(QuietColor, LoudColor, fill);
            _arc.enabled = true;
        }

        private void ShowRing()
        {
            _ring ??= NewLine("Fk_EchoRing", 0.2f, ArcSegments + 1);
            float y = _player.position.y + ArcLift;
            for (int i = 0; i <= ArcSegments; i++)
            {
                float a = Mathf.PI * 2f * i / ArcSegments;
                _ring.SetPosition(i, new Vector3(_mark.x + Mathf.Cos(a) * FarnkeshSilenceRules.EchoRadius, y, _mark.y + Mathf.Sin(a) * FarnkeshSilenceRules.EchoRadius));
            }

            _ring.startColor = _ring.endColor = EchoColor;
            _ring.enabled = true;
        }

        private void HideRing()
        {
            if (_ring != null)
            {
                _ring.enabled = false;
            }
        }

        private LineRenderer NewLine(string objectName, float width, int count)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_FarnkeshSilenceLine_Runtime" };
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
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, FarnkeshSilenceTexts.Get(localization, key));
            }
        }
    }
}
