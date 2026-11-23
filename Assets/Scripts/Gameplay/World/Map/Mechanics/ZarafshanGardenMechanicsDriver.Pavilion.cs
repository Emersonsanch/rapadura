using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The pavilion's door, the altar's Pétala de Luz and the Garden's night (06, <see cref="ZarafshanPavilionRules"/>) on the garden's driver:
    /// with the four sparks free the door on the west of the columns glows; [E] opens it for good (a time mark, written only then) and the eight
    /// columns light up; from then the pavilion mends and the altar at its centre gives one petal per game day (a daily mark, written only then).
    /// At night, inside the walls, the beasts past 9 m are lost to sight (<see cref="BeastSightHider"/>) unless the hero walks by the lit water,
    /// stands in the open pavilion or eats a Pétala de Luz (its light, <see cref="LightPetalBoard"/>, also clears the Garajonay fog). No damage,
    /// never takes the controls.
    /// </summary>
    public sealed partial class ZarafshanGardenMechanicsDriver
    {
        private const float DoorHeight = 4.5f;
        private const float ColumnHeight = 5f;
        private const float ColumnRadius = 8f;
        private const float NightSayCooldown = 90f;

        private static readonly Color DoorGlowColor = new Color(1f, 0.85f, 0.45f, 1f);
        private static readonly Color ColumnLitColor = new Color(1f, 0.95f, 0.75f, 0.85f);
        private static readonly Color AltarColor = new Color(1f, 0.75f, 0.85f, 1f);

        private readonly BeastSightHider _nightSight = new BeastSightHider();
        private LineRenderer _door;
        private LineRenderer _altar;
        private LineRenderer[] _columns;
        private long _doorMark;
        private long _doorLocal;
        private int _altarLocal;
        private float _nextPavilionHint;
        private float _nextNightSay;
        private bool _nightCut;
        private bool _petalWasLit;

        /// <summary>The pavilion's state for the frame (the sparks' marks are read just before).</summary>
        private MihrshahPavilionState PavilionState() =>
            ZarafshanPavilionRules.StateOf(_freed, OverworldLayouts.MihrshahSparks.Length, _doorMark);

        private void HookPetal() => EventBus.Subscribe<ItemConsumedEvent>(OnPetalEaten);

        private void UnhookPetal() => EventBus.Unsubscribe<ItemConsumedEvent>(OnPetalEaten);

        private void OnPetalEaten(ItemConsumedEvent evt)
        {
            if (evt.ItemId != ZarafshanPavilionRules.PetalItemId || _player == null || evt.User == null
                || (evt.User != _player.gameObject && !evt.User.transform.IsChildOf(_player)))
            {
                return;
            }

            LightPetalBoard.Light(Time.time);
            _petalWasLit = true;
            SayPavilion(MihrshahPavilionTexts.PetalLight);
        }

        private void ReadDoor()
        {
            _doorMark = ServiceLocator.TryGet(out WorldMapProgress progress) ? Math.Max(progress.GetTimeMark(ZarafshanPavilionRules.DoorKey), _doorLocal) : _doorLocal;
        }

        private void TickPavilion(Vector2 local)
        {
            ReadDoor();
            Vector2 centre = OverworldLayouts.MihrshahGardenCentre;
            MihrshahPavilionState state = PavilionState();
            ShowPavilion(centre, state);
            if (!_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            Vector2 door = ZarafshanPavilionRules.DoorAt(centre);
            if (state == MihrshahPavilionState.Sealed && (local - door).sqrMagnitude <= ZarafshanPavilionRules.DoorReach * ZarafshanPavilionRules.DoorReach)
            {
                if (Time.unscaledTime >= _nextPavilionHint)
                {
                    _nextPavilionHint = Time.unscaledTime + HintCooldown;
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayText(MihrshahPavilionTexts.Format(localization, MihrshahPavilionTexts.DoorSealed, ZarafshanGardenRules.FreeCount(_freed)));
                }

                return;
            }

            if (ZarafshanPavilionRules.CanOpenDoor(state, centre, local, _stats.IsAlive))
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (Time.unscaledTime >= _nextPavilionHint)
                {
                    _nextPavilionHint = Time.unscaledTime + HintCooldown;
                    SayPavilion(MihrshahPavilionTexts.DoorPrompt);
                }

                if (_controller.Input.InteractPressedThisFrame)
                {
                    OpenDoor();
                }

                return;
            }

            if (state == MihrshahPavilionState.Open && ZarafshanPavilionRules.AtAltar(centre, local))
            {
                TickAltar(state);
            }
        }

        private void OpenDoor()
        {
            // The door opens once for good: with no room for its petals, wait (instead of opening and losing them).
            if (!PetalsFit(ZarafshanPavilionRules.FirstOpenPetals))
            {
                _nextPavilionHint = Time.unscaledTime + HintCooldown;
                SayPavilion(MihrshahPavilionTexts.BagFull);
                return;
            }

            long now = Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            _doorLocal = now;
            _doorMark = now;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(ZarafshanPavilionRules.DoorKey, now);
            }

            int given = GivePetals(ZarafshanPavilionRules.FirstOpenPetals);
            _nextPavilionHint = Time.unscaledTime + HintCooldown;
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(MihrshahPavilionTexts.Format(localization, MihrshahPavilionTexts.DoorOpened, given));
            if (given < ZarafshanPavilionRules.FirstOpenPetals)
            {
                SayPavilion(MihrshahPavilionTexts.BagFull, -0.5f);
            }

            string line = MihrshahPavilionTexts.LineFor(MapRequirementHeroState.Capture(_bag).Hero);
            SayPavilion(line, 0.7f);
        }

        private void TickAltar(MihrshahPavilionState state)
        {
            int today = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            int lastDay = saved ? Math.Max(progress.GetDailyMark(ZarafshanPavilionRules.AltarKey), _altarLocal) : _altarLocal;
            MihrshahAltarCheck check = ZarafshanPavilionRules.CheckAltar(state, lastDay, today, _stats.IsAlive);
            if (check == MihrshahAltarCheck.PickedToday)
            {
                if (Time.unscaledTime >= _nextPavilionHint)
                {
                    _nextPavilionHint = Time.unscaledTime + HintCooldown;
                    SayPavilion(MihrshahPavilionTexts.AltarToday);
                }

                return;
            }

            if (check != MihrshahAltarCheck.Give)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextPavilionHint)
            {
                _nextPavilionHint = Time.unscaledTime + HintCooldown;
                SayPavilion(MihrshahPavilionTexts.AltarPrompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _nextPavilionHint = Time.unscaledTime + HintCooldown;
            if (!PetalsFit(ZarafshanPavilionRules.DailyPetals) || GivePetals(ZarafshanPavilionRules.DailyPetals) <= 0)
            {
                SayPavilion(MihrshahPavilionTexts.BagFull);
                return;
            }

            _altarLocal = today;
            if (saved)
            {
                progress.SetDailyMark(ZarafshanPavilionRules.AltarKey, today);
            }

            SayPavilion(MihrshahPavilionTexts.AltarGave);
        }

        /// <summary>True when all <paramref name="count"/> petals fit in the bag (no bag or no petal item: nothing to lose, the give says so).</summary>
        private bool PetalsFit(int count)
        {
            ItemDefinition petal = ItemDatabase.GetById(ZarafshanPavilionRules.PetalItemId);
            return petal == null || _bag == null || _bag.CanFit(petal, count);
        }

        /// <summary>Puts up to <paramref name="count"/> petals in the bag; how many went in.</summary>
        private int GivePetals(int count)
        {
            ItemDefinition petal = ItemDatabase.GetById(ZarafshanPavilionRules.PetalItemId);
            if (petal == null || _bag == null)
            {
                return 0;
            }

            return count - _bag.AddItem(petal, count);
        }

        // ------------------------------------------------------------------
        // The night of the Garden
        // ------------------------------------------------------------------

        private void TickGardenNight(Vector2 local)
        {
            Vector2 centre = OverworldLayouts.MihrshahGardenCentre;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null && clock.Phase == TimeOfDayPhase.Night;
            bool petal = LightPetalBoard.Lit(Time.time);
            if (_petalWasLit && !petal)
            {
                _petalWasLit = false;
                SayPavilion(MihrshahPavilionTexts.PetalFades);
            }

            float range = ZarafshanPavilionRules.SightRange(
                night,
                ZarafshanPavilionRules.InGarden(centre, local),
                _stats.IsAlive,
                ZarafshanPavilionRules.NearLitCanal(OverworldLayouts.MihrshahSparks, _freed, local),
                ZarafshanPavilionRules.Mends(PavilionState(), centre, local),
                petal);
            _nightSight.Tick(_player.position, range, Time.time);
            bool cut = BeastSightRules.Active(range);
            if (cut && !_nightCut && Time.unscaledTime >= _nextNightSay)
            {
                _nextNightSay = Time.unscaledTime + NightSayCooldown;
                SayPavilion(MihrshahPavilionTexts.NightFalls);
            }

            _nightCut = cut;
        }

        private void LeavePavilion()
        {
            _nightSight.RevealAll();
            _nightCut = false;
            Off(_door);
            Off(_altar);
            if (_columns != null)
            {
                foreach (LineRenderer column in _columns)
                {
                    Off(column);
                }
            }
        }

        private void ShowPavilion(Vector2 centre, MihrshahPavilionState state)
        {
            Vector2 door = ZarafshanPavilionRules.DoorAt(centre);
            if (state == MihrshahPavilionState.Closed)
            {
                Vector3 foot = Ground(door);
                Post(ref _door, "Zf_PavilionDoor", 1.2f, foot, foot + Vector3.up * DoorHeight, DoorGlowColor);
            }
            else
            {
                Off(_door);
            }

            _columns ??= new LineRenderer[8];
            bool open = state == MihrshahPavilionState.Open;
            for (int k = 0; k < _columns.Length; k++)
            {
                if (!open)
                {
                    Off(_columns[k]);
                    continue;
                }

                float angle = (22.5f + k * 45f) * Mathf.Deg2Rad;
                Vector3 foot = Ground(centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ColumnRadius);
                Post(ref _columns[k], "Zf_PavilionColumn", 0.5f, foot, foot + Vector3.up * ColumnHeight, ColumnLitColor);
            }

            if (open)
            {
                Vector3 altar = Ground(centre);
                Post(ref _altar, "Zf_PavilionAltar", 0.4f, altar, altar + Vector3.up * 1.6f, AltarColor);
            }
            else
            {
                Off(_altar);
            }
        }

        private static void Off(LineRenderer line)
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }

        private void SayPavilion(string key, float drop = 0f)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), MihrshahPavilionTexts.Get(localization, key));
        }
    }
}
