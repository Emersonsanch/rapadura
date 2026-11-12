using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The dhows of the Canal de Liyongo (30, <see cref="DhowRules"/>): a landing column at each end of the canal; [E] pays the fare and sails
    /// the canal through a <see cref="MechanicsRide"/> (the controls come back on arrival, on death, if anything else moves the hero and in
    /// OnDisable). The low tide keeps the dhows at their landings. Nothing is saved.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float LandingHeight = 6f;

        private static readonly Color LandingOpenColor = new Color(1f, 0.75f, 0.35f, 1f);
        private static readonly Color LandingDryColor = new Color(0.5f, 0.42f, 0.35f, 0.8f);

        private readonly MechanicsRide _dhowRide = new MechanicsRide();
        private readonly LineRenderer[] _dhowLandings = new LineRenderer[2];
        private OverworldView _view;
        private float _lastDhowRide = float.NegativeInfinity;
        private int _dhowTo = -1;
        private int _dhowHint = -1;
        private bool _inLiyongo;

        /// <summary>One frame of a dhow under sail; true while it holds the controls (the rest of Update waits).</summary>
        private bool TickDhowRide()
        {
            if (!_dhowRide.Active)
            {
                return false;
            }

            RideStep step = _dhowRide.Tick(Time.deltaTime);
            if (step != RideStep.Riding)
            {
                EndDhowRide(step == RideStep.Arrived);
            }

            return true;
        }

        private void TickDhow(Vector2 local, long unix)
        {
            if (!OverworldLayouts.LiyongoDhowBounds.Contains(local))
            {
                LeaveDhow();
                return;
            }

            _inLiyongo = true;
            Vector2[] course = OverworldLayouts.LiyongoDhowCourse;
            CostaTidePhase tide = CostaTideRules.PhaseAt(unix);
            bool open = DhowRules.CanalOpen(tide);
            for (int i = 0; i < 2; i++)
            {
                Vector2 at = i == 0 ? course[0] : course[course.Length - 1];
                _dhowLandings[i] = Pillar(_dhowLandings[i], "Cs_DhowLanding", _costaOrigin + at, 0.5f, LandingHeight, open ? LandingOpenColor : LandingDryColor);
            }

            int landing = LimiarBoatRules.LandingAt(course, local);
            if (landing < 0 || !_stats.IsAlive || _controller.Input == null)
            {
                _dhowHint = -1;
                return;
            }

            int gold = GoldInBag();
            bool sails = DhowRules.CanSail(landing, tide, Time.unscaledTime - _lastDhowRide, gold);
            if (_dhowHint != landing || Time.unscaledTime >= _nextHint)
            {
                _dhowHint = landing;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayR4(!open ? CoastDeltaPassTexts.DhowDry : gold < DhowRules.Fare ? CoastDeltaPassTexts.DhowNoGold : CoastDeltaPassTexts.DhowPrompt);
            }

            if (!sails)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame && SpendGold(DhowRules.Fare))
            {
                BeginDhow(landing);
            }
        }

        private void BeginDhow(int landing)
        {
            Vector2[] course = RidePath.ToWorld(LimiarBoatRules.Course(OverworldLayouts.LiyongoDhowCourse, landing), _costaOrigin, false);
            if (!_dhowRide.Begin(_player.gameObject, _layout, course, DhowRules.Speed, 0f, DhowRules.Hover))
            {
                PayGold(DhowRules.Fare);
                return;
            }

            _dhowTo = 1 - landing;
            _dhowHint = -1;
            SayR4(CoastDeltaPassTexts.DhowSailing);
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and sets the hero on the far landing.</summary>
        private void EndDhowRide(bool arrived)
        {
            _dhowRide.End();
            if (_dhowTo < 0)
            {
                return;
            }

            int to = _dhowTo;
            _dhowTo = -1;
            _lastDhowRide = Time.unscaledTime;
            _view ??= GetComponent<OverworldView>();
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2[] course = OverworldLayouts.LiyongoDhowCourse;
            Vector2 local = to == 0 ? course[0] : course[course.Length - 1];
            Vector2 before = to == 0 ? course[1] : course[course.Length - 2];
            Vector2 end = _costaOrigin + local;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, local - before);
            _view.ResetTracking(end);
            SayR4(CoastDeltaPassTexts.DhowLanded);
        }

        private void LeaveDhow()
        {
            if (!_inLiyongo)
            {
                return;
            }

            _inLiyongo = false;
            _dhowHint = -1;
            Off(_dhowLandings);
        }
    }
}
