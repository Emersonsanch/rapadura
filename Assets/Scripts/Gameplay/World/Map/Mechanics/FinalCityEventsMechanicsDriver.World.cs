using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// What the last capitals change in the world round them: the everlasting night of the Rio das Doze Horas (<see cref="RioEverNightRules"/>), the
    /// low gravity of the Cais da Coluna (<see cref="CeuLowGravityRules"/>) and the pay of the Maré de Treva (<see cref="ProfundoTideRewardRules"/>).
    /// </summary>
    public sealed partial class FinalCityEventsMechanicsDriver
    {
        private float _nightHold;
        private bool _holdingNight;
        private PlayerMotor _gravityMotor;
        private long _tidePaidLocal;

        private void TickEverNight(Vector2 local)
        {
            bool inside = OverworldLayouts.RioBounds.Contains(local);
            _nightHold = RioEverNightRules.Step(_nightHold, inside, Time.unscaledDeltaTime);
            if (_nightHold > 0f || _holdingNight)
            {
                EnvironmentOverrides.SetNightHold(_nightHold);
                _holdingNight = _nightHold > 0f;
            }
        }

        private void TickLowGravity(Vector2 local)
        {
            PlayerMotor wanted = CeuLowGravityRules.InLowGravity(local) ? _motor : null;
            if (wanted == _gravityMotor)
            {
                return;
            }

            ReleaseGravity();
            if (wanted != null)
            {
                wanted.GravityScale = CeuLowGravityRules.GravityScale;
                _gravityMotor = wanted;
                Say(FinalCityEventsTexts.GravityEnter);
            }
        }

        private void ReleaseGravity()
        {
            if (_gravityMotor != null)
            {
                _gravityMotor.GravityScale = 1f;
            }

            _gravityMotor = null;
        }

        /// <summary>Gives the sky back to the clock and the hero back his weight (the driver is switched off away from these regions).</summary>
        private void ReleaseWorld()
        {
            if (_holdingNight || _nightHold > 0f)
            {
                _nightHold = 0f;
                _holdingNight = false;
                EnvironmentOverrides.SetNightHold(0f);
            }

            ReleaseGravity();
        }

        private void HandleTideEnded(ProfundoTideEndedEvent evt)
        {
            if (!_hasFortress || !FindPlayer())
            {
                return;
            }

            long hour = ProfundoTideRewardRules.HourIndex(WorldEventCalendar.UtcNow());
            long mark = GetMark(ProfundoTideRewardRules.PaidKey, _tidePaidLocal);
            if (!ProfundoTideRewardRules.ShouldPay(evt.Rings, mark, hour))
            {
                return;
            }

            int gold = ProfundoTideRewardRules.GoldFor(evt.Rings);
            int stones = ProfundoTideRewardRules.StonesFor(evt.Rings);
            if (!Grant(gold, stones))
            {
                return; // the bag is full: the next tide of the hour pays
            }

            _tidePaidLocal = ProfundoTideRewardRules.PaidMark(hour);
            SetMark(ProfundoTideRewardRules.PaidKey, _tidePaidLocal);
            SayFormat(FinalCityEventsTexts.TidePaid, evt.Rings, gold, stones);
            if (evt.Rings >= ProfundoLampRules.RingCount)
            {
                CousinLine("dark_tide");
            }
        }
    }
}
