using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The rising currents of the Cova do Vento Destruidor (86, <see cref="ProfundoUpdraftRules"/>).</summary>
    public sealed partial class ProfundoPathsMechanicsDriver
    {
        private const float VentViewRange = 160f;
        private const float VentHeight = 22f;
        private const float VentRestHeight = 5f;
        private const float RideHover = 1f;

        private static readonly Color WindColor = new Color(0.7f, 0.86f, 1f, 1f);
        private static readonly Color StillColor = new Color(0.38f, 0.42f, 0.55f, 0.7f);

        private LineRenderer[] _ventLines;
        private LineRenderer _landingLine;
        private float _lastUpdraft = float.NegativeInfinity;
        private int _rideUpdraft = -1;
        private int _pecksApplied;
        private bool _saidPeck;

        /// <summary>True while the hero stands at a vent.</summary>
        private bool TickVents(Vector2 local)
        {
            ProfundoUpdraft[] updrafts = OverworldLayouts.ProfundoUpdrafts;
            int index = ProfundoUpdraftRules.VentAt(updrafts, local);
            if (index < 0)
            {
                return false;
            }

            ProfundoUpdraft updraft = updrafts[index];
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            var route = new SkyRoute { Id = updraft.Id, Kind = SkyRouteKind.Glide, A = updraft.Vent, B = updraft.Landing };
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
            bool tide = ProfundoLampRules.PhaseAt(Time.time) == ProfundoTidePhase.Rising;
            ProfundoUpdraftCheck check = ProfundoUpdraftRules.Check(tide, zones, Time.unscaledTime - _lastUpdraft);
            int spot = 200 + index;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayUpdraft(check);
            }

            if (check == ProfundoUpdraftCheck.Ok)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            if (check == ProfundoUpdraftCheck.Ok)
            {
                BeginUpdraft(index);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayUpdraft(check);
            }

            return true;
        }

        private void SayUpdraft(ProfundoUpdraftCheck check)
        {
            switch (check)
            {
                case ProfundoUpdraftCheck.Ok:
                    Say(ProfundoPathsTexts.UpdraftReady);
                    break;
                case ProfundoUpdraftCheck.TideShut:
                    Say(ProfundoPathsTexts.UpdraftTide);
                    break;
                case ProfundoUpdraftCheck.ZoneBlocked:
                    Say(ProfundoPathsTexts.UpdraftBlocked);
                    break;
                default:
                    Say(ProfundoPathsTexts.UpdraftResting);
                    break;
            }
        }

        private void BeginUpdraft(int index)
        {
            ProfundoUpdraft updraft = OverworldLayouts.ProfundoUpdrafts[index];
            Vector2[] course = { _origin + updraft.Vent, _origin + updraft.Landing };
            ClearWade();
            if (!_ride.Begin(_player.gameObject, _layout, course, ProfundoUpdraftRules.LiftSpeed, ProfundoUpdraftRules.LiftArc, RideHover))
            {
                return;
            }

            _rideUpdraft = index;
            _pecksApplied = 0;
            _saidPeck = false;
            _hintSpot = -1;
            Say(ProfundoPathsTexts.UpdraftLift);
        }

        private void TickUpdraftRide()
        {
            RideStep step = _ride.Tick(Time.deltaTime);
            if (step == RideStep.Riding)
            {
                PeckRider();
                return;
            }

            EndUpdraft(step == RideStep.Arrived);
        }

        /// <summary>The wind harpies peck at the hero hanging in the current: a hazard, never past the floor of life.</summary>
        private void PeckRider()
        {
            int due = ProfundoUpdraftRules.PecksDue(_ride.Elapsed);
            if (_stats == null || _pecksApplied >= due)
            {
                return;
            }

            _pecksApplied = due;
            float peck = ProfundoUpdraftRules.PeckAmount(_stats.CurrentHealth, _stats.MaxHealth, IsNight());
            if (peck <= 0f)
            {
                return;
            }

            _stats.ApplyHazardDamage(peck);
            if (!_saidPeck)
            {
                _saidPeck = true;
                Say(ProfundoPathsTexts.UpdraftPecked);
            }
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and sets the hero down on arrival.</summary>
        private void EndUpdraft(bool arrived)
        {
            _ride.End();
            if (_rideUpdraft < 0)
            {
                return;
            }

            ProfundoUpdraft updraft = OverworldLayouts.ProfundoUpdrafts[_rideUpdraft];
            _rideUpdraft = -1;
            _lastUpdraft = Time.unscaledTime;
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2 end = _origin + updraft.Landing;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, updraft.Landing - updraft.Vent);
            _view.ResetTracking(end);
            _haveLast = false;
            bool hit = Hit(end, ProfundoUpdraftRules.GustRadius, ProfundoUpdraftRules.GustAmount, ElementType.Wind);
            Say(hit ? ProfundoPathsTexts.UpdraftGust : ProfundoPathsTexts.UpdraftLanded);
        }

        private void ShowVents(Vector2 local)
        {
            ProfundoUpdraft[] updrafts = OverworldLayouts.ProfundoUpdrafts;
            bool tide = ProfundoLampRules.PhaseAt(Time.time) == ProfundoTidePhase.Rising;
            int near = -1;
            for (int i = 0; i < updrafts.Length; i++)
            {
                bool visible = (updrafts[i].Vent - local).sqrMagnitude < VentViewRange * VentViewRange;
                LineRenderer line = _ventLines[i];
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
                    line = _ventLines[i] = NewLine("Pf_Updraft", 1.2f);
                }

                Vector3 foot = Ground(updrafts[i].Vent);
                line.enabled = true;
                line.startColor = tide ? StillColor : WindColor;
                line.endColor = new Color(WindColor.r, WindColor.g, WindColor.b, tide ? 0.05f : 0.15f);
                line.SetPosition(0, foot);
                line.SetPosition(1, foot + Vector3.up * (tide ? VentRestHeight : VentHeight));
                if ((updrafts[i].Vent - local).sqrMagnitude <= ProfundoUpdraftRules.Reach * ProfundoUpdraftRules.Reach * 4f)
                {
                    near = i;
                }
            }

            // Where the nearest current sets the hero down, on the outer ring.
            if (near < 0)
            {
                if (_landingLine != null)
                {
                    _landingLine.enabled = false;
                }

                return;
            }

            _landingLine = _landingLine != null ? _landingLine : NewLine("Pf_UpdraftLanding", 0.4f);
            Vector3 landing = Ground(updrafts[near].Landing);
            _landingLine.enabled = true;
            _landingLine.startColor = _landingLine.endColor = WindColor;
            _landingLine.SetPosition(0, landing);
            _landingLine.SetPosition(1, landing + Vector3.up * VentHeight);
        }
    }
}
