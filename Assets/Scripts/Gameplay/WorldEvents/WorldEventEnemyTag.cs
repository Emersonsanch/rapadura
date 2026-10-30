using System.Collections;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Night;
using UnityEngine;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>What a scheduled-event enemy is.</summary>
    public enum WorldEventEnemyRole
    {
        WorldBoss = 0,
        Bandit = 1,
        Captain = 2,
        /// <summary>A common the world boss called (no prop, plain stats).</summary>
        Minion = 3,
    }

    /// <summary>
    /// Dresses one spawned scheduled-event enemy, a frame after spawn like <c>NightVariantTag</c> (EnemyController.Start
    /// re-applies the definition's health first): stats through the event rooms' <see cref="EventRoomEnemyScaler"/>
    /// (health, damage buff, kill-XP scaler, size), then the code-built prop (<see cref="WorldEventRegalia"/>: crown for the
    /// boss, cangaceiro hat for the bando), the night recolour + glowing eyes of a recoloured night form
    /// (<see cref="NightVariantTag.TintModel"/> / <see cref="NightVariantTag.AddEyes"/>) and the floating name.
    /// The enemy keeps its own model, gait and AI: the boss is a giant wolf/scorpion/golem/bat that walks and strikes
    /// with the systems it already has.
    /// </summary>
    public class WorldEventEnemyTag : MonoBehaviour
    {
        private bool _applied;

        public WorldEventEnemyRole Role { get; private set; }
        public bool NightRecolour { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsApplied => _applied;

        public static WorldEventEnemyTag Attach(GameObject enemy, WorldEventEnemyRole role, EventRoomEnemyScale scale, bool nightRecolour, string displayName)
        {
            if (enemy == null)
            {
                return null;
            }

            EventRoomEnemyScaler.Attach(enemy, scale);
            WorldEventEnemyTag tag = enemy.GetComponent<WorldEventEnemyTag>() ?? enemy.AddComponent<WorldEventEnemyTag>();
            tag.Role = role;
            tag.NightRecolour = nightRecolour;
            tag.DisplayName = displayName;
            return tag;
        }

        private IEnumerator Start()
        {
            yield return null;
            ApplyNow();
        }

        /// <summary>Applies stats and look once (idempotent). Public for tests.</summary>
        public void ApplyNow()
        {
            if (_applied)
            {
                return;
            }

            _applied = true;
            if (TryGetComponent(out EventRoomEnemyScaler scaler))
            {
                scaler.ApplyNow();
            }

            Transform visual = transform.Find(EnemyVisualSwapper.VisualName);
            GameObject model = visual != null ? visual.gameObject : gameObject;
            if (NightRecolour)
            {
                NightVariantTag.TintModel(model, NightVariantRules.VariantTint);
            }

            switch (Role)
            {
                case WorldEventEnemyRole.WorldBoss:
                    WorldEventRegalia.AddCrown(gameObject);
                    break;
                case WorldEventEnemyRole.Bandit:
                    WorldEventRegalia.AddHat(gameObject);
                    break;
                case WorldEventEnemyRole.Captain:
                    WorldEventRegalia.AddHat(gameObject, 1.9f);
                    break;
            }

            if (NightRecolour)
            {
                NightVariantTag.AddEyes(model.transform, transform, NightVariantRules.VariantEyeColor);
            }

            if (Application.isPlaying && !string.IsNullOrEmpty(DisplayName))
            {
                CombatFxManager.SpawnLootWarningText(transform.position + Vector3.up * (Role == WorldEventEnemyRole.WorldBoss ? 4.2f : 3f), DisplayName);
            }
        }
    }
}
