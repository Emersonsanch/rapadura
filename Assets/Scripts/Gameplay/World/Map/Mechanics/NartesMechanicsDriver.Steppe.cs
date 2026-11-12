using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Three mechanics of the Estepe dos Nartes on the same driver as the race, the chain and the golds: the dream chests of the Fortim (49,
    /// <see cref="NartesDreamRules"/>), the herding of the Pradaria dos Mil Cavalos (51, <see cref="PrairieHerdRules"/>: the horses are columns
    /// that graze and follow; the day each was delivered and the points stay in the save) and the slow clock of the Arvoredo (50,
    /// <see cref="GroveSlowTimeRules"/>: the TimeOfDayManager's pace is halved in the grove and put back on leaving, on falling and in
    /// OnDisable). Rewards are gold nuggets (<c>Pay</c>); no damage, never takes the controls.
    /// </summary>
    public sealed partial class NartesMechanicsDriver
    {
        private const float DreamViewRange = 200f;
        private const float DreamBeaconHeight = 18f;
        private const float HorseViewRange = 150f;
        private const float HorseHeight = 2.2f;

        private static readonly Color DreamColor = new Color(0.75f, 0.6f, 1f, 0.9f);
        private static readonly Color HorseColor = new Color(0.55f, 0.36f, 0.2f, 1f);

        private readonly List<int> _following = new List<int>();
        private LineRenderer _dreamBeacon;
        private LineRenderer[] _horseLines;
        private bool _inPrairie;
        private bool _inGrove;
        private TimeOfDayManager _groveClock;
        private float _groveOutside = 1f;
        private float _groveSlowed = 0.5f;

        private void TickSteppe(Vector2 local, double now, long today)
        {
            TickDreams(local, today);
            TickHerd(local, now, today);
            TickGrove(local);
        }

        private void LeaveSteppe()
        {
            if (_dreamBeacon != null)
            {
                _dreamBeacon.enabled = false;
            }

            LeaveHerd();
            LeaveGrove();
        }

        // ------------------------------------------------------------------
        // 49 · The dreams of the Fortim
        // ------------------------------------------------------------------

        private bool InFortimMap(Vector2 local)
        {
            foreach (Rect area in OverworldLayouts.FortimDreamAreas)
            {
                if (area.Contains(local))
                {
                    return true;
                }
            }

            return false;
        }

        private void TickDreams(Vector2 local, long today)
        {
            if (!InFortimMap(local))
            {
                if (_dreamBeacon != null)
                {
                    _dreamBeacon.enabled = false;
                }

                return;
            }

            Vector2[] spots = OverworldLayouts.FortimDreamSpots;
            int spot = NartesDreamRules.SpotOf(today, spots.Length);
            long heard = LastDay(NartesDreamRules.HeardKey);
            if (heard != today && OverworldRules.SettlementAt(_layout, _origin + local) != null)
            {
                heard = today;
                SetDay(NartesDreamRules.HeardKey, today);
                ServiceLocator.TryGet(out LocalizationManager localization);
                string where = DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.CompassKeys[NartesDreamRules.CompassOf(OverworldLayouts.FortimOffset, spots[spot])]);
                SayText(string.Format(DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.Dream),
                    NartesDreamRules.ToldDistance(OverworldLayouts.FortimOffset, spots[spot]), where));
            }

            bool open = NartesDreamRules.Open(heard, LastDay(NartesDreamRules.ChestKey), today);
            bool show = open && (spots[spot] - local).sqrMagnitude <= DreamViewRange * DreamViewRange;
            if (show)
            {
                _dreamBeacon = _dreamBeacon != null ? _dreamBeacon : NewLine("Nt_DreamChest", 0.8f);
                _dreamBeacon.startColor = _dreamBeacon.endColor = DreamColor;
                Vector3 foot = Ground(spots[spot]);
                _dreamBeacon.SetPosition(0, foot);
                _dreamBeacon.SetPosition(1, foot + Vector3.up * DreamBeaconHeight);
                _dreamBeacon.enabled = true;
            }
            else if (_dreamBeacon != null)
            {
                _dreamBeacon.enabled = false;
            }

            if (!open || !NartesDreamRules.InReach(spots, spot, local) || _controller.Input == null)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayRoute(DreamsHerdGroveMirrorsTexts.DreamPrompt);
            }

            if (_controller.Input.InteractPressedThisFrame && Pay(NartesDreamRules.Purse))
            {
                SetDay(NartesDreamRules.ChestKey, today);
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(string.Format(DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.DreamFound), NartesDreamRules.Purse));
            }
        }

        // ------------------------------------------------------------------
        // 51 · Herding the loose horses
        // ------------------------------------------------------------------

        private void TickHerd(Vector2 local, double now, long today)
        {
            if (!OverworldLayouts.PrairieHerdBounds.Contains(local))
            {
                LeaveHerd();
                return;
            }

            _inPrairie = true;
            Vector2[] spots = OverworldLayouts.PrairieHorseSpots;
            _horseLines ??= new LineRenderer[spots.Length];
            Vector2 forward = new Vector2(_player.forward.x, _player.forward.z);

            // Delivering: every following horse in the corral counts.
            if (_following.Count > 0 && PrairieHerdRules.InCorral(OverworldLayouts.PocoDaTacaOffset, local))
            {
                DeliverAll(today);
            }

            int nearest = -1;
            float best = PrairieHerdRules.LeadReach * PrairieHerdRules.LeadReach;
            for (int i = 0; i < spots.Length; i++)
            {
                int follow = _following.IndexOf(i);
                bool free = PrairieHerdRules.Free(LastDay(PrairieHerdRules.DayKey(i)), today);
                Vector2 at = follow >= 0 ? PrairieHerdRules.FollowAt(local, forward, follow) : PrairieHerdRules.GrazingAt(spots[i], i, now);
                bool show = (free || follow >= 0) && (at - local).sqrMagnitude <= HorseViewRange * HorseViewRange;
                ShowHorse(i, at, show);
                if (follow < 0 && free)
                {
                    float sq = (at - local).sqrMagnitude;
                    if (sq <= best)
                    {
                        best = sq;
                        nearest = i;
                    }
                }
            }

            if (nearest < 0 || !PrairieHerdRules.CanLead(_following.Count) || _controller.Input == null)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayRoute(DreamsHerdGroveMirrorsTexts.HorsePrompt);
            }

            if (_controller.Input.InteractPressedThisFrame)
            {
                _following.Add(nearest);
                _nextHint = Time.unscaledTime + HintCooldown;
                SayRoute(DreamsHerdGroveMirrorsTexts.HorseFollows);
            }
        }

        private void DeliverAll(long today)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            ServiceLocator.TryGet(out LocalizationManager localization);
            for (int k = _following.Count - 1; k >= 0; k--)
            {
                if (!Pay(PrairieHerdRules.PurseEach))
                {
                    return;
                }

                int horse = _following[k];
                _following.RemoveAt(k);
                SetDay(PrairieHerdRules.DayKey(horse), today);
                if (progress != null)
                {
                    progress.SetTimeMark(PrairieHerdRules.PointsKey, progress.GetTimeMark(PrairieHerdRules.PointsKey) + 1L);
                }

                SayText(string.Format(DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.HorseDelivered), 1, PrairieHerdRules.PurseEach));
            }
        }

        private void ShowHorse(int horse, Vector2 at, bool show)
        {
            LineRenderer line = _horseLines[horse];
            if (!show)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            line = line != null ? line : (_horseLines[horse] = NewLine("Nt_LooseHorse", 0.9f));
            line.startColor = line.endColor = HorseColor;
            Vector3 foot = Ground(at);
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * HorseHeight);
            line.enabled = true;
        }

        private void LeaveHerd()
        {
            if (!_inPrairie)
            {
                return;
            }

            _inPrairie = false;
            if (_following.Count > 0)
            {
                _following.Clear();
                if (_player != null && isActiveAndEnabled)
                {
                    SayRoute(DreamsHerdGroveMirrorsTexts.HorsesLost);
                }
            }

            if (_horseLines != null)
            {
                foreach (LineRenderer line in _horseLines)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // 50 · The slow clock of the Arvoredo
        // ------------------------------------------------------------------

        private void TickGrove(Vector2 local)
        {
            if (!OverworldLayouts.SlowGroveBounds.Contains(local))
            {
                LeaveGrove();
                return;
            }

            if (_inGrove || !ServiceLocator.TryGet(out TimeOfDayManager clock) || clock == null)
            {
                return;
            }

            _inGrove = true;
            _groveClock = clock;
            _groveOutside = clock.TimeScale;
            _groveSlowed = GroveSlowTimeRules.SlowedScale(_groveOutside);
            clock.TimeScale = _groveSlowed;
            SayRoute(DreamsHerdGroveMirrorsTexts.GroveSlow);
        }

        private void LeaveGrove()
        {
            if (!_inGrove)
            {
                return;
            }

            _inGrove = false;
            if (_groveClock != null)
            {
                _groveClock.TimeScale = GroveSlowTimeRules.RestoredScale(_groveClock.TimeScale, _groveSlowed, _groveOutside);
                _groveClock = null;
            }

            if (_player != null && isActiveAndEnabled && _stats != null && _stats.IsAlive)
            {
                SayRoute(DreamsHerdGroveMirrorsTexts.GroveLeft);
            }
        }

        private void SayRoute(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(DreamsHerdGroveMirrorsTexts.Get(localization, key));
        }
    }
}
