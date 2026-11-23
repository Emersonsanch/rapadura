using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The old griffin's presents (48, <see cref="GriffinGiftRules"/>), on the same driver as the Ouros do Céu: once O Grifo Velho do Kurgan is
    /// defeated (<see cref="DefeatedEnemyRegistry"/>) a gold glint stands on its hoard while the playing cousin's present is still there; [E] at it
    /// gives that cousin's item and line. Taking it is the only save mark (one per cousin, on that event).
    /// </summary>
    public sealed partial class NartesMechanicsDriver
    {
        private const float GiftViewRange = 70f;

        private LineRenderer _giftBeacon;
        private bool _hintGift;

        private void TickGifts(Vector2 local)
        {
            Vector2 hoard = OverworldLayouts.GrifoVelhoArena;
            bool near = (hoard - local).sqrMagnitude <= GiftViewRange * GiftViewRange;
            if (!near)
            {
                ShowGift(false);
                _hintGift = false;
                return;
            }

            bool freed = ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(GriffinGiftRules.GriffinEnemyId);
            CharacterId? hero = _heroState.Get(_bag).Hero;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            bool taken = hero.HasValue && progress != null && progress.GetTimeMark(GriffinGiftRules.TakenKey(hero.Value)) > 0L;
            GriffinGiftCheck check = hero.HasValue ? GriffinGiftRules.Check(freed, taken) : GriffinGiftCheck.Guarded;
            ShowGift(check == GriffinGiftCheck.Take);
            if (check == GriffinGiftCheck.Guarded || !GriffinGiftRules.AtHoard(hoard, local) || _controller.Input == null)
            {
                _hintGift = false;
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            if (!_hintGift || Time.unscaledTime >= _nextHint)
            {
                _hintGift = true;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayText(GriffinGiftTexts.Get(localization, check == GriffinGiftCheck.Take ? GriffinGiftTexts.Prompt : GriffinGiftTexts.Taken));
            }

            if (check != GriffinGiftCheck.Take)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            GriffinGift gift = GriffinGiftRules.GiftFor(hero.Value);
            ItemDefinition item = ItemDatabase.GetById(gift.ItemId);
            if (item == null || _bag == null || !_bag.CanFit(item, gift.Quantity) || _bag.AddItem(item, gift.Quantity) > 0)
            {
                SayText(GriffinGiftTexts.Get(localization, GriffinGiftTexts.BagFull));
                return;
            }

            progress?.SetTimeMark(GriffinGiftRules.TakenKey(hero.Value), 1L);
            SayText(GriffinGiftTexts.Get(localization, GriffinGiftTexts.LineFor(hero.Value)));
            SayGiftLifted(string.Format(GriffinGiftTexts.Get(localization, GriffinGiftTexts.Received), item.DisplayName, gift.Quantity));
            ShowGift(false);
        }

        private void ShowGift(bool show)
        {
            if (!show)
            {
                if (_giftBeacon != null)
                {
                    _giftBeacon.enabled = false;
                }

                return;
            }

            if (_giftBeacon == null)
            {
                _giftBeacon = NewLine("Nartes_GriffinGift", 0.5f);
                _giftBeacon.startColor = _giftBeacon.endColor = GoldColor;
                Vector3 foot = Ground(OverworldLayouts.GrifoVelhoArena);
                _giftBeacon.SetPosition(0, foot);
                _giftBeacon.SetPosition(1, foot + Vector3.up * 4f);
            }

            _giftBeacon.enabled = true;
        }

        private void SayGiftLifted(string text) =>
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), text);
    }
}
