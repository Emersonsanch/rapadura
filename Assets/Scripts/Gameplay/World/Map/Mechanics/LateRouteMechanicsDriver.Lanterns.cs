using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The lanterns between the barques (93 → 97, <see cref="SunShipLanternRules"/>): a silver pillar at the lantern rack by the Cabine da Proa on
    /// the Moon's deck and a gold one at the stand by the Cabine da Popa on the Sun's deck. [E] at the rack lights a lantern (a small flame over
    /// the hero's head); it burns down as the hero walks, every blow taken shakes some out, and being carried (a ride of any driver, a teleport,
    /// death) puts it out. [E] at the stand sets it and pays; the fifth of the UTC day completes the voyage of the light. The lantern itself is
    /// never saved (it is a walk, not a belonging); the count of the day is one time mark, written only when a lantern is set.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int LanternLineKey = 600;
        private const int LanternSpot = 600;
        private const float FlameHintSeconds = 30f;

        /// <summary>Where the lantern part runs (region coordinates of the Céu): the trunk up the river of light from the Moon's deck to the Sun's.</summary>
        public static readonly Rect LanternBounds = Rect.MinMaxRect(-40f, 980f, 500f, 2530f);

        private static readonly Color RackColor = new Color(0.8f, 0.85f, 0.95f, 1f);
        private static readonly Color SunStandColor = new Color(1f, 0.78f, 0.3f, 1f);
        private static readonly Color FlameColor = new Color(1f, 0.9f, 0.55f, 1f);

        private bool _lanternOn;
        private float _flame;
        private Vector2 _lanternLast;
        private int _pendingBlows;
        private float _nextFlameHint;

        /// <summary>Every frame: the carried flame burns, takes the blows of the frame, and goes out when the hero is carried.</summary>
        private void TickLanternCarry(Vector2 world)
        {
            if (!_lanternOn)
            {
                _pendingBlows = 0;
                return;
            }

            float jump = Vector2.Distance(world, _lanternLast);
            _lanternLast = world;
            if (!_stats.IsAlive)
            {
                DropLantern(LateRouteMechanicsTexts.LanternOut);
                return;
            }

            _flame = SunShipLanternRules.Burn(_flame, Time.deltaTime, _pendingBlows);
            _pendingBlows = 0;
            switch (SunShipLanternRules.Step(_flame, jump, _ride.Active || !_controller.enabled))
            {
                case LanternStep.Carried:
                    DropLantern(LateRouteMechanicsTexts.LanternCarried);
                    return;
                case LanternStep.BurntOut:
                    DropLantern(LateRouteMechanicsTexts.LanternOut);
                    return;
            }

            if (Time.unscaledTime >= _nextFlameHint)
            {
                _nextFlameHint = Time.unscaledTime + FlameHintSeconds;
                SayFormat(LateRouteMechanicsTexts.LanternFlame, Mathf.CeilToInt(_flame));
            }
        }

        /// <summary>Puts the carried lantern out (<paramref name="key"/>: the line to say, or null).</summary>
        private void DropLantern(string key)
        {
            if (!_lanternOn)
            {
                return;
            }

            _lanternOn = false;
            _flame = 0f;
            if (key != null)
            {
                Say(key);
            }
        }

        /// <summary>True while the hero stands at the rack or the stand.</summary>
        private bool TickLanternStations(Vector2 local, double now)
        {
            if (_lanternOn)
            {
                Vector3 head = _player.position + Vector3.up * 2.3f;
                Segment(LanternLineKey + 2, head, head + Vector3.up * Mathf.Lerp(0.15f, 0.6f, _flame / SunShipLanternRules.FlameSeconds), 0.35f, FlameColor);
            }

            if (!LanternBounds.Contains(local))
            {
                return false;
            }

            Vector2 rack = OverworldLayouts.MoonLanternRack;
            Vector2 stand = OverworldLayouts.SunLanternStand;
            Pillar(LanternLineKey, rack + _ceuOrigin, 4f, 0.5f, RackColor);
            Pillar(LanternLineKey + 1, stand + _ceuOrigin, 4f, 0.5f, SunStandColor);
            if (!CanInteract)
            {
                return false;
            }

            bool atRack = SunShipLanternRules.AtStation(rack, local);
            bool atStand = !atRack && SunShipLanternRules.AtStation(stand, local);
            if (!atRack && !atStand)
            {
                return false;
            }

            ServiceLocator.TryGet(out WorldMapProgress progress);
            long day = UtcDay(now);
            int setToday = progress != null ? SunShipLanternRules.SetOn(progress.GetTimeMark(SunShipLanternRules.SetKey), day) : 0;
            return atRack ? TickRack(setToday, local) : TickSunStand(progress, day, setToday);
        }

        private bool TickRack(int setToday, Vector2 local)
        {
            int spot = LanternSpot;
            if (_lanternOn)
            {
                Hint(spot, Format(LateRouteMechanicsTexts.LanternFlame, Mathf.CeilToInt(_flame)));
                return true;
            }

            if (!SunShipLanternRules.CanSet(setToday))
            {
                HintKey(spot, LateRouteMechanicsTexts.RackDone);
                return true;
            }

            Hint(spot, Format(LateRouteMechanicsTexts.RackPrompt, setToday));
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            _lanternOn = true;
            _flame = SunShipLanternRules.FlameSeconds;
            _lanternLast = local + _ceuOrigin;
            _pendingBlows = 0;
            _nextFlameHint = Time.unscaledTime + FlameHintSeconds;
            SayFormat(LateRouteMechanicsTexts.LanternTaken, Mathf.RoundToInt(SunShipLanternRules.FlameSeconds));
            return true;
        }

        private bool TickSunStand(WorldMapProgress progress, long day, int setToday)
        {
            int spot = LanternSpot + 1;
            if (!_lanternOn)
            {
                HintKey(spot, SunShipLanternRules.CanSet(setToday) ? LateRouteMechanicsTexts.SunStandEmpty : LateRouteMechanicsTexts.RackDone);
                return true;
            }

            if (!SunShipLanternRules.CanSet(setToday))
            {
                DropLantern(null);
                HintKey(spot, LateRouteMechanicsTexts.RackDone);
                return true;
            }

            HintKey(spot, LateRouteMechanicsTexts.SunStandPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            int gold = SunShipLanternRules.GoldFor(setToday);
            if (progress == null || (gold > 0 && !PayGold(gold)))
            {
                // A full bag keeps the lantern: empty it and set the lantern again.
                return true;
            }

            int count = setToday + 1;
            progress.SetTimeMark(SunShipLanternRules.SetKey, SunShipLanternRules.StoreSet(day, count));
            DropLantern(null);
            SayFormat(LateRouteMechanicsTexts.LanternSet, gold, count);
            if (count == SunShipLanternRules.LanternsPerVoyage)
            {
                Say(LateRouteMechanicsTexts.VoyageDone);
            }

            return true;
        }
    }
}
