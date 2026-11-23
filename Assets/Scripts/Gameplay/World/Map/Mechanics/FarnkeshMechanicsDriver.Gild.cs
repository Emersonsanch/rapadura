using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The touch of O Ouro de Midas landing on the hero (53): raised by its ability (<c>RegionBossAbilities</c>, GoldenTouch) and read by the
    /// gilding (<see cref="PactoloGildRules.MidasLayers"/>). A counter, so the reader never misses one between frames.
    /// </summary>
    public static class GoldenTouchSignal
    {
        public static int Count { get; private set; }

        public static void Landed() => Count++;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Count = 0;
    }

    /// <summary>
    /// The gilding of the Vale dos Sátiros (53, <see cref="PactoloGildRules"/>), on the same driver as the gusts: a blow of a Golden Bee (or the
    /// touch of O Ouro de Midas) on the hero, read from <see cref="DamageAppliedEvent"/>, adds layers of gold; a short-lived speed effect follows
    /// the layers, refreshed while they last (it ends by itself if the driver stops); stepping into the Pactolo washes all of it off. The layers
    /// belong to this visit; the cousin's line the first time the river washes them is kept in the save (one mark, on that event).
    /// </summary>
    public sealed partial class FarnkeshMechanicsDriver
    {
        private const string GildSource = "pactolo_gild";
        private const string GildWashedKey = "pactolo_gild_washed";
        private const float GildEffectSeconds = 1.2f;
        private const float GildEffectRefresh = 0.5f;

        private BuffController _gildBuffs;
        private bool _gildSubscribed;
        private bool _inGildRegion;
        private int _gildLayers;
        private int _gildApplied;
        private float _gildSinceChange;
        private float _nextGildRefresh;
        private int _touchesSeen;

        private void SubscribeGild()
        {
            if (_gildSubscribed)
            {
                return;
            }

            _gildSubscribed = true;
            EventBus.Subscribe<DamageAppliedEvent>(OnGildDamage);
        }

        private void UnsubscribeGild()
        {
            if (_gildSubscribed)
            {
                _gildSubscribed = false;
                EventBus.Unsubscribe<DamageAppliedEvent>(OnGildDamage);
            }

            ClearGild();
        }

        private void OnGildDamage(DamageAppliedEvent e)
        {
            // 53 · a hit taken ends the Baile do Vale (FarnkeshMechanicsDriver.Dance.cs).
            if (_dance != null && _player != null && e.Target == _player.gameObject)
            {
                _danceHit = true;
            }

            if (!_inGildRegion || _player == null || e.Target != _player.gameObject || e.Source == null)
            {
                return;
            }

            EnemyController enemy = e.Source.GetComponentInParent<EnemyController>();
            int added = enemy != null && enemy.Definition != null ? PactoloGildRules.LayersFrom(enemy.Definition.name) : 0;
            AddGild(added);
        }

        private void AddGild(int added)
        {
            if (added <= 0)
            {
                return;
            }

            int before = _gildLayers;
            _gildLayers = PactoloGildRules.Add(_gildLayers, added);
            _gildSinceChange = 0f;
            if (_gildLayers == before)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayGildText(_gildLayers >= PactoloGildRules.MaxLayers
                ? PactoloGildTexts.Get(localization, PactoloGildTexts.Heavy)
                : string.Format(PactoloGildTexts.Get(localization, PactoloGildTexts.Gilded), _gildLayers, PactoloGildRules.MaxLayers));
        }

        private void TickGild(Vector2 local)
        {
            _inGildRegion = true;
            if (GoldenTouchSignal.Count != _touchesSeen)
            {
                int touches = GoldenTouchSignal.Count - _touchesSeen;
                _touchesSeen = GoldenTouchSignal.Count;
                AddGild(PactoloGildRules.MidasLayers * Mathf.Clamp(touches, 0, PactoloGildRules.MaxLayers));
            }

            if (_gildLayers <= 0)
            {
                SetGildEffect(0);
                return;
            }

            bool inRiver = PactoloGildRules.InRiver(OverworldLayouts.PactoloRiver, local);
            int before = _gildLayers;
            _gildLayers = PactoloGildRules.Tick(_gildLayers, inRiver, ref _gildSinceChange, Time.deltaTime);
            if (_gildLayers < before)
            {
                if (_gildLayers == 0 && inRiver)
                {
                    SayGild(PactoloGildTexts.Washed);
                    if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(GildWashedKey) <= 0L)
                    {
                        progress.SetTimeMark(GildWashedKey, 1L);
                        SayGild(PactoloGildTexts.LineFor(MapRequirementHeroState.Capture(null).Hero), 0.7f);
                    }
                }
                else
                {
                    SayGild(PactoloGildTexts.Flaked);
                }
            }

            SetGildEffect(_gildLayers);
        }

        /// <summary>Out of the region (or the driver stops): the gold is gone and so is its effect.</summary>
        private void ClearGild()
        {
            _inGildRegion = false;
            _touchesSeen = GoldenTouchSignal.Count;
            _gildLayers = 0;
            _gildSinceChange = 0f;
            SetGildEffect(0);
        }

        private void SetGildEffect(int layers)
        {
            if (_gildBuffs == null && _player != null)
            {
                _gildBuffs = _player.GetComponent<BuffController>();
            }

            if (_gildBuffs == null)
            {
                return;
            }

            if (layers != _gildApplied)
            {
                _gildBuffs.RemoveEffectsBySource(GildSource);
                _gildApplied = layers;
                _nextGildRefresh = 0f;
            }

            if (layers <= 0 || Time.time < _nextGildRefresh)
            {
                return;
            }

            _nextGildRefresh = Time.time + GildEffectRefresh;
            _gildBuffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Dourado",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = PactoloGildRules.SpeedFactor(layers) - 1f,
                durationSeconds = GildEffectSeconds,
                isDebuff = true,
            }, GildSource);
        }

        private void SayGild(string key, float lift = 0f)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayGildText(PactoloGildTexts.Get(localization, key), lift);
        }

        private void SayGildText(string text, float lift = 0f)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), text);
            }
        }
    }
}
