using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Ecos that rise from the noise (56, <see cref="HermitageEchoRules"/>): on each Echo strike an Eco Apagado comes out of the stone on the
    /// mark and chases the hero, two at most; out of the hermitages they go back into the stone. Spawned like the Maré de Lodo's Filhos (the
    /// arena's enemy prefab, ResetForSpawn, visual swap, level scaler, <see cref="MapMonsterInfo"/>, <see cref="EnemySpawnedEvent"/>).
    /// </summary>
    public sealed partial class FarnkeshSilenceMechanicsDriver
    {
        private const string RisenAreaId = "ecos_do_barulho";

        private readonly List<GameObject> _risen = new List<GameObject>();
        private readonly List<KeyValuePair<GameObject, float>> _risenCorpses = new List<KeyValuePair<GameObject, float>>();
        private OverworldView _view;
        private GameObject _echoPrefab;
        private EnemyDefinition _echoDefinition;
        private bool _echoSubscribed;

        private void AttachEchoes()
        {
            if (!_echoSubscribed)
            {
                _echoSubscribed = true;
                EventBus.Subscribe<EnemyDiedEvent>(OnEchoDied);
            }
        }

        private void DetachEchoes()
        {
            if (_echoSubscribed)
            {
                _echoSubscribed = false;
                EventBus.Unsubscribe<EnemyDiedEvent>(OnEchoDied);
            }
        }

        private void OnEchoDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy != null && _risen.Remove(evt.Enemy))
            {
                _risenCorpses.Add(new KeyValuePair<GameObject, float>(evt.Enemy, Time.time + HermitageEchoRules.CorpseSeconds));
            }
        }

        private bool EnsureEchoSpawning()
        {
            if (_view == null || !RegionBossDirector.SpawningEnabled)
            {
                return false;
            }

            if (_echoPrefab == null)
            {
                EncounterArenaController encounter = FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
                _echoPrefab = encounter != null ? encounter.EnemyPrefab : null;
            }

            if (_echoDefinition == null)
            {
                _echoDefinition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{HermitageEchoRules.EchoEnemyId}");
            }

            return _echoPrefab != null && _echoDefinition != null;
        }

        /// <summary>The Echo struck <paramref name="mark"/> (world xz): one more Eco rises there, if fewer than two are up.</summary>
        private void RaiseEcho(Vector2 mark)
        {
            _risen.RemoveAll(echo => echo == null);
            if (!HermitageEchoRules.CanRaise(_risen.Count) || !EnsureEchoSpawning())
            {
                return;
            }

            OverworldZone zone = OverworldRules.ZoneAt(_layout, mark);
            int level = zone != null ? HermitageEchoRules.RisenLevel(zone.MinLevel, zone.MaxLevel) : HermitageEchoRules.RisenLevel(158, 168);
            float footHeight = EnemyVisualSwapper.GetFootHeight(_echoPrefab.transform) * _echoPrefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(mark.x, _view.GroundHeight(mark), mark.y), footHeight);
            GameObject instance = Instantiate(_echoPrefab, position, Quaternion.identity);
            instance.name = $"{_echoPrefab.name}_{_echoDefinition.name}_eco_do_barulho";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetForSpawn(_echoDefinition, position);
                controller.ForceAggro();
            }

            EnemyVisualSwapper.Apply(instance, _echoDefinition);
            EnemyLevelScaler.Attach(instance, level);
            instance.AddComponent<MapMonsterInfo>().Configure(level, zone?.OwnerMapId ?? OverworldLayouts.ErmidasMapId, RisenAreaId, null);
            _risen.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));

            if (_player != null)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), FarnkeshSilenceTexts.Get(localization, FarnkeshSilenceTexts.EchoRises));
            }
        }

        /// <summary>Out of the hermitages: the risen Ecos go back into the stone (the fallen keep their few seconds).</summary>
        private void DismissEchoes()
        {
            foreach (GameObject echo in _risen)
            {
                if (echo != null)
                {
                    Destroy(echo);
                }
            }

            _risen.Clear();
        }

        private void ReleaseEchoCorpses()
        {
            for (int i = _risenCorpses.Count - 1; i >= 0; i--)
            {
                if (_risenCorpses[i].Key == null || Time.time >= _risenCorpses[i].Value)
                {
                    if (_risenCorpses[i].Key != null)
                    {
                        Destroy(_risenCorpses[i].Key);
                    }

                    _risenCorpses.RemoveAt(i);
                }
            }
        }
    }
}
