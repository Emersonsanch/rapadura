using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Festa das Lanternas of Mingcheng (41, <see cref="MingchengLanternFestaRules"/>): five riddle lanterns, four answer lanterns on the plaza.</summary>
    public sealed partial class CityEventsMechanicsDriver
    {
        private const float FestaRange = 120f;
        private const float FestaPlaza = 10f;
        private const float FestaSoonEvery = 60f;
        private const float RiddleRepeatSeconds = 8f;
        private const float RiddleBeamHeight = 5f;
        private const float AnswerBeamHeight = 3.2f;

        private static readonly Color RiddleColor = new Color(0.95f, 0.25f, 0.2f, 0.9f);
        private static readonly Color RiddleTakenColor = new Color(1f, 0.97f, 0.85f, 1f);
        private static readonly Color RiddleSolvedColor = new Color(1f, 0.82f, 0.3f, 0.95f);
        private static readonly Color RiddleDarkColor = new Color(0.35f, 0.15f, 0.15f, 0.6f);

        private static readonly Color[] AnswerColors =
        {
            new Color(1f, 0.7f, 0.2f, 0.95f), // Sun
            new Color(0.85f, 0.88f, 1f, 0.95f), // Moon
            new Color(1f, 0.45f, 0.25f, 0.95f), // Carp
            new Color(0.75f, 0.55f, 0.3f, 0.95f), // Bell
        };

        private readonly float[] _darkUntil = new float[MingchengLanternFestaRules.LanternCount];
        private LineRenderer[] _riddleBeams;
        private LineRenderer[] _answerBeams;
        private int _carrying = -1;
        private int _festaOpenDay;
        private int _festaWarnedDay = int.MinValue;
        private long _festaLocalNight;
        private long _festaLocalSolved;
        private long _festaLocalBonus;
        private float _nextFestaHint;
        private int _festaHintSpot = -1;
        private float _nextFestaSoon;
        private float _nextRiddleRepeat;
        private float _nextBonusTry;

        private void TickFesta(Vector2 local)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.MingchengOffset);
            if (distance > FestaRange || !ServiceLocator.TryGet(out TimeOfDayManager clock) || clock == null)
            {
                HideFesta();
                _carrying = -1;
                _festaOpenDay = 0;
                return;
            }

            int day = clock.Day;
            float hour = clock.CurrentHour;
            if (!MingchengLanternFestaRules.IsOpen(day, hour, out int festaDay))
            {
                ClosedFesta(day, hour, distance);
                return;
            }

            if (_festaOpenDay != festaDay)
            {
                _festaOpenDay = festaDay;
                _carrying = -1;
                Say(CityEventsTexts.FestaStart);
            }

            long solved = MingchengLanternFestaRules.SolvedTonight(GetMark(MingchengLanternFestaRules.NightKey, _festaLocalNight),
                GetMark(MingchengLanternFestaRules.SolvedKey, _festaLocalSolved), festaDay);
            ShowFesta(solved);
            if (MingchengLanternFestaRules.BonusDue(solved, GetMark(MingchengLanternFestaRules.BonusKey, _festaLocalBonus), festaDay) && Time.unscaledTime >= _nextBonusTry)
            {
                PayFestaBonus(festaDay);
            }

            if (!_stats.IsAlive)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int answer = _carrying >= 0 ? MingchengLanternFestaRules.NearestWithin(OverworldLayouts.MingchengAnswerLanterns, local, MingchengLanternFestaRules.AnswerReach) : -1;
            if (answer >= 0)
            {
                TickAnswer(localization, festaDay, solved, (LanternAnswer)answer);
                return;
            }

            int lantern = MingchengLanternFestaRules.NearestWithin(OverworldLayouts.MingchengRiddleLanterns, local, MingchengLanternFestaRules.LanternReach);
            if (lantern >= 0)
            {
                TickRiddleLantern(localization, festaDay, solved, lantern);
                return;
            }

            _festaHintSpot = -1;
            if (_carrying >= 0 && Time.unscaledTime >= _nextRiddleRepeat)
            {
                SayRiddle(localization, festaDay, _carrying);
            }
        }

        private void ClosedFesta(int day, float hour, float distance)
        {
            if (_festaOpenDay != 0)
            {
                _festaOpenDay = 0;
                _carrying = -1;
                Say(CityEventsTexts.FestaOver);
            }

            HideFesta();
            if (MingchengLanternFestaRules.IsWarning(day, hour))
            {
                if (_festaWarnedDay != day)
                {
                    _festaWarnedDay = day;
                    Say(CityEventsTexts.FestaWarning);
                }

                return;
            }

            if (distance > FestaPlaza || Time.unscaledTime < _nextFestaSoon)
            {
                return;
            }

            _nextFestaSoon = Time.unscaledTime + FestaSoonEvery;
            int wait = MingchengLanternFestaRules.DaysUntilFesta(day);
            if (wait == 0)
            {
                Say(CityEventsTexts.FestaTonight);
            }
            else
            {
                SayFormat(CityEventsTexts.FestaSoon, wait);
            }
        }

        private void TickRiddleLantern(LocalizationManager localization, int festaDay, long solved, int lantern)
        {
            bool fresh = _festaHintSpot != lantern || Time.unscaledTime >= _nextFestaHint;
            if (MingchengLanternFestaRules.IsSolved(solved, lantern))
            {
                FestaHint(fresh, lantern, CityEventsTexts.FestaSolvedLantern);
                return;
            }

            if (Time.unscaledTime < _darkUntil[lantern])
            {
                FestaHint(fresh, lantern, CityEventsTexts.FestaDark);
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_carrying != lantern)
            {
                FestaHint(fresh, lantern, CityEventsTexts.FestaRead);
            }

            if (!InteractPressed())
            {
                return;
            }

            _carrying = lantern;
            _nextFestaHint = Time.unscaledTime + HintCooldown;
            SayRiddle(localization, festaDay, lantern);
        }

        private void TickAnswer(LocalizationManager localization, int festaDay, long solved, LanternAnswer answer)
        {
            int spot = 100 + (int)answer;
            if (_festaHintSpot != spot || Time.unscaledTime >= _nextFestaHint)
            {
                _festaHintSpot = spot;
                _nextFestaHint = Time.unscaledTime + HintCooldown;
                SayText(CityEventsTexts.Format(localization, CityEventsTexts.FestaAnswerPrompt, CityEventsTexts.Get(localization, CityEventsTexts.AnswerKey(answer))));
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!InteractPressed())
            {
                return;
            }

            int lantern = _carrying;
            int riddle = MingchengLanternFestaRules.RiddleAt(festaDay, lantern);
            _nextFestaHint = Time.unscaledTime + HintCooldown;
            if (!MingchengLanternFestaRules.IsRight(riddle, answer))
            {
                _carrying = -1;
                _darkUntil[lantern] = Time.unscaledTime + MingchengLanternFestaRules.WrongDarkSeconds;
                Say(CityEventsTexts.FestaWrong);
                return;
            }

            if (!Grant(MingchengLanternFestaRules.GoldPerRiddle, 0))
            {
                return; // the bag is full: the riddle stays in hand, answer again when there is room
            }

            _carrying = -1;
            long now = MingchengLanternFestaRules.Solve(solved, lantern);
            _festaLocalNight = festaDay;
            _festaLocalSolved = now;
            SetMark(MingchengLanternFestaRules.NightKey, festaDay);
            SetMark(MingchengLanternFestaRules.SolvedKey, now);
            SayFormat(CityEventsTexts.FestaRight, MingchengLanternFestaRules.GoldPerRiddle, MingchengLanternFestaRules.SolvedCount(now));
            if (MingchengLanternFestaRules.BonusDue(now, GetMark(MingchengLanternFestaRules.BonusKey, _festaLocalBonus), festaDay))
            {
                PayFestaBonus(festaDay);
            }
        }

        private void PayFestaBonus(int festaDay)
        {
            if (!Grant(0, MingchengLanternFestaRules.AllSolvedStones))
            {
                _nextBonusTry = Time.unscaledTime + 5f;
                return;
            }

            _festaLocalBonus = festaDay;
            SetMark(MingchengLanternFestaRules.BonusKey, festaDay);
            SayFormat(CityEventsTexts.FestaAll, MingchengLanternFestaRules.AllSolvedStones);
            CousinLine("festa");
        }

        private void SayRiddle(LocalizationManager localization, int festaDay, int lantern)
        {
            _nextRiddleRepeat = Time.unscaledTime + RiddleRepeatSeconds;
            string riddle = CityEventsTexts.Get(localization, CityEventsTexts.RiddleKey(MingchengLanternFestaRules.RiddleAt(festaDay, lantern)));
            SayText(CityEventsTexts.Format(localization, CityEventsTexts.FestaTake, riddle));
        }

        private void FestaHint(bool fresh, int spot, string key)
        {
            if (!fresh)
            {
                return;
            }

            _festaHintSpot = spot;
            _nextFestaHint = Time.unscaledTime + HintCooldown;
            Say(key);
        }

        private void ShowFesta(long solved)
        {
            Vector2[] riddles = OverworldLayouts.MingchengRiddleLanterns;
            Vector2[] answers = OverworldLayouts.MingchengAnswerLanterns;
            _riddleBeams ??= new LineRenderer[riddles.Length];
            _answerBeams ??= new LineRenderer[answers.Length];
            for (int i = 0; i < riddles.Length; i++)
            {
                Color color = MingchengLanternFestaRules.IsSolved(solved, i) ? RiddleSolvedColor
                    : i < _darkUntil.Length && Time.unscaledTime < _darkUntil[i] ? RiddleDarkColor
                    : _carrying == i ? RiddleTakenColor : RiddleColor;
                Beam(ref _riddleBeams[i], "Mc_FestaRiddle", 0.45f, _mingchengOrigin + riddles[i], RiddleBeamHeight, color);
            }

            for (int i = 0; i < answers.Length; i++)
            {
                Beam(ref _answerBeams[i], "Mc_FestaAnswer", 0.35f, _mingchengOrigin + answers[i], AnswerBeamHeight, AnswerColors[i % AnswerColors.Length]);
            }
        }

        private void HideFesta()
        {
            Hide(_riddleBeams);
            Hide(_answerBeams);
        }
    }
}
