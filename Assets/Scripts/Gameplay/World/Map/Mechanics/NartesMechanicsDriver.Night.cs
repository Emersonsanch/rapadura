using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The second 2026-11-23 round on the Estepe dos Nartes driver: Amirani's chain at night (52, <see cref="ChainNightGlowRules"/>: the best
    /// ring ever is a save mark written when a new one is reached; at night the links up to it glow and the gusts push at half there, above it the
    /// snow pushes harder) and the good dream once O Pesadelo da Tábua is beaten (49, <see cref="GoodDreamRules"/>, read from the registry of the
    /// defeated). The cousins' lines of the first time are one mark each, set on the event.
    /// </summary>
    public sealed partial class NartesMechanicsDriver
    {
        private int _chainBest = int.MinValue;
        private bool _saidSnow;

        private static bool ChainNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        /// <summary>The best ring ever (−1 never climbed), read once from the save.</summary>
        private int ChainBest()
        {
            if (_chainBest == int.MinValue)
            {
                _chainBest = ServiceLocator.TryGet(out WorldMapProgress progress)
                    ? ChainNightGlowRules.BestFromMark(progress.GetTimeMark(ChainNightGlowRules.BestRingKey))
                    : -1;
            }

            return _chainBest;
        }

        /// <summary>After <c>Reach</c>: a ring higher than any before is written to the save (the only write, on that event).</summary>
        private void NoteChainBest()
        {
            if (_climb == null || !ChainNightGlowRules.IsNewBest(ChainBest(), _climb.BestAnchor))
            {
                return;
            }

            _chainBest = _climb.BestAnchor;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(ChainNightGlowRules.BestRingKey, ChainNightGlowRules.MarkOf(_chainBest));
            }
        }

        private float ChainPushFactor() => _climb == null ? 1f : ChainNightGlowRules.PushFactor(ChainNight(), _climb.SafeAnchor, ChainBest());

        private bool ChainRingLit(int ring) => ChainNightGlowRules.Lit(ring, ChainBest(), ChainNight());

        /// <summary>On stepping onto the links at night: how far the chain glows; the first night on a lit stretch brings the cousin's line.</summary>
        private void SayChainNight()
        {
            _saidSnow = false;
            if (!ChainNight())
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int best = ChainBest();
            if (best <= 0)
            {
                SayText(PeaksSteppeRound2Texts.Get(localization, PeaksSteppeRound2Texts.ChainNightDark));
                return;
            }

            SayText(PeaksSteppeRound2Texts.Format(localization, PeaksSteppeRound2Texts.ChainNightGlow, best));
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(ChainNightGlowRules.FirstGlowKey) <= 0L)
            {
                progress.SetTimeMark(ChainNightGlowRules.FirstGlowKey, 1L);
                SayText(PeaksSteppeRound2Texts.Get(localization, PeaksSteppeRound2Texts.LineFor(PeaksSteppeRound2Texts.ChainLinePrefix, MapRequirementHeroState.Capture(null).Hero)));
            }
        }

        /// <summary>With a gust warned at night above the lit rings: the strong snow, once per climb.</summary>
        private void SayChainSnow()
        {
            if (_saidSnow || _climb == null || !ChainNight() || ChainNightGlowRules.OnLitStretch(_climb.SafeAnchor, ChainBest()))
            {
                return;
            }

            _saidSnow = true;
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(PeaksSteppeRound2Texts.Get(localization, PeaksSteppeRound2Texts.ChainSnow));
        }

        private static bool DreamIsGood() =>
            ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(GoodDreamRules.NightmareEnemyId);
    }
}
