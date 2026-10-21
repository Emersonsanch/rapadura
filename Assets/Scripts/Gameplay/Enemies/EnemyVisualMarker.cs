using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Tag added by <see cref="EnemyVisualSwapper"/> to the visual it builds under an enemy root,
    /// remembering which <see cref="EnemyDefinition"/> it was built for. Lets a pooled enemy that is
    /// re-spawned with the same definition keep its current visual instead of rebuilding it.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyVisualMarker : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _source;
        [SerializeField] private EnemyVisualSource _kind;

        public EnemyDefinition Source => _source;
        public EnemyVisualSource Kind => _kind;

        public void Set(EnemyDefinition source, EnemyVisualSource kind)
        {
            _source = source;
            _kind = kind;
        }
    }
}
