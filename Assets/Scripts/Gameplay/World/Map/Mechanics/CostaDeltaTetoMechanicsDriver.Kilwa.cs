using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The flooded houses of Kilwa (29, <see cref="FloodedHouseRules"/>): each door is a column of sea light with its four carved stones round it;
    /// coming to a door the hero reads the lintel, and [E] at the stones in its order opens the house's chest (a daily mark in the save). The
    /// Bolha de Ar Engarrafada must be in the bag. The pressed stones belong to this visit. No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float HouseViewRange = 120f;

        private static readonly Color DoorColor = new Color(0.35f, 0.75f, 0.85f, 0.9f);
        private static readonly Color StoneIdleColor = new Color(0.8f, 0.85f, 0.75f, 1f);
        private static readonly Color StonePressedColor = new Color(1f, 0.85f, 0.4f, 1f);

        private LineRenderer[] _doorLines;
        private LineRenderer[] _stoneLines;
        private int[] _housePressed;
        private int _houseHere = -1;
        private bool _hasBubble;
        private bool _inKilwa;

        private void TickHouses(Vector2 local)
        {
            if (!OverworldLayouts.KilwaHousesBounds.Contains(local))
            {
                LeaveHouses();
                return;
            }

            _inKilwa = true;
            Vector2[] doors = OverworldLayouts.KilwaHouseDoors;
            _housePressed ??= new int[doors.Length];
            int day = GameDay();
            ShowHouses(local, doors);

            int house = RootBridgeRules.Nearest(doors, local, FloodedHouseRules.DoorReach);
            if (house != _houseHere)
            {
                _houseHere = house;
                _hasBubble = house >= 0 && BubbleInBag();
                if (house >= 0)
                {
                    if (!_hasBubble)
                    {
                        SayR4(CoastDeltaPassTexts.HouseNoBubble);
                    }
                    else
                    {
                        SayR4(CoastDeltaPassTexts.HouseLintel, StoneName(FloodedHouseRules.CodeStone(house, day, 0)),
                            StoneName(FloodedHouseRules.CodeStone(house, day, 1)), StoneName(FloodedHouseRules.CodeStone(house, day, 2)));
                    }
                }
            }

            if (house < 0 || !_hasBubble || !_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            int stone = FloodedHouseRules.StoneNear(doors[house], local);
            if (stone < 0)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayR4(CoastDeltaPassTexts.HouseStone, StoneName(stone));
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            int pressed = FloodedHouseRules.Press(_housePressed[house], stone, house, day);
            _housePressed[house] = pressed;
            if (pressed == 0)
            {
                SayR4(CoastDeltaPassTexts.HouseWrong);
                return;
            }

            if (!FloodedHouseRules.Opened(pressed))
            {
                SayR4(CoastDeltaPassTexts.HouseRight, pressed);
                return;
            }

            _housePressed[house] = 0;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            if (progress != null && progress.GetDailyMark(FloodedHouseRules.HouseKey(house)) >= day)
            {
                SayR4(CoastDeltaPassTexts.HouseTaken);
                return;
            }

            SayR4(CoastDeltaPassTexts.HouseOpened);
            if (GiveR4(CostaTideRules.ChestItem(house + 20, day)))
            {
                progress?.SetDailyMark(FloodedHouseRules.HouseKey(house), day);
            }
        }

        private void ShowHouses(Vector2 local, Vector2[] doors)
        {
            _doorLines ??= new LineRenderer[doors.Length];
            _stoneLines ??= new LineRenderer[doors.Length * FloodedHouseRules.StoneCount];
            for (int i = 0; i < doors.Length; i++)
            {
                bool near = (doors[i] - local).sqrMagnitude <= HouseViewRange * HouseViewRange;
                if (!near)
                {
                    Off(_doorLines[i]);
                    for (int s = 0; s < FloodedHouseRules.StoneCount; s++)
                    {
                        Off(_stoneLines[i * FloodedHouseRules.StoneCount + s]);
                    }

                    continue;
                }

                _doorLines[i] = Pillar(_doorLines[i], "Cs_FloodedDoor", _costaOrigin + doors[i], 0.8f, 5f, DoorColor);
                for (int s = 0; s < FloodedHouseRules.StoneCount; s++)
                {
                    int k = i * FloodedHouseRules.StoneCount + s;
                    bool pressedStone = _housePressed != null && IsPressed(i, s);
                    _stoneLines[k] = Pillar(_stoneLines[k], "Cs_CarvedStone", _costaOrigin + FloodedHouseRules.StoneAt(doors[i], s), 0.45f, 1.2f,
                        pressedStone ? StonePressedColor : StoneIdleColor);
                }
            }
        }

        /// <summary>Whether stone <paramref name="stone"/> of house <paramref name="house"/> is among the ones pressed right so far.</summary>
        private bool IsPressed(int house, int stone)
        {
            int day = GameDay();
            for (int step = 0; step < _housePressed[house]; step++)
            {
                if (FloodedHouseRules.CodeStone(house, day, step) == stone)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether the Air Bubble is in the bag, read when the hero comes to a door (not every frame).</summary>
        private bool BubbleInBag() => MapRequirementHeroState.Capture(_bag).Has(MapRequirementItems.AirBubble);

        private void LeaveHouses()
        {
            if (!_inKilwa)
            {
                return;
            }

            _inKilwa = false;
            _houseHere = -1;
            if (_housePressed != null)
            {
                System.Array.Clear(_housePressed, 0, _housePressed.Length);
            }

            Off(_doorLines);
            Off(_stoneLines);
        }
    }
}
