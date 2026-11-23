using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The cloud of marsh lights the freed Pescador de Luzes lets go (33, <see cref="FisherWispCloudRules"/>): when he falls
    /// (<see cref="EnemyDiedEvent"/>) the moment goes into the save (a time mark, written only then); for 120 s a ring of bright wisps turns over
    /// his ring and, inside it, the hero's mana comes back; for 10 min the lilac night mist of Tarchon hides nothing (the Neighbours partial reads
    /// <see cref="FisherWispsClearNight"/>). No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const int FisherCloudKey = 730;
        private const int FisherWisps = 6;
        private const float FisherViewRange = 140f;
        private const float FisherWispOrbit = 6f;
        private const float FisherTickSeconds = 0.5f;

        private static readonly Color FisherWispColor = new Color(0.8f, 0.95f, 1f, 1f);

        private readonly LineRenderer[] _fisherWisps = new LineRenderer[FisherWisps];
        private long _fisherFreedAt = long.MinValue;
        private float _fisherTick;
        private bool _fisherCloudSaid;
        private bool _fisherNightSaid;
        private bool _fisherShown;

        private void HookFisherWisps() => EventBus.Subscribe<EnemyDiedEvent>(OnFisherDied);

        private void UnhookFisherWisps() => EventBus.Unsubscribe<EnemyDiedEvent>(OnFisherDied);

        private static long UnixSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private long FisherFreedAt()
        {
            if (_fisherFreedAt == long.MinValue)
            {
                WorldMapProgress progress = Progress();
                _fisherFreedAt = progress != null ? progress.GetTimeMark(FisherWispCloudRules.FreedKey, FisherWispCloudRules.NeverFreed) : FisherWispCloudRules.NeverFreed;
            }

            return _fisherFreedAt;
        }

        /// <summary>Whether the lights the Pescador let go still clear Tarchon's night mist.</summary>
        private bool FisherWispsClearNight() => FisherWispCloudRules.NightClear(FisherFreedAt(), UnixSeconds());

        private void OnFisherDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null)
            {
                return;
            }

            EnemyController enemy = evt.Enemy.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null || enemy.Definition.name != RegionBossRules.LightFisherId)
            {
                return;
            }

            long now = Math.Max(1L, UnixSeconds());
            _fisherFreedAt = now;
            _fisherCloudSaid = false;
            _fisherNightSaid = false;
            Progress()?.SetTimeMark(FisherWispCloudRules.FreedKey, now);
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(FisherWispCloudTexts.Format(localization, FisherWispCloudTexts.Freed, FisherWispCloudRules.CloudSeconds, FisherWispCloudRules.NightClearSeconds / 60L));
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f),
                FisherWispCloudTexts.Get(localization, FisherWispCloudTexts.LineFor(MapRequirementHeroState.Capture(null).Hero)));
        }

        private void TickFisherWisps(Vector2 local)
        {
            long now = UnixSeconds();
            long freedAt = FisherFreedAt();
            Vector2 arena = OverworldLayouts.PescadorDeLuzesArena;
            bool cloud = FisherWispCloudRules.CloudOn(freedAt, now);
            if (!cloud || (local - arena).sqrMagnitude > FisherViewRange * FisherViewRange)
            {
                HideFisherWisps();
                _fisherTick = 0f;
            }
            else
            {
                ShowFisherWisps(arena);
                TickFisherMana(local, arena);
            }

            bool clear = FisherWispCloudRules.NightClear(freedAt, now);
            if (clear && IsNight() && OverworldLayouts.TarchonNightBounds.Contains(local) && !_fisherNightSaid)
            {
                _fisherNightSaid = true;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(FisherWispCloudTexts.Get(localization, FisherWispCloudTexts.NightClear));
            }
        }

        private void TickFisherMana(Vector2 local, Vector2 arena)
        {
            if (!_stats.IsAlive || !FisherWispCloudRules.InCloud(arena, local))
            {
                _fisherTick = 0f;
                return;
            }

            _fisherTick += Time.deltaTime;
            if (_fisherTick < FisherTickSeconds)
            {
                return;
            }

            float seconds = _fisherTick;
            _fisherTick = 0f;
            float mana = FisherWispCloudRules.ManaOver(_stats.CurrentMana, _stats.MaxMana, seconds);
            if (mana <= 0f)
            {
                return;
            }

            _stats.RestoreMana(mana);
            if (!_fisherCloudSaid)
            {
                _fisherCloudSaid = true;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(FisherWispCloudTexts.Get(localization, FisherWispCloudTexts.InCloud));
            }
        }

        private void ShowFisherWisps(Vector2 arena)
        {
            _fisherShown = true;
            float spin = Time.time * 0.6f;
            for (int i = 0; i < FisherWisps; i++)
            {
                float angle = spin + i * (Mathf.PI * 2f / FisherWisps);
                float orbit = FisherWispOrbit + Mathf.Sin(Time.time * 1.3f + i) * 2f;
                Vector2 world = _deltaOrigin + arena + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbit;
                _fisherWisps[i] = Pillar(_fisherWisps[i], "Dt_FisherWisp", world, 0.35f, 2.5f + (i % 3), FisherWispColor);
            }

            DrawRing(FisherCloudKey, _deltaOrigin, arena, FisherWispCloudRules.CloudRadius, FisherWispColor, true, true);
        }

        private void HideFisherWisps()
        {
            if (!_fisherShown)
            {
                return;
            }

            _fisherShown = false;
            Off(_fisherWisps);
            HideRing(FisherCloudKey);
        }
    }
}
