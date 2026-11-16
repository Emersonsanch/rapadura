using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The baits of Old Kul's lake and the silver lure's call (64, <see cref="IceBaitRules"/>, <see cref="KulLureCall"/>), on the ice-fishing driver.
    /// The bait goes on the hook when the line goes down; three fish landed on the silver lure at the Buraco Grande while the Velho Kul is away
    /// bring him back to his ring now (<see cref="RegionBossDirector.CallBack"/>). Save: only the moment of the last call (an event mark).
    /// </summary>
    public sealed partial class MarGeladoMechanicsDriver
    {
        private const string KulCallMarkKey = "margelado.kul_lure_call";

        private readonly KulLureCall _kulCall = new KulLureCall();
        private bool _kulCallLoaded;
        private bool _saidBaitRule;

        private static long UnixNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private void LoadKulCall()
        {
            if (_kulCallLoaded)
            {
                return;
            }

            _kulCallLoaded = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                _kulCall.Restore(progress.GetTimeMark(KulCallMarkKey, KulLureCall.NeverCalled));
            }
        }

        /// <summary>Picks the bait for a cast at <paramref name="hole"/>, takes it from the bag and says so.</summary>
        private IceBait TakeBait(int hole)
        {
            if (_bag == null)
            {
                return IceBait.None;
            }

            LoadKulCall();
            bool great = OverworldLayouts.IceFishingHoles[hole].Great;
            int silvers = _bag.GetTotalCount(IceBaitRules.SilverItemId);
            bool open = _kulCall.Open(UnixNow(), RegionBossDirector.IsAway(RegionBossRules.OldKulId));
            if (great && silvers > 0 && !_saidBaitRule)
            {
                _saidBaitRule = true;
                Say(IceBaitTexts.Rule);
            }

            IceBait bait = IceBaitRules.Choose(great, open, _bag.GetTotalCount(IceBaitRules.GrubItemId), silvers);
            if (bait == IceBait.None || !_bag.RemoveById(IceBaitRules.ItemOf(bait), 1))
            {
                return IceBait.None;
            }

            SayBait(bait == IceBait.Silver ? IceBaitTexts.SilverOn : IceBaitTexts.GrubOn);
            return bait;
        }

        /// <summary>A fish landed: on the silver lure at the Great Hole, one tug toward calling the Kul.</summary>
        private void BaitLanded(IceFishingSession session)
        {
            if (session.Bait != IceBait.Silver || !session.GreatHole)
            {
                return;
            }

            LoadKulCall();
            long now = UnixNow();
            KulLureResult result = _kulCall.Tug(now, RegionBossDirector.IsAway(RegionBossRules.OldKulId));
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (result)
            {
                case KulLureResult.KulHere:
                    SayBait(IceBaitTexts.KulHere);
                    break;
                case KulLureResult.Recharging:
                    SayBaitText(IceBaitTexts.Format(localization, IceBaitTexts.Recharging, Math.Max(1L, (_kulCall.RechargeLeft(now) + 59L) / 60L)));
                    break;
                case KulLureResult.Counted:
                    SayBaitText(IceBaitTexts.Format(localization, IceBaitTexts.Tug, _kulCall.TugsAt(now), KulLureCall.TugsToCall));
                    break;
                case KulLureResult.Called:
                    RegionBossDirector.CallBack(RegionBossRules.OldKulId);
                    if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
                    {
                        progress.SetTimeMark(KulCallMarkKey, now);
                    }

                    SayBait(IceBaitTexts.Called);
                    string line = IceBaitTexts.CalledLineFor(ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null);
                    if (line != null)
                    {
                        SayBait(line);
                    }

                    break;
            }
        }

        private void SayBait(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayBaitText(IceBaitTexts.Get(localization, key));
        }

        private void SayBaitText(string text) => SayText(text);
    }
}
