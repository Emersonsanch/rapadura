using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Pughat's diver of the day on Kirta's bottom (28, <see cref="KirtaDiverRescueRules"/>): a turquoise beam rises from the bubble of the walk
    /// along the bottom where the Lodo caught today's diver; at her door Pughat says who and where (or that the abyss waits for the day); passing
    /// through that bubble frees the diver and pays at once (all or nothing; with a full bag the diver waits for the next pass).
    /// </summary>
    public sealed partial class CityStallsMechanicsDriver
    {
        private static readonly Color DiverBeamColor = new Color(0.35f, 0.95f, 0.85f, 0.85f);
        private const float DiverHintCooldown = 20f;

        private bool _kirtaOn;
        private Vector2 _costaOrigin;
        private Vector2 _pughatPoint;
        private Vector2[][] _diveCourses;
        private bool[] _diveAbyss;
        private LineRenderer _diverBeam;
        private float _nextDiverHint;
        private float _nextDiverNear;

        private void StartKirta(OverworldLayout layout, List<Rect> areas)
        {
            OverworldZone costa = layout?.Zone(OverworldLayouts.CostaAnchorZoneId);
            ReefDive[] dives = OverworldLayouts.KirtaReefDives;
            if (costa == null || dives == null || dives.Length == 0)
            {
                return;
            }

            _kirtaOn = true;
            _costaOrigin = costa.Origin;
            _diveCourses = new Vector2[dives.Length][];
            _diveAbyss = new bool[dives.Length];
            for (int i = 0; i < dives.Length; i++)
            {
                _diveCourses[i] = dives[i].Course;
                _diveAbyss[i] = dives[i].Abyss;
            }

            Vector2 town = _costaOrigin + OverworldLayouts.KirtaOffset;
            _pughatPoint = town + DoorOf(CityTownRecipes.KirtaCityId, "pughat");
            var box = Around(town, AreaHalf);
            foreach (Vector2[] course in _diveCourses)
            {
                foreach (Vector2 p in course)
                {
                    box = Rect.MinMaxRect(Mathf.Min(box.xMin, _costaOrigin.x + p.x - 10f), Mathf.Min(box.yMin, _costaOrigin.y + p.y - 10f),
                        Mathf.Max(box.xMax, _costaOrigin.x + p.x + 10f), Mathf.Max(box.yMax, _costaOrigin.y + p.y + 10f));
                }
            }

            areas.Add(box);
        }

        private void TickKirta(Vector2 hero)
        {
            if (!_kirtaOn)
            {
                return;
            }

            long today = Today();
            KirtaDiverSpot spot = KirtaDiverRescueRules.SpotOf(today, _diveCourses);
            if (spot.Dive < 0)
            {
                return;
            }

            WorldMapProgress progress = Progress();
            long freedDay = progress != null ? progress.GetDailyMark(KirtaDiverRescueRules.RescueDailyKey) : 0;
            Vector2 bubble = _costaOrigin + _diveCourses[spot.Dive][spot.Bubble];
            bool abyss = _diveAbyss[spot.Dive];
            string diver = CityStallsTexts.DiverName(KirtaDiverRescueRules.DiverOf(today));
            LocalizationManager l = Localization();

            if (_diverBeam == null)
            {
                _diverBeam = NewBeam("KirtaDiverBeam", 0.4f);
            }

            if (freedDay == today)
            {
                _diverBeam.enabled = false;
            }
            else
            {
                ShowBeam(_diverBeam, bubble, 9f, DiverBeamColor, -2.5f);
            }

            // Pughat at her door says who and where.
            if (CityServiceRules.InReach(_pughatPoint, hero, KirtaDiverRescueRules.PughatReach) && Time.unscaledTime >= _nextDiverHint)
            {
                _nextDiverHint = Time.unscaledTime + DiverHintCooldown;
                if (freedDay == today)
                {
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.DiverHintDone, diver));
                }
                else if (abyss && IsNight())
                {
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.DiverHintNight, diver));
                }
                else
                {
                    SayAbove(CityStallsTexts.Format(l, CityStallsTexts.DiverHint, diver, spot.Bubble,
                        CityStallsTexts.Get(l, CityStallsTexts.WalkNames[abyss ? 1 : 0])));
                }
            }

            // In the lagoon only the walk along the bottom (or a swimming mount) reaches a bubble: being there is enough.
            KirtaDiverCheck check = KirtaDiverRescueRules.Check(true, hero, bubble, freedDay, today);
            if (check != KirtaDiverCheck.Freed)
            {
                return;
            }

            int gold = KirtaDiverRescueRules.GoldFor(abyss);
            var reward = new List<(string, int)>
            {
                (RefineItems.Gold, gold),
                (RefineItems.BlessingStone, KirtaDiverRescueRules.BlessingStones),
                (KirtaDiverRescueRules.BubbleItemId, KirtaDiverRescueRules.BubblesFor(abyss)),
            };
            if (!GiveAll(reward))
            {
                if (Time.unscaledTime >= _nextDiverNear)
                {
                    _nextDiverNear = Time.unscaledTime + DiverHintCooldown;
                    SayText(CityStallsTexts.Get(l, CityStallsTexts.BagFull));
                }

                return;
            }

            progress?.SetDailyMark(KirtaDiverRescueRules.RescueDailyKey, (int)today);
            progress?.SetTimeMark(KirtaDiverRescueRules.RescueCountKey, progress.GetTimeMark(KirtaDiverRescueRules.RescueCountKey) + 1);
            _diverBeam.enabled = false;
            SayText(CityStallsTexts.Format(l, abyss ? CityStallsTexts.DiverFreedBubble : CityStallsTexts.DiverFreed, diver, gold));
            SayCousin(CityStallsTexts.DiverLine);
        }
    }
}
