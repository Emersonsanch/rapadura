using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The beaten Velho Kul lets the fish go (64, <see cref="KulFreedLake"/>) on the ice-fishing driver: when he falls (<see cref="EnemyDiedEvent"/>)
    /// the whole lake is free for 30 min (better fish, shorter rest of the holes and the "dono da pesca" second fish). Save: only the moment of the
    /// last victory (an event mark).
    /// </summary>
    public sealed partial class MarGeladoMechanicsDriver
    {
        private const string KulFreedMarkKey = "margelado.kul_freed";

        private readonly KulFreedLake _freedLake = new KulFreedLake();
        private bool _freedLakeLoaded;
        private bool _saidFreeCast;

        private void HookFreedLake() => EventBus.Subscribe<EnemyDiedEvent>(OnKulDied);

        private void UnhookFreedLake() => EventBus.Unsubscribe<EnemyDiedEvent>(OnKulDied);

        private void LoadFreedLake()
        {
            if (_freedLakeLoaded)
            {
                return;
            }

            _freedLakeLoaded = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                _freedLake.Restore(progress.GetTimeMark(KulFreedMarkKey, KulFreedLake.NeverFreed));
            }
        }

        private bool LakeFreeNow()
        {
            LoadFreedLake();
            return _freedLake.IsFree(UnixNow());
        }

        private void OnKulDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null)
            {
                return;
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            string enemyId = enemy != null && enemy.Definition != null ? enemy.Definition.name : null;
            if (enemyId != RegionBossRules.OldKulId)
            {
                return;
            }

            LoadFreedLake();
            long now = UnixNow();
            _freedLake.Free(now);
            _saidFreeCast = false;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                progress.SetTimeMark(KulFreedMarkKey, now);
            }

            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(IceBaitTexts.Format(localization, IceBaitTexts.LakeFreed, KulFreedLake.FreeSeconds / 60L));
            string line = IceBaitTexts.FreedLineFor(ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null);
            if (line != null)
            {
                SayText(IceBaitTexts.Get(localization, line));
            }
        }

        /// <summary>The first cast on a freed lake says so, with the minutes left.</summary>
        private void SayFreeCast(bool freed)
        {
            if (!freed || _saidFreeCast)
            {
                return;
            }

            _saidFreeCast = true;
            ServiceLocator.TryGet(out LocalizationManager localization);
            long minutes = System.Math.Max(1L, (_freedLake.SecondsLeft(UnixNow()) + 59L) / 60L);
            SayText(IceBaitTexts.Format(localization, IceBaitTexts.FreeCast, minutes));
        }

        /// <summary>"Dono da pesca": a second fish of the same kind into the Coleção and the bag.</summary>
        private void LandExtraFish(IceFishingSession session, ItemDefinition item, string fishName)
        {
            if (!session.ExtraFish)
            {
                return;
            }

            FishingProgress.ResolveShared().RecordCatch(session.Fish.Id, session.Grams);
            int extraLeft = item != null && _bag != null ? _bag.AddItem(item, 1) : 1;
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(IceBaitTexts.Format(localization, extraLeft == 0 ? IceBaitTexts.ExtraFish : IceBaitTexts.ExtraFishBagFull, fishName));
        }
    }
}
