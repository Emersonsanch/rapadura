using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Apep spits the river (76, <see cref="ApepSpitRiverRules"/>) on the Rio driver: when she falls (<see cref="EnemyDiedEvent"/>) the Pedras da
    /// Mistura go into the hero's bag; when the bag is full they stay due (save: the time mark <see cref="ApepSpitRiverRules.OwedKey"/>, written
    /// only then and cleared when paid) and are paid as soon as there is room.
    /// </summary>
    public sealed partial class RioMechanicsDriver
    {
        private const float ApepOwedRetrySeconds = 5f;

        private int _apepOwed = -1;
        private float _nextApepOwedTry;

        private void HookApepSpit() => EventBus.Subscribe<EnemyDiedEvent>(OnApepDied);

        private void UnhookApepSpit() => EventBus.Unsubscribe<EnemyDiedEvent>(OnApepDied);

        private int ApepOwed()
        {
            if (_apepOwed < 0)
            {
                _apepOwed = ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null
                    ? (int)Mathf.Clamp(progress.GetTimeMark(ApepSpitRiverRules.OwedKey, 0L), 0L, 99L)
                    : 0;
            }

            return _apepOwed;
        }

        private void SetApepOwed(int owed)
        {
            _apepOwed = Mathf.Max(0, owed);
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                progress.SetTimeMark(ApepSpitRiverRules.OwedKey, _apepOwed);
            }
        }

        private void OnApepDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null)
            {
                return;
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            if (!ApepSpitRiverRules.IsApep(enemy != null && enemy.Definition != null ? enemy.Definition.name : null))
            {
                return;
            }

            float hour = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.CurrentHour : 12f;
            int stones = ApepSpitRiverRules.StonesFor(hour);
            int owed = ApepOwed();
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_player != null)
            {
                SayText(ApepSpitRiverTexts.Format(localization, stones > ApepSpitRiverRules.Stones ? ApepSpitRiverTexts.SpatMidnight : ApepSpitRiverTexts.Spat, stones));
                string line = ApepSpitRiverTexts.LineFor(ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null);
                if (line != null)
                {
                    SayText(ApepSpitRiverTexts.Get(localization, line));
                }
            }

            if (owed == 0 && MechanicsBagGrant.Grant(_bag, (ApepSpitRiverRules.StoneItemId, stones)))
            {
                return;
            }

            SetApepOwed(ApepSpitRiverRules.Owe(owed, hour));
            _nextApepOwedTry = Time.unscaledTime + ApepOwedRetrySeconds;
            if (_player != null)
            {
                SayText(ApepSpitRiverTexts.Format(localization, ApepSpitRiverTexts.BagFull, _apepOwed));
            }
        }

        /// <summary>Pays the stones still due once the bag has room (called every frame from <c>Update</c>, after the hero is found).</summary>
        private void TickApepOwed()
        {
            if (Time.unscaledTime < _nextApepOwedTry || ApepOwed() == 0)
            {
                return;
            }

            _nextApepOwedTry = Time.unscaledTime + ApepOwedRetrySeconds;
            int owed = _apepOwed;
            if (!MechanicsBagGrant.Grant(_bag, (ApepSpitRiverRules.StoneItemId, owed)))
            {
                return;
            }

            SetApepOwed(0);
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(ApepSpitRiverTexts.Format(localization, ApepSpitRiverTexts.Paid, owed));
        }
    }
}
