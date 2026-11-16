using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The raked gravel and the hidden tea house of the Jardins de Lava Laqueada (66, <see cref="HinokuraGravelRules"/>), and the volcano goat's
    /// hooves over Echeyde's eruption (67, <see cref="HinokuraEruptionRules.HoovesProof"/>), on the same driver as the forges. Each garden is a
    /// square outline with its pattern drawn in light on the gravel (green when it is the right one); [E] within 4.5 m rakes the next pattern.
    /// With the three right the gardens are calm for the UTC day (a daily mark in the save) and the tea house's beacon rises on the moss terrace.
    /// </summary>
    public sealed partial class HinokuraMechanicsDriver
    {
        private const float GardenViewRange = 120f;
        private const float GardenRuleRange = 60f;
        private const float GardenHalf = 2.6f;
        private const float GardenLift = 0.15f;
        private const float TeaBeaconHeight = 14f;
        private const int PatternPoints = 41;

        private static readonly Color GravelWrong = new Color(0.92f, 0.86f, 0.7f, 0.9f);
        private static readonly Color GravelRight = new Color(0.55f, 0.95f, 0.6f, 1f);
        private static readonly Color TeaColor = new Color(0.75f, 0.95f, 0.7f, 1f);

        private GravelPattern[] _gravel;
        private long _gravelDay = long.MinValue;
        private bool _gravelCalm;
        private LineRenderer[] _gardenFrames;
        private LineRenderer[] _gardenPatterns;
        private LineRenderer _teaBeacon;
        private long _gravelRuleDay = long.MinValue;
        private float _nextGravelHint;
        private int _hintGarden = -1;
        private long _hoovesSaidCycle = long.MinValue;
        private InventoryManager _puzzleBag;

        private static long UtcDay() => NartesRaceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        private static CharacterId? PuzzleHero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private bool PuzzleInteract() => _controller.Input != null && _controller.Input.InteractPressedThisFrame;

        private InventoryManager PuzzleBag() => _puzzleBag != null ? _puzzleBag : _puzzleBag = _player.GetComponentInChildren<InventoryManager>();

        // ------------------------------------------------------------------
        // 67 · the goat's hooves
        // ------------------------------------------------------------------

        /// <summary>True (and said once per eruption) while the hero rides the volcano goat: the eruption's lava does not burn.</summary>
        private bool GoatHoovesHold()
        {
            MountController rider = MountController.Active;
            bool proof = rider != null && HinokuraEruptionRules.HoovesProof(rider.IsMounted, rider.Definition != null ? rider.Definition.MountId : null);
            long cycle = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / HinokuraEruptionRules.CycleSeconds;
            if (proof && _hoovesSaidCycle != cycle)
            {
                _hoovesSaidCycle = cycle;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.GoatHooves));
            }

            return proof;
        }

        // ------------------------------------------------------------------
        // 66 · the gravel gardens
        // ------------------------------------------------------------------

        private void TickGravel(Vector2 local)
        {
            Vector2[] gardens = OverworldLayouts.GravelGardens;
            bool near = (local - OverworldLayouts.CasaDeChaOffset).sqrMagnitude < 400f * 400f;
            if (!near)
            {
                HideGravel();
                return;
            }

            ServiceLocator.TryGet(out WorldMapProgress progress);
            LoadGravel(progress);
            ShowGardens(local);
            ShowTeaHouse(local);
            if (!_stats.IsAlive)
            {
                return;
            }

            long day = UtcDay();
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_gravelRuleDay != day && NearAny(gardens, local, GardenRuleRange))
            {
                _gravelRuleDay = day;
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.GravelRule));
            }

            int garden = HinokuraGravelRules.GardenAt(gardens, local);
            if (garden >= 0)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (_hintGarden != garden || Time.unscaledTime >= _nextGravelHint)
                {
                    _hintGarden = garden;
                    _nextGravelHint = Time.unscaledTime + HintCooldown * 2f;
                    SayText(_gravelCalm
                        ? HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.GravelAlreadyCalm)
                        : HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.GravelPrompt,
                            HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.NeighbourNames[(int)HinokuraGravelRules.Neighbours[garden]]),
                            HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.PatternNames[(int)_gravel[garden]])));
                }

                if (PuzzleInteract())
                {
                    Rake(garden, progress, day, localization);
                }

                return;
            }

            _hintGarden = -1;
            if (_gravelCalm && (OverworldLayouts.HiddenTeaHouse - local).sqrMagnitude <= HinokuraGravelRules.TeaReach * HinokuraGravelRules.TeaReach)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (Time.unscaledTime >= _nextGravelHint)
                {
                    _nextGravelHint = Time.unscaledTime + HintCooldown * 2f;
                    SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.TeaPrompt));
                }

                if (PuzzleInteract())
                {
                    TakeTea(progress, day, localization);
                }
            }
        }

        private static bool NearAny(Vector2[] points, Vector2 p, float range)
        {
            foreach (Vector2 point in points)
            {
                if ((point - p).sqrMagnitude < range * range)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The day's gravel: the right patterns when the gardens were calmed today (save), else the morning's mess.</summary>
        private void LoadGravel(WorldMapProgress progress)
        {
            long day = UtcDay();
            if (_gravelDay == day && _gravel != null)
            {
                return;
            }

            _gravelDay = day;
            _gravelCalm = progress != null && progress.GetDailyMark(HinokuraGravelRules.CalmDayKey) == (int)day;
            _gravel = _gravelCalm ? HinokuraGravelRules.Calm() : HinokuraGravelRules.StartFor(day);
            for (int i = 0; _gardenPatterns != null && i < _gardenPatterns.Length; i++)
            {
                DrawGarden(i);
            }
        }

        private void Rake(int garden, WorldMapProgress progress, long day, LocalizationManager localization)
        {
            if (_gravelCalm)
            {
                return;
            }

            _gravel[garden] = HinokuraGravelRules.Next(_gravel[garden]);
            DrawGarden(garden);
            _nextGravelHint = Time.unscaledTime + HintCooldown * 2f;
            if (!HinokuraGravelRules.AllRight(_gravel))
            {
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.GravelRaked,
                    HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.PatternNames[(int)_gravel[garden]]), HinokuraGravelRules.RightCount(_gravel)));
                return;
            }

            _gravelCalm = true;
            progress?.SetDailyMark(HinokuraGravelRules.CalmDayKey, (int)day);
            SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.GravelCalm));
            string line = HinokuraPuzzleTexts.CalmLineFor(PuzzleHero());
            if (line != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), HinokuraPuzzleTexts.Get(localization, line));
            }
        }

        private void TakeTea(WorldMapProgress progress, long day, LocalizationManager localization)
        {
            bool served = progress != null && progress.GetDailyMark(HinokuraGravelRules.TeaDayKey) == (int)day;
            bool found = progress != null && progress.GetTimeMark(HinokuraGravelRules.FoundKey) > 0L;
            (int gold, int stones) = HinokuraGravelRules.TeaReward(_gravelCalm, served, found);
            if (gold <= 0 && stones <= 0)
            {
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.TeaServed));
                return;
            }

            if (!MechanicsBagGrant.Grant(PuzzleBag(), (MechanicsBagGrant.GoldItemId, gold), (EventRoomRules.BlessingStoneItemId, stones)))
            {
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.BagFull));
                return;
            }

            progress?.SetDailyMark(HinokuraGravelRules.TeaDayKey, (int)day);
            if (!found)
            {
                progress?.SetTimeMark(HinokuraGravelRules.FoundKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.TeaFirst, gold, stones));
            }
            else
            {
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.TeaDaily, gold));
            }
        }

        // ------------------------------------------------------------------
        // Drawn on the gravel
        // ------------------------------------------------------------------

        private void ShowGardens(Vector2 local)
        {
            Vector2[] gardens = OverworldLayouts.GravelGardens;
            if (_gardenFrames == null)
            {
                _gardenFrames = new LineRenderer[gardens.Length];
                _gardenPatterns = new LineRenderer[gardens.Length];
                for (int i = 0; i < gardens.Length; i++)
                {
                    Vector3 c = Ground(gardens[i]) + Vector3.up * GardenLift;
                    LineRenderer frame = NewLine("Hinokura_GravelFrame", 0.12f);
                    frame.positionCount = 5;
                    frame.SetPosition(0, c + new Vector3(-GardenHalf, 0f, -GardenHalf));
                    frame.SetPosition(1, c + new Vector3(GardenHalf, 0f, -GardenHalf));
                    frame.SetPosition(2, c + new Vector3(GardenHalf, 0f, GardenHalf));
                    frame.SetPosition(3, c + new Vector3(-GardenHalf, 0f, GardenHalf));
                    frame.SetPosition(4, c + new Vector3(-GardenHalf, 0f, -GardenHalf));
                    _gardenFrames[i] = frame;
                    LineRenderer pattern = NewLine("Hinokura_GravelPattern", 0.1f);
                    pattern.positionCount = PatternPoints;
                    _gardenPatterns[i] = pattern;
                    DrawGarden(i);
                }
            }

            for (int i = 0; i < gardens.Length; i++)
            {
                bool show = (gardens[i] - local).sqrMagnitude < GardenViewRange * GardenViewRange;
                _gardenFrames[i].enabled = show;
                _gardenPatterns[i].enabled = show;
            }
        }

        private void DrawGarden(int garden)
        {
            if (_gardenPatterns == null || _gravel == null)
            {
                return;
            }

            Vector3 c = Ground(OverworldLayouts.GravelGardens[garden]) + Vector3.up * GardenLift;
            LineRenderer line = _gardenPatterns[garden];
            for (int i = 0; i < PatternPoints; i++)
            {
                Vector2 o = PatternPoint(_gravel[garden], i / (float)(PatternPoints - 1)) * (GardenHalf * 0.85f);
                line.SetPosition(i, c + new Vector3(o.x, 0f, o.y));
            }

            Color color = HinokuraGravelRules.IsRight(_gravel, garden) ? GravelRight : GravelWrong;
            line.startColor = line.endColor = color;
            _gardenFrames[garden].startColor = _gardenFrames[garden].endColor = color;
        }

        /// <summary>One point (t from 0 to 1) of a pattern's single stroke, in a unit square round the garden's centre.</summary>
        private static Vector2 PatternPoint(GravelPattern pattern, float t)
        {
            switch (pattern)
            {
                case GravelPattern.Waves:
                {
                    // Three wavy rows, joined at the ends.
                    float row = Mathf.Min(2f, Mathf.Floor(t * 3f));
                    float u = t * 3f - row;
                    float x = (row % 2f == 0f ? u : 1f - u) * 2f - 1f;
                    return new Vector2(x, (row - 1f) * 0.6f + Mathf.Sin(x * Mathf.PI * 2f) * 0.15f);
                }
                case GravelPattern.Rings:
                {
                    // Two rings round the stone: inner then outer.
                    float a = t * Mathf.PI * 4f;
                    float r = t < 0.5f ? 0.45f : 0.9f;
                    return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
                }
                case GravelPattern.Lines:
                {
                    // Four straight furrows, back and forth.
                    float row = Mathf.Min(3f, Mathf.Floor(t * 4f));
                    float u = t * 4f - row;
                    float x = (row % 2f == 0f ? u : 1f - u) * 2f - 1f;
                    return new Vector2(x, row * 0.6f - 0.9f);
                }
                default:
                {
                    float a = t * Mathf.PI * 6f;
                    return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * t;
                }
            }
        }

        private void ShowTeaHouse(Vector2 local)
        {
            bool show = _gravelCalm && (OverworldLayouts.HiddenTeaHouse - local).sqrMagnitude < GardenViewRange * GardenViewRange;
            if (_teaBeacon == null)
            {
                if (!show)
                {
                    return;
                }

                _teaBeacon = NewLine("Hinokura_HiddenTeaHouse", 0.6f);
                Vector3 foot = Ground(OverworldLayouts.HiddenTeaHouse);
                _teaBeacon.SetPosition(0, foot);
                _teaBeacon.SetPosition(1, foot + Vector3.up * TeaBeaconHeight);
                _teaBeacon.startColor = _teaBeacon.endColor = TeaColor;
            }

            _teaBeacon.enabled = show;
        }

        private void HideGravel()
        {
            for (int i = 0; _gardenFrames != null && i < _gardenFrames.Length; i++)
            {
                _gardenFrames[i].enabled = false;
                _gardenPatterns[i].enabled = false;
            }

            if (_teaBeacon != null)
            {
                _teaBeacon.enabled = false;
            }

            _hintGarden = -1;
        }
    }
}
