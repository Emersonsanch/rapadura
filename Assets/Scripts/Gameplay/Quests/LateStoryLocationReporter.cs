using Rapadura.Core.Events;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Turns the area the hero walks into (<see cref="AreaChangedEvent"/> of the overworld) into a ReachLocation report on the <see cref="QuestManager"/>, with the id of
    /// the zone: the place objectives of <see cref="LateStoryCatalog"/>. The last step is the exception: arriving at the Varanda is not enough, it reports when the hero sits by the chair
    /// (<see cref="RockingChairSatEvent"/>). Pure data in, no scene references; one subscription at a time, whoever installs last.
    /// </summary>
    public static class LateStoryLocationReporter
    {
        private static QuestManager _quests;
        private static bool _subscribed;

        /// <summary>Reports to <paramref name="quests"/> from now on (null stops reporting).</summary>
        public static void Install(QuestManager quests)
        {
            _quests = quests;
            if (_subscribed)
            {
                EventBus.Unsubscribe<AreaChangedEvent>(Report);
                EventBus.Unsubscribe<RockingChairSatEvent>(ReportSeat);
                _subscribed = false;
            }

            if (quests != null)
            {
                EventBus.Subscribe<AreaChangedEvent>(Report);
                EventBus.Subscribe<RockingChairSatEvent>(ReportSeat);
                _subscribed = true;
            }
        }

        private static void Report(AreaChangedEvent change)
        {
            if (_quests != null && change.Current != null && (change.ZoneChanged || change.Teleported) && change.Current.Id != LateStoryCatalog.Last.ZoneId)
            {
                _quests.ReportLocationReached(change.Current.Id);
            }
        }

        private static void ReportSeat(RockingChairSatEvent sat) => _quests?.ReportLocationReached(LateStoryCatalog.Last.ZoneId);
    }
}
