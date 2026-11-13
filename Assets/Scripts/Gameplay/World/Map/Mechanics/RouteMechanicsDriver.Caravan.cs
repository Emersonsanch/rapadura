using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Ammo's caravan through the sandstorm on the Estrada das Caravanas (07, <see cref="CaravanRoadEscortRules"/>): three camels (tan pillars) at
    /// their camp below the Pomar gate; [E] by them when the sand is coming sets them out down the caravan road to Qocho. In the storm they walk only
    /// with the hero at their side, and walking there is a shelter (<see cref="RouteMechanicsDriver.HeroBesideCaravan"/>, read by the sandstorm's
    /// own driver); when the sand starts the beasts round the caravan are forced onto the hero. Never takes the controls.
    /// </summary>
    public sealed partial class RouteMechanicsDriver
    {
        private const int Camels = 3;
        private const float CamelGap = 3.5f;
        private const float CamelHeight = 2.4f;
        private const float KneelHeight = 1.3f;
        private const float CamelViewRange = 160f;
        private const int CamelKey = 100;
        private const int CaravanSpot = 100;

        /// <summary>Where the caravan part runs (region coordinates of the Areias): round the caravan road, from the Pomar gate to Qocho.</summary>
        public static readonly Rect CaravanBounds = Rect.MinMaxRect(-420f, -80f, 40f, 520f);

        private static readonly Color CamelColor = new Color(0.82f, 0.64f, 0.4f, 1f);
        private static readonly Color CamelStormColor = new Color(1f, 0.85f, 0.45f, 1f);

        private readonly Vector2[] _camelAt = new Vector2[Camels];
        private bool _caravanOn;
        private float _caravanDistance;
        private float _caravanLength;
        private CaravanRoadState _caravanState;
        private AreiasStormPhase _caravanPhase = AreiasStormPhase.Calm;
        private bool _caravanShelterSaid;

        /// <summary>True while the hero stands at a caravan station (the camp's [E]).</summary>
        private bool TickCaravan(Vector2 local, double now)
        {
            if (!_caravanOn)
            {
                HeroBesideCaravan = false;
                return TickCaravanCamp(local, now);
            }

            Vector2[] road = OverworldLayouts.AreiasCaravanRoad;
            Vector2 head = RidePath.PointAt(road, _caravanDistance);
            float heroDistance = Vector2.Distance(local, head);
            if (!_stats.IsAlive || CaravanRoadEscortRules.Lost(heroDistance) || !OverworldLayouts.AreiasBounds.Contains(local))
            {
                AbandonCaravan(true);
                return false;
            }

            AreiasStormPhase phase = AreiasStormRules.PhaseAt(now);
            if (phase == AreiasStormPhase.Warning && _caravanPhase == AreiasStormPhase.Calm)
            {
                Say(RouteMechanicsTexts.CaravanWarning);
            }

            if (CaravanRoadEscortRules.SandStarted(_caravanPhase, phase))
            {
                CaravanAmbush(head);
            }

            _caravanPhase = phase;
            CaravanRoadState state = CaravanRoadEscortRules.StateFor(phase, heroDistance);
            if (state == CaravanRoadState.Kneeling)
            {
                SayOnce(RouteMechanicsTexts.CaravanKneels);
            }
            else if (state == CaravanRoadState.Waiting)
            {
                SayOnce(RouteMechanicsTexts.CaravanWaits);
            }

            _caravanState = state;
            if (state == CaravanRoadState.Walking)
            {
                _caravanDistance = Mathf.Min(_caravanLength, _caravanDistance + CaravanRoadEscortRules.Speed * Time.deltaTime);
                if (_caravanDistance >= _caravanLength)
                {
                    FinishCaravan(now);
                    return false;
                }
            }

            PlaceCamels(road, _caravanDistance);
            float nearest = float.MaxValue;
            foreach (Vector2 camel in _camelAt)
            {
                nearest = Mathf.Min(nearest, Vector2.Distance(local, camel));
            }

            HeroBesideCaravan = CaravanRoadEscortRules.Shelters(phase, nearest);
            if (HeroBesideCaravan && !_caravanShelterSaid)
            {
                _caravanShelterSaid = true;
                Say(RouteMechanicsTexts.CaravanShelter);
            }

            ShowCamels(state == CaravanRoadState.Kneeling, phase == AreiasStormPhase.Storm);
            return false;
        }

        /// <summary>The camp: the camels rest at the road's head; [E] by them sets the caravan out when the sand is coming.</summary>
        private bool TickCaravanCamp(Vector2 local, double now)
        {
            Vector2 start = OverworldLayouts.AreiasCaravanRoad[0];
            float distance = Vector2.Distance(local, start);
            if (distance > CamelViewRange)
            {
                return false;
            }

            PlaceCamels(OverworldLayouts.AreiasCaravanRoad, 0f);
            ShowCamels(false, false);
            if (distance > CaravanRoadEscortRules.StartReach || !CanInteract)
            {
                return false;
            }

            if (!CaravanRoadEscortRules.CanDepart(now))
            {
                int minutes = Mathf.Max(1, Mathf.CeilToInt(CaravanRoadEscortRules.SecondsToDeparture(now) / 60f));
                Hint(CaravanSpot, Format(RouteMechanicsTexts.CaravanWaitsSand, minutes));
                return true;
            }

            bool paid = ServiceLocator.TryGet(out WorldMapProgress progress)
                && CaravanRoadEscortRules.Paid(progress.GetDailyMark(CaravanRoadEscortRules.RewardKey), UtcDay(now));
            HintKey(CaravanSpot, paid ? RouteMechanicsTexts.CaravanPromptPaid : RouteMechanicsTexts.CaravanPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed)
            {
                _caravanOn = true;
                _caravanDistance = 0f;
                _caravanLength = RidePath.Length(OverworldLayouts.AreiasCaravanRoad);
                _caravanState = CaravanRoadState.Walking;
                _caravanPhase = AreiasStormRules.PhaseAt(now);
                _caravanShelterSaid = false;
                Say(RouteMechanicsTexts.CaravanStart);
                if (_caravanPhase == AreiasStormPhase.Storm)
                {
                    CaravanAmbush(start);
                }
            }

            return true;
        }

        /// <summary>The sand starts: the beasts round the caravan's head (bosses never) come at it.</summary>
        private void CaravanAmbush(Vector2 head)
        {
            bool any = false;
            float range = CaravanRoadEscortRules.AmbushRange * CaravanRoadEscortRules.AmbushRange;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _areiasOrigin;
                if ((at - head).sqrMagnitude <= range)
                {
                    enemy.ForceAggro();
                    any = true;
                }
            }

            if (any)
            {
                Say(RouteMechanicsTexts.CaravanAmbush);
            }
        }

        private void FinishCaravan(double now)
        {
            _caravanOn = false;
            HeroBesideCaravan = false;
            long today = UtcDay(now);
            ServiceLocator.TryGet(out WorldMapProgress progress);
            bool paid = progress != null && CaravanRoadEscortRules.Paid(progress.GetDailyMark(CaravanRoadEscortRules.RewardKey), today);
            if (paid)
            {
                Say(RouteMechanicsTexts.CaravanArrivedPaid);
                return;
            }

            if (PayGold(CaravanRoadEscortRules.Purse))
            {
                progress?.SetDailyMark(CaravanRoadEscortRules.RewardKey, (int)today);
                SayFormat(RouteMechanicsTexts.CaravanArrived, CaravanRoadEscortRules.Purse);
            }
        }

        /// <summary>Ends a running escort without pay (fell, left the caravan or the region, the driver stopped).</summary>
        private void AbandonCaravan(bool say)
        {
            if (_caravanOn && say && _player != null && isActiveAndEnabled)
            {
                Say(RouteMechanicsTexts.CaravanLost);
            }

            _caravanOn = false;
            HeroBesideCaravan = false;
        }

        /// <summary>The camels in file behind the head, <see cref="CamelGap"/> apart (behind the road's start they line up back along its first leg).</summary>
        private void PlaceCamels(Vector2[] road, float headDistance)
        {
            Vector2 dir = RidePath.DirectionAt(road, 0f);
            for (int i = 0; i < Camels; i++)
            {
                float d = headDistance - i * CamelGap;
                _camelAt[i] = d >= 0f ? RidePath.PointAt(road, d) : road[0] + dir * d;
            }
        }

        private void ShowCamels(bool kneeling, bool storm)
        {
            for (int i = 0; i < Camels; i++)
            {
                Pillar(CamelKey + i, _areiasOrigin + _camelAt[i], kneeling ? KneelHeight : CamelHeight, 1.1f, storm ? CamelStormColor : CamelColor);
            }
        }
    }
}
