using System.Collections;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Gives a killed enemy an actual death — before this, <see cref="EnemyController"/> simply
    /// stopped ticking on <see cref="Health.IsDead"/> and the corpse stayed exactly as it was: fully
    /// visible, standing in whatever pose it died in, colliders still active and still physically
    /// blocking the player. Confirmed by killing a wolf in Play Mode and checking its collider/renderer
    /// state afterward — nothing changed. This disables the collider immediately (so a "dead" enemy
    /// stops blocking movement/being targetable) and sinks + fades the corpse out over
    /// <see cref="_sinkDuration"/> seconds before deactivating the GameObject, cheap self-contained
    /// juice with no new assets required.
    /// </summary>
    public class EnemyDeathVfx : MonoBehaviour
    {
        [SerializeField] private float _sinkDuration = 1.5f;
        [SerializeField] private float _sinkDistance = 1.2f;

        private void OnEnable()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null)
            {
                return;
            }

            foreach (Collider collider in evt.Target.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            StartCoroutine(SinkAndDeactivate(evt.Target));
        }

        private IEnumerator SinkAndDeactivate(GameObject enemy)
        {
            Transform enemyTransform = enemy.transform;
            Vector3 startPosition = enemyTransform.position;
            Vector3 endPosition = startPosition + Vector3.down * _sinkDistance;

            Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
            var propertyBlock = new MaterialPropertyBlock();

            float elapsed = 0f;
            while (elapsed < _sinkDuration)
            {
                // The enemy can be destroyed mid-sink (scene unload, pool cleanup).
                if (enemy == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _sinkDuration);
                enemyTransform.position = Vector3.Lerp(startPosition, endPosition, t);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null)
                    {
                        continue;
                    }
                    renderer.GetPropertyBlock(propertyBlock);
                    Color color = renderer.sharedMaterial != null ? renderer.sharedMaterial.color : Color.white;
                    color.a = 1f - t;
                    propertyBlock.SetColor("_BaseColor", color);
                    renderer.SetPropertyBlock(propertyBlock);
                }

                yield return null;
            }

            if (enemy != null)
            {
                enemy.SetActive(false);
            }
        }
    }
}
